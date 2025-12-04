using UnityEngine;

namespace Godgame.DevTools
{
    /// <summary>
    /// Authoring component to add dev menu to a scene.
    /// Add this to any GameObject in your scene to enable the F12 dev menu.
    /// </summary>
    [AddComponentMenu("Godgame/Dev Tools/Dev Menu")]
    public class DevMenuAuthoring : MonoBehaviour
    {
        [Header("Dev Menu Settings")]
        
        [Tooltip("Distance from camera to spawn entities")]
        public float spawnDistance = 10f;
        
        [Tooltip("Align spawned entities to ground")]
        public bool alignToGround = true;
        
        [Header("Performance Overlay")]
        [Tooltip("Show mini performance overlay in corner")]
        public bool showPerformanceOverlay = true;
        
        [Header("Debug Options")]
        [Tooltip("Log all spawn actions to console")]
        public bool verboseLogging = true;
        
        private void Awake()
        {
            // Check if dev menu already exists
            if (GodgameDevMenu.Instance != null)
            {
                Debug.Log("[DevMenu] Dev menu already exists, skipping creation.");
                return;
            }
            
            // Create dev menu
            var devMenuGo = new GameObject("GodgameDevMenu");
            var devMenu = devMenuGo.AddComponent<GodgameDevMenu>();
            devMenu.spawnDistance = spawnDistance;
            devMenu.alignToGround = alignToGround;
            
            Debug.Log("[DevMenu] Created. Press F12 to toggle.");
        }
    }
}
















