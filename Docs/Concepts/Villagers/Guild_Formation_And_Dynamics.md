# Guild Formation & Dynamics

**Status:** Design
**Category:** System - Aggregate Entities
**Scope:** Bottom-up guild formation, multi-membership, guild actions
**Created:** 2025-11-08
**Last Updated:** 2025-01-27

> **Wealth Integration:** Guild treasuries must use the wallet and transaction patterns defined in [`Wealth_And_Ledgers_Chunk1.md`](../Economy/Wealth_And_Ledgers_Chunk1.md). All guild wealth changes must go through transactions.

---

## Purpose

**Primary Goal:** Enable entities to self-organize into guilds based on shared interests, professions, or ideological goals, creating emergent political and economic power structures.

**Secondary Goals:**
- Support multi-guild membership with double agent dynamics
- Enable guild mergers, takeovers, and internal factions
- Create bottom-up political pressure (strikes, riots, coups)
- Allow villageless pseudo-guilds (outlaws, nomads, distributed networks)

**Key Principle:** Guilds are **formed by entities, not spawned by systems**. Formation is driven by individual ambition, outlook compatibility, and pragmatic self-interest.

---

## Guild Formation Process

### Initiation

**Any entity can attempt to form a guild** if they meet the education threshold and can afford the charter fee.

```
Formation Requirements:

Charter Fee: 10 gold (upfront, non-refundable if failed)
Education Check: % chance based on education level
Time Limit: 1 in-game week to gather 10 signatures
Registration: Must register at town hall/village center (or self-register if villageless)

Education-Based Success Probability:

Base Formula:
  SuccessChance = 50% + ((Education - 30) × 1.5%)

Examples:
  Education 30: 50% chance to pass
  Education 40: 65% chance to pass  (50 + (10 × 1.5) = 65%)
  Education 50: 80% chance to pass
  Education 60: 95% chance to pass
  Education 70+: 95% chance (capped)

If Education Check Fails:
  - Charter fee (10 gold) is lost
  - Entity cannot form guild this attempt
  - No cooldown - can immediately try again (if they have another 10 gold)

If Education Check Passes:
  - Charter is officially registered
  - Entity becomes "guild master candidate"
  - 1 week countdown begins to gather 10 signatures
```

**Wealth Advantage:**
Wealthy entities can spam charter attempts until one succeeds. Poor entities risk their limited savings on a single attempt.

```
Example: Wealthy Merchant vs Poor Blacksmith

Wealthy Merchant:
  Wealth: 500 gold
  Education: 40 (65% success chance)
  Attempts: Can afford 50 charter attempts
  Expected Success: 1 - (0.35^50) ≈ 100% (statistically guaranteed within 50 tries)

Poor Blacksmith:
  Wealth: 15 gold
  Education: 35 (57.5% success chance)
  Attempts: Can afford 1 charter attempt
  Expected Success: 57.5% (single roll)

Result: Wealth inequality affects guild formation rates
```

### Signature Gathering (1 Week Period)

**Guild master candidate must gather 10 signatures from other entities.**

**Dual Approach:**
1. **Active Recruitment:** Guild master personally approaches potential members (entities see notification/prompt)
2. **Passive Advertisement:** Charter posted in town center (entities can browse and sign)

**Signature Mechanics:**
- Entities can sign **multiple charters simultaneously** (no exclusivity during formation)
- Guild masters can also sign other charters (no conflict of interest during formation)
- No concept of "opposing guilds" at formation - all guilds start neutral
- Opposition emerges from behavioral incompatibilities over time

```
Signature Probability Formula:

BaseSignChance = 20% (strangers, no relation)

Modifiers:
  Relations Bonus:
    Friend (+50 relation): +30% sign chance
    Acquaintance (+20 relation): +10% sign chance
    Neutral (0 relation): 0% modifier
    Dislike (-20 relation): -15% sign chance
    Enemy (-50 relation): -40% sign chance (nearly impossible)

  Alignment Compatibility:
    Perfect Match (same alignment tri-axis): +15%
    Close Match (1-2 axes differ by <30 points): +5%
    Neutral: 0%
    Opposed (all axes differ by 50+ points): -10%

  Outlook Compatibility:
    Shared Primary Outlook: +20%
    Shared Secondary Outlook: +10%
    No shared outlooks: 0%
    Conflicting outlooks (Warlike vs Peaceful): -5%

  Self-Interest:
    Guild purpose benefits entity's profession: +25%
    Guild purpose aligns with entity's goals: +15%
    No benefit: 0%
    Guild purpose harms entity's interests: -30%

  Fame/Reputation (Guild Master):
    High Fame (80+ Glory): +20% (famous leader attracts followers)
    Moderate Fame (40-80 Glory): +10%
    Nobody (<40 Glory): 0% baseline
    Infamous (negative reputation): -15%

  Nobody Recruitment (Special Case):
    If both guild master AND potential signer are "nobodies" (Glory <40):
      +15% bonus (shared underdog status)
    Result: Nobodies form "nobody guilds" together

FinalSignChance = BaseSignChance + Σ(Modifiers)
  Clamped to 5% minimum, 95% maximum
```

**Example Signature Scenarios:**

```
Scenario 1: Famous Blacksmith Forms Smithing Guild

Guild Master:
  Glory: 85 (famous master craftsman)
  Education: 55 (high skill)
  Profession: Blacksmith
  Alignment: Lawful Neutral
  Outlook: Materialistic

Target Signer: Journeyman Blacksmith
  Relation to Guild Master: +30 (former apprentice)
  Profession: Blacksmith
  Alignment: Lawful Good (1 axis differs by 30 points)
  Outlook: Materialistic (shared)
  Self-Interest: Guild benefits smithing profession (+25%)

SignChance:
  Base: 20%
  + Relations (+30): +20%
  + Alignment close match: +5%
  + Shared outlook: +20%
  + Self-interest (smithing guild): +25%
  + High fame: +20%
  = 110% (capped at 95%)

Result: 95% chance to sign (nearly guaranteed)

---

Scenario 2: Nobody Farmer Forms Peasant Guild

Guild Master:
  Glory: 15 (nobody)
  Education: 30 (baseline)
  Profession: Farmer
  Alignment: Neutral Good
  Outlook: Egalitarian

Target Signer: Poor Laborer
  Relation to Guild Master: 0 (strangers)
  Profession: Laborer
  Alignment: Chaotic Good (1 axis differs by 40 points)
  Outlook: Egalitarian (shared)
  Glory: 10 (also nobody)
  Self-Interest: Guild promises collective bargaining for wages (+15%)

SignChance:
  Base: 20%
  + Relations (neutral): 0%
  + Alignment (slight mismatch): 0%
  + Shared outlook: +20%
  + Self-interest: +15%
  + Nobody recruiting nobody: +15%
  = 70%

Result: 70% chance to sign (likely, but not guaranteed)

Outcome: If guild master recruits 10 such nobodies, it becomes a "Peasants' Guild" - low prestige, but unified lower class.

---

Scenario 3: Chaotic Evil Assassin Forms Assassins' Guild

Guild Master:
  Glory: 50 (infamous killer)
  Education: 45 (competent)
  Profession: Assassin
  Alignment: Chaotic Evil
  Outlook: Warlike

Target Signer: Neutral Rogue
  Relation to Guild Master: -10 (distrust)
  Profession: Thief
  Alignment: Neutral (differs significantly)
  Outlook: Materialistic (not Warlike)
  Self-Interest: Guild offers contracts, but risky association (-5%)

SignChance:
  Base: 20%
  + Relations (-10): -5%
  + Alignment (opposed): -10%
  + Outlook mismatch: 0%
  + Self-interest (risky): -5%
  + Infamous reputation: -15%
  = -15% (capped at 5%)

Result: 5% chance to sign (extremely unlikely)

Outcome: Chaotic Evil guild master struggles to recruit moderates. Must find other chaotic evil entities or coerce/threaten signatures.
```

