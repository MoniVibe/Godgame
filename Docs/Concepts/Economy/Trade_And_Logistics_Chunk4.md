# Trade & Logistics – Chunk 4

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 4  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md)), Chunk 1 ([Wealth & Ledgers](Wealth_And_Ledgers_Chunk1.md)), Chunk 2 ([Resources & Mass](Resources_And_Mass_Chunk2.md)), Chunk 3 ([Businesses & Production](Businesses_And_Production_Chunk3.md))  
**Feeds into:** Chunk 5 ([Markets & Prices](Markets_And_Prices_Chunk5.md)), Chunk 6 ([Policies & Macro](Policies_And_Macro_Chunk6.md)), Strategic AI (village/guild/fleet economic behavior)  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 4 establishes the transport layer of the economy – the system that moves goods between settlements. It answers a fundamental question:

**"Given goods in inventories and wealth in wallets, how do we move stuff between settlements in time, with cost and risk, without yet caring about dynamic prices or policies?"**

This chunk provides the transport layer that:
- Connects local production (Chunk 3) to other settlements/regions
- Moves items, people, and optionally wealth/contracts along routes
- Exposes time, cost, capacity, and risk as first-class properties that future chunks (markets, policies, AI) can react to

**Compliance:** All systems in Chunk 4 must:
- ✅ Use inventories for all cargo (Chunk 2)
- ✅ Use wealth transactions for operating costs (Chunk 1)
- ✅ Move products produced by businesses (Chunk 3)
- ✅ Be agnostic about dynamic pricing (Chunk 5)
- ✅ Be agnostic about tariffs/embargoes/policies (Chunk 6)

**Reference:** This implements the logistics spine from [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) (routes, caravans, stagecoaches, risk, cost).

---

## Scope & Boundaries

### In Scope (Chunk 4)

**Trade routes & networks:**
- Static route templates (A ↔ B with distance, difficulty)
- Dynamic route instances (active lines, caravans, stagecoaches, ships)

**Transport entities:**
- Caravans / wagons / pack animals
- Stagecoaches / passenger routes
- Ships / riverboats / ferries (later, but same spine)
- Couriers (small, fast, low-capacity)

**Cargo & capacity:**
- How much mass/volume a route unit can carry
- Loading / unloading between inventories (Chunk 2)

**Travel time, cost & risk:**
- Time per leg (distance + terrain + speed)
- Monetary cost (fuel, wages, maintenance)
- Risk model (bandits, storms, accidents), producing losses & events

**Basic routing decisions (local):**
- Given known routes between two settlements, decide which route and how often to send transports
- No global optimization yet; just simple heuristics

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed route mechanics, risk models, and transport types.

---

### Out of Scope (For Later Chunks)

**Dynamic pricing and trade profit logic (Chunk 5):**
- ❌ No buy/sell price decisions in Chunk 4
- ❌ Only "ship this quantity along this route", with a cost

**Tariffs, embargoes, sanctions, and policy-based route blocking (Chunk 6):**
- ❌ Chunk 4 exposes hooks for extra fees / blockage, but doesn't own the rules
- ❌ Policy decisions come from Chunk 6

**Complex trade AI:**
- ❌ No merchant brain that chooses what to trade based on margins (Chunk 5 AI)
- ❌ No empire-level policy decisions about trade (Chunk 6 / strategic AI)

---

## Conceptual Model

### World as Trade Graph

At this layer, the world is a graph of trade nodes and edges:

**Trade Nodes:**
- Settlements (villages, towns, cities, colonies)
- Outposts, resource hubs, space stations (Space4X)

**Trade Edges (Route Templates):**
- A ↔ B with:
  - Distance, terrain difficulty, travel time baseline
  - Risk baseline (banditry, weather, piracy)
  - Allowed modes (caravan, stagecoach, ship, etc.)

**Chunk 4 instantiates active routes and transports on top of this graph.**

**Example (Conceptual):**
```
Trade Graph:
  Node A (Village) ←→ Node B (Town)
    Distance: 150 km
    Terrain: Mixed (plains 70%, hills 30%)
    Risk: Medium (bandit chance 15%)
    Modes: Caravan, Stagecoach

  Node B (Town) ←→ Node C (City)
    Distance: 200 km
    Terrain: Plains
    Risk: Low (bandit chance 5%)
    Modes: Caravan, Stagecoach, Ship
```

