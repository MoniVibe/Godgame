# DW2-Style Scale + Presentation Notes (Godgame)

## Sim domain separation
- Region/biome-level logic should be cached and periodic.
- Individual/village behavior remains local and high-frequency.

## Cache + invalidation examples
- Resource availability maps, pathing cost fields, village influence/needs caches.
- Invalidate on: building placement, resource depletion/regrowth, ownership changes.

## Presentation performance
- Use PresentationLayerConfig + PresentationLODState to omit small meshes at distance.
- Prefer instancing/batching for vegetation, props, and FX.
- Favor procedural shader variation over asset explosion.

## Geography as cost fields
- Treat terrain/biomes as continuous cost fields affecting travel and visibility.
- Avoid hard lane-like constraints for villagers unless gameplay requires it.
