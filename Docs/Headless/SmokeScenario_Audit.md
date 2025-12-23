# Godgame Smoke Scenario Audit (2025-12-23)

## Existing Scenario Assets
- `godgame/Assets/Scenarios/Godgame/villager_loop_small.json` – only prefab counts for villagers, storehouse, trees (5/1/20). Used historically by headless + diagnostics messaging.
- `godgame/Assets/Scenarios/Godgame/village_loop_smoke.json` – placeholder stub (empty `entities` array) referenced by presentation bootstrap.
- `godgame/Assets/Scenarios/Godgame/scenario_01.json` – legacy content for `Scenario01` mode (not presently used for smoke parity).

## How scenarios are loaded
- `ScenarioBootstrap` Mono (in `TRI_Godgame_Smoke.unity`) sets `ScenarioOptions.ScenarioPath = "Scenarios/godgame/village_loop_smoke.json"` unless the operator picks a different JSON via UI.
- `ScenarioBootstrapEnsureOptionsSystem` mirrors that behavior for headless/bootstrap scenes: if no `ScenarioOptions` exists it creates one that points to `Scenarios/godgame/village_loop_smoke.json` and ensures `TimeState` exists.
- `GodgameScenarioLoaderSystem` watches `ScenarioOptions` + `GodgameScenarioConfigBlobReference` and:
  - Skips JSON load if `Config.Mode == Scenario01` and no scenario override is requested.
  - Otherwise resolves the path under `Assets/Scenarios/godgame/` (or `GODGAME_SCENARIO_PATH` env var) and converts JSON entity counts into `GodgameScenarioSpawnConfig`.
  - Requires a `SettlementConfig` entity (from the SubScene) to provide prefab references.

## Scene usage
- `TRI_Godgame_Smoke.unity` contains `ScenarioBootstrap`, Scenario UI, and SubScenes with registries + settlement data. It currently points at `village_loop_smoke.json` by default.
- `Assets/Scenes/HeadlessBootstrap.unity` uses the same authoring components; command-line headless runs rely on `ScenarioBootstrapEnsureOptionsSystem` for defaults.
- `Godgame_RenderKeySimDebugSystem` logs warnings when villagers/villages are missing (expects `villager_loop_small.json` content).

## Observations / gaps
- The JSON the presentation scene references (`village_loop_smoke.json`) is effectively empty, so the showcase is driven entirely by PureDOTS procedural spawners rather than authored smoke content.
- The headless diagnostics still reference `villager_loop_small.json`, leaving a mismatch between logs and the scene's default.
- There is no single canonical “smoke” JSON that captures modern systems (collective aggregates, work orders, AI loops). Everything still hinges on the old small loop config.
- SubScenes already provide `ScenarioOptions`, `TimeState`, registries, etc., so introducing a shared smoke JSON only requires updating the filenames and ensuring `ScenarioLoader` runs for both headless + presentation.

## Next steps (per plan)
1. Author a new canonical JSON (e.g., `godgame_smoke.json`) that includes settlements, villagers, aggregates, and resource nodes suited for the smoke showcase.
2. Update Scenario bootstrap defaults (Mono + ensure system) to point to the shared JSON.
3. Align diagnostics/logging (`Godgame_RenderKeySimDebugSystem`, smoke validation systems) so their expectations match the new scenario artifact.

