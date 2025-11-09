# Wealth Strata & Social Dynamics

**Status:** Draft - Concept Design
**Category:** Social Hierarchy & Economic Dynamics
**Scope:** Individual, Family, and Dynasty Aggregate Entities
**Created:** 2025-11-06
**Last Updated:** 2025-11-06

---

## Purpose

**Primary Goal:** Create emergent social stratification and family-based power structures where wealth, alignment, and behavioral traits drive inter-personal dynamics ranging from charitable solidarity to ruthless opportunism.

**Core Innovation:** Families and dynasties are aggregate entities with their own initiative, ambitions, and behavioral profiles that compete with (or align with) individual member desires, creating complex decision-making scenarios.

---

## Wealth Strata System

### Five Tiers

```
Ultra High (Elite)
    ↓
  High (Wealthy)
    ↓
   Mid (Average)
    ↓
  Poor (Struggling)
    ↓
Ultra Poor (Destitute)
```

### Tier Definitions

#### Ultra Poor (Destitute)
**Economic State:**
- No fixed housing
- Street beggars or dispossessed
- Living in wilds without parent village
- Zero or negative wealth (<-100 currency)
- Relies entirely on charity or scavenging

**Social Position:**
- Outside formal village structure
- No political representation
- No family ties (disowned, orphaned, exiled)
- Migration targets: any village accepting refugees

**Behaviors:**
- Begging at village gates
- Scavenging resource nodes
- Petty theft (if desperate + chaotic)
- Petition deity for miracles (survival prayers)

---

#### Poor (Struggling)
**Economic State:**
- Living in hovels or shared tenements
- Subsistence income (0-500 currency)
- Manual labor jobs (gatherers, haulers)
- Minimal assets (clothes, basic tools)

**Social Position:**
- Village residents, but no political power
- Extended families may pool resources
- Vulnerable to eviction if village contracts
- Migration targets: wealthier villages offering jobs

**Behaviors:**
- Work assigned jobs reliably
- Accept charity from wealthier neighbors
- Petition for basic needs (food, shelter)
- Form mutual aid networks with other poor

---

#### Mid (Average)
**Economic State:**
- Standard village housing
- Comfortable income (500-2000 currency)
- Skilled trades or mid-tier jobs (smiths, bakers, guards)
- Modest assets (home, workshop, savings)
- **Baseline depends on village wealth** (rich village mid > poor village mid)

**Social Position:**
- Majority of village population
- Eligible for minor civic roles (guild membership)
- Family units own property
- Stable generational continuity

**Behaviors:**
- Maintain stable households
- Save for family advancement
- Participate in festivals and civic life
- Occasional charity if good-aligned

---

#### High (Wealthy)
**Economic State:**
- Manors or large estates
- Substantial income (2000-10,000 currency)
- Business ownership, large holdings
- Extensive assets (multiple properties, workshops, investments)
- Usually higher education/intelligence/wisdom

**Social Position:**
- Prominent families
- Hold guild leadership or minor council seats
- Dynastic ambitions emerging
- Marriage alliances with other high families

**Behaviors:**
- Invest in businesses and properties
- Sponsor education for children
- Political maneuvering for influence
- Conditional charity (if good-aligned)
- Wealth defense (if corrupt/evil)

---

#### Ultra High (Elite)
**Economic State:**
- Multiple estates, vast holdings
- Dominant income (>10,000 currency)
- Control major businesses and trade routes
- Generational wealth, rare to lose tier
- Hold village council positions (familial representation)

**Social Position:**
- Ruling class
- Family dynasties govern village
- Marriage alliances across villages
- When they fall: **seismic social shock**

**Behaviors:**
- Govern village policy (through council)
- Maintain dynastic power structures
- Broker inter-village alliances
- Disown/disinherit failures or disgraced members
- If corrupt: ruthless power consolidation
- If pure/good: noblesse oblige (patronage, charity)

---

## Social Cohesion vs Opportunism

### Good + Pure Societies (Solidarity)

**Core Mechanic:** Neighbors support each other during hardship.

#### Donation Triggers

When a villager suffers major loss:
- Death of loved one
- Business failure
- Wealth strata drop
- Injury/disability
- House destroyed

