# Projectile Deflection & Fragmentation System (Godgame)

## Overview
Projectiles in flight are affected by environmental forces (explosion blast waves, magical storms, wind) and deliberate manipulation (telekinetic deflection, force barriers). Mass and velocity determine deflection resistance. Projectiles can be shredded mid-flight, producing damaging fragments. Fragment explosives spawn multiple sub-projectiles in 280° upward arcs. Defensive explosives provide anti-projectile countermeasures.

**Integration**: Body ECS (60 Hz) for projectile physics, Mind ECS (1 Hz) for concentration/focus tracking on barrier casters.

---

## Projectile Deflection Mechanics

### Environmental Forces

**1. Explosion Blast Waves**
```
Fireball detonates near flying arrow:
- Blast wave radius: 10 meters
- Force: 500 N (decreases with distance)
- Arrow in flight (5m from epicenter):
  - Arrow mass: 0.05 kg, velocity: 40 m/s
  - Force applied: 500N × (1 - 5/10) = 250N
  - Deflection angle: 15° (lateral push)
  - New trajectory: Arrow curves away from explosion

Result: Archer's shot misses target (aimed center mass, hit shoulder instead)
```

**2. Magical Storms (Wind, Lightning, Chaos)**
```
Lightning Storm active in battlefield:
- Random wind gusts: 20 m/s, changes direction every 2 seconds
- Arrow in flight (travel time: 1.5 seconds):
  - Mid-flight gust: Wind pushes arrow 8° downward
  - Arrow drops 2 meters lower than aimed
  - Hits target's leg instead of chest

Result: Unpredictable aim, skilled archers avoid shooting through storms
```

**3. Natural Wind**
```
Strong crosswind: 15 m/s west-to-east
- Archer shoots north (100m range)
- Arrow flight time: 2.5 seconds
- Wind deflection: 37.5 meters east
- Compensation required: Aim 37.5m west of target

High DEX archer (DEX 85): Compensates automatically (+15° aim adjustment)
Low DEX archer (DEX 40): Fails to compensate, misses target by 20m
```

**4. Gravitational Magic**
```
Wizard casts "Gravity Well" spell:
- Radius: 15m sphere
- Gravitational pull: 3× normal
- Arrow passes through edge of well:
  - Trajectory bent toward center (12° deflection)
  - Arrow speed decreases 30% (pulled downward)
  - Lands 8m short of target

Result: Archer must aim higher and compensate for gravity well
```

### Mass and Velocity Deflection Resistance

**Light Projectiles** (High Deflection):
```
Arrow:
- Mass: 0.05 kg
- Velocity: 40 m/s
- Deflection from 200N blast: 18° angle change

Magic Missile:
- Mass: 0.01 kg (ethereal, minimal mass)
- Velocity: 60 m/s
- Deflection from 200N blast: 35° angle change (highly susceptible)

Thrown Dagger:
- Mass: 0.2 kg
- Velocity: 25 m/s
- Deflection from 200N blast: 8° angle change
```

**Heavy Projectiles** (Low Deflection):
```
Crossbow Bolt:
- Mass: 0.15 kg (3× heavier than arrow)
- Velocity: 80 m/s
- Deflection from 200N blast: 4° angle change (barely affected)

Ballista Bolt:
- Mass: 5 kg
- Velocity: 60 m/s
- Deflection from 200N blast: 0.5° angle change (nearly immune)

Thrown Boulder (Giant):
- Mass: 50 kg
- Velocity: 30 m/s
- Deflection from 200N blast: 0.1° angle change (completely ignores small blasts)
```

**High Velocity Projectiles** (Reduced Deflection):
```
Enchanted Arrow (Haste):
- Mass: 0.05 kg
- Velocity: 120 m/s (3× normal)
- Deflection from 200N blast: 6° angle change (speed resists deflection)

Sling Bullet:
- Mass: 0.08 kg
- Velocity: 100 m/s
- Deflection from 200N blast: 5° angle change
```

**Formula**:
```
Deflection Angle = (Force × Distance) / (Mass × Velocity²) × Environmental Factor
```

---

## Telekinetic & Magical Deflection

