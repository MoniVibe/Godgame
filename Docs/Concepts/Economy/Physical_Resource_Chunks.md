# Physical Resource Chunks System

**Status:** Concept - Design Capture
**Category:** Economy - Resource Extraction & Logistics
**Complexity:** Complex
**Created:** 2025-12-02
**Last Updated:** 2025-12-02

---

## Overview

**Purpose:** Transform abstract resource extraction into physical object manipulation with mass-based logistics
**Player Impact:** Villagers interact with tangible resource chunks that must be physically hauled; strategic decisions about chunk sizes vs hauling capacity
**System Role:** Bridge between resource nodes and storehouse resource piles; foundation for realistic labor simulation

### Terminology Note

**This document uses "resource piles" or "stockpiles"** to refer to accumulated quantities of resources (wood, ore, stone) stored in storehouses. These are simple floats/numbers with no behavior.

**NOT to be confused with "aggregate entities" (groups)** such as villages, guilds, armies, bands—collections of individuals with outlooks, alignments, and behaviors. For group dynamics, see [Aggregate_Entities_And_Individual_Dynamics.md](../Core/Aggregate_Entities_And_Individual_Dynamics.md).

### Design Philosophy

**Data-Driven First:** Physical properties (mass, dimensions, resource value) derive from data. Visual representation follows naturally from the underlying metrics.

**Physical Authenticity:** A tree doesn't magically teleport to storage—it gets cut into logs, each log has real mass, villagers carry what they can lift, and pieces that are too heavy get split further.

**Kenshi-Style Progression:** Carrying heavy loads builds strength over time. Villagers naturally specialize based on what they carry.

---

## How It Works

### Inputs
- **Resource Node:** Tree, ore vein, stone quarry with total resource value
- **Villager Action:** Mining/chopping/cutting produces chunks
- **Villager Strength:** Current carrying capacity (improves with experience)
- **Chunk Mass:** Physical weight inherited from resource type and size

### Process

1. **Resource Extraction (Chipping/Cutting)**
   - Villager interacts with resource node (tree, ore vein, stone)
   - Node breaks down into discrete **chunks** with physical properties
   - Each chunk is an independent entity in the world

2. **Chunk Property Calculation**
   - **Mass:** Derived from resource density × physical volume/size
   - **Resource Value:** Mass × quality tier × resource type multiplier
   - **Physical Size:** Visual representation scales with mass (large rock, small pebble)

3. **Carrying Capacity Check**
   - Villager attempts to lift chunk
   - **Can Lift:** `chunkMass <= villagerStrength × carryingCapacityMultiplier`
   - **Too Heavy:** Villager must cut chunk into smaller pieces first

4. **Chunk Subdivision (Recursive)**
   - If chunk too heavy, villager performs secondary cut
   - Original chunk → 2-4 smaller chunks (depending on resource type)
   - Each smaller chunk inherits proportional mass and resource value
   - Repeat until villager can lift resulting piece

5. **Physical Hauling**
   - Villager picks up chunk (entity parented to villager)
   - Carries to nearest storehouse or designated location
   - Movement speed penalty based on carried mass ratio

6. **Storehouse Deposit**
   - Chunk deposited at storehouse
   - Resource value extracted and added to storehouse inventory (resource pile)
   - Physical chunk entity despawned (or recycled to pool)
   - Chunk contributes to visual resource pile representation at storehouse

7. **Strength Experience Gain**
   - Experience awarded proportional to: `carriedMass × distanceHauled`
   - Kenshi-style: Heavy loads over long distances = significant strength gains
   - Experience thresholds increase carrying capacity

---

## Rules

1. **Mass Conservation:** Total resource value before/after chunking must be identical
   - Condition: Chunk subdivision
   - Effect: `Σ(childChunks.resourceValue) = parentChunk.resourceValue`

2. **Strength Limits:** Villagers cannot lift chunks exceeding their strength capacity
   - Condition: `chunkMass > villagerStrength × capacityMultiplier`
   - Effect: Villager must subdivide chunk or abandon it

3. **Physical Presence:** Chunks are collidable entities until deposited
   - Condition: Chunk exists in world
   - Effect: Other villagers/entities must path around chunks, chunks can be moved/stolen

