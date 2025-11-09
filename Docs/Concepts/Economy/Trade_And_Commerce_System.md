# Trade & Commerce System

**Type:** Core Mechanic
**Status:** `<Draft>` - Concept Design
**Version:** Concept v1.0
**Dependencies:** Resource Production & Crafting, Business & Assets, Entity Relations, Alignment Framework, Wealth & Social Dynamics, Village Stats
**Last Updated:** 2025-11-07

---

## Overview

**Trade & Commerce** is the economic circulation system that moves resources, materials, and finished products between villages, businesses, and individuals. It creates interconnected economic networks, price dynamics, and strategic trade relationships.

**Key Features:**
- **Trade Routes:** Established paths between villages with transportation costs
- **Merchant Caravans:** Mobile trade entities that transport goods
- **Stagecoaches:** Passenger and light cargo transport services
- **Market Pricing:** Dynamic supply/demand-based pricing
- **Trade Agreements:** Diplomatic economic pacts (tariffs, embargoes, free trade)
- **Smuggling:** Illegal trade bypassing tariffs and embargoes
- **Economic Specialization:** Villages become known for specific exports
- **Price Arbitrage:** Merchants profit from price differences between villages

**For Narrative:** Silk Road merchant caravans (cross-cultural trade), Hanseatic League trade networks (merchant guilds controlling routes), Venice-Ottoman trade agreements (economic diplomacy), prohibition-era smuggling (evading embargoes).

---

## Core Concepts

### Trade Routes

**Trade routes** are established paths between two villages with calculated transportation costs based on distance, terrain, and security.

```
Trade Route: Village A ↔ Village B

Properties:
  - Distance: 150 km
  - Terrain: Mixed (plains 70%, hills 30%)
  - Security: Medium (occasional bandit raids)
  - Established: Yes (regular traffic, maintained roads)
  - Travel Time: 3 days (stagecoach), 5 days (caravan)

Transportation Costs (per 100kg per 100km):
  - Stagecoach: 5 currency (fast, light cargo, passengers)
  - Caravan: 3 currency (slow, heavy cargo, bulk goods)

Modifiers:
  - Allied villages: -20% cost (diplomatic discount)
  - Rival villages: +50% cost (tariffs, inspections)
  - War: +100% cost (extreme danger premium)
  - Established route: -10% cost (infrastructure, safety)
```

**Route Quality Levels:**
```
Uncharted (0-20 trips):
  - No established infrastructure
  - High bandit risk (+30% ambush chance)
  - No inns/rest stops
  - Base cost: ×1.5

Developing (21-100 trips):
  - Minimal infrastructure
  - Moderate bandit risk (+15% ambush chance)
  - Few rest stops
  - Base cost: ×1.2

Established (101-500 trips):
  - Good infrastructure (roads, bridges)
  - Low bandit risk (+5% ambush chance)
  - Regular rest stops, stables
  - Base cost: ×1.0 (baseline)

Major Trade Route (501+ trips):
  - Excellent infrastructure
  - Minimal bandit risk (+1% ambush chance)
  - Frequent inns, caravanserais
  - Guard patrols (village-funded)
  - Base cost: ×0.8 (economies of scale)
```

---

## Transportation Services

### Stagecoaches

**Stagecoaches** are fast passenger and light cargo transport operated by stage station businesses.

```
Stagecoach Properties:
  - Capacity: 4 passengers + 200kg cargo
  - Speed: 50 km/day (fast)
  - Cost: 5 currency per 100kg per 100km
  - Passenger Fare: 10 currency per 100km
  - Reliability: High (schedule-based)

Operating Model:
  - Owned by Stage Station business (see Business & Assets)
  - Revenue: Passenger fares + cargo fees
  - Expenses: Horse maintenance, driver wages, repairs
  - Profit: 30-50% margin on successful routes

Example Route:
  Village A → Village B (150 km)

  Passenger Revenue:
    3 passengers × 10 currency × 1.5 (distance factor) = 45 currency

  Cargo Revenue:
    100kg cargo × 5 currency × 1.5 = 7.5 currency per trip

  Total Revenue: 52.5 currency per trip

  Expenses:
    Driver wage: 5 currency
    Horse feed: 3 currency
    Maintenance: 2 currency
    Total: 10 currency

  Net Profit: 42.5 currency per trip
  Trips per week: 2 (3-day round trip)
  Monthly Profit: 42.5 × 8 = 340 currency
```

**Stagecoach Risks:**
```
Bandit Ambush:
  - Chance: Route security level
  - Loss: Cargo stolen, passengers killed/kidnapped
  - Impact: Reputation loss, refund demands
  - Mitigation: Hire guards (+10 currency per trip, -50% ambush chance)

Weather Delays:
  - Chance: 10% (winter 30%)
  - Loss: Delayed arrival, increased expenses
  - Impact: Customer dissatisfaction

Breakdown:
  - Chance: 5% per trip
  - Loss: Repair costs (20-50 currency)
  - Impact: Service interruption
```

### Merchant Caravans

**Merchant caravans** are large-scale cargo transport for bulk goods, operated by merchant businesses or guilds.

```
Caravan Properties:
  - Capacity: 2000kg cargo (10× stagecoach)
  - Speed: 30 km/day (slow, hauling weight)
  - Cost: 3 currency per 100kg per 100km
  - Reliability: Medium (weather-dependent)
  - Security: Self-funded guards

Operating Model:
  - Owned by Merchant Guild or wealthy merchants
  - Revenue: Cargo transport fees OR trade arbitrage
  - Expenses: Guards, animal feed, repairs, tolls
  - Profit: 20-40% margin

Example Trade Run:
  Village A → Village B (150 km)

  Cargo: 1500kg iron ingots
  Transport Cost: 1500kg × 3 currency × 1.5 = 67.5 currency

  Expenses:
    Guards (3): 15 currency
    Animal feed: 10 currency
    Tolls: 5 currency
    Total: 30 currency

  Net Transport Fee: 67.5 - 30 = 37.5 currency profit

  OR Arbitrage Model:
    Buy iron ingots in Village A: 1500kg × 2 currency/kg = 3000 currency
    Transport cost: 30 currency (self-funded)
    Sell in Village B: 1500kg × 3 currency/kg = 4500 currency

    Gross Profit: 4500 - 3000 - 30 = 1470 currency
    Margin: 49% (high-risk, high-reward)
```

**Caravan Types:**
```
Standard Caravan:
  - 5 wagons, 2000kg capacity
  - 3 guards
  - Moderate speed (30 km/day)
  - Cost: 3 currency/100kg/100km

Heavy Caravan:
  - 10 wagons, 5000kg capacity
  - 6 guards
  - Slow speed (20 km/day)
  - Cost: 2.5 currency/100kg/100km (bulk discount)
  - Best for: Ore, lumber, stone (heavy bulk goods)

Luxury Caravan:
  - 3 wagons, 1000kg capacity
  - 8 guards (elite protection)
  - Moderate speed (30 km/day)
  - Cost: 8 currency/100kg/100km (premium)
  - Best for: Gold, jewelry, enchanted items (high-value)
  - Armored wagons, concealment magic
```

---

## Market Pricing & Supply/Demand

### Base Prices

Every resource, material, and product has a **base price** that represents the average market value in a neutral, balanced economy.

```
Base Prices (examples):

Extracted Resources:
  - Iron ore: 1 currency/kg
  - Oak wood: 0.5 currency/kg
  - Granite: 0.8 currency/kg
  - Wheat: 0.3 currency/kg
  - Aloe herb: 2 currency/kg

Produced Materials:
  - Iron ingot: 2 currency/kg (refined, 2× ore value)
  - Steel ingot: 4 currency/kg (advanced, 4× ore value)
  - Oak lumber: 1.5 currency/kg (processed, 3× wood value)
  - Flour: 0.8 currency/kg (processed, 2.67× wheat value)

End Products:
  - Longsword (Common, Tech Tier 3): 80 currency
  - Longsword (Rare, Tech Tier 6): 400 currency
  - Longsword (Legendary, Tech Tier 3): 12,000 currency
  - Healing Potion (Common): 15 currency
  - Wagon (Common): 200 currency
```

### Dynamic Pricing

**Actual market prices** fluctuate based on local supply and demand, village wealth, and current events.

```
Price Formula:
  MarketPrice = BasePrice × SupplyDemandMultiplier × VillageWealthMultiplier × EventMultiplier

SupplyDemandMultiplier:
  = (Demand / Supply)^0.5

  Examples:
    High demand, low supply: (100 / 20)^0.5 = 2.24× (224% base price)
    Balanced: (50 / 50)^0.5 = 1.0× (100% base price)
    Low demand, high supply: (20 / 100)^0.5 = 0.45× (45% base price, glut)

VillageWealthMultiplier:
  Poor village (<5000 avg wealth): 0.7× (limited purchasing power)
  Mid village (5000-20000 avg wealth): 1.0× (standard)
  Wealthy village (>20000 avg wealth): 1.3× (high demand, luxury goods)

EventMultiplier:
  War (weapons demand): 2.0× for weapons/armor
  Siege (food shortage): 3.0× for food
  Festival (luxury demand): 1.5× for wine, entertainment
  Plague (medicine demand): 4.0× for healing potions
  Famine (food scarcity): 5.0× for grain/flour
  Bumper Harvest (surplus): 0.3× for grain/flour
```

**Example Market Prices:**

```
Iron Ingots in Village A:
  Base Price: 2 currency/kg
  Supply: 500 kg available (local blacksmith stockpile)
  Demand: 200 kg/month (10 blacksmiths need materials)

  SupplyDemandMultiplier: (200 / 500)^0.5 = 0.63×
  VillageWealthMultiplier: 1.0× (mid-wealth village)
  EventMultiplier: 1.0× (peacetime)

  MarketPrice: 2 × 0.63 × 1.0 × 1.0 = 1.26 currency/kg

  → Supply glut, buyers market, merchants buy cheap

Iron Ingots in Village B (at war):
  Base Price: 2 currency/kg
  Supply: 100 kg available (limited local production)
  Demand: 800 kg/month (army needs equipment)

  SupplyDemandMultiplier: (800 / 100)^0.5 = 2.83×
  VillageWealthMultiplier: 1.0×
  EventMultiplier: 2.0× (war emergency)

  MarketPrice: 2 × 2.83 × 1.0 × 2.0 = 11.32 currency/kg

  → Extreme scarcity, sellers market

Trade Arbitrage Opportunity:
  Buy in Village A: 1500 kg × 1.26 = 1890 currency
  Transport: 30 currency (caravan cost)
  Sell in Village B: 1500 kg × 11.32 = 16,980 currency

  Gross Profit: 16,980 - 1890 - 30 = 15,060 currency
  Margin: 797% (extraordinary profit during wartime scarcity)

  Risk: Village B may lose war and be unable to pay
```

---

## Trade Agreements & Economic Diplomacy

### Alliance Types

**Villages establish trade agreements** based on diplomatic relations and alignment compatibility.

```
Free Trade Agreement:
  Requirements:
    - Neutral or positive relations (+20)
    - Both villages Lawful or both Chaotic (alignment compatibility)

  Benefits:
    - Zero tariffs (no import/export taxes)
    - -20% transportation costs (infrastructure cooperation)
    - Shared market prices (equilibrium across both villages)
    - Priority access to scarce resources

  Duration: Indefinite (until relations deteriorate)

Tariff Treaty:
  Requirements:
    - Neutral relations (0-20)

  Benefits:
    - Reduced tariffs (10% instead of 30%)
    - Standard transportation costs
    - Market access for specific goods

  Duration: Renegotiated every 5 years

Embargo (Hostile):
  Requirements:
    - Negative relations (<-20)
    - War declaration OR severe grievance

  Effects:
    - Illegal to trade (official ban)
    - Tariff: 200% (if caught smuggling)
    - Village peacekeepers seize contraband
    - Diplomatic penalty if violated

  Duration: Until relations improve

Monopoly Agreement:
  Requirements:
    - Alliance (relations +40)
    - Authoritarian village

  Benefits:
    - Exclusive trade rights for specific goods
    - Example: Village A has exclusive rights to buy Village B's iron
    - Other villages blocked from trade

  Exploitation: Authoritarian villages can monopolize resources
```

### Tariffs & Taxes

**Tariffs** are taxes on imports/exports, used to generate revenue or protect local industries.

