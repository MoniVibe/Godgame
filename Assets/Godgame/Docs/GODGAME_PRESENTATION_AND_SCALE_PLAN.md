# Godgame Presentation, Input & Scale Plan

**Document Location**: `Assets/Godgame/Docs/GODGAME_PRESENTATION_AND_SCALE_PLAN.md`  
**Target Audience**: PureDOTS specialists, Godgame implementers (Unity-side and DOTS-side)  
**Last Updated**: 2025-12-01

---

## Executive Summary

This plan defines how Godgame renders and interacts with millions of simulated entities (villagers, resource chunks, villages, biomes) while maintaining performance and readability. The architecture leverages PureDOTS deterministic simulation, Entities Graphics, and the New Input System to create a scalable visual layer that "shows the world reacting" to divine intervention.

**Key Principles**:
- **Sim-to-View Separation**: PureDOTS simulation entities remain deterministic; Godgame presentation attaches render components directly to sim entities
- **LOD & Aggregates**: Use PureDOTS aggregate contracts (village/band population, wealth, food) to drive impostors and simplified representations at scale
- **Density Sampling**: Render only a fraction of simulated entities using stable ID hashing
- **Input → ECS Pipeline**: New Input System → MonoBehaviour bridge → ECS components → PureDOTS miracle/selection events

---

## Implementation Status

### Completed Components

| Component | File | Description |
|-----------|------|-------------|
| **Presentation Tag Components** | `Assets/Scripts/Godgame/Presentation/PresentationTagComponents.cs` | `VillagerPresentationTag`, `ResourceChunkPresentationTag`, `VillageCenterPresentationTag`, `ResourceNodePresentationTag`, `PresentationLODState`, `VillagerVisualState`, `ResourceChunkVisualState`, `VillageCenterVisualState`, `PresentationConfig` |
| **Villager Presentation Authoring** | `Assets/Scripts/Godgame/Presentation/Authoring/VillagerPresentationAuthoring.cs` | Baker for villager presentation entities |
| **Resource Chunk Presentation Authoring** | `Assets/Scripts/Godgame/Presentation/Authoring/ResourceChunkPresentationAuthoring.cs` | Baker for resource chunk presentation entities |
| **Village Center Presentation Authoring** | `Assets/Scripts/Godgame/Presentation/Authoring/VillageCenterPresentationAuthoring.cs` | Baker for village center presentation entities |
| **Resource Node Presentation Authoring** | `Assets/Scripts/Godgame/Presentation/Authoring/ResourceNodePresentationAuthoring.cs` | Baker for resource node presentation entities |
| **Presentation Config Authoring** | `Assets/Scripts/Godgame/Presentation/Authoring/PresentationConfigAuthoring.cs` | Baker for presentation configuration singleton |
| **Villager Presentation System** | `Assets/Scripts/Godgame/Presentation/Godgame_VillagerPresentationSystem.cs` | LOD and visual state updates for villagers |
| **Resource Chunk Presentation System** | `Assets/Scripts/Godgame/Presentation/Godgame_ResourceChunkPresentationSystem.cs` | LOD and visual state updates for chunks |
| **Village Presentation System** | `Assets/Scripts/Godgame/Presentation/Godgame_VillagePresentationSystem.cs` | LOD and visual state updates for villages |
| **Presentation Metrics System** | `Assets/Scripts/Godgame/Presentation/Godgame_PresentationMetricsSystem.cs` | Metrics collection and display |
| **Input Components** | `Assets/Scripts/Godgame/Input/GodgameInputComponents.cs` | `CameraInput`, `MiracleInput`, `SelectionInput`, `DebugInput` |
| **Input Reader** | `Assets/Scripts/Godgame/Input/GodgameInputReader.cs` | MonoBehaviour bridge from New Input System to ECS |
| **Miracle Input Bridge** | `Assets/Scripts/Godgame/Miracles/Godgame_MiracleInputBridgeSystem.cs` | Bridges new input to miracle system |
| **Demo Bootstrap** | `Assets/Scripts/Godgame/Demo/Godgame_Demo01_BootstrapSystem.cs` | Spawns demo villages, villagers, and resources |

---

## 1. Vertical Slice Definition: Godgame Demo_01

### Scope

**Godgame Demo_01** is the first visual demonstration that expresses the core fantasy with minimal but clear elements.

#### Minimum Required Elements

1. **Villages (aggregates)**: 3 villages with distinct centers, influence zones
2. **Villagers**: 90 villagers (20-40 per village) with different alignments
3. **Resource nodes**: 30 nodes of various types (wood, ore, food, stone, herbs)
4. **Resource chunks**: 20 chunks scattered around the world
5. **Miracles**: Buff, Smite, and Alter Resources (via RMB + slot selection)

### Entity Types to Render

