# Individual Combat System

**Type:** Core Mechanic
**Status:** `<Draft>` - Core vision captured, implementation pending
**Version:** Concept v1.0
**Dependencies:** Individual Progression (Finesse, Strength, Will), Alignment Framework, Behavioral Personality, Entity Relations, Spies & Double Agents, Elite Courts, Stealth & Perception System, Education & Tutoring System
**Last Updated:** 2025-11-07

---

## Overview

**Individual combat** is the resolution system for one-on-one confrontations between entities. Combat can be honorable duels, street brawls, assassinations, champion trials, or desperate self-defense. Outcomes determine life/death, injuries, reputation, and relations.

**Key Features:**
- **Base Attributes:** Strength, Finesse, Will (modifiers for derived stats)
- **Derived Combat Stats:** Attack, Defense, Morale, Attack Speed, Attack Damage, Accuracy, Spell Power, Mana Pool, Critical Hit Chance, Health, Stamina
- **Combat Types:** Duel (honorable), Brawl (chaotic), Assassination (stealth kill), Trial by Combat (legal)
- **Weapons & Armor:** Broad categories (melee, ranged, magic), armor reduces physical damage with affixes
- **Combat Stances:** Determined by outlooks/alignments (bold/vengeful = aggressive)
- **Injury System:** Minor wounds, crippling injuries, death
- **Morale System:** Low morale = yield/fight worse, high morale = sustain pain
- **Alignment Impact:** Good entities prefer non-lethal, Evil entities finish enemies
- **Behavioral Modifiers:** Vengeful/bold fight to death, Craven/forgiving flee when losing

**For Narrative:** Samurai duel honor (controlled, formal), Game of Thrones trial by combat (champions representing justice), John Wick revenge fights (relentless pursuit), medieval tournament melees (chaotic brawls).

---

## Combat Stats

### Base Attributes (Experience Modifiers)

**Base attributes** are the foundation that calculate derived combat stats. They increase through experience, training, and life events.

**Strength (0-100):**
```
Represents: Physical power, muscle, endurance
Primary Modifier For:
  - Attack Damage
  - Health
  - Stamina

Sources:
  - Natural physique (birth stats, genetics)
  - Labor (blacksmiths, soldiers, farmers gain Strength)
  - Age (peak at 25-35, declines after 50)
  - Training (weapon practice, sparring)
```

**Finesse (0-100):**
```
Represents: Skill, speed, agility, precision, dexterity
Primary Modifier For:
  - Attack (hit chance)
  - Defense (dodge)
  - Accuracy
  - Attack Speed
  - Critical Hit Chance

Sources:
  - Natural talent (birth stats)
  - Training (combat practice, thief guild, assassin training)
  - Experience (each combat +1-3 Finesse, capped at 100)
  - Age (declines after 40)
```

**Will (0-100):**
```
Represents: Mental fortitude, courage, determination
Primary Modifier For:
  - Morale
  - Spell Power (if magic user)
  - Mana Pool (if magic user)

Sources:
  - Personality (Bold entities have high Will)
  - Experience (surviving near-death, torture, hardship)
  - Alignment (Lawful entities resist chaos, Good resist despair)
  - Faith (high god relation boosts Will)
```

### Derived Combat Stats

These are the **actual stats used in combat**, calculated from base attributes.

**Attack (To-Hit Chance):**
```
Attack = (Finesse × 0.7) + (Strength × 0.3) + WeaponAccuracyBonus

Example:
  Finesse 80, Strength 60, Longsword (+5 accuracy)
  → (80 × 0.7) + (60 × 0.3) + 5
  → 56 + 18 + 5 = 79 Attack

Attack vs Defense determines hit chance
```

**Defense (Dodge/Block Chance):**
```
Defense = (Finesse × 0.6) + ArmorDefenseBonus + ShieldDefenseBonus + DeflectionBonus

Example:
  Finesse 70, Plate Armor (+15 defense), Shield (+10 defense)
  → (70 × 0.6) + 15 + 10 + 0
  → 42 + 15 + 10 = 67 Defense

DeflectionBonus: +5% to +30% from Weapon Deflection training (see Education System)
  - Taught by high Finesse mentors (Finesse 80+)
  - Allows blocking attacks with weapon (not shield)
  - Requires high Finesse (60-80+)

Higher Defense reduces enemy hit chance

Note: Shield adds to Defense (dodge/block), NOT armor (damage reduction)
```

**Attack Damage:**
```
AttackDamage = (Strength × 0.5) + WeaponBaseDamage

Example:
  Strength 80, Longsword (15 base damage)
  → (80 × 0.5) + 15
  → 40 + 15 = 55 Attack Damage

Reduced by enemy armor
```

**Morale:**
```
Morale = (Will × 0.7) + PersonalityModifier + AlignmentModifier

PersonalityModifier:
  Bold (+80): +20 Morale
  Craven (+80): -20 Morale
  Vengeful (+80): +15 Morale (refuses to yield)
  Forgiving (+80): -10 Morale (yields easily)

AlignmentModifier:
  Lawful Good: +5 Morale (fights for justice)
  Chaotic Evil: +10 Morale (enjoys violence)

Example:
  Will 70, Bold (+20), Lawful Good (+5)
  → (70 × 0.7) + 20 + 5
  → 49 + 20 + 5 = 74 Morale

Morale determines:
  - Yield threshold (low Morale = yield early)
  - Pain tolerance (high Morale = fight while injured)
  - Fear resistance (high Morale = resists intimidation)
```

**Attack Speed (Attacks per Round):**
```
AttackSpeed = (Finesse × 0.8) + WeaponSpeedModifier

Example:
  Finesse 90, Dagger (+10 speed, fast weapon)
  → (90 × 0.8) + 10
  → 72 + 10 = 82 Attack Speed

Attack Speed 80+: 2 attacks per round
Attack Speed 50-79: 1 attack per round
Attack Speed <50: 1 attack every 2 rounds (slow)
```

**Accuracy:**
```
Accuracy = (Finesse × 0.9)

Example:
  Finesse 85
  → 85 × 0.9 = 76.5 Accuracy

Accuracy modifies hit chance against high-Defense targets
Accuracy 80+: +10% hit vs high Defense (80+)
```

**Critical Hit Chance:**
```
CriticalChance = (Finesse / 5) + WeaponCritBonus

Example:
  Finesse 90, Rapier (+3% crit bonus)
  → (90 / 5) + 3
  → 18 + 3 = 21% critical hit chance

Critical Hit: ×2 damage (bypasses 50% armor)
```

**Health (Hit Points):**
```
Health = (Strength × 0.6) + (Will × 0.4) + 50

Example:
  Strength 70, Will 60
  → (70 × 0.6) + (60 × 0.4) + 50
  → 42 + 24 + 50 = 116 HP

Base 50 HP ensures even weak entities survive 1-2 hits
```

**Stamina (Rounds Before Exhaustion):**
```
Stamina = (Strength / 10) + ArmorPenalty

Example:
  Strength 80, Plate Armor (-2 stamina penalty)
  → (80 / 10) - 2
  → 8 - 2 = 6 rounds before exhaustion

Exhaustion:
  - Attack -20%
  - Defense -20%
  - Attack Damage -30%
```

**Spell Power (Magic Users):**
```
SpellPower = (Will × 0.8) + Intelligence × 0.2

Example:
  Will 75, Intelligence 80 (mage)
  → (75 × 0.8) + (80 × 0.2)
  → 60 + 16 = 76 Spell Power

Spell Power determines magic damage and effectiveness
```

**Mana Pool (Magic Users):**
```
ManaPool = (Will × 0.5) + Intelligence × 0.5

Example:
  Will 70, Intelligence 85 (mage)
  → (70 × 0.5) + (85 × 0.5)
  → 35 + 42.5 = 77.5 Mana

Each spell costs 10-50 Mana
```

---

## Combat Resolution

### Hit Chance Calculation

```
HitChance = (AttackerAttack - DefenderDefense) + Modifiers

Example:
  Attacker Attack 79 vs Defender Defense 65
  → 79 - 65 = 14% base hit chance
  → Add modifiers (high ground +10%, flanking +15%)
  → Final: 14 + 10 + 15 = 39% hit chance

Modifiers:
  - High ground: +10% hit
  - Flanking: +15% hit
  - Darkness (attacker): -20% hit
  - Wounded attacker: -10% hit
  - Target prone: +20% hit

Minimum hit chance: 5% (always a chance)
Maximum hit chance: 95% (always can miss)
```

### Damage Calculation

