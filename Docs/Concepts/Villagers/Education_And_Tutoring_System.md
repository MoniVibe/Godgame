# Education & Tutoring System

## Overview

The **Education & Tutoring System** governs how entities learn skills, knowledge, and outlooks through formal education, independent practice, and observation. Lessons must be absorbed and refined by individuals, with wisdom and intelligence determining learning speed. Knowledge accumulates culturally, familially, and dynastically, shaping village outlooks over generations.

**Key Features:**
- **Lessons System:** Quality, Rarity, Tier, Affixes determine effectiveness and stacking
- **Lesson Absorption:** Entities refine lessons over time (Intelligence and Wisdom accelerate)
- **Wisdom Stat:** Derived from Perception, Will, Intelligence - accumulates culturally/dynastically
- **Independent Learning:** Learn through activities (taming animals, botany, observing combat/magic)
- **Observation-Based Learning:** Form self-created lessons by watching others (combat, spellcasting, crafts)
- **Schools:** Teach children with quality/rarity lessons, shift outlooks slowly (NOT alignments)
- **Mentors & Tutors:** High-skill individuals teach advanced techniques (weapon deflection up to +30%)
- **Cultural/Dynastic Wisdom:** Villages are sum of accumulated wisdom, outlooks, cultures

**For Narrative:** Kung Fu self-taught masters (observation learning), Renaissance polymaths (accumulated wisdom), apprentice surpasses master (self-refined superior lesson), village wisdom passed down generations.

---

## Core Stats

### Education (0-100)

**Education:**
```
Represents: Cumulative formal learning, literacy, knowledge
Increases: Base Intelligence, skill acquisition speed, critical thinking
Sources:
  - Schools (slow, children only)
  - Tutors (fast, expensive)
  - Mentors (specialized skills, high-skill teachers)
  - Universities & Academies (advanced, elite access)
  - Guild training (craft-specific)

Education Tiers:
  0-20:   Illiterate (no formal education, common laborers)
  21-40:  Basic Literacy (village school, read/write)
  41-60:  Educated (tutored, apprenticed, literate)
  61-80:  Scholarly (university, advanced knowledge)
  81-100: Master Scholar (rare, academic elite, polymaths)
```

**Education Impact:**
```
Intelligence Bonus:
  Education 0-20:    +0 Intelligence
  Education 21-40:   +5 Intelligence
  Education 41-60:   +10 Intelligence
  Education 61-80:   +15 Intelligence
  Education 81-100:  +25 Intelligence

Skill Acquisition Speed:
  Education 0-20:    1.0× (normal speed)
  Education 21-40:   1.2× (20% faster learning)
  Education 41-60:   1.5× (50% faster)
  Education 61-80:   2.0× (double speed)
  Education 81-100:  3.0× (triple speed, prodigies)
```

### Wisdom (0-100)

**Wisdom (NEW STAT):**
```
Represents: Life experience, cultural knowledge, practical insight, accumulated understanding
Derived From: (Perception × 0.4) + (Will × 0.3) + (Intelligence × 0.3)

Sources:
  - Age (older entities have higher Wisdom)
  - Life experience (surviving hardship, witnessing events)
  - Cultural transmission (village wisdom, family wisdom)
  - Observation (watching others, learning from mistakes)

Wisdom Tiers:
  0-20:   Naive (young, inexperienced)
  21-40:  Experienced (lived through hardship)
  41-60:  Wise (respected elder, teacher)
  61-80:  Sage (village elder, cultural keeper)
  81-100: Legendary Sage (once-in-a-generation wisdom)

Example:
  Entity: Village Elder
  Perception 70, Will 80, Intelligence 60
  → Wisdom = (70 × 0.4) + (80 × 0.3) + (60 × 0.3)
  → Wisdom = 28 + 24 + 18 = 70 (Sage)
```

**Wisdom Impact:**
```
Independent Learning Speed:
  Wisdom 0-20:    1.0× (slow self-teaching)
  Wisdom 21-40:   1.5× (moderate self-teaching)
  Wisdom 41-60:   2.0× (fast self-teaching)
  Wisdom 61-80:   3.0× (rapid insight)
  Wisdom 81-100:  5.0× (master self-teacher, genius insights)

Lesson Absorption Rate:
  Wisdom 0-20:    1.0× (slow refinement)
  Wisdom 21-40:   1.3× (moderate refinement)
  Wisdom 41-60:   1.7× (fast refinement)
  Wisdom 61-80:   2.5× (rapid mastery)
  Wisdom 81-100:  4.0× (instant mastery, prodigies)

Observation Learning Chance:
  Wisdom 0-20:    5% chance per observation
  Wisdom 21-40:   10% chance
  Wisdom 41-60:   20% chance
  Wisdom 61-80:   35% chance
  Wisdom 81-100:  50% chance (high insight)
```

