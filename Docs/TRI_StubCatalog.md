# TRI Stub Catalog

Quick reference for ahead-of-time Godgame stubs.

- **File**: `Assets/Scripts/Godgame/Stubs/GodgameConceptStubComponents.cs`
  - **Module**: Miracles, villager needs, cohesion/boundaries, construction/alerts, grudges/languages, hand input, villager behavior/perception, navigation tickets, economy/crafting hooks, sensors/time/narrative, telemetry/save references, aggregates (bands/guilds), interception.
  - **Types**: `MiracleRequest`, `VillagerNeed`, `NeedSatisfaction`, `BandCohesionState`, `VillageBoundary`, `ConstructionPhase`, `AlertState`, `GrudgeEntry`, `LanguageAffinity`, `HandInputIntent`, `VillagerBehaviorTreeRef`, `VillagerBehaviorState`, `VillagerBehaviorNodeState`, `VillagerPerceptionConfig`, `VillagerPerceptionStimulus`, `VillagerInitiativeState`, `VillagerNeedElement`, `PathIntent`, `DestinationWaypoint`, `NavigationTicketHandle`, `VillagerSensorRig`, `VillagerInterruptTicket`, `GodgameTimeCommand`, `GodgameSituationAnchor`, `TelemetryStreamHook`, `SaveSlotRequest`, `WorkshopRecipeHandle`, `StorehouseInventoryTag`, `VillagerCraftJob`, `MiracleFuelStockpile`, `VillageEconomySnapshot`, `CraftingQueueEntry`, `CraftQualityState`, `BandHandle`, `BandMembershipElement`, `GuildHandle`, `InterceptState`.
  - **Intent**: Allow scenes to wire miracles, needs, aggregate state, AI planners, nav tickets, sensors/time/narrative hooks, telemetry streams, and economy/workshop placeholders without blocking on final gameplay.

- **File**: `Assets/Scripts/Godgame/Stubs/GodgameServiceBridges.cs`
  - **Module**: Game-side bridges for nav/combat/economy/diplomacy/telemetry/sensors/time/narrative/save/behavior/decision/ambition/interception/communication/trade.
  - **Types**: `GodgameNavigationBridgeStub`, `GodgameCombatBridgeStub`, `GodgameEconomyBridgeStub`, `GodgameDiplomacyBridgeStub`, `GodgameTelemetryBridgeStub`, `GodgameSensorBridgeStub`, `GodgameTimeBridgeStub`, `GodgameNarrativeBridgeStub`, `GodgameSaveLoadBridgeStub`, `GodgameBehaviorBridgeStub`, `GodgameDecisionBridgeStub`, `GodgameAmbitionBridgeStub`, `GodgameInterceptBridgeStub`, `GodgameCommunicationBridgeStub`, `GodgameTradeBridgeStub`.
  - **Intent**: Provide deterministic interfaces for Godgame code to call PureDOTS stubs until real services land.
  - **Owner**: Godgame gameplay.
