using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Scenario
{
    /// <summary>
    /// Authoring-time configuration describing which prefabs to spawn for the scenario settlement.
    /// </summary>
    public struct SettlementConfig : IComponentData
    {
        public Entity VillageCenterPrefab;
        public Entity StorehousePrefab;
        public Entity HousingPrefab;
        public Entity WorshipPrefab;
        public Entity VillagerPrefab;
        public int InitialVillagers;
        public float VillagerSpawnRadius;
        public float BuildingRingRadius;
        public float ResourceRingRadius;
        public uint Seed;
    }

    /// <summary>
    /// Tracks runtime state for the settlement bootstrap so we only spawn content once.
    /// </summary>
    public struct SettlementRuntime : IComponentData
    {
        public byte HasSpawned;
        public Entity VillageCenterInstance;
        public Entity StorehouseInstance;
        public Entity HousingInstance;
        public Entity WorshipInstance;
    }

    /// <summary>
    /// Buffer that references the resource nodes spawned for a settlement so behaviour systems can query them quickly.
    /// </summary>
    public struct SettlementResource : IBufferElementData
    {
        public Entity Node;
    }

    /// <summary>
    /// Simple resource node descriptor the villagers can target.
    /// </summary>
    public struct SettlementResourceNode : IComponentData
    {
        public Entity Settlement;
        public float3 Position;
        public FixedString32Bytes Label;
    }

    /// <summary>
    /// High level state for the simple villager behaviour loop used in the scenario.
    /// </summary>
    public enum SettlementVillagerPhase : byte
    {
        Idle = 0,
        ToResource = 1,
        Harvest = 2,
        ToDepot = 3,
        Resting = 4
    }

    /// <summary>
    /// Tracks villager specific information (current phase, timers, and pseudo random state).
    /// </summary>
    public struct SettlementVillagerState : IComponentData
    {
        public Entity Settlement;
        public Entity CurrentResourceNode;
        public Entity CurrentDepot;
        public SettlementVillagerPhase Phase;
        public float PhaseTimer;
        public uint RandomState;
    }
}
