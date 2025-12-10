# Rebellion Mechanics System (Godgame)

## Overview
Vassals and subjects may rebel against their lieges and overlords when loyalty degrades. Populations divide into loyalists (inform on rebels), rebels (actively resist), and neutrals (take no side). Rebellions begin with individual grievances, recruit cautiously among known discontents, and progress through escalation or de-escalation paths toward violent or peaceful resolutions.

**Integration**: Mind ECS (1 Hz) for individual loyalty tracking, Aggregate ECS (0.2 Hz) for faction-wide rebellion coordination.

---

## Loyalty Factions During Rebellion

### The Three Factions

**1. Loyalists (Remain Faithful to Liege)**
- **Motivation**: Personal bonds with liege, ideological support, fear of chaos, material benefits
- **Actions**: Report rebel conspiracies, sabotage rebellion, remain obedient, fight for liege
- **Risk**: Targeted by rebels (assassinated, exiled, property seized if rebellion succeeds)

**2. Rebels (Actively Resist Overlord)**
- **Motivation**: Grievances (heavy taxes, tyranny, broken oaths, injustice)
- **Actions**: Recruit supporters, plan uprising, engage in violence/protest, negotiate demands
- **Risk**: Executed if rebellion fails, branded traitors

**3. Neutrals (Take No Side)**
- **Motivation**: Self-preservation, conflicted loyalties, pragmatism, cowardice
- **Actions**: Refuse to join either side, hide, flee, continue daily life
- **Risk**: Punished by both sides (rebels demand commitment, liege sees neutrality as disloyalty)

### Faction Sizes (Typical Rebellion)

```
Example: Barony with 800 subjects, vassal baron rebels against duke

Initial Loyalty Distribution (before rebellion):
- Loyalists: 320 (40%) - Support baron's rule
- Potential Rebels: 240 (30%) - Begrudged, dissatisfied, but passive
- Neutrals: 240 (30%) - Indifferent, self-interested

Rebellion Declared (baron raises banners against duke):
- Loyalists to Duke: 480 (60%) - Baron's subjects + neighboring loyal vassals
- Loyalists to Baron (Rebels): 160 (20%) - Those who follow baron into rebellion
- Neutrals: 160 (20%) - Refuse to commit

As rebellion progresses (6 months):
- Loyalists to Duke: 400 (50%) - Some neutrals pressured to support duke
- Rebels: 240 (30%) - Recruits from begrudged population + mercenaries
- Neutrals: 160 (20%) - Dwindling (forced to choose)
```

---

## Loyalty Determination

### Individual Loyalty Calculation

```csharp
/// <summary>
/// Determine which faction individual supports
/// </summary>
public static LoyaltyFaction DetermineLoyalty(
    float loyaltyToLiege,              // 0-100 (current loyalty to overlord)
    float grievanceLevel,              // 0-100 (accumulated grievances)
    int personalBondsWithLiege,        // Close relationships with liege/family
    int personalBondsWithRebels,       // Relationships with rebel leaders
    bool hasIdeologicalReason,         // Ideological motivation (justice, freedom, etc.)
    bool fearOfReprisal,               // Fears liege's punishment if neutral/rebel
    int courageLevel)                  // 0-100 (willingness to risk life)
{
    // Loyalist calculation
    float loyalistScore = loyaltyToLiege + (personalBondsWithLiege * 10f);
    if (fearOfReprisal && courageLevel < 50f)
        loyalistScore += 20f; // Fear makes cowards loyal

    // Rebel calculation
    float rebelScore = grievanceLevel + (personalBondsWithRebels * 10f);
    if (hasIdeologicalReason)
        rebelScore += 25f; // True believers more committed
    if (courageLevel > 70f)
        rebelScore += 15f; // Brave more likely to rebel

    // Neutral calculation
    float neutralScore = 50f; // Base neutrality
    if (courageLevel < 40f && !hasIdeologicalReason)
        neutralScore += 20f; // Cowards hide
    if (personalBondsWithLiege > 0 && personalBondsWithRebels > 0)
        neutralScore += 30f; // Conflicted loyalties → neutrality

    // Determine faction
    if (loyalistScore > rebelScore && loyalistScore > neutralScore)
        return LoyaltyFaction.Loyalist;
    else if (rebelScore > loyalistScore && rebelScore > neutralScore)
        return LoyaltyFaction.Rebel;
    else
        return LoyaltyFaction.Neutral;
}
```

### Loyalty Shifts During Rebellion

**Triggers for Switching Factions**:
- **Neutral → Loyalist**: Liege offers rewards, rebels commit atrocities, fear of rebel victory
- **Neutral → Rebel**: Liege commits atrocities, rebels winning, taxation/conscription by liege
- **Loyalist → Neutral**: Liege loses battles, payment stops, disillusionment
- **Rebel → Neutral**: Rebellion failing, fear of execution, rebel leaders incompetent
- **Loyalist → Rebel** (rare): Liege betrays loyalist, family killed by liege's forces
- **Rebel → Loyalist** (rare): Amnesty offered, ideological conversion, captured and turned

