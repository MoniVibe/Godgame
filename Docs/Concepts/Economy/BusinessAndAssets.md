# Business & Asset Ownership System

**Status:** Draft - Concept Design
**Category:** System - Economic & Social
**Scope:** Global (affects villages, families, individuals)
**Created:** 2025-11-06
**Last Updated:** 2025-11-06

---

## Purpose

**Primary Goal:** Create a dynamic economic layer where entities (individuals, families, aggregates) own, operate, and compete through businesses and assets, generating emergent wealth distribution and economic narratives.

**Secondary Goals:**
- Enable wealth generation beyond simple resource gathering
- Create economic competition and cooperation dynamics
- Tie individual skills (education, intelligence, wisdom) to economic outcomes
- Generate inheritance and succession narratives
- Support village economic policies based on alignment (nationalization vs free market)
- Create jobs and employment for villagers
- Enable economic warfare and support (donations, takeovers, franchising)

---

## System Overview

### Core Concept

**Businesses** are data entities attached to production and leisure buildings. They:
- Are owned by one or multiple entities (individuals, families, aggregates)
- Generate profits and pay wages based on transactions
- Gain levels, reputation, and product quality/rarity through employee experience
- Can succeed, fail, be inherited, nationalized, franchised, or migrate
- Respond to economic conditions (wars, sieges, festivals, taxes)

**Assets** are ownable properties that generate value:
- Resource nodes (mines, quarries, forests)
- Real estate (houses, bathhouses, arenas, circuses)
- Agricultural land (wheat fields, vineyards, orchards)
- Storage infrastructure (granaries, warehouses)

### Components

1. **Business Entity** - Data entity with ownership, finances, employees, reputation
2. **Asset Ownership** - Links entities to properties, nodes, buildings
3. **Ownership Shares** - Multi-party ownership via agreements
4. **Business Transactions** - Revenue generation events
5. **Employment Contracts** - Wage-paying relationships
6. **Inheritance Rules** - Succession on owner death
7. **Business Leveling** - Experience and quality progression
8. **Economic Policies** - Village governance rules (taxes, nationalization)
9. **Business Operations** - Outsourcing, franchising, migration

### Connections

```
Entities (Individuals/Families/Aggregates)
    ↓ (own)
Business Shares ← Ownership Agreements
    ↓
Business Entity (attached to Building)
    ↓ (employs)
Villagers → Work → Generate Transactions
    ↓
Revenue → Profits (owners) + Wages (employees) + Taxes (village)
    ↓
Wealth Accumulation → Wealth Strata Shifts
    ↓
Quality/Rarity Improvements (via experience)
    ↓
Reputation & Level Growth
```

### Feedback Loops

**Positive (Success Spiral):**
- Good business → profits → invest in wages/education → skilled workers → better quality → more customers → more profits

**Positive (Franchise Expansion):**
- Successful business → franchise → multiple locations → economies of scale → brand recognition → dominance

**Negative (Death Spiral):**
- Poor quality → fewer customers → low revenue → can't pay wages → workers leave → quality drops further → bankruptcy

**Negative (Tax Burden):**
- High village taxes → low profits → businesses migrate → tax base shrinks → village raises taxes higher → more migration

**Balance (Competition):**
- Multiple businesses compete → prices stabilize → quality improves → customers benefit

---

## Business Types

### Production Businesses

**Blacksmiths**
- **Attached to:** Smithy buildings
- **Products:** Weapons, armor, tools
- **Quality factors:** Physique (strength for forging), Intelligence (design)
- **Rarity factors:** Wisdom (rare material knowledge), Education (advanced techniques)
- **Revenue:** Equipment sales, repairs during war
- **Outsourcing:** Aggregate bands contract for army equipment

**Enchanters/Runers/Jewelcrafters**
- **Attached to:** Enchantment workshops
- **Products:** Magical items, enchanted equipment, jewelry
- **Quality factors:** Intelligence (magical theory), Will (magical stamina)
- **Rarity factors:** Wisdom (rare enchantment knowledge), Arcane achievements
- **Revenue:** High-value luxury sales to elites
- **Outsourcing:** Guilds contract for blessed/cursed items

**Wand Makers**
- **Attached to:** Magical workshops
- **Products:** Wands, staves, magical foci
- **Quality factors:** Finesse (precise crafting), Intelligence
- **Rarity factors:** Wisdom, magical material access
- **Revenue:** Mages' Guild purchases, elite buyers

**Tailors/Leatherworkers**
- **Attached to:** Textile workshops
- **Products:** Clothing, leather goods
- **Quality factors:** Finesse (sewing precision)
- **Rarity factors:** Wisdom (rare fabrics), access to exotic materials
- **Revenue:** Steady population clothing needs

**Farmers (Agricultural Businesses)**
- **Attached to:** Wheat fields, orchards, vineyards
- **Products:** Food, wine, specialty crops
- **Quality factors:** Education (crop rotation), Intelligence (yield optimization)
- **Rarity factors:** Wisdom (heirloom varieties), land quality
- **Revenue:** Food sales, festival supplies

### Service Businesses

**Inns & Taverns**
- **Attached to:** Inn/tavern buildings
- **Products:** Food, drink, lodging, entertainment
- **Quality factors:** Charisma (hospitality), Education (cuisine knowledge)
- **Rarity factors:** Wisdom (exotic recipes), reputation
- **Revenue:** Daily customer traffic, traveler volume
- **Special:** Festival night shifts (surge demand)

**Bathhouses**
- **Attached to:** Bathhouse buildings
- **Products:** Bathing, massage, relaxation services
- **Quality factors:** Charisma (service quality), Finesse (massage skill)
- **Rarity factors:** Wisdom (therapeutic knowledge), luxury amenities
- **Revenue:** Elite clientele, status symbol

**Stage Stations (Transport)**
- **Attached to:** Stables/caravan hubs
- **Products:** Passenger transport, cargo hauling
- **Quality factors:** Logistics skill, route knowledge
- **Rarity factors:** Access to rare destinations, speed bonuses
- **Revenue:** Per-trip fees, contracts with merchants
- **Outsourcing:** Merchants' Guild contracts

**Arenas/Circuses (Entertainment)**
- **Attached to:** Arena/circus buildings
- **Products:** Gladiatorial combat, performances, spectacles
- **Quality factors:** Charisma (showmanship), Physique (performers)
- **Rarity factors:** Exotic acts, legendary performers
- **Revenue:** Ticket sales, elite sponsorship
- **Special:** Political tool (bread and circuses)

### Military/Security Businesses

**Mercenary Companies**
- **Attached to:** Barracks or mercenary halls
- **Products:** Military services, bodyguards, enforcement
- **Quality factors:** Physique, Combat achievements, Tactics
- **Rarity factors:** Elite units, legendary commanders
- **Revenue:** War contracts, peacetime security
- **Outsourcing:** Aggregate bands hire for wars
- **Special:** Can form bands or armies if enough contractors

