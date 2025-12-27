using Godgame.Villagers;
using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Village-level directive profile to bias member routines.
    /// </summary>
    public struct VillageDirectiveProfile : IComponentData
    {
        public VillagerDirectiveWeights Weights;
        public float Weight;
        public float MinDurationSeconds;
        public float MaxDurationSeconds;
    }
}
