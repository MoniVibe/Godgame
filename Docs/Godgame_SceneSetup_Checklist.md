# Godgame Scene Setup Checklist

This document lists the MonoBehaviours and GameObjects that should be present in the Godgame scene, based on what works in Space4X.

**Last Updated**: Based on `Space4X_SceneSetup_Brief.md` analysis

## Key Finding: Godgame Already Has Correct Pattern!

✅ **Godgame's input flow matches Space4X**:
- `GodgameInputReader` (MonoBehaviour) reads Unity Input System → writes to ECS `CameraInput` singleton
- `GodgameCameraController` (MonoBehaviour) reads ECS `CameraInput` → applies to camera transform
- This matches Space4X's pattern (just different component names)

❌ **Space4X does NOT use `BW2CameraInputBridge`** - so Godgame doesn't need it either!

## Bootstrap GameObject

The bootstrap GameObject (often named `PureDOTS_Bootstrap`, `DemoBootstrap`, or `EntitiesBootstrap`) should have these components:

### Required PureDOTS Components

1. **`PureDOTS.Authoring.PureDotsConfigAuthoring`** (may be in SubScene)
   - Provides runtime configuration (time, history, pooling, threading)
   - Location: `Packages/com.moni.puredots/Runtime/Authoring/PureDotsConfigAuthoring.cs`
   - Requires: `PureDotsRuntimeConfig` ScriptableObject asset assigned
   - **Note**: In Space4X, this is often in a SubScene, not main scene

2. **`PureDOTS.Authoring.Hybrid.HybridControlToggleAuthoring`** (optional)
   - Allows cycling between input modes (F9 key)
   - Location: `Packages/com.moni.puredots/Runtime/Authoring/Hybrid/HybridControlToggleAuthoring.cs`
   - Default mode: `Dual` (both Space4X and Godgame input active)
   - **Note**: Not found in Space4X Demo_Space4X_01 scene

3. **`PureDOTS.Runtime.Camera.BW2CameraInputBridge`** ❌ **NOT NEEDED**
   - **Space4X does NOT use this component**
   - Godgame uses `GodgameInputReader` instead (correct pattern)
   - **Do NOT add this** - it's not the cause of WASD issues

### Godgame-Specific Components

4. **`Godgame.Demo.DemoBootstrap`**
   - Godgame demo initialization
   - Location: `Assets/Scripts/Godgame/Demo/DemoBootstrap.cs`
   - Handles scenario loading, time controls, etc.

5. **`Godgame.Demo.DemoEnsureSRP`** (optional, editor-only)
   - Ensures URP is active
   - Location: `Assets/Scripts/Godgame/Demo/DemoEnsureSRP.cs`
   - Execution order: `-10000`

### Input Bridge Components

6. **`Godgame.Input.GodgameInputReader`**
   - Bridges New Input System → ECS input components
   - Location: `Assets/Scripts/Godgame/Input/GodgameInputReader.cs`
   - Reads WASD, mouse, etc. and writes to `CameraInput`, `MiracleInput`, etc.

## Camera GameObject

The main camera GameObject should have:

### Camera Component
- Standard Unity `Camera` component
- Should be tagged as `MainCamera` (or use `Camera.main`)

### Camera Controller Scripts

1. **`Godgame.GodgameCameraController`**
   - Godgame-specific camera controller (RTS/Free-fly mode)
   - Location: `Assets/Scripts/Godgame/Camera/GodgameCameraController.cs`
   - Reads from ECS `CameraInput` component
   - Publishes to `CameraRigService` for DOTS systems

2. **`PureDOTS.Runtime.Camera.BW2StyleCameraController`** (optional)
   - PureDOTS BW2-style camera (if using PureDOTS camera system)
   - Location: `Packages/com.moni.puredots/Runtime/Camera/BW2StyleCameraController.cs`
   - Note: `GodgameCameraController` checks if this is active and defers to it

### Camera Input Router (if using PureDOTS hand system)

3. **`PureDOTS.Input.HandCameraInputRouter`** (optional)
   - Routes camera input for divine hand interactions
   - Location: `Packages/com.moni.puredots/Runtime/Input/HandCameraInputRouter.cs`
   - Required if using `DivineHandInputBridge`

## Entities Graphics Setup

Entities Graphics should be automatically set up by PureDOTS systems, but verify:

1. **RenderMeshArraySingleton entity exists**
   - Created by `PureDOTS.Demo.Rendering.SharedRenderBootstrap`
   - Populated by `Godgame.Demo.GodgameDemoRenderSetupSystem`
   - Check: Look for entity with `RenderMeshArraySingleton` shared component

2. **WorldRenderSettings** (if needed)
   - Unity Entities Graphics requires a camera reference
   - Usually handled automatically, but verify camera is assigned correctly

## Input System Setup

### New Input System
- Ensure `EventSystem` GameObject exists in scene
- Should have `InputSystemUIInputModule` component (not `StandaloneInputModule`)
- `DemoBootstrap` sets this up automatically in `EnsureInputSystemUI()`

### Input Action Asset (optional)
- Can assign custom `InputActionAsset` to `GodgameInputReader`
- If null, uses programmatic default bindings (WASD, mouse, etc.)

## Comparison Steps

### Step 1: Compare Bootstrap GameObject

