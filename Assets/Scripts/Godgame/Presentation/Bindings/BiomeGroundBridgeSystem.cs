using Godgame.Environment;
using Godgame.Presentation.Bindings;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Presentation.Bindings
{
    /// <summary>
    /// Bridge system that reads biome grid and pushes ground style tokens to presentation.
    /// Updates ground material/style based on current biome.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct BiomeGroundBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomeGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var biomeGrid = SystemAPI.GetSingleton<BiomeGrid>();
            if (!biomeGrid.BiomeIds.IsCreated || biomeGrid.Width == 0 || biomeGrid.Height == 0)
            {
                return;
            }

            // Get current biome ID (1×1 grid for MVP)
            uint currentBiomeId = biomeGrid.BiomeIds.Value[0];

            // Try to get biome bindings (optional - presentation can work without them)
            if (!SystemAPI.TryGetSingleton<BiomeBindingSingleton>(out var bindings))
            {
                return; // No bindings configured, skip presentation updates
            }

            // Find binding entry for current biome (use Minimal by default)
            byte groundStyleToken = 0;
            if (bindings.MinimalBindings.IsCreated)
            {
                ref var entries = ref bindings.MinimalBindings.Value.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].BiomeId32 == currentBiomeId)
                    {
                        groundStyleToken = entries[i].GroundStyle;
                        break;
                    }
                }
            }

            // Update or create ground style request singleton
            if (!SystemAPI.HasSingleton<BiomeGroundStyleRequest>())
            {
                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var requestEntity = ecb.CreateEntity();
                ecb.AddComponent(requestEntity, new BiomeGroundStyleRequest
                {
                    GroundStyleToken = groundStyleToken,
                    IsDirty = 1
                });
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
            else
            {
                var request = SystemAPI.GetSingletonRW<BiomeGroundStyleRequest>();
                if (request.ValueRO.GroundStyleToken != groundStyleToken)
                {
                    request.ValueRW.GroundStyleToken = groundStyleToken;
                    request.ValueRW.IsDirty = 1;
                }
            }

            // Note: Actual material/style application happens in presentation systems
            // that read BiomeGroundStyleRequest and apply changes via ECB
        }
    }
}

