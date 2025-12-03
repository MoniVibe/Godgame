# Languages & Magic System

**Status:** Concept - Core System Design
**Category:** Core - Communication, Magic, Learning
**Complexity:** Complex
**Created:** 2025-12-02
**Last Updated:** 2025-12-02

---

## Overview

**Purpose:** Define how entities communicate, learn languages, and access magical abilities through linguistic mastery
**Player Impact:** Cultural depth through language barriers, emergent magic specialization, strategic language learning decisions
**System Role:** Foundation for communication, magic casting, cultural identity, and knowledge transfer

### Core Philosophy

**Language as Culture:** Languages are tied to cultures. Each culture develops linguistic traditions that enable unique magical expressions.

**Mastery Gates Power:** Basic communication is accessible, but true power (Words of Power, ritual crafting) requires complete mastery (200% fluency).

**Might and Magic Coexist:** The system serves both magical traditions (spell signs, rituals) and martial ones (war cries, battle hymns, rousing speeches).

**Learning Through Living:** Entities learn languages naturally through communication, with rates affected by age, intelligence, wisdom, intentions, and cultural attitudes (xenophobic/xenophilic axis).

---

## Language System

### Language Proficiency Scale

**0% - 200% Fluency System:**

- **0%:** Never heard this language before (complete ignorance)
- **1-25%:** Recognizes sounds, basic words ("hello," "yes," "no," directional commands)
- **26-50%:** Simple conversations, present tense, common nouns/verbs
- **51-75%:** Intermediate proficiency, can discuss complex topics with errors
- **76-99%:** Advanced fluency, idiomatic expressions, cultural nuances
- **100%:** Native-level fluency, full comprehension and expression
- **101-150%:** Academic mastery, understands archaic forms, regional dialects, etymology
- **151-199%:** Scholarly expertise, can analyze linguistic structure, teach effectively
- **200%:** Complete mastery, linguistic perfection, **unlocks Words of Power**

### Language Learning Mechanics

#### Learning Rate Formula

```
LearningRate = BaseRate × AgeMultiplier × (Intelligence + Wisdom)/2 × IntentionModifier × XenoModifier × ExposureFrequency
```

**Age Multiplier:**
- **0-5 years:** 2.0× (childhood language acquisition window)
- **6-15 years:** 1.5× (adolescent learning advantage)
- **16-30 years:** 1.0× (adult baseline)
- **31-50 years:** 0.75× (gradual decline)
- **51+ years:** 0.5× (significantly harder in later life)

**Intention Modifier:**
- **Hostile:** 0.5× (learns slower, resistant to adopting enemy language)
- **Neutral/Trade:** 1.0× (baseline commercial/diplomatic learning)
- **Friendly:** 1.25× (eager to connect, motivated learning)
- **Cultural Immersion:** 1.5× (living among speakers, adopting culture)

**Xeno Modifier (Xenophobic ↔ Xenophilic Axis):**
- **Highly Xenophobic:** 0.3× (actively resists foreign languages)
- **Moderately Xenophobic:** 0.6× (reluctant learner)
- **Neutral:** 1.0× (baseline)
- **Moderately Xenophilic:** 1.4× (attracted to foreign cultures)
- **Highly Xenophilic:** 1.8× (passionate about learning other languages)

**Exposure Frequency:**
- **Rare Contact (monthly):** 0.1× progress per encounter
- **Occasional Contact (weekly):** 0.3× progress
- **Regular Contact (daily):** 1.0× baseline progress
- **Immersion (constant):** 2.0× accelerated progress

#### Mastery Plateau

**100% → 200% is exponentially harder:**
- Requires deliberate study (not just exposure)
- Linguistic research (ancient texts, etymology, dialectology)
- Teaching others (deepens own understanding)
- Magical experimentation (Words of Power research)
- Typical timeline: 5-20 in-game years beyond native fluency

### Language and Culture

**Cultural Languages:**
Each culture has primary language(s):
- **Forest Tribes:** Sylvan dialects (nature-focused vocabulary, musical tones)
- **Mountain Clans:** Dwarven tongues (harsh consonants, metallurgy terms)
- **Coastal Merchants:** Trade Common (simplified grammar, borrowed words from many cultures)
- **Ancient Mages:** High Arcane (dead language, preserved in magical texts)
- **Nomadic Bands:** Wanderer's Cant (cryptic, slang-heavy, rapid evolution)

**Magical Potential by Language:**
Some languages are inherently more conducive to magic:
- **High Arcane:** 1.5× magical effectiveness (designed for spellcasting)
- **Draconic:** 1.3× power, 1.5× cost (raw magical potency, dangerous)
- **Celestial:** 1.2× healing/protection, 0.8× destructive magic
- **Infernal:** 1.2× destructive/corrupting magic, alignment consequences
- **Common:** 1.0× baseline (functional but not optimized)

---

## Spell Signs

### Overview

**Spell Signs** are focus-based magical abilities manifested through gestures, symbols, or spoken/sung components. They bridge language and magic, allowing entities to shape reality through learned patterns.

### Types of Spell Signs

#### Cantrips (Minor Spell Signs)

**Characteristics:**
- Low focus cost (1-5 focus per cast)
- Quick casting (instant or 1-3 seconds)
- Subtle effects (light a candle, clean a surface, sense magic nearby)
- Can be learned at 50%+ language proficiency
- Unlimited daily uses (focus-permitting)

**Examples:**
- **Luminos** (Arcane): Create faint light (1 focus, 10-minute duration)
- **Frigus Tactus** (Arcane): Chill small object (2 focus, instant)
- **Veritas Auris** (Celestial): Sense if last statement was truthful (3 focus, instant)
- **Ignis Digitus** (Common): Light small flame on fingertip (1 focus, 1-minute duration)

#### Spell Signs (Standard)

**Characteristics:**
- Moderate focus cost (10-30 focus per cast)
- Casting time: 5-30 seconds
- Significant effects (combat spells, healing, utility)
- Requires 75%+ language proficiency
- Limited by focus pool and cooldowns

**Examples:**
- **Sagitta Ignis** (Arcane): Fire bolt (15 focus, 3-second cast, 2d6 fire damage)
- **Scutum Luminis** (Celestial): Light shield (20 focus, 10-second cast, absorbs 50 damage, 5-minute duration)
- **Catena Ferrum** (Dwarven): Summon iron chains to bind target (25 focus, 8-second cast, STR check to break)
- **Ventus Velox** (Sylvan): Tailwind speed boost (18 focus, 5-second cast, +30% movement for 2 minutes)

#### Sustained Spell Signs

**Characteristics:**
- Initial focus cost + ongoing drain (e.g., 20 focus cast, 5 focus/minute sustain)
- Maintained through concentration (occupies focus capacity)
- Can be interrupted by damage, silence, or focus exhaustion
- Requires 85%+ language proficiency

**Examples:**
- **Lux Perpetua** (Arcane): Sustained bright light (10 focus initial, 2 focus/minute sustain)
- **Sensus Magicae** (Arcane): Detect magic auras (15 focus initial, 3 focus/minute sustain)
- **Fortitudo Corporis** (Celestial): Sustained strength buff (25 focus initial, 5 focus/minute, +20% STR)

#### Spell Modifiers System

**Spell Modifiers** are linguistic techniques learned through language mastery that alter how spells function. They are **language-gated** and shared between might and magic abilities.

**Core Concept:**
- **Base Spell:** "Silentium" (Silence)
- **Modifier Applied:** "Locus" (Area effect)
- **Modified Spell:** "Silentium Locus" (Area Silence)

**Learning Modifiers:**
- Each language can have its own version of modifiers (Arcane "Locus" vs. Draconic "Lokaal")
- Easier to learn modifiers in languages you already know (cross-language transfer)
- Learning difficulty offset by Intelligence/Wisdom and Might/Magic affinity (depending on modifier nature)
- Modifier quality (effectiveness) based on:
  - Entity's language skill in the casting language
  - Intelligence/Wisdom levels
  - Might/Magic affinity alignment with modifier type

**Modifier Availability:**
- **Might modifiers** (Fortis, Celer, Multiplex) easier for might-aligned entities to learn/use
- **Magic modifiers** (Arcana, Aetheria, Anima) easier for magic-aligned entities to learn/use
- **Universal modifiers** (Locus, Minor, Remotus) equally accessible to both

### Universal Modifiers

**Target/Area Modifiers:**
- **Locus** (Area/Zone): Converts single-target to area-of-effect
  - Cost: +150% focus, Duration: unchanged, Effect: 10m radius
  - Example: "Ignis Locus" (area fire), "Fortitudo Locus" (area strength buff)

- **Multiplex** (Multiple/Chain): Affects multiple discrete targets (chains between them)
  - Cost: +100% focus per target, Duration: unchanged, Effect: 3-5 targets
  - Example: "Sagitta Multiplex" (chain lightning), "Curatio Multiplex" (heal party)

- **Singularis** (Single/Focused): Concentrates area spell into single powerful strike
  - Cost: -30% focus, Duration: unchanged, Effect: +50% potency on single target
  - Example: "Ignis Locus Singularis" (condense fireball into piercing beam)

**Range Modifiers:**
- **Remotus** (Distant/Far): Increases range significantly
  - Cost: +50% focus, Cast time: +2 seconds, Effect: +200% range
  - Example: "Sagitta Remotus" (long-range bolt), "Sensus Remotus" (distant scrying)

- **Proximus** (Near/Close): Decreases range but increases power
  - Cost: -20% focus, Cast time: -1 second, Effect: +30% potency, range halved
  - Example: "Ignis Proximus" (point-blank explosion), "Fortitudo Proximus" (touch-range powerful buff)

**Timing Modifiers:**
- **Celer** (Swift/Quick): Reduces cast time
  - Cost: +25% focus, Cast time: -50%, Effect: unchanged
  - Example: "Scutum Celer" (quick shield), "Sagitta Celer" (rapid-fire bolt)

- **Perpetua** (Sustained/Continuous): Converts instant spell to sustained
  - Cost: 50% upfront + 20% per minute sustain, Duration: sustained, Effect: continuous
  - Example: "Ignis Perpetua" (sustained flame), "Fortitudo Perpetua" (maintained buff)

- **Instantis** (Instant/Burst): Converts sustained spell to instant burst
  - Cost: +200% focus, Duration: instant, Effect: all power delivered at once
  - Example: "Lux Perpetua Instantis" (blinding flash instead of sustained light)

- **Momentum** (Delayed/Triggered): Spell activates after delay or on trigger condition
  - Cost: +30% focus, Duration: delayed (1-60 seconds), Effect: detonates on trigger
  - Example: "Ignis Momentum" (delayed fireball trap), "Scutum Momentum" (shield activates when struck)

