using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Components;

namespace Godgame.Scenario
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class GodgameScenarioLoaderSystem : SystemBase
    {
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        private const string CollisionScenarioFile = "godgame_collision_micro.json";
        private bool _loaded;
        private bool _didWarnMissingConfig;
        private bool _loggedSettlementConfigCount;
        private bool _loggedScenarioSkip;
        private EntityQuery _settlementConfigQuery;

        protected override void OnCreate()
        {
            _settlementConfigQuery = GetEntityQuery(ComponentType.ReadOnly<SettlementConfig>());
            RequireForUpdate<TimeState>();
        }

        protected override void OnUpdate()
        {
            if (_loaded)
            {
                Enabled = false;
                return;
            }

            if (!SystemAPI.TryGetSingleton<ScenarioOptions>(out var options))
            {
                return; // Wait for options
            }

            if (!SystemAPI.TryGetSingleton<GodgameScenarioConfigBlobReference>(out var scenarioConfig) ||
                !scenarioConfig.Config.IsCreated)
            {
                return; // Wait for scenario config
            }

            if (scenarioConfig.Config.Value.Mode == GodgameScenarioMode.Scenario01 &&
                !RuntimeMode.IsHeadless &&
                !IsScenarioOverrideRequested())
            {
                if (!_loggedScenarioSkip)
                {
                    Debug.Log("[GodgameScenarioLoaderSystem] Scenario01 active; skipping JSON scenario load.");
                    _loggedScenarioSkip = true;
                }
                _loaded = true;
                Enabled = false;
                return;
            }

            if (_settlementConfigQuery.IsEmptyIgnoreFilter)
            {
                WarnMissingConfig();
                return;
            }

            CleanupDuplicateConfigs();

            var settlementConfig = SystemAPI.GetSingleton<SettlementConfig>();
            var timeState = SystemAPI.GetSingleton<TimeState>();

            ClearMissingConfigWarning();
            _loaded = true;
            LoadScenario(options.ScenarioPath.ToString(), settlementConfig, timeState);
        }

        private void CleanupDuplicateConfigs()
        {
            int count = _settlementConfigQuery.CalculateEntityCount();
            if (count <= 1)
                return;

            Debug.LogError($"[GodgameScenarioLoaderSystem] Found {count} SettlementConfig entities! Destroying duplicates...");
            using var entities = _settlementConfigQuery.ToEntityArray(Allocator.Temp);
            for (int i = 1; i < entities.Length; i++)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }

        private void WarnMissingConfig()
        {
            LogSettlementConfigCountOnce();

            if (_didWarnMissingConfig)
                return;

            Debug.LogWarning("[GodgameScenarioLoaderSystem] Waiting for SettlementConfig SubScene to load...");
            _didWarnMissingConfig = true;
        }

        private void LogSettlementConfigCountOnce()
        {
            if (_loggedSettlementConfigCount)
                return;

            Debug.Log($"[GodgameScenarioLoaderSystem] SettlementConfig count={_settlementConfigQuery.CalculateEntityCount()}");
            _loggedSettlementConfigCount = true;
        }

        private void ClearMissingConfigWarning()
        {
            _didWarnMissingConfig = false;
        }

        private void LoadScenario(string path, SettlementConfig settlementConfig, TimeState timeState)
        {
            var fullPath = ResolveScenarioPath(path);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[GodgameScenarioLoaderSystem] Scenario file not found: {fullPath}");
                return;
            }

            string json = File.ReadAllText(fullPath);
            var scenarioData = JsonUtility.FromJson<ScenarioData>(json);
            var isCollisionScenario = fullPath.EndsWith(CollisionScenarioFile, StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[GodgameScenarioLoaderSystem] Loading scenario: {scenarioData.name}");

            var configEntity = EntityManager.CreateEntity();
            var config = new GodgameScenarioSpawnConfig
            {
                VillagerPrefab = settlementConfig.VillagerPrefab,
                VillageCenterPrefab = settlementConfig.VillageCenterPrefab,
                StorehousePrefab = settlementConfig.StorehousePrefab,
                HousingPrefab = settlementConfig.HousingPrefab,
                WorshipPrefab = settlementConfig.WorshipPrefab,
                SpawnRadius = 20f,
                Seed = 12345
            };
            Debug.Log($"[GodgameScenarioLoaderSystem] SettlementConfig prefabs villager={settlementConfig.VillagerPrefab} storehouse={settlementConfig.StorehousePrefab} center={settlementConfig.VillageCenterPrefab} world={World.Name}");

            // Parse entities from JSON
            int resourceNodeCount = 0;
            DynamicBuffer<GodgameScenarioSpawnOverride> overrides = default;
            if (isCollisionScenario)
            {
                overrides = EntityManager.AddBuffer<GodgameScenarioSpawnOverride>(configEntity);
            }
            foreach (var entityData in scenarioData.entities)
            {
                if (entityData.prefab == "Villager")
                {
                    config.VillagerCount += entityData.count;
                }
                else if (entityData.prefab == "VillageCenter")
                {
                    config.VillageCenterCount += entityData.count;
                }
                else if (entityData.prefab == "Storehouse")
                {
                    config.StorehouseCount += entityData.count;
                }
                else if (entityData.prefab == "Housing")
                {
                    config.HousingCount += entityData.count;
                }
                else if (entityData.prefab == "Worship" || entityData.prefab == "WorshipSite")
                {
                    config.WorshipCount += entityData.count;
                }
                else if (entityData.prefab == "Tree" || entityData.prefab == "ResourceNode")
                {
                    resourceNodeCount += entityData.count;
                }

                if (isCollisionScenario)
                {
                    var kind = ResolveOverrideKind(entityData.prefab);
                    if (kind != GodgameScenarioSpawnKind.None && entityData.count > 0)
                    {
                        overrides.Add(new GodgameScenarioSpawnOverride
                        {
                            Kind = kind,
                            Position = entityData.position,
                            Count = entityData.count
                        });
                    }
                }
            }

            config.ResourceNodeCount = resourceNodeCount;

            EntityManager.AddComponentData(configEntity, config);
            var runtime = new GodgameScenarioRuntime
            {
                HasSpawned = 0,
                RunTicks = scenarioData.runTicks > 0 ? (uint)scenarioData.runTicks : 0u,
                DurationSeconds = scenarioData.duration_s > 0f ? scenarioData.duration_s : 0f
            };
            EntityManager.AddComponentData(configEntity, runtime);

            ScheduleScenarioActions(configEntity, timeState.Tick, timeState.FixedDeltaTime, scenarioData.actions);

            if (runtime.RunTicks > 0 && SystemAPI.TryGetSingletonRW<PureDOTS.Runtime.Scenarios.ScenarioInfo>(out var scenarioInfo))
            {
                scenarioInfo.ValueRW.RunTicks = (int)runtime.RunTicks;
            }
            
            Debug.Log($"[GodgameScenarioLoaderSystem] Configured scenario with {config.VillagerCount} villagers, {config.StorehouseCount} storehouses, {config.VillageCenterCount} centers, {config.HousingCount} housing, {config.WorshipCount} worship.");
        }

        private static GodgameScenarioSpawnKind ResolveOverrideKind(string prefab)
        {
            return prefab switch
            {
                "Villager" => GodgameScenarioSpawnKind.Villager,
                "VillageCenter" => GodgameScenarioSpawnKind.VillageCenter,
                "Storehouse" => GodgameScenarioSpawnKind.Storehouse,
                "Housing" => GodgameScenarioSpawnKind.Housing,
                "Worship" => GodgameScenarioSpawnKind.Worship,
                _ => GodgameScenarioSpawnKind.None
            };
        }

        private static string ResolveScenarioPath(string relativePath)
        {
            var envPath = System.Environment.GetEnvironmentVariable(ScenarioEnvVar);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                var normalized = Path.GetFullPath(envPath);
                if (File.Exists(normalized))
                {
                    return normalized;
                }
                Debug.LogWarning($"[GodgameScenarioLoaderSystem] GODGAME_SCENARIO_PATH set to '{envPath}' but file was not found. Falling back to project Assets path.");
            }

            var safeRelative = relativePath ?? string.Empty;
            var combined = Path.Combine(Application.dataPath, safeRelative);
            return Path.GetFullPath(combined);
        }

        private void ScheduleScenarioActions(Entity configEntity, uint startTick, float fixedDt, ScenarioActionData[] actions)
        {
            if (actions == null || actions.Length == 0)
            {
                return;
            }

            if (EntityManager.HasBuffer<GodgameScenarioAction>(configEntity))
            {
                return;
            }

            var actionsBuffer = EntityManager.AddBuffer<GodgameScenarioAction>(configEntity);
            var safeDt = math.max(1e-6f, fixedDt);

            foreach (var action in actions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.kind))
                {
                    continue;
                }

                var kind = ParseScenarioActionKind(action.kind);
                var executeTick = startTick + (uint)math.ceil(math.max(0f, action.time_s) / safeDt);

                actionsBuffer.Add(new GodgameScenarioAction
                {
                    ExecuteTick = executeTick,
                    Kind = kind,
                    BusinessId = new FixedString64Bytes(action.businessId ?? string.Empty),
                    ItemId = new FixedString64Bytes(action.itemId ?? string.Empty),
                    RecipeId = new FixedString64Bytes(action.recipeId ?? string.Empty),
                    Quantity = action.quantity,
                    Capacity = action.capacity,
                    BusinessType = (byte)ParseBusinessType(action.businessType),
                    Executed = 0
                });
            }
        }

        private static GodgameScenarioActionKind ParseScenarioActionKind(string kind)
        {
            return kind switch
            {
                "EconomyEnable" => GodgameScenarioActionKind.EconomyEnable,
                "ProdCreateBusiness" => GodgameScenarioActionKind.ProdCreateBusiness,
                "ProdAddItem" => GodgameScenarioActionKind.ProdAddItem,
                "ProdRequest" => GodgameScenarioActionKind.ProdRequest,
                _ => GodgameScenarioActionKind.EconomyEnable
            };
        }

        private static PureDOTS.Runtime.Economy.Production.BusinessType ParseBusinessType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return PureDOTS.Runtime.Economy.Production.BusinessType.Blacksmith;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "sawmill":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Sawmill;
                case "quarry":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Quarry;
                case "mill":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Mill;
                case "herbalist":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Herbalist;
                case "wainwright":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Wainwright;
                case "builder":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Builder;
                case "alchemist":
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Alchemist;
                case "blacksmith":
                default:
                    return PureDOTS.Runtime.Economy.Production.BusinessType.Blacksmith;
            }
        }

        private static bool IsScenarioOverrideRequested()
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(ScenarioEnvVar)))
            {
                return true;
            }

            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.Equals(arg, "--scenario", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var prefix = "--scenario=";
                if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        [System.Serializable]
        private class ScenarioData
        {
            public string name;
            public string description;
            public float duration_s;
            public int runTicks;
            public EntityData[] entities;
            public ScenarioActionData[] actions;
        }

        [System.Serializable]
        private class EntityData
        {
            public int count;
            public Vector3 position;
            public string prefab;
            public float radius;
        }

        [System.Serializable]
        private class ScenarioActionData
        {
            public float time_s;
            public string kind;
            public string businessId;
            public string businessType;
            public string itemId;
            public string recipeId;
            public float quantity;
            public float capacity;
        }
    }
}
