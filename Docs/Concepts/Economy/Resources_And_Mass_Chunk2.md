# Resources & Mass – Chunk 2

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 2  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md)), Chunk 1 ([Wealth & Ledgers](Wealth_And_Ledgers_Chunk1.md))  
**Feeds into:** Chunk 3 ([Businesses & Production](Businesses_And_Production_Chunk3.md)), Chunk 4 ([Trade & Logistics](Trade_And_Logistics_Chunk4.md)), Chunk 5 ([Markets & Prices](Markets_And_Prices_Chunk5.md)), Environment/Climate/Vegetation, Combat/Equipment/Courts  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 2 establishes the item/resource and mass layer of the simulation – the physical side of the economy. It answers a fundamental question:

**"What things exist, in what quantities, where are they, and how heavy/bulky are they?"**

This chunk provides the single source of truth for all physical goods. Everything above (businesses, trade, markets, policies) must use this layer to know what is actually there and what can be moved.

**Scope (what's in):**
- ✅ Resource & item definitions (types, categories, tags)
- ✅ Mass / volume / density for each resource/item
- ✅ Inventories (who/what is holding what)
- ✅ Capacity constraints (how much can a cart/warehouse/ship carry)
- ✅ Very basic flows: add/remove/move items

**Out-of-scope (future chunks):**
- ❌ Prices & value (Chunk 5)
- ❌ Business production logic (Chunk 3)
- ❌ Trade routes, caravans, ships (Chunk 4)
- ❌ Tax/tariff/sanction rules (Chunk 6)

**Compliance:** All systems in Chunk 2 must respect Chunk 0 principles:
- Single source of truth (all items in inventories)
- Layer boundaries (Layer 2 only – no wealth, markets, trade routes)
- Data > code (ItemSpec catalogs, capacity rules)

---

## Core Principles (Chunk 0 Alignment)

Chunk 2 must obey the three global rules from Chunk 0:

### Single Source of Truth for Items

**Rule:** All physical goods exist only as entries in inventories attached to entities or world containers.

**No invisible counters or ad-hoc fields:**
- ❌ No `grain = 500` fields in random components
- ❌ No hidden resource counters in businesses or villages
- ✅ All items exist as `InventoryItem` entries in `Inventory` components

**Example (Conceptual):**
```
❌ WRONG:
VillageStats {
  FoodSupply: 1000  // Ghost goods!
}

✅ CORRECT:
StorehouseEntity {
  Inventory {
    Items: [
      { ItemId: "grain", Quantity: 1000, Mass: 1000 * 0.5 }
    ]
  }
}
```

---

### Layers, Not Tangles

**Chunk 2 is Layer 2: Resources & Mass**

**Allowed dependencies:**
- ✅ Basic world data (map, entities)
- ✅ Static catalogs (ItemSpec, world configs)

**NOT allowed:**
- ❌ Wealth balances (Chunk 1)
- ❌ Markets (Chunk 5)
- ❌ Trade routes (Chunk 4)
- ❌ Tax policy (Chunk 6)

**Direction:** Higher layers read Chunk 2; Chunk 2 never reads them.

**Example:**
```
✅ CORRECT - Chunk 5 (Markets) reads Chunk 2:
MarketSystem reads Inventory to calculate supply

❌ WRONG - Chunk 2 reads Chunk 5:
InventorySystem reads MarketPrice to decide what to store
```

---

### Data > Code

**Rule:** Resource types, mass/volume, spoilage flags, legality, etc. all live in Blob catalogs.

**Systems are generic:**
- ✅ Move stacks, enforce capacity, apply catalog rules
- ❌ Nothing hard-coded

**Example:**
```
✅ CORRECT - Data-driven:
ItemSpec Catalog defines:
  - Grain: MassPerUnit = 0.5, StackSize = 1000
  - Sword: MassPerUnit = 2.0, StackSize = 1
System reads catalog and applies rules generically

❌ WRONG - Hard-coded:
if (item == "grain") mass = 0.5;
if (item == "sword") mass = 2.0;
```

---

## Resource & Item Model

### Taxonomy

Conceptually, every "thing" is either:

**1. Bulk Resource** – Indistinguishable units:
- Grain, flour, ore, coal, stone, lumber, water
- Stored as quantity (e.g., 1000 units of grain)
- StackSize typically large (100, 1000, etc.)

**2. Discrete Item** – Indistinguishable units:
- Tools, weapons, armor, books, jewelry, rare artifacts
- Stored as quantity (e.g., 5 swords)
- StackSize typically small (1, 5, 20, etc.)

**3. Structured Good** – Like a ship or cart (big item with its own components/inventories):
- Handled as entities with their own components
- May still have a primary "item spec" for transport purposes
- Can contain their own inventories (ship holds cargo)

**Unified Model:** We use a unified `ItemSpec` with flags for "bulk vs discrete vs structure" to handle all three types consistently.

---

### ItemSpec Catalog

Chunk 2 requires a data catalog that defines all items/resources:

**Catalog Structure:**
- `ItemSpec` entries for each item/resource type
- Each entry contains:

**Required Fields:**
- `ItemId` – Stable ID (FixedString64Bytes)
- `Name` / `Label` – Display name
- `Category` – Enum: Raw, Processed, Food, Fuel, Tool, Weapon, Armor, Luxury, Artifact, CargoContainer, etc.
- `MassPerUnit` – Weight per unit (float, kg)
- `VolumePerUnit` – Volume per unit (float, m³, optional)
- `StackSize` – Max units per stack entry (int, e.g., 1 for big weapons, 20 for potions, 1000 for grain)

**Optional Fields:**
- `Perishable` – Boolean, plus decay rate (optional; real logic later)
- `Durable` – Boolean, plus durability model (for equipment; logic can be future)
- `Tags` – Bitflags: Food, Flammable, Illegal, Sacred, Contraband, Luxury, MilitaryGrade, Rare, BulkOnly
- `BaseValue` – Base value (for reference; actual prices in Chunk 5)

**NOT Included:**
- ❌ Price (base value may exist, but used only in higher layers)
- ❌ Market dynamics
- ❌ Trade restrictions (those come from Chunk 6 policies)

---

### Moddability

**Different item catalogs per world/game are allowed:**

**Godgame Catalog:**
- Crops (wheat, barley, rice)
- Livestock products (milk, eggs, wool)
- Medieval tools (hammers, saws, plows)
- Ingots (iron, copper, gold)
- Weapons (swords, bows, spears)
- Armor (leather, chainmail, plate)

**Space4X Catalog:**
- Ores (iron, plasteel, rare earths)
- Alloys (steel, titanium, advanced composites)
- Fuel (hydrogen, antimatter)
- Components (circuits, engines, weapons)
- Starship modules (hulls, drives, weapons)
- Artifacts (alien tech, relics)

**Mod Support:**
- Mods can add new ItemSpecs with new categories & tags without any code changes
- Systems read catalog and apply rules generically
- No hard-coded item type checks

---

## Inventories & Ownership

### Inventory Containers

**Rule:** Any entity that can "hold things" may have an inventory.

**Container Types:**

**1. Individuals**
- Villagers, crew, heroes
- Personal inventories (limited capacity)
- May have equipment slots (weapons, armor)

**2. Buildings**
- Shops, warehouses, granaries, armories
- Large-capacity inventories
- May have specialized storage (food-only, weapons-only)

**3. Vehicles**
- Carts, wagons, ships, carriers
- Mobile inventories with capacity limits
- Mass affects movement speed (handled in higher layers)

**4. Aggregates**
- Village stockpile, colony supply, guild vault
- Can be implemented as:
  - True aggregate entity with inventory, OR
  - Computed view of member inventories

**Unified Model:** All inventories share a single conceptual model, even if you implement multiple flavors (e.g., limited-slot personal vs large-capacity warehouse).

---

### Inventory Entry Model

Conceptually, each inventory is a list of entry stacks:

**Core Fields:**
- `ItemId` – Which item/resource (FixedString64Bytes)
- `Quantity` – How many units (float or int, depending on item type)

**Optional Fields:**
- `Quality` – Quality tier or float (Normal/Good/Excellent; or 0–1)
- `Durability` – For equipment (current durability / max durability)
- `CustomFlags` – Bound to owner, cursed, stolen, etc. (bitflags)

**Constraints:**
- `Quantity ≤ ItemSpec.StackSize`
- For bulk resources, quantity may be large numbers (but store efficiently)
- Discrete items often have `StackSize = 1` or small

**Example Structure (Conceptual):**
```
Inventory {
  Items: [
    { ItemId: "grain", Quantity: 1000, Mass: 500 },
    { ItemId: "iron_ingot", Quantity: 50, Mass: 250 },
    { ItemId: "sword", Quantity: 1, Quality: 0.8, Durability: 0.9, Mass: 2.0 }
  ],
  TotalMass: 752.0,
  MaxMass: 1000.0
}
```

---

### Inventory Capacity

Capacity constraints are data-driven:

**For each container type** (person, cart, wagon, ship, building):

**Capacity Fields:**
- `MaxMass` – Carrying capacity / load limit (float, kg)
- `MaxVolume` – Volume constraint (float, m³, optional)
- `SlotCount` – If you want slots (personal equipment, quickbars, optional)
- `SpecialRules` – E.g., "only Food & Fuel allowed", "only Ammo & Weapons"

**System Enforcement:**
- ✅ `Sum(MassPerUnit × Quantity) ≤ MaxMass`
- ✅ `Sum(VolumePerUnit × Quantity) ≤ MaxVolume` (if used)
- ✅ Slot rules are obeyed if implemented

**Example (Conceptual):**
```
VillagerInventory {
  MaxMass: 50.0,        // Can carry 50 kg
  SlotCount: 10,        // 10 equipment slots
  SpecialRules: []      // No restrictions
}

WarehouseInventory {
  MaxMass: 10000.0,     // Can store 10,000 kg
  MaxVolume: 500.0,     // 500 m³ volume
  SlotCount: 0,         // No slots, bulk storage
  SpecialRules: []      // No restrictions
}

FoodGranaryInventory {
  MaxMass: 5000.0,
  MaxVolume: 200.0,
  SlotCount: 0,
  SpecialRules: ["Food", "BulkOnly"]  // Only food items, bulk resources only
}
```

**Implementation Note:** Chunk 2 defines the rules, even if initial implementation is lenient while prototyping. Over-capacity behavior should be at least debug-visible (warnings or flags).

---

## Mass & Movement

Mass is crucial for:
- **Trade & logistics (Chunk 4):** Caravan speed, ship fuel usage, travel risk
- **Combat & movement:** Encumbrance, squad speeds
- **Environment:** Supply loads into harsh climates, etc.

---

### Mass Computation Rules

**Total mass of an inventory:**
```
TotalMass = Σ (ItemSpec.MassPerUnit × Quantity)
```

**Example:**
```
Inventory has:
  - 1000 grain (MassPerUnit = 0.5) → 500 kg
  - 50 iron ingots (MassPerUnit = 5.0) → 250 kg
  - 1 sword (MassPerUnit = 2.0) → 2 kg
TotalMass = 752 kg
```

**Encumbrance/Limit Checks:**
- Entities/vehicles have a `MaxCarryMass`
- Movement speed drops or operations fail when over limit
- **Rules for this live in higher layers** (movement, combat, logistics), but Chunk 2 provides the mass

**Example:**
```
Villager has:
  CurrentMass: 60 kg
  MaxCarryMass: 50 kg
  OverCapacity: true

Chunk 2: Provides mass calculation
Chunk 4 (Logistics): Uses mass to calculate movement speed penalty
Chunk 3 (Combat): Uses mass to calculate encumbrance penalties
```

---

### Gravity / Environment

**For Space4X, environment might alter "effective weight":**

**Chunk 2 Responsibility:**
- ✅ Stores mass only
- ✅ Provides mass reliably

**Higher Layers Responsibility:**
- ✅ Combine `mass × local gravity` for effective weight
- ✅ Combine with planetary atmosphere for re-entry constraints
- ✅ Apply environmental modifiers (low gravity, high gravity)

**Example:**
```
Chunk 2: Cargo mass = 1000 kg

Chunk 4 (Logistics) on Space4X:
  Local gravity = 0.5g (moon)
  Effective weight = 1000 × 0.5 = 500 kg
  Movement cost based on effective weight

Chunk 2 doesn't care about gravity values; it just supplies mass reliably.
```

---

## Flows in Chunk 2

Chunk 2 supports basic item flows, independent of wealth/prices:

### Spawn / Despawn

**Scenario Creation:**
- Items spawned into inventories via scenario data
- Explicit "ResourceSpawn" event logged

**Resource Generation:**
- Environment generates resources (trees grow, mines produce)
- God miracles create items
- Explicit "ResourceSpawn" event logged

**Destruction:**
- Fire consumes items
- Consumption (eating food, using fuel)
- Decay (perishable items spoil)
- Explicit "ResourceDestroy" event logged

**Rule:** All spawn/despawn operations must be explicit and logged.

**Example (Conceptual):**
```
Spawn Operation:
  Target: StorehouseInventory
  ItemId: "grain"
  Quantity: 1000
  Source: "scenario_setup"
  Event: ResourceSpawn { ItemId: "grain", Quantity: 1000, Target: StorehouseEntity }

Destroy Operation:
  Target: VillagerInventory
  ItemId: "bread"
  Quantity: 1
  Source: "consumption"
  Event: ResourceDestroy { ItemId: "bread", Quantity: 1, Target: VillagerEntity }
```

---

### Move

**Between Inventories:**
- Person → chest
- Chest → cart
- Cart → warehouse
- Ship → colony

**Spatial (if you track world position separately):**
- Items are where their inventory-holder is
- Moving an entity moves its inventory
- No separate item position tracking needed

**Operation:**
```
Move Operation:
  From: SourceInventory
  To: DestinationInventory
  ItemId: "grain"
  Quantity: 100
  Check capacity before move
  Remove from source, add to destination
```

**Example:**
```
Move 100 grain from VillagerInventory to CartInventory:
  1. Check CartInventory has capacity for 100 grain (50 kg)
  2. Remove 100 grain from VillagerInventory
  3. Add 100 grain to CartInventory
  4. Update mass calculations for both inventories
```

---

### Transform (Atomic)

**Stub for Production (actual recipes in Chunk 3):**

**Example:** Debug/dev transform "turn 10 wood into 1 plank"

**Operation:**
```
Transform Operation:
  Source: Inventory
  Consume: 10 wood
  Produce: 1 plank
  Atomic: All-or-nothing (if can't produce, don't consume)
```

**Implementation:**
1. Check source has 10 wood
2. Check destination has capacity for 1 plank
3. Remove 10 wood
4. Add 1 plank
5. No wealth changes yet (that's Chunk 3)

**Rule:** All flows are stack operations within inventories; no other representation.

**Note:** Full production system comes in Chunk 3. This is just a minimal transform to prove inventory operations work.

---

## Integration Boundaries

### With Chunk 1 (Wealth)

**Chunk 2 must NOT:**
- ❌ Read or write wealth balances
- ❌ Decide whether something is worth doing in money terms

**Higher Layers Can:**
- ✅ Trigger item flows and, separately, wealth transactions that correspond to buy/sell operations (Chunk 5)

**Example (Future Chunk 5 Behavior):**
```
Buy 100 grain:
  1. Chunk 5: Record wealth transaction (buyer → seller, amount = price × 100)
  2. Chunk 2: Move 100 grain from seller inventory to buyer inventory

Right now (Chunk 2), you only implement the second part – the move – without any money.
```

**Separation:** Chunk 2 handles items; Chunk 1 handles wealth. They interact only through higher layers (Chunk 5: Markets).

---

### With Businesses (Chunk 3)

**Businesses use inventories to:**
- Consume inputs (remove items from their inventory)
- Produce outputs (add items to their inventory)

**Chunk 2 Responsibility:**
- ✅ Defines inventory semantics
- ✅ Provides add/remove/move operations

**Chunk 3 Responsibility:**
- ✅ Decides when and how inventories change
- ✅ Applies production recipes
- ✅ Handles business logic

**Example:**
```
Chunk 2: Provides Inventory.AddItem(), Inventory.RemoveItem()
Chunk 3: BusinessProductionSystem calls these methods based on recipes
```

---

### With Trade (Chunk 4)

**Caravans / Ships / Carriers:**
- Are entities with inventories and capacity limits
- Trade routes just move those inventories around

**Chunk 2 Responsibility:**
- ✅ Provides inventory and capacity model
- ✅ Provides mass calculations

**Chunk 4 Responsibility:**
- ✅ Uses inventories for caravans/ships
- ✅ Uses mass to calculate movement speed
- ✅ Moves inventories along trade routes

**Example:**
```
Chunk 2: CaravanEntity has Inventory { MaxMass: 2000 kg }
Chunk 4: TradeRouteSystem moves CaravanEntity (and its inventory) between locations
Chunk 2: Provides mass for speed calculations
```

---

### With Markets & Policies (Chunks 5–6)

**Markets:**
- Read what's in stock in local inventories to derive supply
- Chunk 2 provides read-only access to inventories

**Policies:**
- Define which `ItemSpec.Tags` are taxed, forbidden, or subsidized
- Chunk 2 provides tags; Chunk 6 applies policy rules

**Chunk 2 Responsibility:**
- ✅ Stores items with tags (Food, Contraband, etc.)
- ✅ Provides read access to inventories

**Chunk 5–6 Responsibility:**
- ✅ Read inventories to calculate supply
- ✅ Apply policy rules based on tags

**Example:**
```
Chunk 2: ItemSpec { ItemId: "grain", Tags: ["Food", "BulkOnly"] }
Chunk 5: MarketSystem reads inventories, sees grain tagged as Food, calculates supply
Chunk 6: PolicySystem sees grain tagged as Food, applies food tax policy
```

**Chunk 2 doesn't know about any of that; it only knows there's item X with tag Contraband/Food/etc.**

---

## Telemetry & Debug

To make Chunk 2 usable and debuggable:

### Per-Entity View

**What's in this inventory?**
- List all items with quantities
- Total mass / volume
- Over/under capacity flags
- Capacity utilization percentage

**Example:**
```
VillagerEntity_123 Inventory:
  Items:
    - grain: 50 (25 kg)
    - bread: 5 (2.5 kg)
    - sword: 1 (2 kg)
  Total Mass: 29.5 kg
  Max Mass: 50 kg
  Utilization: 59%
  Status: OK
```

---

### Per-Location View (for Settlements/Colonies)

**Aggregate counts & masses per ItemId or category:**
- "We have 10,000 units of grain, 2,000 of iron ingots, 300 swords"
- Grouped by category (Food, Materials, Weapons, etc.)
- Total mass per category

**Example:**
```
Village_456 Aggregate Inventory:
  Food:
    - grain: 10,000 (5,000 kg)
    - bread: 500 (250 kg)
    Total: 5,250 kg
  
  Materials:
    - iron_ingot: 2,000 (10,000 kg)
    - wood: 5,000 (2,500 kg)
    Total: 12,500 kg
  
  Weapons:
    - sword: 300 (600 kg)
    - bow: 150 (75 kg)
    Total: 675 kg
```

---

### Global View

**Where is all of resource X?**
- Query all inventories for specific ItemId
- List entities holding that resource
- Total quantity across all inventories
- Useful for balancing and scenario design

**Example:**
```
Global Query: "grain"
Results:
  - Village_456 Storehouse: 10,000
  - Villager_123: 50
  - Cart_789: 200
  Total: 10,250 units
```

**Are there unreachable/stranded items?**
- Items in destroyed entities
- Items in inaccessible locations
- Flag for cleanup/debugging

---

### Optional Early Metric

**"Economic weight" of a village:**
- Sum of mass of top N "value-bearing" categories
- Even without prices, this helps you see how material flows might behave
- Useful for balancing and scenario design

**Example:**
```
Village Economic Weight:
  Food: 5,250 kg
  Materials: 12,500 kg
  Weapons: 675 kg
  Total: 18,425 kg
  
  This village is "material-heavy" (lots of raw materials, less food)
```

---

## "Done" Checklist for Chunk 2

Chunk 2 can be marked as "done enough" when all of the following are verified:

### ✅ Resource/Item Definitions
- [ ] An ItemSpec catalog exists with:
  - IDs, names, categories, tags
  - MassPerUnit and (optionally) VolumePerUnit
  - StackSize
- [ ] Core game items (food, basic materials, tools, simple equipment) are defined via this catalog
- [ ] Catalog is moddable (can add new items without code changes)

### ✅ Inventories & Ownership
- [ ] A generic inventory model is agreed and implemented (conceptually: entries with ItemId + Quantity)
- [ ] Major entity types that should hold items have inventories defined:
  - Individuals (if needed at this stage)
  - Businesses / buildings
  - Village/colony stockpiles
  - Basic vehicles/carts/ships (even if only test examples)

### ✅ Capacity & Mass
- [ ] Carrying capacity rules (MaxMass [+ MaxVolume if used]) are defined per container type
- [ ] Total mass calculation is implemented and used in capacity checks
- [ ] Over-capacity behavior is at least debug-visible (warnings or flags)

### ✅ Basic Flows Working
- [ ] Items can be spawned and despawned in inventories via explicit operations
- [ ] Items can be moved between inventories (person ↔ building, building ↔ cart, etc.)
- [ ] A simple transform (consume X of A, produce Y of B) can be executed via inventory operations only (no wealth yet)

### ✅ Debug/Telemetry
- [ ] There is at least one debug UI/log to inspect inventories for entities/settlements
- [ ] You can compute, for a settlement, total mass and counts for key resources
- [ ] Global queries work (where is all of resource X?)

### ✅ Layer & Principles Compliance
- [ ] Chunk 2 does not read or write wealth balances
- [ ] No prices or market logic appear in Chunk 2
- [ ] All item properties that might be modded are driven by the ItemSpec catalog and its related configs
- [ ] Chunk 2 respects Chunk 0 principles (single source of truth, layers, data>code)

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Chunk 1**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) – Wealth layer (independent but complementary)
- **Resource Production**: [`Resource_Production_And_Crafting.md`](Resource_Production_And_Crafting.md) – Production recipes (Chunk 3)
- **Business & Assets**: [`BusinessAndAssets.md`](BusinessAndAssets.md) – Business inventory usage (Chunk 3)
- **Trade & Commerce**: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) – Trade routes using inventories (Chunk 4)

### PureDOTS Integration

Chunk 2 systems should integrate with PureDOTS patterns:

- **Resource Systems**: Use existing `ResourceTypeId` patterns where applicable
- **Storehouse API**: Build on existing storehouse inventory APIs if available
- **Registry Infrastructure**: Use entity registries for efficient inventory queries
- **Time/Rewind**: Use `TickTimeState` for spawn/despawn timestamps, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Inventory operations should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villagers, villages, carts, storehouses, granaries
- **Space4X**: Carriers, colonies, ships, cargo holds, warehouses

Examples in this document use Godgame terminology (villagers, villages, carts), but the same patterns apply to Space4X (carriers, colonies, ships) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

