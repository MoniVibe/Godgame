# Immaterium Galactic Visualization System (Space4X)

**Status:** Concept
**Priority:** Medium (Strategic UI Feature with High Player Engagement Potential)
**Cross-Project:** Space4X (primary), Godgame (not applicable - uses World Tree instead)
**Last Updated:** 2025-12-03

---

## Overview

**Purpose:** An ethereal "immaterium" overlay visualization that reveals the galaxy's aggregate psychic/spiritual state when zooming to galactic view. Each system manifests as a **colored and effect-based cloud** representing the visual aggregate of that system's individuals' outlooks, alignments, happiness, morale, memories, wars, and more. These system clouds combine to form a **giant galactic tree cloud** structure.

**Player Impact:** The ultimate strategic overview - instantly understand galactic balance of power, faction alignments, trade networks, war zones, and cultural influence at a glance. Makes complex multi-system empire management intuitive.

**System Role:** UI visualization layer that aggregates all civilization statistics across the galaxy into a comprehensible, beautiful cloud-based data visualization. Related to [World_Tree_Visualization_System.md](World_Tree_Visualization_System.md) but adapted for space-based hierarchical structure using cloud visualization instead of organic tree.

### Design Philosophy

**The Galaxy Has a Soul (Immaterium):**
- **Normal View:** Physical space - planets, stars, ships, realistic
- **Immaterium View:** Psychic/spiritual overlay - **colored and effect-based clouds** representing aggregate themes, connections, emotions, abstract
- **Toggle Between:** Players switch views like toggling minimap modes
- **Strategic Tool:** Not just aesthetic - reveals information invisible in normal view

**Cloud-Based Visualization:**
- **System Clouds:** Each system manifests as a colored, effect-based cloud that visually aggregates all individuals' outlooks, alignments, happiness, morale, memories, wars, and more
- **Galactic Tree Cloud:** All system and cluster clouds combine to form a **giant galactic tree cloud** structure spanning the galaxy
- **Visual Aggregate:** The cloud is a living visualization of the system's collective consciousness, emotions, and history
- **Dynamic Morphing:** Clouds change color, density, shape, and effects based on aggregated stats from all individuals within the system

**Hierarchical Structure as Tree:**
```
Galaxy
├── Clusters (groups of nearby systems)
│   ├── Systems (star systems)
│   │   ├── Planets (individual worlds)
│   │   │   └── Colonies (settlements on planets)
```

**Each level aggregates statistics upward:**
- Colonies → Planet → System → Cluster → Galaxy
- Like branches → trunk → tree (World Tree equivalent)

