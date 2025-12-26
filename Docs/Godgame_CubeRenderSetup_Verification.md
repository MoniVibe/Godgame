# Godgame Cube Render Setup Verification

## Summary

✅ **Fixed**: `GodgameDemoSpawnSystem` was disabled (`[DisableAutoCreation]`) - now enabled
✅ **Fixed**: Created `GodgameOrbitCubeRenderFixSystem` to add MaterialMeshInfo/RenderBounds to orbit cubes

## Cube Spawning Systems

### 1. GodgameDemoSpawnSystem ✅ CORRECT

**File**: `Assets/Scripts/Godgame/legacy/GodgameDemoSpawnSystem.cs`

**Status**: ✅ **Now Enabled** (removed `[DisableAutoCreation]`)

**Setup**:
- Uses `DemoRenderUtil.MakeRenderable()` which adds:
  - ✅ `MaterialMeshInfo` (mesh index 0, material index 0)
  - ✅ `RenderBounds` (cube bounds: 0.5f extents)
- Creates 5x5 grid of cubes
- **These cubes should now render correctly**

### 2. Orbit Cubes (PureDOTS) ⚠️ FIXED

**Issue**: Orbit cubes spawned by PureDOTS systems may not have `MaterialMeshInfo`/`RenderBounds`

**Fix**: Created `GodgameOrbitCubeRenderFixSystem.cs`
- Runs once in `InitializationSystemGroup` after `SharedRenderBootstrap`
- Finds all orbit cubes missing `MaterialMeshInfo` or `RenderBounds`
- Adds missing components using same pattern as villagers:
  - `MaterialMeshInfo` with mesh index 3 (cube), material index 0
  - `RenderBounds` with cube-sized bounds

**Status**: ✅ **System created, will fix orbit cubes automatically**

### 3. TestEntitySpawnSystem ✅ CORRECT

**File**: `Assets/Scripts/Godgame/legacy/TestEntitySpawnSystem.cs`

**Setup**:
- Uses `DemoRenderUtil.MakeRenderable()` ✅
- Creates single red test cube at (0, 1, 0)
- **Should render correctly**

## Comparison: Cubes vs Villagers

### Villagers (from `Godgame_Demo01_BootstrapSystem`)

```csharp
// MaterialMeshInfo
ecb.AddComponent(villagerEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(
    DemoMeshIndices.DemoMaterialIndex,        // 0
    DemoMeshIndices.VillageVillagerMeshIndex   // 3
));

// RenderBounds
ecb.AddComponent(villagerEntity, new RenderBounds
{
    Value = new AABB
    {
        Center = float3.zero,
        Extents = new float3(0.5f, 1f, 0.5f) // person-sized
    }
});
```

### Cubes (from `DemoRenderUtil.MakeRenderable`)

```csharp
// MaterialMeshInfo
var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(mat: 0, mesh: 0);

// RenderBounds
new RenderBounds
{
    Value = new AABB
    {
        Center = float3.zero,
        Extents = new float3(0.5f, 0.5f, 0.5f) // cube-sized
    }
}
```

**Difference**: 
- Villagers use mesh index **3** (VillageVillagerMeshIndex)
- Cubes use mesh index **0** (default cube mesh)

Both use material index **0** (DemoMaterialIndex) ✅

## Verification Steps

### In Play Mode:

1. **Check `GodgameDemoSpawnSystem` logs**:
   - Should see `[DemoRenderUtil] Made entity X renderable...` messages
   - Should create 25 cubes (5x5 grid)

2. **Check `GodgameOrbitCubeRenderFixSystem` logs**:
   - Should see `[GodgameOrbitCubeRenderFix] Added MaterialMeshInfo/RenderBounds to X orbit cubes`
   - If 0, all orbit cubes already had components (good!)

3. **Check Entity Hierarchy**:
   - Select a cube entity (from GodgameDemoSpawnSystem or orbit cubes)
   - Verify components:
     - ✅ `LocalTransform`
     - ✅ `MaterialMeshInfo` (should show mesh/material indices)
     - ✅ `RenderBounds` (should show AABB)

4. **Check `GodgameOrbitDebugSystem` logs**:
   - Should show orbit cubes with `MMI=...` (MaterialMeshInfo present)
   - Should NOT show "NO MaterialMeshInfo" messages

5. **Visual Check**:
   - Cubes should be visible in Scene/Game view
   - Should match villagers' rendering (same shader, lighting, etc.)

## Mesh Index Reference

**Important**: In `GodgameDemoRenderSetupSystem`, ALL meshes are cubes (line 89: `meshes[i] = cubeMesh`), so any mesh index (0-3) will work. The difference is only in material colors:

From `GodgameDemoRenderSetupSystem`:
- Mesh index 0 = Cube mesh, Material index 0 = **Magenta** material
- Mesh index 1 = Cube mesh, Material index 1 = **Red** material
- Mesh index 2 = Cube mesh, Material index 2 = **Green** material
- Mesh index 3 = Cube mesh, Material index 3 = **Blue** material

From `DemoMeshIndices` (PureDOTS convention):
- `VillageGroundMeshIndex = 0` (ground/terrain)
- `VillageHomeMeshIndex = 1` (home structure)
- `VillageWorkMeshIndex = 2` (workplace structure)
- `VillageVillagerMeshIndex = 3` (villager/cube mesh)

**Current Setup**:
- `GodgameDemoSpawnSystem` cubes: mesh 0, material 0 (magenta cubes) ✅
- Villagers: mesh 3, material 0 (blue cubes) ✅
- Orbit cubes (fixed): mesh 3, material 0 (blue cubes) ✅

**Note**: Since all meshes are cubes, any mesh index works. The material index determines the color.

## Troubleshooting

### Cubes Still Not Visible

1. **Check RenderMeshArray is populated**:
   - Look for `[GodgameDemoRenderSetupSystem] Populated RenderMeshArray...` log
   - Verify mesh/material counts match expectations

2. **Check cube entities have components**:
   - Use Entity Hierarchy to inspect cube entities
   - Verify `MaterialMeshInfo` and `RenderBounds` exist

3. **Check camera**:
   - Verify camera Culling Mask includes Default layer
   - Verify camera is looking at cube positions
   - Check camera near/far clip planes

4. **Check mesh indices**:
   - Verify `MaterialMeshInfo` mesh index matches actual mesh in RenderMeshArray
   - If using index 0 but cube mesh is at index 3, cubes won't render

5. **Check system execution order**:
   - `GodgameDemoSpawnSystem` runs after `SharedRenderBootstrap` ✅
   - `GodgameOrbitCubeRenderFixSystem` runs after `SharedRenderBootstrap` ✅
   - Both should run before presentation systems

## Files Modified

1. ✅ `Assets/Scripts/Godgame/legacy/GodgameDemoSpawnSystem.cs`
   - Removed `[DisableAutoCreation]` - system now runs automatically
   - Added `[UpdateAfter(typeof(SharedRenderBootstrap))]` for correct ordering

2. ✅ `Assets/Scripts/Godgame/legacy/GodgameOrbitCubeRenderFixSystem.cs` (NEW)
   - Adds MaterialMeshInfo/RenderBounds to orbit cubes missing them
   - Runs once in InitializationSystemGroup

## Expected Result

After these fixes:
- ✅ `GodgameDemoSpawnSystem` cubes should render (5x5 grid)
- ✅ Orbit cubes should render (if PureDOTS creates them)
- ✅ All cubes should have same render setup as villagers
- ✅ Cubes should be visible alongside villagers in Scene/Game view