```
If hit successful:
  RawDamage = AttackerAttackDamage + CriticalMultiplier

  If critical hit (roll < CriticalChance):
    RawDamage × 2

  FinalDamage = RawDamage - (DefenderArmor × ArmorEffectiveness)

Example:
  Attacker Attack Damage 55
  Critical hit (×2) → 110 raw damage
  Defender Armor 30 (plate armor, 0.7 effectiveness vs swords)
  → 110 - (30 × 0.7)
  → 110 - 21 = 89 damage taken

Armor Effectiveness:
  vs Swords/Slashing: 0.7 (70% effective)
  vs Maces/Blunt: 0.4 (40% effective, armor less effective)
  vs Piercing/Arrows: 0.5 (50% effective)
  vs Magic: 0.1 (10% effective, armor weak vs spells)
```

---

## Combat Types

### 1. Duel (Honorable Combat)

**Purpose:** Formal, rule-bound combat for honor, justice, or status.

**Initiation:**
```
Conditions:
  - Challenge issued (relation < 0 OR grievance exists)
  - Challenged must accept (or lose honor/reputation -10)
  - Witnesses present (minimum 3 observers)
  - Alignment: Lawful entities prefer duels over brawls

DuelRules:
  - Fight to first blood (non-lethal, honor satisfied)
  - Fight to yield (loser surrenders, survives)
  - Fight to death (mortal combat, winner takes all)

Accepted by Alignment:
  Lawful Good: First blood (mercy preferred)
  Lawful Neutral: To yield (pragmatic)
  Lawful Evil: To death (eliminate rivals)
  Chaotic (any): May break duel rules mid-combat
```

**Mechanics:**

```
Turn Order: Determined by Initiative (Finesse + d20 roll)

Each Round:
  1. Stance Selection (Aggressive, Defensive, Balanced)
  2. Attack Roll (Finesse + modifiers vs Defender Finesse)
  3. Damage Roll (Strength + weapon + modifiers)
  4. Armor Reduction (Defender armor mitigates damage)
  5. Health Check (HP reduced, injury thresholds)
  6. Morale Check (Will check if injured, flee/continue)

Duration: 3-10 rounds (average 5 rounds)
```

**Outcomes:**

```
Victory (Kill):
  - Winner gains reputation (+10 if honorable, +5 if dirty)
  - Winner may loot loser (weapon, armor, currency)
  - Loser's family relation: -60 (killed in duel, grudge)

Victory (Yield):
  - Winner gains honor (+15, merciful victor)
  - Loser survives but humiliated (reputation -15)
  - Loser relation to winner: -40 (resentment) OR +20 (respect, if honorable)

First Blood:
  - Both survive, honor satisfied
  - Winner gains minor reputation (+5)
  - Loser relation: -20 (lost, but alive)
  - Grievance resolved (no further combat)

Mutual Death:
  - Both killed (rare, 5% chance if evenly matched)
  - Families mourn, no clear winner
  - Generational feud may continue
```

**Example Duel:**

```
Challenger: Sir Aldric (Combat Rating 85, Longsword, Plate Armor)
Challenged: Baron Erik (Combat Rating 78, Mace, Chainmail)
Grievance: Aldric accuses Erik of stealing land deed
Rules: To yield (legal duel, witnesses present)

Round 1:
  Initiative: Aldric 15, Erik 12 → Aldric strikes first
  Aldric: Balanced Stance (+0 hit, +0 damage)
  Attack Roll: 75 (hit), Damage: 22
  Erik Armor: 22 - (12 chainmail × 0.4 mace weakness) = 17 damage
  Erik HP: 100 → 83

Round 2:
  Erik: Aggressive Stance (+10 hit, +5 damage)
  Attack Roll: 68 (hit), Damage: 20 + 5 = 25
  Aldric Armor: 25 - (15 plate × 0.7) = 14.5 damage
  Aldric HP: 100 → 85.5

Round 3:
  Aldric: Aggressive Stance (press advantage)
  Attack Roll: 82 (critical hit), Damage: 22 × 2 = 44
  Erik Armor: 44 - 4.8 = 39 damage
  Erik HP: 83 → 44 (badly injured)

Round 4:
  Erik: Morale Check (Will 60, injured 56% HP)
    Roll: 45 (passes, continues fighting)
  Erik: Defensive Stance (-15% hit, reduce damage taken)
  Attack Roll: 50 (miss, Aldric dodges)

Round 5:
  Aldric: Balanced Stance (maintain control)
  Attack Roll: 79 (hit), Damage: 22
  Erik Armor: 22 - 4.8 = 17 damage
  Erik HP: 44 → 27 (critical injury, below 30%)

  Erik: "I yield! You have won, Sir Aldric."

Outcome:
  - Aldric victorious (honor +15, reputation +15)
  - Erik survives, land deed returned (humiliation -15 reputation)
  - Erik relation to Aldric: -40 (resentment, but respects skill)

Narrative: "The baron's mace rang against Aldric's plate. But skill prevailed over rage. When Erik knelt, bloodied, Aldric extended a hand. 'Keep your life, Baron. Return what you stole.' Erik nodded, defeated but alive."
```

### 2. Brawl (Chaotic Combat)

**Purpose:** Unstructured, spontaneous violence (bar fights, street combat, mob justice).

**Initiation:**

```
Triggers:
  - Insult (relation -30+, Vengeful personality)
  - Drunken aggression (tavern, festival)
  - Robbery attempt (thief vs victim)
  - Mob violence (crowd attacks individual)

No Rules:
  - No witnesses required
  - Dirty tactics allowed (eye gouging, groin strikes)
  - Multiple combatants possible
  - Chaotic entities initiate brawls 3× more often
```

**Mechanics:**

```
Less Structured Than Duels:
  - No formal stance selection (random/instinct)
  - Higher critical miss chance (10% vs 5% in duel)
  - Dirty tactics available (+15% hit, -10 honor)
  - Can flee mid-combat (no honor penalty)

Dirty Tactics:
  - Throw dirt in eyes: -30% defender hit next round
  - Groin strike: ×1.5 damage, -10 honor
  - Bite/claw: +5 damage, -5 honor
  - Trip/grapple: Strength check, immobilize enemy

Duration: 2-6 rounds (faster, more chaotic)
```

**Outcomes:**

```
Victory (Kill):
  - Winner gains no honor (murder, not duel)
  - Witnesses may report to authorities (Lawful village → arrest)
  - Loser's family: -80 relation (murdered in street)

Victory (Knockout):
  - Winner subdues loser (unconscious)
  - Can rob, capture, or leave
  - Loser wakes 10 minutes later (injured, humiliated)

Flight (One Flees):
  - Fleeing combatant survives (Craven personality likely)
  - Winner gains reputation +5 (drove off attacker)
  - Fleeing combatant: -10 reputation (coward)

Intervention (Guards Arrive):
  - Combat stopped by peacekeepers
  - Both arrested (if Lawful village)
  - Fines or imprisonment based on alignment
```

**Example Brawl:**

```
Combatants: Marcus (drunk, Chaotic Neutral) vs Theron (insulted, Vengeful)
Location: Village tavern
Trigger: Marcus insults Theron's family

Round 1:
  Marcus: Drunken swing (Finesse reduced -20)
  Attack Roll: 40 (miss, too drunk)
  Theron: Aggressive counterattack
  Attack Roll: 75 (hit), Damage: 18 (fist, no weapon)
  Marcus HP: 80 → 62

Round 2:
  Marcus: Dirty tactic (throw mug at Theron's face)
  Attack Roll: 65 (hit), Damage: 10 (improvised weapon)
  Theron HP: 90 → 80
  Marcus: Follow-up punch
  Attack Roll: 70 (hit), Damage: 15
  Theron HP: 80 → 65

Round 3:
  Theron: Enraged (Vengeful personality, +10 damage)
  Attack Roll: 85 (critical hit), Damage: 18 × 2 = 36
  Marcus HP: 62 → 26 (badly injured)

Round 4:
  Marcus: Morale Check (Will 40, injured to 32% HP)
    Roll: 60 (fails, flees combat)
  Marcus runs out of tavern, escapes

Outcome:
  - Theron victorious (reputation +5, defended honor)
  - Marcus fled (reputation -10, coward)
  - Marcus relation to Theron: -60 (humiliated, vengeful)
  - Tavern owner demands payment for broken furniture (20 currency)

Narrative: "Marcus's insults cost him teeth. Theron's fists rained down. When Marcus ran, bloodied and broken, Theron let him go. The tavern fell silent. 'Who's next?' Theron asked. No one answered."
```

### 3. Assassination (Stealth Kill)

**Purpose:** Eliminate target without fair combat (spies, assassins, political murders).

**Initiation:**

