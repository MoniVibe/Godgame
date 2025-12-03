using System.Collections.Generic;
using Godgame.Abilities;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation system for status specifications.
    /// </summary>
    public static class StatusSpecValidator
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
        /// Validates a status spec catalog.
        /// </summary>
        public static ValidationResult ValidateCatalog(StatusSpecCatalog catalog)
        {
            var result = new ValidationResult { IsValid = true };

            if (catalog == null)
            {
                result.AddError("Status spec catalog is null");
                return result;
            }

            if (catalog.Statuses == null || catalog.Statuses.Length == 0)
            {
                result.AddWarning("Status spec catalog has no statuses");
                return result;
            }

            var statusIds = new HashSet<string>();

            for (int i = 0; i < catalog.Statuses.Length; i++)
            {
                var status = catalog.Statuses[i];
                ValidateStatus(status, i, statusIds, result);
            }

            return result;
        }

        private static void ValidateStatus(
            StatusSpecCatalog.StatusSpecDefinition status,
            int index,
            HashSet<string> statusIds,
            ValidationResult result)
        {
            // ID validation
            if (string.IsNullOrEmpty(status.Id))
            {
                result.AddError($"Status at index {index} has empty ID");
                return;
            }

            if (statusIds.Contains(status.Id))
            {
                result.AddError($"Duplicate status ID: {status.Id}");
            }
            else
            {
                statusIds.Add(status.Id);
            }

            // Max stacks validation
            if (status.MaxStacks == 0)
            {
                result.AddWarning($"Status '{status.Id}' has MaxStacks = 0 (should be at least 1)");
            }

            // Duration validation
            if (status.Duration < 0f)
            {
                result.AddError($"Status '{status.Id}' has negative duration: {status.Duration}");
            }

            // Period validation
            if (status.Period < 0f)
            {
                result.AddError($"Status '{status.Id}' has negative period: {status.Period}");
            }

            // Periodic validation: if Period > 0, Duration should be > Period
            if (status.Period > 0f)
            {
                if (status.Duration <= 0f)
                {
                    result.AddError($"Status '{status.Id}' has periodic tick (Period={status.Period}) but no duration");
                }
                else if (status.Period > status.Duration)
                {
                    result.AddError($"Status '{status.Id}' has period ({status.Period}) greater than duration ({status.Duration})");
                }
            }

            // Check for cycles (statuses that reference themselves or create loops)
            // This would require a status dependency graph - simplified for now
        }
    }
}

