using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class CleanupDemoConfig
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig not found");
            return;
        }

        // Check for missing scripts on the GameObject itself
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} missing scripts from DemoConfig");
        }
        else
        {
            Debug.Log("No missing scripts on DemoConfig");
        }

        // Verify prefabs assigned to DemoSettlementAuthoring
        var authoring = go.GetComponent<Godgame.Demo.DemoSettlementAuthoring>();
        if (authoring != null)
        {
            CheckPrefab("Villager", authoring.villagerPrefab);
            CheckPrefab("Storehouse", authoring.storehousePrefab);
            CheckPrefab("VillageCenter", authoring.villageCenterPrefab);
            CheckPrefab("Housing", authoring.housingPrefab);
            CheckPrefab("Worship", authoring.worshipPrefab);
        }
        
        EditorUtility.SetDirty(go);
    }

    private static void CheckPrefab(string name, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"{name} prefab is NULL");
            return;
        }

        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
        if (missing > 0)
        {
            Debug.LogError($"{name} prefab ({prefab.name}) has {missing} missing scripts!");
        }
        else
        {
            Debug.Log($"{name} prefab ({prefab.name}) is clean.");
        }
    }
}
