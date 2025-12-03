# Outlooks & Alignments System Summary for Advisor

**Purpose:** Overview of the alignment and outlook system design for Godgame, to inform Space4X prefab authoring tool planning.

**Date:** 2025-01-XX  
**Status:** Design phase (not yet implemented)

---

## Executive Summary

Godgame uses a **three-layer alignment/outlook system** that applies to both individual entities (villagers) and aggregate entities (villages, guilds, armies). The system consists of:

1. **Alignment:** Moral/ideological position on three axes (Moral, Order, Purity)
2. **Outlook:** Cultural/behavioral expression (Materialistic, Warlike, Spiritual, etc.)
3. **Disposition:** Individual/entity stance toward external forces (player god, other entities)

This system is **entity-agnostic, scale-agnostic, and genre-agnostic**, making it applicable to both Godgame (fantasy) and Space4X (sci-fi) with appropriate adaptations.

---

## Alignment System

### Three Alignment Axes

Each entity carries **three alignment readings simultaneously**, enabling nuanced combinations:

1. **Moral Axis:** Good ↔ Neutral ↔ Evil
   - Range: -100 (pure evil) to +100 (pure good)
   - Example: +60 = Good-leaning, -70 = Evil-leaning

2. **Order Axis:** Lawful ↔ Neutral ↔ Chaotic
   - Range: -100 (chaotic) to +100 (lawful)
   - Example: +80 = Lawful, -50 = Chaotic-leaning

3. **Purity Axis:** Pure ↔ Neutral ↔ Corrupt
   - Range: -100 (corrupt) to +100 (pure)
   - Example: +40 = Pure-leaning, -60 = Corrupt

**Combination Examples:**
- "Lawful Good Corrupt" = Follows rules, helps others, but tainted by corruption
- "Chaotic Evil Pure" = Unpredictable, harmful, but uncorrupted (raw chaos)
- "Neutral Neutral Neutral" = True neutral, no strong leanings

**Purity Modulation:** Purity states modulate how strongly moral/order choices manifest in behavior and event resolution rolls.

### Who Has Alignment?

