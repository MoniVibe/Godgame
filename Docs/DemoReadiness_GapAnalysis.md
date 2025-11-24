# Demo & Aggregate AI Readiness - Gap Analysis

**Date:** 2025-01-24  
**Status:** Comprehensive Audit  
**Purpose:** Assess current state and identify gaps for fully functional villager/aggregate demo

---

## Executive Summary

The Godgame project has a solid foundation with core systems (villager jobs, construction, time controls, registries) implemented and Burst-compiled. However, several critical gaps remain for a fully functional demo showcasing villagers and aggregate entities (villages, bands) with rich AI behaviors:

1. **Villager AI**: Basic gather/deliver loop exists, but lacks interrupt handling, job scheduling, needs/utility curves, and GOAP hooks
2. **Aggregate Behaviors**: Village/band components exist but lack presentation bindings, aggregate AI decision-making, and full registry sync
3. **Demo Scene**: SubScene wizard exists but needs content pipeline for self-contained demo with proper prefabs
4. **Scenario Runner**: Integration stubbed but not wired to actual spawn/bootstrap systems
5. **Testing**: Core systems tested but missing coverage for AI upgrades, aggregate behaviors, and scenario integration

---

## 1. Villager AI Subsystem

### Current State

**Implemented:**
- `VillagerJobSystem`: Basic state machine (Idle → NavigateToNode → Gather → NavigateToStorehouse → Deliver)
- `VillagerPersonalitySystem`: Grudge generation/decay, patriotism drift, combat stance modifiers
- `VillagerUtilityScheduler`: Initiative calculation, base utility functions (stubs for needs/job selection)
- `VillagerJobComponents`: Job state tracking, navigation, carry capacity
- Registry sync: `GodgameVillagerSyncSystem` mirrors villager data to `VillagerRegistry`

**Gaps Identified:**

1. **Interrupt Handling**
   - No system to handle hand pickup interruptions
   - No path-blocked detection/resolution
   - No job cancellation/resume logic
   - **Files to touch:** `Assets/Scripts/Godgame/Villagers/VillagerJobSystem.cs`, new `VillagerInterruptSystem.cs`

2. **Job Scheduler/Assignment**
   - No job assignment buffer system
   - No GOAP hooks for goal-oriented planning
   - Simple nearest-node selection (no priority/capacity logic)
   - **Files to touch:** New `VillagerJobSchedulerSystem.cs`, extend `VillagerJobComponents.cs`

3. **Needs/Utility Curves**
   - `VillagerUtilityScheduler` has stub methods but no actual need tracking
   - No hunger/fatigue/hygiene/mood components
   - No consumption systems pulling from stores
   - **Files to touch:** New `VillagerNeedsComponents.cs`, `VillagerNeedsSystem.cs`, extend `VillagerUtilityScheduler.cs`

4. **Storehouse Selection**
   - Simple distance-based selection (no capacity/priority logic)
   - No reservation system for incoming deliveries
   - **Files to touch:** `VillagerJobSystem.cs`, extend `StorehouseApi.cs`

5. **Autonomous Actions**
   - `VillagerUtilityScheduler.SelectAutonomousAction` exists but not wired to job system
   - No family formation, business ventures, adventure seeking
   - **Files to touch:** New `VillagerAutonomousActionSystem.cs`, wire into `VillagerJobSystem.cs`

**PureDOTS Integration:**
- PureDOTS has `VillagerUtilityScheduler` (shared package) with utility calculation stubs
- PureDOTS has `VillagerNeedsHot` component structure
- Need to wire Godgame needs tracking to PureDOTS utility calculations

**Required Work:**
- Implement interrupt handling system
- Expand job scheduler with GOAP hooks
- Implement needs tracking (hunger, fatigue, hygiene, mood)
- Improve storehouse selection with capacity/priority
- Wire autonomous actions into job system
- Add tests: `VillagerInterruptTests.cs`, `VillagerNeedsTests.cs`, `VillagerAutonomousActionTests.cs`

---

## 2. Aggregate Entity Behavior & Registry Coverage

