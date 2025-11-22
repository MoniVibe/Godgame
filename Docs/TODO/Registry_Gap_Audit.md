# Registry Gap Audit - Godgame ↔ PureDOTS Integration

**Date:** 2025-01-XX  
**Purpose:** Document missing registry bridge implementations and concrete deliverables for AI integration kickoff.

## Current State

### Existing PureDOTS Registry Contracts

PureDOTS defines the following registry kinds (from `RegistryUtilities.cs`):
- `Villager` (1) - ✅ Has `VillagerRegistryEntry` and `VillagerRegistrySystem`
- `Resource` (2) - ✅ Has `ResourceRegistryEntry` and `ResourceRegistrySystem`
- `Storehouse` (3) - ✅ Has `StorehouseRegistryEntry` and `StorehouseRegistrySystem`
- `Miracle` (4) - ✅ Has `MiracleRegistryEntry` and `MiracleRegistrySystem`
- `Band` (12) - ✅ Has `BandRegistryEntry` and `BandRegistrySystem`
- `Spawner` (14) - ✅ Has `SpawnerRegistryEntry` and `SpawnerRegistrySystem`
- `LogisticsRequest` (10) - ✅ Has `LogisticsRequestRegistryEntry` and system
- `Construction` (11) - ✅ Has `ConstructionRegistryEntry` and system

### Godgame Side Status

**Status:** Registry bridge systems live under `Assets/Scripts/Godgame/Registry/Registry/` (`GodgameRegistryBridgeSystem` orchestrator plus villager/storehouse/band/miracle/spawner/logistics sync systems). They normalise component data and feed the shared PureDOTS registries; scene authoring still needs to supply real authored entities.

**Required Bridge Systems:**
1. `GodgameVillagerSyncSystem` - Sync Godgame villager entities → `VillagerRegistryEntry` (implemented; uses mirror component)
2. `GodgameStorehouseSyncSystem` - Sync Godgame storehouse entities → `StorehouseRegistryEntry` (implemented; aggregates reservations/inventory)
3. `GodgameBandSyncSystem` - Sync Godgame band entities → `BandRegistryEntry` (implemented; mirrors formation/flags)
4. `GodgameMiracleSyncSystem` - Sync Godgame miracle entities → `MiracleRegistryEntry` (implemented; normalizes cost/state/targets before registry)
5. `GodgameSpawnerSyncSystem` - Sync Godgame spawner entities → `SpawnerRegistryEntry` (implemented; mirrors cooldown/state)
6. `GodgameLogisticsSyncSystem` - Sync Godgame logistics → `LogisticsRequestRegistryEntry` (implemented; clamps resource indices/positions)
7. `GodgameRegistryBridgeSystem` - Orchestrates all sync systems and registers with PureDOTS directory (implemented; builds registry buffers)

## Concrete Deliverables

### Phase 1: Core Bridge Infrastructure (Immediate)

1. **Create `Godgame/Assets/Scripts/Godgame/Registry/` directory structure**
   - `GodgameRegistryBridgeSystem.cs` - Main orchestrator ✅ (stub only)
   - `GodgameVillagerSyncSystem.cs` - Villager sync (STUB) ✅
   - `GodgameStorehouseSyncSystem.cs` - Storehouse sync (STUB) ✅
   - `GodgameBandSyncSystem.cs` - Band sync (STUB) ✅
   - `GodgameMiracleSyncSystem.cs` - Miracle sync (STUB) ✅
   - `GodgameSpawnerSyncSystem.cs` - Spawner sync (STUB) ✅
   - `GodgameLogisticsSyncSystem.cs` - Logistics sync (STUB) ✅

2. **Authoring Components**
   - `Godgame/Assets/Scripts/Godgame/Registry/Authoring/RegistryAuthoring.cs` - Base authoring for registry-tagged entities
   - Extend existing authoring components to include registry tags

### Phase 2: PureDOTS AI Scaffolding (Parallel Work)

1. **Villager AI Components** (`PureDOTS/.../Runtime/Villagers/`)
   - `VillagerArchetypeCatalog.cs` - ScriptableObject for archetype definitions
   - `VillagerArchetypeCatalogBaker.cs` - Blob baker for runtime data
   - `VillagerUtilityScheduler.cs` - Utility-based need/job priority system (STUB)
   - `VillagerJobExecutionInterface.cs` - Modular job behavior interface

2. **Combat Components** (`PureDOTS/.../Runtime/Combat/`)
   - `CombatStats.cs` - Base attributes, derived stats, equipment
   - `CombatResolutionSystem.cs` - Placeholder resolver (logs unimplemented)

3. **Aggregate Components** (`PureDOTS/.../Runtime/Aggregates/`)
   - `BandComponents.cs` - Band formation, membership, leadership
   - `GuildComponents.cs` - Guild structure, membership, governance
   - `BandFormationSystem.cs` - Candidate detection (STUB)
   - `GuildFormationSystem.cs` - Guild creation (STUB)

### Phase 3: Test & Scene Integration

1. **Tests**
   - `Godgame/Assets/Scripts/Godgame/Tests/GodgameRegistryBridgeTests.cs` - Verify bridge systems exist ✅ (stub coverage)
   - `PureDOTS/Assets/Tests/VillagerAIScaffoldTests.cs` - Verify AI scaffolding compiles

2. **Scene Updates**
   - Update sample SubScenes to include registry authoring components
   - Ensure entities are tagged for spatial grid participation

### Phase 3: Test & Scene Integration (current sweep)

1. **Tests**
   - `Assets/Scripts/Godgame/Tests/GodgameRegistryBridgeTests.cs` ensures bridge/sync systems instantiate, registries expose metadata/buffers, RegistryDirectory handles exist (Villager/Storehouse/Band/Miracle/Spawner/Logistics/Construction), and a sample logistics request flows through sync → registry buffer. Still needs authored-scene coverage for villagers/storehouses/bands/miracles/spawners.
2. **Scene Updates**
   - TODO: Add registry authoring and spatial tags to sample SubScenes so bridge tests can ingest authored data.
   - Added `Tools ▸ Godgame ▸ Create Registry SubScene…` editor wizard (`Assets/Scripts/Godgame/Editor/GodgameRegistrySubSceneWizard.cs`) that will generate `Assets/Scenes/GodgameRegistrySubScene.unity` with PureDOTS runtime config, spatial profile, and sample registry authoring; needs to be run in-editor to produce the actual SubScene asset.
   - Tracked `Assets/Scenes/GodgameBootstrapSubScene.unity` (copied from the template) as a ready-made registry/spatial SubScene for tests; contains `PureDotsConfigAuthoring`, `GodgameSampleRegistryAuthoring`, and spatial tagging.
   - Logistics sync now implemented (`GodgameLogisticsSyncSystem`), but no authored logistics requests exist yet; bridge/tests still need real request sources once gameplay produces them.

## Implementation Notes

- All stub systems should log warnings when called but not crash
- Use `[UpdateInGroup(typeof(SimulationSystemGroup))]` for all sync systems
- Follow PureDOTS patterns: Burst-compatible, deterministic, SoA-friendly
- Registry entries must implement `IRegistryEntry` and `IComparable<T>`
- Use `DeterministicRegistryBuilder<T>` for building registry buffers

## Dependencies

- PureDOTS registry contracts are stable (already defined)
- Godgame entity structures need to be mapped to registry entries
- Spatial grid integration requires `SpatialIndexedTag` on entities

