using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tracks wandering/lingering offsets for non-work goals.
    /// </summary>
    public struct VillagerNeedMovementState : IComponentData
    {
        public float3 AnchorOffset;
        public float LingerSeconds;
        public uint NextRepathTick;
    }
}
