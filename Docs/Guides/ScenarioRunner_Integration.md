# PureDOTS Scenario Runner & Time Integration Guide

**Date:** 2025-01-24  
**Status:** Implementation Plan  
**Purpose:** Guide for integrating Godgame bootstrap into PureDOTS ScenarioRunner and time spine

---

## Current State Assessment

### Scenario Runner

**Implemented:**
- `GodgameScenarioSpawnLoggerSystem.cs`: Logs scenario entity counts (stub, doesn't spawn)
- PureDOTS has `ScenarioRunnerExecutor` and `ScenarioRunnerEntryPoints`
- PureDOTS has scenario JSON samples

**Gaps:**
- No adapter to convert scenario JSON counts to actual entity spawns
- No registry-backed spawn system for scenarios
- No CLI hooks for scenario execution
- No editor menu items for scenario testing

### Time Integration

**Implemented:**
- `TimeControlSystem.cs`: Pause/speed/step/rewind support
- `TimeDemoHistorySystem.cs`: Snapshot/command log
- PureDOTS time spine (`TimeState`, `RewindState`) integrated

**Gaps:**
- Time controls not fully validated with scenarios
- No determinism tests for scenario + rewind combination

---

## Implementation Plan

### 1. Scenario Spawn System

**Purpose:** Replace logger with actual spawn system that creates entities from scenario JSON

**System to Create:**
- `GodgameScenarioSpawnSystem.cs`: Replaces `GodgameScenarioSpawnLoggerSystem`
- Reads `ScenarioEntityCountElement` buffer from `ScenarioInfo`
- Maps registry IDs to Godgame entity archetypes
- Spawns entities using `EntityCommandBuffer`
- Positions entities within spawn area

**Component Mapping:**
```csharp
// Registry ID → Entity Archetype mapping
private static EntityArchetype GetArchetypeForRegistryId(int registryId)
{
    return registryId switch
    {
        GodgameRegistryIds.VillagerArchetype => VillagerArchetype,
        GodgameRegistryIds.StorehouseArchetype => StorehouseArchetype,
        GodgameRegistryIds.ResourceNodeArchetype => ResourceNodeArchetype,
        GodgameRegistryIds.BandArchetype => BandArchetype,
        // ... etc
    };
}
```

**Files to Touch:**
- Replace: `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnLoggerSystem.cs` → `GodgameScenarioSpawnSystem.cs`
- New: `Assets/Scripts/Godgame/Scenario/ScenarioSpawnMapping.cs` (registry ID → archetype mapping)
- New: `Assets/Scripts/Godgame/Tests/ScenarioSpawnTests.cs`

**Integration Points:**
- Wire to `Godgame_ScenarioBootstrapSystem` for shared spawn logic
- Use `GodgameRegistryBridgeSystem` for registry sync after spawn

---

### 2. Bootstrap Adapter

**Purpose:** Adapter to wire Godgame bootstrap to ScenarioRunner

**Adapter to Create:**
- `GodgameScenarioBootstrapAdapter.cs`: Adapter system that bridges ScenarioRunner to Godgame bootstrap
- Detects when scenario is active (`ScenarioInfo` component present)
- Triggers Godgame bootstrap spawns based on scenario counts
- Ensures PureDOTS singletons are initialized before spawning

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Scenario/GodgameScenarioBootstrapAdapter.cs`
- Modify: `Assets/Scripts/Godgame/Demo/Godgame_ScenarioBootstrapSystem.cs` (support scenario mode)
- New: `Assets/Scripts/Godgame/Tests/ScenarioBootstrapAdapterTests.cs`

**Integration Points:**
- Runs after `CoreSingletonBootstrapSystem` (ensures singletons exist)
- Runs before `GodgameRegistryBridgeSystem` (ensures entities exist for sync)

---

### 3. CLI Hooks

**Purpose:** Add CLI hooks for scenario execution (`--scenario`, `--report`)

**Entry Point to Create:**
- `GodgameScenarioEntryPoints.cs`: Static methods for CLI execution
- Forwards to PureDOTS `ScenarioRunnerEntryPoints.RunScenarioFromArgs`
- Adds Godgame-specific spawn/bootstrap logic

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Scenario/GodgameScenarioEntryPoints.cs`
- Modify: `ProjectSettings/EditorBuildSettings.asset` (add execute method)

**CLI Usage:**
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.RunScenario --scenario "Assets/Scenarios/godgame_demo.json" --report "Logs/scenario_report.json"
```

---

### 4. Editor Menu Items

**Purpose:** Add editor menu items for scenario testing

**Menu Items to Add:**
- `Tools/Godgame/Run Scenario...`: Opens file dialog to select scenario JSON, runs scenario
- `Tools/Godgame/Run Scenario (Headless)`: Runs scenario in headless mode
- `Tools/Godgame/Validate Scenario...`: Validates scenario JSON without running

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Editor/GodgameScenarioMenuItems.cs`
- Modify: `Assets/Scripts/Godgame/Scenario/GodgameScenarioEntryPoints.cs` (add editor-friendly methods)

---

### 5. Scenario JSON Format

**Purpose:** Document Godgame-specific scenario JSON format

**Scenario JSON Structure:**
```json
{
  "scenarioId": "godgame_demo",
  "seed": 12345,
  "runTicks": 1000,
  "entityCounts": [
    {
      "registryId": 1,  // VillagerArchetype
      "count": 10
    },
    {
      "registryId": 2,  // StorehouseArchetype
      "count": 2
    },
    {
      "registryId": 3,  // ResourceNodeArchetype
      "count": 5
    }
  ],
  "spawnConfig": {
    "center": [0, 0, 0],
    "radius": 50.0
  }
}
```

**Files to Touch:**
- New: `Assets/Scenarios/godgame_demo.json` (example scenario)
- Document format in this guide

---

### 6. Time Spine Validation

**Purpose:** Validate time controls work correctly with scenarios

**Enhancements:**
- Ensure `TimeState` and `RewindState` are initialized before scenario spawn
- Validate rewind determinism with scenario entities
- Test scenario + rewind combination

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Tests/ScenarioRewindDeterminismTests.cs`
- Modify: `Assets/Scripts/Godgame/Time/TimeControlSystem.cs` (ensure scenario compatibility)

**Test Strategy:**
- Run scenario for N ticks
- Rewind M ticks
- Resume and validate state matches expected

---

### 7. Registry Continuity with Scenarios

**Purpose:** Ensure registry continuity works with scenario-spawned entities

**Enhancements:**
- Ensure scenario-spawned entities get spatial indexing
- Ensure registry sync happens after scenario spawn
- Validate continuity snapshots include scenario entities

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnSystem.cs` (add spatial indexing)
- Modify: `Assets/Scripts/Godgame/Registry/Registry/GodgameRegistryBridgeSystem.cs` (ensure scenario compatibility)
- New: `Assets/Scripts/Godgame/Tests/ScenarioRegistryContinuityTests.cs`

---

## Implementation Order

### Phase 1: Scenario Spawn System (High Priority)
1. Replace logger with spawn system
2. Implement registry ID → archetype mapping
3. Add tests for spawn system

### Phase 2: Bootstrap Adapter (High Priority)
1. Create bootstrap adapter
2. Wire adapter to ScenarioRunner
3. Add tests for adapter

### Phase 3: CLI Hooks (Medium Priority)
1. Create CLI entry points
2. Add execute method to build settings
3. Test CLI execution

### Phase 4: Editor Menu Items (Medium Priority)
1. Create editor menu items
2. Add file dialog for scenario selection
3. Test editor execution

### Phase 5: Time Spine Validation (Medium Priority)
1. Add scenario + rewind tests
2. Validate determinism
3. Document time spine usage

### Phase 6: Registry Continuity (Low Priority)
1. Ensure spatial indexing for scenario entities
2. Validate registry sync
3. Test continuity snapshots

---

## Testing Strategy

### Unit Tests
- `ScenarioSpawnTests.cs`: Test spawn system creating entities
- `ScenarioBootstrapAdapterTests.cs`: Test adapter wiring
- `ScenarioRewindDeterminismTests.cs`: Test scenario + rewind determinism
- `ScenarioRegistryContinuityTests.cs`: Test registry continuity with scenarios

### Integration Tests
- `ScenarioSpawnToRegistryTests.cs`: Test spawned entities syncing to registries
- `ScenarioBootstrapToTimeTests.cs`: Test bootstrap with time controls

### PlayMode Tests
- `Scenario_FullLoop_Playmode.cs`: Test complete scenario execution
- `Scenario_Rewind_Playmode.cs`: Test scenario with rewind

### Headless Tests
- Run scenarios headless via CLI
- Validate scenario reports
- Compare headless vs editor execution

---

## Files Summary

### New Files to Create
- `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnSystem.cs` (replaces logger)
- `Assets/Scripts/Godgame/Scenario/ScenarioSpawnMapping.cs`
- `Assets/Scripts/Godgame/Scenario/GodgameScenarioBootstrapAdapter.cs`
- `Assets/Scripts/Godgame/Scenario/GodgameScenarioEntryPoints.cs`
- `Assets/Scripts/Godgame/Editor/GodgameScenarioMenuItems.cs`
- `Assets/Scripts/Godgame/Tests/ScenarioSpawnTests.cs`
- `Assets/Scripts/Godgame/Tests/ScenarioBootstrapAdapterTests.cs`
- `Assets/Scripts/Godgame/Tests/ScenarioRewindDeterminismTests.cs`
- `Assets/Scripts/Godgame/Tests/ScenarioRegistryContinuityTests.cs`
- `Assets/Scenarios/godgame_demo.json` (example scenario)

### Files to Modify
- Delete: `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnLoggerSystem.cs`
- Modify: `Assets/Scripts/Godgame/Demo/Godgame_ScenarioBootstrapSystem.cs` (support scenario mode)
- Modify: `Assets/Scripts/Godgame/Time/TimeControlSystem.cs` (ensure scenario compatibility)
- Modify: `Assets/Scripts/Godgame/Registry/Registry/GodgameRegistryBridgeSystem.cs` (ensure scenario compatibility)
- Modify: `ProjectSettings/EditorBuildSettings.asset` (add execute method)

---

## PureDOTS Integration Points

1. **ScenarioRunner**: Use PureDOTS `ScenarioRunnerExecutor` and `ScenarioRunnerEntryPoints`
2. **Time Spine**: Use PureDOTS `TimeState`, `RewindState`, snapshot/command log
3. **Registry Continuity**: Use PureDOTS `RegistryContinuitySnapshot` for scenario entities
4. **Telemetry**: Wire scenario telemetry to PureDOTS `TelemetryStream`

---

## Related Documentation

- `Docs/PureDOTS_ScenarioRunner_Wiring.md` - Scenario runner wiring guide
- `Docs/PureDOTS_TimeIntegration.md` - Time integration guide
- `Docs/Archive/Demo_Legacy/DemoReadiness_GapAnalysis.md` - Gap analysis
- `Docs/Archive/Demo_Legacy/DemoSceneSetup.md` - Scene setup guide

---

## CLI Usage Examples

### Run Scenario Headless
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.RunScenario --scenario "Assets/Scenarios/godgame_demo.json" --report "Logs/scenario_report.json"
```

### Run Scenario with Rewind Test
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.RunScenarioWithRewind --scenario "Assets/Scenarios/godgame_demo.json" --rewindTicks 100 --report "Logs/scenario_rewind_report.json"
```

### Validate Scenario JSON
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.ValidateScenario --scenario "Assets/Scenarios/godgame_demo.json"
```