### High-Level Casters (Active Deflection)

**Telekinesis** (Wizard, INT 90+):
```
Wizard detects incoming arrow (Perception check: INT + WIS):
- Arrow detected: 50m away, 1.25 seconds until impact
- Wizard casts "Telekinetic Deflection" (instant cast, 20 mana)
- Force applied: 100N lateral push
- Arrow deflection: 25° (sufficient to miss wizard)

Result: Arrow curves away, hits ground 3m to the left

Limitations:
- Requires line of sight to projectile
- Cost: 20 mana per projectile
- Can deflect up to 3 projectiles simultaneously (INT/30)
- Range: 50m (INT/2)
```

**Psionic Force** (Psion, PSI 85+):
```
Psion uses "Kinetic Barrier" (personal force field):
- Radius: 2m sphere
- Automatic deflection (no reaction required)
- Any projectile entering sphere deflected 45°
- Duration: 1 minute (concentration)
- Cost: 40 psi, 10 psi/minute maintenance

Arrow enters barrier:
- Automatically deflected 45° upward
- Arrow sails over Psion's head
- No damage

Limitations:
- Continuous concentration required (can't cast other spells)
- Drains psi rapidly (sustain 6 minutes max)
- Heavy projectiles (>1kg) break through (insufficient force)
```

**Battle Mage Mastery** (High-level tactical use):
```
Battle Mage (INT 95, WIS 82) faces 12 archers:
- Detects volley of 12 arrows
- Casts "Mass Deflection" (area telekinesis)
- Applies 80N force to all arrows in 30m cone
- All 12 arrows deflected 15-20° (scatter)
- 10 arrows miss, 2 hit glancing blows (reduced damage)

Cost: 150 mana (expensive, emergency use only)
```

### Force Barriers (Concentration Ritual)

**Ritual Barrier** (Sustained Protection):
```
Wizard performs concentration ritual:
- Ritual time: 10 seconds setup
- Effect: 10m radius dome, deflects all projectiles <0.5kg
- Duration: Concentration (as long as wizard focuses)
- Cost: 100 mana setup, 5 mana/second maintenance

Mechanics:
- Wizard cannot move (breaks concentration)
- Wizard cannot cast other spells
- Taking damage: Concentration check (DC 15 + damage taken)
  - Success: Barrier holds
  - Failure: Barrier collapses
- Protects entire party (10m radius)

Tactical Use:
- Archer squad enters ritual barrier
- Fire arrows outward (barrier one-way, doesn't block outgoing)
- Enemy arrows deflected (can't reach archers inside)
- Wizard vulnerable (standing still, concentrating)
- Enemy must kill wizard to break barrier
```

**Portable Barrier Artifact** (Magic Item):
```
"Aegis Amulet" (Rare magic item):
- Passive: Deflects 1 projectile per round automatically
- Active: Create 3m barrier (30 seconds, 1/day)
- No concentration required (artifact powered)

Deflection priority:
1. Highest damage projectile
2. Closest projectile
3. First detected projectile
```

---

## Projectile Shredding & Fragmentation

### Mid-Flight Destruction

**Shredding Mechanisms**:

**1. Explosive Interception**
```
Mage casts "Fireball" at incoming arrow volley:
- 12 arrows in flight
- Fireball detonates mid-air (intercept trajectory)
- Blast radius: 8m

Shredding Results:
- 4 arrows: Direct hit, vaporized (0 damage)
- 5 arrows: Partial hit, shredded into fragments (40% damage each)
- 3 arrows: Missed, deflected by blast wave (trajectories changed, scatter)

Fragments from shredded arrows:
- Each shredded arrow produces 5-8 fragments
- Fragments deal 8-15 damage each (vs 40 damage intact arrow)
- Fragments scatter in 180° arc forward
- 3 fragments hit nearby soldier (total 35 damage, comparable to 1 arrow)
```

