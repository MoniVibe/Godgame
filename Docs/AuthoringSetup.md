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

## Next Steps

After authoring setup:
1. Create placeholder presentation adapters (Phase 3)
2. Expand AI behaviors and perception (Phase 4)
3. Implement resource gathering/deposit loops (Phase 5)
4. Build construction workflow (Phase 6)
5. Implement building logic systems (Phase 7)
6. Complete vegetation and environment systems (Phase 8)

See `Godgame_PureDOTS_Integration_TODO.md` for detailed phase breakdown.

