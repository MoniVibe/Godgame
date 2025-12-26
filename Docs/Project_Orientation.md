# Godgame Project Orientation

**Date:** 2025-01-24  
**Purpose:** Comprehensive project overview for agents preparing to flesh out gameplay

---

## Project Overview

**Godgame** is a Unity DOTS (Data-Oriented Technology Stack) project implementing a god-game simulation where players influence villagers, villages, and bands through divine intervention. The project uses a shared **PureDOTS** package (`com.moni.puredots`) that provides core infrastructure (registries, time spine, telemetry, spatial grid) consumed by Godgame gameplay systems.

### Architecture Pattern

The project follows a **truth source architecture**:

1. **PureDOTS Runtime Contracts (Truth)**: Core singletons and registries defined in `Packages/com.moni.puredots/`
2. **Godgame Mirror Components**: Burst-friendly gameplay data under `Assets/Scripts/Godgame/`
3. **Sync Systems**: Bridge systems that translate mirrors → PureDOTS registries
4. **Registry Bridge**: Orchestrates all sync systems and registers with PureDOTS directory
5. **Presentation/Agents**: Consumers (HUD, debug, AI) read registries and telemetry

---

## PureDOTS Package

**Location**: `../../PureDOTS/Packages/com.moni.puredots` (relative to Godgame project root)

**Key Systems**:
- **Registries**: Villager, Storehouse, Resource, Band, Miracle, Spawner, LogisticsRequest, Construction
- **Time Spine**: `TimeState`, `RewindState`, snapshot/command log for deterministic rewinding
- **Spatial Grid**: Spatial partitioning for efficient entity queries
- **Telemetry**: `TelemetryStream` + `TelemetryMetric` for metrics/debugging
- **Bootstrap**: `CoreSingletonBootstrapSystem` seeds all singletons at startup

**PureDOTS Integration Points**:
- Registry contracts defined in PureDOTS (`VillagerRegistryEntry`, `StorehouseRegistryEntry`, etc.)
- Godgame sync systems mirror gameplay data into these registries
- Time controls consume PureDOTS time spine for deterministic rewinding
- Spatial grid used for efficient entity queries across registries

---

## Godgame Codebase Structure

### Core Directories

```
Assets/Scripts/Godgame/
├── Registry/          # Registry bridge and sync systems
├── legacy/             # legacy scene bootstrap and settlement systems
├── Villagers/        # Villager AI, jobs, personality (if exists)
├── Bands/            # Band formation and aggregation
├── Miracles/          # Miracle system (input, release, presentation)
├── Environment/      # Weather, biomes, time-of-day
├── Fauna/            # Ambient fauna systems
├── Economy/           # Resource components
├── Logistics/         # Logistics request components
├── Spawners/          # Spawner components
├── Biomes/            # Biome terrain and binding
├── Presentation/      # Presentation bindings (optional)
└── Tests/             # NUnit test fixtures
```

### Key Systems Implemented

#### ✅ Registry Bridge (`Registry/`)
- **GodgameRegistryBridgeSystem**: Orchestrates all sync systems
- **Sync Systems**: `GodgameVillagerSyncSystem`, `GodgameStorehouseSyncSystem`, `GodgameBandSyncSystem`, `GodgameMiracleSyncSystem`, `GodgameSpawnerSyncSystem`, `GodgameLogisticsSyncSystem`
- **Telemetry**: `GodgameRegistryTelemetrySystem` emits metrics to PureDOTS telemetry stream
- **Status**: Fully functional, all registries sync to PureDOTS

#### ✅ Time Controls (`Time/`)
- **TimeControlSystem**: Pause/speed/step/rewind support
- **TimeDemoHistorySystem**: Snapshot/command log for rewinding
- **Status**: Functional, Burst-compiled, tested

#### ✅ Construction (`Construction/`)
- **ConstructionSystem**: Ghost placement → payment → completion flow
- **Status**: Functional, parallel job + sequential payment, tested

#### ✅ Module Refit (`Modules/`)
- **ModuleRefitSystem**: Parallel module maintenance with resource wallet gating
- **ModuleDegradationSystem**: Applies wear, queues repairs
- **Status**: Functional, fully parallelized, tested

