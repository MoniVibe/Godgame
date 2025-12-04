# Markets & Prices – Chunk 5

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 5  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md)), Chunk 1 ([Wealth & Ledgers](Wealth_And_Ledgers_Chunk1.md)), Chunk 2 ([Resources & Mass](Resources_And_Mass_Chunk2.md)), Chunk 3 ([Businesses & Production](Businesses_And_Production_Chunk3.md)), Chunk 4 ([Trade & Logistics](Trade_And_Logistics_Chunk4.md))  
**Feeds into:** Chunk 6 ([Policies & Macro](Policies_And_Macro_Chunk6.md)), Strategic AI (village/guild/fleet economic decisions), UI (price displays, economic heatmaps)  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 5 establishes the pricing layer of the economy – the system that determines what goods are worth in each location. It answers a fundamental question:

**"Given goods, transport, and wealth, what are things worth in each place right now – and how do local conditions shift those prices over time?"**

This chunk creates:
- Per-village/colony market state (prices, supply, demand)
- Dynamic price updates from supply/demand, wealth, and events (war, famine, festivals, etc.)
- A simple market interface for businesses, villagers, guilds, and routes to buy/sell at market
- Trade AI (later) to spot arbitrage and export opportunities

**Compliance:** All systems in Chunk 5 must:
- ✅ Read inventories for supply (Chunk 2)
- ✅ Use wealth transactions for buy/sell (Chunk 1)
- ✅ Observe production for supply signals (Chunk 3)
- ✅ Observe trade arrivals for supply updates (Chunk 4)
- ✅ Be agnostic about tariffs/embargoes/policies (Chunk 6)

**Reference:** This implements the "Market Pricing & Supply/Demand" part of [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) (base prices, dynamic pricing, arbitrage).

**Note:** Chunk 5 does not own trade routes, tariffs, or debt – it just tells everyone "here's the going rate" and executes local buy/sell.

---

## Scope & Boundaries

### In Scope (Chunk 5)

**Market representation per node (village/city/colony):**
- Which goods are tracked
- Base price vs current price
- Supply & demand metrics

**Dynamic pricing logic:**
- Supply/demand multiplier
- Village-wealth multiplier
- Event multipliers (war, famine, festival, plague, etc.)

**Market interaction interface:**
- "I want to buy X units of Y" → attempt to get it at the market price
- "I want to sell X units of Y" → convert goods to wealth at market price
- Simple local clearing – no complex order books

**Arbitrage signals:**
- Price differences between markets exposed for AI (and you) to exploit

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed market mechanics and pricing formulas.

---

### Out of Scope (For Later Chunks)

**Tariffs & taxes on trades (Chunk 6):**
- ❌ Chunk 5 provides base prices
- ❌ Chunk 6 applies tariff multipliers

**Trade agreements, embargoes, smuggling rules (Chunk 6):**
- ❌ Chunk 5 doesn't block trades
- ❌ Chunk 6 applies embargo/sanction rules

**Inter-village loans & debt-driven vassalization (Chunk 6):**
- ❌ Chunk 5 doesn't handle debt
- ❌ Chunk 6 manages loans and debt mechanics

**Full-blown merchant/fleet strategic AI:**
- ❌ Chunk 5 provides pricing oracle
- ❌ Strategic AI (separate system) uses Chunk 5 prices to make decisions

**Chunk 5 is the pricing oracle + local exchange, nothing more.**

---

## Core Concepts

### Markets as "Views" Over Inventories

**A market is not a second store of goods; it's a structured view over:**

**Local inventories (Chunk 2):**
- Stockpiles, shops, warehouses, guild vaults
- Markets aggregate what's available for sale

**Local consumption & production patterns (Chunks 1 & 3):**
- Production creates supply
- Consumption creates demand
- Markets observe these patterns

**Local trade flows (arrivals from Chunk 4):**
- Trade arrivals increase supply
- Trade departures decrease supply
- Markets observe trade flows

**Markets store aggregated stats and prices, not the goods themselves.**

**Example:**
```
VillageA Market:
  Grain Supply: 5000 units (from VillageA Stockpile inventory)
  Grain Demand: 3000 units/month (from consumption patterns)
  Grain Price: 0.5 currency/unit (computed from supply/demand)

The grain itself is still in VillageA Stockpile inventory (Chunk 2).
The market just tracks how much is available and what it's worth.
```

---

### Good Types vs Specific Items

