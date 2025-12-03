using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Resources
{
    /// <summary>
    /// Configuration for aggregate resource piles (capacity, merge/split thresholds).
    /// </summary>
    public struct AggregatePileConfig : IComponentData
    {
        public float DefaultMaxCapacity;
        public float GlobalMaxCapacity;
        public float MergeRadius;
        public float SplitThreshold;
        public float MergeCheckSeconds;
        public float MinSpawnAmount;
        public float ConservationEpsilon;
        public int MaxActivePiles;
    }

    /// <summary>
    /// Command to spawn or add to an aggregate pile.
    /// </summary>
    public struct AggregatePileSpawnCommand : IBufferElementData
    {
        public ushort ResourceType;
        public float Amount;
        public float3 Position;
    }

    /// <summary>
    /// Runtime state of a single aggregate pile.
    /// </summary>
    public struct AggregatePile : IComponentData
    {
        public ushort ResourceType;
        public float Amount;
        public float Capacity;
    }

    /// <summary>
    /// Runtime bookkeeping for aggregate piles.
    /// </summary>
    public struct AggregatePileRuntimeState : IComponentData
    {
        public float NextMergeTime;
        public int ActivePiles;
    }
}
