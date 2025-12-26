using Godgame.Villagers;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for VillagerBehavior traits and VillagerGrudge decay logic.
    /// </summary>
    [TestFixture]
    public class VillagerBehaviorTests
    {
        #region Behavior Trait Tests

        [Test]
        public void VillagerBehavior_Neutral_HasZeroScores()
        {
            var behavior = VillagerBehavior.Neutral;

            Assert.AreEqual(0f, behavior.VengefulScore, "Neutral should have 0 vengeful score");
            Assert.AreEqual(0f, behavior.BoldScore, "Neutral should have 0 bold score");
            Assert.AreEqual(0f, behavior.PatienceScore, "Neutral should have 0 patience score");
            Assert.AreEqual(0f, behavior.InitiativeModifier, "Neutral should have 0 initiative modifier");
        }

        [Test]
        public void VillagerBehavior_Random_StaysWithinRange()
        {
            var random = new Unity.Mathematics.Random(12345u);
            var range = 60f;

            for (int i = 0; i < 100; i++)
            {
                var behavior = VillagerBehavior.Random(ref random, range);

                Assert.GreaterOrEqual(behavior.VengefulScore, -range, "Vengeful should be >= -range");
                Assert.LessOrEqual(behavior.VengefulScore, range, "Vengeful should be <= range");
                Assert.GreaterOrEqual(behavior.BoldScore, -range, "Bold should be >= -range");
                Assert.LessOrEqual(behavior.BoldScore, range, "Bold should be <= range");
                Assert.GreaterOrEqual(behavior.PatienceScore, -range, "Patience should be >= -range");
                Assert.LessOrEqual(behavior.PatienceScore, range, "Patience should be <= range");
            }
        }

        [Test]
        public void VillagerBehavior_InitiativeModifier_ScalesWithBoldScore()
        {
            var boldBehavior = new VillagerBehavior { BoldScore = 100f };
            boldBehavior.RecalculateInitiative();

            var cravenBehavior = new VillagerBehavior { BoldScore = -100f };
            cravenBehavior.RecalculateInitiative();

            var neutralBehavior = new VillagerBehavior { BoldScore = 0f };
            neutralBehavior.RecalculateInitiative();

            Assert.Greater(boldBehavior.InitiativeModifier, 0f, "Bold should have positive initiative");
            Assert.Less(cravenBehavior.InitiativeModifier, 0f, "Craven should have negative initiative");
            Assert.AreEqual(0f, neutralBehavior.InitiativeModifier, "Neutral should have zero initiative");
        }

        [Test]
        public void VillagerBehavior_GrudgeDecayRate_VariesByVengefulScore()
        {
            var forgiving = new VillagerBehavior { VengefulScore = -100f };
            var neutral = new VillagerBehavior { VengefulScore = 0f };
            var vengeful = new VillagerBehavior { VengefulScore = 100f };

            var forgivingRate = forgiving.GetGrudgeDecayRatePerDay();
            var neutralRate = neutral.GetGrudgeDecayRatePerDay();
            var vengefulRate = vengeful.GetGrudgeDecayRatePerDay();

            // Note: Vengeful has higher decay rate, but starts with higher intensity
            // Forgiving has lower rate but also lower starting intensity
            Assert.AreEqual(0f, forgivingRate, 0.001f, "Max forgiving should have ~0 decay");
            Assert.AreEqual(1f, neutralRate, 0.001f, "Neutral should have 1.0 decay/day");
            Assert.AreEqual(2f, vengefulRate, 0.001f, "Max vengeful should have 2.0 decay/day");
        }

        [Test]
        public void VillagerBehavior_GrudgeIntensityMultiplier_DampenedForForgiving()
        {
            var forgiving = new VillagerBehavior { VengefulScore = -100f };
            var neutral = new VillagerBehavior { VengefulScore = 0f };
            var vengeful = new VillagerBehavior { VengefulScore = 100f };

            var forgivingMult = forgiving.GetGrudgeIntensityMultiplier();
            var neutralMult = neutral.GetGrudgeIntensityMultiplier();
            var vengefulMult = vengeful.GetGrudgeIntensityMultiplier();

            Assert.AreEqual(0.3f, forgivingMult, 0.01f, "Max forgiving should dampen to 30%");
            Assert.AreEqual(1f, neutralMult, 0.01f, "Neutral should have full intensity");
            Assert.AreEqual(1f, vengefulMult, 0.01f, "Vengeful should have full intensity");
        }

        #endregion

        #region Combat Behavior Tests

        [Test]
        public void VillagerCombatBehavior_FromBehavior_BoldGetsOffensiveModifiers()
        {
            var bold = new VillagerBehavior { BoldScore = 100f };
            var combat = VillagerCombatBehavior.FromBehavior(in bold);

            Assert.Greater(combat.EngageRangeModifier, 1f, "Bold should have extended engage range");
            Assert.Less(combat.RetreatThreshold, 45, "Bold should retreat at lower HP");
            Assert.Greater(combat.DamageModifier, 1f, "Bold should deal more damage");
            Assert.Greater(combat.MoraleAura, 0, "Bold should boost nearby morale");
        }

        [Test]
        public void VillagerCombatBehavior_FromBehavior_CravenGetsDefensiveModifiers()
        {
            var craven = new VillagerBehavior { BoldScore = -100f };
            var combat = VillagerCombatBehavior.FromBehavior(in craven);

            Assert.Less(combat.EngageRangeModifier, 1f, "Craven should have reduced engage range");
            Assert.Greater(combat.RetreatThreshold, 45, "Craven should retreat at higher HP");
            Assert.Less(combat.DamageModifier, 1f, "Craven should deal less damage");
            Assert.Greater(combat.DodgeChanceModifier, 0f, "Craven should have better dodge");
            Assert.Less(combat.MoraleAura, 0, "Craven should lower nearby morale");
        }

        #endregion

        #region Grudge Tests

        [Test]
        public void VillagerGrudge_BaseSeverity_ScalesByOffenseType()
        {
            var familyKilled = VillagerGrudge.GetBaseSeverity(GrudgeOffenseType.KilledFamily);
            var theft = VillagerGrudge.GetBaseSeverity(GrudgeOffenseType.StolenProperty);
            var slander = VillagerGrudge.GetBaseSeverity(GrudgeOffenseType.Slander);

            Assert.AreEqual(100f, familyKilled, "Killing family should be max severity");
            Assert.AreEqual(40f, theft, "Theft should be moderate severity");
            Assert.AreEqual(15f, slander, "Slander should be low severity");
        }

        [Test]
        public void VillagerGrudge_Create_AppliesIntensityMultiplier()
        {
            var neutral = VillagerBehavior.Neutral;
            var forgiving = new VillagerBehavior { VengefulScore = -100f };

            var neutralGrudge = VillagerGrudge.Create(Entity.Null, GrudgeOffenseType.Assault, 0, in neutral);
            var forgivingGrudge = VillagerGrudge.Create(Entity.Null, GrudgeOffenseType.Assault, 0, in forgiving);

            // Assault has base severity 50
            Assert.AreEqual(50f, neutralGrudge.Intensity, 0.01f, "Neutral should get full intensity");
            Assert.AreEqual(15f, forgivingGrudge.Intensity, 0.01f, "Forgiving should get 30% intensity");
        }

        [Test]
        public void GrudgeSystemConfig_Default_HasReasonableValues()
        {
            var config = GrudgeSystemConfig.Default;

            Assert.Greater(config.MinIntensityThreshold, 0f, "Min threshold should be positive");
            Assert.Greater(config.MaxGrudgesPerVillager, 0, "Should allow at least some grudges");
            Assert.Greater(config.TicksPerDay, 0u, "Ticks per day should be positive");
        }

        #endregion
    }
}
