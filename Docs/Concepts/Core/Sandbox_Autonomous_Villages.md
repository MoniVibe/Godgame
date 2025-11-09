# Sandbox Autonomous Villages

**Status:** Draft  \
**Category:** Core  \
**Scope:** Global  \
**Created:** 2025-11-01  \
**Last Updated:** 2025-11-01

---

## Purpose

Enable a hands-off sandbox where large maps evolve from villager seeds into self-governed villages whose culture, expansion, and conflicts emerge from their shared outlooks and alignments.

**Primary Goal:** Let villages self-organize and progress without mandatory player input.  
**Secondary Goals:**
- Support observation-driven play where the player can nurture or disrupt communities at will.
- Provide a living baseline that other modes (campaign, skirmish, multiplayer) can build upon.
- Maintain systemic hooks for future Bands, Miracles, Logistics, and Alignment truth sources.

---

## System Overview

### Components

1. **World Map & Biomes:** Resource-bearing terrain tiles and nodes that seed the economy. - Resource
2. **Villager Populations:** Autonomous agents with needs, jobs, latent alignments, and personal initiative scores that shape task urgency. - Actor
3. **Village Cohesion:** Grouping logic that aggregates villagers into evolving settlements. - Rule
4. **Shared Outlook Alignment:** Open-ended set of outlooks/alignments (peaceful, expansionist, spiritual, martial, scholarly, etc.) derived from member behaviors that governs how surplus gets reinvested. - Rule
5. **Initiative Signals:** Per-entity and village-level initiative ratings determining pace, risk tolerance, and how aggressively plans execute. - Rule
6. **Player Presence:** Optional godly interventions (miracles, hand actions) that influence worship and alignment. - External Driver

### Connections

```
World Map → Provides → Resource Nodes & Vegetation
Villager Populations → Harvest → Resource Stores
Resource Stores → Sustain → Village Cohesion
Village Cohesion → Shapes → Shared Outlook Alignment
Shared Outlook Alignment → Modulates → Initiative Signals
Initiative Signals → Bias → Expansion & Band Formation Decisions
Player Presence → Modifies → Alignment, Initiative & Worship Propensity
```

### Feedback Loops

- **Positive:** Prosperous villages stockpile resources → morale rises → initiative climbs → birth/immigration increase → larger labor pool accelerates expansion.  
- **Negative:** Scarcity or disasters drain stores → morale drops → initiative collapses → villagers disperse or perish → remaining villagers redistribute, allowing recovery if conditions improve.  
- **Catalysts:** Miracles, crises, and personal events (loss, vengeance vows, festivals) spike or suppress initiative depending on each entity's outlook.  
- **Balance:** Worship feedback moderates player influence—benevolent miracles raise devotion (more prayer power), overreach or neglect shifts alignment and initiative away from the player.

---

## System Dynamics

### Inputs
- Initial village seeds and villager archetypes per biome.
- Ambient world events (weather shifts, wild resource growth/decay, neutral threats).
- Player miracles or interventions when the player chooses to act.
- Emotional catalysts: losses, victories, miracles, threats, festivals, and personal relationships that spike or suppress initiative based on entity outlook.

### Internal Processes
1. **Settlement Formation:** Villagers evaluate proximity, shared outlooks, and resource access to cluster into nascent villages.
2. **Resource Flow:** Harvesters convert node reserves into storehouse inventories; vegetation nodes regenerate, while mines remain depleted.
3. **Cultural Alignment:** Aggregates villager outlooks, recent events, and player actions to set village stance (e.g., isolationist, expansionist, zealous).
4. **Initiative Budgeting:** Entities and villages roll initiative against opportunity scores (threat, distance, effort, reward) to decide which plans execute first; lawful outlooks dampen delta and favor stability, while chaotic outlooks spike or crash initiative quickly in response to stimuli.
5. **Expansion Logic:** Surplus population and morale, weighted by initiative, trigger alignment-directed investments (e.g., isolationists fortify, expansionists spawn outposts, zealots channel surplus into miracle preparation).
6. **Worship Response:** Villages track miracle aid versus neglect/abuse, updating their worship intensity, minting worship mana, and adjusting willingness to align with the player; miracles can grant temporary initiative surges for faithful alignments.
7. **Village Founding:** Like-minded entities band together autonomously, scouting for resource nodes and forming settlements within reasonable distance of food/ore/vegetation clusters; distance tolerance depends on logistics outlooks and initiative.
8. **Knowledge Advancement:** Educated individuals researching in schools, universities, and academies contribute to tech progression; high-education, high-wisdom occupants accelerate advancement tiers when facilities are staffed.
9. **Governance Cycling:** Leadership evaluation runs periodically, checking outlook-driven rules (inheritance, elections, acclaim) to refresh elites without explicit player input.
10. **Resource Economy:** Villagers harvest primary resources (ore, wood, food), refine them into metals or specialized materials, and route goods through local production chains (toolmakers, weapon/armor smiths, artisan workshops) and trade caravans.
11. **Tribute & Miracles:** Player (or AI god) fulfills villager prayers (quests) or nurtures player-owned villages to earn tribute; tribute unlocks miracle families and grants bonuses when advanced villages pledge support.

**Personality Modulation:** Each entity's temperament filters initiative swings—stoic or lawful personalities convert shocks into gradual trend changes, while volatile or chaotic personalities react with sharp spikes or crashes. Traumas such as exile or the death of a bonded villager can impose depression debuffs (initiative floor) or vengeance buffs (initiative ceiling) depending on personal outlook and relationship strength.

**Behavioral Personality Axes:** Individual villagers possess behavioral traits (Vengeful/Forgiving and Bold/Craven) that shape HOW initiative manifests—determining whether autonomous actions lean toward revenge vs reconciliation, risk-taking vs safety-seeking. See `Docs/Concepts/Villagers/Villager_Behavioral_Personality.md` for detailed mechanics on how personality drives life decisions (family, business, adventure, grudges) beyond assigned jobs.

**Event Resolution Roll:** When impactful events fire (death, miracle, threat, festival), entities roll a weighted outcome table driven by their outlook/alignment and purity state (`Pure`, `Neutral`, `Corrupt`). Initiative shifts act as an offset to push the probability toward faster action. Examples:
- *Lawful Materialistic:* Inherit or assume duty (business takeover) with high probability; initiative determines how quickly they reorganize operations.
- *Lawful Spiritual:* Accept loss, gain temporary worship-mana bonus, and redirect initiative toward temple service rather than retaliation.
- *Pure Evil Warlike:* Almost always channel grief into vengeance bands targeting the aggressor's village, with high initiative yielding immediate raids.
- *Good Warlike (Neutral purity):* Favor conquest and vassalization over annihilation, scaling response timing with initiative.
- *Corrupt Good Warlike:* Wrestle between ideals and grudges—often strike preemptively under the banner of “justice,” with initiative tilting whether they escalate or seek symbolic penance after.
- *Neutral Warlike:* 50/50 roll between vengeance raid or diplomatic dominance; initiative biases the outcome toward whichever option triggers sooner.
This roll framework extrapolates to every outlook/alignment combination, letting future content author new behaviors by extending the weighted tables.

### Outputs
- Village roster with state (size tier, alignment, morale, worship intensity).
- Storehouse and reserve metrics for each settlement.
- Bands/armies spawned in response to alignment goals or external pressure.
- Telemetry hooks for sandbox health (population trend, alignment-directed expansion cadence, worship-mana balance).
- Initiative telemetry for entities and villages (average, variance, spikes) to explain rapid expansions or stalls.
- Tech tier tracking (1–20 scale) derived from research institutions and educator cohorts.
- Resource stock telemetry covering raw, refined, and luxury goods per settlement.

---

## State Machine

### States
1. **Nascent:** Small cluster stabilizing basic needs. Entry: villagers share ≥1 outlook and co-locate near resources. Exit: reach minimum population & surplus threshold or initiative spike.
2. **Established:** Sustainable village managing growth. Entry: surplus resources for N ticks. Exit: either escalate (Ascendant) when initiative exceeds ambition threshold or decline (Collapsing).
3. **Ascendant:** Proactive expansion via new bands/outposts. Entry: high morale + worship/ambition trigger + sustained initiative. Exit: resource strain or morale shock.
4. **Collapsing:** Village in decline (famine, fear, abuse). Entry: morale below floor or stores exhausted. Exit: recovered back to Established or dissolve entirely.

### Transitions

```
Nascent → (Surplus Achieved) → Established
Established → (Ambition Trigger) → Ascendant
Ascendant → (Resource Crash) → Collapsing
Collapsing → (Recovery) → Established
Collapsing → (Population <= 0) → Dissolved
```

---

## Key Metrics

| Metric | Target Range | Critical Threshold |
|--------|--------------|-------------------|
| VillageCount | 3–7 active settlements | < 2 (world feels empty) |
| AverageVillageAlignment | -40 to +40 neutral spread | |±80| (polarized, risk of runaway) |
| ResourceStability | 0.8–1.2 (stores to consumption ratio) | < 0.5 (famine spiral) |
| WorshipIntensity | 0.3–0.7 (mixed devotion) | < 0.1 (player irrelevant) |
| AverageInitiative | 0.45–0.65 normalized | < 0.2 (stagnation) / > 0.85 (reckless overreach) |

### Balancing
- **Self:** Regeneration curves for vegetation and demographic caps prevent infinite growth.
- **Player:** Miracles and interventions shift alignment and initiative, allowing course corrections without micromanaging every villager.
- **System:** World events (storms, raids) periodically test stability to avoid stagnation.

---

## Scale & Scope

### Small Scale (Individual)
- Villager routines (needs, jobs, loyalty) fluctuate with local resource availability and village mood.

### Medium Scale (Village)
- Settlements adjust workforce allocation, storehouse usage, and band formation according to alignment goals.

### Large Scale (Population)
- Macro patterns reveal migration corridors, allegiance maps, and territory control without scripting.

---

## Time Dynamics

- **Short Term:** Daily cycles of harvesting, worship, morale updates, and rapid initiative jitters for chaotic entities reacting to events.  
- **Medium Term:** Seasonal shifts in resource regeneration and village ambition; lawful settlements adjust initiative slowly across these spans.  
- **Long Term:** Emergence of cultural identities, territorial borders, and persistent relations with the player.

---

