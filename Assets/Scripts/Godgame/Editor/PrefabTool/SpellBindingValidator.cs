using System.Collections.Generic;
using Godgame.Abilities;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation system for spell presentation bindings.
    /// </summary>
    public static class SpellBindingValidator
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
        /// Validates a binding set against a spell catalog.
        /// </summary>
        public static ValidationResult ValidateBindingSet(
            SpellBindingSet bindingSet,
            SpellSpecCatalog spellCatalog,
            HashSet<string> showcasedSpellIds = null)
        {
            var result = new ValidationResult { IsValid = true };

            if (bindingSet == null)
            {
                result.AddError("Binding set is null");
                return result;
            }

            if (spellCatalog == null)
            {
                result.AddWarning("Spell catalog is null - cannot validate spell ID references");
            }

            var spellIdSet = new HashSet<string>();
            if (spellCatalog != null && spellCatalog.Spells != null)
            {
                foreach (var spell in spellCatalog.Spells)
                {
                    if (!string.IsNullOrEmpty(spell.Id))
                    {
                        spellIdSet.Add(spell.Id);
                    }
                }
            }

            if (bindingSet.Bindings == null || bindingSet.Bindings.Length == 0)
            {
                result.AddWarning($"Binding set '{bindingSet.SetName}' has no bindings");
                return result;
            }

            var bindingSpellIds = new HashSet<string>();

            for (int i = 0; i < bindingSet.Bindings.Length; i++)
            {
                var binding = bindingSet.Bindings[i];
                ValidateBinding(binding, i, bindingSpellIds, spellIdSet, showcasedSpellIds, result);
            }

            // Check showcased spells have bindings
            if (showcasedSpellIds != null)
            {
                foreach (var showcasedId in showcasedSpellIds)
                {
                    if (!bindingSpellIds.Contains(showcasedId))
                    {
                        result.AddWarning($"Showcased spell '{showcasedId}' has no binding in set '{bindingSet.SetName}'");
                    }
                }
            }

            return result;
        }

        private static void ValidateBinding(
            SpellPresentationBinding binding,
            int index,
            HashSet<string> bindingSpellIds,
            HashSet<string> spellIdSet,
            HashSet<string> showcasedSpellIds,
            ValidationResult result)
        {
            if (string.IsNullOrEmpty(binding.SpellId))
            {
                result.AddError($"Binding at index {index} has empty SpellId");
                return;
            }

            if (bindingSpellIds.Contains(binding.SpellId))
            {
                result.AddError($"Duplicate binding for spell ID: {binding.SpellId}");
            }
            else
            {
                bindingSpellIds.Add(binding.SpellId);
            }

            // Validate spell ID exists in catalog
            if (spellIdSet.Count > 0 && !spellIdSet.Contains(binding.SpellId))
            {
                result.AddWarning($"Binding references non-existent spell ID: {binding.SpellId}");
            }

            // Validate FX references (warnings only - they may be assigned later)
            if (binding.StartFX == null)
            {
                result.AddWarning($"Binding for '{binding.SpellId}' has no StartFX");
            }

            if (binding.ImpactFX == null)
            {
                result.AddWarning($"Binding for '{binding.SpellId}' has no ImpactFX");
            }
        }
    }
}

