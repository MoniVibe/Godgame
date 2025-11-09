# Miracle: Rain

**Status:** Concept  
**Category:** Miracle – Weather  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Summons rain clouds that can be picked up and thrown. Clouds glide along the throw vector, dispersing moisture over time to extinguish fires, irrigate crops, and cool overheated villagers.

---

## Player Experience Goals

- **Emotion:** Relief and control when dousing wildfires or reviving parched fields.  
- **Fantasy:** Godlike weather sculpting—literally grab clouds, toss them where needed.  
- **Memorable Moment:** Catch a raging forest fire about to consume a village by dragging in a massive thunderhead and watching flames sputter out.

---

## Core Mechanic

### Activation
- **Sustained Cast**: Select Rain → hold cast button while dragging in the sky to grow/guide a cloud. Release to let it drift.  
- **Throw Cast**: Pull a condensed rain orb into hand → use **Slingshot** for arced placement or **Velocity Throw** for snap toss; on impact it spawns a cloud that begins dispersing.  
- Costs prayer proportional to cloud size; sustained mode drains over time, throw mode charges upfront. Cooldown shared with other weather miracles.

### Behavior
1. Spawns a `RainCloudComponent` with mass, moisture, altitude, glide speed.  
2. Player can grab/throw the cloud; while airborne it drifts and rains beneath its path.  
3. Rain applies moisture to terrain, reduces fire damage ticks, replenishes soil for crops, and cools villagers.  
4. Cloud dissipates when moisture hits zero or after fixed lifetime.

### Feedback
- **Visual:** Cloud size, darkness, lightning flashes indicating moisture left. Rain streak density scales with intensity.  
- **Audio:** Rolling thunder loops, rainfall volume tied to output rate.  
- **UI:** Moisture meter and coverage overlay showing where rain landed.

### Cost
- Base cost 500 prayer per Medium cloud; +50% cost per tier. Faith density reduces cost up to 50%. Entering mana debt possible for emergency storms.

---

## Key Parameters

| Parameter | Draft Value | Notes |
|-----------|-------------|-------|
| Cloud Lifetime | 60s | Dissipates sooner if low altitude. |
| Moisture Pool | 100 / 200 / 400 | Scales with size tier. |
| Glide Speed | 4 m/s | Affected by wind, altitude, god throw strength. |
| Fire Suppression | -5% DPS per second | Stacks per cloud; capped at -90%. |

Edge cases: high winds may blow clouds off course; extreme heat evaporates moisture faster.

---

## System Interactions

- **Building Durability:** Rain arrests fire damage and slows decay.  
- **Agriculture:** Boosts crop growth, prevents drought penalties.  
- **Roads:** Excess rain muddies low-quality roads, reducing speed.  
- **Healthcare:** Cools feverish villagers, reducing heatstroke risk.  
- **Miracle Framework:** Uses shared `RainCloudComponent`.

---

## Balance

- Limit simultaneous clouds to prevent trivial fire mitigation.  
- Larger clouds have slower response (more wind drag).  
- Evil-aligned gods may have weaker rain (capped at 30% suppression) unless they invest in benevolent doctrines.

---

## Technical Notes

- Clouds are DOTS entities with motion/dispersion jobs.  
- Moisture application writes to terrain moisture buffers; fire systems read moisture to adjust damage.  
- Hand input uses existing grab/throw pipeline.

---

## Open Questions

1. Should rain buff faith in affected villages?  
2. How does rain interact with lightning miracles (auto-thunderstorms)?  
3. Do clouds collide with mountains or clip through?

---
