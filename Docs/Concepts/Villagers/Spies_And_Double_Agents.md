# Spies and Double Agents System

**Type:** Mechanic
**Status:** `<Draft>` - Core vision captured, implementation pending
**Version:** Concept v1.0
**Dependencies:** Elite Courts (Spymaster), Guild System, Entity Relations, Individual Progression (Finesse), Bands, Alignment Framework, Individual Combat System, Stealth & Perception System
**Last Updated:** 2025-11-07

---

## Overview

**Spies and double agents** are high-finesse individuals who lead covert bands to perform espionage, sabotage, assassination, theft, and infiltration missions for aggregate entities (villages, guilds, armies, elite families). They operate in shadows, using subtlety, subterfuge, and violence depending on alignment and outlook.

**Key Features:**
- **Spy Bands:** 3-12 member covert teams led by high-finesse spies
- **Mission Types:** Info gathering, assassination, sabotage, theft, heists, blackmail, bribery
- **Infiltration Methods:** Employment/trust-building OR brute force (bribing/killing guards)
- **Double Agents:** Infiltrators working within opposing aggregate entities
- **Family Seduction:** Target elite family members for secrets, kidnapping, assassination, or conversion
- **Spymaster Employment:** Guilds, villages, armies, and elites employ spies through spymasters
- **High Risk, High Reward:** Top-tier wages, completion bonuses, hidden reputation
- **Career Path:** Spies retire to become spymasters, diplomats, or criminals

**For Narrative:** John le Carré-style espionage (conflicted double agents), Mission Impossible heists (coordinated infiltration), Game of Thrones intrigue (spies seduce nobles for secrets).

---

## Spy Bands

### Overview

**Spy bands** are small, elite covert teams (3-12 members) specializing in espionage and black ops. Led by a high-finesse spy, they perform missions for employers (spymasters, nobles, guild masters).

**Composition:**

| Role | Count | Required Skills | Purpose |
|------|-------|----------------|---------|
| **Spy (Leader)** | 1 | Finesse 70+, Intelligence 60+ | Mission planning, infiltration, leadership |
| **Assassin** | 0-2 | Finesse 60+, Strength 50+ | Silent kills, combat backup |
| **Thief** | 1-3 | Finesse 70+, Perception 60+ | Lockpicking, safe-cracking, trap disarm |
| **Saboteur** | 0-2 | Intelligence 60+, Crafting 50+ | Explosive placement, supply destruction |
| **Seducer/Diplomat** | 0-1 | Charisma 70+, Finesse 60+ | Seduce targets, gain trust, extract secrets |
| **Informant** | 0-3 | Perception 70+, Stealth 60+ | Surveillance, reconnaissance, intel gathering |

**Band Size by Mission Complexity:**

```
Info Gathering (low risk):      3-5 members (spy + informants)
Theft/Heist (medium risk):      5-8 members (spy + thieves + lookouts)
Assassination (high risk):      6-10 members (spy + assassins + escape team)
Sabotage (high risk):           7-12 members (spy + saboteurs + combat backup)
Infiltration (extreme risk):    8-12 members (full team, coordinated operation)
```

### Recruitment

**Spies recruit from:**

```
High-Finesse Individuals:
  - Finesse 70+ required (stealth, lockpicking, agility)
  - Intelligence 60+ preferred (planning, problem-solving)
  - Perception 60+ for informants (awareness, pattern recognition)

Sources:
  - Thieves' Guild members (career criminals)
  - Assassins' Guild members (if guild exists)
  - Disgraced nobles (fallen elites seeking redemption or revenge)
  - Ex-military (soldiers with stealth training)
  - Mercenaries (for-hire killers)
  - Unemployed educated villagers (desperate for high-paying work)

Recruitment Conditions:
  - Spy pays 150% market rate (risk premium)
  - Relation +20 minimum (trust required for covert work)
  - Alignment tolerance: ±30 on moral axis (spies work with shady allies)
  - Outlook match: Vengeful/Bold preferred for violent missions
```

**Loyalty Mechanics:**

```
BandLoyalty =
  SalaryCompliance +
  SpyRelation +
  MissionSuccessRate +
  MembersAbandoned (negative) +
  SharedAlignment

SalaryCompliance:
  Paid on time (150% rate): +20
  Late payment: -10
  Missed payment: -30 (may desert)

SpyRelation:
  +50 (Close Friend): +15 loyalty
  +20 (Friendly): +5 loyalty
  0 (Neutral): 0
  -20 (Unfriendly): -10 loyalty (may betray)

MissionSuccessRate:
  80%+ success: +10 loyalty (confidence in leader)
  50-79% success: 0
  <50% success: -15 loyalty (incompetent leader)

MembersAbandoned:
  Each member left behind: -20 loyalty (betrayal)
  Each member rescued: +10 loyalty (leader cares)
  Each member silenced (killed to prevent capture): -30 loyalty (fear)

SharedAlignment:
  Good spy + Good member (peaceful missions): +5
  Evil spy + Evil member (assassinations): +5
  Good spy + Evil member (conflicting morals): -5
```

**If loyalty < -30:** 50% chance member deserts or betrays spy to authorities.

---

## Mission Types

### 1. Information Gathering (Benign Espionage)

**Purpose:** Assess village strength, resource nodes, productivity, alignment, military readiness.

**Method:**
```
Infiltration:
  - Pose as merchants, travelers, or pilgrims
  - Observe from distance (surveillance)
  - Bribe locals for information (10-50 currency)
  - Interview villagers (social engineering)

Duration: 1-3 days
Risk: Low (no violence, minimal detection risk)
Wage: 200-400 currency (base spy wage)
```

**Example:**
```
Village A (Lawful Good) hires spy to assess Village B

Spy infiltrates as merchant:
  - Observes 300 villagers, 50 guards, 2 blacksmiths
  - Notes iron mine (high productivity)
  - Detects Lawful Neutral alignment (compatible)
  - Reports: "Village B is strong but peaceful. Alliance viable."

Payment: 300 currency (2-day mission)
Reputation: No change (benign espionage)
```

### 2. Theft & Heists

**Purpose:** Steal valuables, documents, artifacts from secured locations.

**Method:**

```
Planning Phase (1-2 days):
  - Spy scouts target (building layout, guard patrols, entry points)
  - Identifies vulnerabilities (unlocked windows, bribeable guards)
  - Plans escape route

Execution Phase (1 night):
  Approach A (Stealth - Lawful Good/Neutral):
    - Lockpick doors (Finesse check)
    - Disable traps (Intelligence check)
    - Silent theft (no violence)
    - Escape before dawn

  Approach B (Brute Force - Chaotic/Evil):
    - Bribe guards (100-500 currency)
    - Incapacitate/kill guards if bribe fails
    - Use explosives to breach walls (loud, alerts village)
    - Fight way out if detected

Success Rate:
  Finesse 80+, Intelligence 70+, Luck: 70% success
  Finesse 60-79, Intelligence 50-69: 50% success
  Finesse <60: 30% success (high detection risk)
```

**Consequences:**

```
Success (undetected):
  - Spy gains loot (gold, documents, artifacts)
  - Employer pays completion bonus (+50% wage)
  - Reputation: No change (anonymous)

Partial Success (detected during escape):
  - Spy escapes but identified
  - Target entity relation: -40 (theft victim)
  - Wanted status in target village (arrest on sight)

Failure (captured):
  - Spy imprisoned or executed (alignment-dependent)
  - Employer identity may be revealed (-40 employer-target relation)
  - Spy reputation: -20 (failed mission)
```

**Example:**
```
Duke Aldric (Lawful Evil) hires spy to steal Baron Erik's land deed

Spy band (6 members):
  - Spy (leader, Finesse 85)
  - 2 Thieves (lockpicking)
  - 2 Informants (lookout)
  - 1 Assassin (backup)

Execution:
  - Night infiltration of Baron's manor
  - Thief lockpicks study door (success)
  - Spy locates deed in safe (Intelligence check: success)
  - Thief cracks safe (Finesse check: success)
  - Escape via window before guards notice

Result:
  - Deed stolen (Duke gains legal leverage)
  - Baron never knows who stole it (no relation penalty)
  - Spy paid 800 currency (400 base + 400 bonus)

Narrative: "The deed vanished in the night. Baron Erik suspected Duke Aldric but had no proof. The courts ruled in Aldric's favor."
```

### 3. Assassination

**Purpose:** Eliminate high-value targets (nobles, guild masters, enemy leaders, rival courtiers).

**Method:**

