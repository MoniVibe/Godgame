using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabAuthoringFixer
{
    public static void Execute()
    {
        // Dictionary mapping prefab path to the Authoring component type it needs
        var prefabAuthoringMap = new Dictionary<string, System.Type>
        {
            { "Assets/Prefabs/Buildings/VillageCenter.prefab", typeof(Godgame.Villages.VillageAuthoring) },
            { "Assets/Prefabs/Buildings/Housing.prefab", typeof(Godgame.Authoring.HousingAuthoring) },
            { "Assets/Prefabs/Buildings/WorshipSite.prefab", typeof(Godgame.Authoring.WorshipAuthoring) },
            { "Assets/Prefabs/Buildings/Storehouse.prefab", typeof(Godgame.Authoring.StorehouseAuthoring) },
            { "Assets/Prefabs/Villagers/Villager.prefab", typeof(Godgame.Authoring.VillagerAuthoring) }
        };

        foreach (var kvp in prefabAuthoringMap)
        {
            FixPrefab(kvp.Key, kvp.Value);
        }
        
        // Also fix Settlement_A and Settlement_B in the current scene (DemoConfigSubScene)
        // Wait, I renamed Settlement_A to DemoConfig and deleted Settlement_B.
        // The user prompt says "For each authoring prefab in that scene... Settlement_A/B".
        // I should check if I need to restore Settlement_B or if DemoConfig is enough.
        // Given the previous error about singleton, I should stick with one config.
        // But I should ensure DemoConfig has the correct references.
        
        FixSceneObject("DemoConfig");
    }

    private static void FixPrefab(string path, System.Type authoringType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null)
        {
            Debug.LogError($"Could not load prefab contents at {path}");
            return;
        }

        // Check if component exists
        var component = contents.GetComponent(authoringType);
        if (component == null)
        {
            Debug.Log($"Adding {authoringType.Name} to {path}");
            contents.AddComponent(authoringType);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        else
        {
            Debug.Log($"{authoringType.Name} already exists on {path}");
        }
        PrefabUtility.UnloadPrefabContents(contents);
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
            // Re-assign prefabs to ensure they are valid references
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
