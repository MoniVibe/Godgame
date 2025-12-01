# Godgame Demo Showcase Guide

**Date:** 2025-11-27  
**Purpose:** Step-by-step guide to create and showcase a demo of Godgame's current progress

---

## Overview

This guide walks through creating a demo that showcases the implemented systems in Godgame. The project has a solid foundation with many core systems functional and ready to demonstrate.

### What's Ready to Showcase

✅ **Core Systems (Fully Functional)**
- Registry Bridge: All registries (Villager, Storehouse, Band, Miracle, Spawner, Logistics, Construction) sync to PureDOTS
- Time Controls: Pause/speed/step/rewind with determinism
- Construction: Ghost placement → payment → completion flow
- Module Refit: Parallel maintenance with resource gating
- Resource Systems: Storehouse API, aggregate piles, overflow handling
- Weather/Biomes: Climate integration, presentation bindings
- Miracles: Input/release with registry sync
- Villager AI: Job loop (Idle → Navigate → Gather → Deliver), needs system, personality traits
- Focus System: Combat resource management with abilities
- Status Effects: 50+ effect types with tick/expire/modifiers
- Prayer Power: Villager worship accumulation
- Building Durability: Decay/fire systems
- Entity Relations: Relation tracking and meeting system
- AI Behavior: Initiative, mood, patriotism, daily routines
- Progression: XP allocation, skill unlocks

✅ **Demo Infrastructure**
- `GodgameDemoBootstrapSystem`: Spawns villagers, storehouses, resource nodes
- `GodgameDemoBootstrapAuthoring`: Scene configuration component
- Demo scenes exist in `Assets/Scenes/`

---

## Step 1: Set Up Demo Scene

### Option A: Use Existing Demo Scene

1. Open Unity Editor
2. Navigate to `Assets/Scenes/Godgame_DemoScene.unity`
3. Open the scene

### Option B: Create New Demo Scene

1. **Create Scene:**
   - `File → New Scene`
   - Choose `Basic (Built-in)` or `URP` template
   - Save as `Assets/Scenes/Godgame_Showcase.unity`

2. **Add Bootstrap Configuration:**
   - Create empty GameObject: `GameObject → Create Empty`
   - Rename to `DemoBootstrap`
   - Add component: `GodgameDemoBootstrapAuthoring`
   - Configure in Inspector:
     - **Villager Prefab**: Assign a villager prefab (or create placeholder)
     - **Storehouse Prefab**: Assign a storehouse prefab (or create placeholder)
     - **Initial Villager Count**: 10-20
     - **Resource Node Count**: 6-10
     - **Villager Spawn Radius**: 15
     - **Resource Node Radius**: 20
     - **Seed**: 0 (random) or specific value for determinism
     - **Behavior Randomization Range**: 60 (good variety)

3. **Add Camera:**
   - Position camera to view the spawn area (center at origin)
   - Recommended: Top-down or angled view
   - Add camera controls if desired (WASD pan, mouse orbit)

4. **Add Lighting:**
   - Add Directional Light
   - Configure for your render pipeline (URP/HDRP/Built-in)

---

## Step 2: Create/Configure Prefabs

### Villager Prefab

If you don't have a villager prefab yet:

1. **Create Base Prefab:**
   - `GameObject → Create Empty` → Rename to `Villager`
   - Add `LocalTransform` (automatic in DOTS)
   - Add visual representation:
     - Option A: `GameObject → 3D Object → Capsule` (child object)
     - Option B: Use Entities Graphics (MeshRenderer + Material)
   - Add `VillagerAuthoring` component (if exists)
   - Save as prefab: `Assets/Prefabs/Villager.prefab`

2. **Required Components (Baked):**
   - `LocalTransform`
   - `VillagerNeeds` (health, energy, morale)
   - `VillagerJob` (job type, phase, productivity)
   - `VillagerAIState` (current state, goal)
   - `VillagerFlags` (isIdle, isWorking)
   - `VillagerAvailability`
   - `VillagerBehavior` (personality traits)
   - `VillagerCombatBehavior` (derived from behavior)
   - `VillagerGrudge` buffer (empty)

### Storehouse Prefab

1. **Create Base Prefab:**
   - `GameObject → Create Empty` → Rename to `Storehouse`
   - Add visual: `GameObject → 3D Object → Cube` (scale to building size)
   - Add `StorehouseAuthoring` component (if exists)
   - Add collider for intake (if using hand system)
   - Save as prefab: `Assets/Prefabs/Storehouse.prefab`

2. **Required Components:**
   - `LocalTransform`
   - `StorehouseInventory` (resource capacities)
   - `StorehouseIntakeAuthoring` (if using hand system)

### Resource Node (Runtime Spawned)

