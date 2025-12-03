# Circle Progression System

**Dependencies:** Individual Progression, Education & Tutoring System, Focus & Status Effects System, Individual Combat System, Guild System

---

## Overview

**Circle Progression** is the skill and passive ability system that allows entities to specialize in magical schools, physical disciplines, and hybrid combinations. Entities invest in **circles** (skill trees) to unlock abilities, passive bonuses, and advanced specializations.

**Key Features:**
- **Elemental Circles:** Water, Air, Earth, Fire (foundation for Elementalist)
- **Shadow & Light Circles:** Combine to unlock Twilight specializations
- **Physical Melee Circles:** Blade Mastery, Heavy Weapons, Stealth & Assassination, Warlord, Battlemaster, Monk
- **Ranged Circles:** Marksman, Hunter, Grenadier, Archery
- **Dual-School Combinations:** Air + Fire = Flame Lightning, Shadow + Light = Twilight, Shadow + Fire = Warlock
- **Triple-School Combinations:** Fire + Earth + Water = Earthblood Mage
- **Hybrid Builds:**
  - **Phys/Fin:** Monks (unarmed martial arts), Hunters (beast companions, traps)
  - **Phys/Will:** Battlemages (armored spellcasters), Grenadiers (magical explosives)
  - **Pure Phys:** Warlords (versatile weapons, high mobility), Battlemasters (buff/debuff shouts)
  - **Pure Fin:** Marksmen (long-range sniping), Assassins (stealth one-shots)
  - **Fin/Int:** Spellblades (elegant magic swordsmen), Arcane Rangers (enchanted arrows)
  - **Light/Phys:** Paladins (holy warriors, demon slayers, healers)
  - **Shadow-Frost/Phys:** Death Knights (undeath path, critical damage reduction)
  - **Arcane/Phys:** Wild Mages (chaotic magic, random effects)
  - **Shadow-Fire:** Warlocks (demonic pacts, soul manipulation, DoT specialists)
- **Racial & Cultural Inclinations:** Dwarves (hammers, earth magic), Orcs (axes, berserker rage), Humans (adaptable), Elves (elemental mastery), Dark Elves (shadow/stealth), Gnomes (scholarly magic, grenades)
- **Set Bonuses:** Completing circle progressions grants unique bonuses
- **Guild Specializations:** Guilds focus on specific schools (Fire Mages' Guild, Twilight Order, Warriors' Guild)

**For Narrative:** Elemental mages mastering multiple schools (Avatar-style), dark paladins combining Shadow + Light (Warcraft Death Knights), spellblades mixing swordplay with magic (Witcher signs), arcane archers enchanting arrows, unarmed monks (Kung Fu masters), beast-bonded hunters (ranger companions), versatile warlords swapping weapons mid-combat, holy Paladins slaying demons (Diablo), Death Knights resurrecting as Liches (WoW Arthas), chaotic Wild Mages with unpredictable power surges, Warlocks draining souls for demonic pacts (Warcraft fel magic).

---

## Circle Types

### Elemental Circles

**Core magical schools** based on classical elements. Mastering multiple elemental circles unlocks **Elementalist** status and hybrid specializations.

**Fire Circle:**
```
Theme: Destruction, heat, passion, aggression

Skills:
  - Fireball (20 Mana, 50 damage, AoE)
  - Flame Lance (15 Mana, 70 single-target damage)
  - Immolation (30 Mana, self-buff: +20% fire damage, burn aura)
  - Wall of Fire (25 Mana, zone control: blocks passage, damages enemies)

Passives:
  - Pyromancy I: +10% fire damage
  - Pyromancy II: +20% fire damage, 10% chance to ignite enemies
  - Fire Resistance I: -20% fire damage taken
  - Fire Resistance II: -40% fire damage taken, immune to burn

Set Bonus (4+ Fire skills/passives):
  - Inferno Aura: Enemies within 5m take 5 fire damage/round
  - +15% Spell Power

Stat Requirements:
  - Intelligence 40+ (spell complexity)
  - Will 50+ (channel destructive forces)

Learning Sources:
  - Fire Mages' Guild (formal training)
  - Independent practice (observing fire, experimenting)
  - Tutors (Fire Elementalist mentors)
```

**Water Circle:**
```
Theme: Healing, flow, adaptability, control

Skills:
  - Frost Bolt (15 Mana, 30 damage, slows enemy 30%)
  - Ice Barrier (20 Mana, +30 Defense, absorbs 50 damage)
  - Healing Stream (25 Mana, restore 40 HP over 5 rounds)
  - Tidal Wave (40 Mana, AoE knockback + damage)

Passives:
  - Hydromancy I: +10% water damage
  - Hydromancy II: +20% water damage, 15% chance to freeze
  - Cold Resistance I: -20% cold damage taken
  - Cold Resistance II: -40% cold damage taken, immune to slow

Set Bonus (4+ Water skills/passives):
  - Mana Tide: Restore 5 Mana per round
  - +20% Healing effectiveness

Stat Requirements:
  - Intelligence 35+
  - Will 40+
  - Perception 30+ (flow sensing)
```

**Earth Circle:**
```
Theme: Defense, endurance, stability, protection

Skills:
  - Stone Spear (18 Mana, 45 damage, armor penetration)
  - Earth Shield (25 Mana, +40% Armor for 5 rounds)
  - Tremor (30 Mana, AoE stun, knocks enemies prone)
  - Petrify (35 Mana, turn enemy to stone: immobilized, +80% Armor)

Passives:
  - Geomancy I: +10% earth damage
  - Geomancy II: +20% earth damage, 10% chance to petrify
  - Stone Skin I: +15 Armor
  - Stone Skin II: +30 Armor, -10% physical damage taken

Set Bonus (4+ Earth skills/passives):
  - Mountain Stance: +50 Health, cannot be knocked back
  - +10% Defense

Stat Requirements:
  - Will 60+ (unwavering endurance)
  - Strength 40+ (channel earth's weight)
```

**Air Circle:**
```
Theme: Speed, precision, mobility, chaos

Skills:
  - Lightning Bolt (20 Mana, 60 damage, chains to 2 targets)
  - Wind Gust (15 Mana, knockback + disarm)
  - Haste (25 Mana, +30% Attack Speed for 5 rounds)
  - Thunder Clap (35 Mana, AoE stun + deafen)

Passives:
  - Aeromancy I: +10% air/lightning damage
  - Aeromancy II: +20% air/lightning damage, 15% chance to shock
  - Swift I: +10% Movement Speed
  - Swift II: +20% Movement Speed, +5% Dodge

Set Bonus (4+ Air skills/passives):
  - Storm's Blessing: +15% Attack Speed, +10% Critical Hit Chance
  - Lightning Reflexes: First attack each combat auto-crits

Stat Requirements:
  - Finesse 60+ (precision control)
  - Intelligence 45+ (chaos calculations)
```

### Shadow & Light Circles

**Opposing forces** that, when combined, unlock **Twilight** specializations (balance, duality, forbidden power).

**Shadow Circle:**
```
Theme: Stealth, deception, fear, entropy

Skills:
  - Shadow Bolt (15 Mana, 40 damage, -20% enemy Morale)
  - Invisibility (30 Mana, +80% Stealth for 3 rounds)
  - Fear (25 Mana, enemy flees for 2 rounds)
  - Vampiric Touch (20 Mana, 35 damage, heal for 50% damage dealt)

Passives:
  - Umbramancy I: +10% shadow damage
  - Umbramancy II: +20% shadow damage, drain 5% enemy Mana on hit
  - Shadow Veil I: +20% Stealth
  - Shadow Veil II: +40% Stealth, invisible in darkness

Set Bonus (4+ Shadow skills/passives):
  - Night Stalker: +30% damage from stealth
  - Fear Aura: Enemies within 10m have -15 Morale

Stat Requirements:
  - Finesse 55+ (subtle manipulation)
  - Intelligence 50+ (deception complexity)
  - Will 40+ (resist corruption)
```

**Light Circle:**
```
Theme: Healing, protection, justice, order

Skills:
  - Holy Bolt (15 Mana, 45 damage vs Evil, 30 vs others)
  - Divine Shield (25 Mana, immune to damage for 1 round)
  - Heal (20 Mana, restore 50 HP)
  - Purify (30 Mana, remove all debuffs, cure poison/curse)

Passives:
  - Lumomancy I: +10% holy damage
  - Lumomancy II: +20% holy damage, +15% vs Undead/Evil
  - Divine Protection I: +10% all resistances
  - Divine Protection II: +20% all resistances, immune to fear

Set Bonus (4+ Light skills/passives):
  - Radiance: Heal 3 HP/round, allies within 10m gain +5 Morale
  - Smite Evil: +50% damage vs Evil alignment

Stat Requirements:
  - Will 65+ (unwavering faith)
  - Intelligence 40+
  - Alignment: Good (+40+) OR Lawful (+40+) strongly recommended
```

### Physical Circles

**Melee combat specializations** for warriors, rogues, and martial artists.

**Blade Mastery Circle:**
```
Theme: Precision strikes, finesse combat, swordplay

Skills:
  - Riposte (10 Focus, counter-attack after successful parry)
  - Whirlwind (20 Focus, attack all adjacent enemies)
  - Precision Strike (15 Focus, +30% Critical Hit Chance)
  - Bladestorm (30 Focus, 5 rapid attacks on single target)

Passives:
  - Swordsman I: +10% Attack with swords
  - Swordsman II: +20% Attack, +5% Critical Chance with swords
  - Deflection I: +5% Defense when wielding blade
  - Deflection II: +15% Defense, 20% chance to parry projectiles

Set Bonus (4+ Blade skills/passives):
  - Master Duelist: +10% all stats in 1v1 combat
  - Blade Dance: Every 3rd attack is a guaranteed critical hit

Stat Requirements:
  - Finesse 70+ (precision required)
  - Strength 50+ (blade control)
```

**Heavy Weapons Circle:**
```
Theme: Raw power, armor crushing, battlefield control

Skills:
  - Cleave (20 Focus, attack up to 5 targets)
  - Devastating Blow (25 Focus, +100% damage, ignore 50% armor)
  - War Cry (15 Focus, +20 Morale to allies, -15 to enemies)
  - Ground Slam (30 Focus, AoE knockdown)

Passives:
  - Heavy Hitter I: +15% damage with heavy weapons
  - Heavy Hitter II: +30% damage, +20% armor penetration
  - Endurance I: +20 Stamina
  - Endurance II: +40 Stamina, -30% exhaustion penalties

Set Bonus (4+ Heavy Weapon skills/passives):
  - Unstoppable: Cannot be knocked back or stunned
  - Titan's Strength: +20% Attack Damage, +30 Health

Stat Requirements:
  - Strength 80+ (heavy weapon requirement)
  - Will 60+ (endure weight)
```

**Stealth & Assassination Circle:**
```
Theme: Lethal strikes, ambush, evasion

Skills:
  - Backstab (30 Focus, ×3 damage from stealth)
  - Smoke Bomb (20 Focus, +60% Stealth, disengage)
  - Poison Blade (15 Focus, apply poison: -10 HP/round for 5 rounds)
  - Execute (35 Focus, instant kill if target HP < 20%)
  - Disguise (25 Focus, assume identity: blend into crowds, impersonate)

Passives:
  - Assassin I: +20% damage from stealth
  - Assassin II: +40% damage from stealth, +10% Critical Chance
  - Shadow Step I: +20% Movement Speed
  - Shadow Step II: +40% Movement Speed, can disengage without penalty

Set Bonus (4+ Stealth skills/passives):
  - Silent Killer: Stealth kills do not alert nearby enemies
  - Lethal Precision: Critical hits ignore 70% armor
  - One Shot Kill: First attack from stealth has 50% chance for instant kill if target HP < 40%

Stat Requirements:
  - Finesse 75+ (precision kills)
  - Intelligence 55+ (timing, planning)
```

**Warlord Circle (Pure Physique):**
```
Theme: Weapon versatility, tactical dominance, high survivability and mobility

Skills:
  - Weapon Swap (5 Focus, instantly switch between 2H/1H+Shield/Dual Wield)
  - Shield Bash (10 Focus, stun enemy 1 round, 30 damage)
  - Charge (15 Focus + 10 Stamina, burst movement + knockback attack)
  - Last Stand (40 Focus, when HP < 20%: +50% damage, immune to stun for 3 rounds)

Passives:
  - Versatile Warrior I: +10% damage with all weapon types
  - Versatile Warrior II: +20% damage, no penalties for weapon swapping
  - Iron Will I: +20 Health, +10% armor
  - Iron Will II: +40 Health, +20% armor, -20% crowd control duration

Set Bonus (4+ Warlord skills/passives):
  - Tactical Supremacy: Can hold 3 weapon loadouts, swap instantly (no Focus cost)
  - Indomitable: +30% survivability when outnumbered
  - Mobile Fortress: +30% movement speed, can sprint in heavy armor

Stat Requirements:
  - Strength 85+ (high physical power)
  - Will 65+ (mental fortitude)
```

