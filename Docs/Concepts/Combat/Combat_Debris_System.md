# Combat Debris & Fragmentation System

## Overview

The **Combat Debris System** creates persistent battlefield hazards through realistic fragmentation of damaged war machines, constructs, and fortifications. When **War Golems, Siege Titans, siege engines, or fortifications** sustain damage, they shed physical debris—metal shards, broken gears, stone fragments, wooden splinters, and magical residue—that continues to exist as interactive physics objects.

This debris can **collide with and damage** other entities based on kinetic energy transfer, creating cascading damage effects in dense combat. Debris velocity, mass, and material properties determine impact damage, with high-velocity metal shards acting as improvised projectiles while slower stone fragments create ground hazards.

**Cross-Project Integration:**
- **Godgame**: Medieval/magical debris (stone, metal, wood, arcane fragments)
- **PureDOTS**: Mathematical debris physics framework (see `Debris_Physics_Agnostic.md`)
- **Space4X**: Ship hull fragmentation and component debris (see `Ship_Debris_System.md`)

---

## Core Mechanics

### Debris Generation Triggers

Debris spawns when entities experience specific damage events:

| Trigger Event | Debris Count | Velocity Range | Debris Types |
|---------------|--------------|----------------|--------------|
| **Moderate Damage** (25-50% HP lost) | 5-10 pieces | 2-5 m/s | Small flakes, sparks |
| **Heavy Damage** (50-75% HP lost) | 15-30 pieces | 5-12 m/s | Medium shards, gears, plates |
| **Critical Damage** (75-100% HP lost) | 30-60 pieces | 10-20 m/s | Large fragments, structural pieces |
| **Destruction** (0% HP) | 60-120 pieces | 15-35 m/s | Explosive fragmentation, all materials |
| **Overkill Damage** (damage > remaining HP × 2) | 100-200 pieces | 25-50 m/s | High-velocity shrapnel |

**Damage Threshold Example:**
```
War Golem (1,200 HP) takes 400 damage from trebuchet boulder:
- HP remaining: 800 / 1,200 = 66.7% (33.3% lost)
- Trigger: Moderate Damage (25-50% lost)
- Debris spawned: 8 pieces
- Velocity: 3.5 m/s average (random 2-5 m/s per piece)
- Direction: Radial spread from impact point + momentum transfer from projectile
```

---

### Debris Properties by Material

#### Metal Debris (War Golems, Plate Armor, Weapons)

**Types:**
- **Flakes** (0.01-0.05 kg): Thin metal shavings, sparks
- **Shards** (0.1-0.5 kg): Sharp fragments from armor plating
- **Gears/Components** (0.5-2 kg): Broken mechanical parts
- **Structural Plates** (2-10 kg): Large armor sections

**Damage Calculation:**
```
Impact Damage = (Velocity² × Mass × Material Hardness) / 200

Material Hardness (Metal) = 1.5×

Example: Metal shard (0.3 kg) at 15 m/s
- Damage = (15² × 0.3 × 1.5) / 200
- Damage = (225 × 0.3 × 1.5) / 200
- Damage = 101.25 / 200 = 0.51 damage points per hit
- Against unarmored human (50 HP): ~98 hits to kill
- Against armored soldier (armor 2.0): 0.51 / 2.0 = 0.26 damage per hit
```

**Penetration**: Metal debris can penetrate light armor (leather, cloth) at velocities >8 m/s.

---

#### Stone Debris (Siege Titans, Fortifications, Buildings)

**Types:**
- **Dust** (0.001-0.01 kg): Pulverized stone, minimal damage
- **Chips** (0.05-0.2 kg): Small stone fragments
- **Chunks** (1-5 kg): Fist-sized stones
- **Blocks** (10-50 kg): Large structural pieces

