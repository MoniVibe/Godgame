using System;
using System.Globalization;
using System.IO;
using Godgame.Headless;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Scenarios;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using SystemEnv = System.Environment;

namespace Godgame.Scenario
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(GodgameScenarioLoaderSystem))]
    [UpdateBefore(typeof(GodgameScenarioSpawnSystem))]
    public partial struct GodgameJobLoopScenarioSeedSystem : ISystem
    {
        private byte _active;
        private byte _seedApplied;
        private uint _seed;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioOptions>();
            state.RequireForUpdate<GodgameScenarioSpawnConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_seedApplied != 0)
            {
                state.Enabled = false;
                return;
            }

            if (_active == 0)
            {
                if (!JobLoopScenarioUtility.TryResolveScenarioPath(ref state, out var scenarioPath))
                {
                    return;
                }

                if (!JobLoopScenarioUtility.IsJobLoopScenario(scenarioPath))
                {
                    state.Enabled = false;
                    return;
                }

                _active = 1;
                _seed = JobLoopScenarioUtility.ResolveSeed(scenarioPath);
                JobLoopScenarioUtility.EnsureScenarioInfo(ref state, _seed);
            }

            foreach (var config in SystemAPI.Query<RefRW<GodgameScenarioSpawnConfig>>())
            {
                if (config.ValueRO.Seed == _seed)
                {
                    continue;
                }

                var updated = config.ValueRO;
                updated.Seed = _seed;
                config.ValueRW = updated;
            }

            _seedApplied = 1;
            state.Enabled = false;
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameHeadlessDiagnosticsSystem))]
    public partial struct GodgameJobLoopInvariantSystem : ISystem
    {
        private const float StoredDeltaThreshold = 0.01f;
        private const float NegativeEpsilon = -0.001f;

        private byte _active;
        private byte _reported;
        private uint _spawnTick;
        private float _baselineStored;
        private float _minInvariantSeconds;
        private uint _seed;
        private EntityQuery _villagerQuery;
        private EntityQuery _storehouseQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioOptions>();
            state.RequireForUpdate<TimeState>();
            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<SettlementVillagerState>()
                .Build();
            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<StorehouseInventory>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_reported != 0)
            {
                return;
            }

            if (_active == 0)
            {
                if (!TryActivate(ref state))
                {
                    return;
                }
            }

            if (!TryResolveTick(ref state, out var tick, out var fixedDt))
            {
                return;
            }

            if (_spawnTick == 0)
            {
                if (SystemAPI.TryGetSingleton<GodgameScenarioRuntime>(out var runtime) && runtime.HasSpawned != 0)
                {
                    _spawnTick = tick;
                    _baselineStored = SumStored(ref state);
                }
                else
                {
                    return;
                }
            }

            var minTicks = JobLoopScenarioUtility.ResolveMinTicks(_minInvariantSeconds, fixedDt);
            if (tick < _spawnTick || tick - _spawnTick < minTicks)
            {
                return;
            }

            EvaluateInvariants(ref state);
            _reported = 1;
        }

        private bool TryActivate(ref SystemState state)
        {
            if (!JobLoopScenarioUtility.TryResolveScenarioPath(ref state, out var scenarioPath))
            {
                return false;
            }

            if (!JobLoopScenarioUtility.IsJobLoopScenario(scenarioPath))
            {
                state.Enabled = false;
                return false;
            }

            _active = 1;
            _minInvariantSeconds = JobLoopScenarioUtility.DefaultMinInvariantSeconds;
            _seed = JobLoopScenarioUtility.DefaultSeed;

            if (JobLoopScenarioUtility.TryLoadMeta(scenarioPath, out var meta))
            {
                if (meta.seed != 0)
                {
                    _seed = meta.seed;
                }

                if (meta.min_invariant_s > 0f)
                {
                    _minInvariantSeconds = meta.min_invariant_s;
                }
            }

            JobLoopScenarioUtility.EnsureScenarioInfo(ref state, _seed);
            return true;
        }

        private void EvaluateInvariants(ref SystemState state)
        {
            var villagerCount = _villagerQuery.CalculateEntityCount();
            var storehouseCount = _storehouseQuery.CalculateEntityCount();
            var storedTotal = SumStored(ref state);
            var storedDelta = storedTotal - _baselineStored;

            if (villagerCount == 0)
            {
                ReportInvariant("JobLoop/MissingVillagers", "No villagers spawned.",
                    "villagers=0", "villagers>0");
            }

            if (storehouseCount == 0)
            {
                ReportInvariant("JobLoop/MissingStorehouse", "No storehouse spawned.",
                    "storehouses=0", "storehouses>0");
            }

            if (storedDelta <= StoredDeltaThreshold)
            {
                ReportInvariant("JobLoop/NoDelivery",
                    "Storehouse stored did not increase after the warmup window.",
                    $"stored_delta={FormatFloat(storedDelta)}",
                    "stored_delta>0");
            }

            if (TryFindNegativeInventory(ref state, out var negativeCount, out var minValue))
            {
                ReportInvariant("JobLoop/NegativeInventory",
                    "Storehouse inventory contains negative values.",
                    $"negative_entries={negativeCount} min={FormatFloat(minValue)}",
                    "amount>=0,reserved>=0");
            }
        }

        private static void ReportInvariant(string id, string message, string observed, string expected)
        {
            GodgameHeadlessDiagnostics.ReportInvariant(id, message, observed, expected);
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static float SumStored(ref SystemState state)
        {
            var total = 0f;
            foreach (var inventory in SystemAPI.Query<RefRO<StorehouseInventory>>())
            {
                total += inventory.ValueRO.TotalStored;
            }

            return total;
        }

        private static bool TryFindNegativeInventory(ref SystemState state, out int negativeCount, out float minValue)
        {
            negativeCount = 0;
            minValue = 0f;

            foreach (var inventory in SystemAPI.Query<RefRO<StorehouseInventory>>())
            {
                var stored = inventory.ValueRO.TotalStored;
                if (stored < NegativeEpsilon)
                {
                    negativeCount++;
                    minValue = math.min(minValue, stored);
                }
            }

            foreach (var items in SystemAPI.Query<DynamicBuffer<StorehouseInventoryItem>>())
            {
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (item.Amount < NegativeEpsilon || item.Reserved < NegativeEpsilon)
                    {
                        negativeCount++;
                        minValue = math.min(minValue, math.min(item.Amount, item.Reserved));
                    }
                }
            }

            return negativeCount > 0;
        }

        private static bool TryResolveTick(ref SystemState state, out uint tick, out float fixedDt)
        {
            tick = 0u;
            fixedDt = 0f;

            if (SystemAPI.TryGetSingleton<TickTimeState>(out var tickTimeState))
            {
                tick = tickTimeState.Tick;
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

            return tick != 0 || fixedDt > 0f;
        }
    }

    [Serializable]
    internal sealed class JobLoopScenarioMeta
    {
        public uint seed;
        public float duration_s;
        public float min_invariant_s;
    }

    internal static class JobLoopScenarioUtility
    {
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        internal const string ScenarioFile = "job_loop_01.json";
        internal const string ScenarioIdText = "scenario.godgame.job_loop_01";
        internal const float DefaultMinInvariantSeconds = 10f;
        internal const uint DefaultSeed = 12345;

        internal static bool TryResolveScenarioPath(ref SystemState state, out string resolvedPath)
        {
            var envPath = SystemEnv.GetEnvironmentVariable(ScenarioEnvVar);
            var candidate = string.IsNullOrWhiteSpace(envPath) ? null : envPath;

            if (string.IsNullOrWhiteSpace(candidate))
            {
                if (!SystemAPI.TryGetSingleton<ScenarioOptions>(out var options))
                {
                    resolvedPath = string.Empty;
                    return false;
                }

                candidate = options.ScenarioPath.ToString();
            }

            resolvedPath = ResolveScenarioPath(candidate);
            return !string.IsNullOrWhiteSpace(resolvedPath);
        }

        internal static string ResolveScenarioPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return string.Empty;
            }

            var trimmed = rawPath.Trim().Trim('"');
            if (Path.IsPathRooted(trimmed))
            {
                return Path.GetFullPath(trimmed);
            }

            var combined = Path.Combine(Application.dataPath, trimmed);
            return Path.GetFullPath(combined);
        }

        internal static bool IsJobLoopScenario(string scenarioPath)
        {
            var fileName = Path.GetFileName(scenarioPath);
            return string.Equals(fileName, ScenarioFile, StringComparison.OrdinalIgnoreCase);
        }

        internal static uint ResolveSeed(string scenarioPath)
        {
            if (TryLoadMeta(scenarioPath, out var meta) && meta.seed != 0)
            {
                return meta.seed;
            }

            return DefaultSeed;
        }

        internal static bool TryLoadMeta(string scenarioPath, out JobLoopScenarioMeta meta)
        {
            meta = null;
            if (string.IsNullOrWhiteSpace(scenarioPath) || !File.Exists(scenarioPath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(scenarioPath);
                meta = JsonUtility.FromJson<JobLoopScenarioMeta>(json);
                return meta != null;
            }
            catch
            {
                return false;
            }
        }

        internal static void EnsureScenarioInfo(ref SystemState state, uint seed)
        {
            var scenarioId = new FixedString64Bytes(ScenarioIdText);
            if (SystemAPI.TryGetSingletonRW<ScenarioInfo>(out var infoRw))
            {
                var updated = infoRw.ValueRW;
                updated.ScenarioId = scenarioId;
                updated.Seed = seed;
                infoRw.ValueRW = updated;
                return;
            }

            var scenarioEntity = state.EntityManager.CreateEntity(typeof(ScenarioInfo));
            state.EntityManager.SetComponentData(scenarioEntity, new ScenarioInfo
            {
                ScenarioId = scenarioId,
                Seed = seed,
                RunTicks = 0
            });
        }

        internal static uint ResolveMinTicks(float minSeconds, float fixedDt)
        {
            var safeSeconds = minSeconds > 0f ? minSeconds : DefaultMinInvariantSeconds;
            var safeDt = fixedDt > 1e-4f ? fixedDt : 1f / 60f;
            var ticks = (uint)math.ceil(safeSeconds / safeDt);
            return ticks > 0u ? ticks : 1u;
        }
    }
}
