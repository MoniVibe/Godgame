using System.Collections.Generic;
using System.Linq;
using Godgame.Editor.PrefabTool;
using UnityEngine;
using Unity.Collections;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Quality weights structure matching items.md spec.
    /// </summary>
    [System.Serializable]
    public struct QualityWeights
    {
        public float MaterialPurityWeight;   // default 0.4
        public float MaterialQualityWeight;  // default 0.3
        public float CraftsmanSkillWeight;   // default 0.2
        public float FacilityQualityWeight; // default 0.1

        public static QualityWeights Default => new QualityWeights
        {
            MaterialPurityWeight = 0.4f,
            MaterialQualityWeight = 0.3f,
            CraftsmanSkillWeight = 0.2f,
            FacilityQualityWeight = 0.1f
        };
    }

    /// <summary>
    /// Utility functions for calculating derived stats from materials, tools, and builder skills.
    /// Unified quality/rarity/tech tier calculations per items.md spec.
    /// </summary>
    public static class StatCalculation
    {
        /// <summary>
        /// Calculates building health from materials, tools, and builder skill.
        /// </summary>
        public static float CalculateBuildingHealth(BuildingTemplate template)
        {
            float health = template.baseHealth;

            // Material quality contribution (average of all materials)
            if (template.materials != null && template.materials.Count > 0)
            {
                float avgMaterialQuality = template.materials.Average(m => m.qualityMultiplier);
                float avgHealthBonus = template.materials.Average(m => m.healthBonus);
                health = (health * avgMaterialQuality) + avgHealthBonus;
            }

            // Tool quality contribution
            if (template.tool != null)
            {
                health += template.tool.durabilityBonus;
                health *= (1f + (template.tool.qualityMultiplier - 1f) * 0.3f); // 30% tool influence
            }

            // Builder skill contribution (0-100 maps to 0.7x - 1.3x multiplier)
            float skillMultiplier = 0.7f + (template.builderSkillLevel / 100f) * 0.6f;
            health *= skillMultiplier;

            return Mathf.Max(1f, health);
        }

        /// <summary>
        /// Calculates building desirability from materials, tools, and builder skill.
        /// </summary>
        public static float CalculateBuildingDesirability(BuildingTemplate template)
        {
            float desirability = template.baseDesirability;

            // Material quality contribution
            if (template.materials != null && template.materials.Count > 0)
            {
                float avgMaterialQuality = template.materials.Average(m => m.qualityMultiplier);
                float avgDesirabilityBonus = template.materials.Average(m => m.desirabilityBonus);
                desirability = (desirability * avgMaterialQuality) + avgDesirabilityBonus;
            }

            // Tool quality contribution (lesser influence on desirability)
            if (template.tool != null)
            {
                desirability += template.tool.qualityMultiplier * 5f; // Small bonus
            }

            // Builder skill contribution
            float skillMultiplier = 0.8f + (template.builderSkillLevel / 100f) * 0.4f;
            desirability *= skillMultiplier;

            // Area bonuses (from surrounding buildings)
            // This would be calculated at runtime, but we can add a preview value
            // For now, just return the base calculation

            return Mathf.Max(0f, desirability);
        }

        /// <summary>
        /// Calculates equipment durability from material quality and stats.
        /// </summary>
        public static float CalculateEquipmentDurability(EquipmentTemplate template, MaterialTemplate material = null)
        {
            float durability = template.baseDurability;

            // Use material if provided, otherwise use legacy material field
            if (material != null)
            {
                durability *= (material.baseQuality / 100f);
                durability += material.stats.hardness * 0.5f; // Hardness contributes to durability
                durability += material.stats.toughness * 0.3f; // Toughness contributes to durability
            }
            else if (template.material != null)
            {
                durability *= template.material.qualityMultiplier;
                durability += template.material.healthBonus; // Reuse healthBonus for durability
            }

            return Mathf.Max(1f, durability);
        }

        /// <summary>
        /// Calculates equipment damage from material stats.
        /// </summary>
        public static float CalculateEquipmentDamage(EquipmentTemplate template, MaterialTemplate material = null)
        {
            float damage = template.stats.damage;

            if (material != null)
            {
                // Material hardness and quality affect damage
                damage += material.stats.hardness * 0.3f;
                damage *= (material.baseQuality / 100f);
            }

            return Mathf.Max(0f, damage);
        }

        /// <summary>
        /// Calculates equipment armor from material stats.
        /// </summary>
        public static float CalculateEquipmentArmor(EquipmentTemplate template, MaterialTemplate material = null)
        {
            float armor = template.stats.armor;

            if (material != null)
            {
                // Material hardness and toughness affect armor
                armor += material.stats.hardness * 0.4f;
                armor += material.stats.toughness * 0.2f;
                armor *= (material.baseQuality / 100f);
            }

            return Mathf.Max(0f, armor);
        }

        /// <summary>
        /// Calculates equipment weight (mass) from material density.
        /// </summary>
        public static float CalculateEquipmentWeight(EquipmentTemplate template, MaterialTemplate material = null)
        {
            // Use stats.weight if set, otherwise use base mass
            if (template.stats.weight > 0f)
            {
                return template.stats.weight;
            }

            float weight = template.mass;

            if (material != null)
            {
                // Material density affects weight
                weight *= material.stats.density;
            }

            return Mathf.Max(0.1f, weight);
        }

        /// <summary>
        /// Calculates tool quality from material.
        /// </summary>
        public static float CalculateToolQuality(ToolTemplate template)
        {
            float quality = template.baseQuality;

            if (template.material != null)
            {
                quality *= template.material.qualityMultiplier;
            }

            return Mathf.Clamp(quality, 0f, 100f);
        }

        /// <summary>
        /// Applies area bonuses to building desirability (fertility statues, wells, etc.).
        /// </summary>
        public static float ApplyAreaBonuses(float baseDesirability, List<BuildingTemplate> nearbyBuildings)
        {
            float bonus = 0f;

            foreach (var building in nearbyBuildings)
            {
                if (building.buildingType == BuildingTemplate.BuildingType.Utility)
                {
                    if (building.bonusType == "desirability")
                    {
                        float distanceFactor = 1f; // Would calculate distance at runtime
                        bonus += building.bonusValue * distanceFactor;
                    }
                }
            }

            return baseDesirability + bonus;
        }

        // ============================================================================
        // Unified Quality / Rarity / Tech Tier Calculations (per items.md spec)
        // ============================================================================

        /// <summary>
        /// Unified quality calculation per items.md spec.
        /// Burst-friendly, branch-light math.
        /// </summary>
        public static float CalculateItemQuality(
            float baseQuality,
            QualityWeights weights,
            List<MaterialTemplate> inputMaterials,
            float craftsmanSkill,      // 0-100
            float facilityQuality      // 0-100
        )
        {
            // Average purity / quality of inputs
            float avgPurity = 0f;
            float avgQuality = 0f;

            int len = inputMaterials != null ? inputMaterials.Count : 0;
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    avgPurity += inputMaterials[i].purity;
                    avgQuality += inputMaterials[i].quality;
                }
                float invLen = 1f / len;
                avgPurity *= invLen;
                avgQuality *= invLen;
            }

            float score =
                avgPurity * weights.MaterialPurityWeight +
                avgQuality * weights.MaterialQualityWeight +
                craftsmanSkill * weights.CraftsmanSkillWeight +
                facilityQuality * weights.FacilityQualityWeight;

            // Normalize back to 0-100, scale by definition base quality
            float normalized = Mathf.Clamp(score, 0f, 100f);
            float result = Mathf.Clamp(
                (normalized / 100f) * baseQuality,
                0f, 100f
            );

            return result;
        }

        /// <summary>
        /// Calculate rarity from quality, material rarity, and craftsman skill per items.md spec.
        /// </summary>
        public static Rarity CalculateRarity(
            float quality,
            Rarity maxMaterialRarity,    // max rarity of all input materials
            float craftsmanSkill         // 0-100
        )
        {
            // Base rarity from quality thresholds (per items.md spec)
            Rarity baseRarity;
            if (quality < 40f)
                baseRarity = Rarity.Common;
            else if (quality < 60f)
                baseRarity = Rarity.Uncommon;
            else if (quality < 80f)
                baseRarity = Rarity.Rare;
            else if (quality < 95f)
                baseRarity = Rarity.Epic;
            else
                baseRarity = Rarity.Legendary;

            // Chance to upgrade by skill (per items.md spec)
            // Note: In runtime, this would use deterministic RNG based on entity/world seed
            // For editor preview, we use a simple threshold check
            if (craftsmanSkill >= 80f)
            {
                // 10% upgrade chance (would be random in runtime)
                // For editor, we show potential upgrade
                Rarity upgraded = UpgradeRarity(baseRarity);
                if (upgraded > baseRarity && upgraded <= maxMaterialRarity)
                {
                    // In runtime, this would be: if (RandomValue0to1(seed) < 0.1f)
                    // For editor preview, we don't apply it automatically
                }
            }

            // Clamp to not exceed material rarity
            if (baseRarity > maxMaterialRarity)
                baseRarity = maxMaterialRarity;

            return baseRarity;
        }

        /// <summary>
        /// Upgrade rarity by one level (Common -> Uncommon, etc.).
        /// </summary>
        private static Rarity UpgradeRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common: return Rarity.Uncommon;
                case Rarity.Uncommon: return Rarity.Rare;
                case Rarity.Rare: return Rarity.Epic;
                case Rarity.Epic: return Rarity.Legendary;
                default: return rarity; // Legendary can't be upgraded
            }
        }

        /// <summary>
        /// Check if item can be crafted given village tech tier and requirements.
        /// Per items.md spec gate logic.
        /// </summary>
        public static bool CanCraft(
            byte villageTechTier,
            PrefabTemplate itemDef,
            List<MaterialTemplate> materials
        )
        {
            // Check item required tech tier
            if (villageTechTier < itemDef.requiredTechTier)
                return false;

            // Check all materials are unlocked
            if (materials != null)
            {
                foreach (var material in materials)
                {
                    if (villageTechTier < material.techTier)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get quality tier name from quality value (for UI display).
        /// Per items.md spec quality tiers.
        /// </summary>
        public static string GetQualityTierName(float quality)
        {
            if (quality <= 20f) return "Crude";
            if (quality <= 40f) return "Poor";
            if (quality <= 60f) return "Common";
            if (quality <= 80f) return "Fine";
            if (quality <= 95f) return "Excellent";
            return "Masterwork";
        }
    }
}

