using System;
using System.Globalization;
using Godgame.Rendering;
using Godgame.Scenario;
using PureDOTS.Rendering;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Time;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless-only need satisfaction loop that nudges urgencies down when villagers reach their target buildings.
    /// Keeps the mind loop observable without presentation hooks.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(VillagerMindSystemGroup))]
    [UpdateBefore(typeof(GodgameHeadlessNeedsProofSystem))]
    public partial struct GodgameHeadlessNeedSatisfactionSystem : ISystem
    {
        private byte _enabled;
        private byte _tickInitialized;
        private uint _lastTick;
        private float _arrivalDistanceSq;
        private float _satisfyRate;
        private float _satisfyMultiplier;
        private float _moveSpeed;
        private float _seekUrgency;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerAIState> _aiLookup;
        private BufferLookup<StorehouseRegistryEntry> _storehouseRegistryEntries;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            _tickInitialized = 0;
            _lastTick = 0;

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerNeedState>();
            state.RequireForUpdate<VillagerNeedTuning>();
            state.RequireForUpdate<VillagerGoalState>();

            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _aiLookup = state.GetComponentLookup<VillagerAIState>();
            _storehouseRegistryEntries = state.GetBufferLookup<StorehouseRegistryEntry>(true);

            var arrivalDistance = ReadEnvFloat("GODGAME_HEADLESS_NEED_TARGET_DISTANCE", 2.5f);
            _arrivalDistanceSq = math.max(0.25f, arrivalDistance * arrivalDistance);
            _satisfyRate = math.max(0.01f, ReadEnvFloat("GODGAME_HEADLESS_NEED_SAT_RATE", 0.25f));
            _satisfyMultiplier = math.max(1f, ReadEnvFloat("GODGAME_HEADLESS_NEED_SAT_MULT", 2.0f));
            _moveSpeed = math.max(0.1f, ReadEnvFloat("GODGAME_HEADLESS_NEED_MOVE_SPEED", 10.0f));
            _seekUrgency = math.clamp(ReadEnvFloat("GODGAME_HEADLESS_NEED_SEEK_THRESHOLD", 0.35f), 0f, 1f);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = ResolveDeltaTime(timeState, out var deltaTicks, out var fixedDt);
            if (deltaTicks == 0u || deltaTime <= 0f)
            {
                return;
            }

            state.EntityManager.CompleteDependencyBeforeRW<VillagerNeedState>();
            state.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();
            state.EntityManager.CompleteDependencyBeforeRW<VillagerAIState>();
            _transformLookup.Update(ref state);
            _aiLookup.Update(ref state);
            _storehouseRegistryEntries.Update(ref state);

            var hasSettlement = TryGetSettlementRuntime(ref state, out var runtime);

            foreach (var (needs, goal, tuning, transform, entity) in SystemAPI
                         .Query<RefRW<VillagerNeedState>, RefRO<VillagerGoalState>, RefRO<VillagerNeedTuning>, RefRW<LocalTransform>>()
                         .WithEntityAccess())
            {
                var currentGoal = goal.ValueRO.CurrentGoal;
                if (!IsNeedGoal(currentGoal))
                {
                    continue;
                }

                var needUrgency = GetNeedUrgencyForGoal(currentGoal, needs.ValueRO);
                if (needUrgency < _seekUrgency)
                {
                    continue;
                }

                var position = transform.ValueRO.Position;
                if (!TryResolveNeedTarget(ref state, currentGoal, hasSettlement, runtime, position, out var target, out var targetPos))
                {
                    continue;
                }

                var distSq = math.distancesq(position.xz, targetPos.xz);
                if (distSq > _arrivalDistanceSq)
                {
                    var toTarget = targetPos - position;
                    toTarget.y = 0f;
                    var distance = math.length(toTarget);
                    if (distance > 1e-4f)
                    {
                        var maxMove = _moveSpeed * deltaTime;
                        var move = math.min(distance, maxMove);
                        var dir = toTarget / distance;
                        position += dir * move;
                        transform.ValueRW.Position = position;
                        distSq = math.distancesq(position.xz, targetPos.xz);
                    }
                }

                var arrived = distSq <= _arrivalDistanceSq;
                var satisfactionScale = arrived ? 1f : 0.5f;
                var updated = needs.ValueRO;
                var delta = ComputeSatisfactionDelta(currentGoal, tuning.ValueRO, fixedDt, deltaTime) * satisfactionScale;
                if (delta > 0f)
                {
                    ApplySatisfaction(currentGoal, ref updated, delta);
                    if (arrived)
                    {
                        ClampSatisfied(currentGoal, ref updated, 0.25f);
                    }
                    needs.ValueRW = updated;
                }

                if (_aiLookup.HasComponent(entity))
                {
                    var ai = _aiLookup[entity];
                    ai.TargetEntity = target;
                    ai.TargetPosition = targetPos;
                    _aiLookup[entity] = ai;
                }
            }
        }

        private static float GetNeedUrgencyForGoal(VillagerGoal goal, in VillagerNeedState needs)
        {
            return goal switch
            {
                VillagerGoal.Eat => needs.HungerUrgency,
                VillagerGoal.Sleep => needs.RestUrgency,
                VillagerGoal.Pray => needs.FaithUrgency,
                VillagerGoal.SeekShelter => needs.SafetyUrgency,
                VillagerGoal.Socialize => needs.SocialUrgency,
                VillagerGoal.Work => needs.WorkUrgency,
                _ => 0f
            };
        }

        private bool TryGetSettlementRuntime(ref SystemState state, out SettlementRuntime runtime)
        {
            runtime = default;
            var bestScore = int.MinValue;
            var found = false;

            foreach (var runtimeRef in SystemAPI.Query<RefRO<SettlementRuntime>>())
            {
                var candidate = runtimeRef.ValueRO;
                var score = ScoreSettlementRuntime(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    runtime = candidate;
                    found = true;
                }
            }

            return found && bestScore > int.MinValue;
        }

        private int ScoreSettlementRuntime(in SettlementRuntime runtime)
        {
            var score = 0;
            var validTargets = 0;

            if (TryGetTargetPosition(runtime.StorehouseInstance, out _))
            {
                score += 200;
                validTargets++;
            }

            if (TryGetTargetPosition(runtime.VillageCenterInstance, out _))
            {
                score += 100;
                validTargets++;
            }

            if (TryGetTargetPosition(runtime.HousingInstance, out _))
            {
                score += 60;
                validTargets++;
            }

            if (TryGetTargetPosition(runtime.WorshipInstance, out _))
            {
                score += 60;
                validTargets++;
            }

            if (validTargets == 0)
            {
                return int.MinValue;
            }

            if (runtime.HasSpawned != 0)
            {
                score += 1000;
            }

            return score;
        }

        private bool TryFindNearestStorehouseRegistryTarget(ref SystemState state, in float3 position, out Entity target, out float3 targetPos)
        {
            target = Entity.Null;
            targetPos = float3.zero;

            if (!SystemAPI.TryGetSingletonEntity<StorehouseRegistry>(out var registryEntity))
            {
                return false;
            }

            if (!_storehouseRegistryEntries.HasBuffer(registryEntity))
            {
                return false;
            }

            var entries = _storehouseRegistryEntries[registryEntity];
            if (entries.Length == 0)
            {
                return false;
            }

            var bestDistSq = float.MaxValue;
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.StorehouseEntity == Entity.Null)
                {
                    continue;
                }

                var distSq = math.distancesq(position.xz, entry.Position.xz);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    target = entry.StorehouseEntity;
                    targetPos = entry.Position;
                }
            }

            return target != Entity.Null;
        }

        private bool TryResolveNeedTarget(
            ref SystemState state,
            VillagerGoal goal,
            bool hasSettlement,
            in SettlementRuntime settlement,
            in float3 villagerPosition,
            out Entity target,
            out float3 targetPosition)
        {
            target = Entity.Null;
            targetPosition = float3.zero;

            if (hasSettlement)
            {
                target = ResolveNeedTarget(goal, settlement);
                if (TryGetTargetPosition(target, out targetPosition))
                {
                    return true;
                }

                target = ResolveFallbackTarget(goal, settlement);
                if (TryGetTargetPosition(target, out targetPosition))
                {
                    return true;
                }
            }

            if (goal == VillagerGoal.Eat)
            {
                return TryFindNearestNeedFallbackTarget(ref state, villagerPosition, out target, out targetPosition);
            }

            return TryFindNearestNeedFallbackTarget(ref state, villagerPosition, out target, out targetPosition);
        }

        private static Entity ResolveFallbackTarget(VillagerGoal goal, in SettlementRuntime settlement)
        {
            return goal switch
            {
                VillagerGoal.Eat => settlement.StorehouseInstance != Entity.Null
                    ? settlement.StorehouseInstance
                    : settlement.VillageCenterInstance != Entity.Null
                        ? settlement.VillageCenterInstance
                        : settlement.HousingInstance != Entity.Null
                            ? settlement.HousingInstance
                            : settlement.WorshipInstance,
                VillagerGoal.Sleep => settlement.HousingInstance != Entity.Null
                    ? settlement.HousingInstance
                    : settlement.VillageCenterInstance != Entity.Null
                        ? settlement.VillageCenterInstance
                        : settlement.StorehouseInstance != Entity.Null
                            ? settlement.StorehouseInstance
                            : settlement.WorshipInstance,
                VillagerGoal.Pray => settlement.WorshipInstance != Entity.Null
                    ? settlement.WorshipInstance
                    : settlement.VillageCenterInstance != Entity.Null
                        ? settlement.VillageCenterInstance
                        : settlement.StorehouseInstance != Entity.Null
                            ? settlement.StorehouseInstance
                            : settlement.HousingInstance,
                VillagerGoal.SeekShelter => settlement.HousingInstance != Entity.Null
                    ? settlement.HousingInstance
                    : settlement.VillageCenterInstance != Entity.Null
                        ? settlement.VillageCenterInstance
                        : settlement.StorehouseInstance != Entity.Null
                            ? settlement.StorehouseInstance
                            : settlement.WorshipInstance,
                VillagerGoal.Socialize => settlement.VillageCenterInstance != Entity.Null
                    ? settlement.VillageCenterInstance
                    : settlement.StorehouseInstance != Entity.Null
                        ? settlement.StorehouseInstance
                        : settlement.HousingInstance != Entity.Null
                            ? settlement.HousingInstance
                            : settlement.WorshipInstance,
                _ => Entity.Null
            };
        }

        private bool TryGetTargetPosition(Entity candidate, out float3 position)
        {
            if (candidate != Entity.Null && candidate.Index >= 0 && _transformLookup.HasComponent(candidate))
            {
                position = _transformLookup[candidate].Position;
                return true;
            }

            position = float3.zero;
            return false;
        }

        private bool TryFindNearestNeedFallbackTarget(ref SystemState state, in float3 position, out Entity target, out float3 targetPos)
        {
            target = Entity.Null;
            targetPos = float3.zero;
            var bestDistSq = float.MaxValue;

            if (TryFindNearestStorehouseRegistryTarget(ref state, position, out target, out targetPos))
            {
                return true;
            }

            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                         .WithAll<StorehouseInventory>()
                         .WithEntityAccess())
            {
                var candidatePos = transform.ValueRO.Position;
                var distSq = math.distancesq(position, candidatePos);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    target = entity;
                    targetPos = candidatePos;
                }
            }

            if (target != Entity.Null)
            {
                return true;
            }

            bestDistSq = float.MaxValue;
            foreach (var (transform, semantic, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<RenderSemanticKey>>().WithEntityAccess())
            {
                var key = semantic.ValueRO.Value;
                if (key != GodgameSemanticKeys.Storehouse
                    && key != GodgameSemanticKeys.VillageCenter
                    && key != GodgameSemanticKeys.Housing
                    && key != GodgameSemanticKeys.Worship)
                {
                    continue;
                }

                var candidatePos = transform.ValueRO.Position;
                var distSq = math.distancesq(position, candidatePos);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    target = entity;
                    targetPos = candidatePos;
                }
            }

            return target != Entity.Null;
        }

        private float ResolveDeltaTime(in TimeState timeState, out uint deltaTicks, out float fixedDt)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                deltaTicks = 0u;
                fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
                return 0f;
            }

            deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
                return 0f;
            }

            fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }

        private float ComputeSatisfactionDelta(VillagerGoal goal, in VillagerNeedTuning tuning, float fixedDt, float deltaTime)
        {
            var perTickIncrease = goal switch
            {
                VillagerGoal.Eat => tuning.HungerDecayPerTick,
                VillagerGoal.Sleep => tuning.RestDecayPerTick,
                VillagerGoal.Pray => tuning.FaithDecayPerTick,
                VillagerGoal.SeekShelter => tuning.SafetyDecayPerTick,
                VillagerGoal.Socialize => tuning.SocialDecayPerTick,
                _ => 0f
            };

            perTickIncrease = math.max(0f, perTickIncrease);
            var requiredPerSecond = perTickIncrease / math.max(fixedDt, 1e-4f);
            var satisfyPerSecond = math.max(_satisfyRate, requiredPerSecond * _satisfyMultiplier);
            return satisfyPerSecond * deltaTime;
        }

        private static bool IsNeedGoal(VillagerGoal goal)
        {
            return goal == VillagerGoal.Eat
                   || goal == VillagerGoal.Sleep
                   || goal == VillagerGoal.Pray
                   || goal == VillagerGoal.SeekShelter
                   || goal == VillagerGoal.Socialize;
        }

        private static Entity ResolveNeedTarget(VillagerGoal goal, in SettlementRuntime runtime)
        {
            return goal switch
            {
                VillagerGoal.Eat => runtime.StorehouseInstance != Entity.Null
                    ? runtime.StorehouseInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.Sleep => runtime.HousingInstance != Entity.Null
                    ? runtime.HousingInstance
                    : runtime.StorehouseInstance,
                VillagerGoal.Pray => runtime.WorshipInstance != Entity.Null
                    ? runtime.WorshipInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.SeekShelter => runtime.HousingInstance != Entity.Null
                    ? runtime.HousingInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.Socialize => runtime.VillageCenterInstance != Entity.Null
                    ? runtime.VillageCenterInstance
                    : runtime.StorehouseInstance,
                _ => Entity.Null
            };
        }

        private static void ApplySatisfaction(VillagerGoal goal, ref VillagerNeedState needs, float delta)
        {
            switch (goal)
            {
                case VillagerGoal.Eat:
                    needs.HungerUrgency = math.max(0f, needs.HungerUrgency - delta);
                    break;
                case VillagerGoal.Sleep:
                    needs.RestUrgency = math.max(0f, needs.RestUrgency - delta);
                    break;
                case VillagerGoal.Pray:
                    needs.FaithUrgency = math.max(0f, needs.FaithUrgency - delta);
                    break;
                case VillagerGoal.SeekShelter:
                    needs.SafetyUrgency = math.max(0f, needs.SafetyUrgency - delta);
                    break;
                case VillagerGoal.Socialize:
                    needs.SocialUrgency = math.max(0f, needs.SocialUrgency - delta);
                    break;
            }
        }

        private static void ClampSatisfied(VillagerGoal goal, ref VillagerNeedState needs, float satisfiedThreshold)
        {
            switch (goal)
            {
                case VillagerGoal.Eat:
                    needs.HungerUrgency = math.min(needs.HungerUrgency, satisfiedThreshold);
                    break;
                case VillagerGoal.Sleep:
                    needs.RestUrgency = math.min(needs.RestUrgency, satisfiedThreshold);
                    break;
                case VillagerGoal.Pray:
                    needs.FaithUrgency = math.min(needs.FaithUrgency, satisfiedThreshold);
                    break;
                case VillagerGoal.SeekShelter:
                    needs.SafetyUrgency = math.min(needs.SafetyUrgency, satisfiedThreshold);
                    break;
                case VillagerGoal.Socialize:
                    needs.SocialUrgency = math.min(needs.SocialUrgency, satisfiedThreshold);
                    break;
            }
        }

        private static float ReadEnvFloat(string key, float defaultValue)
        {
            var raw = SystemEnv.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Headless proof that needs rise, trigger a seek goal, and then get satisfied.
    /// Logs exactly one PASS/FAIL line when criteria are met or timeout is reached.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessNeedsProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_NEEDS_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_NEEDS_PROOF_EXIT";
        private const uint DefaultTimeoutTicks = 1200; // ~20 seconds at 60hz

        private byte _enabled;
        private byte _done;
        private byte _initialized;
        private uint _startTick;
        private uint _timeoutTick;
        private float _initialHunger;
        private float _initialRest;
        private float _initialFaith;
        private float _initialSafety;
        private float _initialSocial;
        private float _maxHunger;
        private float _maxRest;
        private float _maxFaith;
        private float _maxSafety;
        private float _maxSocial;
        private byte _sawEatGoal;
        private byte _sawSleepGoal;
        private byte _sawPrayGoal;
        private byte _sawShelterGoal;
        private byte _sawSocialGoal;
        private byte _sawTarget;
        private Entity _trackedVillager;
        private byte _rewindSubjectRegistered;
        private byte _rewindPending;
        private byte _rewindPass;
        private float _rewindObserved;

        private static readonly FixedString64Bytes LoopId = new FixedString64Bytes("needs");
        private static readonly FixedString32Bytes ExpectedRule = new FixedString32Bytes("satisfied");
        private static readonly FixedString32Bytes StepHunger = new FixedString32Bytes("hunger");
        private static readonly FixedString32Bytes StepRest = new FixedString32Bytes("rest");
        private static readonly FixedString32Bytes StepFaith = new FixedString32Bytes("faith");
        private static readonly FixedString32Bytes StepSafety = new FixedString32Bytes("safety");
        private static readonly FixedString32Bytes StepSocial = new FixedString32Bytes("social");
        private static readonly FixedString64Bytes RewindProofId = new FixedString64Bytes("godgame.needs");
        private const byte RewindRequiredMask = (byte)HeadlessRewindProofStage.RecordReturn;

        private EntityQuery _villagerQuery;
        private ComponentLookup<VillagerAIState> _aiLookup;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            state.RequireForUpdate<TimeState>();
            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerNeedState, VillagerGoalState, VillagerNeedTuning, FocusBudget, LocalTransform>()
                .Build();
            _aiLookup = state.GetComponentLookup<VillagerAIState>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0)
            {
                return;
            }

            EnsureRewindSubject(ref state);
            TryFlushRewindProof(ref state);

            if (_done != 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (_timeoutTick == 0)
            {
                _startTick = timeState.Tick;
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            EnsureTrackedVillager(ref state);
            if (_trackedVillager == Entity.Null)
            {
                return;
            }

            var needs = state.EntityManager.GetComponentData<VillagerNeedState>(_trackedVillager);
            var goal = state.EntityManager.GetComponentData<VillagerGoalState>(_trackedVillager);

            if (_initialized == 0)
            {
                _initialized = 1;
                _initialHunger = needs.HungerUrgency;
                _initialRest = needs.RestUrgency;
                _initialFaith = needs.FaithUrgency;
                _initialSafety = needs.SafetyUrgency;
                _initialSocial = needs.SocialUrgency;
                _maxHunger = _initialHunger;
                _maxRest = _initialRest;
                _maxFaith = _initialFaith;
                _maxSafety = _initialSafety;
                _maxSocial = _initialSocial;
            }

            _maxHunger = math.max(_maxHunger, needs.HungerUrgency);
            _maxRest = math.max(_maxRest, needs.RestUrgency);
            _maxFaith = math.max(_maxFaith, needs.FaithUrgency);
            _maxSafety = math.max(_maxSafety, needs.SafetyUrgency);
            _maxSocial = math.max(_maxSocial, needs.SocialUrgency);
            if (goal.CurrentGoal == VillagerGoal.Eat)
            {
                _sawEatGoal = 1;
            }
            else if (goal.CurrentGoal == VillagerGoal.Sleep)
            {
                _sawSleepGoal = 1;
            }
            else if (goal.CurrentGoal == VillagerGoal.Pray)
            {
                _sawPrayGoal = 1;
            }
            else if (goal.CurrentGoal == VillagerGoal.SeekShelter)
            {
                _sawShelterGoal = 1;
            }
            else if (goal.CurrentGoal == VillagerGoal.Socialize)
            {
                _sawSocialGoal = 1;
            }

            state.EntityManager.CompleteDependencyBeforeRO<VillagerAIState>();
            _aiLookup.Update(ref state);
            var hasAi = _aiLookup.HasComponent(_trackedVillager);
            if (hasAi && _aiLookup[_trackedVillager].TargetEntity != Entity.Null)
            {
                _sawTarget = 1;
            }

            var satisfied = TryResolveSatisfiedNeed(in needs, out var satisfiedStep, out var satisfiedCurrent, out var satisfiedMax, out var satisfiedInitial);
            var seekConfirmed = _sawTarget != 0 || !hasAi;

            if (satisfied && seekConfirmed)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 1;
                _rewindObserved = satisfiedCurrent;
                UnityDebug.Log($"[GodgameHeadlessNeedsProof] PASS tick={timeState.Tick} need={satisfiedStep} current={satisfiedCurrent:F2} max={satisfiedMax:F2} initial={satisfiedInitial:F2} goal={goal.CurrentGoal} targetSeen={_sawTarget}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, LoopId, true, satisfiedCurrent, ExpectedRule, DefaultTimeoutTicks, step: satisfiedStep);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 0;
                _rewindObserved = needs.HungerUrgency;
                UnityDebug.LogError($"[GodgameHeadlessNeedsProof] FAIL tick={timeState.Tick} hunger={needs.HungerUrgency:F2} rest={needs.RestUrgency:F2} faith={needs.FaithUrgency:F2} safety={needs.SafetyUrgency:F2} social={needs.SocialUrgency:F2} goal={goal.CurrentGoal} goalUrgency={goal.CurrentGoalUrgency:F2} targetSeen={_sawTarget}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, LoopId, false, needs.HungerUrgency, ExpectedRule, DefaultTimeoutTicks, step: StepHunger);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 4);
            }
        }

        private void EnsureRewindSubject(ref SystemState state)
        {
            if (_rewindSubjectRegistered != 0)
            {
                return;
            }

            if (HeadlessRewindProofUtility.TryEnsureSubject(state.EntityManager, RewindProofId, RewindRequiredMask))
            {
                _rewindSubjectRegistered = 1;
            }
        }

        private void TryFlushRewindProof(ref SystemState state)
        {
            if (_rewindPending == 0)
            {
                return;
            }

            if (!HeadlessRewindProofUtility.TryGetState(state.EntityManager, out var rewindProof) || rewindProof.SawRecord == 0)
            {
                return;
            }

            HeadlessRewindProofUtility.TryMarkResult(state.EntityManager, RewindProofId, _rewindPass != 0, _rewindObserved, ExpectedRule, RewindRequiredMask);
            _rewindPending = 0;
        }

        private static void ExitIfRequested(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private void EnsureTrackedVillager(ref SystemState state)
        {
            if (_trackedVillager != Entity.Null && state.EntityManager.Exists(_trackedVillager))
            {
                return;
            }

            if (_villagerQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            using var entities = _villagerQuery.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
            {
                return;
            }

            _trackedVillager = entities[0];
        }

        private bool TryResolveSatisfiedNeed(in VillagerNeedState needs, out FixedString32Bytes step, out float current, out float max, out float initial)
        {
            if (_sawEatGoal != 0 && _maxHunger >= 0.6f && needs.HungerUrgency <= 0.3f)
            {
                step = StepHunger;
                current = needs.HungerUrgency;
                max = _maxHunger;
                initial = _initialHunger;
                return true;
            }

            if (_sawSleepGoal != 0 && _maxRest >= 0.6f && needs.RestUrgency <= 0.3f)
            {
                step = StepRest;
                current = needs.RestUrgency;
                max = _maxRest;
                initial = _initialRest;
                return true;
            }

            if (_sawPrayGoal != 0 && _maxFaith >= 0.6f && needs.FaithUrgency <= 0.3f)
            {
                step = StepFaith;
                current = needs.FaithUrgency;
                max = _maxFaith;
                initial = _initialFaith;
                return true;
            }

            if (_sawShelterGoal != 0 && _maxSafety >= 0.6f && needs.SafetyUrgency <= 0.3f)
            {
                step = StepSafety;
                current = needs.SafetyUrgency;
                max = _maxSafety;
                initial = _initialSafety;
                return true;
            }

            if (_sawSocialGoal != 0 && _maxSocial >= 0.6f && needs.SocialUrgency <= 0.3f)
            {
                step = StepSocial;
                current = needs.SocialUrgency;
                max = _maxSocial;
                initial = _initialSocial;
                return true;
            }

            step = default;
            current = 0f;
            max = 0f;
            initial = 0f;
            return false;
        }
    }
}
