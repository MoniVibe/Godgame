using PureDOTS.Environment;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Tag to mark vegetation entities (trees, bushes, grass patches, etc.)
    /// </summary>
    public struct VegetationPresentationTag : IComponentData
    {
    }

    /// <summary>
    /// Cached biome and climate data for presentation systems.
    /// Written by Godgame_BiomeDataHookSystem, read by visualization systems.
    /// </summary>
    public struct BiomePresentationData : IComponentData
    {
        /// <summary>Biome type</summary>
        public BiomeType Biome;
        /// <summary>Moisture level (0-100)</summary>
        public float Moisture;
        /// <summary>Temperature in Celsius</summary>
        public float Temperature;
        /// <summary>Fertility (0-100, derived from moisture + temperature)</summary>
        public float Fertility;
        /// <summary>Dirty flag (0/1) - set to 1 when data needs update</summary>
        public byte IsDirty;
    }

    /// <summary>
    /// Visual state for vegetation entities.
    /// </summary>
    public struct VegetationVisualState : IComponentData
    {
        /// <summary>Growth stage (0=Seedling, 1=Growing, 2=Mature, 3=Decaying)</summary>
        public byte GrowthStage;
        /// <summary>Health (0-1)</summary>
        public float Health;
        /// <summary>Whether this is part of a clumped/instanced patch (0/1)</summary>
        public byte IsClumped;
        /// <summary>Color tint based on biome compatibility</summary>
        public float4 BiomeTint;
    }
}
