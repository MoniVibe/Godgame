# Godgame ↔ PureDOTS Integration TODO

Tracking the work required for Godgame gameplay to consume the shared `com.moni.puredots` package. Keep this file updated as bridges land.

## Agent lanes (single source of truth)

- **Agent A (recommended)**: Phase 2 demo closure — time/rewind determinism + mining/haul registry continuity. When starting, log the lane in `Docs/Progress.md` and update this TODO with findings/tests.
- **Agent B**: Alignment/compliance — alignment data on crew/fleet/colony, `CrewAggregationSystem`, `DoctrineAuthoring`, mutiny/desertion demo; keep `Docs/Progress.md` + this TODO in sync.
- **Agent C**: Modules/degradation — module slot/refit system, degradation/repair, crew skills into refit/repair/combat; log progress in `Docs/Progress.md` and update here.

## Next Agent Prompt

- **Priority**: close Phase 2 by validating time/rewind determinism and registry continuity for mining/haul entities (see `prompts.md` Agent 1, recommended).
- **Focus**: exercise the time spine snapshot/command log, assert resource counts/state match after rewind/resim, and ensure mining/haul registries keep continuity snapshots in sync.
- **Deliverables**: PlayMode tests under `Assets/Scripts/Godgame/Tests` proving rewind determinism and continuity, updates to `Docs/TODO/Phase2_Demo_TODO.md` + `Logs/*.xml`, and notes on any missing hooks in the time spine.
- **Alternates**: kick off Phase 3 Agent A (alignment/compliance: alignment data on crew/fleet/colony, `CrewAggregationSystem`, `DoctrineAuthoring` baker, mutiny/desertion demo) or continue Agent B (modules/refit/degradation/repair with crew skills wired into refit/combat); update this TODO with findings either way.

## Registry Alignment

- [ ] Inventory existing Godgame entities (villagers, bands, miracles, storehouses, logistics assets, spawners) and map each to the corresponding PureDOTS registry contract.
  - Villager/storehouse data now flow through `GodgameVillagerSyncSystem` and `GodgameStorehouseSyncSystem`, feeding live state into the bridge; remaining domains (bands, miracles, logistics, spawners) still pending.
- [ ] Author DOTS components/buffers for Godgame domain data that are missing but required by the registries (e.g., villager intent, miracle cooldowns) without duplicating the shared schemas.
- [ ] Flesh out `GodgameRegistryBridgeSystem` so it registers domain singletons/buffers with the shared registries and subscribes to registry events.
- [ ] Provide authoring/baker adapters that populate registry entries from Godgame prefabs/SubScenes.
  - Added standalone `VillagerAuthoring` and `StorehouseAuthoring` MonoBehaviours that bake registry components for individual scene objects; still need to wire them into production prefabs/SubScenes.

## Spatial & Continuity Services

- [x] Connect Godgame spatial grid usage to the PureDOTS spatial service (cell config, provider selection, rebuild cadence).
  - `GodgameSpatialProfile.asset` + `SpatialPartitionAuthoring` now seed the shared spatial grid at load so registry systems inherit the proper world bounds/cell sizing.
- [ ] Ensure continuity/rewind components from PureDOTS are hooked into Godgame determinism flows (time state, rewind state, continuity buffers).
- [ ] Validate Burst compilation for the bridge systems after spatial bindings are in place (fix any hybrid fallbacks).

## Telemetry & Metrics

- [x] Emit telemetry events (villager lifecycle, storehouse inventory, band morale, miracle usage) via the PureDOTS instrumentation buffers so the shared debug HUD reflects Godgame data.
  - [x] Wire metrics counters into the bridge so per-domain stats (population, resource throughput, pending miracles) flow into the neutral dashboards.
  - Godgame registry telemetry now emits miracle counts/energy/cooldown data directly from `GodgameRegistryBridgeSystem` so HUD/debug dashboards show the miracle pipeline alongside villagers/storehouses.

## Weather & Phenomena

- [x] Hook PureDOTS `ClimateState` + biome moisture grids into a presentation-ready Weather Controller (`WeatherBootstrapSystem`, `WeatherControllerSystem`, `WeatherPresentationSystem`) feeding the new `WeatherRigAuthoring` so COZY Weather, VFX Graph prefabs, and ambient loops react to DOTS state + time-of-day events.
  - Miracles now call into the shared `WeatherRequest` queue to trigger rain/storm FX payloads (`miracle.rain`, `miracle.storm`) instead of duplicating presentation logic.
- [ ] Author and assign actual COZY WeatherProfile assets, ambient loops, and special-FX prefabs per biome once those packs live under `Godgame/Assets`.

