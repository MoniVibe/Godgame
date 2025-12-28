using PureDOTS.Runtime.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villagers have leisure cadence state.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerLeisureBootstrapSystem : ISystem
    {
        private EntityQuery _missingQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState>()
                .WithNone<VillagerLeisureState>()
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
                ecb.AddComponent(entity, new VillagerLeisureState
                {
                    CooldownStartTick = 0,
                    CadenceTicks = 0,
                    EpisodeIndex = 0,
                    RerollCount = 0,
                    Action = VillagerLeisureAction.None,
                    ActionTarget = Entity.Null
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
