# Aggregate Entities & Individual Dynamics

**Status:** Concept - Core Design Philosophy
**Category:** Core - Social Systems
**Complexity:** Complex
**Created:** 2025-12-02
**Last Updated:** 2025-12-02

---

## Overview

**Purpose:** Define the bidirectional relationship between individuals and aggregate entities (groups)
**Player Impact:** Emergent social behaviors, cascade effects from composition changes, realistic cultural evolution
**System Role:** Foundation for all group dynamics (villages, guilds, armies, bands, families, dynasties, companies)

### Terminology Clarification

**Aggregate Entities (Groups):** Collections of individual entities acting together in cohesion based on outlooks, alignments, behaviors, stats, morale. Examples:
- Villages
- Guilds
- Armies
- Bands
- Families
- Dynasties
- Companies
- Factions

**NOT Aggregate Resources:** Physical piles/quantities of resources (wood, ore, stone) - these are just floats/numbers with no behavior. See [Physical_Resource_Chunks.md](../Economy/Physical_Resource_Chunks.md) for resource pile mechanics.

### Design Philosophy

**Bidirectional Influence:** Individuals shape group averages, group averages create ambient conditions that influence individuals. Neither side "drives everything" - it's a feedback loop.

**Composition Matters:** The specific individuals in a group determine its character. Losing corrupt warlike members shifts the group toward other outlooks over time (cascade effect).

**Emergent Culture:** Group behavior emerges from member composition, not top-down design. A guild doesn't have "personality" - it has the averaged characteristics of its members.

---

## How It Works

### Bidirectional Influence Model

This is **not** a one-way relationship—individuals shape aggregate entities, and aggregate entities create ambient conditions that influence individuals.

#### Individual Actions → Aggregate Averages

**Mechanism:**
- Individual entity performs actions, gains stats/experience (e.g., villager carries heavy ore → strength increases)
- Group average recalculates based on all member stats (village average strength updates)
- Group outlook coverage shifts based on member composition (more industrious individuals → higher industrious coverage)
- Group reputation spreads to other entities/settlements (strong village known for physical labor)

**Examples:**
- Villager repeatedly carrying heavy chunks gains strength → village average strength increases
- Guild member acts vengefully → guild's vengeful outlook coverage grows
- Army soldier shows initiative → army's initiative average rises
- Family member pursues wealth → family's ambition metric increases

#### Aggregate Averages → Ambient Conditions

**Mechanism:**
- Group's averaged characteristics create ambient pressure/expectations
- Ambient conditions influence individual decision-making (not rigid control)
- New members or outsiders feel pressure to conform to group norms
- Outlier individuals may resist, compensate, or leave

**Examples:**
- Village with high average strength creates expectation of physical capability → weak villagers feel inadequate or seek alternative specializations
- Vengeful guild creates ambient anger → members more likely to pursue revenge quests
- Cautious army creates ambient fear → soldiers avoid risky maneuvers
- Ambitious family creates competitive pressure → even unambitious members feel need to prove themselves

### Cascade Effects from Composition Changes

**Trigger Events:**
- Mass death (battle, disease, disaster)
- Expulsion/execution of members
- Immigration wave
- Voluntary departures
- Recruitment of new members

**Cascade Mechanism:**
1. **Composition Change:** Specific individuals removed/added to group
2. **Aggregate Shift:** Group averages recalculate based on new composition
3. **Ambient Change:** New ambient conditions emerge from shifted averages
4. **Behavioral Response:** Remaining/new members adjust behavior based on new ambient
5. **Long-Term Evolution:** Group culture solidifies or shifts over generations

#### Example: Loss of Corrupt Army Members

1. **Event:** Army's 50 corrupt/warlike soldiers die in battle
2. **Aggregate Shift:**
   - Army corruption average drops from 65 → 48
   - Army warlike outlook coverage decreases from 70% → 45%
   - Other outlooks (honorable, peaceful, pragmatic) gain relative coverage
3. **Ambient Change:**
   - Ambient corruption pressure lowers (fewer corrupt voices influencing decisions)
   - Ambient warlike aggression decreases (less peer pressure for violence)
   - Honor/restraint norms become more prevalent