**Gladiator Services (Elite)**
- **Attached to:** Arenas, training grounds
- **Products:** Gladiators for elite entertainment, bodyguards
- **Quality factors:** Physique, Combat skill
- **Rarity factors:** Undefeated champions, exotic fighters
- **Revenue:** Elite purchases, rental fees
- **Ethical:** Slavery alignment (evil/authoritarian villages)

**Servant/Slave Services (Elite)**
- **Attached to:** Slave markets, servant quarters
- **Products:** Domestic servants, forced labor
- **Quality factors:** Varies by task
- **Rarity factors:** Skilled servants, exotic slaves
- **Revenue:** Elite purchases, rentals
- **Ethical:** Only in evil/corrupt/authoritarian villages
- **Moral:** Presence affects village alignment negatively

### Knowledge Businesses

**Schools/Academies**
- **Attached to:** School buildings
- **Products:** Education services
- **Quality factors:** Education (teacher knowledge), Intelligence
- **Rarity factors:** Wisdom (advanced topics), famous scholars
- **Revenue:** Tuition from wealthy families
- **Outsourcing:** Scholars' Guild support

**Libraries**
- **Attached to:** Library buildings
- **Products:** Book access, research services
- **Quality factors:** Education, Book collection size
- **Rarity factors:** Rare books, ancient texts
- **Revenue:** Subscriptions, copying fees

---

## Ownership Models

### Individual Ownership

**Single Proprietor:**
- One villager owns 100% of business
- Makes all decisions
- Receives all profits (minus wages/taxes)
- Risk: Death triggers inheritance or nationalization

**Example:**
```
Marcus the Blacksmith owns "Marcus's Forge"
- 100% owner
- Employs 2 apprentices (wages: 50 currency/month each)
- Revenue: 500/month
- Taxes: 100/month (20% village rate)
- Wages: 100/month
- Profit: 300/month → Marcus's wealth
```

### Multi-Party Ownership (Shares)

**Partnership:**
- 2+ entities own shares via ownership agreement
- Shares dictate profit distribution
- Decisions may require consensus or majority

**Example:**
```
"Golden Goblet Inn" owned by:
- Elira (40% share) - manages operations
- Her brother Theron (30% share) - financial backer
- Merchants' Guild (30% share) - franchise investor

Revenue: 1000/month
Taxes: 200/month
Wages: 300/month
Profit: 500/month
  → Elira: 200
  → Theron: 150
  → Merchants' Guild: 150
```

**Acquisition Mechanics:**
- Entities can **purchase** shares from existing owners (negotiated price)
- Entities can **invest** in new business creation (initial capital)
- Entities can **extort/blackmail** for ownership (corrupt/evil action)
- Aggregates can **support** struggling businesses (donation for equity)
- Entities can **takeover** by buying out all other owners

### Village/Collective Ownership

**Nationalized Businesses:**
- Village owns 100% (authoritarian alignment)
- Profits go to village treasury
- Manager appointed by village council
- No inheritance (state property)

**Cooperative Ownership:**
- Workers collectively own (egalitarian alignment)
- Equal shares among employees
- Democratic management
- Profits split evenly

**Alignment Impact:**
```
Authoritarian Village (Lawful/Authoritarian):
  → 70%+ businesses nationalized
  → Elite families control remainder via council influence

Egalitarian Village (Good/Egalitarian):
  → 70%+ businesses worker cooperatives
  → Free market for new businesses

Materialistic/Corrupt Village:
  → Free market BUT high taxes (30-50%)
  → Elite families monopolize through wealth

Neutral Village:
  → Mixed economy
  → ~50% private, ~30% nationalized, ~20% cooperative
```

---

## Business Finances

### Revenue Sources

**Transaction-Based:**
- Sales of products/services
- Volume × Price × Quality Multiplier
- **Formula:**
  ```
  DailyRevenue = BaseCustomers × ProductPrice × QualityMultiplier × ReputationMultiplier
  ```

**Quality Multiplier:**
```
Quality Tier 0 (Poor):       0.5× price (customers demand discount)
Quality Tier 1 (Average):    1.0× price
Quality Tier 2 (Good):       1.3× price
Quality Tier 3 (Excellent):  1.7× price
Quality Tier 4 (Masterwork): 2.5× price
Quality Tier 5 (Legendary):  4.0× price (rare, sought after)
```

**Reputation Multiplier:**
```
Reputation 0-20:    0.7× customers (poor reputation)
Reputation 21-40:   0.9× customers
Reputation 41-60:   1.0× customers (neutral)
Reputation 61-80:   1.2× customers (good reputation)
Reputation 81-100:  1.5× customers (famous, attracts distant buyers)
```

### Expenses

**Wages:**
- Each employee requires monthly wage
- Wage amount based on skill level and village wage baseline
- **Formula:**
  ```
  EmployeeWage = VillageBaseWage × SkillMultiplier × NegotiationFactor

  SkillMultiplier:
    Unskilled: 0.8×
    Skilled:   1.2×
    Expert:    1.8×
    Master:    3.0×
  ```

**Village Taxes:**
- Percentage of gross revenue (before wages/expenses)
- Rate determined by village alignment and economic policy
- **Tax Rates:**
  ```
  Egalitarian Good:       10-15% (low taxes, free market)
  Neutral:                20-25%
  Authoritarian:          30-40% (high control)
  Corrupt Materialistic:  40-60% (extractive)
  ```

**Maintenance:**
- Building upkeep costs
- Equipment depreciation
- Material restocking

**Optional Investments:**
- Owner can reinvest profits into:
  - Wage increases (attract better workers)
  - Training programs (improve quality faster)
  - Expansion (larger capacity)
  - Franchise creation
  - Beautification (increase desirability)

### Profitability

**Net Profit Calculation:**
```
NetProfit = Revenue - Taxes - Wages - Maintenance - Investments

Example (Blacksmith):
  Revenue:      800/month (50 transactions × 16 avg price)
  Taxes:        160/month (20% rate)
  Wages:        200/month (2 employees × 100)
  Maintenance:  40/month
  Investments:  100/month (training program)

  Net Profit:   300/month → Owner's wealth
```

**Bankruptcy Threshold:**
- Business has negative wealth for **1 year** → Dies
- During negative period:
  - Cannot pay full wages (workers may leave)
  - Reputation drops (-5/month)
  - Quality degrades (lack of investment)
  - Desirability drops (visible decline)

**Support/Transfusion:**
- Entities can **donate/transfusion** funds to keep business alive
- Motivations:
  - Family support (keep family business afloat)
  - Strategic (prevent competitor from buying distressed asset)
  - Altruistic (Good alignment entities help struggling businesses)
  - Investment (buy equity stake during crisis at discount)

