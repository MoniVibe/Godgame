using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment
{
    /// <summary>
    /// Biome profile defining climate ranges, gameplay biases, and presentation tokens.
    /// </summary>
    public struct BiomeProfile
    {
        public FixedString32Bytes Id;
        public uint BiomeId32;

        // Climate ranges used by the resolver
        public float TempMin;
        public float TempMax;
        public float MoistMin;
        public float MoistMax;
        public float ElevMin;
        public float ElevMax;
        public float SlopeMaxDeg;

        // Gameplay hooks
        public byte VillagerStaminaDrainPct;   // +/- stamina cost modifier (0-255, 100 = no change)
        public byte DiseaseRiskPct;            // Disease risk percentage (0-100)
        public ushort ResourceBiasWood;        // Coarse weight for wood resource seeding (0-1000)
        public ushort ResourceBiasOre;         // Coarse weight for ore resource seeding (0-1000)

        // Presentation tokens (swappable)
        public byte MinimapPalette;            // UI color palette index
        public byte GroundStyle;               // Material/style token
        public byte WeatherProfileToken;       // Weather FX profile token
    }

    /// <summary>
    /// Blob asset containing all biome definitions.
    /// </summary>
    public struct BiomeDefinitionBlob
    {
        public BlobArray<BiomeProfile> Profiles;
    }

    /// <summary>
    /// Singleton component holding the biome definition blob reference.
    /// </summary>
    public struct BiomeDefinitionSingleton : IComponentData
    {
        public BlobAssetReference<BiomeDefinitionBlob> Definitions;
    }

    /// <summary>
    /// Global climate state (temperature, moisture, elevation).
    /// </summary>
    public struct ClimateState : IComponentData
    {
        public float TempC;           // Temperature in Celsius
        public float Moisture01;       // Moisture level 0-1
        public float ElevationMean;    // Mean elevation (for biome resolution)
    }

    /// <summary>
    /// Moisture grid (spatial or global).
    /// Starts as 1×1 global grid, expandable to spatial.
    /// </summary>
    public struct MoistureGrid : IComponentData
    {
        public int Width;
        public int Height;
        public BlobAssetReference<BlobArray<float>> Values;
    }

    /// <summary>
    /// Biome grid mapping spatial tiles to BiomeId32.
    /// Starts as 1×1 global grid, expandable to spatial.
    /// </summary>
    public struct BiomeGrid : IComponentData
    {
        public int Width;
        public int Height;
        public BlobAssetReference<BlobArray<uint>> BiomeIds;
    }

    /// <summary>
    /// Component applied to villagers for biome-based modifiers.
    /// </summary>
    public struct VillagerBiomeModifiers : IComponentData
    {
        public uint CurrentBiomeId32;
        public float StaminaMultiplier;    // Applied to stamina costs (1.0 = no change)
        public float DiseaseRiskMultiplier; // Applied to disease risk (1.0 = no change)
    }
}

