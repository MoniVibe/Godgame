# Wealth & Ledgers – Chunk 1

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 1  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md))  
**Feeds into:** Businesses, trade, politics, social dynamics, courts, guilds  
**Related:** Chunk 2 ([Resources & Mass](Resources_And_Mass_Chunk2.md)) – Independent but complementary layer  
**Related:** Chunk 3 ([Businesses & Production](Businesses_And_Production_Chunk3.md)) – Uses Chunk 1 for wages/profit transactions  
**Related:** Chunk 4 ([Trade & Logistics](Trade_And_Logistics_Chunk4.md)) – Uses Chunk 1 for transport operating costs  
**Related:** Chunk 5 ([Markets & Prices](Markets_And_Prices_Chunk5.md)) – Uses Chunk 1 for buy/sell transactions  
**Related:** Chunk 6 ([Policies & Macro](Policies_And_Macro_Chunk6.md)) – Uses Chunk 1 for tax/loan transactions  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 1 establishes the foundational wealth and ledger system – the bottom layer of the economic stack. It answers a simple but critical question:

**"Who has money, in what form, how does it move, and how do we read social status from it?"**

This chunk provides the infrastructure for all wealth tracking, ensuring every currency unit in the simulation is accounted for and traceable. It establishes the rules that all later economic systems (businesses, trade, markets, policies) must follow.

**Scope:** This chunk deals only with abstract wealth flows. It explicitly does NOT include:
- ❌ Items/resources (Chunk 2)
- ❌ Prices/markets (Chunk 5)
- ❌ Trade routes/logistics (Chunk 4)
- ❌ Tax policies (Chunk 6)

**Core concerns:**
- ✅ Wallets (balances)
- ✅ Strata (poor → rich tiers)
- ✅ Transactions (who paid whom and why)
- ✅ Aggregation (families, dynasties, guilds, treasuries)

**Compliance:** All systems in Chunk 1 must respect Chunk 0 principles:
- Single source of truth (no ghost money)
- Layer boundaries (Layer 1 only)
- Data > code (tiers, inheritance rules in catalogs)

---

## Scope of Chunk 1

Chunk 1 provides the foundation for wealth tracking across both Godgame and Space4X. It defines:

1. **Who can own wealth** – A closed set of wallet types
2. **How wealth is stored** – Wallet components with balances and strata
3. **How wealth moves** – Transaction vocabulary and ledger requirements
4. **How wealth maps to status** – Wealth tier system with data-driven thresholds
5. **How groups aggregate wealth** – Family, dynasty, guild, and settlement aggregation rules

**Success criteria:** When Chunk 1 is complete, you can answer:
- "How much money does entity X have?" → Query wallet component
- "Where did entity X's money come from?" → Query transaction ledger
- "What social tier is entity X?" → Query wealth tier from balance
- "How wealthy is this dynasty?" → Query aggregate calculation

---

## Wealth Owners & Wallets

**Rule:** Every wealth unit in the simulation must be in exactly one wallet belonging to one of these roles. No other place in the sim is allowed to store "some money."

### Wallet Types

#### 1. Individuals
**Entities:** Villagers, agents, crew members, individual actors

**Purpose:** Personal wealth for individuals

**Wallet Component:** `VillagerWealth` (or `IndividualWealth` for Space4X)

**Creation:**
- On entity spawn: Initialize with starting wealth (scenario-defined or zero)
- Can be set via scenario data or spawn rules

**Destruction/Merger:**
- On death: Wealth transferred via inheritance (see Inheritance section)
- On merge: Wealth transferred to absorbing entity

**Usage:** Personal expenses, wages received, individual transactions

---

#### 2. Families / Households
**Entities:** Close kin units, households sharing resources

**Purpose:** Shared family wealth for household expenses and inheritance

**Wallet Component:** `FamilyWealth` (or `HouseholdWealth`)

**Creation:**
- On family formation: Initialize with pooled starting wealth
- Can accumulate from member contributions

**Destruction/Merger:**
- On family dissolution: Wealth distributed to members per dissolution rules
- On family merge: Wealth combined with merging family

**Usage:** Family expenses, member support, inheritance pools

