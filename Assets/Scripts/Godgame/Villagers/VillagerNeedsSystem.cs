using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Manages villager needs decay over time.
    /// Decreases food, rest, sleep, and general health based on time and activity.
    /// Future systems can hook into this to replenish needs (eating, sleeping, etc.).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerStatCalculationSystem))]
    public partial struct VillagerNeedsSystem : ISystem
    {
        private const float FoodDecayPerSecond = 0.1f; // 0.1 per second = 6 per minute = 360 per hour
        private const float RestDecayPerSecond = 0.05f; // 0.05 per second = 3 per minute = 180 per hour
        private const float SleepDecayPerSecond = 0.08f; // 0.08 per second = 4.8 per minute = 288 per hour
        private const float HealthDecayWhenStarving = 0.02f; // Health decays when food = 0
        private const float HealthDecayWhenExhausted = 0.01f; // Health decays when rest = 0

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate(SystemAPI.QueryBuilder()
                .WithAll<VillagerNeeds>()
                .Build());
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var timeState = SystemAPI.GetSingleton<TimeState>();
            
            // Skip if paused
            if (timeState.IsPaused)
                return;

            // Apply time scale
            var scaledDeltaTime = deltaTime * timeState.CurrentSpeedMultiplier;

            foreach (var needsRef in SystemAPI.Query<RefRW<VillagerNeeds>>())
            {
                var needs = needsRef.ValueRO;

                // Decay food
                needs.Food = (byte)math.max(0, needs.Food - (int)(FoodDecayPerSecond * scaledDeltaTime * 100f));

                // Decay rest (faster when working, slower when idle - simplified for now)
                needs.Rest = (byte)math.max(0, needs.Rest - (int)(RestDecayPerSecond * scaledDeltaTime * 100f));

                // Decay sleep
                needs.Sleep = (byte)math.max(0, needs.Sleep - (int)(SleepDecayPerSecond * scaledDeltaTime * 100f));

                // Decay general health when needs are critical
                if (needs.Food == 0)
                {
                    needs.GeneralHealth = (byte)math.max(0, needs.GeneralHealth - (int)(HealthDecayWhenStarving * scaledDeltaTime * 100f));
                }
                if (needs.Rest == 0)
                {
                    needs.GeneralHealth = (byte)math.max(0, needs.GeneralHealth - (int)(HealthDecayWhenExhausted * scaledDeltaTime * 100f));
                }

                // Sync Health and Energy fields for PureDOTS compatibility
                needs.Health = needs.GeneralHealth;
                needs.Energy = needs.Rest;

                needsRef.ValueRW = needs;
            }
        }
    }
}


