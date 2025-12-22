# Mana Grid System

**Status:** Concept
**Category:** Magic - Environment Field Integration
**Scope:** Individual → Area → Regional → Global
**Created:** 2025-01-17
**Last Updated:** 2025-01-17

---

## Purpose

**Primary Goal:** Implement mana as a scalar environment field (like moisture) with dynamic influences (emitters/siphons), enabling spellcasting to query ambient mana availability through the existing SpatialSample API.

**Secondary Goals:**
- Unify mana field with existing environment field architecture
- Support deterministic, replay-stable mana simulation
- Enable mobile inhibitors (null-zones, siphons) as dynamic field influences
- Integrate mana availability with spell cost/failure calculations
- Scale efficiently using dirty-region updates and simulation LOD

---

## System Overview

### Key Insight

**Mana is a scalar environment field (like moisture/temperature) with base grid values plus dynamic influences (sources/sinks), queried through the existing EnvironmentSampler API.**

- **Grid-Based:** Mana stored per cell (similar to moisture grid)
- **Dynamic Influences:** Emitters and siphons modify field (like gravity sources)
- **Spatial Sampling:** Spellcasting queries mana via `SampleMana(pos)` through EnvironmentSampler
- **Deterministic:** Uses fixed-point or deterministic floats for replay stability
- **Performance:** Dirty-region updates, gather-based influence application (no atomics)

### Architecture Integration

**Unified with Existing Systems:**
- Uses `EnvironmentGridMetadata` for grid structure
- Uses `EnvironmentScalarSample` (Base + Contribution) pattern
- Uses `EnvironmentScalarContribution` buffer for dynamic influences
- Follows moisture/temperature field patterns
- Integrates with existing `EnvironmentSampler` API

---

## Field Definition

### Grid Cell Values

**Per-Cell Data (ManaGridBlob):**

```csharp
public struct ManaCellBlob
{
    // Base field values (0-1 normalized, or 0-65535 fixed-point)
    public ushort Mana;              // Q16.16 fixed-point: 0.0 to 1.0 → 0 to 65535
    public ushort ManaBase;          // What cell relaxes back to over time
    public ushort ManaRegenRate;     // Q8.8: regen per second toward base
    public ushort ManaDiffusion;     // Q8.8: spread rate to neighbors
    
    // Optional blocking/sealing
    public byte ManaHardBlock;       // 0 = passable, 255 = complete seal (nullstone, wards)
    public byte SealQuality;         // Quality of seal (affects bypass difficulty)
}
```

**Fixed-Point Representation (Q16.16):**
- **Rationale:** Deterministic, replay-stable, Burst-friendly
- **Range:** 0.0 to 1.0 maps to 0 to 65535
- **Conversion:** `float mana = (float)cell.Mana / 65535.0f`
- **Storage:** `ushort` (2 bytes per cell, efficient)

**Alternative (if fixed-point not required):**
- Use `float` directly (simpler, but less deterministic)
- Accept float precision differences in replay scenarios

---

## Dynamic Influences (Emitters + Siphons)

### ManaInfluence Component

**Generic component for anything that modifies the field:**

```csharp
public struct ManaInfluence : IComponentData
{
    public float Strength;           // Positive = source (emitter), Negative = sink (siphon)
    public float Radius;             // Influence radius (meters)
    public byte FalloffMode;         // 0=Linear, 1=Smoothstep, 2=InverseSquare
    public byte InfluenceMode;       // 0=Additive, 1=ClampMax, 2=SuppressCasting
    public byte UpdateRateTier;      // 0=Every tick, 1=Every N ticks (performance LOD)
    public ushort ChannelId;         // Links to EnvironmentScalarContribution channel
}

// Position component (required for spatial queries)
// Uses LocalTransform.Position for influence center
```

**Influence Modes:**

1. **Additive (0):** Push mana up/down
   - `delta = Strength * falloff(distance)`
   - Adds or subtracts from base mana

