using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Godgame.Scenario;

public class FinalVerify
{
    public static void Execute()
    {
        Debug.Log("--- Final Verification Started ---");
        
        // 1. Check DemoConfig in Scene
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig GameObject not found in scene!");
            return;
        }

        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring == null)
        {
            Debug.LogError("SettlementAuthoring component missing on DemoConfig!");
            return;
        }

        // 2. Check Prefab Assignments
        CheckAssignment("Villager", authoring.villagerPrefab);
        CheckAssignment("Storehouse", authoring.storehousePrefab);
        CheckAssignment("VillageCenter", authoring.villageCenterPrefab);
        CheckAssignment("Housing", authoring.housingPrefab);
        CheckAssignment("Worship", authoring.worshipPrefab);

        // 3. Check Prefab Contents
        CheckPrefabContents("Assets/Prefabs/Villagers/Villager.prefab", typeof(Godgame.Authoring.VillagerAuthoring));
        CheckPrefabContents("Assets/Prefabs/Buildings/Storehouse.prefab", typeof(Godgame.Authoring.StorehouseAuthoring));
        CheckPrefabContents("Assets/Prefabs/Buildings/VillageCenter.prefab", typeof(Godgame.Villages.VillageAuthoring));
        CheckPrefabContents("Assets/Prefabs/Buildings/Housing.prefab", typeof(Godgame.Authoring.HousingAuthoring));
        CheckPrefabContents("Assets/Prefabs/Buildings/WorshipSite.prefab", typeof(Godgame.Authoring.WorshipAuthoring));

        Debug.Log("--- Final Verification Completed ---");
    }

    private static void CheckAssignment(string name, GameObject prefab)
    {
        if (prefab == null)
            Debug.LogError($"[Assignment] {name} prefab is NULL on DemoConfig!");
        else
            Debug.Log($"[Assignment] {name} assigned: {prefab.name}");
    }

    private static void CheckPrefabContents(string path, System.Type requiredComponent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[Prefab] Could not load asset at {path}");
            return;
        }

        // Check for missing scripts
        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
        if (missing > 0)
        {
            Debug.LogError($"[Prefab] {prefab.name} has {missing} missing scripts!");
        }
        else
        {
            Debug.Log($"[Prefab] {prefab.name} has no missing scripts.");
        }

        // Check for required component
        if (prefab.GetComponent(requiredComponent) == null)
        {
            Debug.LogError($"[Prefab] {prefab.name} is missing required component {requiredComponent.Name}!");
        }
        else
        {
            Debug.Log($"[Prefab] {prefab.name} has {requiredComponent.Name}.");
        }
    }
}