# PureDOTS Burst BC1016 Fix Request

**Date:** 2025-11-27  
**Status:** Request for PureDOTS Implementation  
**Priority:** Critical (blocks Burst compilation)  
**Requester:** Godgame Project

---

## Executive Summary

Unity's Burst compiler is failing with **BC1016 errors** across 60+ PureDOTS systems due to 4 files using managed `System.String` operations inside Burst-compiled code. This causes:

1. **Compilation failures** that repeat on every domain reload
2. **Editor freezes** during background Burst compilation attempts
3. **All dependent systems** fail to compile (see affected systems list below)

**Root cause:** Constructing `FixedString*` from managed `string` or calling `.ToString()` inside Burst context.

---

## Error Details

### Current Error (BC1091) - Static Constructor Not Allowed

The previous fix attempt used `static readonly FixedString*` fields, which creates a static constructor (.cctor). Burst cannot compile static constructors!

```
Burst error BC1091: External and internal calls are not allowed inside static constructors: 
  System.Runtime.CompilerServices.RuntimeHelpers.get_OffsetToStringData()
  at Unity.Collections.FixedStringMethods.CopyFromTruncated(...)
  at Unity.Collections.FixedString32Bytes..ctor(...)
  at PureDOTS.Systems.Combat.HazardEmitFromDamageSystem..cctor()  // <-- STATIC CONSTRUCTOR!
```

### Original Error (BC1016) - Managed String Operations

```
Burst error BC1016: The managed function `System.String.get_Length(System.String* this)` is not supported
  at Unity.Collections.FixedString32Bytes..ctor(Unity.Collections.FixedString32Bytes* this, System.String source)
```

### Root Cause

Both errors stem from calling `new FixedString*(string)` in a context that Burst compiles:
- BC1016: Direct call inside `[BurstCompile]` method or job
- BC1091: Indirect call via static field initializer (creates .cctor)

---

## Files Requiring Fixes

**Current state:** Files have `static readonly FixedString*` fields that create static constructors (.cctor) - these need to be converted to instance fields initialized in `OnCreate`.

| # | File Path | .cctor Line | Usage Line | Issue | Severity |
|---|-----------|-------------|------------|-------|----------|
| 1 | `Runtime/Runtime/Registry/Aggregates/AggregateHelpers.cs` | 17 | 189 | `static readonly` field + usage in `GeneratePseudoHistory` | Critical |
| 2 | `Runtime/Systems/Combat/HazardEmitFromDamageSystem.cs` | 76 | 93 | `static readonly` field + usage in job | Critical |
| 3 | `Runtime/Systems/Ships/LifeBoatEjectorSystem.cs` | 53 | 73 | `static readonly` field + usage in job | Critical |
| 4 | `Runtime/Systems/Spells/SchoolFoundingSystem.cs` | 122 | 139 | `static readonly` field + usage in job | Critical |

