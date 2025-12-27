using PureDOTS.Runtime.AI;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring hook for configuring villager carry capacity modifiers.
    /// </summary>
    public sealed class VillagerCarryCapacityAuthoring : MonoBehaviour
    {
        [Min(0f)] public float multiplier = 1f;
        public float bonus = 0f;

        private sealed class Baker : Baker<VillagerCarryCapacityAuthoring>
        {
            public override void Bake(VillagerCarryCapacityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerCarryCapacity
                {
                    Multiplier = Mathf.Max(0f, authoring.multiplier),
                    Bonus = authoring.bonus
                });
            }
        }
    }
}
