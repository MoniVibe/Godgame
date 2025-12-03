using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for bulk prefab generation.
    /// </summary>
    public static class BulkGeneratorUI
    {
        private static BulkPrefabGenerator.BulkGenerationConfig config = new BulkPrefabGenerator.BulkGenerationConfig();
        private static Vector2 scrollPosition;
        private static bool showAdvancedOptions = false;
        private static Action templatesChanged;

        /// <summary>
        /// Draws the bulk generation UI panel.
        /// </summary>
        public static void DrawBulkGeneratorPanel(
            List<MaterialTemplate> materialCatalog,
            List<EquipmentTemplate> equipmentTemplates,
            List<ToolTemplate> toolTemplates,
            System.Action<EquipmentTemplate> onEquipmentCreated,
            System.Action<ToolTemplate> onToolCreated,
            System.Action onTemplatesChanged = null)
        {
            templatesChanged = onTemplatesChanged;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Bulk Prefab Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Generate multiple prefabs from patterns:\n" +
                "• One material → multiple items (e.g., 'Iron Sword', 'Iron Mace')\n" +
                "• One item type → all materials (e.g., 'Iron Sword', 'Gold Sword')",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Generation mode
            config.mode = (BulkPrefabGenerator.BulkGenerationConfig.GenerationMode)
                EditorGUILayout.EnumPopup("Generation Mode", config.mode);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (config.mode)
            {
                case BulkPrefabGenerator.BulkGenerationConfig.GenerationMode.MaterialToItems:
                    DrawMaterialToItemsMode(materialCatalog, equipmentTemplates, toolTemplates, onEquipmentCreated, onToolCreated);
                    break;
                case BulkPrefabGenerator.BulkGenerationConfig.GenerationMode.ItemToMaterials:
                    DrawItemToMaterialsMode(materialCatalog, equipmentTemplates, toolTemplates, onEquipmentCreated, onToolCreated);
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws UI for Material → Items mode.
        /// </summary>
        private static void DrawMaterialToItemsMode(
            List<MaterialTemplate> materialCatalog,
            List<EquipmentTemplate> equipmentTemplates,
            List<ToolTemplate> toolTemplates,
            System.Action<EquipmentTemplate> onEquipmentCreated,
            System.Action<ToolTemplate> onToolCreated)
        {
            EditorGUILayout.LabelField("Generate Multiple Items from One Material", EditorStyles.boldLabel);

            // Material selection
            if (materialCatalog == null || materialCatalog.Count == 0)
            {
                EditorGUILayout.HelpBox("No materials available. Create materials first.", MessageType.Warning);
                return;
            }

            string[] materialNames = materialCatalog.Select(m => m.displayName ?? m.name).ToArray();
            int selectedMaterialIndex = materialCatalog.IndexOf(config.sourceMaterial);
            if (selectedMaterialIndex < 0) selectedMaterialIndex = 0;

            int newIndex = EditorGUILayout.Popup("Source Material", selectedMaterialIndex, materialNames);
            if (newIndex >= 0 && newIndex < materialCatalog.Count)
            {
                config.sourceMaterial = materialCatalog[newIndex];
            }

            EditorGUILayout.Space();

            // Item type selection (Equipment or Tool)
            EditorGUILayout.LabelField("Generate As:", EditorStyles.boldLabel);
            bool generateEquipment = EditorGUILayout.Toggle("Equipment", true);
            bool generateTools = EditorGUILayout.Toggle("Tools", false);

            EditorGUILayout.Space();

            // Item type names
            EditorGUILayout.LabelField("Item Type Names", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Enter item type names (one per line). Examples: Sword, Mace, Axe, Screws, Statue, Shrine",
                MessageType.Info
            );

            string itemTypesText = string.Join("\n", config.itemTypeNames);
            itemTypesText = EditorGUILayout.TextArea(itemTypesText, GUILayout.Height(100));
            config.itemTypeNames = itemTypesText.Split('\n')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (config.itemTypeNames.Count == 0)
            {
                config.itemTypeNames.Add("Sword");
            }

            EditorGUILayout.Space();

            // Naming pattern
            config.namingPattern = EditorGUILayout.TextField("Naming Pattern", config.namingPattern);
            EditorGUILayout.HelpBox(
                "Use {Material} for material name, {ItemType} for item type.\n" +
                "Example: '{Material} {ItemType}' → 'Iron Sword'",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Default stats
            DrawDefaultStatsEditor();

            EditorGUILayout.Space();

            // Generate button
            EditorGUI.BeginDisabledGroup(config.sourceMaterial == null || config.itemTypeNames.Count == 0);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Equipment", GUILayout.Height(30)))
            {
                if (generateEquipment)
                {
                    GenerateEquipmentFromMaterial(onEquipmentCreated);
                }
            }
            if (GUILayout.Button("Generate Tools", GUILayout.Height(30)))
            {
                if (generateTools)
                {
                    GenerateToolsFromMaterial(onToolCreated);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Draws UI for Item → Materials mode.
        /// </summary>
        private static void DrawItemToMaterialsMode(
            List<MaterialTemplate> materialCatalog,
            List<EquipmentTemplate> equipmentTemplates,
            List<ToolTemplate> toolTemplates,
            System.Action<EquipmentTemplate> onEquipmentCreated,
            System.Action<ToolTemplate> onToolCreated)
        {
            EditorGUILayout.LabelField("Generate Item for All Materials", EditorStyles.boldLabel);

            // Base template selection
            EditorGUILayout.LabelField("Base Template", EditorStyles.boldLabel);
            bool useEquipment = EditorGUILayout.Toggle("Use Equipment Template", true);
            bool useTool = EditorGUILayout.Toggle("Use Tool Template", false);

            EditorGUILayout.Space();

            if (useEquipment)
            {
                if (equipmentTemplates == null || equipmentTemplates.Count == 0)
                {
                    EditorGUILayout.HelpBox("No equipment templates available. Create a base template first.", MessageType.Warning);
                    return;
                }

                string[] equipmentNames = equipmentTemplates.Select(e => e.displayName ?? e.name).ToArray();
                int selectedIndex = equipmentTemplates.IndexOf(config.baseItemTemplate);
                if (selectedIndex < 0) selectedIndex = 0;

                int newIndex = EditorGUILayout.Popup("Base Equipment Template", selectedIndex, equipmentNames);
                if (newIndex >= 0 && newIndex < equipmentTemplates.Count)
                {
                    config.baseItemTemplate = equipmentTemplates[newIndex];
                }
            }

            if (useTool)
            {
                if (toolTemplates == null || toolTemplates.Count == 0)
                {
                    EditorGUILayout.HelpBox("No tool templates available. Create a base template first.", MessageType.Warning);
                    return;
                }

                string[] toolNames = toolTemplates.Select(t => t.displayName ?? t.name).ToArray();
                int selectedIndex = toolTemplates.IndexOf(config.baseToolTemplate);
                if (selectedIndex < 0) selectedIndex = 0;

                int newIndex = EditorGUILayout.Popup("Base Tool Template", selectedIndex, toolNames);
                if (newIndex >= 0 && newIndex < toolTemplates.Count)
                {
                    config.baseToolTemplate = toolTemplates[newIndex];
                }
            }

            EditorGUILayout.Space();

            // Category filter
            EditorGUILayout.LabelField("Material Filter", EditorStyles.boldLabel);
            bool useCategoryFilter = EditorGUILayout.Toggle("Filter by Category", false);
            if (useCategoryFilter)
            {
                config.categoryFilter = (MaterialCategory)EditorGUILayout.EnumPopup("Category", 
                    config.categoryFilter ?? MaterialCategory.Raw);
            }
            else
            {
                config.categoryFilter = null;
            }

            EditorGUILayout.Space();

            // Naming pattern
            config.namingPattern = EditorGUILayout.TextField("Naming Pattern", config.namingPattern);
            EditorGUILayout.HelpBox(
                "Use {Material} for material name, {ItemType} for item type.\n" +
                "Example: '{Material} {ItemType}' → 'Iron Sword'",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // Default stats (override base template)
            DrawDefaultStatsEditor();

            EditorGUILayout.Space();

            // Generate button
            EditorGUI.BeginDisabledGroup(
                (useEquipment && config.baseItemTemplate == null) ||
                (useTool && config.baseToolTemplate == null) ||
                materialCatalog == null || materialCatalog.Count == 0
            );

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Equipment for All Materials", GUILayout.Height(30)))
            {
                if (useEquipment && config.baseItemTemplate != null)
                {
                    GenerateEquipmentForAllMaterials(materialCatalog, onEquipmentCreated);
                }
            }
            if (GUILayout.Button("Generate Tools for All Materials", GUILayout.Height(30)))
            {
                if (useTool && config.baseToolTemplate != null)
                {
                    GenerateToolsForAllMaterials(materialCatalog, onToolCreated);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Draws default stats editor.
        /// </summary>
        private static void DrawDefaultStatsEditor()
        {
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Default Stats & Settings", true);
            if (!showAdvancedOptions)
                return;

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Quality & Rarity", EditorStyles.boldLabel);
            config.defaultQuality = EditorGUILayout.Slider("Default Quality", config.defaultQuality, 0f, 100f);
            config.defaultRarity = (Rarity)EditorGUILayout.EnumPopup("Default Rarity", config.defaultRarity);
            config.defaultTechTier = (byte)EditorGUILayout.IntSlider("Default Tech Tier", config.defaultTechTier, 0, 10);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Equipment Defaults", EditorStyles.boldLabel);
            config.defaultEquipmentType = (EquipmentTemplate.EquipmentType)
                EditorGUILayout.EnumPopup("Equipment Type", config.defaultEquipmentType);
            config.defaultSlotKind = (SlotKind)EditorGUILayout.EnumPopup("Slot Kind", config.defaultSlotKind);
            config.defaultDamage = EditorGUILayout.FloatField("Default Damage", config.defaultDamage);
            config.defaultDurability = EditorGUILayout.FloatField("Default Durability", config.defaultDurability);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tool Defaults", EditorStyles.boldLabel);
            config.defaultConstructionSpeedBonus = EditorGUILayout.FloatField("Construction Speed Bonus", config.defaultConstructionSpeedBonus);
            config.defaultDurabilityBonus = EditorGUILayout.FloatField("Durability Bonus", config.defaultDurabilityBonus);
            config.defaultWorkEfficiency = EditorGUILayout.FloatField("Work Efficiency", config.defaultWorkEfficiency);

            EditorGUILayout.Space();

            config.inheritMaterialRequirements = EditorGUILayout.Toggle(
                "Inherit Material Requirements", 
                config.inheritMaterialRequirements
            );

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Generates equipment from selected material.
        /// </summary>
        private static void GenerateEquipmentFromMaterial(System.Action<EquipmentTemplate> onCreated)
        {
            if (config.sourceMaterial == null || config.itemTypeNames.Count == 0)
                return;

            var templates = BulkPrefabGenerator.GenerateEquipmentFromMaterial(
                config.sourceMaterial,
                config.itemTypeNames,
                config
            );

            int created = 0;
            foreach (var template in templates)
            {
                template.id = GetNextId();
                onCreated?.Invoke(template);
                created++;
            }

            EditorUtility.DisplayDialog("Bulk Generation Complete",
                $"Created {created} equipment template(s) from {config.sourceMaterial.displayName ?? config.sourceMaterial.name}.",
                "OK");

            Debug.Log($"BulkPrefabGenerator: Created {created} equipment templates");
            
            // Trigger save and refresh
            EditorUtility.SetDirty(EditorWindow.focusedWindow);
            AssetDatabase.SaveAssets();
            templatesChanged?.Invoke();
        }

        /// <summary>
        /// Generates tools from selected material.
        /// </summary>
        private static void GenerateToolsFromMaterial(System.Action<ToolTemplate> onCreated)
        {
            if (config.sourceMaterial == null || config.itemTypeNames.Count == 0)
                return;

            var templates = BulkPrefabGenerator.GenerateToolsFromMaterial(
                config.sourceMaterial,
                config.itemTypeNames,
                config
            );

            int created = 0;
            foreach (var template in templates)
            {
                template.id = GetNextId();
                onCreated?.Invoke(template);
                created++;
            }

            EditorUtility.DisplayDialog("Bulk Generation Complete",
                $"Created {created} tool template(s) from {config.sourceMaterial.displayName ?? config.sourceMaterial.name}.",
                "OK");

            Debug.Log($"BulkPrefabGenerator: Created {created} tool templates");
            
            // Trigger save and refresh
            EditorUtility.SetDirty(EditorWindow.focusedWindow);
            AssetDatabase.SaveAssets();
            templatesChanged?.Invoke();
        }

        /// <summary>
        /// Generates equipment for all materials.
        /// </summary>
        private static void GenerateEquipmentForAllMaterials(
            List<MaterialTemplate> materialCatalog,
            System.Action<EquipmentTemplate> onCreated)
        {
            if (config.baseItemTemplate == null || materialCatalog == null || materialCatalog.Count == 0)
                return;

            var templates = BulkPrefabGenerator.GenerateEquipmentForAllMaterials(
                materialCatalog,
                config.baseItemTemplate,
                config
            );

            int created = 0;
            foreach (var template in templates)
            {
                template.id = GetNextId();
                onCreated?.Invoke(template);
                created++;
            }

            EditorUtility.DisplayDialog("Bulk Generation Complete",
                $"Created {created} equipment template(s) for all materials.",
                "OK");

            Debug.Log($"BulkPrefabGenerator: Created {created} equipment templates for all materials");
            
            // Trigger save and refresh
            EditorUtility.SetDirty(EditorWindow.focusedWindow);
            AssetDatabase.SaveAssets();
            templatesChanged?.Invoke();
        }

        /// <summary>
        /// Generates tools for all materials.
        /// </summary>
        private static void GenerateToolsForAllMaterials(
            List<MaterialTemplate> materialCatalog,
            System.Action<ToolTemplate> onCreated)
        {
            if (config.baseToolTemplate == null || materialCatalog == null || materialCatalog.Count == 0)
                return;

            var templates = BulkPrefabGenerator.GenerateToolsForAllMaterials(
                materialCatalog,
                config.baseToolTemplate,
                config
            );

            int created = 0;
            foreach (var template in templates)
            {
                template.id = GetNextId();
                onCreated?.Invoke(template);
                created++;
            }

            EditorUtility.DisplayDialog("Bulk Generation Complete",
                $"Created {created} tool template(s) for all materials.",
                "OK");

            Debug.Log($"BulkPrefabGenerator: Created {created} tool templates for all materials");
            
            // Trigger save and refresh
            EditorUtility.SetDirty(EditorWindow.focusedWindow);
            AssetDatabase.SaveAssets();
            templatesChanged?.Invoke();
        }

        /// <summary>
        /// Gets next available ID (simple incrementing counter).
        /// </summary>
        private static int GetNextId()
        {
            // Use timestamp-based ID to avoid conflicts
            return (int)(System.DateTime.Now.Ticks % int.MaxValue);
        }
    }
}