**Donation Calculation:**
```
BaseDonation = Donor.Wealth × 0.05  // 5% of wealth

Modifiers:
  IF Donor.Alignment = Good:          × 1.5
  IF Donor.Purity = Pure:             × 1.3
  IF Donor.Alignment = Neutral:       × 1.0
  IF Donor.Alignment = Evil:          × 0.1 (token gesture only)

  IF Donor.Corrupt > +40:             × 0.5

  Outlook Modifiers:
    IF Victim.Warlike AND (son died in battle):  × 2.0
    IF Victim.Peaceful:                          × 1.2
    IF Victim.Materialistic:                     × 0.7
    IF Victim.Spiritual:                         × 1.3

  Relationship Modifiers:
    IF SameFamily:                               × 3.0
    IF SameDynasty:                              × 1.8
    IF Friend:                                   × 1.5
    IF Stranger:                                 × 1.0
    IF Rival:                                    × 0.2

Final Donation = BaseDonation × All Modifiers
```

**Example:**
```
Wealthy Good Pure villager (5000 wealth) hears of warlike neighbor's son dying in battle:
  Base: 5000 × 0.05 = 250
  Good: × 1.5 = 375
  Pure: × 1.3 = 487
  Warlike son death: × 2.0 = 974

  → Donates 974 currency to grieving family
```

---

### Evil + Corrupt Societies (Opportunism)

**Core Mechanic:** Neighbors actively undermine each other to climb or maintain position.

#### Undermining Actions

When a villager shows weakness:
- Wealth drop triggers predatory responses
- Business failure → competitors buy assets cheap
- Personal tragedy → rivals spread rumors
- Political misstep → opponents capitalize

**Undermining Calculation:**
```
IF Village.AverageAlignment = Evil AND Village.AverageCorruption > +30:

  When target shows weakness:
    Opportunists (corrupt/evil entities) roll initiative to:
      1. Spread damaging rumors (-5 to -20 reputation)
      2. Poach business clients/workers
      3. Buy distressed assets at 30-50% discount
      4. Form coalitions to exclude target
      5. Petition for target's council removal

  Initiate undermining IF:
    Opportunist.Initiative > 0.60 (acts quickly)
    Opportunist.Corrupt > +40 (morally willing)
    Opportunist.Evil < -30 (lacks empathy)
    Target.WealthStrata >= Opportunist.WealthStrata (threat or opportunity)
```

**Example:**
```
Corrupt Evil mid-wealth merchant sees rival's business failing:
  Initiative: 0.72 (high - acts fast)
  Corruption: +65 (very corrupt)
  Evil: -50 (quite evil)

  → Rolls to undermine:
    1. Spreads rumor rival is bad with money (-15 reputation)
    2. Offers rival's best worker higher wages (poaches talent)
    3. Buys rival's workshop at 40% discount when desperate

  Result: Rival drops to poor tier, merchant rises to high tier
```

---

## Families & Dynasties as Aggregate Entities

### Definitions

**Family** = Close relations
- Parents + children
- Married couples
- Siblings under same household
- Those under direct authority of family head

**Dynasty** = Broader bloodline
- Multiple family branches
- Shared ancestry (grandfather → sons → grandsons)
- Familial representation on councils
- Genetic inheritance scope

**Example Structure:**
```
King Aldric Dynasty (Ultra High)
  ├─ Royal Family (King Aldric's household)
  │   ├─ King Aldric (head)
  │   ├─ Queen Mira
  │   ├─ Prince Kael (heir)
  │   └─ Princess Lyra
  │
  ├─ Duke Theron's Family (Aldric's brother)
  │   ├─ Duke Theron (family head, under Aldric's dynasty)
  │   ├─ Duchess Sara
  │   └─ Lord Erik (son)
  │
  └─ Baroness Elira's Family (Aldric's sister)
      ├─ Baroness Elira (family head, under Aldric's dynasty)
      └─ Lady Nessa (daughter)
```

---

### Aggregate Entity Stats

Families and dynasties compute averaged values from members:

