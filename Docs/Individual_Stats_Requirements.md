# Individual Stats & Resistance Requirements

## Summary of Required Stat Categories

### Core Attributes (Experience Modifiers)
- **Physique** (0-100): Physical power, muscle, endurance. Modifies Strength experience gain.
- **Finesse** (0-100): Skill, speed, agility, precision. Modifies Finesse experience gain.
- **Will** (0-100): Mental fortitude, courage, determination. Modifies Will experience gain.
- **Wisdom** (0-100): Accumulates and generates general experience. Higher wisdom = faster overall progression.

### Derived Attributes
- **Strength** (0-100): Physical power (derived from Physique + experience)
- **Agility** (0-100): Speed and dexterity (derived from Finesse + experience)
- **Intelligence** (0-100): Mental acuity (derived from Will + experience, affects magic)

### Social Stats
- **Fame** (0-1000): Public recognition, legendary status threshold at 500+
- **Wealth** (currency): Liquid wealth + asset value
- **Reputation** (-100 to +100): Standing in community
- **Glory** (0-1000): Combat achievements, heroic deeds
- **Renown** (0-1000): Overall legendary status (combines fame + glory)

### Combat Stats (Derived from Base Attributes)
- **Attack** (0-100): To-hit chance (Finesse × 0.7 + Strength × 0.3)
- **Defense** (0-100): Dodge/block chance (Finesse × 0.6 + armor)
- **Health** (HP): Max HP (Strength × 0.6 + Will × 0.4 + 50)
- **Stamina** (0-100): Rounds before exhaustion (Strength / 10)
- **Mana** (0-100): Max mana for magic users (Will × 0.5 + Intelligence × 0.5)

### Need Stats
- **Food** (0-100): Hunger level, 0 = starving
- **Rest** (0-100): Fatigue level, 0 = exhausted
- **Sleep** (0-100): Sleep need, 0 = sleep-deprived
- **General Health** (0-100): Overall health status (separate from combat HP)

### Resistances (Damage Type Modifiers)
- **Physical Resistance** (0-100%): Reduces physical damage
- **Fire Resistance** (0-100%): Reduces fire damage
- **Cold Resistance** (0-100%): Reduces cold damage
- **Poison Resistance** (0-100%): Reduces poison damage
- **Magic Resistance** (0-100%): Reduces magic damage
- **Lightning Resistance** (0-100%): Reduces lightning damage
- **Holy Resistance** (0-100%): Reduces holy damage
- **Dark Resistance** (0-100%): Reduces dark damage

### Healing & Spell Modifiers
- **Heal Bonus** (multiplier): Multiplies healing received (e.g., 1.2 = +20% healing)
- **Spell Duration Modifier** (multiplier): Modifies duration of own spells (e.g., 1.5 = +50% duration)
- **Spell Intensity Modifier** (multiplier): Modifies intensity/damage of own spells (e.g., 1.3 = +30% damage)

### Limb System (Rimworld-style)
- **Limbs**: List of limb references (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, etc.)
- **Limb Health** (0-100%): Per-limb health status
- **Limb Injuries**: Permanent injuries (LostEye, CrippledArm, etc.)
- **Implants**: List of implant references (prosthetics, enhancements)

### Outlooks & Alignments
- **Alignment**: Moral/ideological position (Good/Evil, Lawful/Chaotic, Pure/Corrupt)
- **Outlooks**: Cultural/behavioral expressions (Materialistic, Spiritual, Warlike, Peaceful, etc.)
- **Disposition**: Stance toward external forces (Loyalty, Fear, Love, Trust, Respect)

## Data Model Structure

### IndividualTemplate Extensions
```csharp
public class IndividualTemplate : PrefabTemplate
{
    // Core Attributes (Experience Modifiers)
    public float physique = 50f;        // 0-100
    public float finesse = 50f;        // 0-100
    public float will = 50f;            // 0-100
    public float wisdom = 50f;         // 0-100
    
    // Derived Attributes
    public float strength = 50f;       // 0-100
    public float agility = 50f;       // 0-100
    public float intelligence = 50f;   // 0-100
    
    // Social Stats
    public float fame = 0f;            // 0-1000
    public float wealth = 0f;         // Currency
    public float reputation = 0f;      // -100 to +100
    public float glory = 0f;          // 0-1000
    public float renown = 0f;         // 0-1000
    
    // Combat Stats (Base values, will be calculated from attributes)
    public float baseAttack = 0f;     // 0-100
    public float baseDefense = 0f;    // 0-100
    public float baseHealth = 100f;   // HP
    public float baseStamina = 10f;   // Rounds
    public float baseMana = 0f;       // 0-100
    
    // Need Stats
    public float food = 100f;          // 0-100
    public float rest = 100f;         // 0-100
    public float sleep = 100f;        // 0-100
    public float generalHealth = 100f; // 0-100
    
    // Resistances (0-100% reduction)
    public Dictionary<string, float> resistances = new Dictionary<string, float>();
    
    // Healing & Spell Modifiers (multipliers)
    public float healBonus = 1.0f;              // Default 1.0 = no bonus
    public float spellDurationModifier = 1.0f;  // Default 1.0 = no modifier
    public float spellIntensityModifier = 1.0f; // Default 1.0 = no modifier
    
    // Limb System
    public List<LimbReference> limbs = new List<LimbReference>();
    public List<ImplantReference> implants = new List<ImplantReference>();
    
    // Outlooks & Alignments (references to alignment system)
    public string alignmentId = "";              // Reference to alignment definition
    public List<string> outlookIds = new List<string>(); // References to outlook definitions
    public Dictionary<string, float> disposition = new Dictionary<string, float>(); // Loyalty, Fear, etc.
}
```

## Implementation Notes

1. **Resistances Dictionary**: Keys should match damage types from combat system (e.g., "Physical", "Fire", "Cold", "Poison", "Magic", "Lightning", "Holy", "Dark").

2. **Limb References**: Will reference a limb system (to be implemented separately). For now, use string IDs or simple structs.

3. **Implant References**: Similar to limbs, will reference an implant/prosthetic system.

4. **Alignment/Outlook References**: String IDs that reference alignment/outlook definitions (see `Docs/Outlooks_Alignments_Summary_For_Advisor.md`).

5. **Stat Calculation**: Most combat stats are derived from base attributes. The template provides base values that can be overridden, but runtime systems will calculate final values.

6. **Need Stats**: These are runtime values that change over time. Templates provide starting values.

## Validation Rules

- All stats should be within valid ranges (0-100, 0-1000, etc.)
- Resistances should be 0-100% (0.0 to 1.0 as float)
- Modifiers should be positive (typically 0.5 to 2.0 range)
- Social stats can be negative (reputation: -100 to +100)
- Combat stats should be calculated from attributes unless explicitly overridden

