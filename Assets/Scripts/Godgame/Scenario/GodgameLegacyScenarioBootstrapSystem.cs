using Unity.Entities;

namespace Godgame.Scenario
{
    /// <summary>
    /// Creates the legacy scenario tag when explicitly enabled via environment flag.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct GodgameLegacyScenarioBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!GodgameLegacyScenarioGate.IsEnabled)
            {
                state.Enabled = false;
                return;
            }

            if (!SystemAPI.HasSingleton<LegacyGodgameScenarioTag>())
            {
                var entity = state.EntityManager.CreateEntity(typeof(LegacyGodgameScenarioTag));
                state.EntityManager.SetName(entity, "LegacyGodgameScenarioTag");
            }

            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state) { }
    }
}