**Battlemaster Circle (Pure Physique):**
```
Theme: Battlefield leadership, buffs/debuffs, inspiring presence

Skills:
  - Rally Cry (15 Focus, all allies +20 Morale, +15% damage for 5 rounds)
  - Demoralizing Shout (20 Focus, all enemies -15 Morale, -10% defense for 3 rounds)
  - Commanding Presence (10 Focus passive, allies within 15m gain +10% attack speed)
  - Stand Your Ground (25 Focus, all allies within 10m: immune to fear, +20 Defense)

Passives:
  - Inspiring Leader I: +10% ally damage within 15m
  - Inspiring Leader II: +20% ally damage, allies gain +15 Morale aura
  - Tactical Genius I: +15% Defense, see enemy weak points
  - Tactical Genius II: +30% Defense, allies gain +10% critical chance

Set Bonus (4+ Battlemaster skills/passives):
  - Commander's Aura: Allies within 20m gain +20% all stats
  - Unstoppable Force: +50 Health, cannot be knocked down
  - Veteran's Resilience: -30% damage taken, +20% stamina regeneration

Stat Requirements:
  - Strength 80+ (physical presence)
  - Will 75+ (command authority)
  - Intelligence 60+ (tactical awareness)
```

**Monk Circle (Physique/Finesse Hybrid):**
```
Theme: Unarmed combat, high dodge, debilitating martial arts, spiritual power

Skills:
  - Flurry of Blows (20 Focus, 5 rapid unarmed strikes: 15 damage each)
  - Pressure Point Strike (25 Focus, disable limb: -50% enemy attack/movement)
  - Deflecting Palm (15 Focus, redirect projectile back at attacker)
  - Stunning Fist (18 Focus, stun enemy 2 rounds, bypass armor)

Passives:
  - Martial Artist I: +15% unarmed damage, fist weapons, staves
  - Martial Artist II: +30% unarmed damage, +20% attack speed with monk weapons
  - Evasion I: +15% Dodge
  - Evasion II: +30% Dodge, 20% chance to avoid AoE damage entirely

Set Bonus (4+ Monk skills/passives):
  - Untouchable: +40% Dodge, enemies attacking you have -20% accuracy
  - Iron Body: +30 Armor when unarmored (monk robes/cloth only)
  - Chi Master: Unarmed attacks restore 5 Focus per hit

Stat Requirements:
  - Finesse 80+ (speed, precision)
  - Strength 70+ (striking power)
  - Will 65+ (spiritual discipline)
```

### Ranged Circles

**Ranged combat specializations for physical and hybrid builds.**

**Marksman Circle (Pure Finesse):**
```
Theme: Long-range precision, rifles/longbows, stealth sniping

Skills:
  - Headshot (30 Focus, guaranteed critical on headshot: ×3 damage)
  - Camouflage (20 Focus, +80% Stealth when stationary, invisible in foliage)
  - Trick Shot (25 Focus, arrow ricochets, hits 2 targets)
  - Sniper's Focus (35 Focus, hold breath: +100% accuracy, +50% crit, 1 perfect shot)

Passives:
  - Sharpshooter I: +15% Accuracy, +20% range
  - Sharpshooter II: +30% Accuracy, +40% range, ignore distance penalties
  - Patient Hunter I: +20% damage when stationary
  - Patient Hunter II: +40% damage when stationary, +25% critical chance

Set Bonus (4+ Marksman skills/passives):
  - Perfect Shot: First shot from stealth is guaranteed critical headshot (instant kill on non-elite)
  - Ghost Sniper: +60% Stealth, kills from 100m+ range do not reveal position
  - Long Range Master: +60% range, can target enemies 300m+ away

Stat Requirements:
  - Finesse 85+ (extreme precision)
  - Perception 75+ (target acquisition)
  - Intelligence 60+ (ballistics, wind calculation)
```

**Hunter Circle (Physique/Finesse Hybrid):**
```
Theme: Beast mastery, traps, wilderness survival, reduced kite penalty

Skills:
  - Tame Beast (30 Focus sustained, bond with animal companion: wolf, bear, hawk)
  - Lay Trap (15 Focus, place bear trap: 40 damage, immobilize 2 rounds)
  - Tracking Shot (20 Focus, mark target: +30% damage, see through stealth)
  - Coordinated Attack (25 Focus, command beast companion to attack: combined damage)

Passives:
  - Beast Bond I: Beast companion gains +20% health, +15% damage
  - Beast Bond II: Beast companion gains +40% health, +30% damage, shares Focus pool
  - Survivalist I: -20% kite penalty when moving and shooting
  - Survivalist II: -40% kite penalty, can shoot while sprinting

Set Bonus (4+ Hunter skills/passives):
  - Master Hunter: Beast companion becomes elite (×2 stats)
  - Guerrilla Warfare: Traps cost -50% Focus, can place 5 traps simultaneously
  - Predator's Grace: No kite penalty, +20% movement speed in wilderness

Stat Requirements:
  - Finesse 75+ (precision)
  - Strength 65+ (handle powerful bows, control beasts)
  - Perception 70+ (track prey)
```

**Grenadier Circle (Physique/Will Hybrid):**
```
Theme: Explosive specialist, friendly fire immunity, customizable bombs, magical payloads

Skills:
  - Frag Grenade (15 Focus, AoE 30 damage, knockback, friendly fire immune)
  - Smoke Grenade (10 Focus, obscure vision, +40% stealth for allies in cloud)
  - Incendiary Bomb (20 Focus, AoE 20 fire damage/round for 3 rounds, burning ground)
  - Arcane Payload (30 Focus + 20 Mana, customize bomb: elemental damage type, +50% AoE)

Passives:
  - Demolitionist I: +15% explosive damage, +20% AoE radius
  - Demolitionist II: +30% explosive damage, +40% AoE radius, -30% self-damage
  - Payload Master I: Unlock debilitating bombs (slow, poison, stun)
  - Payload Master II: Unlock magical payloads (fire, ice, lightning, shadow)

Set Bonus (4+ Grenadier skills/passives):
  - Friendly Explosives: Allies immune to grenade damage, gain buffs from friendly grenades
  - Bomb Artisan: Can craft custom grenades with 3 effects (damage + debuff + buff)
  - Chain Reaction: Explosions have 30% chance to trigger secondary blast (+50% damage)

Stat Requirements:
  - Strength 70+ (throw distance, handle heavy grenades)
  - Will 75+ (control magical payloads)
  - Intelligence 65+ (customize bombs, calculate blast radius)
```

**Archery Circle (Baseline Ranged):**
```
Theme: Precision shots, range control, versatile archery

Skills:
  - Multi-Shot (15 Focus, fire 3 arrows at different targets)
  - Deadeye (30 Focus, guaranteed critical hit, +50% armor penetration)
  - Piercing Arrow (20 Focus, penetrates targets, hits up to 3 enemies)
  - Volley (25 Focus, rain arrows on area: AoE damage)

Passives:
  - Marksman I: +10% Accuracy with bows
  - Marksman II: +20% Accuracy, +15% Critical Chance
  - Eagle Eye I: +20% range
  - Eagle Eye II: +40% range, ignore distance penalties

Set Bonus (4+ Archery skills/passives):
  - Rapid Fire: Every 3rd shot is instant (no attack speed delay)
  - Wind Reader: +10% Accuracy, arrows ignore wind/weather penalties

Stat Requirements:
  - Finesse 70+ (precision aiming)
  - Perception 60+ (target tracking)
```

---

## Racial & Cultural Inclinations

**Different races and cultures** have natural affinities for specific circles, learning them faster and with greater effectiveness. This creates distinct playstyles and cultural identities.

### Racial Circle Affinities

**Dwarves:**
```
Theme: Hammers, physical survivability, high damage output, craftsmanship

Circle Affinities:
  - Heavy Weapons Circle: +30% learning speed, +15% effectiveness
  - Earth Circle: +25% learning speed, +10% effectiveness
  - Warlord Circle: +20% learning speed
  - Battlemaster Circle: +20% learning speed

Stat Bonuses:
  - Strength: +10 (base 50 → 60)
  - Will: +15 (endurance, stubbornness)
  - Intelligence: -5 (practical, not scholarly)

Cultural Preferences:
  - Hammers/Axes (2H weapons): +15% damage
  - Heavy Armor: -20% movement penalty (used to weight)
  - Stone/Metal crafting: +30% quality

Guild Focus:
  - Warriors' Guild (Heavy Weapons specialization)
  - Earth Mages' Guild (geomancy, stone shaping)
  - Battlemage training (armored dwarf spellcasters)

Narrative:
  - Dwarven hold defenders wielding rune-hammers
  - Earth mages shaping mountain fortresses
  - Battlemasters leading shield walls
  - Lava Titan dwarves (Fire + Earth specialization)
```

**Orcs:**
```
Theme: Axes, physical survivability, fast damage output, berserker rage

Circle Affinities:
  - Heavy Weapons Circle: +25% learning speed
  - Warlord Circle: +30% learning speed, +15% effectiveness
  - Monk Circle: +20% learning speed (unarmed berserker style)
  - Fire Circle: +15% learning speed (destructive magic)

Stat Bonuses:
  - Strength: +15 (base 50 → 65)
  - Finesse: +10 (fast strikes)
  - Intelligence: -10 (instinct over thought)
  - Will: +5 (battle fury)

Cultural Preferences:
  - Axes (1H/2H): +20% attack speed
  - Dual Wielding: +15% damage
  - Berserker Rage: +30% damage when HP < 50%
  - Light/Medium Armor: +20% mobility

Guild Focus:
  - Warriors' Guild (Warlord/axe mastery)
  - Berserker Clans (rage techniques, fast combat)
  - Fire Shamans (destructive fire magic)

Narrative:
  - Orc warlords leading war parties with dual axes
  - Berserker monks (unarmed fury, high mobility)
  - Fire shamans burning enemy camps
  - Orcish speed > dwarven defense (cultural rivalry)
```

**Humans:**
```
Theme: Swords and shields, balanced, diplomacy, adaptability

Circle Affinities:
  - Blade Mastery Circle: +20% learning speed
  - Any Circle: +10% learning speed (adaptable)
  - Spellblade Specialization: +15% effectiveness

Stat Bonuses:
  - Diplomacy: +20 (charisma, negotiation)
  - Wisdom: +10 (experience, observation)
  - Strength: +0 (average physical)
  - Finesse: +5 (versatile)

Cultural Preferences:
  - Swords + Shields: +15% defense
  - Balanced builds (no min-maxing): +10% all stats
  - Multi-circle mastery: -20% learning time penalty (adaptable)

Guild Focus:
  - Warriors' Guild (Blade Mastery)
  - All Elemental Guilds (human adaptability)
  - Hybrid specializations (Spellblade, Arcane Ranger, Battlemage)

Narrative:
  - Human kingdoms with diverse armies (swordsmen, mages, archers)
  - Spellblades as elite knights
  - Diplomatic envoys preventing wars
  - "Jack of all trades, master of none" (but can master multiple)
```

**Elves:**
```
Theme: Earth and high magic, Will and Finesse, longevity, wisdom

Circle Affinities:
  - Earth Circle: +30% learning speed, +20% effectiveness
  - All Elemental Circles: +20% learning speed
  - Monk Circle: +25% learning speed (graceful martial arts)
  - Spellblade Circle: +20% learning speed (elegant magic swordplay)

Stat Bonuses:
  - Will: +20 (magical discipline, long lifespan)
  - Finesse: +15 (grace, precision)
  - Intelligence: +10 (scholarly)
  - Wisdom: +15 (age, experience)
  - Strength: -10 (lithe physique)

Cultural Preferences:
  - Elemental mastery: +15% Spell Power
  - Rapiers/Finesse weapons: +20% attack speed
  - Longevity: +100 years lifespan (more time for mastery)
  - Ancient forests: +30% earth/air magic effectiveness

Guild Focus:
  - Elemental Academies (multi-school masters)
  - Earthblood Conclave (triple-school mastery)
  - Spellblade Orders (magic swordsmen)
  - Monk Monasteries (martial discipline)

Narrative:
  - Elven archmages mastering triple-school combinations
  - Spellblades defending ancient forests
  - Monks achieving perfect balance (earth/air mastery)
  - Age 200+ elves as legendary masters
```

