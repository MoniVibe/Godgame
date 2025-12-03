using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Behavioral personality axes for villagers (Vengeful↔Forgiving, Bold↔Craven).
    /// Values range from -100 to +100.
    /// </summary>
    public struct VillagerPersonality : IComponentData
    {
        /// <summary>
        /// Vengeful (-100) ↔ Forgiving (+100)
        /// </summary>
        public sbyte VengefulScore;

        /// <summary>
        /// Bold (+100) ↔ Craven (-100)
        /// </summary>
        public sbyte BoldScore;
    }

    /// <summary>
    /// Initiative state tracking for autonomous villager decisions.
    /// </summary>
    public struct VillagerInitiativeState : IComponentData
    {
        /// <summary>
        /// Current initiative value (0.0 to 1.0)
        /// </summary>
        public float CurrentInitiative;

        /// <summary>
        /// Base initiative band (Slow, Measured, Bold, Reckless)
        /// </summary>
        public byte InitiativeBand;

        /// <summary>
        /// Ticks until next initiative check/action
        /// </summary>
        public uint TicksUntilNextAction;

        /// <summary>
        /// Stress level affecting initiative swings
        /// </summary>
        public sbyte StressLevel;
    }

    /// <summary>
    /// Patriotism stat measuring attachment to current settlement/band.
    /// Influences willingness to answer conscription, stay on station, or defect.
    /// </summary>
    public struct VillagerPatriotism : IComponentData
    {
        /// <summary>
        /// Patriotism value (0-100, where 100 = absolute loyalty)
        /// </summary>
        public byte Value;

        /// <summary>
        /// Decay rate per tick
        /// </summary>
        public float DecayRate;
    }

    /// <summary>
    /// Active grudge tracking for vengeful villagers.
    /// </summary>
    public struct VillagerGrudge : IComponentData
    {
        /// <summary>
        /// Grudge intensity (0-100)
        /// </summary>
        public byte Intensity;

        /// <summary>
        /// Target entity (villager, village, band) that wronged this villager
        /// </summary>
        public Entity TargetEntity;

        /// <summary>
        /// Decay rate per day
        /// </summary>
        public float DecayRate;

        /// <summary>
        /// Tick when grudge was generated
        /// </summary>
        public uint GeneratedTick;
    }

    /// <summary>
    /// Alignment values for individual villagers (tri-axis: Moral, Order, Purity).
    /// Each axis ranges from -100 to +100.
    /// </summary>
    public struct VillagerAlignment : IComponentData
    {
        /// <summary>
        /// Moral axis: Good (+100) ↔ Neutral (0) ↔ Evil (-100)
        /// </summary>
        public sbyte MoralAxis;

        /// <summary>
        /// Order axis: Lawful (+100) ↔ Neutral (0) ↔ Chaotic (-100)
        /// </summary>
        public sbyte OrderAxis;

        /// <summary>
        /// Purity axis: Pure (+100) ↔ Neutral (0) ↔ Corrupt (-100)
        /// </summary>
        public sbyte PurityAxis;
    }

    /// <summary>
    /// Outlook slots for villagers (up to 3 regular or 2 fanatic).
    /// </summary>
    public struct VillagerOutlook : IComponentData
    {
        /// <summary>
        /// Outlook type IDs (Materialistic, Warlike, Spiritual, etc.)
        /// </summary>
        public FixedList32Bytes<byte> OutlookTypes;

        /// <summary>
        /// Outlook values (-100 to +100) corresponding to OutlookTypes
        /// </summary>
        public FixedList32Bytes<sbyte> OutlookValues;

        /// <summary>
        /// Flags indicating fanatic outlooks
        /// </summary>
        public byte FanaticFlags;
    }

    /// <summary>
    /// Tag component indicating this villager is undead.
    /// Used for various effects (healing, resurrection, etc.).
    /// </summary>
    public struct UndeadTag : IComponentData
    {
    }

    /// <summary>
    /// Tag component indicating this villager is summoned.
    /// Used for various effects (duration, dismissal, etc.).
    /// </summary>
    public struct SummonedTag : IComponentData
    {
    }
}