```
Conditions:
  - Target unaware (stealth check required)
  - Assassin has weapon (dagger, poison, garrote)
  - No witnesses (or witnesses silenced)

Success Factors:
  AssassinationChance =
    AssassinFinesse +
    TargetAwareness (negative) +
    StealthModifiers +
    WeaponSuitability

  If chance > 70: Instant kill (silent)
  If chance 40-69: Combat initiated (target aware)
  If chance < 40: Failed, target alerted, assassin identified
```

**Mechanics:**

```
Stealth Approach:
  - Assassin makes Finesse check vs Target Perception
  - Darkness bonus: +20 Finesse (nighttime)
  - Target asleep: +30 Finesse (defenseless)
  - Target has bodyguard: -30 Finesse (protected)

Execution:
  Success (target unaware):
    - Instant kill (throat slit, poison, strangulation)
    - No combat rounds needed
    - Silent (no alarm raised)

  Partial (target aware at last second):
    - Target screams OR dodges
    - Combat initiated (defensive stance)
    - Guards alerted (arrive in 2-5 rounds)

  Failure (detected before strike):
    - Target sees assassin approaching
    - Alarm raised immediately
    - Assassin must flee or fight
```

**Outcomes:**

```
Success (Undetected Kill):
  - Target dead, no one knows assassin's identity
  - Employer pays completion bonus
  - Family investigates (counter-spy, if wealthy)

Success (Detected Kill):
  - Target dead, assassin identified
  - Assassin wanted (arrest on sight)
  - Family relation: -90 (murdered, known killer)

Failure (Target Survives):
  - Combat ensues OR assassin captured
  - Target relation: -100 (assassination attempt, eternal enemy)
  - Assassin reputation: -20 (failed mission)

Capture:
  - Interrogation (reveal employer if Charisma check fails)
  - Execution (public, to deter future assassins)
  - Employer exposed (diplomatic crisis)
```

**Example Assassination:**

```
Assassin: Elira (Finesse 90, Spy, hired by Duke)
Target: Baron Erik (Perception 50, asleep in bed)
Mission: Silent kill, no detection

Approach Phase:
  - Night infiltration (darkness +20 Finesse)
  - Baron asleep (+30 Finesse)
  - No bodyguard (Baron trusts his guards outside)
  - Elira lockpicks bedroom window (Finesse check: 95, success)

Stealth Check:
  Elira Finesse 90 + 20 (dark) + 30 (asleep) = 140
  Baron Perception 50 (asleep, reduced to 10)
  → 140 vs 10 = Automatic success

Execution:
  - Elira approaches bed silently
  - Dagger to throat (instant kill)
  - Baron dies without waking

Escape:
  - Elira exits via window
  - Guards discover body at dawn (6 hours later)
  - No evidence, no witnesses

Outcome:
  - Baron dead (appears natural or unknown assailant)
  - Duke pays Elira 2500 currency
  - Baron's family hires investigators (50% chance to eventually identify Elira)

Narrative: "He never woke. The blade was mercy, in its way—quick, silent. By dawn, the Baron was cold. By noon, the duchy was in chaos."
```

### 4. Trial by Combat (Legal Duel)

**Purpose:** Judicial resolution where combat determines guilt/innocence (champion represents defendant).

**Initiation:**

```
Conditions:
  - Legal accusation (theft, murder, treason)
  - Lawful village permits trial by combat
  - Defendant hires champion OR fights self
  - Accuser hires champion OR fights self

ChampionSelection:
  - Wealthy defendants hire professional champions (High Combat Rating)
  - Poor defendants fight themselves (disadvantaged)
  - Village provides champion for destitute (Lawful Good only)

Rules:
  - Fight to yield or death (accuser's choice)
  - Witnesses mandatory (judge, crowd)
  - Winner's side is "proven right by gods/justice"
```

**Mechanics:**

```
Same as Duel mechanics, but:
  - Outcome determines legal verdict
  - Champion's death ≠ employer's death (champion sacrifices)
  - Village enforces verdict (execution, exile, exoneration)

Stakes:
  If Defendant's Champion Wins:
    - Defendant exonerated (innocent by divine judgment)
    - Accuser's champion dead or yields
    - Accuser may be punished for false accusation (if Evil village)

  If Accuser's Champion Wins:
    - Defendant guilty (proven by combat)
    - Defendant executed, imprisoned, or fined
    - Champion paid (1000-5000 currency)
```

**Example Trial by Combat:**

```
Defendant: Marcus (accused of stealing lord's gold, 500 currency)
Accuser: Lord Aldric (powerful noble, certain of guilt)

Marcus: Poor, cannot afford champion (Combat Rating 45, no combat training)
Aldric: Hires champion Sir Gareth (Combat Rating 95, legendary knight)

Trial:
  - Village Lawful Neutral, permits trial by combat
  - Rules: To death (Aldric demands it, wants example made)
  - Witnesses: 200 villagers, judge, council

Combat:
  Round 1:
    Gareth attacks (Finesse 90 vs Marcus Finesse 40)
    Hit: 90%, Damage: 30
    Marcus HP: 70 → 40 (critically injured immediately)

  Round 2:
    Marcus: Defensive stance (desperate)
    Gareth attacks: Hit 85%, Damage: 28
    Marcus HP: 40 → 12 (near death)

  Round 3:
    Marcus: Morale check (Will 30, at 17% HP)
      Roll: 65 (fails, attempts to yield)
    Marcus: "I yield! Mercy!"

    Gareth looks to Aldric
    Aldric: "No mercy. Finish him."

    Gareth executes Marcus (killing blow)

Verdict:
  - Marcus guilty (by combat judgment)
  - Marcus executed publicly (body displayed as warning)
  - Gareth paid 3000 currency by Aldric
  - Village relation to Aldric: -10 (harsh, but legal)

  10 years later:
    Marcus was actually innocent (framed by Aldric's steward)
    Truth revealed by deathbed confession
    Village relation to Aldric: -50 (executed innocent man)

Narrative: "The trial was swift. The champion, merciless. Marcus died screaming his innocence. Years later, the truth emerged. By then, it was too late. Justice, they called it. Murder, history would say."
```

---

## Combat Stances

**Combat stances are automatically determined** by entity's alignment, outlooks, and personality. Stances affect combat modifiers.

### Stance Determination

```
Stance is determined by:
  - Alignment (Good/Evil, Lawful/Chaotic)
  - Outlook (Vengeful/Forgiving, Bold/Craven, Warlike/Peaceful)
  - Current HP (injured entities may switch to defensive)

StanceSelection:
  IF Bold (+60+) OR Vengeful (+60+) OR Warlike (+60+):
    → Aggressive Stance

  ELSE IF Craven (+60+) OR Forgiving (+60+) OR Peaceful (+60+):
    → Defensive Stance

  ELSE IF Chaotic Evil AND (HP < 50% OR desperate):
    → Reckless Stance

  ELSE:
    → Balanced Stance

Dynamic Stance Switching:
  - If HP < 30%: Craven entities switch to Defensive
  - If HP < 20%: Bold entities switch to Reckless (desperation)
  - If winning (enemy HP < 50%): Vengeful switches to Aggressive (finish them)
```

### Aggressive Stance
```
Automatic Selection:
  - Bold (+60+)
  - Vengeful (+60+)
  - Warlike (+60+)
  - Chaotic Evil (enjoys combat)

Effects:
  - Attack +10%
  - Attack Damage +10%
  - Defense -10%

Philosophy: "Press the advantage. Show no mercy."
```

### Defensive Stance
```
Automatic Selection:
  - Craven (+60+)
  - Forgiving (+60+)
  - Peaceful (+60+)
  - Lawful Good (values life)
  - Injured (HP < 30%)

Effects:
  - Defense +20%
  - Damage Taken -20%
  - Attack -10%

Philosophy: "Survive. Yield if necessary. Life is precious."
```

### Balanced Stance
```
Automatic Selection:
  - True Neutral alignment
  - Moderate personalities (no extreme outlooks)
  - Pragmatic entities

Effects:
  - No modifiers (standard combat)

Philosophy: "Adapt to the situation. Neither aggressive nor defensive."
```

### Reckless Stance
```
Automatic Selection:
  - Chaotic Evil (desperate OR bloodlust)
  - HP < 20% AND Bold (last stand)
  - Berserker-type entities

Effects:
  - Attack Damage +20%
  - Critical Hit Chance +10%
  - Defense -30%
  - 15% chance self-injury per round

Philosophy: "All or nothing. Death or glory."
```

### Example Stance Assignments

```
Sir Aldric (Lawful Good, Bold +70, Warlike +40):
  → Aggressive Stance (Bold dominates)
  → Philosophy: "Fight honorably but decisively."

Lady Elira (Lawful Good, Craven +80, Peaceful +90):
  → Defensive Stance (Craven + Peaceful)
  → Philosophy: "Protect myself, yield if outmatched."

Bandit King (Chaotic Evil, Vengeful +90, Bold +60):
  → Aggressive Stance (Vengeful + Bold)
  → If HP < 20%: Switch to Reckless (desperation)
  → Philosophy: "Kill them all. No survivors."

Merchant Marcus (True Neutral, all outlooks moderate):
  → Balanced Stance
  → Philosophy: "Fight pragmatically. Flee if losing."
```

