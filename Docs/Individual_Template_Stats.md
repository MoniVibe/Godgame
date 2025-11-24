# Individual Template Stats Schema

## Overview

The `IndividualTemplate` class has been extended with comprehensive stat systems covering core attributes, derived attributes, social stats, combat stats, need stats, resistances, healing/spell modifiers, limb system, and outlooks/alignments.

## Stat Categories

### Core Attributes (Experience Modifiers)

These attributes modify experience gain in their respective pools:

- **Physique** (0-100): Physical power, muscle, endurance. Modifies Strength experience gain.
- **Finesse** (0-100): Skill, speed, agility, precision. Modifies Finesse experience gain.
- **Will** (0-100): Mental fortitude, courage, determination. Modifies Will experience gain.
- **Wisdom** (0-100): Accumulates and generates general experience. Higher wisdom = faster overall progression.

### Derived Attributes

These attributes are derived from core attributes + experience:

- **Strength** (0-100): Physical power (derived from Physique + experience)
- **Agility** (0-100): Speed and dexterity (derived from Finesse + experience)
- **Intelligence** (0-100): Mental acuity (derived from Will + experience, affects magic)

### Social Stats

- **Fame** (0-1000): Public recognition, legendary status threshold at 500+
- **Wealth** (currency): Liquid wealth + asset value
- **Reputation** (-100 to +100): Standing in community
- **Glory** (0-1000): Combat achievements, heroic deeds
- **Renown** (0-1000): Overall legendary status (combines fame + glory)

### Combat Stats (Base Values)

These are base values that can be overridden in templates. Set to 0 to auto-calculate from attributes at runtime:

- **Base Attack** (0-100, 0=auto): To-hit chance (calculated: Finesse × 0.7 + Strength × 0.3)
- **Base Defense** (0-100, 0=auto): Dodge/block chance (calculated: Finesse × 0.6 + armor)
- **Base Health Override** (>=0, 0=use baseHealth): Max HP override (calculated: Strength × 0.6 + Will × 0.4 + 50)
- **Base Stamina** (>=0, 0=auto): Rounds before exhaustion (calculated: Strength / 10)
- **Base Mana** (0-100, 0=auto or non-magic): Max mana for magic users (calculated: Will × 0.5 + Intelligence × 0.5)

### Need Stats (Starting Values)

Runtime values that change over time. Templates provide starting values:

- **Food** (0-100): Hunger level, 0 = starving
- **Rest** (0-100): Fatigue level, 0 = exhausted
- **Sleep** (0-100): Sleep need, 0 = sleep-deprived
- **General Health** (0-100): Overall health status (separate from combat HP)

### Resistances (Damage Type Modifiers)

Dictionary of damage type resistances (0-100% reduction):

- Keys: "Physical", "Fire", "Cold", "Poison", "Magic", "Lightning", "Holy", "Dark"
- Values: 0.0-1.0 (0.0 = no resistance, 1.0 = 100% immunity)

### Healing & Spell Modifiers

Multipliers that modify healing received and spell effects:

- **Heal Bonus** (multiplier, default 1.0): Multiplies healing received (e.g., 1.2 = +20% healing)
- **Spell Duration Modifier** (multiplier, default 1.0): Modifies duration of own spells (e.g., 1.5 = +50% duration)
- **Spell Intensity Modifier** (multiplier, default 1.0): Modifies intensity/damage of own spells (e.g., 1.3 = +30% damage)

### Limb System (Rimworld-style)

List of `LimbReference` objects:

- **Limb ID**: Reference to limb definition (e.g., "Head", "LeftArm", "RightLeg")
- **Health** (0-100%): Per-limb health status
- **Injuries**: List of permanent injuries (e.g., "LostEye", "CrippledArm")

### Implants

List of `ImplantReference` objects:

- **Implant ID**: Reference to implant definition
- **Attached To Limb**: Which limb this implant is attached to (empty = body)
- **Stat Modifiers**: Dictionary of stat bonuses from implant

