using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Space;
using PureDOTS.Systems.Space;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Features.Planets
{
    /// <summary>
    /// Godgame-specific planet system.
    /// Handles Rimworld-like world generation and planet logic.
    /// This is a thin adapter layer - all core logic is in PureDOTS shared systems.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    // Removed invalid UpdateAfter attributes: PlanetAppealSystem and SpeciesPreferenceMatchingSystem run in SpaceSystemGroup, so cross-group ordering is not supported.
    public partial struct GodgamePlanetSystem : ISystem
    {
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

            // Godgame-specific planet logic:
            // - World generation (procedural planet creation - future)
            // - Villager migration based on planet appeal (future)
            // - Planet-specific miracle effects (future)
            // - World map visualization (future)

            // For now, this is a stub that can be extended with Godgame-specific logic
            foreach (var (planetAppeal, compatibility) in SystemAPI.Query<
                RefRO<PlanetAppeal>,
                DynamicBuffer<PlanetCompatibility>>())
            {
                // Godgame can read appeal and compatibility scores here
                // and apply game-specific logic (e.g., villager migration, world generation)
            }
        }
    }
}

