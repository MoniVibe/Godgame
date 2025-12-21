using UnityEngine;
using UnityEngine.UI;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// MonoBehaviour for selecting scenario presets.
    /// Provides dropdown or button grid to select preset and applies configs on selection.
    /// </summary>
    public class ScenarioPresetSelector : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Dropdown for preset selection")]
        public Dropdown presetDropdown;

        [Tooltip("Button grid container (optional, alternative to dropdown)")]
        public Transform buttonGridContainer;

        [Header("Presets")]
        [Tooltip("Available scenario presets")]
        public ScenarioPreset[] availablePresets;

        [Tooltip("Currently selected preset")]
        public ScenarioPreset selectedPreset;

        private void Start()
        {
            if (presetDropdown != null)
            {
                // Populate dropdown
                presetDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                foreach (var preset in availablePresets)
                {
                    if (preset != null)
                    {
                        options.Add(preset.PresetName);
                    }
                }
                presetDropdown.AddOptions(options);
                presetDropdown.onValueChanged.AddListener(OnPresetSelected);

                // Select first preset by default
                if (availablePresets.Length > 0 && availablePresets[0] != null)
                {
                    selectedPreset = availablePresets[0];
                    ApplyPreset(selectedPreset);
                }
            }
        }

        private void OnPresetSelected(int index)
        {
            if (index >= 0 && index < availablePresets.Length && availablePresets[index] != null)
            {
                selectedPreset = availablePresets[index];
                ApplyPreset(selectedPreset);
            }
        }

        private void ApplyPreset(ScenarioPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            // Apply preset configs
            // This would update GodgameScenarioConfigAuthoring and PresentationConfigAuthoring in the scene
            // For now, this is a placeholder - actual implementation would:
            // 1. Find GodgameScenarioConfigAuthoring in scene
            // 2. Update its Config reference to preset.Config
            // 3. Find PresentationConfigAuthoring in scene
            // 4. Update its values from preset.PresentationConfig (if set)

            Debug.Log($"[ScenarioPresetSelector] Applied preset: {preset.PresetName}");
        }
    }
}