```
Tariff Formula:
  TariffCost = GoodsValue × TariffRate

  TariffRate determined by:
    - Village economic policy (protectionist vs free market)
    - Diplomatic relations (allied -20%, rival +50%)
    - War status (enemy +200%)
    - Alignment (Authoritarian high tariffs, Egalitarian low)

Tariff Rates by Alignment:
  Egalitarian Good (Free Market): 5-10%
  Neutral: 15-25%
  Authoritarian: 30-50% (protectionist, control imports)
  Materialistic: 40-60% (extractive, maximize revenue)

Example:
  Merchant imports 1000 kg steel ingots to Authoritarian Village A
  Base Value: 1000 kg × 4 currency/kg = 4000 currency
  Tariff Rate: 40% (protectionist)

  Tariff Cost: 4000 × 0.40 = 1600 currency
  Total Import Cost: 4000 + 1600 = 5600 currency

  Merchant must sell at >5.6 currency/kg to profit

  If local market price is 6 currency/kg:
    Revenue: 1000 × 6 = 6000 currency
    Profit: 6000 - 5600 = 400 currency (7% margin, tight)
```

**Tariff Evasion (Smuggling):**
```
Corrupt/Chaotic merchants may attempt to smuggle goods to avoid tariffs.

Smuggling Mechanics:
  Detection Chance:
    Base: 30%
    + Lawful village: +20% (strict enforcement)
    + Chaotic village: -15% (lax enforcement)
    + Corrupt village: -25% (bribeable guards)
    + High-value cargo: +10% (scrutinized)

  If Detected:
    - Tariff: 200% (penalty rate)
    - Fine: 50% of goods value
    - Cargo seized
    - Merchant reputation: -20
    - Criminal record (banned from future trade if repeat offender)

  If Successful:
    - Zero tariff paid
    - Full profit margin
    - Merchant reputation: -5 (unethical, if discovered later)

Example:
  Merchant smuggling 500 kg gold ingots
  Value: 500 × 10 = 5000 currency
  Tariff (if legitimate): 5000 × 0.40 = 2000 currency

  Detection Chance:
    Base: 30%
    High-value: +10%
    Neutral village: 0%
    Total: 40% chance detected

  If Detected:
    Tariff penalty: 5000 × 2.0 = 10,000 currency
    Fine: 5000 × 0.5 = 2,500 currency
    Total Loss: 10,000 + 2,500 = 12,500 currency (cargo seized too)

  If Successful:
    Tariff avoided: 2000 currency saved
    Risk-adjusted value: 2000 × 0.6 (success chance) = 1200 currency expected profit

  Decision: High-risk, moderate reward
```

---

## Economic Specialization

### Village Export Profiles

**Villages develop specializations** based on local resources, expertise, and tech tier.

```
Village A (Iron Mining Town):
  Primary Export: Iron ore, Iron ingots
  Production: 5000 kg iron ore/month
  Local Demand: 1000 kg/month (local blacksmiths)
  Export: 4000 kg/month

  Price Advantage:
    Local: 1.2 currency/kg (supply glut, -40% below base)
    Distant Village B: 2.5 currency/kg (scarcity, +25% above base)

  Trade Strategy:
    Export iron to Village B via merchant caravans
    Import finished weapons from Village B (they have better blacksmiths)

Village B (Military City):
  Primary Export: Weapons, Armor
  Production: 500 weapons/month (Legendary blacksmiths)
  Local Demand: 200 weapons/month (city guard)
  Export: 300 weapons/month

  Price Advantage:
    Legendary longswords: 12,000 currency (unique, no competition)
    Rare longswords: 400 currency (high quality, +33% above base)

  Trade Strategy:
    Import iron from Village A (cheap materials)
    Export high-quality weapons to multiple villages (monopoly on quality)

Village C (Agricultural Hub):
  Primary Export: Wheat, Flour, Bread
  Production: 50,000 kg wheat/month (fertile land)
  Local Demand: 10,000 kg/month
  Export: 40,000 kg/month

  Price Advantage:
    Wheat: 0.15 currency/kg (bumper harvest, -50% below base)

  Trade Strategy:
    Export wheat to urban villages (high population, limited agriculture)
    Import luxury goods (wine, jewelry) from wealthy villages
```

### Merchant Networks

**Merchant guilds and families** establish trade networks across multiple villages, profiting from arbitrage.

```
Merchant Family: House Vendari

Trade Network:
  Village A (Iron) ↔ Village B (Weapons) ↔ Village C (Food) ↔ Village D (Luxury)

Monthly Trade Cycle:
  Week 1:
    Buy 2000 kg iron in Village A: 1.2 currency/kg = 2400 currency
    Transport to Village B: 40 currency (caravan)
    Sell iron in Village B: 2.5 currency/kg = 5000 currency
    Profit: 5000 - 2400 - 40 = 2560 currency

  Week 2:
    Buy 300 weapons in Village B: 80 currency/each = 24,000 currency
    Transport to Village D: 150 currency (luxury caravan, secure)
    Sell weapons in Village D: 120 currency/each (wealthy buyers) = 36,000 currency
    Profit: 36,000 - 24,000 - 150 = 11,850 currency

  Week 3:
    Buy 10,000 kg wheat in Village C: 0.15 currency/kg = 1500 currency
    Transport to Village B: 50 currency (heavy caravan)
    Sell wheat in Village B: 0.4 currency/kg (urban demand) = 4000 currency
    Profit: 4000 - 1500 - 50 = 2450 currency

  Week 4:
    Buy 50 kg jewelry in Village D: 100 currency/kg = 5000 currency
    Transport to Village A: 30 currency (luxury caravan)
    Sell jewelry in Village A: 130 currency/kg (luxury demand) = 6500 currency
    Profit: 6500 - 5000 - 30 = 1470 currency

Monthly Profit: 2560 + 11,850 + 2450 + 1470 = 18,330 currency
Annual Profit: 18,330 × 12 = 219,960 currency

Result: House Vendari rises to Ultra High wealth strata through trade empire
```

### Village Production Priorities (Needs-Based Focus)

**Villages prioritize production** based on unmet needs, outsourcing from other villages when local capacity insufficient.

```
Priority System:

Villages assess monthly needs across all categories:
  1. Critical Survival (food, water, shelter)
  2. Security (weapons, armor, fortifications)
  3. Infrastructure (buildings, tools, maintenance)
  4. Luxury (entertainment, education, beautification)

ProductionPriority Calculation:
  Priority = (Demand - Supply) × ImportanceWeight

  ImportanceWeight:
    Critical Survival: 10.0× (starvation = death)
    Security: 5.0× (war = conquest)
    Infrastructure: 2.0× (development)
    Luxury: 1.0× (quality of life)

If (Demand > LocalProduction):
  → Village attempts local production increase (build businesses)
  → If insufficient capacity → Outsource via trade

If (LocalProduction > Demand):
  → Surplus becomes export opportunity
  → Merchants trade surplus to deficit villages

Example: Village A (Population 200, Under Siege)

Month 1 Needs Assessment:
  Food Demand: 2000 kg wheat/month (10 kg per villager)
  Food Production: 1500 kg wheat/month (3 farms)
  Deficit: 500 kg wheat/month

  Weapons Demand: 50 swords (city guard + militia expansion)
  Weapons Production: 10 swords/month (1 blacksmith)
  Deficit: 40 swords/month (urgent, war context)

Priority Calculation:
  Food Priority: (2000 - 1500) × 10.0 = 5,000 points
  Weapons Priority: (50 - 10) × 5.0 = 200 points

Village Council Decision:
  Priority 1: Secure food supply (highest priority)
    Action: Import 500 kg wheat from Village C via caravan
    Cost: 500 kg × 0.4 currency/kg + 20 currency transport = 220 currency

  Priority 2: Outsource weapon production (cannot meet demand internally)
    Action: Contract Village B blacksmith guild for 40 swords
    Cost: 40 swords × 120 currency/each = 4,800 currency
    Delivery: 3 months (production time)

  Priority 3: Build new farm (long-term food security)
    Cost: 2,000 currency + 200 labor days
    ETA: 6 months → +500 kg wheat/month (eliminates food deficit)

  Priority 4: Build second blacksmith (cannot afford during war)
    Cost: 5,000 currency (deferred, focus on survival)

Result: Village survives siege by outsourcing critical needs, goes into debt

Production Constraints:

Capacity Limits:
  Villages have limited business slots (1 per 20 villagers baseline)
  Example: Village with 200 villagers → 10 business slots
    Current: 3 farms, 1 blacksmith, 1 inn, 1 mill, 1 carpenter, 1 temple, 1 market
    Available: 1 slot (can build 1 more business)

Business Construction Time:
  Farm: 3-6 months
  Blacksmith: 6-12 months (complex)
  Factory: 12-24 months (industrial age)

Skilled Labor Shortage:
  Building blacksmith requires:
    - Master blacksmith (Expertise 7+) OR
    - Train new blacksmith (Education 60+, 2 years apprenticeship)

  If no skilled labor available:
    → Cannot build business (even if have funding)
    → MUST outsource from other villages

Capital Constraints:
  Village treasury: 3,000 currency
  Farm cost: 2,000 currency
  Blacksmith cost: 5,000 currency

  Decision: Can only afford farm (critical survival)
  Blacksmith deferred → Continued outsourcing dependency

Outsourcing Decision Matrix:

When to Outsource:
  ✅ Urgent need (war, famine) + No time to build capacity
  ✅ Specialized production (legendary blacksmith elsewhere)
  ✅ Capital shortage (cannot afford business construction)
  ✅ Skilled labor shortage (no experts available)
  ✅ Temporary need (festival, one-time project)

When to Build Locally:
  ✅ Consistent long-term demand (food, housing)
  ✅ Strategic independence (avoid supply chain vulnerability)
  ✅ Export opportunity (surplus = profit)
  ✅ Sufficient capital + skilled labor available
  ✅ Village has comparative advantage (cheap resources, skilled artisans)

Example: Village B (Weapons Monopoly Strategy)

Village B Assessment:
  Weapons Production: 500 swords/month (5 legendary blacksmiths)
  Weapons Demand: 200 swords/month (local city guard)
  Surplus: 300 swords/month

  Iron Demand: 2000 kg/month (weapons production input)
  Iron Production: 500 kg/month (1 small mine)
  Deficit: 1500 kg/month

Strategic Decision:
  Focus: Weapons (comparative advantage = legendary blacksmiths)
  Outsource: Iron (cheap import from Village A)

  Calculation:
    Local iron mining cost: 3 currency/kg (poor local veins)
    Imported iron cost: 1.2 currency/kg + 0.5 transport = 1.7 currency/kg

    Savings: 1,500 kg × (3.0 - 1.7) = 1,950 currency/month

  Redirect Savings:
    Do NOT build more mines (uneconomical)
    Invest in 6th legendary blacksmith (expand weapons production)
    Result: +100 swords/month production → +12,000 currency/month export revenue

Long-term Dependency Risk:
  Village B depends on Village A for iron
  If trade route cut (war, embargo, bandits):
    → Village B cannot produce weapons (no iron)
    → Economic collapse (weapons = primary income)

  Mitigation:
    - Stockpile iron (3 months reserve)
    - Maintain backup iron mine (low production, emergency only)
    - Diversify iron sources (import from multiple villages)
    - Diplomatic protection (military alliance with Village A)
```

---

## Business Quality Standards & Ethics

### Refusal to Produce (Quality Thresholds)

**Businesses may refuse to craft items** if input materials don't meet owner's quality expectations, based on behavioral traits.

