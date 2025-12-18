using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Godgame.Scenario;

public class RecreateDemoConfigComponent
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig not found.");
            return;
        }

        var oldComp = go.GetComponent<SettlementAuthoring>();
        if (oldComp == null)
        {
            Debug.LogError("SettlementAuthoring not found.");
            return;
        }

        // 1. Backup values
        int initialVillagers = oldComp.initialVillagers;
        float villagerSpawnRadius = oldComp.villagerSpawnRadius;
        float buildingRingRadius = oldComp.buildingRingRadius;
        float resourceRingRadius = oldComp.resourceRingRadius;
        uint randomSeed = oldComp.randomSeed;

        // 2. Remove component
        Object.DestroyImmediate(oldComp);

        // 3. Add new component
        var newComp = go.AddComponent<SettlementAuthoring>();

        // 4. Restore values
        newComp.initialVillagers = initialVillagers;
        newComp.villagerSpawnRadius = villagerSpawnRadius;
        newComp.buildingRingRadius = buildingRingRadius;
        newComp.resourceRingRadius = resourceRingRadius;
        newComp.randomSeed = randomSeed;

        // 5. Assign Prefabs (Fresh load)
        newComp.villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Villagers/Villager.prefab");
        newComp.storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Storehouse.prefab");
        newComp.villageCenterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/VillageCenter.prefab");
        newComp.housingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Housing.prefab");
        newComp.worshipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/WorshipSite.prefab");

        // Verify assignments
        if (newComp.villageCenterPrefab == null) Debug.LogError("Failed to load VillageCenter prefab!");
        else Debug.Log("VillageCenter assigned successfully.");

        // 6. Save
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);
        EditorSceneManager.SaveScene(go.scene);
        
        Debug.Log("Recreated SettlementAuthoring component and saved scene.");
    }
}
