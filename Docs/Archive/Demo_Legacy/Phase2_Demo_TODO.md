# Phase 2 – Godgame Demo (spine-driven)

Goal: prove input → gameplay → presentation → rewind with zero asset debt using PureDOTS spines.

## Parallel agent lanes (work concurrently)

### Agent 1 – Slice 1: Move & Act
- [x] Bind WASD camera pan and click-to-spawn `Band` entities via the Registry (no GameObject refs).
- [x] Wire Q to enqueue `PlayEffectRequest("FX.Miracle.Ping")` against the selected target.
- [x] Add a PlayMode test proving input → registry spawn → effect request; keep Burst-friendly.
- [x] Validate the sim runs headless with `PresentationBridge` removed and drives placeholders when present.

### Agent 2 – Slice 2: Construction Stub
- [x] Hotkey places a pure-ECS "Jobsite" ghost (primitive mesh) and completes into a built state.
- [x] Completion triggers an effect request and bumps a HUD/telemetry metric.
- [x] Source Jobsite data from the Registry/Continuity spine only (no local schema divergence).
- [x] Add a PlayMode test covering ghost placement → completion → telemetry/asserted registry state (`Construction_GhostToBuilt_Playmode`).

### Agent 3 – Slice 3: Time Demo
- [x] Hold R to rewind ~3 seconds; replay resimulates deterministically (`TimeControlSystem` + `TimeDemoHistorySystem`).
- [x] Implement snapshots/command log using the PureDOTS time spine (command stream + snapshot storage).
- [x] PlayMode test asserts bytewise equality for a tiny world across rewind and verifies resume determinism (`Time_RewindDeterminism_Playmode`).
- [x] Confirm compatibility when `PresentationBridge` is absent (headless) and placeholder visuals when present (`Presentation_Optionality_Playmode`).

## Acceptance
- [x] No gameplay uses GameObject references; only IDs/requests (all systems use ECS components).
- [x] Removing `PresentationBridge` still yields a running sim; adding it plays placeholder visuals via bindings (`Presentation_Optionality_Playmode`).
- [x] PlayMode tests cover all three slices (input → sim → presentation/telemetry, rewind determinism).

## Next Phase Prep – Logic + Placeholder Presentation
- **Registry-backed villager loop:** promote the existing `GodgameVillager` mirrors into a real job/state graph by (1) finishing the storehouse API + aggregate pile intake tests, (2) defining the Idle→Navigate→Gather→Deliver machine, and (3) syncing job tickets through `GodgameRegistryBridgeSystem` so registries stay authoritative. Ship a PlayMode test that walks a villager through pile→storehouse and asserts conservation/telemetry.
- **Construction ghost acceptance:** extend the Jobsite systems so ghost placement consumes resource tickets from the registry and completion emits both telemetry and `PlayEffectRequest` placeholders. Add deterministic tests that cover placement→build→completion with registry continuity snapshots.
- **Time spine UX:** wire the `TimeDemo` stack into actual time controls (input map from `Docs/Concepts/UI_UX/Time_Controls_Input.md`), surface tick/speed/mode via HUD events, and record byte-for-byte rewind validation under `Assets/Scripts/Godgame/Tests`. Gate rewind commands behind `TimeControlCommand` queries so the sim remains headless-friendly.
- **Placeholder presentation plan:** expand `GodgamePresentationBindingBootstrapSystem` bindings beyond the miracle ping by defining IDs for jobsite ghosts, hand interactions, and module refit sparks, paired with primitive meshes or VFX graph stubs. Document the mapping in `Docs/ConfigureGodgamePrefabs_Summary.md` so art swaps can come later without changing logic.
- **Threading/data layout audit:** keep gameplay systems Burst-ready by preferring `SystemAPI.Query` loops or `IJobChunk` for hot paths (aggregate piles, module maintenance, villager jobs), verify heavy structs (e.g., registry entries) stay SoA-friendly, and introduce `IJobEntity`/`IJobParallelFor` wrappers where we expect >1k entities. Track any hotspots in `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`.

