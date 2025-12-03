using System.Collections.Generic;
using System.Linq;
using Godgame.Abilities;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation system for skill trees (ensures acyclic prerequisites).
    /// </summary>
    public static class SkillTreeValidator
    {
        public class ValidationResult
        {
            public bool IsValid = true;
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
        /// Validates a skill spec catalog for acyclic prerequisites.
        /// </summary>
        public static ValidationResult ValidateCatalog(SkillSpecCatalog catalog)
        {
            var result = new ValidationResult { IsValid = true };

            if (catalog == null)
            {
                result.AddError("Skill spec catalog is null");
                return result;
            }

            if (catalog.Skills == null || catalog.Skills.Length == 0)
            {
                result.AddWarning("Skill spec catalog has no skills");
                return result;
            }

            // Build prerequisite graph
            var skillMap = new Dictionary<string, SkillSpecCatalog.SkillSpecDefinition>();
            var skillIds = new HashSet<string>();

            for (int i = 0; i < catalog.Skills.Length; i++)
            {
                var skill = catalog.Skills[i];
                if (string.IsNullOrEmpty(skill.Id))
                {
                    result.AddError($"Skill at index {i} has empty ID");
                    continue;
                }

                if (skillIds.Contains(skill.Id))
                {
                    result.AddError($"Duplicate skill ID: {skill.Id}");
                }
                else
                {
                    skillIds.Add(skill.Id);
                    skillMap[skill.Id] = skill;
                }
            }

            // Validate prerequisites exist and check for cycles
            foreach (var kvp in skillMap)
            {
                var skillId = kvp.Key;
                var skill = kvp.Value;

                if (skill.Requires != null)
                {
                    foreach (var prereqId in skill.Requires)
                    {
                        if (string.IsNullOrEmpty(prereqId))
                        {
                            result.AddWarning($"Skill '{skillId}' has empty prerequisite ID");
                            continue;
                        }

                        if (!skillMap.ContainsKey(prereqId))
                        {
                            result.AddError($"Skill '{skillId}' requires non-existent skill '{prereqId}'");
                        }
                    }
                }
            }

            // Check for cycles using DFS
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();

            foreach (var skillId in skillMap.Keys)
            {
                if (!visited.Contains(skillId))
                {
                    if (HasCycle(skillId, skillMap, visited, recursionStack, result))
                    {
                        result.AddError($"Skill tree contains cycle involving '{skillId}'");
                    }
                }
            }

            return result;
        }

        private static bool HasCycle(
            string skillId,
            Dictionary<string, SkillSpecCatalog.SkillSpecDefinition> skillMap,
            HashSet<string> visited,
            HashSet<string> recursionStack,
            ValidationResult result)
        {
            if (recursionStack.Contains(skillId))
            {
                return true; // Cycle detected
            }

            if (visited.Contains(skillId))
            {
                return false; // Already processed
            }

            visited.Add(skillId);
            recursionStack.Add(skillId);

            if (skillMap.TryGetValue(skillId, out var skill) && skill.Requires != null)
            {
                foreach (var prereqId in skill.Requires)
                {
                    if (!string.IsNullOrEmpty(prereqId) && skillMap.ContainsKey(prereqId))
                    {
                        if (HasCycle(prereqId, skillMap, visited, recursionStack, result))
                        {
                            return true;
                        }
                    }
                }
            }

            recursionStack.Remove(skillId);
            return false;
        }
    }
}

