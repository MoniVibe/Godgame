# Shield Durability & Damage Localization System

## Overview

The **Shield Durability System** models shields, barriers, and magical wards as **directional defenses** with distinct facing sectors that degrade under sustained attack. Rather than abstract "shield HP pools," shields have **surface area coverage** divided into directional sectors (front, rear, left, right, top, bottom), each with independent durability that depletes from concentrated damage.

When a shield sector receives **repeated hits in the same area**, it accumulates **structural damage** based on attack type:
- **Piercing damage** (arrows, spears, magical bolts) creates **focused holes** through concentrated force
- **Blunt damage** (hammers, siege boulders, shockwaves) creates **cracks and fractures** that spread across the sector

Eventually, a sector's durability drops to zero, creating a **breach** through which subsequent attacks bypass the shield entirely. This forces tactical positioning—units must **rotate damaged sectors away** from enemy fire or risk complete shield failure.

**Cross-Project Integration:**
- **Godgame**: Data-driven sector HP pools with damage type modifiers (simplified performance)
- **PureDOTS**: Mathematical framework for directional shielding (see `Directional_Shield_Physics_Agnostic.md`)
- **Space4X**: Full 3D surface area tracking with localized cell damage (see `Directional_Shield_System.md`)

---

## Core Mechanics

### Shield Sectors & Facing

Shields are divided into **6 directional sectors** based on attack origin:

```
Shield Sectors:
- Front (0-60° from forward vector)
- Right (60-120° from forward vector)
- Rear (120-180° from forward vector)
- Left (240-300° from forward vector)
- Top (elevation >45°)
- Bottom (elevation <-45°)
```

**Sector Determination:**
```
Attack Direction = Normalize(Attacker Position - Defender Position)
Defender Forward = Defender Rotation Vector

Dot Product = Dot(Attack Direction, Defender Forward)
Angle = ArcCos(Dot Product)

If (Angle < 60°): Front sector
Else If (Angle < 120°): Check cross product for Left/Right
Else: Rear sector

If (Attack Elevation > 45°): Top sector override
If (Attack Elevation < -45°): Bottom sector override
```

**Example:**
```
War Golem facing North [0, 0, 1]:
- Enemy archer East of golem [1, 0, 0]
- Attack vector: Normalize([1, 0, 0] - [0, 0, 0]) = [1, 0, 0]
- Dot product: [1, 0, 0] · [0, 0, 1] = 0
- Angle: ArcCos(0) = 90°
- Result: Right sector (60-120° range)
```

---

### Sector Durability & HP

Each sector has **independent HP** determined by shield quality and material:

| Shield Type | Quality | HP Per Sector | Total Shield HP | Regen Rate |
|-------------|---------|---------------|-----------------|------------|
| **Wooden Shield** | Standard | 50 HP | 300 HP | 2 HP/s |
| **Wooden Shield** | Masterwork | 80 HP | 480 HP | 3 HP/s |
| **Steel Kite Shield** | Standard | 120 HP | 720 HP | 5 HP/s |
| **Steel Kite Shield** | Legendary | 200 HP | 1,200 HP | 8 HP/s |
| **Tower Shield (Stone)** | Standard | 250 HP | 1,500 HP | 3 HP/s |
| **Magical Ward** | Artifact | 400 HP | 2,400 HP | 15 HP/s |

**Calculation:**
```
Sector HP = Base HP × Quality Multiplier × Material Multiplier
Total Shield HP = Sector HP × 6 sectors

Quality Multipliers:
- Crude: 0.6×
- Standard: 1.0×
- Masterwork: 1.6×
- Legendary: 2.5×
- Artifact: 4.0×

Material Multipliers:
- Wood: 1.0×
- Iron: 1.5×
- Steel: 2.4×
- Enchanted Metal: 3.0×
- Pure Magic (ward): 3.5×
```

---

### Damage Types & Shield Response

#### Piercing Damage (Arrows, Spears, Bolts)

**Mechanic**: Piercing attacks apply **100% damage to hit sector**, no spread to adjacent sectors.

**Shield Degradation:**
```
Sector HP -= Piercing Damage

If (Sector HP ≤ 0):
    Sector Breached = True
    Overflow Damage = Abs(Sector HP)
    Apply Overflow to Entity HP (penetrated shield)
```

