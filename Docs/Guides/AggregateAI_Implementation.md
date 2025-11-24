# Aggregate Entity Behavior & Registry Sync Implementation Guide

**Date:** 2025-01-24  
**Status:** Implementation Plan  
**Purpose:** Guide for implementing aggregate entity (village/band/guild) behaviors and registry sync

---

## Current State Assessment

### Villages

**Implemented:**
- `VillageComponents.cs`: Core village data structures
  - `Village`: Village ID, name
  - `VillageState`: State machine (Nascent, Established, Ascendant, Collapsing)
  - `VillageAlignmentState`: Aggregate alignment (moral, order, purity axes)
  - `VillageInitiativeState`: Initiative band, tick budget, stress
  - `VillageResourceSummary`: Food, construction, specialty resources
  - `VillageWorshipState`: Morale, worship intensity, faith, mana generation
  - `VillageMember`: Buffer of villager entities
  - `VillageEvent`: Event history buffer

- `VillageAITelemetrySystem.cs`: Telemetry emission for village metrics
- Registry sync: `GodgameRegistryBridgeSystem` partially syncs villages (no dedicated village registry in PureDOTS yet)

**Gaps:**
- No village aggregation system (computing alignment/behavior from members)
- No village-level AI decision-making
- No village presentation bindings
- No village project planning (construction, expansion)

### Bands

**Implemented:**
- `BandFormationSystem.cs`: Formation detection (stub implementation)
- `GodgameBand` component: Band data mirror
- Registry sync: `GodgameRegistryBridgeSystem.UpdateBandRegistry` syncs to `BandRegistry`

**Gaps:**
- Band formation logic incomplete (probability calculation stubs)
- No band goal execution system
- No band presentation bindings
- No member coordination/formation management

### Guilds

**Implemented:**
- PureDOTS has `GuildComponents` (shared package)
- Godgame has no guild systems

**Gaps:**
- No guild formation system
- No guild AI decision-making
- No guild presentation bindings

---

## Implementation Plan

### 1. Village Aggregation System

**Purpose:** Compute village-level alignment, behavior, and stats from member villagers

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villages/VillageAggregationComponents.cs
public struct VillageAggregationState : IComponentData
{
    public float AverageBoldScore;
    public float AverageVengefulScore;
    public sbyte AverageMoralAxis;
    public sbyte AverageOrderAxis;
    public sbyte AveragePurityAxis;
    public byte ExtremeBoldCount;      // Count with |bold| > 60
    public byte ExtremeVengefulCount;  // Count with |vengeful| > 60
    public float AverageInitiative;
    public float AverageMorale;
    public float AverageHealth;
    public uint LastAggregationTick;
}
```

**System to Create:**
- `VillageAggregationSystem.cs`: Aggregates member villager stats into village-level stats
- Runs periodically (every N ticks) or on member changes
- Updates `VillageAlignmentState`, `VillageInitiativeState`, `VillageWorshipState` from aggregated data

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villages/VillageAggregationComponents.cs`
- New: `Assets/Scripts/Godgame/Villages/VillageAggregationSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villages/VillageComponents.cs` (ensure member buffer is populated)
- New: `Assets/Scripts/Godgame/Tests/VillageAggregationTests.cs`

**Integration Points:**
- Wire into `VillageAITelemetrySystem` to emit aggregated stats
- Wire into `GodgameRegistryBridgeSystem` for registry sync

---

### 2. Village AI Decision System

**Purpose:** Village-level autonomous decision-making (projects, expansion, crisis response)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villages/VillageAIDecisionComponents.cs
public struct VillageProject : IBufferElementData
{
    public VillageProjectType Type;
    public Entity TargetEntity;        // Building site, expansion area, etc.
    public float Priority;
    public float Progress;
    public float RequiredResources;
    public uint StartTick;
    public uint TargetCompletionTick;
}

public enum VillageProjectType : byte
{
    None = 0,
    BuildStorehouse = 1,
    BuildTemple = 2,
    ExpandTerritory = 3,
    DefendVillage = 4,
    GatherResources = 5,
    TradeMission = 6
}

public struct VillageCrisisState : IComponentData
{
    public CrisisType Type;
    public float Severity;           // 0-1, 1 = critical
    public uint StartTick;
    public uint EstimatedDurationTicks;
}