**Loyalty Shift Example**:
```
Year 0: Peasant Gareth (Neutral, grievance 45, courage 35)
- Refuses to join rebellion (too cowardly)
- Refuses to fight for baron (resentful of taxes)

Month 3: Baron's troops demand food levy
- Grievance: 45 → 65 (seized entire harvest)
- Loyalty shift: Neutral → Rebel (joins uprising)

Month 6: Rebellion suffers major defeat
- Fear of execution: +30
- Courage check: 35 (fails, too cowardly to continue)
- Loyalty shift: Rebel → Neutral (flees to neighboring barony)
```

---

## Informant Mechanics (Loyalists Report Treachery)

### Loyal Vassals as Informants

**Scenario**: Baron Aldric plots rebellion against Duke Godfrey

```
Duke's Vassals (8 barons total):
- Baron Aldric: Rebel (loyalt to duke: 15, grievance: 80)
- Baron Hamish: Loyalist (loyalty: 85, personal friend of duke)
- Baron Edric: Loyalist (loyalty: 70, benefits from duke's patronage)
- Baron Oswyn: Neutral (loyalty: 45, pragmatic, waits to see who wins)
- Baron Roderick: Rebel Sympathizer (loyalty: 25, secretly agrees with Aldric)
- Baron Cedric: Neutral (loyalty: 50, conflicted)
- Baron Leofric: Loyalist (loyalty: 75, ideological support for ducal authority)
- Baron Theobald: Rebel Sympathizer (loyalty: 30, begrudged by heavy taxes)

Aldric's Recruitment:
- Contacts Roderick (loyalty 25): SUCCESS (safe, known dissatisfaction)
- Contacts Theobald (loyalty 30): SUCCESS (safe, begrudged by taxes)
- Avoids Hamish (loyalty 85): SAFE (known loyalist, would report immediately)
- Contacts Oswyn (loyalty 45): RISKY (neutral, could go either way)

Oswyn's Response:
- Pragmatic calculation: Rebellion has 3/8 barons (37% support) → Likely to fail
- Reports conspiracy to Duke Godfrey
- Duke rewards Oswyn: +1000 gold, +15 loyalty (45 → 60, now Loyalist)

Duke's Response:
- Arrests Aldric before rebellion begins
- Confiscates Aldric's lands
- Offers amnesty to Roderick and Theobald if they renounce Aldric
- Roderick accepts amnesty (loyalty: 25 → 35, remains resentful but cowed)
- Theobald refuses, imprisoned

Outcome: Rebellion crushed before it starts (informant preempted uprising)
```

### Informant Detection Risk

**Rebels Must Avoid Informants**:

```csharp
/// <summary>
/// Calculate risk that recruitment target will inform on rebels
/// </summary>
public static float CalculateInformantRisk(
    float targetLoyaltyToLiege,        // 0-100
    int targetRelationWithRebels,      // -100 to +100 (personal relationship)
    float rebelIntelligence,           // 0-100 (ability to read target)
    bool targetHasPersonalGrievance,   // Does target have reason to rebel?
    bool targetFearful)                // Is target afraid of liege?
{
    // Base risk from loyalty
    float baseRisk = targetLoyaltyToLiege / 100f; // 0-1

    // Personal relationship modifier
    float relationModifier = -targetRelationWithRebels / 200f; // -0.5 to +0.5
    baseRisk += relationModifier;

    // Grievance reduces risk (begrudged targets less likely to inform)
    if (targetHasPersonalGrievance)
        baseRisk -= 0.3f;

    // Fear increases risk (fearful subjects inform to protect themselves)
    if (targetFearful)
        baseRisk += 0.2f;

    // Rebel intelligence: Smart rebels read targets better
    float intelligenceModifier = (100f - rebelIntelligence) / 200f; // 0 to 0.5
    baseRisk += intelligenceModifier;

    return math.clamp(baseRisk, 0f, 1f);
}
```

**Example**:
```
Rebel leader contacts potential recruit:
- Target loyalty to liege: 55 (moderate)
- Relation with rebel: +20 (friendly)
- Rebel intelligence: 75 (smart)
- Target has grievance: Yes (heavy taxes)
- Target fearful: No

Informant Risk:
baseRisk = 55/100 = 0.55
relationModifier = -20/200 = -0.10
grievance = -0.30
fear = 0
intelligence = (100-75)/200 = 0.125

Total Risk: 0.55 - 0.10 - 0.30 + 0 + 0.125 = 0.375 (37.5% chance target informs)

Rebel's Decision: 37.5% is risky, but not prohibitive → Contacts target cautiously
Result: Random roll 0.42 > 0.375 → Target does NOT inform (joins rebellion)
```

### Informant Network Effects

**Cascade Information Flow**:
```
Loyalist Vassal informs Duke of rebellion:
- Duke gains +3 months preparation time
- Duke musters troops (60% larger force ready)
- Duke arrests rebel leaders preemptively (40% chance)
- Duke offers amnesty to sympathizers (splits rebel coalition)

Result: Rebellion crushed before it begins (65% probability) OR weakened significantly (35%)
```

