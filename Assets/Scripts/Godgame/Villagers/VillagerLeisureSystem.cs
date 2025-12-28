using Godgame.Scenario;
using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Villagers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Routes villagers into leisure goals while a work cooldown is active.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    [UpdateAfter(typeof(VillagerWorkCooldownSystem))]
    [UpdateBefore(typeof(VillagerSocialFocusSystem))]
    public partial struct VillagerLeisureSystem : ISystem
    {
        private EntityQuery _villageQuery;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<Navigation> _navLookup;
        private ComponentLookup<VillagerThreatState> _threatLookup;
        private ComponentLookup<VillagerCrowdingState> _crowdingLookup;
        private ComponentLookup<Village> _villageLookup;
        private ComponentLookup<VillageNeedAwareness> _villageNeedLookup;
        private ComponentLookup<VillageResourceSummary> _villageResourceLookup;
        private ComponentLookup<VillagerOutlook> _outlookLookup;
        private ComponentLookup<VillagerArchetypeResolved> _archetypeLookup;
        private BufferLookup<VillagerCooldownOutlookRule> _cooldownOutlookLookup;
        private BufferLookup<VillagerCooldownArchetypeModifier> _cooldownArchetypeLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerWorkCooldown>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerNeedMovementState>();
            state.RequireForUpdate<VillagerNeedState>();
            state.RequireForUpdate<VillagerLeisureState>();
            state.RequireForUpdate<VillagerSocialFocus>();
            state.RequireForUpdate<VillagerCooldownPressureState>();
            state.RequireForUpdate<VillagerScheduleConfig>();

            _villageQuery = SystemAPI.QueryBuilder()
                .WithAll<Village>()
                .Build();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _navLookup = state.GetComponentLookup<Navigation>(true);
            _threatLookup = state.GetComponentLookup<VillagerThreatState>(true);
            _crowdingLookup = state.GetComponentLookup<VillagerCrowdingState>(true);
            _villageLookup = state.GetComponentLookup<Village>(true);
            _villageNeedLookup = state.GetComponentLookup<VillageNeedAwareness>(true);
            _villageResourceLookup = state.GetComponentLookup<VillageResourceSummary>(true);
            _outlookLookup = state.GetComponentLookup<VillagerOutlook>(true);
            _archetypeLookup = state.GetComponentLookup<VillagerArchetypeResolved>(true);
            _cooldownOutlookLookup = state.GetBufferLookup<VillagerCooldownOutlookRule>(true);
            _cooldownArchetypeLookup = state.GetBufferLookup<VillagerCooldownArchetypeModifier>(true);
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

            var tick = timeState.Tick;
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
            var cooldownProfile = SystemAPI.TryGetSingleton<VillagerCooldownProfile>(out var profileValue)
                ? profileValue
                : VillagerCooldownProfile.Default;
            var baseWanderRadius = math.max(0f, schedule.NeedWanderRadius);
            var baseSocialRadius = math.max(baseWanderRadius, schedule.NeedSocialRadius);
            var wanderRadius = baseWanderRadius * math.max(0f, cooldownProfile.LeisureWanderRadiusMultiplier);
            var socialRadius = baseSocialRadius * math.max(0f, cooldownProfile.LeisureSocialRadiusMultiplier);
            var tidyRadius = baseWanderRadius * math.max(0f, cooldownProfile.LeisureTidyRadiusMultiplier);
            var observeRadius = baseWanderRadius * math.max(0f, cooldownProfile.LeisureObserveRadiusMultiplier);
            var arrivalDistance = math.max(0.05f, cooldownProfile.LeisureArrivalDistance);
            var socialDistanceLimit = socialRadius * math.max(1f, cooldownProfile.LeisureSocialTargetDistanceMultiplier);

            _transformLookup.Update(ref state);
            _navLookup.Update(ref state);
            _threatLookup.Update(ref state);
            _crowdingLookup.Update(ref state);
            _villageLookup.Update(ref state);
            _villageNeedLookup.Update(ref state);
            _villageResourceLookup.Update(ref state);
            _outlookLookup.Update(ref state);
            _archetypeLookup.Update(ref state);
            _cooldownOutlookLookup.Update(ref state);
            _cooldownArchetypeLookup.Update(ref state);

            var villageEntities = _villageQuery.ToEntityArray(Allocator.Temp);

            var hasSettlement = TryGetSettlementRuntime(ref state, _transformLookup, out var settlementRuntime);
            var cooldownProfileEntity = SystemAPI.HasSingleton<VillagerCooldownProfile>()
                ? SystemAPI.GetSingletonEntity<VillagerCooldownProfile>()
                : Entity.Null;
            var hasOutlookRules = cooldownProfileEntity != Entity.Null && _cooldownOutlookLookup.HasBuffer(cooldownProfileEntity);
            var hasArchetypeRules = cooldownProfileEntity != Entity.Null && _cooldownArchetypeLookup.HasBuffer(cooldownProfileEntity);
            var outlookRules = hasOutlookRules ? _cooldownOutlookLookup[cooldownProfileEntity] : default;
            var archetypeRules = hasArchetypeRules ? _cooldownArchetypeLookup[cooldownProfileEntity] : default;

            foreach (var (cooldown, goal, movement, leisure, needs, social, pressure, entity) in SystemAPI
                         .Query<RefRW<VillagerWorkCooldown>, RefRW<VillagerGoalState>, RefRW<VillagerNeedMovementState>, RefRW<VillagerLeisureState>, RefRO<VillagerNeedState>, RefRW<VillagerSocialFocus>, RefRW<VillagerCooldownPressureState>>()
                         .WithEntityAccess())
            {
                var cooldownValue = cooldown.ValueRO;
                var cooldownActive = cooldownValue.EndTick != 0 && tick < cooldownValue.EndTick;
                if (!cooldownActive)
                {
                    if (leisure.ValueRO.CooldownStartTick != 0 || leisure.ValueRO.CadenceTicks != 0)
                    {
                        leisure.ValueRW = default;
                    }
                    if (pressure.ValueRO.ActiveMask != VillagerCooldownPressureMask.None)
                    {
                        pressure.ValueRW.ActiveMask = VillagerCooldownPressureMask.None;
                    }
                    continue;
                }

                var pressureEnterScale = ResolveOutlookPressureEnterScale(entity, hasOutlookRules, outlookRules, _outlookLookup)
                                         * ResolveArchetypePressureEnterScale(entity, hasArchetypeRules, archetypeRules, _archetypeLookup);
                var pressureExitScale = ResolveOutlookPressureExitScale(entity, hasOutlookRules, outlookRules, _outlookLookup)
                                        * ResolveArchetypePressureExitScale(entity, hasArchetypeRules, archetypeRules, _archetypeLookup);
                var pressureReason = EvaluatePressure(entity, needs.ValueRO, cooldownProfile, pressureEnterScale, pressureExitScale,
                    villageEntities, _villageLookup, _villageNeedLookup, _villageResourceLookup, _transformLookup, _threatLookup,
                    ref pressure.ValueRW);
                if (pressureReason != VillagerCooldownClearReason.None)
                {
                    var changed = ApplyPressureCooldown(ref cooldown.ValueRW, tick, cooldownProfile, out var cleared);
                    if (changed)
                    {
                        cooldownValue = cooldown.ValueRO;
                    }

                    if (cleared)
                    {
                        pressure.ValueRW.LastClearReason = pressureReason;
                        pressure.ValueRW.LastClearTick = tick;
                        leisure.ValueRW = default;
                        pressure.ValueRW.ActiveMask = VillagerCooldownPressureMask.None;
                        continue;
                    }

                    if (cooldownValue.EndTick == 0 || tick >= cooldownValue.EndTick)
                    {
                        leisure.ValueRW = default;
                        pressure.ValueRW.ActiveMask = VillagerCooldownPressureMask.None;
                        continue;
                    }
                }

                var currentGoal = goal.ValueRO.CurrentGoal;
                if (currentGoal != VillagerGoal.Work &&
                    currentGoal != VillagerGoal.Idle &&
                    currentGoal != VillagerGoal.Socialize)
                {
                    continue;
                }

                if (cooldownValue.StartTick == 0)
                {
                    cooldownValue.StartTick = tick;
                    cooldown.ValueRW.StartTick = tick;
                }

                var mode = cooldownValue.Mode;
                if (mode == VillagerWorkCooldownMode.None)
                {
                    mode = VillagerWorkCooldownMode.Wander;
                    cooldown.ValueRW.Mode = mode;
                    cooldownValue.Mode = mode;
                }

                var desiredGoal = mode == VillagerWorkCooldownMode.Socialize
                    ? VillagerGoal.Socialize
                    : VillagerGoal.Idle;

                if (currentGoal != desiredGoal)
                {
                    goal.ValueRW.PreviousGoal = currentGoal;
                    goal.ValueRW.CurrentGoal = desiredGoal;
                    goal.ValueRW.LastGoalChangeTick = tick;
                    goal.ValueRW.CurrentGoalUrgency = 0f;
                    currentGoal = desiredGoal;
                }

                var cadenceScale = ResolveOutlookCadenceScale(entity, hasOutlookRules, outlookRules, _outlookLookup)
                                   * ResolveArchetypeCadenceScale(entity, hasArchetypeRules, archetypeRules, _archetypeLookup);
                var cadenceTicks = EnsureCadenceTicks(ref leisure.ValueRW, cooldownValue.StartTick, entity, cooldownProfile, cadenceScale);
                var episodeIndex = ResolveEpisodeIndex(cooldownValue.StartTick, tick, cadenceTicks);
                var episodeStartTick = cooldownValue.StartTick + episodeIndex * math.max(1u, cadenceTicks);

                var shouldReroll = false;
                var hardInvalidation = false;
                if (currentGoal == VillagerGoal.Idle)
                {
                    shouldReroll = ReachedDestination(entity, _navLookup, _transformLookup, arrivalDistance);
                }
                else if (currentGoal == VillagerGoal.Socialize)
                {
                    hardInvalidation = SocialTargetInvalid(entity, social.ValueRO, _transformLookup, socialDistanceLimit);
                    shouldReroll = hardInvalidation;
                }

                if (!shouldReroll && cooldownProfile.LeisureCrowdingPressureThreshold > 0f && _crowdingLookup.HasComponent(entity))
                {
                    var crowdingPressure = _crowdingLookup[entity].Pressure;
                    shouldReroll = crowdingPressure >= cooldownProfile.LeisureCrowdingPressureThreshold;
                }

                if (!shouldReroll && leisure.ValueRO.ActionTarget != Entity.Null)
                {
                    hardInvalidation = !_transformLookup.HasComponent(leisure.ValueRO.ActionTarget);
                    shouldReroll = hardInvalidation;
                }

                var minDwellTicks = math.min(math.max(0u, cooldownProfile.LeisureMinDwellTicks), cadenceTicks);
                if (shouldReroll && !hardInvalidation && minDwellTicks > 0u && tick < episodeStartTick + minDwellTicks)
                {
                    shouldReroll = false;
                }

                if (leisure.ValueRO.CooldownStartTick != cooldownValue.StartTick ||
                    leisure.ValueRO.EpisodeIndex != episodeIndex)
                {
                    leisure.ValueRW.CooldownStartTick = cooldownValue.StartTick;
                    leisure.ValueRW.EpisodeIndex = episodeIndex;
                    leisure.ValueRW.RerollCount = 0;
                    ApplyLeisureEpisode(ref leisure.ValueRW, ref movement.ValueRW, ref social.ValueRW, cooldownValue,
                        cadenceTicks, episodeIndex, entity, cooldownProfile, wanderRadius, socialRadius, tidyRadius, observeRadius,
                        hasSettlement, settlementRuntime,
                        villageEntities, _villageLookup, _transformLookup);
                }
                else if (shouldReroll)
                {
                    leisure.ValueRW.RerollCount = (byte)math.min(255, leisure.ValueRO.RerollCount + 1);
                    ApplyLeisureEpisode(ref leisure.ValueRW, ref movement.ValueRW, ref social.ValueRW, cooldownValue,
                        cadenceTicks, episodeIndex, entity, cooldownProfile, wanderRadius, socialRadius, tidyRadius, observeRadius,
                        hasSettlement, settlementRuntime,
                        villageEntities, _villageLookup, _transformLookup);
                }
            }

            villageEntities.Dispose();
        }

        private static VillagerCooldownClearReason EvaluatePressure(Entity entity, in VillagerNeedState needs, in VillagerCooldownProfile profile,
            float pressureEnterScale, float pressureExitScale,
            NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, ComponentLookup<VillageNeedAwareness> villageNeedLookup,
            ComponentLookup<VillageResourceSummary> villageResourceLookup, ComponentLookup<LocalTransform> transformLookup,
            ComponentLookup<VillagerThreatState> threatLookup, ref VillagerCooldownPressureState pressureState)
        {
            var workActive = (pressureState.ActiveMask & VillagerCooldownPressureMask.Work) != 0;
            var threatActive = (pressureState.ActiveMask & VillagerCooldownPressureMask.Threat) != 0;
            var foodActive = (pressureState.ActiveMask & VillagerCooldownPressureMask.Food) != 0;

            var enterScale = math.max(0f, pressureEnterScale);
            var exitScale = math.max(0f, pressureExitScale);
            var workEnter = math.clamp(profile.PressureWorkUrgencyThreshold * enterScale, 0f, 1f);
            var workExit = ResolveExitBelow(profile.PressureWorkUrgencyExitThreshold * exitScale, workEnter);
            var threatEnter = math.clamp(profile.PressureThreatUrgencyThreshold * enterScale, 0f, 1f);
            var threatExit = ResolveExitBelow(profile.PressureThreatUrgencyExitThreshold * exitScale, threatEnter);
            var foodEnter = math.max(0f, profile.PressureFoodRatioThreshold * enterScale);
            var foodExit = ResolveExitAbove(profile.PressureFoodRatioExitThreshold * exitScale, foodEnter);

            var villageEntity = Entity.Null;
            if ((workEnter > 0f || foodEnter > 0f || foodExit > 0f) && villages.Length > 0 && transformLookup.HasComponent(entity))
            {
                var pos = transformLookup[entity].Position;
                var nearest = ResolveNearestVillageIndex(villages, villageLookup, pos);
                if (nearest >= 0)
                {
                    villageEntity = villages[nearest];
                }
            }

            if (workEnter > 0f)
            {
                var workSignal = math.saturate(needs.WorkUrgency);
                if (villageEntity != Entity.Null && villageNeedLookup.HasComponent(villageEntity))
                {
                    workSignal = math.max(workSignal, math.saturate(villageNeedLookup[villageEntity].Work));
                }

                if (!workActive && workSignal >= workEnter)
                {
                    workActive = true;
                }
                else if (workActive && workSignal <= workExit)
                {
                    workActive = false;
                }
            }
            else
            {
                workActive = false;
            }

            if (threatEnter > 0f && threatLookup.HasComponent(entity))
            {
                var urgency = math.max(0f, threatLookup[entity].Urgency);
                if (!threatActive && urgency >= threatEnter)
                {
                    threatActive = true;
                }
                else if (threatActive && urgency <= threatExit)
                {
                    threatActive = false;
                }
            }
            else
            {
                threatActive = false;
            }

            if ((foodEnter > 0f || foodExit > 0f) && villageEntity != Entity.Null && villageResourceLookup.HasComponent(villageEntity))
            {
                var resources = villageResourceLookup[villageEntity];
                if (resources.FoodUpkeep > 0.01f)
                {
                    var ratio = resources.FoodStored / resources.FoodUpkeep;
                    if (!foodActive && ratio <= foodEnter)
                    {
                        foodActive = true;
                    }
                    else if (foodActive && ratio >= foodExit)
                    {
                        foodActive = false;
                    }
                }
                else
                {
                    foodActive = false;
                }
            }
            else
            {
                foodActive = false;
            }

            pressureState.ActiveMask = ResolvePressureMask(workActive, threatActive, foodActive);

            if (threatActive)
            {
                return VillagerCooldownClearReason.Threat;
            }

            if (foodActive)
            {
                return VillagerCooldownClearReason.Food;
            }

            if (workActive)
            {
                return VillagerCooldownClearReason.Pressure;
            }

            return VillagerCooldownClearReason.None;
        }

        private static VillagerCooldownPressureMask ResolvePressureMask(bool workActive, bool threatActive, bool foodActive)
        {
            var mask = VillagerCooldownPressureMask.None;
            if (workActive)
            {
                mask |= VillagerCooldownPressureMask.Work;
            }

            if (threatActive)
            {
                mask |= VillagerCooldownPressureMask.Threat;
            }

            if (foodActive)
            {
                mask |= VillagerCooldownPressureMask.Food;
            }

            return mask;
        }

        private static float ResolveExitBelow(float exitThreshold, float enterThreshold)
        {
            if (enterThreshold <= 0f)
            {
                return 0f;
            }

            if (exitThreshold <= 0f)
            {
                return enterThreshold;
            }

            return math.min(exitThreshold, enterThreshold);
        }

        private static float ResolveExitAbove(float exitThreshold, float enterThreshold)
        {
            if (enterThreshold <= 0f)
            {
                return 0f;
            }

            if (exitThreshold <= 0f)
            {
                return enterThreshold;
            }

            return math.max(exitThreshold, enterThreshold);
        }

        private static float ResolveOutlookPressureEnterScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownOutlookRule> rules, ComponentLookup<VillagerOutlook> outlookLookup)
        {
            if (!hasRules || !outlookLookup.HasComponent(entity))
            {
                return 1f;
            }

            var outlook = outlookLookup[entity];
            var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
            var scale = 1f;
            for (var i = 0; i < slotCount; i++)
            {
                var typeId = outlook.OutlookTypes[i];
                if (!TryGetOutlookRule(typeId, rules, out var rule))
                {
                    continue;
                }

                var value01 = math.abs(outlook.OutlookValues[i]) / 100f;
                var target = rule.PressureEnterScale <= 0f ? 1f : rule.PressureEnterScale;
                scale *= math.lerp(1f, target, value01);
            }

            return math.max(0.1f, scale);
        }

        private static float ResolveOutlookPressureExitScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownOutlookRule> rules, ComponentLookup<VillagerOutlook> outlookLookup)
        {
            if (!hasRules || !outlookLookup.HasComponent(entity))
            {
                return 1f;
            }

            var outlook = outlookLookup[entity];
            var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
            var scale = 1f;
            for (var i = 0; i < slotCount; i++)
            {
                var typeId = outlook.OutlookTypes[i];
                if (!TryGetOutlookRule(typeId, rules, out var rule))
                {
                    continue;
                }

                var value01 = math.abs(outlook.OutlookValues[i]) / 100f;
                var target = rule.PressureExitScale <= 0f ? 1f : rule.PressureExitScale;
                scale *= math.lerp(1f, target, value01);
            }

            return math.max(0.1f, scale);
        }

        private static float ResolveOutlookCadenceScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownOutlookRule> rules, ComponentLookup<VillagerOutlook> outlookLookup)
        {
            if (!hasRules || !outlookLookup.HasComponent(entity))
            {
                return 1f;
            }

            var outlook = outlookLookup[entity];
            var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
            var scale = 1f;
            for (var i = 0; i < slotCount; i++)
            {
                var typeId = outlook.OutlookTypes[i];
                if (!TryGetOutlookRule(typeId, rules, out var rule))
                {
                    continue;
                }

                var value01 = math.abs(outlook.OutlookValues[i]) / 100f;
                var target = rule.CadenceScale <= 0f ? 1f : rule.CadenceScale;
                scale *= math.lerp(1f, target, value01);
            }

            return math.max(0.1f, scale);
        }

        private static float ResolveArchetypePressureEnterScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, ComponentLookup<VillagerArchetypeResolved> archetypeLookup)
        {
            if (!hasRules || !archetypeLookup.HasComponent(entity))
            {
                return 1f;
            }

            var data = archetypeLookup[entity].Data;
            var archetypeName = data.ArchetypeName;
            if (archetypeName.IsEmpty)
            {
                return 1f;
            }

            if (!TryGetArchetypeModifier(archetypeName, modifiers, out var modifier))
            {
                return 1f;
            }

            return modifier.PressureEnterScale <= 0f ? 1f : modifier.PressureEnterScale;
        }

        private static float ResolveArchetypePressureExitScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, ComponentLookup<VillagerArchetypeResolved> archetypeLookup)
        {
            if (!hasRules || !archetypeLookup.HasComponent(entity))
            {
                return 1f;
            }

            var data = archetypeLookup[entity].Data;
            var archetypeName = data.ArchetypeName;
            if (archetypeName.IsEmpty)
            {
                return 1f;
            }

            if (!TryGetArchetypeModifier(archetypeName, modifiers, out var modifier))
            {
                return 1f;
            }

            return modifier.PressureExitScale <= 0f ? 1f : modifier.PressureExitScale;
        }

        private static float ResolveArchetypeCadenceScale(Entity entity, bool hasRules,
            DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, ComponentLookup<VillagerArchetypeResolved> archetypeLookup)
        {
            if (!hasRules || !archetypeLookup.HasComponent(entity))
            {
                return 1f;
            }

            var data = archetypeLookup[entity].Data;
            var archetypeName = data.ArchetypeName;
            if (archetypeName.IsEmpty)
            {
                return 1f;
            }

            if (!TryGetArchetypeModifier(archetypeName, modifiers, out var modifier))
            {
                return 1f;
            }

            return modifier.CadenceScale <= 0f ? 1f : modifier.CadenceScale;
        }

        private static bool TryGetOutlookRule(byte outlookType, in DynamicBuffer<VillagerCooldownOutlookRule> rules,
            out VillagerCooldownOutlookRule rule)
        {
            for (var i = 0; i < rules.Length; i++)
            {
                if (rules[i].OutlookType == outlookType)
                {
                    rule = rules[i];
                    return true;
                }
            }

            rule = default;
            return false;
        }

        private static bool TryGetArchetypeModifier(in FixedString64Bytes archetypeName,
            in DynamicBuffer<VillagerCooldownArchetypeModifier> modifiers, out VillagerCooldownArchetypeModifier modifier)
        {
            for (var i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i].ArchetypeName.Equals(archetypeName))
                {
                    modifier = modifiers[i];
                    return true;
                }
            }

            modifier = default;
            return false;
        }

        private static bool ApplyPressureCooldown(ref VillagerWorkCooldown cooldown, uint tick, in VillagerCooldownProfile profile, out bool cleared)
        {
            cleared = false;
            if (profile.PressureCooldownMaxRemainingTicks == 0)
            {
                if (cooldown.EndTick == 0)
                {
                    return false;
                }

                cooldown = default;
                cleared = true;
                return true;
            }

            var maxRemaining = profile.PressureCooldownMaxRemainingTicks;
            var desiredEnd = tick + maxRemaining;
            if (desiredEnd < cooldown.EndTick)
            {
                cooldown.EndTick = desiredEnd;
                return true;
            }

            return false;
        }

        private static uint EnsureCadenceTicks(ref VillagerLeisureState leisureState, uint cooldownStartTick, Entity entity,
            in VillagerCooldownProfile profile, float cadenceScale)
        {
            if (leisureState.CooldownStartTick == cooldownStartTick && leisureState.CadenceTicks > 0)
            {
                return leisureState.CadenceTicks;
            }

            var cadence = ResolveCadenceTicks(profile, entity, cooldownStartTick, cadenceScale);
            leisureState.CooldownStartTick = cooldownStartTick;
            leisureState.CadenceTicks = cadence;
            leisureState.EpisodeIndex = 0;
            leisureState.RerollCount = 0;
            return cadence;
        }

        private static uint ResolveCadenceTicks(in VillagerCooldownProfile profile, Entity entity, uint cooldownStartTick, float cadenceScale)
        {
            var scale = math.max(0.1f, cadenceScale);
            var min = math.max(1f, profile.LeisureCadenceMinTicks * scale);
            var max = math.max(min, profile.LeisureCadenceMaxTicks * scale);
            var minTicks = (uint)math.round(min);
            var maxTicks = (uint)math.round(max);
            maxTicks = math.max(minTicks, maxTicks);
            if (maxTicks == minTicks)
            {
                return math.max(1u, minTicks);
            }

            var seed = math.hash(new uint3((uint)(entity.Index + 5), cooldownStartTick + 11u, 0x9e37u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var range = maxTicks - minTicks;
            var sample = range == 0 ? 0u : random.NextUInt() % (range + 1u);
            return minTicks + sample;
        }

        private static uint ResolveEpisodeIndex(uint cooldownStartTick, uint tick, uint cadenceTicks)
        {
            if (tick <= cooldownStartTick || cadenceTicks == 0)
            {
                return 0;
            }

            return (tick - cooldownStartTick) / cadenceTicks;
        }

        private static bool ReachedDestination(Entity entity, ComponentLookup<Navigation> navLookup, ComponentLookup<LocalTransform> transformLookup,
            float arrivalDistance)
        {
            if (!navLookup.HasComponent(entity) || !transformLookup.HasComponent(entity))
            {
                return false;
            }

            var destination = navLookup[entity].Destination;
            var pos = transformLookup[entity].Position;
            var distanceSq = math.distancesq(destination, pos);
            var threshold = math.max(0.05f, arrivalDistance);
            return distanceSq <= threshold * threshold;
        }

        private static bool SocialTargetInvalid(Entity entity, in VillagerSocialFocus social, ComponentLookup<LocalTransform> transformLookup,
            float distanceLimit)
        {
            if (social.Target == Entity.Null)
            {
                return true;
            }

            if (!transformLookup.HasComponent(entity) || !transformLookup.HasComponent(social.Target))
            {
                return true;
            }

            if (distanceLimit <= 0f)
            {
                return false;
            }

            var origin = transformLookup[entity].Position;
            var target = transformLookup[social.Target].Position;
            var distanceSq = math.distancesq(origin, target);
            return distanceSq > distanceLimit * distanceLimit;
        }

        private static void ApplyLeisureEpisode(ref VillagerLeisureState leisureState, ref VillagerNeedMovementState movement,
            ref VillagerSocialFocus social, in VillagerWorkCooldown cooldown, uint cadenceTicks,
            uint episodeIndex, Entity entity, in VillagerCooldownProfile profile, float wanderRadius, float socialRadius,
            float tidyRadius, float observeRadius, bool hasSettlement, in SettlementRuntime settlementRuntime,
            NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, ComponentLookup<LocalTransform> transformLookup)
        {
            var seed = math.hash(new uint4((uint)(entity.Index + 1), cooldown.StartTick, episodeIndex, leisureState.RerollCount));
            var nextEpisodeTick = cooldown.StartTick + (episodeIndex + 1u) * math.max(1u, cadenceTicks);
            var action = cooldown.Mode == VillagerWorkCooldownMode.Socialize
                ? VillagerLeisureAction.Socialize
                : ResolveLeisureAction(seed, profile);
            var actionTarget = Entity.Null;
            var offsetRadius = wanderRadius;

            if (action == VillagerLeisureAction.Tidy)
            {
                actionTarget = ResolveTidyTarget(entity, hasSettlement, settlementRuntime, villages, villageLookup, transformLookup);
                offsetRadius = tidyRadius;
            }
            else if (action == VillagerLeisureAction.Observe)
            {
                actionTarget = ResolveObserveTarget(seed, entity, hasSettlement, settlementRuntime, villages, villageLookup, transformLookup);
                offsetRadius = observeRadius;
            }

            if (cooldown.Mode == VillagerWorkCooldownMode.Socialize)
            {
                var hubTarget = ResolveLeisureHub(entity, hasSettlement, settlementRuntime, villages, villageLookup, transformLookup);
                actionTarget = hubTarget != Entity.Null ? hubTarget : entity;
                social.Target = actionTarget;
                social.NextPickTick = nextEpisodeTick;
            }
            else
            {
                social.Target = Entity.Null;
                social.NextPickTick = 0;
            }

            if (actionTarget == Entity.Null && action != VillagerLeisureAction.Socialize)
            {
                action = VillagerLeisureAction.Wander;
                offsetRadius = wanderRadius;
            }

            var offsetSeed = math.hash(new uint4(seed ^ 0x93a2u, (uint)action, cooldown.StartTick, episodeIndex));
            var offset = ResolveLeisureOffset(offsetSeed, action == VillagerLeisureAction.Socialize ? socialRadius : offsetRadius);

            movement.AnchorOffset = offset;
            movement.LingerSeconds = 0f;
            movement.NextRepathTick = nextEpisodeTick;
            leisureState.Action = action;
            leisureState.ActionTarget = actionTarget;
        }

        private static VillagerLeisureAction ResolveLeisureAction(uint seed, in VillagerCooldownProfile profile)
        {
            var wanderWeight = math.max(0f, profile.BaseWanderWeight);
            var tidyWeight = math.max(0f, profile.LeisureTidyWeight);
            var observeWeight = math.max(0f, profile.LeisureObserveWeight);
            var total = wanderWeight + tidyWeight + observeWeight;
            if (total <= 0f)
            {
                return VillagerLeisureAction.Wander;
            }

            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var pick = random.NextFloat(0f, total);
            if (pick < wanderWeight)
            {
                return VillagerLeisureAction.Wander;
            }

            if (pick < wanderWeight + tidyWeight)
            {
                return VillagerLeisureAction.Tidy;
            }

            return VillagerLeisureAction.Observe;
        }

        private static Entity ResolveTidyTarget(Entity entity, bool hasSettlement, in SettlementRuntime settlementRuntime,
            NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, ComponentLookup<LocalTransform> transformLookup)
        {
            if (hasSettlement)
            {
                if (settlementRuntime.StorehouseInstance != Entity.Null &&
                    transformLookup.HasComponent(settlementRuntime.StorehouseInstance))
                {
                    return settlementRuntime.StorehouseInstance;
                }

                if (settlementRuntime.VillageCenterInstance != Entity.Null &&
                    transformLookup.HasComponent(settlementRuntime.VillageCenterInstance))
                {
                    return settlementRuntime.VillageCenterInstance;
                }
            }

            return ResolveLeisureHub(entity, hasSettlement, settlementRuntime, villages, villageLookup, transformLookup);
        }

        private static Entity ResolveObserveTarget(uint seed, Entity entity, bool hasSettlement, in SettlementRuntime settlementRuntime,
            NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, ComponentLookup<LocalTransform> transformLookup)
        {
            Entity candidate0 = Entity.Null;
            Entity candidate1 = Entity.Null;
            Entity candidate2 = Entity.Null;
            Entity candidate3 = Entity.Null;
            var count = 0;

            if (hasSettlement)
            {
                TryAddCandidate(settlementRuntime.VillageCenterInstance, transformLookup, ref candidate0, ref candidate1, ref candidate2, ref candidate3, ref count);
                TryAddCandidate(settlementRuntime.StorehouseInstance, transformLookup, ref candidate0, ref candidate1, ref candidate2, ref candidate3, ref count);
                TryAddCandidate(settlementRuntime.HousingInstance, transformLookup, ref candidate0, ref candidate1, ref candidate2, ref candidate3, ref count);
                TryAddCandidate(settlementRuntime.WorshipInstance, transformLookup, ref candidate0, ref candidate1, ref candidate2, ref candidate3, ref count);
            }

            if (count == 0)
            {
                return ResolveLeisureHub(entity, hasSettlement, settlementRuntime, villages, villageLookup, transformLookup);
            }

            var pick = (int)(seed % (uint)count);
            return pick switch
            {
                0 => candidate0,
                1 => candidate1,
                2 => candidate2,
                _ => candidate3
            };
        }

        private static void TryAddCandidate(Entity candidate, ComponentLookup<LocalTransform> transformLookup,
            ref Entity candidate0, ref Entity candidate1, ref Entity candidate2, ref Entity candidate3, ref int count)
        {
            if (candidate == Entity.Null || !transformLookup.HasComponent(candidate))
            {
                return;
            }

            switch (count)
            {
                case 0:
                    candidate0 = candidate;
                    break;
                case 1:
                    candidate1 = candidate;
                    break;
                case 2:
                    candidate2 = candidate;
                    break;
                case 3:
                    candidate3 = candidate;
                    break;
                default:
                    return;
            }

            count++;
        }

        private static float3 ResolveLeisureOffset(uint seed, float radius)
        {
            if (radius <= 0f)
            {
                return float3.zero;
            }

            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var angle = random.NextFloat(0f, math.PI * 2f);
            var distance = random.NextFloat(0f, radius);
            return new float3(math.cos(angle) * distance, 0f, math.sin(angle) * distance);
        }

        private static Entity ResolveLeisureHub(Entity entity, bool hasSettlement, in SettlementRuntime settlementRuntime,
            NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, ComponentLookup<LocalTransform> transformLookup)
        {
            if (hasSettlement)
            {
                if (settlementRuntime.VillageCenterInstance != Entity.Null &&
                    transformLookup.HasComponent(settlementRuntime.VillageCenterInstance))
                {
                    return settlementRuntime.VillageCenterInstance;
                }

                if (settlementRuntime.StorehouseInstance != Entity.Null &&
                    transformLookup.HasComponent(settlementRuntime.StorehouseInstance))
                {
                    return settlementRuntime.StorehouseInstance;
                }
            }

            if (villages.Length == 0 || !transformLookup.HasComponent(entity))
            {
                return Entity.Null;
            }

            var pos = transformLookup[entity].Position;
            var index = ResolveNearestVillageIndex(villages, villageLookup, pos);
            if (index < 0)
            {
                return Entity.Null;
            }

            var target = villages[index];
            if (!transformLookup.HasComponent(target))
            {
                return Entity.Null;
            }

            return target;
        }

        private static int ResolveNearestVillageIndex(NativeArray<Entity> villages, ComponentLookup<Village> villageLookup, float3 position)
        {
            var bestIndex = -1;
            var bestDistSq = float.MaxValue;

            for (int i = 0; i < villages.Length; i++)
            {
                var villageEntity = villages[i];
                if (!villageLookup.HasComponent(villageEntity))
                {
                    continue;
                }

                var center = villageLookup[villageEntity].CenterPosition;
                var distSq = math.distancesq(center.xz, position.xz);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private bool TryGetSettlementRuntime(ref SystemState state, ComponentLookup<LocalTransform> transformLookup, out SettlementRuntime runtime)
        {
            runtime = default;
            var bestScore = int.MinValue;
            var found = false;

            foreach (var runtimeRef in SystemAPI.Query<RefRO<SettlementRuntime>>())
            {
                var candidate = runtimeRef.ValueRO;
                var score = ScoreSettlementRuntime(transformLookup, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    runtime = candidate;
                    found = true;
                }
            }

            return found && bestScore > int.MinValue;
        }

        private int ScoreSettlementRuntime(ComponentLookup<LocalTransform> transformLookup, in SettlementRuntime runtime)
        {
            var score = 0;

            if (HasTransform(runtime.StorehouseInstance, transformLookup))
            {
                score += 200;
            }

            if (HasTransform(runtime.VillageCenterInstance, transformLookup))
            {
                score += 100;
            }

            if (HasTransform(runtime.HousingInstance, transformLookup))
            {
                score += 60;
            }

            if (HasTransform(runtime.WorshipInstance, transformLookup))
            {
                score += 40;
            }

            return score;
        }

        private static bool HasTransform(Entity entity, ComponentLookup<LocalTransform> transformLookup)
        {
            return entity != Entity.Null && transformLookup.HasComponent(entity);
        }
    }
}
