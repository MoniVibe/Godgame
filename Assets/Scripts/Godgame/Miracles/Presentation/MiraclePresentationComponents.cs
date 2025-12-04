using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using PureDOTS.Runtime.Components;

namespace Godgame.Miracles.Presentation
{
    public struct MiraclePresentationBinding : IComponentData
    {
        public UnityObjectRef<MiracleVisualProfile> Profile;
        public FixedString64Bytes BaseDescriptor;
        public int LastDescriptorHash;
        public float LastIntensity;
    }

    // Note: MiracleDesignerTriggerSource is defined in PureDOTS.Runtime.Components
    // This Godgame-specific version uses UnityObjectRef for authoring compatibility
    // Both can coexist in different namespaces
    public struct MiracleDesignerTriggerSource : IComponentData
    {
        public UnityObjectRef<MiracleVisualProfile> Profile;
        public MiracleType Type;
        public float3 Offset;
    }

    // Note: MiracleDesignerTrigger is defined in PureDOTS.Runtime.Components
    // This Godgame-specific version can coexist, but we should use PureDOTS version for consistency
    // Keeping this for now to avoid breaking existing code
    /// <summary>
    /// Simple request struct designers can enqueue via authoring triggers to force a miracle spawn for preview.
    /// </summary>
    public struct MiracleDesignerTrigger : IBufferElementData
    {
        public FixedString64Bytes DescriptorKey;
        public float3 Position;
        public MiracleType Type;
    }
}
