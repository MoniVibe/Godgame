# Demo Scene Setup Guide

## Overview

The Godgame demo scene demonstrates a complete gameplay loop: villager gather → storehouse → construction → time control. All systems are Burst-compiled and run headless or with placeholder presentation.

## Scene Structure

### Main Scene: `Assets/Scenes/SampleScene.unity`

The demo scene should contain:

1. **PureDOTS Bootstrap GameObject**
   - `PureDotsConfigAuthoring` component
   - Resource catalog configured (wood, stone, food)
   - Time/rewind systems enabled

2. **Camera & Lighting**
   - Main Camera (for presentation bridge)
   - Directional Light

3. **Demo Entities (via SubScene or runtime spawn)**
   - Resource nodes (wood/stone)
   - Storehouses (with capacity buffers)
   - Villagers (with `VillagerJobState` components)
   - Construction sites (ghosts)

## Required Systems

All systems run in `FixedStepSimulationSystemGroup` for determinism:

### Core Systems (always active)
- `CoreSingletonBootstrapSystem` - Seeds singletons (TimeState, RewindState, registries)
- `GodgameRegistryBridgeSystem` - Mirrors entities to registries
- `VillagerJobSystem` - Handles villager gather/deliver loop
- `ConstructionSystem` - Processes construction payment/completion
- `TimeControlSystem` - Handles time controls (pause/speed/rewind)
- `ModuleRefitSystem` - Parallel module maintenance

### Optional Systems (presentation)
- `GodgamePresentationBindingBootstrapSystem` - Seeds placeholder effect IDs
- `PresentationSystem` - Processes presentation commands (if bridge enabled)

## Startup Checklist

1. **Bootstrap Singletons**
   - `CoreSingletonBootstrapSystem.EnsureSingletons()` called in tests
   - In editor: systems auto-bootstrap on play

2. **Resource Catalog**
   - `ResourceTypeIndex` singleton with catalog blob
   - Catalog contains: "wood", "stone", "food" (or custom types)

3. **Time State**
   - `TimeState` singleton (tick, speed multiplier, paused)
   - `RewindState` singleton (mode, playback tick)

4. **Telemetry**
   - `TelemetryStream` singleton entity with `TelemetryMetric` buffer

5. **HUD Events** (optional)
   - Entity with `HudEvent` buffer for time control feedback

## Demo Flow

1. **Villager Gather Loop**
   - Villagers in `Idle` phase find nearest resource node
   - Navigate to node → Gather → Navigate to storehouse → Deliver
   - Resources conserved: `sum(gathered) == sum(stored)`

2. **Construction Payment**
   - Place construction ghost via `PlaceConstructionRequest` buffer
   - System withdraws resources from storehouses
   - When `Paid >= Cost`, ghost converts to built entity
   - Telemetry fires: `construction.completed`

3. **Time Controls**
   - Input: `TimeControlInput` singleton
   - Pause toggle, speed delta, step, rewind hold
   - HUD events emitted for UI feedback

## Headless Mode

To run headless (no presentation):

1. Disable `GodgamePresentationBindingBootstrapSystem`
2. Systems continue running; presentation commands queued but not processed
3. Tests run headless by default

## Testing

### EditMode Tests
- `Registry_AuthoringToRuntime_Mirrors` - Validates authoring → registry flow
- `Jobs_Throughput_Telemetry_Editmode` - Validates telemetry counters

### PlayMode Tests
- `Conservation_VillagerGatherDeliver_Playmode` - Resource conservation
- `Jobs_GatherDeliver_StateGraph_Playmode` - State transitions
- `Construction_GhostToBuilt_Playmode` - Construction flow
- `Time_RewindDeterminism_Playmode` - Rewind determinism
- `Presentation_Optionality_Playmode` - Presentation optionality

## Burst Compliance

All gameplay systems are `[BurstCompile]`:
- `VillagerJobSystem` - Parallel `IJobEntity`
- `ConstructionSystem` - Parallel job + sequential payment
- `TimeControlSystem` - Burst-safe (no EntityManager calls)
- `ModuleRefitSystem` - Parallel `IJobEntity`

## Known Limitations

1. **Construction Payment**: Sequential pass required for storehouse iteration (acceptable for demo)
2. **Resource Node Depletion**: Modified via component lookup (syncs after job completes)
3. **Storehouse Selection**: Finds nearest by distance (no capacity/priority logic yet)

## Next Steps

- Add input bindings for time controls (keyboard/gamepad)
- Wire HUD display for time state
- Expand construction to support multiple resource types
- Add villager job assignment system (GOAP hooks)