### Formation Success/Failure

```
If 10 Signatures Gathered Within 1 Week:
  - Guild officially forms
  - Guild master becomes permanent leader (unless democratic guild votes later)
  - 10 signers become founding members
  - Guild registered in village records (or self-registered if villageless)
  - Charter fee (10 gold) consumed (pays for paperwork, legal recognition)

If <10 Signatures After 1 Week:
  - Charter expires
  - Charter fee (10 gold) lost
  - Guild master can try again (pay another 10 gold, restart process)
  - Signed entities are released (signatures void)

Edge Case: Guild Master Signs Another Charter:
  - Guild master can be member of their own guild AND another guild simultaneously
  - No conflict - multi-membership is allowed
  - Guild master loyalty split between guilds (potential internal conflict later)
```

---

## Multi-Guild Membership & Double Agents

### Membership Rules

**Entities can belong to multiple guilds simultaneously**, even if those guilds have conflicting interests.

```
Multi-Membership Examples:

Valid Multi-Membership:
  - Blacksmith belongs to Smithing Guild AND Merchants' Guild (economic synergy)
  - Mage belongs to Mages' Guild AND Scholars' Guild (knowledge sharing)
  - Warrior belongs to Heroes' Guild AND Village Militia (compatible roles)

Conflicting Multi-Membership (Double Agent Risk):
  - Entity belongs to Guild A AND Guild B, where Guild A and Guild B are rivals/enemies
  - Entity belongs to Lawful Good guild AND Chaotic Evil guild (ideological conflict)
  - Entity belongs to Merchant Guild AND Thieves' Guild (economic vs criminal)

Forbidden Multi-Membership:
  - Certain aggregates explicitly ban members of specific other aggregates
  - Example: Holy Order bans members of Demon Cult (religious opposition)
  - Example: Village Council bans members of Rebel Faction (political opposition)
  - Attempting to join triggers rejection OR forces entity to leave previous aggregate
```

### Double Agent Detection

**Detection uses existing stealth and perception systems** (see Stealth_And_Perception_System.md).

```
Detection Trigger Events:

Suspicious Behavior:
  - Entity leaks guild secrets to rival guild
  - Entity votes against guild interests in critical decisions
  - Entity seen meeting with rival guild leaders
  - Entity's loyalty score drops below threshold (indicating split allegiance)

Detection Roll:
  Spy Master Perception + Intelligence Network vs Entity Stealth + Deception

  If Detection Succeeds:
    - Entity revealed as double agent
    - Guild leadership learns of conflicting membership
    - Exile vote or decree initiated

  If Detection Fails:
    - Entity remains hidden
    - Suspicion may increase (future detection checks easier)

Exile Process:

Democratic Guild:
  - Members vote on exile (majority required)
  - If vote passes: Entity expelled, loses guild benefits
  - If vote fails: Entity remains (members trust them or don't care)

Authoritarian Guild:
  - Guild master decrees exile (no vote)
  - Immediate expulsion, no appeal

Consequences of Exile:
  - Entity loses guild membership
  - Negative reputation penalty (-20 Reputation)
  - May be blacklisted from rejoining
  - Rival guild may also exile them (if they learn of betrayal)
```

**Example Double Agent Scenario:**

```
Entity: Thief named Vex
  Member of: Thieves' Guild (primary loyalty)
  Also member of: Merchants' Guild (infiltrator)
  Goal: Steal merchant trade secrets, sabotage competitors

Month 1-6: Vex feeds Thieves' Guild intel on merchant caravans
  Merchants' Guild suffers 15% increased robbery rate
  No detection (Vex's stealth 75, Merchants' Spy Master perception 50)

Month 7: Merchants' Guild hires better Spy Master (perception 80)
  Detection Roll: 80 + (Intelligence Network 60) = 140 vs Vex's Stealth 75 + Deception 65 = 140
  Roll: d100 = 62 (success)
  Result: Vex caught passing information to Thieves' Guild

Month 7 (Exile Vote):
  Merchants' Guild (democratic) votes on exile
  Vote: 25 Yes (exile), 10 No (give second chance), 5 Abstain
  Result: Vex exiled from Merchants' Guild
  Vex remains in Thieves' Guild (they don't care)

Month 8: Vex's reputation drops (-20), blacklisted from merchant guilds
  Thieves' Guild rewards Vex for successful espionage (+10 Glory)
```

---

## Guild Types & Purposes

### Guild Type Determination

**Guild purpose is defined by the guild master's intent and founding member composition.**

```
Guild Purpose Definition:

Explicit Purpose (Guild Master Declares):
  - Guild master names guild: "Boltmakers' Guild", "Heroes' Guild", "Assassins' Guild"
  - Purpose stated in charter: "Regulate bolt production and pricing"
  - Members sign knowing the purpose

Emergent Purpose (From Member Composition):
  - If 8 of 10 founding members are blacksmiths → Smithing Guild (by default)
  - If 10 of 10 founding members are combatants → Warriors' Guild
  - Mixed membership → General purpose guild (e.g., "Artisans' Guild" for all crafters)

Purpose Shift Over Time:
  - If membership composition changes, guild purpose may drift
  - Example: Smithing Guild recruits 20 carpenters, becomes "Crafters' Guild"
  - Democratic guilds vote on purpose change (60% approval)
  - Authoritarian guilds: Guild master decrees purpose shift
```

