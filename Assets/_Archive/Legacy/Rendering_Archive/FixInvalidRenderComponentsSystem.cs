using Unity.Entities;

namespace Godgame.Demo
{
    /// <summary>
    /// Legacy render-fix system for pre-universal rendering.
    /// UniversalDebugRenderSetupSystem now owns debug MaterialMeshInfo,
    /// so this is intentionally disabled to avoid fighting with Entities Graphics 1.4.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct FixInvalidRenderComponentsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // System kept as a stub so asmdef / references stay intact.
            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            // Intentionally no-op.
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
