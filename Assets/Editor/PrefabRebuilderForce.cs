using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabRebuilderForce
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
            RebuildPrefab(kvp.Key, kvp.Value);
        }

        FixSceneObject("DemoConfig");
    }

    private static void RebuildPrefab(string path, System.Type authoringType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents == null)
        {
            Debug.LogError($"Could not load prefab contents at {path}");
            return;
        }

        // Force rebuild to strip missing scripts
        GameObject cleanCopy = RebuildClean(contents);
        
        // Ensure authoring is on the clean copy
        if (cleanCopy.GetComponent(authoringType) == null)
        {
            cleanCopy.AddComponent(authoringType);
            Debug.Log($"Added {authoringType.Name} to rebuilt {path}");
        }
        else
        {
            Debug.Log($"{authoringType.Name} preserved on rebuilt {path}");
        }

        try
        {
            PrefabUtility.SaveAsPrefabAsset(cleanCopy, path);
            Debug.Log($"Rebuilt and saved {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save rebuilt {path}: {e.Message}");
        }
        
        Object.DestroyImmediate(cleanCopy);
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static GameObject RebuildClean(GameObject source)
    {
        GameObject target = new GameObject(source.name);
        target.layer = source.layer;
        target.tag = source.tag;
        target.isStatic = source.isStatic;
        
        target.transform.localPosition = source.transform.localPosition;
        target.transform.localRotation = source.transform.localRotation;
        target.transform.localScale = source.transform.localScale;

        foreach (Component comp in source.GetComponents<Component>())
        {
            if (comp != null && !(comp is Transform))
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
            }
        }

        foreach (Transform child in source.transform)
        {
            GameObject newChild = RebuildClean(child.gameObject);
            newChild.transform.SetParent(target.transform, false);
        }

        return target;
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