**Power Modifiers:**
- **Magnus** (Greater/Amplified): Increases spell potency
  - Cost: +100% focus, Cast time: +50%, Effect: +50% potency
  - Example: "Sagitta Magnus" (devastating bolt), "Curatio Magnus" (major healing)

- **Minor** (Lesser/Reduced): Reduces power and cost (practice/efficiency mode)
  - Cost: -50% focus, Cast time: -30%, Effect: -40% potency
  - Example: "Ignis Minor" (candle flame), "Fortitudo Minor" (minor buff for training)

- **Maximus** (Maximum/Overwhelming): Ultimate power, reckless
  - Cost: +300% focus, Cast time: +100%, Effect: +150% potency, risk of backfire
  - Example: "Vastitas Maximus" (cataclysmic destruction), "Curatio Maximus" (resurrection-tier healing)

**Duration Modifiers:**
- **Tempus** (Extended Time): Increases duration significantly
  - Cost: +75% focus, Duration: +200%, Effect: unchanged
  - Example: "Scutum Tempus" (long-lasting shield), "Fortitudo Tempus" (hour-long buff)

- **Brevis** (Brief/Short): Reduces duration, increases power
  - Cost: -30% focus, Duration: -60%, Effect: +20% potency
  - Example: "Fortitudo Brevis" (intense short-term burst of strength)

### Magic-Aligned Modifiers

**Elemental Modifiers:**
- **Glacies** (Ice/Frozen): Adds cold damage/slow effect
  - Cost: +40% focus, Effect: base damage + cold DoT + 20% slow
  - Example: "Sagitta Glacies" (frost bolt), "Locus Glacies" (freeze zone)

- **Ignis** (Fire/Burning): Adds fire damage/burn effect
  - Cost: +40% focus, Effect: base damage + fire DoT
  - Example: "Sagitta Ignis" (fire bolt), "Fortitudo Ignis" (flaming weapon buff)

- **Fulgur** (Lightning/Electric): Adds lightning damage/stun effect
  - Cost: +50% focus, Effect: base damage + 10% stun chance + chain potential
  - Example: "Sagitta Fulgur" (lightning bolt), "Scutum Fulgur" (shocking shield)

- **Venenum** (Poison/Toxic): Adds poison damage over time
  - Cost: +35% focus, Effect: base damage + poison DoT (10 seconds)
  - Example: "Sagitta Venenum" (poison dart), "Locus Venenum" (toxic cloud)

- **Terrum** (Earth/Stone): Adds physical damage/knockdown effect
  - Cost: +45% focus, Effect: base damage + 15% knockdown chance
  - Example: "Sagitta Terrum" (stone spike), "Locus Terrum" (earthquake)

**Arcane Modifiers:**
- **Penetrans** (Piercing/Penetrating): Ignores resistances/armor
  - Cost: +60% focus, Effect: bypasses 50% of target's magical resistance
  - Example: "Sagitta Penetrans" (armor-piercing bolt), "Silentium Penetrans" (unresistable silence)

- **Reflectus** (Reflected/Bounced): Spell bounces between targets
  - Cost: +80% focus, Effect: bounces 3-5 times, -20% damage per bounce
  - Example: "Sagitta Reflectus" (ricocheting bolt), "Curatio Reflectus" (bouncing heal)

- **Anima** (Soul/Essence): Drains target's focus instead of dealing damage
  - Cost: +40% focus, Effect: steals focus from target (focus vampire)
  - Example: "Sagitta Anima" (focus-draining bolt), "Locus Anima" (area focus drain)

- **Aetheria** (Ethereal/Phase): Spell ignores physical barriers
  - Cost: +70% focus, Effect: passes through walls/shields
  - Example: "Sagitta Aetheria" (phase bolt), "Sensus Aetheria" (see through walls)

**Defensive Modifiers:**
- **Aegis** (Shield/Protection): Converts offensive spell to defensive ward
  - Cost: +50% focus, Effect: creates protective barrier instead of damage
  - Example: "Ignis Aegis" (fire shield that burns attackers), "Glacies Aegis" (ice armor)

- **Reversa** (Reversed/Inverted): Inverts spell effect
  - Cost: +60% focus, Effect: opposite of base spell
  - Example: "Fortitudo Reversa" (weakness curse), "Curatio Reversa" (damage infliction)

**Life/Death Modifiers:**
- **Vita** (Life/Healing): Converts damage spell to healing
  - Cost: +80% focus, Effect: heals instead of harms
  - Example: "Ignis Vita" (warming heal), "Sagitta Vita" (healing bolt)

- **Mortis** (Death/Lethal): Amplifies damage, adds necrotic properties
  - Cost: +90% focus, Effect: +30% damage + prevents healing for 30 seconds
  - Example: "Sagitta Mortis" (death bolt), "Locus Mortis" (death zone)

- **Sanguis** (Blood/Life-Cost): Spell costs HP instead of focus (blood magic)
  - Cost: 0 focus, HP cost equal to 2× focus cost, Effect: +25% potency
  - Example: "Sagitta Sanguis" (blood bolt), "Fortitudo Sanguis" (blood-fueled strength)

### Might-Aligned Modifiers

**Physical Enhancement Modifiers:**
- **Fortis** (Strong/Empowered): Increases physical might of spell/ability
  - Cost: +50% focus, Effect: +40% physical damage/effect
  - Example: "War Cry Fortis" (empowered battle roar), "Fortitudo Fortis" (superior strength buff)

- **Robustus** (Robust/Enduring): Adds physical resistance/toughness
  - Cost: +45% focus, Effect: grants physical damage resistance
  - Example: "Aegis Robustus" (fortified shield), "Hymn Robustus" (endurance chant)

- **Ferrum** (Iron/Unyielding): Grants unbreakable/unstoppable properties
  - Cost: +70% focus, Effect: cannot be interrupted or dispelled
  - Example: "War Cry Ferrum" (unstoppable charge), "Fortitudo Ferrum" (unbreakable will)

**Martial Modifiers:**
- **Bellum** (War/Combat): Optimizes for combat effectiveness
  - Cost: +30% focus, Effect: +25% damage vs. combatants, reduced vs. non-combatants
  - Example: "Sagitta Bellum" (anti-warrior bolt), "Hymn Bellum" (battle hymn)

- **Gladius** (Blade/Weapon): Channels through melee weapon
  - Cost: +40% focus, Effect: next melee strike carries spell effect
  - Example: "Ignis Gladius" (flaming weapon strike), "Fortitudo Gladius" (empowered slash)

- **Scutum** (Shield/Defense): Channels through shield
  - Cost: +35% focus, Effect: shield bash/block carries spell effect
  - Example: "Ignis Scutum" (burning shield bash), "Fortitudo Scutum" (empowering block)

**Group/Formation Modifiers:**
- **Legio** (Legion/Group): Shares effect among squad/unit
  - Cost: +120% focus, Effect: affects entire formation (10-20 allies)
  - Example: "Fortitudo Legio" (army-wide strength), "Hymn Legio" (legion chant)

- **Cohors** (Cohort/Team): Shares effect among small group
  - Cost: +60% focus, Effect: affects party (3-6 allies)
  - Example: "Fortitudo Cohors" (party buff), "War Cry Cohors" (squad rally)

**Intimidation Modifiers:**
- **Terror** (Terror/Fear): Adds fear/morale damage effect
  - Cost: +55% focus, Effect: damages morale, may cause rout
  - Example: "War Cry Terror" (terrifying roar), "Locus Terror" (area fear)

- **Gloria** (Glory/Inspiration): Adds morale boost/inspiration
  - Cost: +50% focus, Effect: boosts ally morale, intimidates enemies
  - Example: "Hymn Gloria" (glorious anthem), "War Cry Gloria" (inspiring shout)

### Special Case: Silence with Modifiers

**Silence** demonstrates the modifier system:

**Base Spell:**
- **Silentium** (Silence): 20 focus, 5s cast, 30s duration, 20m range, single target
  - Prevents target from speaking

**Modified Variants:**

1. **Silentium Locus** (Area Silence)
   - Applies "Locus" modifier: +150% focus → 50 focus total
   - 10m radius sphere, all entities silenced
   - Requires: 100%+ language proficiency (modifier learning)

2. **Silentium Umbra** (Shadow Silence)
   - Applies "Umbra" modifier (stealth-type, magic-aligned)
   - Umbra: Sustained, moves with caster, +30% stealth bonus
   - Cost: 80 focus initial + 15/min, 5m radius
   - Requires: 120%+ language proficiency, 70+ focus capacity

3. **Silentium Penetrans** (Piercing Silence)
   - Applies "Penetrans" modifier: +60% focus → 32 focus total
   - Ignores silence resistance/anti-silence items
   - Critical for countering protected targets

4. **Silentium Tempus** (Extended Silence)
   - Applies "Tempus" modifier: +75% focus → 35 focus total
   - Duration: 90 seconds (3× base)
   - Useful for long-term prisoner interrogation preparation

5. **Silentium Celer** (Quick Silence)
   - Applies "Celer" modifier: +25% focus → 25 focus total
   - Cast time: 2.5 seconds (half of base)
   - Emergency counter-spell application

**Combining Multiple Modifiers:**
- **Silentium Locus Perpetua** (Sustained Area Silence)
  - Locus (+150%) + Perpetua (converts to sustained)
  - Cost: 75 focus initial + 15 focus/min
  - Effect: 10m radius sustained silence zone
  - Requires: 140%+ proficiency, high focus capacity

- **Silentium Umbra Remotus** (Distant Shadow Silence)
  - Umbra (stealth) + Remotus (range)
  - Cost: 120 focus initial + 15/min
  - Effect: 5m stealth bubble, castable at 40m range
  - Requires: 150%+ proficiency, expert assassin

**Stealth/Assassination Applications:**
- **Requirements for expert use:**
  - High stealth skill (80+)
  - Mastery of Silentium spell (500+ casts)
  - Mastery of Locus/Umbra modifiers (200+ uses each)
  - Focus capacity 70%+

**Perception Mechanics Integration:**

For silence-based stealth takedowns to work, proper perception mechanics must be implemented:

- **Auditory Perception:** Entities detect attacks via sound (cries for help, combat noise, body falling)
- **Visual Perception:** Entities detect attacks via line of sight (seeing ally fall, blood splatter, shadows moving)
- **Alert Propagation:** When entity detects threat, attempts to alert nearby allies (shout, gesture, run to warn)
- **Silence Counters Auditory:** Silenced entities cannot trigger auditory alerts (no screams, no combat noise)
- **Vision Still Active:** Allies with line of sight can still see attacks even if target is silenced
- **Secluded Targets:** Isolated targets (out of sight from allies) can be eliminated silently without triggering alert

