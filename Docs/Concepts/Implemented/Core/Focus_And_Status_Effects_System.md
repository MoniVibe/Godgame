# Focus & Status Effects System

## Overview

The **Focus & Status Effects System** governs resource management for special abilities and temporary/permanent conditions affecting entities. Focus represents an entity's attention and capability to perform special actions, while statuses modify entity capabilities through buffs, debuffs, injuries, and enhancements.

**Key Features:**
- **Focus Resource:** Spent on class-specific abilities (cleaving, stealth, raising minions, multi-tasking)
- **Focus Maximum:** Scales with Finesse, increases with usage over time
- **Multi-Focus:** Some entities can focus on multiple targets/tasks simultaneously
- **Status Effects:** Limb-specific (broken arm, bleeding leg) and whole-body (diamond skin, superior intellect)
- **Status Duration:** Temporary (buffs/debuffs) and permanent (injuries, mutations)
- **Status Intensity:** Slight (increased pain) to Major (unstoppable movement, invulnerability)

**For Narrative:** Berserker cleaving through multiple foes (focus on multi-target), rogue vanishing from sight (focus on stealth), necromancer commanding undead army (focus per minion), mage juggling multiple construction projects (focus per tool).

---

## Focus System

### Focus Resource

**Focus (0-100):**
```
Represents: Mental/physical attention, concentration, capability to perform special actions
Resource Type: Regenerating pool (like stamina)
Regeneration: +10 Focus per round (5 seconds) when not using abilities

Maximum Focus:
  FocusMax = (Finesse × 0.5) + (Intelligence × 0.3) + (Will × 0.2)

Example:
  Warrior: Finesse 80, Intelligence 40, Will 60
  → FocusMax = (80 × 0.5) + (40 × 0.3) + (60 × 0.2)
  → FocusMax = 40 + 12 + 12 = 64 Focus

Focus increases with usage:
  Every 100 focus spent → +1 FocusMax (permanent)
  Capped at 100 FocusMax
```

### Focus Allocation & Mastery

Entities rarely dedicate 100% of their attention to a single opponent. Focus can be **sharded across multiple targets or tasks**, with the distribution directly affecting derived stats (hit chance, dodge, crit, channel stability, etc.). Mastery over this distribution scales off **Finesse + Will**, rewarding agile minds and disciplined resolve.

```
FocusAllocation = FocusPool × AllocationPercentage
DerivedStatBonus = FocusAllocation × ClassBonusCurve

Example Duel:
  High-level champion (FocusMax 80) is ambushed by a rival hero plus three low-tier lackeys.

  Allocation Plan:
    - Primary rival hero: 75% focus (60 points)
    - Three lackeys: 25% focus split evenly ( ~6-7 points each )

  Effects:
    - Main rival receives full benefit: +15% dodge, +20% crit, +10% hit from high focus.
    - Lackeys still face lethal responses because each 6-7 focus chunk is enough to auto-parry or knock them out with cleaves.

  Result:
    The champion dispatches lackeys with minimal effort while preserving majority focus for the true threat.
```

**Mastery Skill (Finesse + Will):**
- Determines how many concurrent focus allocations remain efficient (e.g., `ConcurrentTargets = 1 + floor((Finesse + Will)/80)`).
- Reduces penalty when reallocating focus mid-round (experienced warriors can retarget faster).
- Influences how much focus must be reserved per threat tier—high mastery allows 10-20% focus maintenance on low-tier enemies while keeping bonuses maxed against bosses.

> *Design Intent:* Encourage players to triage threats, split attention intelligently, and feel rewarded for mastering finesse-driven resource juggling rather than button-mashing every foe with equal effort.

### Threat Perception & Courage Routing

Every hostile entity emits a **Threat Level** based on its raw power, recent actions, and perceived intent. Nearby actors absorb this threat and decide how much focus to dedicate using perception-driven logic.

```
ThreatLevel = BasePower × (RecentLethality + AuraModifiers)
PerceivedThreat = ThreatLevel × ThreatAwareness
ThreatAwareness = clamp01( (Perception + Wisdom + Intelligence × 0.5) / 300 )

BoldnessCheck = (Will + Finesse) - PerceivedThreat
  >0 → Entity stands ground, allocates focus optimally
  <0 → Cowardice triggers: reduced focus efficiency, chance to flee or misallocate
```

- **Perception & Finesse** improve rapid assessment, allowing entities to absorb high threat without panicking.
- **Wisdom & Intelligence** temper reactions—wise warriors ignore theatrics, while low-intelligence minions may overcommit focus to low-tier illusions.
- **Bold/Coward States** modify regen and allocation: bold units gain +10% focus efficiency against the threat, cowards lose up to 30% and may dump focus into defensive postures or retreat paths.

Designers can script bosses with aura modifiers (fear, dread, grace) to force morale checks; high-mastery heroes counter by stacking perception buffs or courage miracles before entering battle.

### Expertise & Knowledge Transfer

