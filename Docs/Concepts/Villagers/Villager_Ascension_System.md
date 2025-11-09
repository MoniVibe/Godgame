# Villager Ascension System

**Status:** Design
**Category:** System - Emergent Threats
**Scope:** World Bosses, Celestial Beings, Narrative Events
**Created:** 2025-11-03
**Last Updated:** 2025-11-03

---

## Purpose

Enable emergent transformation of villagers into celestial beings or world bosses through special ascension paths triggered by divine actions, creating deeply personal narrative consequences where player cruelty or heroism births lasting threats or allies.

**Primary Goal:** Transform ordinary villagers into extraordinary threats/allies through criteria-driven ascension mechanics.
**Secondary Goals:**
- Create self-inflicted nemeses (throw a warrior → create demon who remembers and hunts you)
- Retain villager identity through transformation (name, memory, relationships preserved)
- Support both singular and collective ascension paths
- Provide alignment feedback for divine actions that trigger transformations

---

## Core Mechanic: Fallen Star Demon

### Trigger Conditions

**Who can transform:**
Villagers meeting **all core requirements** with weighted RNG check:
- **Chaotic** alignment (evil chaos is most prone, but neutral chaos also qualifies)
- **Warlike** outlook (warrior mentality channels rage)
- **High Belief/Faith** (≥50+) in the deity throwing them (divine connection corrupts)

**What triggers the roll:**
- God **throws villager** via divine hand (slingshot mechanic)
- Villager travels **minimum distance** (50m+ tunable threshold)
- RNG weighted by **Physique stat** (stronger bodies channel more corruption)
- **Battle context bonus:** Plucking from active combat triples transformation odds

**When it resolves:**
- Immediate: Roll occurs at throw moment
- Delayed: If roll succeeds, villager vanishes on impact; transformation completes **one year later** as meteor event

---

## DOTS 1.4 Implementation Architecture

### Component Schema

```csharp
namespace Godgame.Ascension
{
    /// <summary>
    /// Tracks villager's potential for ascension transformations.
    /// Attached to eligible villagers during initialization or when traits update.
    /// </summary>
    public struct AscensionPotential : IComponentData
    {
        // Trait scores (0-100)
        public byte PhysiqueScore;
        public byte ChaoticScore;
        public byte WarlikeScore;
        public byte FaithScore;

        // Computed probability (cached for performance)
        public float TransformationChance;  // 0-1 probability

        // Type of ascension eligible for
        public FixedString64Bytes AscensionType;  // "FallenStarDemon", "AscendedAngel", etc.

        // Cached original identity for retention
        public FixedString64Bytes VillagerName;
        public uint VillagerIdValue;  // Original VillagerId for tracking
    }

    /// <summary>
    /// Marks villager as undergoing ascension after successful roll.
    /// Entity remains in world for visual sequence, then is despawned.
    /// </summary>
    public struct AscensionTriggered : IComponentData
    {
        public float3 ImpactPosition;      // Where villager landed
        public float3 ImpactVelocity;      // Throw velocity at impact
        public Entity ThrowingDeity;       // Which god caused this

        // Context that influenced roll
        public bool WasThrownFromBattle;   // Battle bonus applied?
        public float DistanceTraveled;     // Distance thrown

        public uint TriggerTick;           // When roll succeeded
    }

    /// <summary>
    /// Scheduled meteor event that will spawn the demon after delay.
    /// Lives on a singleton or event queue entity.
    /// </summary>
    public struct ScheduledDemonSpawn : IBufferElementData
    {
        public uint SpawnTick;             // World tick when meteor arrives (1 year delay)
        public float3 SpawnPosition;       // Where meteor will strike

        // Preserved identity from original villager
        public FixedString64Bytes DemonName;
        public uint SourceVillagerId;
        public Entity CausingDeity;

        // Transformation context for lore/telemetry
        public bool BornFromBattle;
        public byte OriginalPhysique;
    }

    /// <summary>
    /// Core celestial being component after transformation completes.
    /// </summary>
    public struct CelestialBeing : IComponentData
    {
        public enum Type : byte
        {
            None,
            RampagingDemon,    // Fallen Star path
            ProtectiveAngel,   // Ascended hero path
            VengefulWraith,    // Betrayal path
            CorruptedLich,     // Scholar path
            // Extensible
        }

        public Type CelestialType;

        // Identity retention
        public FixedString64Bytes Name;            // Original villager name
        public FixedString64Bytes OriginStory;     // "Fallen Warrior", "Betrayed Scholar"

        // Causality tracking
        public Entity SourceVillagerEntity;        // Original entity (may be invalid after despawn)
        public uint SourceVillagerId;              // Original ID for telemetry
        public Entity CausingDeity;                // God responsible for transformation

        // Power metrics
        public float PowerLevel;                   // 0-1: Celestial → World Boss threshold at 0.8+
        public byte ThreatTier;                    // 1-10 threat classification

        // Memory system (optional extension)
        public Entity TargetVillage;               // Specific village to attack (vengeance)
        public byte VengeanceIntensity;            // 0-100: Drive to punish causing deity

        public uint TransformTick;
    }

    /// <summary>
    /// Escalated celestial beings become world bosses at power threshold.
    /// </summary>
    public struct WorldBoss : IComponentData
    {
        public FixedString64Bytes BossName;        // "Korgath the Fallen"
        public FixedString64Bytes Title;           // "Demon of Wrath"

        public float3 TerritoryCenter;
        public float TerritoryRadius;              // Aggro/patrol zone

        public uint SpawnTick;

        // Loot and rewards
        public BlobAssetReference<LootTableBlob> LootTable;
        public bool DropsGodJewelry;               // Unique world boss reward
    }

    /// <summary>
    /// Combat stats for celestial beings (much stronger than villagers).
    /// </summary>
    public struct CelestialCombatStats : IComponentData
    {
        public float Health;
        public float MaxHealth;
        public float Damage;
        public float AttackSpeed;
        public float MoveSpeed;

        // Special properties
        public bool CanFly;
        public bool ImmuneToMortalWeapons;         // Only champions can harm
        public float AreaDamageRadius;             // Splash attacks
    }
}
```

