# Resource Production & Crafting System

**Type:** Core Mechanic
**Status:** `<Draft>` - Core vision captured, implementation pending
**Version:** Concept v1.0
**Dependencies:** Economy Spine & Principles (Chunk 0), Business & Assets, Individual Progression (Expertise, Lessons), Education & Tutoring, Guild System, Circle Progression (Enchanting, Runesmith)
**Last Updated:** 2025-01-27

> **Foundation:** This system (Chunk 3 - Businesses & Production) must comply with the principles defined in [`Economy_Spine_And_Principles.md`](Economy_Spine_And_Principles.md). All resources must exist in inventories, and production must operate by moving/transforming inventory entries.  
> **Specification:** See [`Businesses_And_Production_Chunk3.md`](Businesses_And_Production_Chunk3.md) for the complete Chunk 3 design specification and "done" checklist.  
> **Resource Layer:** Production systems use the inventory and ItemSpec patterns defined in [`Resources_And_Mass_Chunk2.md`](Resources_And_Mass_Chunk2.md). All production consumes inputs from and produces outputs to inventories.

---

## Overview

**Resource Production & Crafting** is the economic chain system that transforms raw extracted materials into refined products and complex end goods. Every item has **Quality, Rarity, and Tech Tier** that propagate through the production chain, influenced by material purity, artisan expertise, lessons learned, and business cooperation.

**Key Features:**
- **Extracted Resources:** Raw materials with **purity** only (ore, wood, stone, herbs)
- **Produced Materials:** Refined resources with **quality, rarity, tech tier** (metal ingots, cut lumber, cut stone)
- **End Products:** Complex crafted goods with **quality, rarity, tech tier** (wagons, weapons, buildings, potions)
- **Quality Propagation:** Low purity materials → low quality products (but high expertise can mitigate)
- **Artisan Influence:** Lessons, expertise, and education improve output quality
- **Business Cooperation:** Specialists can be employed (wage) or partnered (profit share)
- **Cross-Village Trade:** Outsource materials with transportation costs
- **Component Assembly:** Complex products require multiple components (wagon = wheels + axles + nuts/bolts)

