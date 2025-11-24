# Prefab Creation Guide

## Overview

Prefabs are essential for authoring game entities in Unity. They provide reusable templates with authoring components pre-configured, making it easy to place entities in scenes.

## Required Prefabs for Demo

Based on existing authoring components, create these prefabs:

### Core Gameplay Entities

1. **Villager.prefab**
   - Component: `VillagerAuthoring`
   - Default values: `villagerId=1`, `factionId=0`, `jobType=Gatherer`
   - Optional: Add primitive mesh/visual for placeholder presentation

2. **Storehouse.prefab**
   - Component: `StorehouseAuthoring`
   - Default values: `storehouseId=1001`, `totalCapacity=500f`, `woodCapacity=250f`, `oreCapacity=250f`
   - Optional: Add primitive cube mesh for visualization

3. **ResourceNode_Wood.prefab**
   - Component: `ResourceNodeAuthoring`
   - Default values: `resourceType=Wood`, `initialAmount=100f`, `maxAmount=100f`
   - Optional: Add primitive cylinder/sphere mesh

4. **ResourceNode_Ore.prefab**
   - Component: `ResourceNodeAuthoring`
   - Default values: `resourceType=Ore`, `initialAmount=100f`, `maxAmount=100f`
   - Optional: Add primitive mesh

5. **Band.prefab**
   - Component: `BandAuthoring`
   - Default values: `bandId=1`, `factionId=0`, `memberCount=5`
   - Optional: Add placeholder visual

### Optional Entities (for future expansion)

6. **VillageSpawner.prefab**
   - Component: `VillageSpawnerAuthoring`
   - For spawning villagers over time

7. **LogisticsRequest.prefab**
   - Component: `LogisticsAuthoring`
   - For transport requests (typically spawned at runtime)

## Prefab Structure

```
Assets/Prefabs/
├── Villagers/
│   └── Villager.prefab
├── Buildings/
│   └── Storehouse.prefab
├── Resources/
│   ├── ResourceNode_Wood.prefab
│   └── ResourceNode_Ore.prefab
├── Bands/
│   └── Band.prefab
└── Spawners/
    └── VillageSpawner.prefab
```

## Creating Prefabs Manually

### Step 1: Create GameObject
1. Right-click in Hierarchy → Create Empty
2. Rename (e.g., "Villager")

### Step 2: Add Authoring Component
1. Select GameObject
2. Add Component → Search for authoring component (e.g., `VillagerAuthoring`)
3. Configure default values in Inspector

### Step 3: Add Visual (Optional)
1. Add primitive mesh (Cube, Cylinder, Sphere) as child
2. Scale/position as needed
3. This is for placeholder presentation only

### Step 4: Create Prefab
1. Drag GameObject from Hierarchy to `Assets/Prefabs/` folder
2. Delete original GameObject from scene (prefab instance remains)

## Creating Prefabs via Script

Use Unity's MCP tools or create an editor script:

```csharp
// Example: CreateVillagerPrefab.cs (Editor script)
using UnityEditor;
using UnityEngine;
using Godgame.Authoring;

public static class PrefabCreationHelpers
{
    [MenuItem("Godgame/Create Prefabs/Demo Prefabs")]
    public static void CreateDemoPrefabs()
    {
        // Create Villager prefab
        var villagerGO = new GameObject("Villager");
        villagerGO.AddComponent<VillagerAuthoring>();
        PrefabUtility.SaveAsPrefabAsset(villagerGO, "Assets/Prefabs/Villagers/Villager.prefab");
        Object.DestroyImmediate(villagerGO);

        // Create Storehouse prefab
        var storehouseGO = new GameObject("Storehouse");
        var storehouse = storehouseGO.AddComponent<StorehouseAuthoring>();
        // Configure defaults if needed
        PrefabUtility.SaveAsPrefabAsset(storehouseGO, "Assets/Prefabs/Buildings/Storehouse.prefab");
        Object.DestroyImmediate(storehouseGO);

        // Repeat for other prefabs...
    }
}
```

## Prefab Best Practices

