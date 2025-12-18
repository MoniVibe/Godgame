using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Godgame.Scenario;

public class PrefabFixer
{
    public static void Execute()
    {
        List<string> prefabsToFix = new List<string>
        {
            "Assets/Prefabs/Buildings/VillageCenter.prefab",
            "Assets/Prefabs/Buildings/Housing.prefab",
            "Assets/Prefabs/Buildings/WorshipSite.prefab",
            "Assets/Prefabs/Buildings/Storehouse.prefab",
            "Assets/Prefabs/Villagers/Villager.prefab"
        };

        foreach (string path in prefabsToFix)
        {
            FixPrefab(path);
        }
        
        // Also fix Settlement_A and Settlement_B in the current scene
        FixSceneObject("Settlement_A");
        FixSceneObject("Settlement_B");
    }

    private static void FixPrefab(string path)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null)
        {
            Debug.LogError($"Could not load prefab contents at {path}");
            return;
        }

        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} missing scripts from {path}");
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        else
        {
            Debug.Log($"No missing scripts found in {path}");
        }
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static void FixSceneObject(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogWarning($"GameObject {name} not found in scene.");
            return;
        }

        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring != null)
        {
            // Re-assign prefabs to ensure they are valid references
            authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
            authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
            authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
            authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
            
            var worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");
            if (worship == null) worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Temple_Basic.prefab");
            authoring.worshipPrefab = worship;

            EditorUtility.SetDirty(authoring);
            Debug.Log($"Re-assigned prefabs for {name}");
        }
    }
}