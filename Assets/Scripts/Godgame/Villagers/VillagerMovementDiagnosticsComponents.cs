using PureDOTS.Runtime.AI;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Per-entity diagnostics for headless movement validation.
    /// </summary>
    public struct VillagerMovementDiagnosticsState : IComponentData
    {
        public float3 LastPosition;
        public float LastGoalDistance;
        public uint LastTick;
        public int StuckTicks;
        public float MaxSpeed;
        public float MaxTeleport;
        public int StateFlipCount;
        public MoveIntentType LastIntentType;
    }

    /// <summary>
    /// Aggregate counters for movement validation.
    /// </summary>
    public struct VillagerMovementDiagnosticsCounters : IComponentData
    {
        public int NaNCount;
        public int SpeedClampCount;
        public int TeleportCount;
        public int StuckCount;
    }

    [InternalBufferCapacity(16)]
    public struct MovementTickTrace : IBufferElementData
    {
        public uint Tick;
        public float3 Position;
        public float Speed;
        public MoveIntentType IntentType;
    }

    public enum VillagerMovementFailureReason : byte
    {
        None = 0,
        Speed = 1,
        Teleport = 2,
        Stuck = 3,
        StateFlip = 4,
        NaN = 5
    }

    public struct VillagerMovementDiagnosticsFailure : IComponentData
    {
        public VillagerMovementFailureReason Reason;
        public Entity Offender;
        public uint Tick;
        public byte Reported;
    }
}