```
Target Types:
  - Elite family members (nobles, merchants, scholars)
  - Courtiers (stewards, diplomats, rival spymasters)
  - Guild masters (eliminate competition)
  - Military leaders (weaken enemy armies)
  - Double agents (silence traitors)

Approach A (Silent Assassination - Lawful/Neutral):
  - Poison target's food (Intelligence check)
  - Sniper kill from distance (Finesse + Perception)
  - Staged accident (push from balcony, "accidental" fire)
  - No witnesses, minimal violence

Approach B (Message Assassination - Chaotic/Evil):
  - Public execution (duel, ambush in street)
  - Brutal murder (torture, mutilation)
  - Family killed as well (send message)
  - Employer wants target to KNOW who sent assassins

Success Rate:
  Finesse 80+, Stealth 70+: 80% silent kill
  Finesse 60-79: 60% success, 20% detected
  Finesse <60: 40% success, 40% detected, 20% fail and captured
```

**Consequences:**

```
Success (silent, undetected):
  - Target dies, no one knows who hired assassin
  - Employer pays 1000-3000 currency (high-risk mission)
  - Spy reputation: No change (anonymous)

Success (message assassination):
  - Target dies publicly, message sent
  - Target family/village relation to employer: -80 (murder grudge)
  - Employer relation to target village: -60 (known assassin employer)
  - Spy reputation: +10 (feared assassin, attracts future clients)

Failure (assassin captured):
  - Assassin executed or imprisoned
  - Employer revealed if assassin interrogated (Charisma check)
  - Employer-target relation: -90 (assassination attempt)
  - Diplomatic crisis or war may result
```

**Example:**
```
Merchants' Guild hires spy to assassinate rival guild master

Target: Guild Master Theron (Heroes' Guild, rival for contracts)

Spy band (8 members):
  - Spy (leader, Finesse 90)
  - 2 Assassins (killers)
  - 1 Seducer (gain access to Theron's quarters)
  - 2 Thieves (lockpicking, escape routes)
  - 2 Informants (surveillance, guard schedules)

Execution:
  - Seducer befriends Theron's servant (Charisma check: success)
  - Servant grants access to private quarters
  - Assassin poisons Theron's wine (Intelligence check: success)
  - Theron dies in sleep (appears natural)

Result:
  - Theron dead, Heroes' Guild leadership crisis
  - No suspicion of foul play (perfect execution)
  - Merchants' Guild gains contracts
  - Spy paid 2500 currency (1500 base + 1000 bonus)

Narrative: "Guild Master Theron died peacefully in his sleep. The city mourned. The Merchants' Guild quietly celebrated."
```

### 4. Sabotage

**Purpose:** Destroy supplies, buildings, military equipment, or infrastructure to weaken target.

**Targets:**

```
Military Sabotage:
  - Destroy army supply wagons (food, weapons)
  - Burn siege equipment (catapults, rams)
  - Poison water supplies (causes illness, -50% army strength)
  - Kill horses (cavalry disabled)

Economic Sabotage:
  - Burn warehouses (destroy village food stores)
  - Sabotage blacksmith forges (halt weapon production)
  - Destroy trade caravans (economic damage)
  - Contaminate granaries (famine)

Infrastructure Sabotage:
  - Burn temples (reduce god relation, prayer generation)
  - Destroy bridges (isolate village)
  - Collapse mines (halt resource production)
  - Burn libraries (destroy knowledge, education)
```

**Method:**

```
Approach A (Explosives - Chaotic/Neutral):
  - Saboteur places explosives (Crafting check)
  - Timed detonation (Intelligence check for timing)
  - Loud, obvious destruction (alerts guards)
  - Fast escape required

Approach B (Subtle Sabotage - Lawful/Neutral):
  - Poison supplies (food, water)
  - Structural weakening (buildings collapse later)
  - Arson during night (fire spreads before detected)
  - Appears accidental or natural

Detection Risk:
  Explosives: 70% detected immediately (loud)
  Subtle: 30% detected within 1 week
```

**Consequences:**

```
Success:
  - Target loses resources, buildings, military strength
  - Economic/military impact (e.g., -1000 food, -50 soldiers)
  - Employer pays 600-1500 currency (based on damage)

Detected (but escaped):
  - Target knows sabotage occurred
  - Increases security (future missions harder)
  - Target-employer relation: -50 (if employer suspected)

Failure (captured):
  - Saboteur executed
  - Employer revealed
  - War or retaliation likely
```

**Example:**
```
Village A (at war with Village B) hires spy to sabotage B's army

Target: Village B's supply depot (1000 food, 500 weapons)

Spy band (10 members):
  - Spy (leader, Finesse 80)
  - 3 Saboteurs (explosives)
  - 2 Assassins (kill guards)
  - 3 Thieves (lockpicking, entry)
  - 2 Informants (lookout, warn of patrols)

Execution:
  - Night infiltration via forest route
  - Thieves lockpick depot gate
  - Assassins silent-kill 2 guards
  - Saboteurs place explosives on food stores
  - Detonate and escape before alarm raised

Result:
  - Depot destroyed (1000 food, 500 weapons lost)
  - Village B army starves, weakened by 40%
  - Village A wins war 2 weeks later
  - Spy paid 1200 currency (600 base + 600 bonus)

Narrative: "The explosion lit the night sky. Village B's army marched on empty stomachs. They surrendered before reaching the battlefield."
```

### 5. Blackmail & Bribery

**Purpose:** Gain leverage over high-ranking officials (courtiers, nobles, council members) through secrets or payments.

**Method:**

```
Blackmail:
  - Spy investigates target (Perception checks)
  - Discovers secrets:
    * Affair with rival family member
    * Embezzlement from village treasury
    * Illegal business dealings
    * Hidden evil alignment (if village Lawful Good)
    * Bastard children

  - Threatens exposure unless target complies:
    * Vote for employer's policy
    * Leak rival's secrets
    * Sabotage from within
    * Resign from position

Bribery:
  - Spy offers payment (500-5000 currency) to target
  - Target accepts based on:
    * Moral alignment (Evil more likely, Good resists)
    * Wealth (poor targets more tempted)
    * Relation to employer (enemies refuse)
    * Risk of exposure (high risk = high price)

BriberyAcceptance =
  PaymentOffer +
  TargetWealth (negative) +
  MoralAlignment +
  RiskAssessment

PaymentOffer:
  500 currency: +10
  1000 currency: +20
  2000 currency: +35
  5000 currency: +60

TargetWealth (inverse):
  Destitute: +30 (desperate)
  Poor: +20
  Medium: 0
  High: -10 (not tempted)
  Ultra High: -30 (insult to offer bribe)

MoralAlignment:
  Evil (+60 moral): +30 (no qualms)
  Neutral (0 moral): +10 (pragmatic)
  Good (+60 moral): -20 (resists corruption)

RiskAssessment:
  Low risk (secret meeting): +10
  Medium risk (witnesses possible): 0
  High risk (public, could be caught): -20

If acceptance > +30: Target accepts bribe
If acceptance 0-29: Target refuses but keeps quiet
If acceptance < 0: Target reports bribe attempt to authorities
```

**Consequences:**

```
Blackmail Success:
  - Target complies with demands
  - Employer gains political leverage
  - Target relation to spy: -60 (hates blackmailer)
  - If secret revealed anyway: Target seeks revenge (-80 relation)

Blackmail Failure:
  - Target calls bluff, refuses
  - Spy must follow through (reveal secret) or lose credibility
  - Target may hire counter-spy to eliminate blackmailer

Bribery Success:
  - Target votes/acts in employer's favor
  - Employer gains political victory
  - Target becomes corrupt (moral axis shifts -5 toward evil)

Bribery Failure (reported):
  - Spy arrested for bribery attempt
  - Employer exposed (-50 relation with target village)
  - Scandal damages employer's reputation (-20)
```

**Example:**
```
Duke Aldric hires spy to blackmail Baron Erik's steward

Target: Steward Gareth (manages Baron's finances)

Spy Investigation:
  - Follows steward for 2 weeks
  - Discovers: Gareth embezzling 100 currency/month
  - Evidence: Falsified ledgers, secret wealth stash

Blackmail:
  Spy: "I know about the embezzlement. Work for me or I tell the Baron."

  Gareth's decision:
    - Exposure = execution (Baron is Lawful Good, harsh on theft)
    - Compliance = survive, keep stealing
    - Choice: Comply

  Gareth leaks Baron's military plans to Duke

Result:
  - Duke knows Baron's troop movements
  - Duke ambushes Baron's army, wins battle
  - Gareth continues embezzling, now also traitor
  - Spy paid 800 currency

Narrative: "Steward Gareth had a choice: honor or survival. He chose survival. The Baron never knew his closest advisor was a traitor."
```

### 6. Heists (Complex Multi-Stage Missions)

**Purpose:** Steal high-value artifacts, relics, or documents from heavily guarded locations.

**Heist Phases:**

