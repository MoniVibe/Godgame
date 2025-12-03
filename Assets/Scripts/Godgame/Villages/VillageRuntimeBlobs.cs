using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Runtime blob asset containing alignment axis definitions (baked from authoring catalog).
    /// </summary>
    public struct VillageAlignmentDefinitionsBlob
    {
        public BlobArray<VillageAlignmentAxisBlob> Axes;
    }

    public struct VillageAlignmentAxisBlob
    {
        public FixedString64Bytes AxisId;
        public InitiativeResponseBlob InitiativeResponse;
        public SurplusPriorityWeightsBlob SurplusWeights;
    }

    public struct InitiativeResponseBlob
    {
        public float MinBias;
        public float MaxBias;
        public byte EasingType;
    }

    public struct SurplusPriorityWeightsBlob
    {
        public float Build;
        public float Defend;
        public float Proselytize;
        public float Research;
        public float Migrate;
    }

    /// <summary>
    /// Component storing reference to alignment catalog blob asset.
    /// </summary>
    public struct VillageAlignmentCatalogBlob : IComponentData
    {
        public BlobAssetReference<VillageAlignmentDefinitionsBlob> Catalog;
    }

    /// <summary>
    /// Runtime blob asset containing initiative band lookup table.
    /// </summary>
    public struct InitiativeBandLookupBlob
    {
        public BlobArray<InitiativeBandBlob> Bands;
    }

    public struct InitiativeBandBlob
    {
        public FixedString32Bytes BandId;
        public uint TickBudget;
        public uint RecoveryHalfLife;
        public StressBreakpointsBlob StressBreakpoints;
    }

    public struct StressBreakpointsBlob
    {
        public int Panic;
        public int Rally;
        public int Frenzy;
    }

    /// <summary>
    /// Component storing reference to initiative band lookup blob asset.
    /// </summary>
    public struct InitiativeBandLookupBlobComponent : IComponentData
    {
        public BlobAssetReference<InitiativeBandLookupBlob> Lookup;
    }
}
