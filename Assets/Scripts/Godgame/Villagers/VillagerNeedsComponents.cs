using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Need stats tracking villager hunger, fatigue, sleep, and general health.
    /// Range: 0-100 (byte), where 0 = critical need, 100 = fully satisfied.
    /// </summary>
    public struct VillagerNeeds : IComponentData
    {
        /// <summary>
        /// Hunger level, 0 = starving.
        /// </summary>
        public byte Food;

        /// <summary>
        /// Fatigue level, 0 = exhausted.
        /// </summary>
        public byte Rest;

        /// <summary>
        /// Sleep need, 0 = sleep-deprived.
        /// </summary>
        public byte Sleep;

        /// <summary>
        /// Overall health status (separate from combat HP).
        /// </summary>
        public byte GeneralHealth;

        /// <summary>
        /// Current health value (for compatibility with PureDOTS VillagerNeeds structure).
        /// </summary>
        public float Health;

        /// <summary>
        /// Maximum health value (for compatibility with PureDOTS VillagerNeeds structure).
        /// </summary>
        public float MaxHealth;

        /// <summary>
        /// Energy level (0-100, for compatibility with PureDOTS VillagerNeeds structure).
        /// </summary>
        public float Energy;

        /// <summary>
        /// Current morale (0-100, for compatibility with PureDOTS VillagerNeeds structure).
        /// </summary>
        public float Morale;
    }

    /// <summary>
    /// Mood/ morale stat for villagers.
    /// Range: 0-100 (byte), where 0 = very unhappy, 100 = very happy.
    /// </summary>
    public struct VillagerMood : IComponentData
    {
        /// <summary>
        /// Current mood value (0-100).
        /// </summary>
        public float Mood;
    }
}


