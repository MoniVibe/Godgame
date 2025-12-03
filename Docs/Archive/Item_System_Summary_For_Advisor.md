# Item System Summary — For Advisor Review

**Purpose:** Provide comprehensive overview of current item system implementation to guide finalization and locking in of the system architecture.

**Date:** 2025-01-XX  
**Status:** Phase 3 Implementation — Needs Architecture Review

---

## Executive Summary

The Godgame prefab maker now supports a comprehensive item system with:
- **7 Prefab Types:** Buildings, Equipment, Individuals, Materials, Tools, Reagents, Miracles
- **Production Chains:** Materials → Tools → End Products with quality propagation
- **Quality System:** Derived from material purity, material quality, craftsman skill, and forge quality
- **Rarity System:** Defined but not fully integrated (Common, Uncommon, Rare, Epic, Legendary)
- **Tech Tier System:** Referenced in concepts but not yet implemented in prefab maker
- **Material Attributes:** Skilled craftsman bonuses that affect end products

**Key Question:** How should quality, rarity, and tech tier be locked in and integrated across all item types?

---

## Current Item Types & Their Properties

### 1. Materials (Raw/Extracted/Producible/Luxury)

**Current Implementation:**
- `MaterialCategory`: Raw, Extracted, Producible, Luxury
- `MaterialUsage`: Flags (Building, Armor, Weapon, Tool, Container, Fuel, Consumable, Decorative, Ritual)
- `MaterialTraits`: Property-based flags (Ductile, Hard, Flammable, Fireproof, etc.)
- `MaterialStats`: Hardness, Toughness, Density, Melting Point, Conductivity
- `LogisticsBlock`: Package class, Hazard class, Mass/Volume per unit, Spoilage rate
- `EconomyBlock`: Base value, Rarity, Trade multiplier
- `MiracleAffinity`: Fire/Water/Earth/Air/Light/Shadow affinity
- `StyleTokens`: Color palette, texture style, material type, tags

**Quality/Rarity/Tech Tier:**
- ✅ `baseQuality` (0-100) — Implemented
- ✅ `purity` (0-100) — Implemented (for extracted materials)
- ✅ `rarity` (0-100) — In EconomyBlock, but not fully integrated
- ❌ `techTier` (0-10) — Referenced in concepts, NOT implemented in prefab maker
- ✅ `variantOf` — Supports material variants

**Material Attributes:**
- ✅ `possibleAttributes` — List of attributes skilled craftsmen can add
- ✅ Attributes have `minCraftsmanSkill` and `chanceToAdd` thresholds

**Production Chain Integration:**
- ✅ `usedInProduction` — List of tools/products that use this material
- ✅ Can be used as input in tool production chains

---

### 2. Equipment (Weapons, Armor, Tools, Accessories)

**Current Implementation:**
- `EquipmentType`: Weapon, Armor, Tool, Accessory
- `SlotKind`: Hand, Body, Head, Feet, Accessory
- `EquipmentStats`: Damage, Armor, Block Chance, Crit Chance, Crit Damage, Weight, Encumbrance
- `MaterialUsage` requirements (rule-based)
- `MaterialStats` minimum requirements
- `MaterialTraits` required/forbidden

**Quality/Rarity/Tech Tier:**
- ✅ `calculatedDurability` — Derived from material hardness/toughness/quality
- ✅ `calculatedDamage` — Derived from material hardness/quality
- ✅ `calculatedArmor` — Derived from material hardness/toughness/quality
- ❌ `rarity` — NOT implemented (should be Common/Uncommon/Rare/Epic/Legendary)
- ❌ `techTier` — NOT implemented (should gate equipment availability)
- ✅ `baseDurability` — Base value before material calculations

**Stat Calculation:**
- ✅ Damage = baseDamage + (material.hardness × 0.3) × (material.quality / 100)
- ✅ Armor = baseArmor + (material.hardness × 0.4 + material.toughness × 0.2) × (material.quality / 100)
- ✅ Durability = baseDurability × (material.quality / 100) + (material.hardness × 0.5 + material.toughness × 0.3)

