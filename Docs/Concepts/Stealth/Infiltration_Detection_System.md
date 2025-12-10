# Infiltration Detection System - Godgame

## Overview

Infiltration is a high-risk activity where success depends on **avoiding detection triggers**. The detection system is multi-layered, responding to physical evidence, magical perception, witness observations, and environmental changes.

When breaches are detected, entities respond based on personality, training, and context—ranging from immediate combat response to panicked flight to opportunistic theft.

---

## Detection Triggers

### 1. Physical Evidence Detection

**Blood and Gore:**
```
Detection Trigger: Entity enters cell with blood/gore
- Fresh Blood Pool: 95% detection chance (obvious)
- Blood Trail: 80% detection chance (following path)
- Blood Splatter: 70% detection chance (combat signs)
- Dried Blood: 40% detection chance (old evidence)
- Cleaned Blood (residue): 15% detection chance (requires high Perception)

Detection Range:
- Same tile: Automatic (unless blind)
- Adjacent tile: 60% (peripheral vision)
- 2 tiles away: 30% (must be looking)

Factors Increasing Detection:
+ High Perception stat (+5% per 10 points)
+ Bright lighting (+20%)
+ Guard on patrol (+15% alertness)
+ Recent violence nearby (+10% vigilance)

Factors Decreasing Detection:
- Darkness (-40%)
- Distracted (conversation, eating) (-30%)
- Drunk/impaired (-50%)
- Rushed/hurrying (-20%)
```

**Bodies and Remains:**
```
Detection Trigger: Dead or unconscious body in area

Body Detection Chance:
- Full corpse in open: 99% (impossible to miss)
- Corpse partially hidden: 70% (behind furniture, shadows)
- Corpse fully hidden: 20% (stuffed in closet, covered)
- Unconscious person: 85% (looks like sleeping but wrong context)
- Severed limbs: 90% (shocking, immediate attention)
- Bone pile: 60% (may be dismissed as animal remains)

Context Matters:
- Body in bed: 10% detection (looks asleep)
- Body in guard post: 95% detection (wrong place)
- Body in tavern: 40% detection (drunk patron assumption)
- Body in battlefield: 5% detection (expected casualties)
```

**Environmental Disturbance:**
```
Detection Trigger: Misplaced or disturbed objects

Detection Chance:
- Broken furniture: 80% (obvious damage)
- Open door (should be locked): 70%
- Missing valuable item: 60% (owner notices immediately)
- Moved furniture: 40% (subtle change)
- Disturbed papers: 30% (requires attention to detail)
- Missing common item: 15% (takes time to notice)

Owner Bonus: +40% detection (knows their space intimately)
Routine Check: Guards inspecting area get +20% detection
```

**Tracks and Traces:**
```
Detection Trigger: Footprints, drag marks, disturbed dust

Detection Chance:
- Muddy footprints: 75%
- Blood footprints: 85%
- Drag marks (body moved): 70%
- Disturbed dust: 40%
- Broken branches/vegetation: 60%
- Scent trail (for animals/heightened senses): 50%

Ranger/Tracker Bonus: +30% detection for trained trackers
Time Decay: -10% per hour (evidence degrades)
Weather: Rain removes outdoor evidence (-50% after 1 hour)
```

---

### 2. Magical and Mental Detection

**Mind Reading (Active Scan):**
```
Detection Method: Telepath actively scanning for hostile intent

Detection Trigger:
- Hostile Intent: 85% detection (planning violence)
- Nervous/Guilty Thoughts: 60% detection (anxiety about crime)
- Deception: 50% detection (actively lying)
- Calm Innocence: 5% detection (no suspicious thoughts)

Telepath Skill Modifier:
- Novice Mind Reader: -20%
- Journeyman: +0%
- Master: +20%
- Archmage: +40%

Target Resistance:
- Strong Will (80+ Willpower): -25% detection
- Mental Training (discipline): -15%
- Anti-magic amulet: -40%
- Mental blank technique: -60% (requires training)

Detection Range:
- Touch: 95% success
- 5 meters: 70% success
- 10 meters: 40% success
- 20 meters: 15% success
```

