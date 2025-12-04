using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Godgame.Input;

namespace Godgame.DevTools
{
    /// <summary>
    /// Rimworld-style developer menu with IMGUI interface.
    /// Toggle with F12 (configurable). Provides entity spawning, inspection, and debug tools.
    /// </summary>
    public class GodgameDevMenu : MonoBehaviour
    {
        
        [Header("Spawn Settings")]
        public float spawnDistance = 5f;
        public bool alignToGround = true;
        
        // Menu state
        private bool _isOpen;
        private DevMenuTab _currentTab = DevMenuTab.Spawn;
        private Vector2 _scrollPosition;
        private Rect _windowRect = new Rect(20, 20, 400, 600);
        
        // Submenu state
        private string _selectedSpawnCategory = "Villagers";
        private int _spawnCount = 1;
        private bool _randomizeStats = true;
        
        // Performance tracking
        private float _fpsUpdateTimer;
        private int _frameCount;
        private float _currentFps;
        private float _deltaTimeAccum;
        
        // Cached references
        private UnityEngine.Camera _mainCamera;
        private World _ecsWorld;
        
        // Registered spawn categories
        private readonly Dictionary<string, List<SpawnableEntry>> _spawnCategories = new();
        
        // Registered dev panels
        private readonly List<IDevMenuPanel> _customPanels = new();
        
        public enum DevMenuTab
        {
            Spawn,
            Inspect,
            Performance,
            World,
            Combat,
            Custom
        }
        
        public static GodgameDevMenu Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _ecsWorld = World.DefaultGameObjectInjectionWorld;
            RegisterDefaultCategories();
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        private void Update()
        {
            // Toggle menu via ECS input
            if (_ecsWorld != null && _ecsWorld.IsCreated)
            {
                var em = _ecsWorld.EntityManager;
                var query = em.CreateEntityQuery(typeof(DebugInput));
                if (!query.IsEmpty)
                {
                    var debugInput = query.GetSingleton<DebugInput>();
                    if (debugInput.ToggleDevMenu == 1)
                    {
                        _isOpen = !_isOpen;
                        if (_isOpen)
                        {
                            _mainCamera = UnityEngine.Camera.main;
                        }
                    }
                }
            }
            
            // FPS tracking
            _frameCount++;
            _deltaTimeAccum += UnityEngine.Time.unscaledDeltaTime;
            _fpsUpdateTimer += UnityEngine.Time.unscaledDeltaTime;
            if (_fpsUpdateTimer >= 0.5f)
            {
                _currentFps = _frameCount / _deltaTimeAccum;
                _frameCount = 0;
                _deltaTimeAccum = 0f;
                _fpsUpdateTimer = 0f;
            }
        }
        
        private void OnGUI()
        {
            if (!_isOpen) return;
            
            // Draw main window
            _windowRect = GUI.Window(12345, _windowRect, DrawMainWindow, "Godgame Dev Menu (F12)");
            
            // Draw mini performance overlay in corner
            DrawPerformanceOverlay();
        }
        
