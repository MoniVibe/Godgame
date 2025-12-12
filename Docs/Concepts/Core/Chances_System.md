# Chances System - Godgame

## Overview

The **Chances System** enables entities to recognize and act on fleeting opportunities during critical moments. Entities must perceive these opportunities (Perception check), evaluate risks, and decide whether to invest focus to attempt them. Successful chance-taking yields experience bonuses and wisdom gains, creating emergent dramatic moments and character development.

---

## Core Concepts

### 1. Opportunity Recognition

Chances exist as transient opportunities that entities may or may not perceive:

- **Perception-Gated**: Entities must pass a Perception check to identify that a chance exists
- **Context-Triggered**: Specific situations enable specific types of chances
- **Profession-Modified**: Different professions excel at recognizing different chances
  - Spies: Expert at identifying subtle opportunities (social, infiltration, information)
  - Warriors: Skilled at combat opportunities (mini-crits, tactical openings)
  - Mages: Attuned to magical opportunities (spell learning, mana disruptions)
  - Civilians: Poor at recognizing most chances unless desperate

### 2. Risk Evaluation

Once perceived, entities evaluate:

- **Success Probability**: Likelihood of success based on skills, circumstances, timing
- **Consequence Severity**: Potential outcomes if failed (death, injury, exposure, loss of status)
- **Desperation Factor**: Dire circumstances lower risk tolerance threshold
- **Personality Traits**: Bold entities take more chances; cautious entities need higher success probability

### 3. Focus Investment

Attempting a chance requires investing Focus:

- **Variable Cost**: Different chances require different focus amounts
- **Opportunity Cost**: Focus spent cannot be used for other actions
- **Success Modifier**: Higher focus investment can improve success probability (diminishing returns)

---

## Chance Types

### A. Desertion Chances

Entities trapped in hostile groups (cults, gangs, armies) may attempt to escape during chaos:

**Triggers:**
- Group under attack by external force (city guard, rival cult, army)
- Infiltration by hostile entities performing sabotage
- Leadership distracted or incapacitated
- Clemency offered by attacking force during showdown
- Mass confusion (fire, flood, monster attack)

**Perception Check:**
- Base Difficulty: 60
- Modifiers:
  - Alert/Paranoid: +15
  - Desperate (threatened by death): -20
  - Previous escape attempts: +10 per attempt
  - Insider knowledge of group routines: -15

**Evaluation Factors:**
- Survival probability if staying: 20-40% (death threats from group)
- Survival probability if deserting: 50-80% (depends on chaos level, guard attention)
- Alignment with attacking force: +20% if similar behavior profile
- Group's pursuit capability: -30% if group highly organized

**Focus Cost:** 30-50 (high stakes decision)

**Outcomes:**
- **Success**: Escape during chaos, possibly join attackers if compatible
- **Partial Success**: Escape but injured, pursued, or exposed identity
- **Failure**: Captured by own group, severe punishment or death

**Example:**
```
Scenario: Brother Aldric, cult member threatened with blood ritual
- Cult compound under attack by Inquisition forces
- Perception check: 65 vs DC 60 (desperate -20, alert +15) = SUCCESS
- Evaluation: 25% survival if stays, 70% if deserts + Inquisition offers clemency
- Focus investment: 40
- Attempt: During courtyard battle, Aldric approaches Inquisitor
- Outcome: SUCCESS - Reveals cult secrets, joins Inquisition as informant
- Rewards: +35% experience (Will archetype) for 20 minutes, +3 Wisdom
```

### B. Motive Reading (Eye Contact)

During intense moments, entities can invest focus to read each other's true intentions:

**Triggers:**
- Combat standoffs (hostage situations, duels, skirmishes)
- Tense negotiations (both parties suspicious)
- Shared danger (ambush by third party, environmental threat)
- Brief respite in battle (catching breath)

**Perception Check:**
- Base Difficulty: 70
- Modifiers:
  - High INT: -10
  - High WIS: -15
  - Observant trait: -10
  - Target wearing mask/helmet: +20
  - Poor lighting: +10

**Focus Investment:**
- Minimum: 20 (surface motives - hostile/neutral/friendly)
- Moderate: 35 (deeper motives - specific goals, loyalties)
- Maximum: 50 (full intent reading - fears, weaknesses, desperation)

