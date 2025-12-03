using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Integrates Rain and Fire miracles with biome moisture system.
    /// Rain increases moisture, Fire reduces it.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct MiracleBiomeIntegrationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClimateState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // For MVP, we use a simple command buffer approach
            // Future: Listen to miracle event buffers for Rain/Fire casts
            
            // This system is a placeholder that can be extended when miracle systems are implemented
            // For now, it ensures ClimateState exists and can be modified
            
            // Example integration points (to be implemented when miracles are wired):
            // - Rain miracle casts → increase ClimateState.Moisture01 by 0.3
            // - Fire miracle casts → decrease ClimateState.Moisture01 by 0.2
            // - Both effects decay over time (moisture returns to baseline)
        }
    }
}