4. **Behavioral Response:**
   - Borderline-corrupt soldiers feel less supported, may reform or leave
   - Honorable soldiers feel emboldened to speak up against atrocities
   - Army tactics shift toward more disciplined/lawful approaches
   - New recruits drawn to reformed reputation
5. **Long-Term:** Over months/years, army culture transforms from brutal raiders to disciplined military force

#### Example: Executing Traitors on Ship

**Context:** Spaceship crew with mixed alignments discovers 3 traitors

**Immediate Effect:**
- Traitor average influence on ship decreases (3 fewer treacherous individuals)
- Ship loyalty average increases (composition shift toward loyal crew)

**Emotional Responses (Alignment-Dependent):**
- **Forgiving Individuals:** Distressed by harsh execution, may protest or become demoralized
- **Vengeful Individuals:** Satisfied by justice, morale boost from seeing enemies punished
- **Lawful Individuals:** Approve if trial was fair, disapprove if summary execution
- **Chaotic Individuals:** Indifferent to process, care only about outcome/spectacle
- **Pragmatic Individuals:** Accept necessity but uncomfortable with violence

**Cascade:**
1. Remaining crew members adjust behavior based on their alignment's response to execution
2. Ambient loyalty increases but ambient forgiveness may decrease
3. Future potential traitors more fearful (deterrent effect) or more cautious (hiding better)
4. Crew cohesion may improve (threat removed) or fracture (moral disagreements)

---

## Behavioral Dimensions

These dimensions apply to both individuals and aggregate entities (as averaged from members):

### Vengeful ↔ Forgiving

**Individual Level:**
- **Vengeful:** Seeks retribution for slights, remembers grudges, punishes mistakes harshly
- **Forgiving:** Lets go of grievances, gives second chances, focuses on rehabilitation

**Aggregate Level:**
- **Vengeful Group:** Ambient anger, punishment-focused culture, eye-for-eye justice
- **Forgiving Group:** Ambient understanding, rehabilitation focus, restorative justice

**Cascade Example:**
- Vengeful village executes thief → forgiving minority becomes demoralized → some leave → village becomes more vengeful (reinforcing spiral)
- Forgiving guild pardons betrayer → vengeful minority feels unsafe → demand stricter rules → internal factional tension

### Bold ↔ Craven

**Individual Level:**
- **Bold:** Takes risks, pushes limits, faces danger directly, high injury risk but fast growth
- **Craven:** Avoids risks, acts conservatively, prioritizes safety, slow but steady progression

**Aggregate Level:**
- **Bold Group:** Ambient courage, risk-taking normalized, aggressive expansion/exploration
- **Craven Group:** Ambient caution, safety protocols, defensive posture

**Cascade Example:**
- Bold mining team suffers cave collapse → survivors become craven → village shifts to surface quarrying (lower yield but safer)
- Craven army avoids battle → bold soldiers frustrated → transfer to more aggressive armies → army becomes even more craven

### Initiative

**Individual Level:**
- **High Initiative:** Self-directed, proactive, volunteers for tasks, seeks challenges
- **Low Initiative:** Reactive, waits for orders, prefers assigned tasks, avoids leadership

**Aggregate Level:**
- **High Initiative Group:** Self-organizing, rapid response, entrepreneurial culture
- **Low Initiative Group:** Hierarchical, requires direction, slow to adapt

**Cascade Example:**
- High-initiative village members see opportunity → start trading expeditions without orders → village becomes mercantile hub
- Low-initiative guild members wait for leadership → leader dies → guild paralyzed until new leader emerges

### Desires & Ambition

**Individual Level:**
- **Desire for Status:** Seeks recognition, competes for prestige, pursues visible achievements
- **Desire for Wealth:** Accumulates resources, optimizes profit, prioritizes economic gain
- **Desire for Power:** Seeks control over others, pursues authority positions, political maneuvering
- **Desire for Knowledge:** Pursues learning, experiments, explores unknowns
- **Ambition:** Intensity of desire pursuit (high ambition = aggressive goal-seeking)

