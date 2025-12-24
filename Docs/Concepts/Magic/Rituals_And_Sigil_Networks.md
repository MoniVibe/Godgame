# Rituals and Sigil Networks

**Status:** Concept
**Category:** Magic - Infrastructure & Channeling
**Scope:** Individual → Settlement → Regional → Global
**Created:** 2025-12-21
**Last Updated:** 2025-12-21

---

## Purpose

**Primary Goal:** Enable persistent magical infrastructure networks that connect locations, transfer resources/power, enable long-range effects, and allow complex ritual-driven operations.

**Secondary Goals:**
- Create strategic infrastructure (teleportation networks, power grids, surveillance systems)
- Support complex multi-step magical operations (chained rituals, simultaneous effects)
- Enable focus-based magical channeling (alternative to mana/power)
- Provide permanence vs. temporary magical constructs (infrastructure vs. spells)

---

## System Overview

### Key Insight

**Sigils create networks; rituals propagate effects through them.**

- **Sigils:** Crafted magical symbols that form **nodes** in networks. Quality and purpose determine network capabilities.
- **Rituals:** Maintained magical processes that **activate** and **channel** through sigil networks. Use focus as resource (alternative to mana/power).
- **Networks:** Connections between sigil nodes enable various functions (transport, transfer, sensing, chaining).

**Example Emergence:**
- Settlement crafts teleportation sigil network → enables rapid travel between nodes
- Ritual maintains network → requires focus allocation from participants
- Network quality determines range, capacity, reliability
- Temporary vs. permanent networks create strategic choices (quick setup vs. lasting infrastructure)

### Components

1. **Sigils:** Magical symbols/nodes with purpose, quality, connections
2. **Sigil Networks:** Connected sigil graphs (topology determines function)
3. **Rituals:** Maintained processes that activate networks (focus-driven)
4. **Network Functions:** Teleportation, resource transfer, power/mana transfer, vision/senses, chaining
5. **Ritual Chaining:** Simultaneous multi-effect operations (demolition, sacrifices, etc.)

---

## Sigil Networks

### Sigil Creation

**Sigils are crafted magical symbols** that serve as **nodes** in networks.

**Sigil Properties:**

**1. Purpose (Determines Function)**
- **Teleportation:** Enables entity/material transport between nodes
- **Resource Transfer:** Transfers physical resources (materials, items)
- **Power/Mana Transfer:** Channels magical energy between nodes
- **Vision/Senses:** Allows remote perception through network
- **Chaining:** Connects effects for simultaneous activation
- **Summoning:** Attracts or calls entities to sigil location
- **Portal:** Creates gateway for passage (requires ritual activation)

**2. Quality (Determines Capability)**

**Quality Factors:**
- **Crafter Skill:** Entity's sigil crafting expertise (0-100)
- **Material Quality:** Quality of materials used (ink, surfaces, components)
- **Location:** Ambient magical energy, ley lines, ritual sites
- **Ritual Reinforcement:** Repeated ritual maintenance increases quality over time

**Quality Formula:**
```
SigilQuality = 
    (CrafterSkill × 0.5) +
    (MaterialQuality × 0.3) +
    (LocationBonus × 0.1) +
    (RitualReinforcement × 0.1)

Maximum Quality: 100
Typical Range: 30-90 (depending on crafter, materials, location)
```

**Quality Effects:**
- **Range:** Higher quality = longer connection distance
- **Capacity:** Higher quality = more entities/resources/power per transfer
- **Reliability:** Higher quality = lower failure chance, less maintenance
- **Functionality:** Higher quality = unlocks advanced network functions

---

### Network Topology

**Sigils connect to form networks** (graphs of connected nodes).

**Connection Types:**

**1. Direct Connection (Point-to-Point)**
- Two sigils directly linked
- Simple, efficient for small networks
- Range limited by sigil quality
- Example: Village A ↔ Village B teleportation

**2. Hub Network (Star Topology)**
- Central hub sigil connects to multiple nodes
- Efficient for centralized resources/power
- Hub quality determines network capacity
- Example: Capital city hub with regional connections

