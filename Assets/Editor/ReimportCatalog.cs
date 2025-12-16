using UnityEditor;

public class ReimportCatalog
{
    public static void Execute()
    {
        AssetDatabase.ImportAsset("Assets/Rendering/GodgameRenderCatalog.asset", ImportAssetOptions.ForceUpdate);
    }
}
