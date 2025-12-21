using Unity.Burst;
using Unity.Entities;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// Stubbed out while core FX pipeline is not wired to PureDOTS.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameScenarioEffectPingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
        }

        public void OnDestroy(ref SystemState state) {}

        public void OnUpdate(ref SystemState state) {}
    }
}