**Damage Calculation:**
```
Impact Damage = (Velocity² × Mass × Material Hardness) / 200

Material Hardness (Stone) = 1.2×

Example: Stone chunk (3 kg) at 10 m/s
- Damage = (10² × 3 × 1.2) / 200
- Damage = (100 × 3 × 1.2) / 200
- Damage = 360 / 200 = 1.8 damage points per hit
```

**Blunt Trauma**: Stone debris deals **blunt damage** rather than piercing. Ignores 50% of armor but deals full damage to shields/constructs.

---

#### Wooden Debris (Siege Engines, Fortifications, Structures)

**Types:**
- **Splinters** (0.01-0.05 kg): Sharp wooden fragments
- **Boards** (0.5-2 kg): Broken planks
- **Beams** (5-20 kg): Structural timber

**Damage Calculation:**
```
Material Hardness (Wood) = 0.8×

Example: Wooden splinter (0.02 kg) at 12 m/s
- Damage = (12² × 0.02 × 0.8) / 200
- Damage = (144 × 0.02 × 0.8) / 200
- Damage = 2.304 / 200 = 0.012 damage points per hit
```

**Low Threat**: Wooden debris generally deals minimal damage unless very large (beams) or high velocity (>20 m/s).

---

#### Magical/Arcane Debris (Enchanted Constructs, Golems)

**Types:**
- **Residue** (0.001-0.01 kg): Glowing arcane dust, causes magical burns
- **Fragments** (0.1-0.5 kg): Crystallized mana shards
- **Core Pieces** (1-5 kg): Broken power sources, unstable

**Damage Calculation:**
```
Material Hardness (Magical) = 2.0× (ignores physical armor)

Example: Arcane fragment (0.2 kg) at 8 m/s
- Physical damage = (8² × 0.2 × 2.0) / 200 = 0.64 damage
- Magical burn = 0.64 × 1.5 = 0.96 damage per second for 3 seconds
- Total damage = 0.64 + (0.96 × 3) = 3.52 damage over time
```

**Special Effect**: Arcane debris **ignores physical armor** and inflicts **burning damage over time** (3 seconds duration).

**Instability**: Large magical core pieces (>2 kg) have 10% chance to **detonate** on impact, dealing 5-15 AoE damage in 2m radius.

---

## Debris Trajectory & Physics

### Initial Velocity Calculation

When debris spawns from a damaged entity, its velocity is determined by:

```
Debris Velocity = Impact Direction × (Impact Force / Entity Mass) + Radial Scatter

Impact Direction: Vector from attacker to target (normalized)
Impact Force: Damage dealt × Force Multiplier (typically 0.5-2.0)
Entity Mass: Total mass of damaged entity
Radial Scatter: Random vector (magnitude 2-10 m/s) perpendicular to impact
```

**Example:**
```
War Golem (800 kg) struck by warhammer (120 damage, 15 m/s swing):
- Impact force: 120 × 1.0 = 120 N (force multiplier for melee = 1.0)
- Impact direction: [0.8, 0.6, 0] (normalized vector from hammer to golem)
- Base velocity: [0.8, 0.6, 0] × (120 / 800) = [0.12, 0.09, 0] m/s
- Radial scatter: Random perpendicular vector [0.3, -0.4, 0.6] × 5 m/s = [1.5, -2.0, 3.0]
- Final debris velocity: [0.12 + 1.5, 0.09 - 2.0, 0.0 + 3.0] = [1.62, -1.91, 3.0] m/s
- Speed: sqrt(1.62² + 1.91² + 3.0²) = 3.94 m/s
```

---

### Gravity & Trajectory

Debris follows **ballistic arcs** under gravity:

```
Vertical Velocity = Initial Vertical Velocity - (Gravity × Time)
Horizontal Velocity = Initial Horizontal Velocity (constant, ignoring air resistance)

Gravity = 9.8 m/s²

Flight time (until ground impact):
t = (V_y + sqrt(V_y² + 2 × g × h)) / g

Where:
- V_y = Initial vertical velocity
- g = 9.8 m/s²
- h = Initial height above ground
```

