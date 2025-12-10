using Godgame.Economy;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Demo
{
    public struct GodgameDemoBootstrapConfig : IComponentData
    {
        public Entity VillagerPrefab;
        public Entity StorehousePrefab;
        public int InitialVillagerCount;
        public int ResourceNodeCount;
        public float VillagerSpawnRadius;
        public float ResourceNodeRadius;
        public uint Seed;
        public float BehaviorRandomizationRange;
    }

    public struct GodgameDemoBootstrapRuntime : IComponentData
    {
        public byte HasInitialized;
    }

    public struct GodgameDemoResourceNode : IComponentData
    {
        public float3 Position;
        public ResourceType ResourceType;
        public int Capacity;
    }
}

