using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villagers have ponder state for work evaluation pauses.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerPonderBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState>()
                .WithNone<VillagerPonderState>()
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
                ecb.AddComponent(entity, new VillagerPonderState
                {
                    RemainingSeconds = 0f,
                    AnchorPosition = Unity.Mathematics.float3.zero
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
