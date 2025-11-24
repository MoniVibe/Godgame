0. Design Goals

Scale: Support 1M+ entities (villagers, items, buildings, projectiles, etc.) without GC pressure.

DOTS 1.4 / Burst friendly:

All runtime data blittable (no strings, no managed refs, no polymorphism).

Use BlobAssets for static data (templates) and IComponentData/IBufferElementData for instances.

Use byte / ushort / int IDs instead of strings.

Unified item logic:

Same core system for materials, tools, equipment, reagents, buildings.

Unified Quality / Rarity / TechTier model.

Prefab-friendly:

Authoring via ScriptableObjects / editor templates.

Baked into ECS-friendly blobs at build time.

Deterministic & cheap:

All formulas must be simple, branch-light math, Burst-compatible.

No reflection, no dynamic allocation in hot paths.

1. High-Level Architecture
1.1 Authoring vs Runtime

Authoring side (Editor-only):

MaterialTemplate, ToolTemplate, EquipmentTemplate, BuildingTemplate, etc. (what you already have)

Rich data: strings, rule engines, UI labels, icons, etc.

Stored as ScriptableObjects / editor assets.

Bake step → Runtime side:

All templates compiled into BlobAssets:

ItemDefinitionBlob for equipment/tools/reagents/etc.

MaterialDefinitionBlob for materials.

ProductionRecipeBlob for production chains.

Each entry indexed by a compact ID:

MaterialId : ushort

ItemDefId : ushort (covers weapons, armor, tools, reagents)

BuildingDefId : ushort

Runtime entities only store:

IDs (e.g., ItemDefId, MaterialId)

Dynamic instance data (quality, durability, stacks, etc.)

1.2 Template vs Instance

Templates (static):

Core stats, base quality, rarity weights, tech tier, usage tags.

Stored in BlobAssetReference<GlobalItemCatalogBlob>.

Instances (per-entity):

Current durability / condition.

Current quality (rolled from materials/craftsman or modified).

Owner & relations (equipped by, in inventory, etc.).

Stack count for stackable items.

2. IDs & Catalogs
2.1 ID Types

All IDs are small integers (Burst friendly, cache friendly):

public struct MaterialId : IEquatable<MaterialId>
{
    public ushort Value;
}

public struct ItemDefId : IEquatable<ItemDefId>
{
    public ushort Value;
}

public struct RecipeId : IEquatable<RecipeId>
{
    public ushort Value;
}


No strings at runtime in hot paths.

Mapping name ↔ ID happens only:

In editor

In debug tools / dev-only systems

2.2 Global Catalog Blobs
public struct GlobalItemCatalogBlob
{
    public BlobArray<ItemDefinitionBlob>;
    public BlobArray<MaterialDefinitionBlob>;
    public BlobArray<ProductionRecipeBlob>;
}


One global blob, pinned in ItemCatalogSingleton:

public struct ItemCatalogSingleton : IComponentData
{
    public BlobAssetReference<GlobalItemCatalogBlob> Catalog;
}


All systems that need item data:

Get ItemCatalogSingleton via a single SystemAPI.GetSingleton.

Index into arrays by ItemDefId.Value / MaterialId.Value.

3. Core Data Model (Runtime)
3.1 Unified Quality / Rarity / Tech Tier (Template)

These exist on definitions (blobs), not per entity:

public enum Rarity : byte
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

public struct ItemDefinitionBlob
{
    public ItemDefId Id;

    // Classification
    public byte ItemType;      // Weapon / Armor / Tool / Reagent / Accessory / Other
    public byte SlotKind;      // Hand / Body / Head / Feet / Accessory / None

    // Base quality & rarity
    public float BaseQuality;      // 0–100
    public Rarity BaseRarity;      // Lower bound from template
    public byte TechTier;          // Tier to which this item belongs
    public byte RequiredTechTier;  // Min tier to craft/use (can be equal or lower than TechTier)

    // Base stats (unscaled)
    public float BaseDamage;
    public float BaseArmor;
    public float BaseDurability;
    public float BaseWeight;

    // Material/stat modifiers (weights)
    public float HardnessWeightForDamage;
    public float HardnessWeightForArmor;
    public float ToughnessWeightForDurability;
    public float DensityWeightForWeight;

    // Flags (usage, categories) could be bitmasks:
    public uint UsageFlags;        // Bits: Weapon, Armor, Tool, Container, etc.
}


Materials:

public struct MaterialDefinitionBlob
{
    public MaterialId Id;

    public float BaseQuality;  // 0–100
    public float BasePurity;   // 0–100 for extracted materials
    public Rarity BaseRarity;
    public byte TechTier;      // Min tech tier to extract/use

    // Physical stats (for math, no strings)
    public float Hardness;
    public float Toughness;
    public float Density;
    public float MeltingPoint;
    public float Conductivity;

    // Usage weights per item type group (e.g. bow, blade, shield, handle)
    public float BowFlexibilityScore;
    public float BowTensionScore;
    public float ShieldToughnessScore;
    public float ClubMassScore;
    // etc. (simple floats used by formulas)
}

