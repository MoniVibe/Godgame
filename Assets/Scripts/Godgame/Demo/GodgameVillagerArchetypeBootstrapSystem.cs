using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Villagers;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// Ensures every villager has a VillagerArchetypeResolved component so PureDOTS AI systems can evaluate goals.
    /// The authoring data shipped with the demo predates this requirement, so we attach a fallback record at runtime.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(AISystemGroup), OrderFirst = true)]
    public partial struct GodgameVillagerArchetypeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<VillagerAIState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            VillagerArchetypeDefaults.CreateFallback(out var fallback);

            foreach (var (ai, entity) in SystemAPI.Query<RefRO<VillagerAIState>>()
                         .WithNone<VillagerArchetypeResolved>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new VillagerArchetypeResolved
                {
                    ArchetypeIndex = 0,
                    Data = fallback
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}