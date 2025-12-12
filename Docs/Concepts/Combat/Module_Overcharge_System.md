# Module Overcharge and Mana Redistribution System - Godgame

## Overview

Magical constructs and powered entities can dynamically redistribute mana flow between their enchanted modules to adapt to combat situations. Modules can be **overcharged** by channeling additional mana beyond baseline requirements, enhancing their performance at the cost of increased mana consumption and heat generation. Pilots can **focus** mana into critical systems while depowering less essential modules, creating tactical advantages through adaptive power management.

**Core Concepts:**
- **Baseline Power**: Minimum mana required for module to function at 100% effectiveness
- **Overcharge**: Channeling 120-200% of baseline mana for enhanced performance (120-180% effectiveness)
- **Underpowered**: Operating at 50-99% baseline mana results in degraded performance
- **Power Focus**: Redistribute mana from depowered modules to overcharged priority systems
- **Heat Penalty**: Overcharged modules generate 150-300% heat, risking burnout

---

## Mana Allocation System

### Power States

Each module can operate in one of five power states:

**1. Disabled (0% power)**
- Module offline, no mana consumption
- No functionality
- Can be reactivated instantly (no warmup)

**2. Standby (25% power)**
- Module on standby, minimal mana draw
- 40% effectiveness (reduced performance)
- Instant activation to full power

**3. Normal (100% power)**
- Standard operation, baseline mana consumption
- 100% effectiveness
- Default state for most modules

**4. Overcharged (150% power)**
- Enhanced operation, 150% mana consumption
- 140% effectiveness
- +80% heat generation
- 2% burnout risk per minute

**5. Maximum Overcharge (200% power)**
- Peak operation, 200% mana consumption
- 180% effectiveness
- +150% heat generation
- 8% burnout risk per minute
- Pilot concentration required (DC 15)

---

### Mana Budget Example

**War Golem** (2,500 mana capacity, 80 mana/sec regen):

**Normal Configuration:**
- Elemental Fist: 40 mana/sec (idle), 120 mana/strike
- Ward Shield: 50 mana/sec (active)
- Sensory Array: 20 mana/sec
- Chassis idle: 30 mana/sec
- **Total idle**: 140 mana/sec
- **Deficit**: -60 mana/sec (can idle for 42 seconds before depleted)

**Overcharged Configuration (Combat Focus: Weapons):**
- Elemental Fist (Overcharged 150%): 60 mana/sec idle, 180 mana/strike
  - **Effect**: 140% damage (224 damage instead of 160), 70% proc chance instead of 50%
- Ward Shield (Standby 25%): 12.5 mana/sec
  - **Effect**: 40% shield strength (720 HP instead of 1,800 HP)
- Sensory Array (Normal): 20 mana/sec
- Chassis idle: 30 mana/sec
- **Total idle**: 122.5 mana/sec
- **Deficit**: -42.5 mana/sec
- **Combat advantage**: +40% weapon damage, shields weakened to 40%

**Maximum Overcharge Configuration (All-In Assault):**
- Elemental Fist (Max 200%): 80 mana/sec idle, 240 mana/strike
  - **Effect**: 180% damage (288 damage), 85% proc chance, +150% heat
- Ward Shield (Disabled): 0 mana/sec
- Sensory Array (Standby): 5 mana/sec
- Chassis idle: 30 mana/sec
- **Total idle**: 115 mana/sec
- **Deficit**: -35 mana/sec
- **Combat advantage**: +80% weapon damage, no shields, burnout risk 8%/min

---

## Overcharge Performance Scaling

### Damage/Output Scaling

```
Overcharge Effectiveness = Base × (0.6 + (Power% × 0.006))

Power States:
- 0% (Disabled): 0% effectiveness
- 25% (Standby): 0.6 + (25 × 0.006) = 0.75 → 75% effectiveness
- 50% (Underpowered): 0.6 + (50 × 0.006) = 0.90 → 90% effectiveness
- 100% (Normal): 0.6 + (100 × 0.006) = 1.20 → 120% effectiveness (but normalized to 100%)
- 150% (Overcharged): 0.6 + (150 × 0.006) = 1.50 → 150% effectiveness (actual: 140% due to diminishing returns)
- 200% (Max): 0.6 + (200 × 0.006) = 1.80 → 180% effectiveness
```

