using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Derives a gameplay-friendly reason for the current villager goal.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(VillagerMindSystemGroup))]
    [UpdateAfter(typeof(VillagerGoalSelectionSystem))]
    public partial struct VillagerGoalReasonSystem : ISystem
    {
        private ComponentLookup<VillagerThreatState> _threatLookup;
        private ComponentLookup<FocusBudget> _focusLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerGoalReason>();
            state.RequireForUpdate<TimeState>();
            _threatLookup = state.GetComponentLookup<VillagerThreatState>(true);
            _focusLookup = state.GetComponentLookup<FocusBudget>(true);
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

            _threatLookup.Update(ref state);
            _focusLookup.Update(ref state);

            foreach (var (reason, goal, needs, entity) in SystemAPI
                         .Query<RefRW<VillagerGoalReason>, RefRO<VillagerGoalState>, RefRO<VillagerNeedState>>()
                         .WithEntityAccess())
            {
                var next = reason.ValueRO;
                next.Kind = VillagerGoalReasonKind.None;
                next.Urgency = 0f;
                next.SourceEntity = Entity.Null;
                next.SetTick = goal.ValueRO.LastGoalChangeTick;

                if (_focusLookup.HasComponent(entity))
                {
                    var focus = _focusLookup[entity];
                    if (focus.IsLocked != 0 || focus.Current <= focus.Reserved + 0.01f)
                    {
                        next.Kind = VillagerGoalReasonKind.FocusLocked;
                        next.Urgency = 0f;
                        next.SetTick = timeState.Tick;
                        ApplyIfChanged(ref reason.ValueRW, next);
                        continue;
                    }
                }

                switch (goal.ValueRO.CurrentGoal)
                {
                    case VillagerGoal.Flee:
                        next.Kind = VillagerGoalReasonKind.Threat;
                        if (_threatLookup.HasComponent(entity))
                        {
                            var threat = _threatLookup[entity];
                            next.Urgency = threat.Urgency;
                            next.SourceEntity = threat.ThreatEntity;
                        }
                        break;
                    case VillagerGoal.Eat:
                        next.Kind = VillagerGoalReasonKind.NeedHunger;
                        next.Urgency = needs.ValueRO.HungerUrgency;
                        break;
                    case VillagerGoal.Sleep:
                        next.Kind = VillagerGoalReasonKind.NeedRest;
                        next.Urgency = needs.ValueRO.RestUrgency;
                        break;
                    case VillagerGoal.Pray:
                        next.Kind = VillagerGoalReasonKind.NeedFaith;
                        next.Urgency = needs.ValueRO.FaithUrgency;
                        break;
                    case VillagerGoal.SeekShelter:
                        next.Kind = VillagerGoalReasonKind.NeedSafety;
                        next.Urgency = needs.ValueRO.SafetyUrgency;
                        break;
                    case VillagerGoal.Socialize:
                        next.Kind = VillagerGoalReasonKind.NeedSocial;
                        next.Urgency = needs.ValueRO.SocialUrgency;
                        break;
                    case VillagerGoal.Work:
                        next.Kind = VillagerGoalReasonKind.NeedWork;
                        next.Urgency = needs.ValueRO.WorkUrgency;
                        break;
                }

                ApplyIfChanged(ref reason.ValueRW, next);
            }
        }

        private static void ApplyIfChanged(ref VillagerGoalReason current, in VillagerGoalReason next)
        {
            if (current.Kind != next.Kind ||
                current.Urgency != next.Urgency ||
                current.SourceEntity != next.SourceEntity ||
                current.SetTick != next.SetTick)
            {
                current = next;
            }
        }
    }
}
