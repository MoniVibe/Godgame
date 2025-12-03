using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Initializes villager stats from authoring data.
    /// Ensures all stats have valid default values and converts template floats to runtime byte ranges.
    /// Runs once after entities are created.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerStatInitializationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .Build());
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system ensures stats are properly initialized.
            // Most initialization happens in the baker, but this can handle runtime-spawned entities
            // or fix any invalid values.
            
            // Ensure combat stats have valid health if MaxHealth is 0
            foreach (var combatStatsRef in SystemAPI.Query<RefRW<VillagerCombatStats>>())
            {
                var combatStats = combatStatsRef.ValueRO;
                if (combatStats.MaxHealth <= 0f)
                {
                    combatStats.MaxHealth = 100f;
                    combatStats.CurrentHealth = 100f;
                    combatStatsRef.ValueRW = combatStats;
                }
            }

            // Ensure needs have valid values
            foreach (var needsRef in SystemAPI.Query<RefRW<VillagerNeeds>>())
            {
                var needs = needsRef.ValueRO;
                if (needs.MaxHealth <= 0f)
                {
                    needs.MaxHealth = 100f;
                    needs.Health = math.clamp(needs.Health, 0f, needs.MaxHealth);
                    needsRef.ValueRW = needs;
                }
            }
        }
    }
}


