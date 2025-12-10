# Godgame Render Catalog Setup

This directory contains the render catalog system that maps Godgame entities to Unity Entities Graphics meshes and materials.

## Overview

The render pipeline works as follows:

1. **RenderCatalogDefinition** (ScriptableObject) - Defines mesh/material mappings
2. **RenderCatalogAuthoring** (MonoBehaviour) - Bakes catalog into ECS singleton
3. **RenderKey** component - Added to entities to specify which catalog entry to use
4. **ApplyRenderCatalogSystem** - Assigns MaterialMeshInfo based on RenderKey

## Setup Instructions

### 1. Create Render Catalog Asset

1. In Unity Editor, right-click in `Assets/Data/Rendering/` (or create the folder)
2. Select **Create > Godgame > Rendering > RenderCatalog**
3. Name it `GodgameRenderCatalog.asset`

### 2. Configure Catalog Entries

1. Select the `GodgameRenderCatalog.asset` in the Inspector
2. Set **Size** to the number of entity types you want to render (e.g., 5 for villager, village center, resource chunk, resource node, storehouse)
3. For each entry, configure:
   - **Key**: Use values from `GodgameRenderKeys`:
     - `100` = Villager
     - `110` = VillageCenter
     - `120` = ResourceChunk
     - `130` = ResourceNode
     - `150` = Storehouse
   - **Mesh**: Assign a Unity Mesh (e.g., Cube, Sphere, or custom mesh)
   - **Material**: Assign a Material compatible with Entities Graphics
   - **Bounds Center**: Usually `(0, 0, 0)`
   - **Bounds Extents**: Half-size of bounding box (e.g., `(0.5, 0.5, 0.5)` for a 1x1x1 cube)

### 3. Add Catalog to Scene

1. In your Godgame demo scene, create an empty GameObject
2. Name it `GodgameRenderCatalog`
3. Add the **RenderCatalogAuthoring** component
4. Assign the `GodgameRenderCatalog.asset` to the **Catalog Definition** field
5. The baker will automatically create the ECS singleton when the scene is converted

### 4. Verify Setup

After entering Play mode or converting the scene:

- Check the Console for errors from `ApplyRenderCatalogSystem`
- If you see "No RenderKey entities found", ensure spawners are adding `RenderKey` components
- If you see "Missing RenderCatalogSingleton", ensure the catalog authoring GameObject is in the scene

## Example Catalog Entry

```
Entry 0:
  Key: 100 (Villager)
  Mesh: Cube
  Material: DefaultMaterial
  Bounds Center: (0, 0, 0)
  Bounds Extents: (0.5, 1, 0.5)  // 1x2x1 unit villager
```

## Render Keys Reference

See `Godgame/Assets/Scripts/Godgame/Rendering/GodgameRenderKeys.cs` for all available keys.

## Troubleshooting

- **No entities render**: Check that spawners add both `RenderKey` and `RenderFlags` (with `Visible=1`)
- **Catalog not found**: Ensure `RenderCatalogAuthoring` GameObject is in the scene and has the asset assigned
- **Wrong mesh appears**: Verify the `RenderKey.ArchetypeId` matches the catalog entry `Key` value