**Aggregation:** Family wealth can be computed as sum of member wallets + family wallet (see Aggregation section)

---

#### 3. Dynasties / Lineages
**Entities:** Broad bloodlines spanning multiple families

**Purpose:** Dynastic pools for major moves (buy land, fund wars, political campaigns)

**Wallet Component:** `DynastyWealth` (or `LineageWealth`)

**Creation:**
- On dynasty formation: Initialize with founding wealth
- Accumulates from family contributions and dynastic income

**Destruction/Merger:**
- On dynasty extinction: Wealth distributed per extinction rules
- On dynasty merge: Wealth combined with merging dynasty

**Usage:** Major investments, political funding, dynastic projects

**Aggregation:** Dynasty wealth computed as sum of member families' aggregate wealth + dynasty wallet

---

#### 4. Businesses
**Entities:** Operating businesses, shops, production facilities

**Purpose:** Operating capital and retained earnings

**Wallet Component:** `BusinessBalance`

**Creation:**
- On business founding: Initialize with startup capital from owners
- Can receive loans, investments

**Destruction/Merger:**
- On business closure: Remaining wealth distributed to owners per closure rules
- On business sale/merger: Wealth transferred to new owners

**Usage:** Operating expenses, wages paid, profit accumulation

**Reference:** See [`BusinessAndAssets.md`](BusinessAndAssets.md) for business wealth integration

---

#### 5. Guilds / Orders / Factions
**Entities:** Guilds, orders, factions, organizations

**Purpose:** Pooled funds for strikes, coups, campaigns, crisis response

**Wallet Component:** `GuildTreasury` (or `FactionTreasury`)

**Creation:**
- On guild formation: Initialize with founding contributions
- Accumulates from member dues and guild income

**Destruction/Merger:**
- On guild dissolution: Wealth distributed per dissolution rules
- On guild merge: Wealth combined with merging guild

**Usage:** Guild operations, member support, collective actions

**Reference:** See [`Guild_Formation_And_Dynamics.md`](../Villagers/Guild_Formation_And_Dynamics.md) for guild treasury details

---

#### 6. Settlements & Realms
**Entities:** Villages, cities, empires, colonies, realms

**Purpose:** Public treasuries for taxes, war chests, social programs

**Wallet Component:** `VillageTreasury`, `CityTreasury`, `EmpireTreasury` (or `ColonyTreasury` for Space4X)

**Creation:**
- On settlement founding: Initialize with founding treasury
- Accumulates from taxes, tribute, trade

**Destruction/Merger:**
- On settlement destruction: Remaining wealth distributed per destruction rules
- On annexation: Wealth transferred to annexing entity

**Usage:** Public expenses, infrastructure, military, social programs

**Note:** Chunk 1 defines balances and strata only. Politics about who controls treasuries come in later chunks.

---

#### 7. (Future) Carriers / Colonies / Fleets (Space4X)
**Entities:** Space carriers, colonies, fleet task forces

**Purpose:** Operating funds for space operations

**Wallet Component:** `CarrierTreasury`, `ColonyTreasury`, `FleetTreasury`

**Creation/Destruction:** Similar patterns to settlements

**Usage:** Fleet operations, colony development, carrier maintenance

**Note:** This is a placeholder for Space4X-specific wallet types. Implementation follows same patterns as other wallet types.

---

### Wallet Component Structure

Conceptually, every wallet type follows the same structure:

**Core Fields:**
- `Balance` – Current numeric wealth (float)
- `WealthTier` / `WealthStrata` – Current social tier (enum: Ultra Poor, Poor, Mid, High, Ultra High)
- `LastChangeSource` – Optional marker (wages, profit, tax, donation, theft, loan, miracle)
- `LastUpdateTick` – Last time balance was modified

**Implementation Note:** You don't need a single generic component. Each wallet type can have its own component (`VillagerWealth`, `BusinessBalance`, etc.), but they all follow the same semantics: **Balance is the only truth.**

