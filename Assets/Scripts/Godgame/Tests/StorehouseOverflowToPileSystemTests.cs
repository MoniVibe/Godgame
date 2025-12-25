using Godgame.Resources;
using Godgame.Runtime;
using Godgame.Systems.Resources;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Runtime.Resource;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GodgameDivineHandState = Godgame.Runtime.DivineHandState;

namespace Godgame.Tests.Resources
{
    public class StorehouseOverflowToPileSystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;
        private Entity _configEntity;
        private BlobAssetReference<ResourceTypeIndexBlob> _catalog;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(StorehouseOverflowToPileSystemTests));
            _entityManager = _world.EntityManager;
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            _configEntity = _entityManager.CreateEntity(
                typeof(AggregatePileConfig),
                typeof(AggregatePileRuntimeState));

            _entityManager.SetComponentData(_configEntity, new AggregatePileConfig
            {
                DefaultMaxCapacity = 100f,
                GlobalMaxCapacity = 100f,
                MergeRadius = 5f,
                SplitThreshold = 0f,
                MergeCheckSeconds = 1f,
                MinSpawnAmount = 0.01f,
                ConservationEpsilon = 0.001f,
                MaxActivePiles = 8
            });
            _entityManager.SetComponentData(_configEntity, new AggregatePileRuntimeState
            {
                ActivePiles = 0,
                NextMergeTime = 0f
            });
            _entityManager.AddBuffer<AggregatePileSpawnCommand>(_configEntity);

            _catalog = BuildCatalog(new[] { "wood", "stone" });
            var catalogEntity = _entityManager.CreateEntity(typeof(ResourceTypeIndex));
            _entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = _catalog });

            var timeEntity = _entityManager.CreateEntity(typeof(TimeState));
            _entityManager.SetComponentData(timeEntity, new TimeState
            {
                Tick = 0,
                DeltaTime = 1f / 60f,
                DeltaSeconds = 1f / 60f,
                ElapsedTime = 0f,
                WorldSeconds = 0f,
                IsPaused = false,
                FixedDeltaTime = 1f / 60f,
                CurrentSpeedMultiplier = 1f
            });
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
        public void DumpToFullStorehouse_SpawnsPileAndClearsCommand()
        {
            var storehouse = _entityManager.CreateEntity(
                typeof(StorehouseInventory),
                typeof(LocalTransform));
            _entityManager.AddBuffer<StorehouseCapacityElement>(storehouse).Add(new StorehouseCapacityElement
            {
                ResourceTypeId = "wood",
                MaxCapacity = 10f
            });
            _entityManager.AddBuffer<StorehouseInventoryItem>(storehouse).Add(new StorehouseInventoryItem
            {
                ResourceTypeId = "wood",
                Amount = 10f,
                Reserved = 0f
            });
            _entityManager.SetComponentData(storehouse, LocalTransform.FromPosition(new float3(1f, 0f, 2f)));

            var handEntity = _entityManager.CreateEntity(
                typeof(DivineHandTag),
                typeof(GodgameDivineHandState));
            _entityManager.AddBuffer<HandCommand>(handEntity);
            _entityManager.SetComponentData(handEntity, new GodgameDivineHandState
            {
                HeldAmount = 5,
                HeldResourceTypeIndex = 0 // wood
            });
            var commandBuffer = _entityManager.GetBuffer<HandCommand>(handEntity);
            commandBuffer.Add(new HandCommand
            {
                Tick = 0,
                Type = HandCommandType.Dump,
                TargetEntity = storehouse,
                ResourceTypeIndex = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<StorehouseOverflowToPileSystem>());

            var commands = _entityManager.GetBuffer<AggregatePileSpawnCommand>(_configEntity);
            Assert.AreEqual(1, commands.Length);
            Assert.AreEqual(0, commands[0].ResourceType);
            Assert.AreEqual(5f, commands[0].Amount, 0.0001f);
            Assert.AreEqual(new float3(1f, 0f, 2f), commands[0].Position);

            var handState = _entityManager.GetComponentData<GodgameDivineHandState>(handEntity);
            Assert.AreEqual(0, handState.HeldAmount);
            Assert.AreEqual(DivineHandConstants.NoResourceType, handState.HeldResourceTypeIndex);

            var command = _entityManager.GetBuffer<HandCommand>(handEntity);
            Assert.AreEqual(1, command.Length);
            Assert.AreEqual(HandCommandType.None, command[0].Type);
            Assert.AreEqual(Entity.Null, command[0].TargetEntity);
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