---

## Lesson System

### Lesson Attributes

Every lesson has **four** attributes:

**Quality (1-100):**
```
Determines: Effectiveness, how much the lesson improves the learner
Examples:
  1-20:   Poor (village school basics, outdated knowledge, self-taught fumbling)
  21-40:  Average (competent tutor, standard lessons, decent self-teaching)
  41-60:  Good (skilled tutor, proven curriculum, talented self-learner)
  61-80:  Excellent (master tutor, refined techniques, genius insight)
  81-100: Legendary (grandmaster, once-in-a-generation teaching, prodigy self-mastery)
```

**Rarity (Common, Uncommon, Rare, Epic, Legendary):**
```
Determines: How unique the knowledge is, stacking rules
Common:      Basic knowledge (reading, arithmetic, common combat)
Uncommon:    Specialized knowledge (weapon deflection, magic theory)
Rare:        Advanced techniques (dual-wielding, alchemy, diplomacy)
Epic:        Secret knowledge (ancient combat forms, forbidden magic)
Legendary:   Lost knowledge (dragon-slaying, godly intervention techniques)

Stacking Rule:
  Lessons of the SAME RARITY can stack
  → Allows learning the same concept from multiple perspectives
```

**Tier (1-10):**
```
Determines: Complexity level, overwriting rules
Tier 1-3:    Novice (basic concepts, children)
Tier 4-6:    Intermediate (advanced techniques, adults)
Tier 7-9:    Expert (mastery, specialists)
Tier 10:     Grandmaster (legendary, peak knowledge)

Overwriting Rule:
  Lessons of the SAME TIER and SAME RARITY overwrite each other if Quality is higher
  → Prevents redundant learning of identical lessons
```

**Affixes (Optional):**
```
Determines: Special bonuses or unique properties of the lesson
Examples:
  - "Swift" Deflection: +10% Attack Speed in addition to Defense
  - "Riposte" Deflection: +5% Critical Chance when blocking
  - "Brutal" Combat: +15% Damage against armored foes
  - "Efficient" Magic: -10% Mana cost on spells
  - "Botanical Mastery": +20% rare plant harvest chance

Affixes are learned through:
  - Legendary teachers (may grant affixes to their lessons)
  - Self-discovery (entities create affixes through unique insights)
  - Observation (copying a master's unique technique)
```

### Lesson Absorption & Refinement

**Lessons must be absorbed and refined** by individual entities over time. This is NOT instant learning.

**Absorption Process:**
```
When entity receives a lesson (from teacher, observation, self-learning):
  1. Lesson enters "Unrefined" state (50% effectiveness)
  2. Entity must practice and refine over time
  3. Absorption speed scales with Intelligence and Wisdom

Absorption Time:
  BaseTime = (LessonTier × 30 days) / (Intelligence + Wisdom)

  Example:
    Tier 5 Deflection Lesson
    Entity: Intelligence 70, Wisdom 40
    → (5 × 30) / (70 + 40) = 150 / 110 = 1.36 months to fully refine

Refinement Stages:
  Day 0-30%:      Unrefined (50% effectiveness, clumsy execution)
  Day 31-70%:     Refining (75% effectiveness, competent)
  Day 71-100%:    Refined (100% effectiveness, mastered)

High Intelligence/Wisdom Advantage:
  Intelligence 90+, Wisdom 80+:
    → Refine lessons in days instead of months (4-10× faster)
```

**Self-Refinement:**
```
Entities can IMPROVE lessons beyond their teacher's quality through practice:

RefinementCheck (occurs every 30 days of practice):
  Roll d100 + Intelligence + Wisdom

  If roll > 150:
    → Lesson Quality +1 (entity improves technique)
    → May discover Affixes (5% chance if roll > 180)

Example:
  Student learns Tier 5 Deflection (Quality 60) from master
  → Refines over 6 months
  → Makes 2 successful refinement checks
  → Lesson Quality now 62 (student improved the technique)
  → Rolls 185 on final check
  → Discovers "Riposte" affix (+5% Crit when blocking)

Result: Student has Deflection (Quality 62, "Riposte") - BETTER than master's original lesson
```

