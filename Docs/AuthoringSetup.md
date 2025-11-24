# Godgame Authoring Setup Guide

## Overview

This document describes the authoring components and setup required for Godgame entities in PureDOTS.

## Required Authoring Assets

### Core Configuration

1. **PureDotsConfigAuthoring** - Required for resource type catalog
   - Location: `Godgame/Assets/Data/Resources/`
   - Contains: Resource type definitions (Wood, Stone, Food, etc.)
   - Usage: Attach to a GameObject in the scene

2. **EnvironmentGridConfigAuthoring** (Optional)
   - For environment systems (moisture, temperature, wind, sunlight)
   - Can be omitted if environment systems aren't needed initially

### Entity Authoring Components

All authoring components are in `PureDOTS.Authoring` namespace and auto-bake to ECS.

#### Villagers
- **VillagerAuthoring** - Core villager entity
  - Fields: Identity, needs, movement, job, discipline, mood, combat
  - Creates: Full villager component set with buffers

- **VillagerSpawnerAuthoring** - Spawns villagers over time
  - Fields: Prefab reference, initial/max population, spawn radius
  - Creates: Spawn config and population tracking

#### Buildings
- **StorehouseAuthoring** - Resource storage building
  - Fields: Capacity per resource type, input/output rates
  - Creates: StorehouseConfig, capacity buffers, inventory

- **WorshipSiteAuthoring** - Mana/prayer power generation
  - Fields: Mana generation rate, influence range, worshipper capacity
  - Creates: PrayerPowerSource, WorshipSiteConfig, worshipper tracking

- **HousingAuthoring** - Rest and shelter for villagers
  - Fields: Max residents, comfort level, restoration rates
  - Creates: HousingConfig, HousingState, resident tracking

- **VillageCenterAuthoring** - Village management hub
  - Fields: Village ID, spawn config, settlement stats, residency
  - Creates: VillageId, VillageStats, VillageSpawnConfig, residency tracking

- **ConstructionSiteAuthoring** - Building construction progress
  - Fields: Resource costs, progress requirements, completion prefab
  - Creates: ConstructionSiteId, progress tracking, cost buffers

#### Resources
- **ResourceSourceAuthoring** - Gather-able resource deposits
  - Fields: Resource type, initial units, gather rate, respawn settings
  - Creates: ResourceSourceConfig, ResourceSourceState

- **ResourceChunkAuthoring** - Discrete resource chunks/piles
  - Fields: Resource type, mass, scale range, default units
  - Creates: ResourceChunkConfig

#### Vegetation
- **VegetationAuthoring** - Plants, trees, crops
  - Fields: Species type, lifecycle stage, health, production
  - Creates: Full vegetation component set with lifecycle tracking

#### Abilities (Spells & Skills)
- **SpellSpecCatalog** - Spell specification catalog
  - Fields: Array of spell definitions (ID, shape, range, cost, effects, etc.)
  - Creates: `SpellSpecCatalogBlobRef` component with blob asset reference
  - Usage: Create via `Create → Godgame → Spell Spec Catalog`, define spells, bake runs automatically

- **SkillSpecCatalog** - Skill specification catalog
  - Fields: Array of skill definitions (ID, passive/active, stat mods, prerequisites, tier)
  - Creates: `SkillSpecCatalogBlobRef` component with blob asset reference
  - Usage: Create via `Create → Godgame → Skill Spec Catalog`, define skills, validate prerequisites

- **StatusSpecCatalog** - Status effect specification catalog
  - Fields: Array of status definitions (ID, duration, period, stacks, dispel tags)
  - Creates: `StatusSpecCatalogBlobRef` component with blob asset reference
  - Usage: Create via `Create → Godgame → Status Spec Catalog`, define statuses

- **SpellBindingSet** - Presentation binding set (Minimal/Fancy)
  - Fields: Array of spell presentation bindings (FX refs, icon tokens, style tokens, sockets)
  - Usage: Create via `Create → Godgame → Spell Binding Set`, assign FX/icon references per spell

## Bootstrap Profile

The default PureDOTS bootstrap profile (`SystemRegistry.BuiltinProfiles.Default`) includes all required system groups:

- TimeSystemGroup
- EnvironmentSystemGroup  
- SpatialSystemGroup
- GameplaySystemGroup
  - AISystemGroup
  - VillagerSystemGroup
  - ResourceSystemGroup
  - ConstructionSystemGroup
  - VegetationSystemGroup
  - MiracleEffectSystemGroup
- HandSystemGroup
- CombatSystemGroup
- HistorySystemGroup

No custom profile needed - the default profile works for Godgame.

