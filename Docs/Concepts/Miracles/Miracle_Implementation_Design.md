# Miracle Implementation Design

**Status:** Design Concept / Implementation Guide
**Category:** Core - Miracles / Divine Powers
**Audience:** Implementers / Architects / Designers
**Created:** 2025-01-17
**Last Updated:** 2025-01-17

---

## Executive Summary

**Purpose:** Implement miracles as data-driven "payload + delivery + effect program" system, so adding 50 more miracles is mostly content, not new code. Inspired by Black & White 2 patterns with reusable building blocks.

**Key Principle:** Separate content (MiracleSpec) from runtime (MiracleInstance) and delivery systems (Throw/Beam/Zone/Aura) from effect ops (Damage/Heal/Terrain/etc.). This enables rapid content creation without code changes.

**Reference:** [Black and White Wiki](https://blackandwhite.fandom.com/wiki/Miracles)

---

## 1. Copy Proven Black & White Patterns into 4 Cast Archetypes

**Public BW2 descriptions show these delivery styles clearly:**

### A) Throw → "Ball" That Detonates in Area

**Example:** Heal thrown is a ball that explodes and heals in radius.

**Pattern:** Miracle orb is thrown, detonates on impact/time fuse, applies effects in area.

### B) Pour / Beam → Sustained Stream Under Hand

**Example:** Heal poured is a stream that heals whatever it touches.

**Pattern:** Miracle beam instance attached to hand, raycasts each tick, applies effects along ray/point.

### C) Bouncing / Persistent Projectile → Multiple Bounces Then Burst

**Example:** Lightning thrown bounces/zaps then ends in final burst.

**Pattern:** Projectile with bounce behavior, applies effects on each bounce, final burst on last bounce/timeout.

### D) Target Zone / Delayed Strike → Zone Created, Then Barrage Falls

**Example:** Meteor throw creates target zone and then barrage falls.

**Pattern:** Zone spawns first (targeting), then periodic strikes/projectiles spawn within zone.

### E) Sustained Area → Maintained Magic with Constant Cost

**Example:** Storm exists as "maintained" magic (constant cost to keep active).

**Pattern:** Aura/volume spawns, drains mana per tick, stops when out of mana/canceled.

**Reference:** [Black and White Wiki - Miracles](https://blackandwhite.fandom.com/wiki/Miracles)

---

## 2. Miracle = Spec (Content) + Instance (Runtime)

### MiracleSpec (Blob / Catalog)

**Minimal fields:**

```csharp
/// <summary>
/// Miracle specification (content data, loaded from catalog).
/// </summary>
public struct MiracleSpec
{
    public MiracleId Id;                      // Unique identifier
    public CastArchetype Archetype;           // ThrowBomb | Beam | Zone | Aura
    public DeliveryMethod Delivery;           // HandThrow | CreatureCast | Orbital | Trap | RemoteDetonate
    
    // Cost model
    public float UpfrontCost;                 // Initial cost
    public float PerTickCost;                 // Sustained cost per tick (for sustain miracles)
    public IntensityScalingCurve CostCurve;   // Cost scaling with intensity
    
    // Cooldown and limits
    public float CooldownSeconds;
    public byte MaxConcurrent;                // Max simultaneous instances
    
    // Effect program
    public BlobArray<EffectOp> Ops;          // List of effect operations
    
    // Presentation
    public BlobArray<FixedString64Bytes> VFXIds;      // VFX asset IDs
    public BlobArray<FixedString64Bytes> SoundIds;    // Sound asset IDs
    public BlobArray<FixedString64Bytes> DecalIds;    // Decal asset IDs
}

/// <summary>
/// Cast archetype (delivery style).
/// </summary>
public enum CastArchetype : byte
{
    ThrowBomb = 0,      // Thrown orb, detonates on impact
    Beam = 1,           // Sustained stream under hand
    Bouncing = 2,       // Bounces/zaps then burst
    Zone = 3,           // Target zone, delayed strike
    Aura = 4            // Sustained area with per-tick cost
}

/// <summary>
/// Delivery method (how miracle is cast).
/// </summary>
public enum DeliveryMethod : byte
{
    HandThrow = 0,      // Player throws via hand
    CreatureCast = 1,   // Creature casts
    Orbital = 2,        // From orbit/above
    Trap = 3,           // Triggered trap
    RemoteDetonate = 4  // Remote activation
}
```

### MiracleInstance (Entity)

**Runtime state:**

```csharp
/// <summary>
/// Runtime state of an active miracle instance.
/// </summary>
public struct MiracleInstance : IComponentData
{
    public MiracleId MiracleId;        // Reference to spec
    public Entity Caster;              // Caster entity
    public float3 TargetPosition;      // Target position (or Entity.Null for entity target)
    public Entity TargetEntity;        // Target entity (optional)
    
    public uint StartTick;             // Tick when cast started
    public uint EndTick;               // Tick when effect ends
    public uint CurrentTick;           // Current tick (for per-tick updates)
    
    public float Intensity01;          // Intensity (0-1), affects radius/duration/strength
    
    public MiracleInstanceState State;  // Primed | InFlight | Active | Ending
    
    // Budget (per-tick limits for performance)
    public ushort MaxAffectedEntities;  // Max entities affected per tick
    public ushort MaxCellEdits;         // Max terrain cells modified per tick
    public ushort MaxSpawnCount;        // Max entities spawned per tick
}

public enum MiracleInstanceState : byte
{
    Primed = 0,         // Queued/waiting to launch
    InFlight = 1,       // Projectile in flight
    Active = 2,         // Effect actively running
    Ending = 3          // Winding down
}
```

---

## 3. Delivery Systems (Few Systems, Many Miracles)

### A) Throwable "Miracle Orbs" (Pairs with Queued Throw)

**Miracle in hand = a pickable kinematic orb (like primed queue).**

**Integration with Pickup/Throw System:**
- Shift-throw queues it (uses primed throw queue)
- Hotkeys release next/all
- RMB cancels

**On launch:**
- Detach + apply impulse (or kinematic ballistic for rewind-stable)
- Add `MiracleInstance` with `State = InFlight`

**On impact/time fuse:**
- Emit `MiracleDetonateEvent` with position/radius/intensity
- Effect ops system processes event

**This directly matches "thrown ball explodes" miracles (Heal, Fireball).**

```csharp
/// <summary>
/// Miracle orb entity (pickable, throwable).
/// </summary>
public struct MiracleOrb : IComponentData
{
    public MiracleId MiracleId;
    public float Intensity01;
    public float FuseTimeSeconds;      // Time before auto-detonate (0 = on impact only)
}

/// <summary>
/// Event emitted when miracle orb detonates.
/// </summary>
public struct MiracleDetonateEvent : IComponentData
{
    public MiracleId MiracleId;
    public float3 Position;
    public float Radius;
    public float Intensity01;
    public Entity Caster;
}
```

---

### B) Beam / Pour

**Spawn `MiracleBeamInstance` attached to hand.**

**Each tick:**
- Raycast under cursor (from `HandCameraInputRouter` context)
- Apply ops along ray/point (heal/damage/terraform)

**This matches BW2 "poured stream" behavior.**

```csharp
/// <summary>
/// Sustained beam miracle instance.
/// </summary
public struct MiracleBeamInstance : IComponentData
{
    public Entity HandAnchor;          // Attached to hand
    public float3 RayStart;
    public float3 RayDirection;
    public float RayLength;
    public float TickRate;             // Updates per second
    public uint LastTickUpdate;
}
```

---

### C) Zone / Delayed Strike

**Spawn `MiracleZoneInstance` with radius + duration + schedule.**

**Example:** Meteor = zone spawns periodic "meteors" (events or projectiles).

```csharp
/// <summary>
/// Zone miracle instance (target zone with delayed/staged effects).
/// </summary>
public struct MiracleZoneInstance : IComponentData
{
    public float3 Center;
    public float Radius;
    public float DurationSeconds;
    public float StrikeIntervalSeconds;  // Time between strikes
    public uint StrikeCount;             // Total number of strikes
    public uint CurrentStrikeIndex;
    public uint NextStrikeTick;
}
```

---

### D) Aura / Sustained Area

**Spawn `MiracleAuraInstance` volume around target/area.**

**Each tick:**
- Drain mana per tick (check if caster has enough)
- Apply ops within volume
- Stop when out of mana / canceled

**This matches Storm-like sustained model.**

```csharp
/// <summary>
/// Sustained aura miracle instance.
/// </summary>
public struct MiracleAuraInstance : IComponentData
{
    public float3 Center;
    public float Radius;
    public float PerTickCost;
    public uint LastCostTick;
    public bool IsActive;               // False when out of mana/canceled
}
```

---

## 4. Effect Ops (The Reusable "Program")

**Keep ops small and composable; each op is handled by one Burst system:**

```csharp
/// <summary>
/// Effect operation (reusable building block).
/// </summary>
public struct EffectOp
{
    public EffectOpType Type;
    public float Magnitude;             // Strength/amount
    public float Radius;                // Area of effect
    public float Falloff;               // Falloff curve (0=linear, 1=inverse-square)
    public EffectTargetFilter TargetFilter;  // What entities to affect
    public BlobArray<byte> Payload;     // Type-specific data
}

public enum EffectOpType : byte
{
    DamageEntities = 0,
    HealEntities = 1,
    ApplyStatus = 2,
    Impulse = 3,
    ModifyField = 4,
    TerrainDelta = 5,
    SpawnEntities = 6
}
```

### Op Types

**OpDamageEntities (AOE, falloff):**
- Query entities in radius (spatial query)
- Apply damage scaled by distance falloff
- Respect `MaxAffectedEntities` budget

**OpHealEntities:**
- Query entities in radius (spatial query)
- Apply healing scaled by distance falloff

**OpApplyStatus (buff/debuff):**
- Query entities in radius
- Apply status effect component (with duration)
- Stacking rules (replace, stack, ignore)

**OpImpulse (push/pull/knock):**
- Query entities in radius
- Apply physics impulse (via `PhysicsVelocity.ApplyLinearImpulse`)
- Direction: away from center (push), toward center (pull), directional (knock)

**OpModifyField (moisture/mana/temperature/water level, etc.):**
- Query field cells in radius (environment field grid)
- Modify field values (additive or absolute)
- Mark dirty regions for incremental updates

**OpTerrainDelta (dig/raise/flatten within mask):**
- Query terrain cells in radius
- Apply terrain edits (decrease height, increase height, flatten)
- Mark dirty regions for nav/fluid updates

**OpSpawnEntities (summons, hazards, decals):**
- Spawn entities at positions (within radius)
- Respect `MaxSpawnCount` budget
- Use prefab/catalog for entity definition

**Important for scale:** Ops should run against:
- **Spatial queries with budgets** (never "scan all entities")
- **Environment fields (grids) with dirty regions** (never full grid updates)

---

## 5. Costs, Sustain, and "Increased Miracle" Scaling

**BW had "upgraded" versions of miracles (e.g., increased forms). Generalize this as:**

### Intensity Scaling

**Intensity slider (0-1) modifies:**
- Radius: `baseRadius × (1 + intensity × radiusMultiplier)`
- Duration: `baseDuration × (1 + intensity × durationMultiplier)`
- Tick rate: `baseTickRate × (1 + intensity × tickRateMultiplier)`
- Op strength: `baseMagnitude × (1 + intensity × magnitudeMultiplier)`
- Cost: `baseCost × costCurve(intensity)`

**Example Cost Curve:**
```csharp
public struct IntensityScalingCurve
{
    public CurveType Type;              // Linear, Exponential, Quadratic
    public float BaseMultiplier;        // At intensity 0
    public float MaxMultiplier;         // At intensity 1
}

// Cost at intensity: baseCost × lerp(BaseMultiplier, MaxMultiplier, curve(intensity))
```

### Sustain Miracles

**Use per-tick cost (like maintained storms):**
- Each tick: check if caster has enough mana/resources
- If yes: drain cost, continue effect
- If no: stop effect, transition to Ending state

**One-Shot Miracle Items (BW's bubble/dispenser idea):**

Miracles as inventory items you can pick/queue/throw:
- Create `MiracleItem` component (contains `MiracleId`)
- Can be stored in inventory, picked up, thrown
- On throw: spawn miracle orb with that miracle ID

---

## 6. Presentation (Cheap, Scalable)

**Miracles are "simulation ops"; visuals are separate:**

### Event-Driven VFX

**On key events (prime/launch/impact/tick pulses), emit presentation events:**

```csharp
/// <summary>
/// Presentation event for VFX/sound.
/// </summary>
public struct MiraclePresentationEvent : IComponentData
{
    public MiracleId MiracleId;
    public MiraclePresentationEventType Type;  // Prime, Launch, Impact, TickPulse
    public float3 Position;
    public float3 Direction;
    public float Intensity01;
}

public enum MiraclePresentationEventType : byte
{
    Prime = 0,
    Launch = 1,
    Impact = 2,
    TickPulse = 3
}
```

**Drive VFX Graph with events:**
- Unity VFX Graph explicitly supports event-triggered effects
- GPU-side simulation/culling for performance
- Events can carry position, direction, intensity

**Reference:** [Unity VFX Graph - Events](https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@latest/manual/Events.html)

### Material Overrides

**Use Entities Graphics material overrides for glow/tint states:**

```csharp
/// <summary>
/// Material override for miracle orb glow.
/// </summary>
public struct MiracleOrbGlow : IComponentData
{
    public float4 EmissionColor;
    public float Intensity;
}
```

**Reference:** [Unity Entities Graphics - Material Properties](https://docs.unity.cn/Packages/com.unity.entities.graphics@1.0/manual/material-properties.html)

---

## 7. Rewind-Friendly Notes

**If you want miracles compatible with rewind/fast-forward:**

### Treat Cast and Release as Tick-Stamped Commands

```csharp
/// <summary>
/// Command to cast miracle (rewind-safe).
/// </summary>
public struct MiracleCastCommand : IBufferElementData
{
    public MiracleId MiracleId;
    public Entity Caster;
    public float3 TargetPosition;
    public float Intensity01;
    public uint CommandTick;           // Tick when command was issued
    public uint ExecuteTick;           // Tick when command should execute
}
```

### Prefer Kinematic Projectiles

**For miracle orbs:**
- Use kinematic ballistic (deterministic integration + raycast)
- Avoid full physics (unless you snapshot physics state)
- Rewind-friendly: replay commands from history

**Example:**
```csharp
// Kinematic ballistic (deterministic)
public struct MiracleOrbKinematic : IComponentData
{
    public float3 Position;
    public float3 Velocity;
    public float3 Gravity;             // Constant gravity
    public uint StartTick;
}

// Each tick: integrate position deterministically
position += velocity * fixedDt;
velocity += gravity * fixedDt;
```

---

## MVP to Ship First (Small, Very BW-like)

### Phase 1: Core Archetypes

1. **ThrowBomb archetype** (Heal/Fireball-like)
   - Miracle orb (pickable, throwable)
   - Detonates on impact
   - Applies effects in radius

2. **Beam archetype** (pour heal/lightning stream)
   - Attached to hand
   - Raycast each tick
   - Apply effects along ray

3. **Zone archetype** (Meteor)
   - Zone spawns first
   - Periodic strikes within zone
   - Staged delivery

### Phase 2: Core Effect Ops

- **DamageEntities** (AOE damage with falloff)
- **HealEntities** (AOE healing with falloff)
- **ApplyStatus** (buff/debuff application)
- **ModifyField** (environment field modification)
- **TerrainDelta** (terrain dig/raise/flatten)

### Phase 3: Presentation

- **VFX event on prime/impact** (Unity VFX Graph events)
- Material overrides for miracle orb glow
- Sound events for launch/impact

**Once these exist, "add more miracles" becomes "add a new spec + maybe a new op," not a new bespoke system every time.**

---

## Implementation Checklist

### Data Structures

- [ ] `MiracleSpec` (Blob) with `CastArchetype`, `DeliveryMethod`, `EffectOp[]`, cost model
- [ ] `MiracleInstance` component (runtime state)
- [ ] `EffectOp` struct (reusable operations)
- [ ] `MiracleDetonateEvent`, `MiraclePresentationEvent` (events)

### Delivery Systems

- [ ] Throwable miracle orb system (integrates with primed throw queue)
- [ ] Beam/pour system (hand-attached, raycast per tick)
- [ ] Zone/delayed strike system (spawns zone, periodic strikes)
- [ ] Aura/sustained area system (volume with per-tick cost)

### Effect Op Systems

- [ ] `OpDamageEntitiesSystem` (AOE damage with budgets)
- [ ] `OpHealEntitiesSystem` (AOE healing with budgets)
- [ ] `OpApplyStatusSystem` (status effect application)
- [ ] `OpImpulseSystem` (physics impulse application)
- [ ] `OpModifyFieldSystem` (environment field modification)
- [ ] `OpTerrainDeltaSystem` (terrain edits with dirty regions)
- [ ] `OpSpawnEntitiesSystem` (entity spawning with budgets)

### Cost and Scaling

- [ ] Intensity scaling curve (radius/duration/strength/cost)
- [ ] Per-tick cost for sustain miracles
- [ ] One-shot miracle items (inventory integration)

### Presentation

- [ ] Event-driven VFX (Unity VFX Graph integration)
- [ ] Material overrides for miracle orbs
- [ ] Sound event system

### Rewind Support

- [ ] Tick-stamped commands (`MiracleCastCommand`)
- [ ] Kinematic ballistic projectiles (deterministic)
- [ ] Command replay system

---

## 8. Time Manipulation: Time Bubbles (Local Time Domains)

**Implement time manipulation (slow, stop, speedup, rewind) as time domains ("time bubbles") + limited local state history, not by touching global time.**

### Core Principle: Don't Use Time.timeScale

**Unity's `Time.timeScale` is global and changes the time step reported to Update/FixedUpdate (deltaTime, fixedDeltaTime), but doesn't slow execution itself.** For local miracles: keep your fixed-tick sim, and add per-entity/per-area local time.

**Reference:** [Unity Documentation - Time.timeScale](https://docs.unity3d.com/ScriptReference/Time-timeScale.html)

### 1. Core Model: TimeBubble → TimeWarped Entities

**Components (minimal):**

```csharp
/// <summary>
/// Time bubble entity (spatial time domain).
/// </summary>
public struct TimeBubble : IComponentData
{
    public float3 Center;
    public float Radius;                   // Sphere radius (or use shape component)
    public TimeBubbleMode Mode;            // Slow, Stop, Speedup, Rewind
    public float LocalScale;               // 0 = stop, <1 = slow, 1 = normal, >1 = speedup
    public uint DurationTicks;             // Lifetime
    public uint StartTick;
    public ushort DomainId;                // Unique domain ID
    public float FalloffRadius;            // Optional falloff (0 = hard edge)
}

public enum TimeBubbleMode : byte
{
    Slow = 0,
    Stop = 1,
    Speedup = 2,
    Rewind = 3
}

/// <summary>
/// Entity is affected by time warp (added by spatial query).
/// </summary>
public struct TimeWarped : IComponentData
{
    public ushort DomainId;
    public float LocalScale;               // Effective time scale for this entity
    public TimeBubbleMode Mode;
}

/// <summary>
/// Local clock per domain (singleton per DomainId, or per entity).
/// </summary>
public struct LocalClock : IComponentData
{
    public ushort DomainId;
    public float AccumQ;                   // Fixed-point accumulator (Q16.16 or similar)
    public uint RewindCursorTick;          // For rewind mode
    public byte MaxStepsPerGlobalTick;     // Clamp to avoid runaway
}
```

**Membership:** Use spatial grid to tag entities inside bubble as `TimeWarped` (spatial query each tick, or on bubble move).

### 2. How Local Time Advances (Burst-Friendly, Deterministic)

**Use a tick accumulator per domain:**

```csharp
// Per global tick, for each domain:
accum += localScale;                       // Fixed-point accumulation
int steps = (int)math.floor(accum);
accum -= steps;
steps = math.min(steps, MaxStepsPerGlobalTick);  // Clamp to avoid "spiral of death"
```

**Behavior:**
- **Slow:** `localScale < 1` → sometimes `steps = 0` (entity "skips" updates)
- **Stop:** `localScale = 0` → always `steps = 0`
- **Speedup:** `localScale > 1` → `steps >= 1` (multiple local steps per global tick)

**Clamp steps to avoid "spiral of death" style runaway catch-up.** This is the same stability reason fixed-timestep loops clamp frame/step budgets.

**Reference:** [Gaffer On Games - Fix Your Timestep!](https://gafferongames.com/post/fix_your_timestep/)

**Where to apply steps:**

Run only a small set of systems for warped entities:
- Cooldowns/timers
- Action progress (build/haul/cast)
- Kinematic locomotion (agents)
- Perception/decision (optional; often lower rate is enough)

**Unity Entities exposes rate managers** like `FixedRateSimpleManager` / variable rate managers for update control.

**Reference:** [Unity User Manual - Component System Groups](https://docs.unity3d.com/Packages/com.unity.entities@latest/manual/systems-system-groups.html)

### 3. Rewind Mode: Local Playback from Ring Buffer

**Don't "reverse physics." Just restore snapshots for affected entities.**

**State history (bounded):**

For each entity that can be rewound, store a ring buffer of a curated snapshot:

```csharp
/// <summary>
/// Ring buffer entry for rewind state.
/// </summary>
public struct RewindSnapshot
{
    public uint Tick;
    public float3 Position;
    public quaternion Rotation;
    public float3 Velocity;                // If kinematic
    public float Health01;
    public float Mana01;
    public uint CooldownTick;
    // ... other important state flags
}

/// <summary>
/// Per-entity rewind history (bounded ring buffer).
/// </summary>
public struct RewindHistory : IBufferElementData
{
    public RewindSnapshot Snapshot;
}
```

**Limit duration like Prince of Persia does** (their manual describes rewind being limited—e.g., a power timer / seconds budget—and also a slow-time ability where the protagonist continues moving).

**Reference:** [Prince of Persia - Rewind Mechanic](https://phoenixtales.github.io/prince-of-persia-2008/reverse-time-mechanic.html)

**A similar conceptual model appears in Braid analysis:** the mechanic relies on recorded state sequences ("positions array") and creating new states after rewinding.

**Reference:** [Braid Time Mechanics Analysis](https://www.mdpi.com/2076-3417/11/4/1868)

**Rewind application:**

While `Mode = Rewind`: each global tick, move cursor backward by N steps and apply snapshot at that cursor.

When rewind stops: resume recording from the current state (new timeline).

### 4. No Heavy Interaction Rules (Make It Explicit and Cheap)

**To avoid cross-time paradoxes: time domains don't physically interact.**

**Simplest rule set:**

- Entities in different `DomainId` don't collide and don't exchange resources/damage.
- Use Unity Physics collision filtering (`BelongsTo`/`CollidesWith` or `GroupIndex`) to deny collisions between time-warped and normal-time layers.
- Comms across domains: either drop, or treat as "jammed" (low integrity).

**This matches "likely nothing happens to either" preference.**

**Reference:** [Unity User Manual - Collision Filtering](https://docs.unity3d.com/Packages/com.unity.physics@latest/manual/collision_filtering.html)

### 5. Delivery: Make It a Normal Miracle Delivery

**Time miracle is just a miracle that spawns/targets a `TimeBubble`:**

- **Throw bomb** → bubble appears at impact
- **Sustain cast** → bubble follows hand/target and drains per tick
- **Target entity** → attach bubble center to that entity (mobile "time anchor")

```csharp
/// <summary>
/// Effect op for time manipulation (used within EffectOp.Payload).
/// </summary>
public struct OpTimeBubble
{
    public TimeBubbleMode Mode;
    public float LocalScale;               // 0-2.0 typical
    public float Radius;
    public float DurationSeconds;
    public bool AttachToTarget;            // If true, attach bubble center to target entity
}
```

### 6. MVP You Can Ship Safely

**Phase 1: Slow + Stop Bubble**

Affecting:
- Cooldowns/timers
- Action progress (build/haul/cast)
- Kinematic movement (agents only)

**Phase 2: Rewind Bubble**

With a 2-8s ring buffer for transforms + a few stats (health, mana, cooldowns).

**Phase 3: Speedup Bubble**

Implemented as `localScale > 1` but hard-clamped `MaxStepsPerGlobalTick` to avoid runaway.

**Phase 4: No Cross-Domain Collisions**

Via collision filters (different `DomainId` = different collision group).

---

## Related Documentation

- **Pickup and Throw System:** `Docs/Concepts/Core/Pickup_And_Throw_System.md` - Throwable miracle orbs integration
- **Hand/Anchor Components:** `Docs/Concepts/Core/Hand_Anchor_Components_Summary.md` - Hand casting integration
- **Environment Fields:** `Docs/Concepts/Core/` - Field modification ops
- **Storehouse System:** `Docs/Concepts/Core/Storehouse_System_Summary.md` - Resource cost tracking
- **Black and White Wiki:** https://blackandwhite.fandom.com/wiki/Miracles

---

**For Implementers:** Start with MVP (ThrowBomb + Beam + Zone, core ops), then expand with new specs  
**For Architects:** Data-driven design enables rapid content creation without code changes  
**For Designers:** Use archetypes to create new miracles by composing effect ops

