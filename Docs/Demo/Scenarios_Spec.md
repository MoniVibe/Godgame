# Scenarios Specification

**Date:** 2025-01-XX  
**Status:** Specification  
**Purpose:** Scenario format and known-good scenarios for Godgame demo

---

## Overview

Scenarios are JSON files that define demo content: entity counts, initial conditions, and scripted events. They drive deterministic demo runs without hardcoded scene logic.

---

## Scenario Format

### JSON Structure

```json
{
  "name": "villager_loop_small",
  "description": "10 villagers gathering resources",
  "seed": 12345,
  "entities": {
    "villagers": 10,
    "storehouses": 1,
    "resource_nodes": 2,
    "construction_ghosts": 0
  },
  "resources": {
    "wood": 0,
    "stone": 0,
    "food": 0
  },
  "scripted_events": [
    {
      "tick": 100,
      "type": "spawn_ghost",
      "position": [10, 0, 10],
      "cost": 100
    }
  ]
}
```

### Fields

**Metadata:**
- `name` - Scenario identifier
- `description` - Human-readable description
- `seed` - Random seed for determinism

**Entities:**
- `villagers` - Number of villagers to spawn
- `storehouses` - Number of storehouses to spawn
- `resource_nodes` - Number of resource nodes to spawn
- `construction_ghosts` - Initial construction ghosts

**Resources:**
- `wood` - Initial wood count
- `stone` - Initial stone count
- `food` - Initial food count

**Scripted Events:**
- `tick` - Tick when event occurs
- `type` - Event type (spawn_ghost, trigger_miracle, etc.)
- Additional fields per event type

---

## Known-Good Scenarios

### villager_loop_small.json

**Purpose:** Demonstrates villager autonomous behavior loop

**Configuration:**
- 10 villagers
- 1 storehouse
- 2 resource nodes (wood, stone)

**What It Shows:**
- Villagers autonomously gather resources
- Resource conservation (gathered == delivered)
- Job state transitions
- Storehouse inventory accumulation

**Expected Duration:** 30-60 seconds

**Acceptance:**
- All villagers complete gather→deliver cycle
- Conservation counter shows `gathered == delivered`
- No resources lost

---

### construction_ghost.json

**Purpose:** Demonstrates construction system

**Configuration:**
- 1 construction ghost
- Cost: 100 resources
- 1 storehouse (with sufficient resources)

**What It Shows:**
- Ghost placement
- Resource withdrawal from storehouse
- Construction progress
- Completion effect and telemetry

**Expected Duration:** 10-20 seconds

**Acceptance:**
- Tickets correctly withdrawn
- Completion triggers effect
- Telemetry increments

---

### time_rewind_smoke.json

**Purpose:** Demonstrates time control and rewind determinism

**Configuration:**
- Small world (5 villagers, 1 storehouse, 1 node)
- Scripted input events for demo

**What It Shows:**
- Time controls (pause, speed, step)
- Rewind functionality
- Determinism validation

**Scripted Events:**
- Record 5 seconds
- Rewind 2 seconds
- Resimulate and validate

**Expected Duration:** 10-15 seconds

**Acceptance:**
- Rewind produces same totals
- Log "deterministic OK"
- Byte-equal validation passes

---

## Scenario Location

### Directory Structure

```
Assets/
└── Scenarios/
    └── Godgame/
        ├── villager_loop_small.json
        ├── construction_ghost.json
        └── time_rewind_smoke.json
```

### Loading

**Path Resolution:**
- Relative to `Assets/Scenarios/<game>/`
- Example: `"villager_loop_small.json"` → `Assets/Scenarios/Godgame/villager_loop_small.json`

**CLI Usage:**
```bash
--scenario=villager_loop_small.json
```

---

## Scenario Discovery

### UI Discovery

**DemoBootstrap UI:**
1. Scan `Assets/Scenarios/Godgame/` for `.json` files
2. Parse scenario metadata (name, description)
3. Display in scenario selection dropdown

### Metadata Parsing

**Required Fields:**
- `name` - Display name
- `description` - Tooltip/description text

**Optional Fields:**
- `thumbnail` - Preview image path
- `duration` - Expected duration in seconds
- `difficulty` - Difficulty rating

---

## Scenario Validation

### Preflight Validation

**Checks:**
1. JSON syntax valid
2. Required fields present
3. Entity counts within limits
4. Resource values valid
5. Scripted events valid

### Runtime Validation

**Checks:**
1. Scenario file exists
2. Can parse JSON
3. Entity spawns succeed
4. No missing references

---

## Extending Scenarios

### Adding New Scenarios

**Steps:**
1. Create JSON file in `Assets/Scenarios/Godgame/`
2. Define entity counts and configuration
3. Add scripted events (if needed)
4. Test scenario determinism
5. Document in this spec

### Event Types

**Supported Events:**
- `spawn_ghost` - Spawn construction ghost
- `trigger_miracle` - Trigger miracle effect
- `spawn_villager` - Spawn additional villager
- `set_resource` - Set resource count

**Event Format:**
```json
{
  "tick": 100,
  "type": "spawn_ghost",
  "position": [10, 0, 10],
  "cost": 100,
  "building_type": "storehouse"
}
```

---

## Related Documentation

- `DemoBootstrap_Spec.md` - Scenario loading
- `Godgame_Slices.md` - Demo slices (scenario usage)
- `Preflight_Checklist.md` - Scenario validation