**Example Structure (Conceptual):**
```
VillagerWealth:
  Balance: 1250.0
  WealthTier: Mid
  LastChangeSource: "wage"
  LastUpdateTick: 12345

BusinessBalance:
  Balance: 5000.0
  WealthTier: High
  LastChangeSource: "profit"
  LastUpdateTick: 12350
```

---

## Transactions & Ledger

**Rule:** Every balance change must be represented as a transaction. No `Balance += X` anywhere without a transaction record.

### Transaction Vocabulary

Chunk 1 defines the canonical transaction types:

#### Income
Wealth flowing into a wallet:
- **Wages** – Payment for work performed
- **Stipends** – Regular payments (court stipends, pensions)
- **Profit Share** – Distribution of business profits to owners
- **Rent** – Income from owned assets (land, buildings)
- **Tribute** – Payments from vassals or conquered entities

#### Expenses
Wealth flowing out of a wallet:
- **Upkeep** – Regular maintenance costs (living expenses, building maintenance)
- **Living Costs** – Basic survival expenses
- **Maintenance** – Repair and upkeep costs (very simple at this stage)

#### Transfers
Wealth moving between wallets:
- **Gifts** – Voluntary transfers without expectation of return
- **Donations** – Charitable transfers (see Social Dynamics section)
- **Alimony** – Support payments (family support)
- **Family Support** – Intra-family transfers (parents helping children)
- **Guild Dues** – Regular contributions to guild treasury

#### Exceptional
Special wealth flows:
- **Inheritance** – Wealth transfer on death (see Inheritance section)
- **Bankruptcy Wipe** – Debt forgiveness, wealth reset
- **Loans** – Record as flows only; loan rules come in Chunk 6
- **Miracles** – Divine wealth injection/removal (explicit exception)

---

### Transaction Requirements

Every transaction must record:

**Required Fields:**
- `From` – Source entity/wallet
- `To` – Destination entity/wallet
- `Amount` – Wealth amount transferred
- `Type` – Transaction type (Income, Expense, Transfer, Exceptional)
- `Reason` – Specific reason (wage, donation, inheritance, etc.)
- `Tick` – Simulation tick when transaction occurred

**Optional Fields:**
- `Context` – Additional context (business ID, relationship type, etc.)
- `Reversible` – Whether transaction can be reversed (for rewind/debug)

**Implementation Flexibility:**
- Per-entity transaction buffers (`DynamicBuffer<WealthTransaction>`)
- Shared ledger entity with all transactions
- Aggregated per-tick stats (as long as individual transactions are traceable)

**Rule:** The implementation can vary, but the requirement is absolute: **No balance change without a transaction record.**

---

### Transaction Examples (Conceptual)

```
Transaction 1:
  From: BusinessEntity_123
  To: VillagerEntity_456
  Amount: 50.0
  Type: Income
  Reason: "wage"
  Tick: 12345

Transaction 2:
  From: VillagerEntity_789
  To: VillagerEntity_456
  Amount: 100.0
  Type: Transfer
  Reason: "donation"
  Tick: 12350
  Context: "charity_triggered_by_disaster"

Transaction 3:
  From: DeceasedEntity_999
  To: HeirEntity_111
  Amount: 500.0
  Type: Exceptional
  Reason: "inheritance"
  Tick: 12400
  Context: "primogeniture_split"
```

---

### Explicit Exceptions

Only these places are allowed to create/destroy wealth without a transaction:

1. **Scenario Setup** – Initial wealth assignment from scenario data
2. **Divine Miracles** – Explicit god-level wealth injection/removal (marked as miracle transactions)
3. **Admin Tools** – Debug/development tools (must be clearly marked and logged)

All other wealth changes must go through transactions.

---

## Wealth Tiers / Strata

**Rule:** Wealth tiers are computed from balances using data-driven thresholds. All thresholds live in data catalogs, not hardcoded.

### WealthTierSpec Catalog

Chunk 1 requires a data catalog that defines wealth tiers:

**Catalog Structure:**
- `WealthTierSpec` entries for each tier
- Each entry contains:
  - `TierName` – Name (Ultra Poor, Poor, Mid, High, Ultra High)
  - `MinWealth` – Minimum balance for this tier
  - `MaxWealth` – Maximum balance for this tier (or infinity for top tier)
  - `Title` / `Label` – Optional display name (pauper, peasant, comfortable, wealthy, oligarch)
  - `SocialEffects` – Optional social modifiers (base respect/fear/envy, access to institutions, court eligibility)

