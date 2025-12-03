using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Material rule (property-based constraint).
    /// </summary>
    [Serializable]
    public class MaterialRule
    {
        public string name = "";
        public string description = "";
        
        // Rule conditions
        public MaterialUsage requiredUsage = MaterialUsage.None;
        public MaterialTraits requiredTraits = MaterialTraits.None;
        public MaterialTraits forbiddenTraits = MaterialTraits.None;
        public MaterialStats minStats = new MaterialStats();
        public MaterialStats maxStats = new MaterialStats();
        
        // Rule effects
        public bool isForbidden = false; // If true, materials matching this rule are forbidden
        public float priority = 0f; // Higher priority rules evaluated first
    }

    /// <summary>
    /// Recipe input/output definition.
    /// </summary>
    [Serializable]
    public class RecipeInput
    {
        public string materialName = "";
        public MaterialUsage materialUsage = MaterialUsage.None;
        public float quantity = 1f;
        public float minQuality = 0f;
        public float minPurity = 0f;
    }

    /// <summary>
    /// Recipe output definition.
    /// </summary>
    [Serializable]
    public class RecipeOutput
    {
        public string materialName = "";
        public float quantity = 1f;
        public float qualityMultiplier = 1f; // Multiplies input quality
        public float purityMultiplier = 1f; // Multiplies input purity
    }

    /// <summary>
    /// Process recipe (input → output transformation).
    /// </summary>
    [Serializable]
    public class ProcessRecipe
    {
        public string name = "";
        public string description = "";
        public int id = 0;
        
        public List<RecipeInput> inputs = new List<RecipeInput>();
        public List<RecipeOutput> outputs = new List<RecipeOutput>();
        
        // Requirements
        public float timeRequired = 1f; // Seconds
        public float energyRequired = 0f; // Energy units
        public string requiredTool = ""; // Tool name/ID
        public float requiredSkillLevel = 0f; // 0-100
        
        // Validation
        public bool isValid = true;
        public List<string> validationErrors = new List<string>();
    }

    /// <summary>
    /// Rule evaluation result.
    /// </summary>
    public class RuleEvaluationResult
    {
        public bool passes = true;
        public List<string> failures = new List<string>();
        public List<string> warnings = new List<string>();
        public MaterialTemplate suggestedSubstitute = null;
        public float substitutionScore = 0f; // 0-1, higher = better match
    }

    /// <summary>
    /// Material rule engine - validates materials against rules and finds substitutes.
    /// </summary>
    public static class MaterialRuleEngine
    {
        private static List<MaterialRule> rules = new List<MaterialRule>();
        private static List<MaterialTemplate> materialCatalog = new List<MaterialTemplate>();

        /// <summary>
        /// Initialize the rule engine with a material catalog.
        /// </summary>
        public static void Initialize(List<MaterialTemplate> catalog, List<MaterialRule> ruleSet = null)
        {
            materialCatalog = catalog ?? new List<MaterialTemplate>();
            rules = ruleSet ?? new List<MaterialRule>();
        }

        /// <summary>
        /// Evaluate a material against a rule.
        /// </summary>
        public static RuleEvaluationResult EvaluateRule(MaterialTemplate material, MaterialRule rule)
        {
            var result = new RuleEvaluationResult { passes = true };

            // Check usage
            if (rule.requiredUsage != MaterialUsage.None)
            {
                if ((material.usage & rule.requiredUsage) == MaterialUsage.None)
                {
                    result.passes = false;
                    result.failures.Add($"Material '{material.name}' does not have required usage: {rule.requiredUsage}");
                }
            }

            // Check required traits
            if (rule.requiredTraits != MaterialTraits.None)
            {
                if ((material.traits & rule.requiredTraits) != rule.requiredTraits)
                {
                    result.passes = false;
                    result.failures.Add($"Material '{material.name}' missing required traits: {rule.requiredTraits}");
                }
            }

            // Check forbidden traits
            if (rule.forbiddenTraits != MaterialTraits.None)
            {
                if ((material.traits & rule.forbiddenTraits) != MaterialTraits.None)
                {
                    result.passes = false;
                    result.failures.Add($"Material '{material.name}' has forbidden traits: {rule.forbiddenTraits}");
                }
            }

            // Check min stats
            if (material.stats.hardness < rule.minStats.hardness)
            {
                result.passes = false;
                result.failures.Add($"Material '{material.name}' hardness {material.stats.hardness} < required {rule.minStats.hardness}");
            }
            if (material.stats.toughness < rule.minStats.toughness)
            {
                result.passes = false;
                result.failures.Add($"Material '{material.name}' toughness {material.stats.toughness} < required {rule.minStats.toughness}");
            }

            // Check max stats
            if (rule.maxStats.hardness > 0 && material.stats.hardness > rule.maxStats.hardness)
            {
                result.warnings.Add($"Material '{material.name}' hardness {material.stats.hardness} > max {rule.maxStats.hardness}");
            }

            // If rule is forbidden and material matches, fail
            if (rule.isForbidden && result.passes)
            {
                result.passes = false;
                result.failures.Add($"Material '{material.name}' is forbidden by rule: {rule.name}");
            }

            return result;
        }

        /// <summary>
        /// Find substitute materials for a given rule.
        /// </summary>
        public static List<MaterialTemplate> FindSubstitutes(MaterialRule rule, MaterialTemplate preferredMaterial = null)
        {
            var candidates = new List<(MaterialTemplate material, float score)>();

            foreach (var material in materialCatalog)
            {
                var result = EvaluateRule(material, rule);
                if (result.passes)
                {
                    float score = CalculateSubstitutionScore(material, rule, preferredMaterial);
                    candidates.Add((material, score));
                }
            }

            // Sort by score (highest first) and substitution rank (lower = better)
            return candidates
                .OrderByDescending(c => c.score)
                .ThenBy(c => c.material.substitutionRank)
                .Select(c => c.material)
                .ToList();
        }

        /// <summary>
        /// Calculate substitution score (0-1, higher = better match).
        /// </summary>
        private static float CalculateSubstitutionScore(MaterialTemplate material, MaterialRule rule, MaterialTemplate preferredMaterial)
        {
            float score = 1f;

            // Prefer materials with lower substitution rank
            score *= (100f - material.substitutionRank) / 100f;

            // Prefer materials closer to preferred material stats
            if (preferredMaterial != null)
            {
                float hardnessDiff = Mathf.Abs(material.stats.hardness - preferredMaterial.stats.hardness) / 100f;
                float toughnessDiff = Mathf.Abs(material.stats.toughness - preferredMaterial.stats.toughness) / 100f;
                score *= (1f - (hardnessDiff + toughnessDiff) / 2f);
            }

            // Prefer higher quality
            score *= material.baseQuality / 100f;

            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// Validate a material cost requirement and find substitutes if needed.
        /// </summary>
        public static RuleEvaluationResult ValidateMaterialCost(MaterialCost cost, MaterialTemplate material)
        {
            var rule = new MaterialRule
            {
                requiredUsage = cost.requiredUsage,
                requiredTraits = cost.requiredTraits,
                forbiddenTraits = cost.forbiddenTraits,
                minStats = cost.minStats
            };

            var result = EvaluateRule(material, rule);
            
            if (!result.passes && cost.allowSubstitution)
            {
                var substitutes = FindSubstitutes(rule, material);
                if (substitutes.Count > 0)
                {
                    result.suggestedSubstitute = substitutes[0];
                    result.substitutionScore = CalculateSubstitutionScore(substitutes[0], rule, material);
                }
            }

            return result;
        }

        /// <summary>
        /// Validate a recipe for cycles and consistency.
        /// </summary>
        public static List<string> ValidateRecipe(ProcessRecipe recipe, List<ProcessRecipe> allRecipes)
        {
            var errors = new List<string>();

            // Check inputs exist
            foreach (var input in recipe.inputs)
            {
                var material = materialCatalog.FirstOrDefault(m => m.name == input.materialName);
                if (material == null)
                {
                    errors.Add($"Recipe '{recipe.name}': Input material '{input.materialName}' not found in catalog");
                }
            }

            // Check outputs exist or can be created
            foreach (var output in recipe.outputs)
            {
                var material = materialCatalog.FirstOrDefault(m => m.name == output.materialName);
                if (material == null)
                {
                    errors.Add($"Recipe '{recipe.name}': Output material '{output.materialName}' not found in catalog");
                }
            }

            // Check for cycles (simplified - would need full dependency graph)
            // TODO: Implement cycle detection

            return errors;
        }

        /// <summary>
        /// Check for forbidden material combinations (e.g., Armor ∧ Flammable).
        /// </summary>
        public static bool CheckForbiddenCombo(MaterialTraits traits1, MaterialTraits traits2)
        {
            return false;
        }
    }
}

