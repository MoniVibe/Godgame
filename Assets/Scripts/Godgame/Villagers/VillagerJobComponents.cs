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
