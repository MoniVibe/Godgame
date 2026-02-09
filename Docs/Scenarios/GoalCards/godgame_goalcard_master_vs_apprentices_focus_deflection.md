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
Cross-domain mapping: “master mage” equals apex operator/ship; this drill is a skill-expression testbed that ports to gunnery/archery/piloting with PureDOTS timing/accuracy primitives.

## Relations and Social Context
Role map: master (mentor), apprentices (cohort trainees).
Relations: high cohesion expected; coordination improves with cohesion, panic is suppressed.
Spectators: optional observers at edge (no interaction unless variant enabled).
Safety rule: non-lethal training; disable/interrupt instead of lethal outcomes.

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

## Spell Arsenal and Projectiles
Master kit: focus stance, deflect window, disable beam, optional barrier pulse.
Apprentice kit: basic projectile bolt, minor disrupt (low damage, low control).
Projectile tags: kinetic, magic, disruptive, homing (optional variant).
Projectile diversity: some bolts are harder to dodge but easier to deflect; others invert this.

## Schedule Regime
Time base: scenario (60s training window).
Profiles:
- master_focus: training block (0-60s), session focus_drill.
- apprentice_caster: training block (0-60s), session focus_drill.
Training windows: focus_drill pairs mentor master_focus with apprentice_caster trainees.
Attendance rules: require overlap during training; fallback to idle if session missing.
Reuse notes: same regime can drive other mage drills or trainee vs mentor variants.

## Targeting and Fire Control
Detection: direct LOS only, no stealth.
Target selection: apprentices always target master; master targets highest threat apprentice.
Lock time: apprentices have longer lock time; master near-instant with Focus.
Track loss: lock drops on deflect or disable; apprentices reacquire with delay.
Firing solution: straight-line projectiles; no lead required in stationary phase.
Deflection model: windowed deflect (coarse, low-overhead); resolve per volley window, not per projectile.
PureDOTS note: timing/accuracy/deflection logic is shared; this scenario only tunes parameters.

## Mana and Focus Economy
Master: focus uptime prioritized; deflect and disable consume focus; barrier consumes mana.
Apprentices: small mana pool; consistent cadence over burst.
Costs: proportional to projectile threat and action type.

## Movement and Orientation
Formation: apprentices in ring at fixed radius, equal spacing.
Rotation limits: master yaw/pitch constrained by focus stance; apprentices minimal torso rotation.
Facing rules: remain facing target; no backcasting.
Speed profile: stationary by default; optional dodge variant later.

## Damage and Safety Rules
Damage model: training-safe; disable/interrupt > lethal.
Injury thresholds: when exceeded, apprentice is sidelined and stops casting.
Sidelined students: move to safe edge; telemetry marks removal reason.

## Weapons and Arcs
Weapon types: projectile bolts (deflectable), disable beam (focus-only).
Firing arcs: 180 deg forward arc; projectiles blocked if out of arc.
Ammo and heat: infinite for test; heat disabled unless variant enabled.
Deflect rules: deflect only inside arc; successful deflect redirects or nullifies.

## Nuance Prompts (fill what applies)
Perception: LOS only; no occlusion in base scenario.
Coordination: apprentices stagger cadence to avoid over-saturation.
Reaction timing: master can pre-queue deflect on incoming projectiles.
Skill/stat modifiers: master has lower reaction_time_ms and higher deflect_accuracy.
Morale/discipline: apprentices do not panic; master never flees.
Environment/interference: none.
Failure cases: deflect mistimes cause self-hit; disable over-focus causes missed deflects.
Determinism cues: fixed seed, fixed ring placement, fixed volley cadence.
Overhead control: cap deflect checks per tick; fallback to volley-level deflect if budget exceeded.

## Addendum (Optional)
Path: Docs/Scenarios/GoalCards/Addenda/godgame_master_vs_apprentices_notes.md
Notes: Longform combat nuance and deflection timing details.

## Script
1. Warmup (0-5s): apprentices acquire targets, no scoring.
2. Live fire (5-55s): apprentices begin staggered volleys; master cycles Focus, deflects, and disables.
3. Cooldown (55-60s): stop casting; capture late telemetry and resolve lingering projectiles.

## Metrics
- master.deflect_rate: deflected_projectiles / incoming_projectiles
- master.hit_rate: hits_on_master / incoming_projectiles
- master.disable_rate: disables_per_minute
- apprentice.hit_rate: total_hits_on_master / total_projectiles
- focus.uptime: focused_time / duration
- lock.reacquire_time_ms: average time to reacquire after disable or deflect
- master.deflect_timing_error_ms: mean absolute error between deflect window and impact
- apprentice.volley_cadence_variance: variance of inter-volley spacing
- master.mana_spent_per_deflect: mana_spent / deflects
- apprentice.mana_spent_rate: mana_spent / minute
- apprentice.sidelined_count: count of apprentices removed due to injury
- learning.delta_skill: aggregate skill delta across participants

## Scoring
- Score = (deflect_rate * 0.5) + (disable_rate * 0.3) + ((1 - master.hit_rate) * 0.2)

## Acceptance
- master.deflect_rate >= 0.70
- master.hit_rate <= 0.20
- master.disable_rate >= 6 per minute
- Secondary (non-gating): deflect_timing_error_ms <= 80ms; volley_cadence_variance <= 0.20

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
Telemetry IDs: godgame.q.focus.deflection.master_score, godgame.q.focus.deflection.deflect_rate, godgame.q.focus.deflection.disable_rate

## Branch Plan
Branch name: scenarios/godgame/master-vs-apprentices
Merge criteria: pass gates + stable telemetry keys + reviewed by lead
Owner/Reviewer: shonh / TBD

## Variants
- Increase apprentices to 12
- Increase projectile speed by 25 percent
- Enable apprentice dodge behavior
- Add spectators with morale signal only
- Enable barrier pulse (master only)

## Telemetry/Outputs
- godgame.q.focus.deflection.master_score
- Projectile hit and deflect counts

## Dependencies
- Focus mechanic
- Projectile deflection and collision
- Disable status effects
- Mage prefabs for master/apprentice roles
- Mana/focus resource tracking
- Injury/sideline handling

## Risks/Notes
- Focus/deflection are currently stubbed; outcomes will be flat until spell + projectile systems land. Keep this as a design target.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_master_vs_apprentices_micro.json
Version: v0
