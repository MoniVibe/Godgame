# Godgame Behavior Snapshot v1 - Implementation Notes

- When repetition threshold is detected in headless loop, write a single JSON snapshot to `villager_repetition_snapshot.json` in the headless output directory.
- Snapshot includes the top 5 repeating villager entities (ranked by thrash transitions), their JobPhase, target entity id (if any), and current destination.
- Uses offender list first; falls back to highest-transition agents if offenders are empty.

Validation
- PENDING (Build Marshal)