**Example:**
```
Steel Kite Shield (Standard, 120 HP per sector)
Front sector takes 3 arrow hits:
- Arrow 1: 40 damage → Front HP: 120 - 40 = 80 HP
- Arrow 2: 45 damage → Front HP: 80 - 45 = 35 HP
- Arrow 3: 50 damage → Front HP: 35 - 50 = -15 HP (BREACHED)
    - Overflow: 15 damage penetrates to soldier HP
    - Front sector: DESTROYED (0 HP, cannot block further attacks)
```

**Tactical Result**: Front sector now has a **hole**. Further frontal attacks bypass shield entirely.

---

#### Blunt Damage (Hammers, Boulders, Shockwaves)

**Mechanic**: Blunt attacks apply **60% damage to hit sector**, **20% to each adjacent sector** (damage spreads through structural stress).

**Shield Degradation:**
```
Hit Sector HP -= Blunt Damage × 0.6
Adjacent Sector 1 HP -= Blunt Damage × 0.2
Adjacent Sector 2 HP -= Blunt Damage × 0.2

Adjacent Sectors:
- Front: Adjacent to Right, Left
- Right: Adjacent to Front, Rear
- Rear: Adjacent to Right, Left
- Left: Adjacent to Front, Rear
- Top: Adjacent to all horizontal sectors (special case)
- Bottom: Adjacent to all horizontal sectors (special case)
```

**Example:**
```
Tower Shield (Stone, 250 HP per sector)
Front sector takes warhammer strike (100 blunt damage):
- Front HP: 250 - (100 × 0.6) = 250 - 60 = 190 HP
- Right HP: 250 - (100 × 0.2) = 250 - 20 = 230 HP
- Left HP: 250 - (100 × 0.2) = 250 - 20 = 230 HP

After 3 more warhammer strikes to front:
- Front HP: 190 - 180 = 10 HP (critical, near-breach)
- Right HP: 230 - 60 = 170 HP (weakened)
- Left HP: 230 - 60 = 170 HP (weakened)
```

**Tactical Result**: Blunt damage **weakens multiple sectors simultaneously**. Shield becomes vulnerable on all frontal-facing angles.

---

### Shield Breach & Penetration

When a sector's HP reaches **0**, it becomes **breached**:

```
Breached Sector:
- Cannot block incoming attacks
- Attacks from breached direction bypass shield entirely
- Hit entity HP directly (no shield absorption)
- Sector HP remains at 0 (cannot regenerate while breached)
- Requires repair action to restore (not automatic regen)
```

**Breach Repair:**
- **Field Repair**: 30 seconds, restores 50% sector HP
- **Full Repair**: 5 minutes at blacksmith, restores 100% HP
- **Magical Mending**: 10 seconds, restores 80% HP (requires mage)

---

### Material-Specific Behavior

#### Wooden Shields

**Properties:**
- Low HP per sector (50-80 HP)
- Vulnerable to **fire damage** (2× damage from flames)
- **Splinters** on breach (spawns 3-5 wooden debris pieces)
- Cheap to replace (50-100 gold)

**Fire Interaction:**
```
Wooden shield hit by fire arrow (30 fire damage):
- Fire damage modifier: 2.0×
- Actual damage: 30 × 2.0 = 60 damage
- Sector HP: 50 - 60 = -10 HP (instant breach)
- Ignition: 40% chance shield catches fire (2 damage/s for 10s)
```

---

#### Steel/Iron Shields

**Properties:**
- High HP per sector (120-200 HP)
- **Resists piercing** (arrows deal -20% damage)
- Heavy (-10% movement speed when equipped)
- Moderate cost (300-800 gold)

**Arrow Resistance:**
```
Steel kite shield (120 HP) hit by arrow (40 piercing damage):
- Piercing resistance: -20%
- Actual damage: 40 × 0.8 = 32 damage
- Sector HP: 120 - 32 = 88 HP
```

---

#### Magical Wards (Pure Energy)

**Properties:**
- Very high HP per sector (300-400 HP)
- **Ignores physical damage type** (all damage treated equally)
- **Fast regeneration** (15 HP/s base)
- **Mana drain** (costs 5 mana/s to maintain)
- Expensive (2,000-10,000 gold equivalent enchantment)

**Damage Absorption:**
```
Magical ward (400 HP) hit by trebuchet boulder (200 blunt damage):
- Magical wards ignore damage spread (100% to hit sector)
- Sector HP: 400 - 200 = 200 HP
- Regeneration: +15 HP/s (fully restored in 13 seconds if no further damage)
```

