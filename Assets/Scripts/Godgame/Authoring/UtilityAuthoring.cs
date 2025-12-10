using Godgame.Buildings;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class UtilityAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float areaBonusRange = 10f;

        [SerializeField]
        private float bonusValue = 1f;

        [SerializeField]
        private string bonusType = "fertility";

        private sealed class Baker : Unity.Entities.Baker<UtilityAuthoring>
        {
            public override void Bake(UtilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new UtilityDefinition
                {
                    AreaBonusRange = math.max(0f, authoring.areaBonusRange),
                    BonusValue = authoring.bonusValue,
                    BonusType = new FixedString32Bytes(authoring.bonusType)
                });
            }
        }
    }
}