4. **Progressive Specialization:** Repeated carrying of heavy loads increases strength
   - Condition: Villager hauls heavy chunks regularly
   - Effect: Strength stat increases, carrying capacity grows over time

### Edge Cases
- **Chunk too small to subdivide** → Minimum mass threshold (1kg), below which chunk cannot split further; villager calls for help or abandons
- **Multiple villagers carrying together** → Optional: cooperative carrying system where 2+ villagers share load
- **Chunk gets stuck in terrain** → Timeout despawn or push-out physics correction
- **Storehouse full during hauling** → Villager drops chunk at storehouse entrance (overflow pile mechanic)

### Priority Order
1. Check carrying capacity
2. Subdivide if necessary (recursive)
3. Haul to destination
4. Award experience based on effort

---

## Parameters

| Parameter | Default | Range | Impact |
|-----------|---------|-------|--------|
| Base Villager Strength | 25 kg | 15-40 kg | Starting carrying capacity |
| Carrying Capacity Multiplier | 1.0× | 0.5-2.0× | Scales with strength stat |
| Strength XP per kg·km | 0.1 XP | 0.05-0.2 XP | Experience gain rate |
| Subdivision Branches | 3 | 2-4 | How many pieces when splitting |
| Minimum Chunk Mass | 1 kg | 0.5-5 kg | Smallest cuttable unit |
| Movement Speed Penalty | -2% per kg | -1 to -5% | Slowdown when carrying heavy loads |

### Resource Density Examples
| Resource Type | Density (kg/m³) | Example Chunk |
|---------------|----------------|---------------|
| Wood (Oak) | 700 kg/m³ | 1m log = 70 kg |
| Stone (Limestone) | 2400 kg/m³ | 0.5m³ block = 1200 kg |
| Iron Ore | 5000 kg/m³ | 0.3m³ chunk = 1500 kg |
| Gold Ore | 19300 kg/m³ | 0.1m³ nugget = 1930 kg |

---

## Example

### Scenario 1: Tree Harvesting

**Given:** Oak tree with 500 kg total wood, villager with 30 kg carrying capacity
**When:** Villager chops tree
**Then:**
1. Tree breaks into 7 logs (70 kg each, total 500 kg conserved)
2. Villager attempts to lift first log (70 kg)
3. **Check:** 70 kg > 30 kg → Cannot lift
4. Villager cuts log into 3 smaller pieces (23.3 kg each)
5. Villager lifts one piece (23.3 kg) ✓
6. Hauls 200m to storehouse
7. **Experience:** 23.3 kg × 0.2 km = 4.66 kg·km → **0.466 XP** gained
8. Repeat for remaining pieces (21 total hauls required)

### Scenario 2: Stone Quarrying (Extreme Mass)

**Given:** Limestone block 1200 kg, villager with 40 kg carrying capacity (strong worker)
**When:** Villager quarries stone
**Then:**
1. Initial block: 1200 kg
2. **Cannot lift** → Cut into 4 pieces (300 kg each)
3. **Still cannot lift** → Cut each 300kg piece into 4 (75 kg each, 16 total)
4. **Still cannot lift** → Cut each 75kg piece into 2 (37.5 kg each, 32 total)
5. Villager lifts 37.5 kg piece ✓
6. Hauls 150m to storehouse
7. **Experience:** 37.5 kg × 0.15 km = 5.625 kg·km → **0.5625 XP** gained
8. After 10 hauls (cumulative 5+ XP), villager gains strength level → now 45 kg capacity
9. Can now lift 45 kg pieces directly (fewer subdivisions needed)

### Scenario 3: Ore Mining with Quality Propagation

**Given:** Iron ore vein (Quality 85, Rare tier), 300 kg raw ore, villager with 35 kg capacity
**When:** Villager mines ore
**Then:**
1. Vein breaks into 9 chunks (33 kg each, total 300 kg)
2. Each chunk inherits: `Quality = 85`, `TierId = Rare`, `ResourceType = IronOre`
3. Villager lifts 33 kg chunk ✓ (just under 35 kg limit)
4. Hauls 120m to storehouse
5. Storehouse API called: `Add(ResourceType.IronOre, amount=33kg, quality=85, tier=Rare)`
6. **Storehouse logic:** Chunk added to existing Rare iron ore pile (weighted average quality)
7. **Experience:** 33 kg × 0.12 km = 3.96 kg·km → **0.396 XP** gained