```
Quality Acceptance Decision:

When business receives materials for production, owner evaluates:
  InputQuality vs MinimumAcceptableQuality

  Decision Factors:
    - Owner behavioral traits (materialistic, warlike, honor)
    - Business reputation (high reputation = higher standards)
    - Current circumstances (war, siege, desperation)
    - Customer identity (elite vs commoner, ally vs enemy)

Owner Behavioral Profiles:

Pure Materialistic (Profit-Focused):
  Strategy: Maximize output, accept any materials
  MinimumAcceptableQuality: 20 (very low, will craft from garbage)
  Pricing: Market rate (honest pricing for quality delivered)

  Example:
    Blacksmith receives iron ingots (Quality 30, poor)
    Decision: Accept (profit > reputation)
    Crafts poor longsword (Quality 35, Common)
    Sells at 40 currency (discounted from 80, reflects poor quality)
    Customer aware of quality (honest transaction)

  Reputation Impact: Neutral (customers know what they're getting)

Corrupt Materialistic (Exploitative):
  Strategy: Accept low materials, overcharge customers
  MinimumAcceptableQuality: 15 (extremely low, scrap metal)
  Pricing: Full price (deceptive, claims high quality)

  Example:
    Blacksmith receives iron ingots (Quality 25, trash)
    Decision: Accept (easy profit)
    Crafts terrible longsword (Quality 28, Common, breaks after 50 uses)
    Sells at 80 currency (full price, lies about quality)
    Customer deceived (thinks buying good sword)

  Reputation Impact: Severe loss (-20) when sword breaks
  Criminal Risk: Fraud charges if customer complains to peacekeepers

Pure Warlike/Honor-Focused (Reputation-Obsessed):
  Strategy: Only accept high-quality materials, maintain reputation
  MinimumAcceptableQuality: 70 (high standards)
  Pricing: Premium (charges extra for guaranteed quality)

  Example:
    Blacksmith receives iron ingots (Quality 60, mediocre)
    Decision: REFUSE (below standards, would harm reputation)
    Response: "I will not craft subpar weapons. Bring me quality materials or find another smith."
    Customer must source better materials (Quality 70+) or go elsewhere

  Reputation Impact: Positive (+5) for refusing bad work (integrity)
  Exception: Dire circumstances (siege, village survival)

Pure Warlike During Siege (Survival Mode):
  Strategy: Accept lower quality, prioritize volume for defense
  MinimumAcceptableQuality: 50 (lowered from 70)
  Pricing: Cost-based (not profit-focused during crisis)

  Example:
    Village under siege, desperate for weapons
    Blacksmith receives iron ingots (Quality 55, acceptable in crisis)
    Decision: Accept (village survival > personal reputation)
    Crafts adequate longsword (Quality 60, Common)
    Sells at 60 currency (cost + minimal markup)
    Explanation: "This is not my finest work, but it will defend our walls."

  Reputation Impact: Minor loss (-2) offset by patriotic service (+5)
  Net: +3 reputation (community appreciates sacrifice of standards for survival)

Good/Charitable (Community-Focused):
  Strategy: Accept varied quality, adjust pricing fairly
  MinimumAcceptableQuality: 40 (moderate, usable quality)
  Pricing: Fair value (price matches quality honestly)

  Example:
    Blacksmith receives iron ingots (Quality 50, average)
    Decision: Accept (helps customer, honest transaction)
    Crafts average longsword (Quality 55, Common)
    Sells at 50 currency (fair price for average quality)
    Explanation: "This sword will serve you well for everyday use."

  Reputation Impact: Positive (+2) for honesty and fairness

Lawful/Perfectionist (Standards-Obsessed):
  Strategy: Only accept materials meeting strict specifications
  MinimumAcceptableQuality: 80 (very high, near-perfect)
  Pricing: Premium (justified by consistent excellence)

  Example:
    Enchanter receives magical crystals (Quality 75, good but not excellent)
    Decision: REFUSE (below specifications)
    Response: "These crystals have minor impurities. Return with flawless crystals."
    Customer must find Quality 80+ crystals or commission elsewhere

  Reputation Impact: Positive (+3) for uncompromising standards
  Business Impact: Fewer customers (high barrier), but loyal elite clientele

Chaotic/Opportunistic (Situational):
  Strategy: Accept materials based on mood, opportunity, circumstances
  MinimumAcceptableQuality: 30-70 (varies wildly)
  Pricing: Variable (negotiable, whimsical)

  Example Day 1:
    Blacksmith receives iron ingots (Quality 45)
    Decision: Accept (in good mood, needs currency)
    Crafts sword, sells at 55 currency

  Example Day 2:
    Blacksmith receives iron ingots (Quality 60)
    Decision: REFUSE (hangover, doesn't feel like working)
    Response: "Come back tomorrow."

  Reputation Impact: Unpredictable (-5 to +5 randomly)
```

### Business Refusal Consequences

**Refusing materials** creates supply chain disruptions and customer dissatisfaction.

```
Customer Impact:

If business refuses materials:
  Customer must:
    Option 1: Find higher quality materials (expensive, time-consuming)
    Option 2: Find different business with lower standards
    Option 3: Import finished product from other village (transportation cost)
    Option 4: Go without (unacceptable if urgent need)

Village Impact:

If multiple businesses refuse (high standards across village):
  → Material surplus (nobody accepting low-quality inputs)
  → Production shortage (fewer finished goods)
  → Price spike (scarcity of finished products)
  → Economic pressure: Lower standards OR import finished goods

Example: Village A (High Standards Culture)
  All blacksmiths refuse Quality <70 iron
  Miners produce iron (average Quality 55)
  Result: Iron stockpiles grow, but NO SWORDS produced

  Resolution:
    Option 1: Train miners to improve quality (years)
    Option 2: Blacksmiths lower standards (cultural shift)
    Option 3: Import swords from Village B (dependency)
    Option 4: God intervention (player improves mine quality via blessing)

Alignment-Based Standards Culture:

Lawful Good Village:
  High standards enforced by Guild regulations
  Businesses that sell subpar goods face Guild sanctions
  Quality control ensures customer protection
  Result: Expensive but reliable products

Chaotic Neutral Village:
  No standards enforcement, buyer beware
  Wide quality variance (trash to legendary)
  Customers must inspect carefully
  Result: Cheap options available but risky

Evil/Corrupt Village:
  Businesses exploit customers with deceptive pricing
  Peacekeepers ignore fraud complaints (bribeable)
  Quality rarely matches advertised
  Result: Reputation-based shopping (know who to trust)
```

---

## Business Sanctions & Illegal Trade

### Sanctionable Goods & Magic

**Villages and entities can sanction businesses** for producing/trading illegal weapons, magic, or contraband.

```
Illegal Goods Categories:

1. Forbidden Weapons (Village-Specific)
  - Poison weapons (coated blades, toxic arrows)
  - Plague weapons (disease-spreading projectiles)
  - Torture implements (execution/interrogation tools)
  - Explosive devices (grenades, bombs, mines)

  Legality varies by alignment:
    Lawful Good: ALL forbidden (inhumane warfare)
    Neutral: Poison/plague forbidden, explosives regulated
    Lawful Evil: Legal for state use, illegal for civilians
    Chaotic Evil: Legal (anything goes)

2. Forbidden Magic (Alignment-Based)
  - Necromancy (raising undead, soul manipulation)
  - Demonology (demon summoning, pacts)
  - Blood magic (sacrifice rituals, life drain)
  - Fel-flame (corrupting fire magic)
  - Mind control magic (domination, enslavement)
  - Plague magic (disease creation)

  Legality varies by alignment:
    Good Villages: All dark magic forbidden
    Lawful Neutral: Necromancy/demonology forbidden (dangerous)
    Chaotic Neutral: Legal (personal freedom)
    Evil Villages: Legal, often state-sponsored

3. Forbidden Materials
  - Cursed artifacts (demonic relics)
  - Stolen goods (recognized by original owners)
  - Sacred items (temple treasures, holy relics)
  - Contraband drugs (narcotics, hallucinogens)

Sanction Types:

Level 1: Warning (First Offense)
  Penalty: Official warning, business placed on watchlist
  Duration: 1 year probation
  Impact: -10 reputation, +50% peacekeeper inspections

Level 2: Fine (Second Offense)
  Penalty: 1,000-10,000 currency fine
  Duration: 2 years probation
  Impact: -20 reputation, business license suspended 3 months

Level 3: Asset Seizure (Third Offense)
  Penalty: Illegal goods confiscated, inventory searched
  Duration: 5 years probation
  Impact: -30 reputation, 50% inventory seized

Level 4: Business Closure (Repeated Violations)
  Penalty: Business license revoked, building seized
  Duration: Permanent (owner banned from operating in village)
  Impact: -50 reputation, criminal record

Level 5: Criminal Charges (Severe Violations)
  Penalty: Owner arrested, trial, imprisonment/exile/execution
  Duration: Depends on sentence (10 years imprisonment or exile)
  Impact: -100 reputation, business dissolved, assets liquidated

Example: Blacksmith Crafting Poison Weapons

Village: Lawful Good (poison weapons illegal)

Offense 1 (Year 1):
  Blacksmith Darius crafts 5 poison daggers for underground buyer
  Peacekeeper inspection discovers hidden poison daggers
  Penalty: Warning + 500 currency fine
  Darius: "I didn't know they were illegal!" (lie, but plausible)
  Result: Probation, watchlist

Offense 2 (Year 3):
  Darius crafts 10 plague arrows for mercenary band
  Whistleblower reports Darius to peacekeepers
  Peacekeepers raid forge, find plague arrows
  Penalty: 5,000 currency fine + 3-month license suspension
  Darius: Cannot operate, loses 3 months revenue
  Result: Heavy financial loss, reputation damaged

Offense 3 (Year 5):
  Darius crafts 20 torture implements for authoritarian neighboring village
  Peacekeepers intercept shipment at border
  Penalty: Asset seizure (50% inventory confiscated), license suspended 1 year
  Darius: Business nearly bankrupt
  Result: Severe financial crisis

Offense 4 (Year 7):
  Darius crafts necromantic weapons (soul-trapping blades) for dark cult
  Peacekeepers raid, find evidence of demon pact (Darius corrupted)
  Penalty: Criminal charges (treason, dark magic), arrest, trial
  Verdict: Guilty, exiled from village
  Result: Business dissolved, Darius exiled, flees to evil village

Sanction Issuer Variations:

Village Council:
  Authority: Village-wide sanctions
  Criteria: Alignment-based laws, majority vote
  Enforcement: Peacekeepers
  Example: Lawful Good village bans necromancy

Guild (Artisans' Guild):
  Authority: Guild member businesses only
  Criteria: Guild quality/ethics standards
  Enforcement: Guild enforcers, revoke membership
  Example: Blacksmiths' Guild bans shoddy weapons (min Quality 50)

Church/Temple:
  Authority: Religious businesses, faithful citizens
  Criteria: Religious law (holy texts, divine command)
  Enforcement: Temple guards, excommunication
  Example: Temple of Light bans blood magic (unholy)

Warlord/Military:
  Authority: Military suppliers, contractors
  Criteria: Military needs (weapon effectiveness, reliability)
  Enforcement: Military police, contract termination
  Example: Army refuses explosive devices (unreliable, dangerous)

Player (God):
  Authority: Divine decree, followers obey
  Criteria: Player's moral choices
  Enforcement: Divine punishment (curses, smite), follower obedience
  Example: God forbids slavery, followers refuse slave trade

Alignment-Based Sanction Severity:

Lawful Good:
  Enforcement: Strict, consistent
  Penalties: Fair trials, imprisonment/exile (not execution)
  Corruption: Low (peacekeepers honest)
  Example: Due process, evidence-based sanctions

Lawful Evil:
  Enforcement: Brutal, efficient
  Penalties: Harsh (execution, torture, forced labor)
  Corruption: Moderate (officials extractable via bribes)
  Example: Summary execution for dark magic

Chaotic Good:
  Enforcement: Inconsistent, emotion-driven
  Penalties: Lenient (warnings, fines, rarely imprisonment)
  Corruption: Low (but disorganized)
  Example: Mob justice, vigilantes punish offenders

Chaotic Evil:
  Enforcement: Arbitrary, whim-based
  Penalties: Extreme (torture, enslavement, public execution)
  Corruption: Total (officials demand bribes for everything)
  Example: Pay bribe or die, no fair trials
```

---

## Smuggling & Black Markets

**Smugglers thrive on sanctions** - every prohibition creates a lucrative opportunity for those willing to take risks. The more severe the sanctions, the higher the profit margins and the greater the incentive to smuggle.

### Contraband Types

**Illegal goods** vary by village alignment and laws.

```
Contraband by Village Type:

Lawful Good Village:
  Illegal: Weapons (strict control), narcotics, stolen goods, slaves, dark magic items
  Penalty: Confiscation, criminal trial, exile for repeat offenders
  Enforcement: High (70% patrol coverage, dedicated peacekeepers)

Authoritarian Village:
  Illegal: Unapproved imports (protectionism), foreign literature, weapons (state monopoly), anti-state magic
  Penalty: Confiscation, state labor, execution for political contraband
  Enforcement: Very High (90% patrol coverage, secret police informants)

Egalitarian Village:
  Illegal: Slaves (morally forbidden), monopoly goods (anticompetitive), luxury items (inequality promotion)
  Penalty: Confiscation, community service, wealth redistribution fine
  Enforcement: Moderate (50% patrol coverage, community watches)

Corrupt/Evil Village:
  Illegal: (Almost nothing) Goods that don't pay bribes to officials
  Penalty: Bribes (10% goods value), confiscation if refuse
  Enforcement: Low (30% patrol coverage, bribes bypass system)

Chaotic Evil Village:
  Illegal: (Nothing) All trade permitted, might makes right
  Penalty: None (buyer beware)
  Enforcement: None (no peacekeepers)
```

### Sanctions Create Smuggling Opportunities

**Sanctions transform legal goods into contraband**, creating massive profit margins for smugglers.

