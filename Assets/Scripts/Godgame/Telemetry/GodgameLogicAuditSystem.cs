using Godgame.Registry;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Telemetry;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Lightweight invariant checks that emit logic audit records.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct GodgameLogicAuditSystem : ISystem
    {
        private ComponentLookup<Godgame.Villagers.VillagerNeeds> _needsLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<Godgame.Villagers.VillagerAIState>();
            state.RequireForUpdate<TimeState>();
            _needsLookup = state.GetComponentLookup<Godgame.Villagers.VillagerNeeds>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!state.EntityManager.HasBuffer<GodgameLogicAuditRecord>(telemetryEntity))
            {
                return;
            }

            var buffer = state.EntityManager.GetBuffer<GodgameLogicAuditRecord>(telemetryEntity);
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            _needsLookup.Update(ref state);

            var invalidRefs = 0;
            foreach (var ai in SystemAPI.Query<RefRO<Godgame.Villagers.VillagerAIState>>())
            {
                if (ai.ValueRO.TargetEntity != Entity.Null && !state.EntityManager.Exists(ai.ValueRO.TargetEntity))
                {
                    invalidRefs++;
                }
            }

            if (invalidRefs > 0)
            {
                buffer.Add(new GodgameLogicAuditRecord
                {
                    Tick = tick,
                    Kind = GodgameLogicAuditKind.InvalidEntityReference,
                    Count = invalidRefs,
                    Details = "VillagerAIState.Target"
                });
                ScenarioExitUtility.ReportInvariant("Logic/InvalidEntityReference", $"VillagerAIState.Target invalid count={invalidRefs}");
            }

            var nanTransforms = 0;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>())
            {
                var pos = transform.ValueRO.Position;
                if (math.any(math.isnan(pos)))
                {
                    nanTransforms++;
                }
            }

            if (nanTransforms > 0)
            {
                buffer.Add(new GodgameLogicAuditRecord
                {
                    Tick = tick,
                    Kind = GodgameLogicAuditKind.NaNTransform,
                    Count = nanTransforms,
                    Details = "LocalTransform.Position"
                });
                ScenarioExitUtility.ReportInvariant("Logic/NaNTransform", $"NaN transforms detected: {nanTransforms}");
            }

            var missingNeeds = 0;
            foreach (var _ in SystemAPI.Query<RefRO<VillagerJobState>>().WithNone<Godgame.Villagers.VillagerNeeds>())
            {
                missingNeeds++;
            }

            if (missingNeeds > 0)
            {
                buffer.Add(new GodgameLogicAuditRecord
                {
                    Tick = tick,
                    Kind = GodgameLogicAuditKind.MissingComponent,
                    Count = missingNeeds,
                    Details = "VillagerJobState missing VillagerNeeds"
                });
                ScenarioExitUtility.ReportInvariant("Logic/MissingVillagerNeeds", $"VillagerNeeds missing: {missingNeeds}");
            }

            var invalidStates = 0;
            foreach (var job in SystemAPI.Query<RefRO<VillagerJobState>>())
            {
                if ((int)job.ValueRO.Phase > (int)JobPhase.Deliver)
                {
                    invalidStates++;
                }
            }

            if (invalidStates > 0)
            {
                buffer.Add(new GodgameLogicAuditRecord
                {
                    Tick = tick,
                    Kind = GodgameLogicAuditKind.InvalidState,
                    Count = invalidStates,
                    Details = "VillagerJobState.Phase"
                });
                ScenarioExitUtility.ReportInvariant("Logic/InvalidJobPhase", $"Villager job phase invalid count={invalidStates}");
            }
        }
    }
}