---

## Player Feedback

### Visual
- **Chunks:** 3D models scaled to mass (small pebble vs huge log)
- **Carrying Animation:** Villager posture changes based on load (hunched for heavy, upright for light)
- **Strength Progression:** Visible muscle definition increases with strength stat (optional cosmetic)
- **Pile Representation:** Storehouse exterior shows resource piles growing as chunks deposited

### Audio
- **Chop/Mine Sounds:** Different audio per resource type
- **Carrying Grunts:** Villager effort sounds scale with mass ratio
- **Deposit Thud:** Satisfying impact sound when chunk hits storehouse floor

### UI
- **Chunk Tooltip:** Hover shows mass, resource value, quality tier
- **Villager Panel:** Strength stat, current carrying capacity, XP progress bar
- **Storehouse HUD:** Live count of chunks in transit to storehouse

---

## Balance

### Early Game
- **Low Strength:** Villagers must subdivide most resources multiple times
- **Impact:** Labor-intensive hauling, many trips required
- **Strategic Decision:** Assign strongest villagers to heavy resources (mining/quarrying)

### Mid Game
- **Moderate Strength:** Villagers can lift medium chunks directly
- **Impact:** Fewer subdivisions, faster resource gathering
- **Specialization Emerges:** "Miners" naturally stronger from carrying ore

### Late Game
- **High Strength:** Veteran workers lift large chunks easily
- **Impact:** Efficient resource logistics, minimal splitting needed
- **Team Synergy:** Weak villagers handle light tasks (herbs, grain), strong villagers do heavy lifting

### Exploits
- **Infinite Subdivision Loop:** Prevent by enforcing minimum mass threshold (1 kg floor)
- **Experience Grinding:** Natural cap via diminishing returns on repeated short hauls
- **Chunk Duplication:** Ensure despawn on deposit, server-authoritative resource value tracking

---

## Interaction Matrix

| Other Mechanic | Relationship | Notes |
|----------------|--------------|-------|
| **Storehouse API** | Consumer | Chunks deposit via `Add()` contract, added to resource piles in inventory |
| **Resource Quality System** | Integration | Chunks inherit quality/tier from source node, preserved through hauling |
| **Villager Strength Progression** | Dependency | Carrying capacity determined by strength stat growth |
| **Resource Production & Crafting** | Input Source | Deposited chunks feed into refining/crafting pipeline |
| **Resource Piles (Visual)** | Visual Feedback | Storehouse exterior shows physical piles representing stored resources |
| **Spatial Partitioning** | Technical | Chunk entities registered in spatial grid for efficient queries |
| **Task Assignment AI** | Consumer | Villager AI evaluates chunk proximity and strength capacity when selecting tasks |
| **Aggregate Entities (Groups)** | Social Context | Individual hauling actions affect village statistics; see [Aggregate_Entities_And_Individual_Dynamics.md](../Core/Aggregate_Entities_And_Individual_Dynamics.md) |

---

## Technical

### Max Entities
- **Active Chunks:** 500-2000 concurrent (pooled for reuse)
- **Per Resource Node:** 10-50 chunks generated on destruction
- **Chunk Lifetime:** 5-30 minutes (despawn if not picked up)

### Update Frequency
- **Chunk Physics:** Per-frame (Unity physics)
- **Strength XP Calculation:** On chunk deposit (event-driven)
- **Subdivision Check:** On villager interaction (on-demand)

### Data Needs

```csharp
public struct ResourceChunk : IComponentData
{
    public ResourceType ResourceType;          // Wood, Ore, Stone, etc.
    public float Mass;                          // kg (physical weight)
    public float ResourceValue;                 // Resource units (mass × density × quality)
    public byte Quality;                        // 1-100 (from source node)
    public ResourceQualityTier TierId;         // Poor, Common, Rare, etc.
    public Entity SourceNode;                   // Original tree/vein entity
}

public struct ChunkCarrier : IComponentData
{
    public Entity CarriedChunk;                 // Current chunk being carried
    public float CarriedMass;                   // Mass of carried chunk
    public float3 PickupPosition;               // Where chunk was picked up
}

public struct VillagerStrength : IComponentData
{
    public byte StrengthLevel;                  // 1-100 stat
    public float CarryingCapacity;              // kg (base + strength bonuses)
    public float StrengthExperience;            // Cumulative XP
    public float NextLevelThreshold;            // XP required for next level
}

public struct ChunkSubdivisionHistory : IComponentData
{
    public Entity ParentChunk;                  // Original chunk before split
    public byte SubdivisionDepth;               // How many times subdivided (prevent infinite recursion)
}
```

