using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Static definitions and lookup for focus abilities.
    /// Provides cost, effect, and unlock requirements for each ability type.
    /// Covers combat (Finesse/Physique/Arcane) and profession (Crafting/Gathering/Healing/Teaching/Refining) abilities.
    /// </summary>
    public static class FocusAbilityDefinitions
    {
        /// <summary>
        /// Gets the base focus cost for an ability.
        /// </summary>
        public static float GetBaseCost(FocusAbilityType ability)
        {
            return ability switch
            {
                // === COMBAT: Finesse abilities ===
                FocusAbilityType.Parry => 5f,
                FocusAbilityType.DualWieldStrike => 10f,
                FocusAbilityType.CriticalFocus => 8f,
                FocusAbilityType.DodgeBoost => 6f,
                FocusAbilityType.MultiStrike => 15f,
                FocusAbilityType.RapidFire => 7f,
                FocusAbilityType.MultiShot => 12f,
                FocusAbilityType.CriticalHeadshot => 20f,

                // === COMBAT: Physique abilities ===
                FocusAbilityType.IgnorePain => 8f,
                FocusAbilityType.SweepAttack => 12f,
                FocusAbilityType.AttackSpeedBoost => 7f,
                FocusAbilityType.Cleave => 10f,
                FocusAbilityType.BullRush => 15f,
                FocusAbilityType.GroundSlam => 18f,
                FocusAbilityType.Fortify => 10f,

                // === COMBAT: Arcane abilities ===
                FocusAbilityType.SummonBoost => 15f,
                FocusAbilityType.ManaRegen => 5f,
                FocusAbilityType.CooldownReduction => 10f,
                FocusAbilityType.Multicast => 20f,
                FocusAbilityType.BuffExtend => 8f,
                FocusAbilityType.EmpowerCast => 12f,
                FocusAbilityType.ManaChannel => 3f,
                FocusAbilityType.QuickCast => 25f,

                // === CRAFTING abilities ===
                FocusAbilityType.MassProduction => 8f,
                FocusAbilityType.MasterworkFocus => 12f,
                FocusAbilityType.BatchCrafting => 15f,
                FocusAbilityType.PrecisionWork => 6f,
                FocusAbilityType.Reinforce => 8f,
                FocusAbilityType.EfficientCrafting => 5f,
                FocusAbilityType.Inspiration => 10f,
                FocusAbilityType.StudiedCrafting => 4f,

                // === GATHERING abilities ===
                FocusAbilityType.SpeedGather => 5f,
                FocusAbilityType.EfficientGather => 7f,
                FocusAbilityType.GatherOverdrive => 20f,
                FocusAbilityType.ResourceSense => 4f,
                FocusAbilityType.DeepExtraction => 10f,
                FocusAbilityType.LuckyFind => 6f,
                FocusAbilityType.SustainableHarvest => 12f,
                FocusAbilityType.Multitasking => 8f,

                // === HEALING abilities ===
                FocusAbilityType.MassHeal => 10f,
                FocusAbilityType.LifeClutch => 15f,
                FocusAbilityType.PurifyingFocus => 8f,
                FocusAbilityType.RegenerationAura => 6f,
                FocusAbilityType.IntensiveCare => 12f,
                FocusAbilityType.MiracleHealing => 25f,
                FocusAbilityType.LifeTransfer => 10f,
                FocusAbilityType.Triage => 8f,

                // === TEACHING abilities ===
                FocusAbilityType.IntensiveLessons => 10f,
                FocusAbilityType.DeepTeaching => 8f,
                FocusAbilityType.GroupInstruction => 12f,
                FocusAbilityType.InspiringPresence => 5f,
                FocusAbilityType.HandsOnTraining => 7f,
                FocusAbilityType.TalentDiscovery => 15f,
                FocusAbilityType.MindLink => 20f,
                FocusAbilityType.Eureka => 18f,

                // === REFINING abilities ===
                FocusAbilityType.RapidRefine => 6f,
                FocusAbilityType.PureExtraction => 10f,
                FocusAbilityType.BatchRefine => 15f,
                FocusAbilityType.QualityControl => 4f,
                FocusAbilityType.Reclamation => 8f,
                FocusAbilityType.Transmutation => 20f,
                FocusAbilityType.Synthesis => 25f,
                FocusAbilityType.GentleProcessing => 7f,

                _ => 10f
            };
        }

        /// <summary>
        /// Gets the cost type for an ability.
        /// </summary>
        public static FocusCostType GetCostType(FocusAbilityType ability)
        {
            return ability switch
            {
                // === Burst cost (one-time) ===
                // Combat
                FocusAbilityType.DualWieldStrike => FocusCostType.Burst,
                FocusAbilityType.MultiStrike => FocusCostType.Burst,
                FocusAbilityType.CriticalHeadshot => FocusCostType.Burst,
                FocusAbilityType.SweepAttack => FocusCostType.Burst,
                FocusAbilityType.Cleave => FocusCostType.Burst,
                FocusAbilityType.BullRush => FocusCostType.Burst,
                FocusAbilityType.GroundSlam => FocusCostType.Burst,
                FocusAbilityType.Multicast => FocusCostType.Burst,
                FocusAbilityType.QuickCast => FocusCostType.Burst,
                // Crafting
                FocusAbilityType.BatchCrafting => FocusCostType.Burst,
                FocusAbilityType.Inspiration => FocusCostType.Burst,
                // Gathering
                FocusAbilityType.GatherOverdrive => FocusCostType.Burst,
                FocusAbilityType.LuckyFind => FocusCostType.Burst,
                // Healing
                FocusAbilityType.MiracleHealing => FocusCostType.Burst,
                FocusAbilityType.LifeTransfer => FocusCostType.Burst,
                // Teaching
                FocusAbilityType.TalentDiscovery => FocusCostType.Burst,
                FocusAbilityType.Eureka => FocusCostType.Burst,
                // Refining
                FocusAbilityType.BatchRefine => FocusCostType.Burst,
                FocusAbilityType.Transmutation => FocusCostType.Burst,
                FocusAbilityType.Synthesis => FocusCostType.Burst,

                // === Per-use cost ===
                FocusAbilityType.RapidFire => FocusCostType.PerUse,
                FocusAbilityType.MultiShot => FocusCostType.PerUse,

                // === Per-second drain (toggles/sustained) - default ===
                _ => FocusCostType.PerSecond
            };
        }

        /// <summary>
        /// Gets the effect magnitude for an ability.
        /// Meaning varies by ability type - see comments for interpretation.
        /// </summary>
        public static float GetEffectMagnitude(FocusAbilityType ability)
        {
            return ability switch
            {
                // === COMBAT: Finesse - percentages/multipliers ===
                FocusAbilityType.Parry => 1f,               // 100% block chance during window
                FocusAbilityType.DualWieldStrike => 2f,     // 2x attack speed
                FocusAbilityType.CriticalFocus => 0.5f,     // +50% crit chance
                FocusAbilityType.DodgeBoost => 0.4f,        // +40% dodge
                FocusAbilityType.MultiStrike => 3f,         // Hit 3 targets
                FocusAbilityType.RapidFire => 1.5f,         // 1.5x fire rate
                FocusAbilityType.MultiShot => 3f,           // 3 arrows
                FocusAbilityType.CriticalHeadshot => 3f,    // 3x damage crit

                // === COMBAT: Physique ===
                FocusAbilityType.IgnorePain => 0.25f,       // 25% damage reduction
                FocusAbilityType.SweepAttack => 180f,       // 180 degree arc
                FocusAbilityType.AttackSpeedBoost => 0.3f,  // +30% attack speed
                FocusAbilityType.Cleave => 0.5f,            // 50% armor ignore
                FocusAbilityType.BullRush => 5f,            // 5 meter charge
                FocusAbilityType.GroundSlam => 3f,          // 3 meter radius
                FocusAbilityType.Fortify => 1f,             // 100% damage immunity

                // === COMBAT: Arcane ===
                FocusAbilityType.SummonBoost => 2f,         // +2 summon cap
                FocusAbilityType.ManaRegen => 1f,           // +100% mana regen
                FocusAbilityType.CooldownReduction => 0.5f, // 50% cooldown reduction
                FocusAbilityType.Multicast => 2f,           // Cast 2x
                FocusAbilityType.BuffExtend => 0.5f,        // +50% duration
                FocusAbilityType.EmpowerCast => 0.4f,       // +40% power
                FocusAbilityType.ManaChannel => 5f,         // 5x mana regen
                FocusAbilityType.QuickCast => 0f,           // Instant cast

                // === CRAFTING ===
                FocusAbilityType.MassProduction => 1f,      // +100% speed, -30% quality (encoded in tradeoff)
                FocusAbilityType.MasterworkFocus => 0.5f,   // +50% quality, -50% speed
                FocusAbilityType.BatchCrafting => 3f,       // Craft 3 items
                FocusAbilityType.PrecisionWork => 0.25f,    // +25% quality
                FocusAbilityType.Reinforce => 0.75f,        // +75% durability
                FocusAbilityType.EfficientCrafting => 0.4f, // -40% material cost
                FocusAbilityType.Inspiration => 0.2f,       // 20% chance bonus item
                FocusAbilityType.StudiedCrafting => 0.5f,   // +50% XP gain

                // === GATHERING ===
                FocusAbilityType.SpeedGather => 0.5f,       // +50% rate, +20% waste
                FocusAbilityType.EfficientGather => 0.3f,   // -30% waste, -20% speed
                FocusAbilityType.GatherOverdrive => 3f,     // 3x gather rate
                FocusAbilityType.ResourceSense => 10f,      // 10 meter reveal radius
                FocusAbilityType.DeepExtraction => 1f,      // +100% yield from rich nodes
                FocusAbilityType.LuckyFind => 0.15f,        // 15% rare find chance
                FocusAbilityType.SustainableHarvest => 0.5f, // 50% chance no depletion
                FocusAbilityType.Multitasking => 2f,        // Work 2 nodes

                // === HEALING ===
                FocusAbilityType.MassHeal => 0.5f,          // 50% effectiveness per target
                FocusAbilityType.LifeClutch => 1f,          // 100% death prevention
                FocusAbilityType.PurifyingFocus => 3f,      // Remove up to 3 ailments
                FocusAbilityType.RegenerationAura => 5f,    // 5 HP/sec to nearby
                FocusAbilityType.IntensiveCare => 1f,       // +100% healing single target
                FocusAbilityType.MiracleHealing => 1f,      // Heal permanent wounds
                FocusAbilityType.LifeTransfer => 2f,        // 2:1 transfer ratio
                FocusAbilityType.Triage => 30f,             // 30 second no-death window

                // === TEACHING ===
                FocusAbilityType.IntensiveLessons => 1f,    // +100% XP rate, 0.5x duration
                FocusAbilityType.DeepTeaching => 0.5f,      // +50% retention, 2x duration
                FocusAbilityType.GroupInstruction => 0.6f,  // 60% effectiveness per student
                FocusAbilityType.InspiringPresence => 0.2f, // +20% learning speed area
                FocusAbilityType.HandsOnTraining => 0.3f,   // +30% practical XP bonus
                FocusAbilityType.TalentDiscovery => 1f,     // Unlock hidden talent
                FocusAbilityType.MindLink => 5f,            // 5x knowledge transfer
                FocusAbilityType.Eureka => 2f,              // 2x research speed

                // === REFINING ===
                FocusAbilityType.RapidRefine => 0.5f,       // +50% speed, +25% waste
                FocusAbilityType.PureExtraction => 0.5f,    // -50% waste, -30% speed
                FocusAbilityType.BatchRefine => 3f,         // Process 3x materials
                FocusAbilityType.QualityControl => 0.15f,   // +15% purity
                FocusAbilityType.Reclamation => 0.5f,       // 50% waste recovery
                FocusAbilityType.Transmutation => 0.1f,     // 10% rare byproduct chance
                FocusAbilityType.Synthesis => 1f,           // Combine incompatible
                FocusAbilityType.GentleProcessing => 1f,    // Preserve special properties

                _ => 1f
            };
        }

        /// <summary>
        /// Gets whether ability is a toggle (sustained) or instant.
        /// </summary>
        public static bool IsToggleAbility(FocusAbilityType ability)
        {
            return GetCostType(ability) == FocusCostType.PerSecond;
        }

        /// <summary>
        /// Gets the default duration for burst/instant abilities.
        /// 0 = instant effect, >0 = effect lasts this many seconds.
        /// </summary>
        public static float GetDefaultDuration(FocusAbilityType ability)
        {
            return ability switch
            {
                // Combat burst durations
                FocusAbilityType.DualWieldStrike => 3f,
                FocusAbilityType.MultiStrike => 0f,
                FocusAbilityType.CriticalHeadshot => 0f,
                FocusAbilityType.SweepAttack => 0f,
                FocusAbilityType.Cleave => 0f,
                FocusAbilityType.BullRush => 1f,
                FocusAbilityType.GroundSlam => 0f,
                FocusAbilityType.Multicast => 0f,
                FocusAbilityType.QuickCast => 5f,

                // Crafting burst durations
                FocusAbilityType.BatchCrafting => 0f,       // Instant batch
                FocusAbilityType.Inspiration => 60f,        // 60 second window

                // Gathering burst durations
                FocusAbilityType.GatherOverdrive => 10f,    // 10 second boost
                FocusAbilityType.LuckyFind => 0f,           // Instant check

                // Healing burst durations
                FocusAbilityType.MiracleHealing => 0f,      // Instant heal
                FocusAbilityType.LifeTransfer => 0f,        // Instant transfer

                // Teaching burst durations
                FocusAbilityType.TalentDiscovery => 0f,     // Instant discovery
                FocusAbilityType.Eureka => 0f,              // Instant breakthrough

                // Refining burst durations
                FocusAbilityType.BatchRefine => 0f,         // Instant batch
                FocusAbilityType.Transmutation => 0f,       // Instant transmute
                FocusAbilityType.Synthesis => 0f,           // Instant combine

                // Toggle abilities have 0 duration (until deactivated)
                _ => 0f
            };
        }

        /// <summary>
        /// Gets the archetype this ability belongs to.
        /// </summary>
        public static FocusArchetype GetArchetype(FocusAbilityType ability)
        {
            // Use byte value ranges for efficient categorization
            byte val = (byte)ability;

            // Combat: Finesse (10-29)
            if (val >= 10 && val <= 29) return FocusArchetype.Finesse;

            // Combat: Physique (30-49)
            if (val >= 30 && val <= 49) return FocusArchetype.Physique;

            // Combat: Arcane (50-69)
            if (val >= 50 && val <= 69) return FocusArchetype.Arcane;

            // Crafting (70-89)
            if (val >= 70 && val <= 89) return FocusArchetype.Crafting;

            // Gathering (90-109)
            if (val >= 90 && val <= 109) return FocusArchetype.Gathering;

            // Healing (110-129)
            if (val >= 110 && val <= 129) return FocusArchetype.Healing;

            // Teaching (130-149)
            if (val >= 130 && val <= 149) return FocusArchetype.Teaching;

            // Refining (150-169)
            if (val >= 150 && val <= 169) return FocusArchetype.Refining;

            return FocusArchetype.None;
        }

        /// <summary>
        /// Gets the minimum skill/stat requirement to unlock an ability.
        /// Returns (archetype, minValue). For combat = stat, for profession = skill level.
        /// </summary>
        public static (FocusArchetype archetype, byte minValue) GetUnlockRequirement(FocusAbilityType ability)
        {
            var archetype = GetArchetype(ability);

            // === COMBAT Tier 1 (stat 50+) ===
            if (ability == FocusAbilityType.Parry ||
                ability == FocusAbilityType.DodgeBoost ||
                ability == FocusAbilityType.IgnorePain ||
                ability == FocusAbilityType.AttackSpeedBoost ||
                ability == FocusAbilityType.ManaRegen ||
                ability == FocusAbilityType.BuffExtend ||
                ability == FocusAbilityType.ManaChannel)
                return (archetype, 50);

            // === COMBAT Tier 2 (stat 70+) ===
            if (ability == FocusAbilityType.DualWieldStrike ||
                ability == FocusAbilityType.CriticalFocus ||
                ability == FocusAbilityType.MultiStrike ||
                ability == FocusAbilityType.RapidFire ||
                ability == FocusAbilityType.SweepAttack ||
                ability == FocusAbilityType.Cleave ||
                ability == FocusAbilityType.BullRush ||
                ability == FocusAbilityType.SummonBoost ||
                ability == FocusAbilityType.CooldownReduction ||
                ability == FocusAbilityType.Multicast ||
                ability == FocusAbilityType.EmpowerCast)
                return (archetype, 70);

            // === COMBAT Tier 3 (stat 85+) ===
            if (ability == FocusAbilityType.MultiShot ||
                ability == FocusAbilityType.CriticalHeadshot ||
                ability == FocusAbilityType.GroundSlam ||
                ability == FocusAbilityType.Fortify ||
                ability == FocusAbilityType.QuickCast)
                return (archetype, 85);

            // === CRAFTING Tier 1 (skill 25+) ===
            if (ability == FocusAbilityType.MassProduction ||
                ability == FocusAbilityType.EfficientCrafting ||
                ability == FocusAbilityType.StudiedCrafting)
                return (archetype, 25);

            // === CRAFTING Tier 2 (skill 50+) ===
            if (ability == FocusAbilityType.PrecisionWork ||
                ability == FocusAbilityType.Reinforce ||
                ability == FocusAbilityType.BatchCrafting)
                return (archetype, 50);

            // === CRAFTING Tier 3 (skill 75+) ===
            if (ability == FocusAbilityType.MasterworkFocus ||
                ability == FocusAbilityType.Inspiration)
                return (archetype, 75);

            // === GATHERING Tier 1 (skill 25+) ===
            if (ability == FocusAbilityType.SpeedGather ||
                ability == FocusAbilityType.EfficientGather ||
                ability == FocusAbilityType.ResourceSense)
                return (archetype, 25);

            // === GATHERING Tier 2 (skill 50+) ===
            if (ability == FocusAbilityType.GatherOverdrive ||
                ability == FocusAbilityType.DeepExtraction ||
                ability == FocusAbilityType.Multitasking)
                return (archetype, 50);

            // === GATHERING Tier 3 (skill 75+) ===
            if (ability == FocusAbilityType.LuckyFind ||
                ability == FocusAbilityType.SustainableHarvest)
                return (archetype, 75);

            // === HEALING Tier 1 (skill 25+) ===
            if (ability == FocusAbilityType.RegenerationAura ||
                ability == FocusAbilityType.PurifyingFocus ||
                ability == FocusAbilityType.Triage)
                return (archetype, 25);

            // === HEALING Tier 2 (skill 50+) ===
            if (ability == FocusAbilityType.MassHeal ||
                ability == FocusAbilityType.IntensiveCare ||
                ability == FocusAbilityType.LifeTransfer)
                return (archetype, 50);

            // === HEALING Tier 3 (skill 75+) ===
            if (ability == FocusAbilityType.LifeClutch ||
                ability == FocusAbilityType.MiracleHealing)
                return (archetype, 75);

            // === TEACHING Tier 1 (skill 25+) ===
            if (ability == FocusAbilityType.InspiringPresence ||
                ability == FocusAbilityType.HandsOnTraining)
                return (archetype, 25);

            // === TEACHING Tier 2 (skill 50+) ===
            if (ability == FocusAbilityType.IntensiveLessons ||
                ability == FocusAbilityType.DeepTeaching ||
                ability == FocusAbilityType.GroupInstruction)
                return (archetype, 50);

            // === TEACHING Tier 3 (skill 75+) ===
            if (ability == FocusAbilityType.TalentDiscovery ||
                ability == FocusAbilityType.MindLink ||
                ability == FocusAbilityType.Eureka)
                return (archetype, 75);

            // === REFINING Tier 1 (skill 25+) ===
            if (ability == FocusAbilityType.RapidRefine ||
                ability == FocusAbilityType.PureExtraction ||
                ability == FocusAbilityType.QualityControl)
                return (archetype, 25);

            // === REFINING Tier 2 (skill 50+) ===
            if (ability == FocusAbilityType.BatchRefine ||
                ability == FocusAbilityType.Reclamation ||
                ability == FocusAbilityType.GentleProcessing)
                return (archetype, 50);

            // === REFINING Tier 3 (skill 75+) ===
            if (ability == FocusAbilityType.Transmutation ||
                ability == FocusAbilityType.Synthesis)
                return (archetype, 75);

            // Default to tier 1
            return (archetype, 25);
        }

        /// <summary>
        /// Checks if an entity can use an ability based on combat stats.
        /// For profession abilities, use CanUseProfessionAbility instead.
        /// </summary>
        public static bool CanUseAbility(FocusAbilityType ability, in CombatStats stats)
        {
            var (archetype, minValue) = GetUnlockRequirement(ability);

            return archetype switch
            {
                FocusArchetype.Finesse => stats.Finesse >= minValue,
                FocusArchetype.Physique => stats.Physique >= minValue,
                FocusArchetype.Arcane => stats.Intelligence >= minValue,
                // Profession abilities always return false here - use CanUseProfessionAbility
                _ => false
            };
        }

        /// <summary>
        /// Checks if an entity can use a profession ability based on their skill level.
        /// </summary>
        public static bool CanUseProfessionAbility(FocusAbilityType ability, byte skillLevel)
        {
            var (archetype, minValue) = GetUnlockRequirement(ability);

            // Only profession archetypes
            if (archetype < FocusArchetype.Crafting)
                return false;

            return skillLevel >= minValue;
        }

        /// <summary>
        /// Gets the effectiveness multiplier based on stat/skill level above requirement.
        /// Higher stats/skills = more effective abilities.
        /// </summary>
        public static float GetEffectivenessMultiplier(FocusAbilityType ability, in CombatStats stats)
        {
            var (archetype, minValue) = GetUnlockRequirement(ability);

            byte statValue = archetype switch
            {
                FocusArchetype.Finesse => stats.Finesse,
                FocusArchetype.Physique => stats.Physique,
                FocusArchetype.Arcane => stats.Intelligence,
                _ => (byte)50 // Profession abilities need skill-based lookup
            };

            if (statValue < minValue) return 0f;

            // 1.0 at min requirement, up to 1.5 at 100 stat
            float excess = statValue - minValue;
            float maxExcess = 100 - minValue;
            return 1f + (excess / maxExcess) * 0.5f;
        }

        /// <summary>
        /// Gets the effectiveness multiplier for profession abilities based on skill level.
        /// </summary>
        public static float GetProfessionEffectivenessMultiplier(FocusAbilityType ability, byte skillLevel)
        {
            var (_, minValue) = GetUnlockRequirement(ability);

            if (skillLevel < minValue) return 0f;

            // 1.0 at min requirement, up to 1.5 at 100 skill
            float excess = skillLevel - minValue;
            float maxExcess = 100 - minValue;
            return 1f + (excess / maxExcess) * 0.5f;
        }

        /// <summary>
        /// Returns true if the ability is a profession (non-combat) ability.
        /// </summary>
        public static bool IsProfessionAbility(FocusAbilityType ability)
        {
            var archetype = GetArchetype(ability);
            return archetype >= FocusArchetype.Crafting;
        }

        /// <summary>
        /// Returns true if the ability is a combat ability.
        /// </summary>
        public static bool IsCombatAbility(FocusAbilityType ability)
        {
            var archetype = GetArchetype(ability);
            return archetype >= FocusArchetype.Finesse && archetype <= FocusArchetype.Arcane;
        }

        /// <summary>
        /// Gets a description of the ability's effect.
        /// </summary>
        public static string GetDescription(FocusAbilityType ability)
        {
            return ability switch
            {
                // === COMBAT: Finesse ===
                FocusAbilityType.Parry => "Block incoming attacks with a counter-attack window",
                FocusAbilityType.DualWieldStrike => "Attack twice as fast with dual-wield weapons",
                FocusAbilityType.CriticalFocus => "+50% crit chance against single target",
                FocusAbilityType.DodgeBoost => "+40% dodge rating while active",
                FocusAbilityType.MultiStrike => "Hit multiple adjacent targets in rapid sequence",
                FocusAbilityType.RapidFire => "Fire ranged attacks 50% faster",
                FocusAbilityType.MultiShot => "Fire 3 arrows simultaneously",
                FocusAbilityType.CriticalHeadshot => "Precision shot dealing 3x damage",

                // === COMBAT: Physique ===
                FocusAbilityType.IgnorePain => "Reduce incoming damage by 25%",
                FocusAbilityType.SweepAttack => "Arc attack hitting all enemies in front",
                FocusAbilityType.AttackSpeedBoost => "+30% attack speed while active",
                FocusAbilityType.Cleave => "Heavy hit ignoring 50% armor",
                FocusAbilityType.BullRush => "Charge through enemies, knocking them aside",
                FocusAbilityType.GroundSlam => "Ground slam dealing AOE damage",
                FocusAbilityType.Fortify => "Become invulnerable but cannot attack",

                // === COMBAT: Arcane ===
                FocusAbilityType.SummonBoost => "Increase summon cap by 2",
                FocusAbilityType.ManaRegen => "Double mana regeneration rate",
                FocusAbilityType.CooldownReduction => "Reduce ability cooldowns by 50%",
                FocusAbilityType.Multicast => "Cast the same spell twice",
                FocusAbilityType.BuffExtend => "Extend buff/debuff duration by 50%",
                FocusAbilityType.EmpowerCast => "Increase spell power by 40%",
                FocusAbilityType.ManaChannel => "Channel to rapidly restore mana",
                FocusAbilityType.QuickCast => "Next spell is instant cast",

                // === CRAFTING ===
                FocusAbilityType.MassProduction => "+100% craft speed, -30% quality",
                FocusAbilityType.MasterworkFocus => "+50% quality, -50% speed (create masterworks)",
                FocusAbilityType.BatchCrafting => "Craft 3 items at once",
                FocusAbilityType.PrecisionWork => "+25% quality with no speed penalty",
                FocusAbilityType.Reinforce => "+75% durability on crafted items",
                FocusAbilityType.EfficientCrafting => "-40% material cost, slight quality reduction",
                FocusAbilityType.Inspiration => "20% chance to craft a bonus item",
                FocusAbilityType.StudiedCrafting => "+50% crafting XP gain",

                // === GATHERING ===
                FocusAbilityType.SpeedGather => "+50% gather rate, +20% waste",
                FocusAbilityType.EfficientGather => "-30% waste, -20% gather speed",
                FocusAbilityType.GatherOverdrive => "3x gather rate for 10 seconds",
                FocusAbilityType.ResourceSense => "Reveal hidden and rare resources nearby",
                FocusAbilityType.DeepExtraction => "+100% yield from rich nodes",
                FocusAbilityType.LuckyFind => "15% chance to find rare materials",
                FocusAbilityType.SustainableHarvest => "50% chance to not deplete node",
                FocusAbilityType.Multitasking => "Gather from two nodes simultaneously",

                // === HEALING ===
                FocusAbilityType.MassHeal => "Heal multiple targets at 50% effectiveness each",
                FocusAbilityType.LifeClutch => "Prevent single target from dying",
                FocusAbilityType.PurifyingFocus => "Remove up to 3 ailments and debuffs",
                FocusAbilityType.RegenerationAura => "Passive 5 HP/sec to nearby allies",
                FocusAbilityType.IntensiveCare => "+100% healing to single target",
                FocusAbilityType.MiracleHealing => "Heal permanent wounds and disabilities",
                FocusAbilityType.LifeTransfer => "Transfer own health to target (2:1 ratio)",
                FocusAbilityType.Triage => "Stabilize critically injured for 30 seconds",

                // === TEACHING ===
                FocusAbilityType.IntensiveLessons => "+100% XP transfer, half duration",
                FocusAbilityType.DeepTeaching => "+50% skill retention, 2x duration",
                FocusAbilityType.GroupInstruction => "Teach multiple students at 60% effectiveness",
                FocusAbilityType.InspiringPresence => "+20% learning speed for all nearby",
                FocusAbilityType.HandsOnTraining => "+30% practical experience bonus",
                FocusAbilityType.TalentDiscovery => "Unlock hidden potential in student",
                FocusAbilityType.MindLink => "Share knowledge directly (5x transfer rate)",
                FocusAbilityType.Eureka => "2x research speed breakthrough",

                // === REFINING ===
                FocusAbilityType.RapidRefine => "+50% refine speed, +25% waste",
                FocusAbilityType.PureExtraction => "-50% waste, -30% speed",
                FocusAbilityType.BatchRefine => "Process 3x materials at once",
                FocusAbilityType.QualityControl => "+15% output purity/quality",
                FocusAbilityType.Reclamation => "Recover 50% of waste materials",
                FocusAbilityType.Transmutation => "10% chance for rare byproducts",
                FocusAbilityType.Synthesis => "Combine normally incompatible materials",
                FocusAbilityType.GentleProcessing => "Preserve special material properties",

                _ => "Unknown ability"
            };
        }
    }
}
