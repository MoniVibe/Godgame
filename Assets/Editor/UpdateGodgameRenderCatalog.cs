using UnityEditor;
using UnityEngine;
using Godgame.Rendering.Catalog;
using System.Collections.Generic;

public class UpdateGodgameRenderCatalog
{
    public static void Execute()
    {
        string assetPath = "Assets/Rendering/GodgameRenderCatalog.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<GodgameRenderCatalogDefinition>(assetPath);
        if (catalog == null)
        {
            Debug.Log("Creating new GodgameRenderCatalog asset...");
            catalog = ScriptableObject.CreateInstance<GodgameRenderCatalogDefinition>();
            AssetDatabase.CreateAsset(catalog, assetPath);
        }

        // Load Materials
        var matVillager = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VillagerRed.mat");
        var matDebug = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_ECS_Debug_Lit.mat");
        var matGreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GreenURP.mat");
        var matGround = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GroundURP.mat");

        if (matVillager == null) Debug.LogError("VillagerRed.mat not found");
        if (matDebug == null) Debug.LogError("M_ECS_Debug_Lit.mat not found");

        // Get Meshes
        var meshCube = GetPrimitiveMesh(PrimitiveType.Cube);
        var meshSphere = GetPrimitiveMesh(PrimitiveType.Sphere);
        var meshCapsule = GetPrimitiveMesh(PrimitiveType.Capsule);
        var meshCylinder = GetPrimitiveMesh(PrimitiveType.Cylinder);

        var entries = new List<GodgameRenderCatalogDefinition.Entry>();

        // 100: Villager
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 100, Mesh = meshCube, Material = matVillager });
        // 110: VillageCenter
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 110, Mesh = meshCube, Material = matDebug });
        // 120: ResourceChunk
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 120, Mesh = meshSphere, Material = matGreen });
        // 130: Vegetation
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 130, Mesh = meshCapsule, Material = matGreen });
        // 140: ResourceNode
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 140, Mesh = meshCylinder, Material = matGround });
        // 150: Storehouse
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 150, Mesh = meshCube, Material = matDebug });
        // 160: ConstructionGhost
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 160, Mesh = meshCube, Material = matDebug }); // Transparent?
        // 170: Band
        entries.Add(new GodgameRenderCatalogDefinition.Entry { Key = 170, Mesh = meshSphere, Material = matVillager });

        catalog.Entries = entries.ToArray();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"Updated GodgameRenderCatalog with {entries.Count} entries.");
    }

    private static Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        GameObject.DestroyImmediate(go);
        return mesh;
    }
}
