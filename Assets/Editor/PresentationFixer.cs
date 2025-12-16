using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using PureDOTS.Runtime.Resource;
using PureDOTS.Authoring;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class PresentationFixer
{
    public static void Execute()
    {
        FixScene("Assets/Godgame.unity");
        FixScene("Assets/Scenes/TRI_Godgame_Smoke.unity");
    }

    private static void FixScene(string scenePath)
    {
        Debug.Log($"Fixing scene: {scenePath}");
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Fix PureDotsRuntimeConfigLoader
        var configLoaderGO = GameObject.Find("PureDotsRuntimeConfigLoader");
        if (configLoaderGO != null)
        {
            var loader = configLoaderGO.GetComponent<PureDotsRuntimeConfigLoader>();
            if (loader == null)
            {
                loader = configLoaderGO.AddComponent<PureDotsRuntimeConfigLoader>();
                Debug.Log("Added PureDotsRuntimeConfigLoader component.");
            }

            if (loader.config == null)
            {
                string configPath = "Assets/Godgame/Config/PureDotsRuntimeConfig.asset";
                var config = AssetDatabase.LoadAssetAtPath<PureDotsRuntimeConfig>(configPath);
                if (config != null)
                {
                    loader.config = config;
                    EditorUtility.SetDirty(loader);
                    Debug.Log("Assigned PureDotsRuntimeConfig.");
                }
                else
                {
                    Debug.LogError($"Could not find config at {configPath}");
                }
            }
        }
        else
        {
            Debug.LogWarning("PureDotsRuntimeConfigLoader GameObject not found.");
        }

        // 2. Fix DemoEnsureSRP
        var demoBootstrapGO = GameObject.Find("DemoBootstrap");
        if (demoBootstrapGO != null)
        {
            var ensureSRP = demoBootstrapGO.GetComponent<DemoEnsureSRP>();
            if (ensureSRP == null)
            {
                ensureSRP = demoBootstrapGO.AddComponent<DemoEnsureSRP>();
                Debug.Log("Added DemoEnsureSRP component.");
            }

            if (ensureSRP.fallbackAsset == null)
            {
                string urpPath = "Assets/Rendering/DemoURP.asset";
                var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(urpPath);
                if (urpAsset != null)
                {
                    ensureSRP.fallbackAsset = urpAsset;
                    EditorUtility.SetDirty(ensureSRP);
                    Debug.Log("Assigned DemoURP asset.");
                }
                else
                {
                    Debug.LogError($"Could not find URP asset at {urpPath}");
                }
            }
        }
        else
        {
            Debug.LogWarning("DemoBootstrap GameObject not found.");
        }

        // 3. Fix EventSystem (Input System)
        var eventSystemGO = GameObject.Find("EventSystem");
        if (eventSystemGO != null)
        {
            var standaloneInput = eventSystemGO.GetComponent<StandaloneInputModule>();
            if (standaloneInput != null)
            {
                Object.DestroyImmediate(standaloneInput);
                Debug.Log("Removed StandaloneInputModule.");
            }

            var inputSystemUI = eventSystemGO.GetComponent<InputSystemUIInputModule>();
            if (inputSystemUI == null)
            {
                inputSystemUI = eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Debug.Log("Added InputSystemUIInputModule.");
            }
        }
        else
        {
            Debug.LogWarning("EventSystem GameObject not found.");
        }

        // 4. Ensure Camera
        if (Camera.main == null)
        {
            Debug.LogWarning("No Main Camera found in scene.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