## Scenes, Prefabs & Assets

- [x] Review existing scenes/prefabs and add the necessary MonoBehaviour or baker adapters that translate Godgame authoring assets into PureDOTS-friendly data.
  - `GodgamePresentationRegistry.asset` + DOTS presentation adapter systems replace the placeholder adapters, and the `Godgame_VillagerDemo` scene now ships with the `InteractionRig`/hand authoring stack wired up for live demos.
- [x] Scene wizard (`Tools ▸ Godgame ▸ Create Demo Scene…`) now populates the demo scene with villagers, buildings, ground tiles, biome/time/weather/fauna controllers. Remember to move `GodgameDemoContent` into the `_Authoring` SubScene after the wizard runs.
- [ ] Demo settlement bootstrap spawns basic buildings/resource nodes; swap in art-ready prefabs (and update the wizard defaults) once final housing/storehouse/worship meshes and resource props are checked in so the loop no longer depends on the placeholder geometry baked into `Assets/Prefabs/Buildings`.
- [ ] (2025-11-14) Verify that all scenes, SubScenes, and prefabs still resolve their dependencies after relocating third-party packs to `Assets/ThirdParty/AssetStore`; update the wizard defaults and presentation docs with any new paths.
  - Space4x legacy prefabs/data are archived in `Archive/Space4xLegacy` so they stop polluting the active project. Only restore the pieces that are still required for Godgame Authoring flows.
- [ ] Replace legacy service locators in gameplay scripts with registry lookups via the PureDOTS APIs.
- [ ] Update any ScriptableObjects catalogues so they now reference the shared registries instead of local enums or IDs.

## Testing & Validation

- [ ] Stand up PlayMode/DOTS integration tests under `Godgame.Gameplay` that exercise registry registration, data sync, and telemetry emission.
  - `GodgameRegistryBridgeSystemTests` now drives the villager/storehouse sync systems, verifies continuity metadata (including miracle registry baseline), and asserts telemetry keys remain Burst-friendly.
  - Added `JobsiteConstructionTests` to cover jobsite ghost placement → completion while asserting `ConstructionRegistry` entries, effect requests, and telemetry counters.
- [ ] Add validation tests for common flows (villager spawning, band assignment, storehouse transactions, miracle dispatch) proving they interact with the shared registries.
- [ ] Create test utilities/mocks to simulate PureDOTS registries when running focused Godgame tests.

## Foundational Gameplay Systems

- [ ] Stand up the Divine Hand right-click pipeline per TruthSource (`Hand_StateMachine.md`, `RMBtruthsource.md`, `Slingshot_Contract.md`). Deliverables: DOTS-friendly `RightClickRouterSystem` + `DivineHandStateSystem`, handler components for pile siphon/storehouse dump/slingshot aim under `Assets/Scripts/Godgame/Interaction`, HUD events (`OnHandTypeChanged`, `OnHandAmountChanged`, `OnStateChanged`), and PlayMode tests covering priority resolution and frame-rate independence (30 vs 120 FPS).
  - [x] Implement the `PlayerInput` bridge that writes `InputState` and finish `CameraOrbitSystem` grounding.
    - `InputSnapshotBridge` + `HandCameraInputRouter` now copy the Hand/Camera action map straight into DOTS (`GodIntent`, `HandInputEdge`, etc.) and feed the PureDOTS hand systems.
  - [x] Build right-click affordance detectors (storehouse intake, pile surface, valid ground) reusable by router/hand systems.
    - Storehouses/resources/villagers now expose trigger colliders on dedicated layers so the bundled RMB handlers can resolve Dump/Siphon/GroundDrip priorities without warning spam.
  - [x] Define the village influence ring and gate pickups/throws.
    - `InfluenceSourceAuthoring` + `InfluenceSource` components carve out the owned-village radius, and `DivineHandSystem` now checks those sources before allowing grabs or throws.
  - [ ] Create `HandCarrySystem` PD follow + villager interrupt flow and associated jitter/GC tests.
  - [ ] Author slingshot impulse calculation, cooldown handling, and a throw test fixture.
  - [x] Wire HUD + cursor hint listeners for `HandStateChanged`/`HandCarryingChanged`.
    - `DivineHandEventBridge` is live in scene so HUD/debug tooling can bind without bespoke gameplay hooks.
  - [x] Add `HandTelemetrySystem` metrics for siphon/dump/throw.
