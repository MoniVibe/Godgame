# Space4X Scene Setup Brief

**Generated**: 2025-01-XX  
**Source**: Analysis of `Assets/Scenes/Demo_Space4X_01.unity` and related Space4X systems

---

## 1. Bootstrap GameObject Components

**GameObject Name**: `Bootstrap`  
**Location**: Scene root

### Components Found:

1. **`Space4X.Presentation.Demo01Authoring`** (MonoBehaviour)
   - **Purpose**: Demo configuration (carrier count, crafts per carrier, asteroid count, LOD settings)
   - **Namespace**: `Space4X.Presentation`
   - **Key Settings**:
     - CarrierCount: 4
     - CraftsPerCarrier: 4
     - AsteroidCount: 20
     - LOD thresholds (FullDetail, ReducedDetail, Impostor)
     - Faction colors array
   - **Note**: This is a presentation-only config. PureDOTS bootstrap components are likely in a SubScene or added via other systems.

### Expected but NOT Found in Main Scene:

The following components are **expected** but may be in a SubScene or added at runtime:

- ❓ `PureDOTS.Authoring.PureDotsConfigAuthoring` - Not found in main scene (may be in SubScene)
- ❓ `PureDOTS.Authoring.SpatialPartitionAuthoring` - Not found in main scene (may be in SubScene)
- ❓ `PureDOTS.Runtime.Camera.BW2CameraInputBridge` - **NOT USED** in Space4X (Space4X uses its own input bridge)
- ❓ `PureDOTS.Authoring.Hybrid.HybridControlToggleAuthoring` - Not found in main scene

**Key Finding**: Space4X does **NOT** use `BW2CameraInputBridge`. Instead, it uses `Space4XCameraInputSystem` (MonoBehaviour) and `Space4XInputBridge` (MonoBehaviour).

---

## 2. Camera GameObject Setup

**GameObject Name**: `Main Camera`  
**Tag**: `MainCamera`  
**Position**: `(0, 1, -10)`

### Components:

1. **`Camera`** (Unity Component)
   - Clear Flags: Skybox
   - Near Clip: 0.3
   - Far Clip: 1000
   - Field of View: 60
   - Culling Mask: Everything (all layers)
   - HDR: Enabled
   - MSAA: Enabled

2. **`AudioListener`** (Unity Component)
   - Standard Unity audio listener

3. **`UnityEngine.Rendering.Universal.UniversalAdditionalCameraData`** (URP Component)
   - URP-specific camera settings
   - Render Shadows: Enabled
   - Renderer Index: -1 (uses default URP renderer)

### Expected but NOT Found:

- ❓ `Space4X.Registry.Space4XCameraAuthoring` - **NOT on Main Camera** in Demo_Space4X_01
- ❓ `Space4X.CameraSystem.Space4XCameraController` - **NOT on Main Camera** in Demo_Space4X_01
- ❓ `PureDOTS.Runtime.Camera.CameraRigApplier` - **NOT on Main Camera** in Demo_Space4X_01

**Key Finding**: The camera in `Demo_Space4X_01` is a **plain Unity Camera** without Space4X camera authoring components. Camera control may be handled entirely by ECS systems or added at runtime.

**Note**: Other Space4X scenes (e.g., `Space4XShowcase_SubScene.unity`) DO have `Space4XCameraAuthoring` and `Space4XCameraInputAuthoring` on the camera GameObject.

---

## 3. Input System Setup

### Input Flow: Unity Input System → ECS Components

**Path 1: Space4X Camera Input (Used in Demo_Space4X_01)**

1. **MonoBehaviour**: `Space4X.Registry.Space4XCameraInputSystem`
   - **Location**: Runtime system (not a GameObject component)
   - **Purpose**: Reads Unity Input System actions and writes to `Space4XCameraControlState` singleton
   - **Input Actions**: Looks for `InputActionAsset` with "Camera" action map
   - **Default Bindings** (if no asset found):
     - WASD / Arrow Keys → Pan
     - Mouse Scroll / +/- → Zoom
     - Mouse Delta → Rotate
     - Q/E → Vertical Move

2. **ECS Component**: `Space4XCameraControlState` (singleton)
   - Contains: `PanInput`, `ZoomInput`, `VerticalMoveInput`, `RotateInput`, enable flags
   - Written by: `Space4XCameraInputSystem.Update()`
   - Read by: `Space4XCameraSystem` (ECS system)

3. **ECS Component**: `Space4XCameraState` (singleton)
   - Contains: `Position`, `Rotation`, `ZoomDistance`, `FocusPoint`
   - Updated by: `Space4XCameraSystem` based on `Space4XCameraControlState`
   - Applied to: Camera transform (via `CameraRigApplier` or direct transform update)

**Path 2: Space4X Selection/Command Input**

1. **MonoBehaviour**: `Space4X.Presentation.Space4XInputBridge`
   - **Location**: `InputBridge` GameObject in scene
   - **Purpose**: Bridges Unity Input System to ECS `SelectionInput` and `CommandInput` singletons
   - **Input Actions**: Uses `InputActionAsset` (can be null, falls back to defaults)

