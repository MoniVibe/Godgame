# Demo Scene & Content Pipeline Implementation Guide

**Date:** 2025-01-24  
**Status:** Implementation Plan  
**Purpose:** Guide for creating self-contained demo scene with proper content pipeline

---

## Current State Assessment

### Scenes

**Implemented:**
- `GodgameBootstrapSubScene.unity`: Bootstrap scene (exists but minimal content)
- `GodgameRegistrySubScene.unity`: Registry SubScene (wizard creates it)
- `GodgameRegistrySubSceneWizard.cs`: Editor wizard for creating registry SubScene

**Gaps:**
- No complete demo scene with all required entities
- SubScene wizard creates config but not actual entities
- No demo spawn/bootstrap system

### Prefabs

**Implemented:**
- `Assets/Prefabs/Buildings/`: Basic storehouse, temple, fertility statue
- `Assets/Prefabs/Individuals/`: Basic villager prefab
- `Assets/Placeholders/`: Placeholder prefabs (buildings, modules, resource nodes, villagers, wagons)

**Gaps:**
- Placeholder prefabs not wired to final art
- No prefab authoring components for demo entities
- Missing prefab variants (different villager types, building styles)

### Authoring Assets

**Implemented:**
- `PureDotsRuntimeConfig.asset`: Runtime configuration
- `ResourceRecipeCatalog.asset`: Resource recipe catalog
- `GodgameSpatialProfile.asset`: Spatial partition profile
- `PureDotsResourceTypes.asset`: Resource type catalog

**Gaps:**
- May need updates for demo-specific content
- Missing demo-specific ScriptableObjects

---

## Implementation Plan

### 1. Demo Bootstrap System

**Purpose:** Spawn demo entities (villagers, storehouses, resource nodes) at scene start

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Scenario/DemoBootstrapComponents.cs
public struct DemoBootstrapConfig : IComponentData
{
    public int VillagerCount;
    public int StorehouseCount;
    public int ResourceNodeCount;
    public int BandCount;
    public float3 SpawnCenter;
    public float SpawnRadius;
    public uint SpawnTick;
}

public struct DemoEntitySpawned : IComponentData
{
    public EntityType Type;
    public Entity SpawnedEntity;
}

public enum EntityType : byte
{
    Villager = 0,
    Storehouse = 1,
    ResourceNode = 2,
    Band = 3
}
```

**System to Create:**
- `GodgameDemoBootstrapSystem.cs`: Reads bootstrap config, spawns entities
- Uses prefab references or entity archetypes
- Positions entities within spawn radius
- Tags entities for demo tracking

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Scenario/DemoBootstrapComponents.cs`
- New: `Assets/Scripts/Godgame/Scenario/GodgameDemoBootstrapSystem.cs`
- Modify: `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnLoggerSystem.cs` (integrate with bootstrap)
- New: `Assets/Scripts/Godgame/Tests/DemoBootstrapTests.cs`

**Authoring Component:**
- `DemoBootstrapAuthoring.cs`: MonoBehaviour authoring for demo config
- Attach to GameObject in demo scene
- Configure spawn counts, center, radius

---

### 2. Demo Scene Creation

**Purpose:** Create self-contained demo scene with all required systems and entities

**Scene Structure:**
```
GodgameDemoScene.unity
├── PureDOTS Bootstrap (GameObject)
│   ├── PureDotsConfigAuthoring (references PureDotsRuntimeConfig.asset)
│   ├── SpatialPartitionAuthoring (references GodgameSpatialProfile.asset)
│   └── GodgameSampleRegistryAuthoring
├── Demo Bootstrap (GameObject)
│   └── DemoBootstrapAuthoring (configures spawn counts)
├── Camera & Lighting
│   ├── Main Camera
│   └── Directional Light
├── Interaction Rig (GameObject)
│   ├── DivineHandAuthoring
│   ├── CameraControllerAuthoring
│   └── InputBridge components
└── _Authoring SubScene (GodgameDemoContentSubScene.unity)
    ├── Ground/Environment
    ├── Resource Nodes (pre-authored)
    ├── Storehouses (pre-authored)
    └── Initial Villagers (pre-authored, optional)
```

