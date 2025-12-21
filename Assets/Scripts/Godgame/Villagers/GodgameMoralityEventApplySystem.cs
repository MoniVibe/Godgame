using System.Collections.Generic;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Social;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Godgame adapter: applies MoralityEvent effects onto villager alignment axes.
    /// Event-driven and rewind-safe; later iterations should incorporate culture/stance tables.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageAIDecisionSystem))]
    public partial struct GodgameMoralityEventApplySystem : ISystem
    {
        private ComponentLookup<VillagerAlignment> _alignmentLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<MoralityEventQueueTag>();

            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(false);
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

            _alignmentLookup.Update(ref state);

            var queueEntity = SystemAPI.GetSingletonEntity<MoralityEventQueueTag>();
            if (!SystemAPI.HasBuffer<MoralityEvent>(queueEntity))
            {
                return;
            }

            var queue = SystemAPI.GetBuffer<MoralityEvent>(queueEntity);
            if (queue.Length == 0)
            {
                return;
            }

            var events = queue.ToNativeArray(Allocator.Temp);
            var processedCount = events.Length;
            NativeSortExtension.Sort(events, new MoralityEventComparer());

            for (int i = 0; i < events.Length; i++)
            {
                ApplyEvent(events[i]);
            }

            queue.Clear();
            events.Dispose();

            var processingState = SystemAPI.GetComponentRW<MoralityEventProcessingState>(queueEntity);
            processingState.ValueRW.LastProcessedTick = timeState.Tick;
            processingState.ValueRW.LastProcessedCount = processedCount;
            processingState.ValueRW.TotalProcessedCount += processedCount;
        }

        private void ApplyEvent(in MoralityEvent evt)
        {
            if (evt.Actor == Entity.Null || !_alignmentLookup.HasComponent(evt.Actor))
            {
                return;
            }

            var alignment = _alignmentLookup[evt.Actor];
            ApplyToken(ref alignment, evt.Token, evt.Magnitude, evt.IntentFlags);
            _alignmentLookup[evt.Actor] = alignment;
        }

        private static void ApplyToken(ref VillagerAlignment alignment, MoralityActionToken token, short magnitude, MoralityIntentFlags intentFlags)
        {
            var absMagnitude = math.abs((int)magnitude);
            var steps = math.clamp((absMagnitude + 19) / 20, 0, 10);
            if (steps <= 0)
            {
                return;
            }

            var moralDelta = 0;
            var orderDelta = 0;
            var purityDelta = 0;

            switch (token)
            {
                case MoralityActionToken.ObeyOrder:
                    orderDelta = steps;
                    break;
                case MoralityActionToken.DisobeyOrder:
                    orderDelta = -steps;
                    break;
                case MoralityActionToken.Betray:
                    moralDelta = -steps;
                    orderDelta = -(steps / 2);
                    purityDelta = -(steps / 2);
                    break;
                case MoralityActionToken.DefendHome:
                    moralDelta = steps;
                    orderDelta = steps / 2;
                    break;
                case MoralityActionToken.Rescue:
                    moralDelta = steps;
                    break;
                case MoralityActionToken.Torture:
                    moralDelta = -(steps * 2);
                    purityDelta = -steps;
                    break;
                case MoralityActionToken.Execute:
                    moralDelta = -steps;
                    orderDelta = steps / 2;
                    break;
                case MoralityActionToken.Donate:
                    moralDelta = steps;
                    purityDelta = steps / 2;
                    break;
                case MoralityActionToken.ExploitWorkers:
                    moralDelta = -steps;
                    purityDelta = -steps;
                    break;
                case MoralityActionToken.Pollute:
                case MoralityActionToken.Deforest:
                    purityDelta = -steps;
                    break;
                case MoralityActionToken.RestoreNature:
                    purityDelta = steps;
                    moralDelta = steps / 2;
                    break;
                case MoralityActionToken.Build:
                    orderDelta = steps;
                    break;
                case MoralityActionToken.BuildShelter:
                    moralDelta = steps;
                    orderDelta = steps;
                    break;
                case MoralityActionToken.BuildDefense:
                    orderDelta = steps;
                    break;
            }

            if ((intentFlags & MoralityIntentFlags.Malicious) != 0)
            {
                moralDelta -= steps;
                purityDelta -= steps / 2;
            }

            alignment.MoralAxis = (sbyte)math.clamp((int)alignment.MoralAxis + moralDelta, -100, 100);
            alignment.OrderAxis = (sbyte)math.clamp((int)alignment.OrderAxis + orderDelta, -100, 100);
            alignment.PurityAxis = (sbyte)math.clamp((int)alignment.PurityAxis + purityDelta, -100, 100);
        }

        private readonly struct MoralityEventComparer : IComparer<MoralityEvent>
        {
            public int Compare(MoralityEvent x, MoralityEvent y)
            {
                var tickCompare = x.Tick.CompareTo(y.Tick);
                if (tickCompare != 0)
                {
                    return tickCompare;
                }

                var actorCompare = x.Actor.Index.CompareTo(y.Actor.Index);
                if (actorCompare != 0)
                {
                    return actorCompare;
                }

                var tokenCompare = ((ushort)x.Token).CompareTo((ushort)y.Token);
                if (tokenCompare != 0)
                {
                    return tokenCompare;
                }

                return x.Magnitude.CompareTo(y.Magnitude);
            }
        }
    }
}
