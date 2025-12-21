# Miracle Crafting System

**Status:** Draft  
**Category:** Miracle - Custom Creation & Distribution  
**Scope:** Player God → Villagers → Pedestals  
**Created:** 2025-12-21  
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Allow players (and other god entities) to modify and customize baseline miracles they know, adjusting intensity, duration, area, targeting, and other parameters. Enable villagers to create miracle pedestals that duplicate and dispense these crafted (modified) miracles, with quality affecting cooldown, authenticity, and bonuses.

**Secondary Goals:**
- Provide baseline miracles accessible via hotkeys/selection menu (immediate use)
- Enable customization of known miracles through crafting system
- Lock advanced modifications behind progression gates
- Enable strategic miracle placement via pedestals
- Integrate with crafting/profession systems (skill and material quality matter)

---

## Core Concept

**Baseline Miracles:** Players start with baseline miracle versions (Fire, Water, Heal, Lightning, etc.) that are immediately accessible via hotkeys or selection menu. These baseline miracles have default parameters (standard intensity, duration, area, etc.) and can be cast directly without crafting.

**Miracle Crafting:** Players can modify baseline miracles they know by adjusting parameters (intensity, duration, area, targeting, delivery method modifications, addons, etc.). Crafting costs mana and has a cooldown. Crafted (modified) versions are stored as variants and can be assigned to pedestals. Players can only craft modifications for miracles they have learned/unlocked.

**Progression Gates:** Advanced modifications (addons, extreme tunability) are locked behind gameplay progression (discoveries, achievements, faith thresholds, etc.). Basic modifications (intensity, duration, area) are available once a miracle is known.

**Miracle Pedestals:** Villagers (craftsmen, priests, mages) can craft pedestals that duplicate and dispense miracles (baseline or crafted variants). Pedestal quality (determined by crafter skill + material quality) affects:
- **Cooldown Rate:** How fast pedestal accumulates mana to cast (mana cost vs accumulation rate)
- **Authenticity:** How closely the pedestal's miracle matches the original (quality loss)
- **Bonuses/Penalties:** Quality can add beneficial modifiers or introduce imperfections

**Key Insight:** Baseline miracles provide immediate gameplay, while crafting enables customization and strategic optimization. High-quality pedestals act as reliable miracle dispensers, while low-quality ones may introduce risks or inefficiencies. This creates strategic value for skilled craftsmen and rare materials.

---

## How It Works

### Baseline Miracles (Immediate Access)

**Access Method:**
- **Hotkeys:** Players assign baseline miracles to number keys (1, 2, 3, etc.)
- **Selection Menu:** Radial menu or panel for selecting miracles
- **Direct Casting:** Baseline miracles can be cast immediately without crafting

**Baseline Miracle Properties:**
- Each miracle (Fire, Water, Heal, Lightning, etc.) has default parameters
- Standard intensity, duration, area, targeting as defined in individual miracle docs
- No crafting required - immediate use
- Costs prayer/mana as per baseline miracle specifications

**Known Miracles:**
- Players start with some baseline miracles (defined by progression/story)
- Additional baseline miracles unlocked through gameplay (discoveries, achievements, etc.)
- Cannot craft modifications for unknown miracles

### Miracle Crafting (Modifying Known Miracles)

#### Crafting Process

1. **Select Known Miracle:** Choose from baseline miracles the player has learned/unlocked
2. **Modify Parameters:** Adjust tunable aspects of the miracle
3. **Add Modifiers (if unlocked):** Apply addons if progression gates are met
4. **Craft Variant:** Create modified version (costs mana, cooldown applies)
5. **Store Variant:** Modified miracle saved as variant (can assign to pedestals or use directly)

#### Crafting Parameters (Basic - Always Available)

These modifications are available once a miracle is known:

1. **Intensity**
   - Low (0.75x), Standard (1.0x - baseline), High (1.5x), Extreme (2x)
   - Affects damage/healing/effect magnitude
   - Scales mana cost (1x → 1.25x → 1.5x → 2.25x)