```
Phase 1: Planning (2-5 days)
  - Scout target location (guard patrols, entry points)
  - Identify security measures (locks, traps, magic wards)
  - Plan entry, theft, and escape routes
  - Recruit specialists (thieves, saboteurs, combat backup)

Phase 2: Infiltration (1 night)
  - Approach via stealth or bribery
  - Bypass first layer of security (walls, gates)
  - Neutralize guards (silent kills, knockout, or bribery)

Phase 3: Execution (2-4 hours)
  - Lockpick/explosives to reach target
  - Disable traps (Intelligence checks)
  - Steal target item (artifact, document, treasure)

Phase 4: Escape
  - Exit before alarm raised (optimal)
  - Fight way out if detected (sub-optimal)
  - Abandon members if necessary (decision point)

Decision Points During Heist:
  Member Detected:
    - Rescue (delays escape, +10 loyalty, higher detection risk)
    - Abandon (faster escape, -20 loyalty, member captured)
    - Silence (kill member, -30 loyalty, no intel leaked)

  Alarm Raised:
    - Fight guards (high risk, possible casualties)
    - Flee empty-handed (mission failure, no loot)
    - Hostage-taking (grab noble, negotiate escape)

  Loot Division:
    - Fair split (equal shares, +10 loyalty)
    - Leader takes majority (spy greed, -10 loyalty)
    - Betray members (spy takes all, kills team, -80 relation, wanted)
```

**Example Heist:**

```
Thieves' Guild hires spy to steal Crown of Kings from royal vault

Target: Duke's heavily guarded vault
Security: 20 guards, 3 locked doors, trapped chest, magic ward

Spy band (12 members):
  - Spy (leader, Finesse 95)
  - 4 Thieves (lockpicking, trap disarm)
  - 3 Assassins (silent kills)
  - 2 Saboteurs (explosives as backup)
  - 2 Informants (surveillance)
  - 1 Seducer (bribed inside guard)

Planning (3 days):
  - Seducer befriends guard, learns patrol schedule
  - Informants map vault location
  - Thieves study lock types (3-tumbler locks, trapped chest)

Execution (1 night):
  Phase 1: Infiltration
    - Bribed guard unlocks side gate (200 currency)
    - Team enters via servant quarters (avoided main guard)

  Phase 2: Approach Vault
    - Assassins silent-kill 2 corridor guards
    - Thieves lockpick 1st door (Finesse check: success)
    - Thieves lockpick 2nd door (Finesse check: success)

  Phase 3: Vault Entry
    - Thieves disable magic ward (Intelligence check: success)
    - Thieves disarm trap on chest (Finesse check: success)
    - Thieves lockpick final lock (Finesse check: FAIL, alarm triggered)

  Decision: Alarm raised, guards approaching
    - Spy: "Grab crown, fight our way out!"
    - Assassins engage guards (3 guards killed, 1 assassin wounded)
    - Team escapes via pre-planned tunnel route
    - 1 thief captured during escape

  Decision: Rescue captured thief?
    - Spy: "Too risky, he's on his own." (Abandon)
    - Loyalty: -20 (team sees leader abandoned member)
    - Captured thief tortured, reveals Thieves' Guild employer

Result:
  - Crown stolen (worth 10,000 currency)
  - Thieves' Guild exposed (Duke relation: -70)
  - Spy paid 5000 currency (2000 base + 3000 bonus)
  - 1 thief executed by Duke
  - Spy band loyalty: -20 (abandonment)

Narrative: "They got the crown, but at a cost. The thief left behind screamed their names as guards dragged him away. The crew never trusted their leader again."
```

---

## Double Agents

### Overview

**Double agents** are infiltrators who embed themselves within opposing aggregate entities (villages, guilds, courts) to sabotage from within, shift alignment/outlook, or gather intelligence for external forces.

**Formation:**

```
Path 1: Genuine Conversion
  - Individual joins opposing entity (e.g., Good villager joins Evil village)
  - Initially loyal, but conflicting interests trigger doubt
  - Gradually shifts toward original alignment or external employer

Path 2: Planted Agent
  - Spy deliberately infiltrates target entity
  - Pretends loyalty while secretly working for employer
  - Long-term deep cover (months to years)

Path 3: Turned Agent
  - Current member recruited by external force (blackmail, bribery, ideology)
  - Becomes traitor while maintaining cover
  - Double life (public loyalty, secret betrayal)
```

### Infiltration Process

**Phase 1: Entry (1-6 months)**

```
Method A: Employment
  - Apply for position in target entity (steward, guard, council member)
  - Build trust with leaders (relation checks)
  - Perform duties competently (no suspicion)

Method B: Marriage
  - Seduce/marry into elite family
  - Gain access to court, secrets, resources
  - Use spouse as unwitting pawn or sincere conversion target

Method C: Rescue/Heroism
  - Stage or exploit crisis (bandits, fire, plague)
  - "Save" target entity members
  - Gain trust as hero, offered position

Method D: Ideological Appeal
  - Claim conversion to target alignment (pretend to be reformed)
  - Join peacefully, cite disillusionment with former life
  - Slow trust-building over time
```

**Phase 2: Trust Building (6 months - 3 years)**

```
Actions to Build Trust:
  - Perform duties excellently (reputation +1/month)
  - Befriend leaders (relation +5/month via gifts, aid)
  - Marry into family (if seduction route)
  - Defend entity from external threats (prove loyalty)
  - Denounce former allegiance (public rejection of old values)

TrustLevel =
  TimeServed +
  ReputationInEntity +
  RelationWithLeaders +
  ProvenLoyalty (defending entity) +
  MarriageBond (if married in)

Trust Threshold:
  <30: Outsider (watched, no access to secrets)
  30-60: Accepted (regular member, minor secrets)
  60-80: Trusted (inner circle, major secrets)
  80+: Inner Circle (court access, full trust)

Double Agent Goal: Reach 60+ trust to access valuable intel/positions
```

**Phase 3: Espionage/Sabotage (Ongoing)**

```
Once trust ≥ 60, double agent can:

Intelligence Gathering:
  - Leak troop movements to employer
  - Report economic data (resources, wealth)
  - Expose political factions (undermining opportunities)
  - Identify vulnerable targets (nobles, courtiers)

Sabotage:
  - Poison supplies (subtle, blamed on accident)
  - Embezzle funds (weaken entity economically)
  - Spread rumors (sow discord, lower morale)
  - Assassinate key figures (staged accidents)

Alignment Shifting (if Peaceful/Forgiving target):
  - Advocate for policy changes (shift laws toward employer's alignment)
  - Influence council votes (gradual ideological drift)
  - Convert individuals (persuade members to switch sides)
  - 5-10 year process to shift village alignment +10-20

Eradication (if Warlike/Vengeful employer):
  - Weaken defenses before employer's invasion
  - Assassinate leaders to create chaos
  - Open gates during siege (betrayal)
  - Destroy entity from within (total collapse)
```

### Conflicting Interests & Commitment Checks

**Double agents periodically check commitment to employer vs infiltrated entity:**

```
CommitmentCheck (every 3-6 months):

FactorsForEmployer:
  - High payment (1000+ currency/month): +20
  - Shared alignment with employer: +15
  - Hatred of infiltrated entity: +30
  - Fear of employer (blackmail, threats): +10

FactorsForInfiltratedEntity:
  - Genuine friendships formed (relation +60+): +25
  - Married into family (love, not just seduction): +40
  - Aligned with entity's values (ideology): +30
  - Children born in entity (family bonds): +50

If FactorsForEmployer > FactorsForInfiltrated:
  → Agent remains loyal to employer, continues mission

If FactorsForInfiltratedEntity > FactorsForEmployer:
  → Agent genuinely converts, becomes loyal to infiltrated entity
  → May confess to leaders OR become triple agent (feed false intel to employer)

If roughly equal:
  → Conflicting loyalties (narratively rich, unstable)
  → Agent may freeze, sabotage both, or have breakdown
```

**Example Commitment Shift:**

```
Agent: Elira (Lawful Good spy infiltrated Chaotic Evil cult)

Initial:
  - Employer: Lawful Good village (FactorsForEmployer = +45)
  - Mission: Expose cult's leaders for prosecution

Year 1:
  - Elira joins cult, pretends to worship demon lord
  - Befriends cult members (relation +30)
  - Trust: 40 (outsider, limited access)

Year 2:
  - Cult leader trusts Elira (+60 trust, inner circle)
  - Elira witnesses cult's atrocities (sacrifices, murder)
  - Hatred of cult: +30 (FactorsForEmployer = +75)

Year 3:
  - Cult member Marcus befriends Elira (relation +70)
  - Marcus is kind, conflicted, secretly hates cult violence
  - Elira falls in love with Marcus (genuine bond)
  - FactorsForInfiltrated: +25 (friendship)

Commitment Check:
  Employer: +75 (mission, alignment, hatred of cult)
  Infiltrated: +25 (Marcus friendship)

  Result: Still loyal to employer, continues mission

Year 4:
  - Elira and Marcus marry (seduction became love)
  - Marcus confesses he wants to escape cult
  - Elira considers revealing mission to Marcus

  FactorsForInfiltrated: +65 (marriage +40, genuine love)

Commitment Check:
  Employer: +75
  Infiltrated: +65

  Result: Conflicting loyalties (nearly equal)

  Elira's Decision:
    - Reveals truth to Marcus: "I'm a spy. I'm here to destroy the cult. Come with me."
    - Marcus: "I've wanted to leave for years. Let's escape together."
    - Elira and Marcus defect, expose cult to employer
    - Cult destroyed, leaders executed

Final Outcome:
  - Elira successfully completed mission (cult destroyed)
  - But via genuine conversion (love, not espionage)
  - Marries Marcus, both join Lawful Good village

Narrative: "She went in as a spy. She left with a husband. Love turned espionage into redemption."
```

