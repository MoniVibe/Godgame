using Godgame.Registry;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Tracks ticket ownership across frames to detect double-claims and stuck tickets.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameTicketAuditSystem : ISystem
    {
        private NativeParallelHashMap<uint, TicketInfo> _ticketOwners;
        private EntityQuery _villagerQuery;

        private struct TicketInfo
        {
            public Entity Owner;
            public uint StartTick;
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgameVillager>();
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TimeState>();
            _ticketOwners = new NativeParallelHashMap<uint, TicketInfo>(256, Allocator.Persistent);
            _villagerQuery = state.GetEntityQuery(ComponentType.ReadOnly<GodgameVillager>());
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_ticketOwners.IsCreated)
            {
                _ticketOwners.Dispose();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!_ticketOwners.IsCreated || !state.EntityManager.HasBuffer<GodgameTicketClaimRecord>(telemetryEntity))
            {
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var buffer = state.EntityManager.GetBuffer<GodgameTicketClaimRecord>(telemetryEntity);
            const uint stuckThreshold = 900; // ~15 seconds at 60Hz

            var currentOwners = new NativeParallelHashMap<uint, Entity>(_ticketOwners.Count(), Allocator.Temp);
            var villagers = _villagerQuery.ToComponentDataArray<GodgameVillager>(Allocator.Temp);
            var villagerEntities = _villagerQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < villagers.Length; i++)
            {
                var villager = villagers[i];
                var entity = villagerEntities[i];
                if (villager.ActiveTicketId == 0)
                {
                    continue;
                }

                var ticketId = villager.ActiveTicketId;
                currentOwners[ticketId] = entity;

                if (_ticketOwners.TryGetValue(ticketId, out var info))
                {
                    if (info.Owner != entity)
                    {
                        buffer.Add(new GodgameTicketClaimRecord
                        {
                            Tick = tick,
                            Event = GodgameTicketAuditEvent.DoubleClaim,
                            TicketId = ticketId,
                            AgentId = BuildVillagerId(villager),
                            DurationTicks = tick - info.StartTick,
                            Details = "previous owner replaced"
                        });

                        info.Owner = entity;
                        _ticketOwners[ticketId] = info;
                        ScenarioExitUtility.ReportInvariant("Tickets/DoubleClaim", $"Ticket {ticketId} claimed by multiple entities");
                    }
                }
                else
                {
                    _ticketOwners.TryAdd(ticketId, new TicketInfo
                    {
                        Owner = entity,
                        StartTick = tick
                    });
                }
            }

            villagers.Dispose();
            villagerEntities.Dispose();

            var records = _ticketOwners.GetKeyValueArrays(Allocator.Temp);
            for (int i = 0; i < records.Length; i++)
            {
                var ticketId = records.Keys[i];
                var info = records.Values[i];
                if (!currentOwners.ContainsKey(ticketId))
                {
                    buffer.Add(new GodgameTicketClaimRecord
                    {
                        Tick = tick,
                        Event = GodgameTicketAuditEvent.ClaimReleased,
                        TicketId = ticketId,
                        AgentId = BuildEntityId(info.Owner),
                        DurationTicks = tick - info.StartTick,
                        Details = "ticket released"
                    });

                    _ticketOwners.Remove(ticketId);
                }
                else if (tick - info.StartTick > stuckThreshold)
                {
                    buffer.Add(new GodgameTicketClaimRecord
                    {
                        Tick = tick,
                        Event = GodgameTicketAuditEvent.ClaimStuck,
                        TicketId = ticketId,
                        AgentId = BuildEntityId(info.Owner),
                        DurationTicks = tick - info.StartTick,
                        Details = "claim exceeds threshold"
                    });
                    ScenarioExitUtility.ReportInvariant("Tickets/StuckClaim", $"Ticket {ticketId} stuck for {tick - info.StartTick} ticks");
                }
            }

            currentOwners.Dispose();
            records.Dispose();
        }

        private static FixedString64Bytes BuildVillagerId(in GodgameVillager villager)
        {
            FixedString64Bytes agentId = default;
            agentId.Append("villager/");
            agentId.Append(villager.VillagerId);
            return agentId;
        }

        private static FixedString64Bytes BuildEntityId(Entity entity)
        {
            FixedString64Bytes agentId = default;
            agentId.Append("entity/");
            agentId.Append(entity.Index);
            agentId.Append('.');
            agentId.Append(entity.Version);
            return agentId;
        }
    }
}
