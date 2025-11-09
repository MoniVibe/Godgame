# Apocalypse & Rebirth Cycles

**Status:** Design
**Category:** System - World State Transformations
**Scope:** World Bosses, Environmental Systems, Emergent Gameplay
**Created:** 2025-11-03
**Last Updated:** 2025-11-03

---

## Purpose

Enable world bosses to **reshape the entire world** in their image when they achieve total victory, creating apocalypse scenarios that spawn counter-apocalypse events, forming never-ending emergent narrative cycles through state-based triggers and criteria-driven events.

**Primary Goal:** Transform the world completely based on dominant faction, creating entirely new gameplay contexts.
**Secondary Goals:**
- Generate asymmetric counter-threats to apocalypse states (angels vs demons, nature vs undead)
- Support infinite narrative cycles without player intervention required
- Create faction-specific civilizations (demon villages, undead fortresses, etc.)
- Maintain sandbox viability even when one force dominates

---

## Core Concept: World State System

### World States (Singleton)

**Normal State (Baseline):**
- Mixed biomes, mortal civilizations
- Standard spawn tables
- Natural resource regeneration
- Typical world boss threats

**Apocalypse States (World Boss Victory):**
Each world boss type can transform the world into their domain:

1. **Demon Hellscape** - Fallen Star Demons conquer world
2. **Undead Wasteland** - Lich armies consume all life
3. **Frozen Eternity** - Ice elemental freezes world
4. **Verdant Overgrowth** - Nature spirits overwhelm civilization
5. **Celestial Dominion** - Angels purge all sin (totalitarian heaven)
6. **Void Corruption** - Eldritch entities warp reality
7. **[Extensible]** - Any world boss type can define apocalypse state

### Transformation Trigger

**Victory Condition:**
- World boss achieves **80%+ territory control** OR
- World boss eliminates **all opposing civilizations** OR
- World boss lives unchallenged for **X game-years**

**When triggered:**
1. World state singleton switches from `Normal` → `ApocalypseType`
2. Biome conversion systems activate (gradual reshaping)
3. Existing structures destroyed/corrupted
4. Spawn tables swap to apocalypse-specific content
5. Counter-apocalypse timer starts (daily/seasonal event checks)

---

## Apocalypse Example: Demon Hellscape

### World Transformation Sequence

**Phase 1: Corruption Spread (Weeks 1-4)**
- **Biome Conversion:** Normal terrain → Scorched earth, lava rivers, ash clouds
- **Vegetation Death:** All trees/plants wither and burn
- **Structure Corruption:** Mortal villages crumble or transform into demon fortresses
- **Fauna Mutation:** Animals flee, die, or transform into hellhounds/imps
- **Sky Change:** Permanent blood-red sky, no day/night cycle

**Phase 2: Demon Civilization (Month 2+)**
- **Demon Villages Spawn:** Organized settlements emerge at resource nodes
  - Built from obsidian, bone, iron spikes
  - Monuments erected (torture pillars, demon idols, hell gates)
  - Lava forges for infernal crafting
  - Slave pens (captured mortal souls if any survived)

- **Demon Spawning:** Lesser demons continuously warp into world
  - Hell rifts appear as permanent spawn points
  - Demon population grows over time
  - Hierarchies form (imp workers → demon soldiers → lords)

- **Resource Conversion:**
  - Wood nodes → Bone/charred wood
  - Ore veins → Infernal metals (dark iron, brimstone)
  - Water sources → Sulfur pools, blood lakes

**Phase 3: Stable Hellscape (Ongoing)**
- Self-sustaining demon ecosystem
- Demon villages trade via caravans (slaves, souls, weapons)
- Demon bands patrol territories
- Continuous monument construction (demon culture)
- World boss reigns as Demon King/Queen

### Demon Civilization Behavior

**Settlement Patterns:**
- **Warlike + Chaotic alignment** drives demon village logic
- Frequent internal conflicts (demon politics via combat)
- Expansion through conquest (demon vs demon wars)
- Slavery-based economy (mortal souls as currency if captured)

