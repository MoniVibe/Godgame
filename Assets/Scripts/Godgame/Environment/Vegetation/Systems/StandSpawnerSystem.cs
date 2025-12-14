using Godgame.Environment;
using Godgame.Environment.Vegetation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Environment.Vegetation.Systems
{
    /// <summary>
    /// Spawns vegetation stands based on StandSpec and current biome.
    /// Uses deterministic clustering and respects density/min-distance rules.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct StandSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StandSpecSingleton>();
            state.RequireForUpdate<PlantSpecSingleton>();
            state.RequireForUpdate<BiomeGrid>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var standSpecs = SystemAPI.GetSingleton<StandSpecSingleton>();
            var plantSpecs = SystemAPI.GetSingleton<PlantSpecSingleton>();
            var biomeGrid = SystemAPI.GetSingleton<BiomeGrid>();

            if (!standSpecs.Specs.IsCreated || !plantSpecs.Specs.IsCreated)
            {
                return;
            }

            // Get current biome ID
            uint currentBiomeId = biomeGrid.BiomeIds.IsCreated && biomeGrid.Width > 0 && biomeGrid.Height > 0
                ? biomeGrid.BiomeIds.Value[0]
                : 1;

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // For MVP: spawn stands based on biome weights
            // Future: use spatial queries to find valid spawn locations
            ref var stands = ref standSpecs.Specs.Value.Stands;
            ref var plants = ref plantSpecs.Specs.Value.Plants;
            for (int i = 0; i < stands.Length; i++)
            {
                ref var stand = ref stands[i];

                // Check spawn weight for current biome
                int biomeIndex = (int)currentBiomeId - 1;
                if (biomeIndex < 0 || biomeIndex >= stand.SpawnWeightsPerBiome.Length)
                {
                    continue; // Invalid biome index
                }

                float spawnWeight = stand.SpawnWeightsPerBiome[biomeIndex];
                if (spawnWeight <= 0f)
                {
                    continue; // No spawn for this biome
                }

                // Find plant spec
                var plantId = stand.PlantId;
                if (!TryFindPlantSpecIndex(plantSpecs.Specs, ref plantId, out var plantIndex))
                {
                    continue; // Plant spec not found
                }
                ref var plantSpec = ref plants[plantIndex];

                // Check biome compatibility
                uint biomeBit = 1u << biomeIndex;
                if ((plantSpec.BiomeMask & biomeBit) == 0)
                {
                    continue; // Plant not compatible with biome
                }

                // MVP: Simple spawn at origin (0,0,0) with clustering
                // Future: Use spatial queries and proper terrain sampling
                SpawnStand(ecb, ref stand, ref plantSpec, float3.zero, stand.SpawnRadius);
            }
        }

        private static void SpawnStand(
            EntityCommandBuffer ecb,
            ref StandSpec stand,
            ref PlantSpec plantSpec,
            float3 center,
            float radius)
        {
            // Calculate number of plants based on density and area
            float area = math.PI * radius * radius;
            int plantCount = (int)math.ceil(stand.Density * area);

            var random = Unity.Mathematics.Random.CreateFromIndex((uint)stand.Id.GetHashCode());

            for (int i = 0; i < plantCount; i++)
            {
                // Calculate position with clustering
                float angle = random.NextFloat(0f, 2f * math.PI);
                float distance = random.NextFloat(0f, radius);

                // Apply clustering factor
                if (stand.Clustering > 0f)
                {
                    // Clustered: bias toward center
                    distance = math.lerp(distance, distance * stand.Clustering, stand.Clustering);
                }

                float3 position = center + new float3(
                    math.cos(angle) * distance,
                    0f,
                    math.sin(angle) * distance);

                // Check min distance (simplified for MVP - would need spatial queries)
                // For now, just spawn

                // Create plant entity
                var plantEntity = ecb.CreateEntity();
                ecb.AddComponent(plantEntity, new PlantState
                {
                    PlantId = stand.PlantId,
                    Stage = GrowthStage.Seedling,
                    AgeSeconds = 0f,
                    Health01 = 1f,
                    StressLevel01 = 0f
                });

                ecb.AddComponent(plantEntity, new PlantStand
                {
                    StandId = stand.Id,
                    StandCenter = Entity.Null,
                    DistanceFromCenter = distance
                });

                ecb.AddComponent(plantEntity, LocalTransform.FromPosition(position));

                // Add reproduction command buffer
                ecb.AddBuffer<ReproductionCommand>(plantEntity);
            }
        }

        private static bool TryFindPlantSpecIndex(BlobAssetReference<PlantSpecBlob> specs, ref FixedString64Bytes plantId, out int index)
        {
            index = -1;
            if (!specs.IsCreated)
            {
                return false;
            }

            ref var plants = ref specs.Value.Plants;
            for (int i = 0; i < plants.Length; i++)
            {
                if (plants[i].Id.Equals(plantId))
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }
    }
}
