# AI Integration Kickoff - Implementation Summary

**Date:** 2025-01-24  
**Status:** Updated - Villager AI Expansion Plan  
**Last Updated:** 2025-01-24

## Overview

This document summarizes the AI integration work and provides an implementation plan for expanding villager AI behaviors to support a fully functional demo.

---

## Current State Assessment

### Implemented Systems

1. **Basic Job Loop** (`VillagerJobSystem.cs`)
   - State machine: Idle → NavigateToNode → Gather → NavigateToStorehouse → Deliver
   - Resource node selection (nearest, matching type)
   - Resource node depletion tracking
   - Storehouse delivery via `StorehouseApi`
   - Burst-compiled, parallel `IJobEntity` execution

2. **Personality System** (`VillagerPersonalitySystem.cs`)
   - Grudge generation/decay
   - Patriotism drift
   - Combat stance modifiers
   - Initiative boosts from grudges

3. **Utility Scheduler** (`VillagerUtilityScheduler.cs`)
   - Initiative calculation (band + personality modifiers)
   - Base utility calculation functions (stubs)
   - Autonomous action selection (stub implementation)

4. **Registry Sync** (`GodgameVillagerSyncSystem.cs`)
   - Mirrors villager data to `VillagerRegistry`
   - Spatial continuity support
   - Telemetry emission

---

## Required Upgrades

### 1. Interrupt Handling System

**Purpose:** Handle interruptions to villager jobs (hand pickup, path blocked, urgent needs)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villagers/VillagerInterruptComponents.cs
public struct VillagerInterrupt : IComponentData
{
    public InterruptType Type;
    public Entity SourceEntity;
    public float Priority;
    public uint TriggerTick;
}

public enum InterruptType : byte
{
    None = 0,
    HandPickup = 1,
    PathBlocked = 2,
    UrgentNeed = 3,
    CombatThreat = 4,
    VillageCommand = 5
}

public struct VillagerJobStateSnapshot : IBufferElementData
{
    public JobType Type;
    public JobPhase Phase;
    public Entity Target;
    public float3 LastPosition;
    public float CarryCount;
}
```

**System to Create:**
- `VillagerInterruptSystem.cs`: Detects interrupts, saves job state, handles resume
- Integration with `VillagerJobSystem`: Check interrupts before job execution, save state on interrupt

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villagers/VillagerInterruptComponents.cs`
- New: `Assets/Scripts/Godgame/Villagers/VillagerInterruptSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerJobSystem.cs` (add interrupt checks)
- New: `Assets/Scripts/Godgame/Tests/VillagerInterruptTests.cs`

---

### 2. Needs Tracking System

**Purpose:** Track villager needs (hunger, fatigue, hygiene, mood) and trigger autonomous actions

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villagers/VillagerNeedsComponents.cs
public struct VillagerNeeds : IComponentData
{
    public byte HungerPercent;      // 0-100, 0 = starving
    public byte FatiguePercent;     // 0-100, 0 = exhausted
    public byte HygienePercent;      // 0-100, 0 = filthy
    public sbyte MoodPercent;        // -100 to +100, negative = unhappy
    public uint LastMealTick;
    public uint LastRestTick;
    public uint LastBathTick;
}

public struct VillagerNeedCurves : IComponentData
{
    public float HungerDecayRate;      // Per tick
    public float FatigueDecayRate;     // Per tick
    public float HygieneDecayRate;     // Per tick
    public float MoodDecayRate;        // Per tick
    public byte HungerThreshold;       // Below this = urgent
    public byte FatigueThreshold;      // Below this = urgent
    public byte HygieneThreshold;      // Below this = urgent
    public sbyte MoodThreshold;        // Below this = urgent
}
```

**System to Create:**
- `VillagerNeedsSystem.cs`: Updates needs over time, triggers urgent need interrupts
- Integration with `VillagerUtilityScheduler`: Use needs in utility calculations

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villagers/VillagerNeedsComponents.cs`
- New: `Assets/Scripts/Godgame/Villagers/VillagerNeedsSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerUtilityScheduler.cs` (wire needs into utility)
- New: `Assets/Scripts/Godgame/Tests/VillagerNeedsTests.cs`

**PureDOTS Integration:**
- PureDOTS has `VillagerNeedsHot` component structure
- Wire Godgame needs to PureDOTS utility calculations via `VillagerUtilityScheduler.CalculateNeedUtility`

---

### 3. Job Scheduler System

**Purpose:** Assign jobs to villagers based on priorities, capacity, and GOAP goals

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerComponents.cs
public struct JobAssignment : IBufferElementData
{
    public Entity VillagerEntity;
    public JobType Type;
    public Entity TargetEntity;
    public ushort ResourceTypeIndex;
    public float Priority;
    public uint AssignmentTick;
}

public struct JobOffer : IComponentData
{
    public JobType Type;
    public Entity TargetEntity;
    public ushort ResourceTypeIndex;
    public float BasePriority;
    public int RequiredWorkers;
    public int AssignedWorkers;
    public uint ExpirationTick;
}
```

**System to Create:**
- `VillagerJobSchedulerSystem.cs`: Creates job offers, assigns jobs to villagers, manages job queue
- Integration with `VillagerJobSystem`: Consume job assignments instead of autonomous selection

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerComponents.cs`
- New: `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerJobSystem.cs` (consume assignments)
- New: `Assets/Scripts/Godgame/Tests/VillagerJobSchedulerTests.cs`

**GOAP Hooks:**
- Future: Add GOAP goal system that generates job offers based on village needs
- Future: Add goal planning system that breaks down high-level goals into job sequences

