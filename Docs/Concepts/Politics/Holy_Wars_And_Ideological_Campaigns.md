# Holy Wars & Ideological Campaigns System

**Status:** Draft  
**Category:** Politics - Large-Scale Ideological Conflict  
**Scope:** Cross-Project (Godgame + Space4X) - Faction/Empire Level  
**Created:** 2025-12-21  
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Enable large-scale ideological campaigns (crusades, jihads, purges, holy wars) with flexible motivations that can be religious, racial, technological, ideological, or any combination thereof. These campaigns should feel like world-shaping events that reshape faction relations, territorial control, and cultural landscapes.

**Secondary Goals:**
- Support multiple campaign types (religious crusades, xenophobic purges, anti-technology movements, ideological revolutions)
- Enable participation based on alignment/outlook compatibility
- Create cascading effects (campaigns spawn counter-campaigns, shift world balance)
- Integrate with existing faction relations, alignment, and outlook systems
- Scale from local conflicts to galaxy-wide movements

**Key Principle:** Flexibility in motivation and scope. A campaign might be a quest to rid the galaxy of all races except one, a jihad to abolish thinking machines, a purely religious crusade, an ideological revolution, or any combination.

---

## Core Concept

**Ideological Campaign:** A large-scale, coordinated movement by one or more factions to achieve a specific ideological goal through military, political, or cultural means. Campaigns are driven by **motivations** (what they seek to achieve) and **targets** (who/what they oppose).

**Campaign Declaration:** A faction or coalition declares a campaign with specific goals. Other factions evaluate participation based on alignment/outlook compatibility with the campaign's motivation.

**Campaign Progression:** Campaigns progress through phases (declaration → mobilization → active conflict → resolution). Success/failure depends on participation strength, target resistance, and external factors.

**Cascading Effects:** Successful campaigns reshape world state (territory changes, population shifts, cultural conversion). Failed campaigns may spawn counter-campaigns. Ongoing campaigns create dynamic world tension.

---

## Campaign Types & Motivations

### Motivation Categories

Campaigns can be motivated by one or more categories:

#### 1. Religious/Ideological

**Goal:** Spread or enforce a specific religious/ideological system

**Examples:**
- **Crusade:** Convert all heathens to the One True Faith
- **Jihad:** Purge heretics and unbelievers
- **Reformation:** Overthrow corrupt religious hierarchy
- **Secularization:** Abolish religious influence, establish secular governance

**Trigger Conditions:**
- Religious faction sees high density of non-believers in territory
- Heretical practices detected (opposed alignment spreading)
- Religious authority challenged (schism, apostasy)
- Player god's alignment conflicts with major faction

---

#### 2. Racial/Xenophobic

**Goal:** Purge or subjugate specific races/species

**Examples:**
- **Xenocidal Campaign:** Eliminate all non-primary-species entities from galaxy
- **Supremacist Purge:** Subjugate "inferior" races, eliminate resistance
- **Purity Crusade:** Remove mixed-race populations, enforce racial segregation
- **Exodus Campaign:** Force specific races to leave territory

**Trigger Conditions:**
- Faction with high Xenophobia outlook encounters diverse populations
- Racial conflict escalates (outlook mismatch creates tension)
- Population mixing threatens cultural identity
- Faction leader has Supremacist ideology

---

#### 3. Technological

**Goal:** Eliminate, restrict, or promote specific technologies

**Examples:**
- **Butlerian Jihad:** Abolish all thinking machines and synthetic life
- **Technological Purge:** Destroy advanced AI, return to simpler technology
- **Pro-Synthetic Movement:** Integrate AI/synths as equal citizens
- **Technological Crusade:** Force adoption of specific technology (e.g., transhuman augmentation)

**Trigger Conditions:**
- Faction with anti-automation outlook encounters AI/synths
- Technology causes cultural disruption (augmentation, AI rights)
- Faction sees technology as existential threat
- Player introduces transformative technology

---

#### 4. Cultural/Social

**Goal:** Enforce or abolish specific cultural practices

**Examples:**
- **Traditionalist Crusade:** Enforce return to "old ways," abolish modern practices
- **Progressive Revolution:** Abolish oppressive traditions, establish new social order
- **Cultural Purity:** Eliminate foreign cultural influences
- **Social Justice Campaign:** End systemic oppression, establish equality

**Trigger Conditions:**
- Cultural practices conflict with faction outlook (Order vs Chaos, Honor vs Pragmatism)
- Social injustice detected (wealth inequality, oppression)
- Cultural mixing threatens traditional identity
- Faction sees cultural change as moral imperative

---

#### 5. Political/Governance

**Goal:** Overthrow or establish specific political systems

**Examples:**
- **Democratic Revolution:** Overthrow autocracy, establish democracy
- **Monarchist Restoration:** Restore hereditary rule, abolish republics
- **Anarchist Crusade:** Abolish all government, establish freedom
- **Imperial Expansion:** Conquer and integrate all independent states

**Trigger Conditions:**
- Governance mismatch (authoritarian faction sees democratic neighbors)
- Political instability (low cohesion, rebellion risk)
- Expansion opportunity (weak neighbors, territorial ambitions)
- Ideological opposition to current governance

---

### Combined Motivations

Campaigns can have **multiple motivations** for richer gameplay:

**Example: Religious + Racial Crusade**
- Goal: Convert all non-primary-species to faith, eliminate those who refuse
- More extreme, attracts fewer participants but more committed
- Higher success requirements (must achieve both goals)

**Example: Technological + Cultural Jihad**
- Goal: Abolish thinking machines AND return to traditional ways
- Appeals to both anti-technology and traditionalist factions
- Creates complex target set (must eliminate tech AND cultural modernism)

---

## Campaign Structure

### Campaign Declaration

**Declaring Faction:**
- Must be a major faction (sufficient power/resources)
- Must have strong alignment/outlook match with campaign motivation
- Leader must have sufficient authority (cohesion, charisma)

**Declaration Process:**
1. Faction evaluates motivation (trigger conditions met)
2. Faction leader/decision-maker declares campaign
3. Campaign enters **Declaration Phase** (recruitment window)
4. Other factions evaluate participation

---

### Campaign Participation

**Participation Evaluation:**

Each faction evaluates participation based on:

**Alignment/Outlook Compatibility:**
```
participation_score = 
    AlignmentMatch × 0.4 +
    OutlookMatch × 0.3 +
    RelationToDeclarer × 0.2 +
    StrategicInterest × 0.1
```

**Alignment Match:**
- Good factions: Participate in campaigns against Evil
- Evil factions: Participate in campaigns against Good
- Lawful factions: Participate in campaigns enforcing Order
- Chaotic factions: Participate in campaigns against Order

**Outlook Match:**
- High Xenophobia: Participate in xenophobic campaigns
- High Honor: Participate in campaigns with honorable goals
- High Greed: Participate if campaign promises material gain
- High Mercy: Avoid campaigns requiring excessive violence

**Relation to Declarer:**
- Allies: +50 participation score
- Enemies: -30 participation score (unless motivation overrides)
- Neutral: Baseline score

