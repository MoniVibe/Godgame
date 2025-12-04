using Unity.Collections;
using Unity.Entities;

namespace Godgame.Progression
{
    /// <summary>
    /// Skill domains that XP can be earned in.
    /// </summary>
    public enum SkillDomain : byte
    {
        Combat = 0,
        Arcane = 1,
        Stealth = 2,
        Divine = 3,
        Crafting = 4,
        Leadership = 5,
        Trade = 6,
        Survival = 7
    }

    /// <summary>
    /// Mastery levels for skills.
    /// </summary>
    public enum SkillMastery : byte
    {
        Novice = 0,     // Just learned
        Apprentice = 1, // Basic competence
        Journeyman = 2, // Solid skill
        Expert = 3,     // High proficiency
        Master = 4,     // Near-peak ability
        Grandmaster = 5 // Legendary mastery
    }

    /// <summary>
    /// Preordained path types for player guidance.
    /// </summary>
    public enum PreordainedPathType : byte
    {
        None = 0,
        DemonSlayer = 1,
        Archmage = 2,
        ShadowAssassin = 3,
        HolyChampion = 4,
        MasterCraftsman = 5,
        WarLeader = 6,
        MerchantPrince = 7,
        Survivor = 8
    }

    /// <summary>
    /// Per-character progression state.
    /// Tracks level, total XP, and available skill points.
    /// </summary>
    public struct CharacterProgression : IComponentData
    {
        /// <summary>
        /// Current character level (1-100).
        /// </summary>
        public byte Level;

        /// <summary>
        /// Total XP accumulated across all domains.
        /// </summary>
        public uint TotalXP;

        /// <summary>
        /// Unspent skill points (earned per level).
        /// </summary>
        public byte AvailableSkillPoints;

        /// <summary>
        /// XP needed to reach next level.
        /// </summary>
        public uint XPToNextLevel;

        /// <summary>
        /// Tick when last level-up occurred.
        /// </summary>
        public uint LastLevelUpTick;

        /// <summary>
        /// Creates default progression state.
        /// </summary>
        public static CharacterProgression Default => new CharacterProgression
        {
            Level = 1,
            TotalXP = 0,
            AvailableSkillPoints = 0,
            XPToNextLevel = 100,
            LastLevelUpTick = 0
        };

        /// <summary>
        /// Calculates XP needed for a given level.
        /// Uses exponential curve: 100 * level^1.5
        /// </summary>
        public static uint CalculateXPForLevel(byte level)
        {
            return (uint)(100f * Unity.Mathematics.math.pow(level, 1.5f));
        }
    }

    /// <summary>
    /// XP tracking per skill domain.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct SkillXP : IBufferElementData
    {
        /// <summary>
        /// The skill domain this XP applies to.
        /// </summary>
        public SkillDomain Domain;

        /// <summary>
        /// Current XP in this domain.
        /// </summary>
        public uint CurrentXP;

        /// <summary>
        /// XP needed to reach next mastery level in this domain.
        /// </summary>
        public uint XPToNextMastery;

        /// <summary>
        /// Current mastery level in this domain.
        /// </summary>
        public SkillMastery Mastery;

        /// <summary>
        /// Creates default skill XP entry.
        /// </summary>
        public static SkillXP CreateForDomain(SkillDomain domain)
        {
            return new SkillXP
            {
                Domain = domain,
                CurrentXP = 0,
                XPToNextMastery = 500,
                Mastery = SkillMastery.Novice
            };
        }
    }

    /// <summary>
    /// Unlocked skill entry.
    /// </summary>
    [InternalBufferCapacity(12)]
    public struct UnlockedSkill : IBufferElementData
    {
        /// <summary>
        /// Unique identifier for this skill.
        /// </summary>
        public FixedString32Bytes SkillId;

        /// <summary>
        /// Domain this skill belongs to.
        /// </summary>
        public SkillDomain Domain;

        /// <summary>
        /// Current mastery level of this specific skill.
        /// </summary>
        public SkillMastery Mastery;

        /// <summary>
        /// Tick when skill was unlocked.
        /// </summary>
        public uint UnlockedTick;

        /// <summary>
        /// XP earned using this skill (tracks individual skill progress).
        /// </summary>
        public uint SkillXP;

        /// <summary>
        /// Whether this skill is currently active/equipped.
        /// </summary>
        public bool IsActive;
    }

