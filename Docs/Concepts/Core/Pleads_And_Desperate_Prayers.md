# Pleads and Desperate Prayers

**Status:** Draft  
**Category:** Core - Divine Intervention & Conversion  
**Scope:** Individual → Conversion  
**Created:** 2025-12-21  
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Enable entities in dire circumstances to actively petition the player god for aid, offering increasing amounts of mana in exchange for answered prayers, which slightly converts them to the player's belief.

**Secondary Goals:**
- Create emergent moments of desperation where entities reach out to the player
- Provide conversion mechanics that feel organic (not forced)
- Generate prayer power/mana from non-believers through desperation
- Create player dilemmas (answer expensive plea vs ignore vs use for conversion)

---

## Core Concept

When entities face life-threatening situations (health critical, starvation, natural disasters, combat defeat), they may send **desperate prayers** to the player god. However, **not all entities can plead** - they must have sufficient faith/belief in the player god to even attempt to issue a prayer.

These prayers:

1. **Require faith threshold** - Entities must have minimum faith/belief in player god to plead
2. **Offer increasing mana** - Each unanswered plea increases the offered mana amount (with diminishing returns for repeat pleaders)
3. **Request specific aid** - "Save me from this fire", "Heal my wounds", "Stop the bandits"
4. **Convert belief on answer** - When the player answers the prayer, the entity's belief in the player god increases slightly
5. **Scale with faith** - Higher faith entities show larger UI icons and offer greater rewards
6. **Global cooldown** - General populace has global cooldown; important characters have reduced cooldown

**Key Insight:** Only entities with some existing faith/belief can reach out to the player god. Higher faith entities are more visible and rewarding to help, creating strategic choices between high-value pleads and conversion opportunities.

---

## How It Works

### Trigger Conditions

Entities can send pleads when:

1. **Minimum Faith Threshold Met** - Entity must have faith/belief ≥ 5 in player god to issue a plead (tunable threshold)
2. **Dire Circumstance Exists:**
   - **Health Critical** (HP < 20% of max)
   - **Starvation** (Food need critical, health declining from hunger)
   - **Natural Disaster** (Fire approaching, flood rising, earthquake active)
   - **Combat Defeat** (Morale broken, about to die/yield)
   - **Loss of Loved One** (Family member just died, grief + fear)
   - **Economic Collapse** (Business failed, homeless, destitute)
3. **Cooldown Expired** - Entity must not be on cooldown (global or personal)

**Faith Requirement:** Entities with faith < 5 cannot plead - they either don't know about the player god, lack sufficient belief to reach out, or are aligned with other gods. This creates a conversion incentive (raise faith through other means to enable pleads).

**Notable:** Entities with high faith (41+) generate regular prayer power continuously, but can still send pleads in dire circumstances for emergency aid.

### Plead Escalation & Diminishing Returns

Each unanswered plead increases the offered mana, but **recurring pleaders** (entities who have pleaded before) experience diminishing returns:

**First-Time Pleader:**
```
Plead 1: 100 mana offered
Plead 2: 250 mana offered (150% increase)
Plead 3: 500 mana offered (200% increase)
Plead 4: 1000 mana offered (200% increase)
Plead 5: 2000 mana offered (200% increase)
```

**Recurring Pleader (diminishing returns):**
```
Plead 6: 2500 mana (125% increase, reduced from 200%)
Plead 7: 3000 mana (120% increase)
Plead 8: 3500 mana (117% increase)
... (escalation rate continues diminishing)
```

**Escalation Formula:**
```
BaseEscalation = 1.5
DiminishingFactor = max(0.5, 1.0 - (RecurringPleadCount × 0.1))
EffectiveEscalation = BaseEscalation × DiminishingFactor

ManaOffered = BaseMana × (EffectiveEscalation ^ (CurrentPleadCount - 1))
BaseMana = 100 (tunable)
MaxEscalation = 10 pleads (then stops offering more)
```