**Current Design (Proposed):**
- **God Alignment:** Global singleton value (player's moral standing)
- **Village Alignment:** Collective alignment derived from god influence + villager dispositions
- **Villager Disposition:** Individual alignment preferences (how they react to god's alignment)

**Aggregate Entities:** Villages, guilds, companies, bands, armies compute collective alignment from member contributions and leadership influence.

---

## Outlook System

### Core Outlook Families

Outlooks represent **cultural/behavioral expressions** of alignment. An entity may carry up to **three regular outlooks** simultaneously, or trade breadth for **two fanatic outlooks** (extreme positions with stronger behavioral biases).

**Core Outlook Types:**
1. **Materialistic** – wealth, craft, inheritance duty
2. **Spiritual** – faith, ritual, devotion cycles
3. **Warlike** – offense, conquest, martial honor
4. **Peaceful** – caretaking, diplomacy, healing
5. **Expansionist** – settlement growth, frontier pushes
6. **Isolationist** – fortification, inward prosperity
7. **Scholarly** – knowledge, research, arcana
8. **Mercantile** – trade, markets, logistics
9. **Agrarian** – food security, land stewardship
10. **Artisan** – aesthetics, culture, festivals

### Outlook Axes

Outlooks function as **independent ideological axes** with spectra:

- **Xeno Axis:** Xenophilic ↔ Neutral ↔ Xenophobic
- **Warfare Axis:** Warlike ↔ Neutral ↔ Peaceful
- **Expansion Axis:** Expansionist ↔ Neutral ↔ Isolationist
- **Economy Axis:** Materialistic ↔ Neutral ↔ Spiritual
- **Culture Axis:** Scholarly ↔ Neutral ↔ Artisan

**Fanatic Outlooks:** Extreme positions locked near axis endpoints. Impose stronger behavioral biases and heavier initiative modifiers.

**Example Combinations:**
- Regular: `Warlike + Materialistic + Expansionist` (conquest-focused, wealth-driven, expansionist)
- Fanatic: `Fanatic Warlike + Fanatic Xenophobic` (extreme militarism, extreme xenophobia)

### Aggregate Outlook Derivation

For aggregate entities (villages, guilds, armies):
- Collective outlook slots represent **dominant cultural ideals** or **strategic doctrines**
- Derived from member contributions and leadership influence
- Uses **sampling** rather than simple averages (top-performing members emphasized)

**Example:**
- Village with `Warlike + Materialistic` outlook
- Derived from: Leader's outlook (60% weight) + top 20% of villagers (40% weight)
- Result: Village culture favors conquest and wealth accumulation

---

## Disposition System

**Disposition:** Individual/entity stance toward external forces (player god, other entities, factions).

**Purpose:** Determines how entities react to and interact with others based on alignment/outlook compatibility.

### Disposition Calculation

```
Base Disposition = Alignment Compatibility + Outlook Compatibility + Behavioral Modifier
```

**Alignment Compatibility:**
- Matching alignments: +bonus (e.g., Good + Good = +16)
- Opposing alignments: -penalty (e.g., Good vs Evil = -20)
- Order conflicts: -penalty (e.g., Lawful vs Chaotic = -12)

**Outlook Compatibility:**
- Matching outlooks: +bonus (e.g., Warlike + Warlike = +15)
- Opposing outlooks: -penalty (e.g., Warlike vs Peaceful = -15)
- Special interactions: Some opposing outlooks create respectful rivalry (e.g., Materialistic vs Spiritual = -10 base, but +2 per debate)

**Behavioral Modifier:**
- Personality traits affect disposition (e.g., Forgiving +10, Vengeful -5)
- Mutual respect for shared traits (e.g., Bold + Bold = +8)

---

## System Effects

### Visual Effects (Proposed)

**Alignment Visual Changes:**
- Good alignment: Lighter colors, angelic motifs, peaceful aesthetics
- Evil alignment: Darker colors, demonic motifs, aggressive aesthetics
- Neutral: Balanced, natural appearance

**Outlook Visual Changes:**
- Warlike: Military architecture, weapon displays, fortifications
- Spiritual: Religious symbols, temples, ritual spaces
- Materialistic: Wealth displays, markets, trade hubs
- Scholarly: Libraries, research facilities, academic aesthetics

### Mechanical Effects (Proposed)

**Alignment Bonuses:**
- **Good Path:** +20% prayer from happy villagers, -25% heal miracle cost, +10% villager productivity, +50% offensive miracle cost (penalty)
- **Evil Path:** Prayer from fear/intimidation, -25% offensive miracle cost, +15% combat effectiveness, +50% heal miracle cost (penalty)

**Outlook Bonuses:**
- **Warlike:** +combat effectiveness, -diplomacy options
- **Spiritual:** +prayer generation, +miracle effectiveness
- **Materialistic:** +trade income, +craft quality
- **Scholarly:** +research speed, +tech unlocks

**Note:** Mechanical effects are **proposed but not yet implemented**. Current design status is "visual only" or "mechanical bonuses" - decision pending.

---

## Aggregate Entity Behavior

### Stat Derivation

Aggregate entities (villages, armies, guilds) compute stats by **sampling top-performing members** rather than simple averages:

- **Army Perception:** Sample upper scout cohort (top percentile)
- **Village Wisdom:** Drawn from ruling elites/council (top governance skill holders)
- **Band Initiative:** Controlled by leader/co-leader average initiative
- **Trading Guild Logistics:** Weighted by highest logistics-skilled members

**Rationale:** Emphasizes leadership and specialist impact, prevents noise from low-skill populations.

### Outlook Influence on Behavior

Purity + tri-axis alignment + active outlook set define **weighting tables** for:
- Event resolution rolls
- Village cultural behavior
- Decision-making priorities
- Interaction preferences

**Example:**
- Village with `Warlike + Expansionist` outlook
- Prioritizes: Military expansion, conquest, resource acquisition
- De-prioritizes: Diplomacy, trade, cultural development

---

## Outlook Shift (Education System)

**Teachers shift students' outlooks slowly** (NOT personality traits, NOT alignments).

**Mechanics:**
- Shift Rate: 1-5 points per year toward teacher's outlook values
- Example: Teacher `Warlike +60`, Student `Warlike +10` → After 5 years, Student `Warlike +25`

**Modifiers:**
- Teacher Charisma 70+: +50% shift rate
- Student Trusting (+60+): +30% shift rate
- Student Paranoid (+60+): -20% shift rate
- Intense indoctrination (private school, guild): +20% shift rate

**What Does NOT Transfer:**
- Teacher's alignment (Lawful Good) does NOT transfer
- Student remains True Neutral (alignment unchanged)
- Student does NOT become "Bold" trait (just shifts outlook value)

---

## Implementation Status

### Current State

**❌ Not Implemented:**
- No alignment components exist
- No good/evil tracking
- No outlook system
- No visual variation based on alignment
- Complete greenfield design

**⚠️ Design Decisions Pending:**
1. Is alignment in scope? Good vs Evil or skip entirely?
2. Who has alignment? Player god? Individual villagers? Villages?
3. How is it measured? Actions? Choices? Buildings? Miracles?
4. What does it affect? Visuals only? Mechanics? Both?
5. Is it reversible? Can player change alignment or locked in?

### Recommended Approach

**MVP Recommendation:** Skip alignment for MVP (too complex), add in content update (v2.0).

**If Implemented:**
- Start with single axis (Good ↔ Evil) for simplicity
- Add complexity (Order, Purity axes) if needed
- Visual changes first, mechanical bonuses later
- Alignment affects appearance, villager reactions, miracle costs/effects

---

## Space4X Adaptation

### Alignment Axes (Space4X)

**Proposed Axes:**
- **Moral Axis:** Benevolent ↔ Neutral ↔ Malevolent
- **Order Axis:** Authoritarian ↔ Neutral ↔ Libertarian
- **Purity Axis:** Pure ↔ Neutral ↔ Corrupt (or "Honor" ↔ Neutral ↔ "Ruthless")

**Examples:**
- "Authoritarian Benevolent Pure" = Benevolent dictatorship, honorable
- "Libertarian Malevolent Corrupt" = Ruthless free-market exploitation

### Outlook Families (Space4X)

**Proposed Outlooks:**
1. **Militaristic** – conquest, fleet expansion, dominance
2. **Diplomatic** – alliances, trade agreements, cooperation
3. **Expansionist** – colony growth, territory acquisition
4. **Isolationist** – fortification, inward focus, self-sufficiency
5. **Scientific** – research, technology, innovation
6. **Mercantile** – trade, markets, economic dominance
7. **Xenophilic** – open to aliens, cultural exchange
8. **Xenophobic** – alien exclusion, human supremacy
9. **Pacifist** – non-aggression, defensive only
10. **Aggressive** – preemptive strikes, conquest

### Space4X-Specific Considerations

1. **Faction Alignment:** Each faction has base alignment/outlook
2. **Ship Doctrine:** Ships inherit faction alignment/outlook
3. **Crew Compliance:** Crew alignment/outlook affects ship performance
4. **Colony Culture:** Colonies develop collective alignment/outlook over time
5. **Diplomatic Relations:** Alignment/outlook compatibility affects diplomacy

---

## Prefab Maker Integration

### Current State

**❌ Not Supported:**
- Prefab Maker does NOT currently support alignment/outlook variants
- No aggregate prefab system (villages, armies as composed entities)
- No outlook/alignment combo authoring

### Required Features (If Alignment/Outlook Implemented)

1. **Alignment Variants:**
   - Same prefab with different alignment values
   - Visual variants per alignment (Good vs Evil appearance)
   - Stat modifiers per alignment

2. **Outlook Variants:**
   - Same prefab with different outlook combinations
   - Visual variants per outlook (Warlike vs Peaceful appearance)
   - Behavioral modifiers per outlook

3. **Aggregate Prefabs:**
   - Village prefabs (composed of building prefabs)
   - Army prefabs (composed of unit prefabs)
   - Guild prefabs (composed of individual prefabs)
   - Collective alignment/outlook derived from components

4. **Combo System:**
   - Author alignment + outlook combinations
   - Preview visual variants
   - Validate combo compatibility
   - Export combo data for runtime

### Design Questions for Advisor

1. **Do we need aggregate prefabs?**
   - Villages as composed entities (buildings + villagers)?
   - Armies as composed entities (units + equipment)?
   - Or keep discrete prefabs only?

2. **Do we need alignment/outlook variants?**
   - Same building with Good vs Evil appearance?
   - Same ship with Militaristic vs Diplomatic appearance?
   - Or handle variants at runtime via material/color swaps?

3. **How should combos be authored?**
   - Separate variant prefabs per combo?
   - Base prefab + variant data (alignment/outlook modifiers)?
   - Runtime composition (base prefab + alignment/outlook overlay)?

4. **What visual changes are needed?**
   - Full model swaps (Good temple vs Evil temple)?
   - Material/color swaps (same model, different materials)?
   - Particle effects (good aura vs evil aura)?

---

## Recommendations

### For Godgame Prefab Maker

**If Alignment/Outlook Implemented:**
1. Add `Alignment` and `Outlook` fields to `PrefabTemplate` base class
2. Support variant authoring (same prefab, different alignment/outlook)
3. Add aggregate prefab system (villages, armies as composed entities)
4. Visual variant system (Good vs Evil models/materials)
5. Combo validation (ensure compatible alignments/outlooks)

### For Space4X Prefab Tool

**Recommended Features:**
1. **Faction Variants:** Same ship/station with different faction alignment/outlook
2. **Doctrine System:** Ships inherit faction doctrine (alignment + outlook combo)
3. **Crew Compliance:** Crew alignment/outlook affects ship performance
4. **Colony Culture:** Colonies develop collective alignment/outlook
5. **Diplomatic Compatibility:** Alignment/outlook affects faction relations

**Implementation Approach:**
- Base prefab + variant data (alignment/outlook modifiers)
- Runtime composition (base prefab + alignment/outlook overlay)
- Material/color swaps for visual variants (efficient)
- Full model swaps only for extreme variants (Good vs Evil temples)

---

## References

- `Docs/Concepts/Progression/Alignment_System.md` - Main alignment system design
- `Docs/Concepts/Meta/Generalized_Alignment_Framework.md` - Generalized framework (cross-project)
- `Docs/Concepts/Villagers/Village_Villager_Alignment.md` - Village/villager alignment interplay
- `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md` - Alignment/outlook taxonomy
- `Docs/Concepts/Villagers/Education_And_Tutoring_System.md` - Outlook shift mechanics

---

## Questions for Advisor

1. **Is alignment/outlook system in scope for Space4X?**
   - Or defer to post-launch content update?

2. **What Space4X-specific alignment axes are needed?**
   - Benevolent/Malevolent? Authoritarian/Libertarian? Other?

3. **What Space4X-specific outlooks are needed?**
   - Militaristic, Diplomatic, Scientific, etc.?

4. **Do we need aggregate prefabs?**
   - Fleets (composed of ships)?
   - Colonies (composed of stations/buildings)?
   - Or keep discrete prefabs only?

5. **How should visual variants be handled?**
   - Full model swaps?
   - Material/color swaps?
   - Particle effects?

6. **Should alignment/outlook affect gameplay mechanics?**
   - Or visual only?

