using System;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Navigation;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless micro proof for nav preference safe vs direct routing.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PathfindingSystem))]
    public partial struct GodgameHeadlessNavPreferenceGateSystem : ISystem
    {
        private const string ScenarioPathEnv = "GODGAME_SCENARIO_PATH";
        private const string GateScenarioFile = "godgame_navpref_gate_micro.json";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_NAVPREF_GATE_EXIT";
        private const uint DefaultTimeoutTicks = 600;

        private const int SafeMidNode = 3;
        private const int DirectMidNode = 1;
        private const float SafeRiskWeight = 0.9f;
        private const float DirectRiskWeight = 0.1f;

        private byte _bankResolved;
        private bool _bankActive;
        private byte _setup;
        private byte _done;
        private uint _startTick;
        private uint _timeoutTick;
        private Entity _graphEntity;
        private Entity _safeRequest;
        private Entity _directRequest;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!ResolveBankActive(ref state) || _done != 0)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<RewindState>(out var rewindState) || rewindState.Mode != RewindMode.Record)
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
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            EnsureNavBudget(ref state);
            EnsureGraph(ref state);
            EnsureRequests(ref state, timeState.Tick);

            var hasSafe = TryGetMidNode(ref state, _safeRequest, out var safeMid);
            var hasDirect = TryGetMidNode(ref state, _directRequest, out var directMid);

            if (!hasSafe || !hasDirect)
            {
                if (timeState.Tick >= _timeoutTick)
                {
                    ReportFailure(ref state, timeState.Tick, safeMid, directMid, "timeout");
                }
                return;
            }

            if (safeMid == SafeMidNode && directMid == DirectMidNode)
            {
                ReportPass(ref state, timeState.Tick, safeMid, directMid);
                return;
            }

            ReportFailure(ref state, timeState.Tick, safeMid, directMid, "route_mismatch");
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

            _bankActive = scenarioPath.EndsWith(GateScenarioFile, StringComparison.OrdinalIgnoreCase);
            if (!_bankActive)
            {
                state.Enabled = false;
            }

            return _bankActive;
        }

        private static void EnsureNavBudget(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<NavPerformanceBudget>())
            {
                var budgetEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(budgetEntity, NavPerformanceBudget.CreateDefaults());
            }

            if (!SystemAPI.HasSingleton<NavPerformanceCounters>())
            {
                var countersEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(countersEntity, new NavPerformanceCounters());
            }
        }

        private void EnsureGraph(ref SystemState state)
        {
            if (_setup != 0 && _graphEntity != Entity.Null && state.EntityManager.Exists(_graphEntity))
            {
                return;
            }

            if (!SystemAPI.TryGetSingletonEntity<NavGraph>(out _graphEntity))
            {
                _graphEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<NavGraph>(_graphEntity);
                state.EntityManager.AddBuffer<NavNode>(_graphEntity);
                state.EntityManager.AddBuffer<NavEdge>(_graphEntity);
            }

            var nodes = state.EntityManager.GetBuffer<NavNode>(_graphEntity);
            var edges = state.EntityManager.GetBuffer<NavEdge>(_graphEntity);
            nodes.Clear();
            edges.Clear();

            nodes.Add(new NavNode
            {
                Position = new float3(0f, 0f, 0f),
                Flags = NavNodeFlags.Start,
                BaseCost = 0f,
                NodeId = 0
            });
            nodes.Add(new NavNode
            {
                Position = new float3(1f, 0f, 0f),
                Flags = NavNodeFlags.Hazard,
                BaseCost = 0f,
                NodeId = 1
            });
            nodes.Add(new NavNode
            {
                Position = new float3(2f, 0f, 0f),
                Flags = NavNodeFlags.Goal,
                BaseCost = 0f,
                NodeId = 2
            });
            nodes.Add(new NavNode
            {
                Position = new float3(0f, 1f, 0f),
                Flags = NavNodeFlags.None,
                BaseCost = 0f,
                NodeId = 3
            });

            edges.Add(new NavEdge
            {
                FromNode = 0,
                ToNode = 1,
                Cost = 1f,
                AllowedModes = LocomotionMode.Ground,
                Flags = NavEdgeFlags.Dangerous,
                IsBidirectional = 1
            });
            edges.Add(new NavEdge
            {
                FromNode = 1,
                ToNode = 2,
                Cost = 1f,
                AllowedModes = LocomotionMode.Ground,
                Flags = NavEdgeFlags.None,
                IsBidirectional = 1
            });
            edges.Add(new NavEdge
            {
                FromNode = 0,
                ToNode = 3,
                Cost = 1f,
                AllowedModes = LocomotionMode.Ground,
                Flags = NavEdgeFlags.None,
                IsBidirectional = 1
            });
            edges.Add(new NavEdge
            {
                FromNode = 3,
                ToNode = 2,
                Cost = 3f,
                AllowedModes = LocomotionMode.Ground,
                Flags = NavEdgeFlags.None,
                IsBidirectional = 1
            });

            var graph = state.EntityManager.GetComponentData<NavGraph>(_graphEntity);
            graph.Version = graph.Version + 1;
            graph.NodeCount = nodes.Length;
            graph.EdgeCount = edges.Length;
            graph.BoundsMin = new float3(0f, 0f, 0f);
            graph.BoundsMax = new float3(2f, 1f, 0f);
            state.EntityManager.SetComponentData(_graphEntity, graph);

            _setup = 1;
        }

        private void EnsureRequests(ref SystemState state, uint tick)
        {
            if (_safeRequest == Entity.Null || !state.EntityManager.Exists(_safeRequest))
            {
                _safeRequest = CreateRequest(ref state, tick, SafeRiskWeight);
            }

            if (_directRequest == Entity.Null || !state.EntityManager.Exists(_directRequest))
            {
                _directRequest = CreateRequest(ref state, tick, DirectRiskWeight);
            }
        }

        private static Entity CreateRequest(ref SystemState state, uint tick, float riskWeight)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new PathRequest
            {
                RequestingEntity = entity,
                StartPosition = new float3(0f, 0f, 0f),
                GoalPosition = new float3(2f, 0f, 0f),
                LocomotionMode = LocomotionMode.Ground,
                Priority = NavRequestPriority.Normal,
                HeatTier = NavHeatTier.Warm,
                RequestTick = tick,
                IsActive = 1
            });

            var preference = NavPreference.CreateDefault();
            preference.RiskWeight = riskWeight;
            preference.TimeWeight = math.clamp(1f - riskWeight, 0f, 1f);
            state.EntityManager.AddComponentData(entity, preference);

            state.EntityManager.AddComponentData(entity, UpdateCadence.Create(1, 0));
            return entity;
        }

        private static bool TryGetMidNode(ref SystemState state, Entity entity, out int midNode)
        {
            midNode = -1;
            if (entity == Entity.Null || !state.EntityManager.Exists(entity))
            {
                return false;
            }

            if (!state.EntityManager.HasComponent<PathState>(entity) || !state.EntityManager.HasBuffer<PathResult>(entity))
            {
                return false;
            }

            var pathState = state.EntityManager.GetComponentData<PathState>(entity);
            if (pathState.IsValid == 0 || pathState.Status != PathStatus.Success)
            {
                return false;
            }

            var path = state.EntityManager.GetBuffer<PathResult>(entity);
            if (path.Length < 3)
            {
                return false;
            }

            midNode = path[1].NodeIndex;
            return true;
        }

        private void ReportPass(ref SystemState state, uint tick, int safeMid, int directMid)
        {
            _done = 1;
            var tickTime = tick;
            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tickTime = tickTimeState.Tick;
            }

            var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                ? scenario.Tick
                : 0u;

            UnityDebug.Log($"[GodgameHeadlessNavPreferenceGate] PASS safeMid={safeMid} directMid={directMid} tick={tick}");
            LogBankResult(true, string.Empty, tickTime, scenarioTick, safeMid, directMid);
            RequestExitIfEnabled(ref state, tick, 0);
        }

        private void ReportFailure(ref SystemState state, uint tick, int safeMid, int directMid, string reason)
        {
            _done = 1;
            var tickTime = tick;
            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tickTime = tickTimeState.Tick;
            }

            var scenarioTick = SystemAPI.TryGetSingleton<ScenarioRunnerTick>(out var scenario)
                ? scenario.Tick
                : 0u;

            UnityDebug.LogError($"[GodgameHeadlessNavPreferenceGate] FAIL reason={reason} safeMid={safeMid} directMid={directMid} tick={tick}");
            LogBankResult(false, reason, tickTime, scenarioTick, safeMid, directMid);
            RequestExitIfEnabled(ref state, tick, 4);
        }

        private static void RequestExitIfEnabled(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private static void LogBankResult(bool pass, string reason, uint tickTime, uint scenarioTick, int safeMid, int directMid)
        {
            const string testId = "G4.NAVPREF_GATE";
            var delta = (int)tickTime - (int)scenarioTick;
            var details = $"safeMid={safeMid} directMid={directMid}";

            if (pass)
            {
                UnityDebug.Log($"BANK:{testId}:PASS tickTime={tickTime} scenarioTick={scenarioTick} delta={delta} {details}");
                return;
            }

            UnityDebug.Log($"BANK:{testId}:FAIL reason={reason} tickTime={tickTime} scenarioTick={scenarioTick} delta={delta} {details}");
        }
    }
}