```
Pre-Sanction Scenario:
  Village A freely trades iron weapons with Village B
  Market Price: 80 currency/longsword
  Margin: 20 currency profit per sword (25%)

Post-Sanction Scenario (Village B bans all Village A imports):
  Village B blacksmiths desperate for iron ingots (Village A primary supplier)
  Legal alternatives: Distant Village C (200 km away, +50% transport costs)

  Black Market Price: 120-200 currency/longsword (1.5-2.5× markup)
  Smuggler Profit: 40-120 currency per sword (50-150% margin)

  Demand Explosion: Sanctioned goods become status symbols + scarce resources
  Result: Smuggling operations boom

Example - Poison Weapon Sanctions:

Village A (Lawful Good) bans poison weapons (Level 5 sanction - criminal charges)

Pre-Sanction:
  Poison daggers: Legal, 150 currency
  Market: Limited demand (assassins, hunters)
  Production: 5-10 per month

Post-Sanction:
  Poison daggers: Illegal, black market 450-600 currency (3-4× markup)
  Market: Explosive demand (criminals, rebels, desperate defenders)
  Production: 30-50 per month (underground smiths, smuggled imports)

  Smuggler Economics:
    Buy from Village C (no sanction): 150 currency
    Transport cost (secret route): 30 currency
    Bribe peacekeepers: 50 currency (10% chance)
    Sell in Village A (black market): 500 currency

    Net Profit: 500 - 150 - 30 - 5 (expected bribe cost) = 315 currency per dagger
    Return on Investment: 210% profit per item

    Risk: 25% chance caught per transaction = criminal charges, exile
    Expected Value: 315 × 0.75 = 236 currency (still 3× legal trade margins)
```

### Skill-Based Detection Mechanics

**Smuggling success** depends on smuggler skills versus peacekeeper skills.

```
Smuggler Skills:
  - Stealth (1-100): Ability to move undetected through checkpoints
  - Deception (1-100): Ability to lie convincingly about cargo
  - Bribery (1-100): Ability to bribe peacekeepers effectively
  - Routes Knowledge (1-100): Knowledge of secret paths and safe houses
  - Criminal Contacts (1-100): Network of informants, corrupt officials

Peacekeeper Skills:
  - Investigation (1-100): Ability to detect hidden cargo
  - Perception (1-100): Ability to spot suspicious behavior
  - Interrogation (1-100): Ability to break smuggler cover stories
  - Intelligence Network (1-100): Informants providing tips on smuggling operations
  - Incorruptibility (1-100): Resistance to bribes (reduces Bribery effectiveness)

Detection Roll Formula:

BaseDetectionChance = 40% (all smuggling operations start at 40% detection base)

SmugglerModifiers:
  Stealth Bonus: -(SmugglerStealth / 2) %
  Deception Bonus: -(SmugglerDeception / 3) %
  Routes Knowledge Bonus: -(RoutesKnowledge / 4) %

PeacekeeperModifiers:
  Investigation Bonus: +(PeacekeeperInvestigation / 2) %
  Perception Bonus: +(PeacekeeperPerception / 3) %
  Intelligence Network Bonus: +(IntelligenceNetwork / 4) %

BriberyAttempt (Optional):
  If (SmugglerBribery > PeacekeeperIncorruptibility):
    Detection Chance: × 0.5 (bribe successful, peacekeeper looks other way)
  Else:
    Detection Chance: × 1.5 (bribe rejected, peacekeeper suspicious)

  Bribe Cost: 50-500 currency (depends on cargo value)

FinalDetectionChance = BaseDetectionChance + PeacekeeperModifiers - SmugglerModifiers

Example 1: Expert Smuggler vs. Average Peacekeepers

Smuggler Stats:
  Stealth: 80 (very skilled)
  Deception: 70 (convincing liar)
  Routes Knowledge: 90 (knows all secret paths)
  Bribery: 60

Peacekeeper Stats:
  Investigation: 50 (average)
  Perception: 40 (below average)
  Intelligence Network: 30 (limited informants)
  Incorruptibility: 70 (somewhat resistant to bribes)

Detection Calculation:
  Base: 40%
  - Stealth: -(80 / 2) = -40%
  - Deception: -(70 / 3) = -23%
  - Routes: -(90 / 4) = -22%
  + Investigation: +(50 / 2) = +25%
  + Perception: +(40 / 3) = +13%
  + Intelligence: +(30 / 4) = +7%

  Subtotal: 40% - 85% + 45% = 0%

  Final Detection Chance: 0% (expert smuggler easily evades average peacekeepers)

  Result: Smuggling operation succeeds
  Smuggler Profit: 315 currency (no bribe needed)

Example 2: Novice Smuggler vs. Elite Peacekeepers

Smuggler Stats:
  Stealth: 30 (amateur)
  Deception: 40 (nervous liar)
  Routes Knowledge: 20 (limited knowledge)
  Bribery: 50

Peacekeeper Stats:
  Investigation: 90 (elite investigators)
  Perception: 80 (sharp observers)
  Intelligence Network: 70 (extensive informant network)
  Incorruptibility: 85 (very resistant to bribes)

Detection Calculation:
  Base: 40%
  - Stealth: -(30 / 2) = -15%
  - Deception: -(40 / 3) = -13%
  - Routes: -(20 / 4) = -5%
  + Investigation: +(90 / 2) = +45%
  + Perception: +(80 / 3) = +27%
  + Intelligence: +(70 / 4) = +18%

  Subtotal: 40% - 33% + 90% = 97%

  Bribery Attempt:
    SmugglerBribery (50) < PeacekeeperIncorruptibility (85)
    Detection Chance: 97% × 1.5 = 145% (capped at 100%)

  Final Detection Chance: 100% (novice smuggler caught immediately)

  Result: Smuggling operation discovered
  Penalty: Level 3 sanction (asset seizure) + criminal charges
  Loss: 150 + 30 + 50 (bribe attempt) = 230 currency + criminal record

Example 3: Skilled Smuggler vs. Corrupt Peacekeepers

Smuggler Stats:
  Stealth: 60
  Deception: 55
  Routes Knowledge: 50
  Bribery: 80 (master briber)

Peacekeeper Stats (Corrupt Village):
  Investigation: 40
  Perception: 35
  Intelligence Network: 20 (informants unreliable in corrupt system)
  Incorruptibility: 30 (very corruptible)

Detection Calculation:
  Base: 40%
  - Stealth: -(60 / 2) = -30%
  - Deception: -(55 / 3) = -18%
  - Routes: -(50 / 4) = -12%
  + Investigation: +(40 / 2) = +20%
  + Perception: +(35 / 3) = +12%
  + Intelligence: +(20 / 4) = +5%

  Subtotal: 40% - 60% + 37% = 17%

  Bribery Attempt:
    SmugglerBribery (80) > PeacekeeperIncorruptibility (30)
    Detection Chance: 17% × 0.5 = 8.5%
    Bribe Cost: 100 currency (high bribe, ensures cooperation)

  Final Detection Chance: 8.5% (very low, peacekeepers actively assist smuggler)

  Result: Smuggling operation succeeds with peacekeeper cooperation
  Smuggler Profit: 315 - 100 (bribe) = 215 currency

  Long-term: Smuggler develops ongoing relationship with corrupt peacekeepers
    Future bribes: 50 currency (established relationship)
    Detection chance: Reduced to 5% (peacekeepers tip off smuggler about raids)
```

### Behavioral Variations in Smuggling

**Village and individual behaviors** dramatically affect smuggling success rates.

```
Lawful Good Village (High Enforcement):
  Base Detection Chance: 40%
  Enforcement Bonus: +20% (dedicated anti-smuggling peacekeepers)
  Incorruptibility: 80+ (difficult to bribe)

  Smuggling Difficulty: Very Hard
  Expected Profit: High (scarcity drives prices up)

  Smuggler Strategy:
    Focus on stealth and routes knowledge (bribery ineffective)
    Use secret paths, false cargo manifests
    Avoid checkpoints entirely

Authoritarian Village (Total Control):
  Base Detection Chance: 40%
  Enforcement Bonus: +30% (secret police, informants everywhere)
  Incorruptibility: 90+ (bribery = execution)

  Smuggling Difficulty: Extremely Hard
  Expected Profit: Extreme (total prohibition = desperate demand)

  Smuggler Strategy:
    Only elite smugglers operate (Stealth 80+, Routes 80+)
    Use underground tunnels, hidden compartments
    Single-item smuggling (minimize risk per trip)
    Charge 5-10× markups (compensate for extreme risk)

Corrupt Evil Village (Bribery Heaven):
  Base Detection Chance: 40%
  Enforcement Penalty: -20% (peacekeepers don't care)
  Incorruptibility: 20-40 (everyone has a price)

  Smuggling Difficulty: Easy
  Expected Profit: Moderate (low risk = lower markups)

  Smuggler Strategy:
    Focus on bribery and contacts (stealth unnecessary)
    Establish ongoing relationships with peacekeepers
    Pay protection fees to criminal guilds
    Operate openly (everyone knows, nobody cares)

Chaotic Evil Village (Lawless):
  Base Detection Chance: 0%
  Enforcement: None (no peacekeepers)
  Incorruptibility: N/A (no law enforcement)

  Smuggling Difficulty: None (everything is legal)
  Expected Profit: None (no smuggling needed)

  Trade Strategy:
    Open markets for all goods (even forbidden weapons, dark magic items)
    Smugglers relocate to lawful villages (higher profits)
    Village becomes smuggling hub (import/export contraband)

Individual Peacekeeper Behaviors:

Pure Lawful Peacekeeper:
  Incorruptibility: 95+ (refuses all bribes)
  Investigation: 70-90 (thorough, systematic)
  Behavioral Trait: Will pursue smugglers relentlessly, even off-duty
  Smuggler Response: Avoid at all costs, wait for different shift

Corrupt Peacekeeper:
  Incorruptibility: 20-40 (accepts bribes readily)
  Investigation: 30-50 (lazy, overlooks obvious contraband)
  Behavioral Trait: Actively seeks bribes, may extort smugglers
  Smuggler Response: Preferred target, establish long-term bribe relationship

Chaotic/Opportunistic Peacekeeper:
  Incorruptibility: 50-70 (situational, depends on offer)
  Investigation: 40-60 (competent but unmotivated)
  Behavioral Trait: May accept large bribes, but unpredictable
  Smuggler Response: High-risk bribery (may be arrested despite bribe)

Warlike/Honor-Focused Peacekeeper:
  Incorruptibility: 80-90 (honor code prevents bribery)
  Investigation: 60-80 (focused on dangerous contraband only)
  Behavioral Trait: Ignores petty smuggling, targets weapon/magic smugglers
  Smuggler Response: Can smuggle low-tier contraband safely, avoid weapons/magic
```

### Smuggling Risk-Reward Matrix

```
Contraband Type vs. Village Enforcement:

Low-Risk Contraband (Food, Common Goods):
  Lawful Good Village: Detection 20%, Profit Margin 50%
  Authoritarian Village: Detection 60%, Profit Margin 150%
  Corrupt Village: Detection 5%, Profit Margin 30%

Medium-Risk Contraband (Weapons, Luxury Items):
  Lawful Good Village: Detection 40%, Profit Margin 150%
  Authoritarian Village: Detection 80%, Profit Margin 300%
  Corrupt Village: Detection 10%, Profit Margin 70%

High-Risk Contraband (Dark Magic, Forbidden Weapons):
  Lawful Good Village: Detection 70%, Profit Margin 400%
  Authoritarian Village: Detection 95%, Profit Margin 800%
  Corrupt Village: Detection 30%, Profit Margin 200%

Smuggler Decision Matrix:

Risk-Averse Smuggler (Behaviors: Lawful/Neutral):
  Strategy: Low-risk contraband in corrupt villages
  Average Profit: 100-200 currency/month
  Arrest Risk: <5% annually
  Career Length: Decades (sustainable)

Risk-Seeking Smuggler (Behaviors: Chaotic):
  Strategy: High-risk contraband in authoritarian villages
  Average Profit: 1000-5000 currency/month
  Arrest Risk: 50% annually
  Career Length: 1-3 years (burnout, arrest, or wealth)

Expert Smuggler (High Skills):
  Strategy: High-risk contraband with elite skills (Stealth 85+)
  Average Profit: 2000-8000 currency/month
  Arrest Risk: 15% annually (skills mitigate risk)
  Career Length: 5-10 years (retires wealthy or caught eventually)
```

---

## Civil Unrest & Protest Mechanics

**Individuals and aggregate entities** (guilds, factions, neighborhoods) may revolt or protest for or against sanctions, military activities, or unpopular policies. Peacekeeper responses vary dramatically by alignment and behavioral traits.

### Protest Triggers

