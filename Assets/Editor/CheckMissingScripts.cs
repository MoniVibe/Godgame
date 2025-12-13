using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CheckMissingScripts
{
    public static void Execute()
    {
        string[] prefabPaths = new string[]
        {
            "Assets/Resources/Prefabs/CameraInputRig.prefab",
            "Assets/Prefabs/Systems/GodgamePresentationRuntimeBootstrap.prefab",
            "Assets/Prefabs/Scenes/GodgameDemoRig.prefab"
        };

        foreach (string path in prefabPaths)
        {
            if (File.Exists(path))
            {
                Debug.Log($"Checking prefab: {path}");
                ProcessPrefab(path);
            }
            else
            {
                Debug.LogWarning($"Prefab not found at path: {path}");
            }
        }
        
        // Also search for any prefab with "Debug" or "Cube" in the name in Resources
        string[] allPrefabs = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        foreach (string path in allPrefabs)
        {
            if (path.Contains("Debug") || path.Contains("Cube"))
            {
                 // Normalize path separators
                 string normalizedPath = path.Replace("\\", "/");
                 Debug.Log($"Checking discovered prefab: {normalizedPath}");
                 ProcessPrefab(normalizedPath);
            }
        }
        
        AssetDatabase.SaveAssets();
    }

    private static void ProcessPrefab(string path)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        bool modified = false;

        if (contents != null)
        {
            modified = CheckGameObject(contents, path);
            
            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                Debug.Log($"Saved changes to prefab: {path}");
            }
            
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static bool CheckGameObject(GameObject go, string path)
    {
        bool modified = false;
        
        // Use SerializedObject to remove null components
        SerializedObject so = new SerializedObject(go);
        SerializedProperty prop = so.FindProperty("m_Component");
        
        int r = 0;
        for (int i = 0; i < prop.arraySize; i++)
        {
            SerializedProperty componentProp = prop.GetArrayElementAtIndex(i);
            SerializedProperty componentRef = componentProp.FindPropertyRelative("component");
            
            if (componentRef.objectReferenceValue == null)
            {
                Debug.Log($"Found null component at index {i} on '{go.name}' in '{path}'. Removing...");
                prop.DeleteArrayElementAtIndex(i);
                i--; // Adjust index since we removed an element
                r++;
                modified = true;
            }
        }
        
        if (modified)
        {
            so.ApplyModifiedProperties();
            Debug.Log($"Removed {r} null components from '{go.name}' in {path} via SerializedObject");
        }

        // Also try the standard way just in case
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (count > 0)
        {
            Debug.Log($"Removed {count} missing scripts from '{go.name}' in {path} via GameObjectUtility");
            modified = true;
        }

        if (go.name == "DebugCube")
        {
             Debug.Log($"Found DebugCube in {path}");
        }

        foreach (Transform child in go.transform)
        {
            if (CheckGameObject(child.gameObject, path))
            {
                modified = true;
            }
        }
        
        return modified;
    }
}
