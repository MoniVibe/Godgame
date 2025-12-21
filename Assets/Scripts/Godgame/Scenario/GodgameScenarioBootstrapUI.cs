using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Profiling;
using UnityEngine.UI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    [MovedFrom(true, "Godgame.Scenario", null, "DemoBootstrapUI")]
    public class GodgameScenarioBootstrapUI : MonoBehaviour
    {
        public ScenarioBootstrap bootstrap;
        public Dropdown scenarioDropdown;
        public Button loadButton;
        public Button toggleBindingsButton;
        
        // Time controls
        public Button pauseButton;
        public Button stepButton;
        public Button speed1Button;
        public Button speed2Button;
        public Button speed3Button;
        public Button rewindButton;
        public Button spawnGhostButton;

        [Header("HUD")]
        public UnityEngine.UI.Text villagerStats;
        public UnityEngine.UI.Text jobStats;
        public UnityEngine.UI.Text storehouseStats;
        public UnityEngine.UI.Text buildStats;
        public UnityEngine.UI.Text tickStats;
        public UnityEngine.UI.Text fpsStats;
        public UnityEngine.UI.Text fixedTickStats;
        public UnityEngine.UI.Text snapshotStats;
        public UnityEngine.UI.Text ecbStats;

        private EntityManager entityManager;
        private EntityQuery villagerRegistryQuery;
        private EntityQuery storehouseRegistryQuery;
        private EntityQuery logisticsRegistryQuery;
        private EntityQuery timeStateQuery;
        private EntityQuery rewindStateQuery;

        private void Start()
        {
            if (bootstrap == null)
                bootstrap = FindFirstObjectByType<ScenarioBootstrap>();

            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                entityManager = world.EntityManager;
                villagerRegistryQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<VillagerRegistry>());
                storehouseRegistryQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<StorehouseRegistry>());
                logisticsRegistryQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>());
                timeStateQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>());
                rewindStateQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>());
            }

            RefreshScenarios();
            
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);
            
            if (toggleBindingsButton != null)
                toggleBindingsButton.onClick.AddListener(OnToggleBindingsClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => bootstrap?.TogglePause());
            if (stepButton != null)
                stepButton.onClick.AddListener(() => bootstrap?.StepOnce());
            if (speed1Button != null)
                speed1Button.onClick.AddListener(() => bootstrap?.SetSpeed(0.5f));
            if (speed2Button != null)
                speed2Button.onClick.AddListener(() => bootstrap?.SetSpeed(1f));
            if (speed3Button != null)
                speed3Button.onClick.AddListener(() => bootstrap?.SetSpeed(2f));
            if (rewindButton != null)
                rewindButton.onClick.AddListener(() => bootstrap?.RequestRewind());
            if (spawnGhostButton != null)
                spawnGhostButton.onClick.AddListener(() => bootstrap?.SpawnConstructionGhost());
        }

        private void RefreshScenarios()
        {
            if (scenarioDropdown == null) return;

            scenarioDropdown.ClearOptions();
            var path = Path.Combine(Application.dataPath, "Scenarios/Godgame");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.json");
                var options = new List<string>();
                foreach (var file in files)
                {
                    options.Add(Path.GetFileName(file));
                }
                scenarioDropdown.AddOptions(options);
            }
        }

        private void OnLoadClicked()
        {
            if (scenarioDropdown == null) return;
            var selected = scenarioDropdown.options[scenarioDropdown.value].text;
            bootstrap.LoadScenario(selected);
        }

        private void OnToggleBindingsClicked()
        {
            bootstrap.ToggleBindings();
        }

        private void Update()
        {
            UpdateHud();
        }

        private void UpdateHud()
        {
            SetText(fpsStats, UnityEngine.Time.deltaTime > 0f ? $"FPS: {(1.0f / UnityEngine.Time.deltaTime):F1}" : "FPS: --");
            SetText(fixedTickStats, $"Fixed: {(UnityEngine.Time.fixedDeltaTime * 1000f):F1} ms");

            if (World.DefaultGameObjectInjectionWorld == null) return;
            // Re-acquire if needed (e.g. domain reload or startup timing)
            if (entityManager == default || !entityManager.World.IsCreated)
            {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }

            if (!entityManager.WorldUnmanaged.IsCreated)
            {
                return;
            }

            if (villagerRegistryQuery != null && villagerRegistryQuery.TryGetSingleton(out VillagerRegistry villagerRegistry))
            {
                var activeJobs = math.max(0, villagerRegistry.TotalVillagers - villagerRegistry.IdleVillagers);
                SetText(villagerStats, $"Villagers: {villagerRegistry.TotalVillagers} (idle {villagerRegistry.IdleVillagers})");
                SetText(jobStats, $"Jobs active: {activeJobs}");
            }
            else
            {
                SetText(villagerStats, "Villagers: --");
                SetText(jobStats, "Jobs active: --");
            }

            if (storehouseRegistryQuery != null && storehouseRegistryQuery.TryGetSingleton(out StorehouseRegistry storehouseRegistry))
            {
                SetText(storehouseStats, $"Storage: {storehouseRegistry.TotalStored:F0}/{storehouseRegistry.TotalCapacity:F0}");
            }
            else
            {
                SetText(storehouseStats, "Storage: --");
            }

            if (logisticsRegistryQuery != null && logisticsRegistryQuery.TryGetSingleton(out LogisticsRequestRegistry logisticsRegistry))
            {
                SetText(buildStats, $"Build/Logistics: {logisticsRegistry.InProgressRequests} in progress");
            }
            else
            {
                SetText(buildStats, "Build/Logistics: --");
            }

            if (timeStateQuery != null && timeStateQuery.TryGetSingleton(out TimeState timeState))
            {
                SetText(tickStats, $"Tick: {timeState.Tick}");
            }
            else
            {
                SetText(tickStats, $"Tick: {UnityEngine.Time.frameCount}");
            }

            if (rewindStateQuery != null)
            {
                var count = rewindStateQuery.CalculateEntityCount();
                if (count == 1)
                {
                    var rewindState = rewindStateQuery.GetSingleton<RewindState>();
                    SetText(snapshotStats, $"Rewind: {rewindState.Mode} @ {rewindState.PlaybackTick}");
                }
                else
                {
                    SetText(snapshotStats, "Rewind: unavailable");
                }
            }
            else
            {
                var memoryKb = Profiler.GetTotalAllocatedMemoryLong() / 1024f;
                SetText(snapshotStats, $"Memory: {memoryKb:F0} kb");
            }

            SetText(ecbStats, "ECB: --");
        }

        private static void SetText(UnityEngine.UI.Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