1. **Keep Prefabs Simple**: Only include authoring components and minimal visuals
2. **Use Default Values**: Set sensible defaults in authoring components
3. **Document Variants**: Create prefab variants for different configurations (e.g., `Villager_Combat.prefab`)
4. **No Runtime Logic**: Prefabs should only contain authoring data, not runtime systems
5. **Placeholder Visuals**: Add primitive meshes for demo visualization (can be replaced later)

## Using Prefabs in Scenes

1. **Direct Placement**: Drag prefab into scene, configure values per instance
2. **SubScene Authoring**: Place prefabs in SubScenes for better organization
3. **Runtime Spawning**: Use prefabs with `EntityManager.Instantiate()` for runtime spawning

## Demo Scene Setup

For the demo scene (`Assets/Scenes/SampleScene.unity`):

1. Create SubScene: `Assets/Scenes/GodgameDemoSubScene.unity`
2. Place prefabs in SubScene:
   - 3-5 `Villager` prefabs
   - 1-2 `Storehouse` prefabs
   - 2-3 `ResourceNode_Wood` prefabs
   - 1 `Band` prefab (optional)
3. Configure each instance with unique IDs
4. Add to main scene as SubScene reference

## Prefab Editor Tool

A comprehensive editor tool is available for generating and managing prefabs:

### Accessing the Tool

1. Open Unity Editor
2. Go to `Godgame → Prefab Editor` in the menu bar
3. The Prefab Editor window will open

### Using the Tool

1. **Select Category**: Use the tabs at the top (Buildings, Individuals, Equipment, Materials, Tools, Reagents, Miracles)
2. **Create Template**: Click "+ Add New [Category] Template" to create a new template
3. **Edit Template**: Click "Edit" on a template to open the detailed editor
4. **Generate Prefab**: Click "Generate" to create a prefab from a template
5. **Batch Generate**: Click "Generate All Prefabs" to create all prefabs at once
6. **Validate**: Click "Validate All" to check for errors and warnings

### Building Templates

Building templates support derived stats calculation:
- **Materials**: Add materials with quality multipliers and bonuses
- **Tools**: Specify tool quality affecting construction
- **Builder Skill**: Set builder skill level (0-100)
- **Calculated Stats**: Health and desirability are automatically calculated

Example building types:
- **Residence**: Sleep, rest (max residents, comfort level)
- **Workplace**: Work activities (work capacity)
- **Recreation**: Relax, socialize
- **Utility**: Area bonuses (fertility statues, wells)
- **Storage**: Storehouse, warehouse (storage capacity)
- **Worship**: Temple, shrine (mana generation, worshipper capacity)

### Template Data Models

Templates are stored in memory (can be extended to ScriptableObjects or JSON):
- `BuildingTemplate`: Buildings with derived stats
- `IndividualTemplate`: Villagers, animals, creatures
- `EquipmentTemplate`: Weapons, armor, tools, accessories
- `MaterialTemplate`: Raw/extracted/producible/luxury materials
- `ToolTemplate`: Construction tools
- `ReagentTemplate`: Crafting/miracle reagents
- `MiracleTemplate`: Spells and miracles

### Derived Stat Calculation

The tool automatically calculates:
- **Building Health**: From materials (quality multipliers, health bonuses), tools (durability bonus), and builder skill (0.7x - 1.3x multiplier)
- **Building Desirability**: From materials (quality multipliers, desirability bonuses), tools (small bonus), builder skill (0.8x - 1.2x multiplier), and area bonuses
- **Equipment Durability**: From material quality
- **Tool Quality**: From material quality

### Validation

The validation system checks:
- Required fields (name, ID)
- ID uniqueness across templates
- Valid value ranges (health > 0, etc.)
- Missing authoring components
- Prefab asset existence

## Next Steps

1. ✅ Prefab editor tool created
2. Create prefab folder structure (auto-created by tool)
3. Generate core prefabs using the tool
4. Add placeholder visuals (auto-added by tool)
5. Test prefabs in demo scene
6. Document prefab variants and configurations