### Multiple Guilds of Same Type

**Multiple guilds of the same type can coexist** in a single village or across the world.

```
Coexistence Examples:

Two Blacksmith Guilds in Same Village:
  - Guild A: "Master Smiths' Guild" (high-quality, expensive, Lawful)
  - Guild B: "Workers' Smithing Collective" (cheap, mass-production, Egalitarian)
  - Compete for contracts, customers, apprentices
  - Guild Relations: Rivalry (-30 trust)

Multiple Heroes' Guilds Across World:
  - "Lightbringers" (Lawful Good, based in Village A)
  - "Stormbreakers" (Chaotic Good, based in Village B)
  - "Shadowfang Mercenaries" (Neutral Evil, roaming)
  - Compete for world boss contracts, loot, glory
  - May ally temporarily or betray mid-mission

Guild Relations Tracking:

Each guild has relations with every other guild:
  Guild A → Guild B: Trust score (-100 to +100)

  Neutral (0): No history
  Friendly (20-60): Occasional cooperation
  Allied (60-100): Formal alliance, share resources
  Rivalry (-20 to -60): Economic/ideological competition
  Hostile (-60 to -100): Active sabotage, guild warfare
  At War (-100): Open conflict, assassinations
```

---

## Guild Leadership & Governance

### Leadership Types

**Guilds adopt governance structures based on founding members' outlooks and alignments.**

```
Governance Types:

Democratic:
  Formation: If majority of founding members are Egalitarian or Lawful Good
  Leadership: Guild master elected by member vote
  Vote Weight: Equal (1 member = 1 vote)
  Term Length: 2 in-game years (or until vote of no-confidence)
  Decisions: Major policy requires 50%+1 vote

Authoritarian:
  Formation: If guild master is Authoritarian or Lawful Evil
  Leadership: Guild master appoints successor (or seizes power)
  Vote Weight: Only officers vote, guild master has veto
  Term Length: Until death, resignation, or coup
  Decisions: Guild master decrees unilaterally

Meritocratic:
  Formation: If founding members are Scholarly or Materialistic
  Leadership: Highest-skilled member automatically becomes master
  Vote Weight: Weighted by skill level (Master Craftsman = 3 votes, Apprentice = 1 vote)
  Term Length: Until someone surpasses their skill
  Decisions: Officers vote, weighted by expertise

Oligarchic:
  Formation: If founding members are Noble or Elite class
  Leadership: Officers vote among themselves
  Vote Weight: Only officers vote (members excluded)
  Term Length: Indefinite (officers rarely replaced)
  Decisions: Officer majority vote
```

### Voting Mechanics

```
Democratic Guild Voting:

Vote Trigger Events:
  - Guild master election (every 2 years or on-demand)
  - Policy change (charter amendment, war declaration, merger)
  - Exile vote (remove disloyal member)
  - Strike/protest decision (collective action)

Vote Process:
  1. Proposal introduced (by guild master, officer, or petition of 10%+ members)
  2. Debate period (1-3 in-game days)
  3. Each member casts vote (Yes/No/Abstain)
  4. Tally: If Yes > 50% → Pass, else Fail
  5. Execute decision or dismiss proposal

Vote of No-Confidence:
  - Any member can petition for no-confidence vote (requires 20% member signatures)
  - If vote passes (50%+1): Guild master removed, emergency election
  - If vote fails: Petitioner loses reputation (-10), guild master retains power

Example Vote:

Proposal: "Declare war on rival Smithing Guild"
Members: 40 total
Vote:
  - 25 Yes (war hawks, see economic advantage)
  - 10 No (pacifists, fear casualties)
  - 5 Abstain (uncertain)
Result: 25/40 = 62.5% Yes → Proposal passes
Guild declares war on rival guild

---

Authoritarian Guild Voting:

Guild Master Veto Power:
  - Officers may vote on decisions
  - Guild master can veto any vote (overrides officer majority)
  - Only guild master's decree is final

Vote Weight Distribution:
  - Guild Master: Veto power (infinite weight)
  - Vice/Second-in-Command: 3 votes
  - Officers: 2 votes each
  - Members: 0 votes (no say in authoritarian guilds)

Merger Vote (Special Case):
  - Both guilds must vote to merge
  - Authoritarian guild masters can veto merger (even if officers vote yes)
  - Example: Guild A (authoritarian) + Guild B (democratic)
    - Guild B votes: 80% Yes (want merger)
    - Guild A guild master: Vetoes (refuses merger)
    - Result: Merger blocked

Example Veto:

Proposal: "Share treasury equally with all members"
Officers Vote:
  - 5 Yes (egalitarian officers)
  - 2 No (guild master loyalists)
Result: 5/7 = 71% Yes (would pass in democratic guild)
Guild Master: Vetoes (authoritarian power)
Final Result: Proposal rejected, guild master retains treasury control
```

### Succession (Authoritarian Guilds)

```
Named Successor:
  - Guild master explicitly names successor (usually vice/second-in-command)
  - Documented in guild charter (legally binding)
  - On guild master's death: Successor automatically inherits position

Succession Disputes (If No Named Successor):
  - Multiple officers compete for leadership
  - Resolution depends on guild outlook:

  Warlike Guild Succession:
    - Officers duel to the death (or first blood)
    - Winner becomes guild master
    - Example: Two officers fight, Victor wins → Victor is new guild master

  Peaceful Guild Succession:
    - Officers compete in non-lethal trial (debate, craft contest, intelligence test)
    - Winner determined by judges (guild members or external arbiters)
    - Example: Two officers debate guild policy, members vote on best debater

  Materialistic Guild Succession:
    - Officers bid for position (highest gold bid wins)
    - Wealth = power in materialistic guilds
    - Example: Officer A bids 500 gold, Officer B bids 800 gold → Officer B wins

  Scholarly Guild Succession:
    - Officers demonstrate knowledge (research presentation, thesis defense)
    - Most knowledgeable becomes guild master
    - Example: Officers present research, peer review determines winner

Coup Attempts:
  - Ambitious officer may attempt coup (overthrow guild master by force)
  - Success depends on:
    - Officer's combat power vs guild master's guards
    - Member loyalty (do members support coup or defend master?)
    - Village alignment (does village tolerate violent power struggles?)
  - If successful: Officer becomes new guild master (authoritarian transition)
  - If failed: Officer executed or exiled, guild master retains power
```

---

## Villageless Pseudo-Guilds

**Roaming, nomadic, or outlaw entities can form guilds without village affiliation.**