**3. Mesh Network (Distributed)**
- Multiple interconnected sigils (redundant paths)
- Resilient (network functions even if nodes fail)
- Complex, requires high-quality sigils
- Example: Major settlement network with backup routes

**4. Linear Chain (Path)**
- Sequential connections (A → B → C → D)
- Simple but vulnerable (single point of failure)
- Useful for long-distance connections
- Example: Trade route teleportation chain

**Connection Range:**
```
MaxConnectionRange = BaseRange × (SigilQuality1 + SigilQuality2) / 2

Where:
  BaseRange = 1000 units (base connection distance)
  
Example:
  Quality 80 + Quality 90 = (80 + 90) / 2 = 85
  MaxRange = 1000 × 85 = 85,000 units
```

---

### Network Functions

**Different sigil purposes enable different network capabilities:**

---

#### 1. Teleportation Networks

**Function:** Transport entities and materials between sigil nodes.

**Mechanics:**
- **Activation:** Ritual activates teleportation (or player/entity activates)
- **Range:** Limited by sigil connection range
- **Capacity:** Number of entities/items per transport (based on sigil quality)
- **Cost:** Focus cost for ritual activation (or mana/power if non-ritual)

**Quality Effects:**
- **Low Quality (30-50):** 1-3 entities, 500-unit range, 5% failure chance
- **Medium Quality (51-70):** 5-10 entities, 2000-unit range, 2% failure chance
- **High Quality (71-90):** 20-50 entities, 5000-unit range, 0.5% failure chance
- **Legendary Quality (91-100):** 100+ entities, 10000+ unit range, 0.1% failure chance

**Use Cases:**
- Rapid troop deployment
- Trade route shortcuts
- Emergency evacuation
- Resource transport

---

#### 2. Resource Transfer Networks

**Function:** Transfer physical resources (materials, items) between sigil nodes.

**Mechanics:**
- **Transfer Rate:** Mass/volume per time (based on sigil quality)
- **Resource Types:** Compatible resources only (sigil purpose determines compatibility)
- **Storage:** Requires storage capacity at destination
- **Cost:** Focus cost for ritual activation

**Quality Effects:**
- **Low Quality (30-50):** 100 kg/hour, single resource type, 10% loss
- **Medium Quality (51-70):** 500 kg/hour, multiple resource types, 5% loss
- **High Quality (71-90):** 2000 kg/hour, all resource types, 1% loss
- **Legendary Quality (91-100):** 10000 kg/hour, all resource types, 0% loss

**Use Cases:**
- Centralized resource collection
- Supply chain optimization
- Emergency resource distribution

---

#### 3. Power/Mana Transfer Networks

**Function:** Channel magical energy (mana, power) between sigil nodes.

**Mechanics:**
- **Transfer Rate:** Mana/power per time (based on sigil quality)
- **Efficiency:** Energy loss during transfer (reduced by quality)
- **Capacity:** Maximum energy throughput
- **Cost:** Focus cost for ritual activation

**Quality Effects:**
- **Low Quality (30-50):** 10 mana/hour, 20% loss, 50 mana capacity
- **Medium Quality (51-70):** 50 mana/hour, 10% loss, 200 mana capacity
- **High Quality (71-90):** 200 mana/hour, 5% loss, 1000 mana capacity
- **Legendary Quality (91-100):** 1000 mana/hour, 1% loss, 10000 mana capacity

**Use Cases:**
- Powering remote rituals/miracles
- Centralized mana distribution
- Emergency power supply
- Ritual chaining energy requirements

---

#### 4. Vision/Senses Networks

**Function:** Allow entities to perceive (see, hear, sense) through network nodes.

**Mechanics:**
- **Perception Range:** Radius around sigil node (based on quality)
- **Senses:** Visual, auditory, magical sense (determined by sigil purpose)
- **Channeling:** Entity uses focus to channel perception through network
- **Cost:** Focus cost per perception duration

