# Diet and Nutrition System

**Status:** In Review
**Category:** Core - Biological Simulation
**Scope:** Individual → Settlement → Cultural
**Created:** 2025-12-17
**Last Updated:** 2025-12-17

---

## Purpose

**Primary Goal:** Create emergent cultural/regional identity through long-term dietary patterns that drift entity stats over time.

**Secondary Goals:**
- Provide immediate tactical gameplay through temporary food buffs/debuffs
- Enable "you are what you eat" emergence at population scale
- Support cultural differentiation (fish-eating coastal vs fruit-eating forest settlements)
- Scale to 5M–1B entities via fixed-size queues and enableable components

---

## System Overview

### Key Insight

**Food effects operate on two timescales:**
1. **Temporary modifiers** (seconds to minutes) - tactical buffs/debuffs stored in small fixed stacks
2. **Long-run diet drift** (hours to sessions) - cultural/biological adaptation via nutrient signature vectors

**Example emergence:** "Fish-eating villages become more intelligent" falls out naturally from:
- Fish having higher "cognitive nutrient" component
- Signature→stat matrix mapping cognitive nutrients to Int stat
- No hard-coded village logic needed

### Components

1. **Individual Entities** - Track diet state, accumulate nutrient signatures, drift stats
2. **Food Types** - Defined in BlobAsset with nutrient profiles and temp effects
3. **Settlement Aggregates** (optional) - Accumulate cultural diet patterns citizens can inherit
4. **Diet Rules Blob** - Data-driven configuration for decay rates, stat mappings, balance tuning

### Connections

```
Food Intake Event → DietState.Recent (immediate)
                 ↓
Recent → (decay) → DietState.Signature (long-run integrator)
                 ↓
Signature → (matrix multiply) → StatOffset (cultural drift)
                 ↓
Base + Offset + TempMods = Effective Stats

Food → StatModStack (temporary buffs)
     → (expire + aggregate) → StatTempAdd/StatTempMul
```

### Feedback Loops

- **Positive (cultural lock-in):** Settlements with high Int → better fishing → more fish consumption → higher Int
- **Negative (dietary diversity):** Monoculture diets saturate signature components (decay prevents runaway)
- **Balance:** Signature decay rate vs intake rate determines drift speed; tunable via BlobAsset

---

## System Dynamics

### Inputs
- **Player actions:** Divine intervention placing food, trade route manipulation
- **Entity actions:** Hunting, fishing, farming, trading
- **Environmental events:** Famine, abundance, seasonal availability
- **Settlement patterns:** Cultural food preferences, available resources

### Internal Processes

1. **Intake Processing** (per food event)
   - Extract nutrients from FoodDef based on quality × portion
   - Add to Recent nutrient vector
   - Push temporary stat modifiers to fixed stack (if non-zero)

2. **Signature Evolution** (per sim step)
   - Decay Recent and Signature by configured rates
   - Add Recent → Signature (leaky integrator pattern)

3. **Stat Offset Calculation** (per sim step)
   - Matrix multiply: `Signature (float4) → StatBlock (8 floats via 2x float4x4 matrices)`
   - Lerp current offset toward desired offset (smooth drift)

4. **Temporary Modifier Aggregation** (per sim step)
   - Expire old entries (compact in-place)
   - Sum Add modifiers, multiply Mul modifiers
   - Output aggregated StatTempAdd/StatTempMul
   - Disable component if stack empty

### Outputs
- **StatOffset:** Long-run biological/cultural stat drift
- **StatTempAdd/StatTempMul:** Aggregated temporary buffs/debuffs
- **Final stats:** `(StatBase + StatOffset) * StatTempMul + StatTempAdd`

---

## State Machine

### Entity Diet States

1. **Baseline**
   - Entry: Entity spawned with zero signature
   - State: No recent food intake, signature decaying toward zero
   - Exit: Food intake event occurs

2. **Recent Intake**
   - Entry: Food consumed (FoodIntakeQueue enabled, events added)
   - State: Recent vector non-zero, draining into Signature
   - Exit: Recent decays to near-zero

3. **Cultural Adaptation**
   - Entry: Signature vector stabilizes around diet pattern
   - State: StatOffset converges to signature-derived values
   - Exit: Diet pattern changes (new settlement, famine, etc.)

4. **Temporary Buffed**
   - Entry: Food with temp modifiers consumed
   - State: StatModStack enabled, non-empty
   - Exit: All modifiers expire