---

## System Flow: Detection & Roll

```csharp
namespace Godgame.Ascension.Systems
{
    /// <summary>
    /// Detects throw events on villagers with AscensionPotential and rolls for transformation.
    /// Runs AFTER HandThrowSystem resolves throw physics.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HandThrowSystem))]
    public partial struct AscensionDetectionSystem : ISystem
    {
        private ComponentLookup<VillagerCombatStats> _combatLookup;

        public void OnCreate(ref SystemState state)
        {
            _combatLookup = state.GetComponentLookup<VillagerCombatStats>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _combatLookup.Update(ref state);

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var currentTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;

            // Query villagers with ascension potential who were just thrown
            foreach (var (potential, villagerId, transform, entity) in SystemAPI
                .Query<RefRO<AscensionPotential>, RefRO<VillagerId>, RefRO<LocalTransform>>()
                .WithAll<ThrownByHand>()  // Tag component added by HandThrowSystem
                .WithNone<AscensionTriggered>()
                .WithEntityAccess())
            {
                var throwData = state.EntityManager.GetComponentData<ThrownByHand>(entity);

                // Calculate throw distance
                float distanceTraveled = math.distance(throwData.LaunchPosition, transform.ValueRO.Position);

                // Check minimum distance threshold
                const float MinTransformDistance = 50f;  // Tunable
                if (distanceTraveled < MinTransformDistance)
                    continue;

                // Check if thrown from battle (look for combat state)
                bool inBattle = false;
                if (_combatLookup.HasComponent(entity))
                {
                    var combatStats = _combatLookup[entity];
                    inBattle = combatStats.IsInCombat;  // Assumes combat system tracks this
                }

                // Calculate transformation probability
                float chance = CalculateTransformationChance(
                    potential.ValueRO.PhysiqueScore,
                    potential.ValueRO.ChaoticScore,
                    potential.ValueRO.WarlikeScore,
                    potential.ValueRO.FaithScore,
                    inBattle,
                    distanceTraveled
                );

                // Roll for transformation
                var random = Random.CreateFromIndex((uint)(entity.Index + currentTick));
                float roll = random.NextFloat();

                if (roll < chance)
                {
                    // TRANSFORMATION TRIGGERED!
                    ecb.AddComponent(entity, new AscensionTriggered
                    {
                        ImpactPosition = transform.ValueRO.Position,
                        ImpactVelocity = throwData.LaunchVelocity,
                        ThrowingDeity = throwData.ThrownByEntity,
                        WasThrownFromBattle = inBattle,
                        DistanceTraveled = distanceTraveled,
                        TriggerTick = currentTick
                    });

                    // Schedule demon spawn for 1 year later
                    ScheduleDemonSpawn(ref state, ref ecb, entity, potential.ValueRO,
                        transform.ValueRO.Position, throwData.ThrownByEntity,
                        inBattle, currentTick);

                    // Apply alignment consequence to deity
                    ApplyDeityAlignmentShift(ref state, ref ecb, throwData.ThrownByEntity, -40); // Evil

                    // Broadcast telemetry event
                    // "Korgath was thrown and corruption took hold..."
                }
            }
        }

        private float CalculateTransformationChance(
            byte physique, byte chaotic, byte warlike, byte faith,
            bool inBattle, float distanceThrown)
        {
            float baseChance = 0.01f;  // 1% base

            // Physique is squared for dramatic scaling (strong bodies = much higher odds)
            float physiqueWeight = (physique / 100f);
            physiqueWeight *= physiqueWeight;  // Squared

            // Other traits linear
            float chaoticWeight = chaotic / 100f;
            float warlikeWeight = warlike / 100f;
            float faithWeight = faith / 100f;

            // Battle bonus is massive (3x)
            float battleMultiplier = inBattle ? 3f : 1f;

            // Distance bonus (up to 2.5x at 250m+)
            float distanceMultiplier = math.clamp(distanceThrown / 100f, 1f, 2.5f);

            // Final calculation
            return baseChance * physiqueWeight * chaoticWeight * warlikeWeight * faithWeight
                * battleMultiplier * distanceMultiplier;
        }

        private void ScheduleDemonSpawn(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity villagerEntity, AscensionPotential potential, float3 position,
            Entity deity, bool fromBattle, uint currentTick)
        {
            // 1 year delay = 365 days * 24 hours * 60 minutes * 60 seconds * 60 ticks/sec
            // Simplify: 1 year = 365 * 10 in-game days if day = 10 game-seconds
            const uint OneYearTicks = 365 * 10 * 60;  // Tunable based on your time scale

            uint spawnTick = currentTick + OneYearTicks;

            // Get or create scheduled spawn queue singleton
            Entity queueEntity = GetScheduledSpawnQueue(ref state);

            var spawnBuffer = state.EntityManager.GetBuffer<ScheduledDemonSpawn>(queueEntity);
            spawnBuffer.Add(new ScheduledDemonSpawn
            {
                SpawnTick = spawnTick,
                SpawnPosition = position,
                DemonName = potential.VillagerName,  // Retain original name
                SourceVillagerId = potential.VillagerIdValue,
                CausingDeity = deity,
                BornFromBattle = fromBattle,
                OriginalPhysique = potential.PhysiqueScore
            });

            // Mark villager for despawn (they vanish into crater after impact)
            ecb.AddComponent<DespawnAfterImpactVFX>(villagerEntity);
        }

        private Entity GetScheduledSpawnQueue(ref SystemState state)
        {
            // Singleton pattern for spawn queue
            var query = SystemAPI.QueryBuilder().WithAll<ScheduledDemonSpawn>().Build();
            if (query.IsEmpty)
            {
                // Create singleton
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddBuffer<ScheduledDemonSpawn>(entity);
                return entity;
            }
            return query.GetSingletonEntity();
        }

        private void ApplyDeityAlignmentShift(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity deity, int moralShift)
        {
            // Apply to god alignment if system exists
            if (!state.EntityManager.HasComponent<GodAlignment>(deity))
                return;

            var alignment = state.EntityManager.GetComponentData<GodAlignment>(deity);
            alignment.MoralAxis += moralShift;  // -40 = severe evil shift
            alignment.ChaoticAxis += math.abs(moralShift) / 3;  // Also chaotic
            ecb.SetComponent(deity, alignment);
        }
    }
}
```

