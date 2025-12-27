using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tracks the current social focus target for a villager.
    /// </summary>
    public struct VillagerSocialFocus : IComponentData
    {
        public Entity Target;
        public uint NextPickTick;
    }
}
