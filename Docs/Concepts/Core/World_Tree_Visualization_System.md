# World Tree Visualization System

**Status:** Concept
**Priority:** Medium (Cosmetic Feature with High Player Engagement Potential)
**Cross-Project:** Godgame (primary), Space4X (adapted as "Nexus Crystal" or station structure)
**Last Updated:** 2025-12-03

---

## Overview

**Purpose:** A massive, world-spanning tree (Yggdrasil-inspired) that grows and morphs based on the aggregate state of all entities, villages, and civilizations on the planet. Serves as a visual monument to the world's collective journey.

**Player Impact:** The ultimate "virtual bonsai" or "civilization Tamagotchi" - players can watch their world's moral, technological, and cultural evolution reflected in a single majestic organism. Provides instant visual feedback on world state without opening menus.

**System Role:** Living historical record that documents every war, crisis, cultural rise and fall, and individual life through interactive tree structure. Functions as playable museum of civilization's journey; potential future integration as gameplay mechanic (rituals, pilgrimage, world-tree-based magic, environmental effects).

### Design Philosophy

**The World Tree is the Planet's Soul Made Visible:**
- **Single Source of Truth:** One tree per world, aggregates ALL entities (individuals, villages, guilds, armies, cultures)
- **Never Shrinks:** Tree only grows or maintains size; can wither/decay but doesn't reduce in physical scale
- **Can Die:** If world population reaches zero, tree dies and leaves permanent husk monument
- **Can Resurrect:** Slowly regrows from husk as world repopulates (phoenix metaphor)
- **Morphs, Never Replaces:** Tree appearance shifts gradually based on civilization trajectory, no sudden transformations

**Living Visualization:**
The tree is a data visualization that tells stories:
- A lush, crystalline tree with glowing branches → spiritual, good-aligned, magic-focused civilization
- A dark, cybernetic tree with scarred bark → materialistic, evil-aligned, war-torn civilization
- A withered, stunted tree with sparse branches → declining population, low happiness
- A massive, dense tree with varied textures → large, diverse, xenophilic population

---

## Core Mechanics

### Hierarchical Aggregation

**World → Races → Cultures → Dynasties → Families → Individuals**

The tree aggregates statistics from bottom-up:

1. **Individuals:** Each **living** entity (villager, guild member, soldier) contributes their stats
2. **Aggregate Entities:** Villages, guilds, companies, factions, dynasties, families compute averages from **living** members
3. **Cultures:** Cultures aggregate all entities within their cultural group
4. **Races:** Races aggregate all cultures and entities within that race
5. **World:** Tree aggregates all races on the planet

**Entity Tracking:**
- **Primary Tracking:** Only **living entities** are actively tracked and contribute to tree state
- **Dead Entities:** When an individual dies, their leaf turns yellow and falls off over time (1-10 years)
- **Historical Records:** Dead entities remain in historical timeline data, but do not affect current tree appearance or stats
- **Memory System:** Historical events (wars, achievements, etc.) are preserved in timeline, but visual representation (leaves) only shows living entities

**Aggregation Formula:**
```
WorldTreeState = WeightedAverage(AllEntities) + SpecialConditions
```

### Tree Growth and Death

**Growth Mechanics:**

**Size (Scale):**
- **Based On:** Total individual count (population)
- **Formula:** `TreeScale = log10(Population + 1) × ScalingFactor`
- **Example:**
  - 10 individuals → Small sapling (scale ~1.0)
  - 100 individuals → Young tree (scale ~2.0)
  - 1,000 individuals → Mature tree (scale ~3.0)
  - 10,000 individuals → Ancient tree (scale ~4.0)
  - 100,000 individuals → World-spanning colossus (scale ~5.0)

**Death Mechanics:**

**Tree Dies When:**
- **Population Reaches Zero:** All individuals dead, no aggregate entities remain
- **Extinction Event:** Natural disaster, plague, total war annihilation

**Death Process:**
1. Tree begins withering (leaves fall, branches dry, bark cracks)
2. Final entity dies → tree freezes in death state
3. Tree remains as permanent **husk monument** (ghostly, skeletal, petrified)
4. Husk serves as historical marker (players can see "this world once lived")

**Resurrection Mechanics:**

**Tree Resurrects When:**
- **First New Entity Appears:** Migration, respawn, player intervention, natural reproduction

**Resurrection Process:**
1. **Phase 1 (First 10 entities):** Tiny green shoots sprout from husk base (0-5% restoration)
2. **Phase 2 (10-100 entities):** Saplings grow around husk, begin intertwining (5-25% restoration)
3. **Phase 3 (100-1000 entities):** New growth wraps around old husk, absorbing it (25-75% restoration)
4. **Phase 4 (1000+ entities):** Husk fully integrated, tree reborn with scar tissue patterns (75-100% restoration)

**Husk Memory:** The resurrected tree retains "scar patterns" from the old husk, showing the world's history of death and rebirth.

---

## Visual Morphing System

### Alignment and Morality

**Good/Evil Axis (-100 to +100):**

**Pure Good (+75 to +100):**
- **Appearance:** Lush, verdant, vibrant colors (bright greens, golds, whites)
- **Leaves:** Dense, healthy, golden glow
- **Bark:** Smooth, silver-white, radiant
- **Roots:** Visible, intertwined peacefully with landscape
- **Aura:** Soft golden particles floating around tree

**Neutral Good (+25 to +75):**
- **Appearance:** Healthy green, natural forest aesthetic
- **Leaves:** Green, abundant, seasonal variation
- **Bark:** Brown, textured, healthy
- **Roots:** Strong, grounded

**Neutral (-25 to +25):**
- **Appearance:** Realistic tree, mixed textures
- **Leaves:** Green with some brown/yellow (realistic mix)
- **Bark:** Natural wood grain
- **Roots:** Balanced growth

**Neutral Evil (-75 to -25):**
- **Appearance:** Dark greens, shadowy, ominous
- **Leaves:** Dark purple-green, wilted edges
- **Bark:** Blackened, rough, thorny
- **Roots:** Gnarled, spread aggressively

**Pure Evil (-100 to -75):**
- **Appearance:** Dark, lifeless, twisted nightmare tree
- **Leaves:** Dead black leaves, or none (skeletal branches)
- **Bark:** Charred black, cracked, oozing sap (blood-like)
- **Roots:** Exposed, writhing, strangling landscape
- **Aura:** Dark smoke, ash particles, oppressive atmosphere

**Corruption Variant (Independent Variable):**
- **Corruption Level (0-100):** Based on crime, betrayal, decay
- **Visual:** Diseased patches, fungal growth, rot, pustules, twisted branches
- **Example:** Good-aligned but highly corrupt → Beautiful tree with diseased sections (fallen paradise)

### Ethics Axis (Materialist/Spiritual)

**Materialist (-100 to 0):**
- **Appearance:** Cybernetic, mechanical integration
- **Bark:** Metallic panels, circuit patterns, riveted plates
- **Branches:** Metal struts, cables, hydraulic pistons
- **Leaves:** Solar panels, holographic projections, LED grids
- **Roots:** Underground cables, piping, industrial foundation
- **Sounds:** Mechanical hums, electrical buzzing

**Spiritual (0 to +100):**
- **Appearance:** Crystalline, ethereal, glowing
- **Bark:** Translucent crystal, glowing runes, flowing energy
- **Branches:** Prismatic light refraction, rainbow auras
- **Leaves:** Floating energy wisps, glowing mandalas, ethereal petals
- **Roots:** Energy ley lines, visible magical currents
- **Sounds:** Harmonic chimes, wind chimes, ethereal whispers

**Mixed (Balanced):**
- **Bio-Mechanical Fusion:** Crystal circuits, living metal, organic technology
- **Example:** Cyberpunk tree with bioluminescent tech (Ghost in the Shell aesthetic)

### Warfare and Scarring

**Warlike Stat (0-100):**

**Peaceful (0-25):**
- **Appearance:** Smooth bark, unblemished
- **Branches:** Gentle curves, no damage

**Skirmishing (25-50):**
- **Appearance:** Minor battle scars, healed wounds
- **Bark:** Small burn marks, scratches
- **Branches:** Some broken limbs (regrown)

**Warring (50-75):**
- **Appearance:** Heavy scarring, weapon marks
- **Bark:** Deep gashes, axe cuts, sword slashes
- **Branches:** Splintered, broken, sharp edges
- **Roots:** Exposed from battle damage (torn earth)

**Total War (75-100):**
- **Appearance:** Catastrophic damage, battlefield aesthetic
- **Bark:** Charred from fires, crater impacts, siege weapon damage
- **Branches:** Half destroyed, jagged stumps
- **Roots:** Torn apart, landscape devastated
- **Aura:** Smoke, blood stains, weapon fragments embedded in trunk

**Scar Permanence:** War scars never fully heal, creating historical record (old wars visible as healed-over wounds).

### Might/Magic Axis

**Pure Might (-100 to -50):**
- **Appearance:** Chunky, robust, fortress-like
- **Trunk:** Massive diameter, squat, thick
- **Bark:** Rock-hard, stone-like texture, iron-reinforced
- **Branches:** Short, thick, powerful (like muscular arms)
- **Leaves:** Tough, leathery, dark green
- **Roots:** Deep, anchor-like, immovable
- **Overall:** Looks like it could survive anything physical

**Balanced Might/Magic (-50 to +50):**
- **Appearance:** Traditional tree, balanced proportions
- **Mix:** Natural wood with subtle magical glows

**Pure Magic (+50 to +100):**
- **Appearance:** Spindly, delicate, arcane
- **Trunk:** Thin, tall, graceful
- **Bark:** Smooth, glowing with runes and sigils
- **Branches:** Long, thin, weaving in impossible patterns
- **Leaves:** Floating, untethered, glowing with magic
- **Roots:** Barely touching ground, levitating sections
- **Aura:** Neon glows, spell effects, floating orbs of light
- **Overall:** Defies physics, looks fragile but radiates power

### Behavioral Traits

**Vengeful/Forgiving Axis (-100 to +100):**

**Vengeful (-100 to -50):**
- **Branching:** Spacious, aggressive spread
- **Branches:** Reach outward like grasping hands
- **Growth Pattern:** Dominates skyline, blocks sunlight to competitors
- **Thorns:** Prominent, defensive, weapon-like

**Forgiving (+50 to +100):**
- **Branching:** Dense, compact, protective
- **Branches:** Interwoven, supporting each other
- **Growth Pattern:** Creates canopy shelter for other plants
- **Leaves:** Abundant, overlapping, nurturing

**Bold/Craven Axis (-100 to +100):**

**Bold (+50 to +100):**
- **Height:** Extremely tall, reaches for sky
- **Trunk:** Straight, unwavering, proud
- **Branches:** Outstretched, fearless exposure

**Craven (-100 to -50):**
- **Height:** Shorter, hunched, hiding
- **Trunk:** Bent, twisted, seeking cover
- **Branches:** Pulled inward, defensive posture

**Xenophobic/Xenophilic Axis (-100 to +100):**