2. **Duration**
   - Instant (baseline for instant miracles)
   - Short (baseline -50%), Standard (baseline), Long (baseline +100%), Extended (baseline +200%)
   - Duration modifications available only for miracles that support duration changes

3. **Area of Effect**
   - Focused (baseline -30%), Standard (baseline), Expanded (baseline +50%), Massive (baseline +100%)
   - Area modifications available only for area-effect miracles

4. **Targeting** (Limited)
   - Default targeting (per baseline miracle)
   - Friendly-only modifier (if supported)
   - Enemy-only modifier (if supported)

#### Advanced Modifiers (Locked Behind Progression)

These require progression gates (to be conceptualized):

1. **Delivery Method Modifications**
   - Change delivery type (beam ↔ burst ↔ projectile)
   - Requires: [Progression Gate - To Be Defined]

2. **Addons**
   - Spread (fire spreads, healing chains, etc.)
   - Stacking (effects stack vs replace)
   - Penetration (ignores resistances)
   - Cleansing (removes status effects)
   - Chaining (effects jump between targets)
   - Requires: [Progression Gate - To Be Defined]

3. **Extreme Tunability**
   - Custom intensity beyond normal range (0.5x to 3x)
   - Permanent duration modifications
   - Custom targeting filters
   - Requires: [Progression Gate - To Be Defined]

**Progression Gate Placeholders:**
- Miracle mastery (cast baseline miracle X times)
- Faith thresholds (reach Y faith with followers)
- Discovery achievements (find Z locations/knowledge)
- Alignment milestones (achieve specific alignment state)
- [To be fully conceptualized in progression system design]

#### Crafting Cost & Cooldown

**Mana Cost Formula:**
```
BaselineCost = Baseline miracle's standard cost (from miracle definition)

ModificationCost = 0 (baseline)
+ IntensityModifier = (IntensityMultiplier - 1.0) × BaselineCost × 0.5
+ DurationModifier = (DurationMultiplier - 1.0) × BaselineCost × 0.3
+ AreaModifier = (AreaMultiplier - 1.0) × BaselineCost × 0.4
+ TargetingModifier = (if changed) BaselineCost × 0.1
+ AddonCost = Sum of addon costs (if unlocked) × BaselineCost × 0.2 each

TotalCost = BaselineCost + ModificationCost

Example: Baseline Fire Miracle (600 prayer cost)
- High Intensity (1.5x): (1.5 - 1.0) × 600 × 0.5 = 150
- Extended Duration (+100%): (2.0 - 1.0) × 600 × 0.3 = 180
- Expanded Area (+50%): (1.5 - 1.0) × 600 × 0.4 = 120
- Total = 600 + 150 + 180 + 120 = 1,050 prayer cost
```

**Crafting Cooldown:**
```
BaseCooldown = 300 seconds (5 minutes)
ModificationMultiplier = 1.0 + (ModificationCost / BaselineCost) × 0.5
Cooldown = BaseCooldown × ModificationMultiplier
MaxCooldown = 1800 seconds (30 minutes)
```

**Example:** Crafting the modified Fire miracle (450 modification cost over 600 baseline):
- ModificationMultiplier = 1.0 + (450/600) × 0.5 = 1.375
- Cooldown = 300 × 1.375 = 412 seconds (~7 minutes)

#### Miracle Storage

- **Baseline Miracles:** Always accessible via hotkeys/menu (no storage needed)
- **Crafted Variants:** Stored in player's **Miracle Variant Library**
- Each variant references its baseline miracle + modification parameters
- Variants have unique IDs (e.g., "Fire_HighIntensity_Extended", "Heal_Focused_EnemyOnly")
- Can be edited/reforged (costs mana, may have restrictions)
- Can be assigned to multiple pedestals (one variant blueprint, many instances)
- Variants can also be assigned to hotkeys/menu for direct casting (replaces or supplements baseline)

---

### Miracle Pedestal Crafting (Villagers)

