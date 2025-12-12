#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Environment;
using Godgame.Presentation.Bindings;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Presentation.Bindings
{
    /// <summary>
    /// Bridge system that reads weather state and biome bindings to push weather FX requests.
    /// Maps WeatherState.WeatherToken to presentation tokens via biome bindings.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct WeatherBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeatherState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var weatherState = SystemAPI.GetSingleton<WeatherState>();
            var biomeGrid = SystemAPI.GetSingleton<BiomeGrid>();

            // Get current biome ID (1×1 grid for MVP)
            uint currentBiomeId = 1; // Default
            if (biomeGrid.BiomeIds.IsCreated && biomeGrid.Width > 0 && biomeGrid.Height > 0)
            {
                currentBiomeId = biomeGrid.BiomeIds.Value[0];
            }

            // Try to get biome bindings (optional)
            byte weatherProfileToken = weatherState.WeatherToken; // Fallback to raw token
            if (SystemAPI.TryGetSingleton<BiomeBindingSingleton>(out var bindings))
            {
                // Find binding entry for current biome (use Minimal by default)
                if (bindings.MinimalBindings.IsCreated)
                {
                    ref var entries = ref bindings.MinimalBindings.Value.Entries;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        if (entries[i].BiomeId32 == currentBiomeId)
                        {
                            // Use biome's weather profile token, but override with active weather if needed
                            weatherProfileToken = weatherState.WeatherToken != 0 
                                ? weatherState.WeatherToken 
                                : entries[i].WeatherProfile;
                            break;
                        }
                    }
                }
            }

            // Update or create weather request singleton
            if (!SystemAPI.HasSingleton<BiomeWeatherRequest>())
            {
                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var requestEntity = ecb.CreateEntity();
                ecb.AddComponent(requestEntity, new BiomeWeatherRequest
                {
                    WeatherProfileToken = weatherProfileToken,
                    Intensity01 = weatherState.Intensity01,
                    IsDirty = 1
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
            else
            {
                var request = SystemAPI.GetSingletonRW<BiomeWeatherRequest>();
                if (request.ValueRO.WeatherProfileToken != weatherProfileToken ||
                    math.abs(request.ValueRO.Intensity01 - weatherState.Intensity01) > 0.01f)
                {
                    request.ValueRW.WeatherProfileToken = weatherProfileToken;
                    request.ValueRW.Intensity01 = weatherState.Intensity01;
                    request.ValueRW.IsDirty = 1;
                }
            }

            // Note: Actual weather FX application happens in presentation systems
            // that read BiomeWeatherRequest and apply changes via ECB or COZY Weather integration
        }
    }
}
#endif