2. **ClampMax (1):** Inhibitor caps mana to maximum
   - `mana = min(mana, MaxMana * (1 - Strength))`
   - Prevents mana from exceeding threshold

3. **SuppressCasting (2):** Adds disruption separate from mana level
   - Creates "ManaDisruption" field (separate channel)
   - Spellcasting checks both mana availability and disruption
   - Used for null-zones and inhibitors

**Falloff Functions:**

```csharp
// Linear: 1 - (distance / Radius)
float LinearFalloff(float distance, float radius)
    => math.max(0f, 1f - (distance / radius));

// Smoothstep: smooth curve
float SmoothstepFalloff(float distance, float radius)
    => math.smoothstep(radius, 0f, distance);

// Inverse Square: 1 / (1 + distance²)
float InverseSquareFalloff(float distance, float radius)
{
    float normalizedDist = distance / radius;
    return 1f / (1f + normalizedDist * normalizedDist);
}
```

### Performance: Gather-Based (No Atomics)

**Critical Rule:** Do not "scatter-add" into cells with atomics unless influences are extremely rare.

**Better Approach: Gather-Based**

1. **Register Influencers:** Add `ManaInfluence` entities to spatial index
2. **Cells Gather Contributions:** Each cell queries spatial index for nearby influencers
3. **Deterministic & Parallel-Friendly:** No race conditions, fully parallelizable

**Implementation Pattern:**

```csharp
// Per cell update (parallel job)
for each cell:
    var cellPos = GetCellCenter(cellIndex);
    var influences = SpatialQueryHelper.GetEntitiesWithinRadius(
        cellPos, maxInfluenceRadius, spatialGrid);
    
    float influenceDelta = 0f;
    for each influencer in influences:
        var influence = GetComponent<ManaInfluence>(influencer);
        var influencerPos = GetComponent<LocalTransform>(influencer).Position;
        var distance = math.distance(cellPos, influencerPos);
        
        if (distance <= influence.Radius)
        {
            var falloff = ComputeFalloff(distance, influence.Radius, influence.FalloffMode);
            influenceDelta += influence.Strength * falloff;
        }
    
    // Apply influenceDelta to cell
    effectiveDelta = influenceDelta;
```

**Spatial Index Integration:**
- Use existing `SpatialGridConfig` / `SpatialQueryHelper`
- Cells maintain short list of influencer IDs from spatial hash/grid buckets
- No atomics needed, deterministic, Burst-friendly

---

## Field Update Loop (Stable + Scalable)

### Update Formula

**Per-Cell Update (deterministic):**

```
mana' = 
    mana
  + regenRate * dt * (base - mana)           // Relax toward base
  + diffusion * dt * (sumNeighbors(mana) - k * mana)  // Spread to neighbors
  + influenceDelta                            // Dynamic influences
  clamp(0, maxMana)
```

**Where:**
- `regenRate`: Rate toward base (per second)
- `diffusion`: Spread coefficient (how fast mana flows to neighbors)
- `k`: Number of neighbors (typically 4 for 2D grid, 6 for 3D)
- `influenceDelta`: Sum of all influence contributions (gathered from spatial index)

**Neighbor Summation:**

```csharp
// 2D grid (4 neighbors: N, S, E, W)
float sumNeighbors = 
    GetCellMana(cellX + 1, cellY) +
    GetCellMana(cellX - 1, cellY) +
    GetCellMana(cellX, cellY + 1) +
    GetCellMana(cellX, cellY - 1);

float k = 4f; // Number of neighbors
```

### Dirty-Region Updates (Performance LOD)

**Only Update Active Regions:**

1. **Cells Near Casters:** Currently casting entities (active gameplay)
2. **Cells Near Moving Influencers:** Emitters/siphons that moved (dynamic changes)
3. **Cells Far From Base:** After big edits/explosions/rituals (significant changes)
4. **Everything Else:** Relax at slower cadence (simulation LOD)

**Update Tiers:**

