using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Debug script that locks the camera to look at a target cube.
    /// Used for testing basic camera-cube visibility before enabling camera controllers.
    /// </summary>
    public class DebugCameraLock : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _cube; // Assign DebugCube in Inspector

        [Header("Camera Position")]
        [SerializeField] private Vector3 _cameraPosition = new Vector3(0f, 5f, -10f);

        void Start()
        {
            // Put camera in a known, safe position
            transform.position = _cameraPosition;
            transform.rotation = Quaternion.identity;

            if (_cube == null)
            {
                Debug.LogWarning("[DebugCameraLock] No cube assigned! Camera will stay at default position.");
            }
            else
            {
                Debug.Log($"[DebugCameraLock] Camera positioned at {_cameraPosition}, looking at {_cube.name} at {_cube.position}");
            }
        }

        void LateUpdate()
        {
            if (_cube == null)
                return;

            // Always look at the cube
            transform.LookAt(_cube.position);
        }

        private void OnDrawGizmos()
        {
            // Draw line from camera to cube in editor
            if (_cube != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _cube.position);
                Gizmos.DrawWireSphere(_cube.position, 1f);
            }
        }
    }
}
