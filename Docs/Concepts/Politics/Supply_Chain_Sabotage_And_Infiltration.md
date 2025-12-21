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

**Counter-Operations:** Targets may detect sabotage and execute counter-operations (counter-sabotage, counter-espionage, playing along to feed false information). This creates multi-layer conflicts similar to ambush/counter-ambush systems.

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