### Current State

**Villages:**
- `VillageComponents.cs`: Core village data (state machine, alignment, initiative, resources, worship, members, events)
- `VillageAITelemetrySystem.cs`: Telemetry emission for village metrics
- `BandFormationSystem.cs`: Band formation detection (stub implementation)
- Registry sync: `GodgameRegistryBridgeSystem` syncs villages to registries (partial)

**Bands:**
- `BandFormationSystem.cs`: Formation candidate detection, probability calculation (stubs)
- `GodgameBand` component exists
- Registry sync: `GodgameRegistryBridgeSystem.UpdateBandRegistry` syncs band data

**Gaps Identified:**

1. **Village Presentation Bindings**
   - No `GodgameVillagePresentation` component
   - No visual mapping (alignment → building style, behavior → expansion visuals)
   - No village UI panels
   - **Files to touch:** New `GodgameVillagePresentation.cs`, `VillagePresentationSystem.cs`

2. **Village Aggregate AI**
   - No village-level decision-making system
   - No project planning (construction, expansion, resource allocation)
   - No crisis response (famine, attacks, disasters)
   - **Files to touch:** New `VillageAIDecisionSystem.cs`, extend `VillageComponents.cs`

3. **Band Presentation Bindings**
   - No `GodgameBandPresentation` component
   - No formation visualization
   - No group movement/combat formation systems
   - **Files to touch:** New `GodgameBandPresentation.cs`, `BandFormationVisualSystem.cs`

4. **Band Aggregate AI**
   - `BandFormationSystem` has stubs but incomplete logic
   - No band goal execution (raiding, exploration, defense)
   - No member coordination/formation management
   - **Files to touch:** Extend `BandFormationSystem.cs`, new `BandAIDecisionSystem.cs`

5. **Guild Support**
   - PureDOTS has `GuildComponents` but Godgame has no guild systems
   - No guild formation, missions, or member management
   - **Files to touch:** New `GuildFormationSystem.cs`, `GuildAIDecisionSystem.cs`

6. **Aggregate Registry Continuity**
   - Village/band telemetry exists but not fully integrated with continuity snapshots
   - Missing aggregate-level spatial sync for villages/bands
   - **Files to touch:** Extend `GodgameRegistryBridgeSystem.cs`, `VillageAITelemetrySystem.cs`

**PureDOTS Integration:**
- PureDOTS has `AggregateBehaviorProfile` and `AggregateBehaviorSystems` (shared package)
- PureDOTS has `VillageBehaviorComponents` and `BandComponents`
- Need to wire Godgame aggregate AI to PureDOTS behavior profiles

**Required Work:**
- Create presentation components for villages/bands/guilds
- Implement village-level AI decision-making
- Complete band formation logic
- Add guild formation/management systems
- Enhance aggregate registry sync with continuity
- Add tests: `VillageAITests.cs`, `BandFormationTests.cs`, `AggregateRegistrySyncTests.cs`

---

## 3. Demo Scene & Content Pipeline Readiness

### Current State

**Scenes:**
- `GodgameBootstrapSubScene.unity`: Bootstrap scene (exists but needs content)
- `GodgameRegistrySubScene.unity`: Registry SubScene (wizard creates it)
- `GodgameRegistrySubSceneWizard.cs`: Editor wizard for creating registry SubScene

**Prefabs:**
- `Assets/Prefabs/Buildings/`: Basic storehouse, temple, fertility statue
- `Assets/Prefabs/Individuals/`: Basic villager prefab
- `Assets/Placeholders/`: Placeholder prefabs (buildings, modules, resource nodes, villagers, wagons)

**Gaps Identified:**

1. **Self-Contained Demo Scene**
   - No complete demo scene with all required entities
   - SubScene wizard creates registry config but not actual entities
   - No demo spawn/bootstrap system
   - **Files to touch:** New `GodgameDemoBootstrapSystem.cs`, create `GodgameDemoScene.unity`

