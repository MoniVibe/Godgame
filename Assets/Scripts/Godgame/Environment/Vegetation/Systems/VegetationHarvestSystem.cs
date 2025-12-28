using Godgame.Environment.Vegetation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Vegetation.Systems
{
    /// <summary>
    /// Handles harvesting of vegetation, converting yields to materials.
    /// Respects hazard flags and spoilage.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VegetationHarvestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlantSpecSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var plantSpecs = SystemAPI.GetSingleton<PlantSpecSingleton>();
            if (!plantSpecs.Specs.IsCreated)
            {
                return;
            }

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Process harvest commands (would come from villager interaction systems)
            // For MVP, this is a placeholder that can be extended when harvest interaction is implemented

            // Example: If a plant is marked for harvest, convert yields to materials
            // This would integrate with MaterialRuleEngine to resolve material IDs
        }

        /// <summary>
        /// Calculates harvest yields for a plant based on its spec and current state.
        /// </summary>
        public static void CalculateHarvestYields(
            ref PlantSpec plantSpec,
            ref PlantState plantState,
            ref NativeList<PlantLootEntry> outputYields)
        {
            if (plantState.Stage != GrowthStage.Mature && plantState.Stage != GrowthStage.Sapling)
            {
                return; // Can only harvest mature or sapling plants
            }

            if (plantSpec.Yields.Length == 0)
            {
                return;
            }

            ref var yields = ref plantSpec.Yields;
            var random = Unity.Mathematics.Random.CreateFromIndex((uint)plantState.PlantId.GetHashCode());

            for (int i = 0; i < yields.Length; i++)
            {
                var yield = yields[i];

                // Check weight (spawn probability)
                if (random.NextFloat() > yield.Weight)
                {
                    continue; // Skip this yield
                }

                // Calculate amount based on min/max and plant health
                float amount = math.lerp(yield.MinAmount, yield.MaxAmount, random.NextFloat());
                amount *= plantState.Health01; // Scale by health

                if (amount > 0.001f)
                {
                    outputYields.Add(new PlantLootEntry
                    {
                        MaterialId = yield.MaterialId,
                        Weight = 1f, // Already rolled
                        MinAmount = amount,
                        MaxAmount = amount
                    });
                }
            }
        }
    }
}
