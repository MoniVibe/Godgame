using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Updates global climate state with simple deterministic oscillation.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct ClimateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClimateState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var climate = ref SystemAPI.GetSingletonRW<ClimateState>().ValueRW;
            var time = (float)SystemAPI.Time.ElapsedTime;

            // Simple daily oscillation: temperature cycles between 12°C and 22°C
            // Using sin wave for smooth deterministic oscillation
            climate.TempC = math.lerp(12f, 22f, 0.5f + 0.5f * math.sin(time * 0.1f));

            // Moisture stays constant for now (will be modified by weather/miracles)
            // Default is already set in bootstrap, but we preserve any modifications
            climate.Moisture01 = math.clamp(climate.Moisture01, 0f, 1f);

            // Elevation mean stays constant (can be modified by terrain systems later)
            // Default is 0 (sea level)
        }
    }
}