**Base Path:** `C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS\Packages\com.moni.puredots\`

---

## New BC1016 occurrences (2025-11-27)

Recent Godgame runs still hit BC1016 in these files (string → FixedString in Burst paths):

1) `Runtime/Systems/Knowledge/LessonAcquisitionSystem.cs`  
   - Lines ~58-70: `OnUpdate` constructs multiple `FixedString64Bytes("strength"/"dexterity"/...)` passed into a Burst job.  
   - Fix: cache these IDs in `OnCreate` (non-Burst) or build via `Append` and pass into the job; avoid `new FixedString64Bytes(string)` inside Burst.

2) `Runtime/Systems/Combat/ProjectileEffectExecutionSystem.cs`  
   - Lines ~390-409: `ApplyStatus` builds `new FixedString64Bytes($"Status_{effectOp.StatusId}")` inside a Burst job.  
   - Fix: use a Burst-safe builder (`FixedString64Bytes buffId = default; buffId.Append("Status_"); buffId.Append(effectOp.StatusId);`) or map status IDs to precomputed FixedStrings outside Burst.

3) `Runtime/Runtime/Telemetry/Analytics/AnalyticsHelpers.cs`  
   - Lines ~304-318: `UpdateSessionStats` uses `new FixedString32Bytes("build"/"attack"/"trade"/"explore")` in a Burst path.  
   - Fix: precompute these FixedStrings outside Burst (e.g., cached in a static via `SharedStatic` initialized in `OnCreate` or a non-Burst init path) and compare against cached values in the Burst job.

All three cases still trigger BC1016 because `FixedString*(string)` calls `System.String.get_Length` under Burst. Please apply the non-Burst initialization pattern documented above.

---

## Fix Pattern

### Problem Pattern 1 (BC1016 - Breaks Burst)

```csharp
// ❌ INSIDE Burst job or [BurstCompile] method
[BurstCompile]
public void OnUpdate(ref SystemState state)
{
    // This calls System.String.get_Length internally
    var name = new FixedString32Bytes("some_constant_string");
    
    // This is a managed function
    Debug.Log(someFixedString.ToString());
}
```

### Problem Pattern 2 (BC1091 - ALSO Breaks Burst!)

**CRITICAL**: Using `static readonly FixedString*` creates a static constructor (.cctor) which Burst also cannot compile!

```csharp
// ❌ WRONG FIX - creates .cctor that Burst can't compile (BC1091)
public partial struct SomeSystem : ISystem
{
    // This creates a static constructor!
    private static readonly FixedString32Bytes s_SomeConstant = new("some_constant_string");
}
```

### Solution Pattern 1: Initialize in OnCreate (Recommended)

```csharp
public partial struct SomeSystem : ISystem
{
    private FixedString32Bytes _cachedString;
    
    // OnCreate is NOT Burst-compiled - safe to use managed strings here
    public void OnCreate(ref SystemState state)
    {
        _cachedString = new FixedString32Bytes("some_constant_string");
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Use cached value - already initialized
        var name = _cachedString;
        
        // Schedule job with cached string
        new SomeJob { Label = _cachedString }.Schedule();
    }
}

partial struct SomeJob : IJobEntity
{
    public FixedString32Bytes Label;
    
    void Execute(...)
    {
        // Use job field directly
        var label = Label;
    }
}
```

### Solution Pattern 2: Use SharedStatic (for truly static data)

```csharp
public partial struct SomeSystem : ISystem
{
    // SharedStatic is Burst-safe for static data
    private static readonly SharedStatic<FixedString32Bytes> s_CachedString = 
        SharedStatic<FixedString32Bytes>.GetOrCreate<SomeSystem, CachedStringKey>();
    private struct CachedStringKey { }
    
    public void OnCreate(ref SystemState state)
    {
        // Initialize once in OnCreate
        s_CachedString.Data = new FixedString32Bytes("some_constant_string");
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var name = s_CachedString.Data;
    }
}
```

### Solution Pattern 3: Build at runtime with Append (for dynamic strings)

```csharp
// ❌ Don't do this in Burst
var dynamic = new FixedString64Bytes($"entity_{entity.Index}");

// ✅ Build FixedString using Burst-safe append methods
FixedString64Bytes dynamic = default;
dynamic.Append((FixedString32Bytes)"entity_");  // Cast from UTF8 literal
dynamic.Append(entity.Index);
```

### For ToString() Calls

```csharp
// ❌ Don't call ToString() in Burst
Debug.Log(fixedString.ToString());

// ✅ Option 1: Move logging outside Burst/job context
// Option 2: Use FixedString directly (many APIs accept it)
// Option 3: If logging is needed, use [BurstDiscard] method
[BurstDiscard]
private static void LogNonBurst(in FixedString32Bytes msg)
{
    Debug.Log(msg.ToString());
}
```

---

## Specific Fixes Required

**IMPORTANT**: The current errors show BC1091 at static constructors (.cctor), meaning someone tried to use `static readonly FixedString` which is ALSO wrong. The `static readonly` fields trigger static constructor generation which Burst cannot compile.

### Fix 1: AggregateHelpers.cs (static constructor at line 17, usage at line 189)

**Current (WRONG - creates .cctor):**
```csharp
public static class AggregateHelpers
{
    // Line 17 - static readonly creates .cctor!
    private static readonly FixedString32Bytes s_PseudoPrefix = new("pseudo_");
    
