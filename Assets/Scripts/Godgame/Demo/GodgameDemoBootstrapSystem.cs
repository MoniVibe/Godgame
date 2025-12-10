using Unity.Burst;
using Unity.Entities;

namespace Godgame.Demo
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct Godgame_Demo01_BootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Hook for spawning demo entities or loading presets.
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Add one-shot bootstrap logic here; disable when done if desired.
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