**Reasoning:** 
- First-time pleaders escalate aggressively (desperation)
- Recurring pleaders offer less relative increase (they've been helped before, less desperate)
- Prevents infinite escalation from same entity
- Creates tension: answer first pleads for conversion vs wait for escalation

### Plead Content

Each plead includes:

1. **Request Type:** What they need (Heal, Protect, Save, Stop, etc.)
2. **Target Location:** Where the aid is needed (their position, their home, village under attack)
3. **Mana Offered:** Current escalation amount (scaled by faith)
4. **Entity Info:** Name, current faith/belief in player god, alignment, profession, importance level
5. **Urgency:** How dire the situation (affects conversion bonus)
6. **Faith Level:** Entity's faith in player god (affects UI icon size and rewards)

### Answering Pleads

When player answers a plead:

1. **Player receives offered mana** - Added to prayer power pool (scaled by entity's faith)
2. **Miracle cast at target location** - Appropriate miracle for the request (Heal for health, Fire for threats, etc.)
3. **Belief conversion** - Entity's faith in player god increases

**Mana Reward Scaling (by Faith):**
```
BaseMana = PleadEscalationAmount (from escalation formula)
FaithMultiplier = 1.0 + (EntityFaith / 100)  // 1.0x to 2.0x
FinalMana = BaseMana × FaithMultiplier

Example: 500 base mana, entity with 60 faith
FinalMana = 500 × (1.0 + 60/100) = 500 × 1.6 = 800 mana
```

**Conversion Amount:**
```
BaseConversion = 5 faith points
UrgencyBonus = (100 - CurrentHP%) / 10  // Up to +10 bonus
ManaBonus = min(ManaOffered / 200, 5)   // Up to +5 bonus
FaithBonus = EntityFaith / 20            // Up to +5 bonus (high faith = already believes, less conversion needed)
TotalConversion = BaseConversion + UrgencyBonus + ManaBonus + FaithBonus
Capped at: 25 faith points per answered plead
```

**Example:**
- Entity at 15% HP, offered 500 mana, has 40 faith
- UrgencyBonus = (100-15)/10 = 8.5
- ManaBonus = 500/200 = 2.5
- FaithBonus = 40/20 = 2.0
- TotalConversion = 5 + 8.5 + 2.5 + 2.0 = 18 faith points
- ManaReceived = 500 × (1.0 + 40/100) = 700 mana

### Belief Conversion Thresholds

Belief scale: 0-100

- **0-20:** Non-believer (skeptical, won't pray normally)
- **21-40:** Open-minded (might pray in crisis)
- **41-60:** Converted (believes in player god, generates prayer power)
- **61-80:** Devoted (generates more prayer power, more likely to spread belief)
- **81-100:** Fanatical (generates highest prayer power, actively proselytizes)

**Conversion Strategy:** Answer 3-4 pleads from same entity → converts them (reaches 40+ belief) → they start generating regular prayer power.

---

## Player Interaction

### Plead Notifications

**UI Display (Icon Size Scales with Faith):**
```
┌─────────────────────────────────────┐
│ 🆘 PLEAD FOR AID                    │
├─────────────────────────────────────┤
│ [LARGE ICON - High Faith Entity]   │  ← Icon size = Faith / 10 (min 1, max 5)
│ Marcus (Blacksmith, 12% HP)        │
│ Faith: 65 ⭐⭐⭐⭐⭐                   │
│ "Please, save me from these flames!"│
│                                     │
│ Offers: 800 Mana (500 base + faith)│
│ Location: Village Square            │
│                                     │
│ [ANSWER] [IGNORE] [VIEW LOCATION]  │
└─────────────────────────────────────┘
```

**Visual Indicators:**
- **Icon Size:** Larger icons for higher faith entities (makes them stand out)
  - Faith 5-20: Small icon (1x)
  - Faith 21-40: Medium icon (2x)
  - Faith 41-60: Large icon (3x)
  - Faith 61-80: Very Large icon (4x)
  - Faith 81-100: Huge icon (5x)
- Pleading entities have visible prayer aura (rising energy, intensity scales with faith)
- Location marker on map/world (marker size also scales with faith)
- Prayer line connecting entity to sky (visual request, thicker = higher faith)
- Icon sorting: High faith pleads appear at top of list (most visible/rewarding first)

### Player Choices

1. **Answer Immediately**
   - Receive mana offer
   - Cast appropriate miracle
   - Convert entity (small amount)
   - **Best for:** Early conversion, saving lives

2. **Wait for Escalation**
   - Ignore initial pleads
   - Let mana offer increase
   - Risk entity dying before answering
   - **Best for:** Maximizing mana gain, but risky

3. **Ignore Completely**
   - Entity may survive anyway (luck, other help)
   - Entity may die (no conversion, no mana)
   - Entity may convert to another god (if other gods exist)
   - **Best for:** Focus on existing believers only

### Strategic Considerations

- **Early Game:** Answer pleads for conversion (grow believer base)
- **Mid Game:** Balance answering vs letting escalate (mana vs conversion)
- **Late Game:** Focus on high-value pleads (large mana offers, key entities)

---

## System Interactions

### Integration with Prayer Power System

**Prayer Power Flow:**
```
Regular Believers (Faith 41+) → Generate prayer power continuously
Low Faith Entities (Faith 5-40) → Can send pleads (one-time mana + conversion opportunity)
Answered Pleads → Increase faith → May convert to believers → Start generating prayer power
High Faith Pleaders (Faith 60+) → Offer more mana, larger UI icons, already generating prayer power
```

**Design Intent:** 
- Low faith entities can plead but offer less mana (conversion opportunity)
- High faith entities plead less often (they're believers already) but offer significantly more mana when they do
- Creates strategic choice: help low faith for conversion vs help high faith for immediate mana gain

### Integration with Miracle System

**Miracle Selection:**
- Health critical → Heal Miracle
- Fire/disaster → Fire/Water/Protection Miracle
- Combat defeat → Shield/Heal/Teleport Miracle
- Starvation → Food Miracle (if exists) or Bless Harvest

**Cost Considerations:**
- Plead offers mana, but miracles still cost prayer power
- Player must balance: Receive mana offer vs miracle cost
- Net gain if mana offer > miracle cost (usually true for escalated pleads)

### Integration with Belief System

**Current Faith State:**
- Entities with 0-4 faith → **Cannot plead** (insufficient belief to reach out to player god)
- Entities with 5-20 faith → Can send pleads (low faith, small icons, modest mana)
- Entities with 21-40 faith → Can send pleads (medium faith, medium icons, good mana)
- Entities with 41-60 faith → Can send pleads (high faith, large icons, excellent mana, already generating prayer power)
- Entities with 61-100 faith → Can send pleads (very high faith, huge icons, maximum mana, strong prayer generation)

**Conversion Flow:**
```
Non-believer (0-4 faith) → Cannot plead (faith too low)
                        → Need other conversion methods first
                        → Once faith reaches 5+ → Can start pleading

Low believer (5 faith) → Dire circumstance → Plead 1 (100 base × 1.05 = 105 mana, small icon)
                      → Unanswered → Plead 2 (250 base × 1.05 = 262 mana)
                      → Answered → +18 faith → 23 faith (now medium icon)
                      → Still dire → Plead 3 (500 base × 1.23 = 615 mana, medium icon)
                      → Answered → +20 faith → 43 faith
                      → Converted! (40+ faith) → Starts generating prayer power, large icon for future pleads
```

---

## Key Parameters (All Tunable)

| Parameter | Default Value | Reasoning | Tunable By Player |
|-----------|--------------|-----------|-------------------|
| Minimum Faith to Plead | 5 | Prevents entities without belief from pleading | ✅ Yes (0-20 range) |
| Base Mana Offer | 100 | Attractive but not game-breaking | ✅ Yes (50-500 range) |
| Base Escalation Multiplier | 1.5x | Steady increase creates tension | ✅ Yes (1.2x-2.0x range) |
| Diminishing Return Rate | 0.1 per recurring plead | Reduces escalation for repeat pleaders | ✅ Yes (0.05-0.2 range) |
| Max Escalation | 10 pleads | Prevents infinite escalation | ✅ Yes (5-20 range) |
| Base Conversion | 5 faith | Small but meaningful | ✅ Yes (1-10 range) |
| Max Conversion | 25 faith | Prevents instant full conversion | ✅ Yes (10-50 range) |
| Conversion Threshold | 40 faith | Point where entity becomes believer | ✅ Yes (20-60 range) |
| Global Cooldown (General) | 60 seconds | Prevents spam from general populace | ✅ Yes (30-300 seconds) |
| Cooldown Reduction (Important) | 50% reduction | Important characters plead more often | ✅ Yes (0-75% reduction) |
| Faith Multiplier for Mana | 1.0 + (Faith/100) | Higher faith = more mana reward | ✅ Yes (0.5x-2.0x scaling) |
| Icon Size Scaling | Faith/10 (1-5x) | Visual prominence scales with faith | ✅ Yes (Icon size formula) |

---

## Edge Cases

### Entity Dies Before Answer

- **Plead disappears** (entity dead, no conversion possible)
- **Player loses opportunity** (no mana, no conversion)
- **Design Intent:** Creates urgency - answer quickly or risk loss (higher urgency for high faith entities showing larger icons)

### Entity Faith Drops Below Threshold

- **Active plead cancelled** (entity lost faith, can't maintain prayer connection)
- **Can happen if:** Entity converts to another god, loses faith through negative events
- **Design Intent:** Faith maintenance matters - answered pleads build faith, unanswered may reduce it

### Recurring Pleader Reaches Max Escalation

- **Entity stops escalating** (hit max 10 pleads with diminishing returns)
- **Plead remains active** (still offers final mana amount)
- **Player can still answer** (receives diminished reward, but conversion still applies)
- **Design Intent:** Prevents infinite escalation, but doesn't cut off desperate entities completely

### Multiple Entities Plead Simultaneously

- **UI shows list of active pleads**
- **Player can answer multiple** (if has enough prayer power for miracles)
- **Each answered independently** (mana received, conversion applied)

### Entity Already Converting to Another God

- **Pleads override other conversion** (desperation trumps slow conversion)
- **Answering plead converts to player god** (immediate conversion)
- **Ignoring allows other conversion to continue** (other god gains follower)

### Entity Pleads During Active Miracle

- **If miracle helps them** → Plead automatically answered
- **Conversion applies normally** (they see player god answered their prayer)
- **If miracle doesn't help them** → Plead continues (they need specific aid)

---

## Examples

### Example 1: Fire Emergency (Low Faith Entity)

**Setup:**
- Marcus (Blacksmith, 15% HP, 8 faith in player god - just above threshold)
- Village fire spreading, Marcus trapped in workshop
- Player has 2000 prayer power available

**Timeline:**
1. Fire reaches workshop → Marcus sends Plead 1 (100 base × 1.08 = 108 mana, small icon): "Save me from the flames!"
2. Player ignores (waiting for escalation, small icon = lower priority)
3. Fire intensifies → Marcus sends Plead 2 (250 base × 1.08 = 270 mana): "Please, I'll give anything!"
4. Player answers with Fire Miracle (extinguishes fire)
5. Player receives 270 mana (scaled by faith)
6. Marcus gains +18 faith (5 base + 8 urgency + 2 faith bonus = 15, but calculation shows 18)
7. Marcus now has 26 faith (medium icon size for future pleads)

**Outcome:** Player gained 270 mana, saved Marcus, increased faith (still below conversion threshold, but closer).

### Example 2: Combat Defeat (High Faith Entity)

**Setup:**
- Elena (Warrior, Hero importance, 8% HP, 65 faith in player god)
- Losing duel, about to yield/die
- Player has 1500 prayer power

**Timeline:**
1. Elena's morale breaks → Sends Plead 1 (100 base × 1.65 = 165 mana, **very large icon**): "God, grant me strength!"
   - Large icon draws player attention immediately
   - High faith = more visible and rewarding
2. Player answers with Heal Miracle (restores HP)
3. Player receives 165 mana (faith-scaled, even on first plead)
4. Elena gains +22 faith (5 base + 9 urgency + 3 faith bonus)
5. Elena now has 87 faith → **DEVOTED BELIEVER!** Generates strong prayer power continuously
   - Future pleads (if any) would show **huge icon** (5x size)

**Outcome:** Player helped high-value entity, received good mana reward, strengthened existing believer. Large icon made this plead stand out.

**Note:** Elena has Hero importance, so her cooldown is reduced (50% faster than commoners), meaning she can plead again sooner if dire circumstances recur.

### Example 3: Recurring Pleader (Diminishing Returns)

**Setup:**
- Marcus (same blacksmith from Example 1, now has pleaded 4 times before)
- Fire breaks out again, Marcus at 18% HP, now has 35 faith
- Player wants to see escalation, but Marcus is a recurring pleader

**Timeline:**
1. Marcus sends Plead 1 (100 base × 1.35 = 135 mana, medium icon)
   - But he's a recurring pleader (4 previous pleads), so escalation is reduced
   - Diminishing factor: 1.0 - (4 × 0.1) = 0.6
2. Player ignores (waiting for escalation)
3. Fire intensifies → Marcus sends Plead 2
   - Would normally be 250 base, but escalation is: 1.5 × 0.6 = 0.9x (actually diminishing!)
   - Actually: 100 × (0.9 ^ 1) = 90 base mana (less than first!)
   - Final: 90 × 1.35 = 121 mana (diminished)
4. Player realizes diminishing returns → Answers immediately
5. Player receives 135 mana (first plead amount)
6. Marcus gains +19 faith → Now 54 faith (large icon for future pleads)

**Outcome:** Recurring pleaders offer less escalation, creating diminishing returns. Player learned to answer first pleads from recurring entities immediately rather than waiting.

---

## Technical Considerations

### Components Needed

```csharp
// Plead request component
public struct DesperatePlead : IComponentData
{
    public Entity SourceEntity;           // Who is pleading
    public PleadType RequestType;        // Heal, Protect, Save, etc.
    public float3 TargetLocation;        // Where aid needed
    public float BaseManaOffered;        // Base escalation amount (before faith scaling)
    public float FinalManaOffered;       // Final amount after faith multiplier
    public byte PleadCount;              // Escalation level (1-10)
    public byte EntityFaith;             // Entity's faith level (affects icon size, rewards)
    public byte RecurringPleadCount;     // How many times this entity has pleaded before (for diminishing returns)
    public ImportanceLevel Importance;   // Character importance (affects cooldown)
    public uint FirstPleadTick;          // When first plead sent
    public uint LastPleadTick;           // Last plead timestamp
}

// Plead escalation state (on entity)
public struct PleadState : IComponentData
{
    public bool HasActivePlead;          // Currently pleading?
    public byte TotalPleadsSent;         // Lifetime count (for diminishing returns)
    public byte RecurringPleadCount;     // Count of previous plead sessions
    public uint LastPleadTick;           // Cooldown tracking
    public uint GlobalCooldownEndTick;   // When global cooldown expires
    public float TotalManaOffered;       // Lifetime total (for analytics)
}

// Importance level for cooldown scaling
public enum ImportanceLevel : byte
{
    Commoner = 0,        // Full global cooldown
    Notable = 1,         // 25% cooldown reduction
    Important = 2,       // 50% cooldown reduction
    Hero = 3,            // 75% cooldown reduction
    Legendary = 4        // Minimal cooldown (90% reduction)
}

// Tunable configuration (singleton)
public struct PleadSystemConfig : IComponentData
{
    public byte MinimumFaithToPlead;          // Default: 5
    public float BaseManaOffer;               // Default: 100
    public float BaseEscalationMultiplier;    // Default: 1.5
    public float DiminishingReturnRate;       // Default: 0.1
    public byte MaxEscalation;                // Default: 10
    public byte BaseConversion;               // Default: 5
    public byte MaxConversion;                // Default: 25
    public byte ConversionThreshold;          // Default: 40
    public uint GlobalCooldownSeconds;        // Default: 60
    public float CooldownReductionPerTier;    // Default: 0.25 (25% per importance tier)
    public float FaithManaMultiplierBase;     // Default: 1.0
    public float IconSizeScaling;             // Default: 0.1 (Faith/10)
}
```

### Systems Needed

1. **PleadTriggerSystem** - Detects dire circumstances, checks faith threshold, creates pleads
2. **PleadEscalationSystem** - Increases mana offers over time, applies diminishing returns for recurring pleaders
3. **PleadAnswerSystem** - Processes player answers, applies faith scaling to mana rewards, applies conversion
4. **PleadCooldownSystem** - Enforces global cooldown (scaled by importance level)
5. **PleadExpirySystem** - Removes pleads when entity dies/situation resolves
6. **PleadUISystem** - Displays pleads to player (sorted by icon size/faith), handles input
7. **PleadConfigSystem** - Allows player to tune parameters at runtime

### Performance Considerations

- **Limit active pleads:** Max 20 simultaneous pleads (priority queue by faith/icon size, then urgency)
- **Cooldown enforcement:** 
  - Global cooldown prevents population-wide spam (60s default, tunable)
  - Individual cooldown scales with importance (important characters plead more often)
  - Recurring pleaders tracked efficiently (single byte counter)
- **Faith checks:** Only entities with faith ≥ threshold can trigger plead checks (early exit optimization)
- **Burst-compatible:** All systems use ECS patterns, Burst-compiled

---

## Balance Considerations

### Mana Economy

**Risk:** Players could farm pleads for infinite mana
**Mitigation:**
- Cooldowns prevent spam
- Entities must be in dire circumstances (can't be faked)
- Conversion removes future pleads (converted entities don't plead, they pray normally)

### Conversion Rate

**Risk:** Too fast conversion (1-2 pleads) vs too slow (10+ pleads)
**Target:** 3-5 pleads to convert non-believer (feels earned, not instant)
**Tuning:** Adjust BaseConversion and thresholds to hit target

### Player Agency

**Risk:** Pleads feel like annoying notifications
**Mitigation:**
- Visual clarity (clear UI, easy to dismiss)
- Strategic depth (escalation creates meaningful choices)
- Optional (player can ignore if focusing on other goals)

---

## Future Enhancements

### Plead Types Beyond Basic

- **Group Pleads:** Multiple entities plead together (higher mana offer, faster conversion)
- **Tribute Pleads:** Entities offer resources/gold in addition to mana
- **Promised Conversion:** Entities promise full conversion if player helps (contract-style)

### Other Gods

- **Competing Pleads:** Entities plead to multiple gods, player competes for conversion
- **God Reputation:** Answering pleads builds reputation, attracts more pleads

### Narrative Integration

- **Plead Memory:** Entities remember if player answered/didn't answer
- **Gratitude System:** Answered pleads create gratitude, affects future interactions
- **Conversion Stories:** Converted entities tell stories, spread belief to others

---

## Open Questions

1. **Should pleads cost the entity anything?** (Current: Free to send, but offers mana on answer)
2. **Can entities plead to non-player gods?** (Future: Other AI gods)
3. **Should there be negative consequences for ignoring pleads?** (Reputation loss, anger, etc.)
4. **How do pleads interact with alignment?** (Do evil gods get different pleads?)
5. **Should converted entities remember their pleads?** (Narrative depth)

---

## Related Concepts

- **Prayer Power System:** `Docs/Concepts/Implemented/Core/Prayer_Power.md`
- **Miracle System:** `Docs/Concepts/Miracles/Miracle_System_Vision.md`
- **Belief/Conversion:** `Docs/Concepts/Villagers/Village_Villager_Alignment.md` (mentions conversion)
- **Crisis Systems:** `Docs/Concepts/Crisis/Alert_State_System.md`

---

**For Implementers:** 
- Start with faith threshold check (minimum 5 faith to plead)
- Implement basic escalation (1.5x) with diminishing returns tracking
- Add global cooldown system with importance-based scaling
- Implement faith-based mana scaling and icon size calculation
- Add tunable config system for player customization

**For Designers:** 
- Focus on tuning faith thresholds (5 default, but player may want lower/higher)
- Balance diminishing returns rate (0.1 default prevents exploit while still allowing escalation)
- Tune importance-based cooldowns (create meaningful difference between commoners and heroes)
- Icon size scaling creates visual hierarchy (high faith = visible priority)
- All parameters tunable allows players to craft their preferred experience (more/less frequent pleads, faster/slower conversion, etc.)

