using System;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Time;
using Unity.Entities;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Emits bank markers for movement diagnostics scenarios.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(GodgameHeadlessMovementReportSystem))]
    public partial struct GodgameHeadlessMovementBankSystem : ISystem
    {
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string DiagnosticsScenarioFile = "villager_movement_diagnostics.json";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_MOVEMENT_DIAGNOSTICS_EXIT";
        private const uint DefaultWindowTicks = 7200; // 120 seconds at 60hz.

        private byte _bankResolved;
        private bool _bankActive;
        private bool _bankReported;
        private uint _startTick;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!ResolveBankActive(ref state) || _bankReported)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (_startTick == 0)
            {
                _startTick = timeState.Tick;
            }

            if (SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsFailure failure))
            {
                var tickTime = timeState.Tick;
                if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
                {
                    tickTime = tickTimeState.Tick;
                }

                var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                    ? scenario.Tick
                    : 0u;
                LogBankResult(false, ResolveReason(failure.Reason), tickTime, scenarioTick);
                _bankReported = true;
                RequestExitIfEnabled(ref state, timeState.Tick, 2);
                return;
            }

            if (timeState.Tick >= _startTick + DefaultWindowTicks)
            {
                var tickTime = timeState.Tick;
                if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
                {
                    tickTime = tickTimeState.Tick;
                }

                var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                    ? scenario.Tick
                    : 0u;
                LogBankResult(true, string.Empty, tickTime, scenarioTick);
                _bankReported = true;
                RequestExitIfEnabled(ref state, timeState.Tick, 0);
            }
        }

        private bool ResolveBankActive(ref SystemState state)
        {
            if (_bankResolved != 0)
            {
                return _bankActive;
            }

            var scenarioPath = SystemEnv.GetEnvironmentVariable(ScenarioPathEnv);
            if (string.IsNullOrWhiteSpace(scenarioPath))
            {
                return false;
            }

            _bankResolved = 1;
            _bankActive = scenarioPath.EndsWith(DiagnosticsScenarioFile, StringComparison.OrdinalIgnoreCase);
            if (!_bankActive)
            {
                state.Enabled = false;
            }

            return _bankActive;
        }

        private static string ResolveReason(VillagerMovementFailureReason reason)
        {
            return reason switch
            {
                VillagerMovementFailureReason.Speed => "speed",
                VillagerMovementFailureReason.Teleport => "teleport",
                VillagerMovementFailureReason.Stuck => "stuck",
                VillagerMovementFailureReason.StateFlip => "state_flip",
                VillagerMovementFailureReason.NaN => "nan",
                _ => "unknown"
            };
        }

        private static void RequestExitIfEnabled(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private static void LogBankResult(bool pass, string reason, uint tickTime, uint scenarioTick)
        {
            const string testId = "G2.VILLAGER_MOVEMENT_DIAGNOSTICS";
            var delta = (int)tickTime - (int)scenarioTick;

            if (pass)
            {
                UnityDebug.Log($"BANK:{testId}:PASS tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
                return;
            }

            UnityDebug.Log($"BANK:{testId}:FAIL reason={reason} tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
        }
    }
}
