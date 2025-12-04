using Unity.Entities;

namespace Godgame.AI
{
    /// <summary>
    /// Daily milestone phases.
    /// From Docs/Concepts/Core/Sandbox_Autonomous_Villages.md
    /// One in-game day = 8 real-time minutes at normal speed.
    /// </summary>
    public enum DailyPhase : byte
    {
        /// <summary>~minute 1: Wake, breakfast, prep tools</summary>
        Dawn = 0,
        /// <summary>~minute 4: Work peak, market trades, initiative recalc</summary>
        Noon = 1,
        /// <summary>~minute 6: Communal meals, worship, guard rotations</summary>
        Dusk = 2,
        /// <summary>~minute 8: Rest, stealth activities, ambush checks</summary>
        Midnight = 3
    }

    /// <summary>
    /// Sleep archetype determining rest patterns.
    /// </summary>
    public enum SleepArchetype : byte
    {
        /// <summary>Sleep through Nightfall (Dusk→Dawn), default</summary>
        Diurnal = 0,
        /// <summary>Active during Nightfall and Dawn, rest between Dusk and Midnight</summary>
        Nocturnal = 1,
        /// <summary>Short naps staggered across Daylight</summary>
        PolyphasicLight = 2,
        /// <summary>Retire shortly after Dusk, resume work pre-Dawn</summary>
        EarlyRiser = 3
    }

    /// <summary>
    /// Broad day/night state (binary flag for simple checks).
    /// </summary>
    public enum DayNightState : byte
    {
        /// <summary>Dawn to Dusk</summary>
        Daylight = 0,
        /// <summary>Dusk to next Dawn</summary>
        Nightfall = 1
    }

    /// <summary>
    /// Per-villager daily routine state.
    /// </summary>
    public struct VillagerRoutine : IComponentData
    {
        /// <summary>
        /// Current phase of the day.
        /// </summary>
        public DailyPhase CurrentPhase;

        /// <summary>
        /// This villager's sleep archetype.
        /// </summary>
        public SleepArchetype SleepType;

        /// <summary>
        /// Tick when the next phase transition occurs.
        /// </summary>
        public uint NextPhaseTransitionTick;

        /// <summary>
        /// Whether villager is currently sleeping.
        /// </summary>
        public bool IsSleeping;

        /// <summary>
        /// Sleep debt accumulated (affects performance).
        /// </summary>
        public float SleepDebt;

        /// <summary>
        /// Number of meals eaten today.
        /// </summary>
        public byte MealsToday;

        /// <summary>
        /// Whether villager has performed worship today.
        /// </summary>
        public bool HasWorshippedToday;

        /// <summary>
        /// Creates default routine state.
        /// </summary>
        public static VillagerRoutine Default => new VillagerRoutine
        {
            CurrentPhase = DailyPhase.Dawn,
            SleepType = SleepArchetype.Diurnal,
            NextPhaseTransitionTick = 0,
            IsSleeping = false,
            SleepDebt = 0f,
            MealsToday = 0,
            HasWorshippedToday = false
        };

        /// <summary>
        /// Checks if villager should be sleeping based on archetype and phase.
        /// </summary>
        public bool ShouldBeSleeping(DailyPhase phase, DayNightState dayNight)
        {
            return SleepType switch
            {
                SleepArchetype.Diurnal => dayNight == DayNightState.Nightfall,
                SleepArchetype.Nocturnal => phase == DailyPhase.Noon || (dayNight == DayNightState.Daylight && phase != DailyPhase.Dawn),
                SleepArchetype.PolyphasicLight => false, // Handles own napping
                SleepArchetype.EarlyRiser => phase == DailyPhase.Midnight || phase == DailyPhase.Dusk,
                _ => dayNight == DayNightState.Nightfall
            };
        }
    }

    /// <summary>
    /// Global time-of-day state singleton.
    /// Updated by DailyRoutineSystem to track current day phase.
    /// </summary>
    public struct DayTimeState : IComponentData
    {
        /// <summary>
        /// Current phase of the day.
        /// </summary>
        public DailyPhase CurrentPhase;

        /// <summary>
        /// Current day/night state.
        /// </summary>
        public DayNightState DayNight;

        /// <summary>
        /// Current day number (starting from 0).
        /// </summary>
        public uint DayNumber;

        /// <summary>
        /// Tick when current day started.
        /// </summary>
        public uint DayStartTick;

        /// <summary>
        /// Normalized time within current day (0.0 to 1.0).
        /// </summary>
        public float NormalizedDayTime;

        /// <summary>
        /// Creates default time state.
        /// </summary>
        public static DayTimeState Default => new DayTimeState
        {
            CurrentPhase = DailyPhase.Dawn,
            DayNight = DayNightState.Daylight,
            DayNumber = 0,
            DayStartTick = 0,
            NormalizedDayTime = 0f
        };
    }

    /// <summary>
    /// Global daily routine configuration singleton.
    /// </summary>
    public struct RoutineConfig : IComponentData
    {
        /// <summary>
        /// Ticks per in-game day (at normal speed).
        /// Default: 8 real-time minutes = 480 seconds = 14400 ticks at 30 TPS.
        /// </summary>
        public uint TicksPerDay;

        /// <summary>
        /// Ticks from day start to Dawn phase.
        /// </summary>
        public uint DawnStartTick;

        /// <summary>
        /// Ticks from day start to Noon phase.
        /// </summary>
        public uint NoonStartTick;

        /// <summary>
        /// Ticks from day start to Dusk phase.
        /// </summary>
        public uint DuskStartTick;

        /// <summary>
        /// Ticks from day start to Midnight phase.
        /// </summary>
        public uint MidnightStartTick;

        /// <summary>
        /// Sleep requirement per day for diurnal villagers (in ticks).
        /// </summary>
        public uint DiurnalSleepRequired;

        /// <summary>
        /// Expected meals per day.
        /// </summary>
        public byte ExpectedMealsPerDay;

        /// <summary>
        /// Sleep debt penalty to work speed per 100 debt.
        /// </summary>
        public float SleepDebtWorkPenalty;

        /// <summary>
        /// Creates default configuration.
        /// At 30 TPS: 14400 ticks/day, phases at minutes 1, 4, 6, 8.
        /// </summary>
        public static RoutineConfig Default => new RoutineConfig
        {
            TicksPerDay = 14400,        // 8 minutes at 30 TPS
            DawnStartTick = 0,          // Minute 0-1
            NoonStartTick = 7200,       // Minute 4
            DuskStartTick = 10800,      // Minute 6
            MidnightStartTick = 14400,  // Minute 8 (wraps to 0)
            DiurnalSleepRequired = 3600, // ~2 minutes of sleep needed
            ExpectedMealsPerDay = 2,
            SleepDebtWorkPenalty = 0.1f  // 10% penalty per 100 sleep debt
        };
    }

    /// <summary>
    /// Tag indicating a phase transition just occurred.
    /// Systems can query this to trigger phase-specific behaviors.
    /// </summary>
    public struct PhaseTransitionEvent : IComponentData
    {
        /// <summary>
        /// The phase that just started.
        /// </summary>
        public DailyPhase NewPhase;

        /// <summary>
        /// The phase that just ended.
        /// </summary>
        public DailyPhase PreviousPhase;

        /// <summary>
        /// Tick when transition occurred.
        /// </summary>
        public uint TransitionTick;
    }
}

