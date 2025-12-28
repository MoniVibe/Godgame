using System;
using System.Globalization;
using System.IO;
using System.Text;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using SystemEnv = System.Environment;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Adapts the GODGAME_BEHAVIOR_TELEMETRY_PATH env var into the shared telemetry export config so
    /// PureDOTS' TelemetryExportSystem can flush metrics, frame timings, and behavior records.
    /// </summary>
    [UpdateInGroup(typeof(TelemetryExportSystemGroup))]
    public partial struct GodgameBehaviorTelemetryExportSystem : ISystem
    {
        private bool _loggedPath;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TelemetryExportConfig>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var exportPath = SystemEnv.GetEnvironmentVariable("GODGAME_BEHAVIOR_TELEMETRY_PATH");
            var configEntity = SystemAPI.GetSingletonEntity<TelemetryExportConfig>();
            var config = SystemAPI.GetComponent<TelemetryExportConfig>(configEntity);
            var desiredFlags =
                TelemetryExportFlags.IncludeTelemetryMetrics |
                TelemetryExportFlags.IncludeFrameTiming |
                TelemetryExportFlags.IncludeBehaviorTelemetry |
                TelemetryExportFlags.IncludeReplayEvents;

            if (string.IsNullOrWhiteSpace(exportPath))
            {
                _loggedPath = false;
                return;
            }

            var fixedPath = ToFixedString(exportPath);
            bool needsUpdate = config.Enabled == 0 ||
                               !config.OutputPath.Equals(fixedPath) ||
                               (config.Flags & desiredFlags) != desiredFlags;

            if (!needsUpdate)
            {
                return;
            }

            config.OutputPath = fixedPath;
            config.Flags = desiredFlags;
            config.Enabled = 1;
            config.Version++;
            SystemAPI.SetComponent(configEntity, config);

            if (!_loggedPath)
            {
                Debug.Log($"[GodgameTelemetry] Export path set to '{exportPath}'");
                _loggedPath = true;
            }
        }

        private static FixedString512Bytes ToFixedString(string path)
        {
            FixedString512Bytes result = default;
            if (string.IsNullOrEmpty(path))
            {
                return result;
            }

            for (int i = 0; i < path.Length && i < result.Capacity; i++)
            {
                result.Append(path[i]);
            }

            return result;
        }
    }

    /// <summary>
    /// Flushes AI audit buffers (decisions, actions, queues, logic, tickets) to the shared NDJSON stream
    /// so downstream tools can answer "why did they do that?" without replaying a scene.
    /// </summary>
    [UpdateInGroup(typeof(PureDOTS.Systems.PureDotsPresentationSystemGroup))]
    [UpdateAfter(typeof(TelemetryExportSystemGroup))]
    public partial struct GodgameAIAuditExportSystem : ISystem
    {
        private bool _capRecordWritten;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TelemetryExportConfig>();
            EnsureExportStateExists(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var configEntity = SystemAPI.GetSingletonEntity<TelemetryExportConfig>();
            var config = SystemAPI.GetComponent<TelemetryExportConfig>(configEntity);

            if (config.Enabled == 0 || config.OutputPath.Length == 0 || config.RunId.Length == 0)
            {
                return;
            }

            var exportState = SystemAPI.GetSingletonRW<TelemetryExportState>();
            if (!exportState.ValueRO.RunId.Equals(config.RunId) || exportState.ValueRO.MaxOutputBytes != config.MaxOutputBytes)
            {
                ResetExportState(ref exportState.ValueRW, config);
                _capRecordWritten = false;
            }

            if (exportState.ValueRO.CapReached != 0)
            {
                return;
            }

            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            var entityManager = state.EntityManager;

            var hasDecisions = TryGetNonEmptyBuffer<GodgameDecisionTransitionRecord>(telemetryEntity, entityManager, out var decisionBuffer);
            var hasActions = TryGetNonEmptyBuffer<GodgameActionLifecycleRecord>(telemetryEntity, entityManager, out var actionBuffer);
            var hasQueues = TryGetNonEmptyBuffer<GodgameQueuePressureRecord>(telemetryEntity, entityManager, out var queueBuffer);
            var hasLogicAudits = TryGetNonEmptyBuffer<GodgameLogicAuditRecord>(telemetryEntity, entityManager, out var logicBuffer);
            var hasTicketAudits = TryGetNonEmptyBuffer<GodgameTicketClaimRecord>(telemetryEntity, entityManager, out var ticketBuffer);
            var hasDecisionTraces = TryGetNonEmptyBuffer<GodgameDecisionTraceRecord>(telemetryEntity, entityManager, out var traceBuffer);
            var hasActionEffects = TryGetNonEmptyBuffer<GodgameActionEffectRecord>(telemetryEntity, entityManager, out var effectBuffer);
            var hasGatherAttempts = TryGetNonEmptyBuffer<GodgameGatherAttemptRecord>(telemetryEntity, entityManager, out var gatherAttemptBuffer);
            var hasGatherYields = TryGetNonEmptyBuffer<GodgameGatherYieldRecord>(telemetryEntity, entityManager, out var gatherYieldBuffer);
            var hasGatherFailures = TryGetNonEmptyBuffer<GodgameGatherFailureRecord>(telemetryEntity, entityManager, out var gatherFailureBuffer);
            var hasHaulTrips = TryGetNonEmptyBuffer<GodgameHaulTripRecord>(telemetryEntity, entityManager, out var haulTripBuffer);
            var hasCapabilityGrants = TryGetNonEmptyBuffer<GodgameCapabilityGrantedRecord>(telemetryEntity, entityManager, out var capabilityGrantBuffer);
            var hasCapabilityRevokes = TryGetNonEmptyBuffer<GodgameCapabilityRevokedRecord>(telemetryEntity, entityManager, out var capabilityRevokeBuffer);
            var hasCapabilitySnapshots = TryGetNonEmptyBuffer<GodgameCapabilitySnapshotRecord>(telemetryEntity, entityManager, out var capabilitySnapshotBuffer);
            var hasCapabilityUsage = TryGetNonEmptyBuffer<GodgameCapabilityUsageSample>(telemetryEntity, entityManager, out var capabilityUsageBuffer);
            var hasActionFailureSamples = TryGetNonEmptyBuffer<GodgameActionFailureSample>(telemetryEntity, entityManager, out var actionFailureBuffer);
            var hasLiveliness = TryGetNonEmptyBuffer<GodgameVillagerLivelinessRecord>(telemetryEntity, entityManager, out var livelinessBuffer);

            var summaryAvailable = entityManager.HasComponent<GodgameTelemetrySummary>(telemetryEntity);
            var summary = summaryAvailable ? entityManager.GetComponentData<GodgameTelemetrySummary>(telemetryEntity) : default;
            var summaryDirty = summaryAvailable && summary.SummaryDirty != 0;

            if (!hasDecisions && !hasActions && !hasDecisionTraces && !hasActionEffects && !hasQueues &&
                !hasLogicAudits && !hasTicketAudits && !hasGatherAttempts && !hasGatherYields &&
                !hasGatherFailures && !hasHaulTrips && !hasCapabilityGrants && !hasCapabilityRevokes &&
                !hasCapabilitySnapshots && !hasCapabilityUsage && !hasActionFailureSamples &&
                !hasLiveliness && !summaryDirty)
            {
                return;
            }

            var runId = config.RunId.ToString();
            var outputPath = config.OutputPath.ToString();
            ulong maxBytes = config.MaxOutputBytes;
            var tick = GetCurrentTick(ref state);
            ResolveScenarioMetadata(ref state, out var scenarioId, out var scenarioSeed);

            LineBuffer.Get(out var lineBuilder, out var lineWriter);
            string truncatedRecord = null;
            ulong reserveBytes = 0;
            if (maxBytes > 0)
            {
                truncatedRecord = BuildTruncatedRecord(lineBuilder, lineWriter, runId, scenarioId, scenarioSeed, tick, maxBytes);
                reserveBytes = (ulong)Encoding.UTF8.GetByteCount(truncatedRecord);
                if (reserveBytes >= maxBytes)
                {
                    exportState.ValueRW.BytesWritten = exportState.ValueRO.BytesWritten;
                    exportState.ValueRW.MaxOutputBytes = maxBytes;
                    exportState.ValueRW.CapReached = 1;
                    return;
                }
            }

            try
            {
                EnsureDirectory(outputPath);
                ulong bytesWritten = exportState.ValueRO.BytesWritten;
                var fileMode = bytesWritten == 0 ? FileMode.Create : FileMode.OpenOrCreate;
                using var stream = new FileStream(outputPath, fileMode, FileAccess.Write, FileShare.Read);
                if (bytesWritten > 0)
                {
                    stream.Position = stream.Length;
                    if ((ulong)stream.Position != bytesWritten)
                    {
                        bytesWritten = (ulong)stream.Position;
                    }
                }
                else
                {
                    stream.Position = 0;
                }

                using var writer = new StreamWriter(stream, Encoding.UTF8) { NewLine = "\n" };
                var culture = CultureInfo.InvariantCulture;

                if (maxBytes > 0 && bytesWritten >= maxBytes)
                {
                    HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                    return;
                }

                if (hasDecisions)
                {
                    for (int i = 0; i < decisionBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteDecisionRecord(recordWriter, runId, decisionBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    decisionBuffer.Clear();
                }

                if (hasDecisionTraces)
                {
                    for (int i = 0; i < traceBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteDecisionTraceRecord(recordWriter, runId, traceBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    traceBuffer.Clear();
                }

                if (hasActions)
                {
                    for (int i = 0; i < actionBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteActionRecord(recordWriter, runId, actionBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    actionBuffer.Clear();
                }

                if (hasActionEffects)
                {
                    for (int i = 0; i < effectBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteActionEffectRecord(recordWriter, runId, effectBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    effectBuffer.Clear();
                }

                if (hasLiveliness)
                {
                    for (int i = 0; i < livelinessBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteLivelinessRecord(recordWriter, runId, livelinessBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    livelinessBuffer.Clear();
                }

                if (hasCapabilityGrants)
                {
                    for (int i = 0; i < capabilityGrantBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteCapabilityGrantRecord(recordWriter, runId, capabilityGrantBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    capabilityGrantBuffer.Clear();
                }

                if (hasCapabilityRevokes)
                {
                    for (int i = 0; i < capabilityRevokeBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteCapabilityRevokeRecord(recordWriter, runId, capabilityRevokeBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    capabilityRevokeBuffer.Clear();
                }

                if (hasCapabilitySnapshots)
                {
                    for (int i = 0; i < capabilitySnapshotBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteCapabilitySnapshotRecord(recordWriter, runId, capabilitySnapshotBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    capabilitySnapshotBuffer.Clear();
                }

                if (hasCapabilityUsage)
                {
                    for (int i = 0; i < capabilityUsageBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteCapabilityUsageRecord(recordWriter, runId, capabilityUsageBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    capabilityUsageBuffer.Clear();
                }

                if (hasQueues)
                {
                    for (int i = 0; i < queueBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteQueueRecord(recordWriter, runId, queueBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    queueBuffer.Clear();
                }

                if (hasLogicAudits)
                {
                    for (int i = 0; i < logicBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteLogicRecord(recordWriter, runId, logicBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    logicBuffer.Clear();
                }

                if (hasTicketAudits)
                {
                    for (int i = 0; i < ticketBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteTicketRecord(recordWriter, runId, ticketBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    ticketBuffer.Clear();
                }

                if (hasGatherAttempts)
                {
                    for (int i = 0; i < gatherAttemptBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteGatherAttemptRecord(recordWriter, runId, gatherAttemptBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    gatherAttemptBuffer.Clear();
                }

                if (hasGatherYields)
                {
                    for (int i = 0; i < gatherYieldBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteGatherYieldRecord(recordWriter, runId, gatherYieldBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    gatherYieldBuffer.Clear();
                }

                if (hasGatherFailures)
                {
                    for (int i = 0; i < gatherFailureBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteGatherFailureRecord(recordWriter, runId, gatherFailureBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    gatherFailureBuffer.Clear();
                }

                if (hasHaulTrips)
                {
                    for (int i = 0; i < haulTripBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteHaulTripRecord(recordWriter, runId, haulTripBuffer[i], culture)))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    haulTripBuffer.Clear();
                }

                if (hasActionFailureSamples)
                {
                    for (int i = 0; i < actionFailureBuffer.Length; i++)
                    {
                        if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                            WriteActionFailureSample(recordWriter, runId, actionFailureBuffer[i])))
                        {
                            HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                            return;
                        }
                    }
                    actionFailureBuffer.Clear();
                }

                if (summaryDirty)
                {
                    if (!TryWriteRecord(lineBuilder, lineWriter, writer, ref bytesWritten, maxBytes, reserveBytes, recordWriter =>
                        WriteSummaryRecord(recordWriter, runId, summary, culture)))
                    {
                        HandleCapReached(writer, ref bytesWritten, maxBytes, truncatedRecord, reserveBytes, ref exportState.ValueRW);
                        return;
                    }
                    summary.SummaryDirty = 0;
                    entityManager.SetComponentData(telemetryEntity, summary);
                }

                writer.Flush();
                exportState.ValueRW.BytesWritten = bytesWritten;
                exportState.ValueRW.MaxOutputBytes = maxBytes;
                exportState.ValueRW.RunId = config.RunId;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GodgameTelemetry] Failed to export AI audit telemetry: {ex}");
            }
        }

        private static bool TryGetNonEmptyBuffer<T>(Entity entity, EntityManager entityManager, out DynamicBuffer<T> buffer)
            where T : unmanaged, IBufferElementData
        {
            if (entityManager.HasBuffer<T>(entity))
            {
                buffer = entityManager.GetBuffer<T>(entity);
                return buffer.Length > 0;
            }

            buffer = default;
            return false;
        }

        private static void EnsureExportStateExists(ref SystemState state)
        {
            using var query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<TelemetryExportState>());
            if (query.IsEmpty)
            {
                state.EntityManager.CreateEntity(typeof(TelemetryExportState));
            }
        }

        private static void ResetExportState(ref TelemetryExportState state, in TelemetryExportConfig config)
        {
            state.RunId = config.RunId;
            state.BytesWritten = 0;
            state.MaxOutputBytes = config.MaxOutputBytes;
            state.CapReached = 0;
        }

        private static bool TryWriteRecord(StringBuilder lineBuilder, StringWriter lineWriter, StreamWriter writer, ref ulong bytesWritten, ulong maxBytes, ulong reserveBytes, Action<TextWriter> writeRecord)
        {
            lineBuilder.Clear();
            writeRecord(lineWriter);
            var line = lineBuilder.ToString();
            var recordBytes = (ulong)Encoding.UTF8.GetByteCount(line);
            if (maxBytes > 0 && bytesWritten + recordBytes + reserveBytes > maxBytes)
            {
                return false;
            }

            writer.Write(line);
            bytesWritten += recordBytes;
            return true;
        }

        private static string BuildTruncatedRecord(StringBuilder lineBuilder, StringWriter lineWriter, string runId, string scenarioId, uint scenarioSeed, uint tick, ulong maxBytes)
        {
            lineBuilder.Clear();
            lineWriter.Write("{\"type\":\"telemetryTruncated\",\"runId\":\"");
            WriteEscapedString(lineWriter, runId);
            lineWriter.Write("\",\"scenario\":\"");
            WriteEscapedString(lineWriter, scenarioId);
            lineWriter.Write("\",\"seed\":");
            lineWriter.Write(scenarioSeed);
            lineWriter.Write(",\"tick\":");
            lineWriter.Write(tick);
            lineWriter.Write(",\"maxBytes\":");
            lineWriter.Write(maxBytes);
            lineWriter.WriteLine("}");
            return lineBuilder.ToString();
        }

        private void HandleCapReached(StreamWriter writer, ref ulong bytesWritten, ulong maxBytes, string truncatedRecord, ulong truncatedBytes, ref TelemetryExportState exportState)
        {
            if (exportState.CapReached == 0)
            {
                if (!_capRecordWritten && !string.IsNullOrEmpty(truncatedRecord))
                {
                    var recordBytes = truncatedBytes > 0 ? truncatedBytes : (ulong)Encoding.UTF8.GetByteCount(truncatedRecord);
                    if (maxBytes == 0 || bytesWritten + recordBytes <= maxBytes)
                    {
                        writer.Write(truncatedRecord);
                        bytesWritten += recordBytes;
                        _capRecordWritten = true;
                    }
                }

                writer.Flush();
                Debug.LogWarning($"[GodgameTelemetry] Output cap reached ({bytesWritten} of {maxBytes} bytes). Telemetry export paused.");
            }

            exportState.BytesWritten = bytesWritten;
            exportState.MaxOutputBytes = maxBytes;
            exportState.CapReached = 1;
        }

        private uint GetCurrentTick(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenarioTick) && scenarioTick.Tick > 0)
            {
                return scenarioTick.Tick;
            }

            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickState))
            {
                var tick = tickState.Tick;
                if (SystemAPI.TryGetSingleton<TimeState>(out var timeState) && timeState.Tick > tick)
                {
                    tick = timeState.Tick;
                }

                if (tick == 0 && Application.isBatchMode)
                {
                    var dt = (float)SystemAPI.Time.DeltaTime;
                    var elapsed = (float)SystemAPI.Time.ElapsedTime;
                    if (dt > 0f && elapsed > 0f)
                    {
                        var elapsedTick = (uint)(elapsed / dt);
                        if (elapsedTick > tick)
                        {
                            tick = elapsedTick;
                        }
                    }
                }

                return tick;
            }

            if (SystemAPI.TryGetSingleton<TimeState>(out var legacyTime))
            {
                var tick = legacyTime.Tick;
                if (tick == 0 && Application.isBatchMode)
                {
                    var dt = (float)SystemAPI.Time.DeltaTime;
                    var elapsed = (float)SystemAPI.Time.ElapsedTime;
                    if (dt > 0f && elapsed > 0f)
                    {
                        var elapsedTick = (uint)(elapsed / dt);
                        if (elapsedTick > tick)
                        {
                            tick = elapsedTick;
                        }
                    }
                }

                return tick;
            }

            return 0;
        }

        private void ResolveScenarioMetadata(ref SystemState state, out string scenarioId, out uint scenarioSeed)
        {
            scenarioSeed = 0;
            scenarioId = string.Empty;
            if (SystemAPI.TryGetSingleton<ScenarioInfo>(out var scenarioInfo))
            {
                scenarioSeed = scenarioInfo.Seed;
                scenarioId = scenarioInfo.ScenarioId.ToString();
            }
        }

        private static void WriteDecisionRecord(TextWriter writer, string runId, in GodgameDecisionTransitionRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiDecisionTransition\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"oldState\":");
            writer.Write(record.OldState);
            writer.Write(",\"newState\":");
            writer.Write(record.NewState);
            writer.Write(",\"reasonCode\":\"");
            WriteEscapedString(writer, record.Reason.ToString());
            writer.Write("\",\"priority\":");
            writer.Write(record.Priority);

            var targetLabel = BuildEntityLabel(record.Target);
            if (!string.IsNullOrEmpty(targetLabel))
            {
                writer.Write(",\"targetEntityId\":\"");
                WriteEscapedString(writer, targetLabel);
                writer.Write("\"");
            }

            writer.Write(",\"scores\":[");
            for (int i = 0; i < record.Scores.Length; i++)
            {
                var entry = record.Scores[i];
                if (i > 0)
                {
                    writer.Write(',');
                }

                writer.Write("{\"label\":\"");
                WriteEscapedString(writer, entry.Label.ToString());
                writer.Write("\",\"score\":");
                writer.Write(entry.Score.ToString("R", culture));
                writer.Write('}');
            }
            writer.WriteLine("]}");
        }

        private static void WriteActionRecord(TextWriter writer, string runId, in GodgameActionLifecycleRecord record)
        {
            writer.Write("{\"type\":\"aiAction\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"actionId\":");
            writer.Write(record.ActionId);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"phaseId\":");
            writer.Write((byte)record.Phase);
            writer.Write(",\"phase\":\"");
            WriteEscapedString(writer, record.Phase.ToString());
            writer.Write("\",\"event\":\"");
            WriteEscapedString(writer, record.Event.ToString());
            writer.Write("\",\"durationTicks\":");
            writer.Write(record.DurationTicks);

            if (record.FailureReason != GodgameActionFailureReason.None)
            {
                writer.Write(",\"failureReason\":\"");
                WriteEscapedString(writer, record.FailureReason.ToString());
                writer.Write("\"");
            }

            var targetLabel = BuildEntityLabel(record.Target);
            if (!string.IsNullOrEmpty(targetLabel))
            {
                writer.Write(",\"targetEntityId\":\"");
                WriteEscapedString(writer, targetLabel);
                writer.Write("\"");
            }

            writer.WriteLine("}");
        }

        private static void WriteLivelinessRecord(TextWriter writer, string runId, in GodgameVillagerLivelinessRecord record)
        {
            writer.Write("{\"type\":\"aiLiveliness\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"event\":\"");
            WriteEscapedString(writer, record.Event.ToString());
            writer.Write("\"");

            if (record.CooldownMode != VillagerWorkCooldownMode.None)
            {
                writer.Write(",\"cooldownMode\":\"");
                WriteEscapedString(writer, record.CooldownMode.ToString());
                writer.Write("\"");
            }

            if (record.CooldownRemainingTicks > 0)
            {
                writer.Write(",\"cooldownRemainingTicks\":");
                writer.Write(record.CooldownRemainingTicks);
            }

            if (record.CooldownClearReason != VillagerCooldownClearReason.None)
            {
                writer.Write(",\"cooldownClearReason\":\"");
                WriteEscapedString(writer, record.CooldownClearReason.ToString());
                writer.Write("\"");
            }

            if (record.LeisureAction != VillagerWorkCooldownMode.None)
            {
                writer.Write(",\"leisureAction\":\"");
                WriteEscapedString(writer, record.LeisureAction.ToString());
                writer.Write("\"");
            }

            var leisureTargetLabel = BuildEntityLabel(record.LeisureTarget);
            if (!string.IsNullOrEmpty(leisureTargetLabel))
            {
                writer.Write(",\"leisureTargetEntityId\":\"");
                WriteEscapedString(writer, leisureTargetLabel);
                writer.Write("\"");
            }

            var targetLabel = BuildEntityLabel(record.Target);
            if (!string.IsNullOrEmpty(targetLabel))
            {
                writer.Write(",\"targetEntityId\":\"");
                WriteEscapedString(writer, targetLabel);
                writer.Write("\"");
            }

            writer.WriteLine("}");
        }

        private static void WriteQueueRecord(TextWriter writer, string runId, in GodgameQueuePressureRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiQueue\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"queue\":\"");
            WriteEscapedString(writer, record.QueueName.ToString());
            writer.Write("\",\"length\":");
            writer.Write(record.Length);
            writer.Write(",\"duplicates\":");
            writer.Write(record.DuplicateCount);
            writer.Write(",\"stale\":");
            writer.Write(record.StaleCount);
            writer.Write(",\"averageWaitTicks\":");
            writer.Write(record.AverageWaitTicks.ToString("R", culture));
            writer.WriteLine("}");
        }

        private static void WriteLogicRecord(TextWriter writer, string runId, in GodgameLogicAuditRecord record)
        {
            writer.Write("{\"type\":\"aiLogicAudit\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"kind\":\"");
            WriteEscapedString(writer, record.Kind.ToString());
            writer.Write("\",\"count\":");
            writer.Write(record.Count);

            var details = record.Details.ToString();
            if (!string.IsNullOrEmpty(details))
            {
                writer.Write(",\"details\":\"");
                WriteEscapedString(writer, details);
                writer.Write("\"");
            }

            writer.WriteLine("}");
        }

        private static void WriteTicketRecord(TextWriter writer, string runId, in GodgameTicketClaimRecord record)
        {
            writer.Write("{\"type\":\"aiTicketAudit\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"event\":\"");
            WriteEscapedString(writer, record.Event.ToString());
            writer.Write("\",\"ticketId\":");
            writer.Write(record.TicketId);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"durationTicks\":");
            writer.Write(record.DurationTicks);

            var details = record.Details.ToString();
            if (!string.IsNullOrEmpty(details))
            {
                writer.Write(",\"details\":\"");
                WriteEscapedString(writer, details);
                writer.Write("\"");
            }

            writer.WriteLine("}");
        }

        private static void WriteDecisionTraceRecord(TextWriter writer, string runId, in GodgameDecisionTraceRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiDecisionTrace\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"domain\":\"");
            WriteEscapedString(writer, record.Domain.ToString());
            writer.Write("\",\"chosenId\":\"");
            WriteEscapedString(writer, record.ChosenId.ToString());
            writer.Write("\",\"reasonCode\":");
            writer.Write(record.ReasonCode);
            writer.Write(",\"contextHash\":");
            writer.Write(record.ContextHash);
            writer.Write(",\"topChoices\":[");
            for (int i = 0; i < record.TopChoices.Length; i++)
            {
                var entry = record.TopChoices[i];
                if (i > 0)
                {
                    writer.Write(",");
                }

                writer.Write("{\"id\":\"");
                WriteEscapedString(writer, entry.Label.ToString());
                writer.Write("\",\"score\":");
                writer.Write(entry.Score.ToString("0.###", culture));
                writer.Write("}");
            }
            writer.WriteLine("]}");
        }

        private static void WriteActionEffectRecord(TextWriter writer, string runId, in GodgameActionEffectRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiActionEffect\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"actionId\":");
            writer.Write(record.ActionId);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"actionLabel\":\"");
            WriteEscapedString(writer, record.ActionLabel.ToString());
            writer.Write("\",\"deltaCargo\":");
            writer.Write(record.DeltaCargo.ToString("0.###", culture));
            writer.Write(",\"deltaResource\":");
            writer.Write(record.DeltaResource.ToString("0.###", culture));
            writer.Write(",\"deltaHealth\":");
            writer.Write(record.DeltaHealth.ToString("0.###", culture));
            writer.Write(",\"deltaThreat\":");
            writer.Write(record.DeltaThreat.ToString("0.###", culture));
            writer.WriteLine("}");
        }

        private static void WriteGatherAttemptRecord(TextWriter writer, string runId, in GodgameGatherAttemptRecord record)
        {
            writer.Write("{\"type\":\"aiGatherAttempt\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"methodId\":\"");
            WriteEscapedString(writer, record.MethodId.ToString());
            writer.Write("\",\"resourceType\":\"");
            WriteEscapedString(writer, record.ResourceType.ToString());
            writer.Write("\",\"nodeId\":\"");
            WriteEscapedString(writer, record.NodeId.ToString());
            writer.WriteLine("\"}");
        }

        private static void WriteGatherYieldRecord(TextWriter writer, string runId, in GodgameGatherYieldRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiGatherYield\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"methodId\":\"");
            WriteEscapedString(writer, record.MethodId.ToString());
            writer.Write("\",\"resourceType\":\"");
            WriteEscapedString(writer, record.ResourceType.ToString());
            writer.Write("\",\"amount\":");
            writer.Write(record.Amount.ToString("0.###", culture));
            writer.Write(",\"timeSpentTicks\":");
            writer.Write(record.TimeSpentTicks);
            writer.WriteLine("}");
        }

        private static void WriteGatherFailureRecord(TextWriter writer, string runId, in GodgameGatherFailureRecord record)
        {
            writer.Write("{\"type\":\"aiGatherFailure\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"methodId\":\"");
            WriteEscapedString(writer, record.MethodId.ToString());
            writer.Write("\",\"resourceType\":\"");
            WriteEscapedString(writer, record.ResourceType.ToString());
            writer.Write("\",\"reason\":\"");
            WriteEscapedString(writer, record.Reason.ToString());
            writer.WriteLine("\"}");
        }

        private static void WriteHaulTripRecord(TextWriter writer, string runId, in GodgameHaulTripRecord record, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiHaulTrip\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"startTick\":");
            writer.Write(record.StartTick);
            writer.Write(",\"endTick\":");
            writer.Write(record.EndTick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"carried\":");
            writer.Write(record.CarriedAmount.ToString("0.###", culture));
            writer.Write(",\"distance\":");
            writer.Write(record.Distance.ToString("0.###", culture));
            writer.Write(",\"congestion\":");
            writer.Write(record.Congestion.ToString("0.###", culture));
            writer.WriteLine("}");
        }

        private static void WriteCapabilityGrantRecord(TextWriter writer, string runId, in GodgameCapabilityGrantedRecord record)
        {
            writer.Write("{\"type\":\"aiCapabilityGranted\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"capabilityId\":\"");
            WriteEscapedString(writer, record.CapabilityId.ToString());
            writer.Write("\",\"source\":\"");
            WriteEscapedString(writer, record.Source.ToString());
            writer.Write("\",\"level\":");
            writer.Write(record.Level);
            writer.Write(",\"seedHash\":");
            writer.Write(record.SeedHash);
            if (record.SourceId.Length > 0)
            {
                writer.Write(",\"sourceId\":\"");
                WriteEscapedString(writer, record.SourceId.ToString());
                writer.Write("\"");
            }
            writer.WriteLine("}");
        }

        private static void WriteCapabilityRevokeRecord(TextWriter writer, string runId, in GodgameCapabilityRevokedRecord record)
        {
            writer.Write("{\"type\":\"aiCapabilityRevoked\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"capabilityId\":\"");
            WriteEscapedString(writer, record.CapabilityId.ToString());
            writer.Write("\",\"source\":\"");
            WriteEscapedString(writer, record.Source.ToString());
            writer.Write("\",\"level\":");
            writer.Write(record.Level);
            if (record.Reason.Length > 0)
            {
                writer.Write(",\"reason\":\"");
                WriteEscapedString(writer, record.Reason.ToString());
                writer.Write("\"");
            }
            writer.WriteLine("}");
        }

        private static void WriteCapabilitySnapshotRecord(TextWriter writer, string runId, in GodgameCapabilitySnapshotRecord record)
        {
            writer.Write("{\"type\":\"aiCapabilitySnapshot\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"tick\":");
            writer.Write(record.Tick);
            writer.Write(",\"agentId\":\"");
            WriteEscapedString(writer, record.AgentId.ToString());
            writer.Write("\",\"bitsetHash\":");
            writer.Write(record.BitsetHash);
            writer.Write(",\"level\":");
            writer.Write(record.Level);
            writer.Write(",\"experience\":");
            writer.Write(record.Experience);
            if (record.Context.Length > 0)
            {
                writer.Write(",\"context\":\"");
                WriteEscapedString(writer, record.Context.ToString());
                writer.Write("\"");
            }
            writer.WriteLine("}");
        }

        private static void WriteCapabilityUsageRecord(TextWriter writer, string runId, in GodgameCapabilityUsageSample record)
        {
            writer.Write("{\"type\":\"aiCapabilityUsage\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"capabilityId\":\"");
            WriteEscapedString(writer, record.CapabilityId.ToString());
            writer.Write("\",\"count\":");
            writer.Write(record.Count);
            writer.WriteLine("}");
        }

        private static void WriteActionFailureSample(TextWriter writer, string runId, in GodgameActionFailureSample record)
        {
            writer.Write("{\"type\":\"aiActionFailure\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"actionId\":\"");
            WriteEscapedString(writer, record.ActionId.ToString());
            writer.Write("\",\"reason\":\"");
            WriteEscapedString(writer, record.Reason.ToString());
            writer.Write("\",\"count\":");
            writer.Write(record.Count);
            writer.WriteLine("}");
        }

        private static void WriteSummaryRecord(TextWriter writer, string runId, in GodgameTelemetrySummary summary, CultureInfo culture)
        {
            writer.Write("{\"type\":\"aiSummary\",\"runId\":\"");
            WriteEscapedString(writer, runId);
            writer.Write("\",\"resourceTotal\":");
            writer.Write(summary.ResourceTotal.ToString("0.###", culture));
            writer.Write(",\"combatTotal\":");
            writer.Write(summary.CombatTotal.ToString("0.###", culture));
            writer.Write(",\"taskCount\":");
            writer.Write(summary.TaskCount);
            writer.Write(",\"starvationCount\":");
            writer.Write(summary.StarvationCount);
            writer.Write(",\"oscillationCount\":");
            writer.Write(summary.OscillationCount);
            writer.Write(",\"resourcesPerMinute\":");
            writer.Write(summary.ResourcesPerMinute.ToString("0.###", culture));
            writer.Write(",\"combatPerMinute\":");
            writer.Write(summary.CombatPerMinute.ToString("0.###", culture));
            writer.Write(",\"tasksPerMinute\":");
            writer.Write(summary.TasksPerMinute.ToString("0.###", culture));
            writer.Write(",\"capabilityUsageBudgetFailures\":");
            writer.Write(summary.CapabilityUsageBudgetFailures);
            writer.Write(",\"actionFailureBudgetFailures\":");
            writer.Write(summary.ActionFailureBudgetFailures);
            writer.Write(",\"budgetsFailed\":");
            writer.Write(summary.BudgetsFailed);
            writer.WriteLine("}");
        }

        private static string BuildEntityLabel(in Entity entity)
        {
            if (entity == Entity.Null)
            {
                return string.Empty;
            }

            return $"entity/{entity.Index}.{entity.Version}";
        }

        private static void EnsureDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void WriteEscapedString(TextWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '\\':
                        writer.Write("\\\\");
                        break;
                    case '"':
                        writer.Write("\\\"");
                        break;
                    case '\n':
                        writer.Write("\\n");
                        break;
                    case '\r':
                        writer.Write("\\r");
                        break;
                    case '\t':
                        writer.Write("\\t");
                        break;
                    default:
                        if (c < 32)
                        {
                            writer.Write($"\\u{(int)c:X4}");
                        }
                        else
                        {
                            writer.Write(c);
                        }
                        break;
                }
            }
        }

        private static class LineBuffer
        {
            [ThreadStatic] private static StringBuilder _builder;
            [ThreadStatic] private static StringWriter _writer;

            public static void Get(out StringBuilder builder, out StringWriter writer)
            {
                builder = _builder ??= new StringBuilder(512);
                writer = _writer ??= new StringWriter(builder, CultureInfo.InvariantCulture) { NewLine = "\n" };
            }
        }
    }
}
