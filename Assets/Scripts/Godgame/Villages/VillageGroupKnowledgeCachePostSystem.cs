using Godgame.AI;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Knowledge;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Posts micro-level perception signals into the village-level group cache (communications MVP).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Headless.GodgameHeadlessCombatLoopSystem))]
    [UpdateBefore(typeof(VillageAIDecisionSystem))]
    public partial struct VillageGroupKnowledgeCachePostSystem : ISystem
    {
        private const float MinThreatUrgency = 0.15f;
        private const uint ThreatTtlTicks = 600; // ~10s at 60hz

        private BufferLookup<GroupKnowledgeEntry> _groupCacheLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<GroupKnowledgeCacheTag>();
            state.RequireForUpdate<VillagerThreatState>();

            _groupCacheLookup = state.GetBufferLookup<GroupKnowledgeEntry>(false);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _groupCacheLookup.Update(ref state);
            _transformLookup.Update(ref state);

            foreach (var (threat, patriotism, villager) in SystemAPI
                         .Query<RefRO<VillagerThreatState>, RefRO<VillagerPatriotism>>()
                         .WithEntityAccess())
            {
                var villageEntity = patriotism.ValueRO.VillageEntity;
                if (villageEntity == Entity.Null || !_groupCacheLookup.HasBuffer(villageEntity))
                {
                    continue;
                }

                var threatValue = threat.ValueRO;
                if (threatValue.ThreatEntity == Entity.Null || threatValue.Urgency < MinThreatUrgency)
                {
                    continue;
                }

                var confidence = threatValue.HasLineOfSight != 0 ? 1f : 0.55f;
                var position = _transformLookup.HasComponent(villager) ? _transformLookup[villager].Position : float3.zero;

                var entry = new GroupKnowledgeEntry
                {
                    Kind = GroupKnowledgeKind.ThreatSeen,
                    Flags = threatValue.HasLineOfSight,
                    AuxId = 0,
                    FirstTick = timeState.Tick,
                    LastTick = timeState.Tick,
                    ExpireTick = timeState.Tick + ThreatTtlTicks,
                    SubjectEntity = threatValue.ThreatEntity,
                    ReporterEntity = villager,
                    Position = position,
                    Urgency = math.saturate(threatValue.Urgency),
                    Confidence = confidence
                };

                var cache = _groupCacheLookup[villageEntity];
                GroupKnowledgeCache.Upsert(cache, in entry);
            }
        }
    }
}

