# Scale + LOD Notes (Godgame)

Goal: seamless planet-scale presentation with biomes, flora/fauna, and villages all in one runtime layer.

## Principles
- LOD is presentation-only. Sim remains fixed-step and deterministic.
- Distant terrain/biomes remain real in sim, only visibility and impostors change.

## Layer mapping (default intent)
- Colony: villagers, village centers, resource chunks
- Island: resource nodes (can shift to Continent if needed)
- Continent: vegetation/biome-level presentation
- Planet: any other entities that should follow planet-scale thresholds

## Planet surface approach
- Chunk planet surfaces; per-chunk LOD based on camera distance and layer multipliers.
- Chunk updates are idempotent and cacheable (seed + chunk id + LOD).

## Presentation hooks
- PresentationLayerConfig authoring drives per-layer distance multipliers.
- PresentationLODState updates from camera distance; RenderKey.LOD follows when present.

## Notes on continuity
- Avoid scene/layer separations for presentation; treat the planet as one continuous space.
- Distant mountains/biomes should collapse to impostors/icons without breaking sim continuity.
