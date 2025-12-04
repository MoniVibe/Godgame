using Godgame.Buildings;
using Godgame.Effects;
using Godgame.Prayer;
using Godgame.Relations;
using Godgame.Resources;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for core systems: Prayer, Durability, Status Effects, Aggregate Piles, Relations.
    /// </summary>
    [TestFixture]
    public class CoreSystemsTests
    {
        #region Prayer Power Tests

        [Test]
        public void PrayerPowerPool_Add_RespectsCapWhenEnforced()
        {
            var pool = new PrayerPowerPool
            {
                CurrentPrayer = 49000f,
                MaxPrayer = 50000f,
                GenerationRate = 0f,
                TotalGenerated = 0f,
                TotalConsumed = 0f
            };

            float added = pool.Add(2000f);

            Assert.AreEqual(1000f, added, "Should only add up to cap");
            Assert.AreEqual(50000f, pool.CurrentPrayer, "Should be at max");
            Assert.AreEqual(1000f, pool.TotalGenerated, "TotalGenerated should track actual added");
        }

        [Test]
        public void PrayerPowerPool_TryConsume_SucceedsWithSufficientFunds()
        {
            var pool = PrayerPowerPool.Default;
            pool.CurrentPrayer = 5000f;

            bool success = pool.TryConsume(3000f);

            Assert.IsTrue(success);
            Assert.AreEqual(2000f, pool.CurrentPrayer);
            Assert.AreEqual(3000f, pool.TotalConsumed);
        }

        [Test]
        public void PrayerPowerPool_TryConsume_FailsWithInsufficientFunds()
        {
            var pool = PrayerPowerPool.Default;
            pool.CurrentPrayer = 500f;

            bool success = pool.TryConsume(1000f);

            Assert.IsFalse(success);
            Assert.AreEqual(500f, pool.CurrentPrayer, "Prayer should be unchanged");
        }

        [Test]
        public void PrayerGenerator_EffectiveRate_RespectsActiveFlag()
        {
            var active = PrayerGenerator.DefaultVillager;
            var inactive = PrayerGenerator.DefaultVillager;
            inactive.IsActive = false;

            Assert.AreEqual(1.0f, active.EffectiveRate);
            Assert.AreEqual(0f, inactive.EffectiveRate);
        }

        #endregion

        #region Building Durability Tests

        [Test]
        public void BuildingDurability_TakeDamage_UpdatesStatus()
        {
            var durability = BuildingDurability.Default(1000f);

            durability.TakeDamage(300f, DamageSource.Raid, 100);

            Assert.AreEqual(700f, durability.Current);
            Assert.AreEqual(DurabilityStatus.Stable, durability.Status, "70% should still be Stable");
            Assert.AreEqual(1.0f, durability.OutputMultiplier);
        }

        [Test]
        public void BuildingDurability_ThresholdTransitions_ApplyCorrectPenalties()
        {
            var durability = BuildingDurability.Default(1000f);

            // Light damage (75-50%)
            durability.TakeDamage(350f, DamageSource.Raid, 100);
            Assert.AreEqual(DurabilityStatus.LightDamage, durability.Status);
            Assert.AreEqual(0.95f, durability.OutputMultiplier, 0.001f);

            // Moderate damage (50-25%)
            durability.TakeDamage(200f, DamageSource.Raid, 101);
            Assert.AreEqual(DurabilityStatus.ModerateDamage, durability.Status);
            Assert.AreEqual(0.80f, durability.OutputMultiplier, 0.001f);

            // Critical (below 25%)
            durability.TakeDamage(250f, DamageSource.Raid, 102);
            Assert.AreEqual(DurabilityStatus.Critical, durability.Status);
            Assert.AreEqual(0.0f, durability.OutputMultiplier, 0.001f);
            Assert.IsFalse(durability.IsUsable);
        }

        [Test]
        public void BuildingDurability_Repair_RespectsSkillCap()
        {
            var durability = BuildingDurability.Default(1000f);
            durability.Current = 300f;

            // Apprentice repair (70% cap)
            durability.Repair(500f, 0.7f);

            Assert.AreEqual(700f, durability.Current, "Should cap at 70% of max");
        }

        [Test]
        public void BuildingQuality_MultipliersCorrect()
        {
            var poor = new BuildingQualityData { Quality = BuildingQuality.Poor };
            var masterwork = new BuildingQualityData { Quality = BuildingQuality.Masterwork };

            Assert.AreEqual(0.7f, poor.DurabilityMultiplier);
            Assert.AreEqual(1.5f, poor.DecayMultiplier);

            Assert.AreEqual(2.0f, masterwork.DurabilityMultiplier);
            Assert.AreEqual(0.4f, masterwork.DecayMultiplier);
        }

        #endregion

        #region Status Effect Tests

        [Test]
        public void StatusEffect_Create_SetsCorrectDefaults()
        {
            var effect = StatusEffect.Create(StatusEffectType.Blessed, 0.2f, 30f);

            Assert.AreEqual(StatusEffectType.Blessed, effect.Type);
            Assert.AreEqual(0.2f, effect.Magnitude);
            Assert.AreEqual(30f, effect.Duration);
            Assert.AreEqual(1, effect.StackCount);
            Assert.IsFalse(effect.IsExpired);
            Assert.IsFalse(effect.IsPermanent);
        }

        [Test]
        public void StatusEffect_CreateDoT_ConfiguresCorrectly()
        {
            var dot = StatusEffect.CreateDoT(StatusEffectType.Poison, 10f, 2f, 10f);

            Assert.AreEqual(StatusEffectType.Poison, dot.Type);
            Assert.AreEqual(10f, dot.Magnitude);
            Assert.AreEqual(2f, dot.TickInterval);
            Assert.AreEqual(10f, dot.Duration);
            Assert.AreEqual(5, dot.MaxStacks);
        }

        [Test]
        public void StatusEffect_CreatePermanent_HasNegativeDuration()
        {
            var perm = StatusEffect.CreatePermanent(StatusEffectType.Cursed, 0.5f);

            Assert.IsTrue(perm.IsPermanent);
            Assert.Less(perm.Duration, 0f);
        }

        [Test]
        public void StatusEffectModifiers_Default_HasNeutralValues()
        {
            var mods = StatusEffectModifiers.Default;

            Assert.AreEqual(1f, mods.SpeedMultiplier);
            Assert.AreEqual(1f, mods.DamageMultiplier);
            Assert.AreEqual(1f, mods.DamageTakenMultiplier);
            Assert.IsTrue(mods.CanMove);
            Assert.IsTrue(mods.CanAttack);
            Assert.IsTrue(mods.IsVisible);
        }

        #endregion

        #region Aggregate Pile Tests

        [Test]
        public void AggregatePile_Add_RespectsCapacity()
        {
            var typeId = new FixedString64Bytes("wood");
            var pile = AggregatePile.Create(typeId, 0, 2000f, 100);

            float accepted = pile.Add(1000f, 101);

            Assert.AreEqual(500f, accepted, "Should only accept up to capacity");
            Assert.AreEqual(2500f, pile.Amount);
            Assert.IsTrue(pile.IsFull);
        }

        [Test]
        public void AggregatePile_VisualSize_UpdatesCorrectly()
        {
            var typeId = new FixedString64Bytes("wood");
            var pile = AggregatePile.Create(typeId, 0, 50f, 100);

            Assert.AreEqual(PileVisualSize.Tiny, pile.VisualSize);

            pile.Add(100f, 101);
            Assert.AreEqual(PileVisualSize.Small, pile.VisualSize);

            pile.Add(400f, 102);
            Assert.AreEqual(PileVisualSize.Medium, pile.VisualSize);

            pile.Add(500f, 103);
            Assert.AreEqual(PileVisualSize.Large, pile.VisualSize);

            pile.Add(1500f, 104);
            Assert.AreEqual(PileVisualSize.Huge, pile.VisualSize);
        }

        [Test]
        public void AggregatePile_Remove_DecreasesAmount()
        {
            var typeId = new FixedString64Bytes("wood");
            var pile = AggregatePile.Create(typeId, 0, 500f, 100);

            float removed = pile.Remove(200f, 101);

            Assert.AreEqual(200f, removed);
            Assert.AreEqual(300f, pile.Amount);
        }

        [Test]
        public void AggregatePile_Remove_ClampsToAvailable()
        {
            var typeId = new FixedString64Bytes("wood");
            var pile = AggregatePile.Create(typeId, 0, 100f, 100);

            float removed = pile.Remove(500f, 101);

            Assert.AreEqual(100f, removed);
            Assert.AreEqual(0f, pile.Amount);
            Assert.IsTrue(pile.IsEmpty);
        }

        #endregion

        #region Relation Tests

        [Test]
        public void EntityRelation_GetTier_ReturnsCorrectTiers()
        {
            Assert.AreEqual(RelationTier.MortalEnemy, EntityRelation.GetTier(-100));
            Assert.AreEqual(RelationTier.MortalEnemy, EntityRelation.GetTier(-80));
            Assert.AreEqual(RelationTier.Hostile, EntityRelation.GetTier(-79));
            Assert.AreEqual(RelationTier.Hostile, EntityRelation.GetTier(-50));
            Assert.AreEqual(RelationTier.Unfriendly, EntityRelation.GetTier(-49));
            Assert.AreEqual(RelationTier.Neutral, EntityRelation.GetTier(0));
            Assert.AreEqual(RelationTier.Friendly, EntityRelation.GetTier(25));
            Assert.AreEqual(RelationTier.CloseFriend, EntityRelation.GetTier(50));
            Assert.AreEqual(RelationTier.Devoted, EntityRelation.GetTier(80));
            Assert.AreEqual(RelationTier.Devoted, EntityRelation.GetTier(100));
        }

        [Test]
        public void EntityRelation_ModifyRelation_ClampsToRange()
        {
            var relation = EntityRelation.Create(default, 90, MeetingContext.VillageNeighbor, 100);

            relation.ModifyRelation(50, 101);

            Assert.AreEqual(100, relation.RelationValue, "Should clamp to 100");
            Assert.AreEqual(1, relation.PositiveInteractions);
        }

        [Test]
        public void RelationCalculator_MoralAxisDelta_BothGoodPositive()
        {
            // Both good (+50 and +60)
            float delta = RelationCalculator.CalculateMoralAxisDelta(50, 60);

            Assert.Greater(delta, 0f, "Both good should be positive");
        }

        [Test]
        public void RelationCalculator_MoralAxisDelta_OppositeNegative()
        {
            // Good (+60) vs Evil (-60)
            float delta = RelationCalculator.CalculateMoralAxisDelta(60, -60);

            Assert.Less(delta, 0f, "Opposite alignments should be negative");
            Assert.Less(delta, -30f, "Should be significantly negative");
        }

        [Test]
        public void RelationCalculator_KinshipBonus_FamilyOverridesAlignment()
        {
            // Even with opposing alignments, family bonus should help
            sbyte relation = RelationCalculator.CalculateInitialRelation(
                60, 60, 60,    // Lawful Good Pure
                -60, -60, -60, // Chaotic Evil Corrupt
                0, 0, 0, 0,    // Neutral personality
                MeetingContext.VillageNeighbor,
                KinshipType.Sibling, // +60 kinship
                12345);

            // Should be higher than without kinship due to +60 sibling bonus
            Assert.Greater(relation, (sbyte)-50, "Kinship should offset alignment opposition");
        }

        [Test]
        public void RelationCalculator_GetObedienceProbability_ScalesWithRelation()
        {
            Assert.AreEqual(0.95f, RelationCalculator.GetObedienceProbability(80));
            Assert.AreEqual(0.70f, RelationCalculator.GetObedienceProbability(50));
            Assert.AreEqual(0.40f, RelationCalculator.GetObedienceProbability(20));
            Assert.AreEqual(0.10f, RelationCalculator.GetObedienceProbability(0));
            Assert.AreEqual(0f, RelationCalculator.GetObedienceProbability(-30));
        }

        #endregion

        #region Context Offset Tests

        [Test]
        public void MeetingContext_Offsets_MatchSpec()
        {
            Assert.AreEqual(0, EntityRelation.GetContextOffset(MeetingContext.VillageNeighbor));
            Assert.AreEqual(10, EntityRelation.GetContextOffset(MeetingContext.Workplace));
            Assert.AreEqual(20, EntityRelation.GetContextOffset(MeetingContext.FamilyIntroduction));
            Assert.AreEqual(15, EntityRelation.GetContextOffset(MeetingContext.CombatSameSide));
            Assert.AreEqual(-30, EntityRelation.GetContextOffset(MeetingContext.CombatOpposing));
            Assert.AreEqual(-50, EntityRelation.GetContextOffset(MeetingContext.CrimeVictim));
            Assert.AreEqual(40, EntityRelation.GetContextOffset(MeetingContext.RescueSalvation));
        }

        [Test]
        public void KinshipBonus_Values_MatchSpec()
        {
            Assert.AreEqual(80, EntityRelation.GetKinshipBonus(KinshipType.ParentChild));
            Assert.AreEqual(60, EntityRelation.GetKinshipBonus(KinshipType.Sibling));
            Assert.AreEqual(70, EntityRelation.GetKinshipBonus(KinshipType.Spouse));
            Assert.AreEqual(-40, EntityRelation.GetKinshipBonus(KinshipType.Disowned));
        }

        #endregion
    }
}

