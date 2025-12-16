using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabDiagnostic
{
    public static void Execute()
    {
        var paths = new List<string>
        {
            "Assets/Prefabs/Buildings/VillageCenter.prefab",
            "Assets/Prefabs/Buildings/Housing.prefab",
            "Assets/Prefabs/Buildings/WorshipSite.prefab",
            "Assets/Prefabs/Buildings/Storehouse.prefab",
            "Assets/Prefabs/Villagers/Villager.prefab"
        };

        foreach (var path in paths)
        {
            DiagnoseAndFix(path);
        }
        
        FixSceneObject("DemoConfig");
    }

    private static void DiagnoseAndFix(string path)
    {
        Debug.Log($"--- Diagnosing {path} ---");
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null) return;

        int missingCount = 0;
        missingCount += CheckMissing(contents);
        foreach (Transform t in contents.GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject != contents)
                missingCount += CheckMissing(t.gameObject);
        }

        Debug.Log($"Total missing scripts detected via API: {missingCount}");

        if (missingCount > 0)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
            foreach (Transform t in contents.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject != contents)
                    removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            Debug.Log($"Removed {removed} missing scripts via API.");
        }

        // Add Authoring if needed
        System.Type authoringType = null;
        if (path.Contains("VillageCenter")) authoringType = typeof(Godgame.Villages.VillageAuthoring);
        else if (path.Contains("Housing")) authoringType = typeof(Godgame.Authoring.HousingAuthoring);
        else if (path.Contains("WorshipSite")) authoringType = typeof(Godgame.Authoring.WorshipAuthoring);
        else if (path.Contains("Storehouse")) authoringType = typeof(Godgame.Authoring.StorehouseAuthoring);
        else if (path.Contains("Villager")) authoringType = typeof(Godgame.Authoring.VillagerAuthoring);

        if (authoringType != null)
        {
            if (contents.GetComponent(authoringType) == null)
            {
                Debug.Log($"Adding {authoringType.Name}");
                contents.AddComponent(authoringType);
            }
            else
            {
                Debug.Log($"{authoringType.Name} already exists.");
            }
        }

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

    private static int CheckMissing(GameObject go)
    {
        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (count > 0)
        {
            Debug.Log($"GameObject '{go.name}' has {count} missing scripts.");
        }
        return count;
    }

    private static void FixSceneObject(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return;

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