**Expert Ambusher Tactics:**

1. **Scout Phase:** Identify isolated targets, map patrol routes, find seclusion zones
2. **Silence Cast:** Apply area silence to eliminate auditory detection
3. **Sequential Elimination:** Take down targets one by one, always from behind/blind spots
4. **Body Concealment:** Hide bodies to prevent visual detection by patrolling allies
5. **Escape Before Discovery:** Exit before silence duration expires or before allies discover missing members

**Counters to Silence Tactics:**

- **Buddy System:** Pairs of guards maintain line of sight, one always watching partner
- **Timed Check-Ins:** Guards must report at intervals; missing report triggers alarm
- **Magical Wards:** Detection spells that trigger on violence/death (independent of sound)
- **Anti-Silence Items:** Enchanted amulets that grant resistance to silence effects
- **Animal Sentries:** Dogs/birds that react to visual cues and can't be silenced (bark/screech regardless)

### Spell Crafting System

**Entities have maximum freedom to craft custom spells** by combining base components and modifiers. Crafted spells become tradeable intellectual property with quality, rarity, and custom names.

#### Base Spell Components

Instead of pre-defined spells, magic is built from **elemental components**:

**Form Components (Shape/Delivery):**
- **Sagitta** (Arrow/Projectile): Linear projectile with range
- **Globus** (Sphere/Ball): Spherical projectile, arcs, explodes on impact
- **Radius** (Circle/Ring): Expanding ring from caster
- **Conus** (Cone): Cone-shaped area emanating from caster
- **Contactus** (Touch): Melee-range touch effect
- **Aspectus** (Gaze/Ray): Thin beam, instant hit, line-of-sight
- **Orbis** (Orb/Floating): Floating orb that orbits caster or follows target
- **Catena** (Chain/Link): Connects two points, persists
- **Murus** (Wall): Vertical plane/barrier
- **Nubes** (Cloud/Mist): Persistent cloud area

**Effect Components (What It Does):**
- **Ignis** (Fire): Burns, damage over time
- **Glacies** (Ice): Freezes, slows movement
- **Fulgur** (Lightning): Shocks, stuns
- **Venenum** (Poison): Poisons, damages over time
- **Lux** (Light): Illuminates, blinds
- **Umbra** (Shadow): Darkens, conceals
- **Fortitudo** (Strength): Enhances physical power
- **Celeritas** (Speed): Increases movement
- **Sanatio** (Healing): Restores health
- **Protego** (Protection): Creates barrier
- **Mutatio** (Transformation): Changes form/properties
- **Mentis** (Mind): Affects cognition/emotion
- **Gravitas** (Gravity): Manipulates weight/attraction
- **Sonitus** (Sound): Produces/manipulates sound
- **Tempestas** (Storm): Weather effect

**Base Spell Examples:**
- **Sagitta Ignis** (Fire Arrow): Linear fire projectile
- **Globus Glacies** (Ice Ball/Snowball): Spherical ice projectile
- **Conus Fulgur** (Lightning Cone): Cone of lightning
- **Contactus Fortitudo** (Touch Strength): Touch-based strength buff
- **Murus Protego** (Protection Wall): Defensive barrier wall
- **Nubes Venenum** (Poison Cloud): Area poison effect

#### Modifier Stacking

Entities can stack **any modifiers they have mastered** onto base spells:

**Example: Crafting "Balthazar's Wrath"**

**Starting Base:** Globus Ignis (Fire Ball)
- Cost: 25 focus, 3s cast, 20m range, 2d6 fire damage

**Apply Modifiers:**
1. **Proximus** (Near/Close): -20% focus, -1s cast, range halved, +30% potency
   - New: 20 focus, 2s cast, 10m range, 2.6d6 damage
2. **Singularis** (Focused): -30% focus, +50% potency
   - New: 14 focus, 2s cast, 10m range, 3.9d6 damage (concentrated beam instead of explosion)
3. **Instantis** (Instant): +200% focus (applied to current cost)
   - New: 42 focus, instant cast, 10m range, 3.9d6 damage
4. **Maximus** (Maximum): +300% focus, +150% potency, backfire risk
   - New: 168 focus, instant cast, 10m range, 9.75d6 damage
5. **Ignis** (Fire enhancement): +40% focus (redundant with base but amplifies)
   - New: 235 focus, instant cast, 10m range, 9.75d6 fire + burn DoT
6. **Anima** (Soul Drain): +40% focus, drains target focus
   - New: 329 focus, instant cast, 10m range, 9.75d6 fire + burn + 50 focus drain
7. **Sanguis** (Blood Cost): 0 focus, HP cost 2× focus (658 HP!), +25% potency
   - New: 0 focus, 658 HP cost, instant cast, 10m range, 12.2d6 fire + burn + 62 focus drain
8. **Terror** (Fear): +55% focus equivalent in HP
   - Final: 0 focus, 1020 HP cost, instant cast, 10m range, 12.2d6 fire + burn + 62 focus drain + morale damage + fear

**Result: "Balthazar's Wrath"**
- **Full Name:** Proximus Singularis Instantis Maximus Ignis Anima Sanguis Terror Globus Ignis
- **Colloquial Name:** "Balthazar's Wrath" (named by crafter)
- **Effect:** Instant-cast point-blank concentrated fire beam that costs 1020 HP, deals massive fire damage + burn + drains focus + terrifies
- **Quality:** 85 (Excellent) - based on crafter's Intelligence/Wisdom/Language skill
- **Rarity:** Legendary (requires mastery of 8 modifiers + base spell)
- **Language:** Arcane (crafted in Arcane language)
- **Risk:** 30% backfire chance (Maximus modifier + complexity) - may explode in caster's face

**Viable Use Cases:**
- **Desperate Last Stand:** Caster at low focus but full HP, sacrifices life force for devastating attack
- **Blood Mage Specialty:** Entity with high HP pool, low focus capacity, specializes in blood magic
- **Terror Weapon:** Intimidate enemies, cause rout, even if target survives the hit

#### Spell as Craftable Product

Crafted spells have **quality, rarity, tech tier** like any other product:

**Quality (1-100):**
Based on crafter's attributes:
```
Quality = (Intelligence + Wisdom)/2 × LanguageProficiency% × MightMagicAxisBonus
```
- **Intelligence/Wisdom:** Higher stats = better optimization, fewer flaws
- **Language Proficiency:** 200% mastery in crafting language = 2× quality multiplier
- **Might/Magic Axis:** Magic-aligned crafter (+50 axis) gets +25% quality for magic spells

**Quality Effects:**
- **Poor (1-25):** Spell unstable, 20% backfire chance, -20% effectiveness
- **Common (26-50):** Functional but inefficient, +20% focus cost
- **Good (51-75):** Baseline, works as designed
- **Excellent (76-90):** Optimized, -20% focus cost, +10% effectiveness
- **Masterwork (91-100):** Perfect execution, -40% focus cost, +25% effectiveness, cannot backfire

**Rarity:**
Based on modifier count and accessibility:
- **Common:** 0-1 modifiers, widely known
- **Uncommon:** 2-3 modifiers, guild-level knowledge
- **Rare:** 4-5 modifiers, master-level knowledge
- **Epic:** 6-7 modifiers, legendary caster knowledge
- **Legendary:** 8+ modifiers, world-renowned innovator

**Tech Tier:**
- **Tribal (Tier 1):** Base components only, no modifiers
- **Classical (Tier 2):** 1-2 modifiers, traditional combinations
- **Arcane (Tier 3):** 3-5 modifiers, sophisticated theory
- **Mythic (Tier 4):** 6+ modifiers, approaching Words of Power

#### Custom Naming

**Crafter's Signature:**
- Entity crafts spell, names it after themselves: "Balthazar's Wrath"
- Name spreads with the spell (fame/infamy for crafter)
- Other casters learn "Balthazar's Wrath" and attribute it to him
- Variant names emerge: "Modified Balthazar's Wrath," "Balthazar's Lesser Wrath"

**Cultural Naming:**
- Arcane Guild: Technical names ("Proximus Singularis Maximus Globus Ignis")
- Dwarven Forge-Mages: Descriptive names ("Forge-Heart's Smite")
- Elven Naturalists: Poetic names ("Whisper of Autumn's End")
- Orcish Shamans: Brutal names ("Skullcrusher's Doom")

