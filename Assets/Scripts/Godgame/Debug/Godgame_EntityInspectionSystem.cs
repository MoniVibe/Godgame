using Godgame.Economy;
using Godgame.Input;
using Godgame.Presentation;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.AI;
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
            state.RequireForUpdate<TimeState>();
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
            var timeState = SystemAPI.GetSingleton<TimeState>();
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
                inspection.Summary = BuildInspectionSummary(ref state, selectedEntity, timeState);
                inspection.IsValid = 1;
            }
#endif
        }

#if UNITY_EDITOR
        private FixedString512Bytes BuildInspectionSummary(ref SystemState state, Entity entity, in TimeState timeState)
        {
            var em = state.EntityManager;
            var summary = new FixedString512Bytes();

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

                AppendCooldownSummary(ref summary, em, entity, timeState);

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

        private static void AppendCooldownSummary(ref FixedString512Bytes summary, EntityManager em, Entity entity, in TimeState timeState)
        {
            if (!em.HasComponent<VillagerWorkCooldown>(entity))
            {
                return;
            }

            var cooldown = em.GetComponentData<VillagerWorkCooldown>(entity);
            if (cooldown.EndTick == 0 || timeState.Tick >= cooldown.EndTick)
            {
                return;
            }

            var remainingTicks = cooldown.EndTick > timeState.Tick ? cooldown.EndTick - timeState.Tick : 0u;
            var remainingSeconds = remainingTicks * timeState.FixedDeltaTime;
            var roundedSeconds = math.max(0f, math.ceil(remainingSeconds));

            var actionLabel = "Cooling down";
            var leisureMode = cooldown.Mode;
            var leisureAction = VillagerLeisureAction.None;
            if (em.HasComponent<VillagerLeisureState>(entity))
            {
                leisureAction = em.GetComponentData<VillagerLeisureState>(entity).Action;
            }

            if (leisureAction == VillagerLeisureAction.Tidy)
            {
                actionLabel = "Taking a break: Tidy";
            }
            else if (leisureAction == VillagerLeisureAction.Observe)
            {
                actionLabel = "Taking a break: Observe";
            }
            if (leisureMode == VillagerWorkCooldownMode.None && em.HasComponent<VillagerGoalState>(entity))
            {
                var goal = em.GetComponentData<VillagerGoalState>(entity);
                leisureMode = goal.CurrentGoal == VillagerGoal.Socialize
                    ? VillagerWorkCooldownMode.Socialize
                    : goal.CurrentGoal == VillagerGoal.Idle
                        ? VillagerWorkCooldownMode.Wander
                        : VillagerWorkCooldownMode.None;
            }

            if (leisureAction == VillagerLeisureAction.None && leisureMode == VillagerWorkCooldownMode.Socialize)
            {
                actionLabel = "Taking a break: Socialize";
            }
            else if (leisureAction == VillagerLeisureAction.None && leisureMode == VillagerWorkCooldownMode.Wander)
            {
                actionLabel = "Taking a break: Wander";
            }

            summary.Append($"\n{actionLabel} ({roundedSeconds:F0}s)");

            if (leisureMode == VillagerWorkCooldownMode.Socialize && em.HasComponent<VillagerSocialFocus>(entity))
            {
                var focus = em.GetComponentData<VillagerSocialFocus>(entity);
                if (focus.Target != Entity.Null && em.Exists(focus.Target))
                {
                    var targetLabel = ResolveEntityLabel(em, focus.Target);
                    if (!string.IsNullOrEmpty(targetLabel))
                    {
                        summary.Append($" @ {targetLabel}");
                    }
                }
            }
            else if ((leisureAction == VillagerLeisureAction.Tidy || leisureAction == VillagerLeisureAction.Observe)
                     && em.HasComponent<VillagerLeisureState>(entity))
            {
                var leisure = em.GetComponentData<VillagerLeisureState>(entity);
                if (leisure.ActionTarget != Entity.Null && em.Exists(leisure.ActionTarget))
                {
                    var targetLabel = ResolveEntityLabel(em, leisure.ActionTarget);
                    if (!string.IsNullOrEmpty(targetLabel))
                    {
                        summary.Append($" @ {targetLabel}");
                    }
                }
            }
        }

        private static string ResolveEntityLabel(EntityManager em, Entity entity)
        {
            if (entity == Entity.Null)
            {
                return string.Empty;
            }

            if (em.HasComponent<Village>(entity))
            {
                var village = em.GetComponentData<Village>(entity);
                if (village.VillageName.Length > 0)
                {
                    return village.VillageName.ToString();
                }
            }

            var name = em.GetName(entity);
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            return $"Entity {entity.Index}";
        }
#endif
    }
}