---

## System Flow: Scheduled Meteor Spawn

```csharp
namespace Godgame.Ascension.Systems
{
    /// <summary>
    /// Processes scheduled demon spawns after 1-year delay.
    /// Spawns meteor event, then creates demon entity.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ScheduledDemonSpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = (uint)state.WorldUnmanaged.Time.ElapsedTime;
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Get spawn queue
            var queueQuery = SystemAPI.QueryBuilder().WithAll<ScheduledDemonSpawn>().Build();
            if (queueQuery.IsEmpty)
                return;

            var queueEntity = queueQuery.GetSingletonEntity();
            var spawnBuffer = state.EntityManager.GetBuffer<ScheduledDemonSpawn>(queueEntity);

            // Process due spawns (reverse iterate for safe removal)
            for (int i = spawnBuffer.Length - 1; i >= 0; i--)
            {
                var spawn = spawnBuffer[i];

                if (currentTick >= spawn.SpawnTick)
                {
                    // TIME TO SPAWN DEMON!

                    // 1. Spawn meteor VFX entity (visual herald)
                    SpawnMeteorEvent(ref state, ref ecb, spawn.SpawnPosition);

                    // 2. Create demon entity
                    Entity demonEntity = CreateDemonEntity(ref state, ref ecb, spawn);

                    // 3. Broadcast telemetry
                    // "A meteor has struck! Korgath the Fallen emerges!"

                    // Remove from queue
                    spawnBuffer.RemoveAt(i);
                }
            }
        }

        private void SpawnMeteorEvent(ref SystemState state, ref EntityCommandBuffer ecb, float3 position)
        {
            // Create meteor VFX entity
            Entity meteor = ecb.CreateEntity();
            ecb.AddComponent(meteor, new LocalTransform
            {
                Position = position + new float3(0, 100, 0),  // Start in sky
                Rotation = quaternion.identity,
                Scale = 5f
            });
            ecb.AddComponent<MeteorVFX>(meteor);  // Visual component
            ecb.AddComponent<MeteorFallPhysics>(meteor);  // Falls to ground, explodes

            // Meteor creates crater/explosion on impact (handled by MeteorSystem)
        }

        private Entity CreateDemonEntity(ref SystemState state, ref EntityCommandBuffer ecb,
            ScheduledDemonSpawn spawn)
        {
            // Create new entity for demon
            Entity demon = ecb.CreateEntity();

            // Core identity
            ecb.AddComponent(demon, new CelestialBeing
            {
                CelestialType = CelestialBeing.Type.RampagingDemon,
                Name = spawn.DemonName,  // RETAINS ORIGINAL VILLAGER NAME
                OriginStory = new FixedString64Bytes("Fallen Warrior"),
                SourceVillagerId = spawn.SourceVillagerId,
                CausingDeity = spawn.CausingDeity,
                PowerLevel = 0.3f,  // Starts as powerful celestial, not full boss yet
                ThreatTier = 6,
                TargetVillage = Entity.Null,  // Will select target based on memory
                VengeanceIntensity = 80,  // High drive to punish deity
                TransformTick = (uint)state.WorldUnmanaged.Time.ElapsedTime
            });

            // Combat stats (MUCH stronger than villagers)
            ecb.AddComponent(demon, new CelestialCombatStats
            {
                Health = 2000f,  // vs villager ~100
                MaxHealth = 2000f,
                Damage = 80f,  // vs villager ~5-10
                AttackSpeed = 1.2f,
                MoveSpeed = 7f,
                CanFly = true,
                ImmuneToMortalWeapons = true,  // Only champions can harm
                AreaDamageRadius = 5f  // Splash attacks
            });

            // Spatial
            ecb.AddComponent(demon, new LocalTransform
            {
                Position = spawn.SpawnPosition,
                Rotation = quaternion.identity,
                Scale = 4f  // Demons are large
            });

            // AI
            ecb.AddComponent(demon, new CelestialAI
            {
                CurrentState = CelestialAI.State.Seeking,  // Hunts for target
                TargetType = CelestialAI.TargetPreference.CausingDeityVillages,
                AggressionLevel = 0.95f
            });

            // Visual
            ecb.AddComponent<DemonRenderData>(demon);  // Links to demon mesh/materials

            return demon;
        }
    }
}
```

