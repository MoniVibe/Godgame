using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Runtime
{
    /// <summary>Marks an entity queued for throw (shift-queue) by a specific hand.</summary>
    public struct HandQueuedTag : IComponentData
    {
        public Entity Holder;
        public float3 StoredLinearVelocity;
        public float3 StoredAngularVelocity;
        public float StoredGravityFactor;
    }
}

