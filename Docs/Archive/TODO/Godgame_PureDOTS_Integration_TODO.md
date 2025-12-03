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
  - Demo hotkeys/HUD now ride the Input System and fall back to `Time.timeScale` for pause/step/speed; wire these into `TimeControlInput`/command log once the PureDOTS time spine lands so rewind determinism covers the demo UI again.
- [ ] Validate Burst compilation for the bridge systems after spatial bindings are in place (fix any hybrid fallbacks).

## Telemetry & Metrics

- [x] Emit telemetry events (villager lifecycle, storehouse inventory, band morale, miracle usage) via the PureDOTS instrumentation buffers so the shared debug HUD reflects Godgame data.
  - [x] Wire metrics counters into the bridge so per-domain stats (population, resource throughput, pending miracles) flow into the neutral dashboards.
  - Godgame registry telemetry now emits miracle counts/energy/cooldown data directly from `GodgameRegistryBridgeSystem` so HUD/debug dashboards show the miracle pipeline alongside villagers/storehouses.

## Weather & Phenomena

- [x] Hook PureDOTS `ClimateState` + biome moisture grids into a presentation-ready Weather Controller (`WeatherBootstrapSystem`, `WeatherControllerSystem`, `WeatherPresentationSystem`) feeding the new `WeatherRigAuthoring` so COZY Weather, VFX Graph prefabs, and ambient loops react to DOTS state + time-of-day events.
  - Miracles now call into the shared `WeatherRequest` queue to trigger rain/storm FX payloads (`miracle.rain`, `miracle.storm`) instead of duplicating presentation logic.
- [x] Tier-1 Biome System: Implemented biome definition blobs, climate/moisture systems, biome resolution, and placeholder presentation tokens. Three biomes (Temperate, Grasslands, Mountains) with resource seeding bias and villager modifiers.
- [x] Vegetation System: Implemented plant/stand specifications, growth/spawn/harvest systems, presentation bindings, and Prefab Maker Vegetation tab. Plants are data-driven specs with growth stages, yields, hazards, and biome preferences. Stands spawn deterministically with clustering. All visuals are token-based and swappable.
- [x] Rebuilt miracle casting data (`MiracleCasterState`, `MiracleSlotDefinition`, `RainMiracleCommand`, `MiracleReleaseEvent`) and a bootstrap that seeds release/rain command buffers so `GodgameMiracleInputSystem`/`GodgameMiracleReleaseSystem` can run again; added `MiracleSystemsTests` to cover system creation. Follow-up: extend `DivineHandInput` with miracle-specific fields (slot selection, edge triggers) and rerun EditMode tests once Unity CLI is available (currently `Unity` binary missing on host).
- [ ] Domain reload pending (after Agent C namespace restore) to confirm stubbed `PureDOTS.Environment` types (`BiomeType`, climate grids, `EnvironmentGridConfig`) compile cleanly and that `BiomeTerrainProfile`/`GodgameEnvironmentGridConfig` assets load without missing scripts.
- [x] Resolved biome/ground namespace ambiguity: biome terrain/ground tile/weather/fauna/miracle presentation systems now target `PureDOTS.Environment` types, and `GroundBiome` stores a byte for blittable/Burst-safe sampling. Follow-up: rerun domain reload to confirm no remaining duplicate type errors.
- [ ] Author and assign actual COZY WeatherProfile assets, ambient loops, and special-FX prefabs per biome once those packs live under `Godgame/Assets`.

## Scenes, Prefabs & Assets

- [x] Review existing scenes/prefabs and add the necessary MonoBehaviour or baker adapters that translate Godgame authoring assets into PureDOTS-friendly data.
  - `GodgamePresentationRegistry.asset` + DOTS presentation adapter systems replace the placeholder adapters, and the `Godgame_VillagerDemo` scene now ships with the `InteractionRig`/hand authoring stack wired up for live demos.
- [x] Scene wizard (`Tools ▸ Godgame ▸ Create Demo Scene…`) now populates the demo scene with villagers, buildings, ground tiles, biome/time/weather/fauna controllers. Remember to move `GodgameDemoContent` into the `_Authoring` SubScene after the wizard runs.
- [ ] Demo settlement bootstrap spawns basic buildings/resource nodes; swap in art-ready prefabs (and update the wizard defaults) once final housing/storehouse/worship meshes and resource props are checked in so the loop no longer depends on the placeholder geometry baked into `Assets/Prefabs/Buildings`.
- [ ] (2025-11-14) Verify that all scenes, SubScenes, and prefabs still resolve their dependencies after relocating third-party packs to `Assets/ThirdParty/AssetStore`; update the wizard defaults and presentation docs with any new paths.
  - Space4x legacy prefabs/data are archived in `Archive/Space4xLegacy` so they stop polluting the active project. Only restore the pieces that are still required for Godgame Authoring flows.
