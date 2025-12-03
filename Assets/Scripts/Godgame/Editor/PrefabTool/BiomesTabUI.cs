#if false // Disabled until BiomeDefinitionAuthoring is implemented
using System.Collections.Generic;
using System.Linq;
using Godgame.Authoring;
using Godgame.Environment;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// UI for editing biome definitions in the Prefab Maker.
    /// Provides authoring, validation, dry-run, and binding management.
    /// </summary>
    public static class BiomesTabUI
    {
        private static BiomeDefinitionAuthoring selectedBiomeAsset;
        private static Vector2 scrollPosition;
        private static string searchFilter = "";
        private static bool showValidation = true;
        private static bool showDryRun = false;
        private static Vector2 dryRunScrollPosition;

        public static void DrawBiomesTab(Rect position)
        {
            GUILayout.BeginArea(position);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Biome Definitions", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Search filter
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Asset selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Biome Definition Asset:", GUILayout.Width(150));
            var newAsset = EditorGUILayout.ObjectField(
                selectedBiomeAsset,
                typeof(BiomeDefinitionAuthoring),
                false) as BiomeDefinitionAuthoring;
            if (newAsset != selectedBiomeAsset)
            {
                selectedBiomeAsset = newAsset;
                EditorUtility.SetDirty(newAsset);
            }
            EditorGUILayout.EndHorizontal();

            if (selectedBiomeAsset == null)
            {
                EditorGUILayout.HelpBox("Select or create a BiomeDefinitionAuthoring asset to edit biome profiles.", MessageType.Info);
                if (GUILayout.Button("Create New Biome Definition"))
                {
                    var asset = ScriptableObject.CreateInstance<BiomeDefinitionAuthoring>();
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Create Biome Definition",
                        "BiomeDefinition",
                        "asset",
                        "Select save location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        selectedBiomeAsset = asset;
                    }
                }
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            EditorGUILayout.Space();

            // Profile list
            EditorGUILayout.LabelField("Biome Profiles", EditorStyles.boldLabel);
            var profiles = selectedBiomeAsset.Profiles;
            if (profiles == null || profiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No profiles defined. Add profiles below.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < profiles.Length; i++)
                {
                    var profile = profiles[i];
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !profile.id.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Profile {i + 1}: {profile.id}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Biome ID: {i + 1}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"Temp: {profile.tempMin}°C - {profile.tempMax}°C");
                    EditorGUILayout.LabelField($"Moisture: {profile.moistMin:F2} - {profile.moistMax:F2}");
                    EditorGUILayout.LabelField($"Wood Bias: {profile.resourceBiasWood}, Ore Bias: {profile.resourceBiasOre}");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }

            // Validation section
            showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true);
            if (showValidation)
            {
                EditorGUI.indentLevel++;
                ValidateBiomes(selectedBiomeAsset);
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
            EditorGUILayout.HelpBox("Binding blob generation will be implemented in a future update.", MessageType.Info);

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void ValidateBiomes(BiomeDefinitionAuthoring asset)
        {
            if (asset.Profiles == null || asset.Profiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No profiles to validate.", MessageType.Info);
                return;
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            // Check for overlapping ranges
            for (int i = 0; i < asset.Profiles.Length; i++)
            {
                var p1 = asset.Profiles[i];
                for (int j = i + 1; j < asset.Profiles.Length; j++)
                {
                    var p2 = asset.Profiles[j];

                    // Check temperature overlap
                    if (p1.tempMax >= p2.tempMin && p1.tempMin <= p2.tempMax)
                    {
                        warnings.Add($"Temperature overlap: {p1.id} ({p1.tempMin}-{p1.tempMax}°C) and {p2.id} ({p2.tempMin}-{p2.tempMax}°C)");
                    }

                    // Check moisture overlap
                    if (p1.moistMax >= p2.moistMin && p1.moistMin <= p2.moistMax)
                    {
                        warnings.Add($"Moisture overlap: {p1.id} ({p1.moistMin:F2}-{p1.moistMax:F2}) and {p2.id} ({p2.moistMin:F2}-{p2.moistMax:F2})");
                    }

                    // Check elevation overlap
                    if (p1.elevMax >= p2.elevMin && p1.elevMin <= p2.elevMax)
                    {
                        warnings.Add($"Elevation overlap: {p1.id} ({p1.elevMin}-{p1.elevMax}m) and {p2.id} ({p2.elevMin}-{p2.elevMax}m)");
                    }
                }

                // Validate individual profile
                if (p1.tempMin > p1.tempMax)
                {
                    errors.Add($"{p1.id}: tempMin ({p1.tempMin}) > tempMax ({p1.tempMax})");
                }
                if (p1.moistMin > p1.moistMax)
                {
                    errors.Add($"{p1.id}: moistMin ({p1.moistMin}) > moistMax ({p1.moistMax})");
                }
                if (p1.elevMin > p1.elevMax)
                {
                    errors.Add($"{p1.id}: elevMin ({p1.elevMin}) > elevMax ({p1.elevMax})");
                }
                if (string.IsNullOrEmpty(p1.id))
                {
                    errors.Add($"Profile {i + 1}: Missing ID");
                }
            }

            // Display errors
            if (errors.Count > 0)
            {
                EditorGUILayout.HelpBox($"Errors ({errors.Count}):\n" + string.Join("\n", errors), MessageType.Error);
            }

            // Display warnings
            if (warnings.Count > 0)
            {
                EditorGUILayout.HelpBox($"Warnings ({warnings.Count}):\n" + string.Join("\n", warnings.Take(5)), MessageType.Warning);
                if (warnings.Count > 5)
                {
                    EditorGUILayout.LabelField($"... and {warnings.Count - 5} more warnings", EditorStyles.miniLabel);
                }
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("Validation passed!", MessageType.Info);
            }
        }

        private static void DrawDryRunPreview()
        {
            EditorGUILayout.LabelField("Sample Climate Conditions", EditorStyles.boldLabel);

            // Sample climate inputs
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Temperature (°C):", GUILayout.Width(120));
            var tempC = EditorGUILayout.FloatField(17f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Moisture (0-1):", GUILayout.Width(120));
            var moisture = EditorGUILayout.Slider(0.6f, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Elevation (m):", GUILayout.Width(120));
            var elevation = EditorGUILayout.FloatField(0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (selectedBiomeAsset == null || selectedBiomeAsset.Profiles == null || selectedBiomeAsset.Profiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No profiles to preview.", MessageType.Info);
                return;
            }

            // Calculate best match (simplified version of resolver logic)
            int bestMatchIndex = -1;
            float bestScore = float.MinValue;

            for (int i = 0; i < selectedBiomeAsset.Profiles.Length; i++)
            {
                var profile = selectedBiomeAsset.Profiles[i];
                float score = 0f;

                bool inTempRange = tempC >= profile.tempMin && tempC <= profile.tempMax;
                bool inMoistureRange = moisture >= profile.moistMin && moisture <= profile.moistMax;
                bool inElevationRange = elevation >= profile.elevMin && elevation <= profile.elevMax;

                if (inTempRange) score += 0.4f;
                if (inMoistureRange) score += 0.4f;
                if (inElevationRange) score += 0.2f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatchIndex = i;
                }
            }

            if (bestMatchIndex >= 0)
            {
                var bestProfile = selectedBiomeAsset.Profiles[bestMatchIndex];
                EditorGUILayout.HelpBox(
                    $"Best Match: {bestProfile.id} (Score: {bestScore:F2})\n" +
                    $"Ground Style Token: {bestProfile.groundStyle}\n" +
                    $"Weather Profile Token: {bestProfile.weatherProfileToken}\n" +
                    $"Minimap Palette: {bestProfile.minimapPalette}",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No matching biome found for these conditions.", MessageType.Warning);
            }
        }
    }
}
#endif // Disabled until BiomeDefinitionAuthoring is implemented