---

## Business Progression

### Leveling System

**Business Level:** 1-10
- Starts at level 1 (new business)
- Gains experience from successful transactions
- Each level increases:
  - Base quality potential (+5%)
  - Reputation ceiling (+10 max reputation)
  - Customer attraction (+10% base customers)
  - Employee capacity (+1 slot)

**Experience Gain:**
```
XP per successful transaction = TransactionValue × QualityTier

Example:
  100 currency sale at Quality Tier 3 = 300 XP

Level Thresholds:
  Level 1→2:  1,000 XP
  Level 2→3:  3,000 XP
  Level 3→4:  6,000 XP
  Level 4→5:  10,000 XP
  Level 5→6:  15,000 XP
  Level 6→7:  22,000 XP
  Level 7→8:  30,000 XP
  Level 8→9:  40,000 XP
  Level 9→10: 50,000 XP
```

### Reputation System

**Reputation Score:** 0-100
- Affects customer attraction (see multiplier above)
- Gained from:
  - High quality products (+1 per quality tier above average)
  - Successful transactions (+0.1 per transaction)
  - Positive word-of-mouth (random events)
  - Elite patronage (+5 if elite family customer)
- Lost from:
  - Poor quality (-2 per quality tier below average)
  - Failed orders (-5)
  - Scandals (-20) (owner crimes, unethical practices)
  - Bankruptcy rumors (-10)

**Reputation Tiers:**
```
0-20:    Unknown/Poor (new or failing business)
21-40:   Struggling (unreliable)
41-60:   Established (average, reliable)
61-80:   Reputable (known quality)
81-95:   Famous (regional recognition)
96-100:  Legendary (world-renowned, attracts pilgrims)
```

### Product Quality

**Quality Score:** 0-100 (maps to tiers 0-5)
- Determined by:
  - **Employee skill average** (40% weight)
  - **Business level** (20% weight)
  - **Investment in training** (20% weight)
  - **Material quality** (20% weight - if applicable)

**Skill Calculation:**
```
EmployeeSkill = Education + (Intelligence × 0.5) + (RelevantStatistic × 0.5)

RelevantStatistic examples:
  Blacksmith: Physique
  Enchanter: Will + Intelligence
  Innkeeper: Charisma
  Scholar: Intelligence + Wisdom

AverageEmployeeSkill = Σ(EmployeeSkill) / EmployeeCount

QualityScore =
  (AverageEmployeeSkill × 0.4) +
  (BusinessLevel × 5 × 0.2) +
  (TrainingInvestment × 0.2) +
  (MaterialQuality × 0.2)

Quality Tier Mapping:
  0-20:   Tier 0 (Poor)
  21-40:  Tier 1 (Average)
  41-60:  Tier 2 (Good)
  61-80:  Tier 3 (Excellent)
  81-95:  Tier 4 (Masterwork)
  96-100: Tier 5 (Legendary)
```

### Product Rarity

**Rarity Score:** 0-100 (maps to tiers 0-5)
- Determined by:
  - **Wisdom** of master craftsperson (50% weight)
  - **Business reputation** (30% weight)
  - **Access to rare materials** (20% weight)

**Rarity Effects:**
```
Common (0-20):     Standard products, widely available
Uncommon (21-40):  Better-than-average, sought after
Rare (41-60):      Distinctive, valuable
Very Rare (61-80): Exotic, coveted by elites
Epic (81-95):      One-of-a-kind pieces, collectors
Legendary (96-100): Mythical items, stories told about them
```

**Rarity Price Multiplier:**
- Stacks with Quality multiplier
- Common: 1.0×
- Uncommon: 1.3×
- Rare: 1.8×
- Very Rare: 2.5×
- Epic: 4.0×
- Legendary: 8.0×

**Combined Example:**
```
Legendary Quality (4.0×) + Epic Rarity (4.0×) = 16× base price

A basic sword (10 currency) crafted as:
  Legendary Quality + Epic Rarity = 160 currency

Only achievable with:
  - Master blacksmith (Education 90+, Physique 80+, Intelligence 70+)
  - Wisdom 95+ (knows secret techniques)
  - Business level 10
  - Access to rare meteoric iron
```

---

## Inheritance & Succession

### Succession Order

When business owner **dies**, ownership passes in this priority:

1. **Family Members** (highest priority)
   - Spouse (50% chance)
   - Children (40% chance if adult)
   - Siblings (10% chance)
   - Weighted by:
     - Relevant skill (can they run business?)
     - Family standing (favor with deceased)
     - Initiative (proactive members more likely)

2. **Business Partners** (if no suitable family)
   - Other shareholders buy deceased's shares
   - Split proportional to existing ownership

3. **Business Employees** (if no partners)
   - Senior employee inherits (highest skill + tenure)
   - Other employees may buy in as partners

4. **Village/Nationalization** (if no suitable heir)
   - Village seizes business
   - Becomes state-owned
   - Former employees retained as state workers

5. **Demolition** (if village rejects)
   - Business entity deleted
   - Building returns to village pool
   - Assets liquidated

### Inheritance Mechanics

**Family Inheritance:**
```
Eligible heirs roll weighted probability:

HeirScore =
  RelevantSkill × 0.4 +
  FamilyStanding × 0.3 +
  Initiative × 0.2 +
  WealthStrata × 0.1

Heir with highest score inherits.

If HeirScore < MinimumThreshold (40):
  → Pass to next category (partners)
```

**Disowning Impact:**
- Disowned family members **excluded** from inheritance
- Zero chance to inherit, even if highest skilled
- Creates drama: "Talented son disowned, business given to incompetent nephew"

**Dynasty Continuity:**
- Ultra High dynasties ensure businesses stay in family
- May arrange marriages to merge business empires
- Train children specifically for family business

---

## Economic Dynamics

### War Production

**Blacksmiths during War:**
- Aggregate bands **outsource contracts** for:
  - Weapons production (swords, spears, armor)
  - Equipment repairs (battle damage)
  - Siege equipment (if advanced)

**Contract Mechanics:**
```
BandLeader requests: "100 swords, 50 armor sets"
Blacksmith quotes price based on:
  - Material cost
  - Labor time
  - Current capacity
  - War premium (surge pricing)

If accepted:
  → Band pays upfront or upon delivery
  → Blacksmith prioritizes order (may delay civilian orders)
  → Reputation gains if delivered on time (+10)
  → Reputation loss if failed (-20) and contract penalty
```

**Mercenary Companies:**
- Form **bands** if enough contractors/employees
- Can form **armies** for the right sum
- Outsourced by:
  - Villages (defense contracts)
  - Wealthy families (private armies)
  - Aggregate bands (reinforcements)

### Peace Investments

**Beautification Businesses:**
- **Motivation:** Attract villagers, increase land value via desirability
- Elites invest in:
  - Garden landscaping
  - Architectural decorations
  - Public art commissions
  - Festival sponsorships

