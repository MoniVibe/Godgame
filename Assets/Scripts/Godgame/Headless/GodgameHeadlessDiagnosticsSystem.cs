using Godgame.Scenario;
using PureDOTS.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Headless
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Telemetry.TelemetryExportSystem))]
    [UpdateBefore(typeof(GodgameHeadlessExitSystem))]
    public partial struct GodgameHeadlessDiagnosticsSystem : ISystem
    {
        private const uint SampleIntervalTicks = 30;
        private const uint HeartbeatIntervalTicks = 60;
        private uint _lastSampleTick;
        private uint _lastHeartbeatTick;
        private byte _runtimeSeen;
        private byte _runStarted;
        private byte _exitHandled;
        private ScenarioBootPhase _lastBootPhase;

        public void OnCreate(ref SystemState state)
        {
            RuntimeMode.RefreshFromEnvironment();
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            GodgameHeadlessDiagnostics.InitializeFromArgs();
            if (!GodgameHeadlessDiagnostics.Enabled)
            {
                state.Enabled = false;
                return;
            }

            _lastBootPhase = (ScenarioBootPhase)255;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!GodgameHeadlessDiagnostics.Enabled)
            {
                return;
            }

            if (!TryResolveTick(ref state, out var tick, out var fixedDt))
            {
                return;
            }

            GodgameHeadlessDiagnostics.RecordMetrics(tick, fixedDt, state.EntityManager, ref _lastSampleTick, SampleIntervalTicks);
            UpdateProgress(ref state, tick);
            UpdateHeartbeat(tick);

            if (_exitHandled == 0 && SystemAPI.TryGetSingleton(out GodgameHeadlessExitRequest request))
            {
                _exitHandled = 1;
                var exitTick = request.RequestedTick != 0 ? request.RequestedTick : tick;
                GodgameHeadlessDiagnostics.UpdateProgress("shutdown", "exit_request", exitTick);
                GodgameHeadlessDiagnostics.WriteInvariantsForExit(state.EntityManager, request.ExitCode, exitTick);
                GodgameHeadlessDiagnostics.ShutdownWriter();

                if (request.ExitCode != 0 && request.ExitCode != GodgameHeadlessDiagnostics.TestFailExitCode)
                {
                    request.ExitCode = GodgameHeadlessDiagnostics.TestFailExitCode;
                    request.RequestedTick = exitTick;
                    var requestEntity = SystemAPI.GetSingletonEntity<GodgameHeadlessExitRequest>();
                    state.EntityManager.SetComponentData(requestEntity, request);
                }
            }
        }

        private bool TryResolveTick(ref SystemState state, out uint tick, out float fixedDt)
        {
            tick = 0;
            fixedDt = 0f;

            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tick = tickTimeState.Tick;
            }

            if (SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                if (timeState.Tick > tick)
                {
                    tick = timeState.Tick;
                }

                fixedDt = timeState.FixedDeltaTime;
            }

            if (tick == 0 && SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenarioTick))
            {
                tick = scenarioTick.Tick;
            }

            return tick != 0 || fixedDt > 0f || SystemAPI.HasSingleton<ScenarioRunnerTick>();
        }

        private void UpdateProgress(ref SystemState state, uint tick)
        {
            if (SystemAPI.TryGetSingleton<ScenarioState>(out var scenario))
            {
                if (scenario.BootPhase != _lastBootPhase)
                {
                    _lastBootPhase = scenario.BootPhase;
                    GodgameHeadlessDiagnostics.UpdateProgress("boot",
                        $"boot_{scenario.BootPhase.ToString().ToLowerInvariant()}",
                        tick);
                }
            }

            if (SystemAPI.TryGetSingleton<GodgameScenarioRuntime>(out var runtime))
            {
                if (_runtimeSeen == 0)
                {
                    _runtimeSeen = 1;
                    GodgameHeadlessDiagnostics.UpdateProgress("spawn", "spawning", tick);
                }

                if (_runStarted == 0 && runtime.HasSpawned != 0)
                {
                    _runStarted = 1;
                    GodgameHeadlessDiagnostics.UpdateProgress("run", "spawned", tick);
                }
            }
        }

        private void UpdateHeartbeat(uint tick)
        {
            if (tick == 0)
            {
                return;
            }

            if (_lastHeartbeatTick != 0 && tick >= _lastHeartbeatTick && tick - _lastHeartbeatTick < HeartbeatIntervalTicks)
            {
                return;
            }

            _lastHeartbeatTick = tick;
            GodgameHeadlessDiagnostics.UpdateProgress("run", "heartbeat", tick);
        }
    }
}
