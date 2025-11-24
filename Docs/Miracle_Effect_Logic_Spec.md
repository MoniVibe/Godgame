# Miracle Effect Logic Specification

**Status:** Draft  
**Created:** 2025-01-27  
**Purpose:** Define gameplay logic parameters for miracle effects to be authored via Prefab Maker

---

## Overview

This document consolidates the logical effect requirements from the six miracle concept documents (`Fire_Miracle.md`, `Rain_Miracle.md`, `Water_Miracle.md`, `Lightning_Miracle.md`, `Heal_Miracle.md`, `Time_Miracle.md`) and maps them to data structures that can be authored in the Prefab Maker tool.

---

## Activation Modes

All miracles support two activation modes:

1. **Sustained Cast (Channel)**: Hold to maintain effect, drains resources per second
2. **Throw Cast (Projectile)**: Grab orb and throw, explodes on impact

---

## Miracle Effect Taxonomy

### Effect Types

Each miracle can have multiple effect blocks that execute at different phases:

- **Channel Effect**: Active while player holds cast button (sustained mode)
- **Impact Effect**: Triggers on projectile impact (throw mode)
- **Lingering Effect**: Persists after activation ends (area effects, statuses)

### Effect Categories

1. **Damage Effects**: Deal damage to entities/buildings
2. **Healing Effects**: Restore health, cleanse statuses
3. **Status Effects**: Apply buffs/debuffs (burn, shock, fear, haste, slow)
4. **Environmental Effects**: Modify terrain (moisture, fire spread, time dilation)
5. **Knockback Effects**: Apply physics forces
6. **Synergy Effects**: Modify other miracles (water + lightning = electrified puddles)

---

## Per-Miracle Logic Requirements

### Firestorm

**Activation:**
- Sustained: Sweep flame jet, drains cost per second
- Throw: Fire seed explodes on impact, spawns fire pillar

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Damage over time
   - DPS: 50 vs units, 3% durability/sec vs buildings
   - Radius: 12m base, +5m per extra mana
   - Tick interval: 1 second
   - Applies: Burn status

2. **Impact Block** (Throw):
   - Type: Instant damage + fire spawn
   - Damage: 100 instant
   - Radius: 12m
   - Spawns: FirePulse entity

3. **Lingering Block**:
   - Type: Fire propagation
   - Spread chance: 40% per second to adjacent tiles
   - Duration: 20s base
   - Reduced by: Moisture (rain/water miracles)
   - Amplified by: Air miracles

4. **Status Block**:
   - Type: Fear aura
   - Effect: +25% panic chance to enemies
   - Radius: 15m

**Costs:**
- Base: 600 prayer + 30 mana
- Per extra 5m radius: +15 mana
- Sustained cost per second: 50 prayer + 5 mana
- Cooldown: 60s

**System Interactions:**
- Building durability damage
- Generates burn victims for healthcare
- Alignment affects cost (evil discount, benevolent premium)

---

### Rain

**Activation:**
- Sustained: Drag cloud in sky, grows/guides cloud
- Throw: Rain orb spawns cloud on impact

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Cloud growth
   - Moisture per second: 10
   - Max moisture: 100/200/400 (size tier)
   - Drains: Prayer proportional to size

2. **Impact Block** (Throw):
   - Type: Cloud spawn
   - Moisture pool: Based on charge
   - Glide speed: 4 m/s

3. **Lingering Block**:
   - Type: Moisture application
   - Moisture per second: 5
   - Radius: Cloud coverage area
   - Duration: Until moisture depleted or 60s lifetime
   - Effects:
     - Fire suppression: -5% DPS per second (stacks, capped at -90%)
     - Crop growth boost
     - Villager cooling (reduces heatstroke)

**Costs:**
- Base: 500 prayer per Medium cloud
- Size tiers: +50% cost per tier
- Faith density reduces cost up to 50%
- Cooldown: Shared with weather miracles

**System Interactions:**
- Terrain moisture buffers
- Fire damage reduction
- Agriculture growth boost
- Road mudding (excess rain)

---

### Water Burst

**Activation:**
- Sustained: Continuous geyser, sweeps to douse flames
- Throw: Water sphere explodes on impact

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Continuous moisture
   - Moisture per second: 20
   - Radius: 8m
   - Drains: Resources per second