**Dark Elves (Drow):**
```
Theme: Shadow and blood magic, Phys and Finesse, stealth, assassination

Circle Affinities:
  - Shadow Circle: +35% learning speed, +20% effectiveness
  - Water Circle: +25% learning speed (blood magic specialization)
  - Stealth & Assassination Circle: +30% learning speed, +15% effectiveness
  - Shadow Blade Specialization: +25% effectiveness

Stat Bonuses:
  - Finesse: +20 (precision strikes)
  - Will: +15 (resist shadow corruption)
  - Intelligence: +10 (dark magic complexity)
  - Strength: -5 (lithe)
  - Alignment: Chaotic/Evil preferred (+20 Chaotic, +10 Evil base)

Cultural Preferences:
  - Stealth: +40% (darkvision, underground)
  - Poison: +30% potency
  - Shadow magic: +20% Spell Power
  - Daggers/Rapiers: +25% critical chance

Guild Focus:
  - Shadow Covens (shadow magic)
  - Assassin Guilds (stealth kills)
  - Blood Mage Circles (water/blood magic)
  - Death Knight training (shadow/frost warriors)

Narrative:
  - Drow assassins feared in the underdark
  - Shadow Blades teleporting through darkness
  - Blood mages draining enemies
  - Death Knights rising as liches (undeath path)
  - Surface raids (shadow magic in daylight = -20% effectiveness)
```

**Gnomes:**
```
Theme: Education and Will, scholarly magic, innovation, tinkering

Circle Affinities:
  - All Elemental Circles: +25% learning speed
  - Arcane Ranger Circle: +20% learning speed (magical engineering)
  - Grenadier Circle: +30% learning speed, +20% effectiveness (explosives!)
  - Wild Mage Circle: +15% learning speed (experimental magic)

Stat Bonuses:
  - Will: +25 (magical focus)
  - Intelligence: +20 (scholarly, inventive)
  - Wisdom: +15 (research, observation)
  - Strength: -15 (small stature)
  - Finesse: +10 (precision tinkering)

Cultural Preferences:
  - Spell complexity: +20% Spell Power
  - Lesson Quality: +15 (gnome tutors are legendary)
  - Absorption Speed: +30% (fast learners)
  - Grenade crafting: +40% effectiveness

Guild Focus:
  - Elemental Academies (research-focused)
  - Arcane Engineer Guilds (magical inventions)
  - Grenadier Schools (explosive specialists)
  - Wild Mage Societies (experimental magic)

Narrative:
  - Gnome archmages teaching at elite academies
  - Grenadier gnomes with customizable bombs
  - Wild Mages creating chaotic inventions
  - "Gnome tutors" sought by elite families (high Quality lessons)
  - Small stature = cannot use Heavy Weapons effectively
```

### Cultural Modifiers

**Beyond race**, specific cultures within races may have additional modifiers:

```
Dwarven Mountain Clans:
  - Earth Circle: +40% learning speed (total)
  - Lava Titan specialization preferred
  - Volcanic forges: +30% fire resistance

Dwarven Shield Clans:
  - Battlemaster Circle: +35% learning speed (total)
  - Heavy armor mastery
  - Commander's Aura range +10m

Orcish Berserker Tribes:
  - Monk Circle: +35% learning speed (total)
  - Berserker Rage: +50% damage when HP < 30%
  - Light armor only (cultural restriction)

Orcish Shaman Tribes:
  - Fire Circle: +30% learning speed (total)
  - Shadow Circle: +20% learning speed
  - Warlock specialization (shadow fire magic)

Human Knightly Orders:
  - Blade Mastery: +30% learning speed (total)
  - Paladin specialization (Light + Heavy Weapons)
  - Code of Honor: Lawful Good required

Human Mercenary Companies:
  - Warlord Circle: +25% learning speed
  - Any weapon type: +10% versatility
  - Pragmatic Neutral alignment preferred

Elven High Courts:
  - All Elemental Circles: +30% learning speed (total)
  - Earthblood Mage specialization (prestige)
  - Age 300+ masters revered

Elven Wood Rangers:
  - Hunter Circle: +35% learning speed
  - Beast companions: +50% bond strength
  - Marksman Circle: +25% learning speed (forest snipers)

Drow Noble Houses:
  - Shadow Blade: +40% effectiveness (total)
  - Political assassination contracts
  - Surface raids: -30% effectiveness (sunlight weakness)

Drow Underdark Raiders:
  - Stealth Circle: +45% learning speed (total)
  - Darkvision: +60% Stealth in darkness
  - Death Knight specialization (undeath warriors)

Gnome Inventor Guilds:
  - Grenadier Circle: +45% learning speed (total)
  - Custom grenades with 4 effects (instead of 3)
  - Arcane Payload: +30% AoE (total)

Gnome Arcane Universities:
  - All Elemental Circles: +35% learning speed (total)
  - Lesson Quality: +25 (total)
  - Earthblood Mage specialization (rare gnome masters)
```

### Racial Stat Requirements

Some circles have **race-adjusted stat requirements**:

```
Heavy Weapons Circle:
  - Humans/Orcs/Dwarves: Strength 80+
  - Elves/Dark Elves: Strength 90+ (less physical)
  - Gnomes: Strength 95+ (struggle with large weapons)

Monk Circle:
  - Elves: Finesse 75+ (grace bonus)
  - Humans: Finesse 80+
  - Orcs: Finesse 80+ (berserker style)
  - Dwarves: Finesse 85+ (less agile)

Shadow Circle:
  - Dark Elves: Intelligence 40+ (natural affinity)
  - Humans/Elves: Intelligence 50+
  - Dwarves/Orcs: Intelligence 60+ (unnatural magic)
  - Gnomes: Intelligence 45+ (scholarly approach)

Earth Circle:
  - Dwarves: Intelligence 30+ (instinctive)
  - Elves: Intelligence 35+ (natural)
  - Humans: Intelligence 35+
  - Gnomes: Intelligence 30+ (scholarly)
  - Orcs: Intelligence 45+ (unfamiliar magic)
```

---

## Dual-School Combinations

**Mastering 2 circles** unlocks hybrid specializations with unique skills and synergies. There are **X combinations for each 2-school pairing** (to be defined based on feasibility).

### Elemental Combinations

**Flame Lightning (Air + Fire):**
```
Requirements:
  - Air Circle: 3+ skills/passives
  - Fire Circle: 3+ skills/passives

Unlocks Specialization:
  - Flame Lightning Elementalist

Unique Skills:
  - Plasma Bolt (25 Mana, 80 damage, fire + lightning hybrid)
    → Burns AND shocks target
    → Chains to 2 targets (Air), leaves burning ground (Fire)

  - Storm of Fire (40 Mana, AoE: summon firestorm with lightning)
    → 60 fire damage + 40 lightning damage to all in area
    → 30% chance to ignite, 20% chance to shock

  - Superheated Winds (30 Mana, buff: +40% movement speed, leave fire trail)

Unique Passives:
  - Plasma Mastery: +15% fire AND lightning damage
  - Elemental Synergy: Fire spells gain 10% chance to shock, Air spells gain 10% chance to burn

Set Bonus (Flame Lightning Specialization):
  - Storm's Fury: Critical hits with fire/air spells trigger AoE explosion (30 damage)
  - Elemental Overload: Spell Power +20%

Stat Requirements:
  - Intelligence 60+
  - Finesse 55+ (control volatile combo)
  - Will 50+
```

**Earthblood Mage (Fire + Earth + Water):**
```
Requirements:
  - Fire Circle: 2+ skills/passives
  - Earth Circle: 2+ skills/passives
  - Water Circle: 2+ skills/passives

Unlocks Specialization:
  - Earthblood Elementalist (triple-school master)

Theme: Life force manipulation, volcanic power, primordial magic

Unique Skills:
  - Magma Lance (35 Mana, 90 damage, melts armor)
    → Fire + Earth fusion: ignores 60% armor, leaves molten ground

  - Blood Boil (30 Mana, enemy takes 15 damage/round, cannot heal)
    → Water + Fire fusion: internal damage, unstoppable

  - Earthblood Surge (50 Mana, AoE heal 40 HP + grant Stone Skin buff)
    → Earth + Water fusion: defensive/healing hybrid

Unique Passives:
  - Primordial Mastery: +10% fire/earth/water damage
  - Earthblood Reservoir: +50 Mana Pool, +30 Health
  - Elemental Convergence: Each elemental spell cast increases next spell's power by 5% (stacks 3×)

Set Bonus (Earthblood Specialization):
  - Volcanic Resilience: +40 Armor, immune to burn/freeze/petrify
  - Elemental Supremacy: +30% Spell Power, +15% all resistances

Stat Requirements:
  - Intelligence 70+ (complex triple-school mastery)
  - Will 75+ (channel primordial forces)
  - Strength 50+ (endure elemental strain)
```

**Ice Storm (Air + Water):**
```
Requirements:
  - Air Circle: 3+ skills/passives
  - Water Circle: 3+ skills/passives

Unlocks Specialization:
  - Ice Storm Elementalist

Unique Skills:
  - Blizzard (35 Mana, AoE: freeze + slow all enemies, obscure vision)
  - Frozen Lightning (25 Mana, 70 damage, freeze + shock)
  - Glacial Prison (30 Mana, trap enemy in ice: immobilized, +90% Armor to target)

Unique Passives:
  - Cryomancy Mastery: +15% cold/lightning damage
  - Winter's Wrath: Cold spells slow enemies by additional 15%

Set Bonus (Ice Storm Specialization):
  - Permafrost: Frozen enemies take +30% damage from all sources
  - Storm Caller: +25% Spell Power during storms/winter
```

**Lava Titan (Fire + Earth):**
```
Requirements:
  - Fire Circle: 3+ skills/passives
  - Earth Circle: 3+ skills/passives

Unlocks Specialization:
  - Lava Titan Elementalist

Theme: Volcanic destruction, molten armor, siege power

Unique Skills:
  - Lava Flow (40 Mana, create impassable lava zone: 20 damage/round)
  - Molten Armor (30 Mana, +50% Armor, melee attackers take 15 fire damage)
  - Volcanic Eruption (50 Mana, massive AoE: 100 damage, knockback, ignite)

Unique Passives:
  - Molten Core: +20 Armor, +15% fire damage
  - Volcanic Resilience: Immune to fire, -50% cold damage taken

Set Bonus (Lava Titan Specialization):
  - Unstoppable Inferno: Cannot be slowed/rooted, leave burning trail
  - Magma Wrath: +40% damage vs structures/armor
```

### Shadow & Light Combinations

**Twilight (Shadow + Light):**
```
Requirements:
  - Shadow Circle: 3+ skills/passives
  - Light Circle: 3+ skills/passives

Unlocks Specialization:
  - Twilight Adept (forbidden magic, balance, duality)

Theme: Forbidden power, life/death manipulation, moral ambiguity

Unique Skills:
  - Twilight Bolt (20 Mana, 60 damage, heal self for 30% damage dealt, damage enemy Morale -15)
    → Shadow + Light fusion: drain AND smite

  - Duality Shift (25 Mana, toggle between Shadow Form / Light Form)
    → Shadow Form: +40% Stealth, +20% shadow damage, -20% holy damage
    → Light Form: +20% holy damage, +15% all resistances, -20% shadow damage

  - Eclipse (50 Mana, massive AoE: 80 damage to all, heal allies 40 HP)
    → Ultimate duality spell: destroy AND heal simultaneously

Unique Passives:
  - Twilight Mastery: +10% shadow AND holy damage
  - Balance of Power: When HP < 50%, gain +20% Spell Power
  - Moral Ambiguity: No alignment penalties for using Shadow OR Light spells

Set Bonus (Twilight Specialization):
  - Duality's Gift: Casting shadow spell grants +10% holy damage (stacks 3×), vice versa
  - Forbidden Power: +30% Spell Power, +20 Mana Pool
  - Eternal Balance: Immune to fear AND charm

Stat Requirements:
  - Intelligence 65+
  - Will 70+ (resist corruption from duality)
  - Wisdom 50+ (understand balance)

Alignment Interaction:
  - Lawful Good: Forbidden (requires dispensation from deity/mentor)
  - Chaotic Evil: Forbidden (Light Circle rejects them)
  - True Neutral: Ideal (balance philosophy)
  - Lawful Neutral / Chaotic Good: Acceptable with justification
```

### Physical-Magical Hybrid Combinations

