# Strategic Bombardment System - Godgame

## Overview

The **Strategic Bombardment System** enables settlements and fortifications to launch long-range attacks against distant targets within the same region or neighboring territories. When wars escalate to total annihilation, besieging forces deploy siege engines, alchemical weapons, and catastrophic magic to inflict devastating damage ranging from tactical strikes to complete settlement destruction.

---

## Core Concepts

### 1. Bombardment Categories

**Tactical Bombardment:**
- Target: Military installations, fortifications, siege equipment
- Range: 100-500 meters
- Damage: Destroys specific structures (gates, towers, barracks)
- Purpose: Weaken defenses, create breaches, disable counter-siege weapons

**Strategic Bombardment:**
- Target: City infrastructure, population centers, supply depots
- Range: 500-2,000 meters
- Damage: Destroys city districts, kills 100-1,000 civilians
- Purpose: Demoralize population, destroy economy, force surrender

**Annihilation Bombardment:**
- Target: Entire settlement
- Range: Variable (ritual magic can target distant cities)
- Damage: Complete settlement destruction, 5,000-50,000 casualties
- Purpose: Total war, vengeance, eliminate rival permanently

**Curse/Blight (Magical Extinction):**
- Target: Land itself (crops, water, air)
- Range: Region-wide (10-100 km radius)
- Damage: Makes land uninhabitable for years/decades
- Purpose: Ensure enemy cannot rebuild, scorched earth doctrine

### 2. Tech Level Progression

**Tech Level 3-4 (Ancient/Classical):**
- Trebuchets (300m range, 90 kg payload)
- Ballistas (200m range, precision strikes)
- Catapults (150m range, area bombardment)
- Fire arrows (buildings ignited)
- Basic siege towers and rams

**Tech Level 5-6 (Medieval/Renaissance):**
- Counterweight trebuchets (500m range, 200 kg payload)
- Greek fire / alchemical incendiaries (city-wide fires)
- Plague corpses (biological warfare, disease spreads)
- Bombards and early cannons (stone walls breached)
- Siege mines (tunnels collapse walls)

**Tech Level 7-8 (Magical Renaissance):**
- Enchanted siege engines (800m range, 400 kg payload)
- Fireball volleys (mage batteries, 1 km range)
- Lightning strikes (precision, anti-personnel)
- Earthquake spells (structural collapse)
- Summoned elementals (fire/earth elementals raze cities)

**Tech Level 9-10 (High Magic):**
- Meteor swarms (5 km range, city-wide devastation)
- Planar rifts (demons/aberrations unleashed)
- Necromantic plagues (undead apocalypse)
- Divine smiting (gods intervene, total annihilation)
- Reality-warping curses (land becomes twisted wasteland)

---

## Siege Weapon Systems

### A. Trebuchets (Physical Bombardment)

**Specifications:**

| Type | Range | Payload | Accuracy | Reload Time | Crew | Cost |
|------|-------|---------|----------|-------------|------|------|
| Light Trebuchet | 150m | 45 kg | ±20m | 15 min | 4 | 500 gold |
| Medium Trebuchet | 300m | 90 kg | ±30m | 20 min | 8 | 1,200 gold |
| Heavy Trebuchet | 500m | 200 kg | ±50m | 30 min | 16 | 3,000 gold |
| Enchanted Trebuchet (TL 7+) | 800m | 400 kg | ±20m | 10 min | 12 + mage | 15,000 gold |

**Ammunition Types:**
- **Boulders**: Structural damage, breaches walls, crushes buildings (100-500 damage)
- **Fire Pots**: Incendiary, ignites wooden structures, spreads to adjacent buildings (50-200 fire damage + ongoing)
- **Plague Corpses**: Biological warfare, disease spreads to population (10-50 immediate damage, 500-2,000 casualties over 2 weeks)
- **Explosive Barrels** (TL 6+): Alchemical charges, large blast radius (200-800 damage, 50m radius)
- **Magical Projectiles** (TL 7+): Enchanted stones, homing capability, extra damage (300-1,200 damage)

**Trajectory Calculation:**
```
Projectile Arc = Launch Angle (30-60°) + Counterweight Force + Wind Compensation

Range = (Velocity² × sin(2 × Angle)) / Gravity
Accuracy Penalty = ±(Range / 10) meters

Wind Effect:
- Headwind: -10% range
- Tailwind: +10% range
- Crosswind: ±5-15m lateral deviation

Elevation Bonus:
- Firing from high ground: +20% range
- Firing uphill: -15% range
```