Focus interacts with **Expertise**, the long-term proficiency an entity builds within disciplines (blade mastery, beast husbandry, healing, siegecraft, etc.). Expertise governs both **lesson quality** (what the entity can teach others) and **contextual bonuses** (how focus efficiencies translate to outcomes).

Key rules:

- **Lesson Quality**: `LessonRating = ExpertiseTier × FocusCommittedToTeaching`. Master swordsmen spending meaningful focus on instruction produce higher-grade lessons; dabblers offering low focus barely transfer knowledge.
- **Detection & Insight**: Beastmasters with high `AnimalsExpertise` automatically roll enhanced perception checks on creatures, spotting plagues or maladies at a glance.
- **Bonus Hooks**: High expertise tiers unlock additive bonuses (crit chance for swordplay, faster taming for beastmasters, better ritual stability for shamans) that stack with focus allocations.
- **Focus Synergy**: Allocating focus to actions within your top expertise yields +10–25% efficiency (more damage per focus, faster channeling) while off-discipline tasks suffer mild penalties.

> Example: A combatant devoted solely to swords has `SwordExpertise Tier 5`. When teaching apprentices, they spend 40 Focus to run drills, granting elite lesson quality. In battle, their cleave actions gain +15% damage per focus due to deep expertise.

### Combat Movement & Mobility

Movement speed ties into focus, stamina, and gear loadouts. During combat, entities evaluate whether to reposition, kite, or burst toward targets based on class perks.

#### Baseline Formula
```
MoveSpeed = BaseSpeed × (1 - ArmorWeightPenalty + ArmorQualityBonus) + MobilityBuffs
BurstDash = MoveSpeed × BurstMultiplier (costs Stamina)
```

| Archetype | Mobility Traits |
|-----------|-----------------|
| **Warriors** | Can spend stamina for short **Burst Dashes** (+50% speed for 2 seconds). Heavy armor imposes penalties offset by high Physique and armor quality/rarity. |
| **Hunters** | Naturally higher base speed; can **kite** targets by maintaining distance. Sustained kiting adds accuracy penalties (-5% per second moving while aiming) unless high mastery mitigates. |
| **Rogues** | Highest innate speed and acceleration; can weave in/out with minimal penalties. Gain extra dodge frames when sprinting. |
| **Mages/Casters** | Lower base speed; rely on **Blink/Teleport** spells or **Haste** buffs to reposition. Without mobility spells they risk being caught. |

- **Stamina Interaction:** Warrior bursts consume 20 stamina; low stamina halves burst effect. Rogues/hunters use less stamina per dash due to agility perks.  
- **Armor Weight:** Each armor piece adds weight; quality/rarity tags (mithril, enchanted leather) reduce penalties. Physique stats increase carry capacity, reducing slowdowns.  
- **Haste & Blink:** Spellcasters can cast Haste (temporary +30% speed) or Blink (short-range teleport costing focus/mana) to compensate for slower base movement.

#### Aggregate Movement
- **Armies & Bands** move at the **average of member speeds**, modified by logistical traits (mounts, supply wagons, reinforced boots). Slowest units drag the formation unless they ride transports.  
- Specialized officers can spend focus to “urge” the formation, briefly boosting speed at elevated stamina/supply cost.

### Mana Regeneration & Debt

Alongside focus, **Mana** fuels spellcasting and miracles. Mana pools regenerate over in-game days rather than combat rounds.

```
ManaRegenPerDay = clamp( (Will + Wisdom + Faith) / 3, 0, 100 )
```

- Mana scales 0–100%. A caster with middling stats might regenerate ~40% of their pool each day; prodigies can reach 100% regen/day.  
- Entities can **overspend up to 200%** of their mana pool (entering debt). This enables desperate spellcasting but carries risk.  
- **Mana Debt Thresholds**:  
  - 0–50% debt: Mild exhaustion, slower regen.  
  - 50–99% debt: Severe fatigue, reduced focus/morale.  
  - **100% debt**: Entity collapses into a **3-day coma**, requiring medical workers or family caretakers. Without tending they risk death or permanent debuffs.  
- **Alleviation:** Master healers or high-tier casters can downgrade the coma (e.g., to 1 day or severe migraine) by spending focus/mana on restoration rituals. Legendary support casters may prevent coma entirely if they intervene immediately.
- Caretakers must supply food, hydration, and basic medicine; miracles can shorten coma duration.  
- Design intent: allow high-stakes spellcasting while tying consequences into the healthcare and social systems.

### Faith Density & Miracle Efficiency

Believing entities project **faith fields** that influence miracle casting nearby.

- Each faithful villager within the miracle radius reduces the effective miracle cost or casting strain by a stacking percentage, up to a **50% reduction cap**.  
- Base formula: `MiracleCostModifier = clamp(1 - (FaithfulCount × 0.02), 0.5, 1)` for standard miracles.  
- **Modifiers**: Certain alignment traits, relics, or miracles can raise or lower the cap (e.g., holy relic +20% cap, cynicism debuff -10%).  
- **Trade-offs**: Some modifiers instead boost other stats (damage, duration) while reducing cost reduction effectiveness. Designers can tune per miracle.  
- Intent: Encourage players to gather worshippers near rites to gain efficiency, while hostile influences (desecrated ground, evil alignment) can dampen the benefit.

