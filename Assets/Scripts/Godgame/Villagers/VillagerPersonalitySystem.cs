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
                in DynamicBuffer<VillagerGrudge> grudges)
            {
                // Update patriotism drift
                UpdatePatriotism(ref patriotism, DeltaTime);

                // Apply grudge boost to initiative if any active grudges exist
                float totalGrudgeIntensity = 0f;
                for (int i = 0; i < grudges.Length; i++)
                {
                    var grudge = grudges[i];
                    if (!grudge.IsResolved && grudge.Intensity > 0f)
                    {
                        totalGrudgeIntensity += grudge.Intensity;
                    }
                }

                if (totalGrudgeIntensity > 0f)
                {
                    // Boost initiative based on total grudge intensity (max +0.2 at 100 intensity)
                    var grudgeBoost = math.min(0.2f, totalGrudgeIntensity * 0.002f);
                    initiative.CurrentInitiative = math.min(1f, initiative.CurrentInitiative + grudgeBoost);
                }
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
        /// Note: This creates a grudge element that should be added to the DynamicBuffer.
        /// The actual buffer addition should be done by the caller using an EntityCommandBuffer.
        /// </summary>
        [BurstCompile]
        public static void GenerateGrudge(
            in Entity targetEntity,
            GrudgeOffenseType offenseType,
            uint currentTick,
            in VillagerBehavior behavior,
            out VillagerGrudge grudge)
        {
            // Use the factory method from VillagerGrudge which handles intensity calculation
            grudge = VillagerGrudge.Create(in targetEntity, offenseType, currentTick, in behavior);
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

