using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Debug script to help troubleshoot cube visibility issues.
    /// Attach to the debug cube to see why it might not be rendering.
    /// </summary>
    public class CubeVisibilityDebugger : MonoBehaviour
    {
        [SerializeField] private bool _showDebugInfo = true;

        private MeshRenderer _renderer;
        private UnityEngine.Camera _mainCamera;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (!_showDebugInfo)
                return;

            // Check if we're visible to the main camera
            if (_mainCamera != null && _renderer != null)
            {
                Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
                bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, _renderer.bounds);

                if (!isVisible)
                {
                    Debug.LogWarning($"[CubeDebug] {name} is outside camera frustum! Cube: {transform.position}, Camera: {_mainCamera.transform.position}");
                }
            }
        }

        private void OnGUI()
        {
            if (!_showDebugInfo || !Application.isEditor)
                return;

            string debugInfo = $"Cube: {name}\n";
            debugInfo += $"Position: {transform.position:F1}\n";
            debugInfo += $"Layer: {LayerMask.LayerToName(gameObject.layer)}\n";
            debugInfo += $"Renderer: {_renderer?.enabled ?? false}\n";
            debugInfo += $"Material: {_renderer?.sharedMaterial?.name ?? "null"}\n";

            if (_mainCamera != null)
            {
                debugInfo += $"Camera: {_mainCamera.name}\n";
                debugInfo += $"Cam Pos: {_mainCamera.transform.position:F1}\n";
                debugInfo += $"Cam Culling: {_mainCamera.cullingMask}\n";

                // Check if layer is in culling mask
                int layerMask = 1 << gameObject.layer;
                bool layerVisible = (_mainCamera.cullingMask & layerMask) != 0;
                debugInfo += $"Layer Visible: {layerVisible}\n";
            }

            GUI.Label(new Rect(10, 80, 400, 120), debugInfo);
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugInfo)
                return;

            // Draw a line from cube to camera
            if (_mainCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _mainCamera.transform.position);
                Gizmos.DrawWireSphere(_mainCamera.transform.position, 1f);
            }

            // Draw cube bounds
            if (_renderer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.size);
            }
        }
    }
}