---

## Weapons & Armor

### Weapon Categories (Broad)

**Weapons will be detailed in future iterations. For now, broad categories:**

**Melee Weapons:**
```
Light Melee (Daggers, Short Swords):
  - Base Damage: 10-20
  - Accuracy Bonus: +5 to +15
  - Speed: Fast (Attack Speed +10)
  - Best For: High Finesse combatants, assassins

Medium Melee (Swords, Maces, Axes):
  - Base Damage: 20-35
  - Accuracy Bonus: +0 to +10
  - Speed: Medium (Attack Speed +0)
  - Best For: Balanced combatants, soldiers

Heavy Melee (Greatswords, War Hammers):
  - Base Damage: 35-50
  - Accuracy Bonus: -5 to +0
  - Speed: Slow (Attack Speed -10)
  - Armor Penetration: High
  - Best For: High Strength combatants, champions
```

**Ranged Weapons:**
```
Bows:
  - Base Damage: 15-30
  - Range: 50-200 meters
  - Accuracy Bonus: +5 (aimed shots)
  - Best For: High Finesse, ambush

Crossbows:
  - Base Damage: 25-40
  - Range: 30-150 meters
  - Reload: 1 round (slower)
  - Armor Penetration: High
  - Best For: Medium Strength, siege

Thrown (Javelins, Knives):
  - Base Damage: 10-25
  - Range: 10-30 meters
  - Speed: Fast (no reload)
  - Best For: Skirmishers, quick attacks
```

**Magic Weapons (Future):**
```
Staves, Wands:
  - Spell Power Bonus: +10 to +30
  - Mana Pool Bonus: +10 to +20
  - Base Damage: Spell-dependent
  - Best For: Mages, clerics
```

### Armor Categories

**Armor reduces physical damage and provides defense bonuses.**

**Light Armor:**
```
Examples: Leather, Cloth, Light padding
Defense Bonus: +5 to +15
Damage Reduction: 20-30%
Stamina Penalty: 0 (no exhaustion penalty)
Best For: High Finesse (scouts, thieves, mages)

Affixes (future):
  - +Dodge%
  - +Movement Speed
  - +Critical Hit Chance
```

**Medium Armor:**
```
Examples: Chainmail, Scale mail, Reinforced leather
Defense Bonus: +15 to +30
Damage Reduction: 40-60%
Stamina Penalty: -1 round
Best For: Balanced combatants (soldiers, adventurers)

Affixes (future):
  - +Defense vs specific damage types
  - +Health
  - +Stamina
```

**Heavy Armor:**
```
Examples: Plate armor, Full plate, Dragon scale
Defense Bonus: +30 to +50
Damage Reduction: 60-80%
Stamina Penalty: -2 to -3 rounds
Requires: Strength 60+ (otherwise -20% all combat stats)
Best For: High Strength champions, knights

Affixes (future):
  - +Armor Penetration Resistance
  - +Morale
  - +Intimidation (fear aura)
```

**Shields:**
```
Defense Bonus: +8 to +20 (raises dodge/block chance, NOT armor)
Projectile Block Chance: 30-50% (arrows, bolts, thrown weapons)
Stamina Penalty: -1 round (heavy)
Best For: Defensive stance combatants, ranged defense

Shield Types:
  - Basic Shield: +10 Defense, 30% projectile block
  - Tower Shield: +20 Defense, 50% projectile block, -2 stamina
  - Magical Shield: +15 Defense, 40% projectile block, 20% spell block
  - Reflective Shield: +12 Defense, 35% projectile block, 15% spell reflect (rare)

Affixes (future):
  - +Defense%
  - +Projectile Block Chance%
  - +Spell Block Chance%
  - +Spell Reflect Chance%
  - +Bash attack (offensive use)

Note: Shields do NOT provide armor (damage reduction)
      Shields provide Defense (dodge/block) and projectile/spell blocking
```

### Armor Effectiveness vs Damage Types

```
Physical Damage:
  Light Armor: 30% reduction
  Medium Armor: 50% reduction
  Heavy Armor: 70% reduction

Blunt Weapons (Maces, Hammers):
  - Bypass 40% armor (blunt force trauma)
  - Heavy Armor less effective

Piercing Weapons (Arrows, Spears):
  - Bypass 30% armor (penetrate gaps)
  - Medium effectiveness

Slashing Weapons (Swords, Axes):
  - Standard armor effectiveness
  - Deflected by heavy armor

Magic Damage:
  All Armor: 10-20% reduction (armor weak vs magic)
  Exception: Enchanted armor (future)
```

---

## Morale & Yield System

**Morale** is the key stat determining when entities yield, flee, or fight to the death.

### Morale Checks

```
Morale checks occur when:
  - HP drops below 50% (first check)
  - HP drops below 30% (second check)
  - HP drops below 10% (desperation check)
  - Ally dies nearby (witness death)
  - Outnumbered 3-to-1 or more

MoraleCheckRoll = d100 vs CurrentMorale

If roll > Morale:
  → Entity attempts to yield OR flee (based on personality)

If roll ≤ Morale:
  → Entity continues fighting

Example:
  Entity with Morale 60, HP drops to 40% (below 50%)
  → Roll d100: Result 75
  → 75 > 60 → Failed morale check
  → Entity yields: "I surrender!"
```

### Yield Behavior

```
WhenMoraleCheckFails:

IF Craven (+60+) OR Forgiving (+60+):
  → Yield immediately (plead for life)
  → "I yield! Please, mercy!"

ELSE IF Bold (+60+) OR Vengeful (+60+):
  → Continue fighting despite morale check
  → Ignore yield impulse (fight to death)

ELSE IF Pragmatic (True Neutral):
  → Yield if enemy accepts yields (Lawful Good)
  → Flee if enemy executes prisoners (Evil)

Example:
  Craven merchant (Morale 40, HP 25%)
  → Morale check: Roll 65 > 40 (fail)
  → Yields: "Take my gold! Just spare me!"

  Bold knight (Morale 80, HP 25%)
  → Morale check: Roll 85 > 80 (fail)
  → Ignores: "I may die, but I will not yield!"
```

### Pain Tolerance

```
HighMorale (70+):
  - Can fight while injured (HP < 30%)
  - Ignores pain penalties
  - -10% attack penalty (instead of -30%)

ModerateMorale (40-69):
  - Fights normally until morale check
  - Standard pain penalties
  - -20% attack penalty when injured

LowMorale (<40):
  - Yields early (HP < 50%)
  - High pain penalties
  - -40% attack penalty when injured

Example:
  High Morale champion (Morale 85, HP 20%)
  → Attack penalty: -10% (tolerates pain)
  → Continues fighting effectively

  Low Morale bandit (Morale 30, HP 45%)
  → Attack penalty: -40% (panicked)
  → Likely to flee
```

---

**Bows (Ranged):**
```
Damage: 8-12 (moderate)
Range: 50-200 meters
Hit Penalty: -10% per 50m distance
Best Against: Unarmored, leather armor
Cost: 60-150 currency

Mechanics:
  - Requires 1 round to nock arrow
  - Finesse check for accuracy
  - Defender cannot dodge if unaware (sniper shot)
  - Ineffective vs plate armor (arrows deflect)
```

**Spears (Reach, Defensive):**
```
Damage: 10-14 (moderate)
Hit Bonus: +10% (reach advantage)
Defense Bonus: +10% dodge (keep enemy at distance)
Best Against: Cavalry, aggressive enemies
Cost: 40-120 currency

Examples:
  - Short Spear: 10 damage, +10% hit, one-handed
  - Pike: 14 damage, +15% hit, two-handed, anti-cavalry
```

### Armor Types

**Plate Armor (Heavy, Protective):**
```
Armor Value: 15-20
Effectiveness: 0.7 vs swords, 0.4 vs maces, 0.3 vs arrows
Dodge Penalty: -15% (heavy, cumbersome)
Stamina Drain: -2 rounds (exhausts faster)
Best For: High Strength combatants (can wear without penalty)
Cost: 500-2000 currency

Requirements:
  - Strength 60+ (otherwise -20% hit from exhaustion)
```

**Chainmail (Medium, Flexible):**
```
Armor Value: 10-15
Effectiveness: 0.6 vs swords, 0.5 vs maces, 0.4 vs arrows
Dodge Penalty: -5% (somewhat heavy)
Stamina Drain: -1 round
Best For: Balanced combatants (Finesse + Strength)
Cost: 200-800 currency

Requirements:
  - Strength 40+
```