**Catalog Location:** BlobAsset catalog built from ScriptableObject/data files

**Moddability:** Different catalogs per world type (Godgame vs Space4X) or era/scenario are allowed

---

### Tier Computation

**Rule:** On a regular cadence (e.g., monthly or per configurable interval), recompute tier for every wallet from Balance.

**Process:**
1. Query wallet Balance
2. Look up appropriate WealthTierSpec catalog
3. Find tier where `MinWealth <= Balance < MaxWealth`
4. Update wallet's `WealthTier` component
5. Emit tier change event if tier changed

**Frequency:** Configurable (default: monthly, but can be per-tick for testing or yearly for performance)

---

### Tier Usage

Wealth tiers are used by:

**Social/AI Systems:**
- Charity decisions (see Social Dynamics section)
- Opportunism decisions (see Social Dynamics section)
- Respect/intimidation calculations
- Social status displays

**UI & Debug:**
- Wealth tier overlays ("this dynasty is top 1%")
- Social status indicators
- Debug wealth displays

**Reference:** See [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) for detailed tier definitions and social effects

---

## Aggregation: Families, Dynasties, Groups

**Rule:** Group wealth is derived, not stored separately (except for true pooled wallets).

### Family Aggregate

Family wealth can be represented in two ways:

**Option 1: True Family Wallet**
- `FamilyWealth` component with its own Balance
- Family members can contribute to/withdraw from family wallet
- Family wallet is a real wallet with transactions

**Option 2: Computed Aggregate**
- No separate family wallet component
- Family wealth = sum of all member `VillagerWealth.Balance` values
- Used purely for checks & UI

**Recommended:** Use Option 1 (true family wallet) for families that pool resources, Option 2 (computed) for families that don't pool but need aggregate views.

**Aggregation Formula:**
```
FamilyWealthAggregate = FamilyWallet.Balance + Sum(MemberWealth.Balance)
```

---

### Dynasty Aggregate

Dynasty wealth follows the same pattern:

**Aggregation Formula:**
```
DynastyWealthAggregate = DynastyWallet.Balance + Sum(FamilyWealthAggregate for all member families)
```

**Usage:** Dynasty wealth tier checks, "top N dynasties" calculations, social status displays

---

### Guild Treasuries

Guild treasuries are true wallets:
- `GuildTreasury` component with Balance
- All changes via transactions (dues, expenses, distributions)
- No special-case math

**Aggregation:** Guild wealth = `GuildTreasury.Balance` (no member aggregation needed, as members have separate wallets)

---

### Settlement & Realm Treasuries

Settlement treasuries are true wallets:
- `VillageTreasury`, `CityTreasury`, `EmpireTreasury` components with Balance
- All changes via transactions (taxes, expenses, tribute)
- No special-case math

**Note:** Chunk 1 defines balances and strata only. Politics about who controls treasuries (councils, rulers, etc.) come in later chunks.

---

## Basic Flows Required for Chunk 1

To count Chunk 1 as "done," these basic flows must be working end-to-end:

### 1. Wages / Stipends

**Flow:** Source wallet → Individual wallet

**Sources:**
- Employers paying workers
- Villages paying guards/courtiers
- Realms paying stipends

**Transaction Pattern:**
```
From: EmployerWallet
To: EmployeeWallet
Amount: WageAmount
Type: Income
Reason: "wage"
```

**Implementation:** Can be stub-driven initially (fixed wage amounts), but must use transaction pattern.

---

### 2. Family Support

**Flow:** Family member wallet → Family member wallet (intra-family transfers)

**Triggers:**
- Wealth tier differences (richer members help poorer)
- Alignment/purity modifiers (Good/Pure more likely to help)
- Relationship strength

**Transaction Pattern:**
```
From: DonorWallet
To: RecipientWallet
Amount: SupportAmount
Type: Transfer
Reason: "family_support"
Context: "tier_difference_triggered"
```

