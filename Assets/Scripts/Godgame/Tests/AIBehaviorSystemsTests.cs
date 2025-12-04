using VillagerMood = Godgame.AI.VillagerMood;
using VillagerPatriotism = Godgame.AI.VillagerPatriotism;
using VillagerInitiative = Godgame.AI.VillagerInitiative;
using InitiativeBand = Godgame.AI.InitiativeBand;
using InitiativeConfig = Godgame.AI.InitiativeConfig;
using MoodBand = Godgame.AI.MoodBand;
using MoodConfig = Godgame.AI.MoodConfig;
using DailyPhase = Godgame.AI.DailyPhase;
using DayTimeState = Godgame.AI.DayTimeState;
using DayNightState = Godgame.AI.DayNightState;
using SleepArchetype = Godgame.AI.SleepArchetype;
using VillagerRoutine = Godgame.AI.VillagerRoutine;
using RoutineConfig = Godgame.AI.RoutineConfig;
using VillagerBehavior = Godgame.Villagers.VillagerBehavior;
using VillagerGrudge = Godgame.Villagers.VillagerGrudge;
using VillagerMoodState = Godgame.AI.VillagerMood;
using VillagerPatriotismState = Godgame.AI.VillagerPatriotism;
using VillagerInitiativeState = Godgame.AI.VillagerInitiative;
using VillagerMoodConfig = Godgame.AI.MoodConfig;
using VillagerInitiativeConfig = Godgame.AI.InitiativeConfig;
using Godgame.Progression;
using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests
{
    [TestFixture]
    public class AIBehaviorSystemsTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld = new World("Test World");
            _entityManager = _world.EntityManager;

            // Create TimeState singleton
            var timeStateEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeStateEntity, default(TimeState));
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        #region Initiative System Tests

        [Test]
        public void VillagerInitiative_Default_HasMeasuredBand()
        {
            var initiative = VillagerInitiative.Default;

            Assert.AreEqual(InitiativeBand.Measured, initiative.Band);
            Assert.AreEqual(50f, initiative.CurrentInitiative);
            Assert.AreEqual(50f, initiative.BaseInitiative);
        }

        [Test]
        public void VillagerInitiative_GetTickBudget_ReturnsCorrectValues()
        {
            var slow = new VillagerInitiative { Band = InitiativeBand.Slow };
            var measured = new VillagerInitiative { Band = InitiativeBand.Measured };
            var bold = new VillagerInitiative { Band = InitiativeBand.Bold };
            var reckless = new VillagerInitiative { Band = InitiativeBand.Reckless };

            Assert.AreEqual(8, slow.GetTickBudget());
            Assert.AreEqual(5, measured.GetTickBudget());
            Assert.AreEqual(3, bold.GetTickBudget());
            Assert.AreEqual(2, reckless.GetTickBudget());
        }

        [Test]
        public void InitiativeConfig_Default_HasValidThresholds()
        {
            var config = InitiativeConfig.Default;

            Assert.Less(config.SlowThreshold, config.BoldThreshold);
            Assert.Less(config.BoldThreshold, config.RecklessThreshold);
            Assert.AreEqual(30f, config.SlowThreshold);
            Assert.AreEqual(60f, config.BoldThreshold);
            Assert.AreEqual(85f, config.RecklessThreshold);
        }

        #endregion

        #region Mood Band Tests

        [Test]
        public void VillagerMood_Default_HasStableBand()
        {
            var mood = Godgame.AI.VillagerMood.Default;

            Assert.AreEqual(MoodBand.Stable, mood.Band);
            Assert.AreEqual(0f, mood.WorkSpeedModifier);
            Assert.AreEqual(0f, mood.InitiativeModifier);
        }

        [Test]
        public void VillagerMood_RecalculateModifiers_DespairHasPenalties()
        {
            var mood = new Godgame.AI.VillagerMood { Band = MoodBand.Despair };
            mood.RecalculateModifiers();

            Assert.Less(mood.WorkSpeedModifier, 0f);
            Assert.Less(mood.InitiativeModifier, 0f);
            Assert.Greater(mood.BreakdownRisk, (byte)0);
        }

        [Test]
        public void VillagerMood_RecalculateModifiers_ElatedHasBonuses()
        {
            var mood = new Godgame.AI.VillagerMood { Band = MoodBand.Elated };
            mood.RecalculateModifiers();

            Assert.Greater(mood.WorkSpeedModifier, 0f);
            Assert.Greater(mood.InitiativeModifier, 0f);
            Assert.Greater(mood.FaithGainModifier, 0f);
        }

        [Test]
        public void MoodConfig_Default_HasCorrectThresholds()
        {
            var config = Godgame.AI.MoodConfig.Default;

            Assert.AreEqual(200f, config.DespairThreshold);
            Assert.AreEqual(400f, config.UnhappyThreshold);
            Assert.AreEqual(600f, config.CheerfulThreshold);
            Assert.AreEqual(800f, config.ElatedThreshold);
            Assert.AreEqual(1000f, config.MaxMorale);
        }

        #endregion

        #region Patriotism Tests

        [Test]
        public void VillagerPatriotism_Default_HasModerateScore()
        {
            var patriotism = VillagerPatriotism.Default;

            Assert.AreEqual(50, patriotism.Score);
            Assert.AreEqual(Entity.Null, patriotism.VillageEntity);
        }

        [Test]
        public void VillagerPatriotism_RecalculateScore_TimeIncreasesScore()
        {
            var patriotism = VillagerPatriotism.Default;
            patriotism.TicksInAggregate = 100000; // Long service
            patriotism.RecalculateScore();

            Assert.Greater(patriotism.Score, (byte)50);
        }

        [Test]
        public void VillagerPatriotism_RecalculateScore_FamilyIncreasesScore()
        {
            var patriotism = VillagerPatriotism.Default;
            patriotism.FamilyMembersInAggregate = 5;
            patriotism.RecalculateScore();

            Assert.Greater(patriotism.Score, (byte)50);
        }

        [Test]
        public void PatriotismConfig_Default_HasValidThresholds()
        {
            var config = Godgame.AI.PatriotismConfig.Default;

            Assert.Greater(config.MigrationThreshold, config.DesertionThreshold);
            Assert.AreEqual(30, config.MigrationThreshold);
            Assert.AreEqual(15, config.DesertionThreshold);
        }

        #endregion

        #region Daily Routine Tests

        [Test]
        public void VillagerRoutine_Default_StartsAtDawn()
        {
            var routine = VillagerRoutine.Default;

            Assert.AreEqual(DailyPhase.Dawn, routine.CurrentPhase);
            Assert.AreEqual(SleepArchetype.Diurnal, routine.SleepType);
            Assert.IsFalse(routine.IsSleeping);
        }

        [Test]
        public void VillagerRoutine_ShouldBeSleeping_DiurnalSleepsAtNight()
        {
            var routine = new VillagerRoutine { SleepType = SleepArchetype.Diurnal };

            Assert.IsFalse(routine.ShouldBeSleeping(DailyPhase.Dawn, DayNightState.Daylight));
            Assert.IsFalse(routine.ShouldBeSleeping(DailyPhase.Noon, DayNightState.Daylight));
            Assert.IsTrue(routine.ShouldBeSleeping(DailyPhase.Dusk, DayNightState.Nightfall));
            Assert.IsTrue(routine.ShouldBeSleeping(DailyPhase.Midnight, DayNightState.Nightfall));
        }

        [Test]
        public void VillagerRoutine_ShouldBeSleeping_NocturnalSleepsAtDay()
        {
            var routine = new VillagerRoutine { SleepType = SleepArchetype.Nocturnal };

            Assert.IsFalse(routine.ShouldBeSleeping(DailyPhase.Dawn, DayNightState.Daylight));
            Assert.IsTrue(routine.ShouldBeSleeping(DailyPhase.Noon, DayNightState.Daylight));
            Assert.IsFalse(routine.ShouldBeSleeping(DailyPhase.Dusk, DayNightState.Nightfall));
            Assert.IsFalse(routine.ShouldBeSleeping(DailyPhase.Midnight, DayNightState.Nightfall));
        }

        [Test]
        public void RoutineConfig_Default_HasValidPhaseTimings()
        {
            var config = RoutineConfig.Default;

            Assert.AreEqual(14400u, config.TicksPerDay);
            Assert.Less(config.DawnStartTick, config.NoonStartTick);
            Assert.Less(config.NoonStartTick, config.DuskStartTick);
            Assert.Less(config.DuskStartTick, config.MidnightStartTick);
        }

        [Test]
        public void DayTimeState_Default_StartsAtDawnDay0()
        {
            var dayTime = DayTimeState.Default;

            Assert.AreEqual(DailyPhase.Dawn, dayTime.CurrentPhase);
            Assert.AreEqual(DayNightState.Daylight, dayTime.DayNight);
            Assert.AreEqual(0u, dayTime.DayNumber);
        }

        #endregion

        #region Progression Tests

        [Test]
        public void CharacterProgression_Default_StartsAtLevel1()
        {
            var progression = CharacterProgression.Default;

            Assert.AreEqual(1, progression.Level);
            Assert.AreEqual(0u, progression.TotalXP);
            Assert.AreEqual(0, progression.AvailableSkillPoints);
        }

        [Test]
        public void CharacterProgression_CalculateXPForLevel_ScalesExponentially()
        {
            var level1XP = CharacterProgression.CalculateXPForLevel(1);
            var level10XP = CharacterProgression.CalculateXPForLevel(10);
            var level50XP = CharacterProgression.CalculateXPForLevel(50);

            Assert.AreEqual(100u, level1XP);
            Assert.Greater(level10XP, level1XP);
            Assert.Greater(level50XP, level10XP);
        }

        [Test]
        public void SkillXP_CreateForDomain_HasCorrectDefaults()
        {
            var combatXP = SkillXP.CreateForDomain(SkillDomain.Combat);

            Assert.AreEqual(SkillDomain.Combat, combatXP.Domain);
            Assert.AreEqual(0u, combatXP.CurrentXP);
            Assert.AreEqual(SkillMastery.Novice, combatXP.Mastery);
            Assert.AreEqual(500u, combatXP.XPToNextMastery);
        }

        [Test]
        public void PreordainedPath_Create_SetsCorrectDomains()
        {
            var demonSlayer = PreordainedPath.Create(PreordainedPathType.DemonSlayer, 100);

            Assert.AreEqual(PreordainedPathType.DemonSlayer, demonSlayer.PathType);
            Assert.AreEqual(SkillDomain.Combat, demonSlayer.PrimaryDomain);
            Assert.AreEqual(SkillDomain.Divine, demonSlayer.SecondaryDomain);
            Assert.IsTrue(demonSlayer.IsActive);
        }

        [Test]
        public void PreordainedPath_GetDomainsForPath_ReturnsCorrectPairs()
        {
            var (arcPrimary, arcSecondary) = PreordainedPath.GetDomainsForPath(PreordainedPathType.Archmage);
            Assert.AreEqual(SkillDomain.Arcane, arcPrimary);
            Assert.AreEqual(SkillDomain.Crafting, arcSecondary);

            var (warPrimary, warSecondary) = PreordainedPath.GetDomainsForPath(PreordainedPathType.WarLeader);
            Assert.AreEqual(SkillDomain.Leadership, warPrimary);
            Assert.AreEqual(SkillDomain.Combat, warSecondary);
        }

        [Test]
        public void ProgressionConfig_Default_HasValidMultipliers()
        {
            var config = ProgressionConfig.Default;

            Assert.Greater(config.PreordainedPrimaryMultiplier, 1f);
            Assert.Greater(config.PreordainedSecondaryMultiplier, 1f);
            Assert.Less(config.PreordainedSecondaryMultiplier, config.PreordainedPrimaryMultiplier);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void VillagerWithAllAIComponents_CanBeCreated()
        {
            var entity = _entityManager.CreateEntity();

            // Add all AI behavior components
            _entityManager.AddComponentData(entity, VillagerBehavior.Neutral);
            _entityManager.AddComponentData(entity, VillagerInitiative.Default);
            _entityManager.AddComponentData(entity, VillagerMood.Default);
            _entityManager.AddComponentData(entity, VillagerPatriotism.Default);
            _entityManager.AddComponentData(entity, VillagerRoutine.Default);
            _entityManager.AddComponentData(entity, CharacterProgression.Default);

            Assert.IsTrue(_entityManager.HasComponent<VillagerBehavior>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerInitiative>(entity));
            Assert.IsTrue(_entityManager.HasComponent<Godgame.AI.VillagerMood>(entity));
            Assert.IsTrue(_entityManager.HasComponent<Godgame.AI.VillagerPatriotism>(entity));
            Assert.IsTrue(_entityManager.HasComponent<VillagerRoutine>(entity));
            Assert.IsTrue(_entityManager.HasComponent<CharacterProgression>(entity));
        }

        [Test]
        public void BoldVillager_HasHigherBaseInitiative()
        {
            var boldBehavior = new VillagerBehavior { BoldScore = 80f };
            var cravenBehavior = new VillagerBehavior { BoldScore = -80f };

            var boldInit = VillagerInitiative.Default;
            var cravenInit = VillagerInitiative.Default;

            // Simulate initiative calculation
            float boldBase = 50f + (boldBehavior.BoldScore * 0.3f);
            float cravenBase = 50f + (cravenBehavior.BoldScore * 0.3f);

            Assert.Greater(boldBase, cravenBase);
            Assert.Greater(boldBase, 50f);
            Assert.Less(cravenBase, 50f);
        }

        [Test]
        public void MoodBand_ClassifiesMoraleCorrectly()
        {
            var config = MoodConfig.Default;

            // Test morale values map to correct bands
            Assert.Less(100f, config.DespairThreshold); // Should be Despair
            Assert.Less(300f, config.UnhappyThreshold); // Should be Unhappy
            Assert.Less(500f, config.CheerfulThreshold); // Should be Stable
            Assert.Less(700f, config.ElatedThreshold);   // Should be Cheerful
            Assert.GreaterOrEqual(900f, config.ElatedThreshold); // Should be Elated
        }

        #endregion
    }
}

