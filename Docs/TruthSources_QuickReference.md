# Truth Sources – Quick Reference

- **Registries** (PureDOTS): `VillagerRegistryEntry`, `ResourceRegistryEntry`, `StorehouseRegistryEntry`, `MiracleRegistryEntry`, `BandRegistryEntry`, `SpawnerRegistryEntry`, `LogisticsRequestRegistryEntry`, `ConstructionRegistryEntry` (`Packages/com.moni.puredots/Runtime/Registry`). Sync via `Assets/Scripts/Godgame/Registry/*SyncSystem.cs`; orchestrated by `GodgameRegistryBridgeSystem`.
- **Time/Rewind**: `TimeState`, `RewindState`, snapshot/command log (`Packages/com.moni.puredots/Runtime/Time`). Godgame demo runtime in `Assets/Scripts/Godgame/Time/`; determinism tests live under `Assets/Scripts/Godgame/Tests`.
- **Spatial**: `SpatialGridState`, `SpatialGridResidency`, `SpatialIndexedTag`; authored with `SpatialPartitionAuthoring` + `GodgameSpatialProfile.asset`.
- **Telemetry**: `TelemetryStream` + `TelemetryMetric` (`Packages/com.moni.puredots/Runtime/Telemetry`); emitted by bridge systems for registries, miracles, mining.
- **Interaction/Input**: `InputState`, hand-related truth sources in `Assets/Scripts/Godgame/Interaction`; PureDOTS input bridge seeds these buffers for DOTS systems.
- **Mining/Hauling**: resource nodes + carrier flows mirrored into registries/telemetry under `Assets/Scripts/Godgame/Mining`.

Start here when mapping a concept to implementation: choose the registry/time/spatial contract, add/adjust mirror components, extend the relevant sync system, and update tests + TODO docs.
