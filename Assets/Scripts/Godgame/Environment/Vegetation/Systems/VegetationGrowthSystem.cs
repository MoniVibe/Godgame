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
    /// Updates plant growth stages based on climate conditions and age.
    /// Applies stress from drought/fire and enqueues reproduction requests.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct VegetationGrowthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlantSpecSingleton>();
            state.RequireForUpdate<ClimateState>();
            state.RequireForUpdate<MoistureGrid>();
            state.RequireForUpdate<BiomeGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var plantSpecs = SystemAPI.GetSingleton<PlantSpecSingleton>();
            if (!plantSpecs.Specs.IsCreated)
            {
                return;
            }

            var climate = SystemAPI.GetSingleton<ClimateState>();
            var moistureGrid = SystemAPI.GetSingleton<MoistureGrid>();
            var biomeGrid = SystemAPI.GetSingleton<BiomeGrid>();

            // Get current moisture (1×1 grid for MVP)
            float currentMoisture = moistureGrid.Values.IsCreated && moistureGrid.Width > 0 && moistureGrid.Height > 0
                ? moistureGrid.Values.Value[0]
                : climate.Moisture01;

            // Get current biome ID
            uint currentBiomeId = biomeGrid.BiomeIds.IsCreated && biomeGrid.Width > 0 && biomeGrid.Height > 0
                ? biomeGrid.BiomeIds.Value[0]
                : 1;

            var weatherState = SystemAPI.GetSingleton<WeatherState>();
            bool isDrought = weatherState.WeatherToken == 0 && currentMoisture < 0.2f; // Clear weather + low moisture
            bool isFire = weatherState.WeatherToken == 2; // Fire weather token (if implemented)

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var deltaTime = SystemAPI.Time.DeltaTime;

            // Update all plants
            foreach (var (plantState, plantStand, transform, entity) in SystemAPI.Query<RefRW<PlantState>, RefRO<PlantStand>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                var plantId = plantState.ValueRO.PlantId;
                var spec = FindPlantSpec(plantSpecs.Specs, plantId);
                if (!spec.HasValue)
                {
                    continue; // Plant spec not found
                }

                var specValue = spec.Value;

                // Check biome compatibility
                uint biomeBit = 1u << ((int)currentBiomeId - 1);
                if ((specValue.BiomeMask & biomeBit) == 0)
                {
                    // Plant not compatible with current biome - apply stress
                    plantState.ValueRW.StressLevel01 = math.min(1f, plantState.ValueRW.StressLevel01 + deltaTime * 0.1f);
                }

                // Check climate preferences
                bool inTempRange = climate.TempC >= specValue.TempMin && climate.TempC <= specValue.TempMax;
                bool inMoistureRange = currentMoisture >= specValue.MoistMin && currentMoisture <= specValue.MoistMax;

                // Apply stress from drought
                if (isDrought && (specValue.HazardFlags & PlantHazardFlags.Flammable) == 0)
                {
                    // Non-flammable plants suffer from drought
                    if (!inMoistureRange)
                    {
                        plantState.ValueRW.StressLevel01 = math.min(1f, plantState.ValueRW.StressLevel01 + deltaTime * 0.2f);
                    }
                }

                // Apply stress from fire
                if (isFire && (specValue.HazardFlags & PlantHazardFlags.Flammable) != 0)
                {
                    plantState.ValueRW.StressLevel01 = math.min(1f, plantState.ValueRW.StressLevel01 + deltaTime * 0.5f);
                }

                // Kill plant if stress too high
                if (plantState.ValueRO.StressLevel01 >= 1f)
                {
                    plantState.ValueRW.Stage = GrowthStage.Dead;
                    plantState.ValueRW.Health01 = 0f;
                    continue;
                }

                // Update health based on conditions
                if (inTempRange && inMoistureRange)
                {
                    // Optimal conditions - recover health
                    plantState.ValueRW.Health01 = math.min(1f, plantState.ValueRO.Health01 + deltaTime * 0.1f);
                }
                else
                {
                    // Suboptimal conditions - lose health
                    plantState.ValueRW.Health01 = math.max(0f, plantState.ValueRO.Health01 - deltaTime * 0.05f);
                }

                // Advance age and check stage transitions
                plantState.ValueRW.AgeSeconds += deltaTime;
                var currentStage = plantState.ValueRO.Stage;

                if (currentStage == GrowthStage.Dead || plantState.ValueRO.Health01 <= 0f)
                {
                    plantState.ValueRW.Stage = GrowthStage.Dead;
                    continue;
                }

                // Check stage transitions
                float totalAge = plantState.ValueRO.AgeSeconds;
                if (currentStage == GrowthStage.Seedling && totalAge >= specValue.StageSecSeedling)
                {
                    plantState.ValueRW.Stage = GrowthStage.Sapling;
                }
                else if (currentStage == GrowthStage.Sapling && totalAge >= (specValue.StageSecSeedling + specValue.StageSecSapling))
                {
                    plantState.ValueRW.Stage = GrowthStage.Mature;
                }

                // Reproduction for mature plants
                if (currentStage == GrowthStage.Mature && specValue.ReproSeedsPerDay > 0)
                {
                    // Check if in reproduction season (simplified: always reproduce for MVP)
                    // Future: check SeasonMask against current season
                    float reproChance = (specValue.ReproSeedsPerDay / 86400f) * deltaTime; // Convert per-day to per-second
                    if (Unity.Mathematics.Random.CreateFromIndex((uint)entity.Index).NextFloat() < reproChance)
                    {
                        // Enqueue reproduction command
                        var reproBuffer = SystemAPI.GetBuffer<ReproductionCommand>(entity);
                        reproBuffer.Add(new ReproductionCommand
                        {
                            PlantId = plantId,
                            Position = transform.ValueRO.Position,
                            Radius = specValue.ReproRadius,
                            SeedCount = 1 // Single seed per reproduction event
                        });
                    }
                }
            }
        }

        [BurstCompile]
        private static PlantSpec? FindPlantSpec(BlobAssetReference<PlantSpecBlob> specs, FixedString64Bytes plantId)
        {
            if (!specs.IsCreated)
            {
                return null;
            }

            ref var plants = ref specs.Value.Plants;
            for (int i = 0; i < plants.Length; i++)
            {
                if (plants[i].Id.Equals(plantId))
                {
                    return plants[i];
                }
            }

            return null;
        }
    }
}

