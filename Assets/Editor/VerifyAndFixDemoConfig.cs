using UnityEditor;
using UnityEngine;
using Godgame.Scenario;

public class VerifyAndFixDemoConfig
{
    public static void Execute()
    {
        var go = GameObject.Find("DemoConfig");
        if (go == null)
        {
            Debug.LogError("DemoConfig not found");
            return;
        }

        // 1. Check for missing scripts on DemoConfig
        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (missing > 0)
        {
            Debug.LogWarning($"DemoConfig has {missing} missing scripts. Removing...");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }

        // 2. Verify Authoring
        var authoring = go.GetComponent<Godgame.Scenario.SettlementAuthoring>();
        if (authoring == null)
        {
            Debug.LogError("SettlementAuthoring missing on DemoConfig");
            return;
        }

        // 3. Verify and Fix Prefabs
        authoring.villagerPrefab = VerifyPrefab("Assets/Prefabs/Villagers/Villager.prefab", authoring.villagerPrefab);
        authoring.storehousePrefab = VerifyPrefab("Assets/Prefabs/Buildings/Storehouse.prefab", authoring.storehousePrefab);
        authoring.villageCenterPrefab = VerifyPrefab("Assets/Prefabs/Buildings/VillageCenter.prefab", authoring.villageCenterPrefab);
        authoring.housingPrefab = VerifyPrefab("Assets/Prefabs/Buildings/Housing.prefab", authoring.housingPrefab);
        authoring.worshipPrefab = VerifyPrefab("Assets/Prefabs/Buildings/WorshipSite.prefab", authoring.worshipPrefab);

        EditorUtility.SetDirty(authoring);
        Debug.Log("Verification and Fix complete.");
    }

    private static GameObject VerifyPrefab(string path, GameObject current)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at {path}");
            return current;
        }

        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
        if (missing > 0)
        {
            Debug.LogWarning($"Prefab {path} has {missing} missing scripts. Please run PrefabRepair.");
            // We can try to fix it here too if needed, but let's report first.
        }
        else
        {
            Debug.Log($"Prefab {path} is clean.");
        }

        if (current != prefab)
        {
            Debug.Log($"Reassigning {path} to Authoring.");
            return prefab;
        }
        
        return current;
    }
}