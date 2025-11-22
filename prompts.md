# Phase 2 Wrap + Phase 3 Lanes

Use this prompt to coordinate the next three agents. Keep engine-level scaffolding/tests under `PureDOTS/` and Godgame gameplay under `Assets/Scripts/Godgame`. Update `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` and relevant TODO docs as discoveries land.

**Current status**
- Crew growth systems, mining (with telemetry), and fleet intercept systems are in; old nested projects are cleaned up.
- Phase 2 demo: mining loop mostly done, carrier pickup done, time/rewind in progress (needs validation).

## Agent 1 – Phase 2 Demo Closure (recommended)
- Goal: close Phase 2 by proving time spine/rewind determinism and registry continuity for mining/haul entities.
- Tasks: validate rewind determinism for resource counts/state; ensure the time spine snapshot/command log drives resim correctly; add PlayMode tests for rewind determinism; validate registry continuity for mining/haul entities.
- Validation: extend `Docs/TODO/Phase2_Demo_TODO.md` and PlayMode tests under `Assets/Scripts/Godgame/Tests` to lock determinism and continuity; run headless test commands listed in `AGENTS.md`.

## Agent 2 – Phase 3 Agent A: Alignment/Compliance
- Goal: bring the alignment/compliance system online for crew/fleet/colony entities.
- Tasks: wire alignment data to crew/fleet/colony entities; implement `CrewAggregationSystem`; create `DoctrineAuthoring` baker with inspector validation; build a mutiny/desertion demo scene.
- Validation: add EditMode/PlayMode tests covering aggregation and compliance triggers, and record any engine gaps in `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`.

## Agent 3 – Phase 3 Agent B: Modules/Degradation
- Goal: extend crew skills into module slot/refit/degradation paths.
- Tasks: implement module slot/refit system; build component degradation/repair flows; wire crew skills into refit/repair/combat paths.
- Validation: add tests for refit/degradation loops, ensure registries/telemetry capture module state, and log missing assets/APIs in the TODO docs.
