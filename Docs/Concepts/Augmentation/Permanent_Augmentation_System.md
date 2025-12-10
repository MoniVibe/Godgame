# Permanent Augmentation System - Godgame

## Overview

At mid-tech tiers, augmentation technology reaches a critical threshold where certain experimental procedures require **irreversible commitment** from living subjects. These permanent augmentations represent the bleeding edge of biological and arcane enhancement, pioneered by those willing to sacrifice their original form—whether by choice, desperation, or coercion.

Unlike removable grafts or reversible enhancements, permanent augmentation **interrs the consciousness** within a fundamentally altered chassis, creating beings that exist between flesh and construct, mortality and immortality.

---

## Permanent Augmentation Categories

### 1. **Prototype Experimental Augments**

Early-stage augmentations that require living test subjects to pioneer untested technology.

**Subject Acquisition:**
- **Volunteers**: Glory-seekers, dying warriors seeking purpose, religious zealots, condemned criminals offered redemption
- **Conscripts**: War prisoners, sentenced criminals, indentured servants, orphans with no legal protections
- **Desperate Injured**: Those with fatal wounds who accept permanent chassis as alternative to death

**Procedure Outcomes:**

```
Success Chance = (Tech Level × 8) + (Subject Willpower × 2) + (Practitioner Skill × 3) - (Augment Complexity × 2) - (Subject Age / 5)

Results:
- Critical Success (90-100%): Augment integrates perfectly, subject retains full consciousness, gains enhanced capabilities
- Success (70-89%): Augment functional, minor psychological scarring, some original identity retained
- Partial Success (50-69%): Augment works but subject suffers personality fragmentation, reduced autonomy
- Failure (30-49%): Subject survives but augment malfunctions, severe madness, must be interred or terminated
- Critical Failure (0-29%): Subject dies during procedure, soul potentially trapped in failed augment
```

**Example Prototypes:**
- **Arcane Blood Reactor**: Heart replaced with mana-generating crystal core. Subject becomes living mana battery but suffers constant pain (Willpower checks to avoid madness)
- **Chimeric Graft**: Animal limbs/organs permanently fused to subject. Risk of consciousness merging with beast nature
- **Golem Frame Integration**: Subject's skeleton replaced with enchanted metal. Body becomes more construct than flesh
- **Hive Mind Node**: Subject's brain augmented to network with others. Risk of losing individual identity in collective

---

### 2. **Permanent Chassis Interment**

Situations where an individual's consciousness is transferred to or locked within an artificial or heavily modified body.

#### Causes of Interment:

**A. Dire Injury Consequences**
```
Injury Severity Threshold:
- 80%+ body destroyed
- Fatal damage to vital organs beyond regeneration magic
- Soul-body connection severed by death magic
- Corruption/decay spreading beyond containment

Interment Decision Tree:
IF (Injury Severity >= 80%) AND (Healing Magic Fails) THEN
    IF (Soul Intact AND Transferable) THEN
        Offer Permanent Chassis
    ELSE
        Death
```

**B. Enemy Nature Forcing Irreversible Acts**

Certain enemies or environmental threats corrupt biological tissue, forcing civilizations to adopt mechanical/constructed bodies:

- **Necrophage Plagues**: Undead infection that spreads through living flesh. Solution: Transfer consciousness to metal bodies immune to undeath
- **Chaos Corruption**: Reality-warping enemies whose mere presence mutates flesh. Solution: Crystalline bodies immune to chaos
- **Soul-Devouring Demons**: Entities that consume living souls from bodies. Solution: Shielded chassis that hide soul signatures
- **Temporal Decay**: Time-manipulating foes age flesh to dust. Solution: Timeless constructs unaffected by temporal manipulation

**Example Scenario: The Brass Legion**

```
The Kingdom of Aeldrath faced extinction when the Hollow Tide—spectral undead that spread
through flesh contact—swept across their borders. Traditional defenses failed as any wound
from Hollow creatures turned soldiers into more Hollows within hours.

The Royal Academy's desperate solution: The Brass Rite.

Volunteer soldiers (and later, conscripted criminals) underwent soul extraction and transfer
into brass-and-iron constructs. These "Brass Legionnaires" were immune to Hollow infection
but could never return to flesh.

Cultural Impact:
- Veneration: Seen as ultimate sacrifice for kingdom, granted nobility titles, families
  receive pensions
- Tragedy: Many Legionnaires go mad from isolation in metal bodies, some request
  deactivation after war ends
- Controversy: Conscription of criminals seen by some as mercy, by others as cruel
  and unusual punishment
```