**Desirability Impact:**
```
High beautification investment →
  → Property values increase (+10-30%)
  → Attracts migrants (reputation bonus)
  → Elite status symbol (social competition)

Aggregates compete:
  → Who has most beautiful estate?
  → Drives continuous investment
```

### Festival Surges

**During Festivals:**
- **Inns/Taverns:** Night shifts to meet demand
  - Revenue: +200-400% (surge pricing + volume)
  - Wages: +50% (overtime pay)
  - Exhaustion: Workers gain fatigue (temporary productivity drop after)

- **Food Vendors:** Supply festivities
  - Contracts with village for festival supplies
  - High-volume, low-margin

- **Entertainment:** Arenas, circuses, performers
  - Ticket sales spike
  - Reputation gains (+5 if successful festival)

### Siege Shortages

**During Sieges:**
- **Supply disruptions:**
  - Cannot import materials (no caravans)
  - Rationing reduces customer base
  - Prices spike (scarcity)

- **Production Halts:**
  - Businesses requiring external inputs **cannot produce**
  - Example: Blacksmith with no iron ore stockpile → idle

- **Survival Mode:**
  - Revenue drops to near zero
  - Businesses with reserves survive on savings
  - Others go negative (bankruptcy risk)

**Post-Siege Recovery:**
- Businesses with stockpiles profit from scarcity pricing
- Failed businesses create opportunities (new owners buy cheap)

### Franchising

**Conditions:**
- Business must have:
  - Level 5+ (proven success)
  - Reputation 60+ (recognized brand)
  - Wealth to invest in expansion
  - Demand in other villages

**Franchise Mechanics:**
```
Original "Golden Goblet Inn" (Village A)
  → Owner invests 5,000 currency
  → Creates "Golden Goblet Inn - Branch" (Village B)

Branch:
  - Shares brand reputation (starts at 70% of original)
  - Separate finances, but profit-sharing agreement
    → Branch pays 20% of profits to original (franchise fee)
  - Managed by hired manager or partner
  - Can further franchise if successful (chain expansion)
```

**Economies of Scale:**
- 3+ franchises → Bulk purchasing discount (-10% material costs)
- 5+ franchises → Brand recognition bonus (+15% customer attraction)
- 10+ franchises → Market dominance (monopoly concerns)

### Migration

**Business Migration Triggers:**
- **High Village Taxes** (>40%):
  - Owner calculates: "Profit in Village A vs Village B"
  - If B offers better margins → Migrate

- **Poor Economic Conditions:**
  - Customer base shrinking (village declining)
  - Security threats (constant raids)
  - Corrupt extortion (officials demand bribes)

**Migration Mechanics:**
```
Owner decides to migrate:
  1. Sell business in current village (discount if distressed)
     OR
     Close business (lose building, keep liquid wealth)

  2. Move to target village

  3. Establish new business:
     - Purchase building or build new
     - Restart at lower level (loss of local reputation)
     - But retain owner skill/experience

  4. Rebuild customer base over time
```

**Village Impact:**
- Business exodus → Tax revenue drops → Village crisis
- Competing villages attract businesses (economic warfare)

---

## Village Economic Policies

### Nationalization

**Authoritarian Villages:**
- Automatically nationalize businesses when:
  - Owner dies with no heirs (100% chance)
  - Owner commits crime (exile → business seized)
  - Business becomes "strategic" (war production, food supply)
  - Village alignment shifts authoritarian (existing businesses seized)

**Nationalization Process:**
```
Village Council votes to nationalize:
  → Owner compensated (if lawful) or expropriated (if evil)
  → Business becomes village-owned
  → Manager appointed by council (usually elite family member)
  → Profits go to village treasury
  → No inheritance (perpetual state ownership)
```

**Elite Influence:**
- Authoritarian villages: Elites influence council
- Council "appoints" elite family members as managers
- Effectively: Elites control nationalized businesses indirectly
- Corruption: Manager embezzles profits (wealth without ownership)

### Free Market

**Egalitarian Villages:**
- Minimal interference
- Low taxes (10-15%)
- Anyone can start business
- Competition drives quality up, prices down

**Regulation:**
- Prevent monopolies (if >50% market share in one sector)
- Consumer protection (quality minimums)
- Labor laws (wage minimums, worker safety)

### Taxation Policies

**Dynamic Tax Rates:**
```
Village Alignment determines base rate:
  Egalitarian Good:      10-15%
  Peaceful Neutral:      15-20%
  Materialistic:         25-35% (greedy but not authoritarian)
  Authoritarian:         30-40%
  Corrupt Evil:          50-70% (extractive, drives businesses away)

Special Modifiers:
  War Emergency:         +10% (temporary)
  Famine Recovery:       +5%
  Economic Boom:         -5% (encourage growth)
  Business Exodus:       -10% (emergency cut to retain businesses)
```

**Tax Evasion:**
- Corrupt/Chaotic business owners may **evade** taxes
- Risk of discovery (peacekeepers investigation)
- Penalty if caught: Fine (2× owed taxes) or business seizure
- Evil/Corrupt villages: Less enforcement (bribes expected)

---

## Asset Ownership

### Resource Nodes

**Mines, Quarries, Forests:**
- Ownable by entities
- Generate extractable resources
- Owner receives:
  - Exclusive extraction rights OR
  - Lease fees from others extracting

**Example:**
```
Lord Erik owns "Iron Mine" asset
  → Employs 5 miners (wages: 400/month total)
  → Produces 200 iron ore/month
  → Sells ore to blacksmiths at 3 currency/ore = 600 revenue
  → Taxes: 120 (20%)
  → Wages: 400
  → Net: 80/month profit

  OR

  → Leases to Blacksmith Guild for 150/month
  → No labor management, pure passive income
```

**Depletion:**
- Nodes have finite reserves (long-term, 10+ years)
- Depletion visible: "Iron Mine (80% remaining)"
- Drives investment in new node discovery

### Real Estate

**Houses:**
- Ownable residences
- Generate rent if not owner-occupied
- Rent amount based on:
  - House quality (hovel vs manor)
  - Location desirability
  - Village wealth level

**Luxury Properties:**
- **Bathhouses:** High elite demand, premium rents
- **Arenas:** Ticket revenue from events
- **Estates:** Status symbols, high value, attract marriages

**Investment Strategy:**
- Wealthy families buy properties
- Rent to poor/mid-wealth villagers
- Generate passive income
- Accumulate real estate empire

### Agricultural Assets

**Wheat Fields, Vineyards, Orchards:**
- Produce food/luxury goods
- Require seasonal labor (employment)
- Owner profits from harvest sales

**Granaries/Warehouses:**
- Storage capacity
- Lease storage space to others
- Strategic: Control food supply = power

