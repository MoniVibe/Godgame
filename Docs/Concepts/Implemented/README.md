# Implemented Concepts

This folder contains concept documents for features that have been **fully implemented** in the codebase. These docs are preserved for reference but should not be re-implemented by agents.

## Contents

### Core/
- **Prayer_Power.md** - Implemented as:
  - `Assets/Scripts/Godgame/Prayer/PrayerComponents.cs` (PrayerPowerPool singleton, PrayerGenerator per-villager)
  - `Assets/Scripts/Godgame/Prayer/PrayerGenerationSystem.cs` (Accumulation, consumption)
  - `Assets/Scripts/Godgame/Prayer/PrayerAuthoring.cs` (Bootstrap authoring)
  - `Assets/Scripts/Godgame/Tests/CoreSystemsTests.cs` (Unit tests)

- **Focus_And_Status_Effects_System.md** - Implemented as:
  - `Assets/Scripts/Godgame/Effects/StatusEffectComponents.cs` (50+ effect types, buff/debuff buffer)
  - `Assets/Scripts/Godgame/Effects/StatusEffectSystem.cs` (Tick, expire, modifier computation)
  - `Assets/Scripts/Godgame/Tests/CoreSystemsTests.cs` (Unit tests)

### Buildings/
- **Building_Durability_System.md** - Implemented as:
  - `Assets/Scripts/Godgame/Buildings/BuildingDurabilityComponents.cs` (Thresholds, penalties, quality)
  - `Assets/Scripts/Godgame/Buildings/BuildingDurabilitySystem.cs` (Decay, fire damage)
  - `Assets/Scripts/Godgame/Buildings/BuildingDurabilityAuthoring.cs` (Editor authoring)
  - `Assets/Scripts/Godgame/Tests/CoreSystemsTests.cs` (Unit tests)

### Resources/
- **Aggregate_Piles.md** - Implemented as (v1.0 MVP):
  - `Assets/Scripts/Godgame/Resources/AggregatePileComponents.cs` (Visual sizes, states)
  - `Assets/Scripts/Godgame/Resources/AggregatePileSpawnSystem.cs` (Spawn, grow)
  - `Assets/Scripts/Godgame/Resources/AggregatePileAuthoring.cs` (Editor authoring)
  - `Assets/Scripts/Godgame/Tests/CoreSystemsTests.cs` (Unit tests)
  - Note: Merge/split (v2.0) not yet implemented

### Villagers/
- **Villager_Behavioral_Personality.md** - Implemented as:
  - `Assets/Scripts/Godgame/Villagers/VillagerBehaviorComponents.cs` (Bold/Vengeful traits, InitiativeModifier)
  - `Assets/Scripts/Godgame/Villagers/VillagerBehaviorAuthoring.cs` (Editor authoring)
  - `Assets/Scripts/Godgame/Villagers/VillagerGrudgeComponents.cs` (Grudge buffer, offense types)
  - `Assets/Scripts/Godgame/Villagers/VillagerGrudgeDecaySystem.cs` (Decay based on personality)
  - `Assets/Scripts/Godgame/Tests/VillagerBehaviorTests.cs` (Unit tests)

- **Band_Formation_And_Dynamics.md** - Implemented as:
  - `Assets/Scripts/Godgame/Bands/BandComponents.cs` expanded with:
    - `BandPurpose` enum (Military/Logistics/Civilian/Work)
    - `BandGoalType` enum and `BandGoal` component
    - `SharedExperienceType` enum and `SharedExperience` buffer
    - `BandMembership`, `BandEvolutionState`, `BandDesperation`
    - `BandFormationCandidate`, `BandFormationProspect`, `BandJoinRequest`
    - `BandCombatBonus`

- **Entity_Relations_And_Interactions.md** - Implemented as (v1.0 MVP):
  - `Assets/Scripts/Godgame/Relations/EntityRelationComponents.cs` (Relation buffer, tiers, meeting context)
  - `Assets/Scripts/Godgame/Relations/RelationCalculator.cs` (Alignment/personality formulas)
  - `Assets/Scripts/Godgame/Relations/EntityMeetingSystem.cs` (First-meet detection)
  - `Assets/Scripts/Godgame/Tests/CoreSystemsTests.cs` (Unit tests)
  - Note: Full v2.0/v3.0 features (aggregates, hierarchies, god relations) not yet implemented

### Economy/
- **Storehouse_API.md** - Implemented as:
  - `Assets/Scripts/Godgame/Economy/StorehouseAPI.cs` (Add/Remove/Space/Reserve helper methods)

### AI/ (Behavior Systems)
- **Initiative System** - Implemented as:
  - `Assets/Scripts/Godgame/AI/InitiativeComponents.cs` (VillagerInitiative, InitiativeBand, InitiativeConfig)
  - `Assets/Scripts/Godgame/AI/InitiativeSystem.cs` (Updates initiative from personality/mood/grudges)
  - `Assets/Scripts/Godgame/Tests/AIBehaviorSystemsTests.cs` (Unit tests)

- **Mood Band System** - Implemented as:
  - `Assets/Scripts/Godgame/AI/MoodBandComponents.cs` (MoodBand enum, VillagerMood, MoodModifier/MoodMemory buffers)
  - `Assets/Scripts/Godgame/AI/MoodBandSystem.cs` (Classifies morale into bands, applies modifiers)
  - `Assets/Scripts/Godgame/Tests/AIBehaviorSystemsTests.cs` (Unit tests)

- **Patriotism System** - Implemented as:
  - `Assets/Scripts/Godgame/AI/PatriotismComponents.cs` (VillagerPatriotism, migration/desertion tags)
  - `Assets/Scripts/Godgame/AI/PatriotismSystem.cs` (Updates loyalty, triggers migration/desertion)
  - `Assets/Scripts/Godgame/Tests/AIBehaviorSystemsTests.cs` (Unit tests)

- **Daily Routine System** - Implemented as:
  - `Assets/Scripts/Godgame/AI/DailyRoutineComponents.cs` (DailyPhase, SleepArchetype, VillagerRoutine, DayTimeState)
  - `Assets/Scripts/Godgame/AI/DailyRoutineSystem.cs` (Phase transitions, sleep tracking)
  - `Assets/Scripts/Godgame/Tests/AIBehaviorSystemsTests.cs` (Unit tests)

### Progression/
- **Individual_Progression_System.md (MVP)** - Implemented as:
  - `Assets/Scripts/Godgame/Progression/ProgressionComponents.cs` (CharacterProgression, SkillXP buffer, UnlockedSkill buffer, PreordainedPath)
  - `Assets/Scripts/Godgame/Progression/XPAllocationSystem.cs` (Awards XP with path multipliers, level-up handling)
  - `Assets/Scripts/Godgame/Progression/SkillUnlockSystem.cs` (Mastery upgrades, path progress tracking)
  - `Assets/Scripts/Godgame/Tests/AIBehaviorSystemsTests.cs` (Unit tests)
  - Note: Full skill tree effects, abilities, and UI integration not yet implemented

## For Agents

**Do NOT re-implement these features.** If you need to extend or modify them, look at the existing implementation in the codebase first.

If a concept doc is still in the parent `Docs/Concepts/` folder, it may be:
- Not yet implemented
- Partially implemented (check the TODO files)
- A pure design concept without implementation commitment

## Moving More Docs Here

When you implement a concept, move its doc to the appropriate subfolder here and update this README.

