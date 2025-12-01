# Pre-Demo Preflight Checklist

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Pre-demo validation checklist and automation

---

## Overview

The preflight system validates demo readiness before building. It checks prefab generation, scenario determinism, performance budgets, and binding swaps. All checks must pass before a demo build is considered ready.

---

## Preflight Method

### CLI Entry Point

```csharp
public static class Demos
{
    public static class Preflight
    {
        public static void Run(string game)
        {
            // Run all preflight checks
            // Log pass/fail for each
            // Exit with error code if any fail
        }
    }
}
```

### CLI Usage

**Godgame:**
```bash
-executeMethod Demos.Preflight.Run --game=Godgame
```

**Output:**
- Console logs for each check
- Pass/fail status per check
- Exit code: 0 = pass, 1 = fail

---

## Preflight Steps

### Step 1: Prefab Maker

**Action:** Run Prefab Maker in Minimal mode

**Steps:**
1. Set binding mode to Minimal
2. Run Prefab Maker tool
3. Validate all prefabs generated
4. Check for errors or warnings

**Validation:**
- All required prefabs exist
- No missing references
- Idempotency JSON written

**Idempotency Check:**
1. Run Prefab Maker twice
2. Compare output file hashes
3. Assert identical (idempotent)
4. Log pass/fail

**Output:**
- `PrefabMaker_Idempotency.json` - Idempotency validation result
- Console log: `[PASS/FAIL] Prefab Maker idempotency`

---

### Step 2: Scenario Determinism

**Action:** Dry-run determinism tests at multiple frame rates

**Steps:**
1. Load each known-good scenario
2. Run short version (5-10 seconds)
3. Test at 30Hz, 60Hz, 120Hz
4. Compare final state across frame rates

**Validation:**
- Same final state at all frame rates
- No frame-rate-dependent bugs
- Determinism preserved

**Scenarios Tested:**
- `villager_loop_small.json` (short version)
- `construction_ghost.json` (short version)
- `time_rewind_smoke.json` (short version)

**Output:**
- Console log: `[PASS/FAIL] Scenario determinism at 30Hz`
- Console log: `[PASS/FAIL] Scenario determinism at 60Hz`
- Console log: `[PASS/FAIL] Scenario determinism at 120Hz`

---

### Step 3: Performance Budgets

**Action:** Assert performance targets are met

**Checks:**

**Fixed Tick Budget:**
- Assert `fixed_tick_ms ≤ target` (e.g., ≤16ms for 60Hz)
- Log actual vs target
- Fail if exceeds budget

**Snapshot Ring:**
- Assert snapshot ring usage within limits
- Check memory pressure
- Fail if exceeds limits

**Output:**
- Console log: `[PASS/FAIL] Fixed tick budget: 8.2ms ≤ 16ms`
- Console log: `[PASS/FAIL] Snapshot ring: 1024KB ≤ 2048KB`

---

### Step 4: Binding Swap

**Action:** Swap Minimal ↔ Fancy bindings and assert no exceptions

**Steps:**
1. Load demo with Minimal bindings
2. Run for 1 second
3. Swap to Fancy bindings
4. Run for 1 second
5. Swap back to Minimal
6. Assert no exceptions

**Validation:**
- No exceptions during swap
- Visuals update correctly
- Metrics remain identical
- Performance acceptable

**Output:**
- Console log: `[PASS/FAIL] Binding swap Minimal → Fancy`
- Console log: `[PASS/FAIL] Binding swap Fancy → Minimal`
- Console log: `[PASS/FAIL] Metrics identical after swap`

---

## Pass/Fail Output Format

### Console Output

```
=== Preflight Check: Godgame Demo ===

[1/4] Prefab Maker...
  ✓ Prefab Maker idempotency: PASS
  ✓ Prefab validation: PASS

[2/4] Scenario Determinism...
  ✓ villager_loop_small at 30Hz: PASS
  ✓ villager_loop_small at 60Hz: PASS
  ✓ villager_loop_small at 120Hz: PASS
  ✓ construction_ghost at 30Hz: PASS
  ...

[3/4] Performance Budgets...
  ✓ Fixed tick budget: PASS (8.2ms ≤ 16ms)
  ✓ Snapshot ring: PASS (1024KB ≤ 2048KB)

[4/4] Binding Swap...
  ✓ Minimal → Fancy: PASS
  ✓ Fancy → Minimal: PASS
  ✓ Metrics identical: PASS

=== Preflight Result: PASS ===
```

### Exit Codes

- **0:** All checks passed
- **1:** One or more checks failed

---

## Integration

### CI/CD Integration

**Pre-Build Step:**
```bash
# Run preflight before building
-executeMethod Demos.Preflight.Run --game=Godgame
if [ $? -ne 0 ]; then
    echo "Preflight failed, aborting build"
    exit 1
fi

# Build demo
-buildWindows64Player Builds/Godgame_Demo.exe
```

### Editor Integration

**Menu Item:**
- `Tools/Godgame/Run Preflight...`
- Opens dialog with game selection
- Runs preflight checks
- Displays results in console

---

## Failure Handling

### On Failure

**Actions:**
1. Log detailed error information
2. Write failure report to `Reports/Preflight/<timestamp>/failures.json`
3. Exit with error code 1
4. Do not proceed with build

**Failure Report:**
```json
{
  "timestamp": "2025-01-XX-12-34-56",
  "game": "Godgame",
  "failures": [
    {
      "check": "Fixed tick budget",
      "expected": "≤16ms",
      "actual": "18.5ms",
      "message": "Fixed tick exceeds budget"
    }
  ]
}
```

---

## Related Documentation

- `Demo_Build_Spec.md` - Build specification (preflight integration)
- `Bindings_Spec.md` - Binding system (binding swap check)
- `Scenarios_Spec.md` - Scenarios (determinism tests)