2. **Impact Block** (Throw):
   - Type: Instant moisture burst
   - Radius: 8/14/20m (Small/Medium/Large)
   - Moisture: Instant 50/100/200
   - Knockback: 2/4/6m
   - Cleanses: Burns/poisons up to severity 1.0

3. **Lingering Block**:
   - Type: Flood zone
   - Duration: 10-25s
   - Effects:
     - Mud terrain (slows movement)
     - Puddles (can be electrified by lightning)
     - Basement flooding (if overused)

**Costs:**
- Small: 400 prayer, 20 mana
- Medium: 700 prayer, 35 mana
- Large: 1200 prayer, 60 mana
- Cooldown: 45s

**System Interactions:**
- Immediate fire suppression
- Removes burn/poison statuses
- Road decay acceleration (flooding)
- Villager gratitude (faith boost)

---

### Lightning Lance

**Activation:**
- Sustained: Channel repeated strikes, retarget mid-channel
- Throw: Lightning orb detonates, releases chained bolts

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Repeated strikes
   - Strike interval: 2s
   - Cost per strike: 450 prayer + 25 mana
   - Additional bolts: +10 mana each

2. **Impact Block** (Throw):
   - Type: Chained lightning
   - Primary damage: 300 (ignores 50% armor)
   - Chain range: 10m
   - Chain damage decay: -40% per chain
   - Max chains: 5

3. **Status Block**:
   - Type: Shock
   - Duration: 3s (scales with focus)
   - Effects: Stun, disarm

4. **Synergy Block**:
   - Type: Electrify water
   - Triggers on: Water puddles (from rain/water miracles)
   - Effect: Temporary hazard zone
   - Duration: 10s

**Costs:**
- Per strike: 450 prayer + 25 mana
- Channel additional: +10 mana per bolt
- Cooldown: 20s per bolt

**System Interactions:**
- Higher chain chance during storms (rain synergy)
- Creates burn/shock patients
- Faith boost on critical smites
- Time miracle makes retargeting easier

---

### Divine Heal

**Activation:**
- Sustained: Healing beam/aura, sweeps across targets
- Throw: Healing mote detonates, burst heal

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Continuous healing
   - Heal per tick: 120 (scales with faith density)
   - Tick interval: 1s
   - Radius: 10m
   - Cost per second: 100 prayer + 5 mana
   - Upfront cost: 150 prayer + 10 mana

2. **Impact Block** (Throw):
   - Type: Burst heal
   - Heal amount: 500
   - Radius: 10m
   - Cleanses: Statuses up to severity 2.0

3. **Status Block**:
   - Type: Cleanse
   - Removes: Bleed, poison, burns (up to CleanseStrength)
   - CleanseStrength: 2

4. **Special Block**:
   - Type: Coma reduction
   - Effect: -1 day coma duration
   - Condition: Cast within first day
   - Also: Reduces mana debt

5. **Overheal Block**:
   - Type: Fortified Health buff
   - Triggers: When healing exceeds max health
   - Duration: 30s
   - Alternative: Prayer refund (alignment-dependent)

**Costs:**
- Burst: 500 prayer + 30 mana
- Channel: 150 prayer + 10 mana upfront, 100 prayer + 5 mana/sec
- Faith fields reduce cost up to 50%
- Cooldown: 30s (Burst) / 45s (Channel)

**System Interactions:**
- Reduces clinic load
- Mana debt reduction
- Faith boost on high-profile saves
- Alignment variants (evil = life-drain)

---

### Temporal Veil

**Activation:**
- Sustained: Stretch circular outline, release to snap bubble
- Throw: Time sphere seeds bubble on impact

**Effect Blocks:**
1. **Channel Block** (Sustained):
   - Type: Bubble sizing
   - Radius: 8-18m (scales with charge)
   - Cost scales: Cubically with radius
   - Allows early release to save resources

2. **Impact Block** (Throw):
   - Type: Time bubble spawn
   - Radius: Based on charge
   - Duration: 10-20s (extendable with focus)

3. **Lingering Block** (Haste Mode):
   - Type: Speed multiplier
   - Multiplier: 1.5-2.0x
   - Affects: Movement, focus regen, mana regen, cooldowns
   - Target filter: Allies only

4. **Lingering Block** (Stasis Mode):
   - Type: Speed reduction
   - Multiplier: 0.5-0.25x (or freeze at 0.75+ intensity)
   - Affects: Movement, projectiles, fire burn rate
   - Target filter: Enemies/environmental

