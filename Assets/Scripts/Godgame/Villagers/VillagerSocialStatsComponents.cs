using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Social stats tracking villager reputation, wealth, and achievements.
    /// </summary>
    public struct VillagerSocialStats : IComponentData
    {
        /// <summary>
        /// Public recognition, legendary status threshold at 500+.
        /// Range: 0-1000 (ushort)
        /// </summary>
        public ushort Fame;

        /// <summary>
        /// Liquid wealth + asset value (currency).
        /// Stored as float for precision, but typically displayed as integer.
        /// </summary>
        public float Wealth;

        /// <summary>
        /// Standing in community.
        /// Range: -100 to +100 (sbyte)
        /// </summary>
        public sbyte Reputation;

        /// <summary>
        /// Combat achievements, heroic deeds.
        /// Range: 0-1000 (ushort)
        /// </summary>
        public ushort Glory;

        /// <summary>
        /// Overall legendary status (combines fame + glory).
        /// Range: 0-1000 (ushort)
        /// </summary>
        public ushort Renown;
    }
}