2. **ECS Components**:
   - `SelectionInput` (singleton) - Mouse clicks, selection actions
   - `CommandInput` (singleton) - Right-click commands, move/attack/mine orders
   - `SelectionState` (singleton) - Current selection state

### Key Differences from PureDOTS BW2CameraInputBridge:

- ❌ **NOT USED**: `PureDOTS.Runtime.Camera.BW2CameraInputBridge`
- ✅ **USED**: `Space4X.Registry.Space4XCameraInputSystem` (MonoBehaviour runtime system)
- ✅ **USED**: `Space4X.Presentation.Space4XInputBridge` (for selection/commands)

**Execution Order**: No explicit `[DefaultExecutionOrder]` found on `Space4XCameraInputSystem`, but it runs in `Update()` before camera systems.

---

## 4. Entities Graphics Setup

### How Entities Graphics Connects to Camera:

1. **No Explicit WorldRenderSettings GameObject**: Not found in `Demo_Space4X_01.unity`

2. **Entities Graphics Auto-Detection**:
   - Entities Graphics automatically detects the active camera via `Camera.main` or `Camera.current`
   - No explicit `EntitiesGraphicsWorld` component needed
   - Entities Graphics systems query for entities with `MaterialMeshInfo` and `RenderMeshArray`

3. **Render Setup Flow**:
   - `Shared.Demo.SharedDemoRenderBootstrap` creates `RenderMeshArray` singleton
   - `Space4XPresentationLifecycleSystem` adds `MaterialMeshInfo` and `RenderMeshArray` to entities
   - Entities Graphics renders to the active camera automatically

4. **Camera Requirements**:
   - Camera must be on a layer included in the camera's Culling Mask
   - Entities are typically on Default layer (0)
   - Camera Culling Mask should include Default layer (or Everything)

**Key Finding**: Entities Graphics does **NOT** require explicit `WorldRenderSettings` or `EntitiesGraphicsWorld` components. It works automatically with any active Unity Camera.

---

## 5. Scene Hierarchy

### Actual Hierarchy in `Demo_Space4X_01.unity`:

```
Scene Root
├── Main Camera (GameObject)
│   ├── Camera
│   ├── AudioListener
│   └── UniversalAdditionalCameraData (URP)
├── Bootstrap (GameObject)
│   └── Demo01Authoring (Space4X.Presentation.Demo01Authoring)
├── InputBridge (GameObject)
│   └── Space4XInputBridge (Space4X.Presentation.Space4XInputBridge)
├── DebugCube_SanityCheck (GameObject)
│   ├── Transform
│   ├── MeshFilter
│   ├── MeshRenderer
│   └── BoxCollider
├── DiagnoseDOTSRender (GameObject)
│   └── DiagnoseDOTSRender (MonoBehaviour)
└── SubScene (GameObject)
    └── SubScene component (may contain PureDOTS bootstrap components)
```

### Expected Hierarchy (for full Space4X camera setup):

```
Scene Root
├── Main Camera (GameObject)
│   ├── Camera
│   ├── Space4XCameraAuthoring (Space4X.Registry.Space4XCameraAuthoring)
│   ├── Space4XCameraInputAuthoring (Space4X.Registry.Space4XCameraInputAuthoring)
│   ├── Space4XCameraController (Space4X.CameraSystem.Space4XCameraController)
│   └── CameraRigApplier (PureDOTS.Runtime.Camera.CameraRigApplier)
├── PureDotsConfig (GameObject) [May be in SubScene]
│   └── PureDotsConfigAuthoring (PureDOTS.Authoring.PureDotsConfigAuthoring)
├── SpatialPartition (GameObject) [May be in SubScene]
│   └── SpatialPartitionAuthoring (PureDOTS.Authoring.SpatialPartitionAuthoring)
├── Bootstrap (GameObject)
│   └── Demo01Authoring (Space4X.Presentation.Demo01Authoring)
└── InputBridge (GameObject)
    └── Space4XInputBridge (Space4X.Presentation.Space4XInputBridge)
```

---

## 6. Execution Order

### Critical Execution Order Requirements:

1. **No Explicit Order Found**: `Space4XCameraInputSystem` does not have `[DefaultExecutionOrder]` attribute
   - It runs in `Update()`, which typically runs before `LateUpdate()`
   - Camera systems that read `Space4XCameraControlState` should run after input is written

2. **CameraRigApplier**: `[DefaultExecutionOrder(10000)]`
   - Runs in `LateUpdate()` with high priority
   - Ensures camera transform is applied after all camera controllers

3. **Space4XCameraController**: No explicit execution order
   - Runs in `Update()`
   - Publishes `CameraRigState` to `CameraRigService`
   - `CameraRigApplier` reads this state in `LateUpdate()`

**Key Finding**: Space4X does **NOT** require `[DefaultExecutionOrder(-10050)]` like `BW2CameraInputBridge`. Input systems run in normal `Update()` order.

---

## 7. Quick Checklist: "If X doesn't work, check Y"

### WASD Input Not Working:

1. ✅ **Check**: `Space4XCameraInputSystem` MonoBehaviour exists (runtime system, not GameObject)
2. ✅ **Check**: `Space4XCameraInputAuthoring` exists in scene (bakes input config)
3. ✅ **Check**: `InputActionAsset` is assigned to `Space4XCameraInputAuthoring` (or system uses defaults)
4. ✅ **Check**: `Space4XCameraControlState` singleton exists in ECS world
5. ✅ **Check**: `Space4XCameraSystem` is running and reading `Space4XCameraControlState`
6. ✅ **Check**: Camera has `Space4XCameraController` or `CameraRigApplier` to apply state
7. ❌ **NOT NEEDED**: `BW2CameraInputBridge` (Space4X doesn't use it)

### Entities Not Rendering:

1. ✅ **Check**: Camera Culling Mask includes Default layer (or Everything)
2. ✅ **Check**: Entities have `MaterialMeshInfo` component
3. ✅ **Check**: Entities have `RenderMeshArray` shared component
4. ✅ **Check**: Entities have `RenderBounds` or `WorldRenderBounds`
5. ✅ **Check**: `SharedDemoRenderBootstrap` created `RenderMeshArray` singleton
6. ✅ **Check**: `Space4XPresentationLifecycleSystem` added render components to entities
7. ✅ **Check**: Entities Graphics systems are running (check console for disable warnings)
8. ✅ **Check**: Camera is active and not disabled
9. ❌ **NOT NEEDED**: `WorldRenderSettings` GameObject (Entities Graphics auto-detects camera)

### Camera Not Moving:

1. ✅ **Check**: `Space4XCameraInputSystem` is writing to `Space4XCameraControlState`
2. ✅ **Check**: `Space4XCameraSystem` is reading `Space4XCameraControlState` and updating `Space4XCameraState`
3. ✅ **Check**: Camera has `Space4XCameraController` or `CameraRigApplier` to apply transform
4. ✅ **Check**: Input System is enabled (check `InputSystem.settings`)

---

## 8. Key Differences: Space4X vs PureDOTS BW2 Pattern

| Component | PureDOTS BW2 Pattern | Space4X Pattern |
|-----------|---------------------|-----------------|
| **Input Bridge** | `BW2CameraInputBridge` (MonoBehaviour) | `Space4XCameraInputSystem` (MonoBehaviour runtime) |
| **Input Component** | `CameraInput` (singleton) | `Space4XCameraControlState` (singleton) |
| **Camera Controller** | `BW2StyleCameraController` (MonoBehaviour) | `Space4XCameraController` (MonoBehaviour) |
| **Camera Rig** | `CameraRigState` → `CameraRigApplier` | `CameraRigState` → `CameraRigApplier` (same) |
| **Execution Order** | `[DefaultExecutionOrder(-10050)]` | No explicit order (normal Update) |
| **Camera Authoring** | Not required | `Space4XCameraAuthoring` (optional, for ECS systems) |

---

## 9. Recommended Godgame Setup

Based on Space4X patterns, Godgame should have:

### Bootstrap GameObject:
- ✅ `Godgame.Demo.DemoBootstrap` (existing)
- ❓ `PureDOTS.Authoring.PureDotsConfigAuthoring` (if not in SubScene)
- ❓ `PureDOTS.Authoring.SpatialPartitionAuthoring` (if not in SubScene)

### Camera GameObject:
- ✅ `Camera` (existing)
- ✅ `Godgame.GodgameCameraController` (existing)
- ❓ `PureDOTS.Runtime.Camera.CameraRigApplier` (if using CameraRigState)
- ❓ `Space4X.Registry.Space4XCameraInputAuthoring` (if using Space4X input pattern)

### Input Bridge GameObject:
- ✅ `Godgame.Input.GodgameInputReader` (existing)
- ❌ **NOT NEEDED**: `BW2CameraInputBridge` (Space4X doesn't use it)

### Input Flow (Godgame → Match Space4X):
1. `GodgameInputReader` reads Unity Input System
2. Writes to ECS `CameraInput` or `Space4XCameraControlState` singleton
3. Camera system reads ECS component and updates camera state
4. `CameraRigApplier` or `GodgameCameraController` applies transform

---

## 10. Summary

**Space4X Scene Setup Key Points:**

1. **Bootstrap**: Only `Demo01Authoring` in main scene (PureDOTS configs likely in SubScene)
2. **Camera**: Plain Unity Camera (no authoring components in Demo_Space4X_01)
3. **Input**: `Space4XCameraInputSystem` (MonoBehaviour) + `Space4XInputBridge` (MonoBehaviour)
4. **Rendering**: Entities Graphics auto-detects camera, no explicit WorldRenderSettings needed
5. **Execution Order**: No special execution order requirements (normal Update/LateUpdate)

**For Godgame to Match:**

- Use `Space4XCameraInputSystem` pattern (or equivalent) instead of `BW2CameraInputBridge`
- Ensure camera has `CameraRigApplier` if using `CameraRigState`
- Ensure input bridge writes to ECS singleton components
- Entities Graphics will work automatically with any active camera