- [ ] Implement aggregate resource piles and storehouse inventory loop (`Aggregate_Resources.md`, `Storehouse_API.md`). Deliverables: ECS components + baker authoring for `AggregatePile` and `GodgameStorehouse`, pooled prefab with size curve visual updates, Storehouse API surface (`Add/Remove/Space`), telemetry hooks for registry sync, and regression tests for overflow/merge/capacity scenarios.
  - [x] Runtime `AggregatePileSystem` (merge/split, pooling, hit metadata) + authoring for pile prefabs.
  - Initial ECS scaffolding is in place (`AggregatePileComponents`, `AggregatePileSystem`, config authoring + tests) covering spawn commands, merge/split, and conservation; `HandToAggregatePileSystem` converts GroundDrip commands into pile spawns and `StorehouseOverflowToPileSystem` reroutes dump overflow into piles. Next step is wiring storehouse intake/telemetry/presentation and PlayMode loops (hand→pile→storehouse).
  - [x] Storehouse intake authoring (intake collider, capacities) and DOTS totals/events system (`StorehouseIntakeAuthoring` + telemetry systems for totals/per-resource breakdown).
  - [ ] Conservation PlayMode tests (pile→hand→storehouse, spillover when full).
  - [x] Telemetry + registry sync wiring for storehouse totals (StorehouseTelemetrySystem + per-resource telemetry).
- [ ] Establish villager job/state graph aligned with `VillagerTruth.md`, `Villagers_Jobs.md`, `VillagerState.md`. Deliverables: job assignment buffers, state machine system (`Idle/Navigate/Gather/Carry/Deliver/Interrupted`), storehouse handoff via API events, interrupt/resume rules, and integration tests proving Gather→Deliver→Idle with storehouse totals reconciled.
  - [ ] Job scheduler / assignment buffer with GOAP hooks described in TruthSources.
  - [ ] State machine system with guards (`HasPath`, `HasCapacity`, `HasResource`) and events.
  - [ ] Interrupt handling tests (hand pickup, path blocked) and storehouse reconciliation.
- [ ] Bring up the rewindable time engine stack (`TimeTruth.md`, `TimeEngine_Contract.md`, `Timeline_DataModel.md`, `Input_TimeControls.md`). Deliverables: `TimeEngine` singleton with snapshot/command log, rewind GC policy, input bindings routed through Interaction, TimeHUD feedback (tick, speed, branch id), and EditMode tests for pause/rewind/step-back determinism and memory budget guards.
  - [ ] Command stream + snapshot storage implementation with GC tests.
  - [ ] Input routing for time controls, including UI priority overrides.
  - [ ] TimeHUD binding (tick, speed, branch).
  - [ ] Determinism tests for pause, rewind hold, step back, speed multipliers.

## Documentation & Follow-Up

- [ ] Document adapter surfaces and required authoring assets in `Docs/Guides/Godgame` (create folder as needed) and cross-link to PureDOTS truth sources.
- [ ] Update `PureDOTS_TODO.md` and relevant TruthSources when Godgame-specific needs reveal engine-level gaps.
- [ ] Capture open questions or blockers in this file to steer future agent prompts.

### Session Notes – current agent sweep