**Land Value:**
- Fertile land > poor land
- Proximity to village center > outskirts
- Irrigated > rain-dependent

---

## Employment & Wages

### Employment Contracts

**Employer (Business) offers Wage:**
```
Wage = VillageBaseWage × SkillMultiplier × Negotiation

Negotiation factors:
  - Worker education/skill (higher skill → demand higher wage)
  - Business profitability (profitable can afford to pay more)
  - Labor supply (scarce skills → premium wages)
  - Worker charisma (negotiation skill)
  - Employer wealth strata (elites pay more to signal status)
```

**Worker accepts if:**
- Wage ≥ MinimumAcceptableWage (based on wealth strata)
- Working conditions acceptable (no abuse if Good aligned)
- Employer reputation acceptable

**Contract Duration:**
- Indefinite (until fired or quit)
- Seasonal (agricultural workers)
- Contract (mercenary companies, specific projects)

### Worker Progression

**Skill Growth:**
- Workers gain experience from working
- Experience → Education increases
- Education → Higher wage demands
- Creates progression: Apprentice → Journeyman → Master

**Career Mobility:**
- Workers can:
  - Switch employers (better wages)
  - Start own business (if save enough capital)
  - Inherit employer's business
  - Join cooperative

### Unemployment

**Unemployed Villagers:**
- No wage income
- Rely on:
  - Family support (if family wealthy)
  - Charity (if Good village)
  - Begging/crime (if desperate)
- Motivation to accept any job offer

**Labor Market Dynamics:**
```
High Unemployment → Wages drop (surplus labor)
Low Unemployment → Wages rise (scarce labor)

Businesses adjust:
  High wages → Invest in automation/efficiency
  Low wages → Expand hiring, labor-intensive methods
```

---

## Integration with Existing Systems

### Wealth & Social Dynamics

✅ **Business ownership drives wealth accumulation:**
- Successful business → profits → wealth increase → climb strata
- Business failure → bankruptcy → wealth loss → drop strata

✅ **Family businesses:**
- Families pool resources to start businesses
- Family initiative drives collective business ventures
- Dynasty businesses = generational wealth

✅ **Elite dominance:**
- Ultra High families own multiple businesses
- Monopolize high-value sectors (enchanting, luxury services)
- Use political influence to protect business interests

### Guild System

✅ **Guild outsourcing:**
- **Heroes' Guild** contracts blacksmiths for weapons
- **Merchants' Guild** franchises inns/taverns
- **Scholars' Guild** sponsors schools/libraries
- **Mages' Guild** employs enchanters/wand makers

✅ **Business guilds:**
- Artisans' Guild regulates quality standards
- Merchants' Guild coordinates trade routes
- Guilds can own businesses collectively (guild halls, training facilities)

### Alignment System

✅ **Village alignment affects ownership:**
- **Authoritarian:** Nationalized economy, state ownership
- **Egalitarian:** Free market, worker cooperatives
- **Materialistic:** High taxes, elite monopolies
- **Corrupt:** Extortion, bribery, tax evasion

✅ **Individual alignment affects business ethics:**
- **Good:** Fair wages, honest pricing, charity
- **Evil:** Exploitation, price-gouging, slave labor
- **Lawful:** Contracts honored, tax compliance
- **Chaotic:** Tax evasion, contract breaches, opportunism

### Individual Progression

✅ **Skills drive business success:**
- **Education:** Better quality products
- **Intelligence:** Efficient operations, quality
- **Wisdom:** Rare products, sound decisions
- **Charisma:** Customer attraction (inns, taverns)
- **Initiative:** Proactive business decisions (expansion, migration)

✅ **Behavioral traits:**
- **Bold:** Risky investments, aggressive expansion
- **Craven:** Conservative, save profits
- **Vengeful:** Economic warfare against rivals
- **Forgiving:** Charitable support to struggling competitors

---

## DOTS Implementation Architecture

### Core Components

