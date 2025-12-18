using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Godgame.Scenario;

public class ValidateSetup
{
    public static void Execute()
    {
        var prefabs = new List<string>
        {
            "Assets/Prefabs/Buildings/VillageCenter.prefab",
            "Assets/Prefabs/Buildings/Housing.prefab",
            "Assets/Prefabs/Buildings/WorshipSite.prefab",
            "Assets/Prefabs/Buildings/Storehouse.prefab",
            "Assets/Prefabs/Villagers/Villager.prefab"
        };

        foreach (var path in prefabs)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                Debug.LogError($"Prefab not found: {path}");
                continue;
            }

            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missingCount > 0)
            {
                Debug.LogError($"Prefab {path} has {missingCount} missing scripts!");
            }
            else
            {
                Debug.Log($"Prefab {path} is clean.");
            }
        }

        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig != null)
        {
            var authoring = demoConfig.GetComponent<Godgame.Scenario.SettlementAuthoring>();
            if (authoring != null)
            {
                Debug.Log($"DemoConfig found. Checking assignments...");
                CheckAssignment("VillageCenter", authoring.villageCenterPrefab);
                CheckAssignment("Storehouse", authoring.storehousePrefab);
                CheckAssignment("Housing", authoring.housingPrefab);
                CheckAssignment("Worship", authoring.worshipPrefab);
                CheckAssignment("Villager", authoring.villagerPrefab);
            }
            else
            {
                Debug.LogError("DemoConfig missing SettlementAuthoring component.");
            }
        }
        else
        {
            Debug.LogError("DemoConfig GameObject not found in scene.");
        }
    }

    private static void CheckAssignment(string name, GameObject prefab)
    {
        if (prefab == null)
            Debug.LogError($"{name} prefab is NULL on DemoConfig!");
        else
            Debug.Log($"{name} assigned: {prefab.name}");
    }
}