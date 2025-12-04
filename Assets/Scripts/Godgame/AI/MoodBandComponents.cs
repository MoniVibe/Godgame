using Unity.Collections;
using Unity.Entities;

namespace Godgame.AI
{
    /// <summary>
    /// Mood bands based on morale level.
    /// From Docs/Concepts/Core/Sandbox_Autonomous_Villages.md
    /// Mood recalculates at Dawn and Midnight milestones.
    /// </summary>
    public enum MoodBand : byte
    {
        /// <summary>Morale less than 200: initiative -40%, breakdown risk, health decay</summary>
        Despair = 0,
        /// <summary>Morale 200-400: work speed -15%, social friction</summary>
        Unhappy = 1,
        /// <summary>Morale 400-600: neutral baseline</summary>
        Stable = 2,
        /// <summary>Morale 600-800: work +10%, faith gain +5%</summary>
        Cheerful = 3,
        /// <summary>Morale greater than 800: initiative +25%, inspire allies, burnout risk</summary>
        Elated = 4
    }

    /// <summary>
    /// Categories of mood modifiers.
    /// </summary>
    public enum MoodModifierCategory : byte
    {
        Needs = 0,          // Food, rest, hygiene
        Environment = 1,    // Weather, lighting, beauty
        Relationships = 2,  // Friends, family, leadership
        Events = 3,         // Miracles, victories, disasters
        Health = 4,         // Injuries, illness, augments
        Work = 5            // Job satisfaction, achievements
    }

    /// <summary>
    /// Per-villager mood state tracking.
    /// </summary>
    public struct VillagerMood : IComponentData
    {
        /// <summary>
        /// Current mood band classification.
        /// </summary>
        public MoodBand Band;

        /// <summary>
        /// Tick when band was last recalculated.
        /// </summary>
        public uint LastBandUpdateTick;

        /// <summary>
        /// Work speed modifier from mood (-0.15 to +0.10).
        /// </summary>
        public float WorkSpeedModifier;

        /// <summary>
        /// Initiative modifier from mood (-0.40 to +0.25).
        /// </summary>
        public float InitiativeModifier;

        /// <summary>
        /// Faith gain modifier from mood.
        /// </summary>
        public float FaithGainModifier;

        /// <summary>
        /// Risk of mental breakdown (0-100).
        /// Only relevant in Despair band.
        /// </summary>
        public byte BreakdownRisk;

        /// <summary>
        /// Risk of burnout (0-100).
        /// Only relevant in Elated band.
        /// </summary>
        public byte BurnoutRisk;

        /// <summary>
        /// Creates default mood state.
        /// </summary>
        public static VillagerMood Default => new VillagerMood
        {
            Band = MoodBand.Stable,
            LastBandUpdateTick = 0,
            WorkSpeedModifier = 0f,
            InitiativeModifier = 0f,
            FaithGainModifier = 0f,
            BreakdownRisk = 0,
            BurnoutRisk = 0
        };

        /// <summary>
        /// Recalculates modifiers based on current band.
        /// </summary>
        public void RecalculateModifiers()
        {
            switch (Band)
            {
                case MoodBand.Despair:
                    WorkSpeedModifier = -0.20f;
                    InitiativeModifier = -0.40f;
                    FaithGainModifier = -0.10f;
                    BreakdownRisk = 25;
                    BurnoutRisk = 0;
                    break;
                case MoodBand.Unhappy:
                    WorkSpeedModifier = -0.15f;
                    InitiativeModifier = -0.10f;
                    FaithGainModifier = 0f;
                    BreakdownRisk = 5;
                    BurnoutRisk = 0;
                    break;
                case MoodBand.Stable:
                    WorkSpeedModifier = 0f;
                    InitiativeModifier = 0f;
                    FaithGainModifier = 0f;
                    BreakdownRisk = 0;
                    BurnoutRisk = 0;
                    break;
                case MoodBand.Cheerful:
                    WorkSpeedModifier = 0.10f;
                    InitiativeModifier = 0.10f;
                    FaithGainModifier = 0.05f;
                    BreakdownRisk = 0;
                    BurnoutRisk = 0;
                    break;
                case MoodBand.Elated:
                    WorkSpeedModifier = 0.15f;
                    InitiativeModifier = 0.25f;
                    FaithGainModifier = 0.10f;
                    BreakdownRisk = 0;
                    BurnoutRisk = 15;
                    break;
            }
        }
    }

    /// <summary>
    /// Active mood modifier affecting a villager.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct MoodModifier : IBufferElementData
    {
        /// <summary>
        /// Unique identifier for stacking rules.
        /// </summary>
        public FixedString32Bytes ModifierId;

        /// <summary>
        /// Category of this modifier.
        /// </summary>
        public MoodModifierCategory Category;

        /// <summary>
        /// Magnitude of the modifier (-100 to +100).
        /// </summary>
        public sbyte Magnitude;

        /// <summary>
        /// Remaining duration in ticks (0 = permanent until removed).
        /// </summary>
        public uint RemainingTicks;

        /// <summary>
        /// Tick when this modifier was applied.
        /// </summary>
        public uint AppliedTick;

        /// <summary>
        /// Half-life for decay in ticks (0 = no decay).
        /// </summary>
        public uint DecayHalfLife;
    }

    /// <summary>
    /// Memory of a significant event affecting mood.
    /// Decays over time based on half-life.
    /// </summary>
    [InternalBufferCapacity(6)]
    public struct MoodMemory : IBufferElementData
    {
        /// <summary>
        /// Type of memory (trauma, triumph, romance, etc.).
        /// </summary>
        public FixedString32Bytes MemoryType;

        /// <summary>
        /// Initial impact magnitude.
        /// </summary>
        public sbyte InitialMagnitude;

        /// <summary>
        /// Current impact magnitude (decays over time).
        /// </summary>
        public sbyte CurrentMagnitude;

        /// <summary>
        /// Tick when memory was formed.
        /// </summary>
        public uint FormedTick;

        /// <summary>
        /// Half-life in ticks (how fast memory fades).
        /// </summary>
        public uint DecayHalfLife;

        /// <summary>
        /// Entity associated with this memory (if any).
        /// </summary>
        public Entity AssociatedEntity;
    }

    /// <summary>
    /// Global mood system configuration singleton.
    /// </summary>
    public struct MoodConfig : IComponentData
    {
        /// <summary>
        /// Morale threshold for Despair band.
        /// </summary>
        public float DespairThreshold;

        /// <summary>
        /// Morale threshold for Unhappy band.
        /// </summary>
        public float UnhappyThreshold;

        /// <summary>
        /// Morale threshold for Cheerful band.
        /// </summary>
        public float CheerfulThreshold;

        /// <summary>
        /// Morale threshold for Elated band.
        /// </summary>
        public float ElatedThreshold;

        /// <summary>
        /// Maximum morale value.
        /// </summary>
        public float MaxMorale;

        /// <summary>
        /// Base breakdown check interval in ticks.
        /// </summary>
        public uint BreakdownCheckInterval;

        /// <summary>
        /// Creates default configuration.
        /// </summary>
        public static MoodConfig Default => new MoodConfig
        {
            DespairThreshold = 200f,
            UnhappyThreshold = 400f,
            CheerfulThreshold = 600f,
            ElatedThreshold = 800f,
            MaxMorale = 1000f,
            BreakdownCheckInterval = 100 // Check every 100 ticks
        };
    }
}

