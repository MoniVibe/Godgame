using Godgame.Environment.Vegetation;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation.Bindings
{
    /// <summary>
    /// Vegetation binding entry mapping (PlantId, GrowthStage) to presentation tokens.
    /// </summary>
    public struct VegetationBindingEntry
    {
        public FixedString64Bytes PlantId;
        public GrowthStage Stage;
        public byte StylePalette;   // Presentation color/style palette token
        public byte StylePattern;   // Visual pattern/shape token (e.g., Cone for tree, Capsule for sapling)
        public byte PrefabToken;    // Placeholder prefab identifier token
    }

    /// <summary>
    /// Blob asset containing vegetation presentation bindings.
    /// </summary>
    public struct VegetationBindingBlob
    {
        public BlobArray<VegetationBindingEntry> Entries;
    }

    /// <summary>
    /// Singleton component holding vegetation binding blob references.
    /// Supports Minimal and Fancy binding sets.
    /// </summary>
    public struct VegetationBindingSingleton : IComponentData
    {
        public BlobAssetReference<VegetationBindingBlob> MinimalBindings;
        public BlobAssetReference<VegetationBindingBlob> FancyBindings;
    }

    /// <summary>
    /// Component requesting vegetation visual update based on plant state.
    /// </summary>
    public struct VegetationVisualRequest : IComponentData
    {
        public FixedString64Bytes PlantId;
        public GrowthStage Stage;
        public byte StylePalette;
        public byte StylePattern;
        public byte PrefabToken;
        public byte IsDirty;        // Flag to trigger update
    }
}

