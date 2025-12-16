using UnityEditor;
using UnityEngine;
using Godgame.Rendering.Catalog;
using Godgame.Rendering; // Added namespace

public class SetupRenderCatalogInScene
{
    public static void Execute()
    {
        var go = GameObject.Find("GodgameRenderCatalog");
        if (go == null)
        {
            go = new GameObject("GodgameRenderCatalog");
            Debug.Log("Created GodgameRenderCatalog GameObject.");
        }

        var authoring = go.GetComponent<RenderCatalogAuthoring>();
        if (authoring == null)
        {
            authoring = go.AddComponent<RenderCatalogAuthoring>();
            Debug.Log("Added RenderCatalogAuthoring component.");
        }

        var catalogAsset = AssetDatabase.LoadAssetAtPath<GodgameRenderCatalogDefinition>("Assets/Rendering/GodgameRenderCatalog.asset");
        if (catalogAsset == null)
        {
            Debug.LogError("GodgameRenderCatalog.asset not found!");
            return;
        }

        authoring.CatalogDefinition = catalogAsset;
        EditorUtility.SetDirty(go);
        Debug.Log("Assigned CatalogDefinition to RenderCatalogAuthoring.");
    }
}