**Passive Magical Wards:**
```
Detection Method: Enchanted areas trigger on intrusion

Ward Types:
1. Alarm Ward
   - Triggers on unauthorized entry
   - Detection: 90% (unless dispelled or bypassed)
   - Alert: Audible chime or mental ping to owner
   - Bypass: Dispel Magic (vs ward strength), authorized token

2. Scrying Ward
   - Records all who enter area
   - Detection: 100% (passive recording)
   - Alert: Owner reviews logs later
   - Bypass: Anti-scrying amulet (50% success)

3. Hostile Intent Ward
   - Triggers on malicious thoughts within area
   - Detection: 70% (intent-based)
   - Alert: Immediate mental alarm to guards
   - Bypass: Mental blank, genuinely peaceful intent

4. Invisibility Dispel Ward
   - Reveals invisible/disguised entities
   - Detection: 85% (vs invisibility/disguise level)
   - Alert: Visual shimmer, forced reveal
   - Bypass: True Seeing immunity, powerful illusion magic
```

**Divination (Retrospective):**
```
Detection Method: Mage uses divination to view past events

Divination Success:
- Recent (< 1 hour): 80% accuracy
- Hours old (1-6 hours): 60% accuracy
- Day old (6-24 hours): 40% accuracy
- Days old (1-7 days): 20% accuracy
- Week+ old: 5% accuracy

Information Revealed:
- Critical Success: Perfect replay of events, identifies infiltrator
- Success: General events, vague description of infiltrator
- Partial: Emotional imprint, sense of wrongness
- Failure: No information, wasted mana

Anti-Divination Countermeasures:
- Anti-scrying ritual at crime scene: -50% accuracy
- Time passed: -10% per day
- Magical interference: -30%
- Divine blessing/curse on area: Variable
```

**Aura Reading:**
```
Detection Method: Sensitive entities detect auras/spiritual signatures

Detection Triggers:
- Blood-Soaked Aura: 75% detection (recent killer)
- Necromantic Taint: 80% detection (undead/death magic)
- Foreign Soul: 60% detection (possession, body-snatcher)
- Curse Mark: 70% detection (cursed individual)
- Divine Blessing: 65% detection (paladin, chosen)
- Magical Residue: 50% detection (recent spell casting)

Reader Skill:
- Cleric/Paladin: +20% (divine sight)
- Sensitive Commoner: +0%
- Blind Seer: +30% (compensatory sense)
```

---

### 3. Digital and Technological Detection (Space4X Elements)

**Hacking Footprints:**
```
Detection Method: Security AI scans for intrusion traces

Digital Evidence:
- Access Logs Altered: 70% detection (forensic analysis)
- Data Downloaded: 80% detection (bandwidth spike)
- Firewall Breach: 90% detection (immediate alert)
- Credential Misuse: 65% detection (abnormal access patterns)
- System Slowdown: 50% detection (background processes)

Detection Timing:
- Real-time (during hack): 60% immediate detection
- Post-intrusion analysis: 85% detection within 1 hour
- Routine audit: 40% detection within 24 hours

Counter-Hacking:
- Cover Tracks Protocol: -30% detection
- Admin Credentials: -40% detection (authorized access assumed)
- Zero-day Exploit: -50% detection (unknown signature)
- AI Assistant: +35% detection (hacker has advanced tools)
```

**Surveillance Systems:**
```
Detection Method: Cameras, sensors, motion detectors

Camera Coverage:
- Direct line of sight: 95% detection
- Peripheral coverage: 60% detection
- Blind spot: 0% detection (must find gaps)
- Looping footage: 0% detection (if successfully hacked)

Motion Sensors:
- Movement detected: 85% trigger
- Slow movement (<0.5 m/s): 40% trigger
- Crawling: 60% trigger
- Magical invisibility: 30% trigger (heat signature remains)

Biometric Scanners:
- Authorized person: 0% alert
- Disguise/mask: 70% rejection (unless high-quality)
- Shapeshifter: 40% rejection (biological mimicry good enough)
- Hacked credentials: 15% rejection (random audit)
```