**Focus Regeneration:**
```
Base Regeneration: +10 Focus per round

Modifiers:
  - Meditating: +20 Focus per round (not in combat)
  - Resting: +15 Focus per round
  - In Combat: +10 Focus per round (standard)
  - Exhausted (Stamina 0): +5 Focus per round (slowed regen)
  - Panicked (Morale < 30): +5 Focus per round (can't concentrate)

Full Recovery Time:
  0 to 100 Focus: 10 rounds (50 seconds) at base regen
  0 to 100 Focus: 5 rounds (25 seconds) while meditating
```

---

## Focus Abilities by Class

### Warrior Focus Abilities

**Cleaving Strike (Multi-Target Attack):**
```
Focus Cost: 20 per additional target (baseline; see scaling below)
Effect: Attack up to 5 melee targets with standard weapons, or up to 7 when wielding polearms / great weapons.
Duration: Single action

Example:
  Warrior surrounded by 3 enemies
  → Spends 40 Focus (20 × 2 additional targets)
  → Attacks all 3 enemies in one strike
  → Each attack at -10% hit chance (divided attention)

Damage:
  Primary target: Full damage
  Secondary targets: 80% damage
  Tertiary targets: 60% damage

Scaling Rules:
  - Focus Allocation governs damage share: `DamageShare = BaseDamage × (FocusAllocated / FocusPerTarget)`.
  - Maximum simultaneous targets = 5 (standard) or 7 (great weapons) but only if FocusPool and Physique allow.
  - Physique (Strength/Constitution) caps raw output: `PerTargetDamage ≤ PhysiqueScore / Targets`.

High Focus Example:
  Champion with 500 Focus and Strength 100 attempts to cleave 5 foes.
    → Allocates 100 Focus per foe (full damage).
    → Physique limit: 100 Strength ⇒ 100/5 = 20 damage cap per foe unless weapon multipliers apply.
  Great-weapon exception:
    → Witch-king wielding cursed halberd, Focus 500, Strength 200.
    → Can commit 70 Focus per target across 7 foes, each taking ~28 damage before weapon multipliers (Strength 200/7 ≈ 28).
    → Legendary artifacts (ring of power) can raise both Focus cap and Physique threshold, enabling army-cleaving “entity reaper” strikes.
```

**Critical Strike Focus:**
```
Focus Cost: 15 per attack
Effect: +20% Critical Hit Chance for single attack
Duration: Single action

Example:
  Warrior: Base Crit Chance 15%
  → Spends 15 Focus
  → Crit Chance: 15% + 20% = 35%
  → Roll for critical strike

Synergy:
  Can combine with Cleaving (45 Focus total)
  → Cleave 3 targets with +20% crit on all
```

**Defensive Stance Focus:**
```
Focus Cost: 10 per round
Effect: +30% Defense, -50% damage taken
Duration: Sustained (costs 10 Focus per round)

Example:
  Warrior with 64 FocusMax
  → Can sustain for 6 rounds before depleting
  → Regenerates +10/round, so net cost -0/round if not using other abilities
  → Indefinite duration if no other Focus abilities used
```

### Rogue Focus Abilities

**Enhanced Stealth:**
```
Focus Cost: 15 per round
Effect: -30% to enemy Perception checks (harder to detect)
Duration: Sustained (costs 15 Focus per round)

Example:
  Rogue: Stealth 70
  Guard: Perception 60

  Without Enhanced Stealth:
    → Perception check: 60 vs 70 = 10% detection chance

  With Enhanced Stealth (spending 15 Focus/round):
    → Perception check: (60 - 30) vs 70 = 30 vs 70 = 0% detection chance
    → Rogue is virtually invisible

Synergy:
  Combine with environmental darkness (+30% stealth)
  → Can walk past guards undetected
```

**Evasion Focus:**
```
Focus Cost: 20 per attack
Effect: Automatically dodge next incoming attack
Duration: Until next attack received

Example:
  Rogue in combat, enemy attacks
  → Rogue spends 20 Focus
  → Attack automatically misses (perfect dodge)
  → Can use once per enemy attack
```

**Backstab Focus:**
```
Focus Cost: 30
Effect: ×3 damage on next attack from stealth
Duration: Single attack

Example:
  Rogue: Attack Damage 40
  → Spends 30 Focus for Backstab
  → Attacks from stealth: 40 × 3 = 120 damage
  → Bypasses 50% armor (assassination technique)
```

**Critical Overload (Lethal Strike):**
```
Focus Cost: 35
Effect: Pushes crit chance beyond 100% (every attack crits, overflow converts into execute damage).
Duration: Single attack

Example:
  Rogue with 60% base crit, 25% gear bonuses.
  → Activates Critical Overload (+40%).
  → Total crit chance 125%; extra 25% becomes bonus true damage after armor.
  → Ideal for assassinating high-value targets or finishing bosses during stealth windows.
```

