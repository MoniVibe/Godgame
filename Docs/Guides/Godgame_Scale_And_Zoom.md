# Godgame Scale and Zoom (Game Appendix)

This appendix applies the shared framework in `puredots/Docs/Concepts/Core/Scale_And_Zoom_Framework.md` to Godgame.

## Scale intent
- Villagers feel small next to buildings and major world structures.
- Settlements read as coherent clusters at distance while preserving micro detail up close.
- Presentation remains continuous with no scene splits.
- Micro-scale action (villager rituals, duels) must remain coherent within large-scale events.

## Baseline sizes (current targets)
- Individual: 0.5m - 5m

## Target ratios (order-of-magnitude)
- Villager : House = 1 : 4-8
- Villager : Large building (storehouse/temple) = 1 : 10-20
- Villager : Landmark (world tree / major shrine) = 1 : 50-200+

## Proxy rules
- Settlements can aggregate to a single glyph at far zoom, but the glyph must stay anchored to the same world position.
- Landmarks (world tree, divine structures) scale with population or significance and remain visible from far zoom.
- Individual entities keep absolute placement so micro-scale scenes are inspectable at any zoom.

## Related concepts
- `godgame/Docs/Concepts/Core/World_Tree_Visualization_System.md` (population-driven scale).
- `godgame/Docs/Concepts/Core/Immaterium_Galactic_Visualization_System.md` (macro zoom transitions).

## Current tuning touchpoints
- `godgame/Assets/Scripts/Godgame/Scenario/SettlementSpawnSystem.cs` (villager/building/node scale constants).
- `godgame/Assets/Rendering/GodgameRenderCatalog.asset` (mesh bounds that must match scale).

## Notes
- Keep cull distances generous; use LOD rather than hard cutoffs.
- Any origin shifting must be presentation-only and visually seamless.
