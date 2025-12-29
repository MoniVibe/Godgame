using System;
using Godgame.Scenario;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Time;
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
    /// Headless proof that a villager moves and storehouse inventory increases.
    /// Logs exactly one PASS/FAIL line when criteria are met or a timeout is reached.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessVillagerProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_VILLAGER_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_VILLAGER_PROOF_EXIT";
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string SmokeScenarioFile = "godgame_smoke.json";
        private const string LoopScenarioFile = "villager_loop_small.json";
        private const uint DefaultTimeoutTicks = 900; // ~15 seconds at 60hz

        private byte _enabled;
        private byte _done;
        private uint _startTick;
        private uint _timeoutTick;
        private Entity _trackedVillager;
        private float3 _trackedStart;
        private float _initialStored;
        private byte _rewindSubjectRegistered;
        private byte _rewindPending;
        private byte _rewindPass;
        private float _rewindObserved;
        private FixedString64Bytes _bankTestId;
        private byte _bankResolved;
        private static readonly FixedString32Bytes ExpectedDelta = new FixedString32Bytes(">0");
        private static readonly FixedString32Bytes StepGatherDeliver = new FixedString32Bytes("gather_deliver");
        private static readonly FixedString64Bytes RewindProofId = new FixedString64Bytes("godgame.villager");
        private const byte RewindRequiredMask = (byte)HeadlessRewindProofStage.RecordReturn;

        private EntityQuery _villagerQuery;
        private EntityQuery _storehouseQuery;

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
                .WithAll<SettlementVillagerState, LocalTransform>()
                .Build();
            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<StorehouseInventory>()
                .Build();

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
                _initialStored = GetStoredTotal(ref state);
            }

            EnsureTrackedVillager(ref state);
            var moved = 0f;
            if (_trackedVillager != Entity.Null && state.EntityManager.HasComponent<LocalTransform>(_trackedVillager))
            {
                var position = state.EntityManager.GetComponentData<LocalTransform>(_trackedVillager).Position;
                moved = math.distance(position, _trackedStart);
            }

            var stored = GetStoredTotal(ref state);
            var villagerCount = _villagerQuery.CalculateEntityCount();
            var storehouseCount = _storehouseQuery.CalculateEntityCount();
            var phase = GetTrackedPhase(ref state);

            if (moved > 2f && stored > _initialStored + 0.01f)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 1;
                _rewindObserved = stored - _initialStored;
                var tickTime = timeState.Tick;
                if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
                {
                    tickTime = tickTimeState.Tick;
                }

                var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                    ? scenario.Tick
                    : 0u;
                UnityDebug.Log($"[GodgameHeadlessVillagerProof] PASS tick={timeState.Tick} moved={moved:F2} stored={stored:F2} initialStored={_initialStored:F2} villagers={villagerCount} storehouses={storehouseCount} phase={phase}");
                LogBankResult(ResolveBankTestId(), true, string.Empty, tickTime, scenarioTick);
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Logistics, true, stored - _initialStored, ExpectedDelta, DefaultTimeoutTicks, step: StepGatherDeliver);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                _rewindPending = 1;
                _rewindPass = 0;
                _rewindObserved = stored - _initialStored;
                var tickTime = timeState.Tick;
                if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
                {
                    tickTime = tickTimeState.Tick;
                }

                var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                    ? scenario.Tick
                    : 0u;
                UnityDebug.LogError($"[GodgameHeadlessVillagerProof] FAIL tick={timeState.Tick} moved={moved:F2} stored={stored:F2} initialStored={_initialStored:F2} villagers={villagerCount} storehouses={storehouseCount} phase={phase}");
                LogBankResult(ResolveBankTestId(), false, "timeout", tickTime, scenarioTick);
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Logistics, false, stored - _initialStored, ExpectedDelta, DefaultTimeoutTicks, step: StepGatherDeliver);
                TryFlushRewindProof(ref state);
                ExitIfRequested(ref state, timeState.Tick, 3);
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

            HeadlessRewindProofUtility.TryMarkResult(state.EntityManager, RewindProofId, _rewindPass != 0, _rewindObserved, ExpectedDelta, RewindRequiredMask);
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

            float3 storehousePos = float3.zero;
            var hasStorehousePos = false;

            if (!_storehouseQuery.IsEmptyIgnoreFilter)
            {
                using var storehouses = _storehouseQuery.ToEntityArray(Allocator.Temp);
                for (var i = 0; i < storehouses.Length; i++)
                {
                    var storehouse = storehouses[i];
                    if (storehouse != Entity.Null && state.EntityManager.HasComponent<LocalTransform>(storehouse))
                    {
                        storehousePos = state.EntityManager.GetComponentData<LocalTransform>(storehouse).Position;
                        hasStorehousePos = true;
                        break;
                    }
                }
            }

            using var entities = _villagerQuery.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
            {
                return;
            }

            var best = Entity.Null;
            var bestScore = float.MinValue;
            var bestPos = float3.zero;

            for (var i = 0; i < entities.Length; i++)
            {
                var candidate = entities[i];
                if (candidate == Entity.Null || !state.EntityManager.HasComponent<LocalTransform>(candidate))
                {
                    continue;
                }

                var pos = state.EntityManager.GetComponentData<LocalTransform>(candidate).Position;
                var score = hasStorehousePos ? math.distancesq(pos.xz, storehousePos.xz) : candidate.Index;

                if (best == Entity.Null || score > bestScore || (math.abs(score - bestScore) <= 1e-4f && candidate.Index < best.Index))
                {
                    best = candidate;
                    bestScore = score;
                    bestPos = pos;
                }
            }

            if (best == Entity.Null)
            {
                return;
            }

            _trackedVillager = best;
            _trackedStart = bestPos;
        }

        private FixedString64Bytes ResolveBankTestId()
        {
            if (_bankResolved != 0)
            {
                return _bankTestId;
            }

            _bankResolved = 1;
            var scenarioPath = SystemEnv.GetEnvironmentVariable(ScenarioPathEnv);
            if (string.IsNullOrWhiteSpace(scenarioPath))
            {
                return _bankTestId;
            }

            if (scenarioPath.EndsWith(SmokeScenarioFile, StringComparison.OrdinalIgnoreCase))
            {
                _bankTestId = new FixedString64Bytes("G0.GODGAME_SMOKE");
            }
            else if (scenarioPath.EndsWith(LoopScenarioFile, StringComparison.OrdinalIgnoreCase))
            {
                _bankTestId = new FixedString64Bytes("G1.VILLAGER_LOOP_SMALL");
            }

            return _bankTestId;
        }

        private static void LogBankResult(FixedString64Bytes testId, bool pass, string reason, uint tickTime, uint scenarioTick)
        {
            if (testId.IsEmpty)
            {
                return;
            }
            var delta = (int)tickTime - (int)scenarioTick;

            if (pass)
            {
                UnityDebug.Log($"BANK:{testId}:PASS tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
                return;
            }

            UnityDebug.Log($"BANK:{testId}:FAIL reason={reason} tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
        }

        private SettlementVillagerPhase GetTrackedPhase(ref SystemState state)
        {
            if (_trackedVillager == Entity.Null || !state.EntityManager.Exists(_trackedVillager))
            {
                return default;
            }

            if (!state.EntityManager.HasComponent<SettlementVillagerState>(_trackedVillager))
            {
                return default;
            }

            return state.EntityManager.GetComponentData<SettlementVillagerState>(_trackedVillager).Phase;
        }

        private float GetStoredTotal(ref SystemState state)
        {
            var total = 0f;
            using var inventories = _storehouseQuery.ToComponentDataArray<StorehouseInventory>(Allocator.Temp);
            for (var i = 0; i < inventories.Length; i++)
            {
                total += math.max(0f, inventories[i].TotalStored);
            }

            return total;
        }
    }
}
