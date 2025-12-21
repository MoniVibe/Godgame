using System;
using Unity.Entities;
using UnityEngine.Scripting.APIUpdating;

namespace Godgame.Scenario
{
    /// <summary>
    /// Tag used to opt into legacy scenario systems.
    /// </summary>
    [MovedFrom(true, "Godgame.Scenario", null, "LegacyGodgameDemoTag")]
    public struct LegacyGodgameScenarioTag : IComponentData
    {
    }

    public static class GodgameLegacyScenarioGate
    {
        public const string ScenarioEnvVar = "GODGAME_LEGACY_SCENARIO";

        public static bool IsEnabled
        {
            get
            {
                return IsEnabled(ScenarioEnvVar);
            }
        }

        private static bool IsEnabled(string envVar)
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
