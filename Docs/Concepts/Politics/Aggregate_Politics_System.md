# Aggregate Politics System (Godgame)

## Overview
Aggregate entities (families, guilds, villages, bands, armies, peacekeepers) possess internal cohesion mechanics and external diplomatic relations. Group stability depends on ideological alignment of member entities, while external relations follow lifecycle paths from hostility through alliance to merger or dissolution.

**Integration**: Aggregate ECS (0.2 Hz) handles politics calculations, while Mind ECS (1 Hz) updates individual entity opinions and loyalties.

---

## Aggregate Entity Types

### Noble Houses & Dynasties
**Members**: Lords, ladies, heirs, retainers, household guards, servants
**Internal Cohesion**: Family loyalty, succession law, honor traditions
**External Relations**: Marriage alliances, feudal obligations, rivalries
**Governance**: Authoritarian (patriarch/matriarch decides)

**Cohesion Factors**:
- Succession disputes: -40% cohesion
- Honored tradition upheld: +20% cohesion
- Family member murdered: -60% cohesion, potential blood feud
- Successful marriage alliance: +15% cohesion

### Guilds & Trade Associations
**Members**: Master craftsmen, journeymen, apprentices, guild guards
**Internal Cohesion**: Shared trade, profit distribution, craft standards
**External Relations**: Trade agreements, supplier contracts, guild wars
**Governance**: Mixed (master council votes, guildmaster moderates)

**Cohesion Factors**:
- Fair profit sharing: +25% cohesion
- Master hoards profits: -35% cohesion
- Unethical practices (slavery, poison): -50% cohesion (good members), +20% (evil members)
- Successful monopoly: +30% cohesion (greed overcomes ethics temporarily)

### Religious Orders
**Members**: High priests, clerics, paladins, monks, acolytes
**Internal Cohesion**: Shared deity, dogma adherence, zealotry
**External Relations**: Proselytization, crusades, ecumenical councils
**Governance**: Authoritarian (divine hierarchy)

**Cohesion Factors**:
- Heretical leader: -80% cohesion, potential schism
- Miracle witnessed: +40% cohesion
- Allied with opposed alignment: -50% cohesion
- Convert 100+ souls: +20% cohesion

### Villages & Settlements
**Members**: Peasants, farmers, craftsmen, local militia
**Internal Cohesion**: Shared resources, defense needs, harvest success
**External Relations**: Feudal lord, trade with neighbors, bandit threats
**Governance**: Egalitarian (village council) or Authoritarian (appointed mayor)

**Cohesion Factors**:
- Successful harvest: +15% cohesion
- Famine: -40% cohesion
- Lord protects from bandits: +25% cohesion
- Lord ignores bandit raids: -60% cohesion, potential rebellion

### Mercenary Bands & Armies
**Members**: Captain, veterans, soldiers, camp followers
**Internal Cohesion**: Pay regularity, victory streak, loot distribution
**External Relations**: Contracts, rivalries, reputations
**Governance**: Authoritarian (captain commands)

**Cohesion Factors**:
- Regular pay: +20% cohesion
- Missed payday: -50% cohesion, potential mutiny
- Victory with loot: +35% cohesion
- Disastrous defeat: -60% cohesion
- Charismatic captain (CHA 80+): +15% base cohesion

### Peacekeepers & Town Guards
**Members**: Captain, sergeants, guards, investigators
**Internal Cohesion**: Shared duty, law enforcement, corruption level
**External Relations**: Local government, criminal syndicates, populace
**Governance**: Authoritarian (chain of command)

**Cohesion Factors**:
- Corrupt captain: -40% cohesion (lawful members)
- Justice served fairly: +25% cohesion
- Protect innocent successfully: +15% cohesion
- Innocent executed wrongly: -50% cohesion

### Criminal Syndicates
**Members**: Crime boss, enforcers, thieves, fences
**Internal Cohesion**: Fear of boss, profit sharing, code of silence
**External Relations**: Rival gangs, corrupt officials, black market
**Governance**: Authoritarian (boss rules by strength/fear)

**Cohesion Factors**:
- Boss shows weakness: -35% cohesion, potential coup
- Profitable heist: +30% cohesion
- Member snitches: -80% cohesion, execute traitor
- Rival gang encroaches: -20% cohesion (fear), +10% (rally against threat)

