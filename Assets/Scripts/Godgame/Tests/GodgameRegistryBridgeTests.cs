using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Registry
{
    /// <summary>
    /// Smoke coverage for registry bridge/sync systems to ensure they instantiate and seed metadata without managed fields.
    /// </summary>
    public class GodgameRegistryBridgeTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World("GodgameRegistryBridgeTests");
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void BridgeSystems_CreateAndSeedMetadata()
        {
            // Types should remain unmanaged/Burst-friendly.
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameVillagerSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameStorehouseSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameResourceNodeSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameSpawnerSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameBandSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameLogisticsSyncSystem>());
            Assert.IsTrue(UnsafeUtility.IsUnmanaged<GodgameRegistryBridgeSystem>());

            var villagerSync = _world.GetOrCreateSystem<GodgameVillagerSyncSystem>();
            var storehouseSync = _world.GetOrCreateSystem<GodgameStorehouseSyncSystem>();
            var resourceSync = _world.GetOrCreateSystem<GodgameResourceNodeSyncSystem>();
            var spawnerSync = _world.GetOrCreateSystem<GodgameSpawnerSyncSystem>();
            var bandSync = _world.GetOrCreateSystem<GodgameBandSyncSystem>();
            var logisticsSync = _world.GetOrCreateSystem<GodgameLogisticsSyncSystem>();
            var bridge = _world.GetOrCreateSystem<GodgameRegistryBridgeSystem>();
            var directory = _world.GetOrCreateSystem<RegistryDirectorySystem>();

            UpdateSystem(villagerSync);
            UpdateSystem(storehouseSync);
            UpdateSystem(resourceSync);
            UpdateSystem(spawnerSync);
            UpdateSystem(bandSync);
            UpdateSystem(logisticsSync);
            UpdateSystem(bridge);
            UpdateSystem(directory);

            AssertMetadata(GodgameRegistryIds.VillagerArchetype, ComponentType.ReadOnly<VillagerRegistry>());
            AssertMetadata(GodgameRegistryIds.StorehouseArchetype, ComponentType.ReadOnly<StorehouseRegistry>());
            AssertMetadata(GodgameRegistryIds.ResourceNodeArchetype, ComponentType.ReadOnly<ResourceRegistry>());
            AssertMetadata(GodgameRegistryIds.SpawnerArchetype, ComponentType.ReadOnly<SpawnerRegistry>());
            AssertMetadata(GodgameRegistryIds.BandArchetype, ComponentType.ReadOnly<BandRegistry>());
            AssertMetadata(GodgameRegistryIds.MiracleArchetype, ComponentType.ReadOnly<MiracleRegistry>());
            AssertMetadata(GodgameRegistryIds.LogisticsRequestArchetype, ComponentType.ReadOnly<LogisticsRequestRegistry>());
        }

        private void UpdateSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }

        private void AssertMetadata(ushort expectedArchetype, ComponentType registryType)
        {
            using var query = _entityManager.CreateEntityQuery(registryType, ComponentType.ReadOnly<RegistryMetadata>());
            Assert.IsFalse(query.IsEmptyIgnoreFilter, $"Registry {registryType} missing");
            var entity = query.GetSingletonEntity();
            var metadata = _entityManager.GetComponentData<RegistryMetadata>(entity);
            Assert.AreEqual(expectedArchetype, metadata.ArchetypeId);
        }

        [Test]
        public void LogisticsRequests_ArePublishedToRegistry()
        {
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.Tick = 8;
            _entityManager.SetComponentData(timeEntity, timeState);

            var requestEntity = _entityManager.CreateEntity(typeof(LogisticsRequest), typeof(LogisticsRequestProgress));
            _entityManager.SetComponentData(requestEntity, new LogisticsRequest
            {
                SourceEntity = Entity.Null,
                DestinationEntity = Entity.Null,
                SourcePosition = new float3(1f, 0f, 1f),
                DestinationPosition = new float3(4f, 0f, 2f),
                ResourceTypeIndex = 0,
                RequestedUnits = 12f,
                FulfilledUnits = 3f,
                Priority = LogisticsRequestPriority.High,
                Flags = LogisticsRequestFlags.Urgent,
                CreatedTick = timeState.Tick,
                LastUpdateTick = timeState.Tick
            });
            _entityManager.SetComponentData(requestEntity, new LogisticsRequestProgress
            {
                AssignedUnits = 5f,
                AssignedTransportCount = 2,
                LastAssignmentTick = timeState.Tick
            });

            UpdateSystem(_world.GetOrCreateSystem<GodgameLogisticsSyncSystem>());
            UpdateSystem(_world.GetOrCreateSystem<LogisticsRequestRegistrySystem>());
            UpdateSystem(_world.GetOrCreateSystem<GodgameRegistryBridgeSystem>());

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>())
                .GetSingletonEntity();
            var registry = _entityManager.GetComponentData<LogisticsRequestRegistry>(registryEntity);
            Assert.AreEqual(1, registry.TotalRequests);
            Assert.AreEqual(1, registry.InProgressRequests);
            Assert.AreEqual(1, registry.CriticalRequests);

            var entries = _entityManager.GetBuffer<LogisticsRequestRegistryEntry>(registryEntity);
            Assert.AreEqual(1, entries.Length);
            var entry = entries[0];
            Assert.AreEqual(requestEntity, entry.RequestEntity);
            Assert.AreEqual(12f, entry.RequestedUnits, 0.001f);
            Assert.AreEqual(5f, entry.AssignedUnits, 0.001f);
            Assert.AreEqual(LogisticsRequestPriority.High, entry.Priority);
            Assert.AreEqual(LogisticsRequestFlags.Urgent, entry.Flags);
        }
    }
}