---

### 4. Absence Detection (Missing Persons/Items)

**Missing Person Detection:**
```
Detection Trigger: Expected person fails to appear

Detection Timing:
- Guard missing from post: 15 minutes → investigation
- Servant missing from duties: 2 hours → concern
- Noble missing from event: 30 minutes → alarm
- Family member missing: 6 hours → search
- Peasant missing: 24 hours → noticed by neighbors

Relationship Modifier:
- Close relationship (spouse, parent): -50% time (faster detection)
- Professional duty: -30% time (expected punctuality)
- Casual acquaintance: +100% time (slower to notice)

Investigation Triggered:
- High-status person: Immediate guard deployment
- Guard/soldier: Squad sent to investigate
- Commoner: Friends/family search, may report to authorities
```

**Missing Item Detection:**
```
Detection Trigger: Expected item not in place

Detection Timing:
- Guarded treasure: 1 hour (next guard shift inspection)
- Personal valuable: 2-12 hours (owner notices when needed)
- Inventory item: 24 hours (routine stock check)
- Decorative item: 1 week (casual observation)

Value Modifier:
- Priceless artifact: Immediate panic, lockdown
- Valuable (1000+ gold): Same-day investigation
- Moderate (100-1000 gold): Report within 48 hours
- Common (< 100 gold): May not be reported

Routine Checks:
- Treasury: Every 6 hours (shift change)
- Armory: Daily count
- Warehouse: Weekly inventory
- Personal room: When item needed
```

---

## Entity Response Behaviors

### Response Profiles

**1. Combat Response (Trained Guards, Soldiers)**
```
Trigger: Detects intruder or evidence of intrusion

Immediate Actions:
1. Raise Alarm (80% chance)
   - Shout alert to nearby guards
   - Ring alarm bell if available
   - Send mental alert (if magical communication)

2. Engage Intruder (70% chance if visible)
   - Draw weapon
   - Challenge: "Halt! Identify yourself!"
   - Attack if intruder flees or hostile

3. Secure Area (90% chance if no visible threat)
   - Lock doors
   - Guard chokepoints
   - Search hiding spots (closets, under beds, behind curtains)

4. Call Reinforcements (100% if outnumbered)
   - Retreat to defensible position
   - Hold until backup arrives
   - Prioritize protecting VIP or treasure

Personality Modifiers:
- Brave (70+ Courage): +20% engage, -10% call reinforcements
- Cowardly (<30 Courage): -40% engage, +30% flee instead
- Disciplined (military training): +15% follow protocol
- Panicked (low Willpower): -30% all effective responses
```

**2. Alert and Investigate (Cautious NPCs)**
```
Trigger: Suspicious evidence but no immediate threat

Immediate Actions:
1. Stop and Observe (90% chance)
   - Pause current activity
   - Scan surroundings for 5-10 seconds
   - Listen for suspicious sounds

2. Investigate Evidence (70% chance)
   - Approach blood/body carefully
   - Check if person is alive (wake unconscious)
   - Look for obvious clues (weapon, footprints)

3. Decide Next Action (based on findings)
   - If body alive: "Are you alright? What happened?"
   - If body dead: Raise alarm, fetch guards
   - If evidence but no body: Report to superior
   - If false alarm: Resume activity

4. Report Finding (80% chance for law-abiding citizens)
   - Inform nearest guard/authority
   - Describe what was found
   - May exaggerate (rumor propagation)

Personality Modifiers:
- High Intelligence: +25% investigation thoroughness
- High Perception: +30% notice additional details
- Paranoid: +40% raise alarm, -20% investigate personally
- Apathetic: -50% report, may ignore entirely
```

