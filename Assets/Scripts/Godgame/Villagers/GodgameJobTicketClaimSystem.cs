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
    /// Assigns open job tickets to the most suitable nearby villager.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct GodgameJobTicketClaimSystem : ISystem
    {
        private const float ClaimTTLSeconds = 6f;

        private ComponentLookup<JobTicket> _ticketLookup;
        private ComponentLookup<JobAssignment> _assignmentLookup;
        private ComponentLookup<VillagerJobState> _jobLookup;
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerWorkRole> _roleLookup;
        private ComponentLookup<GodgameResourceNodeMirror> _nodeLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private EntityQuery _ticketQuery;
        private EntityQuery _villagerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<JobAssignment>();
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);
            _assignmentLookup = state.GetComponentLookup<JobAssignment>(false);
            _jobLookup = state.GetComponentLookup<VillagerJobState>(true);
            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _roleLookup = state.GetComponentLookup<VillagerWorkRole>(true);
            _nodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(true);

            _ticketQuery = SystemAPI.QueryBuilder()
                .WithAll<JobTicket>()
                .Build();

            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState, JobAssignment, VillagerGoalState, LocalTransform>()
                .Build();
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
            _assignmentLookup.Update(ref state);
            _jobLookup.Update(ref state);
            _goalLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _roleLookup.Update(ref state);
            _nodeLookup.Update(ref state);
            _pileLookup.Update(ref state);

            var villagerEntities = _villagerQuery.ToEntityArray(state.WorldUpdateAllocator);
            if (villagerEntities.Length == 0)
            {
                return;
            }

            var ttlTicks = ResolveClaimTTL(timeState);

            foreach (var ticketEntity in _ticketQuery.ToEntityArray(state.WorldUpdateAllocator))
            {
                if (!_ticketLookup.HasComponent(ticketEntity))
                {
                    continue;
                }

                var ticket = _ticketLookup[ticketEntity];
                if (ticket.State != JobTicketState.Open || ticket.Type != JobTicketType.Gather)
                {
                    continue;
                }

                if (!TryResolveTarget(ticket.TargetEntity, out var targetPosition, out var isPile))
                {
                    ticket.State = JobTicketState.Cancelled;
                    ticket.Assignee = Entity.Null;
                    ticket.ClaimExpiresTick = 0;
                    ticket.LastStateTick = timeState.Tick;
                    _ticketLookup[ticketEntity] = ticket;
                    continue;
                }

                var winner = Entity.Null;
                var bestDistanceSq = float.MaxValue;

                for (int i = 0; i < villagerEntities.Length; i++)
                {
                    var villager = villagerEntities[i];
                    if (!_assignmentLookup.HasComponent(villager) || !_jobLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    var assignment = _assignmentLookup[villager];
                    if (assignment.Ticket != Entity.Null)
                    {
                        continue;
                    }

                    var goal = _goalLookup.HasComponent(villager) ? _goalLookup[villager] : default;
                    if (goal.CurrentGoal != VillagerGoal.Work)
                    {
                        continue;
                    }

                    var job = _jobLookup[villager];
                    if (job.Type != JobType.Gather || job.Phase != JobPhase.Idle || job.DecisionCooldown > 0f)
                    {
                        continue;
                    }

                    var roleKind = _roleLookup.HasComponent(villager) ? _roleLookup[villager].Value : VillagerWorkRoleKind.None;
                    if (isPile)
                    {
                        if (roleKind != VillagerWorkRoleKind.Hauler)
                        {
                            continue;
                        }
                    }
                    else if (roleKind == VillagerWorkRoleKind.Hauler)
                    {
                        continue;
                    }

                    if (roleKind != VillagerWorkRoleKind.Hauler && job.ResourceTypeIndex != ticket.ResourceTypeIndex)
                    {
                        continue;
                    }

                    if (!_transformLookup.HasComponent(villager))
                    {
                        continue;
                    }

                    var position = _transformLookup[villager].Position;
                    var distanceSq = math.distancesq(position, targetPosition);
                    if (distanceSq < bestDistanceSq)
                    {
                        bestDistanceSq = distanceSq;
                        winner = villager;
                    }
                }

                if (winner == Entity.Null)
                {
                    continue;
                }

                var winnerAssignment = _assignmentLookup[winner];
                winnerAssignment.Ticket = ticketEntity;
                winnerAssignment.CommitTick = timeState.Tick;
                _assignmentLookup[winner] = winnerAssignment;

                ticket.State = JobTicketState.Claimed;
                ticket.Assignee = winner;
                ticket.ClaimExpiresTick = timeState.Tick + ttlTicks;
                ticket.LastStateTick = timeState.Tick;
                _ticketLookup[ticketEntity] = ticket;
            }
        }

        private bool TryResolveTarget(Entity target, out float3 position, out bool isPile)
        {
            position = float3.zero;
            isPile = false;

            if (target == Entity.Null)
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

                if (_transformLookup.HasComponent(target))
                {
                    position = _transformLookup[target].Position;
                    return true;
                }
            }

            if (_pileLookup.HasComponent(target))
            {
                var pile = _pileLookup[target];
                if (pile.Amount <= 0f)
                {
                    return false;
                }

                if (_transformLookup.HasComponent(target))
                {
                    position = _transformLookup[target].Position;
                    isPile = true;
                    return true;
                }
            }

            return false;
        }

        private static uint ResolveClaimTTL(in TimeState timeState)
        {
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            return (uint)math.max(1f, math.ceil(ClaimTTLSeconds / secondsPerTick));
        }
    }
}