**Cloud-Based Tree Structure:**
The immaterium manifests as a giant galactic tree made of clouds where:
- **System Clouds** = Each system appears as a colored, effect-based cloud (visual aggregate of individuals' outlooks, alignments, happiness, morale, memories, wars)
- **Cluster Clouds** = Groups of system clouds merge into larger cloud formations
- **Galactic Tree Cloud** = All system and cluster clouds combine to form a massive tree-like cloud structure spanning the galaxy
- **Cloud Connections** = Trade routes, alliances, psychic bonds, rivalries appear as flowing cloud tendrils connecting system clouds
- **Colors & Effects** = Cloud color, density, particle effects reflect faction alignment, ethics, warfare, tech level, happiness

---

## Core Mechanics

### Hierarchical Aggregation

**Galaxy → Clusters → Systems → Planets → Colonies**

Same bottom-up aggregation as World Tree:

1. **Colonies:** Each settlement (space station, planetary colony) contributes stats
2. **Planets:** Aggregate all colonies on that planet
3. **Systems:** Aggregate all planets in that star system
4. **Clusters:** Aggregate nearby systems (spatial grouping, 5-20 systems)
5. **Galaxy:** Aggregate all clusters

**Aggregation Formula:**
```csharp
GalaxyTheme = WeightedAverage(AllColonies) + FactionInfluence + ConnectionStrength
```

### View Modes

**Normal Galactic View (Physical):**
- Realistic star map
- Planets orbiting stars
- Ships moving along paths
- Territory borders
- Trade route lines

**Immaterium View (Spiritual/Psychic):**
- Abstract ethereal aesthetic
- Systems become glowing nodes
- Connections become energy flows
- Factions become color schemes
- Theme/alignment visible instantly

**Toggle Mechanics:**
- **Keybind:** Tab or dedicated button
- **Smooth Transition:** 2-second fade/morph between views
- **Zoom Threshold:** Auto-activates at maximum zoom-out (galactic scale)
- **Layer Blending:** Can overlay immaterium at 50% transparency over normal view

---

## Visual Representation

### System Cloud Visualization

**System Clouds (Individual Star Systems):**

Each system manifests as a **colored and effect-based cloud** that visually aggregates all individuals' outlooks, alignments, happiness, morale, memories, wars, and other stats within that system.

**Cloud Size:**
- Based on total population across all planets/colonies
- Formula: `CloudRadius = log10(totalPopulation + 1) × scaleFactor`
- Range: Small outpost (tiny wisp) → Megacity ecumenopolis (massive cloud formation)
- **Density:** More population = denser, more opaque cloud

**Cloud Color (Primary Visual Aggregate):**
- **Base Color:** Faction ownership (red = Empire A, blue = Empire B, etc.)
- **Color Tint:** Aggregated alignment from all individuals
  - Good (+75 to +100): Bright, saturated, warm (golden, white, light blue)
  - Evil (-100 to -75): Dark, desaturated, cold (black, dark red, purple)
  - Neutral (-25 to +25): Balanced, natural (gray, muted tones)
- **Color Mixing:** Multiple factions in system = blended cloud colors (patchwork)

**Cloud Density & Opacity:**
- **Happiness Level:** Affects cloud brightness and density
  - Joyful (75-100): Radiant, dense, pulsing cloud
  - Content (50-75): Steady, healthy cloud density
  - Unhappy (25-50): Thin, wispy, flickering cloud
  - Miserable (0-25): Dark, sparse, barely visible cloud
- **Morale:** High morale = vibrant, low morale = faded

**Cloud Shape & Form:**
- **Ethics Axis (Materialist/Spiritual):**
  - Materialist (-100 to 0): Geometric, angular cloud formations (circuit-like, industrial)
  - Spiritual (0 to +100): Organic, flowing cloud formations (mandala-like, ethereal)
  - Balanced: Mix of both geometric and organic shapes
- **Tech Level:** Higher tech = more complex cloud structures
  - Tribal (Tier 1): Simple, basic cloud wisps
  - Atomic (Tier 5): Radioactive cloud patterns, energy swirls
  - Transcendent (Tier 7): Reality-distorting cloud formations, dimensional rifts

**Cloud Effects & Particles:**
- **War Memories:** Past wars leave dark streaks, scars, or turbulent patches in cloud
- **Active Wars:** Violent, chaotic cloud turbulence, red flashes
- **Memories:** Historical events manifest as persistent cloud patterns (memories embedded in cloud structure)
- **Morale:** High morale = gentle, flowing cloud movement; low morale = turbulent, chaotic
- **Tech Particles:** Sparks, energy wisps, digital patterns based on tech level

**Cluster Clouds (Groups of Systems):**

**Visual:**
- Multiple system clouds merge into larger cloud formations
- Cluster clouds encompass and blend constituent system clouds
- Color = dominant faction in cluster (most systems owned)
- Pattern = aggregate ethics/alignment of entire cluster
- **Tree Structure:** Cluster clouds form branches of the galactic tree cloud

**Size:**
- Based on number of systems in cluster + total population
- Clusters with 20 systems appear 3-5× larger than single-system clouds
- **Density:** More systems = denser cluster cloud

**Galactic Tree Cloud:**

**Structure:**
- All system and cluster clouds combine to form a **giant galactic tree cloud**
- Tree trunk = galactic core (densest cloud formation)
- Tree branches = cluster clouds extending outward
- Tree leaves/twigs = individual system clouds
- **Hierarchical:** Clusters form major branches, systems form smaller branches/twigs

**Visual Properties:**
- Tree cloud structure reflects galactic organization and relationships
- Connections between systems appear as cloud tendrils/branches
- Tree health = aggregate galactic happiness, prosperity, peace
- Tree corruption = aggregate galactic evil, war, suffering

### Cloud Connection Visualization (Cloud Tendrils/Branches)

**Connection Types (Cloud Tendrils Between System Clouds):**

**1. Trade Routes (Economic):**
- **Visual:** Flowing cloud tendrils connecting system clouds, green/gold tinted
- **Color:** Green/gold cloud wisps (economic prosperity)
- **Thickness/Density:** Based on trade volume (thin wisp = 100 credits/day, thick tendril = 100,000 credits/day)
- **Animation:** Resource particles flowing through cloud tendril (ore, energy, food icons)
- **Bidirectional:** Cloud tendril shows import/export direction with particle flow
- **Cloud Properties:** Dense, flowing, stable cloud formation

**2. Alliances (Diplomatic):**
- **Visual:** Stable, glowing cloud bridges between allied system clouds
- **Color:** Blue/white cloud tendrils (peaceful cooperation)
- **Thickness/Density:** Based on alliance strength (military pact = thick cloud bridge, non-aggression = thin wisp)
- **Animation:** Gentle pulsing cloud movement, harmonic resonance
- **Mutual:** Equal cloud connection both directions
- **Cloud Properties:** Harmonious, stable, peaceful cloud formation

**3. Psychic/Psionic Bonds (Spiritual):**
- **Visual:** Ethereal, flowing cloud streams (aurora-like cloud formations)
- **Color:** Purple/violet cloud wisps (psionic energy)
- **Thickness/Density:** Based on psionic population + spiritual axis alignment
- **Animation:** Swirling, unpredictable cloud patterns, mystical flow
- **Requirements:** Both systems must have spiritual-aligned populations
- **Cloud Properties:** Ethereal, otherworldly, reality-bending cloud formations

**4. Data Networks (Information):**
- **Visual:** Digital cloud streams, binary code patterns flowing through cloud
- **Color:** Cyan/electric blue cloud tendrils (digital information)
- **Thickness/Density:** Based on bandwidth/communication infrastructure
- **Animation:** Fast-moving data packets through cloud, geometric cloud patterns
- **Requirements:** Advanced tech tier (Information/Transcendent)
- **Cloud Properties:** Structured, geometric, tech-infused cloud formations

**5. Rivalries/Tensions (Hostile):**
- **Visual:** Jagged, unstable cloud connections (lightning-like cloud formations)
- **Color:** Orange/red cloud tendrils (hostility, warning)
- **Thickness/Density:** Based on tension level (border dispute = thin, total war = thick)
- **Animation:** Crackling cloud turbulence, aggressive pulsing
- **Warning:** Indicates potential conflict zones
- **Cloud Properties:** Turbulent, chaotic, unstable cloud formations

**6. Active Warfare (Combat):**
- **Visual:** Violent, chaotic cloud bursts between system clouds
- **Color:** Bright red cloud formations (active combat)
- **Thickness/Density:** Based on fleet size engaged
- **Animation:** Explosive cloud effects, battle turbulence, ships clashing
- **Sound:** Battle audio cues when zoomed in
- **Cloud Properties:** Violent, destructive, chaotic cloud formations

**Connection Layering (Multiple Cloud Tendrils):**
- Multiple connection types can exist simultaneously between two system clouds
- Example: Trade route + Alliance + Data network = 3 overlapping cloud tendrils
- Visual stacking: Cloud tendrils slightly offset or blended to avoid overlap confusion
- **Tree Structure:** Connections form branches of the galactic tree cloud, connecting system clouds like tree branches

### Theme-Based Cloud Morphing

**Immaterium Theme (System Cloud Aggregate):**

Each system cloud's **immaterium theme** is a visual aggregate of that system's individuals' outlooks, alignments, happiness, morale, memories, wars, and more. The cloud morphs and changes based on these aggregated factors.

**Alignment Morphing (Good/Evil):**

**Good-Aligned System Clouds (+75 to +100):**
- **Cloud Color:** Bright, warm colors (gold, white, light blue)
- **Cloud Density:** Dense, vibrant, radiant clouds
- **Cloud Movement:** Smooth, flowing, harmonious cloud patterns
- **Cloud Particles:** Gentle, peaceful (butterflies, light wisps, stars)
- **Cloud Aura:** Radiant, inviting, pulsing with life
- **Sound:** Harmonic chimes, uplifting music

**Evil-Aligned System Clouds (-100 to -75):**
- **Cloud Color:** Dark, cold colors (black, dark red, purple)
- **Cloud Density:** Dense but oppressive, dark clouds
- **Cloud Movement:** Turbulent, jagged, aggressive cloud patterns
- **Cloud Particles:** Ominous (smoke, ash, dark energy)
- **Cloud Aura:** Oppressive, menacing, suffocating
- **Sound:** Ominous drone, unsettling whispers

**Corruption Overlay:**
- **Independent Variable:** Crime, betrayal, decay (0-100)
- **Visual:** Disease-like dark patches spreading through cloud, corruption streaks
- **Example:** Good-aligned but corrupt → Beautiful bright cloud with dark diseased patches
- **Cloud Properties:** Corruption manifests as dark veins, rot, or decay patterns within the cloud

**Ethics Morphing (Materialist/Spiritual):**

**Materialist System Clouds (-100 to 0):**
- **Cloud Shape:** Geometric, angular cloud formations, metallic textures
- **Cloud Tendrils:** Straight cloud connections, circuit-like patterns, industrial
- **Cloud Particles:** Sparks, steam, machinery effects within cloud
- **Cloud Pattern:** Grid-based, organized, technological cloud structure
- **Sound:** Mechanical hums, industrial sounds

**Spiritual System Clouds (0 to +100):**
- **Cloud Shape:** Organic, flowing cloud formations, crystalline textures
- **Cloud Tendrils:** Curved, natural cloud connections, ethereal flows
- **Cloud Particles:** Energy wisps, floating runes, aurora effects within cloud
- **Cloud Pattern:** Mandala, fractal, mystical cloud structure
- **Sound:** Chimes, bells, ethereal harmonics

**Xenophobia Morphing:**

**Xenophobic System Clouds (-100 to -50):**
- **Cloud Connections:** Sparse, isolated cloud tendrils, defensive formations
- **Visual:** Fortress-like cloud formations, wall-like cloud barriers
- **Behavior:** Few cloud connections to other factions, inward-focused cloud structure
- **Cloud Color:** Homogeneous (single faction color only, no mixing)

**Xenophilic System Clouds (+50 to +100):**
- **Cloud Connections:** Dense, diverse cloud tendrils, welcoming formations
- **Visual:** Open cloud formations, mixing colors from different factions
- **Behavior:** Many cloud connections to various factions, cosmopolitan cloud structure
- **Cloud Color:** Patchwork, rainbow, multicultural blend of cloud colors

**Might/Magic (Tech/Psionic) Morphing:**

**Tech-Focused System Clouds (-100 to -50):**
- **Cloud Shape:** Chunky, robust cloud formations, fortress-like density
- **Cloud Tendrils:** Thick cloud cables, industrial cloud infrastructure
- **Cloud Particles:** Mechanical, hard-edged particles within cloud
- **Visual:** Looks durable, physical, material cloud formations

**Psionic-Focused System Clouds (+50 to +100):**
- **Cloud Shape:** Delicate, spindly cloud formations, tower-like structures
- **Cloud Tendrils:** Thin energy cloud beams, barely visible cloud threads
- **Cloud Particles:** Ethereal, glowing, immaterial particles within cloud
- **Visual:** Looks fragile but radiates power, otherworldly cloud formations

**Memory-Based Cloud Patterns:**

**War Memories:**
- Past wars leave permanent dark streaks, scars, or turbulent patches in cloud
- Historical battles manifest as persistent cloud patterns
- Cloud density affected by war trauma (thinner, darker patches)

**Achievement Memories:**
- Great achievements create bright, glowing patches in cloud
- Historical accomplishments leave golden or crystalline cloud formations
- Cloud structure enhanced by positive memories

**Cultural Memories:**
- Cultural events, traditions manifest as recurring cloud patterns
- Shared memories create synchronized cloud movements between connected systems
- Cloud color influenced by cultural identity and shared experiences

### Warfare and Conflict Cloud Visualization

**Peacetime:**
- System clouds have smooth, flowing cloud formations
- Cloud connections are stable, harmonious cloud tendrils
- Cloud colors are saturated, healthy, vibrant

**Skirmishes (Low-Intensity Conflict):**
- Minor battle scars manifest as small dark patches or turbulent areas in cloud
- Rivalry cloud connections appear (orange cloud tendrils)
- Occasional red pulses/flashes within cloud at conflict sites
- Cloud density slightly reduced, minor turbulence

**Total War (High-Intensity):**
- **System Clouds:** Heavily damaged cloud formations (dark patches, turbulent, chaotic)
- **Cloud Connections:** Violent red cloud energy surges, explosive cloud tendrils
- **Cloud Particles:** Explosions, debris, ship wrecks within cloud
- **Cloud Aura:** Chaotic, unstable, flickering cloud formations
- **Sound:** Battle sounds, alarms, distress signals
- **Cloud Properties:** War-torn clouds become thin, dark, turbulent, scarred

**War Scars (Permanent Cloud Patterns):**
- Past battles leave permanent dark streaks, scars, or turbulent patches in cloud
- Historical conflicts visible as faded scar patterns within cloud structure
- Destroyed systems remain as ghostly, transparent cloud husks (memorials)
- **Memory Integration:** War memories permanently embedded in cloud structure, affecting cloud appearance

### Population and Cloud Scale

**Population Scaling (Cloud Size & Density):**

**Cloud Size Formula:**
```csharp
float cloudRadius = Mathf.Log10(systemPopulation + 1) * scaleFactor;
float cloudDensity = Mathf.Clamp01(Mathf.Log10(systemPopulation + 1) / 10.0f); // 0-1 density
```

**Examples:**
- **100 colonists:** Tiny cloud wisp (5px radius, very sparse)
- **10,000 colonists:** Small cloud formation (10px, thin density)
- **1 million colonists:** Established cloud (20px, moderate density)
- **100 million colonists:** Major cloud formation (35px, dense)
- **10 billion colonists:** Massive cloud ecumenopolis (50px, very dense, dominates cluster)

**Cloud Density:**
- More population = denser, more opaque cloud
- High population = vibrant, thick cloud formations
- Low population = thin, wispy cloud formations

**Death and Decline:**
- **Population Loss:** Cloud shrinks proportionally, becomes thinner
- **Extinction:** Cloud becomes ghost (transparent, faded cloud wisp)
- **Recovery:** Cloud slowly regrows and becomes denser as population increases
- **Memory Preservation:** Even extinct systems' clouds retain faint memory patterns (historical cloud remnants)

---

## Faction Influence Visualization

### Multi-Faction Systems

**Contested Systems:**
- Node split into pie chart segments (each faction controls % of system)
- Example: 60% Empire A (red), 30% Rebellion (blue), 10% Neutral (gray)
- Connections from each faction diverge from respective segment

**Border Systems:**
- Systems near faction borders have gradient colors (blending factions)
- Visual tension (crackling energy between rival factions)

**Independence Movements:**
- Systems attempting secession pulse/flicker between old and new faction colors
- Unstable aura, warning indicators

### Faction Themes

**Example Faction Visualizations:**

**United Federation (Xenophilic, Peaceful, Democratic):**
- **Color:** Blue/white
- **Nodes:** Open, welcoming, diverse
- **Connections:** Dense alliance networks, many trade routes
- **Particles:** Stars, peace symbols, diplomatic envoys
- **Sound:** Uplifting, cooperative

**Warlike Empire (Militarist, Authoritarian, Xenophobic):**
- **Color:** Red/black
- **Nodes:** Fortress-like, imposing, angular
- **Connections:** Sparse (isolationist), aggressive toward enemies
- **Particles:** Weapons fire, military symbols, marching troops
- **Sound:** Marching drums, war chants

**Spiritual Collective (Psionic, Pacifist, Egalitarian):**
- **Color:** Purple/gold
- **Nodes:** Crystalline, glowing, mystical
- **Connections:** Psionic bonds, ethereal flows
- **Particles:** Floating runes, meditation symbols, aurora lights
- **Sound:** Harmonic chimes, spiritual chants

**Materialist Megacorp (Capitalist, Technological, Ruthless):**
- **Color:** Orange/gray
- **Nodes:** Industrial, geometric, corporate logos
- **Connections:** Trade routes (dominant), data networks
- **Particles:** Credits, resources, corporate advertisements
- **Sound:** Cash registers, stock market bells, machinery

**Hive Mind (Collectivist, Expansionist, Alien):**
- **Color:** Green/brown (organic)
- **Nodes:** Organic, living, pulsing biomass
- **Connections:** Biological tendrils, neural networks
- **Particles:** Spores, insects, biological material
- **Sound:** Insect buzzing, organic pulsing, alien whispers

---

## Strategic Information Overlay

### Resource Flow Visualization

**Trade Route Emphasis Mode:**
- Brighten trade connections, dim everything else
- Show resource types flowing (icons: ore, energy, food, credits)
- Identify trade hubs (largest nodes with most connections)
- Detect bottlenecks (single systems controlling key routes)

**Highlighted Information:**
- **Economic Power:** Cluster with highest trade volume glows brightest
- **Supply Chains:** Follow resource from mining colony → refinery → factory → market
- **Trade Imbalances:** Red highlights on systems with negative trade balance

### Diplomatic Relationship Network

**Alliance Web Mode:**
- Show only alliance connections
- Visualize alliance blocs (clusters of allied factions form unified color zones)
- Identify isolated factions (no alliances, surrounded by enemies)

**Tension Heatmap:**
- Color systems by threat level
  - Green: Safe, surrounded by allies
  - Yellow: Neutral, mixed neighbors
  - Orange: Tense, border disputes
  - Red: Critical, active warfare

### Threat Detection

**Plague/Disease Spread:**
- Infected systems pulse with sickly green/yellow
- Disease connections show spreading vector
- Quarantine zones appear as containment barriers

**Corruption Spread:**
- Corrupt systems have diseased patches (dark veins spreading)
- Corruption connections show influence spreading between systems
- Cleansing efforts appear as purifying light pushing back darkness

**Rebellion/Unrest:**
- Unstable systems flicker between faction colors
- Unrest intensity = flicker speed/violence
- Revolutionary connections link rebel cells

### Tech Level Distribution

**Tech Tier Heatmap:**
- Color systems by tech level
  - Brown: Tribal (Tier 1)
  - Bronze: Classical (Tier 2)
  - Silver: Arcane (Tier 3)
  - Gold: Industrial (Tier 4)
  - Platinum: Atomic (Tier 5)
  - Diamond: Information (Tier 6)
  - Rainbow: Transcendent (Tier 7)

**Tech Spread:**
- Research sharing connections (show tech transfer between allies)
- Tech espionage (stolen tech appears as dark tendrils)

---

## Technical Implementation

### Graph Rendering

**Force-Directed Graph Layout:**

```csharp
// Standard force-directed graph algorithm
// Nodes repel each other, connections attract connected nodes
void UpdateGraphLayout()
{
    // Repulsion force (Coulomb's Law)
    for each pair of nodes (i, j):
        Vector3 direction = nodes[i].position - nodes[j].position;
        float distance = direction.magnitude;
        float repulsion = (k_repulsion * nodes[i].mass * nodes[j].mass) / (distance * distance);
        nodes[i].velocity += direction.normalized * repulsion;
        nodes[j].velocity -= direction.normalized * repulsion;

    // Attraction force (Hooke's Law - spring)
    for each connection (i, j):
        Vector3 direction = nodes[j].position - nodes[i].position;
        float distance = direction.magnitude;
        float attraction = k_attraction * (distance - idealLength);
        nodes[i].velocity += direction.normalized * attraction;
        nodes[j].velocity -= direction.normalized * attraction;

    // Apply velocity
    for each node:
        node.position += node.velocity * Time.deltaTime;
        node.velocity *= dampening; // Friction
}
```

**Performance Optimization:**
- **Spatial Hashing:** Only calculate forces for nearby nodes
- **LOD:** Simplify distant clusters (merge small nodes)
- **Static Caching:** Stable systems don't recalculate layout every frame
- **GPU Acceleration:** Use compute shaders for force calculations

### Shader-Based Cloud Morphing

**System Cloud Material Properties:**

```csharp
Material cloudMaterial = new Material(immateriumCloudShader);

// Faction and Alignment (Cloud Color)
cloudMaterial.SetColor("_FactionColor", faction.primaryColor);
cloudMaterial.SetFloat("_GoodEvilAxis", system.AverageAlignment); // -100 to +100 (affects cloud color tint)
cloudMaterial.SetFloat("_Corruption", system.CorruptionLevel); // 0 to 100 (dark patches in cloud)

// Ethics (Cloud Shape & Pattern)
cloudMaterial.SetFloat("_MaterialSpiritualAxis", system.EthicsAxis); // -100 to +100 (geometric vs organic cloud shape)

// Warfare (Cloud Turbulence & Scars)
cloudMaterial.SetFloat("_WarDamage", system.WarScars); // 0 to 100 (dark streaks, turbulent patches)
cloudMaterial.SetFloat("_ActiveWar", system.IsUnderAttack ? 1.0f : 0.0f); // Chaotic cloud turbulence

// Tech/Psionic (Cloud Structure)
cloudMaterial.SetFloat("_TechPsionicAxis", system.TechPsionicAxis); // -100 to +100 (cloud structure complexity)

// Vitality (Cloud Density & Brightness)
cloudMaterial.SetFloat("_Happiness", system.AverageHappiness); // 0 to 100 (cloud brightness)
cloudMaterial.SetFloat("_Morale", system.AverageMorale); // 0 to 100 (cloud density)
cloudMaterial.SetFloat("_Population", system.TotalPopulation); // Determines cloud size & density

// Xenophobia (Cloud Connections)
cloudMaterial.SetFloat("_XenophobiaAxis", system.XenophobiaAxis); // -100 to +100 (cloud connection density)

// Memories (Cloud Patterns)
cloudMaterial.SetTexture("_MemoryPattern", GenerateMemoryTexture(system.Memories)); // Historical patterns in cloud
cloudMaterial.SetFloat("_MemoryIntensity", system.MemoryStrength); // How strongly memories affect cloud
```

**Cloud Rendering Techniques:**
- **Volumetric Clouds:** Use volumetric rendering for 3D cloud formations
- **Particle Systems:** Particle-based clouds for dynamic, flowing effects
- **Noise-Based:** Procedural noise for organic cloud shapes and patterns
- **LOD:** Simplify cloud detail at distance (fewer particles, lower resolution)

**Cloud Tendril/Connection Material Properties:**

```csharp
Material cloudTendrilMaterial = new Material(immateriumCloudTendrilShader);

// Connection Type (Cloud Tendril Type)
cloudTendrilMaterial.SetInt("_ConnectionType", connectionType); // Trade, Alliance, Psionic, Data, Rivalry, War
cloudTendrilMaterial.SetFloat("_Strength", connectionStrength); // 0 to 1 (cloud tendril density)
cloudTendrilMaterial.SetFloat("_FlowSpeed", flowSpeed); // Cloud particle animation speed
cloudTendrilMaterial.SetColor("_FlowColor", connectionColor); // Cloud tendril color

// Bidirectional flow (Cloud Particle Flow)
cloudTendrilMaterial.SetVector("_FlowDirection", new Vector2(sourceToTarget, targetToSource));

// Cloud Properties
cloudTendrilMaterial.SetFloat("_CloudDensity", connectionStrength); // How dense/thick the cloud tendril is
cloudTendrilMaterial.SetFloat("_Turbulence", isWarConnection ? 1.0f : 0.0f); // Turbulent cloud for war connections
```

### Cloud Particle Systems

**System Cloud Particles (Per System Cloud):**

```csharp
// Good-aligned system cloud
ParticleSystem.EmissionModule emission = cloudParticleSystem.emission;
emission.rateOverTime = happiness * 10; // More cloud particles when happy
cloudParticleSystem.startColor = factionColor + alignmentTint;
cloudParticleSystem.startSpeed = techLevel * 0.5f;

// Cloud particle shape based on ethics
if (ethics > 50) // Spiritual
    cloudParticleSystem.shape.shapeType = ParticleSystemShapeType.Sphere; // Organic cloud particles
else // Materialist
    cloudParticleSystem.shape.shapeType = ParticleSystemShapeType.Box; // Geometric cloud particles

// Cloud density based on population and morale
cloudParticleSystem.startSize = Mathf.Lerp(0.1f, 1.0f, cloudDensity);
cloudParticleSystem.startLifetime = Mathf.Lerp(1.0f, 5.0f, cloudDensity);
```

**Cloud Tendril Particles (Flow Animation Through Cloud Connections):**

```csharp
// Trade route cloud flow
void AnimateTradeFlowThroughCloud(CloudConnection connection)
{
    // Spawn resource particles flowing through cloud tendril
    float t = (Time.time * flowSpeed) % 1.0f;
    Vector3 position = Vector3.Lerp(connection.sourceCloud.position, connection.targetCloud.position, t);

    // Spawn cloud particle with resource icon
    GameObject cloudParticle = Instantiate(cloudParticlePrefab, position, Quaternion.identity);
    cloudParticle.GetComponent<ParticleSystemRenderer>().material.SetTexture("_ResourceIcon", GetResourceSprite(connection.resourceType));
    
    // Cloud particle follows tendril path
    cloudParticle.transform.position = SampleCloudTendrilPath(connection, t);

    // Destroy when reaching target cloud
    if (t > 0.95f)
        Destroy(cloudParticle, 0.5f);
}

// Sample cloud tendril path (curved, flowing path through cloud)
Vector3 SampleCloudTendrilPath(CloudConnection connection, float t)
{
    // Use Bezier curve or noise-based path for organic cloud flow
    Vector3 start = connection.sourceCloud.position;
    Vector3 end = connection.targetCloud.position;
    Vector3 control = (start + end) / 2.0f + Random.insideUnitSphere * cloudTurbulence;
    
    return BezierCurve(start, control, end, t);
}
```

### Camera and Zoom

**Zoom Levels:**

**Level 1 - System View (Closest):**
- See individual planets orbiting star
- Colony details visible
- Normal physical view (not immaterium)

**Level 2 - Local Cluster View:**
- See 5-20 nearby systems
- Connections between systems visible
- Partial immaterium overlay (50% transparency)

**Level 3 - Regional View:**
- See multiple clusters (50-100 systems)
- Clusters aggregate into larger nodes
- Full immaterium overlay (100%)

**Level 4 - Galactic View (Farthest):**
- See entire galaxy
- Clusters become primary nodes (systems hidden inside)
- Maximum abstraction, pure immaterium

**Auto-Transition:**
```csharp
void UpdateCameraView()
{
    float zoomLevel = camera.orthographicSize; // Or distance for perspective

    if (zoomLevel < 10f)
        currentView = ViewMode.SystemView;
    else if (zoomLevel < 100f)
        currentView = ViewMode.LocalCluster;
    else if (zoomLevel < 1000f)
        currentView = ViewMode.Regional;
    else
        currentView = ViewMode.Galactic;

    // Smoothly blend immaterium overlay
    immateriumnOverlayAlpha = Mathf.Clamp01((zoomLevel - 10f) / 90f);
}
```

---

## Gameplay Integration

### Strategic Planning

**Empire Management:**
- **At a Glance:** Instantly see which systems are happy/unhappy
- **Resource Planning:** Identify resource-rich systems, plan trade routes
- **Threat Assessment:** Spot enemy buildups, border tensions, incoming invasions
- **Expansion Targets:** Find unclaimed systems, evaluate habitability

**Diplomatic Interface:**
- **Alliance Visualization:** See who's allied with whom (web of connections)
- **Influence Zones:** Faction spheres of influence shown as color gradients
- **Diplomatic Opportunities:** Isolated factions = potential alliance targets

**Military Command:**
- **War Zones:** Red highlights show active combat
- **Strategic Importance:** Key systems identified by connection count
- **Supply Lines:** Identify critical trade routes to disrupt/protect
- **Fleet Positioning:** See where your fleets are relative to threats

### Player Feedback

**Victory Conditions:**

**Domination Victory:**
- Immaterium shows single faction color dominating entire galaxy
- All nodes glow in your faction's theme
- Celebration particles, triumphant music

**Diplomatic Victory:**
- Galaxy unified by alliance connections (all blue/white)
- Peaceful, harmonious animation
- Cultural exchange particles flowing everywhere

**Technological Victory:**
- All systems glow with transcendent (Tier 7) brilliance
- Rainbow aurora effects, reality-bending visuals
- Ascension aesthetic

**Spiritual Victory (Psionic Ascension):**
- Galaxy transforms into pure psionic energy
- Purple/gold ethereal beauty
- Nodes dissolve into consciousness

**Defeat State:**
- Player's faction nodes dim, flicker, die
- Connections severed one by one
- Final system goes dark, game over

### Event Visualization

**Random Events:**

**Plague Outbreak:**
- Infected system pulses with sickly green
- Disease connections spread to neighboring systems
- Quarantine response shows containment barriers

**Economic Boom:**
- System glows brighter, trade connections thicken
- Gold particles burst from node
- Prosperity spreads to trade partners

**Rebellion:**
- System flickers between loyalist and rebel colors
- Civil war connections (internal conflict)
- Resolution: System settles into new faction color or is crushed

**First Contact:**
- New faction appears suddenly (unknown node color)
- Tentative connections form (diplomatic first contact)
- Can become alliance or rivalry based on choices

**Galactic Crisis:**
- **Endgame Threat:** Massive external threat appears
  - Example: Extragalactic invaders, ancient AI awakening, dimensional rift
- Visual: Chaotic, corrupting energy spreading from crisis source
- All factions' connections turn from rivalry to alliance (united against threat)

---

## Data Structures

### Galactic Tree Cloud Component

```csharp
public struct ImmateriumGalacticTreeCloud : IComponentData
{
    // Galaxy-Wide Aggregates (Tree Cloud Properties)
    public float GalacticGoodEvil;           // -100 to +100 (overall tree cloud color)
    public float GalacticCorruption;        // 0 to 100 (dark patches throughout tree cloud)
    public float GalacticMaterialSpiritual; // -100 to +100 (tree cloud shape: geometric vs organic)
    public float GalacticTechPsionic;       // -100 to +100 (tree cloud structure complexity)
    public float GalacticXenophobia;        // -100 to +100 (tree cloud branch density)
    public float GalacticHappiness;         // 0 to 100 (tree cloud brightness)
    public float GalacticMorale;            // 0 to 100 (tree cloud density)

    // Warfare State (Tree Cloud Turbulence)
    public float GalacticWarIntensity;      // 0 to 100 (% of galaxy at war)
    public int ActiveConflicts;             // Number of ongoing wars
    public float TreeCloudTurbulence;       // 0 to 1 (overall turbulence in tree cloud)

    // Population (Tree Cloud Size)
    public long GalacticPopulation;         // Total across all colonies
    public float TreeCloudScale;            // Overall tree cloud size
    public float TreeCloudDensity;          // 0 to 1 (overall cloud density)

    // Dominant Faction (Tree Cloud Color)
    public Entity DominantFaction;         // Faction with most systems
    public float DominanceDegree;           // 0 to 1 (1 = total control)

    // Tree Structure
    public Entity TreeTrunkCloud;          // Galactic core cloud (trunk of tree)
    public FixedList128Bytes<Entity> TreeBranchClouds; // Cluster clouds (major branches)
    public FixedList512Bytes<Entity> TreeTwigClouds;  // System clouds (twigs/leaves)
}
```

### System Cloud Component

```csharp
public struct ImmateriumSystemCloud : IComponentData
{
    public Entity SystemEntity;              // Reference to star system

    // Immaterium Theme (Visual Aggregate)
    // Aggregated Stats (from all colonies/individuals in system)
    public float AverageGoodEvil;            // -100 to +100 (cloud color tint)
    public float AverageCorruption;          // 0 to 100 (dark patches in cloud)
    public float AverageMaterialSpiritual;   // -100 to +100 (cloud shape: geometric vs organic)
    public float AverageTechPsionic;         // -100 to +100 (cloud structure complexity)
    public float AverageXenophobia;          // -100 to +100 (cloud connection density)
    public float AverageHappiness;           // 0 to 100 (cloud brightness)
    public float AverageMorale;              // 0 to 100 (cloud density)
    
    // Memories (Affect Cloud Patterns)
    public FixedList128Bytes<MemoryPattern> Memories; // Historical events embedded in cloud
    public float MemoryStrength;             // 0 to 100 (how strongly memories affect cloud)

    // Faction Control
    public Entity OwningFaction;             // Primary faction (if single-owner)
    public FixedList64Bytes<FactionControl> FactionShares; // Multi-faction control

    // Population and Cloud Scale
    public long TotalPopulation;             // All colonies in system
    public float CloudRadius;                // Visual cloud size
    public float CloudDensity;               // 0 to 1 (cloud opacity/density)

    // Warfare (Cloud Turbulence & Scars)
    public float WarDamage;                  // 0 to 100 (dark streaks, scars in cloud)
    public bool IsUnderAttack;               // Active combat flag (chaotic cloud turbulence)

    // Tree Position (Galactic Tree Cloud Structure)
    public float3 TreePosition;              // Position in galactic tree cloud (not spatial position)
    public float3 TreeVelocity;              // For tree cloud layout algorithm
    public Entity ParentClusterCloud;        // Which cluster cloud this system cloud belongs to
}

public struct MemoryPattern
{
    public MemoryType Type;                  // War, Achievement, Cultural, Crisis, etc.
    public long Timestamp;                   // When memory occurred
    public float Intensity;                  // 0 to 1 (how strongly it affects cloud)
    public float3 CloudPatternPosition;      // Where in cloud this memory manifests
}

public struct FactionControl
{
    public Entity Faction;
    public float ControlPercentage;          // 0 to 1 (sum of all factions = 1.0)
}
```

### Cloud Connection Component (Cloud Tendril)

```csharp
public struct ImmateriumCloudConnection : IBufferElementData
{
    public Entity SourceSystemCloud;         // Source system cloud
    public Entity TargetSystemCloud;          // Target system cloud

    // Connection Type (Cloud Tendril Type)
    public ConnectionType Type;              // Trade, Alliance, Psionic, Data, Rivalry, War
    public float Strength;                   // 0 to 1 (cloud tendril density)

    // Visual Properties (Cloud Tendril Properties)
    public Color CloudTendrilColor;          // Cloud tendril color
    public float CloudDensity;                // 0 to 1 (how dense/thick the cloud tendril is)
    public float FlowSpeed;                  // Cloud particle animation speed
    public float Turbulence;                 // 0 to 1 (cloud turbulence, higher for war connections)

    // Bidirectional Flow (Cloud Particle Flow)
    public float SourceToTargetFlow;         // 0 to 1 (trade volume, data, etc. flowing through cloud)
    public float TargetToSourceFlow;         // 0 to 1
    
    // Tree Structure (Galactic Tree Cloud Branch)
    public bool IsTreeBranch;                // True if this connection forms part of galactic tree structure
    public float BranchThickness;             // Thickness of tree branch cloud
}

public enum ConnectionType : byte
{
    Trade = 1,
    Alliance = 2,
    Psionic = 3,
    DataNetwork = 4,
    Rivalry = 5,
    ActiveWarfare = 6
}
```

### Cluster Cloud Component

```csharp
public struct ImmateriumClusterCloud : IComponentData
{
    public FixedString64Bytes ClusterName;   // "Orion Arm", "Galactic Core", etc.
    public FixedList128Bytes<Entity> MemberSystemClouds; // System clouds in this cluster

    // Aggregated Stats (from all member system clouds)
    public float ClusterGoodEvil;            // Aggregated cloud color tint
    public float ClusterCorruption;          // Aggregated dark patches
    public float ClusterMaterialSpiritual;   // Aggregated cloud shape
    public float ClusterTechPsionic;         // Aggregated cloud structure
    public float ClusterXenophobia;          // Aggregated connection density
    public float ClusterHappiness;           // Aggregated cloud brightness

    // Cluster Cloud State
    public long ClusterPopulation;           // Sum of all member systems
    public Entity DominantFaction;           // Faction controlling most systems
    public float ClusterCloudRadius;         // Visual cloud size
    public float ClusterCloudDensity;         // 0 to 1 (aggregated cloud density)

    // Tree Position (Galactic Tree Cloud Structure)
    public float3 ClusterTreePosition;       // Position in galactic tree cloud (branch position)
    public Entity ParentGalacticTree;        // Reference to galactic tree cloud entity
}
```

---

## Example Galactic States

### Example 1: Golden Age (Peaceful Federation)

**Galactic State:**
- **Population:** 500 billion across 200 systems
- **Alignment:** +70 Good, 15 Corruption
- **Ethics:** +60 Spiritual (moderate psionic focus)
- **War Intensity:** 5 (minor border skirmishes only)
- **Xenophobia:** +75 Xenophilic (diverse, open)
- **Happiness:** 85 (prosperous, content)
- **Dominant Faction:** United Federation (controls 60% of galaxy)

**Immaterium Appearance:**
- **Nodes:** Bright blue/gold, glowing warmly, crystalline patterns
- **Connections:** Dense alliance networks (blue), thick trade routes (green/gold)
- **Particles:** Peaceful (stars, butterflies, light wisps)
- **Clusters:** Unified by harmonious connections, few rivalries
- **Overall:** Looks like a healthy neural network, vibrant, alive
- **Sound:** Harmonic chimes, uplifting orchestral music

**Player Feeling:** "This is a thriving civilization. Peace and prosperity."

---

### Example 2: Galaxy at War (Total Conflict)

**Galactic State:**
- **Population:** 300 billion across 180 systems (declining from war deaths)
- **Alignment:** -40 Evil (war atrocities, desperation)
- **Ethics:** -80 Materialist (militarization, war economy)
- **War Intensity:** 95 (almost entire galaxy at war)
- **Xenophobia:** -85 Xenophobic (nationalism, "us vs. them")
- **Happiness:** 20 (miserable, war-torn)
- **Dominant Faction:** None (3 major empires deadlocked)

**Immaterium Appearance:**
- **Nodes:** Dark red/black, cracked, burning, fortress-like
- **Connections:** Violent warfare edges (bright red, crackling), severed alliances
- **Particles:** Explosions, debris, battle effects
- **Clusters:** Isolated, fortified, surrounded by hostile rivals
- **Overall:** Looks like a battlefield, chaotic, unstable
- **Sound:** Battle sounds, alarms, screaming, explosions

**Player Feeling:** "This is hell. The galaxy is tearing itself apart."

---

### Example 3: Transcendent Ascension (Post-Scarcity Utopia)

**Galactic State:**
- **Population:** 1 trillion across 250 systems (including digital consciousnesses)
- **Alignment:** +95 Good (post-scarcity, enlightened)
- **Ethics:** +100 Spiritual (full psionic/digital ascension)
- **War Intensity:** 0 (war abolished centuries ago)
- **Xenophobia:** +90 Xenophilic (unified galactic consciousness)
- **Happiness:** 98 (near-perfect)
- **Tech Level:** Transcendent (Tier 7) across entire galaxy
- **Dominant Faction:** Galactic Collective (unified under voluntary cooperation)

**Immaterium Appearance:**
- **Nodes:** Ethereal, barely physical, reality-bending
- **Connections:** Pure psionic bonds (purple/gold aurora), data networks (cyan streams)
- **Particles:** Reality distortion, dimensional rifts, consciousness wisps
- **Clusters:** Dissolving into unified galactic consciousness
- **Overall:** Looks like ascension to higher plane, godlike
- **Sound:** Transcendent harmonics, cosmic music, enlightenment

**Player Feeling:** "We've become something beyond mortal. We've won existence itself."

---

### Example 4: Dark Age (Post-Apocalypse Recovery)

**Galactic State:**
- **Population:** 5 billion across 50 systems (was 800 billion across 300 systems)
- **Alignment:** -60 Evil (survivor desperation, cannibalism, piracy)
- **Ethics:** -70 Materialist (scavenging old tech, no innovation)
- **War Intensity:** 30 (resource wars, banditry)
- **Xenophobia:** -75 Xenophobic (isolated survivor enclaves)
- **Happiness:** 15 (suffering, barely surviving)
- **Dominant Faction:** None (collapsed empires, warlord territories)

**Immaterium Appearance:**
- **Nodes (Active):** Tiny, flickering, struggling to survive (dim red/gray)
- **Nodes (Dead):** Ghostly husks, transparent, memorials to destroyed systems
- **Connections:** Sparse, fragile, mostly severed
- **Clusters:** Isolated pockets of civilization in sea of dead systems
- **Overall:** Looks post-apocalyptic, haunting, tragic
- **Sound:** Mournful, echoing emptiness, distant distress signals

**Player Feeling:** "The galaxy died. Can we rebuild from the ashes?"

---

## Cross-Project Integration

### Godgame (World Tree)

**Not Applicable:**
- Godgame uses physical World Tree on each planet
- No galactic structure (planets are separate worlds)
- Tree is literal organism, not abstract graph

### Space4X (Immaterium)

**Full Implementation:**
- Abstract psychic/spiritual overlay
- Graph-based visualization
- Scales from system → cluster → galaxy
- Toggle between physical and immaterium views

**Shared Principles with World Tree:**
- Both aggregate individual stats → visual representation
- Both morph based on alignment, ethics, warfare, tech
- Both serve as "emotional hook" (players care about visual state)
- Both provide strategic overview at a glance

**Key Differences:**
- **World Tree:** Single organism, grows/withers, can die/resurrect
- **Immaterium:** Network graph, connections central, distributed consciousness

---

## Open Questions

1. **Player Control over Immaterium:** Can players manipulate the immaterium directly (psionic abilities, reality manipulation)?
   - **Option A:** Pure visualization, no interaction
   - **Option B:** Late-game psionic factions can influence graph (bend connections, corrupt nodes)
   - **Recommendation:** Option A initially, Option B for endgame psionic ascension

2. **AI Opponent Visibility:** Can AI see the same immaterium view, or is it player-exclusive?
   - **Option A:** Player-only (UI tool, not "real" in-universe)
   - **Option B:** Psionic factions see it, materialist factions don't
   - **Recommendation:** Option A (keep it as player strategic tool)

3. **Historical Playback:** Can players "rewind" immaterium to see galaxy state 10/100/1000 years ago?
   - **Option A:** No, only current state
   - **Option B:** Yes, timeline slider shows historical evolution
   - **Recommendation:** Option B (incredible storytelling tool, educational)

4. **Connection Auto-Generation:** Should connections auto-generate based on proximity and stats, or player-defined?
   - **Option A:** Auto (trade routes form based on economy, alliances based on diplomacy)
   - **Option B:** Player defines what to visualize (toggle trade/alliance/war layers)
   - **Recommendation:** Hybrid (auto-generate all, player toggles visibility)

5. **Immaterium as Gameplay Mechanic:** Should immaterium state affect actual gameplay?
   - **Option A:** Pure cosmetic (visual only, no game effect)
   - **Option B:** Immaterium state provides buffs/debuffs (e.g., high happiness galaxy = +10% research)
   - **Recommendation:** Option A (keep it UI tool, avoid circular dependency)

6. **Extragalactic View:** What if player controls multiple galaxies (endgame)?
   - **Option A:** One immaterium per galaxy, switch between them
   - **Option B:** Super-immaterium showing connections between galaxies
   - **Recommendation:** Option B (ultimate strategic overview)

---

## Implementation Roadmap

### Phase 1: Basic Graph Visualization (MVP)
- **Goal:** System nodes + trade/alliance connections
- **Features:**
  - Force-directed graph layout
  - Node size based on population
  - Trade route connections (green lines)
  - Alliance connections (blue lines)
- **Success Metric:** Players can see empire structure at galactic scale

### Phase 2: Faction and Alignment
- **Goal:** Nodes reflect faction themes and alignments
- **Features:**
  - Faction color coding
  - Good/evil alignment color tints
  - Happiness affects node brightness
- **Success Metric:** Players can identify faction territories by color

### Phase 3: Full Morphing System
- **Goal:** All variables affect visual appearance
- **Features:**
  - Ethics axis (materialist/spiritual visual styles)
  - Tech/Psionic axis (geometric vs. organic patterns)
  - Warfare (battle scars, active combat visualization)
  - Xenophobia (connection density)
- **Success Metric:** Immaterium is unique visual fingerprint of galaxy state

### Phase 4: Connection Types
- **Goal:** Visualize all connection types
- **Features:**
  - Trade routes (resource flow animation)
  - Alliances (stable peaceful connections)
  - Psionic bonds (ethereal purple energy)
  - Data networks (digital streams)
  - Rivalries (orange tension lines)
  - Active warfare (red combat effects)
- **Success Metric:** Players understand relationship dynamics at a glance

### Phase 5: Cluster Aggregation
- **Goal:** Systems aggregate into clusters at high zoom
- **Features:**
  - Clusters form at regional view
  - Cluster stats aggregate from member systems
  - Smooth transition between zoom levels
- **Success Metric:** Performance optimized, thousands of systems manageable

### Phase 6: Event Visualization
- **Goal:** Real-time events show in immaterium
- **Features:**
  - Plague spread, economic booms, rebellions
  - Galactic crises (endgame threats)
  - Victory/defeat animations
- **Success Metric:** Immaterium tells story of galactic events

---

## Related Systems

- **World Tree Visualization System:** [World_Tree_Visualization_System.md](World_Tree_Visualization_System.md) - Godgame equivalent (planet-scale tree)
- **Aggregate Entities and Individual Dynamics:** [Aggregate_Entities_And_Individual_Dynamics.md](Aggregate_Entities_And_Individual_Dynamics.md) - Stat aggregation from individuals
- **Faction System:** (TBD) - Faction definitions, relationships, themes
- **Diplomacy System:** (TBD) - Alliance/rivalry formation, diplomatic actions
- **Trade and Economy:** (TBD) - Trade route generation, resource flow

---

## Design Intent

**What It Should Feel Like:**

**Player Experience:**
- **Awe:** "The galaxy is alive. It has a soul. I can see it."
- **Strategic Clarity:** "Now I understand the entire war at a glance."
- **Emotional Connection:** "My empire's nodes are glowing bright. We're thriving."
- **Historical Appreciation:** "That cluster is scarred from the Great War 200 years ago."
- **Victory Pride:** "The entire galaxy glows in my faction's color. I've won."

**Emotional Hooks:**
- **God's Eye View:** Players feel like cosmic architects viewing creation
- **Living Galaxy:** Immaterium makes the galaxy feel alive, breathing, evolving
- **Strategic Mastery:** Instantly understand complex multi-faction relationships
- **Beauty:** The visualization should be stunning, screenshot-worthy art

**Narrative Emergent:**
- Peaceful alliance network suddenly severed by war → tells story of betrayal
- Dead systems slowly reconnecting → tells story of post-war recovery
- Psionic bonds spreading across galaxy → tells story of spiritual awakening
- Single faction color dominating → tells story of empire ascension

**The Immaterium is the Galaxy's Soul Made Visible. The Player Becomes God, Watching Creation Unfold.**

---

**Last Updated:** 2025-12-03
**Status:** Concept Captured - Ready for Technical Design