**Mana Depletion:**
```
Ward user has 200 mana:
- Ward drain: 5 mana/s
- Duration: 200 / 5 = 40 seconds max uptime
- If mana depleted: Ward collapses instantly (all sectors HP → 0)
```

---

## Shield Positioning & Rotation

### Active Shield Facing

Units can **rotate shields** to protect vulnerable sectors:

**Rotation Actions:**
- **Face Enemy**: Automatically rotates strongest sector toward highest threat (default AI)
- **Retreat Stance**: Rotates rear sector forward while backing away (protects retreat)
- **Turtle Formation**: Multiple units overlap shields for 360° coverage

**Example - Shield Rotation:**
```
Soldier with damaged shield:
- Front sector: 10 HP (critical)
- Rear sector: 120 HP (undamaged)

Enemy archer fires from front:
- Player command: "Retreat Stance"
- Soldier rotates 180° (rear sector now facing enemy)
- Arrow hits rear sector: 120 - 40 = 80 HP
- Front sector protected, survives
```

---

### Formation Shield Overlap

Multiple units can **overlap shields** for mutual protection:

```
Shield Wall Formation (5 soldiers):
- Each soldier protects front + 40° to each side
- Combined coverage: 180° frontal arc with 2× HP (overlapping shields)
- Flanking penalty: Side/rear attacks hit single-layer shields
```

**Damage Distribution in Formation:**
```
Arrow volley (10 arrows × 40 damage each) hits shield wall:
- Frontal hits: Distributed across all 5 shields (2 arrows per soldier)
- Damage per soldier: 2 × 40 = 80 damage
- Shield HP: 120 - 80 = 40 HP remaining (each soldier)

Same volley on lone soldier:
- All 10 arrows hit single shield
- Damage: 10 × 40 = 400 damage
- Shield HP: 120 - 400 = -280 HP (BREACHED, 280 overflow to soldier)
- Result: Soldier killed
```

**Tactical Value**: Formation multiplies effective shield HP by number of participants.

---

## Damage Accumulation Examples

### Example 1: Piercing Focused Attack

**Setup:**
- Soldier with Steel Kite Shield (120 HP per sector, 6 sectors = 720 HP total)
- 5 enemy archers targeting soldier
- All archers fire at front sector (coordinated volley)

**Combat Sequence:**
```
Initial state:
- Front: 120 HP, Right: 120 HP, Rear: 120 HP, Left: 120 HP, Top: 120 HP, Bottom: 120 HP

Volley 1 (5 arrows, 40 damage each):
- All arrows hit front sector (focused fire)
- Front: 120 - 200 = -80 HP (BREACHED after 3 arrows)
- Overflow: 80 damage to soldier HP
- Soldier HP: 100 - 80 = 20 HP (critical)
- Other sectors: Undamaged (still 120 HP each)

Result: Front sector destroyed in single volley, soldier near death
```

**Lesson**: Piercing damage creates **localized weak points** rapidly. Shield still has 600 HP in other sectors but is useless against frontal attacks.

---

### Example 2: Blunt Distributed Attack

**Setup:**
- War Golem with Tower Shield (250 HP per sector, 6 sectors = 1,500 HP total)
- 2 enemy siege hammers targeting golem
- Hammers strike front sector repeatedly

**Combat Sequence:**
```
Initial state:
- All sectors: 250 HP

Hammer Strike 1 (150 blunt damage to front):
- Front: 250 - 90 = 160 HP (60% of 150)
- Right: 250 - 30 = 220 HP (20% of 150)
- Left: 250 - 30 = 220 HP (20% of 150)

Hammer Strike 2 (150 blunt damage to front):
- Front: 160 - 90 = 70 HP
- Right: 220 - 30 = 190 HP
- Left: 220 - 30 = 190 HP

Hammer Strike 3 (150 blunt damage to front):
- Front: 70 - 90 = -20 HP (BREACHED)
- Right: 190 - 30 = 160 HP
- Left: 190 - 30 = 160 HP
- Overflow: 20 damage to golem HP

Shield status after 3 strikes:
- Front: 0 HP (breached)
- Right: 160 HP (64% remaining)
- Left: 160 HP (64% remaining)
- Rear/Top/Bottom: 250 HP (100% remaining)
- Total shield HP: 850 / 1,500 = 57% remaining
```

