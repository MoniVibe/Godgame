# Processed Insights → Actionables (Godgame)

## Presentation-only scale
- LOD/culling must not change villager, resource, or biome sim state.
- PresentationLayerConfig controls perceived scale; sim stays fixed-step.

## Planet + biome chunking
- Terrain/biomes must be chunked and LODed per chunk.
- Chunk updates are idempotent and cacheable (seed + chunk id + LOD).

## Distance rendering policy
- Distant villages/biomes reduce to icons/impostors via LOD variants.
- Vegetation and props must use instancing/batching at scale.
- Use shader variation for biome diversity instead of unique meshes.

## Cache + invalidation (region logic)
- Cache region/biome views (availability, travel costs, influence).
- Invalidate on building placement, resource depletion/regrowth, ownership changes.

## Travel as cost fields
- Terrain and biome volumes modify travel cost and visibility.
- Pathfinding operates on cost fields, not hard lane graphs.

## Immediate presentation hooks
- Ensure PresentationLayer tagging on all renderable entities.
- Drive PresentationLODState + RenderKey.LOD from camera distance and layer multipliers.

## BAR/Recoil-derived constraints
- Keep a hard sim/presentation boundary (no camera/UI influence on sim).
- Batch and incrementally update expensive systems (pathing/LOS/terrain).
- Use dirty-region invalidation for nav/visibility; avoid global recompute.

## Micro-world streaming (Godgame scale-ins)
- Treat interiors/regions as Sim (always-on) + Present (streamed) micro-worlds.
- Stream presentation by interest (inspect/debug/cinematic) while keeping sim active.
- Toggle room/region visibility via enableables, not structural churn.

## Biodeck/biosculpting patterns (world grids)
- Use patch-first environment grids for terrain/biomes; avoid per-plant entities.
- Apply biosculpt edits via command buffers with deterministic ordering.
- Resolve climate/biome changes only for dirty cells with hysteresis.
- Materialize hero plants only for close/inspected presentation.

## Content registry + presentation contracts
- Assets never hang off sim entities; sim stores stable IDs + small overrides only.
- Presentation resolves IDs via catalogs/registry + streaming; editor writes patches/commands only.
- No UnityEngine.Object pointers or AssetDatabase GUIDs in runtime sim state.
- Avoid SharedComponentData for skins/profiles (chunk fragmentation).
 - Use RegistryIdentity + PresentationContentRegistryAsset as the shared spine.

## Rendering and transforms
- RenderKey/RenderCatalog remain the fast path; registry binds to indices/handles.
- Prefer prefab instantiation/pooling over RenderMeshUtility.AddComponents in bulk.
- Use Parent/Child + LocalTransform; PostTransformMatrix only for non-uniform scale.

## Runtime content refs and streaming
- Use WeakObjectReference/UntypedWeakReferenceId for runtime swaps.
- Use EntitySceneReference + SceneSystem.LoadSceneAsync for streamed micro-worlds.
- Use Scene Sections to load only what’s needed; keep section 0 meta resident.

## In-game editor rules
- All edits are command streams + patches (ECB for structural changes).
- Persist patches keyed by StableId + component diffs + content IDs.
