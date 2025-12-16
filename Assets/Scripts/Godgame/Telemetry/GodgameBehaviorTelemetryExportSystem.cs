using System;
using System.IO;
using System.Text;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using Unity.Entities;
using UnityEngine;
using SystemEnv = System.Environment;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Streams aggregated behavior telemetry records to newline-delimited JSON for headless runs.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GodgameBehaviorTelemetryExportSystem : ISystem
    {
        private EntityQuery _stateQuery;
        private static bool s_loggedPath;
        private static bool s_loggedEmpty;

        public void OnCreate(ref SystemState state)
        {
            _stateQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<BehaviorTelemetryState>(),
                    ComponentType.ReadWrite<BehaviorTelemetryRecord>()
                }
            });

            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<BehaviorTelemetryState>();
        }

        public void OnDestroy(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var exportPath = SystemEnv.GetEnvironmentVariable("GODGAME_BEHAVIOR_TELEMETRY_PATH");
            if (string.IsNullOrWhiteSpace(exportPath))
            {
                return;
            }
            else if (!s_loggedPath)
            {
                Debug.Log($"[GodgameTelemetry] Export path set to '{exportPath}'");
                s_loggedPath = true;
            }

            if (!_stateQuery.TryGetSingletonBuffer(out DynamicBuffer<BehaviorTelemetryRecord> buffer) ||
                buffer.Length == 0)
            {
                if (!s_loggedEmpty)
                {
                    Debug.Log("[GodgameTelemetry] Telemetry buffer empty this frame");
                    s_loggedEmpty = true;
                }
                return;
            }
            s_loggedEmpty = false;

            try
            {
                var directory = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = File.Open(exportPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        WriteRecord(writer, buffer[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Godgame] Failed to export behavior telemetry to '{exportPath}': {ex}");
            }
            finally
            {
                buffer.Clear();
            }
        }

        private static void WriteRecord(StreamWriter writer, in BehaviorTelemetryRecord record)
        {
            writer.Write("{\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"behaviorId\":");
            writer.Write((ushort)record.Behavior);
            writer.Write(",\"behaviorKind\":");
            writer.Write((byte)record.Kind);
            writer.Write(",\"metricId\":");
            writer.Write(record.MetricOrInvariantId);
            writer.Write(",\"valueA\":");
            writer.Write(record.ValueA);
            writer.Write(",\"valueB\":");
            writer.Write(record.ValueB);
            writer.Write(",\"passed\":");
            writer.Write(record.Passed);
            writer.WriteLine("}");
        }
    }
}
