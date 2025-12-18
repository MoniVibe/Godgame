using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Godgame.Scenario;

public class FixGodgamePrefabs
{
    public static void Execute()
    {
        // Map of Prefab Path -> Authoring Component Type
        var prefabMap = new Dictionary<string, Type>
        {
            { "Assets/Prefabs/Villagers/Villager.prefab", typeof(Godgame.Authoring.VillagerAuthoring) },
            { "Assets/Prefabs/Buildings/VillageCenter.prefab", typeof(Godgame.Villages.VillageAuthoring) },
            { "Assets/Prefabs/Buildings/Housing.prefab", typeof(Godgame.Authoring.HousingAuthoring) },
            { "Assets/Prefabs/Buildings/Storehouse.prefab", typeof(Godgame.Authoring.StorehouseAuthoring) },
            { "Assets/Prefabs/Buildings/WorshipSite.prefab", typeof(Godgame.Authoring.WorshipAuthoring) }
        };

        foreach (var kvp in prefabMap)
        {
            FixPrefab(kvp.Key, kvp.Value);
        }

        // Reassign to DemoConfig
        ReassignToDemoConfig();
    }

    private static void FixPrefab(string path, Type authoringType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null)
        {
            Debug.LogError($"Could not load prefab at {path}");
            return;
        }

        bool modified = false;

        // 1. Remove missing scripts
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} missing scripts from {path}");
            modified = true;
        }

        // 2. Ensure Authoring component exists
        if (contents.GetComponent(authoringType) == null)
        {
            Debug.Log($"Adding {authoringType.Name} to {path}");
            contents.AddComponent(authoringType);
            modified = true;
        }
        else
        {
            Debug.Log($"{authoringType.Name} already exists on {path}");
        }

        if (modified)
        {
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            Debug.Log($"Saved changes to {path}");
        }
        
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static void ReassignToDemoConfig()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig GameObject not found in current scene.");
            return;
        }

        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring == null)
        {
            Debug.LogError("SettlementAuthoring component not found on DemoConfig.");
            return;
        }

        authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
        authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
        authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
        authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
        authoring.worshipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");

        EditorUtility.SetDirty(authoring);
        Debug.Log("Reassigned prefabs to DemoConfig.");
    }
}