**Example:**
```
Metal shard spawned at 5m height with velocity [3, 5, 0] m/s (3 m/s horizontal, 5 m/s upward):
- Time to peak: t_peak = 5 / 9.8 = 0.51 seconds
- Max height: h_max = 5 + (5² / (2 × 9.8)) = 5 + 1.28 = 6.28 meters
- Time to ground: t = (5 + sqrt(25 + 2 × 9.8 × 5)) / 9.8 = 1.53 seconds
- Horizontal distance: 3 × 1.53 = 4.59 meters from spawn point
- Impact velocity: sqrt(3² + (5 - 9.8 × 1.53)²) = sqrt(9 + 186.6) = 13.98 m/s
```

**High-Velocity Debris**: Debris with initial velocity >20 m/s may travel 50-100 meters before landing, creating long-range hazards.

---

### Collision Detection & Damage

Debris uses **simplified collision** to reduce performance cost:

**Collision Priority:**
1. **Entities** (soldiers, golems, animals): Full damage calculation
2. **Structures** (walls, buildings): Reduced damage (50%)
3. **Terrain** (ground, rocks): Debris destroyed, no damage dealt
4. **Other Debris**: Ignored (no debris-debris collision)

**Damage on Collision:**
```
Impact Damage = (Debris Velocity² × Debris Mass × Material Hardness) / 200

Applied to target's armor/HP system:
- If target has armor: Damage reduced by armor value
- If damage penetrates: Apply to HP
- If debris velocity < penetration threshold: Deflected (no damage)
```

**Penetration Thresholds by Armor Type:**

| Armor Type | Penetration Velocity (Metal Debris) | Penetration Velocity (Stone Debris) |
|------------|-------------------------------------|-------------------------------------|
| No Armor | 1 m/s | 2 m/s |
| Leather (Armor 1.5) | 5 m/s | 8 m/s |
| Chainmail (Armor 2.0) | 10 m/s | 15 m/s |
| Plate (Armor 3.0) | 18 m/s | 25 m/s |
| Enchanted Plate (Armor 4.0) | 30 m/s | 40 m/s |

---

## Debris Lifetime & Cleanup

### Despawn Conditions

Debris is removed from the simulation when:

1. **Time Limit**: 60 seconds after spawn (default)
2. **Distance Limit**: Travels >200 meters from spawn point
3. **Ground Rest**: Velocity < 0.5 m/s for >5 seconds (settled debris)
4. **Collision**: Hits terrain/structure and velocity < penetration threshold
5. **Entity Limit**: Total debris count exceeds 500 (oldest despawned first)

**Performance Optimization:**
```
If (TotalDebrisCount > 500):
    Sort debris by age (oldest first)
    Despawn oldest 100 debris pieces

If (DebrisVelocity < 0.5 m/s AND TimeAtRest > 5 seconds):
    Convert to static prop (no physics simulation)
    Render as visual-only debris field
```

---

### Visual Debris Fields

After large battles, **static debris fields** remain as visual indicators:

- **Settled Debris**: Converted to low-poly static meshes (no collision)
- **Density**: 50-200 debris pieces per 10m² battlefield area
- **Lifetime**: 300 seconds (5 minutes) after battle ends
- **Fade Out**: Gradual alpha fade over final 30 seconds

**Purpose**: Creates visual atmosphere without performance cost. Players see "scarred" battlefields.

---

## Debris Interaction with Game Systems

### Stasis Field Interaction

**Cross-reference**: See `Impulse_Retention_System.md`

Debris can be **frozen in stasis fields** and accumulate impulses:

**Example:**
```
Scenario: War Golem destroyed near Time Stasis miracle (10m radius)

1. Golem explodes, spawning 80 debris pieces (velocities 15-35 m/s)
2. 30 debris pieces enter stasis field radius (frozen mid-flight)
3. Friendly forces fire arrows through stasis field (accumulate impulses)
4. Stasis expires after 15 seconds
5. Debris releases with accumulated impulses:
   - Original velocity: 25 m/s average
   - Accumulated arrow impacts: 15 arrows × 20 N·s = 300 N·s total
   - Debris mass: 0.3 kg average
   - Additional velocity: 300 / (30 × 0.3) = 33.3 m/s
   - Final velocity: 25 + 33.3 = 58.3 m/s
6. High-velocity debris impacts enemy formation:
   - Impact damage: (58.3² × 0.3 × 1.5) / 200 = 76.4 damage per shard
   - Penetrates plate armor (threshold 18 m/s << 58.3 m/s)
   - 30 shards × 76.4 damage = 2,292 total damage to enemy units
```

**Tactical Application**: Freeze debris from ally destruction, fire on frozen debris to amplify velocity, release toward enemy as improvised shrapnel weapon.

---

### Battlefield Hazards

Debris creates **environmental hazards** that persist after combat:

**1. Caltrops Effect (Small Debris)**
- Metal shards/splinters on ground slow movement by 20%
- Chance to inflict minor damage (0.5-2 HP) per second in debris field
- Lasts 60 seconds after debris settles

**2. Difficult Terrain (Large Debris)**
- Stone blocks/structural pieces create impassable obstacles
- Units must path around or climb over (50% movement penalty)
- Remains until manually cleared or 300 seconds pass

**3. Fire Hazard (Wooden Debris + Flame)**
- Wooden debris ignites if exposed to fire damage
- Burns for 10-30 seconds, spreading to adjacent wood
- Deals 2-5 fire damage per second to units in burning area

---

### Siege Engine Interaction

Debris from destroyed **siege engines** has unique properties:

**Trebuchet Destruction:**
- Spawns 40-60 wooden debris (beams, gears, rope)
- Counterweight (500-1000 kg stone) falls and creates crater
- Arm (50 kg timber) may impact friendly units if mid-swing during destruction

**Ballista Destruction:**
- Spawns 20-30 debris pieces
- **Tension Release**: Loaded bolt fires randomly on destruction (30% chance)
- Bolt velocity: 50-80 m/s (lethal to any unit in path)

**Battering Ram Destruction:**
- Spawns 50-80 heavy wooden debris (beams 10-30 kg)
- Ram head (200 kg iron) falls forward, crushing units beneath
- Creates difficult terrain in 5m radius

---

## Entity-Specific Debris

### War Golem Debris (Standard 3-4m Golem)

**Total Mass**: 800 kg (60% metal, 30% stone, 10% magical)

**Destruction Debris:**
- 40-60 metal shards (0.2-2 kg each, 15-30 m/s)
- 20-30 stone chunks (1-5 kg each, 10-20 m/s)
- 5-10 magical fragments (0.5-2 kg each, 8-15 m/s)
- 1 arcane core (5 kg, 5-10 m/s, 20% detonation chance)

**Debris Pattern:**
```
Radial explosion from golem center:
- Forward cone (120° arc): 60% of debris (direction of killing blow)
- Sides (120° each): 20% of debris per side
- Backward: 0% (blocked by golem mass)

Velocity distribution:
- 30% of debris: 15-20 m/s (high velocity, dangerous)
- 50% of debris: 10-15 m/s (moderate velocity)
- 20% of debris: 5-10 m/s (low velocity, minimal threat)
```

**Damage Potential:**
```
Average debris damage (assuming 25 m/s impact on unarmored soldier):
- Metal shard (0.5 kg): (25² × 0.5 × 1.5) / 200 = 23.4 damage
- Stone chunk (2 kg): (25² × 2 × 1.2) / 200 = 37.5 damage
- Magical fragment (1 kg): (25² × 1 × 2.0) / 200 + burn = 31.25 + 8.64 = 39.9 damage

Total debris damage if all 80 pieces hit enemy formation:
- 50 metal × 23.4 = 1,170 damage
- 25 stone × 37.5 = 937.5 damage
- 5 magical × 39.9 = 199.5 damage
- Total: 2,307 damage distributed across 3-5m radius
- Assuming 10 soldiers in radius: 230 damage per soldier average (460% HP, instant kill)
```

