using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Simplified job type enum for villager job system.
    /// </summary>
    public enum JobType : byte
    {
        None = 0,
        Gather = 1
    }

    /// <summary>
    /// Job phase enum for state machine transitions.
    /// </summary>
    public enum JobPhase : byte
    {
        Idle = 0,
        NavigateToNode = 1,
        Gather = 2,
        NavigateToStorehouse = 3,
        Deliver = 4
    }

    public enum VillagerJobFailCode : byte
    {
        None = 0,
        NoTicket = 1,
        TicketInvalid = 2,
        TicketCancelled = 3,
        TargetInvalid = 4,
        TargetDepleted = 5,
        RoleMismatch = 6,
        ResourceMismatch = 7,
        NoStorehouse = 8,
        StorehouseFull = 9,
        ReservationDenied = 10,
        DepositFailed = 11,
        Cooldown = 12,
        Timeout = 13
    }

    [InternalBufferCapacity(16)]
    public struct VillagerJobDecisionEvent : IBufferElementData
    {
        public uint Tick;
        public JobType JobType;
        public JobPhase Phase;
        public VillagerJobFailCode FailCode;
        public Entity Target;
        public Entity CandidateA;
        public Entity CandidateB;
        public Entity CandidateC;
        public float ScoreA;
        public float ScoreB;
        public float ScoreC;
        public byte PreconditionMask;
    }

    /// <summary>
    /// Villager job state component tracking current job assignment and progress.
    /// </summary>
    public struct VillagerJobState : IComponentData
    {
        public JobType Type;
        public JobPhase Phase;
        public Entity Target;
        public ushort ResourceTypeIndex;
        public ushort OutputResourceTypeIndex;
        public float CarryCount;
        public float CarryMax;
        public float DropoffCooldown;
        public float DecisionCooldown;
        public uint LastDecisionTick;
        public JobType LastChosenJob;
        public Entity LastTarget;
        public VillagerJobFailCode LastFailCode;
        public byte RepeatCount;
        public uint LastFailTick;
        public uint NextEligibleTick;
        public uint NextReplanTick;
    }

    public struct VillagerJob : IComponentData
    {
        public enum JobType : byte
        {
            None = 0,
            Gatherer = 1
        }

        public enum JobPhase : byte
        {
            Idle = 0,
            Working = 1
        }

        public JobType Type;
        public JobPhase Phase;
        public int ActiveTicketId;
        public float Productivity;
    }

    /// <summary>
    /// Navigation component for movement toward a destination.
    /// </summary>
    public struct Navigation : IComponentData
    {
        public float3 Destination;
        public float Speed;
        public float3 Velocity;
    }
}
