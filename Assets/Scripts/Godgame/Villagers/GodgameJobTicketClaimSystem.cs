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
    /// Assigns open job tickets to the most suitable nearby villager.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct GodgameJobTicketClaimSystem : ISystem
    {

        private ComponentLookup<JobTicket> _ticketLookup;
        private ComponentLookup<JobAssignment> _assignmentLookup;
        private ComponentLookup<VillagerJobState> _jobLookup;
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerWorkRole> _roleLookup;
        private ComponentLookup<VillagerWorkCooldown> _cooldownLookup;
        private ComponentLookup<GodgameResourceNodeMirror> _nodeLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private BufferLookup<JobTicketGroupMember> _groupLookup;
        private EntityQuery _ticketQuery;
        private EntityQuery _villagerQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<JobAssignment>();
            state.RequireForUpdate<GodgameJobTicketTuning>();
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);
            _assignmentLookup = state.GetComponentLookup<JobAssignment>(false);
            _jobLookup = state.GetComponentLookup<VillagerJobState>(true);
            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _roleLookup = state.GetComponentLookup<VillagerWorkRole>(true);
            _cooldownLookup = state.GetComponentLookup<VillagerWorkCooldown>(true);
            _nodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(true);
            _groupLookup = state.GetBufferLookup<JobTicketGroupMember>();

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
            _cooldownLookup.Update(ref state);
            _nodeLookup.Update(ref state);
            _pileLookup.Update(ref state);
            _groupLookup.Update(ref state);

            var villagerEntities = _villagerQuery.ToEntityArray(state.WorldUpdateAllocator);
            if (villagerEntities.Length == 0)
            {
                return;
            }

            var tuning = SystemAPI.GetSingleton<GodgameJobTicketTuning>();
            var movementTuning = SystemAPI.TryGetSingleton<VillagerMovementTuning>(out var movementTuningValue)
                ? movementTuningValue
                : VillagerMovementTuning.Default;
            var expectedSpeed = ResolveExpectedMoveSpeed(movementTuning);

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
                if (ticket.WorkAmount <= 0f && (ticket.IsSingleItem == 0 || ticket.ItemMass <= 0f))
                {
                    continue;
                }

                if (!TryResolveTarget(ticket.TargetEntity, ticket.IsSingleItem != 0, out var targetPosition, out var isPile))
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
                    if (job.Type != JobType.Gather || job.Phase != JobPhase.Idle)
                    {
                        continue;
                    }

                    if (tuning.EnableReplanBackoff != 0 && job.NextReplanTick > timeState.Tick)
                    {
                        continue;
                    }

                    if (job.NextEligibleTick != 0 && job.NextEligibleTick > timeState.Tick
                        && ShouldGateTicket(job, ticket, timeState.Tick))
                    {
                        continue;
                    }

                    if (_cooldownLookup.HasComponent(villager) && _cooldownLookup[villager].EndTick > timeState.Tick)
                    {
                        continue;
                    }

                    var roleKind = _roleLookup.HasComponent(villager) ? _roleLookup[villager].Value : VillagerWorkRoleKind.None;
                    if (ticket.IsSingleItem == 0 && isPile)
                    {
                        if (roleKind != VillagerWorkRoleKind.Hauler)
                        {
                            continue;
                        }
                    }
                    else if (ticket.IsSingleItem == 0 && roleKind == VillagerWorkRoleKind.Hauler)
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
                var ttlTicks = ResolveClaimTTLTicks(timeState, tuning, bestDistanceSq, expectedSpeed);
                ticket.ClaimExpiresTick = timeState.Tick + ttlTicks;
                ticket.LastStateTick = timeState.Tick;
                _ticketLookup[ticketEntity] = ticket;

                if (ticket.RequiredWorkers > 1 || ticket.IsSingleItem != 0)
                {
                    if (!_groupLookup.HasBuffer(ticketEntity))
                    {
                        state.EntityManager.AddBuffer<JobTicketGroupMember>(ticketEntity);
                        _groupLookup.Update(ref state);
                    }

                    var group = _groupLookup[ticketEntity];
                    if (!GroupContains(group, winner))
                    {
                        group.Add(new JobTicketGroupMember { Villager = winner });
                    }
                }
            }
        }

        private static bool GroupContains(DynamicBuffer<JobTicketGroupMember> group, Entity member)
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

        private bool TryResolveTarget(Entity target, bool allowGeneric, out float3 position, out bool isPile)
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

            if (allowGeneric && _transformLookup.HasComponent(target))
            {
                position = _transformLookup[target].Position;
                return true;
            }

            return false;
        }

        private static float ResolveExpectedMoveSpeed(in VillagerMovementTuning movementTuning)
        {
            var baseSpeed = VillagerJobSystem.StepJob.DefaultMoveSpeed;
            var baseMultiplier = math.max(0.05f, movementTuning.BaseMoveSpeedMultiplier);
            var walkMultiplier = math.max(0.05f, movementTuning.WalkSpeedMultiplier);
            return math.max(0.05f, baseSpeed * baseMultiplier * walkMultiplier);
        }

        private static uint ResolveClaimTTLTicks(in TimeState timeState, in GodgameJobTicketTuning tuning, float distanceSq, float expectedSpeed)
        {
            var baseTtlSeconds = tuning.ClaimTTLSeconds > 0f ? tuning.ClaimTTLSeconds : 6f;
            var clampedSpeed = math.max(0.05f, expectedSpeed);
            var travelSeconds = math.sqrt(math.max(0f, distanceSq)) / clampedSpeed;
            var ttlSeconds = math.max(baseTtlSeconds, travelSeconds * 1.3f + 2f);
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            return (uint)math.max(1f, math.ceil(ttlSeconds / secondsPerTick));
        }

        private static bool ShouldGateTicket(in VillagerJobState job, in JobTicket ticket, uint tick)
        {
            if (job.NextEligibleTick == 0 || job.NextEligibleTick <= tick)
            {
                return false;
            }

            if (job.LastChosenJob != job.Type || job.LastTarget != ticket.TargetEntity)
            {
                return false;
            }

            if (ticket.TargetEntity == Entity.Null && job.ResourceTypeIndex != ticket.ResourceTypeIndex)
            {
                return false;
            }

            switch (job.LastFailCode)
            {
                case VillagerJobFailCode.TargetInvalid:
                case VillagerJobFailCode.TargetDepleted:
                case VillagerJobFailCode.ResourceMismatch:
                case VillagerJobFailCode.RoleMismatch:
                case VillagerJobFailCode.Timeout:
                    return true;
                default:
                    return false;
            }
        }
    }
}