### Mage Focus Abilities

**Multi-Tool Operation:**
```
Focus Cost: 10 per additional tool
Effect: Operate multiple tools simultaneously
Duration: Sustained (costs Focus per round per tool)

Example:
  Mage controlling 3 magical tools:
    - Tool 1: Constructing building
    - Tool 2: Harvesting crops
    - Tool 3: Mining ore

  Focus Cost: 10 + 10 = 20 per round (2 additional tools beyond first)
  → Mage can sustain for (64 Focus / 20) = 3 rounds before depleting
  → Regenerates +10/round, so net cost -10/round
  → Can sustain indefinitely if using only 1 additional tool (10 Focus/round = regen rate)
```

**Spell Efficiency Focus:**
```
Focus Cost: 15
Effect: -50% Mana cost on next spell
Duration: Single spell

Example:
  Fireball: 30 Mana cost
  → Mage spends 15 Focus
  → Fireball now costs 15 Mana (50% reduction)
  → Useful for expensive spells
```

**Spell Amplification Focus:**
```
Focus Cost: 25
Effect: +50% Spell Power for next spell
Duration: Single spell

Example:
  Mage: SpellPower 70, Fireball damage 50
  → Spends 25 Focus
  → Fireball damage: 50 × 1.5 = 75 damage
```

### Hunter Focus Abilities

**Multi-Shot Focus:**
```
Focus Cost: 15 per additional arrow/bolt
Effect: Fire up to 4 projectiles at separate targets, each incurring -10% accuracy per extra shot.
Scaling: Accuracy penalty reduced by Focus mastery; allocating ≥25 Focus per projectile halves the penalty.

Example:
  Hunter splits 60 Focus across 3 targets (20 each).
  → Fires triple-shot with only -5% accuracy per arrow due to high allocation.
```

**Deadeye Focus:**
```
Focus Cost: 30
Effect: Channel all focus into a single shot, doubling headshot chance and granting +50% armor penetration.
Duration: Next ranged attack.

Example:
  Hunter facing dragon champion allocates full pool to Deadeye.
  → Base headshot chance 25% → becomes 50%; armor penetration bypasses shields.
```

### Elementalist & Paladin Focus Hooks

**Elemental Conjuration:**
```
Focus Cost: 20 upfront + 10 per round per elemental.
Effect: Summon elemental allies; each drains focus over time.
Failure: Dropping below upkeep threshold causes elementals to go feral or dissipate.
```

**Paladin Auras:**
```
Focus Cost: 15 per round per active aura.
Effect: Buff allies (resistance, sanctified weapons, fear shields).
Scaling: Higher Will allows stacking multiple auras; Focus mastery reduces upkeep.
```

### Necromancer Focus Abilities

**Raise Minion:**
```
Focus Cost: 20 per minion (sustained)
Effect: Raise and control undead minion
Duration: Sustained (costs 20 Focus per round per minion)
Maximum Minions: FocusMax / 20

Example:
  Necromancer: FocusMax 64
  → Can control up to 3 minions (64 / 20 = 3.2)
  → Each minion costs 20 Focus/round
  → Total cost: 60 Focus/round
  → Regenerates +10/round, net cost -50/round
  → Minions last (64 Focus / 50 net cost) = 1.28 rounds before depletion
  → Must rest/meditate to sustain long-term

Minion Types:
  - Zombie: 20 Focus, low damage, high HP, slow
  - Skeleton: 15 Focus, medium damage, medium HP, fast
  - Ghost: 25 Focus, high damage, low HP, incorporeal
```

**Command Minion (Tactical Control):**
```
Focus Cost: 10 per command
Effect: Give specific order to minion (attack target, defend position, patrol)
Duration: Until countermanded

Example:
  Necromancer with 3 minions
  → Spends 10 Focus to command Minion 1: "Attack the knight"
  → Spends 10 Focus to command Minion 2: "Defend the gate"
  → Spends 10 Focus to command Minion 3: "Patrol the perimeter"
  → Total: 30 Focus spent
```

**Life Drain Focus:**
```
Focus Cost: 30
Effect: Drain HP from enemy, restore own HP
Duration: Single action

Example:
  Necromancer: HP 60/100
  Enemy: HP 80/120
  → Spends 30 Focus
  → Drains 40 HP from enemy
  → Enemy: HP 40/120
  → Necromancer: HP 100/100 (restored 40 HP)
```

### Monk/Martial Artist Focus Abilities

**Flurry of Blows:**
```
Focus Cost: 25
Effect: Attack 3 times in one round (triple attack speed)
Duration: Single round

Example:
  Monk: Attack Speed 70 (1 attack/round normally)
  → Spends 25 Focus
  → Attacks 3 times in one round
  → Each attack at -5% hit chance (rapid strikes)
```

