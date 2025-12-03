using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using GodgameVillagerAttributes = Godgame.Villagers.VillagerAttributes;
using GodgameVillagerNeeds = Godgame.Villagers.VillagerNeeds;
using GodgameVillagerMood = Godgame.Villagers.VillagerMood;
using GodgameVillagerCombatStats = Godgame.Villagers.VillagerCombatStats;
using GodgameVillagerNeedsSystem = Godgame.Villagers.VillagerNeedsSystem;

namespace Godgame.Tests.Villagers
{
    /// <summary>
    /// Tests for villager stat components and systems.
    /// </summary>
    public class VillagerStatsTests
    {
        private World _world;
        private EntityManager _entityManager;
        private InitializationSystemGroup _initGroup;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World("VillagerStatsTests");
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _initGroup = _world.GetOrCreateSystemManaged<InitializationSystemGroup>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();
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
        public void VillagerStatComponents_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();

            // Add all stat components
            _entityManager.AddComponent<GodgameVillagerAttributes>(entity);
            _entityManager.AddComponent<VillagerDerivedAttributes>(entity);
            _entityManager.AddComponent<VillagerSocialStats>(entity);
            _entityManager.AddComponent<VillagerResistances>(entity);
            _entityManager.AddComponent<VillagerModifiers>(entity);
            _entityManager.AddComponent<GodgameVillagerNeeds>(entity);
            _entityManager.AddComponent<GodgameVillagerMood>(entity);
            _entityManager.AddComponent<GodgameVillagerCombatStats>(entity);

            // Verify components exist
            Assert.IsTrue(_entityManager.HasComponent<GodgameVillagerAttributes>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerDerivedAttributes>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerSocialStats>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerResistances>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerModifiers>(entity));
            Assert.IsTrue(_entityManager.HasComponent<GodgameVillagerNeeds>(entity));
            Assert.IsTrue(_entityManager.HasComponent<GodgameVillagerMood>(entity));
            Assert.IsTrue(_entityManager.HasComponent<GodgameVillagerCombatStats>(entity));
        }

        [Test]
        public void VillagerStatCalculationSystem_CalculatesDerivedStats()
        {
            var entity = _entityManager.CreateEntity();

            // Set up attributes
            _entityManager.AddComponentData(entity, new GodgameVillagerAttributes
            {
                Physique = 80,
                Finesse = 70,
                Will = 60,
                Wisdom = 50
            });

            _entityManager.AddComponentData(entity, new VillagerDerivedAttributes
            {
                Strength = 75,
                Agility = 65,
                Intelligence = 55
            });

            _entityManager.AddComponentData(entity, new GodgameVillagerCombatStats
            {
                Attack = 0, // 0 = auto-calculate
                Defense = 0, // 0 = auto-calculate
                MaxHealth = 0f, // 0 = auto-calculate
                Stamina = 0, // 0 = auto-calculate
                MaxMana = 0, // 0 = auto-calculate
                CurrentHealth = 0f,
                CurrentStamina = 0,
                CurrentMana = 0,
                AttackDamage = 0f,
                AttackSpeed = 0f,
                CurrentTarget = Entity.Null
            });

            var calcSystem = _world.GetOrCreateSystem<VillagerStatCalculationSystem>();
            UpdateSystem(calcSystem);

            var combatStats = _entityManager.GetComponentData<GodgameVillagerCombatStats>(entity);

            // Attack = Finesse × 0.7 + Strength × 0.3 = 70 × 0.7 + 75 × 0.3 = 49 + 22.5 = 71.5 ≈ 72
            Assert.GreaterOrEqual(combatStats.Attack, 70);
            Assert.LessOrEqual(combatStats.Attack, 73);

            // Defense = Finesse × 0.6 = 70 × 0.6 = 42
            Assert.GreaterOrEqual(combatStats.Defense, 40);
            Assert.LessOrEqual(combatStats.Defense, 43);

            // MaxHealth = Strength × 0.6 + Will × 0.4 + 50 = 75 × 0.6 + 60 × 0.4 + 50 = 45 + 24 + 50 = 119
            Assert.GreaterOrEqual(combatStats.MaxHealth, 115f);
            Assert.LessOrEqual(combatStats.MaxHealth, 125f);
            Assert.AreEqual(combatStats.MaxHealth, combatStats.CurrentHealth);

            // Stamina = Strength / 10 = 75 / 10 = 7.5 ≈ 8
            Assert.GreaterOrEqual(combatStats.Stamina, 7);
            Assert.LessOrEqual(combatStats.Stamina, 8);
            Assert.AreEqual(combatStats.Stamina, combatStats.CurrentStamina);

            // MaxMana = Will × 0.5 + Intelligence × 0.5 = 60 × 0.5 + 55 × 0.5 = 30 + 27.5 = 57.5 ≈ 58
            Assert.GreaterOrEqual(combatStats.MaxMana, 57);
            Assert.LessOrEqual(combatStats.MaxMana, 59);
            Assert.AreEqual(combatStats.MaxMana, combatStats.CurrentMana);
        }