**2. Blade Barrier Spell**
```
Cleric casts "Blade Barrier" (whirling blades, 10m wall):
- Arrow passes through barrier
- Barrier shreds arrow into fragments

Shredding chance by velocity:
- Slow arrow (20 m/s): 90% shredded
- Normal arrow (40 m/s): 60% shredded
- Fast arrow (80 m/s): 30% shredded (too fast to fully shred)

Shredded arrow:
- 6 fragments produced
- Each fragment: 12 damage
- Fragments continue forward (reduced velocity: 15 m/s)
- Scatter: ±15° from original trajectory
```

**3. Wind Wall Spell**
```
Druid casts "Wind Wall" (hurricane-force winds, 15m high):
- Arrow enters wind wall
- Wind deflection + structural stress

Effects by arrow quality:
- Cheap arrow (low quality): 70% chance shredded, 30% deflected
- Standard arrow: 40% chance shredded, 60% deflected
- Reinforced arrow: 10% chance shredded, 90% deflected
- Enchanted arrow: 0% chance shredded (magically hardened), 60% deflected
```

**Fragment Damage Calculation**:
```
Original arrow damage: 40
Shredded into 7 fragments
Fragment damage: 40 × 0.4 / 7 = 2.3 damage per fragment

If 3 fragments hit target: 3 × 2.3 = 6.9 damage
If all 7 fragments hit (unlikely): 7 × 2.3 = 16.1 damage (40% of original)

Fragments also carry debuffs (see below)
```

---

## Fragment Explosives

### Fragmentation Grenades & Bombs

**Alchemical Fragmentation Grenade**:
```
Grenade composition:
- Iron casing: 0.5 kg
- Alchemical explosive: 0.2 kg
- Pre-scored casing: Designed to shred into 40 fragments

Detonation:
- Explosion force: 1,000 N
- Casing shatters into 40 fragments
- Fragments launch in 280° upward arc (cone shape)
- Fragment velocity: 30-50 m/s (randomized)
- Fragment mass: 12.5g each

Damage Pattern:
- Each fragment: 15-25 damage
- Fragments spread across 10m radius
- Average 8-12 fragments hit entities within 5m
- Total damage (5m): 120-300 damage (lethal to unarmored)
- Armor reduces fragment damage significantly (each fragment separate hit)
```

**280° Upward Arc Explanation**:
```
Grenade on ground, detonates:
- 280° arc: Covers forward hemisphere + sides (not directly backward/downward)
- Upward bias: Explosion channels energy upward (ground blocks downward fragments)

Fragment distribution:
- 0-45° (forward low): 25% of fragments (10 fragments)
- 45-90° (forward high): 30% of fragments (12 fragments)
- 90-135° (sides high): 25% of fragments (10 fragments)
- 135-180° (sides low): 15% of fragments (6 fragments)
- 180-280° (backward low): 5% of fragments (2 fragments)

Most fragments travel forward/upward (optimal anti-personnel pattern)
```

**Sticky Bomb** (Attach to target):
```
Alchemist throws sticky bomb at knight:
- Bomb adheres to knight's chestplate
- 3-second fuse
- Knight realizes, tries to remove (fails, sticky)
- Detonation:
  - Knight takes 80% of fragments (32/40 fragments)
  - Surrounding entities take 20% of fragments (8/40 scatter)
  - Knight total damage: 480-800 damage (almost certainly fatal)
  - Full plate armor: Reduces to 240-400 damage (still critical)
```

**Fragmentation Arrow** (Airburst):
```
Enchanted arrow with fragmentation tip:
- Arrow flies toward target
- Proximity fuse: Detonates 2m before target
- Arrow tip explodes into 12 fragments
- Fragments spray forward in 120° cone

Purpose: Defeat shields
- Target raises shield to block arrow
- Arrow detonates before hitting shield
- Fragments spray around shield, hit target's exposed areas
- 4-6 fragments hit target (bypasses shield)
- Total damage: 60-90 (less than direct hit, but unavoidable)
```

### Fragment Debuffs

**Bleeding**:
```
Fragment hits unarmored target:
- Jagged metal fragment embeds in flesh
- Bleed effect: 2-5 damage/second for 20 seconds
- Total bleed damage: 40-100 damage
- Requires medical attention (bandage, healing spell)

Multiple fragments:
- 6 fragments hit target
- 6 separate bleed effects (stack)
- Total bleed: 12-30 damage/second
- Target bleeds out in 10 seconds without healing
```