**3. Flee to Safety (Civilians, Non-Combatants)**
```
Trigger: Detects danger (body, blood, intruder)

Immediate Actions:
1. Panic Response (60% chance for civilians)
   - Audible gasp or scream
   - Drop whatever holding
   - Freeze for 1-3 seconds

2. Flee Decision (destination priority)
   - Home (50% chance): Run to personal residence
   - Party/Friends (25% chance): Seek group of allies
   - Peacekeepers (15% chance): Run to guard post
   - Any Safe Place (10% chance): Nearest building

3. Spread Alarm While Fleeing (40% chance)
   - Shout warnings: "Murder! Help!"
   - Attract attention of others (creates chain reaction)
   - May mislead pursuers if panicked

4. Hide and Cower (20% chance instead of flee)
   - Find nearest hiding spot (closet, under table)
   - Remain silent
   - Emerge only when "safe"

Personality Modifiers:
- Brave: -30% panic, +20% attempt to help
- Cowardly: +40% flee, -50% help or investigate
- Strong Bond (family nearby): +60% flee to family
- Drunk/Impaired: -40% effective response, may stumble
```

**4. Opportunistic Theft (Criminals, Chaotic Zones)**
```
Trigger: Finds unconscious/dead person with valuables

Decision Tree:
1. Assess Risk (Intelligence + Perception check)
   - Are guards nearby? (if yes, 70% flee instead)
   - Is person truly unconscious? (check breathing)
   - Are there witnesses? (if yes, 50% abort)

2. Steal Quickly (if risk acceptable)
   - Loot visible valuables (purse, jewelry, weapons)
   - Time Limit: 10-30 seconds before fleeing
   - Take small items only (can't carry body armor)

3. Leave Scene (90% chance)
   - Do NOT report to authorities
   - Avoid area for rest of day
   - Fence stolen goods within 24 hours

4. Finish Off Victim (10% chance, evil alignment)
   - Ensure victim dead (can't identify thief)
   - Quick throat slit or suffocation
   - Increases murder investigation severity

Context Modifiers:
- Lawless slum: +50% chance of theft
- Noble district: -60% chance (too risky)
- War zone: +70% chance (chaos, no consequences)
- Religious area: -40% chance (moral restraint)

Alignment Modifier:
- Evil: +30% theft, +20% finish victim
- Neutral: +10% theft if desperate
- Good: -70% theft, +40% help victim instead
```

**5. Aid and Question (Good-Aligned, Helpers)**
```
Trigger: Finds unconscious person

Immediate Actions:
1. Check Vitals (90% chance)
   - Kneel beside person
   - Check breathing, pulse
   - Look for obvious injuries

2. Wake Them Up (if alive, 85% chance)
   - Gentle shaking: "Wake up! Can you hear me?"
   - Offer water if available
   - Check for concussion

3. Question Victim (once conscious, 95% chance)
   - "What happened to you?"
   - "Who did this?"
   - "Do you need a healer/guard?"

4. Provide Assistance (based on answer)
   - Escort to healer if injured
   - Call guards if crime reported
   - Offer shelter if victim homeless
   - Give coin if victim robbed

Personality Modifiers:
- Paladin/Cleric: 100% help, +healing magic
- Good-aligned: +30% help, may risk self
- Neutral: +10% help if low risk
- Healer profession: +40% medical aid
```

---

## Detection Escalation and Alert Propagation

### Alert Levels

**Level 0: Normal (No Suspicion)**
```
State: Business as usual
Guard Behavior:
- Routine patrols (predictable routes)
- 30% alertness (distracted, chatting)
- Investigation chance: 20% for suspicious activity

Civilian Behavior:
- Daily routines (work, shopping, socializing)
- Minimal awareness of surroundings
- Doors unlocked, windows open
```

**Level 1: Suspicious (Minor Evidence Found)**
```
Trigger: Blood spotted, open door, unusual noise
Duration: 15-30 minutes (if no further evidence)

Guard Behavior:
- Increased patrol frequency (+50%)
- 60% alertness (watching carefully)
- Investigation chance: 50%
- Pair up (buddy system)

Civilian Behavior:
- Nervous chatter, rumors spread
- Lock doors, avoid dark areas
- Report suspicious persons to guards
```

