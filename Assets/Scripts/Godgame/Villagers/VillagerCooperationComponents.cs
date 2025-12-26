using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Lightweight cooperation intent for villagers to align resource and drop-off choices.
    /// </summary>
    public struct VillagerCooperationIntent : IComponentData
    {
        /// <summary>
        /// Resource type index this cooperation intent is focused on.
        /// </summary>
        public ushort ResourceTypeIndex;

        /// <summary>
        /// Urgency for honoring the cooperative intent (0-1).
        /// </summary>
        public float Urgency;

        /// <summary>
        /// Shared storehouse to use when coordinating deliveries.
        /// </summary>
        public Entity SharedStorehouse;

        /// <summary>
        /// Shared resource node to prioritize (optional).
        /// </summary>
        public Entity SharedNode;

        /// <summary>
        /// Tick when this intent was assigned.
        /// </summary>
        public uint AssignedTick;
    }
}
