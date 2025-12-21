using UnityEngine;
using UnityEngine.Serialization;

namespace Godgame.DevTools
{
    /// <summary>
    /// Complete dev tools rig. Add this to a scene to get all dev tools functionality.
    /// Creates the dev menu, performance overlay, and enables runtime entity inspection.
    /// </summary>
    [AddComponentMenu("Godgame/Dev Tools/Dev Tools Rig")]
    public class GodgameDevToolsRig : MonoBehaviour
    {
        [Header("Configuration")]
        
        [Tooltip("Start with performance overlay visible")]
        public bool showOverlayOnStart = true;
        
        [Header("Stress Test Defaults")]
        [Tooltip("Default entity count for stress tests")]
        public int defaultStressTestCount = 1000;
        
        [Header("Scenario Scene Settings")]
        [Tooltip("Number of villages to spawn in scenario setup")]
        [FormerlySerializedAs("demoVillageCount")]
        public int scenarioVillageCount = 3;
        
        [Tooltip("Villagers per village in scenario")]
        public int villagersPerVillage = 10;
        
        [Tooltip("Include combat bands in scenario")]
        [FormerlySerializedAs("includeCombatnBands")]
        public bool includeCombatBands = true;
        
        private GodgameDevMenu _devMenu;
        
        private void Start()
        {
            SetupDevMenu();
            
            if (showOverlayOnStart)
            {
                Debug.Log("[DevTools] Dev Tools Rig active. Press F12 to open menu.");
            }
        }
        
        private void SetupDevMenu()
        {
            // Check if dev menu already exists
            _devMenu = FindFirstObjectByType<GodgameDevMenu>();
            if (_devMenu != null) return;
            
            // Create dev menu GameObject
            var devMenuGo = new GameObject("GodgameDevMenu");
            devMenuGo.transform.SetParent(transform);
            _devMenu = devMenuGo.AddComponent<GodgameDevMenu>();
        }
        
        /// <summary>
        /// Spawns a complete scenario setup with villages, villagers, and optionally combat bands.
        /// </summary>
        [ContextMenu("Spawn Scenario Setup")]
        public void SpawnScenarioSetup()
        {
            if (DevMenuECSBridgeSystem.Bridge == null)
            {
                Debug.LogWarning("[DevTools] Cannot spawn scenario - ECS not running. Enter Play mode first.");
                return;
            }
            
            var bridge = DevMenuECSBridgeSystem.Bridge;
            
            // Spawn villages in a circle
            var angleStep = 360f / scenarioVillageCount;
            var radius = 50f;
            
            for (int i = 0; i < scenarioVillageCount; i++)
            {
                var angle = i * angleStep * Mathf.Deg2Rad;
                var pos = new Unity.Mathematics.float3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
                
                bridge.RequestSpawn(DevSpawnType.Village, pos, villagersPerVillage);
            }
            
            // Optionally spawn combat bands
            if (includeCombatBands)
            {
                // Friendly band at center-left
                bridge.RequestSpawn(DevSpawnType.Band, new Unity.Mathematics.float3(-20f, 0f, 0f), 5, true, true);
                // Enemy band at center-right
                bridge.RequestSpawn(DevSpawnType.Band, new Unity.Mathematics.float3(20f, 0f, 0f), 5, true, false);
            }
            
            Debug.Log($"[DevTools] Scenario setup: {scenarioVillageCount} villages with {villagersPerVillage} villagers each" +
                      (includeCombatBands ? " + combat bands" : ""));
        }
        
        /// <summary>
        /// Runs a stress test with the configured entity count.
        /// </summary>
        [ContextMenu("Run Stress Test")]
        public void RunStressTest()
        {
            if (DevMenuECSBridgeSystem.Bridge == null)
            {
                Debug.LogWarning("[DevTools] Cannot run stress test - ECS not running. Enter Play mode first.");
                return;
            }
            
            DevMenuECSBridgeSystem.Bridge.RequestSpawn(
                DevSpawnType.StressTest, 
                Unity.Mathematics.float3.zero, 
                defaultStressTestCount
            );
            
            Debug.Log($"[DevTools] Stress test: spawning {defaultStressTestCount} entities");
        }
        
        /// <summary>
        /// Clears all dev-spawned entities.
        /// </summary>
        [ContextMenu("Clear Dev Entities")]
        public void ClearDevEntities()
        {
            DevEntityCleanupSystem.CleanupRequested = true;
        }
    }
}















