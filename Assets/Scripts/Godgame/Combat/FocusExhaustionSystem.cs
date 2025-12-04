using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Godgame.AI; // For VillagerMood (affected by exhaustion)

namespace Godgame.Combat
{
    /// <summary>
    /// Tracks focus exhaustion and handles breakdown/coma states.
    /// 
    /// Exhaustion accumulates when focus is below threshold.
    /// High exhaustion causes:
    /// - Breakdown risk (random chance of temporary incapacitation)
    /// - Reduced ability effectiveness
    /// At maximum exhaustion with zero focus = coma state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FocusAbilitySystem))]
    public partial struct FocusExhaustionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<FocusConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = timeState.DeltaTime;
            var config = SystemAPI.GetSingleton<FocusConfig>();
            var currentTick = timeState.Tick;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Update exhaustion levels
            new UpdateExhaustionJob
            {
                DeltaTime = deltaTime,
                Config = config,
                CurrentTick = currentTick,
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();

            // Check for coma recovery
            new CheckComaRecoveryJob
            {
                Config = config,
                CurrentTick = currentTick,
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();

            // Process breakdown events (random chance when at high exhaustion)
            new ProcessBreakdownRiskJob
            {
                CurrentTick = currentTick,
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel();

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public partial struct UpdateExhaustionJob : IJobEntity
        {
            public float DeltaTime;
            public FocusConfig Config;
            public uint CurrentTick;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                Entity entity,
                [ChunkIndexInQuery] int sortKey,
                ref EntityFocus focus)
            {
                // Skip if in coma (handled separately)
                if (focus.IsInComa) return;

                float focusPercent = focus.FocusPercent;

                // Update exhaustion based on focus level
                if (focusPercent < Config.ExhaustionThreshold)
                {
                    // Accumulate exhaustion when focus is low
                    // Rate increases as focus approaches 0
                    float severity = 1f - (focusPercent / Config.ExhaustionThreshold);
                    float accumulationRate = Config.ExhaustionAccumulationRate * severity;

                    int newExhaustion = focus.ExhaustionLevel + (int)(accumulationRate * DeltaTime);
                    focus.ExhaustionLevel = (byte)math.min(newExhaustion, 100);
                }
                else
                {
                    // Recover exhaustion when focus is healthy
                    float recovery = Config.ExhaustionRecoveryRate * DeltaTime;
                    int newExhaustion = focus.ExhaustionLevel - (int)recovery;
                    focus.ExhaustionLevel = (byte)math.max(newExhaustion, 0);
                }

                // Check for breakdown risk threshold
                bool wasAtRisk = focus.IsBreakdownRisk;
                focus.IsBreakdownRisk = focus.ExhaustionLevel >= Config.BreakdownRiskThreshold;

                // Add/remove breakdown risk tag
                if (focus.IsBreakdownRisk && !wasAtRisk)
                {
                    ECB.AddComponent(sortKey, entity, new FocusBreakdownRiskTag
                    {
                        BreakdownChance = (byte)(focus.ExhaustionLevel - Config.BreakdownRiskThreshold),
                        RiskStartTick = CurrentTick
                    });
                }
                else if (!focus.IsBreakdownRisk && wasAtRisk)
                {
                    ECB.RemoveComponent<FocusBreakdownRiskTag>(sortKey, entity);
                }

                // Check for coma trigger: max exhaustion + zero focus
                if (focus.ExhaustionLevel >= 100 && focus.CurrentFocus <= 0f)
                {
                    focus.IsInComa = true;
                    ECB.AddComponent(sortKey, entity, new FocusComaTag
                    {
                        ComaStartTick = CurrentTick,
                        MinRecoveryTicks = Config.MinComaDuration
                    });
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(FocusComaTag))]
        public partial struct CheckComaRecoveryJob : IJobEntity
        {
            public FocusConfig Config;
            public uint CurrentTick;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                Entity entity,
                [ChunkIndexInQuery] int sortKey,
                ref EntityFocus focus,
                in FocusComaTag comaTag)
            {
                if (!focus.IsInComa) return;

                // Check minimum coma duration
                uint ticksInComa = CurrentTick - comaTag.ComaStartTick;
                if (ticksInComa < comaTag.MinRecoveryTicks) return;

                // Slow focus recovery while in coma
                focus.CurrentFocus += focus.BaseRegenRate * 0.1f; // 10% of normal regen

                // Recovery condition: focus above 50% and exhaustion below 50
                if (focus.FocusPercent >= 0.5f && focus.ExhaustionLevel < 50)
                {
                    focus.IsInComa = false;
                    focus.ExhaustionLevel = 50; // Still exhausted after recovery
                    ECB.RemoveComponent<FocusComaTag>(sortKey, entity);
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(FocusBreakdownRiskTag))]
        public partial struct ProcessBreakdownRiskJob : IJobEntity
        {
            public uint CurrentTick;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(
                Entity entity,
                [ChunkIndexInQuery] int sortKey,
                ref EntityFocus focus,
                in FocusBreakdownRiskTag riskTag)
            {
                // Roll for breakdown every ~60 ticks (2 seconds at 30 TPS)
                if ((CurrentTick - riskTag.RiskStartTick) % 60 != 0) return;

                // Simple deterministic "random" based on entity index and tick
                uint hash = (uint)(entity.Index * 31 + CurrentTick);
                uint roll = hash % 100;

                if (roll < riskTag.BreakdownChance)
                {
                    // Breakdown occurs! Apply penalty
                    // Drain remaining focus and spike exhaustion
                    focus.CurrentFocus *= 0.5f;
                    focus.ExhaustionLevel = (byte)math.min(focus.ExhaustionLevel + 20, 100);

                    // If this pushes to coma, let the main job handle it next frame
                }
            }
        }
    }

    /// <summary>
    /// Applies exhaustion effects to other systems (mood, ability effectiveness).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FocusExhaustionSystem))]
    public partial struct FocusExhaustionEffectsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var moodModifierLookup = SystemAPI.GetBufferLookup<AI.MoodModifier>(false);

            // Apply exhaustion mood penalty
            new ApplyExhaustionMoodPenaltyJob
            {
                DeltaTime = deltaTime,
                MoodModifierLookup = moodModifierLookup
            }.Schedule();
        }

        [BurstCompile]
        public partial struct ApplyExhaustionMoodPenaltyJob : IJobEntity
        {
            public float DeltaTime;
            public BufferLookup<AI.MoodModifier> MoodModifierLookup;

            public void Execute(Entity entity, in EntityFocus focus)
            {
                // Only apply if exhausted
                if (focus.ExhaustionLevel < 50) return;

                // Check if entity has mood
                if (!MoodModifierLookup.TryGetBuffer(entity, out var moodModifiers)) return;

                // Check if we already have an exhaustion modifier (identified by ModifierId)
                bool hasExhaustionModifier = false;
                for (int i = 0; i < moodModifiers.Length; i++)
                {
                    if (moodModifiers[i].ModifierId == "focus.fatigue")
                    {
                        hasExhaustionModifier = true;
                        break;
                    }
                }

                // Add/update exhaustion mood penalty if not present
                if (!hasExhaustionModifier)
                {
                    float severity = (focus.ExhaustionLevel - 50) / 50f; // 0 to 1
                    moodModifiers.Add(new AI.MoodModifier
                    {
                        ModifierId = "focus.fatigue",
                        Category = AI.MoodModifierCategory.Health,
                        Magnitude = (sbyte)math.clamp((int)(-severity * 20f), -100, 0),
                        RemainingTicks = 30, // short-lived; reapplied periodically
                        AppliedTick = focus.LastUpdateTick,
                        DecayHalfLife = 0
                    });
                }
            }
        }
    }

