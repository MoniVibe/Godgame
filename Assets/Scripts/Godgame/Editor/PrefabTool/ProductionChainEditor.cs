using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for editing production chains and quality derivation.
    /// </summary>
    public static class ProductionChainEditor
    {
        /// <summary>
        /// Draws UI for editing production inputs.
        /// </summary>
        public static void DrawProductionInputsEditor(List<ProductionInput> inputs, List<MaterialTemplate> materialCatalog)
        {
            EditorGUILayout.LabelField("Production Inputs", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (inputs == null)
            {
                EditorGUILayout.HelpBox("Production inputs list is null. Template may need migration.", MessageType.Warning);
                EditorGUI.indentLevel--;
                return;
            }

            // List existing inputs
            for (int i = 0; i < inputs.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                DrawProductionInputEditor(inputs[i], i, materialCatalog);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Input", GUILayout.Width(120)))
                {
                    inputs.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // Add new input button
            if (GUILayout.Button("+ Add Production Input"))
            {
                inputs.Add(new ProductionInput
                {
                    materialName = "",
                    quantity = 1f
                });
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing a single production input.
        /// </summary>
        private static void DrawProductionInputEditor(ProductionInput input, int index, List<MaterialTemplate> materialCatalog)
        {
            EditorGUILayout.LabelField($"Input #{index + 1}", EditorStyles.boldLabel);

            // Material selection
            if (materialCatalog != null && materialCatalog.Count > 0)
            {
                string[] materialNames = new string[materialCatalog.Count + 1];
                materialNames[0] = "<Select Material>";
                int selectedIndex = 0;
                
                for (int i = 0; i < materialCatalog.Count; i++)
                {
                    materialNames[i + 1] = materialCatalog[i].displayName ?? materialCatalog[i].name;
                    if (materialCatalog[i].name == input.materialName || 
                        materialCatalog[i].displayName == input.materialName)
                    {
                        selectedIndex = i + 1;
                    }
                }

                int newIndex = EditorGUILayout.Popup("Material", selectedIndex, materialNames);
                if (newIndex > 0)
                {
                    input.materialName = materialCatalog[newIndex - 1].name;
                }
                else
                {
                    input.materialName = EditorGUILayout.TextField("Material Name", input.materialName);
                }
            }
            else
            {
                input.materialName = EditorGUILayout.TextField("Material Name", input.materialName);
            }

            // Quantity
            input.quantity = EditorGUILayout.FloatField("Quantity", input.quantity);

            EditorGUILayout.Space();

            // Quality requirements (per items.md spec)
            EditorGUILayout.LabelField("Quality Requirements", EditorStyles.boldLabel);
            input.minPurity = EditorGUILayout.Slider("Min Purity", input.minPurity, 0f, 100f);
            input.minQuality = EditorGUILayout.Slider("Min Quality", input.minQuality, 0f, 100f);
            input.minTechTier = (byte)EditorGUILayout.IntSlider("Min Tech Tier", input.minTechTier, 0, 10);
            input.isOptional = EditorGUILayout.Toggle("Optional", input.isOptional);
        }

        /// <summary>
        /// Draws UI for editing quality derivation.
        /// </summary>
        public static void DrawQualityDerivationEditor(QualityDerivation derivation)
        {
            EditorGUILayout.LabelField("Quality Derivation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Quality is calculated as a weighted average of material purity, material quality, " +
                "craftsman skill, and forge quality. Weights should sum to approximately 1.0.",
                MessageType.Info
            );

            // Weight sliders
            derivation.materialPurityWeight = EditorGUILayout.Slider(
                "Material Purity Weight",
                derivation.materialPurityWeight,
                0f,
                1f
            );
            derivation.materialQualityWeight = EditorGUILayout.Slider(
                "Material Quality Weight",
                derivation.materialQualityWeight,
                0f,
                1f
            );
            derivation.craftsmanSkillWeight = EditorGUILayout.Slider(
                "Craftsman Skill Weight",
                derivation.craftsmanSkillWeight,
                0f,
                1f
            );
            derivation.forgeQualityWeight = EditorGUILayout.Slider(
                "Forge Quality Weight",
                derivation.forgeQualityWeight,
                0f,
                1f
            );

            float totalWeight = derivation.materialPurityWeight + derivation.materialQualityWeight +
                               derivation.craftsmanSkillWeight + derivation.forgeQualityWeight;
            
            if (Mathf.Abs(totalWeight - 1f) > 0.1f)
            {
                EditorGUILayout.HelpBox(
                    $"Total weight: {totalWeight:F2} (should be ~1.0)",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space();

            // Quality multipliers and bounds
            derivation.baseQualityMultiplier = EditorGUILayout.FloatField(
                "Base Quality Multiplier",
                derivation.baseQualityMultiplier
            );
            derivation.minQuality = EditorGUILayout.Slider("Min Quality", derivation.minQuality, 0f, 100f);
            derivation.maxQuality = EditorGUILayout.Slider("Max Quality", derivation.maxQuality, 0f, 100f);

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing material attributes.
        /// </summary>
        public static void DrawMaterialAttributesEditor(List<MaterialAttribute> attributes)
        {
            EditorGUILayout.LabelField("Material Attributes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Attributes that can be added by skilled craftsmen when using this material/tool in production.",
                MessageType.Info
            );

            if (attributes == null)
            {
                EditorGUILayout.HelpBox("Material attributes list is null. Template may need migration.", MessageType.Warning);
                EditorGUI.indentLevel--;
                return;
            }

            // List existing attributes
            for (int i = 0; i < attributes.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                DrawMaterialAttributeEditor(attributes[i], i);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Attribute", GUILayout.Width(120)))
                {
                    attributes.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // Add new attribute button
            if (GUILayout.Button("+ Add Material Attribute"))
            {
                attributes.Add(new MaterialAttribute
                {
                    name = "NewAttribute",
                    minCraftsmanSkill = 50f,
                    chanceToAdd = 1f
                });
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draws UI for editing a single material attribute.
        /// </summary>
        private static void DrawMaterialAttributeEditor(MaterialAttribute attribute, int index)
        {
            EditorGUILayout.LabelField($"Attribute #{index + 1}", EditorStyles.boldLabel);

            attribute.name = EditorGUILayout.TextField("Attribute Name", attribute.name);
            attribute.value = EditorGUILayout.FloatField("Value", attribute.value);
            attribute.isPercentage = EditorGUILayout.Toggle("Is Percentage", attribute.isPercentage);

            EditorGUILayout.Space();

            attribute.minCraftsmanSkill = EditorGUILayout.Slider(
                "Min Craftsman Skill",
                attribute.minCraftsmanSkill,
                0f,
                100f
            );
            attribute.chanceToAdd = EditorGUILayout.Slider(
                "Chance to Add",
                attribute.chanceToAdd,
                0f,
                1f
            );

            EditorGUILayout.HelpBox(
                $"This attribute will be added {attribute.chanceToAdd * 100:F0}% of the time " +
                $"when craftsman skill >= {attribute.minCraftsmanSkill:F0}",
                MessageType.Info
            );
        }

        /// <summary>
        /// Calculates quality based on derivation factors.
        /// </summary>
        public static float CalculateQuality(
            QualityDerivation derivation,
            float materialPurity,
            float materialQuality,
            float craftsmanSkill,
            float forgeQuality
        )
        {
            float quality = 0f;
            quality += materialPurity * derivation.materialPurityWeight;
            quality += materialQuality * derivation.materialQualityWeight;
            quality += craftsmanSkill * derivation.craftsmanSkillWeight;
            quality += forgeQuality * derivation.forgeQualityWeight;

            quality *= derivation.baseQualityMultiplier;
            quality = Mathf.Clamp(quality, derivation.minQuality, derivation.maxQuality);

            return quality;
        }
    }
}

