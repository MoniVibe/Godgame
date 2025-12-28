# Godgame Render Catalog Truth Source

This is the single source of truth for Godgame render catalog wiring.

## Canonical Types (Truth)

All catalog authoring and runtime types live in PureDOTS:

- `puredots/Packages/com.moni.puredots/Runtime/Rendering/RenderPresentationCatalogDefinition.cs`
  - `RenderPresentationCatalogDefinition` (ScriptableObject with `Variants` + `Themes`)
- `puredots/Packages/com.moni.puredots/Runtime/Rendering/RenderPresentationCatalogAuthoring.cs`
  - `RenderPresentationCatalogAuthoring` (MonoBehaviour for baking)
- `puredots/Packages/com.moni.puredots/Runtime/Authoring/Rendering/RenderPresentationCatalogBaker.cs`
  - `RenderPresentationCatalogBaker` (creates `RenderPresentationCatalog` + `RenderMeshArray`)
- `puredots/Packages/com.moni.puredots/Runtime/Rendering/RenderPresentationContract.cs`
  - `RenderPresentationCatalog` + `RenderPresentationCatalogBlob`

## Godgame Catalog Asset

- `godgame/Assets/Rendering/GodgameRenderCatalog.asset` (canonical)
- `godgame/Assets/Scripts/Godgame/Rendering/Catalog/GodgameRenderCatalogDefinition.cs` (asset menu type)

## Scene Wiring

- `godgame/Assets/Scenes/TRI_Godgame_Smoke.unity`
  - `Godgame.Rendering.Catalog.RenderCatalogAuthoring` (baked path)
  - `PureDOTS.Rendering.RenderPresentationCatalogRuntimeBootstrap` (fallback when no baked catalog exists)

## How to Add a Render Entry (Godgame)

1. Open `godgame/Assets/Rendering/GodgameRenderCatalog.asset`.
2. Add a new item under `Variants` with Mesh/Material/Bounds/PresenterMask.
3. Update Theme 0 (and any active themes) to map the `SemanticKey` to the new variant indices (`Lod0Variant`, `Lod1Variant`, `Lod2Variant`).
4. Ensure gameplay writes `RenderSemanticKey` using `GodgameSemanticKeys` (`godgame/Assets/Scripts/Godgame/Rendering/GodgameSemanticKeys.cs`).
5. The baker/runtime bootstrap bumps `RenderCatalogVersion` automatically; no manual versioning needed.

## Assetsvault Upgrade Lane (Hero Pass v1)

Repeatable pipeline:

1. Import from Assetsvault into `godgame/Assets/Art/Assetsvault/MedievalHero/` (Meshes, Materials, Textures, Shaders).
2. Ensure materials are render-pipeline compatible (URP). If a pack ships built-in shaders, convert to URP Lit or author new URP materials.
3. Append new variants to `godgame/Assets/Rendering/GodgameRenderCatalog.asset` (never reorder existing entries).
4. Update Theme 0 mappings to the new variant indices.
5. Verify the RenderMeshArray is populated and the smoke scene logs show MaterialMeshInfo assigned.

Current hero mapping (v1):

- `Storehouse_Gate_Hero_v1` uses `Gate_1A1` + `Gates_1A1`.
- `ResourceNode_Rock_Hero_v1` uses `Cliff_Rocks_Set_A1` + `Cliff_Pieces_1`.
- Theme 0 maps `GodgameSemanticKeys.Storehouse` to `Storehouse_Gate_Hero_v1` and `GodgameSemanticKeys.ResourceNode` to `ResourceNode_Rock_Hero_v1`.

## Runtime Verification

- `RenderPresentationCatalog.Blob.IsCreated == true`
- `RenderPresentationCatalog.RenderMeshArrayEntity` has `RenderMeshArray` shared component
- Entities with `RenderSemanticKey` + enabled presenters resolve to `MaterialMeshInfo` + `RenderBounds`

## Troubleshooting

- **Catalog not found**: Ensure the catalog authoring object lives in the scene/subscene, or add the runtime bootstrap when you cannot bake.
- **Wrong mesh appears**: Verify the theme mapping for `SemanticKey` points at the expected variant index.