```csharp
FamilyAggregate : IComponentData {
    Entity HeadOfFamily;            // Leader whose stats bias average

    // Behavioral averages (weighted by head 2x)
    float AverageVengeful;
    float AverageBold;

    // Alignment averages
    float AverageGoodEvil;
    float AverageLawfulChaotic;
    float AveragePureCorrupt;

    // Outlook profile (dominant outlook wins)
    byte DominantOutlookFlags;      // Warlike, Materialistic, etc.

    // Initiative & ambitions
    float FamilyInitiative;         // Averaged from members
    FixedString32Bytes PrimaryDesire;   // "wealth", "power", "honor"
    FixedString32Bytes PrimaryAmbition; // "rulership", "expansion", "prestige"

    // Economic state
    int TotalFamilyWealth;
    byte WealthStrata;              // Lowest member determines family tier

    // Social cohesion
    float InternalCohesion;         // 0-1: united vs fractured
    byte DisownedMemberCount;       // Exiled members
}

DynastyAggregate : IComponentData {
    Entity DynastyHead;             // Patriarch/Matriarch

    // Same stats as Family, but broader scope
    // Includes all family branches

    // Dynastic power
    byte CouncilSeatsHeld;          // Political representation
    int TotalAssets;                // All dynasty holdings

    // Legacy tracking
    uint FoundedTick;               // Dynasty age
    FixedString64Bytes DynastyName; // "House Aldric"
}
```

---

### Family/Dynasty Initiative

**Collective Action Rate:** Families and dynasties act on their ambitions at rates determined by averaged initiative.

#### Initiative Calculation

```
FamilyInitiative =
  (HeadOfFamily.Initiative × 2.0  +  // Head weighs 2x
   Σ(Member.Initiative) / MemberCount) / 3.0

Example:
  Head (Kael): 0.80
  Wife (Mira): 0.55
  Son (Erik): 0.70
  Daughter (Lyra): 0.60

  → (0.80 × 2.0 + 0.55 + 0.70 + 0.60) / 3.0
  → (1.60 + 1.85) / 3.0
  → 1.15 (clamped to max 1.0)
  → Family Initiative: 0.90 (very active family)
```

#### Collective vs Independent Action

**Lawful Families (average lawful > +30):**
- Act **collectively** toward family goals
- Pool resources
- Coordinate business ventures
- Unified political strategy

**Chaotic Families (average chaotic < -30):**
- Act **independently** toward family goals
- Compete internally for resources
- Multiple business ventures (uncoordinated)
- Fractured political alliances

**Neutral Families:**
- Mix of collective and independent action
- Cooperation on major decisions
- Individual autonomy for minor pursuits

---

### Desires & Ambitions

Families/dynasties have primary desires and ambitions that drive collective initiative:

#### Primary Desires (What They Want)
- **Wealth:** Acquire currency, assets, businesses
- **Power:** Political influence, council seats, governance
- **Honor:** Reputation, glory, prestige
- **Safety:** Security, fortification, stability
- **Knowledge:** Education, research, wisdom
- **Faith:** Worship, temples, divine favor
- **Expansion:** Territory, migration, new settlements

#### Primary Ambitions (How They Pursue It)
- **Rulership:** Gain council seats, titles, governance positions
- **Expansion:** Found new villages, colonize territories
- **Prestige:** Win competitions, host festivals, build monuments
- **Propagation:** Spread family across villages via marriage/migration
- **Domination:** Eliminate rival families, consolidate power
- **Enlightenment:** Build schools, academies, libraries
- **Devotion:** Build temples, sponsor pilgrimages

---

### Individual vs Aggregate Conflict Resolution

**Core Challenge:** What happens when individual desires conflict with family/dynasty ambitions?

#### Resolution Framework

Individual has:
- Personal vengeance desire (from grudge)
- Personal ambition for justice

Family has:
- Desire for power
- Ambition of propagation (spread influence worldwide)

Dynasty has:
- Desire for honor
- Ambition for titleships (gain governance positions)

**Resolution Roll (each initiative cycle):**

```
Individual rolls weighted table based on outlook:

IF Outlook = Peaceful:
  → Check against vengeance once per year (slow)
  → If fails: pursue family/dynasty goals instead

IF Outlook = Warlike:
  → Pursue power (family) + honor (dynasty) first
  → Then seek vengeance after time period
  → If Vengeful > +60: prioritize vengeance sooner

IF Outlook = Spiritual:
  → Roll toward propagation (family ambition)
  → Seek breeding opportunities in other villages
  → Vengeance deferred (spiritual forgiveness)

IF Outlook = Materialistic OR Authoritarian:
  → Roll toward titleship (dynasty ambition)
  → Engage in politicking and maneuvering
  → Vengeance secondary to advancement

Final Action = Weighted probability:
  PersonalDesire × PersonalInitiative × 0.4
  FamilyDesire × FamilyInitiative × 0.3
  DynastyDesire × DynastyInitiative × 0.3

  Outlook bias shifts weights:
    Individualist: Personal +0.2
    Collectivist: Family/Dynasty +0.2
    Chaotic: Personal +0.15
    Lawful: Family/Dynasty +0.15
```