**Quality Effects:**
- **Low Quality (30-50):** 50-unit radius, visual only, 10 focus/minute
- **Medium Quality (51-70):** 200-unit radius, visual + auditory, 5 focus/minute
- **High Quality (71-90):** 500-unit radius, all senses, 2 focus/minute
- **Legendary Quality (91-100):** 2000-unit radius, all senses + magical, 0.5 focus/minute

**Use Cases:**
- Surveillance networks
- Scouting remote locations
- Monitoring ritual sites
- Intelligence gathering

---

#### 5. Chaining Networks

**Function:** Connect multiple effects for simultaneous activation.

**Mechanics:**
- **Chain Size:** Number of linked effects (based on sigil quality)
- **Synchronization:** Effects trigger simultaneously (within tolerance window)
- **Effect Propagation:** Effects propagate through network nodes
- **Cost:** Focus cost for ritual activation (scales with chain size)

**Quality Effects:**
- **Low Quality (30-50):** 3-5 effects, 100ms tolerance, 20 focus/effect
- **Medium Quality (51-70):** 10-15 effects, 50ms tolerance, 10 focus/effect
- **High Quality (71-90):** 30-50 effects, 10ms tolerance, 5 focus/effect
- **Legendary Quality (91-100):** 100+ effects, 1ms tolerance, 1 focus/effect

**Use Cases:**
- Mass demolition (multiple buildings simultaneously)
- Tunnel creation (coordinated excavation)
- Sacrifice rituals (simultaneous offerings)
- Synchronized defenses (barriers, wards activate together)

---

#### 6. Summoning Networks

**Function:** Attract or call entities to sigil location.

**Mechanics:**
- **Summoning Range:** Distance entities can be called from (based on quality)
- **Entity Types:** Compatible entity types (determined by sigil purpose)
- **Attraction:** Entities sense summoning and may respond
- **Cost:** Focus cost for ritual activation

**Quality Effects:**
- **Low Quality (30-50):** 500-unit range, weak attraction, specific entity type
- **Medium Quality (51-70):** 2000-unit range, moderate attraction, entity categories
- **High Quality (71-90):** 5000-unit range, strong attraction, multiple categories
- **Legendary Quality (91-100):** 20000-unit range, irresistible attraction, all compatible entities

**Use Cases:**
- Calling allies to battle
- Gathering resources (animals, materials)
- Emergency assistance
- Ritual participants

---

## Rituals

### Ritual Mechanics

**Rituals are maintained magical processes** that activate and channel effects through sigil networks.

**Key Characteristics:**
- **Focus-Driven:** Rituals use focus (alternative to mana/power)
- **Maintained:** Rituals require ongoing focus allocation (not one-time cost)
- **Propagation:** Rituals propagate effects through connected sigil networks
- **Portal Opening:** Rituals can open portals (entities pass through using focus)

---

### Ritual Types

**1. Network Activation Rituals**

**Purpose:** Activate sigil network functions (teleportation, transfer, etc.)

**Mechanics:**
- **Focus Cost:** Continuous focus drain while network is active
- **Participants:** Multiple entities can contribute focus (reduces individual cost)
- **Duration:** Ritual maintains network function as long as focus is supplied
- **Deactivation:** Network deactivates when focus supply stops

**Example:**
```
Teleportation Network Activation:
  Base Focus Cost: 50 focus/minute
  Participants: 5 entities contributing 10 focus/minute each
  Network Active: As long as focus is supplied
  Deactivation: When focus supply stops, network deactivates
```

---

**2. Portal Opening Rituals**

**Purpose:** Open portals that allow entities to pass through (using focus instead of mana/power).

**Mechanics:**
- **Focus Cost:** Focus cost per entity passage (not mana/power)
- **Portal Location:** Portal opens at sigil node location
- **Destination:** Portal connects to another sigil node (or specified location)
- **Duration:** Portal remains open while ritual maintains focus

**Focus Cost Formula:**
```
PortalPassageCost = BaseCost × DistanceMultiplier × EntitySizeMultiplier

Where:
  BaseCost = 20 focus (base cost per entity)
  DistanceMultiplier = 1.0 + (Distance / 10000) (scales with distance)
  EntitySizeMultiplier = 1.0 (small), 2.0 (medium), 5.0 (large)
  
Example:
  Entity (medium) travels 5000 units:
    Cost = 20 × (1.0 + 5000/10000) × 2.0
         = 20 × 1.5 × 2.0
         = 60 focus
```