**Missing:**
- Rarity assignment (should come from material rarity + craftsman skill)
- Tech tier requirement (should gate equipment availability)
- Quality display/calculation (currently only durability/damage/armor)

---

### 3. Tools (Production Chain Items)

**Current Implementation:**
- `productionInputs` — List of materials needed (with quantity, min purity, min quality)
- `producedFrom` — Parent material/tool name
- `qualityDerivation` — Weighted quality calculation system
- `possibleAttributes` — Attributes skilled craftsmen can add
- `workEfficiency` — Multiplier for work speed/efficiency
- `constructionSpeedBonus` — Bonus to construction speed
- `durabilityBonus` — Bonus to durability

**Quality/Rarity/Tech Tier:**
- ✅ `baseQuality` (0-100) — Implemented
- ✅ `calculatedQuality` — Derived from quality derivation formula
- ✅ Quality derivation weights:
  - Material Purity: 0.4 (default)
  - Material Quality: 0.3 (default)
  - Craftsman Skill: 0.2 (default)
  - Forge Quality: 0.1 (default)
- ❌ `rarity` — NOT implemented
- ❌ `techTier` — NOT implemented
- ✅ `minQuality` / `maxQuality` — Bounds in quality derivation

**Quality Formula:**
```
Quality = (MaterialPurity × purityWeight + 
           MaterialQuality × qualityWeight + 
           CraftsmanSkill × skillWeight + 
           ForgeQuality × forgeWeight) × baseQualityMultiplier
Quality = Clamp(Quality, minQuality, maxQuality)
```

**Production Chain Example:**
```
Iron_Ingot (purity: 85%, quality: 65%)
  ↓
Iron_Screws (requires Iron_Ingot, quality derived from input)
  ↓
Iron_Mace (requires Iron_Screws + Iron_Ingot, quality derived from inputs)
```

**Missing:**
- Rarity assignment based on quality thresholds
- Tech tier requirement for tool availability
- Rarity propagation through production chain

---

### 4. Buildings, Individuals, Reagents, Miracles

**Current Implementation:**
- Buildings: Have derived stats (health, desirability) from materials/tools
- Individuals: Have disciplines, equipment slots, inventory caps
- Reagents: Have potency, rarity (0-100)
- Miracles: Have mana cost, cooldown, range, target filters

**Quality/Rarity/Tech Tier:**
- ❌ Most types lack explicit quality/rarity/tech tier fields
- ✅ Buildings have `calculatedHealth` and `calculatedDesirability`
- ✅ Reagents have `rarity` (0-100) but not enum-based

---

## Quality System — Current State

### What's Implemented

1. **Material Quality:**
   - `baseQuality` (0-100) — Base quality value
   - `purity` (0-100) — For extracted materials
   - Quality affects derived stats (durability, damage, armor)

2. **Tool Quality Derivation:**
   - Weighted formula combining:
     - Material purity (40% default)
     - Material quality (30% default)
     - Craftsman skill (20% default)
     - Forge quality (10% default)
   - Configurable weights per tool
   - Min/max quality bounds

3. **Equipment Quality:**
   - Derived from material quality
   - Affects durability, damage, armor calculations
   - No explicit quality field (only calculated stats)

### What's Missing

1. **Quality Display:**
   - No unified quality display across all item types
   - Equipment doesn't have explicit quality field (only calculated stats)

2. **Quality Propagation:**
   - Production chains calculate quality but don't store it on intermediate products
   - No quality inheritance tracking

3. **Quality Tiers:**
   - No quality tier thresholds (e.g., Poor/Common/Good/Excellent/Masterwork)
   - No quality-based naming (e.g., "Crude Iron Sword" vs "Masterwork Iron Sword")

---

## Rarity System — Current State

### What's Implemented

1. **Rarity Enum (in concepts):**
   ```csharp
   public enum Rarity : byte
   {
       Common = 0,
       Uncommon = 1,
       Rare = 2,
       Epic = 3,
       Legendary = 4
   }
   ```

2. **Material Rarity:**
   - `rarity` field in `EconomyBlock` (0-100 float, not enum)
   - Used for spawn rates and trade value

3. **Reagent Rarity:**
   - `rarity` field (0-100 float, not enum)

### What's Missing