    public static void GeneratePseudoHistory(...) // Called from Burst at line 189
    {
        // Uses s_PseudoPrefix
    }
}
```

**Required fix - move to caller's OnCreate:**
```csharp
public static class AggregateHelpers
{
    // No static readonly fields!
    
    public static void GeneratePseudoHistory(
        ref DynamicBuffer<PseudoHistoryEntry> history,
        in FixedString32Bytes prefix,  // Pass as parameter!
        ...)
    {
        // Use passed-in prefix
    }
}

// In the calling system:
public partial struct PseudoHistorySystem : ISystem
{
    private FixedString32Bytes _pseudoPrefix;
    
    public void OnCreate(ref SystemState state)
    {
        _pseudoPrefix = new FixedString32Bytes("pseudo_");
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AggregateHelpers.GeneratePseudoHistory(ref buffer, _pseudoPrefix, ...);
    }
}
```

### Fix 2: HazardEmitFromDamageSystem.cs (static constructor at line 76, usage at line 93)

**Current (WRONG):**
```csharp
public partial struct HazardEmitFromDamageSystem : ISystem
{
    // Line 76 - static readonly creates .cctor!
    private static readonly FixedString32Bytes s_HazardLabel = new("hazard_");
}
```

**Required fix:**
```csharp
public partial struct HazardEmitFromDamageSystem : ISystem
{
    private FixedString32Bytes _hazardLabel;
    
    public void OnCreate(ref SystemState state)
    {
        _hazardLabel = new FixedString32Bytes("hazard_");
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new HazardEmitFromDamageJob { HazardLabel = _hazardLabel }.Schedule();
    }
}
```

### Fix 3: LifeBoatEjectorSystem.cs (static constructor at line 53, usage at line 73)

**Current (WRONG):**
```csharp
public partial struct LifeBoatEjectorSystem : ISystem
{
    // Line 53 - static readonly creates .cctor!
    private static readonly FixedString32Bytes s_LifeBoatLabel = new("lifeboat_");
}
```

**Required fix:**
```csharp
public partial struct LifeBoatEjectorSystem : ISystem
{
    private FixedString32Bytes _lifeBoatLabel;
    
    public void OnCreate(ref SystemState state)
    {
        _lifeBoatLabel = new FixedString32Bytes("lifeboat_");
    }
}
```

### Fix 4: SchoolFoundingSystem.cs (static constructor at line 122, usage at line 139)

**Current (WRONG):**
```csharp
public partial struct SchoolFoundingSystem : ISystem
{
    // Line 122 - static readonly creates .cctor!
    private static readonly FixedString32Bytes s_SchoolPrefix = new("school_");
}
```

**Required fix:**
```csharp
public partial struct SchoolFoundingSystem : ISystem
{
    private FixedString32Bytes _schoolPrefix;
    
