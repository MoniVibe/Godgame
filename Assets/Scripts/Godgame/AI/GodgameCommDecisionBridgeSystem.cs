using Godgame.AI;
using PureDOTS.Runtime.Communication;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.AI
{
    /// <summary>
    /// Bridges CommDecision entries into Godgame order intents.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(AISystemGroup))]
    public partial struct GodgameCommDecisionBridgeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<CommDecision>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (decisions, entity) in SystemAPI.Query<DynamicBuffer<CommDecision>>().WithEntityAccess())
            {
                if (decisions.Length == 0)
                {
                    continue;
                }

                var bestIndex = -1;
                var bestPriority = (byte)0;
                var bestConfidence = 0f;

                for (int i = 0; i < decisions.Length; i++)
                {
                    var decision = decisions[i];
                    if (decision.Type != CommDecisionType.Accepted)
                    {
                        continue;
                    }

                    var priority = (byte)decision.OrderPriority;
                    if (bestIndex < 0 || priority > bestPriority || (priority == bestPriority && decision.Confidence > bestConfidence))
                    {
                        bestIndex = i;
                        bestPriority = priority;
                        bestConfidence = decision.Confidence;
                    }
                }

                if (bestIndex >= 0)
                {
                    var decision = decisions[bestIndex];
                    var intent = new GodgameCommOrderIntent
                    {
                        Verb = decision.OrderVerb,
                        Sender = decision.Sender,
                        Target = decision.OrderTarget,
                        TargetPosition = decision.OrderTargetPosition,
                        Side = decision.OrderSide,
                        Priority = decision.OrderPriority,
                        TimingWindowTicks = decision.TimingWindowTicks,
                        ContextHash = decision.ContextHash,
                        SourceMessageId = decision.MessageId,
                        ReceivedTick = timeState.Tick,
                        Confidence = decision.Confidence,
                        Inferred = decision.Inferred
                    };

                    if (state.EntityManager.HasComponent<GodgameCommOrderIntent>(entity))
                    {
                        state.EntityManager.SetComponentData(entity, intent);
                    }
                    else
                    {
                        ecb.AddComponent(entity, intent);
                    }
                }

                decisions.Clear();
            }
        }
    }
}
