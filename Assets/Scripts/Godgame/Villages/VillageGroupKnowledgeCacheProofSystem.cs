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
    private const uint ThreatTtlTicks = 600;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillageAIDecision>();
            state.RequireForUpdate<GroupKnowledgeCache>();
            state.RequireForUpdate<GroupKnowledgeConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_printed != 0)
            {
                state.Enabled = false;
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            foreach (var (village, awareness, decision, config, cache) in SystemAPI
                         .Query<RefRO<Village>, RefRO<VillageNeedAwareness>, RefRO<VillageAIDecision>, RefRO<GroupKnowledgeConfig>, DynamicBuffer<GroupKnowledgeEntry>>())
            {
                if (decision.ValueRO.DecisionType != 3)
                {
                    continue;
                }

                if (!TryFindActiveThreat(cache, tick, config.ValueRO, out var threat))
                {
                    continue;
                }

                var mayor = awareness.ValueRO.InfluenceEntity;
                UnityDebug.Log(
                    $"[GodgameVillageGroupCacheProof] PASS tick={tick} village={village.ValueRO.VillageId} mayor={mayor.Index}:{mayor.Version} threat={threat.Subject.Index}:{threat.Subject.Version} entries={cache.Length}");
                _printed = 1;
                state.Enabled = false;
                return;
            }
        }

        private static bool TryFindActiveThreat(DynamicBuffer<GroupKnowledgeEntry> cache, uint tick, in GroupKnowledgeConfig config, out GroupKnowledgeEntry entry)
        {
            entry = default;
            var minScore = math.max(0.15f, config.MinConfidence);
            var staleAfterTicks = config.StaleAfterTicks > 0
                ? math.min(config.StaleAfterTicks, ThreatTtlTicks)
                : ThreatTtlTicks;

            for (int i = 0; i < cache.Length; i++)
            {
                var candidate = cache[i];
                if (candidate.Kind != GroupKnowledgeClaimKind.ThreatSeen || candidate.Subject == Entity.Null)
                {
                    continue;
                }

                if (staleAfterTicks > 0 && tick - candidate.LastSeenTick > staleAfterTicks)
                {
                    continue;
                }

                if (math.saturate(candidate.Confidence) < minScore)
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
