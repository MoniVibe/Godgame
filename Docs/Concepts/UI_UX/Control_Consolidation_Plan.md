# Control & Input Consolidation Plan

**Status:** Planning  
**Scope:** Time controls, camera controls, throw/hand interactions, miracle dispensation  
**Owner:** Design Team  
**Last Updated:** 2025-10-31

---

## Goals

1. Gather all Godgame input systems into a single, coherent concept so implementation teams know which controls exist, how they route, and how they map to DOTS systems.  
2. Ensure time controls, camera behavior, divine hand throwing, and miracle dispensation share the same input priorities and state machines.  
3. Keep the runtime DOTS-friendly while permitting MonoBehaviour wrappers where it simplifies editor/UI work.

---

## Existing References

| Area | Current Docs |
|------|--------------|
| Hand / Throwing | `Docs/TODO/Hand.md`, `Docs/Concepts/Interaction/Slingshot_Throw.md`, legacy `Slingshot_Contract`. |
| Time Controls | `Docs/Concepts/UI_UX/Time_Controls_Input.md`, `Docs/Concepts/legacy/Input_TimeControls.md`, references in `Legacy_TruthSources_Salvage.md`. |
| Camera | `Docs/Legacy_TruthSources_Salvage.md` (cameraimplement), `PureDOTS/Docs/TODO/DivineHandCamera_TODO.md`. |
| Miracles | `Docs/Concepts/Miracles/Miracle_System_Vision.md` (dispensation), new miracle specs. |

---

## Consolidation Approach

1. **Inventory & Alignment**
   - Extract control lists (bindings, gestures, routing order) from each doc.  
   - Identify overlaps/conflicts (e.g., RMB behavior between time control modal vs throw).  
   - Decide on unified priority stack: `UI → Modal Tools → Time Controls → Divine Hand (RMB/LMB) → Camera`.

2. **Author Consolidated Spec**
   - Create `Controls_Master_Spec.md` describing:  
     - Input mapping table (mouse/keyboard/controller).  
     - State machines for divine hand (hold, grab, slingshot, velocity throw).  
     - Time control commands (pause, rewind, step, speed multipliers).  
     - Camera modes (orbit, pan, zoom, tactical view).  
     - Miracle dispensation toggle (sustain vs throw).  
   - Include DOTS vs MonoBehavior notes (e.g., hand state machine in DOTS, camera/time UI may stay Mono for now).

3. **Implementation Staging**
   - **Phase 1 (Mono wrapper-friendly):**  
     - Use Unity Input System in a MonoBehaviour to capture inputs, publish to DOTS via command buffers.  
     - Focus on parity with legacy controls; minimal new features.  
   - **Phase 2 (Full DOTS):**  
     - Move critical loops (hand, time controls) into DOTS systems once stable.  
     - Document data flow (input → command component → action system).

4. **Testing & Tooling**
   - Define playmode tests for slingshot charge, velocity throw, time control overrides.  
   - Provide debug UI overlay showing current control state (modal, hand state, time speed).

5. **Documentation & Handoff**
   - Update `Miracle_System_Vision` and other dependent docs to reference the consolidated spec.  
   - Keep a change log for bindings so downstream teams (Space4X) can replicate or adapt.

---

## DOTS vs MonoBehaviour Notes

- **Preferred**: Input data ultimately stored in DOTS components (`HandInputState`, `TimeControlInput`).  
- **Acceptable**: Initial capture via MonoBehaviours for simplicity, especially for camera and UI-heavy interactions.  
- Provide clear boundaries: Mono captures → writes to DOTS components → systems act deterministically.

### Input System Requirements

- **New Unity Input System only** (no legacy `Input.GetAxis`). All bindings go through Input System action maps for easy rebinds and future controller support.  
- Centralize bindings in a single asset (e.g., `GodgameControls.inputactions`) referenced by both Mono wrappers and DOTS systems.  
- Document default bindings and how to override them per profile.  
- Ensure time controls respect UI focus (as per existing docs) using Input System’s `UIInputModule` gating.

### Camera Considerations

- Camera uses orbit/pan/zoom with configurable orbit radius/height. Need explicit notes:  
  - Clamp orbit distance range (e.g., 5–50 units) but allow designer tuning.  
  - **Object occlusion disabled**—camera should not auto-clip through buildings/villagers; only terrain can clip/offset camera.  
  - Provide optional “ghosting” toggle if we later want to fade objects, but default is no occlusion logic.  
- Terrain clipping: camera should raise or offset when terrain obstructs line of sight; no other geometry should affect camera path.  
- Document any cinematic or tactical modes if they alter orbit size.

### Specific Control Requirements (to capture in master spec)

**Camera Movement**
- **WASD**: Move camera forward/left/back/right relative to view.  
- **Axis Toggle**: Keybinding (e.g., `CapsLock` or custom toggle) locks Y movement so WASD stays horizontal. When unlocked, pressing **W** while looking up moves camera upward, allowing vertical travel.  
- **Z / X**: Explicit vertical translation (Z = up, X = down) regardless of axis toggle; when Y-axis unlocked these respect camera orientation (move along view’s up/down vector).  
- Mouse: RMB drag or edge pan depending on mode (refer to camera todo doc).  
- Mouse Wheel: Zoom (respect orbit constraints).

**Object Queue & Throw**
- **Shift + RMB Hold**: Queue currently held objects for launch rather than immediate throw.  
- **Q**: Release a single queued object (first-in).  
- **E**: Release all queued objects at once.  
- Works with both slingshot and velocity throw modes—queue stores transform + impulse data until release.

**Time Controls**
- **R**: Initiate rewind. Holding R ramps through speeds: slow rewind on first press, additional presses increase up to 4× rewind speed.  
- Releasing R pauses simulation but keeps rewind “armed”, showing **ghost projections** of entities indicating where they’ll return when rewind finalizes.  
- Rewind ends (simulation resumes forward) when player adjusts time with:  
  - **[** : Slow down sim speed (step down).  
  - **]** : Speed up sim speed (step up).  
  - **Space** : Pause/Resume normal time (exits rewind state).  
- Need UI indicator for current rewind speed and ghost state.

---

## Next Actions

1. ✅ Consolidation plan approved; `Controls_Master_Spec.md` drafted.  
2. Implement tasks outlined below:
   - **Input System Setup**: Create `GodgameControls.inputactions`, implement `GodgameInputBridge`, hook action maps to DOTS components.  
   - **Camera Controller**: Build WASD + Z/X movement, Y-axis toggle on `Y`, orbit/zoom, terrain-only clipping, occlusion rules.  
   - **Hand & Throw**: Update state machine for queueing (Shift+RMB), Q/E release, T throw-mode toggle, integrate miracle sustain/throw.  
   - **Time Controls**: Implement R-based rewind ramp, ghost projection visuals (entity ghost + parent line), `[`, `]`, Space resume logic tied into rewind spine.  
   - **Miracle Dispensation**: Wire sustained vs throw casts to new bindings.  
   - **Debug/Testing**: Add overlay showing current control states; create playmode tests for queue release, rewind flows, camera axis toggle.
3. After implementation, update dependent TODO docs (hand, camera, time) with actual status.

---
