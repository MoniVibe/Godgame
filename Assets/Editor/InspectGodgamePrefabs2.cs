using UnityEditor;
using UnityEngine;

public class InspectGodgamePrefabs2
{
    public static void Execute()
    {
        Inspect("Assets/Prefabs/Villagers/Visuals/VillagerView.prefab");
        Inspect("Assets/Prefabs/Buildings/Storehouse_Basic.prefab");
        Inspect("Assets/Prefabs/Buildings/Temple_Basic.prefab");
        Inspect("Assets/Prefabs/Buildings/Fertility_Statue.prefab");
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
        
        Debug.Log($"Prefab: {go.name}\n  Mesh: {meshName}\n  Material: {matName}");
    }
}