        [Test]
        public void VillagerStatCalculationSystem_RespectsOverrides()
        {
            var entity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(entity, new GodgameVillagerAttributes
            {
                Physique = 50,
                Finesse = 50,
                Will = 50,
                Wisdom = 50
            });

            _entityManager.AddComponentData(entity, new VillagerDerivedAttributes
            {
                Strength = 50,
                Agility = 50,
                Intelligence = 50
            });

            // Set explicit overrides
            _entityManager.AddComponentData(entity, new GodgameVillagerCombatStats
            {
                Attack = 90, // Explicit override
                Defense = 80, // Explicit override
                MaxHealth = 200f, // Explicit override
                Stamina = 20, // Explicit override
                MaxMana = 100, // Explicit override
                CurrentHealth = 200f,
                CurrentStamina = 20,
                CurrentMana = 100,
                AttackDamage = 0f,
                AttackSpeed = 0f,
                CurrentTarget = Entity.Null
            });

            var calcSystem = _world.GetOrCreateSystem<VillagerStatCalculationSystem>();
            UpdateSystem(calcSystem);

            var combatStats = _entityManager.GetComponentData<GodgameVillagerCombatStats>(entity);

            // Overrides should be preserved
            Assert.AreEqual(90, combatStats.Attack);
            Assert.AreEqual(80, combatStats.Defense);
            Assert.AreEqual(200f, combatStats.MaxHealth);
            Assert.AreEqual(20, combatStats.Stamina);
            Assert.AreEqual(100, combatStats.MaxMana);
        }

        [Test]
        public void VillagerNeedsSystem_DecaysNeedsOverTime()
        {
            // Ensure TimeState exists (created by CoreSingletonBootstrapSystem)
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = false;
            timeState.CurrentSpeedMultiplier = 1f;
            _entityManager.SetComponentData(timeEntity, timeState);

            var entity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(entity, new GodgameVillagerNeeds
            {
                Food = 100,
                Rest = 100,
                Sleep = 100,
                GeneralHealth = 100,
                Health = 100f,
                MaxHealth = 100f,
                Energy = 100f
            });

            var needsSystem = _world.GetOrCreateSystem<GodgameVillagerNeedsSystem>();
            
            // Simulate 1 second at normal time scale
            UpdateSystem(needsSystem);

            var needs = _entityManager.GetComponentData<GodgameVillagerNeeds>(entity);

            // Needs should have decayed slightly
            Assert.Less(needs.Food, 100);
            Assert.Less(needs.Rest, 100);
            Assert.Less(needs.Sleep, 100);
            Assert.LessOrEqual(needs.GeneralHealth, 100); // May decay if food/rest hit 0

            // Health and Energy should sync
            Assert.AreEqual(needs.GeneralHealth, needs.Health, 1f); // Allow small float precision differences
            Assert.AreEqual(needs.Rest, needs.Energy, 1f);
        }

        [Test]
        public void VillagerNeedsSystem_HealthDecaysWhenStarving()
        {
            // Ensure TimeState exists
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = false;
            timeState.CurrentSpeedMultiplier = 1f;
            _entityManager.SetComponentData(timeEntity, timeState);

            var entity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(entity, new GodgameVillagerNeeds
            {
                Food = 0, // Starving
                Rest = 50,
                Sleep = 50,
                GeneralHealth = 100,
                Health = 100f,
                MaxHealth = 100f,
                Energy = 50f
            });

            var needsSystem = _world.GetOrCreateSystem<GodgameVillagerNeedsSystem>();
            
            // Simulate multiple seconds to see health decay
            for (int i = 0; i < 10; i++)
            {
                UpdateSystem(needsSystem);
            }

            var needs = _entityManager.GetComponentData<GodgameVillagerNeeds>(entity);

            // Health should have decayed because food is 0
            Assert.Less(needs.GeneralHealth, 100);
            Assert.Less(needs.Health, 100f);
        }

        private void UpdateSystem(SystemHandle handle)
        {
            // Try simulation group first (most systems are here)
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}