---

## DOTS Implementation Notes

### Systems

**`ResourceNodeChunkingSystem`** (Authoring Group)
- Destroys resource node entity on depletion
- Spawns chunk entities based on node's total mass
- Distributes quality/tier properties to all child chunks

**`ChunkSubdivisionSystem`** (Simulation Group)
- Checks if villager can lift chunk
- Recursively subdivides if mass exceeds capacity
- Enforces minimum mass threshold and subdivision depth limit

**`ChunkCarryingSystem`** (Simulation Group)
- Parents chunk entity to villager transform
- Applies movement speed penalty based on mass ratio
- Tracks distance hauled for XP calculation

**`ChunkDepositSystem`** (Simulation Group)
- Detects chunk arrival at storehouse
- Calls `StorehouseAPI.Add(resourceType, resourceValue, quality, tier)`
- Despawns chunk entity or returns to pool
- Awards strength XP to villager based on `mass × distance`

**`StrengthProgressionSystem`** (Simulation Group)
- Accumulates XP from chunk hauling
- Levels up strength stat when thresholds reached
- Recalculates carrying capacity based on new strength

### Registry Integration

Extend **`ResourceRegistryEntry`** (from PureDOTS):
```csharp
public struct ResourceRegistryEntry
{
    // ... existing fields ...
    public float DensityKgPerCubicMeter;        // For mass calculations
    public float BaseChunkSizeCubicMeters;      // Default chunk volume
}
```

Extend **`VillagerRegistryEntry`** (from PureDOTS):
```csharp
public struct VillagerRegistryEntry
{
    // ... existing fields ...
    public byte StrengthStat;                   // 1-100
    public float CurrentCarryingCapacity;       // kg
}
```

---

## Tests

- [ ] **Mass Conservation Test:** Subdivide chunk 5 levels deep, verify total resource value unchanged
- [ ] **Carrying Capacity Test:** Villager with 30 kg capacity cannot lift 40 kg chunk
- [ ] **Strength Progression Test:** Haul 100 kg total over 1 km, verify XP gain and level-up
- [ ] **Storehouse Integration Test:** Chunk deposit correctly aggregates into storehouse inventory with quality/tier preserved
- [ ] **Subdivision Limit Test:** Chunk at minimum mass (1 kg) cannot subdivide further
- [ ] **Spatial Collision Test:** Chunks block villager pathfinding until moved
- [ ] **Experience Gain Test:** Heavy load (40 kg) over long distance (500m) awards more XP than light load (10 kg) over short distance (50m)

---

## Open Questions

1. **Cooperative Carrying?** Should 2+ villagers be able to carry a single large chunk together?
   - **Option A:** Yes, allows hauling of otherwise impossible chunks (gameplay variety)
   - **Option B:** No, simplifies system (always subdivide instead)
   - **Recommendation:** V2.0 feature if player feedback requests it

2. **Chunk Decay/Spoilage?** Should organic resources (wood, meat) decay if left unhauled?
   - **Option A:** Yes, adds urgency to resource logistics
   - **Option B:** No, reduces micromanagement stress
   - **Recommendation:** Only for perishable food items, not raw materials

3. **Theft Mechanic?** Can enemy villagers or bandits steal chunks left in the world?
   - **Option A:** Yes, creates tension and guards needed
   - **Option B:** No, avoids frustration
   - **Recommendation:** Only if village-vs-village conflict implemented

4. **Visual Fidelity?** How detailed should chunk models be?
   - **Option A:** Unique models per resource type and size (high fidelity)
   - **Option B:** Generic boxes with color-coding (performance)
   - **Recommendation:** Start with simple cubes/spheres, iterate based on art budget

5. **Chunk Physics?** Should chunks roll downhill, stack on each other?
   - **Option A:** Full physics simulation (emergent gameplay)
   - **Option B:** Kinematic only (simpler, more stable)
   - **Recommendation:** Kinematic for MVP, physics in polish phase