**Tactical Implication**: Destroying enemy golems **in the middle of their formation** creates devastating friendly fire from debris.

---

### Siege Titan Debris (Large 10-15m Titan)

**Total Mass**: 5,000 kg (50% stone, 40% metal, 10% magical)

**Destruction Debris:**
- 80-120 metal plates (2-10 kg each, 20-35 m/s)
- 60-90 stone blocks (5-50 kg each, 15-30 m/s)
- 20-40 magical fragments (1-5 kg each, 10-20 m/s)
- 1 titan core (20 kg, 8-15 m/s, 50% detonation chance for 50 damage AoE)

**Massive Debris**: Siege Titans produce **much larger fragments** due to their size:
```
Stone block (30 kg) at 25 m/s:
- Impact damage = (25² × 30 × 1.2) / 200 = 562.5 damage
- One-shot kill on any infantry (50 HP << 562 damage)
- Heavy damage to War Golems (1,200 HP - 562 = 638 HP remaining)
```

**Debris Range**: Titan destruction debris can travel **100-150 meters** due to high initial velocities and large mass (longer flight time).

**Collateral Damage**: Destroying a Siege Titan in urban environment causes **extensive structural damage** to nearby buildings from large block impacts.

---

## Gameplay Examples

### Example 1: Artillery Duel Debris Cascade

**Setup:**
- Two armies each deploy 3 trebuchets
- Range: 400m apart
- Both sides fire simultaneously

**Combat Sequence:**
1. **Round 1**: Both sides fire, 2 trebuchets per side hit enemy trebuchets
   - 2 trebuchets destroyed per side
   - Each spawns 50 wooden debris pieces (15-25 m/s)
   - Debris travels 30-50m, hits friendly infantry behind trebuchets
   - 100 debris pieces × 1.5 damage average = 150 damage to friendly rear lines

2. **Round 2**: Remaining trebuchets fire
   - Both hit enemy War Golems escorting artillery
   - Golems damaged to critical (75% HP lost)
   - Each spawns 30 debris pieces (10-20 m/s)
   - Debris creates hazard zone in no-man's land

3. **Round 3**: Infantry advance through debris field
   - Movement slowed by 20% (caltrops effect)
   - 5 damage per second from small shards
   - Large metal plates (2-5 kg) create impassable obstacles
   - Advancing force takes 200 additional casualties from debris

**Result**: Debris from destroyed siege equipment inflicts 25% additional casualties beyond direct combat damage.

---

### Example 2: Golem Destruction as Friendly Fire

**Setup:**
- Enemy War Golem (1,200 HP) surrounded by 20 enemy soldiers
- Friendly mages target golem with massive fire blast (1,500 damage, overkill)

**Destruction:**
1. Golem takes 1,500 damage (300 overkill)
2. Overkill triggers high-velocity debris (25-50 m/s range)
3. Debris pattern: 80 pieces in 5m radius explosion
4. Enemy soldiers in 5m radius: All 20 soldiers

**Debris Impact:**
```
Per soldier (average):
- 4 debris pieces hit per soldier (80 / 20)
- Average damage per debris: 35 damage at 35 m/s
- Total damage per soldier: 4 × 35 = 140 damage
- Soldier HP: 50 HP
- Result: Instant kill (140 damage >> 50 HP)

Enemy casualties:
- 1 War Golem destroyed
- 20 soldiers killed by debris (friendly fire from destroying their own golem)
- Total enemy losses: 1,200 HP (golem) + 1,000 HP (soldiers) = 2,200 HP
```

**Tactical Lesson**: Focus fire on enemy constructs **when they're in enemy formations** to maximize debris casualties.

---