```
Pseudo-Guild Formation:

No Town Hall Required:
  - Villageless entities form guild among themselves
  - Self-registering (no official authority recognizes them initially)
  - Other entities learn of existence through actions, rumors, encounters

Formation Process:
  1. Guild master candidate (villageless entity) initiates charter
  2. Gathers 10 signatures from other villageless entities
  3. Guild auto-registers (no village approval needed)
  4. Guild operates as distributed network

Examples:

Guild of Outlaws:
  - 50 bandits across 5 regions
  - No central headquarters (roaming camps)
  - Coordinate via messengers, secret signals
  - Purpose: Raiding, pillaging, evading authorities

Nomadic Merchants' Guild:
  - 30 traveling merchants
  - No home village (constant travel)
  - Meet at crossroads, trade fairs
  - Purpose: Trade route control, caravan protection

Distributed Assassins' Network:
  - 20 assassins in 10 different cities
  - No headquarters (blend into populations)
  - Communicate via dead drops, coded messages
  - Purpose: Contract killings, espionage

Recognition Mechanics:

Other Entities Learn of Guild:
  - Through encounters (attacked by outlaws → learn of Guild of Outlaws)
  - Through rumors (merchant spreads word of roaming guild)
  - Through actions (guild performs high-profile heist → fame spreads)

Village Recognition:
  - Villages may eventually recognize pseudo-guild (add to records)
  - Allows diplomatic relations (village negotiates with outlaw guild for truce)
  - Or declare them enemies (wanted posters, bounties)
```

### Settling Down

**Pseudo-guild members can settle in villages** if compatible.

```
Settlement Process:

Entity Decides to Settle:
  - Villageless entity approaches village
  - Village evaluates:
    - Entity's history (criminal record, reputation)
    - Entity's alignment vs village alignment
    - Village's relations with entity's guild

Settlement Approval:

Compatible Alignment:
  - Lawful Good village + Lawful Good entity → 80% approval
  - Village welcomes new citizen
  - Entity remains in pseudo-guild (but now has village residence)

Incompatible Alignment:
  - Lawful Good village + Chaotic Evil outlaw → 5% approval
  - Village rejects settlement (turn away or arrest)
  - Entity must find compatible village or remain villageless

Negative History:
  - Entity has bounty in village → Arrested on arrival
  - Entity previously raided village → Banned from settlement
  - Entity's guild is enemy of village → Denied entry

Example:

Outlaw Bandits Guild Member Settles:
  Entity: Rogue named Kane
  Guild: Outlaw Bandits (Chaotic Neutral)
  History: Raided 3 villages, no killings (theft only)
  Alignment: Chaotic Neutral

  Village A (Lawful Good):
    Alignment mismatch: -30%
    Criminal record: -40%
    Result: 5% approval → Rejected (arrest warrant issued)

  Village B (Chaotic Neutral):
    Alignment match: +30%
    Criminal record: 0% (village doesn't care)
    Result: 60% approval → Approved
    Kane settles in Village B, remains in Outlaw Bandits Guild
```

---

## Guild Mergers & Takeovers

### Peaceful Merger

**Two guilds can voluntarily merge** if both agree.

```
Merger Process:

Step 1: Negotiation
  - Guild masters of both guilds meet
  - Discuss terms: leadership, resources, membership

Step 2: Voting

  Democratic Guilds:
    - Both guilds vote on merger (majority required: 50%+1)
    - If both vote Yes → Merger proceeds
    - If either votes No → Merger rejected

  Authoritarian Guilds:
    - Guild masters negotiate directly
    - Either guild master can veto (even if officers vote yes)
    - Requires both guild masters to agree

  Mixed (Democratic + Authoritarian):
    - Democratic guild votes (majority)
    - Authoritarian guild master decrees (no vote)
    - Both must approve for merger

Step 3: Merger Execution

  New Guild Formation:
    - Old Guild A + Old Guild B → New Guild C
    - New guild name chosen (negotiated or voted)
    - Combined membership (all members transfer)
    - Combined resources (treasuries merged, shared wealth)

  Leadership Options:

  Option 1: Duo Rulership
    - Both guild masters remain co-leaders
    - Requires cooperation, potential for power struggles
    - Example: Guild A master (Lawful) + Guild B master (Chaotic) = unstable duo

  Option 2: Single Leader (Democratic Vote)
    - New guild holds election
    - Both old guild masters run for position
    - Members vote, winner becomes sole guild master

  Option 3: Single Leader (Negotiated)
    - Guild masters negotiate who steps down
    - Weaker guild master becomes officer or retires
    - Example: Guild A (100 members) + Guild B (20 members) → Guild A master leads

  Option 4: Shared Custodianship (Authoritarian)
    - Guild masters rule as council
    - Both have veto power, must agree on decisions
    - Oligarchic structure (multiple authoritarian leaders)

Example Merger:

Guild A: "Master Smiths' Guild" (50 members, Lawful Neutral, Democratic)
Guild B: "Workers' Smithing Collective" (30 members, Egalitarian, Democratic)

Month 1: Guild masters negotiate merger (both see benefit in unified bargaining power)
Month 2: Both guilds vote
  - Guild A: 35 Yes, 15 No (70% approval)
  - Guild B: 25 Yes, 5 No (83% approval)
Month 3: Merger proceeds
  - New Guild: "United Smithing Guild" (80 members)
  - Leadership vote: Guild A master wins (52% vs 48%)
  - Guild B master becomes Vice (second-in-command)
  - Merged treasury: 5,000 gold (Guild A) + 2,000 gold (Guild B) = 7,000 gold
```

### Hostile Takeover

**One guild can forcibly absorb or destroy another guild.**

