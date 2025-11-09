# Miracle: Firestorm

**Status:** Concept  
**Category:** Miracle – Offensive  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Conjures a firestorm that scorches enemies, clears forests, or cauterizes corruption. Fire spreads along flammable materials, synergizes with air miracles (fan flames), and is countered by rain/water miracles.

---

## Player Experience Goals

- **Emotion:** Raw destructive thrill with tactical consequences.  
- **Fantasy:** Divine wrath—call down pillars of flame.  
- **Memorable Moment:** Incinerate a marauding army while carefully steering flames away from friendly fields.

---

## Core Mechanic

### Activation
- **Sustained Cast**: Hold to sweep a flame jet/curtain across terrain, drawing patterns; drains cost per second.  
- **Throw Cast**: Charge a fire seed and hurl it (slingshot/velocity). Impact spawns a fire pillar that expands according to drawn arc.  
- Prayer cost scales with area/time; throw mode front-loads resource consumption.

### Behavior
1. Spawns `FirePulse` entities that tick damage and attempt to ignite nearby tiles.  
2. Applies burn statuses to entities caught inside; structures take durability damage per tick.  
3. Fire persists until fuel consumed or extinguished by moisture miracles/rain.  
4. Air miracles can amplify spread speed; water miracles suppress instantly.

### Feedback
- **Visual:** Concentric flame walls, sparking embers, smoke plumes visible from afar.  
- **Audio:** Roaring flames, crackling timber.  
- **UI:** Heatmap overlay showing predicted spread.

### Cost
- Base 600 prayer + 30 mana for a 12 m radius, +15 mana per extra 5 m. Faith density seldom helps (destructive). Evil alignments may discount cost; benevolent gods pay premium.

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| DPS | 50 vs units, 3% durability/sec vs buildings | Adjusted by alignment. |
| Spread Chance | 40% per second to adjacent tile | Reduced by moisture. |
| Duration | 20s base | Extended by additional focus. |
| Fear Aura | +25% panic chance | Enemies may flee. |

Edge cases: fire in rain dissipates quickly; fire underground limited to tunnels.

---

## System Interactions

- **Building Durability:** Accelerates damage thresholds.  
- **Roads:** Burns wooden bridges, damages low-tier roads.  
- **Healthcare:** Generates burn victims for clinics.  
- **Faith:** Destruction near worshippers may reduce faith unless justified (defense).

---

## Balance

- Friendly fire risk encourages precise use.  
- Cooldown ~60s.  
- Moral consequences: repeated civilian burns trigger alignment shifts.

---

## Technical Notes

- Uses fire propagation system already defined for environmental fires.  
- Firestorm simply seeds high-intensity nodes; rest handled by existing spread logic.  
- Deterministic random seeds for spread to ensure replay stability.

---

## Open Questions

1. Should fire miracles stoke volcanoes or lava flows?  
2. Can players inscribe patterns (sigils) in fire for intimidation bonuses?  
3. How do fire miracles interact with creatures (fear vs frenzy)?

---