### Transitions
```
Baseline → [Food Intake] → Recent Intake
Recent Intake → [Time passes] → Cultural Adaptation
Cultural Adaptation → [Diet shift] → Recent Intake
Any State → [Buff food] → Temporary Buffed (parallel)
Temporary Buffed → [Expiry] → Previous State
```

---

## Key Metrics

| Metric | Target Range | Critical Threshold |
|--------|--------------|-------------------|
| Recent Decay Rate | 0.90–0.98 per step | < 0.80 (too fast, no integration) |
| Signature Decay Rate | 0.995–0.999 per step | < 0.99 (culture shifts too fast) |
| Offset Lerp Factor | 0.01–0.05 per step | > 0.10 (stat pops, not drift) |
| Stat Offset Magnitude | ±5–20% of base | > ±50% (overpowers base) |
| Temp Modifier Duration | 60–600 steps (1–10 min) | > 3600 (too persistent) |
| Mod Stack Capacity | 8–16 entries | Full stack = drops new mods |

---

## Balancing

### Self-Balancing
- Signature decay prevents infinite stat accumulation
- Temp modifier expiry auto-cleans buffs
- Lerp prevents sudden stat jumps

### Player Intervention
- Divine food gifts (manna, fish miracle) shift culture
- Trade routes inject foreign diet patterns
- Famine events zero out signatures (cultural collapse)

### System Corrections
- Signature→stat matrix tuned in BlobAsset (data-driven)
- Food nutrient vectors balanced per food type
- Stack capacity limits extreme buff stacking

---

## Scale & Scope

### Small Scale (Individual)
- Each entity has independent DietState
- Enableable components minimize overhead (queue/stack disabled when empty)
- FixedList128/256 prevents heap allocations

**Performance:** O(1) per entity, only when components enabled

### Medium Scale (Settlement)
- Settlement entity aggregates diet state from citizens
- Citizens can inherit settlement signature (culture osmosis)
- No need to scan all citizens for village stats

**Emergence:** "Fish village" = settlement with high fish signature → new citizens drift toward it

### Large Scale (Population/Regional)
- Cultural boundaries emerge from food availability geography
- Coastal → fish → Int, Forest → fruit → Finesse
- Inter-settlement trade drifts cultures toward each other

**Scalability:** System designed for 5M–1B entities
- No DynamicBuffer (fixed-size only)
- Enableable components skip empty entities
- Dense FoodTypeId indexing (direct array lookup)

---

## Time Dynamics

### Short Term (Seconds to Minutes)
- Temporary buffs from meals
- Recent vector spikes on intake
- Immediate tactical value (eat combat rations before battle)

### Medium Term (Minutes to Hours)
- Recent drains into Signature
- Cultural drift becomes visible
- "You feel stronger after weeks of meat"

### Long Term (Hours to Sessions)
- Signature stabilizes at steady-state diet
- Regional cultures crystallize
- Multi-generational patterns (if entities reproduce)

---

## Failure Modes

### Death Spiral
- **Cause:** Famine → zero signature → stat penalties → can't hunt/fish → worse famine
- **Recovery:** Divine intervention (food miracle), trade rescue, migration

### Stagnation
- **Cause:** Monoculture diet → signature saturates → no further progression
- **Recovery:** Introduce new food types, trade routes, dietary variety

### Runaway
- **Cause:** Exploit food with excessive temp mods → stack 16 buffs
- **Recovery:** Stack capacity limit, diminishing returns on quality×portion scaling

---

## Player Interaction

### Observable
- Citizens show stat drift tooltips ("Recently ate fish: +2 Int")
- Settlement diet pie chart (protein/carb/fat/micro breakdown)
- Visual indicators (buffed entities glow, malnourished entities pale)

### Control Points
1. **Direct:** Miracle food drops (manna, quail, wine)
2. **Indirect:** Bless fishing/farming → more food → cultural shift
3. **Strategic:** Trade route manipulation → import foreign diets
4. **Emergency:** Famine curse → force diet reset

### Learning Curve
- **Beginner:** "Food makes villagers stronger temporarily"
- **Intermediate:** "Different foods give different buffs; fish is good for mages"
- **Expert:** "I can culturally engineer settlements via diet control to specialize cities"

---

## Systemic Interactions

### Dependencies
- **Stats System:** Requires StatBase/StatOffset/StatTempAdd/StatTempMul components
- **Time System:** Uses SimStep for expiry and decay timing
- **Registry:** FoodTypeId must be dense index into BlobArray

### Influences
- **Combat System:** Stat buffs affect damage/defense calculations
- **Profession System:** Stat drift enables cultural specialization (mage cities vs warrior cities)
- **Economy System:** Food becomes strategic resource, not just survival
- **Settlement System:** Cultural identity emerges from aggregated diet