**Adjusted for Diminishing Returns:**
```
Final Effectiveness = Base × min(1.8, (0.4 + (Power% × 0.008) - (Power%² × 0.00001)))

Results:
- 0%: 0%
- 25%: 60%
- 50%: 82%
- 75%: 94%
- 100%: 100%
- 125%: 112%
- 150%: 140%
- 175%: 165%
- 200%: 180%
- 250%: 180% (capped, diminishing returns prevent further gains)
```

---

### Heat Generation Scaling

```
Heat Generation = Base Heat × (Power%² / 100²)

Examples:
- 100% power: Base Heat × 1.0
- 150% power: Base Heat × 2.25
- 200% power: Base Heat × 4.0
```

**Example:**
Elemental Fist (base heat: 100 heat/strike):
- Normal (100%): 100 heat
- Overcharged (150%): 100 × 2.25 = **225 heat**
- Max Overcharge (200%): 100 × 4.0 = **400 heat**

---

### Burnout Risk

Overcharged modules risk catastrophic failure:

```
Burnout Risk = (Overcharge% - 100%) × 0.04% per second

Power States:
- 100%: 0% risk
- 150%: (150 - 100) × 0.04% = 2% per minute
- 175%: (175 - 100) × 0.04% = 3% per minute
- 200%: (200 - 100) × 0.04% = 4% per minute
- 250%: (250 - 100) × 0.04% = 6% per minute (illegal overcharge, only possible with artifacts)
```

**Burnout Consequences:**
- Module disabled for 60 seconds (emergency cooldown)
- -30% module durability
- 10% chance of permanent damage (requires repair)

**Quality Modifiers:**
- **Crude**: +100% burnout risk (4% at 150% becomes 8%)
- **Standard**: Base risk
- **Masterwork**: -40% burnout risk (4% becomes 2.4%)
- **Legendary**: -70% burnout risk (4% becomes 1.2%)
- **Artifact**: -95% burnout risk (4% becomes 0.2%), can safely reach 250% overcharge

---

## Power Focus Presets

Pilots can select focus presets to instantly redistribute mana:

### 1. Balanced (Default)
All modules at 100% power, no overcharge.

**War Golem Example:**
- Weapons: 100% (160 damage)
- Shields: 100% (1,800 HP)
- Sensors: 100% (100m range)
- Mobility: 100% (5 m/s)

---

### 2. Weapons Focus
Weapons overcharged to 150%, shields at 50%, sensors at 75%, mobility normal.

**War Golem Example:**
- Weapons: 150% (224 damage, +40%)
- Shields: 50% (900 HP, -50%)
- Sensors: 75% (85m range, -15%)
- Mobility: 100% (5 m/s)
- **Mana saved from shields/sensors**: 40 mana/sec
- **Mana redirected to weapons**: +20 mana/sec

---

### 3. Defense Focus
Shields overcharged to 150%, weapons at 75%, sensors at 100%, mobility reduced to 75%.

**War Golem Example:**
- Weapons: 75% (120 damage, -25%)
- Shields: 150% (2,520 HP, +40%)
- Sensors: 100% (100m range)
- Mobility: 75% (3.75 m/s, -25%)

---

### 4. Mobility Focus
Movement overcharged to 150%, weapons at 75%, shields at 50%, sensors at 100%.

**War Golem Example:**
- Weapons: 75% (120 damage)
- Shields: 50% (900 HP)
- Sensors: 100% (100m)
- Mobility: 150% (7.5 m/s, +50%)
- **Tactic**: Hit-and-run, kiting enemies

---

### 5. Stealth Focus
Sensors overcharged to 150%, active camouflage to 150%, weapons at 50%, shields disabled.

**Crystal Automaton Example:**
- Weapons: 50% (40 damage, -50%)
- Shields: 0% (disabled)
- Sensors: 150% (210m range, +40%)
- Camouflage: 150% (92% stealth instead of 75%)
- **Tactic**: Reconnaissance, infiltration

---

### 6. Emergency Power (All-In)
All essential systems disabled, single critical module at 200% maximum overcharge.

