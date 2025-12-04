using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Debug script to detect and manage multiple cameras.
    /// Attach to your main camera to automatically disable other cameras.
    /// </summary>
    public class CameraGuard : MonoBehaviour
    {
        [SerializeField] private bool _disableOtherCameras = true;
        [SerializeField] private string[] _allowedCameraNames = new string[] { "Main Camera", "DebugCamera" };

        private void Awake()
        {
            if (!_disableOtherCameras)
                return;

            // Find all cameras in the scene
            UnityEngine.Camera[] allCameras = Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Debug.Log($"[CameraGuard] Found {allCameras.Length} cameras in scene");

            foreach (UnityEngine.Camera cam in allCameras)
            {
                if (cam == GetComponent<UnityEngine.Camera>())
                {
                    Debug.Log($"[CameraGuard] Keeping main camera: {cam.name}");
                    continue;
                }

                // Check if this camera is in our allowed list
                bool isAllowed = false;
                foreach (string allowedName in _allowedCameraNames)
                {
                    if (cam.name.Contains(allowedName))
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (isAllowed)
                {
                    Debug.Log($"[CameraGuard] Keeping allowed camera: {cam.name}");
                }
                else
                {
                    Debug.LogWarning($"[CameraGuard] Disabling rogue camera: {cam.name} (pos: {cam.transform.position})");
                    cam.enabled = false;
                }
            }
        }

        private void OnGUI()
        {
            if (!Application.isEditor)
                return;

            GUI.Label(new Rect(10, 50, 400, 20), $"Active Cameras: {UnityEngine.Camera.allCamerasCount}");
        }
    }
}