#### Crafting Requirements

**Crafters:**
- **Priests/Shamans:** Highest authenticity (divine connection)
- **Mages/Enchanters:** Good authenticity, can add bonuses
- **Master Craftsmen:** Good quality, reliable cooldown rates
- **Apprentices:** Lower quality, higher risk of penalties

**Materials:**
- **Divine Materials:** Blessed stone, consecrated wood, divine metal (highest quality)
- **Rare Materials:** Rare gems, enchanted materials, blessed materials (high quality)
- **Standard Materials:** Stone, wood, metal (moderate quality)
- **Poor Materials:** Scrap, broken materials (low quality, high risk)

**Quality Formula:**
```
CrafterSkill = Entity's relevant skill (Crafting, Enchanting, or Divinity)
SkillContribution = (CrafterSkill / 100) × 0.6  // 60% from skill

MaterialQuality = Material's quality rating (0-100)
MaterialContribution = (MaterialQuality / 100) × 0.4  // 40% from materials

FinalQuality = (SkillContribution + MaterialContribution) × 100
Capped at 100 (perfect quality)
```

**Example:**
- Priest with 80 Divinity skill
- Blessed stone (90 quality)
- Quality = ((80/100 × 0.6) + (90/100 × 0.4)) × 100 = (0.48 + 0.36) × 100 = 84 quality

#### Pedestal Properties (Quality-Based)

**1. Cooldown Rate (Mana Accumulation)**

Pedestals accumulate mana over time to cast their assigned miracle. Quality affects accumulation rate:

```
MiracleManaCost = Original miracle's mana cost
BaseAccumulationRate = MiracleManaCost / BaseCooldownTime
QualityModifier = 0.5 + (Quality / 200)  // 0.5x to 1.0x

EffectiveAccumulationRate = BaseAccumulationRate × QualityModifier
EffectiveCooldown = MiracleManaCost / EffectiveAccumulationRate
```

**Example:**
- Miracle costs 5000 mana, base cooldown 60 seconds
- Base accumulation = 5000/60 = 83.3 mana/second
- 84 quality pedestal: 0.5 + (84/200) = 0.92x multiplier
- Effective accumulation = 83.3 × 0.92 = 76.7 mana/second
- Effective cooldown = 5000/76.7 = 65 seconds (5 seconds slower than perfect)

**Low Quality Penalty:**
- Quality < 30: Accumulation rate dramatically reduced (can take 5-10x longer)
- Quality < 50: Moderate reduction (2-3x longer)

**High Quality Bonus:**
- Quality > 90: Slight bonus (10-20% faster accumulation)
- Quality = 100: Perfect efficiency (matches base cooldown exactly)

**2. Authenticity (Effect Fidelity)**

Quality determines how closely the pedestal's miracle matches the original:

```
Authenticity = Quality (0-100%)
EffectStrength = OriginalEffect × Authenticity

Example: 84 quality = 84% authenticity
Original 100 damage → Pedestal deals 84 damage
Original 100% heal → Pedestal heals 84%
```

**Quality Thresholds:**
- **Perfect (100):** 100% authentic, no deviation
- **Excellent (90-99):** 95-99% authentic, minor visual differences
- **Good (70-89):** 85-95% authentic, noticeable but acceptable
- **Fair (50-69):** 70-85% authentic, significant reduction in effectiveness
- **Poor (30-49):** 50-70% authentic, unreliable results
- **Broken (<30):** <50% authentic, may fail completely or have dangerous side effects

**3. Bonuses & Penalties**

Quality can add modifiers or introduce imperfections:

**High Quality Bonuses (Quality ≥ 80):**
- **+10% Effect Duration** (excellent materials hold charge better)
- **+5% Area of Effect** (better focusing)
- **-10% Mana Cost** (more efficient)
- **Enhanced Visual Effects** (more impressive, increases faith generation)

**Medium Quality (Quality 50-79):**
- No bonuses, no penalties (standard operation)

