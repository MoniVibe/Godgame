using Unity.Entities;

namespace Godgame.Rendering
{
    /// <summary>
    /// High-level visual role identifiers used purely for coloring/rendering villagers.
    /// </summary>
    public enum VillagerRenderRoleId : byte
    {
        Miner = 0,
        Farmer = 1,
        Forester = 2,
        Breeder = 3,
        Worshipper = 4,
        Refiner = 5,
        Peacekeeper = 6,
        Combatant = 7
    }

    /// <summary>
    /// Optional component that can override the render palette for a villager.
    /// </summary>
    public struct VillagerRenderRole : IComponentData
    {
        public VillagerRenderRoleId Value;
    }
}