**Mutual Recognition:**
If BOTH entities invest sufficient focus and succeed perception checks:
- **Instant Understanding**: Both recognize each other's motives simultaneously
- **Behavior Profile Matching**:
  - Similar Peaceful profiles: Likely form temporary alliance (80%)
  - Similar Warlike profiles: Acknowledge each other but continue combat (60%), or respect-based truce (40%)
  - Opposing profiles: Clear recognition of enmity, no alliance
- **Companionship Duration**: Short-lived (until immediate threat resolved or 1-24 hours)

**Outcomes:**
- **Success**: Correctly identify target's motives and intentions
- **Partial Success**: General impression (hostile/neutral/friendly) but not specifics
- **Failure**: Misread motives, potentially dangerous misunderstanding

**Example:**
```
Scenario: Sir Baran (knight) and Kael (rogue) cornered by undead horde
- Both invest 35 focus for motive reading
- Baran perception: 75 vs DC 70 = SUCCESS
- Kael perception: 80 vs DC 70 = SUCCESS
- Mutual recognition: Both see "survival priority, honor-bound to fight" (Baran) and "pragmatic survivor, not evil" (Kael)
- Behavior profiles: Lawful Good (Baran) vs Neutral Good (Kael) - similar enough
- Alliance formed: Fight undead together for 3 hours until horde defeated
- Rewards (both): +25% experience (Physical + Will) for 15 minutes, +2 Wisdom each
```

### C. Spell Learning by Observation

Mages can attempt to learn spells by carefully observing others cast them:

**Triggers:**
- Witnessing spell cast in combat or demonstration
- Multiple observations of same spell (stacking bonus)
- Access to grimoire or scroll being studied by another mage

**Perception Check:**
- Base Difficulty: 80 (complex magical patterns)
- Modifiers:
  - High INT: -15
  - Arcane Sight spell active: -20
  - Similar spell school known: -10
  - Target obscures casting (Silent Spell metamagic): +30
  - Distance from caster: +5 per 20 feet

**Focus Investment:**
- Minimum: 50 (requires intense concentration)
- Maximum: 100 (attempting to grasp every nuance)
- Per observation: Can spend additional focus on repeat viewings

**Success Probability:**
- Base: 15-25% (very difficult)
- +5% per observation of same spell
- +10% if INT 140+
- +15% if have Spell Thief feat
- -20% if spell is 3+ tiers above known spells

**Outcomes:**
- **Critical Success (10% of successes)**: Learn spell perfectly, add to spellbook
- **Success**: Learn incomplete version (80% effectiveness, requires practice)
- **Partial Success**: Gain insight (+20% on next attempt)
- **Failure**: Nothing learned, focus wasted
- **Critical Failure (5% of failures)**: Magical backlash, 1d6 damage, stunned 1 round

**Example:**
```
Scenario: Mage Lyria observing enemy sorcerer cast Chain Lightning
- First observation: Perception 90 vs DC 80 = SUCCESS
- Focus investment: 75
- Success roll: 18% base + 10% (INT 145) + 5% (Lightning school known) = 33% chance
- Roll: 28 = SUCCESS (incomplete)
- Lyria learns 80% power Chain Lightning (2d6 instead of 3d6 per target)
- Practice requirement: Cast 10 times to master full version
- Rewards: +45% experience (Will archetype) for 25 minutes, +4 Wisdom
```

### D. Secret Learning

Entities may stumble upon or intentionally seek secrets during opportune moments:

**Triggers:**
- Overhearing conversations while disguised
- Finding unattended documents during infiltration
- Guard/servant gossip during shift changes
- Interrogating captured enemy during chaos
- Discovering hidden room/cache during search

**Perception Check:**
- Base Difficulty: Variable (50-90 depending on secret importance)
- Modifiers:
  - High WIS: -10
  - Keen Senses trait: -15
  - Disguise active: -10 (NPCs less guarded)
  - Time pressure (alarm sounding): +20
  - Paranoid environment: +15

**Focus Investment:** 20-40 (moderate attention)

