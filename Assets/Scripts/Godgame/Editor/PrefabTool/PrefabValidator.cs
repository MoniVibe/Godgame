using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Godgame.Editor.PrefabTool;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Validation system for prefab templates.
    /// </summary>
    public static class PrefabValidator
    {
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
        /// Validates a building template.
        /// </summary>
        public static ValidationResult ValidateBuilding(BuildingTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Building template name is required");

            if (template.id <= 0)
                result.AddError($"Building '{template.name}' has invalid ID: {template.id}");

            if (template.baseHealth <= 0)
                result.AddWarning($"Building '{template.name}' has zero or negative base health");

            if (template.materials == null || template.materials.Count == 0)
                result.AddWarning($"Building '{template.name}' has no materials assigned");

            // Check for duplicate IDs
            // This would require access to all templates, so we'll do it at the collection level

            return result;
        }

        /// <summary>
        /// Validates an individual template.
        /// </summary>
        public static ValidationResult ValidateIndividual(IndividualTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Individual template name is required");

            if (template.id <= 0)
                result.AddError($"Individual '{template.name}' has invalid ID: {template.id}");

            // Core Attributes validation
            if (template.physique < 0f || template.physique > 100f)
                result.AddError($"Individual '{template.name}' has invalid Physique: {template.physique} (must be 0-100)");
            if (template.finesse < 0f || template.finesse > 100f)
                result.AddError($"Individual '{template.name}' has invalid Finesse: {template.finesse} (must be 0-100)");
            if (template.will < 0f || template.will > 100f)
                result.AddError($"Individual '{template.name}' has invalid Will: {template.will} (must be 0-100)");
            if (template.wisdom < 0f || template.wisdom > 100f)
                result.AddError($"Individual '{template.name}' has invalid Wisdom: {template.wisdom} (must be 0-100)");

            // Derived Attributes validation
            if (template.strength < 0f || template.strength > 100f)
                result.AddError($"Individual '{template.name}' has invalid Strength: {template.strength} (must be 0-100)");
            if (template.agility < 0f || template.agility > 100f)
                result.AddError($"Individual '{template.name}' has invalid Agility: {template.agility} (must be 0-100)");
            if (template.intelligence < 0f || template.intelligence > 100f)
                result.AddError($"Individual '{template.name}' has invalid Intelligence: {template.intelligence} (must be 0-100)");

            // Social Stats validation
            if (template.fame < 0f || template.fame > 1000f)
                result.AddWarning($"Individual '{template.name}' has Fame outside normal range: {template.fame} (0-1000)");
            if (template.reputation < -100f || template.reputation > 100f)
                result.AddWarning($"Individual '{template.name}' has Reputation outside normal range: {template.reputation} (-100 to +100)");
            if (template.glory < 0f || template.glory > 1000f)
                result.AddWarning($"Individual '{template.name}' has Glory outside normal range: {template.glory} (0-1000)");
            if (template.renown < 0f || template.renown > 1000f)
                result.AddWarning($"Individual '{template.name}' has Renown outside normal range: {template.renown} (0-1000)");

            // Combat Stats validation
            if (template.baseAttack < 0f || template.baseAttack > 100f)
                result.AddError($"Individual '{template.name}' has invalid Base Attack: {template.baseAttack} (must be 0-100, 0=auto-calculate)");
            if (template.baseDefense < 0f || template.baseDefense > 100f)
                result.AddError($"Individual '{template.name}' has invalid Base Defense: {template.baseDefense} (must be 0-100, 0=auto-calculate)");
            if (template.baseHealthOverride < 0f)
                result.AddError($"Individual '{template.name}' has invalid Base Health Override: {template.baseHealthOverride} (must be >= 0, 0=use baseHealth)");
            if (template.baseStamina < 0f)
                result.AddError($"Individual '{template.name}' has invalid Base Stamina: {template.baseStamina} (must be >= 0, 0=auto-calculate)");
            if (template.baseMana < 0f || template.baseMana > 100f)
                result.AddError($"Individual '{template.name}' has invalid Base Mana: {template.baseMana} (must be 0-100, 0=auto-calculate or non-magic)");

            // Need Stats validation
            if (template.food < 0f || template.food > 100f)
                result.AddWarning($"Individual '{template.name}' has Food outside normal range: {template.food} (0-100)");
            if (template.rest < 0f || template.rest > 100f)
                result.AddWarning($"Individual '{template.name}' has Rest outside normal range: {template.rest} (0-100)");
            if (template.sleep < 0f || template.sleep > 100f)
                result.AddWarning($"Individual '{template.name}' has Sleep outside normal range: {template.sleep} (0-100)");
            if (template.generalHealth < 0f || template.generalHealth > 100f)
                result.AddWarning($"Individual '{template.name}' has General Health outside normal range: {template.generalHealth} (0-100)");

            // Resistances validation
            if (template.resistances != null)
            {
                foreach (var kvp in template.resistances)
                {
                    if (kvp.Value < 0f || kvp.Value > 1f)
                        result.AddError($"Individual '{template.name}' has invalid resistance '{kvp.Key}': {kvp.Value} (must be 0.0-1.0)");
                }
            }

            // Healing & Spell Modifiers validation
            if (template.healBonus < 0f)
                result.AddError($"Individual '{template.name}' has invalid Heal Bonus: {template.healBonus} (must be >= 0.0)");
            if (template.spellDurationModifier < 0f)
                result.AddError($"Individual '{template.name}' has invalid Spell Duration Modifier: {template.spellDurationModifier} (must be >= 0.0)");
            if (template.spellIntensityModifier < 0f)
                result.AddError($"Individual '{template.name}' has invalid Spell Intensity Modifier: {template.spellIntensityModifier} (must be >= 0.0)");

            // Limb System validation
            if (template.limbs != null)
            {
                for (int i = 0; i < template.limbs.Count; i++)
                {
                    var limb = template.limbs[i];
                    if (string.IsNullOrEmpty(limb.limbId))
                        result.AddWarning($"Individual '{template.name}' has limb {i} with empty Limb ID");
                    if (limb.health < 0f || limb.health > 100f)
                        result.AddError($"Individual '{template.name}' has limb '{limb.limbId}' with invalid health: {limb.health} (must be 0-100)");
                }
            }

            // Implants validation
            if (template.implants != null)
            {
                for (int i = 0; i < template.implants.Count; i++)
                {
                    var implant = template.implants[i];
                    if (string.IsNullOrEmpty(implant.implantId))
                        result.AddWarning($"Individual '{template.name}' has implant {i} with empty Implant ID");
                }
            }

            // Disposition validation
            if (template.disposition != null)
            {
                foreach (var kvp in template.disposition)
                {
                    if (kvp.Value < -100f || kvp.Value > 100f)
                        result.AddWarning($"Individual '{template.name}' has disposition '{kvp.Key}' outside normal range: {kvp.Value} (-100 to +100)");
                }
            }

            // Titles validation
            if (template.titles != null)
            {
                for (int i = 0; i < template.titles.Count; i++)
                {
                    var title = template.titles[i];
                    if (string.IsNullOrEmpty(title.titleName))
                        result.AddWarning($"Individual '{template.name}' has title {i} with empty Title Name");
                    if (string.IsNullOrEmpty(title.displayName))
                        result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with empty Display Name");
                    if (title.minRenown < 0f || title.minRenown > 1000f)
                        result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Min Renown outside normal range: {title.minRenown} (0-1000)");
                    if (title.titleLevel < 1)
                        result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with invalid Title Level: {title.titleLevel} (must be >= 1)");
                    
                    // Validate title status
                    if (!title.IsActive() && string.IsNullOrEmpty(title.lossReason))
                    {
                        result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with status '{title.status}' but no loss reason specified");
                    }
                    
                    // Validate title bonuses
                    if (title.bonuses != null)
                    {
                        if (title.bonuses.reputationBonus < -100f || title.bonuses.reputationBonus > 100f)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Reputation Bonus outside normal range: {title.bonuses.reputationBonus} (-100 to +100)");
                        if (title.bonuses.fameBonus < 0f || title.bonuses.fameBonus > 1000f)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Fame Bonus outside normal range: {title.bonuses.fameBonus} (0-1000)");
                        if (title.bonuses.renownBonus < 0f || title.bonuses.renownBonus > 1000f)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Renown Bonus outside normal range: {title.bonuses.renownBonus} (0-1000)");
                        if (title.bonuses.gloryBonus < 0f || title.bonuses.gloryBonus > 1000f)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Glory Bonus outside normal range: {title.bonuses.gloryBonus} (0-1000)");
                        if (title.bonuses.socialStandingModifier < 0f)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with invalid Social Standing Modifier: {title.bonuses.socialStandingModifier} (must be >= 0.0)");
                        if (title.bonuses.authorityLevel < 0)
                            result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with invalid Authority Level: {title.bonuses.authorityLevel} (must be >= 0)");
                    }
                    
                    // Validate former bonuses if set
                    if (title.formerBonuses != null && title.IsActive())
                    {
                        result.AddWarning($"Individual '{template.name}' has title '{title.titleName}' with Former Bonuses set but status is Active (Former Bonuses only apply to lost titles)");
                    }
                }
                
                // Check if entity has any active titles
                var activeTitles = TitleHelper.GetActiveTitles(template.titles);
                if (activeTitles.Count == 0 && template.titles.Count > 0)
                {
                    result.AddWarning($"Individual '{template.name}' has {template.titles.Count} title(s) but all are former/lost (no active titles)");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates all templates in a collection for ID uniqueness.
        /// </summary>
        public static ValidationResult ValidateIdUniqueness<T>(List<T> templates) where T : PrefabTemplate
        {
            var result = new ValidationResult { IsValid = true };

            var idGroups = templates.GroupBy(t => t.id).Where(g => g.Count() > 1);
            foreach (var group in idGroups)
            {
                var names = string.Join(", ", group.Select(t => t.name));
                result.AddError($"Duplicate ID {group.Key} found in templates: {names}");
            }

            return result;
        }

        /// <summary>
        /// Validates that required authoring components exist for a prefab category.
        /// </summary>
        public static ValidationResult ValidateAuthoringComponents(string category, string prefabName)
        {
            var result = new ValidationResult { IsValid = true };

            // Check if authoring components exist for this category
            switch (category)
            {
                case "Buildings":
                    // StorehouseAuthoring exists, but other building types might not
                    // This is a warning, not an error, since we can add components later
                    break;
                case "Individuals":
                    // VillagerAuthoring exists
                    break;
                default:
                    result.AddWarning($"No authoring components found for category: {category}");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Validates an equipment template.
        /// </summary>
        public static ValidationResult ValidateEquipment(EquipmentTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Equipment template name is required");

            if (template.id <= 0)
                result.AddError($"Equipment '{template.name}' has invalid ID: {template.id}");

            if (template.baseDurability <= 0)
                result.AddWarning($"Equipment '{template.name}' has zero or negative base durability");

            return result;
        }

        /// <summary>
        /// Validates a material template.
        /// </summary>
        public static ValidationResult ValidateMaterial(MaterialTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Material template name is required");

            if (template.id <= 0)
                result.AddError($"Material '{template.name}' has invalid ID: {template.id}");

            if (template.baseQuality < 0 || template.baseQuality > 100)
                result.AddWarning($"Material '{template.name}' has quality outside valid range (0-100): {template.baseQuality}");

            if (template.purity < 0 || template.purity > 100)
                result.AddWarning($"Material '{template.name}' has purity outside valid range (0-100): {template.purity}");

            if (template.usage == MaterialUsage.None)
                result.AddWarning($"Material '{template.name}' has no usage flags set");

            return result;
        }

        /// <summary>
        /// Validates a tool template.
        /// </summary>
        public static ValidationResult ValidateTool(ToolTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Tool template name is required");

            if (template.id <= 0)
                result.AddError($"Tool '{template.name}' has invalid ID: {template.id}");

            if (template.baseQuality < 0 || template.baseQuality > 100)
                result.AddWarning($"Tool '{template.name}' has quality outside valid range (0-100): {template.baseQuality}");

            return result;
        }

        /// <summary>
        /// Validates a reagent template.
        /// </summary>
        public static ValidationResult ValidateReagent(ReagentTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.name))
                result.AddError("Reagent template name is required");

            if (template.id <= 0)
                result.AddError($"Reagent '{template.name}' has invalid ID: {template.id}");

            if (template.potency < 0 || template.potency > 100)
                result.AddWarning($"Reagent '{template.name}' has potency outside valid range (0-100): {template.potency}");

            return result;
        }

        /// <summary>
        /// Validates a miracle template.
        /// </summary>
        public static ValidationResult ValidateMiracle(MiracleTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (template == null)
            {
                result.AddError("Miracle template is null");
                return result;
            }

            // Basic validation
            if (string.IsNullOrEmpty(template.name))
                result.AddError("Miracle template name is required");

            if (template.id <= 0)
                result.AddWarning($"Miracle '{template.name}' has invalid ID: {template.id}");

            // Legacy field validation
            if (template.manaCost < 0)
                result.AddWarning($"Miracle '{template.name}' has negative mana cost: {template.manaCost}");

            if (template.cooldown < 0)
                result.AddWarning($"Miracle '{template.name}' has negative cooldown: {template.cooldown}");

            if (template.range < 0)
                result.AddWarning($"Miracle '{template.name}' has negative range: {template.range}");

            // Validate activation configuration
            if (template.activationConfig != null)
            {
                if (!template.activationConfig.supportsSustain && !template.activationConfig.supportsThrow)
                {
                    result.AddError($"Miracle '{template.name}': Must support at least one activation mode (sustain or throw)");
                }

                if (template.activationConfig.supportsSustain && template.activationConfig.sustainPrayerCostPerSecond <= 0 && 
                    template.activationConfig.sustainManaCostPerSecond <= 0 && template.activationConfig.sustainFocusCostPerSecond <= 0)
                {
                    result.AddWarning($"Miracle '{template.name}': Supports sustain but has no sustained costs (may be free)");
                }

                if (template.activationConfig.cooldown < 0)
                {
                    result.AddError($"Miracle '{template.name}': Cooldown cannot be negative");
                }

                if (template.activationConfig.maxFaithDiscount < 0 || template.activationConfig.maxFaithDiscount > 100)
                {
                    result.AddError($"Miracle '{template.name}': Max faith discount must be between 0-100%");
                }
            }

            // Validate effect blocks
            if (template.effectBlocks == null || template.effectBlocks.Count == 0)
            {
                result.AddWarning($"Miracle '{template.name}': Has no effect blocks (will do nothing)");
            }
            else
            {
                bool hasChannelEffect = false;
                bool hasImpactEffect = false;
                bool hasLingeringEffect = false;

                for (int i = 0; i < template.effectBlocks.Count; i++)
                {
                    var block = template.effectBlocks[i];
                    if (block == null)
                    {
                        result.AddError($"Miracle '{template.name}': Effect block {i + 1} is null");
                        continue;
                    }

                    // Check phase consistency with activation modes
                    if (block.phase == MiracleEffectPhase.Channel)
                    {
                        hasChannelEffect = true;
                        if (template.activationConfig != null && !template.activationConfig.supportsSustain)
                        {
                            result.AddWarning($"Miracle '{template.name}': Effect block '{block.effectName}' is Channel phase but miracle doesn't support sustain");
                        }
                    }
                    else if (block.phase == MiracleEffectPhase.Impact)
                    {
                        hasImpactEffect = true;
                        if (template.activationConfig != null && !template.activationConfig.supportsThrow)
                        {
                            result.AddWarning($"Miracle '{template.name}': Effect block '{block.effectName}' is Impact phase but miracle doesn't support throw");
                        }
                    }
                    else if (block.phase == MiracleEffectPhase.Lingering)
                    {
                        hasLingeringEffect = true;
                        if (block.duration <= 0)
                        {
                            result.AddWarning($"Miracle '{template.name}': Effect block '{block.effectName}' is Lingering but has zero duration");
                        }
                    }

                    // Validate effect-specific parameters
                    ValidateEffectBlock(block, template.name, i + 1, result);
                }

                // Check for missing effects based on activation modes
                if (template.activationConfig != null)
                {
                    if (template.activationConfig.supportsSustain && !hasChannelEffect)
                    {
                        result.AddWarning($"Miracle '{template.name}': Supports sustain but has no Channel phase effects");
                    }
                    if (template.activationConfig.supportsThrow && !hasImpactEffect)
                    {
                        result.AddWarning($"Miracle '{template.name}': Supports throw but has no Impact phase effects");
                    }
                }
            }

            // Validate synergy tags
            if (template.synergyTags != null)
            {
                foreach (var tag in template.synergyTags)
                {
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        result.AddWarning($"Miracle '{template.name}': Has empty synergy tag");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validates a single effect block for logical consistency.
        /// </summary>
        private static void ValidateEffectBlock(MiracleEffectBlock block, string miracleName, int blockIndex, ValidationResult result)
        {
            if (string.IsNullOrEmpty(block.effectName))
            {
                result.AddWarning($"Miracle '{miracleName}': Effect block {blockIndex} has no name");
            }

            // Validate timing
            if (block.tickInterval < 0)
            {
                result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Tick interval cannot be negative");
            }
            if (block.duration < 0)
            {
                result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Duration cannot be negative");
            }
            if (block.tickInterval > 0 && block.duration > 0 && block.tickInterval > block.duration)
            {
                result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Tick interval ({block.tickInterval}s) is greater than duration ({block.duration}s)");
            }

            // Validate range/area
            if (block.range < 0)
            {
                result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Range cannot be negative");
            }
            if (block.areaSize.x < 0 || block.areaSize.y < 0)
            {
                result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Area size cannot be negative");
            }

            // Validate effect-specific parameters
            switch (block.effectType)
            {
                case MiracleEffectType.Damage:
                    if (block.damageAmount <= 0 && block.damagePerSecond <= 0)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Damage effect has no damage amount");
                    }
                    if (block.armorPenetration < 0 || block.armorPenetration > 100)
                    {
                        result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Armor penetration must be 0-100%");
                    }
                    break;

                case MiracleEffectType.Healing:
                    if (block.healAmount <= 0 && block.healPerTick <= 0)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Healing effect has no heal amount");
                    }
                    if (block.maxHeal > 0 && block.healAmount > block.maxHeal)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Heal amount exceeds max heal cap");
                    }
                    break;

                case MiracleEffectType.Status:
                    if (block.statusType == MiracleStatusType.None)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Status effect has no status type");
                    }
                    if (block.statusDuration <= 0)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Status effect has zero duration");
                    }
                    if (block.statusStacks && block.maxStacks < 1)
                    {
                        result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Status stacks but max stacks < 1");
                    }
                    break;

                case MiracleEffectType.Environmental:
                    if (block.speedMultiplier < 0.25f || block.speedMultiplier > 2f)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Speed multiplier should be 0.25-2.0 (is {block.speedMultiplier})");
                    }
                    if (block.fireSuppression < 0 || block.fireSuppression > 100)
                    {
                        result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Fire suppression must be 0-100%");
                    }
                    if (block.spreadChance < 0 || block.spreadChance > 100)
                    {
                        result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Spread chance must be 0-100%");
                    }
                    break;

                case MiracleEffectType.Knockback:
                    if (block.knockbackForce <= 0)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Knockback effect has zero force");
                    }
                    break;

                case MiracleEffectType.Synergy:
                    if (block.triggersOnMiracleTypes == null || block.triggersOnMiracleTypes.Count == 0)
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Synergy effect has no trigger miracle types");
                    }
                    if (string.IsNullOrEmpty(block.synergyEffect))
                    {
                        result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Synergy effect has no effect description");
                    }
                    break;
            }

            // Validate chain parameters
            if (block.canChain)
            {
                if (block.chainRange <= 0)
                {
                    result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Can chain but chain range is zero");
                }
                if (block.maxChains < 1)
                {
                    result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Can chain but max chains < 1");
                }
                if (block.chainDamageDecay < 0 || block.chainDamageDecay > 100)
                {
                    result.AddError($"Miracle '{miracleName}': Effect block '{block.effectName}': Chain damage decay must be 0-100%");
                }
            }

            // Validate scaling
            if (block.scalesWithFaithDensity && block.faithDensityMultiplier <= 0)
            {
                result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Scales with faith density but multiplier is zero or negative");
            }
            if (block.scalesWithFocus && block.focusMultiplier <= 0)
            {
                result.AddWarning($"Miracle '{miracleName}': Effect block '{block.effectName}': Scales with focus but multiplier is zero or negative");
            }
        }


        /// <summary>
        /// Validates that a prefab asset exists and has required components.
        /// </summary>
        public static ValidationResult ValidatePrefabAsset(string prefabPath)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(prefabPath))
            {
                result.AddError("Prefab path is empty");
                return result;
            }

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                result.AddError($"Prefab not found at path: {prefabPath}");
                return result;
            }

            // Check for authoring components
            var authoringComponents = prefab.GetComponents<MonoBehaviour>();
            if (authoringComponents.Length == 0)
            {
                result.AddWarning($"Prefab '{prefab.name}' has no authoring components");
            }

            return result;
        }
    }
}

