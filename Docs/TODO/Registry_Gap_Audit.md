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

**Missing:** No registry bridge systems exist in Godgame yet. The TODO mentions `GodgameVillagerSyncSystem` and `GodgameStorehouseSyncSystem` but they don't exist.

**Required Bridge Systems:**
1. `GodgameVillagerSyncSystem` - Sync Godgame villager entities → `VillagerRegistryEntry`
2. `GodgameStorehouseSyncSystem` - Sync Godgame storehouse entities → `StorehouseRegistryEntry`
3. `GodgameBandSyncSystem` - Sync Godgame band entities → `BandRegistryEntry` (STUB NEEDED)
4. `GodgameMiracleSyncSystem` - Sync Godgame miracle entities → `MiracleRegistryEntry` (STUB NEEDED)
5. `GodgameSpawnerSyncSystem` - Sync Godgame spawner entities → `SpawnerRegistryEntry` (STUB NEEDED)
6. `GodgameLogisticsSyncSystem` - Sync Godgame logistics → `LogisticsRequestRegistryEntry` (STUB NEEDED)
7. `GodgameRegistryBridgeSystem` - Orchestrates all sync systems and registers with PureDOTS directory

## Concrete Deliverables

### Phase 1: Core Bridge Infrastructure (Immediate)

1. **Create `Godgame/Assets/Scripts/Godgame/Registry/` directory structure**
   - `GodgameRegistryBridgeSystem.cs` - Main orchestrator
   - `GodgameVillagerSyncSystem.cs` - Villager sync (STUB)
   - `GodgameStorehouseSyncSystem.cs` - Storehouse sync (STUB)
   - `GodgameBandSyncSystem.cs` - Band sync (STUB)
   - `GodgameMiracleSyncSystem.cs` - Miracle sync (STUB)
   - `GodgameSpawnerSyncSystem.cs` - Spawner sync (STUB)
   - `GodgameLogisticsSyncSystem.cs` - Logistics sync (STUB)

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
   - `Godgame/Assets/Scripts/Godgame/Tests/GodgameRegistryBridgeTests.cs` - Verify bridge systems exist
   - `PureDOTS/Assets/Tests/VillagerAIScaffoldTests.cs` - Verify AI scaffolding compiles

2. **Scene Updates**
   - Update sample SubScenes to include registry authoring components
   - Ensure entities are tagged for spatial grid participation

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

