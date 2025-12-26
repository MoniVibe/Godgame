using Godgame.Registry;
using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Villagers
{
    /// <summary>
    /// Validates villager job state machine transitions and distance thresholds.
    /// </summary>
    public class Jobs_GatherDeliver_StateGraph_Playmode
    {
        private World _world;
        private EntityManager _entityManager;
        private FixedStepSimulationSystemGroup _fixedStepGroup;
        private BlobAssetReference<ResourceTypeIndexBlob> _catalog;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Jobs_GatherDeliver_StateGraph_Playmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _fixedStepGroup = _world.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();

            // Create resource catalog
            _catalog = BuildCatalog(new[] { "wood" });
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
        public void JobState_Transitions_IdleToNavigate()
        {
            // Create resource node
            var nodeEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(GodgameResourceNodeMirror));
            _entityManager.SetComponentData(nodeEntity, LocalTransform.FromPosition(new float3(10f, 0f, 0f)));
            _entityManager.SetComponentData(nodeEntity, new GodgameResourceNodeMirror
            {
                ResourceTypeIndex = 0,
                RemainingAmount = 100f,
                MaxAmount = 100f,
                IsDepleted = 0
            });

            // Create villager at origin
            var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
            _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(float3.zero));
            _entityManager.SetComponentData(villagerEntity, new VillagerJobState
            {
                Type = JobType.Gather,
                Phase = JobPhase.Idle,
                Target = nodeEntity,
                ResourceTypeIndex = 0,
                OutputResourceTypeIndex = ushort.MaxValue,
                CarryCount = 0f,
                CarryMax = 50f
            });
            _entityManager.SetComponentData(villagerEntity, new Navigation
            {
                Destination = float3.zero,
                Speed = 0f
            });

            // Update system
            UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());

            // Verify transition to NavigateToNode
            var jobState = _entityManager.GetComponentData<VillagerJobState>(villagerEntity);
            Assert.AreEqual(JobPhase.NavigateToNode, jobState.Phase, "Should transition to NavigateToNode");

            var nav = _entityManager.GetComponentData<Navigation>(villagerEntity);
            Assert.Greater(nav.Speed, 0f, "Navigation speed should be set");
        }

        [Test]
        public void Navigation_MovesTowardDestination()
        {
            // Create villager with navigation target
            var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
            var startPos = new float3(0f, 0f, 0f);
            var targetPos = new float3(10f, 0f, 0f);
            _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(startPos));
            _entityManager.SetComponentData(villagerEntity, new VillagerJobState
            {
                Type = JobType.Gather,
                Phase = JobPhase.NavigateToNode,
                Target = Entity.Null,
                ResourceTypeIndex = 0,
                OutputResourceTypeIndex = ushort.MaxValue,
                CarryCount = 0f,
                CarryMax = 50f
            });
            _entityManager.SetComponentData(villagerEntity, new Navigation
            {
                Destination = targetPos,
                Speed = 5f
            });

            // Update multiple times
            for (int i = 0; i < 10; i++)
            {
                UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());
            }

            // Verify position changed
            var transform = _entityManager.GetComponentData<LocalTransform>(villagerEntity);
            var distance = math.length(transform.Position - startPos);
            Assert.Greater(distance, 0.1f, "Villager should have moved");
        }

        [Test]
        public void Gather_Phase_IncrementsCarryCount()
        {
            // Create resource node
            var nodeEntity = _entityManager.CreateEntity(typeof(LocalTransform), typeof(GodgameResourceNodeMirror));
            _entityManager.SetComponentData(nodeEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(nodeEntity, new GodgameResourceNodeMirror
            {
                ResourceTypeIndex = 0,
                RemainingAmount = 100f,
                MaxAmount = 100f,
                IsDepleted = 0
            });

            // Create villager at node position (within gather distance)
            var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
            _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(villagerEntity, new VillagerJobState
            {
                Type = JobType.Gather,
                Phase = JobPhase.Gather,
                Target = nodeEntity,
                ResourceTypeIndex = 0,
                OutputResourceTypeIndex = ushort.MaxValue,
                CarryCount = 0f,
                CarryMax = 50f
            });
            _entityManager.SetComponentData(villagerEntity, new Navigation { Destination = float3.zero, Speed = 0f });

            // Update system
            UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());

            // Verify carry count increased
            var jobState = _entityManager.GetComponentData<VillagerJobState>(villagerEntity);
            Assert.Greater(jobState.CarryCount, 0f, "Carry count should increase during gather");
        }

        [Test]
        public void Deliver_Phase_ResetsToIdle()
        {
            // Create storehouse
            var storehouseEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(storehouseEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.AddBuffer<StorehouseInventoryItem>(storehouseEntity);
            _entityManager.AddBuffer<StorehouseCapacityElement>(storehouseEntity);

            // Create villager at storehouse position with resources
            var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
            _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(new float3(0f, 0f, 0f)));
            _entityManager.SetComponentData(villagerEntity, new VillagerJobState
            {
                Type = JobType.Gather,
                Phase = JobPhase.Deliver,
                Target = storehouseEntity,
                ResourceTypeIndex = 0,
                OutputResourceTypeIndex = ushort.MaxValue,
                CarryCount = 30f,
                CarryMax = 50f
            });
            _entityManager.SetComponentData(villagerEntity, new Navigation { Destination = float3.zero, Speed = 0f });

            // Update system
            UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());

            // Verify reset to Idle
            var jobState = _entityManager.GetComponentData<VillagerJobState>(villagerEntity);
            Assert.AreEqual(JobPhase.Idle, jobState.Phase, "Should reset to Idle after deliver");
            Assert.AreEqual(0f, jobState.CarryCount, 0.001f, "Carry count should be reset");
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
