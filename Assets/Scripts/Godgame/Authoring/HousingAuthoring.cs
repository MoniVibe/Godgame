using Godgame.Buildings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class HousingAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int maxResidents = 4;

        [SerializeField]
        private float comfortLevel = 10f;

        [SerializeField]
        private float restorationRate = 1f;

        private sealed class Baker : Unity.Entities.Baker<HousingAuthoring>
        {
            public override void Bake(HousingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new HousingDefinition
                {
                    MaxResidents = math.max(0, authoring.maxResidents),
                    ComfortLevel = authoring.comfortLevel,
                    RestorationRate = authoring.restorationRate
                });
            }
        }
    }
}
