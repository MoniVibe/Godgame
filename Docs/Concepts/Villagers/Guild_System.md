# Guild System

**Status:** Design
**Category:** System - Autonomous Organizations
**Scope:** Guilds, Crisis Response, Inter-Village Politics
**Created:** 2025-11-03
**Last Updated:** 2025-11-03

---

## Purpose

Enable specialized autonomous organizations (guilds) that transcend individual villages, respond to world crises, form alliances/rivalries based on outlooks/alignments, and create emergent factional politics through decentralized decision-making.

**Primary Goal:** Create self-organizing guilds that respond to threats (world bosses, apocalypses, invasions) independent of player intervention.
**Secondary Goals:**
- Support cross-village cooperation via embassies
- Enable guild progression (members gain loot, traits, experience defeating threats)
- Create guild warfare with tactics driven by outlook/alignment
- Provide multiple guild types (heroes, merchants, scholars, assassins, etc.)
- Generate emergent alliances and betrayals during multi-crisis scenarios

---

## Core Concept: Guilds as Autonomous Aggregates

### What is a Guild?

**Definition:**
A **guild** is a voluntary association of villagers united by shared purpose (crisis response, trade, research, etc.) that operates **across village boundaries** with its own leadership, resources, and diplomatic relations.

**Key Differences from Villages:**
- **Villages:** Territory-based, involuntary (born into), manage local resources/governance
- **Guilds:** Purpose-based, voluntary membership, cross-village embassies, specialized missions

**Guild Types:**

1. **Heroes' Guild** - Responds to world threats (world bosses, demons, monsters)
2. **Merchants' Guild** - Manages trade routes, caravans, markets
3. **Scholars' Guild** - Research, knowledge sharing, tech advancement
4. **Assassins' Guild** - Espionage, political elimination, shadow warfare
5. **Artisans' Guild** - Crafting specialization, quality standards
6. **Farmers' Guild** - Agricultural coordination, food security
7. **Mystics' Guild** - Miracle study, worship coordination, spiritual matters
8. **Mages' Guild** - Arcane research, magical combat, spell development
9. **Holy Orders** - Religious enforcement, divine quests, heresy purges
10. **Rogues' Guild** - Thievery, infiltration, subterfuge (distinct from assassins)
11. **Rebel Factions** - Political uprising, regime change, revolutionary ideals
12. **[Extensible]** - Any specialized purpose can spawn a guild type

**Key Distinction:** All guilds are **aggregate entities** with members, internal politics, and autonomous agendas driven by outlooks/alignments. They pursue interests (altruistic or exploitative) independent of player or village control.

---

## Heroes' Guild (Primary Focus)

### Formation & Purpose

**Spawn Conditions:**
- Village reaches **advanced tier** (tech tier 8+)
- Population sustains dedicated specialists (not all villagers farming/building)
- World has **active threats** (world bosses, celestial beings, apocalypse events)

**Purpose:**
- Respond to world crises (hunt world bosses, fight demons, defend against invasions)
- Gain loot from defeated threats
- Develop specialized combat traits/skills
- Coordinate cross-village defense efforts

### Membership

**Who joins:**
- Villagers with **high combat stats** (physique, combat experience)
- **Warlike or heroic outlooks** (not necessarily chaotic evil - can be lawful good defenders)
- **Adventurous dispositions** (initiative spike when crises appear)
- **Champions/Heroes** often lead or join guilds

**Membership mechanics:**
- **Voluntary:** Villagers apply or are recruited
- **Requirements:** Minimum combat skill, compatible alignment/outlook
- **Retention:** Members can leave if alignment shifts or guild becomes incompatible

### Progression System

**Experience Gain:**
- Defeating world bosses: **+100-500 XP** (scales with boss threat tier)
- Sealing hell gates: **+50 XP**
- Killing celestial beings: **+200 XP**
- Defending villages from raids: **+30 XP**

**Trait Acquisition:**
```
Demon Slayer Trait: +50% damage vs demons (earned after 10 demon kills)
Boss Hunter Trait: +20% damage vs world bosses (earned after 3 boss kills)
Survivor Trait: +10% max health (earned after 5 near-death experiences)
Tactician Trait: +15% party coordination (earned after leading 10 missions)
```

**Loot Distribution:**
- World bosses drop **rare loot** (legendary weapons, god jewelry, artifacts)
- Guild shares loot among mission participants
- Guild master allocates based on guild charter (equal split, merit-based, auction, etc.)
- Loot improves guild power over time (better equipped for harder threats)

**Learning/Adaptation:**
- Guilds develop **specialized knowledge** of threat types
- After fighting demon world boss: Guild gains "Demon Tactics" knowledge (+10% effectiveness vs demons)
- After fighting ice elemental: Guild gains "Cold Resistance" knowledge
- Knowledge stored in guild hall (physical structure in home village)

---

## Multi-Crisis Response

### Simultaneous Threats Scenario

**Example Crisis Stack:**
1. **Demon world boss** rampaging in the north (80% territory control approaching)
2. **Undead lich** raising army in the south
3. **Ice elemental** freezing eastern biome
4. **Player deity** demanding tribute from villages (implicit threat)

**Guild Response:**
- **Multiple heroes' guilds** exist across world
- Each guild **chooses which crisis to address** based on:
  - **Proximity** (closest threat prioritized)
  - **Alignment affinity** (lawful good guild fights demons first, chaotic neutral fights whoever)
  - **Threat assessment** (which crisis endangers their home villages most?)
  - **Loot incentive** (which boss drops best gear?)

### Emergent Alliances

**Scenario: Demon Crisis Too Big for One Guild**

**Alliance Formation:**
1. **Lightbringers Guild** (lawful good, 50 members) engages demon alone, loses 10 members
2. **Stormbreakers Guild** (chaotic good, 30 members) nearby, compatible alignment
3. Lightbringers send **embassy request** to Stormbreakers
4. Stormbreakers **accept** (shared good alignment, mutual demon threat)
5. **Combined force** of 70 members launches coordinated assault
6. Alliance **shares loot** (negotiated split based on contribution)

**Alliance Durability:**
- **Temporary:** Lasts for specific crisis, dissolves after
- **Permanent:** If multiple crises align interests, guilds may formalize long-term pact
- **Betrayal risk:** Chaotic guilds may betray mid-battle if loot opportunity arises

### Tragic Betrayals

**Scenario: Chaotic Evil Guild Betrays Alliance**

**Setup:**
- **Shadowfang Guild** (chaotic evil, assassins) allies with **Ironguard Guild** (lawful neutral, defenders)
- Target: Angelic slayer threatening demon hellscape (ironic - guilds defend demons)

**Betrayal:**
1. Mid-battle, angel is weakened
2. Shadowfang sees opportunity: Kill angel, take loot, **then kill Ironguard members** (steal their loot too)
3. Shadowfang turns on Ironguard during fight
4. Ironguard **loses members** to both angel and betrayal
5. Shadowfang loots both corpses

