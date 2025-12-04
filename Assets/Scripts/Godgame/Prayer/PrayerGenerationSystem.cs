using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Prayer
{
    /// <summary>
    /// System that sums all prayer generators and updates the global prayer pool.
    /// Runs every frame to maintain smooth prayer accumulation.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PrayerGenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Get or create prayer pool singleton
            if (!SystemAPI.HasSingleton<PrayerPowerPool>())
            {
                CreatePrayerPoolSingleton(ref state);
                return;
            }

            // Get time delta
            var timeState = SystemAPI.GetSingleton<TimeState>();
            float deltaTime = timeState.DeltaTime;

            if (deltaTime <= 0)
                return;

            // Get config (or use defaults)
            var config = SystemAPI.HasSingleton<PrayerSystemConfig>()
                ? SystemAPI.GetSingleton<PrayerSystemConfig>()
                : PrayerSystemConfig.Default;

            // Sum all generators
            float totalGeneration = 0f;
            int activeGenerators = 0;

            foreach (var generator in SystemAPI.Query<RefRO<PrayerGenerator>>())
            {
                float rate = generator.ValueRO.EffectiveRate;
                rate = math.clamp(rate, config.MinPrayerPerVillager, config.MaxPrayerPerVillager);
                totalGeneration += rate;

                if (generator.ValueRO.IsActive)
                    activeGenerators++;
            }

            // Apply global multiplier
            totalGeneration *= config.GlobalMultiplier;

            // Update the prayer pool
            ref var pool = ref SystemAPI.GetSingletonRW<PrayerPowerPool>().ValueRW;

            // Store computed generation rate
            pool.GenerationRate = totalGeneration;

            // Add prayer (respecting cap if enabled)
            float prayerToAdd = totalGeneration * deltaTime;

            if (config.EnforceCap)
            {
                pool.Add(prayerToAdd);
            }
            else
            {
                pool.CurrentPrayer += prayerToAdd;
                pool.TotalGenerated += prayerToAdd;
            }
        }

        private void CreatePrayerPoolSingleton(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, PrayerPowerPool.Default);
            state.EntityManager.AddComponentData(entity, new PrayerPoolTag());

            // Also create default config if not exists
            if (!SystemAPI.HasSingleton<PrayerSystemConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, PrayerSystemConfig.Default);
            }
        }
    }

    /// <summary>
    /// System that handles prayer consumption for miracles.
    /// Processes consumption requests and deducts from the pool.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PrayerGenerationSystem))]
    public partial struct PrayerConsumptionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrayerPowerPool>();
            state.RequireForUpdate<PrayerConsumptionRequest>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var pool = ref SystemAPI.GetSingletonRW<PrayerPowerPool>().ValueRW;

            // Process all consumption requests
            foreach (var (request, entity) in SystemAPI.Query<RefRO<PrayerConsumptionRequest>>().WithEntityAccess())
            {
                if (request.ValueRO.Amount > 0 && pool.TryConsume(request.ValueRO.Amount))
                {
                    // Mark request as fulfilled
                    state.EntityManager.SetComponentData(entity, new PrayerConsumptionRequest
                    {
                        Amount = request.ValueRO.Amount,
                        Fulfilled = true,
                        RequestTick = request.ValueRO.RequestTick
                    });
                }
            }

            // Clean up fulfilled requests (or let another system handle it)
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (request, entity) in SystemAPI.Query<RefRO<PrayerConsumptionRequest>>().WithEntityAccess())
            {
                if (request.ValueRO.Fulfilled)
                {
                    ecb.RemoveComponent<PrayerConsumptionRequest>(entity);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Request component to consume prayer power.
    /// Add to an entity to request consumption, system will process and mark as fulfilled.
    /// </summary>
    public struct PrayerConsumptionRequest : IComponentData
    {
        public float Amount;
        public bool Fulfilled;
        public uint RequestTick;
    }
}

