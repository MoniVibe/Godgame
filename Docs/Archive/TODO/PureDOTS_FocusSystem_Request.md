# PureDOTS Focus System (Combat Resource) Request

**Date:** 2025-11-27  
**Status:** Request for PureDOTS Migration  
**Priority:** Medium (game-agnostic system, reusable by multiple projects)  
**Requester:** Godgame Project

---

## Executive Summary

Godgame has implemented a comprehensive **Focus** combat resource system that governs ability activation, exhaustion, and profession-based tradeoffs. This system is **game-agnostic** and should be moved to `com.moni.puredots` so both Space4X and Godgame (and future projects) can consume it.

The Focus system includes:
- Core resource pool with regeneration and exhaustion mechanics
- 24+ Focus ability types across 5 archetypes (Finesse/Physique/Arcane)
- 40+ profession-specific abilities with tradeoff calculations
- Profession skill integration for crafting/gathering/healing/teaching/refining

---

## Current Implementation Location

**Godgame Reference:** `Assets/Scripts/Godgame/Combat/`

All components, systems, and helpers below are already implemented and tested in Godgame.

---

## Components to Move to PureDOTS

### 1. Core Focus Components

```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Focus pool - the core combat resource.
    /// Used for abilities, sustained effects, and mental/physical exertion.
    /// </summary>
    public struct EntityFocus : IComponentData
    {
        public float Current;           // Current focus points
        public float Max;               // Maximum focus capacity
        public float RegenRate;         // Focus regen per second
        public float ExhaustionLevel;   // 0-1, increases with overuse
        public bool InComa;             // True if focus completely depleted
    }

    /// <summary>
    /// Combat stats used for ability unlock gating.
    /// </summary>
    public struct CombatStats : IComponentData
    {
        public byte Physique;           // Physical power
        public byte Finesse;            // Speed/precision
        public byte Intelligence;       // Mental acuity
        public byte Will;               // Mental fortitude
        public byte Wisdom;             // Experience/insight
    }

    /// <summary>
    /// Global focus configuration singleton.
    /// </summary>
    public struct FocusConfig : IComponentData
    {
        public float BaseRegenRate;          // Default regen per second
        public float RestingRegenMultiplier; // Multiplier when resting
        public float ExhaustionDecayRate;    // How fast exhaustion recovers
        public float ComaThreshold;          // Focus % that triggers coma
        public float ComaRecoveryThreshold;  // Focus % to exit coma
        public float BreakdownRiskThreshold; // Exhaustion % for breakdown risk
    }
}
```

### 2. Ability Tracking Components

```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Focus ability type enum.
    /// Categorized by Finesse (1-8), Physique (9-16), Arcane (17-24).
    /// </summary>
    public enum FocusAbilityType : byte
    {
        None = 0,
        
        // Finesse abilities (1-8)
        Parry = 1,
        Dodge = 2,
        QuickStrike = 3,
        Feint = 4,
        Riposte = 5,
        Evasion = 6,
        PrecisionStrike = 7,
        BlindingSpeed = 8,
        
        // Physique abilities (9-16)
        PowerStrike = 9,
        Charge = 10,
        Cleave = 11,
        StandGround = 12,
        Endure = 13,
        Rally = 14,
        Overwhelm = 15,
        Shatter = 16,
        
        // Arcane abilities (17-24)
        SpellFocus = 17,
        Meditation = 18,
        Counterspell = 19,
        Amplify = 20,
        Shield = 21,
        Dispel = 22,
        ChannelPower = 23,
        Overcharge = 24
    }

    /// <summary>
    /// Active focus ability instance.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct ActiveFocusAbility : IBufferElementData
    {
        public FocusAbilityType Type;
        public float RemainingDuration;    // Seconds remaining
        public float DrainPerSecond;       // Focus drain rate
        public float EffectMagnitude;      // Effect multiplier
    }
}
```

### 3. State Tags

```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Tag indicating entity is in focus coma (completely depleted).
    /// </summary>
    public struct FocusComaTag : IComponentData { }
    
    /// <summary>
    /// Tag indicating entity is at risk of focus breakdown.
    /// </summary>
    public struct FocusBreakdownRiskTag : IComponentData { }
}
```

### 4. Profession Focus Components

