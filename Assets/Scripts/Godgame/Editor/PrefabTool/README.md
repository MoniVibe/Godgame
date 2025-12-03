# Prefab Editor Tool

## Overview

The Prefab Editor Tool provides a comprehensive workflow for generating and managing prefabs across all game entity categories. It supports derived stat calculation, batch generation, and validation.

## Features

- **Category-based Organization**: Buildings, Individuals, Equipment, Materials, Tools, Reagents, Miracles, Spells & Skills
- **Derived Stat Calculation**: Automatically calculates building health/desirability from materials, tools, and builder skills
- **Template System**: Create reusable templates with configurable properties
- **Batch Generation**: Generate all prefabs at once
- **Validation**: Check for errors and warnings before generation
- **Placeholder Visuals**: Automatically adds primitive meshes for visualization
- **Spells & Skills Management**: Import/validate/bake spell/skill/status specs and presentation bindings

## Usage

1. Open `Godgame → Prefab Editor` from the Unity menu
2. Select a category tab
3. Create or edit templates
4. Generate prefabs individually or in batch
5. Validate templates before generation

### Spells & Skills Tab

The "Spells & Skills" tab provides a specialized workflow for ability system data:

1. **Assign Catalogs**: Select your `SpellSpecCatalog`, `SkillSpecCatalog`, `StatusSpecCatalog`, and `SpellBindingSet` assets
2. **Review Specs**: Browse spells, skills, statuses, and bindings in sub-tabs
3. **Dry-Run**: Preview what would be generated without writing assets
4. **Validate**: Check for errors (invalid cooldowns, cyclic prerequisites, etc.)
5. **Bake Blobs**: Trigger Unity's baker system to create runtime blob assets

**Validation Rules:**
- Spells: Cooldown ≥ 0, valid ranges/radii, effect magnitudes sane
- Skills: Prerequisites acyclic, all prereqs exist, no duplicate IDs
- Statuses: Period ≤ Duration, valid durations, no cycles
- Bindings: Spell IDs reference valid spells, showcased spells have bindings

## Architecture

### Data Models (`DataModels.cs`)
- `PrefabTemplate`: Base class for all templates
- `BuildingTemplate`: Buildings with derived stats
- `IndividualTemplate`: Villagers, animals, creatures
- `EquipmentTemplate`: Weapons, armor, tools
- `MaterialTemplate`: Raw/extracted/producible/luxury materials
- `ToolTemplate`: Construction tools
- `ReagentTemplate`: Crafting reagents
- `MiracleTemplate`: Spells and miracles

### Stat Calculation (`StatCalculation.cs`)
- `CalculateBuildingHealth()`: Health from materials/tools/skills
- `CalculateBuildingDesirability()`: Desirability with area bonuses
- `CalculateEquipmentDurability()`: Durability from materials
- `CalculateToolQuality()`: Quality from materials

### Prefab Generator (`PrefabGenerator.cs`)
- `GenerateBuildingPrefab()`: Creates building prefabs
- `GenerateIndividualPrefab()`: Creates individual prefabs
- `SavePrefab()`: Saves prefabs to appropriate folders
- `AddPlaceholderVisual()`: Adds primitive meshes

### Validation (`PrefabValidator.cs`)
- `ValidateBuilding()`: Validates building templates
- `ValidateIndividual()`: Validates individual templates
- `ValidateIdUniqueness()`: Checks for duplicate IDs
- `ValidatePrefabAsset()`: Validates generated prefabs

### Editor Windows
- `PrefabEditorWindow`: Main window with category tabs
- `BuildingTemplateEditorWindow`: Detailed building template editor
- `SpellsSkillsTabUI`: Spells & Skills tab UI with validation and baking

### Validators
- `SpellSpecValidator`: Validates spell specs (cooldowns, ranges, effects)
- `SkillTreeValidator`: Validates skill trees (acyclic prerequisites)
- `StatusSpecValidator`: Validates status specs (periods, durations, stacks)
- `SpellBindingValidator`: Validates presentation bindings (spell ID references)

## Folder Structure

Prefabs are organized into:
```
Assets/Prefabs/
├── Buildings/
├── Individuals/
├── Equipment/
├── Materials/
├── Tools/
├── Reagents/
└── Miracles/
```

## Future Enhancements

- ScriptableObject persistence for templates
- JSON/CSV import/export
- Visual preview of calculated stats
- Template inheritance/variants
- Integration with authoring component creation
- Material/tool library management

