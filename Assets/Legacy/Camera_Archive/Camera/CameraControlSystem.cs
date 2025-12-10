using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Moni.Godgame.CameraSystems;

namespace Godgame
{
    /// <summary>
    /// System that controls camera behavior in DOTS.
    /// When GodgameCameraController exists, this system mirrors its state.
    /// When no Mono controller exists, this system could run full control logic.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CameraControlSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Check if we have a Mono controller to mirror from
            if (GodgameCameraController.TryGetCurrentState(out var monoState))
            {
                // Mirror from Mono controller
                foreach (var (transform, modeState, tag) in SystemAPI.Query<
                    RefRW<CameraTransform>,
                    RefRW<CameraModeState>,
                    RefRO<CameraTag>>())
                {
                    // Update transform
                    transform.ValueRW.Position = monoState.Position;
                    transform.ValueRW.Rotation = monoState.Rotation;

                    // Update mode state
                    modeState.ValueRW.DistanceFromPivot = monoState.Distance;
                    modeState.ValueRW.PitchDegrees = math.degrees(monoState.Pitch);
                    modeState.ValueRW.YawDegrees = math.degrees(monoState.Yaw);

                    // Note: Mode and YPlaneLocked are not exposed in CameraRigState
                    // They would need to be added if DOTS systems need this info
                }
            }
            else
            {
                // No Mono controller - could implement full DOTS control logic here
                // For now, this is a placeholder
            }
        }
    }
}
