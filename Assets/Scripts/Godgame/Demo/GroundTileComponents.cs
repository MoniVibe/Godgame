using PureDOTS.Environment;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Demo
{
    /// <summary>
    /// Identifies a ground tile entity that mirrors one cell from the shared environment grids.
    /// Stores lookup indices so runtime systems can resample the grid quickly.
    /// </summary>
    public struct GroundTile : IComponentData
    {
        public int CellIndex;
        public int2 CellCoord;
        public float3 Center;
        public float CellSize;
    }

    public struct GroundMoisture : IComponentData
    {
        public float Value;
    }

    public struct GroundTemperature : IComponentData
    {
        public float Value;
    }

    public struct GroundTerrainHeight : IComponentData
    {
        public float Value;
    }

    public struct GroundBiome : IComponentData
    {
        public byte Value;

        public BiomeType Biome
        {
            readonly get => (BiomeType)Value;
            set => Value = (byte)value;
        }

        public static GroundBiome FromBiome(BiomeType biome)
        {
            return new GroundBiome { Value = (byte)biome };
        }
    }
}