| Entity Type | Sim Fields | Visual Cues |
|-------------|------------|-------------|
| **Villagers** | `LocalTransform`, `VillagerBehavior`, `VillagerAlignment` | Capsule mesh, alignment color tint (red=vengeful, blue=forgiving) |
| **Village Centers** | `Village`, `VillageResource` buffer | Building mesh, phase color (gray=forming, green=growing, blue=stable, red=crisis) |
| **Resource Chunks** | `ExtractedResource`, `LocalTransform` | Box mesh, resource type color (brown=wood, gray=ore, yellow=food) |
| **Resource Nodes** | `ExtractedResource`, `LocalTransform` | Marker mesh, resource type color |

### Demo Flow

1. Camera starts zoomed out showing 3 villages
2. Villagers move within village influence zones
3. Resource nodes and chunks visible around the map
4. Player can select miracle slots (1, 2, 3 keys)
5. Player can cast miracles (RMB click)
6. Metrics overlay shows LOD breakdown (O key to toggle)

---

## 2. Rendering Architecture

### SubScenes & Prefabs Organization

```
Assets/Scenes/
├── Godgame_Demo_01.unity (main scene)
└── SubScenes/
    ├── CoreWorld.subscene          # Core ECS world
    ├── Environment.subscene        # Biomes, ground (optional)
    └── Presentation.subscene       # Camera, lighting, UI

Assets/Godgame/Assets/Prefabs/
├── VillagerPresentation.prefab     # Villager visual
├── ResourceChunkPresentation.prefab # Chunk visual
├── VillageCenterPresentation.prefab # Village center visual
└── ResourceNodePresentation.prefab  # Resource node visual
```

### Sim-to-View Mapping

**Decision**: Sim entities directly hold render components (no separate view entities).

**Rationale**:
- Simpler architecture, fewer entities
- Entities Graphics handles instancing automatically
- LOD/density via component enable/disable

### Core Presentation Systems

| System | Reads | Writes | Responsibility |
|--------|-------|--------|----------------|
| `Godgame_VillagerPresentationSystem` | `VillagerBehavior`, `LocalTransform` | `PresentationLODState`, `VillagerVisualState` | LOD, alignment tint |
| `Godgame_ResourceChunkPresentationSystem` | `ExtractedResource`, `LocalTransform` | `PresentationLODState`, `ResourceChunkVisualState` | LOD, resource type color |
| `Godgame_VillagePresentationSystem` | `Village`, `VillageResource` | `PresentationLODState`, `VillageCenterVisualState` | LOD, phase color |
| `Godgame_PresentationMetricsSystem` | All presentation tags | `PresentationMetrics` | Metrics collection |

---

## 3. LOD, Aggregates, and Render Density

### LOD Bands

| Range | LOD Level | Villager Detail | Target Count |
|-------|-----------|-----------------|--------------|
| < 50 units | LOD0_Full | Full detail, icon overlays | Up to 1,000 |
| 50-200 units | LOD1_Mid | Simplified, no overlays | Up to 10,000 |
| > 200 units | LOD2_Far | Impostors only | Unlimited |

### Density Sampling

**Method**: Stable ID hashing using `entityIndex % densityDivisor`

```csharp
// Density slider: 0.1 = render 10% of villagers
int densityDivisor = (int)(1f / densitySlider);
bool shouldRender = (entityIndex % densityDivisor == 0);
```

**Rules**:
- Always render entities within LOD0 range
- Always render village centers
- Sample only applies to mid/far range entities

### Village Impostors

Village centers always render (serve as impostors at distance):
- **Marker mesh**: Flagpole/obelisk at village center
- **Phase color**: Gray=forming, green=growing, blue=stable, red=crisis
- **Influence ring**: Subtle ground decal (future)

---

## 4. Input, Miracles, and Interaction

### Input Actions

| Action Map | Actions | Bindings |
|------------|---------|----------|
| **Camera** | Move, Rotate, Zoom, Pan, Focus | WASD, MMB drag, Scroll, LMB drag, F |
| **Selection** | Select, VillageSelect, RegionSelect | LMB, Ctrl+LMB, Shift+LMB |
| **Miracles** | MiracleSlot1-3, CastMiracle, SustainedCast, ThrowCast | 1/2/3, RMB click, RMB hold, RMB release |
| **Debug** | ToggleHeatmap, ToggleOverlays, ToggleLOD | H, O, L |

### Input → ECS Pipeline

```
New Input System → GodgameInputReader (MonoBehaviour)
                 → CameraInput, MiracleInput, SelectionInput (ECS components)
                 → Godgame_MiracleInputBridgeSystem
                 → MiracleEffect entities
```

### ECS Input Components

| Component | Fields | Purpose |
|-----------|--------|---------|
| `CameraInput` | Move, Rotate, Zoom, Pan, Focus, PointerPosition, PointerWorldPosition | Camera control |
| `MiracleInput` | SelectedSlot, TargetPosition, CastTriggered, SustainedCastHeld, ThrowCastTriggered | Miracle casting |
| `SelectionInput` | ScreenPosition, SelectedEntity, VillageSelect, RegionSelect | Entity selection |
| `DebugInput` | ToggleHeatmap, ToggleOverlays, ToggleLOD | Debug toggles |

### Miracle Feedback

