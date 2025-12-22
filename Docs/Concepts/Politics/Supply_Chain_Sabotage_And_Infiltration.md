# Supply Chain Sabotage & Infiltration System

**Status:** Draft  
**Category:** Politics - Economic Warfare & Espionage  
**Scope:** Cross-Project (Godgame + Space4X) - Faction/Empire Level  
**Created:** 2025-12-21  
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Enable sophisticated supply chain infiltration and sabotage where critical components (produced in facilities, often outsourced from abroad) become infiltration vectors. Sabotage can affect communications, weapons, combat equipment, and other critical systems with multiple possible outcomes ranging from discovery/rectification to critical success.

**Secondary Goals:**
- Support multi-layered counter-operations (sabotage → counter-sabotage → counter-counter-sabotage)
- Apply to multiple domains (supply chains, documents, electronic warfare, FTL/hyperjump systems)
- Enable complex outcomes (partial success, critical success, discovery with rectification timing)
- Integrate with existing espionage, infiltration, and combat systems

**Key Principle:** Supply chains are vulnerability points. Outsourced components provide infiltration vectors. Sabotage can have cascading effects, and counter-operations create complex multi-layer conflicts (like ambush/counter-ambush systems).

---

## Core Concept

**Supply Chain Vulnerability:** Complex items (weapons, communications, vehicles, ships) require specific components produced in specialized facilities. These components may be **outsourced from foreign suppliers**, creating **infiltration vectors** where saboteurs can insert compromised components into the supply chain.

**Sabotage Outcomes:** Sabotage operations have multiple possible outcomes:
- **Discovery & Rectification (On Time):** Target discovers sabotage, rectifies before use
- **Discovery & Rectification (Too Late):** Target discovers sabotage, but damage already occurs
- **Partial Success:** Sabotage works but not optimally (reduced effectiveness)
- **Full Success:** Sabotage works as intended (complete failure at critical moment)
- **Critical Success:** Sabotage exceeds expectations (catastrophic failure, cascading effects)

**Counter-Operations:** Targets may detect sabotage and execute counter-operations (counter-sabotage, counter-espionage, playing along to feed false information, or even be recruited by the saboteurs). This creates multi-layer conflicts similar to ambush/counter-ambush systems.

---

## Supply Chain Infiltration Vectors

### Component Dependencies

**Complex Items Require Components:**

**Example: Military Communications System**
```
Final Product: Military Comms Array
├─ Requires: Power Core (produced locally)
├─ Requires: Encryption Module (outsourced from Nation A)
├─ Requires: Antenna Array (produced locally)
├─ Requires: Signal Processor (outsourced from Nation B)
└─ Requires: Control Software (outsourced from Nation C)

Infiltration Vectors:
  - Encryption Module supplier (Nation A) → Can insert backdoor
  - Signal Processor supplier (Nation B) → Can insert kill switch
  - Control Software supplier (Nation C) → Can insert malware
```

**Example: Starship Weapons System**
```
Final Product: Plasma Cannon Array
├─ Requires: Power Capacitor (produced locally)
├─ Requires: Focusing Crystal (outsourced from Corporation X)
├─ Requires: Targeting Computer (produced locally)
├─ Requires: Cooling System (outsourced from Corporation Y)
└─ Requires: Fire Control Software (outsourced from Corporation Z)

Infiltration Vectors:
  - Focusing Crystal supplier → Can insert flawed crystal (explodes under stress)
  - Cooling System supplier → Can insert component with hidden flaw (overheats)
  - Fire Control Software supplier → Can insert logic bomb (malfunctions at critical moment)
```

---

### Infiltration Methods

**1. Supplier Infiltration**

**Method:** Infiltrate the **supplier facility** (foreign nation/corporation) and compromise components during production.

**Process:**
1. Infiltrate supplier organization (espionage, bribery, blackmail)
2. Gain access to production line or quality control
3. Introduce compromised components (defective, backdoored, or sabotaged)
4. Components enter supply chain (appear legitimate)
5. Components integrated into final products
6. Sabotage activates when products used

**Example:**
```
Target: Nation B's military communications (during war with Nation A)
Supplier: Corporation X (produces encryption modules for Nation B)
Infiltration: Nation A spies infiltrate Corporation X (bribe quality control engineer)
Sabotage: Engineer introduces encryption modules with backdoor (allows Nation A to decrypt)
Result: Nation A can intercept and decrypt Nation B's military communications
```

---

**2. Shipping Interception**

**Method:** Intercept components **during transport** (before reaching target) and replace with sabotaged versions.

**Process:**
1. Intelligence identifies shipping routes/timelines
2. Intercept shipment (piracy, hijacking, official inspection abuse)
3. Replace legitimate components with sabotaged versions
4. Allow shipment to continue (target unaware of substitution)
5. Sabotaged components integrated into final products

**Example:**
```
Target: Faction A's starship production
Component: Focusing crystals (shipped from Mining Corp to Faction A)
Interception: Faction B pirates intercept shipment in neutral space
Substitution: Replace crystals with flawed versions (structural weakness)
Delivery: Shipment continues, Faction A receives flawed crystals
Result: Starships built with flawed crystals, weapons explode under stress
```

---

**3. Quality Control Compromise**

**Method:** Compromise the target's **quality control** process so sabotaged components pass inspection.

**Process:**
1. Infiltrate target's quality control (bribery, blackmail, replacement)
2. Modify inspection procedures (skip tests, falsify results)
3. Allow sabotaged components to pass (marked as "certified")
4. Components enter production (target trusts "certified" components)
5. Sabotage activates during use

**Example:**
```
Target: Empire X's weapon production
Component: Power cores (outsourced from multiple suppliers)
Infiltration: Empire Y bribes quality control inspector
Compromise: Inspector certifies all power cores without testing
Sabotage: Sabotaged cores pass inspection, enter production
Result: Weapons fail during combat (power cores explode)
```

---

## Sabotage Types

### 1. Communications Sabotage

**Methods:**

**Backdoor/Intel Leak:**
- Component contains hidden backdoor (allows intercept/decryption)
- Saboteur gains access to communications
- **Effect:** Intel leaked to saboteur, communications compromised

**False Information:**
- Component corrupts or manipulates messages
- Messages contain false information
- **Effect:** Target receives/manages false intel, makes wrong decisions

**Jamming/Disruption:**
- Component contains jamming capability (activates on command)
- Communications jammed during critical moments (combat, operations)
- **Effect:** Target loses communications during critical operations

**Example: Encryption Module Backdoor**
```
Sabotage: Encryption module contains backdoor (allows Nation A to decrypt)
Activation: When communications encrypted with module, Nation A can decrypt
Discovery: Nation B detects unusual decryption patterns (if monitoring)
Outcome: 
  - If discovered early: Module replaced, minimal damage
  - If discovered late: Months of communications compromised, intel leaked
  - If not discovered: Ongoing intel advantage for Nation A
```

---

### 2. Weapons Sabotage

**Methods:**

**Explosive Failure:**
- Component designed to fail catastrophically (explode under stress)
- Weapon explodes during use (damages user, reveals position)
- **Effect:** Weapon failure, casualties, equipment loss

**Reduced Effectiveness:**
- Component designed to degrade performance
- Weapon functions but with reduced effectiveness
- **Effect:** Weapon underperforms, combat disadvantage

**Targeting Malfunction:**
- Component corrupts targeting systems
- Weapon misses intended targets (may hit allies)
- **Effect:** Friendly fire, mission failure, tactical disadvantage

