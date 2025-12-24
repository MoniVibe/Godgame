using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Runtime
{
    /// <summary>
    /// Godgame-specific hand state enum.
    /// </summary>
    public enum HandState : byte
    {
        Empty = 0,
        Holding = 1,
        Dragging = 2,
        SlingshotAim = 3,
        Dumping = 4
    }

    /// <summary>
    /// Configuration for Divine Hand behavior (pickup radius, throw settings, etc.).
    /// </summary>
    public struct DivineHandConfig : IComponentData
    {
        public float PickupRadius;
        public float MaxGrabDistance;
        public float HoldLerp;
        public float ThrowImpulse;
        public float ThrowChargeMultiplier;
        public float HoldHeightOffset;
        public float CooldownAfterThrowSeconds;
        public float MinChargeSeconds;
        public float MaxChargeSeconds;
        public int HysteresisFrames;
        public int HeldCapacity;
        public float SiphonRate;
        public float DumpRate;
        public float MinThrowSpeed;
        public float MaxThrowSpeed;
    }

    /// <summary>
    /// Godgame-specific Divine Hand state with extended fields.
    /// Extends PureDOTS.Runtime.Components.DivineHandState with additional game-specific state.
    /// </summary>
    public struct DivineHandState : IComponentData
    {
        public Entity HeldEntity;
        public float3 CursorPosition;
        public float3 AimDirection;
        public float3 HeldLocalOffset;
        public HandState CurrentState;
        public HandState PreviousState;
        public float ChargeTimer;
        public float CooldownTimer;
        public ushort HeldResourceTypeIndex;
        public int HeldAmount;
        public int HeldCapacity;
        public byte Flags;
    }

    /// <summary>
    /// Types of highlights that can be shown for the Divine Hand.
    /// </summary>
    public enum HandHighlightType : byte
    {
        None = 0,
        Storehouse = 1,
        Pile = 2,
        Draggable = 3,
        Ground = 4
    }

    /// <summary>
    /// Highlight information for the Divine Hand, used for visual feedback.
    /// </summary>
    public struct DivineHandHighlight : IComponentData
    {
        public HandHighlightType Type;
        public Entity TargetEntity;
        public float3 Position;
        public float3 Normal;
    }

    /// <summary>
    /// Element in the queue of throws for the Divine Hand.
    /// Buffer on the hand entity containing pending queued throws.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct HandQueuedThrowElement : IBufferElementData
    {
        public Entity Entity;
        public float3 Direction;
        public float Impulse;
        public float ChargeLevel; // 0..1 normalized charge level
    }
}