Resource nodes are spawned at runtime by `GodgameDemoBootstrapSystem`, so you don't need a prefab. The system creates entities with:
- `LocalTransform`
- `GodgameDemoResourceNode` (position, resource type, capacity)

---

## Step 3: Configure Systems

### Verify System Groups

The demo bootstrap system runs in `InitializationSystemGroup`, so it executes early. Verify these systems are enabled:

- ✅ `GodgameRegistryBridgeSystem` (SimulationSystemGroup)
- ✅ `GodgameVillagerSyncSystem` (SimulationSystemGroup)
- ✅ `GodgameStorehouseSyncSystem` (SimulationSystemGroup)
- ✅ `VillagerJobSystem` (if exists)
- ✅ `TimeControlSystem` (if using time controls)

### Check PureDOTS Bootstrap

Ensure PureDOTS systems are initialized:
- `CoreSingletonBootstrapSystem` should run first
- Registry singletons should be created
- Time spine should be initialized

---

## Step 4: What to Showcase

### Core Gameplay Loop

1. **Villager AI:**
   - Villagers spawn with randomized personality traits
   - They autonomously navigate to resource nodes
   - They gather resources
   - They deliver to storehouses
   - Show needs system (health, energy, morale decay/regeneration)
   - Show job state transitions (Idle → Navigate → Gather → Deliver)

2. **Resource Management:**
   - Resource nodes spawn in a circle around center
   - Villagers gather from nodes
   - Storehouse accumulates resources
   - Show aggregate piles if overflow occurs
   - Show telemetry/metrics in registry

3. **Time Controls:**
   - Pause/play
   - Speed control (1x, 2x, 4x)
   - Step forward
   - Rewind (if implemented)
   - Show determinism (rewind → resim → same state)

4. **Construction (if enabled):**
   - Place construction ghost
   - Show payment flow
   - Show completion
   - Show registry sync

5. **Miracles (if enabled):**
   - Trigger miracle
   - Show cooldown/energy
   - Show effect request
   - Show registry sync

6. **Weather/Biomes (if enabled):**
   - Show weather transitions
   - Show biome effects
   - Show presentation bindings

### Registry Visibility

Open these inspectors during play:
- **DOTS Hierarchy**: View entities and components
- **Game View**: See visual representation
- **Registry Debug**: View PureDOTS registries (if debug tools exist)

---

## Step 5: Create Showcase Script

Create a simple showcase controller to highlight features:

```csharp
// Assets/Scripts/Godgame/Demo/GodgameShowcaseController.cs
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Demo
{
    /// <summary>
    /// Simple showcase controller for demo walkthrough.
    /// Attach to a GameObject in the demo scene.
    /// </summary>
    public class GodgameShowcaseController : MonoBehaviour
    {
        [Header("Showcase Settings")]
        public bool autoAdvance = false;
        public float showcaseInterval = 10f;
        
        private float nextShowcaseTime;
        
        void Update()
        {
            if (autoAdvance && Time.time >= nextShowcaseTime)
            {
                // Cycle through showcase features
                nextShowcaseTime = Time.time + showcaseInterval;
            }
        }
        
        [ContextMenu("Show Villager AI")]
        public void ShowVillagerAI()
        {
            Debug.Log("=== Villager AI Showcase ===");
            Debug.Log("Villagers are autonomously gathering resources and delivering to storehouses.");
            Debug.Log("Check DOTS Hierarchy to see job state transitions.");
        }
        
        [ContextMenu("Show Resource Management")]
        public void ShowResourceManagement()
        {
            Debug.Log("=== Resource Management Showcase ===");
            Debug.Log("Resources flow from nodes → villagers → storehouses.");
            Debug.Log("Check registry telemetry for resource counts.");
        }
        
        [ContextMenu("Show Time Controls")]
        public void ShowTimeControls()
        {
            Debug.Log("=== Time Controls Showcase ===");
            Debug.Log("Use time controls to pause/speed/rewind the simulation.");
        }
    }
}
```

---

## Step 6: Recording/Streaming Setup

### For Video Recording

1. **Camera Setup:**
   - Use Cinemachine or manual camera controller
   - Set up smooth follow/orbit
   - Lock frame rate for consistent recording

2. **UI Overlays:**
   - Show registry counts
   - Show time state (tick, speed)
   - Show villager counts
   - Show resource totals

3. **Highlight Features:**
   - Use Unity's selection highlighting
   - Add debug text overlays
   - Use gizmos to show system boundaries

### For Live Demo

1. **Prepare Talking Points:**
   - "This is a Unity DOTS project using PureDOTS shared package"
   - "Villagers have AI with personality traits and autonomous behavior"
   - "All systems are Burst-compiled for performance"
   - "Registry system provides single source of truth"
   - "Time controls allow deterministic rewinding"

