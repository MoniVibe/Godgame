using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    [DisallowMultipleComponent]
    public sealed class StaticVisualAuthoring : MonoBehaviour
    {
        [Header("Visual Prefab")]
        public GameObject visualPrefab;

        [Header("Local Offsets")]
        public Vector3 localOffset = Vector3.zero;
        public Vector3 localRotationOffsetEuler = Vector3.zero;
        public float scaleMultiplier = 1f;
        public bool inheritParentScale = true;

        private class Baker : Unity.Entities.Baker<StaticVisualAuthoring>
        {
            public override void Bake(StaticVisualAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                if (authoring.visualPrefab == null)
                {
                    Debug.LogWarning($"StaticVisualAuthoring on '{authoring.name}' is missing a visual prefab reference.", authoring);
                    return;
                }

                AddComponent(entity, new StaticVisualPrefab
                {
                    LocalOffset = authoring.localOffset,
                    LocalRotationOffset = quaternion.Euler(math.radians(authoring.localRotationOffsetEuler)),
                    ScaleMultiplier = math.max(0.01f, authoring.scaleMultiplier),
                    InheritParentScale = (byte)(authoring.inheritParentScale ? 1 : 0)
                });

                AddComponent(entity, new StaticVisualPrefabReference
                {
                    Prefab = new UnityObjectRef<GameObject> { Value = authoring.visualPrefab }
                });
            }
        }
    }
}