**Example: Siege Titan "Last Stand"**
- Primary Weapon (Lightning Cannon): 200% (2,160 damage instead of 1,200)
- All other modules: Disabled
- **Duration**: 15 seconds (then mana depleted)
- **Risk**: 8% burnout per minute = 2% over 15 sec
- **Use case**: Finishing blow against critical target

---

## Dynamic Redistribution

### Manual Redistribution

Pilots can manually adjust individual module power levels:

**Command**: "Increase weapon power to 175%, reduce shields to 50%"
- Execution time: 0.5 seconds (neural link) or 2 seconds (manual controls)
- Power redistribution is instant once command executed

**Concentration Check (for extreme overcharge 175%+):**
```
DC = 10 + (Overcharge% - 150) / 5

Examples:
- 150%: DC 10
- 175%: DC 15
- 200%: DC 20
- 250%: DC 30 (requires legendary willpower)
```

Pilot must pass INT or WIS check (whichever is higher) to maintain overcharge. Failure results in:
- Overcharge drops to 150%
- -10% effectiveness for 5 seconds (mental strain)

---

### Automated Power Management

Advanced constructs (TL 7+) or constructs with AI cores can auto-manage power:

**AI Power Management Modes:**

**1. Aggressive**
- Automatically overcharge weapons to 150% when enemy detected
- Reduce shields to 75% during offense
- Risk tolerance: Medium (allows 150% overcharge)

**2. Defensive**
- Automatically overcharge shields to 150% when taking damage
- Reduce weapons to 75% when shields below 50%
- Risk tolerance: Low (never exceeds 125% overcharge)

**3. Adaptive**
- Analyzes threat and adjusts dynamically
- Overcharge weapons vs weak enemies (low heat risk)
- Overcharge shields vs strong enemies (survival priority)
- Risk tolerance: High (allows 175% overcharge if needed)

**4. Efficiency**
- Minimize mana consumption
- Never overcharge (all modules at 100% or standby)
- Prioritizes sustained operation over burst performance

---

### Conditional Triggers

Pilots can set automated triggers:

**Examples:**

1. **"If shields below 30%, redirect all power to shields"**
   - Shields: 200% (emergency overcharge)
   - Weapons: 0% (disabled)
   - Sensors: 25% (standby)

2. **"If enemy within 20m, overcharge weapons to 175%"**
   - Weapons: 175%
   - Shields: 75%
   - Triggers close-range alpha strike

3. **"If heat above 80%, disable all non-essential systems"**
   - Weapons: 0%
   - Shields: 100%
   - Sensors: 25%
   - Wait for heat dissipation

4. **"If mana below 20%, reduce all systems to 75%"**
   - All modules: 75%
   - Conserves mana for extended operation

---

## Mana Capacitor Banks

Advanced constructs can install **Mana Capacitors** to store excess mana for burst overcharge:

### Capacitor Module (2 slots)

**Specifications:**
- **Storage**: 500-2,000 mana (TL 5-10)
- **Charge rate**: 20% of core regen (16 mana/sec if core regens 80 mana/sec)
- **Discharge rate**: 200 mana/sec (instant burst)
- **Weight**: 600-1,200 kg

**Quality Modifiers:**
- **Standard**: 1,000 mana storage
- **Masterwork**: 1,500 mana storage, +30% charge rate
- **Legendary**: 2,200 mana storage, +60% charge rate, emergency discharge (400 mana/sec)
- **Artifact**: 3,500 mana storage, +100% charge rate, overcharge immune (can discharge at 300% power)

**Usage:**
- Passive: Slowly charges when core regen exceeds consumption
- Active: Pilot activates burst mode, capacitor discharges into specific module
- **Result**: Module receives 200-400 mana/sec instantly, enabling temporary 250-300% overcharge

**Example:**
War Golem with Masterwork Capacitor (1,500 mana stored):
- Pilot activates "Weapon Burst"
- Capacitor discharges 200 mana/sec into Elemental Fist
- Weapon operates at 250% power for 7.5 seconds (1,500 / 200)
- Damage: 160 × 2.5 = **400 damage per strike** (usually capped at 180%, but capacitor allows temporary overcap)
- After 7.5 seconds, capacitor depleted, weapon returns to normal

