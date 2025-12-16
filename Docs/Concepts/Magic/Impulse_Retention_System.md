# Impulse Retention System - Godgame

## Overview

The **Impulse Retention System** enables entities under stasis effects to accumulate forces, momentum, and magical impulses while frozen. When stasis ends, all accumulated impulses are released simultaneously or gradually, creating devastating kinetic discharges or redirected attacks. Different stasis types (time freeze, invulnerability, temporal lock) have different accumulation and release behaviors.

---

## Core Concepts

### 1. Stasis Types

**Time Stasis (Complete Freeze):**
- Entity frozen in time, cannot move or act
- Accumulates: ALL impulses (kinetic, magical, gravitational, environmental)
- Decay Rate: 0% (no impulse loss over time)
- Release: Instantaneous (all impulses applied in single moment)
- Visual: Blue-white shimmer, entity appears frozen mid-motion

**Invulnerability Stasis (Damage Immunity):**
- Entity cannot be damaged but can perceive surroundings
- Accumulates: Kinetic impulses only (damage converted to force)
- Decay Rate: 5% per second (impulses gradually dissipate)
- Release: Gradual (released over 2-5 seconds, reduces shock)
- Visual: Golden aura, entity appears solid and immovable

**Temporal Lock (Perfect Freeze):**
- Entity frozen with perfect temporal anchor
- Accumulates: NONE (all impulses reflected back at source)
- Decay Rate: N/A (no accumulation)
- Release: N/A (no stored impulses to release)
- Visual: Silver crystalline appearance, mirror-like surface
- Use Case: Perfect defense, attackers damage themselves

**Selective Stasis (Partial Freeze):**
- Entity frozen but certain forces can affect it
- Accumulates: Configurable (e.g., only magical forces, or only kinetic)
- Decay Rate: 2% per second
- Release: Instantaneous (only accumulated type released)
- Visual: Varies by configuration (purple = magic only, gray = kinetic only)

**Momentum Bank (Gravity Accumulation):**
- Entity frozen but gravity impulses accumulate
- Accumulates: Gravitational potential energy only
- Decay Rate: 0% (gravity is constant)
- Release: Amplified 2× (falling from height releases doubled momentum)
- Visual: Faint green shimmer, entity appears weightless
- Use Case: Freeze falling boulder, release for devastating impact

---

## Impulse Sources

### Physical Impulses

**Weapon Strikes:**
```
Impulse = (Damage × Weapon Mass × Velocity) / 1000

Example:
- Warhammer strike: 120 damage, 5 kg mass, 15 m/s velocity
- Impulse = (120 × 5 × 15) / 1000 = 9 impulse units

Accumulated impulses: Vector addition
- Strike 1 from north: 9 units north
- Strike 2 from east: 12 units east
- Total: √(9² + 12²) = 15 units at 53° northeast
```

**Projectiles:**
```
Impulse = (Projectile Mass × Velocity²) / 500

Example:
- Arrow: 0.05 kg, 60 m/s
- Impulse = (0.05 × 60²) / 500 = 0.36 impulse units (weak individual)
- 100 arrows: 36 impulse units (significant when accumulated)
```

**Falling Objects:**
```
Gravitational Impulse = Mass × (Gravity × Time in Stasis)

Example:
- 500 kg boulder frozen mid-fall for 30 seconds
- Impulse = 500 × (10 m/s² × 30s) = 150,000 impulse units
- Release: Boulder impacts at terminal velocity equivalent
```

### Magical Impulses

**Force Spells:**
```
Impulse = (Spell Damage × Spell Level × Caster Intelligence) / 200

Example:
- Force Push (Level 3): 80 damage, caster INT 140
- Impulse = (80 × 3 × 140) / 200 = 168 impulse units
```

**Elemental Attacks:**
```
Kinetic Component = Damage × Element Multiplier

Element Multipliers:
- Fire: 0.3 (mostly heat, little kinetic force)
- Lightning: 0.5 (electrical + kinetic shock)
- Ice: 0.7 (solid projectile)
- Force: 1.5 (pure kinetic)
- Earth: 1.2 (heavy solid matter)

Example:
- Fireball: 300 damage × 0.3 = 90 impulse
- Earth Spike: 300 damage × 1.2 = 360 impulse
```

### Environmental Impulses

