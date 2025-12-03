using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Core attributes (experience modifiers) for villagers.
    /// These modify experience gain in their respective pools.
    /// Range: 0-100 (byte)
    /// </summary>
    public struct VillagerAttributes : IComponentData
    {
        /// <summary>
        /// Physical power, muscle, endurance. Modifies Strength experience gain.
        /// </summary>
        public byte Physique;

        /// <summary>
        /// Skill, speed, agility, precision. Modifies Finesse experience gain.
        /// </summary>
        public byte Finesse;

        /// <summary>
        /// Mental fortitude, courage, determination. Modifies Will experience gain.
        /// </summary>
        public byte Will;

        /// <summary>
        /// Accumulates and generates general experience. Higher wisdom = faster overall progression.
        /// </summary>
        public byte Wisdom;
    }

    /// <summary>
    /// Derived attributes calculated from core attributes + experience.
    /// Range: 0-100 (byte)
    /// </summary>
    public struct VillagerDerivedAttributes : IComponentData
    {
        /// <summary>
        /// Physical power (derived from Physique + experience)
        /// </summary>
        public byte Strength;

        /// <summary>
        /// Speed and dexterity (derived from Finesse + experience)
        /// </summary>
        public byte Agility;

        /// <summary>
        /// Mental acuity (derived from Will + experience, affects magic)
        /// </summary>
        public byte Intelligence;
    }
}