- **Cast trigger**: Spawns `MiracleEffect` entity at target position
- **Effect types**: BuffVillagers (0), Smite (1), AlterResources (2)
- **Visual feedback**: Effect entity with position, radius, intensity, duration
- **Fade out**: Intensity decreases as duration expires

---

## 5. Profiling Hooks & Large-Scale Views

### Presentation Metrics

| Metric | Description |
|--------|-------------|
| `VillagersTotal` | Total villagers in simulation |
| `VillagersRendered` | Villagers with `ShouldRender = 1` |
| `VillagersLOD0/1/2` | Villagers in each LOD band |
| `ChunksTotal/Rendered` | Resource chunks |
| `VillagesTotal/Rendered` | Village centers |
| `ResourceNodesTotal/Rendered` | Resource nodes |
| `RenderRatio` | `VillagersRendered / VillagersTotal` |

### Metrics Display

Press **O** key to toggle metrics overlay showing:
- Villager counts by LOD
- Chunk and village counts
- Render ratio

### Performance Rules

| Rule | Threshold | Action |
|------|-----------|--------|
| Max LOD0 villagers | > 1,000 | Increase LOD distance threshold |
| Max visible villages | > 100 | Force impostor mode |
| Max visible chunks | > 10,000 | Enable chunk sampling |

---

## 6. Usage Guide

### Setting Up a Demo Scene

1. Add `PresentationConfigAuthoring` to a GameObject in your scene
2. Configure LOD distances and density settings
3. Add `GodgameInputReader` to a GameObject for input
4. The `Godgame_Demo01_BootstrapSystem` will spawn demo content automatically

### Adding Presentation to Entities

For villagers:
```csharp
entityManager.AddComponent<VillagerPresentationTag>(entity);
entityManager.AddComponentData(entity, new PresentationLODState { ... });
entityManager.AddComponentData(entity, new VillagerVisualState { ... });
```

For resource chunks:
```csharp
entityManager.AddComponent<ResourceChunkPresentationTag>(entity);
entityManager.AddComponentData(entity, new PresentationLODState { ... });
entityManager.AddComponentData(entity, new ResourceChunkVisualState { ... });
```

### Casting Miracles

1. Select miracle slot with 1/2/3 keys
2. Right-click to cast at mouse position
3. Hold RMB for sustained cast, release for throw cast

---

## 7. Open Questions & Dependencies

### Questions for PureDOTS Specialist

1. **Aggregate Contracts**: What aggregate components will PureDOTS provide for villages/bands?
2. **LOD Hints**: Will PureDOTS provide importance scores for LOD decisions?
3. **Sample Indices**: Should Godgame use stable ID hashing or PureDOTS-provided indices?
4. **Performance Targets**: What are target entity counts for 10k/100k/1M scenarios?
5. **Spatial Queries**: Can Godgame use PureDOTS spatial grid for raycast/selection?

### Dependencies

| Dependency | Version | Status |
|------------|---------|--------|
| Unity Entities | 1.4.3 | Required |
| Entities Graphics | 1.4.3 | Required |
| Input System | 1.7.0 | Required |
| PureDOTS | Local package | Required |

---

## 8. File Index

### Presentation Components & Systems
- `Assets/Scripts/Godgame/Presentation/PresentationTagComponents.cs`
- `Assets/Scripts/Godgame/Presentation/Godgame_VillagerPresentationSystem.cs`
- `Assets/Scripts/Godgame/Presentation/Godgame_ResourceChunkPresentationSystem.cs`
- `Assets/Scripts/Godgame/Presentation/Godgame_VillagePresentationSystem.cs`
- `Assets/Scripts/Godgame/Presentation/Godgame_PresentationMetricsSystem.cs`

### Authoring Components
- `Assets/Scripts/Godgame/Presentation/Authoring/VillagerPresentationAuthoring.cs`
- `Assets/Scripts/Godgame/Presentation/Authoring/ResourceChunkPresentationAuthoring.cs`
- `Assets/Scripts/Godgame/Presentation/Authoring/VillageCenterPresentationAuthoring.cs`
- `Assets/Scripts/Godgame/Presentation/Authoring/ResourceNodePresentationAuthoring.cs`
- `Assets/Scripts/Godgame/Presentation/Authoring/PresentationConfigAuthoring.cs`

### Input System
- `Assets/Scripts/Godgame/Input/GodgameInputComponents.cs`
- `Assets/Scripts/Godgame/Input/GodgameInputReader.cs`

### Miracle System
- `Assets/Scripts/Godgame/Miracles/Godgame_MiracleInputBridgeSystem.cs`

### Demo Bootstrap
- `Assets/Scripts/Godgame/Demo/Godgame_Demo01_BootstrapSystem.cs`

---

## Phase 2: From Demo_01 to Scale-Ready Dev Tool

**Status**: Implemented (2025-12-01)

Phase 2 transforms Demo_01 from a fixed bootstrap into a configurable, debuggable, scale-ready development tool that validates presentation behavior at 10k/100k/1M entity scales.

### Phase 2 Components

