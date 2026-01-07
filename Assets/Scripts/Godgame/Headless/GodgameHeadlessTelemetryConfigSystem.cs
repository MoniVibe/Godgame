using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Headless
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Telemetry.TelemetryExportBootstrapSystem))]
    public partial struct GodgameHeadlessTelemetryConfigSystem : ISystem
    {
        private byte _applied;

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

            state.RequireForUpdate<TelemetryExportConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_applied != 0)
            {
                return;
            }

            var config = SystemAPI.GetSingletonRW<TelemetryExportConfig>();
            if (!GodgameHeadlessDiagnostics.TelemetryEnabled)
            {
                config.ValueRW.Enabled = 0;
                config.ValueRW.OutputPath = default;
                config.ValueRW.Version++;
                _applied = 1;
                return;
            }

            if (!string.IsNullOrWhiteSpace(GodgameHeadlessDiagnostics.TelemetryPath))
            {
                config.ValueRW.OutputPath = new FixedString512Bytes(GodgameHeadlessDiagnostics.TelemetryPath);
                config.ValueRW.Enabled = 1;
                config.ValueRW.Version++;
            }

            _applied = 1;
        }
    }
}