**Spellblade (Blade Mastery + Any Elemental):**
```
Requirements:
  - Blade Mastery Circle: 3+ skills/passives
  - Any Elemental Circle: 2+ skills/passives

Unlocks Specialization:
  - Spellblade (weapon-channeling magic)

Theme: Enchanted strikes, weapon magic, combat mage

Unique Skills:
  - Elemental Blade (20 Mana + 10 Focus, enchant weapon: +30 elemental damage for 5 rounds)
  - Arcane Slash (25 Mana + 15 Focus, melee attack that releases magic wave: 50 damage cone)
  - Spell Parry (15 Mana, deflect incoming spell back at caster)

Unique Passives:
  - Combat Mage I: Casting spells does not interrupt melee attacks
  - Combat Mage II: +15% spell damage while wielding weapon
  - Arcane Warrior: +10% Defense, +10% Spell Power

Set Bonus (Spellblade Specialization):
  - Blade & Magic: Every 3rd melee attack triggers free spell (no Mana cost)
  - Enchanted Strikes: +20% Attack Damage, +15% Spell Power

Stat Requirements:
  - Finesse 65+
  - Intelligence 60+
  - Strength 50+

Example Build (Fire Spellblade):
  - Fire Circle: Fireball, Immolation, Pyromancy I/II
  - Blade Mastery: Riposte, Whirlwind, Swordsman I/II
  - Spellblade Skills: Elemental Blade (fire), Arcane Slash (fire wave)
```

**Arcane Ranger (Archery + Any Elemental):**
```
Requirements:
  - Archery Circle: 3+ skills/passives
  - Any Elemental Circle: 2+ skills/passives

Unlocks Specialization:
  - Arcane Ranger (magical archery)

Theme: Enchanted arrows, elemental shots, precision magic

Unique Skills:
  - Elemental Arrow (15 Mana + 10 Focus, arrow deals +40 elemental damage)
  - Arcane Volley (30 Mana + 20 Focus, rain enchanted arrows: AoE magic damage)
  - Seeking Shot (25 Mana + 15 Focus, arrow auto-tracks target, cannot miss)

Unique Passives:
  - Magic Archer I: Arrows gain +10% elemental damage
  - Magic Archer II: 20% chance arrows apply elemental effect (burn/freeze/shock)
  - Ranger's Focus: +20% Accuracy when casting spells

Set Bonus (Arcane Ranger Specialization):
  - Enchanted Quiver: Every 5th arrow is free (no Focus cost), deals ×2 damage
  - Elemental Precision: +15% Critical Chance, +20% Spell Power

Stat Requirements:
  - Finesse 70+
  - Intelligence 55+
  - Perception 60+

Example Build (Lightning Ranger):
  - Air Circle: Lightning Bolt, Haste, Aeromancy I/II
  - Archery: Multi-Shot, Deadeye, Marksman I/II
  - Arcane Ranger Skills: Elemental Arrow (lightning), Seeking Shot
```

**Battlemage (Heavy Weapons + Any Elemental):**
```
Requirements:
  - Heavy Weapons Circle: 3+ skills/passives
  - Any Elemental Circle: 2+ skills/passives

Unlocks Specialization:
  - Battlemage (armored spellcaster, frontline mage)

Theme: Tanky mage, spell-enhanced power attacks, magical battlefield control

Unique Skills:
  - Thunderous Smash (25 Mana + 20 Focus, heavy attack + AoE spell blast)
  - War Magic (30 Mana, buff: melee attacks trigger elemental explosions for 5 rounds)
  - Arcane Bulwark (20 Mana, +40 Armor, reflect 20% spell damage)

Unique Passives:
  - Battle Caster I: Can cast spells while wearing heavy armor (no penalty)
  - Battle Caster II: +15% Spell Power, +10 Armor
  - War Mage: +20% Attack Damage, +20% Mana Pool

Set Bonus (Battlemage Specialization):
  - Frontline Sorcerer: +30 Health, +25% Spell Power in melee range
  - Arcane Juggernaut: +20 Armor, immune to silence/interrupt

Stat Requirements:
  - Strength 75+
  - Intelligence 60+
  - Will 65+

Example Build (Earth Battlemage):
  - Earth Circle: Stone Spear, Earth Shield, Geomancy I/II, Stone Skin I/II
  - Heavy Weapons: Cleave, Devastating Blow, Heavy Hitter I/II
  - Battlemage Skills: Thunderous Smash (earth), Arcane Bulwark
```

**Shadow Blade (Stealth & Assassination + Shadow):**
```
Requirements:
  - Stealth & Assassination Circle: 3+ skills/passives
  - Shadow Circle: 2+ skills/passives

Unlocks Specialization:
  - Shadow Blade (magical assassin)

Theme: Shadow magic + assassination, fear tactics, nightstalker

Unique Skills:
  - Umbral Strike (20 Mana + 25 Focus, teleport to target + backstab)
  - Shadow Clone (35 Mana, create illusion that mimics attacks: 50% damage)
  - Nightmare (25 Mana, curse enemy: -30 Morale, hallucinate, attack allies)

Unique Passives:
  - Shadowmancer Assassin I: +15% damage from stealth (stacks with Assassin passives)
  - Shadowmancer Assassin II: Stealth kills restore 20 Mana
  - Void Walker: +40% Stealth, can teleport short distances (5m) while in shadow

Set Bonus (Shadow Blade Specialization):
  - Death from Darkness: First attack from invisibility is instant-kill if target HP < 30%
  - Shadow's Embrace: +50% Stealth, +25% shadow damage

Stat Requirements:
  - Finesse 80+
  - Intelligence 60+
  - Will 50+
```

**Paladin/Templar (Light + Physical):**
```
Requirements:
  - Light Circle: 3+ skills/passives
  - Any Physical Melee Circle: 3+ skills/passives (Heavy Weapons or Warlord)

Unlocks Specialization:
  - Paladin/Templar (holy warrior, demon slayer)

Theme: Anti-magic, anti-demon, high survivability, healer/melee hybrid

Unique Skills:
  - Smite Demon (25 Mana + 15 Focus, ×3 damage vs Undead/Demons, stun 1 round)
  - Holy Armor (30 Mana, +50 Armor, immune to curses/poison for 5 rounds)
  - Lay on Hands (20 Mana, heal self or ally 60 HP, remove debuffs)
  - Consecrate Ground (40 Mana, create 10m holy zone: allies +20% all stats, enemies -20% all stats)

Unique Passives:
  - Holy Warrior I: +15% damage vs Undead/Demons/Evil alignment
  - Holy Warrior II: +30% damage vs Undead/Demons, +20% all resistances
  - Divine Protection: Immune to fear, charm, possession
  - Anti-Magic Aura: +30% spell resistance, allies within 10m gain +15% spell resistance

Set Bonus (Paladin/Templar Specialization):
  - Bulwark of Light: +60 Health, +30 Armor, immune to instant-kill effects
  - Demon Bane: ×2 damage vs Demons, Undead flee when killed (fear aura)
  - Healing Aura: Heal self and allies 5 HP/round within 10m

Stat Requirements:
  - Strength 75+
  - Will 80+ (channel divine power)
  - Alignment: Good (+60+) OR Lawful (+60+) required
```

**Death Knight (Shadow/Water + Physical):**
```
Requirements:
  - Shadow Circle: 2+ skills/passives
  - Water Circle: 2+ skills/passives (for frost/blood magic)
  - Any Physical Melee Circle: 3+ skills/passives

Unlocks Specialization:
  - Death Knight (shadow/frost warrior, undeath path)

Theme: Shadow/frost/physical damage, critical damage reduction, may become undead

Unique Skills:
  - Death Strike (20 Mana + 20 Focus, melee attack heals for 50% damage dealt)
  - Frost Aura (25 Mana sustained, enemies within 10m slowed 30%, take 10 frost damage/round)
  - Blood Boil (30 Mana, AoE: 40 shadow damage, lifesteal 30%)
  - Army of the Dead (50 Mana + 40 Focus sustained, raise 3 undead minions for 5 rounds)

Unique Passives:
  - Unholy Strength I: +15% physical damage, +10% shadow/frost damage
  - Unholy Strength II: +30% physical damage, +20% shadow/frost damage
  - Critical Damage Reduction: Reduce critical hit damage taken by 50%
  - Death's Grasp: When HP drops to 0, become Undead (once per combat, 30% max HP)

Set Bonus (Death Knight Specialization):
  - Lich Form: When killed, resurrect as Lich (permanent undead transformation, +40% spell power, lose 50% max HP)
  - Frost Blade: Melee attacks apply frost slow (30% for 2 rounds)
  - Blood Shield: Taking damage grants blood shield (absorbs 20% of damage taken for 3 rounds)

Stat Requirements:
  - Strength 80+
  - Will 70+ (control undeath)
  - Intelligence 60+ (frost/shadow magic)
  - Alignment: Evil (+40+) OR Chaotic (+40+) recommended
```

**Wild Mage (Arcane + Physical):**
```
Requirements:
  - Any Elemental Circle: 2+ skills/passives (represents "arcane" magic)
  - Any Physical Melee Circle: 3+ skills/passives

Unlocks Specialization:
  - Wild Mage (chaotic magic melee, unpredictable power)

Theme: Random magic effects, melee combat, chance for bonus effects on hit

Unique Skills:
  - Chaos Strike (15 Mana + 15 Focus, melee attack + random elemental effect: fire/ice/lightning/shadow)
  - Wild Surge (30 Mana, roll d100: random powerful effect or backlash)
    → 1-20: Heal 80 HP
    → 21-40: AoE 60 damage random element
    → 41-60: +50% all stats for 3 rounds
    → 61-80: Teleport random location within 50m
    → 81-100: Backlash: take 40 damage, lose 20 Mana
  - Arcane Infusion (25 Mana, enchant weapon: every hit has 50% chance for bonus magic damage)
  - Polymorph (40 Mana, transform enemy into harmless creature for 2 rounds)

Unique Passives:
  - Chaotic Magic I: 15% chance melee hits trigger random spell (free, no Mana cost)
  - Chaotic Magic II: 30% chance melee hits trigger random spell, spells cost -20% Mana
  - Wild Luck: Critical hits have 25% chance to refund Mana cost of last spell
  - Arcane Resilience: +20% all resistances, immune to silence

Set Bonus (Wild Mage Specialization):
  - Chaos Mastery: Random effects are 50% more powerful, backlash effects reduced 50%
  - Arcane Overload: Every 5th spell is free and guaranteed powerful effect (no backlash)
  - Unstable Magic: Enemies attacking you have 20% chance to trigger wild magic backlash on themselves

Stat Requirements:
  - Strength 70+
  - Intelligence 75+ (control chaos)
  - Will 65+ (resist backlash)
  - Wisdom 50+ (understand unpredictability)
```

### Dual-Elemental Combinations

**Warlock (Shadow + Fire):**
```
Requirements:
  - Shadow Circle: 3+ skills/passives
  - Fire Circle: 3+ skills/passives

Unlocks Specialization:
  - Warlock (demonic magic, corruption, soul manipulation)

Theme: Shadow fire magic, corruption, demonic pacts, DoT specialist

Unique Skills:
  - Shadowflame (25 Mana, 60 fire/shadow hybrid damage, burns AND curses: -10 HP/round for 5 rounds)
  - Fel Corruption (30 Mana, curse enemy: -20% all stats, spreads to nearby enemies if killed)
  - Soul Drain (20 Mana, drain enemy soul: 40 damage, restore 30 Mana, reduce enemy max HP by 10 permanently)
  - Summon Demon (50 Mana + 30 Focus sustained, summon demon minion: elite stats, costs Focus to maintain)

Unique Passives:
  - Demonic Power I: +15% shadow/fire damage
  - Demonic Power II: +30% shadow/fire damage, DoT effects last +50% longer
  - Soul Harvest: Killing enemies restores 15 Mana
  - Corruption Aura: Enemies within 10m take 5 shadow damage/round, debuffs last +30% longer

Set Bonus (Warlock Specialization):
  - Demonic Pact: +50 Mana Pool, +30% Spell Power, permanently cursed (cannot be healed by Light magic)
  - Shadowflame Mastery: Shadow/fire spells have 30% chance to not consume Mana
  - Soul Reaper: Killing blows permanently increase Spell Power by +1 (stacks infinitely, lost on death)

Stat Requirements:
  - Intelligence 75+
  - Will 80+ (resist demonic corruption)
  - Alignment: Evil (+50+) OR Chaotic (+50+) strongly recommended
```

---

## Learning & Progression

### Circle Acquisition

**How entities learn circles:**