**Example Scenario:**

**Villager Erik:**
- Personal: Vengeance (-70 vengeful), Justice ambition
- Family (House Theron): Desire for Power, Ambition of Propagation
- Dynasty (House Aldric): Desire for Honor, Ambition of Titleships
- Outlook: Warlike + Authoritarian

**Each initiative cycle (every ~5 days):**
```
Roll weights:
  Vengeance: 0.70 × 0.40 = 0.28
  Power/Propagation (family): 0.60 × 0.30 = 0.18
  Honor/Titleships (dynasty): 0.75 × 0.30 = 0.225

  Warlike bias: +0.1 to vengeance
  Authoritarian bias: +0.1 to titleships

  Final weights:
    Vengeance: 0.38
    Family goals: 0.18
    Dynasty goals: 0.325

Weighted random roll → 55% chance pursues titleship politicking
                     → 25% chance pursues vengeance
                     → 20% chance works on family power/propagation
```

**Over time:** Erik spends most effort on dynasty titleship goals (authoritarian + warlike lean), occasionally acts on personal vengeance, sometimes helps family expand influence. Creates emergent narrative where he's a politician who occasionally breaks character to pursue grudges.

---

## Wealth Strata Mobility

### Upward Mobility

**Pathways:**
1. **Meritocratic (Good/Neutral villages):**
   - Successful business ventures
   - Education → high-paying jobs
   - Military glory → rewards
   - Divine favor (miracles granting wealth)

2. **Opportunistic (Evil/Corrupt villages):**
   - Undermine competitors
   - Acquire distressed assets
   - Political manipulation
   - Inheritance via suspicious deaths

**Family Boost:**
- High/Ultra High families invest in children's education
- Business opportunities passed down
- Marriage alliances secure wealth
- Initiative-driven collective ventures

**Initiative Modifier:**
```
Upward mobility chance =
  BaseChance (0.05 per year)
  × (1.0 + Initiative)
  × OutlookMultiplier
  × FamilySupportMultiplier

Example:
  Mid wealth villager, Initiative 0.70, Materialistic outlook, High family support
  → 0.05 × 1.70 × 1.5 × 2.0 = 0.255 = 25.5% chance per year
```

---

### Downward Mobility

**Triggers:**
1. Death of breadwinner
2. Business failure
3. Political disgrace
4. Divine punishment (deity curses)
5. War losses
6. Chronic illness/injury

**Protection:**
- High/Ultra High: Family wealth cushions falls (rarely drop more than 1 tier)
- Mid: Some family support, may drop to Poor
- Poor: No safety net, may become Ultra Poor
- Good/Pure villages: Charity slows descent

**Seismic Shock (Ultra High Falls):**
```
When Ultra High family drops to High or below:
  → Village stability -20
  → Rival families rush to fill power vacuum
  → Political crisis (council seat vacant)
  → Marriage alliances questioned
  → Dependent businesses/clients panic

  IF Good/Pure village:
    → Community support tries to stabilize family

  IF Evil/Corrupt village:
    → Predators strip remaining assets
    → Family may be exiled/destroyed entirely
```

---

## Disinheritance & Disowning

### Triggers

Corrupt/Pure family/dynasty heads may disown members who:

**Pure Heads Disown:**
- Members who become too corrupt (Corruption > +60)
- Criminal acts bringing shame (murder, theft, betrayal)
- Alignment drift too far (Pure Good → Evil)
- Breaking sacred oaths or family codes

**Corrupt Heads Disown:**
- Members who become too pure (interferes with schemes)
- Failure to advance family power
- Public scandals exposing family crimes
- Disobedience to head's authority

**Lawful Heads Disown:**
- Breaking family law/traditions
- Refusing arranged marriages
- Abandoning family businesses

**Chaotic Heads Disown:**
- Rare (chaotic families more tolerant)
- Only extreme failures (cowardice in battle, betrayal)

### Disowning Mechanics

