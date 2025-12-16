using Godgame.Economy;
using Godgame.Input;
using Godgame.Presentation;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Godgame.Debugging
{
    /// <summary>
    /// System that inspects selected entities and writes inspection data.
    /// Editor-only logic (not Burst-compiled) for reading sim data.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_EntityInspectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Create inspection singleton if not exists
            var query = state.GetEntityQuery(typeof(EntityInspectionSingleton));
            if (query.IsEmpty)
            {
                var inspectionEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<EntityInspectionSingleton>(inspectionEntity);
                state.EntityManager.AddComponentData(inspectionEntity, new EntityInspectionData());
                state.EntityManager.SetName(inspectionEntity, "EntityInspectionSingleton");
            }
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            // Get selected entity from input
            Entity selectedEntity = Entity.Null;
            if (SystemAPI.TryGetSingleton<SelectionInput>(out var selectionInput))
            {
                selectedEntity = selectionInput.SelectedEntity;
            }

            // Update inspection data
            foreach (var inspectionRef in SystemAPI.Query<RefRW<EntityInspectionData>>())
            {
                ref var inspection = ref inspectionRef.ValueRW;

                if (selectedEntity == Entity.Null || !state.EntityManager.Exists(selectedEntity))
                {
                    inspection.IsValid = 0;
                    inspection.InspectedEntity = Entity.Null;
                    inspection.Summary = default;
                    continue;
                }

                inspection.InspectedEntity = selectedEntity;
                inspection.Summary = BuildInspectionSummary(ref state, selectedEntity);
                inspection.IsValid = 1;
            }
#endif
        }

#if UNITY_EDITOR
        private FixedString128Bytes BuildInspectionSummary(ref SystemState state, Entity entity)
        {
            var em = state.EntityManager;
            var summary = new FixedString128Bytes();

            // Check if villager
            if (em.HasComponent<VillagerBehavior>(entity))
            {
                var behavior = em.GetComponentData<VillagerBehavior>(entity);
                var transform = em.HasComponent<LocalTransform>(entity) 
                    ? em.GetComponentData<LocalTransform>(entity) 
                    : default;

                summary.Append("Villager\n");
                summary.Append($"Pos: {transform.Position.x:F1}, {transform.Position.z:F1}\n");
                summary.Append($"Vengeful: {behavior.VengefulScore:F0}\n");
                summary.Append($"Bold: {behavior.BoldScore:F0}");

                // Check if carrying resource
                if (em.HasComponent<ExtractedResource>(entity))
                {
                    var resource = em.GetComponentData<ExtractedResource>(entity);
                    summary.Append($"\nCarrying: {resource.Type} x{resource.Quantity}");
                }

                // Check if has job
                if (em.HasComponent<Godgame.Villagers.VillagerJob>(entity))
                {
                    var job = em.GetComponentData<Godgame.Villagers.VillagerJob>(entity);
                    summary.Append($"\nJob: {job.Type}, Phase: {job.Phase}");
                }

                return summary;
            }

            // Check if village
            if (em.HasComponent<Village>(entity))
            {
                var village = em.GetComponentData<Village>(entity);
                var transform = em.HasComponent<LocalTransform>(entity)
                    ? em.GetComponentData<LocalTransform>(entity)
                    : default;

                summary.Append("Village\n");
                summary.Append($"ID: {village.VillageId}\n");
                summary.Append($"Phase: {village.Phase}\n");
                summary.Append($"Members: {village.MemberCount}\n");
                summary.Append($"Radius: {village.InfluenceRadius:F1}");

                // Check resources
                if (em.HasBuffer<VillageResource>(entity))
                {
                    var resources = em.GetBuffer<VillageResource>(entity);
                    summary.Append($"\nResources: {resources.Length} types");
                }

                // Check trend data
                if (em.HasComponent<AggregateTrendData>(entity))
                {
                    var trend = em.GetComponentData<AggregateTrendData>(entity);
                    string popTrend = trend.PopulationTrend == AggregateTrend.Improving ? "+" : 
                                     trend.PopulationTrend == AggregateTrend.Declining ? "-" : "=";
                    string wealthTrend = trend.WealthTrend == AggregateTrend.Improving ? "+" : 
                                        trend.WealthTrend == AggregateTrend.Declining ? "-" : "=";
                    string foodTrend = trend.FoodTrend == AggregateTrend.Improving ? "+" : 
                                      trend.FoodTrend == AggregateTrend.Declining ? "-" : "=";
                    summary.Append($"\nTrends: Pop{popTrend} Wealth{wealthTrend} Food{foodTrend}");
                }

                // Check history for trend details
                if (em.HasBuffer<AggregateHistory>(entity))
                {
                    var history = em.GetBuffer<AggregateHistory>(entity);
                    if (history.Length >= 2)
                    {
                        var latest = history[history.Length - 1];
                        var previous = history[history.Length - 2];
                        int popChange = latest.Population - previous.Population;
                        int foodChange = latest.Food - previous.Food;
                        summary.Append($"\nChange: Pop{popChange:+0;-0;=0} Food{foodChange:+0;-0;=0}");
                    }
                }

                return summary;
            }

            // Check if resource chunk
            if (em.HasComponent<ExtractedResource>(entity))
            {
                var resource = em.GetComponentData<ExtractedResource>(entity);
                var transform = em.HasComponent<LocalTransform>(entity)
                    ? em.GetComponentData<LocalTransform>(entity)
                    : default;

                summary.Append("Resource Chunk\n");
                summary.Append($"Type: {resource.Type}\n");
                summary.Append($"Quantity: {resource.Quantity}\n");
                summary.Append($"Purity: {resource.Purity}%");

                return summary;
            }

            // Default
            summary.Append($"Entity {entity.Index}");
            return summary;
        }
#endif
    }
}