**Consequences:**
- Ironguard **declares war** on Shadowfang (permanent hostility)
- Other lawful guilds **embargo** Shadowfang (no cooperation)
- Shadowfang reputation **tanks** in lawful villages (can't recruit there)
- Chaotic evil villages **welcome** Shadowfang (compatible alignment)

**Alignment System Impact:**
- Betrayal records stored in **guild reputation component**
- Future alliance requests **factor betrayal history**
- Lawful guilds refuse to ally with known betrayers
- Chaotic guilds don't care (or even respect the cunning)

---

## Guild Behavior Codes

### Peace in Villages (Enforced by Peacekeepers)

**Default Rule:**
Guilds **refrain from fighting in villages** to protect civilians and infrastructure.

**Enforcement:**
- **Peacekeepers** (village security force) intervene if guild members fight
- Peacekeepers **arrest combatants**, impose fines, or exile repeat offenders
- Guild masters **discipline members** who break peace codes (maintain guild reputation)

**Exceptions:**
- **Chaotic members** may **ignore peace codes** if:
  - Warlike outlook + xenophobic alignment (enemy guild members present)
  - Low patriotism to host village (don't care about village laws)
  - Impulsive personality (initiative spike overrides restraint)

**Skirmishes in Villages (Chaotic Outliers):**
- **Chaotic warlike xenophobic** guild member sees rival guild member
- Ignores peacekeeper warnings, initiates duel
- Peacekeepers attempt to arrest
- **Outcome depends on:**
  - Peacekeeper strength vs guild member combat stats
  - Guild master intervention (does guild master enforce discipline?)
  - Village alignment (lawful villages banish troublemakers, chaotic villages shrug)

**Guild Reputation Impact:**
- Lawful villages **ban guilds** whose members repeatedly break peace
- Chaotic villages **tolerate** or even enjoy the violence (entertainment)
- Guilds lose **embassy privileges** if they can't control members

---

## Guild-Specific Member Selection Criteria

### Selection Philosophy

**Core Principle:** Each guild type values **different attributes** when recruiting/electing leaders. Guilds use **weighted voting** or **automatic thresholds** based on member stats aligned with guild purpose.

**Universal Factors (All Guilds):**
- **Outlook/Alignment compatibility** (must align with guild's aggregate values)
- **Reputation/Renown** (fame, glory, dynasty prestige)
- **Loyalty to guild** (patriotism equivalent for guilds)

**Guild-Specific Factors:**
Different guilds prioritize different stats/achievements for membership and leadership.

---

### Mages' Guild - Arcane Mastery

**Purpose:** Research magic, develop spells, combat magical threats

**Member Selection Priority:**
1. **Intelligence** (primary) - Raw mental capacity
2. **Wisdom** (secondary) - Practical magical knowledge
3. **Will** (tertiary) - Magical stamina, resistance to corruption
4. **Arcane Achievements:** Spell discoveries, magical duels won, artifacts created

**Leadership Election (Democratic):**
```
Voting Weight Formula:
  Score = (Intelligence × 0.5) + (Wisdom × 0.3) + (Will × 0.2) + ArcaneAchievements

Candidate with highest score wins guild master position
Officers chosen similarly for specialized roles (research, combat, enchanting)
```

**Guild Behaviors (Outlook-Driven):**
- **Scholarly Mages (Lawful Good):** Share magical knowledge freely, teach apprentices, research for public good
- **Power-Hungry Mages (Chaotic Evil):** Hoard forbidden magic, corrupt members with dark rituals, seek dominance
- **Neutral Mages (True Neutral):** Balance order/chaos, pursue magical equilibrium, study all schools impartially

**Autonomous Agendas:**
- **Altruistic:** Defend villages from magical threats, enchant public infrastructure, educate populace
- **Exploitative:** Monopolize magic, extort villages for protection, enslave magical creatures, corrupt other guilds

**Crisis Response:**
- Hunt rogue mages/liches
- Seal magical rifts/hell gates
- Counter enemy spellcasters in wars
- Research counter-measures to apocalypses (anti-demon wards, undead banishment rituals)

---

### Holy Orders - Divine Mandate

**Purpose:** Enforce religious doctrine, purge heresy, serve deity mandates

**Member Selection Priority:**
1. **Faith** (primary) - Devotion to patron deity
2. **Belief** (secondary) - Strength of conviction
3. **Glory** (tertiary) - Heroic deeds in deity's name
4. **Divine Achievements:** Miracles witnessed/performed, heretics converted/purged, holy sites consecrated

**Leadership Election (Varies by Order):**
```
Authoritarian Holy Orders (fanatic):
  - Patriarch/Matriarch appointed by deity (player-chosen champion or AI)
  - Officers inherit positions (divine bloodlines)

Democratic Holy Orders (moderate):
  Score = (Faith × 0.6) + (Belief × 0.3) + Glory + DivineAchievements
  Elected by member vote, must reach quorum threshold
```

**Guild Behaviors (Alignment-Driven):**
- **Lawful Good Orders:** Heal sick, feed poor, defend innocent, convert peacefully
- **Lawful Evil Orders:** Inquisitions, forced conversions, execute heretics, enforce tithes
- **Chaotic Good Orders:** Revolutionary liberation theology, protect oppressed, defy corrupt churches
- **Chaotic Evil Orders:** Blood cults, sacrifice rituals, demonic pacts disguised as worship

**Autonomous Agendas:**
- **Altruistic:** Build temples, provide charity, mediate conflicts, bless crops
- **Exploitative:** Collect mandatory tithes, persecute rival religions, enslave "heathens", monopolize worship

**Crisis Response:**
- Crusades against demonic invasions
- Purge undead (holy water, consecrated weapons)
- Convert apocalypse survivors to faith
- Bless heroes' guilds before battles

**Special Mechanic - Divine Favor:**
Holy orders aligned with player deity receive **divine empowerment** (miracles, stat boosts) when player supports them. Orders aligned with rival deities (AI or other players) may oppose player interests.

---

### Rogues' Guild - Shadow Mastery

**Purpose:** Thievery, infiltration, information brokerage (distinct from assassins - less lethal focus)

**Member Selection Priority:**
1. **Finesse** (primary) - Dexterity, stealth, pickpocketing skill
2. **Glory** (secondary) - Reputation for successful heists
3. **Intelligence** (tertiary) - Planning complex operations
4. **Thievery Achievements:** Legendary thefts, infiltrations, escapes from capture

**Leadership Election (Meritocratic):**
```
Automatic Promotion:
  - Guild master = Member with highest combined Finesse + TheftRecord
  - Officers = Specialists in different domains (burglary, forgery, fencing stolen goods)

Voting only on major decisions (war, alliances), not leadership
```

**Guild Behaviors (Outlook-Driven):**
- **Robin Hood Rogues (Chaotic Good):** Steal from rich, give to poor, oppose tyranny
- **Professional Thieves (Neutral):** Steal as business, honor among thieves code, avoid violence
- **Cutthroat Bandits (Chaotic Evil):** Loot indiscriminately, betray contracts, murder witnesses

**Autonomous Agendas:**
- **Altruistic (rare):** Rob corrupt officials, liberate enslaved villagers, expose political conspiracies
- **Exploitative (common):** Fence stolen goods, blackmail wealthy villagers, extort "protection" money, smuggle contraband

**Crisis Response:**
- Infiltrate enemy factions (spy for allies)
- Steal apocalypse artifacts before enemies acquire them
- Loot abandoned ruins during crises
- Sabotage enemy supply lines (cut caravans, poison stores)

**Distinction from Assassins:**
- **Rogues:** Non-lethal by default, steal for profit, avoid direct combat
- **Assassins:** Lethal specialists, kill for contracts, trained combatants

---

### Rebel Factions - Revolutionary Change

**Purpose:** Overthrow existing power structures, regime change, ideological revolution

**Member Selection Priority:**
1. **Charisma** (primary) - Inspire followers, rally crowds
2. **Fame/Renown** (secondary) - Visible leadership, martyr potential
3. **Patriotism (to cause)** - Devotion to revolutionary ideals
4. **Revolutionary Achievements:** Successful uprisings, regime overthrows, manifestos written

**Leadership Election (Varies by Ideology):**
```
Democratic Rebels (Egalitarian):
  - All members vote equally (one person, one vote)
  - Rotating leadership (prevent power consolidation)

Vanguard Rebels (Authoritarian Left/Right):
  Score = Charisma + Fame + IdeologicalPurity
  - Inner circle votes (elites guide revolution)
  - Strongman can seize control if charismatic enough

Anarchist Rebels (Chaotic):
  - No formal leader (consensus decisions)
  - Spontaneous action (anyone can propose, execute independently)
```

**Guild Behaviors (Alignment-Driven):**
- **Lawful Good Rebels:** Peaceful protests, legal reform, constitutional change, minimize casualties
- **Chaotic Good Rebels:** Armed uprising, guerrilla warfare, topple tyrants by force but avoid civilian harm
- **Lawful Evil Rebels:** Replace old regime with new tyranny, systematic purges, ideological enforcement
- **Chaotic Evil Rebels:** Burn everything down, anarchy, nihilistic destruction

**Autonomous Agendas:**
- **Altruistic:** Liberate oppressed villages, abolish slavery, redistribute wealth, install democracy
- **Exploitative:** Seize power for own faction, purge rivals, impose new orthodoxy, create new aristocracy

**Crisis Response:**
- Exploit apocalypse chaos to overthrow weakened governments
- Form alliances with other factions against common enemy (then betray after crisis)
- Radicalize desperate survivors (apocalypse proves system failure)
- Seize abandoned resources/territories during evacuations

**Formation Triggers:**
- Village suffers prolonged tyranny (low morale, high taxation, brutal justice)
- Alignment/outlook mismatch between population and leadership
- External conquest (occupied villages spawn resistance movements)
- Ideological spread (rebels from one village inspire others)

**Special Mechanic - Underground Network:**
Rebel factions operate **covertly** until strong enough to act openly. Hidden cells in multiple villages coordinate via secret embassies (safe houses). Peacekeepers hunt rebels; rebels evade/infiltrate peacekeepers.

---

### Merchants' Guild - Economic Power

**Member Selection Priority:**
1. **Fortune** (primary) - Total wealth/assets owned
2. **Mercantile Achievements** - Successful trade deals, caravans run, monopolies established
3. **Outlook Alignment** - Materialistic outlook strongly preferred
4. **Reputation** - Trustworthiness in contracts (lawful) or cunning in negotiations (chaotic)

**Leadership Election (Plutocratic):**
```
Voting Weight = Fortune (wealth directly equals votes)
  - Richest members have most influence
  - Guild master = Wealthiest member OR elected by wealth-weighted vote

Officer roles purchased (bid for Quartermaster, Caravan Master positions)
```

**Guild Behaviors:**
- **Ethical Merchants (Lawful Good):** Fair prices, quality goods, support local economies, provide charity
- **Ruthless Monopolists (Lawful Evil):** Price-fixing cartels, crush competitors, exploit shortages, debt slavery
- **Free Traders (Chaotic Neutral):** No regulation, smuggle anything, exploit arbitrage, dynamic pricing

**Autonomous Agendas:**
- **Altruistic:** Stabilize economies during crises, provide low-interest loans, distribute food during famines
- **Exploitative:** Hoard resources to inflate prices, profiteer from wars, monopolize essential goods, debt-trap villages

---

### Scholars' Guild - Knowledge Pursuit

**Member Selection Priority:**
1. **Education Level** (primary) - Years of study, degrees earned
2. **Intelligence** (secondary) - Learning capacity
3. **Wisdom** (tertiary) - Applied knowledge
4. **Scholarly Achievements** - Books written, discoveries made, students taught

**Leadership Election (Academic Meritocracy):**
```
Score = EducationYears + (Intelligence × 0.3) + Publications + Citations

Guild master = Highest academic prestige
Officers = Department heads (history, natural philosophy, mathematics, etc.)
```

**Guild Behaviors:**
- **Open Knowledge (Lawful Good):** Publish freely, open libraries, educate all classes, peer review
- **Gatekeepers (Lawful Neutral):** Restrict dangerous knowledge, control access, preserve orthodoxy
- **Forbidden Researchers (Chaotic Evil):** Study taboo subjects (necromancy, torture science), hoard secrets, experiment on unwilling subjects

**Autonomous Agendas:**
- **Altruistic:** Advance civilization, cure diseases, improve agriculture, archive history
- **Exploitative:** Weaponize knowledge, sell research to highest bidder, suppress inconvenient truths, create dependency on guild expertise

---

## Guild Agendas & Autonomous Behavior

### Interest-Driven Actions

**Guilds act based on aggregated member outlooks/alignments:**

**Altruistic Guilds (Good-aligned majority):**
- Pursue public good (defense, education, charity)
- Share resources with non-members
- Ally with villages/other guilds freely
- Sacrifice for greater cause

**Exploitative Guilds (Evil-aligned majority):**
- Maximize guild power/wealth
- Hoard resources, monopolize services
- Exploit non-members (extortion, coercion, slavery)
- Betray alliances if profitable

**Neutral Guilds (Balanced alignment):**
- Transactional relationships (services for payment)
- Preserve guild interests above all
- Ally when mutually beneficial, abandon when costly
- Pragmatic opportunism

### Guild Interests Examples

**Mages' Guild:**
- Altruistic: Protect villages from magical threats, teach magic freely
- Exploitative: Monopolize magic, extort payment for dispelling curses they secretly cast

**Holy Orders:**
- Altruistic: Heal sick, provide sanctuary, mediate peace
- Exploitative: Force conversions, execute heretics, collect tithes under threat

**Rogues' Guild:**
- Altruistic: Rob corrupt nobles, redistribute to poor
- Exploitative: Extort merchants, fence stolen goods, blackmail officials

**Rebel Factions:**
- Altruistic: Liberate oppressed, install fair governance
- Exploitative: Seize power, purge rivals, impose new tyranny

**Merchants' Guild:**
- Altruistic: Stabilize prices, provide charity, low-interest loans
- Exploitative: Price-gouging, monopolies, debt-trap villages

**Scholars' Guild:**
- Altruistic: Public education, free libraries, publish discoveries
- Exploitative: Hoard knowledge, sell research, suppress dissent

---

## Embassy System

### Purpose

**Why Embassies?**
Guilds are **not limited to one village** - they operate across multiple settlements. Embassies provide:
- **Recruitment centers** in allied villages
- **Diplomatic presence** (negotiate with local governments)
- **Safe houses** for guild members traveling between villages
- **Information networks** (share intel on threats, trade opportunities)

### Embassy Mechanics

**Establishment:**
1. Guild requests **embassy** in target village
2. Village leadership evaluates:
   - **Alignment compatibility:** Lawful good village unlikely to accept chaotic evil assassins
   - **Outlook alignment:** Scholarly villages welcome scholars' guild, warlike villages welcome heroes' guild
   - **Guild reputation:** Has this guild caused problems elsewhere?
   - **Village capacity:** Does village have space/resources for embassy building?

3. If approved, guild **constructs embassy building** (costs resources, takes time)
4. Embassy staffed with **guild representatives** (1-3 members)

**Embassy Functions:**
- **Recruitment:** Village members can join guild via embassy
- **Missions:** Guild posts bounties/quests for local members
- **Sanctuary:** Guild members can rest/heal at embassy
- **Coordination:** Embassy relays intel to guild headquarters

**Embassy Locations:**
- Guilds establish embassies in **like-minded villages**
- Heroes' guild (lawful good) → Embassies in lawful good/neutral villages
- Assassins' guild (chaotic evil) → Embassies in chaotic evil villages, secret hideouts in neutral zones

**Multi-Embassy Networks:**
- Large guilds have **10+ embassies** across world
- Creates **guild territories** (regions with dense embassy presence)
- Rival guilds compete for embassy access in neutral villages

---

## Multiple Guild Types

### Guild Type Behaviors

**Merchants' Guild:**
- **Purpose:** Coordinate trade routes, set prices, protect caravans
- **Membership:** Traders, caravan guards, logistics specialists
- **Crises Response:** Economic collapse, bandit raids on trade routes
- **Warfare:** Economic sanctions, caravan ambushes (if hostile)

**Scholars' Guild:**
- **Purpose:** Research coordination, knowledge sharing, library maintenance
- **Membership:** Educated villagers, researchers, scribes
- **Crises Response:** Apocalypse research (how to counter demons/undead)
- **Warfare:** Knowledge hoarding, sabotage rival research (steal/burn books)

**Assassins' Guild:**
- **Purpose:** Political eliminations, espionage, shadow operations
- **Membership:** Stealthy, high dexterity, morally flexible
- **Crises Response:** Assassinate world boss lieutenants, infiltrate demon cities
- **Warfare:** Targeted killings, sabotage, blackmail

**Artisans' Guild:**
- **Purpose:** Crafting standards, apprenticeship programs, quality control
- **Membership:** Blacksmiths, carpenters, jewelers, tailors
- **Crises Response:** Craft siege weapons, forge anti-demon weapons
- **Warfare:** Resource embargoes, sabotage rival equipment

**Farmers' Guild:**
- **Purpose:** Agricultural coordination, crop rotation, food reserves
- **Membership:** Farmers, ranchers, harvesters
- **Crises Response:** Maintain food supply during apocalypse, combat famine
- **Warfare:** Food blockades, poison enemy crops

**Mystics' Guild:**
- **Purpose:** Study miracles, coordinate worship, spiritual guidance
- **Membership:** High-faith villagers, priests, shamans
- **Crises Response:** Counter dark miracles, purify corrupted land
- **Warfare:** Curse rituals, blessing removal, spiritual warfare

### Inter-Guild Type Interactions

**Natural Alliances:**
- Heroes + Merchants (heroes protect caravans, merchants fund heroes)
- Scholars + Mystics (knowledge sharing, miracle research)
- Artisans + Farmers (tools for farming, food for crafters)

**Natural Rivalries:**
- Heroes + Assassins (honor vs treachery)
- Merchants + Assassins (assassination disrupts trade)
- Lawful Scholars + Chaotic Mystics (ordered knowledge vs chaotic faith)

---

## Guild Warfare

### Declaration of War

**Trigger Conditions:**
- **Betrayal** (alliance broken during crisis)
- **Ideological opposition** (lawful good vs chaotic evil reach critical mass)
- **Resource competition** (both guilds want same territory/embassies)
- **Guild master rivalry** (personal vendetta)

**Declaration Mechanics:**
1. Guild master **calls vote** (democratic guilds) or **decrees war** (authoritarian guilds)
2. Guild members vote (if democratic) - requires **60%+ approval**
3. War declared, guild enters **War Footing state**

### Warfare Tactics (Outlook/Alignment Driven)

**Chaotic Pure Evil Guild (e.g., Demon Worshippers):**
- **No rules:** Attack villages, kill civilians, burn infrastructure
- **Collateral damage:** Don't care about bystanders
- **Terror tactics:** Maximize fear to break enemy morale
- **Scorched earth:** Destroy everything rival guild values

**Lawful Good Guild (e.g., Paladins):**
- **Rules of engagement:** Target only guild combatants
- **Minimize civilian casualties:** Evacuate villages before attacks
- **Capture over kill:** Imprison enemy guild members, offer redemption
- **Precision strikes:** Sabotage guild halls, avoid collateral damage

**Chaotic Good Guild (e.g., Rebels):**
- **Guerrilla warfare:** Hit-and-run, ambushes, asymmetric tactics
- **Protect innocents:** Don't harm civilians, but bend laws
- **Sabotage:** Disrupt enemy logistics without mass destruction
- **Inspire defections:** Convince enemy guild members to switch sides

**Lawful Neutral Guild (e.g., Mercenaries):**
- **Professional warfare:** Follow contract terms precisely
- **Predictable tactics:** Announce attacks, honor truces
- **Economic warfare:** Cut funding, seize assets, negotiate ransoms
- **Minimal casualties:** Efficient violence (war is business, not personal)

**Neutral Evil Guild (e.g., Slavers):**
- **Opportunistic violence:** Attack when advantageous, retreat when costly
- **Capture for profit:** Enslave defeated enemies, sell to highest bidder
- **Pragmatic cruelty:** Torture if it yields intel, spare if it saves resources
- **Flexible alliances:** Betray if profit outweighs loyalty

### Peaceful/Lawful Guild Warfare: Espionage & Subtlety

**Scenario: Lawful Good Heroes vs Chaotic Evil Assassins**

**Lawful Good Approach (No Civilian Casualties):**
1. **Intelligence Gathering:**
   - Hire neutral spies to infiltrate assassin guild
   - Identify assassin guild members via records/embassies
   - Map assassin hideouts without civilian areas

2. **Surgical Strikes:**
   - Raid assassin safe houses at night (no civilians present)
   - Capture assassins one-by-one (avoid public battles)
   - Use peacekeepers to arrest assassins legally (if village law permits)

3. **Economic Pressure:**
   - Boycott villages that host assassin embassies
   - Offer villages **protection deals** if they ban assassins
   - Outbid assassins for contracts (make assassination unprofitable)

4. **Infiltration & Sabotage:**
   - Plant double agents in assassin guild
   - Steal assassin contracts (prevent murders before they happen)
   - Poison assassin weapon caches (non-lethally, just disable)

5. **Legal Warfare:**
   - Petition village councils to **outlaw assassin guilds**
   - Frame assassins for crimes (if corrupt/desperate)
   - Use village justice systems to imprison assassin leaders

**Outcome:**
- **No civilian casualties** (lawful good code maintained)
- **Slow but effective** (assassins lose safe havens, members arrested over months/years)
- **Reputation boost** (lawful villages respect honorable tactics)

### Full Commitment: Guild Total War

**Scenario: Chaotic Evil Assassins vs Lawful Good Heroes (Full War)**

**Chaotic Evil Tactics:**
- **Village Raids:** Attack hero guild embassies, burn buildings, kill anyone inside
- **Poison Wells:** Contaminate water supplies in hero-aligned villages
- **Mass Assassination:** Kill village leaders who support heroes
- **Civilian Hostages:** Capture families of hero guild members (leverage)

**Lawful Good Response (Adapting to Total War):**
- **Evacuation:** Move civilians from contested zones to safe villages
- **Fortification:** Harden embassies (walls, guards, traps)
- **Mobilization:** Call all guild members + allied guilds (form army)
- **Crusade:** March on assassin strongholds (full military assault)

**Escalation:**
- War spreads to allied guilds (heroes' allies vs assassins' allies)
- Villages pick sides (some ban both guilds, others commit resources)
- World state shifts: **Guild War apocalypse** (new crisis type)
- Player deity can **intervene** (miracles to support one side) or **observe** (let guilds resolve it)

**Resolution:**
- **Victory:** One guild destroyed (members killed/disbanded/exiled)
- **Stalemate:** Both guilds exhausted, truce negotiated by neutral guild
- **Absorption:** Losing guild remnants absorbed by winner (convert or die)
- **Apocalypse Interruption:** World boss spawns, both guilds forced to cooperate or perish

---

## Leadership Structure

### Guild Master

**Selection:**
- **Democratic Guilds:** Voted by members (simple majority or ranked choice)
- **Authoritarian Guilds:** Inherited (master's successor) or seized (strongest member claims title)
- **Meritocratic Guilds:** Highest skill/reputation automatically becomes master
- **Oligarchic Guilds:** Officers vote among themselves

**Powers:**
- **Declare war/peace**
- **Accept/reject members**
- **Allocate loot** from guild missions
- **Negotiate alliances/embassies**
- **Set guild policy** (rules of engagement, codes of conduct)

**Accountability:**
- Democratic guilds can **vote no-confidence** (remove master)
- Authoritarian guilds: Master rules until **death or coup**
- Meritocratic guilds: Master loses title if skill drops below threshold

### Officers

**Roles:**
- **Quartermaster:** Manages guild resources/loot
- **Recruiter:** Handles membership applications
- **Diplomat:** Negotiates with other guilds/villages
- **War Master:** Plans military operations
- **Spy Master:** Oversees espionage (if applicable)

**Selection:**
- **Appointed** by guild master (authoritarian)
- **Elected** by members (democratic)
- **Merit-based** (highest skill in relevant area)

**Powers:**
- Execute guild master's orders
- Make tactical decisions (master handles strategy)
- Recommend policy changes (master approves/rejects)

### Voting Mechanics

**Democratic Guilds:**
- Members vote on:
  - Guild master selection (every X years or on-demand if no-confidence)
  - War declarations (60%+ approval required)
  - Major policy changes (embassy expansion, alliance treaties)
  - Loot distribution rules (equal vs merit-based)

**Voting Implementation:**
```
Guild Vote Event:
1. Proposal introduced (by master or petition of 10%+ members)
2. Voting period (1-3 in-game days)
3. Each member casts vote (Yes/No/Abstain)
4. Tally: If Yes > threshold → Pass, else Fail
5. Execute decision or dismiss proposal
```

**Authoritarian Guilds:**
- **No voting** - Master decides unilaterally
- Faster decision-making (no debate)
- Risk: Members may **coup** if master unpopular

---

## Assassination & Espionage

### Assassinating Guild Masters

**Who can attempt:**
- **Rival guild members** (assassins' guild specialty)
- **Own guild members** (coup attempt)
- **Village leaders** (eliminate threatening guild)
- **Player deity** (via miracle or champion)

**Assassination Mechanics:**

**Detection Roll:**
```
Assassin Stealth vs (Guild Master Perception + Officer Vigilance + Bodyguard Count)

If Assassin wins: Assassination attempt proceeds undetected
If Guild Master wins: Assassin discovered, arrested/killed
```

**Execution Roll:**
```
Assassin Damage vs Guild Master Health + Armor + Luck

Critical Success: Instant kill
Success: Master wounded, may survive
Failure: Master unharmed, assassin flees or captured
Critical Failure: Assassin killed by bodyguards
```

**Consequences:**

**Successful Assassination:**
- Guild master **dies**
- **Succession crisis:**
  - Democratic: Emergency election (3-day vote)
  - Authoritarian: Strongest officer claims title (may trigger internal war)
  - Meritocratic: Next highest skilled member promoted

- **Guild Response:**
  - If assassin from **rival guild:** War declaration (if not already at war)
  - If assassin from **own guild:** Traitor hunted, executed if caught
  - If assassin from **village:** Guild vs village conflict (guild may leave or attack)

**Failed Assassination:**
- Assassin **captured:** Interrogated (may reveal employer), executed/imprisoned
- Guild master **retaliates:**
  - Lawful: Legal prosecution, demand justice from assassin's guild
  - Chaotic: Counter-assassination, strike assassin's guild
  - Neutral: Increase security, hire bodyguards, fortify

### Espionage & Intelligence

**Spy Recruitment:**
- Guilds **recruit spies** from neutral populations
- Spies **infiltrate** rival guilds, villages, or courts
- Disguise identities, pose as members

**Intelligence Gathering:**
- **Guild Plans:** Steal mission targets, loot caches, war strategies
- **Membership Rosters:** Identify rival guild members for targeting
- **Embassy Locations:** Map safe houses for raids
- **Village Politics:** Learn which leaders support which guilds

**Counter-Intelligence:**
- **Spy Master role:** Detect infiltrators
- **Loyalty Tests:** Periodic checks (interrogations, truth rituals)
- **Double Agents:** Turn enemy spies (feed false intel)
- **Paranoia:** Authoritarian guilds may purge suspected traitors (false positives common)

**Espionage Outcomes:**
- **Successful Spying:** Guild gains **intelligence advantage** (surprise attacks, preempt ambushes)
- **Caught Spy:** Interrogated → Reveals guild secrets → Counter-espionage
- **Double Agent:** Enemy guild feeds false intel → Ambush or misdirection

---

## DOTS 1.4 Implementation Architecture

### Core Components

```csharp
namespace Godgame.Guilds
{
    /// <summary>
    /// Extended villager stats for guild selection.
    /// Attached to villagers who may join guilds.
    /// </summary>
    public struct VillagerExtendedStats : IComponentData
    {
        // Mental Stats (Mages, Scholars)
        public byte Intelligence;      // 0-100: Raw mental capacity
        public byte Wisdom;            // 0-100: Applied knowledge
        public byte Will;              // 0-100: Mental stamina, magical resistance

        // Physical/Combat Stats (Heroes, Assassins, Rogues)
        public byte Physique;          // 0-100: Physical strength
        public byte Finesse;           // 0-100: Dexterity, stealth, precision
        public byte Agility;           // 0-100: Speed, reflexes

        // Social Stats (Rebels, Holy Orders, Merchants)
        public byte Charisma;          // 0-100: Inspire followers, leadership
        public byte Faith;             // 0-100: Religious devotion
        public byte Belief;            // 0-100: Conviction strength

        // Economic Stats (Merchants, Artisans)
        public float Fortune;          // Total wealth/assets (gold value)
        public ushort AssetsOwned;     // Number of properties/businesses

        // Education (Scholars)
        public byte EducationLevel;    // 0-100: Years of study
        public ushort BooksRead;       // Knowledge accumulation
    }

    /// <summary>
    /// Tracks villager achievements for guild-specific selection.
    /// </summary>
    public struct VillagerAchievements : IComponentData
    {
        // Combat Achievements (Heroes)
        public ushort WorldBossesSlain;
        public ushort DemonsKilled;
        public ushort UndeadKilled;
        public ushort Glory;               // Heroic deed points

        // Arcane Achievements (Mages)
        public byte SpellsDiscovered;
        public ushort MagicalDuelsWon;
        public byte ArtifactsCreated;
        public ushort ArcaneResearch;

        // Thievery Achievements (Rogues)
        public ushort LegendaryThefts;     // High-value heists
        public ushort SuccessfulInfiltrations;
        public byte EscapesFromCapture;
        public float TotalStolenValue;

        // Assassination Achievements (Assassins)
        public ushort ContractsCompleted;
        public byte HighProfileKills;      // Famous targets
        public ushort StealthKills;

        // Divine Achievements (Holy Orders)
        public ushort MiraclesWitnessed;
        public byte HereticsConverted;
        public byte HolySitesConsecrated;

        // Revolutionary Achievements (Rebels)
        public byte UprisingsLed;
        public byte RegimesOverthrown;
        public byte ManifestosWritten;

        // Mercantile Achievements (Merchants)
        public ushort SuccessfulTrades;
        public byte MonopoliesEstablished;
        public uint TotalProfitGenerated;

        // Scholarly Achievements (Scholars)
        public byte BooksWritten;
        public ushort DiscoveriesMade;
        public ushort StudentsTaught;
        public ushort CitationsReceived;
    }

    /// <summary>
    /// Guild-specific selection scoring.
    /// Calculated dynamically when guild evaluates candidates.
    /// </summary>
    public struct GuildCandidateScore : IComponentData
    {
        public Entity GuildEntity;         // Which guild is evaluating
        public float Score;                // Calculated based on guild type
        public uint LastEvaluationTick;
        public bool IsEligible;            // Meets minimum requirements?
    }

    /// <summary>
    /// Core guild entity component.
    /// Guilds are aggregate entities similar to villages/bands.
    /// </summary>
    public struct Guild : IComponentData
    {
        public enum GuildType : byte
        {
            Heroes,
            Merchants,
            Scholars,
            Assassins,
            Artisans,
            Farmers,
            Mystics,
            Mages,          // Arcane specialists
            HolyOrder,      // Religious enforcement
            Rogues,         // Thieves and infiltrators
            Rebels          // Revolutionary factions
            // Extensible
        }

        public GuildType Type;
        public FixedString64Bytes GuildName;       // "Lightbringers", "Shadowfang"
        public uint FoundedTick;

        // Home village (headquarters)
        public Entity HomeVillage;
        public float3 HeadquartersPosition;

        // Power metrics
        public ushort MemberCount;
        public float AverageMemberLevel;           // Combat skill average
        public uint TotalExperience;               // Cumulative XP from missions

        // Diplomatic state
        public byte ReputationScore;               // 0-100 general reputation
        public FixedString64Bytes CurrentMission;  // "Hunt Demon Lord Korgath"
    }

    /// <summary>
    /// Guild membership roster.
    /// </summary>
    public struct GuildMember : IBufferElementData
    {
        public Entity VillagerEntity;              // Which villager is a member
        public uint JoinedTick;
        public ushort ExperienceContributed;       // XP earned for guild
        public byte Rank;                          // 0=Member, 1=Officer, 2=Master
        public bool IsOfficer;
        public bool IsGuildMaster;
    }

    /// <summary>
    /// Guild alignment and outlooks (aggregate of members).
    /// </summary>
    public struct GuildAlignment : IComponentData
    {
        // Tri-axis alignment (like villagers)
        public sbyte MoralAxis;                    // -100 (evil) to +100 (good)
        public sbyte OrderAxis;                    // -100 (chaotic) to +100 (lawful)
        public sbyte PurityAxis;                   // -100 (corrupt) to +100 (pure)

        // Primary outlooks (up to 3)
        public byte Outlook1;                      // VillagerOutlookType enum
        public byte Outlook2;
        public byte Outlook3;

        // Computed from member average
        public bool IsFanatic;                     // Any outlook extreme?
    }

    /// <summary>
    /// Guild leadership structure.
    /// </summary>
    public struct GuildLeadership : IComponentData
    {
        public enum GovernanceType : byte
        {
            Democratic,        // Members vote
            Authoritarian,     // Master rules absolutely
            Meritocratic,      // Highest skill leads
            Oligarchic         // Officers vote
        }

        public GovernanceType Governance;
        public Entity GuildMasterEntity;           // Current leader
        public uint MasterElectedTick;             // When elected/seized power

        // Officers
        public Entity QuartermasterEntity;
        public Entity RecruiterEntity;
        public Entity DiplomatEntity;
        public Entity WarMasterEntity;
        public Entity SpyMasterEntity;             // Only for espionage guilds

        // Voting state
        public bool VoteInProgress;
        public FixedString64Bytes VoteProposal;    // "Declare war on Shadowfang"
        public uint VoteEndTick;
    }

    /// <summary>
    /// Active vote buffer for democratic guilds.
    /// </summary>
    public struct GuildVote : IBufferElementData
    {
        public Entity VoterEntity;
        public bool VotedYes;
        public bool VotedNo;
        public bool Abstained;
    }

    /// <summary>
    /// Guild embassies in other villages.
    /// </summary>
    public struct GuildEmbassy : IBufferElementData
    {
        public Entity VillageEntity;               // Which village hosts embassy
        public float3 EmbassyPosition;
        public Entity EmbassyBuildingEntity;       // Physical building
        public uint EstablishedTick;

        // Embassy staff
        public Entity Representative1;
        public Entity Representative2;
        public Entity Representative3;
    }

    /// <summary>
    /// Inter-guild relations.
    /// </summary>
    public struct GuildRelation : IBufferElementData
    {
        public enum RelationType : byte
        {
            Neutral,
            Allied,
            Hostile,
            AtWar,
            Betrayed       // Special state after alliance broken
        }

        public Entity OtherGuildEntity;
        public RelationType Relation;
        public sbyte TrustLevel;                   // -100 to +100
        public uint RelationSinceTick;

        // Betrayal tracking
        public bool HasBetrayed;                   // Did this guild betray us?
        public uint BetrayalTick;
    }

    /// <summary>
    /// Guild progression and specialized knowledge.
    /// </summary>
    public struct GuildKnowledge : IComponentData
    {
        // Threat-specific bonuses
        public byte DemonSlayingBonus;             // 0-100%
        public byte UndeadSlayingBonus;
        public byte BossHuntingBonus;
        public byte CelestialCombatBonus;

        // Tactical knowledge
        public byte EspionageEffectiveness;
        public byte CoordinationBonus;
        public byte SurvivalBonus;

        // Total kills (for learning)
        public ushort DemonsKilled;
        public ushort UndeadKilled;
        public ushort BossesKilled;
        public ushort CelestialsKilled;
    }

    /// <summary>
    /// Guild resources (loot, treasury).
    /// </summary>
    public struct GuildTreasury : IComponentData
    {
        public float GoldReserves;
        public float LootValue;                    // Total value of stored equipment
        public ushort LegendaryItemCount;
    }

    /// <summary>
    /// Loot inventory buffer.
    /// </summary>
    public struct GuildLootItem : IBufferElementData
    {
        public FixedString64Bytes ItemName;
        public byte ItemTier;                      // 1-5 (common to legendary)
        public float ItemValue;
        public uint AcquiredTick;
        public Entity AcquiredFromBoss;            // Which boss dropped this
    }

    /// <summary>
    /// Active guild mission.
    /// </summary>
    public struct GuildMission : IComponentData
    {
        public enum MissionType : byte
        {
            None,
            HuntWorldBoss,
            DefendVillage,
            SealHellGate,
            AssassinateTarget,
            EscortCaravan,
            Research
        }

        public MissionType Type;
        public Entity TargetEntity;                // Boss, village, etc.
        public float3 TargetPosition;
        public uint MissionStartTick;
        public ushort ParticipantCount;            // How many members on mission

        // Rewards
        public float ExpectedExperience;
        public float ExpectedLoot;
    }

    /// <summary>
    /// Guild warfare state.
    /// </summary>
    public struct GuildWarState : IComponentData
    {
        public bool AtWar;
        public Entity EnemyGuildEntity;
        public uint WarDeclaredTick;

        // War tactics (based on alignment)
        public bool TargetCivilians;               // Chaotic evil = true
        public bool UseEspionage;                  // Lawful/subtle = true
        public bool AcceptSurrender;               // Lawful good = true

        // War progress
        public ushort EnemyMembersKilled;
        public ushort OwnMembersKilled;
        public ushort EmbassiesDestroyed;
    }
}
```

---

## System Flow: Guild Formation

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Spawns guilds when villages reach advanced tier and threats exist.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildFormationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Query villages ready for guilds
            foreach (var (village, techTier, entity) in SystemAPI
                .Query<RefRO<Village>, RefRO<VillageTechTier>>()
                .WithNone<HasGuildTag>()  // Not yet spawned guild
                .WithEntityAccess())
            {
                // Check tech tier threshold
                const byte GuildFormationTier = 8;
                if (techTier.ValueRO.CurrentTier < GuildFormationTier)
                    continue;

                // Check if world has threats (world bosses, apocalypse, etc.)
                bool worldHasThreats = CheckForActiveThreats(ref state);
                if (!worldHasThreats)
                    continue;  // No need for heroes' guild yet

                // Determine guild type based on village alignment/outlook
                var guildType = DetermineGuildType(ref state, entity);

                // Spawn guild
                Entity guildEntity = CreateGuild(ref state, ref ecb, entity, guildType);

                // Mark village as having spawned guild
                ecb.AddComponent<HasGuildTag>(entity);

                // Recruit initial members from village
                RecruitInitialMembers(ref state, ref ecb, guildEntity, entity);
            }
        }

        private bool CheckForActiveThreats(ref SystemState state)
        {
            // Check for world bosses
            var bossQuery = SystemAPI.QueryBuilder().WithAll<WorldBoss>().Build();
            if (!bossQuery.IsEmpty)
                return true;

            // Check for apocalypse state
            if (SystemAPI.TryGetSingleton<WorldStateData>(out var worldState))
            {
                if (worldState.CurrentState != WorldStateData.State.Normal)
                    return true;
            }

            return false;
        }

        private Guild.GuildType DetermineGuildType(ref SystemState state, Entity villageEntity)
        {
            // Read village alignment/outlook
            if (!state.EntityManager.HasComponent<VillageAlignment>(villageEntity))
                return Guild.GuildType.Heroes;  // Default

            var alignment = state.EntityManager.GetComponentData<VillageAlignment>(villageEntity);

            // Warlike villages → Heroes' guild
            if (alignment.Outlook1 == (byte)VillagerOutlookType.Warlike ||
                alignment.Outlook2 == (byte)VillagerOutlookType.Warlike)
            {
                return Guild.GuildType.Heroes;
            }

            // Scholarly villages → Scholars' guild
            if (alignment.Outlook1 == (byte)VillagerOutlookType.Scholarly)
            {
                return Guild.GuildType.Scholars;
            }

            // Mercantile → Merchants
            if (alignment.Outlook1 == (byte)VillagerOutlookType.Mercantile)
            {
                return Guild.GuildType.Merchants;
            }

            // Chaotic evil → Assassins
            if (alignment.MoralAxis < -50 && alignment.OrderAxis < -30)
            {
                return Guild.GuildType.Assassins;
            }

            return Guild.GuildType.Heroes;  // Fallback
        }

        private Entity CreateGuild(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity villageEntity, Guild.GuildType type)
        {
            var guildEntity = ecb.CreateEntity();

            var villageName = state.EntityManager.GetComponentData<VillageName>(villageEntity);
            var villageTransform = state.EntityManager.GetComponentData<LocalTransform>(villageEntity);

            ecb.AddComponent(guildEntity, new Guild
            {
                Type = type,
                GuildName = GenerateGuildName(type, villageName),
                FoundedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
                HomeVillage = villageEntity,
                HeadquartersPosition = villageTransform.Position,
                MemberCount = 0,
                AverageMemberLevel = 0f,
                TotalExperience = 0,
                ReputationScore = 50  // Neutral start
            });

            ecb.AddBuffer<GuildMember>(guildEntity);
            ecb.AddBuffer<GuildEmbassy>(guildEntity);
            ecb.AddBuffer<GuildRelation>(guildEntity);
            ecb.AddBuffer<GuildLootItem>(guildEntity);

            ecb.AddComponent(guildEntity, new GuildLeadership
            {
                Governance = GuildLeadership.GovernanceType.Democratic,  // Default
                GuildMasterEntity = Entity.Null,  // Will elect in RecruitInitialMembers
                MasterElectedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime
            });

            ecb.AddComponent(guildEntity, new GuildKnowledge());
            ecb.AddComponent(guildEntity, new GuildTreasury());

            return guildEntity;
        }

        private FixedString64Bytes GenerateGuildName(Guild.GuildType type, VillageName villageName)
        {
            // Procedural naming based on type and village
            return type switch
            {
                Guild.GuildType.Heroes => new FixedString64Bytes("Lightbringers"),
                Guild.GuildType.Assassins => new FixedString64Bytes("Shadowfang"),
                Guild.GuildType.Scholars => new FixedString64Bytes("Loreseekers"),
                _ => new FixedString64Bytes("The Guild")
            };
        }

        private void RecruitInitialMembers(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity guildEntity, Entity villageEntity)
        {
            // Find suitable villagers in village
            // (Implementation would query villagers with high combat stats, compatible alignment)
            // Add them to guild member buffer
            // Elect guild master from highest-skilled recruit
        }
    }
}
```

---

## System Flow: Crisis Response

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Assigns guilds to respond to active world crises.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildCrisisResponseSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Query all active threats
            var threats = CollectActiveThreats(ref state);

            // For each guild, assign to highest priority threat
            foreach (var (guild, alignment, mission, entity) in SystemAPI
                .Query<RefRO<Guild>, RefRO<GuildAlignment>, RefRW<GuildMission>>()
                .WithAll<Guild>()  // Heroes' guilds
                .WithEntityAccess())
            {
                // Skip if already on mission
                if (mission.ValueRO.Type != GuildMission.MissionType.None)
                    continue;

                // Evaluate threats based on proximity, alignment, loot
                Entity chosenThreat = ChooseThreatTarget(ref state, entity, threats, alignment.ValueRO);

                if (chosenThreat == Entity.Null)
                    continue;  // No suitable threat

                // Assign mission
                mission.ValueRW.Type = GuildMission.MissionType.HuntWorldBoss;
                mission.ValueRW.TargetEntity = chosenThreat;
                mission.ValueRW.MissionStartTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;

                // Mobilize guild members (move toward threat)
                // (Implementation would update member AI targets)
            }
        }

        private NativeList<Entity> CollectActiveThreats(ref SystemState state)
        {
            var threats = new NativeList<Entity>(Allocator.Temp);

            // Collect world bosses
            foreach (var entity in SystemAPI.Query<RefRO<WorldBoss>>().WithEntityAccess())
            {
                threats.Add(entity);
            }

            // Collect celestial beings
            foreach (var entity in SystemAPI.Query<RefRO<CelestialBeing>>().WithEntityAccess())
            {
                threats.Add(entity);
            }

            // Collect hell gates, etc.
            // ...

            return threats;
        }

        private Entity ChooseThreatTarget(ref SystemState state, Entity guildEntity,
            NativeList<Entity> threats, GuildAlignment alignment)
        {
            // Scoring logic based on:
            // - Proximity to guild headquarters
            // - Alignment compatibility (good guilds prioritize demons)
            // - Expected loot value
            // - Threat level (higher = more XP but riskier)

            // Placeholder: Return first threat
            return threats.Length > 0 ? threats[0] : Entity.Null;
        }
    }
}
```

---

## System Flow: Guild Alliances

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Manages guild alliance formation when threats are too big for one guild.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildAllianceSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Query guilds currently on missions
            foreach (var (guild, mission, relations, entity) in SystemAPI
                .Query<RefRO<Guild>, RefRO<GuildMission>, DynamicBuffer<GuildRelation>>()
                .WithEntityAccess())
            {
                // Check if guild is struggling (lost members recently)
                bool needsHelp = CheckIfGuildStrugglingOnMission(ref state, entity, mission.ValueRO);

                if (!needsHelp)
                    continue;

                // Find compatible guilds to request alliance
                Entity allyCandidateEntity = FindAllyCandidate(ref state, entity, guild.ValueRO, relations);

                if (allyCandidateEntity == Entity.Null)
                    continue;  // No suitable ally

                // Request alliance (simplified - would involve negotiation)
                bool allianceAccepted = NegotiateAlliance(ref state, entity, allyCandidateEntity);

                if (allianceAccepted)
                {
                    // Update relations to Allied
                    UpdateGuildRelation(ref state, ref ecb, entity, allyCandidateEntity,
                        GuildRelation.RelationType.Allied);

                    // Mobilize ally guild to same mission target
                    MobilizeAllyToMission(ref state, ref ecb, allyCandidateEntity, mission.ValueRO);
                }
            }
        }

        private bool CheckIfGuildStrugglingOnMission(ref SystemState state, Entity guildEntity,
            GuildMission mission)
        {
            // Check member casualties, mission duration, etc.
            // Placeholder
            return false;
        }

        private Entity FindAllyCandidate(ref SystemState state, Entity requestingGuild,
            Guild guildData, DynamicBuffer<GuildRelation> relations)
        {
            // Find nearby guilds with compatible alignment that are not already hostile
            // Placeholder
            return Entity.Null;
        }

        private bool NegotiateAlliance(ref SystemState state, Entity guild1, Entity guild2)
        {
            // Check alignment compatibility, trust level, mutual benefit
            // Placeholder: Always accept if both lawful good
            return true;
        }

        private void UpdateGuildRelation(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity guild1, Entity guild2, GuildRelation.RelationType relationType)
        {
            // Update relation buffer for both guilds
            // (Implementation would modify GuildRelation buffer)
        }

        private void MobilizeAllyToMission(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity allyGuild, GuildMission mission)
        {
            // Set ally guild's mission to same target
            // (Implementation would update GuildMission component)
        }
    }
}
```

---

## System Flow: Guild-Specific Candidate Scoring

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Calculates candidate scores for guild membership/leadership based on guild type.
    /// Each guild type uses different stats/achievements for scoring.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildCandidateEvaluationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // For each guild, evaluate all potential candidates
            foreach (var (guild, alignment, entity) in SystemAPI
                .Query<RefRO<Guild>, RefRO<GuildAlignment>>()
                .WithEntityAccess())
            {
                // Query villagers with extended stats (potential candidates)
                foreach (var (stats, achievements, villagerId, villagerEntity) in SystemAPI
                    .Query<RefRO<VillagerExtendedStats>, RefRO<VillagerAchievements>, RefRO<VillagerId>>()
                    .WithNone<GuildMember>()  // Not already in THIS guild
                    .WithEntityAccess())
                {
                    // Calculate score based on guild type
                    float score = CalculateGuildScore(guild.ValueRO.Type, stats.ValueRO, achievements.ValueRO);

                    // Check alignment compatibility
                    bool alignmentCompatible = CheckAlignmentCompatibility(
                        ref state, villagerEntity, alignment.ValueRO);

                    // Attach or update candidate score
                    if (state.EntityManager.HasComponent<GuildCandidateScore>(villagerEntity))
                    {
                        var candidateScore = state.EntityManager.GetComponentData<GuildCandidateScore>(villagerEntity);
                        candidateScore.Score = score;
                        candidateScore.IsEligible = alignmentCompatible && score >= GetMinimumScore(guild.ValueRO.Type);
                        candidateScore.LastEvaluationTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;
                        state.EntityManager.SetComponentData(villagerEntity, candidateScore);
                    }
                }
            }
        }

        private float CalculateGuildScore(Guild.GuildType guildType,
            VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            return guildType switch
            {
                Guild.GuildType.Mages => CalculateMageScore(stats, achievements),
                Guild.GuildType.HolyOrder => CalculateHolyOrderScore(stats, achievements),
                Guild.GuildType.Rogues => CalculateRogueScore(stats, achievements),
                Guild.GuildType.Rebels => CalculateRebelScore(stats, achievements),
                Guild.GuildType.Merchants => CalculateMerchantScore(stats, achievements),
                Guild.GuildType.Scholars => CalculateScholarScore(stats, achievements),
                Guild.GuildType.Heroes => CalculateHeroScore(stats, achievements),
                Guild.GuildType.Assassins => CalculateAssassinScore(stats, achievements),
                _ => 0f
            };
        }

        // Guild-specific scoring functions

        private float CalculateMageScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Mages: Intelligence (0.5) + Wisdom (0.3) + Will (0.2) + Arcane Achievements
            float intelligenceScore = stats.Intelligence * 0.5f;
            float wisdomScore = stats.Wisdom * 0.3f;
            float willScore = stats.Will * 0.2f;
            float achievementScore = (achievements.SpellsDiscovered * 10f) +
                                    (achievements.MagicalDuelsWon * 2f) +
                                    (achievements.ArtifactsCreated * 15f) +
                                    (achievements.ArcaneResearch * 0.5f);

            return intelligenceScore + wisdomScore + willScore + achievementScore;
        }

        private float CalculateHolyOrderScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Holy Orders: Faith (0.6) + Belief (0.3) + Glory + Divine Achievements
            float faithScore = stats.Faith * 0.6f;
            float beliefScore = stats.Belief * 0.3f;
            float gloryScore = achievements.Glory * 0.1f;
            float divineScore = (achievements.MiraclesWitnessed * 5f) +
                               (achievements.HereticsConverted * 8f) +
                               (achievements.HolySitesConsecrated * 12f);

            return faithScore + beliefScore + gloryScore + divineScore;
        }

        private float CalculateRogueScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Rogues: Finesse (primary) + Glory + Intelligence + Thievery Achievements
            float finesseScore = stats.Finesse;  // 1:1 weight
            float gloryScore = achievements.Glory * 0.2f;
            float intelligenceScore = stats.Intelligence * 0.1f;
            float theftScore = (achievements.LegendaryThefts * 15f) +
                              (achievements.SuccessfulInfiltrations * 5f) +
                              (achievements.EscapesFromCapture * 10f) +
                              (achievements.TotalStolenValue * 0.01f);

            return finesseScore + gloryScore + intelligenceScore + theftScore;
        }

        private float CalculateRebelScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Rebels: Charisma + Fame/Renown + Revolutionary Achievements
            // Note: Fame would come from achievements.Glory or separate Renown stat
            float charismaScore = stats.Charisma;  // 1:1 weight
            float fameScore = achievements.Glory * 0.3f;  // Using Glory as proxy for fame
            float revolutionaryScore = (achievements.UprisingsLed * 20f) +
                                      (achievements.RegimesOverthrown * 30f) +
                                      (achievements.ManifestosWritten * 10f);

            return charismaScore + fameScore + revolutionaryScore;
        }

        private float CalculateMerchantScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Merchants: Fortune (primary) + Mercantile Achievements
            float fortuneScore = stats.Fortune;  // Gold value directly
            float assetScore = stats.AssetsOwned * 100f;  // Each property = 100 points
            float tradeScore = (achievements.SuccessfulTrades * 2f) +
                              (achievements.MonopoliesEstablished * 50f) +
                              (achievements.TotalProfitGenerated * 0.001f);

            return fortuneScore + assetScore + tradeScore;
        }

        private float CalculateScholarScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Scholars: Education + Intelligence (0.3) + Scholarly Achievements
            float educationScore = stats.EducationLevel * 2f;  // Heavy weight on education
            float intelligenceScore = stats.Intelligence * 0.3f;
            float wisdomScore = stats.Wisdom * 0.2f;
            float scholarlyScore = (achievements.BooksWritten * 20f) +
                                  (achievements.DiscoveriesMade * 15f) +
                                  (achievements.StudentsTaught * 3f) +
                                  (achievements.CitationsReceived * 5f);

            return educationScore + intelligenceScore + wisdomScore + scholarlyScore;
        }

        private float CalculateHeroScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Heroes: Combat stats + Glory + Combat Achievements
            float physiqueScore = stats.Physique * 0.4f;
            float agilityScore = stats.Agility * 0.3f;
            float gloryScore = achievements.Glory;
            float combatScore = (achievements.WorldBossesSlain * 100f) +
                               (achievements.DemonsKilled * 10f) +
                               (achievements.UndeadKilled * 5f);

            return physiqueScore + agilityScore + gloryScore + combatScore;
        }

        private float CalculateAssassinScore(VillagerExtendedStats stats, VillagerAchievements achievements)
        {
            // Assassins: Finesse + Agility + Assassination Achievements
            float finesseScore = stats.Finesse * 0.5f;
            float agilityScore = stats.Agility * 0.3f;
            float gloryScore = achievements.Glory * 0.2f;
            float killScore = (achievements.ContractsCompleted * 15f) +
                             (achievements.HighProfileKills * 50f) +
                             (achievements.StealthKills * 5f);

            return finesseScore + agilityScore + gloryScore + killScore;
        }

        private bool CheckAlignmentCompatibility(ref SystemState state, Entity villagerEntity,
            GuildAlignment guildAlignment)
        {
            // Check if villager's alignment is compatible with guild
            if (!state.EntityManager.HasComponent<VillagerAlignment>(villagerEntity))
                return false;

            var villagerAlign = state.EntityManager.GetComponentData<VillagerAlignment>(villagerEntity);

            // Check alignment deltas (simplified - would use proper alignment comparison)
            int moralDelta = math.abs(villagerAlign.MoralAxis - guildAlignment.MoralAxis);
            int orderDelta = math.abs(villagerAlign.OrderAxis - guildAlignment.OrderAxis);

            // Must be within alignment tolerance (e.g., 40 points difference max)
            const int AlignmentTolerance = 40;
            return (moralDelta <= AlignmentTolerance && orderDelta <= AlignmentTolerance);
        }

        private float GetMinimumScore(Guild.GuildType guildType)
        {
            // Minimum score required to be eligible for guild
            return guildType switch
            {
                Guild.GuildType.Mages => 80f,      // High barrier (intelligence required)
                Guild.GuildType.HolyOrder => 70f,  // Moderate faith requirement
                Guild.GuildType.Rogues => 60f,     // Skill-based
                Guild.GuildType.Rebels => 50f,     // Lower barrier (need members)
                Guild.GuildType.Merchants => 100f, // High barrier (need wealth)
                Guild.GuildType.Scholars => 75f,   // Education requirement
                Guild.GuildType.Heroes => 65f,     // Combat capability
                Guild.GuildType.Assassins => 70f,  // Specialized skills
                _ => 50f
            };
        }
    }
}
```

