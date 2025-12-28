using PureDOTS.Runtime.Scenarios;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Scenario
{
    /// <summary>
    /// Ensures the smoke scenario metadata exists immediately when the Godgame smoke world boots.
    /// Falls back to a hard-coded scenario id so ScenarioRunner contracts stay happy even if
    /// the scenario JSON loader has not yet populated metadata.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameSmokeScenarioSelectorSystem : ISystem
    {
        private const string ScenarioIdText = "scenario.godgame.smoke";
        private const int DefaultRunTicks = 180;
        private const uint DefaultSeed = 42;

        public void OnCreate(ref SystemState state)
        {
            // No additional setup required.
        }

        public void OnDestroy(ref SystemState state)
        {
            // Nothing to clean up.
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ScenarioInfo>())
            {
                state.Enabled = false;
                return;
            }

            var scenarioEntity = state.EntityManager.CreateEntity(typeof(ScenarioInfo));
            state.EntityManager.SetComponentData(scenarioEntity, new ScenarioInfo
            {
                ScenarioId = new FixedString64Bytes(ScenarioIdText),
                Seed = DefaultSeed,
                RunTicks = DefaultRunTicks
            });

            state.Enabled = false;
        }
    }
}