public enum CrisisType : byte
{
    None = 0,
    Famine = 1,
    Attack = 2,
    Disease = 3,
    ResourceShortage = 4,
    MoralCrisis = 5
}
```

**System to Create:**
- `VillageAIDecisionSystem.cs`: Evaluates village state, creates projects, responds to crises
- Considers village alignment, resources, member needs
- Creates job offers for villagers via `VillagerJobSchedulerSystem`

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villages/VillageAIDecisionComponents.cs`
- New: `Assets/Scripts/Godgame/Villages/VillageAIDecisionSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerSystem.cs` (consume village projects)
- New: `Assets/Scripts/Godgame/Tests/VillageAIDecisionTests.cs`

**PureDOTS Integration:**
- Use PureDOTS `VillageBehaviorComponents` and `AggregateBehaviorProfile` for behavior configuration
- Wire village decisions to PureDOTS behavior systems

---

### 3. Village Presentation System

**Purpose:** Visual representation of villages (building styles, expansion visuals, culture visualization)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villages/VillagePresentationComponents.cs
public struct GodgameVillagePresentation : IComponentData
{
    public Entity VillageCenterEntity;      // Main building/center
    public FixedString64Bytes BuildingStyleId;  // "good_temple", "evil_fortress", etc.
    public float ExpansionRadius;
    public float CultureVisualization;      // -1 to +1, affects visual style
}

public struct VillageBuildingCluster : IBufferElementData
{
    public Entity BuildingEntity;
    public BuildingType Type;
    public float3 Position;
}
```

**System to Create:**
- `VillagePresentationSystem.cs`: Updates presentation based on village state
- Maps alignment to building styles
- Maps behavior to expansion visuals
- Updates building clusters based on village projects

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villages/VillagePresentationComponents.cs`
- New: `Assets/Scripts/Godgame/Villages/VillagePresentationSystem.cs`
- Modify: `Assets/Scripts/Godgame/Presentation/GodgamePresentationBindingBootstrapSystem.cs` (add village presentation IDs)
- New: `Assets/Scripts/Godgame/Tests/VillagePresentationTests.cs`

**Integration Points:**
- Wire to `VillageAggregationSystem` for alignment/behavior updates
- Wire to `VillageAIDecisionSystem` for project visualization

---

### 4. Band Formation System Completion

**Purpose:** Complete band formation logic with proper probability calculation and member coordination

**Enhancements to `BandFormationSystem.cs`:**
- Implement `CalculateAlignmentCompatibility`: Compare villager alignments
- Implement `CalculateOutlookCompatibility`: Compare villager outlooks
- Add spatial queries for nearby compatible villagers
- Add leader election based on initiative/alignment
- Add member coordination (formation positions, movement)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Bands/BandFormationComponents.cs (extend existing)
public struct BandGoal : IComponentData
{
    public BandGoalType Type;
    public Entity TargetEntity;
    public float3 TargetPosition;
    public float Priority;
    public uint StartTick;
}

public enum BandGoalType : byte
{
    None = 0,
    Raid = 1,
    Explore = 2,
    Defend = 3,
    Gather = 4,
    Escort = 5
}
```

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Bands/BandFormationSystem.cs` (complete formation logic)
- New: `Assets/Scripts/Godgame/Bands/BandAIDecisionSystem.cs` (goal execution)
- New: `Assets/Scripts/Godgame/Tests/BandFormationTests.cs`

**PureDOTS Integration:**
- Use PureDOTS `BandComponents` and `BandFormationCandidate` structures
- Wire band goals to PureDOTS band behavior systems

---

### 5. Band Presentation System

**Purpose:** Visual representation of bands (formation visualization, group movement, combat formations)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Bands/BandPresentationComponents.cs
public struct GodgameBandPresentation : IComponentData
{
    public Entity LeaderEntity;
    public FormationType CurrentFormation;
    public float3 FormationCenter;
    public float FormationRadius;
}

