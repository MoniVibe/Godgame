using UnityEditor;
using Unity.Scenes;
using UnityEngine;

public class AssignSubScene : MonoBehaviour
{
    public static void Execute()
    {
        var holder = GameObject.Find("RenderKeyTestSubSceneHolder");
        if (holder == null)
        {
            Debug.LogError("RenderKeyTestSubSceneHolder not found");
            return;
        }

        var subScene = holder.GetComponent<SubScene>();
        if (subScene == null)
        {
            Debug.LogError("SubScene component not found on holder");
            return;
        }

        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/RenderKeyTestSubScene.unity");
        if (sceneAsset == null)
        {
            Debug.LogError("Scene asset not found at Assets/Scenes/RenderKeyTestSubScene.unity");
            return;
        }

        subScene.SceneAsset = sceneAsset;
        Debug.Log("Successfully assigned SceneAsset to SubScene");
    }
}
