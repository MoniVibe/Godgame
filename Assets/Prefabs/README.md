# Godgame Prefab Directory

This directory contains all prefabs for the Godgame demo, organized by category.

## Directory Structure

```
Prefabs/
├── Systems/          # Bootstrap, Camera, Divine Hand
├── Villagers/        # Civilian and combat villager variants
├── Buildings/        # Village structures (Center, House, Storehouse, WorshipSite, ConstructionSite)
├── Resources/        # Resource nodes and processors
├── Vegetation/       # Trees, crops, shrubs
└── Factions/         # Band spawners, patrol anchors, diplomacy markers
```

## Prefab Creation

See `PureDOTS/Docs/TODO/Godgame_PrefabChecklist.md` for step-by-step instructions on creating each prefab.

## Quick Reference

### Systems Prefabs
- `SimulationBootstrap.prefab` - Core simulation setup
- `CameraRig.prefab` - Camera controller (runtime, not baked)
- `DivineHand.prefab` - Player interaction system

### Villager Prefabs
- `Villager.prefab` - Base civilian villager
- `Villager_Combat.prefab` - Combat-capable villager
- `VillagerSpawner.prefab` - Population spawner

### Building Prefabs
- `VillageCenter.prefab` - Village management hub
- `House.prefab` - Residential building
- `Storehouse.prefab` - Resource storage
- `WorshipSite.prefab` - Mana generation
- `ConstructionSite.prefab` - Building construction (optional)

### Resource Prefabs
- `Resource_Wood.prefab` - Wood resource node
- `Resource_Stone.prefab` - Stone resource node
- `Resource_Food.prefab` - Food resource node
- `ResourceProcessor.prefab` - Resource transformation (optional)

### Vegetation Prefabs
- `Tree.prefab` - Tree vegetation
- `Crop.prefab` - Crop vegetation
- `Shrub.prefab` - Shrub vegetation
- `ClimateGrid.prefab` - Environment grid
- `RainCloud.prefab` - Weather effect
- `RainMiracle.prefab` - Miracle weather

### Faction Prefabs
- `BandSpawner.prefab` - Band/army spawner
- `PatrolAnchor.prefab` - Patrol waypoint
- `DiplomacyMarker.prefab` - Faction relations marker

## Notes

- All prefabs should use `TransformUsageFlags.Dynamic` for runtime entities
- Placeholder visuals are recommended for debugging
- Resource type IDs must match entries in ResourceTypeIndex catalog
- Faction IDs should be consistent across related prefabs