---

### Transport Entities

Each transport is a moving inventory + wallet context:

**Caravan:**
- Inventory (cargo) – Chunk 2 inventory component
- Capacity (MaxMass/Volume) – Chunk 2 capacity rules
- Speed (based on animals, terrain, load)
- Operating cost rate (wages, feed, maintenance) – Chunk 1 wallet
- Owner (trader, guild, business, village)

**Stagecoach / Passenger Transport:**
- Similar to caravan, but capacity is mostly seats, plus small cargo
- Passenger capacity (people, not just cargo)
- Faster speed, higher cost per unit

**Ship / Riverboat / Spacecraft:**
- Same pattern, different speed/risk and maybe multi-leg routes
- May have fuel requirements (Space4X)
- Different risk profile (storms, piracy vs bandits)

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed transport entity specifications.

---

### Trade Flows vs Economic Logic

We deliberately separate:

**Logistics (Chunk 4):**
- "Transport 500 mass of [these items] from Node A to Node B every 10 days along Route R."
- Handles movement, time, cost, risk
- Does not decide what to trade or why

**Economic reasoning (Chunks 5–6 + AI):**
- "Why 500? Why those items? Is it profitable? Should we route via a safer, longer path?"
- Handles pricing, profit calculations, trade decisions
- Uses Chunk 4's logistics layer to execute decisions

**Chunk 4 does the first; Chunks 5–6 + AI do the second.**

---

## Data Model

### Route Templates

A `TradeRouteTemplate` catalog entry defines:

**Required Fields:**
- `RouteId` – Stable ID
- `NodeA` – Origin settlement/node
- `NodeB` – Destination settlement/node
- `Mode(s) Supported` – Enum: Caravan, Stagecoach, Ship, Mixed
- `Distance` – Abstract units (km, abstract distance)
- `TerrainType` – Enum: Plains, Hills, Mountains, Desert, River, Ocean, etc.
- `TerrainDifficulty` – Float multiplier (1.0 = baseline, higher = harder)
- `BaselineTravelTime` – Per mode, per typical season (days or hours)
- `BaselineRisk` – Risk profile:
  - `BanditChance` – Probability per leg
  - `AccidentChance` – Probability per leg
  - `WeatherHazardChance` – Probability per leg
- `CapacityHints` – How many transports is "typical" (for auto-scaling)

**Optional Fields:**
- `SeasonModifiers` – How weather affects travel time/risk
- `InfrastructureLevel` – Road quality, bridges, etc.
- `SecurityLevel` – Guard patrols, bandit activity

**Moddability:** Mods can add more routes, or entirely different route networks in scenarios.

**Example (Conceptual):**
```
TradeRouteTemplate {
  RouteId: "village_a_to_town_b",
  NodeA: VillageA,
  NodeB: TownB,
  ModesSupported: [Caravan, Stagecoach],
  Distance: 150.0,
  TerrainType: Mixed,
  TerrainDifficulty: 1.2,
  BaselineTravelTime: {
    Caravan: 5.0,      // 5 days
    Stagecoach: 3.0    // 3 days
  },
  BaselineRisk: {
    BanditChance: 0.15,
    AccidentChance: 0.05,
    WeatherHazardChance: 0.10
  },
  CapacityHints: {
    TypicalTransports: 2
  }
}
```

---

### Active Routes & Schedules

A `TradeLine` is an active instance of a template:

**Required Fields:**
- `TradeLineId` – Stable ID
- `RouteTemplate` – Reference to TradeRouteTemplate
- `Owner` – Entity: village, guild, business, realm, or independent trader
- `Schedule`:
  - `Frequency` – Every N days
  - `NumberOfConcurrentTransports` – How many transports run simultaneously
- `PolicyHints`:
  - `CargoTypePriority` – Enum: Food, Fuel, Luxuries, Tools, Materials, etc.
  - `PassengersAllowed` – Boolean

**Optional Fields:**
- `LastDeparture` – Tick when last transport departed
- `NextDeparture` – Tick when next transport should depart
- `ActiveTransports` – List of transport entities currently in transit

