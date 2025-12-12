#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Demo;
using PureDOTS.Environment;
using PureDOTS.Runtime.Camera;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that creates and updates biome overlay chunks for far LOD visualization.
    /// Spawns decal entities or simple mesh quads with biome-colored materials.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_BiomeGroundVisualizationSystem))]
    public partial struct Godgame_BiomeOverlaySystem : ISystem
    {
        private EntityQuery _overlayChunkQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomePresentationData>();
            state.RequireForUpdate<PresentationConfig>();
            _overlayChunkQuery = state.GetEntityQuery(typeof(BiomeOverlayChunk));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<PresentationConfig>();
            // TODO: Get camera position from proper ECS camera component or MonoBehaviour
            // For now, use a placeholder position
            Unity.Mathematics.float3 cameraPosition = Unity.Mathematics.float3.zero;
#if GODGAME_LEGACY_DEBUG
            // Legacy debug: Try to get from CameraRigState if available
            // var cameraState = SystemAPI.GetSingleton<CameraRigState>();
            // var cameraPosition = cameraState.Position;
#endif

            // For far LOD (beyond LOD2Distance), create overlay chunks
            // This is a simplified version - full implementation would:
            // 1. Partition world into chunks (e.g., 50x50 unit chunks)
            // 2. Sample dominant biome and average moisture for each chunk
            // 3. Spawn/update overlay decal entities for chunks within view distance

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Sample ground tiles to create overlay chunks
            // For now, create one overlay chunk per region of ground tiles
            // In a full implementation, this would be more sophisticated (spatial partitioning)

            // Placeholder: This system would spawn overlay entities based on camera distance
            // Actual implementation requires:
            // - Spatial partitioning of ground tiles into chunks
            // - Dominant biome calculation per chunk
            // - Decal/mesh entity spawning with biome-colored materials

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Helper to calculate dominant biome and average moisture for a region.
    /// </summary>
    [BurstCompile]
    public static class BiomeOverlayHelpers
    {
        public static BiomeType CalculateDominantBiome(NativeArray<BiomeType> biomes)
        {
            if (biomes.Length == 0)
            {
                return BiomeType.Unknown;
            }

            // Count biome occurrences (use int key to avoid IEquatable requirement)
            var counts = new NativeHashMap<int, int>(8, Allocator.Temp);
            for (int i = 0; i < biomes.Length; i++)
            {
                var biome = biomes[i];
                if (biome != BiomeType.Unknown)
                {
                    int biomeKey = (int)biome;
                    if (counts.TryGetValue(biomeKey, out int count))
                    {
                        counts[biomeKey] = count + 1;
                    }
                    else
                    {
                        counts[biomeKey] = 1;
                    }
                }
            }

            // Find dominant
            BiomeType dominant = BiomeType.Unknown;
            int maxCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    dominant = (BiomeType)kvp.Key;
                }
            }

            counts.Dispose();
            return dominant;
        }

        public static float CalculateAverageMoisture(NativeArray<float> moistureValues)
        {
            if (moistureValues.Length == 0)
            {
                return 50f; // Default
            }

            float sum = 0f;
            for (int i = 0; i < moistureValues.Length; i++)
            {
                sum += moistureValues[i];
            }

            return sum / moistureValues.Length;
        }
    }
}
#endif
