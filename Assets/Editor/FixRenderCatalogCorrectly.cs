#if false
using UnityEngine;
using UnityEditor;
using Godgame.Rendering.Catalog;

public class FixRenderCatalogCorrectly
{
    public static void Execute()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<GodgameRenderCatalogDefinition>("Assets/Rendering/GodgameRenderCatalog.asset");
        if (catalog == null)
        {
            Debug.LogError("Catalog not found!");
            return;
        }

        var villagerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/Villager_Orange.mat");
        var centerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/VillageCenter_Gray.mat");
        var nodeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/ResourceNode_Cyan.mat");
        var chunkMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/ResourceChunk_Gold.mat");
        var vegMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/Vegetation_Green.mat");

        // Get default cube mesh
        var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx"); 
        if (cube == null) cube = Resources.GetBuiltinResource<Mesh>("Cube");

        if (cube == null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube = go.GetComponent<MeshFilter>().sharedMesh;
            GameObject.DestroyImmediate(go);
        }

        // Resize if needed (we need up to 170)
        // But we can just update existing entries if they exist
        
        for (int i = 0; i < catalog.Entries.Length; i++)
        {
            var entry = catalog.Entries[i];
            
            // 100 Villager -> Orange
            if (entry.Key == 100) entry.Material = villagerMat;
            
            // 110 VillageCenter -> Gray
            else if (entry.Key == 110) entry.Material = centerMat;
            
            // 120 ResourceChunk -> Gold
            else if (entry.Key == 120) entry.Material = chunkMat;
            
            // 130 Vegetation -> Green
            else if (entry.Key == 130) entry.Material = vegMat;
            
            // 140 ResourceNode -> Cyan
            else if (entry.Key == 140) entry.Material = nodeMat;
            
            // 150 Storehouse -> Gray (Reuse center mat)
            else if (entry.Key == 150) entry.Material = centerMat;

            // Ensure mesh is set
            if (entry.Mesh == null) entry.Mesh = cube;

            catalog.Entries[i] = entry;
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("Render Catalog Corrected");
    }
}
#endif
