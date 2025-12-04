# Building Visual Prefabs

This folder is reserved for lightweight view prefabs that wrap the Toony Tiny building FBX assets.

For now the gameplay prefabs reference the raw FBX models directly through `StaticVisualAuthoring`
(ex: Storehouse → `models/buildings/Granary.FBX`). When we want per-building overrides (materials,
LODs, colliders) drop a prefab here that simply instantiates the FBX and point the authoring
component at it instead of the importer asset.
