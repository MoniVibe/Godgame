using Godgame.Registry;
using Godgame.Resources;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Attaches nearby compatible tickets to a villager's current assignment without large detours.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameJobTicketClaimSystem))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct GodgameJobTicketBatchSystem : ISystem
    {
        private ComponentLookup<JobTicket> _ticketLookup;
        private ComponentLookup<GodgameResourceNodeMirror> _nodeLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<GodgameJobTicketTuning>();
            state.RequireForUpdate<JobAssignment>();
            state.RequireForUpdate<JobBatchEntry>();
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);
            _nodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState) || timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _ticketLookup.Update(ref state);
            _nodeLookup.Update(ref state);
            _pileLookup.Update(ref state);
            _transformLookup.Update(ref state);

            var tuning = SystemAPI.GetSingleton<GodgameJobTicketTuning>();
            var maxBatchTickets = math.max(1, tuning.MaxBatchTickets);
            if (maxBatchTickets <= 1 || tuning.MaxAttachPerTick <= 0 || tuning.AttachRadius <= 0f)
            {
                return;
            }

            var attachRadiusSq = tuning.AttachRadius * tuning.AttachRadius;
            var ttlTicks = ResolveClaimTTL(timeState, tuning);
            var maxBatchWork = tuning.MaxBatchWorkUnits;

            var openTickets = new NativeList<OpenTicket>(Allocator.Temp);
            foreach (var (ticket, entity) in SystemAPI.Query<RefRO<JobTicket>>().WithEntityAccess())
            {
                var value = ticket.ValueRO;
                if (value.State != JobTicketState.Open || value.Type != JobTicketType.Gather || value.WorkAmount <= 0f)
                {
                    continue;
                }
                if (value.RequiredWorkers > 1 || value.IsSingleItem != 0)
                {
                    continue;
                }

                if (!TryResolveTargetPosition(value.TargetEntity, out var position))
                {
                    continue;
                }

                openTickets.Add(new OpenTicket
                {
                    TicketEntity = entity,
                    Ticket = value,
                    Position = position,
                    IsValid = 1
                });
            }

            if (openTickets.Length == 0)
            {
                openTickets.Dispose();
                return;
            }

            foreach (var (job, assignment, batch, entity) in SystemAPI
                         .Query<RefRO<VillagerJobState>, RefRW<JobAssignment>, DynamicBuffer<JobBatchEntry>>()
                         .WithEntityAccess())
            {
                if (assignment.ValueRO.Ticket == Entity.Null)
                {
                    continue;
                }

                if (!_ticketLookup.HasComponent(assignment.ValueRO.Ticket))
                {
                    continue;
                }

                var primaryTicket = _ticketLookup[assignment.ValueRO.Ticket];
                if (primaryTicket.Assignee != entity || primaryTicket.State == JobTicketState.Done || primaryTicket.State == JobTicketState.Cancelled)
                {
                    continue;
                }
                if (primaryTicket.RequiredWorkers > 1 || primaryTicket.IsSingleItem != 0)
                {
                    continue;
                }

                if (job.ValueRO.Phase != JobPhase.Idle && job.ValueRO.Phase != JobPhase.NavigateToNode)
                {
                    continue;
                }

                if (!TryResolveTargetPosition(primaryTicket.TargetEntity, out var primaryPosition))
                {
                    continue;
                }

                var batchWork = math.max(0f, primaryTicket.WorkAmount);
                CleanupBatch(entity, batch, ref batchWork);

                var remainingSlots = maxBatchTickets - 1 - batch.Length;
                var attachesLeft = math.min(remainingSlots, tuning.MaxAttachPerTick);
                if (attachesLeft <= 0)
                {
                    continue;
                }

                for (int attachIndex = 0; attachIndex < attachesLeft; attachIndex++)
                {
                    if (!TryFindAttachCandidate(assignment.ValueRO.Ticket, primaryTicket, primaryPosition, batch, openTickets, attachRadiusSq,
                            maxBatchWork, batchWork, out var candidateIndex))
                    {
                        break;
                    }

                    var candidate = openTickets[candidateIndex];
                    candidate.IsValid = 0;
                    openTickets[candidateIndex] = candidate;

                    var ticket = candidate.Ticket;
                    ticket.State = JobTicketState.Claimed;
                    ticket.Assignee = entity;
                    ticket.ClaimExpiresTick = timeState.Tick + ttlTicks;
                    ticket.LastStateTick = timeState.Tick;
                    _ticketLookup[candidate.TicketEntity] = ticket;

                    batch.Add(new JobBatchEntry { Ticket = candidate.TicketEntity });
                    batchWork += math.max(0f, ticket.WorkAmount);
                }
            }

            openTickets.Dispose();
        }

        private void CleanupBatch(Entity owner, DynamicBuffer<JobBatchEntry> batch, ref float batchWork)
        {
            for (int i = batch.Length - 1; i >= 0; i--)
            {
                var entry = batch[i];
                if (!_ticketLookup.HasComponent(entry.Ticket))
                {
                    batch.RemoveAt(i);
                    continue;
                }

                var ticket = _ticketLookup[entry.Ticket];
                if (ticket.Assignee != owner || ticket.State == JobTicketState.Done || ticket.State == JobTicketState.Cancelled)
                {
                    batch.RemoveAt(i);
                    continue;
                }

                batchWork += math.max(0f, ticket.WorkAmount);
            }
        }

        private bool TryFindAttachCandidate(Entity primaryTicketEntity, in JobTicket primaryTicket, float3 primaryPosition,
            DynamicBuffer<JobBatchEntry> batch, NativeList<OpenTicket> openTickets, float attachRadiusSq, float maxBatchWork, float batchWork,
            out int candidateIndex)
        {
            candidateIndex = -1;
            var bestDist = float.MaxValue;
            var bestEntityIndex = int.MaxValue;

            for (int i = 0; i < openTickets.Length; i++)
            {
                var candidate = openTickets[i];
                if (candidate.IsValid == 0)
                {
                    continue;
                }

                if (candidate.Ticket.ResourceTypeIndex != primaryTicket.ResourceTypeIndex)
                {
                    continue;
                }

                if (candidate.TicketEntity == primaryTicketEntity)
                {
                    continue;
                }

                if (BatchContains(batch, candidate.TicketEntity))
                {
                    continue;
                }

                var distSq = math.distancesq(primaryPosition, candidate.Position);
                if (distSq > attachRadiusSq)
                {
                    continue;
                }

                var projectedWork = batchWork + math.max(0f, candidate.Ticket.WorkAmount);
                if (maxBatchWork > 0f && projectedWork > maxBatchWork)
                {
                    continue;
                }

                var candidateIndexValue = candidate.TicketEntity.Index;
                if (distSq < bestDist || (math.abs(distSq - bestDist) <= 0.0001f && candidateIndexValue < bestEntityIndex))
                {
                    bestDist = distSq;
                    bestEntityIndex = candidateIndexValue;
                    candidateIndex = i;
                }
            }

            return candidateIndex >= 0;
        }

        private static bool BatchContains(DynamicBuffer<JobBatchEntry> batch, Entity ticket)
        {
            for (int i = 0; i < batch.Length; i++)
            {
                if (batch[i].Ticket == ticket)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveTargetPosition(Entity target, out float3 position)
        {
            position = float3.zero;
            if (target == Entity.Null)
            {
                return false;
            }

            if (!_transformLookup.HasComponent(target))
            {
                return false;
            }

            if (_nodeLookup.HasComponent(target))
            {
                var node = _nodeLookup[target];
                if (node.IsDepleted != 0 || node.RemainingAmount <= 0f)
                {
                    return false;
                }
            }

            if (_pileLookup.HasComponent(target))
            {
                var pile = _pileLookup[target];
                if (pile.Amount <= 0f)
                {
                    return false;
                }
            }

            position = _transformLookup[target].Position;
            return true;
        }

        private static uint ResolveClaimTTL(in TimeState timeState, in GodgameJobTicketTuning tuning)
        {
            var ttlSeconds = tuning.ClaimTTLSeconds > 0f ? tuning.ClaimTTLSeconds : 6f;
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            return (uint)math.max(1f, math.ceil(ttlSeconds / secondsPerTick));
        }

        private struct OpenTicket
        {
            public Entity TicketEntity;
            public JobTicket Ticket;
            public float3 Position;
            public byte IsValid;
        }
    }
}