| Component | File | Description |
|-----------|------|-------------|
| **DemoConfig** | `Assets/Scripts/Godgame/Demo/DemoConfig.cs` | ScriptableObject for demo parameters |
| **DemoConfigAuthoring** | `Assets/Scripts/Godgame/Demo/DemoConfigAuthoring.cs` | Baker for DemoConfig blob |
| **DemoScenarioMode** | `Assets/Scripts/Godgame/Demo/DemoScenarioMode.cs` | Enum for scenario modes (Demo01, 10k, 100k, 1M) |
| **Scenario Bootstrap** | `Assets/Scripts/Godgame/Demo/Godgame_ScenarioBootstrapSystem.cs` | Integrates with PureDOTS ScenarioRunner |
| **Demo Validation** | `Assets/Scripts/Godgame/Demo/Godgame_DemoValidationSystem.cs` | Editor-only validation checks |
| **Villager Task State System** | `Assets/Scripts/Godgame/Presentation/Godgame_VillagerTaskStateSystem.cs` | Updates visual task state from sim |
| **Village Aggregate State System** | `Assets/Scripts/Godgame/Presentation/Godgame_VillageAggregateStateSystem.cs` | Updates village aggregate visual state |
| **Miracle Presentation Enhancement** | `Assets/Scripts/Godgame/Miracles/Godgame_MiracleInputBridgeSystem.cs` | AOE rings, highlights, tinting |
| **Entity Inspection** | `Assets/Scripts/Godgame/Debug/EntityInspectionData.cs` | Component for entity inspection |
| **Entity Inspection System** | `Assets/Scripts/Godgame/Debug/Godgame_EntityInspectionSystem.cs` | Reads selected entity sim data |
| **Entity Inspection UI** | `Assets/Scripts/Godgame/Debug/EntityInspectionUI.cs` | MonoBehaviour for inspection overlay |
| **LOD Visualization** | `Assets/Scripts/Godgame/Debug/Godgame_LODVisualizationSystem.cs` | Tints entities by LOD (green/yellow/red) |
| **Density Visualization** | `Assets/Scripts/Godgame/Debug/Godgame_DensityVisualizationSystem.cs` | Shows markers above rendered entities |
| **Pathfinding Debug** | `Assets/Scripts/Godgame/Debug/Godgame_PathfindingDebugSystem.cs` | Shows villager target positions |
| **Performance Profiler** | `Assets/Scripts/Godgame/Performance/Godgame_PerformanceProfiler.cs` | Captures Unity Profiler data |
| **Performance Validation** | `Assets/Scripts/Godgame/Performance/Godgame_PerformanceValidationSystem.cs` | Checks budgets and logs warnings |

---

## Configuration Guide

### DemoConfig

Create a `DemoConfig` ScriptableObject asset (Assets > Create > Godgame > Demo Config) and assign it to a `DemoConfigAuthoring` component in your scene.

**Fields**:
- `Mode`: Scenario mode (Demo01, Scenario_10k, Scenario_100k, Scenario_1M)
- `VillageCount`: Number of villages to spawn (default: 3)
- `VillagersPerVillageMin/Max`: Villager count range per village (default: 20-40)
- `VillageSpacing`: Distance between villages (default: 80f)
- `VillageInfluenceRadius`: Influence radius for each village (default: 40f)
- `ResourceNodeCount`: Number of resource nodes (default: 30)
- `ResourceChunkCount`: Number of resource chunks (default: 20)
- `RandomSeed`: Seed for deterministic spawning (default: 12345)

### PresentationConfig

Place a `PresentationConfigAuthoring` component in your scene to configure LOD and performance settings.

**Fields**:
- `LOD0Distance`: Full detail distance (default: 50f)
- `LOD1Distance`: Mid detail distance (default: 200f)
- `LOD2Distance`: Far detail distance (default: 500f)
- `DensitySlider`: Render density (0.01-1.0, default: 1.0)
- `MaxLOD0Villagers`: Max villagers at full detail (default: 1000)
- `MaxRenderedChunks`: Max chunks rendered (default: 10000)
- `MaxFrameTimeMs`: Frame time budget (default: 16.67ms)
- `MaxDrawCalls`: Draw call budget (default: 1000)
- `MaxBatches`: Batch budget (default: 500)

---

## Scenario Integration Guide

### Using Scenario Modes

1. Create a `DemoConfig` asset
2. Set `Mode` to `Scenario_10k`, `Scenario_100k`, or `Scenario_1M`
3. Assign to `DemoConfigAuthoring` in scene
4. `Godgame_ScenarioBootstrapSystem` will automatically:
   - Adjust `PresentationConfig` for scale
   - Add presentation components to PureDOTS scenario entities
   - Configure LOD distances and density sampling

### Scenario-Specific Settings

- **10k**: LOD0=30m, LOD1=150m, Density=50%, MaxLOD0=500
- **100k**: LOD0=20m, LOD1=100m, Density=10%, MaxLOD0=200
- **1M**: LOD0=10m, LOD1=50m, Density=1%, MaxLOD0=100

---

## Debug Tools Reference