**Example:**
```
Heavy Trebuchet firing at city gate (450m away):
- Base range: 500m (sufficient)
- Launch angle: 45° (optimal)
- Wind: 15 km/h crosswind (±10m deviation)
- Target size: 10m wide gate
- Accuracy: ±50m (base) ±10m (wind) = ±60m spread
- Hit probability: 20% (gate is small target)
- Solution: Fire 10 shots, expect 2 direct hits

Outcome:
- 2 boulders strike gate (200 damage each = 400 total)
- Gate HP: 800 → 400 (50% damaged, weakened)
- 3 boulders strike nearby wall (150 damage each to wall)
- 5 boulders miss (land in courtyard, minor casualties)
```

### B. Ballistas (Precision Bombardment)

**Specifications:**

| Type | Range | Projectile | Accuracy | Reload Time | Crew | Cost |
|------|-------|------------|----------|-------------|------|------|
| Light Ballista | 100m | 2 kg bolt | ±2m | 30 sec | 2 | 200 gold |
| Heavy Ballista | 200m | 8 kg bolt | ±5m | 60 sec | 4 | 600 gold |
| Siege Ballista | 300m | 20 kg bolt | ±10m | 90 sec | 6 | 1,500 gold |
| Scorpion (repeating) | 150m | 1.5 kg bolt | ±3m | 10 sec | 3 | 1,000 gold |

**Ammunition Types:**
- **Standard Bolts**: Anti-personnel, penetrates armor (80-200 damage)
- **Explosive Bolts** (TL 6+): Alchemical tip, small blast (150-350 damage, 5m radius)
- **Incendiary Bolts**: Fire spread, ignites target (100 damage + 20/sec fire damage for 30 sec)
- **Chain Bolts**: Anti-siege, destroys enemy siege equipment (250-500 damage vs wooden structures)

**Targeting:**
```
Precision Strike:
- Target size modifier: Large (gate) +20%, Medium (trebuchet) +10%, Small (commander) -20%
- Crew skill bonus: Trained +10%, Veteran +20%, Elite +35%
- Range penalty: -2% per 50m beyond 100m
- Weather penalty: Rain -15%, Fog -30%, Night -25%

Example: Elite crew (130 skill) targeting enemy trebuchet (300m away, medium target):
- Base hit chance: 60%
- Range penalty: -8% (200m beyond base)
- Crew bonus: +35%
- Target size: +10%
- Total: 60 - 8 + 35 + 10 = 97% hit chance
```

### C. Magical Bombardment

**Fireball Volleys (Evocation Magic, TL 7+):**

**Mage Battery Formation:**
- 5-20 mages coordinated by battle mage commander
- Simultaneous casting for overwhelming fire
- Range: 800m (visual line of sight)
- Damage: 200-500 fire damage per fireball
- Reload: 6 seconds per mage (mana recovery)

**Procedure:**
```
Round 1:
- Commander identifies target (enemy trebuchet formation)
- 10 mages prepare Fireball (Level 5 spell, 3 sec cast)
- Mages release simultaneously
- 10 fireballs converge on 30m radius area

Damage:
- Enemy has 3 trebuchets clustered (wooden structures)
- Each fireball: 350 fire damage (8d6 average)
- Total: 3,500 damage distributed across target zone
- Trebuchet 1: Hit by 4 fireballs (1,400 damage) → DESTROYED (wood ignites)
- Trebuchet 2: Hit by 3 fireballs (1,050 damage) → DESTROYED
- Trebuchet 3: Hit by 3 fireballs (1,050 damage) → DESTROYED
- Enemy siege capacity: -3 heavy weapons (devastating tactical blow)

Mana Cost:
- 10 mages × 50 mana per fireball = 500 mana total
- Mana regeneration: 10 mana/minute per mage
- Next volley ready in: 5 minutes
```

**Meteor Swarm (TL 9, Archmage-tier):**

**Specifications:**
- Caster: Archmage (Level 18+, INT 180+)
- Range: 5 km (visual or scrying)
- Cast Time: 10 minutes (ritual)
- Mana Cost: 500 (exhausts caster)
- Cooldown: 24 hours (caster recovers)

**Damage:**
```
Meteor Swarm (9th level spell):
- 4 meteors fall in 100m radius target area
- Each meteor: 40d6 fire damage + 40d6 bludgeoning damage = avg 560 total damage per meteor
- Total damage: 2,240 damage distributed across 100m radius

Target: Enemy city district (residential area, 200 buildings, 5,000 civilians)
- Buildings (50-200 HP each): 80% destroyed (160 buildings razed)
- Civilian casualties: 3,200 dead (64%), 1,200 injured, 600 fled
- Infrastructure: Wells contaminated, roads impassable (rubble), fires spread
- Morale impact: -80% (survivors traumatized, mass exodus begins)

Strategic Effect:
- City population: 25,000 → 21,800 (3,200 dead)
- Homeless refugees: 8,000 (fled destroyed district)
- Economic damage: 500,000 gold (reconstruction cost)
- Surrender probability: +40% (city council debates capitulation)
```