**Leather Armor (Light, Mobile):**
```
Armor Value: 5-8
Effectiveness: 0.5 vs swords, 0.3 vs arrows, 0.2 vs maces
Dodge Penalty: +0% (no hindrance)
Stamina Drain: 0 (no exhaustion)
Best For: High Finesse combatants (spies, scouts, thieves)
Cost: 50-150 currency

No Requirements
```

**Shields (Defensive, Blocking):**
```
Armor Value: 8-12 (bonus to existing armor)
Block Chance: 30% (complete block, no damage)
Dodge Penalty: -5% (one hand occupied)
Best For: Defensive stance, Lawful Good (protect others)
Cost: 60-200 currency

Types:
  - Buckler: 8 armor, 20% block, +0% dodge
  - Kite Shield: 12 armor, 30% block, -5% dodge
  - Tower Shield: 15 armor, 40% block, -15% dodge (very heavy)
```

---

## Injury & Death System

### Health Points (HP)

**HP Range:**
```
Base HP = (Strength + Will) / 2

Example:
  Strength 70, Will 60
  → HP = (70 + 60) / 2 = 65 HP

HP Ranges by Strength/Will:
  Weak (Str 30, Will 30): 30 HP (fragile, die quickly)
  Average (Str 50, Will 50): 50 HP (typical villager)
  Strong (Str 70, Will 70): 70 HP (soldier, guard)
  Champion (Str 90, Will 90): 90 HP (elite fighter)
  Legendary (Str 100, Will 100): 100 HP (peak human)
```

### Injury Thresholds

**HP Remaining:**

```
100-80% HP: Uninjured (minor scratches)
  - No penalties
  - Fighting at full capacity

79-50% HP: Minor Wounds (bruised, bleeding)
  - -5% hit chance (pain distraction)
  - -2 damage (weakened strikes)
  - Cosmetic scars (if survive)

49-30% HP: Serious Wounds (deep cuts, broken bones)
  - -15% hit chance (severe pain)
  - -5 damage (struggling to fight)
  - Morale check required (Will check, continue or flee)
  - Permanent injury risk (10% chance per round)

29-1% HP: Critical Injury (near death)
  - -30% hit chance (barely conscious)
  - -10 damage (desperate)
  - Morale check each round (Will check, continue or collapse)
  - Permanent injury risk (40% chance)
  - Death saving throw if hit again (Will check, survive or die)

0 HP: Death
  - Instant death (heart stopped, throat slit, etc.)
  - OR Death saving throw (Will × 0.5 = % chance to survive at 1 HP)
```

### Permanent Injuries

**If HP drops below 30%, roll for permanent injury (based on damage type):**

```
Permanent Injuries (10-40% chance based on severity):

1. Lost Eye (Perception -30, -15% ranged hit)
2. Crippled Arm (Strength -20, cannot wield two-handed weapons)
3. Crippled Leg (Finesse -20, -20% dodge, slower movement)
4. Broken Ribs (Stamina -2 rounds, -5 damage)
5. Facial Scars (Charisma -10, intimidating appearance)
6. Lost Fingers (Finesse -15, -10% hit with melee)
7. Internal Bleeding (Max HP -20 permanently)
8. Traumatic Brain Injury (Intelligence -15, Will -10)

Recovery:
  - Permanent injuries never fully heal
  - Medical treatment can reduce penalty by 50% (costs 200-1000 currency)
  - Magical healing can remove injury (rare, expensive)
```

### Death Saving Throw

**When HP reaches 0, roll for survival:**

```
DeathSavingThrow = Will × 0.5

Example:
  Will 80 → 80 × 0.5 = 40% chance to survive

Modifiers:
  - Allies present (can provide aid): +10%
  - Medical treatment immediate: +20%
  - Combat continues (no one helps): -20%
  - Execution (deliberate killing blow): -100% (automatic death)

If Successful:
  - Survive at 1 HP (unconscious)
  - Permanent injury roll (80% chance)
  - Recover consciousness after 1-6 hours

If Failed:
  - Death (permanent)
  - Relations impact (family, friends grieve)
  - Inheritance triggered (if elite)
```

---

## Combat Outcomes & Consequences

### Victory Outcomes

**Kill Enemy:**
```
Immediate:
  - Loot enemy (weapon, armor, currency)
  - Reputation based on combat type:
    * Honorable duel: +10 reputation
    * Assassination: +0 reputation (secret)
    * Murder/brawl: -5 reputation (violence)

Relations:
  - Enemy's family: -60 to -90 (grief, vendetta)
  - Enemy's friends: -40 (mourning, suspicion)
  - Witnesses: +5 if justified, -20 if murder

Narrative Hooks:
  - Family hires assassin for revenge (5-20 years later)
  - Children swear vendetta (generational feud)
  - Village holds trial (if murder, not duel)
```

**Capture Enemy:**
```
Options:
  1. Ransom (demand payment from family)
  2. Imprison (hold indefinitely, slavery)
  3. Execute (public or private)
  4. Release (mercy, gain honor +15)

Ransom:
  - Family pays 500-10,000 currency (based on wealth)
  - Enemy relation: -50 (captured, humiliated)
  - Released after payment

Execution:
  - Public: Intimidates enemies (+10 fear reputation)
  - Private: No witnesses, secret murder
```

**Enemy Flees:**
```
Immediate:
  - Winner gains reputation +5 (drove off enemy)
  - Enemy survives (may return for revenge)
  - Enemy relation: -70 (defeated, humiliated)

Pursuit:
  - Winner can chase (Finesse check vs enemy Finesse)
  - If caught: Combat resumes
  - If escaped: Enemy flees village (exile)
```

### Defeat Outcomes

**Killed in Combat:**
```
Immediate:
  - Lose all equipment (looted by winner)
  - Family/friends grief (morale -20)
  - Inheritance triggered (if elite, business owner)

Long-term:
  - Family may seek revenge (hire assassin)
  - Business/assets redistributed
  - Spouse remarries (if young, after mourning period)
```

**Captured:**
```
Ransom Demanded:
  - Family decides whether to pay (alignment-dependent)
  - Lawful Good: Always pays (values life)
  - Neutral: Negotiates (pragmatic)
  - Chaotic Evil: Refuses (expendable)

If Ransomed:
  - Released, -50 relation to captor
  - Reputation -10 (captured, weak)

If Not Ransomed:
  - Executed OR enslaved
  - Family relation to captor: -100 (abandoned to die)
```

**Fled Combat:**
```
Immediate:
  - Survive (Craven personality likely)
  - Reputation -10 (coward, fled)
  - Enemy relation: -40 (unresolved conflict)

Long-term:
  - May rebuild and return (if Vengeful)
  - Reputation recovers slowly (+1/year)
  - Friends respect survival (Bold friends: -10 relation, Craven friends: +5)
```

---

## Alignment Impact on Combat

### Combat Methods by Alignment

**Lawful Good:**
```
Preferred:
  - Duels (honorable, fair)
  - Non-lethal (to yield, first blood)
  - Mercy (spare defeated enemies)
  - Trial by combat (legal justice)

Avoids:
  - Assassination (dishonorable)
  - Dirty tactics (eye gouging, poison)
  - Execution of prisoners (unless extreme crimes)

If Forced to Kill:
  - Clean death (quick, painless)
  - Burial with honors (respect enemy)
  - Guilt (Will -5 temporarily if kills innocent)
```

**Lawful Neutral:**
```
Preferred:
  - Duels (rule-bound)
  - Pragmatic (kill if necessary, spare if beneficial)
  - Legal combat (trial by combat)

Methods:
  - Efficient (end fight quickly)
  - No mercy for criminals (follow law)
  - Ransom if profitable (transactional)
```

**Lawful Evil:**
```
Preferred:
  - Duels (to death, eliminate rivals)
  - Assassination (if legal/justifiable)
  - Execution (public, intimidation)
  - Torture (extract information, punish)

Methods:
  - Systematic (plan every move)
  - Brutal (make example of enemies)
  - No mercy (weakness invites rebellion)

If Victorious:
  - Execute prisoners (prevent future threat)
  - Display corpses (fear tactics)
```

**Neutral Good:**
```
Preferred:
  - Self-defense (reluctant to initiate)
  - Protect innocents (intervene in attacks)
  - Non-lethal (if possible)

Methods:
  - Defensive (wait for enemy aggression)
  - Spare defeated (offer second chances)
  - Capture over kill (reform criminals)

If Forced to Kill:
  - Quick death (minimize suffering)
  - Regret (no celebration)
```