**Counter-Intelligence** (Rebels Detect Informants):
```
Rebel spy network (INT-based):
- Discovers Baron Oswyn met with Duke secretly
- Rebels confront Oswyn: Demands oath of loyalty or execution
- Oswyn refuses oath → Executed as traitor
- Duke loses informant, rebels gain 3 weeks warning before ducal response

Result: Rebellion proceeds but rebels aware of ducal preparations
```

---

## Recruitment Mechanics (Cautious Conspiracy)

### Recruitment Targeting

**Rebels ONLY Recruit Known Discontents**:

```
Recruitment Safety Tiers:

Tier 1 - Safe Recruits (Loyalty to Liege ≤ 25):
- Severely begrudged, visible grievances, public complaints
- Informant risk: 5-15%
- Examples: Peasant whose family was killed by liege's troops, knight denied promotion, merchant overtaxed

Tier 2 - Risky Recruits (Loyalty 26-50):
- Moderately dissatisfied, neutral, pragmatic
- Informant risk: 30-50%
- Examples: Neutral vassals, conflicted loyalties, opportunists

Tier 3 - Dangerous Recruits (Loyalty 51-75):
- Mild loyalists, benefiting from liege, ideologically supportive
- Informant risk: 60-80%
- Examples: Vassals with good relations to liege, ideological monarchists

Tier 4 - Suicidal Recruits (Loyalty 76-100):
- Strong loyalists, personal bonds with liege, ideological zealots
- Informant risk: 90-100%
- Examples: Liege's family, close friends, beneficiaries of patronage
- NEVER contacted (guaranteed informants)
```

**Rebel Strategy**:
```
Baron Aldric's Conspiracy:
Phase 1 (Month 0-2): Recruit Tier 1 only (safe core)
- Contacts 8 Tier 1 targets: 7 join, 1 informs (but weak evidence, duke dismisses)
- Core conspiracy: 7 committed rebels

Phase 2 (Month 3-4): Recruit Tier 2 (expand base)
- Contacts 12 Tier 2 targets: 6 join, 4 refuse (but stay silent), 2 inform
- Duke now has credible intelligence (2 informants confirm conspiracy)
- Rebellion base: 13 rebels

Phase 3 (Month 5): Race against time
- Duke prepares counter-rebellion (musters troops, arrests suspects)
- Rebels must launch uprising NOW or be preempted
- Aldric declares rebellion prematurely (only 13 committed, 50+ needed)

Outcome: Rebellion launches weak, crushed in 3 months
```

### Recruitment Methods

**1. Personal Approach** (High Risk, High Trust):
```
Rebel leader meets target face-to-face:
- Trust: High (personal bond established)
- Risk: High (if target informs, rebel leader's identity exposed)
- Best for: Tier 1 recruits, close friends

Example:
Baron Aldric meets Baron Roderick privately:
- "Brother, the Duke's taxes crush us all. Will you stand with me when I raise my banners?"
- Roderick (loyalty 25): Agrees immediately (safe recruit)
```

**2. Proxy Recruitment** (Low Risk, Lower Trust):
```
Rebel sends trusted agent to recruit:
- Trust: Medium (agent vouches for rebel leader)
- Risk: Low (leader's identity protected if agent captured)
- Best for: Tier 2 recruits, uncertain targets

Example:
Aldric sends trusted knight Sir Gareth to recruit Baron Oswyn:
- "My lord serves a cause greater than tyranny. Would you hear more?"
- Oswyn (loyalty 45): Suspicious, refuses → Reports to Duke
- Gareth captured, tortured, reveals Aldric's identity
- Outcome: Proxy failed, but delayed duke's response by 2 weeks
```

**3. Anonymous Messaging** (Lowest Risk, Lowest Trust):
```
Rebel sends unsigned letter or messenger:
- Trust: Low (target unsure of sender, may be trap)
- Risk: Very Low (no identity exposed)
- Best for: Tier 2 recruits, testing loyalty

Example:
Aldric sends anonymous letter to Baron Cedric:
- "The Duke's reign ends soon. Choose wisely."
- Cedric (loyalty 50): Unsure if genuine or ducal loyalty test → Ignores letter
- Outcome: Safe (Cedric doesn't know sender, can't inform)
```

---

## Neutral Consequences

### Neutrals Punished by Both Sides

**Rebel Perspective**: "Those who are not with us are against us"
**Liege Perspective**: "Neutrality is disloyalty in times of war"

**1. Rebels Demand Commitment**:
```
Village Elder refuses to join rebellion:
- "I will not take sides in this conflict."

Rebel Response:
- Seizes village food supplies (20% of harvest)
- Conscripts 10 young men forcibly
- Threatens: "Support us or be treated as enemies"

Village Elder's Choice:
- Join rebellion (forced)
- Flee to liege's territory (refugee)
- Resist rebels (village razed, elder executed)

Outcome: Elder flees, village split (40% join rebels, 60% flee)
```