**Divine Smiting (TL 10, Divine Intervention):**

**Invocation:**
- High Priest (Level 20+, WIS 190+) prays for divine intervention
- Requires: Major sacrifice (1,000 gold offering, 100 faithful prayers)
- Favor Cost: 500 Divine Favor (accumulated through worship, miracles, holy quests)
- Cast Time: 1 hour ritual (priests, choirs, holy relics)
- Success Probability: 60% base + 10% per 100 additional Favor spent

**Effects (if god answers prayer):**
```
Divine Smiting:
- God manifests wrath (pillar of holy fire, lightning storm, earthquake, etc.)
- Area: 500m radius (entire fortress, city district, or army camp)
- Damage: 100d10 radiant/divine damage = avg 5,500 damage (bypasses mortal defenses)
- Duration: 10 minutes (continuous destruction)

Against Unholy/Opposed Faction:
- Damage multiplied ×2 (11,000 avg)
- No saving throws (god's will is absolute)
- Structures consecrated to opposed deity: Permanently destroyed (cannot rebuild on cursed ground)

Example: High Priest Aldous prays for smiting of demon cult fortress
- Demon cult fortress (Unholy, opposed to Aldous's deity)
- Divine Favor: 650 (sufficient + 150 excess)
- Success roll: 60% + 15% (excess Favor) = 75%, roll 68 = SUCCESS

God's Response:
- Pillar of holy fire descends on fortress
- 11,000 radiant damage (×2 vs unholy)
- Fortress (5,000 HP): VAPORIZED
- Demon cultists (850 total): 100% casualties (divine wrath kills all unholy beings)
- Land: Consecrated to god (demon cultists cannot rebuild here for 100 years)

Strategic Outcome:
- Demon cult permanently destroyed
- Region liberated
- Aldous's deity gains +200 Worship (faithful witness miracle)
- Aldous becomes legendary (performed divine miracle)
```

---

## Damage Categories and Effects

### Tactical Damage (Military Targets)

**Damage Range:** 100-2,000 damage to specific structures

**Targets:**
- Gates (HP: 500-1,500): Breach allows infantry assault
- Towers (HP: 800-2,000): Destroys archer platforms, observation posts
- Barracks (HP: 600-1,200): Reduces enemy troop capacity
- Siege Equipment (HP: 400-1,000): Eliminates counter-siege weapons
- Walls (HP: 1,000-3,000 per 10m section): Creates breach points

**Strategic Value:**
- Enables conventional assault (infantry through breached gates)
- Reduces defender's defensive capabilities
- Minimal civilian casualties (10-50)
- Reconstruction: 5,000-50,000 gold, 2-8 weeks

**Example:**
```
Siege of Castle Ironpeak:
- Objective: Breach main gate for infantry assault
- Assets: 2 heavy trebuchets (90 kg boulders)
- Target: Main gate (1,200 HP)

Bombardment (6 hours):
- Volley 1-12: 24 boulder shots (12 per trebuchet)
- Hit rate: 35% (gate is large target, 300m range)
- Hits: 8 boulders strike gate
- Damage: 8 × 200 = 1,600 total
- Gate HP: 1,200 → 0 (DESTROYED, gate breached)

Assault:
- Infantry charges through breach (500 soldiers)
- Defenders forced to fight inside courtyard (lost gate advantage)
- Castle falls 3 hours later
- Civilian casualties: 25 (caught in crossfire)
- Infrastructure damage: 15,000 gold (gate + courtyard buildings)
```

### Strategic Damage (City Infrastructure)

**Damage Range:** 10,000-100,000 damage distributed across city districts

**Targets:**
- Residential districts (5,000-20,000 population per district)
- Markets and trade centers (economic disruption)
- Granaries and warehouses (starvation siege)
- Water supply (wells, aqueducts)
- Administrative buildings (governance collapse)

**Casualties:**
- Civilian deaths: 500-5,000
- Injuries: 1,000-10,000
- Displaced/refugees: 5,000-50,000

**Economic Impact:**
- Reconstruction: 200,000-2,000,000 gold
- Time: 6 months to 3 years
- Trade disruption: -60% to -90% for duration
- Tax revenue loss: -50% to -80%

