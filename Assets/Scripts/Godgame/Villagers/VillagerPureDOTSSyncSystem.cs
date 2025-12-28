using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Syncs Godgame stat components to PureDOTS components for registry compatibility.
    /// Keeps PureDOTS VillagerNeeds, VillagerMood, and VillagerCombatStats in sync with Godgame components.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerNeedsSystem))]
    [UpdateAfter(typeof(VillagerStatCalculationSystem))]
    [UpdateBefore(typeof(Registry.GodgameVillagerSyncSystem))]
    public partial struct VillagerPureDOTSSyncSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder()
                .WithAll<VillagerNeeds>()
                .Build());
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Sync Godgame VillagerNeeds to PureDOTS VillagerNeeds
            foreach (var (godgameNeeds, entity) in SystemAPI.Query<RefRO<VillagerNeeds>>().WithEntityAccess())
            {
                var needs = godgameNeeds.ValueRO;
                
                // Try to get PureDOTS component, create if missing
                if (!SystemAPI.HasComponent<PureDOTS.Runtime.Components.VillagerNeeds>(entity))
                {
                    ecb.AddComponent(entity, new PureDOTS.Runtime.Components.VillagerNeeds
                    {
                        Health = needs.Health,
                        MaxHealth = needs.MaxHealth,
                        Energy = needs.Energy
                    });
                }
                else
                {
                    var puredotsNeeds = SystemAPI.GetComponentRW<PureDOTS.Runtime.Components.VillagerNeeds>(entity);
                    puredotsNeeds.ValueRW.Health = needs.Health;
                    puredotsNeeds.ValueRW.MaxHealth = needs.MaxHealth;
                    puredotsNeeds.ValueRW.Energy = needs.Energy;
                }
            }

            // Sync Godgame VillagerMood to PureDOTS VillagerMood
            foreach (var (godgameMood, entity) in SystemAPI.Query<RefRO<VillagerMood>>().WithEntityAccess())
            {
                var mood = godgameMood.ValueRO;
                
                // Try to get PureDOTS component, create if missing
                if (!SystemAPI.HasComponent<PureDOTS.Runtime.Components.VillagerMood>(entity))
                {
                    ecb.AddComponent(entity, new PureDOTS.Runtime.Components.VillagerMood
                    {
                        Mood = mood.Mood
                    });
                }
                else
                {
                    var puredotsMood = SystemAPI.GetComponentRW<PureDOTS.Runtime.Components.VillagerMood>(entity);
                    puredotsMood.ValueRW.Mood = mood.Mood;
                }
            }

            // Sync Godgame VillagerCombatStats to PureDOTS VillagerCombatStats
            foreach (var (godgameCombat, entity) in SystemAPI.Query<RefRO<VillagerCombatStats>>().WithEntityAccess())
            {
                var combat = godgameCombat.ValueRO;
                
                // Try to get PureDOTS component, create if missing
                if (!SystemAPI.HasComponent<PureDOTS.Runtime.Components.VillagerCombatStats>(entity))
                {
                    ecb.AddComponent(entity, new PureDOTS.Runtime.Components.VillagerCombatStats
                    {
                        AttackDamage = combat.AttackDamage,
                        AttackSpeed = combat.AttackSpeed,
                        CurrentTarget = combat.CurrentTarget
                    });
                }
                else
                {
                    var puredotsCombat = SystemAPI.GetComponentRW<PureDOTS.Runtime.Components.VillagerCombatStats>(entity);
                    puredotsCombat.ValueRW.AttackDamage = combat.AttackDamage;
                    puredotsCombat.ValueRW.AttackSpeed = combat.AttackSpeed;
                    puredotsCombat.ValueRW.CurrentTarget = combat.CurrentTarget;
                }
            }
        }
    }
}

