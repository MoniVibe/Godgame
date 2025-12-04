using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Alignment values for individual villagers (tri-axis: Moral, Order, Purity).
    /// Each axis ranges from -100 to +100.
    /// Used when evaluating villager relationships (Good/Evil, Lawful/Chaotic, Pure/Corrupt).
    /// </summary>
    public struct VillagerAlignment : IComponentData
    {
        /// <summary>
        /// Moral axis: Good (+100) ↔ Neutral (0) ↔ Evil (-100)
        /// </summary>
        public sbyte MoralAxis;

        /// <summary>
        /// Order axis: Lawful (+100) ↔ Neutral (0) ↔ Chaotic (-100)
        /// </summary>
        public sbyte OrderAxis;

        /// <summary>
        /// Purity axis: Pure (+100) ↔ Neutral (0) ↔ Corrupt (-100)
        /// </summary>
        public sbyte PurityAxis;

        /// <summary>
        /// Neutral/default alignment for generic NPCs.
        /// </summary>
        public static VillagerAlignment Neutral => new VillagerAlignment
        {
            MoralAxis = 0,
            OrderAxis = 0,
            PurityAxis = 0
        };
    }
}