1. Open Space4X scene
2. Find bootstrap GameObject (check Hierarchy for names like `PureDOTS_Bootstrap`, `DemoBootstrap`, etc.)
3. In Inspector, note all MonoBehaviours on it
4. Open Godgame scene
5. Find equivalent bootstrap GameObject
6. Add missing MonoBehaviours:
   - If `BW2CameraInputBridge` is missing → **This is why WASD doesn't work!**
   - If `PureDotsConfigAuthoring` is missing → Add it and assign a config asset
   - If `HybridControlToggleAuthoring` is missing → Add it (optional but useful)

### Step 2: Compare Camera Setup

1. In Space4X, select main camera
2. Note all MonoBehaviours on it
3. In Godgame, select main camera
4. Ensure `GodgameCameraController` is present
5. If Space4X has `BW2StyleCameraController`, consider adding it (or ensure `GodgameCameraController` handles input correctly)

### Step 3: Check for Missing Scripts Warning

If you see:
```
The referenced script (Unknown) on this Behaviour is missing!
```

This means:
- A GameObject in the scene references a MonoBehaviour that no longer exists
- The script was renamed, moved, or deleted
- **Action**: Select the GameObject, remove the missing script component, and add the correct one

### Step 4: Verify Input Bridge

1. Ensure `GodgameInputReader` is on a GameObject in the scene (can be on bootstrap or separate)
2. Ensure `BW2CameraInputBridge` is on bootstrap GameObject
3. Check that `EventSystem` exists and has `InputSystemUIInputModule`

## Common Issues

### WASD Not Working

**Cause**: `GodgameInputReader` not writing to ECS, or `GodgameCameraController` not reading correctly

**Fix** (based on Space4X pattern):
1. ✅ **Verify `GodgameInputReader` exists** in scene (can be on bootstrap or separate GameObject)
2. ✅ **Verify `GodgameInputReader` is enabled** (check Inspector)
3. ✅ **Verify `GodgameInputReader` initializes ECS** (check logs for `[GodgameInput]` messages)
4. ✅ **Verify `CameraInput` singleton exists** (check Entity Hierarchy for entity with `CameraInput` component)
5. ✅ **Verify `GodgameCameraController` is on camera** and enabled
6. ✅ **Check input actions are set up** (WASD should be bound to "Move" action in Camera action map)
7. ❌ **Do NOT add `BW2CameraInputBridge`** - Space4X doesn't use it, Godgame doesn't need it

**Debug Steps**:
- Enable `logInputEachFrame` on `GodgameInputReader` to see if input is being read
- Check console for `[GodgameInput] Camera: Move=...` messages when pressing WASD
- Check Entity Hierarchy for `GodgameInputSingleton` entity and verify it has `CameraInput` component

### Only Sanity Cube Visible

**Cause**: Entities Graphics not connected to camera, or entities missing `MaterialMeshInfo`

**Fix**:
1. Verify `RenderMeshArraySingleton` exists (check logs from `GodgameDemoRenderSetupSystem`)
2. Verify villagers have `MaterialMeshInfo` component (use `DebugEntitySystem` logs)
3. Check camera culling mask includes entities layer
4. Verify camera is assigned to Entities Graphics world (usually automatic)

### Orbit Cubes Not Visible

**Cause**: Orbit cubes created by PureDOTS systems may not have `MaterialMeshInfo`

**Fix**:
- Orbit cubes are created by PureDOTS demo systems
- They should automatically get render components if PureDOTS rendering is set up correctly
- Check `GodgameOrbitDebugSystem` logs to see if they have `MaterialMeshInfo`

## Quick Verification

After setting up the scene, verify:

1. **Bootstrap GameObject has**:
   - ✅ `Godgame.Demo.DemoBootstrap` (existing)
   - ❓ `PureDotsConfigAuthoring` (if not in SubScene)
   - ❌ **NOT NEEDED**: `BW2CameraInputBridge` (Space4X doesn't use it)

2. **Input Bridge GameObject** (can be same as bootstrap or separate):
   - ✅ `Godgame.Input.GodgameInputReader` (existing)
   - ✅ Enabled and initialized (check logs)

3. **Camera GameObject has**:
   - ✅ `Camera` component
   - ✅ `Godgame.GodgameCameraController` (existing)
   - ❓ `PureDOTS.Runtime.Camera.CameraRigApplier` (optional, if using CameraRigState)

4. **Scene has**:
   - ✅ `EventSystem` with `InputSystemUIInputModule`
   - ✅ No "Unknown script" warnings

5. **In Play Mode, check logs**:
   - ✅ `[GodgameDemoRenderSetupSystem] Populated RenderMeshArray...`
   - ✅ `[VillagerDebug] Total Villagers: 90, With MaterialMeshInfo: 90`
   - ✅ `[GodgameInput] Camera: Move=...` when pressing WASD (if `logInputEachFrame` enabled)
   - ✅ No errors about missing `CameraInput` singleton

## Next Steps

If villagers still don't render after fixing scene setup:

1. Check `DebugEntitySystem` logs for MaterialMeshInfo counts
2. Verify RenderMeshArray has meshes/materials
3. Check camera position/rotation (should be looking at village area)
4. Try spawning debug cubes using same pattern as sanity cube
5. Check Entities Graphics renderer is active in URP settings

