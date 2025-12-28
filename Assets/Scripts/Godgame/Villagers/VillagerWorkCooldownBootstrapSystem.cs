using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villagers have work cooldown state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerWorkCooldownBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState>()
                .WithNone<VillagerWorkCooldown>()
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
                ecb.AddComponent(entity, new VillagerWorkCooldown
                {
                    StartTick = 0,
                    EndTick = 0,
                    Mode = VillagerWorkCooldownMode.None
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
