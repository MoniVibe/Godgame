# Individual Progression System

**Status:** Design
**Category:** System - Character Development
**Scope:** Skill Trees, Passive Abilities, Player Guidance
**Created:** 2025-11-03
**Last Updated:** 2025-11-03

---

## Purpose

Enable player to **passively preordain** individual villagers' progression paths through skill and passive trees, allowing customization of guild heroes and notable individuals without direct micromanagement, creating legendary characters through guided specialization.

**Primary Goal:** Allow player to shape heroes/champions/guild members into legendary specialists via skill tree guidance.
**Secondary Goals:**
- Maintain sandbox autonomy (player guides, AI executes)
- Create player attachment to specific individuals
- Enable diverse build variety (tank heroes, glass cannon mages, stealthy assassins)
- Integrate with guild selection systems (skills affect scores)
- Generate emergent legends through specialization

---

## Core Concept: Passive Preordaining

### What is Preordaining?

**Definition:**
Player selects a **desired progression path** via UI, but the villager **autonomously** follows it based on their actions, opportunities, and circumstances. Player sets the **goal**, AI determines the **execution**.

**Key Distinction:**
- **NOT Direct Control:** Player doesn't click buttons to level up skills
- **IS Guidance:** Player says "I want this hero to specialize in demon slaying"
- **AI Autonomy:** Hero gains demon slaying skills by actually killing demons
- **Player Influence:** Hero prioritizes demon-hunting missions over other tasks

**Philosophy:**
"Show me what you want them to become, I'll make sure they pursue it."

---

## UI: Individual Progression Screen

### Access

**Who can be customized:**
- **Champions** (player-chosen champions always)
- **Heroes** (famous villagers with high glory/renown)
- **Guild Masters** (leaders of guilds)
- **Notable Individuals** (high-achieving specialists)
- **Any villager** (if player clicks their portrait/selects them)

**How to access:**
1. Click villager portrait or entity in world
2. Open "Progression" tab in inspector panel
3. View skill trees, passive trees, current stats, achievements

### UI Layout

```
┌──────────────────────────────────────────────────────────────────┐
│  [Portrait]  Korgath the Warrior                        Level 12 │
│              Guild: Lightbringers (Heroes' Guild)                │
│              Glory: 450  |  Fame: 320  |  Renown: 280           │
├──────────────────────────────────────────────────────────────────┤
│  STATS                                                           │
│  Physique: 85  Finesse: 45  Agility: 60                        │
│  Intelligence: 30  Wisdom: 40  Will: 55                         │
│  Charisma: 35  Faith: 60  Belief: 70                           │
├──────────────────────────────────────────────────────────────────┤
│  SKILL TREES               │  PASSIVE TREES                      │
│  ┌──────────────────┐     │  ┌──────────────────┐              │
│  │ Combat (Active)  │     │  │ Demon Slayer     │              │
│  │ ├─ Sword Mastery │     │  │ ├─ Demon Hunter  │  [Preordain]│
│  │ │  ├─ Power Strike│     │  │ │  ├─ +50% dmg vs│              │
│  │ │  └─ Cleave      │     │  │ │  └─ Detect demon│             │
│  │ └─ Shield Bash   │     │  │ └─ Demon Slayer  │  [Active]   │
│  │                   │     │  │    └─ 2x XP vs demon│           │
│  └──────────────────┘     │  └──────────────────┘              │
│                            │                                     │
│  [Preordain: Combat Focus] │  [Preordain: Demon Specialization] │
└──────────────────────────────────────────────────────────────────┘
```

**UI Features:**
- **Skill Trees:** Active abilities (sword strikes, spells, stealth moves)
- **Passive Trees:** Permanent bonuses (damage modifiers, resistances, XP boosts)
- **Preordain Buttons:** Click to set desired path ("I want him to become a demon slayer")
- **Progress Bars:** Show XP toward next unlock in preordained path
- **Unlock Conditions:** Display requirements (e.g., "Kill 50 demons to unlock Demon Slayer")