```csharp
namespace Godgame.Economy
{
    /// <summary>
    /// Core business entity component.
    /// Attached to building entities that host businesses.
    /// </summary>
    public struct Business : IComponentData
    {
        public enum BusinessType : byte
        {
            Blacksmith,
            Enchanter,
            Innkeeper,
            Tavern,
            Bathhouse,
            StageStation,
            Arena,
            Circus,
            MercenaryCompany,
            GladiatorService,
            ServantService,
            School,
            Library,
            WandMaker,
            Jewelcrafter,
            Runer,
            Tailor,
            Leatherworker,
            Farm,
            Orchard,
            Vineyard,
            // Extensible
        }

        public BusinessType Type;
        public FixedString64Bytes BusinessName;
        public uint FoundedTick;

        // Building linkage
        public Entity AttachedBuilding;
        public float3 Location;

        // Progression
        public byte Level;                      // 1-10
        public uint Experience;                 // Total XP
        public byte Reputation;                 // 0-100
        public byte QualityScore;               // 0-100 (maps to tier 0-5)
        public byte RarityScore;                // 0-100 (maps to tier 0-5)

        // Finances
        public int Wealth;                      // Current liquid funds
        public uint MonthlyRevenue;             // Last month's revenue
        public uint MonthlyExpenses;            // Wages + taxes + maintenance
        public int NetProfit;                   // Revenue - expenses

        // State
        public byte EmployeeCount;
        public byte EmployeeCapacity;           // Max employees (increases with level)
        public bool IsOperational;              // false if bankrupt, sieged, etc.
        public uint LastTransactionTick;
    }

    /// <summary>
    /// Ownership share component.
    /// Multiple can exist for multi-party ownership.
    /// </summary>
    public struct BusinessOwnership : IBufferElementData
    {
        public Entity OwnerEntity;              // Individual, Family, or Aggregate
        public float SharePercentage;           // 0.0-1.0 (e.g., 0.40 = 40%)
        public uint AcquiredTick;
        public byte OwnershipType;              // 0=Purchased, 1=Inherited, 2=Founded, 3=Expropriated, 4=Gift
    }

    /// <summary>
    /// Employment relationship.
    /// Workers employed by business.
    /// </summary>
    public struct BusinessEmployee : IBufferElementData
    {
        public Entity VillagerEntity;
        public uint HiredTick;
        public ushort MonthlyWage;              // Currency per month
        public byte SkillLevel;                 // 0=Unskilled, 1=Skilled, 2=Expert, 3=Master
        public uint ExperienceGained;           // XP earned while working here
        public bool IsSenior;                   // Eligible for inheritance if no family heir
    }

    /// <summary>
    /// Transaction history for business.
    /// </summary>
    public struct BusinessTransaction : IBufferElementData
    {
        public uint Tick;
        public Entity CustomerEntity;           // Who bought (can be null for aggregate sales)
        public ushort TransactionValue;         // Currency amount
        public byte ProductQualityTier;         // Quality at time of sale
        public byte ProductRarityTier;          // Rarity at time of sale
    }

    /// <summary>
    /// Franchise relationship.
    /// Original business can have multiple franchise branches.
    /// </summary>
    public struct BusinessFranchise : IBufferElementData
    {
        public Entity FranchiseBranchEntity;    // Child franchise business
        public Entity VillageEntity;            // Where branch is located
        public uint EstablishedTick;
        public float FranchiseFeePercentage;    // 0.0-1.0 (e.g., 0.20 = 20% of branch profits)
        public uint TotalFeesCollected;
    }

    /// <summary>
    /// Asset ownership component.
    /// For resource nodes, real estate, agricultural land.
    /// </summary>
    public struct Asset : IComponentData
    {
        public enum AssetType : byte
        {
            IronMine,
            StoneQuarry,
            ForestTimber,
            GoldMine,
            House,
            Manor,
            Estate,
            Bathhouse,
            Arena,
            WheatField,
            Vineyard,
            Orchard,
            Granary,
            Warehouse,
            // Extensible
        }

        public AssetType Type;
        public FixedString64Bytes AssetName;

        // Value
        public uint MarketValue;                // Current estimated worth
        public uint AcquisitionCost;            // What owner paid

        // Income generation
        public bool GeneratesIncome;            // Passive income (rent, leases)
        public ushort MonthlyIncome;            // If leased/rented
        public ushort MonthlyExpenses;          // Maintenance, taxes

        // Resource nodes
        public uint ResourceReserves;           // Remaining extractable resources (if applicable)
        public ushort ExtractionRate;           // Per month
    }

    /// <summary>
    /// Asset ownership (can be multi-party like businesses).
    /// </summary>
    public struct AssetOwnership : IBufferElementData
    {
        public Entity OwnerEntity;
        public float SharePercentage;
        public uint AcquiredTick;
    }

    /// <summary>
    /// Business bankruptcy state.
    /// Tracks negative wealth duration.
    /// </summary>
    public struct BusinessBankruptcy : IComponentData
    {
        public bool IsInsolvent;                // Wealth < 0
        public uint InsolvencySinceTick;        // When went negative
        public uint BankruptcyThresholdTicks;   // 1 year = death
        public int DebtAmount;                  // How much negative
    }

    /// <summary>
    /// Business support/transfusion record.
    /// Tracks donations to keep business alive.
    /// </summary>
    public struct BusinessSupport : IBufferElementData
    {
        public Entity DonorEntity;
        public uint DonationAmount;
        public uint DonationTick;
        public FixedString32Bytes Reason;       // "family_support", "investment", "charity"
    }

    /// <summary>
    /// Outsourcing contract.
    /// Aggregate entities contract businesses for services.
    /// </summary>
    public struct OutsourcingContract : IBufferElementData
    {
        public Entity ClientEntity;             // Who hired (band, guild, family)
        public Entity BusinessEntity;           // Who provides service
        public FixedString64Bytes ServiceType;  // "weapons", "repairs", "mercenaries"
        public uint ContractValue;              // Total payment
        public uint ContractStartTick;
        public uint ContractDeadlineTick;
        public bool IsCompleted;
        public bool IsPaid;
    }

    /// <summary>
    /// Economic policy component for villages.
    /// Determines taxes, nationalization, regulation.
    /// </summary>
    public struct VillageEconomicPolicy : IComponentData
    {
        public float BusinessTaxRate;           // 0.0-1.0 (e.g., 0.20 = 20%)
        public float NationalizationThreshold;  // 0.0-1.0 (authoritarian = 0.7+)
        public bool AllowPrivateOwnership;      // Egalitarian = true, Authoritarian = false
        public bool AllowSlaveBusinesses;       // Evil alignment only
        public bool EnforceQualityStandards;    // Lawful villages
        public byte MonopolyThreshold;          // % market share triggering regulation (50)
    }

    /// <summary>
    /// Business migration intent.
    /// Business considering moving to another village.
    /// </summary>
    public struct BusinessMigrationIntent : IComponentData
    {
        public bool IntendToMigrate;
        public Entity TargetVillageEntity;
        public uint MigrationPlannedTick;
        public FixedString64Bytes Reason;       // "high_taxes", "declining_market", "security"
        public float ProfitDifferential;        // Expected profit gain in target village
    }

    /// <summary>
    /// Inheritance configuration.
    /// Defines business succession rules.
    /// </summary>
    public struct BusinessInheritance : IComponentData
    {
        public enum SuccessionType : byte
        {
            Family,                // Default: family inheritance
            Partnership,           // Partners buy out
            Employee,              // Senior employee inherits
            Nationalize,           // Village seizes
            Liquidate              // Dissolve and sell assets
        }

        public SuccessionType PreferredSuccession;
        public Entity DesignatedHeir;           // Optional: owner can designate specific heir
        public bool AllowEmployeeInheritance;   // If true, employees eligible if no family
    }
}
```

---

## System Flow Examples

### Business Creation Flow

```csharp
1. Entity (Individual/Family) decides to start business
   → Has sufficient capital (BusinessStartupCost)
   → Finds available building OR constructs new building

2. Create Business entity:
   BusinessEntity = ecb.CreateEntity()
   ecb.AddComponent(BusinessEntity, new Business {
       Type = BusinessType.Blacksmith,
       BusinessName = "Erik's Forge",
       Level = 1,
       Reputation = 50,
       Wealth = StartupCapital,
       AttachedBuilding = BuildingEntity
   })

3. Add ownership:
   ecb.AddBuffer<BusinessOwnership>(BusinessEntity)
   ownershipBuffer.Add(new BusinessOwnership {
       OwnerEntity = ErikEntity,
       SharePercentage = 1.0f,
       OwnershipType = 2 // Founded
   })

4. Hire initial employees (if any):
   employeeBuffer.Add(new BusinessEmployee {
       VillagerEntity = ApprenticeEntity,
       MonthlyWage = 50,
       SkillLevel = 0
   })

5. Business begins operations:
   → Generates transactions
   → Accumulates experience
   → Pays wages and taxes
   → Owner receives profits
```

### Transaction Processing Flow