**Construction:**
- Monuments scale with power: Simple torture racks → Grand coliseums → Hell citadels
- Architecture reflects chaotic evil aesthetic
- Functional structures: Weapon forges, summoning circles, flesh pits

**Social Structure:**
- Might-makes-right hierarchy
- Strongest demons rule villages
- Demon lords emerge as village leaders
- Original fallen star demon (Korgath) is supreme overlord

---

## Counter-Apocalypse: Angelic Intervention

### Trigger Criteria

**Daily/Seasonal Event Checks:**
When world state = `DemonHellscape`, roll each day/season:
```
Base Chance = 0.5% per day (tunable)
Accumulated Chance += 0.1% per day of apocalypse (grows over time)
Demon Population Multiplier = (DemonCount / 500) * 0.5%  // More demons = more likely

If roll succeeds → Spawn Counter-Apocalypse Event
```

**Counter-Event: Angelic Slayer Arrival**

### The Falling Angel

**Spawn Sequence:**
1. **Meteor Herald:** Brilliant white/gold meteor visible across world
2. **Impact:** Meteor strikes in demon-controlled territory (near hell gate)
3. **Emergence:** Single angelic celestial emerges from crater
   - **Armed with divine super shotgun** (tongue-in-cheek reference, serious gameplay)
   - **Mission:** Purge demons, heal the land
   - **Mechanics:** Extremely powerful anti-demon specialist

**Angel Stats (Asymmetric Balance):**
```csharp
Angelic Slayer:
- Health: 5000 (vs demon 2000)
- Damage vs Demons: 200 per shot (vs demon 80)
- Damage vs Non-Demons: 20 (weak against mortals)
- Move Speed: 8 (very mobile)
- Special: Cleansing Aura (heals land in 10m radius, damages demons)
- Immunity: Immune to demon abilities, mortal weapons CANNOT harm
- Vulnerability: Only celestial beings or empowered champions can harm
```

