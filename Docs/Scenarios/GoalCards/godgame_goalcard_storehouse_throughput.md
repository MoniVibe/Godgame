# Goal Card: Storehouse Throughput
ID: economy_storehouse_throughput_v0
Date: 2026-02-08
Owner: shonh
Status: draft

## Goal
Measure the stability and throughput of villager resource delivery to the Storehouse under light-to-moderate load.

## Hypotheses
- Delivery rate remains steady after initial ramp-up.
- Idle time stays low when jobs are available.

## Setup
Map/Scene: Village hub
Actors: 12 villagers
Equipment/Loadouts: default villager settings
Rules/Constraints: continuous gather/deliver loop
Duration: 90 seconds

## Schedule Regime
Time base: scenario (90s).
Profiles: gatherer and hauler roles active throughout.
Training windows: warm-up (0-10s), steady-state throughput (10-90s).
Attendance rules: villagers remain in gather/deliver loop; fallback to idle if jobs missing.
Reuse notes: reuse for warehouse and logistics throughput variants.

## Script
1. Spawn villagers near VillageCenter and Storehouse.
2. Enable gather/deliver jobs to Storehouse.
3. Record deliveries and idle time.

## Metrics
- economy.deliveries_per_minute
- economy.idle_time_ratio
- economy.queue_depth

## Scoring
- Score = (deliveries_per_minute_norm * 0.6) + ((1 - idle_time_ratio) * 0.4)

## Acceptance
- economy.deliveries_per_minute >= baseline * 0.8
- economy.idle_time_ratio <= 0.25

## Variants
- Increase villagers to 20
- Increase Storehouse distance

## Telemetry/Outputs
- godgame.q.economy.storehouse_throughput
- Delivery count log

## Dependencies
- Job assignment
- Resource carry and deposit
- Storehouse capacity tracking

## Risks/Notes
- If resource jobs are stubbed, throughput will be flat; keep as target.

## Scenario JSON
Path: Assets/Scenarios/Godgame/godgame_storehouse_throughput_micro.json
Version: v0
