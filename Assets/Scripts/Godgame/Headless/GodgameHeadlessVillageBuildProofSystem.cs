using System;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless proof that a village-issued build request results in at least one completed jobsite.
    /// Disabled by default; enable via <see cref="EnabledEnv"/>.
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.LateSimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessVillageBuildProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_VILLAGE_BUILD_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_VILLAGE_BUILD_PROOF_EXIT";
        private const uint DefaultTimeoutTicks = 1800; // ~30 seconds at 60hz

        private byte _done;
        private uint _startTick;
        private uint _timeoutTick;

        private static readonly FixedString32Bytes Expected = new FixedString32Bytes(">=1");
        private static readonly FixedString32Bytes Step = new FixedString32Bytes("village_build");

        public void OnCreate(ref SystemState state)
        {
            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            if (!RuntimeMode.IsHeadless && !string.Equals(enabled, "1", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            EnsureHeadlessBuildSliceConfig(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_done != 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            if (_timeoutTick == 0u)
            {
                _startTick = timeState.Tick;
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            var completedCount = 0;
            foreach (var (flags, site) in SystemAPI.Query<RefRO<ConstructionSiteFlags>, RefRO<VillageConstructionSite>>())
            {
                if ((flags.ValueRO.Value & ConstructionSiteFlags.Completed) != 0)
                {
                    completedCount = 1;
                    break;
                }
            }

            if (completedCount > 0)
            {
                _done = 1;
                UnityDebug.Log($"[GodgameHeadlessVillageBuildProof] PASS tick={timeState.Tick} completedSites={completedCount} timeoutTicks={DefaultTimeoutTicks}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Construction, true, completedCount, Expected, DefaultTimeoutTicks, step: Step);
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                UnityDebug.LogError($"[GodgameHeadlessVillageBuildProof] FAIL tick={timeState.Tick} completedSites={completedCount} timeoutTicks={DefaultTimeoutTicks}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Construction, false, completedCount, Expected, DefaultTimeoutTicks, step: Step);
                ExitIfRequested(ref state, timeState.Tick, 5);
            }
        }

        private static void EnsureHeadlessBuildSliceConfig(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<VillageBuildSliceConfig>());
            if (query.IsEmptyIgnoreFilter)
            {
                var entity = entityManager.CreateEntity(typeof(VillageBuildSliceConfig));
                entityManager.SetComponentData(entity, new VillageBuildSliceConfig { EnableInHeadless = 1 });
                return;
            }

            var existing = query.GetSingletonEntity();
            var config = entityManager.GetComponentData<VillageBuildSliceConfig>(existing);
            if (config.EnableInHeadless == 0)
            {
                config.EnableInHeadless = 1;
                entityManager.SetComponentData(existing, config);
            }
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
