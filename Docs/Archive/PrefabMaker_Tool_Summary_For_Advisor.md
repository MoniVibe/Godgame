# Prefab Maker Tool Summary for Advisor

**Purpose:** Overview of the Godgame Prefab Maker tool architecture, features, and lessons learned to inform planning for a Space4X prefab authoring tool.

**Date:** 2025-01-XX  
**Status:** Production-ready, actively used

---

## Executive Summary

The Prefab Maker is a Unity Editor tool that enables data-driven, asset-agnostic prefab generation for Godgame entities. It supports 7 prefab types (Buildings, Individuals, Equipment, Materials, Tools, Reagents, Miracles) with unified quality/rarity/tech tier systems, production chains, visual presentation, and area-of-effect bonuses. The tool generates Unity prefabs from template data, validates production chains, and exports DTOs ready for runtime blob baking.

**Key Achievement:** Eliminated manual prefab authoring workflow, enabling rapid iteration on game entities through data templates rather than Unity editor work.

---

## Architecture Overview

### Core Components

1. **Data Models** (`DataModels.cs`)
   - Base `PrefabTemplate` class with unified fields (quality, rarity, tech tier, IDs)
   - 7 concrete template types with type-specific fields
   - Material rule system (usage flags, traits, stats)
   - Production chain models (inputs, quality derivation, attributes)

2. **Editor Window** (`PrefabEditorWindow.cs`)
   - Split-panel UI: template list (left) + editor panel (right)
   - 7 category tabs (Buildings, Individuals, Equipment, Materials, Tools, Reagents, Miracles)
   - Search/filter, validation, dry-run preview, batch generation

3. **Generation Pipeline** (`PrefabGenerator.cs`)
   - Creates Unity GameObjects from templates
   - Attaches visual presentation (prefabs, meshes, sprites, primitives)
   - Attaches VFX Graph assets
   - Saves as prefab assets in organized folder structure

4. **Validation System**
   - `PrefabValidator.cs`: Template-level validation (IDs, required fields)
   - `ProductionChainValidator.cs`: Production chain validation (cycles, tech tier gates, material existence)
   - `MaterialRuleEngine.cs`: Material substitution and rule evaluation

5. **Migration System** (`PrefabTemplateMigrator.cs`)
   - Schema versioning (current: v2)
   - Automatic migration on window open
   - Forward compatibility for schema changes

6. **Export System** (`BlobDTOs.cs`)
   - DTOs mirroring runtime blob structures
   - Ready for blob baking pipeline
   - Maps editor templates → runtime blobs

---

## Supported Prefab Types

### 1. Buildings
- Types: Residence, Storage, Worship, Utility, Production, Defense
- Fields: Footprint, placement requirements, material costs, facility tags, residency, bonuses
- Example: Fertility Statue (area-of-effect fertility bonus), Temple (happiness + mana generation)

### 2. Individuals
- Types: Villager, Animal, NPC
- Fields: Disciplines (skills), equipment slots, inventory capacity, miracle affinity, spawn weights
- Example: Basic Villager with default stats and equipment slots

### 3. Equipment
- Types: Weapon, Armor, Tool, Accessory
- Fields: Slot kind, durability, combat stats (damage, armor, crit), physical stats (weight, encumbrance)
- Material requirements with rule-based validation and substitution
- Example: Iron Sword (derived stats from material quality)

### 4. Materials
- Categories: Raw, Extracted, Producible, Luxury
- Fields: Usage flags, traits, physical stats (hardness, toughness, density), purity, quality
- Example: Iron Ingot (used in production chains, has purity/quality)

### 5. Tools
- Production chain support: inputs, quality derivation, facility quality
- Fields: Production inputs (materials), quality weights, base facility quality, recipe ID
- Example: Iron Hammer (produced from Iron Ingot, quality derived from material purity + craftsman skill)

### 6. Reagents
- Fields: Potency, rarity, effects
- Example: Common Herb (low potency, used in alchemy)

### 7. Miracles
- Fields: Mana cost, cooldown, range, area shape, target filters, affinity modifiers
- Example: Minor Heal (point target, instant effect)

---

## Key Features

### 1. Unified Quality/Rarity/Tech Tier System
- **Quality:** 0-100 float, calculated from materials + craftsman skill + facility quality
- **Rarity:** Common/Uncommon/Rare/Epic/Legendary, derived from quality thresholds
- **Tech Tier:** 0-10 byte, gates crafting/unlocking
- All prefab types inherit these fields from base `PrefabTemplate`

