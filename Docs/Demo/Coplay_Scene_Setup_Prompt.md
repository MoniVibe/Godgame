# Coplay Prompt: Unity Demo Scene Setup

**Purpose:** Prompt for Coplay to set up the Unity scene for Godgame demo builds

---

## Task: Set Up Godgame Demo Scene

Create a Unity scene that serves as the demo entry point for Godgame. The scene should be data-driven, support scenario loading, and include all necessary systems and UI for demo runs.

---

## Scene Requirements

### Scene Name
- **Path:** `Assets/Scenes/Godgame_DemoScene.unity`
- **Purpose:** Main demo scene for Godgame builds

---

## Required GameObjects and Components

### 1. PureDOTS Bootstrap GameObject

**Name:** `PureDOTS Bootstrap`

**Components:**
- `PureDotsConfigAuthoring` - References `Assets/Settings/PureDotsRuntimeConfig.asset`
- `SpatialPartitionAuthoring` - References `Assets/Settings/GodgameSpatialProfile.asset`
- `GodgameSampleRegistryAuthoring` (if exists)

**Purpose:** Initializes PureDOTS systems, registries, and spatial grid

---

### 2. DemoBootstrap GameObject

**Name:** `DemoBootstrap`

**Components:**
- `DemoBootstrap` (MonoBehaviour) - Main demo controller
  - Scenario selection UI reference
  - Binding set toggle (Minimal/Fancy)
  - Time control handlers

**Purpose:** Manages demo initialization, scenario loading, and runtime toggles

**Configuration:**
- Default scenario: `villager_loop_small.json`
- Default bindings: `Minimal`
- Enable scenario discovery from `Assets/Scenarios/Godgame/`

---

### 3. Camera and Lighting

**Main Camera:**
- Position: Top-down or angled view of spawn area (center at origin)
- Clear flags: Solid color or Skybox
- Culling mask: Appropriate layers
- Add `DemoCameraController` ( `Assets/Scripts/Godgame/Demo/DemoCameraController.cs` ) to enable WASD + QE + RF movement and scroll zoom (demo/editor builds only)

**Directional Light:**
- Rotation: Appropriate for scene lighting
- Intensity: 1.0
- Color: White or warm tone

**Purpose:** Basic scene visibility

---

### 4. Demo UI Canvas (Optional but Recommended)

**Name:** `DemoUI`

**Components:**
- `Canvas` - Screen Space - Overlay
- `DemoBootstrapUI` (MonoBehaviour) - UI controller

**UI Elements:**
- Scenario selection dropdown/list
- Time controls (Pause, Step, Speed buttons)
- Binding swap button (B hotkey)
- Determinism overlay (tick, seed, snapshot usage)
- HUD panels (left: gameplay metrics, right: system metrics)

**Purpose:** User interface for demo controls and feedback

---

### 5. Event System (if using UI)

**Name:** `EventSystem`

**Components:**
- `EventSystem` (Unity UI)
- `StandaloneInputModule`

**Purpose:** Required for UI interactions

---

## Scene Structure

```
Godgame_DemoScene.unity
├── PureDOTS Bootstrap (GameObject)
│   ├── PureDotsConfigAuthoring
│   ├── SpatialPartitionAuthoring
│   └── GodgameSampleRegistryAuthoring
├── DemoBootstrap (GameObject)
│   └── DemoBootstrap (MonoBehaviour)
├── Main Camera (GameObject)
│   ├── Camera
│   └── DemoCameraController
├── Directional Light (GameObject)
│   └── Light
├── DemoUI (GameObject, optional)
│   ├── Canvas
│   └── DemoBootstrapUI (MonoBehaviour)
└── EventSystem (GameObject, if using UI)
    ├── EventSystem
    └── StandaloneInputModule
```

---

## System Requirements

### Required Systems (Auto-Enabled)

The following systems should be enabled and configured:

**Core Systems:**
- `CoreSingletonBootstrapSystem` (PureDOTS) - Seeds singletons
- `GodgameRegistryBridgeSystem` - Mirrors entities to registries
- `VillagerJobSystem` - Handles villager gather/deliver loop
- `ConstructionSystem` - Processes construction payment/completion
- `TimeControlSystem` - Handles time controls (pause/speed/rewind)

**Optional Systems:**
- `GodgamePresentationBindingBootstrapSystem` - Seeds placeholder effect IDs
- `PresentationSystem` (PureDOTS) - Processes presentation commands

**System Groups:**
- All gameplay systems run in `FixedStepSimulationSystemGroup` for determinism

---

## Configuration Assets

### Required Assets

**PureDOTS Config:**
- `Assets/Settings/PureDotsRuntimeConfig.asset` - Runtime configuration
- `Assets/Settings/GodgameSpatialProfile.asset` - Spatial partition profile

**Scenarios:**
- `Assets/Scenarios/Godgame/villager_loop_small.json` (or other scenarios)
- Scenario discovery scans `Assets/Scenarios/Godgame/*.json`