**Xenophobic (-100 to -50):**
- **Appearance:** Homogeneous, uniform texture
- **Bark:** Single color, consistent pattern
- **Leaves:** All identical, monoculture
- **Roots:** Isolationist, don't intermingle with other plants

**Xenophilic (+50 to +100):**
- **Appearance:** Varied, diverse, eclectic
- **Bark:** Patchwork of textures and colors (mosaic)
- **Leaves:** Multiple species grafted together (cherry, oak, willow mix)
- **Roots:** Intertwined with other plants, symbiotic
- **Branches:** Hosting other plants, vines, flowers

### Happiness and Vitality

**Happiness Level (0-100):**

**Miserable (0-25):**
- **Vitality:** Withered, drooping
- **Leaves:** Dry, brown, falling
- **Bark:** Cracked, peeling
- **Aura:** Gloomy, oppressive fog

**Unhappy (25-50):**
- **Vitality:** Struggling, muted colors
- **Leaves:** Sparse, dull green
- **Bark:** Rough, worn

**Content (50-75):**
- **Vitality:** Healthy, standard growth
- **Leaves:** Green, full
- **Bark:** Healthy texture

**Joyful (75-100):**
- **Vitality:** Vibrant, flourishing
- **Leaves:** Brilliant colors, abundant
- **Bark:** Lustrous, radiant
- **Aura:** Cheerful, birds/butterflies, pleasant sounds
- **Flowers:** Blooming year-round, sweet fragrance

### Technology Tiers

**Tech Level Branches:**

Each major technological milestone grows a **special branch type**:

**Tribal (Tier 1):**
- **Branch Type:** Natural wooden branches
- **Markers:** Stone tools embedded in bark, primitive carvings

**Classical (Tier 2):**
- **Branch Type:** Branches with metalworking motifs
- **Markers:** Bronze/iron decorations, written scrolls hanging

**Arcane (Tier 3):**
- **Branch Type:** Glowing magical branches
- **Markers:** Floating spell circles, enchanted crystals

**Industrial (Tier 4):**
- **Branch Type:** Steam-powered mechanical branches
- **Markers:** Gears, smokestacks, factory aesthetics

**Atomic (Tier 5):**
- **Branch Type:** Radioactive glowing branches
- **Markers:** Nuclear symbols, reactor cores, hazard signs

**Information (Tier 6):**
- **Branch Type:** Holographic data streams
- **Markers:** Screens, data flows, digital interfaces

**Transcendent (Tier 7):**
- **Branch Type:** Reality-bending branches (defy physics)
- **Markers:** Dimensional rifts, time distortions, godlike power

**Branch Growth Pattern:**
- New tech tier → New branch grows from main trunk
- Branch size proportional to civilization's mastery of that tech
- Multiple tech paths → Multiple specialized branches (diverse tech tree)
- Abandoned tech → Branch withers but remains (historical marker)

### Population and Scale

**Population Count:**

**Size Scale Formula:**
```csharp
float TreeScale = Mathf.Log10(totalPopulation + 1) * scalingFactor;
```

**Examples:**
- **1 entity:** Tiny seedling (barely visible)
- **10 entities:** Small sapling (1-2 meters)
- **100 entities:** Young tree (5-10 meters)
- **1,000 entities:** Mature tree (20-50 meters)
- **10,000 entities:** Ancient tree (100-200 meters)
- **100,000 entities:** Colossal world-tree (500+ meters, visible from orbit)

**Death and Decay:**
- **High Death Rate:** Leaves fall, branches dry, decay accelerates
- **Population Decline:** Tree stops growing, begins withering
- **Extinction:** Tree dies completely, becomes husk monument

**Atmospheric Extension:**
- **Space Colonies:** If civilization founds colonies outside the planet (space stations, other planets, moons), the tree **visually extends beyond the planet's atmosphere**
- **Visual Growth:** Tree branches reach upward into space, connecting to orbital structures
- **Scale:** Tree can grow to visible-from-orbit size (500+ meters) and extend into low orbit
- **Space Branch Representation:** Off-planet colonies appear as branches extending into space, with visual connection to orbital structures
- **Example:** A civilization with 3 space stations and 2 moon colonies would have 5 "space branches" extending upward from the main tree, reaching beyond the atmosphere layer

---

## Aggregation System

### Data Collection

**Individual Entity Stats → World Tree:**

```csharp
public struct WorldTreeState : IComponentData
{
    // Alignment and Morality
    public float AverageGoodEvil;        // -100 to +100
    public float AverageCorruption;      // 0 to 100

    // Ethics
    public float AverageMaterialSpiritual; // -100 (material) to +100 (spiritual)

    // Warfare
    public float WarlikeLevel;           // 0 to 100 (based on combat participation)
    public float TotalScars;             // Accumulated war damage (never resets)

    // Might/Magic
    public float AverageMightMagicAxis;  // -100 (might) to +100 (magic)

    // Behavioral Traits
    public float AverageVengefulForgiving; // -100 to +100
    public float AverageBoldCraven;        // -100 to +100
    public float AverageXenophobia;        // -100 (xenophobic) to +100 (xenophilic)

    // Vitality
    public float AverageHappiness;       // 0 to 100
    public int TotalPopulation;          // Count of all living entities
    public float DeathRate;              // Deaths per time unit (accelerates decay)

    // Technology
    public FixedList128Bytes<TechBranch> TechBranches; // List of unlocked tech tiers

    // Tree State
    public float TreeScale;              // Logarithmic scale based on population
    public bool IsDead;                  // True if population reached zero
    public float ResurrectionProgress;   // 0-1, progress of regrowth from husk
}

public struct TechBranch
{
    public TechTier Tier;               // Tribal, Classical, Arcane, etc.
    public float Mastery;               // 0-1, how developed this tech is
    public long UnlockTimestamp;        // When this tech was first achieved
}
```

**Aggregation Job (Burst-Compiled):**

```csharp
[BurstCompile]
public struct AggregateWorldTreeStateJob : IJob
{
    [ReadOnly] public NativeArray<IndividualStats> LivingEntities; // Only living entities
    public NativeReference<WorldTreeState> TreeState;

    public void Execute()
    {
        if (LivingEntities.Length == 0)
        {
            // Population extinct, mark tree as dead
            TreeState.Value = new WorldTreeState { IsDead = true };
            return;
        }

        // Aggregate all living individual stats
        float sumGoodEvil = 0;
        float sumCorruption = 0;
        float sumMaterialSpiritual = 0;
        float sumMightMagic = 0;
        float sumVengeful = 0;
        float sumBold = 0;
        float sumXenophobia = 0;
        float sumHappiness = 0;

        for (int i = 0; i < LivingEntities.Length; i++)
        {
            var entity = LivingEntities[i];
            // Only process living entities (dead entities filtered out before this job)
            if (entity.IsAlive)
            {
                sumGoodEvil += entity.GoodEvilAxis;
                sumCorruption += entity.CorruptionLevel;
                sumMaterialSpiritual += entity.MaterialSpiritualAxis;
                sumMightMagic += entity.MightMagicAxis;
                sumVengeful += entity.VengefulForgivingAxis;
                sumBold += entity.BoldCravenAxis;
                sumXenophobia += entity.XenophobicXenophilicAxis;
                sumHappiness += entity.CurrentHappiness;
            }
        }

        int livingCount = LivingEntities.Length;

        TreeState.Value = new WorldTreeState
        {
            AverageGoodEvil = sumGoodEvil / livingCount,
            AverageCorruption = sumCorruption / livingCount,
            AverageMaterialSpiritual = sumMaterialSpiritual / livingCount,
            AverageMightMagicAxis = sumMightMagic / livingCount,
            AverageVengefulForgiving = sumVengeful / livingCount,
            AverageBoldCraven = sumBold / livingCount,
            AverageXenophobia = sumXenophobia / livingCount,
            AverageHappiness = sumHappiness / livingCount,
            TotalPopulation = livingCount, // Only living population
            TreeScale = Mathf.Log10(livingCount + 1) * 1.5f,
            IsDead = false
        };
    }
}
```

**Note:** Dead entities are filtered out before aggregation. Their leaves turn yellow and fall off over time, but they do not contribute to tree state calculations.

### Update Frequency

**Real-Time vs. Batched:**

**Real-Time Updates (Immediate):**
- Population changes (birth/death) → Update tree scale instantly
- Major events (tech unlock, war declaration) → Trigger immediate morph

**Batched Updates (Performance):**
- Alignment/trait averages → Recalculate every 10 game seconds
- Visual morphing → Smooth interpolation over time (no sudden changes)

**Smooth Transitions:**
```csharp
// Tree appearance morphs gradually, never snaps
float currentGoodEvil = Mathf.Lerp(currentGoodEvil, targetGoodEvil, Time.deltaTime * morphSpeed);
```

**Morph Speed:** Configurable, default 0.1 (10% change per second) → Full morph takes ~10 seconds

---

## Technical Implementation

### Procedural Generation

**Shader-Based Morphing:**

**Material Properties:**
```csharp
Material treeMaterial = new Material(treeShader);

// Alignment
treeMaterial.SetFloat("_GoodEvilAxis", worldTree.AverageGoodEvil); // -100 to +100
treeMaterial.SetFloat("_CorruptionLevel", worldTree.AverageCorruption); // 0 to 100

// Ethics
treeMaterial.SetFloat("_MaterialSpiritualAxis", worldTree.AverageMaterialSpiritual);

// Warfare
treeMaterial.SetFloat("_WarScars", worldTree.TotalScars); // Permanent scar overlay
treeMaterial.SetFloat("_WarlikeLevel", worldTree.WarlikeLevel);

// Might/Magic
treeMaterial.SetFloat("_MightMagicAxis", worldTree.AverageMightMagicAxis);

// Traits
treeMaterial.SetFloat("_VengefulAxis", worldTree.AverageVengefulForgiving);
treeMaterial.SetFloat("_XenophilicAxis", worldTree.AverageXenophobia);

// Vitality
treeMaterial.SetFloat("_Happiness", worldTree.AverageHappiness);
treeMaterial.SetFloat("_Withering", worldTree.DeathRate); // Decay speed

// Tech
treeMaterial.SetTexture("_TechBranchMask", GenerateTechBranchMask(worldTree.TechBranches));
```

**Shader Graph Nodes:**
- **Good/Evil Blend:** Lerp between vibrant green (good) and charred black (evil)
- **Corruption Overlay:** Noise-based disease patches
- **Materialist/Spiritual:** Lerp between metallic/crystalline materials
- **Scarring:** Permanent scar texture overlay (additive)
- **Might/Magic:** Mesh deformation (chunky vs. spindly)
- **Tech Branches:** Special geometry spawned at tech unlock positions

### Mesh Generation

**Procedural Tree Mesh (SpeedTree/Houdini/Runtime):**

**Base Mesh:**
- **Trunk:** Cylinder with LOD levels (high poly close, low poly distant)
- **Branches:** Recursive branching algorithm (L-system or custom)
- **Leaves:** Instanced billboards or 3D leaf models

**Morph Targets:**
- **Might-aligned:** Thicker trunk, shorter branches (shape key blend)
- **Magic-aligned:** Thinner trunk, longer branches (shape key blend)
- **Vengeful:** Wider branch spread (branch angle parameter)
- **Forgiving:** Denser foliage (leaf instance count parameter)