---

## Independent Learning (Learning Through Activities)

Entities can **learn lessons independently** through repeated activities. Chance scales with Wisdom and Intelligence.

### Activity-Based Learning

**Animal Handling:**
```
Activity: Taming, training, caring for animals
Lessons Learned:
  - Animal Taming (Common, Tier 2-4)
  - Animal Training (Uncommon, Tier 3-5)
  - Advanced Breeding (Rare, Tier 5-7)

Learning Chance per 30 days:
  BaseChance = (Wisdom + Intelligence) / 10

  Example:
    Entity: Wisdom 50, Intelligence 40
    → (50 + 40) / 10 = 9% chance per month of learning Animal Taming

  After learning:
    → Self-created lesson has Quality = (Wisdom + Intelligence) / 2
    → Quality 45 lesson (average self-teaching)
```

**Botany & Farming:**
```
Activity: Growing plants, harvesting crops
Lessons Learned:
  - Basic Farming (Common, Tier 1-2)
  - Rare Plant Cultivation (Uncommon, Tier 4-6)
  - Harvest Efficiency (Rare, Tier 5-7)

Learning Chance per 30 days:
  BaseChance = (Wisdom + Intelligence) / 8

  Example:
    Botanist: Wisdom 70, Intelligence 80
    → (70 + 80) / 8 = 18.75% chance per month

  Rare Plant Cultivation (Tier 6, Quality 75):
    → +20% harvest yield for rare plants
    → +15% chance to discover new plant species
```

**Combat Observation:**
```
Activity: Watching others fight (duels, battles, sparring)
Lessons Learned:
  - Combat Technique (Common, Tier 2-5)
  - Weapon Mastery (Uncommon, Tier 4-7)
  - Advanced Tactics (Rare, Tier 6-9)

Learning Chance per combat observed:
  BaseChance = (Wisdom / 5) + (Perception / 10)

  Example:
    Observer: Wisdom 60, Perception 70
    → (60 / 5) + (70 / 10) = 12 + 7 = 19% chance per combat observed

  Self-created Combat Technique:
    Quality = (Observer's Wisdom + Perception) / 2
    → Quality 65 (good self-teaching)

  May gain Affixes:
    If observed fighter had unique style (Bold, Vengeful, etc.):
      → 10% chance to copy their affix
      → "Aggressive" Combat (+10% damage, -5% defense)
```

**Magic Observation:**
```
Activity: Watching mages cast spells
Lessons Learned:
  - Spell Mimicry (ability to cast observed spells)
  - Magic Theory (Uncommon, Tier 3-6)
  - Spell Efficiency (Rare, Tier 5-8)

Learning Chance per spell observed:
  BaseChance = (Intelligence / 5) + (Wisdom / 10)

  Requirement: Must have ManaPool > 0 (magic user)

  Example:
    Observer: Intelligence 80, Wisdom 50, ManaPool 60
    → (80 / 5) + (50 / 10) = 16 + 5 = 21% chance per spell observed

  Learned Spell:
    Quality = (Intelligence + Wisdom) / 2 = 65
    → Can now cast the spell at 65% effectiveness
    → May refine over time to 100%

  Risk:
    If Intelligence < 60: 10% chance spell backfires (self-damage)
    If Wisdom < 40: 20% chance miscast (waste mana, no effect)
```

**Crafting & Smithing:**
```
Activity: Crafting weapons, armor, tools
Lessons Learned:
  - Basic Smithing (Common, Tier 1-3)
  - Weapon Forging (Uncommon, Tier 3-6)
  - Master Crafting (Rare, Tier 6-9)

Learning Chance per 30 days:
  BaseChance = (Wisdom + Intelligence + Finesse) / 15

  Example:
    Blacksmith: Wisdom 60, Intelligence 50, Finesse 70
    → (60 + 50 + 70) / 15 = 12% chance per month

  Self-created Weapon Forging (Tier 5, Quality 60):
    → +10% weapon durability
    → +5% weapon damage
    → May discover unique weapon affixes (Critical, Armor Penetration, etc.)
```

---

## Observation-Based Learning

Entities form **self-created lessons** by observing skilled individuals. Quality, rarity, and affixes depend on observer's stats and the observed entity's skill.

### Observation Mechanics

