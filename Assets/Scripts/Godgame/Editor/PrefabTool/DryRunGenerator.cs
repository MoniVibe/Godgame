using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Dry-run result (preview of what would be generated without creating assets).
    /// </summary>
    [Serializable]
    public class DryRunResult
    {
        public string prefabName = "";
        public string prefabPath = "";
        public bool wouldCreate = true; // false if prefab already exists and unchanged
        public bool isValid = true;
        public List<string> validationErrors = new List<string>();
        public List<string> validationWarnings = new List<string>();
        public List<MaterialSubstitution> substitutions = new List<MaterialSubstitution>();
        public Dictionary<string, object> properties = new Dictionary<string, object>();
        public List<SocketDefinition> sockets = new List<SocketDefinition>();
        public List<string> tags = new List<string>();
        
        /// <summary>
        /// Content hash for idempotency checking (SHA256 of serialized result data).
        /// </summary>
        public string contentHash = "";
        
        /// <summary>
        /// Input hash representing the template data that generated this result.
        /// </summary>
        public string inputHash = "";
    }

    /// <summary>
    /// Material substitution record.
    /// </summary>
    [Serializable]
    public class MaterialSubstitution
    {
        public string originalMaterial = "";
        public string substituteMaterial = "";
        public float substitutionScore = 0f;
        public string reason = "";
    }

    /// <summary>
    /// Dry-run report (collection of results).
    /// </summary>
    [Serializable]
    public class DryRunReport
    {
        public DateTime timestamp = DateTime.Now;
        public int totalPrefabs = 0;
        public int wouldCreate = 0;
        public int wouldUpdate = 0;
        public int wouldSkip = 0;
        public int errors = 0;
        public int warnings = 0;
        public List<DryRunResult> results = new List<DryRunResult>();
        public Dictionary<string, int> categoryCounts = new Dictionary<string, int>();
        
        /// <summary>
        /// Overall report hash (SHA256 of all result hashes + inputs).
        /// </summary>
        public string reportHash = "";
        
        /// <summary>
        /// Input configuration hash (represents template set + rules used).
        /// </summary>
        public string inputConfigHash = "";
        
        /// <summary>
        /// Coverage statistics per category.
        /// </summary>
        public Dictionary<string, CoverageStats> coverageStats = new Dictionary<string, CoverageStats>();
    }
    
    /// <summary>
    /// Coverage statistics for a category.
    /// </summary>
    [Serializable]
    public class CoverageStats
    {
        public int totalCatalogIds = 0;
        public int generatedPrefabs = 0;
        public int validPrefabs = 0;
        public int invalidPrefabs = 0;
        public float coveragePercent = 0f;
    }

    /// <summary>
    /// Dry-run generator - previews prefab generation without creating assets.
    /// </summary>
    public static class DryRunGenerator
    {
        /// <summary>
        /// Calculate SHA256 hash of a string.
        /// </summary>
        public static string CalculateHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Calculate content hash for a dry-run result.
        /// </summary>
        private static string CalculateResultHash(DryRunResult result)
        {
            var sb = new StringBuilder();
            sb.Append(result.prefabName);
            sb.Append(result.prefabPath);
            sb.Append(result.wouldCreate);
            sb.Append(result.isValid);
            sb.Append(string.Join("|", result.validationErrors.OrderBy(e => e)));
            sb.Append(string.Join("|", result.validationWarnings.OrderBy(w => w)));
            sb.Append(string.Join("|", result.tags.OrderBy(t => t)));
            
            // Serialize properties in sorted order for deterministic hashing
            var sortedProps = result.properties.OrderBy(kvp => kvp.Key);
            foreach (var kvp in sortedProps)
            {
                sb.Append($"{kvp.Key}:{kvp.Value}");
            }
            
            // Serialize sockets
            var sortedSockets = result.sockets.OrderBy(s => s.name);
            foreach (var socket in sortedSockets)
            {
                sb.Append($"socket:{socket.name}:{socket.type}:{socket.position}");
            }
            
            return CalculateHash(sb.ToString());
        }
        
        /// <summary>
        /// Calculate input hash for templates/rules.
        /// </summary>
        private static string CalculateInputHash<T>(List<T> templates) where T : PrefabTemplate
        {
            var sb = new StringBuilder();
            foreach (var template in templates.OrderBy(t => t.id))
            {
                sb.Append($"{template.id}:{template.name}:{template.GetHashCode()}");
            }
            return CalculateHash(sb.ToString());
        }
        
        /// <summary>
        /// Calculate overall report hash.
        /// </summary>
        public static string CalculateReportHash(DryRunReport report)
        {
            var sb = new StringBuilder();
            sb.Append(report.inputConfigHash);
            sb.Append(report.totalPrefabs);
            sb.Append(report.wouldCreate);
            sb.Append(report.wouldSkip);
            sb.Append(report.errors);
            sb.Append(report.warnings);
            
            // Include all result hashes in sorted order
            var sortedHashes = report.results
                .OrderBy(r => r.prefabPath)
                .Select(r => r.contentHash);
            sb.Append(string.Join("|", sortedHashes));
            
            return CalculateHash(sb.ToString());
        }
        /// <summary>
        /// Generate a dry-run report for buildings.
        /// </summary>
        public static DryRunReport GenerateDryRunBuildings(
            List<BuildingTemplate> templates,
            List<MaterialTemplate> materialCatalog,
            List<MaterialRule> rules)
        {
            var report = new DryRunReport();
            var validator = new BuildingValidator();

            foreach (var template in templates)
            {
                var result = new DryRunResult
                {
                    prefabName = template.displayName ?? template.name,
                    prefabPath = $"Assets/Prefabs/Buildings/{template.name}.prefab"
                };

                // Validate
                var diagnostics = validator.Validate(template, materialCatalog, rules);
                result.isValid = diagnostics.isValid;
                result.validationErrors.AddRange(diagnostics.issues
                    .Where(i => i.severity == ValidationIssue.Severity.Error)
                    .Select(i => i.message));
                result.validationWarnings.AddRange(diagnostics.issues
                    .Where(i => i.severity == ValidationIssue.Severity.Warning)
                    .Select(i => i.message));

                // Check if prefab exists
                var existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(result.prefabPath);
                if (existingPrefab != null)
                {
                    // TODO: Compare with existing prefab to determine if update needed
                    result.wouldCreate = false;
                    report.wouldSkip++;
                }
                else
                {
                    result.wouldCreate = true;
                    report.wouldCreate++;
                }

                // Collect substitutions
                foreach (var cost in template.cost)
                {
                    // Find best matching material
                    MaterialTemplate bestMatch = null;
                    float bestScore = 0f;

                    foreach (var material in materialCatalog)
                    {
                        var costResult = MaterialRuleEngine.ValidateMaterialCost(cost, material);
                        if (costResult.passes)
                        {
                            if (costResult.substitutionScore > bestScore)
                            {
                                bestMatch = material;
                                bestScore = costResult.substitutionScore;
                            }
                        }
                    }

                    if (bestMatch != null)
                    {
                        result.substitutions.Add(new MaterialSubstitution
                        {
                            originalMaterial = cost.requiredUsage.ToString(),
                            substituteMaterial = bestMatch.name,
                            substitutionScore = bestScore,
                            reason = "Best match for cost requirement"
                        });
                    }
                }

                // Collect properties
                result.properties["buildingType"] = template.buildingType.ToString();
                result.properties["calculatedHealth"] = StatCalculation.CalculateBuildingHealth(template);
                result.properties["calculatedDesirability"] = StatCalculation.CalculateBuildingDesirability(template);
                result.properties["footprintSize"] = template.footprint.size;
                result.properties["facilityTags"] = template.facilityTags.ToString();

                // Generate sockets
                result.sockets.Add(new SocketDefinition
                {
                    name = "Entrance",
                    type = "entrance",
                    position = new Vector3(0, 0, -template.footprint.size.y / 2f)
                });

                if ((template.facilityTags & FacilityTags.Storage) != FacilityTags.None)
                {
                    result.sockets.Add(new SocketDefinition
                    {
                        name = "Logistics",
                        type = "logistics",
                        position = Vector3.zero
                    });
                }

                result.tags.Add(template.buildingType.ToString());
                if ((template.facilityTags & FacilityTags.Storage) != FacilityTags.None)
                {
                    result.tags.Add("Storage");
                }

                // Calculate hashes
                result.inputHash = CalculateInputHash(new List<BuildingTemplate> { template });
                result.contentHash = CalculateResultHash(result);

                report.results.Add(result);
            }

            report.totalPrefabs = templates.Count;
            report.errors = report.results.Count(r => !r.isValid);
            report.warnings = report.results.Sum(r => r.validationWarnings.Count);
            report.categoryCounts["Buildings"] = templates.Count;
            
            // Calculate input config hash
            report.inputConfigHash = CalculateInputHash(templates);
            
            // Calculate report hash
            report.reportHash = CalculateReportHash(report);
            
            // Calculate coverage stats
            report.coverageStats["Buildings"] = new CoverageStats
            {
                totalCatalogIds = templates.Count,
                generatedPrefabs = report.results.Count(r => r.wouldCreate),
                validPrefabs = report.results.Count(r => r.isValid),
                invalidPrefabs = report.results.Count(r => !r.isValid),
                coveragePercent = templates.Count > 0 ? (report.results.Count(r => r.wouldCreate) / (float)templates.Count) * 100f : 0f
            };

            return report;
        }

        /// <summary>
        /// Generate a dry-run report for equipment.
        /// </summary>
        public static DryRunReport GenerateDryRunEquipment(
            List<EquipmentTemplate> templates,
            List<MaterialTemplate> materialCatalog,
            List<MaterialRule> rules)
        {
            var report = new DryRunReport();
            var validator = new EquipmentValidator();

            foreach (var template in templates)
            {
                var result = new DryRunResult
                {
                    prefabName = template.displayName ?? template.name,
                    prefabPath = $"Assets/Prefabs/Equipment/{template.name}.prefab"
                };

                // Validate
                var diagnostics = validator.Validate(template, materialCatalog, rules);
                result.isValid = diagnostics.isValid;
                result.validationErrors.AddRange(diagnostics.issues
                    .Where(i => i.severity == ValidationIssue.Severity.Error)
                    .Select(i => i.message));
                result.validationWarnings.AddRange(diagnostics.issues
                    .Where(i => i.severity == ValidationIssue.Severity.Warning)
                    .Select(i => i.message));

                // Check if prefab exists
                var existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(result.prefabPath);
                if (existingPrefab != null)
                {
                    result.wouldCreate = false;
                    report.wouldSkip++;
                }
                else
                {
                    result.wouldCreate = true;
                    report.wouldCreate++;
                }

                // Find best material match
                var rule = new MaterialRule
                {
                    requiredUsage = template.requiredUsage,
                    requiredTraits = template.requiredTraits,
                    forbiddenTraits = template.forbiddenTraits,
                    minStats = template.minStats
                };

                var substitutes = MaterialRuleEngine.FindSubstitutes(rule);
                if (substitutes.Count > 0)
                {
                    result.substitutions.Add(new MaterialSubstitution
                    {
                        originalMaterial = template.requiredUsage.ToString(),
                        substituteMaterial = substitutes[0].name,
                        substitutionScore = 1f,
                        reason = "Best match for equipment requirements"
                    });
                }

                // Collect properties (including quality/rarity/tech tier per items.md spec)
                result.properties["equipmentType"] = template.equipmentType.ToString();
                result.properties["slotKind"] = template.slotKind.ToString();
                result.properties["mass"] = template.mass;
                result.properties["quality"] = template.quality;
                result.properties["calculatedQuality"] = template.calculatedQuality;
                result.properties["rarity"] = template.rarity.ToString();
                result.properties["techTier"] = template.techTier;
                result.properties["requiredTechTier"] = template.requiredTechTier;
                result.properties["itemDefId"] = template.itemDefId.Value;
                result.properties["calculatedDurability"] = StatCalculation.CalculateEquipmentDurability(template);

                // Generate socket
                result.sockets.Add(new SocketDefinition
                {
                    name = "Attach",
                    type = "attach",
                    position = Vector3.zero
                });

                result.tags.Add(template.equipmentType.ToString());
                result.tags.Add(template.slotKind.ToString());

                // Calculate hashes
                result.inputHash = CalculateInputHash(new List<EquipmentTemplate> { template });
                result.contentHash = CalculateResultHash(result);

                report.results.Add(result);
            }

            report.totalPrefabs = templates.Count;
            report.errors = report.results.Count(r => !r.isValid);
            report.warnings = report.results.Sum(r => r.validationWarnings.Count);
            report.categoryCounts["Equipment"] = templates.Count;
            
            // Calculate input config hash
            report.inputConfigHash = CalculateInputHash(templates);
            
            // Calculate report hash
            report.reportHash = CalculateReportHash(report);
            
            // Calculate coverage stats
            report.coverageStats["Equipment"] = new CoverageStats
            {
                totalCatalogIds = templates.Count,
                generatedPrefabs = report.results.Count(r => r.wouldCreate),
                validPrefabs = report.results.Count(r => r.isValid),
                invalidPrefabs = report.results.Count(r => !r.isValid),
                coveragePercent = templates.Count > 0 ? (report.results.Count(r => r.wouldCreate) / (float)templates.Count) * 100f : 0f
            };

            return report;
        }

        /// <summary>
        /// Export dry-run report to JSON (using Unity's JsonUtility).
        /// </summary>
        public static string ExportToJson(DryRunReport report)
        {
            // Unity's JsonUtility doesn't handle nested dictionaries well,
            // so we'll create a simplified JSON structure
            var json = new System.Text.StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"timestamp\": \"{report.timestamp:yyyy-MM-dd HH:mm:ss}\",");
            json.AppendLine($"  \"totalPrefabs\": {report.totalPrefabs},");
            json.AppendLine($"  \"wouldCreate\": {report.wouldCreate},");
            json.AppendLine($"  \"wouldSkip\": {report.wouldSkip},");
            json.AppendLine($"  \"errors\": {report.errors},");
            json.AppendLine($"  \"warnings\": {report.warnings},");
            json.AppendLine($"  \"reportHash\": \"{report.reportHash}\",");
            json.AppendLine($"  \"inputConfigHash\": \"{report.inputConfigHash}\",");
            json.AppendLine("  \"coverageStats\": {");
            
            // Add coverage stats
            bool firstCoverage = true;
            foreach (var kvp in report.coverageStats.OrderBy(c => c.Key))
            {
                if (!firstCoverage) json.AppendLine(",");
                var stats = kvp.Value;
                json.AppendLine($"    \"{kvp.Key}\": {{");
                json.AppendLine($"      \"totalCatalogIds\": {stats.totalCatalogIds},");
                json.AppendLine($"      \"generatedPrefabs\": {stats.generatedPrefabs},");
                json.AppendLine($"      \"validPrefabs\": {stats.validPrefabs},");
                json.AppendLine($"      \"invalidPrefabs\": {stats.invalidPrefabs},");
                json.Append($"      \"coveragePercent\": {stats.coveragePercent:F2}");
                json.Append("    }");
                firstCoverage = false;
            }
            json.AppendLine();
            json.AppendLine("  },");
            json.AppendLine("  \"results\": [");
            
            for (int i = 0; i < report.results.Count; i++)
            {
                var result = report.results[i];
                json.AppendLine("    {");
                json.AppendLine($"      \"prefabName\": \"{result.prefabName}\",");
                json.AppendLine($"      \"prefabPath\": \"{result.prefabPath}\",");
                json.AppendLine($"      \"wouldCreate\": {result.wouldCreate.ToString().ToLower()},");
                json.AppendLine($"      \"isValid\": {result.isValid.ToString().ToLower()},");
                json.AppendLine($"      \"contentHash\": \"{result.contentHash}\",");
                json.AppendLine($"      \"inputHash\": \"{result.inputHash}\",");
                json.AppendLine($"      \"validationErrors\": [{string.Join(", ", result.validationErrors.Select(e => $"\"{e.Replace("\"", "\\\"")}\""))}],");
                json.AppendLine($"      \"validationWarnings\": [{string.Join(", ", result.validationWarnings.Select(w => $"\"{w.Replace("\"", "\\\"")}\""))}]");
                json.Append(i < report.results.Count - 1 ? "    }," : "    }");
            }
            
            json.AppendLine("  ]");
            json.AppendLine("}");
            
            return json.ToString();
        }

        /// <summary>
        /// Generate a dry-run report for materials.
        /// </summary>
        public static DryRunReport GenerateDryRunMaterials(
            List<MaterialTemplate> templates,
            List<MaterialRule> rules)
        {
            var report = new DryRunReport();

            foreach (var template in templates)
            {
                var result = new DryRunResult
                {
                    prefabName = template.displayName ?? template.name,
                    prefabPath = $"Assets/Prefabs/Materials/{template.name}.prefab",
                    isValid = true
                };

                // Validate
                var validation = PrefabValidator.ValidateMaterial(template);
                result.isValid = validation.IsValid;
                result.validationErrors.AddRange(validation.Errors);
                result.validationWarnings.AddRange(validation.Warnings);

                // Check if prefab exists
                var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(result.prefabPath);
                if (existingPrefab != null)
                {
                    result.wouldCreate = false;
                    report.wouldSkip++;
                }
                else
                {
                    result.wouldCreate = true;
                    report.wouldCreate++;
                }

                // Collect properties (including quality/rarity/tech tier per items.md spec)
                result.properties["category"] = template.category.ToString();
                result.properties["usage"] = template.usage.ToString();
                result.properties["quality"] = template.quality;
                result.properties["calculatedQuality"] = template.calculatedQuality;
                result.properties["rarity"] = template.rarity.ToString();
                result.properties["techTier"] = template.techTier;
                result.properties["requiredTechTier"] = template.requiredTechTier;
                result.properties["materialId"] = template.materialId.Value;
                result.properties["purity"] = template.purity;

                result.tags.Add(template.category.ToString());

                // Calculate hashes
                result.inputHash = CalculateInputHash(new List<MaterialTemplate> { template });
                result.contentHash = CalculateResultHash(result);

                report.results.Add(result);
            }

            report.totalPrefabs = templates.Count;
            report.errors = report.results.Count(r => !r.isValid);
            report.warnings = report.results.Sum(r => r.validationWarnings.Count);
            report.categoryCounts["Materials"] = templates.Count;
            
            // Calculate input config hash
            report.inputConfigHash = CalculateInputHash(templates);
            
            // Calculate report hash
            report.reportHash = CalculateReportHash(report);
            
            // Calculate coverage stats
            report.coverageStats["Materials"] = new CoverageStats
            {
                totalCatalogIds = templates.Count,
                generatedPrefabs = report.results.Count(r => r.wouldCreate),
                validPrefabs = report.results.Count(r => r.isValid),
                invalidPrefabs = report.results.Count(r => !r.isValid),
                coveragePercent = templates.Count > 0 ? (report.results.Count(r => r.wouldCreate) / (float)templates.Count) * 100f : 0f
            };

            return report;
        }

        /// <summary>
        /// Generate a dry-run report for individuals.
        /// </summary>
        public static DryRunReport GenerateDryRunIndividuals(
            List<IndividualTemplate> templates)
        {
            var report = new DryRunReport();

            foreach (var template in templates)
            {
                var result = new DryRunResult
                {
                    prefabName = template.displayName ?? template.name,
                    prefabPath = $"Assets/Prefabs/Individuals/{template.name}.prefab",
                    isValid = true
                };

                // Validate
                var validation = PrefabValidator.ValidateIndividual(template);
                result.isValid = validation.IsValid;
                result.validationErrors.AddRange(validation.Errors);
                result.validationWarnings.AddRange(validation.Warnings);

                // Check if prefab exists
                var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(result.prefabPath);
                if (existingPrefab != null)
                {
                    result.wouldCreate = false;
                    report.wouldSkip++;
                }
                else
                {
                    result.wouldCreate = true;
                    report.wouldCreate++;
                }

                // Collect properties
                result.properties["individualType"] = template.individualType.ToString();
                result.properties["baseHealth"] = template.baseHealth;
                result.properties["baseSpeed"] = template.baseSpeed;

                result.tags.Add(template.individualType.ToString());

                // Calculate hashes
                result.inputHash = CalculateInputHash(new List<IndividualTemplate> { template });
                result.contentHash = CalculateResultHash(result);

                report.results.Add(result);
            }

            report.totalPrefabs = templates.Count;
            report.errors = report.results.Count(r => !r.isValid);
            report.warnings = report.results.Sum(r => r.validationWarnings.Count);
            report.categoryCounts["Individuals"] = templates.Count;
            
            // Calculate input config hash
            report.inputConfigHash = CalculateInputHash(templates);
            
            // Calculate report hash
            report.reportHash = CalculateReportHash(report);
            
            // Calculate coverage stats
            report.coverageStats["Individuals"] = new CoverageStats
            {
                totalCatalogIds = templates.Count,
                generatedPrefabs = report.results.Count(r => r.wouldCreate),
                validPrefabs = report.results.Count(r => r.isValid),
                invalidPrefabs = report.results.Count(r => !r.isValid),
                coveragePercent = templates.Count > 0 ? (report.results.Count(r => r.wouldCreate) / (float)templates.Count) * 100f : 0f
            };

            return report;
        }

        /// <summary>
        /// Generate a diff summary (what would change).
        /// </summary>
        public static string GenerateDiffSummary(DryRunReport report)
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Dry-Run Report - {report.timestamp:yyyy-MM-dd HH:mm:ss}");
            summary.AppendLine($"Total Prefabs: {report.totalPrefabs}");
            summary.AppendLine($"Would Create: {report.wouldCreate}");
            summary.AppendLine($"Would Skip: {report.wouldSkip}");
            summary.AppendLine($"Errors: {report.errors}");
            summary.AppendLine($"Warnings: {report.warnings}");
            summary.AppendLine();

            if (report.errors > 0)
            {
                summary.AppendLine("ERRORS:");
                foreach (var result in report.results.Where(r => !r.isValid))
                {
                    summary.AppendLine($"  {result.prefabName}:");
                    foreach (var error in result.validationErrors)
                    {
                        summary.AppendLine($"    - {error}");
                    }
                }
                summary.AppendLine();
            }

            if (report.warnings > 0)
            {
                summary.AppendLine("WARNINGS:");
                foreach (var result in report.results.Where(r => r.validationWarnings.Count > 0))
                {
                    summary.AppendLine($"  {result.prefabName}:");
                    foreach (var warning in result.validationWarnings)
                    {
                        summary.AppendLine($"    - {warning}");
                    }
                }
                summary.AppendLine();
            }

            summary.AppendLine("SUBSTITUTIONS:");
            foreach (var result in report.results.Where(r => r.substitutions.Count > 0))
            {
                summary.AppendLine($"  {result.prefabName}:");
                foreach (var sub in result.substitutions)
                {
                    summary.AppendLine($"    {sub.originalMaterial} → {sub.substituteMaterial} (score: {sub.substitutionScore:F2})");
                }
            }

            return summary.ToString();
        }

        /// <summary>
        /// Generate a dry-run report for tools with production chain validation.
        /// </summary>
        public static DryRunReport GenerateDryRunTools(
            List<ToolTemplate> templates,
            List<MaterialTemplate> materialCatalog)
        {
            var report = new DryRunReport();

            foreach (var template in templates)
            {
                var result = new DryRunResult
                {
                    prefabName = template.displayName ?? template.name,
                    prefabPath = $"Assets/Prefabs/Tools/{template.name}.prefab",
                    isValid = true
                };

                // Validate production chain (per items.md spec)
                var chainValidation = ProductionChainValidator.ValidateToolProductionChain(
                    template,
                    materialCatalog,
                    templates // tool catalog for cycle detection
                );
                
                result.isValid = chainValidation.IsValid;
                result.validationErrors.AddRange(chainValidation.Errors);
                result.validationWarnings.AddRange(chainValidation.Warnings);

                // Validate template itself
                var templateValidation = PrefabValidator.ValidateTool(template);
                result.isValid = result.isValid && templateValidation.IsValid;
                result.validationErrors.AddRange(templateValidation.Errors);
                result.validationWarnings.AddRange(templateValidation.Warnings);

                // Check if prefab exists
                var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(result.prefabPath);
                if (existingPrefab != null)
                {
                    result.wouldCreate = false;
                    report.wouldSkip++;
                }
                else
                {
                    result.wouldCreate = true;
                    report.wouldCreate++;
                }

                // Collect properties (including quality/rarity/tech tier per items.md spec)
                result.properties["quality"] = template.quality;
                result.properties["calculatedQuality"] = template.calculatedQuality;
                result.properties["rarity"] = template.rarity.ToString();
                result.properties["techTier"] = template.techTier;
                result.properties["requiredTechTier"] = template.requiredTechTier;
                result.properties["itemDefId"] = template.itemDefId.Value;
                result.properties["recipeId"] = template.recipeId.Value;
                result.properties["baseFacilityQuality"] = template.baseFacilityQuality;
                result.properties["productionInputs"] = template.productionInputs?.Count ?? 0;

                // Calculate preview quality if inputs exist
                if (template.productionInputs != null && template.productionInputs.Count > 0)
                {
                    var inputMaterials = template.productionInputs
                        .Select(input => materialCatalog?.FirstOrDefault(m => 
                            m.name == input.materialName || m.displayName == input.materialName))
                        .Where(m => m != null)
                        .ToList();
                    
                    if (inputMaterials.Count > 0 && template.qualityDerivation != null)
                    {
                        var weights = template.qualityDerivation.ToQualityWeights();
                        float previewQuality = StatCalculation.CalculateItemQuality(
                            template.quality,
                            weights,
                            inputMaterials,
                            50f, // Example craftsman skill
                            template.baseFacilityQuality
                        );
                        result.properties["previewCalculatedQuality"] = previewQuality;
                        
                        // Calculate rarity preview
                        Rarity maxMaterialRarity = inputMaterials.Max(m => m.rarity);
                        Rarity previewRarity = StatCalculation.CalculateRarity(
                            previewQuality,
                            maxMaterialRarity,
                            50f // Example craftsman skill
                        );
                        result.properties["previewRarity"] = previewRarity.ToString();
                    }
                }

                result.tags.Add("Tool");
                if (template.productionInputs != null && template.productionInputs.Count > 0)
                {
                    result.tags.Add("Producible");
                }
                else
                {
                    result.tags.Add("BaseMaterial");
                }

                // Calculate hashes
                result.inputHash = CalculateInputHash(new List<ToolTemplate> { template });
                result.contentHash = CalculateResultHash(result);

                report.results.Add(result);
            }

            report.totalPrefabs = templates.Count;
            report.errors = report.results.Count(r => !r.isValid);
            report.warnings = report.results.Sum(r => r.validationWarnings.Count);
            report.categoryCounts["Tools"] = templates.Count;
            
            // Calculate input config hash
            report.inputConfigHash = CalculateInputHash(templates);
            
            // Calculate report hash
            report.reportHash = CalculateReportHash(report);
            
            // Calculate coverage stats
            report.coverageStats["Tools"] = new CoverageStats
            {
                totalCatalogIds = templates.Count,
                generatedPrefabs = report.results.Count(r => r.wouldCreate),
                validPrefabs = report.results.Count(r => r.isValid),
                invalidPrefabs = report.results.Count(r => !r.isValid),
                coveragePercent = templates.Count > 0 ? (report.results.Count(r => r.wouldCreate) / (float)templates.Count) * 100f : 0f
            };

            return report;
        }
        
        /// <summary>
        /// Compare two dry-run reports for idempotency checking.
        /// Returns true if reports are identical (same hashes with same inputs).
        /// </summary>
        public static bool CompareReportsForIdempotency(DryRunReport report1, DryRunReport report2)
        {
            if (report1.inputConfigHash != report2.inputConfigHash)
                return false; // Different inputs
            
            if (report1.reportHash != report2.reportHash)
                return false; // Different outputs
            
            return true; // Identical
        }
        
        /// <summary>
        /// Generate coverage heatmap report showing % of catalog IDs with generated prefabs.
        /// </summary>
        public static string GenerateCoverageHeatmap(DryRunReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Prefab Maker Coverage Heatmap ===");
            sb.AppendLine($"Generated: {report.timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            foreach (var kvp in report.coverageStats.OrderBy(c => c.Key))
            {
                var stats = kvp.Value;
                sb.AppendLine($"{kvp.Key}:");
                sb.AppendLine($"  Total Catalog IDs: {stats.totalCatalogIds}");
                sb.AppendLine($"  Generated Prefabs: {stats.generatedPrefabs}");
                sb.AppendLine($"  Valid: {stats.validPrefabs} | Invalid: {stats.invalidPrefabs}");
                sb.AppendLine($"  Coverage: {stats.coveragePercent:F2}%");
                sb.AppendLine();
            }
            
            // Overall summary
            int totalIds = report.coverageStats.Values.Sum(s => s.totalCatalogIds);
            int totalGenerated = report.coverageStats.Values.Sum(s => s.generatedPrefabs);
            float overallCoverage = totalIds > 0 ? (totalGenerated / (float)totalIds) * 100f : 0f;
            
            sb.AppendLine("Overall:");
            sb.AppendLine($"  Total Catalog IDs: {totalIds}");
            sb.AppendLine($"  Generated Prefabs: {totalGenerated}");
            sb.AppendLine($"  Overall Coverage: {overallCoverage:F2}%");
            
            return sb.ToString();
        }
    }
}