3.2 Quality / Rarity / Tech Tier (Instance)

Runtime item instance:

public struct ItemInstance : IComponentData
{
    public ItemDefId DefinitionId;

    // Instance-level quality & durability
    public float Quality;        // 0–100
    public float Condition;      // 0–100 (runtime durability / wear)
    public Rarity Rarity;        // Derived at creation time
    public byte TechTier;        // Effective tech tier of the crafted item

    // Stack size (for stackables, else 1)
    public int StackCount;

    // Ownership / relations
    public Entity Owner;         // Villager / container / building
    public Entity ParentContainer; // Inventory, chest, ground tile, etc.
}


All fields are blittable; no strings, no managed references.

4. Quality System (Runtime Rules)
4.1 Value Ranges

Quality: 0–100 float

Condition: 0–100 float (separate from Quality)

TechTier: 0–10 byte

Rarity: enum (5 levels)

4.2 Unified Quality Calculation

A single Burst-friendly function computes final quality at craft time:

public struct QualityWeights
{
    public float MaterialPurityWeight;   // default 0.4
    public float MaterialQualityWeight;  // default 0.3
    public float CraftsmanSkillWeight;   // default 0.2
    public float FacilityQualityWeight;  // default 0.1
}

public static float CalculateItemQuality(
    in ItemDefinitionBlob def,
    in QualityWeights weights,
    in NativeArray<MaterialDefinitionBlob> inputMaterials,
    float craftsmanSkill,      // 0–100
    float facilityQuality      // 0–100
)
{
    // Average purity / quality of inputs
    float avgPurity = 0f;
    float avgQuality = 0f;

    int len = inputMaterials.Length;
    for (int i = 0; i < len; i++)
    {
        avgPurity  += inputMaterials[i].BasePurity;
        avgQuality += inputMaterials[i].BaseQuality;
    }
    if (len > 0)
    {
        float invLen = 1f / len;
        avgPurity  *= invLen;
        avgQuality *= invLen;
    }

    float score =
        avgPurity  * weights.MaterialPurityWeight +
        avgQuality * weights.MaterialQualityWeight +
        craftsmanSkill * weights.CraftsmanSkillWeight +
        facilityQuality * weights.FacilityQualityWeight;

    // Normalize back to 0–100, scale by definition base quality
    float normalized = math.clamp(score, 0f, 100f);
    float result = math.clamp(
        (normalized / 100f) * def.BaseQuality,
        0f, 100f
    );

    return result;
}


No branches except clamps.

Works for all items that are crafted via recipes.

4.3 Quality Tiers (Naming / UI)

We don’t store this per entity; we derive it on UI side:

Quality Range	Tier Name
0–20	Crude
21–40	Poor
41–60	Common
61–80	Fine
81–95	Excellent
96–100	Masterwork

The runtime only stores the numeric Quality; tier name is computed on demand by the UI (non-Burst).

5. Rarity System
5.1 Rarity Calculation

Rarity assigned at craft time, once:

public static Rarity CalculateRarity(
    float quality,
    Rarity maxMaterialRarity,    // max rarity of all input materials
    float craftsmanSkill         // 0–100
)
{
    // Base rarity from quality thresholds:
    Rarity baseRarity;
    if      (quality < 40f) baseRarity = Rarity.Common;
    else if (quality < 60f) baseRarity = Rarity.Uncommon;
    else if (quality < 80f) baseRarity = Rarity.Rare;
    else if (quality < 95f) baseRarity = Rarity.Epic;
    else                    baseRarity = Rarity.Legendary;

    // Chance to upgrade by skill (Burst-safe if using deterministic RNG)
    // Example: at skill >= 80, 10% upgrade chance:
    if (craftsmanSkill >= 80f)
    {
        // Use deterministic RNG based on entity / world seed in real code
        // Pseudocode:
        // if (RandomValue0to1(seed) < 0.1f)
        //     baseRarity = Upgrade(baseRarity);
    }

    // Clamp to not exceed material rarity
    if (baseRarity > maxMaterialRarity)
        baseRarity = maxMaterialRarity;

    return baseRarity;
}


Rarity is stored on the instance (ItemInstance.Rarity) and never recalculated.

Rarity does not directly affect stats; it’s a descriptor of:

Materials used

Quality achieved

Craftsman prowess

6. Tech Tier System
6.1 Where Tech Tier Lives

Materials: MaterialDefinitionBlob.TechTier

Min tech tier to extract/use material effectively.

Item Definitions: ItemDefinitionBlob.TechTier

Conceptual tier (e.g., “Iron Sword = 2”, “Steel Sword = 3”).

Crafting Requirements: ItemDefinitionBlob.RequiredTechTier

Min tech tier of village/facility to craft this definition.

Instances: ItemInstance.TechTier

Effective tech tier of the crafted item (usually = definition tier or min/max of inputs).

6.2 Gate Logic (Runtime)

Village or Building has a TechTier component:

VillageTechTier : IComponentData { public byte Tier; }

Crafting system validates:

bool CanCraft(
    byte villageTechTier,
    in ItemDefinitionBlob itemDef,
    in NativeArray<MaterialDefinitionBlob> materials
)
{
    if (villageTechTier < itemDef.RequiredTechTier)
        return false;

    // Optional: ensure all materials are unlocked
    for (int i = 0; i < materials.Length; i++)
    {
        if (villageTechTier < materials[i].TechTier)
            return false;
    }

    return true;
}

7. Equipment & Relations for 1M Entities
7.1 Equipment Layout

Villager entity:

public struct EquipmentSlots : IBufferElementData
{
    public byte SlotKind;  // Hand, Head, Body, etc
    public Entity Item;    // Entity of equipped item, or Entity.Null
}


Fixed-sized buffer (e.g., 8–10 entries per villager).

Many villagers but few slots each → extremely cache-friendly.

Item entity:

Has ItemInstance component.

Optionally has EquippedTag (empty IComponentData) to filter equipped items quickly.

7.2 Inventory / Containers

Inventory buffer on owner:

public struct InventoryItem : IBufferElementData
{
    public Entity Item;  // Item entity
}


Villagers, chests, buildings can all share this pattern.

For bulk resources (e.g., 10,000 logs), we can store stack items:

One item entity with StackCount = N.

7.3 Performance Constraints / Guidelines

Don’t put big buffers on items; put buffers on owners/containers.

Item entities should be lightweight: ItemInstance + maybe one or two tags.

Use archetypes:

WorldItem (on ground)

InventoryItem (owned)

EquippedItem

Avoid frequently changing archetypes; consider tags on the same archetype instead.

8. Production Chains (Runtime View)
8.1 Recipe Definition Blob
public struct ProductionInputBlob
{
    public MaterialId MaterialId;
    public float MinPurity;
    public float MinQuality;
    public byte MinTechTier;
    public byte Quantity;
}

public struct ProductionRecipeBlob
{
    public RecipeId Id;
    public ItemDefId OutputItemId;
    public BlobArray<ProductionInputBlob> Inputs;

    public float BaseFacilityQuality;   // optional per recipe
    public QualityWeights QualityWeights;
}

8.2 Crafting System (High-Level Flow)

Read ProductionRecipeBlob for recipe.

Gather MaterialDefinitionBlob for each input material.

Check:

Tech tier.

Min purity / quality.

Run CalculateItemQuality(...).

Run CalculateRarity(...).

Spawn item entity:

Add ItemInstance with computed values.

Initialize Condition = Quality (or 100).

Assign owner/container.

All steps are pure math + simple branching, ideal for Burst jobs.

9. Material Attributes (Simplified Runtime)

To stay Burst-friendly, attributes are:

Lightweight enum flags & numeric modifiers.

Applied during crafting; resulting stats are stored in instance, not re-evaluated every frame.

9.1 Attribute Types (Enum)
public enum ItemAttributeType : byte
{
    None = 0,
    IncreasedDurability = 1,
    IncreasedDamage = 2,
    Lightweight = 3,
    FireResistant = 4,
    Flammable = 5,
    // etc.
}

9.2 Template Attributes (Blob)
public struct AttributeTemplateBlob
{
    public ItemAttributeType Type;
    public float Value;         // magnitude
    public bool IsPercentage;   // true → 0–1; false → flat
    public float MinCraftsmanSkill;
    public float ChanceToAdd;   // 0–1
}


Materials and items can each have a small BlobArray<AttributeTemplateBlob>.

9.3 Runtime Attribute Storage

On the instance, to keep it small:

public struct ItemAttributeInstance : IBufferElementData
{
    public ItemAttributeType Type;
    public float Value;       // signed, can be +/- percentage or flat
}


At craft time, we:

Evaluate all attribute templates (materials + recipe).

Roll them against craftsmanSkill.

Apply them to base stats (damage, armor, durability, weight).

Optionally, also store them in DynamicBuffer<ItemAttributeInstance> for UI / later logic.

10. DOTS 1.4 / Burst Rules Checklist

To keep everything friendly for Burst and 1M entities:

 All runtime structs are blittable.

 No strings, no managed references at runtime in hot paths.

 No inheritance/polymorphism; use IDs and tag components.

 Use BlobAssets for static catalogs.

 Use struct enums and byte/ushort for IDs and flags.

 Heavy logic (naming, pretty strings, tooltips) lives outside Burst (UI systems).

 Quality, rarity, and tech tier computed once at creation, not per frame.

 Relations done via Entity references & small buffers, not nested objects.

11. Minimal “Lock-In” Decisions (Summary)

Every item & material has:

Quality (0–100), Rarity (enum), TechTier (0–10) in a unified model.

Definitions live in Blob catalogs, referenced by small integer IDs.

Instances store:

ItemDefId, Quality, Condition, Rarity, TechTier, StackCount, Owner, ParentContainer.

Crafting always:

Validates TechTier & input requirements.

Computes Quality using unified, weighted formula.

Computes Rarity from quality + material rarity + craftsman skill.

Villager equipment & inventory:

Are buffers on the owner, with items as separate entities.

Attributes:

Evaluated at craft time.

Applied to final stats; stored as compact enum/value pairs.