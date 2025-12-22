using Godgame.Economy;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Scenario
{
    public struct GodgameScenarioBootstrapConfig : IComponentData
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

    public struct GodgameScenarioBootstrapRuntime : IComponentData
    {
        public byte HasInitialized;
    }
}

namespace Godgame.Scenario
{
    public struct GodgameScenarioResourceNode : IComponentData
    {
        public float3 Position;
        public ResourceType ResourceType;
        public int Capacity;
    }
}
