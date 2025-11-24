# CI Commands Reference

## Test Execution

### EditMode Tests
Run all EditMode tests:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform editmode -testResults Logs/editmode-tests.xml
```

Run specific test filter:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform editmode -testFilter GodgameRegistryBridgeSystemTests -testResults Logs/editmode-tests.xml
```

### PlayMode Tests
Run all PlayMode tests:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform playmode -testResults Logs/playmode-tests.xml
```

Run specific test filter:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform playmode -testFilter Conservation_VillagerGatherDeliver_Playmode -testResults Logs/playmode-tests.xml
```

### All Tests
Run both EditMode and PlayMode suites:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform editmode -testResults Logs/editmode-tests.xml
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform playmode -testResults Logs/playmode-tests.xml
```

## Burst Compilation Check

Check Burst compilation status:
1. Open Unity Editor
2. Window → Entities → Burst Inspector
3. Review compilation status for all systems

Or use command-line (requires Unity with Burst package):
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Unity.Burst.Editor.BurstCompiler.CompileAll
```

## Build Validation

Build Windows player:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -buildWindows64Player Builds/Godgame.exe
```

## Scenario Execution

Run scenario headless:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.RunScenario --scenario "Assets/Scenarios/godgame_demo.json" --report "Logs/scenario_report.json"
```

Run scenario with rewind test:
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod Godgame.Scenario.GodgameScenarioEntryPoints.RunScenarioWithRewind --scenario "Assets/Scenarios/godgame_demo.json" --rewindTicks 100 --report "Logs/scenario_rewind_report.json"
```

## Test Coverage

### EditMode Test Coverage
- `Registry_AuthoringToRuntime_Mirrors` - Authoring → registry mirror validation
- `Jobs_Throughput_Telemetry_Editmode` - Telemetry counter validation
- `GodgameRegistryBridgeTests` - Registry bridge system validation

### PlayMode Test Coverage
- `Conservation_VillagerGatherDeliver_Playmode` - Resource conservation
- `Jobs_GatherDeliver_StateGraph_Playmode` - Villager job state transitions
- `Construction_GhostToBuilt_Playmode` - Construction flow
- `Time_RewindDeterminism_Playmode` - Rewind determinism
- `Presentation_Optionality_Playmode` - Presentation optionality

### Planned Test Coverage (See Gap Analysis)
- `VillagerInterruptTests` - Interrupt handling
- `VillagerNeedsTests` - Needs tracking
- `VillagerJobSchedulerTests` - Job scheduling
- `VillagerAutonomousActionTests` - Autonomous actions
- `VillageAggregationTests` - Village aggregation
- `VillageAIDecisionTests` - Village AI decisions
- `BandFormationTests` - Band formation
- `ScenarioSpawnTests` - Scenario spawning
- `ScenarioRewindDeterminismTests` - Scenario + rewind
- `DemoBootstrapTests` - Demo bootstrap

## CI Pipeline Recommendations

1. **Pre-commit**: Run EditMode tests
2. **PR validation**: Run both EditMode and PlayMode suites
3. **Nightly**: Full test suite + Burst compilation check
4. **Release**: Full test suite + build validation + Burst check

## Known Issues

- Some tests require `CoreSingletonBootstrapSystem.EnsureSingletons()` in setup
- PlayMode tests may require headless mode for CI environments
- Burst compilation check requires Unity Editor (no headless support)