```csharp
public enum ManaUpdateTier : byte
{
    Critical = 0,    // Every tick: cells near active casters
    Active = 1,      // Every 4 ticks: cells near moving influencers
    Passive = 2,     // Every 16 ticks: cells far from base
    Inactive = 3     // Every 64 ticks: cells at base, no activity
}
```

**Dirty Marking:**

```csharp
// Mark cells as dirty when:
- Caster enters cell (mark cell + neighbors)
- Influencer moves (mark old cells + new cells)
- Ritual/spell consumes/produces mana (mark affected cells)
- Player edits field (paint tool, place influencer)

// Update system only processes dirty cells
```

**Update Cadence:**

```csharp
// Configurable update rate (not every tick if not needed)
public struct ManaGridUpdateConfig : IComponentData
{
    public float UpdateIntervalSeconds;  // Default: 0.1s (10 Hz)
    public byte ActiveRadiusCells;       // Cells around casters to update every tick
    public byte InfluenceUpdateRadius;   // Cells around influencers to update frequently
}
```

---

## Sampling API (How Gameplay Uses It)

### Extension to EnvironmentSampler

**Add to EnvironmentSampler:**

```csharp
public struct EnvironmentSampler
{
    // ... existing methods ...
    
    public EnvironmentScalarSample SampleManaDetailed(float3 worldPosition, float defaultValue = 0f)
    {
        if (!TryGetSingletonEntity(_manaGridQuery, out var gridEntity))
        {
            return new EnvironmentScalarSample(defaultValue, 0f);
        }

        var grid = _manaGridLookup[gridEntity];
        
        float baseValue;
        if (_manaRuntimeLookup.HasBuffer(gridEntity))
        {
            var runtime = _manaRuntimeLookup[gridEntity].AsNativeArray();
            baseValue = EnvironmentGridMath.SampleBilinear(grid.Metadata, runtime, worldPosition, defaultValue);
        }
        else
        {
            baseValue = grid.SampleBilinear(worldPosition, defaultValue);
        }

        var contribution = SampleScalarContribution(grid.ChannelId, worldPosition, 0f);
        return new EnvironmentScalarSample(baseValue, contribution);
    }
    
    public float SampleMana(float3 worldPosition, float defaultValue = 0f)
    {
        return SampleManaDetailed(worldPosition, defaultValue).Value;
    }
    
    // Optional: Separate disruption channel
    public float SampleManaDisruption(float3 worldPosition, float defaultValue = 0f)
    {
        // Query disruption channel (for inhibitors)
        return SampleScalarContribution(disruptionChannelId, worldPosition, defaultValue);
    }
}
```

**Extended Environment Sample Struct:**

```csharp
public struct EnvironmentSample
{
    // ... existing fields ...
    public float Mana;                    // 0.0 to 1.0
    public float ManaDisruption;          // 0.0 to 1.0 (optional separate channel)
    public MediumType Medium;             // Gas/Liquid/Vacuum/Solid (for "psionics in vacuum")
}
```

### Spellcasting Integration

**Spells Query Mana:**

```csharp
// In spell casting system
var env = EnvironmentSampler.SampleManaDetailed(casterPosition);
var mana = env.Value;  // Base + Contributions
var disruption = EnvironmentSampler.SampleManaDisruption(casterPosition);

// Cost calculation
float costMultiplier = ComputeCostMultiplier(mana, disruption, casterAffinity);
float actualCost = baseCost * costMultiplier;

// Casting check
bool canCast = mana >= minimumThreshold && disruption < disruptionThreshold;
```

**Cost Curves:**