**True Neutral:**
```
Preferred:
  - Pragmatic (whatever works)
  - Self-preservation (flee if losing)
  - Transactional (mercenary, paid combat)

Methods:
  - Efficient (balanced tactics)
  - No strong preference (adapts to situation)
  - Spare or kill based on benefit

If Victorious:
  - Ransom (profit motive)
  - Kill if threat (eliminate danger)
```

**Neutral Evil:**
```
Preferred:
  - Assassination (easiest method)
  - Ambush (unfair advantage)
  - Poison (no risk to self)
  - Dirty tactics (eye gouging, groin strikes)

Methods:
  - Pragmatic cruelty (whatever achieves goal)
  - No honor (survival over fairness)
  - Torture if useful (information extraction)

If Victorious:
  - Loot corpses (profit)
  - Execute prisoners (no witnesses)
```

**Chaotic Good:**
```
Preferred:
  - Self-defense (protect innocents)
  - Vigilante justice (kill criminals)
  - Break duel rules (if enemy cheats first)

Methods:
  - Unpredictable (adapt to enemy)
  - Mercy for repentant (second chances)
  - No mercy for evil (execute villains)

If Victorious:
  - Kill evil enemies (prevent future harm)
  - Spare confused/misguided (rehabilitation)
```

**Chaotic Neutral:**
```
Preferred:
  - Whatever feels right (mood-dependent)
  - Brawls (spontaneous violence)
  - Reckless stance (high risk, high reward)

Methods:
  - Unpredictable (random tactics)
  - May spare OR execute (coin flip)
  - Dirty tactics if fun (chaotic enjoyment)

If Victorious:
  - Random outcome (let dice decide enemy's fate)
```

**Chaotic Evil:**
```
Preferred:
  - Murder (no rules)
  - Torture (enjoy suffering)
  - Ambush/assassination (unfair advantage)
  - Mutilation (send message)

Methods:
  - Brutal (maximize pain)
  - No mercy (kill all enemies)
  - Desecrate corpses (psychological warfare)

If Victorious:
  - Torture to death (enjoy cruelty)
  - Display corpses (terrorize survivors)
  - Kill families (eliminate bloodlines)
```

---

## Behavioral Modifiers

### Vengeful vs Forgiving

**Vengeful (+80 Vengeful):**
```
Combat Behavior:
  - Fight to death (never yield)
  - Pursue fleeing enemies (chase, execute)
  - Execute captured enemies (no ransom)
  - Remember grudges (attack on sight)

Morale:
  - +20 Will (refuses to give up)
  - Ignores injury penalties (until 0 HP)
  - Never flees (even if losing badly)

Example:
  Assassin killed Vengeful entity's family
  → Hunt assassin for 20 years
  → When found, torture to death (no mercy)
  → Desecrate corpse (spit on grave annually)
```

**Forgiving (+80 Forgiving):**
```
Combat Behavior:
  - Prefer non-lethal (to yield, first blood)
  - Spare defeated enemies (offer mercy)
  - Ransom prisoners (return alive)
  - Forget grudges (relation recovers +5/year)

Morale:
  - -10 Will (reluctant to fight)
  - May flee if injured (preserve life)
  - Accept yield easily (end violence)

Example:
  Enemy killed Forgiving entity's friend
  → Grieve, but do not seek revenge
  → If enemy apologizes: Forgive (relation +20 over time)
  → "Violence begets violence. I will not continue the cycle."
```

### Bold vs Craven

**Bold (+80 Bold):**
```
Combat Behavior:
  - Aggressive stance (press advantage)
  - Never flee (even if outmatched)
  - Challenge stronger enemies (honor)
  - Accept all duels (never back down)

Morale:
  - +20 Will (fearless)
  - Fight to 0 HP (no surrender)
  - Inspire allies (+5 morale to nearby friends)

Example:
  Outnumbered 5 vs 1
  → Bold entity: "Come at me, all of you!"
  → Fights to death (takes 3 enemies with them)
  → Legendary reputation (+30, "died fighting")
```

**Craven (+80 Craven):**
```
Combat Behavior:
  - Defensive stance (survive)
  - Flee if injured (<50% HP)
  - Avoid combat (hide, bribe, surrender)
  - Refuse duels (dishonor acceptable)

Morale:
  - -20 Will (fearful)
  - Flee at first injury
  - Surrender if captured (plead for life)

Example:
  Challenged to duel
  → Craven entity: "I... I yield! Please, I'll pay you!"
  → Offers bribe (500 currency)
  → If refused: Flees village (exile self)
  → Reputation -20 ("coward who fled")
```

---

## DOTS Components

```csharp
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Base attributes component - experience modifiers that calculate derived stats.
    /// </summary>
    public struct BaseAttributes : IComponentData
    {
        public byte Strength;                   // 0-100 (physical power)
        public byte Finesse;                    // 0-100 (skill, agility)
        public byte Will;                       // 0-100 (mental fortitude)
        public byte Intelligence;               // 0-100 (for magic users)
    }

    /// <summary>
    /// Derived combat stats component - calculated from base attributes.
    /// These are the actual stats used in combat.
    /// </summary>
    public struct CombatStats : IComponentData
    {
        // Derived stats (calculated from base attributes)
        public byte Attack;                     // To-hit chance (Finesse × 0.7 + Strength × 0.3)
        public byte Defense;                    // Dodge/block (Finesse × 0.6 + armor)
        public byte Morale;                     // Yield threshold (Will × 0.7 + personality)
        public byte AttackSpeed;                // Attacks per round (Finesse × 0.8)
        public byte AttackDamage;               // Raw damage (Strength × 0.5 + weapon)
        public byte Accuracy;                   // Hit precision (Finesse × 0.9)
        public byte CriticalChance;             // 0-100 (Finesse / 5 + weapon)

        public ushort Health;                   // Max HP (Strength × 0.6 + Will × 0.4 + 50)
        public ushort CurrentHealth;            // Current HP (0 = death)
        public byte Stamina;                    // Rounds before exhaustion (Strength / 10)
        public byte CurrentStamina;             // Current stamina

        // Magic stats (for magic users)
        public byte SpellPower;                 // Magic damage (Will × 0.8 + Int × 0.2)
        public byte ManaPool;                   // Max mana (Will × 0.5 + Int × 0.5)
        public byte CurrentMana;                // Current mana

        // Equipment modifiers (applied to derived stats)
        public Entity EquippedWeapon;           // Weapon entity (or Entity.Null)
        public Entity EquippedArmor;            // Armor entity (or Entity.Null)
        public Entity EquippedShield;           // Shield entity (or Entity.Null)

        // Combat state
        public ushort CombatExperience;         // Total combats survived
        public bool IsInCombat;                 // Currently fighting
        public Entity CurrentOpponent;          // Who they're fighting (or Entity.Null)
    }

    /// <summary>
    /// Weapon component - attached to weapon entities.
    /// </summary>
    public struct Weapon : IComponentData
    {
        public enum WeaponType : byte
        {
            Sword,
            Dagger,
            Mace,
            Axe,
            Spear,
            Bow,
            Staff,
            Unarmed
        }

        public WeaponType Type;
        public byte BaseDamage;                 // 5-20 damage
        public sbyte HitBonus;                  // -10 to +15 hit chance
        public byte ArmorPenetration;           // 0-100 (% of armor ignored)
        public byte CriticalChanceBonus;        // 0-20 (% bonus crit)

        public ushort Durability;               // 0-1000 (breaks at 0)
        public ushort MaxDurability;
        public ushort Value;                    // Currency worth
        public FixedString64Bytes WeaponName;   // "Legendary Blade", "Rusty Dagger"
    }

    /// <summary>
    /// Armor component - attached to armor entities.
    /// </summary>
    public struct Armor : IComponentData
    {
        public enum ArmorType : byte
        {
            None,
            Leather,
            Chainmail,
            Plate,
            Shield
        }

        public ArmorType Type;
        public byte ArmorValue;                 // 5-20 armor
        public sbyte DodgePenalty;              // -15 to +0 dodge
        public sbyte StaminaDrain;              // -2 to +0 rounds

        // Effectiveness vs weapon types (0.0-1.0)
        public float EffectivenessVsSword;      // 0.7 for plate
        public float EffectivenessVsMace;       // 0.4 for plate
        public float EffectivenessVsArrow;      // 0.3 for plate

        public ushort Durability;
        public ushort MaxDurability;
        public ushort Value;
        public FixedString64Bytes ArmorName;
    }

    /// <summary>
    /// Active combat component - tracks ongoing fight.
    /// </summary>
    public struct ActiveCombat : IComponentData
    {
        public enum CombatType : byte
        {
            Duel,
            Brawl,
            Assassination,
            TrialByCombat
        }

        public enum CombatStance : byte
        {
            Aggressive,
            Defensive,
            Balanced,
            Reckless
        }

        public CombatType Type;
        public Entity Combatant1;
        public Entity Combatant2;
        public uint CombatStartTick;
        public byte CurrentRound;

        public CombatStance Combatant1Stance;
        public CombatStance Combatant2Stance;

        public byte Combatant1Damage;           // Damage dealt this round
        public byte Combatant2Damage;

        public bool IsDuelToFirstBlood;         // True if duel ends at first injury
        public bool IsDuelToYield;              // True if duel ends on yield
        public bool IsDuelToDeath;              // True if duel ends on death

        public Entity WitnessEntity;            // Judge/witness (for trials)
        public ushort WitnessCount;             // Spectators (for duels)
    }

    /// <summary>
    /// Combat result component - outcome of fight.
    /// </summary>
    public struct CombatResult : IComponentData
    {
        public enum Outcome : byte
        {
            Combatant1Victory,
            Combatant2Victory,
            Combatant1Fled,
            Combatant2Fled,
            MutualDeath,
            Draw
        }

        public Outcome Result;
        public Entity Victor;
        public Entity Defeated;

        public bool DefeatedKilled;             // True if defeated died
        public bool DefeatedCaptured;           // True if defeated captured
        public bool DefeatedFled;               // True if defeated fled

        public uint CombatEndTick;
        public byte RoundsDuration;
        public byte TotalDamageDealt;
        public byte TotalDamageTaken;

        // Reputation impact
        public sbyte VictorReputationChange;    // +10 (heroic) to -20 (murder)
        public sbyte DefeatedReputationChange;  // -15 (lost duel) to +5 (honorable defeat)
    }

    /// <summary>
    /// Injury component - tracks permanent injuries.
    /// </summary>
    public struct Injury : IBufferElementData
    {
        public enum InjuryType : byte
        {
            LostEye,
            CrippledArm,
            CrippledLeg,
            BrokenRibs,
            FacialScars,
            LostFingers,
            InternalBleeding,
            TraumaticBrainInjury
        }

        public InjuryType Type;
        public uint InjuryTick;                 // When injury occurred
        public sbyte StatPenalty;               // -30 to -5 (stat reduction)
        public FixedString64Bytes Description;  // "Lost left eye in duel with Baron"
    }

    /// <summary>
    /// Death saving throw component - active when HP reaches 0.
    /// </summary>
    public struct DeathSavingThrow : IComponentData
    {
        public byte SurvivalChance;             // Will × 0.5 (%)
        public bool AlliesPresent;              // +10% if true
        public bool MedicalTreatment;           // +20% if true
        public bool ExecutionAttempt;           // -100% if true (auto-death)

        public bool RollSuccessful;             // True if survived
        public bool PermanentInjuryRolled;      // True if injury assigned
    }

    /// <summary>
    /// Combat AI component - determines stance selection.
    /// </summary>
    public struct CombatAI : IComponentData
    {
        public sbyte Aggression;                // -50 (defensive) to +50 (reckless)
        public byte PreferredStance;            // Default stance (alignment-based)

        // Thresholds for AI decisions
        public byte FleeThresholdHP;            // Flee if HP < this (Craven = 50, Bold = 0)
        public byte YieldThresholdHP;           // Yield if HP < this (Forgiving = 40, Vengeful = 0)

        public bool PrefersNonLethal;           // True if Good alignment (spare enemies)
        public bool ExecutesPrisoners;          // True if Evil alignment (no mercy)
    }

    /// <summary>
    /// Combat event log - tracks major combat events.
    /// </summary>
    public struct CombatEvent : IBufferElementData
    {
        public enum EventType : byte
        {
            DuelVictory,
            DuelDefeat,
            Assassination,
            BrawlVictory,
            Fled,
            Captured,
            Killed,
            PermanentInjury
        }

        public EventType Type;
        public uint Tick;
        public Entity Opponent;
        public FixedString128Bytes Description;
    }
}
```

