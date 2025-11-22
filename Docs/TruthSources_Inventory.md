# Truth Sources – Inventory

Snapshot of the active “truth” contracts the project leans on. Update as new systems stabilize.

- **Registries (PureDOTS)**: Villager, Resource, Storehouse, Miracle, Band, Spawner, LogisticsRequest, Construction (`Packages/com.moni.puredots/Runtime/Registry/*`). Godgame mirrors live under `Assets/Scripts/Godgame/Registry`.
- **Time spine**: `TimeState`, `RewindState`, snapshot/command log stack (`Packages/com.moni.puredots/Runtime/Time/*`); Godgame demo in `Assets/Scripts/Godgame/Time/`.
- **Spatial grid**: `SpatialGridState`, `SpatialGridResidency`, `SpatialIndexedTag` with profiles authored via `SpatialPartitionAuthoring`.
- **Telemetry**: `TelemetryStream` + `TelemetryMetric` (`Packages/com.moni.puredots/Runtime/Telemetry`), emitted by Godgame bridge/systems.
- **Interaction/Hand**: `InputState`, hand truth source contracts (`Assets/Scripts/Godgame/Interaction`), backed by shared input bridge components.
- **Mining/Hauling**: Resource nodes, carrier pickup, mining loop feeding registries/telemetry (runtime in `Assets/Scripts/Godgame/Mining`).

For data flow and per-system responsibilities, see `Docs/TruthSources_Architecture.md` and `Docs/TruthSources_QuickReference.md`.
