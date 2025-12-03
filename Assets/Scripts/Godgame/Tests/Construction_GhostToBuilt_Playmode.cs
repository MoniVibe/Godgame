using Godgame.Construction;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Construction
{
    /// <summary>
    /// Validates construction ghost→build flow with resource payment and telemetry emission.
    /// </summary>
    public class Construction_GhostToBuilt_Playmode
    {
        private World _world;
        private EntityManager _entityManager;
        private FixedStepSimulationSystemGroup _fixedStepGroup;
        private BlobAssetReference<ResourceTypeIndexBlob> _catalog;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Construction_GhostToBuilt_Playmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _fixedStepGroup = _world.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();

            // Create resource catalog
            _catalog = BuildCatalog(new[] { "wood" });
            var catalogEntity = _entityManager.CreateEntity(typeof(ResourceTypeIndex));
            _entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = _catalog });

            // Create time state
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = false;
            timeState.FixedDeltaTime = 0.016f;
            _entityManager.SetComponentData(timeEntity, timeState);
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
        public void ConstructionGhost_Payment_ConvertsToBuilt()
        {
            // Create storehouse with resources
            var storehouseEntity = _entityManager.CreateEntity(typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 1000f,
                TotalStored = 100f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 1000f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);
            inventory.Add(new StorehouseInventoryItem
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                Amount = 100f,
                Reserved = 0f
            });

            // Create construction ghost with cost
            var ghostEntity = _entityManager.CreateEntity(typeof(ConstructionGhost));
            _entityManager.SetComponentData(ghostEntity, new ConstructionGhost
            {
                ResourceTypeIndex = 0, // wood
                Cost = 50,
                Paid = 0
            });

            // Update construction system
            UpdateSystem(_world.GetOrCreateSystem<ConstructionSystem>());

            // Verify payment occurred
            var ghost = _entityManager.GetComponentData<ConstructionGhost>(ghostEntity);
            Assert.Greater(ghost.Paid, 0, "Payment should have occurred");

            // Continue updating until fully paid
            int iterations = 0;
            while (ghost.Paid < ghost.Cost && iterations < 10)
            {
                UpdateSystem(_world.GetOrCreateSystem<ConstructionSystem>());
                ghost = _entityManager.GetComponentData<ConstructionGhost>(ghostEntity);
                iterations++;
            }

            // Verify conversion to built (ghost component removed, completion tag added)
            Assert.IsFalse(_entityManager.HasComponent<ConstructionGhost>(ghostEntity), "Ghost component should be removed");
            Assert.IsTrue(_entityManager.HasComponent<JobsiteCompletionTag>(ghostEntity), "Completion tag should be added");
        }

        [Test]
        public void Construction_Telemetry_FiresOnce()
        {
            // Create storehouse with resources
            var storehouseEntity = _entityManager.CreateEntity(typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 1000f,
                TotalStored = 100f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 1000f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);
            inventory.Add(new StorehouseInventoryItem
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                Amount = 100f,
                Reserved = 0f
            });

            // Create construction ghost
            var ghostEntity = _entityManager.CreateEntity(typeof(ConstructionGhost));
            _entityManager.SetComponentData(ghostEntity, new ConstructionGhost
            {
                ResourceTypeIndex = 0,
                Cost = 30,
                Paid = 0
            });

            // Get initial telemetry count
            var telemetryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TelemetryStream>())
                .GetSingletonEntity();
            var telemetryBuffer = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            int initialCount = telemetryBuffer.Length;

            // Update until completion
            int iterations = 0;
            while (_entityManager.HasComponent<ConstructionGhost>(ghostEntity) && iterations < 10)
            {
                UpdateSystem(_world.GetOrCreateSystem<ConstructionSystem>());
                iterations++;
            }

            // Verify telemetry was emitted
            telemetryBuffer = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            bool foundConstructionMetric = false;
            for (int i = initialCount; i < telemetryBuffer.Length; i++)
            {
                if (telemetryBuffer[i].Key.Equals(new FixedString64Bytes("construction.completed")))
                {
                    foundConstructionMetric = true;
                    Assert.AreEqual(1f, telemetryBuffer[i].Value, 0.001f, "Telemetry should fire once");
                    break;
                }
            }

            Assert.IsTrue(foundConstructionMetric, "Construction completion telemetry should be emitted");
        }

        [Test]
        public void Construction_ResourceTickets_ConsumedCorrectly()
        {
            // Create storehouse with known amount
            var storehouseEntity = _entityManager.CreateEntity(typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 1000f,
                TotalStored = 100f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 1000f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);
            inventory.Add(new StorehouseInventoryItem
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                Amount = 100f,
                Reserved = 0f
            });

            // Create construction ghost with cost
            int constructionCost = 45;
            var ghostEntity = _entityManager.CreateEntity(typeof(ConstructionGhost));
            _entityManager.SetComponentData(ghostEntity, new ConstructionGhost
            {
                ResourceTypeIndex = 0,
                Cost = constructionCost,
                Paid = 0
            });

            // Get initial inventory amount
            float initialInventory = 0f;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    initialInventory = inventory[i].Amount;
                    break;
                }
            }

            // Update until completion
            int iterations = 0;
            while (_entityManager.HasComponent<ConstructionGhost>(ghostEntity) && iterations < 10)
            {
                UpdateSystem(_world.GetOrCreateSystem<ConstructionSystem>());
                iterations++;
            }

            // Verify resources consumed
            float finalInventory = 0f;
            inventory = _entityManager.GetBuffer<StorehouseInventoryItem>(storehouseEntity);
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    finalInventory = inventory[i].Amount;
                    break;
                }
            }

            float consumed = initialInventory - finalInventory;
            Assert.AreEqual(constructionCost, consumed, 1f, "Resources consumed should equal construction cost");
        }

        private void UpdateSystem(SystemHandle handle)
        {
            _fixedStepGroup.RemoveSystemFromUpdateList(handle);
            _fixedStepGroup.AddSystemToUpdateList(handle);
            _fixedStepGroup.SortSystems();
            _fixedStepGroup.Update();
        }

        private static BlobAssetReference<ResourceTypeIndexBlob> BuildCatalog(string[] ids)
        {
            var builder = new BlobBuilder(Allocator.Persistent);
            ref var root = ref builder.ConstructRoot<ResourceTypeIndexBlob>();
            var idsArray = builder.Allocate(ref root.Ids, ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                idsArray[i] = new FixedString64Bytes(ids[i]);
            }

            return builder.CreateBlobAssetReference<ResourceTypeIndexBlob>(Allocator.Persistent);
        }
    }
}