**Outcomes:**
- **Success**: Learn complete secret (codes, passwords, plans, identities)
- **Partial Success**: Learn partial information (clues, hints)
- **Failure**: Nothing useful learned
- **Critical Failure**: Exposure (cover blown, alarm raised)

**Example:**
```
Scenario: Spy Elena infiltrating noble manor as servant
- Overhears conversation: Lord discussing assassination plot
- Perception check: 75 vs DC 65 = SUCCESS
- Focus investment: 30
- Secret learned: "Duke Aldous to be poisoned at feast, 3 days hence"
- Rewards: +30% experience (Finesse archetype) for 15 minutes, +3 Wisdom
- Intelligence value: High (can prevent assassination or sell to rivals)
```

### E. Mini-Criticals (Combat)

Warriors can invest small amounts of focus during combat for higher critical hit chances:

**Triggers:**
- Active combat (melee or ranged)
- Target vulnerable (off-balance, distracted, wounded)
- Warrior has opening (initiative, flanking, surprise)

**Perception Check:**
- Base Difficulty: 40 (recognizing combat openings)
- Modifiers:
  - High DEX: -10
  - Combat Reflexes trait: -15
  - Target heavily armored: +15
  - Chaotic battlefield: +10

**Focus Investment:**
- Minimum: 5 (small investment for +5% crit chance)
- Moderate: 10 (+10% crit chance)
- Maximum: 15 (+15% crit chance, diminishing returns)

**Damage Modifier:**
- Mini-crits deal 80% of normal critical damage
- Example: Normal crit = 2x damage, Mini-crit = 1.8x damage

**Critical Chance:**
- Base weapon crit chance: 5-20% (weapon dependent)
- +5-15% from focus investment
- Total: 10-35% crit chance for that attack

**Outcomes:**
- **Mini-Crit Success**: Deal 1.6-1.8x damage instead of normal
- **Normal Hit**: Regular damage, focus wasted
- **Miss**: No effect, focus wasted

**Example:**
```
Scenario: Warrior Gorak fighting bandit captain
- Recognizes opening after bandit's overhand swing
- Perception check: 55 vs DC 40 = SUCCESS
- Focus investment: 10 (for +10% crit chance)
- Attack roll: Hit!
- Crit roll: 18% base + 10% focus = 28% chance, roll 25 = MINI-CRIT
- Damage: 12 normal → 21 mini-crit (1.75x)
- Rewards: +20% experience (Physical archetype) for 10 minutes, +1 Wisdom
```

### F. Escape from Hostile Territory

Entities operating under guise in hostile areas may attempt to flee when cover threatened:

**Triggers:**
- Cover story questioned by authorities
- Identity about to be verified (papers checked, face seen)
- Magical detection imminent (True Seeing, Detect Lies)
- Contact captured and may reveal network
- Mission compromised (target alerted, trap sprung)

**Perception Check:**
- Base Difficulty: 55
- Modifiers:
  - High WIS: -10
  - Streetwise skill: -15
  - Unfamiliar with area: +20
  - Pursuit already initiated: +25

**Evaluation Factors:**
- Escape route availability: 0-3 known routes
- Pursuit speed vs own speed
- Safe house distance
- Ally support availability
- Expendable resources (smoke bombs, distractions, bribes)

**Focus Investment:** 25-45 (escape planning + execution)

**Outcomes:**
- **Success**: Clean escape, pursuers lose trail
- **Partial Success**: Escape but injured, or safe house compromised
- **Failure**: Captured, interrogated, or forced to fight
- **Critical Failure**: Captured + allies exposed

**Example:**
```
Scenario: Agent Marcus infiltrating enemy fortress
- Guard recognizes face from wanted poster
- Perception check: 70 vs DC 55 = SUCCESS
- Evaluation: 2 escape routes known, fortress on alert, 30% capture risk
- Focus investment: 35
- Attempt: Drop smoke bomb, sprint to hidden tunnel
- Outcome: SUCCESS - Escapes through tunnel, pursuers search wrong area
- Rewards: +40% experience (Finesse archetype) for 20 minutes, +4 Wisdom
```

---

## Mechanics

### Perception Check System

```
Perception Roll = d100 + (Perception Skill × 0.5) + Profession Modifier

Success: Roll ≥ Difficulty
- Critical Success (roll 95+): Identify additional opportunities
- Failure (roll < DC): Opportunity not recognized
- Critical Failure (roll ≤ 5): Misidentify danger as opportunity
```

