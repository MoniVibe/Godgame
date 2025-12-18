using UnityEditor;
using UnityEngine;
using Godgame.Scenario;

public class DemoConfigSetup
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            go = new GameObject("DemoConfig");
        }

        var authoring = go.GetComponent<SettlementAuthoring>();
        if (authoring == null)
        {
            authoring = go.AddComponent<SettlementAuthoring>();
        }

        // Assign prefabs
        authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
        authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
        authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
        authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
        
        // Try to find worship prefab
        authoring.worshipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");
        if (authoring.worshipPrefab == null)
             authoring.worshipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Temple_Basic.prefab");

        Debug.Log($"Setup SettlementAuthoring on {go.name}. VillagerPrefab: {authoring.villagerPrefab}");
    }
}
