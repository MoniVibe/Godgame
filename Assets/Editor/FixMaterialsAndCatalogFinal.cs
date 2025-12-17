#if false
using UnityEngine;
using UnityEditor;
using Godgame.Rendering.Catalog;

public class FixMaterialsAndCatalogFinal
{
    public static void Execute()
    {
        // 1. Ensure Materials use URP Unlit (safest)
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader"); // Should not happen

        UpdateMaterial("Assets/Materials/Godgame/Villager_Orange.mat", new Color(1f, 0.5f, 0f), shader);
        UpdateMaterial("Assets/Materials/Godgame/VillageCenter_Gray.mat", Color.gray, shader);
        UpdateMaterial("Assets/Materials/Godgame/ResourceNode_Cyan.mat", Color.cyan, shader);
        UpdateMaterial("Assets/Materials/Godgame/ResourceChunk_Gold.mat", new Color(1f, 0.84f, 0f), shader);
        UpdateMaterial("Assets/Materials/Godgame/Vegetation_Green.mat", Color.green, shader);

        AssetDatabase.SaveAssets();

        // 2. Update Catalog
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

        var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        if (cube == null) cube = Resources.GetBuiltinResource<Mesh>("Cube");

        for (int i = 0; i < catalog.Entries.Length; i++)
        {
            var entry = catalog.Entries[i];
            entry.Mesh = cube; // Force cube for everything

            switch (entry.Key)
            {
                case 100: entry.Material = villagerMat; break; // Villager
                case 110: entry.Material = centerMat; break; // VillageCenter
                case 120: entry.Material = chunkMat; break; // ResourceChunk
                case 130: entry.Material = vegMat; break; // Vegetation
                case 140: entry.Material = nodeMat; break; // ResourceNode
                case 150: entry.Material = centerMat; break; // Storehouse
                default: entry.Material = centerMat; break; // Fallback
            }
            catalog.Entries[i] = entry;
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        
        // 3. Force Reimport
        AssetDatabase.ImportAsset("Assets/Rendering/GodgameRenderCatalog.asset", ImportAssetOptions.ForceUpdate);
        
        Debug.Log("FixMaterialsAndCatalogFinal Executed");
    }

    private static void UpdateMaterial(string path, Color color, Shader shader)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }
        mat.SetColor("_BaseColor", color);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
    }
}
#endif
