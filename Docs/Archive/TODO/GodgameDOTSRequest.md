# Godgame DOTS Framework Request (Index)

Use this file as the jump-off for active DOTS tasks. Detailed scopes live in per-agent TODOs.

## Request Documents (for PureDOTS team)

- **CRITICAL** Burst BC1016 Fixes: `Docs/TODO/PureDOTS_Burst_BC1016_Fix_Request.md`
- Focus System Migration: `Docs/TODO/PureDOTS_FocusSystem_Request.md`
- Stats Foundation: `Docs/PureDOTS_Stats_Foundation_Request.md`

## Integration Tracking

- Integration backlog and bridge status: `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md`
- Phase 2 rewind demo slices: `Docs/TODO/Phase2_Demo_TODO.md`
- Divine Hand/pile/storehouse context: `Docs/TODO/DOTSRequest.md`

## Open integration items
- Close Phase 2 spine validation: align `TimeState`/`RewindState` with the PureDOTS time engine and prove rewind determinism for villager/haul/resource loops.
- Ensure registry sync systems (villager/storehouse/band/miracle/spawner/logistics) operate on authored scene data, including spatial continuity snapshots and residency versions.
- Finish aggregate pile + storehouse API integration with the Divine Hand loop; propagate registry/telemetry updates for siphon/dump/throw and conservation tests.
- Confirm Burst compliance across bridge/sync/telemetry systems once spatial/time hooks are live; remove hybrid fallbacks and managed allocations.
- Audit scenes/SubScenes/prefabs (wizard outputs included) for bakers/MonoBehaviours feeding registries and spatial tags; strip service locators in gameplay scripts.
- Align resource/miracle catalogs and other ScriptableObject definitions with shared registry IDs/catalog blobs from PureDOTS.
- Strengthen integration tests (PlayMode + EditMode) for registry registration, telemetry emission, time/rewind continuity, and hand/pile/storehouse flows; provide mocks for registry consumers.

## Validation commands
- Edit-mode registry bridge suite: `Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform editmode -testResults Logs/editmode-tests.xml -testFilter GodgameRegistryBridgeSystemTests`
- Full test sweep: `Unity -projectPath "$(pwd)" -batchmode -quit -executeMethod UnityEditor.TestTools.TestRunner.CommandLineTest.RunAllTests`