**Runtime Deformation:**
```csharp
// Adjust trunk thickness based on might/magic axis
float trunkRadius = baseTrunkRadius * (1.0f + mightMagicAxis * 0.5f);

// Adjust branch spread based on vengeful axis
float branchAngle = baseBranchAngle * (1.0f + vengefulAxis * 0.3f);

// Adjust leaf density based on happiness
int leafCount = baseLeafCount * (happiness / 100.0f);
```

### Particle Systems and VFX

**Atmospheric Effects:**

**Good Alignment:**
- **Particles:** Golden sparkles, butterflies, fireflies
- **Sounds:** Birds chirping, wind chimes, gentle breeze

**Evil Alignment:**
- **Particles:** Black smoke, ash, crows circling
- **Sounds:** Ominous whispers, creaking wood, distant thunder

**Spiritual:**
- **Particles:** Floating runes, energy wisps, aurora lights
- **Sounds:** Harmonic chimes, ethereal hums

**Materialist:**
- **Particles:** Sparks, steam vents, holographic projections
- **Sounds:** Mechanical whirring, electrical buzzing

**Warlike:**
- **Particles:** Smoke from scars, embers from burn marks
- **Sounds:** Distant battle sounds, clashing metal echoes

### LOD (Level of Detail)

**Distance-Based LOD:**

**Close (0-100m):**
- **Mesh:** Full detail, high poly count
- **Leaves:** Individual 3D models
- **Particles:** All effects active
- **Scars:** Detailed texture maps

**Medium (100-500m):**
- **Mesh:** Reduced poly count
- **Leaves:** Billboard sprites
- **Particles:** Reduced particle count
- **Scars:** Simplified textures

**Far (500m+):**
- **Mesh:** Low poly silhouette
- **Leaves:** Textured billboard
- **Particles:** Disabled
- **Scars:** Color tint only

**Distant (1km+):**
- **Mesh:** Impostor billboard (2D sprite)
- **Leaves:** Baked into billboard
- **Particles:** None
- **Scars:** None

---

## Historical Record System

**The World Tree is a Living Museum:**

Every event, conflict, achievement, and tragedy is permanently recorded in the tree's structure. Players can zoom in, inspect individual branches, and read the complete history of their world through visual storytelling.

### Wars and Battle Scars

**Wars Leave Permanent Scars:**

**Scar Types:**

**1. Minor Skirmishes (Low Casualties):**
- **Visual:** Small scratches, healed-over wounds on bark
- **Color:** Faded brown/gray (old scars heal to tree color)
- **Location:** Outer branches (local conflicts)
- **Data:** Battle name, date, participants, casualty count

**2. Major Battles (High Casualties):**
- **Visual:** Deep gashes, burn marks, axe cuts
- **Color:** Dark red/brown (blood-stained)
- **Location:** Major branches or trunk (regional wars)
- **Width:** Proportional to casualty count (wider = more deaths)

**3. Total Wars (Catastrophic):**
- **Visual:** Massive crater impacts, splintered wood, charred sections
- **Color:** Bloody red fading to dark brown over centuries
- **Location:** Trunk itself (world-spanning conflicts)
- **Size:** Can wrap around entire trunk circumference

**Scar Aging:**
```csharp
// Scars fade over time but never disappear
float scarAge = currentTime - scar.timestamp;
float fadeAlpha = Mathf.Clamp01(1.0f - (scarAge / 1000years));
Color scarColor = Color.Lerp(darkBrown, bloodyRed, fadeAlpha);
```

**Blood-Stained Scars:**
- **Fresh (0-10 years):** Bright red, bloody, still "bleeding" (particle effects: dripping sap like blood)
- **Recent (10-50 years):** Dark red, dried blood, scabbed over
- **Old (50-200 years):** Brown, fully healed scar tissue
- **Ancient (200+ years):** Faint outline, barely visible, historical marker

**Interactive Scar Inspection:**
```csharp
// Click scar to see battle details
public struct BattleScar : IComponentData
{
    public FixedString128Bytes BattleName;     // "The Great War", "Siege of Ironhold"
    public long Timestamp;                     // When battle occurred
    public Entity Faction1;                    // Attacking faction
    public Entity Faction2;                    // Defending faction
    public int Casualties;                     // Total deaths
    public int CasualtiesFaction1;             // Attacker losses
    public int CasualtiesFaction2;             // Defender losses
    public Entity Victor;                      // Winning faction (or Entity.Null for stalemate)
    public float3 ScarPosition;                // Location on tree (trunk/branch)
    public float ScarSize;                     // Visual size (0-1)
    public FixedString512Bytes BattleSummary; // "The desperate defense of Ironhold lasted 3 years..."
}
```

**Battle Details UI:**
```
╔══════════════════════════════════════════════╗
║        THE GREAT WAR (Year 487)             ║
╠══════════════════════════════════════════════╣
║ Factions:                                    ║
║   • Northern Alliance (Attacker)             ║
║   • Southern Coalition (Defender)            ║
║                                              ║
║ Casualties:                                  ║
║   • Northern Alliance: 12,450 deaths         ║
║   • Southern Coalition: 8,732 deaths         ║
║   • Total: 21,182 deaths                     ║
║                                              ║
║ Outcome: Southern Coalition Victory          ║
║                                              ║
║ Summary:                                     ║
║   The desperate defense of the southern      ║
║   territories lasted 3 brutal years. The     ║
║   Northern Alliance's siege of Ironhold      ║
║   failed after starvation decimated their    ║
║   forces. Peace treaty signed Year 490.      ║
╚══════════════════════════════════════════════╝
```

### Crises Averted (Giant Bites)

**Near-Death Experiences Leave Wounds:**

**Crisis Types:**

**1. Plague/Disease:**
- **Visual:** Diseased chunk missing from trunk, like rotted wood carved out
- **Healing:** Slowly fills in with scar tissue (takes 50-100 years)
- **Color:** Sickly green/yellow while healing, brown when healed
- **Particle:** Green miasma slowly dissipating over decades

**2. Famine/Starvation:**
- **Visual:** Withered, hollow section of trunk (like tree was starved)
- **Healing:** Slowly fills in but remains thinner than original
- **Color:** Pale, malnourished appearance, gradually strengthening

**3. Natural Disaster:**
- **Visual:** Massive impact crater, splintered section
- **Healing:** New growth wraps around damage like bark healing wound
- **Color:** Raw wood exposed, gradually covered by new bark

**4. Civil War/Rebellion:**
- **Visual:** Internal splitting, cracks radiating from core
- **Healing:** Cracks seal but remain visible as fault lines
- **Color:** Dark fissures, internal scarring

**5. Genocide/Mass Execution:**
- **Visual:** Cleanly cut section, surgical removal
- **Healing:** Never fully heals, leaves permanent hollow
- **Color:** Black, charred edges (burned out)

**Healing Process:**

```csharp
public struct CrisisBite : IComponentData
{
    public FixedString128Bytes CrisisName;     // "The Red Plague", "Year of Famine"
    public CrisisType Type;                    // Plague, Famine, Disaster, etc.
    public long CrisisStart;                   // When crisis began
    public long CrisisEnd;                     // When crisis resolved
    public int PopulationLost;                 // Deaths during crisis
    public float BiteSize;                     // 0-1 (1 = entire trunk section)
    public float HealingProgress;              // 0-1 (0 = fresh wound, 1 = fully healed)
    public float3 BitePosition;                // Location on tree
    public FixedString512Bytes CrisisStory;   // Narrative of how crisis was averted
}

// Healing formula
float healingRate = 0.01f / year; // 1% healing per year
crisisBite.HealingProgress = Mathf.Min(1.0f, crisisBite.HealingProgress + (timeSinceEnd * healingRate));

// Visual: Scar tissue fills hollow
float biteDepth = crisisBite.BiteSize * (1.0f - crisisBite.HealingProgress);
```

**Crisis Details UI:**
```
╔══════════════════════════════════════════════╗
║      THE RED PLAGUE (Year 523-527)          ║
╠══════════════════════════════════════════════╣
║ Type: Plague/Disease                         ║
║ Duration: 4 years                            ║
║ Population Lost: 34,562 (42% of world)       ║
║                                              ║
║ Healing Progress: 67% (Healing since 527)    ║
║ Time to Full Heal: ~16 years remaining       ║
║                                              ║
║ Story:                                       ║
║   The plague arrived on foreign ships and    ║
║   spread rapidly through coastal villages.   ║
║   Healers from the Sylvan Guild discovered   ║
║   a cure using rare moonflowers. The world   ║
║   survived, but barely. Quarantine zones     ║
║   remain to this day.                        ║
╚══════════════════════════════════════════════╝
```

### Branch Structure as Civilization Hierarchy

**Each Branch Represents an Aggregate Entity:**

**Exact Hierarchical Branching Structure:**

```
Trunk (World)
├── Race Branch (e.g., Humans, Elves, Dwarves)
│   ├── Culture Branch (e.g., Northern Tribes, Southern Kingdoms)
│   │   ├── Dynasty Branch (e.g., Royal House of Iron)
│   │   │   ├── Family Branch (e.g., Smith Family)
│   │   │   │   └── Leaf (Individual - attached to highest loyalty aggregate)
│   │   ├── Village Branch (settlements)
│   │   │   ├── Guild Branch (Mages' Guild, Smiths' Guild)
│   │   │   ├── Band Branch (Mercenary Band, Thieves' Guild)
│   │   │   └── Company Branch (Trading Company)
│   │   ├── Faction Branch (political entities)
│   │   └── Empire Branch (large political entities)
│   └── Culture Branch (another culture within same race)
└── Race Branch (another race)
```

**Key Structural Rules:**

1. **Trunk = World:** The base of the tree represents the entire world
2. **Race Branches:** Major racial groups branch directly from the trunk
3. **Culture Branches:** Cultures branch from their parent race branch
4. **Dynasty Branches:** Dynasties branch from their parent culture branch
5. **Family Branches:** Families branch from their parent dynasty branch
6. **Individual Leaves:** Each individual person is a leaf attached to their **highest loyalty aggregate entity** (could be family, guild, village, faction, etc.)
7. **Village Branches:** Villages branch from their parent culture branch
8. **Guild/Band/Company Branches:** These organizations branch from their parent village branch
9. **Faction/Empire Branches:** Political entities branch from their parent culture branch

**Branch Properties:**

**1. Race Branches:**
- **Count:** Number of distinct races in the world
- **Thickness:** Total population of that race
- **Visual Theme:** Racial characteristics (e.g., Elven branches = graceful, Dwarven = stout)

**2. Culture Branches:**
- **Count:** Number of cultures within each race
- **Thickness:** Culture population size
- **Length:** Culture age/longevity
- **Color:** Culture alignment (good = green, evil = dark)
- **Visual Theme:** Culture's outlooks and alignments (see Visual Transformation below)