---

## System Flow: Celestial Escalation to World Boss

```csharp
namespace Godgame.Ascension.Systems
{
    /// <summary>
    /// Monitors celestial beings and escalates them to world boss status when power threshold reached.
    /// Power increases through kills, destruction, stolen worship, etc.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CelestialEscalationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Check all celestial beings not yet world bosses
            foreach (var (celestial, combatStats, transform, entity) in SystemAPI
                .Query<RefRW<CelestialBeing>, RefRO<CelestialCombatStats>, RefRO<LocalTransform>>()
                .WithNone<WorldBoss>()
                .WithEntityAccess())
            {
                // Power grows through actions (kills increment power elsewhere)
                const float WorldBossThreshold = 0.8f;

                if (celestial.ValueRO.PowerLevel >= WorldBossThreshold)
                {
                    // ESCALATE TO WORLD BOSS

                    var bossName = celestial.ValueRO.Name;  // Keep demon name
                    var title = GenerateTitle(celestial.ValueRO);  // "The Fallen", "Demon of Wrath"

                    ecb.AddComponent(entity, new WorldBoss
                    {
                        BossName = bossName,
                        Title = title,
                        SpawnTick = (uint)state.WorldUnmanaged.Time.ElapsedTime,
                        TerritoryCenter = transform.ValueRO.Position,
                        TerritoryRadius = 150f,  // Large threat zone
                        // LootTable assigned based on boss tier
                        DropsGodJewelry = true
                    });

                    // Buff stats to world boss tier
                    ecb.SetComponent(entity, new CelestialCombatStats
                    {
                        Health = combatStats.ValueRO.Health * 3f,
                        MaxHealth = combatStats.ValueRO.MaxHealth * 3f,
                        Damage = combatStats.ValueRO.Damage * 2.5f,
                        AttackSpeed = combatStats.ValueRO.AttackSpeed * 1.2f,
                        MoveSpeed = combatStats.ValueRO.MoveSpeed * 1.1f,
                        CanFly = combatStats.ValueRO.CanFly,
                        ImmuneToMortalWeapons = true,
                        AreaDamageRadius = combatStats.ValueRO.AreaDamageRadius * 2f
                    });

                    // Increase threat tier
                    celestial.ValueRW.ThreatTier = 10;  // Max threat

                    // Broadcast world event
                    // "Korgath the Fallen has become a World Boss!"
                }
            }
        }

        private FixedString64Bytes GenerateTitle(CelestialBeing celestial)
        {
            // Generate based on origin and type
            return celestial.CelestialType switch
            {
                CelestialBeing.Type.RampagingDemon => new FixedString64Bytes("The Fallen"),
                CelestialBeing.Type.ProtectiveAngel => new FixedString64Bytes("The Ascended"),
                CelestialBeing.Type.VengefulWraith => new FixedString64Bytes("The Betrayed"),
                _ => new FixedString64Bytes("The Transformed")
            };
        }
    }
}
```

