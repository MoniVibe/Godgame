using PureDOTS.Input;
using PureDOTS.Runtime.Camera;
using PureDOTS.Runtime.Hand;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Pure DOTS authoring component for camera setup.
    /// Bakes a camera entity with the PureDOTS camera config/state plus input buffers.
    /// </summary>
    public class CameraAuthoring : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Movement speed for WASD controls")]
        public float MovementSpeed = 10f;
        
        [Tooltip("Mouse look rotation sensitivity")]
        public float RotationSensitivity = 2f;
        
        [Tooltip("Zoom speed per scroll tick")]
        public float ZoomSpeed = 6f;
        
        [Tooltip("Minimum zoom distance (meters)")]
        public float ZoomMin = 6f;
        
        [Tooltip("Maximum zoom distance (meters)")]
        public float ZoomMax = 220f;
        
        [Tooltip("Pan sensitivity for LMB drag")]
        public float PanSensitivity = 1f;
        
        [Header("Orbital Mode")]
        [Tooltip("Sensitivity multiplier for close range (6-20m)")]
        public float SensitivityClose = 1.5f;
        
        [Tooltip("Sensitivity multiplier for mid range (20-100m)")]
        public float SensitivityMid = 1.0f;
        
        [Tooltip("Sensitivity multiplier for far range (100-220m)")]
        public float SensitivityFar = 0.6f;
        
        [Tooltip("Minimum pitch angle (degrees)")]
        public float PitchMin = -30f;
        
        [Tooltip("Maximum pitch angle (degrees)")]
        public float PitchMax = 85f;
        
        [Header("Terrain")]
        [Tooltip("Minimum clearance above terrain (meters)")]
        public float TerrainClearance = 2f;
        
        [Tooltip("Collision buffer safety margin (meters)")]
        public float CollisionBuffer = 0.4f;
        
        [Header("Initial Transform")]
        [Tooltip("Initial camera position")]
        public Vector3 InitialPosition = new Vector3(0f, 10f, -10f);
        
        [Tooltip("Initial camera rotation (euler angles)")]
        public Vector3 InitialRotation = new Vector3(45f, 0f, 0f);
    }

    public class CameraBaker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var initialPosition = (float3)authoring.InitialPosition;
            var initialRotation = quaternion.Euler(math.radians(authoring.InitialRotation));
            var forward = math.mul(initialRotation, new float3(0f, 0f, 1f));
            var up = math.mul(initialRotation, new float3(0f, 1f, 0f));
            var pivot = float3.zero;
            var distance = math.length(initialPosition - pivot);

            AddComponent<CameraTag>(entity);
            AddComponent(entity, new CameraConfig
            {
                OrbitYawSensitivity = authoring.RotationSensitivity,
                OrbitPitchSensitivity = authoring.RotationSensitivity,
                PitchClamp = new float2(authoring.PitchMin, authoring.PitchMax),
                PanScale = math.max(authoring.MovementSpeed, authoring.PanSensitivity),
                ZoomSpeed = authoring.ZoomSpeed,
                MinDistance = math.max(0.01f, authoring.ZoomMin),
                MaxDistance = math.max(authoring.ZoomMin, authoring.ZoomMax),
                TerrainClearance = math.max(0f, authoring.TerrainClearance),
                CollisionBuffer = math.max(0f, authoring.CollisionBuffer),
                CloseOrbitSensitivity = math.max(0f, authoring.SensitivityClose),
                FarOrbitSensitivity = math.max(0f, authoring.SensitivityFar),
                SmoothingDamping = 0f
            });

            AddComponent(entity, new CameraState
            {
                LastUpdateTick = 0,
                PlayerId = 0,
                TargetPosition = initialPosition,
                TargetForward = forward,
                TargetUp = up,
                PivotPosition = pivot,
                Distance = distance,
                Pitch = math.degrees(math.asin(forward.y)),
                Yaw = math.degrees(math.atan2(forward.x, forward.z)),
                FOV = 60f,
                IsOrbiting = 0,
                IsPanning = 0,
                PanPlaneHeight = initialPosition.y
            });

            AddComponent(entity, new CameraInputState
            {
                SampleTick = 0,
                PlayerId = 0,
                OrbitDelta = float2.zero,
                PanDelta = float2.zero,
                ZoomDelta = 0f,
                PointerPosition = float2.zero,
                PointerOverUI = 0,
                AppHasFocus = 1,
                MoveInput = float2.zero,
                VerticalMove = 0f,
                YAxisUnlocked = 0,
                ToggleYAxisTriggered = 0
            });

            AddBuffer<CameraInputEdge>(entity);
            AddComponent(entity, new GodIntent());
            AddComponent(entity, LocalTransform.FromPositionRotationScale(initialPosition, initialRotation, 1f));
        }
    }
}

