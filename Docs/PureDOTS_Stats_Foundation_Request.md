# PureDOTS Stats Foundation Request

**Date:** 2025-01-24  
**Status:** Request for PureDOTS Implementation  
**Priority:** High  
**Requester:** Godgame Project

---

## Executive Summary

Godgame has implemented a comprehensive stat system for villagers (individuals) with runtime DOTS components. However, Godgame currently maintains its own stat components and syncs them to PureDOTS components for registry compatibility. This request asks PureDOTS to:

1. **Complete** the PureDOTS stat component definitions to match Godgame's requirements
2. **Refactor** PureDOTS components to serve as the foundation for all game projects using PureDOTS
3. **Standardize** stat component schemas across PureDOTS-compatible games

---

## Current State

### Godgame Implementation

Godgame has implemented the following stat components:

1. **VillagerAttributes** - Core attributes (Physique, Finesse, Will, Wisdom)
2. **VillagerDerivedAttributes** - Derived attributes (Strength, Agility, Intelligence)
3. **VillagerSocialStats** - Social stats (Fame, Wealth, Reputation, Glory, Renown)
4. **VillagerResistances** - Damage type resistances (8 types)
5. **VillagerModifiers** - Healing/spell modifiers
6. **VillagerNeeds** - Need tracking (Food, Rest, Sleep, GeneralHealth, Health, MaxHealth, Energy)
7. **VillagerMood** - Mood/morale tracking
8. **VillagerCombatStats** - Combat stats (Attack, Defense, Health, Stamina, Mana, etc.)

**Location:** `Assets/Scripts/Godgame/Villagers/Villager*Components.cs`

### Current PureDOTS Sync

Godgame maintains a sync system (`VillagerPureDOTSSyncSystem`) that copies data from Godgame components to PureDOTS components:

- `Godgame.Villagers.VillagerNeeds` → `PureDOTS.Runtime.Components.VillagerNeeds`
- `Godgame.Villagers.VillagerMood` → `PureDOTS.Runtime.Components.VillagerMood`
- `Godgame.Villagers.VillagerCombatStats` → `PureDOTS.Runtime.Components.VillagerCombatStats`

**Issue:** PureDOTS components may not exist or may have incomplete schemas.

---

## Requested PureDOTS Components

### 1. VillagerNeeds Component

**Current Godgame Structure:**
```csharp
public struct VillagerNeeds : IComponentData
{
    public byte Food;           // 0-100, hunger level
    public byte Rest;           // 0-100, fatigue level
    public byte Sleep;          // 0-100, sleep need
    public byte GeneralHealth;  // 0-100, overall health status
    
    // PureDOTS compatibility fields
    public float Health;        // Current health value
    public float MaxHealth;     // Maximum health value
    public float Energy;        // Energy level (0-100)
}
```

**Requested PureDOTS Structure:**
```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Need stats tracking for individual entities.
    /// Tracks hunger, fatigue, sleep, and general health.
    /// </summary>
    public struct VillagerNeeds : IComponentData
    {
        /// <summary>
        /// Hunger level (0-100, where 0 = starving, 100 = fully fed).
        /// </summary>
        public byte Food;

        /// <summary>
        /// Fatigue level (0-100, where 0 = exhausted, 100 = fully rested).
        /// </summary>
        public byte Rest;

        /// <summary>
        /// Sleep need (0-100, where 0 = sleep-deprived, 100 = fully rested).
        /// </summary>
        public byte Sleep;

        /// <summary>
        /// Overall health status (0-100, separate from combat HP).
        /// </summary>
        public byte GeneralHealth;

        /// <summary>
        /// Current health value (for combat/registry compatibility).
        /// </summary>
        public float Health;

        /// <summary>
        /// Maximum health value (for combat/registry compatibility).
        /// </summary>
        public float MaxHealth;

        /// <summary>
        /// Energy level (0-100, typically synced with Rest).
        /// </summary>
        public float Energy;
    }
}
```

**Location:** `Packages/com.moni.puredots/Runtime/Components/VillagerNeeds.cs`

---

### 2. VillagerMood Component

**Current Godgame Structure:**
```csharp
public struct VillagerMood : IComponentData
{
    public float Mood;  // 0-100, where 0 = very unhappy, 100 = very happy
}
```

**Requested PureDOTS Structure:**
```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Mood/morale stat for individual entities.
    /// Affects behavior, productivity, and social interactions.
    /// </summary>
    public struct VillagerMood : IComponentData
    {
        /// <summary>
        /// Current mood value (0-100, where 0 = very unhappy, 100 = very happy).
        /// </summary>
        public float Mood;
    }
}
```

**Location:** `Packages/com.moni.puredots/Runtime/Components/VillagerMood.cs`

---

