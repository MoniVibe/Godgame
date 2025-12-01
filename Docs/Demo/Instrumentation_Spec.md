# Instrumentation and Reports Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Design specification for demo metrics, reporting, and artifacts

---

## Overview

The demo instrumentation system collects metrics during demo runs and generates reports for validation, analysis, and comparison. Reports are stored as JSON, CSV, and optional screenshots.

---

## Reporter System

### Attachment

**Location:** Attached to `DemoBootstrap` system

**Lifecycle:**
1. Reporter attaches when demo starts
2. Collects metrics every frame or at intervals
3. Writes reports when demo ends or on demand
4. Detaches when demo completes

### Reporter Component

```csharp
public struct DemoReporter : IComponentData
{
    public FixedString64Bytes ScenarioName;
    public uint StartTick;
    public uint EndTick;
    public NativeList<MetricSnapshot> Snapshots; // Per-frame metrics
}
```

---

## JSON Output

### Key Metrics Per Slice

**Output File:** `Reports/Godgame/<scenario>/<timestamp>/metrics.json`

**Structure:**
```json
{
  "scenario": "villager_loop_small.json",
  "timestamp": "2025-01-XX-12-34-56",
  "slice": "villager_loop",
  "metrics": {
    "damage_total": 0,
    "modules_hit": 0,
    "throughput": 150.5,
    "sanctions": 0,
    "fixed_tick_ms": 8.2,
    "snapshot_kb": 1024,
    "villagers_active": 10,
    "resources_gathered": 45,
    "resources_delivered": 45,
    "construction_completed": 0
  },
  "determinism": {
    "rewind_test": "pass",
    "byte_equal": true
  }
}
```

**Metrics Collected:**

**Godgame-Specific:**
- `villagers_active` - Active villager count
- `resources_gathered` - Total resources gathered
- `resources_delivered` - Total resources delivered
- `construction_completed` - Construction completions
- `jobs_active` - Active job count
- `storehouse_inventory` - Storehouse inventory totals

**System Metrics:**
- `fixed_tick_ms` - Fixed step duration
- `snapshot_kb` - Snapshot ring memory usage
- `ecb_playback_ms` - ECB playback time
- `fps` - Frame rate

**Determinism:**
- `rewind_test` - Pass/fail for rewind determinism
- `byte_equal` - Byte-equal validation result

---

## CSV Output

### Time Series Data

**Output File:** `Reports/Godgame/<scenario>/<timestamp>/timeseries.csv`

**Structure:**
```csv
t,villagers,items,throughput,fixed_tick_ms,snapshot_kb
0,10,0,0.0,8.1,512
1,10,0,0.0,8.2,512
2,10,2,1.0,8.1,512
3,10,5,2.5,8.2,512
...
```

**Columns:**
- `t` - Simulation tick
- `villagers` - Active villager count
- `items` - Resource items in system
- `throughput` - Resources per second
- `fixed_tick_ms` - Fixed step duration
- `snapshot_kb` - Snapshot memory usage

**Sampling Rate:**
- Default: Every frame
- Configurable: Every N ticks or every N seconds

---

## Screenshots

### Capture Points

**Screenshots Taken:**
1. **Scenario Start:** When scenario begins
2. **Scenario End:** When scenario completes
3. **Key Events:** On construction completion, rewind points, etc.

**Output Files:**
- `Reports/Godgame/<scenario>/<timestamp>/screenshot_start.png`
- `Reports/Godgame/<scenario>/<timestamp>/screenshot_end.png`
- `Reports/Godgame/<scenario>/<timestamp>/screenshot_<event>.png`

### Purpose

**Visual Diff Validation:**
- Compare screenshots between Minimal and Fancy bindings
- Verify visual changes don't affect simulation
- Document visual state at key moments

---

## Artifact Storage

### Directory Structure

```
Reports/
├── Godgame/
│   ├── villager_loop_small/
│   │   ├── 2025-01-XX-12-34-56/
│   │   │   ├── metrics.json
│   │   │   ├── timeseries.csv
│   │   │   ├── screenshot_start.png
│   │   │   └── screenshot_end.png
│   │   └── 2025-01-XX-14-22-10/
│   │       └── ...
│   └── construction_ghost/
│       └── ...
```

### Naming Convention

**Timestamp Format:** `YYYY-MM-DD-HH-MM-SS`

**Example:** `2025-01-15-14-30-45`

---

## Report Generation

### Automatic Generation

**Triggers:**
1. Demo completion (normal end)
2. Demo error (exception caught)
3. Manual trigger (hotkey or UI button)

### Manual Generation

**API:**
```csharp
public static class DemoReporter
{
    public static void GenerateReport(string scenarioName)
    {
        // Collect current metrics
        // Write JSON, CSV, screenshots
    }
}
```

---

## Integration Points

### DemoBootstrap

**Flow:**
1. DemoBootstrap starts scenario
2. Attaches DemoReporter component
3. Reporter begins collecting metrics
4. On completion, reporter writes reports

### ScenarioRunner

**Flow:**
1. ScenarioRunner emits events (entity spawn, completion, etc.)
2. Reporter listens to events
3. Records metrics at event points

### Time System

**Flow:**
1. Reporter reads `TimeState` for tick information
2. Records `fixed_tick_ms` from system metrics
3. Tracks rewind events for determinism validation

---

## Acceptance Criteria

1. **Reports Generated:** JSON and CSV files created for every demo run
2. **Metrics Accurate:** All metrics reflect actual simulation state
3. **Determinism Logged:** Rewind tests logged with pass/fail
4. **Screenshots Captured:** Start/end screenshots saved (if enabled)
5. **Artifacts Organized:** Reports stored in correct directory structure

---

## Related Documentation

- `DemoBootstrap_Spec.md` - DemoBootstrap system (reporter attachment)
- `Godgame_Slices.md` - Demo slices (metrics to collect)
- `Preflight_Checklist.md` - Preflight validation (report validation)

