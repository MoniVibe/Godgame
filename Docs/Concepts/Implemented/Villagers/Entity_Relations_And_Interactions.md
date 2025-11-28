# Entity Relations & Interactions System

**Status:** Draft - Concept Design
**Category:** Mechanic - Social Dynamics
**Complexity:** High
**Scope:** Global (all entities with agency)
**Created:** 2025-11-06
**Last Updated:** 2025-11-06

---

## Overview

**Core Concept:** Every entity (individual, family, aggregate, guild) maintains dynamic relationships with other entities they've met, ranging from -100 (bitter enemies) to +100 (devoted allies/friends). Relations form through social contexts (work, adventure, combat) and evolve based on alignment compatibility, behavioral personalities, and shared experiences.

**Status:** Confirmed - Entities have no acquaintance until first meeting
- Initial relations calculated on meeting based on alignment/outlook compatibility
- Relations drift over time through interactions, events, and periodic checks
- Extreme personalities can occasionally act outside norms for narrative drama

**Category:** Social | Diplomatic | Economic | Military

---

## Core Concept

### Relation Spectrum

```
-100                    -50                     0                      +50                   +100
 │                       │                      │                       │                      │
[Mortal Enemies]    [Hostile]            [Neutral/Strangers]        [Friendly]         [Devoted Allies]
     │                   │                      │                       │                      │
 Assassination      Open Conflict         Indifferent           Cooperation          Self-Sacrifice
 Active Sabotage    Undermining          No interaction         Trade/Support        Unwavering Loyalty
```

**Key Thresholds:**
```
-100 to -80:  Mortal Enemies (seek death/destruction)
-79  to -50:  Hostile (active opposition, conflict)
-49  to -25:  Unfriendly (distrust, avoid cooperation)
-24  to +24:  Neutral (no strong feelings, transactional)
+25  to +49:  Friendly (trust, cooperate willingly)
+50  to +79:  Close Friends/Strong Allies (mutual support, sacrifice)
+80  to +100: Devoted (unwavering loyalty, self-sacrifice)
```

---

## How It Works

### Meeting Mechanics

**Acquaintance System:**
- Entities start with **no relations** (unmet)
- First meeting triggers **Initial Relation Calculation**
- Meeting contexts determine base relation offset

**Meeting Contexts:**

| Context | Base Relation Offset | Meeting Probability |
|---------|---------------------|---------------------|
| **Village Neighbor** | 0 (neutral start) | High (same village) |
| **Workplace** | +10 (coworker camaraderie) | High (same business/profession) |
| **Family Introduction** | +20 (family vouches) | Medium (family events) |
| **Festival/Social Event** | +5 (positive mood) | Medium (periodic festivals) |
| **Combat - Same Side** | +15 (shared danger bonds) | Low (wars, crises) |
| **Combat - Opposing Sides** | -30 (enemies) | Low (wars, raids) |
| **Adventuring/Guild** | +10 (shared purpose) | Low (guild missions) |
| **Conscription** | +5 (forced camaraderie) | Low (military drafts) |
| **Diplomatic Meeting** | 0 (formal, neutral) | Very Low (embassy, council) |
| **Crime Victim** | -50 (betrayal/harm) | Rare (theft, assault) |
| **Rescue/Salvation** | +40 (life debt) | Rare (heroic intervention) |

### Initial Relation Calculation

When two entities meet for the first time:

```
InitialRelation =
  ContextOffset +
  AlignmentCompatibility +
  OutlookCompatibility +
  BehavioralModifier +
  FamilialBonus +
  RandomFactor

Clamped to [-100, +100]
```

**Component Breakdown:**

#### 1. Alignment Compatibility

```
AlignmentCompatibility =
  MoralAxisDelta +
  OrderAxisDelta +
  PurityAxisDelta

MoralAxisDelta (Good ↔ Evil):
  Δ = abs(Entity1.MoralAxis - Entity2.MoralAxis)

  If both Good (both > +30):
    Bonus = +20 - (Δ × 0.2)
    Example: +50 and +70 Good → Δ=20 → +20-(20×0.2) = +16

  If both Evil (both < -30):
    Bonus = +15 - (Δ × 0.15)
    Example: -50 and -80 Evil → Δ=30 → +15-(30×0.15) = +10.5

  If opposite (one Good, one Evil):
    Penalty = -(Δ × 0.3)
    Example: +60 Good and -60 Evil → Δ=120 → -(120×0.3) = -36

OrderAxisDelta (Lawful ↔ Chaotic):
  Δ = abs(Entity1.OrderAxis - Entity2.OrderAxis)

  If both Lawful (both > +30):
    Bonus = +15 - (Δ × 0.1)
    Lawfuls respect order, less weight than morality

  If both Chaotic (both < -30):
    Variable = RandomRange(-10, +10)
    Chaotics are unpredictable, relations wildly vary

  If opposite (one Lawful, one Chaotic):
    Penalty = -(Δ × 0.2)
    But less severe than Good/Evil opposition

PurityAxisDelta (Pure ↔ Corrupt):
  Δ = abs(Entity1.PurityAxis - Entity2.PurityAxis)

  If both Pure (both > +30):
    Bonus = +10 - (Δ × 0.05)
    Pure entities appreciate selflessness

  If both Corrupt (both < -30):
    Variable = -(Δ × 0.1) + Egocentric Modifier
    Corrupt entities compete unless aligned goals

  If opposite (one Pure, one Corrupt):
    Pure: Penalty = -(Δ × 0.15)
    Corrupt: Penalty = -(Δ × 0.05) (don't care as much)
```

**Special Alignment Interactions:**

```
Pure Good + Pure Evil:
  Base: -36 (moral opposition) + 10 (both pure) = -26
  → Starts Unfriendly, but Pure modifier allows unlikely friendship
  → If shared experiences (+5 per positive interaction)
  → Can reach +50+ (loyal evil devoted to good ally)

Corrupt Good + Corrupt Evil:
  Base: -36 (moral opposition) - 10 (both corrupt compete) = -46
  → Starts Hostile, but both corrupt = transactional
  → Alliance possible if mutually beneficial (+30 temporary alliance)
  → Constant plotting (-5/year) but never dissolve if profitable

Lawful Good + Chaotic Good:
  Base: +16 (both good) - 12 (order opposition) = +4
  → Starts Neutral, can become friends through good deeds
  → Lawful frustrated by chaos (-2/year)
  → Chaotic frustrated by rigidity (-2/year)
  → But shared morality overcomes (+5/year if cooperate)

Lawful Evil + Chaotic Evil:
  Base: +10 (both evil) - 12 (order opposition) = -2
  → Starts Neutral/Unfriendly
  → Lawful sees chaotic as unreliable (-5/interaction)
  → Chaotic sees lawful as oppressive (-3/interaction)
  → Unlikely to cooperate
```

#### 2. Outlook Compatibility

```
OutlookCompatibility = Σ(MatchingOutlooks × Weight)

Matching Outlooks:
  If Entity1 and Entity2 share any outlook:
    Warlike + Warlike:     +15 (warrior bond)
    Peaceful + Peaceful:   +10 (harmony)
    Spiritual + Spiritual: +12 (shared faith)
    Materialistic + Materialistic: +8 (deal-making)
    Scholarly + Scholarly: +10 (intellectual respect)
    Xenophobic + Xenophobic: +15 (us vs them mentality)
    Egalitarian + Egalitarian: +10 (shared values)
    Authoritarian + Authoritarian: +5 (compete for dominance)

Opposing Outlooks:
  Warlike vs Peaceful:       -15
  Spiritual vs Materialistic: -10 (but special rule below)
  Xenophobic vs Egalitarian: -20 (fundamental opposition)
  Authoritarian vs Egalitarian: -18

Special Outlook Interactions:
  Fanatic Materialistic + Fanatic Spiritual:
    Base: -10 (opposition)
    But: Each debate/interaction → +2 relation
    Reason: Intellectual stimulation, enjoy the challenge
    Cap: +40 (respectful rivalry/friendship)
```

#### 3. Behavioral Modifier

```
BehavioralModifier based on personality traits:

Entity1.Forgiving (>+60) meets anyone:
  Modifier: +10 (gives benefit of doubt)

Entity1.Vengeful (>+60) meets anyone:
  Modifier: -5 (suspicious, grudge-prone)

Entity1.Bold (>+60) meets Entity2.Bold (>+60):
  Modifier: +8 (mutual respect for courage)

Entity1.Craven (>+60) meets Entity2.Craven (>+60):
  Modifier: +5 (mutual understanding of caution)

Entity1.Bold meets Entity2.Craven:
  Bold: -8 (sees as coward)
  Craven: -5 (intimidated)

Entity1.Trusting (>+60):
  Modifier: +12 (assumes best intentions)

Entity1.Paranoid (>+60):
  Modifier: -15 (assumes worst)
```

#### 4. Familial Bonus

```
FamilialBonus based on relationship:

Same Nuclear Family:
  Parent-Child:    +80 (unconditional love, usually)
  Siblings:        +60
  Spouses:         +70

Same Extended Family:
  Grandparent-Grandchild: +50
  Uncle/Aunt-Nephew/Niece: +40
  Cousins:               +30

Same Dynasty (but not immediate family):
  Dynasty members:       +20

Family Vouching (family member introduces):
  Friend's family:       +10
  Friendly family intro: +15

Disowned/Exiled Family:
  Former family:         -40 (betrayal penalty)
  If disowning head still alive: -60
```

#### 5. Random Factor

```
RandomFactor = RandomRange(-10, +10)

Purpose:
  - Ensures not all identical alignments have identical relations
  - Allows for personality quirks
  - Creates unexpected friendships/rivalries

Chaotic Modifier:
  If either entity Chaotic (<-30 Order):
    RandomFactor = RandomRange(-20, +20)
    Doubles unpredictability
```

---

### Example Initial Relation Calculations

#### Example 1: Two Lawful Good Village Neighbors

```
Context: Village Neighbor → 0

Entity A: Lawful Good Pure (+60 moral, +70 order, +50 pure)
Entity B: Lawful Good Pure (+65 moral, +75 order, +45 pure)

MoralAxisDelta:
  Both Good: +20 - (5 × 0.2) = +19

OrderAxisDelta:
  Both Lawful: +15 - (5 × 0.1) = +14.5

PurityAxisDelta:
  Both Pure: +10 - (5 × 0.05) = +9.75

OutlookCompatibility:
  (Assume both Peaceful): +10

BehavioralModifier:
  (Assume both Forgiving +70): +10

FamilialBonus: 0 (not family)

RandomFactor: +3 (rolled)

Total: 0 + 19 + 14.5 + 9.75 + 10 + 10 + 0 + 3 = +66

Result: Close Friends immediately (start at +66/100)
Narrative: "When Erik met Mira, they instantly bonded over their shared values and gentle natures. A lifelong friendship began."
```