---

## System Flow: Duel Combat

**1. Combat Initiation:**

```csharp
// Entity1 challenges Entity2 to duel
ActiveCombat combat = new ActiveCombat
{
    Type = CombatType.Duel,
    Combatant1 = entity1,
    Combatant2 = entity2,
    CombatStartTick = currentTick,
    CurrentRound = 1,
    IsDuelToYield = true,  // Lawful Good duel
    WitnessCount = 50      // 50 spectators
};
```

**2. Initiative Roll (Determine First Strike):**

```csharp
public Entity RollInitiative(Entity e1, Entity e2, Combatant c1, Combatant c2)
{
    int initiative1 = c1.Finesse + Random.Range(0, 20);
    int initiative2 = c2.Finesse + Random.Range(0, 20);

    return (initiative1 >= initiative2) ? e1 : e2;
}
```

**3. Combat Round:**

```csharp
public void ProcessCombatRound(ref ActiveCombat combat, ref Combatant c1, ref Combatant c2)
{
    // 1. Select stances (AI or player)
    combat.Combatant1Stance = DetermineStance(c1);
    combat.Combatant2Stance = DetermineStance(c2);

    // 2. Combatant1 attacks
    if (AttackCheck(c1, c2, combat.Combatant1Stance, combat.Combatant2Stance))
    {
        byte damage = CalculateDamage(c1, c2, combat.Combatant1Stance);
        c2.CurrentHP = (byte)math.max(0, c2.CurrentHP - damage);
        combat.Combatant1Damage = damage;
    }

    // 3. Check if Combatant2 died or yields
    if (c2.CurrentHP == 0)
    {
        // Death saving throw
        if (!DeathSavingThrow(c2))
        {
            EndCombat(combat, CombatResult.Outcome.Combatant1Victory, killed: true);
            return;
        }
    }
    else if (c2.CurrentHP < c2.MaxHP * 0.3f && combat.IsDuelToYield)
    {
        // Morale check (Will check to continue)
        if (Random.Range(0, 100) > c2.Will)
        {
            // Yield
            EndCombat(combat, CombatResult.Outcome.Combatant1Victory, killed: false);
            return;
        }
    }

    // 4. Combatant2 counterattacks (if alive)
    if (AttackCheck(c2, c1, combat.Combatant2Stance, combat.Combatant1Stance))
    {
        byte damage = CalculateDamage(c2, c1, combat.Combatant2Stance);
        c1.CurrentHP = (byte)math.max(0, c1.CurrentHP - damage);
        combat.Combatant2Damage = damage;
    }

    // 5. Check Combatant1 status
    if (c1.CurrentHP == 0)
    {
        if (!DeathSavingThrow(c1))
        {
            EndCombat(combat, CombatResult.Outcome.Combatant2Victory, killed: true);
            return;
        }
    }

    // 6. Increment round
    combat.CurrentRound++;
}
```

**4. Damage Calculation:**

```csharp
public byte CalculateDamage(Combatant attacker, Combatant defender, CombatStance stance)
{
    // Get weapon
    Weapon weapon = GetWeapon(attacker.EquippedWeapon);
    Armor armor = GetArmor(defender.EquippedArmor);

    // Base damage
    int damage = (attacker.Strength / 5) + weapon.BaseDamage;

    // Stance modifiers
    if (stance == CombatStance.Aggressive)
        damage += 5;
    else if (stance == CombatStance.Reckless)
        damage += 10;

    // Critical hit (Finesse check)
    if (Random.Range(0, 100) < (attacker.Finesse / 5 + weapon.CriticalChanceBonus))
    {
        damage *= 2; // Critical!
    }

    // Armor reduction
    float armorEffectiveness = GetArmorEffectiveness(armor, weapon.Type);
    int armorReduction = (int)(armor.ArmorValue * armorEffectiveness);
    damage = math.max(1, damage - armorReduction);

    return (byte)damage;
}
```

**5. End Combat:**

```csharp
public void EndCombat(ActiveCombat combat, CombatResult.Outcome outcome, bool killed)
{
    CombatResult result = new CombatResult
    {
        Result = outcome,
        Victor = (outcome == CombatResult.Outcome.Combatant1Victory) ? combat.Combatant1 : combat.Combatant2,
        Defeated = (outcome == CombatResult.Outcome.Combatant1Victory) ? combat.Combatant2 : combat.Combatant1,
        DefeatedKilled = killed,
        CombatEndTick = currentTick,
        RoundsDuration = combat.CurrentRound
    };

    // Update reputations
    if (combat.Type == CombatType.Duel && !killed)
    {
        result.VictorReputationChange = +15; // Honorable duel
        result.DefeatedReputationChange = -10; // Lost but alive
    }
    else if (killed)
    {
        result.VictorReputationChange = +10; // Killed enemy
        result.DefeatedReputationChange = -50; // Died
    }

    // Update relations
    UpdateRelationAfterCombat(result);

    // Log event
    LogCombatEvent(result);
}
```

---

