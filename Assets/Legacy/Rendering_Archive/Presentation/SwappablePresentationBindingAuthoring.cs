using Godgame.Miracles.Presentation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Presentation.Authoring
{
    public class SwappablePresentationBindingAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public FixedString64Bytes DescriptorKey;
        public int DescriptorHash;

        public class Baker : Baker<SwappablePresentationBindingAuthoring>
        {
            public override void Bake(SwappablePresentationBindingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new SwappablePresentationBinding
                {
                    DescriptorKey = authoring.DescriptorKey,
                    DescriptorHash = authoring.DescriptorHash
                });
                AddComponent<SwappablePresentationDirtyTag>(entity);
            }
        }
    }
}
