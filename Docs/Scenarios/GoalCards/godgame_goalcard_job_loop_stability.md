# Goal Card: Job Loop Stability
ID: economy_job_loop_stability_v0
Date: 2026-02-08
Owner: shonh
Status: draft

## Goal
Ensure villager job scheduling remains stable and avoids oscillation or deadlocks over a full loop.

## Hypotheses
- Job completion rate remains steady after warm-up.
- No long idle streaks when jobs exist.

## Setup
Map/Scene: Village hub
Actors: 8 villagers
Equipment/Loadouts: default villager settings
Rules/Constraints: use existing job_loop_01 scenario
Duration: 120 seconds

## Script
1. Run job_loop_01.
2. Record job assignment changes and completion events.
3. Evaluate idle streaks and loop stability.

## Metrics
- jobs.completed_per_minute
- jobs.idle_streak_seconds
- jobs.assignment_churn

## Scoring
- Score = (completed_per_minute_norm * 0.6) + (1 - idle_streak_norm) * 0.4

## Acceptance
- jobs.completed_per_minute >= baseline * 0.8
- jobs.idle_streak_seconds <= 10

## Variants
- Increase villagers to 16
- Reduce building spacing

## Telemetry/Outputs
- godgame.q.jobs.loop_stability_score
- Job assignment timeline

## Dependencies
- Job scheduler
- Task completion tracking

## Risks/Notes
- If scheduler is stubbed, treat outputs as baseline.

## Scenario JSON
Path: Assets/Scenarios/Godgame/job_loop_01.json
Version: v0