```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Profession type enum.
    /// 35+ professions across multiple archetypes.
    /// </summary>
    public enum ProfessionType : byte
    {
        None = 0,
        
        // Crafting professions
        Blacksmith = 1, Armorer = 2, Weaponsmith = 3, Jeweler = 4,
        Carpenter = 5, Mason = 6, Tailor = 7, Leatherworker = 8,
        Alchemist = 9, Enchanter = 10, Scribe = 11, Glassblower = 12,
        
        // Gathering professions
        Miner = 13, Lumberjack = 14, Farmer = 15, Hunter = 16,
        Fisher = 17, Herbalist = 18, Prospector = 19, Forager = 20,
        
        // Service professions
        Healer = 21, Priest = 22, Surgeon = 23, Apothecary = 24,
        Scholar = 25, Teacher = 26, Sage = 27, Librarian = 28,
        
        // Refining professions
        Smelter = 29, Miller = 30, Brewer = 31, Cook = 32,
        Tanner = 33, Dyer = 34, Refiner = 35
    }

    /// <summary>
    /// Skill levels for profession archetypes.
    /// </summary>
    public struct ProfessionSkills : IComponentData
    {
        public byte CraftingSkill;       // 0-100
        public byte GatheringSkill;      // 0-100
        public byte HealingSkill;        // 0-100
        public byte TeachingSkill;       // 0-100
        public byte RefiningSkill;       // 0-100
    }

    /// <summary>
    /// Active profession focus modifiers.
    /// Applied based on active abilities.
    /// </summary>
    public struct ProfessionFocusModifiers : IComponentData
    {
        public float QualityMultiplier;      // Output quality boost
        public float SpeedMultiplier;        // Work speed boost
        public float WasteReduction;         // Material waste reduction
        public byte TargetCountBonus;        // Additional targets (healing/teaching)
        public float BonusChance;            // Lucky bonus chance
    }

    /// <summary>
    /// Global profession focus settings.
    /// </summary>
    public struct ProfessionFocusConfig : IComponentData
    {
        public float BaseQualityMultiplier;
        public float BaseSpeedMultiplier;
        public float BaseWasteRate;
        public float SkillQualityFactor;     // Skill contribution to quality
        public float SkillSpeedFactor;       // Skill contribution to speed
    }
}
```

### 5. Profession Ability Types

```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Profession-specific ability types.
    /// 40+ abilities across 5 archetypes.
    /// </summary>
    public enum ProfessionAbilityType : byte
    {
        None = 0,
        
        // Crafting abilities (70-89)
        MassProduction = 70,
        MasterworkFocus = 71,
        BatchCrafting = 72,
        PrecisionWork = 73,
        Reinforce = 74,
        EfficientCrafting = 75,
        Inspiration = 76,
        StudiedCrafting = 77,
        
        // Gathering abilities (90-109)
        SpeedGather = 90,
        EfficientGather = 91,
        GatherOverdrive = 92,
        ResourceSense = 93,
        DeepExtraction = 94,
        LuckyFind = 95,
        SustainableHarvest = 96,
        Multitasking = 97,
        
        // Healing abilities (110-129)
        MassHeal = 110,
        LifeClutch = 111,
        PurifyingFocus = 112,
        RegenerationAura = 113,
        IntensiveCare = 114,
        MiracleHealing = 115,
        LifeTransfer = 116,
        Triage = 117,
        
        // Teaching abilities (130-149)
        IntensiveLessons = 130,
        DeepTeaching = 131,
        GroupInstruction = 132,
        InspiringPresence = 133,
        HandsOnTraining = 134,
        TalentDiscovery = 135,
        MindLink = 136,
        Eureka = 137,
        
        // Refining abilities (150-169)
        RapidRefine = 150,
        PureExtraction = 151,
        BatchRefine = 152,
        QualityControl = 153,
        Reclamation = 154,
        Transmutation = 155,
        Synthesis = 156,
        GentleProcessing = 157
    }
}
```

---

## Systems to Move to PureDOTS

### 1. FocusRegenSystem

```csharp
/// <summary>
/// Regenerates focus over time.
/// Applies resting multiplier when applicable.
/// </summary>
[BurstCompile]
public partial struct FocusRegenSystem : ISystem { }
```

**Responsibilities:**
- Regenerate focus based on `RegenRate`
- Apply `RestingRegenMultiplier` when entity has resting state
- Clamp focus to `0..Max`
- Decay exhaustion over time

### 2. FocusAbilitySystem

```csharp
/// <summary>
/// Activates abilities, drains focus, manages durations.
/// </summary>
[BurstCompile]
public partial struct FocusAbilitySystem : ISystem { }
```

