using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Emits lightweight liveliness markers for headless analysis.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Villagers.VillagerJobSystem))]
    [UpdateAfter(typeof(Godgame.Villagers.VillagerWorkCooldownSystem))]
    [UpdateAfter(typeof(Godgame.Villagers.VillagerLeisureSystem))]
    [UpdateAfter(typeof(Godgame.Villagers.VillagerSocialFocusSystem))]
    [UpdateBefore(typeof(VillagerActionTelemetrySystem))]
    public partial struct VillagerLivelinessTelemetrySystem : ISystem
    {
        private ComponentLookup<Godgame.Villagers.VillagerId> _villagerIdLookup;
        private ComponentLookup<VillagerSocialFocus> _socialFocusLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<VillagerWorkCooldown>();
            state.RequireForUpdate<VillagerActionTelemetryState>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerCooldownPressureState>();
            state.RequireForUpdate<TimeState>();
            _villagerIdLookup = state.GetComponentLookup<Godgame.Villagers.VillagerId>(true);
            _socialFocusLookup = state.GetComponentLookup<VillagerSocialFocus>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!entityManager.HasBuffer<GodgameVillagerLivelinessRecord>(telemetryEntity))
            {
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var buffer = entityManager.GetBuffer<GodgameVillagerLivelinessRecord>(telemetryEntity);
            _villagerIdLookup.Update(ref state);
            _socialFocusLookup.Update(ref state);

            foreach (var (job, cooldown, actionState, goal, pressure, entity) in SystemAPI
                         .Query<RefRO<VillagerJobState>, RefRO<VillagerWorkCooldown>, RefRO<VillagerActionTelemetryState>, RefRO<VillagerGoalState>, RefRO<VillagerCooldownPressureState>>()
                         .WithEntityAccess())
            {
                var agentId = BuildAgentId(entity);
                var cooldownActive = cooldown.ValueRO.EndTick > tick;
                var cooldownRemaining = cooldownActive && cooldown.ValueRO.EndTick > tick
                    ? cooldown.ValueRO.EndTick - tick
                    : 0u;
                var leisureAction = ResolveLeisureAction(goal.ValueRO.CurrentGoal);
                var leisureTarget = ResolveLeisureTarget(entity, leisureAction);

                if (actionState.ValueRO.LastPhase == JobPhase.Idle && job.ValueRO.Phase == JobPhase.NavigateToNode)
                {
                    buffer.Add(new GodgameVillagerLivelinessRecord
                    {
                        Tick = tick,
                        AgentId = agentId,
                        Event = GodgameVillagerLivelinessEvent.StartedWork,
                        Target = job.ValueRO.Target,
                        CooldownMode = cooldown.ValueRO.Mode,
                        CooldownRemainingTicks = cooldownRemaining,
                        LeisureAction = leisureAction,
                        LeisureTarget = leisureTarget
                    });
                }

                if (cooldown.ValueRO.StartTick == tick && cooldownActive)
                {
                    buffer.Add(new GodgameVillagerLivelinessRecord
                    {
                        Tick = tick,
                        AgentId = agentId,
                        Event = GodgameVillagerLivelinessEvent.CooldownStart,
                        Target = Entity.Null,
                        CooldownMode = cooldown.ValueRO.Mode,
                        CooldownRemainingTicks = cooldownRemaining,
                        LeisureAction = leisureAction,
                        LeisureTarget = leisureTarget
                    });
                }

                if (cooldownActive && goal.ValueRO.LastGoalChangeTick == tick)
                {
                    var modeEvent = leisureAction == VillagerWorkCooldownMode.Socialize
                        ? GodgameVillagerLivelinessEvent.SocializeStart
                        : leisureAction == VillagerWorkCooldownMode.Wander
                            ? GodgameVillagerLivelinessEvent.WanderStart
                            : GodgameVillagerLivelinessEvent.CooldownStart;
                    if (modeEvent == GodgameVillagerLivelinessEvent.CooldownStart)
                    {
                        continue;
                    }

                    buffer.Add(new GodgameVillagerLivelinessRecord
                    {
                        Tick = tick,
                        AgentId = agentId,
                        Event = modeEvent,
                        Target = leisureTarget,
                        CooldownMode = cooldown.ValueRO.Mode,
                        CooldownRemainingTicks = cooldownRemaining,
                        LeisureAction = leisureAction,
                        LeisureTarget = leisureTarget
                    });
                }

                if (pressure.ValueRO.LastClearTick == tick &&
                    pressure.ValueRO.LastClearReason != VillagerCooldownClearReason.None)
                {
                    buffer.Add(new GodgameVillagerLivelinessRecord
                    {
                        Tick = tick,
                        AgentId = agentId,
                        Event = GodgameVillagerLivelinessEvent.CooldownCleared,
                        Target = Entity.Null,
                        CooldownMode = cooldown.ValueRO.Mode,
                        CooldownRemainingTicks = cooldownRemaining,
                        LeisureAction = leisureAction,
                        LeisureTarget = leisureTarget,
                        CooldownClearReason = pressure.ValueRO.LastClearReason
                    });
                }
            }
        }

        private FixedString64Bytes BuildAgentId(Entity entity)
        {
            if (_villagerIdLookup.HasComponent(entity))
            {
                return GodgameTelemetryStringHelpers.BuildAgentId(_villagerIdLookup[entity].Value);
            }

            return GodgameTelemetryStringHelpers.BuildEntityLabel(entity);
        }

        private static VillagerWorkCooldownMode ResolveLeisureAction(VillagerGoal goal)
        {
            return goal switch
            {
                VillagerGoal.Socialize => VillagerWorkCooldownMode.Socialize,
                VillagerGoal.Idle => VillagerWorkCooldownMode.Wander,
                _ => VillagerWorkCooldownMode.None
            };
        }

        private Entity ResolveLeisureTarget(Entity entity, VillagerWorkCooldownMode leisureAction)
        {
            if (leisureAction != VillagerWorkCooldownMode.Socialize)
            {
                return Entity.Null;
            }

            if (_socialFocusLookup.HasComponent(entity))
            {
                var focus = _socialFocusLookup[entity];
                if (focus.Target != Entity.Null)
                {
                    return focus.Target;
                }
            }

            return Entity.Null;
        }
    }
}
