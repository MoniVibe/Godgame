# Godgame Data Assets

This directory contains ScriptableObject assets and configuration data for the Godgame demo.

## Required Assets

### PureDotsRuntimeConfig
- **File:** `GodgameConfig.asset` (`Assets/Data/Resources/GodgameConfig.asset`)
- **Purpose:** Main configuration for time, history, pooling, and resource types
- **Creation:** Create → PureDOTS → PureDOTS Runtime Config
- **Key Settings:**
  - Resource Types: Wood, Stone, Food, Tools, Mana
  - Time settings: 60 FPS fixed timestep
  - History settings: Default rewind/history configuration

### SceneSpawnProfile
- **File:** `GodgameDemoSpawnProfile.asset` (`Assets/Data/Spawn/GodgameDemoSpawnProfile.asset`)
- **Purpose:** Defines how entities spawn in the demo scene
- **Creation:** Create → PureDOTS → Scene Spawn Profile
- **Key Entries:**
  - Villagers (4-8 per village)
  - Buildings (VillageCenter, Houses, Storehouse, WorshipSite)
  - Resources (Wood, Stone, Food nodes)
  - Vegetation (Trees, Crops, Shrubs)

### VegetationSpeciesCatalog
- **File:** `VegetationSpeciesCatalog.asset` (`Assets/Data/Vegetation/GodgameVegetationSpeciesCatalog.asset`)
- **Purpose:** Defines vegetation species data
- **Creation:** Create → PureDOTS → Vegetation Species Catalog
- **Species:**
  - Index 0: Tree
  - Index 1: Shrub
  - Index 2: Grass
  - Index 3: Crop
  - Index 4: Flower
  - Index 5: Fungus

## Usage

These assets are referenced by authoring components on prefabs:
- `PureDotsConfigAuthoring` → references `GodgameConfig.asset`
- `SceneSpawnAuthoring` → references `GodgameDemoSpawnProfile.asset`
- `VegetationAuthoring` → references `VegetationSpeciesCatalog.asset`

See `PureDOTS/Docs/TODO/Godgame_PrefabChecklist.md` for detailed creation instructions.


