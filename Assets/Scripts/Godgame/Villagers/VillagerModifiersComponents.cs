using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Healing and spell effect modifiers for villagers.
    /// Values are multipliers (1.0 = no modifier, 1.2 = +20%, 0.8 = -20%).
    /// Stored as ushort (0-200) representing 0.0-2.0 range (0.01 precision).
    /// </summary>
    public struct VillagerModifiers : IComponentData
    {
        /// <summary>
        /// Multiplies healing received (e.g., 120 = 1.20 = +20% healing).
        /// Default: 100 (1.0 = no bonus)
        /// </summary>
        public ushort HealBonus; // 0-200 (represents 0.0-2.0)

        /// <summary>
        /// Modifies duration of own spells (e.g., 150 = 1.50 = +50% duration).
        /// Default: 100 (1.0 = no modifier)
        /// </summary>
        public ushort SpellDurationModifier; // 0-200 (represents 0.0-2.0)

        /// <summary>
        /// Modifies intensity/damage of own spells (e.g., 130 = 1.30 = +30% damage).
        /// Default: 100 (1.0 = no modifier)
        /// </summary>
        public ushort SpellIntensityModifier; // 0-200 (represents 0.0-2.0)
    }
}


