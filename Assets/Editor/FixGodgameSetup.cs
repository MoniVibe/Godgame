using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Scenes;
using System.Linq;

public class FixGodgameSetup
{
    public static void Execute()
    {
        // 1. Fix TRI_Godgame_Smoke.unity
        string smokeScenePath = "Assets/Scenes/TRI_Godgame_Smoke.unity";
        Scene smokeScene = EditorSceneManager.OpenScene(smokeScenePath, OpenSceneMode.Single);
        
        var demoLoaderSmoke = GameObject.Find("DemoSubSceneLoader");
        if (demoLoaderSmoke != null)
        {
            GameObject.DestroyImmediate(demoLoaderSmoke);
            Debug.Log("Removed DemoSubSceneLoader from TRI_Godgame_Smoke.unity");
            EditorSceneManager.MarkSceneDirty(smokeScene);
            EditorSceneManager.SaveScene(smokeScene);
        }

        // 2. Fix Godgame.unity
        string godgameScenePath = "Assets/Godgame.unity";
        Scene godgameScene = EditorSceneManager.OpenScene(godgameScenePath, OpenSceneMode.Single);

        var demoLoaderGodgame = GameObject.Find("DemoSubSceneLoader");
        if (demoLoaderGodgame != null)
        {
            GameObject.DestroyImmediate(demoLoaderGodgame);
            Debug.Log("Removed DemoSubSceneLoader from Godgame.unity");
            EditorSceneManager.MarkSceneDirty(godgameScene);
            EditorSceneManager.SaveScene(godgameScene);
        }

        // 3. Fix DemoConfigSubScene.unity
        string subScenePath = "Assets/Scenes/DemoConfigSubScene.unity";
        Scene subScene = EditorSceneManager.OpenScene(subScenePath, OpenSceneMode.Single);

        // Fix RenderCatalog
        var renderCatalog = GameObject.Find("GodgameRenderCatalog");
        if (renderCatalog == null)
        {
            renderCatalog = GameObject.Find("RenderCatalog");
        }

        if (renderCatalog == null)
        {
            renderCatalog = new GameObject("GodgameRenderCatalog");
            Debug.Log("Created GodgameRenderCatalog in DemoConfigSubScene.unity");
        }

        // Remove missing scripts from RenderCatalog if any
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(renderCatalog);

        // Add RenderCatalogAuthoring
        // We need to find the type by name because we don't have direct reference
        var authoringType = System.Type.GetType("Godgame.Rendering.Catalog.RenderCatalogAuthoring, Assembly-CSharp");
        if (authoringType == null)
        {
             // Try to find in all assemblies
             foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
             {
                 authoringType = asm.GetType("Godgame.Rendering.Catalog.RenderCatalogAuthoring");
                 if (authoringType != null) break;
             }
        }

        if (authoringType != null)
        {
            var authoring = renderCatalog.GetComponent(authoringType);
            if (authoring == null)
            {
                authoring = renderCatalog.AddComponent(authoringType);
                Debug.Log("Added RenderCatalogAuthoring to GodgameRenderCatalog");
            }

            // Assign Catalog Definition
            var catalogAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Rendering/GodgameRenderCatalog.asset");
            if (catalogAsset != null)
            {
                var field = authoringType.GetField("CatalogDefinition");
                if (field != null)
                {
                    field.SetValue(authoring, catalogAsset);
                    Debug.Log("Assigned GodgameRenderCatalog.asset to RenderCatalogAuthoring");
                }
                else
                {
                    Debug.LogError("Could not find CatalogDefinition field on RenderCatalogAuthoring");
                }
            }
            else
            {
                Debug.LogError("Could not load Assets/Rendering/GodgameRenderCatalog.asset");
            }
        }
        else
        {
            Debug.LogError("Could not find RenderCatalogAuthoring type");
        }

        // Fix DemoConfig missing script
        var demoConfig = GameObject.Find("DemoConfig");
        if (demoConfig != null)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(demoConfig);
            Debug.Log("Removed missing scripts from DemoConfig");
        }

        EditorSceneManager.MarkSceneDirty(subScene);
        EditorSceneManager.SaveScene(subScene);

        Debug.Log("FixGodgameSetup Completed");
    }
}
