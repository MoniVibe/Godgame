using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Runtime hooks: Scenario seeding, content budgets, spawn packs.
    /// </summary>
    public static class RuntimeHooks
    {
        /// <summary>
        /// Generate a scenario seed pack (initial prefabs for a scenario).
        /// </summary>
        public static ScenarioSeedPack GenerateScenarioSeed(
            ScenarioSeedConfig config,
            List<BuildingTemplate> buildingTemplates,
            List<IndividualTemplate> individualTemplates,
            List<EquipmentTemplate> equipmentTemplates,
            List<MaterialTemplate> materialTemplates)
        {
            var pack = new ScenarioSeedPack
            {
                scenarioName = config.scenarioName,
                buildings = new List<ScenarioSeedEntry>(),
                individuals = new List<ScenarioSeedEntry>(),
                equipment = new List<ScenarioSeedEntry>(),
                materials = new List<ScenarioSeedEntry>()
            };
            
            // Select buildings based on config
            if (config.buildingCounts != null)
            {
                foreach (var kvp in config.buildingCounts)
                {
                    var template = buildingTemplates.FirstOrDefault(b => b.name == kvp.Key);
                    if (template != null)
                    {
                        pack.buildings.Add(new ScenarioSeedEntry
                        {
                            templateName = template.name,
                            count = kvp.Value,
                            spawnPosition = Vector3.zero // Will be set by scenario system
                        });
                    }
                }
            }
            
            // Select individuals
            if (config.individualCounts != null)
            {
                foreach (var kvp in config.individualCounts)
                {
                    var template = individualTemplates.FirstOrDefault(i => i.name == kvp.Key);
                    if (template != null)
                    {
                        pack.individuals.Add(new ScenarioSeedEntry
                        {
                            templateName = template.name,
                            count = kvp.Value
                        });
                    }
                }
            }
            
            // Select equipment
            if (config.equipmentCounts != null)
            {
                foreach (var kvp in config.equipmentCounts)
                {
                    var template = equipmentTemplates.FirstOrDefault(e => e.name == kvp.Key);
                    if (template != null)
                    {
                        pack.equipment.Add(new ScenarioSeedEntry
                        {
                            templateName = template.name,
                            count = kvp.Value
                        });
                    }
                }
            }
            
            // Select materials
            if (config.materialCounts != null)
            {
                foreach (var kvp in config.materialCounts)
                {
                    var template = materialTemplates.FirstOrDefault(m => m.name == kvp.Key);
                    if (template != null)
                    {
                        pack.materials.Add(new ScenarioSeedEntry
                        {
                            templateName = template.name,
                            count = kvp.Value
                        });
                    }
                }
            }
            
            return pack;
        }
        
        /// <summary>
        /// Calculate content budget (how many prefabs of each type can be generated).
        /// </summary>
        public static ContentBudget CalculateContentBudget(
            ContentBudgetConfig config,
            List<BuildingTemplate> buildingTemplates,
            List<IndividualTemplate> individualTemplates,
            List<EquipmentTemplate> equipmentTemplates,
            List<MaterialTemplate> materialTemplates)
        {
            var budget = new ContentBudget
            {
                maxBuildings = config.maxBuildings,
                maxIndividuals = config.maxIndividuals,
                maxEquipment = config.maxEquipment,
                maxMaterials = config.maxMaterials,
                currentBuildings = buildingTemplates.Count,
                currentIndividuals = individualTemplates.Count,
                currentEquipment = equipmentTemplates.Count,
                currentMaterials = materialTemplates.Count
            };
            
            budget.remainingBuildings = budget.maxBuildings - budget.currentBuildings;
            budget.remainingIndividuals = budget.maxIndividuals - budget.currentIndividuals;
            budget.remainingEquipment = budget.maxEquipment - budget.currentEquipment;
            budget.remainingMaterials = budget.maxMaterials - budget.currentMaterials;
            
            return budget;
        }
        
        /// <summary>
        /// Generate a spawn pack (prefabs to spawn at runtime).
        /// </summary>
        public static SpawnPack GenerateSpawnPack(
            SpawnPackConfig config,
            List<BuildingTemplate> buildingTemplates,
            List<IndividualTemplate> individualTemplates,
            List<EquipmentTemplate> equipmentTemplates)
        {
            var pack = new SpawnPack
            {
                packName = config.packName,
                entries = new List<SpawnPackEntry>()
            };
            
            // Select random templates based on config
            if (config.buildingCount > 0 && buildingTemplates.Count > 0)
            {
                var selected = buildingTemplates.OrderBy(x => UnityEngine.Random.value).Take(config.buildingCount);
                foreach (var template in selected)
                {
                    pack.entries.Add(new SpawnPackEntry
                    {
                        templateName = template.name,
                        templateType = "Building",
                        spawnChance = config.spawnChance
                    });
                }
            }
            
            if (config.individualCount > 0 && individualTemplates.Count > 0)
            {
                var selected = individualTemplates.OrderBy(x => UnityEngine.Random.value).Take(config.individualCount);
                foreach (var template in selected)
                {
                    pack.entries.Add(new SpawnPackEntry
                    {
                        templateName = template.name,
                        templateType = "Individual",
                        spawnChance = config.spawnChance
                    });
                }
            }
            
            if (config.equipmentCount > 0 && equipmentTemplates.Count > 0)
            {
                var selected = equipmentTemplates.OrderBy(x => UnityEngine.Random.value).Take(config.equipmentCount);
                foreach (var template in selected)
                {
                    pack.entries.Add(new SpawnPackEntry
                    {
                        templateName = template.name,
                        templateType = "Equipment",
                        spawnChance = config.spawnChance
                    });
                }
            }
            
            return pack;
        }
    }
    
    /// <summary>
    /// Configuration for scenario seeding.
    /// </summary>
    [Serializable]
    public class ScenarioSeedConfig
    {
        public string scenarioName;
        public Dictionary<string, int> buildingCounts = new Dictionary<string, int>();
        public Dictionary<string, int> individualCounts = new Dictionary<string, int>();
        public Dictionary<string, int> equipmentCounts = new Dictionary<string, int>();
        public Dictionary<string, int> materialCounts = new Dictionary<string, int>();
    }
    
    /// <summary>
    /// Scenario seed pack (initial prefabs for a scenario).
    /// </summary>
    [Serializable]
    public class ScenarioSeedPack
    {
        public string scenarioName;
        public List<ScenarioSeedEntry> buildings = new List<ScenarioSeedEntry>();
        public List<ScenarioSeedEntry> individuals = new List<ScenarioSeedEntry>();
        public List<ScenarioSeedEntry> equipment = new List<ScenarioSeedEntry>();
        public List<ScenarioSeedEntry> materials = new List<ScenarioSeedEntry>();
    }
    
    /// <summary>
    /// Entry in a scenario seed pack.
    /// </summary>
    [Serializable]
    public class ScenarioSeedEntry
    {
        public string templateName;
        public int count;
        public Vector3 spawnPosition;
    }
    
    /// <summary>
    /// Content budget configuration.
    /// </summary>
    [Serializable]
    public class ContentBudgetConfig
    {
        public int maxBuildings = 100;
        public int maxIndividuals = 50;
        public int maxEquipment = 200;
        public int maxMaterials = 100;
    }
    
    /// <summary>
    /// Content budget (how many prefabs can be generated).
    /// </summary>
    [Serializable]
    public class ContentBudget
    {
        public int maxBuildings;
        public int maxIndividuals;
        public int maxEquipment;
        public int maxMaterials;
        
        public int currentBuildings;
        public int currentIndividuals;
        public int currentEquipment;
        public int currentMaterials;
        
        public int remainingBuildings;
        public int remainingIndividuals;
        public int remainingEquipment;
        public int remainingMaterials;
    }
    
    /// <summary>
    /// Spawn pack configuration.
    /// </summary>
    [Serializable]
    public class SpawnPackConfig
    {
        public string packName;
        public int buildingCount = 0;
        public int individualCount = 0;
        public int equipmentCount = 0;
        public float spawnChance = 1.0f;
    }
    
    /// <summary>
    /// Spawn pack (prefabs to spawn at runtime).
    /// </summary>
    [Serializable]
    public class SpawnPack
    {
        public string packName;
        public List<SpawnPackEntry> entries = new List<SpawnPackEntry>();
    }
    
    /// <summary>
    /// Entry in a spawn pack.
    /// </summary>
    [Serializable]
    public class SpawnPackEntry
    {
        public string templateName;
        public string templateType;
        public float spawnChance;
    }
}

