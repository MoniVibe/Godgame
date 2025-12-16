using UnityEditor;

public class ReimportMaterialsAndCatalog
{
    public static void Execute()
    {
        AssetDatabase.ImportAsset("Assets/Materials/Godgame/Villager_Orange.mat", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Materials/Godgame/VillageCenter_Gray.mat", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Materials/Godgame/ResourceNode_Cyan.mat", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Materials/Godgame/ResourceChunk_Gold.mat", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Materials/Godgame/Vegetation_Green.mat", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Rendering/GodgameRenderCatalog.asset", ImportAssetOptions.ForceUpdate);
    }
}