**Low Quality Penalties (Quality < 50):**
- **-10% Effect Duration** (materials degrade faster)
- **-5% Area of Effect** (poor focusing)
- **+10% Mana Cost** (inefficiency)
- **Reduced Visual Effects** (less impressive)
- **Chance of Failure:** (50 - Quality)% chance per cast (e.g., 30 quality = 20% failure chance)

**Critical Failure (< 30 Quality):**
- **50%+ Failure Rate**
- **Side Effects:** May damage nearby entities, cause explosions, spawn unintended effects
- **Dangerous:** Villagers may refuse to use or may damage the pedestal to stop it

---

## System Interactions

### Integration with Crafting System

**Crafting Process:**
1. Entity (priest/mage/craftsman) selects miracle blueprint from library
2. Entity gathers materials (or uses provided materials)
3. Entity crafts pedestal over time (crafting duration based on skill + complexity)
4. Quality calculated (skill + materials)
5. Pedestal assigned miracle blueprint
6. Pedestal begins accumulating mana

**Crafting Time:**
```
BaseCraftTime = 300 seconds (5 minutes)
SkillModifier = 1.0 - (CrafterSkill / 200)  // Max 50% reduction
MaterialModifier = 0.9 (divine) to 1.2 (poor)

CraftTime = BaseCraftTime × SkillModifier × MaterialModifier

Example: 80 skill priest, divine materials
= 300 × (1.0 - 80/200) × 0.9 = 300 × 0.6 × 0.9 = 162 seconds (~3 minutes)
```

### Integration with Miracle System

**Baseline vs Variant Casting:**
- **Baseline Miracles:** Player casts directly via hotkeys/menu, uses baseline parameters
- **Crafted Variants:** Player can cast variants directly (if assigned to hotkeys) or via pedestals
- Variants cost more than baseline (due to modifications) but offer customization

**Pedestal Casting:**
- Pedestals can be assigned baseline miracles OR crafted variants
- Pedestals accumulate mana/prayer over time
- When accumulation reaches miracle cost → automatically casts (or player can trigger manually)
- Cast uses pedestal's authenticity-modified effects
- Cooldown resets, accumulation begins again

**Multiple Pedestals:**
- Same miracle (baseline or variant) can be assigned to multiple pedestals
- Each pedestal operates independently (own cooldown, own quality)
- Strategic placement creates miracle networks (defense grids, healing stations, etc.)
- Can mix baseline and variant pedestals (baseline for reliability, variants for optimization)

### Integration with Economy

**Material Economy:**
- Divine materials are rare/expensive
- Creates demand for blessed/consecrated materials
- Priests/mages become valuable for material preparation

**Pedestal Maintenance:**
- Low quality pedestals degrade over time (may need repairs)
- High quality pedestals are durable investments
- Maintenance costs create ongoing economic considerations

---

## Key Parameters (All Tunable)

| Parameter | Default Value | Reasoning | Tunable |
|-----------|--------------|-----------|---------|
| Base Crafting Cost | 1000 mana | Significant investment, not trivial | ✅ Yes (500-5000) |
| Base Crafting Cooldown | 300 seconds | Prevents spam, encourages planning | ✅ Yes (60-1800 seconds) |
| Skill Contribution | 60% | Emphasizes skill over materials | ✅ Yes (40-80%) |
| Material Contribution | 40% | Materials still matter significantly | ✅ Yes (20-60%) |
| Base Accumulation Rate | Cost/60s | Standard cooldown baseline | ✅ Yes (Cost/30s to Cost/120s) |
| Quality Cooldown Range | 0.5x to 1.0x | Significant quality impact | ✅ Yes (0.3x to 1.2x) |
| Authenticity Scaling | 1:1 with quality | Direct quality→effect relationship | ✅ Yes (0.8:1 to 1.2:1) |
| Failure Chance Threshold | Quality < 50 | Low quality has risks | ✅ Yes (Quality < 30 to < 70) |
| Critical Failure Threshold | Quality < 30 | Very low quality is dangerous | ✅ Yes (Quality < 20 to < 40) |

