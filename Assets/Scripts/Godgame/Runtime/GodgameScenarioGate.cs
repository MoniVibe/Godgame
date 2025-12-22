using System;
using Unity.Entities;
using UnityEngine.Scripting.APIUpdating;

namespace Godgame.Scenario
{
    /// <summary>
    /// Tag used to opt into scenario-gated systems.
    /// </summary>
    [MovedFrom(true, "Godgame.Scenario", null, "LegacyGodgameScenarioTag")]
    public struct GodgameScenarioTag : IComponentData
    {
    }

    public static class GodgameScenarioGate
    {
        public const string ScenarioEnvVar = "GODGAME_SCENARIO";
        private const string ScenarioEnvVarCompat = "GODGAME_LEGACY_SCENARIO";

        public static bool IsEnabled
        {
            get
            {
                return IsEnabledEnv(ScenarioEnvVar) || IsEnabledEnv(ScenarioEnvVarCompat);
            }
        }

        private static bool IsEnabledEnv(string envVar)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
