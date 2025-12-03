using System.Collections.Generic;
using System.Linq;
using Godgame.Abilities;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation system for spell specifications.
    /// </summary>
    public static class SpellSpecValidator
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
        /// Validates a spell spec catalog.
        /// </summary>
        public static ValidationResult ValidateCatalog(SpellSpecCatalog catalog)
        {
            var result = new ValidationResult { IsValid = true };

            if (catalog == null)
            {
                result.AddError("Spell spec catalog is null");
                return result;
            }

            if (catalog.Spells == null || catalog.Spells.Length == 0)
            {
                result.AddWarning("Spell spec catalog has no spells");
                return result;
            }

            var spellIds = new HashSet<string>();

            for (int i = 0; i < catalog.Spells.Length; i++)
            {
                var spell = catalog.Spells[i];
                ValidateSpell(spell, i, spellIds, result);
            }

            return result;
        }

        private static void ValidateSpell(
            SpellSpecCatalog.SpellSpecDefinition spell,
            int index,
            HashSet<string> spellIds,
            ValidationResult result)
        {
            // ID validation
            if (string.IsNullOrEmpty(spell.Id))
            {
                result.AddError($"Spell at index {index} has empty ID");
                return;
            }

            if (spellIds.Contains(spell.Id))
            {
                result.AddError($"Duplicate spell ID: {spell.Id}");
            }
            else
            {
                spellIds.Add(spell.Id);
            }

            // Cooldown validation
            if (spell.Cooldown < 0f)
            {
                result.AddError($"Spell '{spell.Id}' has negative cooldown: {spell.Cooldown}");
            }

            // GCD group validation (0 = no GCD, otherwise must be valid)
            if (spell.GcdGroup > 10)
            {
                result.AddWarning($"Spell '{spell.Id}' has unusual GCD group: {spell.GcdGroup}");
            }

            // Cast time validation
            if (spell.CastTime < 0f)
            {
                result.AddError($"Spell '{spell.Id}' has negative cast time: {spell.CastTime}");
            }

            // Range validation
            if (spell.Range < 0f)
            {
                result.AddError($"Spell '{spell.Id}' has negative range: {spell.Range}");
            }

            // Radius validation (for area effects)
            if (spell.Shape == TargetShape.Area && spell.Radius <= 0f)
            {
                result.AddError($"Spell '{spell.Id}' is Area shape but has invalid radius: {spell.Radius}");
            }

            // Cost validation
            if (spell.Cost < 0f)
            {
                result.AddWarning($"Spell '{spell.Id}' has negative cost: {spell.Cost}");
            }

            // Effects validation
            if (spell.Effects == null || spell.Effects.Length == 0)
            {
                result.AddWarning($"Spell '{spell.Id}' has no effects");
            }
            else
            {
                for (int i = 0; i < spell.Effects.Length; i++)
                {
                    ValidateEffect(spell.Effects[i], spell.Id, i, result);
                }
            }
        }

        private static void ValidateEffect(
            SpellSpecCatalog.EffectOpDefinition effect,
            string spellId,
            int index,
            ValidationResult result)
        {
            // Magnitude validation
            if (effect.Magnitude < 0f && effect.Kind != EffectOpKind.Move)
            {
                result.AddWarning($"Spell '{spellId}' effect {index} has negative magnitude: {effect.Magnitude}");
            }

            // Duration validation
            if (effect.Duration < 0f)
            {
                result.AddError($"Spell '{spellId}' effect {index} has negative duration: {effect.Duration}");
            }

            // Period validation (must be > 0 if Duration > 0 for periodic effects)
            if (effect.Period < 0f)
            {
                result.AddError($"Spell '{spellId}' effect {index} has negative period: {effect.Period}");
            }

            if (effect.Duration > 0f && effect.Period > 0f && effect.Period > effect.Duration)
            {
                result.AddWarning($"Spell '{spellId}' effect {index} has period ({effect.Period}) greater than duration ({effect.Duration})");
            }
        }
    }
}

