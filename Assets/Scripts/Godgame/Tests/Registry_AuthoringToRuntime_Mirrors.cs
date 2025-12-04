using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Registry
{
    /// <summary>
    /// Validates that authored bands and logistics entities are correctly mirrored into registry entries with unique IDs and consistent versions.
    /// </summary>
    public class Registry_AuthoringToRuntime_Mirrors
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Registry_AuthoringToRuntime_Mirrors));
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
        public void Bands_FromAuthoring_MirrorToRegistry_WithUniqueIds()
        {
            // Create band entities with unique IDs
            var band1 = _entityManager.CreateEntity(typeof(GodgameBand), typeof(LocalTransform));
            _entityManager.SetComponentData(band1, new GodgameBand
            {
                BandId = 1,
                FactionId = 0,
                MemberCount = 5,
                Morale = 0.6f,
                Cohesion = 0.7f,
                AverageDiscipline = 0.5f,
                StatusFlags = BandStatusFlags.None,
                Formation = BandFormationType.Column
            });
            _entityManager.SetComponentData(band1, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));

            var band2 = _entityManager.CreateEntity(typeof(GodgameBand), typeof(LocalTransform));
            _entityManager.SetComponentData(band2, new GodgameBand
            {
                BandId = 2,
                FactionId = 0,
                MemberCount = 8,
                Morale = 0.8f,
                Cohesion = 0.6f,
                AverageDiscipline = 0.7f,
                StatusFlags = BandStatusFlags.None,
                Formation = BandFormationType.Line
            });
            _entityManager.SetComponentData(band2, LocalTransform.FromPosition(new float3(10f, 0f, 10f)));

            UpdateSystem(_world.GetOrCreateSystem<GodgameRegistryCoordinatorSystem>());

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandRegistry>())
                .GetSingletonEntity();
            var registry = _entityManager.GetComponentData<BandRegistry>(registryEntity);
            var metadata = _entityManager.GetComponentData<RegistryMetadata>(registryEntity);

            Assert.AreEqual(2, registry.TotalBands);
            Assert.AreEqual(13, registry.TotalMembers); // 5 + 8
            Assert.AreEqual(GodgameRegistryIds.BandArchetype, metadata.ArchetypeId);
            Assert.Greater(metadata.LastUpdateTick, 0u);

            var entries = _entityManager.GetBuffer<BandRegistryEntry>(registryEntity);
            Assert.AreEqual(2, entries.Length);

            // Verify unique IDs
            var ids = new NativeHashSet<int>(2, Allocator.Temp);
            foreach (var entry in entries)
            {
                Assert.IsFalse(ids.Contains(entry.BandId), $"Duplicate BandId: {entry.BandId}");
                ids.Add(entry.BandId);
            }
            ids.Dispose();

            // Verify data consistency
            var band1Entry = entries[0].BandId == 1 ? entries[0] : entries[1];
            var band2Entry = entries[0].BandId == 2 ? entries[0] : entries[1];

            Assert.AreEqual(5, band1Entry.MemberCount);
            Assert.AreEqual(0.6f, band1Entry.Morale, 0.001f);
            Assert.AreEqual(band1, band1Entry.BandEntity);

            Assert.AreEqual(8, band2Entry.MemberCount);
            Assert.AreEqual(0.8f, band2Entry.Morale, 0.001f);
            Assert.AreEqual(band2, band2Entry.BandEntity);

        }

        [Test]
        public void Logistics_FromAuthoring_MirrorToRegistry_WithTimestamps()
        {
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.Tick = 42;
            _entityManager.SetComponentData(timeEntity, timeState);

            // Create logistics request entities
            var logistics1 = _entityManager.CreateEntity(typeof(LogisticsRequest), typeof(LogisticsRequestProgress));
            _entityManager.SetComponentData(logistics1, new LogisticsRequest
            {
                ResourceTypeIndex = 0,
                RequestedUnits = 10f,
                SourcePosition = new float3(0f, 0f, 0f),
                DestinationPosition = new float3(5f, 0f, 5f),
                Priority = LogisticsRequestPriority.Normal,
                Flags = LogisticsRequestFlags.None,
                CreatedTick = 0,
                LastUpdateTick = 0
            });

            var logistics2 = _entityManager.CreateEntity(typeof(LogisticsRequest), typeof(LogisticsRequestProgress));
            _entityManager.SetComponentData(logistics2, new LogisticsRequest
            {
                ResourceTypeIndex = 1,
                RequestedUnits = 20f,
                SourcePosition = new float3(10f, 0f, 10f),
                DestinationPosition = new float3(15f, 0f, 15f),
                Priority = LogisticsRequestPriority.High,
                Flags = LogisticsRequestFlags.Urgent,
                CreatedTick = 0,
                LastUpdateTick = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<GodgameLogisticsSyncSystem>());
            UpdateSystem(_world.GetOrCreateSystem<LogisticsRequestRegistrySystem>());
            UpdateSystem(_world.GetOrCreateSystem<GodgameRegistryCoordinatorSystem>());

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>())
                .GetSingletonEntity();
            var registry = _entityManager.GetComponentData<LogisticsRequestRegistry>(registryEntity);
            var metadata = _entityManager.GetComponentData<RegistryMetadata>(registryEntity);

            Assert.AreEqual(2, registry.TotalRequests);
            Assert.AreEqual(GodgameRegistryIds.LogisticsRequestArchetype, metadata.ArchetypeId);
            Assert.Greater(metadata.LastUpdateTick, 0u);

            var entries = _entityManager.GetBuffer<LogisticsRequestRegistryEntry>(registryEntity);
            Assert.AreEqual(2, entries.Length);

            // Verify timestamps are set consistently
            foreach (var entry in entries)
            {
                Assert.Greater(entry.CreatedTick, 0u);
                Assert.AreEqual(timeState.Tick, entry.LastUpdateTick);
            }

            // Verify unique entities
            var entities = new NativeHashSet<Entity>(2, Allocator.Temp);
            foreach (var entry in entries)
            {
                Assert.IsFalse(entities.Contains(entry.RequestEntity), $"Duplicate RequestEntity");
                entities.Add(entry.RequestEntity);
            }
            entities.Dispose();

            // Verify data consistency
            var logistics1Entry = entries[0].RequestEntity == logistics1 ? entries[0] : entries[1];
            var logistics2Entry = entries[0].RequestEntity == logistics2 ? entries[0] : entries[1];

            Assert.AreEqual(0, logistics1Entry.ResourceTypeIndex);
            Assert.AreEqual(10f, logistics1Entry.RequestedUnits, 0.001f);
            Assert.AreEqual(LogisticsRequestPriority.Normal, logistics1Entry.Priority);

            Assert.AreEqual(1, logistics2Entry.ResourceTypeIndex);
            Assert.AreEqual(20f, logistics2Entry.RequestedUnits, 0.001f);
            Assert.AreEqual(LogisticsRequestPriority.High, logistics2Entry.Priority);
            Assert.AreEqual(LogisticsRequestFlags.Urgent, logistics2Entry.Flags);
        }

        [Test]
        public void Registry_Counts_MatchAuthoredEntities()
        {
            // Create multiple bands and logistics requests
            for (int i = 0; i < 5; i++)
            {
                var band = _entityManager.CreateEntity(typeof(GodgameBand), typeof(LocalTransform));
                _entityManager.SetComponentData(band, new GodgameBand
                {
                    BandId = i + 1,
                    FactionId = 0,
                    MemberCount = 3 + i,
                    Morale = 0.5f,
                    Cohesion = 0.5f,
                    AverageDiscipline = 0.5f
                });
                _entityManager.SetComponentData(band, LocalTransform.FromPosition(new float3(i * 5f, 0f, 0f)));
            }

            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            for (int i = 0; i < 3; i++)
            {
                var logistics = _entityManager.CreateEntity(typeof(LogisticsRequest), typeof(LogisticsRequestProgress));
                _entityManager.SetComponentData(logistics, new LogisticsRequest
                {
                    ResourceTypeIndex = (ushort)i,
                    RequestedUnits = 10f + i,
                    SourcePosition = float3.zero,
                    DestinationPosition = new float3(i, 0f, i),
                    Priority = LogisticsRequestPriority.Normal
                });
            }

            UpdateSystem(_world.GetOrCreateSystem<GodgameLogisticsSyncSystem>());
            UpdateSystem(_world.GetOrCreateSystem<LogisticsRequestRegistrySystem>());
            UpdateSystem(_world.GetOrCreateSystem<GodgameRegistryCoordinatorSystem>());

            var bandRegistryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BandRegistry>())
                .GetSingletonEntity();
            var bandRegistry = _entityManager.GetComponentData<BandRegistry>(bandRegistryEntity);
            Assert.AreEqual(5, bandRegistry.TotalBands);

            var logisticsRegistryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>())
                .GetSingletonEntity();
            var logisticsRegistry = _entityManager.GetComponentData<LogisticsRequestRegistry>(logisticsRegistryEntity);
            Assert.AreEqual(3, logisticsRegistry.TotalRequests);
        }

        private void UpdateSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}