```
Sanction-Related Protests:

FOR Sanctions (Lawful Good, Religious, Nationalist populations):
  Trigger: Enemy village producing illegal weapons/magic
  Protest Demand: "Ban all trade with evil Village A!"
  Example: Church faction protests against Village A's necromancy trade
    Protestors: 50 religious villagers (20% of population)
    Intensity: Peaceful march, prayer vigils
    Duration: Until sanctions implemented or Church influence declines

AGAINST Sanctions (Merchants, Materialists, Affected workers):
  Trigger: Village implements sanctions that hurt local economy
  Protest Demand: "Lift the embargo! We need trade to survive!"
  Example: Merchant guild protests against iron weapon sanctions
    Protestors: 30 merchants + 100 unemployed blacksmiths (52% of workers)
    Intensity: Market strikes, blockade of council building
    Duration: Until sanctions lifted or businesses adapt/relocate

Military-Related Protests:

AGAINST War (Pacifists, Egalitarians, War-weary populations):
  Trigger: Village declares war on neighboring village
  Protest Demand: "End the war! Our sons die for nothing!"
  Example: Mothers of fallen soldiers protest ongoing siege
    Protestors: 80 villagers (families of casualties)
    Intensity: Vigils, refusal to pay war taxes
    Duration: Until war ends or casualties mount to rebellion

FOR War (Warlike, Nationalist, Aggressive populations):
  Trigger: Village refuses to retaliate after insult/attack
  Protest Demand: "Fight back! Avenge our honor!"
  Example: Warrior guild demands retaliation after border raid
    Protestors: 40 warriors + 60 nationalist civilians
    Intensity: Armed demonstrations, threats against council
    Duration: Until war declared or warriors form raiding parties

Policy-Related Protests:

AGAINST Taxes (Merchants, Lower classes, Overtaxed populations):
  Trigger: Village increases taxes to fund war/infrastructure
  Protest Demand: "Lower taxes! We cannot afford to live!"
  Example: Farmers protest 40% grain tax (up from 20%)
    Protestors: 120 farmers (60% of agricultural workforce)
    Intensity: Tax strikes, hiding grain from collectors
    Duration: Until taxes lowered or starvation forces compliance

AGAINST Class Inequality (Egalitarians, Lower classes):
  Trigger: Wealthy elite accumulate resources while poor starve
  Protest Demand: "Share the wealth! Feed the people!"
  Example: Urban workers demand grain redistribution during famine
    Protestors: 200 villagers (80% of lower class)
    Intensity: Food riots, storehouse raids
    Duration: Until wealth redistributed or violently suppressed
```

### Protest Escalation Levels

**Protests escalate** based on government response, protest duration, and external pressures.

```
Level 1: Peaceful Protest (Non-Violent)
  Size: 10-50 villagers (5-20% population)
  Actions: Marches, petitions, public speeches, vigils
  Economic Impact: Minimal (protestors still work)
  Violence: None
  Peacekeeper Response: Monitor, protect protestors' rights (Lawful Good)

  Example: Church faction peacefully protests necromancy trade
    Day 1: 30 clergy march to village square, deliver petition
    Day 2: 50 villagers join prayer vigil outside council building
    Day 3: Village council meets to discuss sanctions
    Outcome: Council implements Level 1 warning on necromancy items (protest succeeds)

Level 2: Civil Disobedience (Non-Violent Resistance)
  Size: 50-150 villagers (20-60% population)
  Actions: Strikes, boycotts, tax refusal, sit-ins
  Economic Impact: Moderate (businesses closed, production halted)
  Violence: None (but protestors block roads, occupy buildings)
  Peacekeeper Response: Warnings, arrests for obstruction (Lawful)

  Example: Merchant guild strikes against iron weapon sanctions
    Week 1: 30 merchants close shops, refuse to sell goods
    Week 2: 100 blacksmiths join strike (no weapons produced)
    Week 3: Village economy grinds to halt, council pressured
    Outcome: Council partially lifts sanctions (allows iron ingots, bans finished weapons)

Level 3: Riots (Violent Property Damage)
  Size: 100-300 villagers (40-100% population)
  Actions: Looting, arson, destruction of property, clashes with peacekeepers
  Economic Impact: Severe (businesses destroyed, infrastructure damaged)
  Violence: Property damage, some injuries
  Peacekeeper Response: Non-lethal crowd control (tear gas, arrests) or lethal force (depends on alignment)

  Example: Food riots during famine (village under siege)
    Day 1: 150 starving villagers storm storehouse, seize grain
    Day 2: Riots spread, 200 villagers loot merchants' homes
    Day 3: Peacekeepers intervene (response depends on alignment)
      Lawful Good: Non-lethal dispersal, emergency grain distribution
      Authoritarian: Lethal force, 30 villagers killed, martial law declared
    Outcome: Village council forced to redistribute food reserves or face rebellion

Level 4: Armed Rebellion (Organized Violence)
  Size: 200-500 villagers (80%+ population or organized faction)
  Actions: Armed insurgency, assassination attempts, guerrilla warfare
  Economic Impact: Catastrophic (production ceases, infrastructure destroyed)
  Violence: Casualties on both sides, targeted killings
  Peacekeeper Response: Military crackdown (lethal force, mass arrests, executions)

  Example: Rebellion against oppressive sanctions and war taxes
    Month 1: 300 villagers form rebel militia, raid armory
    Month 2: Rebels control 40% of village, establish shadow government
    Month 3: Civil war erupts
      Victory Conditions:
        Rebel Victory: Council overthrown, new government installed
        Government Victory: Rebels defeated, survivors exiled/executed
    Outcome: Civil war lasts 6-12 months, 20-40% casualties

Level 5: Civil War (Complete Breakdown)
  Size: Entire village divided into factions (50/50 split)
  Actions: Full-scale warfare, ethnic/class cleansing, scorched earth
  Economic Impact: Total collapse (village may not survive)
  Violence: Mass casualties, war crimes, atrocities
  Peacekeeper Response: N/A (peacekeepers join factions or flee)

  Example: Civil war over sanctions and alignment shift
    Year 1: Village A (Lawful Good) under pressure from Evil neighbor
      Warlike faction: "We must fight! Allow dark magic weapons!"
      Pacifist faction: "We must maintain our values! No dark magic!"
    Year 2: Village splits into two factions (150 vs 150)
      Warlike faction: Controls armory, recruits warriors
      Pacifist faction: Controls temple, recruits clergy
    Year 3: Full civil war
      Warlike faction: Uses forbidden weapons, blood magic
      Pacifist faction: Remains Lawful Good, refuses to corrupt
    Outcome:
      Warlike Victory: Village becomes Neutral/Evil, sanctions lifted, dark magic permitted
      Pacifist Victory: Village remains Lawful Good, sanctions maintained, warlike faction exiled
      Stalemate: Village splits into two separate settlements
```

### Peacekeeper Response by Alignment

**Peacekeeper responses** to protests vary dramatically by village alignment and peacekeeper behavioral traits.

```
Lawful Good Village (Rights-Respecting):
  Level 1 Response: Protect protestors' rights
    Actions: Police escort for marches, facilitate dialogue
    Violence: None (peacekeepers ensure safety)
    Public Opinion: +10 reputation (respect for law and order)

  Level 2 Response: Negotiate and enforce law
    Actions: Arrest obstruction offenders, negotiate with leaders
    Violence: Minimal (arrests, no beatings)
    Conditions: Offer compromises (partial sanction lift, tax relief)
    Public Opinion: Neutral (balanced enforcement)

  Level 3 Response: Non-lethal crowd control
    Actions: Tear gas, water cannons, mass arrests
    Violence: Non-lethal force only (no firearms)
    Conditions: Declare state of emergency, martial law
    Public Opinion: -20 reputation (harsh but lawful)

  Level 4+ Response: Military intervention (reluctant)
    Actions: Deploy militia, blockade rebel zones
    Violence: Lethal force only if fired upon (defensive)
    Conditions: Offer amnesty for surrender
    Public Opinion: -40 reputation (civil war = failure of leadership)

Authoritarian Village (Order Above All):
  Level 1 Response: Suppress immediately
    Actions: Disperse protests, arrest organizers
    Violence: Beatings, intimidation
    Public Opinion: -5 reputation (oppressive but expected)

  Level 2 Response: Crackdown
    Actions: Mass arrests, secret police investigations
    Violence: Torture for information, disappearances
    Conditions: No negotiations (demands ignored)
    Public Opinion: -20 reputation (fear maintains order)

  Level 3 Response: Lethal force
    Actions: Shoot looters, public executions
    Violence: Firearms authorized, kill on sight
    Conditions: Martial law, curfews, collective punishment
    Public Opinion: -50 reputation (terror tactics)

  Level 4+ Response: Total war
    Actions: Treat rebels as enemy combatants, scorched earth
    Violence: Artillery on rebel neighborhoods, mass executions
    Conditions: No amnesty, extermination of rebel faction
    Public Opinion: -100 reputation (atrocity, potential player intervention)

Egalitarian Village (Community-Focused):
  Level 1 Response: Facilitate dialogue
    Actions: Host town halls, address grievances
    Violence: None (peacekeepers are community members)
    Public Opinion: +15 reputation (responsive government)

  Level 2 Response: Community mediation
    Actions: Form committees, negotiate solutions
    Violence: None (social pressure, not force)
    Conditions: Implement reforms (redistribute resources, lift sanctions if community agrees)
    Public Opinion: +5 reputation (democratic process)

  Level 3 Response: Community self-policing
    Actions: Community defense forces, intervention of elders
    Violence: Minimal (community restrains rioters)
    Conditions: Emergency councils, rapid reforms
    Public Opinion: -10 reputation (disorder = leadership failure)

  Level 4+ Response: Factional split (village divides)
    Actions: No unified response (peacekeepers join factions)
    Violence: Depends on faction alignment
    Conditions: Village may peacefully split into two communities
    Public Opinion: N/A (new governments formed)

Corrupt Evil Village (Pay to Play):
  Level 1 Response: Extort protestors
    Actions: Demand bribes to allow protests
    Violence: Beatings for non-payers
    Public Opinion: -10 reputation (expected corruption)

  Level 2 Response: Hired thugs
    Actions: Criminal gangs disperse protests (for a fee)
    Violence: Brutal beatings, property destruction
    Conditions: Protestors pay protection money or suffer
    Public Opinion: -30 reputation (lawless violence)

  Level 3 Response: Let it burn
    Actions: Peacekeepers evacuate elites, let riots rage
    Violence: Mob rule, no law enforcement
    Conditions: Rioters plunder lower classes, elites safe
    Public Opinion: -60 reputation (total abandonment)

  Level 4+ Response: Warlord takeover
    Actions: Strongest faction seizes power
    Violence: Winner-take-all warfare
    Conditions: New tyrant emerges from chaos
    Public Opinion: -80 reputation (failed state)

Chaotic Evil Village (Might Makes Right):
  Level 1-4 Response: No peacekeepers (no law enforcement)
    Actions: Strongest faction wins
    Violence: Constant (no restraint on violence)
    Conditions: Protests = gang wars = normal Tuesday
    Public Opinion: N/A (no government to have opinion on)
```

### Sanction-Specific Protest Scenarios

**Detailed examples** of how sanctions trigger civil unrest.

