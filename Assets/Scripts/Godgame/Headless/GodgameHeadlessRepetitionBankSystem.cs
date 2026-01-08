using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Godgame.Registry;
using Godgame.Telemetry;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless repetition detector that emits intent trace events and a bank marker.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessRepetitionBankSystem : ISystem
    {
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string LoopScenarioFile = "villager_loop_small.json";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_REPETITION_EXIT";
        private const string SnapshotFileName = "repetition_snapshot.json";

        private const string WindowSizeEnv = "GODGAME_HEADLESS_REPETITION_WINDOW";
        private const string MinSamplesEnv = "GODGAME_HEADLESS_REPETITION_MIN_SAMPLES";
        private const string OscillationRepeatEnv = "GODGAME_HEADLESS_REPETITION_OSC_REPEAT";
        private const string ShortCycleRepeatEnv = "GODGAME_HEADLESS_REPETITION_SHORT_REPEAT";
        private const string FailPercentEnv = "GODGAME_HEADLESS_REPETITION_FAIL_PCT";
        private const string EntropyMinEnv = "GODGAME_HEADLESS_REPETITION_ENTROPY_MIN";
        private const string WindowTicksEnv = "GODGAME_HEADLESS_REPETITION_WINDOW_TICKS";
        private const string LivelockMinStoredEnv = "GODGAME_HEADLESS_REPETITION_LIVELOCK_MIN_STORED";
        private const string LivelockMinTransitionsEnv = "GODGAME_HEADLESS_REPETITION_LIVELOCK_MIN_TRANSITIONS";

        private const uint DefaultWindowTicks = 3600; // 60 seconds at 60hz.
        private const int DefaultWindowSize = 24;
        private const int DefaultMinSamples = 12;
        private const int DefaultOscillationRepeats = 3;
        private const int DefaultShortCycleRepeats = 3;
        private const float DefaultFailPercent = 0.30f;
        private const float DefaultEntropyMin = 1.1f;
        private const float DefaultLivelockMinStored = 0.1f;
        private const float DefaultLivelockMinTransitions = 8f;
        private const int SnapshotMaxOffenders = 5;

        private byte _bankResolved;
        private bool _bankActive;
        private bool _bankReported;
        private bool _snapshotWritten;
        private uint _startTick;
        private uint _windowTicks;
        private int _windowSize;
        private int _minSamples;
        private int _oscRepeats;
        private int _shortRepeats;
        private float _failPercent;
        private float _entropyMin;
        private float _livelockMinStored;
        private float _livelockMinTransitions;
        private float _startStored;
        private float _lastStored;
        private bool _hasStoredSample;

        private NativeList<AgentTrace> _agents;
        private NativeParallelHashMap<Entity, int> _agentLookup;
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<VillagerJobState> _jobLookup;
        private ComponentLookup<Navigation> _navigationLookup;
        private ComponentLookup<MoveIntent> _moveIntentLookup;
        private ComponentLookup<MovePlan> _movePlanLookup;
        private ComponentLookup<DecisionTrace> _decisionTraceLookup;
        private ComponentLookup<StorehouseInventory> _storehouseLookup;
        private ComponentLookup<GodgameResourceNodeMirror> _resourceNodeLookup;

        private static readonly FixedString64Bytes IntentTraceEvent = new FixedString64Bytes("intent.trace");
        private static readonly FixedString64Bytes IntentTraceSource = new FixedString64Bytes("headless");

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerJobState>();

            _windowTicks = ReadEnvUInt(WindowTicksEnv, DefaultWindowTicks);
            _windowSize = ResolveWindowSize(ReadEnvInt(WindowSizeEnv, DefaultWindowSize));
            _minSamples = math.max(4, ReadEnvInt(MinSamplesEnv, DefaultMinSamples));
            _oscRepeats = math.max(2, ReadEnvInt(OscillationRepeatEnv, DefaultOscillationRepeats));
            _shortRepeats = math.max(2, ReadEnvInt(ShortCycleRepeatEnv, DefaultShortCycleRepeats));
            _failPercent = math.clamp(ReadEnvFloat(FailPercentEnv, DefaultFailPercent), 0.05f, 1f);
            _entropyMin = math.max(0.01f, ReadEnvFloat(EntropyMinEnv, DefaultEntropyMin));
            _livelockMinStored = math.max(0f, ReadEnvFloat(LivelockMinStoredEnv, DefaultLivelockMinStored));
            _livelockMinTransitions = math.max(1f, ReadEnvFloat(LivelockMinTransitionsEnv, DefaultLivelockMinTransitions));

            _agents = new NativeList<AgentTrace>(256, Allocator.Persistent);
            _agentLookup = new NativeParallelHashMap<Entity, int>(512, Allocator.Persistent);

            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _jobLookup = state.GetComponentLookup<VillagerJobState>(true);
            _navigationLookup = state.GetComponentLookup<Navigation>(true);
            _moveIntentLookup = state.GetComponentLookup<MoveIntent>(true);
            _movePlanLookup = state.GetComponentLookup<MovePlan>(true);
            _decisionTraceLookup = state.GetComponentLookup<DecisionTrace>(true);
            _storehouseLookup = state.GetComponentLookup<StorehouseInventory>(true);
            _resourceNodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(true);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_agents.IsCreated)
            {
                _agents.Dispose();
            }

            if (_agentLookup.IsCreated)
            {
                _agentLookup.Dispose();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!ResolveBankActive(ref state) || _bankReported)
            {
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
            }

            if (SystemAPI.TryGetSingleton<StorehouseRegistry>(out var storehouse))
            {
                _lastStored = storehouse.TotalStored;
                if (!_hasStoredSample)
                {
                    _hasStoredSample = true;
                    _startStored = _lastStored;
                }
            }

            _goalLookup.Update(ref state);
            _jobLookup.Update(ref state);
            _navigationLookup.Update(ref state);
            _moveIntentLookup.Update(ref state);
            _movePlanLookup.Update(ref state);
            _decisionTraceLookup.Update(ref state);
            _storehouseLookup.Update(ref state);
            _resourceNodeLookup.Update(ref state);

            DynamicBuffer<TelemetryEvent> eventBuffer = default;
            var emitEvents = false;
            if (SystemAPI.TryGetSingleton<TelemetryExportConfig>(out var config) &&
                config.Enabled != 0 &&
                (config.Flags & TelemetryExportFlags.IncludeTelemetryEvents) != 0)
            {
                var streamEntity = TelemetryStreamUtility.EnsureEventStream(state.EntityManager);
                if (!state.EntityManager.HasBuffer<TelemetryEvent>(streamEntity))
                {
                    state.EntityManager.AddBuffer<TelemetryEvent>(streamEntity);
                }

                eventBuffer = state.EntityManager.GetBuffer<TelemetryEvent>(streamEntity);
                emitEvents = true;
            }

            foreach (var (job, entity) in SystemAPI.Query<RefRO<VillagerJobState>>().WithEntityAccess())
            {
                var snapshot = BuildSnapshot(ref state, entity, job.ValueRO);
                var signature = BuildSignature(snapshot);
                var reasonCode = ResolveReasonCode(ref state, entity);

                var index = GetOrCreateTrace(entity);
                var trace = _agents[index];
                if (UpdateTrace(ref trace, signature, reasonCode, job.ValueRO))
                {
                    if (emitEvents)
                    {
                        EmitIntentTrace(ref eventBuffer, timeState.Tick, entity, snapshot, reasonCode);
                    }
                }

                _agents[index] = trace;
            }

            if (timeState.Tick < _startTick + _windowTicks)
            {
                return;
            }

            var result = EvaluateRepetition();
            var tickTime = timeState.Tick;
            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tickTime = tickTimeState.Tick;
            }

            var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                ? scenario.Tick
                : 0u;
            LogBankResult(result.Pass, result.Reason, tickTime, scenarioTick);
            WriteRepetitionSnapshot(result, tickTime, scenarioTick);
            _bankReported = true;
            LogOffenderSummary(result);
            RequestExitIfEnabled(ref state, timeState.Tick, result.Pass ? 0 : 6);
        }

        private bool ResolveBankActive(ref SystemState state)
        {
            if (_bankResolved != 0)
            {
                return _bankActive;
            }

            _bankResolved = 1;
            var scenarioPath = SystemEnv.GetEnvironmentVariable(ScenarioPathEnv);
            if (string.IsNullOrWhiteSpace(scenarioPath))
            {
                state.Enabled = false;
                return false;
            }

            _bankActive = scenarioPath.EndsWith(LoopScenarioFile, StringComparison.OrdinalIgnoreCase);
            if (!_bankActive)
            {
                state.Enabled = false;
            }

            return _bankActive;
        }

        private IntentSnapshot BuildSnapshot(ref SystemState state, Entity entity, in VillagerJobState job)
        {
            var goal = _goalLookup.HasComponent(entity) ? _goalLookup[entity].CurrentGoal : default;
            var moveIntent = _moveIntentLookup.HasComponent(entity) ? _moveIntentLookup[entity].IntentType : default;
            var movePlan = _movePlanLookup.HasComponent(entity) ? _movePlanLookup[entity].Mode : default;
            var target = job.Target != Entity.Null
                ? job.Target
                : _moveIntentLookup.HasComponent(entity)
                    ? _moveIntentLookup[entity].TargetEntity
                    : Entity.Null;
            var targetClass = ResolveTargetClass(target);

            return new IntentSnapshot
            {
                Goal = (byte)goal,
                JobType = (byte)job.Type,
                JobPhase = (byte)job.Phase,
                MoveIntent = (byte)moveIntent,
                MovePlan = (byte)movePlan,
                Target = target,
                TargetClass = targetClass
            };
        }

        private int GetOrCreateTrace(Entity entity)
        {
            if (_agentLookup.TryGetValue(entity, out var index))
            {
                return index;
            }

            index = _agents.Length;
            _agents.Add(new AgentTrace
            {
                Entity = entity,
                LastSignature = 0u,
                HasSignature = 0,
                Signatures = default,
                Reasons = default,
                LastCarryCount = 0f,
                HasCarrySample = 0,
                ThrashTransitions = 0,
                ProgressEvents = 0
            });
            _agentLookup.TryAdd(entity, index);
            return index;
        }

        private bool UpdateTrace(ref AgentTrace trace, uint signature, byte reasonCode, in VillagerJobState job)
        {
            if (RegisterDelivery(ref trace, in job))
            {
                ResetTrace(ref trace);
                trace.ProgressEvents++;
                return false;
            }

            if (trace.HasSignature != 0 && trace.LastSignature == signature)
            {
                return false;
            }

            trace.HasSignature = 1;
            trace.LastSignature = signature;
            AppendSignature(ref trace, signature, reasonCode);
            trace.ThrashTransitions++;
            return true;
        }

        private static bool RegisterDelivery(ref AgentTrace trace, in VillagerJobState job)
        {
            var delivered = trace.HasCarrySample != 0 &&
                            trace.LastCarryCount > 0.01f &&
                            job.CarryCount <= 0.01f &&
                            job.Phase == JobPhase.Deliver;

            trace.LastCarryCount = job.CarryCount;
            trace.HasCarrySample = 1;
            return delivered;
        }

        private static void ResetTrace(ref AgentTrace trace)
        {
            trace.HasSignature = 0;
            trace.LastSignature = 0u;
            trace.Signatures.Clear();
            trace.Reasons.Clear();
        }

        private void AppendSignature(ref AgentTrace trace, uint signature, byte reasonCode)
        {
            if (trace.Signatures.Length < _windowSize)
            {
                trace.Signatures.Add(signature);
                trace.Reasons.Add(reasonCode);
                return;
            }

            for (int i = 1; i < trace.Signatures.Length; i++)
            {
                trace.Signatures[i - 1] = trace.Signatures[i];
                trace.Reasons[i - 1] = trace.Reasons[i];
            }

            trace.Signatures[trace.Signatures.Length - 1] = signature;
            trace.Reasons[trace.Reasons.Length - 1] = reasonCode;
        }

        private RepetitionResult EvaluateRepetition()
        {
            var totalAgents = 0;
            var oscillationCount = 0;
            var shortCycleCount = 0;
            var totalTransitions = 0;
            var totalProgress = 0;
            var entropySamples = new NativeList<float>(Allocator.Temp);
            var offenders = new NativeList<Offender>(Allocator.Temp);

            for (int i = 0; i < _agents.Length; i++)
            {
                var trace = _agents[i];

                var sampleCount = trace.Signatures.Length;
                if (sampleCount < _minSamples)
                {
                    continue;
                }

                totalAgents++;
                totalTransitions += math.max(0, trace.ThrashTransitions);
                totalProgress += trace.ProgressEvents;

                var entropy = ComputeEntropy(trace.Signatures);
                entropySamples.Add(entropy);

                if (HasRepeatingCycle(trace.Signatures, 2, _oscRepeats))
                {
                    oscillationCount++;
                    offenders.Add(new Offender(trace.Entity, 2, _oscRepeats, entropy, BuildReasonSummary(trace.Reasons)));
                }
                else if (HasRepeatingCycle(trace.Signatures, 3, _shortRepeats))
                {
                    shortCycleCount++;
                    offenders.Add(new Offender(trace.Entity, 3, _shortRepeats, entropy, BuildReasonSummary(trace.Reasons)));
                }
            }

            var medianEntropy = ComputeMedian(entropySamples);
            entropySamples.Dispose();

            var averageTransitions = totalAgents > 0 ? totalTransitions / (float)totalAgents : 0f;
            var storedDelta = _hasStoredSample ? math.max(0f, _lastStored - _startStored) : -1f;
            var livelockTriggered = totalAgents > 0 &&
                _hasStoredSample &&
                storedDelta < _livelockMinStored &&
                averageTransitions >= _livelockMinTransitions;

            var pass = true;
            var reason = string.Empty;
            var repetitionHigh = false;
            if (totalAgents > 0)
            {
                var oscillationRatio = oscillationCount / math.max(1f, totalAgents);
                var shortCycleRatio = shortCycleCount / math.max(1f, totalAgents);

                if (oscillationRatio > _failPercent)
                {
                    repetitionHigh = true;
                    reason = "OSCILLATION";
                }
                else if (shortCycleRatio > _failPercent)
                {
                    repetitionHigh = true;
                    reason = "SHORT_CYCLE";
                }
                else if (medianEntropy > 0f && medianEntropy < _entropyMin)
                {
                    repetitionHigh = true;
                    reason = "LOW_ENTROPY";
                }
            }
            else
            {
                reason = "INSUFFICIENT_SAMPLES";
            }

            if (repetitionHigh && livelockTriggered)
            {
                pass = false;
            }

            var result = new RepetitionResult
            {
                Pass = pass,
                Reason = reason,
                TotalAgents = totalAgents,
                OscillationCount = oscillationCount,
                ShortCycleCount = shortCycleCount,
                TotalTransitions = totalTransitions,
                AverageTransitions = averageTransitions,
                StoredDelta = storedDelta,
                HasStoredDelta = _hasStoredSample,
                TotalProgressEvents = totalProgress,
                LivelockTriggered = livelockTriggered,
                RepetitionHigh = repetitionHigh,
                MedianEntropy = medianEntropy,
                Offenders = offenders
            };

            return result;
        }

        private void LogOffenderSummary(in RepetitionResult result)
        {
            var storedLabel = result.HasStoredDelta ? result.StoredDelta.ToString("F2") : "na";
            var livelockLabel = result.LivelockTriggered ? "1" : "0";
            UnityDebug.Log($"[GodgameHeadlessRepetition] agents={result.TotalAgents} oscillation={result.OscillationCount} shortCycle={result.ShortCycleCount} transitions={result.TotalTransitions} avgTransitions={result.AverageTransitions:F1} progressEvents={result.TotalProgressEvents} storedDelta={storedLabel} livelock={livelockLabel} medianEntropy={result.MedianEntropy:F2} window={_windowSize} minSamples={_minSamples} livelockMinStored={_livelockMinStored:F2} livelockMinTransitions={_livelockMinTransitions:F1}");

            if (result.Offenders.Length == 0)
            {
                result.Offenders.Dispose();
                return;
            }

            var topCount = math.min(3, result.Offenders.Length);
            for (int i = 0; i < topCount; i++)
            {
                var offender = result.Offenders[i];
                UnityDebug.LogWarning($"[GodgameHeadlessRepetition] offender entity={offender.Entity.Index} cycleLen={offender.CycleLength} repeats>={offender.Repeats} entropy={offender.Entropy:F2} reasons={offender.ReasonSummary}");
            }

            result.Offenders.Dispose();
        }

        private void WriteRepetitionSnapshot(in RepetitionResult result, uint tickTime, uint scenarioTick)
        {
            if (_snapshotWritten || !GodgameHeadlessDiagnostics.Enabled)
            {
                return;
            }

            _snapshotWritten = true;

            var snapshots = new List<OffenderSnapshot>(math.min(result.Offenders.Length, SnapshotMaxOffenders));
            for (int i = 0; i < result.Offenders.Length; i++)
            {
                snapshots.Add(BuildOffenderSnapshot(result.Offenders[i]));
            }

            snapshots.Sort(CompareOffenders);

            var json = BuildSnapshotJson(result, snapshots, tickTime, scenarioTick);
            GodgameHeadlessDiagnostics.WriteArtifact(SnapshotFileName, json);
        }

        private OffenderSnapshot BuildOffenderSnapshot(in Offender offender)
        {
            var entity = offender.Entity;
            var job = _jobLookup.HasComponent(entity) ? _jobLookup[entity] : default;
            var goal = _goalLookup.HasComponent(entity) ? _goalLookup[entity].CurrentGoal : default;

            var hasMoveIntent = _moveIntentLookup.HasComponent(entity);
            var moveIntent = hasMoveIntent ? _moveIntentLookup[entity] : default;
            var movePlan = _movePlanLookup.HasComponent(entity) ? _movePlanLookup[entity].Mode : default;

            var target = job.Target != Entity.Null ? job.Target : moveIntent.TargetEntity;
            var targetClass = ResolveTargetClass(target);

            var destination = float3.zero;
            var hasDestination = false;
            if (hasMoveIntent)
            {
                destination = moveIntent.TargetPosition;
                hasDestination = math.lengthsq(destination) > 0.001f;
            }
            else if (_navigationLookup.HasComponent(entity))
            {
                destination = _navigationLookup[entity].Destination;
                hasDestination = math.lengthsq(destination) > 0.001f;
            }

            return new OffenderSnapshot
            {
                Entity = entity,
                CycleLength = offender.CycleLength,
                Repeats = offender.Repeats,
                Entropy = offender.Entropy,
                ReasonSummary = offender.ReasonSummary,
                Goal = (byte)goal,
                JobType = (byte)job.Type,
                JobPhase = (byte)job.Phase,
                MoveIntent = (byte)moveIntent.IntentType,
                MovePlan = (byte)movePlan,
                Target = target,
                TargetClass = targetClass,
                Destination = destination,
                HasDestination = hasDestination
            };
        }

        private static int CompareOffenders(OffenderSnapshot a, OffenderSnapshot b)
        {
            var entropyCompare = a.Entropy.CompareTo(b.Entropy);
            if (entropyCompare != 0)
            {
                return entropyCompare;
            }

            var cycleCompare = b.CycleLength.CompareTo(a.CycleLength);
            if (cycleCompare != 0)
            {
                return cycleCompare;
            }

            return a.Entity.Index.CompareTo(b.Entity.Index);
        }

        private static string BuildSnapshotJson(in RepetitionResult result, List<OffenderSnapshot> offenders, uint tickTime, uint scenarioTick)
        {
            var sb = new StringBuilder(512);
            sb.Append('{');
            AppendString(sb, "snapshot_id", "GODGAME_REPETITION_SNAPSHOT");
            sb.Append(',');
            AppendString(sb, "reason", result.Reason ?? string.Empty);
            sb.Append(',');
            AppendInt(sb, "pass", result.Pass ? 1 : 0);
            sb.Append(',');
            AppendInt(sb, "repetition_high", result.RepetitionHigh ? 1 : 0);
            sb.Append(',');
            AppendInt(sb, "livelock", result.LivelockTriggered ? 1 : 0);
            sb.Append(',');
            AppendInt(sb, "offender_count", result.Offenders.Length);
            sb.Append(',');
            AppendUInt(sb, "tick_time", tickTime);
            sb.Append(',');
            AppendUInt(sb, "scenario_tick", scenarioTick);
            sb.Append(',');
            AppendInt(sb, "total_agents", result.TotalAgents);
            sb.Append(',');
            AppendInt(sb, "oscillation_count", result.OscillationCount);
            sb.Append(',');
            AppendInt(sb, "short_cycle_count", result.ShortCycleCount);
            sb.Append(',');
            AppendFloat(sb, "median_entropy", result.MedianEntropy);
            sb.Append(',');
            sb.Append("\"offenders\":[");

            var count = math.min(SnapshotMaxOffenders, offenders.Count);
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                AppendOffenderJson(sb, offenders[i]);
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private static void AppendOffenderJson(StringBuilder sb, OffenderSnapshot offender)
        {
            sb.Append('{');
            AppendInt(sb, "entity_index", offender.Entity.Index);
            AppendInt(sb, "entity_version", offender.Entity.Version, true);
            AppendInt(sb, "cycle_len", offender.CycleLength, true);
            AppendInt(sb, "repeats", offender.Repeats, true);
            AppendFloat(sb, "entropy", offender.Entropy, true);
            AppendString(sb, "reason_summary", offender.ReasonSummary.ToString(), true);
            AppendInt(sb, "goal", offender.Goal, true);
            AppendInt(sb, "job_type", offender.JobType, true);
            AppendInt(sb, "job_phase", offender.JobPhase, true);
            AppendInt(sb, "move_intent", offender.MoveIntent, true);
            AppendInt(sb, "move_plan", offender.MovePlan, true);
            AppendInt(sb, "target_index", offender.Target.Index, true);
            AppendInt(sb, "target_version", offender.Target.Version, true);
            AppendString(sb, "target_class", ResolveTargetClassLabel(offender.TargetClass), true);
            AppendInt(sb, "has_dest", offender.HasDestination ? 1 : 0, true);
            if (offender.HasDestination)
            {
                sb.Append(",\"dest\":[");
                AppendFloatValue(sb, offender.Destination.x);
                sb.Append(',');
                AppendFloatValue(sb, offender.Destination.y);
                sb.Append(',');
                AppendFloatValue(sb, offender.Destination.z);
                sb.Append(']');
            }

            sb.Append('}');
        }

        private static string ResolveTargetClassLabel(byte targetClass)
        {
            return targetClass switch
            {
                1 => "storehouse",
                2 => "resource",
                3 => "other",
                _ => "none"
            };
        }

        private static void AppendString(StringBuilder sb, string key, string value, bool prependComma = false)
        {
            if (prependComma)
            {
                sb.Append(',');
            }

            sb.Append('"').Append(key).Append("\":\"");
            sb.Append(Escape(value));
            sb.Append('"');
        }

        private static void AppendUInt(StringBuilder sb, string key, uint value)
        {
            sb.Append('"').Append(key).Append("\":").Append(value);
        }

        private static void AppendInt(StringBuilder sb, string key, int value, bool prependComma = false)
        {
            if (prependComma)
            {
                sb.Append(',');
            }

            sb.Append('"').Append(key).Append("\":").Append(value);
        }

        private static void AppendFloat(StringBuilder sb, string key, float value, bool prependComma = false)
        {
            if (prependComma)
            {
                sb.Append(',');
            }

            sb.Append('"').Append(key).Append("\":");
            AppendFloatValue(sb, value);
        }

        private static void AppendFloatValue(StringBuilder sb, float value)
        {
            sb.Append(value.ToString("F3", CultureInfo.InvariantCulture));
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static bool HasRepeatingCycle(in FixedList128Bytes<uint> signatures, int cycleLength, int repeats)
        {
            if (cycleLength <= 0 || repeats <= 1)
            {
                return false;
            }

            var needed = cycleLength * repeats;
            if (signatures.Length < needed)
            {
                return false;
            }

            var start = signatures.Length - needed;
            for (int r = 1; r < repeats; r++)
            {
                var offset = start + (r * cycleLength);
                for (int i = 0; i < cycleLength; i++)
                {
                    if (signatures[start + i] != signatures[offset + i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static float ComputeEntropy(in FixedList128Bytes<uint> signatures)
        {
            if (signatures.Length == 0)
            {
                return 0f;
            }

            var counts = new NativeList<SignatureCount>(Allocator.Temp);
            for (int i = 0; i < signatures.Length; i++)
            {
                var signature = signatures[i];
                var found = false;
                for (int j = 0; j < counts.Length; j++)
                {
                    if (counts[j].Signature == signature)
                    {
                        var entry = counts[j];
                        entry.Count++;
                        counts[j] = entry;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    counts.Add(new SignatureCount
                    {
                        Signature = signature,
                        Count = 1
                    });
                }
            }

            var entropy = 0f;
            var total = (float)signatures.Length;
            for (int i = 0; i < counts.Length; i++)
            {
                var p = counts[i].Count / total;
                if (p > 0f)
                {
                    entropy -= p * math.log2(p);
                }
            }

            counts.Dispose();
            return entropy;
        }

        private static FixedString128Bytes BuildReasonSummary(in FixedList128Bytes<byte> reasons)
        {
            if (reasons.Length == 0)
            {
                return default;
            }

            var counts = new FixedList64Bytes<ReasonCount>();
            for (int i = 0; i < reasons.Length; i++)
            {
                var reason = reasons[i];
                var found = false;
                for (int j = 0; j < counts.Length; j++)
                {
                    if (counts[j].Code == reason)
                    {
                        var entry = counts[j];
                        entry.Count++;
                        counts[j] = entry;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    counts.Add(new ReasonCount
                    {
                        Code = reason,
                        Count = 1
                    });
                }
            }

            var summary = new FixedString128Bytes();
            for (int i = 0; i < counts.Length; i++)
            {
                if (i > 0)
                {
                    summary.Append(",");
                }

                summary.Append("rc");
                summary.Append((int)counts[i].Code);
                summary.Append("=");
                summary.Append(counts[i].Count);
            }

            return summary;
        }

        private static float ComputeMedian(NativeList<float> values)
        {
            if (values.Length == 0)
            {
                return 0f;
            }

            // Simple insertion sort (small N).
            for (int i = 1; i < values.Length; i++)
            {
                var key = values[i];
                var j = i - 1;
                while (j >= 0 && values[j] > key)
                {
                    values[j + 1] = values[j];
                    j--;
                }
                values[j + 1] = key;
            }

            var mid = values.Length / 2;
            if ((values.Length & 1) == 1)
            {
                return values[mid];
            }

            return (values[mid - 1] + values[mid]) * 0.5f;
        }

        private static uint BuildSignature(in IntentSnapshot snapshot)
        {
            return math.hash(new uint4(
                snapshot.Goal,
                (uint)(snapshot.JobType | (snapshot.JobPhase << 8) | (snapshot.MoveIntent << 16) | (snapshot.MovePlan << 24)),
                snapshot.TargetClass,
                0u));
        }

        private byte ResolveReasonCode(ref SystemState state, Entity entity)
        {
            if (_decisionTraceLookup.HasComponent(entity))
            {
                return _decisionTraceLookup[entity].ReasonCode;
            }

            return 0;
        }

        private static void EmitIntentTrace(ref DynamicBuffer<TelemetryEvent> buffer, uint tick, Entity entity, in IntentSnapshot snapshot, byte reasonCode)
        {
            var writer = new GodgameTelemetryJsonWriter(128);
            writer.AddInt("a", entity.Index);
            writer.AddInt("g", snapshot.Goal);
            writer.AddInt("jt", snapshot.JobType);
            writer.AddInt("jp", snapshot.JobPhase);
            writer.AddInt("mi", snapshot.MoveIntent);
            writer.AddInt("mp", snapshot.MovePlan);
            writer.AddInt("t", snapshot.Target.Index);
            writer.AddInt("tc", snapshot.TargetClass);
            writer.AddInt("r", (byte)IntentTraceResult.Started);
            writer.AddInt("rc", reasonCode);
            buffer.AddEvent(IntentTraceEvent, tick, IntentTraceSource, writer.Build());
        }

        private byte ResolveTargetClass(Entity target)
        {
            if (target == Entity.Null)
            {
                return 0;
            }

            if (_storehouseLookup.HasComponent(target))
            {
                return 1;
            }

            if (_resourceNodeLookup.HasComponent(target))
            {
                return 2;
            }

            return 3;
        }

        private static uint ReadEnvUInt(string key, uint defaultValue)
        {
            var raw = SystemEnv.GetEnvironmentVariable(key);
            return uint.TryParse(raw, out var value) ? value : defaultValue;
        }

        private static int ReadEnvInt(string key, int defaultValue)
        {
            var raw = SystemEnv.GetEnvironmentVariable(key);
            return int.TryParse(raw, out var value) ? value : defaultValue;
        }

        private static float ReadEnvFloat(string key, float defaultValue)
        {
            var raw = SystemEnv.GetEnvironmentVariable(key);
            return float.TryParse(raw, out var value) ? value : defaultValue;
        }

        private static int ResolveWindowSize(int configured)
        {
            var capacity = new FixedList128Bytes<uint>().Capacity;
            return math.clamp(configured, 8, capacity);
        }

        private static void LogBankResult(bool pass, string reason, uint tickTime, uint scenarioTick)
        {
            const string testId = "G3.VILLAGER_REPETITION";
            var delta = (int)tickTime - (int)scenarioTick;

            if (pass)
            {
                UnityDebug.Log($"BANK:{testId}:PASS tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
                return;
            }

            UnityDebug.Log($"BANK:{testId}:FAIL reason={reason} tickTime={tickTime} scenarioTick={scenarioTick} delta={delta}");
        }

        private static void RequestExitIfEnabled(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private struct AgentTrace
        {
            public Entity Entity;
            public uint LastSignature;
            public byte HasSignature;
            public FixedList128Bytes<uint> Signatures;
            public FixedList128Bytes<byte> Reasons;
            public float LastCarryCount;
            public byte HasCarrySample;
            public int ThrashTransitions;
            public int ProgressEvents;
        }

        private struct IntentSnapshot
        {
            public byte Goal;
            public byte JobType;
            public byte JobPhase;
            public byte MoveIntent;
            public byte MovePlan;
            public Entity Target;
            public byte TargetClass;
        }

        private struct SignatureCount
        {
            public uint Signature;
            public int Count;
        }

        private struct ReasonCount
        {
            public byte Code;
            public int Count;
        }

        private struct Offender
        {
            public Entity Entity;
            public int CycleLength;
            public int Repeats;
            public float Entropy;
            public FixedString128Bytes ReasonSummary;

            public Offender(Entity entity, int cycleLength, int repeats, float entropy, in FixedString128Bytes reasonSummary)
            {
                Entity = entity;
                CycleLength = cycleLength;
                Repeats = repeats;
                Entropy = entropy;
                ReasonSummary = reasonSummary;
            }
        }

        private struct OffenderSnapshot
        {
            public Entity Entity;
            public int CycleLength;
            public int Repeats;
            public float Entropy;
            public FixedString128Bytes ReasonSummary;
            public byte Goal;
            public byte JobType;
            public byte JobPhase;
            public byte MoveIntent;
            public byte MovePlan;
            public Entity Target;
            public byte TargetClass;
            public float3 Destination;
            public bool HasDestination;
        }

        private struct RepetitionResult
        {
            public bool Pass;
            public string Reason;
            public int TotalAgents;
            public int OscillationCount;
            public int ShortCycleCount;
            public int TotalTransitions;
            public float AverageTransitions;
            public float StoredDelta;
            public bool HasStoredDelta;
            public int TotalProgressEvents;
            public bool LivelockTriggered;
            public bool RepetitionHigh;
            public float MedianEntropy;
            public NativeList<Offender> Offenders;
        }

        private enum IntentTraceResult : byte
        {
            Started = 0,
            Completed = 1,
            Cancelled = 2,
            Failed = 3
        }
    }
}