### Hotkeys

| Key | Function | System |
|-----|----------|--------|
| **O** | Toggle presentation metrics overlay | `PresentationMetricsDisplay` |
| **I** | Toggle entity inspection overlay | `EntityInspectionUI` |
| **L** | Toggle LOD visualization (green/yellow/red tints) | `Godgame_LODVisualizationSystem` |
| **D** | Toggle density sampling markers | `Godgame_DensityVisualizationSystem` |
| **P** | Toggle pathfinding debug (target lines) | `Godgame_PathfindingDebugSystem` |
| **H** | Toggle heatmap overlay | (Future) |
| **O** | Toggle all debug overlays | (Future) |

### Entity Inspection

1. Select an entity (via `SelectionInput`)
2. Press **I** to toggle inspection overlay
3. View entity summary (ID, position, task, resources, etc.)

### LOD Visualization

Press **L** to enable LOD color coding:
- **Green**: LOD0 (Full detail)
- **Yellow**: LOD1 (Mid detail)
- **Red**: LOD2 (Far detail)
- **Gray**: Culled (not rendered)

---

## Visual Language Guide

### Villager Task States

| State | Visual Cue | Source |
|-------|------------|--------|
| **Idle** | Default appearance | `VillagerJob.Phase == Idle` |
| **Walking** | Movement animation | `VillagerJob.Phase == Navigating` |
| **Gathering** | Gathering animation | `VillagerJob.Phase == Gathering` |
| **Carrying** | Carrying pose | `VillagerJob.Phase == Delivering` |
| **Throwing** | Throwing animation | (Future) |
| **MiracleAffected** | Glow/tint overlay | `MiracleAffectedTag` present |

### Village Aggregate States

| State | Visual Cue | Source |
|-------|------------|--------|
| **Normal** | Default phase color | Default state |
| **Prosperous** | Bright green tint | High resources + Growing/Expanding phase |
| **Starving** | Red tint | Low food + Declining phase |
| **UnderMiracle** | Miracle effect overlay | `MiracleEffect` within influence radius |
| **Crisis** | Dark red tint | `VillagePhase == Crisis` |

### Miracle Effects

- **AOE Ring**: Circle decal at miracle position (`MiracleAreaOfEffect`)
- **Target Highlight**: Entities within radius get `MiracleAffectedTag`
- **Pulse Effect**: Intensity fades over duration
- **Entity Tinting**: Villagers/chunks/villages get tint overlay

---

## Performance Budgets

### Recommended Limits

- **Max visible villagers at full detail**: 1,000
- **Max villages with full detail**: 100
- **Max rendered chunks**: 10,000

### Frame Time Targets

| Scenario | Target Frame Time | Acceptable |
|----------|-------------------|------------|
| Demo_01 | < 10ms | < 16ms |
| 10k | < 16ms | < 33ms |
| 100k | < 33ms (30fps) | < 50ms |
| 1M | < 100ms (10fps) | < 200ms (stress test) |

### Performance Validation

`Godgame_PerformanceValidationSystem` checks budgets every 60 frames and logs warnings when:
- Frame time exceeds `MaxFrameTimeMs`
- Rendered villagers exceed `MaxLOD0Villagers * 2`
- Rendered chunks exceed `MaxRenderedChunks`

---

## How to Add a New Visual State

### Adding a Villager Task State

1. Add enum value to `VillagerTaskState` in `PresentationTagComponents.cs`
2. Update `Godgame_VillagerTaskStateSystem` to map sim data to new state
3. Update shader/material to handle new state visually

### Adding a Village Aggregate State

1. Add enum value to `VillageAggregateState` in `PresentationTagComponents.cs`
2. Update `Godgame_VillageAggregateStateSystem` to detect new state
3. Update shader/material to handle new state visually

---

## How to Add a New Miracle Effect

1. Add miracle type to `MiracleType` enum in `Godgame_MiracleInputBridgeSystem.cs`
2. Update `Godgame_MiraclePresentationSystem` to spawn appropriate AOE ring
3. Add visual effect (particles, decals, etc.) to miracle entity
4. Update `Godgame_MiracleTintingSystem` to apply appropriate tint

---

## How to Add a New Debug Overlay

1. Add toggle field to `DebugInput` in `GodgameInputComponents.cs`
2. Update `GodgameInputReader` to read new input action
3. Create new debug system (e.g., `Godgame_NewDebugSystem.cs`)
4. Query `DebugInput` toggle state in system
5. Implement visualization logic

---

## Phase 3: Biome-Aware Demo-Ready Experience

**Status**: Implemented (2025-12-01)

Phase 3 transforms Godgame from a robust dev testbed into a biome-aware, long-term-dynamics viewer with demo-ready UX. Focus on biomes/climate visualization, time-aware aggregates, region-scale miracles, scenario authoring tools, and graceful performance degradation.

### Phase 3 Components