---

## Skill Trees (Active Abilities)

### Combat Skill Tree (Warriors, Heroes)

```
Combat Tree:
├─ Weapon Mastery
│  ├─ Sword Mastery
│  │  ├─ Power Strike (heavy damage, high stamina cost)
│  │  ├─ Cleave (AOE slash)
│  │  └─ Riposte (counter-attack on block)
│  ├─ Axe Mastery
│  │  ├─ Berserker Strike (damage + self-damage)
│  │  └─ Whirlwind (spinning AOE)
│  └─ Spear Mastery
│     ├─ Thrust (long-range poke)
│     └─ Sweep (trip enemies)
│
├─ Shield Techniques
│  ├─ Shield Bash (stun)
│  ├─ Shield Wall (block projectiles)
│  └─ Fortress Stance (immobile, high defense)
│
└─ Battlefield Tactics
   ├─ Rally Cry (buff allies)
   ├─ Intimidate (debuff enemies)
   └─ Last Stand (survive lethal damage once)
```

**Unlocks:** Gained through **combat experience** (kill count, battles won, damage dealt)

---

### Arcane Skill Tree (Mages)

```
Arcane Tree:
├─ Elemental Magic
│  ├─ Fire
│  │  ├─ Fireball (ranged damage)
│  │  ├─ Flame Wall (area denial)
│  │  └─ Meteor (ultimate, massive AOE)
│  ├─ Ice
│  │  ├─ Ice Shard (slow + damage)
│  │  ├─ Freeze (immobilize)
│  │  └─ Blizzard (AOE slow)
│  └─ Lightning
│     ├─ Chain Lightning (multi-target)
│     ├─ Static Field (AOE damage over time)
│     └─ Thunderstorm (ultimate)
│
├─ Summoning
│  ├─ Summon Imp (weak minion)
│  ├─ Summon Elemental (strong minion)
│  └─ Summon Demon (risky, powerful)
│
└─ Enchanting
   ├─ Enchant Weapon (buff ally damage)
   ├─ Protective Ward (shield)
   └─ Dispel Magic (remove buffs/debuffs)
```

**Unlocks:** Gained through **arcane research** (spells cast, magical duels won, artifacts studied)

---

### Rogue Skill Tree (Thieves, Assassins)

```
Rogue Tree:
├─ Stealth
│  ├─ Invisibility (temporary)
│  ├─ Silent Movement (no detection)
│  └─ Shadow Step (teleport short distance)
│
├─ Assassination
│  ├─ Backstab (critical from behind)
│  ├─ Poison Blade (damage over time)
│  └─ Execution (instant kill low-health targets)
│
└─ Infiltration
   ├─ Lockpicking (open chests/doors faster)
   ├─ Pickpocket (steal items from targets)
   └─ Disguise (appear as friendly)
```

**Unlocks:** Gained through **stealth actions** (successful thefts, assassinations, infiltrations)

---

### Divine Skill Tree (Holy Orders, Mystics)

```
Divine Tree:
├─ Healing
│  ├─ Cure Wounds (single target heal)
│  ├─ Regeneration (heal over time)
│  └─ Resurrection (revive fallen ally)
│
├─ Smiting
│  ├─ Holy Strike (bonus damage vs undead/demons)
│  ├─ Exorcism (banish demons)
│  └─ Divine Wrath (AOE holy damage)
│
└─ Blessings
   ├─ Bless Ally (stat buff)
   ├─ Consecrate Ground (area buff)
   └─ Divine Shield (absorb damage)
```

**Unlocks:** Gained through **divine deeds** (prayers answered, miracles witnessed, holy sites consecrated)

---

## Passive Trees (Permanent Bonuses)

### Specialization Passives