**Observing Combat:**
```
When entity observes combat (duel, battle, sparring):

ObservationCheck:
  Roll d100 + Wisdom + Perception

  If roll > 100:
    → Gain insight into combat (learn lesson)

  Lesson Quality:
    Quality = (Observer's Wisdom + Perception) / 2
    Capped by observed fighter's skill

  Example:
    Observer: Wisdom 70, Perception 80
    Observed Fighter: Finesse 90, CombatExperience 500

    Roll: 75 + 70 + 80 = 225 (SUCCESS)

    Lesson Created:
      "Combat Insight" (Uncommon, Tier 4, Quality 75)
      Effect: +10 Attack, +5% Critical Chance
      Affix: "Aggressive" (copied from Bold fighter)

  Multiple Observations:
    Observing same fighter multiple times:
      → 20% chance to refine lesson (+1 Quality per success)
      → 5% chance to discover new affix
```

**Observing Magic:**
```
When entity observes spellcasting:

SpellMimicryCheck:
  Roll d100 + Intelligence + Wisdom

  Requirement: ManaPool > 0

  If roll > 120:
    → Learn to cast observed spell

  Spell Quality:
    Quality = (Observer's Intelligence + Wisdom) / 2
    Capped by caster's SpellPower

  Example:
    Observer: Intelligence 85, Wisdom 60, ManaPool 70
    Observed Mage: SpellPower 90, casting Fireball

    Roll: 110 + 85 + 60 = 255 (SUCCESS)

    Spell Learned:
      "Fireball" (Rare, Tier 6, Quality 72.5)
      Damage: 50 (at Quality 72.5 = 36 damage, 72.5% effectiveness)
      Mana Cost: 30

    Refinement:
      Entity practices spell over time
      → Refines to Quality 100 (full 50 damage)
      → May discover "Efficient" affix (-10% mana cost)
```

**Observing Crafts:**
```
When entity observes crafting (smithing, alchemy, cooking):

CraftInsightCheck:
  Roll d100 + Intelligence + Wisdom + (relevant skill)

  Example: Observing weapon forging
    → Roll d100 + Intelligence + Wisdom + Finesse

  If roll > 130:
    → Learn crafting technique

  Lesson Quality:
    Quality = (Observer's stats) / 2
    Capped by craftsman's skill

  Example:
    Observer: Intelligence 70, Wisdom 80, Finesse 60
    Observed Smith: Finesse 90, crafting legendary sword

    Roll: 95 + 70 + 80 + 60 = 305 (SUCCESS)

    Lesson Learned:
      "Advanced Forging" (Rare, Tier 7, Quality 70)
      Effect: +15% weapon quality when crafting
      Affix: "Durability" (+20% weapon durability)
```

---

## Schools & Teacher Influence

### Teacher Lessons to Children

**Teachers' lessons have Quality and Rarity** (just like mentor lessons).

**Village School Teacher:**
```
Quality: 20-40 (basic, underfunded)
Rarity: Common (literacy, arithmetic)
Tier: 1-2 (novice concepts)

Example Lesson:
  "Basic Literacy" (Common, Tier 1, Quality 30)
  Effect: +10 Education, ability to read/write
  Duration: 5-10 years to fully refine

Children absorb slowly:
  Intelligence 40, Wisdom 10 (young child)
  → Absorption time: (1 × 30) / (40 + 10) = 0.6 months (fast for basic lesson)
```

**Private School Tutor:**
```
Quality: 50-70 (skilled, well-funded)
Rarity: Common to Uncommon
Tier: 2-4 (intermediate concepts)

Example Lesson:
  "Advanced Mathematics" (Uncommon, Tier 3, Quality 60)
  Effect: +15 Education, +10 Intelligence
  Duration: 2-3 years to fully refine

Elite child absorbs faster:
  Intelligence 60, Wisdom 30
  → Absorption time: (3 × 30) / (60 + 30) = 1 month
```

**University Professor:**
```
Quality: 70-90 (world-class)
Rarity: Rare to Epic
Tier: 6-8 (expert concepts)

Example Lesson:
  "Advanced Magic Theory" (Rare, Tier 7, Quality 85)
  Effect: +20 SpellPower, +15 ManaPool, unlock Tier 7 spells
  Duration: 1-2 years to fully refine

Talented student absorbs:
  Intelligence 80, Wisdom 50
  → Absorption time: (7 × 30) / (80 + 50) = 1.6 months
```

