using Godgame.Combat;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests
{
    [TestFixture]
    public class ProfessionFocusTests
    {
        private World _world;
        private EntityManager _entityManager;
        private Entity _workerEntity;

        [SetUp]
        public void SetUp()
        {
            _world = new World("Profession Focus Test World");
            _entityManager = _world.EntityManager;

            // Create worker entity with profession focus components
            _workerEntity = _entityManager.CreateEntity();

            // Add profession skills
            _entityManager.AddComponentData(_workerEntity, new ProfessionSkills
            {
                CraftingSkill = 50,
                GatheringSkill = 50,
                HealingSkill = 50,
                TeachingSkill = 50,
                RefiningSkill = 50,
                PrimaryProfession = ProfessionType.Blacksmith,
                SecondaryProfession = ProfessionType.None
            });

            // Add focus components
            _entityManager.AddComponentData(_workerEntity, EntityFocus.Default);
            _entityManager.AddComponentData(_workerEntity, ProfessionFocusModifiers.Default);
            _entityManager.AddBuffer<ActiveFocusAbility>(_workerEntity);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        #region Profession Ability Type Tests

        [Test]
        public void ProfessionAbilities_AreCorrectlyIdentified()
        {
            // Combat abilities should NOT be profession
            Assert.IsFalse(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.Parry));
            Assert.IsFalse(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.IgnorePain));
            Assert.IsFalse(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.ManaRegen));

            // Crafting abilities should be profession
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.MassProduction));
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.MasterworkFocus));

            // Gathering abilities should be profession
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.SpeedGather));
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.EfficientGather));

            // Healing abilities should be profession
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.MassHeal));
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.LifeClutch));

            // Teaching abilities should be profession
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.IntensiveLessons));
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.DeepTeaching));

            // Refining abilities should be profession
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.RapidRefine));
            Assert.IsTrue(FocusAbilityDefinitions.IsProfessionAbility(FocusAbilityType.PureExtraction));
        }

        [Test]
        public void CombatAbilities_AreCorrectlyIdentified()
        {
            Assert.IsTrue(FocusAbilityDefinitions.IsCombatAbility(FocusAbilityType.Parry));
            Assert.IsTrue(FocusAbilityDefinitions.IsCombatAbility(FocusAbilityType.IgnorePain));
            Assert.IsTrue(FocusAbilityDefinitions.IsCombatAbility(FocusAbilityType.ManaRegen));

            Assert.IsFalse(FocusAbilityDefinitions.IsCombatAbility(FocusAbilityType.MassProduction));
            Assert.IsFalse(FocusAbilityDefinitions.IsCombatAbility(FocusAbilityType.SpeedGather));
        }

        [Test]
        public void ProfessionAbilities_HaveCorrectArchetypes()
        {
            Assert.AreEqual(FocusArchetype.Crafting, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.MassProduction));
            Assert.AreEqual(FocusArchetype.Crafting, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.MasterworkFocus));

            Assert.AreEqual(FocusArchetype.Gathering, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.SpeedGather));
            Assert.AreEqual(FocusArchetype.Gathering, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.DeepExtraction));

            Assert.AreEqual(FocusArchetype.Healing, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.MassHeal));
            Assert.AreEqual(FocusArchetype.Healing, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.LifeClutch));

            Assert.AreEqual(FocusArchetype.Teaching, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.IntensiveLessons));
            Assert.AreEqual(FocusArchetype.Teaching, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.Eureka));

            Assert.AreEqual(FocusArchetype.Refining, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.RapidRefine));
            Assert.AreEqual(FocusArchetype.Refining, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.Transmutation));
        }

        #endregion

        #region Profession Skill Unlock Tests

        [Test]
        public void ProfessionAbility_CanUseProfessionAbility_ChecksSkillLevel()
        {
            // Tier 1 requires skill 25+
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MassProduction, 25));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MassProduction, 50));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MassProduction, 20));

            // Tier 2 requires skill 50+
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.BatchCrafting, 50));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.BatchCrafting, 75));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.BatchCrafting, 40));

            // Tier 3 requires skill 75+
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MasterworkFocus, 75));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MasterworkFocus, 100));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseProfessionAbility(FocusAbilityType.MasterworkFocus, 70));
        }

        [Test]
        public void ProfessionSkills_GetSkillForArchetype_ReturnsCorrectSkill()
        {
            var skills = new ProfessionSkills
            {
                CraftingSkill = 80,
                GatheringSkill = 60,
                HealingSkill = 40,
                TeachingSkill = 70,
                RefiningSkill = 50
            };

            Assert.AreEqual(80, skills.GetSkillForArchetype(FocusArchetype.Crafting));
            Assert.AreEqual(60, skills.GetSkillForArchetype(FocusArchetype.Gathering));
            Assert.AreEqual(40, skills.GetSkillForArchetype(FocusArchetype.Healing));
            Assert.AreEqual(70, skills.GetSkillForArchetype(FocusArchetype.Teaching));
            Assert.AreEqual(50, skills.GetSkillForArchetype(FocusArchetype.Refining));
            Assert.AreEqual(0, skills.GetSkillForArchetype(FocusArchetype.Finesse)); // Combat returns 0
        }

        [Test]
        public void ProfessionEffectiveness_ScalesWithSkill()
        {
            // At minimum (25 for tier 1) = 1.0x
            Assert.AreEqual(1f, FocusAbilityDefinitions.GetProfessionEffectivenessMultiplier(FocusAbilityType.MassProduction, 25), 0.01f);

            // At 100 skill = 1.5x (for tier 1 with min 25)
            Assert.AreEqual(1.5f, FocusAbilityDefinitions.GetProfessionEffectivenessMultiplier(FocusAbilityType.MassProduction, 100), 0.01f);

            // Below minimum = 0x
            Assert.AreEqual(0f, FocusAbilityDefinitions.GetProfessionEffectivenessMultiplier(FocusAbilityType.MassProduction, 20));
        }

        #endregion

        #region Tradeoff Tests

        [Test]
        public void CraftingTradeoffs_MassProduction_IncreasesSpeedDecreasesQuality()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.MassProduction);

            Assert.AreEqual(2f, speed);     // +100% speed
            Assert.AreEqual(0.7f, quality); // -30% quality
            Assert.AreEqual(1f, waste);     // No change to waste
        }

        [Test]
        public void CraftingTradeoffs_MasterworkFocus_DecreasesSpeedIncreasesQuality()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.MasterworkFocus);

            Assert.AreEqual(0.5f, speed);   // -50% speed
            Assert.AreEqual(1.5f, quality); // +50% quality
            Assert.AreEqual(1f, waste);     // No change to waste
        }

        [Test]
        public void GatheringTradeoffs_SpeedGather_IncreasesSpeedAndWaste()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.SpeedGather);

            Assert.AreEqual(1.5f, speed);   // +50% speed
            Assert.AreEqual(1f, quality);   // No change
            Assert.AreEqual(1.2f, waste);   // +20% waste
        }

        [Test]
        public void GatheringTradeoffs_EfficientGather_DecreasesSpeedAndWaste()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.EfficientGather);

            Assert.AreEqual(0.8f, speed);   // -20% speed
            Assert.AreEqual(1f, quality);   // No change
            Assert.AreEqual(0.7f, waste);   // -30% waste
        }

        [Test]
        public void RefiningTradeoffs_RapidRefine_IncreasesSpeedAndWaste()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.RapidRefine);

            Assert.AreEqual(1.5f, speed);   // +50% speed
            Assert.AreEqual(1f, quality);   // No change
            Assert.AreEqual(1.25f, waste);  // +25% waste
        }

        [Test]
        public void RefiningTradeoffs_PureExtraction_DecreasesSpeedAndWaste()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.PureExtraction);

            Assert.AreEqual(0.7f, speed);   // -30% speed
            Assert.AreEqual(1f, quality);   // No change
            Assert.AreEqual(0.5f, waste);   // -50% waste
        }

        [Test]
        public void TeachingTradeoffs_IntensiveLessons_DoublesSpeed()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.IntensiveLessons);

            Assert.AreEqual(2f, speed);     // 2x speed (half duration)
            Assert.AreEqual(1f, quality);
            Assert.AreEqual(1f, waste);
        }

        [Test]
        public void TeachingTradeoffs_DeepTeaching_HalvesSpeedIncreasesRetention()
        {
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.DeepTeaching);

            Assert.AreEqual(0.5f, speed);   // 0.5x speed (2x duration)
            Assert.AreEqual(1.5f, quality); // +50% retention (quality proxy)
            Assert.AreEqual(1f, waste);
        }

        #endregion

        #region Profession Focus Modifiers Tests

        [Test]
        public void ProfessionFocusModifiers_Default_HasNeutralValues()
        {
            var mods = ProfessionFocusModifiers.Default;

            Assert.AreEqual(1f, mods.QualityMultiplier);
            Assert.AreEqual(1f, mods.SpeedMultiplier);
            Assert.AreEqual(1f, mods.WasteMultiplier);
            Assert.AreEqual(1f, mods.TargetCountMultiplier);
            Assert.AreEqual(1f, mods.PerTargetEffectiveness);
            Assert.AreEqual(0f, mods.BonusChance);
            Assert.AreEqual(1f, mods.XPMultiplier);
            Assert.AreEqual(1f, mods.DurationMultiplier);
        }

        [Test]
        public void ProfessionFocusModifiers_Reset_RestoresToNeutral()
        {
            var mods = new ProfessionFocusModifiers
            {
                QualityMultiplier = 2f,
                SpeedMultiplier = 3f,
                WasteMultiplier = 0.5f,
                BonusChance = 0.5f
            };

            mods.Reset();

            Assert.AreEqual(1f, mods.QualityMultiplier);
            Assert.AreEqual(1f, mods.SpeedMultiplier);
            Assert.AreEqual(1f, mods.WasteMultiplier);
            Assert.AreEqual(0f, mods.BonusChance);
        }

        #endregion

        #region Integration Helper Tests

        [Test]
        public void ProfessionFocusIntegration_ApplyCraftingModifiers_CalculatesCorrectly()
        {
            var mods = new ProfessionFocusModifiers
            {
                QualityMultiplier = 1.5f,   // +50% quality
                SpeedMultiplier = 0.5f,     // -50% speed (not used in this calc)
                WasteMultiplier = 0.8f,     // -20% waste
                TargetCountMultiplier = 1f,
                BonusChance = 0f
            };

            var config = ProfessionFocusConfig.Default;

            var (quantity, quality, waste) = ProfessionFocusIntegration.ApplyCraftingModifiers(
                baseQuantity: 1,
                baseQuality: 60,
                baseMaterialCost: 100,
                baseWasteRate: 0.1f,
                mods,
                config,
                randomSeed: 0
            );

            Assert.AreEqual(1, quantity); // 1 * 1 = 1
            Assert.AreEqual(90, quality); // 60 * 1.5 = 90
            Assert.AreEqual(8, waste);    // 100 * 0.1 * 0.8 = 8
        }

        [Test]
        public void ProfessionFocusIntegration_ApplyGatheringModifiers_CalculatesCorrectly()
        {
            var mods = new ProfessionFocusModifiers
            {
                QualityMultiplier = 2f,     // +100% yield
                WasteMultiplier = 0.5f,     // -50% waste
                BonusChance = 0f
            };

            var (yield, waste, preserved) = ProfessionFocusIntegration.ApplyGatheringModifiers(
                baseYield: 10,
                baseWasteRate: 0.2f,
                mods,
                randomSeed: 0
            );

            Assert.AreEqual(20, yield);   // 10 * 2 = 20
            Assert.AreEqual(1, waste);    // 10 * 0.2 * 0.5 = 1
        }

        [Test]
        public void ProfessionFocusIntegration_ApplyHealingModifiers_CalculatesCorrectly()
        {
            var mods = new ProfessionFocusModifiers
            {
                QualityMultiplier = 1.5f,           // +50% healing
                TargetCountMultiplier = 3f,         // 3 targets
                PerTargetEffectiveness = 0.6f       // 60% per target
            };

            var (healAmount, targetCount) = ProfessionFocusIntegration.ApplyHealingModifiers(
                baseHealAmount: 100f,
                mods
            );

            Assert.AreEqual(90f, healAmount, 0.01f);  // 100 * 1.5 * 0.6 = 90
            Assert.AreEqual(3, targetCount);
        }

        [Test]
        public void ProfessionFocusIntegration_ApplyTeachingModifiers_CalculatesCorrectly()
        {
            var mods = new ProfessionFocusModifiers
            {
                XPMultiplier = 1.5f,        // +50% XP
                SpeedMultiplier = 2f,       // 2x speed
                DurationMultiplier = 0.5f,  // Half duration
                PerTargetEffectiveness = 1f
            };

            var (xp, duration) = ProfessionFocusIntegration.ApplyTeachingModifiers(
                baseXP: 100,
                baseDuration: 60f,
                mods
            );

            Assert.AreEqual(150, xp);                    // 100 * 1.5 = 150
            Assert.AreEqual(15f, duration, 0.01f);       // 60 * 0.5 / 2 = 15
        }

        [Test]
        public void ProfessionFocusIntegration_CalculateActionTime_AppliesSpeedModifier()
        {
            var fastMods = new ProfessionFocusModifiers { SpeedMultiplier = 2f };
            var slowMods = new ProfessionFocusModifiers { SpeedMultiplier = 0.5f };

            float fastTime = ProfessionFocusIntegration.CalculateActionTime(10f, fastMods);
            float slowTime = ProfessionFocusIntegration.CalculateActionTime(10f, slowMods);

            Assert.AreEqual(5f, fastTime, 0.01f);   // 10 / 2 = 5
            Assert.AreEqual(20f, slowTime, 0.01f);  // 10 / 0.5 = 20
        }

        #endregion

        #region Focus Cost Reduction Tests

        [Test]
        public void ProfessionFocusHelpers_CalculateFocusCost_ReducesWithSkill()
        {
            float baseCost = FocusAbilityDefinitions.GetBaseCost(FocusAbilityType.MassProduction);
            float costReduction = 0.05f; // 5% per 10 skill levels

            // At skill 0
            float cost0 = ProfessionFocusHelpers.CalculateFocusCost(FocusAbilityType.MassProduction, 0, costReduction);
            Assert.AreEqual(baseCost, cost0);

            // At skill 50 (5 * 0.05 = 0.25 reduction)
            float cost50 = ProfessionFocusHelpers.CalculateFocusCost(FocusAbilityType.MassProduction, 50, costReduction);
            Assert.AreEqual(baseCost * 0.75f, cost50, 0.01f);

            // At skill 100 (10 * 0.05 = 0.5 reduction, but capped at 0.5)
            float cost100 = ProfessionFocusHelpers.CalculateFocusCost(FocusAbilityType.MassProduction, 100, costReduction);
            Assert.AreEqual(baseCost * 0.5f, cost100, 0.01f);
        }

        #endregion

        #region Quality Calculation Tests

        [Test]
        public void ProfessionFocusHelpers_CalculateFinalQuality_ClampsToMinMax()
        {
            byte minQuality = 10;
            byte maxQuality = 100;

            // Normal case
            byte result1 = ProfessionFocusHelpers.CalculateFinalQuality(50, 1.5f, minQuality, maxQuality);
            Assert.AreEqual(75, result1);

            // Exceeds max
            byte result2 = ProfessionFocusHelpers.CalculateFinalQuality(80, 1.5f, minQuality, maxQuality);
            Assert.AreEqual(100, result2); // Clamped to max

            // Below min
            byte result3 = ProfessionFocusHelpers.CalculateFinalQuality(20, 0.3f, minQuality, maxQuality);
            Assert.AreEqual(10, result3); // Clamped to min
        }

        #endregion

        #region Profession Type Tests

        [Test]
        public void ProfessionType_ToFocusArchetype_MapsCorrectly()
        {
            // Crafting professions
            Assert.AreEqual(FocusArchetype.Crafting, ProfessionType.Blacksmith.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Crafting, ProfessionType.Carpenter.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Crafting, ProfessionType.Armorer.ToFocusArchetype());

            // Gathering professions
            Assert.AreEqual(FocusArchetype.Gathering, ProfessionType.Miner.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Gathering, ProfessionType.Woodcutter.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Gathering, ProfessionType.Hunter.ToFocusArchetype());

            // Healing professions
            Assert.AreEqual(FocusArchetype.Healing, ProfessionType.Healer.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Healing, ProfessionType.Medic.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Healing, ProfessionType.Priest.ToFocusArchetype());

            // Teaching professions
            Assert.AreEqual(FocusArchetype.Teaching, ProfessionType.Scholar.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Teaching, ProfessionType.Mentor.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Teaching, ProfessionType.Researcher.ToFocusArchetype());

            // Refining professions
            Assert.AreEqual(FocusArchetype.Refining, ProfessionType.Smelter.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Refining, ProfessionType.Tanner.ToFocusArchetype());
            Assert.AreEqual(FocusArchetype.Refining, ProfessionType.Alchemist.ToFocusArchetype());
        }

        #endregion

        #region Scenario Tests

        [Test]
        public void Scenario_Blacksmith_MassProduction_ProducesMoreLowerQuality()
        {
            // A blacksmith using MassProduction should craft faster but lower quality
            var skills = new ProfessionSkills { CraftingSkill = 50 };

            // Get tradeoffs
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.MassProduction);

            // Simulate crafting 10 items
            int baseQuantity = 10;
            byte baseQuality = 70;
            float baseTime = 100f;

            int finalQuantity = (int)(baseQuantity * speed); // 20 items in same time
            byte finalQuality = (byte)(baseQuality * quality); // 49 quality

            Assert.AreEqual(20, finalQuantity);
            Assert.AreEqual(49, finalQuality);
        }

        [Test]
        public void Scenario_Blacksmith_MasterworkFocus_ProducesFewerHigherQuality()
        {
            // A blacksmith using MasterworkFocus should craft slower but higher quality
            var (speed, quality, waste) = ProfessionFocusHelpers.GetAbilityTradeoffs(FocusAbilityType.MasterworkFocus);

            int baseQuantity = 10;
            byte baseQuality = 70;

            // In the same time, produces half the items
            int finalQuantity = (int)(baseQuantity * speed); // 5 items
            byte finalQuality = (byte)math.min(baseQuality * quality, 100); // 100 quality (capped)

            Assert.AreEqual(5, finalQuantity);
            Assert.AreEqual(100, finalQuality);
        }

        [Test]
        public void Scenario_Healer_MassHeal_HealsManyAtReducedEffectiveness()
        {
            // A healer using MassHeal heals multiple targets but at reduced effectiveness
            var mods = new ProfessionFocusModifiers
            {
                TargetCountMultiplier = 3f,
                PerTargetEffectiveness = 0.5f,
                QualityMultiplier = 1f
            };

            var (healPerTarget, targetCount) = ProfessionFocusIntegration.ApplyHealingModifiers(100f, mods);

            // Heals 3 targets for 50 HP each
            Assert.AreEqual(3, targetCount);
            Assert.AreEqual(50f, healPerTarget, 0.01f);

            // Total healing = 150, but spread across 3 targets
            float totalHealing = healPerTarget * targetCount;
            Assert.AreEqual(150f, totalHealing, 0.01f);
        }

        [Test]
        public void Scenario_Healer_IntensiveCare_SingleTargetMaxHealing()
        {
            // A healer using IntensiveCare focuses on one target for maximum healing
            var mods = new ProfessionFocusModifiers
            {
                TargetCountMultiplier = 1f,
                PerTargetEffectiveness = 1f,
                QualityMultiplier = 2f // +100% healing
            };

            var (healAmount, targetCount) = ProfessionFocusIntegration.ApplyHealingModifiers(100f, mods);

            Assert.AreEqual(1, targetCount);
            Assert.AreEqual(200f, healAmount, 0.01f);
        }

        [Test]
        public void Scenario_Gatherer_SpeedVsEfficiency_TradeoffComparison()
        {
            // Compare SpeedGather vs EfficientGather over 100 seconds
            float baseYieldPerSecond = 1f;
            float baseWasteRate = 0.2f;
            float duration = 100f;

            // Speed Gather: +50% speed, +20% waste
            var speedMods = new ProfessionFocusModifiers
            {
                SpeedMultiplier = 1.5f,
                WasteMultiplier = 1.2f,
                QualityMultiplier = 1f,
                BonusChance = 0f
            };

            // Efficient Gather: -20% speed, -30% waste
            var efficientMods = new ProfessionFocusModifiers
            {
                SpeedMultiplier = 0.8f,
                WasteMultiplier = 0.7f,
                QualityMultiplier = 1f,
                BonusChance = 0f
            };

            // Speed gather: 150 gathered, 36 waste (24%), net 114
            float speedGathered = baseYieldPerSecond * duration * speedMods.SpeedMultiplier;
            float speedWaste = speedGathered * baseWasteRate * speedMods.WasteMultiplier;
            float speedNet = speedGathered - speedWaste;

            // Efficient gather: 80 gathered, 11.2 waste (14%), net 68.8
            float efficientGathered = baseYieldPerSecond * duration * efficientMods.SpeedMultiplier;
            float efficientWaste = efficientGathered * baseWasteRate * efficientMods.WasteMultiplier;
            float efficientNet = efficientGathered - efficientWaste;

            Assert.Greater(speedNet, efficientNet); // Speed gather nets more total
            Assert.Less(speedWaste / speedGathered, efficientWaste / efficientGathered + 0.1f); // But higher waste rate
        }

        #endregion
    }
}