---

### 3. **Cultural Reactions to Interred Beings**

Different cultures view permanent augmentation through vastly different lenses:

#### Veneration Cultures

**Characteristics:**
- View interment as **sacred transformation** or **honored sacrifice**
- Interred beings receive titles, land grants, perpetual honors
- Religious ceremonies celebrate the interment process
- Families of interred gain social status

**Examples:**
- **The Forge-Blessed**: Desert culture that views metal bodies as gifts from the Smith God. Interred individuals become priests
- **The Eternal Guard**: Military order that sees permanent chassis as path to immortal service. Veterans volunteer for interment to continue protecting their nation
- **The Ascended**: Magical aristocracy that views flesh as temporary shell. Interment in crystalline bodies seen as evolution

**Cultural Mechanics:**
```
Veneration Bonus:
- Interred beings receive +20 Reputation with venerating cultures
- May serve in leadership roles (commanders, advisors, champions)
- Receive regular maintenance and upgrades from society
- Psychological stability increased by sense of purpose (+15% Madness Resistance)
```

#### Tragedy Cultures

**Characteristics:**
- View interment as **horrific loss of humanity**
- Interred beings pitied or feared, socially isolated
- Religious teachings condemn body modification
- Families of interred grieve as if for the dead

**Examples:**
- **The Purist Kingdoms**: Religious nations that believe souls belong in flesh. Interred seen as cursed
- **The Naturalists**: Druidic societies that view mechanical augmentation as abomination against nature
- **The Mortality Faithful**: Cultures that believe death should be accepted, not circumvented through artificial means

**Cultural Mechanics:**
```
Tragedy Penalty:
- Interred beings receive -30 Reputation with tragedy cultures
- Cannot enter sacred spaces or hold positions of authority
- May face exile or forced "mercy killing" by fanatics
- Psychological stress from rejection (-15% Madness Resistance)
```

#### Mixed Cultures

Most societies fall somewhere between veneration and tragedy, with different factions holding opposing views:

```
Factional Reputation System:
- Military faction: +25 (respect for sacrifice)
- Religious faction: -20 (theological concerns)
- Merchant faction: 0 (neutral, pragmatic)
- Scholarly faction: +15 (scientific interest)
- Common folk: -5 (fear of the unknown)
```

---

### 4. **Psychological Effects on Interred Individuals**

Permanent interment takes severe psychological toll over time.

#### Madness Mechanics

```csharp
public struct MadnessComponent : IComponentData
{
    public float CurrentMadness; // 0-100
    public float MadnessResistance; // Based on Willpower, cultural support
    public float TimeInChassis; // Years since interment
    public bool IsDormant; // In sleep state
    public float DormancyTimer; // Years dormant
}

public static void CalculateMadnessProgression(
    ref MadnessComponent madness,
    bool hasPurpose, // Active mission/duty
    float socialConnection, // Interaction with others
    float culturalSupport, // Veneration vs rejection
    float deltaTime)
{
    // Base madness accumulation from isolation
    float baseMadness = 0.5f * deltaTime;

    // Purpose reduces madness gain
    float purposeReduction = hasPurpose ? 0.3f : 0f;

    // Social connection helps maintain sanity
    float socialReduction = socialConnection * 0.2f;

    // Cultural context matters
    float culturalModifier = culturalSupport / 100f; // -0.3 to +0.2

    float madnessGain = baseMadness - purposeReduction - socialReduction + culturalModifier;
    madness.CurrentMadness += math.max(0, madnessGain * (1f - madness.MadnessResistance));
    madness.CurrentMadness = math.clamp(madness.CurrentMadness, 0f, 100f);

    madness.TimeInChassis += deltaTime;
}
```

#### Madness Stages

