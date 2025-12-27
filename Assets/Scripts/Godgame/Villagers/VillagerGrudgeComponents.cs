using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Types of offenses that can generate grudges.
    /// </summary>
    public enum GrudgeOffenseType : byte
    {
        None = 0,
        KilledFriend = 1,
        KilledFamily = 2,
        StolenProperty = 3,
        Betrayal = 4,
        Assault = 5,
        Humiliation = 6,
        BrokenPromise = 7,
        DestroyedHome = 8,
        Abandonment = 9,
        Slander = 10
    }

    /// <summary>
    /// Buffer element tracking a grudge against another entity.
    /// Grudges decay over time based on villager's VengefulScore.
    /// See Docs/Concepts/Villagers/Villager_Behavioral_Personality.md
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct VillagerGrudge : IBufferElementData
    {
        /// <summary>
        /// The entity who wronged this villager.
        /// Can be Entity.Null if target is dead/unknown.
        /// </summary>
        public Entity Target;

        /// <summary>
        /// Type of offense that generated this grudge.
        /// </summary>
        public GrudgeOffenseType OffenseType;

        /// <summary>
        /// Current intensity of the grudge (decays over time).
        /// Higher values = stronger desire for revenge.
        /// Typical range: 0-100, but can exceed for severe offenses.
        /// </summary>
        public float Intensity;

        /// <summary>
        /// Game tick when the offense occurred.
        /// </summary>
        public uint OccurredTick;

        /// <summary>
        /// Number of times villager has attempted retaliation.
        /// </summary>
        public byte RetaliationAttempts;

        /// <summary>
        /// If true, grudge has been satisfied (revenge taken or forgiven).
        /// Will be cleaned up by decay system.
        /// </summary>
        public byte IsResolved;

        /// <summary>
        /// Gets the base severity for an offense type.
        /// </summary>
        public static float GetBaseSeverity(GrudgeOffenseType type)
        {
            return type switch
            {
                GrudgeOffenseType.KilledFamily => 100f,
                GrudgeOffenseType.KilledFriend => 80f,
                GrudgeOffenseType.DestroyedHome => 70f,
                GrudgeOffenseType.Betrayal => 60f,
                GrudgeOffenseType.Assault => 50f,
                GrudgeOffenseType.StolenProperty => 40f,
                GrudgeOffenseType.BrokenPromise => 30f,
                GrudgeOffenseType.Abandonment => 25f,
                GrudgeOffenseType.Humiliation => 20f,
                GrudgeOffenseType.Slander => 15f,
                _ => 10f
            };
        }

        /// <summary>
        /// Creates a new grudge with intensity calculated from offense severity and behavior.
        /// </summary>
        public static VillagerGrudge Create(
            in Entity target,
            GrudgeOffenseType offenseType,
            uint currentTick,
            in VillagerBehavior behavior)
        {
            var baseSeverity = GetBaseSeverity(offenseType);
            var intensityMultiplier = behavior.GetGrudgeIntensityMultiplier();

            return new VillagerGrudge
            {
                Target = target,
                OffenseType = offenseType,
                Intensity = baseSeverity * intensityMultiplier,
                OccurredTick = currentTick,
                RetaliationAttempts = 0,
                IsResolved = 0
            };
        }
    }

    /// <summary>
    /// Singleton configuration for grudge system parameters.
    /// </summary>
    public struct GrudgeSystemConfig : IComponentData
    {
        /// <summary>
        /// Minimum intensity below which grudges are removed.
        /// </summary>
        public float MinIntensityThreshold;

        /// <summary>
        /// Maximum grudges a single villager can hold.
        /// Oldest/weakest grudges are removed when exceeded.
        /// </summary>
        public byte MaxGrudgesPerVillager;

        /// <summary>
        /// How many game ticks equal one "day" for decay purposes.
        /// </summary>
        public uint TicksPerDay;

        /// <summary>
        /// If true, forgiving villagers (VengefulScore < -40) instantly forgive minor offenses.
        /// </summary>
        public byte ForgivingInstantForgiveness;

        /// <summary>
        /// Threshold below which forgiving villagers instantly forgive.
        /// </summary>
        public float InstantForgivenessThreshold;

        public static GrudgeSystemConfig Default => new GrudgeSystemConfig
        {
            MinIntensityThreshold = 1f,
            MaxGrudgesPerVillager = 8,
            TicksPerDay = 1200, // Assuming 20 ticks/second, 60 seconds/day
            ForgivingInstantForgiveness = 1,
            InstantForgivenessThreshold = 20f
        };
    }

    /// <summary>
    /// Event buffer for newly generated grudges (for telemetry/AI).
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct GrudgeEvent : IBufferElementData
    {
        public Entity Holder;
        public Entity Target;
        public GrudgeOffenseType OffenseType;
        public float Intensity;
        public uint Tick;
        public byte WasInstantlyForgiven;
    }
}
