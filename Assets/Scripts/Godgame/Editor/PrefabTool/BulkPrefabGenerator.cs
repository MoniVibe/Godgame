using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Bulk prefab generation utility for creating multiple prefabs from patterns.
    /// Supports:
    /// - One material → multiple item templates (e.g., "Iron Sword", "Iron Mace", "Iron Screws")
    /// - One item type → all materials (e.g., "Iron Sword", "Gold Sword", "Orichalcum Sword")
    /// </summary>
    public static class BulkPrefabGenerator
    {
        /// <summary>
        /// Configuration for bulk generation patterns.
        /// </summary>
        [System.Serializable]
        public class BulkGenerationConfig
        {
            public enum GenerationMode
            {
                MaterialToItems,    // One material → multiple item types
                ItemToMaterials      // One item type → all materials
            }

            public GenerationMode mode = GenerationMode.MaterialToItems;
            
            // Source material (for MaterialToItems mode)
            public MaterialTemplate sourceMaterial;
            
            // Item type names (for MaterialToItems mode)
            public List<string> itemTypeNames = new List<string> { "Sword", "Mace", "Axe" };
            
            // Base item template (for ItemToMaterials mode)
            public EquipmentTemplate baseItemTemplate;
            public ToolTemplate baseToolTemplate;
            
            // Naming pattern
            public string namingPattern = "{Material} {ItemType}"; // e.g., "Iron Sword"
            
            // Default stats to apply
            public float defaultQuality = 50f;
            public Rarity defaultRarity = Rarity.Common;
            public byte defaultTechTier = 0;
            
            // Equipment-specific defaults
            public EquipmentTemplate.EquipmentType defaultEquipmentType = EquipmentTemplate.EquipmentType.Weapon;
            public SlotKind defaultSlotKind = SlotKind.Hand;
            public float defaultDamage = 10f;
            public float defaultDurability = 100f;
            
            // Tool-specific defaults
            public float defaultConstructionSpeedBonus = 0f;
            public float defaultDurabilityBonus = 0f;
            public float defaultWorkEfficiency = 1f;
            
            // Material requirements (inherit from source material or base template)
            public bool inheritMaterialRequirements = true;
            
            // Category filter (optional - only generate for materials matching filter)
            public MaterialCategory? categoryFilter = null;
        }

        /// <summary>
        /// Generate multiple equipment templates from one material.
        /// </summary>
        public static List<EquipmentTemplate> GenerateEquipmentFromMaterial(
            MaterialTemplate material,
            List<string> itemTypeNames,
            BulkGenerationConfig config)
        {
            var results = new List<EquipmentTemplate>();
            
            if (material == null)
            {
                Debug.LogError("BulkPrefabGenerator: Source material is null");
                return results;
            }

            foreach (var itemTypeName in itemTypeNames)
            {
                if (string.IsNullOrEmpty(itemTypeName))
                    continue;

                var template = new EquipmentTemplate
                {
                    name = GenerateName(config.namingPattern, material.name, itemTypeName),
                    displayName = GenerateName(config.namingPattern, material.displayName ?? material.name, itemTypeName),
                    description = $"A {itemTypeName.ToLower()} made from {material.displayName ?? material.name}.",
                    id = 0, // Will be assigned by caller
                    
                    // Quality/Rarity/Tech Tier
                    quality = config.defaultQuality,
                    rarity = config.defaultRarity,
                    techTier = config.defaultTechTier,
                    requiredTechTier = config.defaultTechTier,
                    
                    // Equipment properties
                    equipmentType = config.defaultEquipmentType,
                    slotKind = config.defaultSlotKind,
                    baseDurability = config.defaultDurability,
                    
                    // Stats
                    stats = new EquipmentStats
                    {
                        damage = config.defaultDamage
                    },
                    mass = 1f
                };

                // Inherit material requirements from source material
                if (config.inheritMaterialRequirements)
                {
                    template.requiredUsage = material.usage;
                    template.minStats = material.stats != null ? new MaterialStats
                    {
                        hardness = material.stats.hardness,
                        toughness = material.stats.toughness,
                        density = material.stats.density,
                        meltingPoint = material.stats.meltingPoint,
                        conductivity = material.stats.conductivity
                    } : new MaterialStats();
                    template.requiredTraits = material.traits;
                }

                // Initialize required collections
                template.bonuses = new List<PrefabBonus>();
                template.visualPresentation = new VisualPresentation();
                template.vfxPresentation = new VFXPresentation();

                results.Add(template);
            }

            return results;
        }

        /// <summary>
        /// Generate equipment templates for all materials matching a base template pattern.
        /// </summary>
        public static List<EquipmentTemplate> GenerateEquipmentForAllMaterials(
            List<MaterialTemplate> materialCatalog,
            EquipmentTemplate baseTemplate,
            BulkGenerationConfig config)
        {
            var results = new List<EquipmentTemplate>();

            if (baseTemplate == null)
            {
                Debug.LogError("BulkPrefabGenerator: Base equipment template is null");
                return results;
            }

            // Extract item type name from base template
            string itemTypeName = ExtractItemTypeFromName(baseTemplate.name, baseTemplate.displayName);

            foreach (var material in materialCatalog)
            {
                // Apply category filter if specified
                if (config.categoryFilter.HasValue && material.category != config.categoryFilter.Value)
                    continue;

                var template = CloneEquipmentTemplate(baseTemplate);
                
                // Update name using naming pattern
                template.name = GenerateName(config.namingPattern, material.name, itemTypeName);
                template.displayName = GenerateName(config.namingPattern, material.displayName ?? material.name, itemTypeName);
                template.description = $"A {itemTypeName.ToLower()} made from {material.displayName ?? material.name}.";

                // Inherit material properties
                if (config.inheritMaterialRequirements)
                {
                    template.requiredUsage = material.usage;
                    template.minStats = material.stats != null ? new MaterialStats
                    {
                        hardness = material.stats.hardness,
                        toughness = material.stats.toughness,
                        density = material.stats.density,
                        meltingPoint = material.stats.meltingPoint,
                        conductivity = material.stats.conductivity
                    } : new MaterialStats();
                    template.requiredTraits = material.traits;
                }

                // Override defaults if specified
                if (config.defaultQuality > 0)
                    template.quality = config.defaultQuality;
                if (config.defaultTechTier > 0)
                    template.techTier = config.defaultTechTier;

                results.Add(template);
            }

            return results;
        }

        /// <summary>
        /// Generate multiple tool templates from one material.
        /// </summary>
        public static List<ToolTemplate> GenerateToolsFromMaterial(
            MaterialTemplate material,
            List<string> toolTypeNames,
            BulkGenerationConfig config)
        {
            var results = new List<ToolTemplate>();

            if (material == null)
            {
                Debug.LogError("BulkPrefabGenerator: Source material is null");
                return results;
            }

            foreach (var toolTypeName in toolTypeNames)
            {
                if (string.IsNullOrEmpty(toolTypeName))
                    continue;

                var template = new ToolTemplate
                {
                    name = GenerateName(config.namingPattern, material.name, toolTypeName),
                    displayName = GenerateName(config.namingPattern, material.displayName ?? material.name, toolTypeName),
                    description = $"A {toolTypeName.ToLower()} made from {material.displayName ?? material.name}.",
                    id = 0, // Will be assigned by caller
                    
                    // Quality/Rarity/Tech Tier
                    quality = config.defaultQuality,
                    rarity = config.defaultRarity,
                    techTier = config.defaultTechTier,
                    requiredTechTier = config.defaultTechTier,
                    
                    // Tool properties
                    constructionSpeedBonus = config.defaultConstructionSpeedBonus,
                    durabilityBonus = config.defaultDurabilityBonus,
                    workEfficiency = config.defaultWorkEfficiency,
                    
                    // Production chain - uses this material as input
                    productionInputs = new List<ProductionInput>
                    {
                        new ProductionInput
                        {
                            materialName = material.name,
                            quantity = 1f,
                            minPurity = 0f,
                            minQuality = 0f,
                            minTechTier = 0
                        }
                    },
                    qualityDerivation = new QualityDerivation(),
                    baseFacilityQuality = 50f
                };

                // Initialize required collections
                template.bonuses = new List<PrefabBonus>();
                template.visualPresentation = new VisualPresentation();
                template.vfxPresentation = new VFXPresentation();
                template.possibleAttributes = new List<MaterialAttribute>();

                results.Add(template);
            }

            return results;
        }

        /// <summary>
        /// Generate tool templates for all materials matching a base template pattern.
        /// </summary>
        public static List<ToolTemplate> GenerateToolsForAllMaterials(
            List<MaterialTemplate> materialCatalog,
            ToolTemplate baseTemplate,
            BulkGenerationConfig config)
        {
            var results = new List<ToolTemplate>();

            if (baseTemplate == null)
            {
                Debug.LogError("BulkPrefabGenerator: Base tool template is null");
                return results;
            }

            // Extract tool type name from base template
            string toolTypeName = ExtractItemTypeFromName(baseTemplate.name, baseTemplate.displayName);

            foreach (var material in materialCatalog)
            {
                // Apply category filter if specified
                if (config.categoryFilter.HasValue && material.category != config.categoryFilter.Value)
                    continue;

                var template = CloneToolTemplate(baseTemplate);
                
                // Update name using naming pattern
                template.name = GenerateName(config.namingPattern, material.name, toolTypeName);
                template.displayName = GenerateName(config.namingPattern, material.displayName ?? material.name, toolTypeName);
                template.description = $"A {toolTypeName.ToLower()} made from {material.displayName ?? material.name}.";

                // Update production inputs to use this material
                if (template.productionInputs != null && template.productionInputs.Count > 0)
                {
                    // Replace first input with this material
                    template.productionInputs[0].materialName = material.name;
                    template.productionInputs[0].materialId = material.materialId;
                }
                else
                {
                    // Add this material as input
                    template.productionInputs = new List<ProductionInput>
                    {
                        new ProductionInput
                        {
                            materialName = material.name,
                            materialId = material.materialId,
                            quantity = 1f,
                            minPurity = 0f,
                            minQuality = 0f,
                            minTechTier = 0
                        }
                    };
                }

                // Override defaults if specified
                if (config.defaultQuality > 0)
                    template.quality = config.defaultQuality;
                if (config.defaultTechTier > 0)
                    template.techTier = config.defaultTechTier;

                results.Add(template);
            }

            return results;
        }

        /// <summary>
        /// Generate name from pattern (e.g., "{Material} {ItemType}" → "Iron Sword").
        /// </summary>
        private static string GenerateName(string pattern, string materialName, string itemTypeName)
        {
            if (string.IsNullOrEmpty(pattern))
                pattern = "{Material} {ItemType}";

            string result = pattern
                .Replace("{Material}", materialName)
                .Replace("{ItemType}", itemTypeName)
                .Replace("{MATERIAL}", materialName.ToUpper())
                .Replace("{ITEMTYPE}", itemTypeName.ToUpper())
                .Replace("{material}", materialName.ToLower())
                .Replace("{itemtype}", itemTypeName.ToLower());

            // Clean up name (remove invalid characters)
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-zA-Z0-9_]", "_");
            result = result.Replace("__", "_").Trim('_');

            return result;
        }

        /// <summary>
        /// Extract item type name from template name (e.g., "Iron_Sword" → "Sword").
        /// </summary>
        private static string ExtractItemTypeFromName(string name, string displayName)
        {
            // Try to extract from display name first
            if (!string.IsNullOrEmpty(displayName))
            {
                // Look for common patterns: "Material ItemType" or "ItemType of Material"
                var parts = displayName.Split(' ');
                if (parts.Length >= 2)
                {
                    // Assume last part is item type
                    return parts[parts.Length - 1];
                }
            }

            // Fall back to name
            if (!string.IsNullOrEmpty(name))
            {
                var parts = name.Split('_');
                if (parts.Length >= 2)
                {
                    return parts[parts.Length - 1];
                }
            }

            return "Item";
        }

        /// <summary>
        /// Clone an equipment template (deep copy).
        /// </summary>
        private static EquipmentTemplate CloneEquipmentTemplate(EquipmentTemplate source)
        {
            // Use JSON serialization for deep copy (Unity's JsonUtility)
            string json = JsonUtility.ToJson(source);
            var clone = JsonUtility.FromJson<EquipmentTemplate>(json);
            
            // Reinitialize collections (JsonUtility doesn't handle them well)
            clone.bonuses = source.bonuses != null ? new List<PrefabBonus>(source.bonuses) : new List<PrefabBonus>();
            clone.visualPresentation = source.visualPresentation != null ? new VisualPresentation
            {
                prefabAsset = source.visualPresentation.prefabAsset,
                meshAsset = source.visualPresentation.meshAsset,
                spriteAsset = source.visualPresentation.spriteAsset,
                materialOverride = source.visualPresentation.materialOverride,
                scale = source.visualPresentation.scale,
                positionOffset = source.visualPresentation.positionOffset,
                rotationOffset = source.visualPresentation.rotationOffset,
                usePrimitiveFallback = source.visualPresentation.usePrimitiveFallback,
                primitiveType = source.visualPresentation.primitiveType
            } : new VisualPresentation();
            clone.vfxPresentation = source.vfxPresentation != null ? new VFXPresentation
            {
                vfxAsset = source.vfxPresentation.vfxAsset,
                positionOffset = source.vfxPresentation.positionOffset,
                rotationOffset = source.vfxPresentation.rotationOffset,
                playOnAwake = source.vfxPresentation.playOnAwake,
                loop = source.vfxPresentation.loop
            } : new VFXPresentation();
            
            clone.stats = source.stats != null ? new EquipmentStats
            {
                damage = source.stats.damage,
                armor = source.stats.armor,
                blockChance = source.stats.blockChance,
                critChance = source.stats.critChance,
                critDamage = source.stats.critDamage,
                weight = source.stats.weight,
                encumbrance = source.stats.encumbrance
            } : new EquipmentStats();
            
            clone.minStats = source.minStats != null ? new MaterialStats
            {
                hardness = source.minStats.hardness,
                toughness = source.minStats.toughness,
                density = source.minStats.density,
                meltingPoint = source.minStats.meltingPoint,
                conductivity = source.minStats.conductivity
            } : new MaterialStats();

            return clone;
        }

        /// <summary>
        /// Clone a tool template (deep copy).
        /// </summary>
        private static ToolTemplate CloneToolTemplate(ToolTemplate source)
        {
            // Use JSON serialization for deep copy
            string json = JsonUtility.ToJson(source);
            var clone = JsonUtility.FromJson<ToolTemplate>(json);
            
            // Reinitialize collections
            clone.bonuses = source.bonuses != null ? new List<PrefabBonus>(source.bonuses) : new List<PrefabBonus>();
            clone.visualPresentation = source.visualPresentation != null ? new VisualPresentation
            {
                prefabAsset = source.visualPresentation.prefabAsset,
                meshAsset = source.visualPresentation.meshAsset,
                spriteAsset = source.visualPresentation.spriteAsset,
                materialOverride = source.visualPresentation.materialOverride,
                scale = source.visualPresentation.scale,
                positionOffset = source.visualPresentation.positionOffset,
                rotationOffset = source.visualPresentation.rotationOffset,
                usePrimitiveFallback = source.visualPresentation.usePrimitiveFallback,
                primitiveType = source.visualPresentation.primitiveType
            } : new VisualPresentation();
            clone.vfxPresentation = source.vfxPresentation != null ? new VFXPresentation
            {
                vfxAsset = source.vfxPresentation.vfxAsset,
                positionOffset = source.vfxPresentation.positionOffset,
                rotationOffset = source.vfxPresentation.rotationOffset,
                playOnAwake = source.vfxPresentation.playOnAwake,
                loop = source.vfxPresentation.loop
            } : new VFXPresentation();
            
            clone.productionInputs = source.productionInputs != null ? 
                source.productionInputs.Select(input => new ProductionInput
                {
                    materialName = input.materialName,
                    materialId = input.materialId,
                    quantity = input.quantity,
                    minPurity = input.minPurity,
                    minQuality = input.minQuality,
                    minTechTier = input.minTechTier,
                    isOptional = input.isOptional
                }).ToList() : new List<ProductionInput>();
            
            clone.qualityDerivation = source.qualityDerivation != null ? new QualityDerivation
            {
                materialPurityWeight = source.qualityDerivation.materialPurityWeight,
                materialQualityWeight = source.qualityDerivation.materialQualityWeight,
                craftsmanSkillWeight = source.qualityDerivation.craftsmanSkillWeight,
                forgeQualityWeight = source.qualityDerivation.forgeQualityWeight,
                baseQualityMultiplier = source.qualityDerivation.baseQualityMultiplier,
                minQuality = source.qualityDerivation.minQuality,
                maxQuality = source.qualityDerivation.maxQuality
            } : new QualityDerivation();
            
            clone.possibleAttributes = source.possibleAttributes != null ? 
                new List<MaterialAttribute>(source.possibleAttributes) : new List<MaterialAttribute>();

            return clone;
        }
    }
}