**Wind Pressure:**
```
Wind Impulse = Wind Speed² × Entity Surface Area × Time

Example:
- Hurricane winds (50 m/s) on human (1.8 m² surface) for 10 seconds
- Impulse = 50² × 1.8 × 10 = 45,000 impulse units
```

**Explosions:**
```
Blast Impulse = (Explosion Damage × Blast Radius) / Distance

Example:
- 500 damage explosion, 10m radius, target at 5m
- Impulse = (500 × 10) / 5 = 1,000 impulse units
```

---

## Accumulation Mechanics

### Vector Addition

Multiple forces combine via vector addition:

```
Total Impulse Vector = Σ(Individual Impulse Vectors)

Example: Knight frozen by Time Stasis, struck by 4 attackers

Attack 1 (North): 80 units
Attack 2 (East): 60 units
Attack 3 (South): 40 units
Attack 4 (West): 30 units

Net North-South: 80 - 40 = 40 units north
Net East-West: 60 - 30 = 30 units east

Total Magnitude: √(40² + 30²) = 50 units
Direction: 37° northeast

Upon release: Knight flies 50 impulse units northeast
```

### Damage-to-Impulse Conversion

Stasis prevents damage but converts it to impulse:

```
Impulse Gained = Damage Prevented × Damage-to-Impulse Ratio

Damage-to-Impulse Ratios:
- Time Stasis: 2.0 (all damage becomes double impulse)
- Invulnerability: 1.5 (75% damage becomes 112% impulse)
- Selective Stasis: 1.0 (damage = impulse)

Example: Mage in Invulnerability Stasis hit by 800 damage dragon claw
- Damage prevented: 800
- Impulse gained: 800 × 1.5 = 1,200 impulse units
- Direction: Toward mage (dragon's attack direction)
```

### Mass Consideration

Entity mass affects final velocity when impulses release:

```
Release Velocity = Total Impulse / Entity Mass

Example 1: Human (70 kg) with 700 accumulated impulse
- Velocity = 700 / 70 = 10 m/s (moderate speed, survivable)

Example 2: Boulder (5,000 kg) with 700 accumulated impulse
- Velocity = 700 / 5,000 = 0.14 m/s (barely moves)

Example 3: Arrow (0.05 kg) with 700 accumulated impulse
- Velocity = 700 / 0.05 = 14,000 m/s (hypersonic projectile, instant kill)
```

### Decay Over Time

Some stasis types lose accumulated impulses:

```
Remaining Impulse = Initial Impulse × (1 - Decay Rate)^Time

Example: Invulnerability Stasis (5% decay per second)
- Initial: 1,000 impulse units
- After 10 seconds: 1,000 × (1 - 0.05)^10 = 599 impulse units (40% lost)
- After 30 seconds: 1,000 × 0.95^30 = 215 impulse units (78% lost)

Strategic implication: Release stasis quickly to preserve impulses
```

---

## Release Mechanics

### Instantaneous Release (Time Stasis)

All impulses applied in single moment:

```
Scenario: Enemy warrior frozen mid-charge
- 10 archers shoot frozen warrior (10 arrows × 36 impulse = 360 units)
- 3 mages cast Force Push (3 × 168 impulse = 504 units)
- Total accumulated: 864 impulse units (direction: backward, away from attackers)

Release:
- All 864 impulse applied in 1 frame (0.016 seconds at 60 FPS)
- Warrior mass: 90 kg
- Velocity: 864 / 90 = 9.6 m/s (21 mph)
- Effect: Warrior launches backward, crashes into wall 15m away
- Impact damage: 9.6² × 90 / 200 = 41 damage (wall collision)
```

### Gradual Release (Invulnerability Stasis)

Impulses released over time to reduce shock:

```
Release Rate = Total Impulse / Release Duration

Scenario: Paladin with 1,200 accumulated impulse, 3-second release
- Release rate: 1,200 / 3 = 400 impulse per second
- Paladin mass: 100 kg

Second 1: 400 impulse → velocity 4 m/s
Second 2: +400 impulse → velocity 8 m/s
Second 3: +400 impulse → velocity 12 m/s (final)

Effect: Paladin accelerates smoothly over 3 seconds instead of instant jolt
- Reduces risk of self-damage
- Allows mid-release course correction (can try to land safely)
- More survivable for entity under stasis
```

### Amplified Release (Momentum Bank)

Stasis amplifies accumulated forces:

