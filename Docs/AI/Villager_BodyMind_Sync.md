# Villager Body/Mind Sync

**Date:** 2025-02-15  
**Audience:** Gameplay agents wiring the headless sim to the PureDOTS villager mind stack.

This spec explains how Godgame consumes the shared PureDOTS villager components (`VillagerNeedState`, `VillagerNeedTuning`, `VillagerGoalState`, `VillagerFleeIntent`, `FocusBudget`, `VillagerMindCadence`, `VillagerThreatState`) so we can develop behaviors headlessly while presentation moves forward independently.

## 1. Authoring

`Assets/Scripts/Godgame/Authoring/VillagerAuthoring.cs` bakes all PureDOTS mind components into each villager entity:

| Component | Purpose |
|-----------|---------|
| `VillagerNeedState` | Current urgency values (0–1) for hunger, rest, faith, safety, social, work. |
| `VillagerNeedTuning` | Per-villager decay/pressure rates + maximum urgency. |
| `FocusBudget` + buffer | Worker focus pool and reservations so Body systems can gate work. |
| `VillagerMindCadence` | Per-villager cadence override for mind updates. |
| `VillagerGoalState` | Output goal + urgency used by Body systems. |
| `VillagerFleeIntent` | When a threat wins, this encodes direction/urgency for Body steering. |
| `VillagerThreatState` | Tracks the latest threat target/direction/urgency. |

All values ultimately come from authoring fields (decay rates, focus regen, cadence) so designers can tune them without touching systems.

## 2. Headless Systems

Two Burst systems now live under `Assets/Scripts/Godgame/Villagers/` to keep those components updated every fixed tick:

1. **`VillagerFocusBudgetSystem`** (`VillagerFocusBudgetSystem.cs`)
   - Regenerates the shared `FocusBudget` pool each fixed step.
   - Sums the `FocusBudgetReservation` buffer to update `Reserved` and clamps `Current`.
   - Auto-unlocks focus when the pool recovers past 50%, ensuring workers resume tasks without Mono presentation hooks.

2. **`VillagerNeedUpdateSystem`** (`VillagerNeedUpdateSystem.cs`)
   - Runs on the mind cadence per villager (uses `CadenceGate.ShouldRun` + `VillagerMindCadence`).
   - Adds decay pressure from `VillagerNeedTuning` into `VillagerNeedState` fields.
   - Bleeds down `VillagerThreatState.Urgency` so threats de-escalate.
   - Picks the highest urgency between needs and threat to set `VillagerGoalState.CurrentGoal` and `CurrentGoalUrgency`.
   - If threat wins, writes `VillagerFleeIntent` (direction = away from threat, urgency = threat).

These systems run in `FixedStepSimulationSystemGroup`, before `VillagerIntentBridgeSystem`, so the Body loop always consumes fresh goals even in headless builds.

## 3. Body Bridge

`VillagerIntentBridgeSystem` already respected the new components; once `VillagerNeedUpdateSystem` sets `VillagerGoalState` and `VillagerFleeIntent`, the bridge:

- Checks `FocusBudget`/cadence before mutating jobs.
- Resets job state when the decision is non-work.
- Applies flee steering using `VillagerFleeIntent` + hazard lookup (shared steering math).

No presentation code is required: headless scenarios now produce the same job/flee behavior as the visual build.

## 4. Testing

Two EditMode tests protect the new pipeline:

| Test | Location | Coverage |
|------|----------|----------|
| `VillagerNeedUpdateSystemTests` | `Assets/Scripts/Godgame/Tests/` | Verifies that dominant needs select the correct goal and that threat urgency triggers a flee intent. |
| `VillagerFocusBudgetSystemTests` | `Assets/Scripts/Godgame/Tests/` | Ensures focus regeneration + reservations behave deterministically. |

Run via Unity EditMode runner (CI hook optional).

## 5. Telemetry & HUD

`GodgamePlayerHUDSystem` already reads `VillagerGoalState`, `VillagerNeedState`, and `FocusBudget`. With the new systems, the HUD now reflects real mind data even when presentation isn’t running.

`GodgameVillagerMindTelemetrySystem` (Telemetry folder) runs on the same cadence, aggregates villager count, goal distribution, average focus, and peak need urgency, and appends them to the shared `BehaviorTelemetryRecord` buffer (`BehaviorId.VillagerMind`). When the `GODGAME_BEHAVIOR_TELEMETRY_PATH` env var is set (headless builds do this automatically), `GodgameBehaviorTelemetryExportSystem` flushes those records to NDJSON so scenario reports include mind/focus stats alongside gather/dodge metrics.

`GodgameBehaviorTelemetryExportSystem` now also writes the AI audit streams so designers can answer “why did they do that?” without replaying the scene: decision transitions (with reason codes + top weighted options), action lifecycle events (start/end/fail timestamps with failure reasons), queue pressure samples for scenario command buffers, logic audit counters (invalid refs, NaNs, missing components), and ticket/claim sanity (double-claims, stuck claims, release durations). The audit data sits in the same NDJSON so downstream reports keep a single timeline.

---
**Next Steps**
1. Mirror `VillagerNeedUpdateSystem` metrics into behavior telemetry for golden scenarios.
2. Surface focus/need stats in docs (`Docs/AI/Behaviors`) once Earth-moving tests land.
3. Extend headless scenario runner to log goal changes for regression detection.