### Dynamic Leadership Election System

```csharp
namespace Godgame.Guilds.Systems
{
    /// <summary>
    /// Handles guild leadership elections based on guild governance type.
    /// Different guilds use different voting/selection mechanisms.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GuildLeadershipElectionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (guild, leadership, members, entity) in SystemAPI
                .Query<RefRO<Guild>, RefRW<GuildLeadership>, DynamicBuffer<GuildMember>>()
                .WithEntityAccess())
            {
                // Check if election needed (master died, term expired, no-confidence vote, etc.)
                bool needsElection = CheckIfElectionNeeded(ref state, leadership.ValueRO);

                if (!needsElection)
                    continue;

                // Run election based on governance type
                Entity newMaster = leadership.ValueRO.Governance switch
                {
                    GuildLeadership.GovernanceType.Democratic => RunDemocraticElection(ref state, guild.ValueRO, members),
                    GuildLeadership.GovernanceType.Authoritarian => RunAuthoritarianSuccession(ref state, members),
                    GuildLeadership.GovernanceType.Meritocratic => RunMeritocraticSelection(ref state, guild.ValueRO, members),
                    GuildLeadership.GovernanceType.Oligarchic => RunOligarchicVote(ref state, members, leadership.ValueRO),
                    _ => Entity.Null
                };

                if (newMaster != Entity.Null)
                {
                    leadership.ValueRW.GuildMasterEntity = newMaster;
                    leadership.ValueRW.MasterElectedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;

                    // Update member ranks
                    UpdateMemberRanks(ref state, members, newMaster);
                }
            }
        }

        private Entity RunDemocraticElection(ref SystemState state, Guild guild, DynamicBuffer<GuildMember> members)
        {
            // Each member votes based on guild-specific scoring
            // Member with highest average score from all voters wins

            // Collect candidates (members with scores above threshold)
            var candidates = new NativeList<(Entity, float)>(Allocator.Temp);

            foreach (var member in members)
            {
                if (!state.EntityManager.HasComponent<GuildCandidateScore>(member.VillagerEntity))
                    continue;

                var score = state.EntityManager.GetComponentData<GuildCandidateScore>(member.VillagerEntity);
                if (score.IsEligible)
                {
                    candidates.Add((member.VillagerEntity, score.Score));
                }
            }

            // Sort by score descending
            candidates.Sort(new ScoreComparer());

            // Winner = highest score
            var winner = candidates.Length > 0 ? candidates[0].Item1 : Entity.Null;

            candidates.Dispose();
            return winner;
        }

        private Entity RunMeritocraticSelection(ref SystemState state, Guild guild, DynamicBuffer<GuildMember> members)
        {
            // Automatic selection: Member with highest score becomes master
            // Same as democratic but no voting - just highest score

            Entity bestCandidate = Entity.Null;
            float highestScore = 0f;

            foreach (var member in members)
            {
                if (!state.EntityManager.HasComponent<GuildCandidateScore>(member.VillagerEntity))
                    continue;

                var score = state.EntityManager.GetComponentData<GuildCandidateScore>(member.VillagerEntity);
                if (score.Score > highestScore)
                {
                    highestScore = score.Score;
                    bestCandidate = member.VillagerEntity;
                }
            }

            return bestCandidate;
        }

        private Entity RunAuthoritarianSuccession(ref SystemState state, DynamicBuffer<GuildMember> members)
        {
            // Master appoints successor OR strongest member seizes power
            // Simplified: First officer becomes master
            foreach (var member in members)
            {
                if (member.IsOfficer)
                    return member.VillagerEntity;
            }

            // Fallback: First member
            return members.Length > 0 ? members[0].VillagerEntity : Entity.Null;
        }

        private Entity RunOligarchicVote(ref SystemState state, DynamicBuffer<GuildMember> members,
            GuildLeadership leadership)
        {
            // Only officers vote
            // Collect officer votes (simplified - would track actual votes)
            var officerCandidates = new NativeList<Entity>(Allocator.Temp);

            foreach (var member in members)
            {
                if (member.IsOfficer)
                    officerCandidates.Add(member.VillagerEntity);
            }

            // Winner = first officer (simplified)
            var winner = officerCandidates.Length > 0 ? officerCandidates[0] : Entity.Null;

            officerCandidates.Dispose();
            return winner;
        }

        private bool CheckIfElectionNeeded(ref SystemState state, GuildLeadership leadership)
        {
            // Check if guild master entity still exists
            if (!state.EntityManager.Exists(leadership.GuildMasterEntity))
                return true;

            // Check term limits, no-confidence votes, etc.
            // (Implementation would check various conditions)

            return false;
        }

        private void UpdateMemberRanks(ref SystemState state, DynamicBuffer<GuildMember> members, Entity newMaster)
        {
            // Update ranks: New master = rank 2, others = rank 0
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                member.IsGuildMaster = (member.VillagerEntity == newMaster);
                member.Rank = member.IsGuildMaster ? (byte)2 : (byte)0;
                members[i] = member;
            }
        }

        private struct ScoreComparer : IComparer<(Entity, float)>
        {
            public int Compare((Entity, float) x, (Entity, float) y)
            {
                return y.Item2.CompareTo(x.Item2);  // Descending order
            }
        }
    }
}
```

