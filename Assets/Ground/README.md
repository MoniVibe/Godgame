# Ground Presentation Library

This folder curates every terrain presentation profile that feeds the swappable pipeline.
Each biome/moisture/heat/terrain combination has its own folder that contains the URP
materials referenced by both the DOTS presentation registry and the new variant sets.

| Descriptor Key | Folder | Default Prefab |
| --- | --- | --- |
| `godgame.ground.forest.moist.temperate.flat` | `Biome/Forest/MoistTemperateFlat` | `ProceduralGround_Forest_MoistTemperateFlat` |
| `godgame.ground.forest.dry.hot.hills` | `Biome/Forest/DryHotHills` | `ProceduralGround_Forest_DryHotHills` |
| `godgame.ground.grassland.moist.temperate.flat` | `Biome/Grassland/MoistTemperateFlat` | `ProceduralGround_Grassland_MoistTemperateFlat` |
| `godgame.ground.desert.arid.hot.dunes` | `Biome/Desert/AridHotDunes` | `ProceduralGround_Desert_AridHotDunes` |
| `godgame.ground.swamp.lush.warm.flat` | `Biome/Swamp/LushWarmFlat` | `ProceduralGround_Swamp_LushWarmFlat` |
| `godgame.ground.tundra.dry.cold.flat` | `Biome/Tundra/DryColdFlat` | `ProceduralGround_Tundra_DryColdFlat` |

When new descriptors are introduced, mirror the folder pattern here, author the URP
material (re-using the shared Bakings texture stack when possible), then create a
matching prefab + variant set so runtime swaps can resolve immediately.
