# Godgame Content Intent Adapter v0

Shared taxonomy version: `../../puredots/Docs/ContentIntent/MVP_Content_Taxonomy_v0.md`  
Adapter status: scaffold-only (projection mapping to be filled as runtime settles)

## Purpose

Track how shared content intent IDs map into Godgame-specific entities/systems without changing shared meaning.

## Current Policy

1. Reuse shared intent IDs as-is.
2. Add Godgame projection entries incrementally.
3. Keep simulation meaning in PureDOTS docs, not in this adapter.

## Initial Mapping Seeds

| Shared intent ID | Godgame projection placeholder |
|---|---|
| `intent.civilian.shuttle` | village/settlement civilian transport archetype (TBD) |
| `intent.civilian.convoy_freighter` | caravan/hauler archetype (TBD) |
| `intent.colony.frontier_outpost` | remote settlement node (TBD) |
| `intent.station.trade_post` | market structure / trade hall (TBD) |
| `intent.cache.salvage_stash` | hidden stash or ruin cache entity (TBD) |
| `intent.mission.escort_civilians` | escort chain objective with civilian survival score (TBD) |
| `intent.boss.raider_warlord` | raid commander archetype (TBD) |

## TODO for First Godgame Pass

1. Confirm equivalent core roles (civilian, colony-like node, station/shop-like node, cache, boss).
2. Define deterministic success metrics per mapped mission.
3. Add placeholder legend codes for in-editor readability.

## Contract Mapping (Combat + Mining v0)

Godgame currently projects scenario data through:
- `godgame/Assets/Scripts/Godgame/Scenario/GodgameScenarioLoaderSystem.cs`
- `godgame/Assets/Scenarios/Godgame/*.json`

Canonical contract ownership for next passes:
- `../../puredots/Docs/Canonicity/Combat_Mining_DataContracts_v0.md`
- `../../puredots/Docs/Canonicity/Data_Contract_Canon_Sprint_v0.md`
- `../../puredots/Docs/Canonicity/canonical_contracts.v0.json`
- `../../puredots/Docs/Canonicity/canonical_contract_payloads.v0.json`
- `../../puredots/Docs/Canonicity/Payloads/*`
- `../../puredots/Docs/Canonicity/Schemas/contract.mining.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.combat.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.room_profile.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.scenario_envelope.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.mission_objective.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.loot_cache.v0.schema.json`
- `../../puredots/Docs/Canonicity/Schemas/contract.encounter_profile.v0.schema.json`

Rule: Godgame scenario data may remain simple for now, but shared deterministic meaning must map to the same contract IDs.
