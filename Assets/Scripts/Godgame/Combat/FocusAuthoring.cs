using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Combat
{
    /// <summary>
    /// Authoring component for adding focus system to entities.
    /// Configures initial focus stats based on combat stats or explicit values.
    /// </summary>
    public class FocusAuthoring : MonoBehaviour
    {
        [Header("Combat Stats")]
        [Tooltip("Physical strength (0-100). Unlocks Physique focus abilities.")]
        [Range(0, 100)]
        public int physique = 50;

        [Tooltip("Dexterity/Agility (0-100). Unlocks Finesse focus abilities.")]
        [Range(0, 100)]
        public int finesse = 50;

        [Tooltip("Mental acuity (0-100). Unlocks Arcane focus abilities.")]
        [Range(0, 100)]
        public int intelligence = 50;

        [Tooltip("Mental fortitude (0-100). Increases max focus capacity.")]
        [Range(0, 100)]
        public int will = 50;

        [Tooltip("Insight/perception (0-100). Increases focus regen rate.")]
        [Range(0, 100)]
        public int wisdom = 50;

        [Header("Focus Overrides")]
        [Tooltip("Override max focus (0 = calculate from Will)")]
        public float maxFocusOverride = 0f;

        [Tooltip("Override base regen rate (0 = calculate from Wisdom)")]
        public float regenRateOverride = 0f;

        [Tooltip("Starting focus percentage (0-1)")]
        [Range(0f, 1f)]
        public float startingFocusPercent = 1f;

        public class Baker : Baker<FocusAuthoring>
        {
            public override void Bake(FocusAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add combat stats
                AddComponent(entity, new CombatStats
                {
                    Physique = (byte)authoring.physique,
                    Finesse = (byte)authoring.finesse,
                    Intelligence = (byte)authoring.intelligence,
                    Will = (byte)authoring.will,
                    Wisdom = (byte)authoring.wisdom
                });

                // Calculate focus from stats or use overrides
                var focus = EntityFocus.FromStats(
                    (byte)authoring.will,
                    (byte)authoring.wisdom,
                    (byte)authoring.finesse,
                    (byte)authoring.physique,
                    (byte)authoring.intelligence
                );

                // Apply overrides
                if (authoring.maxFocusOverride > 0f)
                {
                    focus.MaxFocus = authoring.maxFocusOverride;
                }

                if (authoring.regenRateOverride > 0f)
                {
                    focus.BaseRegenRate = authoring.regenRateOverride;
                    focus.CurrentRegenRate = authoring.regenRateOverride;
                }

                // Apply starting percentage
                focus.CurrentFocus = focus.MaxFocus * authoring.startingFocusPercent;

                AddComponent(entity, focus);

                // Add active abilities buffer
                AddBuffer<ActiveFocusAbility>(entity);
            }
        }
    }

    /// <summary>
    /// Authoring component for global focus configuration.
    /// Place on a singleton entity in the scene.
    /// </summary>
    public class FocusConfigAuthoring : MonoBehaviour
    {
        [Header("Exhaustion Settings")]
        [Tooltip("Focus percentage below which exhaustion starts accumulating (0-1)")]
        [Range(0f, 0.5f)]
        public float exhaustionThreshold = 0.2f;

        [Tooltip("Exhaustion accumulation rate per second when below threshold")]
        public float exhaustionAccumulationRate = 5f;

        [Tooltip("Exhaustion recovery rate per second when focus is healthy")]
        public float exhaustionRecoveryRate = 2f;

        [Tooltip("Exhaustion level (0-100) that triggers breakdown risk")]
        [Range(50, 99)]
        public int breakdownRiskThreshold = 80;

        [Header("Regeneration Multipliers")]
        [Tooltip("Regen multiplier when entity is idle/resting")]
        public float idleRegenMultiplier = 2f;

        [Tooltip("Regen multiplier when entity is in combat")]
        public float combatRegenMultiplier = 0.5f;

        [Header("Coma Settings")]
        [Tooltip("Minimum ticks in coma before recovery check")]
        public uint minComaDuration = 300;

        public class Baker : Baker<FocusConfigAuthoring>
        {
            public override void Bake(FocusConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new FocusConfig
                {
                    ExhaustionThreshold = authoring.exhaustionThreshold,
                    ExhaustionAccumulationRate = authoring.exhaustionAccumulationRate,
                    ExhaustionRecoveryRate = authoring.exhaustionRecoveryRate,
                    BreakdownRiskThreshold = (byte)authoring.breakdownRiskThreshold,
                    IdleRegenMultiplier = authoring.idleRegenMultiplier,
                    CombatRegenMultiplier = authoring.combatRegenMultiplier,
                    MinComaDuration = authoring.minComaDuration
                });
            }
        }
    }

    /// <summary>
    /// Authoring component for adding a specific combat archetype preset.
    /// Provides quick setup for common entity types.
    /// </summary>
    public class CombatArchetypeAuthoring : MonoBehaviour
    {
        public enum ArchetypePreset
        {
            Warrior,        // High Physique, medium Will
            Rogue,          // High Finesse, medium Wisdom
            Mage,           // High Intelligence, high Wisdom
            Ranger,         // Balanced Finesse/Physique
            Tank,           // Very high Will, high Physique
            Berserker,      // Extreme Physique, low Wisdom (risky focus)
            Spellblade,     // Balanced Intelligence/Finesse
            Cleric,         // High Wisdom, medium Intelligence
            Custom          // Use custom values
        }

        [Tooltip("Archetype preset to apply")]
        public ArchetypePreset preset = ArchetypePreset.Warrior;

        [Header("Custom Stats (only used if preset is Custom)")]
        [Range(0, 100)] public int customPhysique = 50;
        [Range(0, 100)] public int customFinesse = 50;
        [Range(0, 100)] public int customIntelligence = 50;
        [Range(0, 100)] public int customWill = 50;
        [Range(0, 100)] public int customWisdom = 50;

        public class Baker : Baker<CombatArchetypeAuthoring>
        {
            public override void Bake(CombatArchetypeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Get stats from preset or custom values
                var stats = GetStatsForPreset(authoring.preset, authoring);

                AddComponent(entity, stats);

                // Create focus from stats
                var focus = EntityFocus.FromStats(
                    stats.Will, stats.Wisdom, stats.Finesse, stats.Physique, stats.Intelligence
                );
                AddComponent(entity, focus);

                // Add active abilities buffer
                AddBuffer<ActiveFocusAbility>(entity);
            }

            private CombatStats GetStatsForPreset(ArchetypePreset preset, CombatArchetypeAuthoring authoring)
            {
                return preset switch
                {
                    ArchetypePreset.Warrior => new CombatStats
                    {
                        Physique = 75,
                        Finesse = 50,
                        Intelligence = 30,
                        Will = 60,
                        Wisdom = 40
                    },
                    ArchetypePreset.Rogue => new CombatStats
                    {
                        Physique = 40,
                        Finesse = 80,
                        Intelligence = 45,
                        Will = 45,
                        Wisdom = 55
                    },
                    ArchetypePreset.Mage => new CombatStats
                    {
                        Physique = 25,
                        Finesse = 35,
                        Intelligence = 85,
                        Will = 50,
                        Wisdom = 70
                    },
                    ArchetypePreset.Ranger => new CombatStats
                    {
                        Physique = 55,
                        Finesse = 70,
                        Intelligence = 40,
                        Will = 50,
                        Wisdom = 55
                    },
                    ArchetypePreset.Tank => new CombatStats
                    {
                        Physique = 70,
                        Finesse = 30,
                        Intelligence = 30,
                        Will = 85,
                        Wisdom = 45
                    },
                    ArchetypePreset.Berserker => new CombatStats
                    {
                        Physique = 90,
                        Finesse = 45,
                        Intelligence = 20,
                        Will = 40,
                        Wisdom = 25
                    },
                    ArchetypePreset.Spellblade => new CombatStats
                    {
                        Physique = 45,
                        Finesse = 65,
                        Intelligence = 65,
                        Will = 50,
                        Wisdom = 50
                    },
                    ArchetypePreset.Cleric => new CombatStats
                    {
                        Physique = 40,
                        Finesse = 35,
                        Intelligence = 60,
                        Will = 65,
                        Wisdom = 80
                    },
                    ArchetypePreset.Custom => new CombatStats
                    {
                        Physique = (byte)authoring.customPhysique,
                        Finesse = (byte)authoring.customFinesse,
                        Intelligence = (byte)authoring.customIntelligence,
                        Will = (byte)authoring.customWill,
                        Wisdom = (byte)authoring.customWisdom
                    },
                    _ => CombatStats.Default
                };
            }
        }
    }
}

