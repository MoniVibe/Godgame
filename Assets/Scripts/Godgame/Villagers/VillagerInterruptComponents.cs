using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Interrupt types that can override villager job state machine.
    /// </summary>
    public enum VillagerInterruptType : byte
    {
        None = 0,
        HandPickup = 1,      // Divine hand grabbed the villager
        PathBlocked = 2,     // Navigation path is blocked
        Combat = 3,          // Combat engagement
        Flee = 4,            // Fleeing from danger
        Miracle = 5          // Miracle effect override
    }

    /// <summary>
    /// Tracks active interrupt state for a villager.
    /// When present, the interrupt system should override normal job behavior.
    /// </summary>
    public struct VillagerInterrupt : IComponentData
    {
        public VillagerInterruptType Type;
        public Entity SourceEntity;      // Entity that triggered the interrupt (hand, enemy, etc.)
        public uint StartTick;           // When interrupt began
        public float Duration;           // Expected duration (0 = indefinite)
    }

    /// <summary>
    /// Stores the job state before interrupt so it can be resumed.
    /// </summary>
    public struct VillagerInterruptState : IComponentData
    {
        public VillagerJob.JobPhase SavedPhase;
        public VillagerJob.JobType SavedType;
        public Entity SavedTargetEntity;
        public uint SavedTicketId;
    }

    /// <summary>
    /// Tag component indicating villager is currently being carried by divine hand.
    /// </summary>
    public struct HandCarriedTag : IComponentData
    {
    }

    /// <summary>
    /// Tag component indicating villager's path is blocked and needs resolution.
    /// </summary>
    public struct PathBlockedTag : IComponentData
    {
        public uint BlockedTick;
        public float3 BlockedPosition;
    }
}

