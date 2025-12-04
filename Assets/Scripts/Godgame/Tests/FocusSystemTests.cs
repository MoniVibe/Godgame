using Godgame.Combat;
using Godgame.AI;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests
{
    [TestFixture]
    public class FocusSystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private Entity _combatantEntity;

        [SetUp]
        public void SetUp()
        {
            _world = new World("Focus Test World");
            _entityManager = _world.EntityManager;

            // Create combatant entity with focus system
            _combatantEntity = _entityManager.CreateEntity();

            // Add combat stats (balanced)
            _entityManager.AddComponentData(_combatantEntity, CombatStats.Default);

            // Add focus component
            _entityManager.AddComponentData(_combatantEntity, EntityFocus.Default);

            // Add active abilities buffer
            _entityManager.AddBuffer<ActiveFocusAbility>(_combatantEntity);
        }

        [TearDown]
        public void TearDown()
        {
            _world.Dispose();
        }

        #region Focus Component Tests

        [Test]
        public void EntityFocus_Default_HasCorrectValues()
        {
            var focus = EntityFocus.Default;

            Assert.AreEqual(100f, focus.CurrentFocus);
            Assert.AreEqual(100f, focus.MaxFocus);
            Assert.AreEqual(2f, focus.BaseRegenRate);
            Assert.AreEqual(0, focus.ExhaustionLevel);
            Assert.IsFalse(focus.IsInComa);
            Assert.IsTrue(focus.CanUseFocus);
        }

        [Test]
        public void EntityFocus_FromStats_CalculatesCorrectly()
        {
            // High will = more max focus, high wisdom = faster regen
            var focus = EntityFocus.FromStats(
                will: 100,
                wisdom: 100,
                finesse: 30,
                physique: 30,
                intelligence: 30
            );

            Assert.AreEqual(150f, focus.MaxFocus); // 50 + 100*1
            Assert.AreEqual(5f, focus.BaseRegenRate, 0.01f); // 1 + 100*0.04
            Assert.AreEqual(FocusArchetype.None, focus.PrimaryArchetype); // No stat above 50
        }

        [Test]
        public void EntityFocus_FromStats_DeterminesArchetype()
        {
            // High finesse should set Finesse archetype
            var finesseFocus = EntityFocus.FromStats(80, 50, 80, 30, 30);
            Assert.AreEqual(FocusArchetype.Finesse, finesseFocus.PrimaryArchetype);

            // High physique should set Physique archetype
            var physiqueFocus = EntityFocus.FromStats(80, 50, 30, 80, 30);
            Assert.AreEqual(FocusArchetype.Physique, physiqueFocus.PrimaryArchetype);

            // High intelligence should set Arcane archetype
            var arcaneFocus = EntityFocus.FromStats(80, 50, 30, 30, 80);
            Assert.AreEqual(FocusArchetype.Arcane, arcaneFocus.PrimaryArchetype);
        }

        [Test]
        public void EntityFocus_FocusPercent_CalculatesCorrectly()
        {
            var focus = EntityFocus.Default;
            Assert.AreEqual(1f, focus.FocusPercent);

            focus.CurrentFocus = 50f;
            Assert.AreEqual(0.5f, focus.FocusPercent);

            focus.CurrentFocus = 0f;
            Assert.AreEqual(0f, focus.FocusPercent);
        }

        [Test]
        public void EntityFocus_CanUseFocus_ReturnsFalseWhenInComa()
        {
            var focus = EntityFocus.Default;
            Assert.IsTrue(focus.CanUseFocus);

            focus.IsInComa = true;
            Assert.IsFalse(focus.CanUseFocus);
        }

        [Test]
        public void EntityFocus_CanUseFocus_ReturnsFalseWhenEmpty()
        {
            var focus = EntityFocus.Default;
            focus.CurrentFocus = 0f;
            Assert.IsFalse(focus.CanUseFocus);
        }

        #endregion

        #region Combat Stats Tests

        [Test]
        public void CombatStats_Default_HasBalancedValues()
        {
            var stats = CombatStats.Default;

            Assert.AreEqual(50, stats.Physique);
            Assert.AreEqual(50, stats.Finesse);
            Assert.AreEqual(50, stats.Intelligence);
            Assert.AreEqual(50, stats.Will);
            Assert.AreEqual(50, stats.Wisdom);
        }

        #endregion

        #region Ability Definition Tests

        [Test]
        public void FocusAbilityDefinitions_GetBaseCost_ReturnsCorrectValues()
        {
            Assert.AreEqual(5f, FocusAbilityDefinitions.GetBaseCost(FocusAbilityType.Parry));
            Assert.AreEqual(10f, FocusAbilityDefinitions.GetBaseCost(FocusAbilityType.DualWieldStrike));
            Assert.AreEqual(12f, FocusAbilityDefinitions.GetBaseCost(FocusAbilityType.EmpowerCast));
            Assert.AreEqual(20f, FocusAbilityDefinitions.GetBaseCost(FocusAbilityType.CriticalHeadshot));
        }

        [Test]
        public void FocusAbilityDefinitions_GetCostType_CategorizesCorrectly()
        {
            // Burst abilities
            Assert.AreEqual(FocusCostType.Burst, FocusAbilityDefinitions.GetCostType(FocusAbilityType.SweepAttack));
            Assert.AreEqual(FocusCostType.Burst, FocusAbilityDefinitions.GetCostType(FocusAbilityType.Multicast));

            // Per-second abilities (toggles)
            Assert.AreEqual(FocusCostType.PerSecond, FocusAbilityDefinitions.GetCostType(FocusAbilityType.Parry));
            Assert.AreEqual(FocusCostType.PerSecond, FocusAbilityDefinitions.GetCostType(FocusAbilityType.DodgeBoost));

            // Per-use abilities
            Assert.AreEqual(FocusCostType.PerUse, FocusAbilityDefinitions.GetCostType(FocusAbilityType.RapidFire));
        }

        [Test]
        public void FocusAbilityDefinitions_GetArchetype_CategorizesCorrectly()
        {
            // Finesse abilities
            Assert.AreEqual(FocusArchetype.Finesse, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.Parry));
            Assert.AreEqual(FocusArchetype.Finesse, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.DualWieldStrike));
            Assert.AreEqual(FocusArchetype.Finesse, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.MultiShot));

            // Physique abilities
            Assert.AreEqual(FocusArchetype.Physique, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.IgnorePain));
            Assert.AreEqual(FocusArchetype.Physique, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.SweepAttack));
            Assert.AreEqual(FocusArchetype.Physique, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.Fortify));

            // Arcane abilities
            Assert.AreEqual(FocusArchetype.Arcane, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.ManaRegen));
            Assert.AreEqual(FocusArchetype.Arcane, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.Multicast));
            Assert.AreEqual(FocusArchetype.Arcane, FocusAbilityDefinitions.GetArchetype(FocusAbilityType.EmpowerCast));
        }

        [Test]
        public void FocusAbilityDefinitions_CanUseAbility_ChecksStatRequirements()
        {
            // Low stats - can't use tier 1 abilities
            var lowStats = new CombatStats { Physique = 30, Finesse = 30, Intelligence = 30, Will = 30, Wisdom = 30 };
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Parry, lowStats)); // Requires Finesse 50

            // Medium stats - can use tier 1 but not tier 2
            var medStats = new CombatStats { Physique = 50, Finesse = 55, Intelligence = 50, Will = 50, Wisdom = 50 };
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Parry, medStats)); // Finesse 55 >= 50
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.DualWieldStrike, medStats)); // Requires 70

            // High stats - can use tier 2
            var highStats = new CombatStats { Physique = 50, Finesse = 75, Intelligence = 50, Will = 50, Wisdom = 50 };
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.DualWieldStrike, highStats));

            // Very high stats - can use tier 3
            var eliteStats = new CombatStats { Physique = 50, Finesse = 90, Intelligence = 50, Will = 50, Wisdom = 50 };
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.CriticalHeadshot, eliteStats));
        }

        [Test]
        public void FocusAbilityDefinitions_GetEffectivenessMultiplier_ScalesWithStat()
        {
            // At minimum requirement = 1.0x
            var minStats = new CombatStats { Physique = 50, Finesse = 50, Intelligence = 50, Will = 50, Wisdom = 50 };
            Assert.AreEqual(1f, FocusAbilityDefinitions.GetEffectivenessMultiplier(FocusAbilityType.Parry, minStats), 0.01f);

            // At 100 stat = 1.5x for tier 1 abilities (requirement 50)
            var maxStats = new CombatStats { Physique = 100, Finesse = 100, Intelligence = 100, Will = 100, Wisdom = 100 };
            Assert.AreEqual(1.5f, FocusAbilityDefinitions.GetEffectivenessMultiplier(FocusAbilityType.Parry, maxStats), 0.01f);

            // Below requirement = 0x
            var lowStats = new CombatStats { Physique = 30, Finesse = 30, Intelligence = 30, Will = 30, Wisdom = 30 };
            Assert.AreEqual(0f, FocusAbilityDefinitions.GetEffectivenessMultiplier(FocusAbilityType.Parry, lowStats));
        }

        [Test]
        public void FocusAbilityDefinitions_IsToggleAbility_IdentifiesCorrectly()
        {
            // Toggle abilities (per-second cost)
            Assert.IsTrue(FocusAbilityDefinitions.IsToggleAbility(FocusAbilityType.Parry));
            Assert.IsTrue(FocusAbilityDefinitions.IsToggleAbility(FocusAbilityType.IgnorePain));
            Assert.IsTrue(FocusAbilityDefinitions.IsToggleAbility(FocusAbilityType.ManaRegen));

            // Non-toggle abilities (burst cost)
            Assert.IsFalse(FocusAbilityDefinitions.IsToggleAbility(FocusAbilityType.SweepAttack));
            Assert.IsFalse(FocusAbilityDefinitions.IsToggleAbility(FocusAbilityType.Multicast));
        }

        #endregion

        #region Focus Effect Helper Tests

        [Test]
        public void FocusEffectHelpers_HasActiveAbility_DetectsCorrectly()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            Assert.IsFalse(FocusEffectHelpers.HasActiveAbility(buffer, FocusAbilityType.Parry));

            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.Parry, EffectMagnitude = 1f });

            Assert.IsTrue(FocusEffectHelpers.HasActiveAbility(buffer, FocusAbilityType.Parry));
            Assert.IsFalse(FocusEffectHelpers.HasActiveAbility(buffer, FocusAbilityType.DodgeBoost));
        }

        [Test]
        public void FocusEffectHelpers_GetAttackSpeedMultiplier_CombinesEffects()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            // No abilities = 1x speed
            Assert.AreEqual(1f, FocusEffectHelpers.GetAttackSpeedMultiplier(buffer));

            // DualWieldStrike = 2x
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.DualWieldStrike, EffectMagnitude = 2f });
            Assert.AreEqual(2f, FocusEffectHelpers.GetAttackSpeedMultiplier(buffer));

            // DualWieldStrike + AttackSpeedBoost = 2x * 1.3 = 2.6x
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.AttackSpeedBoost, EffectMagnitude = 0.3f });
            Assert.AreEqual(2.6f, FocusEffectHelpers.GetAttackSpeedMultiplier(buffer), 0.01f);
        }

        [Test]
        public void FocusEffectHelpers_GetDamageReduction_CalculatesCorrectly()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            // No abilities = 0 reduction
            Assert.AreEqual(0f, FocusEffectHelpers.GetDamageReduction(buffer));

            // IgnorePain = 25% reduction
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.IgnorePain, EffectMagnitude = 0.25f });
            Assert.AreEqual(0.25f, FocusEffectHelpers.GetDamageReduction(buffer));

            // Fortify = 100% (caps at 1)
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.Fortify, EffectMagnitude = 1f });
            Assert.AreEqual(1f, FocusEffectHelpers.GetDamageReduction(buffer));
        }

        [Test]
        public void FocusEffectHelpers_GetSpellPowerMultiplier_StacksCorrectly()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            // No abilities = 1x power
            Assert.AreEqual(1f, FocusEffectHelpers.GetSpellPowerMultiplier(buffer));

            // EmpowerCast = 1.4x
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.EmpowerCast, EffectMagnitude = 0.4f });
            Assert.AreEqual(1.4f, FocusEffectHelpers.GetSpellPowerMultiplier(buffer), 0.01f);
        }

        [Test]
        public void FocusEffectHelpers_GetSummonCapBonus_ReturnsBonus()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            Assert.AreEqual(0, FocusEffectHelpers.GetSummonCapBonus(buffer));

            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.SummonBoost, EffectMagnitude = 2f });
            Assert.AreEqual(2, FocusEffectHelpers.GetSummonCapBonus(buffer));
        }

        #endregion

        #region Focus Exhaustion Helper Tests

        [Test]
        public void FocusExhaustionHelpers_GetEffectivenessMultiplier_ScalesWithExhaustion()
        {
            // No exhaustion = 100%
            Assert.AreEqual(1f, FocusExhaustionHelpers.GetEffectivenessMultiplier(0));
            Assert.AreEqual(1f, FocusExhaustionHelpers.GetEffectivenessMultiplier(49));

            // Half way exhausted (75) = 85%
            Assert.AreEqual(0.85f, FocusExhaustionHelpers.GetEffectivenessMultiplier(75), 0.01f);

            // Max exhaustion = 70%
            Assert.AreEqual(0.7f, FocusExhaustionHelpers.GetEffectivenessMultiplier(100), 0.01f);
        }

        [Test]
        public void FocusExhaustionHelpers_GetCostMultiplier_IncreasesWithExhaustion()
        {
            // No exhaustion = 100% cost
            Assert.AreEqual(1f, FocusExhaustionHelpers.GetCostMultiplier(0));
            Assert.AreEqual(1f, FocusExhaustionHelpers.GetCostMultiplier(49));

            // Half way exhausted (75) = 125% cost
            Assert.AreEqual(1.25f, FocusExhaustionHelpers.GetCostMultiplier(75), 0.01f);

            // Max exhaustion = 150% cost
            Assert.AreEqual(1.5f, FocusExhaustionHelpers.GetCostMultiplier(100), 0.01f);
        }

        [Test]
        public void FocusExhaustionHelpers_IsTooExhaustedForAbilities_ChecksCorrectly()
        {
            var focus = EntityFocus.Default;

            // Default state = can use abilities
            Assert.IsFalse(FocusExhaustionHelpers.IsTooExhaustedForAbilities(focus));

            // In coma = too exhausted
            focus.IsInComa = true;
            Assert.IsTrue(FocusExhaustionHelpers.IsTooExhaustedForAbilities(focus));

            // High exhaustion + low focus = too exhausted
            focus.IsInComa = false;
            focus.ExhaustionLevel = 95;
            focus.CurrentFocus = 5f;
            Assert.IsTrue(FocusExhaustionHelpers.IsTooExhaustedForAbilities(focus));

            // High exhaustion + good focus = can still use
            focus.CurrentFocus = 50f;
            Assert.IsFalse(FocusExhaustionHelpers.IsTooExhaustedForAbilities(focus));
        }

        [Test]
        public void FocusExhaustionHelpers_GetExhaustionState_ReturnsCorrectDescription()
        {
            var focus = EntityFocus.Default;

            focus.ExhaustionLevel = 0;
            Assert.AreEqual("Fresh", FocusExhaustionHelpers.GetExhaustionState(focus));

            focus.ExhaustionLevel = 30;
            Assert.AreEqual("Tired", FocusExhaustionHelpers.GetExhaustionState(focus));

            focus.ExhaustionLevel = 60;
            Assert.AreEqual("Exhausted", FocusExhaustionHelpers.GetExhaustionState(focus));

            focus.ExhaustionLevel = 85;
            focus.IsBreakdownRisk = true;
            Assert.AreEqual("Breakdown Risk", FocusExhaustionHelpers.GetExhaustionState(focus));

            focus.ExhaustionLevel = 95;
            focus.IsBreakdownRisk = false;
            Assert.AreEqual("Critical", FocusExhaustionHelpers.GetExhaustionState(focus));

            focus.IsInComa = true;
            Assert.AreEqual("COMA", FocusExhaustionHelpers.GetExhaustionState(focus));
        }

        #endregion

        #region Focus Config Tests

        [Test]
        public void FocusConfig_Default_HasReasonableValues()
        {
            var config = FocusConfig.Default;

            Assert.AreEqual(0.2f, config.ExhaustionThreshold);
            Assert.AreEqual(5f, config.ExhaustionAccumulationRate);
            Assert.AreEqual(2f, config.ExhaustionRecoveryRate);
            Assert.AreEqual(80, config.BreakdownRiskThreshold);
            Assert.AreEqual(2f, config.IdleRegenMultiplier);
            Assert.AreEqual(0.5f, config.CombatRegenMultiplier);
            Assert.AreEqual(300u, config.MinComaDuration);
        }

        #endregion

        #region Active Ability Buffer Tests

        [Test]
        public void ActiveFocusAbility_CanBeAddedToBuffer()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            Assert.AreEqual(0, buffer.Length);

            buffer.Add(new ActiveFocusAbility
            {
                AbilityType = FocusAbilityType.Parry,
                CostType = FocusCostType.PerSecond,
                DrainRate = 5f,
                RemainingDuration = 0f, // Toggle
                EffectMagnitude = 1f,
                ActivatedTick = 0,
                IsToggle = true
            });

            Assert.AreEqual(1, buffer.Length);
            Assert.AreEqual(FocusAbilityType.Parry, buffer[0].AbilityType);
        }

        [Test]
        public void ActiveFocusAbility_MultipleAbilitiesCanStack()
        {
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.Parry });
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.DodgeBoost });
            buffer.Add(new ActiveFocusAbility { AbilityType = FocusAbilityType.CriticalFocus });

            Assert.AreEqual(3, buffer.Length);

            // Verify all different abilities
            Assert.AreEqual(FocusAbilityType.Parry, buffer[0].AbilityType);
            Assert.AreEqual(FocusAbilityType.DodgeBoost, buffer[1].AbilityType);
            Assert.AreEqual(FocusAbilityType.CriticalFocus, buffer[2].AbilityType);
        }

        #endregion

        #region Integration Scenario Tests

        [Test]
        public void FocusSystem_FinesseRogue_CanUseFinesseAbilities()
        {
            // Create a rogue with high finesse
            var stats = new CombatStats
            {
                Physique = 40,
                Finesse = 80, // High
                Intelligence = 45,
                Will = 45,
                Wisdom = 55
            };

            // Can use finesse abilities
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Parry, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.DualWieldStrike, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.DodgeBoost, stats));

            // Cannot use physique abilities
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.IgnorePain, stats));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.SweepAttack, stats));

            // Cannot use arcane abilities
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.ManaRegen, stats));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Multicast, stats));
        }

        [Test]
        public void FocusSystem_PhysiqueWarrior_CanUsePhysiqueAbilities()
        {
            var stats = new CombatStats
            {
                Physique = 75, // High
                Finesse = 50,
                Intelligence = 30,
                Will = 60,
                Wisdom = 40
            };

            // Can use physique abilities
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.IgnorePain, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.SweepAttack, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.AttackSpeedBoost, stats));

            // Can use basic finesse (50)
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Parry, stats));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.DualWieldStrike, stats)); // Needs 70
        }

        [Test]
        public void FocusSystem_ArcaneMage_CanUseArcaneAbilities()
        {
            var stats = new CombatStats
            {
                Physique = 25,
                Finesse = 35,
                Intelligence = 85, // High
                Will = 50,
                Wisdom = 70
            };

            // Can use arcane abilities
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.ManaRegen, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Multicast, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.EmpowerCast, stats));
            Assert.IsTrue(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.QuickCast, stats)); // Tier 3

            // Cannot use physical abilities
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.Parry, stats));
            Assert.IsFalse(FocusAbilityDefinitions.CanUseAbility(FocusAbilityType.IgnorePain, stats));
        }

        [Test]
        public void FocusSystem_CombatScenario_CalculatesCorrectEffects()
        {
            // Setup: Rogue with active abilities
            var buffer = _entityManager.GetBuffer<ActiveFocusAbility>(_combatantEntity);

            // Activate DualWieldStrike, DodgeBoost, and CriticalFocus
            buffer.Add(new ActiveFocusAbility
            {
                AbilityType = FocusAbilityType.DualWieldStrike,
                EffectMagnitude = 2f
            });
            buffer.Add(new ActiveFocusAbility
            {
                AbilityType = FocusAbilityType.DodgeBoost,
                EffectMagnitude = 0.4f
            });
            buffer.Add(new ActiveFocusAbility
            {
                AbilityType = FocusAbilityType.CriticalFocus,
                EffectMagnitude = 0.5f
            });

            // Check effects
            Assert.AreEqual(2f, FocusEffectHelpers.GetAttackSpeedMultiplier(buffer));
            Assert.AreEqual(0.4f, FocusEffectHelpers.GetDodgeBonus(buffer));
            Assert.AreEqual(0.5f, FocusEffectHelpers.GetCritBonus(buffer));
            Assert.IsTrue(FocusEffectHelpers.CanParry(buffer) == false); // Parry not active
        }

        #endregion
    }
}

