# Villager Behavioral Personality System

**Status:** Draft - Concept Design
**Category:** Villager Behavior & Autonomy
**Scope:** Per-Villager Personality Traits
**Created:** 2025-11-06
**Last Updated:** 2025-11-06

---

## Purpose

**Primary Goal:** Transform villagers from monotonous NPCs into individuals with emergent life stories driven by personality, opportunity, and initiative.

**Core Innovation:** Initiative determines **when** villagers act autonomously; behavioral traits determine **what** they choose to do.

**Key Principle:** Two villagers with identical initiative scores will pursue completely different life paths based on their personalities.

---

## Core Concept: Initiative as Action Rate

### What Initiative Controls

Initiative governs **life-changing autonomous decisions**, not work efficiency or combat speed:

- **Family Formation:** When does a villager seek a partner and start a family?
- **Career Changes:** When does a gatherer decide to become a merchant?
- **Business Ventures:** When does someone open a shop, workshop, or tavern?
- **Adventure Seeking:** When does someone leave safety to explore or join a band?
- **Social Actions:** When does someone organize festivals, mediate disputes, or form factions?
- **Revenge Quests:** When does someone act on a grudge?
- **Major Relocations:** When does someone migrate to a new village or found their own?

### Initiative as a Throttle

```
High Initiative (0.8+)
→ Rapid life decisions
→ Frequent autonomous actions
→ "Restless" personality
→ Acts on opportunities immediately

Medium Initiative (0.4-0.6)
→ Measured decisions
→ Waits for right moment
→ "Patient" personality
→ Evaluates before acting

Low Initiative (0.0-0.3)
→ Slow to act
→ Rarely initiates change
→ "Passive" personality
→ Only acts when pressured
```

**Example:**
- Village base initiative: 0.50 (measured band)
- Bold villager (+60 boldness): 0.50 + 0.12 = **0.62**
- Craven villager (-60 boldness): 0.50 - 0.10 = **0.40**
- Result: Bold villager initiates major life changes ~55% more frequently

---

## Behavioral Axes

### Axis 1: Vengeful ↔ Forgiving
**What it measures:** Response to harm, betrayal, and loss

```
Vengeful (-100) ←―――――――[0]―――――――→ Forgiving (+100)
     ↓                    ↓                      ↓
"Never forget"      "Pragmatic"         "Let it go"
```

**Vengeful Traits (-100 to -40):**
- Holds grudges for extended periods
- Seeks retribution for wrongs
- Remembers who caused harm
- Prioritizes payback over efficiency
- Forms grudge-holder factions

**Neutral Traits (-40 to +40):**
- Pragmatic response to harm
- Forgives if compensated
- Moves on after justice served
- Proportional retaliation

**Forgiving Traits (+40 to +100):**
- Lets go of grievances quickly
- Seeks reconciliation
- Prioritizes harmony over justice
- Offers second chances
- Bonds with former enemies after apology

---

### Axis 2: Bold ↔ Craven
**What it measures:** Response to danger and risk tolerance

```
Craven (-100) ←―――――――[0]―――――――→ Bold (+100)
     ↓                    ↓                    ↓
"Flee first"       "Calculate"         "Charge forward"
```

**Craven Traits (-100 to -40):**
- Flees at first sign of danger
- Avoids risky tasks
- Prioritizes survival over glory
- Cautious in combat (defensive stance)
- Seeks safe, fortified locations

**Neutral Traits (-40 to +40):**
- Assesses risk/reward
- Stands ground if odds favorable
- Takes calculated risks
- Balanced combat tactics

**Bold Traits (+40 to +100):**
- Seeks dangerous challenges
- Volunteers for risky tasks
- Prioritizes glory/honor over safety
- Aggressive in combat (offensive stance)
- Attracted to frontier opportunities

---

## How Behavior Shapes Initiative Expression

**Key Principle:** Behavioral axes don't change initiative **rate**, they change initiative **targets**.

### Example: Same Initiative (0.70), Different Personalities

**Villager Kael: Bold (+70) + Forgiving (+60)**
- Initiative triggers every ~3 days
- Actions chosen:
  - Volunteers for dangerous scouting mission
  - Organizes reconciliation feast after village conflict
  - Proposes to partner quickly (2 weeks of courtship)
  - Opens risky frontier trading post

**Villager Mira: Craven (-70) + Vengeful (-80)**
- Initiative triggers every ~3 days (same rate!)
- Actions chosen:
  - Plots covert revenge against enemy (sets traps)
  - Stockpiles resources for personal survival cache
  - Avoids courtship (too vulnerable), focuses on security
  - Opens well-defended pawn shop in town center