#### Example 2: Pure Good Paladin meets Pure Evil Assassin

```
Context: Combat - Same Side (defending village from demons) → +15

Entity A: Pure Good Lawful (+80 moral, +60 order, +80 pure)
Entity B: Pure Evil Chaotic (-80 moral, -50 order, +70 pure)

MoralAxisDelta:
  Opposite: -(160 × 0.3) = -48

OrderAxisDelta:
  Opposite: -(110 × 0.2) = -22

PurityAxisDelta:
  Both Pure: +10 - (10 × 0.05) = +9.5

OutlookCompatibility:
  (Assume Warlike both): +15

BehavioralModifier:
  A.Forgiving (+80): +10
  B.Vengeful (+70): -5
  Net: +5

FamilialBonus: 0

RandomFactor: +8

Total: 15 - 48 - 22 + 9.5 + 15 + 5 + 0 + 8 = -17.5

Result: Neutral/Slightly Unfriendly (start at -17)
Narrative: "The paladin Aldric and the assassin Shade fought side-by-side against the demon horde. Despite their moral opposition, Aldric's forgiving nature and Shade's purity created an uneasy respect. Aldric thought: 'He may be evil, but he fights with honor.' Shade thought: 'He judges me, yet trusts me in battle. Curious.'"

Over time with shared battles: +5 per battle → After 10 battles: +33
After 20 battles: +83 (Devoted)
Narrative: "Against all odds, the holy warrior and the dark assassin became inseparable. Shade, loyal to a fault despite his evil nature, would die for Aldric. 'You are the only one who never abandoned me,' Shade said. Aldric replied, 'And you've proven good and evil are not so simple.'"
```

#### Example 3: Corrupt Good Merchant meets Corrupt Evil Warlord

```
Context: Diplomatic Meeting (trade negotiation) → 0

Entity A: Corrupt Good (+40 moral, +20 order, -60 corrupt)
Entity B: Corrupt Evil (-50 moral, +30 order, -70 corrupt)

MoralAxisDelta:
  Opposite: -(90 × 0.3) = -27

OrderAxisDelta:
  Both Lawful-ish: +15 - (10 × 0.1) = +14

PurityAxisDelta:
  Both Corrupt: Egocentric competition
  -(10 × 0.1) = -1

OutlookCompatibility:
  (Assume both Materialistic): +8

BehavioralModifier:
  Both Opportunistic: 0

FamilialBonus: 0

RandomFactor: -3

Total: 0 - 27 + 14 - 1 + 8 + 0 + 0 - 3 = -9

Result: Neutral/Slightly Unfriendly (start at -9)
Narrative: "The merchant Lord Verin met Warlord Krath for trade talks. Neither trusted the other, both saw only profit. 'We can help each other,' Verin proposed. 'Or destroy each other,' Krath countered. They shook hands."

Alliance Modifier: +30 (mutually beneficial trade)
Current Relation: +21 (Friendly for business)

But plotting continues:
  Each plots against the other (-5/year)
  But profit keeps alliance stable (+10/year)
  Net: +5/year drift

After 10 years: +71 (Strong Allies)
Narrative: "Ten years of scheming, ten years of profit. Verin once tried to poison Krath's supplies. Krath once tried to seize Verin's caravans. Both failed. Both laughed. 'We keep each other sharp,' Verin said. Krath agreed: 'I'd miss you if you died. But I'd still try to kill you if it were profitable.' They toasted."
```

#### Example 4: Fanatic Materialist meets Fanatic Spiritual

```
Context: Festival/Social Event → +5

Entity A: Fanatic Materialistic (+20 moral, 0 order, -40 corrupt)
Entity B: Fanatic Spiritual (+30 moral, +10 order, +60 pure)

MoralAxisDelta:
  Both Good-ish: +20 - (10 × 0.2) = +18

OutlookCompatibility:
  Materialistic vs Spiritual: -10

But Special Rule: Each debate → +2

BehavioralModifier:
  A.Bold (+50): +8
  B.Bold (+55): +8
  Mutual respect: +5

RandomFactor: +2

Total: 5 + 18 - 10 + 5 + 2 = +20

After 1st debate: +22
After 10 debates: +40
After 20 debates: +60

Result: Close Friends through constant intellectual sparring
Narrative: "Marcus the banker and Sister Elira met at a festival. Within minutes they were debating the nature of wealth and spirit. 'Gold is the measure of all things!' 'No, faith is!' They argued for hours. And the next day. And every week for twenty years. Their friendship deepened with each debate. Neither converted the other. Neither wanted to."
```

---

## Parameters / Variables

### Core Relation Parameters

| Parameter | Default | Range | Notes |
|-----------|---------|-------|-------|
| **RelationValue** | 0 (unmet) | -100 to +100 | Core relation score |
| **RelationDriftPerYear** | 0 | -50 to +50 | Annual passive change |
| **LastInteractionTick** | 0 | uint | When last interacted |
| **InteractionCount** | 0 | uint | Total interactions |
| **PositiveInteractions** | 0 | uint | Helpful actions |
| **NegativeInteractions** | 0 | uint | Harmful actions |
| **SharedBattles** | 0 | uint | Combat together |
| **SharedBusinesses** | 0 | uint | Economic partnerships |

### Rationality Class

**Determines likelihood of acting against personality:**

| Rationality Class | Description | Out-of-Character Chance | Check Frequency |
|-------------------|-------------|------------------------|-----------------|
| **Fanatic** | Extreme personality, rarely deviates | 1% per year | Every 6 months |
| **Strong** | Strongly aligned to personality | 5% per year | Every 3 months |
| **Moderate** | Generally follows personality | 15% per year | Every month |
| **Weak** | Personality is suggestion, not rule | 30% per year | Every 2 weeks |
| **Variable** | Chaotic, unpredictable | 50% per year | Every week |

**Calculation:**
```
RationalityClass determined by personality extremity:

If Vengeful > +80 OR Forgiving > +80 OR Bold > +80 OR Craven > +80:
  → Fanatic

If any personality > +60:
  → Strong

If any personality > +40:
  → Moderate

If all personalities < +40:
  → Weak

If Chaotic < -60:
  → Variable (overrides above)
```

**Example:**
```
Entity: Peaceful (+90), Forgiving (+85), Craven (+70)
Rationality: Fanatic (Forgiving +85 triggers)

Behavior checks:
  Every 6 months: Roll for vengeance despite Forgiving personality
  Chance: 1% (0.5% per check)

After 100 years (200 checks):
  Expected vengeance acts: ~1

Narrative: "Elder Mira lived 100 years without a single vengeful act. But once, when her granddaughter was murdered, she broke. The village watched in shock as the gentle saint hunted the killer for three days. She never spoke of it again."
```

### Periodic Behavior Checks

**Check Types:**

| Check Type | Personality | Base Frequency | Rationality Modifier |
|------------|-------------|----------------|---------------------|
| **Vengeance Check** | Forgiving | 1/year | Fanatic: 1% chance<br>Strong: 5%<br>Moderate: 15% |
| **Forgiveness Check** | Vengeful | 1/year | Fanatic: 1%<br>Strong: 5%<br>Moderate: 15% |
| **Worship Check** | Materialistic | During crises | Fanatic: 5%<br>Strong: 15%<br>Moderate: 30% |
| **Greed Check** | Spiritual | During scarcity | Fanatic: 2%<br>Strong: 10%<br>Moderate: 25% |
| **Courage Check** | Craven | During battles | Fanatic: 1%<br>Strong: 8%<br>Moderate: 20% |
| **Caution Check** | Bold | During danger | Fanatic: 3%<br>Strong: 10%<br>Moderate: 25% |
| **Mercy Check** | Evil | When victorious | Fanatic: 0.5%<br>Strong: 3%<br>Moderate: 10% |
| **Cruelty Check** | Good | When threatened | Fanatic: 0.5%<br>Strong: 3%<br>Moderate: 10% |

**Crisis Modifiers:**
```
During Apocalypse/Siege/Famine:
  All out-of-character check chances × 3

Example:
  Fanatic Materialist Worship Check during crisis:
    Normal: 5%
    During crisis: 15%

  Narrative: "Even the greediest merchant prayed when demons surrounded the village."
```

---

## Edge Cases

### 1. First Meeting in Combat (Opposing Sides)

```
Context: Combat - Opposing Sides → -30

Entity A: Lawful Good
Entity B: Lawful Neutral (defending invaded village)

MoralAxisDelta: Small positive (both lean good)
OutlookCompatibility: Similar
But: Combat context dominates

Initial: -30 + modifiers = -15 (Unfriendly)

After combat if A spares B's life:
  Mercy bonus: +30
  New relation: +15 (Friendly)

After war ends, B joins A's village:
  Shared village bonus: +10/year
  After 5 years: +65 (Close Friends)

Narrative: "They met as enemies. Ser Marcus could have killed the farmer defending his home but chose mercy. Years later, that farmer saved Marcus from assassination. 'I owed you my life,' he said. 'Now we're even.'"
```

### 2. Chaotic Entities (Unpredictable Relations)

```
Entity A: Chaotic Evil (-60 moral, -80 order)
Entity B: Lawful Good (+70 moral, +80 order)

Initial Calculation:
  MoralAxis: -(130 × 0.3) = -39
  OrderAxis: -(160 × 0.2) = -32
  RandomFactor (chaotic): RandomRange(-20, +20) = +18

Total: -39 - 32 + 18 = -53 (Hostile)

But: Chaotic Drift
  Every month: RandomRange(-10, +10) relation change

Month 1: -53 + 8 = -45
Month 2: -45 - 12 = -57
Month 3: -57 + 15 = -42
...
Year 1: Might be -80 (Mortal Enemies) or -10 (Unfriendly)

Narrative: "No one could predict how the mad cultist felt about the paladin. One day he cursed her name. The next he brought her flowers. She learned to expect nothing."
```

### 3. Familial Betrayal (Disowning)