---

## Overcharge Synergies

### Module Combinations

Some module combinations synergize when overcharged together:

**1. Weapon + Targeting Computer (both 150%)**
- Weapon: +40% damage
- Targeting: +40% accuracy
- **Synergy Bonus**: +15% crit chance
- **Total**: +40% damage, +40% accuracy, +15% crit

**2. Shield + Armor Plates (both 150%)**
- Shield: +40% HP
- Armor: +40% rating
- **Synergy Bonus**: +20% damage reflection
- **Total**: +40% shield HP, +40% armor, +20% reflection

**3. Sensors + Camouflage (both 150%)**
- Sensors: +40% range
- Camouflage: +40% stealth
- **Synergy Bonus**: Detect enemies before they detect you (+25% initiative)
- **Total**: +40% sensor range, +40% stealth, +25% initiative

**4. Jump Jets + Mobility Enhancer (both 175%)**
- Jump Jets: +75% distance (50m becomes 87.5m)
- Mobility: +75% speed (5 m/s becomes 8.75 m/s)
- **Synergy Bonus**: Mid-air dash (180° direction change while airborne)
- **Risk**: 3% burnout per minute each (6% combined)

---

### Elemental Core Synergies

Elemental cores provide bonuses when overcharging matching modules:

**Fire Core (Legendary):**
- Normal: +20% damage to fire weapons
- Overcharged 150%: +35% damage to fire weapons, +15% proc chance
- Overcharged 200%: +60% damage to fire weapons, +30% proc chance, AoE +50%

**Lightning Core (Legendary):**
- Normal: +25% recharge speed for all modules
- Overcharged 150%: +45% recharge speed, -20% cooldowns
- Overcharged 200%: +80% recharge speed, -40% cooldowns, chain lightning (hit 2 extra targets)

**Ice Core (Legendary):**
- Normal: +30% cooling, no overheating
- Overcharged 150%: +60% cooling, modules can reach 175% with 0% burnout risk
- Overcharged 200%: +100% cooling, modules can reach 200% with 0% burnout risk, freeze enemies (10% chance on hit)

**Earth Core (Legendary):**
- Normal: +15 armor, +40% structural HP
- Overcharged 150%: +30 armor, +80% structural HP, tremor sense (detect through walls)
- Overcharged 200%: +50 armor, +140% structural HP, seismic slam (AoE stun on landing)

---

## Overcharge Drawbacks

### Heat Buildup

Overcharged modules generate exponentially more heat:

**Heat Formula:**
```
Heat Generated = Base Heat × (Power%² / 100²)
```

**Example (Elemental Fist, base heat 100):**
- 100%: 100 heat/strike
- 125%: 156 heat/strike (+56%)
- 150%: 225 heat/strike (+125%)
- 175%: 306 heat/strike (+206%)
- 200%: 400 heat/strike (+300%)

**Construct Heat Capacity:**
- War Golem: 1,200 heat
- Siege Titan: 5,000 heat

**Overcharge Example:**
War Golem fires Elemental Fist at 200% overcharge:
- Heat per strike: 400
- Heat capacity: 1,200
- **Strikes until overheat**: 1,200 / 400 = **3 strikes**
- Normal (100% power): 1,200 / 100 = **12 strikes**

**Solution**: Install Heat Sinks (+500-1,200 heat dissipation/sec) or operate in bursts with cooldown periods.

---

### Mana Drain

Overcharged modules consume mana exponentially:

**Mana Consumption Formula:**
```
Mana Consumed = Base Cost × (Power% / 100)
```

**Example (Shield Generator, base 50 mana/sec):**
- 100%: 50 mana/sec
- 150%: 75 mana/sec (+50%)
- 200%: 100 mana/sec (+100%)

**Construct Mana Regen:**
- War Golem: 80 mana/sec

**Overcharge Scenario:**
War Golem overcharges 3 modules to 150%:
- Weapon: 40 → 60 mana/sec
- Shield: 50 → 75 mana/sec
- Sensors: 20 → 30 mana/sec
- Chassis: 30 mana/sec
- **Total**: 195 mana/sec
- **Deficit**: 195 - 80 = **-115 mana/sec**
- **Time to depletion**: 2,500 mana / 115 = **21.7 seconds**