### Detection & Consequences

**How double agents are caught:**

```
Detection Triggers:
  - Failed Finesse check (caught stealing documents): 30% exposure
  - Witnessed sabotage (seen poisoning supplies): 80% exposure
  - Interrogation of accomplice (spy reveals double agent): 60% exposure
  - Counter-spy investigation (target hires rival spy to find mole): 50% exposure
  - Behavioral inconsistency (acts out of alignment too often): 20% exposure

Suspicion System:
  - Each suspicious act: +10 suspicion
  - Suspicion ≥ 50: Entity investigates agent
  - Suspicion ≥ 80: Arrest and interrogation

Suspicious Acts:
  - Caught in restricted area: +15 suspicion
  - Refuses to participate in entity's core rituals (worship, oaths): +10
  - Advocates for opposing policies too aggressively: +5/month
  - Friends with known enemies of entity: +20
  - Unexplained wealth (bribed by employer): +25
```

**Consequences if Caught:**

```
Infiltrated Entity Alignment Response:

Lawful Good:
  - Trial (evidence required)
  - If guilty: Imprisonment, exile, or execution (based on severity)
  - If innocent: Apology, compensation for false accusation

Lawful Evil:
  - Show trial (predetermined guilt)
  - Public execution (example to others)
  - Torture family to extract employer's identity

Chaotic Good:
  - Quick judgment (leader decides)
  - Exile or execution (based on damage done)
  - May forgive if genuine regret shown

Chaotic Evil:
  - Brutal torture (extract all information)
  - Public execution (spectacle, dismemberment)
  - Hunt down employer and all accomplices (vendetta)
```

**Example Detection:**

```
Double Agent: Kael (planted by Duke Aldric in Baron Erik's court)

Mission: Sabotage Baron's economy, steal trade secrets

Year 1-2:
  - Kael works as steward (manages finances)
  - Embezzles 50 currency/month (subtle)
  - Leaks trade routes to Duke
  - Trust: 70 (inner circle)

Year 3:
  - Baron hires counter-spy to investigate missing funds
  - Counter-spy follows Kael, witnesses meeting with Duke's messenger
  - Evidence: Kael receiving payment, documents exchanged
  - Suspicion: 100 (confirmed traitor)

Baron's Response (Lawful Good):
  - Arrests Kael publicly
  - Trial held (evidence presented)
  - Kael confesses under oath (Charisma check: failed)
  - Sentence: 10 years imprisonment

  - Baron confronts Duke Aldric (relation: -80, "You sent a spy?")
  - Duke denies (Charisma check: success, "Kael acted alone")
  - Relations strained but no war (insufficient proof of Duke's involvement)

Kael's Fate:
  - Imprisoned for 10 years
  - Duke disavows Kael (no rescue attempt, too risky)
  - Kael bitter, eventually released, becomes mercenary

Narrative: "Kael spent a decade in chains, abandoned by the man who sent him. When he emerged, he served no one but himself."
```

---

## Family Targeting & Seduction

### Seduce for Secrets

**Method:**

```
Target: Elite family members (nobles' children, spouses, courtiers' families)

Seduction Process:
  1. Identify vulnerable target (lonely, unhappy marriage, ambitious)
  2. Befriend target (relation +20 via gifts, flattery)
  3. Romantic overture (Charisma check, Finesse check)
  4. Affair begins (relation +60, trust builds)
  5. Extract secrets during pillow talk (Intelligence check)

Secrets Gained:
  - Family financial status (wealth, debts)
  - Political alliances (who supports whom)
  - Military plans (troop movements, defenses)
  - Blackmail material (affairs, illegitimate children, crimes)
  - Vault locations (for heists)

Success Rate:
  Charisma 70+, Finesse 70+, Target lonely: 70% seduction success
  Charisma 50-69, Finesse 60-69: 40% success
  Charisma <50: 20% success (target rejects, may report)
```

**Outcomes:**

```
Success (secrets extracted, affair ends):
  - Spy gains valuable intel
  - Target heartbroken (relation: -40, "I was used")
  - If target discovers truth: Vendetta (-80 relation, may hire assassin)

Success (genuine love develops):
  - Spy and target fall in love (unplanned)
  - Spy faces dilemma: Betray love or employer
  - May lead to elopement, marriage, or tragedy

Failure (target rejects):
  - Seduction attempt reported to family
  - Family relation to spy's employer: -30 (suspicious)
  - Spy's reputation: -10 (failed mission)

Failure (target is counter-spy):
  - Target was bait (family suspected spy)
  - Spy captured, interrogated
  - Employer exposed
```

**Example:**

```
Spy: Theron (hired by rival Duke to steal Baron's plans)

Target: Lady Elira (Baron's daughter, age 25, unmarried, lonely)

Seduction:
  Month 1: Theron poses as merchant, meets Elira at market
  Month 2: Theron sends gifts (flowers, poems) (relation +10)
  Month 3: Theron dances with Elira at festival (Charisma check: success, relation +20)
  Month 4: Affair begins (relation +60)

  During affair:
    Elira (pillow talk): "Father is planning to marry me to Duke Aldric."
    Theron: "When?"
    Elira: "Three months. I dread it. Aldric is cruel."

  Theron reports to employer: "Baron plans alliance with Aldric in 3 months."
  Employer accelerates rival plans, disrupts alliance

Month 5:
  - Theron's mission complete, prepares to leave
  - Elira: "Where are you going? I love you."
  - Theron (genuine regret): "I'm sorry. I was never real."
  - Elira (heartbroken): "You used me."

  Theron leaves, Elira devastated (relation: -80, "I'll never trust again")

Consequence:
  - Theron paid 600 currency
  - Elira becomes vengeful (Behavioral shift: +30 Vengeful)
  - 5 years later, Elira hires assassin to kill Theron (revenge)

Narrative: "She gave him her heart. He gave her lies. Years later, she gave him death."
```

### Kidnap for Ransom

**Method:**

```
Target: Elite family members (children, spouses, heirs)

Kidnapping Process:
  1. Identify high-value target (heir, beloved child)
  2. Surveil target's routine (find vulnerability)
  3. Abduct during isolated moment (carriage ambush, night raid)
  4. Demand ransom (gold, political concessions, prisoner exchange)

Ransom Demands:
  - Monetary: 5000-50,000 currency (based on family wealth)
  - Political: Vote for policy, step down from council, grant land
  - Prisoner Exchange: Release jailed ally in exchange for hostage
  - Information: Reveal secrets in exchange for safe return
```

**Family Response:**

```
Alignment-Based Response:

Lawful Good:
  - Pays ransom (values family life above wealth)
  - Negotiates for safe return
  - Tracks kidnappers after release (bring to justice)

Lawful Evil:
  - Pays ransom (pragmatic)
  - Hunts kidnappers after return (execution)
  - May double-cross (ambush during exchange)

Chaotic Good:
  - Refuses ransom (won't reward crime)
  - Hires heroes to rescue hostage
  - Vengeance against kidnappers (kill on sight)

Chaotic Evil:
  - Refuses ransom (hostage expendable)
  - OR pays then hunts kidnappers (torture, kill families)
```

**Consequences:**

```
Ransom Paid, Hostage Released:
  - Kidnappers gain wealth/concessions
  - Family relation to kidnappers: -90 (will seek revenge)
  - Hostage trauma (may develop PTSD, alignment shift)

Ransom Refused, Hostage Killed:
  - Family grief (morale -20)
  - Family may collapse (if heir killed)
  - Employer reputation: +10 (feared, ruthless)

Rescue Attempt Successful:
  - Hostage freed
  - Kidnappers killed or captured
  - Family gratitude (relation +40 to rescuers)

Rescue Attempt Failed:
  - Hostage killed in crossfire
  - Family blames rescuers AND kidnappers
```

**Example:**

```
Kidnappers: Spy band hired by rival Duke

Target: Baron's son (age 10, heir to barony)

Execution:
  - Spy ambushes carriage during forest travel
  - Kills guards (4 soldiers)
  - Abducts boy, brings to hideout
  - Sends ransom note: "10,000 currency or the boy dies"

Baron's Response (Lawful Good):
  - Pays ransom immediately (son's life > gold)
  - Exchange arranged at bridge (neutral ground)

Exchange:
  - Baron brings gold, kidnappers bring boy
  - Trade successful, boy released unharmed
  - Kidnappers flee

Aftermath:
  - Baron hires Heroes' Guild to hunt kidnappers
  - 6 months later, guild finds hideout
  - Kidnappers killed in raid (justice served)
  - Baron: "My son is alive. The kidnappers are dead. Justice."

Narrative: "They took his son. He paid the price. But he never forgot. Six months later, they died screaming."
```

