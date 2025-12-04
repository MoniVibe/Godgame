using System;
using PureDOTS.Environment;

namespace Godgame.Environment
{
    /// <summary>
    /// Simple bitmask wrapper so designers can target multiple biomes without hand-maintaining descriptor strings.
    /// </summary>
    [Flags]
    public enum BiomeMask : ushort
    {
        None = 0,
        Tundra = 1 << 0,
        Taiga = 1 << 1,
        Grassland = 1 << 2,
        Forest = 1 << 3,
        Desert = 1 << 4,
        Rainforest = 1 << 5,
        Savanna = 1 << 6,
        Swamp = 1 << 7,
        All = ushort.MaxValue
    }

    public static class BiomeMaskUtility
    {
        public static bool Allows(this BiomeMask mask, BiomeType biome)
        {
            if (mask == BiomeMask.All)
            {
                return true;
            }

            var bit = biome switch
            {
                BiomeType.Tundra => BiomeMask.Tundra,
                BiomeType.Taiga => BiomeMask.Taiga,
                BiomeType.Grassland => BiomeMask.Grassland,
                BiomeType.Forest => BiomeMask.Forest,
                BiomeType.Desert => BiomeMask.Desert,
                BiomeType.Rainforest => BiomeMask.Rainforest,
                BiomeType.Savanna => BiomeMask.Savanna,
                BiomeType.Swamp => BiomeMask.Swamp,
                _ => BiomeMask.None
            };

            return (mask & bit) != 0;
        }
    }
}