**3. Dynasty Branches:**
- **Sprout from culture branches**
- **Thickness:** Dynasty size (number of family members)
- **Length:** Dynasty age
- **Visual:** Noble families have golden highlights, common families are standard
- **Lowborn Dynasty Cleanup:** Many individuals are lowborn nobodies. Dynasty branches with only lowborn families (spindly branches, short if not many members) **fall off** when no family members are alive, clearing visual space on the culture branch
- **Fall-Off Criteria:** 
  - All family members dead (no living members)
  - Dynasty is lowborn (no notable achievements, no noble status)
  - Branch is spindly (thin, weak appearance)
  - Branch is short (few families or members historically)
- **Preservation:** Notable dynasties (noble, achieved greatness, large historical impact) remain as stumps even when extinct, but lowborn dynasties disappear completely

**4. Family Branches:**
- **Sprout from dynasty branches**
- **Small twigs with few leaves**
- **Genealogy visible:** Parent branch → child branches
- **Famous families:** Thicker twigs, golden highlights
- **Extinct families:** Withered twig stumps (if above bands/armies level)

**5. Village Branches:**
- **Sprout from culture branches**
- **Count:** Number of villages = number of village branches
- **Thickness:** Village population size
- **Length:** Village age/longevity
- **Color:** Village alignment (good = green, evil = dark)
- **Leaves:** Individual villagers attached to their highest loyalty aggregate

**6. Guild/Band/Company Branches:**
- **Sprout from village branches**
- **Thickness:** Organization membership size
- **Color:** Organization specialization
  - Mages' Guild: Purple/arcane glow
  - Warriors' Guild: Red/metallic
  - Merchants' Guild: Gold/coins
  - Thieves' Guild: Shadow/dark
- **Leaves:** Organization members (if highest loyalty)

**7. Faction/Empire Branches:**
- **Sprout from culture branches**
- **Thickness:** Faction/empire size (population under control)
- **Visual:** Political power reflected in branch prominence

**8. Individual Leaves:**
- **Attachment Point:** Leaf attaches to the aggregate entity the individual has **highest loyalty** to (could be family, guild, village, faction, etc.)
- **Color:** Individual's alignment and traits
- **State:** 
  - **Living:** Green, vibrant
  - **Recently Dead:** Yellow, falling off over time
  - **Dead:** Fallen, removed from tree

**Entity Tracking Rules:**

**Living Entities Only:**
- **Primary Tracking:** Only **living entities** are actively tracked and displayed
- **Dead Leaves:** When an individual dies, their leaf turns **yellow** and gradually **falls off** over time (not immediately removed)
- **Fall Duration:** Dead leaves take 1-10 years to fully fall off (configurable)
- **Memory:** Historical records of dead entities remain in the timeline system, but visual representation (leaves) disappears

**Stump Creation Threshold:**
- **Only Major Aggregates Leave Stumps:** Only aggregate entities **above the bands/armies level** leave permanent stumps when completely erased
- **Stump-Creating Aggregates:** Villages, Guilds, Companies, Factions, Empires, Cultures, Notable Dynasties, Races
- **No Stumps For:** 
  - Bands, Armies, and smaller temporary groups (these simply disappear without trace)
  - **Lowborn Dynasties:** Lowborn dynasty branches (spindly, short, no notable achievements) **fall off** when no family members are alive, clearing visual space (exception to stump rule)
- **Dynasty Exception:** Only **notable dynasties** (noble status, achievements, significant impact) create stumps when extinct. Lowborn dynasties fall off completely.

**Dynamic Branch Growth:**

```csharp
// New culture founded → new branch grows from race branch
void OnCultureCreated(Entity culture, Entity parentRace)
{
    var branch = new TreeBranch
    {
        BranchType = BranchType.Culture,
        ParentBranch = GetRaceBranch(parentRace),
        Entity = culture,
        CreationTimestamp = currentTime,
        Thickness = Mathf.Log10(culture.population + 1),
        Length = 0.0f, // Grows over time
        BaseVisualMaterial = GetBaseMaterial(culture), // Base appearance locked
        CurrentVisualMaterial = GetAlignmentMaterial(culture.alignment) // Extends from base
    };

    SpawnBranchGeometry(branch);
}

// New village founded → new branch grows from culture branch
void OnVillageCreated(Entity village, Entity parentCulture)
{
    var branch = new TreeBranch
    {
        BranchType = BranchType.Village,
        ParentBranch = GetCultureBranch(parentCulture),
        Entity = village,
        CreationTimestamp = currentTime,
        Thickness = Mathf.Log10(village.population + 1),
        Length = 0.0f,
        BaseVisualMaterial = GetBaseMaterial(village),
        CurrentVisualMaterial = GetAlignmentMaterial(village.alignment)
    };

    SpawnBranchGeometry(branch);
}

// Individual dies → leaf turns yellow and falls off
void OnIndividualDied(Entity individual)
{
    var leaf = GetLeaf(individual);
    leaf.State = LeafState.Dying; // Yellow
    leaf.FallStartTime = currentTime;
    leaf.FallDuration = Random.Range(1year, 10years);
    
    // Schedule removal after fall duration
    ScheduleLeafRemoval(leaf, currentTime + leaf.FallDuration);
}

// Aggregate destroyed → becomes stump (if above bands/armies level)
void OnAggregateDestroyed(Entity aggregate)
{
    if (IsAboveBandsArmiesLevel(aggregate))
    {
        CreateStump(aggregate); // See Erased Cultures section
    }
    else
    {
        // Simply remove, no stump
        RemoveBranch(aggregate);
    }
}

// Lowborn dynasty cleanup - falls off when no living members
void CheckDynastyCleanup(Entity dynasty)
{
    var dynastyBranch = GetDynastyBranch(dynasty);
    var allFamilies = GetFamiliesInDynasty(dynasty);
    
    // Check if any family members are alive
    bool hasLivingMembers = false;
    foreach (var family in allFamilies)
    {
        if (HasLivingMembers(family))
        {
            hasLivingMembers = true;
            break;
        }
    }
    
    // If no living members and dynasty is lowborn, fall off
    if (!hasLivingMembers && IsLowbornDynasty(dynasty))
    {
        // Check if dynasty is notable enough to preserve as stump
        if (IsNotableDynasty(dynasty))
        {
            // Notable dynasties become stumps (even if lowborn, if they achieved something)
            CreateStump(dynasty);
        }
        else
        {
            // Lowborn nobodies fall off completely, clearing visual space
            StartDynastyFallOff(dynastyBranch);
        }
    }
}

bool IsLowbornDynasty(Entity dynasty)
{
    // Check if dynasty has notable achievements, noble status, or significant impact
    var stats = GetDynastyStats(dynasty);
    
    // Lowborn criteria:
    // - No noble titles
    // - No significant achievements
    // - Small historical impact
    // - Spindly branch appearance (few families, few members)
    
    bool hasNobleStatus = stats.HasNobleTitles;
    bool hasAchievements = stats.NotableAchievements.Count > 0;
    bool hasSignificantImpact = stats.HistoricalImpact > 10; // Threshold
    bool isLargeDynasty = stats.TotalMembersEver > 50; // Threshold
    
    // Lowborn if none of the above
    return !hasNobleStatus && !hasAchievements && !hasSignificantImpact && !isLargeDynasty;
}

bool IsNotableDynasty(Entity dynasty)
{
    // Even lowborn dynasties can be notable if they achieved something
    var stats = GetDynastyStats(dynasty);
    return stats.NotableAchievements.Count > 0 || stats.HistoricalImpact > 20;
}

void StartDynastyFallOff(TreeBranch dynastyBranch)
{
    // Mark branch for fall-off animation
    dynastyBranch.State = BranchState.FallingOff;
    dynastyBranch.FallStartTime = currentTime;
    dynastyBranch.FallDuration = Random.Range(5years, 15years); // Takes 5-15 years to fall
    
    // Schedule removal after fall duration
    ScheduleBranchRemoval(dynastyBranch, currentTime + dynastyBranch.FallDuration);
    
    // Visual: Branch turns brown, withers, then falls off culture branch
    // This clears visual space for other dynasties
}
```

**Visual Transformation System:**

**Base Material Preservation:**
- **Base Stays Same:** When a branch is first created, its **base visual material** is locked (based on founding conditions, race, culture type, etc.)
- **Base Never Changes:** The original appearance remains as the foundation

**Alignment-Based Extensions:**
- **New Visual Material Extends:** As aggregate alignments, outlooks, and traits change over time, **new visual material extends** from the base
- **Gradual Morphing:** The branch doesn't replace its appearance; instead, it **grows new visual layers** on top of the base
- **Example:** A culture branch starts as natural wood (base). As it becomes more evil, dark thorny material extends from the base. As it becomes more spiritual, crystalline material extends. The base wood remains visible underneath.

**Transformation Mechanics:**

```csharp
public struct TreeBranch : IComponentData
{
    public Entity Entity;                      // Aggregate entity reference
    public BranchType Type;                    // Race, Culture, Dynasty, etc.
    public Entity ParentBranch;                // Parent branch entity
    
    // Visual Materials
    public Material BaseMaterial;              // Locked at creation, never changes
    public Material CurrentMaterial;           // Extends from base, morphs over time
    
    // Transformation State
    public float AlignmentShift;               // How much alignment has changed from base
    public float VisualExtension;              // 0-1, how far new material extends from base
    
    // Base Properties (locked)
    public Color BaseColor;
    public float BaseThickness;
    public float BaseLength;
    
    // Current Properties (morphing)
    public Color CurrentColor;                 // Blends base + alignment
    public float CurrentThickness;             // Can grow/shrink
    public float CurrentLength;                // Grows over time
}

// Update branch visual as alignment changes
void UpdateBranchVisual(ref TreeBranch branch, AggregateStats stats)
{
    // Calculate how much alignment has shifted from base
    float alignmentDelta = stats.CurrentAlignment - branch.BaseAlignment;
    branch.AlignmentShift = alignmentDelta;
    
    // Extend new visual material based on shift
    branch.VisualExtension = Mathf.Clamp01(Mathf.Abs(alignmentDelta) / 100.0f);
    
    // Blend base material with new alignment-based material
    branch.CurrentMaterial = BlendMaterials(
        branch.BaseMaterial,                    // Original appearance
        GetAlignmentMaterial(stats.CurrentAlignment), // New appearance
        branch.VisualExtension                   // How much new material shows
    );
    
    // Branch extends visually: base stays, new material grows outward
    float branchExtension = branch.BaseLength * (1.0f + branch.VisualExtension * 0.5f);
    branch.CurrentLength = branchExtension;
}
```

**Visual Example:**
- **Year 0:** Culture branch created as natural brown wood (base locked)
- **Year 50:** Culture becomes slightly evil → Dark material begins extending from base (10% extension)
- **Year 100:** Culture becomes very evil → Dark thorny material extends further (50% extension), base wood still visible at core
- **Year 150:** Culture shifts to spiritual → Crystalline material begins extending over dark material (new layer), creating layered appearance
- **Result:** Branch shows history of transformation - base wood at core, dark evil layer in middle, crystalline spiritual layer on outside

