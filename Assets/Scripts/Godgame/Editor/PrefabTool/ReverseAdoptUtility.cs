using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Reverse-adopt: Extract template data from existing prefabs.
    /// Useful for migrating legacy prefabs into the Prefab Maker system.
    /// </summary>
    public static class ReverseAdoptUtility
    {
        /// <summary>
        /// Extract template from an existing prefab by analyzing its components and structure.
        /// </summary>
        public static PrefabTemplate ExtractTemplate(GameObject prefab, PrefabType expectedType)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }
            
            // Load prefab asset if it's a scene instance
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            if (prefabAsset == null)
            {
                prefabAsset = prefab;
            }
            
            switch (expectedType)
            {
                case PrefabType.Building:
                    return ExtractBuildingTemplate(prefabAsset);
                case PrefabType.Equipment:
                    return ExtractEquipmentTemplate(prefabAsset);
                case PrefabType.Material:
                    return ExtractMaterialTemplate(prefabAsset);
                case PrefabType.Tool:
                    return ExtractToolTemplate(prefabAsset);
                case PrefabType.Individual:
                    return ExtractIndividualTemplate(prefabAsset);
                default:
                    throw new ArgumentException($"Unsupported prefab type: {expectedType}");
            }
        }
        
        /// <summary>
        /// Extract templates from all prefabs in a folder.
        /// </summary>
        public static List<PrefabTemplate> ExtractTemplatesFromFolder(string folderPath, PrefabType expectedType)
        {
            var templates = new List<PrefabTemplate>();
            
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning($"[ReverseAdopt] Invalid folder: {folderPath}");
                return templates;
            }
            
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    try
                    {
                        var template = ExtractTemplate(prefab, expectedType);
                        templates.Add(template);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ReverseAdopt] Failed to extract template from {path}: {ex.Message}");
                    }
                }
            }
            
            return templates;
        }
        
        private static BuildingTemplate ExtractBuildingTemplate(GameObject prefab)
        {
            var template = new BuildingTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            // Extract visual presentation
            template.visualPresentation.prefabAsset = prefab;
            
            // Estimate footprint
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                template.footprint.size = new Vector2(bounds.size.x, bounds.size.z);
            }
            else
            {
                // Try collider
                var collider = prefab.GetComponent<Collider>();
                if (collider != null)
                {
                    Bounds bounds = collider.bounds;
                    template.footprint.size = new Vector2(bounds.size.x, bounds.size.z);
                }
                else
                {
                    template.footprint.size = new Vector2(1, 1);
                }
            }
            
            // Try to extract from authoring components
            // TODO: Look for BuildingAuthoring component
            
            return template;
        }
        
        private static EquipmentTemplate ExtractEquipmentTemplate(GameObject prefab)
        {
            var template = new EquipmentTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            // Try to extract from authoring components
            // TODO: Look for EquipmentAuthoring component
            
            return template;
        }
        
        private static MaterialTemplate ExtractMaterialTemplate(GameObject prefab)
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
        
        private static ToolTemplate ExtractToolTemplate(GameObject prefab)
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
        
        private static IndividualTemplate ExtractIndividualTemplate(GameObject prefab)
        {
            var template = new IndividualTemplate
            {
                name = prefab.name,
                displayName = prefab.name,
                id = 0
            };
            
            template.visualPresentation.prefabAsset = prefab;
            
            // Try to extract from authoring components
            // TODO: Look for IndividualAuthoring component
            
            return template;
        }
    }
}

