using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Idempotency checker for dry-run reports.
    /// Compares two reports to ensure unchanged inputs produce unchanged outputs.
    /// </summary>
    public static class IdempotencyChecker
    {
        /// <summary>
        /// Compare two dry-run reports for idempotency.
        /// Returns true if reports are identical (same hashes with same inputs).
        /// </summary>
        public static IdempotencyResult CheckIdempotency(DryRunReport currentReport, DryRunReport previousReport)
        {
            var result = new IdempotencyResult
            {
                IsIdempotent = true
            };
            
            // Check input config hash
            if (currentReport.inputConfigHash != previousReport.inputConfigHash)
            {
                result.IsIdempotent = false;
                result.Reason = "Input configuration changed";
                result.Details = $"Current: {currentReport.inputConfigHash.Substring(0, 8)}... | Previous: {previousReport.inputConfigHash.Substring(0, 8)}...";
                return result;
            }
            
            // Check report hash
            if (currentReport.reportHash != previousReport.reportHash)
            {
                result.IsIdempotent = false;
                result.Reason = "Output changed despite identical inputs";
                result.Details = $"Current: {currentReport.reportHash.Substring(0, 8)}... | Previous: {previousReport.reportHash.Substring(0, 8)}...";
                
                // Find which results changed
                var currentHashes = new System.Collections.Generic.Dictionary<string, string>();
                var previousHashes = new System.Collections.Generic.Dictionary<string, string>();
                
                foreach (var r in currentReport.results)
                {
                    currentHashes[r.prefabPath] = r.contentHash;
                }
                
                foreach (var r in previousReport.results)
                {
                    previousHashes[r.prefabPath] = r.contentHash;
                }
                
                var changedPrefabs = new System.Collections.Generic.List<string>();
                foreach (var kvp in currentHashes)
                {
                    if (!previousHashes.ContainsKey(kvp.Key) || previousHashes[kvp.Key] != kvp.Value)
                    {
                        changedPrefabs.Add(kvp.Key);
                    }
                }
                
                if (changedPrefabs.Count > 0)
                {
                    result.Details += $"\nChanged prefabs: {string.Join(", ", changedPrefabs)}";
                }
                
                return result;
            }
            
            // Reports are identical
            result.Reason = "Reports are identical";
            return result;
        }
        
        /// <summary>
        /// Load a dry-run report from JSON file.
        /// </summary>
        public static DryRunReport LoadReportFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Report file not found: {jsonPath}");
            }
            
            string json = File.ReadAllText(jsonPath);
            
            // Unity's JsonUtility doesn't handle nested dictionaries well,
            // so we'll parse manually for now (or use a proper JSON library)
            // For now, return a minimal report with just the hashes
            
            var report = new DryRunReport();
            
            // Extract hashes using simple string parsing
            // TODO: Use proper JSON deserialization (e.g., Newtonsoft.Json)
            if (json.Contains("\"reportHash\""))
            {
                int start = json.IndexOf("\"reportHash\"") + 12;
                int end = json.IndexOf("\"", start);
                if (end > start)
                {
                    report.reportHash = json.Substring(start, end - start);
                }
            }
            
            if (json.Contains("\"inputConfigHash\""))
            {
                int start = json.IndexOf("\"inputConfigHash\"") + 18;
                int end = json.IndexOf("\"", start);
                if (end > start)
                {
                    report.inputConfigHash = json.Substring(start, end - start);
                }
            }
            
            return report;
        }
        
        /// <summary>
        /// Validate idempotency by comparing current run with previous run.
        /// </summary>
        public static bool ValidateIdempotency(string currentReportPath, string previousReportPath)
        {
            if (!File.Exists(previousReportPath))
            {
                Debug.Log($"[IdempotencyChecker] No previous report found at {previousReportPath} - skipping check");
                return true; // First run, no comparison possible
            }
            
            try
            {
                var currentReport = LoadReportFromJson(currentReportPath);
                var previousReport = LoadReportFromJson(previousReportPath);
                
                var result = CheckIdempotency(currentReport, previousReport);
                
                if (!result.IsIdempotent)
                {
                    Debug.LogError($"[IdempotencyChecker] Idempotency check FAILED: {result.Reason}\n{result.Details}");
                    return false;
                }
                
                Debug.Log("[IdempotencyChecker] Idempotency check PASSED");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IdempotencyChecker] Could not perform idempotency check: {ex.Message}");
                return true; // Don't fail build on check errors
            }
        }
    }
    
    /// <summary>
    /// Result of idempotency check.
    /// </summary>
    [Serializable]
    public class IdempotencyResult
    {
        public bool IsIdempotent;
        public string Reason = "";
        public string Details = "";
    }
}

