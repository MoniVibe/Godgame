using Godgame.Villages;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for village AI systems (initiative, events, expansion).
    /// </summary>
    public class VillageAISystemTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld = new World("Test World");
            _entityManager = _world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void VillageComponents_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(entity, new Village
            {
                VillageId = 1,
                VillageName = new FixedString64Bytes("TestVillage")
            });

            _entityManager.AddComponentData(entity, new VillageState
            {
                CurrentState = VillageStateType.Established,
                StateEntryTick = 100u,
                SurplusThreshold = 1.2f
            });

            _entityManager.AddComponentData(entity, new VillageAlignmentState
            {
                MoralAxis = 50,
                OrderAxis = 30,
                PurityAxis = -20
            });

            _entityManager.AddComponentData(entity, new VillageInitiativeState
            {
                CurrentInitiative = 0.6f,
                InitiativeBand = 1, // Measured
                TicksUntilNextProject = 5u,
                StressLevel = 0
            });

            _entityManager.AddComponentData(entity, new VillageResourceSummary
            {
                FoodStored = 500f,
                FoodUpkeep = 200f,
                ConstructionStored = 300f,
                ConstructionUpkeep = 150f
            });

            _entityManager.AddComponentData(entity, new VillageWorshipState
            {
                AverageMorale = 70f,
                WorshipIntensity = 0.5f,
                FaithAverage = 60f,
                ManaGenerationRate = 10f,
                OutstandingPrayerCount = 3
            });

            var eventsBuffer = _entityManager.AddBuffer<VillageEvent>(entity);
            eventsBuffer.Add(new VillageEvent
            {
                EventId = 1,
                TriggerTick = 100u,
                Seed = 12345u,
                Magnitude = 0.5f,
                EventFamily = (byte)VillageEventFamily.Social,
                AffectedAxesMask = 7
            });

            Assert.IsTrue(_entityManager.HasComponent<Village>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillageState>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillageAlignmentState>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillageInitiativeState>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillageResourceSummary>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillageWorshipState>(entity));
            Assert.IsTrue(_entityManager.HasBuffer<VillageEvent>(entity));

            var events = _entityManager.GetBuffer<VillageEvent>(entity);
            Assert.AreEqual(1, events.Length);
        }

        [Test]
        public void VillageState_TransitionsCorrectly()
        {
            var entity = _entityManager.CreateEntity();
            var state = new VillageState
            {
                CurrentState = VillageStateType.Nascent,
                StateEntryTick = 0u,
                SurplusThreshold = 1.2f
            };

            _entityManager.AddComponentData(entity, state);

            // Transition to Established
            state.CurrentState = VillageStateType.Established;
            state.StateEntryTick = 100u;
            _entityManager.SetComponentData(entity, state);

            var updatedState = _entityManager.GetComponentData<VillageState>(entity);
            Assert.AreEqual(VillageStateType.Established, updatedState.CurrentState);
            Assert.AreEqual(100u, updatedState.StateEntryTick);
        }
    }
}

