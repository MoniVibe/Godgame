# Phase 3 – Test & Scene Integration

Use these prompts to drive the Phase 3 items from `Docs/TODO/Registry_Gap_Audit.md` (Test & Scene Integration). PureDOTS is the shared DOTS template (see `/mnt/c/Users/shonh/OneDrive/Documents/claudeprojects/Unity/TRI_PROJECT_BRIEFING.md`); keep engine-level scaffolding/tests under `PureDOTS/` and Godgame gameplay under `Assets/Scripts/Godgame`. Update `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` and the audit doc with discoveries.

**Shared status/gaps**
- Registry bridge + sync systems now live under `Assets/Scripts/Godgame/Registry/Registry` (villager, storehouse, resource node, spawner, band, miracle, bridge snapshot/metadata). Logistics sync/registry wiring is still missing.
- `GodgameRegistryBridgeTests.cs` exists but only smoke-tests unmanaged types/metadata; it does not assert authored data, spatial continuity, or miracle/logistics entries.
- `Assets/Scenes/GodgameBootstrapSubScene.unity` is tracked; the wizard `Tools ▸ Godgame ▸ Create Registry SubScene…` will generate `Assets/Scenes/GodgameRegistrySubScene.unity` with sample authoring + spatial profile, but that SubScene is not checked in/validated yet.
- PureDOTS AI scaffolding (archetype catalog + scheduler) still lacks a smoke test, and any gaps should be reflected back into `PureDOTS_TODO.md`.

## Agent 1 – Bridge Data + Logistics Coverage
- Goal: prove the bridge exports real entries (villager/storehouse/resource/spawner/band/miracle) and add the missing logistics sync path.
- Tasks: implement `GodgameLogisticsSyncSystem` and extend `GodgameRegistryBridgeSystem` to publish `LogisticsRequestRegistry` entries with deterministic metadata; clamp resource indices and support spatial continuity. Expand bridge tests to load/sample authored data (from the bootstrap/registry SubScene) and assert entry counts/fields, continuity snapshots, and Burst-safety.
- Tests: extend `GodgameRegistryBridgeTests.cs` (or add a new fixture) to run a world with the sample authoring, verifying registry buffers are populated and metadata reflects mutations (miracle cooldowns, resource nodes, spawners, logistics requests).

## Agent 2 – PureDOTS AI Scaffold Smoke
- Goal: create `PureDOTS/Assets/Tests/VillagerAIScaffoldTests.cs` to prove the AI scaffold types compile/boot.
- Tasks: build a tiny archetype catalog blob, instantiate the scheduler/job interface types, and run a no-op update; record any missing blobs/APIs in `PureDOTS_TODO.md`.
- Tests: EditMode test inside PureDOTS assemblies; ensure `OnUpdate` stays Burst-compatible (no managed allocs) and leaves registries untouched.

## Agent 3 – Registry SubScene/Wizard Integration
- Goal: generate and wire `Assets/Scenes/GodgameRegistrySubScene.unity` via the wizard so bridge/tests can consume authored data with spatial tags.
- Tasks: run/verify the wizard output, ensure `GodgameSampleRegistryAuthoring` + `SpatialPartitionAuthoring` + runtime config assets are present, and hook the SubScene into the demo/Sample scene as needed. Refresh README/TODO with any missing assets or wizard friction.
- Validation: open the SubScene to confirm registry buffers populate (villager/storehouse/resource/spawner/miracle), spatial grid participation is enabled, and no missing references remain; capture gaps in `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`.