**Branch Inspection:**
```
╔══════════════════════════════════════════════╗
║         IRONHOLD VILLAGE (Branch)           ║
╠══════════════════════════════════════════════╣
║ Founded: Year 234 (315 years ago)            ║
║ Population: 1,247 villagers                  ║
║ Alignment: +65 Good, 15 Corruption           ║
║                                              ║
║ Sub-Branches:                                ║
║   • Ironworkers' Guild (324 members)         ║
║   • Merchants' Company (89 members)          ║
║   • Smith Dynasty (12 family members)        ║
║                                              ║
║ Notable Events:                              ║
║   • Year 345: Withstood goblin siege         ║
║   • Year 412: Founded Ironworkers' Guild     ║
║   • Year 489: Plague survivors (67% died)    ║
║                                              ║
║ Current State: Thriving, prosperous          ║
╚══════════════════════════════════════════════╝
```

### Interactive Zoom and Inspection

**Multi-Level Exploration:**

**Zoom Level 1: World View (Maximum Zoom Out):**
- See entire tree at once
- Major branches visible (continents/kingdoms)
- Trunk scars visible (world wars, global crises)
- God Player's root visible at base

**Zoom Level 2: Regional View:**
- Focus on single major branch (continent)
- Medium branches visible (regions/villages)
- Regional conflicts visible as scars
- Can see guild twigs

**Zoom Level 3: Village View:**
- Focus on single village branch
- All guild/company/family twigs visible
- Individual leaves (people) visible as tiny dots
- Can click leaves to see individual stories

**Zoom Level 4: Individual View (Maximum Zoom In):**
- Single leaf enlarged
- Individual's life story displayed
- Birth/death dates, achievements, family tree
- Cause of death, legacy
- **Player Interaction:** Click leaf to kill individual or bless with miracle (see Direct Player Interaction section)

**Camera Controls:**
```csharp
// Mouse wheel: Zoom in/out
// Click-drag: Rotate tree
// Double-click branch: Focus zoom to that branch
// Hover over element: Tooltip with basic info
// Click element: Full details panel
// Click leaf: Open interaction menu (Kill/Bless) - See Direct Player Interaction section
// Right-click leaf: Quick bless menu
```

**Timeline Scrubber:**

```
[═══════════════════════════════════════════════════]
Year 0               Year 250              Year 549 (Current)
         ^                      ^                ^
    Founding            Great War          Red Plague
```

- Drag slider to see tree at different points in history
- Tree morphs to show historical state (branches grow/wither, scars appear)
- "Playback" mode: Watch history unfold in timelapse

### Erased Cultures (Stumps)

**Destroyed Civilizations Leave Permanent Memorials:**

**Stump Creation Threshold:**

**Only Major Aggregates Leave Stumps:**
- **Stump-Creating Aggregates:** Only aggregate entities **above the bands/armies level** leave permanent stumps when completely erased
- **Creates Stumps:** Villages, Guilds, Companies, Factions, Empires, Cultures, **Notable Dynasties**, Races
- **No Stumps:** 
  - Bands, Armies, and smaller temporary groups simply disappear without trace (no memorial)
  - **Lowborn Dynasties:** Lowborn dynasty branches (spindly, short, no notable achievements) **fall off** when no family members are alive, clearing visual space (see Dynasty Branches section for details)

**Stump Creation Process:**

**When Major Aggregate Destroyed:**
1. All members die or disband
2. Branch stops growing immediately
3. Branch withers over 10 years (leaves fall, bark dries)
4. After 10 years: Branch "cut off" - becomes stump
5. Stump remains permanently as memorial

**When Minor Aggregate Destroyed (Bands/Armies):**
1. All members die or disband
2. Branch simply disappears (no stump)
3. No memorial created (too small to warrant permanent marker)

**Stump Visual:**
- **Appearance:** Severed branch, jagged cut, petrified wood
- **Color:** Gray, lifeless, stone-like
- **Texture:** Rough, weathered, ancient
- **Aura:** Ghostly mist, mournful atmosphere
- **Sound:** Wind through hollow wood, whispers of the dead

**Types of Erasure:**

**1. Conquest (Enemy Destroyed):**
- **Stump:** Clean cut, axe marks visible
- **Memorial Plaque:** "Ironhold Village, destroyed Year 487 by Northern Alliance. Population: 1,247. None survived."

**2. Plague/Disaster (Natural Death):**
- **Stump:** Withered, rotted, diseased
- **Memorial Plaque:** "Quarantine Zone 7, lost to Red Plague Year 526. Population: 892. Remembered."

**3. Voluntary Abandonment:**
- **Stump:** Smooth cut, peaceful
- **Memorial Plaque:** "Wanderer's Rest, abandoned Year 402. Population migrated north. Founded New Haven."

**4. Genocide/Purge:**
- **Stump:** Burned, charred, blackened
- **Memorial Plaque:** "Elven Sanctuary, purged Year 301 by Human Supremacists. Population: 3,421. Never forget."

**Stump Interaction:**

```csharp
public struct CultureStump : IComponentData
{
    public FixedString128Bytes CultureName;     // "Ironhold Village", "Mages' Guild"
    public Entity OriginalEntity;               // Historical reference
    public long FoundedTimestamp;               // When founded
    public long DestroyedTimestamp;             // When destroyed
    public int FinalPopulation;                 // Population at time of destruction
    public DestructionCause Cause;              // Conquest, Plague, Abandonment, Genocide
    public Entity Destroyer;                    // Who/what destroyed it (if applicable)
    public float3 StumpPosition;                // Location on tree
    public FixedString512Bytes Memorial;       // Epitaph/remembrance text
}

public enum DestructionCause
{
    Conquest,
    Plague,
    Famine,
    NaturalDisaster,
    Abandonment,
    Genocide,
    CivilCollapse
}
```

**Memorial UI:**
```
╔══════════════════════════════════════════════╗
║           IRONHOLD VILLAGE                  ║
║            (Destroyed)                       ║
╠══════════════════════════════════════════════╣
║ Founded: Year 234                            ║
║ Destroyed: Year 487                          ║
║ Lifespan: 253 years                          ║
║                                              ║
║ Final Population: 1,247                      ║
║ Cause of Death: Conquest                     ║
║ Destroyed By: Northern Alliance              ║
║                                              ║
║ Memorial:                                    ║
║   "Here stood Ironhold, the fortress of      ║
║   the southern plains. They held the line    ║
║   for three years against impossible odds.   ║
║   None survived, but their sacrifice saved   ║
║   a thousand others. Heroes, all."           ║
║                                              ║
║ [Light a Candle] [Leave Offering]            ║
╚══════════════════════════════════════════════╝
```

**Stump Rituals (Optional Gameplay):**
- **Light Candles:** Players can pay respect, small morale boost to nearby villages
- **Leave Offerings:** Sacrifice resources, ancestors bless descendants
- **Remembrance Day:** Annual event, stumps glow softly, ghosts appear

### Fading Glory (Achievements Fall Off)

**Old Achievements Eventually Forgotten:**

**Achievement Types:**

**1. Personal Achievements (Leaves):**
- **Visual:** Golden/glowing leaves attached to individual's leaf
- **Examples:**
  - "Slayer of the Dragon King"
  - "First to Master Arcane Language 200%"
  - "Founded Mages' Guild"
  - "Saved village from plague"

**2. Cultural Achievements (Branch Decorations):**
- **Visual:** Ornamental flowers, fruits, ribbons on branch
- **Examples:**
  - "Won the Great War"
  - "Built first library"
  - "Discovered alchemy"
  - "Established trade empire"

**Decay Mechanics:**

**Achievement Lifespan:**
- **Recent (0-50 years):** Vibrant, glowing, prominent
- **Fading (50-200 years):** Dulling colors, losing detail
- **Ancient (200-500 years):** Nearly invisible, historical footnote
- **Forgotten (500+ years):** Falls off tree, disappears

```csharp
public struct Achievement : IComponentData
{
    public FixedString128Bytes AchievementName;
    public FixedString512Bytes Description;
    public long EarnedTimestamp;               // When achieved
    public Entity Achiever;                    // Who achieved it
    public AchievementRarity Rarity;           // Common, Rare, Legendary
    public float FadeProgress;                 // 0-1 (1 = fully faded, disappears)
}

// Fade formula
float achievementAge = currentTime - achievement.EarnedTimestamp;
float fadeRate = 1.0f / (500years * achievement.Rarity); // Legendary fades slower
achievement.FadeProgress = Mathf.Min(1.0f, achievementAge * fadeRate);

// Visual alpha
float achievementAlpha = 1.0f - achievement.FadeProgress;

// Falls off when fully faded
if (achievement.FadeProgress >= 1.0f)
    DespawnAchievement(achievement);
```

**Rarity Modifiers:**
- **Common (1.0×):** Fades in 50 years (e.g., "Killed 100 goblins")
- **Uncommon (2.0×):** Fades in 100 years (e.g., "Founded village")
- **Rare (4.0×):** Fades in 200 years (e.g., "Won major battle")
- **Epic (8.0×):** Fades in 400 years (e.g., "Saved world from plague")
- **Legendary (Never):** Never fades (e.g., "Slew ancient evil god")

**Legendary Achievements:**
- Permanently inscribed on trunk (like carved names on tree)
- Glowing golden text
- Particle effects (eternal flame, divine light)
- Become part of tree's mythology

**Achievement Inspection:**
```
╔══════════════════════════════════════════════╗
║         ACHIEVEMENT (Fading)                ║
╠══════════════════════════════════════════════╣
║ Name: "Defender of Ironhold"                 ║
║ Earned: Year 345 (204 years ago)             ║
║ Achiever: Sir Aldric the Bold                ║
║                                              ║
║ Description:                                 ║
║   Led the defense of Ironhold against the    ║
║   goblin horde. Held the gates for 30 days   ║
║   until reinforcements arrived. Saved 1,247  ║
║   lives. Knighted by the king.               ║
║                                              ║
║ Rarity: Rare (Fades in 200 years)            ║
║ Fade Progress: 98% (2 years until forgotten) ║
║                                              ║
║ Status: Almost forgotten by time...          ║
╚══════════════════════════════════════════════╝
```

**Preserving Achievements:**
- **Write in Books:** Transfer to library, permanently preserved
- **Oral Tradition:** Bards sing tales, extends lifespan +100 years
- **Monuments:** Build statue, achievement never fades
- **Guild Records:** Guilds maintain archives, slow fading rate

### God Player's Root System

**The Player's Moral Journey Reflected Below:**

**Root Appearance Based on Player Alignment:**

**Pure Good/Lawful Good (+75 to +100 Alignment):**
- **Visual:** Clean, golden roots, radiant glow
- **Texture:** Smooth, polished, marble-like
- **Particle:** Golden light emanating from roots
- **Growth:** Roots spread wide, nurturing soil
- **Effect on Tree:** Tree grows faster, healthier
- **Sound:** Angelic chimes, divine music

**Good/Neutral Good (+25 to +75):**
- **Visual:** Healthy brown roots, natural
- **Texture:** Normal bark, wood grain
- **Particle:** Soft green glow (life energy)
- **Growth:** Balanced, stable root system
- **Effect on Tree:** Normal growth
- **Sound:** Gentle rustling, wind through leaves

