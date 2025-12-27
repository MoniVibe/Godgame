using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tracks short ponder delays when evaluating work tasks.
    /// </summary>
    public struct VillagerPonderState : IComponentData
    {
        public float RemainingSeconds;
        public float3 AnchorPosition;
    }
}
