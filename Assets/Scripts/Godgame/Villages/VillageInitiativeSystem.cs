using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Processes village initiative budgeting and project scheduling.
    /// Consumes blob data from InitiativeBandLookup.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillageInitiativeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillageInitiativeState>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<InitiativeBandLookupBlobComponent>(out var lookupComponent))
            {
                return; // No initiative band lookup available
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var lookup = lookupComponent.Lookup;

            var job = new ProcessVillageInitiativeJob
            {
                Tick = tick,
                Lookup = lookup
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ProcessVillageInitiativeJob : IJobEntity
        {
            public uint Tick;
            [ReadOnly] public BlobAssetReference<InitiativeBandLookupBlob> Lookup;

            void Execute(
                Entity entity,
                ref VillageInitiativeState initiative,
                in VillageAlignmentState alignment,
                in VillageResourceSummary resources,
                in VillageWorshipState worship)
            {
                // Get band definition
                if (initiative.InitiativeBand >= Lookup.Value.Bands.Length)
                {
                    return;
                }

                var band = Lookup.Value.Bands[initiative.InitiativeBand];

                // Update initiative based on resources and morale
                var resourceModifier = CalculateResourceModifier(resources);
                var moraleModifier = CalculateMoraleModifier(worship.AverageMorale);
                var alignmentModifier = CalculateAlignmentModifier(alignment);

                var baseInitiative = GetBaseInitiativeForBand(band);
                initiative.CurrentInitiative = math.clamp(
                    baseInitiative + resourceModifier + moraleModifier + alignmentModifier,
                    0f, 1f);

                // Check stress breakpoints
                var stressValue = initiative.StressLevel;
                if (stressValue <= band.StressBreakpoints.Panic)
                {
                    initiative.CurrentInitiative *= 0.5f; // Panic reduces initiative
                }
                else if (stressValue >= band.StressBreakpoints.Rally)
                {
                    initiative.CurrentInitiative = math.min(1f, initiative.CurrentInitiative * 1.2f); // Rally boosts
                }
                else if (stressValue >= band.StressBreakpoints.Frenzy)
                {
                    initiative.CurrentInitiative = math.min(1f, initiative.CurrentInitiative * 1.5f); // Frenzy
                }

                // Decrement project tick counter
                if (initiative.TicksUntilNextProject > 0)
                {
                    initiative.TicksUntilNextProject--;
                }
                else
                {
                    // Reset counter based on band tick budget
                    initiative.TicksUntilNextProject = band.TickBudget;
                }
            }

            private static float GetBaseInitiativeForBand(InitiativeBandBlob band)
            {
                // Map band to base initiative value
                // This could be stored in the blob or calculated
                return 0.5f; // Default
            }

            private static float CalculateResourceModifier(VillageResourceSummary resources)
            {
                // Surplus boosts initiative, deficit reduces it
                var foodRatio = resources.FoodUpkeep > 0 ? resources.FoodStored / resources.FoodUpkeep : 1f;
                var constructionRatio = resources.ConstructionUpkeep > 0
                    ? resources.ConstructionStored / resources.ConstructionUpkeep
                    : 1f;

                var avgRatio = (foodRatio + constructionRatio) * 0.5f;
                return (avgRatio - 1f) * 0.2f; // +0.2 at 200% surplus, -0.2 at 0% surplus
            }

            private static float CalculateMoraleModifier(float averageMorale)
            {
                // Morale affects initiative (0-100 scale)
                return (averageMorale - 50f) / 100f * 0.3f; // +0.3 at 100 morale, -0.15 at 0 morale
            }

            private static float CalculateAlignmentModifier(VillageAlignmentState alignment)
            {
                // Lawful = stable initiative, Chaotic = volatile
                return alignment.OrderAxis * 0.001f; // Small modifier
            }
        }
    }
}