---

## Internal Politics: Cohesion Mechanics

### Base Cohesion Formula
```
Base Cohesion = Average(Member Loyalty) × Ideal Alignment × Leadership Quality

Member Loyalty = (Satisfaction with group + Ideological match + Personal bonds) / 3
Ideal Alignment = How well members' ethics/goals align (0.0 - 1.0)
Leadership Quality = Leader's CHA/100 + Governance match bonus
```

### Ideological Alignment

**Good-Aligned Guild with Evil Practices**:
```
Slave Trading Guild:
- Evil members: +20% cohesion (aligns with nature)
- Neutral members: -10% cohesion (uncomfortable but tolerate)
- Good members: -50% cohesion (fundamentally opposed)
- Infiltrators/spies: +0% cohesion (fake loyalty to gather intel)

Likely outcome: Good members leave, neutral members corrupted over time or depart
Result after 6 months: 80% evil, 15% neutral (corrupted), 5% spies
```

**Lawful Peacekeepers with Corrupt Captain**:
```
- Lawful Good guards: -60% cohesion, report to higher authority
- Lawful Neutral guards: -30% cohesion, internal conflict
- Neutral Evil guards: +20% cohesion, profit from corruption
- Chaotic members: -10% cohesion, don't care about hypocrisy

Likely outcome: Lawful members request transfer, quit, or report captain
Split chance: 40% if 6+ lawful members remain
```

### Member Opinion Divergence

**Opinion Topics**:
- Leadership competence (0-100)
- Profit/resource distribution fairness (0-100)
- Group's ethical practices (0-100)
- External threat response (0-100)
- Long-term goals alignment (0-100)

**Divergence Calculation**:
```csharp
// Example: Guild with 12 members voting on "Accept slavery contract"
Member Opinions:
8 members: Strongly Opposed (-80 to -60)
2 members: Neutral (-10 to +10)
2 members: Strongly Support (+60 to +80)

Average Opinion: -38 (overall opposition)
Opinion Standard Deviation: 54 (HIGH divergence)

Cohesion Penalty = Standard Deviation / 2 = -27% cohesion
```

**High Divergence (σ > 40)**: Group is fractured, potential split
**Medium Divergence (σ 20-40)**: Tense but stable
**Low Divergence (σ < 20)**: United, high cohesion

---

## External Politics: Diplomatic Relations

### Relationship Values
**Scale**: -100 (Blood Feud) to +100 (Merged/Vassalized)

**Relationship Tiers**:
- **-100 to -80**: Blood Feud (kill on sight, generational hatred)
- **-79 to -50**: Hostile (active conflict, sabotage)
- **-49 to -20**: Rival (competitive, distrust)
- **-19 to +19**: Neutral (indifferent, transactional)
- **+20 to +49**: Friendly (trade agreements, mutual defense pacts)
- **+50 to +79**: Allied (marriage alliances, resource sharing)
- **+80 to +99**: Confederated (joint leadership, integrated operations)
- **+100**: Merged (one entity)

### Diplomatic Weight

**Governance Type Impact**:

**Egalitarian Groups** (Councils, Democracies):
```
Village Council (12 members):
- Each villager has 1 vote
- Majority decides (7/12 = decision passes)
- Leader (Village Elder) has 1.5× vote weight
- Elders/respected members: 1.2× vote weight

Diplomatic Weight Distribution:
- Common peasant: 0.08 weight (1/12)
- Respected elder: 0.10 weight (1.2/12)
- Village elder (leader): 0.125 weight (1.5/12)

Member satisfaction: +30% cohesion (voices heard)
Decision speed: Slow (requires consensus)
```

**Authoritarian Groups** (Monarchies, Dictatorships):
```
Noble House (20 members):
- Lord/Lady: 0.80 weight (80% decision power)
- Heir: 0.10 weight (10% decision power)
- All other family: 0.10 weight combined (0.005 each)

Diplomatic Weight Distribution:
- Minor family member: 0.005 weight
- Heir: 0.10 weight
- Lord: 0.80 weight

Member satisfaction: Variable (loyal members +20%, oppressed members -40%)
Decision speed: Fast (leader decides instantly)
```

