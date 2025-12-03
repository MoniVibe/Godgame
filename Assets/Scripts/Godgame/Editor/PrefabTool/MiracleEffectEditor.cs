using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Editor utility for authoring miracle effect blocks.
    /// </summary>
    public static class MiracleEffectEditor
    {
        private static Dictionary<MiracleTemplate, bool> expandedStates = new Dictionary<MiracleTemplate, bool>();

        /// <summary>
        /// Draws the miracle effect blocks editor.
        /// </summary>
        public static void DrawMiracleEffectsEditor(MiracleTemplate template)
        {
            if (template == null)
            {
                EditorGUILayout.HelpBox("No miracle template selected.", MessageType.Warning);
                return;
            }

            // Initialize collections if null
            if (template.effectBlocks == null)
            {
                template.effectBlocks = new List<MiracleEffectBlock>();
            }
            if (template.activationConfig == null)
            {
                template.activationConfig = new MiracleActivationConfig();
            }
            if (template.synergyTags == null)
            {
                template.synergyTags = new List<string>();
            }

            EditorGUILayout.LabelField("Miracle Effects", EditorStyles.boldLabel);

            // Activation Configuration
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Activation Configuration", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            template.activationConfig.supportsSustain = EditorGUILayout.Toggle("Supports Sustain (Channel)", template.activationConfig.supportsSustain);
            template.activationConfig.supportsThrow = EditorGUILayout.Toggle("Supports Throw (Projectile)", template.activationConfig.supportsThrow);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Base Costs", EditorStyles.boldLabel);
            template.activationConfig.basePrayerCost = EditorGUILayout.FloatField("Prayer Cost", template.activationConfig.basePrayerCost);
            template.activationConfig.baseManaCost = EditorGUILayout.FloatField("Mana Cost", template.activationConfig.baseManaCost);
            template.activationConfig.baseFocusCost = EditorGUILayout.FloatField("Focus Cost", template.activationConfig.baseFocusCost);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sustained Costs (per second)", EditorStyles.boldLabel);
            template.activationConfig.sustainPrayerCostPerSecond = EditorGUILayout.FloatField("Prayer/sec", template.activationConfig.sustainPrayerCostPerSecond);
            template.activationConfig.sustainManaCostPerSecond = EditorGUILayout.FloatField("Mana/sec", template.activationConfig.sustainManaCostPerSecond);
            template.activationConfig.sustainFocusCostPerSecond = EditorGUILayout.FloatField("Focus/sec", template.activationConfig.sustainFocusCostPerSecond);

            EditorGUILayout.Space();
            template.activationConfig.cooldown = EditorGUILayout.FloatField("Cooldown (seconds)", template.activationConfig.cooldown);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Faith Discount", EditorStyles.boldLabel);
            template.activationConfig.faithDensityReducesCost = EditorGUILayout.Toggle("Faith Reduces Cost", template.activationConfig.faithDensityReducesCost);
            if (template.activationConfig.faithDensityReducesCost)
            {
                template.activationConfig.maxFaithDiscount = EditorGUILayout.Slider("Max Discount %", template.activationConfig.maxFaithDiscount, 0f, 100f);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Alignment Modifiers", EditorStyles.boldLabel);
            template.activationConfig.evilAlignmentCostMultiplier = EditorGUILayout.FloatField("Evil Cost Multiplier", template.activationConfig.evilAlignmentCostMultiplier);
            template.activationConfig.benevolentAlignmentCostMultiplier = EditorGUILayout.FloatField("Benevolent Cost Multiplier", template.activationConfig.benevolentAlignmentCostMultiplier);

            EditorGUI.indentLevel--;

            // Effect Blocks
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Effect Blocks", EditorStyles.boldLabel);

            bool isExpanded = expandedStates.ContainsKey(template) && expandedStates[template];
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"Effect Blocks ({template.effectBlocks.Count})", true);
            expandedStates[template] = isExpanded;

            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                for (int i = 0; i < template.effectBlocks.Count; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawEffectBlockEditor(template.effectBlocks[i], i);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        template.effectBlocks.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("+ Add Effect Block"))
                {
                    template.effectBlocks.Add(new MiracleEffectBlock());
                }

                EditorGUI.indentLevel--;
            }

            // Synergy Tags
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Synergy Tags", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            for (int i = 0; i < template.synergyTags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                template.synergyTags[i] = EditorGUILayout.TextField($"Tag {i + 1}", template.synergyTags[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    template.synergyTags.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Synergy Tag"))
            {
                template.synergyTags.Add("");
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws editor for a single effect block.
        /// </summary>
        private static void DrawEffectBlockEditor(MiracleEffectBlock block, int index)
        {
            if (block == null)
                return;

            EditorGUILayout.LabelField($"Effect Block {index + 1}", EditorStyles.boldLabel);

            block.effectName = EditorGUILayout.TextField("Effect Name", block.effectName);
            block.effectType = (MiracleEffectType)EditorGUILayout.EnumPopup("Effect Type", block.effectType);
            block.phase = (MiracleEffectPhase)EditorGUILayout.EnumPopup("Phase", block.phase);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Target Filtering", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            block.targetType = (MiracleTargetType)EditorGUILayout.EnumPopup("Target Type", block.targetType);
            block.friendlyFire = EditorGUILayout.Toggle("Friendly Fire", block.friendlyFire);
            block.requiresLineOfSight = EditorGUILayout.Toggle("Requires Line of Sight", block.requiresLineOfSight);
            block.range = EditorGUILayout.FloatField("Range", block.range);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Area of Effect", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            block.areaShape = (AreaShape)EditorGUILayout.EnumPopup("Area Shape", block.areaShape);
            block.areaSize = EditorGUILayout.Vector2Field("Area Size", block.areaSize);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            block.duration = EditorGUILayout.FloatField("Duration", block.duration);
            block.tickInterval = EditorGUILayout.FloatField("Tick Interval", block.tickInterval);
            EditorGUI.indentLevel--;

            // Effect-specific parameters
            EditorGUILayout.Space();
            bool showDamage = block.effectType == MiracleEffectType.Damage;
            bool showHealing = block.effectType == MiracleEffectType.Healing;
            bool showStatus = block.effectType == MiracleEffectType.Status;
            bool showEnvironmental = block.effectType == MiracleEffectType.Environmental;
            bool showKnockback = block.effectType == MiracleEffectType.Knockback;
            bool showCleanse = block.effectType == MiracleEffectType.Cleanse;

            if (showDamage)
            {
                EditorGUILayout.LabelField("Damage Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.damageAmount = EditorGUILayout.FloatField("Damage Amount", block.damageAmount);
                block.damagePerSecond = EditorGUILayout.FloatField("Damage Per Second", block.damagePerSecond);
                block.armorPenetration = EditorGUILayout.Slider("Armor Penetration %", block.armorPenetration, 0f, 100f);
                EditorGUI.indentLevel--;
            }

            if (showHealing)
            {
                EditorGUILayout.LabelField("Healing Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.healAmount = EditorGUILayout.FloatField("Heal Amount", block.healAmount);
                block.healPerTick = EditorGUILayout.FloatField("Heal Per Tick", block.healPerTick);
                block.maxHeal = EditorGUILayout.FloatField("Max Heal (0 = no cap)", block.maxHeal);
                block.cleanseStrength = EditorGUILayout.FloatField("Cleanse Strength", block.cleanseStrength);
                block.overhealGivesBuff = EditorGUILayout.Toggle("Overheal Gives Buff", block.overhealGivesBuff);
                EditorGUI.indentLevel--;
            }

            if (showStatus)
            {
                EditorGUILayout.LabelField("Status Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.statusType = (MiracleStatusType)EditorGUILayout.EnumPopup("Status Type", block.statusType);
                block.statusDuration = EditorGUILayout.FloatField("Status Duration", block.statusDuration);
                block.statusSeverity = EditorGUILayout.FloatField("Status Severity", block.statusSeverity);
                block.statusStacks = EditorGUILayout.Toggle("Status Stacks", block.statusStacks);
                if (block.statusStacks)
                {
                    block.maxStacks = EditorGUILayout.IntField("Max Stacks", block.maxStacks);
                }
                EditorGUI.indentLevel--;
            }

            if (showEnvironmental)
            {
                EditorGUILayout.LabelField("Environmental Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.moistureAmount = EditorGUILayout.FloatField("Moisture Amount", block.moistureAmount);
                block.fireSuppression = EditorGUILayout.Slider("Fire Suppression %", block.fireSuppression, 0f, 100f);
                block.spreadChance = EditorGUILayout.Slider("Spread Chance %", block.spreadChance, 0f, 100f);
                block.speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", block.speedMultiplier, 0.25f, 2f);
                EditorGUI.indentLevel--;
            }

            if (showKnockback)
            {
                EditorGUILayout.LabelField("Knockback Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.knockbackForce = EditorGUILayout.FloatField("Knockback Force", block.knockbackForce);
                block.radialKnockback = EditorGUILayout.Toggle("Radial Knockback", block.radialKnockback);
                EditorGUI.indentLevel--;
            }

            if (showCleanse)
            {
                EditorGUILayout.LabelField("Cleanse Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.cleanseStrength = EditorGUILayout.FloatField("Cleanse Strength", block.cleanseStrength);
                EditorGUI.indentLevel--;
            }

            // Special parameters (always shown if relevant)
            if (block.effectType == MiracleEffectType.Healing || block.effectType == MiracleEffectType.Cleanse)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Special Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.comaReduction = EditorGUILayout.FloatField("Coma Reduction (days)", block.comaReduction);
                block.manaDebtReduction = EditorGUILayout.FloatField("Mana Debt Reduction", block.manaDebtReduction);
                EditorGUI.indentLevel--;
            }

            if (block.effectType == MiracleEffectType.Environmental && block.speedMultiplier != 1f)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Temporal Lashback Risk", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.lashbackChance = EditorGUILayout.Slider("Lashback Chance %", block.lashbackChance, 0f, 100f);
                block.lashbackManaDebtIncrease = EditorGUILayout.FloatField("Lashback Mana Debt Increase", block.lashbackManaDebtIncrease);
                EditorGUI.indentLevel--;
            }

            // Chain/Propagation
            if (block.effectType == MiracleEffectType.Damage || block.effectType == MiracleEffectType.Environmental)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Chain/Propagation", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                block.canChain = EditorGUILayout.Toggle("Can Chain", block.canChain);
                if (block.canChain)
                {
                    block.chainRange = EditorGUILayout.FloatField("Chain Range", block.chainRange);
                    block.chainDamageDecay = EditorGUILayout.Slider("Chain Damage Decay %", block.chainDamageDecay, 0f, 100f);
                    block.maxChains = EditorGUILayout.IntField("Max Chains", block.maxChains);
                }
                EditorGUI.indentLevel--;
            }

            // Synergy
            if (block.effectType == MiracleEffectType.Synergy)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Synergy Parameters", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                if (block.triggersOnMiracleTypes == null)
                    block.triggersOnMiracleTypes = new List<string>();

                for (int i = 0; i < block.triggersOnMiracleTypes.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    block.triggersOnMiracleTypes[i] = EditorGUILayout.TextField($"Triggers On {i + 1}", block.triggersOnMiracleTypes[i]);
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        block.triggersOnMiracleTypes.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+ Add Trigger Miracle Type"))
                {
                    block.triggersOnMiracleTypes.Add("");
                }

                block.synergyEffect = EditorGUILayout.TextField("Synergy Effect", block.synergyEffect);
                EditorGUI.indentLevel--;
            }

            // Scaling
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scaling", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            block.scalesWithFaithDensity = EditorGUILayout.Toggle("Scales With Faith Density", block.scalesWithFaithDensity);
            if (block.scalesWithFaithDensity)
            {
                block.faithDensityMultiplier = EditorGUILayout.FloatField("Faith Density Multiplier", block.faithDensityMultiplier);
            }
            block.scalesWithFocus = EditorGUILayout.Toggle("Scales With Focus", block.scalesWithFocus);
            if (block.scalesWithFocus)
            {
                block.focusMultiplier = EditorGUILayout.FloatField("Focus Multiplier", block.focusMultiplier);
            }
            EditorGUI.indentLevel--;
        }
    }
}