- [ ] Presentation binding assets now live under `Assets/Resources/Bindings`; `PresentationBindingBootstrapSystem` seeds a Minimal binding reference and logs if the asset is missing. Wire the binding reference into the presentation bridge once the registry/FX stack is online.
- [ ] Replace legacy service locators in gameplay scripts with registry lookups via the PureDOTS APIs.
- [ ] Update any ScriptableObjects catalogues so they now reference the shared registries instead of local enums or IDs.

## Testing & Validation

- [ ] Stand up PlayMode/DOTS integration tests under `Godgame.Gameplay` that exercise registry registration, data sync, and telemetry emission.
  - `GodgameRegistryBridgeSystemTests` now drives the villager/storehouse sync systems, verifies continuity metadata (including miracle registry baseline), and asserts telemetry keys remain Burst-friendly.
  - Added `JobsiteConstructionTests` to cover jobsite ghost placement → completion while asserting `ConstructionRegistry` entries, effect requests, and telemetry counters.
  - Added `TimeRewindDeterminismPlayModeTests` to cover time spine rewind gating and logistics registry continuity after rewind/catch-up.
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
- [x] Establish villager job/state graph aligned with `VillagerTruth.md`, `Villagers_Jobs.md`, `VillagerState.md`. Deliverables: job assignment buffers, state machine system (`Idle/Navigate/Gather/Carry/Deliver/Interrupted`), storehouse handoff via API events, interrupt/resume rules, and integration tests proving Gather→Deliver→Idle with storehouse totals reconciled.
  - [x] State machine system (`VillagerJobSystem`) with `Idle→NavigateToNode→Gather→NavigateToStorehouse→Deliver` phases.
  - [x] Storehouse API integration (`StorehouseApi.TryDeposit/Withdraw`) for resource handoff.
  - [x] Integration tests (`Conservation_VillagerGatherDeliver_Playmode`, `Jobs_GatherDeliver_StateGraph_Playmode`).
  - [ ] Job scheduler / assignment buffer with GOAP hooks described in TruthSources (future enhancement).
  - [ ] Interrupt handling tests (hand pickup, path blocked) and storehouse reconciliation (future enhancement).
- [x] Bring up the rewindable time engine stack (`TimeTruth.md`, `TimeEngine_Contract.md`, `Timeline_DataModel.md`, `Input_TimeControls.md`). Deliverables: `TimeEngine` singleton with snapshot/command log, rewind GC policy, input bindings routed through Interaction, TimeHUD feedback (tick, speed, branch id), and EditMode tests for pause/rewind/step-back determinism and memory budget guards.
  - [x] Time control system (`TimeControlSystem`) with pause/speed/step/rewind support.
  - [x] HUD event buffer (`HudEvent`) for time state feedback.
  - [x] Determinism test (`Time_RewindDeterminism_Playmode`) for rewind/resim validation.
  - [x] `TimeDemoHistorySystem` for snapshot/command log (existing).
  - [ ] Input routing for time controls via Interaction system (future enhancement - currently uses `TimeControlInput` singleton).
  - [ ] Rewind GC policy and memory budget guards (future enhancement).

## PureDOTS Feature Requests (Game-Agnostic Systems)

The following systems were prototyped in Godgame but are **game-agnostic** and should be moved to `com.moni.puredots` for consumption by both Space4X and Godgame.

### Focus System (Combat Resource) — REQUEST PENDING

**Reference Implementation:** `Assets/Scripts/Godgame/Combat/`

**Core Components to move to PureDOTS:**
- `EntityFocus` — Focus pool with capacity, regen rate, exhaustion tracking, coma state
- `CombatStats` — Physique/Finesse/Intelligence/Will/Wisdom (ability unlock gating)
- `FocusAbilityType` — Enum for Finesse/Physique/Arcane abilities (24+ types)
- `ActiveFocusAbility` — Buffer for tracking active abilities with drain/duration
- `FocusConfig` — Singleton for global exhaustion/regen/coma parameters
- `FocusComaTag`, `FocusBreakdownRiskTag` — State tags

**Systems to move:**
- `FocusRegenSystem` — Regenerates focus over time (faster when resting)
- `FocusAbilitySystem` — Activates abilities, drains focus, manages durations
- `FocusExhaustionSystem` — Tracks exhaustion, triggers breakdown/coma
- `FocusAbilityDefinitions` — Static ability cost/effect/unlock data

**Helper utilities to move:**
- `FocusEffectHelpers` — Query active abilities for combat modifiers (attack speed, dodge, crit, spell power)
- `FocusExhaustionHelpers` — Query exhaustion state for effectiveness/cost multipliers
- `ProfessionFocusHelpers` — Tradeoff calculations, quality/speed/waste formulas
- `ProfessionFocusIntegration` — Static helpers for job systems (crafting, gathering, healing, teaching, refining)

