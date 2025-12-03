# Agent 1: Fix Burst BC1016 Errors - Aggregates/Spells

## Your Mission
Fix BC1016 Burst compilation errors in PureDOTS by extracting string literals from Burst-compiled code.

## Project Location
`C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS`

## Required Reading First
Read `TRI_PROJECT_BRIEFING.md` in the project root, specifically sections **P8** and **P13**.

---

## Files to Fix

### File 1: BandFormationSystem.cs
**Path**: `Packages/com.moni.puredots/Runtime/Systems/Aggregates/BandFormationSystem.cs`
**Line**: ~255
**Method**: `GoalToDescription`

**Error**:
```
BC1016: The managed function `System.String.get_Length` is not supported
at Unity.Collections.FixedString128Bytes..ctor
at PureDOTS.Systems.Aggregates.BandFormationSystem.GoalToDescription
```

**Problem**: Creates `FixedString128Bytes` from string literal inside Burst-compiled code.

---

### File 2: SpellEffectExecutionSystem.cs
**Path**: `Packages/com.moni.puredots/Runtime/Systems/Spells/SpellEffectExecutionSystem.cs`
**Line**: ~311
**Method**: `ApplyShieldEffect`

**Error**:
```
BC1016: The managed function `System.String.get_Length` is not supported
at Unity.Collections.FixedString64Bytes..ctor
at PureDOTS.Systems.Spells.SpellEffectExecutionSystem.ProcessSpellEffectsJob.ApplyShieldEffect
```

**Problem**: Creates `FixedString64Bytes` from string literal inside Burst-compiled job.

---

## Fix Pattern

```csharp
// ❌ WRONG - Inside Burst method:
private FixedString128Bytes GoalToDescription(Goal goal)
{
    return goal switch
    {
        Goal.Idle => new FixedString128Bytes("Idle"),      // BC1016!
        Goal.Work => new FixedString128Bytes("Working"),   // BC1016!
        _ => new FixedString128Bytes("Unknown")            // BC1016!
    };
}

// ✅ CORRECT - Define constants at class level (OUTSIDE methods):
private static readonly FixedString128Bytes GoalDescIdle = "Idle";
private static readonly FixedString128Bytes GoalDescWork = "Working";
private static readonly FixedString128Bytes GoalDescUnknown = "Unknown";

// Then in Burst method:
private FixedString128Bytes GoalToDescription(Goal goal)
{
    return goal switch
    {
        Goal.Idle => GoalDescIdle,      // ✅ Just copies constant
        Goal.Work => GoalDescWork,      // ✅ 
        _ => GoalDescUnknown            // ✅
    };
}
```

---

## Step-by-Step Instructions

1. Open `BandFormationSystem.cs`
2. Find the `GoalToDescription` method (~line 255)
3. Identify all string literals being passed to FixedString constructors
4. Create `private static readonly` constants at class level for each string
5. Replace the constructors with references to the constants
6. Repeat for `SpellEffectExecutionSystem.cs` → `ApplyShieldEffect` method

---

## Verification

After fixing, verify by:
1. Opening Unity Editor for PureDOTS project
2. Triggering domain reload (edit any script or Ctrl+R)
3. Check Console - should have NO BC1016 errors mentioning:
   - `BandFormationSystem`
   - `SpellEffectExecutionSystem`

---

## Constraints

- Use C# 9 syntax (no `ref readonly`, no collection expressions)
- Unity Entities 1.4.2 (not 1.5+)
- All constants must be `static readonly`, not `const`
- Keep constants near the top of the class, before methods

---

## Report When Complete

```
✅ BandFormationSystem.GoalToDescription - FIXED
   - Added X static constants
   - Replaced X string constructors
   
✅ SpellEffectExecutionSystem.ApplyShieldEffect - FIXED
   - Added X static constants
   - Replaced X string constructors

Build status: Domain reload succeeded with no BC1016 errors for these files
```


