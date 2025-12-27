using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Optional per-villager schedule override profile.
    /// </summary>
    public struct VillagerScheduleProfile : IComponentData
    {
        public VillagerScheduleConfig Schedule;
        public float Weight;
    }
}