### Day & Season Cadence
- **Clock Scale:** One in-game day lasts **8 real-time minutes** at normal speed. Simulation still runs on the project’s fixed-step cadence (e.g., 30–60 updates per second), but we anchor behavior to four daily milestones plus the broad day/night split.
- **Season Structure:** 10 days compose a season; 4 seasons complete a year. Seasonal transitions flip climate modifiers (temperature, precipitation, daylight curve) and trigger alignment-dependent rituals.
- **Daily Milestones:**
  - **Dawn (~minute 1):** Villagers wake, share breakfast, and prep tools. Peacekeepers hand off night patrols, bands break camp, nocturnal specialists wind down.
  - **Noon (~minute 4):** Midday heat check, morale pulse, and registry snapshot. Ideal for market trades, civic announcements, and initiative recalculations.
  - **Dusk (~minute 6):** Communal meals, worship gatherings, caravan departures/arrivals, and guard rotations.
  - **Midnight (~minute 8 / rollover):** Nightly morale decay, prayer queue aging, stealth activity checks, and ambush resolution before the clock loops.
- **Day vs Night:** Outside of the four anchors, the world sits in either **Daylight** (Dawn→Dusk) or **Nightfall** (Dusk→next Dawn). Systems that only care about lighting or predator pressure read this binary flag, while nuanced behaviors (worship bonuses, patrol swaps, trade windows) hook into the anchors.
- **Sleep Archetypes:**
  - **Diurnal Majority:** Sleep through Nightfall, needing roughly one real-time minute to recover fully.
  - **Nocturnal Specialists:** Active during Nightfall and Dawn (astronomers, smugglers, certain faith sects), resting between Dusk and Midnight.
  - **Polyphasic Light Sleepers:** Short naps staggered across Daylight, useful for scouts and couriers needing quick turnaround.
  - **Early Risers:** Retire shortly after Dusk, resume work pre-Dawn to extend productivity—common for agrarian/logistics outlooks.
- **Special Day Events:** Story calendar reserves signature days (market fairs, holy observances, eclipses). Each anchor time hosts distinct rituals—dawn vow renewals, noon market blessings, dusk ancestor feasts, midnight omen readings—that modify initiative, worship yield, or miracle costs for participating villages.
- **Night Safety:** Ambient threat level spikes during Nightfall. Peacekeepers, village lighting, watchtowers, and miracles raise deterrence; caravans without escorts risk ambushes near Midnight.
- **Band Camp Routine:** Armies halt marches at Dusk, pitch deterministic camps, and split forces into rest and patrol squads. Patrols loop camp perimeters on short intervals to pre-empt attacks while rest squads repair gear and recover stamina.
- **Outskirts Coverage:** Peacekeepers rotate three patrol patterns—inner ring (streets/storehouses) multiple times during Daylight, outer ring (fields/forest edge) once during Daylight and once during Nightfall, and deep-road patrols at least once per day unless crises demand heavier presence.


## Logistics & Risk Management

- **Caravan AI Managers:** Each village hosts a logistics coordinator entity that samples production vs consumption deltas every Noon milestone. Surplus nodes schedule outgoing caravans; deficits request imports. Caravans inherit priorities from outlook profiles (e.g., spiritual → food/incense; mercantile → luxuries; warlike → metal/arms).
- **Adaptive Routing:** Managers evaluate travel time, known threats, and alliance willingness. If shortages persist after two delivery cycles, the `StorehouseAlert` flag trips, escalating to diplomacy, miracle petitions, or—if alignment skews aggressive—bandit reprisals.
- **Risk/Reward Check:** Before departure, caravans compute `ThreatScore = (CaravanStrength - TargetStrengthEstimate) * AggregatePerception * WisdomFactor`, then bias the result by outlook/culture (reckless cultures discount danger; cautious ones inflate it). Negative scores trigger extra escorts, stealth routing, or mission aborts; positive scores raise payout multipliers and fame gains.
- **Telemetry Hooks:** Every run reports planned vs delivered cargo, threat scores, and incidents to `VillageSandboxSnapshot` and `VillageEventLogRegistry`, enabling UI overlays to highlight choke points and chronic shortages.


## Endgame Vision

- **Polycentric Civilization:** By late game, multiple villages federate into cultural blocs with shared rituals at each anchor time. Dawn oaths unify federations, noon markets span linked territories, dusk rituals cement diplomacy, and midnight omens steer grand strategy.
- **Miracle Economy:** Worship mana flows at continental scale; tribute tiers unlock mythic miracles that reshape seasons (perpetual harvests, eclipse shields) while demanding careful alignment management to avoid schisms.
- **Dynamic Frontiers:** Logistics networks crisscross vast biomes, defended by elite bands and peacekeeper legions. Threat spikes (elder beasts, rival gods) challenge caravan routes, forcing the player to orchestrate escorts, miracles, or strategic withdrawals.
- **Cultural Divergence:** Outlook-driven megaprojects emerge—scholarly sanctuaries researching alignment theory, expansionist citadels projecting influence, spiritual pilgrimage circuits chaining midnight omens into continent-wide prophecies.
- **Player Role:** The god-hand shifts from village babysitter to regional architect—balancing worship favor, sanctioning federations, responding to ritual omens, and deciding which cultures ascend or fade as the sandbox tends its own grand narrative.


---

## Failure Modes

- **Death Spiral:** Repeated disasters drive morale to zero → villagers disperse → settlements collapse; needs emergency aid levers.
- **Stagnation:** All villages plateau with high stores but no ambition; requires ambition triggers tied to alignment and world threats.
- **Runaway:** Single alignment dominates map; introduce faction friction or diminishing returns to keep diversity.
- **Emotional Collapse:** Chain traumas suppress initiative across a settlement (collective depression), slowing recovery unless countered by rituals or supportive miracles.

---

## Player Interaction

- **Observable:** Registry dashboards expose village stats, worship mana meters, alignment shifts, and initiative bands to explain behavioral tempo.  
- **Control Points:** Miracles, hand interactions, and targeted blessings/curses that spend or generate worship mana and can nudge initiative (calming lawfuls, stoking chaotic zeal).  
- **Learning Curve:**
  - Beginner: Watch villages thrive or falter without intervention.  
  - Intermediate: Time miracles to steer alignment and growth.  
  - Expert: Orchestrate multiple settlements, balancing fear and favor for optimal worship output.

---

## Systemic Interactions

### Dependencies
- **Villager Truth Sources:** `VillagerId`, `VillagerNeeds`, `VillagerMood`, `VillagerJob` for baseline behavior.  
- **Resource Loops:** `StorehouseInventory`, vegetation node components for economic inputs.

### Influences
- **Bands & Combat:** Alignment pushes band creation for defense or conquest.  
- **Miracles:** Acts as catalytic modifiers on morale and worship.  
- **Logistics:** Determines how surplus transfers between linked settlements.
- **Narrative Events:** Personal stories (marriages, deaths, exiles) propagate initiative modifiers through relationship graphs.

### Synergies
- Benevolent miracle streak + logistics network accelerates ascendant villages.  
- Fear-based play amplifies combat readiness at cost of worship diversity.  
- Alignments that prioritize scholarship or spirituality funnel surplus into worship mana, unlocking sustained miracle usage.
- High initiative stacked with expansionist outlooks rapidly consumes frontier resources; pairing with logistics is mandatory to avoid burnout.

---

## Configuration & Data Contracts

### Alignment & Initiative Assets
- **`VillageAlignmentCatalog.asset`** (ScriptableObject) owns the set of active axes. Each axis entry includes `AxisId` (`FixedString32Bytes`), the evaluation `ValueCurve` for tiering behavior, an `InitiativeResponse` block (min/max bias plus easing function), and `SurplusPriority` weights (build, defend, proselytize, research, migrate) that village planners consume when budgeting stockpiles.
- **`VillageOutlookProfile.asset`** captures cultural playbooks. Bakers emit a `VillageOutlookProfileComponent` singleton containing the `AxisBlend` values mapped to the alignment catalog, the `DefaultInitiativeBand` (slow, measured, bold, reckless) for seeding new settlements, a `GovernanceTemplate` reference used by leadership cycling, and a `WorshipModifier` curve that alters faith gain/decay.
- **`InitiativeBandTable.asset`** standardizes pacing. Each band stores a `BandId`, deterministic `TickBudget` (PureDOTS fixed steps between major actions), `RecoveryHalfLife` (tick-based falloff toward baseline), and `StressBreakpoints` that switch mindsets (panic, rally, frenzy) without breaking replay.
- Bakers convert these assets into Burst-friendly blobs:
  - `VillageAlignmentDefinitions` blob asset shared by the simulation world.  
  - `VillageOutlookLookup` buffer for constant-time axis/outlook queries during initiative budgeting.  
  - `InitiativeBandLookup` blob read by both villager and village initiative systems.

### Authoring Workflow
- Authoring scripts expose dropdowns for Axis IDs, outlook profiles, and initiative bands so designers configure villages without touching code.
- `VillageSpawnerAuthoring` stores default outlook + initiative selections; bakers apply them to spawned settlements ensuring consistent seeds.
- Balance passes happen by editing ScriptableObjects, preserving PureDOTS template reuse and minimizing code churn.

---

### Sample Data Defaults
- **Alignment Axes (initial `VillageAlignmentCatalog` entries):**
  - `OrderChaos` → ValueCurve centers at 0, plateauing toward ±100; `InitiativeResponse` shifts ±15% initiative gain for lawful (+) vs chaotic (-) when morale rises/falls; surplus weights favor `defend` (0.35) for lawful and `migrate` (0.25) for chaotic settlements.
  - `IsolationExpansion` → ValueCurve skews toward expansion once surplus > 120% of upkeep; initiative bias adds +20% project cadence for expansionist villages; surplus weights emphasize `build` outposts (0.30) and `research` logistics (0.20).
  - `DevotionSkepticism` → Controls worship momentum; believers grant +25% worship mana conversion, skeptics reduce by 15%; surplus weights split `proselytize` (0.35) vs `research` (0.25).
  - `MercyRetribution` → Drives disaster responses; retributive settlements push `raid` investments (0.30) when crises linger > 3 days; merciful settlements lean into `defend` (0.25) and `proselytize` (0.20).
- **Outlook Profiles (`VillageOutlookProfile.asset` samples):**
  - `Caretaker` → AxisBlend `{OrderChaos: +0.6, IsolationExpansion: -0.4, DevotionSkepticism: +0.7, MercyRetribution: +0.3}`; `DefaultInitiativeBand = Measured`; `GovernanceTemplate = Council`; `WorshipModifier` grants +10% faith gain, -5% decay.
  - `FrontierGuild` → AxisBlend `{OrderChaos: 0.0, IsolationExpansion: +0.8, DevotionSkepticism: +0.1, MercyRetribution: 0.0}`; `DefaultInitiativeBand = Bold`; governance uses `Meritocratic` template; worship modifier neutral.
  - `ZealousHost` → AxisBlend `{OrderChaos: +0.3, IsolationExpansion: +0.2, DevotionSkepticism: +0.9, MercyRetribution: +0.4}`; `DefaultInitiativeBand = Reckless`; governance `Theocratic`; worship modifier +35% gain, +15% decay to keep pressure high.
  - `ShadowMarket` → AxisBlend `{OrderChaos: -0.5, IsolationExpansion: -0.1, DevotionSkepticism: -0.6, MercyRetribution: -0.7}`; `DefaultInitiativeBand = Slow`; governance `Oligarchic`; worship modifier -20% gain, -20% decay (apathetic but steady).