**Bindings:**
- `Assets/Resources/Bindings/Minimal.asset` - Minimal presentation bindings
- `Assets/Resources/Bindings/Fancy.asset` - Fancy presentation bindings

---

## DemoBootstrap Component Configuration

### DemoOptions Defaults

```csharp
ScenarioPath: "Scenarios/Godgame/villager_loop_small.json"
BindingsSet: 0  // Minimal
Veteran: 0      // Off (for Space4X compatibility)
```

### UI References

- Scenario dropdown/list: References `Assets/Scenarios/Godgame/` folder
- Binding toggle: Swaps between Minimal (0) and Fancy (1)
- Time controls: Maps to `TimeControlInput` singleton

---

## Hotkey Setup

### Input Mapping

**Time Controls:**
- **P** → Pause/Play toggle
- **[** → Step back (single tick)
- **]** → Step forward (single tick)
- **1/2/3** → Speed ×0.5/×1/×2
- **R** → Trigger rewind sequence

**Demo Controls:**
- **G** → Spawn construction ghost
- **B** → Swap Minimal/Fancy binding

**Implementation:**
- Map to `TimeControlInput` singleton for time controls
- Map to `DemoBootstrap` for demo-specific actions
- Emit HUD events for feedback

---

## HUD Layout Requirements

### Left Side (Gameplay Metrics)

**Villagers:**
- Active villager count
- Idle/Working breakdown
- Job state distribution

**Jobs:**
- Active jobs count
- Job type breakdown
- Completion rate

**Storehouse:**
- Inventory totals per resource
- Capacity usage
- Overflow status

**Build Progress:**
- Active construction sites
- Progress per site
- Completion count

### Right Side (System Metrics)

**Tick:**
- Current simulation tick
- Tick rate (ticks/second)

**FPS:**
- Frame rate
- Target FPS

**Fixed Tick ms:**
- Fixed step duration
- Budget vs actual

**Snapshot Bytes:**
- Snapshot ring usage
- Memory budget

**ECB Playback ms:**
- EntityCommandBuffer playback time
- Structural change cost

---

## Acceptance Criteria

### Scene Setup

1. **Scene Opens:** Scene loads without errors
2. **Systems Enabled:** All required systems are enabled and configured
3. **Bootstrap Works:** DemoBootstrap initializes correctly
4. **Scenario Loading:** Can load scenarios from `Assets/Scenarios/Godgame/`
5. **UI Functional:** All UI elements work (if UI is included)

### Runtime Validation

1. **Entities Spawn:** ScenarioRunner spawns entities correctly
2. **Systems Run:** All gameplay systems execute without errors
3. **Hotkeys Work:** All hotkeys function correctly
4. **HUD Updates:** HUD displays accurate metrics
5. **Binding Swap:** Can swap Minimal ↔ Fancy bindings live

---

## Implementation Notes

### Data-Driven Approach

- **No Hardcoded Logic:** Scene should not contain hardcoded entity spawns
- **Scenario-Driven:** All content loaded from scenario JSON files
- **Binding-Driven:** Visuals determined by binding assets, not code

### Determinism

- **Fixed Step:** All systems run in `FixedStepSimulationSystemGroup`
- **Seed Control:** Scenarios include seed for deterministic runs
- **Rewind Support:** Scene supports time rewind functionality

### Headless Support

- **Optional Presentation:** Scene should run without PresentationBridge
- **Metrics Still Work:** Counters and telemetry work in headless mode
- **CLI Compatible:** Scene can be launched via CLI with scenario parameters

---

## Related Documentation

- `Docs/Demo/DemoBootstrap_Spec.md` - DemoBootstrap system design
- `Docs/Demo/Godgame_Slices.md` - Demo slices and features
- `Docs/Demo/Bindings_Spec.md` - Presentation binding system
- `Docs/Demo/Scenarios_Spec.md` - Scenario format
- `Docs/Demo/Demo_Build_Spec.md` - Master build specification

---

## Next Steps After Scene Setup

1. **Test Scenario Loading:** Load `villager_loop_small.json` and verify entities spawn
2. **Test Hotkeys:** Verify all hotkeys work correctly
3. **Test Binding Swap:** Swap Minimal ↔ Fancy and verify no errors
4. **Test Headless:** Remove PresentationBridge and verify sim still runs
5. **Validate Determinism:** Run scenario twice with same seed, verify identical results

---

## Troubleshooting

### Common Issues

**No Entities Spawn:**
- Check DemoBootstrap configuration
- Verify scenario file exists and is valid JSON
- Check ScenarioRunner system is enabled

**Systems Not Running:**
- Verify PureDOTS Bootstrap initialized
- Check system groups are configured correctly
- Review console for errors

**UI Not Working:**
- Verify EventSystem exists
- Check Canvas render mode
- Verify UI component references

**Hotkeys Not Responding:**
- Check Input System configuration
- Verify hotkey mappings in DemoBootstrap
- Check TimeControlInput singleton exists

---

**Ready to set up the scene!** Follow this prompt to create a complete, data-driven demo scene that supports all demo features.