        private void DrawMainWindow(int windowId)
        {
            // Tab bar
            GUILayout.BeginHorizontal();
            foreach (DevMenuTab tab in Enum.GetValues(typeof(DevMenuTab)))
            {
                var style = _currentTab == tab ? GUI.skin.button : GUI.skin.box;
                if (GUILayout.Button(tab.ToString(), style, GUILayout.Width(60)))
                {
                    _currentTab = tab;
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Tab content
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            switch (_currentTab)
            {
                case DevMenuTab.Spawn:
                    DrawSpawnTab();
                    break;
                case DevMenuTab.Inspect:
                    DrawInspectTab();
                    break;
                case DevMenuTab.Performance:
                    DrawPerformanceTab();
                    break;
                case DevMenuTab.World:
                    DrawWorldTab();
                    break;
                case DevMenuTab.Combat:
                    DrawCombatTab();
                    break;
                case DevMenuTab.Custom:
                    DrawCustomPanels();
                    break;
            }
            GUILayout.EndScrollView();
            
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        }
        
        private void DrawSpawnTab()
        {
            GUILayout.Label("Entity Spawning", GUI.skin.box);
            
            // Category selection
            GUILayout.BeginHorizontal();
            GUILayout.Label("Category:", GUILayout.Width(70));
            foreach (var category in _spawnCategories.Keys)
            {
                var style = _selectedSpawnCategory == category ? GUI.skin.button : GUI.skin.box;
                if (GUILayout.Button(category, style))
                {
                    _selectedSpawnCategory = category;
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Spawn options
            GUILayout.BeginHorizontal();
            GUILayout.Label("Count:", GUILayout.Width(70));
            int.TryParse(GUILayout.TextField(_spawnCount.ToString(), GUILayout.Width(50)), out _spawnCount);
            _spawnCount = Mathf.Clamp(_spawnCount, 1, 10000);
            
            if (GUILayout.Button("x10")) _spawnCount = Mathf.Min(_spawnCount * 10, 10000);
            if (GUILayout.Button("x100")) _spawnCount = Mathf.Min(_spawnCount * 100, 10000);
            GUILayout.EndHorizontal();
            
            _randomizeStats = GUILayout.Toggle(_randomizeStats, "Randomize Stats");
            
            GUILayout.Space(10);
            
            // Entity buttons for selected category
            if (_spawnCategories.TryGetValue(_selectedSpawnCategory, out var entries))
            {
                foreach (var entry in entries)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button($"Spawn {entry.DisplayName}", GUILayout.Height(30)))
                    {
                        SpawnEntity(entry, _spawnCount, _randomizeStats);
                    }
                    GUILayout.Label(entry.Description, GUILayout.Width(150));
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(20);
            
            // Quick spawn buttons
            GUILayout.Label("Quick Spawn", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Village + 20 Villagers")) QuickSpawnVillage(20);
            if (GUILayout.Button("Combat Test")) QuickSpawnCombatTest();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stress Test: 1000")) QuickSpawnStressTest(1000);
            if (GUILayout.Button("Stress Test: 5000")) QuickSpawnStressTest(5000);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stress Test: 10000")) QuickSpawnStressTest(10000);
            if (GUILayout.Button("Clear All")) ClearAllEntities();
            GUILayout.EndHorizontal();
        }
        
        private void DrawInspectTab()
        {
            GUILayout.Label("Entity Inspection (Click to select)", GUI.skin.box);
            
            // TODO: Integration with ECS - show selected entity components
            GUILayout.Label("Left-click an entity in the scene to inspect.");
            GUILayout.Label("Right-click to destroy.");
            
            GUILayout.Space(10);
            GUILayout.Label("Selected Entity: None");
            
            GUILayout.Space(10);
            GUILayout.Label("Entity Query", GUI.skin.box);
            if (GUILayout.Button("List All Villagers")) ListEntitiesOfType("Villager");
            if (GUILayout.Button("List All Villages")) ListEntitiesOfType("Village");
            if (GUILayout.Button("List All Bands")) ListEntitiesOfType("Band");
            if (GUILayout.Button("List All Buildings")) ListEntitiesOfType("Building");
        }
        
        private void DrawPerformanceTab()
        {
            GUILayout.Label("Performance Metrics", GUI.skin.box);
            
            GUILayout.Label($"FPS: {_currentFps:F1}");
            GUILayout.Label($"Frame Time: {(1000f / _currentFps):F2}ms");
            GUILayout.Label($"Time Scale: {UnityEngine.Time.timeScale}x");
            
            GUILayout.Space(10);
            
            // Entity counts from ECS bridge
            GUILayout.Label("Entity Counts", GUI.skin.box);
            if (Bridge != null)
            {
                GUILayout.Label($"Total Entities: {Bridge.TotalEntityCount:N0}");
                GUILayout.Label($"Villagers: {Bridge.VillagerCount:N0}");
                GUILayout.Label($"Villages: {Bridge.VillageCount:N0}");
                GUILayout.Label($"Buildings: {Bridge.BuildingCount:N0}");
            }
            else
            {
                GUILayout.Label("(ECS not running)");
            }
            
            GUILayout.Space(10);
            
            // Memory stats
            GUILayout.Label("Memory", GUI.skin.box);
            var totalMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            GUILayout.Label($"Managed Heap: {totalMemory:F1} MB");
            
            GUILayout.Space(10);
            
            GUILayout.Label("System Controls", GUI.skin.box);
            if (GUILayout.Button("Force GC Collect"))
            {
                System.GC.Collect();
                Debug.Log("[DevMenu] GC.Collect() called.");
            }
            if (GUILayout.Button("Log Entity Stats"))
            {
                ListEntitiesOfType("All");
            }
        }
        
        private void DrawWorldTab()
        {
            GUILayout.Label("World Controls", GUI.skin.box);
            
            GUILayout.Label("Time Control", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pause")) UnityEngine.Time.timeScale = 0f;
            if (GUILayout.Button("1x")) UnityEngine.Time.timeScale = 1f;
            if (GUILayout.Button("2x")) UnityEngine.Time.timeScale = 2f;
            if (GUILayout.Button("5x")) UnityEngine.Time.timeScale = 5f;
            if (GUILayout.Button("10x")) UnityEngine.Time.timeScale = 10f;
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Day/Night Phase", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dawn")) SetDailyPhase(0);
            if (GUILayout.Button("Noon")) SetDailyPhase(1);
            if (GUILayout.Button("Dusk")) SetDailyPhase(2);
            if (GUILayout.Button("Night")) SetDailyPhase(3);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Weather", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear")) SetWeather("Clear");
            if (GUILayout.Button("Rain")) SetWeather("Rain");
            if (GUILayout.Button("Storm")) SetWeather("Storm");
            if (GUILayout.Button("Snow")) SetWeather("Snow");
            GUILayout.EndHorizontal();
        }
        
        private void DrawCombatTab()
        {
            GUILayout.Label("Combat Testing", GUI.skin.box);
            
            GUILayout.Label("Spawn Combat Entities", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Friendly Band (5)")) SpawnBand("Friendly", 5);
            if (GUILayout.Button("Enemy Band (5)")) SpawnBand("Enemy", 5);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Friendly Army (20)")) SpawnArmy("Friendly", 20);
            if (GUILayout.Button("Enemy Army (20)")) SpawnArmy("Enemy", 20);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Focus System", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Max Focus All")) SetAllFocus(1f);
            if (GUILayout.Button("Zero Focus All")) SetAllFocus(0f);
            if (GUILayout.Button("Trigger Breakdown")) TriggerRandomBreakdown();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Combat Triggers", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Battle")) StartBattle();
            if (GUILayout.Button("Force Retreat All")) ForceRetreat();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.Label("Stats Override", GUI.skin.box);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("God Mode (Player)")) ToggleGodMode();
            if (GUILayout.Button("One-Hit Kill")) ToggleOneHitKill();
            GUILayout.EndHorizontal();
        }
        
        private void DrawCustomPanels()
        {
            if (_customPanels.Count == 0)
            {
                GUILayout.Label("No custom panels registered.", GUI.skin.box);
                GUILayout.Label("Use GodgameDevMenu.Instance.RegisterPanel() to add panels.");
                return;
            }
            
            foreach (var panel in _customPanels)
            {
                if (GUILayout.Button(panel.PanelName, GUI.skin.box))
                {
                    panel.IsExpanded = !panel.IsExpanded;
                }
                if (panel.IsExpanded)
                {
                    panel.DrawPanel();
                }
                GUILayout.Space(5);
            }
        }
        
        private void DrawPerformanceOverlay()
        {
            var rect = new Rect(Screen.width - 200, 10, 190, 80);
            GUI.Box(rect, "");
            GUILayout.BeginArea(rect);
            GUILayout.Label($"FPS: {_currentFps:F1} | {UnityEngine.Time.timeScale}x");
            if (Bridge != null)
            {
                GUILayout.Label($"Entities: {Bridge.TotalEntityCount:N0}");
                GUILayout.Label($"Villagers: {Bridge.VillagerCount:N0}");
            }
            else
            {
                GUILayout.Label("ECS: Waiting...");
            }
            GUILayout.Label("F12: Toggle Menu");
            GUILayout.EndArea();
        }
        
        #region Spawn Categories Registration
        
        private void RegisterDefaultCategories()
        {
            // Villagers
            RegisterSpawnCategory("Villagers", new List<SpawnableEntry>
            {
                new("Villager", "Basic villager", "Assets/Prefabs/Villagers/Villager.prefab", SpawnType.Prefab),
                new("Worker", "Specialized worker", null, SpawnType.Archetype),
                new("Warrior", "Combat-trained", null, SpawnType.Archetype),
                new("Scholar", "Research-focused", null, SpawnType.Archetype),
                new("Priest", "Faith-generating", null, SpawnType.Archetype)
            });
            
            // Buildings
            RegisterSpawnCategory("Buildings", new List<SpawnableEntry>
            {
                new("Storehouse", "Resource storage", "Assets/Prefabs/Buildings/Storehouse.prefab", SpawnType.Prefab),
                new("Housing", "Villager homes", "Assets/Prefabs/Buildings/Housing.prefab", SpawnType.Prefab),
                new("Village Center", "Core building", "Assets/Prefabs/Buildings/VillageCenter.prefab", SpawnType.Prefab),
                new("Worship Site", "Faith generation", "Assets/Prefabs/Buildings/WorshipSite.prefab", SpawnType.Prefab)
            });
            
            // Villages
            RegisterSpawnCategory("Villages", new List<SpawnableEntry>
            {
                new("Small Village", "5 villagers + buildings", null, SpawnType.Complex),
                new("Medium Village", "15 villagers + buildings", null, SpawnType.Complex),
                new("Large Village", "30 villagers + buildings", null, SpawnType.Complex)
            });
            
            // Military
            RegisterSpawnCategory("Military", new List<SpawnableEntry>
            {
                new("Scout Band", "3 light units", null, SpawnType.Complex),
                new("War Band", "5 mixed units", null, SpawnType.Complex),
                new("Army", "20 soldiers", null, SpawnType.Complex),
                new("Champion", "Single powerful unit", null, SpawnType.Archetype)
            });
            
            // Resources
            RegisterSpawnCategory("Resources", new List<SpawnableEntry>
            {
                new("Iron Node", "Ore deposit", null, SpawnType.Entity),
                new("Wood Pile", "Timber", null, SpawnType.Entity),
                new("Food Cache", "Rations", null, SpawnType.Entity),
                new("Stone Quarry", "Stone blocks", null, SpawnType.Entity)
            });
            
            // Creatures
            RegisterSpawnCategory("Creatures", new List<SpawnableEntry>
            {
                new("Wolf Pack", "3 wolves", null, SpawnType.Complex),
                new("Deer Herd", "5 deer", null, SpawnType.Complex),
                new("Demon", "Single demon", null, SpawnType.Archetype),
                new("Angel", "Single angel", null, SpawnType.Archetype)
            });
        }
        
        public void RegisterSpawnCategory(string category, List<SpawnableEntry> entries)
        {
            _spawnCategories[category] = entries;
        }
        
        public void RegisterPanel(IDevMenuPanel panel)
        {
            _customPanels.Add(panel);
        }
        
        #endregion
        
        #region Spawn Actions (ECS Integration)
        
        private DevMenuECSBridge Bridge => DevMenuECSBridgeSystem.Bridge;
        
        private void SpawnEntity(SpawnableEntry entry, int count, bool randomize)
        {
            var spawnPos = GetSpawnPosition();
            
            if (Bridge == null)
            {
                Debug.LogWarning("[DevMenu] ECS bridge not available. Make sure the game is running.");
                return;
            }
            
            // Map entry type to spawn type
            var spawnType = entry.DisplayName switch
            {
                "Villager" => DevSpawnType.Villager,
                "Worker" => DevSpawnType.Villager,
                "Warrior" => DevSpawnType.Villager,
                "Scholar" => DevSpawnType.Villager,
                "Priest" => DevSpawnType.Villager,
                "Small Village" => DevSpawnType.Village,
                "Medium Village" => DevSpawnType.Village,
                "Large Village" => DevSpawnType.Village,
                _ => DevSpawnType.Villager
            };
            
            // Adjust count for complex types
            var adjustedCount = entry.DisplayName switch
            {
                "Small Village" => 5,
                "Medium Village" => 15,
                "Large Village" => 30,
                _ => count
            };
            
            Bridge.RequestSpawn(spawnType, (float3)spawnPos, adjustedCount, randomize);
        }
        
        private Vector3 GetSpawnPosition()
        {
            if (_mainCamera == null) _mainCamera = UnityEngine.Camera.main;
            if (_mainCamera == null) return Vector3.zero;
            
            var ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (UnityEngine.Physics.Raycast(ray, out var hit, 100f))
            {
                return hit.point;
            }
            return _mainCamera.transform.position + _mainCamera.transform.forward * spawnDistance;
        }
        
        private void QuickSpawnVillage(int villagerCount)
        {
            if (Bridge == null) return;
            Bridge.RequestSpawn(DevSpawnType.Village, (float3)GetSpawnPosition(), villagerCount);
        }
        
        private void QuickSpawnCombatTest()
        {
            if (Bridge == null) return;
            
            var pos = GetSpawnPosition();
            // Spawn friendly band on left
            Bridge.RequestSpawn(DevSpawnType.Band, (float3)(pos + Vector3.left * 10f), 5, true, true);
            // Spawn enemy band on right
            Bridge.RequestSpawn(DevSpawnType.Band, (float3)(pos + Vector3.right * 10f), 5, true, false);
        }
        
        private void QuickSpawnStressTest(int count)
        {
            if (Bridge == null) return;
            Bridge.RequestSpawn(DevSpawnType.StressTest, (float3)GetSpawnPosition(), count);
        }
        
        private void ClearAllEntities()
        {
            DevEntityCleanupSystem.CleanupRequested = true;
        }
        
        private void ListEntitiesOfType(string type)
        {
            if (Bridge == null)
            {
                Debug.Log($"[DevMenu] ECS bridge not available.");
                return;
            }
            
            Debug.Log($"[DevMenu] Entity counts:");
            Debug.Log($"  Total: {Bridge.TotalEntityCount}");
            Debug.Log($"  Villagers: {Bridge.VillagerCount}");
            Debug.Log($"  Villages: {Bridge.VillageCount}");
            Debug.Log($"  Buildings: {Bridge.BuildingCount}");
        }
        
        private void SetDailyPhase(int phase)
        {
            Debug.Log($"[DevMenu] Setting daily phase to {phase}");
            // TODO: Implement DailyPhase singleton modification
        }
        
        private void SetWeather(string weather)
        {
            Debug.Log($"[DevMenu] Setting weather to {weather}");
            // TODO: Integration with weather system
        }
        
        private void SpawnBand(string faction, int size)
        {
            if (Bridge == null) return;
            var pos = GetSpawnPosition();
            bool isFriendly = faction == "Friendly";
            Bridge.RequestSpawn(DevSpawnType.Band, (float3)pos, size, true, isFriendly);
        }
        
        private void SpawnArmy(string faction, int size)
        {
            if (Bridge == null) return;
            var pos = GetSpawnPosition();
            bool isFriendly = faction == "Friendly";
            Bridge.RequestSpawn(DevSpawnType.Army, (float3)pos, size, true, isFriendly);
        }
        
        private void SetAllFocus(float normalized)
        {
            Debug.Log($"[DevMenu] Setting all focus to {normalized * 100}%");
            // TODO: Implement EntityFocus bulk modification
        }
        
        private void TriggerRandomBreakdown()
        {
            Debug.Log("[DevMenu] Triggering random focus breakdown");
            // TODO: Implement random breakdown trigger
        }
        
        private void StartBattle()
        {
            Debug.Log("[DevMenu] Starting battle between nearest opposing forces");
            // TODO: Implement battle trigger
        }
        
        private void ForceRetreat()
        {
            Debug.Log("[DevMenu] Forcing all units to retreat");
            // TODO: Implement retreat command
        }
        
        private void ToggleGodMode()
        {
            Debug.Log("[DevMenu] Toggling god mode");
            // TODO: Implementation
        }
        
        private void ToggleOneHitKill()
        {
            Debug.Log("[DevMenu] Toggling one-hit kill mode");
            // TODO: Implementation
        }
        
        #endregion
    }
    
    public enum SpawnType
    {
        Prefab,     // GameObject prefab to instantiate
        Entity,     // Pure ECS entity archetype
        Archetype,  // Entity with specific component preset
        Complex     // Multi-entity spawning (villages, bands)
    }
    
    public class SpawnableEntry
    {
        public string DisplayName;
        public string Description;
        public string PrefabPath;
        public SpawnType Type;
        
        public SpawnableEntry(string displayName, string description, string prefabPath, SpawnType type)
        {
            DisplayName = displayName;
            Description = description;
            PrefabPath = prefabPath;
            Type = type;
        }
    }
    
    /// <summary>
    /// Interface for custom dev menu panels.
    /// </summary>
    public interface IDevMenuPanel
    {
        string PanelName { get; }
        bool IsExpanded { get; set; }
        void DrawPanel();
    }
}