---

## DOTS Best Practices Applied

### Entity Recycling
- **Villager → Demon:** Entity is NOT destroyed and recreated
- Component swap: Remove villager components, add celestial components
- Preserves entity references in other systems
- **Performance:** No entity churn, no fragmentation

### Burst Compatibility
- All systems `[BurstCompile]` compatible
- No managed strings (use `FixedString64Bytes`)
- Random via `Random.CreateFromIndex` (deterministic)
- Math via `Unity.Mathematics`

### Delayed Events Pattern
- **Buffer-based queue:** `ScheduledDemonSpawn` buffer on singleton
- Tick-based scheduling avoids coroutines
- Systems check queue each frame (cheap iteration)
- Scalable: Hundreds of scheduled events no problem

### Component Lookups
- Cache `ComponentLookup<T>` in `OnCreate`
- Update in `OnUpdate` before use
- Read-only when possible for job scheduling flexibility

### Entity Command Buffer Usage
- Single ECB per system from `EndSimulationEntityCommandBufferSystem`
- Structural changes deferred until group boundary
- Playback order deterministic

### Registry Integration
- Celestial beings auto-publish to `CelestialRegistry` via existing bridge pattern
- No special telemetry code needed
- Sync systems handle component → registry mapping

---

## Extensibility: Adding New Ascension Paths

### Framework Pattern

1. **Define Criteria Component:**
   ```csharp
   struct AscendedAngelPotential : IComponentData
   {
       byte LawfulScore;
       byte GoodScore;
       byte FaithScore;
       byte HeroismScore;
       float TransformationChance;
   }
   ```