**Spell Variants:**
- **Original:** "Balthazar's Wrath" (Legendary quality, 8 modifiers)
- **Apprentice Version:** "Balthazar's Spark" (Minor modifier, learner-friendly)
- **Guild Version:** "Sanctum's Wrath" (Mages' Guild adopted and refined it)
- **Corrupted Version:** "Dark Wrath" (Blood cultists removed Sanguis, added Mortis)

#### Trading and Learning Spells

**Spell as Intellectual Property:**

Spells can be:
- **Taught:** Master teaches student (requires both present, demonstration)
- **Sold:** Crafter sells spell scroll/tome with instructions
- **Stolen:** Rival mage observes casting, reverse-engineers (Intelligence check)
- **Traded:** Guild exchange programs (share spell libraries)
- **Auctioned:** Legendary spells sold to highest bidder
- **Hoarded:** Kept secret by paranoid mages/guilds

**Learning Mechanics:**

**To Learn a Spell, Entity Needs:**
1. **Access to Spell:** Scroll, tome, teacher, or observed casting
2. **Base Component Knowledge:** Must know Form + Effect components
3. **Modifier Knowledge:** Must know all modifiers (or be able to learn them)
4. **Sufficient Intelligence/Wisdom:** Higher complexity = higher stat requirements
5. **Practice:** Repeated attempts to cast until mastered

**Language Proficiency NOT Required (But Helps):**

**Learning Spell Without Language Knowledge:**
- **Possible:** Can learn "Balthazar's Wrath" with 0% Arcane proficiency
- **Difficult:** -50% learning speed (treating it as foreign phonetic sounds)
- **Limited Understanding:** Can cast it but doesn't understand "why" it works
- **No Modification:** Cannot tweak modifiers without language knowledge

**Learning Spell With Language Knowledge:**
- **Easier:** +100% learning speed at 100%+ proficiency
- **Deep Understanding:** Comprehends magical theory, can debug flaws
- **Modification Possible:** Can adjust modifiers, create variants
- **Teaching Enabled:** Can teach spell to others effectively

**Language Learning Bonus from Spell Study:**

**Studying Foreign-Language Spell Grants Language XP:**
```
LanguageXP = SpellComplexity × StudyTime × (Intelligence + Wisdom)/2
```

**Example:**
- Entity with 0% Arcane studies "Balthazar's Wrath" (8 modifiers, Legendary complexity)
- Spends 100 hours practicing pronunciation and gestures
- Gains significant Arcane language XP (may reach 20-30% proficiency through spell study alone)
- Motivated to continue learning Arcane to unlock spell modification capabilities

**Incentive Structure:**
- **Powerful Spells → Language Learning:** "I want Balthazar's Wrath, so I'll learn Arcane to master it"
- **Language Mastery → Spell Access:** "I mastered Arcane, now I can access the guild's spell library"
- **Spell Crafting → Fame:** "I'll craft the most powerful fire spell to make a name for myself"

#### Spell Libraries and Language Prestige

**Languages Compete Based on Spell Quality:**

**Arcane Language:**
- **Prestige:** High (ancient tradition, many guild masters)
- **Spell Library:** 500+ documented spells, Tier 3-4 complexity
- **Famous Crafters:** Balthazar (fire specialist), Morgana (time magic), Arcturus (teleportation)
- **Draw:** Magic users worldwide learn Arcane to access spell library

**Draconic Language:**
- **Prestige:** Very High (inherently magical, draconic power Words)
- **Spell Library:** 200+ documented spells, raw power focus
- **Famous Crafters:** Ancient dragons, dragon-blooded sorcerers
- **Draw:** Power-seekers learn Draconic for devastating combat spells

**Common Language:**
- **Prestige:** Low (functional but not optimized for magic)
- **Spell Library:** 50 documented spells, Tier 1-2 simplicity
- **Famous Crafters:** Few (folk hedge-mages, village healers)
- **Draw:** Beginner-friendly, accessible to non-specialists

**Elvish Sylvan:**
- **Prestige:** Medium (nature magic specialty)
- **Spell Library:** 300+ documented spells, nature/healing focus
- **Famous Crafters:** Elven Archdruids, ancient treekeepers
- **Draw:** Druids and healers learn Sylvan for nature magic

**Guild Formation Around Languages:**

**Arcane Mages' Guild:**
- **Language:** Arcane (required for membership)
- **Spell Library:** Extensive, well-documented, hierarchical access (apprentice → master)
- **Guild Masters:** Legendary spell crafters who innovate new combinations
- **Attraction:** Aspiring mages join to learn from masters, access library

**Draconic Sorcerers' Cabal:**
- **Language:** Draconic (200% mastery required for inner circle)
- **Spell Library:** Exclusive, guarded, dangerous spells
- **Guild Masters:** Dragon-blooded individuals with innate Draconic affinity
- **Attraction:** Power-hungry mages willing to risk corruption for strength

**Common Folk Healers:**
- **Language:** Common (no barrier to entry)
- **Spell Library:** Small, practical, healing/utility focus
- **Guild Masters:** Village elders with decades of experience
- **Attraction:** Regular villagers learning practical magic for daily life

#### Economic Implications

**Spell Market:**
- **Spell Scrolls:** Single-use spell (50-500 gold depending on rarity)
- **Spell Tomes:** Permanent learning resource (500-10,000 gold)
- **Tutoring:** Master teaches spell in person (1000-50,000 gold + reputation cost)
- **Guild Membership:** Access to entire spell library (annual dues + service obligations)

**Crafter Fame:**
- **Balthazar's Wrath spreads:** Balthazar becomes famous, gains students, guild offers
- **Variant Market:** Other mages create "Balthazar-inspired" spells, diluting brand
- **Legacy:** Centuries later, "Balthazar School of Fire Magic" persists as tradition

**Language Value:**
- **High-Value Languages:** Arcane, Draconic (rich spell libraries)
- **Medium-Value Languages:** Sylvan, Celestial, Infernal (specialized libraries)
- **Low-Value Languages:** Common, regional dialects (limited magical utility)

**Piracy and Theft:**
- **Spell Stealing:** Rival mage observes "Balthazar's Wrath," reverse-engineers, sells as own
- **Guild Espionage:** Infiltrate rival guild, copy spell library, establish competing guild
- **Scroll Forgery:** Create fake scrolls with flawed spells (scam market)

### Somatic Casting (Spell Signs)

**Spell Signs** are gesture-based magic using hand movements and body language instead of vocal incantations. They provide an **alternative casting method** for mutes or situational use when silenced.

#### Verbal vs. Somatic Casting

**Two Casting Methods:**

**Verbal Casting (Standard):**
- **Requires:** Voice (speaking spell words)
- **Vulnerable To:** Silence, gags, throat injury
- **Immune To:** Hand restraints, missing limbs
- **Casting Speed:** Standard
- **Power:** Baseline

**Somatic Casting (Spell Signs):**
- **Requires:** Hands (gestures, signs, mudras)
- **Vulnerable To:** Hand restraints, missing limbs, paralysis
- **Immune To:** Silence, gags, mute condition
- **Casting Speed:** Varies (some faster, some slower)
- **Power:** Varies (can dual-cast for amplification)

#### Who Uses Somatic Casting

**Mutes (Natural Somatic Casters):**
- Born mute or suffered throat injury/curse
- **+50% proficiency** with somatic spells (compensating adaptation)
- Can only use somatic casting (no verbal alternative)
- Often develop unique sign-based spell variants

**Tactical Users:**
- Stealth mages (silent casting for infiltration)
- Backup casters (keep somatic spells ready if silenced)
- Dual-casters (cast from both hands simultaneously)

**Cultural Practitioners:**
- Monastic traditions (vows of silence)
- Sign language cultures (natural integration)
- Ninja/Assassin traditions (stealth focus)

#### Dual-Hand Casting

**Somatic spells enable simultaneous casting from multiple hands:**

**Single-Hand Casting (Standard):**
- Cast one spell with one hand
- Other hand free for weapon/shield/component

**Dual-Hand Casting (Advanced):**
Three modes:

1. **Parallel Casting** (Two Independent Spells):
   - Cast two different spells simultaneously
   - **Requirement:** 150%+ somatic proficiency, 80+ focus capacity
   - **Cost:** Both spells' focus costs (no discount)
   - **Example:** Right hand casts Sagitta Ignis (fire bolt), left hand casts Scutum (shield)
   - **Complexity:** Concentration check (INT/WIS), 20% chance of interference (both spells fizzle)

2. **Amplified Casting** (Combine Power):
   - Both hands cast same spell for increased potency
   - **Requirement:** 120%+ somatic proficiency
   - **Cost:** +50% focus
   - **Effect:** +50% power, +100% stability (cannot backfire)
   - **Example:** Both hands cast Globus Ignis → double-strength fireball

3. **Complex Casting** (Hybrid Spell):
   - Each hand casts different component, merge into hybrid spell
   - **Requirement:** 180%+ somatic proficiency, 90+ focus capacity
   - **Cost:** Sum of both spells + 50%
   - **Effect:** Unique combination impossible with single-hand
   - **Example:** Right hand Ignis + left hand Glacies → Steam blast (fire + ice = steam explosion)

#### Somatic Modifiers

**Somatic casting has unique modifiers** not available to verbal casting:

**Hand-Based Modifiers:**
- **Manus** (Hand/Touch): Melee-range touch casting (faster, higher risk)
  - Cost: -30% focus, Cast time: -50%, Range: touch only, Effect: +40% potency
  - Example: "Manus Ignis" (palm strike fire blast)

- **Digitus** (Finger/Precision): Pinpoint accuracy, needle-like focus
  - Cost: +20% focus, Cast time: unchanged, Effect: +50% accuracy, ignores 25% armor
  - Example: "Digitus Fulgur" (lightning finger-jab)

- **Pugnus** (Fist/Impact): Channels spell through punch
  - Cost: +30% focus, Cast time: instant on hit, Effect: physical damage + spell effect
  - Example: "Pugnus Glacies" (freezing punch)

- **Gestus** (Gesture/Flourish): Sweeping motion, theatrical
  - Cost: +10% focus, Cast time: +30%, Effect: +20% area, intimidation bonus
  - Example: "Gestus Ignis Locus" (dramatic flame wave)

**Body-Based Modifiers:**
- **Saltus** (Leap/Jump): Cast while jumping/dodging
  - Cost: +40% focus, Cast time: instant, Requirement: movement
  - Example: "Saltus Sagitta" (backflip while firing bolt)

- **Rotatio** (Spin/Rotation): 360° casting
  - Cost: +80% focus, Cast time: 2s spin, Effect: hits all targets in radius
  - Example: "Rotatio Fulgur" (spinning lightning storm)

- **Prostratus** (Prone/Ground): Cast from prone position
  - Cost: -20% focus, Cast time: +50%, Effect: +30% concealment
  - Example: "Prostratus Umbra" (cast from hiding, shadow blend)

**Mudra Modifiers (Sacred Hand Seals):**
- **Anjali** (Prayer Hands): Defensive, protective
  - Cost: +30% focus, Effect: converts offensive spell to defensive ward
  - Example: "Anjali Ignis" (fire becomes protective warmth barrier)

- **Abhaya** (Fearlessness Hand): Dispels fear, grants courage
  - Cost: +25% focus, Effect: spell also removes fear/terror effects
  - Example: "Abhaya Lux" (light that banishes fear)

- **Varada** (Generosity Hand): Sharing, multi-target blessing
  - Cost: +60% focus, Effect: buff spell affects all allies in 15m radius
  - Example: "Varada Fortitudo" (shared strength blessing)

- **Dhyana** (Meditation Hand): Sustained focus, efficiency
  - Cost: -40% focus, Cast time: +100%, Effect: perfectly efficient, no waste
  - Example: "Dhyana Sanatio" (slow but efficient heal)

#### Somatic Base Components

**Somatic casting uses same Form + Effect bases** but with gesture-specific variations:

**Form Variations:**
- **Sagitta** (verbal: spoken projectile) → **Iacula** (somatic: finger-point projectile)
- **Globus** (verbal: orb) → **Sphaera** (somatic: hand-shaped orb, spins)
- **Murus** (verbal: wall) → **Palma** (somatic: palm-thrust barrier)

**Effect Variations:**
- **Ignis** (verbal: spoken flame) → **Flamma** (somatic: hand-kindled fire, warm touch)
- **Glacies** (verbal: frost word) → **Gelu** (somatic: chi-frozen ice, cold hand)

**Somatic-Exclusive Components:**
- **Impactus** (Impact/Collision): Physical force channeled through strike
  - Example: "Pugnus Impactus" (force punch), "Palma Impactus" (palm strike shockwave)

- **Deflexio** (Deflection/Redirect): Redirects incoming attacks
  - Example: "Manus Deflexio" (catch spell with hand, throw back)

- **Tactus** (Touch/Transmission): Transmits effect through contact
  - Example: "Tactus Sanatio" (healing touch), "Tactus Venenum" (poison touch)

#### Mute Casters

**Natural somatic specialists:**

**Advantages:**
- **+50% somatic proficiency** (compensating adaptation)
- **-30% somatic spell costs** (lifelong practice efficiency)
- **Can dual-cast at 100% proficiency** (instead of 150%)
- **Immune to silence** (already mute)
- **+25% gesture speed** (refined muscle memory)

**Disadvantages:**
- **Cannot cast verbal spells** (no voice)
- **Vulnerable to restraints** (entire magic system disabled if hands bound)
- **Communication barriers** (harder to teach verbal casters)
- **Social stigma** (some cultures view muteness as curse/weakness)

**Cultural Integration:**
- Some cultures revere mute mages as "Silent Masters"
- Monasteries with vow-of-silence traditions attract mute students
- Sign language communities naturally produce more somatic casters

#### Tactical Considerations

**Silence vs. Restraint:**

**Verbal Caster in Combat:**
- **Silenced:** Cannot cast (hard counter)
- **Hands Bound:** Can still cast (immunity)
- **Gag Applied:** Cannot cast (alternative silence)

**Somatic Caster in Combat:**
- **Silenced:** Can still cast (immunity)
- **Hands Bound:** Cannot cast (hard counter)
- **Missing Hand:** -50% casting ability (can one-hand cast at penalty)

**Hybrid Caster (Knows Both):**
- **Flexibility:** Switch between verbal and somatic based on situation
- **Cost:** Must learn both systems (double effort)
- **Mastery:** Rarely masters either (spreading focus too thin)

**Stealth Applications:**
- **Somatic casting is silent** (perfect for infiltration)
- **No vocal components** (cannot be overheard)
- **Subtle hand movements** (can cast while appearing unarmed)
- **Combined with Umbra modifier** (invisible hand signs in darkness)

#### Somatic Spell Examples

**"Silent Assassin's Bolt"** (Manus Umbra Celer Iacula Mortis):
- **Base:** Iacula Mortis (finger-point death projectile)
- **Modifiers:** Manus (touch-range instant), Umbra (shadow-cloaked), Celer (swift)
- **Cost:** 45 focus, instant cast, touch range
- **Effect:** Point at target, instant death bolt from fingertip, shrouded in shadow
- **Use Case:** Assassination in darkness, no sound, target doesn't see it coming

**"Twin Dragon Fists"** (Pugnus Ignis + Pugnus Glacies, Dual-Hand Complex):
- **Right Hand:** Pugnus Ignis (fire punch)
- **Left Hand:** Pugnus Glacies (ice punch)
- **Combo:** Alternating strikes create temperature shock
- **Cost:** 80 focus total
- **Effect:** Fire punch → ice punch → fire punch (thermal shock damage +50%)
- **Use Case:** Martial artist mage, monk combat style

**"Meditative Barrier"** (Dhyana Anjali Palma Protego Perpetua):
- **Base:** Palma Protego (palm-thrust barrier)
- **Modifiers:** Dhyana (meditative efficiency), Anjali (prayer hands protection), Perpetua (sustained)
- **Cost:** 30 focus initial, 3 focus/min (Dhyana reduced), prayer hand position maintained
- **Effect:** Sustained protective barrier while meditating
- **Use Case:** Monk meditation, ritual protection, long-term defense

**"Whirling Blade Storm"** (Rotatio Multiplex Iacula Terrum):
- **Base:** Iacula Terrum (stone projectiles)
- **Modifiers:** Rotatio (360° spin), Multiplex (multiple projectiles)
- **Cost:** 120 focus, 2s spin cast
- **Effect:** Spin in circle, launch stone shards in all directions, hits all enemies around caster
- **Use Case:** Surrounded by enemies, defensive crowd control

### Spell Casting Mechanics

#### Casting Requirements

1. **Casting Method Choice:** Verbal (voice) or Somatic (hands)
2. **Language/Somatic Proficiency:** Meet minimum % threshold (or learn phonetically/gesturally at penalty)
3. **Focus Availability:** Sufficient focus pool to pay cost
4. **Component Knowledge:** Know all base components and modifiers
5. **Physical Capability:**
   - **Verbal:** Requires voice (silenced/gagged = cannot cast)
   - **Somatic:** Requires hands (bound/missing limbs = cannot cast)
   - **Hybrid Spells:** Requires both voice and hands
6. **Line of Sight:** Most targeted spells require seeing target
7. **Concentration:** Sustained spells occupy focus capacity

#### Skill Progression

**Repeated Use Improves Mastery:**
- **Novice (0-100 casts):** Full focus cost, standard cast time
- **Adept (101-500 casts):** 90% focus cost, 90% cast time
- **Expert (501-2000 casts):** 75% focus cost, 75% cast time
- **Master (2001+ casts):** 60% focus cost, 60% cast time, can cast while moving

**Critical Success/Failure:**
- **Critical Success (5% chance):** Half focus cost, double effect potency
- **Critical Failure (5% chance):** Full focus cost, spell fizzles or backfires

---

## Words of Power

### Overview

**Words of Power** are primordial linguistic constructs that directly manipulate reality. They bypass normal magical limitations but are **only accessible to those who have achieved 200% mastery** of a language.

### Characteristics

**Mastery-Gated:**
- Requires 200% fluency in the language (with rare exceptions for prodigies or divine gifts)
- Cannot be taught directly; must be discovered through research or revelation
- Each Word is tied to a specific language's phonetic/conceptual structure

**Exceptions to Mastery Gate:**
- **Prodigies:** Rare individuals (1 in 10,000) with innate Word affinity, can learn at 150%+
- **Divine Revelation:** Gods may grant Words to chosen champions (bypasses mastery)
- **Cursed/Blessed Artifacts:** Items that contain Words, usable by wielder regardless of proficiency
- **Possession:** Entities channeling powerful spirits/demons may temporarily access their Words

**Power and Risk:**
- Significantly more potent than standard spell signs (10-100× effect magnitude)
- High focus cost (50-200 focus per use)
- Risk of unforeseen consequences (reality doesn't always interpret intent correctly)
- Can permanently alter local reality (terrain changes, magical dead zones, etc.)

### Example Words of Power

**Arcane Language:**
- **VASTITAS** (Devastation): Unleashes raw destructive force in 30-meter radius (100 focus, 10d10 damage, leaves scorched earth)
- **RESTITUO** (Restoration): Reverses recent damage/changes to target area (150 focus, rewinds 1 minute of local time for objects only)
- **EXSILIUM** (Banishment): Forces extraplanar entity back to home plane (80 focus, no save if entity's true name spoken)

**Celestial Language:**
- **LUX AETERNA** (Eternal Light): Creates permanent light source immune to magical darkness (120 focus, cannot be dispelled)
- **VITA REDDO** (Life Return): Resurrects recently dead (200 focus, must cast within 1 hour of death, target returns at 1 HP)
- **SANCTUM** (Sanctuary): Designates area as consecrated ground, repels undead/fiends (100 focus, permanent until desecrated)

**Draconic Language:**
- **DOVAHZUL** (Dragonforce): Grants temporary draconic abilities (150 focus, 5-minute duration: flight, breath weapon, scaled skin)
- **KRENT** (Crumble): Instantly shatters non-magical stone/metal in area (75 focus, 10-meter radius)
- **ZUN** (Weapon): Imbues weapon with permanent magical enhancement (100 focus, weapon becomes +1 enchanted)

### Discovering Words of Power

**Research Paths:**
1. **Linguistic Archaeology:** Study ancient texts, decipher lost dialects (Intelligence/Wisdom checks, years of research)
2. **Magical Experimentation:** Combine known spell signs in novel ways, stumble upon primordial patterns (risky, chance of backfire)
3. **Dream Visions:** Meditation and trance states may grant insights (Wisdom-based, rare random events)
4. **Pact/Revelation:** Gods, demons, or ancient entities may teach Words in exchange for service
5. **Natural Prodigy:** Rare spontaneous awakening of Word knowledge (1 in 10,000 entities)

**Combination Potential:**
Multiple Words can be chained for unprecedented effects:
- **VASTITAS + RESTITUO:** Destructive blast followed by instant terrain restoration (display of power without permanent damage)
- **VITA REDDO + SANCTUM:** Resurrection within consecrated ground, returns target at full HP instead of 1 HP
- **ZUN + DOVAHZUL:** Temporarily transforms weapon into draconic artifact during draconic transformation

---

## Sigils

### Overview

**Sigils** are symbolic representations of magical intent, drawn from ancient traditions (inspired by Goetic and other occult systems). They serve as focal points for magical energy and can encode complex spell structures into visual form.

### Current Status: Function TBD

**Potential Functions (Under Design):**

1. **Magical Anchors:**
   - Draw sigil on surface to anchor spell effects to location
   - Example: Sigil of Amon anchors fire spell to doorway, triggers when enemy passes

2. **Summoning Glyphs:**
   - Specific sigils linked to entities (demons, spirits, elementals)
   - Drawing sigil creates sympathetic link, enables summoning/binding

3. **Spell Storage:**
   - Inscribe sigil onto object (scroll, weapon, armor)
   - Sigil stores prepared spell, releases on command or trigger condition
   - Example: Sword with Sigil of Bael, releases fire burst on crit

4. **Ritual Components:**
   - Sigils as part of ritual casting (see Rituals section)
   - Each ritual requires specific sigil configuration
   - Quality of sigil drawing affects ritual success chance

5. **Identification/Authentication:**
   - Personal sigils as magical signatures
   - Prove identity, seal contracts, ward possessions
   - Example: Mage's personal sigil on spell scroll proves authorship

6. **Passive Enchantments:**
   - Permanent sigils on buildings/areas create ambient effects
   - Sigil of Vassago grants occupants +10% learning speed
   - Sigil of Astaroth in bedroom improves sleep quality (morale boost)

### Sigil Mechanics (Proposed)

**Sigil Crafting:**
- Requires focus expenditure (10-50 focus depending on complexity)
- Quality based on crafter's Intelligence + Wisdom + relevant language proficiency
- Materials matter: chalk (temporary, 1-day duration), ink (1-week duration), blood (permanent but corruption risk), enchanted pigments (permanent, stable)

**Sigil Library:**
72 traditional sigils (based on Goetic tradition) + custom sigils discoverable/craftable
- Each sigil has associated entity/concept
- Example from image: Bael (sigil #1), Agares (sigil #2), Vassago (sigil #3), etc.

**Learning Sigils:**
- Must achieve 100%+ fluency in magical language
- Each sigil must be learned separately (research, teaching, or discovery)
- Sigil knowledge tradeable (can teach others, create instructional texts)

---

## Rituals

### Overview

**Rituals** are sustained channeled magic that produces powerful, often prolonged effects. Unlike instant spell signs, rituals require continuous concentration and often involve multiple components (language, sigils, materials, multiple casters).

### Ritual Characteristics

**Sustained Channel Cast:**
- Initial casting period (1 minute to several hours depending on ritual complexity)
- Ongoing maintenance via focus expenditure (caster must sustain concentration)
- Can be interrupted by:
  - **Silence:** Inability to speak (gag, magical silence, throat injury)
  - **Focus Loss:** Focus pool depleted or caster takes damage (concentration check)
  - **Physical Interruption:** Caster moved, ritual components disturbed
  - **Counterspell:** Hostile entity disrupts magical flow

**Language Requirement:**
- Most rituals require speaking (chanting, incantations, recitations)
- Language proficiency requirement: 85%+ for simple rituals, 150%+ for complex/powerful rituals
- Silenced casters cannot initiate new rituals, but can maintain existing ones if only concentration required (voice was only needed for initial casting)

**Focus Costs:**
- Initial cast: 30-200 focus (paid upfront)
- Sustain cost: 5-50 focus per minute (drains pool continuously)
- **Skill reduces cost:** Expert ritualists can reduce sustain cost by 40-60%

### Ritual Types

#### Magical Rituals

**Examples:**
- **Ritual of Warding:** Creates protective barrier around area (50 focus initial, 10 focus/minute, repels hostile entities)
- **Ritual of Scrying:** Remote viewing of distant location (80 focus initial, 15 focus/minute, shows live vision)
- **Ritual of Binding:** Traps entity within circle (120 focus initial, 20 focus/minute, STR check to break each turn)
- **Ritual of Transmutation:** Converts base metals to precious metals (150 focus initial, 30 focus/minute, 1 hour duration, yields gold/silver)

#### Might Rituals (War Cries, Hymns, Bardic Magic)

**Not limited to magic users!** Might-aligned entities (warriors, bards, leaders) can craft rituals of martial prowess using language.

**Examples:**
- **War Cry of the Ancestors:** Warlord channels ancestral spirits through guttural chant (40 focus initial, 8 focus/minute, all allies +20% melee damage)
- **Hymn of Endurance:** Cleric/Bard sustains inspirational song (30 focus initial, 5 focus/minute, all allies +15% HP regeneration)
- **Rousing Speech:** General delivers stirring oration before battle (60 focus initial, 0 focus sustain after 5-minute speech, morale +30 for 1 hour)
- **Chant of the Legion:** Unit marches while chanting in unison (multiple casters, 5 focus/minute each, +10% movement speed, improved formation cohesion)

**Might Ritual Characteristics:**
- Based on shared memories, cultural identity, martial tradition
- Often require multiple participants (squad/army chanting together)
- Can instill new memories or reinforce existing ones (bardic storytelling as ritual)
- Buff allies, intimidate enemies, coordinate complex maneuvers

### Ritual Learning

**Entities Can Learn by Observing:**
- An entity focusing on a ritual in progress can attempt to learn:
  - **The language being spoken** (if not known): Gains fluency XP proportional to observation time
  - **The ritual itself** (if language known): Learns ritual structure, components, execution

**Learning Chance:**
- Observer Intelligence/Wisdom check vs. Ritual Complexity
- Higher language proficiency = better learning chance
- Taking notes (if literate) grants +20% learning bonus
- Multiple observations required for complex rituals (may take 5-10 viewings to fully learn)

### Ritual Crafting

**Quality, Rarity, Tech Tier System:**
Rituals are treated like craftable products:
- **Quality:** 1-100 (affects potency, stability, focus cost)
  - Poor Quality (1-25): 150% focus cost, 20% chance of backfire
  - Common Quality (26-50): 120% focus cost, stable
  - Good Quality (51-75): 100% baseline
  - Excellent Quality (76-90): 80% focus cost, +20% potency
  - Masterwork Quality (91-100): 60% focus cost, +50% potency, cannot backfire
- **Rarity:** Common, Uncommon, Rare, Epic, Legendary (based on knowledge accessibility)
- **Tech Tier:** Tribal (basic rituals), Classical (refined techniques), Arcane (sophisticated magical theory), Mythic (approaching Words of Power)

**Cooperative Crafting:**
Multiple entities can collaborate on ritual creation:
- Primary crafter (highest relevant stats) sets base quality
- Assistants contribute skill bonuses (up to 3 assistants, each adds +5% quality per tier above primary)
- Diverse language knowledge enables cross-cultural rituals (unique hybrid effects)

**Crafting Process:**
1. **Research Phase:** Study magical theory, language mastery, gather knowledge (days to months)
2. **Design Phase:** Outline ritual structure, components, focus costs (Intelligence/Wisdom checks)
3. **Testing Phase:** Attempt ritual in controlled environment (risk of failure/backfire)
4. **Refinement Phase:** Adjust based on test results, optimize focus costs
5. **Documentation Phase:** Record ritual in tome/scroll for future use and teaching

### Unforeseen Consequences

**Magical crafting is dangerous:**
- **Critical Failure (5% chance on test):** Ritual produces opposite/unintended effect
  - Healing ritual spreads disease
  - Warding ritual attracts hostile entities
  - Transmutation ritual transforms caster instead of target
- **Magical Contamination:** Area where ritual was tested may develop strange properties
  - Plants grow unnaturally fast
  - Shadows behave oddly
  - Ambient temperature shifts
- **Unwanted Attention:** Powerful rituals may attract extraplanar entities, rival mages, or god notice
- **Corruption Risk:** Dark/forbidden rituals may shift caster's alignment over time

---

## Might/Magic Axis

### Overview

The **Might/Magic Axis** determines an entity's natural affinity for physical prowess vs. magical aptitude. This is not a binary choice—entities exist on a spectrum—but it significantly impacts learning rates and effectiveness.

### Axis Scale

```
[-----Might-----|-----Balanced-----|-----Magic-----]
   Warrior         Battlemage          Wizard
```

**-100 (Pure Might) ← 0 (Balanced) → +100 (Pure Magic)**

### Racial Magic Affinity

**Each race has default axis position:**
- **Humans:** -10 to +10 (balanced, variable)
- **Elves:** +40 to +60 (strong magical affinity)
- **Dwarves:** -50 to -30 (strong physical affinity, magic resistance)
- **Orcs:** -60 to -40 (overwhelmingly physical)
- **Gnomes:** +30 to +50 (magical inclination, intellectual)
- **Half-breeds:** Average of parent races

**Individual Variation:**
Each entity has personal modifier ±30 from racial baseline
- Orc mage: Racial -50, personal +30 = -20 (still might-leaning but can cast)
- Elven warrior: Racial +50, personal -30 = +20 (still magic-capable but prefers martial)

### Aggregate Entity Affinity

**Groups (villages, guilds, armies) have averaged affinity:**
- Village average calculated from all villager affinities
- Guild selection naturally filters toward shared affinity (Mages' Guild: +60 average)
- Army composition affects strategic options (might-heavy army: melee focused, magic-light army: casters/support)

### Axis Effects

#### Learning Rates

**Martial Skills (Might side):**
- **Pure Might (-100):** 200% learning speed for martial, 10% for magic
- **Balanced (0):** 100% learning speed for both
- **Pure Magic (+100):** 10% learning speed for martial, 200% for magic

**Magical Skills (Magic side):**
- Inverse of martial learning rates
- Magic-aligned entities learn spell signs faster, master languages quicker (for magical purposes)
- Might-aligned entities struggle with subtle spellcasting, excel at war cries/might rituals

#### Effectiveness Multipliers

**Combat:**
- **Might Entity Casting Fireball:** 50% effectiveness (weak magical attack)
- **Magic Entity Casting Fireball:** 150% effectiveness (potent magical attack)
- **Might Entity Using War Cry:** 150% effectiveness (powerful buff)
- **Magic Entity Using War Cry:** 50% effectiveness (weak buff)

**Ritual Crafting:**
- Might entities craft superior might rituals (war cries, battle hymns)
- Magic entities craft superior magical rituals (wards, transmutations)
- Both can learn the other's rituals, but at reduced effectiveness and higher learning cost

### Shifting the Axis

**Affinity is not entirely fixed:**
- Repeated magical practice slowly shifts toward magic (+0.1 per 100 spell casts)
- Repeated martial training slowly shifts toward might (-0.1 per 100 melee kills)
- Lifetime shift limited to ±20 from starting position (racial + individual variance)
- Extreme experiences can cause sudden shifts (±5):
  - Near-death from magic → shift away from magic (trauma)
  - Mystical revelation → shift toward magic (enlightenment)
  - Legendary martial feat → shift toward might (proving prowess)

---

## Communication & Language Learning (Social)

### Natural Language Acquisition

**Entities Learn Through Interaction:**

When two entities communicate:
1. **Language Detection:** Listener identifies language being spoken (if known at any %)
2. **Comprehension Check:** Listener's proficiency determines understanding
   - <25%: Catches isolated words, infers from context/gestures
   - 25-50%: Understands simple sentences, misses nuance
   - 51-75%: Comprehends most conversation, occasional confusion
   - 76-99%: Full understanding except rare idioms/slang
   - 100%+: Perfect comprehension
3. **Learning Gain:** Each conversation grants fluency XP:
   ```
   XP = ConversationLength × LearningRate × (SpeakerProficiency/100)
   ```
   - Listening to master speaker (200%) grants 2× XP vs. novice speaker (50%)
   - Longer conversations = more learning

### Xenophobic/Xenophilic Axis

**Cultural Attitude Toward Foreigners:**

**-100 (Xenophobic) ← 0 (Neutral) → +100 (Xenophilic)**

#### Individual Attitude
Each entity has personal xenophobia/xenophilia rating:
- **Xenophobic (-100 to -50):** Distrusts foreigners, reluctant to learn their language, may refuse to speak it even if known
- **Mildly Xenophobic (-49 to -20):** Cautious with outsiders, learns slowly
- **Neutral (-19 to +19):** Baseline, learns at standard rate
- **Mildly Xenophilic (+20 to +49):** Curious about other cultures, learns faster
- **Xenophilic (+50 to +100):** Passionate about foreign cultures, actively seeks to learn languages, may adopt foreign customs

#### Aggregate Entity Attitude
Villages/guilds have averaged xenophobia/xenophilia:
- **Xenophobic Village (-60):** Foreigners unwelcome, trade limited, language learning discouraged
- **Neutral Village (0):** Foreigners tolerated, trade normal, language learning for practical purposes
- **Xenophilic Village (+70):** Foreigners celebrated, multicultural hub, polyglots common

**Cascade Effects:**
- Xenophobic village expels foreign-language speakers → becomes more xenophobic (reinforcing)
- Xenophilic village attracts multilingual traders → becomes more cosmopolitan (reinforcing)
- Mixed-attitude village under stress may fracture along xenophobic/xenophilic lines (factional tension)

### Intentions and Learning

**Why Entities Learn Languages:**

1. **Hostile Intentions (0.5× learning rate):**
   - Enemy soldier learns captors' language to gather intelligence
   - Spy infiltrates foreign court, learns language to blend in
   - Learns reluctantly, maintains psychological distance

2. **Trade/Diplomatic Intentions (1.0× baseline):**
   - Merchant learns customer's language for business
   - Ambassador learns host nation's language for negotiations
   - Pragmatic learning, no emotional investment

3. **Friendly Intentions (1.25× learning rate):**
   - Refugee adopted by foreign village learns language to integrate
   - Romantic partners teach each other languages
   - Motivated by desire to connect

4. **Cultural Immersion Intentions (1.5× learning rate):**
   - Scholar studies foreign culture, seeks fluency to understand texts
   - Immigrant fully adopts new home, abandons native language
   - Deepest learning, may eventually prefer new language over native

### Multilingualism

**Entities Can Know Multiple Languages:**
- No hard cap on languages known
- Each language tracks separately (can be 200% in Arcane, 30% in Draconic, 0% in Infernal)
- Polyglots gain social bonuses (trade, diplomacy, cultural bridge roles)
- Rare "linguistic savants" (1 in 5,000) learn all languages at 1.5× rate

**Language Atrophy:**
- Unused languages slowly decay (lose 1% per year if never spoken)
- Cannot decay below 50% (once intermediate, never fully forgotten)
- Refresher conversations quickly restore lost proficiency

---

## Cross-Project Implementation

### Godgame (Primary Focus)

**Full Magic System:**
- All components active: Languages, Spell Signs, Words of Power, Sigils, Rituals
- Villagers learn languages through proximity and trade
- Mages pursue mastery to unlock Words of Power
- Warriors develop might rituals (war cries, battle hymns)
- Gods may grant divine language knowledge or teach Words of Power to champions
- Cultural languages tied to village/guild identity
- Language barriers create strategic trade/diplomacy challenges

**Example Gameplay:**
- Player's village speaks Common (100% average)
- Neighboring Elven village speaks Sylvan (player's village: 15% average)
- Trade difficult without translator
- Player assigns xenophilic villager to learn Sylvan (reaches 75% in 2 years)
- Villager becomes translator, enables trade treaty
- Eventually multiple villagers learn Sylvan, intermarriage occurs, bilingual village emerges
- Bilingual villagers learn Elven ritual magic, blend with human martial traditions → unique hybrid culture

### Space4X (Adapted System)

**Less Magic, More Tech:**
- **Languages:** Still present (alien species, human dialects, ancient precursor languages)
- **"Words of Power":** Replaced with advanced tech/psionic abilities requiring mastery
- **"Spell Signs":** Replaced with psionic abilities, tech interfaces, ship commands
- **"Rituals":** Replaced with complex multi-stage tech procedures, crew coordination protocols
- **"Sigils":** Replaced with technical schematics, genetic/psionic markers

**Might/Magic Axis → Physical/Psionic Axis:**
- **Physical (-100):** Marines, engineers, strong physical presence
- **Balanced (0):** Versatile crew members
- **Psionic (+100):** Telepaths, navigators, reality-benders

**Example Space4X Gameplay:**
- Human crew speaks English (100%)
- Encounter alien species speaking Xrill (0%)
- First Contact: Use translation software (imperfect, 30% equivalent)
- Linguist crew member studies Xrill over months (reaches 80%)
- Learns alien "psionic navigation ritual" requires 150% Xrill
- Crew member spends years mastering Xrill to 150%+
- Unlocks alien FTL navigation technique ("Word of Power" equivalent)
- Ship gains strategic advantage: hybrid human/alien navigation system

---

## Parameters

| Parameter | Default | Range | Impact |
|-----------|---------|-------|--------|
| Base Language Learning Rate | 1.0%/day (immersion) | 0.1-2.0%/day | How fast entities learn with constant exposure |
| Age Peak (Childhood Bonus) | 2.0× multiplier | 1.5-3.0× | Advantage for young learners |
| Age Decline Start | 30 years old | 20-40 years | When learning becomes harder |
| Mastery Plateau (100%→200%) | 0.1%/month | 0.05-0.2%/month | How hard it is to achieve complete mastery |
| Spell Sign Focus Cost | 10-30 focus | 5-100 focus | Cost to cast standard spells |
| Ritual Sustain Cost | 5-20 focus/min | 1-50 focus/min | Ongoing drain for rituals |
| Word of Power Focus Cost | 50-200 focus | 30-300 focus | Cost for primordial magic |
| Xenophobic Learning Penalty | 0.3× rate | 0.1-0.5× | How much xenophobia slows learning |
| Xenophilic Learning Bonus | 1.8× rate | 1.2-2.5× | How much xenophilia accelerates learning |
| Language Atrophy Rate | -1%/year | -0.5 to -2%/year | Decay rate for unused languages |
| Might/Magic Axis Learning Modifier | ±100% | ±50% to ±200% | How much axis affects martial vs. magic learning |

---

## Examples

### Scenario 1: Young Mage Pursuing Mastery

**Given:** Elara, age 8, human mage prodigy (magic axis: +60, xenophilic: +40, Intelligence: 85, Wisdom: 70)
**Goal:** Master High Arcane to learn Words of Power

**Timeline:**
1. **Years 0-5 (Age 8-13):** Immersion in Arcane Academy
   - Learning rate: 1.0% base × 1.5 (age) × 1.6 (magic axis) × 1.4 (xenophilic) × 1.5 (immersion) × 0.775 (avg INT/WIS) = **2.6%/day**
   - Reaches 100% fluency in ~1.5 years (excellent for a child)
2. **Years 5-15 (Age 13-23):** Advanced study (100% → 150%)
   - Slows to 0.2%/month = 2.4%/year
   - Takes ~21 years to go from 100% → 150%
3. **Years 15-25 (Age 23-33):** Mastery pursuit (150% → 200%)
   - Further slows to 0.1%/month = 1.2%/year
   - Takes ~42 years to go from 150% → 200%
   - **Age 65:** Achieves 200% mastery, unlocks Words of Power

**Total: ~57 years of study** to go from 0% → 200% for a highly talented, motivated mage. Most never reach this level.

### Scenario 2: Warrior Learning Enemy Language

**Given:** Kragg, age 35, orc warrior (might axis: -70, xenophobic: -60, Intelligence: 40, Wisdom: 35, hostile intent)
**Context:** Captured by human army, forced to listen to guard conversations

**Timeline:**
1. **Month 1-6 (Occasional exposure, 1 hour/day):**
   - Learning rate: 1.0% base × 0.75 (age) × 0.3 (might/magic for linguistic tasks) × 0.3 (xenophobic) × 0.5 (hostile intent) × 0.3 (occasional exposure) × 0.375 (avg INT/WIS) = **0.00063%/day**
   - After 6 months (180 days): **0.11% fluency** (barely recognizes a few words)
2. **Month 6-12 (Realizes language = survival, neutral intent):**
   - Switches to neutral intent (1.0× instead of 0.5×)
   - New rate: **0.00126%/day**
   - After another 6 months: **0.11% + 0.23% = 0.34% fluency** (recognizes more words, can say "food," "water")
3. **Year 2-5 (Daily immersion, pragmatic learning):**
   - Immersion (2.0×) instead of occasional (0.3×)
   - New rate: **0.0084%/day = 3.07%/year**
   - After 3 more years: **0.34% + 9.2% = 9.5% fluency**
   - Can understand simple commands, speak broken Common

**Result:** After 4 years of forced exposure, xenophobic might-aligned orc reaches ~10% fluency—enough for basic communication but far from fluent. A xenophilic, high-INT individual would reach 10% in weeks.

### Scenario 3: Trade Village Becoming Multilingual

**Given:** Riverside village (100 villagers, average Common 100%, average Sylvan 5%, average Dwarven 0%)
**Event:** Trade route established with Elven forest (Sylvan speakers) and Dwarven mountain (Dwarven speakers)

**Year 1:**
- Merchants interact weekly with Elves and Dwarves
- 20 villagers (xenophilic merchants) prioritize learning
- Learning rate (merchants): ~0.5%/week (regular contact, trade intent)
- **Merchants reach:** Sylvan 26%, Dwarven 26% (basic conversation)
- **Village average:** Sylvan 5% → 10%, Dwarven 0% → 5%

**Year 5:**
- Merchants now fluent (Sylvan 75%, Dwarven 75%)
- Start teaching other villagers (10 become students)
- 5 intermarriages with Elves → bilingual children born (native in both languages)
- **Village average:** Sylvan 10% → 35%, Dwarven 5% → 30%

**Year 10:**
- Bilingual children grow up (now age 5-10, teaching parents)
- 15 more villagers learn from immersion (daily exposure to bilingual neighbors)
- Village becomes regional trade hub (linguistic advantage)
- **Village average:** Sylvan 35% → 60%, Dwarven 30% → 55%

**Year 20:**
- New generation is natively trilingual (Common/Sylvan/Dwarven)
- Village identity shifts: "We are the Bridge Folk" (cultural pride in multilingualism)
- Xenophilic average increases from +10 → +40 (cascade: linguistic diversity breeds openness)
- **Village average:** Sylvan 60% → 85%, Dwarven 55% → 80%

**Result:** Single trade route transforms monolingual village into trilingual cultural crossroads over 20 years. Linguistic diversity becomes core identity.

### Scenario 4: Crafting a Hybrid Ritual

**Given:**
- Elara (human mage, Arcane 200%, Celestial 120%)
- Thrain (dwarf warrior-priest, Dwarven 180%, Celestial 90%)
- Silas (human bard, Common 150%, Arcane 85%, Dwarven 60%)

**Goal:** Craft ritual that combines Arcane destruction, Celestial healing, and Dwarven resilience

**Process:**
1. **Design Phase (3 months):**
   - Elara leads (highest magic axis, knows Arcane/Celestial)
   - Thrain contributes Dwarven endurance chants
   - Silas bridges linguistic gaps, composes hybrid incantation
   - Base quality: 70 (Elara's expertise)
   - Assistant bonuses: +10 (Thrain's unique Dwarven knowledge), +5 (Silas's linguistic synthesis)
   - **Final design quality: 85 (Excellent)**

2. **Testing Phase (1 month, 5 attempts):**
   - **Attempt 1:** Critical failure (5% chance) → Ritual explodes, all take 2d6 damage, Elara loses 50 focus for 1 week (exhaustion)
   - **Attempt 2-4:** Partial successes, refine focus costs and timing
   - **Attempt 5:** Success! Ritual functions as intended

3. **Final Ritual: "Trinity Ward of the Forge"**
   - **Effect:** Creates protective barrier (Arcane), heals allies within (Celestial), grants damage resistance (Dwarven)
   - **Cost:** 120 focus initial, 25 focus/minute sustain
   - **Language Components:** Alternates Arcane/Celestial/Dwarven phrases (requires all 3 languages at 85%+)
   - **Quality:** 85 (Excellent) → 20% focus cost reduction, +20% potency
   - **Actual Cost:** 96 focus initial, 20 focus/minute sustain
   - **Rarity:** Rare (requires trilingual caster or 3-person team)
   - **Tier:** Arcane (sophisticated magical theory)

4. **Unforeseen Consequence:**
   - Test site develops strange property: Metal objects within 10 meters slowly heal damage over days (forge + healing resonance)
   - Becomes sought-after "Mending Ground" for repairing equipment
   - Adventurers pay to use the site → Elara/Thrain/Silas profit

**Result:** Cooperative crafting produces unique hybrid ritual impossible for single caster. Demonstrates power of multilingualism and cross-cultural collaboration in magic.

---

## Integration with Other Systems

### Focus System
- All magic (spell signs, rituals, Words of Power) costs focus
- Sustained spells occupy focus capacity (limits multitasking)
- Focus pool growth tied to usage (casting spells increases FocusMax over time)
- See [Focus_And_Status_Effects_System.md](../Implemented/Core/Focus_And_Status_Effects_System.md)

### Aggregate Entities
- Villages/guilds have average language proficiencies
- Linguistic diversity affects trade options, diplomatic relations
- Language barriers can cause factional splits (monolingual vs. polyglot factions)
- See [Aggregate_Entities_And_Individual_Dynamics.md](Aggregate_Entities_And_Individual_Dynamics.md)

### Individual Progression
- Language learning is major progression path (0% → 200%)
- Achieving mastery (200%) is lifetime achievement for most entities
- Rare entities master multiple languages (polyglot mages/scholars)
- See [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md)

### Guild System
- Mages' Guild selects for high magic axis, prioritizes Arcane language
- Warriors' Guild selects for high might axis, develops might rituals
- Bards' Guild values multilingualism, xenophilia, diplomatic skills
- See [Guild_System.md](../Villagers/Guild_System.md)

### Miracle System
- Gods can grant divine language knowledge (instant fluency gift)
- Divine Words of Power exist (separate from linguistic Words)
- Priests may learn rituals through divine revelation
- See [Miracle_System_Vision.md](../Miracles/Miracle_System_Vision.md)

---

## Technical Implementation

### Data Structures

```csharp
// Language proficiency tracking
public struct LanguageProficiency : IComponentData
{
    public FixedString64Bytes LanguageName;    // "Arcane", "Common", "Draconic", etc.
    public float Fluency;                      // 0.0 - 200.0 (0% to 200%)
    public float Experience;                   // XP toward next fluency level
    public long LastUsedTimestamp;             // For atrophy tracking
}

// Entity can have multiple languages (buffer)
public struct LanguageProficiencyBuffer : IBufferElementData
{
    public LanguageProficiency Language;
}

// Known spell signs
public struct SpellSignKnowledge : IComponentData
{
    public FixedString64Bytes SpellName;      // "Ignis Sagitta", "Lux Perpetua", etc.
    public FixedString64Bytes RequiredLanguage;
    public float RequiredFluency;              // Minimum % to cast
    public int TimesCast;                      // Mastery tracking
    public float FocusCost;                    // Base cost (modified by mastery)
    public float CastTime;                     // Base cast time (modified by mastery)
}

public struct SpellSignBuffer : IBufferElementData
{
    public SpellSignKnowledge Spell;
}

// Words of Power (rare, powerful)
public struct WordOfPowerKnowledge : IComponentData
{
    public FixedString64Bytes WordName;        // "VASTITAS", "RESTITUO", etc.
    public FixedString64Bytes Language;
    public int TimesUsed;
    public float FocusCost;
}

public struct WordOfPowerBuffer : IBufferElementData
{
    public WordOfPowerKnowledge Word;
}

// Ritual knowledge
public struct RitualKnowledge : IComponentData
{
    public FixedString64Bytes RitualName;
    public FixedString64Bytes PrimaryLanguage;
    public float RequiredFluency;
    public float InitialFocusCost;
    public float SustainFocusCostPerMinute;
    public int TimesPerformed;                 // Mastery tracking
    public byte Quality;                        // 1-100
    public ResourceQualityTier TierId;
}

public struct RitualBuffer : IBufferElementData
{
    public RitualKnowledge Ritual;
}

// Might/Magic axis
public struct MightMagicAxis : IComponentData
{
    public sbyte AxisPosition;                  // -100 (pure might) to +100 (pure magic)
    public sbyte RacialBase;                    // Racial default
    public sbyte IndividualModifier;            // Personal variance
    public float LifetimeShift;                 // Gradual shift from experience
}

// Xenophobic/Xenophilic axis
public struct XenophobiaAxis : IComponentData
{
    public sbyte AxisPosition;                  // -100 (xenophobic) to +100 (xenophilic)
    public sbyte CulturalBase;                  // Village/guild influence
    public sbyte IndividualModifier;            // Personal variance
}

// Active ritual (currently casting/sustaining)
public struct ActiveRitual : IComponentData
{
    public Entity RitualDefinition;             // Reference to ritual data
    public float FocusDrainPerSecond;
    public long StartTimestamp;
    public bool RequiresSpeech;                 // Can be sustained if silenced?
    public bool RequiresSomatic;                // Requires hand gestures?
}
```

### Systems

**`LanguageLearningSystem`** (Simulation Group, runs daily)
- Tracks entity conversations/exposure
- Calculates learning XP based on formulas
- Applies age/axis/xeno modifiers
- Updates fluency percentages
- Handles language atrophy for unused languages

**`SpellCastingSystem`** (Simulation Group, runs per-frame)
- Checks if entity can cast spell (focus available, language proficiency met, not silenced)
- Deducts focus cost (modified by mastery)
- Applies spell effects
- Tracks usage for mastery progression
- Handles critical success/failure rolls

**`RitualCastingSystem`** (Simulation Group)
- Initiates ritual casting (check requirements, deduct initial focus)
- Sustains ritual (drain focus per tick, monitor concentration)
- Detects interruptions (silence, damage, focus depletion)
- Ends ritual gracefully or via interruption

**`RitualLearningSystem`** (Simulation Group)
- Detects entities observing active rituals
- Rolls learning checks (INT/WIS vs. complexity)
- Grants ritual knowledge XP
- Unlocks ritual when XP threshold reached

**`WordOfPowerDiscoverySystem`** (Event-driven)
- Monitors entities reaching 200% fluency
- Rare random events grant Words through dreams/visions
- Tracks research activities (reading ancient texts, experimentation)
- Handles Word discovery (celebration event, notification)

**`MightMagicAxisUpdateSystem`** (Simulation Group, runs weekly)
- Tracks martial vs. magical actions
- Gradually shifts axis based on repeated actions
- Caps lifetime shift at ±20 from starting position
- Handles extreme events (sudden ±5 shifts)

---

## Open Questions

1. **Sigil Function Finalization:** Which sigil mechanics should be implemented first?
   - **Recommendation:** Start with spell storage (scrolls/wands) and ritual components (most gameplay utility)

2. **Word of Power Balance:** How rare should Words be? Risk vs. reward?
   - **Option A:** Extremely rare (1-2 per entity lifetime), massive power, high risk
   - **Option B:** Moderately rare (5-10 per master caster), situational power, moderate risk
   - **Recommendation:** Option A for Godgame (epic moments), Option B for Space4X (tech progression)

3. **Language Variety:** How many languages should exist?
   - **Option A:** Small set (5-10 languages, deep mechanics for each)
   - **Option B:** Large set (20-50 languages, procedural generation, less depth)
   - **Recommendation:** Option A for MVP, expand with community feedback

4. **Multilingual Advantage:** Should polyglots get mechanical bonuses beyond access?
   - **Option A:** No bonuses (access to more spells/rituals is reward enough)
   - **Option B:** Small bonuses (+5% trade/diplomacy per language known)
   - **Recommendation:** Option B (encourages linguistic diversity)

5. **Translation Magic:** Should "Comprehend Languages" spells exist?
   - **Option A:** Yes, but temporary and focus-intensive (50 focus, 10 min duration)
   - **Option B:** No, language barriers are core challenge
   - **Recommendation:** Option A with high cost (circumventable but not trivial)

6. **Perception Mechanics Implementation:** Required for silence-based stealth tactics
   - **Status:** Design captured, implementation pending
   - **Requirements:**
     - Auditory perception system (detect sounds: combat, cries, footsteps, equipment)
     - Visual perception system (line of sight, detect movement, blood, bodies)
     - Alert propagation system (entities communicate threats to allies)
     - Isolation detection (identify targets out of sight/hearing of allies)
   - **Integration Points:**
     - Stealth skill system
     - Silence spell effectiveness
     - Guard AI (patrol routes, buddy system, timed check-ins)
     - Ambusher AI (scout, silence, sequential elimination, body concealment)
   - **Note:** Proper perception mechanics enable expert ambushers to silence entire areas/bands and eliminate targets one by one without detection (requires 80+ stealth, 500+ silence mastery, 70+ focus capacity)

---

## Version History

- **v0.1 - 2025-12-02:** Initial concept capture based on design discussion

---

## Related Mechanics

- **Focus System:** [Focus_And_Status_Effects_System.md](../Implemented/Core/Focus_And_Status_Effects_System.md) - Focus costs for magic
- **Aggregate Entities:** [Aggregate_Entities_And_Individual_Dynamics.md](Aggregate_Entities_And_Individual_Dynamics.md) - Group language averages, cultural identity
- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Language mastery as progression path
- **Guild System:** [Guild_System.md](../Villagers/Guild_System.md) - Guild language requirements, might/magic specialization
- **Miracle System:** [Miracle_System_Vision.md](../Miracles/Miracle_System_Vision.md) - Divine language gifts, god-granted Words of Power

---

**Last Updated:** 2025-12-02
**Status:** Concept Captured - Core System Design
