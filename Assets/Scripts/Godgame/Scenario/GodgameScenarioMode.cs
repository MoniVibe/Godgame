using UnityEngine.Scripting.APIUpdating;

namespace Godgame.Scenario
{
    /// <summary>
    /// Mode for scenario bootstrapping.
    /// </summary>
    [MovedFrom(true, "Godgame.Scenario", null, "DemoScenarioMode")]
    public enum GodgameScenarioMode : byte
    {
        /// <summary>Fixed Scenario_01 with configurable parameters</summary>
        Scenario01 = 0,
        /// <summary>10k villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_10k = 1,
        /// <summary>100k villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_100k = 2,
        /// <summary>1M villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_1M = 3
    }
}