2. **Interactive Features:**
   - Pause and inspect entities
   - Show component data in DOTS Hierarchy
   - Demonstrate time controls
   - Show registry telemetry

---

## Step 7: Polish & Enhancement

### Visual Polish (Optional)

1. **Replace Placeholders:**
   - Swap primitive meshes for art assets
   - Add materials/textures
   - Add particle effects for gathering/delivery
   - Add UI for registry display

2. **Camera Controls:**
   - Add WASD pan
   - Add mouse orbit
   - Add zoom
   - Add smooth follow

3. **Debug Visualization:**
   - Draw villager paths
   - Draw resource node ranges
   - Draw storehouse influence
   - Draw registry connections

### Feature Additions (Future)

1. **Hand System:**
   - Right-click to pick up villagers/resources
   - Drag and drop
   - Slingshot throw

2. **Construction:**
   - Click to place buildings
   - Show ghost preview
   - Show resource costs

3. **Miracles:**
   - UI for miracle selection
   - Visual effects
   - Cooldown indicators

---

## Step 8: Testing the Demo

### Pre-Demo Checklist

- [ ] Scene opens without errors
- [ ] Bootstrap system spawns entities
- [ ] Villagers navigate and gather
- [ ] Storehouses receive resources
- [ ] Registry syncs correctly
- [ ] Time controls work
- [ ] No console errors/warnings
- [ ] Performance is acceptable (60+ FPS with 10-20 villagers)

### Validation Tests

Run these tests to verify functionality:

```bash
# Run registry bridge tests
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform editmode -testResults Logs/editmode-tests.xml -testFilter GodgameRegistryBridgeSystemTests

# Run villager job tests
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform playmode -testResults Logs/playmode-tests.xml -testFilter Conservation_VillagerGatherDeliver_Playmode

# Run time determinism tests
Unity -projectPath "$(pwd)" -batchmode -quit -runTests -testPlatform playmode -testResults Logs/playmode-tests.xml -testFilter Time_RewindDeterminism_Playmode
```

---

## Step 9: Documentation

### Create Demo README

Create `Assets/Scenes/Godgame_Showcase_README.md`:

```markdown
# Godgame Showcase Demo

## Quick Start
1. Open this scene
2. Press Play
3. Watch villagers gather resources

## Controls
- WASD: Pan camera (if implemented)
- Mouse: Orbit camera (if implemented)
- Space: Pause/Play (if time controls enabled)

## Features Demonstrated
- Villager AI with personality traits
- Resource gathering and delivery
- Storehouse inventory management
- Registry synchronization
- Time controls (pause/speed/rewind)

## Systems
- GodgameDemoBootstrapSystem: Spawns demo entities
- GodgameRegistryBridgeSystem: Syncs to PureDOTS registries
- VillagerJobSystem: Handles villager job loop
- TimeControlSystem: Manages time state
```

---

## Troubleshooting

### Common Issues

1. **No Entities Spawn:**
   - Check `GodgameDemoBootstrapAuthoring` is in scene
   - Verify prefabs are assigned
   - Check console for errors
   - Verify PureDOTS bootstrap ran

2. **Villagers Don't Move:**
   - Check `VillagerJobSystem` is enabled
   - Verify navigation system (if using)
   - Check `LocalTransform` components
   - Verify job state transitions

3. **Registry Not Syncing:**
   - Check `GodgameRegistryBridgeSystem` is enabled
   - Verify PureDOTS package is linked
   - Check registry singletons exist
   - Look for sync system errors

4. **Performance Issues:**
   - Reduce villager count
   - Check Burst compilation
   - Profile with Unity Profiler
   - Verify systems are parallelized

---

## Next Steps

After creating the demo:

1. **Record Video:**
   - Showcase key features
   - Highlight architecture
   - Demonstrate performance

2. **Share Progress:**
   - Update `Docs/Progress.md`
   - Create demo status document
   - Share with team/stakeholders

3. **Iterate:**
   - Gather feedback
   - Add requested features
   - Polish visuals
   - Expand gameplay

---

## Related Documentation

- `Docs/Project_Orientation.md` - Project overview
- `Docs/Progress.md` - Current progress tracking
- `Docs/TODO/Phase2_Demo_TODO.md` - Phase 2 demo tasks
- `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Integration status
- `Assets/Scripts/Godgame/Demo/` - Demo system code

---

**Ready to showcase!** The foundation is solid, and you have many working systems to demonstrate. Focus on the core gameplay loop (villager AI → resource gathering → storehouse delivery) and highlight the architecture (PureDOTS integration, Burst compilation, registry system).