**Iron Skin Focus:**
```
Focus Cost: 20 per round
Effect: +50% Armor (damage reduction) for duration
Duration: Sustained (costs 20 Focus per round)

Example:
  Monk: Armor 20 (20% damage reduction)
  → Spends 20 Focus/round
  → Armor: 20 + (20 × 0.5) = 30 (30% damage reduction)
```

---

## Multi-Focus System

Some entities can focus on multiple targets/tasks simultaneously. This is a learned ability.

**Multi-Focus Capacity:**
```
Base Capacity: 1 target/task (most entities)
High Finesse (80+): 2 targets/tasks
Legendary Finesse (90+): 3 targets/tasks
Multi-Focus Lesson (Rare, Tier 7): +1 target/task

Example:
  Warrior: Finesse 85, learned Multi-Focus Lesson
  → Can focus on 2 (Finesse 80+) + 1 (Lesson) = 3 targets
  → Can cleave 4 enemies total (primary + 3 additional)
  → Focus cost: 20 × 3 = 60 Focus
```

**Multi-Task Examples:**

**Mage Operating Multiple Tools:**
```
Mage: Finesse 75, Intelligence 90, Multi-Focus Capacity 2
→ Can operate 2 tools simultaneously
→ Focus cost: 10 Focus/round per additional tool
→ Total cost: 10 Focus/round

If mage learns Multi-Focus Lesson:
→ Multi-Focus Capacity becomes 3
→ Can operate 3 tools simultaneously
→ Focus cost: 20 Focus/round
```

**Warrior Fighting Multiple Enemies:**
```
Warrior: Finesse 90, Multi-Focus Capacity 3
→ Can attack 3 enemies simultaneously (cleave)
→ Focus cost: 20 × 2 = 40 Focus (2 additional targets beyond primary)
→ Each attack at -10% hit chance
```

**Necromancer Controlling Minions:**
```
Necromancer: FocusMax 80, Multi-Focus Capacity 2
→ Can control 4 minions total (80 / 20 = 4)
→ But can only give tactical commands to 2 minions simultaneously
→ Other minions follow last command until new command given
```

---

## Focus Maximum Increase

Focus maximum increases with usage, similar to how muscles grow with exercise.

**Usage-Based Increase:**
```
Every 100 Focus spent → +1 FocusMax (permanent)
Capped at 100 FocusMax

Example:
  Rogue starts with FocusMax 50 (Finesse 70, Int 40, Will 30)
  → Uses Enhanced Stealth for 100 rounds (15 Focus/round)
  → Total Focus spent: 1500
  → FocusMax increases: 1500 / 100 = 15 points
  → New FocusMax: 50 + 15 = 65

Tracking:
  FocusSpentTotal: 1500
  FocusMaxIncreases: 15
  CurrentFocusMax: 65

Cap:
  FocusMax cannot exceed 100
  Example: Entity with FocusMax 95 spends 1000 Focus
  → Would gain +10, but capped at 100
  → FocusMax becomes 100 (max cap)
```

**Health, Stamina, Mana Increase Similarly:**
```
Health: Every 500 damage taken → +1 MaxHealth (capped at 200)
Stamina: Every 50 rounds of combat → +1 MaxStamina (capped at 20)
Mana: Every 500 Mana spent → +1 MaxMana (capped at 200)

Example:
  Warrior: Health 116, takes 2000 damage over career
  → Health increases: 2000 / 500 = 4 points
  → New MaxHealth: 116 + 4 = 120

This creates veteran entities with increased resource pools
```

---

## Status Effects System

Statuses are conditions affecting entities, either temporarily or permanently.

### Status Categories

**1. Limb-Specific Statuses:**
```
Affect individual body parts (arms, legs, head, torso)

Examples:
  - Broken Arm: -50% Attack, cannot dual-wield, -30% Defense
  - Bleeding Leg: -20% Movement Speed, -5 HP per round until healed
  - Crushed Hand: Cannot wield weapon in that hand, -40% Finesse checks
  - Blinded Eye: -30% Perception, -20% Accuracy
  - Concussed Head: -20% Intelligence, -15% Perception, dizzy (10% miss chance)
```

**2. Whole-Body Statuses:**
```
Affect entire entity

Examples:
  - Diamond Skin: +50% Armor, -20% Movement Speed (magically hardened skin)
  - Superior Intellect: +30 Intelligence, +20% learning speed (magical enhancement)
  - Unstoppable Movement: Cannot be slowed, rooted, or knocked down (berserker rage)
  - Invisibility: Cannot be seen, +80% Stealth (magical effect)
  - Regeneration: +10 HP per round (troll blood, magical healing)
```

---

## Status Effects Catalog

### Combat Statuses