**For performance & clarity, Chunk 5 prices GoodTypes, not every concrete item:**

**Example GoodTypes (as in MarketPrice):**
- IronOre, IronIngot, SteelIngot
- Wheat, Flour, Bread
- Weapon, Armor, Tool
- Luxury, Artifact

**A mapping from ItemSpec → GoodType decides which bucket an item falls into:**
- All common longswords might map to `GoodType.Weapon`
- All iron ingots map to `GoodType.IronIngot`
- You can later extend to quality bands (`Weapon_Common`, `Weapon_Rare`)

**This is data-driven, so mods can define new GoodTypes and mappings.**

**Example:**
```
ItemSpec → GoodType Mapping:
  "longsword_common" → GoodType.Weapon
  "longsword_rare" → GoodType.Weapon_Rare (optional quality band)
  "iron_ingot" → GoodType.IronIngot
  "steel_ingot" → GoodType.SteelIngot

Market prices GoodType.Weapon, not individual sword variants.
```

---

## Data Model

### Base Prices Catalog

Base prices from the Trade doc become a shared `BasePrice` catalog:

**For each GoodType (or ItemSpec):**
- `BasePrice` – Currency per kg or per unit
- Stored in a Blob so different worlds/scenarios can ship different base prices

**These are the "neutral, balanced" prices before any dynamic multipliers.**

**Example (Conceptual):**
```
BasePrice Catalog:
  GoodType.IronOre: 1.0 currency/kg
  GoodType.IronIngot: 5.0 currency/kg
  GoodType.Wheat: 0.2 currency/kg
  GoodType.Flour: 0.5 currency/kg
  GoodType.Weapon: 50.0 currency/unit
  GoodType.Armor: 100.0 currency/unit
```

**Moddability:** Different worlds/scenarios can have different base prices (medieval vs space age, different economic scales).

---

### Per-Village MarketPrice Buffer

Reuse and formalize the MarketPrice buffer from the Trade doc:

**Each village/colony has a buffer:**

**Required Fields:**
- `GoodType` – Which good this entry tracks
- `BasePrice` – Base price from catalog
- `CurrentPrice` – Computed current price
- `Supply` – Effective local supply metric for this good
- `MonthlyDemand` – Expected monthly demand/consumption

**Derived Fields (for debugging):**
- `SupplyDemandRatio` – Supply / Demand
- `SupplyDemandMultiplier` – Computed multiplier from ratio
- `VillageWealthMultiplier` – Multiplier from village wealth
- `EventMultiplier` – Multiplier from current events

**Example (Conceptual):**
```
VillageA MarketPrice Buffer:
  [
    { GoodType: IronIngot, BasePrice: 5.0, CurrentPrice: 6.5, Supply: 1000, MonthlyDemand: 1500, SupplyDemandRatio: 0.67, SupplyDemandMultiplier: 1.3, VillageWealthMultiplier: 1.0, EventMultiplier: 1.0 },
    { GoodType: Wheat, BasePrice: 0.2, CurrentPrice: 0.15, Supply: 5000, MonthlyDemand: 3000, SupplyDemandRatio: 1.67, SupplyDemandMultiplier: 0.75, VillageWealthMultiplier: 1.0, EventMultiplier: 1.0 },
    { GoodType: Weapon, BasePrice: 50.0, CurrentPrice: 75.0, Supply: 50, MonthlyDemand: 100, SupplyDemandRatio: 0.5, SupplyDemandMultiplier: 1.5, VillageWealthMultiplier: 1.0, EventMultiplier: 1.0 }
  ]
```

**Chunk 5 defines how Supply & MonthlyDemand are measured (see Supply & Demand Measurement section).**

---

### Pricing Config (Moddable)

Instead of hardcoding `(Demand / Supply)^0.5`, we define a `MarketPricingConfig` Blob:

**Required Fields:**
- `SupplyDemandExponent` (alpha) – e.g., 0.5 (sqrt)
- `MultiplierCaps` – Min/max bounds (e.g., 0.2–5.0)
- `VillageWealthBreakpoints` – Array of (wealth threshold, multiplier)
- `DefaultEventMultipliers` – Map of (event type, multiplier):
  - War: 1.5
  - Famine: 2.0
  - Festival: 0.8
  - Plague: 1.3
  - Siege: 1.8

**This lets each game / scenario tweak responsiveness and volatility.**

