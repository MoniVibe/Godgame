# Godgame ↔ PureDOTS Integration TODO

Tracking the work required for Godgame gameplay to consume the shared `com.moni.puredots` package. Keep this file updated as bridges land.

## Next Agent Prompt

- Focus: Deliver the initial Godgame bridge into the shared registries.
- Starting point: expand `GodgameRegistryBridgeSystem` to register villager/storehouse buffers with the neutral registries and emit at least one telemetry sample to the PureDOTS HUD.
- Deliverables: minimal DOTS components representing villagers or storehouses, baker/authoring glue for a sample prefab/SubScene, and a PlayMode/DOTS test verifying the registry tells.
- Constraints: keep gameplay-specific code under `Godgame/Assets/Scripts/Godgame`; escalate only genuine engine-level gaps to PureDOTS and document them here.

## Registry Alignment

- [ ] Inventory existing Godgame entities (villagers, bands, miracles, storehouses, logistics assets, spawners) and map each to the corresponding PureDOTS registry contract.
- [ ] Author DOTS components/buffers for Godgame domain data that are missing but required by the registries (e.g., villager intent, miracle cooldowns) without duplicating the shared schemas.
- [ ] Flesh out `GodgameRegistryBridgeSystem` so it registers domain singletons/buffers with the shared registries and subscribes to registry events.
- [ ] Provide authoring/baker adapters that populate registry entries from Godgame prefabs/SubScenes.

## Spatial & Continuity Services

- [ ] Connect Godgame spatial grid usage to the PureDOTS spatial service (cell config, provider selection, rebuild cadence).
- [ ] Ensure continuity/rewind components from PureDOTS are hooked into Godgame determinism flows (time state, rewind state, continuity buffers).
- [ ] Validate Burst compilation for the bridge systems after spatial bindings are in place (fix any hybrid fallbacks).

## Telemetry & Metrics

- [ ] Emit telemetry events (villager lifecycle, storehouse inventory, band morale, miracle usage) via the PureDOTS instrumentation buffers so the shared debug HUD reflects Godgame data.
- [ ] Wire metrics counters into the bridge so per-domain stats (population, resource throughput, pending miracles) flow into the neutral dashboards.

## Scenes, Prefabs & Assets

- [ ] Review existing scenes/prefabs and add the necessary MonoBehaviour or baker adapters that translate Godgame authoring assets into PureDOTS-friendly data.
- [ ] Replace legacy service locators in gameplay scripts with registry lookups via the PureDOTS APIs.
- [ ] Update any ScriptableObjects catalogues so they now reference the shared registries instead of local enums or IDs.

## Testing & Validation

- [ ] Stand up PlayMode/DOTS integration tests under `Godgame.Gameplay` that exercise registry registration, data sync, and telemetry emission.
- [ ] Add validation tests for common flows (villager spawning, band assignment, storehouse transactions, miracle dispatch) proving they interact with the shared registries.
- [ ] Create test utilities/mocks to simulate PureDOTS registries when running focused Godgame tests.

## Documentation & Follow-Up

- [ ] Document adapter surfaces and required authoring assets in `Docs/Guides/Godgame` (create folder as needed) and cross-link to PureDOTS truth sources.
- [ ] Update `PureDOTS_TODO.md` and relevant TruthSources when Godgame-specific needs reveal engine-level gaps.
- [ ] Capture open questions or blockers in this file to steer future agent prompts.

