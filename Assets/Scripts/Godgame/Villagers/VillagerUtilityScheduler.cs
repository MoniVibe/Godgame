using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Utility-based scheduler for villager needs and job priorities.
    /// Influenced by initiative bands and personality weights.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerUtilityScheduler : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerInitiativeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Process utility calculations for villagers with initiative state
            var job = new CalculateUtilityJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct CalculateUtilityJob : IJobEntity
        {
            public float DeltaTime;

            void Execute(
                Entity entity,
                ref VillagerInitiativeState initiative,
                in VillagerPersonality personality,
                in VillagerAlignment alignment,
                in VillagerOutlook outlook)
            {
                // Update initiative based on band and personality modifiers
                var baseInitiative = GetBaseInitiative(initiative.InitiativeBand);
                var personalityModifier = GetPersonalityModifier(personality, alignment);
                var stressModifier = GetStressModifier(initiative.StressLevel);

                initiative.CurrentInitiative = math.clamp(
                    baseInitiative + personalityModifier + stressModifier,
                    0f, 1f);

                // Decrement tick counter
                if (initiative.TicksUntilNextAction > 0)
                {
                    initiative.TicksUntilNextAction--;
                }
            }

            private static float GetBaseInitiative(byte band)
            {
                // Default initiative values per band (tunable)
                return band switch
                {
                    0 => 0.3f, // Slow
                    1 => 0.5f, // Measured
                    2 => 0.7f, // Bold
                    3 => 0.85f, // Reckless
                    _ => 0.5f
                };
            }

            private static float GetPersonalityModifier(in VillagerPersonality personality, in VillagerAlignment alignment)
            {
                // Bold villagers get initiative boost
                var boldModifier = personality.BoldScore * 0.001f; // +0.1 at max boldness

                // Vengeful villagers get boost when grudge active (handled elsewhere)
                var vengefulModifier = 0f;

                // Alignment affects initiative response (lawful = stable, chaotic = volatile)
                var orderModifier = alignment.OrderAxis * 0.0005f; // Lawful = slight boost, Chaotic = slight penalty

                return boldModifier + vengefulModifier + orderModifier;
            }

            private static float GetStressModifier(sbyte stressLevel)
            {
                // Stress can spike or suppress initiative
                return stressLevel * 0.01f; // -1.0 at max negative stress, +1.0 at max positive stress
            }
        }

        /// <summary>
        /// Calculate utility for a specific need type.
        /// </summary>
        [BurstCompile]
        public static float CalculateNeedUtility(
            float needValue,
            float threshold,
            float urgencyMultiplier)
        {
            if (needValue >= threshold)
            {
                return 0f; // Need satisfied
            }

            var deficit = threshold - needValue;
            return deficit * urgencyMultiplier;
        }

        /// <summary>
        /// Calculate utility for a specific job type based on outlook and alignment.
        /// </summary>
        [BurstCompile]
        public static float CalculateJobUtility(
            in VillagerOutlook outlook,
            in VillagerAlignment alignment,
            byte jobType,
            float baseUtility)
        {
            var outlookModifier = GetOutlookModifier(outlook, jobType);
            var alignmentModifier = GetAlignmentModifier(alignment, jobType);

            return baseUtility * (1f + outlookModifier + alignmentModifier);
        }

        private static float GetOutlookModifier(in VillagerOutlook outlook, byte jobType)
        {
            // Outlook-specific job preferences
            // Warlike → combat jobs, Materialistic → gathering/crafting, etc.
            // Simplified for now - can be expanded with lookup tables
            return 0f;
        }

        private static float GetAlignmentModifier(in VillagerAlignment alignment, byte jobType)
        {
            // Alignment-specific job preferences
            // Good → healing/caretaking, Evil → combat/raiding, etc.
            return 0f;
        }

        /// <summary>
        /// Weighted decision table for autonomous villager actions.
        /// </summary>
        [BurstCompile]
        public static byte SelectAutonomousAction(
            in VillagerPersonality personality,
            in VillagerAlignment alignment,
            in VillagerOutlook outlook,
            float initiative,
            ref Random random)
        {
            // Action types (simplified enum)
            const byte ActionAdventure = 0;
            const byte ActionStartFamily = 1;
            const byte ActionOpenBusiness = 2;
            const byte ActionJoinBand = 3;
            const byte ActionMigrate = 4;
            const byte ActionRevenge = 5;

            var weights = new NativeArray<float>(6, Allocator.Temp);

            // Base weights
            weights[ActionAdventure] = 1f;
            weights[ActionStartFamily] = 1f;
            weights[ActionOpenBusiness] = 1f;
            weights[ActionJoinBand] = 1f;
            weights[ActionMigrate] = 0.5f;
            weights[ActionRevenge] = 0.5f;

            // Bold villagers favor adventure and combat
            if (personality.BoldScore > 40)
            {
                weights[ActionAdventure] *= 10f;
                weights[ActionJoinBand] *= 9f;
            }
            else if (personality.BoldScore < -40)
            {
                weights[ActionAdventure] *= 0.1f;
                weights[ActionJoinBand] *= 0.1f;
            }

            // Vengeful villagers favor revenge
            if (personality.VengefulScore < -40)
            {
                weights[ActionRevenge] *= 9f;
            }

            // Warlike outlook favors band joining
            // Materialistic outlook favors business
            // (Simplified - can expand with outlook lookup)

            // Select weighted random action
            var totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                totalWeight += weights[i];
            }

            var roll = random.NextFloat(0f, totalWeight);
            var cumulative = 0f;
            byte selectedAction = 0;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    selectedAction = (byte)i;
                    break;
                }
            }

            weights.Dispose();
            return selectedAction;
        }
    }
}

