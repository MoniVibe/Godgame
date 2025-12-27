using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures directive state and bias exist for villagers.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerDirectiveBootstrapSystem : ISystem
    {
        private EntityQuery _missingStateQuery;
        private EntityQuery _missingBiasQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingStateQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState>()
                .WithNone<VillagerDirectiveState>()
                .Build();
            _missingBiasQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState>()
                .WithNone<VillagerDirectiveBias>()
                .Build();

            state.RequireForUpdate<VillagerGoalState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var stateEntities = _missingStateQuery.ToEntityArray(state.WorldUpdateAllocator);
            for (int i = 0; i < stateEntities.Length; i++)
            {
                ecb.AddComponent(stateEntities[i], new VillagerDirectiveState
                {
                    Current = VillagerDirective.None,
                    Previous = VillagerDirective.None,
                    LastChangeTick = 0,
                    NextDecisionTick = 0
                });
            }

            var biasEntities = _missingBiasQuery.ToEntityArray(state.WorldUpdateAllocator);
            for (int i = 0; i < biasEntities.Length; i++)
            {
                ecb.AddComponent(biasEntities[i], VillagerDirectiveBias.Default);
            }

            if (stateEntities.Length > 0 || biasEntities.Length > 0)
            {
                ecb.Playback(state.EntityManager);
            }

            ecb.Dispose();
        }
    }
}