**Solution**: Use capacitor banks, reduce non-essential modules to standby, or operate in bursts.

---

### Durability Degradation

Overcharged modules degrade faster:

**Durability Loss Formula:**
```
Durability Loss = Base Loss × (Power% / 100)²
```

**Example (module takes 1,000 damage, loses 1% durability normally):**
- 100%: 1% durability loss
- 150%: 1% × 2.25 = **2.25% durability loss**
- 200%: 1% × 4.0 = **4% durability loss**

**Long-Term Impact:**
- Normal operation: Module lasts 100 combats before needing repair
- 150% overcharge: Module lasts 44 combats (56% lifespan reduction)
- 200% overcharge: Module lasts 25 combats (75% lifespan reduction)

**Solution**: Use higher quality modules (Legendary/Artifact resist degradation), install self-repair enchantments, maintain regularly.

---

## Combat Scenarios

### Scenario 1: Defensive Overcharge

**Golem "Ironclad"** faces 5 enemy knights (300 damage/sec each):

**Normal Configuration:**
- HP: 4,000
- Shields: 1,800 HP (50 mana/sec)
- Armor: 50
- Incoming damage: 1,500 DPS - 50 armor = **1,450 DPS**
- **Time to death**: (4,000 + 1,800) / 1,450 = **4 seconds**

**Defense Focus (Shields 175%, Armor 150%, Weapons 50%):**
- Shields: 1,800 × 1.65 = **2,970 HP** (75 mana/sec)
- Armor: 50 × 1.4 = **70 armor**
- Incoming damage: 1,500 - 70 = **1,430 DPS**
- **Time to death**: (4,000 + 2,970) / 1,430 = **4.87 seconds**
- **Survival increase**: +0.87 seconds (+22%)

**Defense Focus + Capacitor Burst (Shields 250%):**
- Shields: 1,800 × 2.5 = **4,500 HP** (capacitor powered, 10 sec duration)
- Armor: 70
- Incoming damage: 1,430 DPS
- **Time to death**: (4,000 + 4,500) / 1,430 = **5.94 seconds** (first 10 sec with mega-shield)
- After capacitor depletes (shields drop to 175%), still has 4,000 HP + remaining shields
- **Total survival**: ~8 seconds (+100% survival time)

---

### Scenario 2: Alpha Strike Overcharge

**Golem "Wrath"** ambushes enemy siege tower (12,000 HP, 80 armor):

**Normal Attack:**
- Weapon damage: 350
- Attack rate: 1 strike/sec
- DPS: 350 - 80 = **270 DPS**
- **Time to destroy**: 12,000 / 270 = **44.4 seconds**

**Weapons Focus (200% Overcharge, All Other Systems Disabled):**
- Weapon damage: 350 × 1.8 = **630 damage**
- DPS: 630 - 80 = **550 DPS**
- Heat: 400/strike, capacity 1,200 = **3 strikes before overheat**
- Mana: 240/strike, capacity 2,500 = **10 strikes before depleted**
- **Burst window**: 3 seconds (heat limited)
- **Burst damage**: 550 × 3 = **1,650 damage**
- After overheat, emergency cooldown (10 sec), then resume at 100% power
- **Total time**: 3 sec (burst) + 10 sec (cooldown) + 38 sec (normal) = **51 seconds**

**Weapons Focus + Capacitor + Ice Core (200% Overcharge, No Overheat):**
- Weapon damage: 630 damage
- Ice Core: +100% cooling, no overheat at 200%
- Capacitor: +200 mana/sec for 15 seconds
- Mana: 240/strike - 200 (capacitor) = **40 net mana/strike**
- **Sustained burst**: 15 seconds (capacitor duration)
- **Burst damage**: 550 × 15 = **8,250 damage**
- Tower HP after burst: 12,000 - 8,250 = **3,750 HP**
- Remaining kills in: 3,750 / 270 = **13.9 seconds**
- **Total time**: 15 + 13.9 = **28.9 seconds** (35% faster than normal)

---

### Scenario 3: Mobility Kiting

**Crystal Automaton Swarm (6 units)** vs Heavy Armored Knight (3,000 HP, 100 armor, 3 m/s):