**Morale Impact:**
- Population morale: -60% to -90%
- Surrender pressure: +30% to +70%
- Rebellion risk: +20% (population turns on leadership)

**Example:**
```
Siege of City of Thornhaven:
- City population: 40,000
- Besieging army: Kingdom of Veldara (enemy)
- Escalation: 6-month siege, city refuses surrender, attackers resort to strategic bombardment

Assets:
- 8 enchanted trebuchets (TL 7, 800m range)
- 1 mage battery (12 fire mages, fireball volleys)
- Alchemical incendiaries (Greek fire equivalent)

Bombardment Plan (3 days):
Day 1: Target market district (economic disruption)
- 6 trebuchets fire explosive barrels (50 shots over 12 hours)
- Hit rate: 40% (large district)
- 20 explosive hits across 200m radius
- Damage: 20 × 600 = 12,000 damage
- Buildings destroyed: 80 shops, 40 homes (120 structures)
- Casualties: 400 dead, 800 injured
- Economic impact: Market closed, -70% trade

Day 2: Target granaries (starvation)
- 8 trebuchets + mage battery coordinate strike
- 40 fire pots + 30 fireballs
- Granaries (stores 6 months food for 40,000): 80% destroyed
- Remaining food: 6 weeks supply
- Morale: -50% (starvation imminent)
- Surrender pressure: +40%

Day 3: Target residential district (terror)
- Mage battery unleashes full bombardment (100 fireballs over 4 hours)
- Residential district (8,000 population): 40% casualties
- Dead: 3,200 civilians
- Homes destroyed: 600 (75% of district)
- Morale: -80% (mass panic, refugees flee)
- Surrender pressure: +60% (city council debates surrender)

City Response:
- City leadership calls emergency council
- Vote: 8 for surrender, 4 for continued resistance
- Surrender: White flags raised on Day 4
- Terms: City spared further bombardment, garrison surrenders, city pays 500,000 gold reparations

Total Casualties:
- Dead: 3,600 civilians
- Injured: 2,400
- Displaced: 12,000 (fled city during bombardment)
- Reconstruction cost: 800,000 gold
- Rebuild time: 18 months
```

### Annihilation Damage (Total Settlement Destruction)

**Damage Range:** 100,000+ damage, city-wide devastation

**Methods:**
- Prolonged bombardment (weeks of continuous strikes)
- Meteor Swarm (archmage-tier magic)
- Divine Smiting (god intervenes)
- Summoned catastrophes (fire elementals raze city, earthquakes collapse all structures)
- Necromantic plague (undead rise, kill all living)

**Casualties:**
- 80-100% of population killed or displaced
- No survivors if total annihilation achieved

**Outcome:**
- City ceases to exist (ruins remain)
- Land may be uninhabitable (cursed, plagued, toxic)
- Economic impact: Total loss (millions of gold)
- Reconstruction: 10-50 years if possible, may never rebuild

**Trigger Conditions:**
- Total war (rival kingdom elimination)
- Revenge (enemy committed atrocity)
- Religious purge (heretic city must be destroyed)
- Desperation (last resort to prevent enemy victory)

**Example:**
```
Annihilation of City of Blackthorn (Total War Scenario):

Context:
- Blackthorn: Capital of rival kingdom (pop. 80,000)
- Attacker: Grand Alliance of Five Kingdoms (coalition)
- Trigger: Blackthorn's king assassinated Alliance High Priest, murdered entire diplomatic envoy (war crime)
- Alliance response: Total annihilation authorized (vengeance + ensure Blackthorn never threatens again)

Annihilation Plan:
Phase 1 (Days 1-7): Conventional bombardment
- 30 trebuchets (Alliance coalition siege train)
- 4 mage batteries (40 mages total)
- Continuous bombardment (24 hours/day)
- Targets: All districts (systematic destruction)
- Casualties: 15,000 dead, 30,000 fled (refugees)
- Remaining: 35,000 trapped inside (Alliance blocks escape routes)

Phase 2 (Day 8): Archmage Intervention
- Alliance Archmage Council convenes (5 archmages, Level 18-20)
- Ritual: Combined Meteor Swarm (5 simultaneous casts)
- Target: Entire city (5 × 100m radius = city-wide coverage)
- Cast time: 1 hour (archmages synchronize ritual)
- Mana cost: 2,500 total (500 per archmage, exhausts all)

Meteor Swarm Devastation:
- 20 meteors fall across city (5 swarms × 4 meteors each)
- Total damage: 5 × 2,240 = 11,200 damage distributed city-wide
- Buildings: 95% destroyed (city is rubble)
- Casualties: 30,000 dead (85% of remaining population)
- Survivors: 5,000 (trapped in cellars, fled during chaos)

Phase 3 (Day 9): Divine Judgment
- Alliance High Priest's successor invokes divine wrath
- God of Justice: Grants smiting (city committed war crime)
- Pillar of holy fire descends on city ruins
- Remaining structures: 100% destroyed (total annihilation)
- Remaining population: 4,500 dead (90%), 500 escape (flee into wilderness)

Final Outcome:
- City of Blackthorn: DESTROYED (ceases to exist)
- Total casualties: 49,500 dead (62% of original population)
- Refugees: 30,000 (fled to neighboring territories, become displaced persons)
- Survivors: 500 (scattered, traumatized, leaderless)
- Land: Cursed (god declared city's ground unholy, cannot rebuild for 200 years)
- Strategic result: Rival kingdom eliminated, Alliance victorious
- Political fallout: Neutral kingdoms condemn Alliance (war crime accusations), Alliance argues justified retribution
- Long-term: Blackthorn becomes legendary cautionary tale (ruins remain as memorial to war's horrors)
```

