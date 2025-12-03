using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Godgame.Editor.PrefabTool;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// CLI entry point for Prefab Maker batch jobs.
    /// Supports: --set=Minimal, --dryRun, --output=path
    /// </summary>
    public static class PrefabMakerCLI
    {
        /// <summary>
        /// Execution preset configurations.
        /// </summary>
        public enum ExecutionPreset
        {
            Minimal,    // Minimal validation, fast
            Standard,   // Standard validation
            Full        // Full validation + coverage reports
        }

        /// <summary>
        /// CLI entry point for batch execution.
        /// Called via: Unity -executeMethod PrefabMakerCLI.Run --set=Minimal --dryRun
        /// </summary>
        public static void Run()
        {
            var preset = ExecutionPreset.Standard;
            bool dryRun = false;
            string outputPath = "Logs/prefab_maker_report.json";
            
            // Parse command line arguments
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--set" && i + 1 < args.Length)
                {
                    if (Enum.TryParse<ExecutionPreset>(args[i + 1], true, out var parsedPreset))
                    {
                        preset = parsedPreset;
                    }
                }
                else if (args[i] == "--dryRun")
                {
                    dryRun = true;
                }
                else if (args[i] == "--output" && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                }
            }
            
            Debug.Log($"[PrefabMakerCLI] Starting batch job - Preset: {preset}, DryRun: {dryRun}, Output: {outputPath}");
            
            try
            {
                // Load templates from PrefabEditorWindow
                var window = ScriptableObject.CreateInstance<PrefabEditorWindow>();
                window.LoadDefaultTemplates();
                
                // Get all templates
                var buildingTemplates = window.GetBuildingTemplates();
                var individualTemplates = window.GetIndividualTemplates();
                var equipmentTemplates = window.GetEquipmentTemplates();
                var materialTemplates = window.GetMaterialTemplates();
                var toolTemplates = window.GetToolTemplates();
                var reagentTemplates = window.GetReagentTemplates();
                var miracleTemplates = window.GetMiracleTemplates();
                
                var materialRules = new List<MaterialRule>(); // TODO: Load from somewhere
                
                // Generate dry-run reports for all categories
                var allReports = new Dictionary<string, DryRunReport>();
                
                if (buildingTemplates.Count > 0)
                {
                    allReports["Buildings"] = DryRunGenerator.GenerateDryRunBuildings(
                        buildingTemplates, materialTemplates, materialRules);
                }
                
                if (equipmentTemplates.Count > 0)
                {
                    allReports["Equipment"] = DryRunGenerator.GenerateDryRunEquipment(
                        equipmentTemplates, materialTemplates, materialRules);
                }
                
                if (materialTemplates.Count > 0)
                {
                    allReports["Materials"] = DryRunGenerator.GenerateDryRunMaterials(
                        materialTemplates, materialRules);
                }
                
                if (individualTemplates.Count > 0)
                {
                    allReports["Individuals"] = DryRunGenerator.GenerateDryRunIndividuals(
                        individualTemplates);
                }
                
                if (toolTemplates.Count > 0)
                {
                    allReports["Tools"] = DryRunGenerator.GenerateDryRunTools(
                        toolTemplates, materialTemplates);
                }
                
                // Combine reports
                var combinedReport = CombineReports(allReports);
                
                // Export JSON
                string json = DryRunGenerator.ExportToJson(combinedReport);
                
                // Ensure output directory exists
                string outputDir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                File.WriteAllText(outputPath, json);
                Debug.Log($"[PrefabMakerCLI] Report written to: {outputPath}");
                
                // Generate coverage heatmap
                string heatmap = DryRunGenerator.GenerateCoverageHeatmap(combinedReport);
                string heatmapPath = outputPath.Replace(".json", "_coverage.txt");
                File.WriteAllText(heatmapPath, heatmap);
                Debug.Log($"[PrefabMakerCLI] Coverage heatmap written to: {heatmapPath}");
                
                // Idempotency check (if previous report exists)
                string previousReportPath = outputPath.Replace(".json", "_previous.json");
                if (File.Exists(previousReportPath))
                {
                    bool isIdempotent = IdempotencyChecker.ValidateIdempotency(outputPath, previousReportPath);
                    if (!isIdempotent && preset != ExecutionPreset.Minimal)
                    {
                        Debug.LogError("[PrefabMakerCLI] Idempotency check failed - outputs changed with identical inputs!");
                        EditorApplication.Exit(2); // Exit code 2 = idempotency failure
                    }
                }
                
                // Save current report as previous for next run
                File.Copy(outputPath, previousReportPath, true);
                
                // Exit with error code if validation failed
                if (combinedReport.errors > 0)
                {
                    Debug.LogError($"[PrefabMakerCLI] Validation failed with {combinedReport.errors} errors");
                    EditorApplication.Exit(1);
                }
                else
                {
                    Debug.Log("[PrefabMakerCLI] Batch job completed successfully");
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PrefabMakerCLI] Error: {ex.Message}\n{ex.StackTrace}");
                EditorApplication.Exit(1);
            }
        }
        
        /// <summary>
        /// Combine multiple category reports into one.
        /// </summary>
        private static DryRunReport CombineReports(Dictionary<string, DryRunReport> reports)
        {
            var combined = new DryRunReport
            {
                timestamp = DateTime.Now
            };
            
            // Combine input config hashes
            var inputHashes = new List<string>();
            
            foreach (var kvp in reports)
            {
                var report = kvp.Value;
                combined.totalPrefabs += report.totalPrefabs;
                combined.wouldCreate += report.wouldCreate;
                combined.wouldSkip += report.wouldSkip;
                combined.errors += report.errors;
                combined.warnings += report.warnings;
                combined.results.AddRange(report.results);
                
                if (!string.IsNullOrEmpty(report.inputConfigHash))
                {
                    inputHashes.Add($"{kvp.Key}:{report.inputConfigHash}");
                }
                
                foreach (var catCount in report.categoryCounts)
                {
                    if (combined.categoryCounts.ContainsKey(catCount.Key))
                        combined.categoryCounts[catCount.Key] += catCount.Value;
                    else
                        combined.categoryCounts[catCount.Key] = catCount.Value;
                }
                
                foreach (var coverage in report.coverageStats)
                {
                    combined.coverageStats[coverage.Key] = coverage.Value;
                }
            }
            
            // Calculate combined input config hash
            if (inputHashes.Count > 0)
            {
                combined.inputConfigHash = DryRunGenerator.CalculateHash(string.Join("|", inputHashes.OrderBy(h => h)));
            }
            
            // Recalculate combined report hash
            combined.reportHash = DryRunGenerator.CalculateReportHash(combined);
            
            return combined;
        }
    }
}

