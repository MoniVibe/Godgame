using Godgame.Buildings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class WorshipAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float manaGenerationRate = 1f;

        [SerializeField]
        private float worshipperCapacity = 10f;

        private sealed class Baker : Unity.Entities.Baker<WorshipAuthoring>
        {
            public override void Bake(WorshipAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new WorshipDefinition
                {
                    ManaGenerationRate = math.max(0f, authoring.manaGenerationRate),
                    WorshipperCapacity = math.max(0f, authoring.worshipperCapacity)
                });
            }
        }
    }
}