**Lesson**: Blunt damage **depletes total shield HP faster** due to spread. After 3 strikes, shield lost 43% total HP (piercing would only lose 1/6 = 17%).

---

### Example 3: Shield Regeneration Under Fire

**Setup:**
- Knight with Legendary Steel Kite Shield (200 HP per sector, 8 HP/s regen)
- Continuous arrow fire (1 arrow every 2 seconds, 40 damage)
- Knight in defensive stance (not moving)

**Combat Sequence:**
```
t = 0s:
- Front sector: 200 HP
- Regen active: +8 HP/s

t = 2s (Arrow 1 hits):
- Regeneration: 200 + (8 × 2) = 216 HP (capped at max 200 HP)
- Arrow damage: 200 - 40 = 160 HP

t = 4s (Arrow 2 hits):
- Regeneration: 160 + (8 × 2) = 176 HP
- Arrow damage: 176 - 40 = 136 HP

t = 6s (Arrow 3 hits):
- Regeneration: 136 + (8 × 2) = 152 HP
- Arrow damage: 152 - 40 = 112 HP

Equilibrium analysis:
- Damage rate: 40 damage / 2 seconds = 20 damage/s
- Regen rate: 8 HP/s
- Net loss: 20 - 8 = 12 HP/s
- Time to breach: 200 HP / 12 HP/s = 16.7 seconds (9 arrows)
```

**Lesson**: Regeneration **extends shield durability** but cannot prevent eventual breach under sustained fire.

---

## Integration with Existing Systems

### Mech System Integration

**Cross-reference**: See `Mech_System.md`

War Golems and Siege Titans can equip **oversized shields**:

**Golem Tower Shield:**
- 6 sectors × 500 HP = 3,000 HP total
- 10 HP/s regen (powered by arcane core)
- **Shield bash attack**: Uses shield as weapon (200 blunt damage, 5s cooldown)
- Weight: 300 kg (requires golem strength to wield)

**Titan Energy Barrier:**
- 6 sectors × 1,200 HP = 7,200 HP total
- 25 HP/s regen (fusion-powered)
- **Adaptive facing**: AI automatically rotates weakest sector away from fire
- Mana cost: 15 mana/s (titan has 1,000 mana pool)

---

### Debris Interaction

**Cross-reference**: See `Combat_Debris_System.md`

Shields can be **hit by debris** from nearby explosions:

```
Debris from destroyed War Golem (15 pieces @ 25 m/s):
- 8 pieces hit nearby soldier's shield
- Each debris: 23 damage (metal shard, 0.5 kg, 25 m/s)
- Front sector: 120 - (8 × 23) = 120 - 184 = -64 HP (BREACHED)
- Overflow: 64 damage to soldier

Result: Debris from ally destruction breaches friendly shield (friendly fire)
```

---

### Magical Ward Integration

**Cross-reference**: See magic system documentation

Magical wards have **special properties**:

**Elemental Absorption:**
- Fire damage: Absorbed, converted to mana (+2 mana per 10 fire damage)
- Ice damage: Normal damage, slows regeneration (-50% regen for 5s)
- Lightning damage: 2× damage (energy overload)

**Example:**
```
Magical ward (400 HP) hit by fireball (80 fire damage):
- Ward HP: 400 - 80 = 320 HP
- Mana conversion: 80 / 10 × 2 = 16 mana restored
- Ward user mana: 150 + 16 = 166 mana

Result: Fire attacks are least effective against magical wards
```

---

## Gameplay Balance

### Shield Repair Economics

| Shield Type | Repair Cost (50% HP) | Repair Cost (Full) | Replacement Cost |
|-------------|----------------------|--------------------|------------------|
| Wooden Shield | 10 gold | 20 gold | 50 gold |
| Iron Shield | 40 gold | 80 gold | 300 gold |
| Steel Kite Shield | 80 gold | 160 gold | 800 gold |
| Tower Shield | 150 gold | 300 gold | 1,200 gold |
| Magical Ward | 500 gold (mana) | 1,000 gold | 10,000 gold |

**Repair Efficiency:**
- Field repair: 50% HP restored in 30 seconds (free, requires no combat)
- Blacksmith repair: 100% HP restored in 5 minutes (costs 50-300 gold)
- Magical mending: 80% HP restored in 10 seconds (costs 20 mana)

