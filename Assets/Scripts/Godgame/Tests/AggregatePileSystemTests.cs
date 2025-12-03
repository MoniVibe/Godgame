using Godgame.Resources;
using Godgame.Systems.Resources;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Resources
{
    public class AggregatePileSystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;
        private Entity _configEntity;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(AggregatePileSystemTests));
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
                MergeCheckSeconds = 0f,
                MinSpawnAmount = 0.01f,
                ConservationEpsilon = 0.001f,
                MaxActivePiles = 32
            });
            _entityManager.SetComponentData(_configEntity, new AggregatePileRuntimeState
            {
                ActivePiles = 0,
                NextMergeTime = 0f
            });
            _entityManager.AddBuffer<AggregatePileSpawnCommand>(_configEntity);
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
        public void SpawnCommands_CreateSinglePileAndCountActive()
        {
            var commands = _entityManager.GetBuffer<AggregatePileSpawnCommand>(_configEntity);
            commands.Add(new AggregatePileSpawnCommand { ResourceType = 1, Amount = 30f, Position = float3.zero });
            commands.Add(new AggregatePileSpawnCommand { ResourceType = 1, Amount = 20f, Position = float3.zero });

            UpdateSystem(_world.GetOrCreateSystem<AggregatePileSystem>());

            var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<AggregatePile>());
            Assert.AreEqual(1, query.CalculateEntityCount());
            var pile = query.GetSingleton<AggregatePile>();
            Assert.AreEqual(50f, pile.Amount, 0.001f);

            var runtime = _entityManager.GetComponentData<AggregatePileRuntimeState>(_configEntity);
            Assert.AreEqual(1, runtime.ActivePiles);
        }

        [Test]
        public void SpawnCommands_SplitOverflowIntoMultiplePiles()
        {
            var commands = _entityManager.GetBuffer<AggregatePileSpawnCommand>(_configEntity);
            commands.Add(new AggregatePileSpawnCommand { ResourceType = 2, Amount = 250f, Position = float3.zero });

            UpdateSystem(_world.GetOrCreateSystem<AggregatePileSystem>());

            var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<AggregatePile>());
            Assert.AreEqual(3, query.CalculateEntityCount());

            using var piles = query.ToComponentDataArray<AggregatePile>(Allocator.Temp);
            float total = 0f;
            foreach (var pile in piles)
            {
                total += pile.Amount;
                Assert.LessOrEqual(pile.Amount, 100.001f);
            }

            Assert.AreEqual(250f, total, 0.001f);
        }

        [Test]
        public void MergeNearbyPiles_CombineWhenWithinRadius()
        {
            var commands = _entityManager.GetBuffer<AggregatePileSpawnCommand>(_configEntity);
            commands.Add(new AggregatePileSpawnCommand { ResourceType = 3, Amount = 40f, Position = float3.zero });
            commands.Add(new AggregatePileSpawnCommand { ResourceType = 3, Amount = 60f, Position = new float3(10f, 0f, 0f) });

            // First update: outside merge radius, so two piles.
            UpdateSystem(_world.GetOrCreateSystem<AggregatePileSystem>());
            var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<AggregatePile>(), ComponentType.ReadOnly<LocalTransform>());
            Assert.AreEqual(2, query.CalculateEntityCount());

            // Widen merge radius and force another update to merge.
            var config = _entityManager.GetComponentData<AggregatePileConfig>(_configEntity);
            config.MergeRadius = 20f;
            _entityManager.SetComponentData(_configEntity, config);

            UpdateSystem(_world.GetOrCreateSystem<AggregatePileSystem>());
            Assert.AreEqual(1, query.CalculateEntityCount());
            var merged = query.GetSingleton<AggregatePile>();
            Assert.AreEqual(100f, merged.Amount, 0.001f);
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