```
Parent-Child: Start +80

Child commits crime (theft, murder):
  Crime penalty: -60

Current: +20 (Friendly, but strained)

Parent (Pure Lawful +80) triggers disowning:
  Disown penalty: -60 additional

Final: -40 (Hostile)

Parent's internal conflict:
  Lawful demands justice: Disown
  Familial love: +80 base
  Rationality Check (Strong, 5% chance): Forgive instead

95% of time: Disowns (relation -40)
5% of time: Forgives (relation remains +20, +10 forgiveness bonus → +30)

Narrative: "Lord Aldric disowned his son for theft. It was the law. But every night he wept. When his son was executed, Aldric never smiled again."

OR (5% chance):

Narrative: "Lord Aldric stood before the council. 'My son stole to feed the poor,' he lied. 'I vouch for him.' The law bent. The son wept in gratitude. Years later, he became the most just ruler the village ever knew."
```

### 4. Corrupt Entities Competing

```
Entity A: Corrupt Good Merchant (-50 pure, +30 moral)
Entity B: Corrupt Good Merchant (-55 pure, +35 moral)

MoralAxisDelta: +20 - (5 × 0.2) = +19
PurityAxisDelta (both corrupt): -(5 × 0.1) = -0.5
OutlookCompatibility (both materialistic): +8
BehavioralModifier (both opportunistic): 0

Initial: +26.5 (Friendly for business)

But: Corrupt competition
  Both want same resources: -10/year

After 5 years: -23.5 (Unfriendly)

Unless: Mutual profit exceeds competition
  If alliance profitable: +15/year
  Net: +5/year

After 5 years: +51.5 (Strong Allies)

Narrative: "The two merchants hated each other. Until they realized fighting was expensive. Together, they monopolized the market. Neither trusted the other. Both got rich."
```

---

## Aggregate Entity Relations

### Overview

**Aggregate entities** (villages, families, dynasties, guilds, bands, armies) can have relations with other aggregates and with individuals. These relations determine cooperation, conflict, vassalization, and humanitarian aid.

**Relation Mechanics:**
- Aggregates use same -100 to +100 spectrum
- Relations calculated from average member alignment OR leader's alignment (weighted)
- Aggregate actions (aid, invasion, trade) affect relations

### Village-to-Village Relations

**Initial Relation (when villages first interact):**

```
InitialVillageRelation =
  ProximityFactor +
  AlignmentCompatibility (village aggregated alignment) +
  ResourceCompetition +
  TradeOpportunity +
  SharedThreats +
  RandomFactor

ProximityFactor:
  Adjacent villages:    -10 (territorial tension)
  1-2 regions away:     0 (neutral)
  3+ regions away:      +5 (no competition)

ResourceCompetition:
  Competing for same resource nodes:  -20
  Complementary resources:            +10 (trade opportunity)

TradeOpportunity:
  Surplus-deficit match:  +15
  Both surplus:           0
  Both deficit:           -5 (neither can help)

SharedThreats:
  Both threatened by same enemy:  +30 (common cause)
  One threatened, one safe:       0
```

**Example:**
```
Village A (Lawful Good) and Village B (Lawful Good):
  Proximity: Adjacent → -10
  Alignment: Both Lawful Good → +35
  Resources: Complementary (A has iron, B has wood) → +10
  Trade: A needs wood, B needs iron → +15
  Threats: Both threatened by demon horde → +30

  Total: -10 + 35 + 10 + 15 + 30 = +80 (Strong Allies)

Narrative: "The two villages, though neighbors, united against the demon threat. Their alliance would last centuries."
```

### Humanitarian Aid & Outreach

**Triggers for Aid (from high-relation villages):**

| Trigger | Relation Threshold | Aid Type |
|---------|-------------------|----------|
| **Famine** | +30 (Friendly) | Food shipments |
| **Siege** | +40 (Close Allies) | Peacekeeper relief bands |
| **Plague** | +50 (Strong Allies) | Mobile hospitals, volunteer doctors |
| **Natural Disaster** | +35 (Friendly) | Reconstruction materials, labor |
| **Refugee Crisis** | +25 (Friendly) | Temporary shelter, integration |

**Peacekeeper Relief Bands:**
```
Conditions:
  - Relation ≥ +40 (Close Allies)
  - Requesting village under siege OR bandit raids
  - Sending village has military surplus (not also under threat)

Composition:
  - 20-50 armed peacekeepers (village guards)
  - 5-10 medical workers (volunteer doctors)
  - 3-5 supply wagons (food, medicine)

Duration:
  - Until threat resolved OR 3 months (whichever first)
  - Extended if relation ≥ +60 (Strong Allies)

Relation Impact:
  - If successful (siege broken): +10 relation boost
  - If peacekeepers die defending: +20 relation boost (martyrs)
  - If sending village recalls early: -15 relation penalty (betrayal)
```

**Mobile Hospitals:**
```
Conditions:
  - Relation ≥ +50 (Strong Allies)
  - Plague, epidemic, or mass casualties
  - Sending village has medical guild OR high education

Staff:
  - 10-20 volunteer doctors (educated villagers)
  - 30-50 nurses/helpers
  - 5 wagons of medical supplies

Effectiveness:
  - Reduces mortality rate by 30-60% (based on doctor education)
  - Costs sending village 500-1000 currency/month
  - Doctors gain medical experience (+5 Education per month)

Relation Impact:
  - During aid: +5 relation/month
  - If epidemic cured: +25 relation boost
  - If doctors die from disease: +15 relation (sacrifice honored)
```

**Example Scenario:**
```
Village A (+70 relation with Village B) hears B has plague outbreak

Village A (Good alignment, +60 moral):
  Initiative check: High (proactive)
  Decision: Send mobile hospital

10 doctors, 30 nurses, 5 wagons deployed
  Cost: 800 currency/month
  Duration: 4 months

Results:
  - Mortality reduced from 80% to 35%
  - 3 doctors died (caught plague)
  - 800 villagers saved

Relation impact:
  - During aid: +5/month × 4 = +20
  - Epidemic cured: +25
  - Doctors martyred: +15
  Total: +60 relation gain → New relation: +130 (capped at +100)

Final: Village A and B at +100 (Devoted Allies)

Narrative: "When the plague came, Village A did not hesitate. Their doctors knew the risks. Three died saving strangers. Village B would never forget. 'We are brothers now,' they said. 'Forever.'"
```

---

## Hierarchical Relations

### Vassalization & Protectorate Systems

**Relationship Types:**

| Type | Power Dynamic | Autonomy | Tribute | Protection |
|------|--------------|----------|---------|-----------|
| **Vassalization** | Overlord > Vassal | Low (controlled) | High (30-50% tax) | Guaranteed |
| **Protectorate** | Protector > Client | Medium (semi-autonomous) | Medium (15-30%) | Guaranteed |
| **Guardianship** | Guardian > Ward | Medium (guidance) | Low (10-20%) | Full support |
| **Alliance** | Equal | Full | None (mutual aid) | Mutual defense |
| **Ward** | Guardian > Minor | Low (full control) | None (charity) | Full custody |
| **Adoption** | Parent > Child | None (family) | None | Full integration |

### Vassalization Mechanics

**Formation Conditions:**

```
Vassalization occurs when:
  1. One village conquers another (forced vassalization)
  2. Weak village requests protection (voluntary vassalization)
  3. Elite family gains control over another village
  4. Dynasty expands through political marriage + dominance

Vassalization Terms:
  - Tribute Rate: 30-50% of vassal's tax revenue
  - Military Support: Vassal must provide troops on demand
  - Autonomy: Vassal retains internal governance (village council)
  - Protection: Overlord defends vassal from external threats
  - Relations: Overlord-Vassal relation tracked separately
```

**Vassal Types:**

**1. Loyal Vassal (Relation +40 to +100):**
```
Characteristics:
  - Pays tribute on time (100% compliance)
  - Provides troops enthusiastically
  - Adopts overlord's alignment over time (slow drift)
  - Economic cooperation (trade bonuses)

Tribute Compliance: 100%
Military Support: Immediate response
Rebellion Risk: 0% (unless overlord relation drops)

Reasons for Loyalty:
  - Favorable vassalization terms (low tribute, high autonomy)
  - Overlord protects effectively (defeats threats)
  - Cultural/alignment match (both Lawful Good)
  - Economic prosperity (vassal benefits from protection)

Example:
  Village A (Lawful Good overlord) and Village B (vassal)
  Tribute: 30% (favorable)
  Autonomy: High (retain council, culture)
  Protection: Overlord defeated 3 invasions

  Relation: +75 (Strong Allies)

  Narrative: "Village B thrived under Village A's protection. 'Why would we want independence?' they said. 'Our overlord is just and protects us. We are content.'"
```

**2. Neutral Vassal (Relation 0 to +39):**
```
Characteristics:
  - Pays tribute reluctantly (80-95% compliance)
  - Provides troops when pressed
  - Maintains own culture/alignment
  - Seeks opportunities for better terms or independence

Tribute Compliance: 80-95%
Military Support: Delayed, minimal
Rebellion Risk: 10% if overlord weakened

Reasons for Neutrality:
  - Moderate tribute (35-40%)
  - Overlord distant, minimal interaction
  - Neither love nor hate overlord
  - Waiting for better opportunity

Example:
  Village C (Neutral overlord) and Village D (vassal)
  Tribute: 35%
  Autonomy: Medium
  Protection: Adequate but impersonal

  Relation: +15 (Neutral-Friendly)

  Narrative: "Village D paid their taxes and sent troops when asked. They neither loved nor hated their overlord. 'It's just business,' they said."
```

**3. Undermining Vassal (Relation -25 to -1):**
```
Characteristics:
  - Pays tribute late, underpays (50-70% compliance)
  - Provides troops unwillingly, deserts likely
  - Actively seeks liberation (contacts rivals, plots)
  - Hides resources, underreports production

Tribute Compliance: 50-70%
Military Support: Minimal, unreliable
Rebellion Risk: 30-50%

Undermining Actions:
  - Economic sabotage: Hide wealth, underreport production
  - Diplomatic: Secret alliance with rival overlords
  - Military: Train secret militia, prepare for rebellion
  - Cultural: Resist assimilation, maintain independence identity

Example:
  Village E (Authoritarian Evil overlord) and Village F (vassal)
  Tribute: 50% (extractive)
  Autonomy: Low (overlord controls council)
  Protection: Minimal, overlord selfish

  Relation: -10 (Unfriendly)

  Narrative: "Village F hated their overlord. They paid half their taxes and plotted in secret. 'One day,' they whispered, 'we will be free.'"
```