- **Initiative Bands (`InitiativeBandTable.asset` draft):**
  - `Slow` → `TickBudget = 8`, `RecoveryHalfLife = 16`, `StressBreakpoints = {panic: -40, rally: +35}`.
  - `Measured` → `TickBudget = 5`, `RecoveryHalfLife = 12`, `StressBreakpoints = {panic: -35, rally: +40}`.
  - `Bold` → `TickBudget = 3`, `RecoveryHalfLife = 8`, `StressBreakpoints = {panic: -25, rally: +45, frenzy: +70}`.
  - `Reckless` → `TickBudget = 2`, `RecoveryHalfLife = 6`, `StressBreakpoints = {panic: -15, rally: +35, frenzy: +55}`.
- **Event Threshold Seeds:** drought alerts fire when rolling 7-day average moisture < 45% of biome norm; birth festivals when morale > 75 and surplus > 130%; raid escalations when crisis backlog > 3 unresolved entries or worship devotion dips below 30.

---

## Deterministic Event Catalog

### Event Families
- **Seasonal & Environmental:** Weather turns, harvest festivals, drought alarms kept in `EnvironmentEventStream` aligned with climate systems.
- **Social & Emotional:** Births, marriages, oaths, rivalries recorded in `VillageSocialEventBuffer` with deterministic seeds.
- **Crisis & Threat:** Raids, plagues, shortages queued into `VillageCrisisEventBuffer`; they drive initiative penalties and prayer surges.
- **Miracle & Worshipful:** Player miracles, omens, divine punishments mirrored in `WorshipTelemetryBuffer` so mana spend lines up with reactions.

### Storage & Replay Guarantees
- Each event struct packs `EventId`, `VillageEntity`, `TriggerTick`, `Seed`, `Magnitude`, and an `AffectedAxesMask` for alignment math.
- Buffers use ring allocation (default 32 entries per village) sized for rewind; no runtime allocations, deterministic over replay.
- Global `SandboxEventHistory` singleton aggregates pointers per tick so registries and UI queries read identical sequences post-rewind.

### Generation & Consumption
- Generators in `SandboxEventGenerationSystemGroup` evaluate thresholds once per fixed step, enqueue events when catalog criteria trigger.
- Consumers (initiative, worship, governance, logistics) read buffers sorted by `TriggerTick` then `EventId` to maintain cross-platform determinism.
- Events carry `TelemetryTags` enabling dashboards to chart morale, faith, logistics stresses without additional probes.

### Tooling
- Designers add definitions in `SandboxEventCatalog.asset`; inspectors enforce required fields per family.
- Event entries include visibility flags (`PlayerFacing`, `TelemetryOnly`, `SimulationOnly`) so the HUD stays focused while analytics still capture full detail.
- Regression tests replay scripted sequences under rewind to ensure initiative/outlook deltas reproduce exactly.

---

## System Group Integration

### Placement within PureDOTS
- **`SandboxVillageSetupSystemGroup`** (after `ResourceSystemGroup`) seeds alignment/outlook data when new settlements form.
- **`SandboxInitiativeSystemGroup`** (child of `GameplaySystemGroup`) processes initiative rolls against `InitiativeBandLookup`.
- **`SandboxEventGenerationSystemGroup`** (sibling to initiative) evaluates catalysts post-resource update, pre-worship.
- **`WorshipFeedbackSystemGroup`** ingests event buffers to mint mana/faith telemetry in the same tick.

### Hot/Cold Archetype Strategy
- `VillageCore` archetype keeps only hot components: `VillageId`, `VillageAlignmentState`, `VillageInitiativeState`, `VillageResourceSummary`.
- Presentation data (dashboards, VFX anchors) lives on companion `VillagePresentation` entities tagged with `PlaybackGuard` so rewind can rebuild visuals cleanly.
- Narrative metadata sits in `VillageNarrativeBuffer` on a cold archetype to avoid bloating simulation chunks.

### Scheduling Notes
- Initiative systems depend on `ResourceUpdateBarrier` and `MoraleUpdateBarrier`; barriers flush command buffers before initiative executes.
- Event consumers queue structural changes through `EndSimulationEntityCommandBufferSystem`, keeping Burst compatibility and deferring structural edits.
- Governance cycling executes late inside `SandboxInitiativeSystemGroup`, observing resolved events before leadership switches.

### Registry Hooks
- `SandboxRegistryBridgeSystem` mirrors alignment/initiative state into shared registries for dashboards.
- Worship/crisis events push `RegistryEventLog` entries, keeping observability pillars aligned with PureDOTS tooling.

---

## Registry Telemetry

### Core Village Record
- Registry entry `VillageSandboxSnapshot` keeps per-village rows (`RegistryVillageId`) with:
  - Alignment axis scores (normalized -100…+100) plus dominant outlook label.
  - Current initiative band, tick countdown until next project window, and stress status (calm, rally, frenzy).
  - Resource summary (food, construction, specialty) vs upkeep targets, highlighting deficits (<110%) and surpluses (>140%).
  - Worship telemetry: faith average, devotion split (fear/respect/love), mana generation rate, outstanding prayer count.
  - Governance flags (current leader archetype, legitimacy %, succession timer).
- Snapshot updates once per fixed step in `SandboxRegistryBridgeSystem`, writing to a ring buffer `VillageSandboxSnapshotBuffer` for UI polling.

### Event & Catalyst Stream
- `VillageEventLogRegistry` mirrors entries from `SandboxEventHistory`, exposing `EventId`, `VillageId`, `Tick`, `Family`, `Magnitude`, and `TelemetryTags` for dashboards.
- HUD queries filter by `PlayerFacing` flag; analytics ingest the full stream (including simulation-only events) for trend charts.
- Crisis severity aggregates into `VillageCrisisTelemetry` (rolling sum weighted by magnitude) so designers spot simmering issues.

### Prayer & Tribute Metrics
- Registry keeps `VillagePrayerQueueState` summarizing total petitions, urgency buckets, and answered/expired counts per deity.
- Tribute tiers map to miracle families via registry config (`MiracleUnlockRegistry`), enabling UI to highlight upcoming unlocks when worship mana crosses thresholds (e.g., Tier 2 at 2,500 lifetime tribute).
- Faith decay spikes or fulfilled prayer streaks append `WorshipEventTelemetry` entries, giving analytics context for sudden mana swings.

### Consumption Pattern
- Editor dashboards and in-game inspectors read registry buffers through existing Godgame registry UI bindings; telemetry also exported to logs during automated tests for validation.
- Rewind safety: registries rebuild from authoritative buffers each frame rather than storing mutable state, aligning with PureDOTS observability expectations.

---

## Leadership & Governance Patterns

### Founding & Alliances
- Villagers identify compatible outlook/alignment peers, form proto-bands, and choose settlement sites near viable resource triads (food, construction material, specialty node) within a configurable radius.
- Logistics or mercantile outlooks widen acceptable travel distance; isolationist outlooks favor defensible terrain even if resource density is lower.

### Elite Selection Rules (Examples)
- **Authoritarian / Lawful Villages:** Titles inherited along bloodlines when heirs meet minimum metrics (fame, fortune, loyalty).
- **Balanced / Neutral Villages:** Leadership determined via mixed system—coin flip between inheritance and electoral vote when vacancies appear.
- **Popular Mandate Cultures:** Elections favor candidates with highest fame/popularity scores; corrupt candidates can sway rolls via demagoguery modifiers, while pure candidates gain honesty bonuses.
- **Warlike Societies:** Vote or acclaim veteran warriors with high glory; corrupt warleaders may seize power if intimidation exceeds threshold.
- **Materialistic Communities:** Prioritize wealth/fame champions brandishing prosperity; tie-breakers reference trade success.
- **Spiritual Cultures:** Elevate high-faith individuals aligned with dominant belief (player/AI deity preference).
- **Xenophilic Settlements:** Apply race/species weighting encouraging diverse leadership; offsets combine with other outlooks to avoid monolithic picks.
- **Xenophobic Settlements:** Strong bias toward in-group lineage, resisting external candidates unless other outlooks overwhelm preference.

### Election Cadence & Eligibility
- Governance checks run on a timer (e.g., seasonal/annual) but also trigger when candidates surpass outlook-aligned thresholds (average fame/fortune/glory above village mean, research breakthroughs, major victories).
- Fanatic outlooks tighten eligibility windows (e.g., fanatic warlike requires legendary glory before candidacy); relaxed cultures accept broader participation.

These rules feed directly into aggregate outlook calculations and impact derived stats (wisdom, initiative, diplomacy) depending on the selected ruling cohort.

---

## Autonomous AI Behaviors

### Band & Army Formation
- **Chaotic Settlements:** Bands form opportunistically—multiple small war parties emerge based on personal initiative spikes. Patriotic sentiment is volatile; high patriotism can rally ad-hoc coalitions despite chaos.
- **Lawful Settlements:** Conscription flows through a single organized army. Auxiliary bands spin off for support roles (scouting, supply) but report back to the main force. Recruitment logic favors streamlined rosters and clear command structure.
- **Neutral Settlements:** Blend of the two—core army with periodic independent bands depending on situational stress.

### Aggressiveness & Task Handling
- **Outlook-Driven:** Aggregate outlook/alignment (warlike, defensive, expansionist) dictates aggression baseline. Individual tasks (defend, patrol, raid) refine behavior.
- **Aggressive Orders:** Chaotic warlike armies assigned to defend will still hunt enemies beyond village bounds, leaving smaller detachments to guard home. Lawful armies hold defensive positions, dispatching calculated offensive bands. Neutral forces adapt, mixing pursuit and defense.
- **Pursuit Radius:** Configurable per outlook; chaotic entities chase farther, lawful stay within influence radius, neutral splits difference.

