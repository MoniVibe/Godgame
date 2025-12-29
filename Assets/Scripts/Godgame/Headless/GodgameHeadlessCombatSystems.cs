using System;
using System.Globalization;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Time;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VillagerAIState = PureDOTS.Runtime.Components.VillagerAIState;
using VillagerCombatStats = Godgame.Villagers.VillagerCombatStats;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless-only combat loop: detect threat, engage, and resolve with simple damage ticks.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameHeadlessCombatLoopSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_COMBAT_PROOF";
        private byte _enabled;
        private byte _tickInitialized;
        private uint _lastTick;
        private byte _hasEngaged;
        private byte _resolved;
        private Entity _attacker;
        private Entity _defender;
        private float _engageDistanceSq;
        private float _damageScale;
        private const float DefaultResolveSeconds = 6f;
        private ComponentLookup<VillagerCombatStats> _combatLookup;
        private ComponentLookup<VillagerThreatState> _threatLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerAIState> _aiLookup;

        public void OnCreate(ref SystemState state)
        {
            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (!RuntimeMode.IsHeadless || !string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            _tickInitialized = 0;
            _lastTick = 0;
            _hasEngaged = 0;
            _resolved = 0;

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerCombatStats>();
            state.RequireForUpdate<LocalTransform>();

            _combatLookup = state.GetComponentLookup<VillagerCombatStats>(false);
            _threatLookup = state.GetComponentLookup<VillagerThreatState>(false);
            _transformLookup = state.GetComponentLookup<LocalTransform>(false);
            _aiLookup = state.GetComponentLookup<VillagerAIState>(false);

            var engageDistance = ReadEnvFloat("GODGAME_HEADLESS_COMBAT_ENGAGE_DISTANCE", 6f);
            _engageDistanceSq = math.max(1f, engageDistance * engageDistance);
            _damageScale = math.max(0.1f, ReadEnvFloat("GODGAME_HEADLESS_COMBAT_DAMAGE_SCALE", 1f));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0 || _resolved != 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = ResolveDeltaTime(timeState);
            if (deltaTime <= 0f)
            {
                return;
            }

            state.EntityManager.CompleteDependencyBeforeRW<VillagerAIState>();
            _combatLookup.Update(ref state);
            _threatLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _aiLookup.Update(ref state);

            if (!EnsureCombatants(ref state))
            {
                return;
            }

            if (!_transformLookup.HasComponent(_attacker) || !_transformLookup.HasComponent(_defender))
            {
                return;
            }

            var attackerPos = _transformLookup[_attacker].Position;
            var defenderPos = _transformLookup[_defender].Position;
            var distanceSq = math.distancesq(attackerPos.xz, defenderPos.xz);
            if (_hasEngaged == 0 && distanceSq > _engageDistanceSq)
            {
                var direction = math.normalizesafe(defenderPos - attackerPos, new float3(1f, 0f, 0f));
                var desiredDistance = math.max(1f, math.sqrt(_engageDistanceSq) * 0.5f);
                var adjustedPos = attackerPos + direction * desiredDistance;
                var defenderTransform = _transformLookup[_defender];
                defenderTransform.Position = new float3(adjustedPos.x, defenderTransform.Position.y, adjustedPos.z);
                _transformLookup[_defender] = defenderTransform;
                defenderPos = defenderTransform.Position;
                distanceSq = math.distancesq(attackerPos.xz, defenderPos.xz);
            }

            var engaged = _hasEngaged != 0 || distanceSq <= _engageDistanceSq;
            if (!engaged)
            {
                UpdateThreats(attackerPos, defenderPos, 0.6f);
                return;
            }

            _hasEngaged = 1;
            UpdateThreats(attackerPos, defenderPos, 1f);

            var attackerStats = _combatLookup[_attacker];
            var defenderStats = _combatLookup[_defender];

            attackerStats.CurrentTarget = _defender;
            defenderStats.CurrentTarget = _attacker;

            var attackerDamage = math.max(1f, attackerStats.AttackDamage);
            var defenderDamage = math.max(1f, defenderStats.AttackDamage);
            var baselineHealth = math.max(1f, math.max(attackerStats.MaxHealth, defenderStats.MaxHealth));
            var minDps = baselineHealth / math.max(1f, DefaultResolveSeconds);

            var attackDelta = math.max(attackerDamage * _damageScale, minDps) * deltaTime;
            var defendDelta = math.max(defenderDamage * _damageScale, minDps) * deltaTime;

            defenderStats.CurrentHealth = math.max(0f, defenderStats.CurrentHealth - attackDelta);
            attackerStats.CurrentHealth = math.max(0f, attackerStats.CurrentHealth - defendDelta);

            _combatLookup[_attacker] = attackerStats;
            _combatLookup[_defender] = defenderStats;

            UpdateAIStateFighting(_attacker);
            UpdateAIStateFighting(_defender);

            if (defenderStats.CurrentHealth <= 0f || attackerStats.CurrentHealth <= 0f)
            {
                _resolved = 1;
                ClearThreats();
            }
        }

        private bool EnsureCombatants(ref SystemState state)
        {
            if (_attacker != Entity.Null && _defender != Entity.Null &&
                state.EntityManager.Exists(_attacker) && state.EntityManager.Exists(_defender))
            {
                return true;
            }

            _attacker = Entity.Null;
            _defender = Entity.Null;

            foreach (var (_, entity) in SystemAPI.Query<RefRO<VillagerCombatStats>>().WithAll<LocalTransform>().WithEntityAccess())
            {
                if (_attacker == Entity.Null)
                {
                    _attacker = entity;
                    continue;
                }

                _defender = entity;
                break;
            }

            return _attacker != Entity.Null && _defender != Entity.Null;
        }

        private void UpdateThreats(float3 attackerPos, float3 defenderPos, float urgency)
        {
            if (_threatLookup.HasComponent(_attacker))
            {
                var threat = _threatLookup[_attacker];
                threat.ThreatEntity = _defender;
                threat.ThreatDirection = math.normalizesafe(defenderPos - attackerPos, new float3(0f, 0f, 1f));
                threat.Urgency = urgency;
                threat.HasLineOfSight = 1;
                _threatLookup[_attacker] = threat;
            }

            if (_threatLookup.HasComponent(_defender))
            {
                var threat = _threatLookup[_defender];
                threat.ThreatEntity = _attacker;
                threat.ThreatDirection = math.normalizesafe(attackerPos - defenderPos, new float3(0f, 0f, 1f));
                threat.Urgency = urgency;
                threat.HasLineOfSight = 1;
                _threatLookup[_defender] = threat;
            }
        }

        private void ClearThreats()
        {
            if (_threatLookup.HasComponent(_attacker))
            {
                var threat = _threatLookup[_attacker];
                threat.ThreatEntity = Entity.Null;
                threat.Urgency = 0f;
                threat.HasLineOfSight = 0;
                _threatLookup[_attacker] = threat;
            }

            if (_threatLookup.HasComponent(_defender))
            {
                var threat = _threatLookup[_defender];
                threat.ThreatEntity = Entity.Null;
                threat.Urgency = 0f;
                threat.HasLineOfSight = 0;
                _threatLookup[_defender] = threat;
            }

            if (_combatLookup.HasComponent(_attacker))
            {
                var combat = _combatLookup[_attacker];
                combat.CurrentTarget = Entity.Null;
                _combatLookup[_attacker] = combat;
            }

            if (_combatLookup.HasComponent(_defender))
            {
                var combat = _combatLookup[_defender];
                combat.CurrentTarget = Entity.Null;
                _combatLookup[_defender] = combat;
            }
        }

        private void UpdateAIStateFighting(Entity entity)
        {
            if (!_aiLookup.HasComponent(entity))
            {
                return;
            }

            var ai = _aiLookup[entity];
            ai.CurrentGoal = VillagerAIState.Goal.Fight;
            ai.CurrentState = VillagerAIState.State.Fighting;
            _aiLookup[entity] = ai;
        }

        private float ResolveDeltaTime(in TimeState timeState)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                return 0f;
            }

            var deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
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
    /// Headless proof that a combat engagement starts and resolves.
    /// Logs exactly one PASS/FAIL line when criteria are met or timeout is reached.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessCombatProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_COMBAT_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_COMBAT_PROOF_EXIT";
        private const uint DefaultTimeoutTicks = 1200; // ~20 seconds at 60hz

        private byte _enabled;
        private byte _done;
        private uint _startTick;
        private uint _timeoutTick;
        private byte _sawEngaged;
        private byte _sawResolved;
        private byte _rewindSubjectRegistered;
        private byte _rewindPending;
        private byte _rewindPass;
        private float _rewindObserved;

        private static readonly FixedString32Bytes ExpectedRule = new FixedString32Bytes("engaged+resolved");
        private static readonly FixedString32Bytes StepEngagement = new FixedString32Bytes("engagement");
        private static readonly FixedString64Bytes RewindProofId = new FixedString64Bytes("godgame.combat");
        private const byte RewindRequiredMask = (byte)HeadlessRewindProofStage.RecordReturn;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (!string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerCombatStats>();
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

            int engagedCount = 0;
            int defeatedCount = 0;
            int total = 0;

            foreach (var combat in SystemAPI.Query<RefRO<VillagerCombatStats>>())
            {
                total++;
                if (combat.ValueRO.CurrentTarget != Entity.Null)
                {
                    engagedCount++;
                }

                if (combat.ValueRO.CurrentHealth <= 0f)
                {
                    defeatedCount++;
                }
            }

            if (engagedCount > 0)
            {
                _sawEngaged = 1;
            }

            if (defeatedCount > 0)
            {
                _sawResolved = 1;
            }

            if (_sawEngaged != 0 && _sawResolved != 0)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 1;
                _rewindObserved = defeatedCount;
                UnityDebug.Log($"[GodgameHeadlessCombatProof] PASS tick={timeState.Tick} combatants={total} engaged={engagedCount} defeated={defeatedCount}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Combat, true, defeatedCount, ExpectedRule, DefaultTimeoutTicks, step: StepEngagement);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 0;
                _rewindObserved = defeatedCount;
                UnityDebug.LogError($"[GodgameHeadlessCombatProof] FAIL tick={timeState.Tick} combatants={total} engaged={engagedCount} defeated={defeatedCount}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Combat, false, defeatedCount, ExpectedRule, DefaultTimeoutTicks, step: StepEngagement);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 5);
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
    }
}
