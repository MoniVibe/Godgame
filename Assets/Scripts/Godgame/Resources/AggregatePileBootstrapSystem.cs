using Unity.Burst;
using Unity.Entities;

namespace Godgame.Resources
{
    /// <summary>
    /// Ensures aggregate pile config and runtime singletons exist in headless runs.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct AggregatePileBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<AggregatePileConfig>() && SystemAPI.HasSingleton<AggregatePileRuntimeState>())
            {
                state.Enabled = false;
                return;
            }

            Entity entity;
            if (SystemAPI.TryGetSingletonEntity<AggregatePileConfig>(out var configEntity))
            {
                entity = configEntity;
            }
            else if (SystemAPI.TryGetSingletonEntity<AggregatePileRuntimeState>(out var runtimeEntity))
            {
                entity = runtimeEntity;
            }
            else
            {
                entity = state.EntityManager.CreateEntity();
            }

            if (!state.EntityManager.HasComponent<AggregatePileConfig>(entity))
            {
                state.EntityManager.AddComponentData(entity, AggregatePileConfig.Default);
            }

            if (!state.EntityManager.HasComponent<AggregatePileRuntimeState>(entity))
            {
                state.EntityManager.AddComponentData(entity, new AggregatePileRuntimeState
                {
                    NextMergeTime = 0f,
                    ActivePiles = 0
                });
            }

            state.Enabled = false;
        }
    }
}