```csharp
// Cost: cheap in rich areas, expensive in poor areas
float ComputeCostMultiplier(float mana, float disruption, float casterAffinity)
{
    // Mana availability curve (lerp from max cost to min cost)
    float manaFactor = math.lerp(
        MaxCostMultiplier,    // 2.0x cost in poor areas
        MinCostMultiplier,    // 0.5x cost in rich areas
        math.smoothstep(LowManaThreshold, HighManaThreshold, mana)
    );
    
    // Disruption penalty (inhibitors increase cost/failure)
    float disruptionPenalty = math.smoothstep(
        DisruptLowThreshold,  // 0.3
        DisruptHighThreshold, // 0.7
        disruption
    );
    
    // Caster affinity (some casters work better in certain mana conditions)
    float affinityBonus = casterAffinity * (mana - 0.5f); // Bonus in rich, penalty in poor
    
    return manaFactor * (1f + disruptionPenalty) - affinityBonus;
}
```

**Failure/Disruption Probability:**

```csharp
// For inhibitors: chance spell fails due to disruption
float ComputeFailureProbability(float disruption)
{
    return math.smoothstep(DisruptLowThreshold, DisruptHighThreshold, disruption);
    // 0% at low disruption, 100% at high disruption
}
```

---

## Natural Mana Siphons (Mobile Inhibitors)

### Implementation as ManaInfluence

**Mobile inhibitors are just ManaInfluence components:**

```csharp
// Null-zone inhibitor (mobile)
var nullZone = new ManaInfluence
{
    Strength = -1.0f,              // Strong sink
    Radius = 10f,                  // 10m radius
    FalloffMode = FalloffMode.Smoothstep,
    InfluenceMode = InfluenceMode.ClampMax,  // Caps mana to zero
    UpdateRateTier = UpdateRateTier.Active   // Updates frequently
};

// Optional: Drain to self (siphon gains internal mana)
public struct ManaSiphonComponent : IComponentData
{
    public float InternalManaPool;     // Siphon's internal storage
    public float DrainEfficiency;      // How much drained mana converts to internal
}
```

**Moving Null Bubble:**

```csharp
// When inhibitor moves, mark dirty regions
void OnInhibitorMoved(Entity inhibitor, float3 oldPos, float3 newPos)
{
    // Mark old cells as dirty (mana can regen)
    MarkCellsDirty(GetCellsInRadius(oldPos, inhibitor.Radius));
    
    // Mark new cells as dirty (mana gets drained)
    MarkCellsDirty(GetCellsInRadius(newPos, inhibitor.Radius));
    
    // Update spatial index (influencer moved)
    SpatialGridSystem.UpdateEntity(inhibitor, newPos);
}
```

**Update Only Bubble Region:**

- Only update cells within inhibitor radius (not entire grid)
- Use spatial queries to find affected cells
- Efficient for mobile inhibitors

---

## Integration with Entity Mana Pools (Optional but Powerful)

### Dual Mana System

**Both Internal and Ambient:**

1. **Internal Mana (ManaPool):** Personal reserves (entity component)
   ```csharp
   public struct ManaPool : IComponentData
   {
       public float CurrentMana;      // 0.0 to MaxMana
       public float MaxMana;          // Entity's capacity
       public float RegenRate;        // Base regen per second
   }
   ```

2. **Ambient Mana (Field):** Efficiency/availability modifier (from grid)
   - Queried via `EnvironmentSampler.SampleMana(pos)`
   - Affects regen speed, casting cost, max channeling

**Combined Effects:**

```csharp
// In rich areas: regen faster, cast cheaper, higher max channeling
float ambientMana = EnvironmentSampler.SampleMana(entityPos);

// Regen modified by ambient
float effectiveRegen = baseRegen * (0.5f + ambientMana); // 0.5x to 1.5x

// Cast cost modified by ambient
float costMultiplier = ComputeCostMultiplier(ambientMana, disruption, affinity);

// Max channeling modified by ambient (sustained spells)
float maxChanneling = baseMaxChanneling * (0.7f + 0.6f * ambientMana);
```

**Siphon Aura Effect:**

```csharp
// Siphon suppresses channeling even if you have internal mana
float disruption = EnvironmentSampler.SampleManaDisruption(entityPos);
if (disruption > DisruptionThreshold)
{
    // Cannot channel (sustained spells fail)
    // But instant spells might still work (lower disruption threshold)
}
```