**2. Liege Punishes Neutrality**:
```
Baron Oswyn declares neutrality:
- "I will not raise banners for either side."

Duke's Response:
- "You owe me fealty. Neutrality breaks your oath."
- Demands full military levy (400 soldiers)
- Threatens: "Join me or be branded traitor"

Oswyn's Choice:
- Join duke (forced)
- Join rebels (defection)
- Maintain neutrality (lands confiscated, exiled)

Outcome: Oswyn joins duke reluctantly (loyalty 45 → 50, avoids punishment)
```

**3. Neutrals Exploited by Both**:
```
Merchant Guild declares neutrality:
- "We trade with all, fight for none."

Both Sides Exploit:
- Rebels: Demand "loans" (confiscate goods, never repay)
- Liege: Impose war taxes (50% of profits)
- Both: Conscript guild guards

Guild's Losses:
- 60% wealth seized by both sides
- 30 guards conscripted (split between sides)
- Trade routes disrupted (80% revenue loss)

Outcome: Guild bankrupt, neutrality untenable (forced to choose side for protection)
```

**4. Neutrals Left Alone** (Rare, Strategic):
```
Remote mountain village declares neutrality:
- Too distant to matter strategically
- Poor (not worth exploiting)
- Defensible (costly to attack)

Both Sides Ignore:
- Rebels: Focus on wealthy lowlands
- Liege: Focus on rebel strongholds

Outcome: Village survives neutrality (isolated but intact)
```

### Neutral Survival Strategies

**1. Pay Both Sides** (Bribery):
```
Rich merchant pays:
- 500 gold to rebels ("protection fee")
- 500 gold to duke ("loyalty tax")
- Maintains trade with both sides

Outcome: Survives if both sides accept bribes (70% success rate)
Risk: If one side discovers dual-payment, executed as traitor
```

**2. Flee Territory** (Refugee):
```
Peasant family flees war zone:
- Abandons farm, takes portable wealth
- Seeks refuge in neutral neighboring kingdom

Outcome: Survives but impoverished (loses 80% wealth)
```

**3. Hide/Disappear** (Evasion):
```
Scholar hides in monastery:
- Claims religious neutrality
- Monastery shelters refugees

Outcome: Survives if monastery not attacked (60% success)
```

---

## Rebellion Initiation & Progression

### Individual Grievances → Mass Uprising

**Stage 1: Individual Dissent** (Single Begrudged Person)
```
Month 0: Peasant Gareth
- Duke's tax collectors seize entire harvest
- Family starves, child dies
- Grievance: 85, Loyalty: 10

Gareth's Action: Refuses next tax payment
- Duke's bailiff arrests Gareth
- Gareth resists arrest, kills bailiff
- Flees to forest, becomes outlaw

Status: 1 individual rebel (no organization, no threat)
```

**Stage 2: Small Conspiracy** (5-20 Rebels)
```
Month 3: Gareth recruits other outlaws
- 8 fellow outlaws join (all begrudged peasants)
- Form bandit gang, raid tax collectors

Conspiracy Actions:
- Rob 3 tax caravans (2,000 gold stolen)
- Kill 5 ducal soldiers
- Hide in forest

Status: Small bandit gang (local threat, not rebellion)
Duke's Response: Sends 50 soldiers to hunt bandits
```

**Stage 3: Organized Movement** (50-200 Rebels)
```
Month 6: Gareth's gang grows
- Reputation spreads (folk hero to peasants)
- 60 peasants join rebellion
- Minor knight (Sir Aldric) joins, provides military leadership

Movement Actions:
- Raid small ducal outpost, seize weapons
- Free prisoners (20 more recruits)
- Establish forest camp (fortified)

Status: Organized rebel movement (regional threat)
Duke's Response: Offers 1,000 gold bounty on Gareth, sends 200 soldiers
```

**Stage 4: Mass Uprising** (500+ Rebels)
```
Month 12: Movement becomes rebellion
- 3 barons defect to rebels (bring 400 soldiers)
- Peasant uprising in 5 villages (300 militia)
- Total rebel force: 760 combatants

Rebellion Actions:
- Capture ducal castle (small)
- Declare Gareth "People's Champion"
- Issue demands: Reduce taxes 50%, pardon all rebels, dismiss corrupt officials

Status: Full-scale rebellion (civil war)
Duke's Response: Musters full army (1,200 soldiers), requests royal aid
```

### Escalation Paths

**Path A: Peaceful Escalation** (Protest → Civil Disobedience → Negotiated Settlement)
```
Month 0: Peasants petition duke (reduce taxes)
- Duke ignores petition

Month 2: Peasants refuse tax payment (civil disobedience)
- Duke sends troops to enforce

Month 4: Troops massacre 20 peasants (escalation)
- Peasants arm themselves, fortify villages

Month 6: Armed standoff (neither side attacks)
- Duke realizes military solution costly
- Opens negotiations

Month 8: Negotiated settlement
- Taxes reduced 30%
- Amnesty for protesters
- Duke saves face (no major battle)

Outcome: Peaceful resolution (both sides compromise)
```

