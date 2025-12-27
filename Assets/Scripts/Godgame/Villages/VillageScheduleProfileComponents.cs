using Godgame.Villagers;
using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Village-level schedule profile that can influence member routines.
    /// </summary>
    public struct VillageScheduleProfile : IComponentData
    {
        public VillagerScheduleConfig Schedule;
        public float Weight;
    }
}