**Mixed Governance** (Guild Councils):
```
Craftsmen Guild (15 masters, 40 journeymen, 60 apprentices):
- Guildmaster: 0.30 weight
- Master council (15 masters): 0.60 weight combined (0.04 each)
- Journeymen: 0.08 weight combined
- Apprentices: 0.02 weight combined

Diplomatic Weight Distribution:
- Apprentice: 0.0003 weight (negligible)
- Journeyman: 0.002 weight (minimal)
- Master: 0.04 weight (significant)
- Guildmaster: 0.30 weight (dominant)

Member satisfaction: +15% cohesion (skilled members have voice)
Decision speed: Medium (council votes, guildmaster breaks ties)
```

### Relationship Lifecycle Paths

**Path 1: Neutral → Allied → Merged**
```
Year 1: Trading Guild A and B are neutral (relation: +5)
- Successful trade contracts: +10 relation
- Share market intelligence: +8 relation
Year 2: Now friendly (relation: +23)
- Joint expedition: +12 relation
- Marriage between guild families: +15 relation
Year 3: Now allied (relation: +50)
- Exclusive partnership: +10 relation
- Defend each other from rival guild: +15 relation
Year 5: Now confederated (relation: +85)
- Joint councils meet monthly: +5 relation
- Share all profits equally: +10 relation
Year 7: Merger vote passes (relation: +100)
- Guilds merge into "United Merchants Guild"
- Combined resources, membership, political power
```

**Path 2: Allied → Vassalization**
```
Year 1: Village A and Noble House B are allied (relation: +55)
- Village requests protection from bandits
- Noble House deploys troops: +10 relation
Year 2: Now strongly allied (relation: +65)
- Village swears fealty: Becomes vassal
- Village pays 20% taxes, receives military protection
- Relation becomes: +70 (Vassal/Liege)

Vassal mechanics:
- Liege obligation: Protect vassal, mediate disputes, grant privileges
- Vassal obligation: Pay taxes, provide levies, host liege
- Breaking oath: -80 relation, Blood Feud possible
```

**Path 3: Guaranteed Independence**
```
Year 1: Kingdom A and Free City B are allied (relation: +60)
- Free City fears annexation
- Kingdom grants "Charter of Eternal Freedom"
- Relation locked: +75 (Cannot merge, guaranteed independence)

Benefits:
- Free City: Never absorbed, self-governance preserved
- Kingdom: Loyal trading partner, defensive ally
- Both: Mutual defense pact, exclusive trade rights

Breaking guarantee: -100 relation with all witnesses, reputation destroyed
```

**Path 4: Allied → Split (Reversal)**
```
Year 1: Peacekeepers are united (internal cohesion: 80%)
Year 2: New captain is corrupt (cohesion drops to 45%)
- Lawful guards: Oppose corruption (-60% personal cohesion)
- Corrupt guards: Support corruption (+20% personal cohesion)
Year 3: Internal split (cohesion below 30%)
- 12 lawful guards resign, form "City Watch Loyalists"
- 8 corrupt guards remain with captain as "City Guard"
- Relation between factions: -60 (Hostile)

Result: One aggregate becomes two hostile aggregates
```

**Path 5: Merged → Split (Civil War)**
```
Year 1: United Kingdom (80 nobles, relation average: +65)
Year 3: Succession crisis (king dies, 2 heirs claim throne)
- Northern nobles support Heir A: 40 nobles
- Southern nobles support Heir B: 35 nobles
- Neutral nobles: 5 nobles
Year 4: Civil War (cohesion below 20%)
- Kingdom splits into "Northern Kingdom" and "Southern Kingdom"
- Relation: -80 (Blood Feud, civil war)
- Neutral nobles choose sides or get absorbed

Result: One aggregate becomes two hostile aggregates, potential Blood Feud
```

---

## Marriage Alliances (Dynasty Mechanics)

### Marriage Strategy

**High-Relation Dynasties (Relation +60 to +79)**:
```
House Valorian (Relation +68 with House Drakmor):

Marriage Motivation: Strengthen Bonds
- Lord Valorian offers daughter (Age 18, CHA 75, INT 62)
- Lord Drakmor offers son (Age 22, CHA 68, STR 80)
- Marriage agreed, relation increases: +68 → +78

Benefits:
- Shared inheritance claims (children inherit both houses if lines die)
- Military alliance strengthened (+10% troop support)
- Trade privileges (+15% commerce between houses)
- Potential merger path (if both lines unify through heirs)
```