**Path B: Violent Escalation** (Raid → Battle → Siege → Total War)
```
Month 0: Bandits raid tax caravan
- Duke sends soldiers to punish

Month 2: Soldiers burn village (collective punishment)
- Survivors join bandits (escalation)

Month 4: Rebels ambush ducal column, kill 50 soldiers
- Duke declares rebels traitors (no mercy)

Month 6: Open battle at River Crossing
- Rebels win (duke's army routed)

Month 8: Rebels besiege duke's castle
- Duke requests royal intervention

Month 12: Royal army arrives, crushes rebels
- Gareth executed, 200 rebels hanged

Outcome: Violent resolution (total rebel defeat)
```

**Path C: De-Escalation** (Rebellion → Negotiation → Amnesty)
```
Month 0: Rebellion active (400 rebels vs 600 ducal troops)

Month 2: Stalemate (neither side can win decisively)
- Both sides suffer heavy casualties
- Rebel leaders propose truce

Month 3: Negotiations begin
- Rebels demand: Tax reduction, amnesty
- Duke offers: Partial amnesty, minor tax reduction

Month 4: Agreement reached
- Taxes reduced 20%
- Amnesty for common rebels
- Rebel leaders exiled (not executed)
- Rebels disband

Outcome: De-escalated resolution (compromise)
```

---

## Rebellion Types & Outcomes

### Rebellion Types

**1. Violent Rebellion** (Armed Conflict)
```
Characteristics:
- Military confrontation
- Sieges, battles, raids
- High casualties
- Winner-takes-all

Example: Baron's Army vs Duke's Army
- 600 rebels vs 800 loyalists
- Battle of Redford Plain
- Rebels routed, 300 killed
- Baron executed for treason

Outcome: Total loyalist victory
```

**2. Peaceful Rebellion** (Civil Disobedience, Protest)
```
Characteristics:
- Non-violent resistance
- Tax refusal, strikes, petitions
- Low casualties (unless suppressed violently)
- Negotiated settlements more common

Example: Peasant Tax Revolt
- 800 peasants refuse tax payment
- Barricade villages peacefully
- Duke negotiates (cheaper than military campaign)
- Taxes reduced 25%

Outcome: Partial rebel success (negotiated)
```

**3. Coup d'État** (Internal Overthrow)
```
Characteristics:
- Sudden seizure of power
- Led by insiders (vassal, heir, general)
- Targets leader personally (assassination, capture)
- Quick resolution (days, not months)

Example: Duke's Brother Usurps Throne
- Poisons duke at banquet
- Seizes castle with 50 loyal knights
- Declares himself new duke
- Most vassals accept fait accompli (avoid civil war)

Outcome: Successful coup (total rebel victory in 3 days)
```

**4. Secession** (Independence Movement)
```
Characteristics:
- Goal is separation, not regime change
- Regional (border provinces, remote territories)
- Defensive (rebels fortify borders)
- Often ends in negotiated independence or reconquest

Example: Northern Provinces Secede
- 5 northern barons declare independence
- Form "Free Northern League"
- Duke lacks resources to reconquer (mountains, distance)
- Negotiates recognition in exchange for tribute

Outcome: Successful secession (new independent state)
```

### Rebellion Outcomes

**1. Total Rebel Victory**
```
Conditions:
- Rebels defeat liege militarily OR
- Liege assassinated/captured OR
- Liege flees/abdicates

Resolution:
- Rebel leader becomes new ruler
- Loyalists executed/exiled/pardoned
- New regime established

Example:
Baron Aldric defeats Duke in battle:
- Duke killed, castle captured
- Aldric crowned new duke by rebel vassals
- Loyalist vassals submit or stripped of lands
- Kingdom recognizes Aldric (fait accompli)

Aftermath:
- Loyalists (40% population): Resentful, plot counter-rebellion
- Rebels (30% population): Rewarded, new elites
- Neutrals (30% population): Accept new regime pragmatically
```

**2. Total Rebel Defeat**
```
Conditions:
- Rebels defeated militarily OR
- Rebel leader killed/captured OR
- Rebellion collapses (starvation, desertion)

Resolution:
- Liege remains in power
- Rebel leaders executed
- Rebel sympathizers punished (fines, land seizure, exile)
- Harsh crackdown

Example:
Duke crushes Baron Aldric's rebellion:
- Aldric executed publicly
- Rebel vassals stripped of titles
- 200 rebel soldiers hanged
- Villages that supported rebellion heavily taxed (punitive)

Aftermath:
- Loyalists (60% population): Rewarded, strengthened
- Defeated Rebels (20% population): Cowed, resentful, planning next rebellion
- Neutrals (20% population): Fear liege's power
- Grievances unresolved: Next rebellion in 10-20 years
```

**3. Partial Rebel Victory** (Negotiated Settlement)
```
Conditions:
- Stalemate (neither side can win) OR
- War exhaustion (both sides want peace) OR
- External pressure (king mediates)

Resolution:
- Rebels gain some demands (tax reduction, reforms, autonomy)
- Liege remains in power
- Amnesty for most rebels
- Compromise

Example:
Duke and Baron Aldric negotiate after 8 months stalemate:
- Aldric's demands: 50% tax reduction, veto power on ducal decrees
- Duke's counter: 20% tax reduction, advisory council (no veto)
- Agreement: 25% tax reduction, council with voting rights on taxes only
- Aldric pardoned, retains barony

Aftermath:
- Loyalists (50% population): Unhappy with concessions
- Rebels (30% population): Partially satisfied
- Neutrals (20% population): Relieved war ended
- Fragile peace (potential future conflict if terms broken)
```