### Outlook Shift (NOT Alignment)

**Teachers shift students' outlooks slowly** (NOT personality traits, NOT alignments).

**Outlook Shift Mechanics:**
```
Students shift toward teacher's OUTLOOKS (not Bold/Craven traits, not alignment)

Shift Rate: 1-5 points per year toward teacher's outlook values

Example:
  Teacher Outlooks: Bold +80, Vengeful +70, Warlike +60
  Student Outlooks: Bold +30, Vengeful +20, Warlike +10

  After 5 years:
    Student Bold: +30 → +50 (+4 per year, trending toward teacher)
    Student Vengeful: +20 → +35 (+3 per year)
    Student Warlike: +10 → +25 (+3 per year)

  NOT ABSORBED:
    - Teacher's alignment (Lawful Good) does NOT transfer
    - Student remains True Neutral (alignment unchanged)
    - Student does NOT become "Bold" trait (just shifts outlook value)
```

**Outlook Shift Modifiers:**
```
Shift Speed Modifiers:
  - Teacher Charisma 70+: +50% shift rate
  - Student Trusting (+60+): +30% shift rate
  - Student Paranoid (+60+): -20% shift rate
  - Intense indoctrination (private school, guild): +20% shift rate
  - Teacher Quality (lesson quality): Higher quality = faster shift

Example:
  Teacher: Charisma 80, Lesson Quality 70
  Student: Trusting +65

  Shift Rate: Base 3 points/year
    → +50% (Charisma) = 4.5
    → +30% (Trusting) = 5.85
    → +20% (Quality) = 7 points/year (rapid outlook shift)
```

---

## Wisdom Accumulation

### Cultural Wisdom

**Villages accumulate wisdom** as the sum of their inhabitants' wisdom.

**Village Wisdom:**
```
VillageWisdom = (Sum of all villager Wisdom) / VillagePopulation

Example:
  Village A: 100 villagers
    - 10 elders (Wisdom 70-80)
    - 30 adults (Wisdom 40-60)
    - 60 young (Wisdom 10-30)

  VillageWisdom = (10×75 + 30×50 + 60×20) / 100
                = (750 + 1500 + 1200) / 100
                = 34.5 (Experienced village)

High Wisdom Villages:
  → Faster independent learning for all villagers (+20% base chance)
  → Better lesson quality when self-teaching (+10 Quality)
  → Cultural knowledge repository (elders teach wisdom to youth)

Low Wisdom Villages:
  → Slower learning (-10% base chance)
  → Poor self-teaching (−5 Quality)
  → Lost knowledge (no elders to pass wisdom)
```

### Familial Wisdom

**Families pass wisdom** through generations.

**Family Wisdom Transmission:**
```
Children born into family:
  Starting Wisdom = (Parent1 Wisdom + Parent2 Wisdom) / 4

Example:
  Mother: Wisdom 70
  Father: Wisdom 60
  Child: Starting Wisdom = (70 + 60) / 4 = 32.5 (Experienced, even as child)

High Wisdom Families:
  → Children learn faster (born with cultural knowledge)
  → +20% lesson absorption rate
  → Inherit family lessons (cooking, crafting, combat techniques)

Low Wisdom Families:
  → Children start naive (Wisdom 5-10)
  → Must learn everything from scratch
```

### Dynastic Wisdom

**Dynasties accumulate wisdom** over multiple generations.

**Dynasty Wisdom:**
```
DynastyWisdom = Average of all family members' Wisdom across generations

Example:
  Dynasty of Scholars:
    Generation 1: Average Wisdom 50
    Generation 2: Average Wisdom 60 (learned from elders)
    Generation 3: Average Wisdom 70 (cumulative knowledge)

  DynastyWisdom = (50 + 60 + 70) / 3 = 60 (Wise dynasty)

High Wisdom Dynasties:
  → +30% lesson absorption for all members
  → Inherit Epic/Legendary lessons from ancestors
  → +20 starting Wisdom for newborns
  → Cultural libraries (stored knowledge)

Dynasty Collapse:
  If dynasty loses all high-wisdom members (war, plague, assassination):
    → Wisdom drops to village average
    → Lost knowledge (Epic/Legendary lessons forgotten)
    → Future generations start from scratch
```

---

## School Ownership & Economics

### Village Schools (Public)