**4. Rebellious Vassal (Relation -100 to -26):**
```
Characteristics:
  - Refuses tribute (0-20% compliance)
  - Refuses military support, may join enemies
  - Open rebellion or secession attempts
  - Violence, assassinations, sabotage

Tribute Compliance: 0-20%
Military Support: None, may attack overlord
Rebellion Risk: 80-100% (imminent or ongoing)

Rebellion Triggers:
  - Overlord weakened (defeated in war)
  - Rival overlord offers support (liberation army)
  - Brutal overlord (massacres, oppression)
  - Chaotic vassal (rebels on principle)

Example:
  Village G (Cruel Evil overlord) and Village H (vassal)
  Tribute: 60% (crushing)
  Autonomy: None (martial law)
  Protection: Overlord started the threat (invaded H originally)

  Relation: -70 (Hostile)

  Rebellion:
    - Village H refuses tribute
    - Assassinates overlord's tax collectors
    - Contacts rival Village I for military support
    - Declares independence

  Overlord response (based on alignment):
    IF Lawful Evil: Systematic punishment (execute ringleaders)
    IF Chaotic Evil: Massacre (burn village)
    IF Lawful Good: Negotiation (offer better terms)

  Outcome:
    - Lawful Evil: Rebellion crushed, harsher terms (-80 relation permanent)
    - Chaotic Evil: Village H destroyed (no more vassal, relation N/A)
    - Lawful Good: Peaceful separation (independence granted)
```

### Overlord Reactions to Vassals

**Based on Alignment & Outlook:**

| Overlord Type | Loyal Vassal Reward | Rebellious Vassal Response |
|---------------|---------------------|---------------------------|
| **Lawful Good** | Lower tribute, autonomy grants | Negotiation, offer better terms |
| **Lawful Neutral** | Honor tribute pact, fair treatment | Legal enforcement, contract terms |
| **Lawful Evil** | Maintain status quo, no reward | Systematic punishment, execute leaders |
| **Neutral Good** | Economic aid, protection | Diplomatic pressure, sanctions |
| **True Neutral** | Transactional relationship | Proportional response, pragmatic |
| **Neutral Evil** | Exploit loyalty (increase tribute) | Brutal retaliation, enslave |
| **Chaotic Good** | Freedom, minimal tribute | Understand rebellion, may grant independence |
| **Chaotic Neutral** | Unpredictable, may forget to collect | Unpredictable (might ignore or massacre) |
| **Chaotic Evil** | No reward, may attack anyway | Total annihilation, example to others |

**Authoritarian vs Egalitarian:**

```
Authoritarian Overlord:
  - High control: Replaces vassal council with appointees
  - Cultural assimilation: Forces vassal to adopt culture
  - Tribute: 45-60% (extractive)
  - Rebellion response: Martial law, occupation

Egalitarian Overlord:
  - Low control: Vassal retains full autonomy
  - Cultural respect: Allows diversity
  - Tribute: 10-25% (cooperative)
  - Rebellion response: Referendum, may grant independence
```

### Protectorate (Favorable Vassalization)

**Differences from Vassalization:**

```
Protectorate Terms:
  - Tribute: 15-30% (lower than vassalization)
  - Autonomy: High (client state governs itself)
  - Protection: Protector treats client as junior partner
  - Relations: More equal, respectful

Formation:
  - Weak village requests protection voluntarily
  - Strong village offers protection (benevolent)
  - Mutual benefit (protector gains influence, client gains security)

Example:
  Village J (Lawful Good, powerful) and Village K (weak, threatened)

  Village K requests protection from bandits
  Village J agrees, offers protectorate terms:
    - Tribute: 20% (light)
    - Autonomy: Full (K retains council, culture)
    - Protection: J sends garrison, defends borders
    - Relations start: +40 (Friendly)

  After 10 years:
    - K prospers under protection
    - Tribute pays for itself (K's economy grows)
    - Relations: +80 (Strong Allies)
    - K prefers protectorate over independence

  Narrative: "Village K never regretted their decision. Village J was a gentle protector. When offered independence on their 20th anniversary, K refused. 'Why leave a good thing?' they said."
```

### Guardianship & Wards

**Elite-to-Elite Guardianship:**

```
Guardianship (between noble families):
  - Guardian Family takes custody of Ward (minor noble)
  - Ward raised in guardian household
  - Political tool: Secure loyalty, hostage for good behavior
  - Cultural transfer: Ward adopts guardian's values/alignment

Formation:
  - Diplomatic marriage (ward is child of alliance)
  - Vassal noble sends child as ward (ensure loyalty)
  - Orphaned noble taken in by guardian
  - Strategic hostage (prevent rebellion)

Relations:
  Guardian-Ward: Starts neutral (0)
  Grows based on treatment:
    Good treatment: +10/year → +50 after 5 years (family bond)
    Poor treatment: -5/year → -25 after 5 years (resentment)

Example:
  Duke Aldric (overlord) takes Baron Erik's son as ward

  Purpose: Ensure Baron Erik remains loyal (son as hostage)

  Treatment:
    - Aldric treats ward as own son (+10/year)
    - Ward becomes devoted to guardian (+60 after 6 years)
    - When ward inherits barony, remains loyal to Aldric

  Outcome: Political masterstroke (vassal loyalty secured through love, not fear)

  OR (if bad treatment):
    - Aldric mistreats ward (servant, not family) (-5/year)
    - Ward resents guardian (-40 after 8 years)
    - When inherits, leads rebellion against guardian

  Narrative: "The ward becomes the overlord's greatest ally or bitterest enemy, depending on how they were raised."
```

### Village-to-Elite Guardianship

**Village adopts orphaned elite:**

```
Conditions:
  - Elite family destroyed (war, plague, assassination)
  - Child survives, inherits title/land
  - Village takes guardianship (protect child until adulthood)

Relations:
  - Village-Ward: Starts +20 (protective)
  - If village Good: Treat ward as community child (+10/year)
  - If village Evil: Exploit ward's assets (-10/year, control wealth)

Outcomes:
  - Good village: Ward becomes loyal ruler when adult (+80 relation)
  - Evil village: Ward escapes or is controlled as puppet (-60 relation)
```

### Adoption (Full Integration)

**Village-to-Village:**
```
Rare, only when:
  - Small village absorbed by larger neighbor
  - Voluntary merger (both benefit)
  - Population migration (village abandoned, residents relocate)

Process:
  - Smaller village dissolves as entity
  - Population integrates into larger village
  - Cultural blending (average alignment shifts)
  - Relations N/A (now one entity)
```

**Elite-to-Elite:**
```
Noble adopts commoner or orphan:
  - Elevates commoner to nobility (rare)
  - Inherits family name, wealth, title
  - Relations: +60 (parent-child bond, usually)
  - Can trigger dynasty expansion (new bloodline)
```

---

## Guild-to-Guild Relations

**Same mechanics as individual relations, but aggregate:**

```
GuildRelation =
  AlignmentCompatibility (guild aggregate) +
  OutlookCompatibility (guild purpose) +
  GuildMasterRelation (personal) +
  SharedMissions +
  CompetitionFactor

Competition:
  Same guild type (two Heroes' Guilds):  -20 (compete for same missions)
  Complementary types (Heroes + Merchants): +10 (mutual benefit)

Shared Missions:
  Each successful cooperation: +5 relation
  Betrayal during mission: -40 relation (permanent grudge)
```

**High Relation Cooperation (+50+):**

```
Joint Missions:
  - Pool members for difficult threats
  - Share loot (negotiated split)
  - Coordinate training (knowledge exchange)
  - Embassy exchanges (recruit in each other's territories)

Economic Cooperation:
  - Merchants' Guild funds Heroes' Guild expeditions
  - Scholars' Guild researches for Mages' Guild
  - Artisans' Guild crafts for Heroes' Guild (discount)

Political Alliance:
  - Defend each other from hostile guilds
  - Veto hostile guild embassies in allied villages
  - Share intelligence (spy networks)
```

**Low Relation Conflict (-50-):**

```
Guild Wars (see Guild_System.md):
  - Sabotage missions
  - Assassinate rival guild masters
  - Poach members
  - Embargo (refuse services to rival's allies)
```

---

## Animal & Fauna Relations

### Individual Animals

**Simple Fauna (wolves, bears, deer):**
```
Relations: Limited to individuals only
Relation Range: -50 to +50 (simpler than humans)

RelationFactors:
  - Feeding: +5 per feeding event
  - Harming: -20 (attacked, injured)
  - Taming: +10 per successful taming attempt
  - Proximity: +1/month if peaceful coexistence

Domestication:
  - Relation +30: Animal becomes pet (follows owner)
  - Relation +50: Animal defends owner (combat ally)
  - Relation -30: Animal attacks on sight
  - Relation -50: Animal flees or fights to death

Example:
  Villager feeds wolf pup for 10 days (+50 relation)
  Wolf becomes pet, follows villager
  When villager attacked, wolf defends (+20 combat bonus)

  Narrative: "The wolf was his shadow. When bandits came, the wolf fought beside him. Both survived."
```

### Intelligent Fauna (Dragons, Mythical Creatures)

**Complex Relations (same as individuals + aggregates):**

```
Intelligent fauna can have relations with:
  - Individual entities (-100 to +100)
  - Village aggregates (-100 to +100)
  - Band/Army aggregates
  - Family/Dynasty aggregates (for long-lived creatures)

Dragon Example:
  Age: 500 years
  Intelligence: High (speaks, negotiates)
  Relations tracked:
    - With 50+ individual villagers met over centuries
    - With 10+ villages (territorial)
    - With 3 dynasties (ancient pacts)

  Village A (Lawful Good):
    Initial: -40 (fear of dragon)
    After dragon defends village from demons: +30
    After 50 years peaceful coexistence: +70
    Dragon becomes village guardian

  Relation benefits:
    - +70 relation: Dragon defends village from threats
    - Village provides tribute (gold, livestock)
    - Mutual respect (no attacks)

  Dynasty B (ancient bloodline):
    Initial: +60 (ancestor made pact 300 years ago)
    Dragon remembers: "Your great-great-grandfather saved my hatchlings. I am forever in your debt."
    Current heir: Automatic +60 relation (inherited)

    Narrative: "The dragon watched over the dynasty for ten generations. When enemies came, fire rained from the sky."
```

**Aggregate Relations for Narrative:**

```
Intelligent fauna remember:
  - Villages that aided them (+persistent relation)
  - Villages that attacked them (-persistent grudge)
  - Families that honored pacts (+inherited loyalty)
  - Bands that hunted them (-vendetta)

Mythical Creature Council:
  - Dragons may have relations with other dragons
  - Phoenix relations with mages' guilds
  - Unicorns with pure good entities only

Special mechanic:
  - Long-lived creatures accumulate century-spanning grudges
  - Ancient dragon has -90 relation with Village C (they killed his mate 200 years ago)
  - Current villagers innocent, but dragon doesn't care
  - Creates epic narrative quests (atone for ancestors' sins)
```