### Patriotism & Loyalty
- Introduce a `Patriotism` stat per entity measuring attachment to current settlement/band. Influences willingness to answer conscription, stay on station, or defect. Lawful cultures foster stable patriotism; chaotic ones oscillate with recent events. Inputs include time spent in the aggregate, number of family members residing there, personal assets invested locally, village advancement tier, resource security, and alignment/outlook alignment between individual and leadership.
- 100% patriotism yields absolute loyalty (willingness to die for the aggregate), while near 0% triggers migration/desertion even before trouble arrives. Miracles indirectly support patriotism by sustaining prosperity but do not directly modify the stat.
- Matching outlooks/alignments accelerate patriotism gain; mismatches dampen it. Victories, supportive leadership acts, and shared assets push patriotism up, while defeats, social disparity (for egalitarians), or leadership abuses drag it down.

### Military Rank Progression & Honor
- **Dual Ladders:** Armies maintain two parallel rank tracks. **Enlisted** begin as `Recruit` → `Private` → `Legionary` → `Veteran Legionary` → `Centurion` → `Champion`. **Elite Cadres** (nobles, officer-school graduates, divine champions) enter directly at `Lieutenant` → `Captain` → `Commander` → `Marshal`. Crossovers are possible but require exceptional honor scores and political approval.
- **Honor & Glory Ledger:** Every combat action logs `HonorPoints`, `Glory`, and `Reputation` into a per-entity ledger. Enlisted units earn multiplier bonuses for climbing from humble origins (e.g., `Private` → `Veteran Legionary` grants +25% honor weighting), whereas elite officers receive smaller multipliers to reflect inherited status. Surpassing honor thresholds triggers promotion candidacy checks that weigh ledger totals, patriotism, outlook alignment, and command aptitude traits (Leadership, Tactics, Resolve).
- **Promotion Ceremonies:** Successful promotions fire village-wide events, adjusting fame/glory for the individual and the host settlement. Enlisted promotions generate larger public mood swings—villagers celebrate underdog ascents, boosting morale and worship mana among compatible outlooks. Elites still gain prestige, but failure to accumulate glory risks ridicule or demotion if honor stagnates.
- **Unlock Potential:** Rank ups unlock talent trees (battle formations, aura buffs, tactical miracles) with deeper branches available to those who rose from enlisted origins. Veteran legionaries can ascend to `Centurion` or even officer track if their honor ledger surpasses elite peers, creating emergent hero narratives. Elite officers who underperform may stagnate, losing command slots to decorated veterans in cultures that prize merit over lineage.
- **Alignment & Outlook Bias:** Lawful/martial cultures prioritize adherence to doctrine and clean battle ledgers; chaotic warlike factions embrace audacious feats and collateral tolerance. Spiritual outlooks demand miracles or righteous conduct during battles to award promotions. Corrupt/evil regimes may sell commissions, introducing black-market paths that undermine honor but amplify infamy.
- **Telemetry Hooks:** Honor ledgers feed `MilitaryRank`, `HonorLedger`, and `PromotionHistory` truth sources so registries can surface active commanders, promotion queues, and cultural sentiment toward the army. Cataclysmic spell usage by ranking officers annotates ledgers with either heroic or infamous tags, influencing future diplomacy and prayer tones toward the commanding deity.

### Resource & Policy Conflicts
- Settlement aggregate outlook/alignments set strategic direction (trade vs hoard, offense vs defense). Individuals act according to personal ideals when managing personal property, creating micro-conflicts resolved via patriotism, leadership mandates, or faction negotiation systems.
- **Spatial Expansion:** Lawful settlements grow in orderly districts; chaotic ones sprawl more loosely. If space saturates, villages build vertically or redevelop (demolish/replace) older structures to accommodate growth. Outlook influences district emphasis (industry, worship, housing).
- **Aesthetic Morphs:** Alignment/outlook combinations reshape architecture—lawful good produces orderly gardens and luminous temples; chaotic evil favors jagged fortifications; materialistic flaunt bustling markets; spiritual villages grow shrine complexes. Higher tech tiers add lighting, statues, and banners echoing dominant ideologies.

### Miracle Reactions
- **Evasion:** Upon detecting incoming miracles (any type), villagers execute dodge routines first.
- **Post-Effect Response:** After impact, reactions depend on outcome—beneficial miracles trigger celebration/tribute, harmful ones cause fear, evacuation, or retaliation planning based on loyalty alignment.

### Inter-Village Diplomacy
- **Compatible Outlooks/Alignments:** Drive formal alliances, shared defence pacts, and political marriages between elites/rulers. Sustained cooperation can lead to full consolidation (federations, empires) with merged governance and pooled resources.
- **Conflicting Outlooks:** Villages still consider cooperation/trade unless alignments are directly opposed; hostility escalates only when ideological tension passes threshold or patriotism collapses.
- **Shared Deity:** Villages worshipping the same god suppress hostilities by default; conflict requires extreme opposition (e.g., divergent fanatic alignments). Shared worship also encourages joint prayer rituals and cooperative miracle funding.
- **Unification Process:** Consolidation enters an integration period merging elites, assets, and governance. Outlooks average across member villages; armies remain distinct but coordinate under a unified AI directive. Tech tiers converge gradually—logistics throttles integration unless settlements physically merge. Miracles during timed windows are interpreted as omens; for example, besieging armies struck by offensive miracles from their patron deity may desert or refuse orders based on collective belief and outlook alignment.
- **Multi-Deity Dynamics:** Villagers with belief in multiple gods split worship mana according to belief ratios. Rival deities can contest influence by investing mana via competing miracles; outcomes resolve on mana investment and village loyalty/faith—no formal divine diplomacy exists.
- **Deity Conversion:** Switching patron deities sparks upheaval—corrupt/evil rulers enforce conversion or executions, while neutral/good avoid lethal measures. Opposing worship structures are sacked/razed; champions/heroes receive no special treatment. Legacy prayers fade over time as allegiance shifts.

### Morale, Mood & Breakdown States
- **Individual Breakdown Spectrum:** Mood/morale collapse triggers behaviors ranging from benign (work binges, silent withdrawal, stoic endurance) to chaotic (random attacks, vandalism, dramatic despair proclamations). Expression depends on personal outlook/alignment.
- **Collective Unrest:** Like-minded groups with low morale coordinate strikes, riots, or coups. Resolution hinges on leadership concessions, miracle interventions (e.g., Joy), or security crackdowns.
- **Recovery & Migration:** Time, compassionate leadership, resource surpluses, or targeted miracles reduce breakdown frequency. Persistent despair erodes patriotism and pushes migration/self-exile.
- **Migration Destinations:** Low-patriotism villagers roll outcomes based on outlook/alignment—most seek nearby compatible settlements (matching outlooks or shared deity), some wander until welcomed, others join roaming bands/bandits if no sanctuary exists.
- **Bandit Evolution:** Chaotic deserters can coalesce into bandit bands, establish hideouts, engage in piracy on trade routes, or even seed chaotic villages if they seize resources. Neighboring settlements deploy patrols, bounties, or suppression campaigns tailored to their outlook/alignment to neutralize or negotiate with outlaw groups.
- **Disposition Modifiers:** Stoic personalities dampen initiative swings; vengeful characters spike initiative after personal harm; empathic villagers lose initiative when allies suffer. These traits interact with outlook/alignment to shape post-event behavior.
- **Lifecycle & Families:** Outlook/alignment drive courtship, marriage, and family formation. Pairs auto-match via compatible ideals, miracles, or festival rituals. Offspring inherit traits (education potential, outlook bias) and progress through the education pipeline; adults age, retire, or die. Family bonds feed patriotism and prayer priorities.
- **Justice & Crime:** Outlooks define crimes (materialists focus on theft/sabotage, spiritualists on heresy); alignments dictate punishment severity. Lawful settlements run jails and due process; chaotic ones use slave pens or pits. Pure villages favor exile/rehabilitation, corrupt/evil sacrifice offenders, good lawfuls may extend asylum to migrants. Punishments can shift alignment/patriotism—tyrannical actions fuel unrest, just rulings raise loyalty.
- **Sieges & Combat:** Patriotism determines whether villagers rally, flee, or defect. Buildings take damage/ignite; peacekeepers (non-discipline safety role) handle firefighting and hazard mitigation. Conquered settlements may be razed, vassalized, plundered, liberated, or sacrificed per occupier outlook/alignment. Miracles during battle (e.g., Joy → blood frenzy, Despair → dread) apply area buffs/debuffs equally to all caught inside, altering morale and desertion odds. Post-battle miracles follow normal rules.
- **Peacekeepers:** Dedicated internal security force patrolling borders, keeping fauna at bay, responding to fires, escorting caravans, and enforcing curfews. Good/pure peacekeepers assist villagers; corrupt/evil accept bribes. Patriotism determines willingness to die saving others. They gain combat and utility skills over time, benefit from tech upgrades, and coordinate with bands/armies during sieges while prioritizing civilian and asset protection.

### Everyday Villager Loop
- **Tick Phases:** Each PureDOTS fixed step runs a compact decision tree: (1) urgent needs, (2) assigned jobs, (3) communal obligations, (4) leisure. Priorities resolve using initiative plus outlook modifiers; unmet stages roll urgency forward to the next tick.
- **Needs & Consumption:** Hunger, fatigue, hygiene, and mood each maintain buffers. When thresholds breach, villagers pivot to procure food, rest, bathe, or seek comfort. Consumption pulls from local stores; shortages trigger crisis modes (scavenge, request aid, pray) before morale penalties escalate.
- **Rest & Sleep:** Circadian preferences (diurnal/nocturnal) schedule rest blocks. Villagers reserve bunks/homes; overcrowding forces shared shifts or outdoor sleep, reducing recovery and increasing illness risk. Miracles or tech can stretch wakefulness but add fatigue debt tracked in telemetry.
- **Eating Cadence:** Baseline expectation is two meals per day plus optional snack. Meal tier depends on resource abundance and outlook (luxury feasts for spiritual zealots, efficient rations for mercantile pragmatists). Communal dining windows double as social nodes that boost patriotism.

### Socialization & Leisure
- **Scheduled Gatherings:** Work shifts include built-in social pulses (market gossip, workshop banter, shrine conversations). Outlook biases interaction topics; successful exchanges share morale, drift alignment, and queue cooperative tasks.
- **Festivals & Rituals:** Calendar events (weekly worship, seasonal fairs, hero vigils) transition villages into leisure state. Participation resets social debt meters, awards morale, and feeds worship mana calculators.
- **Downtime Activities:** When obligations are satisfied and initiative dips below threshold, villagers sample leisure lists—music, sparring, storytelling, crafting hobbies, meditation. Leisure choices emit mood modifiers and relationship gains to relationship buffers.
- **Idle & Wander States:** If no jobs or leisure remain, villagers roam safe paths, observe infrastructure, or offer spontaneous assistance requests. Wander data updates spatial heatmaps, keeping settlements lively and exposing dormant regions for events.

