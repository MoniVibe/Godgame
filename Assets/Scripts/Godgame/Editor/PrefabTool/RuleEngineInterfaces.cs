using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Rule set interface for validating prefab specs using material traits/stats.
    /// </summary>
    public interface IRuleSet<TSpec> where TSpec : PrefabTemplate
    {
        /// <summary>
        /// Validate a spec against the rule set.
        /// </summary>
        ValidationResult Validate(TSpec spec, List<MaterialTemplate> materialCatalog);

        /// <summary>
        /// Find substitute materials for a spec.
        /// </summary>
        List<MaterialTemplate> FindSubstitutes(TSpec spec, List<MaterialTemplate> materialCatalog);
    }

    /// <summary>
    /// Prefab recipe interface - converts catalog/spec data → prefab definition (no assets).
    /// </summary>
    public interface IPrefabRecipe<TSpec> where TSpec : PrefabTemplate
    {
        /// <summary>
        /// Generate a prefab definition from a spec (does not create Unity assets).
        /// </summary>
        PrefabDefinition Generate(TSpec spec, List<MaterialTemplate> materialCatalog, List<MaterialRule> rules);
    }

    /// <summary>
    /// Prefab validator interface - produces rich diagnostics + substitutions.
    /// </summary>
    public interface IPrefabValidator<TSpec> where TSpec : PrefabTemplate
    {
        /// <summary>
        /// Validate a spec and return detailed diagnostics.
        /// </summary>
        ValidationDiagnostics Validate(TSpec spec, List<MaterialTemplate> materialCatalog, List<MaterialRule> rules);
    }

    /// <summary>
    /// Basic validation result used by rule sets.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public void AddError(string message)
        {
            IsValid = false;
            Errors.Add(message);
        }

        public void AddWarning(string message)
        {
            Warnings.Add(message);
        }

        public void Merge(ValidationResult other)
        {
            if (other == null) return;
            IsValid &= other.IsValid;
            Errors.AddRange(other.Errors);
            Warnings.AddRange(other.Warnings);
        }
    }

    /// <summary>
    /// Prefab definition (data-only, no Unity assets).
    /// </summary>
    public class PrefabDefinition
    {
        public string prefabId = "";
        public string prefabName = "";
        public Dictionary<string, object> properties = new Dictionary<string, object>();
        public List<SocketDefinition> sockets = new List<SocketDefinition>();
        public List<string> tags = new List<string>();
        public StyleTokens styleTokens = new StyleTokens();
    }

    /// <summary>
    /// Socket definition (attachment point).
    /// </summary>
    public class SocketDefinition
    {
        public string name = "";
        public string type = ""; // "entrance", "power", "logistics", "attach", "interaction"
        public UnityEngine.Vector3 position = UnityEngine.Vector3.zero;
        public UnityEngine.Quaternion rotation = UnityEngine.Quaternion.identity;
    }

    /// <summary>
    /// Validation diagnostics with detailed explanations.
    /// </summary>
    public class ValidationDiagnostics
    {
        public bool isValid = true;
        public List<ValidationIssue> issues = new List<ValidationIssue>();
        public List<MaterialTemplate> suggestedSubstitutes = new List<MaterialTemplate>();
    }

    /// <summary>
    /// Validation issue with explanation.
    /// </summary>
    public class ValidationIssue
    {
        public enum Severity
        {
            Error,
            Warning,
            Info
        }

        public Severity severity = Severity.Error;
        public string message = "";
        public string ruleClause = ""; // Which rule clause failed
        public string suggestion = ""; // Suggested fix
    }

    // ============================================================================
    // Concrete implementations
    // ============================================================================

    /// <summary>
    /// Building rule set implementation.
    /// </summary>
    public class BuildingRuleSet : IRuleSet<BuildingTemplate>
    {
        public ValidationResult Validate(BuildingTemplate spec, List<MaterialTemplate> materialCatalog)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate placement constraints
            if (spec.placement.maxSlope < 0 || spec.placement.maxSlope > 90)
            {
                result.AddError($"Building '{spec.name}': Invalid max slope {spec.placement.maxSlope} (must be 0-90)");
            }

            // Validate cost materials
            foreach (var cost in spec.cost)
            {
                // Find materials matching the cost requirement
                bool foundMatch = false;
                foreach (var material in materialCatalog)
                {
                    var costResult = MaterialRuleEngine.ValidateMaterialCost(cost, material);
                    if (costResult.passes)
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    result.AddError($"Building '{spec.name}': No materials found matching cost requirement (usage: {cost.requiredUsage}, min hardness: {cost.minStats.hardness})");
                }
            }

            // Validate forbidden combos
            foreach (var cost1 in spec.cost)
            {
                foreach (var cost2 in spec.cost)
                {
                    if (cost1 != cost2)
                    {
                        // Check if any material combinations would be forbidden
                        // This is simplified - would need to check actual material assignments
                    }
                }
            }

            return result;
        }

        public List<MaterialTemplate> FindSubstitutes(BuildingTemplate spec, List<MaterialTemplate> materialCatalog)
        {
            var substitutes = new List<MaterialTemplate>();

            foreach (var cost in spec.cost)
            {
                var rule = new MaterialRule
                {
                    requiredUsage = cost.requiredUsage,
                    requiredTraits = cost.requiredTraits,
                    forbiddenTraits = cost.forbiddenTraits,
                    minStats = cost.minStats
                };

                var found = MaterialRuleEngine.FindSubstitutes(rule);
                substitutes.AddRange(found);
            }

            return substitutes.Distinct().ToList();
        }
    }

    /// <summary>
    /// Equipment rule set implementation.
    /// </summary>
    public class EquipmentRuleSet : IRuleSet<EquipmentTemplate>
    {
        public ValidationResult Validate(EquipmentTemplate spec, List<MaterialTemplate> materialCatalog)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate material compatibility
            var rule = new MaterialRule
            {
                requiredUsage = spec.requiredUsage,
                requiredTraits = spec.requiredTraits,
                forbiddenTraits = spec.forbiddenTraits,
                minStats = spec.minStats
            };

            bool foundMatch = false;
            foreach (var material in materialCatalog)
            {
                var evalResult = MaterialRuleEngine.EvaluateRule(material, rule);
                if (evalResult.passes)
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                result.AddError($"Equipment '{spec.name}': No materials found matching requirements (usage: {spec.requiredUsage}, min hardness: {spec.minStats.hardness})");
            }

            // Check forbidden combos (e.g., Armor ∧ Flammable)
            // Trait sanity checks (none defined for Armor in current trait set)

            return result;
        }

        public List<MaterialTemplate> FindSubstitutes(EquipmentTemplate spec, List<MaterialTemplate> materialCatalog)
        {
            var rule = new MaterialRule
            {
                requiredUsage = spec.requiredUsage,
                requiredTraits = spec.requiredTraits,
                forbiddenTraits = spec.forbiddenTraits,
                minStats = spec.minStats
            };

            return MaterialRuleEngine.FindSubstitutes(rule);
        }
    }

    /// <summary>
    /// Building validator implementation.
    /// </summary>
    public class BuildingValidator : IPrefabValidator<BuildingTemplate>
    {
        private BuildingRuleSet ruleSet = new BuildingRuleSet();

        public ValidationDiagnostics Validate(BuildingTemplate spec, List<MaterialTemplate> materialCatalog, List<MaterialRule> rules)
        {
            var diagnostics = new ValidationDiagnostics { isValid = true };

            // Use rule set for basic validation
            var result = ruleSet.Validate(spec, materialCatalog);
            if (!result.IsValid)
            {
                diagnostics.isValid = false;
                foreach (var error in result.Errors)
                {
                    diagnostics.issues.Add(new ValidationIssue
                    {
                        severity = ValidationIssue.Severity.Error,
                        message = error,
                        suggestion = "Check material catalog or adjust cost requirements"
                    });
                }
            }

            // Add detailed explanations for each cost requirement
            foreach (var cost in spec.cost)
            {
                bool foundMatch = false;
                MaterialTemplate bestMatch = null;
                float bestScore = 0f;

                foreach (var material in materialCatalog)
                {
                    var costResult = MaterialRuleEngine.ValidateMaterialCost(cost, material);
                    if (costResult.passes)
                    {
                        foundMatch = true;
                        if (costResult.substitutionScore > bestScore)
                        {
                            bestMatch = material;
                            bestScore = costResult.substitutionScore;
                        }
                    }
                }

                if (!foundMatch)
                {
                    diagnostics.issues.Add(new ValidationIssue
                    {
                        severity = ValidationIssue.Severity.Error,
                        message = $"No materials match cost requirement: usage={cost.requiredUsage}, min hardness={cost.minStats.hardness}",
                        ruleClause = $"Cost requirement {cost.requiredUsage}",
                        suggestion = "Add materials with matching usage and stats, or adjust requirements"
                    });

                    // Find closest substitutes
                    var substitutes = ruleSet.FindSubstitutes(spec, materialCatalog);
                    diagnostics.suggestedSubstitutes.AddRange(substitutes.Take(3));
                }
            }

            return diagnostics;
        }
    }

    /// <summary>
    /// Equipment validator implementation.
    /// </summary>
    public class EquipmentValidator : IPrefabValidator<EquipmentTemplate>
    {
        private EquipmentRuleSet ruleSet = new EquipmentRuleSet();

        public ValidationDiagnostics Validate(EquipmentTemplate spec, List<MaterialTemplate> materialCatalog, List<MaterialRule> rules)
        {
            var diagnostics = new ValidationDiagnostics { isValid = true };

            var result = ruleSet.Validate(spec, materialCatalog);
            if (!result.IsValid)
            {
                diagnostics.isValid = false;
                foreach (var error in result.Errors)
                {
                    diagnostics.issues.Add(new ValidationIssue
                    {
                        severity = ValidationIssue.Severity.Error,
                        message = error
                    });
                }
            }

            // Find substitutes
            var substitutes = ruleSet.FindSubstitutes(spec, materialCatalog);
            diagnostics.suggestedSubstitutes.AddRange(substitutes.Take(3));

            return diagnostics;
        }
    }
}