**Economic Decision**: Is it cheaper to repair or replace? Tower shield at 10% HP:
- Repair: 300 gold (full repair)
- Replace: 1,200 gold (new shield)
- **Optimal**: Repair (saves 900 gold)

---

### Counterplay & Tactics

**1. Flanking to Exploit Breached Sectors**

Enemy observes soldier's front sector is breached:
- Enemy archer repositions to frontal angle
- Arrows now bypass shield (0 HP sector cannot block)
- Soldier forced to retreat or rotate shield

**2. Blunt Weapons vs Heavy Shields**

Tower shields resist piercing well (250 HP), but blunt damage spreads:
- Warhammer strike: 150 blunt damage
  - Front: -90 HP
  - Right/Left: -30 HP each
  - Total shield depletion: 150 HP across 3 sectors
- After 2 strikes: Front breached, sides weakened
- Tactic: Use blunt weapons to "crack" heavy shields faster

**3. Shield Wall Counter**

Shield wall provides 2× effective HP on front, but vulnerable to AoE:
- Trebuchet fires boulder into formation (300 AoE blunt damage)
- Hits all 5 soldiers simultaneously
- Each soldier's front sector: -180 HP (60% of 300)
- 3 soldiers' shields breached (if HP < 180)
- Formation broken, vulnerability exploited

---

## ECS Component Definitions

### Core Components

```csharp
/// <summary>
/// Shield with directional sector damage tracking
/// </summary>
public struct DirectionalShield : IComponentData
{
    public ShieldMaterial Material;
    public ShieldQuality Quality;

    // Sector HP (6 sectors: Front, Right, Rear, Left, Top, Bottom)
    public float FrontSectorHP;
    public float RightSectorHP;
    public float RearSectorHP;
    public float LeftSectorHP;
    public float TopSectorHP;
    public float BottomSectorHP;

    public float MaxSectorHP;                // Max HP per sector
    public float RegenerationRatePerSecond;
    public float LastDamageTimestamp;        // For regen delay

    // Breach tracking
    public bool FrontBreached;
    public bool RightBreached;
    public bool RearBreached;
    public bool LeftBreached;
    public bool TopBreached;
    public bool BottomBreached;
}

public enum ShieldMaterial : byte
{
    Wood = 0,          // 1.0× HP, 2× fire damage
    Iron = 1,          // 1.5× HP, 0.8× piercing damage
    Steel = 2,         // 2.4× HP, 0.8× piercing damage
    EnchantedMetal = 3, // 3.0× HP, resists magic
    PureMagic = 4      // 3.5× HP, mana-powered
}

public enum ShieldQuality : byte
{
    Crude = 0,         // 0.6× HP multiplier
    Standard = 1,      // 1.0× HP multiplier
    Masterwork = 2,    // 1.6× HP multiplier
    Legendary = 3,     // 2.5× HP multiplier
    Artifact = 4       // 4.0× HP multiplier
}

/// <summary>
/// Determines which shield sector is hit based on attack angle
/// </summary>
public struct ShieldSectorDetermination : IComponentData
{
    public float3 ShieldForwardVector;       // Direction shield is facing
    public ShieldSector LastHitSector;
}

public enum ShieldSector : byte
{
    Front = 0,
    Right = 1,
    Rear = 2,
    Left = 3,
    Top = 4,
    Bottom = 5,
    None = 255         // No shield or miss
}

/// <summary>
/// Shield damage application with type-specific behavior
/// </summary>
public struct ShieldDamageEvent : IComponentData
{
    public ShieldSector TargetSector;
    public float Damage;
    public DamageType Type;                  // Piercing or Blunt
    public float3 ImpactDirection;           // For sector calculation
    public double Timestamp;
}

public enum DamageType : byte
{
    Piercing = 0,      // 100% to hit sector
    Blunt = 1,         // 60% hit sector, 20% to each adjacent
    Fire = 2,          // Special: 2× vs wood, absorbed by magic wards
    Energy = 3,        // Special: 2× vs magic wards
    Magical = 4        // Ignores material resistances
}
```

---

### Shield Behavior Components