1. **Rarity Assignment:**
   - No automatic rarity assignment based on quality thresholds
   - No rarity propagation through production chains
   - Equipment/Tools don't have rarity fields

2. **Rarity Integration:**
   - Rarity not used in material requirements (only quality/purity)
   - Rarity not displayed in UI
   - Rarity not affecting stat calculations

3. **Rarity Sources:**
   - Should rarity come from material rarity?
   - Should rarity be upgraded by craftsman skill?
   - Should rarity be separate from quality?

---

## Tech Tier System — Current State

### What's Referenced (in concepts)

1. **Tech Tier (0-10):**
   - Gates recipe availability
   - Material unlocks tied to tech tier
   - Business tech specialization

2. **Tech Tier Examples:**
   - Tier 1-5: Basic iron to master steel
   - Tier 6-8: Advanced magical forging (Mithril)
   - Tier 8+: Legendary materials

### What's Missing

1. **Tech Tier Fields:**
   - ❌ No `techTier` field in any template type
   - ❌ No tech tier requirements in production chains
   - ❌ No tech tier gating in material requirements

2. **Tech Tier Integration:**
   - Should tech tier gate material availability?
   - Should tech tier gate tool/equipment crafting?
   - Should tech tier affect quality caps?

3. **Tech Tier Display:**
   - No UI for setting tech tier
   - No tech tier validation

---

## Production Chain System — Current State

### What's Implemented

1. **Production Inputs:**
   - `ProductionInput` class with material name, quantity, min purity, min quality
   - UI for adding/editing production inputs
   - Material dropdown selection

2. **Production Chain Tracking:**
   - `producedFrom` — Parent material/tool name
   - `usedInProduction` — List of products using this material

3. **Quality Propagation:**
   - Quality derivation formula propagates through chain
   - Material purity/quality affects end product quality

### What's Missing

1. **Rarity Propagation:**
   - No rarity inheritance through production chain
   - Should end product rarity be based on input rarity?

2. **Tech Tier Propagation:**
   - No tech tier requirements in production chains
   - Should tech tier gate production chain steps?

3. **Production Chain Validation:**
   - No cycle detection (A → B → A)
   - No validation that all inputs exist
   - No validation that production chain is complete

---

## Material Attributes System — Current State

### What's Implemented

1. **MaterialAttribute Class:**
   - `name` — Attribute name (e.g., "IncreasedDurability")
   - `value` — Attribute value
   - `isPercentage` — Is value a percentage?
   - `minCraftsmanSkill` — Minimum skill to add (0-100)
   - `chanceToAdd` — Probability (0-1)

2. **Attribute Sources:**
   - Materials can have `possibleAttributes`
   - Tools can have `possibleAttributes`
   - Attributes transfer to end products

3. **Attribute Examples:**
   - "IncreasedDurability" (+15% if craftsman skill ≥ 70)
   - "SharpEdge" (+5 damage if craftsman skill ≥ 80)

### What's Missing

1. **Attribute Application:**
   - No runtime system to apply attributes based on craftsman skill
   - No attribute stacking rules
   - No attribute conflict resolution

2. **Attribute Types:**
   - Should attributes be enum-based or string-based?
   - Should attributes have categories (Combat, Utility, Quality)?

---

## Key Design Questions for Advisor

### 1. Quality System

**Q: Should all item types have explicit quality fields?**
- Currently: Materials/Tools have quality, Equipment only has calculated stats
- Options:
  - A) Add explicit `quality` field to Equipment (0-100)
  - B) Keep quality implicit in calculated stats
  - C) Hybrid: Quality field + calculated stats

**Q: How should quality be displayed/used?**
- Should quality affect item naming? (e.g., "Crude" vs "Masterwork")
- Should quality have tiers? (Poor/Common/Good/Excellent/Masterwork)
- Should quality affect all stats or just durability?

### 2. Rarity System

**Q: How should rarity be assigned?**
- A) Based on quality thresholds (e.g., Quality 0-40 = Common, 41-60 = Uncommon, etc.)
- B) Based on material rarity (inherit from input materials)
- C) Based on craftsman skill (high skill = higher rarity chance)
- D) Manual assignment per item