```csharp
VillagerFamilyStatus : IComponentData {
    Entity CurrentFamily;           // Family aggregate entity
    Entity CurrentDynasty;          // Dynasty aggregate entity

    byte FamilyStanding;            // 0-100: head's favor
    byte DynastyStanding;           // 0-100: dynasty head's favor

    byte Disinherited;              // 1 = excluded from inheritance
    byte Disowned;                  // 1 = expelled from family
}
```

**Process:**
1. Member's actions trigger standing drops
2. If standing < 20 AND head.Initiative high: disowning check
3. Head rolls weighted by alignment/corruption
4. If disowned:
   - Remove from family aggregate
   - Zero inheritance rights
   - Lose family business access
   - Wealth strata may drop (lost support)
   - Narrative event: "Exiled from House [Name]"

**Consequences:**
- Social stigma (reputation -30)
- Political allies distance themselves
- Former family may actively oppose
- Opportunity for redemption arc (regain favor)
- Or opportunity for revenge arc (destroy family)

---

## Narrative Opportunities

### Family Drama Examples

**The Disgraced Heir:**
- Ultra High pure family
- Heir becomes corrupt through dark magic study
- Father (dynasty head) disowns son
- Son falls to Poor tier (lost inheritance)
- Seeks revenge by allying with rival dynasty
- Initiates political sabotage against father

**The Reluctant Politician:**
- High materialistic family desires wealth
- Dynasty desires honor through titleships
- Individual is peaceful + forgiving
- Conflicted between personal values and family duty
- Yearly vengeance checks against corrupt rival
- Sometimes pursues family business, sometimes titleship politics

**The Opportunistic Clan:**
- Corrupt evil mid-wealth family
- Sees wealthy neighbor's business failing
- Family collectively undermines (lawful coordination)
- Buys assets, poaches workers, spreads rumors
- Rival family collapses to poor tier
- Opportunists rise to high tier
- Village social structure shifts (new elite family)

---

## Integration With Existing Systems

### Alignment/Outlook System
✅ **Drives Social Behavior**
- Good/Pure → charity and solidarity
- Evil/Corrupt → opportunism and undermining
- Outlook modifies donation amounts and political strategies

### Behavioral Personality
✅ **Filters Family Actions**
- Bold families pursue aggressive expansion
- Craven families fortify and defend wealth
- Vengeful dynasties hold grudges across generations
- Forgiving families reconcile with rivals

### Initiative System
✅ **Governs Collective Action Rate**
- High initiative families act quickly on opportunities
- Low initiative families slow to adapt
- Individual initiative conflicts with family initiative

### Governance System
✅ **Political Representation**
- Ultra High families hold council seats
- Dynasties compete for titles and governance
- Disinheritance creates political vacuums

---

## Truth Source Components (Draft)

```csharp
VillagerWealth : IComponentData {
    int Currency;                   // Liquid wealth
    int AssetValue;                 // Properties, businesses
    byte WealthStrata;              // 0=UltraPoor, 4=UltraHigh
    uint LastStratumShift;          // Tick of last tier change
}

FamilyAggregate : IComponentData {
    Entity HeadOfFamily;

    // Behavioral averages
    float AverageVengeful;
    float AverageBold;
    float AverageGoodEvil;
    float AverageLawfulChaotic;
    float AveragePureCorrupt;

    // Economic
    int TotalWealth;
    byte LowestMemberStrata;        // Family tier = lowest member

    // Initiative & desires
    float FamilyInitiative;
    FixedString32Bytes PrimaryDesire;
    FixedString32Bytes PrimaryAmbition;

    float InternalCohesion;
}

DynastyAggregate : IComponentData {
    Entity DynastyHead;
    FixedString64Bytes DynastyName;

    // Same as family, broader scope
    byte CouncilSeatsHeld;
    int TotalDynasticAssets;
    uint FoundedTick;
}

VillagerFamilyMembership : IBufferElementData {
    Entity MemberEntity;
    byte RelationshipType;          // 0=spouse, 1=child, 2=parent, 3=sibling
    float ContributionWeight;       // For averaging stats
    byte Standing;                  // Favor with family head (0-100)
}

CharityEvent : IBufferElementData {
    Entity Donor;
    Entity Recipient;
    int Amount;
    FixedString32Bytes Reason;      // "death_of_son", "business_failure"
    uint Tick;
}

UnderminningAction : IBufferElementData {
    Entity Opportunist;
    Entity Target;
    FixedString32Bytes ActionType;  // "spread_rumor", "poach_worker"
    int Severity;
    uint Tick;
}
```

---