**1. Guild Training (Primary Source):**
```
Entities join specialized guilds to learn circle skills/passives:
  - Fire Mages' Guild: Fire Circle
  - Thieves' Guild: Stealth & Assassination Circle
  - Warriors' Guild: Blade Mastery, Heavy Weapons
  - Twilight Order: Shadow + Light (rare, secretive)

Guild Training:
  - Teachers provide Lessons (Quality, Rarity, Tier, Affixes)
  - Lessons must be absorbed/refined (see Education System)
  - Guild membership grants access to higher-tier skills
  - Guild libraries contain rare techniques

Example:
  Fire Mages' Guild offers:
    - Tier 1-3: Apprentice (Fireball, Pyromancy I)
    - Tier 4-6: Journeyman (Immolation, Pyromancy II)
    - Tier 7-10: Master (Wall of Fire, Fire Resistance II)
```

**2. Independent Learning (Secondary Source):**
```
Entities can discover circle skills through:
  - Experimentation (Intelligence + Wisdom checks)
  - Observation (watching others cast spells, use skills)
  - Self-refinement (practicing existing skills to unlock new ones)

Independent Learning Chances:
  BaseChance = (Intelligence + Wisdom) / 10

  Example:
    Intelligence 70, Wisdom 60
    → (70 + 60) / 10 = 13% chance per 30 days practice
    → Self-discover Tier 1-2 skills only
    → Higher tiers require teaching or rare insights

Self-Made Lessons:
  - Quality: (Intelligence + Wisdom) / 2 (avg 50-70)
  - Rarity: Common (90%), Uncommon (10%), Rare (1%)
  - Affixes: 5% chance (if Intelligence + Wisdom > 150)

Observation-Based Learning:
  Observing Combat/Magic:
    BaseChance = (Wisdom / 5) + (Perception / 10)

    Example:
      Watching Fire Mage cast Fireball 10 times
      → Wisdom 60, Perception 50
      → (60 / 5) + (50 / 10) = 17% chance to learn
      → Learn Tier 1 Fireball (Quality 40-60, Common)
```

**3. Tutoring (Elite Source):**
```
Wealthy entities hire private tutors (see Education System):
  - Tutors teach high-Quality, high-Rarity lessons
  - Faster absorption (tutor guides refinement)
  - Access to rare/unique skills not taught in guilds

Example:
  Elite heir hires Master Fire Elementalist tutor
  → Learns Legendary Tier 5 Immolation lesson (Quality 85, Epic rarity)
  → Absorption time: 30 days (Intelligence 80, Wisdom 70)
  → Refinement: 60 days to 100% effectiveness
```

### Circle Progression

**Unlocking skills and passives:**

```
Skill/Passive Acquisition:
  1. Learn lesson (guild/tutor/independent)
  2. Absorb lesson (time-based, Intelligence + Wisdom)
  3. Refine lesson (practice, 50% → 100% effectiveness)
  4. Unlock skill/passive for use

Circle Advancement:
  Tier 1 (Novice): 0-2 skills/passives
  Tier 2 (Apprentice): 3-4 skills/passives
  Tier 3 (Journeyman): 5-6 skills/passives
  Tier 4 (Adept): 7-8 skills/passives
  Tier 5 (Master): 9+ skills/passives

Dual-School Unlocks:
  Requires:
    - Circle A: 3+ skills/passives (Journeyman+)
    - Circle B: 3+ skills/passives (Journeyman+)
    - Combined Intelligence + Will > 120 (complex mastery)

  Example:
    Fire Circle: Fireball, Immolation, Pyromancy I, Pyromancy II (4 total)
    Air Circle: Lightning Bolt, Haste, Aeromancy I, Swift I (4 total)
    → Unlocks Flame Lightning specialization
    → Can now learn Plasma Bolt, Storm of Fire, etc.

Triple-School Unlocks:
  Requires:
    - Circle A: 2+ skills/passives
    - Circle B: 2+ skills/passives
    - Circle C: 2+ skills/passives
    - Combined Intelligence + Will > 140 (mastery rare)
    - Age 30+ (experience required) OR prodigy trait

  Example:
    Fire Circle: Fireball, Pyromancy I (2 total)
    Earth Circle: Stone Spear, Geomancy I (2 total)
    Water Circle: Frost Bolt, Hydromancy I (2 total)
    → Unlocks Earthblood Mage specialization (rare!)
```

---

## Focus Integration

**Circle skills consume Focus** (see Focus & Status Effects System).

**Focus Allocation for Circle Skills:**
```
Spell Casting:
  - Most spells cost Mana (resource pool)
  - Complex spells require Focus allocation (attention)
  - Multi-casting requires splitting Focus

Example (Fire Mage multi-casting):
  FocusMax: 70
  Casting Fireball (20 Mana) + Immolation buff (30 Mana, sustained)

  Focus Allocation:
    - Fireball: 10 Focus (quick cast)
    - Immolation: 25 Focus (sustained buff, ongoing attention)
    - Remaining: 35 Focus (available for dodging, awareness)

Physical Skills:
  - Consume Focus (attention/precision)
  - Stamina (physical exertion)

Example (Warrior cleaving):
  FocusMax: 60
  Cleaving Strike (20 Focus per additional target)

  Focus Allocation:
    - Primary target: 0 Focus (natural attack)
    - Secondary target: 20 Focus
    - Tertiary target: 20 Focus
    - Remaining: 20 Focus (defensive awareness)

Hybrid Builds (Spellblade):
  - Must balance Mana, Focus, AND Stamina
  - High complexity, high reward

Example (Spellblade in combat):
  FocusMax: 65, Mana: 80, Stamina: 8 rounds

  Turn 1:
    - Cast Elemental Blade (20 Mana, 10 Focus: enchant weapon)
    - Melee attack (5 Stamina, 15 Focus)
    - Remaining: 40 Focus (defensive)

  Turn 2:
    - Arcane Slash (25 Mana, 15 Focus: melee + magic wave)
    - Remaining: 50 Focus (Elemental Blade ended, freed 10 Focus)
```

**Sustained Abilities:**
```
Some skills require continuous Focus allocation:
  - Immolation (Fire buff): 25 Focus/round
  - Raise Minion (Necromancer): 20 Focus/minion
  - Arcane Bulwark (Battlemage shield): 20 Focus/round

Dropping Sustained Abilities:
  - Focus instantly freed for reallocation
  - Allows dynamic mid-combat adaptation

Example:
  Battlemage maintaining Arcane Bulwark (20 Focus)
  → Boss appears, requires full Focus on offense
  → Drops Arcane Bulwark (shield ends)
  → 20 Focus instantly reallocated to Thunderous Smash
```

---

## Guild Specializations

**Guilds focus on specific circles** (see Guild System).

### School-Specific Guilds

**Fire Mages' Guild:**
```
Focus: Fire Circle mastery
Membership Requirements:
  - Intelligence 50+
  - Fire Circle: 2+ skills/passives OR strong desire to learn

Services:
  - Fire Circle training (Tier 1-10)
  - Flame Lightning specialization (if Air Circle also mastered)
  - Lava Titan specialization (if Earth Circle also mastered)
  - Research: New fire spells, enchanted fire weapons
  - Contracts: Burn buildings, battlefield support, forge work

Guild Structure:
  - Guild Master: Fire Master (9+ Fire skills, legendary)
  - Inner Circle: Fire Adepts (7+ Fire skills)
  - Journeymen: Fire practitioners (4-6 Fire skills)
  - Apprentices: Learning Fire basics (1-3 Fire skills)

Example Member Progression:
  Join at age 18 (Intelligence 55, no Fire skills)
  → Apprentice: Learn Fireball, Pyromancy I (2 years)
  → Journeyman: Learn Immolation, Flame Lance, Pyromancy II (5 years)
  → Adept: Learn Wall of Fire, Fire Resistance I/II (10 years)
  → Inner Circle: Age 33, Fire Master status
```

**Twilight Order (Shadow + Light):**
```
Focus: Forbidden duality magic
Membership Requirements:
  - Intelligence 60+
  - Will 65+ (resist corruption)
  - Shadow Circle: 2+ skills/passives
  - Light Circle: 2+ skills/passives
  - Alignment: True Neutral preferred, Lawful/Chaotic Neutral acceptable
  - Secretive recruitment (invitation only)

Services:
  - Twilight specialization training
  - Duality philosophy teaching
  - Moral ambiguity counseling (prevent corruption)
  - Contracts: Balance enforcement, curse removal, forbidden rites

Guild Structure:
  - Grand Arbiter: Twilight Master (9+ Twilight skills, ageless)
  - Council of Duality: 5 Twilight Adepts (balance keepers)
  - Initiates: Shadow + Light practitioners (learning balance)

Secrecy:
  - Headquarter locations hidden
  - Members use code names
  - Public suspicion (Light clergy condemn, Shadow cults distrust)
  - Player deity intervention may expose or protect them

Example Member Progression:
  Join at age 25 (already Shadow Adept + Light Novice)
  → Learn Twilight Bolt, Duality Shift (3 years secret training)
  → Master Duality Shift forms (5 years practice)
  → Learn Eclipse (10 years mastery, rare)
  → Council member at age 45 (if survive suspicion/purges)
```

**Warriors' Guild (Physical Circles):**
```
Focus: All martial disciplines (Blade, Heavy Weapons, Stealth)
Membership Requirements:
  - Finesse 50+ OR Strength 50+
  - Any Physical Circle: 1+ skill/passive

Services:
  - Martial training (all physical circles)
  - Spellblade/Battlemage hybrid training (if entity also mage)
  - Contracts: Bodyguard, champion duels, mercenary work
  - Tournaments: Reputation, prize money, recruitment

Guild Structure:
  - Champion Master: Multi-circle master (legendary warrior)
  - Weapon Masters: Specialists (Blade Master, Heavy Weapon Master, Stealth Master)
  - Veterans: Experienced fighters (5+ skills)
  - Recruits: Training warriors (1-3 skills)
```

### Outlook-Based Guilds

**Thieves' Guild:**
```
Focus: Stealth & Assassination Circle + criminal enterprises
Membership Requirements:
  - Finesse 55+
  - Craven OR Pragmatic outlook (Bold warriors rejected)
  - Stealth: 1+ skill/passive OR strong aptitude

Services:
  - Stealth training, lock-picking, poison craft
  - Shadow Blade specialization (if Shadow Circle learned)
  - Contracts: Theft, assassination, espionage, smuggling
  - Fence: Sell stolen goods, acquire illegal items

Guild Structure:
  - Guildmaster: Shadow Blade Master (stealth + magic)
  - Master Thieves: Elite operatives
  - Cutthroats: Assassins for hire
  - Pickpockets: Novice thieves

Alignment Interaction:
  - Lawful Good: Will not join (illegal)
  - Chaotic Good: May join (Robin Hood justification)
  - Evil: Preferred (no moral qualms)
  - Neutral: Pragmatic membership
```

**Elite Families' Private Academies:**
```
Focus: Any circles, exclusive to elite heirs
Membership Requirements:
  - Wealth Stratum: High or Ultra-High
  - Family endorsement
  - Tuition: 5000-20,000 currency/year

Services:
  - Multi-circle training (Fire, Water, Earth, Air, Physical, Ranged)
  - Legendary tutors (Quality 80-100 lessons)
  - Rare specializations (Earthblood Mage, Twilight, etc.)
  - Political connections, elite networking

Example:
  Duke's heir attends Royal Academy
  → Learns Fire Circle (Tier 1-5) from Grand Fire Mage tutor
  → Learns Blade Mastery (Tier 1-7) from Champion duelist
  → Age 18-25: Becomes Fire Spellblade (elite warrior-mage)
  → Graduates with 12 skills/passives, Quality 80+ lessons
  → Joins family's Elite Court as Champion
```

---

## Set Bonuses

**Completing circle progressions** grants powerful set bonuses.

### Single-Circle Set Bonuses

