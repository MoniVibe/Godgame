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
    /// Authoring component for camera controller setup.
    /// Bakes camera settings and initial transform into DOTS singleton components.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraControllerAuthoring : MonoBehaviour
    {
        [Header("RTS/Free-fly Settings")]
        public float movementSpeed = 10f;
        public float rotationSensitivity = 2f;

        [Header("Zoom Settings")]
        public float zoomSpeed = 6f;
        public float zoomMin = 6f;
        public float zoomMax = 220f;

        [Header("Orbital Settings")]
        public float orbitalRotationSpeed = 1f;
        public float panSensitivity = 1f;
        
        [Header("Distance-Scaled Sensitivity")]
        [Tooltip("Sensitivity multiplier for close range (6-20m)")]
        public float sensitivityClose = 1.5f;
        [Tooltip("Sensitivity multiplier for mid range (20-100m)")]
        public float sensitivityMid = 1.0f;
        [Tooltip("Sensitivity multiplier for far range (100-220m)")]
        public float sensitivityFar = 0.6f;

        [Header("Pitch Limits (degrees)")]
        public float pitchMin = -30f;
        public float pitchMax = 85f;

        [Header("Terrain Collision")]
        public float terrainClearance = 2f;
        public float collisionBuffer = 0.4f;

        private class Baker : Baker<CameraControllerAuthoring>
        {
            public override void Bake(CameraControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var worldPos = (float3)authoring.transform.position;
                var worldRot = authoring.transform.rotation;
                var forward = math.mul(worldRot, new float3(0f, 0f, 1f));
                var up = math.mul(worldRot, new float3(0f, 1f, 0f));
                var pivot = float3.zero;
                var distance = math.length(worldPos - pivot);

                AddComponent<CameraTag>(entity);
                AddComponent(entity, new CameraConfig
                {
                    OrbitYawSensitivity = authoring.rotationSensitivity,
                    OrbitPitchSensitivity = authoring.rotationSensitivity,
                    PitchClamp = new float2(authoring.pitchMin, authoring.pitchMax),
                    PanScale = math.max(authoring.movementSpeed, authoring.panSensitivity),
                    ZoomSpeed = authoring.zoomSpeed,
                    MinDistance = math.max(0.01f, authoring.zoomMin),
                    MaxDistance = math.max(authoring.zoomMin, authoring.zoomMax),
                    TerrainClearance = math.max(0f, authoring.terrainClearance),
                    CollisionBuffer = math.max(0f, authoring.collisionBuffer),
                    CloseOrbitSensitivity = math.max(0f, authoring.sensitivityClose),
                    FarOrbitSensitivity = math.max(0f, authoring.sensitivityFar),
                    SmoothingDamping = 0f
                });

                AddComponent(entity, new CameraState
                {
                    LastUpdateTick = 0,
                    PlayerId = 0,
                    TargetPosition = worldPos,
                    TargetForward = forward,
                    TargetUp = up,
                    PivotPosition = pivot,
                    Distance = distance,
                    Pitch = math.degrees(math.asin(forward.y)),
                    Yaw = math.degrees(math.atan2(forward.x, forward.z)),
                    FOV = 60f,
                    IsOrbiting = 0,
                    IsPanning = 0,
                    PanPlaneHeight = worldPos.y
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

                AddComponent(entity, LocalTransform.FromPositionRotationScale(
                    worldPos,
                    worldRot,
                    1f));
            }
        }
    }
}