**Example (Conceptual):**
```
MarketPricingConfig {
  SupplyDemandExponent: 0.5,  // sqrt
  MultiplierCaps: { Min: 0.2, Max: 5.0 },
  VillageWealthBreakpoints: [
    { Wealth: 0, Multiplier: 0.8 },      // Poor villages: cheaper
    { Wealth: 1000, Multiplier: 1.0 },   // Average: baseline
    { Wealth: 5000, Multiplier: 1.2 }    // Rich villages: more expensive
  ],
  DefaultEventMultipliers: {
    War: 1.5,
    Famine: 2.0,
    Festival: 0.8,
    Plague: 1.3,
    Siege: 1.8
  }
}
```

---

## Supply & Demand Measurement

This is the heart of Chunk 5: how we compute Supply and MonthlyDemand for each GoodType.

### Supply

Supply should reflect "how much of this good is reasonably accessible for sale/use in this market."

**Sources:**

**1. Local inventories:**
- Sum of all items of that GoodType in:
  - Public stockpiles, markets, business inventories, guild shops
- Optionally exclude:
  - "Reserved" items (already allocated to jobs/builds)
  - Strategic reserves (flagged not-for-sale)

**2. In-transit arrivals (optional tightening later):**
- If you want, consider shipments due to arrive soon from Chunk 4 as part of supply expectations
- For Chunk 5's first pass, it's enough to use current local stock as supply

**Example:**
```
VillageA Supply Calculation (Grain):
  VillageA Stockpile: 3000 units
  VillageA Granary: 2000 units
  VillageA Market: 0 units (sold out)
  Total Supply: 5000 units

  Excluded:
    - Reserved for mill: 500 units (not for sale)
    - Strategic reserve: 1000 units (not for sale)
  
  Available Supply: 3500 units
```

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed supply calculation methods.

---

### Demand

Demand is trickier but we can approximate based on:

**1. Historical consumption:**
- Track how much of each GoodType was consumed/converted per month:
  - Food eaten
  - Materials used in production
  - Items equipped / removed from stock
- Use smoothed consumption (EMA over last 3–6 months)

**2. Projected needs:**
- From village "needs & priorities" (food, security, infra, luxury) in Trade doc
- Each category contributes an expected consumption amount

**Simplified formula:**
```
MonthlyDemand = HistoricalConsumptionSmoothed + ProjectedNeed
```

**When you don't have enough telemetry yet (early implementation), you can seed demand from population & businesses:**
- Food demand: `population × food-per-capita`
- Weapon demand: `guard count × weapons-per-guard, plus war modifiers`
- Material demand: `target production scenarios`

**Example:**
```
VillageA Demand Calculation (Grain):
  Historical Consumption (EMA, 3 months):
    Month 1: 2500 units
    Month 2: 2800 units
    Month 3: 3000 units
    Smoothed: 2767 units
  
  Projected Need:
    Population: 200
    Food per capita: 10 units/month
    Base need: 2000 units
    War modifier: +500 units (siege preparation)
    Total projected: 2500 units
  
  MonthlyDemand: 2767 + 2500 = 5267 units
```

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed demand calculation methods.

---

## Price Update System

### Pricing Formula (Data-Driven)

We adopt the doc's formula but parameterized:

```
MarketPrice = BasePrice
              × SupplyDemandMultiplier
              × VillageWealthMultiplier
              × EventMultiplier
```

**Where:**

**SupplyDemandMultiplier:**
```
SupplyDemandRatio = Supply / max(1, Demand)
SupplyDemandMultiplier = (Demand / Supply)^alpha
  where alpha = MarketPricingConfig.SupplyDemandExponent
```

**VillageWealthMultiplier:**
- Derived from average wealth in Chunk 1 / VillageStats
- Look up multiplier from `VillageWealthBreakpoints` based on average wealth

**EventMultiplier:**
- From current events & conditions (war, famine, siege, festival, plague, etc.)
- Look up multiplier from `DefaultEventMultipliers` based on active events

