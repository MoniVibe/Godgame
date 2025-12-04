using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Focus ability archetypes based on primary stat or profession.
    /// Combat archetypes (1-9), Profession archetypes (10-19).
    /// </summary>
    public enum FocusArchetype : byte
    {
        None = 0,

        // Combat archetypes (stat-based)
        Finesse = 1,    // High dexterity/agility - precision combat
        Physique = 2,   // High strength/constitution - power combat
        Arcane = 3,     // High intelligence/wisdom - magic combat

        // Profession archetypes (skill-based)
        Crafting = 10,  // Blacksmiths, carpenters, weavers - quality vs quantity
        Gathering = 11, // Miners, woodcutters, foragers - speed vs efficiency
        Healing = 12,   // Healers, herbalists, medics - multi vs intensive
        Teaching = 13,  // Researchers, mentors, instructors - breadth vs depth
        Refining = 14   // Smelters, tanners, alchemists - waste vs time
    }

    /// <summary>
    /// Types of focus abilities available.
    /// Abilities are grouped by category for unlock gating.
    /// Combat: 10-69, Crafting: 70-89, Gathering: 90-109, Healing: 110-129, Teaching: 130-149, Refining: 150-169
    /// </summary>
    public enum FocusAbilityType : byte
    {
        None = 0,

        // ===== COMBAT: Finesse Abilities (dexterity/agility) 10-29 =====
        /// <summary>Block incoming attacks with counter-attack window</summary>
        Parry = 10,
        /// <summary>Attack twice as fast with dual-wield weapons</summary>
        DualWieldStrike = 11,
        /// <summary>+50% crit chance against single target</summary>
        CriticalFocus = 12,
        /// <summary>+40% dodge rating while active</summary>
        DodgeBoost = 13,
        /// <summary>Hit multiple adjacent targets in sequence</summary>
        MultiStrike = 14,
        /// <summary>Rapid consecutive shots for ranged</summary>
        RapidFire = 15,
        /// <summary>Multi-target ranged attack</summary>
        MultiShot = 16,
        /// <summary>High-damage precision headshot</summary>
        CriticalHeadshot = 17,

        // ===== COMBAT: Physique Abilities (strength/constitution) 30-49 =====
        /// <summary>Increase pain threshold, reduce incoming damage 25%</summary>
        IgnorePain = 30,
        /// <summary>Arc attack hitting all enemies in front</summary>
        SweepAttack = 31,
        /// <summary>+30% attack speed while active</summary>
        AttackSpeedBoost = 32,
        /// <summary>Heavy hit that ignores 50% armor</summary>
        Cleave = 33,
        /// <summary>Charge through enemies, knocking them aside</summary>
        BullRush = 34,
        /// <summary>Ground slam dealing AOE damage</summary>
        GroundSlam = 35,
        /// <summary>Temporary invulnerability but cannot attack</summary>
        Fortify = 36,

        // ===== COMBAT: Arcane Abilities (intelligence/wisdom) 50-69 =====
        /// <summary>+2 to summon cap while active</summary>
        SummonBoost = 50,
        /// <summary>+100% mana regeneration while active</summary>
        ManaRegen = 51,
        /// <summary>-50% cooldown on abilities while active</summary>
        CooldownReduction = 52,
        /// <summary>Cast same spell twice (burst cost)</summary>
        Multicast = 53,
        /// <summary>+50% duration on buffs/debuffs while active</summary>
        BuffExtend = 54,
        /// <summary>+40% spell damage/healing while active</summary>
        EmpowerCast = 55,
        /// <summary>Channel to restore mana rapidly (vulnerable)</summary>
        ManaChannel = 56,
        /// <summary>Instant cast next spell (burst cost)</summary>
        QuickCast = 57,

        // ===== CRAFTING Abilities (blacksmiths, carpenters, etc.) 70-89 =====
        /// <summary>+100% craft speed, -30% quality</summary>
        MassProduction = 70,
        /// <summary>+50% quality, -50% speed</summary>
        MasterworkFocus = 71,
        /// <summary>Craft 3 items at once (burst)</summary>
        BatchCrafting = 72,
        /// <summary>+25% quality, no speed penalty</summary>
        PrecisionWork = 73,
        /// <summary>+75% durability on crafted items</summary>
        Reinforce = 74,
        /// <summary>-40% material cost, slightly lower quality</summary>
        EfficientCrafting = 75,
        /// <summary>Chance to craft bonus item</summary>
        Inspiration = 76,
        /// <summary>Learn from crafting, +50% XP gain</summary>
        StudiedCrafting = 77,

        // ===== GATHERING Abilities (miners, woodcutters, foragers) 90-109 =====
        /// <summary>+50% gather rate, +20% waste</summary>
        SpeedGather = 90,
        /// <summary>-30% waste, -20% speed</summary>
        EfficientGather = 91,
        /// <summary>3x gather for 10 seconds (burst)</summary>
        GatherOverdrive = 92,
        /// <summary>Reveal hidden/rare resources nearby</summary>
        ResourceSense = 93,
        /// <summary>+100% yield from rich nodes</summary>
        DeepExtraction = 94,
        /// <summary>Chance to find rare materials</summary>
        LuckyFind = 95,
        /// <summary>Gather without depleting node (limited)</summary>
        SustainableHarvest = 96,
        /// <summary>Gather multiple node types simultaneously</summary>
        Multitasking = 97,

        // ===== HEALING Abilities (healers, herbalists, medics) 110-129 =====
        /// <summary>Heal multiple targets at 50% effectiveness</summary>
        MassHeal = 110,
        /// <summary>Prevent single target from dying</summary>
        LifeClutch = 111,
        /// <summary>Remove ailments and debuffs</summary>
        PurifyingFocus = 112,
        /// <summary>Passive heal over time to nearby allies</summary>
        RegenerationAura = 113,
        /// <summary>+100% healing effectiveness on single target</summary>
        IntensiveCare = 114,
        /// <summary>Heal wounds that would be permanent</summary>
        MiracleHealing = 115,
        /// <summary>Transfer own health to target</summary>
        LifeTransfer = 116,
        /// <summary>Stabilize critically injured (no death for duration)</summary>
        Triage = 117,

        // ===== TEACHING Abilities (researchers, mentors, instructors) 130-149 =====
        /// <summary>+100% XP transfer, half duration</summary>
        IntensiveLessons = 130,
        /// <summary>+50% skill retention, 2x duration</summary>
        DeepTeaching = 131,
        /// <summary>Teach multiple students at 60% effectiveness</summary>
        GroupInstruction = 132,
        /// <summary>+20% learning speed for all nearby</summary>
        InspiringPresence = 133,
        /// <summary>Student gains practical experience bonus</summary>
        HandsOnTraining = 134,
        /// <summary>Unlock hidden potential in student</summary>
        TalentDiscovery = 135,
        /// <summary>Share memories/knowledge directly (draining)</summary>
        MindLink = 136,
        /// <summary>Research breakthroughs come faster</summary>
        Eureka = 137,

        // ===== REFINING Abilities (smelters, tanners, alchemists) 150-169 =====
        /// <summary>+50% refine speed, +25% waste</summary>
        RapidRefine = 150,
        /// <summary>-50% waste, -30% speed</summary>
        PureExtraction = 151,
        /// <summary>Process 3x materials at once (burst)</summary>
        BatchRefine = 152,
        /// <summary>+15% output purity/quality</summary>
        QualityControl = 153,
        /// <summary>Recover materials from failed/waste products</summary>
        Reclamation = 154,
        /// <summary>Create rare byproducts during refining</summary>
        Transmutation = 155,
        /// <summary>Combine incompatible materials</summary>
        Synthesis = 156,
        /// <summary>Refine while maintaining material special properties</summary>
        GentleProcessing = 157
    }

    /// <summary>
    /// How focus is consumed by an ability.
    /// </summary>
    public enum FocusCostType : byte
    {
        /// <summary>One-time cost when activated</summary>
        Burst = 0,
        /// <summary>Continuous drain while active</summary>
        PerSecond = 1,
        /// <summary>Cost per use/attack while toggled on</summary>
        PerUse = 2
    }

    /// <summary>
    /// Entity's focus resource pool.
    /// Focus is a mental battery that enables powerful combat abilities.
    /// </summary>
    public struct EntityFocus : IComponentData
    {
        /// <summary>
        /// Current focus available (0 to MaxFocus).
        /// </summary>
        public float CurrentFocus;

        /// <summary>
        /// Maximum focus capacity. Influenced by Will stat.
        /// </summary>
        public float MaxFocus;

        /// <summary>
        /// Base regeneration rate per second. Influenced by Wisdom.
        /// </summary>
        public float BaseRegenRate;

        /// <summary>
        /// Current regeneration rate (modified by state/buffs).
        /// </summary>
        public float CurrentRegenRate;

        /// <summary>
        /// Total drain rate from all active abilities.
        /// </summary>
        public float TotalDrainRate;

        /// <summary>
        /// Exhaustion level (0-100). Increases when focus is low.
        /// At 100 + 0 focus = coma.
        /// </summary>
        public byte ExhaustionLevel;

        /// <summary>
        /// True if entity is in focus-depletion coma.
        /// </summary>
        public bool IsInComa;

        /// <summary>
        /// True if entity is at risk of mental breakdown.
        /// </summary>
        public bool IsBreakdownRisk;

        /// <summary>
        /// Primary focus archetype based on stats.
        /// </summary>
        public FocusArchetype PrimaryArchetype;

        /// <summary>
        /// Tick when focus state was last updated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Creates default focus state.
        /// </summary>
        public static EntityFocus Default => new EntityFocus
        {
            CurrentFocus = 100f,
            MaxFocus = 100f,
            BaseRegenRate = 2f,
            CurrentRegenRate = 2f,
            TotalDrainRate = 0f,
            ExhaustionLevel = 0,
            IsInComa = false,
            IsBreakdownRisk = false,
            PrimaryArchetype = FocusArchetype.None,
            LastUpdateTick = 0
        };

        /// <summary>
        /// Creates focus state based on entity stats.
        /// </summary>
        public static EntityFocus FromStats(byte will, byte wisdom, byte finesse, byte physique, byte intelligence)
        {
            // Max focus scales with Will (50-150 range)
            float maxFocus = 50f + (will * 1f);

            // Regen rate scales with Wisdom (1-5 range)
            float regenRate = 1f + (wisdom * 0.04f);

            // Determine primary archetype from highest stat
            FocusArchetype archetype = FocusArchetype.None;
            var maxStatInt = math.max((int)finesse, (int)physique);
            maxStatInt = math.max(maxStatInt, (int)intelligence);
            byte maxStat = (byte)maxStatInt;
            if (maxStat >= 50)
            {
                if (finesse == maxStat) archetype = FocusArchetype.Finesse;
                else if (physique == maxStat) archetype = FocusArchetype.Physique;
                else if (intelligence == maxStat) archetype = FocusArchetype.Arcane;
            }

            return new EntityFocus
            {
                CurrentFocus = maxFocus,
                MaxFocus = maxFocus,
                BaseRegenRate = regenRate,
                CurrentRegenRate = regenRate,
                TotalDrainRate = 0f,
                ExhaustionLevel = 0,
                IsInComa = false,
                IsBreakdownRisk = false,
                PrimaryArchetype = archetype,
                LastUpdateTick = 0
            };
        }

        /// <summary>
        /// Gets focus as a percentage (0-1).
        /// </summary>
        public float FocusPercent => MaxFocus > 0 ? CurrentFocus / MaxFocus : 0f;

        /// <summary>
        /// Returns true if entity can use focus abilities.
        /// </summary>
        public bool CanUseFocus => !IsInComa && CurrentFocus > 0f;
    }

    /// <summary>
    /// Currently active focus ability on an entity.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct ActiveFocusAbility : IBufferElementData
    {
        /// <summary>
        /// Type of ability active.
        /// </summary>
        public FocusAbilityType AbilityType;

        /// <summary>
        /// How this ability costs focus.
        /// </summary>
        public FocusCostType CostType;

        /// <summary>
        /// Focus drain rate (per second for PerSecond, per use for PerUse).
        /// </summary>
        public float DrainRate;

        /// <summary>
        /// Remaining duration in seconds (0 = permanent until deactivated).
        /// </summary>
        public float RemainingDuration;

        /// <summary>
        /// Magnitude/effectiveness of the ability effect.
        /// </summary>
        public float EffectMagnitude;

        /// <summary>
        /// Tick when ability was activated.
        /// </summary>
        public uint ActivatedTick;

        /// <summary>
        /// True if ability is toggled on (vs one-shot).
        /// </summary>
        public bool IsToggle;
    }

    /// <summary>
    /// Request to activate a focus ability.
    /// Processed by FocusAbilitySystem.
    /// </summary>
    public struct FocusAbilityRequest : IComponentData
    {
        /// <summary>
        /// Ability to activate.
        /// </summary>
        public FocusAbilityType RequestedAbility;

        /// <summary>
        /// Target entity (if applicable).
        /// </summary>
        public Entity TargetEntity;

        /// <summary>
        /// Tick when request was made.
        /// </summary>
        public uint RequestTick;

        /// <summary>
        /// Whether to toggle off if already active.
        /// </summary>
        public bool ToggleOff;
    }

    /// <summary>
    /// Combat stats that influence focus abilities.
    /// Can be used standalone or alongside existing stat systems.
    /// </summary>
    public struct CombatStats : IComponentData
    {
        /// <summary>Physical strength (0-100)</summary>
        public byte Physique;
        /// <summary>Dexterity/agility (0-100)</summary>
        public byte Finesse;
        /// <summary>Mental acuity (0-100)</summary>
        public byte Intelligence;
        /// <summary>Mental fortitude (0-100)</summary>
        public byte Will;
        /// <summary>Insight/perception (0-100)</summary>
        public byte Wisdom;

        /// <summary>
        /// Creates default combat stats.
        /// </summary>
        public static CombatStats Default => new CombatStats
        {
            Physique = 50,
            Finesse = 50,
            Intelligence = 50,
            Will = 50,
            Wisdom = 50
        };
    }

    /// <summary>
    /// Tag indicating entity is in focus coma and incapacitated.
    /// </summary>
    public struct FocusComaTag : IComponentData
    {
        /// <summary>
        /// Tick when coma started.
        /// </summary>
        public uint ComaStartTick;

        /// <summary>
        /// Minimum ticks before recovery is possible.
        /// </summary>
        public uint MinRecoveryTicks;
    }

    /// <summary>
    /// Tag indicating entity is at risk of mental breakdown.
    /// </summary>
    public struct FocusBreakdownRiskTag : IComponentData
    {
        /// <summary>
        /// Breakdown probability per check (0-100).
        /// </summary>
        public byte BreakdownChance;

        /// <summary>
        /// Tick when risk state started.
        /// </summary>
        public uint RiskStartTick;
    }

    /// <summary>
    /// Global focus system configuration.
    /// </summary>
    public struct FocusConfig : IComponentData
    {
        /// <summary>
        /// Focus percentage below which exhaustion starts accumulating.
        /// </summary>
        public float ExhaustionThreshold;

        /// <summary>
        /// Exhaustion accumulation rate per second when below threshold.
        /// </summary>
        public float ExhaustionAccumulationRate;

        /// <summary>
        /// Exhaustion recovery rate per second when focus is above threshold.
        /// </summary>
        public float ExhaustionRecoveryRate;

        /// <summary>
        /// Exhaustion level that triggers breakdown risk.
        /// </summary>
        public byte BreakdownRiskThreshold;

        /// <summary>
        /// Regen multiplier when entity is idle/resting.
        /// </summary>
        public float IdleRegenMultiplier;

        /// <summary>
        /// Regen multiplier when entity is in combat.
        /// </summary>
        public float CombatRegenMultiplier;

        /// <summary>
        /// Minimum ticks in coma before recovery check.
        /// </summary>
        public uint MinComaDuration;

        /// <summary>
        /// Creates default configuration.
        /// </summary>
        public static FocusConfig Default => new FocusConfig
        {
            ExhaustionThreshold = 0.2f,
            ExhaustionAccumulationRate = 5f,
            ExhaustionRecoveryRate = 2f,
            BreakdownRiskThreshold = 80,
            IdleRegenMultiplier = 2f,
            CombatRegenMultiplier = 0.5f,
            MinComaDuration = 300 // ~10 seconds at 30 TPS
        };
    }

    /// <summary>
    /// Why an entity chooses to push their focus usage.
    /// Most entities only use high focus when motivated.
    /// </summary>
    public enum FocusMotivation : byte
    {
        /// <summary>No strong motivation - uses focus casually (~20-40%)</summary>
        Casual = 0,
        /// <summary>Enjoys relaxation/socializing - rarely uses focus (~10-30%)</summary>
        Leisurely = 1,
        /// <summary>Life is in danger - will use 100% if needed</summary>
        Survival = 2,
        /// <summary>Strong ambition/goals driving them (~60-90%)</summary>
        Ambitious = 3,
        /// <summary>Burning passion for their work (~70-100%)</summary>
        Passionate = 4,
        /// <summary>Duty-bound (soldiers, guards) - uses as required (~50-80%)</summary>
        Dutiful = 5,
        /// <summary>Desperate circumstances (debt, threats) (~80-100%)</summary>
        Desperate = 6,
        /// <summary>Pride/perfectionism - won't accept mediocre work (~60-90%)</summary>
        Perfectionist = 7
    }

    /// <summary>
    /// Tracks focus usage over time for XP/wisdom growth calculation.
    /// Entities who push themselves grow faster.
    /// </summary>
    public struct FocusGrowthTracking : IComponentData
    {
        /// <summary>
        /// Total focus spent (lifetime). Influences wisdom gain.
        /// </summary>
        public float TotalFocusSpent;

        /// <summary>
        /// Focus spent this day. Resets at dawn.
        /// </summary>
        public float DailyFocusSpent;

        /// <summary>
        /// Average focus usage intensity (0-1) over recent period.
        /// Higher = entity pushes harder.
        /// </summary>
        public float AverageIntensity;

        /// <summary>
        /// Peak focus intensity reached today (0-1).
        /// </summary>
        public float PeakIntensityToday;

        /// <summary>
        /// Days where entity used >50% focus capacity.
        /// </summary>
        public ushort HighEffortDays;

        /// <summary>
        /// Days where entity barely used focus (<20%).
        /// </summary>
        public ushort LowEffortDays;

        /// <summary>
        /// Current motivation driving focus usage.
        /// </summary>
        public FocusMotivation CurrentMotivation;

        /// <summary>
        /// Base personality tendency for focus usage (0-1).
        /// 0 = lazy/leisurely, 1 = driven/ambitious.
        /// </summary>
        public float DrivePersonality;

        /// <summary>
        /// Tick when tracking was last updated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Creates default tracking with moderate drive.
        /// </summary>
        public static FocusGrowthTracking Default => new FocusGrowthTracking
        {
            TotalFocusSpent = 0f,
            DailyFocusSpent = 0f,
            AverageIntensity = 0.3f,
            PeakIntensityToday = 0f,
            HighEffortDays = 0,
            LowEffortDays = 0,
            CurrentMotivation = FocusMotivation.Casual,
            DrivePersonality = 0.5f,
            LastUpdateTick = 0
        };

        /// <summary>
        /// Gets target focus usage percentage based on motivation and personality.
        /// </summary>
        public float GetTargetUsagePercent()
        {
            // Base from personality (20% to 70%)
            float baseUsage = 0.2f + (DrivePersonality * 0.5f);

            // Motivation modifier
            float motivationMod = CurrentMotivation switch
            {
                FocusMotivation.Leisurely => 0.6f,      // Reduces usage
                FocusMotivation.Casual => 0.8f,         // Slight reduction
                FocusMotivation.Dutiful => 1.1f,        // Slight increase
                FocusMotivation.Ambitious => 1.4f,      // Strong increase
                FocusMotivation.Passionate => 1.6f,     // Very strong
                FocusMotivation.Perfectionist => 1.5f,  // Strong
                FocusMotivation.Survival => 2.0f,       // Maximum when life threatened
                FocusMotivation.Desperate => 1.8f,      // Near maximum
                _ => 1.0f
            };

            return Unity.Mathematics.math.clamp(baseUsage * motivationMod, 0.1f, 1.0f);
        }
    }

    /// <summary>
    /// Configuration for focus-based growth bonuses.
    /// </summary>
    public struct FocusGrowthConfig : IComponentData
    {
        /// <summary>
        /// XP multiplier per focus point spent (e.g., 0.01 = 1% XP per focus).
        /// </summary>
        public float XPPerFocusSpent;

        /// <summary>
        /// Wisdom gain per 1000 lifetime focus spent.
        /// </summary>
        public float WisdomPer1000Focus;

        /// <summary>
        /// Bonus XP multiplier for high-effort days (>50% capacity).
        /// </summary>
        public float HighEffortXPBonus;

        /// <summary>
        /// XP penalty multiplier for low-effort days (<20% capacity).
        /// </summary>
        public float LowEffortXPPenalty;

        /// <summary>
        /// Daily focus threshold for "high effort" (0-1).
        /// </summary>
        public float HighEffortThreshold;

        /// <summary>
        /// Daily focus threshold for "low effort" (0-1).
        /// </summary>
        public float LowEffortThreshold;

        /// <summary>
        /// Max wisdom gain achievable from focus usage.
        /// </summary>
        public byte MaxWisdomFromFocus;

        /// <summary>
        /// Creates default growth configuration.
        /// </summary>
        public static FocusGrowthConfig Default => new FocusGrowthConfig
        {
            XPPerFocusSpent = 0.02f,           // 2% XP per focus point
            WisdomPer1000Focus = 1f,           // +1 Wisdom per 1000 focus spent
            HighEffortXPBonus = 1.25f,         // +25% XP on high-effort days
            LowEffortXPPenalty = 0.75f,        // -25% XP on lazy days
            HighEffortThreshold = 0.5f,        // 50% of max focus used
            LowEffortThreshold = 0.2f,         // 20% of max focus used
            MaxWisdomFromFocus = 30            // Cap at +30 wisdom from focus
        };
    }

    /// <summary>
    /// Static helpers for focus growth calculations.
    /// </summary>
    public static class FocusGrowthHelpers
    {
        /// <summary>
        /// Calculates XP bonus from focus spent on an action.
        /// </summary>
        public static uint CalculateFocusXPBonus(float focusSpent, float baseXP, in FocusGrowthConfig config)
        {
            float bonus = focusSpent * config.XPPerFocusSpent * baseXP;
            return (uint)Unity.Mathematics.math.max(0, bonus);
        }

        /// <summary>
        /// Calculates daily XP modifier based on effort level.
        /// </summary>
        public static float GetDailyXPModifier(in FocusGrowthTracking tracking, in EntityFocus focus, in FocusGrowthConfig config)
        {
            float usagePercent = tracking.DailyFocusSpent / focus.MaxFocus;

            if (usagePercent >= config.HighEffortThreshold)
                return config.HighEffortXPBonus;
            else if (usagePercent <= config.LowEffortThreshold)
                return config.LowEffortXPPenalty;

            return 1f; // Normal XP
        }

        /// <summary>
        /// Calculates wisdom gained from lifetime focus usage.
        /// </summary>
        public static byte CalculateWisdomFromFocus(float totalFocusSpent, in FocusGrowthConfig config)
        {
            float wisdom = (totalFocusSpent / 1000f) * config.WisdomPer1000Focus;
            return (byte)Unity.Mathematics.math.min(wisdom, config.MaxWisdomFromFocus);
        }

        /// <summary>
        /// Updates tracking after focus is spent.
        /// </summary>
        public static void RecordFocusSpent(ref FocusGrowthTracking tracking, float amount, float intensity)
        {
            tracking.TotalFocusSpent += amount;
            tracking.DailyFocusSpent += amount;
            tracking.PeakIntensityToday = Unity.Mathematics.math.max(tracking.PeakIntensityToday, intensity);

            // Update rolling average intensity (exponential moving average)
            tracking.AverageIntensity = tracking.AverageIntensity * 0.95f + intensity * 0.05f;
        }

        /// <summary>
        /// Resets daily tracking at dawn. Called by daily routine system.
        /// </summary>
        public static void ResetDailyTracking(ref FocusGrowthTracking tracking, in EntityFocus focus, in FocusGrowthConfig config)
        {
            float usagePercent = tracking.DailyFocusSpent / focus.MaxFocus;

            if (usagePercent >= config.HighEffortThreshold)
                tracking.HighEffortDays++;
            else if (usagePercent <= config.LowEffortThreshold)
                tracking.LowEffortDays++;

            tracking.DailyFocusSpent = 0f;
            tracking.PeakIntensityToday = 0f;
        }

        /// <summary>
        /// Gets descriptive text for an entity's work ethic based on their focus patterns.
        /// </summary>
        public static string GetWorkEthicDescription(in FocusGrowthTracking tracking)
        {
            float ratio = tracking.HighEffortDays > 0
                ? (float)tracking.HighEffortDays / (tracking.HighEffortDays + tracking.LowEffortDays + 1)
                : 0f;

            if (ratio > 0.7f) return "Driven";
            if (ratio > 0.5f) return "Diligent";
            if (ratio > 0.3f) return "Moderate";
            if (ratio > 0.1f) return "Leisurely";
            return "Lazy";
        }
    }
}

