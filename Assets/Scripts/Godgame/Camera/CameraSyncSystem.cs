using Unity.Entities;
using Unity.Transforms;
using Moni.Godgame.CameraSystems;

namespace Godgame
{
    /// <summary>
    /// System that synchronizes camera state between DOTS and other systems.
    /// This ensures DOTS camera components stay in sync with the authoritative Mono controller.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CameraControlSystem))]
    public partial class CameraSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // This system primarily ensures DOTS camera state is available for other systems to read
            // The actual sync happens in CameraControlSystem

            // Could add telemetry or other sync operations here if needed
            foreach (var (transform, modeState, tag) in SystemAPI.Query<
                RefRO<CameraTransform>,
                RefRO<CameraModeState>,
                RefRO<CameraTag>>())
            {
                // Camera state is now available for other DOTS systems to read
                // Systems can query for CameraTag + CameraTransform + CameraModeState
            }
        }
    }
}
