# Policies & Macro – Chunk 6

**Type:** Design Specification  
**Status:** Design – Foundation  
**Version:** Chunk 6  
**Depends on:** Chunk 0 ([Economy Spine & Principles](Economy_Spine_And_Principles.md)), Chunk 1 ([Wealth & Ledgers](Wealth_And_Ledgers_Chunk1.md)), Chunk 2 ([Resources & Mass](Resources_And_Mass_Chunk2.md)), Chunk 3 ([Businesses & Production](Businesses_And_Production_Chunk3.md)), Chunk 4 ([Trade & Logistics](Trade_And_Logistics_Chunk4.md)), Chunk 5 ([Markets & Prices](Markets_And_Prices_Chunk5.md))  
**Feeds into:** Social / political simulation (villages, guilds, elites, courts), Strategic AI (empires, guilds, megacorps), Event systems (strikes, riots, coups, sanctions, wars)  
**Last Updated:** 2025-01-27

---

## Purpose

Chunk 6 establishes the policy layer of the economy – the system that allows rulers and institutions to change the rules and how those rules feed back into wealth, trade, and unrest. It answers a fundamental question:

**"Given a working economy and markets, how do rulers and institutions change the rules – and how do those rules feed back into wealth, trade, and unrest?"**

This chunk adds a policy layer that:
- Defines fiscal policy: taxes, subsidies, public spending
- Defines trade policy: tariffs, embargoes, sanctions, quotas
- Defines credit policy: loans, interest, defaults, debt-based leverage
- Defines enforcement vs smuggling: how hard it is to evade rules, and at what risk
- Hooks all of that into social and political tension: protests, strikes, revolts, coups

**Compliance:** All systems in Chunk 6 must:
- ✅ Set rules/parameters that lower chunks use when they run
- ✅ Use Chunk 1 transaction APIs for all wealth changes (taxes, loans, fines)
- ✅ Use Chunk 2 inventory APIs for asset seizure/confiscation
- ✅ Provide multipliers/permissions for Chunk 4 (tariffs, embargoes)
- ✅ Provide multipliers/permissions for Chunk 5 (transaction taxes, embargo checks)
- ✅ Emit signals (not full behavior) for social/political systems

**Note:** Chunk 6 does not manually push coins or items around; it sets rules/parameters that lower chunks use when they run.

---

## Scope & Boundaries

### In Scope (Chunk 6)

**Taxation & redistribution:**
- Income / wage tax
- Business / profit tax
- Trade/transaction taxes (on market trades)
- Wealth/tier-based taxes, tithes, charity obligations
- Basic budget "where tax goes" (welfare, schools, army, court, infrastructure)

**Trade policy:**
- Tariffs by good type & origin
- Embargoes / trade bans
- Sanctions on specific nodes/factions/goods
- Quotas & export controls

**Debt & credit:**
- Rules for loans between villages, guilds, dynasties, empires
- Interest accrual
- Default consequences: asset seizure, tribute, political vassalization, casus belli

**Enforcement, smuggling & black markets:**
- Enforcement level per node
- Smuggling difficulty, detection, penalties
- Black market prices & availability for banned/over-taxed goods

**Macro → unrest hooks:**
- How harsh policy + inequality + shortages → protests, strikes, riots, coups, revolts
- These are signals into your social/violence systems, not full AI here

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed debt mechanics, loan behaviors, and vassalization patterns.

---

### Out of Scope (For This Chunk)

**Micro behavior:**
- ❌ Individual villager job choice
- ❌ One-off bargains