**Files to Touch:**
- New: `Assets/Scenes/GodgameDemoScene.unity`
- New: `Assets/Scenes/GodgameDemoContentSubScene.unity`
- Modify: `Assets/Scripts/Godgame/Editor/GodgameRegistrySubSceneWizard.cs` (add demo scene creation option)

**Editor Wizard Enhancement:**
- Add menu item: `Tools/Godgame/Create Demo Scene...`
- Creates demo scene with all required GameObjects
- Creates SubScene with demo content
- Configures all authoring components

---

### 3. Prefab Authoring Pipeline

**Purpose:** Create authoring components for demo prefabs

**Authoring Components to Create:**
- `VillagerPrefabAuthoring.cs`: Already exists, enhance for demo variants
- `StorehousePrefabAuthoring.cs`: Already exists, enhance for demo variants
- `ResourceNodePrefabAuthoring.cs`: Already exists, enhance for demo variants
- `BandPrefabAuthoring.cs`: Create new for band prefabs

**Prefab Variants to Create:**
- `Villager_Basic.prefab`: Basic villager (already exists)
- `Villager_Bold.prefab`: Bold personality variant
- `Villager_Craven.prefab`: Craven personality variant
- `Storehouse_Basic.prefab`: Basic storehouse (already exists)
- `Storehouse_Large.prefab`: Large capacity variant
- `ResourceNode_Wood.prefab`: Wood resource node
- `ResourceNode_Stone.prefab`: Stone resource node
- `ResourceNode_Food.prefab`: Food resource node

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Authoring/VillagerPrefabAuthoring.cs` (enhance for variants)
- Modify: `Assets/Scripts/Godgame/Authoring/StorehouseAuthoring.cs` (enhance for variants)
- Modify: `Assets/Scripts/Godgame/Authoring/ResourceNodeAuthoring.cs` (enhance for variants)
- New: `Assets/Scripts/Godgame/Authoring/BandPrefabAuthoring.cs`
- Create prefab variants in `Assets/Prefabs/`

---

### 4. COZY Weather Integration

**Purpose:** Integrate COZY Weather assets with biome system

**Weather Assets to Create:**
- `WeatherProfile_Temperate.asset`: Temperate biome weather
- `WeatherProfile_Grasslands.asset`: Grasslands biome weather
- `WeatherProfile_Mountains.asset`: Mountains biome weather
- Ambient loops per biome
- Special-FX prefabs per biome

**Integration Points:**
- Wire weather profiles to `BiomeDefinitionAuthoring`
- Wire ambient loops to `WeatherControllerSystem`
- Wire special-FX prefabs to `WeatherPresentationSystem`

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Environment/Systems/WeatherControllerSystem.cs` (wire COZY assets)
- Modify: `Assets/Scripts/Godgame/Environment/Systems/WeatherPresentationSystem.cs` (wire FX prefabs)
- Create weather assets in `Assets/Settings/Weather/`

**Note:** COZY Weather integration is visual polish, not required for basic demo functionality

---

### 5. Interaction Rig Setup

**Purpose:** Set up hand/camera interaction rigs in demo scene

**Components Required:**
- `DivineHandAuthoring`: Hand interaction system
- `CameraControllerAuthoring`: Camera control system
- `InputBridge` components: Input routing to DOTS