---

## Version History

- **v0.1 - 2025-12-02:** Initial concept capture based on design discussion

---

## Related Mechanics

- **Storehouse API:** [Storehouse_API.md](Storehouse_API.md) - Deposit contract integration
- **Resource Production & Crafting:** [Resource_Production_And_Crafting.md](Resource_Production_And_Crafting.md) - Quality/tier system inheritance
- **Resource Quality & Processing:** [ResourceQualityAndProcessing.md](../../../PureDOTS/Packages/com.moni.puredots/Documentation/DesignNotes/ResourceQualityAndProcessing.md) - Shared quality model
- **Resource Piles (Visual):** [Aggregate_Piles.md](../Implemented/Resources/Aggregate_Piles.md) - Visual storehouse representation (Note: "Aggregate Piles" is legacy terminology; refers to resource stockpiles, not social groups)
- **Villager AI Task Selection:** (TBD) - How villagers choose hauling tasks
- **Individual Progression System:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Strength stat growth integration
- **Aggregate Entities & Individual Dynamics:** [Aggregate_Entities_And_Individual_Dynamics.md](../Core/Aggregate_Entities_And_Individual_Dynamics.md) - How individual hauling actions affect village group characteristics (bidirectional influence model)

---

## Design Intent (What It Should Feel Like)

### Player Perspective
- **Physical Connection:** Resources feel *real*—you see the tree fall, the logs scatter, villagers struggling under heavy loads
- **Strategic Depth:** Assigning strong villagers to mining isn't just optimal, it's *necessary*
- **Progression Satisfaction:** Watching a novice villager struggle with a 25 kg log, then months later effortlessly carrying 50 kg chunks
- **Emergent Storytelling:** "Big Hank" became the village's best miner not through assignment, but through naturally carrying heavy ore chunks and gaining strength

### Design Goals
- **Data-Driven Authenticity:** Physical properties (mass, size, resource value) drive gameplay naturally
- **No Magic Teleportation:** Resources must be physically moved by villagers, creating believable logistics
- **Kenshi-Style Mastery:** Repeated action (carrying heavy loads) makes villagers better at that action
- **Visual Coherence:** What you see (chunk size) matches what villagers experience (mass/difficulty)

### Narrative Opportunities
- Legendary blacksmith strong enough to carry raw iron ore chunks alone
- Frail herbalist who can only gather light mushrooms and herbs
- Village mining team that developed incredible strength over years of quarrying
- Merchant caravan struggling under overloaded wagons (extension of system to NPCs)

---

## Implementation Roadmap

### Phase 1: Core Chunk System (MVP)
- [ ] `ResourceChunk` component and entity spawning
- [ ] Basic subdivision logic (recursive splitting)
- [ ] Villager carrying capacity checks
- [ ] Simple haul-to-storehouse behavior
- [ ] Storehouse deposit integration

### Phase 2: Strength Progression
- [ ] `VillagerStrength` component and XP system
- [ ] Experience calculation (mass × distance)
- [ ] Level-up thresholds and carrying capacity scaling
- [ ] UI feedback (strength stat display)

### Phase 3: Visual Polish
- [ ] Chunk 3D models (scale with mass)
- [ ] Carrying animations (adjust based on load)
- [ ] Storehouse pile visualization updates
- [ ] Audio feedback (chopping, hauling, depositing)

### Phase 4: Advanced Features (Post-MVP)
- [ ] Cooperative carrying (optional)
- [ ] Chunk physics (rolling, stacking)
- [ ] Resource decay for perishables
- [ ] Theft/banditry mechanics

---

**For Implementers:** Start with wood chunks (simplest resource type) before tackling ore/stone with extreme mass. Ensure mass conservation testing in place from day one—bugs here create resource duplication exploits.

**For Designers:** Tune strength XP rates so that dedicated miners reach "comfortable" carrying capacity (75% of typical ore chunk mass) within 1-2 game years. Too slow = frustration, too fast = no meaningful progression.

**For Narrative:** The physical chunk system creates organic stories—villagers who specialize naturally (based on what they carry), supply chain bottlenecks when strong villagers are away, and visible resource accumulation that feels earned.

---

**Last Updated:** 2025-12-02
**Status:** Concept Captured - Ready for Implementation Planning