**Q: Should rarity be separate from quality?**
- Can you have high quality but common rarity?
- Can you have low quality but rare rarity (due to material rarity)?

**Q: How should rarity propagate through production chains?**
- Should end product rarity be max of input rarities?
- Should end product rarity be average of input rarities?
- Should rarity upgrade based on craftsman skill?

### 3. Tech Tier System

**Q: Where should tech tier be stored?**
- A) On materials (gates material availability)
- B) On tools/equipment (gates item crafting)
- C) On production recipes (gates recipe availability)
- D) All of the above

**Q: How should tech tier gate items?**
- Should tech tier gate material extraction? (e.g., can't extract Mithril until Tier 6)
- Should tech tier gate tool crafting? (e.g., can't craft advanced tools until Tier 5)
- Should tech tier affect quality caps? (e.g., max quality increases with tier)

**Q: Should tech tier be per-village or global?**
- Should different villages have different tech tiers?
- Should tech tier unlock globally or per-village?

### 4. Production Chain Integration

**Q: How should quality/rarity/tech tier flow through production chains?**
- Should quality be averaged, maxed, or weighted?
- Should rarity be inherited or calculated?
- Should tech tier be max of inputs or separate requirement?

**Q: Should production chains validate tech tier?**
- Should each step in chain require minimum tech tier?
- Should end product tech tier be max of all inputs?

### 5. Material Attributes

**Q: How should attributes be applied?**
- Should attributes be applied deterministically (if skill met) or probabilistically (chanceToAdd)?
- Should attributes stack? (e.g., multiple "IncreasedDurability" attributes)
- Should attributes conflict? (e.g., can't have both "Flammable" and "Fireproof")

**Q: Should attributes be enum-based or string-based?**
- Enum-based: Type-safe, limited set
- String-based: Flexible, extensible
- Hybrid: Common attributes as enum, custom as string

---

## Recommended Architecture Decisions

### 1. Unified Quality/Rarity/Tech Tier Fields

**Recommendation:** Add explicit fields to all item types:

```csharp
public abstract class PrefabTemplate
{
    // Quality (0-100)
    public float quality = 50f;
    public float calculatedQuality = 50f; // Derived from materials/skills
    
    // Rarity (enum)
    public Rarity rarity = Rarity.Common;
    
    // Tech Tier (0-10)
    public byte techTier = 0;
    public byte requiredTechTier = 0; // Minimum tier to craft/use
}
```

### 2. Quality Calculation

**Recommendation:** Unified quality calculation function:

```csharp
public static float CalculateItemQuality(
    float baseQuality,
    List<MaterialTemplate> inputs, // Production inputs
    float craftsmanSkill,
    float facilityQuality,
    QualityDerivation derivation
)
{
    // Weighted average of inputs + craftsman + facility
    // Apply derivation weights
    // Clamp to min/max
}
```

### 3. Rarity Assignment

**Recommendation:** Rarity based on quality thresholds + material rarity:

```csharp
public static Rarity CalculateRarity(
    float quality,
    Rarity materialRarity, // Max rarity of input materials
    float craftsmanSkill
)
{
    // Base rarity from quality thresholds
    Rarity baseRarity = QualityToRarity(quality);
    
    // Upgrade chance based on craftsman skill
    if (craftsmanSkill >= 80 && Random.value < 0.1f)
        baseRarity = UpgradeRarity(baseRarity);
    
    // Can't exceed material rarity
    return Min(baseRarity, materialRarity);
}
```

### 4. Tech Tier Integration

**Recommendation:** Tech tier on materials and recipes:

```csharp
public class MaterialTemplate : PrefabTemplate
{
    public byte techTier = 0; // Tier required to extract/use this material
}

public class ProductionInput
{
    public byte minTechTier = 0; // Minimum tech tier to use this input
}

public class ToolTemplate : PrefabTemplate
{
    public byte requiredTechTier = 0; // Minimum tier to craft this tool
}
```

### 5. Production Chain Validation

**Recommendation:** Add validation system:

```csharp
public static ValidationResult ValidateProductionChain(
    ToolTemplate tool,
    List<MaterialTemplate> materialCatalog,
    List<ToolTemplate> toolCatalog
)
{
    // Check all inputs exist
    // Check no cycles (A → B → A)
    // Check tech tier requirements
    // Check quality/rarity requirements are achievable
}
```

---

## Implementation Checklist

### Phase 1: Core Fields
- [ ] Add `quality`, `rarity`, `techTier` to base `PrefabTemplate`
- [ ] Add `requiredTechTier` to `ToolTemplate` and `EquipmentTemplate`
- [ ] Update all template types to include these fields

### Phase 2: Quality System
- [ ] Create unified `CalculateItemQuality()` function
- [ ] Update quality calculation for all item types
- [ ] Add quality display in UI
- [ ] Add quality tier thresholds (Poor/Common/Good/Excellent/Masterwork)

### Phase 3: Rarity System
- [ ] Convert `rarity` from float to enum in all templates
- [ ] Implement `CalculateRarity()` function
- [ ] Add rarity propagation through production chains
- [ ] Add rarity display in UI
- [ ] Add rarity-based item naming

### Phase 4: Tech Tier System
- [ ] Add `techTier` to `MaterialTemplate`
- [ ] Add `requiredTechTier` to `ToolTemplate` and `EquipmentTemplate`
- [ ] Add `minTechTier` to `ProductionInput`
- [ ] Add tech tier validation in production chains
- [ ] Add tech tier display in UI

### Phase 5: Production Chain Validation
- [ ] Implement cycle detection
- [ ] Implement input validation
- [ ] Implement tech tier validation
- [ ] Implement quality/rarity requirement validation

### Phase 6: Material Attributes
- [ ] Decide on enum vs string-based attributes
- [ ] Implement attribute application system
- [ ] Implement attribute stacking rules
- [ ] Implement attribute conflict resolution

---

## Current File Structure

**Core Data Models:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/DataModels.cs` — All template classes
- `Assets/Scripts/Godgame/Editor/PrefabTool/MaterialRuleEngine.cs` — Rule evaluation
- `Assets/Scripts/Godgame/Editor/PrefabTool/StatCalculation.cs` — Stat calculations

**Editor UI:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabEditorWindow.cs` — Main editor window
- `Assets/Scripts/Godgame/Editor/PrefabTool/BuildingTemplateEditorWindow.cs` — Building editor
- `Assets/Scripts/Godgame/Editor/PrefabTool/ProductionChainEditor.cs` — Production chain UI
- `Assets/Scripts/Godgame/Editor/PrefabTool/BonusEditor.cs` — Bonus/attribute UI
- `Assets/Scripts/Godgame/Editor/PrefabTool/VisualPresentationEditor.cs` — Visual asset UI

**Generation:**
- `Assets/Scripts/Godgame/Editor/PrefabTool/PrefabGenerator.cs` — Prefab generation
- `Assets/Scripts/Godgame/Editor/PrefabTool/DryRunGenerator.cs` — Dry-run preview

---

## Questions for Advisor

1. **Architecture:** Should quality/rarity/tech tier be unified across all item types, or can they differ?

2. **Rarity Assignment:** What should determine rarity? Quality thresholds? Material rarity? Craftsman skill? Combination?

3. **Tech Tier:** Should tech tier gate material extraction, tool crafting, or both? Should it be per-village or global?

4. **Production Chains:** How should quality/rarity/tech tier propagate? Average? Max? Weighted?

5. **Material Attributes:** Should attributes be enum-based (type-safe) or string-based (flexible)? How should they stack/conflict?

6. **Quality Display:** Should quality affect item naming? Should there be quality tiers (Poor/Common/Good/etc.)?

7. **Backward Compatibility:** How should we handle existing templates that don't have quality/rarity/tech tier?

---

## Next Steps

1. **Advisor Review:** Review this document and provide guidance on architecture decisions
2. **Design Lock:** Finalize quality/rarity/tech tier system architecture
3. **Implementation:** Add missing fields and systems based on advisor feedback
4. **Validation:** Add production chain validation and quality/rarity/tech tier validation
5. **Testing:** Create test fixtures for quality/rarity/tech tier systems
6. **Documentation:** Update user-facing documentation with final system

---

**End of Summary**

