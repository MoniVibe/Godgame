# Businesses & Production – Chunk 3

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 3  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md)), Chunk 1 ([Wealth & Ledgers](Wealth_And_Ledgers_Chunk1.md)), Chunk 2 ([Resources & Mass](Resources_And_Mass_Chunk2.md))  
**Feeds into:** Chunk 4 ([Trade & Logistics](Trade_And_Logistics_Chunk4.md)), Chunk 5 ([Markets & Prices](Markets_And_Prices_Chunk5.md)), Chunk 6 ([Policies & Macro](Policies_And_Macro_Chunk6.md))  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 3 establishes the local production engine – the system that transforms resources into refined materials and finished goods within a settlement. It answers a fundamental question:

**"Given resources in inventories and people with skills, how do businesses turn those into refined materials and finished goods?"**

This chunk provides the local, in-settlement production engine:

1. **Mines/forests → raw chunks → stockpiles** (Chunk 2)
2. **Stockpiles + artisans + workshops → refined materials** (ingots, lumber, cut stone, processed herbs, flour)
3. **Refined materials + components + expert artisans → end products** (weapons, armor, wagons, buildings, potions, etc.)

**Compliance:** All systems in Chunk 3 must:
- ✅ Use inventories for all inputs/outputs (Chunk 2)
- ✅ Use wealth ledgers for wages/profit (Chunk 1)
- ✅ Be agnostic about cross-village trade (Chunk 4)
- ✅ Be agnostic about market-driven prices (Chunk 5)
- ✅ Be agnostic about tax/tariff/policy effects (Chunk 6)

**Reference:** This implements the core of the [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) system within one settlement/worldspace.

---

## Scope & Boundaries

### In Scope (Chunk 3)

**Business-level production pipelines:**
- **Extracted → Refined** (purity → quality)
- **Refined → End Products** (quality/rarity/tech tier propagation)

**Business operations:**
- Local job queues, capacity, throughput, downtime
- Wages to workers, simple profit accounting (connecting to Chunk 1)

**Artisan influence:**
- Expertise, lessons, business quality → better outputs (quality/rarity/tech tiers)

**Integration with Physical Resource Chunks:**
- Resource nodes → chunks → storehouse inventories → production inputs