**4. Rebellion Failure Without Bloodshed** (Preempted/Collapsed)
```
Conditions:
- Rebellion discovered before launch (informants) OR
- Rebel leaders arrested preemptively OR
- Rebellion collapses from lack of support

Resolution:
- Rebel leaders punished (imprisonment, exile, execution)
- Followers pardoned (most uninvolved)
- Reforms sometimes offered (prevent future rebellion)

Example:
Baron Oswyn informs duke of Baron Aldric's conspiracy:
- Aldric arrested before rebellion begins
- Imprisoned (not executed, to avoid martyrdom)
- Sympathizers offered amnesty if swear renewed loyalty
- Duke reduces taxes 10% (goodwill gesture)

Aftermath:
- Would-be rebels (30% population): Disappointed, but no bloodshed
- Loyalists (60% population): Satisfied, rewarded
- Neutrals (10% population): Unaffected
- Aldric in prison, becomes symbol for future reformers
```

**5. Martyrdom** (Defeated but Inspiring)
```
Conditions:
- Rebellion crushed violently
- Rebel leader executed dramatically
- Cause gains sympathy posthumously

Resolution:
- Immediate defeat (rebels killed/scattered)
- Long-term victory (martyrdom inspires future rebellions)

Example:
Peasant Gareth's rebellion crushed, Gareth hanged publicly:
- Duke expects execution to deter future rebels
- Instead, Gareth becomes folk hero
- Ballads sung about "Gareth the Just"
- 15 years later: Larger rebellion uses Gareth's name as rallying cry
- Second rebellion succeeds, Gareth posthumously honored

Aftermath:
- Immediate: Total defeat
- 15 years later: Martyrdom inspires successful revolution
- Duke's dynasty overthrown, Gareth's legacy triumphant
```

---

## Information Warfare & Counter-Intelligence

### Rebel Counter-Intelligence

**Detecting Informants**:
```
Rebel leader suspects informant in conspiracy:

Method 1: False Information Test
- Tell each suspect different false plans
- Whichever plan duke responds to reveals informant
- Example: Tell Baron A "we attack north gate," Baron B "we attack south gate"
- Duke prepares north gate defenses → Baron A is informant

Method 2: Surveillance
- Watch suspects for secret meetings with liege's agents
- Requires high INT, stealth skills
- Example: Rebel spy follows Baron Oswyn, sees meeting with duke's spy master

Method 3: Interrogation (Intimidation/Torture)
- Confront suspect, demand confession
- High risk (if wrong, alienates innocent ally)
- Example: Rebels torture suspected informant, who confesses under duress
```

**Eliminating Informants**:
```
Once informant identified:
- Execution (permanent solution, sends message)
- Feeding False Information (turn informant into asset)
- Exile (remove from conspiracy, less brutal)
- Imprisonment (prevent further leaks)

Example:
Rebels discover Baron Oswyn informed duke:
- Execute Oswyn publicly (warning to other potential informants)
- Duke loses intelligence source
- Rebels gain 4 weeks operational security
- But: Oswyn's execution hardens loyalist resistance (martyr effect)
```

### Liege Counter-Rebellion Intelligence

**Infiltrating Rebels**:
```
Duke sends loyal spy into rebellion:
- Spy poses as begrudged peasant
- Joins rebel conspiracy
- Reports back to duke

Spy Actions:
- Identifies rebel leaders
- Learns rebel plans (attack targets, timing)
- Sabotages (destroys supplies, misinforms rebels)

Example:
Duke's spy "Willem" joins Gareth's rebellion:
- Learns rebels plan to attack grain warehouse (next week)
- Reports to duke
- Duke sets ambush
- Rebels walk into trap, 40 killed, rebellion crippled
```

**Sowing Distrust**:
```
Duke's agents spread rumors among rebels:
- "Baron Aldric plans to betray common rebels after victory"
- "Aldric negotiating secret deal with duke"
- Rebels become suspicious, paranoid

Result:
- Rebel cohesion: 75% → 50% (distrust)
- 20% rebels defect (believe rumors)
- Aldric must purge suspected traitors (kills innocents, worsens cohesion)
- Rebellion weakened by internal conflict
```

---

## ECS Integration

### Mind ECS (1 Hz) - Individual Loyalty

**Systems**:
- `EntityLoyaltyCalculationSystem`: Calculate loyalty to liege vs rebels
- `EntityGrievanceAccumulationSystem`: Track grievances over time
- `EntityRecruitmentTargetingSystem`: Determine if entity is safe rebel recruit
- `EntityInformantDecisionSystem`: Decide if loyalist informs on rebels
- `EntityFactionSwitchSystem`: Handle loyalty shifts during rebellion

