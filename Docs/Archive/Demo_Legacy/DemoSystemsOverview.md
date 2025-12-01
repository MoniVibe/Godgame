# Demo Systems Overview

**Date:** 2025-01-24  
**Status:** Updated - Comprehensive System Architecture  
**Purpose:** Overview of all systems involved in the Godgame demo

---

## System Architecture

### Core Systems (Always Active)

#### 1. PureDOTS Bootstrap
- **System**: `CoreSingletonBootstrapSystem` (PureDOTS)
- **Purpose**: Seeds singletons (TimeState, RewindState, registries, telemetry)
- **Location**: `Packages/com.moni.puredots/Runtime/Systems/`
- **Dependencies**: None (runs first)

#### 2. Registry Bridge
- **System**: `GodgameRegistryBridgeSystem`
- **Purpose**: Mirrors Godgame entities to PureDOTS registries
- **Location**: `Assets/Scripts/Godgame/Registry/Registry/`
- **Dependencies**: PureDOTS bootstrap, spatial grid
- **Registries**: Villager, Storehouse, Resource, Band, Miracle, Spawner, Logistics, Construction

#### 3. Spatial Grid
- **System**: `SpatialGridBuildSystem`, `SpatialGridSyncSystem` (PureDOTS)
- **Purpose**: Spatial partitioning for efficient queries
- **Location**: `Packages/com.moni.puredots/Runtime/Spatial/`
- **Dependencies**: PureDOTS bootstrap
- **Authoring**: `SpatialPartitionAuthoring` + `GodgameSpatialProfile.asset`

#### 4. Time Controls
- **System**: `TimeControlSystem`
- **Purpose**: Pause/speed/step/rewind support
- **Location**: `Assets/Scripts/Godgame/Time/`
- **Dependencies**: PureDOTS time spine (`TimeState`, `RewindState`)
- **Features**: Pause toggle, speed multiplier (0.25x - 8x), step, rewind hold

#### 5. Villager Job Loop
- **System**: `VillagerJobSystem`
- **Purpose**: Handles villager gather/deliver state machine
- **Location**: `Assets/Scripts/Godgame/Villagers/`
- **Dependencies**: Resource catalog, storehouse API
- **States**: Idle → NavigateToNode → Gather → NavigateToStorehouse → Deliver

#### 6. Construction System
- **System**: `ConstructionSystem`
- **Purpose**: Processes construction payment/completion
- **Location**: `Assets/Scripts/Godgame/Construction/`
- **Dependencies**: Storehouse API, registry bridge
- **Flow**: Ghost placement → Resource payment → Completion → Telemetry

#### 7. Module Refit
- **System**: `ModuleRefitSystem`
- **Purpose**: Parallel module maintenance
- **Location**: `Assets/Scripts/Godgame/Modules/`
- **Dependencies**: Module components, resource wallets
- **Features**: Parallel `IJobEntity`, thread-safe telemetry

---

### Villager AI Systems

#### 8. Personality System
- **System**: `VillagerPersonalitySystem`
- **Purpose**: Processes grudge generation/decay, patriotism drift, combat stance modifiers
- **Location**: `Assets/Scripts/Godgame/Villagers/`
- **Dependencies**: Time state
- **Features**: Grudge decay, patriotism updates, combat modifiers

#### 9. Utility Scheduler
- **System**: `VillagerUtilityScheduler`
- **Purpose**: Utility-based need/job priority system
- **Location**: `Assets/Scripts/Godgame/Villagers/`
- **Dependencies**: Initiative state, personality, alignment
- **Features**: Initiative calculation, utility functions (stubs for needs/job selection)

#### 10. Village AI Telemetry
- **System**: `VillageAITelemetrySystem`
- **Purpose**: Publishes village AI telemetry to shared stream
- **Location**: `Assets/Scripts/Godgame/Registry/`
- **Dependencies**: Village components, telemetry stream
- **Metrics**: Initiative, alignment, state, events

---

### Aggregate Systems

#### 11. Band Formation
- **System**: `BandFormationSystem`
- **Purpose**: Detects band formation candidates and creates bands
- **Location**: `Assets/Scripts/Godgame/Bands/`
- **Dependencies**: Time state, villager components
- **Status**: Stub implementation (needs completion)

#### 12. Village Components
- **Components**: `VillageComponents.cs`
- **Purpose**: Village data structures (state, alignment, resources, members)
- **Location**: `Assets/Scripts/Godgame/Villages/`
- **Dependencies**: None (data only)
- **Status**: Components exist, aggregation system needed

---

### Resource Systems

#### 13. Storehouse API
- **System**: `StorehouseApi` (static utility)
- **Purpose**: Resource deposit/withdrawal API
- **Location**: `Assets/Scripts/Godgame/Resources/`
- **Dependencies**: Resource catalog
- **Features**: TryDeposit, TryWithdraw, capacity checks

#### 14. Aggregate Pile System
- **System**: `AggregatePileSystem`
- **Purpose**: Handles resource pile merge/split, pooling
- **Location**: `Assets/Scripts/Godgame/Resources/`
- **Dependencies**: Resource catalog
- **Features**: Spawn commands, merge/split, conservation

#### 15. Storehouse Overflow
- **System**: `StorehouseOverflowToPileSystem`
- **Purpose**: Reroutes dump overflow into piles
- **Location**: `Assets/Scripts/Godgame/Resources/`
- **Dependencies**: Storehouse API, aggregate pile system
- **Features**: Overflow detection, pile creation

