# Godgame PureDOTS Entity Mapping Guide

**Status:** Implementation Guide  
**Category:** Godgame Integration  
**Scope:** How Godgame maps PureDOTS entities to presentation  
**Created:** 2025-01-XX  
**Last Updated:** 2025-01-XX

---

## Purpose

This document explains how Godgame maps PureDOTS agnostic entities to Godgame-specific presentation and gameplay layers.

---

## Entity Types in Godgame

### Individual Entities: Villagers

**PureDOTS Foundation:**
- Uses: `VillagerAlignment`, `VillagerBehavior`, `VillagerInitiativeState`
- Systems: All PureDOTS villager systems process these entities

**Godgame Layer:**
- **Presentation:** 3D GameObject with mesh, animation, visual effects
- **Components:** `GodgameVillagerPresentation`, `GodgameVillagerVisuals`
- **Interaction:** Direct player interaction (click, select, command)
- **Visualization:** 
  - Alignment affects clothing color, building style
  - Behavior affects animation (bold = confident walk, craven = cautious)
  - Initiative affects action frequency (visible in UI tooltips)

**Example:**
```csharp
// PureDOTS entity (agnostic)
Entity villagerEntity;
AddComponent(villagerEntity, new VillagerAlignment { MoralAxis = 70 });
AddComponent(villagerEntity, new VillagerBehavior { BoldScore = 60 });

// Godgame presentation layer
AddComponent(villagerEntity, new GodgameVillagerPresentation 
{ 
    GameObject = villagerPrefab,
    ClothingColor = ComputeColorFromAlignment(alignment),
    AnimationStyle = ComputeAnimationFromBehavior(behavior)
});
```

### Aggregate Entities: Villages

**PureDOTS Foundation:**
- Uses: `VillagerAlignment`, `VillagerBehavior`, `VillagerInitiativeState` (same as individuals!)
- Systems: Same PureDOTS systems process aggregates
- **Computation:** Alignment/behavior computed from member averages

**Godgame Layer:**
- **Presentation:** Collection of buildings, villagers, visual effects
- **Components:** `GodgameVillagePresentation`, `GodgameVillageVisuals`
- **Interaction:** Indirect player interaction (select village, command aggregate)
- **Visualization:**
  - Alignment affects village culture (building materials, decoration style)
  - Behavior affects village atmosphere (bold villages = expansionist visuals)
  - Initiative affects village expansion rate (visible in village UI)

**Example:**
```csharp
// PureDOTS aggregate entity (same components as individual!)
Entity villageEntity;
AddComponent(villageEntity, new VillagerAlignment { MoralAxis = 55 }); // Averaged from members
AddComponent(villageEntity, new VillagerBehavior { BoldScore = 40 });  // Averaged from members

// Godgame presentation layer
AddComponent(villageEntity, new GodgameVillagePresentation
{
    BuildingStyle = ComputeStyleFromAlignment(alignment),
    ExpansionVisuals = ComputeExpansionFromBehavior(behavior)
});
```

### Aggregate Entities: Bands

**PureDOTS Foundation:**
- Uses: `Band` component + `VillagerAlignment`, `VillagerBehavior` (same alignment/behavior!)
- Systems: Same PureDOTS systems process bands

**Godgame Layer:**
- **Presentation:** Formation visualization, group movement, combat formations
- **Components:** `GodgameBandPresentation`, `GodgameBandFormation`
- **Interaction:** Select band, command formation, assign targets
- **Visualization:**
  - Alignment affects band banner/colors
  - Behavior affects formation style (bold = aggressive formation, craven = defensive)
  - Initiative affects band action frequency

### Aggregate Entities: Guilds

**PureDOTS Foundation:**
- Uses: `Guild` component + `VillagerAlignment`, `VillagerBehavior` (same alignment/behavior!)
- Systems: Same PureDOTS systems process guilds

**Godgame Layer:**
- **Presentation:** Guild hall buildings, member roster UI, mission board
- **Components:** `GodgameGuildPresentation`, `GodgameGuildUI`
- **Interaction:** Select guild hall, view members, assign missions
- **Visualization:**
  - Alignment affects guild hall architecture
  - Behavior affects guild mission types (bold = dangerous missions)
  - Initiative affects guild expansion rate

---

## Presentation Component Pattern

### Pattern: PureDOTS Foundation + Godgame Presentation

**All entities follow this pattern:**

1. **PureDOTS Foundation (Agnostic):**
   ```csharp
   AddComponent(entity, new VillagerAlignment { ... });
   AddComponent(entity, new VillagerBehavior { ... });
   AddComponent(entity, new VillagerInitiativeState { ... });
   ```

2. **Godgame Presentation (Game-Specific):**
   ```csharp
   // For individuals
   AddComponent(entity, new GodgameVillagerPresentation { ... });
   
   // For aggregates
   AddComponent(entity, new GodgameVillagePresentation { ... });
   AddComponent(entity, new GodgameBandPresentation { ... });
   AddComponent(entity, new GodgameGuildPresentation { ... });
   ```

3. **Godgame Systems (Game-Specific):**
   - Read PureDOTS components
   - Update presentation components
   - Handle player interaction
   - Render visuals

---

## Implementation Checklist

### For Individual Villagers

- [x] PureDOTS components: `VillagerAlignment`, `VillagerBehavior`, `VillagerInitiativeState`
- [ ] Godgame presentation: `GodgameVillagerPresentation` component
- [ ] Godgame visuals: Mesh, animation, clothing system
- [ ] Godgame interaction: Click selection, command system
- [ ] Godgame UI: Tooltip showing alignment/behavior/initiative

### For Aggregate Villages

- [x] PureDOTS components: `VillagerAlignment`, `VillagerBehavior`, `VillagerInitiativeState` (aggregated)
- [ ] Godgame presentation: `GodgameVillagePresentation` component
- [ ] Godgame visuals: Building style, culture visualization
- [ ] Godgame interaction: Village selection, aggregate commands
- [ ] Godgame UI: Village panel showing aggregate stats

### For Aggregate Bands

- [x] PureDOTS components: `Band` + `VillagerAlignment`, `VillagerBehavior` (aggregated)
- [ ] Godgame presentation: `GodgameBandPresentation` component
- [ ] Godgame visuals: Formation visualization, group movement
- [ ] Godgame interaction: Band selection, formation commands
- [ ] Godgame UI: Band panel showing members and stats

### For Aggregate Guilds

- [x] PureDOTS components: `Guild` + `VillagerAlignment`, `VillagerBehavior` (aggregated)
- [ ] Godgame presentation: `GodgameGuildPresentation` component
- [ ] Godgame visuals: Guild hall architecture, member visualization
- [ ] Godgame interaction: Guild selection, mission assignment
- [ ] Godgame UI: Guild panel showing members, missions, stats

---

## Next Steps

1. **Create Godgame presentation components** for each entity type
2. **Implement visual mapping systems** (alignment → visuals, behavior → animations)
3. **Wire up interaction systems** (selection, commands)
4. **Build UI panels** showing PureDOTS data in Godgame context

---

**Related Documentation:**
- PureDOTS Entity Agnostic Design: `PureDOTS/Docs/Concepts/Entity_Agnostic_Design.md`
- Godgame Registry Bridge: `Godgame/Docs/TODO/Registry_Gap_Audit.md`

---

**Last Updated:** 2025-01-XX  
**Status:** Implementation Guide - In Progress