**Level 2: Incident (Body Found or Intruder Seen)**
```
Trigger: Corpse discovered, intruder spotted, alarm raised
Duration: 1-3 hours (until resolved or searches exhausted)

Guard Behavior:
- Full mobilization (all guards active)
- 90% alertness (combat-ready)
- Investigation chance: 80%
- Lock down area (no entry/exit without questioning)
- Search protocol:
  - Room-by-room sweep
  - Check all hiding spots
  - Question all persons in area

Civilian Behavior:
- Curfew enforced (stay indoors)
- Cooperate with guards (fear overrides loyalty)
- Barricade homes
- Mob mentality may form (lynch suspect if found)
```

**Level 3: Crisis (Multiple Deaths, Assassination, Invasion)**
```
Trigger: VIP killed, multiple bodies, or confirmed hostile infiltration
Duration: Until threat eliminated or escaped

Guard Behavior:
- Martial law declared
- 100% alertness (no rest, shifts extended)
- Investigation chance: 95%
- Lockdown protocol:
  - Gates closed, no one enters or leaves
  - House-to-house searches
  - Detain all strangers, question under duress
  - Execute suspects if necessary

Civilian Behavior:
- Total panic or militarized cooperation
- Vigilante groups form
- Informant rewards posted (100-1000 gold)
- Martial law acceptance or rebellion (depends on culture)

Special Response:
- Mages deployed (divination, scrying, combat)
- Elite units called in (royal guard, inquisitors)
- Sealing rituals (trap infiltrator in area)
```

### Alert Propagation

**Verbal Alert Spread:**
```
Guard Shout:
- Range: 30 meters (outdoor), 15 meters (indoor)
- Propagation: Each alerted guard shouts, creating chain
- Speed: 30 meters per 3 seconds (shout travels fast)
- Result: Full guard barracks alerted in 1-2 minutes

Civilian Rumor:
- Range: 10 meters (conversation distance)
- Propagation: Slower, person-to-person
- Speed: 50 meters per 5 minutes (people spread news)
- Distortion: +20% inaccuracy per hop (details garbled)
- Result: Whole neighborhood knows in 30 minutes (with errors)
```

**Magical Alert:**
```
Telepathic Network (Guard Command):
- Range: Unlimited (within city)
- Propagation: Instant to all networked guards
- Speed: <1 second
- Accuracy: 100% (direct mental transfer)
- Result: Entire guard force alerted simultaneously

Alarm Spell:
- Range: 100 meter radius
- Propagation: Audible tone + mental ping
- Speed: Instant
- Accuracy: 95% (clear alarm, may miss specifics)
- Result: All entities in range aware of breach
```

**Technological Alert (Space4X):**
```
Security Network:
- Range: Station/city-wide
- Propagation: Digital alert to all security personnel
- Speed: Instant
- Accuracy: 100% (includes location, camera feed, threat assessment)
- Result: Coordinated response, optimal unit deployment
```

---

## Investigation Mechanics

### Crime Scene Investigation

**Guard Investigation Procedure:**
```
1. Secure Scene (5 minutes)
   - Prevent tampering
   - Establish perimeter
   - Document initial observations

2. Evidence Collection (15-60 minutes)
   - Examine body (cause of death, time of death estimate)
   - Collect physical evidence (weapons, blood, footprints)
   - Interview witnesses
   - Magical scan (if mage available)

3. Analysis (1-6 hours)
   - Compare evidence to known criminals/methods
   - Consult records (past crimes, similar patterns)
   - Divination attempt (if high-value target)
   - Formulate suspect profile

4. Pursuit (if suspect identified)
   - Issue description to all guards
   - Search likely hiding places
   - Set up checkpoints at city exits
   - Offer reward for information
```

