# Request for Space4X Agent: Scene Setup Brief

## Context

The Godgame project needs to match the Space4X scene setup to fix:
1. **WASD input not working** (camera movement)
2. **Only sanity cube visible** (entities not rendering despite having correct components)

## What We Need

Please provide a brief document listing:

### 1. Bootstrap GameObject Components

What MonoBehaviours are on the bootstrap GameObject in `Demo_Space4X_01.unity`?

Expected components (please confirm):
- `PureDOTS.Authoring.PureDotsConfigAuthoring`
- `PureDOTS.Runtime.Camera.BW2CameraInputBridge`
- `PureDOTS.Authoring.Hybrid.HybridControlToggleAuthoring`
- `Space4X.Authoring.Space4XCameraAuthoring` (or similar)
- `Space4X.Authoring.Space4XCameraInputAuthoring` (or similar)
- Any other critical MonoBehaviours

### 2. Camera GameObject Setup

What components are on the main camera GameObject?

Expected:
- `Camera` component
- `Space4XCameraController` or similar?
- Any other camera-related MonoBehaviours

### 3. Input System Setup

How is input bridged from Unity Input System to ECS?

- Is there a MonoBehaviour that reads input and writes to ECS components?
- What ECS components receive input (e.g., `CameraInput`, `CameraRigState`)?
- Is `BW2CameraInputBridge` required, or is there a Space4X-specific bridge?

### 4. Entities Graphics Setup

How is Entities Graphics connected to the camera?

- Is there a `WorldRenderSettings` GameObject?
- Is there an `EntitiesGraphicsWorld` component somewhere?
- How does Entities Graphics know which camera to render to?

### 5. Scene Hierarchy

What is the typical scene hierarchy?

Example:
```
Scene Root
├── PureDOTS_Bootstrap (GameObject)
│   ├── PureDotsConfigAuthoring
│   ├── BW2CameraInputBridge
│   └── ...
├── Main Camera (GameObject)
│   ├── Camera
│   ├── Space4XCameraController
│   └── ...
├── EventSystem (GameObject)
│   └── InputSystemUIInputModule
└── ...
```

### 6. Execution Order

Are there any critical execution order requirements?

- `BW2CameraInputBridge` has `[DefaultExecutionOrder(-10050)]` - is this critical?
- Any other execution order dependencies?

## Current Godgame Setup

Godgame currently has:
- ✅ `Godgame.Demo.DemoBootstrap` on bootstrap GameObject
- ✅ `Godgame.Input.GodgameInputReader` (reads input, writes to ECS)
- ✅ `Godgame.GodgameCameraController` on camera (reads from ECS `CameraInput`)
- ❓ Missing: `BW2CameraInputBridge` (suspected cause of WASD issue)
- ❓ Missing: Other PureDOTS bootstrap components?

## Expected Output

A markdown document (`Space4X_SceneSetup_Brief.md`) with:

1. **Bootstrap GameObject**: List of all MonoBehaviours with their namespaces
2. **Camera GameObject**: List of all components
3. **Input Flow**: How input flows from Unity → ECS → Camera
4. **Rendering Setup**: How Entities Graphics is configured
5. **Quick Checklist**: "If X doesn't work, check Y"

This will allow Godgame to match Space4X's working setup.

