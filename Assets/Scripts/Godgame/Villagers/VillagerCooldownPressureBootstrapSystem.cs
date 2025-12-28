using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures cooldown pressure state exists for villagers with work cooldowns.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerCooldownPressureBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerWorkCooldown>()
                .WithNone<VillagerCooldownPressureState>()
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
                ecb.AddComponent(entity, new VillagerCooldownPressureState
                {
                    ActiveMask = VillagerCooldownPressureMask.None,
                    LastClearReason = VillagerCooldownClearReason.None,
                    LastClearTick = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
