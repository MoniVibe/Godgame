using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation guardrails: Rule-engine lints, recipe/tech gates, no-hybrid fence, ID/name policy.
    /// </summary>
    public static class ValidationGuardrails
    {
        /// <summary>
        /// Validate rule set for logical consistency.
        /// </summary>
        public static ValidationResult ValidateRuleSet(List<MaterialRule> rules)
        {
            var result = new ValidationResult();
            
            foreach (var rule in rules)
            {
                // Check for conflicting requirements
                if (rule.requiredTraits != MaterialTraits.None && rule.forbiddenTraits != MaterialTraits.None)
                {
                    var conflict = rule.requiredTraits & rule.forbiddenTraits;
                    if (conflict != MaterialTraits.None)
                    {
                        result.AddError($"Rule has conflicting traits: {conflict} is both required and forbidden");
                    }
                }
                
                // Check for impossible stat requirements
                if (rule.minStats != null)
                {
                    // Validate stat ranges
                    // TODO: Add specific stat validation
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate production chain for tech tier gates and cycles.
        /// </summary>
        public static ValidationResult ValidateProductionChain(
            ToolTemplate tool,
            List<MaterialTemplate> materialCatalog,
            List<ToolTemplate> toolCatalog)
        {
            var result = new ValidationResult();
            
            if (tool.productionInputs == null || tool.productionInputs.Count == 0)
            {
                return result; // No production chain to validate
            }
            
            // Check tech tier gates
            foreach (var input in tool.productionInputs)
            {
                var material = materialCatalog.FirstOrDefault(m => 
                    m.name == input.materialName || m.displayName == input.materialName);
                
                if (material != null)
                {
                    if (material.requiredTechTier > tool.techTier)
                    {
                        result.AddError($"Production input '{material.name}' requires tech tier {material.requiredTechTier}, but tool '{tool.name}' is tier {tool.techTier}");
                    }
                }
            }
            
            // Check for cycles (tool A requires tool B, tool B requires tool A)
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            
            if (HasCycle(tool, toolCatalog, materialCatalog, visited, recursionStack))
            {
                result.AddError($"Production chain for '{tool.name}' contains a cycle");
            }
            
            return result;
        }
        
        /// <summary>
        /// Check for production chain cycles using DFS.
        /// </summary>
        private static bool HasCycle(
            ToolTemplate tool,
            List<ToolTemplate> toolCatalog,
            List<MaterialTemplate> materialCatalog,
            HashSet<string> visited,
            HashSet<string> recursionStack)
        {
            if (recursionStack.Contains(tool.name))
            {
                return true; // Cycle detected
            }
            
            if (visited.Contains(tool.name))
            {
                return false; // Already processed
            }
            
            visited.Add(tool.name);
            recursionStack.Add(tool.name);
            
            if (tool.productionInputs != null)
            {
                foreach (var input in tool.productionInputs)
                {
                    // Check if input is itself a tool
                    var inputTool = toolCatalog.FirstOrDefault(t => 
                        t.name == input.materialName || t.displayName == input.materialName);
                    
                    if (inputTool != null)
                    {
                        if (HasCycle(inputTool, toolCatalog, materialCatalog, visited, recursionStack))
                        {
                            return true;
                        }
                    }
                }
            }
            
            recursionStack.Remove(tool.name);
            return false;
        }
        
        /// <summary>
        /// No-hybrid fence: Prevent mixing incompatible systems.
        /// Example: Don't allow both DOTS and GameObject-based components.
        /// </summary>
        public static ValidationResult ValidateNoHybrid(PrefabTemplate template)
        {
            var result = new ValidationResult();
            
            // Check for hybrid patterns
            // TODO: Add specific hybrid detection logic based on project requirements
            
            // Example: Check if template has both visualPresentation.prefabAsset and vfxPresentation.vfxAsset
            // that might conflict
            if (template.visualPresentation != null && template.vfxPresentation != null)
            {
                // This is actually fine - they can coexist
                // But we could add more specific checks here
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate ID/name policy: Ensure names and IDs follow conventions.
        /// </summary>
        public static ValidationResult ValidateIdNamePolicy(PrefabTemplate template)
        {
            var result = new ValidationResult();
            
            // Validate technical name format
            if (!TokenGenerator.IsValidTechnicalName(template.name))
            {
                result.AddError($"Template name '{template.name}' is not a valid technical name (must be alphanumeric + underscores)");
            }
            
            // Validate display name is set
            if (string.IsNullOrEmpty(template.displayName))
            {
                result.AddWarning($"Template '{template.name}' has no display name");
            }
            
            // Validate ID is positive
            if (template.id <= 0)
            {
                result.AddError($"Template '{template.name}' has invalid ID: {template.id} (must be > 0)");
            }
            
            // Validate catalog IDs for items
            if (template is EquipmentTemplate equipment)
            {
                if (equipment.itemDefId.Value == 0)
                {
                    result.AddWarning($"Equipment '{equipment.name}' has no ItemDefId");
                }
            }
            
            if (template is ToolTemplate tool)
            {
                if (tool.itemDefId.Value == 0)
                {
                    result.AddWarning($"Tool '{tool.name}' has no ItemDefId");
                }
                if (tool.recipeId.Value == 0 && tool.productionInputs != null && tool.productionInputs.Count > 0)
                {
                    result.AddWarning($"Tool '{tool.name}' has production inputs but no RecipeId");
                }
            }
            
            if (template is MaterialTemplate material)
            {
                if (material.materialId.Value == 0)
                {
                    result.AddWarning($"Material '{material.name}' has no MaterialId");
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate all guardrails for a template.
        /// </summary>
        public static ValidationResult ValidateAllGuardrails(
            PrefabTemplate template,
            List<MaterialTemplate> materialCatalog = null,
            List<ToolTemplate> toolCatalog = null)
        {
            var result = new ValidationResult();
            
            // ID/name policy
            result.Merge(ValidateIdNamePolicy(template));
            
            // No-hybrid fence
            result.Merge(ValidateNoHybrid(template));
            
            // Production chain validation (for tools)
            if (template is ToolTemplate tool && materialCatalog != null && toolCatalog != null)
            {
                result.Merge(ValidateProductionChain(tool, materialCatalog, toolCatalog));
            }
            
            return result;
        }
    }
}
