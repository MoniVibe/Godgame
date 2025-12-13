using UnityEditor;
using UnityEngine;

public class ReimportConfigAssets
{
    public static void Execute()
    {
        string[] paths = new string[]
        {
            "Assets/Godgame/Config/PureDotsResourceTypes.asset",
            "Assets/Godgame/Config/PureDotsRuntimeConfig.asset",
            "Assets/Scenes/GodgameConfig.unity"
        };

        foreach (string path in paths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Reimported {path}");
        }
    }
}
