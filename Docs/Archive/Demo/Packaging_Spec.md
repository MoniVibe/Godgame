# Packaging Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Demo build packaging requirements and structure

---

## Overview

Demo builds are packaged as self-contained zip archives containing the executable, scenarios, bindings, documentation, and reports. Each demo build is isolated and can run independently.

---

## Package Output

### Zip File

**Naming:** `Godgame_Demo_<date>.zip`

**Date Format:** `YYYY-MM-DD`

**Example:** `Godgame_Demo_2025-01-15.zip`

**Location:** `Builds/` directory (or specified output path)

---

## Package Contents

### Required Files

**Executable:**
- `Godgame_Demo.exe` (Windows)
- Platform-specific executable for target platform

**Scenarios:**
- `Scenarios/Godgame/` folder
- All scenario JSON files
- Scenario metadata files

**Bindings:**
- `Bindings/Minimal.asset`
- `Bindings/Fancy.asset`
- Binding documentation (optional)

**Documentation:**
- `Readme.md` - User-facing documentation
- Hotkeys reference
- Known limits and issues

**Reports:**
- `Reports/last_run.json` - Last demo run metrics
- Optional: Previous run reports

### Directory Structure

```
Godgame_Demo_2025-01-15.zip
├── Godgame_Demo.exe
├── Scenarios/
│   └── Godgame/
│       ├── villager_loop_small.json
│       ├── construction_ghost.json
│       └── time_rewind_smoke.json
├── Bindings/
│   ├── Minimal.asset
│   └── Fancy.asset
├── Readme.md
└── Reports/
    └── last_run.json
```

---

## Readme.md Contents

### Required Sections

**Quick Start:**
- How to run the demo
- System requirements
- Controls overview

**Hotkeys:**
- Complete hotkey reference
- Time controls
- Demo controls

**Known Limits:**
- Performance limitations
- Known issues
- Workarounds

**Scenarios:**
- Available scenarios
- How to run scenarios
- Scenario descriptions

### Example Readme

```markdown
# Godgame Demo

## Quick Start

1. Run `Godgame_Demo.exe`
2. Select a scenario from the menu
3. Use hotkeys to control the simulation

## System Requirements

- Windows 10/11
- DirectX 11 compatible GPU
- 4GB RAM minimum

## Hotkeys

### Time Controls
- **P** - Pause/Play
- **[** - Step back
- **]** - Step forward
- **1/2/3** - Speed ×0.5/×1/×2
- **R** - Rewind

### Demo Controls
- **G** - Spawn construction ghost
- **B** - Swap Minimal/Fancy bindings

## Known Limits

- Maximum 1000 villagers for stable performance
- Rewind limited to 10 seconds of history
- Some scenarios may not work at 120Hz

## Scenarios

- `villager_loop_small` - 10 villagers gathering resources
- `construction_ghost` - Construction demonstration
- `time_rewind_smoke` - Time control demonstration

## Support

For issues or questions, see the project repository.
```

---

## Optional Components

### Launcher (Optional)

**Purpose:** Small launcher application that lists scenarios and loads them

**Features:**
- Scenario browser
- Configuration options
- Direct scenario launch

**Implementation:**
- Separate executable or integrated into main executable
- Optional feature, not required

### Additional Reports

**Previous Runs:**
- Include previous run reports for comparison
- Optional, can be excluded to reduce package size

**Screenshots:**
- Include demo screenshots
- Optional, can be excluded to reduce package size

---

## Packaging Process

### Build Pipeline

**Step 1: Build Executable**
```bash
Unity -projectPath "$(pwd)" -batchmode -quit -buildWindows64Player Builds/Godgame_Demo.exe
```

**Step 2: Copy Assets**
- Copy `Scenarios/Godgame/` to package
- Copy `Bindings/` to package
- Copy `Readme.md` to package

**Step 3: Generate Reports**
- Run demo with default scenario
- Copy `Reports/last_run.json` to package

**Step 4: Create Zip**
```bash
zip -r Godgame_Demo_2025-01-15.zip Builds/Godgame_Demo.exe Scenarios/ Bindings/ Readme.md Reports/
```

### Automation

**CI/CD Integration:**
- Automated packaging in build pipeline
- Version number from git tag or date
- Upload to distribution location

---

## Package Size Considerations

### Size Targets

**Target:** < 500MB (uncompressed)

**Optimization:**
- Exclude unnecessary assets
- Compress textures and meshes
- Exclude development tools
- Exclude test assemblies (unless `GODGAME_TESTS` enabled)

### Compression

**Zip Compression:** Standard zip compression
**Alternative:** 7z for better compression (optional)

---

## Distribution

### Distribution Channels

**Internal:**
- Shared network drive
- Internal file server
- CI/CD artifact storage

**External:**
- File hosting service
- Cloud storage
- Direct download link

### Versioning

**Version Format:** `YYYY-MM-DD` or semantic version

**Example:** `Godgame_Demo_2025-01-15.zip` or `Godgame_Demo_v1.0.0.zip`

---

## Related Documentation

- `Demo_Build_Spec.md` - Build specification
- `Scenarios_Spec.md` - Scenario format
- `Bindings_Spec.md` - Binding assets
- `Instrumentation_Spec.md` - Reports format