**Bleeding (Limb-Specific):**
```
Cause: Slashing weapons, critical hits
Duration: Until healed (permanent if untreated)
Effect:
  - Light Bleeding: -2 HP per round
  - Moderate Bleeding: -5 HP per round
  - Heavy Bleeding: -10 HP per round

Affected Limb:
  - Arm: -20% Attack if using that arm
  - Leg: -20% Movement Speed
  - Torso: -10% Stamina regeneration

Treatment:
  - Bandage: Stops light/moderate bleeding
  - Medical attention: Stops heavy bleeding
  - Healing magic: Instant stop
```

**Broken Bone (Limb-Specific):**
```
Cause: Blunt weapons, falls, crushing damage
Duration: 30-60 days to heal naturally, 1 day with magic
Effect:
  - Broken Arm: -50% Attack, cannot dual-wield, -30% Defense
  - Broken Leg: -50% Movement Speed, -30% Defense (poor stance)
  - Broken Ribs: -20% Stamina, -10% Health (painful breathing)

Pain Modifier:
  - +20 pain threshold (intense pain)
  - -20% Morale (fighting while injured)
```

**Poisoned (Whole-Body):**
```
Cause: Poisoned weapons, venomous creatures
Duration: 10-50 rounds (depends on poison strength)
Effect:
  - Weak Poison: -5 HP per round, -10% Attack
  - Strong Poison: -10 HP per round, -20% Attack, -10% Defense
  - Deadly Poison: -20 HP per round, -40% all stats, paralysis

Treatment:
  - Antidote: Stops poison immediately
  - Wait it out: Poison wears off after duration
  - Healing magic: Removes poison
```

**Stunned (Whole-Body):**
```
Cause: Blunt weapons, critical hits, magic
Duration: 1-3 rounds
Effect:
  - Cannot attack or defend (Defense = 0)
  - Vulnerable to attacks (+50% damage taken)
  - Cannot use Focus abilities

Recovery:
  - Automatic after duration
  - Will check to recover early (d100 < Will)
```

### Enhancement Statuses

**Diamond Skin (Whole-Body):**
```
Cause: Magical spell, rare mutation, alchemy
Duration: 10-30 rounds (spell) or permanent (mutation)
Effect:
  - +50% Armor (physical damage reduction)
  - +30% Defense (harder to hit)
  - -20% Movement Speed (heavy, rigid skin)
  - Immune to bleeding

Synergy:
  - Heavy armor wearers become nearly invulnerable
  - Trade-off: Slow, cannot dodge effectively
```

**Superior Intellect (Whole-Body):**
```
Cause: Magical enhancement, rare mutation, divine blessing
Duration: 30 rounds (spell) or permanent (mutation)
Effect:
  - +30 Intelligence
  - +20% SpellPower
  - +20% learning speed (faster lesson absorption)
  - +15% Mana regeneration

Example:
  Mage: Intelligence 70, SpellPower 60
  → Gains Superior Intellect
  → Intelligence: 70 + 30 = 100
  → SpellPower: 60 + (60 × 0.2) = 72
```

**Unstoppable Movement (Whole-Body):**
```
Cause: Berserker rage, divine blessing, magic
Duration: 5-15 rounds (rage) or permanent (blessing)
Effect:
  - Cannot be slowed, rooted, or knocked down
  - Immune to terrain penalties (move through rough terrain at full speed)
  - +30% Movement Speed
  - Ignore pain (fight at 100% effectiveness even at 1 HP)

Trade-off:
  - -20% Defense (reckless movement)
  - Cannot yield (fight to death)
```

**Invisibility (Whole-Body):**
```
Cause: Magic spell, rare potion, rogue ability
Duration: 10-20 rounds (spell) or permanent (rare mutation)
Effect:
  - Cannot be seen by normal vision
  - +80% Stealth (virtually undetectable)
  - Enemies cannot target (must guess location)

Breaking Invisibility:
  - Attacking breaks invisibility (visible for 1 round)
  - Taking damage breaks invisibility
  - Loud noise (running, shouting) reduces Stealth to +40%
```

**Regeneration (Whole-Body):**
```
Cause: Troll blood, magical item, rare mutation
Duration: Permanent (mutation) or 20-40 rounds (potion)
Effect:
  - +10 HP per round (regenerate health)
  - Cannot die from bleeding (regenerates faster than bleed)
  - Broken bones heal in 5 rounds instead of 30 days

Weakness:
  - Fire damage stops regeneration for 3 rounds
  - Decapitation prevents regeneration (instant death)
```

### Debuff Statuses

**Cursed (Whole-Body):**
```
Cause: Dark magic, cursed items, divine punishment
Duration: Permanent until dispelled
Effect:
  - -20% to all stats (Attack, Defense, Health, Stamina, etc.)
  - -30% luck (increased chance of negative events)
  - Social penalty (-40 relation with all entities)

Removal:
  - Dispel magic spell (requires SpellPower 70+)
  - Divine intervention (prayer to god)
  - Complete quest to lift curse
```

**Exhausted (Whole-Body):**
```
Cause: Stamina reaches 0, overexertion
Duration: Until rest (10 rounds resting)
Effect:
  - -40% Attack, -40% Defense, -50% Movement Speed
  - Cannot use Focus abilities (Focus regeneration -50%)
  - -20% Morale (too tired to fight effectively)

Recovery:
  - Rest for 10 rounds: Full recovery
  - Sleep: Instant recovery
  - Stamina potion: Instant recovery
```