**Normal Configuration:**
- Automaton speed: 12 m/s
- Damage: 80/strike
- Hit-and-run: Stay at 15m range, fire, retreat
- Knight cannot catch automatons (12 m/s vs 3 m/s)
- DPS per automaton: 80 - 100 armor = **0 damage** (armor too high)

**Mobility Focus (Movement 150%, Weapons 150%, Shields Disabled):**
- Speed: 12 × 1.4 = **16.8 m/s**
- Damage: 80 × 1.4 = **112 damage**
- DPS: 112 - 100 = **12 DPS per automaton**
- Total swarm DPS: 12 × 6 = **72 DPS**
- **Time to kill**: 3,000 / 72 = **41.7 seconds**

**Maximum Overcharge (Movement 200%, Weapons 200%, Everything Else Disabled):**
- Speed: 12 × 1.8 = **21.6 m/s**
- Damage: 80 × 1.8 = **144 damage**
- DPS: 144 - 100 = **44 DPS per automaton**
- Total swarm DPS: 44 × 6 = **264 DPS**
- **Time to kill**: 3,000 / 264 = **11.4 seconds**
- **Risk**: 8% burnout per minute per automaton
- **Expected burnouts**: (11.4 sec / 60 sec) × 8% × 6 automatons = **0.91 burnouts** (likely 1 automaton fails)
- **Acceptable loss**: 5 automatons survive, mission success

---

## Pilot Skill Integration

### Overcharge Mastery (Skill Tree)

**Level 1**: Basic Overcharge
- Unlock 125% overcharge (no concentration check required)

**Level 3**: Efficient Overcharge
- Reduce heat generation by 20% when overcharging
- Reduce mana consumption by 10% when overcharging

**Level 5**: Safe Overcharge
- Reduce burnout risk by 50%
- Can maintain 150% overcharge without concentration check

**Level 8**: Advanced Power Management
- Unlock automated conditional triggers
- 3 custom trigger slots

**Level 10**: Master Overcharger
- Unlock 175% overcharge
- Reduce heat generation by 40%
- Reduce burnout risk by 75%

**Level 12**: Emergency Power
- Unlock 200% overcharge
- Can redistribute power instantly (no 0.5 sec delay)

**Level 15**: Overcharge Synergist
- Unlock synergy bonuses (weapon+targeting, shield+armor, etc.)
- +25% to all synergy bonuses

**Level 18**: Capacitor Expert
- +50% capacitor charge rate
- +30% capacitor storage
- Can overcharge capacitor discharge (300% power spikes)

**Level 20**: Legendary Control
- Unlock 250% overcharge (Artifact-quality modules only)
- 0% burnout risk at 200% overcharge
- Can maintain 250% overcharge indefinitely (with sufficient mana)

---

## ECS Integration (Mind Pillar, 1 Hz)

### Components

