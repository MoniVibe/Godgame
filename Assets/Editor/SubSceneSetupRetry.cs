using UnityEditor;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Godgame.Scenario;

public class SubSceneSetupRetry
{
    public static void Execute()
    {
        string mainScenePath = "Assets/Scenes/TRI_Godgame_Smoke.unity";
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";

        // Open main scene
        var mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // Find or create DemoConfigSubScene holder
        var subSceneGO = GameObject.Find("DemoConfigSubScene");
        if (subSceneGO == null)
        {
            subSceneGO = new GameObject("DemoConfigSubScene");
        }

        var subScene = subSceneGO.GetComponent<SubScene>();
        if (subScene == null)
        {
            subScene = subSceneGO.AddComponent<SubScene>();
        }

        // Create or Load SubScene Asset
        Scene subSceneInstance;
        
        // Check if scene is already loaded
        subSceneInstance = SceneManager.GetSceneByPath(subScenePath);
        if (!subSceneInstance.IsValid())
        {
            // If not loaded, try to load it additively
            if (System.IO.File.Exists(subScenePath))
            {
                subSceneInstance = EditorSceneManager.OpenScene(subScenePath, OpenSceneMode.Additive);
            }
            else
            {
                // Create new if doesn't exist
                subSceneInstance = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(subSceneInstance, subScenePath);
            }
        }

        // Assign to SubScene component
        subScene.SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(subScenePath);
        subScene.AutoLoadScene = true;

        // Find DemoConfig
        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig != null)
        {
            // Ensure it's a root object
            demoConfig.transform.SetParent(null);
            
            // Move to subscene
            if (demoConfig.scene != subSceneInstance)
            {
                SceneManager.MoveGameObjectToScene(demoConfig, subSceneInstance);
                Debug.Log("Moved DemoConfig to SubScene.");
            }
            else
            {
                Debug.Log("DemoConfig is already in SubScene.");
            }
        }
        else
        {
            // If not found in main scene, maybe it's already in the subscene?
            // We can't easily search across scenes with GameObject.Find if it's not active?
            // GameObject.Find searches all active scenes.
            Debug.LogWarning("DemoConfig not found in active scenes. Creating new one in SubScene.");
            
            var newDemoConfig = new GameObject("DemoConfig");
            SceneManager.MoveGameObjectToScene(newDemoConfig, subSceneInstance);
            
            // Add Authoring
            var authoring = newDemoConfig.AddComponent<Godgame.Scenario.SettlementAuthoring>();
            
            // Assign prefabs
            authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
            authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
            authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
            authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
            
            var worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");
            if (worship == null) worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Temple_Basic.prefab");
            authoring.worshipPrefab = worship;
            
            Debug.Log("Created new DemoConfig in SubScene.");
        }

        EditorSceneManager.MarkSceneDirty(subSceneInstance);
        EditorSceneManager.SaveScene(subSceneInstance);
        
        EditorSceneManager.MarkSceneDirty(mainScene);
        EditorSceneManager.SaveScene(mainScene);
        
        Debug.Log("SubScene setup retry complete.");
    }
}