**Profession Focus Components to move:**
- `ProfessionType` enum — 35+ profession types (Blacksmith, Miner, Healer, Scholar, Smelter, etc.)
- `ProfessionSkills` — Skill levels per archetype (Crafting/Gathering/Healing/Teaching/Refining)
- `ProfessionFocusModifiers` — Quality/Speed/Waste/TargetCount/BonusChance multipliers
- `ProfessionFocusConfig` — Global profession settings

**40+ Profession Abilities (all game-agnostic):**
- **Crafting (70-89)**: MassProduction, MasterworkFocus, BatchCrafting, PrecisionWork, Reinforce, EfficientCrafting, Inspiration, StudiedCrafting
- **Gathering (90-109)**: SpeedGather, EfficientGather, GatherOverdrive, ResourceSense, DeepExtraction, LuckyFind, SustainableHarvest, Multitasking
- **Healing (110-129)**: MassHeal, LifeClutch, PurifyingFocus, RegenerationAura, IntensiveCare, MiracleHealing, LifeTransfer, Triage
- **Teaching (130-149)**: IntensiveLessons, DeepTeaching, GroupInstruction, InspiringPresence, HandsOnTraining, TalentDiscovery, MindLink, Eureka
- **Refining (150-169)**: RapidRefine, PureExtraction, BatchRefine, QualityControl, Reclamation, Transmutation, Synthesis, GentleProcessing

**Usage Examples (Godgame-specific, stay local):**
- `FocusAuthoring`, `CombatArchetypeAuthoring` — Presets for Warrior/Rogue/Mage/etc.

**Related game-agnostic combat utilities needed in PureDOTS:**
- [ ] Target selection system (priority queues, threat assessment)
- [ ] Range check utilities (melee/ranged/AOE distance queries)
- [ ] Hit calculation system (accuracy, dodge, armor, damage rolls)
- [ ] Projectile/AOE resolution (burst vs sustained effects)
- [ ] Combat state machine (engaged/fleeing/stunned/recovering)

**Blocking:** None — can be used from Godgame now, move to PureDOTS when prioritized.

---

## Burst BC1016 Error Batch (2025-11-27) — PureDOTS