### Assassinate to Send Message (Chaotic)

**Purpose:** Terrorize target family, demonstrate power, provoke response.

**Method:**

```
Target: Family member (not necessarily high-value, symbolic)

Execution (Chaotic Evil):
  - Public murder (market square, festival)
  - Brutal method (beheading, immolation, torture)
  - Message left (note, symbol carved into body)
  - Example: "This is what happens to those who defy us."

Intended Effect:
  - Fear (target family and allies)
  - Provoke irrational response (emotional decision-making)
  - Demonstrate ruthlessness (attract fearful allies)
```

**Consequences:**

```
Target Family Response:

Lawful Good:
  - Mourn publicly (funeral, honors)
  - Seek justice (legal prosecution of killers)
  - Relation to employer: -100 (eternal enemies)

Chaotic Good:
  - Immediate vengeance (hunt killers, no trial)
  - Blood feud (multi-generational hatred)

Chaotic Evil:
  - Escalation (kill employer's family in return, more brutally)
  - Spiral of violence (tit-for-tat murders)
```

**Example:**

```
Assassin: Hired by Chaotic Evil warlord to intimidate Baron

Target: Baron's wife (symbolic, non-combatant)

Execution:
  - Assassin infiltrates Baron's estate
  - Murders wife in bedroom (stabbing)
  - Carves warlord's symbol into wall with blood
  - Escapes, leaves message: "Your family dies until you submit."

Baron's Response (Lawful Good):
  - Devastated (morale -50, relation to warlord: -100)
  - Declares war on warlord (total commitment)
  - Hires every hero, mercenary, guild to hunt warlord
  - 2-year war, Baron eventually wins (warlord executed)

Aftermath:
  - Baron never remarries (mourns forever)
  - Baron's son inherits hatred of warlord's faction
  - Generational vendetta continues for 50+ years

Narrative: "They killed his wife to break him. Instead, they forged an enemy who would never rest until every one of them was dead."
```

### Turn Family Member to Cause

**Method:**

```
Target: Discontented family member (younger sibling, disinherited heir, unhappy spouse)

Recruitment Process:
  1. Identify grievance (passed over for inheritance, abused, ideological conflict)
  2. Befriend target, offer sympathy (relation +30)
  3. Offer solution: "Work with me, I'll help you get what you deserve"
  4. Recruit as ally (pawn or sincere partnership)

Recruitment Types:

Pawn (Manipulated):
  - Target believes spy is genuine ally
  - Spy uses target for info, access, then discards
  - Target betrayed when no longer useful (relation: -80, vendetta)

Sincere Ally (Genuine Partnership):
  - Spy and target share goals (overthrow corrupt father, reform family)
  - Mutual benefit relationship
  - May lead to marriage, business partnership, or alliance
```

**Example (Pawn):**

```
Spy: Marcus (hired by rival Duke)

Target: Baron's younger son (disinherited, bitter)

Recruitment:
  Marcus: "Your brother gets everything. You get nothing. That's not fair."
  Son: "I know. But what can I do?"
  Marcus: "Help me. I'll help you take what's yours."

  Son agrees, leaks family secrets to Marcus
  Marcus reports to Duke, Duke uses intel to bankrupt Baron

Result:
  - Baron financially ruined
  - Younger son expects reward from Marcus
  - Marcus: "Our deal is done. You're on your own."
  - Son (betrayed): "You used me!"

  Son becomes vengeful (hires assassin to kill Marcus)

Narrative: "He thought they were partners. He was just a tool."
```

**Example (Sincere Ally):**

```
Spy: Elira (hired by Lawful Good village to reform Chaotic Evil noble family)

Target: Noble's daughter (secretly Good alignment, hates family's cruelty)

Recruitment:
  Elira: "You don't belong here. You're not like them."
  Daughter: "I've wanted to leave for years. But they'll kill me if I try."
  Elira: "Not if we do it together. Help me expose your family's crimes. I'll protect you."

  Daughter agrees, provides evidence of father's atrocities
  Elira brings evidence to Lawful Good authorities
  Father arrested, executed for war crimes

Result:
  - Daughter freed from family
  - Joins Lawful Good village, starts new life
  - Elira and daughter become lifelong friends (relation +80)

Narrative: "She wasn't just a spy. She was a liberator. Together, they destroyed evil and built something good."
```

---

## Spymaster Employment

### Spymaster Role (Court Position)

**Spymasters** are high-Intelligence, high-Finesse courtiers who manage spy networks for elite families, guilds, or villages.

**Responsibilities:**

```
1. Recruit Spies
   - Identify high-Finesse individuals (Finesse 70+)
   - Negotiate wages (150-200% market rate)
   - Build spy network (3-20 spies, based on employer wealth)

2. Issue Missions
   - Assess threats (rival nobles, enemy villages, rebellious vassals)
   - Design missions (info gathering, assassination, sabotage)
   - Assign spies based on skillset (thief vs assassin vs seducer)

3. Manage Operations
   - Track mission progress (success/failure rates)
   - Pay spies (completion bonuses, failure penalties)
   - Handle captured spies (rescue, disavow, or silence)

4. Analyze Intelligence
   - Compile spy reports (troop movements, economic data, secrets)
   - Present to employer (lord, guild master, council)
   - Recommend actions (preemptive strike, alliance, sabotage)

5. Counter-Espionage
   - Detect enemy spies (counter-spy investigations)
   - Root out double agents (interrogations, loyalty tests)
   - Protect employer from infiltration
```

**Spymaster Stats:**

```
Required:
  - Intelligence 70+ (strategic thinking, analysis)
  - Finesse 60+ (understands espionage tradecraft)
  - Charisma 50+ (recruit and manage spies)

Preferred:
  - Perception 70+ (detect lies, spot infiltrators)
  - Former spy background (knows operational risks)
  - Ruthless outlook (willing to abandon captured spies)
```

**Corruption Risk:**

```
Spymaster Corruption = High (75% over career)

Temptations:
  - Sell secrets to rival employers (double agent spymaster)
  - Embezzle spy budgets (skim 10-20% off top)
  - Blackmail employer (use gathered intel for leverage)
  - Frame rivals (fabricate evidence, false reports)

Prevention:
  - Employer monitors spymaster (relation checks, audits)
  - Pay high salary (reduce financial temptation)
  - Loyalty tests (plant false intel, see if spymaster leaks)
```

### Mission Assignment by Employer Type

**Villages:**

```
Benign Missions (Lawful Good village):
  - Info gathering on neighbors (assess alliance potential)
  - Humanitarian aid coordination (scout disaster zones)
  - Bandit hunting (locate outlaw camps)

Hostile Missions (Chaotic Evil village):
  - Assassinate rival village leaders
  - Sabotage enemy food supplies (cause famine)
  - Kidnap enemy nobles for ransom

Wage Budget:
  - Small village (500 pop): 200-500 currency/month (1-2 spies)
  - Large village (2000 pop): 1000-3000 currency/month (5-10 spies)
  - City (10,000 pop): 5000-20,000 currency/month (20-50 spies)
```

**Guilds:**

```
Merchants' Guild:
  - Sabotage rival guild businesses (burn warehouses)
  - Steal trade secrets (new products, supply routes)
  - Bribe officials (gain favorable trade laws)

Heroes' Guild:
  - Infiltrate villain lairs (reconnaissance before raid)
  - Rescue kidnapped civilians (hostage extraction)
  - Expose corrupt nobles (gather evidence for prosecution)

Thieves' Guild:
  - Heists (steal artifacts, treasures)
  - Blackmail nobles (gather dirt, extort payments)
  - Assassinate rival thieves (eliminate competition)

Wage Budget:
  - Small guild (20 members): 300-800 currency/month (1-3 spies)
  - Large guild (100+ members): 2000-5000 currency/month (10-20 spies)
```

**Elite Families:**

```
Noble Families:
  - Spy on rival families (political intelligence)
  - Sabotage rival businesses (economic warfare)
  - Arrange "accidents" (assassinations disguised as misfortune)
  - Seduce rivals' children (gain leverage)
  - Steal land deeds (legal warfare)

Merchant Dynasties:
  - Industrial espionage (steal inventions, formulas)
  - Sabotage competitor supply chains
  - Bribe officials for favorable contracts
  - Kidnap rival heirs (ransom or elimination)

Wage Budget:
  - High wealth family: 500-2000 currency/month (2-5 spies)
  - Ultra High wealth family: 3000-10,000 currency/month (10-30 spies)
```

**Armies:**