**Use Cases:**
- Alternative to mana-based teleportation
- Focus-rich entities can travel without mana
- Secret travel (portals don't consume mana, harder to detect)

---

**3. Summoning Rituals**

**Purpose:** Summon entities to sigil location.

**Mechanics:**
- **Focus Cost:** Focus cost for ritual activation and maintenance
- **Attraction Strength:** Based on sigil quality and ritual power
- **Entity Response:** Entities may choose to respond (not forced)
- **Range:** Summoning range based on sigil quality

**Use Cases:**
- Calling allies
- Gathering resources
- Emergency assistance

---

**4. Chaining Rituals**

**Purpose:** Chain multiple effects together for simultaneous activation.

**Mechanics:**
- **Chain Activation:** Multiple effects trigger simultaneously
- **Focus Cost:** Focus cost scales with number of effects
- **Synchronization:** Effects trigger within tolerance window (based on sigil quality)
- **Effect Types:** Compatible effects (demolition, tunnels, sacrifices, etc.)

**Chaining Examples:**

**Mass Demolition:**
```
Ritual: Chained Demolition
Effects: 20 buildings destroyed simultaneously
Focus Cost: 20 × 5 = 100 focus (5 focus per effect)
Synchronization: 10ms tolerance (high-quality sigil network)
Result: All 20 buildings collapse at once
```

**Tunnel Creation:**
```
Ritual: Chained Excavation
Effects: 15 tunnel sections created simultaneously
Focus Cost: 15 × 8 = 120 focus (8 focus per section)
Synchronization: 10ms tolerance
Result: Complete tunnel network created instantly
```

**Sacrifice Ritual:**
```
Ritual: Chained Sacrifice
Effects: 50 entities sacrificed simultaneously at different nodes
Focus Cost: 50 × 10 = 500 focus (10 focus per sacrifice)
Synchronization: 1ms tolerance (legendary-quality sigil network)
Result: Massive power/mana generation from simultaneous sacrifices
```

---

### Ritual Maintenance

**Rituals require ongoing focus allocation** to maintain effects.

**Maintenance Mechanics:**

**1. Continuous Focus Drain**
- Ritual consumes focus per tick while active
- Focus must be supplied continuously (or ritual deactivates)
- Multiple participants can share focus cost

**2. Focus Pool Management**
- Entities contribute focus to ritual pool
- Pool drains per tick based on ritual cost
- If pool empties, ritual deactivates

**3. Ritual Reinforcement**
- Long-term ritual maintenance increases sigil quality (reinforcement bonus)
- Reinforced sigils become more efficient (lower focus cost over time)
- Reinforcement bonus caps at +10 quality after extended maintenance

---

## Temporary vs. Permanent Networks

### Temporary Networks

**Characteristics:**
- **Quick Setup:** Fast to create (low setup time)
- **Lower Quality:** Typically lower quality (limited time/resources)
- **Short Duration:** Network exists for limited time (ritual duration, or until sigil fades)
- **Lower Cost:** Lower initial investment (materials, time)

**Use Cases:**
- Emergency teleportation (quick escape)
- Temporary resource transfer (one-time need)
- Tactical operations (short-term advantage)
- Experimental networks (testing before permanent installation)

**Decay Mechanics:**
- Temporary sigils fade over time (quality decreases)
- Ritual must be maintained to prevent decay
- After ritual ends, sigils fade completely (network breaks)

---

### Permanent Networks

**Characteristics:**
- **Long Setup:** Extended creation time (careful crafting)
- **Higher Quality:** Typically higher quality (best materials, skilled crafters)
- **Long Duration:** Network persists indefinitely (doesn't fade)
- **Higher Cost:** Significant initial investment (materials, time, skilled crafters)

**Use Cases:**
- Infrastructure (settlement teleportation hubs)
- Trade routes (long-term resource transfer)
- Power grids (permanent mana/power distribution)
- Surveillance networks (long-term monitoring)

**Permanence Mechanics:**
- Permanent sigils don't fade (anchored to location)
- Ritual maintenance optional (network functions without ritual, but rituals enhance)
- Can be destroyed (damage, sabotage, enemy action)
- Reinforced over time (ritual maintenance increases quality)

---

## Integration with Other Systems

### Focus System

**Rituals use focus** (alternative to mana/power):
- Entities allocate focus to rituals (reduces available focus for other abilities)
- Multiple entities can contribute focus (shares burden)
- Focus allocation is voluntary (entities choose to participate)

**Focus Allocation:**
```
Entity allocates focus to ritual:
  AllocatedFocus = min(RequestedFocus, AvailableFocus)
  
Ritual pool receives focus:
  RitualFocusPool += AllocatedFocus
  
Ritual consumes focus:
  RitualFocusPool -= RitualCostPerTick
```

---

### Miracle System

**Rituals complement miracles:**
- **Miracles:** Immediate, mana-powered effects (player/god actions)
- **Rituals:** Maintained, focus-powered effects (entity-driven, infrastructure)
- **Synergy:** Rituals can channel miracle effects through networks (extend range, distribute power)

**Example:**
```
Player casts healing miracle at sigil node A
Ritual channels miracle through network
Miracle effect propagates to nodes B, C, D
All connected nodes receive healing effect
```

---

### Entity Crafting System

**Skilled entities craft sigils:**
- **Crafting Skill:** Entity's sigil crafting expertise
- **Material Quality:** Materials used affect sigil quality
- **Location Selection:** Entities choose locations (ley lines, ritual sites, strategic positions)

---

### Combat & Warfare

**Strategic Infrastructure:**
- **Teleportation Networks:** Rapid troop deployment
- **Vision Networks:** Surveillance, intelligence
- **Chaining Networks:** Coordinated attacks, simultaneous strikes

**Vulnerability:**
- Networks can be sabotaged (destroy sigils, disrupt rituals)
- Enemy can use networks (captured sigils, infiltrated rituals)
- Strategic targets (destroying network hubs disables entire networks)

---

## Component Structure

### Sigil Components

```csharp
// Sigil node
public struct SigilNode : IComponentData
{
    public Entity Owner;              // Entity that owns/controls sigil
    public SigilPurpose Purpose;      // Teleportation, Transfer, Vision, etc.
    public float Quality;              // 0-100 (determines capability)
    public bool IsPermanent;           // True if permanent, false if temporary
    public uint CreationTick;          // When sigil was created
    public float Reinforcement;        // Quality bonus from ritual maintenance
}

// Sigil network connection
public struct SigilConnection : IBufferElementData
{
    public Entity TargetSigil;         // Connected sigil node
    public float ConnectionStrength;   // Connection quality (avg of both sigils)
    public float MaxRange;             // Maximum connection range
}

// Sigil network function state
public struct NetworkFunction : IComponentData
{
    public NetworkFunctionType Type;   // Teleportation, Transfer, Vision, etc.
    public bool IsActive;              // True if function is active
    public float CurrentCapacity;      // Current usage (entities/resources/power)
    public float MaxCapacity;          // Maximum capacity (based on quality)
}
```

---

### Ritual Components

```csharp
// Ritual state
public struct RitualState : IComponentData
{
    public Entity RitualOwner;         // Entity managing ritual
    public RitualType Type;            // Activation, Portal, Summoning, Chaining
    public Entity TargetSigil;         // Sigil node ritual affects
    public float FocusCostPerTick;     // Focus cost per update tick
    public float CurrentFocusPool;     // Current focus available
    public bool IsActive;              // True if ritual is active
}

// Focus contribution to ritual
public struct RitualFocusContribution : IComponentData
{
    public Entity RitualEntity;        // Ritual receiving focus
    public float ContributedFocus;     // Focus contributed per tick
}

// Portal state (if ritual opens portal)
public struct PortalState : IComponentData
{
    public Entity SourceSigil;         // Portal source sigil
    public Entity DestinationSigil;    // Portal destination sigil
    public float FocusCostPerPassage;  // Focus cost per entity passage
    public bool IsOpen;                // True if portal is open
}

// Chaining ritual (multiple effects)
public struct ChainedRitual : IComponentData
{
    public Entity RitualOwner;
    public BlobAssetReference<ChainedEffectBlob> Effects; // List of chained effects
    public float SynchronizationTolerance; // ms tolerance for simultaneous activation
    public uint ActivationTick;        // When effects will trigger
}
```

---

### Network Topology Tracking

```csharp
// Network graph (singleton or per-network)
public struct SigilNetworkGraph : IBufferElementData
{
    public Entity SigilNode;           // Node in graph
    public DynamicBuffer<SigilConnection> Connections; // Connected nodes
    public NetworkFunctionType EnabledFunctions; // Bitfield of active functions
}
```

---

## System Dynamics

### Inputs
- Entity actions (crafting sigils, initiating rituals, contributing focus)
- Player/god actions (activating networks, channeling miracles through networks)
- Environmental events (ley lines, ritual sites, magical disturbances)
- Network events (connection/disconnection, quality changes, sabotage)

### Internal Processes
1. **Sigil Crafting:** Entities craft sigils (skill, materials, location determine quality)
2. **Network Formation:** Sigils connect based on range, purpose, topology
3. **Ritual Activation:** Rituals activate network functions (focus-driven)
4. **Effect Propagation:** Effects propagate through network (teleportation, transfer, vision, chaining)
5. **Maintenance:** Rituals maintain networks (focus allocation, reinforcement)

### Outputs
- Network functions (teleportation, transfer, vision, summoning, chaining)
- Infrastructure capabilities (rapid travel, resource distribution, surveillance)
- Strategic advantages (deployment, intelligence, coordinated operations)
- Quality improvements (reinforcement from maintenance)

---

## Balancing Considerations

### Focus vs. Mana/Power

**Focus Advantages:**
- Alternative resource (doesn't consume mana/power)
- Can be shared (multiple entities contribute)
- Suitable for maintained effects (infrastructure)

**Focus Disadvantages:**
- Reduces available focus for other abilities
- Requires ongoing allocation (not one-time)
- Limited by entity focus capacity

**Balance:** Focus suits infrastructure/maintained effects; mana/power suits immediate/instant effects.

---

### Network Quality Scaling

**Quality determines capability:**
- Higher quality = better capabilities (range, capacity, reliability)
- Quality requires investment (skilled crafters, good materials, strategic locations)
- Reinforcement rewards long-term maintenance (quality improvement over time)

**Balance:** Quality scaling provides progression (low-quality networks for early game, high-quality for late game).

---

### Temporary vs. Permanent Trade-offs

**Temporary Networks:**
- Quick setup, lower cost, short duration
- Suitable for tactical operations, emergencies

**Permanent Networks:**
- Long setup, higher cost, long duration
- Suitable for infrastructure, strategic positions

**Balance:** Both serve different purposes (tactical vs. strategic).

---

## Open Questions

1. **Network Interference:** Can multiple networks overlap? Do they interfere or enhance?
2. **Ritual Failure:** What happens if focus supply is interrupted mid-ritual? Partial effects or complete failure?
3. **Sigil Destruction:** How easily can sigils be destroyed? What are destruction mechanics?
4. **Network Limits:** Maximum network size? Maximum connections per sigil?
5. **Player Control:** Can players craft sigils directly, or only through entities?
6. **Network Visualization:** How are networks visualized to players? UI representation?

---

## Related Documentation

- **Focus System:** `Docs/Concepts/Implemented/Core/Focus_And_Status_Effects_System.md`
- **Miracle System:** `Docs/Concepts/Miracles/Miracle_System_Vision.md`
- **Miracle Crafting:** `Docs/Concepts/Miracles/Miracle_Crafting_System.md`
- **Languages & Magic:** `Docs/Concepts/Core/Languages_And_Magic_System.md`
- **Entity Crafting:** `Docs/Concepts/Villagers/Entity_Crafting_System.md`






