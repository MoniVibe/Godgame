# Building Durability & Quality System

**Status:** Concept  
**Category:** Buildings / Construction  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Buildings are composite entities with durability, quality, rarity, and tech tier attributes—mirroring the modular item framework. Durability tracks structural integrity; quality/rarity boost performance, and tech tier controls the baseline stats. As structures take damage or suffer neglect, penalties kick in at thresholds, eventually rendering them unusable until repaired by builders or divine intervention.

---

## Durability Thresholds & Penalties

| Durability % | Status | Penalty |
|--------------|--------|---------|
| 100–75 | Stable | No penalty |
| 75–50 | Light Damage | Minor stat penalties (e.g., -5% output, -10% defensive rating) |
| 50–25 | Moderate Damage | Significant penalties (-20% output, -25% defense, slower services) |
| 25–0 | Critical | Building offline/unusable until repairs |

- Thresholds apply multiplicatively to outputs (production, housing capacity, aura strength).  
- Visual/UI cues shift (cracks, scaffolding) to signal each tier.  
- Damage sources: raids, weather, earthquakes, poor maintenance, deliberate sabotage.

---

## Quality, Rarity & Tech Tier

- **Quality**: Materials/workmanship; high quality raises base durability, slows decay.  
- **Rarity**: Grants unique affixes (e.g., temple that grants rare miracles, fortress with aura).  
- **Tech Tier**: Defines baseline stats; legendary-quality Tier 3 building ≈ Tier 6 common equivalent.  
- Use the same equivalence rules as items (Rare = +1 tier, Epic = +2, Legendary = +3).

---

## Repair & Maintenance

- **Builders**: Restore durability up to their skill cap (apprentice 70%, journeyman 85%, master 100%).  
- **God Intervention**: Spend resources/prayer power to instantly restore durability or upgrade quality.  
- **Upkeep Loops**: Assign maintenance crews to slow decay; neglect accelerates wear.  
- **Partial Repairs**: Low-skill repairs cap out early, leaving latent structural weaknesses until a better builder reworks the section.

### Fire & Weather Effects

- **Fires**: Continuous damage over time to buildings and crops. Durability ticks down each frame; once durability drops below **1% the structure despawns** (reduced to ash).  
- **Extinguishing**: Fires persist until heavy rain, water miracles, water mage spells, or flooding occurs. Air mages can fan flames, increasing spread speed and damage rate.  
- **Spread Logic**: Adjacent tiles receive ignition chance based on material flammability and wind direction.  
- **Rain / Clouds**: Rain events replenish soil moisture and reduce fire damage ticks. God-hand can pick up **rain clouds** of varying size/density and throw them; they glide along the throw vector, raining as they go. Altitude determines dispersion—higher throws cover wider areas but dilute rain intensity.

---

## Road Network Durability

Roads share the same durability/quality model as buildings but are initialized differently.

- **Spontaneous Paths**: When entities walk the same routes frequently, “basic roads” form automatically via navigation heatmaps. These crude paths cost nothing from the god’s perspective but have low durability/quality, degrading quickly under heavy traffic.  
- **Upgrades**: Builders can invest materials to upgrade paths into cobblestone, paved, or enchanted roads. Each tier raises move speed, reduces maintenance decay, and improves aesthetics.  
- **Attributes**:
  - **Durability**: Determines how long the road stays in good condition; low durability roads develop potholes causing movement penalties.  
  - **Quality/Rarity**: Rare/legendary roads (e.g., mithril-laced causeways) grant unique buffs (anti-corrosion, morale boosts).  
  - **Tech Tier**: Defines baseline speed multipliers; legendary Tier 3 road ≈ Tier 6 common, mirroring item equivalence.
- **Maintenance**: Road crews repair sections based on skill; gods can patch instantly with resource expenditure. Lack of upkeep reverts roads to dirt trails over time.

---

## Integration Hooks

- **Economy**: Repair costs consume materials (wood, stone, rare alloys).  
- **Events**: Quakes or sieges trigger durability loss events.  
- **Faith**: Higher-quality temples yield better miracles but demand expensive upkeep.  
- **PureDOTS Tie-in**: Mirror the item quality data model for building components (`WallSegment`, `Foundation`, `Roof`). Durability stored per component for deterministic simulation.

---

## Open Questions

1. Should certain penalties target services (hospital downgrade) vs stats (defense)?  
2. How do magical repairs (e.g., Earth miracles) interact with quality tiers?  
3. Do rare/legendary buildings require unique materials for maintenance?

---