## Open Questions & Design Decisions

### Economic Tuning
1. **Wealth thresholds?** What currency amounts define each tier?
   - Ultra Poor: <-100
   - Poor: 0-500
   - Mid: 500-2000
   - High: 2000-10,000
   - Ultra High: >10,000
   - <NEEDS TUNING> based on village economy

2. **Donation amounts?** Are 5% donations too generous or too stingy?

3. **Undermining severity?** How much should opportunists gain from exploiting weakness?

### Family Mechanics
4. **Family size limits?** Max members per family before splitting?

5. **Dynasty formation?** How many generations before family becomes dynasty?

6. **Inheritance rules?** Primogeniture, equal split, merit-based?

### Political Systems
7. **Council seat allocation?** How many ultra high families per council?

8. **Titleship mechanics?** <TO CONCEPTUALIZE> How do dynasties gain/lose titles?

9. **Politicking mechanics?** <TO CONCEPTUALIZE> What are the actual political actions?

### Balancing
10. **Social mobility rate?** Should mid→high be rare or achievable?

11. **Elite stability?** Should ultra high families ever fall naturally?

12. **Charity impact?** Can donations prevent strata drops?

---

## Next Steps

**Before Implementation:**
- [ ] Define exact wealth thresholds per tier
- [ ] Design inheritance mechanics (primogeniture, partible, etc.)
- [ ] Conceptualize political/titleship systems
- [ ] Design council seat allocation rules
- [ ] Create family formation/splitting rules
- [ ] Define dynasty founding criteria

**Dependent Systems to Conceptualize:**
- [ ] **Governance & Politics** - Council mechanics, voting, titles
- [ ] **Marriage & Breeding** - Alliance formation, genetic inheritance
- [x] **Education System** - ✅ See [Education_And_Tutoring_System.md](Education_And_Tutoring_System.md) - Elite monopoly on quality education, private schools, tutors
- [x] **Business Ownership** - ✅ See [BusinessAndAssets.md](../Economy/BusinessAndAssets.md) - Asset management, income generation
- [x] **Entity Relations** - ✅ See [Entity_Relations_And_Interactions.md](Entity_Relations_And_Interactions.md) - Social dynamics, friendships, rivalries
- [ ] **Reputation System** - How rumors and undermining affect standing

**Documentation:**
- [ ] Create example family trees with stats
- [ ] Document conflict resolution probability tables
- [ ] Write telemetry metrics for wealth mobility
- [ ] Define UI representation of family/dynasty relationships

---

## Related Documentation

- **Entity Relations:** [Entity_Relations_And_Interactions.md](Entity_Relations_And_Interactions.md) - Relation mechanics, charity triggers, undermining mechanics
- **Business & Assets:** [BusinessAndAssets.md](../Economy/BusinessAndAssets.md) - Business ownership, asset management, economic dynamics
- **Education & Tutoring:** [Education_And_Tutoring_System.md](Education_And_Tutoring_System.md) - Elite private schools, tutors, education monopoly
- **Behavioral Personality:** [Villager_Behavioral_Personality.md](Villager_Behavioral_Personality.md) (vengeful/forgiving, bold/craven)
- **Alignment Framework:** [Village_Villager_Alignment.md](Village_Villager_Alignment.md) (good/evil, lawful/chaotic, pure/corrupt)
- **Sandbox Villages:** [Sandbox_Autonomous_Villages.md](../Core/Sandbox_Autonomous_Villages.md) (initiative, village culture)
- **Elite Courts:** [Elite_Courts_And_Retinues.md](Elite_Courts_And_Retinues.md) - Courtier positions, retinues for ruling families
- **Spies & Double Agents:** [Spies_And_Double_Agents.md](Spies_And_Double_Agents.md) - Elite espionage warfare, dynastic assassinations
- **Governance (TBD):** Politics, titles, council mechanics
- **Marriage & Breeding (TBD):** Alliance formation, propagation

---

**For Designers:** Wealth strata + family ambitions create emergent power struggles. Focus on tuning charity vs opportunism based on alignment to create distinct village cultures.

**For Implementers:** Start with simple wealth tracking and family aggregates. Political/titleship systems can layer on top once economic foundation is stable.

**For Narrative:** Disinheritance, seismic elite falls, and family vs individual conflict create rich emergent stories. Track these events for player-facing narratives.

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core vision captured, dependent systems require conceptualization