---

## Examples

### Example 1: Player Modifies Baseline Heal Miracle

**Setup:**
- Player knows baseline Heal Miracle (500 prayer, 10m radius, 120 heal/tick, 30s channel)
- Wants to create variant for battlefield use

**Modifications Selected:**
- Intensity: High (1.5x heal amount)
- Duration: Extended (+100% duration, 60s channel)
- Area: Expanded (+50% radius, 15m radius)
- Targeting: Friendly entities only (if progression unlocked)

**Cost Calculation:**
- Baseline: 500 prayer
- Intensity: (1.5 - 1.0) × 500 × 0.5 = 125
- Duration: (2.0 - 1.0) × 500 × 0.3 = 150
- Area: (1.5 - 1.0) × 500 × 0.4 = 100
- Targeting: (if unlocked) 500 × 0.1 = 50
- Total: 500 + 125 + 150 + 100 + 50 = 925 prayer

**Cooldown:** 
- ModificationCost = 425
- Multiplier = 1.0 + (425/500) × 0.5 = 1.425
- Cooldown = 300 × 1.425 = 427 seconds (~7 minutes)

**Result:** "Heal_Extended_Battlefield" variant stored in library:
- 180 heal/tick (1.5x baseline)
- 60s duration (2x baseline)
- 15m radius (1.5x baseline)
- Friendly-only targeting
- Ready to assign to pedestals or use directly

### Example 2: Priest Crafts High-Quality Pedestal

**Setup:**
- Priest (90 Divinity skill)
- Blessed stone (95 material quality)
- Assigns modified Heal miracle variant (925 prayer cost from Example 1)

**Quality Calculation:**
- Skill: (90/100) × 0.6 = 0.54
- Material: (95/100) × 0.4 = 0.38
- Quality = (0.54 + 0.38) × 100 = 92 quality

**Pedestal Properties:**
- **Cooldown:** Baseline miracle cooldown 30s, but pedestal accumulation rate calculated
  - Base accumulation = 925/30 = 30.8 prayer/sec
  - Quality modifier 0.96x → Effective accumulation 29.6 prayer/sec
  - Effective cooldown = 925/29.6 = 31.3 seconds (4% slower than perfect)
- **Authenticity:** 92% → Healing effect is 92% of variant (166 heal/tick instead of 180)
- **Bonuses:** +10% duration (66s instead of 60s), +5% area (15.75m instead of 15m), -10% mana cost (832 prayer instead of 925)
- **Result:** Reliable, efficient healing station with excellent quality

### Example 3: Apprentice Crafts Low-Quality Pedestal

**Setup:**
- Apprentice (35 Crafting skill)
- Scrap materials (20 material quality)
- Assigns same modified Heal miracle variant (925 prayer cost)

**Quality Calculation:**
- Skill: (35/100) × 0.6 = 0.21
- Material: (20/100) × 0.4 = 0.08
- Quality = (0.21 + 0.08) × 100 = 29 quality (Critical Failure threshold)

**Pedestal Properties:**
- **Cooldown:** Base accumulation 30.8 prayer/sec, quality modifier 0.645x → Effective accumulation 19.9 prayer/sec → Cooldown 46.5 seconds (55% slower than perfect)
- **Authenticity:** 29% → Healing effect is only 29% of variant (52 heal/tick instead of 180, barely better than baseline!)
- **Penalties:** -10% duration (54s instead of 60s), -5% area (14.25m instead of 15m), +10% mana cost (1017 prayer instead of 925), 21% failure chance per cast
- **Critical Failure Risk:** 50%+ failure rate, may damage nearby entities
- **Result:** Unreliable, dangerous pedestal that villagers may avoid or destroy

---

## Edge Cases

### Pedestal Destruction

- **Low Quality Failure:** Pedestal may explode/destroy itself on critical failure
- **Enemy Attack:** Pedestals can be destroyed in combat (durability based on quality)
- **Player Removal:** Player can remove pedestals (materials may be recoverable based on quality)