### Relationships, Courtship & Family Growth
- **Affinity Tracking:** Relationship buffers record shared work, aid, and leisure. Positive affinity grants teamwork bonuses; negative affinity seeds rivalries that sap initiative or trigger duels (per outlook tolerance).
- **Courtship Flow:** Eligible villagers (age band, status, compatibility thresholds) enter courtship mini-cycles during leisure or festivals. Success forms households that cohabit, pool resources, and synchronize initiative plans.
- **Breeding Logic:** Households evaluate housing availability, food surplus, healthcare capacity, and outlook fertility bias before pregnancies proceed. Gestation taps medical services; births raise celebration events, adjust resource rations, and spawn child entities inheriting blended outlook seeds.
- **Child Rearing & Education:** Families schedule caretaking shifts; education systems assign tutors by outlook (scholarly, martial, spiritual). Childhood phases (toddler, youth, apprentice) unlock age-appropriate chores and training, influencing future job pools. Neglect increases migration likelihood at adulthood.

### Personal Maintenance & Relaxation
- **Hygiene & Health:** Bathhouses, rivers, and herbal clinics maintain hygiene; neglect raises disease risk and emits warning telemetry. Healers schedule rest days, distribute remedies, or petition miracles for severe cases.
- **Spiritual Practice:** Devout villagers insert prayer/meditation micro-blocks multiple times per day; skeptics only pray when morale collapses. Practice increases faith and worship mana contributions per telemetry band.
- **Mental Reset:** After high-stress duties (combat, crisis response) villagers auto-book decompression—quiet reflection, communal storytelling, guided therapy—dependent on outlook. Skipped resets escalate breakdown risk and reduce initiative.

### Mood, Modifiers & Wellbeing
- **Mood Bands:** Mood values range from -100 to +100 with tiers inspired by RimWorld—Despair (< -60), Unhappy (-60 to -20), Stable (-20 to +20), Cheerful (+20 to +60), Elated (> +60). Mood recalculates at Dawn and Midnight, blending immediate events with lingering memories.
- **Band Effects:** Despair: initiative -40%, breakdown chance, health decay; Unhappy: work speed -15%, social friction; Stable: neutral; Cheerful: work +10%, social buffs, faith gain +5%; Elated: initiative +25%, chance to inspire allies, but burnout checks ramp if unmet needs persist.
- **Modifier Sources:** Needs (food, rest, hygiene), environment (beauty, weather, lighting), relationships, leadership edicts, rituals, miracles, injuries, augments, climate adversity. Modifiers include magnitude, duration, stacking rules, and decay half-life; repetitive events apply diminishing returns to prevent farming.
- **Outlook Bias:** Outlook/alignment profiles remap modifiers (stoic halves negatives, hedonist amplifies luxury positives, ascetic dampens comfort bonuses). Cultural norms can invert certain modifiers (taboo miracles causing double penalties, martyr cultures gaining mood from austerity).
- **Memories & Narrative Hooks:** Major events create `MoodMemory` entries with 1–10 day decay half-lives and tags (trauma, triumph, romance). Memories inform narrative/dialogue systems and diplomacy checks, allowing long-term consequences for miracles or disasters.
- **Telemetry:** Registry snapshots expose average mood, band distribution, top modifiers, and burnout risk so designers and players spot simmering crises.

### Body Health, Limbs & Augments
- **Anatomy Graph:** Entities use a hierarchical body map (torso → organs/limbs → subparts). Each part tracks `Integrity` and `Efficiency`; DOTS systems aggregate these into stats (Manipulation, Mobility, Perception, Stamina, Cognition).
- **Damage States:** Parts progress through Bruised → Wounded → Crippled → Destroyed thresholds. Destroyed vital organs trigger death unless stasis miracles intervene; destroyed limbs zero related efficiency until replaced.
- **Efficiency Examples:** Arms boost manipulation (craft speed/carry weight), legs drive mobility (travel speed, dodge), eyes/ears feed perception (threat detection, scout radius), heart/lungs govern stamina regen, brain/head controls wisdom and miracle channeling.
- **Medical & Prosthetic Workflow:** Clinics schedule surgeries at Noon milestone. Prosthetic tiers: crude (wood/bone), refined (metal/alloy), mythic (miracle-grown). Augments carry cultural tags (sacred, taboo, arcane) that inform mood modifiers and diplomacy reactions.
- **Pain & Mood Hooks:** Injuries apply pain modifiers proportional to nerve density; augments can supply mood buffs (blessed limb +15) or penalties (taboo graft -10 for conservative villagers). Healing or successful surgery removes negatives; chronic wounds spawn recurring memories until resolved.
- **Regrowth & Miracles:** Late-game biotech or miracles regenerate limbs/organs, restoring efficiency but possibly shifting alignment (tech-heavy regrowth nudges Order/Progress outlooks).
- **Medical Roles:** Apothecaries and doctors (non-disciplined civilian specialists) manage diagnostics, surgeries, and augment installations. They triage cases flagged by logistics managers or family petitions and will travel if contracted—or coerced in chaotic cultures.
- **Cultural Attitudes:** Augments unlock at higher tech tiers. Spiritualist societies shun them (mood penalties, social stigma); materialists celebrate them—elites are expected to enhance, and extremist factions ostracize the unaugmented. Arcane cultures prefer summoned/sacrificed organ replacements tuned to mage traditions.
- **Augment Spectrum:** Prosthetics (baseline), bionics (enhanced), and specialized super-organs (grown, summoned, or artificed) each demand specific tech levels, reagents, and rituals. All augments carry **quality** (craftsmanship), **rarity** (drop frequency), **tier** (tech level), and **manufacturer signature** granting unique traits (e.g., Matron Forge limbs resist corrosion; Whisper Loom organs boost stealth). Alignments dictate availability (Order favors engineered bionics; Chaos leans toward living mutations).
- **Illness & Treatment Gating:** Diseases have tech prerequisites (herbal, surgical, biotech). Villagers may undertake long journeys to advanced clinics; immobile elders rely on kin who learn techniques or, in desperate/chaotic cases, kidnap qualified doctors. Pure/good cultures offer payment before considering coercion.
- **Resource Quests:** Certain surgeries require rare plants, animals, or relics, triggering quests for bands or brave family members to procure or purchase materials. Success grants mood boosts, prestige, and potential alignment drift toward cooperative outlooks.
- **Telemetry & AI:** Registry snapshots log manipulation/mobility efficiency, augment adoption rates, medical queue depth, and outstanding medical quests; AI schedulers reassign jobs based on efficiency and coordinate medical and logistics trips accordingly.

### Mage Guilds & Holy Orders
- **Tiered Orders:** Mystic institutions mirror tech tiers. Advancing from Initiate to Archon unlocks higher-rank spells, rituals, and passive blessings. Alignment/outlook gates progression (lawful zealous orders emphasize protective miracles; chaotic arcane cabals push destructive sorcery).
- **Village Relationship:** Orders cooperate with host villages on logistics and defense but reserve autonomy. If leadership strays from order values, they may quietly aid loyalist rebellions or sponsor coups they deem just.
- **Hero Training:** Guilds/Orders can sponsor chosen heroes, teaching celestial-grade abilities needed to confront demons, angels, and world bosses. Training consumes worship mana or rare reagents and records in the registry for player oversight.
- **School Diversity:** Spellcasting flavors reflect cultural leanings—wizards, warlocks, sorcerers, psychics, thaumaturges, hydromancers, pyromancers, geomancers, necromancers, etc. Each school defines core abilities, passives, and alignment affinities. Villages adopt schools aligned with their outlook; taboo schools incur mood penalties and legal sanctions.
- **Knowledge Transmission:** Spells propagate through mentorship. Casters must learn from existing practitioners, recovered scrolls, or self-discovery. Magic can be lost; abandoned knowledge respawns as relic scrolls that explorers analyze and reintroduce via guild libraries.
- **Guild Sabbaths:** Members owe at least two festival days per year to their order. Sabbaths are knowledge exchange events where spells are taught, new rituals debated, and apprentices assessed. Attendance grants mood bonuses and unlock chances for rare spells.
- **Forbidden Lore:** Guild masters can sequester dangerous spells in vaults. Forbidden tomes can be stolen, sold, or weaponized—sparking quests for thieves, inquisitors, or rival orders.
- **Quest Hooks:** Clients frequently commission covert runs to obtain secret spells (e.g., warlords craving earthquake sigils, corrupt priests pursuing blood rites). Outcomes reshape alignment: noble aims may sour into villainy, while successful thefts can ignite inter-order conflicts or miraculous arms races.
- **Ideological Wars:** Orders may declare war over forbidden or mandated magics—orthodox crusades to purge heresy, or cynical masters staging conflicts as false casus belli to seize rival libraries. Victory conditions include burning guild towers, stealing grimoires, or enforcing doctrinal edicts. Holy orders escalate to crusades/jihads when outlooks conflict (e.g., blood rites vs sun worship).
- **Knowledge Propagation:** Like manufacturers, guild knowledge can be reverse-engineered. Explorers uncover ruined libraries, salvaging spell scrolls and training manuals. Analysis checks gate how much of an opposing school’s spellbook can be absorbed.
- **Spell Mastery Loop:** Learning tracks five gated states—**Knowledge** (observation, scroll study), **Practice** (drills under mentor supervision), **Casting** (reliable field use), **Mastery** (adept/expert/master ranks), and **Transcendence** (paragon signature). Each state accrues discrete XP pools (`KnowledgeXP`, `PracticeXP`, `CastingXP`) with decay timers if neglected. Attributes gate progression (Intellect/Wisdom for knowledge, Discipline/Focus for practice, Faith/Resolve for mastery) and require access to appropriate facilities (schools, sanctums, battlefields). Mastery tiers reduce mana costs, unlock modifiers (e.g., spread angles, damage types), and add fail-state dampening (misfires become backlashes rather than catastrophes).
- **Practice & Innovation Windows:** Apprentices can schedule `PracticeSlots` on guild calendars; missing slots slows advancement and can trigger rivalries. Periodic `Guild Symposiums` accelerate learning—attendees gain temporary XP multipliers and discover mentor-specific techniques. Practice tasks can be codified as jobs for DOTS scheduling, letting villages allocate time budgets for magical upkeep without derailing economic loops.
- **Spell Fusion & Innovation:** Masters can fuse known spells (e.g., Frost Bolt + Fireball → Frostfire Bomb) at combined resource cost plus aptitude modifiers. Successful fusion creates new spell blueprints shared within allied guilds. Fusion rolls use weighted averages of component mastery ranks, charisma (for collaboration), and outlook compatibility; failure risks backlash injuries or temporary mana channel burnout. Rare polymaths found entirely new schools, becoming guild founders if charisma/aptitude criteria are met. Newly born schools spawn provisional doctrines (`SchoolManifesto` assets) that describe ethos, signature spells, and alignment drift; communities decide whether to ratify or outlaw them.
- **Cataclysmic Combos:** Mastery across multiple schools unlocks catastrophic spells (Flame Tornado, Lava Fissure, Comet Shower). These demand massive mana, collect multi-day **Attunement Charges**, and enforce regional cooldowns (weeks of in-game time). Casting near populated villages automatically queues diplomacy checks: benevolent cultures demand reparations, neutral ones open tribunals, evil-aligned or vengeful societies cheer punitive strikes. Collateral damage updates infamy/fame ledgers per alignment axis, feeds future prayer tone (pleading, fearful, celebratory), and can toggle wartime states if casualties cross configured thresholds.
- **School Genesis & Guild Leadership:** To formalize a new school, at least three paragon-rank casters must ratify a manifesto, build a dedicated sanctum, and survive a `Founding Trial` scenario (ritual duel, research breakthrough, or communal miracle). Success establishes a new guild tier branch with bespoke progression perks; failure scatters research, spawning relics that other factions can salvage. Governance titles (Archon, Magister, Hierophant) rotate via alignment-weighted elections; charisma, fame, and doctrine fidelity carry voting weight. Guild masters can defect to hostile villages, sparking splinter factions and schisms that ripple through diplomacy networks.
- **Diplomatic & Reputation Consequences:** Spell innovators gain fame/reputation/glory; villages with compatible outlooks celebrate them, while opposing cultures may brand them heretics. Reputation feeds into trade terms, miracle petition tone, and access to relic markets. Cataclysmic misuse generates infamy markers tracked by registries; repeated atrocities invite inquisitions, divine sanctions, or band coalitions seeking justice. Conversely, lifesaving miracles grant renown buffs, unlocking cross-faction apprenticeships and honorary guild memberships for allied cultures.
- **Telemetry & Truth Source Hooks:** Spell progression writes to `CasterProgression` buffers (per entity) and `GuildChronicle` logs (per institution). Fusion events emit analytics packets noting component spells, outcome rarity, casualties, and reputation deltas. Cataclysmic casts flag `GlobalCooldown` entries so AI planners respect aftermath recovery. These feeds ensure PureDOTS systems can budget mana economy, AI threat responses, and diplomatic shifts deterministically.

