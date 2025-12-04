using Unity.Collections;
using Unity.Entities;

namespace Godgame.Miracles.Presentation
{
    // Marks entities whose visuals can be swapped between variants.
    public struct SwappablePresentation : IComponentData
    {
        // Index into some variant set (different meshes/materials/etc.).
        public int VariantIndex;
    }

    // Binds a logical sim entity to a specific visual entity/variant.
    public struct SwappablePresentationBinding : IComponentData
    {
        // Descriptor key for the presentation variant
        public FixedString64Bytes DescriptorKey;
        // Hash of the descriptor key
        public int DescriptorHash;
    }

    // Tag used to signal that the binding/variant changed and needs refresh.
    public struct SwappablePresentationDirtyTag : IComponentData
    {
    }
}