---

### Presentation Systems (Optional)

#### 16. Presentation Binding Bootstrap
- **System**: `GodgamePresentationBindingBootstrapSystem`
- **Purpose**: Seeds placeholder effect IDs
- **Location**: `Assets/Scripts/Godgame/Presentation/`
- **Dependencies**: None (optional)
- **Features**: Miracle ping, jobsite ghost, module refit sparks, hand affordance

#### 17. Presentation System
- **System**: `PresentationSystem` (PureDOTS)
- **Purpose**: Processes presentation commands
- **Location**: `Packages/com.moni.puredots/Runtime/Presentation/`
- **Dependencies**: Presentation bridge (optional)
- **Features**: Effect requests, visual updates

---

### Demo/Scenario Systems

#### 18. Demo Bootstrap
- **System**: `GodgameDemoBootstrapSystem` (planned)
- **Purpose**: Spawns demo entities at scene start
- **Location**: `Assets/Scripts/Godgame/Scenario/` (to be created)
- **Dependencies**: PureDOTS bootstrap, registry bridge
- **Status**: Planned (see `Docs/Guides/DemoScene_ContentPipeline.md`)

#### 19. Scenario Spawn
- **System**: `GodgameScenarioSpawnSystem` (planned, replaces logger)
- **Purpose**: Spawns entities from scenario JSON
- **Location**: `Assets/Scripts/Godgame/Scenario/` (to be created)
- **Dependencies**: ScenarioRunner, registry bridge
- **Status**: Planned (see `Docs/Guides/ScenarioRunner_Integration.md`)

---

## Data Flow

### Entity Lifecycle
1. **Authoring**: Prefabs/SubScenes with authoring components
2. **Baking**: Authoring → Runtime components (via bakers)
3. **Mirror Sync**: Runtime components → Mirror components (`Godgame*SyncSystem`)
4. **Registry Bridge**: Mirror components → Registry entries (`GodgameRegistryBridgeSystem`)
5. **Telemetry**: Registry/metrics → Telemetry stream (`TelemetryMetric` buffer)

### Villager Job Flow
1. **Idle**: Villager in `Idle` phase, `VillagerJobSystem` finds nearest resource node
2. **Navigate**: Villager moves toward node (`Navigation` component)
3. **Gather**: Villager gathers resources, depletes node
4. **Navigate**: Villager moves toward nearest storehouse
5. **Deliver**: Villager deposits resources via `StorehouseApi`
6. **Idle**: Cycle repeats

### Construction Flow
1. **Placement**: Player places construction ghost (`PlaceConstructionRequest` buffer)
2. **Payment**: `ConstructionSystem` withdraws resources from storehouses
3. **Completion**: When `Paid >= Cost`, ghost converts to built entity
4. **Telemetry**: Completion fires telemetry events

### Time Control Flow
1. **Input**: `TimeControlInput` singleton receives input
2. **Command**: `TimeControlSystem` processes commands (pause/speed/step/rewind)
3. **State Update**: `TimeState` and `RewindState` updated
4. **HUD Events**: `HudEvent` buffer emitted for UI feedback
5. **Rewind**: `TimeDemoHistorySystem` handles snapshot/command log

---

## System Dependencies

### Dependency Graph
```
CoreSingletonBootstrapSystem (PureDOTS)
├── SpatialGridBuildSystem (PureDOTS)
├── GodgameRegistryBridgeSystem
│   ├── GodgameVillagerSyncSystem
│   ├── GodgameStorehouseSyncSystem
│   └── ... (other sync systems)
├── TimeControlSystem
│   └── TimeDemoHistorySystem
├── VillagerJobSystem
│   ├── StorehouseApi
│   └── ResourceTypeIndex
├── ConstructionSystem
│   └── StorehouseApi
└── PresentationSystem (optional)
    └── GodgamePresentationBindingBootstrapSystem
```

---

## Burst Compliance

All gameplay systems are `[BurstCompile]`:
- `VillagerJobSystem`: Parallel `IJobEntity`
- `ConstructionSystem`: Parallel job + sequential payment
- `TimeControlSystem`: Burst-safe (no EntityManager calls)
- `ModuleRefitSystem`: Parallel `IJobEntity`
- `VillagerPersonalitySystem`: Parallel `IJobEntity`
- `VillagerUtilityScheduler`: Parallel `IJobEntity`

---

## Performance Characteristics

- **Villager Job System**: Parallel `IJobEntity`, processes all villagers concurrently
- **Construction System**: Parallel job for completion checks, sequential payment pass
- **Module Refit**: Fully parallelized with thread-safe counters
- **Time Control**: Burst-safe singleton updates
- **Registry Bridge**: Sequential pass (acceptable for demo scale)

---

## Known Limitations

1. **Construction Payment**: Sequential pass required for storehouse iteration (acceptable for demo scale)
2. **Resource Node Sync**: Component lookup updates sync after job completes (minor delay acceptable)
3. **Storehouse Selection**: Simple distance-based selection (no capacity/priority logic yet)
4. **Villager Job Assignment**: No GOAP hooks or job scheduler yet (basic state machine only)
5. **Rewind GC**: No memory budget guards or GC policy (acceptable for demo)

---

## Related Documentation

- `Docs/DemoReadiness_Status.md` - Demo readiness status
- `Docs/DemoSceneSetup.md` - Scene setup guide
- `Docs/DemoReadiness_GapAnalysis.md` - Gap analysis
- `Docs/CI_Commands.md` - CI commands reference