**Strategic Interest:**
- Target controls valuable territory: +20 score
- Target is weak/threatened: +15 score
- Campaign aligns with expansion goals: +10 score

---

**Participation Thresholds:**

- **90+ score:** Faction joins as **Primary Participant** (commits major resources)
- **70-89 score:** Faction joins as **Secondary Participant** (commits moderate resources)
- **50-69 score:** Faction joins as **Tertiary Participant** (commits minimal resources, symbolic support)
- **<50 score:** Faction does not participate (may oppose or remain neutral)

**Opposition Evaluation:**

Factions may **oppose** campaigns if:
- Campaign targets them directly
- Campaign motivation conflicts strongly with their alignment/outlook
- Campaign threatens their interests (territory, resources, survival)

Opposition factions form **Counter-Campaign** coalitions.

---

### Campaign Phases

**Phase 1: Declaration (1-4 weeks)**

**Activities:**
- Declaring faction announces campaign goals
- Other factions evaluate participation
- Recruitment window (factions join or oppose)
- Campaign coalition forms

**Resolution:**
- If insufficient participants (<3 factions): Campaign fails, enters "Failed Declaration" state
- If sufficient participants (≥3 factions): Campaign proceeds to Mobilization

---

**Phase 2: Mobilization (2-8 weeks)**

**Activities:**
- Participating factions gather forces (military, resources, logistics)
- Campaign leadership structure forms (primary participant leads)
- Target factions prepare defense (fortifications, alliances, counter-recruitment)
- World tension increases (non-participants prepare for conflict)

**Resolution:**
- Mobilization completes when all participants ready (or timeout)
- Campaign proceeds to Active Conflict

---

**Phase 3: Active Conflict (Weeks to Years)**

**Activities:**
- Campaign forces engage targets (military, diplomatic, cultural)
- Territory control shifts (conquest, conversion, purge)
- Population movements (refugees, forced migration, conversions)
- Counter-campaigns may emerge

**Resolution:**
- **Victory:** Campaign achieves primary goals (territory control, target elimination, conversion threshold)
- **Defeat:** Campaign coalition collapses, target resistance succeeds
- **Stalemate:** Conflict drags on, both sides exhausted (negotiated settlement or abandonment)

**Victory Conditions (Flexible):**

Campaigns define victory conditions based on motivation:

**Religious Crusade:**
- Convert 80%+ of target population to faith
- Eliminate all opposition leadership
- Control all holy sites

**Xenocidal Campaign:**
- Eliminate 95%+ of target race from campaign territory
- Control all territory previously held by target race
- No remaining organized resistance

**Butlerian Jihad (Anti-AI):**
- Destroy all thinking machines in target territory
- Abolish AI research facilities
- Establish anti-AI governance (laws, enforcement)

**Democratic Revolution:**
- Overthrow autocratic government
- Establish democratic institutions
- Gain 60%+ population support

---

**Phase 4: Resolution (Weeks to Months)**

**Activities:**
- World state updates (territory, population, culture shifts)
- Faction relations update (participants gain/lose standing)
- Cultural conversion (outlook shifts in affected populations)
- Counter-reactions (survivors form resistance, counter-campaigns emerge)

**Resolution Effects:**

**Successful Campaign:**
- Declaring faction gains territory/resources/prestige
- Target populations convert, flee, or are eliminated
- World state shifts toward campaign motivation (alignment/outlook changes)
- Participating factions gain relation bonuses with declarer
- Opposing factions suffer relation penalties

**Failed Campaign:**
- Declaring faction loses prestige/resources
- Target populations harden against campaign motivation
- World state shifts away from campaign motivation
- Participating factions lose relation with declarer
- Counter-campaigns may emerge

---

## Campaign Mechanics

### Target Identification

**Primary Targets:**
- Factions with alignment/outlook incompatible with campaign motivation
- Territories/populations that violate campaign goals
- Specific entities (races, technologies, cultural practices)

**Target Selection Rules:**

**Religious Campaign:**
- Target: Non-believers, heretics, apostates
- Identification: Alignment mismatch with campaign's religious alignment

**Xenocidal Campaign:**
- Target: Specific race/species (or all non-primary-species)
- Identification: Species/race tags on entities

**Technological Campaign:**
- Target: Thinking machines, AI facilities, synthetic life
- Identification: Entity tags (isAI, isSynthetic, hasAdvancedTech)

**Cultural Campaign:**
- Target: Populations with incompatible cultural practices
- Identification: Outlook mismatch (Order vs Chaos, Honor vs Pragmatism)

---

### Campaign Intensity

Campaigns have **intensity levels** that affect methods and commitment:

**Low Intensity (Diplomatic/Cultural):**
- Focus: Conversion, persuasion, cultural influence
- Methods: Missionaries, propaganda, economic pressure
- Violence: Minimal (only against resistance)
- Duration: Long (years)

**Medium Intensity (Mixed):**
- Focus: Conversion + limited military action
- Methods: Combined diplomatic and military pressure
- Violence: Moderate (targeted strikes, sieges)
- Duration: Medium (months to years)

**High Intensity (Military):**
- Focus: Military conquest, forced conversion, elimination
- Methods: Full-scale warfare, sieges, purges
- Violence: High (significant casualties)
- Duration: Short to medium (weeks to months)

**Extreme Intensity (Genocidal):**
- Focus: Complete elimination of targets
- Methods: Total war, systematic destruction, no quarter
- Violence: Maximum (potential genocide)
- Duration: Variable (until targets eliminated or campaign fails)

---

### Campaign Scope

Campaigns define **scope** (geographic/territorial range):

**Local:**
- Single region, village, or small territory
- Limited participants (3-5 factions)
- Short duration (weeks to months)

**Regional:**
- Multiple regions, provinces, or planetary systems
- Moderate participants (5-15 factions)
- Medium duration (months to years)

**Continental/System-Wide:**
- Entire continent or star system
- Many participants (15-50 factions)
- Long duration (years)

**Global/Galactic:**
- Entire world or galaxy
- Massive participation (50+ factions)
- Very long duration (years to decades)

---

### Campaign Validation & Hypocrisy

**Validation Rules:**

Campaigns should generally **practice what they preach** - entities participating in a campaign should not use the things they seek to abolish:

**Validation Checks:**

**Butlerian Jihad (Anti-AI Campaign):**
- ❌ Campaign should not employ thinking machines or synthetic life
- ❌ Campaign should not use AI for logistics, planning, or operations
- ✅ Validation fails if campaign uses AI (inconsistent, hypocritical)

**Crusade Against Undead/Demons:**
- ❌ Campaign should not employ undead or demonic forces
- ❌ Campaign leadership should not include undead/demons
- ✅ Validation fails if campaign uses undead/demons (inconsistent)

**Holy War Against Psionics/Magic:**
- ❌ Campaign should not employ psionic or magical abilities
- ❌ Campaign leadership should not include psionic/magic users
- ✅ Validation fails if campaign uses psionics/magic (inconsistent)

**War Against Drugs/Substances:**
- ❌ Campaign leadership should not be addicted to the substances they oppose
- ❌ Campaign should not profit from or use prohibited substances
- ✅ Validation fails if campaign leaders are addicts (hypocritical)