    /// <summary>
    /// Static helpers for querying exhaustion effects.
    /// </summary>
    public static class FocusExhaustionHelpers
    {
        /// <summary>
        /// Gets the effectiveness multiplier based on exhaustion level.
        /// 100% at 0 exhaustion, down to 70% at max exhaustion.
        /// </summary>
        public static float GetEffectivenessMultiplier(byte exhaustionLevel)
        {
            if (exhaustionLevel < 50) return 1f;

            // Linear decrease from 100% at 50 exhaustion to 70% at 100 exhaustion
            float penalty = (exhaustionLevel - 50) / 50f * 0.3f;
            return 1f - penalty;
        }

        /// <summary>
        /// Gets the cost multiplier for abilities based on exhaustion.
        /// Abilities cost more when exhausted.
        /// </summary>
        public static float GetCostMultiplier(byte exhaustionLevel)
        {
            if (exhaustionLevel < 50) return 1f;

            // Linear increase from 100% at 50 exhaustion to 150% at 100 exhaustion
            float penalty = (exhaustionLevel - 50) / 50f * 0.5f;
            return 1f + penalty;
        }

        /// <summary>
        /// Returns true if entity is too exhausted to use focus abilities.
        /// </summary>
        public static bool IsTooExhaustedForAbilities(in EntityFocus focus)
        {
            return focus.IsInComa || (focus.ExhaustionLevel >= 95 && focus.CurrentFocus < 10f);
        }

        /// <summary>
        /// Gets a descriptive state for UI/debug display.
        /// </summary>
        public static string GetExhaustionState(in EntityFocus focus)
        {
            if (focus.IsInComa) return "COMA";
            if (focus.ExhaustionLevel >= 90) return "Critical";
            if (focus.IsBreakdownRisk) return "Breakdown Risk";
            if (focus.ExhaustionLevel >= 50) return "Exhausted";
            if (focus.ExhaustionLevel >= 25) return "Tired";
            return "Fresh";
        }
    }
}