**Both act frequently, but pursue completely different life paths.**

---

## Action Selection: Weighted Decision Tables

When initiative triggers, villager evaluates available actions weighted by personality:

### Bold Villager Weights
```
Available Actions:
  [Adventure Quest]        Weight: 10.0  (seeks danger)
  [Start Family]           Weight: 5.0   (commits quickly)
  [Open Risky Business]    Weight: 8.0   (frontier ventures)
  [Challenge Authority]    Weight: 6.0   (confrontational)
  [Join Military Band]     Weight: 9.0   (combat appeal)
  [Forgive Enemy]          Weight: 3.0   (if also forgiving)

→ Weighted random roll → Likely picks adventure or military
```

### Craven Villager Weights
```
Available Actions:
  [Adventure Quest]        Weight: 0.5   (avoids danger)
  [Start Family]           Weight: 7.0   (seeks stable bonds)
  [Open Safe Business]     Weight: 8.0   (town center shop)
  [Challenge Authority]    Weight: 0.2   (too risky)
  [Join Military Band]     Weight: 0.1   (terrifying)
  [Plot Revenge]           Weight: 9.0   (if vengeful - safe option)

→ Weighted random roll → Likely picks business or covert actions
```

---

## Grudge System (Vengeful Mechanic)

### Grudge Generation

**When wronged:**
```
IF VengefulScore < -20:  // Vengeful personality
  Generate Grudge:
    Intensity = HarmSeverity × VengefulMagnitude
    DecayRate = 0.01 × (100 + VengefulScore) per day

  Example (Friend killed by bandit):
    Severity: 80
    Vengeful: -70
    → Intensity: 56
    → Decay: 0.3 per day
    → Grudge lasts ~187 days if unresolved
```

**When wronged (forgiving):**
```
IF VengefulScore > +40:  // Forgiving personality
  Generate Grudge:
    Intensity = HarmSeverity × 0.3  // Dampened
    DecayRate = 2.0 per day         // Rapid fade

  Example (Friend killed):
    Severity: 80
    Forgiving: +60
    → Intensity: 24
    → Decay: 2.0 per day
    → Grudge lasts ~12 days naturally
```

### Grudge Effects on Initiative

```
Active Grudge Present:
  InitiativeBoost = Intensity × 0.002

Example:
  Base Initiative: 0.50
  Grudge Intensity: 60
  → Boosted to: 0.62 (+24% action frequency)

Result: Villager acts more frequently while seeking revenge
```

### <OPEN QUESTION> Permanent Personality Drift?

**Option A:** Trauma causes permanent shift
- First trauma: VengefulScore shifts -5 to -10
- Multiple traumas: Cumulative drift (cap at ±30 lifetime)
- Creates "hardened veterans" and "broken survivors"

**Option B:** Temporary obsession only
- Grudge active: Behavior shifts temporarily
- Grudge resolved/faded: Returns to baseline
- No permanent personality change

**Option C:** Conditional drift
- Depends on how trauma resolves (justice vs ignored)
- Successful revenge: +5 permanent vengeful
- Denied justice: +10 permanent vengeful
- Accepted apology: +5 permanent forgiving

**Current Status:** Undecided - user flagged uncertainty

---

## Combat Behavior Modulation

### Bold Combat Modifiers
```
Behavioral Adjustments:
  - Engage Range: +50% (charges earlier)
  - Retreat Threshold: 30 HP (fights longer)
  - Target Priority: Strongest enemy (seeks glory)
  - Formation Position: Front line preference
  - Dodge Chance: -10% (less defensive)
  - Damage Output: +15% (aggressive swings)
  - Morale Aura: +5 to nearby allies
```

### Craven Combat Modifiers
```
Behavioral Adjustments:
  - Engage Range: -50% (waits for enemies)
  - Retreat Threshold: 60 HP (flees early)
  - Target Priority: Weakest/isolated (safe kills)
  - Formation Position: Rear line preference
  - Dodge Chance: +20% (highly defensive)
  - Damage Output: -10% (cautious swings)
  - Morale Aura: -5 to nearby allies (fear spreads)
```

**Note:** These modify combat stance/tactics, NOT attack speed or simulation rate.

---

## Life Decision Examples

### Starting a Family

```
Eligible villager checks every initiative cycle:

Base Threshold: 0.60
Modifiers:
  - Village prosperity: -0.15
  - Available partners: -0.10
  - Housing available: -0.20
  - Age factor: -0.05 per decade over 20

IF Initiative > Adjusted Threshold:
  → Begin courtship
```

