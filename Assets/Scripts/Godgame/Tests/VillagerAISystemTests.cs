using Godgame.Villagers;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for villager AI systems (utility scheduler, personality, initiative).
    /// </summary>
    public class VillagerAISystemTests
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
        public void VillagerPersonalityComponent_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(entity, new VillagerPersonality
            {
                VengefulScore = -70,
                BoldScore = 60
            });

            Assert.IsTrue(_entityManager.HasComponent<VillagerPersonality>(entity));
            var personality = _entityManager.GetComponentData<VillagerPersonality>(entity);
            Assert.AreEqual(-70, personality.VengefulScore);
            Assert.AreEqual(60, personality.BoldScore);
        }

        [Test]
        public void VillagerInitiativeStateComponent_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(entity, new VillagerInitiativeState
            {
                CurrentInitiative = 0.65f,
                InitiativeBand = 2, // Bold
                TicksUntilNextAction = 5,
                StressLevel = 10
            });

            Assert.IsTrue(_entityManager.HasComponent<VillagerInitiativeState>(entity));
            var initiative = _entityManager.GetComponentData<VillagerInitiativeState>(entity);
            Assert.AreEqual(0.65f, initiative.CurrentInitiative, 0.01f);
            Assert.AreEqual(2, initiative.InitiativeBand);
        }

        [Test]
        public void VillagerPatriotismComponent_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(entity, new VillagerPatriotism
            {
                Value = 75,
                DecayRate = 0.1f
            });

            Assert.IsTrue(_entityManager.HasComponent<VillagerPatriotism>(entity));
            var patriotism = _entityManager.GetComponentData<VillagerPatriotism>(entity);
            Assert.AreEqual(75, patriotism.Value);
            Assert.AreEqual(0.1f, patriotism.DecayRate, 0.01f);
        }

        [Test]
        public void VillagerAlignmentComponent_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(entity, new VillagerAlignment
            {
                MoralAxis = 60, // Good-leaning
                OrderAxis = 80, // Lawful
                PurityAxis = -40 // Corrupt-leaning
            });

            Assert.IsTrue(_entityManager.HasComponent<VillagerAlignment>(entity));
            var alignment = _entityManager.GetComponentData<VillagerAlignment>(entity);
            Assert.AreEqual(60, alignment.MoralAxis);
            Assert.AreEqual(80, alignment.OrderAxis);
            Assert.AreEqual(-40, alignment.PurityAxis);
        }

        [Test]
        public void UtilityScheduler_CalculateNeedUtility_ReturnsCorrectValues()
        {
            // Need satisfied
            var utility1 = VillagerUtilityScheduler.CalculateNeedUtility(0.8f, 0.7f, 1f);
            Assert.AreEqual(0f, utility1, 0.01f);

            // Need not satisfied
            var utility2 = VillagerUtilityScheduler.CalculateNeedUtility(0.3f, 0.7f, 1f);
            Assert.Greater(utility2, 0f);
        }

        [Test]
        public void PersonalitySystem_GenerateGrudge_CreatesValidGrudge()
        {
            var behavior = new VillagerBehavior
            {
                VengefulScore = -80f
            };
            var targetEntity = Entity.Null;

            VillagerPersonalitySystem.GenerateGrudge(
                in targetEntity,
                GrudgeOffenseType.Assault,
                100u,
                in behavior,
                out var grudge);

            Assert.Greater(grudge.Intensity, 0);
            Assert.IsFalse(grudge.IsResolved);
        }

        [Test]
        public void PersonalitySystem_GetCombatStanceModifiers_ReturnsCorrectModifiers()
        {
            var boldPersonality = new VillagerPersonality
            {
                BoldScore = 70
            };

            var modifiers = new VillagerPersonalitySystem.CombatStanceModifiers();
            VillagerPersonalitySystem.GetCombatStanceModifiers(ref boldPersonality, ref modifiers);

            Assert.Greater(modifiers.EngageRangeMultiplier, 1f); // Bold charges earlier
            Assert.Less(modifiers.RetreatThreshold, 50f); // Bold fights longer
            Assert.Greater(modifiers.DamageOutputMultiplier, 1f); // Bold deals more damage
            Assert.Greater(modifiers.MoraleAura, 0); // Bold inspires allies
        }
    }
}