**Frightened (Whole-Body):**
```
Cause: Intimidation, witnessing horror, low Morale
Duration: 5-10 rounds or until threat removed
Effect:
  - -30% Attack, -20% Defense
  - 50% chance to flee each round (Morale check)
  - Cannot use offensive Focus abilities (too scared)

Recovery:
  - Morale check each round (d100 < Morale)
  - Rally from ally (+20 Morale for check)
  - Kill source of fear (instant recovery)
```

**Paralyzed (Whole-Body):**
```
Cause: Poison, magic, critical nerve damage
Duration: 3-10 rounds
Effect:
  - Cannot move, attack, or defend (Defense = 0)
  - Fully vulnerable (+100% damage taken)
  - Cannot use any abilities

Recovery:
  - Will check each round (d100 < Will) to break paralysis
  - Healing magic: Instant recovery
  - Wait out duration
```

### Permanent Injuries

**Crippled Limb (Limb-Specific):**
```
Cause: Permanent damage, amputation, severe injury
Duration: Permanent
Effect:
  - Crippled Arm: -50% Attack, cannot dual-wield, -40% Finesse
  - Crippled Leg: -50% Movement Speed, -30% Defense
  - Lost Eye: -40% Perception, -30% Accuracy, -20% Defense

Adaptation:
  - Entity can learn to compensate over time
  - After 100 combats: Reduce penalties by 20%
  - After 500 combats: Reduce penalties by 40%
  - Example: Crippled arm -50% Attack → -30% Attack (veteran)
```

**Scarred (Limb-Specific or Whole-Body):**
```
Cause: Severe injury, fire, acid
Duration: Permanent
Effect:
  - Minor Scar: -5% Charisma (social penalty)
  - Major Scar: -15% Charisma, intimidation bonus (+10% vs Craven)
  - Disfigured: -40% Charisma, cannot seduce, +20% intimidation

Narrative Impact:
  - Scars tell story of entity's battles
  - Can increase reputation as veteran warrior
  - May affect marriage prospects, diplomacy
```

---

## DOTS Components (Unity ECS)

### FocusStats Component

```csharp
public struct FocusStats : IComponentData
{
    public byte CurrentFocus;               // 0-100 (current focus available)
    public byte MaxFocus;                   // 0-100 (maximum focus capacity)
    public ushort FocusSpentTotal;          // Total focus spent (for max increase)
    public byte FocusMaxIncreases;          // Times FocusMax has increased

    // Regeneration
    public byte FocusRegenRate;             // Focus per round (default 10)
    public bool IsMeditating;               // +20 regen if true

    // Multi-focus
    public byte MultiFocusCapacity;         // Number of simultaneous focuses
    public byte CurrentFocusTargets;        // How many targets/tasks currently focused on
}
```

### FocusAbility Component

```csharp
public struct FocusAbility : IComponentData
{
    public FixedString64Bytes AbilityName; // "Cleaving Strike", "Enhanced Stealth"
    public FocusAbilityType Type;           // Warrior, Rogue, Mage, etc.
    public byte FocusCost;                  // Focus cost per use
    public bool IsSustained;                // True if costs Focus per round
    public byte Duration;                   // Rounds (0 = instant)

    // Effect
    public FocusEffect Effect;              // What the ability does
    public short EffectValue;               // Magnitude (+30% Defense, etc.)

    public bool IsActive;                   // Currently active (for sustained abilities)
    public float ActivationTimestamp;       // When activated
}

public enum FocusAbilityType : byte
{
    Warrior = 0,
    Rogue = 1,
    Mage = 2,
    Necromancer = 3,
    Monk = 4,
    // ... more classes
}

public enum FocusEffect : byte
{
    MultiTarget = 0,                // Cleaving
    CriticalBoost = 1,              // Increased crit chance
    DefenseBoost = 2,               // Increased defense
    StealthBoost = 3,               // Enhanced stealth
    Evasion = 4,                    // Auto-dodge
    SpellEfficiency = 5,            // Reduced mana cost
    ControlMinion = 6,              // Raise/command undead
    MultiTool = 7,                  // Operate multiple tools
    // ... more effects
}
```

### StatusEffect Component

