# Godgame Controls Master Spec

**Status:** Draft  
**Scope:** Input routing, bindings, state machines for camera, hand/throw, time, and miracles  
**Owner:** Design Team  
**Last Updated:** 2025-10-31

---

## 1. Input Routing & Priority

Routing ensures inputs reach the correct system without conflicts.

1. **UI Layer** – When UI has focus (menus, sliders, miracle wheel), gameplay inputs are suppressed except for explicitly allowed bindings (e.g., camera zoom).  
2. **Modal Tools** – Hand modes (placement, selection, slingshot aim) and miracle targeting take precedence when active.  
3. **Time Controls** – Rewind/fast-forward commands run next; they intentionally override camera/hand unless blocked by higher layers.  
4. **Divine Hand / Throw** – Default RMB/LMB interactions, pick-up/drop, queue throws.  
5. **Camera** – WASD movement, mouse orbit, zoom.  
6. **Fallback** – Any unhandled input is ignored (no legacy IMGUI shortcuts).

Implementation detail: Input System action maps should support enabling/disabling action sets per layer. Example: when a miracle sustain is channeling, disable standard hand actions except cancel/throw release.

---

## 2. Binding Table (Default Keyboard/Mouse)

| Action | Binding | Notes |
|--------|---------|-------|
| Move Camera Forward/Back | W / S | When Y-locked, W/S stay horizontal. |
| Move Camera Left/Right | A / D | |
| Vertical Move | Z (up), X (down) | Always vertical; when Y unlocked, follows camera up vector. |
| Y-Axis Toggle | Y | Locks/unlocks camera’s Y-follow mode. |
| Mouse Look / Orbit | RMB drag | Disabled if hand slingshot or throw queue is active unless in “camera override” mode. |
| Zoom | Mouse Wheel | Clamped orbit distances. |
| Edge Pan (optional) | Screen edges + modifier | Toggle in settings. |
| Grab / Interact | LMB | Pick up villagers/resources, start miracles, etc. |
| Context / Queue | Shift + RMB hold | Queue held objects for later throw. |
| Release queued (single) | Q | Launch first queued object. |
| Release queued (all) | E | Launch every queued object. |
| Slingshot Mode Toggle | T | Toggle default throw mode (slingshot vs velocity). |
| Rewind | R (hold) | Cycles through rewind speeds (1× → 4×). |
| Time Slow/Fast | `[` / `]` | Adjust sim speed while not rewinding or to exit rewind. |
| Pause / Resume | Space | Toggles global pause; when rewinding, finalizes rewind and resumes. |
| Miracle Mode Switch | 1–6 or radial wheel | Selects current miracle (Rain, Water Burst, etc.). |
| Miracle Sustained Cast | Hold LMB / assigned key | Applies until release. |
| Miracle Throw Cast | RMB + drag / velocity flick | Uses throw pipeline. |
| Cancel Miracle / Mode | Esc or RMB (when not throwing) | Returns to default hand. |

Controller support to be defined later; stick with mouse/keyboard baseline for now.

---

## 3. State Machines

### 3.1 Divine Hand & Throw

States:
1. **Idle** – No selection; listens for LMB/RMB.  
2. **Holding** – Hand carrying entity/resource.  
3. **Slingshot Aim** – Activated via RMB hold (with cargo) when slingshot mode on. Charge builds over time (see `Slingshot_Throw.md`).  
4. **Velocity Throw Prep** – Short RMB tap + flick; calculates immediate impulse.  
5. **Queue Mode** – Shift+RMB toggles queue state; held items added to queue buffer.  
6. **Release** – Q releases one queued item; E flushes all.  
7. **Miracle Channel** – Overrides hand to maintain miracle effect; (substates for sustain vs throw).  

Transitions follow existing hand TODO doc; add new transitions for queue release and miracle throw. Each state writes to a DOTS `HandStateComponent` (even if inputs captured via Mono).

### 3.2 Time Control

States:
1. **Normal** – Simulation running at user-selected speed.  
2. **Rewinding** – Triggered by R; tracks current rewind multiplier (1× to 4×). Camera/hand remain in present (player still orbits/pans normally); only gameplay simulation and miracles rewind.  
3. **Ghost Preview** – R released; simulation paused but displays ghost projections showing final positions if rewind ended now.  
4. **Resume** – Press `[` / `]` / Space to exit rewind, resume forward progression at chosen speed.

Actions:
- Each press of R while held increases multiplier up to 4×.  
- Releasing R locks the multiplier and enters Ghost Preview.  
- Press Space to resume at paused speed; `[`/`]` adjust speed and exit rewind simultaneously.
- **Scope:** Current rewind implementation only affects miracle entities (they are “unmade”). Camera and divine hand stay in present-time, so player keeps planning during the rewind visualization.

### 3.3 Camera

States:
1. **Orbit Mode** – Default; WASD, Z/X, RMB orbit active.  
2. **Pan/Drag** – When middle mouse or specific key engaged.  
3. **Cinematic/Tactical** (optional) – Future state toggles lens/height.

State machine tracks whether Y-axis lock is active (affects translation vectors). When hand/miracle states take over, camera stops consuming inputs unless override toggles are pressed.

---

## 4. DOTS vs MonoBehaviour Strategy

While DOTS is the long-term goal, initial implementation can use MonoBehaviours for input capture and camera control:

- **Input Capture**: Unity Input System action maps handled by a MonoBehaviour (`GodgameInputBridge`). It reads action events, writes the results into DOTS components (`HandInput`, `TimeControlInput`, `CameraInput`). This reduces friction for prototyping.  
- **Camera Controller**: MonoBehaviour camera rig can remain for now, provided it consumes the same input data and respects the occlusion/terrain rules. When ready, a DOTS camera system can drive Cinemachine or Entities Graphics camera.  
- **Hand/Throw & Time Systems**: Prefer DOTS for actual gameplay state (hand state, queue buffers, rewind). Even if input capture is Mono, these core mechanics should be deterministic systems.  
- Document how to migrate to full DOTS later (e.g., capture input to dynamic buffers each frame, have systems read them).

Special considerations:
- Ensure Mono wrappers run before DOTS systems reading inputs (`UpdateBefore`).  
- Avoid storing state exclusively in Mono—always mirror critical values into components to stay replay-friendly.

---

## 5. Outstanding Questions

1. ✅ Y-axis toggle = **Y**.  
2. ✅ No controller support for MVP.  
3. Velocity throw sensitivity stays as designed (no extra setting now).  
4. Ghost projection: show actual entity ghost plus line pointing to original parent, retracting as rewind progresses. Implement as DOTS ghost entities or lightweight visuals.  
5. Separate action maps not needed for now; single map suffices.

---
