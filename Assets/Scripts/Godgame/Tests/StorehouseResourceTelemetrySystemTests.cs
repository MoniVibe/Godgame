using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Tests.Registry
{
    public class StorehouseResourceTelemetrySystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;
        private BlobAssetReference<ResourceTypeIndexBlob> _catalog;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(StorehouseResourceTelemetrySystemTests));
            _entityManager = _world.EntityManager;
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);

            _catalog = BuildCatalog(new[] { "wood", "stone" });
            var catalogEntity = _entityManager.CreateEntity(typeof(ResourceTypeIndex));
            _entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = _catalog });

            var storehouse = _entityManager.CreateEntity(
                typeof(StorehouseInventory));
            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouse);
            capacities.Add(new StorehouseCapacityElement { ResourceTypeId = "wood", MaxCapacity = 100f });
            capacities.Add(new StorehouseCapacityElement { ResourceTypeId = "stone", MaxCapacity = 50f });

            var items = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouse);
            items.Add(new StorehouseInventoryItem { ResourceTypeId = "wood", Amount = 60f, Reserved = 0f });
            items.Add(new StorehouseInventoryItem { ResourceTypeId = "stone", Amount = 10f, Reserved = 0f });
        }

        [TearDown]
        public void TearDown()
        {
            if (_catalog.IsCreated)
            {
                _catalog.Dispose();
            }

            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void EmitsPerResourceMetrics()
        {
            var telemetryEntity = _entityManager.CreateEntity(typeof(TelemetryStream));
            _entityManager.AddBuffer<TelemetryMetric>(telemetryEntity);

            UpdateSystem(_world.GetOrCreateSystem<StorehouseResourceTelemetrySystem>());

            var metrics = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            AssertMetric(metrics, "storehouse.resource.wood.stored", 60f);
            AssertMetric(metrics, "storehouse.resource.wood.capacity", 100f);
            AssertMetric(metrics, "storehouse.resource.stone.stored", 10f);
            AssertMetric(metrics, "storehouse.resource.stone.capacity", 50f);
        }

        private static void AssertMetric(DynamicBuffer<TelemetryMetric> buffer, string key, float expected)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    Assert.AreEqual(expected, buffer[i].Value, 0.0001f);
                    return;
                }
            }

            Assert.Fail($"Metric {key} missing.");
        }

        private static BlobAssetReference<ResourceTypeIndexBlob> BuildCatalog(string[] ids)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ResourceTypeIndexBlob>();
            var array = builder.Allocate(ref root.Ids, ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                array[i] = ids[i];
            }

            var blob = builder.CreateBlobAssetReference<ResourceTypeIndexBlob>(Allocator.Persistent);
            builder.Dispose();
            return blob;
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