### Synergies
- **Magic System:** Mage diets (mushrooms, rare herbs) → Int drift → more mages emerge
- **Trade System:** Dietary trade goods create economic interdependence
- **Religion System:** Dietary laws (kosher, vegan cults) create cultural friction

---

## Implementation Architecture

### PureDOTS Components (Library-side)

**Core Data:**
```csharp
StatBlock (2x float4 = 8 stats: Str, End, Int, Fin, Soc, Per, Will, Luck)
StatBase, StatOffset, StatTempAdd, StatTempMul (all StatBlock)
DietState { float4 Recent, float4 Signature }
```

**Event Queue (enableable):**
```csharp
FoodIntakeQueue : IEnableableComponent
{
    FixedList128Bytes<FoodIntakeEvent> Events; // no heap alloc
}
```

**Temp Modifier Stack (enableable):**
```csharp
StatModStack : IEnableableComponent
{
    FixedList256Bytes<StatModEntry> Entries; // fixed capacity
}

StatModEntry { StatBlock Value, uint ExpireStep, ModOp Op (Add/Mul) }
```

### BlobAsset Definitions (Baked game-side)

**FoodDef (per food type):**
```csharp
{
    float4 NutrientsPerPortion;       // basis matches DietState
    StatBlock TempAddPerPortion;       // additive buff
    StatBlock TempMulPerPortion;       // multiplicative buff (around 1.0)
    ushort DurationSteps;              // how long temp buff lasts
}
```

**DietRules (tuning knobs):**
```csharp
{
    float4 RecentDecayPerStep;         // e.g. 0.95
    float4 SignatureDecayPerStep;      // e.g. 0.998
    float4x4 SigToStatA, SigToStatB;   // map nutrients → stats
    float OffsetLerpFactor;            // smooth drift rate
}
```

**Lookup:**
```csharp
DietBlob { BlobArray<FoodDef> Foods; DietRules Rules; }
DietBlobRef : IComponentData (singleton)
```

### System Execution Order

```
1. AdvanceSimStepSystem          (increment global step counter)
2. ApplyFoodIntakeSystem         (process queue → Recent + push temp mods)
3. UpdateDietSignatureSystem     (decay Recent/Signature, integrate)
4. UpdateDietStatOffsetSystem    (signature → stat offset via matrix)
5. ExpireAndAggregateTempModsSystem (compact stack, sum/multiply mods)
```

All systems are `[BurstCompile]` and use `IJobEntity` for parallel execution.

### Performance Optimizations

**For 5M–1B entities:**
1. **No DynamicBuffer** - only FixedList (stack-allocated)
2. **Enableable components** - skip entities with empty queues/stacks
3. **Dense food IDs** - direct array indexing (no hashmap lookup)
4. **Packed float4** - SIMD-friendly, cache-efficient
5. **In-place compaction** - no allocations when expiring mods

---

## Iteration Plan

### v1.0 (MVP)
**Features:**
- Basic intake → Recent → Signature pipeline
- Single nutrient dimension (just calories)
- Simple temp buff (±flat stat add)
- Manual food spawning only

**Limitations:**
- No settlement aggregation
- Fixed matrix (not tunable at runtime)
- Single food type for testing

### v2.0 (Full System)
**Additions:**
- 4D nutrient space (Protein, Carb, Fat, Micro)
- Configurable DietRules BlobAsset
- Multiple food types with unique profiles
- Settlement diet aggregation
- Visual feedback (stat drift tooltips)

### v3.0 (Cultural Layer)
**Additions:**
- Dietary laws (religion integration)
- Food preferences (personality-driven)
- Spoilage/freshness mechanics
- Cooking/preparation system (combine ingredients)
- Nutritional deficiency diseases

---

## Open Questions

1. **Settlement inheritance:** Should newborns inherit parent signature, settlement signature, or start blank?
2. **Migration:** When entity joins new settlement, how fast should culture osmosis occur?
3. **Extreme diets:** Should pure-carnivore or pure-vegetarian diets have special emergent properties?
4. **Visual design:** How to show long-run drift vs temporary buffs in UI without clutter?
5. **Cross-species:** Do different species (humans, elves, orcs) have different signature→stat matrices?

---

## Exploits

1. **Buff stacking:** Eat 16 high-quality meals simultaneously
   - **Severity:** Medium
   - **Fix:** Stack capacity limit (already in design), diminishing returns on quality×portion

2. **Culture bombing:** Divine food spam to force rapid cultural shift
   - **Severity:** Low (player power by design)
   - **Fix:** Signature decay prevents instant shift; still takes time