**True Neutral (-25 to +25):**
- **Visual:** Gray roots, stone-like
- **Texture:** Mixed (organic + mineral)
- **Particle:** Faint shimmer, neither good nor evil
- **Growth:** Efficient, pragmatic spread
- **Effect on Tree:** Unaffected
- **Sound:** Silence, stillness

**Evil/Neutral Evil (-75 to -25):**
- **Visual:** Dark roots, black/brown, ominous
- **Texture:** Rough, thorny, twisted
- **Particle:** Dark smoke, ash
- **Growth:** Gnarled, aggressive, strangling
- **Effect on Tree:** Tree grows darker
- **Sound:** Ominous humming, dread

**Pure Evil/Chaotic Evil (-100 to -75):**
- **Visual:** Black, thorny roots, writhing
- **Texture:** Rotting, diseased, sharp spines
- **Particle:** Black miasma, corruption spreading
- **Growth:** Roots strangle landscape, kill plants
- **Effect on Tree:** Tree becomes nightmare
- **Sound:** Screaming, tortured souls, horror

**Corrupted (Independent Variable, 75-100 Corruption):**
- **Visual:** Sickly, spotty roots, diseased
- **Texture:** Pustules, fungal growth, rot
- **Particle:** Green miasma, plague clouds
- **Growth:** Erratic, diseased spread
- **Effect on Tree:** Tree sickens, withers
- **Sound:** Gurgling, sickness, decay

**Chaotic (Chaotic Alignment Axis):**
- **Visual:** Intertwined, sporadic, tangled
- **Texture:** Messy, unpredictable patterns
- **Particle:** Sparks, random bursts
- **Growth:** No pattern, chaotic sprawl
- **Effect on Tree:** Unpredictable branches
- **Sound:** Discordant notes, random sounds

**Root System Data:**

```csharp
public struct GodPlayerRoot : IComponentData
{
    public Entity PlayerEntity;                 // Reference to god player
    public float PlayerAlignment;               // -100 to +100
    public float PlayerCorruption;              // 0 to 100
    public float PlayerChaos;                   // 0 to 100 (lawful vs. chaotic)

    // Visual properties
    public Color RootColor;                     // Computed from alignment
    public float ThorninessLevel;               // 0-1 (evil = thorny)
    public float GlowIntensity;                 // Good = bright, evil = dark
    public float CorruptionSpots;               // Diseased patches
    public float ChaosTangles;                  // Random intertwining

    // Effect on tree
    public float TreeGrowthModifier;            // Good = +0.25, Evil = -0.25
    public float TreeHealthModifier;            // Corruption = -0.5
}

// Update root appearance based on player actions
void UpdatePlayerRoot(ref GodPlayerRoot root, PlayerActions actions)
{
    // Good deeds → roots lighten
    if (actions.HelpedVillagers)
        root.PlayerAlignment += 0.1f;

    // Evil deeds → roots darken, grow thorns
    if (actions.DestroyedVillage)
    {
        root.PlayerAlignment -= 5.0f;
        root.ThorninessLevel = Mathf.Min(1.0f, root.ThorninessLevel + 0.1f);
    }

    // Corruption spreads from corrupt actions
    if (actions.UsedBloodMagic)
        root.PlayerCorruption += 1.0f;

    // Chaotic actions → tangled roots
    if (actions.UnpredictableBehavior)
        root.ChaosTangles += 0.05f;
}
```

**Root Inspection (Player's Moral Report):**

```
╔══════════════════════════════════════════════╗
║           YOUR DIVINE ESSENCE               ║
╠══════════════════════════════════════════════╣
║ Alignment: -73 (Neutral Evil)                ║
║ Corruption: 45 (Moderately Corrupt)          ║
║ Chaos: 12 (Mostly Lawful)                    ║
║                                              ║
║ Root Appearance:                             ║
║   Dark, thorny roots strangling the soil.    ║
║   Diseased patches spread corruption.        ║
║   Tree grows darker under your influence.    ║
║                                              ║
║ Moral Summary:                               ║
║   You have ruled with cruelty and ambition.  ║
║   Villages fear your wrath. Wars flourish.   ║
║   Yet, you maintain order through tyranny.   ║
║                                              ║
║ Tree Health Impact: -35%                     ║
║ Warning: Tree withering from your evil!      ║
╚══════════════════════════════════════════════╝
```

**Root Effects on Tree:**

**Good Player (Benevolent God):**
- Tree grows **+25% faster**
- Happiness naturally **+10** across world
- Plagues less likely (-50% chance)
- Wars shorter (morale recovers faster)

**Evil Player (Tyrant God):**
- Tree grows **-25% slower** (struggles against evil)
- Happiness naturally **-10** across world
- Wars more brutal (+50% casualties)
- Rebellions more common

**Corrupt Player:**
- Tree sickens (disease spread +100%)
- Healers less effective (-30% healing)
- Crops fail more often (famine risk +50%)

**Chaotic Player:**
- Random events occur 3× more often
- Unpredictable buffs/debuffs
- Tree branches grow erratically

### Timeline System and Historical Playback

**Scrub Through World History:**

**Timeline UI:**

```
╔══════════════════════════════════════════════╗
║              WORLD TIMELINE                 ║
╠══════════════════════════════════════════════╣
║                                              ║
║ [◄◄] [◄] [▶] [▶▶] [■] [Speed: 1×]           ║
║                                              ║
║ ┌────────────────────────────────────────┐  ║
║ │░░░░░░░░░░░░░░░░░▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│  ║
║ └────────────────────────────────────────┘  ║
║ Year 0            Year 275          Year 549║
║                                    (Current) ║
║                                              ║
║ Major Events:                                ║
║   • Year 0: World Founded                    ║
║   • Year 234: Ironhold Village Founded       ║
║   • Year 345: Goblin Siege                   ║
║   • Year 487: The Great War                  ║
║   • Year 523: Red Plague Begins              ║
║   • Year 527: Red Plague Ends                ║
║   • Year 549: Current Day                    ║
╚══════════════════════════════════════════════╝
```

**Playback Controls:**
- **◄◄:** Rewind 100 years
- **◄:** Rewind 10 years
- **▶:** Forward 10 years
- **▶▶:** Forward 100 years
- **■:** Stop playback, return to current
- **Speed:** 1×, 2×, 5×, 10× (timelapse speed)

**Historical Playback Visualization:**

```csharp
// Render tree as it appeared in past
void RenderTreeAtTimestamp(long historicalTimestamp)
{
    // Hide branches founded after this date
    foreach (var branch in allBranches)
    {
        if (branch.FoundedTimestamp > historicalTimestamp)
            branch.SetActive(false);
    }

    // Show stumps that existed at this time but were later cut
    foreach (var stump in allStumps)
    {
        if (stump.FoundedTimestamp <= historicalTimestamp &&
            stump.DestroyedTimestamp > historicalTimestamp)
        {
            // Resurrect as living branch temporarily
            ConvertStumpToBranch(stump, historicalTimestamp);
        }
    }

    // Show scars that existed at this time
    foreach (var scar in allScars)
    {
        if (scar.Timestamp <= historicalTimestamp)
        {
            // Scar appears as it would have looked then (fresher)
            float ageAtTime = historicalTimestamp - scar.Timestamp;
            scar.SetVisualAge(ageAtTime);
        }
        else
        {
            scar.SetActive(false); // Hasn't happened yet
        }
    }

    // Update tree appearance to match historical world state
    WorldTreeState historicalState = GetWorldStateAtTime(historicalTimestamp);
    ApplyVisualMorphing(historicalState);
}
```

**Timelapse Mode (Automatic Playback):**

```
╔══════════════════════════════════════════════╗
║         TIMELAPSE: WORLD HISTORY            ║
╠══════════════════════════════════════════════╣
║ Now showing: The Rise and Fall of Ironhold   ║
║                                              ║
║ Year 234: ▓ Ironhold founded (branch grows) ║
║ Year 345: ▓ Goblin siege (scar appears)     ║
║ Year 412: ▓ Guild founded (twig sprouts)    ║
║ Year 487: ▓ Great War (massive scar)        ║
║ Year 489: ▓ Plague (crisis bite)            ║
║ Year 523: ▓ Final destruction (stump)       ║
║                                              ║
║ Watch tree evolve before your eyes...        ║
╚══════════════════════════════════════════════╝
```

**Event Markers on Timeline:**
- **Green markers:** Positive events (village founded, crisis averted)
- **Red markers:** Negative events (war, plague, destruction)
- **Yellow markers:** Neutral events (achievement earned, guild formed)
- **Click marker:** Jump to that moment in history

---

## Gameplay Integration (Future Potential)

### Current Status: Cosmetic

**Primary Function:**
- **Visual Feedback:** Players instantly see world state without menus
- **Emotional Connection:** Players care about tree health (like Tamagotchi)
- **Historical Record:** Tree scars and husks show world history

### Direct Player Interaction: Picking Leaves

**Players Can Interact with Individual Leaves:**

Players can directly interact with individual leaves on the World Tree to affect the individuals they represent. This provides direct god-like control over individual lives.

**1. Picking Leaves to Kill:**

**Mechanic:**
- **Action:** Player clicks/picks a leaf on the tree
- **Effect:** The individual represented by that leaf **dies immediately**
- **Visual:** Leaf turns yellow instantly, then falls off over 1-10 years (normal death fall-off)
- **Alignment Impact:** Killing individuals affects player's alignment (evil action, -1 to -5 alignment per kill depending on individual's importance)
- **Tree Impact:** Leaf removal reduces population count, may affect branch health if many leaves removed

**Use Cases:**
- **Targeted Elimination:** Remove problematic individuals (criminals, traitors, corrupt leaders)
- **Divine Punishment:** Execute individuals who have committed grave sins
- **Strategic Removal:** Eliminate key figures to destabilize factions or dynasties
- **Evil Playthrough:** Mass culling for evil-aligned players

**Implementation:**
```csharp
// Player picks leaf to kill individual
void OnPlayerPicksLeaf(Entity leafEntity, Entity playerEntity)
{
    var individual = GetIndividualFromLeaf(leafEntity);
    
    // Kill the individual
    KillIndividual(individual, DeathCause.DivineIntervention);
    
    // Update leaf state
    var leaf = GetLeafComponent(leafEntity);
    leaf.State = LeafState.Dying; // Yellow
    leaf.FallStartTime = currentTime;
    leaf.FallDuration = Random.Range(1year, 10years);
    
    // Update player alignment (evil action)
    var playerRoot = GetPlayerRoot(playerEntity);
    float alignmentPenalty = CalculateKillPenalty(individual);
    playerRoot.PlayerAlignment -= alignmentPenalty;
    
    // Visual feedback
    PlayKillAnimation(leafEntity); // Leaf withers, turns yellow
    ShowNotification($"You have ended {individual.Name}'s life.");
}

float CalculateKillPenalty(Entity individual)
{
    // More important individuals = larger alignment penalty
    var stats = GetIndividualStats(individual);
    float basePenalty = 1.0f;
    
    if (stats.IsNoble) basePenalty += 2.0f;
    if (stats.IsLeader) basePenalty += 3.0f;
    if (stats.HasAchievements) basePenalty += 1.0f;
    
    return basePenalty;
}
```