```
Military Espionage:
  - Scout enemy troop positions (reconnaissance)
  - Sabotage enemy supplies (burn food, poison water)
  - Assassinate enemy commanders (decapitation strategy)
  - Spread false intelligence (deception campaigns)
  - Turn enemy soldiers (bribery, ideology)

Wage Budget:
  - Small army (100 soldiers): 500-1000 currency/month (3-5 spies)
  - Large army (1000+ soldiers): 3000-10,000 currency/month (20-50 spies)
```

---

## Wages & Compensation

### Base Wages

**Spy Pay Structure:**

```
Base Wage (per mission):
  Info Gathering (low risk):        200-400 currency
  Theft/Heist (medium risk):        500-1000 currency
  Assassination (high risk):        1000-3000 currency
  Sabotage (high risk):             600-1500 currency
  Long-term infiltration (monthly): 300-800 currency/month

Risk Premium:
  - High-security target (vaults, palaces): +50% wage
  - Hostile territory (enemy village): +30% wage
  - Low escape chance (suicide mission): +100% wage

Completion Bonus:
  - Success (undetected): +50% base wage
  - Success (detected but escaped): +25% base wage
  - Partial success (some objectives met): +10% base wage
  - Failure (mission aborted): 0 bonus
```

**Band Member Wages:**

```
Spy band members paid 150% market rate (risk premium):

Assassin:   600 currency/month (300 base × 2.0 risk multiplier)
Thief:      450 currency/month (300 base × 1.5 risk multiplier)
Saboteur:   450 currency/month (300 base × 1.5 risk multiplier)
Informant:  375 currency/month (250 base × 1.5 risk multiplier)
Seducer:    525 currency/month (350 base × 1.5 risk multiplier)
```

### Reputation Mechanics

**Hidden Reputation:**

```
Spy Reputation = Stealth-based (not public like heroes)

Reputation tracked by:
  - Employers (spymasters, nobles)
  - Underworld (thieves' guilds, criminal networks)
  - NOT tracked by public (anonymous profession)

Reputation Changes:
  Success (undetected): 0 change (perfect, anonymous)
  Success (detected):   +5 (word spreads in underworld, "skilled but messy")
  Failure (captured):   -20 (incompetent, untrustworthy)
  Betrayal (sold out employer): -50 (never hired again)

High Reputation (50+):
  - Premium wages (+50% base wage)
  - Elite clients (nobles, kings, guilds)
  - Legendary status (retired assassins, master thieves)

Low Reputation (<0):
  - Blacklisted (no spymaster hires)
  - Hunted (former employers want dead)
  - Career over (must flee or retire)
```

### Retirement Paths

**Spies retire after 5-15 years (high attrition, dangerous work):**

```
Retirement Options:

1. Spymaster (Intelligence 70+, Reputation 40+)
   - Manage spy networks for employers
   - Wage: 800-2000 currency/month
   - Lifespan: 10-30 years (safer than fieldwork)

2. Diplomat (Charisma 60+, former seducer/informant)
   - Use espionage skills for negotiation
   - Wage: 500-1500 currency/month
   - Reputation: Respectable (clean career transition)

3. Criminal Underworld Leader (Reputation 30+)
   - Lead thieves' guild or bandit gang
   - Wage: Variable (theft, extortion, smuggling)
   - Lifespan: Until killed by rivals or law

4. Assassin-for-Hire (Finesse 80+, Reputation 50+)
   - Independent contractor (no employer)
   - Wage: 2000-5000 currency/mission
   - Legendary status (feared, sought after)

5. Wealthy Retirement (accumulated 10,000+ currency)
   - Invest in businesses, live off wealth
   - Disappear (new identity, distant village)
   - Peaceful end (if enemies don't find them)

6. Death in Service (50% of spies)
   - Killed during mission (combat, execution)
   - Captured and executed (torture, beheading)
   - Betrayed by employer (silenced to hide secrets)
```

**Example Retirement:**

```
Spy: Theron (Age 40, 15 years of service)

Career Summary:
  - 50 missions (40 successes, 10 failures)
  - Reputation: 60 (legendary)
  - Wealth: 15,000 currency (accumulated)

Retirement Decision:
  - Tired of killing, wants peace
  - Invests 10,000 currency in merchant business
  - Retires to distant village (new identity)
  - Lives as "merchant" for 30 years
  - Dies peacefully at age 70

Narrative: "He killed for gold. He retired for peace. No one in his new village knew what he'd been. He never told them."
```

---

## DOTS Components

```csharp
using Unity.Entities;
using Unity.Collections;

namespace Godgame.Espionage
{
    /// <summary>
    /// Spy component - attached to individuals leading spy bands.
    /// </summary>
    public struct Spy : IComponentData
    {
        public byte Finesse;                    // 0-100 (lockpicking, stealth, agility)
        public byte Intelligence;               // 0-100 (planning, analysis)
        public byte Charisma;                   // 0-100 (seduction, recruitment)
        public byte Perception;                 // 0-100 (awareness, detect lies)

        public Entity CurrentEmployer;          // Spymaster or noble employing spy
        public Entity CurrentMission;           // Active mission entity (or Entity.Null)

        public ushort MissionCount;             // Total missions undertaken
        public ushort SuccessfulMissions;       // Undetected completions
        public ushort FailedMissions;           // Captures, aborts
        public sbyte Reputation;                // -100 to +100 (underworld reputation)

        public uint WealthAccumulated;          // Total currency earned (retirement fund)
        public ushort YearsInService;           // Career length (retirement trigger)

        public bool IsRetired;                  // True if retired from active duty
        public bool IsBlacklisted;              // True if reputation < 0, unhireable
    }

    /// <summary>
    /// Spy band component - covert team led by spy.
    /// </summary>
    public struct SpyBand : IComponentData
    {
        public Entity Leader;                   // Spy entity leading band
        public byte MemberCount;                // 3-12 members
        public sbyte BandLoyalty;               // -100 to +100

        public uint FormationTick;
        public ushort MissionCount;
        public byte SuccessRate;                // 0-100% (historical success)

        // Composition (count of each role)
        public byte AssassinCount;
        public byte ThiefCount;
        public byte SaboteurCount;
        public byte InformantCount;
        public byte SeducerCount;
    }

    /// <summary>
    /// Spy mission component - quest assigned by spymaster.
    /// </summary>
    public struct SpyMission : IComponentData
    {
        public enum MissionType : byte
        {
            InfoGathering,
            Theft,
            Heist,
            Assassination,
            Sabotage,
            Blackmail,
            Bribery,
            Infiltration,
            Seduction,
            Kidnapping
        }

        public enum MissionStatus : byte
        {
            Assigned,
            InProgress,
            Successful,
            PartialSuccess,
            Failed,
            Aborted
        }

        public MissionType Type;
        public MissionStatus Status;

        public Entity AssignedSpy;              // Spy undertaking mission
        public Entity Employer;                 // Who hired spy (spymaster, noble)
        public Entity Target;                   // Village, noble, building, etc.

        public uint AssignedTick;
        public uint DeadlineTick;               // Mission must complete by this tick
        public ushort BaseWage;                 // Currency payment
        public ushort CompletionBonus;          // Bonus if successful

        // Mission parameters
        public FixedString128Bytes Objective;   // "Steal land deed", "Assassinate Baron", etc.
        public byte RiskLevel;                  // 0-100 (low = info, high = assassination)
        public byte DetectionChance;            // 0-100% base chance of being caught

        // Results
        public bool WasDetected;                // True if spy identified
        public byte LoyaltyImpact;              // Impact on band loyalty (±30)
        public ushort LootValue;                // Currency or items stolen
    }

    /// <summary>
    /// Double agent component - infiltrator within opposing entity.
    /// </summary>
    public struct DoubleAgent : IComponentData
    {
        public enum AgentType : byte
        {
            PlantedAgent,       // Deliberately infiltrated
            TurnedAgent,        // Recruited while serving target
            GenuineConvert      // Originally loyal, now conflicted
        }

        public AgentType Type;

        public Entity RealEmployer;             // Who agent actually works for
        public Entity InfiltratedEntity;        // Village/guild/family being spied on
        public uint InfiltrationStartTick;
        public ushort MonthsEmbedded;

        public byte TrustLevel;                 // 0-100 (how much infiltrated entity trusts agent)
        public byte SuspicionLevel;             // 0-100 (accumulates, triggers investigation)

        public sbyte CommitmentToEmployer;      // -100 to +100
        public sbyte CommitmentToInfiltrated;   // -100 to +100 (conflict if equal)

        // Cover identity
        public FixedString64Bytes CoverRole;    // "Steward", "Guard", "Merchant", etc.
        public Entity CoverSpouse;              // If married into family (or Entity.Null)
        public bool HasChildrenInEntity;        // True if children born (bonding factor)

        // Activity tracking
        public ushort IntelReportsSubmitted;    // Reports sent to real employer
        public ushort SabotageActsCommitted;    // Sabotages performed
        public uint LastReportTick;
    }

    /// <summary>
    /// Spymaster component - courtier managing spy network.
    /// </summary>
    public struct Spymaster : IComponentData
    {
        public Entity Employer;                 // Noble, guild, village employing spymaster
        public byte NetworkSize;                // 0-50 spies managed
        public ushort MonthlyBudget;            // Currency allocated for spy operations

        public byte Intelligence;               // 0-100 (strategic thinking)
        public byte Finesse;                    // 0-100 (understands tradecraft)
        public byte Charisma;                   // 0-100 (recruit spies)

        public ushort MissionsAssigned;
        public ushort SuccessfulMissions;
        public byte SuccessRate;                // 0-100%

        // Corruption tracking
        public byte CorruptionLevel;            // 0-100 (0=loyal, 100=traitor)
        public bool IsDoubleAgent;              // True if selling secrets to rival
        public Entity SecretEmployer;           // If double agent (or Entity.Null)
    }

    /// <summary>
    /// Spy network buffer - list of spies managed by spymaster.
    /// </summary>
    public struct SpyNetworkMember : IBufferElementData
    {
        public Entity SpyEntity;
        public uint HiredTick;
        public ushort TotalWagePaid;
        public byte SuccessRate;                // This spy's success rate (0-100%)
        public sbyte Loyalty;                   // Spy's loyalty to spymaster (-100 to +100)
    }

    /// <summary>
    /// Seduction target component - tracks active seduction missions.
    /// </summary>
    public struct SeductionTarget : IComponentData
    {
        public Entity Seducer;                  // Spy performing seduction
        public Entity Target;                   // Elite family member being seduced
        public uint SeductionStartTick;
        public byte SeductionProgress;          // 0-100 (100 = affair begins)

        public sbyte TargetRelationToSeducer;   // -100 to +100
        public bool AffairActive;               // True if romance successful
        public bool GenuineLove;                // True if seducer fell in love (unplanned)

        // Secrets extracted
        public ushort SecretsGained;            // Count of intel pieces
        public uint LastSecretExtracted;
    }

    /// <summary>
    /// Kidnapping component - tracks hostage situations.
    /// </summary>
    public struct Kidnapping : IComponentData
    {
        public Entity Kidnapper;                // Spy or band leader
        public Entity Hostage;                  // Family member kidnapped
        public Entity TargetFamily;             // Family being ransomed
        public uint KidnappedTick;

        public ushort RansomDemand;             // Currency demanded
        public uint RansomDeadline;             // Tick by which ransom must be paid

        public bool RansomPaid;
        public bool HostageReleased;
        public bool HostageKilled;              // True if deadline passed or rescue failed
    }

    /// <summary>
    /// Counter-espionage investigation component.
    /// Tracks efforts to find spies/double agents within entity.
    /// </summary>
    public struct CounterEspionage : IComponentData
    {
        public Entity InvestigatingEntity;      // Village, guild, or noble investigating
        public Entity Spymaster;                // Spymaster conducting investigation
        public uint InvestigationStartTick;
        public ushort DurationMonths;           // How long investigation runs

        public byte SuspectsIdentified;         // Count of potential spies found
        public byte ConfirmedSpies;             // Count of confirmed double agents

        // Investigation methods
        public bool UsedLoyaltyTests;           // Planted false intel to see who leaks
        public bool UsedInterrogations;         // Tortured suspects
        public bool UsedSurveillance;           // Followed suspects
    }

    /// <summary>
    /// Espionage event log - tracks major spy-related events.
    /// </summary>
    public struct EspionageEvent : IBufferElementData
    {
        public enum EventType : byte
        {
            MissionSuccess,
            MissionFailure,
            SpyCaptured,
            SpyExecuted,
            DoubleAgentExposed,
            DoubleAgentConverted,      // Genuinely switched sides
            SeductionSuccess,
            KidnappingSuccess,
            AssassinationSuccess,
            HeistSuccess
        }

        public EventType Type;
        public uint Tick;
        public Entity SpyEntity;
        public Entity TargetEntity;
        public FixedString128Bytes Description;
    }
}
```