**Example (Conceptual):**
```
TradeLine {
  TradeLineId: "village_a_export_line",
  RouteTemplate: "village_a_to_town_b",
  Owner: VillageA,
  Schedule: {
    Frequency: 10,  // Every 10 days
    NumberOfConcurrentTransports: 2
  },
  PolicyHints: {
    CargoTypePriority: Food,
    PassengersAllowed: false
  },
  LastDeparture: Tick_12345,
  NextDeparture: Tick_12355
}
```

---

### Transport Units

Each caravan / coach / ship has:

**Required Components:**
- `CurrentNode` or `LegProgress` – Where transport is now
- `CurrentCargoInventory` – Chunk 2 inventory component
- `AssignedRoute` – Reference to TradeLine
- `NextDestination` – Node entity
- `Capacity` – MaxMass, MaxVolume (Chunk 2 capacity)
- `Speed` – Current speed (affected by load, terrain)
- `CostModel` – Operating cost rate
- `OwnerWallet` – Chunk 1 wallet reference for operating costs

**Optional Components:**
- `Crew` – List of crew/guard entities
- `Passengers` – List of passenger entities
- `CurrentLeg` – Which leg of multi-leg route
- `RiskModifiers` – Current risk adjustments (guards, weather, etc.)

**Example (Conceptual):**
```
CaravanEntity {
  CurrentNode: VillageA,
  LegProgress: 0.0,  // 0% complete
  CargoInventory: {
    Items: [
      { ItemId: "grain", Quantity: 1000, Mass: 500 }
    ],
    TotalMass: 500,
    MaxMass: 2000
  },
  AssignedRoute: "village_a_export_line",
  NextDestination: TownB,
  Speed: 30.0,  // km/day
  CostModel: {
    BaseCostPerDay: 10.0,  // wages, feed, maintenance
    CostPerKm: 0.1
  },
  OwnerWallet: VillageATreasury
}
```

---

## Core Flows

### Planning & Scheduling Transports

Per TradeLine:

**1. Check Time:**
- If `LastDeparture + Frequency <= Now`, schedule new departure
- Update `NextDeparture` accordingly

**2. Evaluate Capacity Needs:**
- (Minimal version) Always send one transport up to capacity
- (Later) Scale based on backlog / demand from Chunk 5

**3. Spawn or Reuse Transport Entity:**
- Create or fetch a caravan/coach/ship
- Assign to route, initialize inventory empty
- Set owner, destination, route template

**4. Load Cargo:**
- Chunk 4 only needs simple rules initially:
  - Load any items flagged "export" from origin's trade stockpile, up to capacity
  - Actual "export flags" will be set by production/AI/trade chunk (or designer)
- Move items from origin stockpile inventory to transport inventory (Chunk 2)

**5. Start Journey:**
- Move transport from node A toward node B along route template
- Initialize leg progress to 0.0
- Record departure event

---

### Movement Along Route

Transport ticks:

**1. Position & Time:**
- Progress along route based on speed and time step
- Update `LegProgress` (0.0 to 1.0)
- When `LegProgress >= 1.0`, transport arrives at destination

**2. Speed Modifier:**
- Base speed from transport type
- Load factor: `Speed = BaseSpeed × (1.0 - LoadFactor × LoadPenalty)`
  - Heavier = slower
- Terrain modifiers: `Speed = Speed × TerrainModifier`
- Season/weather (if climate system integrated)

**3. Risk Resolution:**
- At intervals (e.g., per leg or daily):
  - Roll for events based on route's baseline risk
  - **Bandit attack:** Steal/destroy part of cargo, maybe kill crew
  - **Accident:** Lose some cargo, damage transport
  - **Weather event:** Delay or partial loss

**Effects:**
- Item loss: Remove items from transport inventory (Chunk 2)
- Wealth consequences: Owner pays for losses, insurance, compensation (Chunk 1 transactions)
- Event logging: Record for telemetry and narrative

**4. Operating Cost:**
- Periodic cost paid by owner:
  - Wages (crew/guards)
  - Feed (animals)
  - Fuel (ships, Space4X)
  - Wear & tear (maintenance)
