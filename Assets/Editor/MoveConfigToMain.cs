using UnityEditor;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class MoveConfigToMain
{
    public static void Execute()
    {
        string mainScenePath = "Assets/Scenes/TRI_Godgame_Smoke.unity";
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";

        var mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);
        var subSceneInstance = EditorSceneManager.OpenScene(subScenePath, OpenSceneMode.Additive);

        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig != null)
        {
            SceneManager.MoveGameObjectToScene(demoConfig, mainScene);
            Debug.Log("Moved DemoConfig to Main Scene.");
        }
        else
        {
            Debug.LogError("DemoConfig not found.");
        }

        EditorSceneManager.SaveScene(subSceneInstance);
        EditorSceneManager.SaveScene(mainScene);
    }
}
