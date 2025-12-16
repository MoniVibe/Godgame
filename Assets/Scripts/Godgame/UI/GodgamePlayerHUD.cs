using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// MonoBehaviour for displaying player-facing HUD.
    /// Shows scenario name, stats, and buttons for Metrics/Debug menu.
    /// </summary>
    public class GodgamePlayerHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Text element for scenario name (top-left)")]
        public Text scenarioNameText;

        [Tooltip("Text element for village count (top-right)")]
        public Text villageCountText;

        [Tooltip("Text element for villager count (top-right)")]
        public Text villagerCountText;

        [Tooltip("Text element for world health (top-right)")]
        public Text worldHealthText;

        [Tooltip("Text element for displaying the dominant villager goal")]
        public Text villagerGoalText;

        [Tooltip("Text element for average focus budget")]
        public Text villagerFocusText;

        [Tooltip("Text element for peak need urgency")]
        public Text villagerNeedText;

        [Tooltip("Button for toggling metrics overlay")]
        public Button metricsButton;

        [Tooltip("Dropdown for debug menu")]
        public Dropdown debugMenuDropdown;

        private World _ecsWorld;
        private PlayerHUDData _cachedHUDData;

        private void Start()
        {
            _ecsWorld = World.DefaultGameObjectInjectionWorld;

            if (metricsButton != null)
            {
                metricsButton.onClick.AddListener(ToggleMetrics);
            }

            if (debugMenuDropdown != null)
            {
                debugMenuDropdown.onValueChanged.AddListener(OnDebugMenuSelected);
                debugMenuDropdown.options.Clear();
                debugMenuDropdown.options.Add(new Dropdown.OptionData("Debug Menu"));
                debugMenuDropdown.options.Add(new Dropdown.OptionData("Toggle LOD Visualization"));
                debugMenuDropdown.options.Add(new Dropdown.OptionData("Toggle Density Visualization"));
                debugMenuDropdown.options.Add(new Dropdown.OptionData("Toggle Pathfinding Debug"));
            }
        }

        private void Update()
        {
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                return;
            }

            var query = _ecsWorld.EntityManager.CreateEntityQuery(typeof(PlayerHUDData));
            if (!query.IsEmpty)
            {
                _cachedHUDData = query.GetSingleton<PlayerHUDData>();

                if (scenarioNameText != null)
                {
                    scenarioNameText.text = _cachedHUDData.ScenarioName.ToString();
                }

                if (villageCountText != null)
                {
                    villageCountText.text = $"Villages: {_cachedHUDData.VillageCount}";
                }

                if (villagerCountText != null)
                {
                    villagerCountText.text = $"Villagers: {_cachedHUDData.VillagerCount}";
                }

                if (worldHealthText != null)
                {
                    int healthPercent = Mathf.RoundToInt(_cachedHUDData.WorldHealth * 100f);
                    worldHealthText.text = $"World Health: {healthPercent}%";
                    // Color based on health
                    worldHealthText.color = Color.Lerp(Color.red, Color.green, _cachedHUDData.WorldHealth);
                }

                if (villagerGoalText != null)
                {
                    villagerGoalText.text = $"Villager goal: {_cachedHUDData.DominantVillagerGoal.ToString()}";
                }

                if (villagerFocusText != null)
                {
                    villagerFocusText.text = $"Avg Focus: {_cachedHUDData.AverageVillagerFocus:F2}";
                }

                if (villagerNeedText != null)
                {
                    villagerNeedText.text = $"Peak Need: {Mathf.RoundToInt(_cachedHUDData.PeakNeedUrgency * 100f)}%";
                }
            }
        }

        private void ToggleMetrics()
        {
            // Toggle PresentationMetricsDisplay
            var metricsDisplay = Object.FindFirstObjectByType<PresentationMetricsDisplay>();
            if (metricsDisplay != null)
            {
                metricsDisplay.enabled = !metricsDisplay.enabled;
            }
        }

        private void OnDebugMenuSelected(int index)
        {
            // Handle debug menu selections
            // This would trigger debug toggles via DebugInput component
            // For now, placeholder
        }
    }
}