**Design Benefit:**
- Casters playable anywhere (internal mana pool)
- Terrain still meaningful (ambient affects efficiency)
- Strategic positioning matters (seek rich areas, avoid siphons)

---

## Ship Interiors vs Planets (Two Backends, Same API)

### Dual Backend Architecture

**Planet/Outdoor: Grid Field**
- Uses `ManaGrid` (2D/3D grid cells)
- Diffusion between neighbors
- Standard spatial sampling

**Ship Interior: Compartment Graph**
- Nodes = rooms/compartments
- Edges = doors/vents (conductivity)
- Diffusion along edges

**Same API, Different Provider:**

```csharp
// Unified interface
public interface IManaFieldProvider
{
    EnvironmentScalarSample SampleMana(float3 position);
    void ApplyInfluence(float3 position, float strength, float radius);
}

// Planet implementation
public struct GridManaFieldProvider : IManaFieldProvider
{
    ManaGrid grid;
    // ... grid-based sampling
}

// Ship interior implementation
public struct CompartmentManaFieldProvider : IManaFieldProvider
{
    CompartmentGraph graph;
    // ... graph-based sampling
}
```

**Compartment Graph Details:**

```csharp
public struct ManaCompartmentNode
{
    public float Mana;
    public float ManaBase;
    public float ManaRegenRate;
    public float3 Position;
}

public struct ManaCompartmentEdge
{
    public int FromNode;
    public int ToNode;
    public float Conductivity;  // 0 = sealed, 1 = open door
    public ManaSealType SealType; // None, Door, Vent, Bulkhead
}

// Diffusion along edges
manaDiffusion = conductivity * (neighborMana - thisMana) * dt;
```

**Seals/Doors:**

```csharp
// Doors/vents reduce conductivity
if (edge.SealType != ManaSealType.None)
{
    conductivity *= SealConductivityMultiplier[edge.SealType];
    // Door: 0.1x, Vent: 0.5x, Bulkhead: 0.01x
}

// Siphon in compartment affects node + nearby nodes
// Query graph for neighbors, apply influence decayed by edge conductivity
```

**Benefits:**
- Same systems work for planets and ship interiors
- Different field providers handle domain-specific logic
- Spellcasting code doesn't need to know which backend

---

## Authoring + Player Editor Tools

### Authoring Components

**ManaLayerConfigAuthoring (ScriptableObject):**

```csharp
[CreateAssetMenu(fileName = "ManaLayerConfig", menuName = "PureDOTS/Mana Layer Config")]
public class ManaLayerConfigAuthoring : ScriptableObject
{
    [Header("Base Levels")]
    public AnimationCurve BaseManaByBiome;      // Biome → base mana mapping
    public float DefaultBaseMana = 0.5f;
    
    [Header("Regen & Diffusion")]
    public float DefaultRegenRate = 0.1f;       // Per second
    public float DefaultDiffusionRate = 0.05f;  // Spread rate
    
    [Header("Grid Settings")]
    public float CellSize = 10f;                // Meters per cell
    public int2 GridResolution = new int2(100, 100);
    
    [Header("Seals")]
    public ManaSealConfig[] SealTypes;          // Different seal qualities/types
}
```

**ManaInfluenceAuthoring (MonoBehaviour):**

```csharp
public class ManaInfluenceAuthoring : MonoBehaviour
{
    public float Strength = 1.0f;
    public float Radius = 10f;
    public ManaFalloffMode FalloffMode = ManaFalloffMode.Smoothstep;
    public ManaInfluenceMode Mode = ManaInfluenceMode.Additive;
    public int UpdateRateTier = 0;
    
    // Baker converts to ManaInfluence component
}
```

### Player Editor Tools

**ManaPaintTool:**