### Curse/Blight (Magical Extinction)

**Damage:** Land itself becomes uninhabitable

**Methods:**
- **Salting the Earth**: Alchemical salts spread across farmland (crops fail for 10-50 years)
- **Necromantic Plague**: Undead curse, raises all dead as undead (land becomes Death Zone)
- **Demonic Corruption**: Planar rift permanently open (demons spawn endlessly, land lost to Abyss)
- **Divine Curse**: God curses land (eternal famine, disease, storms)
- **Reality Warping**: High magic tears fabric of reality (land becomes Twisted Wastes, physics broken)

**Effects:**
```
Cursed Land:
- Agriculture: Impossible (crops wither, animals die)
- Water: Toxic or cursed (drinking causes disease/madness)
- Air: Miasma or corruption (breathing causes illness)
- Magic: Unstable or forbidden (spells backfire, magic zones chaotic)
- Settlement: Impossible (population cannot survive)

Duration:
- Minor curse: 10-50 years
- Major curse: 100-500 years
- Eternal curse: Permanent (land lost forever)

Removal:
- Requires powerful counter-ritual (archmage or high priest)
- Cost: 100,000-1,000,000 gold (cleansing rituals, holy relics)
- Time: 1-10 years (gradual purification)
- Success rate: 30-80% (some curses cannot be lifted)
```

**Example:**
```
The Blighted Wastes (Necromantic Extinction):

History:
- Formerly: Verdant Valley (pop. 150,000, 20 towns, rich farmland)
- Year 847: Necromancer Lord Malachar defeated by Alliance army
- Malachar's Final Act: Death curse ritual (10 minutes before death)
- Curse: "All who dwell in this valley shall serve me in undeath for eternity"

Curse Activation:
- Malachar dies, curse triggers
- All dead within 100 km radius rise as undead (200,000 corpses, includes cemeteries)
- Living population exposed to necromantic plague (90% mortality within 1 week)
- Undead horde: 320,000 (200k existing dead + 135k newly dead)
- Survivors: 15,000 (fled before plague killed them)

Immediate Effects:
- All 20 towns overrun by undead (no survivors)
- Farmland cursed (crops die, animals rise as undead)
- Water sources tainted (drinking causes undead transformation)
- Land becomes Death Zone (living cannot survive more than 24 hours)

Long-Term State (178 years later, present day):
- Population: 0 living (none can survive)
- Undead: 400,000+ (continues to rise from plague victims)
- Land: Blighted Wastes (gray soil, dead trees, eternal fog)
- Border: Alliance erected Wall of Wards (magical barrier, contains undead)
- Status: Permanent loss (no cleansing has succeeded, curse too powerful)

Strategic Impact:
- 100 km × 80 km territory lost (8,000 km²)
- Former food basket: Now produces nothing
- Economic loss: 50 million gold (yearly output, lost for 178 years)
- Cultural trauma: Verdant Valley erased from maps, becomes legend of "what was lost"

Cleansing Attempts:
- Attempt 1 (Year 850): 3 archmages, 20 priests, 500,000 gold → FAILED (2 archmages killed by curse backlash)
- Attempt 2 (Year 900): Divine intervention sought, God of Life sends avatar → PARTIAL SUCCESS (10 km² cleansed, curse too strong for full removal)
- Attempt 3 (Year 980): Grand ritual, 50 mages + 100 priests, 2 million gold → FAILED (curse strengthens over time, now impossible to remove)

Current Status:
- Blighted Wastes: Permanent undead territory (Alliance policy is containment, not reclamation)
- Undead leak through wards occasionally (regular patrols eliminate escapees)
- Becomes training ground for necromancer hunters (Alliance sends teams to cull undead, gain experience)
- Legendary site: Adventurers seek Malachar's lair deep in wastes (rumors of powerful artifacts, 99% die trying)
```