**For Narrative:** Medieval supply chains (iron ore → ingots → swords), artisan guilds controlling quality standards (Blacksmiths' Guild rejects subpar work), legendary crafters creating artifacts (master blacksmith with 30 years experience forges legendary blade), cross-village trade routes (timber from northern forests shipped to southern smithies).

---

## Resource Categories

### 1. Extracted Resources (Raw Materials)

**Extracted resources** are harvested directly from nature or mines. They have **purity only** (not quality/rarity/tech tier).

**Purity (0-100%):**
```
Purity: Percentage of usable material in raw extraction

Example:
  100 raw ore with 69% average purity
  → Yields 69 metal ingots (after refining)
  → 31 ore discarded as slag/waste

Purity Sources:
  - Resource node quality (mine richness, forest maturity)
  - Extraction expertise (skilled miners find better veins)
  - Extraction tools (quality pickaxes, saws)
  - Random variance (±10% per extraction)

High Purity (80-100%): Rich veins, mature forests, expert extraction
Medium Purity (50-79%): Average deposits, standard extraction
Low Purity (20-49%): Poor veins, young forests, unskilled extraction
Trash Purity (0-19%): Depleted mines, wasteful extraction
```

**Extracted Resource Types:**

**Ores (Mining):**
```
Iron Ore:
  - Purity: 50-90%
  - Yields: Iron ingots (1:1 purity ratio)
  - Extracted By: Miner (Mining business)
  - Tools: Pickaxe, mine shaft

Copper Ore:
  - Purity: 40-85%
  - Yields: Copper ingots
  - Used For: Bronze (copper + tin), tools, coins

Gold Ore:
  - Purity: 20-60% (rare, hard to find pure veins)
  - Yields: Gold ingots
  - Used For: Jewelry, coins, enchantments

Silver Ore, Tin Ore, Mithril Ore (rare):
  - Similar purity mechanics
  - Higher rarity = lower purity averages
```

**Wood (Forestry):**
```
Raw Wood (Logs):
  - Purity: 60-95% (wood quality, rot, knots)
  - Yields: Lumber (cut planks)
  - Extracted By: Forester (Logging business)
  - Tools: Axe, saw
  - Factors: Tree age (older = higher purity), tree species

Example:
  100 logs with 75% average purity
  → Yields 75 lumber planks (after sawmill processing)
  → 25 logs discarded (bark, rot, unusable wood)
```

**Stone (Quarrying):**
```
Raw Stone (Quarry Blocks):
  - Purity: 70-95% (fissures, impurities)
  - Yields: Cut stone (building blocks)
  - Extracted By: Quarryman (Quarry business)
  - Tools: Chisel, hammer, explosives (high tech)

Example:
  100 raw stone blocks with 80% purity
  → Yields 80 cut stone blocks (after stonecutter processing)
  → 20 blocks discarded (cracks, unusable)
```

**Herbs (Herbalism):**
```
Raw Herbs (Vegetation):
  - Purity: 40-90% (freshness, contamination, proper harvesting)
  - Yields: Processed herbs (medicinal compounds)
  - Extracted By: Herbalist (Herbalist business)
  - Tools: Sickle, drying racks, preservation methods
  - Factors: Harvest timing (seasonal peak), preservation (storage quality)

Example:
  100 raw herbs with 60% purity
  → Yields 60 processed herbs (dried, cleaned)
  → 40 herbs discarded (wilted, contaminated, improperly harvested)
```

**Grain (Farming):**
```
Raw Grain (Wheat, Barley, Rye):
  - Purity: 50-95% (chaff, moisture, pest damage)
  - Yields: Flour (after milling)
  - Extracted By: Farmer (Farm business)
  - Refined By: Miller (Mill business)

Example:
  100 grain with 70% purity
  → Yields 70 flour (after milling)
  → 30 grain discarded (chaff, damaged kernels)
```

---

### 2. Produced Materials (Refined Resources)

**Produced materials** are refined from extracted resources. They gain **Quality, Rarity, Tech Tier** based on:
- **Input purity** (extracted resource quality)
- **Artisan expertise** (refiner's lessons, experience)
- **Business quality** (equipment, facility)
- **Tech tier** (refining techniques)

**Quality (1-100):**
```
Quality: Craftsmanship level of refined product

Quality Calculation:
  BaseQuality = InputPurity × 0.6
  ArtisanBonus = (ArtisanExpertise / 5) + (LessonQuality / 10)
  BusinessBonus = (BusinessQuality / 10)
  TechTierBonus = TechTier × 5

  FinalQuality = BaseQuality + ArtisanBonus + BusinessBonus + TechTierBonus
  FinalQuality = clamp(FinalQuality, 1, 100)

Example (Master Blacksmith refining iron):
  Input: Iron ore, 80% purity
  Artisan: Expertise Tier 7 (70/5 = 14), Lesson Quality 85 (85/10 = 8.5)
  Business: Forge Quality 75 (75/10 = 7.5)
  Tech Tier: 3 (×5 = 15)

  → BaseQuality: 80 × 0.6 = 48
  → ArtisanBonus: 14 + 8.5 = 22.5
  → BusinessBonus: 7.5
  → TechTierBonus: 15
  → FinalQuality: 48 + 22.5 + 7.5 + 15 = 93 (Legendary quality iron ingot)

Example (Novice Blacksmith refining iron):
  Input: Iron ore, 60% purity
  Artisan: Expertise Tier 2 (40/5 = 8), Lesson Quality 40 (40/10 = 4)
  Business: Forge Quality 50 (50/10 = 5)
  Tech Tier: 1 (×5 = 5)

  → BaseQuality: 60 × 0.6 = 36
  → ArtisanBonus: 8 + 4 = 12
  → BusinessBonus: 5
  → TechTierBonus: 5
  → FinalQuality: 36 + 12 + 5 + 5 = 58 (Common quality iron ingot)
```

**Rarity (Tier System):**
```
Rarity: How rare/valuable the material is

Rarity Tiers:
  - Common: Standard materials (iron, oak, limestone)
  - Uncommon: Better materials (steel alloy, mahogany, marble)
  - Rare: Specialized materials (mithril, ebony, enchanted stone)
  - Epic: Legendary materials (adamantite, dragonbone, celestial stone)
  - Legendary: Mythical materials (orichalcum, world tree wood, deity-blessed stone)

Rarity Determination:
  1. Base material rarity (iron = Common, mithril = Rare)
  2. Refinement process (legendary lesson + high expertise can upgrade rarity by 1 tier)
  3. Random chance (critical success during refinement: 5% chance +1 rarity tier)

Example:
  Iron Ore (Common) → Iron Ingot (Common)
  Mithril Ore (Rare) → Mithril Ingot (Rare)

  Special case:
    Master blacksmith with Legendary lesson (Quality 95+)
    → 10% chance to upgrade Common → Uncommon (steel alloy instead of iron)
    → "The blacksmith's technique transforms ordinary iron into superior steel"
```

**Tech Tier (0-10):**
```
Tech Tier: Technological advancement level

Tech Tier Sources:
  - Artisan lessons (high-tier lessons unlock advanced techniques)
  - Business equipment (advanced forges, precision tools)
  - Village tech level (technological progression)
  - Research/innovation (guilds unlock new techniques)

Tech Tier 1 (Bronze Age):
  - Basic smelting, simple forging
  - Copper, bronze, basic iron

Tech Tier 2-3 (Iron Age):
  - Advanced forging, steel production
  - Iron, steel, basic alloys

Tech Tier 4-5 (Medieval):
  - Master forging, precision smithing
  - High-quality steel, specialized alloys
  - Damascus steel techniques

Tech Tier 6-7 (Renaissance):
  - Advanced metallurgy, precision engineering
  - Superior alloys, precision tools
  - Clockwork mechanisms

Tech Tier 8-10 (Magitech):
  - Enchanted forging, magical refinement
  - Mithril, adamantite, orichalcum
  - Rune-infused materials, divine metals
```

**Rarity vs Tech Tier Equivalence:**
```
IMPORTANT: For equippable items (weapons, armor, shields), rarity tiers and tech tiers have equivalent base stat relationships:

Base Stat Equivalence:
  - Legendary Tech Tier X ≈ Uncommon/Rare Tech Tier X+3 (base stats only)
  - Epic Tech Tier X ≈ Uncommon/Rare Tech Tier X+2 (base stats only)
  - Rare Tech Tier X ≈ Uncommon Tech Tier X+1 (base stats only)

Example:
  Legendary Longsword (Tech Tier 3, Quality 90):
    → Base Damage: 30 (equivalent to Rare Tech Tier 6 longsword)

  Rare Longsword (Tech Tier 6, Quality 90):
    → Base Damage: 30 (same as Legendary Tech Tier 3)

KEY DIFFERENCE: Unique Affixes & Attributes
  Higher rarity items have exclusive affixes ONLY available to their rarity tier:
    - Legendary items: Game-changing unique effects
    - Epic items: Powerful rare effects
    - Rare items: Specialized effects
    - Uncommon items: Minor bonuses
    - Common items: No special affixes

Example Comparison:
  Legendary "Soulreaver" Longsword (Tech Tier 3, Quality 95):
    - Base Damage: 30 (same as Rare Tech Tier 6)
    - Legendary Affixes:
      → "Soulbound": Returns to wielder when dropped (cannot be stolen)
      → "Demon Slayer": +100% damage vs demons/undead
      → "Immortal Edge": Never loses durability
      → "Vorpal Strike": 5% chance instant-kill on non-boss enemies
      → "Lifesteal": Restore 10% of damage dealt as HP
    - Rarity: Legendary
    - Worth: 5000+ currency

  Rare "Masterwork Steel Blade" (Tech Tier 6, Quality 90):
    - Base Damage: 30 (same base as Legendary Tech Tier 3)
    - Rare Affixes:
      → "+15% Critical Hit Chance"
      → "+20% Armor Penetration"
    - Rarity: Rare
    - Worth: 400 currency

Conclusion:
  → Legendary Tech Tier 3 sword is FAR superior to Rare Tech Tier 6 sword
  → Base stats are equivalent, but legendary affixes make it irreplaceable
  → A veteran warrior would choose Legendary Tech Tier 3 over Rare Tech Tier 6
  → Tech advancement allows Common/Uncommon items to match old Legendary base stats
  → But Legendary affixes remain unique and cannot be replicated
```

**Unique Affixes by Rarity Tier:**
```
Common Rarity:
  - No special affixes
  - Base stats only
  - Mass-produced, affordable

Uncommon Rarity:
  - Minor stat bonuses (+5-10% single stat)
  - Examples:
    → "+8% Attack Speed"
    → "+10 Health"
    → "+5% Critical Chance"
  - Craftable by skilled artisans

Rare Rarity:
  - Specialized combat bonuses (10-20% single stat or dual stat)
  - Examples:
    → "+15% Critical Hit Chance"
    → "+20% Armor Penetration"
    → "+15% Damage vs Beasts"
    → "Bleeding Strike: 20% chance to apply bleed (5 damage/round for 3 rounds)"
    → "+10% Dodge"
  - Craftable by master artisans with rare materials

Epic Rarity:
  - Powerful multi-stat bonuses or unique mechanics
  - Examples:
    → "+20% Critical Damage AND +10% Critical Chance"
    → "Flaming Weapon: +15 fire damage, ignites enemies"
    → "Mana Leech: Restore 5 mana per hit"
    → "Phase Strike: Attacks ignore 30% defense"
    → "+30 Health, +15% Stamina regeneration"
    → "Knockback: 25% chance to knock enemies back 3m"
  - Requires master artisan + epic materials + enchantments
  - Cannot be mass-produced

Legendary Rarity:
  - Game-changing unique effects, named items with lore
  - Examples:
    → "Soulbound: Returns to wielder, cannot be stolen"
    → "Demon Slayer: ×2 damage vs demons/undead"
    → "Immortal Edge: Never loses durability"
    → "Vorpal Strike: 5% instant-kill on non-boss"
    → "Lifesteal: Restore 10% damage as HP"
    → "Elemental Fury: Randomly applies fire/ice/lightning/shadow on hit"
    → "Time Dilation: 10% chance to attack twice in one action"
    → "Soul Harvest: Killing blows permanently +1 damage (stacks infinitely)"
    → "Realm Shift: Once per day, teleport to any previously visited location"
    → "Undying: Resurrect once per week when killed (return at 30% HP)"
  - Named items with unique history/lore
  - Examples: "Excalibur", "Frostmourne", "Durendal", "Gram"
  - Cannot be replicated, one-of-a-kind artifacts
  - Worth: 10,000+ currency (often priceless)

Affix Acquisition:
  - Common/Uncommon: Crafted through artisan skill + quality materials
  - Rare: Crafted by masters with specialized lessons (Rare+ lessons)
  - Epic: Requires enchantments (Fire/Shadow/Light Circle mastery) + epic materials
  - Legendary:
    → Cannot be crafted intentionally
    → Divine intervention (god blesses item during creation)
    → Ancient artifacts (found, not crafted)
    → Critical success during epic crafting (0.1% chance)
    → Unique quest rewards (slay demon lord, reward = legendary weapon)
```

**Produced Material Types:**

**Metal Ingots (Refined by Blacksmith/Refiner):**
```
Iron Ingot:
  - Input: Iron ore (purity 50-90%)
  - Quality: 1-100 (based on formula above)
  - Rarity: Common (base), Uncommon (steel, if high expertise)
  - Tech Tier: 1-5 (basic iron to master steel)
  - Used For: Weapons, armor, tools, nails, hinges

Copper Ingot, Gold Ingot, Silver Ingot:
  - Similar mechanics
  - Copper used for coins, tools, bronze
  - Gold/Silver used for jewelry, enchantments

Mithril Ingot (Rare):
  - Input: Mithril ore (purity 30-70%, rare deposits)
  - Quality: 40-100 (requires high expertise to refine properly)
  - Rarity: Rare (base), Epic (master refinement)
  - Tech Tier: 6-8 (requires advanced magical forging)
  - Used For: Legendary weapons, enchanted armor
```

**Lumber (Refined by Sawyer/Sawmill):**
```
Lumber (Cut Planks):
  - Input: Raw logs (purity 60-95%)
  - Quality: 1-100 (sawing precision, drying technique)
  - Rarity: Common (oak, pine), Uncommon (mahogany), Rare (ebony, ironwood)
  - Tech Tier: 1-4 (basic sawing to precision carpentry)
  - Used For: Buildings, wagons, furniture, bows

Example:
  100 oak logs (75% purity) → 75 oak lumber planks (Quality 60, Common rarity)
  100 ebony logs (65% purity, rare wood) → 65 ebony lumber planks (Quality 75, Rare rarity)
```

**Cut Stone (Refined by Stonecutter):**
```
Cut Stone (Building Blocks):
  - Input: Raw stone (purity 70-95%)
  - Quality: 1-100 (cutting precision, shaping)
  - Rarity: Common (limestone), Uncommon (granite), Rare (marble), Epic (obsidian)
  - Tech Tier: 1-5 (basic cutting to precision masonry)
  - Used For: Buildings, walls, monuments

Example:
  100 limestone blocks (80% purity) → 80 cut limestone (Quality 65, Common)
  100 marble blocks (85% purity) → 85 cut marble (Quality 80, Rare)
```

**Flour (Refined by Miller):**
```
Flour (Milled Grain):
  - Input: Raw grain (purity 50-95%)
  - Quality: 1-100 (milling fineness, sifting)
  - Rarity: Common (wheat flour), Uncommon (refined white flour)
  - Tech Tier: 1-3 (hand mill to water mill to windmill)
  - Used For: Bread, pastries, food production

Example:
  100 grain (70% purity) → 70 flour (Quality 55, Common)
```

**Processed Herbs (Refined by Herbalist):**
```
Processed Herbs (Medicinal Compounds):
  - Input: Raw herbs (purity 40-90%)
  - Quality: 1-100 (drying technique, preservation, purity extraction)
  - Rarity: Common (lavender, mint), Uncommon (mandrake), Rare (dragon's breath flower)
  - Tech Tier: 1-6 (basic drying to alchemical extraction)
  - Used For: Potions, medicine, poisons

Example:
  100 mandrake roots (60% purity) → 60 processed mandrake (Quality 70, Uncommon)
```

---

### 3. End Products (Crafted Goods)

**End products** are complex items crafted from produced materials and/or other components. They inherit quality from materials and gain additional quality from artisan expertise.

**Quality Propagation Formula:**
```
End Product Quality Calculation:

BaseQuality = Average(ComponentQualities) × 0.7
ArtisanBonus = (ArtisanExpertise / 5) + (LessonQuality / 10)
BusinessBonus = (BusinessQuality / 10)
TechTierBonus = TechTier × 5

FinalQuality = BaseQuality + ArtisanBonus + BusinessBonus + TechTierBonus
FinalQuality = clamp(FinalQuality, 1, 100)

Rarity Inheritance:
  FinalRarity = Highest(ComponentRarities)
  → If all components Common, product is Common
  → If one component is Rare, product is Rare (minimum)
  → Master artisan can upgrade rarity by 1 tier (5% critical success)

Tech Tier Inheritance:
  FinalTechTier = Min(ArtisanTechTier, Average(ComponentTechTiers))
  → Artisan cannot exceed their own tech tier knowledge
  → Components limit final tech tier
```

**Durability Inheritance (Component Weakest Link):**

Items inherit **durability** from their components based on importance weights. The product is only as durable as its weakest critical component.

```
Durability Inheritance Formula:

ComponentDurability = Quality × MaterialDurabilityMultiplier × TechTierBonus

MaterialDurabilityMultiplier:
  Iron: 1.0× baseline
  Steel: 1.5× (stronger, more resistant to wear)
  Mithril: 3.0× (magical resistance to damage)
  Wood (oak): 0.6× (softer, wears faster)
  Leather: 0.4× (weak point in armor/weapons)

TechTierBonus:
  Tech Tier × 100 durability points

WeightedDurability Calculation:
  FinalDurability = Σ(ComponentDurability × ImportanceWeight)

  ImportanceWeight depends on component's criticality:
    Critical (1.0): Components essential to function (axles, wheels, blade)
    Major (0.7): Important but not essential (wagon bed, grip)
    Minor (0.3): Cosmetic or secondary (pommel, decorations)

Example 1: Longsword Durability

Components:
  - Iron blade (Quality 70, Durability 70 × 1.0 × 300 = 21,000) × CRITICAL (1.0 weight)
  - Leather grip (Quality 60, Durability 60 × 0.4 × 200 = 4,800) × MAJOR (0.7 weight)
  - Wood pommel (Quality 50, Durability 50 × 0.6 × 200 = 6,000) × MINOR (0.3 weight)

Calculation:
  WeightedDurability = (21,000 × 1.0) + (4,800 × 0.7) + (6,000 × 0.3)
  = 21,000 + 3,360 + 1,800
  = 26,160 durability points

  BUT: Leather grip is weakest critical component (only 4,800 base durability)

  Effective Durability = min(WeightedAverage, LowestCriticalComponent × 1.5)
  = min(26,160, 4,800 × 1.5)
  = min(26,160, 7,200)
  = 7,200 durability points

Result: Sword lasts 7,200 uses before grip fails (requires repair)

Failure Mode:
  After 7,200 uses, leather grip wears out → Sword unusable until repaired
  Repair cost: Replace grip component only (new leather + blacksmith labor)
  Blade still intact (21,000 durability), can be reused

Example 2: Wagon Durability (Equal Critical Components)

Components:
  - Wheels (Quality 65) × 4 → CRITICAL (1.0 weight EACH)
    Durability: 65 × 1.0 × 300 = 19,500 each
  - Axles (Quality 70) × 2 → CRITICAL (1.0 weight EACH)
    Durability: 70 × 1.0 × 300 = 21,000 each
  - Wagon bed (Quality 60) × 1 → MAJOR (0.7 weight)
    Durability: 60 × 0.6 × 200 = 7,200
  - Nuts/bolts (Quality 55) × 50 → CRITICAL (aggregate 1.0 weight)
    Durability: 55 × 1.0 × 200 = 11,000
  - Leather straps (Quality 50) × 3 → MINOR (0.3 weight)
    Durability: 50 × 0.4 × 200 = 4,000

Critical Component Analysis:
  Wheels: 19,500 (weakest critical component)
  Axles: 21,000
  Nuts/bolts: 11,000

FinalDurability = Lowest critical component durability
  = 19,500 (wheels wear out first)

Result: Wagon lasts 19,500 km traveled before wheels fail

Failure Cascade:
  - At 19,500 km: First wheel fails → Wagon unusable
  - Repair: Replace 1 wheel (or all 4 preventatively)
  - At ~78,000 km: Axles would fail (if wheels maintained)
  - At ~55,000 km: Nuts/bolts would fail (if other components maintained)

Strategic Implications:
  - Wheels are weak point (wood + iron rim, constant ground contact)
  - Wainwright should use higher quality wheels to improve wagon lifespan
  - Preventative maintenance: Replace wheels every 15,000 km (before failure)

Example 3: Plate Armor Durability

Components:
  - Chest plate (Quality 90, Steel) × 1 → CRITICAL (1.0)
    Durability: 90 × 1.5 × 500 = 67,500
  - Leg plates (Quality 85, Steel) × 2 → CRITICAL (1.0 each)
    Durability: 85 × 1.5 × 500 = 63,750 each
  - Arm plates (Quality 88, Steel) × 2 → MAJOR (0.7 each)
    Durability: 88 × 1.5 × 500 = 66,000 each
  - Helmet (Quality 92, Steel) × 1 → CRITICAL (1.0)
    Durability: 92 × 1.5 × 500 = 69,000
  - Leather straps (Quality 70, Leather) × 10 → CRITICAL (aggregate 1.0)
    Durability: 70 × 0.4 × 400 = 11,200

Critical Component Analysis:
  Leather straps: 11,200 (weakest critical component!)
  Leg plates: 63,750
  Chest plate: 67,500
  Helmet: 69,000

FinalDurability = 11,200 (leather straps fail first)

Result: Armor lasts 11,200 combat encounters before straps break

Irony: Legendary steel plates (60,000+ durability) useless when leather straps fail
Solution: Use mithril thread for straps (Durability: 70 × 3.0 × 400 = 84,000)
  → Armor lasts 63,750 encounters (leg plates now weakest)

Design Principle:
  "A chain is only as strong as its weakest link"
  → Artisans must balance ALL component qualities
  → Cheap components sabotage expensive components
  → Legendary items require legendary components throughout
```

**End Product Types:**

**Weapons (Crafted by Blacksmith/Weaponsmith):**
```
Longsword:
  Components:
    - Iron ingot (Quality 70, Common, Tech Tier 3) × 3
    - Leather (Quality 60, Common, Tech Tier 2) × 1 (grip)
    - Wood (Quality 50, Common, Tech Tier 2) × 1 (pommel)

  Artisan:
    - Weaponsmith Expertise Tier 6 (high skill)
    - Lesson Quality 80 (master bladesmithing lesson)
    - Business Quality 70 (good forge)
    - Tech Tier 4 (advanced forging techniques)

  Calculation:
    → BaseQuality: (70 + 70 + 70 + 60 + 50) / 5 = 64
    → BaseQuality × 0.7 = 44.8
    → ArtisanBonus: (60/5) + (80/10) = 12 + 8 = 20
    → BusinessBonus: 70/10 = 7
    → TechTierBonus: 4 × 5 = 20
    → FinalQuality: 44.8 + 20 + 7 + 20 = 91.8 ≈ 92 (Legendary longsword)

  Rarity: Common (all components Common, no critical success)
  Tech Tier: 3 (min of artisan Tech Tier 4, component average 2.6)

  Stats:
    - Base Damage: 25 (longsword baseline)
    - Quality Bonus: +18% damage (Quality 92 → +18%)
    - Final Damage: 25 × 1.18 = 29.5 ≈ 30 damage
    - Accuracy Bonus: +10 (Quality 92 → +10 accuracy)
    - Durability: 500 uses (Quality 92 → high durability)

Example (Poor Quality Sword):
  Components:
    - Iron ingot (Quality 40, Common, Tech Tier 1) × 3
    - Leather (Quality 30, Common, Tech Tier 1) × 1
    - Wood (Quality 35, Common, Tech Tier 1) × 1

  Artisan:
    - Novice blacksmith, Expertise Tier 2
    - Lesson Quality 45 (basic blacksmithing)
    - Business Quality 40 (shabby forge)
    - Tech Tier 1

  Calculation:
    → BaseQuality: (40 + 40 + 40 + 30 + 35) / 5 = 37
    → BaseQuality × 0.7 = 25.9
    → ArtisanBonus: (40/5) + (45/10) = 8 + 4.5 = 12.5
    → BusinessBonus: 40/10 = 4
    → TechTierBonus: 1 × 5 = 5
    → FinalQuality: 25.9 + 12.5 + 4 + 5 = 47.4 ≈ 47 (Poor quality sword)

  Stats:
    - Base Damage: 25
    - Quality Bonus: +5% damage (Quality 47 → +5%)
    - Final Damage: 25 × 1.05 = 26 damage
    - Accuracy Bonus: +2
    - Durability: 150 uses (breaks easily)
```

**Rarity Tier Comparisons (Longsword Examples):**
```
Demonstrating rarity vs tech tier equivalence with affixes:

1. Common "Iron Longsword" (Tech Tier 3, Quality 85):
  Components: Iron ingot (Quality 80, Common, Tech Tier 3) × 3
  Artisan: Master weaponsmith (Expertise Tier 7)

  Stats:
    - Base Damage: 25 (longsword baseline Tech Tier 3)
    - Quality Bonus: +17% damage (Quality 85 → +17%)
    - Final Damage: 29
    - Accuracy: +8
    - Durability: 400 uses
    - Affixes: None (Common rarity)
    - Worth: 80 currency

2. Uncommon "Steel Longsword" (Tech Tier 4, Quality 90):
  Components: Steel ingot (Quality 85, Uncommon, Tech Tier 4) × 3
  Artisan: Master weaponsmith (Expertise Tier 8)

  Stats:
    - Base Damage: 27 (Tech Tier 4 = +2 damage over Tech Tier 3)
    - Quality Bonus: +20% damage (Quality 90 → +20%)
    - Final Damage: 32
    - Accuracy: +10
    - Durability: 600 uses
    - Affixes:
      → "+8% Attack Speed" (Uncommon affix)
    - Worth: 150 currency

3. Rare "Masterwork Steel Blade" (Tech Tier 6, Quality 90):
  Components: Masterwork steel (Quality 90, Rare, Tech Tier 6) × 3
  Artisan: Grandmaster weaponsmith (Expertise Tier 9)

  Stats:
    - Base Damage: 30 (Tech Tier 6 = +5 damage over Tech Tier 3)
    - Quality Bonus: +20% damage (Quality 90 → +20%)
    - Final Damage: 36
    - Accuracy: +12
    - Durability: 900 uses
    - Affixes:
      → "+15% Critical Hit Chance" (Rare affix)
      → "+20% Armor Penetration" (Rare affix)
    - Worth: 400 currency

4. Epic "Flamebrand" (Tech Tier 5, Quality 95):
  Components: Enchanted steel (Quality 90, Epic, Tech Tier 5) × 3
  Enchantment: Fire rune (Quality 85, Rare, Tech Tier 7)
  Artisan: Grandmaster weaponsmith + Master enchanter

  Stats:
    - Base Damage: 28 (Tech Tier 5 ≈ Rare Tech Tier 7 equivalent)
    - Quality Bonus: +25% damage (Quality 95 → +25%)
    - Final Damage: 35
    - Accuracy: +15
    - Durability: 1200 uses
    - Affixes:
      → "Flaming Weapon: +15 fire damage, ignites enemies (5 fire damage/round for 3 rounds)" (Epic affix)
      → "Mana Leech: Restore 3 mana per hit" (Epic affix)
      → "+12% Critical Chance AND +15% Critical Damage" (Epic dual affix)
    - Total Damage: 35 physical + 15 fire = 50 damage + ignite
    - Worth: 2000 currency

5. Legendary "Soulreaver" (Tech Tier 3, Quality 100):
  Components: Divine iron (Quality 95, Legendary, Tech Tier 3) × 3
  Blessing: God of War blessed during forging (0.1% chance critical success)
  Artisan: Legendary weaponsmith (Expertise Tier 10)

  Stats:
    - Base Damage: 30 (Tech Tier 3, but Legendary quality = Rare Tech Tier 6 equivalent)
    - Quality Bonus: +30% damage (Quality 100 → +30%)
    - Final Damage: 39
    - Accuracy: +20
    - Durability: ∞ (Immortal Edge affix)
    - Affixes (Legendary only):
      → "Soulbound: Returns to wielder when dropped/stolen" (Legendary affix)
      → "Demon Slayer: ×2 damage vs demons/undead" (Legendary affix)
      → "Immortal Edge: Never loses durability" (Legendary affix)
      → "Vorpal Strike: 5% instant-kill on non-boss enemies" (Legendary affix)
      → "Lifesteal: Restore 10% of damage dealt as HP" (Legendary affix)
    - Named Item: "Soulreaver, Blade of the Fallen King"
    - Lore: Forged by King Aldric's royal weaponsmith, blessed by the God of War
    - Total Damage vs Demons: 39 × 2 = 78 damage + lifesteal + instant-kill chance
    - Worth: 12,000 currency (often priceless, inheritance heirloom)

Comparison Analysis:
  - Legendary "Soulreaver" (Tech Tier 3) vs Rare "Masterwork Steel" (Tech Tier 6):
    → Base damage: 30 vs 30 (equivalent due to rarity vs tech tier relationship)
    → Final damage: 39 vs 36 (Legendary quality 100 vs Rare quality 90)
    → Legendary has: Soulbound, Demon Slayer, Immortal Edge, Vorpal Strike, Lifesteal
    → Rare has: +15% crit, +20% armor pen
    → Legendary is FAR superior despite lower tech tier
    → No veteran warrior would choose Rare Tech Tier 6 over Legendary Tech Tier 3

  - Epic "Flamebrand" (Tech Tier 5) vs Rare "Masterwork Steel" (Tech Tier 6):
    → Base damage: 28 vs 30 (Epic Tech Tier 5 ≈ Rare Tech Tier 7, so Epic is slightly behind)
    → Epic has: Fire damage, ignite, mana leech, dual crit affix
    → Rare has: Crit chance, armor pen
    → Epic total damage: 50 + ignite (far exceeds Rare 36 damage)
    → Epic is superior due to unique affixes

Key Takeaway:
  → Rarity determines unique affixes, not just base stats
  → Legendary Tech Tier 3 > Epic Tech Tier 5 > Rare Tech Tier 6 (despite lower tech tier)
  → Tech advancement allows Commons to match old Legendaries in BASE stats
  → But Legendary affixes (Soulbound, Demon Slayer, Immortal Edge) cannot be replicated
  → Heirloom weapons remain relevant for centuries due to unique affixes
```

**Armor (Crafted by Armorer):**
```
Plate Armor:
  Components:
    - Steel ingot (Quality 85, Uncommon, Tech Tier 4) × 15
    - Leather (Quality 70, Common, Tech Tier 3) × 5 (straps, padding)
    - Iron rivets (Quality 60, Common, Tech Tier 2) × 50 (small components)

  Artisan:
    - Master armorer, Expertise Tier 8
    - Lesson Quality 90 (legendary armorsmithing)
    - Business Quality 80 (masterwork forge)
    - Tech Tier 5

  Calculation:
    → ComponentAverage: (85×15 + 70×5 + 60×50) / 70 = (1275 + 350 + 3000) / 70 = 66.1
    → BaseQuality × 0.7 = 46.3
    → ArtisanBonus: (80/5) + (90/10) = 16 + 9 = 25
    → BusinessBonus: 80/10 = 8
    → TechTierBonus: 5 × 5 = 25
    → FinalQuality: 46.3 + 25 + 8 + 25 = 104.3 → clamp to 100 (MAX quality plate armor)

  Rarity: Uncommon (steel component is Uncommon)
  Tech Tier: 4 (component average ~3.5, artisan Tech Tier 5)

  Stats:
    - Defense Bonus: +50 (plate baseline)
    - Quality Bonus: +50% defense (Quality 100 → max bonus)
    - Final Defense: +75
    - Damage Reduction: 80% (plate maximum)
    - Durability: 2000 uses (nearly indestructible)
```

**Wagons (Crafted by Wainwright/Cartwright):**
```
Wagon:
  Components:
    - Wheels (Quality 65, Common, Tech Tier 3) × 4
      → Wheels made from: Lumber × 8, Iron rim × 1 each
    - Axles (Quality 70, Common, Tech Tier 3) × 2
      → Axles made from: Iron ingot × 2 each
    - Wagon bed (Quality 60, Common, Tech Tier 2) × 1
      → Bed made from: Lumber × 20
    - Nuts/Bolts/Screws (Quality 55, Common, Tech Tier 2) × 50
      → Made from: Iron ingot, blacksmith precision work
    - Leather (Quality 50, Common, Tech Tier 2) × 3 (harness straps)

  Artisan:
    - Wainwright, Expertise Tier 5
    - Lesson Quality 70 (wagon construction)
    - Business Quality 65 (wagon workshop)
    - Tech Tier 3

  Calculation:
    → ComponentAverage: (65×4 + 70×2 + 60 + 55×50 + 50×3) / 60 ≈ 60.2
    → BaseQuality × 0.7 = 42.1
    → ArtisanBonus: (50/5) + (70/10) = 10 + 7 = 17
    → BusinessBonus: 65/10 = 6.5
    → TechTierBonus: 3 × 5 = 15
    → FinalQuality: 42.1 + 17 + 6.5 + 15 = 80.6 ≈ 81 (High quality wagon)

  Rarity: Common
  Tech Tier: 3

  Stats:
    - Cargo Capacity: 500 kg (baseline)
    - Quality Bonus: +25% capacity (Quality 81 → +25%)
    - Final Capacity: 625 kg
    - Durability: 800 km travel (before repairs)
    - Speed: Standard (no quality bonus to speed, only capacity/durability)
```

**Buildings (Crafted by Builder/Carpenter):**
```
Stone House:
  Components:
    - Cut stone (Quality 75, Common, Tech Tier 3) × 500
    - Lumber (Quality 70, Common, Tech Tier 3) × 100 (roof beams)
    - Iron nails (Quality 60, Common, Tech Tier 2) × 500
    - Thatch/Tile (Quality 65, Common, Tech Tier 2) × 200 (roofing)

  Artisan:
    - Master builder, Expertise Tier 7
    - Lesson Quality 85 (advanced masonry)
    - Business Quality 75 (builder's crew, tools)
    - Tech Tier 4

  Calculation:
    → ComponentAverage: (75×500 + 70×100 + 60×500 + 65×200) / 1300 ≈ 69.6
    → BaseQuality × 0.7 = 48.7
    → ArtisanBonus: (70/5) + (85/10) = 14 + 8.5 = 22.5
    → BusinessBonus: 75/10 = 7.5
    → TechTierBonus: 4 × 5 = 20
    → FinalQuality: 48.7 + 22.5 + 7.5 + 20 = 98.7 ≈ 99 (Masterwork house)

  Rarity: Common
  Tech Tier: 3

  Stats:
    - Housing Capacity: 6 people (baseline stone house)
    - Quality Bonus: +50% durability (Quality 99 → near-max bonus)
    - Durability: 150 years (before major repairs)
    - Weather Resistance: 95% (nearly weatherproof)
    - Fire Resistance: 80% (stone construction)
```

**Potions (Crafted by Apothecary/Alchemist):**
```
Healing Potion:
  Components:
    - Processed mandrake (Quality 80, Uncommon, Tech Tier 4) × 1
    - Processed lavender (Quality 70, Common, Tech Tier 3) × 1
    - Distilled water (Quality 90, Common, Tech Tier 5) × 1 (purified by alchemist)
    - Glass vial (Quality 65, Common, Tech Tier 3) × 1

  Artisan:
    - Master alchemist, Expertise Tier 8
    - Lesson Quality 95 (legendary alchemy)
    - Business Quality 85 (alchemical laboratory)
    - Tech Tier 6

  Calculation:
    → ComponentAverage: (80 + 70 + 90 + 65) / 4 = 76.25
    → BaseQuality × 0.7 = 53.4
    → ArtisanBonus: (80/5) + (95/10) = 16 + 9.5 = 25.5
    → BusinessBonus: 85/10 = 8.5
    → TechTierBonus: 6 × 5 = 30
    → FinalQuality: 53.4 + 25.5 + 8.5 + 30 = 117.4 → clamp to 100 (MAX quality potion)

  Rarity: Uncommon (mandrake is Uncommon)
  Tech Tier: 4 (component average ~3.75, artisan Tech Tier 6)

  Stats:
    - Healing: 50 HP (baseline healing potion)
    - Quality Bonus: +100% healing (Quality 100 → double effect)
    - Final Healing: 100 HP restored
    - Side Effects: None (Quality 100 → no negative effects)
    - Duration: Instant
```

**Enchanted Items (Multi-Artisan Cooperation):**
```
Enchanted Longsword (+Fire Damage):
  Base Item:
    - Longsword (Quality 92, Common, Tech Tier 3)
      → Crafted by weaponsmith

  Enchantment Component:
    - Fire Rune (Quality 85, Rare, Tech Tier 7)
      → Crafted by runesmith (Fire Circle mastery)
    - Mana Crystal (Quality 90, Epic, Tech Tier 8)
      → Crafted by enchanter (Arcane Circle mastery)
    - Enchanting ritual (Quality 95, Legendary lesson)
      → Performed by enchanter

  Business Cooperation:
    - Weaponsmith crafts base sword (100 currency wage)
    - Enchanter adds fire rune + mana crystal (50% profit share on final sale)
    - OR: Enchanter employed by weaponsmith (500 currency wage)

  Calculation:
    → BaseQuality: (92 + 85 + 90) / 3 = 89
    → BaseQuality × 0.7 = 62.3
    → ArtisanBonus (Enchanter): (85/5) + (95/10) = 17 + 9.5 = 26.5
    → BusinessBonus: 85/10 = 8.5
    → TechTierBonus: 7 × 5 = 35
    → FinalQuality: 62.3 + 26.5 + 8.5 + 35 = 132.3 → clamp to 100 (MAX quality enchanted sword)

  Rarity: Epic (mana crystal is Epic)
  Tech Tier: 7 (enchantment tech tier)

  Stats:
    - Base Damage: 30 (from Quality 92 longsword)
    - Fire Damage: +20 (enchantment baseline)
    - Quality Bonus: +100% fire damage (Quality 100 enchantment)
    - Final Fire Damage: +40
    - Total Damage: 30 + 40 = 70 damage
    - Special: Ignites enemies (10 fire damage/round for 3 rounds)
    - Durability: 1000 uses (enchanted weapons last longer)
```

---

## Business Cooperation

### Employment (Wage)

**Specialist employed by primary business for fixed wage.**

```
Example: Enchanter Employed by Blacksmith

Blacksmith Business:
  - Crafts weapons (longswords, axes, maces)
  - Wants to offer enchanted weapons (premium product)
  - Hires enchanter for 500 currency/month wage

Enchanter:
  - Fire Circle mastery (Expertise Tier 7)
  - Adds fire enchantments to weapons
  - Works exclusively for blacksmith (no outside clients)
  - Wage: 500 currency/month (guaranteed income)

Process:
  1. Blacksmith crafts longsword (Quality 85)
  2. Enchanter adds fire rune (Quality 90)
  3. Final product: Enchanted Longsword (Quality 95, sells for 800 currency)
  4. Blacksmith keeps 100% profit (800 - 200 materials - 500 wage = 100 profit)
  5. Enchanter receives guaranteed 500 currency wage

Advantages (Employment):
  - Enchanter: Guaranteed steady income, no business risk
  - Blacksmith: Full control, predictable costs, keeps all profit

Disadvantages (Employment):
  - Enchanter: No profit sharing (misses out on high-value sales)
  - Blacksmith: Fixed wage cost even if sales are low
```

### Partnership (Profit Share)

**Specialist partners with primary business, sharing profits.**

```
Example: Jewelcrafter Partnered with Weaponsmith

Weaponsmith Business:
  - Crafts weapons
  - Wants to offer jeweled weapons (ornate, high-value)
  - Partners with jewelcrafter (50/50 profit split)

Jewelcrafter:
  - Expertise in gemcutting, jewelry
  - Adds gemstones to weapons (ruby pommel, sapphire crossguard)
  - Works with weaponsmith on commission basis
  - Profit Share: 50% of final sale

Process:
  1. Weaponsmith crafts longsword (Quality 80, 150 currency materials)
  2. Jewelcrafter adds ruby pommel (Quality 85, 200 currency ruby)
  3. Final product: Jeweled Longsword (Quality 90, sells for 1000 currency)
  4. Total costs: 150 (sword materials) + 200 (ruby) = 350 currency
  5. Profit: 1000 - 350 = 650 currency
  6. Weaponsmith receives 325 currency (50%)
  7. Jewelcrafter receives 325 currency (50%)

Advantages (Partnership):
  - Both share risk and reward
  - Jewelcrafter earns more than wage if sales are good
  - Weaponsmith doesn't pay wage if no sales

Disadvantages (Partnership):
  - Jewelcrafter has no guaranteed income
  - Weaponsmith shares profit (less total income)
  - Requires trust and contract management
```

### Outsourcing (One-Time Contract)

**Primary business purchases completed components from specialist.**

```
Example: Wainwright Outsources Wheels

Wainwright Business:
  - Crafts wagons
  - Needs wheels (complex component)
  - Outsources wheel production to wheelwright

Wheelwright:
  - Specialist in wheel construction
  - Crafts wheels (Quality 75) for 30 currency each
  - Sells to wainwright (one-time sale)

Process:
  1. Wainwright orders 4 wheels (120 currency total)
  2. Wheelwright crafts wheels (60 currency materials, 30 labor, 30 profit per wheel)
  3. Wainwright receives wheels (Quality 75)
  4. Wainwright assembles wagon (wheels + axles + bed + hardware)
  5. Final wagon (Quality 80, sells for 500 currency)

Advantages (Outsourcing):
  - Wainwright doesn't need wheel expertise (specialization)
  - Wheelwright focuses on one product (efficiency)
  - One-time transaction (no long-term commitment)

Disadvantages (Outsourcing):
  - Higher component cost (wheelwright's profit margin)
  - Quality dependency (wainwright can't control wheel quality)
  - Transportation costs if cross-village outsourcing
```

---

## Cross-Village Trade

**Businesses can outsource materials from other villages if local supply is unavailable or inferior.**

### Transportation Costs

```
Transportation via Stagecoach:
  - Cost: 5 currency per 100 kg per 100 km
  - Speed: 40 km/day (8 hours travel)
  - Capacity: 500 kg max

Example:
  Blacksmith in Village A needs mithril ingots (not available locally)
  → Village B has mithril mine and refinery (200 km away)
  → Orders 100 kg mithril ingots (1000 currency)
  → Transportation cost: 5 × (100/100) × (200/100) = 5 × 1 × 2 = 10 currency
  → Total cost: 1010 currency
  → Delivery time: 5 days (200 km ÷ 40 km/day)

Transportation via Caravan:
  - Cost: 3 currency per 100 kg per 100 km (cheaper, slower)
  - Speed: 25 km/day (more stops, trade route)
  - Capacity: 2000 kg max (larger wagons)

Example:
  Wainwright in Village A needs 500 kg lumber (oak unavailable locally)
  → Village C has oak forests (300 km away)
  → Orders 500 kg oak lumber (200 currency)
  → Transportation cost: 3 × (500/100) × (300/100) = 3 × 5 × 3 = 45 currency
  → Total cost: 245 currency
  → Delivery time: 12 days (300 km ÷ 25 km/day)
```

### Trade Willingness

```
Trade Requirements:
  - Source village must be willing to export (not at war, not embargoed)
  - Destination village must be willing to import (trade agreements)
  - Materials must be available (surplus production)
  - Transportation route must be safe (no bandits, war zones)

Trade Modifiers:
  - Allied villages: -20% transportation cost
  - Neutral villages: Standard cost
  - Rival villages: +50% transportation cost (tariffs)
  - War: No trade (embargo)

Example:
  Village A (Human kingdom) wants iron from Village B (Dwarven hold)
  → Villages are allied (trade agreement)
  → Transportation cost: -20%
  → Regular cost: 10 currency, Allied cost: 8 currency

  Village A wants iron from Village D (Orc tribe)
  → Villages are rivals (border skirmishes)
  → Transportation cost: +50%
  → Regular cost: 10 currency, Rival cost: 15 currency
```

---

## Quality Propagation Examples

### Example 1: Legendary Sword (High Input Quality)

```
Objective: Craft legendary longsword

Step 1: Extract Iron Ore
  - Mine: Rich vein (purity 90%)
  - Miner: Expertise Tier 6 (skilled)
  - Tools: Quality pickaxe (+5% purity)
  - Result: 100 iron ore, 95% average purity

Step 2: Refine Iron Ingots
  - Input: 100 iron ore, 95% purity → yields 95 iron ingots
  - Blacksmith: Expertise Tier 8 (master)
  - Lessons: Legendary refining lesson (Quality 95)
  - Forge: Quality 85 (masterwork forge)
  - Tech Tier: 5 (advanced forging)
  - Result: 95 iron ingots, Quality 98, Uncommon rarity (steel upgrade), Tech Tier 5

Step 3: Craft Longsword
  - Input: 3 steel ingots (Quality 98, Uncommon, Tech Tier 5)
  - Weaponsmith: Expertise Tier 9 (grandmaster)
  - Lessons: Legendary bladesmithing (Quality 100)
  - Workshop: Quality 90 (legendary forge)
  - Tech Tier: 6 (masterwork techniques)

  Calculation:
    → BaseQuality: 98 × 0.7 = 68.6
    → ArtisanBonus: (90/5) + (100/10) = 18 + 10 = 28
    → BusinessBonus: 90/10 = 9
    → TechTierBonus: 6 × 5 = 30
    → FinalQuality: 68.6 + 28 + 9 + 30 = 135.6 → clamp to 100 (MAX)

  Result: Longsword (Quality 100, Uncommon rarity, Tech Tier 5)
  Stats:
    - Damage: 25 × 2.0 = 50 (double damage from Quality 100)
    - Accuracy: +20
    - Durability: 5000 uses (legendary durability)
    - Name: "Moonshadow Blade" (legendary items get custom names)
```

### Example 2: Poor Quality Wagon (Low Input Quality)

```
Objective: Craft wagon with subpar materials

Step 1: Extract Wood
  - Forest: Young trees (purity 55%)
  - Forester: Expertise Tier 2 (novice)
  - Tools: Basic axe (no bonus)
  - Result: 100 logs, 55% average purity

Step 2: Refine Lumber
  - Input: 100 logs, 55% purity → yields 55 lumber planks
  - Sawyer: Expertise Tier 2 (novice)
  - Lessons: Basic sawing (Quality 40)
  - Sawmill: Quality 45 (shabby)
  - Tech Tier: 1
  - Result: 55 lumber planks, Quality 35, Common rarity, Tech Tier 1

Step 3: Extract Iron Ore
  - Mine: Depleted vein (purity 40%)
  - Miner: Expertise Tier 1 (unskilled)
  - Result: 100 ore, 40% purity

Step 4: Refine Iron Ingots
  - Input: 100 ore, 40% purity → yields 40 iron ingots
  - Blacksmith: Expertise Tier 1 (apprentice)
  - Lessons: Basic smithing (Quality 35)
  - Forge: Quality 40 (poor)
  - Tech Tier: 1
  - Result: 40 iron ingots, Quality 30, Common, Tech Tier 1

Step 5: Craft Wheels (by Wheelwright)
  - Input: Lumber (Quality 35) × 8, Iron rim (Quality 30) × 1 each
  - Wheelwright: Expertise Tier 3
  - Lessons: Basic wheelmaking (Quality 50)
  - Workshop: Quality 50
  - Tech Tier: 2
  - Result: 4 wheels, Quality 45, Common, Tech Tier 2

Step 6: Craft Axles (by Blacksmith)
  - Input: Iron ingots (Quality 30) × 2 each
  - Blacksmith: Expertise Tier 1
  - Result: 2 axles, Quality 35, Common, Tech Tier 1

Step 7: Assemble Wagon (by Wainwright)
  - Input: Wheels (Q 45) × 4, Axles (Q 35) × 2, Lumber bed (Q 35), Hardware (Q 30)
  - Wainwright: Expertise Tier 3
  - Lessons: Basic wagon construction (Quality 55)
  - Workshop: Quality 50
  - Tech Tier: 2

  Calculation:
    → ComponentAverage: (45×4 + 35×2 + 35 + 30) / 8 ≈ 38.75
    → BaseQuality × 0.7 = 27.1
    → ArtisanBonus: (30/5) + (55/10) = 6 + 5.5 = 11.5
    → BusinessBonus: 50/10 = 5
    → TechTierBonus: 2 × 5 = 10
    → FinalQuality: 27.1 + 11.5 + 5 + 10 = 53.6 ≈ 54 (Poor wagon)

  Result: Wagon (Quality 54, Common, Tech Tier 2)
  Stats:
    - Cargo Capacity: 500 kg (baseline)
    - Quality Bonus: +8% capacity (Quality 54 → +8%)
    - Final Capacity: 540 kg
    - Durability: 300 km (breaks down frequently, needs repairs)
```

**Key Takeaway:** Low purity extraction → low quality materials → poor end product, even with moderate artisan skill.

---

## DOTS Components

```csharp
using Unity.Entities;
using Unity.Collections;

namespace Godgame.Economy
{
    /// <summary>
    /// Extracted resource component (raw materials with purity only).
    /// </summary>
    public struct ExtractedResource : IComponentData
    {
        public ResourceType Type;               // Ore, Wood, Stone, Herbs, Grain
        public byte Purity;                     // 0-100% (yield percentage)
        public ushort Quantity;                 // Amount extracted (kg, units)
        public Entity ExtractorBusiness;        // Mine, Logging, Quarry, Herbalist, Farm
    }

    public enum ResourceType : byte
    {
        // Ores
        IronOre = 0,
        CopperOre = 1,
        GoldOre = 2,
        SilverOre = 3,
        TinOre = 4,
        MithrilOre = 5,        // Rare

        // Wood
        RawWood = 10,

        // Stone
        RawStone = 20,

        // Herbs
        RawHerbs = 30,

        // Grain
        RawGrain = 40,
    }

    /// <summary>
    /// Produced material component (refined resources with quality/rarity/tech tier).
    /// </summary>
    public struct ProducedMaterial : IComponentData
    {
        public MaterialType Type;               // Ingots, Lumber, Cut Stone, Processed Herbs, Flour
        public byte Quality;                    // 1-100
        public Rarity Rarity;                   // Common, Uncommon, Rare, Epic, Legendary
        public byte TechTier;                   // 0-10
        public ushort Quantity;                 // Amount produced
        public Entity ProducerBusiness;         // Blacksmith, Sawmill, Stonecutter, Herbalist, Mill
    }

    public enum MaterialType : byte
    {
        // Metal Ingots
        IronIngot = 0,
        SteelIngot = 1,
        CopperIngot = 2,
        GoldIngot = 3,
        SilverIngot = 4,
        MithrilIngot = 5,       // Rare

        // Lumber
        Lumber = 10,

        // Stone
        CutStone = 20,

        // Processed Herbs
        ProcessedHerbs = 30,

        // Flour
        Flour = 40,
    }

    public enum Rarity : byte
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    /// <summary>
    /// End product component (complex crafted goods).
    /// </summary>
    public struct EndProduct : IComponentData
    {
        public ProductType Type;                // Weapon, Armor, Wagon, Building, Potion
        public FixedString64Bytes Name;         // "Longsword", "Plate Armor", "Healing Potion"
        public byte Quality;                    // 1-100
        public Rarity Rarity;                   // Common, Uncommon, Rare, Epic, Legendary
        public byte TechTier;                   // 0-10
        public Entity CrafterBusiness;          // Weaponsmith, Armorer, Wainwright, Builder, Alchemist

        // Product-specific stats (stored as blob asset or separate components)
        // For weapons: Damage, Accuracy, Durability
        // For armor: Defense, Damage Reduction, Durability
        // For wagons: Capacity, Durability
        // For potions: Healing, Duration, Side Effects
    }

    public enum ProductType : byte
    {
        // Weapons
        Longsword = 0,
        Greatsword = 1,
        Dagger = 2,
        Mace = 3,
        Bow = 4,
        Crossbow = 5,

        // Armor
        LeatherArmor = 10,
        ChainmailArmor = 11,
        PlateArmor = 12,

        // Shields
        BasicShield = 20,
        TowerShield = 21,

        // Wagons
        Wagon = 30,

        // Buildings (constructed, not crafted items)
        StoneHouse = 40,
        WoodenHouse = 41,

        // Potions
        HealingPotion = 50,
        ManaPotion = 51,
        Poison = 52,

        // Enchanted Items (weapons/armor with enchantments)
        EnchantedWeapon = 60,
        EnchantedArmor = 61,
    }

    /// <summary>
    /// Component for items (weapons, armor, etc.) with detailed stats.
    /// </summary>
    public struct WeaponStats : IComponentData
    {
        public byte BaseDamage;                 // 10-50
        public byte QualityBonus;               // 0-100% (from quality)
        public byte AccuracyBonus;              // 0-20
        public ushort Durability;               // Uses before breaking (50-5000)
        public ushort CurrentDurability;        // Current uses remaining
        public DamageType DamageType;           // Slashing, Piercing, Blunt, Magic
    }

    public enum DamageType : byte
    {
        Slashing = 0,
        Piercing = 1,
        Blunt = 2,
        Magic = 3,
    }

    public struct ArmorStats : IComponentData
    {
        public byte DefenseBonus;               // 5-75
        public byte DamageReduction;            // 20-80%
        public ushort Durability;               // Uses before breaking
        public ushort CurrentDurability;
        public byte StaminaPenalty;             // -1 to -3 rounds
    }

    public struct WagonStats : IComponentData
    {
        public ushort CargoCapacity;            // kg (500-1000)
        public byte QualityBonus;               // 0-50% capacity bonus
        public ushort Durability;               // km before repairs (300-2000)
        public ushort CurrentDurability;        // km traveled
    }

    public struct PotionStats : IComponentData
    {
        public byte HealingAmount;              // 0-100 HP
        public byte ManaRestoration;            // 0-100 Mana
        public byte Duration;                   // Rounds (0 = instant)
        public bool HasSideEffects;             // Low quality potions have side effects
    }

    /// <summary>
    /// Item affix component - unique effects based on rarity tier.
    /// Uses DynamicBuffer to support multiple affixes.
    /// </summary>
    [InternalBufferCapacity(8)]  // Max 8 affixes per item (Legendary can have 5+)
    public struct ItemAffix : IBufferElementData
    {
        public AffixType Type;                  // Affix type (stat bonus, unique effect)
        public byte Value;                      // Affix magnitude (0-100)
        public FixedString64Bytes Description;  // "Demon Slayer: ×2 damage vs demons"
    }

    public enum AffixType : ushort
    {
        // Common/Uncommon (minor stat bonuses)
        None = 0,
        AttackSpeedBonus = 1,           // +5-10% attack speed
        HealthBonus = 2,                // +10-20 health
        CritChanceBonus = 3,            // +5-10% crit
        AccuracyBonus = 4,              // +5-10 accuracy
        DodgeBonus = 5,                 // +5-10% dodge

        // Rare (specialized bonuses)
        CritChanceMajor = 10,           // +15-20% crit
        ArmorPenetration = 11,          // +20-30% armor pen
        DamageVsBeasts = 12,            // +15-25% vs beasts
        BleedingStrike = 13,            // 20% chance bleed (5 damage/round)
        DodgeMajor = 14,                // +15-20% dodge
        LifestealMinor = 15,            // Restore 5% damage as HP

        // Epic (powerful unique mechanics)
        CritDualBonus = 20,             // +12% crit chance AND +15% crit damage
        FlamingWeapon = 21,             // +15 fire damage, ignite
        ManaLeech = 22,                 // Restore 3-5 mana per hit
        PhaseStrike = 23,               // Ignore 30% defense
        HealthStaminaBonus = 24,        // +30 health, +15% stamina regen
        Knockback = 25,                 // 25% chance knockback 3m
        FrostWeapon = 26,               // +12 frost damage, slow 30%
        LightningWeapon = 27,           // +10 lightning damage, chain to 2 targets

        // Legendary (game-changing effects)
        Soulbound = 100,                // Returns to wielder, cannot be stolen
        DemonSlayer = 101,              // ×2 damage vs demons/undead
        ImmortalEdge = 102,             // Never loses durability
        VorpalStrike = 103,             // 5% instant-kill non-boss
        LifestealMajor = 104,           // Restore 10% damage as HP
        ElementalFury = 105,            // Random fire/ice/lightning/shadow on hit
        TimeDilation = 106,             // 10% attack twice in one action
        SoulHarvest = 107,              // Killing blows +1 damage (infinite stack)
        RealmShift = 108,               // Once per day teleport to visited location
        Undying = 109,                  // Resurrect once per week at 30% HP
        TrueStrike = 110,               // Cannot miss (100% hit chance)
        Banish = 111,                   // 3% chance banish enemy to void (instant kill demons)
        ChainLightning = 112,           // Lightning chains to 5 targets
        Regeneration = 113,             // Restore 5 HP/round when equipped
        SpellReflect = 114,             // 25% chance reflect spells back at caster
        ShadowStep = 115,               // Teleport 10m once per round (5 Focus cost)
    }

    /// <summary>
    /// Business cooperation component - tracks employment/partnership relationships.
    /// </summary>
    public struct BusinessCooperation : IComponentData
    {
        public Entity PrimaryBusiness;          // Main business (blacksmith)
        public Entity SpecialistBusiness;       // Specialist (enchanter, jewelcrafter)
        public CooperationType Type;            // Employment, Partnership, Outsourcing
        public ushort WageOrShare;              // Wage (currency) OR profit share (%)
    }

    public enum CooperationType : byte
    {
        Employment = 0,         // Fixed wage
        Partnership = 1,        // Profit share
        Outsourcing = 2,        // One-time purchase
    }

    /// <summary>
    /// Trade route component - tracks cross-village material outsourcing.
    /// </summary>
    public struct TradeRoute : IComponentData
    {
        public Entity SourceVillage;            // Village B (exporter)
        public Entity DestinationVillage;       // Village A (importer)
        public MaterialType MaterialType;       // Iron ingots, lumber, etc.
        public ushort QuantityOrdered;          // kg or units
        public ushort TransportationCost;       // Currency
        public byte DeliveryTime;               // Days
        public bool IsDelivered;                // Delivery status
    }

    /// <summary>
    /// Component tracking refining/crafting process in progress.
    /// </summary>
    public struct ProductionInProgress : IComponentData
    {
        public Entity Business;                 // Business performing production
        public ProductionType Type;             // Refining, Crafting
        public Entity InputResource;            // Extracted resource or produced material
        public Entity OutputProduct;            // Produced material or end product
        public byte ProgressPercentage;         // 0-100%
        public byte DaysRemaining;              // Time until completion
    }

    public enum ProductionType : byte
    {
        Refining = 0,           // Extracted → Produced
        Crafting = 1,           // Produced → End Product
        Enchanting = 2,         // End Product → Enchanted Product
    }
}
```

---

## Related Documentation

- **Business & Assets:** [BusinessAndAssets.md](BusinessAndAssets.md) - Business ownership, asset management, quality mechanics
- **Individual Progression:** [Individual_Progression_System.md](../Villagers/Individual_Progression_System.md) - Expertise tiers, skill influence on quality
- **Education & Tutoring:** [Education_And_Tutoring_System.md](../Villagers/Education_And_Tutoring_System.md) - Lessons, absorption, refinement quality
- **Guild System:** [Guild_System.md](../Villagers/Guild_System.md) - Guild training, artisan recruitment, quality standards
- **Circle Progression:** [Circle_Progression_System.md](../Core/Circle_Progression_System.md) - Enchanting, runesmith, magical crafting
- **Individual Combat:** [Individual_Combat_System.md](../Combat/Individual_Combat_System.md) - Weapon/armor stats integration

---

## Open Questions

1. **Material Scarcity:** Should rare materials (mithril, ebony) be location-locked? (e.g., only dwarven mountains have mithril)
   - Current: Materials can be traded cross-village (expensive)
   - Alternative: Some materials are region-exclusive (creates trade dependency)

2. **Critical Success Mechanics:** Should artisans have a chance to create exceptional items?
   - Current: 5% chance to upgrade rarity by 1 tier during critical success
   - Alternative: No random upgrades, only expertise/lessons determine quality

3. **Component Breakdown:** Should failed crafting return components?
   - Current: Not specified
   - Option A: Failed crafting destroys 50% of components (loss)
   - Option B: Failed crafting returns 80% of components (minimal loss)

4. **Enchantment Limits:** Can any item be enchanted, or only high-quality base items?
   - Current: Any item can be enchanted (but low quality base = low quality enchantment)
   - Alternative: Require Quality 60+ base item for enchantments

5. **Tech Tier Progression:** How do villages unlock higher tech tiers?
   - Current: Not specified
   - Option A: Research mechanic (guilds research new techniques over time)
   - Option B: Discovery mechanic (rare events unlock tech tiers)
   - Option C: Trade/learning (villages learn from neighboring villages)

6. **Durability Repair:** Can items be repaired, or do they break permanently?
   - Current: Not specified
   - Option A: Items can be repaired (cost = 20% of original materials, requires artisan)
   - Option B: Items break permanently (creates demand for new crafting)

---

**For Implementers:** Start with simple extraction → refining → crafting pipeline (iron ore → ingots → swords) before adding complex multi-component items (wagons, enchanted weapons). Business cooperation can be v2.0 feature.

**For Designers:** Tune quality formula coefficients to ensure high expertise artisans can mitigate low-purity materials (but not completely negate them). Material purity should always matter.

**For Narrative:** Quality propagation creates emergent stories (legendary blacksmith turns trash ore into decent swords through sheer skill, master alchemist creates life-saving potion from wilted herbs, corrupt merchant sells poor-quality wagons that break down mid-journey).

---

**Last Updated:** 2025-11-07
**Status:** Concept Draft - Core resource chain captured, cooperation mechanics defined
