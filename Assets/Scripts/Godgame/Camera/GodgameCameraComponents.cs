using Unity.Entities;
using Unity.Mathematics;

namespace Moni.Godgame.CameraSystems
{
    /// <summary>
    /// Current camera mode.
    /// </summary>
    public enum CameraMode : byte
    {
        RTSFreeFly,
        Orbital
    }

    /// <summary>
    /// Static camera settings that don't change during gameplay.
    /// </summary>
    public struct CameraSettings : IComponentData
    {
        public float MovementSpeed;
        public float FastMovementMultiplier;
        public float ZoomSpeed;
        public float ZoomMin;
        public float ZoomMax;
        public float PitchMin;
        public float PitchMax;
        public bool StartWithYPlaneLock;
    }

    /// <summary>
    /// Current camera transform state.
    /// </summary>
    public struct CameraTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    /// <summary>
    /// Current camera mode state and settings.
    /// </summary>
    public struct CameraModeState : IComponentData
    {
        public CameraMode Mode;
        public bool YPlaneLocked;
        public float DistanceFromPivot;
        public float PitchDegrees;
        public float YawDegrees;
        public float3 OrbitalFocus;
    }

    /// <summary>
    /// Tag component identifying the main camera entity.
    /// </summary>
    public struct CameraTag : IComponentData { }

    /// <summary>
    /// Default values for camera components.
    /// </summary>
    public static class CameraDefaults
    {
        public static CameraSettings DefaultSettings => new()
        {
            MovementSpeed = 10f,
            FastMovementMultiplier = 3f,
            ZoomSpeed = 6f,
            ZoomMin = 4f,
            ZoomMax = 120f,
            PitchMin = -89f,
            PitchMax = 89f,
            StartWithYPlaneLock = true
        };

        public static CameraTransform DefaultTransform => new()
        {
            Position = float3.zero,
            Rotation = quaternion.identity
        };

        public static CameraModeState DefaultModeState => new()
        {
            Mode = CameraMode.RTSFreeFly,
            YPlaneLocked = true,
            DistanceFromPivot = 12f,
            PitchDegrees = 45f,
            YawDegrees = 0f,
            OrbitalFocus = float3.zero
        };
    }
}