### Example 3: Stasis + Debris Combo

**Setup:**
- Friendly mage casts Time Stasis (10m radius) in enemy formation
- Enemy War Golem inside stasis field
- Friendly archers fire 50 arrows into stasis (accumulate impulses on golem)

**Combo Execution:**
1. **Golem Frozen**: All damage accumulated as impulse (no HP loss yet)
2. **Arrows Impact**: 50 arrows × 20 N·s = 1,000 N·s accumulated
3. **Stasis Expires**: Golem receives accumulated impulse
   - Golem mass: 800 kg
   - Velocity gained: 1,000 / 800 = 1.25 m/s (thrown sideways)
   - Damage from impact: 50 arrows worth (500 damage)
   - Golem HP: 1,200 - 500 = 700 HP (heavy damage tier)
4. **Debris Spawns**: Heavy damage (58% HP lost) spawns 25 debris pieces
5. **Debris Also Frozen**: 15 of 25 pieces still inside stasis field radius
6. **Stasis Continues**: Mage extends stasis for 5 more seconds (talent)
7. **Second Volley**: 30 more arrows fired into frozen debris
8. **Debris Releases**:
   - Debris accumulated: 30 arrows × 20 N·s = 600 N·s per piece
   - Debris mass: 0.3 kg average
   - Velocity gained: 600 / 0.3 = 2,000 m/s (hypersonic!)
   - 15 pieces × 2,000 m/s = lethal shrapnel
9. **Enemy Formation**: 15 hypersonic shards impact enemy soldiers
   - Damage per shard: (2,000² × 0.3 × 1.5) / 200 = 9,000 damage per hit
   - Result: 15 instant kills, debris passes through bodies and continues

**Result**: Stasis + debris combo turns 80 arrows into 15 hypersonic kill shots. Total casualties: 1 War Golem destroyed + 15 soldiers killed.

---

## ECS Component Definitions

### Core Components

```csharp
/// <summary>
/// Debris entity component with physics and damage properties
/// </summary>
public struct CombatDebris : IComponentData
{
    public DebrisMaterial Material;
    public float Mass;                       // Kilograms
    public float3 Velocity;                  // m/s
    public float MaterialHardness;           // Damage multiplier (0.8-2.0)
    public float SpawnTimestamp;             // Game time when spawned
    public float DistanceTraveled;           // Meters from spawn point
    public Entity SourceEntity;              // Entity that spawned this debris
    public bool IsSettled;                   // True if velocity < 0.5 m/s for >5s
}

public enum DebrisMaterial : byte
{
    Metal = 0,         // Hardness 1.5×
    Stone = 1,         // Hardness 1.2×
    Wood = 2,          // Hardness 0.8×
    Magical = 3,       // Hardness 2.0× (ignores armor)
    Bone = 4,          // Hardness 1.0×
    Crystal = 5        // Hardness 1.8×
}

/// <summary>
/// Tracks debris generation from damaged entities
/// </summary>
public struct DebrisSource : IComponentData
{
    public DebrisGenerationProfile Profile;
    public float LastDebrisSpawnHP;          // HP when debris last spawned
    public ushort TotalDebrisSpawned;        // Lifetime debris count
    public bool CanGenerateDebris;
}

/// <summary>
/// Configuration for how entity generates debris
/// </summary>
public struct DebrisGenerationProfile
{
    public DebrisMaterial PrimaryMaterial;   // 60% of debris
    public DebrisMaterial SecondaryMaterial; // 30% of debris
    public DebrisMaterial TertiaryMaterial;  // 10% of debris

    public float ModerateDebrisCount;        // 5-10 pieces
    public float HeavyDebrisCount;           // 15-30 pieces
    public float CriticalDebrisCount;        // 30-60 pieces
    public float DestructionDebrisCount;     // 60-120 pieces

    public float MinDebrisVelocity;          // 2 m/s
    public float MaxDebrisVelocity;          // 50 m/s

    public float MinDebrisMass;              // 0.01 kg
    public float MaxDebrisMass;              // 50 kg
}

/// <summary>
/// Magical debris with detonation chance
/// </summary>
public struct MagicalDebris : IComponentData
{
    public float DetonationChance;           // 0-1 probability
    public float DetonationRadius;           // Meters
    public float DetonationDamage;           // AoE damage
    public bool HasDetonated;
}

/// <summary>
/// Debris settled on ground (static visual)
/// </summary>
public struct SettledDebris : IComponentData
{
    public float3 GroundPosition;
    public float SettleTimestamp;
    public float FadeStartTime;              // When to begin alpha fade
}
```

