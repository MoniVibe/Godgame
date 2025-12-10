using Godgame.CameraRig;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.CameraRig
{
    /// <summary>
    /// Integrates camera rig commands into the rig state.
    /// </summary>
    public partial struct CameraRigSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraRigState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (rig, command, settings) in SystemAPI.Query<RefRW<CameraRigState>, RefRW<CameraRigCommand>, RefRO<CameraRigSettings>>())
            {
                var rigValue = rig.ValueRO;
                var cmdValue = command.ValueRO;
                var cfg = settings.ValueRO;

                rigValue.Yaw += cmdValue.OrbitInput.x * cfg.OrbitSensitivity;
                rigValue.Pitch = math.clamp(rigValue.Pitch + cmdValue.OrbitInput.y * cfg.OrbitSensitivity, cfg.MinPitch, cfg.MaxPitch);

                var pan = new float3(cmdValue.PanInput.x, 0f, cmdValue.PanInput.y);
                rigValue.FocusPoint += pan * cfg.PanSpeed * deltaTime;

                rigValue.Distance = math.clamp(rigValue.Distance - cmdValue.ZoomInput * cfg.ZoomSpeed * deltaTime, cfg.MinDistance, cfg.MaxDistance);

                rig.ValueRW = rigValue;
                command.ValueRW = default;
            }
        }
    }
}
