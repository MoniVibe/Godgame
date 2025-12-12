#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Demo;
using Godgame.Villages;
using Godgame.Villagers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that updates player-facing HUD data from simulation.
    /// Reads Village entities, VillagerBehavior entities, and calculates world health.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillageAggregateStateSystem))]
    public partial struct Godgame_PlayerHUDSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Create HUD singleton if not exists
            var query = state.GetEntityQuery(typeof(PlayerHUDSingleton));
            if (query.IsEmpty)
            {
                var hudEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<PlayerHUDSingleton>(hudEntity);
                state.EntityManager.AddComponentData(hudEntity, new PlayerHUDData());
                state.EntityManager.SetName(hudEntity, "PlayerHUDSingleton");
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get scenario name from DemoConfig
            FixedString64Bytes scenarioName = "Demo_01";
            // TODO: Read from DemoConfigBlobReference when available
            // For now, use default name

            // Count villages
            int villageCount = SystemAPI.QueryBuilder().WithAll<Village>().Build().CalculateEntityCount();

            // Count villagers
            int villagerCount = SystemAPI.QueryBuilder().WithAll<VillagerBehavior>().Build().CalculateEntityCount();

            // Calculate world health from village aggregate states
            float worldHealth = CalculateWorldHealth(ref state);

            // Update HUD data
            foreach (var hudDataRef in SystemAPI.Query<RefRW<PlayerHUDData>>())
            {
                ref var hudData = ref hudDataRef.ValueRW;
                hudData.ScenarioName = scenarioName;
                hudData.VillageCount = villageCount;
                hudData.VillagerCount = villagerCount;
                hudData.WorldHealth = worldHealth;
            }
        }

        [BurstCompile]
        private float CalculateWorldHealth(ref SystemState state)
        {
            int totalVillages = 0;
            float healthSum = 0f;

            foreach (var visualState in SystemAPI.Query<RefRO<VillageCenterVisualState>>().WithAll<Village>())
            {
                totalVillages++;
                // Map aggregate state to health value (0-1)
                float villageHealth = visualState.ValueRO.AggregateState switch
                {
                    VillageAggregateState.Prosperous => 1f,
                    VillageAggregateState.Normal => 0.7f,
                    VillageAggregateState.UnderMiracle => 0.8f,
                    VillageAggregateState.Starving => 0.3f,
                    VillageAggregateState.Crisis => 0.1f,
                    _ => 0.5f
                };
                healthSum += villageHealth;
            }

            return totalVillages > 0 ? healthSum / totalVillages : 0.5f;
        }
    }
}
#endif
