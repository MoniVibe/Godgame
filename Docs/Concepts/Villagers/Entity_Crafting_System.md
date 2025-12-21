# Entity Crafting System

**Status:** Draft  
**Category:** Villagers - Birth & Genetics  
**Scope:** Individual → Generational  
**Created:** 2025-12-21  
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Allow players to influence entity birth and tailor entities within reasonable constraints, respecting genealogy. Players can reinforce desirable traits or remove undesirable ones using mana, with compounding costs that increase with each modification.

**Secondary Goals:**
- Enable strategic entity design (create specialized villagers for specific roles)
- Maintain genealogical integrity (children inherit from parents)
- Balance player power (compounding costs prevent excessive manipulation)
- Create meaningful choices (which traits to prioritize, when to intervene)

---

## Core Concept

**Birth Influence:** Players can intervene during entity conception/birth to influence trait inheritance and stat distribution. Modifications must respect genealogical constraints (can't completely override inheritance) and cost mana that compounds with each change.

**Trait Reinforcement/Removal:** Players can:
- **Reinforce traits:** Increase likelihood/inheritance strength of desirable traits
- **Remove traits:** Decrease or eliminate undesirable traits
- **Adjust stats:** Modify base stat distributions within reasonable ranges

**Genealogical Constraints:** Entities inherit traits and stat ranges from parents. Player modifications work within these inherited constraints - you can strengthen weak inherited traits or reduce strong ones, but cannot create traits that weren't inherited.

**Compounding Costs:** Each modification during a single birth event costs more mana than the last, preventing unlimited customization and creating strategic decisions about which modifications are worth the cost.

---

## How It Works

### Birth Process Overview

1. **Parents Selected/Matched:** Entities pair up for reproduction (romance, arranged marriage, etc.)
2. **Inheritance Calculation:** System calculates inherited traits and stat ranges from both parents
3. **Player Intervention Window:** Player can modify traits/stats (limited window, typically conception or birth)
4. **Modifications Applied:** Player's changes are applied with compounding mana costs
5. **Entity Born:** New entity spawns with final trait/stat configuration

### Genealogy & Inheritance

**Inherited Traits:**
- Traits are inherited from parents (Lawful, Chaotic, Good, Evil, Warlike, Peaceful, Corrupt, etc.)
- Inheritance strength depends on:
  - Parent trait strength (strong trait in parent = more likely to inherit)
  - Dominance rules (some traits dominant over others)
  - Random variation (natural genetic diversity)

**Inherited Stat Ranges:**
- Base stats (Physique, Finesse, Will, Intelligence, etc.) inherit ranges from parents
- Example: If both parents have high Intelligence (70-90), child inherits range (65-95)
- Natural variation within range creates diversity

**Genealogical Constraints:**
- Players cannot add traits that weren't inherited from either parent
- Players cannot push stats beyond reasonable range expansions (e.g., can't create 100 stat from 50-range parents)
- Constraints ensure genealogical integrity (children still resemble parents)

---

### Player Modifications

#### Trait Modifications

**Reinforce Trait:**
- Increases trait strength/inheritance likelihood
- Cost: BaseCost × (1 + ModificationCount)^1.5
- Effect: Trait becomes more prominent, more likely to express

**Remove/Weaken Trait:**
- Decreases trait strength or eliminates it (if weak enough)
- Cost: BaseCost × (1 + ModificationCount)^1.5 × 1.2 (20% more expensive than reinforce)
- Effect: Trait becomes less prominent or removed entirely
- Cannot remove if trait is too strongly inherited (genealogical constraint)

**Modification Limits:**
- Can only modify traits that exist in inheritance (cannot add new traits)
- Each trait can be modified once per birth event
- Total modifications limited by reasonable constraints (typically 3-5 traits max)

#### Stat Modifications

**Adjust Stat Range:**
- Shifts stat range up or down within genealogical limits
- Cost: BaseCost × (1 + ModificationCount)^1.5 × 0.8 (cheaper than traits)
- Effect: Stat distribution shifts (e.g., Intelligence 50-70 → 55-75)

**Enhance Stat Potential:**
- Increases maximum potential within range
- Cost: BaseCost × (1 + ModificationCount)^1.5 × 1.5 (more expensive)
- Effect: Stat can reach higher values (e.g., max 70 → max 80)

**Modification Limits:**
- Cannot push stats beyond +20% of inherited range (genealogical constraint)
- Each stat can be modified once per birth event
- Total stat modifications limited (typically 2-4 stats)

---

### Compounding Cost System

**Base Costs:**
```
Trait Reinforcement: 500 mana (base)
Trait Removal: 600 mana (base, 20% more expensive)
Stat Adjustment: 400 mana (base)
Stat Enhancement: 750 mana (base, 50% more expensive)
```

**Compounding Formula:**
```
ModificationCount = Number of modifications already made in this birth event
CostMultiplier = (1 + ModificationCount)^1.5

FinalCost = BaseCost × CostMultiplier × TypeModifier
```

**Cost Progression Examples:**

**Birth Event 1: Reinforce Lawful trait**
- ModificationCount = 0
- Cost = 500 × (1 + 0)^1.5 = 500 mana

**Birth Event 2: Remove Chaotic trait**
- ModificationCount = 1
- Cost = 600 × (1 + 1)^1.5 × 1.2 = 600 × 2.83 × 1.2 = 2,038 mana

**Birth Event 3: Enhance Intelligence stat**
- ModificationCount = 2
- Cost = 750 × (1 + 2)^1.5 × 1.5 = 750 × 5.20 × 1.5 = 5,850 mana

**Birth Event 4: Adjust Finesse stat**
- ModificationCount = 3
- Cost = 400 × (1 + 3)^1.5 × 0.8 = 400 × 8.0 × 0.8 = 2,560 mana

**Birth Event 5: Reinforce Good trait**
- ModificationCount = 4
- Cost = 500 × (1 + 4)^1.5 = 500 × 11.18 = 5,590 mana

**Total Cost for 5 modifications: 16,538 mana**

---

### Reasonable Constraints

**Genealogical Limits:**
- Cannot add traits not present in either parent
- Cannot remove traits that are strongly inherited (>70% inheritance strength) without extreme cost (10x multiplier)
- Stat modifications limited to ±20% of inherited range
- Cannot create "perfect" entities (stats capped at 95, traits have natural variation)

**Modification Limits:**
- Maximum 5-7 total modifications per birth event (prevents excessive manipulation)
- Each trait/stat can only be modified once per event
- Player must have sufficient mana for all desired modifications (cannot partially apply)

**Natural Variation:**
- Even with modifications, entities retain some natural variation
- Perfect control is impossible (entities still have personality, random traits)
- Prevents creating identical "clones"

---

## Key Parameters (All Tunable)

| Parameter | Default Value | Reasoning | Tunable |
|-----------|--------------|-----------|---------|
| Base Trait Reinforcement Cost | 500 mana | Significant but not prohibitive | ✅ Yes (200-1000) |
| Base Trait Removal Cost | 600 mana | Slightly more expensive (harder to remove) | ✅ Yes (400-1200) |
| Base Stat Adjustment Cost | 400 mana | Cheaper than traits | ✅ Yes (200-800) |
| Base Stat Enhancement Cost | 750 mana | More expensive (stronger effect) | ✅ Yes (500-1500) |
| Compounding Exponent | 1.5 | Steep but not exponential | ✅ Yes (1.2-2.0) |
| Max Modifications Per Birth | 7 | Prevents excessive manipulation | ✅ Yes (3-10) |
| Stat Range Limit | ±20% | Maintains genealogical integrity | ✅ Yes (±10% to ±30%) |
| Strong Trait Removal Multiplier | 10x | Makes removing strong traits very expensive | ✅ Yes (5x-20x) |
| Strong Trait Threshold | 70% inheritance | Threshold for expensive removal | ✅ Yes (60%-80%) |

---

## Examples

### Example 1: Reinforce Desirable Traits

**Setup:**
- Parents: Both Lawful Good, high Intelligence (70-85 range)
- Player wants to ensure child inherits Lawful Good strongly and has high Intelligence

**Modifications:**
1. Reinforce Lawful trait (500 mana)
2. Reinforce Good trait (500 × 2.83 = 1,415 mana)
3. Enhance Intelligence stat (750 × 5.20 × 1.5 = 5,850 mana)
4. Adjust Will stat upward (400 × 8.0 × 0.8 = 2,560 mana)

**Total Cost: 10,325 mana**

**Result:**
- Child strongly Lawful Good (high trait inheritance)
- Intelligence range 75-90 (enhanced from 70-85)
- Will stat improved
- Child likely to become virtuous scholar/priest

### Example 2: Remove Undesirable Traits

**Setup:**
- Parents: One Chaotic Evil, one Neutral Good
- Child inherits Chaotic and Evil traits (undesirable for player's goals)
- Player wants to remove Chaotic, weaken Evil

**Modifications:**
1. Remove Chaotic trait (600 × 1.0 × 1.2 = 720 mana)
2. Weaken Evil trait (600 × 2.83 × 1.2 = 2,038 mana) - reduces to Neutral
3. Reinforce Good trait (500 × 5.20 = 2,600 mana)

**Total Cost: 5,358 mana**

**Result:**
- Child loses Chaotic trait entirely
- Evil trait weakened to Neutral (balanced)
- Good trait reinforced
- Child becomes Neutral Good (acceptable alignment)

### Example 3: Create Specialized Entity

**Setup:**
- Parents: Skilled craftsmen (high Finesse, moderate Intelligence)
- Player wants to create master craftsman (very high Finesse, good Intelligence)

**Modifications:**
1. Enhance Finesse stat (750 × 1.0 × 1.5 = 1,125 mana)
2. Adjust Intelligence upward (400 × 2.83 × 0.8 = 906 mana)
3. Reinforce Warlike trait (for discipline, 500 × 5.20 = 2,600 mana)

**Total Cost: 4,631 mana**

**Result:**
- Finesse range enhanced (e.g., 75-95 instead of 65-85)
- Intelligence improved
- Warlike trait reinforced (disciplined craftsman)
- Child destined for crafting excellence

### Example 4: Compounding Cost Limit

**Setup:**
- Player wants to create "perfect" entity with 7 modifications
- Has 20,000 mana available

**Modifications Planned:**
1-3: 500 + 1,415 + 2,600 = 4,515 mana
4-5: 5,850 + 2,560 = 8,410 mana
6-7: Would cost ~15,000+ mana each (total exceeds budget)

**Decision:** Player stops at 5 modifications (13,925 mana spent), accepts that entity won't be "perfect" but is still highly optimized.

**Result:** Strategic limitation - player must prioritize most important modifications, cannot create perfect entities easily.

---

## System Interactions

### Integration with Reproduction System

**Birth Events:**
- System triggers player intervention window when birth occurs
- Player has limited time to make modifications (typically 30-60 seconds in-game)
- If player doesn't intervene, entity is born with natural inheritance only

**Multiple Births:**
- Each birth event is independent (compounding resets per birth)
- Player can craft multiple entities over time
- Strategic resource management (save mana for important births)

### Integration with Trait System

**Trait Inheritance:**
- Inherited traits from parent system feed into crafting system
- Player modifications adjust inheritance strength/expression
- Modified traits still follow trait interaction rules (Lawful vs Chaotic conflict, etc.)

**Trait Expression:**
- Reinforced traits are more likely to express strongly in entity behavior
- Removed/weakened traits may not express at all or express weakly
- Natural variation still applies (not 100% deterministic)

### Integration with Stat System

**Stat Ranges:**
- Inherited stat ranges from parent system
- Player modifications shift these ranges
- Final stats rolled within modified ranges (still has randomness)

**Stat Progression:**
- Modified stats affect starting stats
- Entity still progresses through experience/training
- High potential from modifications + progression = exceptional entities

---

## Edge Cases

### Insufficient Mana

**Scenario:** Player starts modifications but runs out of mana partway through
**Resolution:** 
- System prevents starting modifications if insufficient mana for all planned changes
- Player must commit to all modifications before applying (all-or-nothing)
- Alternative: System could allow partial application with refund for uncommitted changes

### Extremely Strong Inheritance

**Scenario:** Both parents have very strong trait (95% inheritance strength), player wants to remove it
**Resolution:**
- Removal requires 10x multiplier (extremely expensive)
- May still be impossible if inheritance > 95%
- Creates meaningful choice: accept strong trait or pay enormous cost

### No Inheritance Available

**Scenario:** Player wants to add trait that neither parent has
**Resolution:**
- System prevents this (genealogical constraint)
- Player cannot create traits from nothing
- Must wait for different parent pair or accept limitations

---

## Technical Considerations

### Components Needed

```csharp
// Birth modification request (during intervention window)
public struct BirthModificationRequest : IComponentData
{
    public Entity Parent1;
    public Entity Parent2;
    public ModificationType Type;              // ReinforceTrait, RemoveTrait, AdjustStat, EnhanceStat
    public TraitType TargetTrait;              // If trait modification
    public StatType TargetStat;                // If stat modification
    public float ModificationStrength;         // How much to modify (0.0 to 1.0)
    public byte ModificationIndex;             // Which modification this is (for compounding)
}

// Birth modification state (tracking during event)
public struct BirthModificationState : IComponentData
{
    public Entity Parent1;
    public Entity Parent2;
    public byte ModificationCount;             // Current count (for compounding)
    public float TotalCost;                    // Running total
    public bool IsComplete;                    // All modifications applied
    public uint BirthTick;                     // When birth event started
}

// Inherited traits/stats (calculated from parents)
public struct InheritedGenetics : IComponentData
{
    public BlobAssetReference<TraitInheritanceData> Traits;    // Inherited trait strengths
    public BlobAssetReference<StatRangeData> StatRanges;       // Inherited stat ranges
    public byte Parent1EntityId;
    public byte Parent2EntityId;
}
```

### Systems Needed

1. **InheritanceCalculationSystem** - Calculates inherited traits/stats from parents
2. **BirthInterventionSystem** - Manages player intervention window, UI, input
3. **ModificationCostSystem** - Calculates compounding costs for modifications
4. **ModificationApplicationSystem** - Applies player modifications to birth outcome
5. **GenealogyValidationSystem** - Ensures modifications respect genealogical constraints

---

## Balance Considerations

### Compounding Cost Balance

**Risk:** Compounding too steep (players never modify) vs too gentle (unlimited modifications)
**Mitigation:**
- Exponent 1.5 creates noticeable but not prohibitive increase
- First 2-3 modifications remain affordable
- 4+ modifications become expensive but not impossible
- Creates strategic choices (which modifications are worth the cost)

### Genealogical Integrity

**Risk:** Players create "perfect" entities that break immersion (children nothing like parents)
**Mitigation:**
- Cannot add traits not in parents
- Stat modifications limited to ±20% range
- Natural variation still applies
- Constraints ensure children still resemble parents

### Resource Economy

**Risk:** Players hoard mana for entity crafting, ignoring other gameplay
**Mitigation:**
- High costs encourage selective use
- Other systems (miracles, pleads) compete for mana
- Strategic choice: invest in entities vs immediate power

---

## Future Enhancements

### Advanced Modifications

- **Trait Fusion:** Combine traits from both parents in new ways (very expensive)
- **Stat Specialization:** Extreme focus on single stat (trade-offs required)
- **Gene Lineage Tracking:** Track crafted entities' descendants, maintain "bloodline" bonuses

### Progression Gates

- **Crafting Mastery:** Unlock ability to make more modifications per birth
- **Genealogical Expertise:** Reduce constraints (can modify ±30% instead of ±20%)
- **Divine Intervention:** Special abilities that bypass some constraints (requires progression)

### Villager Awareness

- **Crafted Entity Recognition:** Villagers may recognize divinely-crafted entities
- **Bloodline Reputation:** Crafted entities' descendants gain prestige
- **Social Dynamics:** Crafted entities may be treated differently (admired, feared, or resented)

---

## Open Questions

1. **When does intervention window occur?** (Conception, birth, or both?)
2. **Can players modify entities after birth?** (Post-birth modifications would be different system)
3. **How do crafted entities age/develop?** (Do modifications affect growth curves?)
4. **Can AI gods also craft entities?** (Competition for "perfect" lineages)
5. **How do villagers react to crafted entities?** (Social dynamics, acceptance/rejection)

---

## Related Concepts

- **Individual Progression:** `Docs/Concepts/Villagers/Individual_Progression_System.md`
- **Diet & Nutrition:** `Docs/Concepts/Core/Diet_And_Nutrition_System.md` (mentions inheritance)
- **Trait Systems:** `Docs/Concepts/Politics/Faction_Relations_System.md` (trait definitions)
- **Stats System:** (To be linked when stats system documented)

---

**For Implementers:** 
- Start with basic trait inheritance calculation, simple modification system (reinforce/remove traits, adjust stats)
- Implement compounding cost formula
- Add genealogical constraint validation
- Add UI for birth intervention window

**For Designers:** 
- Balance compounding exponent (1.5 default, but may need tuning)
- Define genealogical constraints (how strict should inheritance be?)
- Determine modification limits (how many modifications is reasonable?)
- Consider social/immersion implications of crafted entities

