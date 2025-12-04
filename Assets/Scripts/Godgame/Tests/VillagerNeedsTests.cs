using Godgame.Villagers;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using GTimeState = PureDOTS.Runtime.Time.TimeState;
using GInterrupt = Godgame.Villagers.VillagerInterrupt;
using GInterruptType = Godgame.Villagers.VillagerInterruptType;
using Needs = Godgame.Villagers.VillagerNeeds;
using JobState = Godgame.Villagers.VillagerJob;
using JobType = Godgame.Villagers.VillagerJob.JobType;
using JobPhase = Godgame.Villagers.VillagerJob.JobPhase;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for villager needs system (decay/replenish).
    /// </summary>
    public class VillagerNeedsTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null)
            {
                _world = new World("Test World");
            }
            _entityManager = _world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.Name == "Test World")
            {
                _world.Dispose();
            }
        }

        [Test]
        public void VillagerNeedsSystem_Exists()
        {
            var systemHandle = _world.GetOrCreateSystem<VillagerNeedsSystem>();
            Assert.AreNotEqual(SystemHandle.Null, systemHandle, "VillagerNeedsSystem should exist");
        }

        [Test]
        public void VillagerNeeds_EnergyDecaysWhenWorking()
        {
            // Create test villager with needs
            var villager = _entityManager.CreateEntity();
            _entityManager.AddComponentData(villager, new Needs
            {
                Health = 100f,
                MaxHealth = 100f,
                Energy = 1000f, // Full energy
                Morale = 1000f
            });

            _entityManager.AddComponentData(villager, new JobState
            {
                Type = JobType.Gatherer,
                Phase = JobPhase.Gathering, // Working
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.None
            });

            // Create time state
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(GTimeState));

            // Run system
            var needsSystem = _world.GetOrCreateSystem<VillagerNeedsSystem>();
            _world.Update();

            // Verify energy decayed
            var needs = _entityManager.GetComponentData<Needs>(villager);
            Assert.Less(needs.Energy, 1000f, "Energy should decay when working");
        }

        [Test]
        public void VillagerNeeds_EnergyDecaysSlowerWhenIdle()
        {
            // Create test villager with needs
            var villager = _entityManager.CreateEntity();
            var initialEnergy = 1000f;
            _entityManager.AddComponentData(villager, new Needs
            {
                Health = 100f,
                MaxHealth = 100f,
                Energy = initialEnergy,
                Morale = 1000f
            });

            _entityManager.AddComponentData(villager, new JobState
            {
                Type = JobType.Gatherer,
                Phase = JobPhase.Idle, // Idle
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.None
            });

            // Create time state
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(GTimeState));

            // Run system
            var needsSystem = _world.GetOrCreateSystem<VillagerNeedsSystem>();
            _world.Update();

            // Verify energy decayed less than when working
            var needs = _entityManager.GetComponentData<Needs>(villager);
            var energyLost = initialEnergy - needs.Energy;
            Assert.Less(energyLost, 1f, "Energy should decay slowly when idle");
        }

        [Test]
        public void VillagerNeeds_MoraleDecaysOverTime()
        {
            // Create test villager with needs
            var villager = _entityManager.CreateEntity();
            var initialMorale = 1000f;
            _entityManager.AddComponentData(villager, new Needs
            {
                Health = 100f,
                MaxHealth = 100f,
                Energy = 1000f,
                Morale = initialMorale
            });

            _entityManager.AddComponentData(villager, new JobState
            {
                Type = JobType.Gatherer,
                Phase = JobPhase.Idle,
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.None
            });

            // Create time state
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(GTimeState));

            // Run system
            var needsSystem = _world.GetOrCreateSystem<VillagerNeedsSystem>();
            _world.Update();

            // Verify morale decayed
            var needs = _entityManager.GetComponentData<Needs>(villager);
            Assert.Less(needs.Morale, initialMorale, "Morale should decay over time");
        }
    }
}

