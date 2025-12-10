using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Debug script to show which camera is rendering in the Game view.
    /// Attach to any Camera GameObject to see which one is active.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraDebugOverlay : MonoBehaviour
    {
        [SerializeField] private string _label = "";
        [SerializeField] private Color _textColor = Color.white;

        private UnityEngine.Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        void OnGUI()
        {
            // Only show for the camera that's actually rendering
            if (_camera != UnityEngine.Camera.current)
                return;

            GUI.color = _textColor;
            GUI.Label(new Rect(10, 10, 600, 40),
                $"🎥 Camera: {name} ({_label}) | Pos: {transform.position:F1} | Rot: {transform.rotation.eulerAngles:F1} | Enabled: {_camera.enabled}");

            GUI.color = Color.white;
        }
    }
}