**Public Education:**
```
Ownership: Village government
Access: All children age 6-12, free
Teachers: 1-3 village-employed teachers (Intelligence 40-60)
Lesson Quality: 20-40 (basic, underfunded)
Lesson Rarity: Common

Curriculum:
  - Literacy & Arithmetic (Tier 1-2)
  - Basic History & Geography
  - Village culture & outlooks

Effect:
  - +10 Education over 5-10 years
  - +5 Intelligence
  - Outlook shift toward village average (+3 points/year)
  - Wisdom +5 (basic cultural transmission)
```

### Private Schools (Elite-Owned)

**Private Education:**
```
Ownership: Elite families (High-Ultra High wealth)
Access: Elite children, tuition 100-500 currency/year
Teachers: 2-5 hired tutors (Intelligence 60-80)
Lesson Quality: 50-70 (well-funded, skilled teachers)
Lesson Rarity: Common to Uncommon

Curriculum:
  - Advanced Literacy (Tier 2-3)
  - Mathematics & Science (Tier 3-4)
  - History, Politics, Economics
  - Elite culture

Effect:
  - +30 Education over 5-10 years
  - +15 Intelligence
  - Outlook shift toward elite outlooks (+5 points/year)
  - Wisdom +15 (elite cultural transmission)
  - Networking with other elite children (future allies)
```

### Guild Schools (Specialized)

**Guild Education:**
```
Ownership: Guilds (Thieves, Mages, Blacksmiths, Merchants)
Access: Guild members' children, apprentices
Teachers: 1-3 guild masters (relevant skill 70-90)
Lesson Quality: 60-80 (highly specialized, excellent in niche)
Lesson Rarity: Uncommon to Rare

Curriculum:
  - Guild-specific skills (lockpicking, spellcasting, smithing)
  - Trade secrets (guild techniques, proprietary knowledge)
  - Guild culture & loyalty

Effect:
  - +40 Education in guild specialty
  - +20 to guild-specific skill (Finesse for thieves, Intelligence for mages)
  - Outlook shift toward guild outlooks (+7 points/year)
  - Wisdom +20 (guild cultural transmission)
  - Loyalty to guild (+30 relation with guild)
  - May learn Epic lessons (guild trade secrets)
```

### Universities & Academies

**University Education:**
```
Ownership: Elite families, villages (rare), autonomous institutions
Access: Wealthy students (tuition 500-2000 currency/year), scholarship (rare)
Teachers: 5-15 professors (Intelligence 80-95, specialists)
Lesson Quality: 70-90 (world-class, cutting-edge knowledge)
Lesson Rarity: Rare to Epic

Curriculum:
  - Advanced Magic Theory (Tier 6-8)
  - Philosophy, Law, Medicine
  - Research & Innovation (unlock new techniques)
  - Elite networking (future rulers, mages, diplomats)

Effect:
  - +60 Education over 3-5 years
  - +25 Intelligence
  - Unlock advanced professions (court mage, diplomat, physician)
  - Wisdom +30 (academic cultural transmission)
  - May learn Legendary lessons (research breakthroughs)
```

---

## Weapon Deflection Mechanic

### Deflection Training

**High finesse mentors** teach weapon deflection, allowing entities to **block attacks with their weapon** (not shield).

**Deflection Bonus (Up to +30% Defense):**
```
Requirement: Learn from mentor with Finesse 80+
Lesson Rarity: Uncommon to Rare (depending on tier)
Lesson Tiers:
  Tier 3 (Novice):      +5% Deflection  (basic parrying, Finesse 50+)
  Tier 5 (Intermediate): +15% Deflection (angle deflection, Finesse 65+)
  Tier 7 (Expert):       +30% Deflection (master riposte, Finesse 80+)

Cost: 500-2000 currency per tier
Duration: 30-90 days training per tier (then absorption/refinement)

Deflection Effect:
  Adds directly to Defense stat (dodge/block)
  → Defense = (Finesse × 0.6) + ArmorDefenseBonus + ShieldDefenseBonus + DeflectionBonus

Example:
  Finesse 80, Plate Armor (+15 Defense), No Shield
  Deflection Tier 7 (+30% Deflection)

  → Defense = (80 × 0.6) + 15 + 0 + 30
  → Defense = 48 + 15 + 30 = 93 Defense (very high)

Absorption:
  Entity must refine deflection over 2-4 months
  Intelligence 70, Wisdom 50:
    → (7 × 30) / (70 + 50) = 1.75 months to fully refine

Self-Refinement:
  Entity may improve deflection beyond teacher's Quality
  → Discover "Riposte" affix (+5% Crit when blocking)
  → Quality 60 → Quality 65 (student surpasses master)
```

