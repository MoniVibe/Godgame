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
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Headless.GodgameHeadlessCombatLoopSystem))]
    [UpdateBefore(typeof(VillageAIDecisionSystem))]
    public partial struct VillageGroupKnowledgeCachePostSystem : ISystem
    {
        private const float MinThreatUrgency = 0.15f;
        private const uint ThreatTtlTicks = 600; // ~10s at 60hz

        private BufferLookup<GroupKnowledgeEntry> _groupCacheLookup;
        private ComponentLookup<GroupKnowledgeConfig> _configLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<GroupKnowledgeCache>();
            state.RequireForUpdate<GroupKnowledgeConfig>();
            state.RequireForUpdate<VillagerThreatState>();

            _groupCacheLookup = state.GetBufferLookup<GroupKnowledgeEntry>(false);
            _configLookup = state.GetComponentLookup<GroupKnowledgeConfig>(true);
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
            _configLookup.Update(ref state);
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
                var flags = (byte)GroupKnowledgeFlags.FromPerception;
                if (threatValue.HasLineOfSight == 0)
                {
                    flags |= GroupKnowledgeFlags.Unreliable;
                }

                var entry = new GroupKnowledgeEntry
                {
                    Kind = GroupKnowledgeClaimKind.ThreatSeen,
                    Flags = flags,
                    Subject = threatValue.ThreatEntity,
                    Source = villager,
                    Position = position,
                    Confidence = math.saturate(threatValue.Urgency) * confidence,
                    LastSeenTick = timeState.Tick,
                    PayloadId = default
                };

                var cache = _groupCacheLookup[villageEntity];
                var config = _configLookup.HasComponent(villageEntity)
                    ? _configLookup[villageEntity]
                    : GroupKnowledgeConfig.Default;
                UpsertEntry(cache, timeState.Tick, config, entry, ThreatTtlTicks);
            }
        }

        private static void UpsertEntry(
            DynamicBuffer<GroupKnowledgeEntry> entries,
            uint tick,
            in GroupKnowledgeConfig config,
            GroupKnowledgeEntry candidate,
            uint fallbackStaleTicks)
        {
            if (candidate.Confidence < config.MinConfidence)
            {
                candidate.Flags |= GroupKnowledgeFlags.Unreliable;
            }

            candidate.LastSeenTick = tick;

            var matchIndex = -1;
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Kind != candidate.Kind)
                {
                    continue;
                }

                if (candidate.Subject != Entity.Null && entry.Subject == candidate.Subject)
                {
                    matchIndex = i;
                    break;
                }
            }

            if (matchIndex >= 0)
            {
                var existing = entries[matchIndex];
                existing.Confidence = math.max(existing.Confidence, candidate.Confidence);
                existing.LastSeenTick = tick;
                existing.Subject = candidate.Subject != Entity.Null ? candidate.Subject : existing.Subject;
                existing.Source = candidate.Source != Entity.Null ? candidate.Source : existing.Source;
                existing.Position = math.lengthsq(candidate.Position) > 0f ? candidate.Position : existing.Position;
                existing.Flags |= candidate.Flags;
                entries[matchIndex] = existing;
                return;
            }

            if (entries.Length < math.max(1, config.MaxEntries))
            {
                entries.Add(candidate);
                return;
            }

            var staleAfterTicks = config.StaleAfterTicks > 0 ? config.StaleAfterTicks : fallbackStaleTicks;
            var evictIndex = SelectEvictionIndex(entries, tick, staleAfterTicks);
            entries[evictIndex] = candidate;
        }

        private static int SelectEvictionIndex(DynamicBuffer<GroupKnowledgeEntry> entries, uint tick, uint staleAfterTicks)
        {
            var worstIndex = 0;
            var worstScore = float.MaxValue;
            var decayTicks = staleAfterTicks > 0 ? staleAfterTicks : 600u;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var age = tick >= entry.LastSeenTick ? tick - entry.LastSeenTick : 0u;
                var score = entry.Confidence - math.min(1f, age / (float)decayTicks);
                if (score < worstScore)
                {
                    worstScore = score;
                    worstIndex = i;
                }
            }

            return worstIndex;
        }
    }
}
