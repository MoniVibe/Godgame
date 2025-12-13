#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptSweeper
{
    [MenuItem("Tools/PureDOTS/Sweep Missing Scripts (Prefabs + Open Scenes)")]
    public static void SweepAll()
    {
        SweepPrefabs();
        SweepOpenScenes();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MissingScriptSweeper] Done.");
    }

    static void SweepPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab")) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
                if (removed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    fixedCount += removed;
                    Debug.Log($"[MissingScriptSweeper] Removed {removed} missing scripts in prefab: {path}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        if (fixedCount > 0)
            Debug.Log($"[MissingScriptSweeper] Total missing scripts removed from prefabs: {fixedCount}");
    }

    static void SweepOpenScenes()
    {
        int removedTotal = 0;

        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            var scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
                removedTotal += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

            if (removedTotal > 0)
                EditorSceneManager.MarkSceneDirty(scene);
        }

        if (removedTotal > 0)
            Debug.Log($"[MissingScriptSweeper] Removed {removedTotal} missing scripts from open scenes.");
    }
}
#endif
