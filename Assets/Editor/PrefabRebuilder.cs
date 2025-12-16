using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PrefabRebuilder
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

        // Try to add component and save first
        bool needsRebuild = false;
        if (contents.GetComponent(authoringType) == null)
        {
            contents.AddComponent(authoringType);
        }

        try
        {
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            Debug.Log($"Saved {path} (no rebuild needed)");
        }
        catch
        {
            Debug.LogWarning($"Failed to save {path}, attempting rebuild...");
            needsRebuild = true;
        }

        if (needsRebuild)
        {
            GameObject cleanCopy = RebuildClean(contents);
            
            // Ensure authoring is on the clean copy
            if (cleanCopy.GetComponent(authoringType) == null)
            {
                cleanCopy.AddComponent(authoringType);
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
        }

        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static GameObject RebuildClean(GameObject source)
    {
        GameObject target = new GameObject(source.name);
        // Copy basic properties
        target.layer = source.layer;
        target.tag = source.tag;
        target.isStatic = source.isStatic;
        // Transform is handled by parent assignment or manual copy if root
        // But new GameObject has Transform.
        
        // Copy Transform properties
        target.transform.localPosition = source.transform.localPosition;
        target.transform.localRotation = source.transform.localRotation;
        target.transform.localScale = source.transform.localScale;

        // Copy components
        foreach (Component comp in source.GetComponents<Component>())
        {
            if (comp != null && !(comp is Transform))
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
            }
        }

        // Recursively rebuild children
        foreach (Transform child in source.transform)
        {
            GameObject newChild = RebuildClean(child.gameObject);
            newChild.transform.SetParent(target.transform, false);
            // Local transform properties are preserved by RebuildClean and SetParent(false)
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
