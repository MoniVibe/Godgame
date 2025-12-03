using UnityEditor;
using UnityEngine;
using Godgame.Editor.PrefabTool;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Detailed editor window for building templates.
    /// </summary>
    public class BuildingTemplateEditorWindow : EditorWindow
    {
        private BuildingTemplate template;
        private Vector2 scrollPosition;

        public void SetTemplate(BuildingTemplate template)
        {
            this.template = template;
        }

        private void OnGUI()
        {
            if (template == null)
            {
                EditorGUILayout.HelpBox("No template selected", MessageType.Warning);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Building Template Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic info
            template.name = EditorGUILayout.TextField("Name", template.name);
            template.displayName = EditorGUILayout.TextField("Display Name", template.displayName);
            template.id = EditorGUILayout.IntField("ID", template.id);
            template.description = EditorGUILayout.TextArea(template.description, GUILayout.Height(60));

            EditorGUILayout.Space();

            // Building type
            template.buildingType = (BuildingTemplate.BuildingType)EditorGUILayout.EnumPopup("Building Type", template.buildingType);

            EditorGUILayout.Space();

            // Base stats
            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
            template.baseHealth = EditorGUILayout.FloatField("Base Health", template.baseHealth);
            template.baseDesirability = EditorGUILayout.FloatField("Base Desirability", template.baseDesirability);

            EditorGUILayout.Space();

            // Materials
            EditorGUILayout.LabelField("Materials", EditorStyles.boldLabel);
            for (int i = 0; i < template.materials.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                var material = template.materials[i];
                material.materialName = EditorGUILayout.TextField("Material", material.materialName, GUILayout.Width(150));
                material.qualityMultiplier = EditorGUILayout.Slider("Quality", material.qualityMultiplier, 0.5f, 2f);
                material.healthBonus = EditorGUILayout.FloatField("Health Bonus", material.healthBonus, GUILayout.Width(100));
                material.desirabilityBonus = EditorGUILayout.FloatField("Desirability Bonus", material.desirabilityBonus, GUILayout.Width(120));
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.materials.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ Add Material"))
            {
                template.materials.Add(new MaterialQuality { materialName = "Wood", qualityMultiplier = 1f });
            }

            EditorGUILayout.Space();

            // Tool
            EditorGUILayout.LabelField("Tool", EditorStyles.boldLabel);
            if (template.tool != null)
            {
                EditorGUILayout.BeginHorizontal("box");
                template.tool.toolName = EditorGUILayout.TextField("Tool Name", template.tool.toolName, GUILayout.Width(150));
                template.tool.qualityMultiplier = EditorGUILayout.Slider("Quality", template.tool.qualityMultiplier, 0.5f, 2f);
                template.tool.durabilityBonus = EditorGUILayout.FloatField("Durability Bonus", template.tool.durabilityBonus);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    template.tool = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("+ Add Tool"))
                {
                    template.tool = new ToolQuality { toolName = "Basic Tool", qualityMultiplier = 1f };
                }
            }

            EditorGUILayout.Space();

            // Builder skill
            template.builderSkillLevel = EditorGUILayout.Slider("Builder Skill Level", template.builderSkillLevel, 0f, 100f);

            EditorGUILayout.Space();

            // Calculated stats (read-only)
            EditorGUILayout.LabelField("Calculated Stats", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            template.calculatedHealth = StatCalculation.CalculateBuildingHealth(template);
            template.calculatedDesirability = StatCalculation.CalculateBuildingDesirability(template);
            EditorGUILayout.FloatField("Calculated Health", template.calculatedHealth);
            EditorGUILayout.FloatField("Calculated Desirability", template.calculatedDesirability);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            // Area-of-effect bonuses
            BonusEditor.DrawBonusesEditor(template.bonuses);
            EditorGUILayout.Space();

            // Visual presentation
            VisualPresentationEditor.DrawVisualPresentationEditor(template.visualPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawVFXPresentationEditor(template.vfxPresentation);
            EditorGUILayout.Space();
            VisualPresentationEditor.DrawPresentationIdEditor(template);
            EditorGUILayout.Space();

            // Building-specific properties
            EditorGUILayout.LabelField("Building Properties", EditorStyles.boldLabel);
            switch (template.buildingType)
            {
                case BuildingTemplate.BuildingType.Residence:
                    template.maxResidents = EditorGUILayout.IntField("Max Residents", template.maxResidents);
                    template.comfortLevel = EditorGUILayout.FloatField("Comfort Level", template.comfortLevel);
                    template.restorationRate = EditorGUILayout.FloatField("Restoration Rate", template.restorationRate);
                    break;
                case BuildingTemplate.BuildingType.Workplace:
                    template.workCapacity = EditorGUILayout.FloatField("Work Capacity", template.workCapacity);
                    break;
                case BuildingTemplate.BuildingType.Utility:
                    template.areaBonusRange = EditorGUILayout.FloatField("Bonus Range", template.areaBonusRange);
                    template.bonusValue = EditorGUILayout.FloatField("Bonus Value", template.bonusValue);
                    template.bonusType = EditorGUILayout.TextField("Bonus Type", template.bonusType);
                    break;
                case BuildingTemplate.BuildingType.Storage:
                    template.storageCapacity = EditorGUILayout.FloatField("Storage Capacity", template.storageCapacity);
                    break;
                case BuildingTemplate.BuildingType.Worship:
                    template.manaGenerationRate = EditorGUILayout.FloatField("Mana Generation Rate", template.manaGenerationRate);
                    template.worshipperCapacity = EditorGUILayout.FloatField("Worshipper Capacity", template.worshipperCapacity);
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Action buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save & Generate Prefab"))
            {
                GameObject go = PrefabGenerator.GenerateBuildingPrefab(template);
                string path = PrefabGenerator.SavePrefab(go, "Buildings", template.name);
                Debug.Log($"Generated building prefab: {path}");
                AssetDatabase.Refresh();
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

