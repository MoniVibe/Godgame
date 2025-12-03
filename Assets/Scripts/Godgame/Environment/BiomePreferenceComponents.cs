using Unity.Collections;
using Unity.Entities;

namespace Godgame.Environment
{
    /// <summary>
    /// Biome preference for races/cultures/villager types.
    /// Defines which biomes a race/culture prefers or avoids.
    /// </summary>
    public struct BiomePreference
    {
        public FixedString64Bytes RaceOrCultureId;
        public uint PreferredBiomeMask;    // Bitmask of preferred biome IDs
        public uint AvoidedBiomeMask;       // Bitmask of avoided biome IDs
        public float PreferenceWeight;     // Weight multiplier for preferred biomes (1.0 = neutral, >1.0 = preferred)
        public float AvoidancePenalty;     // Penalty multiplier for avoided biomes (1.0 = neutral, <1.0 = penalty)
    }

    /// <summary>
    /// Blob asset containing biome preferences for races/cultures.
    /// </summary>
    public struct BiomePreferenceBlob
    {
        public BlobArray<BiomePreference> Preferences;
    }

    /// <summary>
    /// Singleton component holding biome preference blob reference.
    /// </summary>
    public struct BiomePreferenceSingleton : IComponentData
    {
        public BlobAssetReference<BiomePreferenceBlob> Preferences;
    }

    /// <summary>
    /// Component applied to villagers/entities indicating their race/culture.
    /// Used to look up biome preferences.
    /// </summary>
    public struct RaceOrCulture : IComponentData
    {
        public FixedString64Bytes Id;
    }
}