2. **Add Detection Logic:**
   - Check trigger condition (e.g., dies heroically saving allies)
   - Roll vs `TransformationChance`
   - Schedule spawn with appropriate delay

3. **Define Spawn Behavior:**
   - Different VFX (divine light vs meteor)
   - Different stats (angel is defensive, demon is offensive)
   - Different AI (angel protects villages, demon attacks them)

4. **Reuse Core Systems:**
   - `CelestialBeing` component supports all types via enum
   - `CelestialEscalationSystem` works for any celestial
   - `WorldBoss` component type-agnostic

### Collective Ascension Example

**Concept:** Entire village transforms into undead legion
```csharp
struct CollectiveAscensionPotential : IComponentData
{
    Entity VillageEntity;  // Which village can transform
    float CollectiveDespairLevel;  // 0-1: Mass suffering threshold
    uint CursedByDeity;    // Which god cursed them
}

// Trigger: Village despair > 0.9 + cursed by evil deity
// Result: All villagers → skeleton warriors, village → undead fortress
```

---

## Tuning Parameters (Data-Driven)

**Create scriptable asset or blob for tuning:**

```csharp
public struct AscensionConfig
{
    // Fallen Star Demon
    public float FallenDemonBaseChance;          // 0.01
    public float FallenDemonPhysiqueExponent;   // 2.0 (squared)
    public float FallenDemonBattleMultiplier;   // 3.0
    public float FallenDemonMinDistance;        // 50m
    public uint FallenDemonDelayTicks;          // 1 year

    // Ascended Angel
    public float AscendedAngelBaseChance;       // 0.05
    public float AscendedAngelHeroismWeight;    // 1.5
    public uint AscendedAngelDelayTicks;        // 3 days

    // Celestial → World Boss
    public float WorldBossEscalationThreshold;  // 0.8
    public float WorldBossPowerGainPerKill;     // 0.05
    public float WorldBossPowerGainPerVillageDestroyed;  // 0.2

    // Alignment consequences
    public int CreateDemonAlignmentPenalty;     // -40 (evil)
    public int CreateAngelAlignmentBonus;       // +30 (good)
}
```

Load via `BlobAssetReference<AscensionConfigBlob>` for burst-safe access.

---

## Telemetry & Events

**Key Metrics:**
- `AscensionAttempts` - How many rolls occurred
- `AscensionSuccesses` - How many transformed
- `ActiveCelestials` - Current celestial being count
- `CelestialsEscalated` - How many became world bosses
- `DemonKillsByDeity` - Which gods created the most demons

**Narrative Events:**
- "Korgath was thrown by [Deity] and corruption took hold"
- "A year has passed... A meteor approaches!"
- "Korgath the Fallen has emerged as a Rampaging Demon"
- "Korgath the Fallen has become a World Boss!"
- "Champion [Name] has slain Korgath the Fallen"

---

## Open Questions

1. **Power Growth Rate:** How fast do celestials gain power toward world boss threshold?
   - Kills? Destruction? Time alive? Worship stolen?

2. **Memory Specificity:** Should demons remember individual villagers they knew?
   - Could target former friends/family for extra drama

3. **Multiple Simultaneous Transforms:** Can multiple villagers thrown at once all transform?
   - Creates demon army scenario

4. **Redemption Arc:** Can a demon be purified back into a villager/angel?
   - Requires champion intervention + miracle?

5. **Collective Transform Triggers:** What causes entire villages to ascend?
   - Mass sacrifice, plague, divine abandonment, apocalypse events?

---

## Related Documentation

- **Origin:** `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md` - World Bosses & Celestial Beings section
- **Truth Sources:** Will need `CelestialBeing`, `WorldBoss`, `AscensionPotential` added to inventory
- **Alignment:** `Docs/Concepts/Meta/Generalized_Alignment_Framework.md` - Alignment consequence patterns
- **Bands:** `Docs/Concepts/Villagers/Bands_Vision.md` - Champion bands hunting world bosses

---

**For Implementers:** Start with Fallen Star Demon as proof-of-concept, then extend to other ascension paths using same framework.
**For Designers:** Tune probability formulas and delay timings to create dramatic pacing without overwhelming players with threats.
**For Narrative:** Leverage preserved identity (name, memory, relationships) to create personal emergent stories where player cruelty has lasting consequences.