- Modules/degradation: added maintainer aggregation (host refs + crew skills), module host/definition authoring bakers, explicit damage handling, offline/repair telemetry, and expanded `ModuleMaintenanceTests`. Next: wire authoring into scenes/prefabs and surface module metrics in HUD/registries.
- Module costs: refit/repair work now supports optional resource wallets and cost-per-work gating; tests cover resource insufficiency and subsequent progress once funded.
- Latest guidance: crew growth systems, mining w/ telemetry, and fleet intercept are done; time/rewind validation for Phase 2 remains open. Recommended next step is Agent 1 (Phase 2 demo closure: rewind determinism + mining/haul registry continuity); alternates are Agent A (alignment/compliance with `CrewAggregationSystem` + `DoctrineAuthoring`) and Agent B (modules/refit/degradation wired to crew skills).
- Jobsite ghost loop now runs headless: `JobsitePlacementHotkey/Placement/Build/Completion` systems spawn ECS construction sites, route `PlayEffectRequest` entries through the shared `PresentationCommandQueue`, and bump `TelemetryStream` metrics keyed off the Construction registry metadata; placement config/state now live on the `ConstructionRegistry` entity for continuity-aware ids/positions.
- Registry bridge systems now live under `Assets/Scripts/Godgame/Registry/Registry/` (villager/storehouse/band/miracle/spawner/logistics) and feed the shared registries; the edit-mode `GodgameRegistryBridgeTests` fixture verifies metadata seeding via the PureDOTS directory. Scene/SubScene authoring still needs to provide real entities so these paths exercise authored data instead of mirrors.
- Miracle sync now sanitizes miracle definitions/runtime state via `GodgameMiracleSyncSystem` before `MiracleRegistrySystem` builds entries (clamps costs, radii, charge percent, and target positions).
- Time demo bootstrap/motion/history systems now live under `Godgame.Time`: a rewindable demo actor (with placeholder visual) records snapshots via `TimeStreamHistory`, consumes `TimeControlCommand` log output from the shared spine, and a determinism test drives the rewind → catch-up → resume path to assert the state matches byte-for-byte after a ~3s rewind. HUD bindings and pause/speed-step coverage are still outstanding.
- Registry SubScene wizard added (`Tools ▸ Godgame ▸ Create Registry SubScene…`, see `Assets/Scripts/Godgame/Editor/GodgameRegistrySubSceneWizard.cs`) to generate `Assets/Scenes/GodgameRegistrySubScene.unity` with PureDOTS runtime config, spatial profile, and `GodgameSampleRegistryAuthoring`; run in-editor to create the actual SubScene asset since no scenes are currently tracked in the repo.
- PureDOTS gameplay code lives under `Packages/com.moni.puredots/Runtime/` with domain folders (`Runtime`, `Systems`, `Authoring`). Registries, telemetry, and time/rewind singletons are all defined there; consumer projects should reference those assemblies rather than duplicating structs.
- Core bootstrap (`PureDOTS.Systems.CoreSingletonBootstrapSystem`) seeds `TimeState`, `RewindState`, registry entities, telemetry, and registry health instrumentation. Godgame bridge systems must assume these singletons already exist (or call `EnsureSingletons` in tests) before attempting registry writes.
- Module maintenance scaffold added under `Assets/Scripts/Godgame/Modules/`: components for module slots/state/refit requests + maintainer assignments/links, host/definition bakers, damage handler, `ModuleDegradationSystem` to apply wear/queue repairs (tracks damaged/offline counts), and `ModuleRefitSystem` to consume work with worker or host-aggregated skills and emit telemetry (`modules.total/damaged/offline/refitsQueued/refitCompleted/repairCompleted`). `ModuleMaintenanceTests` cover damage→refit queueing, auto-queue thresholds, host-skill speedups, and telemetry output.
- Optional resource cost gating added to `ModuleRefitSystem` via `ModuleResourceWallet` and `ResourceCostPerWork`; tests verify work pauses without resources and resumes when funded.
- Registry contracts of interest:
  - `VillagerRegistryEntry` expects availability flags, discipline, AI state, health/morale/energy bytes, and spatial continuity data (`CellId`, `SpatialVersion`).
  - `StorehouseRegistryEntry` supports per-resource capacity summaries via `FixedList32Bytes<StorehouseRegistryCapacitySummary>` and requires continuity metadata when spatial queries are enabled.
  - `RegistryMetadata.MarkUpdated` records entry counts, tick, and optional `RegistryContinuitySnapshot`; pass `RegistryContinuitySnapshot.WithoutSpatialData()` if Godgame lacks a spatial grid in early slices.
