using PureDOTS.Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Godgame.Environment
{
    /// <summary>
    /// Bootstrap system that ensures biome singletons and grids exist.
    /// Runs early in initialization to set up the environment state.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BiomeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomeDefinitionSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Ensure ClimateState singleton exists
            if (!SystemAPI.HasSingleton<ClimateState>())
            {
                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var climateEntity = ecb.CreateEntity();
                ecb.AddComponent(climateEntity, new ClimateState
                {
                    TempC = 17f,        // Default temperate temperature
                    Moisture01 = 0.6f,  // Default moderate moisture
                    ElevationMean = 0f   // Default sea level
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            // Ensure WeatherState singleton exists
            if (!SystemAPI.HasSingleton<WeatherState>())
            {
                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var weatherEntity = ecb.CreateEntity();
                ecb.AddComponent(weatherEntity, new WeatherState
                {
                    Current = WeatherType.Clear,
                    Target = WeatherType.Clear,
                    Intensity = 0f,
                    MoisturePercent = 0.6f,
                    TemperatureCelsius = 17f,
                    ActivePhase = (byte)TimeOfDayPhase.Morning,
                    DominantBiome = BiomeType.Unknown,
                    DominantBiomeMoisture = 0.6f,
                    LastChangeTick = 0
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            // Ensure MoistureGrid singleton exists (1×1 global grid)
            if (!SystemAPI.HasSingleton<MoistureGrid>())
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobArray<float>>();
                var moistureArray = builder.Allocate(ref root, 1);
                moistureArray[0] = 0.6f; // Default moisture
                var blobAsset = builder.CreateBlobAssetReference<BlobArray<float>>(Allocator.Persistent);
                builder.Dispose();

                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var moistureEntity = ecb.CreateEntity();
                ecb.AddComponent(moistureEntity, new MoistureGrid
                {
                    Width = 1,
                    Height = 1,
                    Values = blobAsset
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            // Ensure BiomeGrid singleton exists (1×1 global grid)
            if (!SystemAPI.HasSingleton<BiomeGrid>())
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobArray<uint>>();
                var biomeArray = builder.Allocate(ref root, 1);
                biomeArray[0] = 1; // Default to first biome (Temperate)
                var blobAsset = builder.CreateBlobAssetReference<BlobArray<uint>>(Allocator.Persistent);
                builder.Dispose();

                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var biomeEntity = ecb.CreateEntity();
                ecb.AddComponent(biomeEntity, new BiomeGrid
                {
                    Width = 1,
                    Height = 1,
                    BiomeIds = blobAsset
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            // Disable this system after first run
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonRW<MoistureGrid>(out var moistureGrid))
            {
                ref var grid = ref moistureGrid.ValueRW;
                if (grid.Values.IsCreated)
                {
                    grid.Values.Dispose();
                    grid.Values = default;
                }
            }

            if (SystemAPI.TryGetSingletonRW<BiomeGrid>(out var biomeGrid))
            {
                ref var grid = ref biomeGrid.ValueRW;
                if (grid.BiomeIds.IsCreated)
                {
                    grid.BiomeIds.Dispose();
                    grid.BiomeIds = default;
                }
            }
        }
    }
}