**Reference:** See [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) for family support formulas

---

### 3. Simple Donations

**Flow:** Donor wallet → Recipient wallet (charity vs opportunism)

**Charity (Good/Pure alignment):**
- Trigger: Neighbor/family/guild member suffers major loss (death, disaster, business failure, strata drop)
- Formula: `BaseDonation = Donor.Wealth × Percentage × Modifiers(alignment, purity, outlook, relation)`
- Transaction: Wealth Transfer with type Donation

**Opportunism (Evil/Corrupt alignment):**
- Trigger: Victim shows weakness (wealth drop, business failure)
- Behavior: Predatory transfers at discount
- Transaction: Wealth Transfer with flag "distressed_sale" vs "fair_sale"

**Implementation:** Expressed purely as wealth transfers, no side effects yet. Full sabotage logic (rumors, politics) handled in other systems using these wealth events as triggers.

**Reference:** See [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) for charity/opportunism formulas

---

### 4. Inheritance

**Flow:** Deceased wallet → Heir wallets (on death)

**Process:**
1. On entity death, query deceased wallet Balance
2. Look up inheritance rule (data-driven: primogeniture, equal split, merit-based)
3. Calculate split amounts per inheritance rule
4. Drain deceased wallet (set Balance to 0, or transfer to "estate" wallet temporarily)
5. Credit heir wallets via transactions
6. Any "lost to state/guild/god" portion is transaction to that wallet

**Transaction Pattern:**
```
From: DeceasedWallet
To: HeirWallet1
Amount: InheritedAmount1
Type: Exceptional
Reason: "inheritance"
Context: "primogeniture_split"

From: DeceasedWallet
To: VillageTreasury
Amount: EstateTaxAmount
Type: Exceptional
Reason: "inheritance_tax"
```

**Inheritance Rules:** Defined in data catalog (moddable)

---

### 5. Minimal Business Profit/Loss

**Flow:** Business operations → BusinessBalance (monthly net result)

**Requirement:** Businesses should have monthly "net result": +profit or -loss into BusinessBalance

**Implementation:** Can be temporarily stub-driven ("magic profit × difficulty factor"), but must:
- Respect ledger rules (use transactions)
- Prove the flow works end-to-end
- Be replaceable with real production system later

**Transaction Pattern:**
```
From: (BusinessOperations)
To: BusinessBalance
Amount: ProfitAmount
Type: Income
Reason: "business_profit"
```

**Note:** Full production system comes in Chunk 3. This is just a minimal flow to prove wealth tracking works.

---

## Interfaces with Other Systems

Even though other chunks aren't implemented yet, Chunk 1 should define how other layers will interact with wealth:

### Transaction Service Pattern

**Pattern:** "RecordTransaction + ApplyBalanceChange"

**Service Interface (Conceptual):**
```
RecordTransaction(
  from: Entity,
  to: Entity,
  amount: float,
  type: TransactionType,
  reason: FixedString64Bytes,
  context: Optional<TransactionContext>
)
```

**Behavior:**
1. Create transaction record
2. Update source wallet Balance (decrease)
3. Update destination wallet Balance (increase)
4. Emit transaction event (for telemetry/debug)
5. Check for tier changes (if balance crosses tier threshold)

**Allowed Transaction Reasons:**
- `wage`, `stipend`, `profit_share`, `rent`, `tribute`
- `upkeep`, `living_cost`, `maintenance`
- `gift`, `donation`, `alimony`, `family_support`, `guild_dues`
- `inheritance`, `bankruptcy`, `loan`, `miracle`
- `quest_reward`, `tax`, `fine`, `bribe` (for future chunks)

---

### Alignment/Social System Hooks

**Charity/Opportunism Decisions Read:**
- `WealthTier` – Current tier of donor and recipient
- `Relationship` – Relationship strength/type
- `Alignment` – Good/Evil alignment
- `Purity` – Pure/Corrupt alignment

**Decision Process:**
1. Check trigger conditions (disaster, wealth drop, etc.)
2. Query wealth tiers of potential donor and recipient
3. Query alignment/purity/relationship
4. Calculate donation probability/amount using formulas
5. Execute transaction if triggered