#### ✅ Resource Systems (`Economy/`, `Resources/`)
- **StorehouseApi**: Resource deposit/withdrawal API
- **AggregatePileSystem**: Resource pile merge/split, pooling
- **StorehouseOverflowToPileSystem**: Reroutes overflow into piles
- **Status**: Functional, tested

#### ✅ Weather & Biomes (`Environment/`, `Biomes/`)
- **WeatherBootstrapSystem**: Seeds weather state from PureDOTS climate
- **WeatherControllerSystem**: Updates weather based on time/biome
- **WeatherPresentationSystem**: Emits presentation commands
- **BiomeTerrainAgent**: Biome resolution and terrain binding
- **Status**: Functional, presentation bindings optional

#### ✅ Miracles (`Miracles/`)
- **GodgameMiracleInputSystem**: Handles miracle input
- **GodgameMiracleReleaseSystem**: Releases miracles with cost/cooldown
- **MiraclePresentation**: Presentation bindings for effects
- **Status**: Functional, syncs to PureDOTS MiracleRegistry

#### ✅ legacy Systems (`legacy/`)
- **SettlementSpawnSystem**: Spawns legacy entities
- **DemoVillagerBehaviorSystem**: legacy villager behaviors
- **GroundTileSystems**: Procedural ground tile generation
- **Status**: Functional, used in legacy scenes

---

## Current Implementation Status

### ✅ Completed (Burst-Compiled, Tested)

1. **Registry Bridge**: All registries (Villager, Storehouse, Band, Miracle, Spawner, Logistics, Construction) sync to PureDOTS
2. **Time Controls**: Pause/speed/step/rewind with determinism tests
3. **Construction**: Ghost → payment → completion flow
4. **Module Refit**: Parallel maintenance with resource gating
5. **Resource Systems**: Storehouse API, aggregate piles, overflow handling
6. **Weather/Biomes**: Climate integration, presentation bindings
7. **Miracles**: Input/release with registry sync
8. **Spatial Grid**: Integrated with PureDOTS spatial service

### ⚠️ Partially Implemented

1. **Villager AI**: 
   - Basic job loop exists (Idle → Navigate → Gather → Deliver)
   - Missing: Interrupt handling, needs system, job scheduler, GOAP hooks
   - Location: May be in PureDOTS package or needs creation

2. **Aggregate Behaviors**:
   - Village/Band components exist
   - Missing: Presentation bindings, aggregate AI decision-making, full registry sync
   - Location: `Bands/`, `Villages/` (if exists)

3. **legacy Scene**:
   - SubScene wizard exists
   - Missing: Self-contained legacy scene, bootstrap system, prefab pipeline

4. **Scenario Runner**:
   - Logger system exists (stub)
   - Missing: Actual spawn system, bootstrap adapter, CLI hooks

### ❌ Not Yet Implemented

1. **Villager Interrupts**: Hand pickup, path blocked detection
2. **Villager Needs**: Hunger, fatigue, hygiene, mood tracking
3. **Job Scheduler**: GOAP hooks, job assignment buffer
4. **Aggregate Presentation**: Village/band visual bindings
5. **Guild Systems**: Formation, missions, member management
6. **Input Routing**: Time controls via Interaction system (currently uses singleton)

---

## Key Gaps for Gameplay Fleshing

### High Priority (Block Core Functionality)

1. **Villager AI Interrupts** (`Assets/Scripts/Godgame/Villagers/`)
   - Hand pickup interruption system
   - Path-blocked detection/resolution
   - Job cancellation/resume logic

2. **Villager Needs System** (`Assets/Scripts/Godgame/Villagers/`)
   - Needs components (hunger, fatigue, hygiene, mood)
   - Needs consumption systems
   - Utility curve integration

3. **legacy Scene Bootstrap** (`Assets/Scripts/Godgame/legacy/`)
   - Self-contained legacy scene
   - Entity spawn/bootstrap system
   - Prefab authoring pipeline

4. **Scenario Spawn Wiring** (`Assets/Scripts/Godgame/Scenario/`)
   - Replace logger with actual spawn system
   - Bootstrap adapter for ScenarioRunner
   - CLI hooks (`--scenario`, `--report`)