    public void OnCreate(ref SystemState state)
    {
        _schoolPrefix = new FixedString32Bytes("school_");
    }
}
```

---

## Affected Systems (Compilation Graph)

These 60+ systems are currently failing to compile due to the above 4 errors:

**Registry/Aggregates:**
- `PseudoHistorySystem`, `AggregateRegistrySystem`, `CompressionSystem`

**Combat:**
- `HazardEmitFromDamageSystem`, `HitDetectionSystem`, `TargetSelectionSystem`
- `ResolveDirectionalDamageSystem`, `ThreatUpdateSystem`, `ProjectileCollisionSystem`

**Ships:**
- `LifeBoatEjectorSystem`, `ModuleCriticalEffectsSystem`, `ModuleRepairSystem`
- `DerelictClassifierSystem`, `SalvageClaimSystem`, `CrewContractSystem`

**Spells:**
- `SchoolFoundingSystem`, `SpellEffectExecutionSystem`, `SpellLearningSystem`
- `SpellPracticeSystem`, `HybridizationSystem`, `ObservationalLearningSystem`
- `SignatureUnlockSystem`

**Social/Morale:**
- `RelationInteractionSystem`, `RelationDecaySystem`, `GrudgeEscalationSystem`
- `MoraleBandSystem`, `MoraleModifierDecaySystem`, `MoraleMemoryDecaySystem`
- `LoyaltyUpdateSystem`, `LoyaltyEventSystem`, `AddGrudgeSystem`

**AI/Navigation:**
- `AITaskResolutionSystem`, `AISteeringSystem`, `AIUtilityScoringSystem`
- `AIVirtualSensorSystem`, `FlowFieldFollowSystem`, `SpatialSensorUpdateSystem`

**Economy/Items:**
- `TradeExecutionSystem`, `MarketPriceUpdateSystem`, `MarketEventExpirySystem`
- `DurabilityWearSystem`

**Other:**
- `SatisfyNeedSystem`, `NeedsUrgencySystem`, `TelemetryTrendSystem`
- `BalanceAnalysisSystem`, `PlayerBehaviorSystem`, `StatusEffectApplicationSystem`
- `FormationSlotUpdateSystem`, `FormationMemberPositionSystem`, `FormationAssignSystem`
- `DayCycleSystem`, `StatAggregationSystem`, `StatHistorySamplingSystem`
- `AutoProgressionSystem`, `LessonAcquisitionSystem`, `LessonEffectApplicationSystem`
- `KnowledgeLessonProgressionSystem`, `InstanceQualityCalculationSystem`
- `MiningLoopSystem`, `HaulingLoopSystem`, `HaulingJobAssignmentSystem`
- `ResourcePileSystem`, `ResourceUrgencySystem`, `DropOnlyHarvesterSystem`
- `SpatialGridInitialBuildSystem`, `SpatialGridSnapshotSystem`
- ... and more

---

## Testing Requirements

### Pre-Fix Verification

1. Open Unity with Godgame project
2. Observe console errors showing BC1016 from the 4 files listed

### Post-Fix Verification

1. Apply fixes to all 4 files
2. Trigger domain reload in Unity
3. Verify **zero** BC1016 errors in console
4. Verify all systems in affected list now compile
5. Run existing PureDOTS tests to ensure no regressions

### Burst Compilation Check

```bash
# After fixes, run Unity with Burst enabled and verify no errors
Unity -projectPath "$(pwd)" -batchmode -quit -logFile Logs/burst-check.log
grep -i "BC1016\|burst error" Logs/burst-check.log
# Should return empty (no matches)
```

---

## Success Criteria

- [ ] All 4 files updated with Burst-safe string handling
- [ ] Zero BC1016 errors on domain reload
- [ ] All 60+ affected systems compile successfully
- [ ] No new Burst errors introduced
- [ ] Existing PureDOTS tests pass
- [ ] Godgame can enter play mode without Burst compilation failures

---

## Timeline

**Priority:** Critical (blocking)  
**Estimated Effort:** 1-2 hours  
**Dependencies:** None (PureDOTS can implement independently)

**Requested Completion:** ASAP - currently blocking Godgame development

---

## Additional Error: CS0121 Ambiguous math.max (PureDOTS)

**File:** `Runtime/Runtime/Rendering/AnchoredSimulationHelpers.cs`  
**Lines:** 40, 65, 102

### Error

```
error CS0121: The call is ambiguous between the following methods or properties: 
  'math.max(int, int)' and 'math.max(uint2, uint2)'
```

### Cause

Passing arguments to `math.max()` that can implicitly convert to multiple overload signatures. This happens when one argument is `int` and another is `uint`, or when using literals.

### Fix

Explicitly cast arguments to the intended type:

```csharp
// ❌ Ambiguous - compiler can't choose overload
var result = math.max(someValue, 0);

// ✅ Explicit int
var result = math.max(someValue, (int)0);
// OR
var result = math.max((int)someValue, 0);

// ✅ Explicit uint
var result = math.max(someValue, 0u);
```

### Lesson Learned

When using Unity.Mathematics `math.*` functions, always ensure argument types match exactly to avoid ambiguous overload resolution - especially with mixed signed/unsigned integers or literal values.

---

## Related Documentation

- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Contains fix table and session notes
- Unity Burst documentation: [Burst User Guide - Supported Types](https://docs.unity3d.com/Packages/com.unity.burst@latest)

---

**End of Request Document**
