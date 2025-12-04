using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Profession categories for focus ability gating.
    /// </summary>
    public enum ProfessionType : byte
    {
        None = 0,

        // Crafting professions
        Blacksmith = 10,
        Carpenter = 11,
        Weaver = 12,
        Leatherworker = 13,
        Jeweler = 14,
        Fletcher = 15,
        Armorer = 16,
        Weaponsmith = 17,

        // Gathering professions
        Miner = 30,
        Woodcutter = 31,
        Forager = 32,
        Fisherman = 33,
        Hunter = 34,
        Herbalist = 35,
        Quarrier = 36,

        // Healing professions
        Healer = 50,
        Medic = 51,
        Apothecary = 52,
        Surgeon = 53,
        Priest = 54,
        Shaman = 55,

        // Teaching professions
        Scholar = 70,
        Mentor = 71,
        Instructor = 72,
        Researcher = 73,
        Sage = 74,
        Librarian = 75,

        // Refining professions
        Smelter = 90,
        Tanner = 91,
        Alchemist = 92,
        Brewer = 93,
        Cook = 94,
        Miller = 95,
        Dyer = 96
    }

    /// <summary>
    /// Maps profession types to their focus archetype category.
    /// </summary>
    public static class ProfessionTypeExtensions
    {
        public static FocusArchetype ToFocusArchetype(this ProfessionType profession)
        {
            byte val = (byte)profession;

            if (val >= 10 && val <= 29) return FocusArchetype.Crafting;
            if (val >= 30 && val <= 49) return FocusArchetype.Gathering;
            if (val >= 50 && val <= 69) return FocusArchetype.Healing;
            if (val >= 70 && val <= 89) return FocusArchetype.Teaching;
            if (val >= 90 && val <= 109) return FocusArchetype.Refining;

            return FocusArchetype.None;
        }
    }

    /// <summary>
    /// Skill levels for profession focus abilities.
    /// </summary>
    public struct ProfessionSkills : IComponentData
    {
        /// <summary>Crafting skill level (0-100)</summary>
        public byte CraftingSkill;
        /// <summary>Gathering skill level (0-100)</summary>
        public byte GatheringSkill;
        /// <summary>Healing skill level (0-100)</summary>
        public byte HealingSkill;
        /// <summary>Teaching skill level (0-100)</summary>
        public byte TeachingSkill;
        /// <summary>Refining skill level (0-100)</summary>
        public byte RefiningSkill;

        /// <summary>Primary profession type</summary>
        public ProfessionType PrimaryProfession;
        /// <summary>Secondary profession type (if any)</summary>
        public ProfessionType SecondaryProfession;

        /// <summary>
        /// Gets skill level for a focus archetype.
        /// </summary>
        public byte GetSkillForArchetype(FocusArchetype archetype)
        {
            return archetype switch
            {
                FocusArchetype.Crafting => CraftingSkill,
                FocusArchetype.Gathering => GatheringSkill,
                FocusArchetype.Healing => HealingSkill,
                FocusArchetype.Teaching => TeachingSkill,
                FocusArchetype.Refining => RefiningSkill,
                _ => 0
            };
        }

        /// <summary>
        /// Creates default profession skills.
        /// </summary>
        public static ProfessionSkills Default => new ProfessionSkills
        {
            CraftingSkill = 0,
            GatheringSkill = 0,
            HealingSkill = 0,
            TeachingSkill = 0,
            RefiningSkill = 0,
            PrimaryProfession = ProfessionType.None,
            SecondaryProfession = ProfessionType.None
        };
    }

    /// <summary>
    /// Active modifiers from focus abilities affecting job outcomes.
    /// Applied by ProfessionFocusSystem based on active abilities.
    /// </summary>
    public struct ProfessionFocusModifiers : IComponentData
    {
        /// <summary>
        /// Quality multiplier for crafted/refined items (0.5 to 2.0).
        /// 1.0 = normal quality.
        /// </summary>
        public float QualityMultiplier;

        /// <summary>
        /// Speed multiplier for job actions (0.5 to 3.0).
        /// 1.0 = normal speed.
        /// </summary>
        public float SpeedMultiplier;

        /// <summary>
        /// Waste/efficiency multiplier (0.5 to 1.5).
        /// Lower = less waste. 1.0 = normal waste.
        /// </summary>
        public float WasteMultiplier;

        /// <summary>
        /// Target count multiplier for multi-target abilities (1 to 5).
        /// Used for mass heal, group instruction, etc.
        /// </summary>
        public float TargetCountMultiplier;

        /// <summary>
        /// Effectiveness multiplier per target when multi-targeting (0.3 to 1.0).
        /// Lower when targeting more entities.
        /// </summary>
        public float PerTargetEffectiveness;

        /// <summary>
        /// Chance for bonus output (0 to 0.5).
        /// Used for inspiration, lucky find, etc.
        /// </summary>
        public float BonusChance;

        /// <summary>
        /// XP gain multiplier (0.5 to 2.0).
        /// 1.0 = normal XP gain.
        /// </summary>
        public float XPMultiplier;

        /// <summary>
        /// Duration multiplier for effects/teaching (0.5 to 2.0).
        /// </summary>
        public float DurationMultiplier;

        /// <summary>
        /// Currently active profession archetype.
        /// </summary>
        public FocusArchetype ActiveArchetype;

        /// <summary>
        /// Tick when modifiers were last updated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Creates neutral (1.0) modifiers.
        /// </summary>
        public static ProfessionFocusModifiers Default => new ProfessionFocusModifiers
        {
            QualityMultiplier = 1f,
            SpeedMultiplier = 1f,
            WasteMultiplier = 1f,
            TargetCountMultiplier = 1f,
            PerTargetEffectiveness = 1f,
            BonusChance = 0f,
            XPMultiplier = 1f,
            DurationMultiplier = 1f,
            ActiveArchetype = FocusArchetype.None,
            LastUpdateTick = 0
        };

        /// <summary>
        /// Resets all modifiers to neutral values.
        /// </summary>
        public void Reset()
        {
            QualityMultiplier = 1f;
            SpeedMultiplier = 1f;
            WasteMultiplier = 1f;
            TargetCountMultiplier = 1f;
            PerTargetEffectiveness = 1f;
            BonusChance = 0f;
            XPMultiplier = 1f;
            DurationMultiplier = 1f;
            ActiveArchetype = FocusArchetype.None;
        }
    }

    /// <summary>
    /// Tradeoff configuration for quality vs quantity decisions.
    /// Used by focus abilities like MassProduction, MasterworkFocus, etc.
    /// </summary>
    public struct FocusTradeoff : IComponentData
    {
        /// <summary>
        /// Quality penalty when prioritizing speed (0 to 0.5).
        /// </summary>
        public float SpeedQualityPenalty;

        /// <summary>
        /// Speed penalty when prioritizing quality (0 to 0.5).
        /// </summary>
        public float QualitySpeedPenalty;

        /// <summary>
        /// Waste increase when prioritizing speed (0 to 0.5).
        /// </summary>
        public float SpeedWasteIncrease;

        /// <summary>
        /// Creates default tradeoff values matching ability definitions.
        /// </summary>
        public static FocusTradeoff Default => new FocusTradeoff
        {
            SpeedQualityPenalty = 0.3f,     // MassProduction: +100% speed, -30% quality
            QualitySpeedPenalty = 0.5f,     // MasterworkFocus: +50% quality, -50% speed
            SpeedWasteIncrease = 0.25f      // RapidRefine: +50% speed, +25% waste
        };
    }

    /// <summary>
    /// Result of a focused profession action.
    /// Created after crafting/gathering/healing/teaching/refining completes.
    /// </summary>
    public struct FocusedActionResult : IComponentData
    {
        /// <summary>
        /// Base output quantity before modifiers.
        /// </summary>
        public int BaseQuantity;

        /// <summary>
        /// Final output quantity after modifiers.
        /// </summary>
        public int FinalQuantity;

        /// <summary>
        /// Base quality before modifiers (0-100).
        /// </summary>
        public byte BaseQuality;

        /// <summary>
        /// Final quality after modifiers (0-100).
        /// </summary>
        public byte FinalQuality;

        /// <summary>
        /// Waste generated (materials lost).
        /// </summary>
        public int WasteAmount;

        /// <summary>
        /// Bonus items created (from Inspiration, etc.).
        /// </summary>
        public int BonusItems;

        /// <summary>
        /// XP earned from this action.
        /// </summary>
        public uint XPEarned;

        /// <summary>
        /// Focus spent on this action.
        /// </summary>
        public float FocusSpent;

        /// <summary>
        /// Time taken to complete (in seconds).
        /// </summary>
        public float TimeTaken;

        /// <summary>
        /// Type of focus ability used (if any).
        /// </summary>
        public FocusAbilityType AbilityUsed;
    }

    /// <summary>
    /// Configuration for profession focus system behavior.
    /// </summary>
    public struct ProfessionFocusConfig : IComponentData
    {
        /// <summary>
        /// Base XP per action without focus.
        /// </summary>
        public uint BaseXPPerAction;

        /// <summary>
        /// Maximum quality achievable (100 = perfect).
        /// </summary>
        public byte MaxQuality;

        /// <summary>
        /// Minimum quality floor (even with penalties).
        /// </summary>
        public byte MinQuality;

        /// <summary>
        /// Maximum targets for multi-target abilities.
        /// </summary>
        public byte MaxTargets;

        /// <summary>
        /// Focus cost reduction per 10 skill levels (0-0.5).
        /// </summary>
        public float SkillCostReduction;

        /// <summary>
        /// Creates default configuration.
        /// </summary>
        public static ProfessionFocusConfig Default => new ProfessionFocusConfig
        {
            BaseXPPerAction = 10,
            MaxQuality = 100,
            MinQuality = 10,
            MaxTargets = 5,
            SkillCostReduction = 0.05f // 5% reduction per 10 skill levels
        };
    }

    /// <summary>
    /// Static helpers for profession focus calculations.
    /// </summary>
    public static class ProfessionFocusHelpers
    {
        /// <summary>
        /// Calculates final quality after applying modifiers.
        /// </summary>
        public static byte CalculateFinalQuality(byte baseQuality, float qualityMultiplier, byte minQuality, byte maxQuality)
        {
            float result = baseQuality * qualityMultiplier;
            return (byte)math.clamp(result, minQuality, maxQuality);
        }

        /// <summary>
        /// Calculates final speed after applying modifiers.
        /// </summary>
        public static float CalculateFinalSpeed(float baseSpeed, float speedMultiplier)
        {
            return baseSpeed * math.max(0.1f, speedMultiplier);
        }

        /// <summary>
        /// Calculates waste amount after applying modifiers.
        /// </summary>
        public static int CalculateWaste(int baseMaterials, float wasteMultiplier, float baseWasteRate)
        {
            float waste = baseMaterials * baseWasteRate * wasteMultiplier;
            return (int)math.max(0, waste);
        }

        /// <summary>
        /// Rolls for bonus chance.
        /// </summary>
        public static bool RollBonusChance(float bonusChance, uint seed)
        {
            // Simple deterministic hash for reproducibility
            uint hash = seed * 1103515245 + 12345;
            float roll = (hash % 10000) / 10000f;
            return roll < bonusChance;
        }

        /// <summary>
        /// Gets the appropriate skill level for a focus ability.
        /// </summary>
        public static byte GetSkillLevelForAbility(FocusAbilityType ability, in ProfessionSkills skills)
        {
            var archetype = FocusAbilityDefinitions.GetArchetype(ability);
            return skills.GetSkillForArchetype(archetype);
        }

        /// <summary>
        /// Calculates focus cost after skill reduction.
        /// Higher skill = lower focus cost.
        /// </summary>
        public static float CalculateFocusCost(FocusAbilityType ability, byte skillLevel, float skillCostReduction)
        {
            float baseCost = FocusAbilityDefinitions.GetBaseCost(ability);
            float reduction = (skillLevel / 10) * skillCostReduction;
            return baseCost * math.max(0.5f, 1f - reduction);
        }

        /// <summary>
        /// Gets tradeoff values for a specific focus ability.
        /// Returns (speedMultiplier, qualityMultiplier, wasteMultiplier).
        /// </summary>
        public static (float speed, float quality, float waste) GetAbilityTradeoffs(FocusAbilityType ability)
        {
            return ability switch
            {
                // Crafting tradeoffs
                FocusAbilityType.MassProduction => (2f, 0.7f, 1f),      // +100% speed, -30% quality
                FocusAbilityType.MasterworkFocus => (0.5f, 1.5f, 1f),   // -50% speed, +50% quality
                FocusAbilityType.EfficientCrafting => (1f, 0.9f, 0.6f), // -10% quality, -40% waste

                // Gathering tradeoffs
                FocusAbilityType.SpeedGather => (1.5f, 1f, 1.2f),       // +50% speed, +20% waste
                FocusAbilityType.EfficientGather => (0.8f, 1f, 0.7f),   // -20% speed, -30% waste

                // Refining tradeoffs
                FocusAbilityType.RapidRefine => (1.5f, 1f, 1.25f),      // +50% speed, +25% waste
                FocusAbilityType.PureExtraction => (0.7f, 1f, 0.5f),    // -30% speed, -50% waste

                // Teaching tradeoffs
                FocusAbilityType.IntensiveLessons => (2f, 1f, 1f),      // 2x speed (0.5x duration)
                FocusAbilityType.DeepTeaching => (0.5f, 1.5f, 1f),      // 0.5x speed (2x duration), +50% retention

                // Default - no tradeoff
                _ => (1f, 1f, 1f)
            };
        }
    }
}