**Deflection vs. Shield:**
```
Shields:
  - Provide +8 to +20 Defense
  - Block projectiles (30-50%)
  - Block/reflect spells (rare shields)
  - Require equipment slot

Deflection:
  - Provides +5% to +30% Defense (up to +30 flat bonus)
  - Does NOT block projectiles (melee only)
  - No equipment slot required (weapon-based technique)
  - Requires high Finesse and training
  - Can be self-refined to surpass teacher

Best For:
  - Dual-wielders (no shield slot available)
  - High Finesse duelists (maximize agility)
  - Assassins (fast, evasive combat)
```

---

## DOTS Components (Unity ECS)

### EducationStats Component

```csharp
public struct EducationStats : IComponentData
{
    public byte Education;                  // 0-100 (cumulative learning)
    public byte Wisdom;                     // 0-100 (life experience, cultural knowledge)
    public byte IntelligenceBonus;          // +0 to +25 from education
    public float SkillAcquisitionMultiplier; // 1.0× to 3.0× learning speed

    // School enrollment
    public Entity CurrentSchool;            // School entity (or Entity.Null)
    public Entity CurrentTeacher;           // Teacher entity (or Entity.Null)
    public ushort SchoolYearsCompleted;     // Total years in school

    // Lesson tracking
    public bool IsLearning;                 // Currently enrolled
    public ushort LessonsLearned;           // Total lessons completed
    public ushort LessonsRefined;           // Total lessons fully refined
}
```

### Lesson Component

```csharp
public struct Lesson : IComponentData
{
    public FixedString64Bytes LessonName;   // "Weapon Deflection", "Magic Theory"
    public byte Quality;                    // 1-100 (lesson effectiveness)
    public LessonRarity Rarity;             // Common, Uncommon, Rare, Epic, Legendary
    public byte Tier;                       // 1-10 (complexity)
    public LessonAffix Affix;               // Special bonus (Swift, Riposte, etc.)

    // Effects
    public LessonEffect PrimaryEffect;      // What stat this lesson improves
    public short EffectValue;               // Bonus amount (+30 Defense, +20 Attack, etc.)

    // Absorption tracking
    public float RefinementProgress;        // 0.0 to 1.0 (0% to 100% refined)
    public float CurrentEffectiveness;      // 0.5 to 1.0 (50% to 100%)
    public bool FullyRefined;               // True when 100% refined

    public Entity LearnedFrom;              // Teacher entity (or Entity.Null if self-taught)
    public float LearnedTimestamp;          // When lesson was learned
}

public enum LessonRarity : byte
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

public enum LessonEffect : byte
{
    Defense = 0,                // Weapon deflection
    Attack = 1,                 // Combat training
    SpellPower = 2,             // Magic theory
    Stealth = 3,                // Stealth techniques
    Misdirection = 4,           // Deception training
    AnimalTaming = 5,           // Animal handling
    BotanyYield = 6,            // Plant cultivation
    CraftingQuality = 7,        // Smithing, alchemy, etc.
    // ... more effects
}

public enum LessonAffix : byte
{
    None = 0,
    Swift = 1,                  // +10% Attack Speed
    Riposte = 2,                // +5% Crit when blocking
    Brutal = 3,                 // +15% Damage vs armored
    Efficient = 4,              // -10% Mana cost
    Durable = 5,                // +20% item durability
    // ... more affixes
}
```

### WisdomAccumulation Component (Village/Family/Dynasty)

```csharp
public struct WisdomAccumulation : IComponentData
{
    public float CulturalWisdom;            // Village wisdom (sum / population)
    public float FamilialWisdom;            // Family wisdom (average across members)
    public float DynasticWisdom;            // Dynasty wisdom (across generations)

    public ushort GenerationsTracked;       // How many generations of wisdom stored
    public ushort HighWisdomMemberCount;    // Members with Wisdom 60+

    // Cultural knowledge repository
    public BlobAssetReference<BlobArray<Entity>> StoredLessons; // Lessons stored culturally
}
```

### IndependentLearningTracker Component

