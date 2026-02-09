# Goal Card: Villager Crowd Flow
ID: movement_crowd_flow_v0
Date: 2026-02-08
Owner: shonh
Status: draft

## Goal
Validate that dense villager movement around core buildings remains stable, collision-free, and avoids stuck agents under crowd pressure.

## Hypotheses
- Stuck rate stays near zero even at high crowd density.
- Average movement speed remains within expected bounds despite congestion.

## Setup
Map/Scene: Village hub
Actors: 30 villagers
Equipment/Loadouts: default villager settings
Rules/Constraints: continuous roaming within hub radius
Duration: 60 seconds

## Schedule Regime
Time base: scenario.
Profiles: villager_roam active throughout.
Training windows: none; continuous roaming is the test.
Attendance rules: all villagers must remain assigned to roam behavior.
Reuse notes: use for plaza crowd tests and market congestion variants.

## Script
1. Spawn villagers around VillageCenter, Storehouse, and Housing.
2. Allow roaming/movement behavior to proceed without intervention.
3. Record movement, collision, and stuck events for duration.

## Metrics
- crowd.stuck_rate: stuck_agents / total_agents
- crowd.collision_rate: collisions / second
- crowd.avg_speed: average movement speed
- crowd.dwell_time: mean time spent idle or stalled

## Scoring
- Score = (1 - stuck_rate) * 0.5 + (avg_speed_norm * 0.3) + (1 - collision_rate_norm) * 0.2

## Acceptance
- crowd.stuck_rate <= 0.05
- crowd.avg_speed >= 70 percent of baseline

## Variants
- Increase villagers to 50
- Reduce hub radius by 30 percent

## Telemetry/Outputs
- godgame.q.movement.crowd_flow_score
- Stuck and collision counters

## Dependencies
- Villager movement + avoidance
- Collision detection

## Risks/Notes
- If avoidance is stubbed, stuck rate will be high; treat as baseline.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_villager_crowd_flow_micro.json
Version: v0