2. **Prefab Pipeline**
   - Placeholder prefabs exist but not wired to final art
   - No prefab authoring components for demo entities
   - Missing prefab variants (different villager types, building styles)
   - **Files to touch:** Extend authoring components, create prefab variants

3. **COZY Weather Integration**
   - Weather system exists (`WeatherBootstrapSystem`, `WeatherControllerSystem`)
   - No actual COZY WeatherProfile assets assigned
   - No ambient loops or special-FX prefabs per biome
   - **Files to touch:** Create weather assets, wire to biome system

4. **Interaction Rigs**
   - Hand/camera authoring exists but not fully wired in demo scene
   - No interaction rig setup in demo scene
   - **Files to touch:** Update demo scene with interaction rigs

5. **Authoring Assets**
   - `PureDotsRuntimeConfig.asset` exists but may need updates
   - `ResourceRecipeCatalog.asset` exists but may need demo recipes
   - Missing demo-specific ScriptableObjects
   - **Files to touch:** Update/create authoring assets

**Required Work:**
- Create self-contained demo scene with all entities
- Wire demo bootstrap system to spawn entities
- Create prefab authoring pipeline for demo content
- Integrate COZY weather assets
- Set up interaction rigs in demo scene
- Create/update authoring assets for demo

---

## 4. PureDOTS Scenario & Time Integration

### Current State

**Scenario Runner:**
- `GodgameScenarioSpawnLoggerSystem.cs`: Logs scenario entity counts (stub)
- PureDOTS has `ScenarioRunnerExecutor` and `ScenarioRunnerEntryPoints`
- PureDOTS has scenario JSON samples

**Time Integration:**
- `TimeControlSystem.cs`: Pause/speed/step/rewind support
- `TimeDemoHistorySystem.cs`: Snapshot/command log
- PureDOTS time spine (`TimeState`, `RewindState`) integrated

**Gaps Identified:**

1. **Scenario Spawn Wiring**
   - `GodgameScenarioSpawnLoggerSystem` only logs, doesn't spawn
   - No adapter to convert scenario JSON counts to actual entity spawns
   - No registry-backed spawn system for scenarios
   - **Files to touch:** Replace `GodgameScenarioSpawnLoggerSystem.cs` with `GodgameScenarioSpawnSystem.cs`

2. **Bootstrap Integration**
   - No adapter to wire Godgame bootstrap to ScenarioRunner
   - No CLI hooks for scenario execution
   - No editor menu items for scenario testing
   - **Files to touch:** New `GodgameScenarioBootstrapAdapter.cs`, add CLI/editor hooks

3. **Time Spine Validation**
   - Time controls work but not fully validated with scenarios
   - No determinism tests for scenario + rewind combination
   - **Files to touch:** New `ScenarioRewindDeterminismTests.cs`

**PureDOTS Integration:**
- PureDOTS has `ScenarioRunnerEntryPoints.RunScenarioFromArgs` for CLI
- PureDOTS has scenario JSON format with entity counts
- Need to map Godgame entity types to scenario counts

**Required Work:**
- Implement scenario spawn system replacing logger
- Create bootstrap adapter for ScenarioRunner
- Add CLI hooks (`--scenario`, `--report`)
- Add editor menu items for scenario testing
- Add determinism tests for scenario + rewind
- Document scenario mapping (registry IDs → entity archetypes)

---

## 5. Testing & Validation

### Current State

**Tests:**
- `GodgameRegistryBridgeTests.cs`: Registry bridge validation
- `Conservation_VillagerGatherDeliver_Playmode`: Resource conservation
- `Jobs_GatherDeliver_StateGraph_Playmode`: State transitions
- `Construction_GhostToBuilt_Playmode`: Construction flow
- `Time_RewindDeterminism_Playmode`: Rewind determinism
- `Presentation_Optionality_Playmode`: Presentation optionality

**Gaps Identified:**

1. **Villager AI Tests**
   - No interrupt handling tests
   - No needs/utility tests
   - No autonomous action tests
   - No job scheduler tests
   - **Files to touch:** New `VillagerInterruptTests.cs`, `VillagerNeedsTests.cs`, `VillagerAutonomousActionTests.cs`, `VillagerJobSchedulerTests.cs`

