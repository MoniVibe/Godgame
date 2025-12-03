using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Core village entity component.
    /// </summary>
    public struct Village : IComponentData
    {
        public int VillageId;
        public FixedString64Bytes VillageName;
    }

    /// <summary>
    /// Village state machine (Nascent, Established, Ascendant, Collapsing).
    /// </summary>
    public struct VillageState : IComponentData
    {
        public VillageStateType CurrentState;
        public uint StateEntryTick;
        public float SurplusThreshold;
    }

    public enum VillageStateType : byte
    {
        Nascent = 0,
        Established = 1,
        Ascendant = 2,
        Collapsing = 3
    }

    /// <summary>
    /// Aggregate alignment state for village (derived from members).
    /// </summary>
    public struct VillageAlignmentState : IComponentData
    {
        public sbyte MoralAxis;
        public sbyte OrderAxis;
        public sbyte PurityAxis;
        public byte DominantOutlookId;
    }

    /// <summary>
    /// Village initiative state (band, tick budget, stress).
    /// </summary>
    public struct VillageInitiativeState : IComponentData
    {
        public float CurrentInitiative;
        public byte InitiativeBand;
        public uint TicksUntilNextProject;
        public sbyte StressLevel;
    }

    /// <summary>
    /// Village resource summary (food, construction, specialty).
    /// </summary>
    public struct VillageResourceSummary : IComponentData
    {
        public float FoodStored;
        public float FoodUpkeep;
        public float ConstructionStored;
        public float ConstructionUpkeep;
        public float SpecialtyStored;
        public float SpecialtyUpkeep;
    }

    /// <summary>
    /// Village morale and worship metrics.
    /// </summary>
    public struct VillageWorshipState : IComponentData
    {
        public float AverageMorale;
        public float WorshipIntensity;
        public float FaithAverage;
        public float ManaGenerationRate;
        public int OutstandingPrayerCount;
    }

    /// <summary>
    /// Village member tracking (villagers belonging to this village).
    /// </summary>
    public struct VillageMember : IBufferElementData
    {
        public Entity VillagerEntity;
    }

    /// <summary>
    /// Village event buffer for deterministic event history.
    /// </summary>
    public struct VillageEvent : IBufferElementData
    {
        public uint EventId;
        public uint TriggerTick;
        public uint Seed;
        public float Magnitude;
        public byte EventFamily;
        public byte AffectedAxesMask;
    }

    public enum VillageEventFamily : byte
    {
        Seasonal = 0,
        Social = 1,
        Crisis = 2,
        Miracle = 3,
        Threat = 4
    }
}

