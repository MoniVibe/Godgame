# Stealth & Perception System

## Overview

The Stealth & Perception System governs how entities hide, detect hidden entities, spot traps, commune with ghosts, and see through magical illusions. This system is critical for spies, assassins, thieves, and any entity attempting covert operations.

## Core Mechanics

### Base Attributes

**From Base Attributes (Experience Modifiers):**
- **Finesse (0-100)**: Primary modifier for stealth effectiveness
- **Intelligence (0-100)**: Primary modifier for perception checks and misdirection
- **Will (0-100)**: Secondary modifier for resisting detection under pressure

### Derived Stats

**Stealth (Hiding Ability):**
```
Stealth = (Finesse × 0.7) + (Intelligence × 0.3) + EquipmentBonus
```

**Perception (Detection Ability):**
```
Perception = (Intelligence × 0.6) + (Finesse × 0.3) + (Will × 0.1) + EquipmentBonus
```

**Misdirection (Deception Ability):**
```
Misdirection = (Intelligence × 0.8) + (Finesse × 0.2) + PersonalityModifier
```

---

## Stealth Mechanics

### Stealth States

Entities can enter stealth mode, reducing perception checks against them:

**Stealth Levels:**
1. **Exposed** (0% stealth bonus): Fully visible, no hiding attempt
2. **Concealed** (25% stealth bonus): Hiding behind cover, blending in crowds
3. **Hidden** (50% stealth bonus): Actively sneaking, using shadows
4. **Invisible** (75% stealth bonus): Magical invisibility, perfect darkness

### Environmental Modifiers

**Light Conditions:**
```
Broad Daylight:     -30% Stealth (harder to hide)
Overcast/Cloudy:    -10% Stealth
Dusk/Dawn:          +0% Stealth (neutral)
Moonlight:          +15% Stealth
Pitch Black:        +30% Stealth (easier to hide)
```

**Terrain Modifiers:**
```
Open Field:         -20% Stealth
Urban Street:       +0% Stealth
Crowded Market:     +15% Stealth (blend in)
Forest/Woods:       +20% Stealth
Dense Fog:          +25% Stealth
Underground:        +10% Stealth
```

**Movement Speed vs. Stealth:**
```
Stationary:         +10% Stealth
Sneaking (slow):    +0% Stealth
Walking:            -15% Stealth
Running:            -40% Stealth (hard to stay hidden)
```

### Stealth Check Formula

When a stealthed entity attempts to avoid detection:

```
StealthCheck = d100 + EntityStealth + EnvironmentalModifiers + EquipmentBonus

vs

PerceptionCheck = d100 + GuardPerception + AlertnessModifier
```

**If StealthCheck > PerceptionCheck:**
- Entity remains undetected

**If PerceptionCheck > StealthCheck (Minor Fail):**
- Guard becomes suspicious (+20% alertness for 10 rounds)
- May investigate the area

**If PerceptionCheck > StealthCheck + 30 (Major Fail):**
- Entity spotted, identity may be revealed
- Finesse check to escape recognition

**If PerceptionCheck > StealthCheck + 60 (Critical Fail):**
- Entity fully exposed, identity revealed
- Motives may be discovered (Intelligence check)

---

## Perception Mechanics

### Perception Targets

Entities make perception checks to detect:

1. **Hidden Entities** (spies, assassins, thieves)
2. **Traps** (mechanical, magical)
3. **Hidden Treasures** (secret rooms, buried gold)
4. **Ghosts & Spirits** (requires special vision)
5. **Magical Illusions** (fake walls, disguised entities)
6. **Invisible Entities** (magical invisibility)

### Perception Check Types

**Passive Perception:**
```
Automatic checks every round for entities on guard duty
PassivePerception = EntityPerception + AlertnessModifier + RoleBonus

RoleBonus:
  Champion/Bodyguard:  +15 (trained to watch for threats)
  Peacekeeper/Guard:   +10 (trained observers)
  Common Villager:     +0
  Distracted/Drunk:    -20
```

