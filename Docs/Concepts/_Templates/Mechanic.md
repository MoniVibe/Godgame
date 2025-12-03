# [Mechanic Name]

## Overview

*What is this mechanic and why does it exist?*

**Purpose:** [1 sentence goal]  
**Player Impact:** [What does this enable/prevent?]  
**System Role:** [How does this fit into larger systems?]

---

## How It Works

### Inputs
*What goes into this mechanic?*
- Player actions
- World state
- Other system outputs

### Process
*Step-by-step what happens*
1. [First step]
2. [Second step]
3. [Outcome]

### Outputs
*What results from this mechanic?*
- State changes
- Events triggered
- Resources generated/consumed

---

## Rules

1. **[Rule Name]:** [Description]
   - Condition: [When?]
   - Effect: [What happens?]

### Edge Cases
- [Scenario] → [How we handle it]

### Priority Order
1. [Highest priority]
2. [Second priority]
3. [Fallback]

---

## Parameters

| Parameter | Default | Range | Impact |
|-----------|---------|-------|--------|
| [Name] | [Value] | [Min-Max] | [What this affects] |

---

## Player Interaction

### Player Decisions
*What choices does this mechanic give players?*

### Feedback to Player
- **Visual:** [Animation, VFX, UI]
- **Audio:** [Sound effects]
- **UI:** [Numbers, bars, icons]

---

## Balance Considerations

- **Balance Goals:** [What this mechanic should achieve]
- **Tuning Knobs:** [What can be adjusted]
- **Known Issues:** [Current balance problems or exploits]

---

## Integration Points

*How this mechanic interacts with others*

| Other Mechanic | Relationship | Notes |
|----------------|--------------|-------|
| [Mechanic A] | Synergy | [How they work together] |
| [Mechanic B] | Conflict | [How we resolve] |
| [Mechanic C] | Independent | [No interaction] |

---

## Shareability Assessment

**PureDOTS Candidate:** [Yes/No/Partial]

**Rationale:** [Why this can/cannot be shared between games]

**Shared Components:** [List components that could be PureDOTS]
- Component A: [description]
- Component B: [description]

**Game-Specific Adapters:** [What games would need to implement]
- Godgame: [specific adapters needed]
- Space4x: [specific adapters needed]

---

## Technical Implementation

*Component schemas, system design*

```csharp
// Example component structure
public struct MyComponent : IComponentData
{
    // Fields
}
```

**System Design:**
- [System name]: [What it does]
- [Integration points]: [How it connects]

---

## Performance Budget

- **Max Entities:** [count]
- **Update Frequency:** [per frame/tick]
- **Burst Compatibility:** [Yes/No - constraints]
- **Memory Budget:** [per entity/component]

---

## Examples

**Given:** [Starting conditions]  
**When:** [Action/trigger]  
**Then:** [Expected result]

---

## Related Mechanics

- [Mechanic Name]: `Docs/Concepts/Category/Mechanic.md`
- [System Name]: `Docs/Concepts/Category/System.md`

---

**Implementation:** See `Docs/TruthSources_Inventory.md` for component/system design