## Iteration Plan

**v1.0 (MVP - Basic Combat):**
- ✅ Combat stats (Finesse, Strength, Will, HP)
- ✅ Simple combat (attack, damage, death)
- ✅ Weapons (swords, daggers, basic damage)
- ✅ Armor (plate, leather, basic reduction)
- ✅ Death (HP 0 = death, no saving throws)
- ❌ No stances
- ❌ No injuries (permanent)
- ❌ No combat types (all treated as brawls)

**v2.0 (Enhanced - Combat Depth):**
- ✅ Combat stances (Aggressive, Defensive, Balanced, Reckless)
- ✅ Combat types (Duel, Brawl, Assassination, Trial)
- ✅ Death saving throws (Will × 0.5 survival)
- ✅ Morale checks (flee when injured)
- ✅ Permanent injuries (crippled arm, lost eye)
- ✅ Alignment-based combat AI (Good = non-lethal, Evil = execute)
- ✅ Behavioral modifiers (Vengeful = never flee, Craven = flee early)

**v3.0 (Complete - Advanced Combat):**
- ✅ Weapon specialization (Finesse bonus for trained weapons)
- ✅ Combo attacks (Finesse check for multi-hit)
- ✅ Disarm mechanics (Strength check to disarm opponent)
- ✅ Grappling (Strength check to immobilize)
- ✅ Environmental factors (high ground, darkness, terrain)
- ✅ Champion trials (represent lords in legal combat)
- ✅ Legendary weapons (unique bonuses, quest artifacts)
- ✅ Combat training (gain Finesse from sparring)

---

## Open Questions

### Critical

1. **Critical hit frequency?** Is 18% crit at Finesse 90 balanced or too high?
2. **Death rate in duels?** Should 30% of duels end in death or 10%?
3. **Permanent injury rate?** Is 40% chance at critical injury too punishing?
4. **Armor dominance?** Does plate armor make combatants invincible vs leather?

### Design

5. **Morale check timing?** Check every round when injured or only at thresholds (50%, 30%)?
6. **Stance AI complexity?** Should AI predict opponent's stance or random?
7. **Assassination detection?** Should targets have Perception passive check even while asleep?
8. **Trial by combat frequency?** Should Lawful villages allow this for all crimes or only major ones?

### Balancing

9. **Finesse vs Strength?** Is Finesse too powerful (hit + dodge + crit)?
10. **Reckless stance viability?** Is +10 damage worth -30% dodge and self-injury risk?
11. **Will importance?** Does Will matter enough or should it affect damage too?
12. **Champion cost?** Should professional champions cost 3000 currency or 10,000?

### Technical

13. **Combat duration?** Average 5 rounds acceptable or should be 3 (faster)?
14. **HP scaling?** Should legendary fighters have 150 HP or cap at 100?
15. **Injury healing?** Should permanent injuries reduce penalty over time (years)?
16. **Weapon durability?** Should weapons break during combat or only after many fights?

---

## Exploits

**Problem:** Player spams Reckless stance for high damage, ignores self-injury risk
- **Fix:** Reckless self-injury triggers permanent injury (10% chance/round)
- **Cap:** Max 3 Reckless rounds before forced Defensive stance (exhaustion)

**Problem:** Plate armor makes Strength 90+ entities invincible
- **Fix:** Maces/hammers ignore 60% plate armor (counter exists)
- **Fatigue:** Plate armor drains stamina, -2 rounds (max 5 rounds combat before exhaustion)

**Problem:** Assassination trivializes all enemies (instant kill)
- **Fix:** High-value targets have bodyguards (Elite families, nobles)
- **Detection:** Perception passive check even when asleep (Perception / 5 = % wake up)

**Problem:** Death saving throws prevent all deaths (Will 80 = 40% survive)
- **Fix:** Execution (deliberate killing blow) = -100% survival (guaranteed death)
- **Bleeding Out:** If survive at 1 HP, must receive medical aid within 1 hour or die

**Problem:** Craven entities never die (flee at 50% HP)
- **Fix:** Vengeful opponents chase (Finesse check, if caught → execution)
- **Reputation:** Flee 3+ times = -30 reputation (no one hires cowards)

---

## Player Interaction

### Observable

**Combat Visible:**
- Player sees combat rounds (attacks, damage, HP bars)
- Stances displayed (Aggressive, Defensive icons)
- Injuries announced ("Sir Aldric crippled Baron's arm!")
- Deaths create funeral events (villagers mourn)

**Combat Reports:**
- Duel results ("Champion Gareth defeated Bandit Leader in 4 rounds")
- Assassination notices ("Baron Erik found dead in sleep, no suspects")
- Trial verdicts ("Marcus executed by champion, proven guilty by combat")

**UI Indicators:**
- Combatant HP bars (real-time)
- Stance icons (aggressive = sword, defensive = shield)
- Injury status (eye patch, cane for crippled leg)

### Player Agency

**Initiate Combat:**
- Player can command champions to duel enemies
- Hire assassins for political murders
- Order trials by combat for accused villagers

**Equip Combatants:**
- Assign weapons/armor to champions, heroes, guards
- Upgrade equipment (buy legendary swords)
- Enchant weapons (if magic system exists)

**Intervene in Combat:**
- Player deity can:
  * Smite combatant with lightning (instant kill, -50 god relation)
  * Heal combatant mid-duel (+20 HP, other combatant sees divine favor)
  * Blind combatant (reduce hit chance to 10%, -30 god relation)

**Manage Consequences:**
- Pay ransom for captured champions
- Heal injured heroes (medical treatment costs)
- Revenge assassinations (family hires spy to kill murderer)

### Difficulty Scaling

**Novice:**
- Death saving throws: Will × 0.8 (80% of Will)
- Permanent injuries: 20% chance (reduced)
- Morale checks: +20 bonus (fight longer)

**Intermediate:**
- Death saving throws: Will × 0.5 (standard)
- Permanent injuries: 40% chance
- Morale checks: Standard

**Expert:**
- Death saving throws: Will × 0.3 (30% of Will, brutal)
- Permanent injuries: 60% chance (common)
- Morale checks: -20 penalty (flee/yield more often)

---

## Systemic Interactions

### Dependencies

**Individual Progression:** ✅ Finesse, Strength, Will determine combat effectiveness
**Alignment Framework:** ✅ Combat methods vary by alignment (Good = non-lethal, Evil = execute)
**Behavioral Personality:** ✅ Vengeful = never yield, Craven = flee early
**Entity Relations:** ✅ Combat affects relations (-60 killed, +20 respect if honorable)
**Spies & Double Agents:** ✅ Assassination missions use stealth combat
**Elite Courts:** ✅ Champions defend families in trials by combat

### Influences

**Death:** Triggers inheritance, succession, family grief
**Reputation:** Combat victories boost reputation (+10), cowardice reduces (-10)
**Wealth:** Winners loot losers (weapons, armor, currency)
**Politics:** Trial by combat determines legal verdicts
**Relations:** Killing creates vendettas (-80 family relation)
**Business:** Death of business owner triggers ownership transfer

### Synergies

**Combat + Relations:** High relation enemies may yield early (spare friend)
**Combat + Wealth:** Wealthy entities hire champions (avoid personal risk)
**Combat + Spies:** Assassinations eliminate enemies without war
**Combat + Alignment:** Lawful Good villages demand trial by combat (legal justice)
**Combat + Courts:** Champions represent lords in duels (proxy combat)
**Combat + Injuries:** Permanent injuries reduce combat effectiveness (balanced)

---

## Related Documentation

- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Finesse, Strength, Will stats
- **Alignment Framework:** [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Combat methods by alignment
- **Behavioral Personality:** [Villager_Behavioral_Personality.md](../Villagers/Villager_Behavioral_Personality.md) - Vengeful/forgiving, bold/craven
- **Entity Relations:** [Entity_Relations_And_Interactions.md](../Villagers/Entity_Relations_And_Interactions.md) - Combat relation penalties
- **Spies & Double Agents:** [Spies_And_Double_Agents.md](../Villagers/Spies_And_Double_Agents.md) - Assassination mechanics
- **Elite Courts:** [Elite_Courts_And_Retinues.md](../Villagers/Elite_Courts_And_Retinues.md) - Champions, trial by combat
- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](../Villagers/Wealth_And_Social_Dynamics.md) - Inheritance after death

---

**For Implementers:** Start with basic combat (attack, damage, death). Stances and injuries can be v2.0. Death saving throws add dramatic tension (survive at 1 HP vs instant death).

**For Narrative:** Combat creates emergent stories: honorable duels (mutual respect), brutal assassinations (generational vendettas), trial by combat (justice through violence), permanent injuries (one-eyed veteran seeking redemption).

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core mechanics defined, ready for prototyping