**Example:**
```
VillageA Grain Price:
  BasePrice: 0.2 currency/kg
  Supply: 5000 units
  Demand: 3000 units/month
  SupplyDemandRatio: 1.67
  SupplyDemandMultiplier: (3000 / 5000)^0.5 = 0.775
  
  Average Wealth: 1500 currency (Mid tier)
  VillageWealthMultiplier: 1.0 (baseline)
  
  Active Events: None
  EventMultiplier: 1.0
  
  CurrentPrice: 0.2 × 0.775 × 1.0 × 1.0 = 0.155 currency/kg
  (Surplus → lower price)

VillageB Grain Price (Famine):
  BasePrice: 0.2 currency/kg
  Supply: 500 units
  Demand: 3000 units/month
  SupplyDemandRatio: 0.167
  SupplyDemandMultiplier: (3000 / 500)^0.5 = 2.45
  
  Average Wealth: 800 currency (Poor tier)
  VillageWealthMultiplier: 0.9
  
  Active Events: Famine
  EventMultiplier: 2.0
  
  CurrentPrice: 0.2 × 2.45 × 0.9 × 2.0 = 0.882 currency/kg
  (Shortage + famine → much higher price)
```

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed pricing formulas and examples.

---

### MarketPricingSystem

Runs at a coarse cadence (e.g., monthly or every X ticks), not every frame:

**For each village:**

**1. Gather:**
- MarketPrice buffer
- VillageStats (avg wealth, population, events)
- Supply & demand metrics for each GoodType

**2. For each MarketPrice entry:**
- Compute `SupplyDemandRatio = Supply / max(1, Demand)`
- Compute `SupplyDemandMultiplier` using config exponent
- Compute `VillageWealthMultiplier` from avg wealth thresholds
- Compute `EventMultiplier` from events (as per the Trade doc)
- Clamp multipliers to config caps
- Compute `CurrentPrice = BasePrice × multipliers`
- Store updated price & derived fields back into buffer

**This is basically the MarketPricingSystem in the doc, but generalized and parameterized.**

**Example (Conceptual):**
```
MarketPricingSystem Update Cycle (Monthly):
  For each village:
    For each GoodType in MarketPrice buffer:
      1. Query Supply from inventories (Chunk 2)
      2. Query Demand from consumption/projection
      3. Read BasePrice from catalog
      4. Read VillageStats (wealth, events)
      5. Compute multipliers
      6. Compute CurrentPrice
      7. Update MarketPrice buffer entry
```

---

## Market Interaction: Buy & Sell

Chunk 5 also defines a minimal local exchange API.

### Intent Components

Entities that want to use the market create intent components:

**MarketBuyIntent:**
- `Who` – Entity, wallet, and target market (village)
- `GoodType` & `DesiredQuantity`
- `MaxPrice` – Max price they are willing to pay (optional; default = current price)

**MarketSellIntent:**
- `Who` – Entity, inventory, and target market
- `GoodType` & `QuantityOffered`
- `MinPrice` – Min price they are willing to accept (optional; default = current price)

**These are simple, Burst-safe components (no need to get super fancy).**

**Example (Conceptual):**
```
MarketBuyIntent {
  Buyer: VillagerEntity_123,
  BuyerWallet: VillagerWealth_123,
  TargetMarket: VillageA,
  GoodType: Grain,
  DesiredQuantity: 100,
  MaxPrice: 0.3  // Willing to pay up to 0.3 currency/kg
}

MarketSellIntent {
  Seller: BusinessEntity_456,
  SellerInventory: BusinessInventory_456,
  TargetMarket: VillageA,
  GoodType: Grain,
  QuantityOffered: 500,
  MinPrice: 0.1  // Willing to sell for at least 0.1 currency/kg
}
```

---

### Local Clearing System

**MarketClearingSystem:**

Runs after prices are updated, but before/alongside logistics & production.

**For each market:**

**1. Collect all MarketBuyIntent & MarketSellIntent targeting this village**

**2. At MVP, treat market as price-taker:**
- Everyone trades at `CurrentPrice` if:
  - Buyers' `MaxPrice ≥ CurrentPrice`
  - Sellers' `MinPrice ≤ CurrentPrice`

**3. Match volume:**
- If total offered ≥ total demanded:
  - All buyers filled; sellers filled partially by some rule (pro-rata or FIFO)
- If demand > supply:
  - Sellers fully filled; buyers filled partially

**4. Execute trades:**

For each match:
- **Chunk 1:** Wealth transaction (`BuyerWallet → SellerWallet`, amount = `qty × CurrentPrice`)
- **Chunk 2:** Inventory move (`Seller inventory → Buyer inventory`)

**5. Update demand/supply telemetry (optional):**
- Use executed trades as part of next period's demand/supply metrics

**This keeps Chunk 5 as the only layer that directly combines wealth + items + prices in a market context.**