**Embedded Fragments** (Movement Penalty):
```
3 fragments embed in target's leg:
- Movement speed: -30% (pain, impaired mobility)
- DEX penalty: -10 (reduced agility)
- Duration: Until removed (field surgery or healing magic)

Removal:
- Field surgery: 1 minute per fragment, INT check (DC 12)
- Healing spell: Instant removal (50 mana)
- Failure: Deals additional 10 damage, fragment remains
```

**Infection Risk**:
```
Dirty fragment (rusty iron, contaminated):
- Infection chance: 40% if not treated within 10 minutes
- Infection effect: -20 HP max, -2 CON, fever
- Treatment: Cleanse Wounds spell (20 mana) or antiseptic potion
- Untreated infection: Spreads to bloodstream, death in 3 days
```

**Organ Damage** (Critical Fragments):
```
Fragment critical hit (chest, head, abdomen):
- Pierces vital organ
- Immediate: 50 bonus damage
- Ongoing: Internal bleeding (10 damage/second)
- Fatal in 30 seconds without emergency healing
- Requires high-level Heal spell (Rank 5+, 150 mana)
```

---

## Defensive Explosives (Anti-Projectile)

### Active Defense Systems

**1. Archer Squad Defensive Protocol**
```
Elite archer squad (8 archers):
- Doctrine: 1 designated "shielder" (grenadier role)
- Shielder carries 6 fragmentation grenades
- Enemy archer volley incoming (24 arrows)

Defensive Response:
- Shielder throws grenade to intercept arrow volley mid-flight
- Grenade detonates in arrow cloud (8m radius)
- Results:
  - 12 arrows vaporized (direct blast)
  - 8 arrows shredded (fragments produced, scattered)
  - 4 arrows deflected (blast wave)
  - 0 arrows reach archer squad (100% interception)

Cost: 1 grenade (50 gold)
Benefit: Saved 8 archers from potential casualties
```

**2. Castle Wall Defense**
```
Castle defenders face siege:
- 40 flaming arrows incoming (fire volley)
- Defender mage casts "Fireball" to intercept
- Fireball detonates mid-air (15m from wall)

Results:
- 28 arrows destroyed (70% interception)
- 8 arrows deflected (hit ground before wall)
- 4 arrows penetrate (hit wall, minimal damage)

Cost: 60 mana
Benefit: Prevented fire from spreading on wall structures
```

**3. Ship Anti-Boarding Defense**
```
Warship faces boarding action:
- Enemy ship launches grappling hooks (12 hooks with ropes)
- Ship's alchemist throws fragmentation grenades at hooks
- 3 grenades detonate near hooks in flight

Results:
- 8 hooks destroyed (ropes severed by fragments)
- 3 hooks deflected (blown off course)
- 1 hook penetrates (attaches to ship)

Enemy boarding attempt: Severely weakened (1/12 hooks vs 12/12)
Ship crew: Defends single grapple point successfully
```

**4. Personal Defense (Desperate Measure)**
```
Knight faces dragon fire breath:
- Dragon inhales (preparing flame breath)
- Knight throws grenade into dragon's mouth (INT check DC 18)
- Success: Grenade enters throat
- Detonation: Internal explosion

Results:
- Dragon takes 400 damage (internal, bypasses scales)
- Dragon chokes (breath attack interrupted)
- Knight survives (no fire breath)

Risk: Extremely dangerous (close range, requires precision throw)
Reward: Negate devastating breath attack
```

### Ships with Explosive Countermeasures

**Naval Warship** (Broadside Defense):
```
Warship loadout:
- 20 cannons (primary armament)
- 8 swivel guns (anti-personnel)
- 4 fragmentation catapults (anti-projectile)

Fragmentation Catapult:
- Launches fragmentation bomb 50m
- Airburst detonation (proximity fuse)
- 280° arc (forward-facing)
- Purpose: Destroy incoming flaming arrows, grappling hooks

Combat Scenario:
- Enemy ship fires 30 flaming arrows
- Warship launches 2 fragmentation bombs to intercept
- Bombs detonate in arrow cloud
- 22 arrows destroyed (73% interception)
- 8 arrows hit ship (manageable damage)
```

