using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// Source of damage to buildings.
    /// </summary>
    public enum DamageSource : byte
    {
        None = 0,
        Raid = 1,
        Weather = 2,
        Earthquake = 3,
        Neglect = 4,
        Sabotage = 5,
        Fire = 6,
        Age = 7,
        Divine = 8,
        Combat = 9
    }

    /// <summary>
    /// Durability status thresholds.
    /// See Docs/Concepts/Buildings/Building_Durability_System.md
    /// </summary>
    public enum DurabilityStatus : byte
    {
        /// <summary>100-75% - No penalty</summary>
        Stable = 0,
        /// <summary>75-50% - Minor penalties (-5% output, -10% defense)</summary>
        LightDamage = 1,
        /// <summary>50-25% - Significant penalties (-20% output, -25% defense)</summary>
        ModerateDamage = 2,
        /// <summary>25-0% - Building offline/unusable</summary>
        Critical = 3
    }

    /// <summary>
    /// Building durability component tracking structural integrity.
    /// Damage triggers penalty thresholds; critical damage makes building unusable.
    /// </summary>
    public struct BuildingDurability : IComponentData
    {
        /// <summary>
        /// Current durability points.
        /// </summary>
        public float Current;

        /// <summary>
        /// Maximum durability (defines 100%).
        /// </summary>
        public float Max;

        /// <summary>
        /// Current status tier based on durability percentage.
        /// </summary>
        public DurabilityStatus Status;

        /// <summary>
        /// Most recent source of damage.
        /// </summary>
        public DamageSource LastDamageSource;

        /// <summary>
        /// Output penalty multiplier (0-1, where 1 = no penalty).
        /// Applied to production, housing capacity, aura strength.
        /// </summary>
        public float OutputMultiplier;

        /// <summary>
        /// Defense penalty multiplier (0-1, where 1 = no penalty).
        /// </summary>
        public float DefenseMultiplier;

        /// <summary>
        /// Last game tick when damage was taken.
        /// </summary>
        public uint LastDamageTick;

        /// <summary>
        /// Durability percentage (0-1).
        /// </summary>
        public float Percentage => Max > 0 ? Current / Max : 0f;

        /// <summary>
        /// Whether building is usable (not critical).
        /// </summary>
        public bool IsUsable => Status != DurabilityStatus.Critical;

        /// <summary>
        /// Apply damage and update status.
        /// </summary>
        public void TakeDamage(float amount, DamageSource source, uint tick)
        {
            if (amount <= 0) return;

            Current = Current - amount;
            if (Current < 0) Current = 0;

            LastDamageSource = source;
            LastDamageTick = tick;

            UpdateStatus();
        }

        /// <summary>
        /// Repair durability (capped at max).
        /// </summary>
        /// <param name="amount">Amount to repair</param>
        /// <param name="skillCap">Maximum percentage repairable (0-1). Apprentice=0.7, Master=1.0</param>
        public void Repair(float amount, float skillCap = 1.0f)
        {
            if (amount <= 0) return;

            float maxRepairable = Max * skillCap;
            Current = Current + amount;
            if (Current > maxRepairable) Current = maxRepairable;

            UpdateStatus();
        }

        /// <summary>
        /// Update status and multipliers based on current percentage.
        /// </summary>
        public void UpdateStatus()
        {
            float pct = Percentage;

            if (pct >= 0.75f)
            {
                Status = DurabilityStatus.Stable;
                OutputMultiplier = 1.0f;
                DefenseMultiplier = 1.0f;
            }
            else if (pct >= 0.50f)
            {
                Status = DurabilityStatus.LightDamage;
                OutputMultiplier = 0.95f;  // -5% output
                DefenseMultiplier = 0.90f; // -10% defense
            }
            else if (pct >= 0.25f)
            {
                Status = DurabilityStatus.ModerateDamage;
                OutputMultiplier = 0.80f;  // -20% output
                DefenseMultiplier = 0.75f; // -25% defense
            }
            else
            {
                Status = DurabilityStatus.Critical;
                OutputMultiplier = 0.0f;   // Offline
                DefenseMultiplier = 0.50f; // -50% defense (still has some)
            }
        }

        /// <summary>
        /// Create default durability for a building.
        /// </summary>
        public static BuildingDurability Default(float maxDurability = 1000f)
        {
            return new BuildingDurability
            {
                Current = maxDurability,
                Max = maxDurability,
                Status = DurabilityStatus.Stable,
                LastDamageSource = DamageSource.None,
                OutputMultiplier = 1.0f,
                DefenseMultiplier = 1.0f,
                LastDamageTick = 0
            };
        }
    }

    /// <summary>
    /// Building quality tier affecting base durability and decay rate.
    /// Higher quality = higher durability, slower decay.
    /// </summary>
    public enum BuildingQuality : byte
    {
        Poor = 0,      // 0.7x durability, 1.5x decay
        Common = 1,    // 1.0x durability, 1.0x decay
        Fine = 2,      // 1.2x durability, 0.8x decay
        Superior = 3,  // 1.5x durability, 0.6x decay
        Masterwork = 4 // 2.0x durability, 0.4x decay
    }

    /// <summary>
    /// Optional quality component for buildings with enhanced durability.
    /// </summary>
    public struct BuildingQualityData : IComponentData
    {
        public BuildingQuality Quality;

        /// <summary>
        /// Durability multiplier based on quality.
        /// </summary>
        public float DurabilityMultiplier => Quality switch
        {
            BuildingQuality.Poor => 0.7f,
            BuildingQuality.Common => 1.0f,
            BuildingQuality.Fine => 1.2f,
            BuildingQuality.Superior => 1.5f,
            BuildingQuality.Masterwork => 2.0f,
            _ => 1.0f
        };

        /// <summary>
        /// Decay rate multiplier based on quality.
        /// </summary>
        public float DecayMultiplier => Quality switch
        {
            BuildingQuality.Poor => 1.5f,
            BuildingQuality.Common => 1.0f,
            BuildingQuality.Fine => 0.8f,
            BuildingQuality.Superior => 0.6f,
            BuildingQuality.Masterwork => 0.4f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Component tracking ongoing fire damage to a building.
    /// Fire persists until extinguished by rain, water miracle, etc.
    /// </summary>
    public struct BuildingOnFire : IComponentData
    {
        /// <summary>
        /// Damage per second from fire.
        /// </summary>
        public float DamagePerSecond;

        /// <summary>
        /// When fire started (game tick).
        /// </summary>
        public uint StartTick;

        /// <summary>
        /// Accumulated fire damage this frame (for spread calculation).
        /// </summary>
        public float AccumulatedDamage;
    }

    /// <summary>
    /// Tag for buildings that need repair.
    /// Added when durability drops below threshold, removed when repaired.
    /// </summary>
    public struct NeedsRepairTag : IComponentData { }

    /// <summary>
    /// Configuration for building durability system.
    /// </summary>
    public struct BuildingDurabilityConfig : IComponentData
    {
        /// <summary>
        /// Natural decay rate per day (percentage of max).
        /// Default: 0.1% per day
        /// </summary>
        public float NaturalDecayRatePerDay;

        /// <summary>
        /// Fire damage multiplier.
        /// </summary>
        public float FireDamageMultiplier;

        /// <summary>
        /// Durability threshold to add NeedsRepairTag.
        /// </summary>
        public float RepairThreshold;

        public static BuildingDurabilityConfig Default => new BuildingDurabilityConfig
        {
            NaturalDecayRatePerDay = 0.001f,  // 0.1% per day
            FireDamageMultiplier = 1.0f,
            RepairThreshold = 0.75f
        };
    }
}

