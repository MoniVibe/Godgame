# Goal Card: Profile Conflict (Warlike vs Peaceful)
ID: social_profile_conflict_warlike_vs_peaceful_v0
Date: 2026-02-09
Owner: shonh
Status: draft

## Goal
Validate that outlook profiles drive distinct behavior under conflict: warlikes escalate and refuse surrender; peacefuls attempt reason/conversion with limited success.

## Hypotheses
- Warlikes prioritize lethal outcomes and avoid prisoner-taking unless explicitly constrained.
- Peacefuls attempt de-escalation and can convert a small fraction based on charisma vs target resistance.
- Corrupt or chaotic profiles break truces more often than lawful/pure profiles.

## Scenario Frame
Theme: Moral conflict between a warlike raiding party and a peaceful commune.
Why this scenario matters: It anchors outlook-driven behavior and social conversion as measurable systems.

## Setup
Map/Scene: Village perimeter with a neutral meeting space.
Actors: 8 warlikes, 8 peacefuls.
Equipment/Loadouts: non-lethal and lethal options for warlikes; persuasion toolkit for peacefuls.
Rules/Constraints: warlikes may initiate combat after warning window; peacefuls may attempt conversion before fighting.
Duration: 90 seconds

## Schedule Regime
Time base: scenario.
Profiles: warlike_raider and peaceful_mediator active throughout.
Training windows: warning/parley (0-20s), conflict (20-90s).
Attendance rules: if any group fails to spawn, mark run invalid.
Reuse notes: use for other outlook-vs-outlook conflict tests.

## Needs and Shift Overrides
Needs modeled: hunger, fatigue, injury, morale.
Thresholds: soft breaks at 0.6, hard at 0.85.
Override rules: soft finish current action; hard immediate interrupt.
Profile nuance: warlikes ignore soft hunger during conflict; peacefuls treat injury as hard break.
Examples: warlike fanatics eat during shift; corrupts flee if advantage drops.

## Profile Interplay and Outcomes
Interplay focus: conflict (this is a conflict-heavy variant; other scenarios can use negotiation or cooperation).
Conflicting profiles:
- Warlike raiders: pure + evil + warlike + authoritarian (ruthless escalation).
- Peaceful mediators: pure + good + peaceful + cooperative (parley-first posture).
Expected outcomes:
- Warlikes prefer lethal outcomes, low surrender acceptance, minimal prisoners.
- Peacefuls attempt conversion in parley window; conversion success 5-20 percent depending on charisma vs target resistance.
Outcome reporting:
- Track kills, surrenders, and conversions split by profile tags (warlike vs peaceful, pure vs corrupt).
- Report parley attempts and acceptance rate by profile group.
Notes: Interplay does not have to be conflict; the intent is dynamic profile expression with or against other profiles or the simulation itself.

## Script
1. Both groups move to meeting space.
2. Peacefuls attempt persuasion/conversion during parley window.
3. Warlikes decide whether to attack; conflict unfolds for remainder.

## Metrics
- conflict.kills: total kills by warlikes.
- conflict.surrenders: peacefuls surrendered vs killed.
- social.conversions: peacefuls converted to warlike or vice versa.
- social.parley_success_rate.

## Scoring
- Score = (kills * 0.4) + (1 - parley_success_rate) * 0.2 + (conversions * 0.4)

## Acceptance
- warlikes kill rate > peacefuls by meaningful margin.
- conversions occur but stay below 30 percent without high charisma.

## Nightly Focus
Scenario ID: godgame_profile_conflict_warlike_vs_peaceful_micro
Run budget: 90s sim, 1 seed
Pass gates: kill rate, conversion rate, parley outcomes
Do not regress: determinism, conversion telemetry
Priority work:
- Implement parley window and conversion attempt logic.
- Wire outlook profile traits into combat vs persuasion choices.
Telemetry IDs: godgame.q.social.profile_conflict_score

## Branch Plan
Branch name: scenarios/godgame/profile-conflict
Merge criteria: pass gates + stable telemetry keys + reviewed by lead
Owner/Reviewer: shonh / TBD

## Dependencies
- Outlook profile system
- Persuasion/convert mechanics
- Conflict initiation rules

## Risks/Notes
- If profiles are stubbed, behavior will be flat; keep as design target.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_profile_conflict_warlike_vs_peaceful_micro.json
Version: v0
