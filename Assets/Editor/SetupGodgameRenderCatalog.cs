using UnityEngine;
using UnityEditor;
using Godgame.Rendering;
using Godgame.Rendering.Catalog;
using PureDOTS.Rendering;
using System.Collections.Generic;

public class SetupGodgameRenderCatalog : MonoBehaviour
{
    public static void Execute()
    {
        var assetPath = "Assets/Rendering/GodgameRenderCatalog.asset";
        var catalogDef = AssetDatabase.LoadAssetAtPath<GodgameRenderCatalogDefinition>(assetPath);
        
        if (!AssetDatabase.IsValidFolder("Assets/Rendering"))
        {
            AssetDatabase.CreateFolder("Assets", "Rendering");
        }

        if (catalogDef == null)
        {
            catalogDef = ScriptableObject.CreateInstance<GodgameRenderCatalogDefinition>();
            AssetDatabase.CreateAsset(catalogDef, assetPath);
        }

        var urpLit = LoadLitMaterial();
        var fallbackMaterial = urpLit != null ? urpLit : AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        var fallbackMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        catalogDef.FallbackMaterial = fallbackMaterial;
        catalogDef.FallbackMesh = fallbackMesh;

        var variants = new List<RenderPresentationCatalogDefinition.VariantDefinition>();
        var variantLookup = new Dictionary<string, int>();

        int AddVariant(string variantName, string prefabPath, string fallbackPrimitive)
        {
            if (variantLookup.TryGetValue(variantName, out var existingIndex))
                return existingIndex;

            Mesh mesh = null;
            Material material = null;
            Vector3 boundsCenter = Vector3.zero;
            Vector3 boundsExtents = Vector3.one * 0.5f;

            if (!string.IsNullOrEmpty(prefabPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    var mf = prefab.GetComponentInChildren<MeshFilter>();
                    var mr = prefab.GetComponentInChildren<MeshRenderer>();
                    if (mf != null) mesh = mf.sharedMesh;
                    if (mr != null) material = mr.sharedMaterial;
                    if (material != null && material.shader != null && material.shader.name == "Standard" && urpLit != null)
                    {
                        material = urpLit;
                    }
                    if (mesh != null)
                    {
                        boundsCenter = mesh.bounds.center;
                        boundsExtents = mesh.bounds.extents;
                    }
                }
            }

            if (mesh == null)
            {
                var primitive = CreatePrimitive(fallbackPrimitive);
                mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
                if (material == null)
                {
                    var primitiveMaterial = primitive.GetComponent<MeshRenderer>().sharedMaterial;
                    material = urpLit != null ? urpLit : primitiveMaterial;
                }
                GameObject.DestroyImmediate(primitive);
                boundsCenter = mesh.bounds.center;
                boundsExtents = mesh.bounds.extents;
            }

            if (material == null)
            {
                material = fallbackMaterial;
            }

            variants.Add(new RenderPresentationCatalogDefinition.VariantDefinition
            {
                Name = variantName,
                Mesh = mesh,
                Material = material,
                BoundsCenter = boundsCenter,
                BoundsExtents = boundsExtents,
                PresenterMask = RenderPresenterMask.Mesh
            });
            var index = variants.Count - 1;
            variantLookup[variantName] = index;
            Debug.Log($"[SetupGodgameRenderCatalog] Added variant '{variantName}' mesh={mesh?.name} material={material?.name}");
            return index;
        }

        GameObject CreatePrimitive(string primitive)
        {
            return primitive switch
            {
                "Sphere" => GameObject.CreatePrimitive(PrimitiveType.Sphere),
                "Capsule" => GameObject.CreatePrimitive(PrimitiveType.Capsule),
                "Cylinder" => GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                _ => GameObject.CreatePrimitive(PrimitiveType.Cube)
            };
        }

        var semanticConfigs = new List<(ushort Key, string VariantName, string PrefabPath, string Primitive)>
        {
            (GodgameSemanticKeys.VillagerMiner, "VillagerMiner_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerFarmer, "VillagerFarmer_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerForester, "VillagerForester_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerBreeder, "VillagerBreeder_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerWorshipper, "VillagerWorshipper_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerRefiner, "VillagerRefiner_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerPeacekeeper, "VillagerPeacekeeper_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillagerCombatant, "VillagerCombatant_Cylinder", "Assets/Placeholders/Villager_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.VillageCenter, "VillageCenter_Cube", "Assets/Placeholders/Building_Placeholder.prefab", "Cube"),
            (GodgameSemanticKeys.ResourceChunk, "ResourceChunk_Sphere", "Assets/Godgame/GG_Rock.prefab", "Sphere"),
            (GodgameSemanticKeys.Vegetation, "Vegetation_Cylinder", "Assets/Prefabs/Vegetation/Tree_Placeholder.prefab", "Cylinder"),
            (GodgameSemanticKeys.ResourceNode, "ResourceNode_Cube", "Assets/Placeholders/ResourceNode_Placeholder.prefab", "Cube"),
            (GodgameSemanticKeys.Storehouse, "Storehouse_Cube", "Assets/Placeholders/Building_Placeholder.prefab", "Cube"),
            (GodgameSemanticKeys.Housing, "Housing_Cube", "Assets/Placeholders/Building_Placeholder.prefab", "Cube"),
            (GodgameSemanticKeys.Worship, "Worship_Cube", "Assets/Placeholders/Building_Placeholder.prefab", "Cube")
        };

        foreach (var config in semanticConfigs)
        {
            AddVariant(config.VariantName, config.PrefabPath, config.Primitive);
        }

        var baseThemeMappings = new List<RenderPresentationCatalogDefinition.SemanticVariant>();
        foreach (var config in semanticConfigs)
        {
            if (!variantLookup.TryGetValue(config.VariantName, out var variantIndex))
            {
                Debug.LogError($"[SetupGodgameRenderCatalog] Missing variant '{config.VariantName}' for semantic {config.Key}.");
                continue;
            }
            baseThemeMappings.Add(new RenderPresentationCatalogDefinition.SemanticVariant
            {
                SemanticKey = config.Key,
                Lod0Variant = variantIndex,
                Lod1Variant = 0,
                Lod2Variant = 0
            });
        }

        catalogDef.Variants = variants.ToArray();
        catalogDef.Themes = new[]
        {
            new RenderPresentationCatalogDefinition.ThemeDefinition
            {
                Name = "Default",
                ThemeId = 0,
                SemanticVariants = baseThemeMappings.ToArray()
            }
        };
        EditorUtility.SetDirty(catalogDef);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Updated GodgameRenderCatalog asset at {assetPath} with {catalogDef.Variants.Length} variants across {catalogDef.Themes.Length} themes.");

        // 2. Create GameObject in Scene
        var goName = "GodgameRenderCatalog";
        var go = GameObject.Find(goName);
        if (go == null)
        {
            go = new GameObject(goName);
            Debug.Log($"Created GameObject '{goName}'");
        }
        
        var authoring = go.GetComponent<RenderCatalogAuthoring>();
        if (authoring == null)
        {
            authoring = go.AddComponent<RenderCatalogAuthoring>();
        }

        authoring.CatalogDefinition = catalogDef;
        Debug.Log($"Assigned CatalogDefinition to '{goName}'");
        
        EditorUtility.SetDirty(go);
    }

    private static Material LoadLitMaterial()
    {
        var lit = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
        if (lit == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                lit = new Material(shader);
                lit.enableInstancing = true;
            }
        }
        return lit;
    }
}
