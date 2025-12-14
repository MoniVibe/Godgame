using Unity.Burst;
using Unity.Entities;

namespace Godgame.Demo
{
    /// <summary>
    /// Stubbed out while core FX pipeline is not wired to PureDOTS.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DemoEffectPingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DemoSceneTag>();
        }

        public void OnDestroy(ref SystemState state) {}

        public void OnUpdate(ref SystemState state) {}
    }
}