```
Requirement: 4+ skills/passives from same circle

Fire Circle Set Bonus:
  - Inferno Aura: Enemies within 5m take 5 fire damage/round
  - +15% Spell Power

Water Circle Set Bonus:
  - Mana Tide: Restore 5 Mana per round
  - +20% Healing effectiveness

Earth Circle Set Bonus:
  - Mountain Stance: +50 Health, cannot be knocked back
  - +10% Defense

Air Circle Set Bonus:
  - Storm's Blessing: +15% Attack Speed, +10% Critical Hit Chance
  - Lightning Reflexes: First attack each combat auto-crits

Shadow Circle Set Bonus:
  - Night Stalker: +30% damage from stealth
  - Fear Aura: Enemies within 10m have -15 Morale

Light Circle Set Bonus:
  - Radiance: Heal 3 HP/round, allies within 10m gain +5 Morale
  - Smite Evil: +50% damage vs Evil alignment

Blade Mastery Set Bonus:
  - Master Duelist: +10% all stats in 1v1 combat
  - Blade Dance: Every 3rd attack is guaranteed critical hit

Heavy Weapons Set Bonus:
  - Unstoppable: Cannot be knocked back or stunned
  - Titan's Strength: +20% Attack Damage, +30 Health

Stealth & Assassination Set Bonus:
  - Silent Killer: Stealth kills do not alert nearby enemies
  - Lethal Precision: Critical hits ignore 70% armor
  - One Shot Kill: First attack from stealth has 50% chance for instant kill if target HP < 40%

Warlord Set Bonus:
  - Tactical Supremacy: Can hold 3 weapon loadouts, swap instantly (no Focus cost)
  - Indomitable: +30% survivability when outnumbered
  - Mobile Fortress: +30% movement speed, can sprint in heavy armor

Battlemaster Set Bonus:
  - Commander's Aura: Allies within 20m gain +20% all stats
  - Unstoppable Force: +50 Health, cannot be knocked down
  - Veteran's Resilience: -30% damage taken, +20% stamina regeneration

Monk Set Bonus:
  - Untouchable: +40% Dodge, enemies attacking you have -20% accuracy
  - Iron Body: +30 Armor when unarmored (monk robes/cloth only)
  - Chi Master: Unarmed attacks restore 5 Focus per hit

Archery Set Bonus:
  - Rapid Fire: Every 3rd shot is instant (no attack speed delay)
  - Wind Reader: +10% Accuracy, arrows ignore wind/weather penalties

Marksman Set Bonus:
  - Perfect Shot: First shot from stealth is guaranteed critical headshot (instant kill on non-elite)
  - Ghost Sniper: +60% Stealth, kills from 100m+ range do not reveal position
  - Long Range Master: +60% range, can target enemies 300m+ away

Hunter Set Bonus:
  - Master Hunter: Beast companion becomes elite (×2 stats)
  - Guerrilla Warfare: Traps cost -50% Focus, can place 5 traps simultaneously
  - Predator's Grace: No kite penalty, +20% movement speed in wilderness

Grenadier Set Bonus:
  - Friendly Explosives: Allies immune to grenade damage, gain buffs from friendly grenades
  - Bomb Artisan: Can craft custom grenades with 3 effects (damage + debuff + buff)
  - Chain Reaction: Explosions have 30% chance to trigger secondary blast (+50% damage)
```

### Dual-Circle Set Bonuses

```
Requirement: Unlock dual-school specialization + 2+ specialization skills

Flame Lightning Set Bonus:
  - Storm's Fury: Critical hits with fire/air spells trigger AoE explosion (30 damage)
  - Elemental Overload: Spell Power +20%

Twilight Set Bonus:
  - Duality's Gift: Casting shadow spell grants +10% holy damage (stacks 3×), vice versa
  - Forbidden Power: +30% Spell Power, +20 Mana Pool
  - Eternal Balance: Immune to fear AND charm

Spellblade Set Bonus:
  - Blade & Magic: Every 3rd melee attack triggers free spell (no Mana cost)
  - Enchanted Strikes: +20% Attack Damage, +15% Spell Power

Arcane Ranger Set Bonus:
  - Enchanted Quiver: Every 5th arrow is free (no Focus cost), deals ×2 damage
  - Elemental Precision: +15% Critical Chance, +20% Spell Power

Battlemage Set Bonus:
  - Frontline Sorcerer: +30 Health, +25% Spell Power in melee range
  - Arcane Juggernaut: +20 Armor, immune to silence/interrupt

Shadow Blade Set Bonus:
  - Death from Darkness: First attack from invisibility is instant-kill if target HP < 30%
  - Shadow's Embrace: +50% Stealth, +25% shadow damage

Warlock Set Bonus:
  - Demonic Pact: +50 Mana Pool, +30% Spell Power, permanently cursed (cannot be healed by Light magic)
  - Shadowflame Mastery: Shadow/fire spells have 30% chance to not consume Mana
  - Soul Reaper: Killing blows permanently increase Spell Power by +1 (stacks infinitely, lost on death)

Paladin/Templar Set Bonus:
  - Bulwark of Light: +60 Health, +30 Armor, immune to instant-kill effects
  - Demon Bane: ×2 damage vs Demons, Undead flee when killed (fear aura)
  - Healing Aura: Heal self and allies 5 HP/round within 10m

Death Knight Set Bonus:
  - Lich Form: When killed, resurrect as Lich (permanent undead transformation, +40% spell power, lose 50% max HP)
  - Frost Blade: Melee attacks apply frost slow (30% for 2 rounds)
  - Blood Shield: Taking damage grants blood shield (absorbs 20% of damage taken for 3 rounds)

Wild Mage Set Bonus:
  - Chaos Mastery: Random effects are 50% more powerful, backlash effects reduced 50%
  - Arcane Overload: Every 5th spell is free and guaranteed powerful effect (no backlash)
  - Unstable Magic: Enemies attacking you have 20% chance to trigger wild magic backlash on themselves
```

### Triple-Circle Set Bonuses

```
Requirement: Unlock triple-school specialization + 3+ specialization skills

Earthblood Mage Set Bonus:
  - Volcanic Resilience: +40 Armor, immune to burn/freeze/petrify
  - Elemental Supremacy: +30% Spell Power, +15% all resistances
  - Primordial Mastery: Elemental spells cost -20% Mana

Ice Storm Set Bonus:
  - Permafrost: Frozen enemies take +30% damage from all sources
  - Storm Caller: +25% Spell Power during storms/winter
  - Blizzard Sovereign: Immune to cold, +40% cold damage

Lava Titan Set Bonus:
  - Unstoppable Inferno: Cannot be slowed/rooted, leave burning trail
  - Magma Wrath: +40% damage vs structures/armor
  - Volcanic Core: +50 Health, +30 Armor
```

---

## DOTS Components

```csharp
using Unity.Entities;
using Unity.Collections;

namespace Godgame.Progression
{
    /// <summary>
    /// Race and culture identity for circle progression modifiers.
    /// </summary>
    public struct RaceComponent : IComponentData
    {
        public Race Race;
        public Culture Culture;
    }

    public enum Race : byte
    {
        Human = 0,
        Dwarf = 1,
        Orc = 2,
        Elf = 3,
        DarkElf = 4,
        Gnome = 5,
        // ... more races
    }

    public enum Culture : byte
    {
        None = 0,

        // Dwarven cultures
        DwarvenMountainClan = 1,
        DwarvenShieldClan = 2,

        // Orcish cultures
        OrcishBerserkerTribe = 3,
        OrcishShamanTribe = 4,

        // Human cultures
        HumanKnightlyOrder = 5,
        HumanMercenaryCompany = 6,

        // Elven cultures
        ElvenHighCourt = 7,
        ElvenWoodRanger = 8,

        // Dark Elf cultures
        DrowNobleHouse = 9,
        DrowUnderdarkRaiders = 10,

        // Gnome cultures
        GnomeInventorGuild = 11,
        GnomeArcaneUniversity = 12,

        // ... more cultures
    }

    /// <summary>
    /// Circle progression tracking - which circles entity has invested in.
    /// </summary>
    public struct CircleProgression : IComponentData
    {
        // Elemental Circles (0-10 skill count)
        public byte FireCircle;
        public byte WaterCircle;
        public byte EarthCircle;
        public byte AirCircle;

        // Shadow & Light
        public byte ShadowCircle;
        public byte LightCircle;

        // Physical Melee Circles
        public byte BladeMasteryCircle;
        public byte HeavyWeaponsCircle;
        public byte StealthAssassinationCircle;
        public byte WarlordCircle;          // Pure Phys: versatile weapons
        public byte BattlemasterCircle;     // Pure Phys: buffs/debuffs
        public byte MonkCircle;             // Phys/Fin: unarmed martial arts

        // Ranged Circles
        public byte ArcheryCircle;          // Baseline ranged
        public byte MarksmanCircle;         // Pure Fin: long-range sniping
        public byte HunterCircle;           // Phys/Fin: beast companions, traps
        public byte GrenadierCircle;        // Phys/Will: explosives, magical payloads

        // Specialization unlocks (bitflags)
        public CircleSpecialization Specializations;
    }

    /// <summary>
    /// Racial and cultural bonuses for circle learning and effectiveness.
    /// Applied on top of base learning rates.
    /// </summary>
    public struct RacialCircleBonuses : IComponentData
    {
        // Learning speed multipliers (0-200%, stored as 0-200)
        // 100 = normal speed, 130 = +30% faster, 70 = -30% slower
        public byte FireCircleLearningSpeed;
        public byte WaterCircleLearningSpeed;
        public byte EarthCircleLearningSpeed;
        public byte AirCircleLearningSpeed;
        public byte ShadowCircleLearningSpeed;
        public byte LightCircleLearningSpeed;
        public byte BladeMasteryLearningSpeed;
        public byte HeavyWeaponsLearningSpeed;
        public byte StealthLearningSpeed;
        public byte WarlordLearningSpeed;
        public byte BattlemasterLearningSpeed;
        public byte MonkLearningSpeed;
        public byte ArcheryLearningSpeed;
        public byte MarksmanLearningSpeed;
        public byte HunterLearningSpeed;
        public byte GrenadierLearningSpeed;

        // Effectiveness multipliers (0-200%, stored as 0-200)
        // 100 = normal effectiveness, 115 = +15% effectiveness
        public byte FireCircleEffectiveness;
        public byte WaterCircleEffectiveness;
        public byte EarthCircleEffectiveness;
        public byte AirCircleEffectiveness;
        public byte ShadowCircleEffectiveness;
        public byte LightCircleEffectiveness;
        public byte BladeMasteryEffectiveness;
        public byte HeavyWeaponsEffectiveness;
        public byte StealthEffectiveness;
        public byte WarlordEffectiveness;
        public byte BattlemasterEffectiveness;
        public byte MonkEffectiveness;
        public byte ArcheryEffectiveness;
        public byte MarksmanEffectiveness;
        public byte HunterEffectiveness;
        public byte GrenadierEffectiveness;

        // Specialization bonuses
        public byte SpellbladeEffectiveness;
        public byte ArcaneRangerEffectiveness;
        public byte BattlemageEffectiveness;
        public byte ShadowBladeEffectiveness;
        public byte PaladinEffectiveness;
        public byte DeathKnightEffectiveness;
        public byte WildMageEffectiveness;
        public byte WarlockEffectiveness;
        public byte LavaTitanEffectiveness;
        public byte EarthbloodEffectiveness;
    }

    [System.Flags]
    public enum CircleSpecialization : uint
    {
        None = 0,

        // Dual-School Elemental
        FlameLightning = 1 << 0,        // Air + Fire
        IceStorm = 1 << 1,              // Air + Water
        LavaTitan = 1 << 2,             // Fire + Earth
        MudGolem = 1 << 3,              // Earth + Water
        Warlock = 1 << 4,               // Shadow + Fire
        // ... (X combinations total)

        // Triple-School Elemental
        EarthbloodMage = 1 << 10,       // Fire + Earth + Water
        StormElemental = 1 << 11,       // Air + Water + Earth
        // ... (Y combinations total)

        // Shadow & Light
        Twilight = 1 << 15,             // Shadow + Light

        // Physical-Magical Hybrids
        Spellblade = 1 << 20,           // Blade + Elemental
        ArcaneRanger = 1 << 21,         // Archery + Elemental
        Battlemage = 1 << 22,           // Heavy Weapons + Elemental
        ShadowBlade = 1 << 23,          // Stealth + Shadow
        Paladin = 1 << 24,              // Light + Physical
        DeathKnight = 1 << 25,          // Shadow/Water + Physical
        WildMage = 1 << 26,             // Arcane + Physical
    }

    /// <summary>
    /// Active circle skills - skills entity has learned and can use.
    /// </summary>
    public struct CircleSkills : IComponentData
    {
        public Entity SkillSlot1;  // Skill entity (or Entity.Null)
        public Entity SkillSlot2;
        public Entity SkillSlot3;
        public Entity SkillSlot4;
        public Entity SkillSlot5;
        public Entity SkillSlot6;
        public Entity SkillSlot7;
        public Entity SkillSlot8;
        public Entity SkillSlot9;
        public Entity SkillSlot10;

        // Total learned skills (includes passives)
        public byte TotalSkillsLearned;
        public byte TotalPassivesLearned;
    }

    /// <summary>
    /// Individual skill entity component.
    /// </summary>
    public struct Skill : IComponentData
    {
        public CircleType Circle;           // Which circle this belongs to
        public SkillType Type;              // Active skill or passive
        public FixedString64Bytes Name;     // Skill name (e.g., "Fireball")

        // Costs
        public byte ManaCost;               // 0-100
        public byte FocusCost;              // 0-100
        public byte StaminaCost;            // 0-10

        // Effectiveness
        public byte BaseDamage;             // 0-255 (if damage skill)
        public byte BonusPercentage;        // 0-100 (if passive bonus)
        public byte Quality;                // 1-100 (from lesson)
        public LessonRarity Rarity;         // Common/Uncommon/Rare/Epic/Legendary

        // Refinement
        public byte RefinementLevel;        // 0-100% (effectiveness)
        public ushort PracticeTime;         // Days practiced (for self-refinement)

        // Cooldown (if applicable)
        public byte CooldownRounds;         // 0-10
        public byte CurrentCooldown;        // Current cooldown remaining
    }

    public enum CircleType : byte
    {
        // Elemental
        Fire = 0,
        Water = 1,
        Earth = 2,
        Air = 3,
        Shadow = 4,
        Light = 5,

        // Physical Melee
        BladeMastery = 6,
        HeavyWeapons = 7,
        StealthAssassination = 8,
        Warlord = 9,
        Battlemaster = 10,
        Monk = 11,

        // Ranged
        Archery = 12,
        Marksman = 13,
        Hunter = 14,
        Grenadier = 15,

        // Specializations
        FlameLightning = 20,
        IceStorm = 21,
        LavaTitan = 22,
        Earthblood = 23,
        Twilight = 24,
        Warlock = 25,
        Spellblade = 26,
        ArcaneRanger = 27,
        Battlemage = 28,
        ShadowBlade = 29,
        Paladin = 30,
        DeathKnight = 31,
        WildMage = 32,
    }

    public enum SkillType : byte
    {
        ActiveSkill = 0,    // Costs resources, manual activation
        Passive = 1,        // Always active, no cost
    }

    /// <summary>
    /// Set bonuses component - tracks which set bonuses are active.
    /// </summary>
    public struct SetBonuses : IComponentData
    {
        // Single-Circle Sets (4+ skills/passives)
        public bool FireSetBonus;
        public bool WaterSetBonus;
        public bool EarthSetBonus;
        public bool AirSetBonus;
        public bool ShadowSetBonus;
        public bool LightSetBonus;
        public bool BladeMasterySetBonus;
        public bool HeavyWeaponsSetBonus;
        public bool StealthSetBonus;
        public bool WarlordSetBonus;
        public bool BattlemasterSetBonus;
        public bool MonkSetBonus;
        public bool ArcherySetBonus;
        public bool MarksmanSetBonus;
        public bool HunterSetBonus;
        public bool GrenadierSetBonus;

        // Dual-Circle Sets (specialization + 2+ spec skills)
        public bool FlameLightningSetBonus;
        public bool TwilightSetBonus;
        public bool WarlockSetBonus;
        public bool SpellbladeSetBonus;
        public bool ArcaneRangerSetBonus;
        public bool BattlemageSetBonus;
        public bool ShadowBladeSetBonus;
        public bool PaladinSetBonus;
        public bool DeathKnightSetBonus;
        public bool WildMageSetBonus;

        // Triple-Circle Sets (specialization + 3+ spec skills)
        public bool EarthbloodSetBonus;
        public bool IceStormSetBonus;
        public bool LavaTitanSetBonus;
    }

    /// <summary>
    /// Circle learning progress - tracks lessons being absorbed.
    /// </summary>
    public struct CircleLearningProgress : IComponentData
    {
        public Entity CurrentLesson;        // Lesson entity being absorbed (or Entity.Null)
        public ushort DaysAbsorbing;        // Days spent absorbing current lesson
        public ushort AbsorptionTimeRequired; // Total days needed
        public byte AbsorptionPercentage;   // 0-100%
    }

    /// <summary>
    /// Lesson entity component (see Education System).
    /// </summary>
    public struct CircleLesson : IComponentData
    {
        public CircleType Circle;           // Which circle this teaches
        public FixedString64Bytes SkillName; // Skill being taught
        public byte Quality;                // 1-100
        public LessonRarity Rarity;         // Common/Uncommon/Rare/Epic/Legendary
        public byte Tier;                   // 1-10

        public Entity Teacher;              // Who taught this (or Entity.Null if self-made)
        public LessonAffix Affix;           // Special bonus (if any)
    }

    public enum LessonRarity : byte
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    [System.Flags]
    public enum LessonAffix : ushort
    {
        None = 0,
        Swift = 1 << 0,         // -10% Focus cost
        Efficient = 1 << 1,     // -15% Mana cost
        Brutal = 1 << 2,        // +20% damage
        Riposte = 1 << 3,       // Counter-attack chance
        Persistent = 1 << 4,    // +50% duration
        Amplified = 1 << 5,     // +30% range/AoE
        // ... more affixes
    }
}
```

