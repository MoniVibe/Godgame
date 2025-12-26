using Godgame.Relations;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Perception;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Emits lightweight chatter events when villagers are near trusted neighbors.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerChatterSystem : ISystem
    {
        private BufferLookup<EntityRelation> _relationLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerSpeechChannel>();
            state.RequireForUpdate<TimeState>();
            _relationLookup = state.GetBufferLookup<EntityRelation>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var config = SystemAPI.HasSingleton<VillagerChatterConfig>()
                ? SystemAPI.GetSingleton<VillagerChatterConfig>()
                : VillagerChatterConfig.Default;

            if (!CadenceGate.ShouldRun(timeState.Tick, config.CadenceTicks))
            {
                return;
            }

            var cooldownTicks = (uint)math.max(1, math.ceil(config.BaseCooldownSeconds / math.max(1e-4f, timeState.FixedDeltaTime)));
            var maxRecipients = math.clamp((int)config.MaxRecipients, 0, 8);
            if (maxRecipients == 0)
            {
                return;
            }

            DynamicBuffer<VillagerChatterEvent> eventBuffer = default;
            if (SystemAPI.TryGetSingletonEntity<VillagerChatterEventBuffer>(out var eventEntity) &&
                state.EntityManager.HasBuffer<VillagerChatterEvent>(eventEntity))
            {
                eventBuffer = state.EntityManager.GetBuffer<VillagerChatterEvent>(eventEntity);
                eventBuffer.Clear();
            }

            _relationLookup.Update(ref state);

            foreach (var (speech, ai, perceived, entity) in SystemAPI
                         .Query<RefRW<VillagerSpeechChannel>, RefRO<VillagerAIState>, DynamicBuffer<PerceivedEntity>>()
                         .WithEntityAccess())
            {
                var channel = speech.ValueRW;
                if (channel.NextAvailableTick > timeState.Tick)
                {
                    continue;
                }

                if (!TryResolveChatterKind(ai.ValueRO, config, out var chance, out var kind))
                {
                    continue;
                }

                if (!RollChance(entity, timeState.Tick, chance))
                {
                    continue;
                }

                var candidates = new FixedList128Bytes<ChatterCandidate>();
                for (int i = 0; i < perceived.Length; i++)
                {
                    var contact = perceived[i];
                    if (contact.TargetEntity == Entity.Null || contact.TargetEntity == entity)
                    {
                        continue;
                    }

                    if (contact.Confidence < config.MinPerceptionConfidence)
                    {
                        continue;
                    }

                    if (config.MaxDistance > 0f && contact.Distance > config.MaxDistance)
                    {
                        continue;
                    }

                    var tier = ResolveRelationTier(entity, contact.TargetEntity, ref _relationLookup);
                    if (tier < config.MinRelationTier)
                    {
                        continue;
                    }

                    ConsiderCandidate(ref candidates, maxRecipients, new ChatterCandidate
                    {
                        Target = contact.TargetEntity,
                        Distance = contact.Distance
                    });
                }

                if (candidates.Length == 0)
                {
                    continue;
                }

                var messageId = math.hash(new uint2((uint)(entity.Index + 1), timeState.Tick));
                channel.NextAvailableTick = timeState.Tick + cooldownTicks;
                channel.LastSpeechTick = timeState.Tick;
                channel.LastMessageId = messageId;
                channel.LastListener = candidates[0].Target;
                speech.ValueRW = channel;

                if (!eventBuffer.IsCreated)
                {
                    continue;
                }

                for (int i = 0; i < candidates.Length; i++)
                {
                    eventBuffer.Add(new VillagerChatterEvent
                    {
                        Speaker = entity,
                        Listener = candidates[i].Target,
                        Kind = kind,
                        Tick = timeState.Tick,
                        MessageId = messageId
                    });
                }
            }
        }

        private static bool TryResolveChatterKind(in VillagerAIState ai, in VillagerChatterConfig config, out float chance, out VillagerChatterKind kind)
        {
            switch (ai.CurrentState)
            {
                case VillagerAIState.State.Working:
                case VillagerAIState.State.Travelling:
                    chance = config.ChatterChanceWorking;
                    kind = VillagerChatterKind.Work;
                    return chance > 0f;
                case VillagerAIState.State.Idle:
                case VillagerAIState.State.Eating:
                    chance = config.ChatterChanceIdle;
                    kind = VillagerChatterKind.Idle;
                    return chance > 0f;
                default:
                    chance = 0f;
                    kind = VillagerChatterKind.Idle;
                    return false;
            }
        }

        private static bool RollChance(Entity entity, uint tick, float chance)
        {
            var seed = math.hash(new uint2((uint)(entity.Index + 1), tick));
            var rng = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            return rng.NextFloat() <= math.saturate(chance);
        }

        private static RelationTier ResolveRelationTier(Entity self, Entity other, ref BufferLookup<EntityRelation> relationLookup)
        {
            if (!relationLookup.HasBuffer(self))
            {
                return RelationTier.Neutral;
            }

            var relations = relationLookup[self];
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i].OtherEntity == other)
                {
                    return relations[i].Tier;
                }
            }

            return RelationTier.Neutral;
        }

        private static void ConsiderCandidate(ref FixedList128Bytes<ChatterCandidate> candidates, int maxRecipients, in ChatterCandidate candidate)
        {
            if (maxRecipients <= 0)
            {
                return;
            }

            if (candidates.Length < maxRecipients)
            {
                candidates.Add(candidate);
                return;
            }

            var worstIndex = 0;
            var worstDistance = candidates[0].Distance;
            for (int i = 1; i < candidates.Length; i++)
            {
                if (candidates[i].Distance > worstDistance)
                {
                    worstDistance = candidates[i].Distance;
                    worstIndex = i;
                }
            }

            if (candidate.Distance < worstDistance)
            {
                candidates[worstIndex] = candidate;
            }
        }

        private struct ChatterCandidate
        {
            public Entity Target;
            public float Distance;
        }
    }
}
