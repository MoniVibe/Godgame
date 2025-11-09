# Miracle: Divine Heal

**Status:** Concept  
**Category:** Miracle – Support  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Restores health, cleanses ailments, and accelerates recovery for villagers, armies, and creatures. Scales with faith density and caster focus; can stabilize coma patients or reverse mana debt consequences when used skillfully.

---

## Player Experience Goals

- **Emotion:** Compassion and relief after catastrophic events.  
- **Fantasy:** Lay-on-hands across entire settlements.  
- **Memorable Moment:** Heal a devastated army mid-battle, turning retreat into victory.

---

## Core Mechanic

### Activation
- **Sustained Cast**: Hold to maintain a healing beam or sweeping aura over targets; classic channel mode with per-second drain.  
- **Throw Cast**: Launch a healing mote that detonates on landing, releasing a burst heal (slingshot for precision, velocity for quick toss).  
- Players can combo: throw to stabilize distant group, then sustain over survivors.

### Behavior
1. Spawns `HealPulse` entity applying heal ticks and cleanse effects within radius.  
2. Cancels non-lethal statuses (bleed, poison, burns) up to CleanseStrength.  
3. Stabilizes coma patients by reducing mana debt and shortening coma duration (when cast within first day).  
4. Overhealing converts to temporary buff (Fortified Health) or prayer refunds depending on alignment.

### Feedback
- **Visual:** Radiant glyph, golden light, restorative vines.  
- **Audio:** Choir pads, chimes.  
- **UI:** Health bars surge; status icons fade when cleansed.

### Cost
- Burst: 500 prayer + 30 mana. Channel: 150 prayer + 10 mana upfront, 100 prayer + 5 mana per second. Faith fields reduce cost up to 50%. Focus cost optional for precision targeting.

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| Heal Amount | 120 per tick | Scales with Faith density. |
| Cleanse Strength | 2 | Removes moderate ailments; severe curses require higher tier. |
| Radius | 10 m | Channel mode can sweep across battlefield. |
| Coma Reduction | -1 day | Can prevent death if applied early. |

Edge cases: healing undead/evil creatures may harm them unless player toggles “dark inversion”.

---

## System Interactions

- **Healthcare:** Reduces clinic load; may anger doctors if overused (lost income).  
- **Mana Debt:** Slows or cancels coma, as per new mana rules.  
- **Faith:** High-profile saves massively boost faith; refusal to heal may lower it.  
- **Alignment:** Evil gods twist heal into life-drain variant if desired.

---

## Balance

- Cooldown 30s (Burst) / 45s (Channel).  
- Overuse causes “miracle fatigue” penalty, lowering effectiveness for short period.  
- Requires line-of-sight; cannot heal through mountains.

---

## Technical Notes

- Heal pulses run as ECS jobs writing to health buffers.  
- Integration with medical statuses ensures deterministic stacking (no double-heal).  
- Needs guard rails to prevent performance issues when channeling over hundreds of entities.

---

## Open Questions

1. Should heal miracles resurrect recently deceased?  
2. How does heal interact with prosthetics (repair vs regrow)?  
3. Do clinics charge fees after a miracle heals their patients?

---