public enum FormationType : byte
{
    Loose = 0,
    Tight = 1,
    Line = 2,
    Wedge = 3,
    Circle = 4
}
```

**System to Create:**
- `BandPresentationSystem.cs`: Updates band visual representation
- Maps alignment to banner/colors
- Maps behavior to formation style
- Updates member positions based on formation

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Bands/BandPresentationComponents.cs`
- New: `Assets/Scripts/Godgame/Bands/BandPresentationSystem.cs`
- Modify: `Assets/Scripts/Godgame/Presentation/GodgamePresentationBindingBootstrapSystem.cs` (add band presentation IDs)
- New: `Assets/Scripts/Godgame/Tests/BandPresentationTests.cs`

---

### 6. Guild Formation System

**Purpose:** Create and manage guilds (guild halls, missions, member management)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Guilds/GuildComponents.cs
public struct GodgameGuild : IComponentData
{
    public int GuildId;
    public FixedString64Bytes GuildName;
    public GuildType Type;
    public Entity GuildHallEntity;
    public int MemberCount;
}

public enum GuildType : byte
{
    None = 0,
    Merchant = 1,
    Warrior = 2,
    Scholar = 3,
    Religious = 4
}

public struct GuildMission : IBufferElementData
{
    public MissionType Type;
    public Entity TargetEntity;
    public float Reward;
    public uint ExpirationTick;
}

public enum MissionType : byte
{
    None = 0,
    Escort = 1,
    Gather = 2,
    Combat = 3,
    Trade = 4
}
```

**System to Create:**
- `GuildFormationSystem.cs`: Detects guild formation candidates, creates guilds
- `GuildAIDecisionSystem.cs`: Creates missions, manages members
- `GuildPresentationSystem.cs`: Visual representation of guilds

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Guilds/GuildComponents.cs`
- New: `Assets/Scripts/Godgame/Guilds/GuildFormationSystem.cs`
- New: `Assets/Scripts/Godgame/Guilds/GuildAIDecisionSystem.cs`
- New: `Assets/Scripts/Godgame/Guilds/GuildPresentationSystem.cs`
- New: `Assets/Scripts/Godgame/Tests/GuildFormationTests.cs`

**PureDOTS Integration:**
- Use PureDOTS `GuildComponents` structures
- Wire guild systems to PureDOTS guild behavior systems

---

### 7. Aggregate Registry Sync Enhancement

**Purpose:** Enhance registry sync for villages/bands/guilds with continuity and spatial support