**Active Perception (Scrutiny):**
```
Entity actively searching for hidden things
ActivePerception = EntityPerception + d20 + FocusBonus

FocusBonus:
  Actively searching:  +20
  Following orders:    +10
  Casual observation:  +0
```

**Opposed Checks (Stealth vs. Perception):**
```
When stealthed entity enters guard's line of sight:

GuardRoll = d100 + GuardPerception + AlertnessModifier + EnvironmentModifier
vs
EntityRoll = d100 + EntityStealth + EnvironmentModifier + EquipmentBonus

If GuardRoll > EntityRoll:
  → Detection (level depends on margin)
```

### Scrutiny Levels

Guards and watchful entities have varying alertness:

```
Relaxed (0 alertness):
  - No recent threats
  - Perception checks at -10%

Alert (20 alertness):
  - Suspicious activity reported
  - Perception checks at +0%

Heightened (50 alertness):
  - Recent crime or assassination attempt
  - Perception checks at +20%
  - Active patrols every 5 rounds

Lockdown (80+ alertness):
  - Active manhunt
  - Perception checks at +40%
  - All entrances guarded
  - Constant patrols
```

---

## Misdirection & Framing

Agents can perform actions while disguised as third parties, creating enmity between factions.

### Misdirection Check

```
MisdirectionCheck = d100 + EntityMisdirection + DisguiseQuality

vs

SuspicionCheck = d100 + TargetIntelligence + PriorRelationshipModifier
```

**Success Levels:**

**Critical Success (MisdirectionCheck > SuspicionCheck + 40):**
- Target fully believes the framing
- Enmity created with framed faction (-30 to -60 relationship)
- No suspicion toward the real perpetrator

**Success (MisdirectionCheck > SuspicionCheck):**
- Target suspects the framed party
- Enmity created with framed faction (-15 to -30 relationship)
- Minor doubt about the story

**Failure (SuspicionCheck > MisdirectionCheck):**
- Target doubts the story, no enmity created
- Agent's true identity may be investigated
- Finesse check to escape suspicion

**Critical Failure (SuspicionCheck > MisdirectionCheck + 40):**
- Agent's identity revealed
- Motives exposed
- Enmity created with the target instead (-40 to -80 relationship)
- Target may alert authorities or retaliate

### Disguise Quality

```
Poor Disguise:        +0 to Misdirection (clothing doesn't match)
Basic Disguise:       +10 to Misdirection (matching uniform/colors)
Expert Disguise:      +25 to Misdirection (forged documents, perfect outfit)
Magical Disguise:     +40 to Misdirection (illusion magic, shapeshift)
```

### Framing Tactics

**Plant Evidence:**
- Leave faction symbols, weapons, or documents
- Intelligence check vs. target's perception
- +20 to misdirection if evidence is convincing

**Impersonate:**
- Wear faction colors/uniform
- Speak with faction dialect (Intelligence check)
- +15 to misdirection if impersonation is believable

**False Witness:**
- Bribe or coerce witnesses to testify
- Misdirection check vs. target's network intelligence
- +10 to misdirection per corrupted witness

---

## Special Vision & Detection

### Ghostly Vision

Entities with special training or magic can see and commune with ghosts.

**Ghost Sight Requirements:**
```
Natural Gift (rare trait):  Automatic ghost vision
Magical Training:           Intelligence 60+, trained in necromancy
Holy Blessing:              Priest with 50+ piety
Cursed/Haunted:             Entity with traumatic death experience
```

**Communing with Ghosts:**
```
CommuneCheck = d100 + EntityIntelligence + MagicPower

vs

GhostResistance = d100 + GhostWill + TimeDeceased

Success: Ghost shares knowledge of their death, hidden treasures, secrets
Failure: Ghost remains silent or hostile
Critical Failure: Ghost attacks (drains morale/will)
```

**Knowledge from Ghosts:**
- Location of hidden treasures
- Identity of their murderer
- Secrets they learned in life
- Location of traps or hidden passages

### Magical Sight (Detect Illusions)

Mages and trained entities can detect magical illusions.

