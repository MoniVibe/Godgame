using UnityEngine;
using UnityEngine.Rendering;

public class DebugCameraInfo : MonoBehaviour
{
    void Start()
    {
        var cam = GetComponent<Camera>();
        if (cam != null)
        {
            Debug.Log($"[DebugCameraInfo] Name: {name}");
            Debug.Log($"[DebugCameraInfo] Position: {transform.position}");
            Debug.Log($"[DebugCameraInfo] Rotation: {transform.rotation.eulerAngles}");
            Debug.Log($"[DebugCameraInfo] CullingMask: {cam.cullingMask}");
            Debug.Log($"[DebugCameraInfo] Near: {cam.nearClipPlane}, Far: {cam.farClipPlane}");
            Debug.Log($"[DebugCameraInfo] Depth: {cam.depth}");
            Debug.Log($"[DebugCameraInfo] ClearFlags: {cam.clearFlags}");
            Debug.Log($"[DebugCameraInfo] RenderPath: {cam.renderingPath}");
            
            // var additional = GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            // if (additional != null)
            // {
            //     // Debug.Log($"[DebugCameraInfo] URP Camera Type: {additional.cameraType}");
            //     Debug.Log($"[DebugCameraInfo] URP Render Shadows: {additional.renderShadows}");
            //     Debug.Log($"[DebugCameraInfo] URP Render PostProcessing: {additional.renderPostProcessing}");
            // }
        }
        
        var cube = GameObject.Find("ReferenceCube");
        if (cube != null)
        {
            Debug.Log($"[DebugCameraInfo] ReferenceCube Layer: {cube.layer}");
            Debug.Log($"[DebugCameraInfo] ReferenceCube Position: {cube.transform.position}");
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Debug.Log($"[DebugCameraInfo] ReferenceCube Visible: {renderer.isVisible}");
                Debug.Log($"[DebugCameraInfo] ReferenceCube Bounds: {renderer.bounds}");
            }
        }
        else
        {
            Debug.LogError("[DebugCameraInfo] ReferenceCube not found!");
        }
    }
}