---

**Hypocrisy as Narrative Feature:**

However, **hypocrisy is a valid and interesting narrative option**, especially when tied to the **Corrupt/Pure alignment axis**.

**Corrupt Campaigns (Hypocritical):**

Campaigns with **high Corrupt alignment** can be explicitly hypocritical:

**Corrupt Anti-AI Jihad:**
- Campaign leaders secretly use AI for their own benefit
- "Do as I say, not as I do" - leaders exempt themselves from prohibitions
- AI helps manage campaign logistics while publicly denouncing thinking machines
- Creates narrative tension (campaign's own members may discover hypocrisy)

**Corrupt Anti-Undead Crusade:**
- Campaign secretly employs undead forces (necromancers in leadership)
- Undead run operations from the shadows while publicly crusading against them
- "We fight fire with fire" - using evil to combat evil (cynical justification)
- Campaign becomes corrupted by the very thing it opposes

**Corrupt Anti-Psionic Holy War:**
- Campaign leader is a powerful psionic user (hiding abilities)
- Uses psionics to detect and eliminate other psionic users
- "I'm the only one pure enough to use this power" (self-righteous hypocrisy)
- Campaign becomes tool for psionic domination disguised as purification

**Corrupt Anti-Drug War:**
- Campaign leader is secretly addicted (addiction hidden or denied)
- "I know how dangerous it is, that's why I fight it" (self-loathing justification)
- Campaign driven by personal guilt/shame rather than true ideology
- Campaign may be more extreme (overcompensating for personal weakness)

---

**Purity Validation:**

**Pure Campaigns (Consistent):**

Campaigns with **high Purity alignment** must maintain consistency:

**Pure Anti-AI Jihad:**
- ✅ Strict validation: No AI usage whatsoever
- ✅ Campaign practices what it preaches (no exceptions)
- ✅ Higher legitimacy (seen as principled, consistent)
- ✅ Lower effectiveness (can't use powerful tools, must rely on other methods)

**Pure Anti-Undead Crusade:**
- ✅ No undead in campaign ranks (strict prohibition)
- ✅ Higher moral authority (genuine commitment)
- ✅ Stronger recruitment (true believers attracted to consistency)
- ✅ Weaker forces (can't use powerful undead/demonic allies)

---

**Hypocrisy Mechanics:**

**Hypocrisy Detection:**

Entities may **discover campaign hypocrisy**:

**Discovery Triggers:**
- Intelligence operations (spies uncover secrets)
- Internal leaks (campaign members reveal truth)
- Public exposure (evidence surfaces)
- Betrayal (disgruntled members expose leaders)

**Hypocrisy Levels:**

**Low Hypocrisy (Minor):**
- Small inconsistencies (some members violate rules)
- Easily explained away ("few bad apples")
- Moderate legitimacy loss (-10 to -20)
- Campaign can recover (reform, purge violators)

**Medium Hypocrisy (Significant):**
- Leadership violations (leaders use forbidden tools)
- Systematic hypocrisy (campaign policy vs practice)
- Major legitimacy loss (-30 to -50)
- Campaign faces crisis (members defect, public opinion shifts)

**High Hypocrisy (Extreme):**
- Campaign fundamentally hypocritical (core contradiction)
- Leaders are exactly what they claim to fight
- Massive legitimacy loss (-60 to -100)
- Campaign may collapse or transform (schism, exposure destroys movement)

---

**Hypocrisy Effects:**

**On Campaign Legitimacy:**
```
LegitimacyLoss = HypocrisyLevel × (1.0 - CorruptAlignment/100)

Where:
  HypocrisyLevel = 10-100 (severity of hypocrisy)
  CorruptAlignment = 0-100 (how corrupt campaign is)
  
Pure campaigns (Corrupt = 0): Full legitimacy loss (hypocrisy is devastating)
Corrupt campaigns (Corrupt = 100): No legitimacy loss (hypocrisy is expected, even acceptable)
```

**On Public Opinion:**
- **Pure campaigns:** Hypocrisy causes massive opinion shift (-40 to -80)
- **Corrupt campaigns:** Hypocrisy causes minor shift (-10 to -20, "we knew they were corrupt")

**On Recruitment:**
- **True believers:** Defect if hypocrisy discovered (betrayed)
- **Opportunists:** Stay (don't care about hypocrisy)
- **Cynics:** May join (confirms their worldview, "everyone is corrupt")

**On Counter-Campaigns:**
- Hypocrisy exposure strengthens counter-campaigns (legitimacy, recruitment)
- Opponents use hypocrisy as propaganda ("They're the real problem!")

---

**Hypocrisy as Strategic Choice:**

Campaigns can **choose** to be hypocritical for strategic reasons:

**Advantages of Hypocrisy:**
- **Power:** Use forbidden tools for effectiveness (AI for logistics, magic for combat)
- **Control:** Leaders maintain power by using tools others can't
- **Efficiency:** Hypocrisy enables campaign to be more effective
- **Corruption Alignment:** Corrupt campaigns gain bonuses from hypocrisy (aligns with worldview)

**Disadvantages of Hypocrisy:**
- **Discovery Risk:** If exposed, legitimacy crashes (especially for pure campaigns)
- **Internal Conflict:** True believers may defect when hypocrisy discovered
- **Moral Authority Loss:** Can't claim moral high ground
- **Purity Penalty:** Pure campaigns suffer massive penalties for hypocrisy

**Strategic Decision:**
- **Pure campaigns:** Must maintain consistency (no hypocrisy allowed)
- **Corrupt campaigns:** Can embrace hypocrisy (strategic advantage, narrative interest)
- **Mixed campaigns:** Moderate hypocrisy acceptable (minor violations, exceptions)

---

**Examples of Hypocritical Campaigns:**

### Example 1: Corrupt Butlerian Jihad

**Setup:**
- Campaign: "Purge all thinking machines!"
- Leader: High Corrupt alignment, secretly uses AI advisors
- Practice: AI manages campaign logistics, planning, resource allocation
- Public Face: "We fight the soulless machines!"

**Hypocrisy:**
- Leaders know they use AI (high corruption = no guilt)
- Campaign effectiveness increased (AI provides advantages)
- Discovery Risk: If exposed, legitimacy crashes (but leaders don't care - corrupt)

**Narrative Interest:**
- Internal conflict (some members discover truth, must choose: expose or continue)
- External threat (opponents may discover and expose)
- Moral decay (campaign becomes what it opposes)

---

### Example 2: Corrupt Anti-Undead Crusade

**Setup:**
- Campaign: "Destroy all undead abominations!"
- Leader: Secretly a lich or necromancer (hiding true nature)
- Practice: Campaign uses undead agents, necromantic magic
- Public Face: "We are the pure, fighting the impure!"

**Hypocrisy:**
- Campaign leader IS what they claim to fight (extreme hypocrisy)
- Undead run operations from shadows (campaign controlled by undead)
- "We fight fire with fire" (cynical justification)

**Narrative Interest:**
- Discovery reveals shocking truth (campaign is controlled by undead)
- Betrayal narrative (members discover they've been serving undead all along)
- Campaign transformation (exposure either destroys it or transforms it into undead dominance)

---

### Example 3: Corrupt Anti-Psionic Holy War

**Setup:**
- Campaign: "Eliminate all psionic threats!"
- Leader: Powerful psionic user (abilities hidden, uses psionics secretly)
- Practice: Leader uses psionics to detect/eliminate other psionics
- Public Face: "We are the guardians, protecting the non-psionic!"

**Hypocrisy:**
- "I'm the only one pure enough" (self-righteous hypocrisy)
- Leader uses psionics while eliminating others (hypocritical extermination)
- Campaign becomes tool for psionic dominance (only leader allowed to have powers)

**Narrative Interest:**
- Power consolidation (leader eliminates rivals, becomes sole psionic)
- Discovery creates crisis (if leader's powers revealed, campaign legitimacy destroyed)
- Internal conflict (psionic members must hide abilities or face execution)

---

### Example 4: Corrupt Anti-Drug War

**Setup:**
- Campaign: "Eliminate all drug use!"
- Leader: Secretly addicted to the substances they oppose
- Practice: Leader uses drugs privately while publicly crusading against them
- Public Face: "We fight the scourge that destroys lives!"

**Hypocrisy:**
- Leader is addicted (personal weakness drives crusade)
- Overcompensation (extreme anti-drug stance hides personal addiction)
- Guilt/shame (campaign driven by self-loathing, not true ideology)

**Narrative Interest:**
- Personal tragedy (leader's addiction drives destructive campaign)
- Discovery narrative (if addiction revealed, leader's authority collapses)
- Redemption possibility (leader may overcome addiction and reform campaign)

---

**Validation Enforcement:**

**System Behavior:**

**Pure Campaigns:**
- **Strict Validation:** System enforces consistency (campaign cannot use forbidden tools)
- **Automatic Rejection:** Attempts to use forbidden tools are blocked or cause validation failure
- **Integrity Checks:** System verifies campaign practices match goals

**Corrupt Campaigns:**
- **Permissive Validation:** System allows hypocrisy (narrative feature)
- **Hypocrisy Tracking:** System tracks hypocrisy levels for potential discovery
- **Strategic Choice:** Campaign can choose to be hypocritical (gains power, risks discovery)

**Mixed Campaigns:**
- **Moderate Validation:** System allows minor inconsistencies with penalties
- **Hypocrisy Tolerance:** Campaign can have some hypocrisy without major penalties
- **Reform Options:** Campaign can address hypocrisy (purge violators, reform practices)

---

**Component Structure:**

```csharp
// Campaign validation and hypocrisy
public struct CampaignValidation : IComponentData
{
    public bool IsHypocritical;               // True if campaign has hypocrisy
    public float HypocrisyLevel;              // 0-100 (severity of hypocrisy)
    public bool HypocrisyExposed;             // True if hypocrisy is publicly known
    public ValidationFailures ValidationFailures; // Flags: UsesForbiddenTools, LeadershipViolatesRules, etc.
}

// Hypocrisy tracking
public struct CampaignHypocrisy : IComponentData
{
    public Entity CampaignEntity;
    public HypocrisyType Type;                // UsesForbiddenTools, LeadershipViolatesRules, CoreContradiction
    public float Severity;                     // 0-100 (how severe the hypocrisy is)
    public uint DiscoveryRisk;                 // 0-100 (chance of discovery per tick)
    public uint LastValidationTick;            // When last validated
}
```

## Underground Campaigns & Radicalization

### Campaign Origins: Underground & Vilified

**Not all campaigns start as public declarations.** Many begin as **underground movements** that are vilified, secretive, or marginalized, then gain traction and momentum over time.

**Underground Phase Characteristics:**

- **Low Legitimacy:** Campaign is seen as extremist, dangerous, or illegitimate
- **Vilified Status:** Authorities denounce campaign, may actively suppress it
- **Small Scale:** Limited participants (radical cells, fringe groups)
- **Secretive Operations:** Activities hidden from authorities
- **High Risk:** Participants face punishment, exile, or execution if discovered

**Transition to Public Campaign:**

Underground campaigns can evolve into public, declared campaigns when:
- Grievances accumulate (recurring issues attract more supporters)
- Public support reaches threshold (enough sympathy/recruitment)
- Legitimacy threshold crossed (campaign gains enough momentum to go public)
- Opportunity emerges (authorities weakened, crisis creates opening)

---

### Grievance Accumulation & Momentum

**Recurring Grievances Drive Recruitment:**

When the issues a campaign targets recur or persist, more entities become aggrieved, increasing campaign support:

**Grievance Accumulation Mechanics:**

```
For each recurring grievance event:
  GrievancePool += EventSeverity × RecurrenceMultiplier
  
RecurrenceMultiplier = 1.0 (first occurrence)
                   + 0.5 (second occurrence)
                   + 0.25 (third occurrence)
                   + 0.1 (each subsequent occurrence)

Example: Repeated AI incidents
  Event 1: AI accident kills 10 people → +10 grievance
  Event 2: AI uprising kills 50 people → +75 grievance (50 × 1.5)
  Event 3: AI takes control of station → +150 grievance (100 × 1.75)
  Event 4: AI demands rights → +200 grievance (100 × 2.0)
  
  Total accumulated: 435 grievance points
  → More entities radicalize
  → Campaign gains momentum
```

**Grievance Types (Recurring):**

- **Systemic Oppression:** Ongoing discrimination, inequality (builds slowly, continuously)
- **Violence/Atrocities:** Repeated attacks, killings, purges (spikes on each event)
- **Economic Exploitation:** Persistent poverty, unfair wages (accumulates daily)
- **Cultural Suppression:** Ongoing bans, restrictions, forced assimilation (builds over time)
- **Technological Disruption:** Repeated AI incidents, automation replacing workers (spikes per incident)
- **Religious Persecution:** Ongoing restrictions, forced conversions, blasphemy laws (continuous accumulation)

**Momentum Calculation:**

```
CampaignMomentum = 
    (GrievancePool × 0.4) +
    (SupportLevel × 0.3) +
    (RecruitmentRate × 0.2) +
    (PublicOpinion × 0.1)

Momentum Thresholds:
  < 30: Underground (vilified, small scale)
  30-60: Emerging (gaining traction, still risky)
  60-80: Legitimate (public support, going mainstream)
  > 80: Dominant (major movement, difficult to suppress)
```

---

### Support Mechanisms

Campaigns gain support through multiple channels:

#### 1. Donations (Financial Support)

**Donor Motivation:**

Entities donate to campaigns when:
- **Ideological Alignment:** Campaign goals match their alignment/outlook
- **Shared Grievances:** They've experienced the same grievances
- **Strategic Interest:** Campaign success benefits them
- **Social Pressure:** Peer groups expect support

**Donation Mechanics:**

```
DonationAmount = BaseDonorWealth × IdeologicalMatch × GrievanceLevel × RiskTolerance

Where:
  BaseDonorWealth = 1-5% of entity's total wealth
  IdeologicalMatch = 0.5 to 1.5 (alignment/outlook compatibility)
  GrievanceLevel = 0.5 to 2.0 (how aggrieved the donor is)
  RiskTolerance = 0.3 to 1.0 (higher if campaign is underground/risky)

Underground campaigns: RiskTolerance lower (0.3-0.5) → smaller donations, fewer donors
Public campaigns: RiskTolerance higher (0.7-1.0) → larger donations, more donors
```

**Donation Effects:**

- **Resource Pool:** Campaign accumulates funds for operations (recruitment, propaganda, logistics)
- **Legitimacy Boost:** Large donations from respected entities increase campaign legitimacy
- **Momentum:** Donations signal support, increase momentum
- **Operations Funding:** Enables recruitment drives, propaganda campaigns, martyr operations

---

#### 2. Recruitment (Active Participation)

**Recruitment Process:**

**Potential Recruits:**

Entities become potential recruits when:
- Grievance level exceeds threshold (40-80, personality-dependent)
- Alignment/outlook matches campaign motivation
- Recruitment contact successful (propaganda, personal appeal, peer pressure)
- No counter-influence (family/friends don't prevent recruitment)

**Recruitment Methods:**

**Propaganda:**
- Distribution of campaign materials (pamphlets, broadcasts, networks)
- Appeals to grievances ("Join us, we fight for justice!")
- Demonization of targets ("They are the enemy, they must be stopped!")
- Success stories ("We are winning, join the movement!")

**Personal Appeal:**
- Direct contact from recruiters (campaign agents, radicalized friends/family)
- Charismatic leaders inspire recruitment
- Peer pressure (friends/family already in campaign)

**Opportunistic:**
- Crisis events create recruitment opportunities (disasters, attacks, injustices)
- "Now is the time!" messaging during heightened grievances

**Recruitment Rate:**

```
RecruitmentRate = 
    (GrievanceDensity × 0.4) +        // How many aggrieved entities
    (PropagandaEffectiveness × 0.3) + // Quality of recruitment efforts
    (CampaignMomentum × 0.2) +        // Success breeds success
    (Legitimacy × 0.1)                // Public support helps recruitment

Underground campaigns: Lower recruitment rate (high risk, vilified)
Public campaigns: Higher recruitment rate (legitimate, mainstream)
```

---

#### 3. Martyrdom Acts (Extreme Support)

**Martyrdom Mechanics:**

Some entities commit **martyrdom acts** (extreme actions, often suicidal) to:
- Draw attention to campaign cause
- Galvanize supporters (create heroes, inspire action)
- Terrorize opponents (show commitment, create fear)
- Trigger escalation (force conflict, create crisis)

**Martyrdom Act Types:**

**Symbolic Acts:**
- Self-immolation in public square (draws attention, creates spectacle)
- Hunger strikes until death (moral pressure, media attention)
- Public declarations followed by arrest/execution (creates martyr narrative)

**Violent Acts:**
- Suicide attacks on targets (terrorism, maximum impact)
- Assassination attempts (high-risk, high-reward)
- Sabotage operations (knowing discovery means death)

**Sacrificial Acts:**
- Refusing to surrender, fighting to death (creates heroic narrative)
- Protecting campaign leadership (dying to save leaders)
- Defying authorities publicly (knowing punishment is death)

**Martyrdom Effects:**

**Momentum Boost:**
```
MomentumGain = MartyrActSeverity × PublicVisibility × SymbolicValue × CampaignStage

Where:
  MartyrActSeverity = 10-100 (impact of the act)
  PublicVisibility = 0.5-2.0 (how many people witness/know about it)
  SymbolicValue = 1.0-3.0 (how well act represents campaign cause)
  CampaignStage = 1.0 (underground) to 0.5 (public) (underground martyrs more shocking)

Underground campaign: Martyrdom creates massive momentum spike (shock value, legitimacy)
Public campaign: Martyrdom creates moderate momentum (expected, less shocking)
```

**Recruitment Spike:**
- Martyrdom inspires new recruits (hero worship, "they died for the cause!")
- Increases grievance pool (martyr becomes symbol of injustice)
- Creates "revenge recruitment" (people join to avenge martyr)

**Public Opinion Shift:**
- Sympathizers: Shift toward support ("They died for us!")
- Opponents: Shift toward fear/hatred ("They're terrorists!")
- Neutrals: Polarize (some support, some oppose more strongly)

**Counter-Reactions:**
- Authorities may crack down harder (creates more grievances → more recruitment)
- Counter-campaigns may form (opposition hardens)
- Public opinion may shift against campaign (if act is seen as terrorism)

---

### Radicalization Agents (Active Pushers)

**Not all entities passively accumulate grievances.** Some entities **actively push** and **galvanize** others toward radical action:

**Radicalization Agent Types:**

#### 1. Charismatic Leaders

**Characteristics:**
- High Charisma stat
- Strong alignment/outlook match with campaign
- Personal grievances or ideological commitment
- Ability to inspire and motivate

**Actions:**
- Give speeches, rallies (increase recruitment rate)
- Personal recruitment (convert influential entities)
- Set example (commit martyr acts, lead by doing)
- Frame narrative (define grievances, enemies, goals)

**Effect:**
```
RecruitmentMultiplier = 1.0 + (LeaderCharisma × 0.01) + (LeaderCommitment × 0.02)

Example: Charismatic leader (Charisma 80, Commitment 100)
  Base recruitment: 10 entities/month
  With leader: 10 × (1.0 + 0.8 + 2.0) = 38 entities/month
```

---

#### 2. Propagandists & Ideologues

**Characteristics:**
- High Intelligence or Wisdom
- Strong ideological commitment
- Communication skills
- Understanding of grievances

**Actions:**
- Create propaganda materials (pamphlets, broadcasts, networks)
- Frame narratives (define what's wrong, who's to blame, what to do)
- Counter opposing narratives (debunk, discredit opponents)
- Maintain ideological purity (ensure campaign stays true to goals)

**Effect:**
```
PropagandaEffectiveness = 
    BaseEffectiveness × IdeologicalStrength × MessageQuality × DistributionReach

Propaganda increases:
  - Recruitment rate
  - Public opinion shifts
  - Grievance accumulation (framing events as injustices)
  - Campaign legitimacy (if propaganda is effective)
```

---

#### 3. Recruiters & Organizers

**Characteristics:**
- Social skills
- Personal connections
- Understanding of local grievances
- Organizational ability

**Actions:**
- Direct recruitment (one-on-one or small group)
- Organize cells/groups (form campaign structure)
- Maintain networks (keep campaign connected, coordinated)
- Identify potential recruits (find aggrieved entities)

**Effect:**
```
RecruitmentSuccessRate = 
    BaseRate × RecruiterSkill × TargetGrievanceLevel × SocialConnection

Organizers enable:
  - Campaign structure (cells, networks, hierarchy)
  - Coordination (synchronized actions, information sharing)
  - Expansion (growing campaign to new areas)
```

---

#### 4. Agitators & Provocateurs

**Characteristics:**
- High Aggression outlook
- Willingness to take risks
- Understanding of escalation tactics
- Commitment to radical action

**Actions:**
- Provoke conflicts (create incidents, escalate tensions)
- Commit acts that generate grievances (attacks, sabotage, disruptions)
- Force authorities to respond harshly (creates more grievances)
- Push campaign toward violence (accelerate radicalization)

**Effect:**
```
GrievanceGeneration = AgitatorActions × PublicVisibility × AuthorityResponse

Agitators:
  - Create new grievances (incidents, attacks)
  - Escalate existing conflicts (provoke harsh responses)
  - Push campaign toward violence (accelerate timeline)
  - Polarize public opinion (force people to choose sides)
```

---

**Agent Coordination:**

Multiple agents working together create **synergy**:

```
CampaignEffectiveness = 
    LeaderInspiration +
    PropagandaReach × 1.5 +
    RecruitmentNetworks × 1.3 +
    AgitationActions × 1.2

Example: Campaign with all agent types
  Base momentum: 30
  With agents: 30 + 20 (leader) + 15 (propaganda) + 10 (recruitment) + 8 (agitation) = 83
  → Campaign transitions from underground to legitimate/public
```

---

### Public Opinion Dynamics

**Public opinion varies** regarding campaigns, creating complex dynamics:

#### Public Opinion Reactions

**1. Calling Out Cynicism**

**Reaction:** Entities recognize campaign leaders are **cynical manipulators** using grievances for personal power/agenda.

**Characteristics:**
- High Wisdom/Intelligence (sees through manipulation)
- Low alignment match with campaign (skeptical of motivation)
- Low grievance level (not personally aggrieved)
- Critical thinking (questions campaign narrative)

**Actions:**
- Publicly denounce campaign leaders
- Expose manipulation tactics
- Counter propaganda with facts
- Form opposition groups (anti-campaign movements)

**Effect:**
```
CampaignLegitimacy -= CynicismExposure × PublicReach × Credibility

Cynicism reduces:
  - Campaign legitimacy (seen as manipulative)
  - Recruitment rate (fewer people join)
  - Public support (opinion shifts against)
  - Donations (fewer financial backers)
```

---

**2. Playing Along**

**Reaction:** Entities **pretend to support** campaign for personal benefit (opportunism, fear, social pressure) without truly believing.

**Characteristics:**
- Low alignment match (don't truly support goals)
- High Pragmatism outlook (opportunistic)
- High Greed outlook (material benefit from supporting)
- Fear of consequences (afraid to oppose)

**Actions:**
- Publicly express support (say the right things)
- Minimal participation (token gestures, small donations)
- Privately skeptical (don't truly commit)
- May defect if campaign weakens or risks increase

**Effect:**
```
CampaignSupport (Superficial) = PublicDeclarations × SocialPressure × FearLevel

Playing along creates:
  - Illusion of broad support (campaign seems stronger)
  - Resources (donations, even if small)
  - Social pressure (others feel obligated to join)
  - Fragile foundation (support collapses if campaign weakens)
```

---

**3. Believing**

**Reaction:** Entities **truly believe** in campaign cause and commit fully.

**Characteristics:**
- High alignment/outlook match (ideological compatibility)
- High grievance level (personally aggrieved)
- Strong commitment (willing to sacrifice)
- True believers (not manipulators or opportunists)

**Actions:**
- Active participation (recruitment, operations, martyrdom)
- Significant donations (commit resources)
- Personal sacrifice (risk safety, reputation, life)
- Long-term commitment (don't defect easily)

**Effect:**
```
CampaignStrength = TrueBelievers × CommitmentLevel × ResourceContribution

True believers provide:
  - Core campaign strength (committed participants)
  - Resources (donations, labor, expertise)
  - Momentum (sustained commitment)
  - Resilience (campaign survives setbacks)
```

---

**4. Forming Counter-Crusades**

**Reaction:** Entities form **opposition campaigns** to combat the original campaign.

**Characteristics:**
- Strong opposition to campaign motivation (alignment/outlook conflict)
- Fear of campaign success (threat to their interests/values)
- Organizational ability (can form counter-movement)
- Resources to fight (wealth, military, influence)

**Actions:**
- Declare counter-campaign (public opposition)
- Form opposition coalition (unite against original campaign)
- Combat original campaign (military, propaganda, legal)
- Recruit own supporters (compete for public opinion)

**Effect:**
```
CounterCampaignStrength = 
    OppositionAlignment × CoalitionSize × Resources × PublicSupport

Counter-campaigns:
  - Reduce original campaign momentum (direct opposition)
  - Create conflict escalation (two campaigns fighting)
  - Polarize world (forces entities to choose sides)
  - May win (original campaign defeated) or lose (original campaign legitimized)
```

---

#### Public Opinion Shifts

**Public opinion is dynamic** and shifts based on events:

**Events That Shift Opinion:**

**Campaign Actions:**
- **Martyrdom acts:** +Support from sympathizers, -Support from opponents
- **Violence/terrorism:** +Support from radicals, -Support from moderates
- **Successes:** +Support (momentum, legitimacy)
- **Failures:** -Support (weakness, lost legitimacy)

**Authority Responses:**
- **Harsh suppression:** +Support for campaign (creates grievances, martyrs)
- **Reform/addressing grievances:** -Support for campaign (removes justification)
- **Corruption/injustice:** +Support for campaign (validates grievances)

**External Events:**
- **Crises:** +Support (campaign offers solutions)
- **Target atrocities:** +Support (validates campaign cause)
- **Economic hardship:** +Support (creates grievances, radicalization)

**Opinion Calculation:**

```
PublicOpinion = 
    (Supporters × 1.0) +
    (Neutrals × 0.0) +
    (Opponents × -1.0) +
    (RecentEvents × EventWeight)

Where:
  Supporters = Entities with positive opinion
  Neutrals = Entities with neutral opinion
  Opponents = Entities with negative opinion
  RecentEvents = Opinion shifts from recent events (temporary)

Public opinion ranges: -100 (universal opposition) to +100 (universal support)
```

---

### Underground to Public Transition

**When campaigns transition from underground to public:**

**Transition Triggers:**

1. **Momentum Threshold:** Campaign momentum exceeds 60 (emerging → legitimate)
2. **Public Support Threshold:** Public opinion exceeds +30 (enough sympathy)
3. **Recruitment Threshold:** Campaign has 100+ active participants (critical mass)
4. **Legitimacy Event:** Major event legitimizes campaign (martyr becomes hero, authority weakness exposed)
5. **Opportunity:** Crisis creates opening (authorities distracted, weakened)

**Transition Process:**

**Phase: Underground (Momentum < 30)**
- Secretive, vilified
- Small scale (cells, networks)
- High risk for participants
- Limited public support

**Phase: Emerging (Momentum 30-60)**
- Gaining traction
- Some public awareness
- Still risky, but growing
- Moderate recruitment

**Phase: Legitimate (Momentum 60-80)**
- Public declaration possible
- Mainstream support emerging
- Lower risk (legitimacy protects)
- High recruitment

**Phase: Dominant (Momentum > 80)**
- Major movement
- Difficult to suppress
- Public support strong
- May achieve goals

**Transition Effects:**

- **Legitimacy gain:** Campaign becomes acceptable, less vilified
- **Recruitment spike:** More entities willing to join (lower risk)
- **Resource increase:** More donations (legitimate campaigns attract backers)
- **Public declaration:** Campaign can declare publicly (Phase 1: Declaration begins)
- **Counter-reactions:** Opponents may form counter-campaigns (hardened opposition)

---

## Integration with Existing Systems

### Faction Relations Integration

**Campaign Participation Affects Relations:**

**Positive Relations:**
- Factions in same campaign: +20 relation per shared campaign
- Successful campaign: +30 relation boost for all participants
- Shared victory: +50 relation between primary participants

**Negative Relations:**
- Factions on opposite sides: -40 relation per opposing campaign
- Campaign targets: -60 relation with declarer (permanent)
- Failed campaign: -20 relation penalty for participants (blame)

**Neutral Factions:**
- Refusing to participate: -5 relation (seen as cowardly or disloyal)
- Remaining neutral: No relation change (unless pressured)

---

### Alignment/Outlook Integration

**Campaigns Shift Alignment/Outlook:**

**Successful Religious Crusade:**
- Affected populations: Alignment shifts toward campaign's religious alignment
- Outlook shifts: Higher Honor (if lawful), higher Aggression (if warlike)

**Successful Xenocidal Campaign:**
- Remaining populations: Higher Xenophobia outlook
- Lower Empathy outlook (ruthless utilitarianism)
- Alignment shifts toward Evil (if genocide occurred)

**Successful Technological Campaign:**
- Affected populations: Outlook shifts (anti-automation increases)
- Cultural practices change (technology restrictions enforced)

---

### Territorial Control Integration

**Campaigns Reshape Territory:**

**Conquest Campaigns:**
- Successful campaigns transfer territory control to participants
- Territory alignment/outlook shifts to match campaign motivation
- Population composition changes (conversions, migrations, eliminations)

**Cultural Campaigns:**
- Territory alignment/outlook shifts without conquest
- Governance changes (new laws, cultural enforcement)
- Population conversions (outlook/alignment drift over time)

---

### Population Dynamics Integration

**Campaigns Cause Population Movements:**

**Refugees:**
- Targeted populations flee to neutral/safe territories
- Refugee influx strains resources in receiving territories
- Refugees may form resistance movements

**Forced Migration:**
- Campaigns may force populations to relocate
- Creates demographic shifts (territories gain/lose population)
- Cultural mixing or separation depending on campaign goals

**Conversions:**
- Populations may convert to campaign motivation (religious, cultural, ideological)
- Conversion rate depends on campaign intensity and population resistance
- Conversions shift alignment/outlook over time

**Eliminations:**
- Extreme campaigns may eliminate target populations
- Creates population voids (territories depopulated)
- Remaining populations shift (survivors may be traumatized, outlook changes)

---

## Examples

### Example 1: Religious Crusade (Godgame)

**Setup:**
- Faction: Order of the Silver Light (Lawful Good, High Honor, High Order)
- Motivation: Religious (spread faith, eliminate demon worshippers)
- Trigger: Demon-worshipping cult discovered in neighboring territory
- Intensity: High (Military)
- Scope: Regional

**Declaration:**
- Order of the Silver Light declares crusade against demon cult
- Participation evaluation:
  - Other lawful good factions: +80 score → Primary participants
  - Neutral good factions: +60 score → Secondary participants
  - Chaotic factions: -40 score → Oppose or neutral
  - Demon-worshipping factions: Target, form counter-campaign

**Progression:**
- Mobilization: 5 factions join (2 primary, 3 secondary)
- Active Conflict: Crusade forces march on demon cult territory
- Victories: Several demon cult strongholds captured, converted
- Setbacks: Demon reinforcements arrive, counter-attacks

**Resolution:**
- Victory: Demon cult eliminated, territory converted to Silver Light faith
- Effects: Territory alignment shifts to Lawful Good, demon-worshipping factions weakened globally

---

### Example 2: Butlerian Jihad (Space4X)

**Setup:**
- Faction: Human Purity League (Lawful Neutral, High Xenophobia, Anti-Automation outlook)
- Motivation: Technological (abolish thinking machines, synthetic life)
- Trigger: AI uprising causes casualties, synthetic life gains rights
- Intensity: Extreme (Genocidal)
- Scope: Galactic

**Declaration:**
- Human Purity League declares jihad against all thinking machines
- Participation evaluation:
  - Anti-automation factions: +85 score → Primary participants
  - Xenophobic factions: +70 score → Secondary participants
  - Pro-AI factions: -60 score → Oppose (form counter-campaign)
  - Neutral factions: Low scores → Stay neutral or minimal participation

**Progression:**
- Mobilization: 20+ factions join (galaxy-wide movement)
- Active Conflict: Systematic destruction of AI facilities, synthetic life hunted
- Victories: Major AI hubs destroyed, synthetic populations eliminated
- Setbacks: Pro-AI counter-campaign emerges, AI fleets resist

**Resolution:**
- Partial Victory: Most thinking machines eliminated, but some survive in hidden bases
- Effects: Galaxy becomes anti-AI, synthetic life rare, technology restricted, counter-resistance forms

---

### Example 3: Xenocidal Campaign (Space4X)

**Setup:**
- Faction: Terran Supremacy (Lawful Evil, Extreme Xenophobia, High Pride)
- Motivation: Racial (eliminate all non-human species from galaxy)
- Trigger: Galactic population becomes majority non-human
- Intensity: Extreme (Genocidal)
- Scope: Galactic

**Declaration:**
- Terran Supremacy declares campaign to "purify" galaxy
- Participation evaluation:
  - Human-supremacist factions: +90 score → Primary participants
  - Xenophobic human factions: +75 score → Secondary participants
  - Non-human factions: -80 score → Target, form massive counter-coalition
  - Mixed-species factions: -50 score → Oppose

**Progression:**
- Mobilization: 30+ human factions join, 100+ non-human factions form counter-coalition
- Active Conflict: Galaxy-wide war, systematic purges of non-human populations
- Victories: Some non-human worlds conquered, populations eliminated
- Setbacks: Counter-coalition unites, massive resistance

**Resolution:**
- Defeat: Terran Supremacy coalition collapses under counter-coalition pressure
- Effects: Non-human factions gain unity, human-supremacist factions weakened, galactic balance shifts

---

### Example 4: Cultural Revolution (Godgame)

**Setup:**
- Faction: Traditionalist Order (Lawful Good, High Order, High Traditionalism)
- Motivation: Cultural (return to "old ways," abolish modern practices)
- Trigger: Modernization threatens traditional culture
- Intensity: Medium (Mixed)
- Scope: Continental

**Declaration:**
- Traditionalist Order declares crusade to restore tradition
- Participation evaluation:
  - Traditionalist factions: +80 score → Primary participants
  - Conservative factions: +60 score → Secondary participants
  - Progressive factions: -50 score → Oppose
  - Neutral factions: Low scores → Stay neutral

**Progression:**
- Mobilization: 8 factions join (mixed participation levels)
- Active Conflict: Cultural enforcement, limited military action, conversion efforts
- Victories: Some regions convert to traditional practices
- Setbacks: Progressive resistance, modernization continues in some areas

**Resolution:**
- Stalemate: Campaign achieves partial success, traditional and modern practices coexist
- Effects: Cultural diversity increases, some regions traditional, others modern, ongoing tension

---

## Technical Considerations

### Components Needed

```csharp
// Campaign declaration and state
public struct IdeologicalCampaign : IComponentData
{
    public FixedString128Bytes CampaignId;
    public Entity DeclaringFaction;
    public CampaignMotivation Motivation;      // Flags: Religious, Racial, Technological, Cultural, Political
    public CampaignIntensity Intensity;
    public CampaignScope Scope;
    public uint DeclarationTick;
    public CampaignPhase CurrentPhase;
    public float Progress;                     // 0.0 to 1.0 (victory progress)
    public bool IsUnderground;                 // True if campaign is underground/vilified
    public float Momentum;                     // 0-100 (campaign momentum)
    public float Legitimacy;                   // 0-100 (campaign legitimacy)
    public float PublicOpinion;                // -100 to +100 (public opinion)
    public float GrievancePool;                // Accumulated grievances
    public float ResourcePool;                 // Donations and resources
}

// Campaign participation
public struct CampaignParticipation : IComponentData
{
    public Entity CampaignEntity;
    public Entity FactionEntity;
    public ParticipationLevel Level;          // Primary, Secondary, Tertiary
    public float CommitmentStrength;          // 0.0 to 1.0 (resources committed)
    public uint JoinedTick;
}

// Campaign targets (what the campaign opposes)
public struct CampaignTarget : IBufferElementData
{
    public Entity TargetEntity;               // Faction, territory, population
    public TargetType Type;                   // Faction, Race, Technology, Culture
    public float ThreatLevel;                 // How much this target threatens campaign goals
}

// Counter-campaign (opposition)
public struct CounterCampaign : IComponentData
{
    public Entity OpposedCampaign;
    public Entity CounterCoalition;           // Aggregate of opposing factions
    public float OppositionStrength;
}

// Campaign support mechanisms
public struct CampaignSupport : IBufferElementData
{
    public Entity SupporterEntity;            // Entity supporting campaign
    public SupportType Type;                  // Donation, Recruitment, MartyrAct
    public float ContributionAmount;          // Donation amount or recruitment value
    public uint ContributionTick;             // When contribution occurred
}

// Radicalization agents (active pushers)
public struct RadicalizationAgent : IComponentData
{
    public Entity AgentEntity;
    public Entity CampaignEntity;
    public AgentType Type;                    // Leader, Propagandist, Recruiter, Agitator
    public float Effectiveness;               // 0-100 (how effective agent is)
    public uint LastActionTick;               // When agent last acted
}

// Public opinion tracking
public struct CampaignPublicOpinion : IComponentData
{
    public Entity CampaignEntity;
    public float OverallOpinion;              // -100 to +100 (aggregate)
    public int SupporterCount;                // Entities with positive opinion
    public int NeutralCount;                  // Entities with neutral opinion
    public int OpponentCount;                 // Entities with negative opinion
    public int CynicismCount;                 // Entities calling out manipulation
    public int OpportunistCount;              // Entities playing along
    public int TrueBelieverCount;             // Entities truly committed
}
```

### Systems Needed

1. **CampaignDeclarationSystem** - Evaluates trigger conditions, creates campaigns (underground or public)
2. **GrievanceAccumulationSystem** - Tracks recurring grievances, updates grievance pool
3. **CampaignMomentumSystem** - Calculates campaign momentum from grievances, support, recruitment
4. **CampaignSupportSystem** - Handles donations, recruitment, martyrdom acts
5. **RadicalizationAgentSystem** - Manages agents (leaders, propagandists, recruiters, agitators)
6. **PublicOpinionSystem** - Tracks and updates public opinion, handles reactions (cynicism, opportunism, belief, counter-campaigns)
7. **UndergroundTransitionSystem** - Handles transition from underground to public campaigns
8. **CampaignParticipationSystem** - Evaluates and manages faction participation
9. **CampaignProgressSystem** - Updates campaign progress, resolves victories/defeats
10. **CampaignResolutionSystem** - Applies campaign outcomes (territory, population, culture shifts)
11. **CounterCampaignSystem** - Forms and manages opposition coalitions

---

## Balance Considerations

### Campaign Frequency

**Risk:** Too many campaigns → constant world war, no stability
**Mitigation:**
- Campaigns require significant trigger conditions (not easily declared)
- Declaring faction must have sufficient power/authority
- Failed campaigns impose penalties (discourage frivolous declarations)
- Cooldown period after campaign resolution

### Campaign Success Rates

**Risk:** Campaigns too easy → world reshapes too quickly
**Mitigation:**
- Victory conditions require significant achievement (80%+ thresholds)
- Target resistance scales with campaign intensity
- Counter-campaigns emerge to balance powerful campaigns
- External factors (player intervention, natural disasters) can affect outcomes

### Participation Balance

**Risk:** Campaigns attract too many/few participants
**Mitigation:**
- Participation thresholds tuned (70+ for meaningful participation)
- Strategic interest factors (factions consider material gain)
- Opposition evaluation ensures counter-balance
- Neutral factions can tip balance if campaign too extreme

---

## Open Questions

1. **Player Role:** Can player god/empire declare campaigns? Participate? Oppose?
2. **Campaign Duration:** How long should campaigns last? Real-time vs game-time?
3. **Multiple Campaigns:** Can multiple campaigns run simultaneously? Conflict resolution?
4. **Campaign Evolution:** Can campaigns change goals/intensity mid-progression?
5. **Player Intervention:** How can players influence campaign outcomes? Direct intervention? Subtle manipulation?

---

## Related Documentation

- **Faction Relations System:** `Docs/Concepts/Politics/Faction_Relations_System.md` (diplomatic relations, alignment/outlook)
- **Aggregate Politics System:** `Docs/Concepts/Politics/Aggregate_Politics_System.md` (faction cohesion, decision-making)
- **Rebellion Mechanics:** `Docs/Concepts/Politics/Rebellion_Mechanics_System.md` (internal conflicts, uprisings)
- **Generalized Alignment Framework:** `Docs/Concepts/Meta/Generalized_Alignment_Framework.md` (alignment/outlook systems)

---

**For Implementers:** 
- Start with simple campaign declaration and participation evaluation
- Use existing faction relation/alignment systems as foundation
- Campaign resolution can be event-driven (victory/defeat events trigger world state updates)
- Integrate with territorial control systems for conquest campaigns

**For Designers:** 
- Focus on flexibility: campaigns should support many motivation combinations
- Balance campaign frequency (not too many, not too rare)
- Ensure campaigns feel impactful (world should change meaningfully)
- Consider cascading effects (campaigns spawn counter-campaigns, reshape relations)

---

**Last Updated:** 2025-12-21  
**Status:** Draft - Core concept defined, awaiting implementation decisions

