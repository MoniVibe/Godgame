using UnityEditor;
using UnityEngine;
using Godgame.Scenario;

public class FixDemoConfigReferences
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig not found in scene.");
            return;
        }

        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring == null)
        {
            Debug.LogError("SettlementAuthoring component not found on DemoConfig.");
            return;
        }

        // Load prefabs
        var villager = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
        var storehouse = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
        var villageCenter = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
        var housing = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
        var worship = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");

        if (villager == null) Debug.LogError("Villager prefab not found!");
        if (storehouse == null) Debug.LogError("Storehouse prefab not found!");
        if (villageCenter == null) Debug.LogError("VillageCenter prefab not found!");
        if (housing == null) Debug.LogError("Housing prefab not found!");
        if (worship == null) Debug.LogError("WorshipSite prefab not found!");

        // Assign
        authoring.villagerPrefab = villager;
        authoring.storehousePrefab = storehouse;
        authoring.villageCenterPrefab = villageCenter;
        authoring.housingPrefab = housing;
        authoring.worshipPrefab = worship;

        EditorUtility.SetDirty(authoring);
        Debug.Log("Re-assigned all prefabs to DemoConfig.");
    }
}