---

## Entity-God Relations

### Overview

**Every entity has a relation toward the player deity:**

```
RelationToGod: -100 to +100

Affects:
  - Prayer generation (high relation → more prayer)
  - Worship enthusiasm (festivals, temple attendance)
  - Obedience to divine commands (miracles, mandates)
  - Resistance to rival deities

Visibility:
  - Entity knows their own god relation
  - God (player) sees all entity relations (UI overlay)
  - Other entities infer from behavior (worship frequency)
```

### Initial God Relation (New Villagers)

**Calculated on birth or first god encounter:**

```
InitialGodRelation =
  ParentalInfluence +
  AlignmentCompatibility (entity vs god's perceived alignment) +
  VillageGodRelation (peer pressure) +
  FirstMiracle (if witnessed god action)

ParentalInfluence:
  - Parent's god relation × 0.5
  - If parent +80 god relation: Child starts +40
  - If parent -60 god relation: Child starts -30 (taught distrust)

AlignmentCompatibility:
  - If entity and god similar alignment: +20
  - If entity and god opposite: -30

  Example:
    Lawful Good villager + Lawful Good god: +20
    Lawful Good villager + Chaotic Evil god: -30

VillageGodRelation:
  - Average village relation to god
  - Peer pressure: +5 if village worships god
  - Peer pressure: -10 if village rejects god

FirstMiracle:
  - Beneficial miracle witnessed: +20 (Heal, Bless, Rain)
  - Harmful miracle witnessed: -30 (Lightning, Fire, Destruction)
  - Awe-inspiring miracle: +10 (any dramatic display)
```

**Example:**

```
Newborn villager Erik:
  Parents: Father +70 god relation, Mother +60 god relation
  ParentalInfluence: ((70 + 60) / 2) × 0.5 = +32.5

  Erik's alignment: Lawful Good (+60 moral, +70 order)
  God's perceived alignment: Lawful Good (based on past miracles)
  AlignmentCompatibility: +20

  Village average god relation: +50 (worshipful village)
  VillageGodRelation: +5

  FirstMiracle: God blessed Erik's birth (heal miracle on mother)
  FirstMiracle bonus: +20

  Total: 32.5 + 20 + 5 + 20 = +77.5

  Erik starts life as devoted worshipper (+77 god relation)

Narrative: "Erik was born blessed. His parents told him stories of the merciful god who saved his mother. He prayed every day."
```

### God Actions Affecting Relations

**Miracles:**

| Miracle Type | Target | Relation Change |
|-------------|--------|----------------|
| **Heal** | Individual healed | +20 |
| **Heal** | Witnesses (nearby) | +5 |
| **Bless** | Individual blessed | +15 |
| **Rain (during drought)** | Entire village | +10 |
| **Food (during famine)** | Entire village | +25 |
| **Lightning (smite enemy)** | Village (if enemy threatening) | +8 |
| **Lightning (smite villager)** | Targeted villager | -40 |
| **Lightning (smite villager)** | Witnesses (fear) | -15 |
| **Fire (burn enemy village)** | Own village (if warlike) | +5 |
| **Fire (burn own village)** | Entire village | -50 (betrayal) |
| **Earthquake (destroy buildings)** | Entire village | -30 (wrath) |
| **Resurrect dead** | Resurrected individual | +60 (life debt) |
| **Resurrect dead** | Family of resurrected | +30 |
| **Throw villager (harmful)** | Thrown villager | -25 |
| **Throw villager (playful, no harm)** | Thrown villager | +5 (god plays with us) |

**Divine Hand Interaction:**

```
Gentle Pickup (no harm):           +2
Hold for extended time:             +1/minute (enjoys presence)
Drop from height (injury):          -10
Fling to death:                     -80 (murdered by god)
  → Entire village: -20 (god is wrathful, fear)

Rescue from danger (pick up, move to safety): +25
```

**Divine Mandates:**

```
God commands via miracle UI or divine message:
  "Build temple"
  "Go to war"
  "Sacrifice livestock"

Entity response based on god relation:

God Relation +80 (Devoted):
  Obedience: 95% (will do anything)
  "The god commands, I obey without question."

God Relation +50 (Worshipful):
  Obedience: 70% (obeys reasonable requests)
  "The god has been good to us. I will do this."

God Relation +20 (Respectful):
  Obedience: 40% (obeys if beneficial)
  "The god asks, I will consider."

God Relation 0 (Neutral):
  Obedience: 10% (ignores unless forced)
  "I do not know this god. Why should I obey?"

God Relation -30 (Distrustful):
  Obedience: 0% (refuses, may flee)
  "This god has harmed us. I will not obey."

God Relation -60 (Hostile):
  Obedience: 0% (actively resists, sabotage)
  "This god is our enemy. I will resist."
```

### Outlook/Alignment Effects on God Relations

**Spiritual vs Materialistic:**

```
Spiritual Fanatic (+90 Spiritual):
  Base god relation: +30 (predisposed to worship)
  Miracle impact: +50% (blessings feel more meaningful)
  Prayer frequency: Daily
  Temple attendance: 100%

  Even if god is evil:
    "The god is wrathful, but divine. I worship in fear."

Materialistic Fanatic (+90 Materialistic):
  Base god relation: -10 (skeptical of divine)
  Miracle impact: +20% (needs proof)
  Prayer frequency: Rarely (only in crisis)
  Temple attendance: 0% (unless profitable)

  Requires transactional god:
    "Show me wealth, and I will worship."
```

**Warlike vs Peaceful:**

```
Warlike (+80 Warlike):
  Favors gods who:
    - Smite enemies (+15 relation)
    - Grant victory in battle (+20 relation)
    - Bless warriors (+10 relation)

  Dislikes gods who:
    - Refuse to help in war (-10 relation)
    - Heal enemies (-20 relation)
    - Demand peace (-15 relation)

Peaceful (+80 Peaceful):
  Favors gods who:
    - Heal sick (+20 relation)
    - Bless crops (+15 relation)
    - Stop wars (+25 relation)

  Dislikes gods who:
    - Smite innocent (-40 relation)
    - Demand war (-30 relation)
    - Destroy for no reason (-50 relation)
```

**Xenophobic vs Egalitarian:**

```
Xenophobic (+80 Xenophobic):
  Favors gods who:
    - Protect THEIR village (+20 relation)
    - Destroy OTHER villages (+10 relation)
    - Grant exclusive blessings (+15 relation)

  Dislikes gods who:
    - Help other villages (-20 relation)
    - Demand integration (-30 relation)
    - Bless foreigners (-15 relation)

Egalitarian (+80 Egalitarian):
  Favors gods who:
    - Help all villages equally (+20 relation)
    - Promote peace and trade (+15 relation)
    - Universal blessings (+10 relation)

  Dislikes gods who:
    - Play favorites (-20 relation)
    - Demand war on others (-25 relation)
    - Exclusive worship (-15 relation)
```

### God Relation Drift

**Passive Changes:**

```
If god frequently helps village:
  +2 relation/month for entire village (consistent benevolence)

If god ignores prayers:
  -1 relation/month (feels abandoned)

If god alternates good/evil miracles:
  Chaotic entities: +1 relation/month (enjoy unpredictability)
  Lawful entities: -2 relation/month (frustrated by inconsistency)

If god has been absent for 1 year:
  -5 relation for entire village (forgotten, doubts arise)

If rival deity appears and offers better miracles:
  -10 relation (tempted to switch allegiance)
```

### Entity Death & God Relation

**When entity dies:**

```
If high god relation (+60+):
  Afterlife: "Blessed" (good afterlife, rewarded)
  Family remembers: "God favored them."

If neutral god relation (0±30):
  Afterlife: Standard (no special treatment)
  Family remembers: "They lived and died."

If hostile god relation (-60-):
  Afterlife: "Cursed" (bad afterlife, punished)
  Family blames god: -10 relation to god
  Narrative: "The god killed them. We will not forgive."
```

### God Relation Examples

**Example 1: Benevolent God (Lawful Good)**

```
Entity: Marcus (Lawful Good villager)
God: Lawful Good deity (consistent blessings)

Year 1:
  Born: +77 (high parental influence, aligned)
  Age 5: Witnessed god heal plague (+10) → +87
  Age 10: God blessed harvest (+10 village) → +97
  Age 15: God resurrected Marcus's father (+30) → +100 (capped, Devoted)

  Marcus becomes priest, dedicates life to god
  Prays 3×/day, builds shrines, evangelizes

  When god commands "Build temple":
    Marcus: "I will build it with my own hands."

  When Marcus dies at age 70:
    Afterlife: Blessed
    Children inherit +50 god relation (father's devotion)

Narrative: "Marcus lived and died in service to the god. His children swore the same oath."
```

**Example 2: Wrathful God (Chaotic Evil)**

```
Entity: Elira (Peaceful Good villager)
God: Chaotic Evil deity (destructive, capricious)

Year 1:
  Born: +10 (neutral start, no strong influence)
  Age 3: God smites neighbor with lightning (-15 fear) → -5
  Age 8: God demands sacrifice, village refuses, god burns fields (-30) → -35
  Age 12: God randomly heals Elira's sick mother (+20) → -15 (confused)
  Age 15: God burns enemy village (Elira peaceful, dislikes) (-10) → -25

  Elira distrusts god, prays rarely
  When god commands "Go to war":
    Elira: "No. I will hide." (0% obedience)

  Age 20: God smites Elira for disobedience (lightning)
  Elira survives, maimed, relation: -80 (Hostile)

  Elira becomes heretic, spreads anti-god sentiment (-5 village relation/month)

  When Elira dies at age 40 (executed for heresy):
    Afterlife: Cursed (punished by god)
    Children inherit -40 god relation (mother's hatred)

Narrative: "Elira died hating the god. Her children vowed to resist. The cycle of hatred continued."
```

**Example 3: Neutral God (True Neutral)**

```
Entity: Kael (True Neutral merchant)
God: True Neutral deity (balanced, transactional)

Year 1:
  Born: +5 (neutral alignment match)
  Age 10: God helps village sometimes, ignores sometimes (0 net) → +5
  Age 20: God blesses Kael's trade caravan (+15) → +20

  Kael sees god as business partner:
    "I pray, god helps sometimes. Fair deal."

  When god commands "Donate to temple":
    Kael: "How much? What do I get in return?" (40% obedience, negotiable)

  Age 50: God hasn't helped in 5 years (-5) → +15

  Kael's god relation: Respectful but pragmatic
  When Kael dies at age 65:
    Afterlife: Standard (no special treatment)
    Children inherit +7 god relation (neutral)

Narrative: "Kael worshiped the god as he worshiped gold. Useful, but not sacred."
```

