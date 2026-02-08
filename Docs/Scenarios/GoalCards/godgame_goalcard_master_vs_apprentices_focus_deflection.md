# Goal Card: Master vs Apprentices Focus Drill
ID: combat_focus_master_vs_apprentices_v0
Date: 2026-02-08
Owner: shonh
Status: active

## Goal
Demonstrate that a master mage using Focus and projectile deflection outperforms apprentices in accuracy, reaction time, and disabling control in a controlled training duel.

## Hypotheses
- The master deflects most incoming projectiles while maintaining a low self-hit rate.
- The master disables apprentices faster and more consistently than apprentices can land hits.
- Focus cycles reduce time-to-disable without inflating misfires or target lock churn.

## Scenario Frame
Theme: Training drill to validate mastery vs apprentice skill gaps.
Why this scenario matters: It anchors spell combat pacing, focus mechanics, and experience scaling in a readable, measurable micro.

## Setup
Map/Scene: Training arena (flat, unobstructed)
Actors: 1 master mage, 8 apprentices
Equipment/Loadouts: Standard spell set + Focus mechanic (deflect + disable)
Rules/Constraints: Apprentices keep ring formation; no movement during casting unless dodging is explicitly enabled.
Duration: 60 seconds

## Roles and Experience
- Seats or roles: master_focus, apprentice_caster
- Experience tiers: master (elite) vs apprentices (rookie/experienced mix)
- Skill effects per seat:
  - master_focus: reaction_time_ms, focus_gain, deflect_accuracy, disable_success
  - apprentice_caster: aim_spread, lock_time, fire_rate variance

## Behavior Profile
Cooperation: apprentices coordinate cadence (staggered volleys), master acts solo.
Target sharing: apprentices broadcast the master target; master retargets apprentices based on disable queue.
Discipline: apprentices hold fire if master is already deflecting; master prioritizes uninterrupted disable loops.
Failure modes: focus thrash, target lock oscillation, deflect timing drift.

## Targeting and Fire Control
Detection: direct LOS only, no stealth.
Target selection: apprentices always target master; master targets highest threat apprentice.
Lock time: apprentices have longer lock time; master near-instant with Focus.
Track loss: lock drops on deflect or disable; apprentices reacquire with delay.
Firing solution: straight-line projectiles; no lead required in stationary phase.

## Movement and Orientation
Formation: apprentices in ring at fixed radius, equal spacing.
Rotation limits: master yaw/pitch constrained by focus stance; apprentices minimal torso rotation.
Facing rules: remain facing target; no backcasting.
Speed profile: stationary by default; optional dodge variant later.

## Weapons and Arcs
Weapon types: projectile bolts (deflectable), disable beam (focus-only).
Firing arcs: 180 deg forward arc; projectiles blocked if out of arc.
Ammo and heat: infinite for test; heat disabled unless variant enabled.

## Nuance Prompts (fill what applies)
Perception: LOS only; no occlusion in base scenario.
Coordination: apprentices stagger cadence to avoid over-saturation.
Reaction timing: master can pre-queue deflect on incoming projectiles.
Skill/stat modifiers: master has lower reaction_time_ms and higher deflect_accuracy.
Morale/discipline: apprentices do not panic; master never flees.
Environment/interference: none.
Failure cases: deflect mistimes cause self-hit; disable over-focus causes missed deflects.
Determinism cues: fixed seed, fixed ring placement, fixed volley cadence.

## Script
1. Apprentices begin a staggered firing pattern toward the master.
2. Master cycles Focus between apprentices, casting disable and deflection spells.
3. Continue for duration; capture projectile outcomes and disable events.

## Metrics
- master.deflect_rate: deflected_projectiles / incoming_projectiles
- master.hit_rate: hits_on_master / incoming_projectiles
- master.disable_rate: disables_per_minute
- apprentice.hit_rate: total_hits_on_master / total_projectiles
- focus.uptime: focused_time / duration
- lock.reacquire_time_ms: average time to reacquire after disable or deflect

## Scoring
- Score = (deflect_rate * 0.5) + (disable_rate * 0.3) + ((1 - master.hit_rate) * 0.2)

## Acceptance
- master.deflect_rate >= 0.70
- master.hit_rate <= 0.20
- master.disable_rate >= 6 per minute

## Regression Guardrails
- Determinism preserved across identical seeds.
- No increase in lock churn beyond baseline.

## Nightly Focus
Scenario ID: godgame_master_vs_apprentices_micro
Run budget: 60s sim, 1 seed
Pass gates: deflect_rate, hit_rate, disable_rate
Do not regress: determinism, focus.uptime
Priority work:
- Implement deflect timing with focus bonus
- Add disable spell with cooldown + duration
- Wire projectile telemetry for deflect/hit
Telemetry IDs: godgame.q.focus.deflection.master_score

## Branch Plan
Branch name: scenarios/godgame/master-vs-apprentices
Merge criteria: pass gates + stable telemetry keys + reviewed by lead
Owner/Reviewer: shonh / TBD

## Variants
- Increase apprentices to 12
- Increase projectile speed by 25 percent
- Enable apprentice dodge behavior

## Telemetry/Outputs
- godgame.q.focus.deflection.master_score
- Projectile hit and deflect counts

## Dependencies
- Focus mechanic
- Projectile deflection and collision
- Disable status effects
- Mage prefabs for master/apprentice roles

## Risks/Notes
- Focus/deflection are currently stubbed; outcomes will be flat until spell + projectile systems land. Keep this as a design target.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_master_vs_apprentices_micro.json
Version: v0