**Components**:
```csharp
public struct EntityLoyaltyComponent : IComponentData
{
    public Entity LiegeEntity;
    public float LoyaltyToLiege;       // 0-100
    public float GrievanceLevel;       // 0-100
    public LoyaltyFaction Faction;     // Loyalist, Rebel, Neutral
    public int PersonalBondsWithLiege; // Close relationships
    public int PersonalBondsWithRebels;
    public bool HasIdeologicalMotivation; // True believer vs opportunist
}

public struct EntityRecruitmentRiskComponent : IComponentData
{
    public float InformantRisk;        // 0-1 (probability of informing)
    public RecruitmentTier SafetyTier; // Safe, Risky, Dangerous, Suicidal
    public bool HasBeenContacted;      // Rebels already approached
    public bool InformedOnRebels;      // Reported conspiracy to liege
}

public struct EntityGrievanceComponent : IComponentData
{
    public float TaxBurden;            // 0-1 (heavy taxes increase grievance)
    public int FamilyKilledByLiege;    // Personal tragedy
    public bool DeniedJustice;         // Liege ignored petition
    public bool LandSeized;            // Property confiscated
    public int MonthsSinceLastGrievance;
}

public enum LoyaltyFaction : byte
{
    Loyalist,   // Supports liege
    Rebel,      // Actively resists liege
    Neutral     // Takes no side
}

public enum RecruitmentTier : byte
{
    Safe,       // Loyalty ≤ 25, informant risk < 20%
    Risky,      // Loyalty 26-50, informant risk 30-50%
    Dangerous,  // Loyalty 51-75, informant risk 60-80%
    Suicidal    // Loyalty 76-100, informant risk 90-100%
}
```

### Aggregate ECS (0.2 Hz) - Rebellion Coordination

**Systems**:
- `RebellionInitiationSystem`: Track when rebellion begins (cohesion threshold)
- `RebellionEscalationSystem`: Handle escalation/de-escalation paths
- `RebellionRecruitmentSystem`: Coordinate rebel recruitment campaigns
- `RebellionOutcomeSystem`: Determine rebellion success/failure
- `LoyalistResponseSystem`: Coordinate liege's counter-rebellion

**Components**:
```csharp
public struct RebellionStateComponent : IComponentData
{
    public Entity LiegeEntity;
    public Entity RebelLeaderEntity;
    public RebellionStage Stage;       // Individual, Conspiracy, Movement, Uprising
    public RebellionType Type;         // Violent, Peaceful, Coup, Secession
    public int RebelCount;             // Number of committed rebels
    public int LoyalistCount;          // Number of loyalists
    public int NeutralCount;           // Number of neutrals
    public float MonthsActive;         // Rebellion duration
    public bool IsViolent;             // Armed conflict vs civil disobedience
}

public struct RebellionEscalationComponent : IComponentData
{
    public float EscalationLevel;      // 0-1 (peaceful to total war)
    public EscalationPath Path;        // Escalating, De-escalating, Stalemate
    public float NegotiationWillingness; // 0-1 (both sides' openness to talks)
    public int BattlesFought;
    public int CasualtiesTotal;
}

public struct RebellionRecruitmentComponent : IComponentData
{
    public int Tier1Contacted;         // Safe recruits contacted
    public int Tier2Contacted;         // Risky recruits contacted
    public int InformantsEncountered;  // Loyalists who reported conspiracy
    public float RecruitmentCaution;   // 0-1 (how cautious rebels are)
    public bool ConspiracyExposed;     // Liege knows of rebellion
}

public struct RebellionOutcomeComponent : IComponentData
{
    public OutcomeType Outcome;        // Victory, Defeat, Negotiated, Martyrdom, etc.
    public float RebelSuccessChance;   // 0-1 (calculated from military strength)
    public bool NegotiationsActive;
    public float SettlementTerms;      // 0-1 (favor rebels to favor liege)
}

public enum RebellionStage : byte
{
    Individual,   // 1 person
    Conspiracy,   // 5-20 people
    Movement,     // 50-200 people
    Uprising      // 500+ people, civil war
}

public enum RebellionType : byte
{
    Violent,      // Armed conflict
    Peaceful,     // Civil disobedience
    Coup,         // Internal overthrow
    Secession     // Independence movement
}

public enum EscalationPath : byte
{
    Escalating,     // Violence increasing
    DeEscalating,   // Moving toward peace
    Stalemate       // Neither side advancing
}

public enum OutcomeType : byte
{
    TotalRebelVictory,
    TotalRebelDefeat,
    PartialRebelVictory,    // Negotiated
    RebellionPreempted,     // Crushed before launch
    Martyrdom               // Defeated but inspiring
}
```

---

## Example Scenarios

### Scenario 1: Informant Preempts Rebellion
```
Month 0: Baron Aldric (loyalty to duke: 15, grievance: 80)
- Begins recruiting for rebellion
- Contacts 5 fellow barons

Month 1: Recruitment Results
- Baron Roderick (loyalty 25): Joins (Tier 1 safe)
- Baron Theobald (loyalty 30): Joins (Tier 1 safe)
- Baron Cedric (loyalty 50): Refuses, INFORMS duke (Tier 2 risky)
- Baron Leofric (loyalty 75): Not contacted (Tier 3 dangerous)
- Baron Hamish (loyalty 85): Not contacted (Tier 4 suicidal)

Month 2: Duke's Response
- Cedric's intelligence: "Aldric plotting rebellion, has 2 supporters"
- Duke arrests Aldric, Roderick, Theobald preemptively
- No battle, conspiracy crushed

Outcome:
- Aldric imprisoned (15 years)
- Roderick and Theobald fined, pardoned (swear loyalty)
- Cedric rewarded (1,000 gold, promoted to duke's council)
- Rebellion preempted, 0 casualties
```

