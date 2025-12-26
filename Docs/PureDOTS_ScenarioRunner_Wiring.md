PureDOTS ScenarioRunner Wiring - Godgame
=======================================

Objective: wire Godgame legacy spawns into the shared PureDOTS ScenarioRunner executor (headless + editor).

Next steps for agents:
1) Identify the bootstrap/spawn entry point used for the minimal legacy loop (villager/storehouse/band). Add a thin adapter (in Godgame) that, when a ScenarioRunner scenario is active, seeds entities/registries according to scenario JSON counts (see `PureDOTS/Packages/com.moni.puredots/Runtime/Runtime/Scenarios/Samples/godgame_smoke.json`).
2) Use only PureDOTS registries/spawner systems; do not add any time/rewind systems. ScenarioRunner already drives TickTimeState/RewindState via time-control commands.
3) Provide a CLI hook in Godgame that forwards `--scenario`/`--report` to the shared entry: `PureDOTS.Runtime.Devtools.ScenarioRunnerEntryPoints.RunScenarioFromArgs`.
4) Add an editor button/menu item to run the same for quick local smoke tests.
5) Document the Godgame-specific spawn mapping (registry IDs and entity archetypes) in this doc after wiring.

Reminder: the ScenarioRunner lives in the PureDOTS package; keep Godgame changes limited to spawning/adapters only.
Note: `GodgameScenarioSpawnLoggerSystem` currently logs scenario entity counts from ScenarioRunner. Replace it with real spawn/bootstrap wiring.