---

## System Flow: Spy Mission

**1. Mission Assignment:**

```csharp
// Spymaster assigns mission to spy
SpyMission mission = new SpyMission
{
    Type = MissionType.Assassination,
    Status = MissionStatus.Assigned,
    AssignedSpy = spyEntity,
    Employer = spymasterEmployer,
    Target = baronEntity,
    AssignedTick = currentTick,
    DeadlineTick = currentTick + (30 * ticksPerDay), // 30 days
    BaseWage = 1500,
    CompletionBonus = 1000,
    Objective = "Assassinate Baron Erik",
    RiskLevel = 90,
    DetectionChance = 40
};
```

**2. Spy Execution:**

```csharp
// Spy performs mission (checks during execution)
public void ExecuteAssassination(SpyMission mission, Spy spy)
{
    // Planning phase (Intelligence check)
    int planningBonus = spy.Intelligence / 10;

    // Infiltration phase (Finesse check)
    int infiltrationRoll = Random.Range(0, 100) + spy.Finesse + planningBonus;

    if (infiltrationRoll > 100)
    {
        // Silent kill (perfect execution)
        mission.Status = MissionStatus.Successful;
        mission.WasDetected = false;
        mission.CompletionBonus = mission.CompletionBonus; // Full bonus
    }
    else if (infiltrationRoll > 70)
    {
        // Success but detected during escape
        mission.Status = MissionStatus.PartialSuccess;
        mission.WasDetected = true;
        mission.CompletionBonus = mission.CompletionBonus / 2; // Half bonus

        // Target entity knows spy's identity (relation penalty)
        AddRelation(mission.Target, spy.Entity, -80); // Assassination attempt
    }
    else
    {
        // Failure, spy captured
        mission.Status = MissionStatus.Failed;
        mission.WasDetected = true;
        spy.FailedMissions++;
        spy.Reputation -= 20;

        // Spy imprisoned/executed (alignment-dependent)
        HandleCapturedSpy(spy, mission.Target);
    }
}
```

**3. Payment:**

```csharp
// Pay spy based on results
public void PaySpy(Spy spy, SpyMission mission)
{
    int totalPay = mission.BaseWage;

    if (mission.Status == MissionStatus.Successful)
    {
        totalPay += mission.CompletionBonus;
        spy.SuccessfulMissions++;
        spy.Reputation += 0; // Silent success, no reputation change
    }
    else if (mission.Status == MissionStatus.PartialSuccess)
    {
        totalPay += mission.CompletionBonus / 2;
        spy.Reputation += 5; // Detected but skilled
    }
    else if (mission.Status == MissionStatus.Failed)
    {
        totalPay = 0; // No payment for failure
    }

    spy.WealthAccumulated += (uint)totalPay;

    // Check retirement threshold (10,000+ currency or 15+ years)
    if (spy.WealthAccumulated >= 10000 || spy.YearsInService >= 15)
    {
        ConsiderRetirement(spy);
    }
}
```

**4. Double Agent Commitment Check:**

```csharp
// Periodic check for double agent loyalty
public void CheckDoubleAgentCommitment(DoubleAgent agent)
{
    int employerFactors = 0;
    int infiltratedFactors = 0;

    // Employer factors
    if (agent.MonthlyWage >= 1000) employerFactors += 20;
    if (AlignmentMatch(agent, agent.RealEmployer)) employerFactors += 15;
    if (HasHatredOf(agent, agent.InfiltratedEntity)) employerFactors += 30;

    // Infiltrated entity factors
    if (HasCloseRelations(agent, agent.InfiltratedEntity)) infiltratedFactors += 25;
    if (agent.CoverSpouse != Entity.Null && GenuineLove) infiltratedFactors += 40;
    if (agent.HasChildrenInEntity) infiltratedFactors += 50;

    agent.CommitmentToEmployer = (sbyte)Mathf.Clamp(employerFactors, -100, 100);
    agent.CommitmentToInfiltrated = (sbyte)Mathf.Clamp(infiltratedFactors, -100, 100);

    // Decision
    if (agent.CommitmentToInfiltrated > agent.CommitmentToEmployer + 30)
    {
        // Agent genuinely converts, becomes loyal to infiltrated entity
        agent.Type = AgentType.GenuineConvert;

        // May confess or become triple agent (feed false intel to employer)
        ConsiderConfession(agent);
    }
}
```

---

## Iteration Plan

**v1.0 (MVP - Basic Espionage):**
- ✅ Spy component (Finesse, Intelligence, Reputation)
- ✅ Basic mission types (Info Gathering, Theft, Assassination)
- ✅ Spymaster employment (assign missions, pay wages)
- ✅ Success/failure checks (Finesse rolls)
- ✅ Detection mechanics (captured spies)
- ✅ Simple wage system (base + bonus)
- ❌ No spy bands
- ❌ No double agents
- ❌ No seduction/kidnapping
- ❌ No heists (multi-stage missions)