```
Takeover Types:

Economic Takeover:
  - Richer guild buys out poorer guild
  - Offers members higher wages, better benefits
  - Poorer guild loses members, eventually dissolves
  - Example:
    - Rich Merchants' Guild (10,000 gold treasury) vs Poor Traders' Guild (500 gold)
    - Merchants' Guild offers 50% wage increase to all Traders' Guild members
    - 20 of 30 members defect to Merchants' Guild
    - Traders' Guild collapses (below minimum 10 members)

Political Takeover:
  - Larger guild uses political influence to delegitimize rival
  - Lobbies village council to ban rival guild
  - Or infiltrates rival guild leadership (coup from within)
  - Example:
    - Guild A has 200 members (40% of village population)
    - Guild B has 30 members (6% of village population)
    - Guild A lobbies council: "Guild B is redundant, we represent all workers"
    - Council votes to revoke Guild B's charter
    - Guild B dissolved by law

Violent Takeover:
  - Guild warfare (see Guild Actions: Riots & Coups below)
  - Assassinate rival guild master, officers
  - Intimidate/threaten rival members into joining
  - Example:
    - Chaotic Evil Assassins' Guild vs Neutral Rogues' Guild
    - Assassins kill Rogues' guild master
    - Assassins threaten remaining rogues: "Join us or die"
    - 15 rogues join Assassins (forced), 10 flee village, 5 killed

Resistance:

Guilds can resist takeovers:
  - Economic: Counter-offer (raise wages, improve benefits)
  - Political: Lobby village council for protection
  - Violent: Hire bodyguards, fortify guild hall, retaliate with own violence

Takeover Success Factors:
  - Wealth disparity (economic takeover)
  - Member count disparity (political takeover)
  - Combat power disparity (violent takeover)
  - Village support (does village tolerate hostile takeover?)

Example Resistance:

Poor Traders' Guild (30 members) vs Rich Merchants' Guild (100 members)

Merchants' Guild Action: Economic takeover attempt (offer higher wages)

Traders' Guild Resistance:
  - Cannot match wages (too poor)
  - Instead: Emphasize solidarity, shared values
  - Appeal to members' loyalty: "We built this together, don't abandon us for gold"
  - Emphasize Egalitarian outlook (vs Merchants' Materialistic outlook)

Result:
  - 10 materialistic members defect (prefer higher wages)
  - 20 egalitarian members stay (loyalty > gold)
  - Traders' Guild survives but weakened (30 → 20 members)
```

---

## Guild Actions & Political Pressure

### Strikes & Demonstrations

**Guilds can organize strikes** to pressure villages or rival guilds.

```
Strike Triggers:

Economic Grievances:
  - Low wages (below guild's demanded minimum)
  - Poor working conditions (unsafe, exploitative)
  - Unfair contracts (village takes too much tax)

Political Grievances:
  - Guild banned from village council representation
  - Village laws harm guild interests
  - Rival guild gets preferential treatment

Ideological Grievances:
  - Village alignment conflicts with guild alignment
  - Village supports enemy faction (e.g., village aids rival guild)

Strike Decision Process:

Democratic Guild:
  - Members vote on strike (50%+1 approval required)
  - If vote passes: Guild officially strikes

Authoritarian Guild:
  - Guild master decrees strike (no vote)
  - Members obey or risk punishment

Chaotic Guild:
  - Strike erupts organically (no formal decision)
  - Tensions rise → Spontaneous walkout
  - Guild leadership may formalize it post-hoc or lose control

Strike Effects:

Production Halt:
  - Striking guild members stop working
  - If guild controls critical industry → Village production chain breaks

  Example:
    - Smithing Guild strikes (50 blacksmiths)
    - Village weapon production drops to 0
    - Village militia cannot rearm
    - Village vulnerable to attack

Economic Damage:
  - Village loses tax revenue from guild
  - Businesses dependent on guild suffer

  Example:
    - Farmers' Guild strikes (100 farmers)
    - No grain production → Bakeries close → Food shortage
    - Village faces famine within 2 months

Political Pressure:
  - Village council forced to negotiate
  - Public opinion shifts (villagers support or oppose strikers)

Strike Resolution:

Negotiation (Peaceful + Lawful):
  - Village council meets with guild representatives
  - Compromise: Village raises wages 10%, guild ends strike
  - Example:
    - Miners' Guild demands 30% wage increase
    - Village offers 15%
    - Guild accepts (democratic vote: 60% approval)
    - Strike ends

Forceful Suppression (Authoritarian + Warlike):
  - Village sends peacekeepers to break strike
  - Arrests guild leaders
  - Forces workers back to jobs under threat
  - Example:
    - Authoritarian village (Lawful Evil)
    - Smithing Guild strikes
    - Village arrests 10 guild leaders, executes 3 "ringleaders"
    - Remaining smiths return to work (fear > solidarity)

Stalemate (Chaotic):
  - Neither side backs down
  - Strike drags on for months
  - Village economy collapses OR guild dissolves (members starve)
  - Example:
    - Strike lasts 6 months
    - Village imports goods from neighboring village (bypasses strike)
    - Guild loses leverage, members defect
    - Guild dissolves (failure)
```

### Riots & Violence

**Guilds can escalate to riots** if strikes fail or tensions are extreme.

```
Riot Triggers:

Failed Negotiations:
  - Strike ignored by village council
  - Guild demands rejected repeatedly
  - Frustration boils over → Violence

Ideological Conflict:
  - Guild vs Guild (rival guilds clash in streets)
  - Guild vs Village Elites (class warfare)
  - Guild vs Everyone (chaotic rampage)

External Shock:
  - Economic collapse (mass unemployment)
  - War/Siege (desperation, shortages)
  - Coup attempt (political instability)

Riot Types:

Guild vs Guild Riot:
  - Two rival guilds fight in streets
  - Example:
    - Lawful Good Artisans' Guild vs Chaotic Evil Thieves' Guild
    - Artisans accuse thieves of stealing tools
    - Street brawl erupts (50 vs 30)
  - Casualties: 10 artisans, 15 thieves
  - Village peacekeepers intervene (arrests on both sides)

Guild vs Village Elites Riot:
  - Lower-class guild attacks wealthy/noble district
  - Example:
    - Peasants' Guild (200 poor farmers) vs Noble Houses (20 elites)
    - Peasants storm noble estates, loot wealth
  - Village response: Militia deployed (lethal force)
  - Casualties: 50 peasants killed, 5 nobles killed
  - Riot suppressed, guild leaders executed

Guild vs Everyone Riot (Chaotic Rampage):
  - Chaotic guild loses control, indiscriminate violence
  - Example:
    - Outlaw Guild (50 bandits) occupies village during siege
    - Bandits loot, burn, kill without discrimination
  - Village response: Total war (every able-bodied villager fights)
  - Casualties: 30 bandits, 40 villagers
  - Bandits driven out or killed

Riot Outcomes:

Success (Guild Wins):
  - Village capitulates to guild demands
  - Example:
    - Workers' Guild riots, burns noble estates
    - Village council votes to redistribute wealth
    - Guild achieves political victory (but at cost of lives)

Failure (Guild Crushed):
  - Village suppresses riot with overwhelming force
  - Guild leaders executed or exiled
  - Guild dissolved, banned from reforming
  - Example:
    - Rebel Guild riots, attempts coup
    - Village militia crushes rebellion (100 casualties)
    - Guild master executed publicly (deterrent)
    - Surviving members flee or imprisoned

Stalemate (Ongoing Conflict):
  - Neither side wins decisively
  - Village enters civil war state
  - Riots continue sporadically for months/years
  - Example:
    - Egalitarian Guild vs Authoritarian Village
    - 2-year low-intensity conflict
    - Village loses 20% population (emigration + casualties)
    - Guild never achieves demands, but never fully suppressed
```

