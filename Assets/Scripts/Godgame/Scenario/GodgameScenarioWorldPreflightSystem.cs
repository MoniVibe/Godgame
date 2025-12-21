using Unity.Burst;
using Unity.Entities;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// Stub preflight config system; real implementation will be
    /// reattached to the current PureDOTS config types later.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameScenarioWorldPreflightSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
        }

        public void OnDestroy(ref SystemState state) {}

        public void OnUpdate(ref SystemState state) {}
    }
}
