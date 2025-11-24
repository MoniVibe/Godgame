# Prefab Maker Full Functionality Fixes

**Date:** 2025-01-XX  
**Status:** Completed

---

## Overview

Fixed critical issues preventing the Prefab Maker tool from functioning properly, including non-functional editing workflows, missing null guards, and lack of forward compatibility for schema changes.

---

## Issues Fixed

### 1. Non-Functional Editing UI

**Problem:** 
- `EditMaterialTemplate()` and `EditEquipmentTemplate()` were drawing UI inline without a way to close the editor
- Templates were stored in memory but not persisted
- No visual feedback for which template was being edited

**Solution:**
- Implemented split-panel UI: template list on left, editor panel on right
- Added `selectedTemplate` field to track currently editing template
- Updated all Edit button handlers to set `selectedTemplate` instead of calling edit methods directly
- Created `DrawTemplateEditor()` method that routes to appropriate editor based on template type
- Added "Close" button in editor panel

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs`

### 2. Missing Null Guards

**Problem:**
- Editors accessed `bonuses`, `visualPresentation`, `vfxPresentation`, `productionInputs`, etc. without null checks
- Caused NullReferenceExceptions when templates lacked these fields

**Solution:**
- Added null checks and initialization in all editor methods:
  - `EditMaterialTemplate()`
  - `EditEquipmentTemplate()`
  - `EditToolTemplate()`
  - `EditBuildingTemplate()`
  - `EditIndividualTemplate()`
  - `EditReagentTemplate()`
  - `EditMiracleTemplate()`
- Added null guards in helper editors:
  - `BonusEditor.DrawBonusesEditor()`
  - `VisualPresentationEditor.DrawVisualPresentationEditor()`
  - `VisualPresentationEditor.DrawVFXPresentationEditor()`
  - `ProductionChainEditor.DrawProductionInputsEditor()`
  - `ProductionChainEditor.DrawMaterialAttributesEditor()`
- Added null guards in `PrefabGenerator.AddVisualPresentation()` and `AddVFXPresentation()`

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/BonusEditor.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/VisualPresentationEditor.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/ProductionChainEditor.cs`
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabGenerator.cs`

### 3. Forward Compatibility & Migration

**Problem:**
- No mechanism to update old prefab templates when schema changes
- Templates created with older versions would break when new fields were added

**Solution:**
- Created `PrefabTemplateMigrator` utility class with schema versioning
- Implements migration from schema v0 → v1 → v2:
  - **v1:** Ensures `visualPresentation`, `vfxPresentation`, `bonuses` exist
  - **v2:** Ensures `quality`, `rarity`, `techTier`, `requiredTechTier` exist, initializes catalog IDs
- Added automatic migration on window enable (`OnEnable()`)
- Added migration after loading default templates (`LoadDefaultTemplates()`)
- Added menu item: `Godgame/Prefab Tool/Migrate All Templates` for manual migration

**Files Created:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabTemplateMigrator.cs`

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs`

### 4. Pipeline Stability

**Problem:**
- `PerformDryRun()` and `GenerateAllPrefabs()` could receive null data
- Missing error handling in generators

**Solution:**
- Verified `PerformDryRun()` properly initializes material catalog from `materialTemplates`
- Added null checks in `PrefabGenerator` methods before accessing template fields
- Ensured all template lists are initialized before use

**Files Modified:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabGenerator.cs`

---

## Migration Utility Details

### Schema Versions

**Current Schema Version:** 2

**Version 1 Migration:**
- Initializes `visualPresentation` if null
- Initializes `vfxPresentation` if null
- Initializes `bonuses` list if null

**Version 2 Migration:**
- Sets default `quality` (50) if not set, infers from legacy `baseQuality` fields
- Sets default `rarity` (Common) if not set
- Initializes catalog IDs (`MaterialId`, `ItemDefId`, `RecipeId`) from template `id`
- Type-specific migrations:
  - `MaterialTemplate`: Initializes `materialId`, `stats`, `possibleAttributes`
  - `EquipmentTemplate`: Initializes `itemDefId`, `stats`, `minStats`
  - `ToolTemplate`: Initializes `itemDefId`, `recipeId`, `productionInputs`, `qualityDerivation`, `possibleAttributes`

### Usage

**Automatic:** Migration runs automatically when:
- Prefab Editor window is opened (`OnEnable()`)
- Default templates are loaded (`LoadDefaultTemplates()`)

**Manual:** Use menu item:
```
Godgame → Prefab Tool → Migrate All Templates
```

---

## Testing Checklist

- [x] Material template editing works (no null refs)
- [x] Equipment template editing works (no null refs)
- [x] Tool template editing works (no null refs)
- [x] All template types can be edited without errors
- [x] Editor panel closes properly
- [x] Migration runs automatically on window open
- [x] Migration handles legacy templates gracefully
- [x] Dry-run reports generate without errors
- [x] Prefab generation works for all types
- [x] Validation works for all template types

---

## Known Limitations

1. **Persistence:** Templates are still stored in memory. Future work should add ScriptableObject or JSON persistence.

2. **Schema Version Tracking:** Templates don't store their schema version. Migration runs on every window open (idempotent, but could be optimized).

3. **Editor Window State:** Selected template is lost when switching tabs. Could be improved to remember selection per tab.

---

## Future Improvements

1. **Persistence System:**
   - Save templates to ScriptableObjects or JSON
   - Load templates on window open
   - Auto-save on changes

2. **Schema Version Tracking:**
   - Add `schemaVersion` field to templates
   - Only migrate if version < current
   - Store version in persisted templates

3. **Better Editor UX:**
   - Remember selected template per tab
   - Add "Save" button to persist changes
   - Add "Revert" button to discard changes

4. **Validation Feedback:**
   - Show validation errors/warnings in editor panel
   - Highlight invalid fields
   - Auto-fix common issues

---

## References

- `Docs/PrefabTool_Alignment_Implementation.md`: Previous alignment work
- `pref-5848e7.plan.md`: Full functionality plan
- `Assets/Scripts/Godgame/Editor/PrefabTool/`: Implementation files