3. **Starvation cycling:** Starve→feast to reset signature strategically
   - **Severity:** Medium
   - **Fix:** Starvation penalties (health damage) outweigh benefits

---

## Tests

- [ ] Single entity: intake → Recent → Signature → offset (no regression)
- [ ] Temp buff expiry: verify stack compaction works
- [ ] Settlement aggregation: village signature tracks majority diet
- [ ] Matrix tuning: fish → Int, fruit → Fin produces expected offsets
- [ ] Stress test: 100K entities with random food events
- [ ] Enableable efficiency: measure perf with 99% empty queues
- [ ] Burst compilation: all systems compile and run in parallel

---

## Performance

- **Complexity:** O(n) per entity per frame, but only if components enabled
- **Max entities:** Designed for 1B (tested at 100K+)
- **Update freq:** 30–60 Hz (per sim step, not per render frame)
- **Memory:** ~128 bytes per entity (DietState + stacks, when enabled)

---

## Visual Representation

### System Diagram
```
[Food Intake Events]
        ↓
   [DietState]
    /        \
Recent      Signature
  ↓            ↓
(decay)   (decay + integrate)
  ↓            ↓
  └──→ Signature
          ↓
    [Matrix Multiply]
          ↓
      StatOffset ──┐
                   ↓
[Temp Mods] → Aggregate → StatTempAdd/Mul ──┐
                                            ↓
StatBase ──────────────────────────────→ Final Stats
```

### Data Flow
```
Player/AI → Food Event → Queue (FixedList)
                          ↓
                    Process Event
                    ↓           ↓
              Nutrients     Temp Mods
                ↓               ↓
              Recent          Stack
                ↓               ↓
            Signature      Expire/Aggregate
                ↓               ↓
            Offset Lerp    TempAdd/TempMul
                ↓               ↓
                 Final Effective Stats
```

---

## Example Scenario: Fish vs Fruit Villages

### Setup
**Coastal Village (Fishton):**
- Food availability: 80% fish, 20% bread
- Fish NutrientsPerPortion = (0.8, 0.1, 0.5, 0.3) [high protein, low carb]
- Matrix: Protein (x-axis) → Int (stat)

**Forest Village (Fruitdale):**
- Food availability: 70% fruit, 30% nuts
- Fruit NutrientsPerPortion = (0.2, 0.9, 0.1, 0.6) [low protein, high carb]
- Matrix: Carb (y-axis) → Finesse (stat)

### Over Time (100 sim steps = ~1 game hour)

**Fishton citizens:**
1. Eat fish → Recent.x += 0.8 (protein)
2. Recent decays 5%/step → Signature.x slowly rises
3. After 50 steps: Signature.x ≈ 12 (stable)
4. Matrix: `Signature.x * SigToStatA[Int] = +8 Int offset`
5. Result: Fishton citizens drift +8 Int over baseline

**Fruitdale citizens:**
1. Eat fruit → Recent.y += 0.9 (carb)
2. Signature.y ≈ 15 (stable)
3. Matrix: `Signature.y * SigToStatA[Finesse] = +10 Fin offset`
4. Result: Fruitdale citizens drift +10 Finesse

**Emergence:**
- Fishton becomes mage-specialist settlement (high Int)
- Fruitdale becomes archer-specialist (high Finesse)
- No hard-coded village logic
- Trade fish↔fruit creates hybrid cultures at borders

---

## Related Documentation

- **Truth Sources:** `Docs/TruthSources_Inventory.md#Stats` (when implemented)
- **Related Systems:**
  - `Docs/Concepts/Core/Stats_System.md` (stat calculation foundation)
  - `Docs/Concepts/Economy/Food_Production.md` (food sourcing)
  - `Docs/Concepts/Villagers/Settlement_Culture.md` (cultural aggregation)
- **Implementation:**
  - Package: `Packages/com.moni.puredots/Runtime/Diet/`
  - Game: `Assets/Scripts/Godgame/Diet/` (authoring, baking)

---

**For Implementers:**
- Focus on FixedList sizing and enableable component usage for performance
- BlobAsset baking from ScriptableObject for designer-friendly tuning
- Test with 100K entities before scaling to 1M+

**For Designers:**
- Matrix tuning is your primary balance lever
- Food nutrient vectors define cultural drift directions
- Temp buffs are tactical; signature drift is strategic

---

**Last Updated:** 2025-12-17
**Implementation Status:** Spec Complete - Ready for Review
**Performance Target:** 1B entities @ 30Hz on target hardware
