using Godgame.Scenario;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Systems.Telemetry;
using Unity.Entities;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    public struct GodgameHeadlessExitRequest : IComponentData
    {
        public int ExitCode;
        public uint RequestedTick;
    }

    /// <summary>
    /// Defers <see cref="Application.Quit(int)"/> until after the telemetry export pass has had a chance to flush.
    /// This prevents losing one-shot proof events when headless runs exit immediately.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(TelemetryExportSystem))]
    public partial struct GodgameHeadlessExitSystem : ISystem
    {
        private const string ExitMinTickEnv = "GODGAME_HEADLESS_EXIT_MIN_TICK";
        private byte _quitIssued;
        private byte _loggedExitMinTick;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<GodgameHeadlessExitRequest>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_quitIssued != 0)
            {
                return;
            }

            var minTickRaw = SystemEnv.GetEnvironmentVariable(ExitMinTickEnv);
            if (!string.IsNullOrWhiteSpace(minTickRaw) &&
                uint.TryParse(minTickRaw, out var minTick))
            {
                var currentTick = 0u;
                var tickSource = "none";
                if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
                {
                    currentTick = tickTimeState.Tick;
                    tickSource = "tickTime";
                }
                else if (SystemAPI.TryGetSingleton<TimeState>(out var timeState))
                {
                    currentTick = timeState.Tick;
                    tickSource = "time";
                }

                if (_loggedExitMinTick == 0)
                {
                    _loggedExitMinTick = 1;
                    UnityDebug.Log($"[GodgameHeadlessExitSystem] ExitMinTick={minTick} currentTick={currentTick} source={tickSource}");
                }

                if (tickSource == "none" || currentTick < minTick)
                {
                    return;
                }
            }

            foreach (var request in SystemAPI.Query<RefRO<GodgameHeadlessExitRequest>>())
            {
                _quitIssued = 1;
                UnityDebug.Log($"[GodgameHeadlessExitSystem] Quit requested (code={request.ValueRO.ExitCode}, tick={request.ValueRO.RequestedTick}); quitting.");
                Quit(request.ValueRO.ExitCode);
                break;
            }
        }

        private static void Quit(int exitCode)
        {
#if UNITY_EDITOR
            if (Application.isEditor && Application.isBatchMode)
            {
                UnityEditor.EditorApplication.Exit(exitCode);
                return;
            }
#endif
            Application.Quit(exitCode);
        }

        public static void Request(ref SystemState state, uint tick, int exitCode)
        {
            var entityManager = state.EntityManager;
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<GodgameHeadlessExitRequest>());
            if (query.IsEmptyIgnoreFilter)
            {
                var entity = entityManager.CreateEntity(typeof(GodgameHeadlessExitRequest));
                entityManager.SetComponentData(entity, new GodgameHeadlessExitRequest
                {
                    ExitCode = exitCode,
                    RequestedTick = tick
                });
                return;
            }

            var existing = query.GetSingletonEntity();
            var request = entityManager.GetComponentData<GodgameHeadlessExitRequest>(existing);
            if (request.ExitCode != 0 && exitCode == 0)
            {
                return;
            }

            request.ExitCode = request.ExitCode != 0 ? request.ExitCode : exitCode;
            request.RequestedTick = tick;
            entityManager.SetComponentData(existing, request);
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameHeadlessDiagnosticsSystem))]
    public partial struct GodgameHeadlessScenarioQuitSystem : ISystem
    {
        private byte _startTickSet;
        private uint _startTick;

        public void OnCreate(ref SystemState state)
        {
            RuntimeMode.RefreshFromEnvironment();
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!TryResolveTick(ref state, out var tick, out var fixedDt))
            {
                return;
            }

            if (!TryResolveRunTicks(ref state, fixedDt, out var runTicks, out var hasSpawned))
            {
                return;
            }

            if (runTicks == 0 || hasSpawned == 0)
            {
                return;
            }

            if (_startTickSet == 0)
            {
                _startTickSet = 1;
                _startTick = tick;
            }

            var endTick = _startTick + runTicks;
            if (tick < endTick)
            {
                return;
            }

            if (SystemAPI.HasSingleton<GodgameHeadlessExitRequest>())
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, 0);
        }

        private static bool TryResolveTick(ref SystemState state, out uint tick, out float fixedDt)
        {
            tick = 0;
            fixedDt = 0f;

            if (SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenarioTick))
            {
                tick = scenarioTick.Tick;
            }

            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                if (tickTimeState.Tick > tick)
                {
                    tick = tickTimeState.Tick;
                }
                fixedDt = tickTimeState.FixedDeltaTime;
            }

            if (SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                if (timeState.Tick > tick)
                {
                    tick = timeState.Tick;
                }

                if (fixedDt <= 0f)
                {
                    fixedDt = timeState.FixedDeltaTime;
                }
            }

            return tick != 0 || fixedDt > 0f || SystemAPI.HasSingleton<ScenarioRunnerTick>();
        }

        private static bool TryResolveRunTicks(ref SystemState state, float fixedDt, out uint runTicks, out byte hasSpawned)
        {
            runTicks = 0;
            hasSpawned = 1;

            if (SystemAPI.TryGetSingleton<ScenarioInfo>(out var scenarioInfo) && scenarioInfo.RunTicks > 0)
            {
                runTicks = (uint)scenarioInfo.RunTicks;
            }

            if (SystemAPI.TryGetSingleton<GodgameScenarioRuntime>(out var runtime))
            {
                hasSpawned = runtime.HasSpawned;
                if (runtime.RunTicks > 0)
                {
                    runTicks = runtime.RunTicks;
                }
                else if (runtime.DurationSeconds > 0f && fixedDt > 0f)
                {
                    runTicks = (uint)Mathf.CeilToInt(runtime.DurationSeconds / fixedDt);
                }
            }

            return runTicks > 0;
        }
    }
}