    /// <summary>
    /// Player-set preordained path for this character.
    /// AI will prioritize actions that advance this path.
    /// </summary>
    public struct PreordainedPath : IComponentData
    {
        /// <summary>
        /// The path type player has chosen for this character.
        /// </summary>
        public PreordainedPathType PathType;

        /// <summary>
        /// Progress toward path completion (0-100%).
        /// </summary>
        public byte Progress;

        /// <summary>
        /// Whether path is actively being pursued.
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Tick when path was preordained.
        /// </summary>
        public uint PreordainedTick;

        /// <summary>
        /// Primary domain this path focuses on.
        /// </summary>
        public SkillDomain PrimaryDomain;

        /// <summary>
        /// Secondary domain this path uses.
        /// </summary>
        public SkillDomain SecondaryDomain;

        /// <summary>
        /// Creates default (no path) state.
        /// </summary>
        public static PreordainedPath None => new PreordainedPath
        {
            PathType = PreordainedPathType.None,
            Progress = 0,
            IsActive = false,
            PreordainedTick = 0,
            PrimaryDomain = SkillDomain.Combat,
            SecondaryDomain = SkillDomain.Combat
        };

        /// <summary>
        /// Creates a path with domain mappings.
        /// </summary>
        public static PreordainedPath Create(PreordainedPathType type, uint tick)
        {
            var (primary, secondary) = GetDomainsForPath(type);
            return new PreordainedPath
            {
                PathType = type,
                Progress = 0,
                IsActive = true,
                PreordainedTick = tick,
                PrimaryDomain = primary,
                SecondaryDomain = secondary
            };
        }

        /// <summary>
        /// Gets the primary and secondary domains for a path type.
        /// </summary>
        public static (SkillDomain primary, SkillDomain secondary) GetDomainsForPath(PreordainedPathType type)
        {
            return type switch
            {
                PreordainedPathType.DemonSlayer => (SkillDomain.Combat, SkillDomain.Divine),
                PreordainedPathType.Archmage => (SkillDomain.Arcane, SkillDomain.Crafting),
                PreordainedPathType.ShadowAssassin => (SkillDomain.Stealth, SkillDomain.Combat),
                PreordainedPathType.HolyChampion => (SkillDomain.Divine, SkillDomain.Leadership),
                PreordainedPathType.MasterCraftsman => (SkillDomain.Crafting, SkillDomain.Trade),
                PreordainedPathType.WarLeader => (SkillDomain.Leadership, SkillDomain.Combat),
                PreordainedPathType.MerchantPrince => (SkillDomain.Trade, SkillDomain.Leadership),
                PreordainedPathType.Survivor => (SkillDomain.Survival, SkillDomain.Stealth),
                _ => (SkillDomain.Combat, SkillDomain.Combat)
            };
        }
    }

    /// <summary>
    /// Global progression configuration singleton.
    /// </summary>
    public struct ProgressionConfig : IComponentData
    {
        /// <summary>
        /// XP multiplier for preordained path primary domain.
        /// </summary>
        public float PreordainedPrimaryMultiplier;

        /// <summary>
        /// XP multiplier for preordained path secondary domain.
        /// </summary>
        public float PreordainedSecondaryMultiplier;

        /// <summary>
        /// Base XP award per action.
        /// </summary>
        public uint BaseXPPerAction;

        /// <summary>
        /// XP bonus per mastery level.
        /// </summary>
        public float MasteryXPBonus;

        /// <summary>
        /// Skill points awarded per level.
        /// </summary>
        public byte SkillPointsPerLevel;

        /// <summary>
        /// Creates default configuration.
        /// </summary>
        public static ProgressionConfig Default => new ProgressionConfig
        {
            PreordainedPrimaryMultiplier = 1.5f,
            PreordainedSecondaryMultiplier = 1.2f,
            BaseXPPerAction = 10,
            MasteryXPBonus = 0.1f,
            SkillPointsPerLevel = 2
        };
    }

    /// <summary>
    /// Event indicating XP was awarded to an entity.
    /// Can be processed by telemetry or UI systems.
    /// </summary>
    public struct XPAwardEvent : IComponentData
    {
        /// <summary>
        /// Amount of XP awarded.
        /// </summary>
        public uint Amount;

        /// <summary>
        /// Domain the XP was awarded in.
        /// </summary>
        public SkillDomain Domain;

        /// <summary>
        /// Source of the XP (combat, crafting, etc.).
        /// </summary>
        public FixedString32Bytes Source;

        /// <summary>
        /// Tick when XP was awarded.
        /// </summary>
        public uint AwardedTick;
    }
}

