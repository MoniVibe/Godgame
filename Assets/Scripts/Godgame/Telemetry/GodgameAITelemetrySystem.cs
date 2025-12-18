using Godgame.Telemetry;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Scenarios;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Systems
{
    /// <summary>
    /// Emits BehaviorProfileAssigned and GoalChanged telemetry for villagers so headless CI can verify doctrine coverage.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct GodgameAITelemetrySystem : ISystem
    {
        private static readonly FixedString64Bytes SourceId = new FixedString64Bytes("Godgame.Villagers");
        private static readonly FixedString64Bytes EventProfileAssigned = new FixedString64Bytes("BehaviorProfileAssigned");
        private static readonly FixedString64Bytes EventGoalChanged = new FixedString64Bytes("GoalChanged");
        private EntityQuery _missingTelemetryQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerAIState>();
            state.RequireForUpdate<AIRole>();
            state.RequireForUpdate<AIDoctrine>();
            state.RequireForUpdate<AIBehaviorProfile>();
            state.RequireForUpdate<TelemetryStream>();
            state.RequireForUpdate<TelemetryStreamSingleton>();
            state.RequireForUpdate<ScenarioTick>();

            _missingTelemetryQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VillagerAIState>(),
                    ComponentType.ReadOnly<AIRole>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<GodgameAITelemetryState>()
                }
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!TryGetTelemetryEventBuffer(ref state, out var eventBuffer))
            {
                return;
            }

            var tick = SystemAPI.GetSingleton<ScenarioTick>().Value;

            if (!_missingTelemetryQuery.IsEmptyIgnoreFilter)
            {
                state.EntityManager.AddComponent<GodgameAITelemetryState>(_missingTelemetryQuery);
            }

            foreach (var (aiState, role, doctrine, profile, telemetry, entity) in SystemAPI
                         .Query<RefRO<VillagerAIState>, RefRO<AIRole>, RefRO<AIDoctrine>, RefRO<AIBehaviorProfile>, RefRW<GodgameAITelemetryState>>()
                         .WithEntityAccess())
            {
                if (telemetry.ValueRO.ProfileLogged == 0 ||
                    telemetry.ValueRO.LastRoleId != role.ValueRO.RoleId ||
                    telemetry.ValueRO.LastDoctrineId != doctrine.ValueRO.DoctrineId ||
                    telemetry.ValueRO.LastProfileHash != profile.ValueRO.ProfileHash)
                {
                    var payload = BuildProfilePayload(entity, role.ValueRO.RoleId, doctrine.ValueRO.DoctrineId, profile.ValueRO);
                    eventBuffer.AddEvent(EventProfileAssigned, tick, SourceId, payload);

                    telemetry.ValueRW.LastRoleId = role.ValueRO.RoleId;
                    telemetry.ValueRW.LastDoctrineId = doctrine.ValueRO.DoctrineId;
                    telemetry.ValueRW.LastProfileHash = profile.ValueRO.ProfileHash;
                    telemetry.ValueRW.ProfileLogged = 1;
                }

                if (aiState.ValueRO.CurrentGoal != telemetry.ValueRO.LastGoal)
                {
                    var payload = BuildGoalPayload(entity, role.ValueRO.RoleId, doctrine.ValueRO.DoctrineId, aiState.ValueRO, telemetry.ValueRO.LastGoal);
                    eventBuffer.AddEvent(EventGoalChanged, tick, SourceId, payload);
                    telemetry.ValueRW.LastGoal = aiState.ValueRO.CurrentGoal;
                    telemetry.ValueRW.LastGoalTick = tick;
                }
            }
        }

        private static FixedString128Bytes BuildProfilePayload(Entity entity, ushort roleId, ushort doctrineId, in AIBehaviorProfile profile)
        {
            var writer = new GodgameTelemetryJsonWriter();
            writer.AddEntity("entity", entity);
            writer.AddInt("roleId", roleId);
            writer.AddInt("doctrineId", doctrineId);
            writer.AddInt("profileId", profile.ProfileId);
            writer.AddUInt("profileHash", profile.ProfileHash);
            writer.AddInt("profileSource", profile.SourceId);
            return writer.Build();
        }

        private static FixedString128Bytes BuildGoalPayload(Entity entity, ushort roleId, ushort doctrineId, in VillagerAIState aiState, VillagerAIState.Goal previousGoal)
        {
            var writer = new GodgameTelemetryJsonWriter();
            writer.AddEntity("entity", entity);
            writer.AddInt("roleId", roleId);
            writer.AddInt("doctrineId", doctrineId);
            writer.AddString("oldGoal", previousGoal.ToString());
            writer.AddString("newGoal", aiState.CurrentGoal.ToString());
            writer.AddString("state", aiState.CurrentState.ToString());
            writer.AddEntity("target", aiState.TargetEntity);
            writer.AddUInt("stateStartTick", 0); // StateStartTick not available in VillagerAIState
            writer.AddBool("hasCommand", aiState.TargetEntity != Entity.Null);
            return writer.Build();
        }

        private bool TryGetTelemetryEventBuffer(ref SystemState state, out DynamicBuffer<TelemetryEvent> buffer)
        {
            buffer = default;
            if (!SystemAPI.TryGetSingleton<TelemetryStreamSingleton>(out var telemetryRef))
            {
                return false;
            }

            if (telemetryRef.Entity == Entity.Null || !state.EntityManager.HasBuffer<TelemetryEvent>(telemetryRef.Entity))
            {
                return false;
            }

            buffer = state.EntityManager.GetBuffer<TelemetryEvent>(telemetryRef.Entity);
            return true;
        }
    }
}
