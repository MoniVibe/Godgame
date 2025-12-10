using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.CameraRig
{
    public struct CameraRigState : IComponentData
    {
        public float3 FocusPoint;
        public float Distance;
        public float Yaw;
        public float Pitch;
    }

    public struct CameraRigCommand : IComponentData
    {
        public float2 OrbitInput;
        public float2 PanInput;
        public float ZoomInput;
    }

    public struct CameraRigSettings : IComponentData
    {
        public float OrbitSensitivity;
        public float PanSpeed;
        public float ZoomSpeed;
        public float MinDistance;
        public float MaxDistance;
        public float MinPitch;
        public float MaxPitch;
    }
}