---

## Shareability Assessment

**PureDOTS Candidate:** Partial

**Rationale:** Core progression mechanics (skill learning, circle investment, set bonuses) could be shared, but circle definitions and racial bonuses are Godgame-specific.

**Shared Components:**
- `CircleProgression`: Track circle investments
- `CircleSkills`: Active skills component
- `SetBonuses`: Set bonus tracking
- `CircleLearningProgress`: Lesson absorption tracking
- Generic skill/lesson system

**Game-Specific Adapters:**
- Godgame: Elemental circles, racial bonuses, guild specializations
- Space4x: Would need different circle definitions (tech trees, officer progression)

## Performance Budget

- **Max Entities:** 10,000 entities with circle progression
- **Update Frequency:** Per tick (when learning/refining)
- **Burst Compatibility:** Yes - all progression systems Burst-compiled
- **Memory Budget:** ~32 bytes per CircleProgression component, ~16 bytes per skill slot

---

## Example Builds

### 1. Fire Spellblade (Dual-Circle Hybrid)

**Concept:** Sword-wielding fire mage, melee + magic fusion.

**Circle Investments:**
- Fire Circle: 5 skills/passives
  - Fireball, Immolation, Wall of Fire, Pyromancy I, Pyromancy II
- Blade Mastery Circle: 4 skills/passives
  - Riposte, Whirlwind, Swordsman I, Swordsman II
- Spellblade Specialization: 2 skills
  - Elemental Blade (fire), Arcane Slash (fire wave)

**Set Bonuses:**
- Fire Set Bonus: Inferno Aura (5m fire damage), +15% Spell Power
- Blade Mastery Set Bonus: Master Duelist (+10% 1v1 stats), Blade Dance (every 3rd crit)
- Spellblade Set Bonus: Blade & Magic (every 3rd melee = free spell), Enchanted Strikes (+20% Attack Damage, +15% Spell Power)

**Combat Strategy:**
1. Cast Elemental Blade (enchant sword with fire)
2. Engage melee with Whirlwind (attack multiple enemies)
3. Every 3rd attack triggers free Fireball (no Mana cost)
4. Use Riposte for counter-attacks
5. Immolation buff for sustained fire damage
6. Arcane Slash for ranged fire wave when kiting

**Stats:**
- Finesse 70 (sword precision)
- Strength 60 (blade control)
- Intelligence 65 (fire magic)
- Will 55 (mana pool)

**Guilds:**
- Fire Mages' Guild (Fire Circle training)
- Warriors' Guild (Blade Mastery training)
- Spellblade Academy (hybrid specialization)

**Progression Path:**
- Join Fire Mages' Guild
- Learn Fire Circle
- Join Warriors' Guild (parallel training)
- Learn Blade Mastery
- Unlock Spellblade specialization
- Master Spellblade (all bonuses active)

---

### 2. Twilight Adept (Forbidden Duality)

**Concept:** Shadow + Light master, moral ambiguity, forbidden power.

**Circle Investments:**
- Shadow Circle: 5 skills/passives
  - Shadow Bolt, Invisibility, Fear, Vampiric Touch, Umbramancy I
- Light Circle: 4 skills/passives
  - Holy Bolt, Divine Shield, Heal, Lumomancy I
- Twilight Specialization: 3 skills
  - Twilight Bolt, Duality Shift, Eclipse

**Set Bonuses:**
- Shadow Set Bonus: Night Stalker (+30% stealth damage), Fear Aura (-15 Morale 10m)
- Light Set Bonus: Radiance (heal 3 HP/round, +5 ally Morale), Smite Evil (+50% vs Evil)
- Twilight Set Bonus: Duality's Gift (shadow/holy damage stacking), Forbidden Power (+30% Spell Power, +20 Mana), Eternal Balance (immune fear/charm)

**Combat Strategy:**
1. Duality Shift to Shadow Form (stealth engagement)
2. Invisibility + Shadow Bolt (stealth damage + morale drain)
3. Switch to Light Form when allies need healing
4. Heal + Divine Shield (support mode)
5. Eclipse for massive AoE damage + heal (ultimate ability)

**Stats:**
- Intelligence 70 (dual-school complexity)
- Will 75 (resist corruption)
- Finesse 60 (shadow precision)
- Wisdom 55 (understand balance)

**Guilds:**
- Shadow Coven (Shadow Circle training)
- Light Clergy (Light Circle training, controversial)
- Twilight Order (invitation, secretive)

**Alignment:**
- True Neutral (required for Twilight)
- Started Chaotic Good, shifted to True Neutral via Twilight philosophy

**Progression Path:**
- Learn Shadow Circle (Shadow Coven)
- Learn Light Circle (defect to Light Clergy)
- Twilight Order invitation (recognized duality)
- Master Twilight (secret training)
- Hunted by both Light purists AND Shadow cultists (narrative conflict)

---

### 3. Earthblood Mage (Triple-Circle Elementalist)

**Concept:** Primordial elementalist, volcanic power, life force manipulation.

**Circle Investments:**
- Fire Circle: 3 skills/passives
  - Fireball, Immolation, Pyromancy I
- Earth Circle: 3 skills/passives
  - Stone Spear, Earth Shield, Geomancy I
- Water Circle: 3 skills/passives
  - Frost Bolt, Healing Stream, Hydromancy I
- Earthblood Specialization: 4 skills
  - Magma Lance, Blood Boil, Earthblood Surge, Primordial Mastery passive

**Set Bonuses:**
- Fire Set Bonus: None (only 3 skills, need 4+)
- Earth Set Bonus: None
- Water Set Bonus: None
- Earthblood Set Bonus: Volcanic Resilience (+40 Armor, immune burn/freeze/petrify), Elemental Supremacy (+30% Spell Power, +15% resistances), Primordial Mastery (-20% Mana cost)

**Combat Strategy:**
1. Earthblood Surge (AoE heal + Stone Skin buff to allies)
2. Magma Lance (armor-melting attacks)
3. Blood Boil (unstoppable internal damage on boss)
4. Immolation + Earth Shield (tanky fire mage)
5. Healing Stream for sustained healing

**Stats:**
- Intelligence 75 (triple-school mastery)
- Will 80 (channel primordial forces)
- Strength 55 (endure elemental strain)
- Wisdom 65 (rare insight required)

**Guilds:**
- Fire Mages' Guild
- Earth Mages' Guild
- Water Healers' Circle
- Earthblood Conclave (invitation, ultra-rare)

