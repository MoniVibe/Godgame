using Godgame.Economy;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Scripting.APIUpdating;

namespace Godgame.Scenario
{
    [MovedFrom(true, "Godgame.Scenario", null, "GodgameDemoBootstrapConfig")]
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

    [MovedFrom(true, "Godgame.Scenario", null, "GodgameDemoBootstrapRuntime")]
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
