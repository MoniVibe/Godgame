using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class SceneDebugger : MonoBehaviour
{
    private GameObject referenceCube;
    private Camera mainCamera;

    void Start()
    {
        if (RuntimeMode.IsHeadless)
        {
            enabled = false;
            return;
        }

        Debug.Log("[SceneDebugger] Starting...");

        // 1. Ensure Camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Check if any camera exists before creating a new one
            mainCamera = FindFirstObjectByType<Camera>();
            if (mainCamera != null)
            {
                Debug.Log($"[SceneDebugger] Found existing camera '{mainCamera.name}', using it.");
                if (!mainCamera.CompareTag("MainCamera"))
                {
                    if (Godgame.Core.DefaultTagRegistryGuard.TryEnter())
                    {
                        mainCamera.tag = "MainCamera";
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SceneDebugger] Main Camera not found. Creating one.");
                var camGo = new GameObject("Main Camera");
                mainCamera = camGo.AddComponent<Camera>();
                if (Godgame.Core.DefaultTagRegistryGuard.TryEnter())
                {
                    camGo.tag = "MainCamera";
                }
            }
        }

        // 2. Force Camera Position
        mainCamera.transform.position = new Vector3(0, 1, -10);
        mainCamera.transform.LookAt(Vector3.zero);
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 1000f;
        mainCamera.cullingMask = -1; // Everything
        mainCamera.clearFlags = CameraClearFlags.Skybox;

        Debug.Log($"[SceneDebugger] Camera set to {mainCamera.transform.position}, looking at {Vector3.zero}");

        // 3. Create Reference Cube
        referenceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        referenceCube.name = "ReferenceCube_Runtime";
        referenceCube.transform.position = new Vector3(0, 1, 0);
        referenceCube.transform.localScale = Vector3.one;
        var renderer = referenceCube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.material.color = Color.green;
        }
        Debug.Log("[SceneDebugger] Created ReferenceCube_Runtime at (0,1,0)");
    }

    void Update()
    {
        // Keep forcing camera position to fight any other systems
        if (mainCamera != null)
        {
            if (Vector3.Distance(mainCamera.transform.position, new Vector3(0, 1, -10)) > 0.1f)
            {
                Debug.LogWarning($"[SceneDebugger] Camera moved to {mainCamera.transform.position}! Resetting.");
                mainCamera.transform.position = new Vector3(0, 1, -10);
                mainCamera.transform.LookAt(Vector3.zero);
            }
        }
    }
}