**Investigation Skill Check:**
```
Success Chance = (Investigator Intelligence × 0.5) + (Perception × 0.3) + (Investigation Skill × 0.2) + Evidence Quality

Evidence Quality:
- Fresh crime scene: +30%
- Physical evidence abundant: +20%
- Witnesses present: +25%
- Magical traces: +15%
- Time passed (> 1 hour): -10% per hour
- Scene tampered with: -30%
- Cleaned up: -50%

Critical Success (90%+): Identify exact infiltrator, know their location
Success (70-89%): Accurate description, general direction fled
Partial (50-69%): Vague description, know method used
Failure (30-49%): Wrong conclusions, misdirect search
Critical Failure (<30%): Blame innocent person, political mess
```

---

## False Positives and Negatives

### False Positive Detection

**Innocent Triggers:**
```
Scenarios That Trigger Alerts (But Aren't Infiltration):

1. Medical Emergency
   - Person collapsed from heart attack
   - Blood from legitimate injury
   - Guards investigate, find medical issue, call healer
   - Alert level: Raised briefly, then lowered

2. Accident
   - Worker drops red paint, looks like blood
   - Guards investigate, realize mistake
   - Alert level: Minimal, guards embarrassed

3. Prank or Test
   - Prankster leaves fake body (dummy)
   - Security drill (intentional false alarm)
   - Guards respond seriously, then relieved/annoyed

4. Animal Intrusion
   - Wild animal leaves blood trail (predator with prey)
   - Guards track "intruder", find wolf
   - Alert level: Animal control, not security crisis

Consequences:
- Guard morale: -5% per false alarm (fatigue, complacency)
- Civilian trust: -10% if frequent false alarms
- "Cry wolf" effect: Real infiltration may be dismissed (-20% response speed)
```

### False Negative (Missed Detection)

**Infiltrator Avoids Detection:**
```
Success Factors:

1. Stealth Mastery
   - Invisibility spell: 70% avoid visual detection
   - Silence spell: 80% avoid auditory detection
   - Scent masking: 60% avoid animal/tracker detection

2. Perfect Cleanup
   - Hide body in container (barrel, chest): 85% avoid discovery
   - Clean blood thoroughly: 70% avoid visual detection
   - Magical erasure of evidence: 90% avoid physical detection

3. Misdirection
   - Frame innocent person: Investigation targets wrong suspect
   - Create diversion: Guards focus elsewhere
   - Forge evidence: Point investigation away from truth

4. Luck and Timing
   - Guards distracted during intrusion
   - Witness drunk or asleep
   - Alert drowned out by festival noise

Consequences:
- Infiltrator escapes cleanly
- Mission success (theft, assassination achieved)
- Guards blamed for failure (morale penalty, may be punished)
- Future security enhanced (harder next time)
```

---

## Special Detection Scenarios

### Detecting Shapeshifters

```
Difficulty: Very High (shapeshifter mimics appearance perfectly)

Detection Methods:

1. Behavioral Tells (40% success for close associates)
   - Wrong mannerisms (posture, speech patterns)
   - Lack of private knowledge (pet names, inside jokes)
   - Emotional inconsistency (spouse acts cold)

2. Magical Detection (70% success for skilled mages)
   - True Seeing spell reveals true form
   - Aura reading shows foreign soul signature
   - Anti-illusion ward forces shape reveal

3. Physical Tests (60% success)
   - Silver test (if shapeshifter vulnerable)
   - Blood test (may have different blood type)
   - DNA scan (Space4X, 95% accurate)

4. Security Protocols (85% success with strict process)
   - Daily passphrase changes (shapeshifter doesn't know today's)
   - Biometric scans (retina, fingerprint if tech available)
   - Personal questions only real person would know
```

### Detecting Invisible Intruders