```
Scenario 1: Weapon Sanctions During War

Village A (Lawful Good) at war with Village B (Neutral Evil)
Village A council bans poison weapons (Level 5 sanction - criminal charges)

Month 1: Warlike faction protests
  Protestors: 40 warriors + 60 civilians (33% population)
  Demand: "Lift the ban! We need every advantage to survive!"
  Actions: Armed demonstrations outside council building
  Threat Level: Level 2 (civil disobedience)

Month 2: Council refuses, maintains sanction
  Protestors: Escalate to Level 3 (riots)
  Actions: Raid armory, steal poison daggers from confiscated stores
  Peacekeepers: Arrest 10 warriors (non-lethal force)
  Casualties: 5 injured peacekeepers, 15 injured rioters

Month 3: War turns desperate (Village B besieges Village A)
  Protestors: Escalate to Level 4 (armed rebellion)
  Actions: 50 warriors form rebel militia, threaten council
  Ultimatum: "Lift the ban or we will overthrow you and install a wartime council"
  Council Decision:
    Option A: Lift sanctions (Alignment shift to Neutral, warlike faction wins)
    Option B: Maintain sanctions (Civil war erupts, 30% casualties)
    Option C: Player intervenes (God descends, settles dispute divinely)

Outcome - Option A Chosen:
  Village A lifts poison weapon ban (warlike faction victory)
  Alignment: Lawful Good → Neutral Good (compromised values for survival)
  Consequence: Village A wins war using poison weapons, but loses moral authority
  Long-term: Egalitarian/Pacifist factions emigrate (20% population loss)

Scenario 2: Trade Sanctions Hurt Economy

Village A implements total embargo on Village B (all trade banned)
Village A blacksmiths lose 60% of iron supply (Village B primary supplier)

Week 1: Merchant faction protests
  Protestors: 30 merchants + 80 blacksmiths (44% of labor force)
  Demand: "Lift the embargo! We cannot survive without iron!"
  Actions: Market strike (no goods sold, production halted)
  Threat Level: Level 2 (civil disobedience)

Week 2: Council refuses (sanctions justified by Village B's aggression)
  Protestors: Escalate, form smuggling network
  Actions: Criminal guild offers to smuggle iron (3× markup)
    Elite blacksmiths: Pay smugglers, maintain production
    Poor blacksmiths: Cannot afford smuggled iron, go bankrupt
  Result: Economic divide widens (wealthy vs poor)

Week 3: Poor blacksmiths escalate to riots
  Protestors: 60 unemployed blacksmiths + 100 lower class (53% of underclass)
  Demand: "Feed us or we will take what we need!"
  Actions: Loot merchants' homes, storehouse raids
  Threat Level: Level 3 (riots)

Week 4: Peacekeeper response (Lawful Good village)
  Actions: Non-lethal dispersal, emergency relief
    Peacekeepers: Arrest 20 rioters, no lethal force
    Council: Implements social welfare (distribute grain reserves)
  Compromise: Partial sanction lift (allow iron ingots, ban finished weapons)
  Outcome: Riots end, economy stabilizes, smugglers lose business

Alternative Outcome (Authoritarian village):
  Actions: Lethal force, mass executions
    Peacekeepers: Shoot 15 rioters, public executions of organizers
    Council: Maintains total embargo, enforces with terror
  Result: Riots suppressed by force, population terrorized
  Long-term: Underground rebellion forms (escalates to civil war within 6 months)

Scenario 3: Religious Sanctions Split Community

Village A (Mixed alignment: 60% Lawful Good, 40% Neutral)
Church faction demands sanctions on necromancy items from Village B

Month 1: Church faction protests FOR sanctions
  Protestors: 80 religious villagers (40% of population)
  Demand: "Ban all necromancy! It is an abomination!"
  Actions: Peaceful vigils, petitions to council
  Threat Level: Level 1 (peaceful protest)

Month 2: Merchant faction counter-protests AGAINST sanctions
  Protestors: 40 merchants (necromancy items = 20% of trade revenue)
  Demand: "Trade is trade! We need the income!"
  Actions: Economic boycott threat (if sanctions pass, merchants strike)
  Threat Level: Level 2 (civil disobedience threat)

Month 3: Council implements sanctions (Church faction wins)
  Sanctions: Level 3 (asset seizure of necromancy items)
  Result: Merchants lose 20% revenue, 10 go bankrupt

Month 4: Merchant faction escalates to riots
  Protestors: 40 merchants + 80 unemployed workers (48% of labor force)
  Actions: Loot church properties, burn temples
  Violence: 5 clergy killed, 20 churches vandalized
  Threat Level: Level 3 (riots)

Month 5: Church faction retaliates (armed militia)
  Actions: 60 armed clergy form militia, defend temples
  Violence: Street battles, 15 casualties (both sides)
  Threat Level: Level 4 (armed rebellion, both factions)

Month 6: Civil war erupts
  Factions:
    Church Militia (80 fighters): Lawful Good, maintains sanctions
    Merchant Militia (120 fighters): Neutral, demands sanction lift
  Duration: 8-month civil war
  Outcome:
    Merchant Victory: Sanctions lifted, church faction exiled (Village A → Neutral)
    Church Victory: Sanctions maintained, merchant faction exiled (Village A → Lawful Good)
    Player Intervention: God settles dispute (force compromise or divine judgment)
```

### Civil Unrest Impact on Economy

**Protests and rebellions** have severe economic consequences.

```
Economic Impact by Protest Level:

Level 1 (Peaceful Protest):
  Production: -10% (protestors take time off work)
  Trade: No impact (routes remain open)
  Tax Revenue: No impact
  Duration: 1-4 weeks

Level 2 (Civil Disobedience):
  Production: -40% (strikes halt key industries)
  Trade: -20% (merchants close shops, embargoes)
  Tax Revenue: -30% (tax refusal)
  Duration: 1-3 months

Level 3 (Riots):
  Production: -70% (businesses destroyed, workers flee)
  Trade: -60% (caravans avoid unsafe routes)
  Tax Revenue: -50% (government focused on crisis)
  Infrastructure Damage: 100-1000 currency (looted buildings, arson)
  Duration: 1-6 weeks

Level 4 (Armed Rebellion):
  Production: -90% (war economy, only military production)
  Trade: -80% (trade routes cut off by rebels)
  Tax Revenue: -70% (rebel territories refuse taxes)
  Infrastructure Damage: 5,000-50,000 currency (warfare destroys buildings)
  Duration: 6-18 months

Level 5 (Civil War):
  Production: -95% (total collapse, subsistence only)
  Trade: -100% (complete isolation)
  Tax Revenue: -90% (government controls shrinking territory)
  Infrastructure Damage: 50,000-500,000 currency (village may not recover)
  Population Loss: 20-50% (casualties, refugees, famine)
  Duration: 1-5 years

Recovery Time:

Level 1-2: 1 month (minimal damage)
Level 3: 6-12 months (rebuild looted businesses)
Level 4: 2-4 years (repair war damage, restore trust)
Level 5: 5-10 years or never (village may collapse entirely)
```

---

## DOTS Implementation Architecture

### Core Components

```csharp
namespace Godgame.Economy.Trade
{
    /// <summary>
    /// Trade route between two villages.
    /// Tracks distance, costs, security, and route quality.
    /// </summary>
    public struct TradeRoute : IComponentData
    {
        public Entity VillageA;                     // Origin village
        public Entity VillageB;                     // Destination village
        public float DistanceKm;                    // Route distance
        public byte SecurityLevel;                  // 0-100 (affects bandit risk)
        public byte RouteQuality;                   // 0-100 (0-20=Uncharted, 100=Major)
        public uint TotalTrips;                     // Lifetime trip count (improves quality)
        public uint LastTripTick;                   // Last caravan/stagecoach trip

        // Cost modifiers
        public float BaseTransportCost;             // Per 100kg per 100km
        public float DiplomaticModifier;            // Allied -0.2, Rival +0.5
        public float TerrainModifier;               // Plains 1.0, Hills 1.3, Mountains 1.8
        public float WarModifier;                   // Wartime +1.0 (danger premium)

        public bool IsActive;                       // False if embargo/closed
    }

    /// <summary>
    /// Merchant caravan entity component.
    /// Moves cargo between villages.
    /// </summary>
    public struct MerchantCaravan : IComponentData
    {
        public enum CaravanType : byte
        {
            Standard,       // 2000kg, moderate speed
            Heavy,          // 5000kg, slow, bulk discount
            Luxury,         // 1000kg, fast, armored, high-value
        }

        public CaravanType Type;
        public ushort CargoCapacity;                // kg
        public byte Guards;                         // Number of guards
        public byte Speed;                          // km/day
        public Entity Owner;                        // Merchant business or guild

        // Current journey
        public Entity OriginVillage;
        public Entity DestinationVillage;
        public float3 CurrentPosition;
        public float JourneyProgress;               // 0.0-1.0
        public uint ArrivalTick;                    // Expected arrival

        public bool IsActive;                       // Currently on route
    }

    /// <summary>
    /// Cargo manifest for caravan/stagecoach.
    /// Tracks goods being transported.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct CargoManifest : IBufferElementData
    {
        public enum CargoType : byte
        {
            Resource,       // Extracted resource (ore, wood)
            Material,       // Produced material (ingots, lumber)
            Product,        // End product (weapons, armor)
        }

        public CargoType Type;
        public Entity CargoEntity;                  // Reference to resource/material/product
        public ushort Quantity;                     // kg or units
        public ushort DeclaredValue;                // Currency (for tariff calculation)
        public Entity Consignee;                    // Who receives cargo (buyer, business)

        public bool IsContraband;                   // Illegal in destination village
        public bool IsConcealed;                    // Smuggling attempt
    }

    /// <summary>
    /// Stagecoach entity component.
    /// Fast passenger + light cargo transport.
    /// </summary>
    public struct Stagecoach : IComponentData
    {
        public byte PassengerCapacity;              // Max passengers (usually 4)
        public ushort CargoCapacity;                // kg (usually 200)
        public byte CurrentPassengers;
        public Entity Owner;                        // Stage Station business

        // Current journey
        public Entity OriginVillage;
        public Entity DestinationVillage;
        public float3 CurrentPosition;
        public uint ArrivalTick;

        public bool HasGuards;                      // Hired escort
        public bool IsActive;
    }

    /// <summary>
    /// Passenger manifest for stagecoach.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct StagecoachPassenger : IBufferElementData
    {
        public Entity PassengerEntity;              // Villager traveling
        public ushort FarePaid;                     // Currency
        public Entity Destination;                  // Village destination
        public FixedString32Bytes TravelReason;     // "trade", "migration", "diplomacy"
    }

    /// <summary>
    /// Market price component for villages.
    /// Tracks supply/demand for goods and dynamic pricing.
    /// </summary>
    [InternalBufferCapacity(32)]
    public struct MarketPrice : IBufferElementData
    {
        public enum GoodType : byte
        {
            IronOre,
            IronIngot,
            SteelIngot,
            Wheat,
            Flour,
            Weapon,
            // ... extensible
        }

        public GoodType Good;
        public ushort BasePrice;                    // Currency per kg/unit
        public ushort CurrentPrice;                 // Dynamic price
        public ushort Supply;                       // Available quantity (kg/units)
        public ushort MonthlyDemand;                // Consumption rate
        public float SupplyDemandRatio;             // Supply / Demand

        // Price calculation factors
        public float SupplyDemandMultiplier;        // (Demand/Supply)^0.5
        public float VillageWealthMultiplier;       // 0.7-1.3 based on avg wealth
        public float EventMultiplier;               // War, festival, famine, etc.
    }

    /// <summary>
    /// Trade agreement between two villages.
    /// Diplomatic economic pact.
    /// </summary>
    public struct TradeAgreement : IComponentData
    {
        public enum AgreementType : byte
        {
            FreeTrade,      // Zero tariffs, shared market
            TariffTreaty,   // Reduced tariffs (10%)
            Embargo,        // Trade banned (illegal)
            Monopoly,       // Exclusive trade rights
        }

        public AgreementType Type;
        public Entity VillageA;
        public Entity VillageB;
        public uint EstablishedTick;
        public uint ExpirationTick;                 // 0 = indefinite

        // Terms
        public float TariffRate;                    // 0.0-2.0 (0%=free, 200%=embargo)
        public float TransportCostModifier;         // -0.2 to +0.5
        public bool AllowContraband;                // Illegal goods exempted
    }

    /// <summary>
    /// Village tariff policy component.
    /// </summary>
    public struct VillageTariffPolicy : IComponentData
    {
        public float BaseTariffRate;                // 0.05-0.60 (5%-60%)
        public float AlliedModifier;                // -0.20 (discount)
        public float RivalModifier;                 // +0.50 (penalty)
        public float EmbargoPenalty;                // +2.0 (200% if caught smuggling)

        public byte EnforcementStrength;            // 0-100 (detection chance)
        public bool AllowSmuggling;                 // Corrupt villages tolerate it
    }

    /// <summary>
    /// Smuggling attempt component.
    /// Attached to caravans attempting illegal trade.
    /// </summary>
    public struct SmugglingAttempt : IComponentData
    {
        public Entity CaravanEntity;
        public Entity DestinationVillage;
        public ushort ContrabandValue;              // Currency value of illegal goods
        public byte DetectionChance;                // 0-100%
        public bool Detected;
        public bool BribeAttempted;                 // Tried to bribe guards
        public ushort BribeAmount;
    }

    /// <summary>
    /// Black market component for villages.
    /// Hidden market for contraband.
    /// </summary>
    public struct BlackMarket : IComponentData
    {
        public Entity VillageEntity;
        public byte Size;                           // 0-100 (activity level)
        public byte DiscoveryRisk;                  // 0-100% (peacekeeper crackdown)
        public float PriceMultiplier;               // 1.5-3.0× (scarcity + risk)

        public bool IsActive;                       // False if crackdown
        public uint LastRaidTick;                   // Peacekeeper raid
    }

    /// <summary>
    /// Merchant profit tracking.
    /// Records arbitrage profits across trade routes.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct MerchantProfit : IBufferElementData
    {
        public uint Tick;
        public Entity OriginVillage;
        public Entity DestinationVillage;
        public ushort GoodsSold;                    // Currency value
        public ushort CostOfGoods;                  // Purchase + transport
        public short NetProfit;                     // Can be negative if loss
        public byte ArbitrageType;                  // 0=Resource, 1=Material, 2=Product
    }

    /// <summary>
    /// Village export profile.
    /// Defines specialization and primary exports.
    /// </summary>
    public struct VillageExportProfile : IComponentData
    {
        public enum SpecializationType : byte
        {
            Mining,         // Iron, gold, stone
            Agriculture,    // Wheat, wine
            Manufacturing,  // Weapons, armor
            Luxury,         // Jewelry, enchantments
            Services,       // Entertainment, education
        }

        public SpecializationType Specialization;
        public FixedString64Bytes PrimaryExport;    // "Iron Ingots", "Legendary Weapons"
        public ushort MonthlyExportVolume;          // kg or units
        public uint ExportRevenue;                  // Currency/month

        // Competitive advantage
        public float LocalCostAdvantage;            // -40% to +0% vs base price
        public float QualityAdvantage;              // 0% to +200% (legendary items)
    }
}
```