```csharp
public struct ModulePowerState : IComponentData
{
    public Entity ParentConstruct;
    public Entity ModuleEntity;
    public float BaselinePowerDraw;      // Mana/sec at 100%
    public float CurrentPowerAllocation; // 0-250% (0 = disabled, 100 = normal, 200 = max overcharge)
    public float CurrentPowerDraw;       // Actual mana/sec consumed
    public float EffectivenessMultiplier; // 0-1.8 (calculated from CurrentPowerAllocation)
    public float HeatMultiplier;         // 1-16 (calculated from CurrentPowerAllocation²)
    public float BurnoutRisk;            // % per second
    public float TimeSinceLastBurnout;   // seconds
}

public struct PowerFocusPreset : IComponentData
{
    public Entity ConstructEntity;
    public FocusMode Mode;               // Balanced, Weapons, Defense, Mobility, Stealth, Emergency
    public FixedList64Bytes<float> ModulePowerAllocations;  // Power % for each module
}

public struct ManaCapacitor : IComponentData
{
    public Entity ParentConstruct;
    public float MaxStorage;
    public float CurrentStored;
    public float ChargeRate;             // Mana/sec
    public float DischargeRate;          // Mana/sec (burst)
    public bool IsDischargingActive;
    public Entity DischargeTargetModule; // Which module receives burst
    public float DischargeRemaining;     // Seconds left in burst
}

public struct PowerRedistributionCommand : IComponentData
{
    public Entity ConstructEntity;
    public Entity ModuleToIncrease;
    public Entity ModuleToDecrease;
    public float PowerTransferAmount;    // % to transfer
    public float ExecutionDelay;         // 0.5 sec neural link, 2 sec manual
    public bool RequiresConcentration;   // For 175%+ overcharge
    public int ConcentrationDC;          // Check difficulty
}

public struct AutomatedPowerManager : IComponentData
{
    public Entity ConstructEntity;
    public PowerManagementMode Mode;     // Aggressive, Defensive, Adaptive, Efficiency
    public FixedList64Bytes<PowerTrigger> Triggers;  // Conditional power adjustments
    public float RiskTolerance;          // 0-1 (how much burnout risk to accept)
}

public struct PowerTrigger : IComponentData
{
    public TriggerCondition Condition;   // ShieldsBelowPercent, EnemyWithinRange, HeatAbovePercent, etc.
    public float ThresholdValue;
    public Entity TargetModule;
    public float NewPowerAllocation;     // What to set module to when triggered
}

public struct OverchargeSynergy : IComponentData
{
    public Entity Module1;
    public Entity Module2;
    public SynergyType Type;             // WeaponTargeting, ShieldArmor, SensorCamo, etc.
    public float BonusMagnitude;         // Additional bonus when both overcharged
    public bool IsActive;
}
```

---

### Systems

**PowerAllocationSystem** (1 Hz):
```csharp
public partial struct PowerAllocationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var modulePower in SystemAPI.Query<RefRW<ModulePowerState>>())
        {
            float powerPercent = modulePower.ValueRO.CurrentPowerAllocation;

            // Calculate effectiveness
            float effectiveness = CalculateEffectiveness(powerPercent);
            modulePower.ValueRW.EffectivenessMultiplier = effectiveness;

            // Calculate actual power draw
            float powerDraw = modulePower.ValueRO.BaselinePowerDraw * (powerPercent / 100f);
            modulePower.ValueRW.CurrentPowerDraw = powerDraw;

            // Calculate heat multiplier
            float heatMult = (powerPercent / 100f) * (powerPercent / 100f);
            modulePower.ValueRW.HeatMultiplier = heatMult;

            // Calculate burnout risk
            float burnoutRisk = math.max(0f, (powerPercent - 100f) * 0.04f / 60f);  // % per second
            modulePower.ValueRW.BurnoutRisk = burnoutRisk;

            // Apply quality modifier to burnout risk
            var module = SystemAPI.GetComponent<ConstructModule>(modulePower.ValueRO.ModuleEntity);
            burnoutRisk *= GetBurnoutRiskModifier(module.Quality);
            modulePower.ValueRW.BurnoutRisk = burnoutRisk;
        }
    }

    private float CalculateEffectiveness(float powerPercent)
    {
        if (powerPercent <= 0f) return 0f;

        // Diminishing returns formula
        float effectiveness = 0.4f + (powerPercent * 0.008f) - (powerPercent * powerPercent * 0.00001f);
        return math.clamp(effectiveness, 0f, 1.8f);
    }

    private float GetBurnoutRiskModifier(Quality quality)
    {
        return quality switch
        {
            Quality.Crude => 2.0f,
            Quality.Standard => 1.0f,
            Quality.Masterwork => 0.6f,
            Quality.Legendary => 0.3f,
            Quality.Artifact => 0.05f,
            _ => 1.0f
        };
    }
}
```

---

**PowerBudgetSystem** (1 Hz):
```csharp
public partial struct PowerBudgetSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (powerCore, chassis) in SystemAPI.Query<RefRW<PowerCore>, RefRO<ConstructChassis>>())
        {
            float totalDraw = CalculateTotalPowerDraw(chassis.ValueRO.Entity);
            float netDeficit = totalDraw - powerCore.ValueRO.RegenerationRate;

            if (netDeficit > 0f)
            {
                // Drawing more power than regenerating
                powerCore.ValueRW.CurrentPower -= netDeficit * SystemAPI.Time.DeltaTime;

                if (powerCore.ValueRW.CurrentPower <= 0f)
                {
                    // Power depleted, emergency shutdown
                    EmergencyShutdown(chassis.ValueRO.Entity);
                }
            }
            else
            {
                // Regenerating power
                powerCore.ValueRW.CurrentPower += math.abs(netDeficit) * SystemAPI.Time.DeltaTime;
                powerCore.ValueRW.CurrentPower = math.min(powerCore.ValueRW.CurrentPower, powerCore.ValueRO.MaxCapacity);
            }
        }
    }
}
```

