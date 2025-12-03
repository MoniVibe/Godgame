# Demo Talk Track

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** 2-3 minute demo presentation script for Godgame

---

## Overview

This talk track provides a concise script for presenting the Godgame demo. It highlights key features: deterministic simulation, input→sim→present flow, rewind determinism, construction system, and visual binding swap.

---

## Talk Track Script

### Opening (15 seconds)

"Welcome to the Godgame demo. This is a Unity DOTS project built on PureDOTS, demonstrating deterministic simulation with optional presentation. Everything you see is driven by data—scenarios, not hardcoded scene logic."

**Visual:** Show demo scene, highlight scenario selection

---

### Core Loop (30 seconds)

"Here's the deterministic loop: input drives the simulation, which produces presentation. Watch as villagers autonomously gather resources and deliver to storehouses. Notice the conservation counter—everything gathered equals everything delivered. No resources are lost or duplicated."

**Visual:** Show villager loop, highlight conservation counter in HUD

**Actions:**
- Point to villager gathering
- Show storehouse delivery
- Highlight conservation counter

---

### Construction System (20 seconds)

"Construction works the same way. I'll place a ghost, and the system withdraws tickets from the storehouse. When construction completes, you see the effect and telemetry bump. All of this is deterministic—same inputs produce same results."

**Visual:** Place construction ghost, show payment, completion

**Actions:**
- Press G to spawn ghost
- Show resource withdrawal
- Show completion effect

---

### Time Control & Rewind (30 seconds)

"Now watch the time demo. I'll record 5 seconds, then rewind 2 seconds and resimulate. Notice the totals match exactly—this is byte-equal determinism. The simulation produces identical results after rewind, proving our time spine works correctly."

**Visual:** Show time controls, rewind, resimulation

**Actions:**
- Show time controls (pause, speed)
- Trigger rewind (R)
- Show determinism validation log

---

### Visual Binding Swap (20 seconds)

"Finally, watch as I swap between Minimal and Fancy bindings. The visuals change, but the metrics stay identical. This proves presentation is optional—the simulation runs the same whether you have fancy visuals or simple primitives."

**Visual:** Swap bindings (B), show visual change, highlight metrics unchanged

**Actions:**
- Press B to swap bindings
- Show visual difference
- Point to unchanged metrics

---

### Closing (15 seconds)

"That's the Godgame demo: deterministic simulation, optional presentation, and data-driven content. All systems are Burst-compiled for performance, and everything runs headless or with visuals. Thank you."

**Visual:** Show final state, highlight key systems

---

## Key Points Summary

### Must Mention

1. **Deterministic:** Same inputs → same results
2. **Data-Driven:** Scenarios, not hardcoded logic
3. **Rewind Works:** Byte-equal determinism validated
4. **Construction:** Ticket withdrawal, completion, telemetry
5. **Visual Swap:** Presentation optional, metrics identical

### Technical Highlights

- Unity DOTS + PureDOTS
- Burst-compiled systems
- Registry-based architecture
- Headless-capable

---

## Timing Breakdown

| Section | Duration | Key Actions |
|---------|----------|-------------|
| Opening | 15s | Show scene, explain architecture |
| Core Loop | 30s | Villager gathering, conservation |
| Construction | 20s | Ghost placement, completion |
| Time/Rewind | 30s | Rewind demo, determinism |
| Binding Swap | 20s | Visual swap, metrics unchanged |
| Closing | 15s | Summary, thank you |
| **Total** | **2m 10s** | |

---

## Visual Cues

### HUD Highlights

**Left Side:**
- Conservation counter (villager loop)
- Storehouse inventory (construction)
- Build progress (construction)

**Right Side:**
- Tick counter (time demo)
- Snapshot bytes (rewind demo)
- FPS (performance)

### On-Screen Actions

- Point to relevant HUD elements
- Highlight visual changes during binding swap
- Show console log for determinism validation

---

## Common Questions & Answers

**Q: How deterministic is it?**
A: Byte-equal. Same inputs produce identical results, validated by rewind tests.

**Q: Can it run headless?**
A: Yes. Removing PresentationBridge still yields a running sim with correct metrics.

**Q: What's the performance target?**
A: Fixed tick stays under 16ms for 60Hz, validated in preflight checks.

**Q: How do scenarios work?**
A: JSON files define entity counts and scripted events. No hardcoded scene logic.

---

## Related Documentation

- `Godgame_Slices.md` - Demo slices (what to show)
- `DemoBootstrap_Spec.md` - DemoBootstrap system
- `Bindings_Spec.md` - Binding swap explanation