```
Released Impulse = Accumulated Impulse × Amplification Factor

Amplification Factors:
- Time Stasis: 1.0× (no amplification)
- Invulnerability: 1.0× (no amplification)
- Momentum Bank: 2.0× (gravity doubles)
- Overcharged Stasis: 3.0× (forbidden magic, dangerous)

Scenario: 1,000 kg boulder frozen mid-fall for 20 seconds
- Gravitational impulse: 1,000 × (10 × 20) = 200,000 units
- Momentum Bank amplification: 200,000 × 2.0 = 400,000 units
- Release velocity: 400,000 / 1,000 = 400 m/s (supersonic)
- Impact: Creates 50m diameter crater, 5,000+ damage to target
```

### Directional Release

Caster can redirect accumulated impulses:

```
Redirection Cost: 50 mana per 45° angle change

Scenario: Arrow frozen by mage, struck by 5 more arrows (accumulated 216 impulse units)
- Original direction: Toward mage (dangerous)
- Mage spends 100 mana to redirect 180° (back at attackers)
- Released impulse: 216 units toward enemies
- Arrow mass: 0.05 kg
- Velocity: 216 / 0.05 = 4,320 m/s (hypersonic arrow)
- Result: Arrow penetrates 3 enemies (instant kills)
```

---

## Gameplay Examples

### Example 1: Frozen Arrow Barrage

**Setup:**
- Enemy archer fires arrow at mage
- Mage casts Time Stasis on arrow mid-flight
- Allied archers shoot the frozen arrow (5 arrows hit it)

**Accumulation:**
- Original arrow impulse: 36 units (toward mage)
- 5 allied arrows: 5 × 36 = 180 units (same direction)
- Total: 216 units

**Release:**
- Mage releases stasis
- Arrow now has 216 impulse (6× original)
- Original arrow velocity: 60 m/s
- New velocity: 60 × 6 = 360 m/s (6× faster)
- Arrow pierces enemy archer's armor (200 damage instead of 35)

**Tactical Value:**
- Turns enemy attack into super-weapon
- Low mana cost (25 mana for 1-second stasis)
- Requires coordination (allies must hit frozen arrow)

---

### Example 2: Boulder Drop Amplification

**Setup:**
- 2,000 kg boulder pushed off cliff (100m height)
- Allied mage casts Momentum Bank stasis on boulder mid-fall
- Boulder frozen at 50m altitude for 30 seconds

**Accumulation:**
- Gravitational impulse: 2,000 × (10 × 30) = 600,000 units
- Amplification: 600,000 × 2.0 = 1,200,000 units

**Release:**
- Mage releases stasis over enemy siege camp
- Boulder velocity: 1,200,000 / 2,000 = 600 m/s (Mach 1.8)
- Impact: Creates 40m crater
- Siege camp: 12 trebuchets destroyed, 450 soldiers killed
- Shockwave: Additional 200 casualties within 100m

**Strategic Value:**
- Turns simple boulder into tactical nuke
- Gravitational accumulation is free (no mana cost)
- Long stasis duration = more accumulated impulse

---

### Example 3: Invulnerability Tank

**Setup:**
- Paladin charges enemy lines
- Paladin activates Invulnerability Stasis (lasts 10 seconds)
- 30 enemy soldiers strike paladin (total 3,600 damage prevented)

**Accumulation:**
- Damage-to-impulse conversion: 3,600 × 1.5 = 5,400 impulse units
- Direction: Toward paladin (enemies attacking from all sides, vectors cancel)
- Net impulse: ~200 units forward (slight imbalance favors forward direction)
- Decay (5% per second, 10 seconds): 5,400 × 0.95^10 = 3,234 remaining

**Release (Gradual, 5 seconds):**
- Paladin mass: 120 kg (heavy armor)
- Final velocity: 3,234 / 120 = 27 m/s (60 mph)
- Acceleration: 27 / 5 = 5.4 m/s² (smooth acceleration)
- Effect: Paladin becomes battering ram, plows through enemy ranks
- Impact: Knocks down 10+ enemies, deals 40-80 damage each

**Tactical Value:**
- Converts enemy attacks into offensive momentum
- Survivable gradual release (no self-damage)
- Disrupts enemy formation

---

### Example 4: Fireball Redirection

**Setup:**
- Enemy mage casts Fireball (400 damage) at player's city
- Player's archmage casts Selective Stasis (magic only) on fireball
- 3 allied fire mages cast additional Fireballs at frozen fireball

