using System;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Entities;
using UnityEngine;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessSmokeExitSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_SMOKE_EXIT";
        private const string MinTicksEnv = "GODGAME_HEADLESS_SMOKE_MIN_TICKS";
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string SmokeScenarioFile = "godgame_smoke.json";
        private const uint DefaultMinTicks = 1800;

        private byte _enabled;
        private byte _resolvedScenario;
        private byte _isSmokeScenario;
        private uint _startTick;
        private uint _minTicks;

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
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0)
            {
                return;
            }

            ResolveScenario();
            if (_isSmokeScenario == 0)
            {
                _enabled = 0;
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (_startTick == 0)
            {
                _startTick = timeState.Tick;
                _minTicks = ResolveMinTicks();
            }

            if (timeState.Tick - _startTick < _minTicks)
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, timeState.Tick, 0);
            _enabled = 0;
        }

        private void ResolveScenario()
        {
            if (_resolvedScenario != 0)
            {
                return;
            }

            _resolvedScenario = 1;
            var scenarioPath = SystemEnv.GetEnvironmentVariable(ScenarioPathEnv);
            if (!string.IsNullOrWhiteSpace(scenarioPath) &&
                scenarioPath.EndsWith(SmokeScenarioFile, StringComparison.OrdinalIgnoreCase))
            {
                _isSmokeScenario = 1;
            }
        }

        private static uint ResolveMinTicks()
        {
            var raw = SystemEnv.GetEnvironmentVariable(MinTicksEnv);
            if (!string.IsNullOrWhiteSpace(raw) && uint.TryParse(raw, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return DefaultMinTicks;
        }
    }
}