### Scenario 2: Neutral Punished by Both Sides
```
Month 0: Baron Oswyn (loyalty 45) declares neutrality in rebellion
- "I will not raise banners for duke or rebels"

Month 2: Duke demands Oswyn join him
- "Your oath binds you. Fight or be branded traitor."
- Oswyn refuses
- Duke confiscates 50% of Oswyn's lands

Month 4: Rebels demand Oswyn join them
- "Neutrality helps the tyrant. Join or be treated as enemy."
- Oswyn refuses
- Rebels raid Oswyn's villages, seize food supplies

Month 6: Oswyn's Position
- Lost 50% lands to duke
- Lost 30% resources to rebels
- Soldiers deserted (split between duke and rebels)
- Bankrupt, powerless

Month 8: Oswyn forced to choose
- Calculates: Duke winning (60% probability)
- Joins duke reluctantly to salvage remaining lands
- Loyalty: 45 → 40 (resentful but pragmatic)

Outcome: Neutrality untenable, forced into loyalist faction
```

### Scenario 3: Peaceful Escalation to Violent Suppression
```
Month 0: Peasants petition duke (reduce taxes from 50% to 30%)
- Duke ignores petition

Month 2: Peasants refuse tax payment (civil disobedience)
- 600 peasants barricade villages peacefully
- No violence yet

Month 3: Duke sends 100 soldiers to enforce taxes
- Soldiers burn 1 village (collective punishment)
- 20 peasants killed

Month 4: Escalation
- Survivors arm themselves
- Attack ducal soldiers (ambush, 15 soldiers killed)
- Rebellion now violent

Month 6: Open battle
- 400 peasant militia vs 300 ducal troops
- Peasants lose (150 killed, rebels scatter)

Month 8: Guerrilla warfare
- Remaining rebels (50) conduct raids from forests
- Duke offers amnesty (war exhaustion)

Month 10: Negotiated settlement
- Rebels disband in exchange for 20% tax reduction
- Amnesty for common rebels, leaders exiled

Outcome: Escalated from peaceful to violent, ended in compromise
```

### Scenario 4: Martyrdom Inspires Future Victory
```
Year 0: Peasant leader Gareth leads tax rebellion
- 200 rebels vs 600 ducal troops
- Rebels crushed, Gareth captured

Year 1: Gareth's Execution
- Hanged publicly in capital square
- Duke expects execution to deter future rebels
- Crowd witnesses execution, many sympathize

Year 2-10: Gareth's Legend Grows
- Ballads sung: "Gareth the Just, who defied tyranny"
- Peasants tell stories: "Gareth fought for us"
- Gareth becomes folk hero, martyr

Year 15: New Rebellion
- Young peasant leader invokes Gareth's name
- "We fight for Gareth's dream!"
- 2,000 rebels rally (Gareth's martyrdom inspires)

Year 16: New Rebellion Succeeds
- Duke overthrown (popular support for rebels)
- Gareth posthumously honored (statue erected)

Outcome: Immediate defeat, long-term victory through martyrdom
```

---

## Key Design Principles

1. **Loyalty Divides**: Every rebellion creates three factions (loyalists, rebels, neutrals), each with different motivations and risks
2. **Informants Are Deadly**: Loyal subjects who report conspiracies can preempt rebellions entirely
3. **Recruitment Is Risky**: Rebels must carefully target known discontents, avoid loyalists
4. **Neutrality Has Costs**: Neutrals punished by both sides (forced to choose or suffer)
5. **Rebellions Escalate**: Start with individuals, grow to mass uprisings (or get crushed early)
6. **Violence vs Peace**: Peaceful rebellions more likely to end in negotiation, violent in total victory/defeat
7. **Outcomes Vary**: Total victory, total defeat, compromise, preemption, martyrdom all possible
8. **Information Warfare**: Counter-intelligence, spy infiltration, false information critical to both sides
9. **Martyrdom Endures**: Defeated rebels can inspire future victories if they become symbols
10. **Timing Matters**: Premature rebellions crushed, delayed rebellions preempted, well-timed rebellions succeed

---

**Integration with Other Systems**:
- **Infiltration Detection**: Liege uses detection systems to identify rebel conspirators before uprising
- **Crisis Alert States**: External threats reduce rebellion risk (rally effect +20%), failed defense increases risk (blame leadership -30%)
- **Aggregate Politics**: Low aggregate cohesion (<30%) triggers rebellion initiation
- **Soul System**: Dead rebel leaders' souls transferred to sympathizers, continue rebellion posthumously
- **Blueprint System**: Rebels capture weapons blueprints, improve militia equipment quality
