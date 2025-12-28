using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villagers have a job assignment component for shared ticket flow.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerJobAssignmentBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState>()
                .Build();
            state.RequireForUpdate(_missingQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var entity in _missingQuery.ToEntityArray(state.WorldUpdateAllocator))
            {
                if (!state.EntityManager.HasComponent<JobAssignment>(entity))
                {
                    ecb.AddComponent(entity, new JobAssignment
                    {
                        Ticket = Entity.Null,
                        CommitTick = 0
                    });
                }

                if (!state.EntityManager.HasBuffer<JobBatchEntry>(entity))
                {
                    ecb.AddBuffer<JobBatchEntry>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
