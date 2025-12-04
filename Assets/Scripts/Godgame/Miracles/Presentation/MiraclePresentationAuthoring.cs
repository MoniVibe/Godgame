using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Miracles.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MiraclePresentationAuthoring : MonoBehaviour
    {
        public MiracleVisualProfile profile;
        public string baseDescriptor = "godgame.miracle.rain";

        private sealed class Baker : Baker<MiraclePresentationAuthoring>
        {
            public override void Bake(MiraclePresentationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var descriptorKey = string.IsNullOrWhiteSpace(authoring.baseDescriptor)
                    ? string.Empty
                    : authoring.baseDescriptor.Trim();

                FixedString64Bytes descriptor = default;
                if (!string.IsNullOrEmpty(descriptorKey))
                {
                    descriptor.CopyFromTruncated(descriptorKey);
                }

                AddComponent(entity, new MiraclePresentationBinding
                {
                    Profile = new UnityObjectRef<MiracleVisualProfile> { Value = authoring.profile },
                    BaseDescriptor = descriptor,
                    LastDescriptorHash = 0,
                    LastIntensity = 1f
                });

                AddComponent(entity, new SwappablePresentationBinding
                {
                    DescriptorKey = descriptor,
                    DescriptorHash = string.IsNullOrEmpty(descriptorKey) ? 0 : Animator.StringToHash(descriptorKey)
                });

                AddComponent<SwappablePresentationDirtyTag>(entity);
            }
        }
    }
}