---

## Defense Systems

### Counter-Siege Weapons

**Defensive Ballistas:**
- Range: 200-400m (outrange enemy approach)
- Target: Enemy siege engines, sappers, siege towers
- Crew: 4-6 trained gunners
- Effectiveness: 60% hit rate vs large siege equipment, can destroy trebuchets before they fire

**Catapults (Counter-Battery Fire):**
- Range: 100-300m
- Target: Enemy siege camps, supply trains
- Ammunition: Fire pots (disrupt siege operations), boulders (destroy equipment)
- Effectiveness: 40% hit rate, forces enemy to position siege engines farther away

**Mage Countermeasures:**
- **Dispel Magic**: Intercept fireballs mid-flight (50% success rate per attempt)
- **Shield Wall Spell**: Magical barrier deflects projectiles (blocks 80% of non-magical projectiles for 10 minutes, costs 200 mana)
- **Counter-Fireball**: Mages duel (attacker's fireball vs defender's fireball, higher caster level wins)

### Structural Defenses

**Fortification Hardening:**
- **Thick Walls**: 3-5 meter thick stone (HP: 3,000-5,000 per 10m section)
- **Reinforced Gates**: Iron-banded oak (HP: 1,500-2,500)
- **Moats**: 10-20m wide, 3-5m deep (forces siege towers to build bridges, delays assault)
- **Earthen Ramparts**: Absorbs trebuchet impacts (reduces damage by 30%)