**Flying Ship** (Aerial Defense):
```
Airship (magical levitation):
- Vulnerable to ground-based ballista fire
- Defense: "Chaff Bombs" (fragmentation grenades suspended on parachutes)

Deployment:
- Drop 6 chaff bombs below airship
- Bombs float on parachutes (10m below ship)
- Incoming ballista bolt triggers proximity detonation
- Bolt shredded by fragments

Result: Airship protected from ground fire (90% interception rate)
```

**Dragon Rider** (Personal Defense):
```
Dragon rider carries 4 sticky bombs:
- Enemy archer squad fires volley (8 arrows at rider)
- Rider throws bomb forward
- Bomb detonates in arrow path
- 6 arrows destroyed, 2 deflected

Alternative use:
- Rider throws bombs at enemy fortifications (offensive)
- Dragon provides bombing platform (aerial superiority)
```

---

## Skill and Stat Interactions

### Archer Prediction (DEX + WIS)

**Low-Skill Archer** (DEX 40, WIS 35):
```
Shoots arrow in windy battlefield:
- Does not account for wind (lacks experience)
- Does not predict explosions (poor battlefield awareness)
- Arrow misses target by 5m (wind deflection)

Hit chance: 40% (base) - 20% (environmental hazards) = 20%
```

**Mid-Skill Archer** (DEX 65, WIS 50):
```
Shoots arrow in windy battlefield:
- Partially compensates for wind (+5° aim adjustment)
- Aware of nearby explosions (avoids shooting through blast radius)
- Arrow misses target by 2m (incomplete compensation)

Hit chance: 65% (base) - 10% (environmental hazards) = 55%
```

**Master Archer** (DEX 90, WIS 80):
```
Shoots arrow in windy battlefield:
- Fully compensates for wind (+12° aim adjustment)
- Predicts explosion timing (delays shot 0.5s to avoid blast)
- Accounts for gravity well (aims 3° higher)
- Arrow hits target center mass

Hit chance: 90% (base) - 2% (environmental hazards) = 88%

Master archers can even USE environmental hazards:
- Intentionally shoot through explosion to curve arrow around shield
- "Impossible shot": Arrow curves 30° mid-flight (blast-assisted)
```

### Mage Interception (INT + Perception)

**Novice Mage** (INT 60, Perception 40):
```
Cannot detect fast projectiles (arrows >60 m/s too fast to perceive)
Can only deflect slow projectiles (thrown rocks, magic missiles)
Reaction time: 1 second (too slow for arrow interception)
```

**Adept Mage** (INT 80, Perception 65):
```
Detects arrows within 30m (0.75s reaction time)
Can deflect 1 arrow per round (single-target telekinesis)
Cannot handle volleys (overwhelmed by multiple targets)
```

**Archmage** (INT 98, Perception 90):
```
Detects arrows within 80m (0.2s reaction time)
Can deflect 5 arrows simultaneously (mass telekinesis)
Predicts volley patterns (intercepts entire volley with single Fireball)
Can even catch arrows and throw them back (advanced telekinesis)
```

---

## ECS Integration

### Body ECS (60 Hz) - Projectile Physics

**Systems**:
- `ProjectileDeflectionSystem`: Apply environmental forces to projectiles in flight
- `ProjectileShreddingSystem`: Detect when projectiles are destroyed, spawn fragments
- `FragmentSpawnSystem`: Create fragment sub-projectiles from shredded projectiles
- `BlastWaveSystem`: Calculate explosion blast waves, apply forces to nearby projectiles
- `TelekineticForceSystem`: Apply magical forces from caster spells to projectiles