**Behavior:**
- **Seeks demon concentrations** (villages, hell gates)
- **Methodical extermination** - clears area by area
- **Heals terrain** as it travels (scorched earth → grassland over time)
- **Plants holy markers** (create safe zones demons can't spawn in)
- **Solo operative** - does not ally with mortals (focused mission)

### Land Healing Mechanics

**As Angel moves:**
- Biome conversion reversal: Hellscape → Normal terrain (gradual, 1 day per cell)
- Vegetation regrows in cleansed zones
- Hell gates sealed (no more demon spawns)
- Demon structures crumble when angel nearby
- Sky clears to normal day/night in cleansed regions

**Full Cleansing:**
If Angel eliminates **80%+ demons** and seals all hell gates:
- World state reverts: `DemonHellscape` → `Normal`
- Surviving mortals can repopulate
- New cycle begins (world boss threats can spawn again)

---

## Recursive Counter-Play: Demonic Champions

### God Intervention Option

**Player Response to Angel Threat:**
If player deity has **demonic alignment** or **wants to preserve hellscape:**

**Action: Choose Demonic Champion**
- Select a powerful demon (or transform existing champion)
- Grant **celestial damage capability** (normally demons can't harm angels)
- Equip with **infernal artifacts** (weapons that harm celestial beings)
- Set mission: **Hunt the angel**

**Demonic Champion Stats:**
```csharp
Empowered by Dark God:
- Health: 3000 (between angel and standard demon)
- Damage vs Angels: 150 per strike
- Damage vs Mortals: 100
- Special: Hellfire Aura (damages angel over time when nearby)
- Equipment: Infernal blade, corruption armor
- Divine Backing: Can channel god's miracles (Dark Shield, Hellstorm)
```

**Asymmetric Battle:**
- Angel is stronger 1v1
- Demonic Champion must use tactics (ambush, demon army support)
- Angel has range advantage (shotgun), Champion has melee/magic
- Winner reshapes local region (Angel heals, Champion re-corrupts)

### Emergent Outcomes

**Scenario 1: Angel Wins**
- Kills demonic champion
- Continues cleansing
- World reverts to normal over time
- New threats can emerge (different world boss types)

**Scenario 2: Demonic Champion Wins**
- Slays angel
- Hellscape preserved
- Champion becomes new world boss threat (power boost)
- Counter-apocalypse timer resets (angel can spawn again later)

**Scenario 3: Stalemate**
- Both survive, territory divided
- Angel controls cleansed zones (holy lands)
- Champion defends corrupted zones (hellscape)
- World becomes **hybrid state** (part normal, part apocalypse)
- Both forces build up for eventual confrontation

---

## Other Apocalypse Types (Framework)

### Undead Wasteland (Lich Apocalypse)

**Transformation:**
- Gray fog blankets world, permanent twilight
- All life dies → reanimates as undead
- Villages become crypts/necropoli
- Skeletal workers harvest bones/souls

**Counter-Event: Life Priests**
- Group of 3-5 clerics spawn together
- Cast resurrection magic (revive fallen villages)
- Purge undead with holy fire
- Plant life seeds (trees regrow in their wake)

### Frozen Eternity (Ice Elemental Apocalypse)

**Transformation:**
- Eternal winter, glaciers cover land
- All water freezes solid
- Villages entombed in ice
- Ice elementals roam frozen wastes

**Counter-Event: Fire Titan**
- Massive fire elemental awakens from volcano
- Melts ice wherever it walks
- Restores warmth to world
- Fights ice elementals

### Verdant Overgrowth (Nature Spirit Apocalypse)

**Transformation:**
- Hyper-aggressive plant growth
- Jungle covers everything (structures consumed)
- Giant carnivorous plants spawn
- Villages overgrown, mortals can't clear fast enough

**Counter-Event: Druids of Balance**
- Nature mages who oppose wild growth
- Prune overgrowth, restore ecological balance
- Tame carnivorous plants
- Re-establish mortal-nature coexistence

### Celestial Dominion (Angelic Purge Apocalypse)

**Transformation (Ironic Dystopia):**
- Angels descend and "purify" world
- All sin punished (totalitarian heaven)
- Mortals forced into rigid order
- Free will suppressed

**Counter-Event: Fallen Hero**
- Mortal rebel refuses angelic control
- Leads resistance movement
- Fights for freedom vs forced purity
- Angels become antagonists (order taken too far)

---

## DOTS 1.4 Implementation Architecture

### World State Singleton

```csharp
namespace Godgame.WorldState
{
    /// <summary>
    /// Singleton tracking global world state and apocalypse conditions.
    /// </summary>
    public struct WorldStateData : IComponentData
    {
        public enum State : byte
        {
            Normal,
            DemonHellscape,
            UndeadWasteland,
            FrozenEternity,
            VerdantOvergrowth,
            CelestialDominion,
            VoidCorruption,
            HybridState  // Multiple apocalypses coexist
        }

        public State CurrentState;
        public State PreviousState;  // Track transitions
        public uint StateChangeTick;

        // Apocalypse progress
        public float TransformationProgress;  // 0-1: How much world is converted
        public uint ApocalypseStartTick;

        // Territory control (for victory condition)
        public float DominantFactionControl;  // 0-1: % world controlled by victor
        public Entity DominantFactionEntity;  // Which world boss/faction leads

        // Counter-apocalypse tracking
        public float CounterEventAccumulatedChance;  // Grows daily
        public uint LastCounterEventCheckTick;
        public uint CounterEventsSpawned;
    }

    /// <summary>
    /// Tracks regional apocalypse state (for hybrid scenarios).
    /// Attached to biome/region entities or spatial grid cells.
    /// </summary>
    public struct RegionalApocalypseState : IComponentData
    {
        public WorldStateData.State RegionState;
        public float CorruptionLevel;        // 0-1: How corrupted
        public uint CorruptionStartTick;
        public Entity CorruptingEntity;      // Which world boss/faction
    }

    /// <summary>
    /// Marks entities that spread apocalypse transformation.
    /// World bosses and their minions carry this.
    /// </summary>
    public struct ApocalypseAgent : IComponentData
    {
        public WorldStateData.State ApocalypseType;
        public float CorruptionRadius;       // Area of effect
        public float CorruptionRate;         // Speed of transformation
    }
}
```

### Biome Conversion System

```csharp
namespace Godgame.WorldState.Systems
{
    /// <summary>
    /// Converts biomes/terrain when apocalypse spreads.
    /// Gradual transformation based on proximity to apocalypse agents.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BiomeConversionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var worldState = SystemAPI.GetSingleton<WorldStateData>();

            // Only run if apocalypse active
            if (worldState.CurrentState == WorldStateData.State.Normal)
                return;

            var deltaTime = SystemAPI.Time.DeltaTime;

            // Query all apocalypse agents (world bosses, demons, etc.)
            foreach (var (agent, transform) in SystemAPI
                .Query<RefRO<ApocalypseAgent>, RefRO<LocalTransform>>())
            {
                var agentPos = transform.ValueRO.Position;
                var corruptionRadius = agent.ValueRO.CorruptionRadius;

                // Query regional states within radius and corrupt them
                // (Implementation would use spatial queries for performance)
                // Pseudo-code:
                // - Find all biome cells within radius
                // - Increment CorruptionLevel based on agent.CorruptionRate * deltaTime
                // - When CorruptionLevel >= 1.0, convert biome type
                //   (e.g., Temperate Forest → Scorched Hellscape)
            }

            // Update world transformation progress
            // (Calculate % of world in apocalypse state vs normal)
        }
    }

    /// <summary>
    /// Monitors world state and triggers full apocalypse when conditions met.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ApocalypseTriggerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var worldState = SystemAPI.GetSingleton<WorldStateData>();

            // Only check in Normal state (not already apocalypse)
            if (worldState.CurrentState != WorldStateData.State.Normal)
                return;

            // Query all world bosses for territory control
            foreach (var (boss, celestial, entity) in SystemAPI
                .Query<RefRO<WorldBoss>, RefRO<CelestialBeing>>()
                .WithEntityAccess())
            {
                // Calculate territory control (simplified - would use spatial registry)
                float controlPercent = CalculateTerritoryControl(ref state, entity);

                const float ApocalypseThreshold = 0.8f;  // 80% control
                if (controlPercent >= ApocalypseThreshold)
                {
                    // TRIGGER APOCALYPSE!
                    TriggerApocalypse(ref state, celestial.ValueRO.CelestialType, entity);
                    break;  // Only one apocalypse at a time (for now)
                }
            }
        }

        private float CalculateTerritoryControl(ref SystemState state, Entity bossEntity)
        {
            // Implementation would:
            // - Query all villages/settlements
            // - Count how many are controlled by this boss's faction
            // - Return percentage
            return 0f;  // Placeholder
        }

        private void TriggerApocalypse(ref SystemState state,
            CelestialBeing.Type celestialType, Entity bossEntity)
        {
            var worldState = SystemAPI.GetSingleton<WorldStateData>();

            // Map celestial type to apocalypse state
            var apocalypseState = celestialType switch
            {
                CelestialBeing.Type.RampagingDemon => WorldStateData.State.DemonHellscape,
                CelestialBeing.Type.CorruptedLich => WorldStateData.State.UndeadWasteland,
                CelestialBeing.Type.IceElemental => WorldStateData.State.FrozenEternity,
                CelestialBeing.Type.ProtectiveAngel => WorldStateData.State.CelestialDominion,
                _ => WorldStateData.State.Normal
            };

            if (apocalypseState == WorldStateData.State.Normal)
                return;  // No apocalypse defined for this type

            // Update world state
            worldState.PreviousState = worldState.CurrentState;
            worldState.CurrentState = apocalypseState;
            worldState.StateChangeTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;
            worldState.TransformationProgress = 0f;
            worldState.ApocalypseStartTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;
            worldState.DominantFactionControl = 1f;
            worldState.DominantFactionEntity = bossEntity;
            worldState.CounterEventAccumulatedChance = 0f;

            SystemAPI.SetSingleton(worldState);

            // Broadcast event
            // "The world has fallen! Demon Hellscape spreads!"
        }
    }
}
```

### Counter-Apocalypse Spawn System

```csharp
namespace Godgame.WorldState.Systems
{
    /// <summary>
    /// Checks daily for counter-apocalypse event spawns.
    /// Probability increases over time apocalypse persists.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CounterApocalypseSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var worldState = SystemAPI.GetSingleton<WorldStateData>();

            // Only run if apocalypse active
            if (worldState.CurrentState == WorldStateData.State.Normal)
                return;

            var currentTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;

            // Check daily (1 game day = X ticks - tunable)
            const uint TicksPerDay = 60 * 10;  // Assuming 10 seconds = 1 day
            uint ticksSinceLastCheck = currentTick - worldState.LastCounterEventCheckTick;

            if (ticksSinceLastCheck < TicksPerDay)
                return;  // Not time yet

            // Daily roll for counter-apocalypse event
            float baseChance = 0.005f;  // 0.5% base
            float dailyAccumulation = 0.001f;  // +0.1% per day
            uint daysOfApocalypse = (currentTick - worldState.ApocalypseStartTick) / TicksPerDay;

            worldState.CounterEventAccumulatedChance += dailyAccumulation;

            float finalChance = baseChance + worldState.CounterEventAccumulatedChance;

            // Additional multipliers based on apocalypse type
            finalChance *= GetPopulationMultiplier(ref state, worldState.CurrentState);

            // Roll
            var random = Random.CreateFromIndex(currentTick);
            float roll = random.NextFloat();

            if (roll < finalChance)
            {
                // SPAWN COUNTER-APOCALYPSE EVENT!
                SpawnCounterEvent(ref state, worldState.CurrentState);

                // Reset accumulation
                worldState.CounterEventAccumulatedChance = 0f;
                worldState.CounterEventsSpawned++;
            }

            worldState.LastCounterEventCheckTick = currentTick;
            SystemAPI.SetSingleton(worldState);
        }

        private float GetPopulationMultiplier(ref SystemState state, WorldStateData.State apocalypseType)
        {
            // More dominant faction entities = higher spawn chance
            // E.g., 500 demons = 0.5x multiplier
            if (apocalypseType == WorldStateData.State.DemonHellscape)
            {
                int demonCount = CountEntitiesWithComponent<DemonTag>(ref state);
                return (demonCount / 500f) * 0.5f;
            }

            return 0f;
        }

        private int CountEntitiesWithComponent<T>(ref SystemState state) where T : unmanaged, IComponentData
        {
            var query = SystemAPI.QueryBuilder().WithAll<T>().Build();
            return query.CalculateEntityCount();
        }

        private void SpawnCounterEvent(ref SystemState state, WorldStateData.State apocalypseType)
        {
            // Determine counter-event based on apocalypse type
            switch (apocalypseType)
            {
                case WorldStateData.State.DemonHellscape:
                    SpawnAngelicSlayer(ref state);
                    break;

                case WorldStateData.State.UndeadWasteland:
                    SpawnLifePriests(ref state);
                    break;

                case WorldStateData.State.FrozenEternity:
                    SpawnFireTitan(ref state);
                    break;

                // ... other cases
            }
        }

        private void SpawnAngelicSlayer(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Find spawn location (near demon concentration)
            float3 spawnPos = FindDemonConcentration(ref state);

            // 1. Spawn meteor herald
            Entity meteor = CreateMeteorVFX(ref ecb, spawnPos + new float3(0, 200, 0), isHoly: true);

            // 2. Schedule angel spawn after meteor impact (5 seconds)
            var spawnQueue = GetScheduledSpawnQueue(ref state);
            var buffer = state.EntityManager.GetBuffer<ScheduledCounterEventSpawn>(spawnQueue);
            buffer.Add(new ScheduledCounterEventSpawn
            {
                SpawnTick = (uint)state.WorldUnmanaged.Time.ElapsedTime + (60 * 5),  // 5 sec
                SpawnPosition = spawnPos,
                EventType = CounterEventType.AngelicSlayer
            });

            // Broadcast event
            // "A holy meteor approaches! Divine intervention incoming!"
        }

        private float3 FindDemonConcentration(ref SystemState state)
        {
            // Implementation would:
            // - Query all demon entities
            // - Find cluster with highest density
            // - Return center point
            return float3.zero;  // Placeholder
        }

        private Entity CreateMeteorVFX(ref EntityCommandBuffer ecb, float3 position, bool isHoly)
        {
            var meteor = ecb.CreateEntity();
            ecb.AddComponent(meteor, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 8f
            });

            if (isHoly)
            {
                ecb.AddComponent<HolyMeteorVFX>(meteor);  // White/gold trail
            }
            else
            {
                ecb.AddComponent<MeteorVFX>(meteor);  // Standard
            }

            ecb.AddComponent<MeteorFallPhysics>(meteor);
            return meteor;
        }

        private Entity GetScheduledSpawnQueue(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<ScheduledCounterEventSpawn>().Build();
            if (query.IsEmpty)
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddBuffer<ScheduledCounterEventSpawn>(entity);
                return entity;
            }
            return query.GetSingletonEntity();
        }
    }

    /// <summary>
    /// Scheduled counter-event spawn buffer.
    /// </summary>
    public struct ScheduledCounterEventSpawn : IBufferElementData
    {
        public uint SpawnTick;
        public float3 SpawnPosition;
        public CounterEventType EventType;
    }

    public enum CounterEventType : byte
    {
        AngelicSlayer,
        LifePriests,
        FireTitan,
        DruidsOfBalance,
        FallenHero
    }
}
```

### Angelic Slayer Entity

```csharp
namespace Godgame.CounterApocalypse
{
    /// <summary>
    /// The Angelic Slayer - counter-apocalypse hero for demon hellscape.
    /// Extremely powerful anti-demon specialist.
    /// </summary>
    public struct AngelicSlayer : IComponentData
    {
        public FixedString64Bytes Name;  // Procedurally generated
        public uint SpawnTick;
        public uint DemonsSlain;
        public uint HellGatesSealed;

        // Mission progress
        public float WorldCleansingPercent;  // 0-1: How much land healed
        public Entity CurrentTarget;         // Current demon/hell gate target
    }

    /// <summary>
    /// Cleansing aura that heals land and damages demons.
    /// </summary>
    public struct CleansingAura : IComponentData
    {
        public float Radius;             // Area of effect (10m)
        public float HealRate;           // Terrain heal per second
        public float DemonDamageRate;    // Damage to demons per second
    }

    /// <summary>
    /// Stats for angelic slayer (asymmetrically powerful vs demons).
    /// </summary>
    public struct AngelicSlayerCombat : IComponentData
    {
        public float Health;             // 5000
        public float MaxHealth;
        public float DamageVsDemons;     // 200 per shot
        public float DamageVsOthers;     // 20 (weak vs non-demons)
        public float AttackSpeed;        // 2.0 (fast)
        public float MoveSpeed;          // 8.0 (very mobile)

        // Special
        public bool ImmuneToMortalWeapons;
        public bool ImmuneToDemonAbilities;  // Can't be cursed, corrupted, etc.
        public byte ShotgunAmmo;             // Unlimited (celestial weapon)
    }

    /// <summary>
    /// AI for angelic slayer - seeks demon concentrations methodically.
    /// </summary>
    public struct AngelicSlayerAI : IComponentData
    {
        public enum State : byte
        {
            Seeking,      // Looking for demons
            Engaging,     // Fighting demons
            Sealing,      // Closing hell gates
            Healing,      // Cleansing land
            Patrolling    // Guarding cleared area
        }

        public State CurrentState;
        public float3 PatrolCenter;      // Where to return after clearing
        public float PatrolRadius;       // Area to defend
    }
}
```

---

## Demonic Champion Response System

```csharp
namespace Godgame.Champions
{
    /// <summary>
    /// Allows player god to empower a demon as champion to fight counter-apocalypse.
    /// </summary>
    public struct DemonicChampionCandidate : IComponentData
    {
        public float PowerLevel;         // How strong is this demon?
        public byte CombatExperience;    // How many kills?
        public float LoyaltyToGod;       // Devotion to dark god
    }

    /// <summary>
    /// Empowered demonic champion (can harm angels).
    /// </summary>
    public struct DemonicChampion : IComponentData
    {
        public FixedString64Bytes Name;
        public Entity PatronDeity;       // Which dark god empowered them
        public uint ChampioningSince;

        // Mission
        public Entity CurrentTarget;     // Angel or other threat
        public bool CanHarmCelestials;   // TRUE when empowered
    }

    /// <summary>
    /// Combat stats for empowered demon champion.
    /// </summary>
    public struct DemonicChampionCombat : IComponentData
    {
        public float Health;             // 3000 (between angel and normal demon)
        public float MaxHealth;
        public float DamageVsAngels;     // 150
        public float DamageVsMortals;    // 100
        public float AttackSpeed;        // 1.5
        public float MoveSpeed;          // 7.0

        // Special abilities (from god)
        public bool CanCastDarkMiracles;  // Draw from god's mana pool
        public float HellFireAuraDamage;  // Passive damage to angels nearby
    }
}
```

---

## Hybrid World States

### Concept: Multiple Apocalypses Coexist

**When counter-apocalypse succeeds partially:**
- Angel cleanses 40% of demon hellscape
- World splits into zones:
  - **Holy Lands** (angel-controlled, cleansed terrain)
  - **Hellscape** (demon-controlled, corrupted terrain)
  - **Contested Borders** (active battlefield)

**Result:**
- World state = `HybridState`
- Regional tracking: Each biome cell has own `RegionalApocalypseState`
- Both factions continue building civilizations in their zones
- Perpetual conflict at borders
- New world bosses can emerge in either zone

**Visual:**
```
[Holy Lands]       [Border War]      [Hellscape]
Angel villages     Fortifications    Demon villages
Grasslands         Scorched earth    Lava fields
Day/night cycle    Twilight          Blood-red sky
```

---

## Never-Ending Narrative Cycles

### Cascade Pattern

```
Normal World
    ↓ (World boss conquers)
Apocalypse State
    ↓ (Counter-event succeeds)
Cleansed World OR Hybrid State
    ↓ (New world boss emerges)
New Apocalypse (different type)
    ↓ (Counter-event succeeds)
...infinitely
```

### Emergent Storytelling Examples

**Story 1: "The Fallen's Reign"**
1. Player throws chaotic warrior Korgath → Fallen Star Demon (Year 1)
2. Korgath conquers world over 10 years → Demon Hellscape (Year 11)
3. Demon civilization flourishes, monuments built (Years 11-15)
4. Angel spawns, begins cleansing (Year 15 Day 120)
5. Player empowers demon lord Zargoth as champion (Year 15 Day 150)
6. Zargoth defeats angel, preserves hellscape (Year 15 Day 200)
7. World remains demonic, Zargoth becomes co-ruler with Korgath
8. New threat: Ice elemental world boss spawns in frozen north (Year 20)
9. Demons ally with player to fight ice invasion (unexpected twist)

**Story 2: "Eternal Conflict"**
1. Lich apocalypse → Undead Wasteland
2. Life priests cleanse 50% → Hybrid State (living south, undead north)
3. Border wars for centuries
4. Neither side wins completely
5. Demon world boss spawns in living south
6. Lich and Life priests ALLY against demon threat (enemy of my enemy)
7. Triple faction world (undead, living, demons)

**Story 3: "The Purge Cycle"**
1. Angel purge apocalypse → Celestial Dominion (totalitarian heaven)
2. Mortal rebel hero spawns → Fights for freedom
3. Hero wins, banishes angels → Normal world restored
4. World prospers peacefully for 50 years
5. Nature apocalypse → Verdant Overgrowth (plants consume world)
6. Druids restore balance → Hybrid State (wild zones + civilized zones)
7. Cycle repeats with new threats

---

## Design Philosophy: Self-Perpetuating Sandbox

**Core Principles:**
1. **No end state** - Every apocalypse has counter
2. **No player required** - Cycles happen automatically (player can intervene)
3. **Criteria-driven** - Events trigger based on world state checks
4. **Faction diversity** - Each apocalypse type creates unique content
5. **Asymmetric balance** - Counter-threats tailored to specific apocalypse
6. **Recursive depth** - Counter-events can themselves be threatened

**Gameplay Implications:**
- Player can **observe** cycles without intervening
- Player can **accelerate** apocalypse by backing world boss
- Player can **delay** apocalypse by raising champions to fight world boss
- Player can **switch sides** mid-cycle (support demons then switch to angels)
- Multiple playstyles: Chaos agent, balance keeper, faction loyalist, opportunist

---

## Performance Considerations (DOTS)

### Biome Conversion
- **Batch updates:** Convert chunks of cells per frame (not all at once)
- **Spatial queries:** Use spatial grid for radius checks (O(1) per cell)
- **Job parallelization:** Biome conversion jobs can run in parallel
- **LOD system:** Distant regions update less frequently

### Apocalypse Agent Spreading
- **Component lookups:** Cache for ApocalypseAgent, RegionalState
- **Update frequency:** Corruption spreads at 1 Hz, not per frame
- **Burst compilation:** All systems burst-compatible

### Counter-Event Spawning
- **Infrequent checks:** Daily rolls (1/day), not per-frame
- **Singleton pattern:** World state singleton avoids query overhead
- **Event queue:** Schedule spawns via buffer (cheap iteration)

### Entity Budgets
- **Apocalypse entities:** Each apocalypse type adds ~500-2000 entities (demons, structures, etc.)
- **Counter-entities:** Single hero or small groups (1-5 entities)
- **Performance target:** Support 3000+ total entities (normal + apocalypse content)

---

## Tuning Parameters

```csharp
public struct ApocalypseConfig
{
    // Trigger thresholds
    public float ApocalypseTerritoryThreshold;      // 0.8 (80% control)
    public uint ApocalypseTimeSurvivalYears;        // 10 years unchallenged

    // Counter-event spawn
    public float CounterEventBaseDailyChance;       // 0.005 (0.5%)
    public float CounterEventDailyAccumulation;     // 0.001 (+0.1% per day)
    public float CounterEventPopulationMultiplier;  // (PopCount / 500) * 0.5

    // Biome conversion
    public float CorruptionRatePerSecond;           // 0.01 (1% per second)
    public float CleansingRatePerSecond;            // 0.02 (2% per second, faster healing)

    // Angelic slayer
    public float AngelHealth;                       // 5000
    public float AngelDamageVsDemons;               // 200
    public float AngelCleansingRadius;              // 10m

    // Demonic champion
    public float ChampionHealth;                    // 3000
    public float ChampionDamageVsAngels;            // 150

    // Hybrid state
    public float HybridStateThreshold;              // 0.3 (30% control splits world)
}
```

---

## Open Questions

1. **Multiple simultaneous apocalypses:** Can demon hellscape and undead wasteland coexist initially?
2. **Counter-event failure:** What if counter-event loses? Does apocalypse become permanent?
3. **Player-triggered apocalypse:** Should player be able to directly trigger apocalypse (skip world boss)?
4. **Apocalypse monuments:** Do demon monuments grant gameplay bonuses or purely aesthetic?
5. **Cross-faction diplomacy:** Can demons and mortals ally against angels in hybrid states?
6. **Apocalypse tech trees:** Should each apocalypse type have unique progression systems?

---

## Related Documentation

- **Origin:** `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md` - World Bosses section
- **Ascension:** `Docs/Concepts/Villagers/Villager_Ascension_System.md` - How world bosses are born
- **Environment:** `Docs/Concepts/Meta/Generalized_Environment_Framework.md` - Biome systems
- **Combat:** `Docs/Concepts/Villagers/Bands_Vision.md` - Champion bands vs threats

---

**For Implementers:** Start with single apocalypse type (Demon Hellscape) + one counter-event (Angelic Slayer) as proof-of-concept.
**For Designers:** Balance counter-event spawn rates to create drama without trivializing apocalypse (apocalypse should persist for meaningful time).
**For Narrative:** Each apocalypse → counter-event → hybrid state creates unique story arcs with lasting consequences and player choice points.
