using Godgame.Presentation;
using Godgame.Presentation.Authoring;
using UnityEngine;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// ScriptableObject for scenario presets.
    /// Defines different starting conditions for Godgame scenarios.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioPreset", menuName = "Godgame/Scenario Preset", order = 2)]
    public class ScenarioPreset : ScriptableObject
    {
        [Header("Preset Info")]
        [Tooltip("Name of the preset")]
        public string PresetName = "New Preset";

        [Tooltip("Description of the preset")]
        [TextArea(3, 5)]
        public string Description = "Description of this scenario preset";

        [Header("Configuration")]
        [Tooltip("Scenario mode")]
        public DemoScenarioMode Mode = DemoScenarioMode.Demo01;

        [Tooltip("Demo config asset")]
        public DemoConfig Config;

        [Tooltip("Presentation config asset (optional, uses default if not set)")]
        public PresentationConfigAuthoring PresentationConfig;

        [Header("Biome Configuration")]
        [Tooltip("Biome distribution (placeholder for future biome config)")]
        public string BiomeConfig = "Default";
    }

    /// <summary>
    /// Helper class for creating preset instances.
    /// </summary>
    public static class ScenarioPresetHelper
    {
        /// <summary>
        /// Creates a "Peaceful Growth" preset.
        /// </summary>
        public static ScenarioPreset CreatePeacefulGrowth()
        {
            var preset = ScriptableObject.CreateInstance<ScenarioPreset>();
            preset.PresetName = "Peaceful Growth";
            preset.Description = "Many villages, abundant resources, stable biomes. Ideal for observing long-term growth.";
            preset.Mode = DemoScenarioMode.Demo01;
            // Config would be created separately
            return preset;
        }

        /// <summary>
        /// Creates a "Famine Stress Test" preset.
        /// </summary>
        public static ScenarioPreset CreateFamineStressTest()
        {
            var preset = ScriptableObject.CreateInstance<ScenarioPreset>();
            preset.PresetName = "Famine Stress Test";
            preset.Description = "Few resources, many villagers, harsh biomes. Tests crisis management and resource scarcity.";
            preset.Mode = DemoScenarioMode.Demo01;
            return preset;
        }

        /// <summary>
        /// Creates a "Miracle Playground" preset.
        /// </summary>
        public static ScenarioPreset CreateMiraclePlayground()
        {
            var preset = ScriptableObject.CreateInstance<ScenarioPreset>();
            preset.PresetName = "Miracle Playground";
            preset.Description = "Many villages, frequent miracle opportunities. Perfect for testing miracle effects.";
            preset.Mode = DemoScenarioMode.Demo01;
            return preset;
        }

        /// <summary>
        /// Creates a "Biome Stress" preset.
        /// </summary>
        public static ScenarioPreset CreateBiomeStress()
        {
            var preset = ScriptableObject.CreateInstance<ScenarioPreset>();
            preset.PresetName = "Biome Stress";
            preset.Description = "Max vegetation, many nodes, diverse biomes. Tests biome visualization and performance.";
            preset.Mode = DemoScenarioMode.Scenario_10k;
            return preset;
        }
    }
}
