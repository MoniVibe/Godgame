using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Plans village expansion based on alignment/outlook priorities and initiative.
    /// Consumes surplus resources and schedules expansion projects.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageInitiativeSystem))]
    public partial struct VillageExpansionPlannerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();
            state.RequireForUpdate<VillageInitiativeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<VillageAlignmentCatalogBlob>(out var catalogComponent))
            {
                return; // No alignment catalog available
            }

            var catalog = catalogComponent.Catalog;
            var job = new PlanExpansionJob
            {
                Catalog = catalog
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct PlanExpansionJob : IJobEntity
        {
            [ReadOnly] public BlobAssetReference<VillageAlignmentDefinitionsBlob> Catalog;

            void Execute(
                Entity entity,
                ref VillageState state,
                ref VillageInitiativeState initiative,
                ref VillageResourceSummary resources,
                in VillageAlignmentState alignment,
                in VillageWorshipState worship)
            {
                // Only plan when initiative triggers and surplus exists
                if (initiative.TicksUntilNextProject > 0)
                {
                    return;
                }

                var hasSurplus = resources.FoodStored > resources.FoodUpkeep * 1.2f &&
                                 resources.ConstructionStored > resources.ConstructionUpkeep * 1.1f;

                if (!hasSurplus)
                {
                    return;
                }

                // Calculate surplus priority weights based on alignment
                var weights = CalculateSurplusWeights(alignment, Catalog);

                // Select expansion action based on weights and initiative
                var action = SelectExpansionAction(weights, initiative.CurrentInitiative);

                // Execute expansion action (simplified - actual implementation would spawn bands/buildings)
                ExecuteExpansionAction(action, ref resources, ref state, initiative.CurrentInitiative);
            }

            private static SurplusPriorityWeightsBlob CalculateSurplusWeights(
                VillageAlignmentState alignment,
                BlobAssetReference<VillageAlignmentDefinitionsBlob> catalog)
            {
                // Find matching axis and use its weights
                // Simplified - assumes first axis for now
                if (catalog.Value.Axes.Length > 0)
                {
                    return catalog.Value.Axes[0].SurplusWeights;
                }

                // Default weights
                return new SurplusPriorityWeightsBlob
                {
                    Build = 0.3f,
                    Defend = 0.2f,
                    Proselytize = 0.2f,
                    Research = 0.15f,
                    Migrate = 0.15f
                };
            }

            private static ExpansionAction SelectExpansionAction(
                SurplusPriorityWeightsBlob weights,
                float initiative)
            {
                // Weighted random selection based on priorities
                var totalWeight = weights.Build + weights.Defend + weights.Proselytize +
                                 weights.Research + weights.Migrate;

                // Initiative affects selection (higher initiative = more aggressive actions)
                var adjustedBuild = weights.Build;
                var adjustedDefend = weights.Defend * (1f + initiative * 0.5f); // Defend favored at high initiative
                var adjustedProselytize = weights.Proselytize;
                var adjustedResearch = weights.Research;
                var adjustedMigrate = weights.Migrate * (1f + initiative); // Migrate favored at high initiative

                // Simplified selection - pick highest weight
                var maxWeight = math.max(math.max(math.max(adjustedBuild, adjustedDefend), math.max(adjustedProselytize, adjustedResearch)), adjustedMigrate);
                if (maxWeight == adjustedBuild) return ExpansionAction.Build;
                if (maxWeight == adjustedDefend) return ExpansionAction.Defend;
                if (maxWeight == adjustedProselytize) return ExpansionAction.Proselytize;
                if (maxWeight == adjustedResearch) return ExpansionAction.Research;
                return ExpansionAction.Migrate;
            }

            private static void ExecuteExpansionAction(
                ExpansionAction action,
                ref VillageResourceSummary resources,
                ref VillageState state,
                float initiative)
            {
                // Consume resources based on action
                var costMultiplier = 1f + initiative; // Higher initiative = more expensive actions

                switch (action)
                {
                    case ExpansionAction.Build:
                        resources.ConstructionStored -= 100f * costMultiplier;
                        break;
                    case ExpansionAction.Defend:
                        resources.ConstructionStored -= 50f * costMultiplier;
                        break;
                    case ExpansionAction.Proselytize:
                        resources.FoodStored -= 30f * costMultiplier;
                        break;
                    case ExpansionAction.Research:
                        resources.SpecialtyStored -= 20f * costMultiplier;
                        break;
                    case ExpansionAction.Migrate:
                        resources.FoodStored -= 50f * costMultiplier;
                        resources.ConstructionStored -= 30f * costMultiplier;
                        break;
                }

                // Update village state based on action
                if (state.CurrentState == VillageStateType.Nascent && initiative > 0.6f)
                {
                    state.CurrentState = VillageStateType.Established;
                }
                else if (state.CurrentState == VillageStateType.Established && initiative > 0.75f)
                {
                    state.CurrentState = VillageStateType.Ascendant;
                }
            }

            private enum ExpansionAction : byte
            {
                Build = 0,
                Defend = 1,
                Proselytize = 2,
                Research = 3,
                Migrate = 4
            }
        }
    }
}