| Component | File | Description |
|-----------|------|-------------|
| **BiomePresentationData** | `Assets/Scripts/Godgame/Presentation/BiomePresentationComponents.cs` | Cached biome/climate data for presentation |
| **Biome Data Hook System** | `Assets/Scripts/Godgame/Presentation/Godgame_BiomeDataHookSystem.cs` | Reads GroundBiome/Moisture/Temperature and writes presentation data |
| **Biome Ground Visualization** | `Assets/Scripts/Godgame/Presentation/Godgame_BiomeGroundVisualizationSystem.cs` | Updates ground tile material properties based on biome |
| **Biome Overlay System** | `Assets/Scripts/Godgame/Presentation/Godgame_BiomeOverlaySystem.cs` | Creates overlay chunks for far LOD visualization |
| **Vegetation Presentation** | `Assets/Scripts/Godgame/Presentation/Godgame_VegetationPresentationSystem.cs` | Updates vegetation visual state with LOD/density sampling |
| **Aggregate History** | `Assets/Scripts/Godgame/Presentation/AggregateHistoryComponents.cs` | Rolling buffer for village/region history sampling |
| **Aggregate History System** | `Assets/Scripts/Godgame/Presentation/Godgame_AggregateHistorySystem.cs` | Samples aggregates periodically and stores in buffers |
| **Trend Visualization** | `Assets/Scripts/Godgame/Presentation/Godgame_TrendVisualizationSystem.cs` | Calculates and visualizes trends from history |
| **Time Control System** | `Assets/Scripts/Godgame/Input/GodgameTimeControlSystem.cs` | Integrates with PureDOTS TimeControlCommand |
| **Time Control UI** | `Assets/Scripts/Godgame/Presentation/TimeControlUI.cs` | MonoBehaviour for time control buttons and display |
| **Region Miracle Components** | `Assets/Scripts/Godgame/Miracles/RegionMiracleComponents.cs` | Components for region-scale miracles |
| **Region Miracle System** | `Assets/Scripts/Godgame/Miracles/Godgame_RegionMiracleSystem.cs` | Applies region miracles to biome/vegetation |
| **Scenario Preset** | `Assets/Scripts/Godgame/Demo/ScenarioPreset.cs` | ScriptableObject for scenario presets |
| **Scenario Preset Selector** | `Assets/Scripts/Godgame/Demo/ScenarioPresetSelector.cs` | MonoBehaviour for selecting presets |

---

## Phase 3: Biome & Climate Visualization

### Biome Data Hookup

The `Godgame_BiomeDataHookSystem` reads `GroundBiome`, `GroundMoisture`, and `GroundTemperature` from existing `GroundTile` entities and writes `BiomePresentationData` for presentation systems. It also samples the `ClimateState` singleton for global climate data.

**Components**:
- `BiomePresentationData`: Cached biome type, moisture, temperature, and calculated fertility
- `BiomeOverlayChunk`: Tag for overlay chunks (far LOD visualization)

### Biome/Ground Visualization (Hybrid)

**Close-Up (GroundTiles)**:
- Extends `GroundTile` entities with material property updates
- Updates material properties based on `BiomePresentationData`:
  - Biome color tint (per `BiomeType`)
  - Moisture intensity (affects saturation)
  - Temperature tint (warm/cool shift)

**Far LOD (Overlay)**:
- Creates chunk-based overlay system for distant views
- `BiomeOverlayChunk` represents a region chunk with dominant biome and average moisture
- Spawns decal entities or simple mesh quads with biome-colored materials

**LOD Integration**:
- Close-up (LOD0-1): Use `GroundTile` material properties
- Far (LOD2+): Use overlay chunks, hide individual ground tiles
- Density sampling: Only show overlay chunks for sampled regions

### Vegetation Rendering Strategy

**Components**:
- `VegetationPresentationTag`: Tag for vegetation entities
- `VegetationVisualState`: Growth stage, health, clumped flag, biome tint

**Rendering Strategy**:
- **Individual entities** (LOD0, near camera): Render individual `PlantStand` entities
- **Clumped/instanced** (LOD1, mid-range): Use instanced rendering for vegetation patches
- **Aggregated** (LOD2+, far): Show simplified markers or hide entirely

**Biome Reactivity**:
- Vegetation appearance adjusts based on `BiomePresentationData`
- Higher fertility = greener/more vibrant colors
- Lower fertility = lower health and darker colors

---

## Phase 3: Long-Term Dynamics & Time-Aware Visualization

### Aggregate History Sampling

The `Godgame_AggregateHistorySystem` periodically samples key aggregates and stores them in rolling buffers:

**Components**:
- `AggregateHistory`: Buffer entry with tick, population, wealth, food, vegetation health, fertility
- `VillageHistory`: Attached to village entities (60 samples = ~1 minute at 30 TPS)
- `RegionHistory`: Attached to region/biome entities (60 samples)

**Sampling Logic**:
- Samples every N ticks (configurable, default: 30 ticks = 1 second)
- For villages: Samples `Village.MemberCount`, `VillageResource` totals, food amount
- For regions: Samples average `VegetationHealth`, `BiomePresentationData.Fertility`
- Uses FIFO rolling buffers (fixed size: 60 samples)