```csharp
[BurstCompile]
public partial struct BusinessTransactionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (business, transactions, employees, entity) in SystemAPI
            .Query<RefRW<Business>, DynamicBuffer<BusinessTransaction>, DynamicBuffer<BusinessEmployee>>()
            .WithEntityAccess())
        {
            // Simulate customer transactions based on business parameters
            int customerCount = CalculateCustomerVolume(business.ValueRO);

            for (int i = 0; i < customerCount; i++)
            {
                // Calculate transaction value
                float basePrice = GetBasePrice(business.ValueRO.Type);
                float qualityMult = GetQualityMultiplier(business.ValueRO.QualityScore);
                float rarityMult = GetRarityMultiplier(business.ValueRO.RarityScore);
                float reputationMult = GetReputationMultiplier(business.ValueRO.Reputation);

                ushort transactionValue = (ushort)(basePrice × qualityMult × rarityMult × reputationMult);

                // Record transaction
                transactions.Add(new BusinessTransaction {
                    Tick = currentTick,
                    TransactionValue = transactionValue,
                    ProductQualityTier = GetQualityTier(business.ValueRO.QualityScore),
                    ProductRarityTier = GetRarityTier(business.ValueRO.RarityScore)
                });

                // Add to revenue
                business.ValueRW.MonthlyRevenue += transactionValue;

                // Gain experience
                business.ValueRW.Experience += (uint)(transactionValue × GetQualityTier(business.ValueRO.QualityScore));

                // Reputation gain
                if (GetQualityTier(business.ValueRO.QualityScore) >= 2)
                {
                    business.ValueRW.Reputation = (byte)math.min(100, business.ValueRO.Reputation + 1);
                }
            }

            // Check for level up
            if (business.ValueRO.Experience >= GetLevelThreshold(business.ValueRO.Level))
            {
                business.ValueRW.Level++;
                business.ValueRW.EmployeeCapacity++;
            }
        }
    }
}
```

### Monthly Financial Settlement

```csharp
[BurstCompile]
public partial struct BusinessFinancialSystem : ISystem
{
    // Runs once per in-game month

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (business, employees, ownerships, entity) in SystemAPI
            .Query<RefRW<Business>, DynamicBuffer<BusinessEmployee>, DynamicBuffer<BusinessOwnership>>()
            .WithEntityAccess())
        {
            // Calculate expenses
            uint totalWages = 0;
            foreach (var employee in employees)
            {
                totalWages += employee.MonthlyWage;
            }

            // Get village tax rate
            Entity villageEntity = GetVillageForBusiness(state, business.ValueRO.Location);
            var policy = state.EntityManager.GetComponentData<VillageEconomicPolicy>(villageEntity);
            uint taxes = (uint)(business.ValueRO.MonthlyRevenue × policy.BusinessTaxRate);

            uint maintenance = CalculateMaintenance(business.ValueRO);

            business.ValueRW.MonthlyExpenses = totalWages + taxes + maintenance;
            business.ValueRW.NetProfit = (int)(business.ValueRO.MonthlyRevenue - business.ValueRW.MonthlyExpenses);

            // Update wealth
            business.ValueRW.Wealth += business.ValueRO.NetProfit;

            // Distribute profits to owners
            if (business.ValueRO.NetProfit > 0)
            {
                foreach (var ownership in ownerships)
                {
                    int ownerProfit = (int)(business.ValueRO.NetProfit × ownership.SharePercentage);

                    // Add to owner wealth
                    AddWealthToEntity(state, ownership.OwnerEntity, ownerProfit);
                }
            }

            // Check bankruptcy
            if (business.ValueRO.Wealth < 0)
            {
                if (!state.EntityManager.HasComponent<BusinessBankruptcy>(entity))
                {
                    ecb.AddComponent(entity, new BusinessBankruptcy {
                        IsInsolvent = true,
                        InsolvencySinceTick = currentTick,
                        DebtAmount = -business.ValueRO.Wealth
                    });
                }
            }
            else
            {
                // Recovered from bankruptcy
                if (state.EntityManager.HasComponent<BusinessBankruptcy>(entity))
                {
                    ecb.RemoveComponent<BusinessBankruptcy>(entity);
                }
            }

            // Reset monthly counters
            business.ValueRW.MonthlyRevenue = 0;
            business.ValueRW.MonthlyExpenses = 0;
        }
    }
}
```

### Inheritance/Succession Flow

```csharp
[BurstCompile]
public partial struct BusinessInheritanceSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Triggered when business owner dies

        foreach (var (business, ownerships, employees, inheritance, entity) in SystemAPI
            .Query<RefRO<Business>, DynamicBuffer<BusinessOwnership>, DynamicBuffer<BusinessEmployee>, RefRO<BusinessInheritance>>()
            .WithEntityAccess())
        {
            foreach (var ownership in ownerships)
            {
                // Check if owner is dead
                if (!state.EntityManager.Exists(ownership.OwnerEntity) || IsEntityDead(state, ownership.OwnerEntity))
                {
                    // Owner died, trigger succession
                    Entity newOwner = DetermineSuccessor(state, ownership.OwnerEntity, business.ValueRO, employees, inheritance.ValueRO);

                    if (newOwner != Entity.Null)
                    {
                        // Transfer ownership
                        ownership.OwnerEntity = newOwner;
                        ownership.AcquiredTick = currentTick;
                        ownership.OwnershipType = 1; // Inherited
                    }
                    else
                    {
                        // No suitable heir → Nationalize or liquidate
                        Entity villageEntity = GetVillageForBusiness(state, business.ValueRO.Location);
                        var policy = state.EntityManager.GetComponentData<VillageEconomicPolicy>(villageEntity);

                        if (policy.NationalizationThreshold > 0.5f)
                        {
                            // Nationalize
                            ownership.OwnerEntity = villageEntity;
                            ownership.SharePercentage = 1.0f;
                            ownership.OwnershipType = 3; // Expropriated
                        }
                        else
                        {
                            // Liquidate business
                            ecb.DestroyEntity(entity);
                        }
                    }
                }
            }
        }
    }

    private Entity DetermineSuccessor(ref SystemState state, Entity deceasedOwner, Business business, DynamicBuffer<BusinessEmployee> employees, BusinessInheritance inheritance)
    {
        // Priority 1: Designated heir
        if (inheritance.DesignatedHeir != Entity.Null && state.EntityManager.Exists(inheritance.DesignatedHeir))
        {
            return inheritance.DesignatedHeir;
        }

        // Priority 2: Family members
        Entity familyHeir = FindFamilyHeir(state, deceasedOwner, business.Type);
        if (familyHeir != Entity.Null)
            return familyHeir;

        // Priority 3: Business partners (other owners)
        // (Skip if sole proprietor)

        // Priority 4: Senior employee
        if (inheritance.AllowEmployeeInheritance)
        {
            Entity seniorEmployee = FindSeniorEmployee(employees);
            if (seniorEmployee != Entity.Null)
                return seniorEmployee;
        }

        // No successor
        return Entity.Null;
    }
}
```

---

## Open Questions & Design Decisions

### Critical

1. **Business density per village?** How many businesses per 100 villagers?
   - Too many: Dilutes customer base, all businesses struggle
   - Too few: Limited economic diversity
   - **PROPOSAL:** 1 business per 20 villagers at mid-tier (5 businesses for 100 pop)

