# Truth Sources – Architecture

Reference flow for how Godgame uses PureDOTS truth sources:

1. **PureDOTS runtime contracts (truth)**: registry entities, time spine, spatial grid, telemetry singletons are created by `CoreSingletonBootstrapSystem`.
2. **Godgame mirror components**: gameplay systems write mirror data (`Assets/Scripts/Godgame/Registry/*`, `Assets/Scripts/Godgame/Time/*`, `Assets/Scripts/Godgame/Interaction/*`) to stay Burst-friendly and deterministic.
3. **Sync systems**: `Godgame*SyncSystem` structs translate mirrors into shared registry buffers (`DeterministicRegistryBuilder<T>`), normalizing indices and continuity metadata.
4. **Registry bridge**: `GodgameRegistryBridgeSystem` registers buffers with the PureDOTS `RegistryDirectory`, marks metadata, and wires telemetry.
5. **Presentation/agents**: consumers (HUD, debug, AI) read registries and telemetry; rewind/time controls drive resim via the time spine snapshot + command log.

When adding a new domain, define the contract in PureDOTS (or reuse an existing registry), add mirror components/bakers under `Assets/Scripts/Godgame/`, sync them through the bridge, and cover determinism in tests.