### Manufacturer Specialization
- **Craft Signatures:** Blacksmiths, artificers, alchemists, and arcane ateliers develop personal manufacturer tags. Two makers can produce the same base template yet ship wildly different stats—attack rate, base damage, crit windows, durability, elemental infusions—based on their specialty curves.
- **Experience Growth:** Manufacturers gain mastery XP from completed commissions and festival showcases. As mastery rises, their unique traits scale (e.g., Emberline smiths add +burn stacks; Silkmire fletchers refine homing arrows). Legendary-tier items inherit multiple amplified traits, making high-level craftsmen global celebrities.
- **Regional Differentiation:** Biome resources and outlook biases steer specializations (desert cities craft heat-hardened blades; spiritual enclaves weave miracle-conductive vestments). Trading networks compete to secure favored makers.
- **Evolving Catalogs:** Master craftsmen periodically unlock new blueprints or mutate existing ones, creating limited-run models. Collectors and militias issue quests to commission or retrieve these sets, feeding endgame economy and diplomacy.
- **Knowledge Exchange:** Techniques follow the same mentorship rules as magic—apprentices learn directly, or through recovered schematics. Analysis checks (intelligence, wisdom, education) govern how much of a rival maker’s specialization can be reverse-engineered.
- **Ruined Workshops:** Razed towns leave behind production ruins containing schematics and efficiency data. Adventurers can explore ruins for RNG-based loot rolls that unlock tech tiers or specialty traits when returned to active workshops.

### Micro-AI Interfaces
- **Command Integration:** Player miracles, hand gestures, and global edicts inject temporary state overrides (calmed, inspired, terrified). Overrides enqueue via command buffers consumed next tick to preserve determinism.
- **Telemetry Feed:** Short-term behaviors emit lightweight signals (`NeedsSatisfied`, `RestDebt`, `LeisureType`, `SocialPulse`) that registries aggregate for tooltips/analytics without per-frame storage.
- **Fallback Behavior:** When villages cannot meet needs (no food, no shelter, no allies), villagers escalate to pilgrimage/migration routines, spam prayer queues, or—under desperate alignments—embrace banditry, creating visible crises the player can intercept.
- **Festivals & Rituals:** Every ~3 days villages run festivals/rituals aligned with their outlooks (production feasts, fertility rites, warrior games, devotional ceremonies). Events grant temporary buffs (e.g., productivity, harvest yield, army morale) and can be amplified with Joy or denounced with Despair, nudging outlook sentiment.
- **Disasters & Environmental Events:** Natural phenomena (storms, rain clouds, earthquakes) are physical entities gods can pick up/throw. Plagues emerge from environmental conditions. Disasters spark prayer surges, evacuations, and outlook-specific responses (spiritual rituals, material reinforcement). Deities can avert or weaponize events (e.g., fling plagued villagers into enemy villages) with alignment consequences.
- **Fauna & Monsters:** Each biome spawns wildlife with roaming/migration patterns. Outlooks determine responses—hunt, domesticate, worship as totems. Dangerous beasts near settlements trigger peacekeeper hunts, hunter guilds, or cults. Deities can bless/curse fauna via miracles.
- **Champions:** Deity-selected individuals blessed to spread belief via deeds. Champions gain boosted stats/XP scaling with their god's tribute tier, can cast delegated miracles by drawing from the god's mana pool, and pursue goals set by their patron. Mortals treat them as notable villagers, but champions are the sole agents capable of harming celestial beings (demons, angels, otherworld entities). Champions may form bands to tackle world bosses.
- **Heroes:** Fame/popularity/glory-based elites championing their home village. Heroes carry local gravitas, often joining the ruling elite. By default they are mortal-scale, but gods can delegate celestial damage via relics or direct empowerment when crises demand, enabling them to confront otherworldly threats.
- **World Bosses:** Unique entities spawning at scheduled times/locations. Adventurer bands (often led by champions/heroes) can defeat them for rare loot and god jewelry. Deities may nurture bosses to gain loyalty, granting bonuses or direct control. Some bosses aid villages (welcomed by peaceful/xenophilic cultures); others are feral threats ignoring boundaries.
  - **Ascension Origins:** Villagers can transform into celestial or worldly threats through special ascension paths (singular or collective transformations). These emergent world bosses retain their mortal identity and carry narrative weight tied to their origin story.
  - **World Reshaping:** World bosses achieving total victory (80%+ territory control) can trigger **apocalypse transformations**, reshaping the entire world in their image (demon hellscapes, undead wastelands, frozen eternities, etc.). Each apocalypse spawns faction-specific civilizations and triggers **counter-apocalypse events** (angelic slayers, life priests, fire titans) creating never-ending emergent narrative cycles. See `Docs/Concepts/World/Apocalypse_And_Rebirth_Cycles.md` for full system.
- **Celestial Beings:** Demons, angels, and other entities enter the world randomly (events, rituals, major wars). They can terrorize or aid villages, potentially escalating into world bosses. Narrative arcs leverage their appearances; champions (or empowered heroes) are required to oppose hostile arrivals.

### Villager Ascension Paths

**Fallen Star Demon (Singular Transformation):**
When a god throws a villager meeting specific criteria, they may undergo catastrophic transformation into a rampaging demon. The transformation triggers through divine violence combined with the villager's inherent traits and circumstances.

**Transformation Criteria:**
- **Core Requirements:**
  - Chaotic alignment
  - Warlike outlook
  - High belief/faith in the throwing deity (divine connection amplifies corruption)
- **Physical Weighting:** Physique stat heavily weights RNG probability—stronger villagers channel more divine energy during flight, increasing transformation odds
- **Contextual Multipliers:**
  - **Battle Context:** Plucking a villager from active combat (mid-battle throw) significantly increases transformation chance; warrior adrenaline and bloodlust prime them for demonhood
  - **Distance Thrown:** Farther throws accumulate more divine corruption during flight
  - **Velocity/Impact Force:** High-speed impacts concentrate transformative energy

**Transformation Sequence:**
1. **Falling Star Phase:** Villager trails divine fire/corruption VFX during flight; trajectory marks the sky
2. **Delayed Emergence:** Upon impact, villager vanishes into a crater. **One year of game time passes** before re-emergence
3. **Meteor Return:** After the year delay, a **meteor strike** occurs at or near the impact site, heralding the demon's birth
4. **Demonhood:** Entity emerges as a **Rampaging Demon** celestial being, **retaining the villager's original name** (e.g., "Korgath the Fallen")
5. **Memory & Vengeance:** The demon remembers the deity who threw them and may specifically target that god's villages, creating a self-inflicted nemesis

**Probability Formula (Tunable):**
```
Base Chance = 1%
Physique Multiplier = (Physique / 100)²  // Squared for dramatic scaling
Chaos Weight = (ChaoticScore / 100)
Warlike Weight = (WarlikeScore / 100)
Faith Weight = (FaithScore / 100)
Battle Bonus = InBattle ? 3.0 : 1.0  // Triple odds if plucked from combat
Distance Bonus = clamp(DistanceThrown / 100, 1.0, 2.5)

Final Chance = Base * Physique² * Chaos * Warlike * Faith * BattleBonus * DistanceBonus

Example: Physique 80, Chaotic 90, Warlike 85, Faith 70, in battle, thrown 150m
= 0.01 * (0.8)² * 0.9 * 0.85 * 0.7 * 3.0 * 1.5
= 0.01 * 0.64 * 0.9 * 0.85 * 0.7 * 3.0 * 1.5 ≈ 0.173 = 17.3% chance
```

**Alignment Consequences:**
- Creating a Fallen Star Demon applies **severe evil alignment shift** to the causing deity (-30 to -50 moral axis)
- Witnesses (villagers who saw the throw) gain fear and lose loyalty to that god
- The demon's origin story is recorded in telemetry and narrative systems for emergent storytelling

**Other Ascension Paths (Framework):**
- **Ascended Angel:** Lawful Good + High Faith + Dies heroically saving others → Returns as protective angel after delay
- **Vengeful Wraith:** Any alignment + Betrayed by player god + High resentment → Becomes haunting spirit
- **Corrupted Lich:** Scholarly + Forbidden research + Cursed → Undead necromancer threat
- **Collective Ascension:** Entire villages/bands can undergo group transformations under extreme conditions (mass sacrifice, divine abandonment, apocalyptic events)