2. **Startup capital requirements?** How much wealth to start business?
   - **PROPOSAL:**
     - Low-tier (farm, tavern): 500 currency (mid-wealth can afford)
     - Mid-tier (blacksmith, inn): 2,000 currency (high-wealth)
     - High-tier (enchanter, arena): 10,000 currency (ultra-high only)

3. **Bankruptcy duration?** 1 year too long or too short?
   - 1 year (12 in-game months) seems reasonable for recovery attempts
   - Allows family/friends time to support
   - **CONFIRM:** 1 year = 12 months of negative wealth before death

4. **Multi-party ownership voting?** How do partners make decisions?
   - **PROPOSAL:** Majority share controls decisions
   - 50/50 split requires consensus (deadlock possible)

### Important

5. **Franchise profit-sharing?** What % of branch profits go to original?
   - **PROPOSAL:** 15-25% (negotiated based on brand value)

6. **Migration cost?** How expensive to move business to new village?
   - Lose building investment (unless sold)
   - Restart reputation at 50% of original (brand recognition travels partially)
   - **PROPOSAL:** ~30% wealth loss from migration

7. **Employee loyalty?** Do workers stay during bankruptcy?
   - **PROPOSAL:**
     - If wages unpaid for 2 months → 50% chance quit
     - If wages unpaid for 4 months → 90% chance quit
     - Good-aligned workers more loyal (+20% retention)

8. **Quality/Rarity caps?** Can any business reach legendary tier?
   - **PROPOSAL:** Quality capped by employee skill ceiling
   - Legendary quality (96-100) requires Master employees (Education 90+)
   - Rarity capped by Wisdom (need Wisdom 95+ for legendary rarity)

### Nice to Have

9. **Business marriages?** Do business empires merge via arranged marriages?
   - **YES:** Families arrange marriages to merge businesses
   - Creates dynasty business conglomerates

10. **Hostile takeovers?** Can evil entities force buyouts?
    - **YES:** Corrupt/Evil entities can:
      - Sabotage competitors (lower quality/reputation)
      - Buy distressed businesses at discounts
      - Extort ownership shares via blackmail

11. **Cooperatives vs corporations?** Different ownership models?
    - **YES:**
      - Cooperative: Equal shares among workers (egalitarian)
      - Corporation: Share-based, traded ownership (materialistic)
      - Family business: Dynastic continuity (traditional)

12. **Business wars?** Can businesses directly combat each other?
    - Economic warfare: Price-cutting, poaching employees, sabotage
    - Physical warfare: Mercenary businesses hire each other to destroy competitors

---

## Failure Modes

**Death Spiral (Business Bankruptcy):**
- Poor quality → Fewer customers → Low revenue → Can't pay wages → Workers leave → Quality drops further → Bankruptcy
- **Recovery:**
  - External support (family donations, investor transfusion)
  - Takeover by competent owner
  - Price cuts to attract customers (temporary loss for long-term recovery)

**Death Spiral (Village Tax Exodus):**
- High taxes → Businesses migrate → Tax base shrinks → Village raises taxes → More migration → Economic collapse
- **Recovery:**
  - Village cuts taxes (emergency policy shift)
  - Nationalize remaining businesses (authoritarian response)
  - Attract new businesses (tax holidays, incentives)

**Stagnation (Monopoly):**
- Single business dominates sector → No competition → Quality stagnates → Prices high → Customer dissatisfaction
- **Recovery:**
  - Village regulation (break up monopoly)
  - New competitor enters (entrepreneurial villager)
  - Guild intervention (Merchants' Guild enforces competition)

**Runaway (Franchise Explosion):**
- Successful business franchises rapidly → 20+ branches → Saturates market → Brand dilution → Quality drops
- **Recovery:**
  - Market saturation self-corrects (unprofitable branches close)
  - Franchise fees support struggling branches
  - Reputation penalties for over-expansion

---

## Player Interaction

**Observable:**
- Business buildings have visible signage (names, quality indicators)
- Busy businesses have customer traffic (visual activity)
- Bankrupt businesses show decay (broken signs, empty)
- Franchise chains visible across villages (brand recognition)

**Control Points:**
- **Indirect:** Player miracles affect village economy (prosperity → more customers)
- **Indirect:** Player influences village alignment → Economic policies
- **Indirect:** Player education miracles → Skilled workforce → Better businesses
- **Direct (potential):** Player can bless/curse specific businesses (reputation boost/penalty)

**Learning Curve:**
- **Beginner:** Notices businesses exist, provides jobs
- **Intermediate:** Understands wealth generation from business ownership
- **Expert:** Manipulates village policies to favor certain business models, creates business dynasties

---

## Iteration Plan

**v1.0 (MVP):**
- ✅ Basic business entities attached to buildings
- ✅ Single-owner businesses (no multi-party yet)
- ✅ Simple revenue/expense/profit tracking
- ✅ Employee wages
- ✅ Village taxes (fixed rate)
- ✅ Inheritance (family priority)
- ✅ Bankruptcy (1 year negative → death)
- ❌ No franchising
- ❌ No migration
- ❌ No outsourcing contracts
- ❌ Limited business types (blacksmith, inn, farm only)

**v2.0 (Enhanced):**
- ✅ Multi-party ownership (shares)
- ✅ Franchising system
- ✅ Business migration
- ✅ Asset ownership (mines, real estate)
- ✅ Outsourcing contracts (bands hire blacksmiths)
- ✅ Dynamic village tax rates (alignment-based)
- ✅ Quality/rarity progression
- ✅ All business types implemented

**v3.0 (Complete):**
- ✅ Economic warfare (sabotage, hostile takeovers)
- ✅ Cooperatives and alternative ownership models
- ✅ Advanced AI (business owners make strategic decisions)
- ✅ Cross-village business networks
- ✅ Guild business ownership
- ✅ Player deity business blessings/curses

---

## Related Documentation

- **Entity Relations:** [Entity_Relations_And_Interactions.md](../Villagers/Entity_Relations_And_Interactions.md) - Partnership relations, business rivalries
- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](../Villagers/Wealth_And_Social_Dynamics.md) - Wealth strata, families, dynasties
- **Guild System:** [Guild_System.md](../Villagers/Guild_System.md) - Guild outsourcing, guild-owned businesses
- **Alignment Framework:** [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Village economic policies
- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Skills affecting business quality
- **Sandbox Villages:** [Sandbox_Autonomous_Villages.md](../Core/Sandbox_Autonomous_Villages.md) - Initiative, village culture

---

**For Designers:** Focus on tuning profitability curves and bankruptcy thresholds to create meaningful economic drama without frustrating business death spirals.

**For Implementers:** Start with simple single-owner businesses and basic revenue/expense tracking. Franchising and multi-party ownership can be layered on top once foundation is stable.

**For Narrative:** Business inheritance, bankruptcies, hostile takeovers, and franchise empires create rich emergent economic stories tied to family dynasties and village politics.

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core vision captured, ready for technical specification and prototyping