**Components**:
```csharp
public struct ProjectileComponent : IComponentData
{
    public float3 Position;
    public float3 Velocity;            // m/s
    public float Mass;                 // kg
    public float DragCoefficient;      // Air resistance
    public ProjectileType Type;        // Arrow, Bolt, Magic, Fragment
    public bool IsShredded;            // Destroyed, spawning fragments
}

public struct DeflectionForceComponent : IComponentData
{
    public float3 ForceVector;         // N (newtons)
    public float3 ForceOrigin;         // Position of force source
    public float ForceRadius;          // Effective radius (m)
    public ForceType Type;             // Explosion, Wind, Telekinesis, Gravity
}

public struct FragmentationComponent : IComponentData
{
    public int FragmentCount;          // Number of fragments to spawn (5-40)
    public float FragmentDamagePercent; // % of original damage per fragment
    public float SpreadAngle;          // Cone angle (280° typical)
    public float3 SpreadDirection;     // Primary spread direction
    public bool HasDebuff;             // Fragments inflict bleed/embedded
}

public struct TelekineticBarrierComponent : IComponentData
{
    public Entity CasterEntity;
    public float3 BarrierCenter;
    public float BarrierRadius;        // m
    public float DeflectionForce;      // N applied to projectiles
    public float ConcentrationLevel;   // 0-1 (drops if caster damaged)
    public float ManaCostPerSecond;    // Mana drain rate
}

public struct ProjectileFragmentComponent : IComponentData
{
    public Entity ParentProjectile;    // Original projectile that fragmented
    public float OriginalDamage;       // Parent projectile damage
    public int FragmentIndex;          // Which fragment (0 to FragmentCount-1)
    public DebuffType InflictedDebuff; // Bleed, Embedded, Infection
}

public enum ProjectileType : byte
{
    Arrow,
    Bolt,
    MagicMissile,
    Fireball,
    ThrowingKnife,
    Fragment,      // Spawned from shredded projectile
    BallistaBolt,
    Boulder
}

public enum ForceType : byte
{
    Explosion,
    Wind,
    Telekinesis,
    GravityWell,
    MagicalStorm
}

public enum DebuffType : byte
{
    Bleed,
    Embedded,
    Infection,
    OrganDamage
}
```

### Mind ECS (1 Hz) - Concentration Tracking

**Systems**:
- `ConcentrationSystem`: Track caster concentration for barrier maintenance
- `PerceptionSystem`: Detect incoming projectiles for deflection
- `PredictionSystem`: Calculate archer aim compensation for environmental forces

**Components**:
```csharp
public struct ConcentrationComponent : IComponentData
{
    public Entity ActiveSpell;         // Spell/barrier being concentrated on
    public float ConcentrationLevel;   // 0-1 (100% = full focus)
    public float DamageTaken;          // Accumulated damage (reduces concentration)
    public bool IsConcentrating;       // Currently maintaining spell
    public int ConcentrationDC;        // Difficulty check threshold
}

public struct PerceptionComponent : IComponentData
{
    public int PerceptionSkill;        // 0-100
    public float DetectionRange;       // m (INT/2 for mages)
    public float ReactionTime;         // seconds (lower is better)
    public int SimultaneousTargets;    // How many projectiles can track (INT/30)
}

public struct ArcherPredictionComponent : IComponentData
{
    public int DEX;
    public int WIS;
    public float WindCompensation;     // Degrees of aim adjustment
    public bool CanPredictExplosions;  // WIS > 70
    public bool CanUseCurveShots;      // DEX > 85, intentionally use deflections
}
```

---

## Example Scenarios

### Scenario 1: Archer vs Explosion Hazard
```
Archer (DEX 70, WIS 55) shoots at enemy knight (50m away):
- Fireball detonates 20m from archer, 10m from arrow flight path
- Blast wave: 400N force

Arrow in flight:
- Mass: 0.05 kg
- Velocity: 45 m/s
- Deflection: (400 × 10) / (0.05 × 45²) ≈ 40 / 101 = 0.4 radians ≈ 23°

Result: Arrow deflected 23° left, misses knight by 10m

Archer's prediction:
- WIS 55 check (DC 50): Success (barely)
- Archer predicted explosion, delayed shot by 0.8 seconds
- Explosion passes, archer shoots after
- Arrow hits knight (no deflection)

Outcome: Skilled archer compensates for environmental hazards
```

