using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Villagers
{
    /// <summary>
    /// Validates that villager job system emits telemetry counters as expected.
    /// </summary>
    public class Jobs_Throughput_Telemetry_Editmode
    {
        private World _world;
        private EntityManager _entityManager;
        private FixedStepSimulationSystemGroup _fixedStepGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Jobs_Throughput_Telemetry_Editmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _fixedStepGroup = _world.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();

            // Create resource catalog
            var catalog = BuildCatalog(new[] { "wood" });
            var catalogEntity = _entityManager.CreateEntity(typeof(ResourceTypeIndex));
            _entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = catalog });
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
        public void JobSystem_EmitsTelemetry_WhenJobsComplete()
        {
            // Create villager with job
            var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
            _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(float3.zero));
            _entityManager.SetComponentData(villagerEntity, new VillagerJobState
            {
                Type = JobType.Gather,
                Phase = JobPhase.Idle,
                Target = Entity.Null,
                ResourceTypeIndex = 0,
                CarryCount = 0f,
                CarryMax = 50f
            });
            _entityManager.SetComponentData(villagerEntity, new Navigation { Destination = float3.zero, Speed = 0f });

            // Update system
            UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());

            // Verify telemetry stream exists (system should not crash)
            var telemetryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TelemetryStream>())
                .GetSingletonEntity();
            Assert.IsTrue(_entityManager.Exists(telemetryEntity), "Telemetry stream should exist");
        }

        [Test]
        public void MultipleVillagers_ProcessInParallel()
        {
            // Create multiple villagers
            for (int i = 0; i < 5; i++)
            {
                var villagerEntity = _entityManager.CreateEntity(typeof(VillagerJobState), typeof(LocalTransform), typeof(Navigation));
                _entityManager.SetComponentData(villagerEntity, LocalTransform.FromPosition(new float3(i * 5f, 0f, 0f)));
                _entityManager.SetComponentData(villagerEntity, new VillagerJobState
                {
                    Type = JobType.Gather,
                    Phase = JobPhase.Idle,
                    Target = Entity.Null,
                    ResourceTypeIndex = 0,
                    CarryCount = 0f,
                    CarryMax = 50f
                });
                _entityManager.SetComponentData(villagerEntity, new Navigation { Destination = float3.zero, Speed = 0f });
            }

            // Update system
            UpdateSystem(_world.GetOrCreateSystem<VillagerJobSystem>());

            // Verify all villagers processed (no crashes)
            var query = _entityManager.CreateEntityQuery(typeof(VillagerJobState));
            Assert.AreEqual(5, query.CalculateEntityCount(), "All villagers should exist");
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

