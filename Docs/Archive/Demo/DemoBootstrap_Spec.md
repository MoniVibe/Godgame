# DemoBootstrap Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Design specification for DemoBootstrap system

---

## Overview

`DemoBootstrap` is a Mono/ISystem that initializes demo runs, loads scenarios, exposes runtime toggles, and manages demo state. It serves as the entry point for all demo builds.

---

## System Design

### Architecture Options

**Option A: MonoBehaviour (Recommended for UI)**
- Attach to GameObject in demo scene
- Handles UI interactions (scenario select, toggles)
- Calls into DOTS systems for actual work

**Option B: ISystem (Pure DOTS)**
- Runs in system group
- Requires separate UI bridge for interactions
- More Burst-friendly but less flexible for UI

**Recommendation:** Use MonoBehaviour for initial implementation, can migrate to ISystem later if needed.

---

## Core Components

### DemoOptions Component

```csharp
public struct DemoOptions : Unity.Entities.IComponentData
{
    public FixedString64Bytes ScenarioPath;
    public byte BindingsSet; // 0=Minimal, 1=Fancy
    public byte Veteran;     // 0/1 (for Space4X compatibility)
}
```

**Properties:**
- `ScenarioPath`: Path to scenario JSON file (e.g., `"Scenarios/Godgame/villager_loop_small.json"`)
- `BindingsSet`: Active presentation binding set (0=Minimal, 1=Fancy)
- `Veteran`: Veteran proficiency toggle (0=off, 1=on) - for Space4X compatibility

**Usage:**
- Loaded at boot from UI or CLI arguments
- Read-only in Burst systems
- Only mutated by non-Burst input handler

---

## Scenario Select UI

### UI Requirements

**Scenario Selection:**
- Dropdown or list of available scenarios from `/Assets/Scenarios/Godgame/*.json`
- Display scenario name and description
- Load button to start selected scenario

**Scenario Discovery:**
- Scan `Assets/Scenarios/Godgame/` for `.json` files
- Parse scenario metadata (name, description, entity counts)
- Display in UI

---

## Runtime Toggles

### Time Controls

**Toggles:**
- **Pause/Play:** Toggle simulation pause state
- **Step:** Single tick forward (when paused)
- **Speed:** ×0.5, ×1, ×2 multipliers
- **Rewind:** Enable/disable rewind mode

**Implementation:**
- Map to `TimeControlInput` singleton
- Emit HUD events for feedback
- Display current state in determinism overlay

### Presentation Controls

**Toggles:**
- **Bindings:** Swap Minimal ↔ Fancy (hotkey: B)
- **Debug Overlays:** Show/hide debug visualization
- **Visual Quality:** Adjust presentation detail level

**Implementation:**
- Hotkey B swaps `DemoOptions.BindingsSet`
- Presentation system reads binding set and applies visuals
- No code changes required for swap

### Determinism Overlay

**Display:**
- **Tick:** Current simulation tick
- **RNG Seed:** Active random seed
- **Snapshot Ring Usage:** Memory usage for snapshots
- **ECB Playback ms:** EntityCommandBuffer playback time

**Implementation:**
- Read from `TimeState`, `RewindState` singletons
- Display in HUD overlay (right side)
- Update every frame

---

## CLI Interface

### Execute Method

```csharp
public static class Demos
{
    public static class Build
    {
        public static void Run(string game, string scenario, string bindings)
        {
            // Load scenario
            // Set DemoOptions
            // Start ScenarioRunner
        }
    }
}
```

### CLI Usage

**Godgame:**
```bash
-executeMethod Demos.Build.Run --game=Godgame --scenario=construction_ghost.json --bindings=Minimal
-executeMethod Demos.Build.Run --game=Godgame --scenario=villager_loop_small.json --bindings=Fancy
```

**Parameters:**
- `--game`: Game identifier (`Godgame` or `Space4X`)
- `--scenario`: Scenario filename (relative to `Assets/Scenarios/<game>/`)
- `--bindings`: Binding set (`Minimal` or `Fancy`)

---

## Integration Points

### ScenarioRunner

**Flow:**
1. DemoBootstrap loads scenario JSON
2. Parses entity counts and configuration
3. Calls `ScenarioRunner.RunScenario(scenarioPath, seed, options)`
4. ScenarioRunner spawns entities and starts simulation

### Presentation System

**Flow:**
1. DemoBootstrap sets `DemoOptions.BindingsSet`
2. Presentation system reads binding set on startup
3. Loads corresponding binding asset (`Bindings/Minimal.asset` or `Bindings/Fancy.asset`)
4. Applies visual mappings

### Time System

**Flow:**
1. DemoBootstrap exposes time controls via UI/hotkeys
2. Updates `TimeControlInput` singleton
3. `TimeControlSystem` processes commands
4. HUD events emitted for feedback

---

## File Structure

### Implementation Files

**DemoBootstrap System:**
- `Assets/Scripts/Godgame/Demo/DemoBootstrap.cs` (MonoBehaviour)
- `Assets/Scripts/Godgame/Demo/DemoBootstrapSystem.cs` (ISystem, optional)
- `Assets/Scripts/Godgame/Demo/DemoOptions.cs` (IComponentData)

**UI:**
- `Assets/Scripts/Godgame/Demo/DemoBootstrapUI.cs` (UI controller)
- `Assets/Prefabs/UI/DemoBootstrapUI.prefab` (UI prefab)

**CLI:**
- `Assets/Scripts/Godgame/Demo/Demos.Build.cs` (CLI entry point)

---

## Acceptance Criteria

1. **Scenario Loading:** Can load and start any scenario from `Assets/Scenarios/Godgame/`
2. **Toggles Work:** All time/presentation/determinism toggles function correctly
3. **CLI Compatible:** Can run demos headless via CLI with all parameters
4. **Binding Swap:** Can swap Minimal ↔ Fancy bindings live without exceptions
5. **Determinism Display:** Overlay shows accurate tick, seed, snapshot usage

---

## Related Documentation

- `Demo_Build_Spec.md` - Master build specification
- `Godgame_Slices.md` - Demo slices and features
- `Bindings_Spec.md` - Presentation binding system
- `Scenarios_Spec.md` - Scenario format