### Coups & Political Overthrow

**Guilds can attempt to seize control of village government.**

```
Coup Triggers:

Extreme Grievances:
  - Village government is tyrannical, exploitative
  - Guild has no other recourse (strikes/riots failed)

Ideological Mission:
  - Revolutionary guild (Rebel Faction) seeks to install new regime
  - Religious guild (Holy Order) seeks theocracy

Opportunism:
  - Village weakened by war, famine, plague
  - Guild sees chance to seize power

Coup Mechanics:

Step 1: Planning
  - Guild leadership plans coup (secret meetings)
  - Identify targets (village council, ruler, elites)
  - Recruit allies (other guilds, factions, sympathizers)

Step 2: Execution
  - Coup attempt (arrests, assassinations, declarations)
  - Example:
    - Rebel Guild seizes town hall at night
    - Arrests village councilors
    - Declares new government

Step 3: Village Response

  Support Coup (Village Population Backs Guild):
    - If guild has 50%+ popular support → Coup succeeds
    - Population does not resist, may actively help
    - Example:
      - Peasants' Guild overthrows tyrannical noble council
      - 80% of village population supports peasants
      - No resistance, nobles flee
      - Peasants install democratic council

  Oppose Coup (Village Loyal to Government):
    - If guild has <30% support → Coup fails
    - Population resists, forms militias to defend government
    - Example:
      - Chaotic Evil Assassins' Guild attempts coup
      - 90% of village opposes (fear of tyranny)
      - Village militia + peacekeepers crush coup
      - Guild leaders executed

  Divided (Civil War):
    - If guild has 30-50% support → Civil war
    - Village splits into factions
    - Prolonged conflict, high casualties
    - Example:
      - Egalitarian Guild vs Authoritarian Elite
      - 45% support guild, 40% support elite, 15% neutral
      - 1-year civil war
      - Eventual negotiated settlement (power-sharing government)

Coup Success Effects:

Alignment Rewrite:
  - Village alignment shifts to match guild alignment
  - Example:
    - Village was Lawful Neutral
    - Chaotic Good Rebel Guild wins coup
    - Village becomes Chaotic Good

  Laws Rewritten:
    - New government enacts new laws (reflect guild ideology)
    - Old elites purged or exiled

  Outlook Shift:
    - Village outlook shifts to match guild outlook
    - Example:
      - Village was Materialistic
      - Egalitarian Guild wins coup
      - Village becomes Egalitarian (redistribute wealth)

  International Consequences:
    - Neighboring villages react (support, condemn, invade)
    - Trade disrupted (other villages embargo coup government)
    - Refugees flee (old regime supporters emigrate)
```

---

## Resource Sharing & Wealth

### Aggregate Wealth

**Guild wealth is an abstract average**, not literal currency pool.

```
Aggregate Wealth Calculation:

GuildAverageWealth = Σ(MemberWealth) / MemberCount

Example:
  Guild has 20 members:
    - 10 members with 50 gold each = 500 gold
    - 5 members with 200 gold each = 1,000 gold
    - 5 members with 500 gold each = 2,500 gold
  Total: 4,000 gold
  Average: 4,000 / 20 = 200 gold per member

Purpose:
  - Indicates guild's economic power (abstract metric)
  - Does NOT mean guild has 4,000 gold in a shared treasury
  - Used for guild reputation, political weight, economic influence

Actual Treasury (Separate):
  - Some guilds maintain a shared treasury (pooled funds)
  - Members contribute % of income (if guild charter requires)
  - Used for guild expenses (hall maintenance, bribes, war funds)
```

### Resource Sharing by Alignment

**Resource sharing depends on guild alignment and outlook.**

```
Sharing Probability:

Lawful Guilds:
  - High sharing (80% default)
  - Members contribute to shared treasury
  - Resources distributed equitably

Chaotic Guilds:
  - Low sharing (20% default)
  - Members hoard personal wealth
  - Guild treasury minimal

But Outlook Overrides:

Lawful + Materialistic:
  - Sharing drops to 40% (greed overrides order)

Chaotic + Charitable:
  - Sharing rises to 60% (generosity overrides chaos)

Lawful + Egalitarian:
  - Sharing rises to 95% (perfect alignment for sharing)

Chaotic + Corrupt:
  - Sharing drops to 5% (everyone steals, hoards)

Example:

Guild A: Lawful Good Egalitarian
  - Sharing: 95%
  - Members contribute 20% of income to shared treasury
  - Guild master distributes funds equitably
  - Used for: Guild hall upkeep, charity, member aid

Guild B: Chaotic Evil Corrupt
  - Sharing: 5%
  - Members hoard all wealth
  - Guild master embezzles any shared funds
  - Result: No functioning treasury, internal theft common
```

### Internal Factions

**Conflicting outlooks within a guild create internal factions.**

```
Faction Formation:

If Guild Has Mixed Outlooks:
  - Example: Guild has 60% Lawful members, 40% Chaotic members
  - Lawful Faction forms (wants order, sharing, rules)
  - Chaotic Faction forms (wants freedom, hoarding, autonomy)

Factional Conflict:

Lawful Faction Demands:
  - Mandatory treasury contributions (10% income)
  - Strict guild rules (code of conduct)
  - Democratic votes on all decisions

Chaotic Faction Demands:
  - Voluntary contributions (no mandate)
  - Minimal rules (personal freedom)
  - Guild master decrees (faster decisions, no voting delays)

Resolution:

Option 1: Compromise
  - Guild votes on middle ground
  - Example: 5% mandatory contribution (Lawful concedes), minimal rules (Chaotic concedes)

Option 2: Factional Split
  - Guild splits into two guilds
  - Lawful Faction forms "Lawful Smithing Guild"
  - Chaotic Faction forms "Free Smiths Collective"
  - Both operate independently

Option 3: Internal Coup
  - Stronger faction overthrows weaker faction's leadership
  - Example: Lawful Faction (60% members) votes out Chaotic guild master
  - Elects Lawful guild master
  - Chaotic members leave or submit

Example:

Guild: "United Crafters' Guild" (100 members)
  - 60 Lawful members (want sharing, rules)
  - 40 Chaotic members (want freedom, hoarding)

Year 1: Tensions rise
  - Lawful faction proposes mandatory 10% treasury contribution
  - Chaotic faction opposes (refuse to contribute)
  - Vote: 60 Yes, 40 No → Proposal passes

Year 2: Chaotic faction sabotages
  - Chaotic members refuse to pay contributions (civil disobedience)
  - Lawful faction demands exile for non-payers
  - Guild master (Lawful) issues ultimatum: "Pay or leave"

Year 3: Factional split
  - 35 Chaotic members leave, form "Free Crafters Collective"
  - 5 Chaotic members stay (submit to Lawful rules)
  - "United Crafters' Guild" now 65 members (95% Lawful)

Result: Two guilds emerge from internal conflict
```

