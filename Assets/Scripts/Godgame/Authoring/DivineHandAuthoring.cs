using Godgame.Runtime;
using PureDOTS.Input;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Miracles;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Godgame.Physics;

namespace Godgame.Authoring
{
    public class DivineHandAuthoring : MonoBehaviour
    {
        [Header("Capacity & Rates")]
        public int heldCapacity = 1000;
        public float maxCarryMass = 200f;
        public float dumpRatePerSecond = 150f;
        public float siphonRange = 8f;

        [Header("Throw & Aim")]
        public float grabLiftHeight = 3f;
        public float throwScalar = 25f;
        public float cooldownAfterThrowSeconds = 0.15f;
        public float minChargeSeconds = 0.2f;
        public float maxChargeSeconds = 1.5f;
        public float minThrowSpeed = 15f;
        public float maxThrowSpeed = 35f;

        private class Baker : Baker<DivineHandAuthoring>
        {
            public override void Bake(DivineHandAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var cursor = new float3(0f, authoring.grabLiftHeight, 0f);
                var aim = new float3(0f, -1f, 0f);

                AddComponent<DivineHandTag>(entity);
                AddComponent(entity, new DivineHandConfig
                {
                    PickupRadius = math.max(0.1f, authoring.siphonRange),
                    MaxGrabDistance = math.max(authoring.siphonRange, authoring.grabLiftHeight + 1f),
                    HoldLerp = 0.25f,
                    ThrowImpulse = authoring.throwScalar,
                    ThrowChargeMultiplier = math.max(0.1f, authoring.throwScalar * 0.5f),
                    HoldHeightOffset = authoring.grabLiftHeight,
                    CooldownAfterThrowSeconds = math.max(0f, authoring.cooldownAfterThrowSeconds),
                    MinChargeSeconds = math.max(0f, authoring.minChargeSeconds),
                    MaxChargeSeconds = math.max(authoring.minChargeSeconds, authoring.maxChargeSeconds),
                    HysteresisFrames = 3,
                    HeldCapacity = math.max(1, authoring.heldCapacity),
                    SiphonRate = math.max(0f, authoring.dumpRatePerSecond),
                    DumpRate = math.max(0f, authoring.dumpRatePerSecond),
                    MinThrowSpeed = math.max(0.1f, authoring.minThrowSpeed),
                    MaxThrowSpeed = math.max(math.max(0.1f, authoring.minThrowSpeed), authoring.maxThrowSpeed)
                });

                AddComponent(entity, new Godgame.Runtime.DivineHandState
                {
                    HeldEntity = Entity.Null,
                    CursorPosition = cursor,
                    AimDirection = aim,
                    HeldLocalOffset = float3.zero,
                    CurrentState = Godgame.Runtime.HandState.Empty,
                    PreviousState = Godgame.Runtime.HandState.Empty,
                    ChargeTimer = 0f,
                    CooldownTimer = 0f,
                    HeldResourceTypeIndex = DivineHandConstants.NoResourceType,
                    HeldAmount = 0,
                    HeldCapacity = math.max(1, authoring.heldCapacity),
                    Flags = 0
                });

                AddComponent(entity, new HandInteractionState
                {
                    HandEntity = entity,
                    CurrentState = PureDOTS.Runtime.Components.HandState.Idle,
                    PreviousState = PureDOTS.Runtime.Components.HandState.Idle,
                    ActiveCommand = DivineHandCommandType.None,
                    ActiveResourceType = DivineHandConstants.NoResourceType,
                    HeldAmount = 0,
                    HeldCapacity = math.max(1, authoring.heldCapacity),
                    CooldownSeconds = 0f,
                    LastUpdateTick = 0,
                    Flags = 0
                });

                AddComponent(entity, new ResourceSiphonState
                {
                    HandEntity = entity,
                    TargetEntity = Entity.Null,
                    ResourceTypeIndex = DivineHandConstants.NoResourceType,
                    SiphonRate = math.max(0f, authoring.dumpRatePerSecond),
                    DumpRate = math.max(0f, authoring.dumpRatePerSecond),
                    AccumulatedUnits = 0f,
                    LastUpdateTick = 0,
                    Flags = 0
                });

                AddComponent(entity, new DivineHandInput
                {
                    SampleTick = 0,
                    PlayerId = 0,
                    PointerPosition = float2.zero,
                    PointerDelta = float2.zero,
                    CursorWorldPosition = cursor,
                    AimDirection = aim,
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

                AddComponent(entity, new DivineHandCommand
                {
                    Type = DivineHandCommandType.None,
                    TargetEntity = Entity.Null,
                    TargetPosition = cursor,
                    TargetNormal = new float3(0f, 1f, 0f),
                    TimeSinceIssued = 0f
                });

                AddComponent(entity, new Godgame.Runtime.DivineHandHighlight
                {
                    Type = Godgame.Runtime.HandHighlightType.None,
                    TargetEntity = Entity.Null,
                    Position = cursor,
                    Normal = new float3(0f, 1f, 0f)
                });

                AddBuffer<DivineHandEvent>(entity);
                AddBuffer<HandInputRouteRequest>(entity);
                AddComponent(entity, HandInputRouteResult.None);
                AddBuffer<Godgame.Runtime.HandQueuedThrowElement>(entity);
                AddBuffer<MiracleReleaseEvent>(entity);
                AddBuffer<MiracleSlotDefinition>(entity);

                AddComponent(entity, new MiracleCasterState
                {
                    HandEntity = entity,
                    SelectedSlot = 0,
                    SustainedCastHeld = 0,
                    ThrowCastTriggered = 0
                });

                AddComponent(entity, new MiracleRuntimeStateNew
                {
                    SelectedId = MiracleId.None,
                    IsActivating = 0,
                    IsSustained = 0
                });

                AddComponent(entity, new MiracleChargeState
                {
                    Charge01 = 0f,
                    HeldTime = 0f,
                    TierIndex = 0,
                    IsCharging = 0
                });

                AddComponent(entity, new MiracleChargeDisplayData
                {
                    ChargePercent = 0f,
                    CurrentTier = 0,
                    HoldTimeSeconds = 0f,
                    IsCharging = 0
                });

                AddComponent(entity, new MiracleChannelState
                {
                    ActiveEffectEntity = Entity.Null,
                    ChannelingId = MiracleId.None,
                    ChannelStartTime = 0f
                });

                AddComponent(entity, new MiracleCasterSelection
                {
                    SelectedSlot = 0
                });

                AddComponent(entity, new MiracleTargetSolution
                {
                    TargetPoint = cursor,
                    TargetEntity = Entity.Null,
                    Radius = 0f,
                    IsValid = 0,
                    ValidityReason = MiracleTargetValidityReason.NoTargetFound,
                    PreviewArcStart = cursor,
                    PreviewArcEnd = cursor,
                    SelectedMiracleId = MiracleId.None
                });

                AddComponent(entity, new MiraclePreviewData
                {
                    Position = cursor,
                    Radius = 0f,
                    IsValid = 0,
                    ValidityReason = MiracleTargetValidityReason.NoTargetFound,
                    SelectedMiracleId = MiracleId.None
                });

                AddComponent(entity, new MiracleRequestCreationState
                {
                    PreviousIsActivating = 0,
                    PreviousIsSustained = 0
                });

                AddComponent(entity, new MiracleChargeTrackingState
                {
                    PreviousSelectedId = MiracleId.None
                });
            }
        }
    }
}