**Profession Modifiers:**
- Spy/Assassin: +20 (expert at spotting chances)
- Warrior/Guard: +10 (combat-focused perception)
- Mage/Scholar: +5 (analytical but not street-smart)
- Merchant/Diplomat: +5 (social awareness)
- Civilian/Laborer: +0 (untrained)

**Circumstance Modifiers:**
- Desperate situation: -15 (heightened awareness)
- Distracted (wounded, fatigued): +15
- High INT (140+): -5
- High WIS (140+): -10
- Keen Senses trait: -15
- Dull Senses trait: +15

### Risk Evaluation System

Entities evaluate chances using:

```
Expected Value = (Success Probability × Reward Value) - (Failure Probability × Consequence Severity)

Risk Tolerance Threshold = Base Threshold × Desperation Factor × Personality Modifier

Attempt Chance if: Expected Value > Risk Tolerance Threshold
```

**Base Threshold by Personality:**
- Bold/Reckless: 10 (low threshold, takes many chances)
- Balanced: 30 (moderate threshold)
- Cautious: 60 (high threshold, only obvious good chances)

**Desperation Factor:**
- No threat: 1.0
- Minor threat (loss of money, status): 0.8
- Moderate threat (imprisonment, exile): 0.5
- Severe threat (death, torture): 0.2
- Immediate death: 0.05 (will take almost any chance)

**Example Evaluation:**
```
Cult member desertion during raid:
- Success Probability: 70%
- Reward Value: 90 (survival + freedom)
- Failure Probability: 30%
- Consequence Severity: 80 (torture + death)
- Expected Value = (0.7 × 90) - (0.3 × 80) = 63 - 24 = 39

Personality: Cautious
Base Threshold: 60
Desperation Factor: 0.2 (death threat)
Adjusted Threshold: 60 × 0.2 = 12

39 > 12 → ATTEMPT DESERTION
```

### Focus Investment System

```
Focus Cost = Base Cost + (Difficulty Modifier × 0.5)

Success Probability Modifier = +2% per additional 5 focus (beyond minimum)
Maximum Bonus: +20% (100 additional focus)
```

**Focus Costs by Chance Type:**
| Chance Type | Min Focus | Optimal Focus | Max Focus |
|-------------|-----------|---------------|-----------|
| Mini-Critical | 5 | 10 | 15 |
| Secret Learning | 20 | 30 | 40 |
| Desertion | 30 | 40 | 50 |
| Motive Reading | 20 | 35 | 50 |
| Escape | 25 | 35 | 45 |
| Spell Learning | 50 | 75 | 100 |

### Rewards System

Successful chance-taking grants temporary experience bonuses and wisdom:

**Experience Bonus:**
```
Bonus Magnitude = 15% + (Difficulty × 0.4%)
Duration = 10 minutes + (Difficulty × 0.15 minutes)

Applies to archetype tasks:
- Physical: Combat, athletics, crafting
- Finesse: Stealth, lockpicking, acrobatics
- Will: Spellcasting, persuasion, knowledge
```

**Bonus by Difficulty:**
| Difficulty | XP Bonus | Duration |
|------------|----------|----------|
| Easy (40) | +20% | 16 min |
| Moderate (60) | +30% | 19 min |
| Hard (80) | +40% | 22 min |
| Very Hard (90) | +50% | 24 min |

**Wisdom Gain:**
```
Wisdom Gained = 1 + floor(Difficulty / 30) + Critical Success Bonus

Critical Success: +2 Wisdom
Normal Success: +0 to +3 Wisdom
Failure: +0 Wisdom (no penalty)
```

**Example:**
```
Hard chance (DC 80) success:
- XP Bonus: +40% to relevant archetype tasks
- Duration: 22 minutes
- Wisdom: +3 (1 base + 2 from DC 80 + 0 normal success)
```

---

## ECS Components

### ChanceOpportunity