**Enhancements to `GodgameRegistryBridgeSystem.cs`:**
- Add village registry sync (if PureDOTS adds `VillageRegistry`)
- Enhance band registry sync with aggregate stats
- Add guild registry sync (if PureDOTS adds `GuildRegistry`)
- Add spatial continuity for aggregate entities
- Add aggregate telemetry emission

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Registry/Registry/GodgameRegistryBridgeSystem.cs` (enhance aggregate sync)
- Modify: `Assets/Scripts/Godgame/Registry/VillageAITelemetrySystem.cs` (enhance telemetry)
- New: `Assets/Scripts/Godgame/Tests/AggregateRegistrySyncTests.cs`

**PureDOTS Integration:**
- Use PureDOTS `RegistryContinuitySnapshot` for aggregate entities
- Wire aggregate telemetry to PureDOTS `TelemetryStream`

---

## Implementation Order

### Phase 1: Village Aggregation (High Priority)
1. Implement village aggregation system
2. Wire aggregation into telemetry
3. Add tests for aggregation

### Phase 2: Village AI Decisions (High Priority)
1. Implement village AI decision system
2. Wire village projects to job scheduler
3. Add tests for village decisions

### Phase 3: Village Presentation (Medium Priority)
1. Implement village presentation system
2. Wire presentation to aggregation/AI systems
3. Add tests for presentation

### Phase 4: Band Formation Completion (Medium Priority)
1. Complete band formation logic
2. Implement band AI decision system
3. Add tests for band formation

### Phase 5: Band Presentation (Low Priority)
1. Implement band presentation system
2. Wire presentation to band AI
3. Add tests for band presentation

### Phase 6: Guild Systems (Future)
1. Implement guild formation system
2. Implement guild AI decision system
3. Implement guild presentation system
4. Add tests for guild systems

---

## Testing Strategy

### Unit Tests
- `VillageAggregationTests.cs`: Test aggregation from member stats
- `VillageAIDecisionTests.cs`: Test village project creation, crisis response
- `VillagePresentationTests.cs`: Test presentation updates
- `BandFormationTests.cs`: Test band formation probability, member selection
- `BandAIDecisionTests.cs`: Test band goal execution
- `BandPresentationTests.cs`: Test band visual representation
- `GuildFormationTests.cs`: Test guild creation, mission generation

### Integration Tests
- `VillageAggregationToAIDecisionTests.cs`: Test aggregation feeding AI decisions
- `VillageAIDecisionToJobSchedulerTests.cs`: Test village projects creating job offers
- `BandFormationToAIDecisionTests.cs`: Test band formation leading to goal execution
- `AggregateRegistrySyncTests.cs`: Test aggregate entities syncing to registries

### PlayMode Tests
- `VillageAI_FullLoop_Playmode.cs`: Test complete village AI loop
- `BandAI_FullLoop_Playmode.cs`: Test complete band AI loop
- `AggregatePresentation_Playmode.cs`: Test aggregate visual representation

---

## Files Summary

### New Files to Create
- `Assets/Scripts/Godgame/Villages/VillageAggregationComponents.cs`
- `Assets/Scripts/Godgame/Villages/VillageAggregationSystem.cs`
- `Assets/Scripts/Godgame/Villages/VillageAIDecisionComponents.cs`
- `Assets/Scripts/Godgame/Villages/VillageAIDecisionSystem.cs`
- `Assets/Scripts/Godgame/Villages/VillagePresentationComponents.cs`
- `Assets/Scripts/Godgame/Villages/VillagePresentationSystem.cs`
- `Assets/Scripts/Godgame/Bands/BandAIDecisionSystem.cs`
- `Assets/Scripts/Godgame/Bands/BandPresentationComponents.cs`
- `Assets/Scripts/Godgame/Bands/BandPresentationSystem.cs`
- `Assets/Scripts/Godgame/Guilds/GuildComponents.cs`
- `Assets/Scripts/Godgame/Guilds/GuildFormationSystem.cs`
- `Assets/Scripts/Godgame/Guilds/GuildAIDecisionSystem.cs`
- `Assets/Scripts/Godgame/Guilds/GuildPresentationSystem.cs`
- `Assets/Scripts/Godgame/Tests/VillageAggregationTests.cs`
- `Assets/Scripts/Godgame/Tests/VillageAIDecisionTests.cs`
- `Assets/Scripts/Godgame/Tests/VillagePresentationTests.cs`
- `Assets/Scripts/Godgame/Tests/BandFormationTests.cs`
- `Assets/Scripts/Godgame/Tests/BandAIDecisionTests.cs`
- `Assets/Scripts/Godgame/Tests/BandPresentationTests.cs`
- `Assets/Scripts/Godgame/Tests/GuildFormationTests.cs`
- `Assets/Scripts/Godgame/Tests/AggregateRegistrySyncTests.cs`

### Files to Modify
- `Assets/Scripts/Godgame/Bands/BandFormationSystem.cs` (complete formation logic)
- `Assets/Scripts/Godgame/Registry/Registry/GodgameRegistryBridgeSystem.cs` (enhance aggregate sync)
- `Assets/Scripts/Godgame/Registry/VillageAITelemetrySystem.cs` (enhance telemetry)
- `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerSystem.cs` (consume village projects)
- `Assets/Scripts/Godgame/Presentation/GodgamePresentationBindingBootstrapSystem.cs` (add aggregate presentation IDs)

---

## PureDOTS Integration Points

1. **Aggregate Behavior Profiles**: Use PureDOTS `AggregateBehaviorProfile` for behavior configuration
2. **Village Behavior Components**: Use PureDOTS `VillageBehaviorComponents` structures
3. **Band Components**: Use PureDOTS `BandComponents` structures
4. **Guild Components**: Use PureDOTS `GuildComponents` structures
5. **Registry Continuity**: Use PureDOTS `RegistryContinuitySnapshot` for aggregate entities
6. **Telemetry**: Wire aggregate telemetry to PureDOTS `TelemetryStream`

---

## Related Documentation

- `Docs/DemoReadiness_GapAnalysis.md` - Gap analysis
- `Docs/Guides/Godgame_PureDOTS_Entity_Mapping.md` - Entity mapping guide
- `Docs/Concepts/Villagers/Village_Villager_Alignment.md` - Village alignment design
- `Docs/Concepts/Villagers/Entity_Relations_And_Interactions.md` - Aggregate relations design