```csharp
public struct IndependentLearningTracker : IComponentData
{
    // Activity tracking
    public ushort DaysPerformingActivity;   // Days spent on current activity
    public ActivityType CurrentActivity;    // What entity is doing

    // Learning progress
    public float LearningProgress;          // 0.0 to 1.0 (progress toward next lesson)
    public byte LearningChance;             // % chance per 30 days

    // Observation tracking
    public ushort CombatsObserved;          // Total combats watched
    public ushort SpellsObserved;           // Total spells watched
    public ushort CraftsObserved;           // Total crafts watched
}

public enum ActivityType : byte
{
    None = 0,
    AnimalHandling = 1,
    Farming = 2,
    CombatObservation = 3,
    MagicObservation = 4,
    CraftingObservation = 5,
    Smithing = 6,
    Alchemy = 7,
    // ... more activities
}
```

---

## Integration with Existing Systems

### Individual Combat System
- **Deflection** adds +5% to +30% Defense (weapon-based blocking)
- Mentors teach combat techniques (dual-wielding, critical hit, advanced stances)
- Observation learning allows entities to learn combat by watching fights
- Education affects tactical intelligence (better combat decisions)

### Individual Progression
- **Education** increases Intelligence (+0 to +25)
- **Wisdom** increases with age, experience, cultural transmission
- **Skill Acquisition Speed** multiplier (1.0× to 3.0×)
- Lessons unlock new abilities (deflection, dual-wield, advanced magic)

### Wealth & Social Dynamics
- **Elite monopoly** on quality education (private schools, tutors)
- **Poor children** limited to village schools (low quality)
- Education reinforces wealth stratification (elite graduates → courtiers, rulers)
- **Dynastic wisdom** creates elite knowledge monopolies

### Guild System
- **Guild schools** train specialized skills (thieves, mages, blacksmiths)
- Guild loyalty instilled through education (+30 relation)
- Trade secrets taught only to guild members (Epic/Legendary lessons)
- **Guild wisdom** accumulates across generations

### Elite Courts & Retinues
- **Private tutors** for elite children (prepare for courtier roles)
- Champions learn weapon deflection from master mentors
- Spymasters teach misdirection, political intrigue to heirs
- **Dynastic wisdom** ensures elite families maintain power

### Stealth & Perception System
- **Stealth mentors** teach advanced techniques (+20% Stealth)
- Assassination training (first strike +100% damage)
- Trap disarming lessons (unlock complex traps)
- **Observation learning** allows spies to learn by watching

### Alignment Framework
- **Teachers shift outlooks** (NOT alignments)
- Village schools reinforce village outlooks
- Chaotic entities prefer independent learning (self-teaching)
- Lawful entities prefer formal education (universities)

---

## Summary

The revised Education & Tutoring System creates:

- **Wisdom stat** (Perception + Will + Intelligence) determines learning speed
- **Lesson absorption** requires time and refinement (Intelligence + Wisdom)
- **Self-refinement** allows students to surpass teachers (create better lessons)
- **Independent learning** through activities (taming animals, botany, observing combat/magic)
- **Observation-based learning** forms self-created lessons with unique affixes
- **Outlook shift** from teachers (NOT personality traits, NOT alignments)
- **Cultural/familial/dynastic wisdom** accumulates over generations
- **Village wisdom** is sum of inhabitants' wisdom
- **Teachers' lessons** have quality and rarity (like mentor lessons)

This creates emergent narratives of:
- Self-taught masters surpassing their teachers
- Villages losing wisdom through war/plague (knowledge collapse)
- Dynasties hoarding Epic/Legendary knowledge
- Prodigies learning by observation (copying masters)
- Cultural wisdom shaping village character

---

**Related Documentation:**
- **Individual Combat:** [Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - Deflection mechanic, combat training
- **Individual Progression:** [Individual_Progression_System.md](Individual_Progression_System.md) - Education stat, skill acquisition, Wisdom stat
- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](Wealth_And_Social_Dynamics.md) - Elite education access, class stratification
- **Guild System:** [Guild_System.md](Guild_System.md) - Guild schools, specialized training
- **Elite Courts:** [Elite_Courts_And_Retinues.md](Elite_Courts_And_Retinues.md) - Private tutors for heirs, courtier preparation
- **Stealth & Perception:** [../Combat/Stealth_And_Perception_System.md](../Combat/Stealth_And_Perception_System.md) - Stealth mentor training
- **Alignment Framework:** [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Outlook shift (not alignment)

---

**Last Updated:** 2025-11-07
**Status:** Concept Draft - Core vision captured, revised for lesson absorption, wisdom, and independent learning
