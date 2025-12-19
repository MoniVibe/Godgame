using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Scenario
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class GodgameScenarioLoaderSystem : SystemBase
    {
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        private bool _loaded;
        private bool _didWarnMissingConfig;
        private bool _loggedSettlementConfigCount;
        private EntityQuery _settlementConfigQuery;

        protected override void OnCreate()
        {
            _settlementConfigQuery = GetEntityQuery(ComponentType.ReadOnly<SettlementConfig>());
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

            if (_settlementConfigQuery.IsEmptyIgnoreFilter)
            {
                WarnMissingConfig();
                return;
            }

            CleanupDuplicateConfigs();

            var settlementConfig = SystemAPI.GetSingleton<SettlementConfig>();

            ClearMissingConfigWarning();
            _loaded = true;
            LoadScenario(options.ScenarioPath.ToString(), settlementConfig);
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

        private void LoadScenario(string path, SettlementConfig settlementConfig)
        {
            var fullPath = ResolveScenarioPath(path);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[GodgameScenarioLoaderSystem] Scenario file not found: {fullPath}");
                return;
            }

            string json = File.ReadAllText(fullPath);
            var scenarioData = JsonUtility.FromJson<ScenarioData>(json);

            Debug.Log($"[GodgameScenarioLoaderSystem] Loading scenario: {scenarioData.name}");

            var configEntity = EntityManager.CreateEntity();
            var config = new GodgameScenarioConfig
            {
                VillagerPrefab = settlementConfig.VillagerPrefab,
                StorehousePrefab = settlementConfig.StorehousePrefab,
                SpawnRadius = 20f,
                Seed = 12345
            };

            // Parse entities from JSON
            foreach (var entityData in scenarioData.entities)
            {
                if (entityData.prefab == "Villager")
                {
                    config.VillagerCount += entityData.count;
                }
                else if (entityData.prefab == "Storehouse")
                {
                    config.StorehouseCount += entityData.count;
                }
                else if (entityData.prefab == "Tree") // Assuming Tree maps to ResourceNode for now or ignored
                {
                    // config.ResourceNodeCount += entityData.count; 
                    // We don't have TreePrefab in SettlementConfig usually, maybe map to ResourceNode?
                }
            }
            
            // Hardcode resource nodes for now if not in JSON or to match scenario expectations
            config.ResourceNodeCount = 5; 

            EntityManager.AddComponentData(configEntity, config);
            EntityManager.AddComponentData(configEntity, new GodgameScenarioRuntime());
            
            Debug.Log($"[GodgameScenarioLoaderSystem] Configured scenario with {config.VillagerCount} villagers, {config.StorehouseCount} storehouses.");
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

        [System.Serializable]
        private class ScenarioData
        {
            public string name;
            public string description;
            public EntityData[] entities;
        }

        [System.Serializable]
        private class EntityData
        {
            public int count;
            public Vector3 position;
            public string prefab;
            public float radius;
        }
    }
}