```csharp
public struct ChanceOpportunity : IComponentData
{
    public ChanceType Type;              // Desertion, MotiveReading, SpellLearning, etc.
    public float PerceptionDifficulty;   // 40-90 DC
    public float SuccessProbability;     // 15-80% base
    public float RewardValue;            // 1-100 expected utility
    public float ConsequenceSeverity;    // 1-100 penalty if failed
    public float MinFocusCost;           // 5-100 focus required
    public float OptimalFocusCost;       // 10-150 focus optimal
    public float WindowDuration;         // 5-60 seconds (opportunity expires)
    public float TimeRemaining;          // Countdown timer
    public Entity Trigger;               // What created this opportunity (entity, event)
}

public enum ChanceType : byte
{
    Desertion,
    MotiveReading,
    SpellLearning,
    SecretLearning,
    MiniCritical,
    Escape,
    AllianceFormation,
    Intimidation,
    Deception
}
```

### ChancePerception

```csharp
public struct ChancePerception : IComponentData
{
    public float PerceptionSkill;        // 0-200 skill level
    public float ProfessionModifier;     // -20 to +20 (spy +20, civilian +0)
    public float PerceptionBonus;        // From INT, WIS, traits
    public float PerceptionPenalty;      // From distraction, wounds, fatigue
    public float LastCheckTime;          // Time since last perception check
    public float CheckCooldown;          // 1-5 seconds between checks
}
```

### ChanceEvaluation

```csharp
public struct ChanceEvaluation : IComponentData
{
    public float ExpectedValue;          // Calculated EV of chance
    public float RiskToleranceThreshold; // Personality-based threshold
    public float DesperationFactor;      // 0.05-1.0 (lower = more desperate)
    public PersonalityType Personality;  // Bold (10), Balanced (30), Cautious (60)
    public float CurrentThreatLevel;     // 0-100 (determines desperation)
    public bool WillAttempt;             // Decision result
}

public enum PersonalityType : byte
{
    Reckless = 5,   // Takes almost any chance
    Bold = 10,      // Takes many chances
    Balanced = 30,  // Moderate risk-taking
    Cautious = 60,  // Only good odds
    Paranoid = 90   // Almost never takes chances
}
```

### ChanceAttempt

```csharp
public struct ChanceAttempt : IComponentData
{
    public ChanceType Type;
    public Entity Target;                // Target of attempt (entity, spell, secret)
    public float FocusInvested;          // 5-150 focus spent
    public float BonusSuccessChance;     // +0% to +20% from extra focus
    public float TotalSuccessChance;     // Base + bonuses
    public float AttemptStartTime;       // When attempt began
    public float AttemptDuration;        // 1-10 seconds (varies by type)
    public AttemptState State;
}

public enum AttemptState : byte
{
    InProgress,
    Success,
    PartialSuccess,
    Failure,
    CriticalSuccess,
    CriticalFailure
}
```

### ChanceRewards

```csharp
public struct ChanceRewards : IComponentData
{
    public float ExperienceBonus;        // +15% to +50%
    public ArchetypeType BonusArchetype; // Physical, Finesse, or Will
    public float BonusDuration;          // 10-30 minutes
    public float BonusStartTime;         // When bonus applied
    public int WisdomGained;             // +1 to +5
    public bool IsActive;                // Whether bonus is currently active
}

public enum ArchetypeType : byte
{
    Physical,  // Combat, athletics, crafting
    Finesse,   // Stealth, acrobatics, lockpicking
    Will       // Magic, persuasion, knowledge
}
```

### MotiveReadingState

```csharp
public struct MotiveReadingState : IComponentData
{
    public Entity OtherEntity;           // Entity being read / reading you
    public float FocusInvested;          // 20-50 focus
    public float ReadingDepth;           // 20 focus = surface, 50 = deep
    public bool MutualRecognition;       // Both succeeded perception + focus
    public MotiveType DetectedMotive;    // What was detected
    public BehaviorProfile MyProfile;    // Peaceful, Warlike, Neutral
    public BehaviorProfile TheirProfile; // Peaceful, Warlike, Neutral
    public float AllianceProbability;    // 0-100% based on profile matching
    public float AllianceDuration;       // 1-24 hours if formed
}

public enum MotiveType : byte
{
    Hostile,           // Actively wants to harm
    Neutral,           // Indifferent
    Friendly,          // Cooperative intent
    Survival,          // Desperate to survive
    Opportunistic,     // Will exploit weakness
    HonorBound,        // Follows code of honor
    Deceptive          // Lying about true intent
}

public enum BehaviorProfile : byte
{
    Peaceful,   // Avoids violence, cooperative
    Neutral,    // Pragmatic, situation-dependent
    Warlike     // Aggressive, combat-seeking
}
```

