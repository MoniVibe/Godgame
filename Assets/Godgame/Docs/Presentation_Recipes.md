# Presentation Recipes

**Document Location**: `Assets/Godgame/Docs/Presentation_Recipes.md`  
**Target Audience**: Godgame implementers  
**Last Updated**: 2025-12-01

---

This document provides step-by-step recipes for common presentation system tasks.

---

## Recipe 1: Adding a New Villager Task State

### Step 1: Extend the Enum

Edit `Assets/Scripts/Godgame/Presentation/PresentationTagComponents.cs`:

```csharp
public enum VillagerTaskState : byte
{
    Idle = 0,
    Walking = 1,
    Gathering = 2,
    Carrying = 3,
    Throwing = 4,
    MiracleAffected = 5,
    NewTaskState = 6  // Add your new state here
}
```

### Step 2: Update Task State System

Edit `Assets/Scripts/Godgame/Presentation/Godgame_VillagerTaskStateSystem.cs`:

```csharp
switch (job.Phase)
{
    // ... existing cases ...
    case VillagerJob.JobPhase.YourNewPhase:
        taskState = VillagerTaskState.NewTaskState;
        intensity = 0.8f;
        break;
}
```

### Step 3: Update Visual Representation

Update shader/material to handle the new state (e.g., different animation, color tint, icon).

---

## Recipe 2: Adding a New Miracle Type with Visual Effect

### Step 1: Add Miracle Type

Edit `Assets/Scripts/Godgame/Miracles/Godgame_MiracleInputBridgeSystem.cs`:

```csharp
public enum MiracleType : byte
{
    BuffVillagers = 0,
    Smite = 1,
    AlterResources = 2,
    NewMiracleType = 3  // Add your new type
}
```

### Step 2: Spawn Visual Effect

In `Godgame_MiraclePresentationSystem.EnsureAOERing`, add logic to spawn appropriate visual:

```csharp
switch (effect.Type)
{
    case MiracleType.NewMiracleType:
        // Spawn custom particle system, decal, etc.
        break;
}
```

### Step 3: Apply Effect to Entities

In `Godgame_MiraclePresentationSystem.TagAffectedEntities`, add logic for your miracle type:

```csharp
if (effect.Type == MiracleType.NewMiracleType)
{
    // Apply specific effect to entities within radius
}
```

---

## Recipe 3: Adding a New Village Aggregate State

### Step 1: Extend the Enum

Edit `Assets/Scripts/Godgame/Presentation/PresentationTagComponents.cs`:

```csharp
public enum VillageAggregateState : byte
{
    Normal = 0,
    Prosperous = 1,
    Starving = 2,
    UnderMiracle = 3,
    Crisis = 4,
    NewAggregateState = 5  // Add your new state
}
```

### Step 2: Update Aggregate State System

Edit `Assets/Scripts/Godgame/Presentation/Godgame_VillageAggregateStateSystem.cs`:

In `UpdateVillageAggregateStateJob.Execute`, add detection logic:

```csharp
if (/* your condition */)
{
    aggregateState = VillageAggregateState.NewAggregateState;
    intensity = /* your intensity calculation */;
}
```

### Step 3: Update Visual Representation

Update shader/material to handle the new state (e.g., different color tint, particle effect).

---

## Recipe 4: Adding a New Debug Overlay

### Step 1: Add Toggle to DebugInput

Edit `Assets/Scripts/Godgame/Input/GodgameInputComponents.cs`:

```csharp
public struct DebugInput : IComponentData
{
    // ... existing fields ...
    public byte ToggleNewOverlay;  // Add your toggle
}
```

### Step 2: Update Input Reader

Edit `Assets/Scripts/Godgame/Input/GodgameInputReader.cs`:

Add input action mapping:

```csharp
private InputAction _debugToggleNewOverlayAction;

// In Awake():
_debugToggleNewOverlayAction = _debugMap.FindAction("ToggleNewOverlay");

// In Update():
debugInput.ToggleNewOverlay = _debugToggleNewOverlayAction.WasPressedThisFrame() ? (byte)1 : (byte)0;
```