**Demon Slayer Tree:**
```
Demon Slayer:
├─ Demon Hunter I
│  ├─ +25% damage vs demons
│  └─ Detect demons within 20m radius
├─ Demon Hunter II
│  ├─ +50% damage vs demons
│  └─ Demons prioritize you (taunt)
└─ Demon Slayer (Ultimate)
   ├─ +100% damage vs demons
   ├─ 2x XP from demon kills
   └─ Gain legendary title "Demon Slayer"
```

**Unlocks:** Kill demons (10 → 50 → 200 kills)

---

**Boss Hunter Tree:**
```
Boss Hunter:
├─ Giant Killer I
│  ├─ +15% damage vs large enemies
│  └─ Ignore 25% of boss armor
├─ Giant Killer II
│  ├─ +30% damage vs large enemies
│  └─ Knock down large enemies (critical hits)
└─ Boss Hunter (Ultimate)
   ├─ +50% damage vs world bosses
   ├─ 3x loot from boss kills
   └─ Gain legendary title "World Boss Slayer"
```

**Unlocks:** Kill world bosses (1 → 3 → 10 kills)

---

**Archmage Tree:**
```
Archmage:
├─ Spell Adept I
│  ├─ -10% spell mana cost
│  └─ +10% spell damage
├─ Spell Adept II
│  ├─ -20% spell mana cost
│  └─ +25% spell damage
└─ Archmage (Ultimate)
   ├─ -30% spell mana cost
   ├─ +50% spell damage
   ├─ Unlock Forbidden Magic tree
   └─ Gain legendary title "Archmage"
```

**Unlocks:** Cast spells (100 → 500 → 2000 casts)

---

**Master Thief Tree:**
```
Master Thief:
├─ Cat Burglar I
│  ├─ +15% stealth effectiveness
│  └─ +10% movement speed while sneaking
├─ Cat Burglar II
│  ├─ +30% stealth effectiveness
│  └─ Pick locks 50% faster
└─ Master Thief (Ultimate)
   ├─ +50% stealth effectiveness
   ├─ 2x value from stolen goods
   └─ Gain legendary title "Master Thief"
```

**Unlocks:** Complete heists (10 → 50 → 200 successful thefts)

---

### Attribute Passives

**Warrior Physique Tree:**
```
├─ Strength Training I: +5 Physique
├─ Strength Training II: +10 Physique
└─ Titan's Might: +20 Physique, +25% max health
```

**Agile Reflexes Tree:**
```
├─ Agility Training I: +5 Agility
├─ Agility Training II: +10 Agility
└─ Lightning Speed: +20 Agility, +25% attack speed
```

**Genius Intellect Tree:**
```
├─ Mental Training I: +5 Intelligence
├─ Mental Training II: +10 Intelligence
└─ Mastermind: +20 Intelligence, +25% XP gain
```

---

## Preordaining System

### How Preordaining Works

**Step 1: Player Sets Goal**
- Player opens Korgath's progression screen
- Clicks "Preordain: Demon Slayer" passive tree
- Clicks "Preordain: Combat Focus" skill tree (Sword Mastery branch)

**Step 2: System Records Intent**
```csharp
PreordainedPath component attached to Korgath:
- DesiredPassiveTree: DemonSlayer
- DesiredSkillBranch: SwordMastery
- Priority: High (player explicitly set this)
```

**Step 3: AI Adjusts Behavior**
- When Korgath chooses missions: **Prioritize demon-hunting quests** (+50% weight)
- When Korgath allocates XP: **Channel XP into Sword Mastery** (unlock Power Strike next)
- When Korgath loots gear: **Prefer swords** over axes/spears
- When Korgath trains: **Practice swordsmanship** at training grounds

**Step 4: Progression Happens Autonomously**
- Korgath kills demons → Gains XP toward Demon Hunter I
- Korgath practices sword → Gains XP toward Power Strike unlock
- Player sees progress bars fill up in UI
- Unlocks happen automatically when conditions met