---

## Open Questions

1. **Guild Member Limits:** Max members per guild? Scales with tech tier or fixed?
2. **Embassy Costs:** How expensive to establish embassy? Resource cost vs just time?
3. **Spy Detection:** What triggers counter-intelligence checks? Random chance or behavior patterns?
4. **Guild Dissolution:** Can guilds disband if all members leave? Merge with other guilds?
5. **Cross-Guild Membership:** Can villagers belong to multiple guilds (heroes + scholars)?
6. **Guild Territories:** Do guilds claim land like villages or purely organizational?
7. **Loot Distribution Fairness:** How do members react to unfair loot splits? Can cause rebellion?
8. **Champion Integration:** Do champions auto-join heroes' guilds or remain independent?

---

## Related Documentation

- **Entity Relations:** [Entity_Relations_And_Interactions.md](Entity_Relations_And_Interactions.md) - Alliance mechanics, trust, betrayal tracking
- **Business & Assets:** [BusinessAndAssets.md](../Economy/BusinessAndAssets.md) - Outsourcing contracts, guild-owned businesses, economic interactions
- **Education & Tutoring:** [Education_And_Tutoring_System.md](Education_And_Tutoring_System.md) - Guild schools, specialized training, trade secrets
- **Spies & Double Agents:** [Spies_And_Double_Agents.md](Spies_And_Double_Agents.md) - Spy recruitment from Thieves' Guild, counter-espionage
- Crisis events: `Docs/Concepts/World/Apocalypse_And_Rebirth_Cycles.md`
- Village politics: `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md`
- Band system: `Docs/Concepts/Villagers/Bands_Vision.md`
- Alignment framework: `Docs/Concepts/Meta/Generalized_Alignment_Framework.md`

---

**For Implementers:** Start with Heroes' Guild as proof-of-concept, focusing on single-crisis response before adding multi-guild alliances.
**For Designers:** Tune alliance probability and betrayal consequences to create dramatic emergent stories without making cooperation impossible.
**For Narrative:** Guild warfare, alliances, and betrayals create factional drama that persists across multiple world states and apocalypse cycles.