### 3. VillagerCombatStats Component

**Current Godgame Structure:**
```csharp
public struct VillagerCombatStats : IComponentData
{
    public byte Attack;              // 0-100, to-hit chance
    public byte Defense;             // 0-100, dodge/block chance
    public float MaxHealth;          // Max HP
    public float CurrentHealth;      // Current HP
    public byte Stamina;             // 0-100, rounds before exhaustion
    public byte CurrentStamina;      // Current stamina
    public byte MaxMana;             // 0-100, max mana for magic users
    public byte CurrentMana;         // Current mana
    
    // PureDOTS compatibility fields
    public float AttackDamage;       // Attack damage value
    public float AttackSpeed;        // Attack speed value
    public Entity CurrentTarget;     // Current combat target
}
```

**Requested PureDOTS Structure:**
```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Combat stats for individual entities.
    /// Used for combat calculations and registry queries.
    /// </summary>
    public struct VillagerCombatStats : IComponentData
    {
        /// <summary>
        /// Attack damage value (for damage calculations).
        /// </summary>
        public float AttackDamage;

        /// <summary>
        /// Attack speed value (attacks per second or similar).
        /// </summary>
        public float AttackSpeed;

        /// <summary>
        /// Current combat target entity.
        /// </summary>
        public Entity CurrentTarget;
    }
}
```

**Note:** PureDOTS component focuses on registry/query needs. Game-specific combat stats (Attack, Defense, Health, Stamina, Mana) remain in game-specific components.

**Location:** `Packages/com.moni.puredots/Runtime/Components/VillagerCombatStats.cs`

---

## Additional Components (Future Consideration)

### 4. VillagerAttributes Component (Optional)

**Request:** Consider adding a minimal attributes component to PureDOTS for cross-game compatibility.

**Proposed Structure:**
```csharp
namespace PureDOTS.Runtime.Components
{
    /// <summary>
    /// Core attributes for individual entities.
    /// Minimal set for registry/query purposes.
    /// </summary>
    public struct VillagerAttributes : IComponentData
    {
        /// <summary>
        /// Physical power/strength (0-100).
        /// </summary>
        public byte Strength;

        /// <summary>
        /// Agility/dexterity (0-100).
        /// </summary>
        public byte Agility;

        /// <summary>
        /// Mental acuity/intelligence (0-100).
        /// </summary>
        public byte Intelligence;
    }
}
```

**Rationale:** Many games may want to query entities by attributes (e.g., "find all strong villagers"). Having a minimal PureDOTS component enables cross-game queries while allowing games to extend with their own attribute systems.

**Priority:** Low (can be added later if needed)

---

## Refactoring Requirements

### 1. Component Naming Consistency

**Current Issue:** PureDOTS components may use different naming conventions than Godgame expects.

**Request:** Ensure PureDOTS components match the exact structure expected by Godgame's sync system:
- Field names must match exactly
- Field types must match exactly
- Component namespace: `PureDOTS.Runtime.Components`

### 2. Burst Compatibility

**Requirement:** All PureDOTS stat components must be Burst-compatible:
- No managed types
- No string fields (use `FixedString` if needed)
- All fields must be blittable

### 3. Documentation

**Request:** Add XML documentation to all PureDOTS stat components:
- Field descriptions
- Value ranges
- Usage examples
- Relationship to game-specific components

### 4. Default Values

**Request:** Ensure PureDOTS components have sensible defaults:
- `VillagerNeeds`: Food/Rest/Sleep = 100, Health = 100, MaxHealth = 100, Energy = 100
- `VillagerMood`: Mood = 50 (neutral)
- `VillagerCombatStats`: AttackDamage = 0, AttackSpeed = 0, CurrentTarget = Entity.Null

---

## Integration Points

### Registry Sync

PureDOTS components are used by `GodgameVillagerSyncSystem` to populate registry mirror components:

```csharp
// Current usage in GodgameVillagerSyncSystem.cs
if (_needsLookup.HasComponent(entity))
{
    var needs = _needsLookup[entity];
    mirror.HealthPercent = needs.MaxHealth > 0f
        ? math.saturate(needs.Health / math.max(0.0001f, needs.MaxHealth)) * 100f
        : 0f;
    mirror.EnergyPercent = math.clamp(needs.Energy, 0f, 100f);
}
```

**Requirement:** PureDOTS components must support this usage pattern.

### Sync System

Godgame's `VillagerPureDOTSSyncSystem` syncs game-specific components to PureDOTS components:

```csharp
// Current sync pattern
var puredotsNeeds = SystemAPI.GetComponentRW<PureDOTS.Runtime.Components.VillagerNeeds>(entity);
puredotsNeeds.ValueRW.Health = needs.Health;
puredotsNeeds.ValueRW.MaxHealth = needs.MaxHealth;
puredotsNeeds.ValueRW.Energy = needs.Energy;
```