**Aggregate Level:**
- Groups with shared desires develop specialized cultures (wealth-focused merchant guilds, status-focused warrior bands)
- High-ambition groups drive rapid change and expansion
- Low-ambition groups maintain stability but resist innovation

**Cascade Example:**
- Ambitious blacksmith trains apprentices → apprentices become ambitious → village develops competitive crafting culture → attracts ambitious immigrants → becomes renowned crafting center

---

## Rules

1. **No Determinism:** Aggregates create ambient pressure, not rigid control
   - Condition: Individual with high initiative in low-initiative group
   - Effect: Individual can act independently, but faces social friction/isolation

2. **Outliers Possible:** Individuals can defy group norms
   - Condition: Bold individual in craven village
   - Effect: Individual takes risks others avoid, may become hero or outcast

3. **Composition Drives Averages:** Group characteristics emerge from member stats
   - Condition: Group gains/loses members
   - Effect: Averages recalculate immediately, ambient conditions shift over time

4. **Cascade Delays:** Behavioral shifts take time to propagate
   - Condition: Major composition change (50% member loss)
   - Effect: Immediate average shift, but cultural/behavioral response unfolds over weeks/months

### Edge Cases

- **Single-Member Groups:** No aggregate dynamics (individual = aggregate)
- **Evenly Split Groups:** High internal tension, frequent factional conflicts
- **Rapid Turnover:** Unstable culture, no long-term identity formation
- **Homogeneous Groups:** Strong cohesion but vulnerable to groupthink, lack adaptability

---

## Examples

### Scenario 1: Village Mining Disaster

**Given:** Mining village, 60 villagers, 15 veteran miners (strength 80+, bold, high initiative)
**When:** Cave collapse kills all 15 veteran miners
**Then:**

1. **Immediate (Day 0):**
   - Village average strength drops from 55 → 48
   - Bold coverage drops from 35% → 22%
   - Initiative average drops from 62 → 54
   - Village loses 25% of ore production capacity

2. **Short-Term (Weeks 1-4):**
   - Remaining villagers avoid deep mining (craven dimension activated)
   - Surface quarrying increases (safer, lower yield)
   - Village imports ore rather than extracting locally
   - Mourning period depresses morale across all villagers

3. **Medium-Term (Months 2-6):**
   - Bold/initiative-driven villagers step up (ambition to fill gap)
   - New apprentice miners begin training (slow strength growth)
   - Village reputation shifts: "once-great mining hub now cautious"
   - Immigration of risk-averse individuals attracted by safety culture

4. **Long-Term (Years 1-3):**
   - New generation of miners develops (strength slowly recovers)
   - Village culture permanently more cautious (ambient craven higher)
   - Diversification into safer industries (farming, crafting)
   - Oral tradition preserves disaster memory ("The Deep Collapse")

### Scenario 2: Guild Expelling Lazy Members

**Given:** Laborers' Guild, 40 members, 6 chronically lazy (low initiative, avoid heavy work)
**When:** Guild council votes to expel lazy members
**Then:**

1. **Immediate:**
   - Guild initiative average increases from 58 → 64
   - Productivity metrics improve (less task abandonment)
   - Expelled members join rival guild or form own band

2. **Emotional Responses:**
   - **Forgiving Members (10):** Protest expulsion, 2 leave guild in solidarity
   - **Vengeful Members (8):** Celebrate removal, demand harsher standards
   - **Pragmatic Members (22):** Accept decision, focus on work

3. **Cascade:**
   - Ambient "industrious" expectation strengthens
   - Borderline-lazy members (initiative 45-55) feel pressure:
     - 3 work harder to avoid expulsion (initiative increases)
     - 2 leave voluntarily (join less demanding guilds)
   - New recruits screened more carefully (high initiative requirement)

4. **Long-Term:**
   - Guild reputation: "demanding but elite" (attracts ambitious workers)
   - Expelled members form "Freelancers' Band" (low-pressure, flexible work)
   - Factional rivalry emerges between guilds (cultural identity conflict)

