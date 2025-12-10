using Godgame.Buildings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class WorkplaceAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float workCapacity = 5f;

        [SerializeField]
        private float efficiencyMultiplier = 1f;

        private sealed class Baker : Unity.Entities.Baker<WorkplaceAuthoring>
        {
            public override void Bake(WorkplaceAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new WorkplaceDefinition
                {
                    WorkCapacity = math.max(0f, authoring.workCapacity),
                    EfficiencyMultiplier = math.max(0f, authoring.efficiencyMultiplier)
                });
            }
        }
    }
}