**Trait Breeding (Genetic Goals)**:
```
House Silverblood (has "Magical Prodigy" trait, rare):
House Ironheart (has "Warrior's Resolve" trait, rare):

Relation: +55 (Allied but not deeply bonded)

Marriage Motivation: Combine Traits
- House Silverblood offers son (INT 88, WIS 76, Magical Prodigy)
- House Ironheart offers daughter (STR 85, CON 82, Warrior's Resolve)
- Marriage agreed, relation increases: +55 → +60

Children (4 offspring):
- Child 1: Inherits Magical Prodigy (40% chance) ✓
- Child 2: Inherits Warrior's Resolve (40% chance) ✓
- Child 3: Inherits BOTH traits (15% chance) ✓✓✓ (LEGENDARY HEIR)
- Child 4: Inherits neither (standard noble)

Result: Child 3 becomes legendary "Battle Mage" lineage founder
Both houses now invested in Child 3's success, relation: +70
```

**Political Marriage (Low Initial Relation)**:
```
House Blackwood (Relation +15 with House Thornhall):

Situation: Neutral houses, minor border disputes
Marriage Motivation: Peace Treaty

- Arranged marriage as part of peace negotiations
- House Blackwood offers daughter (Age 20, reluctant)
- House Thornhall offers son (Age 25, ambitious)
- Marriage agreed, relation increases: +15 → +35

Challenges:
- Loveless marriage: -10% cohesion for both houses (family sympathy for bride)
- Bride unhappy: 30% chance she influences husband against Blackwood
- Potential espionage: Bride becomes informant for Blackwood (if INT high)

Success Scenario (40% chance):
- Bride and groom develop genuine bond over 3 years
- Cohesion penalty removed, relation increases: +35 → +50
```

### Marriage Failure & Consequences

**Refused Marriage Proposal**:
```
House A (Relation +50 with House B):
- House A offers marriage alliance
- House B refuses (daughter already betrothed, or sees House A as inferior)
- Insult taken, relation drops: +50 → +35

Severe Refusal (public humiliation):
- Relation drops: +50 → +20
- House A may seek revenge or rival alliance
```

**Divorced/Annulled Marriage**:
```
House A and House B (married 8 years, relation +65):
- Husband (House B) annuls marriage (wife barren, or scandalous affair)
- Wife (House A) returns home in disgrace
- Relation crashes: +65 → +5
- Potential consequences:
  - Dowry dispute: -15 additional relation
  - Children custody battle: -20 additional relation
  - Wife's family demands justice: Relation → -30 (Rival)
```

**Widowing & Remarriage**:
```
House A and House B (married 12 years, relation +70):
- Wife dies in childbirth (tragic but natural)
- Relation maintained: +70
- Widower (House B) may remarry within House A (sister, cousin): +78 relation
- Or remarry elsewhere: Relation slowly decays: +70 → +50 over 5 years (no renewal)
```

---

## Group Splitting Mechanics

### Tension Accumulation

**Tension Sources**:
- Ideological conflict: +5 tension/month
- Unfair resource distribution: +8 tension/month
- Leadership incompetence: +10 tension/month
- External pressure (war, famine): +15 tension/month
- Fundamental moral disagreement: +20 tension/month

**Tension Reduction**:
- Address grievances: -10 tension/month
- Replace bad leader: -25 tension (instant)
- Successful group achievement: -15 tension (instant)
- Mediation by charismatic leader (CHA 80+): -12 tension/month

### Split Threshold

**Cohesion < 30% for 3+ months**: Split becomes possible

**Split Probability**:
```csharp
float splitChance = (100f - cohesion) * (tensionLevel / 100f) * leadershipFailureMultiplier;

// Example: Slave-trading guild with good members
Cohesion: 25%
Tension: 85/100
Leadership: Tyrant (multiplier 1.5)

SplitChance = (100 - 25) * (85/100) * 1.5 = 75 * 0.85 * 1.5 = 95.6%

Split occurs next check (95.6% > 80% threshold)
```

### Split Process

**1. Faction Identification**:
- Members cluster by opinion similarity
- Largest faction: Keeps original aggregate identity
- Smaller factions: Form new aggregates

**2. Resource Division**:
- Fair split (cohesion 20-30%): Resources divided proportionally
- Hostile split (cohesion < 20%): Contested, potential violence
- Leadership controls resources: Majority goes to leader's faction

