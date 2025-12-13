using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Motivation;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Motivation
{
    /// <summary>
    /// Stub system that reads MotivationIntent and translates into concrete actions.
    /// Example: Boosts crafting job priority when intent is "craft rare item" (SpecId=1001).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameVillagerGoalExecutionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<MotivationConfigState>();
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

            // TODO: Implement goal execution logic
            // Example: Read MotivationIntent for entities
            // If ActiveSpecId == 1001 (craft rare item), boost crafting job priority
            // Decode SpecId via MotivationCatalog if needed
            // Translate intent into job assignments, task creation, or behavior modifiers
        }
    }
}


