**Example: Focusing Crystal Explosion**
```
Sabotage: Focusing crystal has structural flaw (explodes at 80% power)
Activation: When weapon fires at high power, crystal explodes
Discovery: 
  - Pre-combat testing (if thorough): Crystal fails test, replaced
  - During combat: Crystal explodes, weapon destroyed, user injured
Outcome:
  - Partial Success: Some weapons fail, others work (mixed results)
  - Full Success: All sabotaged weapons fail during combat
  - Critical Success: Explosion damages other systems, cascading failure
```

---

### 3. Combat Equipment Sabotage

**Methods:**

**Performance Degradation:**
- Component reduces equipment effectiveness
- Equipment functions but performs poorly
- **Effect:** Tactical disadvantage, reduced combat effectiveness

**Critical Failure:**
- Component designed to fail at critical moment
- Equipment fails during combat/operations
- **Effect:** Mission failure, casualties, strategic disadvantage

**Intel Gathering:**
- Component contains surveillance capability
- Equipment reports position/status to saboteur
- **Effect:** Target location/intel leaked to enemy

**Example: Targeting Computer Backdoor**
```
Sabotage: Targeting computer contains backdoor (reports target data)
Activation: Computer sends targeting data to saboteur (enemy knows what target is aiming at)
Discovery: 
  - Network monitoring: Unusual data transmission detected
  - Pattern analysis: Enemy always knows target location (suspicious)
Outcome:
  - If discovered: Computer replaced, backdoor removed
  - If not discovered: Ongoing intel advantage (enemy knows target's focus)
```

---

## Sabotage Outcomes

### Outcome Categories

**1. Discovery & Rectification (On Time)**

**Definition:** Target discovers sabotage **before** it causes significant damage.

**Discovery Methods:**
- Quality control testing (thorough inspection catches sabotage)
- Routine maintenance (anomalies detected during service)
- Intelligence operations (counter-intelligence identifies threat)
- Pattern analysis (unusual behavior/failures detected)

**Rectification:**
- Sabotaged components identified and removed
- Replacements obtained (may delay production/operations)
- Systems repaired/updated
- Counter-measures implemented (prevent future sabotage)

**Effect:**
- **Minimal Damage:** Sabotage prevented before activation
- **Cost:** Replacement components, delayed operations, increased security
- **Strategic Impact:** Low (sabotage neutralized)

**Example:**
```
Sabotage: Encryption module backdoor
Discovery: Routine security audit detects unusual encryption patterns (Week 2)
Rectification: All modules replaced with secure versions (Week 3)
Damage: 2 weeks of potential intel leak (minimal, no critical operations during period)
Outcome: Sabotage neutralized, minimal strategic impact
```

---

**2. Discovery & Rectification (Too Late)**

**Definition:** Target discovers sabotage **after** it has already caused damage.

**Discovery Timing:**
- During critical operation (sabotage activates, causes failure)
- After operation failure (post-mortem analysis reveals sabotage)
- Pattern analysis (repeated failures indicate sabotage)

**Damage Already Done:**
- Critical operations failed (missions lost, battles lost)
- Intel leaked (enemy gained advantage)
- Equipment lost (weapons/vehicles destroyed)
- Casualties occurred (personnel killed/injured)

**Rectification:**
- Sabotaged components removed/replaced
- Systems repaired
- Counter-measures implemented
- Damage assessment (evaluate strategic impact)

**Effect:**
- **Significant Damage:** Sabotage succeeded partially before discovery
- **Cost:** Replacement components, lost operations, casualties, strategic disadvantage
- **Strategic Impact:** Medium to High (damage already occurred)

**Example:**
```
Sabotage: Focusing crystal explosion flaw
Discovery: Crystal explodes during combat, weapon destroyed (Month 6, during battle)
Rectification: All crystals replaced, production halted temporarily
Damage: 6 months of production compromised, battle lost due to weapon failures, casualties
Outcome: Sabotage partially succeeded, significant strategic impact
```

---

**3. Partial Success**

**Definition:** Sabotage works but **not optimally** (reduced effectiveness, some components work correctly).

**Causes:**
- Incomplete infiltration (only some components sabotaged)
- Manufacturing variation (some sabotaged components work correctly)
- Quality control catches some (partial detection)
- Sabotage design flaws (not all sabotaged components fail as intended)

**Effect:**
- **Mixed Results:** Some failures, some successes
- **Reduced Effectiveness:** Sabotage causes problems but not catastrophic
- **Strategic Impact:** Medium (partial tactical/strategic disadvantage)

**Example:**
```
Sabotage: Targeting computer backdoor (50% of computers compromised)
Activation: 50% of weapons report targeting data to enemy
Discovery: Pattern analysis detects unusual transmissions (Month 3)
Rectification: Compromised computers replaced (Month 4)
Damage: 3 months of partial intel leak, some tactical disadvantages
Outcome: Partial success (sabotage worked but incomplete), moderate impact
```

---

**4. Full Success**

**Definition:** Sabotage works **as intended** (complete failure at critical moment).

**Activation:**
- Sabotage activates during critical operation (battle, mission, crisis)
- All sabotaged components fail as designed
- Target suffers complete failure at worst possible moment

**Effect:**
- **Complete Failure:** Critical operation fails completely
- **Strategic Impact:** High (major tactical/strategic disadvantage)
- **Cascading Effects:** Failure may cause additional problems

**Example:**
```
Sabotage: Encryption module backdoor (100% of modules compromised)
Activation: All military communications decrypted by enemy (during major offensive)
Discovery: Enemy uses intel to counter-attack, reveals they have decryption capability
Damage: Major offensive fails, strategic plans compromised, months of intel leaked
Outcome: Full success (sabotage worked perfectly), high strategic impact
```

---

**5. Critical Success**

**Definition:** Sabotage **exceeds expectations** (catastrophic failure, cascading effects).

**Characteristics:**
- Sabotage causes catastrophic failure
- Failure triggers cascading effects (damages other systems, causes secondary failures)
- Strategic impact exceeds original sabotage goals
- Target suffers maximum damage

**Effect:**
- **Catastrophic Failure:** Complete system collapse
- **Cascading Damage:** Failure damages other systems/components
- **Strategic Impact:** Very High (major strategic disadvantage, may determine conflict outcome)

**Example:**
```
Sabotage: Power core flaw (designed to fail under stress)
Activation: Power core explodes during combat (as designed)
Critical Success: Explosion damages ship's structural integrity
Cascading Effects: 
  - Structural damage weakens hull
  - Hull breach during FTL jump
  - Ship destroyed, crew lost
  - Strategic position lost (ship was key to defense)
Outcome: Critical success (sabotage caused catastrophic failure beyond original goal), very high strategic impact
```

---

## Counter-Operations

### Counter-Operation Types

**Similar to ambush/counter-ambush systems**, supply chain sabotage enables multi-layer counter-operations:

**Layer 1: Sabotage**
- Attacker sabotages supply chain

**Layer 2: Counter-Sabotage (Detection & Rectification)**
- Target detects sabotage, removes/replaces components

**Layer 3: Counter-Counter-Sabotage (Playing Along)**
- Target detects sabotage but **plays along** (feeds false information, traps saboteur)

**Layer 4: Counter-Counter-Counter-Sabotage (Double Bluff)**
- Attacker knows target is playing along, **uses it against them** (double deception)

---

### Counter-Operation: Playing Along (False Information)

**Strategy:** Target discovers sabotage but **doesn't reveal discovery**, instead uses sabotaged components to **feed false information** to saboteur.

**Process:**
1. Target discovers sabotage (backdoor, surveillance, etc.)
2. Target **doesn't remove** sabotaged components (maintains deception)
3. Target **feeds false information** through sabotaged channel
4. Saboteur receives false information (believes sabotage successful)
5. Target gains advantage (saboteur acts on false information)

