using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures chatter config, event buffer, and speech channels exist.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerChatterBootstrapSystem : ISystem
    {
        private EntityQuery _missingSpeech;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            _missingSpeech = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerSpeechChannel>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<VillagerChatterConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, VillagerChatterConfig.Default);
            }

            if (!SystemAPI.TryGetSingletonEntity<VillagerChatterEventBuffer>(out var eventEntity))
            {
                eventEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<VillagerChatterEventBuffer>(eventEntity);
                state.EntityManager.AddBuffer<VillagerChatterEvent>(eventEntity);
            }

            if (_missingSpeech.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missingSpeech, new VillagerSpeechChannel
            {
                NextAvailableTick = 0u,
                LastSpeechTick = 0u,
                LastMessageId = 0u,
                LastListener = Entity.Null
            });
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
