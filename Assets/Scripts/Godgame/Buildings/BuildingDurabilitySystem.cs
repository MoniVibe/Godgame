using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Buildings
{
    /// <summary>
    /// System that manages building durability: natural decay, fire damage, and status updates.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BuildingDurabilitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BuildingDurability>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            float deltaTime = timeState.DeltaTime;
            uint currentTick = timeState.Tick;

            if (deltaTime <= 0)
                return;

            // Get config (or use defaults)
            var config = SystemAPI.HasSingleton<BuildingDurabilityConfig>()
                ? SystemAPI.GetSingleton<BuildingDurabilityConfig>()
                : BuildingDurabilityConfig.Default;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process natural decay and fire damage
            foreach (var (durability, qualityOpt, fireOpt, entity) in SystemAPI
                .Query<RefRW<BuildingDurability>, RefRO<BuildingQualityData>, RefRW<BuildingOnFire>>()
                .WithEntityAccess())
            {
                ProcessBuilding(ref durability.ValueRW, in qualityOpt.ValueRO, ref fireOpt.ValueRW, 
                    config, deltaTime, currentTick, entity, ref ecb);
            }

            // Process buildings without quality component
            foreach (var (durability, fireOpt, entity) in SystemAPI
                .Query<RefRW<BuildingDurability>, RefRW<BuildingOnFire>>()
                .WithEntityAccess()
                .WithNone<BuildingQualityData>())
            {
                var defaultQuality = new BuildingQualityData { Quality = BuildingQuality.Common };
                ProcessBuilding(ref durability.ValueRW, in defaultQuality, ref fireOpt.ValueRW, 
                    config, deltaTime, currentTick, entity, ref ecb);
            }

            // Process buildings without fire
            foreach (var (durability, qualityOpt, entity) in SystemAPI
                .Query<RefRW<BuildingDurability>, RefRO<BuildingQualityData>>()
                .WithEntityAccess()
                .WithNone<BuildingOnFire>())
            {
                ProcessNaturalDecay(ref durability.ValueRW, in qualityOpt.ValueRO, config, deltaTime, currentTick, entity, ref ecb);
            }

            // Process buildings without fire or quality
            foreach (var (durability, entity) in SystemAPI
                .Query<RefRW<BuildingDurability>>()
                .WithEntityAccess()
                .WithNone<BuildingOnFire, BuildingQualityData>())
            {
                var defaultQuality = new BuildingQualityData { Quality = BuildingQuality.Common };
                ProcessNaturalDecay(ref durability.ValueRW, in defaultQuality, config, deltaTime, currentTick, entity, ref ecb);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void ProcessBuilding(
            ref BuildingDurability durability,
            in BuildingQualityData quality,
            ref BuildingOnFire fire,
            BuildingDurabilityConfig config,
            float deltaTime,
            uint currentTick,
            Entity entity,
            ref EntityCommandBuffer ecb)
        {
            // Apply fire damage
            float fireDamage = fire.DamagePerSecond * deltaTime * config.FireDamageMultiplier;
            fire.AccumulatedDamage += fireDamage;
            durability.TakeDamage(fireDamage, DamageSource.Fire, currentTick);

            // Apply natural decay (reduced)
            float decayRate = config.NaturalDecayRatePerDay * quality.DecayMultiplier;
            float decayDamage = durability.Max * decayRate * (deltaTime / 86400f); // Convert to per-second
            durability.TakeDamage(decayDamage, DamageSource.Neglect, currentTick);

            // Update repair tag
            UpdateRepairTag(ref durability, config, entity, ref ecb);
        }

        private void ProcessNaturalDecay(
            ref BuildingDurability durability,
            in BuildingQualityData quality,
            BuildingDurabilityConfig config,
            float deltaTime,
            uint currentTick,
            Entity entity,
            ref EntityCommandBuffer ecb)
        {
            // Apply natural decay only
            float decayRate = config.NaturalDecayRatePerDay * quality.DecayMultiplier;
            float decayDamage = durability.Max * decayRate * (deltaTime / 86400f);

            if (decayDamage > 0.0001f)
            {
                durability.TakeDamage(decayDamage, DamageSource.Neglect, currentTick);
            }

            // Update repair tag
            UpdateRepairTag(ref durability, config, entity, ref ecb);
        }

        private void UpdateRepairTag(
            ref BuildingDurability durability,
            BuildingDurabilityConfig config,
            Entity entity,
            ref EntityCommandBuffer ecb)
        {
            bool needsRepair = durability.Percentage < config.RepairThreshold;

            // This would need additional tracking to avoid adding/removing every frame
            // For now, we just ensure status is updated
            durability.UpdateStatus();
        }
    }

    /// <summary>
    /// System that handles fire spread between adjacent buildings.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BuildingDurabilitySystem))]
    public partial struct BuildingFireSpreadSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BuildingOnFire>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Fire spread logic would use spatial queries to find adjacent buildings
            // For MVP, we just track fire damage accumulation
            // Full implementation would need PureDOTS spatial grid integration
        }
    }

    /// <summary>
    /// System that extinguishes fires from rain/weather events.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BuildingFireSpreadSystem))]
    public partial struct BuildingFireExtinguishSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BuildingOnFire>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Check for rain/weather state and extinguish fires
            // Would integrate with WeatherState singleton

            // For now, stub implementation
            // Real implementation would query weather and remove BuildingOnFire components
        }
    }
}