**Full narrative/political simulation:**
- ❌ Parliaments, elections, diplomacy
- ❌ Detailed political decision-making (that's in social/political layer)

**Detailed "design" of unrest events:**
- ❌ Chunk 6 only triggers unrest signals
- ❌ Full event behavior lives in the social/violence layer

---

## Layering Rules (Re-stated for Policies)

Chunk 6 is the top economic layer in your spine. It must obey:

### Single Source of Truth

**Tax rates, tariffs, interest rules live in policy components & catalogs, not sprinkled constants:**
- ✅ All policy data in PolicySpec catalogs (BlobAssets)
- ✅ Policy components reference these catalogs
- ❌ No hardcoded tax rates or tariff values in systems

**All effects on wealth/items still go through Chunks 1–5 (transactions & inventory ops):**
- ✅ Taxes use Chunk 1 transaction APIs
- ✅ Asset seizure uses Chunk 2 inventory APIs
- ✅ Tariffs add costs via Chunk 4 route cost calculations
- ❌ No direct `Treasury.Balance += tax` without transactions

### Layers, Not Tangles

**Chunk 6 can read everything below (wealth, items, markets, trade flows, production):**
- ✅ Read Chunk 1 wealth balances for tax calculations
- ✅ Read Chunk 2 inventories for asset valuation
- ✅ Read Chunk 3 production for profit tax
- ✅ Read Chunk 4 trade flows for tariff application
- ✅ Read Chunk 5 prices for transaction tax calculations

**It may not directly modify inventories or ledgers except via:**
- ✅ Tax/loan "instructions" that call the same transaction APIs everyone else uses
- ✅ Policy multipliers/permissions that lower chunks read and apply
- ❌ Direct component modifications bypassing Chunk APIs

### Data > Code

**Policy types, thresholds, and effects are data-driven:**
- ✅ Tax brackets, tariff tables, smuggling penalties, loan rules in catalogs
- ✅ Systems here are generic interpreters of that data
- ❌ Policy logic hardcoded in systems

**Example:**
```
TaxPolicy Catalog Entry:
  IncomeTaxBrackets: [
    { MinWealth: 0, MaxWealth: 100, Rate: 0.05 },
    { MinWealth: 100, MaxWealth: 500, Rate: 0.15 },
    { MinWealth: 500, MaxWealth: Infinity, Rate: 0.30 }
  ]
  
System reads catalog and applies brackets generically.
No hardcoded "if wealth > 500, tax = 30%" in code.
```

---

## Policy Data Model

### Fiscal Policy (Taxes + Budget)

**Per governing entity (village, realm, empire):**

**TaxPolicy:**
- `IncomeTaxBrackets` – Array of {MinWealth, MaxWealth, Rate} for wealth tier-based income tax
- `BusinessProfitTaxRate` – Flat or bracketed rate on business profits
- `TransactionTaxRates` – Map of {GoodType, Rate} for market trade taxes
- `SpecialLevies` – Array of special taxes:
  - War tax multiplier
  - Tithes (religious obligation)
  - Guild dues multipliers

**BudgetPolicy:**
- Target % of revenue allocated to:
  - `PublicSchoolsEducation` – Education funding
  - `ReliefWelfare` – Food, basic goods for poor
  - `Infrastructure` – Roads, walls, ports
  - `MilitarySecurity` – Army upkeep, patrols, garrisons
  - `CourtElites` – Court expenses, elite maintenance
  - `ReservesDebtRepayment` – Savings, loan payments

**These live in a PolicySpec catalog so you can define "Spartan Militarist", "Feudal Tax Farm", "Merchant Republic", etc.**

**Example (Conceptual):**
```
TaxPolicy "Feudal Tax Farm":
  IncomeTaxBrackets: [
    { MinWealth: 0, MaxWealth: 50, Rate: 0.20 },    // Poor: 20%
    { MinWealth: 50, MaxWealth: 200, Rate: 0.35 },   // Middle: 35%
    { MinWealth: 200, MaxWealth: Infinity, Rate: 0.50 } // Rich: 50%
  ]
  BusinessProfitTaxRate: 0.40
  TransactionTaxRates: {
    "Grain": 0.10,  // 10% tax on grain trades
    "Weapon": 0.25  // 25% tax on weapon trades
  }
  SpecialLevies: {
    WarTax: 1.5,   // 50% extra during war
    Tithes: 0.10    // 10% to church
  }

BudgetPolicy "Feudal Tax Farm":
  PublicSchoolsEducation: 0.05   // 5% of revenue
  ReliefWelfare: 0.10            // 10% of revenue
  Infrastructure: 0.15           // 15% of revenue
  MilitarySecurity: 0.40         // 40% of revenue (high)
  CourtElites: 0.20               // 20% of revenue
  ReservesDebtRepayment: 0.10    // 10% of revenue
```

---

### Trade Policy

**Per governing entity and/or per pair of nodes:**

**TariffPolicy:**
- `ImportTariffs` – Map of {GoodType, Rate} for import tariffs
- `ExportTariffs` – Map of {GoodType, Rate} for export tariffs
- `PartnerTariffs` – Map of {PartnerNode/Faction, Multiplier} for extra tariffs on specific partners
- `GoodTypeCategories` – Categories (Food, Weapon, Luxury, Contraband) for bulk tariff application

**EmbargoPolicy:**
- `ForbiddenImports` – Array of {GoodType, SourceNode/Faction} pairs that are banned
- `ForbiddenExports` – Array of {GoodType, DestinationNode/Faction} pairs that are banned
- `EnforcementLevel` – How strictly embargo is enforced (affects smuggling difficulty)

**SanctionPolicy:**
- `TargetNode/Faction` – Who is sanctioned
- `TariffMultiplier` – Extra tariffs (e.g., 2.0 = double normal tariffs)
- `EmbargoedGoodTypes` – Array of GoodTypes that cannot be traded
- `AssetFreeze` – Boolean: freeze target's assets in this node

**Trade policy does not move anything; it just provides multipliers and permissions used by Chunk 4 (routes) and Chunk 5 (market trades).**

**Example (Conceptual):**
```
TariffPolicy "Protectionist":
  ImportTariffs: {
    "Grain": 0.20,      // 20% import tax on grain
    "Weapon": 0.50,     // 50% import tax on weapons
    "Luxury": 0.30      // 30% import tax on luxuries
  }
  ExportTariffs: {
    "Grain": 0.05       // 5% export tax (encourage exports)
  }
  PartnerTariffs: {
    "VillageB": 1.5     // 50% extra tariffs on VillageB goods
  }

EmbargoPolicy "War Embargo":
  ForbiddenImports: [
    { GoodType: "Weapon", Source: "VillageB" },
    { GoodType: "IronIngot", Source: "VillageB" }
  ]
  ForbiddenExports: [
    { GoodType: "Grain", Destination: "VillageB" }
  ]
  EnforcementLevel: 0.8  // High enforcement (80% detection chance)

SanctionPolicy "Economic Sanctions":
  Target: "VillageB"
  TariffMultiplier: 3.0   // Triple all tariffs
  EmbargoedGoodTypes: ["Weapon", "IronIngot", "Luxury"]
  AssetFreeze: true        // Freeze VillageB assets
```

---

### Credit & Debt Policy

**At macro level (villages, guilds, dynasties, empires):**

**LoanRuleSpec:**
- `AllowedLenderTypes` – Array of entity types that can lend (Village, Guild, Dynasty, Empire)
- `AllowedBorrowerTypes` – Array of entity types that can borrow
- `InterestFormula` – Formula for interest calculation (fixed rate, variable, compound)
- `GracePeriod` – Days/ticks before interest starts accruing
- `RepaymentSchedule` – Payment frequency and amounts
- `AcceptableCollateralTypes` – Array of {CollateralType, ValuationMultiplier}:
  - Land, assets, trade rights, future tax revenue

**DefaultResponseSpec:**
- `SoftResponse` – Extended deadline, increased interest
- `MediumResponse` – Asset seizure, forced sale, revenue garnishing
- `HardResponse` – Political vassalization, loss of autonomy, casus belli

**This defines how loans work; actual loan instances live as Loan components/records between wallets.**

**Example (Conceptual):**
```
LoanRuleSpec "Standard Inter-Village Loan":
  AllowedLenderTypes: [Village, Guild, Dynasty]
  AllowedBorrowerTypes: [Village, Guild]
  InterestFormula: FixedRate
  BaseInterestRate: 0.10  // 10% annual
  GracePeriod: 30         // 30 days grace
  RepaymentSchedule: Monthly
  AcceptableCollateralTypes: [
    { Type: Mine, ValuationMultiplier: 0.8 },
    { Type: Building, ValuationMultiplier: 0.6 },
    { Type: TradeRoute, ValuationMultiplier: 0.5 }
  ]

DefaultResponseSpec "Feudal Default":
  SoftResponse: {
    ExtendedDeadline: 60,  // 60 more days
    IncreasedInterest: 1.5  // 50% more interest
  }
  MediumResponse: {
    AssetSeizure: true,
    RevenueGarnishing: 0.20  // 20% of revenue to lender
  }
  HardResponse: {
    Vassalization: true,
    AutonomyLoss: 0.50,  // 50% autonomy loss
    CasusBelli: true     // Lender can declare war
  }
```

---

### Enforcement & Smuggling Policy

**Per node/region:**

**EnforcementProfile:**
- `PatrolStrength` – Number/quality of patrols (affects detection chance)
- `InspectorCount` – Number of trade inspectors
- `CorruptionLevel` – How easy to bribe (0.0 = incorruptible, 1.0 = very corruptible)
- `LegalSeverity` – Punishment model:
  - Fine (wealth penalty)
  - Prison (temporary entity removal)
  - Execution (permanent entity removal)
  - Reputation hit

**SmugglingPolicy:**
- `BaseDifficulty` – Base detection chance for moving tariffed/illegal goods
- `DetectionChancePerLeg` – Probability per trade route leg
- `DetectionChancePerTransaction` – Probability per market transaction
- `PenaltyModel` – Map of {DetectionLevel, Penalty}:
  - Caught smuggling → wealth fines, confiscation, jail, rep, execution

**Black markets (see below) read these to set their risk/reward.**

**Example (Conceptual):**
```
EnforcementProfile "High Security":
  PatrolStrength: 50      // Strong patrols
  InspectorCount: 10      // Many inspectors
  CorruptionLevel: 0.1    // Low corruption (hard to bribe)
  LegalSeverity: {
    Fine: 1000,           // 1000 currency fine
    Prison: 30,           // 30 days prison
    Execution: false,     // No execution
    ReputationHit: -20    // -20 reputation
  }

SmugglingPolicy "Strict Enforcement":
  BaseDifficulty: 0.3     // 30% base detection chance
  DetectionChancePerLeg: 0.15  // 15% per route leg
  DetectionChancePerTransaction: 0.10  // 10% per market trade
  PenaltyModel: {
    FirstOffense: { Fine: 500, ReputationHit: -10 },
    RepeatOffense: { Fine: 2000, Prison: 60, ReputationHit: -30 },
    Contraband: { Confiscation: true, Fine: 5000, Prison: 120 }
  }
```

---

## Policy → Effect Flows

### Tax Collection

**Taxes sit on top of existing flows, not separate:**

**On wages & profits:**
- Payroll & profit systems (Chunk 3) call a helper:
  - "Compute tax on this payment given TaxPolicy"
- That helper:
  - Calculates amounts (bracketed or flat)
  - Issues wealth transactions:
    - `Payer → Treasury` (village/realm wallet)
    - Net to worker or business

**On market trades:**
- Market trade system (Chunk 5) calls a helper:
  - "Compute transaction tax for this GoodType, buyer/seller, direction"
- That helper:
  - Adds extra payments to/from treasury as separate transactions

**On wealth/tiers (if you want annual wealth tax):**
- Periodic pass over wallets:
  - Apply tier-based tax from data
  - Transactions: `wallet → treasury`

**No system should ever just do `Treasury += X` without a transaction record.**

**Example:**
```
Wage Payment (Chunk 3):
  Gross Wage: 100 currency
  Tax Helper: ComputeTax(100, TaxPolicy)
    → Income Tax: 20 currency (20% bracket)
  Transactions:
    Employer → Worker: 80 currency (net wage)
    Worker → Treasury: 20 currency (tax)

Market Trade (Chunk 5):
  Trade: 1000 grain × 0.2 currency/kg = 200 currency
  Tax Helper: ComputeTransactionTax("Grain", 200, TaxPolicy)
    → Transaction Tax: 20 currency (10% on grain)
  Transactions:
    Buyer → Seller: 200 currency (trade)
    Buyer → Treasury: 20 currency (transaction tax)
```

---

### Budget Allocation

**Budget doesn't directly push coins to entities; it modifies how much funding flows into systems like:**
- Education institutions (already planned in your Elite/Education pipelines)
- Military & security (bands/armies upkeep, patrol presence, garrisons)
- Infrastructure upkeep (road maintenance, walls, ports)
- Welfare systems (food or minimum income to worst-off)

**Mechanically:**
- Treasury has inflows (tax, tariffs, loans)
- A `BudgetExecutionSystem`:
  - Reads BudgetPolicy
  - For each category:
    - Sets per-system funding multipliers:
      - e.g., `EducationFundingMultiplier = 1.2`, `WelfareFundingMultiplier = 0.5`
  - Optionally caps: if treasury can't cover full plan, underfund less-priority categories

**Those multipliers are used by lower-level systems (Education, Band upkeep, Infrastructure, Welfare) when they compute their own transactions and outcomes (more teachers, better schools, more patrols, etc.).**

**Example:**
```
Treasury Inflows (This Month):
  Taxes: 5000 currency
  Tariffs: 1000 currency
  Loans: 2000 currency
  Total: 8000 currency

BudgetPolicy Allocation:
  Education: 0.15 → 1200 currency → EducationFundingMultiplier = 1.2
  Welfare: 0.10 → 800 currency → WelfareFundingMultiplier = 0.8
  Infrastructure: 0.20 → 1600 currency → InfrastructureFundingMultiplier = 1.0
  Military: 0.40 → 3200 currency → MilitaryFundingMultiplier = 1.1
  Court: 0.10 → 800 currency → CourtFundingMultiplier = 0.9
  Reserves: 0.05 → 400 currency

Education System:
  Reads EducationFundingMultiplier = 1.2
  Computes: BaseCost × 1.2 = ActualFunding
  Uses funding to hire teachers, build schools, etc.
```

---

### Trade Policy: Tariffs, Embargoes, Sanctions

#### Tariffs

**Applied at border crossings or market entry:**
- When goods enter a node from another node, logistic system (Chunk 4) calls:
  - "Compute tariff for cargo manifest given TariffPolicy of destination"
- That helper:
  - Sums taxable value from GoodTypes × TariffRates
  - Creates transaction:
    - `Payer (owner of cargo) → Treasury`
  - May set a "tariff-paid" flag on cargo to avoid double-taxing

**Example:**
```
Caravan Arrives at VillageA:
  Cargo: 1000 kg grain, 500 kg iron ingots
  TariffPolicy Helper: ComputeTariff(cargo, TariffPolicy)
    Grain: 1000 kg × 0.2 currency/kg × 0.20 (20% tariff) = 40 currency
    IronIngot: 500 kg × 5.0 currency/kg × 0.30 (30% tariff) = 750 currency
    Total Tariff: 790 currency
  
  Transactions:
    CaravanOwner → VillageATreasury: 790 currency (tariff)
    Cargo marked "tariff-paid"
```

---

#### Embargoes & Quotas

**Embargo:**
- Policy that forbids specific goods from specific origins/destinations
- Logistics (Chunk 4) and Market Clearing (Chunk 5) check:
  - If trade violates embargo → block or route to smuggling path

**Quotas:**
- Limit how many units of some GoodType can be imported/exported per period
- Keep counters per period; once quota reached, further shipments:
  - Either blocked, or charged punitive tariffs, or forced into smuggling

**Example:**
```
EmbargoPolicy Check (Chunk 4):
  Trade: VillageB → VillageA, 500 kg weapons
  Check: Is "weapons from VillageB" embargoed?
    → Yes, embargoed
  Action: Block trade OR route to smuggling path

Quota Check:
  Quota: 1000 kg grain/month from VillageC
  Current Month Imports: 800 kg
  New Shipment: 500 kg
  Result: First 200 kg allowed, remaining 300 kg blocked OR punitive tariff
```

---

#### Sanctions

**A higher-intensity policy bundle:**
- High tariffs + embargoes + asset freezing for target

**Effects:**
- Effective price spikes
- Trade routes disabled or heavily penalized
- Incentive for smuggling & black markets

**Chunk 6 does not decide sanctions – that's AI/Player; it only defines how they work.**

**Example:**
```
SanctionPolicy "Economic Sanctions on VillageB":
  TariffMultiplier: 3.0  // Triple all tariffs
  EmbargoedGoodTypes: ["Weapon", "IronIngot"]
  AssetFreeze: true
  
Effects:
  - VillageB goods face 3× normal tariffs
  - Weapons/iron cannot be traded legally
  - VillageB assets in VillageA frozen (cannot access)
  - Smuggling becomes attractive (bypass sanctions)
```

---

## Debt & Vassalization

### Loan Lifecycle

**Between any two wealth-owning roles (village ↔ village, guild ↔ dynasty, etc.):**

**Loan record:**
- Lender, borrower
- Principal, interest rate
- Payment schedule & due dates
- Collateral info or "implied collateral" (future taxes, trade rights)

**A LoanSystem:**
- Accrues interest periodically
- Schedules and applies payments as wealth transactions
- When borrower cannot pay:
  - Consults DefaultResponseSpec to pick outcome:
    - More time vs seizing assets vs forcing tribute

**Example:**
```
Loan Created:
  Lender: VillageA
  Borrower: VillageB
  Principal: 10000 currency
  InterestRate: 0.10 (10% annual)
  PaymentSchedule: Monthly, 1000 currency/month

LoanSystem (Monthly):
  Accrue Interest: 10000 × 0.10 / 12 = 83.33 currency
  Scheduled Payment: 1000 currency
  Total Due: 1083.33 currency
  
  Check Borrower Balance:
    VillageB Treasury: 500 currency (insufficient)
  
  Default Response:
    MissedPaymentCount: 1
    If MissedPaymentCount >= 3:
      → Trigger DefaultResponseSpec (asset seizure, vassalization)
```

---

### Debt Vassalization

**At high-level (villages/realms):**

**If a borrower defaults repeatedly:**
- Policy may force:
  - Long-term tribute (slice of tax revenue)
  - Control over trade routes (lender gets veto over tariffs or quotas)
  - De facto vassal/subject status (used by political layer)

**This doesn't have to be deeply simulated at first – a simple flag & tribute rule is enough:**
- Borrower becomes `VassalOf = Lender`
- A percentage of its treasury inflows automatically rerouted

**Example:**
```
VillageB Defaults (3+ missed payments):
  DefaultResponseSpec: HardResponse
  Actions:
    1. Asset Seizure: VillageB mine → VillageA ownership
    2. Revenue Garnishing: 30% of VillageB tax revenue → VillageA
    3. Vassalization: VillageB.VassalOf = VillageA
    
Monthly Tribute:
  VillageB Tax Revenue: 2000 currency
  Tribute: 2000 × 0.30 = 600 currency
  Transaction: VillageBTreasury → VillageATreasury: 600 currency (tribute)
  VillageB Keeps: 1400 currency
```

**Reference:** See [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) for detailed debt mechanics, loan behaviors, and vassalization patterns.

---

## Enforcement, Smuggling & Black Markets

### Enforcement

**Per node:**
- Enforcement level influences:
  - Chance to catch untaxed trades
  - Tariff evasion at borders
  - Prohibited goods crossing
  - Severity of punishment

**Enforcement uses existing skills (stealth, perception, corruption) and band/patrol presence from your social/army systems.**

**Example:**
```
VillageA Enforcement:
  PatrolStrength: 50
  InspectorCount: 10
  CorruptionLevel: 0.1 (low)
  
Detection Chance:
  Base: 0.3 (30%)
  + Patrol Modifier: +0.2 (strong patrols)
  + Inspector Modifier: +0.1 (many inspectors)
  - Corruption Modifier: -0.05 (low corruption)
  Total: 0.55 (55% detection chance)
```

---

### Smuggling

**If a trade is blocked or would incur huge tariffs:**
- Traders (or AI) may choose to smuggle:
  - Mark trade as smuggling attempt
  - Logistics system applies:
    - Alternative risk model:
      - Higher event chance (confiscation, prison, execution)
    - If detected:
      - Goods seized (inventory lost)
      - Fines or harsher punishments (wealth & reputation hits)

**Smuggling still uses Chunk 2/4 flows; Chunk 6 only provides probabilities & consequences.**

**Example:**
```
Smuggling Attempt:
  Trade: VillageB → VillageA, 500 kg weapons (embargoed)
  Marked as "smuggling"
  
  Chunk 4 Route Risk:
    Normal Risk: 0.15 (bandits)
    Smuggling Risk: 0.15 + 0.55 (enforcement) = 0.70 (70% detection)
  
  Roll: Detected (70% chance)
  Consequences:
    - Goods seized: 500 kg weapons → VillageA Treasury (confiscated)
    - Fine: 2000 currency → VillageBTreasury → VillageATreasury
    - Reputation Hit: -20 (VillageB reputation with VillageA)
```

---

### Black Markets

**For heavily restricted goods:**
- Node can spawn a `BlackMarket` component:
  - Lists contraband GoodTypes
  - Has its own price multipliers (often > normal market)
  - Black market trades:
    - Bypass tariffs but carry high enforcement risk
    - Use the same MarketBuyIntent / MarketSellIntent pattern but flagged as illegal

**Example:**
```
VillageA Black Market:
  ContrabandGoodTypes: ["Weapon", "Contraband"]
  PriceMultiplier: 1.5  // 50% markup over normal market
  
Normal Market Weapon Price: 50 currency/unit
Black Market Weapon Price: 75 currency/unit (1.5×)

Trade:
  Buyer pays 75 currency/unit (higher price)
  Seller receives 75 currency/unit (no tariff)
  Risk: 70% detection chance (high enforcement)
  If detected: Confiscation + fine + reputation hit
```

---

## Macro → Unrest Hooks

**Chunk 6 turns "bad macro" into stress signals for your social/violence spines:**

### Inputs to Unrest

**High effective tax burden on poor tiers:**
- Tax burden = (Tax Paid / Income) for each wealth tier
- If poor tier burden > threshold → unrest signal

**Severe price spikes (especially food / basic goods):**
- Price index = (CurrentPrice / BasePrice) for essential goods
- If price index > threshold → unrest signal

**Unemployment / business failures:**
- Unemployment rate = (Unemployed / Total Workers)
- If unemployment > threshold → unrest signal

**Perceived injustice:**
- Corruption level (unequal enforcement)
- Sanctions hitting commoners (not elites)
- Tax burden inequality (poor taxed more than rich)

**Heavy debt vassalization or predatory lending:**
- Debt-to-revenue ratio
- If ratio > threshold → unrest signal

### Output Types (Signals, Not Full Behavior)

**TaxProtestEvent:**
- Petitions, marches
- Triggered by: High tax burden + perceived injustice

**StrikeEvent:**
- Guilds/workers strike
- Triggered by: High tax burden + unemployment + low wages

**RiotEvent:**
- If protests mishandled + hunger + harsh repression
- Triggered by: Multiple unrest factors + failed protest resolution

**CoupEvent potentials:**
- Elites vs ruler
- Triggered by: Elite dissatisfaction + treasury crisis + military support

**RevoltEvent potentials:**
- Villages vs empire
- Triggered by: Heavy vassalization + food shortages + military weakness

**Other systems (villages/guilds/courts/armies) read these and decide how to act (your existing social docs already outline a lot of this).**

**Example:**
```
VillageA Unrest Calculation:
  Tax Burden (Poor Tier): 0.40 (40% of income)
  Tax Burden (Rich Tier): 0.20 (20% of income)
  Inequality: 0.20 (20% difference)
  
  Food Price Index: 2.5 (250% of base price - famine)
  Unemployment Rate: 0.30 (30% unemployed)
  
  Debt-to-Revenue: 3.0 (debt = 3× annual revenue)
  
Unrest Signals Generated:
  - TaxProtestEvent (high tax burden + inequality)
  - RiotEvent (food price spike + unemployment + high tax)
  - CoupEvent (debt crisis + elite dissatisfaction)
  
Social/Political Systems:
  Read these signals and decide:
    - Village council responds to protests
    - Military suppresses riots
    - Elites plot coup
```

---

## Telemetry & Debug

Chunk 6 adds some powerful debug views:

### Per Node

**Tax rates by bracket & type:**
- Current tax policy breakdown
- Effective tax rates per wealth tier
- Transaction tax rates per GoodType

**Effective tax share of income & trade:**
- What % of income goes to taxes
- What % of trade value goes to tariffs/transaction taxes

**Tariffs/sanctions in effect:**
- Active tariff rates
- Embargoed goods/partners
- Sanction targets

**Debt status:**
- Owed, to whom, terms
- Interest accrual
- Default risk

**Enforcement level & smuggling attempts:**
- Patrol strength, inspector count
- Corruption level
- Smuggling detection rate
- Recent smuggling attempts (success/failure)

**Example:**
```
VillageA Policy Debug View:
  Tax Rates:
    Income Tax (Poor): 20%
    Income Tax (Rich): 50%
    Business Tax: 40%
    Transaction Tax (Grain): 10%
  
  Effective Tax Share:
    Poor Tier: 25% of income
    Rich Tier: 35% of income
    Trade: 15% of trade value
  
  Trade Policy:
    Import Tariffs: Grain 20%, Weapon 50%
    Embargoes: Weapons from VillageB
    Sanctions: VillageB (3× tariffs, asset freeze)
  
  Debt Status:
    Owed to VillageC: 10000 currency (10% interest)
    Monthly Payment: 1000 currency
    Missed Payments: 1/3 (default risk: medium)
  
  Enforcement:
    Patrol Strength: 50
    Corruption: 0.1 (low)
    Detection Rate: 55%
    Smuggling Attempts (This Month): 5 (3 detected, 2 successful)
```

---

### Global

**Who is sanctioning whom:**
- Sanction network graph
- Shows economic warfare relationships

**Debt network graph:**
- Lender → Borrower relationships
- Debt amounts and default risks
- Vassalization chains

**Hotspots of unrest risk due to economic pressure:**
- Map showing nodes with high unrest signals
- Color-coded by unrest type (protests, riots, coups, revolts)

**Example:**
```
Global Policy View:
  Sanction Network:
    VillageA → VillageB (economic sanctions)
    VillageC → VillageB (trade embargo)
    Empire → VillageD (full sanctions)
  
  Debt Network:
    VillageA → VillageB: 10000 currency (default risk: high)
    VillageC → VillageB: 5000 currency (default risk: low)
    VillageB → VillageD: 2000 currency (vassalized)
  
  Unrest Hotspots:
    VillageB: High (debt crisis + sanctions + high tax)
    VillageD: Medium (vassalization + food shortage)
    VillageE: Low (stable economy)
```

---

## "Done Enough" Checklist for Chunk 6

Chunk 6 can be marked as "done enough" when all of the following are verified:

### ✅ Fiscal Policy
- [ ] `TaxPolicy` & `BudgetPolicy` catalogs exist for at least a few archetypes (low-tax, war-tax, oppressive, welfare-heavy)
- [ ] Payroll & profit flows call into tax helpers; taxes arrive in treasuries as transactions
- [ ] Budget multipliers influence at least:
  - Education/Schools
  - Military/Patrols
  - Basic welfare or public works

### ✅ Trade Policy
- [ ] `TariffPolicy` applies extra costs on cross-node cargo or market trades
- [ ] `EmbargoPolicy` blocks specific goods/partners at the policy level
- [ ] `SanctionPolicy` can be defined and causes noticeable friction in trade

### ✅ Debt
- [ ] Loans can be created between nodes/entities with principal & interest
- [ ] A simple interest accrual + repayment schedule works
- [ ] On default, at least one consequence works (tribute or vassal flag)

### ✅ Enforcement & Smuggling
- [ ] Enforcement level affects detection chance on smuggling/tax-evasion attempts
- [ ] Smuggling trades can happen and sometimes succeed/fail with clear consequences
- [ ] Optional black market path exists for at least one contraband good

### ✅ Unrest Hooks
- [ ] Simple economic stress metrics (tax burden, price index, unemployment) are computed per node
- [ ] These metrics generate unrest signals when thresholds are crossed (e.g., "High Hunger + High Tax → Protest")
- [ ] Other systems (villages/guilds/armies) can read these signals (even if their full reactions aren't finished yet)

---

## Integration Notes

### Related Documents

This specification builds on and integrates with:

- **Chunk 0**: [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md) – Foundation principles
- **Chunk 1**: [`Wealth_And_Ledgers_Chunk1.md`](Wealth_And_Ledgers_Chunk1.md) – Wealth layer for tax/loan transactions
- **Chunk 2**: [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md) – Inventory layer for asset seizure
- **Chunk 3**: [`Businesses_And_Production_Chunk3.md`](Businesses_And_Production_Chunk3.md) – Production layer for profit tax
- **Chunk 4**: [`Trade_And_Logistics_Chunk4.md`](Trade_And_Logistics_Chunk4.md) – Trade layer for tariff application
- **Chunk 5**: [`Markets_And_Prices_Chunk5.md`](Markets_And_Prices_Chunk5.md) – Market layer for transaction taxes and embargo checks
- **Trade & Commerce**: [`Trade_And_Commerce_System.md`](Trade_And_Commerce_System.md) – Detailed debt mechanics, loan behaviors, vassalization patterns

### PureDOTS Integration

Chunk 6 systems should integrate with PureDOTS patterns:

- **Resource Systems**: Use existing transaction APIs from Chunk 1, inventory APIs from Chunk 2
- **Registry Infrastructure**: Use entity registries for efficient policy/loan queries
- **Time/Rewind**: Use `TickTimeState` for periodic tax/interest calculations, respect `RewindState` for deterministic simulation
- **Component Patterns**: Follow DOTS component patterns from TRI_PROJECT_BRIEFING.md
- **Burst Compliance**: Policy calculations should be Burst-compatible where possible

### Cross-Project Applicability

These patterns apply to both:

- **Godgame**: Villages, taxes, tariffs, debt, vassalization, unrest
- **Space4X**: Colonies, taxes, tariffs, debt, vassalization, unrest

Examples in this document use Godgame terminology (villages, grain, caravans), but the same patterns apply to Space4X (colonies, ores, carriers) with appropriate entity type substitutions.

---

**Last Updated:** 2025-01-27  
**Maintainer:** Economy Architecture Team  
**Status:** Design – Foundation. Implementation work should follow this specification.

**Note:** Once Chunk 6 is complete, you've basically completed the economic spine:
- **0:** Principles
- **1:** Wealth
- **2:** Stuff
- **3:** Production
- **4:** Movement
- **5:** Prices
- **6:** Rules & Power


