**3. Relationship Initialization**:
```
Fair Split: Relation = +10 to +20 (Neutral, peaceful separation)
Contested Split: Relation = -20 to -40 (Rival, resentment)
Hostile Split: Relation = -50 to -70 (Hostile, blood shed during split)
```

**Example: Guild Split**:
```
Year 1: Merchant Guild (30 members, cohesion 28%)
Issue: Guildmaster wants to trade with slavers

Faction A (18 members, good-aligned): Oppose slavery
Faction B (12 members, neutral/evil-aligned): Support profit regardless

Split Process:
1. Faction A: Majority, keeps "Merchant Guild" identity
2. Faction B: Forms "Free Traders Consortium"
3. Resources: 60% to Faction A (18/30), 40% to Faction B (12/30)
4. Initial relation: +15 (Fair split, no violence)

Year 2 onwards:
- Merchant Guild refuses to trade with Free Traders (ethics)
- Relation slowly decays: +15 → 0 → -15 (Rival) over 3 years
```

---

## Relationship Lifecycle Endpoints

### 1. Merger (Unification)
**Requirements**:
- Relation ≥ +95
- Similar governance type
- Compatible ideologies
- Mutual vote passes (70%+ members approve)

**Process**:
```
Phase 1: Merger Proposal (Relation +95)
- Leaders meet, discuss terms
- Member vote scheduled

Phase 2: Member Vote
- Each member votes based on: Ideology match, resource benefit, leadership preference
- Egalitarian groups: Simple majority (51%)
- Authoritarian groups: Leader decides (may ignore members, cohesion risk)

Phase 3: Integration (if approved)
- Resources pooled
- Leadership unified (co-leaders, council, or one dominant leader)
- Members integrated
- New aggregate identity formed

Phase 4: Post-Merger Cohesion
- First 6 months: -20% cohesion (adjustment period)
- If integration succeeds: Cohesion recovers to 70%+
- If integration fails: Re-split possible (cohesion < 30%)
```

**Example**:
```
Blacksmiths Guild (25 members) + Armorers Guild (18 members):
Relation: +96
Vote: Blacksmiths 22/25 approve (88%), Armorers 16/18 approve (89%)

Merger Success:
- New identity: "Metalworkers Guild" (43 members)
- Resources: Combined forges, tools, contacts
- Leadership: Co-guildmasters (one from each original guild)
- Cohesion: 55% (adjustment period), rises to 75% by month 8
```