```csharp
public struct StatusEffect : IComponentData
{
    public FixedString64Bytes StatusName;   // "Bleeding", "Diamond Skin"
    public StatusType Type;                 // Buff, Debuff, Injury, Enhancement
    public StatusScope Scope;               // Limb-Specific or Whole-Body
    public Limb AffectedLimb;               // Which limb (if limb-specific)

    // Effect
    public StatusEffectType Effect;         // What stat is modified
    public short EffectValue;               // Magnitude (-50% Attack, +30 Intelligence)
    public bool IsPercentage;               // True if percentage modifier

    // Duration
    public bool IsPermanent;                // True if cannot be removed naturally
    public ushort DurationRemaining;        // Rounds remaining (0 = instant, 65535 = permanent)
    public float AppliedTimestamp;          // When status was applied

    // Source
    public Entity AppliedBy;                // Who/what applied this status (or Entity.Null)
}

public enum StatusType : byte
{
    Buff = 0,
    Debuff = 1,
    Injury = 2,
    Enhancement = 3,
    Mutation = 4
}

public enum StatusScope : byte
{
    WholeBody = 0,
    LimbSpecific = 1
}

public enum Limb : byte
{
    None = 0,           // Whole-body
    Head = 1,
    Torso = 2,
    LeftArm = 3,
    RightArm = 4,
    LeftLeg = 5,
    RightLeg = 6,
    LeftHand = 7,
    RightHand = 8
}

public enum StatusEffectType : byte
{
    Attack = 0,
    Defense = 1,
    Health = 2,
    Stamina = 3,
    MovementSpeed = 4,
    Intelligence = 5,
    Perception = 6,
    Stealth = 7,
    Armor = 8,
    Morale = 9,
    // ... more stat types
}
```

### StatusEffectModifiers Component (Aggregated)

```csharp
public struct StatusEffectModifiers : IComponentData
{
    // Combat modifiers
    public sbyte AttackModifier;            // Sum of all Attack status effects
    public sbyte DefenseModifier;           // Sum of all Defense status effects
    public sbyte ArmorModifier;             // Sum of all Armor status effects

    // Movement modifiers
    public sbyte MovementSpeedModifier;     // Sum of all Movement Speed effects

    // Stat modifiers
    public sbyte IntelligenceModifier;      // Sum of all Intelligence effects
    public sbyte PerceptionModifier;        // Sum of all Perception effects
    public sbyte StealthModifier;           // Sum of all Stealth effects
    public sbyte MoraleModifier;            // Sum of all Morale effects

    // Special flags
    public bool IsInvisible;                // Invisibility status active
    public bool IsStunned;                  // Stunned status active
    public bool IsParalyzed;                // Paralyzed status active
    public bool HasUnstoppableMovement;     // Unstoppable Movement active
    public bool HasRegeneration;            // Regeneration active
    public sbyte RegenerationRate;          // HP per round (if HasRegeneration)
}
```

---

## Integration with Existing Systems

### Individual Combat System
- **Focus abilities** enhance combat (cleaving, critical strikes, defensive stance)
- **Status effects** modify combat stats (broken arm -50% Attack, poisoned -20% Defense)
- **Injuries** create permanent combat penalties (crippled leg -50% Movement Speed)

### Individual Progression
- **FocusMax increases** with usage (every 100 spent → +1 Max)
- **Health/Stamina/Mana** increase with usage (similar to Focus)
- **Status effects** can grant permanent stat boosts (Superior Intellect +30 Int)

### Stealth & Perception System
- **Rogue Focus** reduces perception checks (-30% with Enhanced Stealth)
- **Invisibility status** grants +80% Stealth
- **Blinded status** reduces Perception by 40%

### Education & Tutoring System
- **Multi-Focus** can be learned as a lesson (Rare, Tier 7)
- **Focus abilities** can be taught by mentors
- **Status removal** can be learned (dispel magic, healing)

### Magic System (Future)
- **Mage Focus** operates multiple tools simultaneously
- **Spell Efficiency** reduces mana cost via Focus
- **Status effects** created by spells (Diamond Skin, Invisibility, Curse)

---

## Summary

The Focus & Status Effects System creates:

- **Focus as a resource** for class-specific special abilities
- **FocusMax increases** with usage (100 spent → +1 Max, capped at 100)
- **Multi-Focus** allows high-Finesse entities to target/task multiple things
- **Limb-specific statuses** (broken arm, bleeding leg) with targeted penalties
- **Whole-body statuses** (diamond skin, superior intellect, unstoppable movement)
- **Permanent injuries** that entities can adapt to over time
- **Health/Stamina/Mana** increase with usage (veteran entities have larger pools)

This creates emergent gameplay:
- Warriors cleaving through hordes at high Focus cost
- Rogues vanishing from sight by burning Focus
- Necromancers managing undead armies with limited Focus
- Mages juggling multiple construction projects
- Entities with permanent injuries becoming legendary veterans
- Status effects creating tactical depth (Diamond Skin tank, Invisible assassin)

---

**Related Documentation:**
- **Individual Combat:** [Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - Focus abilities in combat
- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Focus/Health/Stamina/Mana increases
- **Stealth & Perception:** [../Combat/Stealth_And_Perception_System.md](../Combat/Stealth_And_Perception_System.md) - Rogue Focus abilities
- **Education & Tutoring:** [../Villagers/Education_And_Tutoring_System.md](../Villagers/Education_And_Tutoring_System.md) - Learning Focus abilities

---

**Last Updated:** 2025-11-07
**Status:** Concept Draft - Core vision captured
