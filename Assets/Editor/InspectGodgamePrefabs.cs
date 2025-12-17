using UnityEditor;
using UnityEngine;

public class InspectGodgamePrefabs
{
    public static void Execute()
    {
        Inspect("Assets/Prefabs/Villagers/Villager.prefab");
        Inspect("Assets/Prefabs/Buildings/VillageCenter.prefab");
        Inspect("Assets/Prefabs/Buildings/Storehouse.prefab");
        Inspect("Assets/Prefabs/Buildings/Housing.prefab"); // For VillageCenter or just Housing? Keys didn't list Housing explicitly, maybe it maps to VillageCenter or Storehouse or generic?
        // Wait, GodgameSemanticKeys has VillageCenter(110) and Storehouse(150). Housing is not listed.
        // Maybe Housing uses VillageCenter key? Or maybe I should add a key for it?
        // The prompt says: "Villager 100, VillageCenter 110, ResourceNode 120, ResourceChunk 130, Vegetation 140".
        // It doesn't mention Storehouse key in that list, but I found Storehouse=150 in the file.
        // I'll assume Housing might use 110 or I should check if there is a Housing key.
        // GodgameSemanticKeys.cs did NOT have Housing.
        
        Inspect("Assets/Prefabs/Ambient/AmbientProp_RockSpire.prefab"); // For ResourceNode?
        Inspect("Assets/Prefabs/Vegetation/VegetationCluster_ForestBroadleaf.prefab"); // For Vegetation?
    }

    private static void Inspect(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null)
        {
            Debug.LogError($"Prefab not found: {path}");
            return;
        }

        var mf = go.GetComponentInChildren<MeshFilter>();
        var mr = go.GetComponentInChildren<MeshRenderer>();

        string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "null";
        string matName = mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.name : "null";
        string matPath = mr != null && mr.sharedMaterial != null ? AssetDatabase.GetAssetPath(mr.sharedMaterial) : "null";
        string meshPath = mf != null && mf.sharedMesh != null ? AssetDatabase.GetAssetPath(mf.sharedMesh) : "null";

        Debug.Log($"Prefab: {go.name}\n  Mesh: {meshName} ({meshPath})\n  Material: {matName} ({matPath})");
    }
}