---

### Debris Physics Components

```csharp
/// <summary>
/// Tracks debris trajectory and collision
/// </summary>
public struct DebrisPhysics : IComponentData
{
    public float3 Velocity;
    public float3 AngularVelocity;           // Rotation speed (for visual spinning)
    public float Gravity;                    // 9.8 m/s² (can be modified by magic)
    public float AirDragCoefficient;         // 0.0-1.0 (higher = more drag)
    public bool HasLanded;                   // True if hit ground/structure
}

/// <summary>
/// Debris collision damage calculator
/// </summary>
public struct DebrisCollisionDamage : IComponentData
{
    public float PenetrationThreshold;       // Minimum velocity to penetrate armor
    public float DamageMultiplier;           // Material hardness factor
    public bool CanPenetrateArmor;
    public bool DealsBluntDamage;            // True for stone (ignores 50% armor)
}
```

---

### Battlefield Hazard Components

```csharp
/// <summary>
/// Debris field that slows movement (caltrops effect)
/// </summary>
public struct DebrisHazardField : IComponentData
{
    public float3 CenterPosition;
    public float Radius;                     // Meters
    public float MovementPenalty;            // 0-1 (0.2 = 20% slower)
    public float DamagePerSecond;            // Tick damage to units in field
    public float ExpirationTime;             // Game time when field despawns
}

/// <summary>
/// Large debris obstacle (blocks pathing)
/// </summary>
public struct DebrisObstacle : IComponentData
{
    public float3 Position;
    public float Radius;                     // Meters
    public float Height;                     // Meters (for climbing checks)
    public bool IsPassable;                  // False = blocks movement
    public float ClimbPenalty;               // Movement penalty to climb over (0.5 = 50%)
}

/// <summary>
/// Burning wooden debris (fire hazard)
/// </summary>
public struct BurningDebris : IComponentData
{
    public float FireDamagePerSecond;
    public float BurnDuration;               // Seconds remaining
    public float SpreadRadius;               // Meters (fire spreads to nearby wood)
    public bool CanSpread;
}
```

---

## Performance Optimization

### Debris Limit System

To prevent performance degradation from excessive debris:

```csharp
public struct DebrisLimitConfig : IComponentData
{
    public ushort MaxActiveDebris;           // 500 default
    public ushort MaxDebrisPerEntity;        // 120 per destruction
    public ushort MaxSettledDebris;          // 2,000 static debris (visual only)
    public float CleanupInterval;            // 10 seconds (check for old debris)
}
```

**Cleanup Algorithm:**
```
Every 10 seconds:
1. Count active debris (unsettled, physics-enabled)
2. If count > MaxActiveDebris:
   - Sort debris by age (oldest first)
   - Despawn oldest (count - MaxActiveDebris) pieces
3. Count settled debris (static visuals)
4. If count > MaxSettledDebris:
   - Sort by age
   - Fade out oldest (count - MaxSettledDebris) pieces over 5 seconds
```

---

### LOD System for Debris

**Level of Detail** reduces rendering cost for distant debris:

| Distance from Camera | Debris Detail | Physics Simulation |
|---------------------|---------------|-------------------|
| 0-50m | Full detail (individual pieces) | Full physics |
| 50-100m | Medium detail (merged meshes) | Simplified physics |
| 100-200m | Low detail (impostors) | No physics (visual only) |
| >200m | Not rendered | Despawned |

