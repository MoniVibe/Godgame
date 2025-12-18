using UnityEditor;
using UnityEngine;
using Godgame.Scenario;

public class FixDemoConfigDirectly
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig not found in open scene.");
            return;
        }

        // 1. Remove missing scripts
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} missing scripts from DemoConfig.");
        }

        // 2. Re-assign prefabs
        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring != null)
        {
            authoring.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
            authoring.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
            authoring.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
            authoring.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
            authoring.worshipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");

            EditorUtility.SetDirty(authoring);
            Debug.Log("Re-assigned prefabs to DemoConfig.");
        }
        else
        {
            Debug.LogError("SettlementAuthoring component missing on DemoConfig.");
        }
    }
}