**Example:**
```
VillageA Market Clearing:
  CurrentPrice (Grain): 0.2 currency/kg
  
  Buy Intents:
    - Buyer1: 100 units, MaxPrice: 0.3 ✅
    - Buyer2: 200 units, MaxPrice: 0.15 ❌ (below current price)
  
  Sell Intents:
    - Seller1: 500 units, MinPrice: 0.1 ✅
    - Seller2: 300 units, MinPrice: 0.25 ❌ (above current price)
  
  Matched:
    - Buyer1 (100 units) ↔ Seller1 (100 units)
    - Price: 0.2 currency/kg
    - Total: 20 currency
  
  Executed:
    - Chunk 1: Transaction (Buyer1 → Seller1, 20 currency, "market_purchase")
    - Chunk 2: Move (Seller1 inventory → Buyer1 inventory, 100 grain)
  
  Remaining:
    - Seller1: 400 units still available
    - Buyer2: No trade (price too low)
```

---

## Integration Touchpoints

### With Chunk 3 – Businesses & Production

**Businesses read CurrentPrice and supply/demand metrics as hints:**
- To pick what to produce (profitable or high-need goods)
- To decide whether to buy materials locally vs import later (when Chunk 4 AI leans on this)

**Chunk 3 itself still uses base costs; using prices is optional and a later refinement.**

**Example:**
```
Blacksmith Business:
  Reads MarketPrice (IronIngot): CurrentPrice = 6.5 currency/kg
  Reads MarketPrice (Weapon): CurrentPrice = 75.0 currency/unit
  
  Decision:
    - If (WeaponPrice - IronIngotCost × Recipe) > ProfitThreshold:
      → Produce weapons (profitable)
    - If WeaponDemand > WeaponSupply:
      → Produce weapons (high demand)
```

---

### With Chunk 4 – Trade & Logistics

**Trade AI (later chunk or separate spine) uses price differences:**
- If `Price_B >> Price_A` for GoodType X, consider exporting from A to B

**On arrival of cargo (Chunk 4):**
- Market's Supply is updated to reflect new stock
- Next price update cycle will reflect increased supply

**Chunk 5 does not spawn caravans; it just informs whoever does.**

**Example:**
```
Arbitrage Detection:
  VillageA Grain Price: 0.15 currency/kg
  VillageB Grain Price: 0.5 currency/kg (famine)
  Price Difference: 0.35 currency/kg
  
  Trade AI Decision:
    - Transport cost: 0.1 currency/kg
    - Profit margin: 0.25 currency/kg
    - If profitable → Spawn caravan (Chunk 4)
  
  On Arrival:
    - VillageB Supply increases
    - VillageB Grain Price decreases (next update cycle)
```

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed arbitrage mechanics.

---

### With Chunk 6 – Policies & Macro (Future)

**Taxes & tariffs:**
- Chunk 6 will adjust:
  - Effective prices (tax-inclusive)
  - Price floors/ceilings for regulated goods

**Smuggling & black markets:**
- Chunk 6 modifies or overrides prices for contraband goods via BlackMarket components & multipliers

**Chunk 5 just defines base markets and hooks where policy can plug in.**

**Example (Future Chunk 6 Integration):**
```
VillageA Market (Normal):
  Grain CurrentPrice: 0.2 currency/kg

VillageA Market (With Tariff):
  Grain BasePrice: 0.2 currency/kg
  Tariff: +20% (Chunk 6 policy)
  EffectivePrice: 0.24 currency/kg

VillageA Black Market (Chunk 6):
  Contraband Grain CurrentPrice: 0.3 currency/kg (smuggling premium)
  Risk: Detection chance 15%
```

---

## Telemetry & Debug

For balancing and "why is bread so expensive?" questions:

### Per Village

**Table of GoodType → BasePrice, CurrentPrice, Supply, Demand, multipliers:**
- Current state snapshot
- Recent changes (Δ price, Δ supply, Δ demand)
- Multiplier breakdown (which factor is driving price)

**Example:**
```
VillageA Market (Grain):
  BasePrice: 0.2
  CurrentPrice: 0.882 (↑ 341%)
  Supply: 500 (↓ 90%)
  Demand: 3000 (↑ 20%)
  
  Multipliers:
    SupplyDemandMultiplier: 2.45 (shortage)
    VillageWealthMultiplier: 0.9 (poor village)
    EventMultiplier: 2.0 (famine)
  
  Recent Changes:
    Price: 0.2 → 0.882 (+341%)
    Supply: 5000 → 500 (-90%)
    Demand: 2500 → 3000 (+20%)
  
  Explanation: Famine event + supply shortage → price spike
```