**Stage 1: Disorientation (0-20 Madness)**
- Phantom limb sensations from lost biological body
- Memory confusion about original vs current form
- Minor personality shifts
- Effects: -5% to all skills requiring fine motor control

**Stage 2: Identity Crisis (21-40 Madness)**
- Questioning whether they're still "themselves"
- Obsessive behaviors (counting, repetitive tasks)
- Emotional numbness or volatility
- Effects: -10% to social interactions, -5% combat effectiveness

**Stage 3: Dissociation (41-60 Madness)**
- Viewing original self as separate person who "died"
- Detachment from mortal concerns
- Paranoia or delusions
- Effects: -20% social interactions, +10% combat aggression, may refuse non-combat orders

**Stage 4: Fragmentation (61-80 Madness)**
- Multiple personality states
- Hearing voices of "dead self"
- Violent outbursts or catatonic episodes
- Effects: -30% to all skills, may attack allies during episodes, requires handler

**Stage 5: Complete Madness (81-100 Madness)**
- Total loss of original identity
- Feral or berserk behavior
- Must be restrained or put into dormancy
- Effects: Uncontrollable, danger to self and others

#### Dormancy Cycles

To preserve sanity, many interred individuals enter **dormancy**—a sleep state where consciousness is suspended.

```csharp
public struct DormancyComponent : IComponentData
{
    public bool CanEnterDormancy; // Requires compatible chassis
    public float DormancyDuration; // Years asleep
    public DormancyTrigger WakeTrigger; // What awakens them
    public float MadnessReductionRate; // Madness healed per year dormant
}

public enum DormancyTrigger
{
    Manual,              // Awakened by command
    WarDeclaration,      // Nation goes to war
    ThresholdThreat,     // Enemy power level exceeds threshold
    SpecificEnemy,       // Particular foe detected
    CalendarEvent,       // Specific date/holiday
    SoulCall            // Summoned by ritual
}

public static void ProcessDormancy(
    ref MadnessComponent madness,
    ref DormancyComponent dormancy,
    float yearsElapsed)
{
    if (dormancy.CanEnterDormancy)
    {
        // Dormancy slowly heals madness
        madness.CurrentMadness -= dormancy.MadnessReductionRate * yearsElapsed;
        madness.CurrentMadness = math.max(0, madness.CurrentMadness);

        dormancy.DormancyDuration += yearsElapsed;
        madness.DormancyTimer += yearsElapsed;
    }
}
```

**Dormancy Cultural Examples:**

- **The Sleepers of Korth**: Ancient warriors interred in stone bodies, dormant in crypts. Awaken only when nation invaded
- **The Tide Wardens**: Coastal defenders in coral-metal bodies, sleep beneath waves. Rise when sea monsters threaten
- **The Starwatch**: Void-chassis guardians orbiting planet, dormant for centuries. Wake when extra-dimensional threats detected

---

### 5. **Consciousness Redemption Through Transfer Technologies**

Advanced civilizations develop methods to **redeem** interred consciousness by transferring it to better vessels or even returning it to biological form.

#### Redemption Methods

**A. Chassis Upgrade Transfer**

Transfer consciousness from crude prototype chassis to refined, stable body:

```
Upgrade Transfer Success = (Soul Strength × 0.6) + (Transfer Tech Level × 5) + (Target Chassis Quality × 0.3) - (Current Madness / 2)

Target Chassis Types:
- Refined Construct: Better sensory input, reduced madness gain (-50% madness accumulation)
- Hybrid Bio-Mechanical: Partial flesh restoration, better identity retention
- Perfected Golem: Eternal body, complete sensory parity with biological form
```

**B. Cloned Body Restoration**

Growing new biological body from original genetic material and transferring soul back:

```
Clone Restoration Requirements:
- Original biological sample (hair, blood, preserved tissue)
- Advanced bio-arcana facilities
- Soul still intact and transferable
- Minimum 50,000 gold cost

Restoration Success = (Soul Integrity × 0.8) + (Clone Quality × 0.15) + (Practitioner Skill × 0.05) - (Years in Chassis × 0.5)

Effects if Successful:
- Return to biological existence
- Madness reduced by 80%
- Retain memories of chassis period
- May suffer PTSD from interment experience
```

