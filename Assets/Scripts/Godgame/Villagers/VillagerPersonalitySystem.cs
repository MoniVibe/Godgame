using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Processes grudge generation/decay, patriotism drift, mood modifiers, and combat stance overrides.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerUtilityScheduler))]
    public partial struct VillagerPersonalitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPersonality>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var deltaTime = SystemAPI.Time.DeltaTime;

            var withGrudgeJob = new ProcessPersonalityWithGrudgeJob
            {
                Tick = tick,
                DeltaTime = deltaTime
            };
            var noGrudgeJob = new ProcessPersonalityNoGrudgeJob
            {
                DeltaTime = deltaTime
            };

            state.Dependency = withGrudgeJob.ScheduleParallel(state.Dependency);
            state.Dependency = noGrudgeJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ProcessPersonalityWithGrudgeJob : IJobEntity
        {
            public uint Tick;
            public float DeltaTime;

            void Execute(
                Entity entity,
                ref VillagerPersonality personality,
                ref VillagerInitiativeState initiative,
                ref VillagerPatriotism patriotism,
                ref VillagerAlignment alignment,
                in VillagerGrudge grudge)
            {
                // Process grudge decay
                if (grudge.Intensity > 0)
                {
                    ProcessGrudgeDecay(ref personality, grudge, DeltaTime);
                }

                // Update patriotism drift
                UpdatePatriotism(ref patriotism, DeltaTime);

                // Apply grudge boost to initiative if active
                if (grudge.Intensity > 0)
                {
                    var grudgeBoost = grudge.Intensity * 0.002f; // +0.2 at max intensity
                    initiative.CurrentInitiative = math.min(1f, initiative.CurrentInitiative + grudgeBoost);
                }
            }
            
            private static void ProcessGrudgeDecay(ref VillagerPersonality personality, VillagerGrudge grudge, float deltaTime)
            {
                // Grudge decay rate depends on vengeful score
                // More vengeful = slower decay
                var decayMultiplier = 1f - (math.abs(personality.VengefulScore) / 100f) * 0.5f;
                var actualDecayRate = grudge.DecayRate * decayMultiplier * deltaTime;

                // Decay is handled by separate grudge system - this is just for personality effects
            }

            private static void UpdatePatriotism(ref VillagerPatriotism patriotism, float deltaTime)
            {
                // Apply decay
                var decayAmount = patriotism.DecayRate * deltaTime;
                patriotism.Value = (byte)math.max(0, patriotism.Value - decayAmount);

                // Patriotism can be boosted by events (handled elsewhere)
            }
        }

        [BurstCompile]
        [WithNone(typeof(VillagerGrudge))]
        public partial struct ProcessPersonalityNoGrudgeJob : IJobEntity
        {
            public float DeltaTime;

            void Execute(
                ref VillagerPersonality personality,
                ref VillagerInitiativeState initiative,
                ref VillagerPatriotism patriotism,
                ref VillagerAlignment alignment)
            {
                // No grudge - just update patriotism
                UpdatePatriotism(ref patriotism, DeltaTime);
            }

            private static void UpdatePatriotism(ref VillagerPatriotism patriotism, float deltaTime)
            {
                var decayAmount = patriotism.DecayRate * deltaTime;
                patriotism.Value = (byte)math.max(0, patriotism.Value - decayAmount);
            }
        }

        /// <summary>
        /// Generate a grudge when villager is wronged.
        /// </summary>
        [BurstCompile]
        public static void GenerateGrudge(
            ref VillagerPersonality personality,
            float harmSeverity,
            ref Entity targetEntity,
            uint currentTick,
            out VillagerGrudge grudge)
        {
            var vengefulMagnitude = math.abs(personality.VengefulScore);
            var intensity = (byte)math.clamp(harmSeverity * (vengefulMagnitude / 100f), 0f, 100f);

            // Decay rate: more vengeful = slower decay
            var baseDecayRate = 0.3f; // per day
            var vengefulModifier = 1f - (vengefulMagnitude / 100f) * 0.7f; // 0.3x to 1.0x
            var decayRate = baseDecayRate * vengefulModifier;

            grudge = new VillagerGrudge
            {
                Intensity = intensity,
                TargetEntity = targetEntity,
                DecayRate = decayRate,
                GeneratedTick = currentTick
            };
        }

        /// <summary>
        /// Get combat stance modifiers based on personality.
        /// </summary>
        [BurstCompile]
        public static void GetCombatStanceModifiers(ref VillagerPersonality personality, ref CombatStanceModifiers modifiers)
        {
            var boldModifier = personality.BoldScore / 100f; // -1 to +1

            modifiers = new CombatStanceModifiers
            {
                EngageRangeMultiplier = 1f + (boldModifier * 0.5f), // +50% at max bold
                RetreatThreshold = boldModifier > 0.4f ? 30f : 60f, // Bold fights longer
                DodgeChanceModifier = -boldModifier * 0.1f, // Bold dodges less
                DamageOutputMultiplier = 1f + (boldModifier * 0.15f), // +15% at max bold
                MoraleAura = (sbyte)(boldModifier * 5f) // +5 at max bold, -5 at max craven
            };
        }

        public struct CombatStanceModifiers
        {
            public float EngageRangeMultiplier;
            public float RetreatThreshold;
            public float DodgeChanceModifier;
            public float DamageOutputMultiplier;
            public sbyte MoraleAura;
        }
    }
}