---

## Exploitation Mechanics

**Guilds exploit based on alignment and outlook.**

```
Exploitation Types:

Economic Exploitation:
  - Price-fixing (guilds collude to set high prices)
  - Monopoly (guild controls all production, no competition)
  - Wage theft (guild masters underpay workers)
  - Usury (guild lends at predatory interest rates)

Political Exploitation:
  - Blackmail (guild threatens to strike unless council capitulates)
  - Coercion (guild forces village to grant monopoly rights)
  - Bribery (guild bribes officials for favorable laws)
  - Vote manipulation (guild members vote as bloc to control elections)

Violent Exploitation:
  - Extortion (guild threatens violence for protection money)
  - Protection rackets (guild "protects" businesses from guild-caused harm)
  - Intimidation (guild threatens non-members to prevent competition)
  - Assassination (guild eliminates rivals, critics)

Alignment Determines Methods:

Lawful Evil Guild:
  - Economic exploitation (legal, systematic)
  - Example: Merchants' Guild monopolizes grain, price-gouges during famine
  - Lawful = uses legal system (lobbies for monopoly law)
  - Evil = exploits crisis for profit

Chaotic Evil Guild:
  - Violent exploitation (illegal, brutal)
  - Example: Thieves' Guild extorts merchants ("Pay us or we burn your shop")
  - Chaotic = ignores laws, uses violence
  - Evil = no empathy, maximizes harm for profit

Neutral Evil Guild:
  - Opportunistic exploitation (whatever works)
  - Example: Assassins' Guild uses blackmail, extortion, assassination
  - Neutral = pragmatic, uses any method
  - Evil = profit-driven, no moral restraint

Lawful Good Guild:
  - No exploitation (ethical business)
  - Example: Artisans' Guild fair prices, quality products, worker protections
  - Lawful = follows rules, transparent
  - Good = serves community, no harm

Chaotic Good Guild:
  - Robin Hood exploitation (steal from rich, give to poor)
  - Example: Rebel Guild raids noble estates, redistributes to peasants
  - Chaotic = ignores laws (illegal redistribution)
  - Good = helps oppressed, harms exploiters

Example Scenarios:

Lawful Evil Merchants' Guild (Price-Fixing):
  - Guild controls 90% of village grain supply
  - Drought occurs → Grain scarce
  - Guild raises prices 300% (legal, but exploitative)
  - Villagers starve, guild profits
  - Village council cannot stop (guild has monopoly law)

Chaotic Evil Thieves' Guild (Protection Racket):
  - Guild approaches merchant: "Pay 50 gold/month or we rob you"
  - Merchant refuses → Guild burns shop
  - Merchant complies → Guild "protects" merchant from guild-caused harm
  - Village peacekeepers powerless (guild too large, violent)

Neutral Evil Assassins' Guild (Blackmail):
  - Guild discovers councilor's affair
  - Guild threatens: "Support our charter or we expose you"
  - Councilor complies → Guild gains legal recognition
  - Guild later assassinates councilor (eliminate witness)
```

---

## DOTS Implementation Architecture

### Core Components

```csharp
namespace Godgame.Guilds
{
    /// <summary>
    /// Guild master candidate attempting to form a guild.
    /// Attached to entity during charter period.
    /// </summary>
    public struct GuildCharter : IComponentData
    {
        public Entity CandidateEntity;             // Who is forming guild
        public FixedString64Bytes ProposedGuildName;
        public FixedString128Bytes ProposedPurpose;
        public uint CharterStartTick;              // When 1-week countdown started
        public uint CharterEndTick;                // When countdown expires
        public byte SignaturesGathered;            // Current signature count (0-10)
        public bool EducationCheckPassed;          // Did candidate pass education threshold?
    }

    /// <summary>
    /// Signature on a guild charter.
    /// Buffer on charter entity.
    /// </summary>
    [InternalBufferCapacity(10)]
    public struct CharterSignature : IBufferElementData
    {
        public Entity SignerEntity;                // Who signed
        public uint SignedTick;                    // When they signed
    }

    /// <summary>
    /// Guild entity (formed after charter succeeds).
    /// </summary>
    public struct Guild : IComponentData
    {
        public FixedString64Bytes GuildName;
        public FixedString128Bytes Purpose;        // "Smithing", "Heroes", "Trade", etc.
        public uint FoundedTick;
        public Entity GuildMasterEntity;           // Current leader
        public GovernanceType Governance;

        // Metrics
        public ushort MemberCount;
        public float AverageWealth;                // Abstract metric (not actual currency pool)
        public byte ReputationScore;               // 0-100

        public enum GovernanceType : byte
        {
            Democratic,
            Authoritarian,
            Meritocratic,
            Oligarchic
        }
    }

    /// <summary>
    /// Guild membership (on entities).
    /// Entities can have multiple of these (multi-guild membership).
    /// </summary>
    [InternalBufferCapacity(4)]  // Most entities belong to 1-4 guilds
    public struct GuildMembership : IBufferElementData
    {
        public Entity GuildEntity;                 // Which guild
        public uint JoinedTick;
        public byte LoyaltyScore;                  // 0-100 loyalty to THIS guild
        public MemberRole Role;

        public enum MemberRole : byte
        {
            Member,
            Officer,
            GuildMaster
        }
    }

    /// <summary>
    /// Guild relations with other guilds.
    /// Buffer on guild entity.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct GuildRelation : IBufferElementData
    {
        public Entity OtherGuildEntity;
        public sbyte TrustScore;                   // -100 to +100
        public RelationType Relation;

        public enum RelationType : byte
        {
            Neutral,
            Friendly,
            Allied,
            Rivalry,
            Hostile,
            AtWar
        }
    }

    /// <summary>
    /// Internal faction within guild.
    /// Emerges when members have conflicting outlooks.
    /// </summary>
    public struct GuildFaction : IBufferElementData
    {
        public FixedString32Bytes FactionName;     // "Lawful Faction", "Chaotic Faction"
        public byte MemberCount;                   // How many guild members in this faction
        public byte DominantOutlook;               // VillagerOutlookType
        public sbyte Alignment;                    // Moral/Order/Purity axis average
    }

    /// <summary>
    /// Active strike by guild.
    /// </summary>
    public struct GuildStrike : IComponentData
    {
        public Entity TargetEntity;                // Village or rival guild being struck against
        public FixedString64Bytes Demands;         // "30% wage increase", "Ban rival guild"
        public uint StrikeStartTick;
        public bool IsActive;
    }

    /// <summary>
    /// Active riot/violence.
    /// </summary>
    public struct GuildRiot : IComponentData
    {
        public RiotType Type;
        public Entity TargetEntity;                // Guild, village elites, or Entity.Null (everyone)
        public uint RiotStartTick;
        public ushort Casualties;

        public enum RiotType : byte
        {
            GuildVsGuild,
            GuildVsElites,
            GuildVsEveryone
        }
    }

    /// <summary>
    /// Coup attempt.
    /// </summary>
    public struct GuildCoup : IComponentData
    {
        public Entity TargetVillageEntity;
        public uint CoupStartTick;
        public byte PopularSupportPercent;         // 0-100%
        public CoupStatus Status;

        public enum CoupStatus : byte
        {
            Planning,
            InProgress,
            Success,
            Failed
        }
    }
}
```