**C. Soul Healing and Reintegration**

For those too damaged for transfer, gradual healing of fragmented consciousness:

```csharp
public struct SoulHealingComponent : IComponentData
{
    public float FragmentationLevel; // How broken the soul is
    public float HealingProgress; // 0-100%
    public int HealingSessions; // Number of treatments
    public float PractitionerSkill; // Healer's effectiveness
}

public static void ProcessSoulHealing(
    ref MadnessComponent madness,
    ref SoulHealingComponent healing,
    float practitionerSkill,
    float magicPower)
{
    // Each healing session reduces fragmentation
    float healingAmount = (practitionerSkill * 0.4f) + (magicPower * 0.3f);
    healing.FragmentationLevel -= healingAmount;
    healing.FragmentationLevel = math.max(0, healing.FragmentationLevel);

    // As soul heals, madness reduces
    madness.CurrentMadness *= (healing.FragmentationLevel / 100f);

    healing.HealingSessions++;
    healing.HealingProgress = (1f - healing.FragmentationLevel / 100f) * 100f;
}
```

**D. Collective Consciousness Merger**

For those who cannot be restored individually, merge with hive mind or group consciousness:

```
Merger Process:
- Individual consciousness too fragmented for solo existence
- Merge with compatible collective (military unit hive-mind, religious communion)
- Retain echo of individual identity within collective
- Madness becomes shared burden across group
- Individual "dies" but aspects persist in collective memory

Cultural Views:
- Veneration Cultures: Beautiful apotheosis, becoming part of something greater
- Tragedy Cultures: Final death of individual, horrifying erasure
- Pragmatic Cultures: Acceptable solution to unsolvable problem
```

---

## Integration with Existing Systems

### Connection to Soul System

Permanent augmentation relies on **soul extraction and transfer** mechanics:

```csharp
public struct PermanentAugmentTransfer
{
    public Entity OriginalBody; // Biological body (may be destroyed)
    public Entity TargetChassis; // Permanent augment body
    public SoulComponent Soul; // Soul being transferred
    public float TransferProgress; // Procedure progress
    public bool IsReversible; // Almost always false for permanent augments
}

// From Soul_System.md - Reusing soul transfer mechanics
public static bool AttemptPermanentInterment(
    SoulComponent soul,
    Entity targetChassis,
    float practitionerSkill,
    out float finalIntegrity)
{
    // Permanent interment is high-risk soul transfer
    float compatibility = CalculateVesselCompatibility(soul, targetChassis);

    // Permanent chassis typically has lower compatibility than biological body
    compatibility *= 0.6f; // 40% penalty for artificial body

    float transferSuccess = (soul.Strength × 0.4f) + (compatibility × 0.3f) + (practitionerSkill × 0.3f);

    bool success = UnityEngine.Random.Range(0f, 100f) < transferSuccess;

    if (success)
    {
        // Soul integrity damaged by forced transfer
        finalIntegrity = soul.Integrity * 0.85f; // 15% integrity loss
        return true;
    }
    else
    {
        // Failed transfer damages soul severely
        finalIntegrity = soul.Integrity * 0.5f;
        return false;
    }
}
```

### Connection to Grafting System

Early-stage grafts can become permanent augments if integrated too deeply:

```
Graft → Permanent Augment Progression:
1. Removable Graft: Can be detached with minor surgery
2. Integrated Graft: Requires major surgery to remove
3. Fused Graft: Removal would cause death, but consciousness still in original body
4. Permanent Augment: Consciousness partially or fully dependent on augment to exist

Transition Triggers:
- Graft rejection forcing deeper integration
- Deliberate enhancement pushing graft deeper
- Combat damage requiring augment to preserve life
- Voluntary choice to maximize augment power
```

---

## Narrative Examples

### Example 1: The Volunteer