- Implemented as wealth transactions (Chunk 1)
- Cost accumulates over journey

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed risk models and cost calculations.

---

### Arrival & Unloading

When a transport reaches destination:

**1. Unload Cargo:**
- Move cargo from transport inventory to destination stockpile inventories (Chunk 2)
- Check destination capacity before unloading
- If destination full, hold cargo or find alternative storage

**2. Record Arrival Event:**
- For telemetry and for market chunk later:
  - "X units of Item Y arrived from Node A to Node B"
  - Record timestamp, quantity, item type
  - Emit event for Chunk 5 (markets) to observe

**3. Optional Local Distribution:**
- For now, simply deposit into a "trade stockpile" inventory
- Later, Chunk 5 & AI decide who buys/uses it

**4. Return or Continue:**
- For two-way routes: Assign transport back along route (B → A) after turnaround
- For ring routes: Move to next leg
- For one-way routes: Transport stays at destination or despawns

**5. Update TradeLine:**
- Record `LastDeparture` = arrival time
- Schedule next departure if route continues

---

## Integration with Previous Chunks

### With Chunk 2 – Items & Mass

**All cargo is just normal inventories:**
- ✅ Loading/unloading = moving stacks between inventories
- ✅ Use Chunk 2 inventory APIs for all cargo operations

**Capacity & speed:**
- ✅ Use total mass (Chunk 2) to compute encumbrance & speed modifiers
- ✅ `Speed = BaseSpeed × (1.0 - (CurrentMass / MaxMass) × LoadPenalty)`
- ✅ Over-capacity warnings/flags from Chunk 2 capacity checks

**Example:**
```
Transport has:
  CurrentMass: 1500 kg
  MaxMass: 2000 kg
  LoadFactor: 0.75
  SpeedModifier: 1.0 - (0.75 × 0.2) = 0.85
  EffectiveSpeed: BaseSpeed × 0.85
```

---

### With Chunk 3 – Production

**Origin nodes:**
- ✅ Provide exportable items from local production/business inventories
- ✅ Chunk 3 produces items → Chunk 4 moves them

**Destination nodes:**
- ✅ Receive items into their stockpiles
- ✅ Production there can now use them as inputs (Chunk 3)

**Chunk 4 itself does not create or transform items; it only moves them.**

**Example:**
```
VillageA Production (Chunk 3):
  Produces: 1000 grain → VillageA Stockpile

VillageA Trade (Chunk 4):
  Loads: 500 grain from VillageA Stockpile → Caravan
  Transports: Caravan → TownB
  Unloads: 500 grain → TownB Stockpile

TownB Production (Chunk 3):
  Uses: 500 grain from TownB Stockpile → Mill → Flour
```

---

### With Chunk 1 – Wealth

**Operating cost:**
- ✅ Owner's wallet pays per journey/time unit
- ✅ Implemented as wealth transactions (Chunk 1)
- ✅ No ad-hoc wealth modifications

**Optional revenue stub:**
- ✅ For now, you can keep revenue as a simple "transport fee", not price-dependent:
  - e.g., fixed per-mass or per-slot rate, from either sender, receiver, or both
- ✅ Full buy/sell revenue decisions come in Chunk 5

**No wealth is created or destroyed except through explicit transactions; no free teleports of value.**

**Example:**
```
Operating Cost (Chunk 1):
  Transaction: OwnerWallet → OperatingCostWallet
  Amount: 50.0
  Reason: "caravan_operating_cost"
  Tick: CurrentTick

Transport Fee (Chunk 1, optional stub):
  Transaction: SenderWallet → OwnerWallet
  Amount: 100.0
  Reason: "transport_fee"
  Tick: DepartureTick
```

---

## Telemetry & Debug

To keep things sane and debuggable:

### Per Route / Line

**Metrics:**
- Departures per season/month
- Average travel time
- Cargo throughput (mass / units by item type)
- Losses (bandit, accident, weather)
- Success rate (completed vs failed journeys)

**Example:**
```
TradeLine "village_a_export_line":
  Departures (Last Month): 6
  Average Travel Time: 4.8 days
  Cargo Throughput:
    - Grain: 3000 units (1500 kg)
    - Tools: 50 units (100 kg)
  Losses:
    - Bandit Attacks: 2 (lost 200 kg grain)
    - Accidents: 1 (lost 50 kg tools)
  Success Rate: 83% (5/6 completed)
```