---

### Global View

**Heatmaps for key goods (food, weapons, key materials):**
- Visual representation of prices across all villages
- Shows regional price differences
- Highlights arbitrage opportunities

**Arbitrage opportunities:**
- Biggest price spreads between villages
- Sorted by profit potential (price difference - transport cost)

**Example:**
```
Global Arbitrage Opportunities (Grain):
  1. VillageC → VillageB: 0.35 currency/kg profit (0.5 - 0.15)
  2. VillageA → VillageD: 0.25 currency/kg profit (0.4 - 0.15)
  3. VillageE → VillageB: 0.20 currency/kg profit (0.5 - 0.3)
```

---

### Event-Aware View

**Famine/war overlays explaining extreme multipliers:**
- Show which events are affecting prices
- Explain why prices are high/low
- Track event duration and price recovery

**Example:**
```
VillageB Price Analysis:
  Grain Price: 0.5 currency/kg (↑ 150%)
  
  Active Events:
    - Famine (EventMultiplier: 2.0, Duration: 2 months remaining)
    - War (EventMultiplier: 1.5, Duration: ongoing)
  
  Combined Multiplier: 2.0 × 1.5 = 3.0
  Price Impact: BasePrice × 3.0 = 0.6 (but capped at 5.0x = 1.0)
  Actual Price: 0.5 (supply/demand also factors in)
```

---

## "Done Enough" Checklist for Chunk 5

Chunk 5 can be marked as "done enough" when all of the following are verified:

### ✅ Pricing Data & Config
- [ ] A `BasePrice` catalog exists for core GoodTypes (food, common materials, common products)
- [ ] `MarketPrice` buffers are attached to all markets (villages/colonies) for those GoodTypes
- [ ] A `MarketPricingConfig` Blob exists with tunable exponents & caps

### ✅ Supply & Demand Measurement
- [ ] There is a defined rule for computing Supply per GoodType from local inventories
- [ ] There is at least a basic method for computing MonthlyDemand per GoodType (using smoothed consumption and/or needs)
- [ ] GoodType mapping from ItemSpec exists (data-driven)

### ✅ Price Update System
- [ ] `MarketPricingSystem` runs on a fixed cadence and:
  - Computes `CurrentPrice` per GoodType from `BasePrice × multipliers`
  - Stores/exposes multipliers for debug (S/D, wealth, events)
- [ ] Price updates reflect supply/demand changes correctly
- [ ] Event multipliers affect prices as expected

### ✅ Market Interaction
- [ ] Entities can issue simple buy & sell intents targeting a local market
- [ ] `MarketClearingSystem`:
  - Matches buy/sell at `CurrentPrice`
  - Executes wealth transactions (Chunk 1) and inventory moves (Chunk 2)
- [ ] No tariffs, smuggling, or debt logic lives in Chunk 5 (reserved for Chunk 6)

### ✅ Telemetry
- [ ] At least one debug view shows:
  - Current prices & multipliers per village
  - Supply/demand numbers for a few key goods
- [ ] At least one visible price spike (e.g., famine test) behaving as expected

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Chunk 1**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) – Wealth layer for buy/sell transactions
- **Chunk 2**: [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md) – Inventory layer for supply measurement
- **Chunk 3**: [`Businesses_And_Production_Chunk3.md`](Businesses_And_Production_Chunk3.md) – Production layer that creates supply
- **Chunk 4**: [`Trade_And_Logistics_Chunk4.md`](Trade_And_Logistics_Chunk4.md) – Trade layer that moves goods between markets
- **Trade & Commerce**: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) – Detailed market mechanics, pricing formulas, arbitrage

### PureDOTS Integration

Chunk 5 systems should integrate with PureDOTS patterns:

- **Resource Systems**: Use existing inventory APIs from Chunk 2 for supply measurement
- **Registry Infrastructure**: Use entity registries for efficient market/intent queries
- **Time/Rewind**: Use `TickTimeState` for price update cadence, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Price calculations and market clearing should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villages, markets, price dynamics, local exchange
- **Space4X**: Colonies, markets, price dynamics, local exchange

Examples in this document use Godgame terminology (villages, grain, caravans), but the same patterns apply to Space4X (colonies, ores, carriers) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