**Distributed Storage:**
- **Multiple Granaries**: 3-5 locations (destroying one doesn't cause mass starvation)
- **Hidden Water Wells**: 10-20 wells throughout city (cannot all be poisoned)
- **Decoy Structures**: False targets (enemy wastes ammunition on empty buildings)

### Evacuation and Shelters

**Underground Shelters:**
- Capacity: 10-30% of population
- Protection: Immune to bombardment (unless direct hit collapses tunnel)
- Supplies: 2-4 weeks food/water
- Cost: 100,000-500,000 gold (major infrastructure investment)

**Evacuation Protocols:**
- **Pre-Siege Evacuation**: Non-combatants sent to allied territory (reduces civilian casualties by 60-80%)
- **Underground Tunnels**: Secret escape routes (500-2,000 civilians per day can flee)
- **Parley for Civilians**: Negotiate safe passage (attacker may allow non-combatant exodus for humanitarian reasons or to demoralize defenders)

---

## Orbital Mechanics (Trajectory Calculation)

### Projectile Physics

**Basic Trajectory:**
```
Range = (Initial Velocity² × sin(2 × Launch Angle)) / Gravity

Optimal Angle:
- Flat terrain: 45° (maximum range)
- Uphill: 50-55° (compensate for elevation)
- Downhill: 35-40° (reduce angle to avoid overshooting)

Gravity: 10 m/s² (fantasy Earth-like)
Initial Velocity:
- Light trebuchet: 40 m/s
- Medium trebuchet: 50 m/s
- Heavy trebuchet: 60 m/s
- Enchanted trebuchet: 80 m/s (magical enhancement)
```

**Wind Compensation:**
```
Wind Speed Effect:
- 0-10 km/h: Negligible (±2m deviation)
- 10-20 km/h: Moderate (±5-10m deviation)
- 20-30 km/h: Significant (±15-25m deviation)
- 30+ km/h: Severe (±30-50m deviation, firing not recommended)

Crosswind Correction:
- Aim 10m upwind per 10 km/h crosswind
- Veteran crews can compensate up to 25 km/h crosswind
- Elite crews can compensate up to 35 km/h crosswind

Example:
Target: 300m away, 20 km/h crosswind from left
- Expected deviation: ±10m to the right
- Correction: Aim 20m to the left
- Result: Projectile drifts right 10m, hits intended target
```

**Elevation and Terrain:**
```
High Ground Bonus:
- +20% range (gravity assists projectile)
- +10% accuracy (better visibility, easier angle calculation)

Uphill Penalty:
- -15% range (fighting gravity)
- -5% accuracy (harder to calculate arc)

Example: Trebuchet on hill firing at valley fortress
- Base range: 500m
- High ground bonus: +20% = 600m effective range
- Accuracy bonus: +10%
- Defender's counter-fire: -15% range (firing uphill), disadvantaged
```

### Multi-Body Trajectory (Advanced, TL 8+ Magical Calculation)

For extremely long-range magical bombardment (meteor swarm, divine smiting):

```
Factors:
- Planetary rotation (target moves during projectile flight)
- Atmospheric drag (high-altitude trajectories)
- Magical interference (ley lines, anti-magic zones)
- Scrying accuracy (viewing target from afar, resolution limits)

Calculation:
- Archmage Intelligence 180+ required
- Calculation time: 10-30 minutes
- Error margin: ±50m at 5 km range
- Weather interference: ±100m additional error in storms

Example: Meteor Swarm at 5 km range
- Flight time: 20 seconds (meteors falling from high altitude)
- Planetary rotation: 5 km orbit, target moves 4m during flight
- Archmage adjusts aim: +4m eastward
- Wind (30 km/h westward): -15m drift
- Net adjustment: Aim 11m west of target
- Result: Meteors land within 50m of intended target (acceptable for 100m blast radius)
```

---

## Strategic Doctrine

### When Bombardment is Authorized

**Tactical Bombardment (Always Permitted):**
- Siege warfare (standard military operation)
- Fortification destruction (enabling assault)
- Counter-siege (defensive operations)

**Strategic Bombardment (Command Authorization Required):**
- City infrastructure targeting (civilian casualties expected)
- Requires: General or higher rank authorization
- Justification: Military necessity (force surrender, prevent prolonged siege)
- War crime risk: Moderate (civilian deaths investigated post-war)

**Annihilation Bombardment (Monarch/Council Authorization Required):**
- Total city destruction
- Requires: King, parliament, or war council unanimous vote
- Justification: Total war, enemy war crimes, existential threat
- War crime risk: High (international condemnation, trials, sanctions)

**Curse/Blight (Forbidden in Most Jurisdictions):**
- Permanent land destruction
- Requires: Desperate circumstances, no alternative
- Justification: Prevent demon invasion, undead apocalypse, planar rift (defensive only)
- War crime risk: Extreme (perpetrator may be hunted by international coalition)

### Mutually Assured Destruction (MAD)

**Deterrence:**
- If both factions have strategic bombardment capability (TL 7+ magic, large siege trains)
- Neither uses annihilation bombardment (both would be destroyed)
- Conventional warfare preferred (limited objectives, occupation, tribute)

**Breakdown of MAD:**
- Desperation (losing war, enemy approaching capital)
- Revenge (enemy committed atrocity first)
- Ideology (religious extremism, "better dead than conquered")
- Miscalculation (thought enemy wouldn't retaliate)

**Example:**
```
Scenario: Kingdom of Valen vs Empire of Korith (TL 8 civilizations, both have mage armies)

Conventional War (Years 1-3):
- Border skirmishes, fort sieges, limited tactical bombardment
- Casualties: 10,000 soldiers (both sides)
- Civilian casualties: <500 (collateral damage)
- Result: Stalemate

Escalation (Year 4):
- Korith mage battery bombards Valen border city (strategic bombardment)
- Casualties: 2,000 civilians
- Valen King: Orders retaliatory strike on Korith city
- Valen mages bombard Korith city: 3,000 civilian casualties
- Korith Emperor: Threatens meteor swarm if Valen continues
- Valen King: Threatens divine smiting in response

MAD Standoff:
- Both sides realize mutual annihilation possible
- Neutral kingdoms mediate ceasefire
- Treaty signed: No strategic bombardment, return to conventional war limits
- Result: MAD deterrence successful, war continues but limited

Alternative (MAD Failure):
- Korith Emperor orders meteor swarm despite threat
- Valen capital: 50,000 dead (archmage destroys city)
- Valen King (survived, was outside capital): Orders total retaliation
- Valen archmages: Coordinate 5 meteor swarms on Korith cities
- Korith casualties: 200,000 dead (5 cities devastated)
- Korith response: Total mobilization, divine intervention sought
- Gods intervene: Both factions' deities manifest, forbid further annihilation
- War ends: Both sides exhausted, millions dead, decades to recover
```

---

## ECS Components

### SiegeWeapon

```csharp
public struct SiegeWeapon : IComponentData
{
    public SiegeWeaponType Type;         // Trebuchet, Ballista, Catapult
    public float Range;                  // 100-800 meters
    public float PayloadWeight;          // 2-400 kg
    public float AccuracyRadius;         // ±2m to ±50m
    public float ReloadTimeSeconds;      // 10-1800 seconds
    public int CrewRequired;             // 2-16 personnel
    public bool IsEnchanted;             // TL 7+ magical enhancement
    public float MagicalRangeBonus;      // +60% range if enchanted
}

public enum SiegeWeaponType : byte
{
    LightTrebuchet,
    MediumTrebuchet,
    HeavyTrebuchet,
    EnchantedTrebuchet,
    LightBallista,
    HeavyBallista,
    SiegeBallista,
    ScorpionRepeating,
    Catapult
}
```

### BombardmentTarget

```csharp
public struct BombardmentTarget : IComponentData
{
    public Entity TargetEntity;          // Structure, settlement, or unit
    public float3 TargetPosition;        // World coordinates
    public BombardmentType Type;         // Tactical, Strategic, Annihilation
    public float ExpectedDamage;         // 100-100,000+ damage
    public float CivilianCasualties;     // 0-50,000 estimated deaths
    public bool IsAuthorized;            // Command authorization flag
    public Entity AuthorizingCommander;  // Who authorized strike
}

public enum BombardmentType : byte
{
    Tactical,        // Military targets
    Strategic,       // Infrastructure, economy
    Annihilation,    // Total destruction
    Curse            // Permanent land blight
}
```

### ProjectileTrajectory

```csharp
public struct ProjectileTrajectory : IComponentData
{
    public float3 StartPosition;         // Launch point
    public float3 TargetPosition;        // Intended impact point
    public float LaunchAngleDegrees;     // 30-60° typical
    public float InitialVelocity;        // 40-80 m/s
    public float FlightTimeSeconds;      // Calculated time to impact
    public float3 WindVelocity;          // Wind speed and direction
    public float ExpectedDeviationMeters;// ±2m to ±60m
    public bool CompensateWind;          // Veteran crew can compensate
    public float GravityCompensation;    // High ground bonus, uphill penalty
}
```

### MagicalBombardment

```csharp
public struct MagicalBombardment : IComponentData
{
    public MagicBombardmentType Type;    // Fireball, Meteor, Divine
    public int CasterLevel;              // 5-20
    public int ManaCost;                 // 50-500 mana
    public float RangeKilometers;        // 0.8-5 km
    public float BlastRadiusMeters;      // 10-100m
    public float DamagePerTarget;        // 200-5,500 damage
    public float CastTimeSeconds;        // 3-600 seconds
    public int CastersRequired;          // 1-20 (battery coordination)
    public bool RequiresDivineIntervention; // Divine smiting only
}

public enum MagicBombardmentType : byte
{
    Fireball,
    LightningStrike,
    Earthquake,
    MeteorSwarm,
    DivinePillar,
    NecroticPlague,
    SummonCatastrophe
}
```

### StrategicDamage

```csharp
public struct StrategicDamage : IComponentData
{
    public Entity TargetSettlement;      // City/fortress being bombarded
    public int BuildingsDestroyed;       // 0-5,000 structures
    public int CivilianDeaths;           // 0-50,000 casualties
    public int CivilianInjured;          // 0-100,000 injured
    public int RefugeesDisplaced;        // 0-200,000 fled
    public float EconomicDamageGold;     // 0-50 million gold
    public float MoraleReduction;        // -10% to -100%
    public float SurrenderProbability;   // +0% to +90%
    public bool TotalAnnihilation;       // City destroyed completely
    public bool LandCursed;              // Permanent blight applied
}
```

---

## Summary

The **Strategic Bombardment System** enables devastating long-range attacks in medieval/fantasy settings:

- **Siege Weapons**: Trebuchets (300-800m, 45-400 kg), Ballistas (100-300m, precision), Catapults (100-300m, area)
- **Magical Bombardment**: Fireball volleys (800m), Meteor Swarm (5 km, 2,240 damage), Divine Smiting (500m radius, 11,000 damage vs unholy)
- **Damage Categories**: Tactical (military targets, 100-2,000 damage), Strategic (infrastructure, 10,000-100,000 damage), Annihilation (total destruction, 100,000+ damage), Curse/Blight (permanent land loss)
- **Defenses**: Counter-battery fire, structural hardening, evacuation, underground shelters
- **Physics**: Trajectory calculation (wind, elevation, range), accuracy vs reload trade-offs
- **Strategy**: MAD deterrence (mutual annihilation prevents use), authorization levels (tactical always permitted, annihilation requires monarch)

This system integrates with Three Pillar ECS:
- **Body Pillar (60 Hz)**: Projectile physics, impact detection, structural damage
- **Mind Pillar (1 Hz)**: Target selection, authorization checks, civilian casualty estimates
- **Aggregate Pillar (0.2 Hz)**: Long-term strategic damage (economy, morale, surrender probability), curse duration

Bombardment creates emergent total war scenarios where desperate factions resort to annihilation, triggering MAD standoffs or catastrophic mutual destruction.
