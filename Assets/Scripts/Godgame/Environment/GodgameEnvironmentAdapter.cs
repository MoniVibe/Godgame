using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Environment;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Environment
{
    /// <summary>
    /// Adapter system that maps PureDOTS environment to Godgame-specific visuals and mechanics.
    /// Reads shared environment state and applies it to Godgame systems.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameEnvironmentAdapter : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();

            // Skip if paused or rewinding
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            // TODO: Implement Godgame-specific environment mapping
            // Example: Read ClimateState and apply to GodgameBiomeSystem
            // Example: Read MoistureGridState and apply visual effects (grass color)
            // Example: Read WindState and apply to tree sway animations
            // Example: Read SunlightState and apply to lighting system
        }
    }
}

