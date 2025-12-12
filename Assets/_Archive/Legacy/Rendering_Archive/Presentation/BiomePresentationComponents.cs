#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using PureDOTS.Environment;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// Tag component for biome overlay chunks (far LOD visualization).
    /// </summary>
    public struct BiomeOverlayChunk : IComponentData
    {
        /// <summary>Center position of the chunk</summary>
        public Unity.Mathematics.float3 CenterPosition;
        /// <summary>Radius of the chunk</summary>
        public float Radius;
        /// <summary>Dominant biome in this chunk</summary>
        public BiomeType DominantBiome;
        /// <summary>Average moisture in this chunk</summary>
        public float AverageMoisture;
    }
}
#endif
