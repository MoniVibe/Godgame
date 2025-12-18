using UnityEditor;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Godgame.Scenario;

public class SubSceneFinalSetup
{
    public static void Execute()
    {
        string mainScenePath = "Assets/Scenes/TRI_Godgame_Smoke.unity";
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";

        var mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
        
        // Ensure SubScene GO exists
        var subSceneGO = GameObject.Find("DemoConfigSubScene");
        if (subSceneGO == null)
        {
            subSceneGO = new GameObject("DemoConfigSubScene");
            subSceneGO.AddComponent<SubScene>();
        }
        var subSceneComp = subSceneGO.GetComponent<SubScene>();
        subSceneComp.SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(subScenePath);
        subSceneComp.AutoLoadScene = true;

        // Open SubScene additively to move object into it
        var subSceneInstance = EditorSceneManager.OpenScene(subScenePath, OpenSceneMode.Additive);

        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig == null)
        {
            demoConfig = new GameObject("DemoConfig");
            demoConfig.AddComponent<Godgame.Scenario.SettlementAuthoring>();
        }

        if (demoConfig.scene != subSceneInstance)
        {
            SceneManager.MoveGameObjectToScene(demoConfig, subSceneInstance);
            Debug.Log("Moved DemoConfig to SubScene.");
        }

        // Re-assign prefabs to ensure they are valid
        var authoring = demoConfig.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
        authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
        authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
        authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
        
        var worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");
        if (worship == null) worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Temple_Basic.prefab");
        authoring.worshipPrefab = worship;

        Debug.Log($"Re-assigned prefabs. VillageCenter: {authoring.villageCenterPrefab}");

        EditorUtility.SetDirty(authoring);
        EditorSceneManager.MarkSceneDirty(subSceneInstance);
        EditorSceneManager.SaveScene(subSceneInstance);
        EditorSceneManager.SaveScene(mainScene);

        // Close the SubScene to force baking?
        // Actually, we can't easily close it via API in a way that persists state for Play Mode exactly like UI?
        // But usually, if it's a SubScene, it will be converted.
        
        // Let's try to close it.
        EditorSceneManager.CloseScene(subSceneInstance, true);
        
        Debug.Log("SubScene setup and closed.");
    }
}