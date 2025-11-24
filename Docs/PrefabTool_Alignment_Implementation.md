# Prefab Maker Alignment with items.md Spec

**Status:** Completed  
**Date:** 2025-01-XX  
**Related:** `Docs/Concepts/items.md`, `pref-5848e7.plan.md`

---

## Overview

This document describes the alignment of the Prefab Maker tool with the unified item system specification defined in `Docs/Concepts/items.md`. The alignment ensures that prefab templates can export data compatible with the runtime blob asset system.

---

## Completed Work

### 1. Unified Template Fields

**Added to `PrefabTemplate` base class:**
- `quality` (float, 0-100): Base quality from template
- `calculatedQuality` (float, 0-100): Derived quality from materials/skills
- `rarity` (Rarity enum): Base rarity (Common, Uncommon, Rare, Epic, Legendary)
- `techTier` (byte, 0-10): Tier to which this item belongs
- `requiredTechTier` (byte, 0-10): Minimum tier to craft/use

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs`
  - Added `Rarity` enum matching runtime spec
  - Added unified fields to `PrefabTemplate` base class
  - All concrete templates (Materials, Tools, Equipment, etc.) inherit these fields

### 2. Catalog IDs & Blob Preparation

**Added ID structures:**
- `MaterialId` (ushort): Runtime MaterialId for materials
- `ItemDefId` (ushort): Runtime ItemDefId for items/equipment/tools
- `RecipeId` (ushort): Runtime RecipeId for production recipes

**Templates Updated:**
- `MaterialTemplate`: Added `materialId` field
- `EquipmentTemplate`: Added `itemDefId` field
- `ToolTemplate`: Added `itemDefId` and `recipeId` fields
- `ProductionInput`: Added `materialId` and `minTechTier` fields

**DTO Structures Created:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/BlobDTOs.cs`
  - `MaterialDefinitionDTO`: Maps to `MaterialDefinitionBlob`
  - `ItemDefinitionDTO`: Maps to `ItemDefinitionBlob`
  - `ProductionRecipeDTO`: Maps to `ProductionRecipeBlob`
  - `ProductionInputDTO`: Maps to `ProductionInputBlob`
  - `PrefabCatalogExport`: Container for all DTOs ready for blob baking

### 3. Quality/Rarity/TechTier Calculation Utilities

**Extended `StatCalculation.cs`:**
- `CalculateItemQuality()`: Unified quality calculation per items.md spec
  - Uses `QualityWeights` struct (MaterialPurityWeight, MaterialQualityWeight, CraftsmanSkillWeight, FacilityQualityWeight)
  - Burst-friendly, branch-light math
- `CalculateRarity()`: Rarity derivation from quality, material rarity, and craftsman skill
  - Quality thresholds: <40 Common, <60 Uncommon, <80 Rare, <95 Epic, >=95 Legendary
  - Skill-based upgrade chance (10% at skill >=80)
- `CanCraft()`: Tech tier gate checking
  - Validates village tech tier >= required tech tier
  - Validates all materials unlocked
- `GetQualityTierName()`: UI helper for quality tier display

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/StatCalculation.cs`
  - Added `QualityWeights` struct
  - Added unified calculation methods
  - Updated `QualityDerivation` class with `ToQualityWeights()` converter

### 4. Production Chain Validation & Recipes

**Created `ProductionChainValidator.cs`:**
- `ValidateToolProductionChain()`: Validates tool production chains
  - Checks all referenced materials exist
  - Validates tech tier gates (material tech tier <= tool tech tier)
  - Checks purity/quality gates
  - Detects production cycles using DFS
- `ValidateAllProductionChains()`: Batch validation for all tools

**Recipe Metadata:**
- `ToolTemplate`: Added `baseFacilityQuality` field (0-100)
- `ProductionInput`: Added `minTechTier` field (0-10)
- Quality derivation weights stored in `QualityDerivation` class

**Files Created:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/ProductionChainValidator.cs`

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs` (integrated validation)

### 5. UI & Workflow Updates

**Editor UI Updates:**
- `EditMaterialTemplate()`: Added quality/rarity/tech tier fields and MaterialId editor
- `EditEquipmentTemplate()`: Added quality/rarity/tech tier fields and ItemDefId editor
- `EditToolTemplate()`: Added quality/rarity/tech tier fields, ItemDefId/RecipeId editors, and baseFacilityQuality
- List views updated to show quality/rarity/tech tier and IDs in compact format

**Production Chain Editor:**
- `DrawProductionInputEditor()`: Added `minTechTier` slider (0-10)

**Dry-Run Reports:**
- Updated `GenerateDryRunEquipment()`: Includes quality/rarity/tech tier and ItemDefId
- Updated `GenerateDryRunMaterials()`: Includes quality/rarity/tech tier and MaterialId
- Added `GenerateDryRunTools()`: Includes production chain validation, quality/rarity/tech tier, ItemDefId/RecipeId, and preview calculated quality/rarity

**Validation Integration:**
- `ValidateAllTemplates()`: Now includes production chain validation for tools
- `PerformDryRun()`: Includes tool report with production chain validation

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/ProductionChainEditor.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/DryRunGenerator.cs`