### 2. Production Chains
- Tools define production inputs (materials with min purity/quality/tech tier)
- Quality derivation weights (material purity, material quality, craftsman skill, facility quality)
- Production chain validation (detects cycles, validates tech tier gates, checks material existence)
- Quality propagation: low purity materials → low quality products (mitigated by high skill)

### 3. Material Rule Engine
- Rule-based material requirements (usage flags, traits, min stats)
- Automatic substitution when preferred material unavailable
- Substitution scoring for ranking alternatives

### 4. Visual Presentation System
- Supports: Prefab assets, mesh assets, sprite assets, material overrides
- Transform settings (position, rotation, scale offsets)
- Primitive fallback (cube, sphere, etc.) when no asset assigned
- VFX Graph integration (playback settings, transforms)

### 5. Area-of-Effect Bonuses
- Flexible bonus system (fertility, happiness, productivity, etc.)
- Configurable radius, falloff, target filters, duration, stacking
- Applied to buildings, individuals, equipment, materials

### 6. Catalog IDs
- Runtime IDs: `MaterialId`, `ItemDefId`, `RecipeId` (ushort)
- Set during blob baking, used for validation and export
- Maps editor templates → runtime blob arrays

### 7. Dry-Run Preview
- Simulates prefab generation without creating assets
- Shows calculated quality/rarity, validation errors/warnings, substitutions
- Exports JSON report for analysis

### 8. Migration System
- Automatic schema migration on window open
- Handles legacy templates gracefully
- Initializes missing fields with defaults
- Menu item for manual migration

---

## Data Flow

```
Editor Templates (in-memory)
    ↓
[User edits in Prefab Editor Window]
    ↓
[Validation: PrefabValidator + ProductionChainValidator]
    ↓
[Generation: PrefabGenerator creates Unity prefabs]
    ↓
[Export: BlobDTOs ready for baking]
    ↓
Runtime Blobs (BlobAssetReference<GlobalItemCatalogBlob>)
```

---

## Current Limitations

1. **Persistence:** Templates stored in memory only (lost on Unity restart)
   - **Impact:** Must recreate templates each session
   - **Recommendation for Space4X:** Use ScriptableObjects or JSON persistence

2. **Schema Version Tracking:** Templates don't store their schema version
   - **Impact:** Migration runs on every window open (idempotent but inefficient)
   - **Recommendation for Space4X:** Add `schemaVersion` field to templates

3. **Editor State:** Selected template lost when switching tabs
   - **Impact:** Minor UX friction
   - **Recommendation for Space4X:** Remember selection per tab

4. **No Auto-Save:** Changes not persisted automatically
   - **Impact:** Risk of data loss
   - **Recommendation for Space4X:** Auto-save on changes or explicit Save button

---

## Lessons Learned

### What Worked Well

1. **Split-Panel UI:** List + editor panel provides clear workflow
2. **Null Guards Everywhere:** Prevents crashes from missing fields
3. **Automatic Migration:** Ensures forward compatibility without user intervention
4. **Unified Base Class:** Quality/rarity/tech tier on all types simplifies logic
5. **Rule-Based Material System:** Flexible substitution enables design iteration
6. **Dry-Run Preview:** Catches issues before generating assets

### What Could Be Improved

1. **Persistence:** Should have been ScriptableObjects from day one
2. **Schema Versioning:** Should track version per template
3. **Validation Feedback:** Could show errors in editor panel, not just console
4. **Batch Operations:** Could support bulk edit/delete/duplicate
5. **Template Library:** Could share templates between projects

---

## Recommendations for Space4X Prefab Tool

### Must-Have Features

1. **Persistence System**
   - ScriptableObject-based templates (survives Unity restarts)
   - Auto-save on changes or explicit Save button
   - Template library folder structure

2. **Schema Versioning**
   - `schemaVersion` field on all templates
   - Migration only runs if version < current
   - Version stored in persisted templates

3. **Space4X-Specific Prefab Types**
   - Ships (hull types, weapon slots, cargo capacity)
   - Stations (orbital, surface, resource extraction)
   - Components (engines, weapons, shields, sensors)
   - Resources (minerals, gases, energy)
   - Technologies (research unlocks, tech tree dependencies)

4. **Space4X-Specific Systems**
   - Ship stats (hull points, shield capacity, weapon damage, speed, cargo)
   - Component slots (weapon hardpoints, utility slots, engine mounts)
   - Resource requirements (minerals for construction, energy for operation)
   - Tech tier system (early/mid/late game unlocks)
   - Faction-specific variants (different stats per faction)