**Visual Quality**:
- **Full Detail**: Each debris piece rendered individually with PBR materials
- **Medium Detail**: 5-10 debris merged into single mesh, shared material
- **Low Detail**: Billboard sprite representing debris cluster
- **Despawned**: Removed from memory

---

## Integration with Other Systems

### Mech System Integration

**Cross-reference**: See `Mech_System.md`

War Golems and Siege Titans use the **debris generation system** for realistic destruction:

**Module Destruction Debris:**
When a golem **module** is destroyed (before full golem death):
- Spawns 3-8 debris pieces specific to module type
- Weapon modules: Metal shards + gears
- Armor modules: Metal plates + stone chunks
- Arcane modules: Magical fragments + core pieces

**Example:**
```
War Golem with 6 modules takes critical hit to left arm module:
- Left arm module destroyed (200 HP lost)
- Spawns 5 metal shards (0.3-1 kg, 8-15 m/s)
- Debris velocity influenced by golem's current movement (additive)
- If golem moving at 5 m/s forward: Debris velocity = [8-15 m/s] + [5 m/s] = 13-20 m/s
```

---

### Siege Weapon Integration

**Cross-reference**: See `Strategic_Bombardment_System.md`

Siege weapons create **massive debris** when destroyed:

**Trebuchet Destruction:**
- Spawns 60 wooden debris (beams 5-30 kg)
- Counterweight (800 kg stone) falls, creating 3m crater
- If mid-swing: Arm (40 kg) impacts ground/units at 20 m/s (800 damage potential)

**Battering Ram Destruction:**
- Ram head (200 kg iron) falls forward
- Impact damage: (5² × 200 × 1.5) / 200 = 37.5 damage
- But massive mass × low velocity = crushing damage (instant kill any infantry beneath)

---

### Magic System Integration

**Cross-reference**: See magic system documentation

Debris can be manipulated by **telekinesis, wind, and gravity magic**:

**Wind Magic:**
```
Gale spell (10 m/s wind) in debris field:
- All debris < 2 kg affected
- Debris velocity += wind velocity vector
- Light debris (0.1 kg) accelerates to 15-20 m/s
- Creates improvised projectile swarm
```

**Gravity Magic:**
```
Gravity Well spell (2× gravity) in 10m radius:
- All debris falls faster (19.6 m/s² instead of 9.8)
- Flight time reduced by 50%
- Impact velocity increased by 41% (sqrt(2) factor)
```

**Telekinesis:**
```
Mage lifts 10 metal shards (3 kg total) and hurls at enemy:
- Velocity: 25 m/s (mage's telekinesis power)
- Each shard: 0.3 kg × 25 m/s = 23.4 damage on impact
- Targeted attack (100% accuracy vs random debris scatter)
```

---

## Summary

The **Combat Debris & Fragmentation System** adds:

1. **Realistic Battlefield Physics**: Damaged entities shed debris that continues to interact
2. **Cascading Damage**: Debris from destroyed units damages nearby entities (friend or foe)
3. **Environmental Hazards**: Debris fields slow movement, burning debris spreads fire
4. **Tactical Depth**: Destroy enemy constructs in their formations for friendly fire, freeze debris with stasis for improvised weapons
5. **Visual Atmosphere**: Battlefields littered with debris create immersive "war-torn" aesthetic
6. **Performance Management**: LOD system and cleanup algorithms prevent lag from excessive debris

**Cross-Project Consistency:**
- **Godgame**: Metal/stone/wood/magical debris from medieval constructs
- **PureDOTS**: Mathematical debris physics framework (trajectory, collision, damage)
- **Space4X**: Ship hull fragments and component debris (see `Ship_Debris_System.md`)

All debris types use **identical kinetic damage formulas** from PureDOTS framework: `Damage = (V² × M × H) / 200`
