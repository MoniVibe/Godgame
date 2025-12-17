# Faction Relations System

**Status:** In Review
**Category:** Politics - Diplomacy & Conflict
**Scope:** Faction-level (not citizen-level)
**Created:** 2025-12-17
**Last Updated:** 2025-12-17

---

## Purpose

**Primary Goal:** Generate faction-level diplomacy from alignment + outlook profiles without hard-coded rules, enabling emergent conflict/cooperation patterns.

**Secondary Goals:**
- Keep social logic at faction/group level (not per-citizen) for scalability
- Support complex "honor code" systems (e.g., lawful warriors won't attack unwilling targets)
- Enable cultural friction from worldview incompatibility
- Scale to thousands of factions via small relation matrix (NxN where N = factions, not citizens)

---

## System Overview

### Key Insight

**Citizens never compute relations; factions do.**

Individual soldiers don't evaluate diplomatic stance—they inherit it from their faction's relation matrix. Units read `matrix[attackerFaction, targetFaction]` and apply engagement gates (honor rules) to determine valid actions.

**Example emergence:**
- "Corrupt evil warlike factions prey on peaceful villages" = automatic from trait combo + outlook distance
- "Lawful warlike armies refuse to attack fleeing civilians" = honor gate, not relation score
- No per-citizen AI for diplomacy → faction-level matrix updated only when profiles change

### Components

1. **Factions** - Hold FactionProfile (traits + outlook axes), dense FactionIndex
2. **Relation Matrix** - Singleton NxN buffer of RelationCell (score + action masks)
3. **Units/Groups** - Reference FactionIndex, read matrix for target evaluation
4. **Target Stance** - Per-group flags (WillFight, NonCombatant) for honor gate checks
5. **Faction Rules Blob** - Data-driven weights, thresholds, bonuses

### Connections

```
Faction Profile (Traits + Outlook)
        ↓
[Profile Change Detected]
        ↓
Rebuild Relation Matrix (NxN)
  ↓                      ↓
Score Calculation    Action Masks
  ↓                      ↓
matrix[A,B] = { score, actions (Trade/Ally/Attack/Raid) }
        ↓
Unit Decision: Read matrix[myFaction, targetFaction]
        ↓
Apply Engagement Gate: HonorGateAllowsAttack(traits, targetStance)
        ↓
Final Action: Attack | Trade | Ignore
```

### Feedback Loops

- **Positive (arms race):** Attack → hostility → counter-attack → war escalation
- **Negative (détente):** Trade → prosperity → alignment shift → peace
- **Balance:** Alignment drift from cultural exchange dampens pure hostility spirals

---

## System Dynamics

### Inputs
- **Faction creation:** New faction added → matrix rebuilt
- **Profile drift:** Cultural/religious events shift outlook axes → matrix updated
- **Trait changes:** Corruption spreads, honor code adopted → relation recalculation
- **Player intervention:** Divine mandate shifts faction alignment

### Internal Processes

1. **Profile Evaluation** (faction-level, not citizen-level)
   - Compute L1 distance between outlook axes (8 dimensions packed as 2x int4)
   - Weight distance by configured importance (rules blob)
   - Apply trait combo bonuses/penalties (predator vs prey logic)

2. **Score Calculation**
   ```
   score = BaseBias
           - (weighted L1 distance across 8 outlook axes)
           - PreyBonus (if predator traits see peaceful target)
   ```

3. **Action Mask Generation**
   - If `score <= AttackThreshold` → enable Attack
   - If `score >= TradeThreshold` → enable Trade
   - Intermediate scores → neutral/ignore

4. **Matrix Rebuild**
   - Triggered when any faction profile changes
   - O(N²) but N = factions (small), not citizens (large)
   - Store in singleton DynamicBuffer<RelationCell>

### Outputs
- **Relation Matrix:** NxN grid of (score, actions)
- **Diplomatic State:** Per-faction-pair stance (hostile/neutral/friendly)
- **Unit Behavior:** Attack decisions, trade willingness, alliance formation

---

## Faction Profile Structure

### Trait Flags (bitfield)
```
Lawful, Chaotic, Good, Evil, Warlike, Peaceful, Corrupt
```

**Combos with special meaning:**
- `Lawful + Warlike` → Honor-bound warriors (won't attack noncombatants)
- `Corrupt + Evil + Warlike` → Predatory raiders (seek weak prey)
- `Good + Peaceful` → Pacifist monks (won't fight even in self-defense)
- `Chaotic + Warlike` → Berserkers (ignore honor rules)

### Outlook Axes (8 dimensions, -100 to +100)

**Outlook0 (int4):**
1. **Aggression:** Eager for conflict ↔ Prefer peace
2. **Honor:** Strict codes ↔ Pragmatic expediency
3. **Order:** Hierarchical structure ↔ Anarchic freedom
4. **Empathy:** Value all life ↔ Ruthless utilitarianism

**Outlook1 (int4):**
5. **Greed:** Wealth-focused ↔ Ascetic
6. **Xenophobia:** Isolationist ↔ Cosmopolitan
7. **Pride:** Arrogant supremacy ↔ Humble service
8. **Mercy:** Forgive enemies ↔ Total vengeance

**Design intent:** L1 distance creates "cultural compatibility" metric. Similar outlooks → higher relation score.

---

## Relation Evaluation Algorithm

### Core Formula (Burst-safe, integer-only)

```csharp
int4 d0 = abs(factionA.Outlook0 - factionB.Outlook0);
int4 d1 = abs(factionA.Outlook1 - factionB.Outlook1);

int score = rules.BaseBias
            - dot(d0, rules.W0)  // weighted distance
            - dot(d1, rules.W1);

// Special case: predator sees prey
if (isPredator(factionA) && isPeaceful(factionB))
    score -= rules.PreyBonus;  // more hostile

score = clamp(score, -32768, 32767); // fit in short
```

### Predator Detection
```csharp
bool isPredator = (traits & (Corrupt | Evil | Warlike)) == ALL_THREE;
bool isPrey = (traits & Peaceful) != 0;
```

### Action Masks
```csharp
actions = None;
if (score <= AttackThreshold) actions |= Attack;
if (score >= TradeThreshold) actions |= Trade;
// Future: Ally, Raid, Extort thresholds
```

---

## Engagement Gates (Honor Rules)

**Problem:** Relation matrix says "attack allowed" but cultural rules forbid it.

**Solution:** Secondary check before committing to action.

### Lawful Warlike Honor Gate

```csharp
bool HonorGateAllowsAttack(TraitFlags attacker, StanceFlags target)
{
    bool isLawfulWarlike = (attacker & (Lawful | Warlike)) == BOTH;
    bool targetUnwilling = (target & WillFight) == 0;

    if (isLawfulWarlike && targetUnwilling)
        return false; // Honor code prevents attack

    return true; // Chaotic or willing target = OK
}
```

**Emergence:**
- Lawful warlike faction surrounds fleeing bandits → won't attack once bandits drop weapons
- Chaotic warlike faction → ignores surrender, slaughters all
- Good peaceful faction → won't attack even if matrix says "hostile"

### Stance Flags (per unit/group)
```
WillFight    = actively engaging in combat
NonCombatant = civilian, merchant, unarmed
```

**Set by unit AI:**
- Soldier in battle → WillFight = true
- Fleeing civilian → WillFight = false, NonCombatant = true
- Merchant caravan → NonCombatant = true (unless guards present)

---

## State Machine

### Faction Relation States (per pair)

1. **Neutral/Unknown**
   - Entry: Factions haven't met or score near zero
   - State: No trade, no combat, just ignore
   - Exit: Score crosses threshold

2. **Hostile**
   - Entry: Score <= AttackThreshold
   - State: Attack action enabled, units seek targets
   - Exit: Profile drift or player intervention

3. **Friendly**
   - Entry: Score >= TradeThreshold
   - State: Trade caravans, possible alliance
   - Exit: Cultural drift or betrayal event

4. **Complex (Trade + Raid)**
   - Entry: Corrupt factions with high trade incentive
   - State: Trade during day, raid at night (duplicitous)
   - Exit: Victim retaliates, relation crashes

### Transitions
```
Neutral → [Outlook diverges] → Hostile
Neutral → [Cultural exchange] → Friendly
Hostile → [Peace treaty / player miracle] → Neutral
Friendly → [Corruption spreads] → Complex
```

---

## Key Metrics

| Metric | Target Range | Critical Threshold |
|--------|--------------|-------------------|
| Matrix Size (NxN) | N < 1000 | N > 10,000 (too slow) |
| Rebuild Frequency | 0.1–1 Hz | > 10 Hz (thrashing) |
| Score Spread | -500 to +500 | All scores near zero (no differentiation) |
| Predator Prey Bonus | -50 to -200 | > -500 (overpowers outlook) |
| Honor Gate Override Rate | 5–20% of attacks | > 50% (honor too restrictive) |

---

## Balancing

### Self-Balancing
- Similar factions naturally cluster (low L1 distance)
- Extreme outlooks create natural enemies (high distance)
- Predator-prey dynamic self-limits (peaceful factions hide/flee)

### Player Intervention
- Divine mandates shift faction outlooks
- Miracles spawn "good" factions to counter "evil" ones
- Corruption curses spread evil/chaos traits

### System Corrections
- Weight tuning in FactionRules blob (data-driven)
- Threshold adjustments to prevent all-war or all-peace
- Predator bonus scaling to balance aggression

---

## Scale & Scope

### Small Scale (Faction-level)
- N factions (typically 10–1000)
- O(N²) matrix rebuild is acceptable (1M cells for 1K factions)
- Matrix stored on singleton entity (single DynamicBuffer)

**Performance:** Rebuild 1000×1000 matrix in <1ms (Burst-compiled)

### Medium Scale (Group-level)
- Units/armies reference FactionIndex (dense 0..N-1)
- Single lookup `matrix[myFaction * N + targetFaction]` per decision
- Honor gate is cheap (bitfield checks)

**Emergence:** Army behavior (attack/trade/ignore) falls out from faction relation + unit stance

### Large Scale (World-level)
- Thousands of units, but only hundreds of factions
- Citizens never compute relations (inherit from faction)
- No per-citizen DynamicBuffer bloat

**Scalability:** System designed for 1M units across 1K factions
- Matrix size: 1M cells × 4 bytes = 4MB (trivial)
- Per-unit cost: 1 matrix lookup + 1 gate check = ~50ns

---

## Time Dynamics

### Short Term (Seconds to Minutes)
- Matrix lookup per combat encounter
- Honor gate check per attack decision
- Immediate diplomatic response (hostile/friendly)

### Medium Term (Minutes to Hours)
- Faction profiles drift from cultural events
- Matrix rebuilds (low frequency, fast execution)
- Alliances form/break based on score changes

### Long Term (Hours to Sessions)
- Cultural divergence creates permanent enemies
- Trade relationships stabilize scores
- Honor code evolution (lawful→chaotic from corruption)

---

## Failure Modes

### Death Spiral
- **Cause:** All factions become hostile → endless war → population collapse
- **Recovery:** Player spawns peaceful faction, divine peace mandate

### Stagnation
- **Cause:** All factions too similar → no conflict, boring
- **Recovery:** Inject chaotic/evil faction, corruption event

### Runaway
- **Cause:** Predator faction exterminates all peaceful targets
- **Recovery:** Peaceful factions flee/hide, player intervention, predator infighting

---

## Player Interaction

### Observable
- Faction relation tooltips (score + action masks)
- Diplomacy screen showing NxN matrix heatmap
- Unit behavior explanations ("Won't attack due to honor code")

### Control Points
1. **Direct:** Divine mandate shifts faction outlook axes
2. **Indirect:** Bless/curse events change traits (corruption spread)
3. **Strategic:** Spawn new factions with designed profiles
4. **Emergency:** Force peace treaty (override matrix temporarily)

### Learning Curve
- **Beginner:** "Red factions fight, green factions trade"
- **Intermediate:** "Lawful warriors won't attack fleeing enemies"
- **Expert:** "I can engineer faction profiles to create specific diplomatic landscapes"

---

## Systemic Interactions

### Dependencies
- **Faction Registry:** Dense FactionIndex assignment
- **Trait System:** TraitFlags propagation (corruption, honor codes)
- **Culture System:** Outlook drift from cultural exchange

### Influences
- **Combat System:** Relation matrix gates attack actions
- **Trade System:** Trade mask enables economic routes
- **Alliance System:** Friendly scores trigger alliance formation
- **Rebellion System:** Hostile internal factions trigger revolts

### Synergies
- **Diet System:** Cultural diets drift outlooks (fish-eaters become more empathetic?)
- **Religion System:** Divine alignment shifts faction profiles
- **Espionage System:** Spies read enemy outlook axes, sabotage to shift them

---

## Implementation Architecture

### PureDOTS Components (Library-side)

**Faction Profile:**
```csharp
FactionProfile : IComponentData
{
    TraitFlags Traits;      // bitfield (Lawful, Evil, etc.)
    int4 Outlook0;          // Aggression, Honor, Order, Empathy
    int4 Outlook1;          // Greed, Xenophobia, Pride, Mercy
}

FactionIndex : IComponentData { int Value; } // dense 0..N-1
```

**Relation Matrix (singleton):**
```csharp
RelationCell : IBufferElementData
{
    short Score;            // -32768 to +32767
    ActionMask Actions;     // bitfield (Trade, Ally, Attack, Raid)
}
```

**Unit/Group Runtime:**
```csharp
TargetStance : IComponentData
{
    StanceFlags Value;      // WillFight, NonCombatant
}
```

### BlobAsset Definitions (Baked game-side)

**FactionRules (tuning knobs):**
```csharp
{
    int4 W0, W1;            // weights for 8 outlook axes
    short PreyBonus;        // hostility boost for predator vs peaceful
    short BaseBias;         // baseline score offset
    short AttackThreshold;  // score <= this → Attack enabled
    short TradeThreshold;   // score >= this → Trade enabled
}
```

**Lookup:**
```csharp
FactionBlob { FactionRules Rules; }
FactionBlobRef : IComponentData (singleton)
```

### System Execution

**RebuildFactionMatrixSystem:**
1. Query all factions (FactionProfile + FactionIndex)
2. For each pair (i, j):
   - Compute L1 distance across 8 axes
   - Apply weights, bonuses
   - Generate score + action masks
   - Store in `matrix[i * N + j]`
3. Runs when faction profiles change (not every frame)

**EngagementDecisionSystem (example):**
```csharp
foreach (unit with FactionIndex myFaction)
{
    foreach (potential target with FactionIndex targetFaction, TargetStance stance)
    {
        var cell = matrix[myFaction * N + targetFaction];

        if (cell.Actions & Attack)
        {
            if (HonorGateAllowsAttack(myTraits, stance.Value))
                CommitAttack(target);
        }
    }
}
```

All computation is Burst-compiled, integer-only (no float inaccuracy).

### Performance Optimizations

**For 1K factions × 1M units:**
1. **Matrix is NxN (factions), not MxM (units)** - 1K² = 1M cells, not 1M² = 1T cells
2. **Dense FactionIndex** - array lookup, not hashmap
3. **Integer-only evaluation** - no float precision issues, faster
4. **Change detection** - only rebuild matrix when profiles dirty
5. **Packed int4** - SIMD L1 distance calculation

---

## Iteration Plan

### v1.0 (MVP)
**Features:**
- Basic profile (3 traits, 4 outlook axes)
- Simple L1 distance scoring
- Single action mask (Attack only)
- No honor gates (all attacks allowed)

**Limitations:**
- No trade/ally actions
- Manual faction spawning
- Fixed weights (no BlobAsset tuning)

### v2.0 (Full Diplomacy)
**Additions:**
- 8 outlook axes, 7 trait flags
- Attack, Trade, Ally, Raid action masks
- Honor gate system (lawful warlike)
- Configurable FactionRules BlobAsset
- Diplomacy UI (relation matrix heatmap)

### v3.0 (Dynamic Factions)
**Additions:**
- Faction profile drift from cultural events
- Espionage to reveal/manipulate outlooks
- Sub-faction splits (rebels inherit modified profile)
- Reputation system (individual entities deviate from faction norm)

---

## Open Questions

1. **Sub-factions:** Should rebels inherit parent outlook with mutation, or start blank?
2. **Reputation:** Can individual heroes/villains deviate from faction stance?
3. **Temporary treaties:** Should honor gates have expiry (cease-fire ends)?
4. **Multi-faction targets:** How to evaluate attacking group with mixed factions?
5. **Player faction:** Does player god have a FactionProfile, or override system?

---

## Exploits

1. **Profile manipulation:** Player rapidly shifts outlook to confuse AI
   - **Severity:** Low (player power by design)
   - **Fix:** Rate-limit outlook changes, or cost divine power

2. **Honor gate abuse:** Enemies feign noncombatant to avoid attack
   - **Severity:** Medium
   - **Fix:** Stance change has cooldown; attacking resets stance instantly

3. **Predator stacking:** All factions become corrupt+evil+warlike
   - **Severity:** High (no peaceful targets = broken)
   - **Fix:** Balance events spawn good factions; corruption has spread cost

---

## Tests

- [ ] Matrix rebuild: 100 factions → 10K cells populated correctly
- [ ] L1 distance: verify scoring matches manual calculation
- [ ] Predator detection: corrupt+evil+warlike sees peaceful → bonus applied
- [ ] Honor gate: lawful warlike refuses unwilling target
- [ ] Action masks: score thresholds enable correct actions
- [ ] Change detection: matrix doesn't rebuild when profiles unchanged
- [ ] Stress test: 1000 factions × 10K units decision loop
- [ ] Burst compilation: RelationEval.Evaluate compiles cleanly

---

## Performance

- **Complexity:** O(N²) matrix rebuild (N = factions), O(1) lookup (per unit decision)
- **Max factions:** 1000 (1M matrix cells = 4MB)
- **Update freq:** Matrix 0.1–1 Hz (on profile change), Lookup 30–60 Hz (per decision)
- **Memory:** 4 bytes per cell (2 short) × N² factions

---

## Visual Representation

### System Diagram
```
[Faction Profiles]
   ↓ (on change)
[Rebuild Matrix System]
   ↓
[NxN Relation Matrix] (singleton buffer)
   ↓
[Unit Decision Loop]
   ↓
Read matrix[myFaction, targetFaction]
   ↓
Actions enabled? (Attack/Trade/Ally)
   ↓
Apply Honor Gate (if Attack)
   ↓
Commit Action
```

### Data Flow
```
FactionProfile (Traits + Outlook) ──┐
                                     ↓
                            [Evaluate Relation]
                             ↓            ↓
                        L1 Distance   Trait Bonuses
                             ↓            ↓
                            Score ← BaseBias
                             ↓
                        Threshold Checks
                             ↓
                        ActionMask
                             ↓
                  Store in matrix[i,j]
                             ↓
                  Unit reads matrix
                             ↓
                  Honor gate check
                             ↓
                    Final decision
```

---

## Example Scenario: Lawful Knights vs Corrupt Raiders

### Setup

**Faction A: Order of the Silver Dawn (Lawful + Good + Warlike)**
- Traits: `Lawful | Good | Warlike`
- Outlook: `(Aggression: 50, Honor: 90, Order: 80, Empathy: 60)`
- **Profile:** Disciplined warriors with strict honor code

**Faction B: Shadowfang Marauders (Corrupt + Evil + Warlike)**
- Traits: `Corrupt | Evil | Warlike`
- Outlook: `(Aggression: 90, Honor: -50, Order: -40, Empathy: -80)`
- **Profile:** Predatory raiders who prey on weak

**Faction C: Riverside Farmers (Peaceful + Good)**
- Traits: `Peaceful | Good`
- Outlook: `(Aggression: -60, Honor: 40, Order: 20, Empathy: 70)`
- **Profile:** Defenseless civilians

### Relation Calculation

**A vs B (Knights vs Raiders):**
```
L1 distance = |50-90| + |90-(-50)| + |80-(-40)| + |60-(-80)|
            = 40 + 140 + 120 + 140 = 440
score = 0 - (440 * 0.5) = -220
Actions: Attack (score < -100 threshold)
```
**Result:** Hostile (knights hunt raiders)

**B vs C (Raiders vs Farmers):**
```
L1 distance = |90-(-60)| + ... = 420
score = 0 - 210 - 150 (predator vs prey bonus) = -360
Actions: Attack + Raid
```
**Result:** Extreme hostility (raiders target civilians)

**A vs C (Knights vs Farmers):**
```
L1 distance = 150
score = 0 - 75 = -75
Actions: None (above attack threshold)
```
**Result:** Neutral (knights protect farmers)

### Engagement Example

**Scenario 1: Knights encounter fleeing raiders**
1. Matrix: `matrix[Knights, Raiders].Actions = Attack` → enabled
2. Raiders have `TargetStance = { WillFight: false }` (fleeing)
3. Honor gate: `HonorGateAllowsAttack(Lawful|Warlike, !WillFight)` → **FALSE**
4. **Knights do not attack fleeing raiders** (honor code)

**Scenario 2: Raiders encounter unarmed farmers**
1. Matrix: `matrix[Raiders, Farmers].Actions = Attack + Raid` → enabled
2. Farmers have `TargetStance = { NonCombatant: true, WillFight: false }`
3. Honor gate: `HonorGateAllowsAttack(Corrupt|Evil|Warlike, ...)` → **TRUE** (no honor)
4. **Raiders slaughter farmers** (predatory nature)

**Scenario 3: Knights defend farmers from raiders**
1. Knights engage raiders
2. Raiders set `WillFight: true` (active combat)
3. Honor gate now allows knights to attack (willing combatants)
4. **Knights destroy raiders** (honorable combat)

### Emergent Outcomes
- Knights patrol roads → raiders avoid them (relation matrix says hostile, but knights won't chase fleeing enemies)
- Raiders wait for knights to leave → attack farmers
- Farmers petition knights for permanent garrison (systemic need emerges)
- Player must choose: spread knights thin (protect all farmers) or concentrate (raiders escape)

**No hard-coded quest logic**—all emergent from relation matrix + honor gates.

---

## Related Documentation

- **Truth Sources:** `Docs/TruthSources_Inventory.md#Factions` (when implemented)
- **Related Systems:**
  - `Docs/Concepts/Politics/Rebellion_Mechanics_System.md` (internal faction conflict)
  - `Docs/Concepts/Politics/Sub_Band_System.md` (faction hierarchy)
  - `Docs/Concepts/Combat/Engagement_Rules.md` (combat decision-making)
- **Implementation:**
  - Package: `Packages/com.moni.puredots/Runtime/Factions/`
  - Game: `Assets/Scripts/Godgame/Factions/` (authoring, baking)

---

**For Implementers:**
- Use dense FactionIndex (0..N-1) for direct array indexing
- Matrix rebuild should be change-detected (don't rebuild every frame)
- Honor gate is separate from relation score (layered decision)

**For Designers:**
- Outlook weights are your primary tuning lever
- Predator-prey bonus creates asymmetric hostility
- Honor gates add cultural flavor without complicating matrix

---

**Last Updated:** 2025-12-17
**Implementation Status:** Spec Complete - Ready for Review
**Performance Target:** 1000 factions × 1M units @ 30Hz
