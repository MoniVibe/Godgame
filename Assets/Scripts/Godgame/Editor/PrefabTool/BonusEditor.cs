using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for editing prefab bonuses and attributes.
    /// </summary>
    public static class BonusEditor
    {
        /// <summary>
        /// Draws UI for editing bonuses list.
        /// </summary>
        public static void DrawBonusesEditor(List<PrefabBonus> bonuses)
        {
            EditorGUILayout.LabelField("Area-of-Effect Bonuses", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (bonuses == null)
            {
                return; // Can't initialize here, caller must initialize
            }

            // List existing bonuses
            for (int i = 0; i < bonuses.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                DrawBonusEditor(bonuses[i], i);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Bonus", GUILayout.Width(120)))
                {
                    bonuses.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // Add new bonus button
            if (GUILayout.Button("+ Add Bonus"))
            {
                bonuses.Add(new PrefabBonus
                {
                    bonusType = BonusType.Custom,
                    displayName = "New Bonus",
                    radius = 10f
                });
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing a single bonus.
        /// </summary>
        private static void DrawBonusEditor(PrefabBonus bonus, int index)
        {
            EditorGUILayout.LabelField($"Bonus #{index + 1}", EditorStyles.boldLabel);

            // Basic info
            bonus.displayName = EditorGUILayout.TextField("Display Name", bonus.displayName);
            bonus.description = EditorGUILayout.TextArea(bonus.description, GUILayout.Height(40));

            EditorGUILayout.Space();

            // Bonus type
            bonus.bonusType = (BonusType)EditorGUILayout.EnumPopup("Bonus Type", bonus.bonusType);
            
            if (bonus.bonusType == BonusType.Custom)
            {
                bonus.customBonusName = EditorGUILayout.TextField("Custom Bonus Name", bonus.customBonusName);
            }

            EditorGUILayout.Space();

            // Bonus value
            EditorGUILayout.LabelField("Effect", EditorStyles.boldLabel);
            bonus.bonusValue = EditorGUILayout.FloatField("Bonus Value", bonus.bonusValue);
            bonus.isPercentage = EditorGUILayout.Toggle("Is Percentage", bonus.isPercentage);
            
            if (bonus.isPercentage)
            {
                EditorGUILayout.HelpBox($"This bonus will apply as {bonus.bonusValue}% modifier", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Area of effect
            EditorGUILayout.LabelField("Area of Effect", EditorStyles.boldLabel);
            bonus.radius = EditorGUILayout.FloatField("Radius (meters)", bonus.radius);
            bonus.useFalloff = EditorGUILayout.Toggle("Use Distance Falloff", bonus.useFalloff);
            
            if (bonus.useFalloff)
            {
                bonus.falloffRate = EditorGUILayout.Slider("Falloff Rate", bonus.falloffRate, 0.1f, 2f);
                EditorGUILayout.HelpBox(
                    $"Falloff Rate: 1.0 = linear, <1.0 = slower falloff, >1.0 = faster falloff",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space();

            // Target filters
            EditorGUILayout.LabelField("Target Filters", EditorStyles.boldLabel);
            bonus.affectsSelf = EditorGUILayout.Toggle("Affects Self", bonus.affectsSelf);
            bonus.affectsAllies = EditorGUILayout.Toggle("Affects Allies", bonus.affectsAllies);
            bonus.affectsEnemies = EditorGUILayout.Toggle("Affects Enemies", bonus.affectsEnemies);

            // Target tags
            EditorGUILayout.LabelField("Target Tags (optional)", EditorStyles.miniLabel);
            if (bonus.targetTags == null)
            {
                bonus.targetTags = new List<string>();
            }

            for (int i = 0; i < bonus.targetTags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                bonus.targetTags[i] = EditorGUILayout.TextField($"Tag {i + 1}", bonus.targetTags[i]);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    bonus.targetTags.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Target Tag", GUILayout.Width(120)))
            {
                bonus.targetTags.Add("");
            }

            EditorGUILayout.Space();

            // Duration
            EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel);
            bonus.isPermanent = EditorGUILayout.Toggle("Permanent", bonus.isPermanent);
            
            if (!bonus.isPermanent)
            {
                bonus.duration = EditorGUILayout.FloatField("Duration (seconds)", bonus.duration);
            }

            EditorGUILayout.Space();

            // Stacking
            EditorGUILayout.LabelField("Stacking", EditorStyles.boldLabel);
            bonus.stacks = EditorGUILayout.Toggle("Can Stack", bonus.stacks);
            
            if (bonus.stacks)
            {
                bonus.maxStacks = EditorGUILayout.FloatField("Max Stacks (0 = unlimited)", bonus.maxStacks);
            }
        }

        /// <summary>
        /// Gets a display string for a bonus type.
        /// </summary>
        public static string GetBonusTypeDisplayName(BonusType bonusType)
        {
            switch (bonusType)
            {
                case BonusType.Fertility:
                    return "Fertility (Pregnancy Bonus)";
                case BonusType.Happiness:
                    return "Happiness (Morale Bonus)";
                case BonusType.Productivity:
                    return "Productivity (Work Speed)";
                case BonusType.Desirability:
                    return "Desirability (Building Appeal)";
                case BonusType.HealthRegen:
                    return "Health Regeneration";
                case BonusType.ManaGeneration:
                    return "Mana Generation";
                case BonusType.MovementSpeed:
                    return "Movement Speed";
                case BonusType.ResourceYield:
                    return "Resource Yield";
                case BonusType.ConstructionSpeed:
                    return "Construction Speed";
                case BonusType.TradeValue:
                    return "Trade Value";
                case BonusType.Comfort:
                    return "Comfort";
                case BonusType.Defense:
                    return "Defense";
                case BonusType.Attack:
                    return "Attack";
                case BonusType.Experience:
                    return "Experience Gain";
                case BonusType.Moisture:
                    return "Moisture";
                case BonusType.Temperature:
                    return "Temperature";
                case BonusType.Light:
                    return "Light";
                case BonusType.Custom:
                    return "Custom";
                default:
                    return bonusType.ToString();
            }
        }
    }
}