```csharp
// Runtime tool for editing mana field
public struct ManaPaintCommand : ICommandData
{
    public float3 Position;
    public float Radius;
    public ManaPaintMode Mode;  // Raise, Lower, SetBase, SetSeal
    public float Value;
}

// Commands mark dirty regions and apply deltas
void ApplyPaintCommand(ManaPaintCommand cmd)
{
    var affectedCells = GetCellsInRadius(cmd.Position, cmd.Radius);
    foreach (var cell in affectedCells)
    {
        switch (cmd.Mode)
        {
            case ManaPaintMode.Raise:
                cell.Mana += cmd.Value;
                break;
            case ManaPaintMode.Lower:
                cell.Mana -= cmd.Value;
                break;
            case ManaPaintMode.SetBase:
                cell.ManaBase = cmd.Value;
                break;
            case ManaPaintMode.SetSeal:
                cell.ManaHardBlock = (byte)(cmd.Value * 255f);
                break;
        }
        MarkCellDirty(cell);
    }
    
    // No full-grid recompute, just mark dirty and update incrementally
}
```

**Runtime Debug Overlay (Heatmap):**

```csharp
// Visualize mana field like moisture overlay
public struct ManaGridOverlay : IComponentData
{
    public bool ShowManaLevels;       // Color-coded mana (blue = low, green = high)
    public bool ShowInfluences;       // Show emitter/siphon radii
    public bool ShowSeals;            // Show sealed/nullstone cells
    public bool ShowDirtyRegions;     // Debug: show which cells updating
}

// Render heatmap using existing overlay system
// Blue (0.0) → Green (0.5) → Yellow (1.0)
```

**Place Siphon Tool:**

```csharp
// Runtime tool to place inhibitors
public struct PlaceSiphonCommand : ICommandData
{
    public float3 Position;
    public float Strength;
    public float Radius;
    public ManaInfluenceMode Mode;
}

// Creates entity with ManaInfluence component
Entity CreateSiphon(PlaceSiphonCommand cmd)
{
    var entity = EntityManager.CreateEntity();
    EntityManager.AddComponent<ManaInfluence>(entity);
    EntityManager.AddComponent<LocalTransform>(entity);
    
    EntityManager.SetComponentData(entity, new ManaInfluence
    {
        Strength = -cmd.Strength,  // Negative = sink
        Radius = cmd.Radius,
        Mode = cmd.Mode
    });
    
    EntityManager.SetComponentData(entity, LocalTransform.FromPosition(cmd.Position));
    
    return entity;
}
```

---

## MVP Slice (Fastest Path)

### Phase 1: Core Field + Sampler

1. **Add Mana scalar field:**
   - Create `ManaGrid` component (similar to `MoistureGrid`)
   - Create `ManaGridBlob` with cell data (Mana, ManaBase, RegenRate, Diffusion)
   - Create `ManaGridRuntimeCell` buffer for dynamic updates

2. **Add ManaInfluence sinks/sources:**
   - Create `ManaInfluence` component
   - Use existing `EnvironmentScalarContribution` buffer pattern
   - Implement gather-based influence application (no atomics)

3. **Extend EnvironmentSampler:**
   - Add `SampleMana()` method
   - Add `SampleManaDetailed()` method
   - Follow existing moisture/temperature patterns

4. **Hook spell cost/success:**
   - Spell casting system queries `SampleMana(pos)`
   - Apply cost multiplier based on mana availability
   - Optional: Check disruption for failure probability

5. **Add debug heatmap:**
   - Visualize mana levels (blue → green → yellow)
   - Show influence radii
   - Show sealed cells

6. **Add "place siphon" tool:**
   - Runtime command to create inhibitor entity
   - Visual feedback (radius visualization)

---

## Technical Implementation Details

### Data Structures

