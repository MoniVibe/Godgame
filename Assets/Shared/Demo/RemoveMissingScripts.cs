#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu action to strip missing MonoBehaviours from the active scene.
/// </summary>
public static class RemoveMissingScripts
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts In Scene")]
    public static void Scene()
    {
        int removed = 0;
        for (int si = 0; si < SceneManager.sceneCount; si++)
        {
            var scene = SceneManager.GetSceneAt(si);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                removed += Recurse(root);
            }
        }
        Debug.Log($"[Cleanup] Removed {removed} missing scripts from scene.");
    }

    private static int Recurse(GameObject go)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
        {
            count += Recurse(child.gameObject);
        }
        return count;
    }
}
#endif
