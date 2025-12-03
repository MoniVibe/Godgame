using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Vegetation
{
    /// <summary>
    /// Growth stages for vegetation entities.
    /// </summary>
    public enum GrowthStage : byte
    {
        Seedling = 0,
        Sapling = 1,
        Mature = 2,
        Dead = 3
    }

    /// <summary>
    /// Hazard flags for plants (bitmask).
    /// </summary>
    [System.Flags]
    public enum PlantHazardFlags : uint
    {
        None = 0,
        Poisonous = 1 << 0,
        Flammable = 1 << 1,
        Spreads = 1 << 2,  // Can spread/reproduce aggressively
        Invasive = 1 << 3  // Spreads and displaces other plants
    }

    /// <summary>
    /// Loot/yield entry for plant harvesting.
    /// </summary>
    public struct PlantLootEntry
    {
        public FixedString64Bytes MaterialId;  // Material ID from catalog
        public float Weight;                   // Spawn weight (0-1)
        public float MinAmount;                // Minimum yield
        public float MaxAmount;                // Maximum yield
    }

    /// <summary>
    /// Style tokens for presentation (no asset references in ECS).
    /// </summary>
    public struct PlantStyleTokens
    {
        public byte StylePalette;   // Color palette index
        public byte StylePattern;  // Visual pattern/shape token
        public byte FamilyToken;    // Plant family identifier
    }

    /// <summary>
    /// Plant specification defining species/family properties, preferences, growth, and yields.
    /// </summary>
    public struct PlantSpec
    {
        public FixedString64Bytes Id;
        public FixedString64Bytes VariantOf;  // If non-empty, inherits from this plant and applies deltas
        public uint BiomeMask;                 // Bitmask of allowed biome IDs (1 << BiomeId32)

        // Climate preferences
        public float TempMin;
        public float TempMax;
        public float MoistMin;
        public float MoistMax;
        public float SunMin;        // Sunlight requirement (0-1)
        public float SunMax;

        // Terrain preferences
        public float ElevMin;
        public float ElevMax;
        public float SlopeMaxDeg;

        // Soil preferences (bitmask)
        public uint SoilMask;       // Soil type compatibility flags

        // Hazard flags
        public PlantHazardFlags HazardFlags;

        // Growth stage durations (seconds)
        public float StageSecSeedling;
        public float StageSecSapling;
        public float StageSecMature;

        // Reproduction
        public float ReproSeedsPerDay;  // Average seeds produced per day when mature
        public float ReproRadius;        // Cluster radius for reproduction
        public uint SeasonMask;          // Bitmask of seasons when reproduction occurs

        // Yields (harvestable materials)
        public BlobArray<PlantLootEntry> Yields;

        // Presentation tokens
        public PlantStyleTokens Style;
    }

    /// <summary>
    /// Blob asset containing all plant specifications.
    /// </summary>
    public struct PlantSpecBlob
    {
        public BlobArray<PlantSpec> Plants;
    }

    /// <summary>
    /// Stand/patch specification for vegetation clustering.
    /// </summary>
    public struct StandSpec
    {
        public FixedString64Bytes Id;
        public FixedString64Bytes PlantId;     // Primary plant species

        // Spawning rules
        public float Density;                  // Plants per square meter
        public float Clustering;               // Clustering factor (0=uniform, 1=clustered)
        public float MinDistance;              // Minimum distance between plants
        public float SpawnRadius;               // Spawn radius for stand

        // Biome spawn weights (per biome ID)
        public BlobArray<float> SpawnWeightsPerBiome;  // Indexed by BiomeId32 - 1
    }

    /// <summary>
    /// Blob asset containing all stand specifications.
    /// </summary>
    public struct StandSpecBlob
    {
        public BlobArray<StandSpec> Stands;
    }

    /// <summary>
    /// Singleton component holding plant spec blob reference.
    /// </summary>
    public struct PlantSpecSingleton : IComponentData
    {
        public BlobAssetReference<PlantSpecBlob> Specs;
    }

    /// <summary>
    /// Singleton component holding stand spec blob reference.
    /// </summary>
    public struct StandSpecSingleton : IComponentData
    {
        public BlobAssetReference<StandSpecBlob> Specs;
    }

    /// <summary>
    /// Runtime state component for individual plant entities.
    /// </summary>
    public struct PlantState : IComponentData
    {
        public FixedString64Bytes PlantId;
        public GrowthStage Stage;
        public float AgeSeconds;
        public float Health01;          // Health 0-1 (affected by stress)
        public float StressLevel01;     // Stress from drought/fire/etc (0-1)
    }

    /// <summary>
    /// Component marking a plant as part of a stand/cluster.
    /// </summary>
    public struct PlantStand : IComponentData
    {
        public FixedString64Bytes StandId;
        public Entity StandCenter;      // Reference to stand center entity (if exists)
        public float DistanceFromCenter;
    }

    /// <summary>
    /// Command buffer element for plant reproduction requests.
    /// </summary>
    public struct ReproductionCommand : IBufferElementData
    {
        public FixedString64Bytes PlantId;
        public float3 Position;
        public float Radius;
        public uint SeedCount;
    }
}