### Miracle Blueprint Changes

- **Editing Original:** Changing a miracle blueprint affects all pedestals using it
- **Blueprint Deletion:** Deleting a blueprint makes assigned pedestals inactive (can reassign)
- **Version Control:** May want to track blueprint versions to prevent unexpected changes

### Material Scarcity

- **Divine Materials Rare:** Creates strategic choices (use on important pedestals vs spread out)
- **Trade Routes:** Materials become valuable trade goods
- **Material Preparation:** Priests/mages can "bless" materials to improve quality (takes time, costs resources)

---

## Technical Considerations

### Components Needed

// Baseline miracle reference (from miracle system)
public enum BaselineMiracleType : byte
{
    Fire = 0,
    Water = 1,
    Heal = 2,
    Lightning = 3,
    Shield = 4,
    // ... other baseline miracles
}

// Crafted miracle variant
public struct MiracleVariant : IComponentData
{
    public FixedString128Bytes VariantId;        // Unique ID (e.g., "Fire_HighIntensity_Extended")
    public BaselineMiracleType BaselineType;     // Reference to baseline miracle
    public float IntensityMultiplier;            // 0.75 to 2.0 (modification)
    public float DurationMultiplier;             // 0.5 to 3.0 (modification, 1.0 = baseline)
    public float AreaMultiplier;                 // 0.7 to 2.0 (modification, 1.0 = baseline)
    public TargetingFilter TargetingOverride;    // Optional targeting change
    public BlobAssetReference<MiracleAddonData> Addons; // Advanced modifiers (if unlocked)
    public float ModifiedCost;                   // Calculated cost (baseline + modifications)
    public uint CreationTick;
    public bool IsProgressionLocked;             // True if uses locked features
}

// Miracle pedestal
public struct MiraclePedestal : IComponentData
{
    public Entity CrafterEntity;                 // Who crafted it
    public bool UsesVariant;                     // True if variant, false if baseline
    public BaselineMiracleType BaselineType;     // Baseline miracle (always set)
    public FixedString128Bytes VariantId;        // Variant ID (only if UsesVariant = true)
    public byte Quality;                         // 0-100
    public float CurrentMana;                    // Accumulated mana/prayer
    public float AccumulationRate;               // Mana/prayer per second
    public float Authenticity;                   // 0.0 to 1.0 (quality/100)
    public uint LastCastTick;
    public uint CooldownEndTick;
    public bool IsActive;
    public byte FailureCount;                    // Track failures for low quality
}