**Responsibilities:**
- Activate abilities from request buffer
- Check focus availability before activation
- Drain focus per second for sustained abilities
- Track and decrement durations
- Remove expired abilities

### 3. FocusExhaustionSystem

```csharp
/// <summary>
/// Tracks exhaustion, triggers breakdown/coma.
/// </summary>
[BurstCompile]
public partial struct FocusExhaustionSystem : ISystem { }
```

**Responsibilities:**
- Accumulate exhaustion from ability usage
- Add/remove `FocusBreakdownRiskTag` based on threshold
- Trigger coma when focus depletes completely
- Add/remove `FocusComaTag` as appropriate

---

## Helper Utilities to Move

### FocusEffectHelpers

```csharp
/// <summary>
/// Query active abilities for combat modifiers.
/// </summary>
public static class FocusEffectHelpers
{
    // Get attack speed modifier from active abilities
    public static float GetAttackSpeedModifier(DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Get dodge chance modifier
    public static float GetDodgeChanceModifier(DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Get critical hit modifier
    public static float GetCriticalModifier(DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Get spell power modifier
    public static float GetSpellPowerModifier(DynamicBuffer<ActiveFocusAbility> abilities);
}
```

### FocusExhaustionHelpers

```csharp
/// <summary>
/// Query exhaustion state for effectiveness/cost multipliers.
/// </summary>
public static class FocusExhaustionHelpers
{
    // Get effectiveness reduction from exhaustion
    public static float GetEffectivenessMultiplier(float exhaustionLevel);
    
    // Get focus cost multiplier from exhaustion
    public static float GetFocusCostMultiplier(float exhaustionLevel);
    
    // Check if at breakdown risk
    public static bool IsAtBreakdownRisk(float exhaustionLevel, FocusConfig config);
}
```

### ProfessionFocusHelpers

```csharp
/// <summary>
/// Tradeoff calculations, quality/speed/waste formulas.
/// </summary>
public static class ProfessionFocusHelpers
{
    // Calculate output quality with skill + ability modifiers
    public static float CalculateQuality(ProfessionSkills skills, ProfessionFocusModifiers mods, 
                                          ProfessionFocusConfig config, ProfessionType type);
    
    // Calculate work speed
    public static float CalculateSpeed(ProfessionSkills skills, ProfessionFocusModifiers mods,
                                        ProfessionFocusConfig config, ProfessionType type);
    
    // Calculate material waste rate
    public static float CalculateWaste(ProfessionSkills skills, ProfessionFocusModifiers mods,
                                        ProfessionFocusConfig config, ProfessionType type);
}
```

### ProfessionFocusIntegration

```csharp
/// <summary>
/// Static helpers for job systems integration.
/// </summary>
public static class ProfessionFocusIntegration
{
    // Apply crafting ability effects
    public static void ApplyCraftingEffects(ref ProfessionFocusModifiers mods, 
                                             DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Apply gathering ability effects
    public static void ApplyGatheringEffects(ref ProfessionFocusModifiers mods,
                                              DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Apply healing ability effects
    public static void ApplyHealingEffects(ref ProfessionFocusModifiers mods,
                                            DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Apply teaching ability effects
    public static void ApplyTeachingEffects(ref ProfessionFocusModifiers mods,
                                             DynamicBuffer<ActiveFocusAbility> abilities);
    
    // Apply refining ability effects
    public static void ApplyRefiningEffects(ref ProfessionFocusModifiers mods,
                                             DynamicBuffer<ActiveFocusAbility> abilities);
}
```

---

## Ability Definitions (Static Data)

### FocusAbilityDefinitions

Static data for all 24 focus abilities and 40 profession abilities:

| Ability | Focus Cost | Duration | Drain/sec | Unlock Requirement |
|---------|------------|----------|-----------|-------------------|
| Parry | 10 | 5s | 2 | Finesse 20 |
| Dodge | 15 | 3s | 3 | Finesse 25 |
| PowerStrike | 25 | Instant | - | Physique 30 |
| SpellFocus | 20 | 10s | 4 | Intelligence 25 |
| MassProduction | 30 | 60s | 1 | Crafting 50 |
| SpeedGather | 15 | 30s | 0.5 | Gathering 30 |
| MassHeal | 40 | Instant | - | Healing 60 |
| ... | ... | ... | ... | ... |

Full definitions in `FocusAbilityDefinitions.cs`.

---

## Related Combat Utilities (Also Game-Agnostic)

These systems are related and may also be candidates for PureDOTS:

