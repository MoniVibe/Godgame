using Godgame.Registry;
using Godgame.Resources;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Clears expired or invalid job ticket claims and closes tickets for depleted targets.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameJobTicketSpawnSystem))]
    public partial struct GodgameJobTicketCleanupSystem : ISystem
    {
        private ComponentLookup<GodgameResourceNodeMirror> _nodeLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private BufferLookup<JobTicketGroupMember> _groupLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            _nodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(true);
            _groupLookup = state.GetBufferLookup<JobTicketGroupMember>();
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

            _nodeLookup.Update(ref state);
            _pileLookup.Update(ref state);
            _groupLookup.Update(ref state);
            _transformLookup.Update(ref state);

            foreach (var (ticket, entity) in SystemAPI.Query<RefRW<JobTicket>>().WithEntityAccess())
            {
                ref var value = ref ticket.ValueRW;

                var target = value.TargetEntity;
                var hasNode = target != Entity.Null && _nodeLookup.HasComponent(target);
                var hasPile = target != Entity.Null && _pileLookup.HasComponent(target);
                var hasGeneric = value.IsSingleItem != 0 && target != Entity.Null && _transformLookup.HasComponent(target);

                if (!hasNode && !hasPile && !hasGeneric)
                {
                    if (value.State != JobTicketState.Cancelled)
                    {
                        value.State = JobTicketState.Cancelled;
                        value.Assignee = Entity.Null;
                        value.ClaimExpiresTick = 0;
                        value.LastStateTick = timeState.Tick;
                        ClearGroup(entity);
                    }
                    continue;
                }

                if (value.State == JobTicketState.Claimed
                    && value.ClaimExpiresTick != 0
                    && value.ClaimExpiresTick <= timeState.Tick)
                {
                    value.State = JobTicketState.Open;
                    value.Assignee = Entity.Null;
                    value.ClaimExpiresTick = 0;
                    value.LastStateTick = timeState.Tick;
                }

                if (value.IsSingleItem == 0 && value.WorkAmount <= 0f)
                {
                    if (value.State != JobTicketState.Done)
                    {
                        value.State = JobTicketState.Done;
                        value.Assignee = Entity.Null;
                        value.ClaimExpiresTick = 0;
                        value.LastStateTick = timeState.Tick;
                        ClearGroup(entity);
                    }
                    continue;
                }

                if (hasNode)
                {
                    var node = _nodeLookup[target];
                    if (node.IsDepleted != 0 || node.RemainingAmount <= 0f)
                    {
                        if (value.State != JobTicketState.Done)
                        {
                            value.State = JobTicketState.Done;
                            value.Assignee = Entity.Null;
                            value.ClaimExpiresTick = 0;
                            value.LastStateTick = timeState.Tick;
                            ClearGroup(entity);
                        }
                    }
                }

                if (hasPile)
                {
                    var pile = _pileLookup[target];
                    if (pile.Amount <= 0f)
                    {
                        if (value.State != JobTicketState.Done)
                        {
                            value.State = JobTicketState.Done;
                            value.Assignee = Entity.Null;
                            value.ClaimExpiresTick = 0;
                            value.LastStateTick = timeState.Tick;
                            ClearGroup(entity);
                        }
                    }
                }
            }
        }

        private void ClearGroup(Entity ticketEntity)
        {
            if (_groupLookup.HasBuffer(ticketEntity))
            {
                var group = _groupLookup[ticketEntity];
                group.Clear();
            }
        }
    }
}
