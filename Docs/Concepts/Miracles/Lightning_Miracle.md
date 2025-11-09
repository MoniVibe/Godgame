# Miracle: Lightning Lance

**Status:** Concept  
**Category:** Miracle – Offensive / Precision  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Calls down targeted lightning strikes to obliterate units, overload machinery, or stun spectral threats. Lightning can chain between conductive targets, breaking formations or powering dormant artifacts.

---

## Player Experience Goals

- **Emotion:** Surgical godlike power—zap exactly who needs smiting.  
- **Fantasy:** Zeus-style bolts from the heavens.  
- **Memorable Moment:** Snipe an enemy champion leading an assault, causing their troops to scatter.

---

## Core Mechanic

### Activation
- **Sustained Cast**: Hold to channel repeated strikes, dragging cursor to retarget mid-channel. Costs accrue per bolt.  
- **Throw Cast**: Manifest a lightning orb and fling it (slingshot for arced drop, velocity for snap). Orb detonates, releasing chained bolts from impact point.  
- Focus can be spent to steady channel or pre-charge thrown orbs for longer chain range.

### Behavior
1. Spawns `LightningArc` entity pointing at target.  
2. Deals instant burst damage, applies shock status (stun, disarm).  
3. Chains to nearby conductive targets within ChainRange until arc budget spent.  
4. Electrifies water puddles (from rain/water miracles), turning them into temporary hazards.

### Feedback
- **Visual:** Blinding bolt, branching arcs, glowing scorch marks.  
- **Audio:** Thunderclap synchronized with strike.  
- **UI:** Target preview lines showing potential chains.

### Cost
- 450 prayer + 25 mana per strike. Channeling adds 10 mana per extra bolt. Faith density lightly reduces cost; accuracy modifiers remain unaffected.

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| Base Damage | 300 | Ignores 50% armor. |
| Shock Duration | 3s | Scales with focus investment. |
| Chain Range | 10 m | Each chain deals -40% damage. |
| Overload Effect | +25% damage vs machines | Good for siege. |

Edge cases: hitting lightning rods intentionally triggers protective dispersal; hitting friendly water may stun allies unless toggled off.

---

## System Interactions

- **Weather:** Higher chance to trigger additional bolts during storms (rain miracle synergy).  
- **Healthcare:** Creates burn/shock patients needing advanced care.  
- **Faith:** Impressive critical smites boost local faith; collateral reduces it.  
- **Time Miracle:** Slowed zones make lightning easier to retarget mid-channel.

---

## Balance

- Cooldown ~20s per bolt.  
- Excessive use may attract storm events (random lightning).  
- Evil alignment may add corruption debuff (fear instead of faith gain).

---

## Technical Notes

- Lightning arcs simulated via deterministic raycasts; chain logic uses spatial query buffer.  
- Presentation uses VFX graph but subscribes through companion entities to keep DOTS hot path clean.

---

## Open Questions

1. Should lightning charge batteries/power grids for economy bonuses?  
2. Do we allow “hold to target” for manual arcs like Black & White 2?  
3. How do we handle underground targets?

---
