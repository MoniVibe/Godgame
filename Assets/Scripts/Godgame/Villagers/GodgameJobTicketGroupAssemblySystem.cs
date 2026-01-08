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
    /// Assembles helpers for group tickets and gates execution until quorum is met.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameJobTicketClaimSystem))]
    [UpdateBefore(typeof(GodgameJobTicketBatchSystem))]
    public partial struct GodgameJobTicketGroupAssemblySystem : ISystem
    {
        private ComponentLookup<JobTicket> _ticketLookup;
        private BufferLookup<JobTicketGroupMember> _groupLookup;
        private ComponentLookup<JobAssignment> _assignmentLookup;
        private ComponentLookup<VillagerJobState> _jobLookup;
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<VillagerWorkRole> _roleLookup;
        private ComponentLookup<VillagerAlignment> _alignmentLookup;
        private ComponentLookup<VillagerWorkCooldown> _cooldownLookup;
        private ComponentLookup<VillagerCarryCapacity> _carryLookup;
        private ComponentLookup<GodgameResourceNodeMirror> _nodeLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<GodgameJobTicketTuning>();
            state.RequireForUpdate<JobAssignment>();
            state.RequireForUpdate<VillagerJobState>();
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);
            _groupLookup = state.GetBufferLookup<JobTicketGroupMember>();
            _assignmentLookup = state.GetComponentLookup<JobAssignment>(false);
            _jobLookup = state.GetComponentLookup<VillagerJobState>(true);
            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _roleLookup = state.GetComponentLookup<VillagerWorkRole>(true);
            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
            _cooldownLookup = state.GetComponentLookup<VillagerWorkCooldown>(true);
            _carryLookup = state.GetComponentLookup<VillagerCarryCapacity>(true);
            _nodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        [BurstCompile]
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
            _groupLookup.Update(ref state);
            _assignmentLookup.Update(ref state);
            _jobLookup.Update(ref state);
            _goalLookup.Update(ref state);
            _roleLookup.Update(ref state);
            _alignmentLookup.Update(ref state);
            _cooldownLookup.Update(ref state);
            _carryLookup.Update(ref state);
            _nodeLookup.Update(ref state);
            _pileLookup.Update(ref state);
            _transformLookup.Update(ref state);

            var tuning = SystemAPI.GetSingleton<GodgameJobTicketTuning>();
            if (tuning.HeavyCarryAssemblyRadius <= 0f)
            {
                return;
            }

            var assemblyRadiusSq = tuning.HeavyCarryAssemblyRadius * tuning.HeavyCarryAssemblyRadius;
            var ttlTicks = ResolveClaimTTL(timeState, tuning);
            var carryCapacityOverride = ResolveCarryCapacityOverride();

            var candidates = new NativeList<Candidate>(Allocator.Temp);
            foreach (var (job, assignment, goal, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerJobState>, RefRW<JobAssignment>, RefRO<VillagerGoalState>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                if (assignment.ValueRO.Ticket != Entity.Null)
                {
                    continue;
                }

                if (goal.ValueRO.CurrentGoal != VillagerGoal.Work)
                {
                    continue;
                }

                if (job.ValueRO.Type != JobType.Gather || job.ValueRO.Phase != JobPhase.Idle || job.ValueRO.DecisionCooldown > 0f)
                {
                    continue;
                }

                if (tuning.EnableReplanBackoff != 0 && job.ValueRO.NextReplanTick > timeState.Tick)
                {
                    continue;
                }

                if (_cooldownLookup.HasComponent(entity) && _cooldownLookup[entity].EndTick > timeState.Tick)
                {
                    continue;
                }

                candidates.Add(new Candidate
                {
                    Entity = entity,
                    Position = transform.ValueRO.Position,
                    Role = _roleLookup.HasComponent(entity) ? _roleLookup[entity].Value : VillagerWorkRoleKind.None
                });
            }

            if (candidates.Length == 0)
            {
                candidates.Dispose();
                return;
            }

            foreach (var (ticketRef, ticketEntity) in SystemAPI.Query<RefRO<JobTicket>>().WithEntityAccess())
            {
                var ticket = ticketRef.ValueRO;
                if ((ticket.RequiredWorkers <= 1 && ticket.IsSingleItem == 0)
                    || ticket.State == JobTicketState.Done
                    || ticket.State == JobTicketState.Cancelled)
                {
                    continue;
                }
                if (ticket.Assignee == Entity.Null)
                {
                    continue;
                }

                if (ticket.TargetEntity == Entity.Null || !_transformLookup.HasComponent(ticket.TargetEntity))
                {
                    continue;
                }

                var targetPosition = _transformLookup[ticket.TargetEntity].Position;
                var isPile = ticket.TargetEntity != Entity.Null && _pileLookup.HasComponent(ticket.TargetEntity);
                var isNode = ticket.TargetEntity != Entity.Null && _nodeLookup.HasComponent(ticket.TargetEntity);
                if (!isPile && !isNode && ticket.IsSingleItem == 0)
                {
                    continue;
                }

                if (!_groupLookup.HasBuffer(ticketEntity))
                {
                    state.EntityManager.AddBuffer<JobTicketGroupMember>(ticketEntity);
                    _groupLookup.Update(ref state);
                }

                var group = _groupLookup[ticketEntity];
                PruneGroup(ref group, ticketEntity);
                var minWorkers = math.max(1, ticket.MinWorkers);
                var requiredWorkers = math.max(minWorkers, ticket.RequiredWorkers);
                if (ticket.IsSingleItem != 0 && ticket.ItemMass > 0f)
                {
                    var minCohesion = math.max(0.1f, 1f + tuning.GroupCohesionMin);
                    var baseCapacity = math.max(1f, carryCapacityOverride * minCohesion);
                    var estimated = (int)math.ceil(ticket.ItemMass / baseCapacity);
                    requiredWorkers = math.max(requiredWorkers, estimated);
                }

                if (group.Length < requiredWorkers)
                {
                    for (int i = 0; i < candidates.Length && group.Length < requiredWorkers; i++)
                    {
                        var candidate = candidates[i];
                        if (IsInGroup(group, candidate.Entity))
                        {
                            continue;
                        }

                        if (ticket.IsSingleItem == 0 && isPile && candidate.Role != VillagerWorkRoleKind.Hauler)
                        {
                            continue;
                        }

                        if (ticket.IsSingleItem == 0 && !isPile && candidate.Role == VillagerWorkRoleKind.Hauler)
                        {
                            continue;
                        }

                        var distSq = math.distancesq(candidate.Position, targetPosition);
                        if (distSq > assemblyRadiusSq)
                        {
                            continue;
                        }

                        group.Add(new JobTicketGroupMember { Villager = candidate.Entity });
                        var assignment = _assignmentLookup[candidate.Entity];
                        assignment.Ticket = ticketEntity;
                        assignment.CommitTick = timeState.Tick;
                        _assignmentLookup[candidate.Entity] = assignment;
                    }
                }

                if (group.Length >= minWorkers && ticket.State == JobTicketState.Claimed)
                {
                    var ready = ticket.IsSingleItem != 0 && ticket.ItemMass > 0f
                        ? ResolveGroupCarryCapacity(ref group, carryCapacityOverride, tuning) >= ticket.ItemMass
                        : group.Length >= minWorkers;

                    if (ready)
                    {
                        ticket.State = JobTicketState.InProgress;
                        ticket.ClaimExpiresTick = timeState.Tick + ttlTicks;
                        ticket.LastStateTick = timeState.Tick;
                        _ticketLookup[ticketEntity] = ticket;
                    }
                }
            }

            candidates.Dispose();
        }

        private static bool IsInGroup(DynamicBuffer<JobTicketGroupMember> group, Entity member)
        {
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i].Villager == member)
                {
                    return true;
                }
            }

            return false;
        }

        private void PruneGroup(ref DynamicBuffer<JobTicketGroupMember> group, Entity ticketEntity)
        {
            for (int i = group.Length - 1; i >= 0; i--)
            {
                var member = group[i].Villager;
                if (!_assignmentLookup.HasComponent(member))
                {
                    group.RemoveAt(i);
                    continue;
                }

                var assignment = _assignmentLookup[member];
                if (assignment.Ticket != ticketEntity)
                {
                    group.RemoveAt(i);
                }
            }
        }

        private static uint ResolveClaimTTL(in TimeState timeState, in GodgameJobTicketTuning tuning)
        {
            var ttlSeconds = tuning.ClaimTTLSeconds > 0f ? tuning.ClaimTTLSeconds : 6f;
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            return (uint)math.max(1f, math.ceil(ttlSeconds / secondsPerTick));
        }

        private struct Candidate
        {
            public Entity Entity;
            public float3 Position;
            public VillagerWorkRoleKind Role;
        }

        private float ResolveCarryCapacityOverride()
        {
            if (SystemAPI.TryGetSingleton<BehaviorConfigRegistry>(out var registry))
            {
                var gather = registry.GatherDeliver;
                if (gather.CarryCapacityOverride > 0f)
                {
                    return gather.CarryCapacityOverride;
                }
            }

            return 50f;
        }

        private float ResolveGroupCarryCapacity(ref DynamicBuffer<JobTicketGroupMember> group, float baseCarryCapacity, in GodgameJobTicketTuning tuning)
        {
            float totalCapacity = 0f;
            float orderSum = 0f;
            int count = 0;

            for (int i = 0; i < group.Length; i++)
            {
                var member = group[i].Villager;
                if (!_jobLookup.HasComponent(member))
                {
                    continue;
                }

                var job = _jobLookup[member];
                var capacity = job.CarryMax > 0f ? job.CarryMax : baseCarryCapacity;
                if (_carryLookup.HasComponent(member))
                {
                    var modifier = _carryLookup[member];
                    capacity = capacity * math.max(0.1f, modifier.Multiplier) + modifier.Bonus;
                }

                if (capacity <= 0f)
                {
                    continue;
                }

                totalCapacity += capacity;
                if (_alignmentLookup.HasComponent(member))
                {
                    orderSum += _alignmentLookup[member].OrderAxis;
                }
                count++;
            }

            if (count == 0 || totalCapacity <= 0f)
            {
                return 0f;
            }

            var orderAvg = (orderSum / count) / 100f;
            var cohesion = math.clamp(tuning.GroupCohesionBase + orderAvg * tuning.GroupCohesionOrderWeight,
                tuning.GroupCohesionMin,
                tuning.GroupCohesionMax);
            return totalCapacity * math.max(0.1f, 1f + cohesion);
        }
    }
}