**Critical:** These errors block Burst compilation and cause repeated compilation failures that can freeze the Editor. All are in the **PureDOTS** package (`C:\Users\Moni\Documents\claudeprojects\unity\PureDOTS\Packages\com.moni.puredots\`).

### Root Cause
Using managed `System.String` operations inside Burst-compiled code:
- `new FixedString32Bytes(string)` / `new FixedString64Bytes(string)` — calls `CopyFromTruncated` which uses `String.get_Length`
- `FixedString*.ToString()` — managed function not supported in Burst

### Affected Files & Lines

| File | Line | Issue | Fix |
|------|------|-------|-----|
| `Runtime/Runtime/Registry/Aggregates/AggregateHelpers.cs` | 227 | `new FixedString32Bytes(string)` | Cache string as static `FixedString32Bytes` field |
| `Runtime/Systems/Combat/HazardEmitFromDamageSystem.cs` | 128 | `FixedString32Bytes.ToString()` | Remove `.ToString()` or move to non-Burst context |
| `Runtime/Systems/Ships/LifeBoatEjectorSystem.cs` | 85 | `FixedString32Bytes.ToString()` | Remove `.ToString()` or move to non-Burst context |
| `Runtime/Systems/Spells/SchoolFoundingSystem.cs` | 148 | `new FixedString64Bytes(string)` | Cache string as static `FixedString64Bytes` field |

### Fix Pattern

**Before (breaks Burst):**
```csharp
// In Burst job or [BurstCompile] method
var name = new FixedString32Bytes("some_string");
Debug.Log(fixedString.ToString()); // Also breaks
```

**After (Burst-safe):**
```csharp
// At class level, outside Burst context
private static readonly FixedString32Bytes s_SomeString = new FixedString32Bytes("some_string");

// In Burst job
var name = s_SomeString;
// Avoid ToString() in Burst - if needed for logging, do it outside job
```

### Systems Affected (from compile log)
These systems are failing to compile due to the above errors:
- `PseudoHistorySystem`, `SatisfyNeedSystem`, `RelationInteractionSystem`, `TelemetryTrendSystem`
- `CompressionSystem`, `MoraleBandSystem`, `BalanceAnalysisSystem`, `AggregateRegistrySystem`
- `HazardEmitFromDamageSystem`, `LifeBoatEjectorSystem`, `SchoolFoundingSystem`
- `SpellEffectExecutionSystem`, `HybridizationSystem`, `SpellLearningSystem`, `SpellPracticeSystem`
- ~60+ other systems in the compile graph

### Action Required
Fix the 4 files above in PureDOTS, then rebuild to clear the Burst compilation errors.

---

## Documentation & Follow-Up

- [ ] Document adapter surfaces and required authoring assets in `Docs/Guides/Godgame` (create folder as needed) and cross-link to PureDOTS truth sources.
- [ ] Update `PureDOTS_TODO.md` and relevant TruthSources when Godgame-specific needs reveal engine-level gaps.
- [ ] Verify PureDOTS CreateAssetMenu warnings are cleared after removing attributes on CultureStoryCatalogAuthoring, LessonCatalogAuthoring, SpellCatalogAuthoring, ItemPartCatalogAuthoring, and EnlightenmentProfileAuthoring (Agent 5); rerun a domain reload when the PureDOTS Editor pass resumes.
- [ ] **Fix Burst BC1016 errors** in PureDOTS (see batch above) — 4 files need string→FixedString caching.
- [ ] Capture open questions or blockers in this file to steer future agent prompts.

### Session Notes – current agent sweep
- 2025-11-27 — Captured new Burst BC1016 error batch from Unity console: 4 files in PureDOTS (`AggregateHelpers.cs:227`, `HazardEmitFromDamageSystem.cs:128`, `LifeBoatEjectorSystem.cs:85`, `SchoolFoundingSystem.cs:148`) use `new FixedString*(string)` or `.ToString()` inside Burst context; added fix table above. Also disabled orphaned `Space4X.Editor.asmdef` in Godgame that was causing assembly resolution failures, and rate-limited `EntityMeetingSystem` O(n²) algorithm to prevent play-mode freezes.
- 2025-11-27 — Agent 1 (BC1016 fixes): moved Burst-facing BandFormationSystem goal descriptions and BandFormationProcessingSystem goal/name checks to cached FixedString constants; SpellEffectExecutionSystem shield default already cached. Unity domain reload/tests not run yet; rerun PureDOTS with Burst enabled to confirm BC1016 clears.
- 2025-11-27 — Agent 6 (CreateAssetMenu warning cleanup): removed CreateAssetMenu attributes from PureDOTS authoring MonoBehaviours (BuffCatalogAuthoring, SchoolComplexityCatalogAuthoring, QualityFormulaAuthoring, SpellSignatureCatalogAuthoring, QualityCurveAuthoring) to stop ignored attribute warnings; Unity domain reload/tests not run.
- 2025-11-27 — Agent 2 (BC1016 fixes): Verified `TimelineBranchSystem.WhatIfSimulationSystem` uses a cached branch name constant and moved attribute ID comparisons in `LessonAcquisitionSystem.CheckAttributeRequirement` to static readonly FixedString fields to avoid Burst string constructors; Unity domain reload/tests not run this pass.
- 2025-11-27 — Agent 3 (stale presentation references check): re-verified `SwappablePresentationBindingEditor` resolves `SwappablePresentationBindingAuthoring`/`PresentationRegistry` via `Godgame.Presentation` + Editor asmdef reference; no additional code changes required, Unity tests not run this pass.
- 2025-11-27 — Agent 4 (scene tools prompt refresh): verified `GodgameDevSceneSetup` and `GodgameDemoSceneWizard` already use `FindObjectsByType`/`FindFirstObjectByType` APIs and target `GodgameDevToolsRig`/`GodgameDemoBootstrapAuthoring`; no additional changes needed, Unity tests not run this pass.
- 2025-11-27 — Agent 3 (presentation editor fixes): added `Godgame.Presentation` runtime asmdef and pointed `Godgame.Presentation.Editor` to it so `SwappablePresentationBindingEditor` can see `PresentationRegistry`/`SwappablePresentationBindingAuthoring`; ensure new asmdef/meta under `Assets/Scripts/Godgame/Presentation` are tracked despite Assets being gitignored.
- 2025-11-27 — Agent 6 (Phase 2 demo closure): Local `com.moni.puredots` file dependency is missing (manifest expects `../../PureDOTS/Packages/com.moni.puredots`), so shared types like `TimeState`/`RewindState` are unresolved; added an Editor checker to surface the missing path. Need the PureDOTS repo or updated package path before running determinism/registry tests.
- 2025-11-22 — Code sweep (Agent A planning): confirmed registry bridge covers villagers/storehouses/resources/spawners/bands/miracles/logistics with telemetry snapshots, module degradation/refit loops have Burst-friendly systems + NUnit coverage, aggregate pile + storehouse overflow logic is live, and the rewindable time demo records/catches up via `TimeStreamHistory`. Next focus: finish construction ghost → completion telemetry tests, bring villager job graph/storehouse API online, wire time controls & HUD to the spine, and expand placeholder presentation bindings beyond the current miracle ping effect.
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
