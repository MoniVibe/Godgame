using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PureDOTS.Authoring;
using Godgame.Scenario;

public class GodgameFixer
{
    [MenuItem("Godgame/Fix Storehouse Prefab")]
    public static void FixStorehouse()
    {
        string path = "Assets/Prefabs/Buildings/Storehouse_Basic.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at {path}");
            return;
        }

        // We need to instantiate to modify and save back, or use PrefabUtility
        // But RemoveMonoBehavioursWithMissingScript works on GameObject
        // For assets, we might need to open it.
        
        // Actually, GameObjectUtility.RemoveMonoBehavioursWithMissingScript works on the prefab asset directly in recent Unity versions?
        // Or we can use PrefabUtility.LoadPrefabContents
        
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(contents);
        if (count > 0)
        {
            Debug.Log($"Removed {count} missing scripts from {path}");
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        else
        {
            Debug.Log($"No missing scripts found in {path}");
        }
        PrefabUtility.UnloadPrefabContents(contents);
    }

    [MenuItem("Godgame/Populate Resource Types")]
    public static void PopulateResourceTypes()
    {
        string path = "Assets/Godgame/Config/PureDotsResourceTypes.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<ResourceTypeCatalog>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ResourceTypeCatalog>();
            AssetDatabase.CreateAsset(catalog, path);
        }

        var serialized = new SerializedObject(catalog);
        var entriesProp = serialized.FindProperty("entries");
        
        // Ensure wood, stone, food exist
        EnsureResource(entriesProp, "wood", new Color(0.643f, 0.404f, 0.247f));
        EnsureResource(entriesProp, "stone", new Color(0.5f, 0.5f, 0.5f));
        EnsureResource(entriesProp, "food", new Color(0.941f, 0.745f, 0.196f));

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(catalog);
        Debug.Log("Populated ResourceTypes");
    }

    private static void EnsureResource(SerializedProperty entries, string id, Color color)
    {
        for (int i = 0; i < entries.arraySize; i++)
        {
            var element = entries.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("id").stringValue == id)
            {
                return; // Already exists
            }
        }

        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);
        var newElement = entries.GetArrayElementAtIndex(index);
        newElement.FindPropertyRelative("id").stringValue = id;
        newElement.FindPropertyRelative("displayColor").colorValue = color;
    }

    [MenuItem("Godgame/Setup Demo Scenario")]
    public static void SetupDemoScenario()
    {
        // 1. Copy villager_loop_small.json to Demo_01.json if not exists
        string sourcePath = "Assets/Scenarios/Godgame/villager_loop_small.json";
        string destPath = "Assets/Scenarios/Godgame/Demo_01.json";
        
        if (System.IO.File.Exists(sourcePath) && !System.IO.File.Exists(destPath))
        {
            AssetDatabase.CopyAsset(sourcePath, destPath);
            Debug.Log("Created Demo_01.json");
        }

        // 2. Update DemoBootstrap in Godgame_DemoScene.unity
        // We can't easily modify scene file via script without opening it.
        // But we can try to find the prefab if it's a prefab.
        // It seems DemoBootstrap is a scene object.
        // We will leave this to manual or scene open.
    }
}