All ascension paths retain the original entity's **name**, **relationships**, and **memory**, creating deeply personal emergent narratives where player actions have lasting, unpredictable consequences.

- **Seasons & Climate:** Ten-day seasons cycle through biome-specific weather patterns affecting resources, morale, and cultural behaviors. Races/outlooks respond differently (e.g., winter hardship, spring fertility). Deities cannot shift season timing but may influence climate within a season via miracles.
- **Death & Memorials:** Villages maintain burial sites; graves scale with fame/fortune/glory. High-belief individuals emit passive mana posthumously (tripled for martyrs). Festivals can honor the deceased, granting morale or thematic bonuses. Hovering graves reveals final stats/memories, reinforcing legacy.
---

## Technology Progression & Research

### Tech Tier Ladder
- **Tier Range:** 1 (mud-hut hamlet) → 20 (multopolis, high-tech civilization).
- **Early Tiers (1–5):** Basic huts, manual farming, improvised tools; armies wield simple weapons, minimal armor.
- **Mid Tiers (6–12):** Timber/stone architecture, organized logistics, basic metallurgy, animal-drawn transport upgrades, budding siege tech.
- **High Tiers (13–17):** Advanced crafts, proto-industrial processes, semi-automated extraction, disciplined armies with composite armor and ranged tech.
- **Apex Tiers (18–20):** Automated infrastructure, flying/engine-driven transport, power armor and weaponry, artisans crafting epic/legendary gear with ease, full aesthetic unlocks tied to outlook/alignments.

### Research Infrastructure
- **Schools:** Entry-level literacy; boost baseline knowledge growth when staffed by educated villagers.
- **Universities:** Mid-tier research hubs accelerating tech progression; require scholars with high education and wisdom.
- **Academies:** Advanced institutions unlocking specialist tech trees (military, logistics, arcana) and enabling apex-tier breakthroughs.
- **Supplementals (Optional):** Libraries, archives, observatories, laboratories—tunable modules to specialize domains (science, magic, engineering).
- **Education Pipeline:** Villagers grow through nurseries → schools → universities → academies. Progress scales with individual wisdom and will. Apprenticeships copy mentor wisdom/experience; traits can be inherited. Materialistic outlooks gain modest learning-speed bonuses.
- **Artisan Progression:** Tech tiers unlock new equipment recipes (common → rare → legendary). Outlooks influence priorities (materialists craft wealth gear, warlike focus weapons, spiritual produce relics). Champions/heroes acquire gear like others but capture elite loot via their exploits. Legendary items confer cultural bonuses, spark special prayers if lost/stolen, and boost artisan fame.

### Staffing & Progression Drivers
- Researchers contribute via education, wisdom, and relevant outlook traits (scholarly, spiritual, materialistic, etc.).
- Facilities apply multipliers based on staffing quality and capacity utilization (e.g., academies cap at small cohorts but grant large tier boosts).
- Tech tier increments trigger milestone events, unlock unique buildings/outlooks, and update registry telemetry.

### Outlook Influence
- Scholarly/spiritual/materialistic outlook weights shift which research domains receive priority (e.g., spiritual academies bias miracles, warlike bias military tech).
- Fanatic outlooks may lock certain tech paths or accelerate specialized branches at the expense of others.

### Telemetry & Feedback
- Registry snapshots broadcast current tech tier, active research rate, and facility occupancy.
- Visual evolution (architecture, lighting, VFX) escalates with tier milestones, reinforcing sandbox progression without direct player control.

All numbers/tier thresholds are tunable data so designers can iterate on pacing per game mode.

---

## Resource Economy & Logistics

### Primary Resource Families
- **Ore:** Mineral deposits with richness tiers (basic metal, rare metal); infinite mines rebalancing extraction rate by color-coded richness.
- **Wood:** Forest/vegetation stands that propagate over time; supports construction, fuel, and crafted goods.
- **Food:** Crops, gathered flora, domesticated fauna; empowers population growth and spiritual/agrarian outlook bonuses.
- **Luxury Resources:** Gems, exotic flora/fauna, crafted fineries; fuel artisan output, trade leverage, and morale spikes.
- **Building Materials:** Refined goods (planks, bricks, alloys) produced via local chains for higher-tier structures.

### Production Chains
- **Refinement:** Ore → smelters → metals/rare alloys; wood → lumber mills → planks/treated timber.
- **Crafting:** Toolmakers, blacksmiths (weapon/armor specialization), armorers, artisans each convert inputs into equipment, epic gear, or cultural goods.
- **Food Processing:** Farms → mills → bakeries; ranches → smokehouses; spiritual outlooks may dedicate surplus to rituals.

### Outlook Priorities
- **Materialistic:** Pursue surplus in all categories, emphasizing rapid production, expansion, and infrastructure.
- **Spiritual:** Hoard food, encourage population growth, funnel luxuries into worship rites.
- **Xenophilic:** Seek outreach/indenture arrangements depending on moral alignment—trade webs for good-aligned, exploitative networks for corrupt variants.
- **Warlike:** Prioritize metals, rare alloys, and gear production to outfit bands/armies.
- **Agrarian/Mercantile:** Bias food stability and market throughput respectively.

### Regeneration & Extraction
- Animals respawn on ecological timers; vegetation spreads via propagation models.
- Ore mines are infinite but reflect richness via extraction speed/reward modifiers (color-coded tiers).
- Production buildings are auto-placed by villages when prerequisites are met; specialization arises from staffing outlooks.

### Trade & Logistics
- Villages establish markets and dispatch caravans along safe routes; individuals and aggregates can participate in trade pacts.
- Outlook/alignment governs trade openness: xenophilic/mercantile societies reach out aggressively, while xenophobic/isolationist groups restrict exchanges.
- Higher tech tiers unlock improved transport (engine wagons, aerial haulers) that boost throughput and reduce travel risk.
- Trade routes arise via AI-negotiated agreements (automatic fixed pricing for allies). Caravan makeup scales with cargo value, distance, and threat level; special items (luxuries, relics) receive extra escorts but attract bandit targeting. Raided caravans trigger bounties, retaliatory bands, or patrol increases depending on loss severity.

### Telemetry & Visual Cues
- Per-village resource metrics track raw stock, refined goods, luxury reserves, and trade throughput; aggregate-level dashboards planned for later iterations.
- Storehouses and caravans display visual load cues to communicate abundance or scarcity.
- Scarcity alarms flag when critical resources drop below outlook-defined safety thresholds.
- Outlook-driven escalation: shortages trigger collective prayers that intensify as deficits grow. Reasonable outlooks (mercantile, diplomatic) prioritize diplomacy/trade agreements first; aggressive outlooks pivot to raids once peaceful options fail or patriotism erodes.

All resource rates, regeneration curves, and trade caps are tunable data to support balancing across sandbox, campaign, or multiplayer modes.

---

## Worship & Miracle System

### Worship Mana Flow
- **Belief:** Each entity pledges to a god (player or AI). Belief determines which mana pool their devotion feeds and which alignment axis their loyalty affects.
- **Faith:** Measures devotion strength; baseline worshiper contributes `1 mana/second` at faith 1, ramping linearly to `4 mana/second` at faith 100 (tunable). Place-of-worship structures (shrines, temples, cathedrals) apply additional multipliers. Spiritual-aligned worshippers gain an extra `+50–100%` bonus on top of faith scaling. Faith boosts persist over a believer's lifetime, trickling tribute/mana even after prayers are fulfilled.
- **Loyalty Axis:** Fear ↔ Respect ↔ Love describes how worshippers relate to their deity. High fear skews toward evil alignment shifts, love pushes good, respect stabilizes neutral. Loyalty feedback grants alignment points to the worshiped god and influences miracle reception.
- **Quests as Prayers:** Villagers generate prayer requests for divine assistance (combat survival, construction acceleration, feeding, healing). Answered prayers increase belief in the responding deity, raising individual faith and granting immediate tribute plus ongoing tithe over that villager's lifetime. Multiple gods may compete to respond; credit and alignment influence go to the first to complete the request, with cooperative assists granting partial rewards. Prayer chains can be shared between believers for multiplicative gains, encouraging communal rituals. Each villager maintains a current desire (visible via tooltip/inspect); prayers grow in urgency over time, can be overwritten by higher-priority crises, and aggregate into collective petitions when needs align.
- **Prayer Triggers:** Conditions include health below thresholds, hunger/starvation risk, resource scarcity, stalled construction, homelessness, and other outlook-specific needs. Urgency spikes compound when crises persist or stack (injury + hunger). Outlooks generate unique petitions (e.g., materialists praying for rich mine veins) enabling gods to bless assets proactively.
- **Request Cadence:** Fulfilled prayers impose a grace period (≈one day) before identical petitions reissue; unmet needs can reshuffle multiple times per day as urgency rises or priorities shift.

- **Tribute Loop:** Tribute is earned by fulfilling prayer quests or leveling player-controlled villages. Tribute unlocks new miracle families, while advanced villages grant special modifiers (reduced cost, unique variants). Prayers expire based on urgency (saving lives vs bumper harvest); expired requests lower faith and reduce future tribute potential.
- **Legacy Calls:** Legacy miracles (e.g., Rain Miracle) re-enter via this path—Rain costs **100 mana** baseline before modifiers.

### Casting Parameters
- Miracles expose behavioral modes per type (e.g., Rain: nourishing drizzle vs destructive downpour vs blizzard/hail when climate allows).
- Intensity slider controls footprint, duration, and potency; higher intensity exponentially increases mana costs. No cooldowns—access is gated purely by mana, tribute, and alignment requirements.
- Casting outside player-controlled influence rings applies distance-based mana surcharges (up to +1000% at extreme range), encouraging local stewardship and infrastructure expansion.
- Casting triggers village reaction checks: grateful (love/respect) communities escalate worship; fearful ones may increase obedience but also resentment if overused.
- Quest feedback loops: fulfilled prayers broadcast gratitude events; ignored or failed prayers decrease faith and generate alignment penalties toward the negligent deity. High-faith believers offer larger tribute rewards for answered prayers and continue tithing mana over time.
- Shared-belief villagers can co-sign prayers to increase payout multipliers, but only the deity who completes the request receives the prayer fulfillment credit.
- Prayer precedence: life-critical requests (self or loved ones) override other needs; outlooks reorder remaining priorities (materialistic → production, spiritual → sustenance, etc.). Villagers may reissue the same prayer with multiplicative rewards—saving the same individual repeatedly greatly amplifies loyalty and future tribute.
- Time bubbles obey edge-freeze rules: entities entering or exiting are locked until the effect ends, preventing exploits. Rewind modes require recorded state snapshots (entities, inventory, resource delta) and clamp to a configurable PureDOTS tick window; fidelity prioritizes villager state above world resources when buffers saturate.