### 2. Vassalization (Subordination)
**Requirements**:
- Relation +40 to +79 (doesn't require highest relation)
- Power imbalance (one aggregate significantly stronger)
- Weaker aggregate needs protection OR stronger demands submission

**Voluntary Vassalization**:
```
Small Village (50 people) threatened by bandits:
- Approaches nearby Baron (Relation +45)
- Offers fealty in exchange for protection
- Baron accepts, relation increases: +45 → +65 (Liege/Vassal bond)

Vassal obligations:
- Taxes: 15-30% of production
- Levies: 10% of population in wartime
- Hosting: Provide lodging for liege when visiting

Liege obligations:
- Military protection
- Legal arbitration
- Infrastructure support (roads, bridges)
- Famine relief

Breaking vassalage (Vassal rebels):
- Requires relation ≤ +20 (severely degraded)
- Liege incompetence (fails to protect, excessive taxes)
- Result: Relation → -60 (Hostile), potential war
```

**Forced Vassalization** (Conquest):
```
Conquering Army defeats City:
- City surrenders, becomes vassal
- Initial relation: -20 (Resentful)
- High garrison required (20% occupation force)

Year 1-3: Occupation
- Harsh taxes: Relation -20 → -10 (resentment lessens with competent rule)
- Fair governance: Relation -10 → +10 (acceptance)
- Oppressive rule: Relation -10 → -40 (rebellion brewing)

Year 5+: Integration
- If relation reaches +40: Vassal becomes willing, garrison reduced
- If relation stays negative: Perpetual occupation, rebellion risk
```

### 3. Protectorate (Guaranteed Independence)
**Requirements**:
- Relation +60 to +85
- Weaker aggregate values sovereignty
- Stronger aggregate respects autonomy

**Protectorate Agreement**:
```
Free City (independent, wealthy, militarily weak):
- Relation +70 with neighboring Kingdom
- Fears annexation but values independence
- Proposes protectorate status

Agreement Terms:
- Free City: Maintains self-governance, no taxes to Kingdom
- Kingdom: Defends Free City, receives exclusive trade rights
- Relation locked: +75 (cannot merge without breaking agreement)

Benefits:
- Free City: Protected, sovereign
- Kingdom: Loyal trade partner, strategic location

Breaking protectorate:
- Kingdom annexes Free City: Relation → -100 (Blood Feud)
- Witnesses (other nations): -40 relation with Kingdom (oath-breaker)
- Kingdom's reputation: "Untrustworthy" trait for 20 years
```

### 4. Confederation (Joint Governance)
**Requirements**:
- Relation +80 to +94
- Not ready for full merger
- Shared external threats or goals

**Confederate Structure**:
```
Three merchant guilds form "Northern Trade Confederation":
- Guild A (30 members), Guild B (25 members), Guild C (20 members)
- Relation between all: +82 average
- Shared council: 5 representatives from each guild (15 total)

Confederate Powers:
- Joint military: Combined guards (75 total)
- Trade policy: Unified external tariffs
- Internal autonomy: Each guild governs own members

Cohesion: 70% (strong but not merged)

Path forward:
- Success: Relation increases → +95 → Merger proposal
- Failure: External shock (war, famine) strains cooperation → Dissolution
```

### 5. Dissolution (Collapse)
**Causes**:
- Cohesion < 15% for 6+ months
- Catastrophic failure (military defeat, bankruptcy, plague)
- Leadership assassination without successor
- Irreconcilable internal conflict

**Dissolution Process**:
```
Mercenary Band (40 members, cohesion 12%):
Cause: Captain killed, no successor, 3 months unpaid

Dissolution:
- Members disperse individually or in small groups
- Resources looted/divided chaotically
- No successor aggregate forms

Former members:
- 15 join other mercenary bands
- 10 become bandits
- 8 return home to villages
- 7 become solo adventurers

Original aggregate ceases to exist
```

### 6. Rebellion & Independence (Vassal Liberation)
**Requirements**:
- Vassal relation with liege ≤ +15
- Liege fails obligations (no protection, excessive taxes, tyranny)
- Vassal has military strength (or external ally)

**Rebellion Process**:
```
Year 1: Village Vassal (Relation +40 with Baron liege)
Year 2: Bandits raid village, Baron ignores plea for help
- Relation: +40 → +20 (liege incompetence)
Year 3: Baron demands double taxes for war
- Relation: +20 → +5 (exploitation)
Year 4: Village refuses taxes, declares independence
- Relation: +5 → -50 (Rebellion)

Rebellion War:
- Village allies with neighboring Count (Relation +55)
- Baron sends troops to suppress rebellion
- Count intervenes militarily
- Baron defeated

Outcome:
- Village independence achieved
- New vassal: Village swears to Count (Relation +60)
- Baron: Relation -80 (Blood Feud with village, -40 with Count)
```

---

## Espionage & Infiltration (Political Subversion)

### Spies in Evil Organizations

**Scenario: Good infiltrator in slave-trading guild**:
```
Infiltrator Profile:
- Alignment: Lawful Good
- Cover: Fake alignment as Neutral Evil
- Goal: Gather evidence, sabotage operations, free slaves

Infiltration Mechanics:
- Base detection chance: 15%/month (evil members suspicious)
- CHA/Deception skill: -8% detection (skilled liar)
- Perform evil acts to maintain cover: -5% detection, +10 tension (moral conflict)
- Refuse evil acts: +15% detection (suspicious behavior)

Monthly Check:
Detection Chance: 15% - 8% - 5% = 2%/month (performs token evil acts)

Moral Stress:
- Each evil act: +5 mental stress
- Accumulation: 50 stress = moral crisis (must commit major evil or blow cover)

Endgame:
1. Successfully gather evidence (12 months), escape, expose guild → Guild dissolved
2. Detected before escape → Executed, infiltration failed
3. Moral crisis → Refuse major evil act → Detected, but frees slaves during escape
```

### Political Sabotage

**Saboteur in Noble House**:
```
Saboteur Profile:
- Disguised as house servant
- Goal: Destabilize house from within

Sabotage Actions:
- Spread rumors: -5% cohesion/month
- Forge documents (false affairs, embezzlement): -15% cohesion if believed
- Poison food (non-lethal): -10% cohesion, blame falls on cook
- Assassinate heir: -40% cohesion, succession crisis

Detection:
- Investigation initiated if cohesion drops >20% in 3 months
- INT/Investigation checks by house steward
- If detected: Execute saboteur, but damage already done
```

---

## ECS Integration

### Aggregate ECS (0.2 Hz) - Politics Calculations

**Systems**:
- `AggregateRelationUpdateSystem`: Update relations between aggregates
- `AggregateCohesionCalculationSystem`: Calculate internal cohesion
- `AggregateLifecycleSystem`: Handle mergers, splits, vassalization
- `AggregateDiplomaticWeightSystem`: Calculate diplomatic power distribution
- `AggregateMarriageSystem`: Arrange marriages, calculate trait inheritance

**Components**:
```csharp
public struct AggregateRelationComponent : IComponentData
{
    public Entity TargetAggregate;
    public float RelationValue;        // -100 to +100
    public RelationType Type;          // Neutral, Friendly, Allied, Vassal, etc.
    public float TensionLevel;         // 0-100
    public float MonthsSinceLastEvent;
}

public struct AggregateCohesionComponent : IComponentData
{
    public float CohesionPercent;      // 0-100%
    public float IdeologicalAlignment; // 0-1 (how well members agree)
    public float LeadershipQuality;    // 0-1
    public float MemberSatisfaction;   // 0-100
    public float TensionLevel;         // 0-100
    public GovernanceType Governance;  // Egalitarian, Authoritarian, Mixed
}

public struct AggregateMemberOpinionComponent : IComponentData
{
    public Entity MemberEntity;
    public float LeadershipOpinion;    // 0-100
    public float EthicsOpinion;        // 0-100
    public float ResourceFairness;     // 0-100
    public float LoyaltyLevel;         // 0-100
    public float DiplomaticWeight;     // 0-1 (voting power)
}

public struct AggregateSplitRiskComponent : IComponentData
{
    public float SplitProbability;     // 0-1
    public int FactionCount;           // Number of emerging factions
    public Entity DominantFaction;     // Largest faction entity
    public float MonthsBelowThreshold; // Months cohesion < 30%
}

public struct DynastyMarriageComponent : IComponentData
{
    public Entity SpouseEntity;
    public Entity SpouseHouse;
    public float MarriageSatisfaction; // 0-100
    public int ChildrenCount;
    public bool ArrangedMarriage;
    public int YearsMarried;
}
```

### Mind ECS (1 Hz) - Individual Political Opinions

**Systems**:
- `EntityOpinionUpdateSystem`: Update individual opinions of aggregate leadership
- `EntityLoyaltySystem`: Calculate loyalty to aggregate
- `EntityFactionAlignmentSystem`: Determine which faction entity supports during splits

**Components**:
```csharp
public struct EntityPoliticalOpinionComponent : IComponentData
{
    public Entity AggregateEntity;     // Which aggregate this entity belongs to
    public float LeadershipOpinion;    // Opinion of current leader (0-100)
    public float GroupEthicsOpinion;   // Opinion of group's moral practices (0-100)
    public float LoyaltyToGroup;       // 0-100
    public float DissatisfactionLevel; // 0-100 (inverse of satisfaction)
}

public struct EntityFactionAffinityComponent : IComponentData
{
    public Entity PreferredFaction;    // Which faction entity supports if split occurs
    public float FactionLoyalty;       // 0-100
    public bool WillingToSplit;        // Would leave aggregate if split happens
}
```

---

## Example Scenarios

### Scenario 1: Guild Merger Success
```
Year 0: Blacksmiths Guild (20 members) and Armorers Guild (15 members)
- Relation: +55 (Friendly, trade agreements)

Year 1: Joint project (craft armor for lord's army)
- Success: +12 relation → +67 (Allied)
- Profit shared fairly: Both guilds +15% cohesion

Year 2: Propose confederacy
- Vote: Blacksmiths 18/20 approve, Armorers 14/15 approve
- Confederacy formed: Relation +67 → +82

Year 3-4: Confederate operations
- Joint guild hall built: +5 relation → +87
- Shared apprentice program: +4 relation → +91
- Combined political influence: Win city council seats

Year 5: Merger proposal
- Relation: +91 → +96 (after successful year)
- Vote: Blacksmiths 19/20 approve, Armorers 15/15 approve
- Merger approved

Year 6: "Metalworkers Guild" (35 members)
- Combined resources, political power doubled
- Cohesion: 60% (adjustment), rises to 78% by year 7
- Dominates city metalwork industry
```

### Scenario 2: Vassal Rebellion
```
Year 0: Village (vassal of Baron, relation +50)
- Taxes: 20%, Protection: Regular patrols

Year 2: Baron goes to war, recalls troops
- Village undefended
- Bandits raid village (20 dead, resources stolen)
- Relation: +50 → +30 (liege failed protection obligation)

Year 3: Baron demands 40% war tax
- Village protests
- Baron threatens military enforcement
- Relation: +30 → +10 (exploitation)

Year 4: Village refuses tax payment
- Baron sends soldiers
- Village allies with neighboring Count (relation +60)
- Count intervenes, defeats Baron's force
- Village declares independence, swears to Count
- New relation: Village → Baron -70 (Blood Feud), Village → Count +65 (Vassal)

Year 5: Baron plots revenge
- Hires mercenaries to raid village
- Count's troops ambush mercenaries
- Relation: Baron → Count -50 (Hostile)
```

### Scenario 3: Noble House Split (Succession Crisis)
```
Year 0: House Valorian (30 nobles, cohesion 75%)
- Lord Valorian (Age 68, CHA 72, 2 sons)

Year 1: Lord Valorian dies unexpectedly (assassinated)
- Elder son claims throne (Primogeniture tradition)
- Younger son contests (more charismatic, CHA 85 vs 62)

Factions form:
- Traditionalist Faction (18 nobles): Support elder son (primogeniture law)
- Charismatic Faction (10 nobles): Support younger son (better leader)
- Neutral (2 nobles): Undecided

Year 2: Internal conflict
- Cohesion: 75% → 35% (succession dispute)
- Tension: 70/100
- Traditionalists control family castle
- Charismatics control western estates

Year 3: Civil War
- Cohesion: 35% → 18% (armed conflict)
- Split occurs:
  - House Valorian (Elder son, 18 nobles, keeps castle)
  - House Valorian-Renewed (Younger son, 10 nobles, western estates)
- Relation: -75 (Blood Feud, civil war)

Year 5: Uneasy peace
- Border skirmishes continue
- Marriage alliances with external houses (both seek allies)
- Relation: -75 → -55 (still hostile, but exhausted)

Year 10: Reconciliation attempt (external threat unifies them)
- Dragon attacks region
- Both houses cooperate to defeat dragon
- Relation: -55 → -20 (Rival, but cooperation possible)
```

---

## Key Design Principles

1. **Cohesion Is King**: Internal cohesion determines aggregate stability; low cohesion (<30%) enables splits, high cohesion (>70%) enables mergers
2. **Ideological Alignment Matters**: Groups with conflicting ethics (good members in evil organizations) suffer cohesion penalties
3. **Governance Affects Voice**: Egalitarian groups empower members (slow decisions, high satisfaction), Authoritarian groups concentrate power (fast decisions, variable satisfaction)
4. **Relations Are Dynamic**: Every interaction modifies relations; positive feedback loops lead to mergers, negative loops lead to Blood Feuds
5. **Marriage Is Political**: Dynasties use marriage to strengthen alliances, breed superior heirs, or secure peace
6. **Lifecycle Is Bidirectional**: All endpoints (merger, vassalization, independence) can reverse under right conditions
7. **Espionage Subverts**: Spies and saboteurs can destabilize aggregates from within, but risk detection and moral stress
8. **External Pressure Unifies or Fractures**: Shared threats can unite (rally effect) or divide (blame/fear); depends on leadership and cohesion

---

**Integration with Other Systems**:
- **Soul System**: Transferred souls may retain loyalties to old aggregates, creating espionage opportunities
- **Blueprint System**: Guild mergers combine design libraries, increasing production capabilities
- **Infiltration Detection**: Detecting spies within aggregate requires Mind ECS investigation checks
- **Crisis Alert States**: External threats modify cohesion (rally around flag +15%, or panic/blame leadership -25%)
