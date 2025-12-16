using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SaveSubScene
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetSceneByPath("Assets/Scenes/DemoConfigSubScene.unity");
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Saved DemoConfigSubScene.unity");
        }
        else
        {
            Debug.LogError("Could not find scene Assets/Scenes/DemoConfigSubScene.unity");
        }
    }
}
