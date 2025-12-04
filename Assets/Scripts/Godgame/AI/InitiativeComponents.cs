using Unity.Entities;

namespace Godgame.AI
{
    /// <summary>
    /// Initiative bands that determine action pacing.
    /// From Docs/Concepts/Core/Sandbox_Autonomous_Villages.md
    /// </summary>
    public enum InitiativeBand : byte
    {
        /// <summary>Slow: TickBudget=8, RecoveryHalfLife=16</summary>
        Slow = 0,
        /// <summary>Measured: TickBudget=5, RecoveryHalfLife=12</summary>
        Measured = 1,
        /// <summary>Bold: TickBudget=3, RecoveryHalfLife=8</summary>
        Bold = 2,
        /// <summary>Reckless: TickBudget=2, RecoveryHalfLife=6</summary>
        Reckless = 3
    }

    /// <summary>
    /// Stress states that modify initiative behavior.
    /// </summary>
    public enum InitiativeStressState : byte
    {
        Calm = 0,
        Rally = 1,
        Frenzy = 2,
        Panic = 3
    }

    /// <summary>
    /// Per-villager initiative tracking.
    /// Controls how frequently villagers make autonomous decisions.
    /// </summary>
    public struct VillagerInitiative : IComponentData
    {
        /// <summary>
        /// Current initiative score (0-100). Higher = more likely to act.
        /// </summary>
        public float CurrentInitiative;

        /// <summary>
        /// Base initiative derived from personality (Bold/Craven).
        /// </summary>
        public float BaseInitiative;

        /// <summary>
        /// Current initiative band determining action pacing.
        /// </summary>
        public InitiativeBand Band;

        /// <summary>
        /// Current stress state affecting behavior.
        /// </summary>
        public InitiativeStressState StressState;

        /// <summary>
        /// Ticks until next major action window opens.
        /// When 0, villager can make autonomous decisions.
        /// </summary>
        public ushort TicksUntilAction;

        /// <summary>
        /// Last tick when initiative was recalculated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Number of actions taken this day (resets at dawn).
        /// </summary>
        public byte ActionsToday;

        /// <summary>
        /// Creates default initiative state.
        /// </summary>
        public static VillagerInitiative Default => new VillagerInitiative
        {
            CurrentInitiative = 50f,
            BaseInitiative = 50f,
            Band = InitiativeBand.Measured,
            StressState = InitiativeStressState.Calm,
            TicksUntilAction = 0,
            LastUpdateTick = 0,
            ActionsToday = 0
        };

        /// <summary>
        /// Gets the tick budget for this initiative band.
        /// </summary>
        public byte GetTickBudget()
        {
            return Band switch
            {
                InitiativeBand.Slow => 8,
                InitiativeBand.Measured => 5,
                InitiativeBand.Bold => 3,
                InitiativeBand.Reckless => 2,
                _ => 5
            };
        }

        /// <summary>
        /// Gets the recovery half-life (ticks) for this initiative band.
        /// </summary>
        public byte GetRecoveryHalfLife()
        {
            return Band switch
            {
                InitiativeBand.Slow => 16,
                InitiativeBand.Measured => 12,
                InitiativeBand.Bold => 8,
                InitiativeBand.Reckless => 6,
                _ => 12
            };
        }
    }

    /// <summary>
    /// Global initiative configuration singleton.
    /// Defines thresholds for initiative bands and stress transitions.
    /// </summary>
    public struct InitiativeConfig : IComponentData
    {
        /// <summary>
        /// Initiative threshold for Slow band (below this = Slow).
        /// </summary>
        public float SlowThreshold;

        /// <summary>
        /// Initiative threshold for Bold band (above this = Bold).
        /// </summary>
        public float BoldThreshold;

        /// <summary>
        /// Initiative threshold for Reckless band (above this = Reckless).
        /// </summary>
        public float RecklessThreshold;

        /// <summary>
        /// Stress level that triggers Panic state.
        /// </summary>
        public float PanicThreshold;

        /// <summary>
        /// Stress level that triggers Rally state.
        /// </summary>
        public float RallyThreshold;

        /// <summary>
        /// Stress level that triggers Frenzy state.
        /// </summary>
        public float FrenzyThreshold;

        /// <summary>
        /// Base initiative recovery rate per tick.
        /// </summary>
        public float RecoveryRate;

        /// <summary>
        /// Initiative boost from being in combat.
        /// </summary>
        public float CombatBoost;

        /// <summary>
        /// Initiative penalty from low mood.
        /// </summary>
        public float LowMoodPenalty;

        /// <summary>
        /// Initiative boost from active grudges.
        /// </summary>
        public float GrudgeBoostPerIntensity;

        /// <summary>
        /// Creates default configuration based on concept doc.
        /// </summary>
        public static InitiativeConfig Default => new InitiativeConfig
        {
            SlowThreshold = 30f,
            BoldThreshold = 60f,
            RecklessThreshold = 85f,
            PanicThreshold = -40f,
            RallyThreshold = 35f,
            FrenzyThreshold = 70f,
            RecoveryRate = 0.5f,
            CombatBoost = 15f,
            LowMoodPenalty = 20f,
            GrudgeBoostPerIntensity = 0.1f
        };
    }

    /// <summary>
    /// Tag indicating an entity can perform an autonomous action this tick.
    /// Added by InitiativeSystem when TicksUntilAction reaches 0.
    /// </summary>
    public struct ReadyToActTag : IComponentData { }
}

