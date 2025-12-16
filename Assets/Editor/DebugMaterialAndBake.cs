using UnityEngine;
using UnityEditor;

public class DebugMaterialAndBake
{
    public static void Execute()
    {
        // 1. Change Villager_Orange to Blue to distinguish from Error Pink
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/Villager_Orange.mat");
        if (mat != null)
        {
            mat.SetColor("_BaseColor", Color.blue);
            EditorUtility.SetDirty(mat);
            Debug.Log("Set Villager_Orange to Blue");
        }

        // 2. Force reimport of GodgameRenderCatalog.asset
        AssetDatabase.ImportAsset("Assets/Rendering/GodgameRenderCatalog.asset", ImportAssetOptions.ForceUpdate);
        Debug.Log("Reimported GodgameRenderCatalog.asset");

        // 3. Touch the GodgameRenderCatalog GameObject in the scene to force bake
        var go = GameObject.Find("GodgameRenderCatalog");
        if (go != null)
        {
            EditorUtility.SetDirty(go);
            Debug.Log("Set GodgameRenderCatalog GameObject dirty");
        }
        else
        {
            Debug.LogError("GodgameRenderCatalog GameObject not found in scene");
        }
        
        AssetDatabase.SaveAssets();
    }
}
