using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Combat stats for villagers, derived from attributes or overridden.
    /// </summary>
    public struct VillagerCombatStats : IComponentData
    {
        /// <summary>
        /// To-hit chance (calculated: Finesse × 0.7 + Strength × 0.3, or overridden).
        /// Range: 0-100 (byte)
        /// </summary>
        public byte Attack;

        /// <summary>
        /// Dodge/block chance (calculated: Finesse × 0.6 + armor, or overridden).
        /// Range: 0-100 (byte)
        /// </summary>
        public byte Defense;

        /// <summary>
        /// Max HP (calculated: Strength × 0.6 + Will × 0.4 + 50, or overridden).
        /// </summary>
        public float MaxHealth;

        /// <summary>
        /// Current HP (0 = death).
        /// </summary>
        public float CurrentHealth;

        /// <summary>
        /// Rounds before exhaustion (calculated: Strength / 10, or overridden).
        /// </summary>
        public byte Stamina;

        /// <summary>
        /// Current stamina remaining.
        /// </summary>
        public byte CurrentStamina;

        /// <summary>
        /// Max mana for magic users (calculated: Will × 0.5 + Intelligence × 0.5, or 0 for non-magic).
        /// Range: 0-100 (byte)
        /// </summary>
        public byte MaxMana;

        /// <summary>
        /// Current mana remaining.
        /// </summary>
        public byte CurrentMana;

        /// <summary>
        /// Attack damage (for compatibility with PureDOTS VillagerCombatStats structure).
        /// </summary>
        public float AttackDamage;

        /// <summary>
        /// Attack speed (for compatibility with PureDOTS VillagerCombatStats structure).
        /// </summary>
        public float AttackSpeed;

        /// <summary>
        /// Current combat target entity.
        /// </summary>
        public Entity CurrentTarget;
    }
}


