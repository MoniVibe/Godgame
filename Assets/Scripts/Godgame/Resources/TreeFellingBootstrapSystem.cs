using Unity.Burst;
using Unity.Entities;

namespace Godgame.Resources
{
    /// <summary>
    /// Ensures tree felling tuning and event buffers exist in headless runs.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TreeFellingBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hasTuning = SystemAPI.HasSingleton<TreeFellingTuning>();
            var hasBuffer = SystemAPI.HasSingleton<TreeFallEventBuffer>();

            if (hasTuning && hasBuffer)
            {
                state.Enabled = false;
                return;
            }

            Entity entity;
            if (SystemAPI.TryGetSingletonEntity<TreeFellingTuning>(out var tuningEntity))
            {
                entity = tuningEntity;
            }
            else if (SystemAPI.TryGetSingletonEntity<TreeFallEventBuffer>(out var bufferEntity))
            {
                entity = bufferEntity;
            }
            else
            {
                entity = state.EntityManager.CreateEntity();
            }

            if (!state.EntityManager.HasComponent<TreeFellingTuning>(entity))
            {
                state.EntityManager.AddComponentData(entity, TreeFellingTuning.Default);
            }

            if (!state.EntityManager.HasComponent<TreeFallEventBuffer>(entity))
            {
                state.EntityManager.AddComponent<TreeFallEventBuffer>(entity);
            }

            if (!state.EntityManager.HasBuffer<TreeFallEvent>(entity))
            {
                state.EntityManager.AddBuffer<TreeFallEvent>(entity);
            }

            state.Enabled = false;
        }
    }
}