- Reuse `DeterministicRegistryBuilder<T>` when mirroring Godgame entities into shared buffers so ordering, metadata updates, and accumulator hooks stay deterministic with PureDOTS expectations.
- Telemetry is published via the `TelemetryStream` singleton buffer (`TelemetryMetric` elements). Godgame telemetry systems should batch metrics per frame and rely on PureDOTS HUD to visualise them.
- Open questions: when Godgame introduces spatial cells, align bridge with `RegistrySpatialSyncState` so registries can advertise real `CellId`/version data; confirm whether Godgame needs additional registry kinds (miracle/band/logistics) surfaced through PureDOTS packages.
- Bridge now watches `SpatialGridResidency`/`RegistrySpatialSyncState` and will mark continuity with real cell ids once Godgame entities receive spatial data; until then it falls back to non-spatial snapshots.
- Sample baker now applies `SpatialIndexedTag` to baked villagers/storehouses; new `GodgameSpatialIndexingSystem` covers runtime-spawned entities so they participate in PureDOTS spatial rebuilds without manual tagging.
- `GodgameRegistryBridgeSystemTests` seeds PureDOTS bootstrap singletons via `CoreSingletonBootstrapSystem.EnsureSingletons`, drives the spatial dirty/build systems, and asserts registry entries expose non-negative `CellId`/`SpatialVersion`.
- Follow-up: `SpatialGridResidency.Version` still lags the grid `SpatialGridState.Version` after initial rebuild, so bridge entries currently report via the fallback path. Investigate whether PureDOTS spatial systems should bump residency versions post-build or if Godgame needs a lightweight sync system.
- New runtime sync systems (`GodgameVillagerSyncSystem`, `GodgameStorehouseSyncSystem`) keep mirror components Burst-safe and free of allocations; they sanitize resource summaries and leverage the persistent resource catalog blob for index lookups.
- Registry bridge telemetry now caches metric keys and runs entirely via `TelemetryMetric` value writes, preserving Miracle registry consumers from managed allocations; tests lock this in by checking the metadata continuity snapshot.
- Godgame hand components now live directly under the `Godgame.Interaction` namespace (rather than `Godgame.Interaction.Hand`) to match Entities codegen expectations, and the gameplay asmdef references `Unity.Burst` so Burst attributes resolve during compile.
- Move & Act bootstrap now seeds headless camera/hand input, pointer-world fallback, and registry-backed band spawns; Q triggers the Miracle Ping effect id via a placeholder presentation binding. A PlayMode test drives input → band registry entry → effect request to lock the path.
- Stubbed registry bridge systems (villager/storehouse/band/miracle/spawner/logistics + orchestrator) landed under `Assets/Scripts/Godgame/Registry/` with an EditMode test ensuring they instantiate and registry metadata is seeded; functional sync/mirroring still TODO.
- Registry bridge test also asserts RegistryDirectory handles exist for core registries (villager/storehouse/band/miracle/spawner/logistics/construction); still need scenes/SubScenes with authored registry data and real sync implementations.
- Logistics sync clamps/timestamps requests and registry bridge metadata now covers logistics; new `GodgameLogisticsRegistryTests` proves requests flow through `LogisticsRequestRegistrySystem`. Still need authored logistics data and spatial continuity validation.
- Phase2 demo remains largely open: only the `JobsitePlacement/Build/Completion` loop (triggered by `KeyCode.J` and using local defaults) plus an EditMode test exist; there is no band spawn/input bridge or time/rewind snapshot stack yet, so the acceptance checkboxes in `Docs/TODO/Phase2_Demo_TODO.md` are still unchecked.
- Modules/degradation: defined `ModuleSlotIds` for villagers (helm/neck/torso/arms/hands/legs/feet/rings/trinkets/usables/backpack/cloak/main/off/sidearm/mount), wagons (mounts, cargo/personnel compartments, material, decor), and buildings (material, decor). Villager baker now seeds these slots on every villager entity; wagon/building authoring components seed their slot sets for pure-data runs so future equipment/repair work aligns with Godgame vocab.
- Modules/degradation: placeholder prefabs can now be generated via `Tools ▸ Godgame ▸ Create Placeholder Prefabs` (villager=cylinder w/ slots + maintainer assignment, building/wagon=cube with slot buffers, resource node=sphere). Uses only primitives + authoring so no SubScenes/assets required.
- Modules/degradation: prefab generator also emits placeholder gear modules (mainhand/offhand/armor/underlayer/amulet/ring/trinket/backpack/cloak) with `ModuleDefinitionAuthoring` so DOTS conversion has concrete module data without real art.
- Modules/degradation: `ModuleMaintainerAggregationSystem` stamps host references on slotted modules and aggregates maintainer skills into host `SkillSet` data; `ModuleRefitSystem` falls back to host skills when no direct maintainer assignment is present. `ModuleMaintenanceTests` cover host-skill coverage. Still need authoring for module slot buffers and module host refs, plus richer telemetry for refit/repair completions.
- Alignment/compliance: `DoctrineAuthoring` now sanitizes ids and warns on duplicates, `CrewAggregationSystem` seeds compliance/samples/alerts via cached queries and treats empty buffers as missing data, and new `CrewComplianceTests` cover warning/alert/missing-data flows; still need Godgame-side alignment data on crew/fleet/colony and a mutiny/desertion demo scene.
- Concept note (World Phasing): Add an island/world “compression” mode where whole islands can be collapsed into background simulation while observing others. Rendering pauses but registries/compliance continue in a lightweight tick, enabling “virtually” billions of entities. This should surface as an `IslandAggregateRegistry`/telemetry view without replacing the true per-entity aggregates. Persistent islands keep simulating crises/growth in the background; revisiting should reveal altered states with a lightweight per-island event log (pseudo history) to brief players on what happened while they were away. Portals can fuse worlds/islands, temporarily lifting compression to merge sims (with a perf hit) before settling back into phased background ticks.
- Alignment/compliance: `DoctrineAuthoring` now sanitizes ids and warns on duplicates, `CrewAggregationSystem` seeds compliance/samples/alerts queries without per-entity adds and treats empty buffers as missing data, and new `CrewComplianceTests` cover warning/alert/missing-data flows; still need Godgame-side alignment data on crew/fleet/colony and a mutiny/desertion demo scene.
