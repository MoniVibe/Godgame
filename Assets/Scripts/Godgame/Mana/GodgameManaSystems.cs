using Godgame.Mana;
using Godgame.Modules;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Power;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems.Mana
{
    [BurstCompile]
    [UpdateInGroup(typeof(AISystemGroup))]
    public partial struct GodgameManaFocusCommandSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaFocusCommand>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (command, entity) in SystemAPI.Query<RefRO<ManaFocusCommand>>().WithEntityAccess())
            {
                if (SystemAPI.HasComponent<ManaFocus>(entity))
                {
                    var focus = SystemAPI.GetComponent<ManaFocus>(entity);
                    focus.Mode = command.ValueRO.Mode;
                    SystemAPI.SetComponent(entity, focus);
                }
                else
                {
                    ecb.AddComponent(entity, new ManaFocus { Mode = command.ValueRO.Mode });
                }

                ecb.RemoveComponent<ManaFocusCommand>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(AISystemGroup))]
    [UpdateAfter(typeof(GodgameManaFocusCommandSystem))]
    public partial struct GodgameManaFocusPresetSystem : ISystem
    {
        private ComponentLookup<ManaFocus> _focusLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaFocus>();
            state.RequireForUpdate<ManaModuleConfig>();
            _focusLookup = state.GetComponentLookup<ManaFocus>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _focusLookup.Update(ref state);

            foreach (var (moduleState, moduleConfig, hostRef) in SystemAPI
                         .Query<RefRW<ManaModuleState>, RefRO<ManaModuleConfig>, RefRO<ModuleHostReference>>())
            {
                if (!_focusLookup.HasComponent(hostRef.ValueRO.Host))
                {
                    continue;
                }

                var focus = _focusLookup[hostRef.ValueRO.Host];
                moduleState.ValueRW.State = ResolveState(focus.Mode, moduleConfig.ValueRO.Category);
            }
        }

        private static ManaAllocationState ResolveState(ManaFocusMode mode, ManaModuleCategory category)
        {
            return mode switch
            {
                ManaFocusMode.Balanced => ManaAllocationState.Normal,
                ManaFocusMode.Weapons => category switch
                {
                    ManaModuleCategory.Weapons => ManaAllocationState.Overcharged,
                    ManaModuleCategory.Shields => ManaAllocationState.Standby,
                    ManaModuleCategory.Sensors => ManaAllocationState.Standby,
                    _ => ManaAllocationState.Normal
                },
                ManaFocusMode.Defense => category switch
                {
                    ManaModuleCategory.Shields => ManaAllocationState.Overcharged,
                    ManaModuleCategory.Weapons => ManaAllocationState.Standby,
                    ManaModuleCategory.Mobility => ManaAllocationState.Standby,
                    _ => ManaAllocationState.Normal
                },
                ManaFocusMode.Mobility => category switch
                {
                    ManaModuleCategory.Mobility => ManaAllocationState.Overcharged,
                    ManaModuleCategory.Weapons => ManaAllocationState.Standby,
                    ManaModuleCategory.Shields => ManaAllocationState.Standby,
                    _ => ManaAllocationState.Normal
                },
                ManaFocusMode.Stealth => category switch
                {
                    ManaModuleCategory.Stealth => ManaAllocationState.Overcharged,
                    ManaModuleCategory.Sensors => ManaAllocationState.Overcharged,
                    ManaModuleCategory.Shields => ManaAllocationState.Disabled,
                    _ => ManaAllocationState.Standby
                },
                ManaFocusMode.Emergency => category switch
                {
                    ManaModuleCategory.Weapons => ManaAllocationState.Max,
                    _ => ManaAllocationState.Disabled
                },
                _ => ManaAllocationState.Normal
            };
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PowerSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Power.PowerAllocationSystem))]
    public partial struct GodgameManaModuleAllocationSystem : ISystem
    {
        private BufferLookup<VillagerLimb> _limbLookup;
        private ComponentLookup<PowerAllocationPercent> _allocationLookup;
        private ComponentLookup<PowerBurnoutSettings> _burnoutLookup;
        private ComponentLookup<ModuleData> _moduleLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ManaModuleState>();
            state.RequireForUpdate<PowerAllocationPercent>();
            state.RequireForUpdate<TimeState>();
            _limbLookup = state.GetBufferLookup<VillagerLimb>(true);
            _allocationLookup = state.GetComponentLookup<PowerAllocationPercent>(false);
            _burnoutLookup = state.GetComponentLookup<PowerBurnoutSettings>(false);
            _moduleLookup = state.GetComponentLookup<ModuleData>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var deltaTime = timeState.FixedDeltaTime;
            _limbLookup.Update(ref state);
            _allocationLookup.Update(ref state);
            _burnoutLookup.Update(ref state);
            _moduleLookup.Update(ref state);

            foreach (var (moduleState, moduleConfig, hostRef, entity) in SystemAPI
                         .Query<RefRW<ManaModuleState>, RefRO<ManaModuleConfig>, RefRO<ModuleHostReference>>()
                         .WithEntityAccess())
            {
                if (!_allocationLookup.HasComponent(entity))
                {
                    continue;
                }

                var desiredPercent = ManaAllocationUtility.ToPercent(moduleState.ValueRO.State);
                var maxPercent = desiredPercent;
                var riskMultiplier = 1f;

                if (_moduleLookup.HasComponent(entity))
                {
                    var moduleData = _moduleLookup[entity];
                    if (moduleData.Status == ModuleStatus.Offline ||
                        moduleData.Status == ModuleStatus.Inactive ||
                        moduleData.Status == ModuleStatus.Refit)
                    {
                        desiredPercent = ManaAllocationUtility.DisabledPercent;
                        maxPercent = ManaAllocationUtility.DisabledPercent;
                    }
                    else if (moduleData.Status == ModuleStatus.Damaged)
                    {
                        maxPercent = math.min(maxPercent, ManaAllocationUtility.NormalPercent);
                        riskMultiplier *= 1.25f;
                    }
                }

                if (moduleConfig.ValueRO.LimbId.Length > 0 && _limbLookup.HasBuffer(hostRef.ValueRO.Host))
                {
                    var limbs = _limbLookup[hostRef.ValueRO.Host];
                    var limbId = moduleConfig.ValueRO.LimbId;
                    for (var i = 0; i < limbs.Length; i++)
                    {
                        var limb = limbs[i];
                        if (!limb.LimbId.Equals(limbId))
                        {
                            continue;
                        }

                        const byte lostFlag = 0x01;
                        const byte crippledFlag = 0x02;
                        if ((limb.InjuryFlags & lostFlag) != 0)
                        {
                            desiredPercent = ManaAllocationUtility.DisabledPercent;
                            maxPercent = ManaAllocationUtility.DisabledPercent;
                            riskMultiplier *= 1.5f;
                        }
                        else if ((limb.InjuryFlags & crippledFlag) != 0 || limb.Health < 25)
                        {
                            maxPercent = math.min(maxPercent, ManaAllocationUtility.StandbyPercent);
                            riskMultiplier *= 1.4f;
                        }
                        else if (limb.Health < 50)
                        {
                            maxPercent = math.min(maxPercent, ManaAllocationUtility.NormalPercent);
                            riskMultiplier *= 1.2f;
                        }

                        break;
                    }
                }

                var targetPercent = math.clamp(math.min(desiredPercent, maxPercent), 0f, 250f);
                var currentPercent = _allocationLookup[entity].Value;
                var finalPercent = MoveTowardPercent(currentPercent, targetPercent, moduleConfig.ValueRO, deltaTime);
                _allocationLookup[entity] = new PowerAllocationPercent { Value = finalPercent };

                if (_burnoutLookup.HasComponent(entity))
                {
                    var burnout = _burnoutLookup[entity];
                    var baseRisk = math.max(0f, moduleConfig.ValueRO.BaseBurnoutRiskMultiplier);
                    burnout.RiskMultiplier = baseRisk * riskMultiplier;
                    _burnoutLookup[entity] = burnout;
                }
            }
        }

        private static float MoveTowardPercent(float current, float target, in ManaModuleConfig config, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return target;
            }

            var rampUp = math.max(0f, config.RampUpPerSecond);
            var rampDown = math.max(0f, config.RampDownPerSecond);
            var step = target > current ? rampUp : rampDown;
            if (step <= 0f)
            {
                return target;
            }

            var maxDelta = step * deltaTime;
            var delta = target - current;
            delta = math.clamp(delta, -maxDelta, maxDelta);
            return math.clamp(current + delta, 0f, 250f);
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PowerSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Power.PowerBudgetSystem))]
    public partial struct GodgameManaModuleDomainSyncSystem : ISystem
    {
        private ComponentLookup<PowerDomainRef> _domainLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ModuleHostReference>();
            _domainLookup = state.GetComponentLookup<PowerDomainRef>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _domainLookup.Update(ref state);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (hostRef, entity) in SystemAPI
                         .Query<RefRO<ModuleHostReference>>()
                         .WithAll<PowerConsumer>()
                         .WithEntityAccess())
            {
                if (_domainLookup.HasComponent(entity))
                {
                    var domain = _domainLookup[entity];
                    if (domain.Value != hostRef.ValueRO.Host)
                    {
                        domain.Value = hostRef.ValueRO.Host;
                        _domainLookup[entity] = domain;
                    }
                }
                else
                {
                    ecb.AddComponent(entity, new PowerDomainRef { Value = hostRef.ValueRO.Host });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