**Progression Path:**
- Learn Fire Circle (Fire Guild)
- Learn Earth Circle (Earth Guild, parallel practice Fire)
- Learn Water Circle (Healers' Circle)
- Unlock Earthblood specialization (rare achievement)
- Master Earthblood (legendary status)

**Rarity:**
- Only 1-2 Earthblood Mages per generation in a village
- Requires extensive multi-school training
- High stat requirements (Intelligence + Will > 140)
- Narrative: Revered as elemental sage, sought for counsel

---

### 4. Shadow Blade Assassin (Physical-Magical Hybrid)

**Concept:** Magical assassin, shadow magic + stealth kills.

**Circle Investments:**
- Stealth & Assassination Circle: 5 skills/passives
  - Backstab, Smoke Bomb, Poison Blade, Assassin I, Assassin II
- Shadow Circle: 3 skills/passives
  - Shadow Bolt, Invisibility, Umbramancy I
- Shadow Blade Specialization: 3 skills
  - Umbral Strike, Shadow Clone, Nightmare

**Set Bonuses:**
- Stealth Set Bonus: Silent Killer (stealth kills don't alert), Lethal Precision (crits ignore 70% armor)
- Shadow Set Bonus: Night Stalker (+30% stealth damage), Fear Aura (-15 Morale 10m)
- Shadow Blade Set Bonus: Death from Darkness (instant-kill <30% HP from invisibility), Shadow's Embrace (+50% Stealth, +25% shadow damage)

**Combat Strategy:**
1. Invisibility (enter combat undetected)
2. Umbral Strike (teleport + backstab)
3. If target survives, Nightmare curse (hallucinate, attack allies)
4. Shadow Clone for distraction
5. Poison Blade for DoT finish
6. Smoke Bomb to disengage if surrounded

**Stats:**
- Finesse 85 (precision assassinations)
- Intelligence 65 (shadow magic)
- Will 55 (mana pool)
- Perception 70 (target tracking)

**Guilds:**
- Thieves' Guild (age 16-22, Stealth training)
- Shadow Coven (age 23-28, Shadow Circle)
- Shadow Blade Cult (invitation age 29, secretive assassins)

**Alignment:**
- Chaotic Neutral (pragmatic killer)
- Contracts: Assassinate corrupt nobles, eliminate threats, espionage

**Progression Path:**
- Join Thieves' Guild (street urchin background)
- Master Stealth Circle
- Learn Shadow Circle (Shadow Coven)
- Unlock Shadow Blade specialization
- Legendary assassin (never caught, feared)

---

### 5. Dwarven Lava Titan (Racial Specialization)

**Concept:** Dwarven Earth + Fire master, volcanic power, defensive tank.

**Race & Culture:**
- Race: Dwarf
- Culture: Dwarven Mountain Clan
- Racial Bonuses:
  - Earth Circle: +40% learning speed (racial +25%, cultural +15%)
  - Heavy Weapons: +30% learning speed
  - Fire Circle: Base learning speed
  - Strength: +10 (base 50 → 60)
  - Will: +15 (base 50 → 65)

**Circle Investments:**
- Earth Circle: 5 skills/passives
  - Stone Spear, Earth Shield, Tremor, Geomancy I, Stone Skin I
  - **Learning Time: 4 years** (instead of 6 years for humans, thanks to +40% racial bonus)
- Fire Circle: 4 skills/passives
  - Fireball, Immolation, Pyromancy I, Fire Resistance I
  - **Learning Time: 5 years** (normal speed, no racial bonus)
- Lava Titan Specialization: 3 skills
  - Lava Flow, Molten Armor, Volcanic Eruption
  - **Learning Time: 3 years**

**Set Bonuses:**
- Earth Set Bonus: Mountain Stance (+50 Health, cannot be knocked back), +10% Defense
- Fire Set Bonus: None (only 4 skills, need 4+)
- Lava Titan Set Bonus: Unstoppable Inferno (can't be slowed/rooted, burning trail), Magma Wrath (+40% vs structures/armor), Volcanic Core (+50 Health, +30 Armor)

**Combat Strategy:**
1. Molten Armor (tank mode: +50% Armor, melee attackers take fire damage)
2. Earth Shield (+40% Armor buff)
3. Lava Flow (zone control: create impassable lava zones)
4. Stone Spear + Immolation (armored fire warrior)
5. Volcanic Eruption (siege weapon: 100 AoE damage)
6. Tremor (AoE stun when surrounded)

**Stats (with racial bonuses):**
- Strength: 85 (60 base + 25 training)
- Will: 80 (65 base + 15 training)
- Intelligence: 65 (45 base + 20 training, -5 racial penalty offset)
- Finesse: 50 (average, not needed)

**Guilds:**
- Earth Mages' Guild (accelerated thanks to racial bonus)
- Fire Mages' Guild (normal pace)
- Lava Titan Forge (invitation, dwarven-exclusive)

**Progression Path:**
- Learn Earth Circle (accelerated, racial bonus)
- Learn Fire Circle (normal pace)
- Unlock Lava Titan specialization
- Master Lava Titan (all bonuses active)

**Training Efficiency:** Faster than human Lava Titans due to racial bonuses

**Racial Advantages:**
1. **Faster Earth Mastery:** +40% learning speed = 4 years instead of 6
2. **Higher Base Strength/Will:** +10 Str, +15 Will = easier stat requirements
3. **Cultural Guilds:** Dwarven Mountain Clan has exclusive Lava Titan forges
4. **Heavy Armor Mastery:** -20% movement penalty (can wear plate armor without speed loss)
5. **Volcanic Forges:** +30% fire resistance (cultural environment)

**Racial Disadvantages:**
1. **Low Intelligence:** -5 penalty = harder to learn complex magic (Fire Circle slower)
2. **Low Finesse:** 85 required for Monk Circle (very difficult for dwarves)
3. **Cultural Restrictions:** Dwarven Mountain Clans discourage Shadow/Light magic

**Narrative:**
- Thrain Ironforge, age 35 Lava Titan dwarf
- Defended his mountain hold from orcish siege (Volcanic Eruption destroyed siege towers)
- Molten Armor + Earth Shield = unkillable tank (200+ Health, 80+ Armor)
- Created impassable lava moats around fortress gates
- Revered as "Forge Lord" by his clan
- Cannot be slowed, rooted, or knocked back (set bonuses)

**Comparison to Human Lava Titan:**
- Dwarf: 12 years training, +10 Str, +15 Will, better tank
- Human: 14 years training, +10 Wisdom, +20 Diplomacy, more versatile
- Elf: 10 years training (Earth +30% racial), +20 Will, +15 Wis, more spell power
- Gnome: 11 years training (+25% elemental), +20 Int, better spell complexity

---

### 6. Dark Elf Shadow Blade (Racial Specialization)

**Concept:** Drow assassin with shadow teleportation, underground predator.

**Race & Culture:**
- Race: Dark Elf (Drow)
- Culture: Drow Noble House
- Racial Bonuses:
  - Shadow Circle: +35% learning speed, +20% effectiveness
  - Stealth Circle: +30% learning speed, +15% effectiveness
  - Shadow Blade: +25% effectiveness (cultural +15%)
  - Finesse: +20 (base 50 → 70)
  - Intelligence: +10 (base 50 → 60)
  - Will: +15 (base 50 → 65)

**Circle Investments:**
- Shadow Circle: 4 skills/passives
  - Shadow Bolt, Invisibility, Vampiric Touch, Umbramancy I
  - **Learning Time: 2.5 years** (instead of 4 years, +35% racial bonus)
- Stealth & Assassination Circle: 5 skills/passives
  - Backstab, Smoke Bomb, Poison Blade, Assassin I, Assassin II
  - **Learning Time: 3 years** (instead of 5 years, +30% racial bonus)
- Shadow Blade Specialization: 3 skills
  - Umbral Strike, Shadow Clone, Nightmare
  - **Learning Time: 2 years**
  - **+40% effectiveness total** (racial +20%, cultural +25%)

**Set Bonuses:**
- Shadow Set Bonus: Night Stalker (+30% stealth damage), Fear Aura (-15 Morale 10m)
- Stealth Set Bonus: Silent Killer (stealth kills don't alert), Lethal Precision (crits ignore 70% armor), One Shot Kill (50% instant kill < 40% HP)
- Shadow Blade Set Bonus: Death from Darkness (instant-kill < 30% HP from invisibility), Shadow's Embrace (+50% Stealth, +25% shadow damage)

**Combat Strategy:**
1. Invisibility (+80% Stealth, invisible in darkness with darkvision)
2. Umbral Strike (teleport to target + backstab: ×3 damage)
3. **Racial bonus: +40% Shadow Blade effectiveness = ×4.2 damage** instead of ×3
4. Nightmare curse (enemy hallucinates, attacks allies)
5. Shadow Clone (distraction)
6. Poison Blade (DoT finish)

**Stats (with racial bonuses):**
- Finesse: 95 (70 base + 25 training)
- Intelligence: 75 (60 base + 15 training)
- Will: 70 (65 base + 5 training)
- Strength: 45 (average for drow, -5 racial)

**Guilds:**
- Drow Assassin Academy (accelerated stealth training)
- Shadow Coven (accelerated shadow magic)
- Shadow Blade Cult (invitation, drow-dominated)

**Progression Path:**
- Learn Stealth Circle (accelerated, +30% racial bonus)
- Learn Shadow Circle (accelerated, +35% racial bonus)
- Unlock Shadow Blade specialization
- Master Shadow Blade (all bonuses, +40% effectiveness)

**Training Efficiency:** Faster than human Shadow Blades due to racial bonuses

**Racial Advantages:**
1. **Fastest Shadow Mastery:** +35% learning speed = 2.5 years instead of 4
2. **Darkvision:** +60% Stealth in darkness (underground advantage)
3. **Shadow Blade +40% effectiveness:** Teleport damage ×4.2 instead of ×3
4. **High Base Finesse/Will:** +20 Fin, +15 Will = easier stat requirements
5. **Poison Mastery:** +30% poison potency (cultural)

**Racial Disadvantages:**
1. **Sunlight Weakness:** -30% effectiveness on surface during day
2. **Low Strength:** -5 penalty = cannot use Heavy Weapons effectively
3. **Cultural Evil Alignment:** +20 Chaotic, +10 Evil = rejected by most surface guilds
4. **Hunted:** Drow assassins are kill-on-sight in most surface cities

**Narrative:**
- Xalyth Nightwhisper, age 27 drow Shadow Blade
- Operates from underdark, contracts for noble house assassinations
- Teleport strikes deal ×4.2 damage (racial bonus)
- Invisible in darkness (darkvision + racial stealth bonus)
- Killed 37 targets, never caught
- Surface raids at night only (daylight weakness)
- Rival: Human Shadow Blade (less powerful, but no sunlight weakness)

**Comparison to Human Shadow Blade:**
- Drow: 7.5 years training, +40% Shadow Blade effectiveness, darkvision, sunlight weakness
- Human: 12 years training, normal effectiveness, no racial restrictions, adaptable
- Elf: 9 years training (Shadow +20% racial), +20 Will, better spell power, no sunlight weakness
- Orc: 10 years training (Shadow -10% penalty), +15 Str, worse at stealth

---

## Related Documentation

- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Base attributes (Str/Fin/Will/Int)
- **Education & Tutoring:** [Education_And_Tutoring_System.md](../Villagers/Education_And_Tutoring_System.md) - Lesson system, absorption, refinement, Wisdom stat
- **Focus & Status Effects:** [Focus_And_Status_Effects_System.md](Focus_And_Status_Effects_System.md) - Focus allocation for skills, sustained abilities
- **Individual Combat:** [../Combat/Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - How circle skills integrate with combat
- **Guild System:** [../Villagers/Guild_System.md](../Villagers/Guild_System.md) - Guild training, specializations, recruitment
- **Stealth & Perception:** [../Combat/Stealth_And_Perception_System.md](../Combat/Stealth_And_Perception_System.md) - Shadow Circle, Stealth skills

---

## Open Questions

1. **Combination Count (X and Y):** How many dual-school and triple-school combinations should exist?
   - X (dual-school): 10? 20? (based on feasibility and balance)
   - Y (triple-school): 3? 5? (rare, requires high mastery)

2. **Physical-Magical Balance:** Should hybrid builds (Spellblade, Battlemage) have penalties?
   - Currently: No penalties, just high stat requirements
   - Alternative: -10% effectiveness in both schools (jack-of-all-trades penalty)

3. **Circle Respec:** Can entities unlearn circles to reallocate?
   - Current: No (permanent choices)
   - Alternative: Rare ritual (deity intervention, memory wipe) allows respec

4. **Guild Exclusivity:** Can entities join multiple guilds simultaneously?
   - Current: Yes, but guild loyalty/reputation may suffer
   - Alternative: Exclusive membership (choose one primary guild)

5. **Alignment Restrictions:** Should Evil alignment block Light Circle entirely?
   - Current: Strongly discouraged (stat requirements), but not impossible
   - Alternative: Hard-lock (Evil cannot learn Light, Good cannot learn Shadow)

6. **Set Bonus Stacking:** Do single-circle bonuses stack with specialization bonuses?
   - Current: Yes (Fire Set + Spellblade Set both active)
   - Alternative: Choose one active set bonus at a time

---

**For Implementers:** Start with single-circle systems (Fire, Water, Blade Mastery) before adding dual/triple combinations. Focus on lesson integration with Education System first. Implement RaceComponent and RacialCircleBonuses components to apply learning speed and effectiveness modifiers based on race and culture.

**For Designers:** Tune stat requirements and set bonuses to create meaningful build diversity. Avoid power creep where triple-school masters are always superior.

**For Narrative:** Circle progressions create character arcs (naive fire apprentice → legendary Earthblood Mage), factional conflicts (Twilight Order hunted by Light/Shadow purists), racial rivalries (dwarven Lava Titans vs orcish Warlords), cultural identity (drow Shadow Blades feared in underdark, gnome Grenadiers sought as siege specialists), and emergent stories (Shadow Blade assassin kills corrupt noble, sparks civil war; elven archmage achieves triple-school mastery after 200 years).