### Medium Priority (Enhance Quality)

1. **Job Scheduler/GOAP** (`Assets/Scripts/Godgame/Villagers/`)
   - Job assignment buffer system
   - GOAP hooks for goal-oriented planning
   - Priority/capacity logic for node selection

2. **Village AI Decisions** (`Assets/Scripts/Godgame/Villages/`)
   - Village-level decision-making
   - Project planning (construction, expansion)
   - Crisis response (famine, attacks, disasters)

3. **Band Formation Logic** (`Assets/Scripts/Godgame/Bands/`)
   - Complete band formation system
   - Band goal execution (raiding, exploration, defense)
   - Member coordination/formation management

4. **Storehouse Selection** (`Assets/Scripts/Godgame/Resources/`)
   - Capacity/priority logic
   - Reservation system for incoming deliveries

### Low Priority (Polish & Future)

1. **Guild Systems**: Formation, missions, member management
2. **COZY Weather Assets**: Visual polish integration
3. **Prefab Variants**: Content expansion
4. **Input Bindings**: Keyboard/gamepad routing
5. **HUD Display**: UI for `HudEvent` buffer

---

## Testing Infrastructure

### Test Location
`Assets/Scripts/Godgame/Tests/`

### Existing Tests
- ✅ `GodgameRegistryBridgeTests`: Registry bridge validation
- ✅ `Conservation_VillagerGatherDeliver_Playmode`: Resource conservation
- ✅ `Jobs_GatherDeliver_StateGraph_Playmode`: State transitions
- ✅ `Construction_GhostToBuilt_Playmode`: Construction flow
- ✅ `Time_RewindDeterminism_Playmode`: Rewind determinism
- ✅ `Presentation_Optionality_Playmode`: Presentation optionality

### Missing Test Coverage
- Villager AI (interrupts, needs, autonomous actions)
- Aggregate behaviors (village AI, band formation)
- Scenario integration (spawn, rewind determinism)
- legacy scene (bootstrap, validation)

### CI Commands
See `Docs/CI_Commands.md` for:
- Test execution commands
- Burst compilation checks
- Build validation

---

## PureDOTS Integration Patterns

### Registry Sync Pattern

1. **Godgame Components**: Define gameplay data (e.g., `GodgameVillager`)
2. **Mirror Components**: Burst-friendly mirrors (e.g., `VillagerMirror`)
3. **Sync System**: `GodgameVillagerSyncSystem` translates mirrors → `VillagerRegistryEntry`
4. **Registry Bridge**: `GodgameRegistryBridgeSystem` registers with `RegistryDirectory`

### Time Integration Pattern

1. **PureDOTS Time Spine**: `TimeState`, `RewindState` singletons
2. **Godgame Time Controls**: `TimeControlSystem` reads/writes time state
3. **History System**: `TimeDemoHistorySystem` records snapshots/commands
4. **Determinism**: Rewind → resim → assert state matches

### Telemetry Pattern

1. **Godgame Systems**: Emit metrics via `TelemetryMetric` buffer
2. **PureDOTS Telemetry**: `TelemetryStream` singleton aggregates metrics
3. **HUD/Debug**: Consumers read telemetry stream for visualization

---

## Development Workflow

### Starting Work
1. Check `Docs/Progress.md` for active lanes (A/B/C)
2. Log start in `Docs/Progress.md` with lane and focus
3. Update relevant TODO (`Godgame_PureDOTS_Integration_TODO.md` or `Phase2_Demo_TODO.md`)

### During Work
1. Follow Burst compliance: `[BurstCompile]`, parallel `IJobEntity`, no managed allocations
2. Use PureDOTS contracts: Don't duplicate schemas, use shared registries
3. Add tests: At least one PlayMode/EditMode test per feature
4. Update docs: Cross-reference PureDOTS APIs, document new systems

### Finishing Work
1. Run tests: Use CI commands from `Docs/CI_Commands.md`
2. Update TODOs: Mark completed items, note blockers
3. Log end in `Docs/Progress.md` with progress, tests run, next steps

---

## Key Documentation

