using Godgame.MoveAct;
using PureDOTS.Input;
using PureDOTS.Runtime.Camera;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Seeds headless-friendly input, camera, and band spawn singletons for the Move & Act slice.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct MoveActBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            EnsureBandSpawnConfig(ref state);
            EnsureInputEntities(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
        }

        private void EnsureBandSpawnConfig(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            Entity configEntity;

            if (!SystemAPI.TryGetSingletonEntity<BandSpawnConfig>(out configEntity))
            {
                configEntity = entityManager.CreateEntity(typeof(BandSpawnConfig));
                entityManager.SetComponentData(configEntity, new BandSpawnConfig
                {
                    StartPosition = new float3(0f, 0f, 0f),
                    PositionStep = new float3(4f, 0f, 0f),
                    DefaultFactionId = 0,
                    DefaultMemberCount = 5,
                    DefaultMorale = 50f,
                    DefaultCohesion = 50f,
                    DefaultDiscipline = 50f,
                    DefaultSpacing = 2.25f
                });
            }

            if (!entityManager.HasComponent<BandSpawnState>(configEntity))
            {
                var config = entityManager.GetComponentData<BandSpawnConfig>(configEntity);
                entityManager.AddComponentData(configEntity, new BandSpawnState
                {
                    NextBandId = 1,
                    NextPosition = config.StartPosition
                });
            }

            if (!entityManager.HasComponent<BandSelection>(configEntity))
            {
                entityManager.AddComponentData(configEntity, new BandSelection
                {
                    Selected = Entity.Null,
                    Position = float3.zero,
                    SelectionTick = 0
                });
            }

            if (!entityManager.HasBuffer<BandSpawnRequest>(configEntity))
            {
                entityManager.AddBuffer<BandSpawnRequest>(configEntity);
            }

            if (!entityManager.HasComponent<BandActionHotkeyState>(configEntity))
            {
                entityManager.AddComponentData(configEntity, new BandActionHotkeyState
                {
                    PlayPingRequested = 0
                });
            }
        }

        private void EnsureInputEntities(ref SystemState state)
        {
            var entityManager = state.EntityManager;

            if (!SystemAPI.TryGetSingletonEntity<DivineHandTag>(out var handEntity))
            {
                handEntity = entityManager.CreateEntity(
                    typeof(DivineHandTag),
                    typeof(DivineHandInput),
                    typeof(GodIntent));
                entityManager.SetComponentData(handEntity, new DivineHandInput
                {
                    SampleTick = 0,
                    PlayerId = 0,
                    PointerPosition = float2.zero,
                    PointerDelta = float2.zero,
                    CursorWorldPosition = float3.zero,
                    AimDirection = new float3(0f, 0f, 1f),
                    PrimaryHeld = 0,
                    SecondaryHeld = 0,
                    ThrowCharge = 0f,
                    PointerOverUI = 0,
                    AppHasFocus = 1,
                    QueueModifierHeld = 0,
                    ReleaseSingleTriggered = 0,
                    ReleaseAllTriggered = 0,
                    ToggleThrowModeTriggered = 0,
                    ThrowModeIsSlingshot = 0
                });
                entityManager.SetComponentData(handEntity, new GodIntent());
                entityManager.AddBuffer<HandInputEdge>(handEntity);
            }
            else
            {
                if (!entityManager.HasComponent<GodIntent>(handEntity))
                {
                    entityManager.AddComponent<GodIntent>(handEntity);
                }

                if (!entityManager.HasBuffer<HandInputEdge>(handEntity))
                {
                    entityManager.AddBuffer<HandInputEdge>(handEntity);
                }
            }

            if (!SystemAPI.TryGetSingletonEntity<PureDOTS.Systems.Input.CameraTag>(out var cameraEntity))
            {
                cameraEntity = entityManager.CreateEntity(
                    typeof(PureDOTS.Systems.Input.CameraTag),
                    typeof(CameraInputState),
                    typeof(PureDOTS.Runtime.Camera.CameraConfig),
                    typeof(PureDOTS.Runtime.Camera.CameraState),
                    typeof(GodIntent));

                var pivot = float3.zero;
                var targetPosition = new float3(0f, 14f, -18f);
                var forward = math.normalizesafe(pivot - targetPosition, new float3(0f, 0f, 1f));

                entityManager.SetComponentData(cameraEntity, new PureDOTS.Runtime.Camera.CameraConfig
                {
                    OrbitYawSensitivity = 1f,
                    OrbitPitchSensitivity = 1f,
                    PitchClamp = new float2(-65f, 80f),
                    PanScale = 6f,
                    ZoomSpeed = 8f,
                    MinDistance = 4f,
                    MaxDistance = 80f,
                    TerrainClearance = 2f,
                    CollisionBuffer = 0.5f,
                    CloseOrbitSensitivity = 1.25f,
                    FarOrbitSensitivity = 0.75f,
                    SmoothingDamping = 0f
                });

                entityManager.SetComponentData(cameraEntity, new PureDOTS.Runtime.Camera.CameraState
                {
                    LastUpdateTick = 0,
                    PlayerId = 0,
                    TargetPosition = targetPosition,
                    TargetForward = forward,
                    TargetUp = new float3(0f, 1f, 0f),
                    PivotPosition = pivot,
                    Distance = math.length(targetPosition - pivot),
                    Pitch = -22.5f,
                    Yaw = 0f,
                    FOV = 60f,
                    IsOrbiting = 0,
                    IsPanning = 0,
                    PanPlaneHeight = targetPosition.y
                });

                entityManager.SetComponentData(cameraEntity, new CameraInputState
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

                entityManager.SetComponentData(cameraEntity, new GodIntent());
                entityManager.AddBuffer<CameraInputEdge>(cameraEntity);
            }
            else
            {
                if (!entityManager.HasComponent<GodIntent>(cameraEntity))
                {
                    entityManager.AddComponent<GodIntent>(cameraEntity);
                }

                if (!entityManager.HasBuffer<CameraInputEdge>(cameraEntity))
                {
                    entityManager.AddBuffer<CameraInputEdge>(cameraEntity);
                }

                if (!entityManager.HasComponent<CameraInputState>(cameraEntity))
                {
                    entityManager.AddComponentData(cameraEntity, new CameraInputState
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
                }
            }
        }
    }
}