---

### Per Settlement

**Metrics:**
- Imports & exports per item type over time
- Net balance of key resources (grain, tools, metals, luxuries)
- Trade volume (total mass moved in/out)
- Trade partners (which settlements trade with this one)

**Example:**
```
VillageA Trade (Last Month):
  Exports:
    - Grain: 3000 units
    - Tools: 50 units
  Imports:
    - Iron Ingots: 200 units
    - Luxuries: 10 units
  Net Balance:
    - Grain: -3000 (net exporter)
    - Tools: -50 (net exporter)
    - Iron Ingots: +200 (net importer)
  Trade Partners: [TownB, VillageC]
```

---

### Per Transport

**Metrics:**
- Trip history (from, to, duration, loss events)
- Cargo carried (what items, quantities)
- Operating costs incurred
- Success/failure status

**Example:**
```
Caravan_123 Trip History:
  Trip 1:
    From: VillageA → To: TownB
    Duration: 5 days
    Cargo: 1000 grain (500 kg)
    Operating Cost: 50 currency
    Events: None
    Status: Success
  
  Trip 2:
    From: TownB → To: VillageA
    Duration: 4 days
    Cargo: 200 iron ingots (1000 kg)
    Operating Cost: 45 currency
    Events: Bandit attack (lost 50 kg cargo)
    Status: Partial success
```

**This data will be key for tuning future Chunks 5 & 6 (markets & policies) and for AI.**

---

## "Done Enough" Checklist for Chunk 4

Chunk 4 can be marked as "done enough" when all of the following are verified:

### ✅ Route & Transport Definitions
- [ ] A `TradeRouteTemplate` catalog exists with routes between several settlements (at least 2–3 test links)
- [ ] Transport units (e.g., caravans) have:
  - Inventories wired to Chunk 2
  - Capacity, speed, and basic cost parameters
  - Owner wallet references (Chunk 1)

### ✅ Movement & Risk
- [ ] Caravans can:
  - Be scheduled from Node A to Node B along a route at set intervals
  - Move over time with distance & speed correctly applied
- [ ] Basic risk model implemented:
  - Occasionally triggers "loss events" (bandits/accidents/weather) that remove/damage cargo
  - Events are logged for telemetry

### ✅ Cargo & Cost Flows
- [ ] Loading/unloading:
  - Items are explicitly moved from origin stockpiles to caravans and from caravans to destination stockpiles
  - Uses Chunk 2 inventory APIs
- [ ] Operating costs:
  - Owner wealth decreases over the journey in a predictable way via transactions
  - Uses Chunk 1 transaction APIs

### ✅ Layering & Rules
- [ ] No dynamic price or profit calculations appear in Chunk 4
- [ ] Chunk 4 doesn't change recipes or production logic (Chunk 3) or invent new items
- [ ] All route stats, risk, and cost parameters live in data catalogs/configs, not hard-coded

### ✅ Telemetry
- [ ] There's at least one debug view showing:
  - Active routes and transports
  - Cargo in transit
  - Recent arrivals & losses per route

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Chunk 1**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) – Wealth layer for operating costs
- **Chunk 2**: [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md) – Inventory layer for cargo
- **Chunk 3**: [`Businesses_And_Production_Chunk3.md`](Businesses_And_Production_Chunk3.md) – Production layer that creates tradeable goods
- **Trade & Commerce**: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) – Detailed trade mechanics, route quality, transport types

### PureDOTS Integration

Chunk 4 systems should integrate with PureDOTS patterns:

- **Resource Systems**: Use existing inventory APIs from Chunk 2
- **Registry Infrastructure**: Use entity registries for efficient route/transport queries
- **Time/Rewind**: Use `TickTimeState` for journey timestamps, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Transport movement and risk resolution should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villages, caravans, stagecoaches, trade routes
- **Space4X**: Colonies, carriers, ships, logistics routes, space stations

Examples in this document use Godgame terminology (villages, caravans, stagecoaches), but the same patterns apply to Space4X (colonies, carriers, ships) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