```csharp
// Grid singleton
public struct ManaGrid : IComponentData
{
    public EnvironmentGridMetadata Metadata;
    public BlobAssetReference<ManaGridBlob> BaseValues;
    public ushort ChannelId;  // For EnvironmentScalarContribution
}

// Base grid data (authoring/initial state)
public struct ManaGridBlob
{
    public BlobArray<ManaCellBlob> Cells;
}

// Runtime cell buffer (for dynamic updates)
[InternalBufferCapacity(0)]
public struct ManaGridRuntimeCell : IBufferElementData
{
    public ushort Mana;          // Q16.16: 0.0 to 1.0
    public ushort ManaBase;      // Q16.16: relaxation target
    public byte UpdateTier;      // Update cadence tier
    public byte DirtyFlag;       // 0 = clean, 1 = dirty
}

// Influence component (on entities)
public struct ManaInfluence : IComponentData
{
    public float Strength;       // Positive = emitter, Negative = siphon
    public float Radius;
    public ManaFalloffMode FalloffMode;
    public ManaInfluenceMode Mode;
    public byte UpdateRateTier;
    public ushort ChannelId;
}

public enum ManaFalloffMode : byte
{
    Linear = 0,
    Smoothstep = 1,
    InverseSquare = 2
}

public enum ManaInfluenceMode : byte
{
    Additive = 0,        // Push mana up/down
    ClampMax = 1,        // Cap mana to maximum
    SuppressCasting = 2  // Add disruption (separate channel)
}
```

### Update System

```csharp
[UpdateInGroup(typeof(EnvironmentSystemGroup))]
[BurstCompile]
public partial struct ManaGridUpdateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Mark dirty cells (casters, moving influencers, edits)
        // 2. Gather influences (spatial query for ManaInfluence entities)
        // 3. Update dirty cells (regen + diffusion + influences)
        // 4. Apply clamps and bounds
    }
}
```

### Integration with Inhibition System

**Unified with Inhibition System:**
- Null-zones use `ManaInfluence` with `Mode = ClampMax, Strength = -1.0`
- Silence areas don't affect mana field (affect casting method, not mana)
- Mana drain inhibitors use `Mode = Additive, Strength < 0`
- Spell absorption uses `Mode = SuppressCasting` (disruption channel)

**Quality Integration:**
- Inhibitor quality affects `Strength` and `Radius` of `ManaInfluence`
- Higher quality = stronger suppression, larger radius
- Follows material > skill > craft > product quality formula

---

## Performance Considerations

### Optimization Strategies

1. **Dirty-Region Updates:** Only update cells that changed or are near active entities
2. **Update Tiering:** Different cadence for active vs. passive regions
3. **Spatial Indexing:** Use existing spatial grid for efficient influence gathering
4. **Burst Compilation:** All update jobs must be Burst-compilable
5. **Fixed-Point Math:** Q16.16 fixed-point for deterministic replay

### Scalability

**Target Performance:**
- 1M entity support (inhibitors are rare, <1000)
- Grid sizes: 100×100 to 1000×1000 cells
- Update rate: 10-60 Hz (configurable)
- Influence queries: O(log n) via spatial index

**Memory Budget:**
- Grid cells: 2-4 bytes per cell (ushort mana + metadata)
- Runtime buffers: Only for active/dirty cells
- Influence entities: ~32 bytes per influencer

---

## Related Documentation

- **Inhibition System:** `Docs/Concepts/Magic/Inhibition_System.md` - Unified with mana grid
- **Environment Fields:** `Docs/Concepts/Core/Simulation_LOD_And_Environment_Fields.md` - Architecture foundation
- **Languages & Magic:** `Docs/Concepts/Core/Languages_And_Magic_System.md` - Spell casting integration
- **Quality System:** `Docs/Concepts/Economy/Resource_Production_And_Crafting.md` - Quality formulas
- **Spatial Systems:** `Packages/com.moni.puredots/Runtime/Runtime/Spatial/` - Spatial index integration

---

**For Implementers:** Focus on EnvironmentSampler integration, gather-based influence application, and dirty-region update logic  
**For Designers:** Focus on cost curves, disruption thresholds, and strategic positioning gameplay

