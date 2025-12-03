using Godgame.Resources;
using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GodgameStorehouseApi = Godgame.Resources.StorehouseApi;

namespace Godgame.Tests.Resources
{
    /// <summary>
    /// Validates resource conservation in the villager gather→deliver→storehouse flow.
    /// </summary>
    public class Conservation_VillagerGatherDeliver_Playmode
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;
        private BlobAssetReference<ResourceTypeIndexBlob> _catalog;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Conservation_VillagerGatherDeliver_Playmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            // Create resource catalog
            _catalog = BuildCatalog(new[] { "wood", "stone", "ore" });
            var catalogEntity = _entityManager.CreateEntity(typeof(ResourceTypeIndex));
            _entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = _catalog });
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
        public void VillagerGatherDeliver_ConservesResources()
        {
            // Create resource node (wood, index 0)
            var nodeEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(nodeEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            var nodeMirror = new GodgameResourceNodeMirror
            {
                ResourceTypeIndex = 0, // wood
                RemainingAmount = 100f,
                MaxAmount = 100f,
                RegenerationRate = 0f,
                IsDepleted = 0,
                LastMutationTick = 0
            };
            _entityManager.AddComponentData(nodeEntity, nodeMirror);

            // Create storehouse with capacity for wood
            var storehouseEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, LocalTransform.FromPosition(new float3(10f, 0f, 0f)));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 500f,
                TotalStored = 0f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 500f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);

            // Simulate villager gathering 50 units from node
            float gathered = 50f;
            nodeMirror.RemainingAmount -= gathered;
            _entityManager.SetComponentData(nodeEntity, nodeMirror);

            // Villager deposits into storehouse via API
            var deposited = GodgameStorehouseApi.TryDeposit(ref inventory, _catalog, 0, gathered, 500f);
            Assert.IsTrue(deposited, "Deposit should succeed");

            // Verify conservation: gathered amount equals stored amount
            float totalStored = 0f;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    totalStored = inventory[i].Amount;
                    break;
                }
            }

            Assert.AreEqual(gathered, totalStored, 0.001f, "Gathered amount should equal stored amount");
            Assert.AreEqual(50f, nodeMirror.RemainingAmount, 0.001f, "Node should have remaining amount");
        }

        [Test]
        public void MultipleDeposits_AccumulateCorrectly()
        {
            // Create storehouse
            var storehouseEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 200f,
                TotalStored = 0f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 200f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);

            // Simulate multiple deposits
            float deposit1 = 30f;
            float deposit2 = 40f;
            float deposit3 = 25f;

            Assert.IsTrue(GodgameStorehouseApi.TryDeposit(ref inventory, _catalog, 0, deposit1, 200f));
            Assert.IsTrue(GodgameStorehouseApi.TryDeposit(ref inventory, _catalog, 0, deposit2, 200f));
            Assert.IsTrue(GodgameStorehouseApi.TryDeposit(ref inventory, _catalog, 0, deposit3, 200f));

            // Verify total
            float totalStored = 0f;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    totalStored = inventory[i].Amount;
                    break;
                }
            }

            float expectedTotal = deposit1 + deposit2 + deposit3;
            Assert.AreEqual(expectedTotal, totalStored, 0.001f, "Multiple deposits should accumulate");
        }

        [Test]
        public void Withdraw_RemovesCorrectAmount()
        {
            // Create storehouse with initial stock
            var storehouseEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 200f,
                TotalStored = 100f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 200f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);
            inventory.Add(new StorehouseInventoryItem
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                Amount = 100f,
                Reserved = 0f
            });

            // Withdraw 40 units
            float withdrawn = GodgameStorehouseApi.Withdraw(ref inventory, _catalog, 0, 40f);
            Assert.AreEqual(40f, withdrawn, 0.001f, "Should withdraw requested amount");

            // Verify remaining
            float remaining = 0f;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    remaining = inventory[i].Amount;
                    break;
                }
            }

            Assert.AreEqual(60f, remaining, 0.001f, "Remaining should be correct");
        }

        [Test]
        public void CapacityLimit_RejectsExcess()
        {
            // Create storehouse with limited capacity
            var storehouseEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(StorehouseInventory));
            _entityManager.SetComponentData(storehouseEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(storehouseEntity, new StorehouseInventory
            {
                TotalCapacity = 100f,
                TotalStored = 0f,
                LastUpdateTick = 0
            });

            var capacities = _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 100f
            });

            var inventory = _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);

            // Try to deposit 150 units (exceeds capacity)
            bool deposited = GodgameStorehouseApi.TryDeposit(ref inventory, _catalog, 0, 150f, 100f);
            Assert.IsTrue(deposited, "Should accept partial deposit");

            // Verify only capacity amount was stored
            float totalStored = 0f;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(new FixedString64Bytes("wood")))
                {
                    totalStored = inventory[i].Amount;
                    break;
                }
            }

            Assert.AreEqual(100f, totalStored, 0.001f, "Should only store up to capacity");
        }

        private static BlobAssetReference<ResourceTypeIndexBlob> BuildCatalog(string[] ids)
        {
            using var builder = new BlobBuilder(Unity.Collections.Allocator.Persistent);
            ref var root = ref builder.ConstructRoot<ResourceTypeIndexBlob>();
            var idsArray = builder.Allocate(ref root.Ids, ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                idsArray[i] = new FixedString64Bytes(ids[i]);
            }

            return builder.CreateBlobAssetReference<ResourceTypeIndexBlob>(Unity.Collections.Allocator.Persistent);
        }
    }
}

