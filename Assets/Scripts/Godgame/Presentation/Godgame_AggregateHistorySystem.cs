using Godgame.Economy;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that periodically samples key aggregates and stores them in rolling buffers.
    /// Samples villages (population, wealth, food) and regions (vegetation health, fertility).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_BiomeDataHookSystem))]
    public partial struct Godgame_AggregateHistorySystem : ISystem
    {
        private uint _lastSampleTick;

        public void OnCreate(ref SystemState state)
        {
            // Create config singleton if not exists
            var query = state.GetEntityQuery(typeof(AggregateHistoryConfig));
            if (query.IsEmpty)
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, new AggregateHistoryConfig
                {
                    SampleIntervalTicks = 30, // 1 second at 30 TPS
                    MaxSamples = 60 // ~1 minute of history
                });
                state.EntityManager.SetName(configEntity, "AggregateHistoryConfig");
            }

            _lastSampleTick = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                return;
            }

            var config = SystemAPI.GetSingleton<AggregateHistoryConfig>();

            // Check if it's time to sample
            if (timeState.Tick - _lastSampleTick < config.SampleIntervalTicks)
            {
                return;
            }

            _lastSampleTick = timeState.Tick;

            // Sample villages
            SampleVillageHistory(ref state, timeState.Tick, config.MaxSamples);

            // Sample regions
            SampleRegionHistory(ref state, timeState.Tick, config.MaxSamples);
        }

        [BurstCompile]
        private void SampleVillageHistory(ref SystemState state, uint currentTick, int maxSamples)
        {
            foreach (var (village, resources, historyBuffer, entity) in SystemAPI.Query<RefRO<Village>, DynamicBuffer<VillageResource>, DynamicBuffer<AggregateHistory>>().WithEntityAccess())
            {
                // Calculate aggregate stats
                int population = (int)village.ValueRO.MemberCount;
                int wealth = 0;
                int food = 0;

                for (int i = 0; i < resources.Length; i++)
                {
                    var resource = resources[i];
                    wealth += resource.Quantity; // Simple wealth = total quantity

                    // Check if this is food (ResourceType >= 40 is agricultural)
                    if (resource.ResourceTypeIndex >= 40 && resource.ResourceTypeIndex <= 44)
                    {
                        food += resource.Quantity;
                    }
                }

                // Add sample to buffer
                var sample = new AggregateHistory
                {
                    Tick = currentTick,
                    Population = population,
                    Wealth = wealth,
                    Food = food,
                    VegetationHealth = 0f, // Not applicable for villages
                    Fertility = 0f // Not applicable for villages
                };

                historyBuffer.Add(sample);

                // Remove oldest samples if buffer exceeds max
                while (historyBuffer.Length > maxSamples)
                {
                    historyBuffer.RemoveAt(0);
                }
            }
        }

        [BurstCompile]
        private void SampleRegionHistory(ref SystemState state, uint currentTick, int maxSamples)
        {
            // Sample regions (ground tiles with biome data)
            foreach (var (biomeData, historyBuffer, entity) in SystemAPI.Query<RefRO<BiomePresentationData>, DynamicBuffer<AggregateHistory>>().WithEntityAccess())
            {
                // Calculate aggregate stats
                float fertility = biomeData.ValueRO.Fertility;
                float vegetationHealth = 1f; // Placeholder - would read from PlantStand components

                // Add sample to buffer
                var sample = new AggregateHistory
                {
                    Tick = currentTick,
                    Population = 0, // Not applicable for regions
                    Wealth = 0, // Not applicable for regions
                    Food = 0, // Not applicable for regions
                    VegetationHealth = vegetationHealth,
                    Fertility = fertility
                };

                historyBuffer.Add(sample);

                // Remove oldest samples if buffer exceeds max
                while (historyBuffer.Length > maxSamples)
                {
                    historyBuffer.RemoveAt(0);
                }
            }
        }
    }
}