### Scenario 3: Vengeful vs Forgiving Ship Crew

**Given:** Starship, 120 crew, 30% vengeful, 50% forgiving, 20% pragmatic
**When:** Crew discovers saboteur, captain executes without trial
**Then:**

1. **Vengeful Crew (36):**
   - Morale boost (+10) from seeing justice served
   - Ambient aggression increases (more willing to use force)
   - Support captain's authority (loyalty +5)

2. **Forgiving Crew (60):**
   - Morale penalty (-15) from harsh execution
   - Ambient fear increases (worry they could be next)
   - 8 crew members request transfer at next port
   - 4 crew members openly criticize captain (risk of mutiny seed)

3. **Pragmatic Crew (24):**
   - Mild morale penalty (-5) from lack of due process
   - Accept necessity but uncomfortable with violence
   - Neutral stance (don't support or oppose captain)

4. **Cascade:**
   - 8 forgiving crew leave → ship forgiving coverage drops to 43%
   - Ambient forgiveness decreases, ambient aggression increases
   - Remaining forgiving crew feel less supported (may conform or resist)
   - Future moral dilemmas become more contentious (factional split widens)

---

## Integration with Other Systems

### Physical Resource Chunks
- Individual carrying heavy chunks → strength gains → village average strength shifts
- Strong village attracts mining jobs/immigrants (reputation effect)
- Weak villagers in strong village seek alternative specializations

### Guild System
- Guild selection criteria filter for specific traits → homogeneous composition
- Guild wars shift member alignments (veterans become more warlike/cautious)
- Embassy system spreads guild culture to other villages

### Focus System
- High-focus individuals influence group strategic decisions
- Groups with high focus-users develop intellectual culture
- Mana debt/coma events shift group caution (avoid overextension)

### Miracle System
- God's moral alignment influences village composition over time
- Villages matching god's alignment receive more blessings → attract like-minded immigrants
- Mismatched villagers may leave or rebel

---

## Design Implications

### Avoid Determinism
- Aggregates create ambient pressure, not rigid control
- Individuals with high initiative/ambition can defy ambient norms
- Outlier behavior (bold in craven group, vengeful in forgiving group) should be possible
- Player actions can empower outliers (promote bold individual to leadership)

### Track Composition Changes
- System should log significant demographic shifts (deaths, expulsions, immigrations)
- UI should surface aggregate trends ("Village becoming more cautious after mine disaster")
- Historical events should be preserved ("The Deep Collapse of Year 3")

### Feedback Clarity
- Players should see connection: "Veteran miners died → village weaker → extraction slowed"
- Tooltips: "This villager is bold (rare in this cautious village)"
- Aggregate stats visible: "Village Average Strength: 48 (↓7 from last month)"
- Composition breakdowns: "Village Outlooks: 35% Industrious, 28% Cautious, 22% Pragmatic, 15% Other"

### Emergent Gameplay
- Players should be able to shape group culture through composition management (recruitment, expulsion, promotion)
- Natural selection: Groups with maladaptive compositions fail (craven army loses wars)
- Cultural evolution: Successful groups spread their culture (victorious armies recruit admirers)

---

## Technical Considerations

### Update Frequency
- **Aggregate Averages:** Recalculate on member add/remove (event-driven)
- **Ambient Conditions:** Update daily or weekly (batch process)
- **Behavioral Shifts:** Gradual interpolation over weeks/months (avoid sudden flips)

### Data Needs

```csharp
// Individual entity (villager, crew member, guild member, etc.)
public struct IndividualBehavioralTraits : IComponentData
{
    public byte Strength;              // 1-100
    public byte Initiative;            // 1-100 (self-direction)
    public sbyte VengefulForgiving;    // -100 (forgiving) to +100 (vengeful)
    public sbyte BoldCraven;           // -100 (craven) to +100 (bold)
    public byte Ambition;              // 1-100 (intensity of goal pursuit)

    // Desires (can have multiple, intensity varies)
    public float DesireForStatus;      // 0-1
    public float DesireForWealth;      // 0-1
    public float DesireForPower;       // 0-1
    public float DesireForKnowledge;   // 0-1
}

// Aggregate entity (village, guild, army, band, family, etc.)
public struct AggregateEntityStats : IComponentData
{
    public Entity GroupEntity;         // Reference to village/guild/etc.

    // Averaged from all members
    public float AverageStrength;      // 1-100
    public float AverageInitiative;    // 1-100
    public float AverageVengeful;      // -100 to +100
    public float AverageBold;          // -100 to +100
    public float AverageAmbition;      // 1-100

    // Coverage percentages (sum to 1.0)
    public float StatusDesireCoverage;
    public float WealthDesireCoverage;
    public float PowerDesireCoverage;
    public float KnowledgeDesireCoverage;
}

// Ambient conditions (affects all members of group)
public struct AmbientGroupConditions : IComponentData
{
    public Entity GroupEntity;

    // Ambient pressure intensities (0-1)
    public float AmbientCourage;       // From bold average
    public float AmbientFear;          // From craven average
    public float AmbientAnger;         // From vengeful average
    public float AmbientCompassion;    // From forgiving average
    public float AmbientDrive;         // From initiative/ambition

    // Cultural norms (emergent from composition)
    public float ExpectationStrength;  // How strong members "should" be
    public float ExpectationLoyalty;   // How loyal members "should" be
    public float ToleranceForOutliers; // How accepting of non-conformity
}

// Composition change tracking
public struct GroupCompositionChange : IBufferElementData
{
    public long Timestamp;             // When change occurred
    public ChangeType Type;            // Death, Expulsion, Immigration, etc.
    public int Count;                  // How many individuals affected
    public float StrengthDelta;        // Change in average strength
    public float InitiativeDelta;      // Change in average initiative
    // ... other relevant deltas
}
```

### Performance Considerations
- Aggregate recalculation on every member change could be expensive for large groups
- Cache averages, mark dirty on composition change, recalculate in batch
- Ambient conditions don't need frame-by-frame updates (daily/weekly is sufficient)
- Cascade effects should be event-driven (triggered by significant composition changes, not continuous polling)

---

## Open Questions

1. **Outlier Threshold:** How far from group average can individual diverge before facing consequences?
   - **Option A:** Fixed tolerance (±20 from average)
   - **Option B:** Group-dependent (high ToleranceForOutliers groups more accepting)
   - **Recommendation:** Option B for emergent diversity

2. **Cascade Speed:** How quickly should behavioral shifts propagate?
   - **Option A:** Immediate (averages change → instant behavior change)
   - **Option B:** Gradual (weeks/months interpolation)
   - **Recommendation:** Option B for realistic cultural inertia

3. **Faction Formation:** Should internal disagreements automatically spawn sub-groups?
   - **Option A:** Manual split (player/leader decides to expel faction)
   - **Option B:** Automatic (high tension → faction leaves voluntarily)
   - **Recommendation:** Mix of both (automatic for extreme cases, manual for moderate)

4. **Cross-Group Influence:** Can individuals in multiple groups (guild + village) create cultural bridges?
   - **Option A:** Isolated (each group independent)
   - **Option B:** Cross-pollination (guild members spread guild culture to home villages)
   - **Recommendation:** Option B for richer emergent dynamics

---

## Version History

- **v0.1 - 2025-12-02:** Initial concept capture, moved from Physical_Resource_Chunks.md

---

## Related Mechanics

- [Guild_System.md](../Villagers/Guild_System.md) - Guild-specific aggregate dynamics
- [Village_Villager_Alignment.md](../Villagers/Village_Villager_Alignment.md) - Village social systems
- [Bands_Vision.md](../Villagers/Bands_Vision.md) - Band formation and dynamics
- [Physical_Resource_Chunks.md](../Economy/Physical_Resource_Chunks.md) - Example of individual actions (hauling) affecting aggregates (village strength)
- [Wealth_And_Social_Dynamics.md](../Villagers/Wealth_And_Social_Dynamics.md) - Economic stratification in groups

---

**Last Updated:** 2025-12-02
**Status:** Concept Captured - Core Design Philosophy
