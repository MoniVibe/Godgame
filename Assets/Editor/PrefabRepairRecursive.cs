using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabRepairRecursive
{
    public static void Execute()
    {
        var prefabMap = new Dictionary<string, System.Type>
        {
            { "Assets/Prefabs/Buildings/VillageCenter.prefab", typeof(Godgame.Villages.VillageAuthoring) },
            { "Assets/Prefabs/Buildings/Housing.prefab", typeof(Godgame.Authoring.HousingAuthoring) },
            { "Assets/Prefabs/Buildings/WorshipSite.prefab", typeof(Godgame.Authoring.WorshipAuthoring) },
            { "Assets/Prefabs/Buildings/Storehouse.prefab", typeof(Godgame.Authoring.StorehouseAuthoring) },
            { "Assets/Prefabs/Villagers/Villager.prefab", typeof(Godgame.Authoring.VillagerAuthoring) }
        };

        foreach (var kvp in prefabMap)
        {
            RepairPrefab(kvp.Key, kvp.Value);
        }

        // Fix scene object
        FixSceneObject("DemoConfig");
    }

    private static void RepairPrefab(string path, System.Type authoringType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null)
        {
            Debug.LogError($"Could not load prefab contents at {path}");
            return;
        }

        // 1. Remove missing scripts recursively
        int removedTotal = RemoveMissingScriptsRecursively(contents);
        if (removedTotal > 0)
        {
            Debug.Log($"Removed {removedTotal} missing scripts from {path}");
        }

        // 2. Add correct authoring component if missing
        if (contents.GetComponent(authoringType) == null)
        {
            Debug.Log($"Adding {authoringType.Name} to {path}");
            contents.AddComponent(authoringType);
        }
        else
        {
            Debug.Log($"{authoringType.Name} already present on {path}");
        }

        // 3. Save
        try
        {
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            Debug.Log($"Saved {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save {path}: {e.Message}");
        }
        
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static int RemoveMissingScriptsRecursively(GameObject go)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
        {
            count += RemoveMissingScriptsRecursively(child.gameObject);
        }
        return count;
    }

    private static void FixSceneObject(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            Debug.LogWarning($"GameObject {name} not found in scene.");
            return;
        }

        var authoring = go.GetComponent<Godgame.Demo.DemoSettlementAuthoring>();
        if (authoring != null)
        {
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