---

### 4. Improved Storehouse Selection

**Purpose:** Select storehouses based on capacity, priority, and reservation status

**Enhancements to `VillagerJobSystem`:**
- Check storehouse capacity before selecting
- Prefer storehouses with available capacity
- Support reservation system for incoming deliveries
- Consider storehouse priority/type (food vs construction materials)

**Files to Touch:**
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerJobSystem.cs` (improve storehouse selection)
- Modify: `Assets/Scripts/Godgame/Resources/StorehouseApi.cs` (add reservation methods)
- Modify: `Assets/Scripts/Godgame/Tests/Conservation_VillagerGatherDeliver_Playmode.cs` (test improved selection)

---

### 5. Autonomous Action System

**Purpose:** Execute autonomous actions (family formation, business ventures, adventure seeking)

**Components to Add:**
```csharp
// Assets/Scripts/Godgame/Villagers/VillagerAutonomousActionComponents.cs
public struct AutonomousActionRequest : IBufferElementData
{
    public byte ActionType;
    public Entity TargetEntity;
    public float Utility;
    public uint RequestTick;
}

public enum AutonomousActionType : byte
{
    None = 0,
    StartFamily = 1,
    OpenBusiness = 2,
    JoinBand = 3,
    Migrate = 4,
    Revenge = 5,
    Adventure = 6
}
```

**System to Create:**
- `VillagerAutonomousActionSystem.cs`: Evaluates autonomous actions, executes when initiative threshold met
- Integration with `VillagerUtilityScheduler`: Use `SelectAutonomousAction` to choose actions

**Files to Touch:**
- New: `Assets/Scripts/Godgame/Villagers/VillagerAutonomousActionComponents.cs`
- New: `Assets/Scripts/Godgame/Villagers/VillagerAutonomousActionSystem.cs`
- Modify: `Assets/Scripts/Godgame/Villagers/VillagerUtilityScheduler.cs` (wire action selection)
- New: `Assets/Scripts/Godgame/Tests/VillagerAutonomousActionTests.cs`

---

## Implementation Order

### Phase 1: Core Interrupts & Needs (High Priority)
1. Implement interrupt handling system
2. Implement needs tracking system
3. Wire needs into utility calculations
4. Add tests for interrupts and needs

### Phase 2: Job Scheduling (Medium Priority)
1. Implement job scheduler system
2. Improve storehouse selection
3. Wire job scheduler into job system
4. Add tests for job scheduling

### Phase 3: Autonomous Actions (Low Priority)
1. Implement autonomous action system
2. Wire autonomous actions into utility scheduler
3. Add tests for autonomous actions

---

## Testing Strategy

### Unit Tests
- `VillagerInterruptTests.cs`: Test interrupt detection, state saving, resume logic
- `VillagerNeedsTests.cs`: Test needs decay, threshold detection, urgent need triggers
- `VillagerJobSchedulerTests.cs`: Test job assignment, priority calculation, capacity checks
- `VillagerAutonomousActionTests.cs`: Test action selection, execution, utility calculation

### Integration Tests
- `VillagerNeedsToJobIntegrationTests.cs`: Test needs triggering job changes
- `VillagerInterruptToResumeIntegrationTests.cs`: Test interrupt → resume flow
- `VillagerJobSchedulerToJobIntegrationTests.cs`: Test scheduler → job execution flow

### PlayMode Tests
- `VillagerAI_FullLoop_Playmode.cs`: Test complete villager AI loop with interrupts, needs, scheduling
- `VillagerAI_AutonomousActions_Playmode.cs`: Test autonomous actions triggering and executing

---

## Files Summary

### New Files to Create
- `Assets/Scripts/Godgame/Villagers/VillagerInterruptComponents.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerInterruptSystem.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerNeedsComponents.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerNeedsSystem.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerComponents.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerJobSchedulerSystem.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerAutonomousActionComponents.cs`
- `Assets/Scripts/Godgame/Villagers/VillagerAutonomousActionSystem.cs`
- `Assets/Scripts/Godgame/Tests/VillagerInterruptTests.cs`
- `Assets/Scripts/Godgame/Tests/VillagerNeedsTests.cs`
- `Assets/Scripts/Godgame/Tests/VillagerJobSchedulerTests.cs`
- `Assets/Scripts/Godgame/Tests/VillagerAutonomousActionTests.cs`

### Files to Modify
- `Assets/Scripts/Godgame/Villagers/VillagerJobSystem.cs` (add interrupt checks, consume assignments)
- `Assets/Scripts/Godgame/Villagers/VillagerUtilityScheduler.cs` (wire needs, wire autonomous actions)
- `Assets/Scripts/Godgame/Resources/StorehouseApi.cs` (add reservation methods)
- `Assets/Scripts/Godgame/Tests/Conservation_VillagerGatherDeliver_Playmode.cs` (test improved selection)

---

## PureDOTS Integration Points

1. **Utility Calculations**: Use PureDOTS `VillagerUtilityScheduler.CalculateNeedUtility` and `CalculateJobUtility`
2. **Needs Components**: Align with PureDOTS `VillagerNeedsHot` structure
3. **Behavior Profiles**: Future integration with PureDOTS `VillagerBehaviorConfig` and `AggregateBehaviorProfile`

---

## Related Documentation

- `Docs/DemoReadiness_GapAnalysis.md` - Gap analysis
- `Docs/Concepts/Villagers/Villager_Behavioral_Personality.md` - Personality system design
- `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md` - Autonomous village behaviors
- `Docs/Guides/Godgame_PureDOTS_Entity_Mapping.md` - Entity mapping guide