**Requirement:** PureDOTS components must support read/write access via `SystemAPI.GetComponentRW`.

---

## Testing Requirements

### Unit Tests

**Request:** PureDOTS should include unit tests for:
- Component creation and initialization
- Default value validation
- Burst compilation verification
- Memory layout verification (for performance)

### Integration Tests

**Request:** PureDOTS should include integration tests that verify:
- Components can be queried via `SystemAPI.Query`
- Components can be accessed via `ComponentLookup`
- Components work with `EntityCommandBuffer` operations
- Components are compatible with registry systems

---

## Migration Path

### Phase 1: Component Creation (Immediate)

1. Create/update PureDOTS stat components with exact schemas specified above
2. Ensure Burst compatibility
3. Add XML documentation
4. Add unit tests

### Phase 2: Godgame Integration (After Phase 1)

1. Godgame removes `VillagerPureDOTSSyncSystem` (no longer needed)
2. Godgame bakers add PureDOTS components directly
3. Godgame systems use PureDOTS components as source of truth
4. Godgame-specific components become extensions/wrappers

### Phase 3: Standardization (Future)

1. Other PureDOTS games adopt PureDOTS stat components
2. Cross-game queries become possible
3. Shared stat systems can be built in PureDOTS

---

## Files to Create/Modify in PureDOTS

### New Files

1. `Packages/com.moni.puredots/Runtime/Components/VillagerNeeds.cs`
2. `Packages/com.moni.puredots/Runtime/Components/VillagerMood.cs`
3. `Packages/com.moni.puredots/Runtime/Components/VillagerCombatStats.cs`
4. `Packages/com.moni.puredots/Tests/Runtime/Components/VillagerNeedsTests.cs`
5. `Packages/com.moni.puredots/Tests/Runtime/Components/VillagerMoodTests.cs`
6. `Packages/com.moni.puredots/Tests/Runtime/Components/VillagerCombatStatsTests.cs`

### Modified Files (if components already exist)

1. Update existing component schemas to match requested structure
2. Update documentation
3. Update tests

---

## Success Criteria

### Component Completeness

- ✅ All three components (`VillagerNeeds`, `VillagerMood`, `VillagerCombatStats`) exist in PureDOTS
- ✅ Component schemas match Godgame's expected structure exactly
- ✅ All fields have appropriate types and ranges

### Burst Compatibility

- ✅ All components compile with Burst
- ✅ No managed allocations
- ✅ Components can be used in Burst-compiled systems

### Documentation

- ✅ All components have XML documentation
- ✅ Field descriptions explain purpose and ranges
- ✅ Usage examples provided

### Testing

- ✅ Unit tests cover component creation and defaults
- ✅ Integration tests verify SystemAPI compatibility
- ✅ Tests verify registry sync compatibility

### Integration

- ✅ Godgame can remove sync system and use PureDOTS components directly
- ✅ Registry sync systems work with PureDOTS components
- ✅ No breaking changes to existing PureDOTS systems

---

## Timeline

**Priority:** High  
**Estimated Effort:** 2-4 hours  
**Dependencies:** None (PureDOTS can implement independently)

**Requested Completion:** Before next Godgame milestone (stats expansion)

---

## Contact

**Godgame Project Maintainer:** [Your Name/Team]  
**PureDOTS Maintainer:** [PureDOTS Team]  
**Related Documentation:**
- `Docs/Stats_Implementation_Summary.md` - Godgame stat implementation details
- `Docs/Stats_And_PureDOTS_Assessment.md` - Assessment of current state

---

## Appendix: Component Schema Reference

### VillagerNeeds Schema

| Field | Type | Range | Description |
|-------|------|-------|-------------|
| Food | byte | 0-100 | Hunger level (0 = starving) |
| Rest | byte | 0-100 | Fatigue level (0 = exhausted) |
| Sleep | byte | 0-100 | Sleep need (0 = sleep-deprived) |
| GeneralHealth | byte | 0-100 | Overall health status |
| Health | float | >= 0 | Current health value |
| MaxHealth | float | > 0 | Maximum health value |
| Energy | float | 0-100 | Energy level |

### VillagerMood Schema

| Field | Type | Range | Description |
|-------|------|-------|-------------|
| Mood | float | 0-100 | Mood value (0 = unhappy, 100 = happy) |

### VillagerCombatStats Schema

| Field | Type | Range | Description |
|-------|------|-------|-------------|
| AttackDamage | float | >= 0 | Attack damage value |
| AttackSpeed | float | >= 0 | Attack speed value |
| CurrentTarget | Entity | - | Current combat target entity |

---

**End of Request Document**