```
Difficulty: High (can't see them)

Detection Methods:

1. Auditory (60% success)
   - Footstep sounds (unless silenced)
   - Breathing (heavy breathing from exertion)
   - Clothing rustle (armor, fabric)
   - Equipment clink (keys, coins, weapons)

2. Environmental Clues (50% success)
   - Footprints in dust/snow/mud
   - Displaced objects (floating items if carrying)
   - Disturbed smoke/fog (body shape outline)
   - Shadow (if light source behind them)

3. Magical Detection (80% success)
   - See Invisibility spell
   - Detect Magic (reveals invisibility aura)
   - Dispel Magic (forces visibility)
   - Truesight (penetrates illusions)

4. Physical Contact (90% success)
   - Thrown flour/paint (coats invisible person)
   - Trip wires (alert when broken)
   - Narrow hallway (bump into walls/guards)

5. Animal Senses (70% success)
   - Guard dogs smell intruder (scent remains)
   - Cats stare at invisible person (uncanny awareness)
   - Familiar animals alert master
```

### Detecting Possessed/Mind-Controlled Entities

```
Difficulty: Extreme (entity appears normal, behaves mostly normal)

Detection Methods:

1. Behavioral Anomalies (30% success for strangers, 60% for close associates)
   - Acts out of character (peaceful person becomes violent)
   - Follows orders blindly (no questions)
   - Emotionless or wrong emotional responses
   - Sudden skill changes (knows things they shouldn't)

2. Magical Scanning (85% success for skilled clerics/mages)
   - Detect Evil (if possessor is malevolent)
   - Aura reading (foreign presence visible)
   - Exorcism attempt (resistance confirms possession)
   - Telepathy (two minds detected)

3. Physical Signs (50% success)
   - Eye discoloration (possessed often have changed eye color)
   - Unnatural strength (possession grants power)
   - Pain immunity (doesn't react to injuries)
   - Speaking in different voice/language

4. Interview/Questioning (40% success)
   - Possessed person confused about recent actions
   - Memory gaps for time controlled
   - Contradictory statements
   - Can't answer personal questions
```

---

## Integration with ECS Systems

### Body ECS (60 Hz): Physical Detection

```csharp
public struct PhysicalEvidenceComponent : IComponentData
{
    public EvidenceType Type; // Blood, body, tracks, etc.
    public float3 Location;
    public float Freshness; // 1.0 = fresh, decays over time
    public float Obviousness; // 0-1, how hard to miss
}

public struct DetectionCheckComponent : IComponentData
{
    public float PerceptionRange; // How far entity can detect
    public float PerceptionSkill; // 0-100
    public bool IsAlerted; // Currently on high alert
    public float AlertLevel; // 0-3 (normal to crisis)
}

// Detection check runs every frame for entities near evidence
public static bool CheckPhysicalDetection(
    in PhysicalEvidenceComponent evidence,
    in DetectionCheckComponent detector,
    float3 detectorPosition,
    bool hasLineOfSight,
    float lightingLevel) // 0-1
{
    float distance = math.distance(evidence.Location, detectorPosition);

    if (distance > detector.PerceptionRange)
        return false;

    if (!hasLineOfSight)
        return false;

    // Base chance from evidence obviousness
    float baseChance = evidence.Obviousness * evidence.Freshness;

    // Perception skill bonus
    float perceptionBonus = detector.PerceptionSkill / 200f; // 0-0.5

    // Distance penalty
    float distancePenalty = distance / detector.PerceptionRange; // 0-1

    // Lighting modifier
    float lightingMod = lightingLevel; // Dark = hard to see

    // Alert modifier
    float alertBonus = detector.IsAlerted ? 0.2f : 0f;

    float finalChance = (baseChance + perceptionBonus + alertBonus) * lightingMod - distancePenalty;

    return UnityEngine.Random.Range(0f, 1f) < finalChance;
}
```

### Mind ECS (1 Hz): Mental Detection and Response

