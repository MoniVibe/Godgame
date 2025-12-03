using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Generates parametric variants of prefab templates (e.g., "Iron Sword +1, +2, +3").
    /// </summary>
    public static class ParametricVariantGenerator
    {
        /// <summary>
        /// Generate variants of an equipment template with incremental stat boosts.
        /// </summary>
        public static List<EquipmentTemplate> GenerateEquipmentVariants(
            EquipmentTemplate baseTemplate,
            int variantCount,
            VariantConfig config)
        {
            var variants = new List<EquipmentTemplate>();
            
            for (int i = 1; i <= variantCount; i++)
            {
                var variant = CloneTemplate(baseTemplate);
                var baseStats = baseTemplate.stats ?? new EquipmentStats();
                
                // Apply stat modifiers
                variant.displayName = $"{baseTemplate.displayName} +{i}";
                variant.name = $"{baseTemplate.name}_Plus{i}";
                
                // Scale stats based on variant level
                float multiplier = 1f + (config.statMultiplierPerLevel * i);
                
                variant.stats.damage = baseStats.damage * multiplier;
                variant.stats.armor = baseStats.armor * multiplier;
                variant.baseDurability = baseTemplate.baseDurability * multiplier;
                variant.mass = baseTemplate.mass * multiplier;
                
                // Adjust quality/rarity
                variant.quality = baseTemplate.quality + (config.qualityBonusPerLevel * i);
                variant.rarity = CalculateRarityForLevel(baseTemplate.rarity, i, config);
                
                // Adjust tech tier if needed
                if (config.techTierIncrement > 0 && i % config.techTierIncrement == 0)
                {
                    var incrementedTier = baseTemplate.techTier + (i / config.techTierIncrement);
                    variant.techTier = (byte)Mathf.Clamp(incrementedTier, byte.MinValue, byte.MaxValue);
                }
                
                variants.Add(variant);
            }
            
            return variants;
        }
        
        /// <summary>
        /// Generate variants of a tool template with incremental quality/durability boosts.
        /// </summary>
        public static List<ToolTemplate> GenerateToolVariants(
            ToolTemplate baseTemplate,
            int variantCount,
            VariantConfig config)
        {
            var variants = new List<ToolTemplate>();
            
            for (int i = 1; i <= variantCount; i++)
            {
                var variant = CloneTemplate(baseTemplate);
                
                variant.displayName = $"{baseTemplate.displayName} +{i}";
                variant.name = $"{baseTemplate.name}_Plus{i}";
                
                float multiplier = 1f + (config.statMultiplierPerLevel * i);
                
                variant.durabilityBonus = baseTemplate.durabilityBonus * multiplier;
                variant.workEfficiency = baseTemplate.workEfficiency * multiplier;
                variant.quality = baseTemplate.quality + (config.qualityBonusPerLevel * i);
                variant.rarity = CalculateRarityForLevel(baseTemplate.rarity, i, config);
                
                variants.Add(variant);
            }
            
            return variants;
        }
        
        /// <summary>
        /// Generate variants of a material template with incremental purity/quality boosts.
        /// </summary>
        public static List<MaterialTemplate> GenerateMaterialVariants(
            MaterialTemplate baseTemplate,
            int variantCount,
            VariantConfig config)
        {
            var variants = new List<MaterialTemplate>();
            
            for (int i = 1; i <= variantCount; i++)
            {
                var variant = CloneTemplate(baseTemplate);
                
                variant.displayName = $"{baseTemplate.displayName} +{i}";
                variant.name = $"{baseTemplate.name}_Plus{i}";
                
                variant.purity = Mathf.Clamp01(baseTemplate.purity + (config.purityBonusPerLevel * i));
                variant.quality = baseTemplate.quality + (config.qualityBonusPerLevel * i);
                variant.rarity = CalculateRarityForLevel(baseTemplate.rarity, i, config);
                
                variants.Add(variant);
            }
            
            return variants;
        }
        
        private static EquipmentTemplate CloneTemplate(EquipmentTemplate source)
        {
            return new EquipmentTemplate
            {
                id = source.id, // Will be reassigned by caller
                name = source.name,
                displayName = source.displayName,
                equipmentType = source.equipmentType,
                slotKind = source.slotKind,
                stats = source.stats != null ? new EquipmentStats
                {
                    damage = source.stats.damage,
                    armor = source.stats.armor,
                    blockChance = source.stats.blockChance,
                    critChance = source.stats.critChance,
                    critDamage = source.stats.critDamage,
                    weight = source.stats.weight,
                    encumbrance = source.stats.encumbrance,
                    calculatedDamage = source.stats.calculatedDamage,
                    calculatedArmor = source.stats.calculatedArmor,
                    calculatedDurability = source.stats.calculatedDurability
                } : new EquipmentStats(),
                baseDurability = source.baseDurability,
                mass = source.mass,
                requiredUsage = source.requiredUsage,
                requiredTraits = source.requiredTraits,
                forbiddenTraits = source.forbiddenTraits,
                minStats = source.minStats,
                quality = source.quality,
                rarity = source.rarity,
                techTier = source.techTier,
                requiredTechTier = source.requiredTechTier,
                itemDefId = source.itemDefId,
                bonuses = source.bonuses != null ? new List<PrefabBonus>(source.bonuses) : new List<PrefabBonus>(),
                visualPresentation = source.visualPresentation,
                vfxPresentation = source.vfxPresentation,
                productionFacilities = source.productionFacilities != null ? new List<string>(source.productionFacilities) : new List<string>()
            };
        }
        
        private static ToolTemplate CloneTemplate(ToolTemplate source)
        {
            return new ToolTemplate
            {
                id = source.id,
                name = source.name,
                displayName = source.displayName,
                durabilityBonus = source.durabilityBonus,
                workEfficiency = source.workEfficiency,
                quality = source.quality,
                rarity = source.rarity,
                techTier = source.techTier,
                requiredTechTier = source.requiredTechTier,
                itemDefId = source.itemDefId,
                recipeId = source.recipeId,
                productionInputs = source.productionInputs != null ? source.productionInputs.Select(input => new ProductionInput
                {
                    materialName = input.materialName,
                    materialId = input.materialId,
                    quantity = input.quantity,
                    minPurity = input.minPurity,
                    minQuality = input.minQuality,
                    minTechTier = input.minTechTier,
                    isOptional = input.isOptional
                }).ToList() : new List<ProductionInput>(),
                qualityDerivation = source.qualityDerivation != null ? new QualityDerivation
                {
                    materialPurityWeight = source.qualityDerivation.materialPurityWeight,
                    materialQualityWeight = source.qualityDerivation.materialQualityWeight,
                    craftsmanSkillWeight = source.qualityDerivation.craftsmanSkillWeight,
                    forgeQualityWeight = source.qualityDerivation.forgeQualityWeight,
                    baseQualityMultiplier = source.qualityDerivation.baseQualityMultiplier,
                    minQuality = source.qualityDerivation.minQuality,
                    maxQuality = source.qualityDerivation.maxQuality
                } : new QualityDerivation(),
                baseFacilityQuality = source.baseFacilityQuality,
                constructionSpeedBonus = source.constructionSpeedBonus,
                productionFacilities = source.productionFacilities != null ? new List<string>(source.productionFacilities) : new List<string>(),
                possibleAttributes = source.possibleAttributes != null ? new List<MaterialAttribute>(source.possibleAttributes) : new List<MaterialAttribute>(),
                baseQuality = source.baseQuality,
                calculatedQuality = source.calculatedQuality,
                producedFrom = source.producedFrom
            };
        }
        
        private static MaterialTemplate CloneTemplate(MaterialTemplate source)
        {
            return new MaterialTemplate
            {
                id = source.id,
                name = source.name,
                displayName = source.displayName,
                category = source.category,
                usage = source.usage,
                purity = source.purity,
                quality = source.quality,
                rarity = source.rarity,
                techTier = source.techTier,
                requiredTechTier = source.requiredTechTier,
                materialId = source.materialId,
                productionFacilities = source.productionFacilities != null ? new List<string>(source.productionFacilities) : new List<string>()
            };
        }
        
        private static Rarity CalculateRarityForLevel(Rarity baseRarity, int level, VariantConfig config)
        {
            int rarityValue = (int)baseRarity;
            int increment = config.rarityIncrementPerLevel;
            
            int newRarityValue = rarityValue + (increment * level);
            
            // Clamp to valid enum range
            int maxRarity = Enum.GetValues(typeof(Rarity)).Cast<int>().Max();
            int minRarity = Enum.GetValues(typeof(Rarity)).Cast<int>().Min();
            
            newRarityValue = Mathf.Clamp(newRarityValue, minRarity, maxRarity);
            
            return (Rarity)newRarityValue;
        }
    }
    
    /// <summary>
    /// Configuration for generating parametric variants.
    /// </summary>
    [Serializable]
    public class VariantConfig
    {
        /// <summary>
        /// Stat multiplier per variant level (e.g., 0.1 = +10% per level).
        /// </summary>
        public float statMultiplierPerLevel = 0.1f;
        
        /// <summary>
        /// Quality bonus per variant level.
        /// </summary>
        public float qualityBonusPerLevel = 5f;
        
        /// <summary>
        /// Purity bonus per variant level (for materials).
        /// </summary>
        public float purityBonusPerLevel = 0.05f;
        
        /// <summary>
        /// Rarity increment per variant level (enum steps).
        /// </summary>
        public int rarityIncrementPerLevel = 0; // 0 = no auto-increment
        
        /// <summary>
        /// Tech tier increment threshold (every N levels, increment tech tier).
        /// </summary>
        public int techTierIncrement = 0; // 0 = no auto-increment
    }
}

