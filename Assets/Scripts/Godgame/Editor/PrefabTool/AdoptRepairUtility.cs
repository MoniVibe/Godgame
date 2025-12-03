using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Adopt existing prefabs into the Prefab Maker system and repair broken references.
    /// </summary>
    public static class AdoptRepairUtility
    {
        /// <summary>
        /// Adopt an existing prefab into the template system.
        /// Attempts to extract template data from the prefab's components.
        /// </summary>
        public static PrefabTemplate AdoptPrefab(GameObject prefab, PrefabType expectedType)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }
            
            switch (expectedType)
            {
                case PrefabType.Building:
                    return AdoptBuildingPrefab(prefab);
                case PrefabType.Equipment:
                    return AdoptEquipmentPrefab(prefab);
                case PrefabType.Material:
                    return AdoptMaterialPrefab(prefab);
                case PrefabType.Tool:
                    return AdoptToolPrefab(prefab);
                case PrefabType.Individual:
                    return AdoptIndividualPrefab(prefab);
                default:
                    throw new ArgumentException($"Unsupported prefab type: {expectedType}");
            }
        }
        
        /// <summary>
        /// Repair broken references in a template (missing assets, invalid paths).
        /// </summary>
        public static RepairResult RepairTemplate(PrefabTemplate template)
        {
            var result = new RepairResult
            {
                Template = template,
                FixedIssues = new List<string>()
            };
            
            // Repair visual presentation
            if (template.visualPresentation != null)
            {
                if (template.visualPresentation.prefabAsset == null && !string.IsNullOrEmpty(template.name))
                {
                    // Try to find prefab asset by name
                    string[] guids = AssetDatabase.FindAssets($"{template.name} t:Prefab");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        template.visualPresentation.prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        result.FixedIssues.Add($"Found prefab asset: {path}");
                    }
                }
                
                if (template.visualPresentation.meshAsset == null)
                {
                    // Try to find mesh by name
                    string[] guids = AssetDatabase.FindAssets($"{template.name} t:Mesh");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        template.visualPresentation.meshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        result.FixedIssues.Add($"Found mesh asset: {path}");
                    }
                }
            }
            
            // Repair VFX presentation
            if (template.vfxPresentation != null)
            {
                if (template.vfxPresentation.vfxAsset == null && !string.IsNullOrEmpty(template.name))
                {
                    string[] guids = AssetDatabase.FindAssets($"{template.name} t:VisualEffectAsset");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        template.vfxPresentation.vfxAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.VFX.VisualEffectAsset>(path);
                        result.FixedIssues.Add($"Found VFX asset: {path}");
                    }
                }
            }
            
            // Ensure display name is set
            if (string.IsNullOrEmpty(template.displayName))
            {
                template.displayName = template.name;
                result.FixedIssues.Add("Set display name from name");
            }
            
            result.Success = true;
            return result;
        }
        
        /// <summary>
        /// Repair all templates in a list.
        /// </summary>
        public static List<RepairResult> RepairAllTemplates<T>(List<T> templates) where T : PrefabTemplate
        {
            var results = new List<RepairResult>();
            
            foreach (var template in templates)
            {
                results.Add(RepairTemplate(template));
            }
            
            return results;
        }
        
        private static BuildingTemplate AdoptBuildingPrefab(GameObject prefab)
        {
            var template = new BuildingTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0 // Will be reassigned
            };
            
            // Try to extract data from components
            // TODO: Look for BuildingAuthoring or similar components
            
            // Extract visual presentation
            template.visualPresentation.prefabAsset = prefab;
            
            // Estimate footprint from bounds
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                template.footprint.size = new Vector2(bounds.size.x, bounds.size.z);
            }
            else
            {
                template.footprint.size = new Vector2(1, 1); // Default
            }
            
            return template;
        }
        
        private static EquipmentTemplate AdoptEquipmentPrefab(GameObject prefab)
        {
            var template = new EquipmentTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            // Try to extract stats from components
            // TODO: Look for EquipmentAuthoring or similar
            
            return template;
        }
        
        private static MaterialTemplate AdoptMaterialPrefab(GameObject prefab)
        {
            var template = new MaterialTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            return template;
        }
        
        private static ToolTemplate AdoptToolPrefab(GameObject prefab)
        {
            var template = new ToolTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            return template;
        }
        
        private static IndividualTemplate AdoptIndividualPrefab(GameObject prefab)
        {
            var template = new IndividualTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            return template;
        }
    }
    
    /// <summary>
    /// Result of repair operation.
    /// </summary>
    [Serializable]
    public class RepairResult
    {
        public PrefabTemplate Template;
        public bool Success;
        public List<string> FixedIssues = new List<string>();
    }
    
    /// <summary>
    /// Prefab type enumeration.
    /// </summary>
    public enum PrefabType
    {
        Building,
        Equipment,
        Material,
        Tool,
        Individual,
        Reagent,
        Miracle
    }
}