### Key Systems

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Handles guild charter formation (signature gathering).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildCharterSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Process active charters
            foreach (var (charter, signatures, entity) in SystemAPI
                .Query<RefRW<GuildCharter>, DynamicBuffer<CharterSignature>>()
                .WithEntityAccess())
            {
                // Check if 1 week expired
                uint currentTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;
                if (currentTick >= charter.ValueRO.CharterEndTick)
                {
                    // Charter expired
                    if (charter.ValueRO.SignaturesGathered >= 10)
                    {
                        // Success: Form guild
                        FormGuild(ref state, charter.ValueRO, signatures);
                        state.EntityManager.DestroyEntity(entity);
                    }
                    else
                    {
                        // Failure: Lose charter fee, dissolve charter
                        // (Charter fee already paid, so just clean up)
                        state.EntityManager.DestroyEntity(entity);
                    }
                }
            }
        }

        private void FormGuild(ref SystemState state, GuildCharter charter, DynamicBuffer<CharterSignature> signatures)
        {
            // Create guild entity
            var guildEntity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponentData(guildEntity, new Guild
            {
                GuildName = charter.ProposedGuildName,
                Purpose = charter.ProposedPurpose,
                FoundedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
                GuildMasterEntity = charter.CandidateEntity,
                Governance = DetermineGovernance(ref state, signatures),
                MemberCount = (ushort)(signatures.Length + 1)  // Signers + guild master
            });

            // Add guild master to guild
            AddGuildMembership(ref state, charter.CandidateEntity, guildEntity, GuildMembership.MemberRole.GuildMaster);

            // Add all signers to guild
            foreach (var signature in signatures)
            {
                AddGuildMembership(ref state, signature.SignerEntity, guildEntity, GuildMembership.MemberRole.Member);
            }
        }

        private Guild.GovernanceType DetermineGovernance(ref SystemState state, DynamicBuffer<CharterSignature> signatures)
        {
            // Count member outlooks/alignments, determine governance type
            // (Simplified - would actually analyze member components)
            return Guild.GovernanceType.Democratic;
        }

        private void AddGuildMembership(ref SystemState state, Entity entityToAdd, Entity guildEntity, GuildMembership.MemberRole role)
        {
            var buffer = state.EntityManager.GetBuffer<GuildMembership>(entityToAdd);
            buffer.Add(new GuildMembership
            {
                GuildEntity = guildEntity,
                JoinedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
                LoyaltyScore = 70,  // Default starting loyalty
                Role = role
            });
        }
    }

    /// <summary>
    /// Handles signature probability calculations.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildSignatureSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // When entity requests signature from another entity, calculate probability
            // (Would be called by AI decision system or player interaction)
        }

        public float CalculateSignatureProbability(ref SystemState state, Entity guildMasterCandidate, Entity potentialSigner)
        {
            float baseChance = 0.2f;  // 20% baseline

            // Get relations bonus
            // (Would query RelationComponent between entities)
            float relationsBonus = 0f;  // Placeholder

            // Get alignment compatibility
            // (Would query VillagerAlignment on both entities)
            float alignmentBonus = 0f;  // Placeholder

            // Get outlook compatibility
            // (Would query VillagerOutlook on both entities)
            float outlookBonus = 0f;  // Placeholder

            // Get self-interest
            // (Would analyze if guild purpose benefits signer's profession/goals)
            float selfInterestBonus = 0f;  // Placeholder

            // Get fame bonus
            // (Would query Glory stat on guild master candidate)
            float fameBonus = 0f;  // Placeholder

            float finalChance = baseChance + relationsBonus + alignmentBonus + outlookBonus + selfInterestBonus + fameBonus;
            return math.clamp(finalChance, 0.05f, 0.95f);  // 5% min, 95% max
        }
    }
}
```

---

## Open Questions

1. **Charter fee scaling**: Should charter fee vary by village wealth or fixed 10 gold?
2. **Signature revocation**: Can signers withdraw signature before 1 week expires?
3. **Competing charters**: If two guild masters recruit same person simultaneously, who gets priority?
4. **Guild dissolution**: Minimum member count before guild auto-dissolves?
5. **Pseudo-guild registration**: Do they need ANY village to register initially, or fully autonomous?
6. **Faction mechanics**: How quickly do factions form? Requires specific % threshold?
7. **Coup success formula**: Exact calculation for popular support %?
8. **Exploitation detection**: Do villages detect guild exploitation automatically or only if investigate?

---

## Related Documentation

- [Guild_System.md](Guild_System.md) - Original guild mechanics (tech tier 8+ spawn system)
- [Entity_Relations_And_Interactions.md](Entity_Relations_And_Interactions.md) - Relations, trust, betrayal
- [Trade_And_Commerce_System.md](../Economy/Trade_And_Commerce_System.md) - Sanctions, smuggling, economic warfare
- [Stealth_And_Perception_System.md](../Combat/Stealth_And_Perception_System.md) - Double agent detection mechanics

---

**For Implementers:** Start with charter formation system (signature gathering, education checks), then add multi-membership tracking, finally add guild actions (strikes, riots, coups).

**For Designers:** Test charter fee economics (does 10 gold create sufficient risk for poor entities?). Balance signature probability formula (too easy = guild spam, too hard = no guild formation).

**For Narrative:** Guild formation creates emergent class dynamics (nobodies form nobody guilds, elites form elite guilds). Coups enable political upheaval stories. Multi-membership creates espionage drama.
