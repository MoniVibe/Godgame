# Miracle: Temporal Veil

**Status:** Concept  
**Category:** Miracle – Control  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Manipulates local time flow—either hastening allied actions or slowing enemies/environmental hazards. Also affects focus regen, mana regen, and resource processing while active. High-risk due to potential paradox backlash and mana debt spikes.

---

## Player Experience Goals

- **Emotion:** Strategic mastery—freeze threats or turbo-charge your villagers.  
- **Fantasy:** Bend time like a deity, echoing “Slow Time” from legacy specs.  
- **Memorable Moment:** Freeze an incoming meteor long enough to evacuate villagers and build defenses.

---

## Core Mechanic

### Activation
- **Sustained Cast (Stretch-and-release)**: Hold to stretch a circular outline from the hand’s origin; on release the circle snaps into a time bubble centered on that point.  
- **Throw Cast**: Charge a time sphere and throw it; impact seeds the bubble at landing point (slingshot/velocity).  
- Intensity (set later via UI slider wheel) dictates flow direction/speed. Costs scale with radius/duration regardless of method; sustained allows early release to save resources.

### Behavior
1. Spawns `TimeDistortion` entity with radius, speed multiplier, duration.  
2. Haste Bubble: Allies gain speed multiplier (1.5–2.0x), faster focus/mana regen, shorter cooldowns.  
3. Stasis Field: Enemies slowed (0.5–0.25x), projectiles crawl, fires burn slower.  
4. Excessive use risks “Temporal Lashback” status (stuns caster, increases mana debt).

### Feedback
- **Visual:** Distorted shimmer, color shifts (blue for slow, gold for haste).  
- **Audio:** Reverb/doppler effects.  
- **UI:** Timeline overlay showing altered tick rate.

### Cost
- 900 prayer + 60 mana + 30 focus for a 12 m radius, 15 s duration. Costs scale cubically with radius to prevent abuse. Faith density offers minimal discount (time magic is taxing).

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| Radius | 8–18 m | Larger bubbles require permission (epic miracle). |
| Speed Multiplier | 0.4–2.0 | Mode-specific. |
| Duration | 10–20 s | Extend by spending additional focus mid-cast. |
| Lashback Chance | 15% when mana debt >50% | Applies stun/coma risk. |

Edge cases: Time field near mana-debt coma patient may desync care timing; need guard rails.

### Intensity Mapping (UI Slider Wheel – numbers set later)

- Slider controls intensity 0–100. Defaults around 50.  
- **0**: Fast-forward (time flows faster → haste effect beyond bubble, e.g., 2x speed).  
- **50**: Baseline slow (approx 0.5x).  
- **75**: Time stop (entities freeze).  
- **75–100**: Rewind territory; at 100 the bubble rewinds 2× speed relative to outside time.  
- Fine tuning (e.g., 25 for mild haste) from same wheel later.  
- Rewind requires logging/rollback of bubble contents; to be scoped in implementation doc.
- Must integrate with the existing rewind feature. Only entities captured inside the bubble should rewind locally while the global rewind spine stays authoritative—needs dedicated tests once implemented.

---

## System Interactions

- **Focus & Mana**: Modifies regen as per multiplier; interacts with coma rules.  
- **Combat Movement**: Alters burst dash lengths, projectile travel.  
- **Miracles**: Casting other miracles inside a time bubble changes their effective cooldown/cost (need balancing).  
- **Espionage**: Slows detection timers, enabling stealth infiltration.

---

## Balance

- Long cooldown (~90s).  
- Temporal lashback ensures risk.  
- Requires positional play—bubbles centered on hand or on beacon.

---

## Technical Notes

- Uses `TimeDistortion` component; systems multiply local delta-time for affected entities, keeping global time deterministic.  
- Requires careful scheduling to maintain determinism (apply multipliers via component data, not actual Unity deltaTime changes).  
- Play nice with replay/rewind by logging activation events.

---

## Open Questions

1. Can players stack multiple bubbles (e.g., haste inside stasis)?  
2. Should time fields affect construction timers?  
3. How do we visualize lashback warnings?

---