## Prefab Structure

Recommended prefab organization:

```
Godgame/Assets/Prefabs/
├── Villagers/
│   ├── Villager_Basic.prefab
│   └── Villager_Combat.prefab
├── Buildings/
│   ├── Storehouse.prefab
│   ├── WorshipSite.prefab
│   ├── Housing.prefab
│   └── VillageCenter.prefab
├── Resources/
│   ├── ResourceSource_Wood.prefab
│   ├── ResourceSource_Stone.prefab
│   └── ResourceChunk.prefab
└── Vegetation/
    ├── Tree.prefab
    ├── Shrub.prefab
    └── Crop.prefab
```

## Scene Setup Checklist

1. [ ] Add PureDotsConfigAuthoring GameObject with ResourceTypes catalog
2. [ ] Create SubScene(s) containing:
   - [ ] Villager entities (or spawners)
   - [ ] Building entities (storehouses, worship sites, housing, village centers)
   - [ ] Resource sources
   - [ ] Vegetation entities
3. [ ] Ensure all entities have appropriate authoring components
4. [ ] Verify spatial indexing is enabled (automatically added by bakers)
5. [ ] Test baking and verify entities appear in ECS world

## Spells & Skills Workflow

### Overview
Spells, skills, and status effects are defined as data (ScriptableObject catalogs) and baked into DOTS blob assets. The Prefab Maker tool provides a unified interface for managing these specs, validating them, and baking blobs.

### Workflow

1. **Create Spec Catalogs**
   - Create `SpellSpecCatalog`, `SkillSpecCatalog`, and `StatusSpecCatalog` assets
   - Define spells/skills/statuses in the inspector

2. **Use Prefab Maker "Spells & Skills" Tab**
   - Open `Godgame → Prefab Editor`
   - Select the "Spells & Skills" tab
   - Assign your catalogs and binding sets

3. **Validate**
   - Click "Validate All" to check for:
     - Cooldown ≥ 0, GCD groups valid
     - Skill prerequisite DAG is acyclic
     - Status periods are valid (period ≤ duration)
     - Binding references exist for showcased spells

4. **Dry-Run**
   - Click "Dry-Run (Preview)" to see what would be generated without writing assets
   - Review the summary report

5. **Bake Blobs**
   - Click "Bake Blobs" to trigger Unity's baker system
   - Blobs are created automatically when catalogs are baked
   - Runtime systems read spell/skill/status data by ID from blob assets

### Validation Rules

**Spells:**
- Cooldown ≥ 0
- Cast time ≥ 0
- Range ≥ 0
- Area spells must have radius > 0
- Effects must have valid magnitudes and durations

**Skills:**
- Prerequisites must exist in catalog
- Prerequisite graph must be acyclic (no cycles)
- Duplicate IDs are invalid

**Statuses:**
- Duration ≥ 0
- Period ≥ 0
- If Period > 0, Duration must be > Period
- MaxStacks ≥ 1

**Bindings:**
- Spell IDs must exist in spell catalog
- Showcased spells should have bindings (warning if missing)

### Presentation Bindings

Spell presentation bindings map spell IDs to visual assets:
- **StartFX**: Prefab for cast start effect
- **LoopFX**: Prefab for channeled spell loop
- **ImpactFX**: Prefab for impact effect
- **SFX**: Audio clip for sound effect
- **IconToken**: Prefab for UI icon (optional)
- **StyleTokens**: String array for visual theming
- **Sockets**: String array for attachment points (e.g., "Socket_Hand_R")

Binding sets come in two flavors:
- **Minimal**: Placeholder tokens for rapid iteration
- **Fancy**: Full FX assets for final presentation

Runtime can swap binding sets without changing gameplay (spell specs remain unchanged).

### Testing

Required test suites:
- `Spells_Idempotent_BlobHashes`: Verifies deterministic blob generation
- `Skills_Tree_Acyclic_And_Prereqs_Resolved`: Validates skill tree structure
- `Statuses_NoCycles_PeriodicValid`: Checks status effect validity
- `Bindings_MinimalVsFancy_Swap_NoGameplayChange`: Ensures binding swaps don't affect gameplay

## Next Steps

After authoring setup:
1. Create placeholder presentation adapters (Phase 3)
2. Expand AI behaviors and perception (Phase 4)
3. Implement resource gathering/deposit loops (Phase 5)
4. Build construction workflow (Phase 6)
5. Implement building logic systems (Phase 7)
6. Complete vegetation and environment systems (Phase 8)

See `Godgame_PureDOTS_Integration_TODO.md` for detailed phase breakdown.

