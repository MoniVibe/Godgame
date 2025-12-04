namespace Godgame.Demo
{
    /// <summary>
    /// Mode for demo/scenario bootstrapping.
    /// </summary>
    public enum DemoScenarioMode : byte
    {
        /// <summary>Fixed Demo_01 with configurable parameters</summary>
        Demo01 = 0,
        /// <summary>10k villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_10k = 1,
        /// <summary>100k villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_100k = 2,
        /// <summary>1M villagers scenario (PureDOTS ScenarioRunner)</summary>
        Scenario_1M = 3
    }
}