---

## System Flow Examples

### Caravan Trade Cycle

```csharp
[BurstCompile]
public partial struct CaravanTradeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (caravan, cargo, entity) in SystemAPI
            .Query<RefRW<MerchantCaravan>, DynamicBuffer<CargoManifest>>()
            .WithEntityAccess())
        {
            if (!caravan.ValueRO.IsActive)
                continue;

            // Update journey progress
            float3 origin = GetVillagePosition(state, caravan.ValueRO.OriginVillage);
            float3 destination = GetVillagePosition(state, caravan.ValueRO.DestinationVillage);

            float distance = math.distance(origin, destination);
            float speed = GetCaravanSpeed(caravan.ValueRO.Type); // km/day
            float progress = (currentTick - caravan.ValueRO.DepartureTick) * speed / distance;

            caravan.ValueRW.JourneyProgress = math.min(1.0f, progress);
            caravan.ValueRW.CurrentPosition = math.lerp(origin, destination, progress);

            // Check for arrival
            if (progress >= 1.0f)
            {
                // Arrived at destination
                ProcessCaravanArrival(state, entity, caravan, cargo);
            }

            // Random events during journey
            if (Random.NextFloat() < GetBanditRiskChance(state, caravan.ValueRO))
            {
                ProcessBanditAmbush(state, entity, caravan, cargo);
            }
        }
    }

    private void ProcessCaravanArrival(ref SystemState state, Entity caravan, RefRW<MerchantCaravan> caravanData, DynamicBuffer<CargoManifest> cargo)
    {
        Entity destinationVillage = caravanData.ValueRO.DestinationVillage;

        // Calculate tariffs
        var tariffPolicy = state.EntityManager.GetComponentData<VillageTariffPolicy>(destinationVillage);
        uint totalTariffs = 0;

        for (int i = 0; i < cargo.Length; i++)
        {
            var item = cargo[i];

            // Check for contraband
            if (item.IsContraband)
            {
                if (item.IsConcealed)
                {
                    // Smuggling attempt
                    bool detected = DetectSmuggling(state, destinationVillage, item);

                    if (detected)
                    {
                        // Confiscate, fine, criminal record
                        PenalizeSmuggling(state, caravanData.ValueRO.Owner, item);
                        cargo.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                else
                {
                    // Open contraband → automatic seizure
                    cargo.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            // Calculate tariff
            uint tariff = (uint)(item.DeclaredValue * tariffPolicy.BaseTariffRate);
            totalTariffs += tariff;
        }

        // Owner pays tariffs
        AddExpense(state, caravanData.ValueRO.Owner, totalTariffs);

        // Deliver cargo to consignees
        foreach (var item in cargo)
        {
            TransferCargo(state, item.CargoEntity, item.Consignee);
        }

        // Clear cargo manifest
        cargo.Clear();

        // Caravan returns to origin or stays for next journey
        caravanData.ValueRW.IsActive = false;
    }
}
```

### Dynamic Market Pricing

```csharp
[BurstCompile]
public partial struct MarketPricingSystem : ISystem
{
    // Runs monthly to recalculate market prices

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (marketPrices, villageStats, entity) in SystemAPI
            .Query<DynamicBuffer<MarketPrice>, RefRO<VillageStats>>()
            .WithEntityAccess())
        {
            for (int i = 0; i < marketPrices.Length; i++)
            {
                var price = marketPrices[i];

                // Calculate supply/demand ratio
                price.SupplyDemandRatio = (float)price.Supply / math.max(1, price.MonthlyDemand);

                // Supply/demand multiplier: (Demand / Supply)^0.5
                price.SupplyDemandMultiplier = math.sqrt(1.0f / math.max(0.01f, price.SupplyDemandRatio));

                // Village wealth multiplier
                float avgWealth = villageStats.ValueRO.AverageWealth;
                if (avgWealth < 5000)
                    price.VillageWealthMultiplier = 0.7f; // Poor village
                else if (avgWealth > 20000)
                    price.VillageWealthMultiplier = 1.3f; // Wealthy village
                else
                    price.VillageWealthMultiplier = 1.0f; // Mid-tier

                // Event multiplier (war, famine, etc.)
                price.EventMultiplier = CalculateEventMultiplier(state, entity, price.Good);

                // Final price
                price.CurrentPrice = (ushort)(
                    price.BasePrice *
                    price.SupplyDemandMultiplier *
                    price.VillageWealthMultiplier *
                    price.EventMultiplier
                );

                marketPrices[i] = price;
            }
        }
    }

    private float CalculateEventMultiplier(ref SystemState state, Entity village, MarketPrice.GoodType good)
    {
        float multiplier = 1.0f;

        // Check for war
        if (IsVillageAtWar(state, village))
        {
            if (good == MarketPrice.GoodType.Weapon || good == MarketPrice.GoodType.IronIngot)
                multiplier *= 2.0f; // War demand for weapons
        }

        // Check for famine
        if (HasFamine(state, village))
        {
            if (good == MarketPrice.GoodType.Wheat || good == MarketPrice.GoodType.Flour)
                multiplier *= 5.0f; // Extreme food scarcity
        }

        // Check for festival
        if (HasFestival(state, village))
        {
            multiplier *= 1.5f; // General demand increase
        }

        return multiplier;
    }
}
```

---

## Inter-Village Loans & Economic Vassalization

### Debt-Based Conquest

**Villages can take loans** from other villages, creating debt obligations that can lead to economic vassalization and silent takeover.

```
Loan Mechanics:

Loan Request:
  Borrower Village (Village A) requests loan from Lender Village (Village B)

  Reasons:
    - War emergency (fund army, buy weapons)
    - Famine response (import food, survive crisis)
    - Infrastructure investment (build factory, expand capacity)
    - Debt consolidation (refinance existing debts)

Loan Terms:
  Principal: Amount borrowed (currency)
  Interest Rate: Annual % (5-50% depending on risk, relations, alignment)
  Repayment Schedule: Monthly, Quarterly, or Annual
  Collateral: Assets pledged if default (mines, buildings, trade routes)
  Duration: 1-20 years

Interest Rate Calculation:
  BaseRate = 10% (standard)

  Modifiers:
    + Poor Relations: +10% per -20 relations
    + High Risk (war, instability): +20%
    + No Collateral: +15%
    + Poor Credit History (previous defaults): +25%
    - Allied Villages: -5%
    - Authoritarian Lender (exploitative): +30%
    - Materialistic Lender (greedy): +20%
    - Good Lender (charitable): -8%

  Example (Desperate Wartime Loan):
    Village A (at war, poor relations with Village B)
    Loan: 50,000 currency
    Relations: -10 (neutral but tense)
    Risk: War (+20%)
    Lender: Authoritarian Village B (+30%)

    Interest Rate: 10% + 10% + 20% + 30% = 70% annual

    Annual Interest: 50,000 × 0.70 = 35,000 currency
    Monthly Payment: (50,000 / 60 months) + (35,000 / 12) = 833 + 2,917 = 3,750 currency/month

    Total Repayment (5 years): 50,000 + (35,000 × 5) = 225,000 currency (4.5× principal)

Default Conditions:
  Village defaults if:
    - Cannot make 3 consecutive payments (insolvency)
    - Refuses to pay (rebellion)
    - Attempts to flee debt (migration, exile elites)

  Consequences:
    - Collateral seized (mines, buildings, trade routes)
    - Lender gains ownership of assets
    - Diplomatic penalty (reputation -50, other villages distrust)
    - Economic sanctions (embargo, trade cut-off)
```

### Vassalization Through Debt

**Systematic debt accumulation** leads to economic vassalization where lender village gains control without military conquest.

```
Vassalization Process:

Stage 1: Initial Loan (Innocuous)
  Year 1:
    Village A borrows 10,000 currency from Village B
    Purpose: Build new farm (expand food production)
    Interest: 15% annual (moderate)
    Monthly Payment: 200 currency

  Village A Assessment:
    "Reasonable loan, can afford payments from farm surplus"
    No concern (yet)

Stage 2: Crisis Loan (Trap Set)
  Year 3:
    Village A suffers drought (crop failure)
    Food deficit: Must import 5,000 kg wheat (15,000 currency)
    Treasury: 2,000 currency (insufficient)

  Village A Options:
    Option 1: Borrow 13,000 from Village B
    Option 2: Starve (unacceptable)

  Village A Takes Loan:
    Debt increases: 10,000 → 23,000 currency
    Interest: 20% (crisis premium)
    Monthly Payment: 200 → 600 currency

  Village B Strategy (Silent Takeover):
    "Lend during crisis when desperate, charge premium rates"

Stage 3: Debt Spiral (Dependence)
  Year 5:
    Village A at war (neighbor invades)
    Weapons needed: 100 swords (12,000 currency)
    Treasury: 500 currency (almost empty, debt payments drained reserves)

  Village A Options:
    Option 1: Borrow 11,500 from Village B
    Option 2: Lose war (conquest, enslavement)

  Village A Takes Loan:
    Total Debt: 34,500 currency
    Interest: 30% (war risk premium)
    Monthly Payment: 600 → 1,200 currency

  Village A Treasury:
    Monthly Revenue: 3,000 currency (taxes, business profits)
    Monthly Expenses: 2,000 currency (wages, maintenance)
    Monthly Debt Payment: 1,200 currency

    Net: 3,000 - 2,000 - 1,200 = -200 currency (deficit)

  Result: Village A cannot meet basic expenses + debt
    → Village A must cut services (fire workers, close businesses)
    → Economic decline (businesses fail, population drops)
    → Downward spiral begins

Stage 4: Default & Vassalization (Takeover)
  Year 7:
    Village A misses 3 consecutive debt payments
    Total Debt: 50,000 currency (compounding interest + penalties)

  Village B Enforcement:
    "Village A has defaulted. We are seizing collateral per loan terms."

  Collateral Seized:
    - Iron mine (Village A's primary income source): Now owned by Village B
    - 3 farms: Now owned by Village B
    - Market building: Now owned by Village B
    - Trade routes: Village B controls access

  Economic Vassalization:
    Village A no longer controls its own economy
    Village B owns 60% of Village A's productive assets
    Village A becomes economic vassal (de facto colony)

  Silent Takeover Complete:
    No military conquest needed
    No diplomatic penalty (legal debt enforcement)
    Village A remains "independent" (puppet state)

Stage 5: Elite Indenture (Political Control)
  Year 10:
    Village A elites deeply in debt (personal loans from Village B elites)
    Owed: 100,000 currency (collective elite debt)

  Elite Options:
    Option 1: Pay debt (impossible, already impoverished)
    Option 2: Flee (abandon village, lose all wealth)
    Option 3: Submit to Village B control

  Village B Offer:
    "We will forgive debt IF you pledge loyalty to Village B"
    "Your children will marry our elites (arranged marriages)"
    "You will vote in our interests on village council"
    "You will enforce our economic policies"

  Elite Submission:
    Village A elites become agents of Village B
    Village A policies now serve Village B interests
    Village A becomes puppet state (political vassalization)

Stage 6: Population Indenture (Complete Control)
  Year 15:
    Village A population in debt (40% villagers owe money to Village B citizens)
    Debts: 5,000 currency average per indebted villager

  Village B Strategy:
    "Work off debt through labor contracts"
    "Your children will work for us (generational debt)"
    "Debt servitude for 20 years"

  Population Indenture:
    Village A becomes indentured labor force for Village B
    Villagers cannot leave (debt bondage)
    Children inherit parents' debts (perpetual servitude)

  Complete Vassalization:
    Village A is colony of Village B in all but name
    Economic, political, and social control achieved
    No military force used (purely economic conquest)
```

### Behavioral Factors (Alignment & Outlooks)

**Loan outcomes depend on** borrower/lender alignments and behavioral traits.

