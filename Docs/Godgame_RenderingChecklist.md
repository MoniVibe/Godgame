# Godgame Rendering Checklist (Demo_01)

Lightweight checklist for ensuring ECS entities render correctly with URP + Entities Graphics.

## Required Components for Renderable Entities

All renderable entities must have:

1. **LocalTransform** - Position, rotation, scale
2. **MaterialMeshInfo** - References mesh/material indices in shared RenderMeshArray
3. **RenderBounds** - AABB bounds (relative to LocalTransform)
4. **WorldRenderBounds** - Automatically calculated by Entities Graphics from RenderBounds
5. **Shared RenderMeshArray** - Via RenderMeshArraySingleton singleton entity

## Entity-Specific Setup

### Villagers

**Components:**
- `LocalTransform`
- `MaterialMeshInfo` (mesh: `DemoMeshIndices.VillageVillagerMeshIndex`, mat: `DemoMeshIndices.DemoMaterialIndex`)
- `RenderBounds` (AABB: Center=0, Extents=(0.5, 1.0, 0.5) - person-sized)
- `VillagerPresentationTag`
- `PresentationLODState`
- `VillagerVisualState`

**Where Added:**
- `Godgame_Demo01_BootstrapSystem.SpawnVillagerViaECB()` - via ECB

### Village Centers

**Components:**
- `LocalTransform`
- `MaterialMeshInfo` (mesh: `DemoMeshIndices.VillageHomeMeshIndex`, mat: `DemoMeshIndices.DemoMaterialIndex`)
- `RenderBounds` (AABB: Center=0, Extents=(2.0, 2.0, 2.0) - structure-sized)
- `VillageCenterPresentationTag`
- `PresentationLODState`
- `VillageCenterVisualState`

**Where Added:**
- `Godgame_Demo01_BootstrapSystem.SpawnDemoVillages()` - via ECB

### Resource Nodes

**Components:**
- `LocalTransform`
- `MaterialMeshInfo` (mesh: `DemoMeshIndices.VillageVillagerMeshIndex`, mat: `DemoMeshIndices.DemoMaterialIndex`)
- `RenderBounds` (AABB: Center=0, Extents=(0.5, 0.5, 0.5) - cube-sized)
- `ResourceNodePresentationTag`
- `PresentationLODState`

**Where Added:**
- `Godgame_Demo01_BootstrapSystem.SpawnDemoResourceNodes()` - via EntityManager

### Resource Chunks

**Components:**
- `LocalTransform`
- `MaterialMeshInfo` (mesh: `DemoMeshIndices.VillageVillagerMeshIndex`, mat: `DemoMeshIndices.DemoMaterialIndex`)
- `RenderBounds` (AABB: Center=0, Extents=(0.25, 0.25, 0.25) - small chunk)
- `ResourceChunkPresentationTag`
- `PresentationLODState`
- `ResourceChunkVisualState`

**Where Added:**
- `Godgame_Demo01_BootstrapSystem.SpawnDemoResourceNodes()` - via EntityManager

## Render Setup Systems

### GodgameDemoRenderSetupSystem

**Purpose:** Populates the shared RenderMeshArray with meshes and materials.

**Dependencies:**
- Runs after `PureDOTS.Demo.Rendering.SharedRenderBootstrap`
- Requires `RenderMeshArraySingleton` entity to exist

**What It Does:**
1. Finds the `RenderMeshArraySingleton` entity created by PureDOTS
2. Creates 4 materials (magenta, red, green, blue) using URP Simple Lit shader
3. Creates 4 cube meshes (all using built-in Cube.fbx)
4. Populates the RenderMeshArray at indices matching `DemoMeshIndices`:
   - Index 0: VillageGroundMeshIndex (magenta)
   - Index 1: VillageHomeMeshIndex (red)
   - Index 2: VillageWorkMeshIndex (green)
   - Index 3: VillageVillagerMeshIndex (blue)

**Order of Operations:**
1. `PureDOTS.Demo.Rendering.SharedRenderBootstrap` - Creates empty RenderMeshArraySingleton
2. `GodgameDemoRenderSetupSystem` - Populates RenderMeshArray with meshes/materials
3. `Godgame_Demo01_BootstrapSystem` - Spawns entities with MaterialMeshInfo referencing the array
4. Presentation systems - Update LOD, visual state, etc.

## MaterialMeshInfo Index Resolution

Entities reference the shared RenderMeshArray via `MaterialMeshInfo.FromRenderMeshArrayIndices(materialIndex, meshIndex)`:

- **Material Index:** Always `DemoMeshIndices.DemoMaterialIndex` (0) for Demo_01
- **Mesh Indices:**
  - Villagers: `DemoMeshIndices.VillageVillagerMeshIndex` (3)
  - Village Centers: `DemoMeshIndices.VillageHomeMeshIndex` (1)
  - Resource Nodes/Chunks: `DemoMeshIndices.VillageVillagerMeshIndex` (3)

## Debugging

### Verify Entities Exist
- `DebugEntitySystem` logs all entities with `LocalTransform`
- Filters villagers by `VillagerPresentationTag` and logs positions

### Verify Render Components
- Check that entities have `MaterialMeshInfo` component
- Check that entities have `RenderBounds` component
- Verify `RenderMeshArraySingleton` entity exists and is populated

### If Entities Don't Render
1. Confirm GameObject cube renders (proves camera + URP work)
2. Check that `RenderMeshArraySingleton` is populated (no "No RenderMeshArray entity found" warning)
3. Verify entities have all required components (use Entity Inspector or debug logging)
4. Check camera settings (URP renderer assigned, UniversalAdditionalCameraData present)
5. If still not visible, add `GodgameVillagerDebugCubeSystem` to spawn debug cubes at villager positions

## Notes

- **WorldRenderBounds:** Automatically calculated by Entities Graphics from RenderBounds + LocalTransform
- **LOD:** Controlled via `PresentationLODState.ShouldRender` flag
- **Camera:** Must use URP renderer and have `UniversalAdditionalCameraData` component
- **No Entities Renderer Feature needed:** Entities Graphics integrates automatically with URP 1.4+

