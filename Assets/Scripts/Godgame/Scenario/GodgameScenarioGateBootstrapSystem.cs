using Unity.Entities;

namespace Godgame.Scenario
{
    /// <summary>
    /// Creates the scenario gate tag when explicitly enabled via environment flag.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct GodgameScenarioGateBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!GodgameScenarioGate.IsEnabled)
            {
                state.Enabled = false;
                return;
            }

            if (!SystemAPI.HasSingleton<GodgameScenarioTag>())
            {
                var entity = state.EntityManager.CreateEntity(typeof(GodgameScenarioTag));
                state.EntityManager.SetName(entity, "GodgameScenarioTag");
            }

            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state) { }
    }
}
