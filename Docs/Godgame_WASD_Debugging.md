# Godgame WASD Input Debugging Guide

Based on Space4X scene setup analysis, here's how to debug WASD input issues in Godgame.

## Input Flow (Godgame Pattern - Matches Space4X)

```
Unity Input System (WASD keys)
    ↓
GodgameInputReader.Update() (MonoBehaviour)
    ↓
ECS CameraInput singleton component
    ↓
GodgameCameraController.Update() (MonoBehaviour)
    ↓
Camera Transform
```

## Debug Checklist

### Step 1: Verify GodgameInputReader Exists and is Enabled

1. **Find `GodgameInputReader` in scene**:
   - Search Hierarchy for GameObject with `GodgameInputReader` component
   - Or check bootstrap GameObject

2. **Check Inspector**:
   - ✅ Component is enabled (checkbox checked)
   - ✅ No errors in Inspector (red text)
   - ✅ `Input Actions` field is either:
     - Assigned to an `InputActionAsset`, OR
     - Null (will use programmatic defaults)

3. **Check Console**:
   - ✅ No errors about `GodgameInputReader` initialization
   - ✅ Look for `[GodgameInput]` log messages (if `logInputEachFrame` enabled)

### Step 2: Verify Input Actions Are Set Up

1. **If using InputActionAsset**:
   - Open the asset in Project window
   - Verify "Camera" action map exists
   - Verify "Move" action exists in Camera map
   - Verify "Move" is bound to WASD keys

2. **If using programmatic defaults** (no asset assigned):
   - `GodgameInputReader` creates default bindings:
     - W/S → Move Up/Down
     - A/D → Move Left/Right
   - These should work automatically

### Step 3: Verify ECS CameraInput Singleton Exists

1. **In Play Mode**, open Entity Hierarchy window:
   - Window → Entities → Hierarchy

2. **Search for entity with `CameraInput` component**:
   - Should be named "GodgameInputSingleton"
   - Should have `CameraInput` component
   - Should have `GodgameInputSingleton` component

3. **If missing**:
   - `GodgameInputReader.TryInitializeECS()` may have failed
   - Check console for errors about World not found
   - Ensure `World.DefaultGameObjectInjectionWorld` exists

### Step 4: Verify GodgameCameraController Reads Input

1. **Check camera GameObject**:
   - ✅ Has `Godgame.GodgameCameraController` component
   - ✅ Component is enabled
   - ✅ No errors in Inspector

2. **Check `GodgameCameraController.Update()` logic**:
   - Reads from ECS `CameraInput` singleton (line 123)
   - Processes `cameraInput.Move` (line 143)
   - Applies movement to camera transform (line 194)

3. **Debug logging**:
   - Temporarily add `Debug.Log($"Move input: {cameraInput.Move}")` in `GodgameCameraController.Update()`
   - Should show non-zero values when pressing WASD

### Step 5: Enable Debug Logging

1. **In `GodgameInputReader` Inspector**:
   - Check `Log Input Each Frame` checkbox
   - This will log `[GodgameInput] Camera: Move=...` messages

2. **In Play Mode, press WASD**:
   - Should see log messages with non-zero Move values
   - If Move is always (0, 0), input isn't being read

3. **Check `GodgameCameraController`**:
   - Add temporary logging to see if it's reading the input
   - Check if `query.IsEmpty` is true (would prevent reading)

## Common Issues and Fixes

### Issue: "CameraInput singleton not found"

**Symptom**: `GodgameCameraController` logs "query.IsEmpty" or no movement

**Fix**:
1. Ensure `GodgameInputReader` runs before `GodgameCameraController`
2. Check that `GodgameInputReader.TryInitializeECS()` succeeded
3. Verify `World.DefaultGameObjectInjectionWorld` exists

### Issue: Input is read but camera doesn't move

**Symptom**: Logs show Move values, but camera stays still

**Fix**:
1. Check `GodgameCameraController` guard clause (line 104):
   - `BW2StyleCameraController.HasActiveRig` might be returning true
   - This would cause early return
2. Check camera transform is not locked
3. Verify `ApplyCameraPose()` is being called (line 194)

### Issue: WASD works but Q/E doesn't

**Symptom**: Horizontal movement works, vertical doesn't

**Fix**:
- Q/E is handled directly in `GodgameCameraController` (line 168-173)
- Not part of ECS `CameraInput` component
- Check keyboard is working, or add Q/E to `CameraInput` component

### Issue: Input works in one scene but not another

**Symptom**: WASD works in Space4X scene but not Godgame scene

**Fix**:
1. Compare scene setups:
   - Ensure `GodgameInputReader` exists in both
   - Ensure `EventSystem` exists with `InputSystemUIInputModule`
   - Ensure New Input System is enabled (Project Settings → Player → Active Input Handling)

## Quick Test

Add this temporary debug code to `GodgameCameraController.Update()`:

```csharp
// After line 123 (var cameraInput = query.GetSingleton<CameraInput>();)
if (math.lengthsq(cameraInput.Move) > 0.001f)
{
    Debug.Log($"[GodgameCamera] Received Move input: {cameraInput.Move}");
}
```

If you see this log when pressing WASD, input is flowing correctly. If not, check `GodgameInputReader`.

## Summary

Godgame's input pattern is **correct** and matches Space4X:
- ✅ `GodgameInputReader` reads Unity Input System → writes to ECS
- ✅ `GodgameCameraController` reads ECS → applies to camera
- ❌ **Do NOT add `BW2CameraInputBridge`** - Space4X doesn't use it

The issue is likely:
1. `GodgameInputReader` not enabled/initialized
2. Input actions not bound correctly
3. `CameraInput` singleton not created
4. `GodgameCameraController` guard clause preventing execution