### MiniCriticalState

```csharp
public struct MiniCriticalState : IComponentData
{
    public Entity Target;                // Combat target
    public float FocusInvested;          // 5-15 focus
    public float BonusCritChance;        // +5% to +15%
    public float TotalCritChance;        // Base weapon + bonus
    public float DamageMultiplier;       // 1.6x to 1.8x (less than full crit)
    public bool IsActive;                // Ready to attempt on next attack
    public float WindowDuration;         // 3-5 seconds before expires
}
```

---

## Example Scenarios

### Scenario 1: Cult Desertion During Raid

**Setup:**
- Brother Aldric, cult member (Level 3 Acolyte)
- Blood Moon Cult compound under attack by City Guard
- Aldric threatened with sacrifice if fails loyalty test (death threat)
- 40 guards vs 25 cultists, guard captain offers clemency to deserters

**Chance Recognition:**
- Perception check: 85 (65 base + 10 WIS) vs DC 60 (base 60 + 15 alert - 20 desperate + 5 profession)
- Result: SUCCESS - Aldric recognizes desertion opportunity

**Risk Evaluation:**
- Success Probability: 65% (chaos provides cover, guards distracted, captain's offer genuine)
- Reward Value: 85 (survival 80 + freedom 5)
- Failure Probability: 35%
- Consequence Severity: 90 (cult will torture + sacrifice if caught)
- Expected Value = (0.65 × 85) - (0.35 × 90) = 55.25 - 31.5 = 23.75

- Personality: Balanced (threshold 30)
- Desperation Factor: 0.2 (death threat)
- Adjusted Threshold: 30 × 0.2 = 6

- 23.75 > 6 → ATTEMPT DESERTION

**Execution:**
- Focus investment: 45 (high stakes)
- Attempt: During courtyard melee, Aldric drops weapon, approaches guard captain
- Success roll: 65% base + 4% (extra 10 focus) = 69% chance, roll 52 = SUCCESS
- Outcome: Guard captain accepts surrender, Aldric provides cult intelligence
- Intelligence: Reveals ritual chamber location, cultist identities, sacrifice schedule

**Rewards:**
- Experience Bonus: +35% (Will archetype) for 19 minutes
- Wisdom Gained: +3 (1 base + 2 from DC 60)
- Narrative: Aldric becomes guard informant, helps dismantle cult network

### Scenario 2: Mutual Motive Reading During Ambush

**Setup:**
- Sir Baran (Lawful Good Knight, Level 5)
- Kael (Neutral Good Rogue, Level 4)
- Both cornered by 15 undead skeletons in crypt
- Neither knows the other, both fighting separately

**Trigger:**
- Brief lull in combat (skeletons regrouping)
- Baran and Kael make eye contact, both breathing hard
- Each recognizes other is skilled fighter, not undead thrall

**Chance Recognition:**
- Baran perception: 75 (60 base + 10 WIS + 5 knight training) vs DC 70 = SUCCESS
- Kael perception: 82 (70 base + 12 WIS) vs DC 70 = SUCCESS
- Both recognize opportunity to assess each other's motives

**Focus Investment:**
- Both invest 35 focus (moderate depth reading)
- Baran wants to know: "Is this rogue an enemy or potential ally?"
- Kael wants to know: "Will this knight kill me after the undead are dead?"

**Motive Reading Results:**
- Baran reads Kael:
  - Surface: Not evil, pragmatic survivor
  - Deeper: "Wants to survive, willing to cooperate, no honor code but not treacherous"
  - Profile: Neutral Good (Neutral behavior)

- Kael reads Baran:
  - Surface: Honorable, duty-bound
  - Deeper: "Sworn to fight undead, respects competent fighters, won't attack non-evil on sight"
  - Profile: Lawful Good (Peaceful + Honorable)

**Mutual Recognition:**
- Both succeeded perception and invested sufficient focus
- Instant understanding: "We're not enemies, and this undead horde will kill us if we don't work together"
- Behavior profile comparison: Peaceful-leaning (Baran) + Neutral (Kael) = Compatible (70% alliance probability)

**Alliance Formation:**
- Baran: "Rogue, guard my left flank!"
- Kael: "On it, tin man!"
- Tactical coordination: Baran tanks skeleton attacks, Kael backstabs from flanks
- Duration: Until undead destroyed (estimated 15-45 minutes)

**Outcome:**
- Fight 15 skeletons cooperatively
- Baran takes 24 damage, Kael takes 8 damage
- All skeletons destroyed in 18 minutes
- Post-battle: Respectful parting, no hostility
  - Baran: "You fight with honor, rogue."
  - Kael: "You're not so bad yourself, knight."
  - Alliance dissolves naturally after mutual nod

**Rewards (Both Entities):**
- Experience Bonus: +30% (Physical + Will archetypes) for 18 minutes
- Wisdom Gained: +2 each (1 base + 1 from DC 70)
- Narrative: Both learned that enemies-by-default can cooperate when necessary

### Scenario 3: Spell Learning by Observation

**Setup:**
- Lyria (Evoker Mage, Level 6, INT 145)
- Knows: Fireball, Lightning Bolt, Cone of Cold
- Observing: Enemy Sorcerer Malthus casting Chain Lightning (Tier 4 spell)
- Context: Malthus attacking city walls, Lyria hidden 60 feet away

**Chance Recognition:**
- Perception check: 92 (75 base + 15 INT + 2 Evoker profession) vs DC 80 = SUCCESS
- Spell is one tier above Lyria's known spells (challenging but not impossible)

**Focus Investment:**
- First observation: 75 focus (intense concentration)
- Lyria watches Malthus's hand gestures, verbal components, mana flow patterns

**Success Probability:**
- Base: 20% (difficult spell, higher tier)
- INT 145 bonus: +10%
- Lightning school known (Lightning Bolt): +10%
- First observation: +0%
- Total: 40% chance

**Attempt:**
- Roll: 38 = SUCCESS (barely!)
- Lyria grasps fundamental structure of Chain Lightning
- Incomplete version learned: 70% effectiveness (2d6 damage per target instead of 3d6)
- Requires practice to master full version

**Practice Requirement:**
- Must cast incomplete version 15 times successfully
- Each casting: 15% chance to improve (cumulative)
- Expected mastery: 7-10 practice sessions
- Full mastery: 3d6 damage per target, can chain to 6 targets (full power)

**Rewards:**
- Experience Bonus: +45% (Will archetype) for 22 minutes
- Wisdom Gained: +4 (1 base + 2 from DC 80 + 1 spell learning bonus)
- Spellbook: Incomplete Chain Lightning added

**Follow-Up:**
- Lyria retreats to safe house, practices new spell
- 8 practice sessions over 3 days
- Roll 68 on mastery check after 8th session = SUCCESS
- Full Chain Lightning mastered: 3d6 damage per target, 6 target max
- Narrative: Lyria's magical arsenal expanded through observation and dedication

### Scenario 4: Warrior Mini-Critical Chain

**Setup:**
- Gorak (Barbarian Warrior, Level 7, DEX 120, STR 160)
- Fighting: Bandit Captain Vex (Level 6, DEX 110, HP 52/65)
- Context: Duel for leadership of bandit camp
- Gorak's Greataxe: Base crit chance 15%, crit multiplier 2.5x

**Combat Round 1:**
- Initiative: Gorak wins
- Perception check: 58 (45 base + 8 DEX + 5 warrior profession) vs DC 40 = SUCCESS
- Recognizes opening: Vex's left guard is low after previous block
- Focus investment: 10 (for +10% crit chance)
- Attack roll: Hit! (18 vs AC 16)
- Crit roll: 15% base + 10% focus = 25% chance, roll 22 = MINI-CRIT
- Damage: 14 normal → 24 mini-crit (1.7x multiplier instead of 2.5x)
- Vex HP: 52 → 28

**Combat Round 2:**
- Vex attacks: Hit for 11 damage, Gorak HP 68 → 57
- Gorak's turn:
  - Perception check: 62 vs DC 40 = SUCCESS
  - Opening: Vex overextended on attack
  - Focus investment: 15 (max, for +15% crit chance)
  - Attack roll: Hit! (20 vs AC 16)
  - Crit roll: 15% base + 15% focus = 30% chance, roll 18 = MINI-CRIT
  - Damage: 16 normal → 28 mini-crit (1.75x multiplier)
  - Vex HP: 28 → 0 (DEFEATED)

**Outcome:**
- Gorak defeats Vex in 2 rounds using mini-crits
- Bandits acknowledge Gorak as new captain (intimidation + strength)
- Total focus spent: 25 (cost-effective combat strategy)

**Rewards (Cumulative):**
- Round 1 mini-crit: +20% experience (Physical) for 10 minutes, +1 Wisdom
- Round 2 mini-crit: +20% experience (Physical) for 10 minutes (refreshed), +1 Wisdom
- Total: +2 Wisdom, 20% Physical XP bonus active
- Narrative: Gorak's tactical combat awareness demonstrated, earns bandit loyalty

---

## Design Notes

### Emergence and Drama

The Chances System creates emergent storytelling moments:

- **Opportunistic Gameplay**: Entities don't just follow scripted behaviors; they recognize and exploit opportunities
- **Moral Complexity**: Desertion, alliances, and motive reading create gray-area decisions
- **Skill Expression**: High-perception, high-intelligence entities excel at identifying and capitalizing on chances
- **Risk-Reward Dynamics**: Bold entities take frequent chances and learn faster; cautious entities miss opportunities but avoid disasters

### Archetype Integration

Chances tie into the three core archetypes:

- **Physical Archetype**: Mini-criticals, combat opportunities, physical escapes
- **Finesse Archetype**: Espionage chances, desertion, secret learning, escape from pursuit
- **Will Archetype**: Spell learning, motive reading, intimidation, magical opportunities

### Wisdom as Meta-Progression

Wisdom gains from chance-taking represent:

- **Meta-Learning**: Entities learn from taking risks, not just succeeding at tasks
- **Character Growth**: Even failed chances don't penalize wisdom (learning from mistakes)
- **Experiential Knowledge**: Wisdom grows from exposure to critical decision points

### Balance Considerations

- **Focus Costs**: Prevent spamming chances; entities must choose when to invest focus
- **Cooldowns**: Perception checks have 1-5 second cooldowns to prevent constant scanning
- **Expiring Opportunities**: Chances have 5-60 second windows; must act quickly
- **Diminishing Returns**: Extra focus investment provides diminishing success bonuses (+2% per 5 focus, max +20%)

### AI Decision-Making

NPC entities evaluate chances using:
1. Perception check (can they see it?)
2. Risk evaluation (is it worth attempting?)
3. Personality modifier (bold vs cautious)
4. Desperation factor (dire situations lower threshold)
5. Focus availability (can they afford the cost?)

This creates believable AI behavior where:
- Desperate NPCs take risky chances
- Cautious NPCs only take obvious good opportunities
- Bold NPCs frequently exploit marginal chances
- High-intelligence NPCs correctly evaluate risk-reward

---

## Summary

The **Chances System** enables emergent, dramatic gameplay where entities:
- **Perceive** fleeting opportunities during critical moments
- **Evaluate** risks vs rewards based on skills, personality, and desperation
- **Invest** focus to attempt chances with variable success rates
- **Gain** experience bonuses and wisdom from successful risk-taking

Opportunities include desertion from hostile groups, reading others' motives through eye contact, learning spells by observation, discovering secrets, executing mini-critical hits, and escaping hostile territory.

The system integrates with the Three Pillar ECS architecture:
- **Body Pillar (60 Hz)**: Combat chances (mini-crits), perception checks
- **Mind Pillar (1 Hz)**: Risk evaluation, focus investment decisions
- **Aggregate Pillar (0.2 Hz)**: Long-term wisdom accumulation, personality trait influences

**Core Formula:**
```
Chance Success = Perception Check (recognize) + Risk Evaluation (decide) + Focus Investment (attempt) + Success Roll (outcome) + Rewards (growth)
```

This system rewards attentive, intelligent, and bold entities while creating memorable story moments through opportunistic decision-making.
