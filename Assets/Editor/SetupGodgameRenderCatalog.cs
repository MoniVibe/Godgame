using UnityEngine;
using UnityEditor;
using Godgame.Rendering.Catalog;
using Godgame.Rendering;
using System.Collections.Generic;

public class SetupGodgameRenderCatalog : MonoBehaviour
{
    public static void Execute()
    {
        // 1. Create the Asset
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

        var entries = new List<GodgameRenderCatalogDefinition.Entry>();
        var urpLit = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
        if (urpLit == null)
        {
            var urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader != null)
            {
                urpLit = new Material(urpShader);
                urpLit.enableInstancing = true;
            }
        }

        // Helper to add entry
        void AddEntry(ushort key, string prefabPath, string fallbackPrimitive = "Cube")
        {
            Mesh mesh = null;
            Material material = null;
            Vector3 boundsCenter = Vector3.zero;
            Vector3 boundsExtents = Vector3.one * 0.5f;

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

            if (mesh == null)
            {
                var prim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                if (fallbackPrimitive == "Sphere") prim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                else if (fallbackPrimitive == "Capsule") prim = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                else if (fallbackPrimitive == "Cylinder") prim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                
                mesh = prim.GetComponent<MeshFilter>().sharedMesh;
                if (material == null) material = urpLit != null ? urpLit : prim.GetComponent<MeshRenderer>().sharedMaterial;
                GameObject.DestroyImmediate(prim);
                
                boundsCenter = mesh.bounds.center;
                boundsExtents = mesh.bounds.extents;
            }
            
            // If material is still null, use URP Lit (preferred) or final default fallback.
            if (material == null && urpLit != null) material = urpLit;
            if (material == null) material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

            entries.Add(new GodgameRenderCatalogDefinition.Entry
            {
                Key = key,
                Mesh = mesh,
                Material = material,
                BoundsCenter = boundsCenter,
                BoundsExtents = boundsExtents
            });
            
            Debug.Log($"Added Entry Key={key} Mesh={mesh?.name} Mat={material?.name}");
        }

        // Villager roles (100-107)
        AddEntry(GodgameRenderKeys.VillagerMiner, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerFarmer, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerForester, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerBreeder, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerWorshipper, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerRefiner, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerPeacekeeper, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");
        AddEntry(GodgameRenderKeys.VillagerCombatant, "Assets/Placeholders/Villager_Placeholder.prefab", "Capsule");

        // VillageCenter (110)
        AddEntry(GodgameRenderKeys.VillageCenter, "Assets/Placeholders/Building_Placeholder.prefab", "Cube");

        // ResourceChunk (120)
        AddEntry(GodgameRenderKeys.ResourceChunk, "Assets/Godgame/GG_Rock.prefab", "Sphere");
        
        // Vegetation (130)
        AddEntry(GodgameRenderKeys.Vegetation, "Assets/Prefabs/Vegetation/Tree_Placeholder.prefab", "Cylinder");

        // ResourceNode (140)
        AddEntry(GodgameRenderKeys.ResourceNode, "Assets/Placeholders/ResourceNode_Placeholder.prefab", "Cube");
        
        // Storehouse (150)
        AddEntry(GodgameRenderKeys.Storehouse, "Assets/Placeholders/Building_Placeholder.prefab", "Cube"); // Reuse building for now

        catalogDef.Entries = entries.ToArray();
        EditorUtility.SetDirty(catalogDef);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Updated GodgameRenderCatalog asset at {assetPath} with {entries.Count} entries.");

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
}
