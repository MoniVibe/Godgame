using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validates production chains for cycles, tech tier gates, and material references.
    /// Per items.md spec validation requirements.
    /// </summary>
    public static class ProductionChainValidator
    {
        /// <summary>
        /// Validation result for production chain checks.
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid;
            public List<string> Errors = new List<string>();
            public List<string> Warnings = new List<string>();

            public void AddError(string message)
            {
                IsValid = false;
                Errors.Add(message);
            }

            public void AddWarning(string message)
            {
                Warnings.Add(message);
            }
        }

        /// <summary>
        /// Validates a tool template's production chain.
        /// Checks: material existence, tech tier gates, production cycles, purity/quality gates.
        /// </summary>
        public static ValidationResult ValidateToolProductionChain(
            ToolTemplate tool,
            List<MaterialTemplate> materialCatalog,
            List<ToolTemplate> toolCatalog
        )
        {
            var result = new ValidationResult { IsValid = true };

            if (tool.productionInputs == null || tool.productionInputs.Count == 0)
            {
                result.AddWarning($"Tool '{tool.name}' has no production inputs (may be a raw material).");
                return result;
            }

            // Check all referenced materials exist
            foreach (var input in tool.productionInputs)
            {
                if (string.IsNullOrEmpty(input.materialName))
                {
                    result.AddError($"Tool '{tool.name}' has production input with empty material name.");
                    continue;
                }

                var material = materialCatalog?.FirstOrDefault(m => 
                    m.name == input.materialName || m.displayName == input.materialName);

                if (material == null)
                {
                    result.AddError($"Tool '{tool.name}' references non-existent material '{input.materialName}'.");
                    continue;
                }

                // Check tech tier gate
                if (material.techTier > tool.techTier)
                {
                    result.AddError(
                        $"Tool '{tool.name}' (tech tier {tool.techTier}) requires material '{input.materialName}' " +
                        $"(tech tier {material.techTier}). Material tech tier must be <= tool tech tier."
                    );
                }

                // Check purity/quality gates
                if (input.minPurity > material.purity)
                {
                    result.AddWarning(
                        $"Tool '{tool.name}' requires min purity {input.minPurity} for '{input.materialName}', " +
                        $"but material has purity {material.purity}."
                    );
                }

                if (input.minQuality > material.quality)
                {
                    result.AddWarning(
                        $"Tool '{tool.name}' requires min quality {input.minQuality} for '{input.materialName}', " +
                        $"but material has quality {material.quality}."
                    );
                }

                if (input.minTechTier > material.techTier)
                {
                    result.AddError(
                        $"Tool '{tool.name}' requires min tech tier {input.minTechTier} for '{input.materialName}', " +
                        $"but material has tech tier {material.techTier}."
                    );
                }
            }

            // Check for production cycles (tool A requires tool B, tool B requires tool A)
            if (toolCatalog != null)
            {
                CheckProductionCycles(tool, toolCatalog, materialCatalog, result);
            }

            return result;
        }

        /// <summary>
        /// Checks for production cycles using DFS.
        /// </summary>
        private static void CheckProductionCycles(
            ToolTemplate startTool,
            List<ToolTemplate> toolCatalog,
            List<MaterialTemplate> materialCatalog,
            ValidationResult result
        )
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var path = new List<string>();

            bool HasCycle(string toolName)
            {
                if (recursionStack.Contains(toolName))
                {
                    // Found cycle
                    path.Add(toolName);
                    result.AddError(
                        $"Production cycle detected: {string.Join(" -> ", path)} -> {toolName}"
                    );
                    return true;
                }

                if (visited.Contains(toolName))
                    return false;

                visited.Add(toolName);
                recursionStack.Add(toolName);
                path.Add(toolName);

                var tool = toolCatalog.FirstOrDefault(t => t.name == toolName);
                if (tool != null && tool.productionInputs != null)
                {
                    foreach (var input in tool.productionInputs)
                    {
                        // Check if input is a tool (not just a material)
                        var inputTool = toolCatalog.FirstOrDefault(t => 
                            t.name == input.materialName || t.displayName == input.materialName);
                        
                        if (inputTool != null)
                        {
                            if (HasCycle(inputTool.name))
                                return true;
                        }
                    }
                }

                recursionStack.Remove(toolName);
                path.RemoveAt(path.Count - 1);
                return false;
            }

            HasCycle(startTool.name);
        }

        /// <summary>
        /// Validates all tools in catalog for production chain issues.
        /// </summary>
        public static Dictionary<string, ValidationResult> ValidateAllProductionChains(
            List<ToolTemplate> toolCatalog,
            List<MaterialTemplate> materialCatalog
        )
        {
            var results = new Dictionary<string, ValidationResult>();

            foreach (var tool in toolCatalog)
            {
                results[tool.name] = ValidateToolProductionChain(tool, materialCatalog, toolCatalog);
            }

            return results;
        }
    }
}

