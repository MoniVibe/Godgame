#if false // Disabled until PlantSpecCatalogAuthoring and StandSpecCatalogAuthoring are implemented
using System.Collections.Generic;
using System.Linq;
using Godgame.Authoring;
using Godgame.Environment.Vegetation;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for editing vegetation (plant/stand) specifications in the Prefab Maker.
    /// Provides authoring, validation, dry-run, and binding generation.
    /// </summary>
    public static class VegetationTabUI
    {
        private static PlantSpecCatalogAuthoring selectedPlantCatalog;
        private static StandSpecCatalogAuthoring selectedStandCatalog;
        private static Vector2 plantScrollPosition;
        private static Vector2 standScrollPosition;
        private static string searchFilter = "";
        private static bool showValidation = true;
        private static bool showDryRun = false;
        private static Vector2 dryRunScrollPosition;
        private static int selectedSubTab = 0; // 0=Plants, 1=Stands

        public static void DrawVegetationTab(Rect position)
        {
            GUILayout.BeginArea(position);
            
            EditorGUILayout.LabelField("Vegetation Specifications", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Catalog selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Plant Catalog:", GUILayout.Width(120));
            selectedPlantCatalog = EditorGUILayout.ObjectField(
                selectedPlantCatalog,
                typeof(PlantSpecCatalogAuthoring),
                false) as PlantSpecCatalogAuthoring;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stand Catalog:", GUILayout.Width(120));
            selectedStandCatalog = EditorGUILayout.ObjectField(
                selectedStandCatalog,
                typeof(StandSpecCatalogAuthoring),
                false) as StandSpecCatalogAuthoring;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Sub-tabs
            selectedSubTab = GUILayout.Toolbar(selectedSubTab, new[] { "Plants", "Stands" });
            EditorGUILayout.Space();

            // Search filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            plantScrollPosition = EditorGUILayout.BeginScrollView(plantScrollPosition);

            if (selectedSubTab == 0)
            {
                DrawPlantsPanel();
            }
            else
            {
                DrawStandsPanel();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Validation section
            showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true);
            if (showValidation)
            {
                EditorGUI.indentLevel++;
                ValidateVegetation();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Dry-run section
            showDryRun = EditorGUILayout.Foldout(showDryRun, "Dry-Run Preview", true);
            if (showDryRun)
            {
                EditorGUI.indentLevel++;
                DrawDryRunPreview();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Binding management
            EditorGUILayout.LabelField("Binding Management", EditorStyles.boldLabel);
            if (GUILayout.Button("Generate Vegetation Bindings"))
            {
                GenerateVegetationBindings();
            }
            EditorGUILayout.HelpBox("Generates Minimal and Fancy binding sets mapping (PlantId, GrowthStage) to placeholder tokens.", MessageType.Info);

            GUILayout.EndArea();
        }

        private static void DrawPlantsPanel()
        {
            EditorGUILayout.LabelField("Plant Specifications", EditorStyles.boldLabel);

            if (selectedPlantCatalog == null)
            {
                EditorGUILayout.HelpBox("Select or create a PlantSpecCatalogAuthoring asset to edit plant specs.", MessageType.Info);
                if (GUILayout.Button("Create New Plant Catalog"))
                {
                    var asset = ScriptableObject.CreateInstance<PlantSpecCatalogAuthoring>();
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Create Plant Spec Catalog",
                        "PlantSpecCatalog",
                        "asset",
                        "Select save location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        selectedPlantCatalog = asset;
                    }
                }
                return;
            }

            var plants = selectedPlantCatalog.Plants;
            if (plants == null || plants.Length == 0)
            {
                EditorGUILayout.HelpBox("No plants defined. Add plants below.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Total Plants: {plants.Length}");
                EditorGUILayout.Space();

                for (int i = 0; i < plants.Length; i++)
                {
                    var plant = plants[i];
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !plant.id.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Plant {i + 1}: {plant.id}", EditorStyles.boldLabel);
                    if (!string.IsNullOrEmpty(plant.variantOf))
                    {
                        EditorGUILayout.LabelField($"Variant of: {plant.variantOf}", EditorStyles.miniLabel);
                    }
                    EditorGUILayout.LabelField($"Temp: {plant.tempMin}°C - {plant.tempMax}°C");
                    EditorGUILayout.LabelField($"Moisture: {plant.moistMin:F2} - {plant.moistMax:F2}");
                    EditorGUILayout.LabelField($"Biome Mask: {plant.biomeMask}");
                    EditorGUILayout.LabelField($"Yields: {plant.yields?.Length ?? 0} entries");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }
        }

        private static void DrawStandsPanel()
        {
            EditorGUILayout.LabelField("Stand Specifications", EditorStyles.boldLabel);

            if (selectedStandCatalog == null)
            {
                EditorGUILayout.HelpBox("Select or create a StandSpecCatalogAuthoring asset to edit stand specs.", MessageType.Info);
                if (GUILayout.Button("Create New Stand Catalog"))
                {
                    var asset = ScriptableObject.CreateInstance<StandSpecCatalogAuthoring>();
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Create Stand Spec Catalog",
                        "StandSpecCatalog",
                        "asset",
                        "Select save location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        selectedStandCatalog = asset;
                    }
                }
                return;
            }

            var stands = selectedStandCatalog.Stands;
            if (stands == null || stands.Length == 0)
            {
                EditorGUILayout.HelpBox("No stands defined. Add stands below.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"Total Stands: {stands.Length}");
                EditorGUILayout.Space();

                for (int i = 0; i < stands.Length; i++)
                {
                    var stand = stands[i];
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !stand.id.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Stand {i + 1}: {stand.id}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Plant: {stand.plantId}");
                    EditorGUILayout.LabelField($"Density: {stand.density} plants/m²");
                    EditorGUILayout.LabelField($"Clustering: {stand.clustering:F2}");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }
        }

        private static void ValidateVegetation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Validate plants
            if (selectedPlantCatalog != null && selectedPlantCatalog.Plants != null)
            {
                var plants = selectedPlantCatalog.Plants;
                for (int i = 0; i < plants.Length; i++)
                {
                    var plant = plants[i];

                    // Check preference contradictions
                    if (plant.tempMin > plant.tempMax)
                    {
                        errors.Add($"{plant.id}: tempMin ({plant.tempMin}) > tempMax ({plant.tempMax})");
                    }
                    if (plant.moistMin > plant.moistMax)
                    {
                        errors.Add($"{plant.id}: moistMin ({plant.moistMin}) > moistMax ({plant.moistMax})");
                    }
                    if (plant.elevMin > plant.elevMax)
                    {
                        errors.Add($"{plant.id}: elevMin ({plant.elevMin}) > elevMax ({plant.elevMax})");
                    }

                    // Check unreachable stages
                    if (plant.stageSecSeedling <= 0 && plant.stageSecSapling > 0)
                    {
                        warnings.Add($"{plant.id}: Seedling duration is 0, but Sapling duration > 0 (unreachable)");
                    }
                    if (plant.stageSecSapling <= 0 && plant.stageSecMature > 0)
                    {
                        warnings.Add($"{plant.id}: Sapling duration is 0, but Mature duration > 0 (unreachable)");
                    }

                    // Check missing biome mask
                    if (plant.biomeMask == 0)
                    {
                        warnings.Add($"{plant.id}: Biome mask is 0 (no biomes allowed)");
                    }

                    // Check yield sanity
                    if (plant.yields != null)
                    {
                        for (int j = 0; j < plant.yields.Length; j++)
                        {
                            var yield = plant.yields[j];
                            if (yield.minAmount > yield.maxAmount)
                            {
                                errors.Add($"{plant.id} yield {j}: minAmount ({yield.minAmount}) > maxAmount ({yield.maxAmount})");
                            }
                            if (yield.weight < 0 || yield.weight > 1)
                            {
                                warnings.Add($"{plant.id} yield {j}: weight ({yield.weight}) should be 0-1");
                            }
                        }
                    }

                    // Check VariantOf references
                    if (!string.IsNullOrEmpty(plant.variantOf))
                    {
                        bool found = false;
                        for (int j = 0; j < plants.Length; j++)
                        {
                            if (plants[j].id == plant.variantOf)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            warnings.Add($"{plant.id}: VariantOf '{plant.variantOf}' not found in catalog");
                        }
                    }
                }
            }

            // Validate stands
            if (selectedStandCatalog != null && selectedStandCatalog.Stands != null)
            {
                var stands = selectedStandCatalog.Stands;
                for (int i = 0; i < stands.Length; i++)
                {
                    var stand = stands[i];

                    if (string.IsNullOrEmpty(stand.plantId))
                    {
                        errors.Add($"Stand {stand.id}: Missing plantId");
                    }

                    if (stand.density <= 0)
                    {
                        warnings.Add($"Stand {stand.id}: Density is {stand.density} (should be > 0)");
                    }

                    if (stand.spawnWeightsPerBiome == null || stand.spawnWeightsPerBiome.Length == 0)
                    {
                        warnings.Add($"Stand {stand.id}: No spawn weights per biome defined");
                    }
                }
            }

            // Display errors
            if (errors.Count > 0)
            {
                EditorGUILayout.HelpBox($"Errors ({errors.Count}):\n" + string.Join("\n", errors.Take(10)), MessageType.Error);
                if (errors.Count > 10)
                {
                    EditorGUILayout.LabelField($"... and {errors.Count - 10} more errors", EditorStyles.miniLabel);
                }
            }

            // Display warnings
            if (warnings.Count > 0)
            {
                EditorGUILayout.HelpBox($"Warnings ({warnings.Count}):\n" + string.Join("\n", warnings.Take(10)), MessageType.Warning);
                if (warnings.Count > 10)
                {
                    EditorGUILayout.LabelField($"... and {warnings.Count - 10} more warnings", EditorStyles.miniLabel);
                }
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("Validation passed!", MessageType.Info);
            }
        }

        private static void DrawDryRunPreview()
        {
            EditorGUILayout.LabelField("Dry-Run Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Dry-run functionality will show what bindings would be generated and which plants fail validation.", MessageType.Info);
            
            if (GUILayout.Button("Generate Dry-Run Report"))
            {
                GenerateDryRunReport();
            }
        }

        private static void GenerateDryRunReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== VEGETATION DRY-RUN REPORT ===");
            report.AppendLine();

            if (selectedPlantCatalog != null && selectedPlantCatalog.Plants != null)
            {
                report.AppendLine($"Plants: {selectedPlantCatalog.Plants.Length}");
                foreach (var plant in selectedPlantCatalog.Plants)
                {
                    report.AppendLine($"  - {plant.id}: {GetGrowthStagesCount(plant)} stages");
                }
            }

            if (selectedStandCatalog != null && selectedStandCatalog.Stands != null)
            {
                report.AppendLine($"Stands: {selectedStandCatalog.Stands.Length}");
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Dry-Run Complete", report.ToString(), "OK");
        }

        private static int GetGrowthStagesCount(PlantSpecCatalogAuthoring.PlantSpecData plant)
        {
            int count = 0;
            if (plant.stageSecSeedling > 0) count++;
            if (plant.stageSecSapling > 0) count++;
            if (plant.stageSecMature > 0) count++;
            return count;
        }

        private static void GenerateVegetationBindings()
        {
            EditorUtility.DisplayDialog("Binding Generation", "Vegetation binding generation will be implemented to create Minimal/Fancy binding sets.", "OK");
        }
    }
}
#endif // Disabled until PlantSpecCatalogAuthoring and StandSpecCatalogAuthoring are implemented