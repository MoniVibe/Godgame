using System;
using System.Globalization;
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
        private const string LoopInvariantId = "Invariant/VillagerLoopRepetition";

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

        private byte _bankResolved;
        private bool _bankActive;
        private bool _bankReported;
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
            _bankReported = true;
            if (!result.Pass)
            {
                ReportLoopInvariant(result, tickTime, scenarioTick);
            }
            LogOffenderSummary(result);
            RequestExit(ref state, timeState.Tick, result.Pass, 6);
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

        private static void RequestExit(ref SystemState state, uint tick, bool pass, int failExitCode)
        {
            if (!pass)
            {
                GodgameHeadlessExitSystem.Request(ref state, tick, failExitCode);
                return;
            }

            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, 0);
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

        private void ReportLoopInvariant(in RepetitionResult result, uint tick, uint scenarioTick)
        {
            var totalAgents = math.max(1, result.TotalAgents);
            var oscillationRatio = result.OscillationCount / (float)totalAgents;
            var shortCycleRatio = result.ShortCycleCount / (float)totalAgents;
            var observed = string.Format(CultureInfo.InvariantCulture,
                "oscillation_ratio={0:0.###} short_cycle_ratio={1:0.###} stored_delta={2:0.###} avg_transitions={3:0.###} median_entropy={4:0.###}",
                oscillationRatio, shortCycleRatio, result.StoredDelta, result.AverageTransitions, result.MedianEntropy);
            var expected = string.Format(CultureInfo.InvariantCulture,
                "oscillation_ratio<={0:0.###} short_cycle_ratio<={0:0.###} median_entropy>={1:0.###} stored_delta>={2:0.###} avg_transitions<{3:0.###}",
                _failPercent, _entropyMin, _livelockMinStored, _livelockMinTransitions);
            var context = string.Format(CultureInfo.InvariantCulture,
                "{{\"reason\":\"{0}\",\"tick\":{1},\"scenarioTick\":{2},\"totalAgents\":{3},\"oscillation\":{4},\"shortCycle\":{5},\"oscRepeats\":{6},\"shortRepeats\":{7},\"windowSize\":{8},\"minSamples\":{9},\"failPercent\":{10:0.###},\"entropyMin\":{11:0.###},\"medianEntropy\":{12:0.###},\"storedDelta\":{13:0.###},\"livelockMinStored\":{14:0.###},\"avgTransitions\":{15:0.###},\"livelockMinTransitions\":{16:0.###}}}",
                result.Reason ?? string.Empty,
                tick,
                scenarioTick,
                result.TotalAgents,
                result.OscillationCount,
                result.ShortCycleCount,
                _oscRepeats,
                _shortRepeats,
                _windowSize,
                _minSamples,
                _failPercent,
                _entropyMin,
                result.MedianEntropy,
                result.StoredDelta,
                _livelockMinStored,
                result.AverageTransitions,
                _livelockMinTransitions);

            ScenarioExitUtility.ReportInvariant(LoopInvariantId,
                $"Villager repetition loop detected (reason={result.Reason}, agents={result.TotalAgents}).");
            GodgameHeadlessDiagnostics.ReportInvariant(LoopInvariantId,
                "Villager repetition loop detected.",
                observed,
                expected,
                context);
        }
    }
}
