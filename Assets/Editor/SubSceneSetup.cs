using UnityEditor;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SubSceneSetup
{
    public static void Execute()
    {
        var subSceneGO = GameObject.Find("DemoConfigSubScene");
        if (subSceneGO == null)
        {
            Debug.LogError("DemoConfigSubScene not found.");
            return;
        }

        var subScene = subSceneGO.GetComponent<SubScene>();
        if (subScene == null)
        {
            Debug.LogError("SubScene component not found on DemoConfigSubScene.");
            return;
        }

        // Create a new scene asset for the subscene
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SaveScene(scene, subScenePath);
        
        // Assign the scene asset to the SubScene component
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(subScenePath);
        subScene.SceneAsset = sceneAsset;
        subScene.AutoLoadScene = true;

        // Move DemoConfig to the new scene
        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig != null)
        {
            // Unparent first to ensure it's a root object
            demoConfig.transform.SetParent(null);
            
            // Move DemoConfig to the newly created scene
            SceneManager.MoveGameObjectToScene(demoConfig, scene);
        }
        else
        {
            Debug.LogError("DemoConfig not found to move.");
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); // Save main scene
        
        Debug.Log("SubScene setup complete.");
    }
}