**v2.0 (Enhanced - Spy Bands & Double Agents):**
- ✅ Spy bands (3-12 members, composition)
- ✅ Band loyalty mechanics (abandonment, rescue)
- ✅ Double agent system (infiltration, trust building)
- ✅ Commitment checks (conflicting loyalties)
- ✅ Seduction missions (extract secrets via romance)
- ✅ Kidnapping (ransom demands)
- ✅ Blackmail & bribery (political leverage)
- ✅ Counter-espionage (detect moles)
- ✅ Hidden reputation (underworld only)

**v3.0 (Complete - Advanced Operations):**
- ✅ Multi-stage heists (planning, execution, escape)
- ✅ Decision points during missions (rescue/abandon)
- ✅ Loot division (band loyalty impact)
- ✅ Retirement paths (spymaster, criminal leader, wealthy retirement)
- ✅ Dynasty-spanning vendettas (family member killed → eternal grudge)
- ✅ Triple agents (double agents who feed false intel)
- ✅ Spy guilds (organized espionage networks)
- ✅ Player deity espionage (God-sponsored missions)

---

## Open Questions

### Critical

1. **Spy detection frequency?** What percentage of missions should result in capture? 10%? 30%?
2. **Double agent conversion rate?** What % genuinely convert vs remain loyal to employer?
3. **Spymaster corruption prevalence?** Should most spymasters eventually become double agents?
4. **Retirement enforcement?** Do spies auto-retire at 15 years or player/AI choice?

### Design

5. **Spy band AI?** Do band members have individual agency or purely controlled by leader?
6. **Seduction romance depth?** How complex should love vs manipulation mechanics be?
7. **Counter-spy effectiveness?** How quickly should moles be detected if investigation active?
8. **Mission failure consequences?** Should captured spies always reveal employer or resist interrogation?

### Balancing

9. **Wage inflation?** Are spy wages (150-200% market rate) sustainable for employers?
10. **Assassination impact?** Should killing key figures (guild masters, nobles) destabilize entities significantly?
11. **Heist risk-reward?** Should multi-stage heists have 70% success or lower (higher risk)?
12. **Reputation cap?** Should spies reach legendary status (+100 reputation) or cap at +60?

### Technical

13. **Espionage event logging?** Track all missions or only major successes/failures?
14. **Double agent pathfinding?** How do agents navigate conflicting employer orders?
15. **Seduction relationship decay?** Do affairs end naturally or only via betrayal/discovery?
16. **Spy guild mechanics?** How do organized espionage networks differ from independent spies?

---

## Exploits

**Problem:** Player spams assassination missions to eliminate all rival nobles
- **Fix:** Escalating security (each assassination increases guard count, detection chance)
- **Cap:** Max 3 assassinations per entity per year before target becomes untouchable

**Problem:** Double agents accumulate infinite trust, never detected
- **Fix:** Periodic suspicion checks (random events trigger investigations)
- **Mechanic:** Trust ≥ 80 still has 5% chance/year of random exposure

**Problem:** Spies accumulate infinite wealth, break economy
- **Fix:** Wealth cap (10,000 currency triggers forced retirement)
- **Tax:** Spy wealth taxed 30% (risky profession, bribes to stay hidden)

**Problem:** Player plants double agents in all rival courts, omniscient intel
- **Fix:** Double agent capacity (spymaster can manage max 5 deep-cover agents)
- **Cost:** Each double agent costs 800 currency/month (expensive to maintain)

**Problem:** Seduction exploited to mind-control all elite families
- **Fix:** Seduction cooldown (can only seduce 1 family member per family/year)
- **Resistance:** Families increase vigilance after 1st seduction (+30% detection for future attempts)

**Problem:** Spy bands abandon all captured members, no loyalty penalty
- **Fix:** Loyalty penalty scales (-20 per abandonment, cumulative)
- **Consequence:** Loyalty < -50 → Band mutinies, kills leader

---

## Player Interaction

### Observable

**Missions Visible:**
- Player sees spy mission progress (assigned → in progress → completed/failed)
- Spy reports provide intel (troop counts, resource levels, secrets)
- Failed missions create diplomatic incidents (relations plummet)

**Espionage Events:**
- Spy captured: "Your spy was caught in Baron's vault. Baron demands explanation."
- Assassination success: "Duke Aldric found dead in sleep. Investigators suspect foul play."
- Double agent exposed: "Your agent Elira was revealed as traitor. Executed publicly."

**UI Indicators:**
- Spy network size (spymaster manages 5/20 spies)
- Mission success rate (78% historically)
- Wealth invested in espionage (2000 currency/month)

### Player Agency

**Hire Spies:**
- Player approves spymaster's recruit recommendations
- Set spy budget (high budget = more/better spies)

**Assign Missions:**
- Player chooses mission type (info, theft, assassination)
- Select target (rival noble, enemy village, resource)
- Set priority (rush mission or wait for perfect opportunity)

**Handle Failures:**
- Captured spy: Rescue (risky, expensive) vs Disavow (cheap, spy dies)
- Exposed double agent: Deny involvement vs Apologize vs Declare war

**Manage Double Agents:**
- Receive commitment check notifications ("Agent Kael's loyalty wavering")
- Increase pay to retain loyalty
- Extract agent before exposure

### Difficulty Scaling

**Novice:**
- High success rates (70-80%)
- Low detection (20%)
- Cheap spies (100% market rate)

**Intermediate:**
- Moderate success (50-60%)
- Medium detection (40%)
- Standard spies (150% market rate)

**Expert:**
- Low success (30-40%)
- High detection (60%)
- Expensive spies (200% market rate)
- Active counter-espionage (rivals hunt your spies)

---

## Systemic Interactions

### Dependencies

**Elite Courts:** ✅ Spymaster courtier position manages spy networks
**Guild System:** ✅ Thieves' Guild provides recruits, Heroes' Guild performs counter-espionage
**Entity Relations:** ✅ Assassination attempts, theft create permanent grudges (-80 relation)
**Individual Progression:** ✅ Finesse skill determines spy effectiveness
**Bands:** ✅ Spy bands use same structure as mercenary/military bands
**Alignment Framework:** ✅ Methods vary by alignment (stealth vs brute force)
**Business & Assets:** ✅ Businesses are sabotage targets, theft targets
**Wealth & Social Dynamics:** ✅ Elite families employ spies for dynastic warfare

### Influences

**Warfare:** Sabotaged supplies weaken armies (-50% strength)
**Economy:** Stolen trade secrets shift market dominance
**Politics:** Blackmail controls council votes, policies
**Diplomacy:** Exposed espionage triggers wars (-90 relation)
**Succession:** Assassinated heirs create succession crises
**Marriage:** Seduced nobles leak family secrets, undermined by affairs

### Synergies

**Relations + Espionage:** High relation required to recruit spies (+20 minimum)
**Wealth + Espionage:** Wealthy employers afford larger spy networks
**Guilds + Espionage:** Thieves' Guild provides trained assassins/thieves
**Courts + Espionage:** Corrupt spymasters sell secrets to rivals (double agents)
**Alignment + Espionage:** Evil entities prefer assassination, Good prefer info gathering
**Double Agents + Relations:** Genuine conversion when relation to infiltrated > employer

---

## Related Documentation

- **Elite Courts:** [Elite_Courts_And_Retinues.md](Elite_Courts_And_Retinues.md) - Spymaster courtier position
- **Entity Relations:** [Entity_Relations_And_Interactions.md](Entity_Relations_And_Interactions.md) - Relation penalties from espionage
- **Guild System:** [Guild_System.md](Guild_System.md) - Thieves' Guild, spy recruitment
- **Individual Progression:** [Individual_Progression_System.md](Individual_Progression_System.md) - Finesse skill for spies
- **Bands:** [Bands_Vision.md](Bands_Vision.md) - Spy band structure, loyalty mechanics
- **Alignment Framework:** [Generalized_Alignment_Framework.md](../Meta/Generalized_Alignment_Framework.md) - Espionage methods by alignment
- **Business & Assets:** [BusinessAndAssets.md](../Economy/BusinessAndAssets.md) - Sabotage targets
- **Wealth & Social Dynamics:** [Wealth_And_Social_Dynamics.md](Wealth_And_Social_Dynamics.md) - Elite espionage warfare
- **Individual Combat:** [../Combat/Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - Assassination combat mechanics

---

**For Implementers:** Start with basic spy missions (info gathering, theft) before adding double agents. Double agent commitment checks are narratively rich but mechanically complex.

**For Narrative:** Spies create John le Carré-style moral dilemmas (abandon captured ally vs risk team), Mission Impossible heists (coordinated infiltration), and Game of Thrones intrigue (seduced nobles leak secrets, dynastic assassinations).

---

**Last Updated:** 2025-11-06
**Status:** Concept Draft - Core vision captured, dependent systems integrated
