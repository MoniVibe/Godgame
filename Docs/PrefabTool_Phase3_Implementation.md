# Prefab Tool Phase 3 Implementation Summary

## Overview

Phase 3 expansion of the Godgame Prefab Maker has been implemented, adding a trait-based rule engine and expanded schemas for all prefab types. The system is now data-driven and asset-agnostic, with materials as the foundation.

## Completed Components

### 1. Material System Expansion ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs`

- Added `MaterialCategory`, `MaterialUsage`, `MaterialTraits`, `HazardClass`, `PackageClass` enums/flags
- Extended `MaterialTemplate` with:
  - `MaterialStats` (hardness, toughness, density, meltingPoint, conductivity)
  - `LogisticsBlock` (package class, hazard class, mass/volume per unit, spoilage rate)
  - `EconomyBlock` (base value, rarity, trade multiplier)
  - `MiracleAffinity` (fire/water/earth/air/light/shadow affinity)
  - `StyleTokens` (color palette, texture style, material type, tags)
  - Quality/purity/variant fields
  - Substitution support (substitutionRank, substituteFor)

### 2. Expanded Template Types ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs`

#### BuildingTemplate
- `Footprint` (size, shape, polygon points)
- `Placement` (biome, slope, altitude, water, road requirements)
- `MaterialCost` list (rule-based material requirements)
- `FacilityTags` (RefitFacility, Storage, RitualSite, etc.)
- Residency/continuity fields

#### EquipmentTemplate
- `SlotKind` (Hand, Body, Head, Feet, Accessory)
- Mass field
- Rule-based material requirements (usage, traits, min stats)
- Substitution support

#### IndividualTemplate
- `Discipline` list (skills/levels)
- Equipment slots list
- Inventory package class (determines carry capacity)
- Miracle affinity
- Spawn weights

#### MiracleTemplate
- `TargetFilter` (traits/tags for valid targets)
- `AreaShape` (Point, Circle, Rectangle, Line, Cone)
- Area size
- Affinity modifications

#### ResourceNodeTemplate
- Resource type index (ties to MaterialTemplate)
- Capacity and regrowth rate
- Deterministic regrowth seed
- Harvest tool requirements (rule-based)
- Hazard classification
- Footprint

#### ContainerTemplate
- Capacity units
- Accepted package classes
- Throughput rate
- Decay/spoilage policy
- Footprint

### 3. Material Rule Engine ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/MaterialRuleEngine.cs`

- `MaterialRule` class (property-based constraints)
- `ProcessRecipe` class (input→output with time/energy/tool/skill requirements)
- `RuleEvaluationResult` with failure explanations
- `MaterialRuleEngine` static class with:
  - Rule evaluation against materials
  - Substitute finding with scoring
  - Material cost validation
  - Recipe validation (cycle detection placeholder)
  - Forbidden combo checking (e.g., Armor ∧ Flammable)

### 4. Rule Engine Interfaces ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/RuleEngineInterfaces.cs`

- `IRuleSet<TSpec>` - validates specs using material traits/stats
- `IPrefabRecipe<TSpec>` - converts specs → prefab definitions (no assets)
- `IPrefabValidator<TSpec>` - produces rich diagnostics + substitutions

**Concrete Implementations:**
- `BuildingRuleSet` - validates buildings against placement and cost rules
- `EquipmentRuleSet` - validates equipment material compatibility
- `BuildingValidator` - detailed diagnostics with substitution suggestions
- `EquipmentValidator` - equipment-specific validation

### 5. Dry-Run Functionality ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/DryRunGenerator.cs`

- `DryRunResult` - preview of what would be generated
- `DryRunReport` - collection of results with statistics
- `MaterialSubstitution` - records of material substitutions
- Functions:
  - `GenerateDryRunBuildings()` - preview building generation
  - `GenerateDryRunEquipment()` - preview equipment generation
  - `ExportToJson()` - export report to JSON
  - `GenerateDiffSummary()` - human-readable summary

