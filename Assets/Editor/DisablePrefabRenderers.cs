using UnityEngine;
using UnityEditor;

public class DisablePrefabRenderers
{
    public static void Execute()
    {
        DisableRenderer("Assets/Prefabs/Villagers/Villager.prefab");
        DisableRenderer("Assets/Prefabs/Buildings/VillageCenter.prefab");
        DisableRenderer("Assets/Prefabs/Buildings/Storehouse.prefab");
        // Rocks are spawned by system, so no prefab to modify for them (unless they use one, but system creates entity directly)
    }

    private static void DisableRenderer(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"Prefab not found: {path}");
            return;
        }

        var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = false;
        }
        
        // Also disable MeshFilter to be sure? 
        // Actually, if MeshRenderer is disabled, it shouldn't render.
        // But baking might still pick it up?
        // Let's remove the components if possible, or just disable.
        // Disabling is safer.
        
        EditorUtility.SetDirty(prefab);
        Debug.Log($"Disabled renderers on {path}");
    }
}