**Detect Illusion Check:**
```
DetectCheck = d100 + EntityIntelligence + MagicPower + Perception

vs

IllusionStrength = d100 + CasterSpellPower + IllusionComplexity

Success: Illusion revealed (fake wall, disguised entity, phantom treasure)
Failure: Illusion persists
Critical Success: Illusion dispelled
Critical Failure: Entity becomes confused (-20% perception for 10 rounds)
```

**Common Illusions:**
```
Fake Wall:           Hides secret passage (IllusionStrength = 40)
Disguised Entity:    Mage appears as commoner (IllusionStrength = 50)
Phantom Treasure:    Fake gold to lure victims (IllusionStrength = 30)
Invisible Entity:    Complete invisibility (IllusionStrength = 70)
```

---

## Trap Detection & Disarming

Perception checks allow entities to spot traps before triggering them.

### Trap Detection

```
TrapDetectionCheck = d100 + EntityPerception + Finesse

vs

TrapConcealment = d100 + TrapComplexity + EnvironmentBonus

Success: Trap spotted before triggering
Failure: Trap unnoticed
Trigger: Entity steps on trap (damage or alarm)
```

**Trap Types:**
```
Mechanical Trap (arrow, spike pit):
  Concealment: 40
  Damage: 20-50 HP
  Disarm Check: Finesse 50+

Alarm Trap (tripwire, pressure plate):
  Concealment: 30
  Effect: Alerts guards (+50 alertness)
  Disarm Check: Finesse 40+

Magical Trap (rune, glyph):
  Concealment: 60
  Damage: 30-70 HP (fire, lightning)
  Disarm Check: Intelligence 60+ OR MagicPower 50+
```

### Disarming Traps

```
DisarmCheck = d100 + EntityFinesse (mechanical) OR Intelligence (magical)

vs

TrapComplexity = d100 + TrapDifficulty

Success: Trap disarmed, safe passage
Failure: Trap remains armed
Critical Failure: Trap triggers immediately
```

---

## Identity Revelation & Consequences

When stealth fails, the entity may be recognized.

### Identity Check (When Spotted)

```
IF StealthCheck fails by 30+:
  → IdentityCheck = d100 + EntityFinesse + DisguiseQuality

  vs

  RecognitionCheck = d100 + GuardIntelligence + FamiliarityBonus

FamiliarityBonus:
  Never seen before:       +0
  Seen once or twice:      +10
  Known criminal:          +30
  Personal enemy:          +50
```

**If RecognitionCheck > IdentityCheck:**
- Entity's identity revealed
- Guards alert authorities
- Enmity created with faction (-20 to -60 relationship)
- Bounty may be placed on entity

**If RecognitionCheck > IdentityCheck + 40 (Critical):**
- Motives exposed (guard understands why entity is sneaking)
- Immediate combat or arrest attempt
- All guards in area alerted
- Escape difficulty increased (+30% to all escape checks)

---

## Equipment & Stealth Bonuses

### Stealth Equipment

```
Dark Clothing:          +5% Stealth (reduces visibility)
Leather Boots:          +3% Stealth (quiet movement)
Cloak of Shadows:       +15% Stealth (magical concealment)
Invisibility Potion:    +50% Stealth (temporary, 10 rounds)
Smoke Bomb:             +20% Stealth (escape tool, 3 rounds)
```

### Perception Equipment

```
Spyglass:               +10% Perception (long distance)
Enchanted Lens:         +15% Perception, detects magic
Lantern:                +5% Perception, -10% Stealth (light source)
Owl Familiar:           +20% Perception (scouting)
Detect Magic Amulet:    +25% vs. illusions
```

---

## DOTS Components (Unity ECS)

### PerceptionStats Component