**Reference:** See [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) for detailed formulas

---

### Telemetry Hooks

**Basic Stats Required:**

**Top N Richest Dynasties:**
- Query all dynasty aggregate wealth
- Sort by aggregate balance
- Return top N with balances and tiers

**Gini-like Inequality Measure per Settlement:**
- Query all individual wallets in settlement
- Calculate inequality metric (Gini coefficient or simpler measure)
- Track over time for mobility analysis

**Recent Bankruptcies / Zero Crossings:**
- Track wallets that cross zero balance (positive to negative or vice versa)
- Emit bankruptcy event if balance stays negative for threshold duration
- Log for debugging and narrative hooks

**Wealth Mobility:**
- Track % of entities that changed tier in last X years
- Track upward/downward mobility rates
- Track dynastic concentration (share of total wealth owned by top N dynasties)

**Implementation:** These metrics are for tuning and future "Garry's-mod" tools. Can be computed on-demand or cached with periodic updates.

---

## "Done" Checklist for Chunk 1

Chunk 1 can be marked as "done enough" when all of the following are verified:

### ✅ Wallet Infrastructure
- [ ] Every wealth-owning role has a wallet component
- [ ] No other random wealth fields exist in other systems
- [ ] All wallet types are created on entity spawn/founding
- [ ] All wallet types have destruction/merger rules defined

### ✅ Transaction System
- [ ] All existing systems that give/take money use transactions, not direct balance changes
- [ ] Transaction vocabulary is defined and documented
- [ ] Transaction recording works (per-entity buffers or shared ledger)
- [ ] Transaction service pattern is implemented and documented

### ✅ Wealth Tiers
- [ ] WealthTierSpec catalog is defined (data-driven)
- [ ] Tier computation system works (recomputes tiers on cadence)
- [ ] Tiers are visible in debug/UI
- [ ] Tier change events are emitted

### ✅ Aggregation
- [ ] Family aggregate wealth numbers are computed and visible
- [ ] Dynasty aggregate wealth numbers are computed and visible
- [ ] Guild treasury aggregation works
- [ ] Settlement treasury aggregation works

### ✅ Basic Flows Working
- [ ] Wages/stipends flow works end-to-end
- [ ] Inheritance flow works end-to-end
- [ ] Simple donations/opportunism flow works end-to-end
- [ ] Business monthly profit/loss → BusinessBalance flow works end-to-end

### ✅ Debug/Telemetry
- [ ] At least one simple ledger/debug view exists
- [ ] Wallet balances are inspectable
- [ ] Recent big transactions are visible
- [ ] Basic telemetry stats work (top N dynasties, inequality measure, bankruptcies)

### ✅ Chunk 0 Compliance
- [ ] Single source of truth: All wealth in wallet components only
- [ ] Layer boundaries: Chunk 1 stays in Layer 1 (no markets, prices, trade)
- [ ] Data > code: Tiers, inheritance rules, donation formulas in catalogs

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Wealth & Social Dynamics**: [`Wealth_And_Social_Dynamics.md`](../Villagers/Wealth_And_Social_Dynamics.md) – Tier definitions, social dynamics
- **Business & Assets**: [`BusinessAndAssets.md`](BusinessAndAssets.md) – Business wealth integration
- **Guild System**: [`Guild_Formation_And_Dynamics.md`](../Villagers/Guild_Formation_And_Dynamics.md) – Guild treasury model
- **Elite Courts**: [`Elite_Courts_And_Retinues.md`](../Villagers/Elite_Courts_And_Retinues.md) – Court stipends, maintenance

### PureDOTS Integration

Chunk 1 systems should integrate with PureDOTS patterns:

- **Registry Infrastructure**: Use entity registries for efficient wallet queries
- **Time/Rewind**: Use `TickTimeState` for transaction timestamps, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Transaction recording systems should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villagers, villages, bands, businesses, guilds
- **Space4X**: Carriers, colonies, fleets, empires

Examples in this document use Godgame terminology (villagers, villages), but the same patterns apply to Space4X (carriers, colonies) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

