# Phase 2 – Godgame Demo (spine-driven)

Goal: prove input → gameplay → presentation → rewind with zero asset debt using PureDOTS spines.

## Parallel agent lanes (work concurrently)

### Agent 1 – Slice 1: Move & Act
- [x] Bind WASD camera pan and click-to-spawn `Band` entities via the Registry (no GameObject refs).
- [x] Wire Q to enqueue `PlayEffectRequest("FX.Miracle.Ping")` against the selected target.
- [x] Add a PlayMode test proving input → registry spawn → effect request; keep Burst-friendly.
- [x] Validate the sim runs headless with `PresentationBridge` removed and drives placeholders when present.

### Agent 2 – Slice 2: Construction Stub
- [ ] Hotkey places a pure-ECS “Jobsite” ghost (primitive mesh) and completes into a built state.
- [ ] Completion triggers an effect request and bumps a HUD/telemetry metric.
- [ ] Source Jobsite data from the Registry/Continuity spine only (no local schema divergence).
- [ ] Add a PlayMode test covering ghost placement → completion → telemetry/asserted registry state.

### Agent 3 – Slice 3: Time Demo
- [ ] Hold R to rewind ~3 seconds; replay resimulates deterministically.
- [ ] Implement snapshots/command log using the PureDOTS time spine (command stream + snapshot storage).
- [ ] PlayMode test asserts bytewise equality for a tiny world across rewind and verifies resume determinism.
- [ ] Confirm compatibility when `PresentationBridge` is absent (headless) and placeholder visuals when present.

## Acceptance
- [ ] No gameplay uses GameObject references; only IDs/requests.
- [ ] Removing `PresentationBridge` still yields a running sim; adding it plays placeholder visuals via bindings.
- [ ] PlayMode tests cover all three slices (input → sim → presentation/telemetry, rewind determinism).