---

## Player Interaction

**Observable:**
- Villagers visibly interact (conversations, shared work, combat)
- Relations affect cooperation speed (friends work faster together)
- Hostile entities avoid each other or fight
- Close friends seek each other out

**Control Points:**
- **Indirect:** Player miracles affect mood (blessing → positive interactions)
- **Indirect:** Player education → Higher rationality (less predictable)
- **Direct (potential):** Player can bless friendships (+10 relation) or curse them (-10)

**Learning Curve:**
- **Beginner:** Notices villagers like/dislike each other
- **Intermediate:** Understands alignment affects relations
- **Expert:** Manipulates social networks (engineer alliances, create rivalries)

---

## Systemic Interactions

### Dependencies

**Alignment Framework:** ✅ Uses moral/order/purity axes for compatibility
**Behavioral Personality:** ✅ Uses vengeful/forgiving, bold/craven for modifiers
**Wealth & Social Dynamics:** ✅ Relations affect charity, undermining, elite politics
**Guild System:** ✅ Relations affect guild alliances, betrayals, wars
**Business & Assets:** ✅ Relations determine partnership willingness, trade
**Sandbox Villages:** ✅ Village relations drive humanitarian aid, alliances
**Individual Progression:** ✅ Initiative determines proactive aid/diplomacy

### Influences

**Family Dynamics:** Relations determine inheritance favoritism
**Economic Partnerships:** +50 relation required for business partnership
**Guild Alliances:** +30 relation between guild masters → alliance probable
**Marriage Arrangements:** +20 relation preferred, -20+ arranged marriages fail
**Warfare:** -50 relation → Open conflict, -80 → Assassination attempts
**Vassalization:** Relation determines loyalty, tribute compliance, rebellion risk
**Humanitarian Aid:** +40 relation triggers peacekeeper bands, +50 mobile hospitals
**God Worship:** God relation affects prayer generation, obedience, temple attendance
**Animal Companions:** +30 relation domesticates, +50 creates combat ally

### Synergies

**Relations + Wealth:** Wealthy friends donate to poor friends (charity)
**Relations + Alignment:** Pure Good + Pure Evil = unlikely devoted friendship (ironic narrative)
**Relations + Business:** Close friends franchise together (+10 trust bonus)
**Relations + Guilds:** Hostile guilds never ally, friendly guilds ally easily
**Relations + Villages:** High village relations enable federation, low relations trigger wars
**Relations + God:** High god relation villages generate more prayer power
**Relations + Fauna:** Dragons remember century-old grudges, ancient pacts with dynasties
**Relations + Espionage:** Spy missions create permanent grudges (-80 assassination), blackmail destroys trust

---

## Exploits

**Problem:** Player repeatedly blesses same entities to force +100 relation
- **Fix:** Diminishing returns (each blessing +10 → +5 → +2 → +1)
- **Cap:** Max +30 from divine intervention

