using Godgame.Presentation;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Performance
{
    public struct PresentationMetrics : IComponentData
    {
        public int VillagerCount;
        public int ChunkCount;
        public int VillageCenterCount;
        public int VillagersRendered;
        public int ChunksRendered;
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_PresentationMetricsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<PresentationMetrics>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var metricsEntity = SystemAPI.GetSingletonEntity<PresentationMetrics>();
            var metrics = SystemAPI.GetComponentRW<PresentationMetrics>(metricsEntity);

            var villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerPresentationTag>()
                .Build();
            var chunkQuery = SystemAPI.QueryBuilder()
                .WithAll<ResourceChunkPresentationTag>()
                .Build();
            var centerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillageCenterPresentationTag>()
                .Build();

            var villagers = villagerQuery.CalculateEntityCount();
            var chunks = chunkQuery.CalculateEntityCount();
            var centers = centerQuery.CalculateEntityCount();

            metrics.ValueRW = new PresentationMetrics
            {
                VillagerCount = villagers,
                ChunkCount = chunks,
                VillageCenterCount = centers,
                VillagersRendered = villagers,
                ChunksRendered = chunks
            };
        }

        public void OnDestroy(ref SystemState state) { }
    }

}
