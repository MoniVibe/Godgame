using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Knowledge;
using Unity.Entities;
using Unity.Mathematics;
using UnityDebug = UnityEngine.Debug;

namespace Godgame.Villages
{
    /// <summary>
    /// Headless-only proof that village macro decisions are driven by the group cache (communications MVP).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageAIDecisionSystem))]
    public partial struct VillageGroupKnowledgeCacheProofSystem : ISystem
    {
        private byte _printed;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillageAIDecision>();
            state.RequireForUpdate<GroupKnowledgeCacheTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_printed != 0)
            {
                state.Enabled = false;
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            foreach (var (village, awareness, decision, cache) in SystemAPI
                         .Query<RefRO<Village>, RefRO<VillageNeedAwareness>, RefRO<VillageAIDecision>, DynamicBuffer<GroupKnowledgeEntry>>())
            {
                if (decision.ValueRO.DecisionType != 3)
                {
                    continue;
                }

                if (!TryFindActiveThreat(cache, tick, out var threat))
                {
                    continue;
                }

                var mayor = awareness.ValueRO.InfluenceEntity;
                UnityDebug.Log(
                    $"[GodgameVillageGroupCacheProof] PASS tick={tick} village={village.ValueRO.VillageId} mayor={mayor.Index}:{mayor.Version} threat={threat.SubjectEntity.Index}:{threat.SubjectEntity.Version} entries={cache.Length}");
                _printed = 1;
                state.Enabled = false;
                return;
            }
        }

        private static bool TryFindActiveThreat(DynamicBuffer<GroupKnowledgeEntry> cache, uint tick, out GroupKnowledgeEntry entry)
        {
            entry = default;

            for (int i = 0; i < cache.Length; i++)
            {
                var candidate = cache[i];
                if (candidate.Kind != GroupKnowledgeKind.ThreatSeen || candidate.SubjectEntity == Entity.Null)
                {
                    continue;
                }

                if (candidate.ExpireTick != 0u && tick > candidate.ExpireTick)
                {
                    continue;
                }

                if (math.saturate(candidate.Urgency) * math.saturate(candidate.Confidence) < 0.15f)
                {
                    continue;
                }

                entry = candidate;
                return true;
            }

            return false;
        }
    }
}