**Accumulation:**
- Original fireball: 400 damage × 0.3 (fire multiplier) = 120 impulse
- 3 additional fireballs: 3 × 120 = 360 impulse
- Total: 480 impulse units

**Redirection:**
- Archmage spends 200 mana to redirect 180° (back at enemy)
- Released impulse: 480 units toward enemy mage

**Result:**
- Combined fireball (400 + 360 + 360 + 360 = 1,480 damage equivalent)
- Enemy mage: 1,480 damage (instant death, mages have ~600 HP)
- Blast radius: 15m (kills 5 nearby enemy soldiers)

**Tactical Value:**
- Defensive counter-attack
- Amplifies friendly magic (4× multiplier)
- Punishes enemy mages

---

### Example 5: Temporal Lock Reflection

**Setup:**
- Dragon breathes fire (1,200 damage) at knight
- Knight activates Temporal Lock (perfect freeze, 2 seconds)
- Dragon's fire reflects back at dragon

**Mechanics:**
- Temporal Lock: No accumulation, all impulses reflected
- Dragon's fire: 1,200 damage × 0.3 = 360 impulse
- Reflection: 360 impulse returns to dragon

**Result:**
- Dragon takes 360 reflected impulse
- Dragon mass: 8,000 kg
- Dragon knockback: 360 / 8,000 = 0.045 m/s (minimal, dragon barely flinches)
- BUT: Dragon takes own fire damage (1,200 damage, reduced by dragon's fire resistance 80%)
- Net damage to dragon: 1,200 × 0.2 = 240 damage

**Tactical Value:**
- Perfect defense (no impulse accumulation = no risk)
- Punishes powerful single attacks
- Limited duration (expensive, 100 mana per second)

---

## Stasis Spell Specifications

### Time Stasis

**Mana Cost:** 50 mana per second
**Duration:** 1-30 seconds (caster choice)
**Range:** 20 meters
**Target:** Single entity (up to 500 kg mass, larger entities require more mana)

**Effects:**
- Target frozen in time, cannot act or perceive
- Accumulates all impulses (kinetic, magical, environmental)
- No decay (100% retention)
- Instantaneous release (all impulses applied at once)

**Upgrade Path:**
- Level 1: 1-5 seconds duration, 200 kg max
- Level 2: 1-10 seconds, 500 kg max
- Level 3: 1-20 seconds, 2,000 kg max, can redirect release angle (±45°)
- Level 4: 1-30 seconds, 10,000 kg max, full directional control

---

### Invulnerability Stasis

**Mana Cost:** 30 mana per second
**Duration:** 5-60 seconds
**Range:** Self or touch
**Target:** Self or ally

**Effects:**
- Target immune to damage, can still perceive and think
- Accumulates kinetic impulses (damage converted to force)
- 5% decay per second
- Gradual release (2-5 seconds)

**Upgrade Path:**
- Level 1: 5-10 seconds, 5% decay
- Level 2: 5-30 seconds, 3% decay
- Level 3: 5-60 seconds, 2% decay, can choose release duration
- Level 4: 5-120 seconds, 0% decay (perfect retention)

---

### Momentum Bank

**Mana Cost:** 20 mana to activate, 5 mana per second to maintain
**Duration:** Unlimited (until released or mana depleted)
**Range:** 30 meters
**Target:** Single object or entity

**Effects:**
- Target frozen, only gravitational impulses accumulate
- 0% decay
- 2× amplification on release
- Instantaneous release

**Upgrade Path:**
- Level 1: 2× amplification
- Level 2: 2.5× amplification, can target 2 objects
- Level 3: 3× amplification, can target 5 objects
- Level 4: 4× amplification, can target 10 objects, area effect (10m radius)

---

### Temporal Lock

**Mana Cost:** 100 mana per second (expensive!)
**Duration:** 1-10 seconds
**Range:** Self only
**Target:** Self

**Effects:**
- Perfect temporal freeze, no impulse accumulation
- All incoming forces reflected back at source
- Cannot act while locked (trade-off: perfect defense but no offense)
- No release mechanic (no stored impulses)

**Upgrade Path:**
- Level 1: 1-2 seconds, 100% mana cost
- Level 2: 1-5 seconds, 80% mana cost
- Level 3: 1-10 seconds, 60% mana cost, can reflect to different target (not just source)
- Level 4: 1-20 seconds, 40% mana cost, 10m aura (affects allies within radius)

---

## ECS Components

### StasisEffect

```csharp
public struct StasisEffect : IComponentData
{
    public StasisType Type;              // TimeStasis, Invulnerability, etc.
    public float RemainingDuration;      // Seconds until stasis ends
    public float StartTime;              // When stasis began
    public Entity Caster;                // Who cast the stasis
    public bool IsActive;                // Currently in stasis
    public float ManaCostPerSecond;      // Mana drain for maintained stasis
}

public enum StasisType : byte
{
    TimeStasis,          // Freeze time, all impulses
    Invulnerability,     // Damage immunity, kinetic only
    TemporalLock,        // Perfect freeze, reflection
    SelectiveStasis,     // Configurable accumulation
    MomentumBank         // Gravity only, amplified
}
```

### AccumulatedImpulse

```csharp
public struct AccumulatedImpulse : IComponentData
{
    public float3 TotalImpulseVector;    // Net impulse (N⋅s)
    public float TotalMagnitude;         // Magnitude of impulse
    public float DecayRatePerSecond;     // 0.0 (no decay) to 0.1 (10%/sec)
    public float LastDecayTime;          // Time of last decay application
    public int ImpulseCount;             // Number of impacts accumulated
    public float MaxImpulse;             // Single largest impulse (for tracking)
}
```

### ImpulseSource

```csharp
public struct ImpulseSource : IComponentData
{
    public Entity SourceEntity;          // What caused this impulse
    public float3 ImpulseVector;         // Direction and magnitude
    public ImpulseType Type;             // Kinetic, Magical, Gravitational
    public float Damage;                 // Associated damage (if any)
    public float Timestamp;              // When impulse was applied
}

public enum ImpulseType : byte
{
    Kinetic,         // Physical impacts
    Magical,         // Spell forces
    Gravitational,   // Gravity accumulation
    Environmental,   // Wind, water, explosion
    Divine           // Holy/unholy forces
}
```

### StasisRelease

```csharp
public struct StasisRelease : IComponentData
{
    public ReleaseMode Mode;             // Instantaneous, Gradual, Amplified
    public float ReleaseDuration;        // 0 = instant, 1-10 = gradual
    public float AmplificationFactor;    // 1.0 = normal, 2.0 = doubled
    public float3 RedirectedAngle;       // Euler angles for redirection
    public bool IsRedirected;            // Whether impulse was redirected
    public float RedirectionManaCost;    // Mana spent on redirection
}

public enum ReleaseMode : byte
{
    Instantaneous,   // All at once
    Gradual,         // Over time
    Amplified,       // Multiplied
    Directional      // Redirected
}
```

### StasisCollision

```csharp
public struct StasisCollision : IComponentData
{
    public Entity StasisedEntity;        // Entity in stasis
    public Entity CollidingEntity;       // Entity colliding with stasis
    public float3 CollisionPoint;        // World position of collision
    public float3 CollisionNormal;       // Surface normal
    public float ImpulseMagnitude;       // Force of collision
    public bool ShouldAccumulate;        // Whether to add to accumulated impulse
    public bool ShouldReflect;           // Temporal Lock reflection
}
```

---

## Summary

The **Impulse Retention System** creates emergent physics-based gameplay where:

- **Stasis Types**: Time Stasis (all impulses), Invulnerability (kinetic only), Temporal Lock (reflection), Selective (configurable), Momentum Bank (gravity amplified)
- **Accumulation**: Vector addition of forces, damage-to-impulse conversion, mass consideration, decay over time
- **Release**: Instantaneous (violent), Gradual (smooth), Amplified (multiplied), Directional (redirected)
- **Tactical Applications**: Projectile redirection, boulder drop bombs, invulnerability charges, fireball combination attacks, perfect reflection defense

**Core Formula:**
```
Release Velocity = (Total Accumulated Impulse × Amplification) / Entity Mass

Total Impulse = Σ(Individual Impulses × Damage-to-Impulse Ratio × (1 - Decay)^Time)

Release Damage (if collision) = (Velocity² × Mass) / 200
```

This system integrates with Three Pillar ECS:
- **Body Pillar (60 Hz)**: Collision detection, impulse accumulation, real-time vector addition
- **Mind Pillar (1 Hz)**: Stasis casting decisions, redirection choices, release timing
- **Aggregate Pillar (0.2 Hz)**: Decay accumulation, strategic impulse banking

Impulse retention creates strategic depth where skilled mages can turn enemy attacks into devastating counterattacks through careful timing and positioning.