**Reference:** See [`BusinessAndAssets.md`](BusinessAndAssets.md) for business ownership and operations, [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed production mechanics.

---

### Explicitly Out of Scope (Later Chunks)

**Cross-village outsourcing & trade routes:**
- ❌ Chunk 4 handles trade routes and logistics
- ❌ Chunk 3 only handles local, in-settlement production

**Dynamic prices & arbitrage:**
- ❌ Chunk 5 handles market pricing
- ❌ Chunk 3 uses base costs or fixed recipe costs

**Sanctions, tariffs, debt mechanics (macro policies):**
- ❌ Chunk 6 handles policy effects
- ❌ Chunk 3 is policy-agnostic

**Complex business cooperation contracts:**
- ❌ Profit-sharing, partnerships can be stubbed but not fully simulated yet
- ❌ Basic employment contracts only

---

## Conceptual Layers Inside Chunk 3

Chunk 3 defines three conceptual "stages" in the production pipeline. These correspond to different semantic meanings, not different physical representations – all of them are still items in inventories (Chunk 2). Chunk 3 defines the meaning and transforms.

### 1. Extracted Resources (Raw, Purity Only)

**What:** Raw materials harvested from nature or mines

**Properties:**
- ResourceType (IronOre, RawWood, RawStone, RawHerbs, RawGrain)
- Purity (0–100%) – influences yield when refining

**Examples:**
- Iron ore from mines
- Raw logs from forests
- Raw stone from quarries
- Raw herbs from fields
- Raw grain from farms

**Storage:** Stored in inventories as items with purity metadata

**Reference:** See [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed extracted resource mechanics.

---

### 2. Produced Materials (Refined, with Quality/Rarity/Tech Tier)

**What:** Refined materials processed from extracted resources

**Properties:**
- MaterialType (IronIngot, SteelIngot, Lumber, CutStone, Flour, ProcessedHerbs)
- Quality (1–100)
- Rarity (Common, Uncommon, Rare, Epic, Legendary)
- TechTier (Tech level of the refining process)

**Examples:**
- Iron ingots from iron ore
- Lumber from raw logs
- Cut stone from raw stone blocks
- Flour from raw grain
- Processed herbs from raw herbs

**Storage:** Stored in inventories as items with quality/rarity/tech tier metadata

**Reference:** See [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed produced material mechanics.

---

### 3. End Products (Complex Goods, with Stats & Durability)

**What:** Finished goods crafted from produced materials

**Properties:**
- ProductType (Longsword, PlateArmor, Wagon, StoneHouse, HealingPotion)
- Quality (1–100)
- Rarity (Common, Uncommon, Rare, Epic, Legendary)
- TechTier (Tech level of the crafting process)
- Stat components (WeaponStats, ArmorStats, WagonStats, PotionStats, etc.)
- Durability (for equipment)

**Examples:**
- Weapons (swords, bows, spears)
- Armor (leather, chainmail, plate)
- Vehicles (wagons, carts)
- Buildings (houses, workshops)
- Consumables (potions, food)

**Storage:** Stored in inventories as items with quality/rarity/tech tier/stats/durability metadata

**Reference:** See [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed end product mechanics.

---

## Data Model

### Resource Stages

We standardize three conceptual "stages", all represented via ItemSpec (Chunk 2) + extra metadata:

**Extracted Resources (Raw):**
- Stored in inventories as items with:
  - `ResourceType` (IronOre, RawWood, RawStone, RawHerbs, RawGrain)
  - `Purity` (0–100) – influences yield when refining

**Produced Materials (Refined):**
- Stored in inventories as items with:
  - `MaterialType` (IronIngot, SteelIngot, Lumber, CutStone, Flour, ProcessedHerbs)
  - `Quality` (1–100)
  - `Rarity` (enum)
  - `TechTier` (int)

**End Products (Complex Goods):**
- Stored in inventories as items with:
  - `ProductType` (Longsword, PlateArmor, Wagon, StoneHouse, HealingPotion)
  - `Quality` (1–100)
  - `Rarity` (enum)
  - `TechTier` (int)
  - Stat components (WeaponStats, ArmorStats, etc.)
  - `Durability` (for equipment)

**Note:** Chunk 2 already provides mass/volume/stack via ItemSpec. Chunk 3 adds these semantic layers and relationships via extra components/catalog metadata.

---

### Production Recipes (Catalog, Moddable)

Chunk 3 requires a generic, data-driven `ProductionRecipe` catalog:

**Required Fields:**
- `RecipeId` – Stable ID
- `Stage` – Enum: Refining / Crafting / Enchanting (Enchanting optional or later)
- `Inputs` – Array of (ItemId or StageTag, quantity, expected purity/quality range)
- `Outputs` – Array of (ItemId/Stage, quantity)
- `RequiredBusinessType` – Enum: Blacksmith, Sawmill, Quarry, Mill, Herbalist, Wainwright, Builder, Alchemist, etc.
- `MinTechTier` – Minimum tech tier required
- `MinArtisanExpertise` – Minimum artisan skill level required
- `BaseTimeCost` – Base time to complete (worker-hours)
- `LaborCost` – Worker-hours required

**Optional Fields:**
- `SupportsBatch` – Boolean: Can produce multiple units at once
- `WasteByproducts` – Array of (ItemId, quantity) for waste products (slag, bark, chaff, etc.)
- `WasteDestination` – Where waste goes (discard, sell, reuse)

**Moddability:** Modders can add recipes to define new materials (ebony lumber, mithril ingots) and products (magitech armor, spaceship parts) without touching logic.

**Example (Conceptual):**
```
ProductionRecipe {
  RecipeId: "iron_ore_to_ingot",
  Stage: Refining,
  Inputs: [
    { ItemId: "iron_ore", Quantity: 100, PurityRange: [0, 100] }
  ],
  Outputs: [
    { ItemId: "iron_ingot", Quantity: "InputPurity%" }
  ],
  RequiredBusinessType: Blacksmith,
  MinTechTier: 1,
  MinArtisanExpertise: 10,
  BaseTimeCost: 8.0,  // 8 worker-hours
  LaborCost: 1.0,     // 1 worker
  WasteByproducts: [
    { ItemId: "slag", Quantity: "100 - InputPurity%" }
  ]
}
```

---

### Business & Worker Metadata Integration

Chunk 3 uses existing data from other systems:

**From Business & Assets:**
- Business quality, level, asset condition, installed equipment
- Business capacity, throughput limits

**From Individual Progression / Education:**
- Artisan expertise/skill tier
- Lessons and quality of those lessons
- Education level

**From Guild System:**
- Guild-imposed quality standards
- Rejection of subpar work
- Guild training benefits

**Integration:** These feed into quality/rarity/tech-tier calculations, but the rules of how they affect output quality are defined in data plus shared formulas (see Formula Configs section).

**Reference:** See [`BusinessAndAssets.md`](BusinessAndAssets.md) for business metadata, [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for artisan influence formulas.

---

### Formula Configs (Data-Driven)

Chunk 3 requires data-driven formula configurations:

**RefiningFormulaConfig:**
- Coefficients for quality calculation from purity + skill
- Example: `BaseQuality = Purity × 0.6 + ArtisanExpertise × 0.3 + BusinessQuality × 0.1`
- Coefficients are in config, not hardcoded

**CraftingFormulaConfig:**
- Coefficients for quality propagation from component qualities
- Example: `BaseQuality = Average(ComponentQualities) × 0.7 + ArtisanBonus + TechTierBonus`
- Coefficients are in config, not hardcoded

**DurabilityConfig:**
- Rules for durability inheritance from components
- "Chain is as strong as weakest link" rules
- Coefficients are in config, not hardcoded

**RarityConfig:**
- Rules for rarity calculation
- Base from rarest components
- Critical success chances based on expertise/lessons

**Moddability:** All formula coefficients live in Blob configs so you can tweak them per world without rewriting systems.

---

## Production Flows

### From Physical Chunk → Stockpile

This is the bridge from Physical Resource Chunks to "normal items":

**Process:**
1. Resource node destroyed or worked → emits ResourceChunk entities (Chunk 2)
2. Villagers haul these chunks to a storehouse
3. On deposit, Chunk 2's storehouse API:
   - Calculates resource type, mass, quality/tier
   - Adds corresponding ExtractedResource item(s) to the storehouse inventory
   - Despawns or pools the chunk entities

**Result:** Chunk 3 then only sees "we have X kg of IronOre at Y% purity in this inventory", not the micro-chunk geometry.

**Reference:** See Physical Resource Chunks documentation (when available) for chunk mechanics.

---

### Extracted → Produced (Refining Stage)

**Examples:**
- Iron ore → iron/steel ingots
- Raw logs → lumber
- Raw stone → cut stone
- Raw grain → flour
- Raw herbs → processed herbs

**Core Rules:**

**Yield based on purity:**
```
OutputQty = InputQty × Purity% (discard rest as waste)
```

**Example:**
- 100 iron ore at 75% purity → 75 iron ingots + 25 slag

**Quality based on:**
- Input purity (base)
- Artisan expertise & lessons (bonus)
- Business quality (equipment, workplace) (bonus)
- Tech tier (techniques) (bonus)

**Formula (Conceptual):**
```
BaseQuality = Purity × PurityCoefficient + ArtisanExpertise × ExpertiseCoefficient + BusinessQuality × BusinessCoefficient + TechTier × TechCoefficient
```

**Reference:** See [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed refining formulas. Formula coefficients live in `RefiningFormulaConfig` Blob so you can tweak them per world.

---

### Produced → End Products (Crafting Stage)

**Examples:**
- Ingots + wood/leather → weapons & armor
- Lumber + iron + hardware → wagons
- Cut stone + lumber + hardware → buildings
- Processed herbs + bottles + reagents → potions

**Core Rules:**

**Quality propagation:**
```
BaseQuality ≈ Average(ComponentQualities) × AverageCoefficient + ArtisanBonus + BusinessBonus + TechTierBonus
```

**Rarity:**
- Base from rarest components
- Occasionally upgraded (critical success) based on expertise/lessons

**Tech tier:**
- Limited by both artisan knowledge and component tech tiers
- Cannot exceed minimum of (artisan tech tier, lowest component tech tier)

**Durability:**
- Inherited from components
- Limited by weakest critical component ("chain is as strong as weakest link")

**Reference:** See [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) for detailed crafting formulas. Formula coefficients live in `CraftingFormulaConfig` and `DurabilityConfig`, not hardcoded.

---

### Business Operation Loop (Local Only)

Per business (e.g., Blacksmith, Sawmill, Mill), the production loop:

**1. Detect or Enqueue Demand:**
- Simple approaches:
  - Restock thresholds ("produce swords if we have < N in stock")
  - Contracts (village wants X tools)
- Real AI demand logic can come later

**2. Check Inputs & Capacity:**
- Are the required inputs present in business/storehouse inventories?
- Is there enough worker time & operating capital (Chunk 1) for wages?

**3. Reserve Inputs:**
- Mark required items as reserved to avoid double-use in other recipes
- Reservation prevents other jobs from consuming the same inputs

**4. Start Production Job:**
- Create `ProductionJob` (conceptually):
  - RecipeId
  - Input refs (reserved items)
  - Expected output type & quantity
  - Estimated duration
  - Worker assignment
  - Business entity

**5. Progress Over Time:**
- Time passes in working-hours / days
- Possible speed modifiers:
  - Artisan skill (faster with higher skill)
  - Business tooling (better equipment = faster)
  - Fatigue (workers get tired)

**6. Complete Job:**
- Remove input items from inventory (consumption)
- Add output items (produced material or end product)
- Write wealth-flow hooks:
  - Pay wages to worker(s) (Chunk 1 transaction)
  - Adjust BusinessBalance for costs & profit (Chunk 1 transaction)

**Note:** No markets, no dynamic prices. Cost can be approximated from base values or fixed recipe costs at this stage.

---

## Alignment with Spine Rules

### Single Source of Truth

**All resource/material/product quantities live in inventories (Chunk 2):**
- ✅ Extracted resources in storehouse inventories
- ✅ Produced materials in business/storehouse inventories
- ✅ End products in business/storehouse inventories
- ❌ No duplicate counters elsewhere

**Physical chunks exist only in world sim until deposit:**
- ✅ After deposit, only the inventory counts matter
- ✅ Chunk entities are despawned/pooled after deposit

**All wealth changes due to production/wages use Chunk 1 transactions:**
- ✅ Wages paid via transactions
- ✅ Profit/loss recorded via transactions
- ❌ No ad-hoc wealth modifications

**Quality/rarity/tech-tier are properties on items:**
- ✅ Stored as item metadata
- ❌ Not duplicated across systems

---

### Layering

**Chunk 3 reads:**
- ✅ Inventories (Chunk 2)
- ✅ Wallets/wealth (Chunk 1)
- ✅ Artisan/business stats, lessons, guild standards

**Chunk 3 does NOT:**
- ❌ Read or write market prices (Chunk 5)
- ❌ Alter trade routes or policies (Chunks 4 & 6)
- ❌ Do cross-village price-based decisions (Chunks 4+5+6)

**If a production decision needs prices:**
- Will later consult Chunk 5
- In Chunk 3, rely on base costs and simple "keep stock healthy" heuristics

---

### Data > Code

**Moddable catalogs:**
- ✅ `ItemSpec` (already in Chunk 2)
- ✅ `ProductionRecipe` (refining, crafting, enchanting)
- ✅ `RefiningFormulaConfig`, `CraftingFormulaConfig`, `DurabilityConfig`
- ✅ `BusinessTypeSpec` (capacity, efficiency, supported recipes)

**Systems are generic:**
- ✅ "For each job, read recipe, apply formulas, move inventory items, write outputs and wealth transactions"
- ❌ No hardcoded item types or recipes

**Example:**
```
✅ CORRECT - Generic system:
ProductionSystem reads ProductionRecipe catalog
Applies RefiningFormulaConfig coefficients
Moves items via Chunk 2 inventory APIs
Records transactions via Chunk 1 transaction APIs

❌ WRONG - Hardcoded:
if (recipe == "iron_ore_to_ingot") {
  quality = purity * 0.6 + skill * 0.3;  // Hardcoded!
}
```

---

## Debug & Telemetry

Chunk 3 should be inspectable similar to Chunks 1 & 2:

### Per Business View

**Current jobs:**
- Recipe being executed
- Inputs consumed
- Outputs produced
- Progress (time remaining)

**Throughput metrics:**
- Items/day/week
- Quality distribution of outputs
- Success/failure rates

**Recent job history:**
- Last N jobs completed
- Quality distribution
- Time taken per job

**Example:**
```
Blacksmith_123:
  Current Jobs:
    - Job_1: Recipe "iron_ore_to_ingot", Progress: 60%, ETA: 2 hours
    - Job_2: Recipe "sword_craft", Progress: 20%, ETA: 6 hours
  
  Throughput (Last Week):
    - Iron Ingots: 150/day
    - Swords: 5/day
    - Average Quality: 72/100
  
  Recent Jobs:
    - Job_10: Produced 10 iron ingots, Quality: 75, Time: 8 hours
    - Job_9: Produced 1 sword, Quality: 80, Rarity: Uncommon, Time: 12 hours
```

---

### Per Settlement View

**Aggregate production:**
- "Last month: 200 lumber, 50 ingots, 10 swords"
- Grouped by material/product type
- Quality averages

**Input bottlenecks:**
- Recipes that fail due to missing inputs
- Most common missing inputs
- Suggestions for input acquisition

**Example:**
```
Village_456 Production (Last Month):
  Extracted → Produced:
    - Iron Ingots: 500 (Avg Quality: 70)
    - Lumber: 200 (Avg Quality: 75)
    - Flour: 1000 (Avg Quality: 65)
  
  Produced → End Products:
    - Swords: 10 (Avg Quality: 72)
    - Wagons: 2 (Avg Quality: 68)
  
  Bottlenecks:
    - Blacksmith: Missing iron ore (needs 200 more)
    - Wainwright: Missing hardware (needs 50 more)
```

---

### Per Artisan View

**Outputs they were responsible for:**
- Items produced
- Average quality
- Quality distribution

**Experience gained:**
- Skill progression
- Lessons applied
- Expertise increases

**Example:**
```
Artisan_789 (Blacksmith):
  Outputs (Last Month):
    - Iron Ingots: 150 (Avg Quality: 78)
    - Swords: 5 (Avg Quality: 82)
    - Quality Range: 70-90
  
  Experience:
    - Skill Level: 45 → 47
    - Lessons Applied: 3
    - Expertise Bonus: +15% quality
```

---

## "Done" Checklist for Chunk 3

Chunk 3 can be marked as "done enough" when all of the following are verified:

### ✅ Data & Definitions
- [ ] A `ProductionRecipe` catalog exists for at least:
  - One simple refining chain (e.g., iron ore → iron ingots)
  - One simple crafting chain (e.g., ingots + wood → basic tools or sword)
- [ ] Formula configs exist (even rough) for:
  - Refining quality from purity + skill
  - Crafting quality from component qualities + skill
  - Rarity/tech-tier rules (even if simplified from the full doc)

### ✅ Business Operation Loop
- [ ] Businesses can enqueue & run production jobs based on recipes
- [ ] Inputs are checked, reserved, and consumed from inventories (Chunk 2)
- [ ] Outputs are added back to inventories
- [ ] Wages are paid and basic profit/loss is written to wallets (Chunk 1)

### ✅ Integration with Chunks 1 & 2
- [ ] Physical resource chunks deposit into inventories as Extracted resources (with purity)
- [ ] Refining jobs consume those extracted resources and output produced materials
- [ ] Crafting jobs consume produced materials and create end products with quality fields used by combat/equipment where appropriate

### ✅ Layering & Constraints
- [ ] Chunk 3 systems never read or write market prices
- [ ] No cross-village trade logic is implemented in Chunk 3
- [ ] All mod-tweakable behavior (recipes, coefficients, tech tiers) lives in catalogs/configs

### ✅ Telemetry
- [ ] You can inspect at least one business and see:
  - Its current job queue
  - Inputs, outputs, and resulting quality

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Chunk 1**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) – Wealth layer for wages/profit
- **Chunk 2**: [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md) – Inventory layer for inputs/outputs
- **Business & Assets**: [`BusinessAndAssets.md`](BusinessAndAssets.md) – Business ownership and operations
- **Resource Production**: [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) – Detailed production mechanics and formulas

### PureDOTS Integration

Chunk 3 systems should integrate with PureDOTS patterns:

- **Resource Systems**: Use existing `ResourceTypeId` patterns where applicable
- **Storehouse API**: Build on existing storehouse inventory APIs
- **Registry Infrastructure**: Use entity registries for efficient business/job queries
- **Time/Rewind**: Use `TickTimeState` for job timestamps, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Production job processing should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villagers, workshops, blacksmiths, sawmills, production businesses
- **Space4X**: Factories, refineries, workshops, production facilities

Examples in this document use Godgame terminology (villagers, blacksmiths, workshops), but the same patterns apply to Space4X (factories, refineries, production facilities) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