---

**BurnoutRiskSystem** (1 Hz):
```csharp
public partial struct BurnoutRiskSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var random = new Random((uint)System.DateTime.Now.Ticks);

        foreach (var modulePower in SystemAPI.Query<RefRW<ModulePowerState>>())
        {
            if (modulePower.ValueRO.BurnoutRisk > 0f)
            {
                float burnoutChance = modulePower.ValueRO.BurnoutRisk * SystemAPI.Time.DeltaTime;
                float roll = random.NextFloat(0f, 1f);

                if (roll < burnoutChance)
                {
                    // Burnout occurred!
                    TriggerBurnout(modulePower.ValueRW, modulePower.ValueRO.ModuleEntity);
                }
            }

            modulePower.ValueRW.TimeSinceLastBurnout += SystemAPI.Time.DeltaTime;
        }
    }

    private void TriggerBurnout(RefRW<ModulePowerState> modulePower, Entity moduleEntity)
    {
        // Disable module for 60 seconds
        modulePower.ValueRW.CurrentPowerAllocation = 0f;

        // Reduce durability
        var module = SystemAPI.GetComponent<ConstructModule>(moduleEntity);
        module.Durability -= 0.3f;

        // 10% chance of permanent damage
        var random = new Random((uint)System.DateTime.Now.Ticks);
        if (random.NextFloat() < 0.1f)
        {
            module.CurrentHP *= 0.7f;  // Permanent HP reduction
        }

        // Schedule re-enable after 60 seconds
        ScheduleModuleReactivation(moduleEntity, 60f);
    }
}
```

---

**CapacitorSystem** (1 Hz):
```csharp
public partial struct CapacitorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (capacitor, powerCore) in SystemAPI.Query<RefRW<ManaCapacitor>, RefRO<PowerCore>>())
        {
            if (capacitor.ValueRO.IsDischargingActive)
            {
                // Discharge burst
                float dischargAmount = capacitor.ValueRO.DischargeRate * SystemAPI.Time.DeltaTime;
                dischargAmount = math.min(dischargAmount, capacitor.ValueRO.CurrentStored);

                capacitor.ValueRW.CurrentStored -= dischargAmount;

                // Add discharged mana to target module
                var modulePower = SystemAPI.GetComponent<ModulePowerState>(capacitor.ValueRO.DischargeTargetModule);
                modulePower.CurrentPowerAllocation += (dischargAmount / modulePower.BaselinePowerDraw) * 100f;

                capacitor.ValueRW.DischargeRemaining -= SystemAPI.Time.DeltaTime;

                if (capacitor.ValueRW.DischargeRemaining <= 0f || capacitor.ValueRW.CurrentStored <= 0f)
                {
                    // End discharge
                    capacitor.ValueRW.IsDischargingActive = false;
                }
            }
            else
            {
                // Charge capacitor from excess power
                float excessPower = powerCore.ValueRO.RegenerationRate - CalculateTotalPowerDraw(capacitor.ValueRO.ParentConstruct);
                if (excessPower > 0f)
                {
                    float chargeAmount = capacitor.ValueRO.ChargeRate * SystemAPI.Time.DeltaTime;
                    capacitor.ValueRW.CurrentStored = math.min(
                        capacitor.ValueRW.CurrentStored + chargeAmount,
                        capacitor.ValueRO.MaxStorage
                    );
                }
            }
        }
    }
}
```

---

## Conclusion

The module overcharge and mana redistribution system enables dynamic tactical adaptation in combat. Pilots can sacrifice defensive capabilities for devastating alpha strikes, redirect power to shields during desperate last stands, or balance resource consumption for sustained operations. Mastery of power management separates novice pilots from legendary commanders, transforming constructs from static war machines into fluid instruments of destruction.
