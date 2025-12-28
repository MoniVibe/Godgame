using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures VillagerPersonality and VillagerBehavior coexist with consistent sign conventions.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerPersonalityBehaviorSyncSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (personality, entity) in SystemAPI.Query<RefRO<VillagerPersonality>>()
                         .WithNone<VillagerBehavior>()
                         .WithEntityAccess())
            {
                var behavior = new VillagerBehavior
                {
                    VengefulScore = -personality.ValueRO.VengefulScore,
                    BoldScore = personality.ValueRO.BoldScore,
                    PatienceScore = personality.ValueRO.PatienceScore,
                    InitiativeModifier = 0f,
                    ActiveGrudgeCount = 0,
                    LastMajorActionTick = 0
                };
                behavior.RecalculateInitiative();
                ecb.AddComponent(entity, behavior);

                if (!state.EntityManager.HasComponent<VillagerCombatBehavior>(entity))
                {
                    ecb.AddComponent(entity, VillagerCombatBehavior.FromBehavior(in behavior));
                }
            }

            foreach (var (behavior, entity) in SystemAPI.Query<RefRO<VillagerBehavior>>()
                         .WithNone<VillagerPersonality>()
                         .WithEntityAccess())
            {
                var vengeful = math.clamp(-behavior.ValueRO.VengefulScore, -100f, 100f);
                var bold = math.clamp(behavior.ValueRO.BoldScore, -100f, 100f);
                var patience = math.clamp(behavior.ValueRO.PatienceScore, -100f, 100f);
                ecb.AddComponent(entity, new VillagerPersonality
                {
                    VengefulScore = (sbyte)math.round(vengeful),
                    BoldScore = (sbyte)math.round(bold),
                    PatienceScore = (sbyte)math.round(patience)
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