**Behavioral Influence:**
- **Bold:** Pursues courtship aggressively, proposes quickly
- **Craven:** Cautious courtship, long evaluation period
- **Vengeful:** Avoids courtship if active grudge exists (distracted)
- **Forgiving:** Willing to partner with former rivals after reconciliation

---

### Opening a Business

```
Villager with capital + discipline checks:

Base Threshold: 0.70
Modifiers:
  - Available location: -0.15
  - Market demand: -0.10
  - Competition: +0.20
  - Risk level: +0.15 (bold) or -0.15 (craven)

IF Initiative > Adjusted Threshold:
  → Found business entity
```

**Business Type by Behavior:**

**Bold merchants choose:**
- Frontier trading posts (high risk/reward)
- Luxury goods (volatile markets)
- Exploration supply stores

**Craven merchants choose:**
- Town bakeries (steady demand)
- Armor repair shops (defensive niche)
- Savings & loan (low physical risk)

---

### Seeking Adventure

```
Able-bodied villager checks:

Base Threshold: 0.75
Modifiers:
  - Village safety: -0.10
  - Personal obligations: +0.30 (family)
  - Reputation potential: -0.15
  - Bold influence: -0.002 × BoldScore

IF Initiative > Adjusted Threshold:
  → Join adventuring band
```

**Effective thresholds:**
- Bold (+70): ~0.60 (volunteers frequently)
- Craven (-70): ~0.90 (almost never)

---

## Example Scenarios

### The Grudge War (Vengeful + Bold)

**Villager Kael:**
- Vengeful: -80, Bold: +60
- Initiative: 0.65

**Event:** Friend murdered by bandit Roth

**Response Chain:**
1. **Day 0:** Grudge generated, intensity 70
2. **Day 1:** Initiative spikes to 0.79 (grudge boost)
3. **Day 2:** Abandons gatherer job, forms hunting party (bold = recruits openly)
4. **Day 5:** Tracks Roth to hideout, confronts directly (bold = no ambush)
5. **Day 7:** Captures Roth, demands village trial (lawful alignment influence)
6. **If justice served:** Grudge clears, initiative returns to 0.65
7. **If denied:** Grudge intensifies to 85, may break law to kill Roth

**Village Impact:**
- Other vengeful villagers rally (+morale)
- Forgiving villagers disturbed (-morale)
- Village reputation: "Justice-Seeking"

---

### The Unlikely Hero (Craven + Forgiving)

**Villager Finn:**
- Craven: -60, Forgiving: +50
- Initiative: 0.42

**Event:** Child trapped in burning building during raid

**Response Chain:**
1. **Immediate instinct:** Flee (craven default)
2. **Alignment override:** Chaotic Good + child in danger → moral conflict
3. **Initiative spike:** Crisis boost +0.30 → 0.72 (temporary heroism)
4. **Action:** Runs into fire despite terror
5. **Outcome:** Saves child, suffers burns, permanent scar

**<OPEN QUESTION> Aftermath:**
- Does Finn permanently become braver (bold +15)?
- Was this one-time alignment override?
- Does village reaction determine drift (celebrated = change, forgotten = none)?

**Village Impact:**
- Unexpected hero story spreads
- Other craven villagers inspired
- Cultural memory: "Even the afraid can be brave"

---

## Village Aggregate Effects

Villages track behavioral averages:

```csharp
VillageBehaviorProfile : IComponentData {
    float AverageVengeful;
    float AverageBold;
    byte ExtremeVengefulCount;  // Count > |60|
    byte ExtremeBoldCount;      // Count > |60|
}
```

### Cultural Drift Through Migration

**Bold Village (avg +40):**
- Attracts risk-takers (migration preference)
- Expansion initiatives +25%
- Forms military bands frequently
- Defensive preparedness -15% (overconfident)

**Craven Village (avg -40):**
- Attracts refugees seeking safety
- Heavy fortification investment
- Expansion initiatives -30%
- Surrenders earlier in sieges

**Vengeful Village (avg -40):**
- Long feuds with neighbors
- Raid/retaliation cycles
- Diplomacy penalty (-15 trust)
- Prayer tone: "Smite our enemies!"

**Forgiving Village (avg +40):**
- Accepts enemy refugees
- Reconciliation diplomacy
- Diplomacy bonus (+20 trust)
- Prayer tone: "Bless all peoples"

---

## Integration With Existing Systems