```
Lender Behaviors:

Authoritarian Lender:
  Strategy: Exploit debt for political control
  Interest Rates: High (30-50%)
  Forgiveness: Never (extractive)
  Goal: Vassalization, puppet state
  Example: Village B lends to Village A at predatory rates, seizes assets on default

Materialistic Lender:
  Strategy: Maximize profit from interest
  Interest Rates: Moderate-High (15-30%)
  Forgiveness: Rare (only if profitable)
  Goal: Wealth accumulation, not political control
  Example: Merchant guild lends at high rates, willing to renegotiate if profitable

Good/Charitable Lender:
  Strategy: Help allies, strengthen bonds
  Interest Rates: Low (5-10%)
  Forgiveness: Common (if genuine hardship)
  Goal: Build trust, mutual prosperity
  Example: Allied village lends at low rates, forgives debt during famine

Evil/Exploitative Lender:
  Strategy: Debt trap, enslavement
  Interest Rates: Predatory (50-100%)
  Forgiveness: Never
  Goal: Indenture population, slave labor
  Example: Evil empire lends to struggling village, demands debt servitude

Borrower Behaviors:

Lawful Borrower:
  Debt Honor: Always attempts to pay
  Default Risk: Low (will sacrifice to repay)
  Reaction to Exploitation: Endures, seeks legal resolution
  Example: Lawful Good village pays debt even during famine (honor-bound)

Chaotic Borrower:
  Debt Honor: Opportunistic (pays if convenient)
  Default Risk: High (will flee if cornered)
  Reaction to Exploitation: Rebellion, debt repudiation
  Example: Chaotic Neutral village refuses to pay predatory debt, flees region

Desperate Borrower (Any Alignment):
  Debt Honor: Irrelevant (survival priority)
  Default Risk: Very High (cannot pay)
  Reaction to Exploitation: Submission (no choice)
  Example: Village facing starvation takes ANY loan terms, regardless of consequences

Strategic Borrower (Materialistic/Cunning):
  Debt Honor: Uses debt strategically
  Default Risk: Calculated (defaults if profitable)
  Reaction to Exploitation: Counter-exploitation
  Example: Village borrows heavily, uses funds to build military, conquers lender, debt erased

Alignment Impact on Terms:

Good Lender + Good Borrower:
  Interest: 5-8% (friendly, low-risk)
  Forgiveness: Common (mutual support)
  Outcome: Strengthened alliance, shared prosperity

Authoritarian Lender + Lawful Borrower:
  Interest: 20-35% (exploitative but honored)
  Forgiveness: Never (extractive relationship)
  Outcome: Vassalization (borrower honors debts to ruin)

Evil Lender + Desperate Borrower:
  Interest: 50-100% (predatory)
  Forgiveness: Never
  Outcome: Debt slavery, indenture, complete subjugation

Chaotic Lender + Chaotic Borrower:
  Interest: Variable (unpredictable)
  Forgiveness: Random (whim-based)
  Outcome: Chaos (defaults, renegotiations, disputes)
```

### Debt Resistance & Counter-Strategies

**Borrowers can resist vassalization** through various means.

```
Resistance Strategies:

1. Debt Repudiation (Chaotic/Revolutionary):
  Action: Refuse to pay debt, declare it illegitimate
  Justification: "Predatory lending is theft, not legitimate debt"

  Consequences:
    - Diplomatic penalty (-50 relations with lender, -20 with all villages)
    - Economic sanctions (lender embargoes borrower)
    - Military threat (lender may invade to enforce debt)
    - Reputation damage (other villages distrust borrower)

  Success Conditions:
    - Borrower has military strength to resist invasion
    - International support (other villages condemn predatory lending)
    - Lender too weak/distant to enforce

  Example:
    Village A declares Village B's 100,000 debt "illegitimate usury"
    Village A refuses payment, severs diplomatic ties
    Village B embargoes Village A (trade cut-off)
    Village A survives via self-sufficiency + trade with Village C
    Result: Debt erased, but isolated economically

2. Debt Consolidation (Refinance):
  Action: Take loan from Village C to pay off Village B
  Terms: Lower interest rate from friendly lender

  Example:
    Village A owes Village B 50,000 at 40% interest
    Village C (ally) offers 50,000 loan at 8% interest
    Village A pays off Village B, now owes Village C at manageable rate

  Result: Escape predatory lender, maintain honor, avoid default

3. Economic Growth (Outgrow Debt):
  Action: Invest borrowed funds in productive capacity
  Goal: Generate revenue exceeding debt payments

  Example:
    Village A borrows 20,000 to build factory
    Factory generates 5,000 revenue/month
    Debt payment: 1,000/month (20% of factory income)
    Net Gain: 4,000/month

  Result: Debt manageable, village thrives despite debt

4. Military Conquest (Eliminate Lender):
  Action: Conquer lender village, erase debt by force
  Justification: "Might makes right"

  Example:
    Village A owes Village B 80,000 (crippling)
    Village A builds army, conquers Village B
    Debt erased (lender no longer exists)
    Village A absorbs Village B's assets

  Result: Debt eliminated, but aggressive expansion creates enemies

5. Debt Strike (Collective Repudiation):
  Action: Multiple borrower villages coordinate default
  Power: Lender cannot enforce against all simultaneously

  Example:
    Villages A, C, D all owe Village B (total: 200,000)
    Villages coordinate: All default simultaneously
    Village B cannot seize all collateral (lacks capacity)
    Village B forced to negotiate (forgive 50% debt)

  Result: Partial debt relief through collective action

6. Asset Hiding (Fraud):
  Action: Hide assets, claim poverty, avoid seizure
  Risk: If discovered, total loss of reputation

  Example:
    Village A transfers mine ownership to "independent merchant"
    Village A claims "no assets to seize"
    Village B investigates, discovers fraud
    Diplomatic catastrophe: Village A branded criminal

  Result: Risky, high penalty if caught
```

### DOTS Component Architecture

```csharp
namespace Godgame.Economy.Finance
{
    /// <summary>
    /// Inter-village loan component.
    /// Tracks debt obligations between villages.
    /// </summary>
    public struct InterVillageLoan : IComponentData
    {
        public Entity BorrowerVillage;           // Village A (owes money)
        public Entity LenderVillage;             // Village B (lender)
        public uint Principal;                   // Original loan amount
        public uint RemainingDebt;               // Current balance owed
        public float AnnualInterestRate;         // 0.05-1.0 (5%-100%)
        public uint MonthlyPayment;              // Required payment each month
        public uint LoanIssuedTick;              // When loan granted
        public uint LoanMaturityTick;            // When loan must be repaid
        public byte MissedPayments;              // Consecutive missed payments (default at 3)

        public bool IsInDefault;                 // Borrower defaulted
        public uint DefaultTick;                 // When default occurred
    }

    /// <summary>
    /// Collateral pledged for loan.
    /// Assets seized on default.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct LoanCollateral : IBufferElementData
    {
        public enum CollateralType : byte
        {
            Mine,           // Resource node
            Building,       // Business, farm, factory
            TradeRoute,     // Exclusive trade access
            Territory,      // Land ownership
        }

        public CollateralType Type;
        public Entity CollateralEntity;          // Asset pledged
        public uint EstimatedValue;              // Currency value
        public bool IsSeized;                    // Lender took possession
    }

    /// <summary>
    /// Debt vassalization state.
    /// Tracks economic control percentage.
    /// </summary>
    public struct DebtVassalization : IComponentData
    {
        public Entity VassalVillage;             // Village A (controlled)
        public Entity OverlordVillage;           // Village B (controller)
        public float EconomicControl;            // 0.0-1.0 (0%=independent, 100%=colony)
        public float PoliticalControl;           // 0.0-1.0 (elite influence)
        public float PopulationIndenture;        // 0.0-1.0 (% in debt servitude)
        public uint VassalizationStartTick;

        public bool IsCompleteVassalization;     // Full puppet state
    }

    /// <summary>
    /// Debt repudiation event.
    /// Borrower refuses to pay.
    /// </summary>
    public struct DebtRepudiation : IComponentData
    {
        public enum RepudiationType : byte
        {
            Chaotic,        // Opportunistic default
            Revolutionary,  // Political refusal (predatory lending)
            Conquest,       // Military elimination of lender
            Bankruptcy,     // Genuine insolvency
        }

        public RepudiationType Type;
        public Entity BorrowerVillage;
        public Entity LenderVillage;
        public uint DebtRepudiated;              // Amount refused
        public uint RepudiationTick;

        public byte DiplomaticPenalty;           // -50 relations
        public bool MilitaryResponseActive;      // Lender invading
    }
}
```

---

## Integration with Existing Systems

### Resource Production & Crafting

✅ **Trade enables cross-village material sourcing:**
- Blacksmith in Village A imports rare mithril from Village B via caravan
- Quality propagation works across villages (import high-purity ore)
- Legendary craftsmen can import legendary materials from distant mines

✅ **Transportation costs affect material selection:**
- Nearby iron mine: 1.2 currency/kg (cheap transport)
- Distant mithril mine: 50 currency/kg + 10 currency transport = 60 total
- Artisan must balance quality vs cost

### Business & Assets

✅ **Transportation businesses profit from trade:**
- Stage stations earn revenue from passenger fares + cargo fees
- Merchant caravans owned by merchant businesses
- Trade volume affects business profitability

✅ **Outsourcing crosses village boundaries:**
- Village A blacksmith outsources enchantments to Village B enchanter
- Transport cost factored into outsourcing decisions

### Wealth & Social Dynamics

✅ **Trade creates merchant class:**
- Successful merchants accumulate wealth through arbitrage
- Rise from Mid-wealth → High → Ultra High via trade networks
- Merchant families become dynasties (Vendari Trading House)

✅ **Economic inequality:**
- Villages with valuable exports (gold mines) become wealthy
- Resource-poor villages remain poor (must import everything)
- Creates migration pressure (poor villagers migrate to wealthy trade hubs)

### Alignment Framework

✅ **Trade agreements reflect alignments:**
- Lawful Good villages: Free trade agreements, low tariffs
- Authoritarian villages: Protectionist tariffs, monopolies
- Chaotic Evil villages: Smuggling, black markets, piracy

✅ **Ethical trade practices:**
- Good merchants refuse to trade slaves or contraband
- Evil merchants profit from smuggling embargoed goods

---

## Failure Modes

**Trade Route Collapse:**
```
Major trade route blocked (war, bandit takeover)
  → Supply chain disruption
  → Scarcity in dependent villages
  → Price spikes (5-10× inflation)
  → Starvation risk if food imports blocked

Recovery:
  - Military expedition to clear bandits
  - Establish alternative route (longer, more expensive)
  - Diplomatic resolution (end war, reopen borders)
```

**Tariff War:**
```
Village A raises tariffs → Village B retaliates with higher tariffs
  → Trade volume collapses
  → Both villages lose revenue
  → Embargo spiral

Recovery:
  - Diplomatic negotiation (reduce tariffs mutually)
  - Third-party mediation (neutral village brokers peace)
  - Economic pain forces capitulation
```

**Smuggling Epidemic:**
```
High tariffs → Widespread smuggling
  → Village tariff revenue collapses
  → Black market dominates economy
  → Criminal guilds gain power

Recovery:
  - Reduce tariffs (legitimize trade)
  - Crackdown on smugglers (peacekeepers, executions)
  - Bribe officials (corrupt villages embrace it)
```

**Merchant Monopoly:**
```
Single merchant family controls all trade routes
  → Price-fixing (artificially high)
  → Villages dependent on monopolist
  → Economic extortion

Recovery:
  - Village regulation (break up monopoly)
  - New merchants enter (entrepreneurship)
  - Villages establish state-owned caravans (nationalization)
```

---

## Player Interaction

**Observable:**
- Merchant caravans visible on map (moving between villages)
- Trade route quality indicated (well-traveled roads vs overgrown paths)
- Market prices displayed (economic heatmaps)
- Smuggling raids create drama (public executions, cargo seizures)

**Control Points:**
- **Indirect:** Player miracles affect trade (bless caravan, curse bandits)
- **Indirect:** Player influences village relations (improve/worsen trade agreements)
- **Indirect:** Player education miracles create skilled merchants
- **Direct (potential):** Player can bless/curse specific trade routes (prosperity/banditry)

**Learning Curve:**
- **Beginner:** Notices caravans moving, understands villages trade
- **Intermediate:** Recognizes price arbitrage, economic specialization
- **Expert:** Manipulates trade networks to create economic dependencies, isolate rival villages

---

## Related Documentation

- **Resource Production & Crafting:** [Resource_Production_And_Crafting.md](Resource_Production_And_Crafting.md) - Material sourcing, quality propagation
- **Business & Assets:** [BusinessAndAssets.md](BusinessAndAssets.md) - Transportation businesses, merchant operations
- **Wealth & Social Dynamics:** [../Villagers/Wealth_And_Social_Dynamics.md](../Villagers/Wealth_And_Social_Dynamics.md) - Merchant class, wealth accumulation
- **Entity Relations:** [../Villagers/Entity_Relations_And_Interactions.md](../Villagers/Entity_Relations_And_Interactions.md) - Diplomatic trade agreements
- **Alignment Framework:** [../Meta/Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Trade policies, smuggling ethics
- **Guild System:** [../Villagers/Guild_System.md](../Villagers/Guild_System.md) - Merchant guilds, trade networks

---

**For Designers:** Balance tariff rates and smuggling risk to create meaningful economic choices. Trade should feel like a strategic layer, not just passive background simulation.

**For Implementers:** Start with simple point-to-point trade routes and basic pricing. Dynamic market pricing and smuggling can be layered on once foundation is stable.

**For Narrative:** Trade creates emergent stories of merchant dynasties, economic warfare, smuggling rings, and village dependencies that drive political intrigue.

---

**Last Updated:** 2025-11-07
**Status:** Concept Draft - Core vision captured, ready for integration with Resource Production & Business systems
