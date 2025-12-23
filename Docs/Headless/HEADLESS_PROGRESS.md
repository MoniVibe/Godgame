# Headless & Presentation Progress (updated 2025-12-23)

## Canonical Smoke Scenario
- **Scenario artifact**: `Assets/Scenarios/Godgame/godgame_smoke.json`
  - 2 settlements (Villager + Storehouse groupings) + resource belts for the gather/haul/build loop.
  - Shared between headless runner and `TRI_Godgame_Smoke.unity` via `ScenarioOptions`.
- **Scenario injection**:
  - `ScenarioBootstrapEnsureOptionsSystem` seeds `ScenarioOptions.ScenarioPath = Scenarios/godgame/godgame_smoke.json` for headless/Bootstrap scenes.
  - `ScenarioBootstrap` (Mono in `TRI_Godgame_Smoke`) resolves the same scenario, auto-upgrading legacy `village_loop_smoke.json` references.
- **Headless run**:
  - `HeadlessBootstrap.unity` (or ScenarioRunner CLI) loads the SubScene stack + `godgame_smoke.json`.
  - `GODGAME_SCENARIO_PATH` env var or `--scenario=` CLI can override, but the smoke workflow assumes the shared JSON.

## Smoke Scene Expectations
- Scene: `Assets/Scenes/TRI_Godgame_Smoke.unity`
  - Contains Scenario UI, diagnostics (`Godgame_RenderKeySimDebugSystem`, `Godgame_SmokeValidationSystem`), cameras, and registry SubScenes.
  - Uses the same render catalog + presentation setup as the headless world; no presentation-only spawners.
  - **Camera rig**: if the scene lacks a camera, `ScenarioBootstrap` spawns `CameraInputRig (Auto)` (WASD/QE translation, RMB orbit, scroll zoom). Designers can still drop their own rig to override it.
- Diagnostic checkpoints:
  - `Godgame_RenderKeySimDebugSystem` logs counts for villagers/villages/bands on load—warnings now cite `godgame_smoke.json`.
  - `Godgame_SmokeValidationSystem` + `GodgameSmokeTimeAndMovementProbeSystem` verify TimeState/RewindState and villager motion.

## Incremental Feature Mirroring Rules
1. **Headless first**: land new behavior in `godgame_smoke.json` (or associated PureDOTS systems) and prove it via telemetry/logs.
2. **Presentation follow-up**:
   - Expose semantic keys / diagnostics for the new entity type.
   - Extend `TRI_Godgame_Smoke` with visualization hooks only (overlays, debug UI). No extra gameplay entities.
3. **Documentation**: update this file (and archive the previous snapshot) within 3–4 days of a new smoke feature landing.

## Quick Validation Checklist
- Run headless command (see `headless_runbook.md`) → confirm logs mention `godgame_smoke.json` and villager/storehouse counts.
- Open `TRI_Godgame_Smoke.unity` → ensure Scenario Bootstrap UI reports `godgame_smoke.json`.
- Check `Console` for:
  - `[Godgame Render SIM] ... godgame_smoke.json` message.
  - Smoke validation systems reporting TimeState/RewindState healthy.
- Optional: run the smoke diagnostics PlayMode test suite (under `Assets/Tests/PlayMode/GodgameSmokeTests.cs`) to catch regressions.
