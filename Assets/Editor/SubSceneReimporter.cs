using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class SubSceneReimporter
{
    public static void Execute()
    {
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";
        
        // Force reimport of the subscene asset to trigger baking
        AssetDatabase.ImportAsset(subScenePath, ImportAssetOptions.ForceUpdate);
        
        Debug.Log("Reimported DemoConfigSubScene.unity");
    }
}