### Trend Visualizations

The `Godgame_TrendVisualizationSystem` calculates trends from aggregate history:

**Village Trends**:
- Compares last 10 samples vs previous 10 samples
- Determines: Improving (+), Stable (=), Declining (-)
- Visual indicators:
  - Small icon overlay on village center (arrow up/down/stable)
  - Color shift in `VillageCenterVisualState` (green = improving, red = declining)

**Region Trends**:
- Compares vegetation health / fertility over time
- Visual indicators:
  - Color overlay on biome ground (brighter = improving, darker = declining)
  - Subtle gradient shifts in `BiomePresentationData` tint

**Inspection UI Integration**:
- Extended `EntityInspectionUI` shows trend summary:
  - "Population: 45 → 52 (+7, improving)"
  - "Food: 120 → 80 (-40, declining)"
  - Simple text-based trend indicators

### Time Controls (PureDOTS Integration)

The `GodgameTimeControlSystem` and `TimeControlUI` integrate with PureDOTS `TimeControlCommand`:

**PureDOTS Integration**:
- Reads `TimeControlCommand` buffer from `RewindState` entity
- Writes commands: `Pause`, `SetSpeed`, `StepForward`, `StartRewind`
- Reads `TimeState.IsPaused`, `TimeState.TimeScale` for UI feedback

**Input Actions**:
- `TimeControls/Pause` (Space key)
- `TimeControls/FastForward` (Tab key, cycles: 1x → 2x → 5x → 10x → 1x)
- `TimeControls/SlowMotion` (Shift+Tab, cycles: 1x → 0.5x → 0.25x → 1x)
- `TimeControls/StepForward` (Period key, dev-only)

**UI**:
- Shows current time scale (1x, 2x, Paused, etc.)
- Shows current tick from `TimeState.Tick`
- Buttons for pause/fast-forward/slow-motion
- Dev-only: Step forward button

---

## Phase 3: Miracles 2.0 - Biome- & Aggregate-Scale Interactions

### Biome / Region Miracles

**New Miracle Types**:
- `BlessRegion`: Increases fertility, vegetation health
- `CurseRegion`: Decreases fertility, causes drought
- `RestoreBiome`: Heals damaged vegetation, restores fertility

**Components**:
- `RegionMiracleEffect`: Tracks region miracles with center position, radius, type, intensity, duration

**Visual Presentation**:
- Area overlays: Soft gradient decals covering affected region
- Special tints: Long-lived but low-cost VFX (material property changes)
- Not per-entity spam: Single overlay entity per region miracle

**System**:
- `Godgame_RegionMiracleSystem`: Applies region miracles to biome/vegetation
- Updates `BiomePresentationData` (fertility, moisture)
- Updates `VegetationVisualState` (health) for affected vegetation

### Aggregate-Focused Miracles

Enhanced `Godgame_MiraclePresentationSystem` detects village-level effects:
- When miracle affects village:
  - Updates `VillageCenterVisualState.AggregateState`:
    - `UnderMiracle` → transitions to `Prosperous` (if buff) or `Crisis` (if curse)
  - Applies aggregate stat modifiers:
    - Buff: Increase `Village.MemberCount` growth rate, `VillageResource` accumulation
    - Curse: Decrease growth rate, increase unrest
- Long-term visual shifts: Village center color/intensity changes persist after miracle expires
- Tracks miracle effects in `VillageHistory` buffer (show before/after in trends)

### Miracle Telemetry + Feedback

Extended `PresentationMetrics` includes:
- `MiraclesCastThisSession`
- `RegionMiraclesCast`
- `VillageMiraclesCast`

**Before/After Metrics**:
- When miracle cast on village:
  - Captures snapshot: `Village.MemberCount`, `VillageResource` totals
  - Stores in `MiracleEffect` component or separate tracking entity
  - After N ticks (e.g., 300 ticks = 10 seconds), compares and logs change
- Display in inspection UI or miracle feedback overlay:
  - "BlessRegion cast: Population +5, Food +20"

---

## Phase 3: Scenario Authoring UX & Presets

### Scenario Presets

**ScriptableObject**: `ScenarioPreset`
- `PresetName`: Name of the preset
- `Description`: Description text
- `Mode`: `DemoScenarioMode` (Demo01, 10k, 100k, 1M)
- `Config`: Reference to `DemoConfig` asset
- `PresentationConfig`: Reference to `PresentationConfig` asset (optional)
- `BiomeConfig`: Biome distribution (placeholder)

**Preset Types**:
- "Peaceful Growth": Many villages, abundant resources, stable biomes
- "Famine Stress Test": Few resources, many villagers, harsh biomes
- "Miracle Playground": Many villages, frequent miracle opportunities
- "Biome Stress": Max vegetation, many nodes, diverse biomes

**Selection Experience**:
- `ScenarioPresetSelector` MonoBehaviour:
  - Dropdown or button grid to select preset
  - Shows preset description
  - Applies preset configs on selection

### Authoring Helpers