**Setup Steps:**
1. Create Interaction Rig GameObject in demo scene
2. Add `DivineHandAuthoring` component
3. Add `CameraControllerAuthoring` component
4. Configure input bindings
5. Wire to DOTS systems

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Authoring/DivineHandAuthoring.cs` (ensure demo-ready)
- Modify: `Assets/Scripts/Godgame/Authoring/CameraControllerAuthoring.cs` (ensure demo-ready)
- Update demo scene with interaction rig setup

---

### 6. Authoring Assets Updates

**Purpose:** Update/create authoring assets for demo

**Assets to Update:**
- `PureDotsRuntimeConfig.asset`: Ensure demo-compatible settings
- `ResourceRecipeCatalog.asset`: Add demo recipes if needed
- `GodgameSpatialProfile.asset`: Ensure demo-compatible spatial config

**Assets to Create:**
- `DemoBootstrapConfig.asset`: Demo bootstrap configuration (optional, can use component)
- `DemoVillagerProfiles.asset`: Villager personality profiles for demo
- `DemoVillageProfiles.asset`: Village configuration profiles for demo

**Files to Touch:**
- Review: `Assets/Settings/PureDotsRuntimeConfig.asset`
- Review: `Assets/Settings/ResourceRecipeCatalog.asset`
- Review: `Assets/Settings/GodgameSpatialProfile.asset`
- Create demo-specific assets if needed

---

## Implementation Order

### Phase 1: Demo Bootstrap System (High Priority)
1. Implement demo bootstrap system
2. Create demo bootstrap authoring component
3. Add tests for bootstrap system

### Phase 2: Demo Scene Creation (High Priority)
1. Create demo scene structure
2. Create demo content SubScene
3. Enhance editor wizard for demo scene creation

### Phase 3: Prefab Pipeline (Medium Priority)
1. Enhance prefab authoring components
2. Create prefab variants
3. Wire prefabs to demo scene

### Phase 4: Interaction Rig Setup (Medium Priority)
1. Set up interaction rigs in demo scene
2. Configure input bindings
3. Test interaction in demo scene

### Phase 5: Authoring Assets (Low Priority)
1. Review/update existing assets
2. Create demo-specific assets if needed
3. Document asset usage

### Phase 6: COZY Weather (Future/Polish)
1. Create weather assets
2. Wire weather to biome system
3. Test weather in demo scene

---

## Testing Strategy

### Unit Tests
- `DemoBootstrapTests.cs`: Test bootstrap system spawning entities
- `DemoSceneValidationTests.cs`: Test demo scene structure and components

### Integration Tests
- `DemoBootstrapToRegistryTests.cs`: Test bootstrap entities syncing to registries
- `DemoSceneToSystemsTests.cs`: Test demo scene systems initializing correctly

### PlayMode Tests
- `DemoScene_FullLoop_Playmode.cs`: Test complete demo scene loop
- `DemoScene_Interaction_Playmode.cs`: Test interaction rigs in demo scene

---

## Files Summary

### New Files to Create
- `Assets/Scripts/Godgame/Scenario/DemoBootstrapComponents.cs`
- `Assets/Scripts/Godgame/Scenario/GodgameDemoBootstrapSystem.cs`
- `Assets/Scripts/Godgame/Authoring/DemoBootstrapAuthoring.cs`
- `Assets/Scripts/Godgame/Authoring/BandPrefabAuthoring.cs`
- `Assets/Scripts/Godgame/Tests/DemoBootstrapTests.cs`
- `Assets/Scripts/Godgame/Tests/DemoSceneValidationTests.cs`
- `Assets/Scenes/GodgameDemoScene.unity`
- `Assets/Scenes/GodgameDemoContentSubScene.unity`

### Files to Modify
- `Assets/Scripts/Godgame/Scenario/GodgameScenarioSpawnLoggerSystem.cs` (integrate with bootstrap)
- `Assets/Scripts/Godgame/Editor/GodgameRegistrySubSceneWizard.cs` (add demo scene creation)
- `Assets/Scripts/Godgame/Authoring/VillagerPrefabAuthoring.cs` (enhance for variants)
- `Assets/Scripts/Godgame/Authoring/StorehouseAuthoring.cs` (enhance for variants)
- `Assets/Scripts/Godgame/Authoring/ResourceNodeAuthoring.cs` (enhance for variants)

### Prefabs to Create
- `Assets/Prefabs/Individuals/Villager_Bold.prefab`
- `Assets/Prefabs/Individuals/Villager_Craven.prefab`
- `Assets/Prefabs/Buildings/Storehouse_Large.prefab`
- `Assets/Prefabs/Resources/ResourceNode_Wood.prefab`
- `Assets/Prefabs/Resources/ResourceNode_Stone.prefab`
- `Assets/Prefabs/Resources/ResourceNode_Food.prefab`

---

## Related Documentation

- `Docs/DemoReadiness_GapAnalysis.md` - Gap analysis
- `Docs/DemoSceneSetup.md` - Scene setup guide
- `Docs/DemoReadiness_Status.md` - Demo readiness status
- `Docs/AuthoringSetup.md` - Authoring setup guide

