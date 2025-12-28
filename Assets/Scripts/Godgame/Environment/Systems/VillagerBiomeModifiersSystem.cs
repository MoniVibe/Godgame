using Godgame.Environment;
using Godgame.Registry;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Applies biome-based modifiers to villagers (stamina, disease risk).
    /// Reads current biome from BiomeGrid and applies modifiers via VillagerBiomeModifiers component.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VillagerBiomeModifiersSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomeDefinitionSingleton>();
            state.RequireForUpdate<BiomeGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var biomeDefs = SystemAPI.GetSingleton<BiomeDefinitionSingleton>();
            if (!biomeDefs.Definitions.IsCreated)
            {
                return;
            }

            var biomeGrid = SystemAPI.GetSingleton<BiomeGrid>();
            if (!biomeGrid.BiomeIds.IsCreated || biomeGrid.Width == 0 || biomeGrid.Height == 0)
            {
                return;
            }

            // Get current biome ID (1×1 grid for MVP)
            uint currentBiomeId = biomeGrid.BiomeIds.Value[0];

            // Find biome profile
            ref var profiles = ref biomeDefs.Definitions.Value.Profiles;
            BiomeProfile? currentProfile = null;
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profiles[i].BiomeId32 == currentBiomeId)
                {
                    currentProfile = profiles[i];
                    break;
                }
            }

            if (!currentProfile.HasValue)
            {
                return; // Biome not found, skip modifiers
            }

            var profile = currentProfile.Value;

            // Apply modifiers to all villagers
            // For MVP, we apply global modifiers. Future: per-villager based on position
            foreach (var (modifiers, entity) in SystemAPI.Query<RefRW<VillagerBiomeModifiers>>()
                .WithEntityAccess())
            {
                modifiers.ValueRW.CurrentBiomeId32 = currentBiomeId;
                
                // Convert stamina drain percentage to multiplier (100 = 1.0, 150 = 1.5, 50 = 0.5)
                modifiers.ValueRW.StaminaMultiplier = profile.VillagerStaminaDrainPct / 100f;
                
                // Convert disease risk percentage to multiplier (0-100 -> 0.0-1.0)
                modifiers.ValueRW.DiseaseRiskMultiplier = profile.DiseaseRiskPct / 100f;
            }

            // Add modifiers component to villagers that don't have it yet
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<GodgameVillager>>()
                .WithNone<VillagerBiomeModifiers>()
                .WithEntityAccess())
            {
                ecb.AddComponent(entity, new VillagerBiomeModifiers
                {
                    CurrentBiomeId32 = currentBiomeId,
                    StaminaMultiplier = profile.VillagerStaminaDrainPct / 100f,
                    DiseaseRiskMultiplier = profile.DiseaseRiskPct / 100f
                });
            }
        }
    }
}