**Integration:**
- Added "Dry-Run (Preview)" button to `PrefabEditorWindow`
- Exports JSON to `Logs/dry-run-buildings.json`
- Displays summary in console and dialog

### 6. Editor Integration ✅

**File:** `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs`

- Added dry-run button and functionality
- Integrated with material catalog and rule engine
- Console output and file export

## Architecture Principles Implemented

✅ **Traits over hard lists** - Material rules are property-based  
✅ **Idempotent generation** - Dry-run shows what would change  
✅ **Prefab == data shell** - Prefab definitions contain only IDs, sockets, tags, style tokens  
✅ **Validation first** - Every generator step has validation with explanations  

## Remaining Work

### High Priority

1. **Validation Panel UI** (Partially Complete)
   - `ValidationDiagnostics` exists with detailed explanations
   - Need UI panel showing "why invalid" with rule clause failures
   - Need substitution suggestions displayed in UI

2. **Tests** (Not Started)
   - `Building_PlacementRules_EditMode` - Test placement validation
   - `Building_CostValidation_PlayMode` - Test cost validation with substitutions
   - `Building_BindingOptionality_PlayMode` - Test optional bindings
   - `Equipment_MaterialCompatibility_EditMode` - Test material compatibility rules
   - `Equipment_Substitution_PlayMode` - Test substitution logic
   - `Equipment_DurabilityFromStats_EditMode` - Test durability calculation

3. **Prefab Generation Integration**
   - Update `PrefabGenerator` to use rule engine for material selection
   - Support substitutions when preferred materials unavailable
   - Emit telemetry events for substitutions

### Medium Priority

4. **Recipe System**
   - Complete cycle detection in `ValidateRecipe()`
   - Recipe authoring UI
   - Recipe baking to DOTS blobs

5. **Additional Template Types**
   - Complete UI for Individuals, Miracles, Nodes, Containers tabs
   - Implement generators for all types

6. **ScriptableObject Persistence**
   - Save templates to ScriptableObjects
   - Load from assets on editor open

## Usage

### Running Dry-Run

1. Open `Godgame → Prefab Editor`
2. Create/edit building or equipment templates
3. Click "Dry-Run (Preview)"
4. Check Console for summary
5. Check `Logs/dry-run-buildings.json` for detailed JSON report

### Creating Rule-Based Templates

```csharp
var building = new BuildingTemplate
{
    name = "Storehouse_Stone",
    footprint = new Footprint { size = new Vector2(4, 4) },
    placement = new Placement { maxSlope = 15f },
    cost = new List<MaterialCost>
    {
        new MaterialCost
        {
            requiredUsage = MaterialUsage.Building,
            minStats = new MaterialStats { hardness = 60f },
            requiredTraits = MaterialTraits.None,
            forbiddenTraits = MaterialTraits.Flammable,
            quantity = 100f,
            allowSubstitution = true
        }
    },
    facilityTags = FacilityTags.Storage
};
```

## Files Modified/Created

- ✅ `god.plan.md` - Updated with Phase 3 plan
- ✅ `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs` - Expanded with Phase 3 schemas
- ✅ `Assets/Scripts/Godgame/Editor/PrefabTool/MaterialRuleEngine.cs` - New rule engine
- ✅ `Assets/Scripts/Godgame/Editor/PrefabTool/RuleEngineInterfaces.cs` - New interfaces and implementations
- ✅ `Assets/Scripts/Godgame/Editor/PrefabTool/DryRunGenerator.cs` - New dry-run system
- ✅ `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs` - Added dry-run integration

## Next Steps

1. Create test fixtures for Building and Equipment validation
2. Implement validation panel UI in PrefabEditorWindow
3. Update PrefabGenerator to use rule engine
4. Add ScriptableObject persistence for templates
5. Complete remaining template type generators

