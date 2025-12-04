using Unity.Collections;
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

        public static AggregatePileConfig Default => new AggregatePileConfig
        {
            DefaultMaxCapacity = 2500f,
            GlobalMaxCapacity = 2500f,
            MergeRadius = 2.5f,
            SplitThreshold = 0.8f,
            MergeCheckSeconds = 5f,
            MinSpawnAmount = 10f,
            ConservationEpsilon = 0.01f,
            MaxActivePiles = 200
        };
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
    /// Visual size category for pile rendering.
    /// </summary>
    public enum PileVisualSize : byte
    {
        Tiny = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        Huge = 4
    }

    /// <summary>
    /// Pile state for lifecycle tracking.
    /// </summary>
    public enum PileState : byte
    {
        Stable = 0,
        Growing = 1,
        Shrinking = 2,
        Merging = 3,
        Splitting = 4
    }

    /// <summary>
    /// Runtime state of a single aggregate pile.
    /// </summary>
    public struct AggregatePile : IComponentData
    {
        public FixedString64Bytes ResourceTypeId;
        public ushort ResourceTypeIndex;
        public float Amount;
        public float MaxCapacity;
        public PileState State;
        public uint LastModifiedTick;
        public PileVisualSize VisualSize;

        /// <summary>
        /// Legacy field for compatibility - maps to ResourceTypeIndex.
        /// </summary>
        public ushort ResourceType
        {
            get => ResourceTypeIndex;
            set => ResourceTypeIndex = value;
        }

        /// <summary>
        /// Legacy field for compatibility - maps to MaxCapacity.
        /// </summary>
        public float Capacity
        {
            get => MaxCapacity;
            set => MaxCapacity = value;
        }

        /// <summary>
        /// Check if pile is empty.
        /// </summary>
        public bool IsEmpty => Amount <= 0f;

        /// <summary>
        /// Check if pile is full.
        /// </summary>
        public bool IsFull => Amount >= MaxCapacity;

        /// <summary>
        /// Create a new aggregate pile.
        /// </summary>
        public static AggregatePile Create(in FixedString64Bytes resourceTypeId, ushort resourceTypeIndex, float amount, uint tick)
        {
            var pile = new AggregatePile
            {
                ResourceTypeId = resourceTypeId,
                ResourceTypeIndex = resourceTypeIndex,
                Amount = amount,
                MaxCapacity = 2500f, // Default capacity
                State = PileState.Stable,
                LastModifiedTick = tick,
                VisualSize = PileVisualSize.Tiny
            };
            pile.UpdateVisualSize();
            return pile;
        }

        /// <summary>
        /// Update visual size based on amount.
        /// </summary>
        public void UpdateVisualSize()
        {
            if (Amount <= 0f)
            {
                VisualSize = PileVisualSize.Tiny;
            }
            else if (Amount < 100f)
            {
                VisualSize = PileVisualSize.Tiny;
            }
            else if (Amount < 500f)
            {
                VisualSize = PileVisualSize.Small;
            }
            else if (Amount < 1000f)
            {
                VisualSize = PileVisualSize.Medium;
            }
            else if (Amount < 2000f)
            {
                VisualSize = PileVisualSize.Large;
            }
            else
            {
                VisualSize = PileVisualSize.Huge;
            }
        }

        /// <summary>
        /// Get visual scale for rendering based on visual size.
        /// </summary>
        public float GetVisualScale()
        {
            return VisualSize switch
            {
                PileVisualSize.Tiny => 0.5f,
                PileVisualSize.Small => 0.75f,
                PileVisualSize.Medium => 1.0f,
                PileVisualSize.Large => 1.5f,
                PileVisualSize.Huge => 2.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Add resources to the pile.
        /// </summary>
        public float Add(float amount, uint tick)
        {
            float spaceAvailable = MaxCapacity - Amount;
            float accepted = math.min(amount, spaceAvailable);
            Amount += accepted;
            LastModifiedTick = tick;
            UpdateVisualSize();
            return accepted;
        }

        /// <summary>
        /// Remove resources from the pile.
        /// </summary>
        public float Remove(float amount, uint tick)
        {
            float available = Amount;
            float removed = math.min(amount, available);
            Amount -= removed;
            LastModifiedTick = tick;
            UpdateVisualSize();
            return removed;
        }
    }

    /// <summary>
    /// Runtime bookkeeping for aggregate piles.
    /// </summary>
    public struct AggregatePileRuntimeState : IComponentData
    {
        public float NextMergeTime;
        public int ActivePiles;
    }

    /// <summary>
    /// Statistics singleton tracking aggregate pile state.
    /// </summary>
    public struct AggregatePileStats : IComponentData
    {
        public int TotalPiles;
        public float TotalResourceAmount;
        public uint LastMergeCheckTick;
    }
}
