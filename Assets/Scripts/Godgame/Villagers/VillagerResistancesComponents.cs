using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Damage type resistances for villagers.
    /// Values are multipliers (0.0 = no resistance, 1.0 = 100% immunity).
    /// Stored as byte (0-100) for Burst efficiency, converted to float (0.0-1.0) when needed.
    /// </summary>
    public struct VillagerResistances : IComponentData
    {
        /// <summary>
        /// Physical damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Physical;

        /// <summary>
        /// Fire damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Fire;

        /// <summary>
        /// Cold damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Cold;

        /// <summary>
        /// Poison damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Poison;

        /// <summary>
        /// Magic damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Magic;

        /// <summary>
        /// Lightning damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Lightning;

        /// <summary>
        /// Holy damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Holy;

        /// <summary>
        /// Dark damage resistance (0-100, represents 0-100%)
        /// </summary>
        public byte Dark;
    }
}


