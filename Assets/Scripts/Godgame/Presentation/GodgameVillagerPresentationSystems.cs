using Unity.Burst;
using Unity.Entities;

namespace Godgame.Presentation
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_VillagerPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPresentationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Placeholder for villager presentation bridge.
        }

        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_ResourceChunkPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceChunkPresentationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Placeholder for resource chunk presentation bridge.
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