### Scenario 2: Telekinetic Barrier vs Arrow Volley
```
Wizard (INT 88) uses Kinetic Barrier (2m radius):
- 8 arrows incoming (massed volley)
- Arrows enter barrier radius sequentially (0.1s apart)

Barrier effects:
- Arrow 1-3: Deflected 45° upward (barrier fresh, full power)
- Arrow 4-6: Deflected 30° upward (wizard tiring, 60 mana spent)
- Arrow 7-8: Deflected 15° upward (wizard exhausted, 100 mana spent)

Result:
- Arrows 1-6 sail overhead (complete deflection)
- Arrow 7 grazes wizard's shoulder (5 damage, partial deflection)
- Arrow 8 hits wizard's leg (20 damage, insufficient deflection)

Wizard concentration check (DC 15 + 25 damage = DC 40):
- INT 88 + d20 roll (14) = 102 vs DC 40: Success
- Barrier holds despite damage

Outcome: Barrier blocks most arrows, but not perfect (wizard injured but alive)
```

### Scenario 3: Fragmentation Grenade in Chokepoint
```
Alchemist throws frag grenade at 8 enemy soldiers in hallway (chokepoint):
- Grenade lands center of group
- Detonation: 40 fragments, 280° upward arc

Fragment distribution (hallway confines spread):
- 32 fragments hit soldiers (80% of fragments, narrow space)
- 8 fragments hit walls (20% wasted)

Soldier casualties:
- Each soldier hit by 4 fragments average (32/8)
- Each fragment: 18 damage
- Each soldier: 4 × 18 = 72 damage
- Soldiers have 50 HP, light leather armor (10% reduction)
- Effective damage: 72 × 0.9 = 65 damage
- Result: All 8 soldiers killed instantly

Additional effects:
- 6 soldiers suffer bleed debuffs (would have bled out if survived)
- Hallway choked with bodies (movement impeded)

Outcome: Single grenade eliminates entire squad (devastating in enclosed space)
```

### Scenario 4: Ship Defensive Explosives
```
Warship faces enemy fireship attack:
- Fireship launches 20 grappling hooks + 40 flaming arrows
- Warship has 4 fragmentation catapults

Defensive sequence:
- Catapult 1 targets grappling hooks: Destroys 8/20 hooks (40%)
- Catapult 2 targets grappling hooks: Destroys 6/12 remaining (50%)
- Catapult 3 targets arrow volley: Destroys 18/40 arrows (45%)
- Catapult 4 targets arrow volley: Destroys 12/22 arrows (55%)

Net result:
- Grappling hooks: 6/20 penetrate (30% success)
- Flaming arrows: 10/40 penetrate (25% success)

Outcome:
- Warship partially boarded (6 grapples vs 20, manageable)
- Minor fire damage (10 arrows vs 40, crew extinguishes easily)
- Ship survives fireship attack (defensive explosives critical)
```

---

## Key Design Principles

1. **Mass and Velocity Matter**: Heavy/fast projectiles resist deflection, light/slow projectiles easily deflected
2. **Environmental Unpredictability**: Skilled entities compensate, unskilled entities surprised
3. **High-Level Casters Dominate**: Telekinetic deflection and barriers powerful but costly (mana drain)
4. **Concentration = Vulnerability**: Barrier casters immobile, cannot cast other spells, break if damaged
5. **Fragments Still Damage**: Shredded projectiles produce damaging fragments (40% original damage)
6. **Fragment Explosives Lethal**: 280° arc upward spread, 8-12 fragments hit entities in 5m radius
7. **Debuffs Accumulate**: Multiple fragments = multiple bleeds (stacking damage over time)
8. **Defensive Explosives Viable**: Anti-projectile countermeasures effective (40-70% interception)
9. **Tactical Complexity**: Skilled archers use deflections intentionally (curve shots around obstacles)
10. **Armor Important**: Plate armor resists fragments better than leather (damage reduction per fragment)

---

**Integration with Other Systems**:
- **Infiltration Detection**: Explosive traps trigger, deflect projectiles from infiltrators
- **Crisis Alert States**: High alert = more defensive explosives deployed (anticipate attack)
- **Blueprint System**: Alchemists learn improved fragmentation designs (more fragments, better spread)
- **Permanent Augmentation**: Cybernetic eyes (enhanced perception, detect projectiles earlier)