// Pedestal modifiers (from quality)
public struct PedestalModifiers : IComponentData
{
    public float DurationMultiplier;             // 0.9 to 1.1
    public float AreaMultiplier;                 // 0.95 to 1.05
    public float ManaCostMultiplier;             // 0.9 to 1.1
    public float FailureChance;                  // 0.0 to 0.5
    public bool HasCriticalFailureRisk;          // Quality < 30
}
```

### Systems Needed

1. **BaselineMiracleSystem** - Handles baseline miracle casting (hotkeys, menu, direct casting)
2. **MiracleCraftingSystem** - Handles player crafting UI, variant creation, progression gate checks
3. **MiracleProgressionSystem** - Tracks miracle unlocks, progression gates, addon availability
4. **PedestalCraftingSystem** - Villagers craft pedestals, calculates quality
5. **PedestalAccumulationSystem** - Pedestals accumulate mana/prayer over time
6. **PedestalCastingSystem** - Pedestals cast miracles (baseline or variant) when ready
7. **PedestalQualitySystem** - Calculates and applies quality-based modifiers
8. **PedestalFailureSystem** - Handles failure chances and critical failures

---

## Balance Considerations

### Baseline vs Crafted Balance

**Risk:** Crafted variants too powerful compared to baseline (making baseline obsolete)
**Mitigation:**
- Modifications add cost (variants always more expensive)
- Cooldown scales with modifications (variants have longer cooldowns)
- Baseline miracles remain viable for quick/cheap casts
- Strategic choice: baseline for efficiency vs variants for optimization

### Crafting Cost Scaling

**Risk:** Players craft overpowered variants (high intensity + large area + extended duration)
**Mitigation:**
- Multiplicative cost scaling (modifications compound)
- Cooldown scales with modification cost
- Progression gates lock extreme modifications until appropriate progression

### Quality Impact

**Risk:** Quality differences too extreme (low quality unusable, high quality trivial)
**Mitigation:**
- Smooth quality curves (no hard cutoffs except critical failure)
- Medium quality (50-79) is viable, just less efficient
- High quality (80+) provides bonuses, not requirements

### Material Economy

**Risk:** Divine materials too rare (can't craft good pedestals) or too common (trivial)
**Mitigation:**
- Multiple quality tiers (divine, rare, standard, poor)
- Materials can be prepared/blessed by skilled entities
- Creates strategic choices (where to invest rare materials)

---

## Future Enhancements

### Progression System Integration

- **Define Progression Gates:** Fully conceptualize what unlocks advanced modifications
  - Miracle mastery milestones (cast X times)
  - Faith/alignment thresholds
  - Discovery achievements
  - Story/quest completion
  - Research/knowledge unlocks

### Advanced Crafting

- **Variant Fusion:** Combine two variants into one hybrid (very expensive, requires progression)
- **Variant Templates:** Save common modification sets as templates for quick crafting
- **Miracle Research:** Unlock new baseline miracles through research/exploration (expands crafting options)

### Pedestal Networks

- **Linked Pedestals:** Pedestals can share mana or trigger chains
- **Pedestal Upgrades:** Improve existing pedestals (replace materials, enhance quality)
- **Pedestal Automation:** Set conditions for automatic casting (defense triggers, healing thresholds)

### Villager Interaction

- **Pedestal Worship:** Villagers may pray at pedestals (generates extra prayer power)
- **Pedestal Maintenance:** Villagers maintain pedestals (prevents quality degradation)
- **Pedestal Festivals:** High-quality pedestals become focal points for celebrations

---

## Open Questions

1. **Which baseline miracles are available at start?** (Need progression/starting setup definition)
2. **How are new baseline miracles unlocked?** (Progression system to be defined)
3. **What are the specific progression gates for addons/extreme modifications?** (To be conceptualized)
4. **Can other god entities craft variants?** (AI gods, player-controlled allies)
5. **Can pedestals be moved after crafting?** (Or fixed location only)
6. **Do pedestals require maintenance/repairs over time?** (Quality degradation)
7. **Can players "tune" existing pedestals?** (Improve quality, change assigned miracle/variant)
8. **How do pedestals interact with alignment?** (Evil gods may corrupt pedestals)
9. **Can players cast variants directly via hotkeys?** (Or only via pedestals)

---

## Related Concepts

- **Miracle System:** `Docs/Concepts/Miracles/Miracle_System_Vision.md`
- **Prayer Power:** `Docs/Concepts/Implemented/Core/Prayer_Power.md`
- **Crafting Systems:** (To be linked when crafting system documented)
- **Material Quality:** (To be linked when material system documented)

---

**For Implementers:** 
- Start with baseline miracle system (hotkeys, menu, direct casting)
- Implement basic crafting (intensity, duration, area modifications for known miracles)
- Add progression gate system (placeholder checks, full gates to be defined)
- Simple quality calculation (skill + materials) for pedestals
- Basic pedestal accumulation/casting
- Add bonuses/penalties, failure systems, and advanced modifiers in v2.0

**For Designers:** 
- Define baseline miracle roster and unlock progression
- Conceptualize progression gates for addons and extreme modifications
- Balance modification costs (variants should be better but more expensive than baseline)
- Balance quality curves (smooth transitions, meaningful differences)
- Define material rarity and availability (strategic choices without frustration)