**Editor Tools** (placeholder - full implementation requires editor scripts):
- Menu items: `Tools/Godgame/Place Village Center`, `Place Resource Node`, `Place Biome Region`
- Click in scene → spawns GameObject with appropriate authoring component
- Automatically generates ECS entities via bakers

**Authoring Components** (to be created):
- `VillagePlacementAuthoring`: Place village centers in scene
- `ResourceNodePlacementAuthoring`: Place resource nodes
- `BiomeRegionAuthoring`: Define biome regions

---

## Phase 3: Demo-Ready UX & Failsafe Defaults

### Minimal Player-Facing HUD

**Components**:
- `PlayerHUDData`: Singleton for HUD data
  - `ScenarioName`: Current scenario name/mode
  - `VillageCount`: Number of villages
  - `VillagerCount`: Number of villagers
  - `WorldHealth`: Global "world health" indicator (0-1, aggregate of village states)

**HUD Elements**:
- Current scenario name/mode (top-left)
- Key stats (top-right): Number of villages, villagers, world health indicator
- Buttons (bottom-right):
  - "Metrics" toggle (shows/hides `PresentationMetricsDisplay`)
  - "Debug" menu (dropdown with LOD/Density/Pathfinding toggles)

**System**:
- `Godgame_PlayerHUDSystem`: Updates `PlayerHUDData` from sim
- Reads `Village` entities, `VillagerBehavior` entities
- Calculates world health from `VillageCenterVisualState.AggregateState`

### Safe Config Presets

**Preset Modes**:
- "Showcase" mode:
  - `DensitySlider = 0.8f` (render 80%)
  - `LOD0Distance = 40f`, `LOD1Distance = 150f`
  - `MaxLOD0Villagers = 800`
  - Visually rich but safe on mid-range hardware
- "Scale Stress" mode:
  - `DensitySlider = 0.1f` (render 10%)
  - `LOD0Distance = 20f`, `LOD1Distance = 100f`
  - `MaxLOD0Villagers = 200`
  - Focused on metrics and performance

**Default Configuration**:
- `DemoConfig` and `PresentationConfig` have sensible defaults
- Game starts in "Showcase" mode by default

### Graceful Degradation

**System**: `Godgame_AutoPerformanceAdjustmentSystem` (to be created)

**Responsibilities**:
- Monitors `PresentationMetrics` and `PresentationConfig` budgets
- If budgets exceeded:
  - Automatically increases LOD thresholds (`LOD0Distance` → `LOD1Distance`)
  - Increases density sampling factor (`DensitySlider` *= 0.8f)
  - Shows notification: "Auto-adjusting visual density to maintain performance"

**Logic**:
- Checks every 60 frames (1 second at 60fps)
- If frame time > `MaxFrameTimeMs` for 3 consecutive checks:
  - Applies degradation
  - Logs notification
- If performance recovers (frame time < `MaxFrameTimeMs * 0.8f` for 10 seconds):
  - Gradually restores settings (optional, or keep degraded)

---

## Phase 3 Completion Note

**What was done**:
- Biome & climate data hookup: `BiomePresentationData` component and hook system
- Biome/ground visualization: Hybrid approach (GroundTiles + overlay chunks)
- Vegetation presentation: LOD strategy with biome reactivity
- Aggregate history sampling: Village and region history buffers
- Trend visualization: Village icons, region color shifts, inspection UI integration
- PureDOTS time controls: Integration with `TimeControlCommand` for pause/speed/rewind
- Region miracles: `BlessRegion`, `CurseRegion`, `RestoreBiome` types
- Aggregate-focused miracles: Enhanced village-level effects with long-term visual shifts
- Scenario presets: `ScenarioPreset` ScriptableObject with preset types
- Scenario preset selector: UI for selecting and applying presets
- Player-facing HUD: Scenario name, stats, world health indicator
- Safe config presets: Showcase and Scale Stress modes
- Graceful degradation: Auto-performance adjustment system (placeholder)

**What was deferred**:
- Full biome overlay chunk implementation (spatial partitioning, decal spawning)
- Editor authoring tools (VillagePlacementAuthoring, ResourceNodePlacementAuthoring, BiomeRegionAuthoring)
- Full vegetation PlantStand component integration (requires PureDOTS vegetation system)
- Miracle telemetry system (before/after metrics tracking)
- Auto-performance adjustment system (full implementation)
- Player HUD MonoBehaviour (UI setup)
- Tests for Phase 3 systems

**Known limitations**:
- Biome overlay system is a placeholder - full implementation requires spatial partitioning
- Vegetation system uses placeholder values until PlantStand components are available
- Material property updates require shader setup (not implemented in code)
- Time control input requires InputActions asset setup
- Scenario preset application requires scene setup (finding authoring components)

**Suggested directions for Phase 4**:
- Advanced biome transitions (seasons, climate change)
- Multi-camera views (split-screen, picture-in-picture)
- Replay/recording system for demo playback
- Advanced aggregate AI presentation (band movements, trade routes)
- Full editor authoring tools with scene placement
- Complete vegetation system integration with PureDOTS PlantStand components

---

**End of Plan**