### Outlooks & Alignments

- **Alignment ID**: Reference to alignment definition (Moral/ideological position)
- **Outlook IDs**: List of references to outlook definitions (Cultural/behavioral expressions)
- **Disposition**: Dictionary of disposition toward external forces:
  - Keys: "Loyalty", "Fear", "Love", "Trust", "Respect"
  - Values: -100 to +100

## Usage Guidelines

### For Designers

1. **Core Attributes**: Set these to define the individual's potential. Higher values = faster progression in that area.
2. **Derived Attributes**: Can be set directly or left to auto-calculate from core attributes + experience at runtime.
3. **Combat Stats**: Set to 0 to auto-calculate from attributes, or override for specific templates (e.g., legendary warriors with fixed high stats).
4. **Resistances**: Use sparingly. Most individuals should have 0% resistance to most damage types. Special individuals (e.g., fire-resistant creatures) can have higher values.
5. **Healing/Spell Modifiers**: Use to create specialists (e.g., healers with +50% heal bonus, mages with +30% spell intensity).
6. **Limb System**: For now, use simple string IDs. Full limb system implementation will come later.
7. **Outlooks/Alignments**: Reference alignment/outlook definitions by ID. These will be validated against the alignment system.

### For Implementers

1. **Stat Calculation**: Most combat stats are derived from base attributes. The template provides base values that can be overridden, but runtime systems should calculate final values.
2. **Resistances Dictionary**: Serialize as key-value pairs. For JSON compatibility, consider serializing as separate lists (keys and values).
3. **Limb References**: Will reference a limb system (to be implemented separately). For now, use string IDs or simple structs.
4. **Implant References**: Similar to limbs, will reference an implant/prosthetic system.
5. **Alignment/Outlook References**: String IDs that reference alignment/outlook definitions. Validate these exist in the alignment system.

### Validation Rules

- All stats should be within valid ranges (0-100, 0-1000, etc.)
- Resistances should be 0-100% (0.0 to 1.0 as float)
- Modifiers should be positive (typically 0.5 to 2.0 range)
- Social stats can be negative (reputation: -100 to +100)
- Combat stats should be calculated from attributes unless explicitly overridden
- Limb health should be 0-100%
- Disposition values should be -100 to +100

## Runtime Integration

### Stat Calculation Example

```csharp
// Calculate Attack from attributes
float attack = template.baseAttack;
if (attack <= 0f)
{
    attack = (template.finesse * 0.7f) + (template.strength * 0.3f);
}

// Calculate Health from attributes
float health = template.baseHealthOverride;
if (health <= 0f)
{
    health = template.baseHealth;
    if (health <= 0f)
    {
        health = (template.strength * 0.6f) + (template.will * 0.4f) + 50f;
    }
}

// Apply resistance to damage
float damageReceived = rawDamage * (1.0f - resistance);
```

### DTO Conversion

When converting `IndividualTemplate` to `IndividualDefinitionDTO`:

1. Copy all stat values directly
2. Convert `resistances` dictionary to separate lists (`ResistanceTypes` and `ResistanceValues`)
3. Convert `disposition` dictionary to separate lists (`DispositionTypes` and `DispositionValues`)
4. Convert `limbs` list to `LimbIds` and `LimbHealths` lists
5. Convert `implants` list to `ImplantIds` list

## Related Documentation

- **Individual Progression System**: `Docs/Concepts/Villagers/Individual_Progression_System.md`
- **Individual Combat System**: `Docs/Concepts/Combat/Individual_Combat_System.md`
- **Wealth & Social Dynamics**: `Docs/Concepts/Villagers/Wealth_And_Social_Dynamics.md`
- **Outlooks & Alignments**: `Docs/Outlooks_Alignments_Summary_For_Advisor.md`
- **Individual Stats Requirements**: `Docs/Individual_Stats_Requirements.md`

