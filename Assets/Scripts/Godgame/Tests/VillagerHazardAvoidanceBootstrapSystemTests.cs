#if UNITY_INCLUDE_TESTS
using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Tests
{
    public sealed class VillagerHazardAvoidanceBootstrapSystemTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(VillagerHazardAvoidanceBootstrapSystemTests));
            _entityManager = _world.EntityManager;
            _entityManager.CreateEntity(typeof(GameWorldTag));
            _entityManager.SetComponentData(_entityManager.CreateEntity(typeof(TimeState)), new TimeState
            {
                Tick = 0,
                FixedDeltaTime = 0.1f,
                IsPaused = false
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
        public void BootstrapAddsHazardComponentsToVillagers()
        {
            var villager = _entityManager.CreateEntity(
                typeof(VillagerId),
                typeof(LocalTransform));

            var system = _world.GetOrCreateSystem<VillagerHazardAvoidanceBootstrapSystem>();
            system.Update(_world.Unmanaged);

            Assert.IsTrue(_entityManager.HasComponent<HazardRaycastProbe>(villager));
            Assert.IsTrue(_entityManager.HasComponent<AvoidanceProfile>(villager));
            Assert.IsTrue(_entityManager.HasComponent<HazardAvoidanceState>(villager));
            Assert.IsTrue(_entityManager.HasComponent<HazardRaycastState>(villager));
            Assert.IsTrue(_entityManager.HasComponent<AvoidanceReactionState>(villager));
            Assert.IsTrue(_entityManager.HasComponent<HazardDodgeTelemetry>(villager));
        }
    }
}
#endif