### Alignment/Outlook System
✅ **Orthogonal (Independent)**
- Alignment = WHAT you value morally
- Behavior = HOW you respond emotionally
- Example: Lawful Good + Vengeful = seeks justice relentlessly through legal means
- Example: Chaotic Evil + Forgiving = respects strength, moves on from conflict

### Village Initiative Bands
✅ **Extends Individual Expression**
- Village sets base initiative (Slow/Measured/Bold/Reckless)
- Individual behavioral traits modify personal rate
- Creates variance: villagers don't act in lockstep

### Job/Discipline System
✅ **Governs Autonomous Actions Beyond Work**
- Discipline = what they do for work (gatherer, smith)
- Initiative + Behavior = what they do for themselves (business, romance, revenge)

### Combat System
✅ **Modulates Tactics, Not Speed**
- Bold/Craven affects stance (aggressive vs defensive)
- Does NOT affect attack rate or DPS (stats handle that)

---

## Truth Source Components (Draft)

```csharp
VillagerBehavior : IComponentData {
    // Behavioral axes (-100 to +100)
    float VengefulScore;        // Forgiving(-) to Vengeful(+)
    float BoldScore;            // Craven(-) to Bold(+)

    // Computed modifiers (sync system calculates)
    float InitiativeModifier;   // Applied to base initiative

    // State tracking
    byte ActiveGrudgeCount;     // Unresolved grudges
    uint LastMajorAction;       // Tick of last autonomous decision
}

VillagerGrudge : IBufferElementData {
    Entity Target;                      // Who wronged them
    FixedString64Bytes OffenseType;     // "killed_friend", "stole_property"
    float IntensityScore;               // Current intensity (decays)
    uint OccurredTick;                  // When it happened
    byte RetaliationAttempts;           // Revenge attempt count
}

VillagerInitiativeState : IComponentData {
    float CurrentInitiative;            // Computed value
    uint NextActionTick;                // When next evaluation occurs
    FixedString32Bytes PendingAction;   // "seek_courtship", "plot_revenge"
}
```

---

## Open Questions & Design Decisions

### Core Mechanics
1. **Initiative frequency formula?** How often should 0.70 initiative trigger decisions?
   - Every 2-5 days? 10-20 days? Scale with village size?

2. **Permanent personality drift?** Should trauma/triumph change scores?
   - <UNCERTAINTY> User flagged - temporary vs permanent unclear

3. **Behavioral inheritance?** Random or from parents?
   - Inheritance = family behavioral patterns (grudge clans)
   - Random = high diversity

### Balancing
4. **Extreme combos?** Should Vengeful -100 + Bold +100 be rare?
   - Extremes create interesting edge cases
   - But may destabilize villages if common

5. **Grudge resolution?** How can grudges clear besides death?
   - Apologies, divine intervention, mediation, time?

### Gameplay Expression
6. **Player visibility?** Show scores in tooltips or keep hidden?
   - Transparent = scores visible
   - Immersive = behavior emerges naturally

7. **Divine influence?** Can miracles shift traits?
   - "Courage" miracle grants temporary bold?
   - "Peace" miracle reduces grudges?

---

## Next Steps

**Before Implementation:**
- [ ] Finalize initiative frequency formula
- [ ] Decide personality drift mechanics (temp vs permanent)
- [ ] Design grudge resolution paths
- [ ] Define behavioral distribution (how many extremes?)
- [ ] Create action weight tables per archetype
- [ ] Test diversity vs stability trade-offs

**Documentation:**
- [ ] Create behavioral archetype catalog
- [ ] Document alignment interaction clearly
- [ ] Write grudge lifecycle spec
- [ ] Define telemetry metrics

---

## Related Documentation

- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](Wealth_And_Social_Dynamics.md) (family/dynasty aggregates, initiative conflicts, charity vs opportunism)
- Village Initiative System: [Sandbox_Autonomous_Villages.md](../Core/Sandbox_Autonomous_Villages.md) (initiative bands)
- Alignment Framework: [Village_Villager_Alignment.md](Village_Villager_Alignment.md) (moral values)
- Generalized Alignment: [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) (cross-game patterns)

---

**For Designers:** Initiative + Behavior creates emergent individual stories. Focus on tuning action frequency and weighting.

**For Implementers:** Start with grudge tracking and initiative modifiers. Action weighting can be data-driven (ScriptableObject tables).

**For Testers:** Watch for extremes dominating villages or diversity causing instability. Balance interesting individuals vs functional communities.

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core vision captured, awaiting design decisions