```csharp
public struct PerceptionStats : IComponentData
{
    // Derived stats
    public byte Stealth;                    // 0-100 (hiding ability)
    public byte Perception;                 // 0-100 (detection ability)
    public byte Misdirection;               // 0-100 (deception ability)

    // State
    public StealthLevel CurrentStealthLevel; // Exposed, Concealed, Hidden, Invisible
    public byte Alertness;                  // 0-100 (guard awareness)
    public bool IsStealthed;                // Currently hiding
    public bool IsScrutinizing;             // Actively searching

    // Vision abilities
    public bool HasGhostSight;              // Can see ghosts
    public bool HasMagicSight;              // Can detect illusions
    public bool HasDarkVision;              // See in darkness

    // Equipment
    public Entity StealthEquipment;         // Cloak, boots, etc.
    public Entity PerceptionEquipment;      // Spyglass, amulet, etc.
}

public enum StealthLevel : byte
{
    Exposed = 0,        // 0% bonus
    Concealed = 1,      // 25% bonus
    Hidden = 2,         // 50% bonus
    Invisible = 3       // 75% bonus
}
```

### EnvironmentalModifiers Component

```csharp
public struct EnvironmentalModifiers : IComponentData
{
    public sbyte LightModifier;             // -30 to +30 (darkness helps stealth)
    public sbyte TerrainModifier;           // -20 to +25 (forest, fog, etc.)
    public sbyte MovementPenalty;           // -40 to +10 (running vs. sneaking)
    public byte AreaAlertness;              // 0-100 (lockdown, heightened, etc.)
}
```

### DetectionEvent Component (Tag)

```csharp
public struct DetectionEvent : IComponentData
{
    public Entity DetectedEntity;           // Who was spotted
    public Entity DetectorEntity;           // Who spotted them
    public DetectionLevel Level;            // Suspicious, Spotted, Identified
    public bool MotivesRevealed;            // Critical fail
    public float Timestamp;                 // When detected
}

public enum DetectionLevel : byte
{
    Suspicious = 0,     // Minor fail (+20% alertness)
    Spotted = 1,        // Major fail (identity check)
    Identified = 2,     // Identity revealed
    Exposed = 3         // Identity + motives revealed
}
```

---

## Integration with Existing Systems

### Assassination (Individual Combat System)
- Assassins use Stealth to approach targets undetected
- Successful stealth allows first strike bonus (+50% damage)
- Failed stealth alerts target and bodyguards

### Spies & Double Agents
- Spies use Misdirection to frame third parties
- Perception checks to spot enemy spies
- Ghost Sight to extract secrets from murdered victims

### Elite Courts & Retinues
- Champions and bodyguards have +15 Perception bonus
- Court intrigue uses Misdirection to plant false evidence
- Retinues may include scouts with enhanced perception

### Entity Relations
- Detection penalties: Spotted spy = -40 relationship
- Critical failure (motives exposed) = -60 to -100 relationship
- Successful framing creates enmity with framed faction

---

## Behavioral Modifiers (Personality & Alignment)

### Stealth Preference by Personality

```
Deceitful (+60+):       +15% Misdirection (natural liar)
Honest (+60+):          -15% Misdirection (poor liar)

Bold (+60+):            -10% Stealth (prefers direct action)
Craven (+60+):          +10% Stealth (prefers hiding)

Paranoid (+60+):        +15% Perception (always watching)
Trusting (+60+):        -10% Perception (assumes best)
```

### Alignment Impact on Detection

```
Good Entities:
  - Less likely to assassinate (stealth for escape)
  - More likely to turn self in if exposed

Evil Entities:
  - Use stealth for assassination and theft
  - Will kill witnesses if detected

Lawful Entities:
  - Respect guard authority (less stealth in legal areas)
  - Higher penalty if caught (-40% reputation)

Chaotic Entities:
  - Ignore authority (stealth everywhere)
  - Lower penalty if caught (-20% reputation)
```

---

## Summary

The Stealth & Perception System creates dynamic cat-and-mouse gameplay where:

- **Finesse and Intelligence** determine stealth and detection ability
- **Environmental modifiers** (light, terrain, movement) affect hiding success
- **Guards and champions** have varying alertness levels
- **Misdirection** allows framing third parties for enmity creation
- **Special vision** enables ghost communion and illusion detection
- **Detection consequences** scale from suspicion to identity exposure
- **Equipment and personality** modify stealth/perception effectiveness

This system integrates with combat (assassination), espionage (spies), and diplomacy (framing) to create rich stealth gameplay.
