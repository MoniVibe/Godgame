using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures crowding state exists for movement-aware villagers.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerCrowdingBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerNeedMovementState>()
                .WithNone<VillagerCrowdingState>()
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
                ecb.AddComponent(entity, new VillagerCrowdingState
                {
                    Pressure = 0f,
                    LastSampleTick = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
