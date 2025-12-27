using Godgame.Runtime;
using PureDOTS.Input;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Runtime.Miracles;
using Unity.Entities;
using Unity.Mathematics;
using GodgameDivineHandState = Godgame.Runtime.DivineHandState;
using GodgameHandState = Godgame.Runtime.HandState;

namespace Godgame.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct DivineHandBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            Entity handEntity;

            if (!SystemAPI.TryGetSingletonEntity<DivineHandTag>(out handEntity))
            {
                handEntity = entityManager.CreateEntity();
                entityManager.AddComponent<DivineHandTag>(handEntity);
            }

            EnsureDivineHandComponents(entityManager, handEntity);
        }

        private static void EnsureDivineHandComponents(EntityManager entityManager, Entity handEntity)
        {
            if (!entityManager.HasComponent<DivineHandConfig>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new DivineHandConfig
                {
                    PickupRadius = 8f,
                    MaxGrabDistance = 12f,
                    HoldLerp = 0.25f,
                    ThrowImpulse = 25f,
                    ThrowChargeMultiplier = 12.5f,
                    HoldHeightOffset = 3f,
                    CooldownAfterThrowSeconds = 0.15f,
                    MinChargeSeconds = 0.2f,
                    MaxChargeSeconds = 1.5f,
                    HysteresisFrames = 3,
                    HeldCapacity = 1000,
                    SiphonRate = 150f,
                    DumpRate = 150f,
                    MinThrowSpeed = 15f,
                    MaxThrowSpeed = 35f
                });
            }

            if (!entityManager.HasComponent<GodgameDivineHandState>(handEntity))
            {
                var cursor = new float3(0f, 3f, 0f);
                entityManager.AddComponentData(handEntity, new GodgameDivineHandState
                {
                    HeldEntity = Entity.Null,
                    CursorPosition = cursor,
                    AimDirection = new float3(0f, -1f, 0f),
                    HeldLocalOffset = float3.zero,
                    CurrentState = GodgameHandState.Empty,
                    PreviousState = GodgameHandState.Empty,
                    ChargeTimer = 0f,
                    CooldownTimer = 0f,
                    HeldResourceTypeIndex = DivineHandConstants.NoResourceType,
                    HeldAmount = 0,
                    HeldCapacity = 1000,
                    Flags = 0
                });
            }

            if (!entityManager.HasComponent<PureDOTS.Runtime.Hand.HandState>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new PureDOTS.Runtime.Hand.HandState
                {
                    CurrentState = HandStateType.Idle,
                    PreviousState = HandStateType.Idle,
                    HeldEntity = Entity.Null,
                    HoldPoint = float3.zero,
                    HoldDistance = 0f,
                    ChargeTimer = 0f,
                    CooldownTimer = 0f,
                    StateTimer = 0
                });
            }

            if (!entityManager.HasComponent<HandInteractionState>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new HandInteractionState
                {
                    HandEntity = handEntity,
                    CurrentState = PureDOTS.Runtime.Components.HandState.Idle,
                    PreviousState = PureDOTS.Runtime.Components.HandState.Idle,
                    ActiveCommand = DivineHandCommandType.None,
                    ActiveResourceType = DivineHandConstants.NoResourceType,
                    HeldAmount = 0,
                    HeldCapacity = 1000,
                    CooldownSeconds = 0f,
                    LastUpdateTick = 0,
                    Flags = 0
                });
            }

            if (!entityManager.HasComponent<ResourceSiphonState>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new ResourceSiphonState
                {
                    HandEntity = handEntity,
                    TargetEntity = Entity.Null,
                    ResourceTypeIndex = DivineHandConstants.NoResourceType,
                    SiphonRate = 150f,
                    DumpRate = 150f,
                    AccumulatedUnits = 0f,
                    LastUpdateTick = 0,
                    Flags = 0
                });
            }

            if (!entityManager.HasComponent<DivineHandInput>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new DivineHandInput
                {
                    SampleTick = 0,
                    PlayerId = 0,
                    PointerPosition = float2.zero,
                    PointerDelta = float2.zero,
                    CursorWorldPosition = float3.zero,
                    AimDirection = new float3(0f, -1f, 0f),
                    PrimaryHeld = 0,
                    SecondaryHeld = 0,
                    ThrowCharge = 0f,
                    PointerOverUI = 0,
                    AppHasFocus = 1,
                    QueueModifierHeld = 0,
                    ReleaseSingleTriggered = 0,
                    ReleaseAllTriggered = 0,
                    ToggleThrowModeTriggered = 0,
                    ThrowModeIsSlingshot = 1
                });
            }

            if (!entityManager.HasComponent<GodIntent>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new GodIntent());
            }

            if (!entityManager.HasComponent<HandHover>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new HandHover
                {
                    TargetEntity = Entity.Null,
                    HitPosition = float3.zero,
                    HitNormal = new float3(0f, 1f, 0f),
                    Distance = float.MaxValue
                });
            }

            if (!entityManager.HasComponent<HandAffordances>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new HandAffordances
                {
                    Flags = HandAffordanceFlags.None,
                    TargetEntity = Entity.Null,
                    ResourceTypeIndex = DivineHandConstants.NoResourceType
                });
            }

            if (!entityManager.HasComponent<DivineHandCommand>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new DivineHandCommand
                {
                    Type = DivineHandCommandType.None,
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    TargetNormal = new float3(0f, 1f, 0f),
                    TimeSinceIssued = 0f
                });
            }

            if (!entityManager.HasComponent<HandInputRouteResult>(handEntity))
            {
                entityManager.AddComponentData(handEntity, HandInputRouteResult.None);
            }

            if (!entityManager.HasBuffer<DivineHandEvent>(handEntity))
            {
                entityManager.AddBuffer<DivineHandEvent>(handEntity);
            }

            if (!entityManager.HasBuffer<HandInputRouteRequest>(handEntity))
            {
                entityManager.AddBuffer<HandInputRouteRequest>(handEntity);
            }

            if (!entityManager.HasBuffer<Godgame.Runtime.HandQueuedThrowElement>(handEntity))
            {
                entityManager.AddBuffer<Godgame.Runtime.HandQueuedThrowElement>(handEntity);
            }

            if (!entityManager.HasBuffer<HandCommand>(handEntity))
            {
                entityManager.AddBuffer<HandCommand>(handEntity);
            }

            if (!entityManager.HasBuffer<MiracleReleaseEvent>(handEntity))
            {
                entityManager.AddBuffer<MiracleReleaseEvent>(handEntity);
            }

            if (!entityManager.HasBuffer<MiracleSlotDefinition>(handEntity))
            {
                entityManager.AddBuffer<MiracleSlotDefinition>(handEntity);
            }

            if (!entityManager.HasComponent<MiracleCasterState>(handEntity))
            {
                entityManager.AddComponentData(handEntity, new MiracleCasterState
                {
                    HandEntity = handEntity,
                    SelectedSlot = 0,
                    SustainedCastHeld = 0,
                    ThrowCastTriggered = 0
                });
            }
        }
    }
}