**2. Blessing Leaves with Miracles:**

**Mechanic:**
- **Action:** Player right-clicks/picks a leaf and selects "Bless" or "Miracle"
- **Effect:** The individual receives a **divine blessing** or **miracle** from the tree
- **Visual:** Leaf glows with golden/divine light, particles emanate from leaf
- **Alignment Impact:** Blessing individuals affects player's alignment (good action, +0.5 to +2 alignment per blessing)
- **Tree Impact:** Blessings may improve branch health, increase happiness in that branch

**Miracle Types:**

**1. Healing Miracle:**
- **Effect:** Cures diseases, injuries, restores health to full
- **Visual:** Green/golden healing particles, leaf becomes vibrant
- **Use Case:** Save individuals from plague, heal wounded heroes

**2. Strength Miracle:**
- **Effect:** Permanently increases individual's physical/magical abilities
- **Visual:** Leaf glows with power, branch slightly thickens
- **Use Case:** Empower champions, create legendary heroes

**3. Fortune Miracle:**
- **Effect:** Grants temporary luck boost, improves success rates
- **Visual:** Golden sparkles around leaf
- **Use Case:** Help individuals succeed in important endeavors

**4. Longevity Miracle:**
- **Effect:** Extends individual's lifespan significantly
- **Visual:** Leaf becomes more vibrant, ages slower
- **Use Case:** Preserve important individuals, extend dynasty lines

**5. Protection Miracle:**
- **Effect:** Grants temporary divine protection (reduced damage, immunity to certain threats)
- **Visual:** Shield-like aura around leaf
- **Use Case:** Protect individuals during wars, crises

**6. Revelation Miracle:**
- **Effect:** Grants knowledge, unlocks skills, reveals secrets
- **Visual:** Glowing runes appear on leaf
- **Use Case:** Empower mages, unlock technological breakthroughs

**Implementation:**
```csharp
// Player blesses leaf with miracle
void OnPlayerBlessesLeaf(Entity leafEntity, Entity playerEntity, MiracleType miracleType)
{
    var individual = GetIndividualFromLeaf(leafEntity);
    
    // Apply miracle effect
    ApplyMiracle(individual, miracleType);
    
    // Update leaf visual
    var leaf = GetLeafComponent(leafEntity);
    leaf.HasBlessing = true;
    leaf.BlessingType = miracleType;
    leaf.BlessingExpiryTime = currentTime + GetMiracleDuration(miracleType);
    
    // Update player alignment (good action)
    var playerRoot = GetPlayerRoot(playerEntity);
    float alignmentBonus = CalculateBlessingBonus(individual, miracleType);
    playerRoot.PlayerAlignment += alignmentBonus;
    
    // Visual feedback
    PlayBlessingAnimation(leafEntity, miracleType); // Golden glow, particles
    ShowNotification($"You have blessed {individual.Name} with {miracleType}.");
    
    // Tree health boost
    var branch = GetBranchFromLeaf(leafEntity);
    branch.Health += 0.01f; // Small health boost to branch
}

void ApplyMiracle(Entity individual, MiracleType miracleType)
{
    var stats = GetIndividualStats(individual);
    
    switch (miracleType)
    {
        case MiracleType.Healing:
            stats.Health = stats.MaxHealth;
            stats.Diseases.Clear();
            break;
            
        case MiracleType.Strength:
            stats.PhysicalPower += 10;
            stats.MagicalPower += 10;
            break;
            
        case MiracleType.Fortune:
            stats.LuckBoost = 50; // Lasts 1 year
            stats.LuckExpiry = currentTime + 1year;
            break;
            
        case MiracleType.Longevity:
            stats.MaxLifespan += 20years;
            break;
            
        case MiracleType.Protection:
            stats.DivineProtection = true;
            stats.ProtectionExpiry = currentTime + 5years;
            break;
            
        case MiracleType.Revelation:
            stats.KnowledgeBoost = 100;
            stats.SkillUnlockChance = 1.0f; // Guaranteed skill unlock
            break;
    }
}

float CalculateBlessingBonus(Entity individual, MiracleType miracleType)
{
    float baseBonus = 0.5f;
    
    // More important individuals = larger alignment bonus
    var stats = GetIndividualStats(individual);
    if (stats.IsNoble) baseBonus += 0.5f;
    if (stats.IsLeader) baseBonus += 1.0f;
    
    // More powerful miracles = larger bonus
    if (miracleType == MiracleType.Revelation || miracleType == MiracleType.Longevity)
        baseBonus += 0.5f;
    
    return baseBonus;
}
```

**Player Interaction UI:**

```
╔══════════════════════════════════════════════╗
║         INDIVIDUAL LEAF SELECTED            ║
╠══════════════════════════════════════════════╣
║ Name: Aldric the Bold                        ║
║ Age: 45                                      ║
║ Status: Village Guard                        ║
║ Health: 60/100 (Wounded)                     ║
║                                              ║
║ [Kill Individual]                            ║
║   └─ Ends this person's life immediately    ║
║                                              ║
║ [Bless with Miracle]                        ║
║   ├─ [Healing] - Restore health              ║
║   ├─ [Strength] - Increase abilities        ║
║   ├─ [Fortune] - Grant luck                 ║
║   ├─ [Longevity] - Extend lifespan          ║
║   ├─ [Protection] - Divine shield            ║
║   └─ [Revelation] - Grant knowledge          ║
║                                              ║
║ Alignment Impact: +1.5 (Good Action)         ║
╚══════════════════════════════════════════════╝
```

**Balance Considerations:**

