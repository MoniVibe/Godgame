using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation.Bindings
{
    /// <summary>
    /// Biome binding entry mapping biome ID to presentation tokens.
    /// </summary>
    public struct BiomeBindingEntry
    {
        public uint BiomeId32;
        public byte MinimapPalette;   // UI color palette index
        public byte GroundStyle;      // Material/style token
        public byte WeatherProfile;   // Weather FX profile token
    }

    /// <summary>
    /// Blob asset containing biome presentation bindings.
    /// </summary>
    public struct BiomeBindingBlob
    {
        public BlobArray<BiomeBindingEntry> Entries;
    }

    /// <summary>
    /// Singleton component holding biome binding blob references.
    /// Supports multiple binding sets (Minimal, Fancy, etc.)
    /// </summary>
    public struct BiomeBindingSingleton : IComponentData
    {
        public BlobAssetReference<BiomeBindingBlob> MinimalBindings;
        public BlobAssetReference<BiomeBindingBlob> FancyBindings;
    }

    /// <summary>
    /// Component requesting ground material/style update based on biome.
    /// </summary>
    public struct BiomeGroundStyleRequest : IComponentData
    {
        public byte GroundStyleToken; // Material/style token from biome binding
        public byte IsDirty;          // Flag to trigger update
    }

    /// <summary>
    /// Component requesting weather FX update based on biome/weather state.
    /// </summary>
    public struct BiomeWeatherRequest : IComponentData
    {
        public byte WeatherProfileToken; // Weather FX profile token
        public float Intensity01;         // Weather intensity 0-1
        public byte IsDirty;              // Flag to trigger update
    }
}

