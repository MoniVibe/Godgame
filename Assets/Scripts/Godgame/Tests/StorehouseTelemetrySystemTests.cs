using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Tests.Registry
{
    public class StorehouseTelemetrySystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(StorehouseTelemetrySystemTests));
            _entityManager = _world.EntityManager;
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            var registryEntity = _entityManager.CreateEntity(typeof(StorehouseRegistry));
            _entityManager.SetComponentData(registryEntity, new StorehouseRegistry
            {
                TotalStorehouses = 3,
                TotalCapacity = 120f,
                TotalStored = 45f,
                LastUpdateTick = 10
            });
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
        public void PublishesTotalsToTelemetryStream()
        {
            var telemetryEntity = _entityManager.CreateEntity(typeof(TelemetryStream));
            _entityManager.AddBuffer<TelemetryMetric>(telemetryEntity);

            UpdateSystem(_world.GetOrCreateSystem<StorehouseTelemetrySystem>());

            var metrics = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            Assert.AreEqual(3, metrics.Length);

            AssertMetric(metrics, "storehouse.capacity.total", 120f);
            AssertMetric(metrics, "storehouse.stored.total", 45f);
            AssertMetric(metrics, "storehouse.count", 3f);
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

        private void UpdateSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}