**Killing Leaves:**
- **Cooldown:** May have cooldown per leaf (can't spam kill)
- **Cost:** May cost divine power/resources
- **Consequences:** Killing too many may cause unrest, rebellions
- **Alignment Lock:** Very evil actions may lock player into evil path

**Blessing Leaves:**
- **Cooldown:** May have cooldown per leaf (can't spam blessings)
- **Cost:** May cost divine power/resources
- **Tree Health:** Blessings improve tree health, but too many may exhaust tree
- **Alignment Lock:** Very good actions may lock player into good path

**Strategic Depth:**
- **Target Selection:** Players must choose carefully - kill threats or bless heroes?
- **Resource Management:** Limited miracles/kills per time period
- **Long-term Impact:** Killing key figures may destabilize civilizations
- **Moral Choices:** Players face consequences of their divine interventions

### Future Gameplay Possibilities

**1. Pilgrimage and Rituals:**
- **Mechanic:** Entities travel to tree for religious rituals
- **Effect:** Boost morale, grant blessings, unlock special spells
- **Implementation:** Pilgrimage task, radius-based buff zone

**2. World-Tree-Based Magic:**
- **Mechanic:** Druids/shamans channel tree's power for spells
- **Effect:** Spell power scales with tree vitality/alignment
- **Example:** "Tree's Wrath" spell only available if tree is vengeful

**3. Environmental Effects:**
- **Mechanic:** Tree affects local weather/terrain
- **Effect:** Good tree → fertile lands, Evil tree → barren wastelands
- **Implementation:** Radius-based terrain modifier

**4. Tech Tree Visualization:**
- **Mechanic:** Players "read" tech progress by examining tree branches
- **Effect:** Educational, intuitive tech tree interface
- **Implementation:** Clickable branches show tech details

**5. Conquest Objective:**
- **Mechanic:** Destroying enemy's world tree = victory condition
- **Effect:** High-stakes warfare, must protect tree at all costs
- **Implementation:** Tree has HP, dies if destroyed

**6. Resurrection Minigame:**
- **Mechanic:** If tree dies, players must nurture seedlings to resurrect it
- **Effect:** Recovery challenge after catastrophic failure
- **Implementation:** Planting minigame, resource investment

**7. Cross-World Comparison:**
- **Mechanic:** Players manage multiple worlds, compare their trees
- **Effect:** Competitive "which world is healthiest?" metagame
- **Implementation:** World selection screen shows all trees side-by-side

**8. Player Housing/Guild Halls:**
- **Mechanic:** Players build structures within tree's branches
- **Effect:** Physical connection to world tree, prestige location
- **Implementation:** Branch platforms, interior spaces

---

## Example Tree States

### Example 1: Utopian Spiritual Civilization

**Population:** 50,000 entities
**Alignment:** +90 Good, 5 Corruption
**Ethics:** +85 Spiritual
**Warfare:** 10 Warlike (peaceful)
**Might/Magic:** +75 Magic
**Traits:** +80 Forgiving, +70 Xenophilic
**Happiness:** 90
**Tech:** Arcane (Tier 3), Transcendent (Tier 7)

**Tree Appearance:**
- **Scale:** ~4.7 (massive, 300+ meters tall)
- **Trunk:** Translucent crystal, glowing with golden runes
- **Bark:** Prismatic rainbows, smooth ethereal texture
- **Branches:** Long, spindly, weaving in impossible patterns, glowing with arcane energy
- **Leaves:** Floating golden wisps, untethered, forming mandala patterns
- **Roots:** Barely touching ground, levitating sections with visible ley lines
- **Aura:** Golden particles, rainbow auroras, harmonic chimes
- **Tech Branches:** Glowing arcane branch (large), reality-bending transcendent branch (medium)
- **Scars:** Minimal (peaceful history)
- **Overall:** Majestic, otherworldly, looks like a divine artifact

**Player Feeling:** "This is paradise. I've built something beautiful."

---

### Example 2: Dystopian Cyberpunk Warzone

**Population:** 25,000 entities
**Alignment:** -70 Evil, 80 Corruption
**Ethics:** -90 Materialist
**Warfare:** 95 Warlike (constant war)
**Might/Magic:** -60 Might
**Traits:** -85 Vengeful, -60 Xenophobic
**Happiness:** 20
**Tech:** Industrial (Tier 4), Atomic (Tier 5), Information (Tier 6)

**Tree Appearance:**
- **Scale:** ~4.4 (large, 200+ meters tall)
- **Trunk:** Cybernetic, metallic panels, riveted armor plates
- **Bark:** Charred black metal, deep gashes, burn marks, blood stains
- **Branches:** Thick, short, aggressive spread, hydraulic pistons, weapon-like
- **Leaves:** Solar panels, holographic screens, LED grids (many broken)
- **Roots:** Underground cables, piping, torn-up earth from battles
- **Aura:** Smoke, sparks, electrical arcing, oppressive red lighting
- **Tech Branches:** Massive industrial branch (gears, smokestacks), atomic branch (glowing radioactive), information branch (holographic data streams)
- **Scars:** Catastrophic (entire sections destroyed, siege weapon impacts, fire damage)
- **Overall:** Nightmarish, looks like a war machine, intimidating

**Player Feeling:** "This is hell. What have I created?"

---

### Example 3: Dying World (Resurrection)

**Population:** 3 entities (was 10,000)
**Alignment:** -20 Evil (survivors traumatized)
**Ethics:** 0 Neutral
**Warfare:** 60 Warlike (recent catastrophe)
**Might/Magic:** 0 Neutral
**Traits:** -40 Vengeful, -30 Xenophobic
**Happiness:** 10
**Tech:** Classical (Tier 2)

**Tree Appearance:**
- **Scale:** ~0.5 (tiny saplings sprouting from massive husk)
- **Husk:** Massive skeletal tree (scale ~4.0, 250 meters), petrified, ghostly gray
- **New Growth:** Tiny green shoots at base, 3 small saplings (1 meter tall)
- **Bark (Husk):** Cracked, petrified stone, frozen in death
- **Bark (Saplings):** Fragile green, struggling
- **Aura:** Mournful, faint green glow fighting against gray death
- **Scars:** Entire husk is a scar monument
- **Resurrection Progress:** 2% (just begun)
- **Overall:** Tragic, post-apocalyptic, haunting beauty

**Player Feeling:** "There's still hope. I can rebuild."

---

### Example 4: Balanced Classical Civilization

**Population:** 5,000 entities
**Alignment:** +20 Good, 30 Corruption
**Ethics:** +10 Spiritual (slight lean)
**Warfare:** 40 Warlike (occasional conflicts)
**Might/Magic:** +10 Magic (slight lean)
**Traits:** +15 Forgiving, +25 Xenophilic
**Happiness:** 65
**Tech:** Classical (Tier 2), Arcane (Tier 3) early

**Tree Appearance:**
- **Scale:** ~3.7 (mature, 100 meters tall)
- **Trunk:** Healthy brown wood, natural grain
- **Bark:** Textured, with some metalworking decorations (bronze shields, iron tools)
- **Branches:** Balanced spread, some with subtle magical glows
- **Leaves:** Lush green, full canopy, slight golden shimmer
- **Roots:** Strong, grounded, intermingled with other plants
- **Aura:** Natural, pleasant, occasional magical sparkle
- **Tech Branches:** Classical branch (bronze decorations), small arcane branch (glowing crystals just appearing)
- **Scars:** Moderate (healed-over battle wounds, shows history)
- **Overall:** Realistic, relatable, healthy civilization in progress

**Player Feeling:** "This feels like a real civilization. We're doing well."

---

## Cross-Project Adaptations

### Godgame (Primary)

**Full Implementation:**
- Yggdrasil-style world tree
- All morphing systems active
- Multiple worlds = multiple trees
- Tree visible from world map view
- Can zoom in to see details

**Cultural Variants:**
- **Norse Cultures:** Classic Yggdrasil (ash tree, nine realms symbolism)
- **Elven Cultures:** Ethereal, crystalline tree (Telperion/Laurelin inspiration)
- **Dwarven Cultures:** Stone-metal hybrid tree (mountain roots, metalwork branches)
- **Orcish Cultures:** Dark, thorny, aggressive growth
- **Human Cultures:** Oak-like, versatile, balanced

### Space4X (Adapted)

**"Nexus Crystal" or "Station Spire":**

**Concept:** Instead of organic tree, space stations/colonies have a central crystalline/architectural structure that morphs based on civilization state.

**Morphing Variables (Same Logic):**
- **Alignment:** Crystal color (blue = good, red = evil)
- **Ethics:** Tech style (sleek tech = materialist, glowing runes = psionic)
- **Warfare:** Battle damage (scars, hull breaches)
- **Tech:** New modules/wings added per tech tier
- **Population:** Size/scale of station
- **Happiness:** Lighting (bright = happy, dim = miserable)

**Visual:**
- **Good + Materialist:** Clean white station, blue lights, pristine
- **Evil + Psionic:** Dark crystal fortress, red glows, menacing
- **Warlike:** Scorched hull, weapon emplacements, fortifications
- **Peaceful:** Gardens, observation decks, artistic architecture

**Same Emotional Impact:** Players look at their station and instantly know civilization state.

---

## Data Structures

### World Tree Component

```csharp
public struct WorldTreeComponent : IComponentData
{
    // Aggregated Stats (from all entities)
    public float AverageGoodEvil;            // -100 to +100
    public float AverageCorruption;          // 0 to 100
    public float AverageMaterialSpiritual;   // -100 to +100
    public float AverageMightMagicAxis;      // -100 to +100
    public float AverageVengefulForgiving;   // -100 to +100
    public float AverageBoldCraven;          // -100 to +100
    public float AverageXenophobia;          // -100 to +100
    public float AverageHappiness;           // 0 to 100

    // Warfare and Damage
    public float WarlikeLevel;               // 0 to 100
    public float TotalScars;                 // Accumulated, never resets

    // Population and Scale
    public int TotalPopulation;              // Living entity count
    public float TreeScale;                  // Logarithmic scale
    public float DeathRate;                  // Deaths per minute

    // Tech Progression
    public int UnlockedTechTiers;            // Bitmask of unlocked tiers
    public float TechDiversity;              // 0-1, how many different tech paths

    // Tree State
    public bool IsDead;                      // True if population = 0
    public float ResurrectionProgress;       // 0-1, regrowth from husk
    public long DeathTimestamp;              // When tree died (for husk age)

    // Visual State (cached for performance)
    public float CurrentTrunkRadius;
    public float CurrentBranchAngle;
    public int CurrentLeafCount;
    public Color CurrentBarkColor;
    public Color CurrentLeafColor;
}
```

### Tech Branch Component

```csharp
public struct TechBranchComponent : IBufferElementData
{
    public TechTier Tier;                    // Tribal, Classical, Arcane, etc.
    public float Mastery;                    // 0-1, development level
    public long UnlockTimestamp;             // When unlocked
    public float3 BranchGrowthDirection;     // Where this branch grows from trunk
    public float BranchScale;                // Visual size of this tech branch
}

public enum TechTier : byte
{
    Tribal = 1,
    Classical = 2,
    Arcane = 3,
    Industrial = 4,
    Atomic = 5,
    Information = 6,
    Transcendent = 7
}
```

---

## Open Questions

1. **Player Interaction:** Should players be able to directly interact with the tree (touch, climb, harvest), or is it purely visual monument?
   - **Option A:** Pure monument (no interaction, reduces complexity)
   - **Option B:** Harvestable (players collect magical seeds/leaves for crafting)
   - **Recommendation:** Option A initially, Option B as future expansion

2. **Multiple Trees per World:** Should large worlds have regional trees (one per continent/island), or always one global tree?
   - **Option A:** One global tree (easier to track, clear world state)
   - **Option B:** Regional trees (more granular, shows regional differences)
   - **Recommendation:** Option A for Godgame, Option B for Space4X (multiple colonies)

3. **Tree Damage from Attacks:** Can enemies attack and damage the world tree directly?
   - **Option A:** Invulnerable (pure visualization, can't be targeted)
   - **Option B:** Vulnerable (can be sieged, becomes victory condition)
   - **Recommendation:** Option A initially, Option B as optional game mode

4. **Resurrection Control:** Should resurrection be automatic (when population > 0), or require player action (planting ritual)?
   - **Option A:** Automatic (simpler, always happens)
   - **Option B:** Player-initiated (adds challenge, recovery gameplay)
   - **Recommendation:** Option A (automatic but slow, visible progress)

5. **Tree Age Visualization:** Should the tree show age (rings, ancient appearance) independent of population?
   - **Option A:** Age affects appearance (old worlds have ancient trees)
   - **Option B:** Only population/stats affect appearance (no age variable)
   - **Recommendation:** Option A (trunk ring count = world age in years)

6. **Multiplayer Visibility:** In multiplayer, can players see each other's world trees?
   - **Option A:** Private (only owner sees their tree)
   - **Option B:** Public (all players see all trees, competitive comparison)
   - **Recommendation:** Option B (encourages competition, "my tree is healthier than yours")

---

## Implementation Roadmap

### Phase 1: Basic Tree Visualization (MVP)
- **Goal:** Single tree per world, basic scale based on population
- **Features:**
  - Procedural tree mesh generation (trunk + branches + leaves)
  - Scale based on population (logarithmic)
  - Simple death/resurrection (tree dies at population 0, regrows when repopulated)
- **Success Metric:** Players can see tree grow as population increases

### Phase 2: Alignment Morphing
- **Goal:** Tree appearance reflects good/evil, corruption
- **Features:**
  - Shader-based color morphing (green ↔ black, healthy ↔ diseased)
  - Particle systems (golden sparkles for good, smoke for evil)
- **Success Metric:** Players can identify world alignment by looking at tree

### Phase 3: Full Morphing System
- **Goal:** All variables affect tree appearance
- **Features:**
  - Ethics axis (materialist/spiritual visual effects)
  - Might/Magic axis (chunky vs. spindly mesh deformation)
  - Behavioral traits (branching patterns, density)
  - Happiness (vitality, withering)
  - War scars (permanent damage overlays)
- **Success Metric:** Tree is unique visual fingerprint of civilization

### Phase 4: Tech Branches
- **Goal:** Technology unlocks grow special branches
- **Features:**
  - Tech-specific branch geometry
  - Branch growth animation on tech unlock
  - Multiple tech paths = diverse branch types
- **Success Metric:** Players can "read" tech progress from tree structure

### Phase 5: Gameplay Integration (Optional)
- **Goal:** Tree becomes interactive gameplay element
- **Features:**
  - Pilgrimage system
  - World-tree-based magic
  - Environmental effects
  - Victory conditions (tree destruction)
- **Success Metric:** Tree is core strategic element, not just cosmetic

---

## Related Systems

- **Aggregate Entities and Individual Dynamics:** [Aggregate_Entities_And_Individual_Dynamics.md](Aggregate_Entities_And_Individual_Dynamics.md) - Individual stats feed into world tree
- **Languages and Magic System:** [Languages_And_Magic_System.md](Languages_And_Magic_System.md) - Magic alignment affects tree appearance
- **Focus and Status Effects:** [../Implemented/Core/Focus_And_Status_Effects_System.md](../Implemented/Core/Focus_And_Status_Effects_System.md) - World-tree-based buffs/debuffs
- **Guild System:** [../Villagers/Guild_System.md](../Villagers/Guild_System.md) - Guild members' stats aggregate to tree

---

## Design Intent

**What It Should Feel Like:**

**Player Experience:**
- **Pride:** "Look at my beautiful tree! My civilization is thriving."
- **Shame:** "My tree is corrupted and dying. I need to fix this."
- **History:** "Those scars show when we fought the great war 50 years ago."
- **Recovery:** "The tree is regrowing! We're coming back from the brink."
- **Competition:** "My tree is bigger and more beautiful than my friend's."

**Emotional Hooks:**
- **Tamagotchi Effect:** Players check tree health compulsively, feel responsible for its wellbeing
- **Bonsai Care:** Players make decisions based on "how will this affect my tree?"
- **Monument Pride:** Tree becomes symbol of player's achievement, screenshot-worthy

**Narrative Emergent:**
- A thriving golden tree suddenly scarred and withered → tells story of invasion and decline
- A dead husk slowly regrowing → tells story of recovery and hope
- A massive tree with diverse tech branches → tells story of scientific renaissance
- A dark, cybernetic, war-torn tree → tells story of dystopian nightmare

**The World Tree is the Player's Legacy Made Visible.**

---

**Last Updated:** 2025-12-03
**Status:** Concept Captured - Ready for Technical Design
