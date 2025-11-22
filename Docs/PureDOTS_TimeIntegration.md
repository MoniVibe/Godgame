PureDOTS Time Integration (Godgame)
===================================

The Godgame project must use the shared PureDOTS time/rewind pipeline. Do not add or fork time systems here.

- Source of truth lives in `PureDOTS/Packages/com.moni.puredots` (TickTimeState, RewindState, TimeControlCommand, logs, CoreSingletonBootstrapSystem, PureDotsWorldBootstrap).
- Scenarios/headless runs: call `-batchmode -nographics -executeMethod PureDOTS.Runtime.Devtools.ScenarioRunnerEntryPoints.RunScenarioFromArgs --scenario <path> [--report <path>]`. Scenario JSON samples live under `PureDOTS/Packages/com.moni.puredots/Runtime/Runtime/Scenarios/Samples`.
- Use the shared ScenarioRunner executor; wire Godgame spawn/bootstrap to scenarios without introducing a custom time pipeline.
- HUD/debug: reuse `DebugDisplayReader` and `RewindTimelineDebug` bound to the shared `DebugDisplayData` singleton; avoid duplicate HUDs.

Reminder: if you open the PureDOTS repo, the `projects/` folder there is not the Godgame worktree. The real Godgame repo is this directory (`C:\Users\shonh\OneDrive\Documents\claudeprojects\Unity\Godgame`).