### Planned Miracle Families (Extensible)
- **Weather & Growth:** Rain, sunlight, fertile blessings.
- **Healing & Protection:** Restore health, shields, shelter domes.
- **Destruction & Punishment:** Lightning, meteors, plagues.
- **Summoning & Manifestation:** Spawn guardians, resource nodes, or divine constructs.
- **Industrial & Logistics:** Accelerate production, teleport caravans, automate harvests.
- **Time Manipulation:** Slow, hasten, pause, or rewind localized time bubbles; intensity defines depth (e.g., minor haste vs full rewind).

### Baseline Miracle Roster (Sandbox Slice)
- **Rain:** Nourishing drizzle, heavy storm, or climate-tuned blizzard/hail with intensity slider.
- **Water:** Moisturize crops, flood terrain, cleanse corruption.
- **Meteor:** Precision strike or carpet bombardment; higher intensity escalates crater size and fallout.
- **Fire:** Ember ignition, wildfire spread, or focused inferno blast.
- **Life:** Revive fallen villagers, regrow vegetation, purify blighted zones.
- **Death:** Cull hostile forces, wither crops, impose decay debuffs.
- **Heal:** Burst heal, regenerative aura, or cleansing of ailments.
- **Electricity:** Chain lightning, storm fields, power grid jumpstarts.
- **Air:** Gust control, lift units, disperse toxins or fog.
- **Shield:** Protective domes, directional barriers, anti-projectile walls.
- **Tornado:** Mobile cyclone for crowd control or terrain reshaping.
- **Joy:** Morale surge, productivity buffs, festival-triggered loyalty shifts. Boosts fighting efficiency, accelerates tech momentum, and spurs population growth; effects last roughly one day with diminishing returns on repeated casts.
- **Despair:** Fear shockwave, productivity and morale debuffs, rebellion suppression. Reduces fighting efficiency and mood, increasing risk of mental breakdowns; mirrors Joy duration/decay with diminishing returns.
- **Time:** Area bubble that slow/haste/stop entities inside; edges freeze entrants while active; high intensity enables localized rewinds (rolling back entity state/resources within scope) at steep mana cost.
- **Gating (MVP):** All miracles above are available baseline except `Death`, which remains tribute-locked initially to validate the unlock pipeline.

Numbers (costs, cooldowns, loyalty modifiers) remain tunable to balance sandbox pacing and alignment playstyles.

---

## Alignment & Outlook Taxonomy

### Alignment Axes (Godgame Baseline)
- **Moral Axis:** Good ↔ Neutral ↔ Evil
- **Order Axis:** Lawful ↔ Neutral ↔ Chaotic
- **Purity Axis:** Pure ↔ Neutral ↔ Corrupt

Each entity carries all three alignment readings simultaneously (e.g., Lawful Good Corrupt), enabling nuanced combinations like "Corrupt Good". Purity states modulate how strongly moral/order choices manifest in behavior and rolls.

### Core Outlook Families
- `Materialistic` – wealth, craft, inheritance duty
- `Spiritual` – faith, ritual, devotion cycles
- `Warlike` – offense, conquest, martial honor
- `Peaceful` – caretaking, diplomacy, healing
- `Expansionist` – settlement growth, frontier pushes
- `Isolationist` – fortification, inward prosperity
- `Scholarly` – knowledge, research, arcana
- `Mercantile` – trade, markets, logistics
- `Agrarian` – food security, land stewardship
- `Artisan` – aesthetics, culture, festivals

### Outlook Axes
Outlooks function as independent ideological axes similar to alignments, with spectra such as:
- **Xeno Axis:** Xenophilic ↔ Neutral ↔ Xenophobic
- **Warfare Axis:** Warlike ↔ Neutral ↔ Peaceful
- **Expansion Axis:** Expansionist ↔ Neutral ↔ Isolationist
- **Economy Axis:** Materialistic ↔ Neutral ↔ Spiritual (or other economic/spiritual pairs)
- **Culture Axis:** Scholarly ↔ Neutral ↔ Artisan (example pairing)

An entity may carry up to **three regular outlooks** simultaneously, or trade that breadth for **two fanatic outlooks** (extreme positions locked near the axis endpoints). Fanatic outlooks impose stronger behavioral biases and heavier initiative modifiers. Aggregate entities (villages, guilds, companies, bands, armies) follow the same rules: their collective outlook slots represent dominant cultural ideals or strategic doctrines, derived from member contributions and leadership influence.

Purity plus the tri-axis alignment readings and the active outlook set define the weighting tables referenced in event resolution rolls and village cultural behavior.

- **Aggregate Stat Derivation:** Collective entities compute their key stats by sampling top-performing members rather than simple averages, emphasizing leadership and specialist impact. Numbers below are illustrative and should be parameterized/tuned per domain.
- **Army Perception:** Sample the upper scout cohort (e.g., top percentile slice) to represent reconnaissance acuity.
- **Village Wisdom:** Drawn from ruling elites or council (e.g., top governance skill holders) to reflect decision-making quality.
- **Band Initiative:** Controlled by leader or co-leader average initiative scores, cascading to member urgency.
- **Trading Guild Logistics:** Weighted by highest logistics-skilled artisans/clerks to simulate operational expertise.
Sampling windows are configurable per stat (percentile, fixed count, role-weighted), enabling nuanced aggregation and preventing noise from low-skill populations. Tune these thresholds via data assets so designers can iterate without code changes.

---

## Exploits

- **Miracle Spam:** Overusing low-cost miracles could trivialize alignment drift—needs diminishing returns or prayer cost scaling (Severity: Medium).  
- **Resource Hoarding:** Player-built bottlenecks might starve AI settlements—consider NPC fallback trade or smuggling (Severity: Medium).

---

## Tests

- [ ] Simulated 60-minute sandbox run maintains ≥3 active villages.
- [ ] Alignment extremes trigger appropriate village state transitions.
- [ ] Player miracle interventions correctly adjust worship mana metrics without destabilizing simulation.
- [ ] Performance stays within target entity budgets at peak population.
- [ ] High-initiative villages pursue expansion actions despite low surplus, while low-initiative analogues defer plans.
- [ ] Personality variants (lawful vs chaotic) exhibit distinct initiative frequency/amplitude when exposed to identical event sequences.

---

## Performance

- **Complexity:** O(n) per village for aggregation; O(m) per villager for alignment sampling (aim for linear passes).  
- **Max Entities:** Target 500 villagers, 10 villages, 20 active bands.  
- **Update Freq:** Evaluate cohesion/alignment every 0.5s simulated; expensive recalculations (expansion planning) every 5s.

---

## Visual Representation

### System Diagram
```
[World Resources] → sustain → [Villagers]
[Villagers] → cluster into → [Villages]
[Villages] ↔ share → [Alignment Outlooks]
[Villages] → spawn → [Bands]
[Player Miracles] → influence → [Villages & Alignment]
```

### Data Flow
```
Resource Nodes → Harvest → Storehouses → Village Cohesion → Alignment-directed Surplus Spend → Worship Mana → Registry/Telemetry
```

---

## Iteration Plan

- **v1.0 (MVP):** Villagers cluster into villages, manage resources, track worship mana as the miracle fuel.  
- **v2.0:** Add alignment variants affecting expansion logic and band behavior.
- **v3.0:** Integrate diplomacy, inter-village conflict, and multiplayer hooks.

---

## Open Questions

### ✅ **ANSWERED:**
1. ✅ **Village founding threshold:** Villagers split when resources in their area are "reasonably exploited" 
   - <DESIGN QUESTION: What % depletion = "reasonably exploited"? 60%? 75%? 90%?>
   - <DESIGN QUESTION: Does this check raw nodes remaining, harvest rate decline, or storehouse throughput?>

3. ✅ **Initiative system:** Events trigger rolls that can increase or decrease initiative, with rationale on both sides
   - <DESIGN QUESTION: What's the roll formula? d20 + modifiers vs difficulty?>
   - <DESIGN QUESTION: Example rationales needed - what argues for +initiative vs -initiative per event type?>

5. ✅ **Tech tier progression:** Advancement requires unlocking a set number of research milestones per level
   - <DESIGN QUESTION: How many milestones per tier? Fixed (e.g., 5) or scaling (tier 1 = 3, tier 10 = 8)?>
   - <DESIGN QUESTION: Are milestones domain-specific (military, civic, arcane) or generic research points?>

6. ✅ **Resource crisis threshold:** 10% stockpile triggers desperate state (diplomacy/conflict responses)
   - <DESIGN QUESTION: Is this per-resource or aggregate? (10% food = desperate even if 100% wood?)>
   - <DESIGN QUESTION: What actions unlock at desperate state? Raid priority? Surrender offers?>

### ❌ **STILL OPEN:**
7. How should tribute tiers map to miracle family unlocks and unique village-specific bonuses?
8. How are prayer requests prioritized/queued when multiple crises occur simultaneously?
9. How do shared or competing prayers distribute credit and alignment influence across multiple deities without double-counting?
10. How do we surface villager desires/prayer state in UI without overwhelming the player?

---

## References

- `Docs/TruthSources_Inventory.md#villagers` for current villager truth sources.
- `Docs/TruthSources_Inventory.md#storehouses` for resource storage flows.
- `Docs/Concepts/Villagers/Village_Villager_Alignment.md` for deeper alignment exploration.
- `Docs/Concepts/Core/Prayer_Power.md` for prayer economy interactions.

---

## Related Documentation

- **Villager Behavioral Personality:** `Docs/Concepts/Villagers/Villager_Behavioral_Personality.md` (initiative expression, vengeful/forgiving, bold/craven axes)
- **Wealth & Social Dynamics:** `Docs/Concepts/Villagers/Wealth_And_Social_Dynamics.md` (wealth strata, family/dynasty aggregates, charity vs opportunism, disinheritance)
- Villager transformation mechanics: `Docs/Concepts/Villagers/Villager_Ascension_System.md`
- World reshaping and counter-events: `Docs/Concepts/World/Apocalypse_And_Rebirth_Cycles.md`
- Pending Bands registry concept: `Docs/Concepts/Villagers/Bands_Vision.md`
- Resource loops: `Docs/Concepts/Resources/` (legacy material pending refresh)

---

**For Implementers:** Align future truth source work (Village, Alignment, Bands) with the state machine and metrics above.
**For Designers:** Use Player Interaction and Iteration Plan sections to scope sandbox milestones before layering campaign/skirmish variations.