| System | Purpose | Priority |
|--------|---------|----------|
| Target selection | Priority queues, threat assessment | Medium |
| Range check utilities | Melee/ranged/AOE distance queries | Medium |
| Hit calculation | Accuracy, dodge, armor, damage rolls | Medium |
| Projectile/AOE resolution | Burst vs sustained effects | Low |
| Combat state machine | Engaged/fleeing/stunned/recovering | Low |

---

## What Stays in Godgame

These are game-specific and should remain local:

- `FocusAuthoring` / `CombatArchetypeAuthoring` — Presets for Warrior/Rogue/Mage archetypes
- Godgame-specific profession definitions and ability balancing
- UI/HUD bindings for focus display
- Godgame-specific combat integration hooks

---

## Testing Requirements

### Unit Tests

```csharp
// Focus regen test
[Test] public void FocusRegen_IncreasesOverTime() { }
[Test] public void FocusRegen_AppliesRestingMultiplier() { }
[Test] public void FocusRegen_ClampsToMax() { }

// Exhaustion tests
[Test] public void Exhaustion_TriggersBreakdownRisk() { }
[Test] public void Exhaustion_TriggersComa() { }
[Test] public void Exhaustion_DecaysOverTime() { }

// Ability tests
[Test] public void Ability_DrainsFocusPerSecond() { }
[Test] public void Ability_ExpiresAfterDuration() { }
[Test] public void Ability_FailsWithInsufficientFocus() { }

// Profession tests
[Test] public void Profession_QualityScalesWithSkill() { }
[Test] public void Profession_SpeedAffectedByAbilities() { }
[Test] public void Profession_WasteReducedBySkill() { }
```

---

## Migration Path

### Phase 1: Copy to PureDOTS

1. Copy all components to `Packages/com.moni.puredots/Runtime/Components/Focus/`
2. Copy all systems to `Packages/com.moni.puredots/Runtime/Systems/Focus/`
3. Copy helpers to `Packages/com.moni.puredots/Runtime/Helpers/`
4. Update namespaces from `Godgame.Combat` to `PureDOTS.Runtime.Components`

### Phase 2: Godgame Migration

1. Remove duplicate components from Godgame
2. Update `using` statements to reference PureDOTS
3. Keep only game-specific authoring and integration

### Phase 3: Space4X Adoption

1. Space4X can now reference the same focus system
2. Unified ability definitions across games
3. Cross-game balancing becomes possible

---

## File Structure in PureDOTS

```
Packages/com.moni.puredots/
├── Runtime/
│   ├── Components/
│   │   └── Focus/
│   │       ├── EntityFocus.cs
│   │       ├── CombatStats.cs
│   │       ├── FocusConfig.cs
│   │       ├── FocusAbilityType.cs
│   │       ├── ActiveFocusAbility.cs
│   │       ├── FocusComaTag.cs
│   │       ├── FocusBreakdownRiskTag.cs
│   │       ├── ProfessionType.cs
│   │       ├── ProfessionSkills.cs
│   │       ├── ProfessionFocusModifiers.cs
│   │       ├── ProfessionFocusConfig.cs
│   │       └── ProfessionAbilityType.cs
│   ├── Systems/
│   │   └── Focus/
│   │       ├── FocusRegenSystem.cs
│   │       ├── FocusAbilitySystem.cs
│   │       └── FocusExhaustionSystem.cs
│   └── Helpers/
│       ├── FocusEffectHelpers.cs
│       ├── FocusExhaustionHelpers.cs
│       ├── ProfessionFocusHelpers.cs
│       └── ProfessionFocusIntegration.cs
└── Tests/
    └── Runtime/
        └── Focus/
            ├── FocusRegenTests.cs
            ├── FocusAbilityTests.cs
            ├── FocusExhaustionTests.cs
            └── ProfessionFocusTests.cs
```

---

## Success Criteria

- [ ] All focus components exist in PureDOTS
- [ ] All focus systems compile with Burst
- [ ] All profession abilities defined and tested
- [ ] Helper utilities are Burst-compatible
- [ ] Godgame can remove local duplicates and reference PureDOTS
- [ ] Unit tests cover all major paths
- [ ] Documentation includes usage examples

---

## Timeline

**Priority:** Medium  
**Estimated Effort:** 4-6 hours  
**Dependencies:** None (Godgame implementation is complete)

**Blocking:** Nothing - Godgame can continue using local implementation until migration.

---

## Related Documentation

- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Integration tracking
- `Docs/TruthSources_Inventory.md` - Truth source definitions

---

**End of Request Document**