5. **Special Block** (Rewind Mode):
   - Type: Time rewind
   - Intensity: 75-100 (slider)
   - Speed: 2x rewind relative to outside
   - Requires: Entity state logging

6. **Risk Block**:
   - Type: Temporal lashback
   - Chance: 15% when mana debt >50%
   - Effect: Stun caster, increase mana debt

**Costs:**
- Base: 900 prayer + 60 mana + 30 focus
- Radius: 12m, 15s duration
- Scales cubically with radius
- Faith density: Minimal discount
- Cooldown: 90s

**System Interactions:**
- Modifies focus/mana regen rates
- Alters combat movement speeds
- Changes miracle cooldowns inside bubble
- Slows detection timers (espionage)

---

## Common Effect Parameters

### Damage Effects
- `damageAmount`: Base damage value
- `damageType`: Physical, Fire, Lightning, etc.
- `armorPenetration`: Percentage ignored (0-100%)
- `tickInterval`: Seconds between damage ticks (0 = instant)
- `duration`: Total effect duration

### Healing Effects
- `healAmount`: Health restored per tick
- `tickInterval`: Seconds between heals
- `maxHeal`: Cap on total healing
- `cleanseStrength`: Maximum status severity removed
- `overhealBehavior`: Buff or refund

### Status Effects
- `statusType`: Burn, Shock, Fear, Haste, Slow, etc.
- `duration`: How long status lasts
- `stacking`: Can it stack? Max stacks?
- `severity`: Intensity level (affects damage/effectiveness)

### Environmental Effects
- `moistureAmount`: Terrain moisture added
- `fireSuppression`: Percentage reduction in fire DPS
- `spreadChance`: Probability of propagation per second
- `speedMultiplier`: Time dilation factor (0.25-2.0)

### Knockback Effects
- `force`: Knockback distance/strength
- `direction`: Radial, away from center, etc.
- `friendlyFire`: Affects allies?

### Synergy Effects
- `triggersOn`: Other miracle types that activate this
- `effect`: What happens when triggered
- `duration`: How long synergy lasts

---

## Target Filtering

All effects need target filtering:

- **Target Type**: Units, Buildings, Terrain, All
- **Friendly Fire**: Affects allies? (boolean)
- **Enemy Only**: Only affects enemies? (boolean)
- **Line of Sight**: Requires LOS? (boolean)
- **Range**: Maximum distance from effect center

---

## Cost Structure

Each miracle has:

- **Base Cost**: Prayer + Mana (one-time for throw, upfront for channel)
- **Sustained Cost**: Prayer + Mana per second (channel mode)
- **Focus Cost**: Optional, for precision/charge
- **Cooldown**: Seconds before can cast again
- **Faith Discount**: Percentage reduction based on faith density (0-50%)

---

## Alignment Modifiers

Effects can be modified by alignment:

- **Cost Modifiers**: Evil alignment discounts offensive, benevolent discounts healing
- **Effect Modifiers**: Evil heal becomes life-drain, benevolent fire costs premium
- **Visual Variants**: Different effect IDs for alignment variants (future)

---

## Combo/Synergy Rules

- **Water + Lightning**: Electrified puddles
- **Rain + Lightning**: Higher chain chance
- **Fire + Air**: Amplified spread speed
- **Fire + Water/Rain**: Instant suppression
- **Time + Any**: Modified cooldowns/costs inside bubble

---

## Data Structure Requirements

For Prefab Maker, each miracle needs:

1. **Activation Metadata**:
   - Supports sustain (boolean)
   - Supports throw (boolean)
   - Base costs (prayer, mana, focus)
   - Sustained costs per second
   - Cooldown

2. **Effect Blocks** (ordered list):
   - Effect type enum
   - Phase (channel/impact/lingering)
   - Parameters (damage, heal, status, etc.)
   - Target filters
   - Duration/tick intervals
   - Radius/area

3. **Synergy Tags**:
   - List of miracle types this synergizes with
   - Synergy effect definitions

4. **Alignment Modifiers**:
   - Cost multipliers per alignment
   - Effect variant IDs (future)

---

## Next Steps

1. Define `MiracleEffectBlock` struct in `DataModels.cs`
2. Extend `MiracleTemplate` with effect blocks list
3. Create editor UI for authoring effect blocks
4. Add validation for logical consistency
5. Export to runtime DTOs for gameplay systems