- **Entry Point**: `AGENTS.md` (workflow + commands)
- **Active TODOs**: `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`
- **Truth Sources**: `Docs/TruthSources_QuickReference.md`
- **Progress**: `Docs/Progress.md` (session log)
- **legacy Status**: `Docs/Archive/Demo_Legacy/DemoReadiness_Status.md`
- **Gap Analysis**: `Docs/Archive/Demo_Legacy/DemoReadiness_GapAnalysis.md`
- **System Overview**: `Docs/Archive/Demo_Legacy/DemoSystemsOverview.md`

---

## Next Steps for Gameplay Fleshing

### Recommended Starting Points

1. **Villager AI Expansion** (High Priority)
   - Implement interrupt handling system
   - Add needs tracking (hunger, fatigue, hygiene, mood)
   - Wire needs into utility scheduler
   - Add tests: `VillagerInterruptTests`, `VillagerNeedsTests`

2. **Scenario Bootstrap** (High Priority)
   - Create self-contained scenario scene
   - Implement scenario bootstrap spawn system
   - Wire prefab authoring pipeline
   - Add tests: `ScenarioBootstrapTests`

3. **Job Scheduler** (Medium Priority)
   - Implement job assignment buffer
   - Add GOAP hooks (if PureDOTS provides)
   - Improve node/storehouse selection logic
   - Add tests: `VillagerJobSchedulerTests`

4. **Aggregate AI** (Medium Priority)
   - Implement village-level decision-making
   - Complete band formation logic
   - Add presentation bindings
   - Add tests: `VillageAITests`, `BandFormationTests`

---

## PureDOTS Package Reference

**Location**: `../../PureDOTS/Packages/com.moni.puredots`

**Key Namespaces**:
- `PureDOTS.Runtime.Registry`: Registry contracts and builders
- `PureDOTS.Runtime.Time`: Time spine (`TimeState`, `RewindState`)
- `PureDOTS.Runtime.Spatial`: Spatial grid (`SpatialGridState`, `SpatialGridResidency`)
- `PureDOTS.Runtime.Telemetry`: Telemetry stream (`TelemetryStream`, `TelemetryMetric`)
- `PureDOTS.Systems.CoreSingletonBootstrapSystem`: Bootstrap system

**Registry Contracts**:
- `VillagerRegistryEntry`: Availability, discipline, AI state, health/morale/energy
- `StorehouseRegistryEntry`: Per-resource capacity summaries
- `BandRegistryEntry`: Formation, morale, members
- `MiracleRegistryEntry`: Cooldown, charge, target
- `SpawnerRegistryEntry`: Spawn rate, capacity
- `LogisticsRequestRegistryEntry`: Request type, priority, status
- `ConstructionRegistryEntry`: Progress, cost, completion

---

## Code Style & Conventions

- **Indentation**: 4 spaces
- **Namespaces**: `Godgame.*` for gameplay, `PureDOTS.*` for shared package
- **Naming**: PascalCase for types/methods, camelCase for locals
- **Burst**: All hot paths `[BurstCompile]`, use `IJobEntity` for parallel work
- **Strings**: Use `FixedString64Bytes` for DOTS-compatible strings
- **Documentation**: XML comments (`///`) for public APIs

---

## Conclusion

The Godgame project has a **solid foundation** with core systems (registries, time, construction, modules) implemented and tested. The main gaps are in **villager AI** (interrupts, needs, scheduling) and **aggregate behaviors** (village/band AI, presentation). The PureDOTS integration is **complete** for existing systems, providing a stable base for gameplay expansion.

**Ready for**: Villager AI expansion, legacy scene bootstrap, job scheduler, aggregate AI decision-making.

**Blockers**: None identified. All core infrastructure is in place.

---

**Related Documentation**:
- `Docs/Archive/Demo_Legacy/DemoReadiness_Status.md` - Current legacy status
- `Docs/Archive/Demo_Legacy/DemoReadiness_GapAnalysis.md` - Detailed gap analysis
- `Docs/Archive/Demo_Legacy/DemoSystemsOverview.md` - System architecture
- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Integration TODO
- `Docs/TruthSources_QuickReference.md` - Truth source quick reference
