using Godgame.Miracles.Presentation;
using Godgame.Presentation.Authoring;
using PureDOTS.Environment;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// Resamples the shared environment grids for each ground tile, updates cached climate values,
    /// and derives presentation descriptor keys so swappable bindings track biome/moisture changes.
    /// </summary>
    [UpdateInGroup(typeof(EnvironmentSystemGroup))]
    [UpdateAfter(typeof(BiomeDerivationSystem))]
    public partial struct GroundTileDescriptorSystem : ISystem
    {
        private EntityQuery _gridConfigQuery;
        private EntityQuery _moistureGridQuery;
        private EntityQuery _temperatureGridQuery;
        private EntityQuery _biomeGridQuery;
        private ComponentLookup<SwappablePresentationDirtyTag> _dirtyLookup;
        private BufferLookup<MoistureGridRuntimeCell> _moistureRuntimeLookup;
        private BufferLookup<BiomeGridRuntimeCell> _biomeRuntimeLookup;

        private static readonly FixedString64Bytes s_DefaultDescriptorKey = new FixedString64Bytes("ground.default");
        private static readonly int s_DefaultDescriptorHash = Animator.StringToHash("ground.default");
        private static readonly FixedString64Bytes s_GroundPrefix = new FixedString64Bytes("ground.");
        private static readonly FixedString64Bytes s_GroundBiomePrefix = new FixedString64Bytes("ground.biome.");
        private static readonly FixedString64Bytes s_BiomeTundra = new FixedString64Bytes("tundra");
        private static readonly FixedString64Bytes s_BiomeTaiga = new FixedString64Bytes("taiga");
        private static readonly FixedString64Bytes s_BiomeGrassland = new FixedString64Bytes("grassland");
        private static readonly FixedString64Bytes s_BiomeForest = new FixedString64Bytes("forest");
        private static readonly FixedString64Bytes s_BiomeDesert = new FixedString64Bytes("desert");
        private static readonly FixedString64Bytes s_BiomeRainforest = new FixedString64Bytes("rainforest");
        private static readonly FixedString64Bytes s_BiomeSavanna = new FixedString64Bytes("savanna");
        private static readonly FixedString64Bytes s_BiomeSwamp = new FixedString64Bytes("swamp");
        private static readonly FixedString64Bytes s_BiomeDefault = new FixedString64Bytes("default");
        private static readonly FixedString32Bytes s_MoistureWet = new FixedString32Bytes("wet");
        private static readonly FixedString32Bytes s_MoistureTemperate = new FixedString32Bytes("temperate");
        private static readonly FixedString32Bytes s_MoistureDry = new FixedString32Bytes("dry");
        private static readonly FixedString32Bytes s_TempCold = new FixedString32Bytes("cold");
        private static readonly FixedString32Bytes s_TempHot = new FixedString32Bytes("hot");
        private static readonly FixedString32Bytes s_TempMild = new FixedString32Bytes("mild");

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GroundTile>();
            _dirtyLookup = state.GetComponentLookup<SwappablePresentationDirtyTag>(isReadOnly: true);
            _moistureRuntimeLookup = state.GetBufferLookup<MoistureGridRuntimeCell>(isReadOnly: true);
            _biomeRuntimeLookup = state.GetBufferLookup<BiomeGridRuntimeCell>(isReadOnly: true);
            _gridConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<EnvironmentGridConfigData>());
            _moistureGridQuery = state.GetEntityQuery(ComponentType.ReadOnly<MoistureGrid>());
            _temperatureGridQuery = state.GetEntityQuery(ComponentType.ReadOnly<TemperatureGrid>());
            _biomeGridQuery = state.GetEntityQuery(ComponentType.ReadOnly<BiomeGrid>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_gridConfigQuery.TryGetSingleton(out EnvironmentGridConfigData gridConfig))
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            _dirtyLookup.Update(ref state);
            _moistureRuntimeLookup.Update(ref state);
            _biomeRuntimeLookup.Update(ref state);

            var hasMoisture = _moistureGridQuery.TryGetSingleton(out MoistureGrid moistureGrid);
            NativeArray<MoistureGridRuntimeCell> moistureRuntime = default;
            if (hasMoisture)
            {
                var moistureEntity = _moistureGridQuery.GetSingletonEntity();
                if (_moistureRuntimeLookup.HasBuffer(moistureEntity))
                {
                    moistureRuntime = _moistureRuntimeLookup[moistureEntity].AsNativeArray();
                }
            }

            var hasTemperature = _temperatureGridQuery.TryGetSingleton(out TemperatureGrid temperatureGrid);

            var hasBiome = _biomeGridQuery.TryGetSingleton(out BiomeGrid biomeGrid);
            NativeArray<BiomeGridRuntimeCell> biomeRuntime = default;
            if (hasBiome)
            {
                var biomeEntity = _biomeGridQuery.GetSingletonEntity();
                if (_biomeRuntimeLookup.HasBuffer(biomeEntity))
                {
                    biomeRuntime = _biomeRuntimeLookup[biomeEntity].AsNativeArray();
                }
            }

            foreach (var (tile, moisture, temperature, terrain, biome, binding, entity) in SystemAPI
                         .Query<RefRO<GroundTile>, RefRW<GroundMoisture>, RefRW<GroundTemperature>, RefRW<GroundTerrainHeight>, RefRW<GroundBiome>, RefRW<SwappablePresentationBinding>>()
                         .WithEntityAccess())
            {
                int index = math.max(0, math.min(tile.ValueRO.CellIndex, GetCellCapacity(in gridConfig) - 1));

                float moistureValue = SampleMoisture(index, hasMoisture, moistureGrid, moistureRuntime);
                float temperatureValue = SampleTemperature(index, hasTemperature, temperatureGrid);
                float terrainHeight = SampleTerrainHeight(index, hasMoisture, moistureGrid);
                var biomeValue = SampleBiome(index, hasBiome, biomeRuntime);

                moisture.ValueRW.Value = moistureValue;
                temperature.ValueRW.Value = temperatureValue;
                terrain.ValueRW.Value = terrainHeight;
                biome.ValueRW.Biome = biomeValue;

                var descriptorKey = BuildDescriptorKey(biomeValue, hasBiome, moistureValue, hasMoisture, temperatureValue, hasTemperature);

                if (!binding.ValueRO.DescriptorKey.Equals(descriptorKey))
                {
                    binding.ValueRW.DescriptorKey = descriptorKey;
                    int hash = ComputeDescriptorHash(descriptorKey);
                    binding.ValueRW.DescriptorHash = hash;

                    if (!_dirtyLookup.HasComponent(entity))
                    {
                        ecb.AddComponent<SwappablePresentationDirtyTag>(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static int GetCellCapacity(in EnvironmentGridConfigData configData)
        {
            if (configData.BiomeEnabled != 0)
            {
                return math.max(1, configData.Biome.CellCount);
            }

            if (configData.Moisture.CellCount > 0)
            {
                return configData.Moisture.CellCount;
            }

            if (configData.Temperature.CellCount > 0)
            {
                return configData.Temperature.CellCount;
            }

            if (configData.Sunlight.CellCount > 0)
            {
                return configData.Sunlight.CellCount;
            }

            return math.max(1, configData.Wind.CellCount);
        }

        private static float SampleMoisture(int index, bool hasMoisture, in MoistureGrid grid, NativeArray<MoistureGridRuntimeCell> runtime)
        {
            if (!hasMoisture)
            {
                return 0f;
            }

            if (runtime.IsCreated && index >= 0 && index < runtime.Length)
            {
                return runtime[index].Moisture;
            }

            if (grid.Blob.IsCreated)
            {
                ref var blob = ref grid.Blob.Value.Moisture;
                if (index >= 0 && index < blob.Length)
                {
                    return blob[index];
                }
            }

            return 0f;
        }

        private static float SampleTemperature(int index, bool hasTemperature, in TemperatureGrid grid)
        {
            if (!hasTemperature || !grid.Blob.IsCreated)
            {
                return 0f;
            }

            ref var temps = ref grid.Blob.Value.TemperatureCelsius;
            if (index >= 0 && index < temps.Length)
            {
                return temps[index];
            }

            return 0f;
        }

        private static float SampleTerrainHeight(int index, bool hasMoisture, in MoistureGrid grid)
        {
            if (!hasMoisture || !grid.Blob.IsCreated)
            {
                return 0f;
            }

            ref var heights = ref grid.Blob.Value.TerrainHeight;
            if (index >= 0 && index < heights.Length)
            {
                return heights[index];
            }

            return 0f;
        }

        private static BiomeType SampleBiome(int index, bool hasBiome, NativeArray<BiomeGridRuntimeCell> runtime)
        {
            if (!hasBiome || !runtime.IsCreated)
            {
                return BiomeType.Unknown;
            }

            if (index >= 0 && index < runtime.Length)
            {
                return runtime[index].Value;
            }

            return BiomeType.Unknown;
        }

        private static FixedString64Bytes BuildDescriptorKey(BiomeType biome, bool hasBiome, float moisture, bool hasMoisture, float temperature, bool hasTemperature)
        {
            FixedString64Bytes key = default;

            if (hasBiome && biome != BiomeType.Unknown)
            {
                key.Append(s_GroundBiomePrefix);
                AppendBiomeName(ref key, biome);
                return key;
            }

            if (hasMoisture && hasTemperature)
            {
                key.Append(s_GroundPrefix);
                AppendMoistureBucket(ref key, moisture);
                key.Append('.');
                AppendTemperatureBucket(ref key, temperature);
                return key;
            }

            key.Append(s_DefaultDescriptorKey);
            return key;
        }

        private static int ComputeDescriptorHash(in FixedString64Bytes key)
        {
            var hash = key.GetHashCode();
            return hash == 0 ? s_DefaultDescriptorHash : hash;
        }

        private static void AppendBiomeName(ref FixedString64Bytes key, BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Tundra:
                    key.Append(s_BiomeTundra);
                    break;
                case BiomeType.Taiga:
                    key.Append(s_BiomeTaiga);
                    break;
                case BiomeType.Grassland:
                    key.Append(s_BiomeGrassland);
                    break;
                case BiomeType.Forest:
                    key.Append(s_BiomeForest);
                    break;
                case BiomeType.Desert:
                    key.Append(s_BiomeDesert);
                    break;
                case BiomeType.Rainforest:
                    key.Append(s_BiomeRainforest);
                    break;
                case BiomeType.Savanna:
                    key.Append(s_BiomeSavanna);
                    break;
                case BiomeType.Swamp:
                    key.Append(s_BiomeSwamp);
                    break;
                default:
                    key.Append(s_BiomeDefault);
                    break;
            }
        }

        private static void AppendMoistureBucket(ref FixedString64Bytes key, float moisture)
        {
            if (moisture >= 70f)
            {
                key.Append(s_MoistureWet);
            }
            else if (moisture >= 40f)
            {
                key.Append(s_MoistureTemperate);
            }
            else
            {
                key.Append(s_MoistureDry);
            }
        }

        private static void AppendTemperatureBucket(ref FixedString64Bytes key, float temperature)
        {
            if (temperature <= 0f)
            {
                key.Append(s_TempCold);
            }
            else if (temperature >= 28f)
            {
                key.Append(s_TempHot);
            }
            else
            {
                key.Append(s_TempMild);
            }
        }
    }
}
