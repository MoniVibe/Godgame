using Godgame.Combat;
using Godgame.Telemetry;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Ensures honor ledgers have a telemetry cache so we can emit capability events.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameCapabilityTelemetryBootstrapSystem : ISystem
    {
        private EntityQuery _missingCacheQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingCacheQuery = SystemAPI.QueryBuilder()
                .WithAll<HonorLedger>()
                .WithNone<GodgameCapabilityTelemetryCache>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_missingCacheQuery.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            ecb.AddComponent(_missingCacheQuery, new GodgameCapabilityTelemetryCache());
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Emits capability grant/revoke/snapshot events for honor-based promotions.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CombatHonorSystem))]
    public partial struct GodgameCapabilityTelemetrySystem : ISystem
    {
        private const uint SnapshotIntervalTicks = 600;
        private ComponentLookup<Godgame.Villagers.VillagerId> _villagerIdLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HonorLedger>();
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TimeState>();
            _villagerIdLookup = state.GetComponentLookup<Godgame.Villagers.VillagerId>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            var entityManager = state.EntityManager;
            if (!entityManager.HasBuffer<GodgameCapabilityGrantedRecord>(telemetryEntity))
            {
                return;
            }

            var granted = entityManager.GetBuffer<GodgameCapabilityGrantedRecord>(telemetryEntity);
            var revoked = entityManager.GetBuffer<GodgameCapabilityRevokedRecord>(telemetryEntity);
            var snapshots = entityManager.GetBuffer<GodgameCapabilitySnapshotRecord>(telemetryEntity);
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            _villagerIdLookup.Update(ref state);

            foreach (var (ledger, cache, entity) in SystemAPI.Query<RefRO<HonorLedger>, RefRW<GodgameCapabilityTelemetryCache>>().WithEntityAccess())
            {
                var agentId = BuildAgentId(entity);
                var currentRank = ledger.ValueRO.CurrentRank;
                var lastRank = cache.ValueRO.LastRank;

                if (currentRank != lastRank)
                {
                    if (currentRank > lastRank)
                    {
                        granted.Add(new GodgameCapabilityGrantedRecord
                        {
                            Tick = tick,
                            AgentId = agentId,
                            CapabilityId = BuildRankCapability(currentRank),
                            Source = GodgameCapabilitySource.Experience,
                            Level = (byte)currentRank,
                            SourceId = agentId,
                            SeedHash = GodgameTelemetryStringHelpers.HashContext(agentId, tick)
                        });
                    }
                    else
                    {
                        revoked.Add(new GodgameCapabilityRevokedRecord
                        {
                            Tick = tick,
                            AgentId = agentId,
                            CapabilityId = BuildRankCapability(lastRank),
                            Source = GodgameCapabilitySource.Experience,
                            Level = (byte)lastRank,
                            Reason = "rank regression"
                        });
                    }

                    cache.ValueRW.LastRank = currentRank;
                    cache.ValueRW.LastSeedHash = GodgameTelemetryStringHelpers.HashContext(agentId, tick ^ (uint)currentRank);
                    cache.ValueRW.LastSnapshotTick = tick;
                }

                if (tick - cache.ValueRO.LastSnapshotTick >= SnapshotIntervalTicks)
                {
                    snapshots.Add(new GodgameCapabilitySnapshotRecord
                    {
                        Tick = tick,
                        AgentId = agentId,
                        BitsetHash = BuildCapabilityBitset(currentRank),
                        Level = (byte)currentRank,
                        Experience = (uint)math.max(0, ledger.ValueRO.HonorPoints),
                        Context = BuildSnapshotContext(currentRank)
                    });
                    cache.ValueRW.LastSnapshotTick = tick;
                }

                cache.ValueRW.LastHonorPoints = (uint)math.max(0, ledger.ValueRO.HonorPoints);
            }
        }

        private FixedString64Bytes BuildAgentId(Entity entity)
        {
            if (_villagerIdLookup.HasComponent(entity))
            {
                return GodgameTelemetryStringHelpers.BuildAgentId(_villagerIdLookup[entity].Value);
            }

            return GodgameTelemetryStringHelpers.BuildEntityLabel(entity);
        }

        private static FixedString64Bytes BuildRankCapability(MilitaryRank rank)
        {
            FixedString64Bytes id = default;
            id.Append("capability/rank/");
            id.Append(rank.ToString());
            return id;
        }

        private static ulong BuildCapabilityBitset(MilitaryRank rank)
        {
            var index = math.clamp((int)rank, 0, 60);
            return 1ul << index;
        }

        private static FixedString64Bytes BuildSnapshotContext(MilitaryRank rank)
        {
            FixedString64Bytes context = default;
            context.Append("rank=");
            context.Append(rank.ToString());
            return context;
        }
    }
}