### Step 3: Create Debug System

Create `Assets/Scripts/Godgame/Debug/Godgame_NewDebugSystem.cs`:

```csharp
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct Godgame_NewDebugSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        bool enabled = false;
        if (SystemAPI.TryGetSingleton<DebugInput>(out var debugInput))
        {
            enabled = debugInput.ToggleNewOverlay == 1;
        }

        if (!enabled)
        {
            return;
        }

        // Your debug visualization logic here
    }
}
```

### Step 4: Add Input Action

In Unity Editor, add a new input action to your Input Actions asset:
- Action Map: `Debug`
- Action Name: `ToggleNewOverlay`
- Binding: Your desired key (e.g., `N`)

---

## Recipe 5: Adding a New Entity Type to Presentation

### Step 1: Create Presentation Tag

Edit `Assets/Scripts/Godgame/Presentation/PresentationTagComponents.cs`:

```csharp
public struct NewEntityPresentationTag : IComponentData { }
```

### Step 2: Create Visual State Component

```csharp
public struct NewEntityVisualState : IComponentData
{
    public float4 ColorTint;
    public float Scale;
    // Add your visual state fields
}
```

### Step 3: Create Presentation System

Create `Assets/Scripts/Godgame/Presentation/Godgame_NewEntityPresentationSystem.cs`:

```csharp
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct Godgame_NewEntityPresentationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Read sim data, update visual state
        var job = new UpdateNewEntityPresentationJob();
        job.ScheduleParallel();
    }
}
```

### Step 4: Create Authoring Component

Create `Assets/Scripts/Godgame/Presentation/Authoring/NewEntityPresentationAuthoring.cs`:

```csharp
public class NewEntityPresentationAuthoring : MonoBehaviour
{
    public GameObject MeshPrefab;
    public Material Material;

    class Baker : Baker<NewEntityPresentationAuthoring>
    {
        public override void Bake(NewEntityPresentationAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent<NewEntityPresentationTag>(entity);
            AddComponent(entity, new NewEntityVisualState());
            AddComponent(entity, new PresentationLODState());
            // Add render components
        }
    }
}
```

---

## Recipe 6: Adding Biome Visualization

### Step 1: Ensure GroundTile Has BiomePresentationData

Ground tiles should have `BiomePresentationData` component added by `Godgame_BiomeDataHookSystem`. If not present, add it manually:

```csharp
// In a baker or bootstrap system
AddComponent<BiomePresentationData>(groundTileEntity);
```

### Step 2: Update Material Properties

The `Godgame_BiomeGroundVisualizationSystem` calculates biome tints. To actually apply them:

1. Ensure ground tile entities have `MaterialProperty` components
2. Update shader/material to read biome tint from material properties
3. Use `BiomePresentationData` to drive material property values

### Step 3: Create Overlay Chunks (Far LOD)

For far LOD visualization, use `BiomeOverlayChunk`:

```csharp
var overlayEntity = ecb.CreateEntity();
ecb.AddComponent(overlayEntity, new BiomeOverlayChunk
{
    CenterPosition = chunkCenter,
    Radius = chunkRadius,
    DominantBiome = calculatedBiome,
    AverageMoisture = calculatedMoisture
});
// Add decal/mesh renderer components
```

---

## Recipe 7: Adding Aggregate History Tracking

### Step 1: Add History Component to Entity

For villages:

```csharp
// In baker or bootstrap
var historyBuffer = em.AddBuffer<AggregateHistory>(villageEntity);
// Buffer will be populated by Godgame_AggregateHistorySystem
```

For regions:

```csharp
// In baker or bootstrap
var historyBuffer = em.AddBuffer<AggregateHistory>(regionEntity);
```

### Step 2: Configure Sampling Interval

Adjust `AggregateHistoryConfig` singleton:

```csharp
var config = SystemAPI.GetSingleton<AggregateHistoryConfig>();
config.SampleIntervalTicks = 60; // Sample every 2 seconds at 30 TPS
config.MaxSamples = 120; // Keep 2 minutes of history
```

### Step 3: Read History in Systems

```csharp
if (em.HasBuffer<AggregateHistory>(entity))
{
    var history = em.GetBuffer<AggregateHistory>(entity);
    if (history.Length >= 2)
    {
        var latest = history[history.Length - 1];
        var previous = history[history.Length - 2];
        // Calculate trend
    }
}
```

---

## Recipe 8: Adding a Region Miracle Type

### Step 1: Extend MiracleType Enum

Edit `Assets/Scripts/Godgame/Miracles/Godgame_MiracleInputBridgeSystem.cs`:

```csharp
public enum MiracleType : byte
{
    // ... existing types ...
    BlessRegion = 3,
    CurseRegion = 4,
    RestoreBiome = 5,
    NewRegionMiracle = 6  // Add your new type
}
```

### Step 2: Handle in Region Miracle System

Edit `Assets/Scripts/Godgame/Miracles/Godgame_RegionMiracleSystem.cs`:

In `ApplyRegionMiracleEffects`, add case:

```csharp
case MiracleType.NewRegionMiracle:
    // Apply your effect to BiomePresentationData
    biomeData.Fertility += influence * effect.Intensity * 5f;
    break;
```

### Step 3: Spawn Region Miracle Effect

When casting a region miracle:

```csharp
var regionMiracleEntity = ecb.CreateEntity();
ecb.AddComponent(regionMiracleEntity, new RegionMiracleEffect
{
    CenterPosition = targetPosition,
    Radius = 50f,
    Type = MiracleType.NewRegionMiracle,
    Intensity = 1f,
    Duration = 10f,
    RemainingDuration = 10f
});
```

---

## Recipe 9: Creating a Scenario Preset

### Step 1: Create Preset Asset

1. In Unity Editor: `Assets > Create > Godgame > Scenario Preset`
2. Set `PresetName` and `Description`
3. Assign `DemoConfig` asset
4. Optionally assign `PresentationConfig` asset

### Step 2: Configure Preset Values

- Set `Mode` (Demo01, Scenario_10k, etc.)
- Ensure `Config` has appropriate values for your scenario
- Set `BiomeConfig` (placeholder for future biome distribution)

### Step 3: Use Preset in Scene

1. Add `ScenarioPresetSelector` MonoBehaviour to a GameObject
2. Assign preset assets to `availablePresets` array
3. Select preset from dropdown/buttons
4. Preset configs are applied automatically

---

## Recipe 10: Adding Time Control Input

### Step 1: Add Input Actions

In Unity Editor, add to Input Actions asset:
- Action Map: `TimeControls`
- Actions:
  - `Pause` (binding: Space)
  - `FastForward` (binding: Tab)
  - `SlowMotion` (binding: Shift+Tab)
  - `StepForward` (binding: Period, dev-only)

### Step 2: Extend GodgameInputReader

Edit `Assets/Scripts/Godgame/Input/GodgameInputReader.cs`:

```csharp
private InputAction _timePauseAction;
private InputAction _timeFastForwardAction;
// ... etc

// In Awake():
var timeMap = _inputActions.FindActionMap("TimeControls");
_timePauseAction = timeMap.FindAction("Pause");
_timeFastForwardAction = timeMap.FindAction("FastForward");

// In Update():
if (_timePauseAction.WasPressedThisFrame())
{
    // Write TimeControlCommand to buffer
    var rewindEntity = em.CreateEntityQuery(typeof(RewindState)).GetSingletonEntity();
    var commandBuffer = em.GetBuffer<TimeControlCommand>(rewindEntity);
    commandBuffer.Add(new TimeControlCommand
    {
        Type = TimeControlCommand.CommandType.Pause
    });
}
```

### Step 3: Update TimeControlUI

The `TimeControlUI` MonoBehaviour reads from ECS and displays time state. Ensure it's attached to a GameObject in the scene.

---

**End of Recipes**

