# Demo Build Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Master specification for Godgame demo builds

---

## Principles for Demo Builds

### Data-First
Load everything from catalogs/scenarios; no scene-hardcoded logic. All demo content is driven by JSON scenarios and configuration assets.

### Deterministic
Every demo action is driven by the ScenarioRunner or hotkeys that map to time/rewind + spawn systems. The simulation produces identical results given the same inputs and seed.

### Idempotent Tooling
Prefab Maker runs before a demo build; bindings switch Minimal ↔ Fancy live without requiring code changes or rebuilds.

### Isolation
One build per game, gated by defines so they never compile each other's code. Godgame and Space4X demos are completely separate builds.

---

## Scripting Defines

### Godgame Demo
- **Define:** `GODGAME_DEMO`
- **Purpose:** Enables demo-specific code paths and excludes non-demo systems
- **Usage:** Set in build profile scripting symbols

### Tests (Opt-In)
- **Define:** `GODGAME_TESTS`
- **Purpose:** Includes test assemblies in demo builds (optional)
- **Usage:** Set when building test-enabled demos

### Space4X (Not Used in Godgame)
- **Define:** `SPACE4X_DEMO` (for reference only)
- **Note:** Space4X has its own demo build in its repository

---

## Build Targets & Gating

### Godgame Demo Build Profile

**Scripting Symbols:**
- `GODGAME_DEMO`
- Optional: `GODGAME_TESTS` (if including test assemblies)

**Assembly Exclusions:**
- Exclude Space4X assemblies (they already have `SPACE4X` defineConstraints)
- Optional: `PUREDOTS_AUTHORING` off unless authoring is embedded

**Build Output:**
- Windows: `Builds/Godgame_Demo.exe`
- Includes: Executable, Scenarios/, Bindings/, Reports/

### Build Methods

**CLI Build:**
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -buildWindows64Player Builds/Godgame_Demo.exe
```

**Editor Build Profile:**
- Create build profile: `Godgame Demo`
- Set scripting symbols: `GODGAME_DEMO`
- Configure assembly exclusions
- Set output path: `Builds/Godgame_Demo.exe`

---

## Demo Bootstrap

All demos start via `DemoBootstrap` system (see `DemoBootstrap_Spec.md`):

1. Loads scenario from `/Assets/Scenarios/Godgame/*.json`
2. Exposes toggles for time, presentation, determinism overlay
3. Starts ScenarioRunner with seed + options
4. Attaches reporter for metrics (see `Instrumentation_Spec.md`)

---

## Pre-Demo Checklist

Before building a demo, run preflight validation (see `Preflight_Checklist.md`):

1. Prefab Maker: Run in Minimal, validate, check idempotency
2. Scenarios: Dry-run determinism at 30/60/120Hz
3. Budgets: Assert fixed_tick_ms ≤ target, snapshot ring within limits
4. Bindings: Swap Minimal↔Fancy, assert no exceptions

---

## Packaging

Demo builds are packaged as zips (see `Packaging_Spec.md`):

- **Output:** `Godgame_Demo_<date>.zip`
- **Contents:**
  - Executable
  - `Scenarios/` folder with JSON files
  - `Bindings/Minimal.asset`, `Bindings/Fancy.asset`
  - `Readme.md` (hotkeys, known limits)
  - `Reports/last_run.json`

---

## Related Documentation

- `DemoBootstrap_Spec.md` - DemoBootstrap system design
- `Godgame_Slices.md` - Demo slices and features
- `Bindings_Spec.md` - Presentation binding system
- `Instrumentation_Spec.md` - Metrics and reporting
- `Preflight_Checklist.md` - Pre-demo validation
- `Packaging_Spec.md` - Packaging requirements
- `Scenarios_Spec.md` - Scenario format and known-good scenarios
- `Talk_Track.md` - Demo presentation script

---

## Notes

- This specification covers **Godgame only**. Space4X has its own demo documentation in its repository.
- All demo builds are deterministic and data-driven.
- Presentation bindings can be swapped live without code changes.