### 6. Future Runtime Hooks (Stubs)

**DTO Export Structure:**
- `BlobDTOs.cs` defines structures that mirror runtime blob definitions
- DTOs include editor metadata (Name, DisplayName) that won't be baked to blobs
- Ready for serialization step that converts templates → DTOs → BlobAssetReference

**Documentation:**
- This document outlines the mapping from templates to DTOs to blobs
- Comments in `BlobDTOs.cs` reference items.md spec sections

---

## Mapping to items.md Spec

### MaterialDefinitionBlob Mapping

```
MaterialTemplate → MaterialDefinitionDTO → MaterialDefinitionBlob

Fields:
- materialId.Value → Id (MaterialId)
- quality → BaseQuality (0-100)
- purity → BasePurity (0-100)
- rarity → BaseRarity
- techTier → TechTier
- stats.hardness → Hardness
- stats.toughness → Toughness
- stats.density → Density
- stats.meltingPoint → MeltingPoint
- stats.conductivity → Conductivity
```

### ItemDefinitionBlob Mapping

```
EquipmentTemplate/ToolTemplate → ItemDefinitionDTO → ItemDefinitionBlob

Fields:
- itemDefId.Value → Id (ItemDefId)
- quality → BaseQuality (0-100)
- rarity → BaseRarity
- techTier → TechTier
- requiredTechTier → RequiredTechTier
- stats.damage → BaseDamage
- stats.armor → BaseArmor
- baseDurability → BaseDurability
- stats.weight / mass → BaseWeight
```

### ProductionRecipeBlob Mapping

```
ToolTemplate → ProductionRecipeDTO → ProductionRecipeBlob

Fields:
- recipeId.Value → RecipeId
- itemDefId.Value → OutputItemDefId
- productionInputs → Inputs (ProductionInputDTO[])
- qualityDerivation.ToQualityWeights() → QualityWeights
- baseFacilityQuality → BaseFacilityQuality
- requiredTechTier → RequiredTechTier
```

---

## Workflow

### Editor Workflow

1. **Create Templates**: Use Prefab Editor to create Material/Equipment/Tool templates
2. **Set Quality/Rarity/Tech Tier**: Configure base values in template editor
3. **Assign IDs**: Set MaterialId/ItemDefId/RecipeId (will be auto-assigned during baking)
4. **Configure Production Chains**: For tools, set production inputs and quality derivation
5. **Validate**: Use "Validate All" to check for errors, including production chain validation
6. **Dry-Run**: Use "Dry-Run (Preview)" to see calculated quality/rarity and validation results
7. **Generate Prefabs**: Use "Generate All Prefabs" to create Unity prefabs

### Export Workflow (Future)

1. **Export to DTOs**: Serialize templates to `PrefabCatalogExport` (JSON/ScriptableObject)
2. **Assign Runtime IDs**: Map editor IDs to runtime ushort IDs (MaterialId, ItemDefId, RecipeId)
3. **Convert to Blobs**: During baking pipeline, convert DTOs to `BlobAssetReference<GlobalItemCatalogBlob>`
4. **Runtime Access**: Systems access catalog via `SystemAPI.GetSingleton<ItemCatalogSingleton>`

---

## Testing

### Validation Tests

- Production chain validation detects cycles
- Tech tier gates prevent crafting with insufficient tech
- Material existence checks prevent broken references
- Quality/rarity calculations match items.md spec formulas

### Dry-Run Tests

- Dry-run reports show calculated quality/rarity for tools
- Production chain validation errors appear in dry-run
- Preview quality uses example craftsman skill (50) and facility quality

---

## Next Steps

1. **ID Assignment**: Implement automatic ID assignment during export (ensure uniqueness)
2. **Serialization**: Add ScriptableObject or JSON export for `PrefabCatalogExport`
3. **Baking Pipeline**: Create baker that converts DTOs to runtime blobs
4. **Runtime Integration**: Wire up `ItemCatalogSingleton` and blob access in runtime systems

---

## References

- `Docs/Concepts/items.md`: Unified item system specification
- `pref-5848e7.plan.md`: Implementation plan
- `Assets/Scripts/Godgame/Editor/PrefabTool/`: Prefab Maker tool implementation