2. **Aggregate Behavior Tests**
   - No village AI decision tests
   - No band formation tests
   - No aggregate registry sync tests
   - **Files to touch:** New `VillageAITests.cs`, `BandFormationTests.cs`, `AggregateRegistrySyncTests.cs`

3. **Scenario Integration Tests**
   - No scenario spawn tests
   - No scenario + rewind determinism tests
   - **Files to touch:** New `ScenarioSpawnTests.cs`, `ScenarioRewindDeterminismTests.cs`

4. **Demo Scene Tests**
   - No demo bootstrap tests
   - No demo scene validation tests
   - **Files to touch:** New `DemoBootstrapTests.cs`, `DemoSceneValidationTests.cs`

**Required Work:**
- Add villager AI test coverage
- Add aggregate behavior test coverage
- Add scenario integration test coverage
- Add demo scene test coverage
- Update CI commands documentation

---

## 6. Documentation Updates

### Current State

**Documentation:**
- `Docs/DemoReadiness_Status.md`: Current demo status
- `Docs/DemoSceneSetup.md`: Scene setup guide
- `Docs/DemoSystemsOverview.md`: Empty (needs content)
- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`: Integration TODO
- `Docs/Progress.md`: Session log

**Gaps Identified:**

1. **Demo Systems Overview**
   - `Docs/DemoSystemsOverview.md` is empty
   - Needs system architecture diagram
   - Needs data flow documentation
   - **Files to touch:** `Docs/DemoSystemsOverview.md`

2. **AI Integration Guide**
   - No guide for implementing villager AI upgrades
   - No guide for aggregate AI behaviors
   - **Files to touch:** New `Docs/Guides/VillagerAI_Implementation.md`, `Docs/Guides/AggregateAI_Implementation.md`

3. **Scenario Runner Guide**
   - No guide for scenario integration
   - No guide for CLI/editor hooks
   - **Files to touch:** New `Docs/Guides/ScenarioRunner_Integration.md`

**Required Work:**
- Populate `Docs/DemoSystemsOverview.md`
- Create AI implementation guides
- Create scenario runner integration guide
- Update `Docs/DemoReadiness_Status.md` with new findings
- Update `Docs/Progress.md` with session notes

---

## Summary of Critical Gaps

### High Priority (Block Demo Functionality)

1. **Villager AI Interrupts** - Without interrupt handling, villagers can't respond to hand pickups or path blocks
2. **Villager Needs System** - Without needs tracking, villagers are just resource gatherers without autonomy
3. **Aggregate Presentation** - Without presentation bindings, villages/bands are invisible
4. **Demo Scene Bootstrap** - Without bootstrap system, demo scene can't spawn entities
5. **Scenario Spawn Wiring** - Without scenario spawn, headless testing isn't possible

### Medium Priority (Enhance Demo Quality)

1. **Job Scheduler/GOAP** - Improves villager job assignment and planning
2. **Village AI Decisions** - Enables village-level autonomous behaviors
3. **Band Formation Logic** - Completes band formation system
4. **Storehouse Selection** - Improves resource delivery efficiency

### Low Priority (Polish & Future)

1. **Guild Systems** - Future feature, not required for demo
2. **COZY Weather Assets** - Visual polish, not required for functionality
3. **Prefab Variants** - Content expansion, not required for basic demo

---

## Next Steps

1. **Immediate**: Implement villager interrupt handling and needs system
2. **Short-term**: Create demo scene bootstrap and scenario spawn wiring
3. **Medium-term**: Add aggregate presentation bindings and AI decision systems
4. **Long-term**: Expand test coverage and documentation

---

**Related Documentation:**
- `Docs/DemoReadiness_Status.md` - Current demo status
- `Docs/DemoSceneSetup.md` - Scene setup guide
- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Integration TODO
- `Docs/Guides/Godgame_PureDOTS_Entity_Mapping.md` - Entity mapping guide