**Step 5: Player Sees Results**
- Korgath becomes legendary "Demon Slayer" over time
- Player didn't click level-up buttons, just guided the path
- Korgath's choices aligned with player vision

---

### Preordaining Constraints

**AI Still Makes Choices:**
- If no demons available, Korgath does other missions (doesn't sit idle)
- If survival threatened, Korgath abandons preordained path temporarily
- If better opportunities arise (world boss nearby), Korgath may deviate

**Alignment/Outlook Override:**
- Lawful Good Korgath won't become assassin (even if player preordains it)
- Chaotic Evil Korgath won't become holy paladin
- Preordaining respects villager's core identity

**Resource Limits:**
- XP is finite (gained through actions)
- Can't unlock entire tree at once
- Must choose between multiple preordained paths if XP scarce

---

## Legendary Individuals

### What Makes a Legend?

**Criteria:**
1. **Specialization Depth:** Reach ultimate tier in passive tree (Demon Slayer, Archmage, etc.)
2. **Fame Threshold:** Glory/Renown exceeds 500 (top 1% of population)
3. **Unique Title:** Gain legendary title (autogenerated or player-named)
4. **Achievement Record:** Multiple rare achievements (world bosses slain, apocalypses survived)

**Benefits of Legendary Status:**
- **Permanent Legacy:** Survives death (statues, memorials, books written about them)
- **Inspiration Aura:** Nearby villagers gain morale/productivity buffs
- **Recruitment Magnet:** Guilds aggressively recruit legends, offer leadership positions
- **Player Attachment:** UI highlights legends, notifications on major events
- **Narrative Weight:** Legends appear in telemetry, become part of world history

---

### Example: Crafting a Legend

**Player Goal:** Create a demon-slaying sword master

**Step-by-Step:**
1. **Select Villager:** Find chaotic warlike villager with high physique (or champion one)
2. **Preordain Path:**
   - Passive: Demon Slayer tree
   - Skill: Sword Mastery branch
3. **Assign to Guild:** Recruit into Heroes' Guild (demon-hunting missions available)
4. **Equip Gear:** Give legendary demon-slaying sword (if found via loot)
5. **Observe Progression:**
   - Villager hunts demons (kills 10 → unlocks Demon Hunter I)
   - Practices swordsmanship (unlocks Power Strike)
   - Kills more demons (kills 50 → unlocks Demon Hunter II)
   - Defeats demon world boss (kills 1 boss → unlocks Boss Hunter I)
   - Kills 200 demons total → **Unlocks Demon Slayer (Ultimate)**
6. **Legendary Status Achieved:**
   - Title: "Korgath the Demon Slayer"
   - Fame: 650 (legendary tier)
   - Guild: Guild Master of Lightbringers
   - Legacy: Statue erected in home village

**Player Satisfaction:** "I guided Korgath from rookie warrior to legendary demon slayer"

---

## DOTS 1.4 Implementation Architecture

### Skill Tree Data Structures

```csharp
namespace Godgame.Progression
{
    /// <summary>
    /// Skill tree definition (blob asset, shared across all villagers).
    /// </summary>
    public struct SkillTreeBlob
    {
        public FixedString64Bytes TreeName;  // "Combat", "Arcane", "Rogue"
        public BlobArray<SkillNodeBlob> Nodes;
    }

    public struct SkillNodeBlob
    {
        public ushort NodeId;
        public FixedString64Bytes SkillName;      // "Power Strike"
        public FixedString128Bytes Description;   // "Heavy attack dealing 2x damage"

        // Prerequisites
        public ushort ParentNodeId;               // Which node must be unlocked first
        public ushort RequiredLevel;              // Minimum character level
        public ushort RequiredXP;                 // XP cost to unlock

        // Effects (simplified - would reference effect system)
        public float DamageMultiplier;            // 2.0 for Power Strike
        public float StaminaCost;                 // 25 stamina
        public float Cooldown;                    // 5 seconds
    }

    /// <summary>
    /// Passive tree definition (blob asset).
    /// </summary>
    public struct PassiveTreeBlob
    {
        public FixedString64Bytes TreeName;       // "Demon Slayer"
        public BlobArray<PassiveNodeBlob> Nodes;
    }

    public struct PassiveNodeBlob
    {
        public ushort NodeId;
        public FixedString64Bytes PassiveName;    // "Demon Hunter I"
        public FixedString128Bytes Description;   // "+25% damage vs demons"

        // Prerequisites
        public ushort ParentNodeId;
        public ushort RequiredAchievement;        // "Kill 10 demons"

        // Effects
        public float DamageVsDemons;              // +0.25
        public float DetectionRadius;             // 20m
        public bool GrantsTitle;                  // True for ultimate tier
        public FixedString64Bytes TitleName;      // "Demon Slayer"
    }
}
```

---

### Character Progression Component

```csharp
namespace Godgame.Progression
{
    /// <summary>
    /// Tracks individual's progression state.
    /// Attached to villagers who have progression (heroes, champions, guild members).
    /// </summary>
    public struct CharacterProgression : IComponentData
    {
        public ushort Level;                      // Overall character level (1-100)
        public uint TotalXP;                      // Cumulative experience
        public uint AvailableSkillPoints;         // Unspent points for skill unlocks
        public uint AvailablePassivePoints;       // Unspent points for passive unlocks

        // Legendary status
        public bool IsLegendary;
        public FixedString64Bytes LegendaryTitle; // "Korgath the Demon Slayer"
        public ushort Fame;                       // 0-1000 (legendary at 500+)
        public ushort Renown;
    }

    /// <summary>
    /// Unlocked skills buffer.
    /// </summary>
    public struct UnlockedSkill : IBufferElementData
    {
        public BlobAssetReference<SkillTreeBlob> TreeRef;
        public ushort NodeId;                     // Which skill in tree
        public byte CurrentRank;                  // 0-3 (skill level)
        public uint UnlockedTick;
    }

    /// <summary>
    /// Unlocked passives buffer.
    /// </summary>
    public struct UnlockedPassive : IBufferElementData
    {
        public BlobAssetReference<PassiveTreeBlob> TreeRef;
        public ushort NodeId;
        public uint UnlockedTick;
    }

    /// <summary>
    /// Player-set preordained path (guidance, not control).
    /// </summary>
    public struct PreordainedPath : IComponentData
    {
        public enum Priority : byte
        {
            None,
            Low,
            Medium,
            High        // Player explicitly set this
        }

        // Desired trees
        public BlobAssetReference<SkillTreeBlob> DesiredSkillTree;
        public ushort DesiredSkillBranchRoot;     // Which branch (Sword Mastery vs Axe)

        public BlobAssetReference<PassiveTreeBlob> DesiredPassiveTree;
        public ushort DesiredPassiveBranchRoot;   // Which specialization

        public Priority PathPriority;
        public uint PreordainedTick;              // When player set this
    }
}
```

---

### XP Allocation System

```csharp
namespace Godgame.Progression.Systems
{
    /// <summary>
    /// Automatically allocates XP based on preordained path when villagers gain experience.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct XPAllocationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // Query villagers with progression and preordained paths
            foreach (var (progression, preordained, skills, passives, entity) in SystemAPI
                .Query<RefRW<CharacterProgression>, RefRO<PreordainedPath>,
                      DynamicBuffer<UnlockedSkill>, DynamicBuffer<UnlockedPassive>>()
                .WithEntityAccess())
            {
                // Check if villager has unspent skill/passive points
                if (progression.ValueRO.AvailableSkillPoints == 0 &&
                    progression.ValueRO.AvailablePassivePoints == 0)
                    continue;

                // Allocate skill points toward preordained skill branch
                if (progression.ValueRO.AvailableSkillPoints > 0 &&
                    preordained.ValueRO.DesiredSkillTree.IsCreated)
                {
                    AllocateSkillPoints(ref state, ref ecb, entity, progression,
                        preordained.ValueRO, skills);
                }

                // Allocate passive points toward preordained passive branch
                if (progression.ValueRO.AvailablePassivePoints > 0 &&
                    preordained.ValueRO.DesiredPassiveTree.IsCreated)
                {
                    AllocatePassivePoints(ref state, ref ecb, entity, progression,
                        preordained.ValueRO, passives);
                }
            }
        }

        private void AllocateSkillPoints(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity entity, RefRW<CharacterProgression> progression,
            PreordainedPath preordained, DynamicBuffer<UnlockedSkill> skills)
        {
            // Find next unlockable skill in desired branch
            ref var tree = ref preordained.DesiredSkillTree.Value;

            for (int i = 0; i < tree.Nodes.Length; i++)
            {
                ref var node = ref tree.Nodes[i];

                // Check if this node is in desired branch (descends from branch root)
                if (!IsInBranch(ref tree, node.NodeId, preordained.DesiredSkillBranchRoot))
                    continue;

                // Check if already unlocked
                if (HasSkill(skills, node.NodeId))
                    continue;

                // Check if prerequisites met
                if (!MeetsPrerequisites(skills, node, progression.ValueRO.Level))
                    continue;

                // UNLOCK SKILL
                skills.Add(new UnlockedSkill
                {
                    TreeRef = preordained.DesiredSkillTree,
                    NodeId = node.NodeId,
                    CurrentRank = 1,
                    UnlockedTick = (uint)state.WorldUnmanaged.Time.ElapsedTime
                });

                progression.ValueRW.AvailableSkillPoints -= 1;

                // Broadcast notification: "Korgath learned Power Strike!"
                break;  // Unlock one skill per update
            }
        }

        private void AllocatePassivePoints(ref SystemState state, ref EntityCommandBuffer ecb,
            Entity entity, RefRW<CharacterProgression> progression,
            PreordainedPath preordained, DynamicBuffer<UnlockedPassive> passives)
        {
            // Similar to AllocateSkillPoints but for passive tree
            // (Implementation would follow same pattern)
        }

        private bool IsInBranch(ref SkillTreeBlob tree, ushort nodeId, ushort branchRoot)
        {
            // Walk up parent chain to see if this node descends from branch root
            ushort current = nodeId;
            while (current != 0)
            {
                if (current == branchRoot)
                    return true;

                // Find parent
                for (int i = 0; i < tree.Nodes.Length; i++)
                {
                    if (tree.Nodes[i].NodeId == current)
                    {
                        current = tree.Nodes[i].ParentNodeId;
                        break;
                    }
                }
            }
            return false;
        }

        private bool HasSkill(DynamicBuffer<UnlockedSkill> skills, ushort nodeId)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i].NodeId == nodeId)
                    return true;
            }
            return false;
        }

        private bool MeetsPrerequisites(DynamicBuffer<UnlockedSkill> skills,
            in SkillNodeBlob node, ushort level)
        {
            // Check level requirement
            if (level < node.RequiredLevel)
                return false;

            // Check parent unlocked
            if (node.ParentNodeId != 0 && !HasSkill(skills, node.ParentNodeId))
                return false;

            return true;
        }
    }
}
```

---

### AI Mission Priority Adjustment

```csharp
namespace Godgame.Progression.Systems
{
    /// <summary>
    /// Adjusts AI mission selection to prioritize missions aligned with preordained path.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PreordainedPathInfluenceSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Query villagers with preordained paths selecting missions
            foreach (var (preordained, aiState, guildMember, entity) in SystemAPI
                .Query<RefRO<PreordainedPath>, RefRW<VillagerAIState>, RefRO<GuildMember>>()
                .WithEntityAccess())
            {
                // If preordained to become Demon Slayer, prioritize demon-hunting missions
                if (IsPreordainedForPassive(preordained.ValueRO, "DemonSlayer"))
                {
                    // Modify AI goal weights
                    // (Implementation would adjust mission scoring)
                    // E.g., Demon-hunting missions get +50% priority weight
                }

                // If preordained for Sword Mastery, prefer sword-based combat
                if (IsPreordainedForSkill(preordained.ValueRO, "SwordMastery"))
                {
                    // Modify gear preferences
                    // (Implementation would adjust equipment selection)
                }
            }
        }

        private bool IsPreordainedForPassive(PreordainedPath path, FixedString64Bytes passiveName)
        {
            // Check if desired passive tree matches
            if (!path.DesiredPassiveTree.IsCreated)
                return false;

            return path.DesiredPassiveTree.Value.TreeName.Equals(passiveName);
        }

        private bool IsPreordainedForSkill(PreordainedPath path, FixedString64Bytes skillBranch)
        {
            // Check if desired skill branch matches
            // (Implementation would verify branch root name)
            return false;  // Placeholder
        }
    }
}
```

---

## UI Integration (Conceptual)

### Read-Only Entity Inspection

**UI Binding:**
- UI reads `CharacterProgression`, `UnlockedSkill`, `UnlockedPassive` components
- Displays current stats, skill trees, passive trees
- Shows progress bars toward next unlock
- **Does NOT directly modify components** (uses command pattern)

**Player Actions:**
1. Click "Preordain: Demon Slayer" → Sends command to game systems
2. Game system creates/updates `PreordainedPath` component
3. AI systems read `PreordainedPath` and adjust behavior
4. XP allocation system respects preordained path
5. UI displays updated progression state next frame

---

## Integration with Guild System

**Synergy:**
- Passives affect guild candidate scores:
  - Demon Slayer passive → +50 bonus to Heroes' Guild score
  - Archmage passive → +75 bonus to Mages' Guild score
  - Master Thief passive → +60 bonus to Rogues' Guild score

- Guild membership accelerates specialization:
  - Heroes' Guild provides demon-hunting missions (more demon kills → faster Demon Slayer unlocks)
  - Mages' Guild provides arcane research (more spell casts → faster Archmage unlocks)

- Legendary individuals become guild masters:
  - High progression + legendary title → automatic guild master candidacy
  - Player-crafted legends lead guilds toward player's strategic goals

---

## Open Questions

1. **Respec System:** Can player reset preordained path mid-progression? Cost?
2. **XP Sources:** What actions grant XP? Combat only or also crafting, research, diplomacy?
3. **Skill Synergies:** Do skills from different trees combo? (e.g., Fireball + Sword Strike = Flaming Blade)
4. **Passive Caps:** Maximum number of passives active simultaneously?
5. **Legendary Persistence:** Do legendary titles pass to descendants/students?
6. **Cross-Character Teaching:** Can legends train other villagers (XP boost for students)?
7. **Player Naming:** Can player rename legendary individuals ("Korgath the Demon Slayer" → custom name)?

---

## Related Documentation

- Guild selection mechanics: `Docs/Concepts/Villagers/Guild_System.md`
- Extended stats and achievements: `Docs/Concepts/Villagers/Guild_System.md#core-components`
- Champion system: `Docs/Concepts/Core/Sandbox_Autonomous_Villages.md#champions`
- Alignment framework: `Docs/Concepts/Meta/Generalized_Alignment_Framework.md`

---

**For Implementers:** Skill/passive trees stored as blob assets for performance; progression components per-villager; UI reads components via queries.
**For Designers:** Balance XP rates and unlock thresholds to create satisfying progression pacing (legendary status achievable but requires 20-50 hours of focused play per character).
**For Players:** "Guide your heroes to legendary status - I'll make sure they follow your vision while maintaining their autonomy."