```csharp
public struct MentalDetectionComponent : IComponentData
{
    public bool HasTelepathy;
    public float TelepathyRange;
    public float TelepathySkill; // 0-100
    public bool CanDetectIntent;
}

public struct ThreatResponseComponent : IComponentData
{
    public ResponseProfile Profile; // Combat, Flee, Investigate, Steal, Help
    public float CourageStat; // 0-100
    public float IntelligenceStat;
    public AlignmentType Alignment;
}

public enum ResponseProfile
{
    CombatResponse,    // Trained guards
    FleeToSafety,      // Civilians
    InvestigateAlert,  // Cautious NPCs
    OpportunisticTheft,// Criminals
    AidAndQuestion     // Good-aligned helpers
}

public static ResponseProfile DetermineResponse(
    in ThreatResponseComponent response,
    bool seesDirectThreat,
    bool hasBackup,
    EvidenceType evidenceFound)
{
    // Combat response for brave trained individuals
    if (response.Profile == ResponseProfile.CombatResponse &&
        response.CourageStat > 60f)
    {
        return ResponseProfile.CombatResponse;
    }

    // Flee if seeing direct threat and not brave
    if (seesDirectThreat && response.CourageStat < 40f)
    {
        return ResponseProfile.FleeToSafety;
    }

    // Criminals steal from unconscious
    if (evidenceFound == EvidenceType.UnconsciousBody &&
        response.Alignment == AlignmentType.Evil)
    {
        // Risk assessment
        if (!hasBackup && response.IntelligenceStat > 40f)
            return ResponseProfile.OpportunisticTheft;
    }

    // Good alignment helps
    if (evidenceFound == EvidenceType.UnconsciousBody &&
        response.Alignment == AlignmentType.Good)
    {
        return ResponseProfile.AidAndQuestion;
    }

    // Default: investigate
    return ResponseProfile.InvestigateAlert;
}
```

### Aggregate ECS (0.2 Hz): Alert Propagation

```csharp
public struct AlertLevelComponent : IComponentData
{
    public int CurrentAlertLevel; // 0-3
    public float AlertDecayTimer; // Time until alert drops
    public int GuardsAlerted; // Count of guards aware
    public float RumorSpreadRadius; // How far rumor has spread
}

public static void PropagateAlert(
    ref AlertLevelComponent alert,
    float3 incidentLocation,
    float deltaTime)
{
    // Alert spreads over time
    alert.RumorSpreadRadius += 10f * deltaTime; // 10 meters per second

    // Alert decays if no new evidence
    alert.AlertDecayTimer -= deltaTime;

    if (alert.AlertDecayTimer <= 0f)
    {
        // Drop alert level
        alert.CurrentAlertLevel = math.max(0, alert.CurrentAlertLevel - 1);

        // Reset decay timer based on new level
        switch (alert.CurrentAlertLevel)
        {
            case 3: alert.AlertDecayTimer = float.MaxValue; // Crisis doesn't decay
                break;
            case 2: alert.AlertDecayTimer = 3600f; // Incident: 1 hour
                break;
            case 1: alert.AlertDecayTimer = 1800f; // Suspicious: 30 min
                break;
            case 0: alert.AlertDecayTimer = 0f;
                break;
        }
    }
}
```

---

## Summary

The Infiltration Detection System creates **realistic security challenges** through:

1. **Multi-Layered Detection**: Physical, magical, digital, and absence-based triggers
2. **Realistic Responses**: Entities react based on personality, training, and context
3. **Alert Escalation**: Detection severity determines response (investigation → lockdown → martial law)
4. **Investigation Mechanics**: Guards analyze evidence, question witnesses, pursue suspects
5. **False Positives/Negatives**: System accounts for mistakes and masterful infiltration

**Key Design Principles:**
- **Context Matters**: Same evidence triggers different responses based on location, culture, alert level
- **Personality Drives Behavior**: Guards fight, civilians flee, criminals steal, helpers aid
- **Escalation is Gradual**: Minor evidence raises suspicion, major evidence locks down area
- **Counterplay Exists**: Infiltrators can clean evidence, misdirect, use magic to evade
- **Failure Has Consequences**: Failed infiltration creates harder conditions for next attempt

This system supports emergent gameplay where infiltrators must carefully consider **what evidence they leave**, **who might find it**, and **how to manipulate or avoid detection systems**.