**Example: Encryption Module Backdoor (Counter-Operation)**
```
Sabotage: Nation A inserts backdoor in Nation B's encryption modules
Discovery: Nation B detects backdoor during security audit (Week 1)
Counter-Operation: Nation B plays along (doesn't remove backdoor)
False Information: Nation B feeds false strategic plans through backdoored communications
Result: Nation A receives false intel, makes strategic decisions based on false information
Outcome: Nation B gains advantage (Nation A's strategy based on false intel)
```

---

### Counter-Operation: Trap & Counter-Ambush

**Strategy:** Target discovers sabotage, **allows it to proceed** but sets trap to **capture/eliminate saboteur agents** or **counter-attack**.

**Process:**
1. Target discovers sabotage
2. Target **allows sabotage to activate** (doesn't prevent it)
3. Target sets trap (monitors saboteur activity, prepares counter-attack)
4. Sabotage activates (as saboteur expects)
5. Target executes counter-attack (eliminates saboteur agents, strikes back)

**Example: Targeting Computer Backdoor (Trap)**
```
Sabotage: Enemy inserts backdoor in targeting computers (reports target data)
Discovery: Target detects backdoor during network monitoring
Counter-Operation: Target allows backdoor to activate, sets trap
Trap: Target feeds false targeting data, monitors enemy responses
Counter-Attack: When enemy acts on false data, target ambushes enemy forces
Outcome: Enemy walks into trap, suffers heavy losses
```

---

### Counter-Operation: Double Bluff

**Strategy:** Attacker **knows target is playing along**, uses target's deception **against them** (double deception).

**Process:**
1. Attacker inserts sabotage
2. Target discovers sabotage, plays along (feeds false information)
3. Attacker **detects** target is playing along (intelligence, pattern analysis)
4. Attacker **uses target's false information** against them (double bluff)
5. Target's deception backfires (target acts on assumption attacker believes false info)

**Example: Encryption Module (Double Bluff)**
```
Sabotage: Nation A inserts backdoor
Counter-Operation: Nation B plays along, feeds false plans
Double Bluff: Nation A detects Nation B is playing along
Nation A Strategy: Nation A acts as if believing false plans, but prepares for real plans
Nation B Assumption: Nation B thinks Nation A believes false plans
Outcome: Nation A gains advantage (Nation B's assumption is wrong)
```

---

## Electronic Warfare: ECM/ECW/ECCW/ECCCW

### Electronic Warfare Layers

**Similar to ambush/counter-ambush**, electronic warfare creates multi-layer conflicts:

**ECM (Electronic Counter-Measures):**
- Jamming, spoofing, signal disruption
- Prevents enemy from using electronic systems effectively

**ECW (Electronic Counter-Counter-Measures):**
- Counter-jamming, anti-spoofing, signal filtering
- Defends against ECM, restores electronic system functionality

**ECCW (Electronic Counter-Counter-Counter-Measures):**
- Advanced jamming that overcomes ECW
- Defeats counter-measures, re-establishes ECM effectiveness

**ECCCW (Electronic Counter-Counter-Counter-Counter-Measures):**
- Advanced defense that overcomes ECCW
- Defeats advanced counter-measures, re-establishes ECW effectiveness

**And so on...** (layers can continue)

---

### Electronic Warfare & Supply Chain Sabotage

**Sabotaged Components Enable Electronic Warfare:**

**Example: Communications Jamming Component**
```
Sabotage: Signal processor contains hidden jamming capability
Activation: Saboteur activates jamming during combat
Effect: Target's communications jammed (ECM - Electronic Counter-Measures)

Counter-Operation: Target detects jamming, activates ECW (Electronic Counter-Counter-Measures)
ECW: Signal filtering, frequency hopping, anti-jamming protocols
Effect: Communications partially restored

Counter-Counter: Saboteur upgrades jamming (ECCW - Electronic Counter-Counter-Counter-Measures)
ECCW: Advanced jamming that overcomes ECW
Effect: Jamming re-established, communications disrupted again

Counter-Counter-Counter: Target upgrades defense (ECCCW)
ECCCW: Advanced anti-jamming that overcomes ECCW
Effect: Communications restored, jamming defeated

(Conflict continues...)
```

---

## Document Falsification & Counter-Operations

### Document Sabotage

**Method:** Saboteurs **falsify documents** (orders, intelligence, credentials) to mislead target.

**Types:**
- **False Orders:** Forged command documents (target follows wrong orders)
- **False Intelligence:** Fabricated intelligence reports (target makes wrong decisions)
- **False Credentials:** Forged identity documents (saboteurs gain access)
- **False Communications:** Forged messages, transmissions, correspondence

---

### Document/Communication Forgery Quality

**Forgery quality determines detection difficulty** and effectiveness.

---

#### Quality Factors

**1. Original Message Clarity**

**Source Material Quality:**
- **Clear Original:** Intercepted message is clear, complete, high-quality (good template for forgery)
- **Partial/Corrupted Original:** Message is incomplete, corrupted, unclear (poor template, harder to forge convincingly)
- **No Original:** Forging without source material (most difficult, requires full creation)

**Quality Contribution:**
```
ClarityContribution = OriginalClarity × 0.3

Where:
  OriginalClarity = 0.0-1.0 (clarity/completeness of intercepted source)
  
Example:
  Clear original (0.9): +0.27 quality
  Partial original (0.5): +0.15 quality
  No original (0.0): +0.0 quality
```

---

**2. Forgery Skill**

**Entity's Forgery Ability:**
- **Forgery Skill:** Entity's expertise in document/communication forgery (0-100)
- **Experience:** Previous successful forgeries increase skill
- **Specialization:** Some entities specialize in specific document types (orders, intelligence, credentials)

**Quality Contribution:**
```
SkillContribution = ForgerySkill × 0.4

Where:
  ForgerySkill = 0-100 (entity's forgery expertise)
  
Example:
  Master forger (90 skill): +36 quality
  Skilled forger (70 skill): +28 quality
  Novice forger (30 skill): +12 quality
```

---

**3. Language/Protocol Knowledge**

**Understanding Target Systems:**
- **Language Proficiency:** Knowledge of target's language (fluency, grammar, syntax)
- **Protocol Knowledge:** Understanding of communication protocols (military codes, diplomatic formats, official styles)
- **Technical Knowledge:** Understanding of technical systems (encryption formats, digital signatures, authentication methods)

**Quality Contribution:**
```
LanguageContribution = (LanguageKnowledge × 0.15) + (ProtocolKnowledge × 0.15)

Where:
  LanguageKnowledge = 0-100 (language proficiency)
  ProtocolKnowledge = 0-100 (protocol/system understanding)
  
Example:
  Fluent speaker + protocol expert (90/90): +27 quality
  Basic speaker + protocol novice (50/30): +12 quality
```

---

**4. Cultural Knowledge (Bonus)**

**Understanding Target Culture:**
- **Lingo/Slang:** Knowledge of cultural slang, jargon, colloquialisms
- **Abbreviations:** Understanding of common abbreviations, shorthand
- **Phonics/Accents:** Knowledge of phonetic patterns, regional accents (spoken forgeries)
- **Cultural Context:** Understanding of cultural norms, references, inside jokes
- **Behavioral Patterns:** Knowledge of how target communicates (style, tone, formality)

**Cultural Bonus:**
```
CulturalBonus = 
    (LingoKnowledge × 0.05) +
    (AbbreviationKnowledge × 0.03) +
    (PhoneticKnowledge × 0.02) +
    (CulturalContext × 0.05)

Where:
  LingoKnowledge = 0-100 (slang/jargon knowledge)
  AbbreviationKnowledge = 0-100 (abbreviation/shorthand knowledge)
  PhoneticKnowledge = 0-100 (phonetic/accent knowledge - for spoken forgeries)
  CulturalContext = 0-100 (cultural norms/references knowledge)
  
Maximum Cultural Bonus: +15 quality (if all cultural knowledge is 100)
```

**Example:**
```
Spy has deep cultural knowledge:
  Lingo: 95 (knows all slang)
  Abbreviations: 90 (understands all shorthand)
  Phonetic: 80 (recognizes accent patterns)
  Cultural Context: 85 (understands cultural references)
  
Cultural Bonus = (95 × 0.05) + (90 × 0.03) + (80 × 0.02) + (85 × 0.05)
               = 4.75 + 2.7 + 1.6 + 4.25 = +13.3 quality bonus
```

---

#### Total Forgery Quality Calculation

```
ForgeryQuality = 
    ClarityContribution +
    SkillContribution +
    LanguageContribution +
    CulturalBonus

Maximum Quality: 100 (perfect forgery)
Typical Range: 20-90 (depending on entity skills and source material)
```

**Quality Examples:**

**Master Forgery (High Quality):**
```
Clear original (0.9) × 0.3 = 0.27
Master forger (95 skill) × 0.4 = 38.0
Fluent + expert (95/95) × 0.3 = 28.5
Cultural knowledge bonus = +14.0

Total Quality: 80.77 (excellent forgery, very hard to detect)
```

**Novice Forgery (Low Quality):**
```
No original (0.0) × 0.3 = 0.0
Novice forger (25 skill) × 0.4 = 10.0
Basic + novice (40/20) × 0.3 = 9.0
No cultural knowledge = +0.0

Total Quality: 19.0 (poor forgery, easy to detect)
```

---

### On-the-Fly Forgery

**Some spies are so skilled they can forge documents on the fly** (during conversation, in real-time).

**On-the-Fly Forgery Characteristics:**

**Requirements:**
- **High Forgery Skill:** 80+ required (exceptional expertise)
- **Language Fluency:** 90+ required (native-level proficiency)
- **Quick Thinking:** High Intelligence/Adaptability
- **Cultural Knowledge:** Deep understanding (lingo, context, patterns)

**Limitations:**
- **Lower Quality:** On-the-fly forgeries typically 10-20 quality points lower than prepared forgeries
- **Time Pressure:** Must forge quickly (increases detection risk)
- **No Revision:** Can't refine/edit (first draft is final)

**Use Cases:**
- **Conversational Forgery:** Creating false documents during conversation/negotiation
- **Immediate Response:** Forging documents in response to unexpected requests
- **Improvised Operations:** Creating forgeries when plans change

**Example:**
```
Spy Profile:
  Forgery Skill: 95
  Language Knowledge: 98
  Protocol Knowledge: 90
  Cultural Knowledge: 92

Situation: Target asks spy to produce authorization document immediately
On-the-Fly Forgery:
  Base Quality: 75 (excellent skills)
  On-the-Fly Penalty: -15 (time pressure, no revision)
  Final Quality: 60 (good quality, but detectable with careful inspection)
```

---

### Perfect Forgeries & Belief Systems

**Some documents are so finely crafted** that detection becomes extremely difficult, and the forgery itself becomes "real" in the target's belief system.

---

#### Quality Thresholds & Detection

**Detection Difficulty:**
```
DetectionDifficulty = 100 - ForgeryQuality

Where:
  ForgeryQuality = 0-100 (calculated as above)
  
Detection Difficulty Ranges:
  0-20: Very Easy (obvious forgery, easily detected)
  21-40: Easy (detectable with standard inspection)
  41-60: Moderate (requires careful analysis to detect)
  61-80: Difficult (requires expert analysis, may be missed)
  81-95: Very Difficult (expert analysis needed, often believed)
  96-100: Nearly Impossible (essentially perfect, almost always believed)
```

---

#### High-Quality Forgery Effects

**When Forgery Quality > 80:**

**Target Behavior Changes:**

**1. Using Forged Papers as Evidence**

**Instead of clearing the accused's name**, the aggregate entity **uses the forged papers as evidence against the accused element**:

**Example:**
```
Forgery: High-quality forged orders (Quality 85) implicate General X in treason
Target Reaction:
  - Target believes forgery is genuine (detection failed)
  - Target uses forged orders as evidence (prosecutes General X)
  - General X is accused, tried, convicted based on forged evidence
  - Target never questions authenticity (forgery too convincing)
```

**Psychological Mechanism:**
- Forgery is so convincing it bypasses skepticism
- Target's confirmation bias (wants to believe evidence)
- Authority/power dynamics (higher-ups trust the document)

---

**2. Accused Believes Manipulation**

**The accused entity may believe mind control or other manipulations are at play:**

**Example:**
```
Forgery: Perfect forgery (Quality 95) of General X's orders to attack allies
Target Reaction:
  - Target uses forged orders to prosecute General X
  - General X sees forged orders (appears to be his own handwriting, signature)
  - General X cannot explain orders (doesn't remember giving them)
  - General X Conclusion: "I must have been mind-controlled/manipulated!"
  
Result:
  - Accused believes manipulation occurred (can't explain "their" actions)
  - Psychological crisis (identity confusion, trust breakdown)
  - May confess to crimes they didn't commit (believes manipulation is real)
```

**Psychological Mechanism:**
- Perfect forgeries are indistinguishable from originals
- Accused has no memory of creating document (because they didn't)
- Only logical explanation (to accused): manipulation/mind control
- Creates internal crisis (confusion, self-doubt, paranoia)

---

#### Forgery Quality & Belief Systems

**Quality 80-90:**
- **Target uses as evidence:** High likelihood (80-90%)
- **Accused believes manipulation:** Moderate likelihood (30-50%)
- **Detection possible:** Expert analysis required (20-30% detection chance)

**Quality 91-95:**
- **Target uses as evidence:** Very high likelihood (95%+)
- **Accused believes manipulation:** High likelihood (60-80%)
- **Detection possible:** Extremely difficult (5-10% detection chance)

**Quality 96-100 (Perfect/Imperfect Perfection):**
- **Target uses as evidence:** Near certainty (99%+)
- **Accused believes manipulation:** Very high likelihood (80-95%)
- **Detection possible:** Nearly impossible (1-3% detection chance)
- **May never be discovered:** Forgery becomes accepted as truth

---

#### Forgery Discovery Consequences

**If Perfect Forgery is Eventually Discovered:**

**Scenario 1: Discovery After Conviction**
```
Timeline:
  - Forgery used as evidence (Month 1)
  - Accused convicted, executed (Month 2)
  - Forgery discovered as fake (Month 6)
  
Consequences:
  - Massive legitimacy crisis (injustice revealed)
  - Public outrage (wrongful execution)
  - Leadership credibility destroyed (believed false evidence)
  - Accused's family/faction demands justice
  - Internal conflict (who allowed this? who forged it?)
```

**Scenario 2: Discovery Before Conviction**
```
Timeline:
  - Forgery used as evidence (Month 1)
  - Accused on trial (Month 2)
  - Forgery discovered as fake (Month 2, during trial)
  
Consequences:
  - Accused exonerated (but already accused, reputation damaged)
  - Leadership credibility damaged (believed false evidence)
  - Counter-espionage intensifies (seeking forger)
  - Accused may still have psychological crisis (briefly believed manipulation)
```

---

### Forgery Quality Examples

**Example 1: High-Quality Forgery (Quality 85)**

**Setup:**
```
Spy Profile:
  Forgery Skill: 90
  Language Knowledge: 95
  Protocol Knowledge: 88
  Cultural Knowledge: 85
  Original Clarity: 0.8 (good source material)

Calculation:
  Clarity: 0.8 × 0.3 = 0.24
  Skill: 90 × 0.4 = 36.0
  Language: (95 + 88) × 0.15 = 27.45
  Cultural: 85 × 0.15 = 12.75
  Total: 76.44

On-the-Fly Penalty: None (prepared forgery)
Final Quality: 76 (rounded)
```

**Outcome:**
- **Detection Difficulty:** 24 (Difficult - expert analysis needed)
- **Target uses as evidence:** 85% likelihood
- **Accused believes manipulation:** 35% likelihood

---

**Example 2: Perfect Forgery (Quality 97)**

**Setup:**
```
Master Spy Profile:
  Forgery Skill: 100 (legendary master)
  Language Knowledge: 100 (native-level fluency)
  Protocol Knowledge: 100 (complete protocol mastery)
  Cultural Knowledge: 100 (deep cultural immersion)
  Original Clarity: 1.0 (perfect source material)

Calculation:
  Clarity: 1.0 × 0.3 = 0.30
  Skill: 100 × 0.4 = 40.0
  Language: (100 + 100) × 0.15 = 30.0
  Cultural: 100 × 0.15 = 15.0
  Total: 85.3

Bonus: Perfect execution bonus +12 (exceptional craftsmanship)
Final Quality: 97 (nearly perfect)
```

**Outcome:**
- **Detection Difficulty:** 3 (Nearly Impossible)
- **Target uses as evidence:** 99% likelihood
- **Accused believes manipulation:** 90% likelihood
- **Never discovered:** 70% chance forgery never detected

---

**Example 3: On-the-Fly Perfect Forgery (Quality 82)**

**Setup:**
```
Master Spy (same as above) forges document during conversation:

Base Quality: 85.3 (excellent skills)
On-the-Fly Penalty: -15 (time pressure, no revision)
Final Quality: 70 (rounded to 70, then +12 perfect execution bonus = 82)
```

**Outcome:**
- **Detection Difficulty:** 18 (Difficult - but expert analysis might catch it)
- **Target uses as evidence:** 75% likelihood
- **Accused believes manipulation:** 40% likelihood
- **Remarkable achievement:** On-the-fly forgery of this quality is extremely rare

---

### Forgery Detection Mechanics

**Detection Methods:**

**1. Visual Inspection:**
- Handwriting analysis (stylometric analysis)
- Signature verification (signature experts)
- Format checking (protocol compliance, style consistency)

**2. Technical Analysis:**
- Digital signature verification (cryptographic validation)
- Document metadata (creation date, author, modification history)
- Material analysis (paper type, ink composition, age)

**3. Contextual Analysis:**
- Timeline verification (did events happen as document claims?)
- Cross-reference checking (does document match other evidence?)
- Behavioral analysis (does document match accused's typical behavior?)

**4. Expert Analysis:**
- Forgery detection specialists (trained experts)
- Cultural experts (verify cultural context, lingo, references)
- Protocol experts (verify protocol compliance, formatting)

**Detection Success:**
```
DetectionSuccess = 
    InspectorSkill × 0.4 +
    TechnicalCapability × 0.3 +
    ContextualEvidence × 0.2 +
    RandomFactor × 0.1

Where:
  InspectorSkill = 0-100 (forgery detection expertise)
  TechnicalCapability = 0-100 (technical analysis tools/capability)
  ContextualEvidence = 0-100 (contextual inconsistencies found)
  RandomFactor = 0-100 (luck, chance discoveries)
  
Detection occurs if: DetectionSuccess > DetectionDifficulty
```

---

### Component Updates

**Forgery Quality Tracking:**

```csharp
// Document/communication forgery
public struct ForgeryOperation : IComponentData
{
    public Entity TargetEntity;              // Target organization
    public Entity ForgerEntity;              // Entity creating forgery
    public ForgeryType Type;                 // Orders, Intelligence, Credentials, Communications
    public float Quality;                    // 0-100 (forgery quality)
    public bool IsOnTheFly;                  // True if forged in real-time
    public bool IsUsedAsEvidence;            // True if target uses forgery as evidence
    public bool AccusedBelievesManipulation; // True if accused believes manipulation occurred
    public uint CreationTick;                // When forgery was created
}

// Forgery quality components
public struct ForgerProfile : IComponentData
{
    public float ForgerySkill;               // 0-100 (forgery expertise)
    public float LanguageKnowledge;          // 0-100 (target language proficiency)
    public float ProtocolKnowledge;          // 0-100 (protocol/system understanding)
    public float LingoKnowledge;             // 0-100 (slang/jargon knowledge)
    public float AbbreviationKnowledge;      // 0-100 (abbreviation/shorthand knowledge)
    public float PhoneticKnowledge;          // 0-100 (phonetic/accent knowledge)
    public float CulturalContext;            // 0-100 (cultural norms/references knowledge)
    public bool CanForgeOnTheFly;            // True if can forge in real-time (requires 80+ skill)
}
```

---

### Counter-Operation: Playing Along (Document Falsification)

**Similar to supply chain counter-operations**, targets can **play along with false documents**:

**Process:**
1. Saboteur falsifies documents (false orders, false intel)
2. Target **discovers** documents are false (verification, intelligence)
3. Target **plays along** (acts as if believing false documents)
4. Target sets trap (prepares for saboteur's expected action)
5. Saboteur acts on assumption target believes false documents
6. Target counter-attacks (ambushes saboteur, gains advantage)

**Example: False Orders (Counter-Operation)**
```
Sabotage: Enemy forges orders (command to attack Position A)
Discovery: Target verifies orders, discovers forgery
Counter-Operation: Target plays along (pretends to follow false orders)
False Action: Target moves forces toward Position A (as false orders command)
Trap: Target prepares ambush at Position A (expects enemy attack)
Enemy Assumption: Enemy thinks target is attacking Position A (weakens defense elsewhere)
Counter-Ambush: Enemy attacks elsewhere, walks into trap at Position A
Outcome: Target gains advantage (enemy's strategy based on false assumption)
```

---

### Counter-Counter-Operation: Double Bluff (Documents)

**Strategy:** Saboteur **knows target is playing along**, uses target's deception against them.

**Example: False Orders (Double Bluff)**
```
Sabotage: Enemy forges orders (attack Position A)
Counter-Operation: Target plays along, moves toward Position A
Double Bluff: Enemy detects target is playing along (intelligence, pattern analysis)
Enemy Strategy: Enemy prepares for target's real plan (not false orders)
Target Assumption: Target thinks enemy believes false orders
Outcome: Enemy gains advantage (target's assumption is wrong)
```

---

## FTL/Hyperjump Denial & Counter-Operations

### Hyperjump Sabotage

**Method:** Saboteurs compromise **hyperjump/FTL systems** to deny or disrupt faster-than-light travel.

**Types:**
- **Jump Denial:** Component prevents hyperjump activation (ships can't jump)
- **Jump Disruption:** Component causes jump failure (ship lost in jump, damaged)
- **Jump Redirect:** Component redirects jump destination (ship arrives at wrong location)
- **Jump Intel:** Component reports jump coordinates (enemy knows destination)

---

### Hyperjump Counter-Operations

**Similar multi-layer conflicts:**

**Layer 1: Jump Denial Sabotage**
- Saboteur inserts component that prevents/denies hyperjump

**Layer 2: Counter-Operation (Detection & Rectification)**
- Target detects sabotage, replaces component, restores jump capability

**Layer 3: Counter-Counter-Operation (Playing Along)**
- Target detects sabotage but **allows it** (feeds false jump coordinates)

**Layer 4: Counter-Counter-Counter (Double Bluff)**
- Saboteur knows target is playing along, uses it against them

---

**Example: Hyperjump Denial (Multi-Layer)**
```
Sabotage: Enemy inserts component that prevents hyperjump (jump denial)
Discovery: Target detects component flaw during maintenance
Counter-Operation: Target plays along (doesn't remove component)
False Strategy: Target plans false jump (enemy expects jump to be denied)
Enemy Assumption: Enemy thinks target can't jump (weakens defenses at false destination)
Counter-Ambush: Target uses alternative jump method, arrives at real destination
Outcome: Target gains advantage (enemy's defense based on false assumption)

Double Bluff: Enemy detects target has alternative jump capability
Enemy Strategy: Enemy prepares for target's real jump destination
Target Assumption: Target thinks enemy believes jump is denied
Outcome: Enemy gains advantage (target's assumption is wrong)
```

---

## Complex Operations & Strategic Assets

### Strategic Planning for Complex Objectives

**Complex infiltration objectives** require sophisticated planning with **strategic assets on standby** and **multiple contingency plans**.

**Intelligent Aggregate Entities:**

Sophisticated factions/empires plan complex operations with:
- **Multiple contingency plans** (success scenarios, failure scenarios)
- **Strategic assets on standby** (extraction teams, backup operations, diversionary actions)
- **Layered operations** (primary objective + backup targets)
- **Risk management** (agent extraction vs termination decisions)

---

### Contingency Planning

**Contingency Plan Types:**

#### 1. Success Contingencies

**When Primary Objective Succeeds:**

**Follow-Up Operations:**
- Exploit success (expand operation, target additional objectives)
- Secure gains (prevent counter-operations, protect agents)
- Maintain cover (agents continue operations, gather more intel)

**Strategic Assets:**
- **Extraction Teams:** Ready to extract agents if success exposes them
- **Backup Operations:** Additional operations prepared to exploit success
- **Diversionary Actions:** Operations to distract from successful infiltration

**Example:**
```
Primary Objective: Sabotage encryption modules (succeeds)
Success Contingency:
  - Exploit: Use backdoor to gather intel for 6 months
  - Extract: Extraction team ready if agents exposed
  - Backup: Additional sabotage operations prepared (signal processors, targeting computers)
  - Diversion: False flag operations distract from encryption compromise
```

---

#### 2. Failure Contingencies

**When Primary Objective Fails:**

**Containment:**
- Prevent discovery (agents escape before detection)
- Limit damage (minimize intelligence loss)
- Protect other operations (prevent cascade of exposure)

**Strategic Assets:**
- **Extraction Teams:** Evacuate agents before capture
- **Termination Teams:** Eliminate agents if extraction impossible
- **Cover Operations:** False operations to mask real objective
- **Backup Targets:** Alternative objectives if primary fails

**Example:**
```
Primary Objective: Sabotage encryption modules (fails - detected early)
Failure Contingency:
  - Containment: Agents extract immediately (prevents exposure)
  - Backup Target: Shift to signal processor sabotage (alternative infiltration vector)
  - Cover Operation: False operation masks real objective (target thinks attack was on different system)
  - Intelligence Protection: Agents don't reveal other operations if captured
```

---

#### 3. Partial Success Contingencies

**When Primary Objective Partially Succeeds:**

**Adaptation:**
- Adjust objectives (exploit partial success, mitigate failures)
- Combine with backup operations (layer partial successes)
- Maintain operations (continue despite setbacks)

**Strategic Assets:**
- **Adaptive Operations:** Modify plans based on partial results
- **Layered Attacks:** Combine partial successes from multiple operations
- **Recovery Teams:** Salvage operations from partial failures

**Example:**
```
Primary Objective: Sabotage encryption modules (partial success - 50% compromised)
Partial Success Contingency:
  - Adapt: Exploit 50% compromised modules, target remaining 50%
  - Layer: Combine with signal processor sabotage (enhanced effect)
  - Recover: Additional infiltration to complete sabotage
```

---

### Agent Extraction & Termination

**When Operations Are Compromised:**

Intelligent aggregate entities must decide: **Extract agents** or **Terminate them**.

---

#### Extraction (Preferred Option)

**When to Extract:**

**Conditions Favoring Extraction:**
- Agents have valuable intelligence (knowledge of other operations)
- Agents are loyal/high-value (investment worth protecting)
- Extraction is feasible (agents can reach extraction point)
- Exposure risk is manageable (extraction doesn't expose more)

**Extraction Methods:**

**Standard Extraction:**
- **Extraction Teams:** Dedicated teams retrieve agents
- **Safe Houses:** Temporary locations for agent recovery
- **Evacuation Routes:** Pre-planned escape paths
- **Cover Identities:** New identities for extracted agents

**Emergency Extraction:**
- **Exfiltration Under Fire:** Extract during active pursuit
- **Covert Extraction:** Extract without detection (maintain cover)
- **False Flag Extraction:** Extract using false operation (mask real extraction)

**Extraction Planning:**
```
Extraction Plan:
  - Extraction Team: 6-member team (combat-trained, stealth-capable)
  - Safe House: Abandoned warehouse in neutral territory
  - Evacuation Route: Land route → sea route → neutral port
  - Cover: Agents extracted as "refugees" fleeing conflict
  - Timeline: Extraction must occur within 48 hours of compromise
```

---

#### Termination (Containment Option)

**When to Terminate:**

**Conditions Favoring Termination:**
- Extraction is impossible (agents surrounded, no escape route)
- Agents are compromised (already captured, likely to reveal intelligence)
- Agents are low-value (disposable, not worth extraction risk)
- Exposure risk is high (extraction would expose more operations)

**Termination Methods:**

**Covert Termination:**
- **Assassination:** Silent kill (appears accidental or natural)
- **Suicide:** Agent commits suicide (appears voluntary)
- **False Flag:** Terminate using enemy forces (blame on target)

**Overt Termination:**
- **Elimination Team:** Combat team eliminates agents (high risk, last resort)
- **Explosive:** Remotely detonated device (deniable, no traces)
- **Poisoning:** Undetectable poison (appears natural death)

**Termination Planning:**
```
Termination Plan:
  - Method: Poison pill (undetectable, appears natural)
  - Execution: Remote activation if agent compromised
  - Fallback: Elimination team if poison fails
  - Deniability: Operation appears unconnected to employer
  - Timeline: Immediate (within hours of compromise)
```

---

### Agent Suicide Protocols

**Agent Suicide (Last Resort):**

Some agents will **commit suicide** if caught, depending on **loyalty and profile**.

---

#### Suicide Decision Factors

**Loyalty:**
- **High Loyalty (80+):** Will commit suicide to protect operations
- **Medium Loyalty (50-79):** May commit suicide if pressured
- **Low Loyalty (<50):** Will not commit suicide, likely to reveal intelligence

**Profile (Alignment/Outlook):**
- **High Honor/High Purity:** More likely to commit suicide (honor-bound, protect secrets)
- **High Fear/Low Courage:** Less likely to commit suicide (fear of death)
- **High Aggression/High Pride:** May commit suicide (prefer death over capture/humiliation)

**Suicide Probability:**
```
SuicideChance = 
    (Loyalty × 0.4) +
    (HonorOutlook × 0.2) +
    (PurityAlignment × 0.2) +
    (FearOutlook × -0.1) +
    (CourageLevel × 0.1)

Where:
  Loyalty = 0-100 (agent loyalty to employer)
  HonorOutlook = 0-100 (honor-bound outlook)
  PurityAlignment = 0-100 (purity alignment - self-sacrifice)
  FearOutlook = 0-100 (fear outlook - negative modifier)
  CourageLevel = 0-100 (courage level)
  
Example:
  Loyalty 90, Honor 80, Purity 70, Fear 20, Courage 75
  = (90 × 0.4) + (80 × 0.2) + (70 × 0.2) + (20 × -0.1) + (75 × 0.1)
  = 36 + 16 + 14 - 2 + 7.5 = 71.5% suicide chance
```

---

#### Suicide Methods

**Pill (Poison):**
- **Cyanide Pill:** Fast-acting, undetectable (appears natural death)
- **Activation:** Agent takes pill when capture imminent
- **Advantages:** Deniable, appears voluntary, no traces

**Other Methods:**
- **Explosive Device:** Suicide bomb (eliminates agent + potential captors)
- **Self-Inflicted Wound:** Agent kills self (gunshot, blade, fall)
- **Defiance:** Agent attacks captors (suicide by combat, "death before capture")

**Example:**
```
Agent Profile:
  - Loyalty: 95 (extremely loyal)
  - Honor: 85 (high honor outlook)
  - Purity: 80 (high purity alignment)
  - Suicide Probability: 88%

Capture Scenario:
  Agent surrounded, no escape route, extraction impossible
  Agent Decision: Takes cyanide pill (suicide)
  Result: Agent dies, intelligence protected, operations secure
```

---

### Multi-Team/Band Contingencies

**Complex operations involve multiple teams/bands:**

---

#### Primary Team (Objective)

**Primary Team Responsibilities:**
- Execute primary objective (sabotage, infiltration, etc.)
- Maintain cover (avoid detection)
- Report progress (intelligence updates)

---

#### Backup Team (Contingency)

**Backup Team Responsibilities:**
- Standby for extraction (if primary team compromised)
- Execute backup objectives (if primary objective fails)
- Provide diversion (distract from primary operation)

**Example:**
```
Primary Team: Infiltrate supplier facility, sabotage encryption modules
Backup Team: Standby for extraction OR execute signal processor sabotage (if encryption fails)

Operation Flow:
  - Primary team infiltrates, begins sabotage
  - Backup team monitors (ready to extract if compromised)
  - If primary succeeds: Backup team extracts primary team
  - If primary fails: Backup team executes alternative sabotage (signal processors)
```

---

#### Extraction Team (Rescue)

**Extraction Team Responsibilities:**
- Extract agents if compromised
- Provide cover fire (combat support during extraction)
- Evacuate to safety (safe houses, neutral territory)

**Example:**
```
Operation: Encryption module sabotage
Primary Team: Infiltrates facility, sabotages modules
Extraction Team: Standby 20km away, ready to extract if compromised

Compromise Scenario:
  Primary team detected, surrounded by security forces
  Extraction team activates: Provides diversion, extracts primary team
  Result: Primary team rescued, operation partially successful (some modules sabotaged)
```

---

#### Termination Team (Containment)

**Termination Team Responsibilities:**
- Eliminate agents if extraction impossible
- Prevent intelligence leaks (silence compromised agents)
- Maintain deniability (appears unrelated to employer)

**Example:**
```
Operation: Encryption module sabotage
Primary Team: Infiltrates facility, sabotages modules
Termination Team: Standby, ready to eliminate if extraction impossible

Compromise Scenario:
  Primary team captured, extraction impossible (heavily guarded facility)
  Termination team activates: Eliminates primary team (appears as facility security response)
  Result: Agents eliminated, intelligence protected, operation partially successful
```

---

### Layered Operations & Strategic Backups

**Complex operations target multiple objectives simultaneously:**

---

#### Primary + Backup Targets

**Operation Structure:**

**Primary Target (Main Objective):**
- Complex, high-value objective (sabotage critical system)
- Requires significant resources
- High risk, high reward

**Backup Targets (Strategic Backups):**
- Simpler, alternative objectives (if primary fails)
- Lower risk, moderate reward
- Provide fallback options

**Example:**
```
Primary Target: Sabotage military communications encryption (complex, high-value)
Backup Target 1: Sabotage signal processors (simpler, moderate value)
Backup Target 2: Sabotage targeting computers (simpler, moderate value)

Operation Flow:
  - Primary team attempts primary target
  - If primary succeeds: Exploit success, backup teams extract
  - If primary fails: Backup teams execute backup targets (alternative infiltration vectors)
```

---

#### High-Value Target Operations

**Operations backed up by high-value targets:**

**1. Ambassador/Envoy/Elite/Rulership Trips**

**Target:** High-ranking individuals traveling to neutral/contested/hostile areas.

**Objectives:**
- **Kidnapping:** Capture for intelligence, ransom, political leverage
- **Assassination:** Eliminate political/military leaders
- **Sabotage:** Compromise diplomatic missions
- **Intelligence Gathering:** Monitor negotiations, gather intel

**Strategic Backup:**
- If primary infiltration fails, target ambassador/elite trips (alternative objective)
- Ambassador trips provide access to high-value targets (rulers, elites)
- Success provides intelligence, political leverage, or elimination of key figures

**Example:**
```
Primary Operation: Sabotage encryption modules
Backup Operation: Target ambassador trip (if primary fails)

Ambassador Trip:
  - Ambassador travels to neutral territory for peace negotiations
  - Backup team intercepts: Kidnaps ambassador, extracts intelligence
  - Result: Alternative objective achieved (intel from ambassador compensates for failed sabotage)
```

---

**2. Bank/Guild Convoys**

**Target:** Convoys carrying valuables (gold, resources, rare items).

**Objectives:**
- **Interception:** Capture convoy, steal valuables
- **Sabotage:** Destroy convoy, economic damage
- **Intelligence:** Monitor convoy routes, gather economic intel

**Strategic Backup:**
- If primary operation fails, target convoys (economic disruption)
- Convoys provide valuable resources (fund future operations)
- Economic damage weakens target (reduces ability to counter-operations)

**Example:**
```
Primary Operation: Sabotage encryption modules
Backup Operation: Intercept bank convoy (if primary fails)

Bank Convoy:
  - Guild convoy carrying 10,000 gold, rare materials
  - Backup team intercepts: Captures convoy, steals valuables
  - Result: Alternative objective achieved (economic damage + funding for future operations)
```

---

**3. Rich Trade Routes**

**Target:** High-value trade routes (merchant caravans, shipping lanes).

**Objectives:**
- **Disruption:** Block trade routes, economic damage
- **Interception:** Capture shipments, steal resources
- **Intelligence:** Monitor trade patterns, gather economic intel

**Strategic Backup:**
- If primary operation fails, disrupt trade routes (economic warfare)
- Trade disruption weakens target economy
- Provides resources for future operations

**Example:**
```
Primary Operation: Sabotage encryption modules
Backup Operation: Disrupt trade routes (if primary fails)

Trade Route Disruption:
  - Key trade route between two nations
  - Backup team blocks route: Piracy, sabotage, interception
  - Result: Economic damage, trade disrupted, alternative objective achieved
```

---

**4. Remote Colonies**

**Target:** Remote colonies, outposts, frontier settlements.

**Objectives:**
- **Conquest:** Capture colony, expand territory
- **Sabotage:** Destroy colony infrastructure
- **Intelligence:** Monitor colony activities, gather intel

**Strategic Backup:**
- If primary operation fails, target remote colonies (territorial gains)
- Colonies are vulnerable (isolated, weaker defenses)
- Success provides strategic position, resources, or intelligence

**Example:**
```
Primary Operation: Sabotage encryption modules
Backup Operation: Target remote colony (if primary fails)

Remote Colony:
  - Frontier mining colony (isolated, weak defenses)
  - Backup team attacks: Captures colony, extracts resources
  - Result: Alternative objective achieved (territorial gain + resources)
```

---

### Multi-Operation Coordination

**Intelligent aggregate entities coordinate multiple operations:**

**Operation Hierarchy:**

**Tier 1: Primary Operations**
- Complex, high-value objectives
- Significant resource investment
- High strategic impact

**Tier 2: Backup Operations**
- Alternative objectives (if primary fails)
- Moderate resource investment
- Moderate strategic impact

**Tier 3: Diversionary Operations**
- Distract from primary operations
- Low resource investment
- Low strategic impact (but protects primary)

**Tier 4: Contingency Operations**
- Extract/terminate agents
- Protect intelligence
- Minimize exposure

**Example: Multi-Operation Coordination**
```
Primary Operation: Sabotage encryption modules (Tier 1)
  - Primary Team: Infiltrates supplier, sabotages modules
  - Extraction Team: Standby for extraction (Tier 4)
  - Termination Team: Standby for termination if extraction impossible (Tier 4)

Backup Operation 1: Target ambassador trip (Tier 2)
  - Backup Team 1: Monitors ambassador, ready to intercept if primary fails

Backup Operation 2: Intercept bank convoy (Tier 2)
  - Backup Team 2: Monitors convoy route, ready to intercept if primary fails

Diversionary Operation: False flag sabotage (Tier 3)
  - Diversion Team: Executes false operation (distracts from real objective)

Coordination:
  - If primary succeeds: All teams support success exploitation
  - If primary fails: Backup teams execute alternative objectives
  - If primary compromised: Extraction/termination teams activate
  - Diversionary operation runs throughout (maintains deception)
```

---

### Strategic Asset Management

**Intelligent aggregate entities manage strategic assets:**

**Asset Types:**

**Intelligence Assets:**
- Infiltration teams (primary, backup, extraction)
- Intelligence networks (informants, double agents)
- Surveillance capabilities (monitoring, reconnaissance)

**Combat Assets:**
- Extraction teams (combat-trained rescue units)
- Termination teams (assassination, elimination)
- Diversionary forces (false operations, distractions)

**Economic Assets:**
- Funding for operations (payroll, equipment, bribes)
- Resource acquisition (steal from targets, fund operations)
- Economic disruption (weaken target economy)

**Asset Allocation:**

```
Strategic Asset Allocation:
  Primary Operation: 40% of assets (main objective)
  Backup Operations: 30% of assets (alternative objectives)
  Contingency Operations: 20% of assets (extraction/termination)
  Diversionary Operations: 10% of assets (deception, distraction)
```

---

## Integration with Existing Systems

### Espionage & Infiltration Integration

**Supply chain sabotage uses existing infiltration systems:**
- Infiltrate supplier organizations (use infiltration mechanics)
- Gain access to production lines (infiltration levels, access tiers)
- Insert compromised components (sabotage operations)
- Maintain cover (infiltration detection, counter-intelligence)

---

### Combat System Integration

**Sabotage affects combat:**
- Weapon failures during combat (weapon sabotage)
- Communications jamming (communications sabotage)
- Equipment malfunctions (combat equipment sabotage)
- Electronic warfare (ECM/ECW layers affect combat systems)

---

### Economic System Integration

**Supply chains are economic:**
- Component production (economic activity)
- Outsourcing relationships (trade, economic dependencies)
- Supply chain disruptions (economic impact)
- Counter-measures cost (economic cost of security)

---

## Technical Considerations

### Components Needed

```csharp
// Supply chain component
public struct SupplyChainComponent : IComponentData
{
    public Entity FinalProduct;              // Item requiring components
    public BlobAssetReference<ComponentList> RequiredComponents; // List of required components
}

// Component entry (in supply chain)
public struct ComponentEntry : IBufferElementData
{
    public Entity ComponentEntity;           // Component item
    public Entity SupplierEntity;            // Who supplies this component
    public bool IsOutsourced;                // True if supplied from abroad
    public bool IsSabotaged;                 // True if component is sabotaged
    public SabotageType SabotageType;        // Type of sabotage (if sabotaged)
    public float SabotageSeverity;           // 0-1 (how severe sabotage is)
    public uint SabotageActivationTick;      // When sabotage activates (if scheduled)
}

// Sabotage operation
public struct SabotageOperation : IComponentData
{
    public Entity TargetEntity;              // Target organization
    public Entity SaboteurEntity;            // Who is sabotaging
    public Entity SupplierEntity;            // Supplier being infiltrated
    public SabotageType Type;                // Type of sabotage
    public float SuccessChance;              // 0-1 (chance of success)
    public SabotageOutcome Outcome;          // Current outcome state
    public bool IsDiscovered;                // True if target discovered sabotage
    public bool TargetPlayingAlong;          // True if target is counter-operating
}

// Counter-operation state
public struct CounterOperationState : IComponentData
{
    public Entity SabotageOperation;         // Sabotage being countered
    public CounterOperationType Type;        // PlayingAlong, Trap, DoubleBluff, etc.
    public float FalseInformationQuality;    // 0-1 (quality of false info being fed)
    public uint CounterActivationTick;       // When counter-operation activates
}
```

---

### Systems Needed

1. **SupplyChainAnalysisSystem** - Identifies supply chain vulnerabilities, infiltration vectors
2. **SupplyChainInfiltrationSystem** - Manages infiltration of supplier organizations
3. **ComponentSabotageSystem** - Inserts sabotaged components into supply chain
4. **SabotageActivationSystem** - Activates sabotage at appropriate times
5. **SabotageDetectionSystem** - Detects sabotage (quality control, intelligence, pattern analysis)
6. **SabotageOutcomeSystem** - Resolves sabotage outcomes (discovery timing, success level)
7. **CounterOperationSystem** - Manages counter-operations (playing along, traps, double bluffs)
8. **ElectronicWarfareSystem** - Handles ECM/ECW/ECCW/ECCCW layers
9. **DocumentVerificationSystem** - Detects document falsification, manages counter-operations
10. **FTLJumpSystem** - Integrates hyperjump denial sabotage and counter-operations

---

## Open Questions

1. **Discovery Probability:** How should discovery chances be calculated? Quality control effectiveness, intelligence capabilities, pattern analysis?
2. **Counter-Operation Detection:** How do saboteurs detect targets are "playing along"? Intelligence, pattern analysis, double agents?
3. **Multi-Layer Limits:** How many layers of counter-operations should be supported? (ECM/ECW/ECCW/ECCCW/...)
4. **Economic Impact:** How should supply chain disruptions affect production costs, timelines?
5. **Player Intervention:** Can players detect/initiate counter-operations? How much control?

---

## Related Documentation

- **Spies and Double Agents:** `Docs/Concepts/Villagers/Spies_And_Double_Agents.md` (infiltration, espionage)
- **Infiltration Detection:** `Docs/Concepts/Stealth/Infiltration_Detection_System.md` (infiltration mechanics)
- **Combat Systems:** `Docs/Concepts/Combat/` (weapon/equipment systems affected by sabotage)
- **Economic Systems:** `Docs/Concepts/Economy/` (supply chains, production, trade)

---

**For Implementers:** 
- Start with supply chain identification (what components are required, which are outsourced)
- Implement basic sabotage insertion (infiltrate supplier, insert compromised component)
- Add outcome resolution (discovery timing, success levels)
- Layer in counter-operations (playing along, traps) similar to ambush/counter-ambush system

**For Designers:** 
- Focus on creating interesting supply chain vulnerabilities (what can be sabotaged?)
- Design multi-layer counter-operation gameplay (sabotage → counter → counter-counter)
- Balance discovery probabilities (not too easy, not too hard)
- Consider economic impact (supply chain disruptions affect production)

---

**Last Updated:** 2025-12-21  
**Status:** Draft - Core concept defined, awaiting implementation decisions

