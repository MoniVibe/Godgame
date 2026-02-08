# Goal Card: Master vs Apprentices Focus Drill
ID: combat_focus_master_vs_apprentices_v0
Date: 2026-02-08
Owner: shonh
Status: draft

## Goal
Demonstrate that a master mage using Focus and projectile deflection outperforms apprentices in accuracy, reaction time, and disabling control in a controlled training duel.

## Hypotheses
- The master deflects most incoming projectiles while maintaining low self-hit rate.
- The master disables apprentices faster and more consistently than apprentices can land hits.

## Setup
Map/Scene: Training arena (flat, unobstructed)
Actors: 1 master mage, 8 apprentices
Equipment/Loadouts: Standard spell set + Focus mechanic
Rules/Constraints: Apprentices remain in ring formation; no movement during casting
Duration: 60 seconds

## Script
1. Apprentices begin a synchronized firing pattern toward the master.
2. Master cycles Focus between apprentices, casting disable and deflection spells.
3. Continue for duration; capture projectile outcomes and disable events.

## Metrics
- master.deflect_rate: deflected_projectiles / incoming_projectiles
- master.hit_rate: hits_on_master / incoming_projectiles
- master.disable_rate: disables_per_minute
- apprentice.hit_rate: total_hits_on_master / total_projectiles
- focus.uptime: focused_time / duration

## Scoring
- Score = (deflect_rate * 0.5) + (disable_rate * 0.3) + ((1 - master.hit_rate) * 0.2)

## Acceptance
- master.deflect_rate >= 0.70
- master.hit_rate <= 0.20
- master.disable_rate >= 6 per minute

## Variants
- Increase apprentices to 12
- Increase projectile speed by 25 percent

## Telemetry/Outputs
- godgame.q.focus.deflection.master_score
- Projectile hit and deflect counts

## Dependencies
- Focus mechanic
- Projectile deflection and collision
- Disable status effects
- Mage prefabs for master/apprentice roles

## Risks/Notes
- If Focus or deflection is stubbed, outcomes will be flat; note stub behavior in results.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_master_vs_apprentices_micro.json
Version: v0