```csharp
/// <summary>
/// Shield regeneration behavior
/// </summary>
public struct ShieldRegeneration : IComponentData
{
    public float RegenRatePerSecond;
    public float RegenDelayAfterDamage;      // Seconds to wait before regen starts
    public bool IsRegenerating;
    public bool CanRegenBreachedSectors;     // False = requires repair action
}

/// <summary>
/// Magical ward-specific behavior
/// </summary>
public struct MagicalWard : IComponentData
{
    public float ManaCostPerSecond;          // 5 mana/s typical
    public float CurrentMana;
    public float MaxMana;
    public bool IsActive;
    public bool ElementalAbsorption;         // Fire damage → mana conversion
}

/// <summary>
/// Shield positioning AI
/// </summary>
public struct ShieldFacingAI : IComponentData
{
    public bool AutoFaceStrongestSector;     // Rotate weakest sector away from fire
    public float RotationSpeed;              // Degrees per second
    public ShieldSector PreferredSector;     // Sector to face toward enemy
}

/// <summary>
/// Shield formation overlap (shield wall)
/// </summary>
public struct ShieldFormationMember : IComponentData
{
    public Entity FormationLeader;
    public int FormationPosition;            // 0-N index in formation
    public bool IsOverlapping;               // True if adjacent shields overlap
    public float OverlapHPBonus;             // +100% HP from overlapping shields
}
```

---

### Shield Breach Components

```csharp
/// <summary>
/// Tracks shield breach state for UI/VFX
/// </summary>
public struct ShieldBreachState : IComponentData
{
    public byte BreachedSectorCount;         // 0-6 (all sectors)
    public float TotalShieldIntegrity;       // 0-1 (percentage remaining)
    public bool IsCritical;                  // True if >3 sectors breached
}

/// <summary>
/// Shield repair action
/// </summary>
public struct ShieldRepairAction : IComponentData
{
    public ShieldSector TargetSector;
    public float RepairAmount;               // HP to restore
    public float RepairDuration;             // Seconds to complete
    public float RepairProgress;             // 0-1 completion
    public bool IsFieldRepair;               // True = 50% HP, False = 100% HP
}
```

---

## Performance Optimization

### Data-Driven Approach (Godgame)

To minimize performance cost, Godgame uses **simplified sector tracking**:

```csharp
/// <summary>
/// Lightweight shield for performance (Godgame only)
/// </summary>
public struct SimplifiedShield : IComponentData
{
    // Store sectors as fixed array instead of individual fields
    public FixedList32Bytes<float> SectorHP;  // 6 sectors × 4 bytes = 24 bytes
    public byte BreachedSectorBitmask;        // Bits 0-5 = sectors breached
    public float MaxSectorHP;
    public float RegenRate;
}

// Bitmask operations for breach checking
public static bool IsSectorBreached(byte bitmask, ShieldSector sector)
{
    return (bitmask & (1 << (int)sector)) != 0;
}

public static void SetSectorBreached(ref byte bitmask, ShieldSector sector)
{
    bitmask |= (byte)(1 << (int)sector);
}
```

**Performance**: Uses 32 bytes vs 48 bytes for full struct (33% memory savings).

---

### Batch Damage Processing

Process shield damage in batches to reduce system overhead:

```csharp
[BurstCompile]
public partial struct ShieldDamageBatchSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Collect all damage events this frame
        var damageEvents = new NativeList<ShieldDamageEvent>(Allocator.TempJob);

        // Process in single batch (vectorized)
        foreach (var damageEvent in damageEvents)
        {
            // Apply damage using SIMD operations
        }

        damageEvents.Dispose();
    }
}
```

---

## Summary

The **Shield Durability & Damage Localization System** provides:

1. **Directional Defense**: 6 sectors with independent HP pools
2. **Damage Type Mechanics**: Piercing (focused holes) vs Blunt (spreading cracks)
3. **Localized Failure**: Breached sectors allow penetration, forcing tactical rotation
4. **Material Diversity**: Wood (cheap, fire-weak), Steel (strong, heavy), Magic (fast regen, mana cost)
5. **Formation Synergy**: Shield walls multiply effective HP through overlap
6. **Economic Balance**: Repair vs replacement decisions, field repairs vs blacksmith
7. **Performance Optimization**: Data-driven approach minimizes overhead for Godgame

**Cross-Project Consistency:**
- **Godgame**: Simplified sector HP pools with bitmask breach tracking
- **PureDOTS**: Mathematical framework for angle calculation, damage distribution
- **Space4X**: Full 3D surface area tracking with localized cell damage (next file)

All shield systems use **consistent sector definitions** (Front/Right/Rear/Left/Top/Bottom) and damage formulas from PureDOTS framework.
