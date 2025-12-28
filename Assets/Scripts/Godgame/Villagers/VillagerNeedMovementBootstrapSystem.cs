using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villager need movement state exists for non-work wandering.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerNeedMovementBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState>()
                .WithNone<VillagerNeedMovementState>()
                .Build();
            state.RequireForUpdate(_missingQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = _missingQuery.ToEntityArray(state.WorldUpdateAllocator);
            if (entities.Length == 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var entity in entities)
            {
                ecb.AddComponent(entity, new VillagerNeedMovementState
                {
                    AnchorOffset = Unity.Mathematics.float3.zero,
                    LingerSeconds = 0f,
                    NextRepathTick = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
