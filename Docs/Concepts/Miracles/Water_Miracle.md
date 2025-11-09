# Miracle: Water Burst

**Status:** Concept  
**Category:** Miracle – Utility  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Creates an instant moisture explosion at the target point, flooding the area, extinguishing fires, cleansing toxins, and rehydrating villagers. Unlike Rain, Water Burst is immediate and localized, ideal for emergency responses.

---

## Player Experience Goals

- **Emotion:** Panic button satisfaction—slam a water sphere down and save lives.  
- **Fantasy:** Summon geysers and tidal bursts anywhere.  
- **Memorable Moment:** Detonate a water pulse inside a burning granary, saving grains while blasting raiders off their feet.

---

## Core Mechanic

### Activation
- **Sustained Cast**: Hold to create a continuous geyser, sweeping it to douse flames or irrigate rows; drains resources per second.  
- **Throw Cast**: Grab a water sphere and launch it via Slingshot (precise arc) or Velocity Throw (quick lob). Explosion happens on impact, consuming cost upfront.  
- UI offers quick toggle between burst sizes prior to release.

### Behavior
1. Spawns a `MoistureBurst` entity with radius, volume, knockback.  
2. Applies immediate moisture increase to terrain, resetting fire fuel counters.  
3. Cleanses burning or poisoned villagers; may knock light entities prone.  
4. Excess water forms puddles that slowly evaporate; can flood basements if overused.

### Feedback
- **Visual:** Expanding water shockwave, mist, lingering puddles.  
- **Audio:** Low boom with rushing water.  
- **UI:** Radius preview before cast; post-cast shows moisture saturation.

### Cost
- Small: 400 prayer, 20 mana. Medium: 700 prayer, 35 mana. Large: 1200 prayer, 60 mana. Faith density reduces cost; mana debt applies if overspent.

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| Radius | 8 / 14 / 20 m | Larger blasts risk collateral (washing away roads). |
| Knockback | 2 / 4 / 6 m | Friendly fire toggles whether allies get tossed. |
| Cleanse Strength | 1.0 | Removes burns/poisons up to severity 1; higher severity needs heal miracle. |
| Flood Duration | 10–25s | Converts dirt to mud, slowing movement. |

---

## System Interactions

- **Fire System:** Immediate suppression.  
- **Healthcare:** Removes burn/poison statuses, but may induce hypothermia if spammed.  
- **Road Durability:** Flooding accelerates decay on poor roads.  
- **Faith Density:** Villager gratitude increases local faith when life-saving use detected.

---

## Balance

- Cooldown to prevent constant spam (e.g., 45s).  
- Overuse leads to negative effects (mud, washed seed).  
- Evil alignment may convert water burst into corrosive sludge variant.

---

## Technical Notes

- Uses `MoistureBurst` component; integrates with fire/weather data.  
- Burst spawns temporary “wet zone” entities controlling mud speed penalties.  
- Optionally chain with Rain by triggering atmospheric moisture replenishment.

---

## Open Questions

1. Should water bursts push floating debris realistically?  
2. Do we allow players to shape the burst (cone, wall)?  
3. Should large bursts spawn fish / resources as easter eggs?

---