**Background**: Marcus, veteran knight, age 45, dying from corrupted wound
**Choice**: Death in 3 days, or interment in experimental golem chassis
**Outcome**: Accepts interment, becomes first of "Iron Paladins"
**Cultural Reaction**: Hailed as hero, bronze statue erected, family receives pension
**Psychological Arc**:
- Year 1: Struggles with phantom pain, questions identity (15 Madness)
- Year 5: Finds purpose training next generation (10 Madness - reduced by purpose)
- Year 20: Begins dissociating from human concerns (35 Madness)
- Year 50: Enters dormancy voluntarily, awakens only for major wars (50 Madness stable)
- Year 200: Consciousness upgraded to refined chassis, madness reduced to 20
**Legacy**: Founder of honored order, inspires thousands to volunteer for service

---

### Example 2: The Conscript

**Background**: Elara, convicted thief, age 22, sentenced to death
**Choice**: Execution, or serve as test subject for chimeric graft prototype
**Outcome**: Survives procedure, gains beast limbs but suffers personality fragmentation
**Cultural Reaction**: Seen as abomination by religious faction, pitied by commons, valued by military
**Psychological Arc**:
- Month 1: Severe disorientation, animalistic urges (25 Madness)
- Year 1: Personality split between human and beast aspects (45 Madness)
- Year 3: Beast personality dominant, reassigned to frontline shock trooper (65 Madness)
- Year 5: Goes berserk during battle, kills allies, must be put in dormancy (85 Madness)
- Year 10: Awakened for critical siege, performs mission successfully, returns to dormancy
**Legacy**: Cautionary tale about dangers of forced augmentation, used in ethical debates

---

### Example 3: The Enemy-Forced

**Background**: Entire border town of Millhaven, 2,000 civilians, exposed to Necrophage Plague
**Choice**: Die and rise as undead within hours, or emergency mass interment in brass bodies
**Outcome**: 1,400 accept interment, 600 choose death, town becomes "Brasshaven"
**Cultural Reaction**:
- Other nations horrified at mass transformation
- Kingdom grants Brasshaven autonomy as independent city-state
- Religious schism over whether interred citizens still have souls
**Psychological Arc**:
- Year 1: Mass grief, 40% average madness across population
- Year 10: Community identity forms around shared trauma, madness stabilizes at 30%
- Year 50: "First Generation" interred begin entering dormancy, younger constructs take over governance
- Year 100: Brasshaven becomes major military power, feared and respected
- Year 200: Soul healing technologies allow some First Generation to transfer to cloned bodies
**Legacy**: Brasshaven becomes center of augmentation research, splits opinion across continent

---

## Gameplay Integration

### Player Choices

**As Player Ruler:**
- Decide whether to fund augmentation research (ethical vs military necessity)
- Set policies on voluntary vs conscripted test subjects
- Choose cultural attitude: Veneration, tragedy, or pragmatism
- Manage interred veterans (assign missions, approve dormancy requests, fund redemption)

**As Player Character:**
- Accept or refuse interment if fatally wounded
- Choose to volunteer for experimental augment
- Interact with interred beings (recruit, befriend, fear)
- Seek redemption technologies for self or companions

### Strategic Implications

**Military:**
- Interred units are immortal (don't die of age) but require maintenance
- High madness units powerful but unpredictable
- Dormancy allows "sleeping armies" to be awakened in crisis
- Cultural reaction affects diplomatic relations

**Economic:**
- Augmentation research expensive but creates powerful units
- Redemption technologies late-game gold sink
- Maintenance costs for permanent chassis ongoing expense

**Diplomatic:**
- Other nations may embargo you for unethical augmentation
- Veneration cultures offer alliances
- Tragedy cultures may declare holy war against "abominations"

---

## Summary

Permanent augmentation represents the **dark price of technological progress**: beings willing to sacrifice their humanity for power, survival, or duty. The system creates tragic heroes, cautionary tales, and philosophical questions about identity, consciousness, and the soul.

Key mechanical pillars:
1. **Irreversibility**: Permanent commitment with high stakes
2. **Psychological Cost**: Madness mechanics create ticking clock on mental stability
3. **Cultural Context**: Society's reaction shapes interred experience
4. **Redemption Arc**: Late-game path to reclaim lost humanity
5. **Narrative Depth**: Every interred being has a story of sacrifice, horror, or heroism

This system integrates with Soul mechanics (consciousness transfer), Grafting (augmentation progression), and cultural systems (veneration vs tragedy) to create rich emergent narratives about the cost of power and the nature of humanity.
