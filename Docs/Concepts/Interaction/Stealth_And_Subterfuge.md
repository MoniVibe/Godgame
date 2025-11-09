# Stealth & Subterfuge Network

**Status:** Concept  
**Category:** Interaction / Systems  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Covert agents slip through settlements, temples, and rival domains by bending light, social cover, and divine attention. Stealth governs how well an entity avoids mortal perception, while subterfuge governs how they misdirect witnesses and manipulate faction relations without exposing their true allegiance.

---

## Player Experience Goals

- **Emotion:** Feel the tension of orchestrating covert miracles or sabotages while hoping villagers and guardians never notice.  
- **Fantasy:** Become the unseen hand—dispatching apostles, spies, or corrupted villagers to reshape the world without overt divine intervention.  
- **Memorable Moment:** A double agent sabotages a rival god’s shrine under cover of night, framing a neighbouring faction and igniting a proxy conflict.

---

## Core Mechanic

### Trigger / Activation
- Any entity carrying the `StealthProfile` blessing (agents, villager cult cells, cursed champions) may enter stealth stance.
- Stealth stance consumes interaction bandwidth (slower work rates, limited conversation options) in exchange for reduced detection.

### Behaviour
- Observers run deterministic `PerceptionChecks = BasePerception + LightModifier + VigilanceAssist` against the agent’s `StealthScore`.
- Light levels modulate detection: full sun grants bonuses to observers; deep shadow or extinguished braziers penalize them.
- When contesting guarded assets, agents roll finesse or intelligence checks against guardian vigilance. Outcomes range from success, partial suspicion, exposure, to catastrophic reveal (identity + motive).
- Agents may perform misdirection actions, impersonating third parties to redirect blame. On success, target factions accrue enmity toward the framed group; on failure suspicion spikes toward the real sponsor.

### Feedback
- **Visual:** Stealth shimmer intensity, light overlays showing safe / risky zones, suspicion halos around alerted guardians.
- **Audio:** Whispered cues when detection risk rises; choral alarms on exposure to underline divine awareness.
- **Numerical:** Suspicion meters, opposed roll breakdowns, faction relationship deltas after misdirection.

### Cost
- Entering stealth drains tempo (reduced labour, limited miracle access).  
- Misdirection consumes favour or material bribes.  
- Failed rolls raise `SuspicionScore`, risking tribunals, inquisitions, or miracles backfiring.

---

## Key Parameters

| Parameter | Value (Draft) | Reasoning |
|-----------|---------------|-----------|
| `StealthScore` | 0.6 | Balanced baseline before gear/blessing modifiers. |
| `LightLevel` | 0.0–1.0 | 0 = pitch night, 1 = direct sun; stored per area tick. |
| `LightModifier` | -0.4–+0.2 | Darkness penalty up to -0.4 on perception; sunlight bonus up to +0.2. |
| `GuardianVigilance` | 0.5 | Represents diligence of champions, peacekeepers, priests. |
| `SuspicionThreshold` | 0.7 | Breach point where inquisitions or divine audits trigger. |

### Edge Cases
- **Critical Exposure:** Guardian crit success learns identity; agent crit failure reveals motives and sponsoring god.  
- **Ambient Swings:** Miracles (eclipses, sacred flames) recalc modifiers instantly.  
- **Spectral Sight:** Entities blessed to see spirits halve darkness penalties and perceive ghosts, illusions, and fake walls.  
- **Illusionary Terrain:** Mage-built false walls add stealth, but unbriefed agents risk disorientation penalties.

### Failure States
- Suspicion beyond threshold triggers divine intervention or mob justice.  
- Failed misdirection may invert enmity, punishing the sponsor directly.  
- Repeated stealth actions can erode villager trust, reducing prayer power inflow.

---

## System Interactions

- **Villager Lifecycles:** Converts disgruntled villagers into covert cells; failure may trigger purges.  
- **Miracle Economy:** Darkness miracles or daylight blessings shift light modifiers; stealth acts may require prayer power upkeep.  
- **Alignment System:** Good-aligned gods suffer harsher penalties when covert harm is exposed; evil-aligned gain fear bonuses.  
- **City Impressiveness:** Well-lit, impressive cities reduce stealth viability; neglected districts empower covert ops.  
- **Truth Sources Link:** Requires `StealthProfile`, `PerceptionSensor`, `LightExposure`, `SuspicionScore`, `SpectralSight` components for deterministic simulation.

---

## Progression & Unlock

- **Unlock Condition:** Mid-game quests unlock stealth blessings or rogue guild contracts.  
- **Upgrade Path:** Improved stealth gear, darkness miracles, spectral sight rituals.  
- **Mastery:** Players learn to pre-plan lighting, rotate agents to manage suspicion, and combine misdirection with diplomacy.

---

## Balance

- **Power Level:** Should open alternate paths, not replace miracles or open warfare.  
- **Counterplay:** Guardians, inquisitors, illumination miracles, surveillance wards.  
- **Tuning Knobs:** Light attenuation curves, suspicion decay rate, misdirection severity, critical band thresholds.

---

## Technical Constraints

- Needs deterministic light sampling service (shared PureDOTS module).  
- Suspicion adjustments must be incremental to avoid structural thrash.  
- Multiplayer (future) must replicate stealth rolls via shared seeds.

---

## Visuals & Audio

- **Visual Style:** Subtle, shadow-driven, with divine glyph overlays when exposure spikes.  
- **VFX:** Torch sabotage sparks, spectral reveals, illusion shimmer.  
- **SFX:** Heartbeat escalation, whispers, inquisitorial bells.

---

## UI/UX

- **Controls:** Toggle stealth stance via hotkey or radial menu; misdirection actions from target context menu.  
- **HUD:** Suspicion meter, light meter, guardian vigilance indicators.  
- **Tutorial:** Scenario showing dusk infiltration vs daylight failure.

---

## Success Metrics

- **Usage:** ≥ 40% of mid-game sessions deploy at least one stealth op.  
- **Fun:** Playtests report “tense and rewarding” stealth experiences.  
- **Clarity:** Players understand light modifiers within first tutorial stealth mission.

---

## Acceptance Criteria

- [ ] Deterministic stealth vs perception checks implemented.  
- [ ] Light modifier service integrated with PureDOTS environment sampling.  
- [ ] Suspicion system interacts with Alignment and Law mechanics.  
- [ ] Misdirection outcomes adjust faction relations and spawn events.  
- [ ] UI communicates stealth risk, suspicion, and misdirection fallout.  
- [ ] Tutorial scenario authored and validated.

---

## Open Questions

1. Should gods detect covert actions automatically, or require invested miracles / oracles?  
2. How does stealth interact with creature companions (leash vs autonomous infiltrators)?  
3. What penalties or rewards apply when players deliberately expose their own agents for narrative gain?

---

## Alternatives Considered

- **Pure RNG Stealth:** Discarded—needs deterministic replay and player agency.  
- **Binary Visibility:** Rejected—stealth should degrade gradually via suspicion.  
- **Instant Exposure on Light:** Too punitive; current model allows mitigations through blessings or gear.

---

## Implementation Notes

- Author `StealthProfile`, `PerceptionSensor`, `LightExposure`, `SuspicionScore`, `SpectralSight` truth source components in PureDOTS and surface bakers in Godgame project.  
- Extend event streams to broadcast stealth successes/failures for narrative scripting.  
- Leverage PureDOTS spatial services to cache light probes per district to keep performance stable.

---
