using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabRepairManual
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

        // 1. Remove missing scripts manually using SerializedObject
        int removed = RemoveMissingScriptsRecursively(contents);
        if (removed > 0)
        {
            Debug.Log($"Manually removed {removed} missing scripts from {path}");
        }
        else
        {
            // Try the utility as well
            int utilRemoved = 0;
            utilRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
            foreach(Transform t in contents.GetComponentsInChildren<Transform>(true))
            {
                utilRemoved += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            if (utilRemoved > 0) Debug.Log($"Utility removed {utilRemoved} missing scripts from {path}");
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
        int count = 0;
        
        // Use SerializedObject to inspect components
        SerializedObject so = new SerializedObject(go);
        SerializedProperty prop = so.FindProperty("m_Component");
        
        // Iterate backwards to remove
        for (int i = prop.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty element = prop.GetArrayElementAtIndex(i);
            SerializedProperty componentProp = element.FindPropertyRelative("component");
            
            if (componentProp.objectReferenceValue == null)
            {
                // This is a missing script/component
                prop.DeleteArrayElementAtIndex(i);
                count++;
            }
            else if (componentProp.objectReferenceValue is MonoBehaviour mb)
            {
                // Check if the script reference itself is missing (MonoBehaviour exists but script is null)
                // This is harder to detect via SerializedObject of GameObject.
                // But usually 'component' being null covers the "Missing Script" case in Inspector.
                
                if (mb == null) // Should be covered above
                {
                     prop.DeleteArrayElementAtIndex(i);
                     count++;
                }
            }
        }
        
        if (count > 0)
        {
            so.ApplyModifiedProperties();
        }

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
