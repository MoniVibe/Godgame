# Godgame Demo Slices

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Demo slices, hotkeys, HUD layout, and acceptance criteria for Godgame demo

---

## Demo Slices

### Slice 1: Villager Loop

**Description:** Demonstrates villager autonomous behavior and resource flow.

**Flow:**
1. Villagers spawn in `Idle` state
2. Navigate to nearest resource node
3. Gather resources (depletes node)
4. Navigate to nearest storehouse
5. Deliver resources (conservation counter in HUD)

**What to Show:**
- Villagers autonomously selecting nodes
- Resource conservation (gathered == delivered)
- Job state transitions visible in HUD
- Storehouse inventory accumulation

**Acceptance:**
- Conservation counter shows `gathered == delivered`
- All villagers complete gather→deliver cycle
- No resources lost or duplicated

---

### Slice 2: Construction Ghost → Build

**Description:** Demonstrates construction system with resource payment.

**Flow:**
1. Place construction ghost (hotkey G)
2. System withdraws tickets from storehouse
3. Construction progresses as resources paid
4. On completion: emits effect + telemetry bump

**What to Show:**
- Ghost placement visual
- Resource withdrawal from storehouse
- Construction progress bar
- Completion effect and telemetry

**Acceptance:**
- Tickets correctly withdrawn from storehouse
- Completion triggers effect request
- Telemetry counter increments
- Registry sync shows construction complete

---

### Slice 3: Time Demo (Rewind)

**Description:** Demonstrates deterministic time control and rewind.

**Flow:**
1. Record 5 seconds of simulation
2. Rewind 2 seconds (hotkey R)
3. Resimulate to same totals
4. Log "deterministic OK" if byte-equal

**What to Show:**
- Time controls (pause, speed, step)
- Rewind visual feedback
- Determinism validation (same totals after rewind)
- Snapshot ring usage

**Acceptance:**
- Rewind run produces same totals as original
- Log "deterministic OK" on success
- Snapshot ring stays within memory limits

---

### Slice 4: Biome/Placeholder (Optional)

**Description:** Demonstrates environment hooks and visual swapping.

**Flow:**
1. Swap biome palette
2. Show environment effects on villagers/resources
3. Demonstrate placeholder → art swap

**What to Show:**
- Biome visual differences
- Environment modifiers (if implemented)
- Binding swap (Minimal ↔ Fancy)

**Acceptance:**
- Palette swap works without errors
- Environment hooks function correctly

---

## Hotkeys

### Time Controls

| Key | Action |
|-----|--------|
| **P** | Pause/Play toggle |
| **[** | Step back (single tick) |
| **]** | Step forward (single tick) |
| **1** | Speed ×0.5 |
| **2** | Speed ×1.0 |
| **3** | Speed ×2.0 |
| **R** | Trigger rewind sequence |

### Demo Controls

| Key | Action |
|-----|--------|
| **G** | Spawn construction ghost |
| **B** | Swap Minimal/Fancy binding |

### Hotkey Implementation

**Input Mapping:**
- Map to `TimeControlInput` singleton for time controls
- Map to `DemoBootstrap` for demo-specific actions
- Emit HUD events for feedback

---

## HUD Layout

### Left Side (Gameplay Metrics)

**Villagers:**
- Active villager count
- Idle/Working breakdown
- Job state distribution

**Jobs:**
- Active jobs count
- Job type breakdown
- Completion rate

**Storehouse:**
- Inventory totals per resource
- Capacity usage
- Overflow status

**Build Progress:**
- Active construction sites
- Progress per site
- Completion count

### Right Side (System Metrics)

**Tick:**
- Current simulation tick
- Tick rate (ticks/second)

**FPS:**
- Frame rate
- Target FPS

**Fixed Tick ms:**
- Fixed step duration
- Budget vs actual

**Snapshot Bytes:**
- Snapshot ring usage
- Memory budget

**ECB Playback ms:**
- EntityCommandBuffer playback time
- Structural change cost

---

## Acceptance Criteria

### Core Acceptance

1. **Headless Operation:**
   - Removing `PresentationBridge` → sim runs
   - Counters still tick correctly
   - No exceptions or errors

2. **Rewind Determinism:**
   - Rewind run = same totals as original
   - Log "deterministic OK" on success
   - Byte-equal validation passes

3. **Binding Swap:**
   - Swapping Minimal ↔ Fancy works live
   - No exceptions during swap
   - Metrics remain identical

### Performance Acceptance

1. **Fixed Tick Budget:**
   - `fixed_tick_ms ≤ target` (e.g., ≤16ms for 60Hz)
   - Logged in reports

2. **Snapshot Ring:**
   - Snapshot ring stays within limits
   - No memory pressure warnings

3. **Frame Rate:**
   - Maintains target FPS (30/60/120Hz)
   - Deterministic across frame rates

---

## Known-Good Scenarios

See `Scenarios_Spec.md` for scenario details:

- `villager_loop_small.json` - 10 villagers, 1 storehouse, 2 nodes
- `construction_ghost.json` - 1 ghost, cost 100
- `time_rewind_smoke.json` - Scripted input for demo

---

## Related Documentation

- `DemoBootstrap_Spec.md` - DemoBootstrap system
- `Bindings_Spec.md` - Presentation bindings
- `Instrumentation_Spec.md` - Metrics and reporting
- `Scenarios_Spec.md` - Scenario format

