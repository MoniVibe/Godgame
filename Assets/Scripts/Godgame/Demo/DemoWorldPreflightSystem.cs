using Unity.Burst;
using Unity.Entities;

namespace Godgame.Demo
{
    /// <summary>
    /// Stub preflight config system; real implementation will be
    /// reattached to the current PureDOTS config types later.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DemoWorldPreflightSystem : ISystem
    {
        public void OnCreate(ref SystemState state) {}

        public void OnDestroy(ref SystemState state) {}

        public void OnUpdate(ref SystemState state) {}
    }
}
