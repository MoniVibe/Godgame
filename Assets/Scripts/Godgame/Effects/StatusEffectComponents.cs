using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Effects
{
    /// <summary>
    /// Types of status effects that can be applied to entities.
    /// See Docs/Concepts/Core/Focus_And_Status_Effects_System.md
    /// </summary>
    public enum StatusEffectType : byte
    {
        None = 0,

        // Damage over time
        Bleed = 1,
        Poison = 2,
        Burn = 3,
        Freeze = 4,
        Shock = 5,

        // Healing/Recovery
        Regeneration = 10,
        HealOverTime = 11,
        ManaRegen = 12,

        // Stat modifiers
        SpeedBoost = 20,
        SpeedSlow = 21,
        DamageBoost = 22,
        DamageReduction = 23,
        ArmorBoost = 24,
        ArmorBreak = 25,

        // Crowd control
        Stunned = 30,
        Rooted = 31,
        Silenced = 32,
        Feared = 33,
        Charmed = 34,
        Confused = 35,

        // Buffs
        Blessed = 40,
        Inspired = 41,
        FortifiedHealth = 42,
        Shield = 43,
        Invisible = 44,
        Haste = 45,

        // Debuffs
        Cursed = 50,
        Weakened = 51,
        Vulnerable = 52,
        Marked = 53,
        Exhausted = 54,
        Dazed = 55,

        // Special
        OnFire = 60,
        Wet = 61,
        Chilled = 62,
        Electrified = 63,
        Divine = 64,
        Demonic = 65
    }

    /// <summary>
    /// Categories for status effect handling.
    /// </summary>
    public enum StatusEffectCategory : byte
    {
        None = 0,
        DamageOverTime = 1,
        HealOverTime = 2,
        StatModifier = 3,
        CrowdControl = 4,
        Buff = 5,
        Debuff = 6,
        Elemental = 7,
        Divine = 8
    }

    /// <summary>
    /// How multiple instances of the same effect interact.
    /// </summary>
    public enum StatusEffectStacking : byte
    {
        /// <summary>Refresh duration, no stacking</summary>
        Refresh = 0,
        /// <summary>Stack magnitude up to max stacks</summary>
        Stack = 1,
        /// <summary>Only keep highest magnitude</summary>
        KeepHighest = 2,
        /// <summary>Each instance is independent</summary>
        Independent = 3
    }

    /// <summary>
    /// Active status effect on an entity.
    /// Buffer element for dynamic effect storage.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct StatusEffect : IBufferElementData
    {
        /// <summary>
        /// Type of effect.
        /// </summary>
        public StatusEffectType Type;

        /// <summary>
        /// Effect magnitude (damage per tick, speed multiplier, etc.)
        /// Interpretation depends on effect type.
        /// </summary>
        public float Magnitude;

        /// <summary>
        /// Remaining duration in seconds.
        /// -1 = permanent (until dispelled)
        /// </summary>
        public float Duration;

        /// <summary>
        /// Time since last tick (for DoT/HoT effects).
        /// </summary>
        public float TimeSinceLastTick;

        /// <summary>
        /// Tick interval in seconds (for periodic effects).
        /// </summary>
        public float TickInterval;

        /// <summary>
        /// Entity that applied this effect (for attribution).
        /// </summary>
        public Entity Source;

        /// <summary>
        /// Current stack count (for stackable effects).
        /// </summary>
        public byte StackCount;

        /// <summary>
        /// Maximum stacks allowed.
        /// </summary>
        public byte MaxStacks;

        /// <summary>
        /// Whether this effect has expired.
        /// </summary>
        public bool IsExpired => Duration == 0f;

        /// <summary>
        /// Whether this is a permanent effect.
        /// </summary>
        public bool IsPermanent => Duration < 0f;

        /// <summary>
        /// Create a simple timed effect.
        /// </summary>
        public static StatusEffect Create(StatusEffectType type, float magnitude, float duration, Entity source = default)
        {
            return new StatusEffect
            {
                Type = type,
                Magnitude = magnitude,
                Duration = duration,
                TimeSinceLastTick = 0f,
                TickInterval = 1f,
                Source = source,
                StackCount = 1,
                MaxStacks = 1
            };
        }

        /// <summary>
        /// Create a damage-over-time effect.
        /// </summary>
        public static StatusEffect CreateDoT(StatusEffectType type, float damagePerTick, float tickInterval, float duration, Entity source = default)
        {
            return new StatusEffect
            {
                Type = type,
                Magnitude = damagePerTick,
                Duration = duration,
                TimeSinceLastTick = 0f,
                TickInterval = tickInterval,
                Source = source,
                StackCount = 1,
                MaxStacks = 5
            };
        }

        /// <summary>
        /// Create a stackable buff.
        /// </summary>
        public static StatusEffect CreateStackable(StatusEffectType type, float magnitudePerStack, float duration, byte maxStacks, Entity source = default)
        {
            return new StatusEffect
            {
                Type = type,
                Magnitude = magnitudePerStack,
                Duration = duration,
                TimeSinceLastTick = 0f,
                TickInterval = 0f,
                Source = source,
                StackCount = 1,
                MaxStacks = maxStacks
            };
        }

        /// <summary>
        /// Create a permanent effect (until dispelled).
        /// </summary>
        public static StatusEffect CreatePermanent(StatusEffectType type, float magnitude, Entity source = default)
        {
            return new StatusEffect
            {
                Type = type,
                Magnitude = magnitude,
                Duration = -1f,
                TimeSinceLastTick = 0f,
                TickInterval = 0f,
                Source = source,
                StackCount = 1,
                MaxStacks = 1
            };
        }
    }

    /// <summary>
    /// Configuration for a status effect type.
    /// Defines default behavior and constraints.
    /// </summary>
    public struct StatusEffectDefinition
    {
        public StatusEffectType Type;
        public StatusEffectCategory Category;
        public StatusEffectStacking StackingBehavior;
        public float DefaultDuration;
        public float DefaultTickInterval;
        public byte DefaultMaxStacks;
        public bool IsDispellable;
        public bool IsPositive;

        /// <summary>
        /// Get total effective magnitude considering stacks.
        /// </summary>
        public float GetEffectiveMagnitude(in StatusEffect effect)
        {
            return effect.Magnitude * effect.StackCount;
        }
    }

    /// <summary>
    /// Computed stat modifiers from all active status effects.
    /// Updated by StatusEffectSystem each frame.
    /// </summary>
    public struct StatusEffectModifiers : IComponentData
    {
        /// <summary>Speed multiplier (1.0 = normal)</summary>
        public float SpeedMultiplier;

        /// <summary>Damage dealt multiplier</summary>
        public float DamageMultiplier;

        /// <summary>Damage received multiplier</summary>
        public float DamageTakenMultiplier;

        /// <summary>Armor bonus (additive)</summary>
        public float ArmorBonus;

        /// <summary>Healing received multiplier</summary>
        public float HealingMultiplier;

        /// <summary>Whether entity can move</summary>
        public bool CanMove;

        /// <summary>Whether entity can attack</summary>
        public bool CanAttack;

        /// <summary>Whether entity can cast</summary>
        public bool CanCast;

        /// <summary>Whether entity is visible</summary>
        public bool IsVisible;

        /// <summary>Damage per second from all DoT effects</summary>
        public float TotalDoTDamage;

        /// <summary>Healing per second from all HoT effects</summary>
        public float TotalHoTHealing;

        /// <summary>
        /// Reset to default values.
        /// </summary>
        public static StatusEffectModifiers Default => new StatusEffectModifiers
        {
            SpeedMultiplier = 1f,
            DamageMultiplier = 1f,
            DamageTakenMultiplier = 1f,
            ArmorBonus = 0f,
            HealingMultiplier = 1f,
            CanMove = true,
            CanAttack = true,
            CanCast = true,
            IsVisible = true,
            TotalDoTDamage = 0f,
            TotalHoTHealing = 0f
        };
    }

    /// <summary>
    /// Tag for entities that have active status effects.
    /// Added when first effect applied, removed when all cleared.
    /// </summary>
    public struct HasStatusEffectsTag : IComponentData { }

    /// <summary>
    /// Request to apply a status effect to an entity.
    /// </summary>
    public struct ApplyStatusEffectRequest : IComponentData
    {
        public Entity Target;
        public StatusEffect Effect;
    }

    /// <summary>
    /// Request to remove/dispel a status effect.
    /// </summary>
    public struct RemoveStatusEffectRequest : IComponentData
    {
        public Entity Target;
        public StatusEffectType Type;
        public bool RemoveAllStacks;
    }
}

