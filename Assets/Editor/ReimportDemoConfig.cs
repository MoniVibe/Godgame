using UnityEditor;

public class ReimportDemoConfig
{
    public static void Execute()
    {
        AssetDatabase.ImportAsset("Assets/Scenes/DemoConfigSubScene.unity", ImportAssetOptions.ForceUpdate);
    }
}