**Problem:** Chaotic entities farm random swings for exploits
- **Fix:** Chaotic drift capped at ±10/month (can't jump -50 to +50 instantly)

**Problem:** Players force unlikely alliances for mechanical benefit
- **Fix:** Unlikely alliances (opposite alignments) have +20% failure rate on cooperation checks

---

## Tests

- [ ] **Unit Test:** Initial relation calculation matches formula
- [ ] **Unit Test:** Familial bonus overrides alignment opposition
- [ ] **Integration:** Good + Evil entities can reach +80 through shared battles
- [ ] **Integration:** Corrupt entities betray alliances if profitable
- [ ] **Emergent:** Fanatic personalities occasionally act out-of-character
- [ ] **Emergent:** Chaotic relations drift wildly, lawful relations stable
- [ ] **Stress:** 10,000 entities with 100 relations each = 1M relation pairs

---

## Performance

**Complexity:**
- Initial relation calculation: O(1) per pair
- Relation updates: O(n) where n = active relations per entity
- Periodic checks: O(n) where n = entities (randomized, not all at once)

**Max Entities:** 10,000 entities × 100 relations = 1,000,000 relations
**Update Frequency:**
- Active relations (same village): Every in-game day
- Inactive relations (different villages): Every in-game month
- Periodic checks: Staggered (1% of entities per tick)

**Optimization:**
- Store only met entities (sparse matrix)
- Update only active relations (same location)
- Batch periodic checks across ticks

---

## Visual Representation

### Relation Flow Diagram

```
[Entity A] ←→ [Relation Component] ←→ [Entity B]
     ↓                  ↓                    ↓
Alignment          Relation Value        Alignment
Personality       (-100 to +100)         Personality
  ↓                     ↓                     ↓
  └──> Modifiers → Initial Calc ←─────────────┘
            ↓
       Interactions
            ↓
     Relation Changes
            ↓
       Action Triggers
       (Charity, Betrayal,
        Alliance, Assassination)
```

### Relation Dynamics Over Time

```
Example: Peaceful Forgiving Paladin + Vengeful Evil Assassin

Initial: -17 (Unfriendly)
    ↓
Shared battle +5
    ↓
  -12 (Unfriendly)
    ↓
10 battles later (+50)
    ↓
  +38 (Friendly)
    ↓
Paladin forgives crime (+10)
    ↓
  +48 (Close Friends)
    ↓
Assassin saves Paladin's life (+20)
    ↓
  +68 (Strong Allies)
    ↓
20 years of loyalty (+15)
    ↓
  +83 (Devoted)

Narrative climax:
  Paladin: "You are evil, but you are my brother."
  Assassin: "I would die for you. I cannot explain why."
```

---

## Iteration Plan

**v1.0 (MVP - Individual Relations):**
- ✅ Basic relation storage (met entities only)
- ✅ Initial relation calculation (alignment + context)
- ✅ Simple relation thresholds (enemy, neutral, friend)
- ✅ Familial bonuses
- ✅ Relation-based cooperation (friends work together)
- ✅ Basic god relation (entity-deity)
- ❌ No periodic checks
- ❌ No rationality classes
- ❌ No chaotic drift
- ❌ No aggregate relations

**v2.0 (Enhanced - Aggregate & Hierarchical):**
- ✅ Full alignment compatibility formula
- ✅ Outlook compatibility
- ✅ Behavioral modifiers
- ✅ Rationality classes
- ✅ Periodic behavior checks (vengeance, worship, etc.)
- ✅ Chaotic drift
- ✅ Unlikely alliances (Pure Good + Pure Evil)
- ✅ Village-to-village relations
- ✅ Humanitarian aid (peacekeepers, mobile hospitals)
- ✅ Guild-to-guild relations
- ✅ Animal relations (simple fauna)
- ✅ God relation mechanics (miracles, worship, obedience)

**v3.0 (Complete - Advanced Diplomacy):**
- ✅ Complex drift formulas (debates increase relation)
- ✅ Reputation system integration
- ✅ Player deity relation manipulation
- ✅ Historical relation tracking (events log)
- ✅ Dynamic relation UI (relationship webs)
- ✅ Vassalization & protectorate systems
- ✅ Guardianship & wards (elite politics)
- ✅ Intelligent fauna (dragons, mythical creatures)
- ✅ Ancient pacts (century-spanning agreements)
- ✅ Rebel vassal mechanics
- ✅ Rival deity systems

---

## Open Questions

### Critical

1. **Relation update frequency?** Every day, week, or month?
   - **PROPOSAL:** Same village = daily, different village = monthly, inactive = yearly

2. **Max relations per entity?** Unlimited or cap at N?
   - **PROPOSAL:** No hard cap, but only track "significant" relations (±10 threshold)
   - Neutral strangers (0±9) pruned after 1 year no interaction

3. **Relation inheritance?** Do children inherit parent's relations?
   - **PROPOSAL:** Partial inheritance
     - Friend's child: +20 (parent vouching)
     - Enemy's child: -10 (sins of the father, but lighter)

4. **Death impact?** How do relations change when entity dies?
   - **PROPOSAL:**
     - Close friends: -20 morale for 1 year (grief)
     - Mortal enemies: +10 morale for 1 month (relief)
     - Family: Devastating (see wealth/social dynamics)

### Important

5. **Relation visibility?** Can entities see others' relations?
   - **PROPOSAL:** Partial visibility
     - Know family relations (observable)
     - Infer friend/enemy from behavior
     - Spies can reveal hidden relations (espionage)

6. **False friends?** Can entities hide true feelings?
   - **PROPOSAL:** Corrupt entities can fake relations
     - Displayed relation: +30 (Friendly)
     - True relation: -20 (Unfriendly)
     - Revealed by: Betrayal, truth spells, investigation

7. **Relation decay?** Do unused relations fade?
   - **PROPOSAL:** Yes, slow decay toward neutral
     - No interaction for 5 years: -1/year toward 0
     - Exception: Family (never decays)

8. **Group relations?** Do entities have relations with aggregates (families, guilds)?
   - **YES:** Same mechanics
     - Individual can be friend with Family entity (+50)
     - Individual can be enemy with Guild entity (-60)
     - Affects interactions with all members

### Nice to Have

9. **Jealousy mechanics?** Do close friends compete for attention?
   - **YES:** If Entity A is +80 with both B and C:
     - B and C start at -10 (jealousy)
     - Unless B and C also friends (+30+ own relation)

10. **Love vs Friendship?** Different relation types?
    - **PROPOSAL:** Single relation value, but tags:
      - Romantic: true/false
      - Familial: true/false
      - Professional: true/false
      - Same value (-100 to +100), different context

11. **Reputation cascade?** Do relations affect others' initial relations?
    - **YES:** "Friend of my friend" bonus
      - If A and B are friends (+50)
      - And B introduces C to A
      - A starts at +10 with C (B's vouching)

12. **Relation-based abilities?** Devoted allies grant combat bonuses?
    - **YES:** +80 relation = +10% combat effectiveness when fighting together
    - Loyalty bonus: Will not retreat if ally in danger

---

## DOTS Implementation Architecture

### Core Components

```csharp
namespace Godgame.Relations
{
    /// <summary>
    /// Relation between two entities.
    /// Sparse storage: Only store if entities have met.
    /// </summary>
    public struct EntityRelation : IBufferElementData
    {
        public Entity OtherEntity;              // Who this relation is with

        public sbyte RelationValue;             // -100 to +100
        public byte RelationTier;               // 0=MortalEnemy, 1=Hostile, 2=Unfriendly,
                                                // 3=Neutral, 4=Friendly, 5=CloseFriend, 6=Devoted

        // History tracking
        public uint FirstMetTick;
        public byte MeetingContext;             // How they met (0=neighbor, 1=combat, etc.)
        public uint LastInteractionTick;
        public ushort PositiveInteractions;     // Helpful actions
        public ushort NegativeInteractions;     // Harmful actions
        public ushort SharedExperiences;        // Battles, festivals, etc. together

        // Relation dynamics
        public sbyte AnnualDrift;               // -50 to +50 passive change per year
        public bool IsKinship;                  // Family relation (never decays)
        public bool IsRomantic;                 // Romance flag
        public bool IsProfessional;             // Work/business relationship

        // Hidden relations (for corrupt entities)
        public bool IsFake;                     // True if displayed ≠ true relation
        public sbyte TrueRelation;              // Actual feeling (if faking)
    }

    /// <summary>
    /// Rationality class determines out-of-character behavior probability.
    /// </summary>
    public struct RationalityClass : IComponentData
    {
        public enum Class : byte
        {
            Fanatic,        // 1% OOC chance/year
            Strong,         // 5%
            Moderate,       // 15%
            Weak,           // 30%
            Variable        // 50% (chaotic)
        }

        public Class CurrentClass;
        public uint LastCheckTick;
        public ushort CheckFrequencyTicks;      // How often to check (varies by class)
    }

    /// <summary>
    /// Tracks periodic behavior checks for out-of-character actions.
    /// </summary>
    public struct BehaviorCheck : IBufferElementData
    {
        public enum CheckType : byte
        {
            VengeanceCheck,     // Forgiving → Vengeful
            ForgivenessCheck,   // Vengeful → Forgiving
            WorshipCheck,       // Materialistic → Spiritual
            GreedCheck,         // Spiritual → Materialistic
            CourageCheck,       // Craven → Bold
            CautionCheck,       // Bold → Craven
            MercyCheck,         // Evil → Good
            CrueltyCheck        // Good → Evil
        }

        public CheckType Type;
        public uint NextCheckTick;
        public float SuccessProbability;        // % chance of OOC action
        public bool IsActive;                   // Currently checking
    }

    /// <summary>
    /// Relation event log for narrative tracking.
    /// </summary>
    public struct RelationEvent : IBufferElementData
    {
        public Entity OtherEntity;
        public uint EventTick;
        public FixedString64Bytes EventType;    // "combat_together", "betrayal", "rescue"
        public sbyte RelationChange;            // How much relation changed
        public sbyte RelationAfter;             // New relation value
    }

    /// <summary>
    /// Meeting context when two entities first meet.
    /// </summary>
    public struct MeetingContext
    {
        public enum Context : byte
        {
            VillageNeighbor,
            Workplace,
            FamilyIntroduction,
            FestivalSocial,
            CombatSameSide,
            CombatOpposingSides,
            Adventuring,
            Conscription,
            DiplomaticMeeting,
            CrimeVictim,
            RescueSalvation
        }

        public static sbyte GetContextOffset(Context context)
        {
            return context switch
            {
                Context.VillageNeighbor => 0,
                Context.Workplace => 10,
                Context.FamilyIntroduction => 20,
                Context.FestivalSocial => 5,
                Context.CombatSameSide => 15,
                Context.CombatOpposingSides => -30,
                Context.Adventuring => 10,
                Context.Conscription => 5,
                Context.DiplomaticMeeting => 0,
                Context.CrimeVictim => -50,
                Context.RescueSalvation => 40,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Alignment compatibility calculator.
    /// </summary>
    public struct AlignmentCompatibility
    {
        public static float CalculateMoralAxisDelta(sbyte moral1, sbyte moral2)
        {
            int delta = math.abs(moral1 - moral2);

            bool bothGood = moral1 > 30 && moral2 > 30;
            bool bothEvil = moral1 < -30 && moral2 < -30;
            bool opposite = (moral1 > 30 && moral2 < -30) || (moral1 < -30 && moral2 > 30);

            if (bothGood)
                return 20f - (delta * 0.2f);
            if (bothEvil)
                return 15f - (delta * 0.15f);
            if (opposite)
                return -(delta * 0.3f);

            return 0f;
        }

        public static float CalculateOrderAxisDelta(sbyte order1, sbyte order2)
        {
            int delta = math.abs(order1 - order2);

            bool bothLawful = order1 > 30 && order2 > 30;
            bool bothChaotic = order1 < -30 && order2 < -30;
            bool opposite = (order1 > 30 && order2 < -30) || (order1 < -30 && order2 > 30);

            if (bothLawful)
                return 15f - (delta * 0.1f);
            if (bothChaotic)
            {
                // Chaotic unpredictable
                Unity.Mathematics.Random rng = new Unity.Mathematics.Random((uint)UnityEngine.Time.time);
                return rng.NextFloat(-10f, 10f);
            }
            if (opposite)
                return -(delta * 0.2f);

            return 0f;
        }

        public static float CalculatePurityAxisDelta(sbyte purity1, sbyte purity2)
        {
            int delta = math.abs(purity1 - purity2);

            bool bothPure = purity1 > 30 && purity2 > 30;
            bool bothCorrupt = purity1 < -30 && purity2 < -30;
            bool opposite = (purity1 > 30 && purity2 < -30) || (purity1 < -30 && purity2 > 30);

            if (bothPure)
                return 10f - (delta * 0.05f);
            if (bothCorrupt)
                return -(delta * 0.1f); // Corrupt compete
            if (opposite)
            {
                // Pure sees corrupt negatively, corrupt doesn't care as much
                if (purity1 > 30)
                    return -(delta * 0.15f);
                else
                    return -(delta * 0.05f);
            }

            return 0f;
        }
    }

    /// <summary>
    /// Outlook compatibility calculator.
    /// </summary>
    public struct OutlookCompatibility
    {
        public static float Calculate(byte outlook1_1, byte outlook1_2, byte outlook1_3,
                                       byte outlook2_1, byte outlook2_2, byte outlook2_3)
        {
            float total = 0f;

            // Check all combinations for matches
            if (OutlooksMatch(outlook1_1, outlook2_1, out float bonus1)) total += bonus1;
            if (OutlooksMatch(outlook1_1, outlook2_2, out float bonus2)) total += bonus2;
            if (OutlooksMatch(outlook1_1, outlook2_3, out float bonus3)) total += bonus3;
            if (OutlooksMatch(outlook1_2, outlook2_1, out float bonus4)) total += bonus4;
            if (OutlooksMatch(outlook1_2, outlook2_2, out float bonus5)) total += bonus5;
            if (OutlooksMatch(outlook1_2, outlook2_3, out float bonus6)) total += bonus6;
            if (OutlooksMatch(outlook1_3, outlook2_1, out float bonus7)) total += bonus7;
            if (OutlooksMatch(outlook1_3, outlook2_2, out float bonus8)) total += bonus8;
            if (OutlooksMatch(outlook1_3, outlook2_3, out float bonus9)) total += bonus9;

            return total;
        }

        private static bool OutlooksMatch(byte outlook1, byte outlook2, out float bonus)
        {
            bonus = 0f;
            if (outlook1 == 0 || outlook2 == 0)
                return false;

            if (outlook1 == outlook2)
            {
                // VillagerOutlookType enum values (from existing docs)
                bonus = outlook1 switch
                {
                    1 => 15f,  // Warlike
                    2 => 10f,  // Peaceful
                    3 => 12f,  // Spiritual
                    4 => 8f,   // Materialistic
                    5 => 10f,  // Scholarly
                    6 => 15f,  // Xenophobic
                    7 => 10f,  // Egalitarian
                    8 => 5f,   // Authoritarian (compete)
                    _ => 0f
                };
                return true;
            }

            // Check oppositions
            if ((outlook1 == 1 && outlook2 == 2) || (outlook1 == 2 && outlook2 == 1))
            {
                bonus = -15f; // Warlike vs Peaceful
                return true;
            }
            if ((outlook1 == 3 && outlook2 == 4) || (outlook1 == 4 && outlook2 == 3))
            {
                bonus = -10f; // Spiritual vs Materialistic (but special rule: debates +2)
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Familial relation bonus calculator.
    /// </summary>
    public struct FamilialBonus
    {
        public enum RelationshipType : byte
        {
            None,
            ParentChild,
            Sibling,
            Spouse,
            GrandparentGrandchild,
            UncleAuntNephewNiece,
            Cousin,
            DynastyMember,
            FamilyIntroduction,
            DisownedFamily
        }

        public static sbyte GetBonus(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.ParentChild => 80,
                RelationshipType.Sibling => 60,
                RelationshipType.Spouse => 70,
                RelationshipType.GrandparentGrandchild => 50,
                RelationshipType.UncleAuntNephewNiece => 40,
                RelationshipType.Cousin => 30,
                RelationshipType.DynastyMember => 20,
                RelationshipType.FamilyIntroduction => 10,
                RelationshipType.DisownedFamily => -40,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Hierarchical relation component (vassalization, protectorate, guardianship).
    /// Attached to entities in hierarchical relationships.
    /// </summary>
    public struct HierarchicalRelation : IComponentData
    {
        public enum RelationType : byte
        {
            None,
            Vassalization,      // Overlord > Vassal
            Protectorate,       // Protector > Client
            Guardianship,       // Guardian > Ward
            Alliance,           // Equal partners
            Ward,               // Guardian > Minor child
            Adoption            // Full integration
        }

        public Entity OverlordEntity;           // Who is in power (or Entity.Null if equal)
        public Entity SubordinateEntity;        // Who is under authority
        public RelationType Type;

        public byte TributeRate;                // % of tax revenue (0-100)
        public byte Autonomy;                   // How much self-governance (0-100)
        public sbyte Relation;                  // -100 to +100 relation between parties
        public uint EstablishedTick;

        // Compliance tracking
        public byte TributeCompliance;          // 0-100% actual tribute paid
        public byte MilitarySupportCompliance;  // 0-100% troops provided when requested
        public byte RebellionRisk;              // 0-100% probability of rebellion
    }

    /// <summary>
    /// Village-to-Village relation component.
    /// Tracks aggregate relations between settlements.
    /// </summary>
    public struct VillageRelation : IBufferElementData
    {
        public Entity OtherVillageEntity;
        public sbyte RelationValue;             // -100 to +100
        public uint FirstContactTick;

        // Diplomacy flags
        public bool IsAllied;                   // Military alliance
        public bool IsTradingPartner;           // Economic cooperation
        public bool IsAtWar;                    // Open conflict

        // Aid tracking
        public uint LastAidSentTick;            // When last humanitarian aid sent
        public byte AidTypeLastSent;            // 0=Food, 1=Peacekeepers, 2=Hospital, etc.
    }

    /// <summary>
    /// Humanitarian aid mission component.
    /// Tracks active relief bands, mobile hospitals, etc.
    /// </summary>
    public struct HumanitarianAid : IComponentData
    {
        public enum AidType : byte
        {
            FoodShipment,
            PeacekeeperBand,
            MobileHospital,
            ReconstructionCrew,
            RefugeeShelter
        }

        public Entity SendingVillage;
        public Entity ReceivingVillage;
        public AidType Type;

        public uint DeployedTick;
        public uint PlannedEndTick;
        public ushort MonthlyCost;              // Currency cost to sending village

        // Personnel
        public byte DoctorCount;                // For mobile hospitals
        public byte PeacekeeperCount;           // For peacekeeper bands
        public byte WorkerCount;                // General workers

        // Effectiveness tracking
        public byte MortalityReductionPercent;  // For hospitals (0-100%)
        public ushort VillagersSaved;           // Lives saved
        public byte DeathsInService;            // Aid workers who died
    }

    /// <summary>
    /// Entity relation to player god.
    /// Every entity tracks their feeling toward the deity.
    /// </summary>
    public struct GodRelation : IComponentData
    {
        public sbyte RelationToGod;             // -100 to +100

        // Worship metrics
        public byte PrayerFrequency;            // 0=Never, 100=Constant
        public byte TempleAttendance;           // 0=Never, 100=Always
        public byte Obedience;                  // 0=Rebel, 100=Devout
        public uint LastPrayerTick;
        public uint LastMiracleWitnessed;

        // Inherited influence
        public sbyte ParentalInfluence;         // -100 to +100 from parents
        public sbyte PeerPressure;              // -50 to +50 from village average
    }

    /// <summary>
    /// Divine miracle effect log.
    /// Tracks how miracles affected entity's god relation.
    /// </summary>
    public struct DivineMiracleEffect : IBufferElementData
    {
        public enum MiracleType : byte
        {
            Heal,
            Bless,
            Rain,
            Food,
            Lightning,
            Fire,
            Earthquake,
            Resurrect,
            ThrowVillager
        }

        public MiracleType Type;
        public uint Tick;
        public sbyte RelationChange;            // How much god relation changed
        public bool WasDirectTarget;            // True if entity was target, false if witness
    }

    /// <summary>
    /// Animal relation component.
    /// Simpler relation system for fauna.
    /// </summary>
    public struct AnimalRelation : IBufferElementData
    {
        public Entity OtherEntity;              // Who this relation is with
        public sbyte RelationValue;             // -50 to +50 (simpler range)

        public ushort FeedingEvents;            // Times fed
        public ushort HarmEvents;               // Times harmed
        public ushort TamingAttempts;           // Taming interactions

        public bool IsDomesticated;             // Pet status (relation +30+)
        public bool WillDefend;                 // Combat ally (relation +50)
    }

    /// <summary>
    /// Intelligent fauna component (dragons, mythical creatures).
    /// Full relation system + aggregate relations.
    /// </summary>
    public struct IntelligentFaunaRelations : IComponentData
    {
        public ushort Age;                      // Years old (for long-lived creatures)
        public byte Intelligence;               // 0-100 (high = complex relations)

        // Aggregate relations enabled
        public bool CanHaveVillageRelations;    // Can have relations with villages
        public bool CanHaveDynastyRelations;    // Can track dynasty pacts
        public bool CanHaveBandRelations;       // Can remember bands/armies

        // Memory span (how long they remember)
        public ushort MemorySpanYears;          // Default: 100 for dragons, 10 for others
    }

    /// <summary>
    /// Ancient pact component (for intelligent fauna with dynasties).
    /// Tracks century-spanning agreements.
    /// </summary>
    public struct AncientPact : IBufferElementData
    {
        public Entity DynastyEntity;            // Which dynasty
        public uint EstablishedTick;            // When pact made
        public sbyte InheritedRelation;         // Relation passed to heirs
        public FixedString64Bytes PactTerms;    // "protect_dynasty", "tribute_gold", etc.
    }

    /// <summary>
    /// Guild-to-Guild relation component.
    /// </summary>
    public struct GuildRelation : IBufferElementData
    {
        public Entity OtherGuildEntity;
        public sbyte RelationValue;             // -100 to +100

        public ushort SharedMissions;           // Successful cooperations
        public ushort Betrayals;                // Times betrayed
        public bool IsAllied;                   // Formal alliance
        public bool IsAtWar;                    // Open conflict
    }
}
```

---

## System Flow: First Meeting

```csharp
[BurstCompile]
public partial struct EntityMeetingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Query entities in proximity (same location, same event, etc.)
        foreach (var (transform1, alignment1, personality1, entity1) in SystemAPI
            .Query<RefRO<LocalTransform>, RefRO<VillagerAlignment>, RefRO<VillagerBehavioralPersonality>>()
            .WithEntityAccess())
        {
            foreach (var (transform2, alignment2, personality2, entity2) in SystemAPI
                .Query<RefRO<LocalTransform>, RefRO<VillagerAlignment>, RefRO<VillagerBehavioralPersonality>>()
                .WithEntityAccess())
            {
                if (entity1 == entity2)
                    continue;

                // Check if already met
                var relations1 = state.EntityManager.GetBuffer<EntityRelation>(entity1);
                bool alreadyMet = false;
                foreach (var rel in relations1)
                {
                    if (rel.OtherEntity == entity2)
                    {
                        alreadyMet = true;
                        break;
                    }
                }

                if (alreadyMet)
                    continue;

                // Check proximity (must be close to meet)
                float distance = math.distance(transform1.ValueRO.Position, transform2.ValueRO.Position);
                if (distance > 10f) // Meeting distance threshold
                    continue;

                // Determine meeting context
                var context = DetermineMeetingContext(state, entity1, entity2);

                // Calculate initial relation
                sbyte initialRelation = CalculateInitialRelation(
                    alignment1.ValueRO,
                    alignment2.ValueRO,
                    personality1.ValueRO,
                    personality2.ValueRO,
                    context
                );

                // Create bidirectional relations
                CreateRelation(ref state, ref ecb, entity1, entity2, initialRelation, context);
                CreateRelation(ref state, ref ecb, entity2, entity1, initialRelation, context);
            }
        }
    }

    private MeetingContext.Context DetermineMeetingContext(ref SystemState state, Entity e1, Entity e2)
    {
        // Check various context flags
        // Same village? Same workplace? Combat? etc.

        // Simplified: Default to village neighbor
        return MeetingContext.Context.VillageNeighbor;
    }

    private sbyte CalculateInitialRelation(
        VillagerAlignment align1,
        VillagerAlignment align2,
        VillagerBehavioralPersonality pers1,
        VillagerBehavioralPersonality pers2,
        MeetingContext.Context context)
    {
        float total = 0f;

        // Context offset
        total += MeetingContext.GetContextOffset(context);

        // Alignment compatibility
        total += AlignmentCompatibility.CalculateMoralAxisDelta(align1.MoralAxis, align2.MoralAxis);
        total += AlignmentCompatibility.CalculateOrderAxisDelta(align1.OrderAxis, align2.OrderAxis);
        total += AlignmentCompatibility.CalculatePurityAxisDelta(align1.PurityAxis, align2.PurityAxis);

        // Outlook compatibility (simplified - would use actual outlook components)
        // total += OutlookCompatibility.Calculate(...)

        // Behavioral modifiers
        if (pers1.Forgiving > 60)
            total += 10f;
        if (pers1.Vengeful > 60)
            total -= 5f;

        // Random factor
        Unity.Mathematics.Random rng = new Unity.Mathematics.Random((uint)UnityEngine.Time.time);
        total += rng.NextFloat(-10f, 10f);

        // Clamp to valid range
        return (sbyte)math.clamp(total, -100, 100);
    }

    private void CreateRelation(ref SystemState state, ref EntityCommandBuffer ecb,
        Entity owner, Entity other, sbyte relationValue, MeetingContext.Context context)
    {
        var relations = state.EntityManager.GetBuffer<EntityRelation>(owner);

        relations.Add(new EntityRelation
        {
            OtherEntity = other,
            RelationValue = relationValue,
            RelationTier = GetRelationTier(relationValue),
            FirstMetTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
            MeetingContext = (byte)context,
            LastInteractionTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
            PositiveInteractions = 0,
            NegativeInteractions = 0,
            SharedExperiences = 0,
            AnnualDrift = 0,
            IsKinship = false,
            IsRomantic = false,
            IsProfessional = false,
            IsFake = false,
            TrueRelation = relationValue
        });
    }

    private byte GetRelationTier(sbyte value)
    {
        if (value <= -80) return 0; // Mortal Enemies
        if (value <= -50) return 1; // Hostile
        if (value <= -25) return 2; // Unfriendly
        if (value <= 24) return 3;  // Neutral
        if (value <= 49) return 4;  // Friendly
        if (value <= 79) return 5;  // Close Friends
        return 6;                   // Devoted
    }
}
```

---

## Related Documentation

- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](Wealth_And_Social_Dynamics.md) - Charity, undermining, family dynamics
- **Guild System:** [Guild_System.md](Guild_System.md) - Guild alliances, betrayals, member relations
- **Alignment Framework:** [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Moral/order/purity axes
- **Behavioral Personality:** [Villager_Behavioral_Personality.md](Villager_Behavioral_Personality.md) - Vengeful/forgiving, bold/craven
- **Business & Assets:** [BusinessAndAssets.md](../Economy/BusinessAndAssets.md) - Business partnerships, rivalries
- **Individual Progression:** [Individual_Progression_System.md](Individual_Progression_System.md) - Initiative, rationality
- **Elite Courts:** [Elite_Courts_And_Retinues.md](Elite_Courts_And_Retinues.md) - Courtier loyalty, hero integration
- **Spies & Double Agents:** [Spies_And_Double_Agents.md](Spies_And_Double_Agents.md) - Espionage relation penalties, seduction, blackmail
- **Individual Combat:** [../Combat/Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - Combat relation penalties, duels, grudges from killing

---

**For Designers:** Focus on tuning alignment compatibility formulas to ensure unlikely friendships (Pure Good + Pure Evil) are rare but possible through shared experiences.

**For Implementers:** Start with simple relation storage and initial calculation. Periodic checks and rationality classes can be added in v2.0 for narrative depth.

**For Narrative:** Relations create emergent stories of unlikely alliances, bitter betrayals, and lifelong friendships that transcend alignment boundaries. Track relation events for player-facing narratives.

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core formulas defined, ready for prototyping and tuning