5. **Visual Presentation**
   - 3D model support (ship meshes, station models)
   - Material variants (faction colors, damage states)
   - VFX integration (thruster effects, shield visuals, weapon fire)

### Nice-to-Have Features

1. **Template Inheritance:** Base templates with variants
2. **Bulk Operations:** Batch edit, duplicate, delete
3. **Template Validation:** Real-time error highlighting in editor
4. **Preview Window:** 3D preview of generated prefab
5. **Template Library:** Share templates between projects
6. **Import/Export:** JSON export for version control, import from other projects

### Architecture Recommendations

1. **Follow Same Pattern:** Base template class + concrete types
2. **Unified Systems:** Quality/rarity/tech tier on all types (if applicable)
3. **Rule Engine:** Component requirements, resource requirements (similar to material rules)
4. **Migration System:** Essential for forward compatibility
5. **Validation System:** Template-level + cross-reference validation
6. **Export System:** DTOs ready for runtime blob baking

### Space4X-Specific Considerations

1. **Faction Variants:** Same ship/component with different stats per faction
   - Could use template inheritance or variant system
   - Example: "Fighter" base template → "Human Fighter", "Alien Fighter" variants

2. **Tech Tree Integration:** Prefabs should reference tech requirements
   - Link to tech tree nodes
   - Validate tech dependencies

3. **Component Slots:** Ships/stations have configurable slot systems
   - Weapon hardpoints (size, count, location)
   - Utility slots (shields, sensors, cargo)
   - Engine mounts (thruster types, count)

4. **Resource Costs:** Construction/operation costs
   - Minerals, energy, time
   - Validate resource availability

5. **Scale System:** Space4X has multiple scales (ship, fleet, system, galaxy)
   - Prefabs might need scale-specific variants
   - Or scale-agnostic design with runtime scaling

---

## Code Structure Reference

### Key Files

```
Assets/Scripts/Godgame/Editor/PrefabTool/
├── DataModels.cs              # Template classes, enums, structs
├── PrefabEditorWindow.cs      # Main editor window UI
├── PrefabGenerator.cs         # Prefab generation logic
├── PrefabValidator.cs         # Template validation
├── ProductionChainValidator.cs # Production chain validation
├── MaterialRuleEngine.cs      # Material rule evaluation
├── PrefabTemplateMigrator.cs  # Schema migration
├── StatCalculation.cs         # Quality/rarity calculations
├── BlobDTOs.cs                # Export DTOs
├── DryRunGenerator.cs         # Dry-run preview
├── BonusEditor.cs             # Bonus UI editor
├── VisualPresentationEditor.cs # Visual asset UI editor
└── ProductionChainEditor.cs   # Production chain UI editor
```

### Template Hierarchy

```
PrefabTemplate (base)
├── quality, rarity, techTier, requiredTechTier
├── visualPresentation, vfxPresentation
├── bonuses
└── presentationId

Concrete Types:
├── BuildingTemplate
├── IndividualTemplate
├── EquipmentTemplate
├── MaterialTemplate
├── ToolTemplate
├── ReagentTemplate
└── MiracleTemplate
```

---

## Conclusion

The Godgame Prefab Maker demonstrates a successful data-driven prefab authoring workflow. Key takeaways for Space4X:

1. **Start with persistence** (ScriptableObjects or JSON)
2. **Unified base class** simplifies cross-type logic
3. **Migration system** essential for long-term maintenance
4. **Null guards everywhere** prevents crashes
5. **Validation system** catches errors early
6. **Dry-run preview** saves time during iteration

The tool is production-ready and actively used. The architecture is sound and can be adapted for Space4X with Space4X-specific prefab types and systems.

---

## Questions for Advisor

1. What Space4X prefab types are needed? (Ships, Stations, Components, Resources, Technologies, etc.)
2. Should Space4X use quality/rarity/tech tier system similar to Godgame?
3. What Space4X-specific systems need template support? (Component slots, resource costs, faction variants, tech tree links)
4. Should Space4X prefabs support multiple scales (ship, fleet, system)?
5. What visual presentation needs? (3D models, VFX, material variants, damage states)

---

**Contact:** See `Docs/PrefabTool_Alignment_Implementation.md` and `Docs/PrefabTool_FullFunctionality_Fixes.md` for detailed implementation notes.

