using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Visual presentation asset reference.
    /// </summary>
    [Serializable]
    public class VisualPresentation
    {
        public GameObject prefabAsset; // Prefab with mesh/model
        public Mesh meshAsset; // Direct mesh reference
        public Sprite spriteAsset; // 2D sprite
        public Material materialOverride; // Optional material override
        
        // Scale and offset
        public Vector3 scale = Vector3.one;
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        
        // Use primitive if no asset assigned
        public bool usePrimitiveFallback = true;
        public PrimitiveType primitiveType = PrimitiveType.Cube;
    }

    /// <summary>
    /// VFX asset reference.
    /// </summary>
    [Serializable]
    public class VFXPresentation
    {
        public UnityEngine.VFX.VisualEffectAsset vfxAsset; // VFX Graph asset
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        public bool playOnAwake = true;
        public bool loop = false;
    }

    /// <summary>
    /// Rarity enum matching runtime spec (items.md).
    /// </summary>
    public enum Rarity : byte
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    /// <summary>
    /// Editor-side ID types (will map to runtime ushort IDs).
    /// </summary>
    [Serializable]
    public struct MaterialId
    {
        public ushort Value;
        public MaterialId(ushort value) { Value = value; }
    }

    [Serializable]
    public struct ItemDefId
    {
        public ushort Value;
        public ItemDefId(ushort value) { Value = value; }
    }

    [Serializable]
    public struct RecipeId
    {
        public ushort Value;
        public RecipeId(ushort value) { Value = value; }
    }

    /// <summary>
    /// Base template for all prefab categories.
    /// Unified quality/rarity/tech tier model per items.md spec.
    /// </summary>
    [Serializable]
    public abstract class PrefabTemplate
    {
        public string name;
        public string displayName;
        public string description;
        public int id; // Editor ID (can be larger), runtime will use ushort IDs
        
        // Unified Quality / Rarity / Tech Tier (per items.md spec)
        public float quality = 50f;              // 0-100, base quality
        public float calculatedQuality = 50f;   // 0-100, derived from materials/skills
        public Rarity rarity = Rarity.Common;   // Base rarity (lower bound)
        public byte techTier = 0;                // 0-10, tier to which this item belongs
        public byte requiredTechTier = 0;       // 0-10, min tier to craft/use
        
        // Visual presentation
        public VisualPresentation visualPresentation = new VisualPresentation();
        public VFXPresentation vfxPresentation = new VFXPresentation();
        
        // Presentation ID for runtime binding (optional)
        public string presentationId = "";
        
        // Area-of-effect bonuses and attributes
        public List<PrefabBonus> bonuses = new List<PrefabBonus>();
    }

    // ============================================================================
    // Material System Enums & Flags
    // ============================================================================

    /// <summary>
    /// Material category classification.
    /// </summary>
    public enum MaterialCategory
    {
        Raw,        // Raw wood, stone, ore
        Extracted,  // Processed materials (cut stone, refined metal)
        Producible, // Crafted materials (bricks, planks)
        Luxury      // Rare/exotic materials
    }

    /// <summary>
    /// Material usage contexts (what the material can be used for).
    /// </summary>
    [Flags]
    public enum MaterialUsage
    {
        None = 0,
        Building = 1 << 0,
        Armor = 1 << 1,
        Weapon = 1 << 2,
        Tool = 1 << 3,
        Container = 1 << 4,
        Fuel = 1 << 5,
        Consumable = 1 << 6,
        Decorative = 1 << 7,
        Ritual = 1 << 8
    }

    /// <summary>
    /// Material traits (property-based flags).
    /// </summary>
    [Flags]
    public enum MaterialTraits
    {
        None = 0,
        Ductile = 1 << 0,      // Can be shaped/hammered
        Brittle = 1 << 1,      // Breaks easily
        Hard = 1 << 2,         // High hardness
        Soft = 1 << 3,         // Low hardness
        Flammable = 1 << 4,    // Can catch fire
        Fireproof = 1 << 5,    // Resistant to fire
        Corrosive = 1 << 6,    // Damages other materials
        CorrosionResistant = 1 << 7,
        Magnetic = 1 << 8,
        Conductive = 1 << 9,   // Electrical conductivity
        Insulating = 1 << 10,
        Transparent = 1 << 11,
        Opaque = 1 << 12,
        Flexible = 1 << 13,
        Rigid = 1 << 14,
        Porous = 1 << 15,      // Absorbs liquids
        Waterproof = 1 << 16,
        Edible = 1 << 17,
        Toxic = 1 << 18,
        Magical = 1 << 19      // Has magical properties
    }

    /// <summary>
    /// Hazard classification for logistics/transport.
    /// </summary>
    public enum HazardClass
    {
        None,
        Flammable,
        Explosive,
        Toxic,
        Corrosive,
        Radioactive,
        Magical
    }

    /// <summary>
    /// Package class for logistics/containers.
    /// </summary>
    public enum PackageClass
    {
        Bulk,       // Loose bulk materials
        Stackable,  // Can be stacked
        Fragile,    // Requires careful handling
        Liquid,     // Requires sealed container
        Gas,        // Requires pressurized container
        Perishable  // Spoils over time
    }

    /// <summary>
    /// Material stats block (physical properties).
    /// </summary>
    [Serializable]
    public class MaterialStats
    {
        public float hardness = 50f;        // 0-100, affects durability
        public float toughness = 50f;      // 0-100, affects impact resistance
        public float density = 1f;         // kg/m³, affects mass
        public float meltingPoint = 1000f; // Temperature in °C
        public float conductivity = 0f;    // Thermal/electrical
    }

    /// <summary>
    /// Logistics block (transport/storage properties).
    /// </summary>
    [Serializable]
    public class LogisticsBlock
    {
        public PackageClass packageClass = PackageClass.Bulk;
        public HazardClass hazardClass = HazardClass.None;
        public float massPerUnit = 1f;     // kg per unit
        public float volumePerUnit = 1f;    // m³ per unit
        public float spoilageRate = 0f;    // Per day (0 = no spoilage)
        public float maxStackSize = 100f;  // Maximum units per stack
    }

    /// <summary>
    /// Economy block (value/trade properties).
    /// </summary>
    [Serializable]
    public class EconomyBlock
    {
        public float baseValue = 1f;       // Base currency value per unit
        public float rarity = 0f;          // 0-100, affects spawn rates
        public float tradeMultiplier = 1f; // Modifier for trade value
    }

    /// <summary>
    /// Miracle affinity (magical properties).
    /// </summary>
    [Serializable]
    public class MiracleAffinity
    {
        public float fireAffinity = 0f;    // 0-100
        public float waterAffinity = 0f;
        public float earthAffinity = 0f;
        public float airAffinity = 0f;
        public float lightAffinity = 0f;
        public float shadowAffinity = 0f;
    }

    /// <summary>
    /// Style tokens (visual/presentation hints).
    /// </summary>
    [Serializable]
    public class StyleTokens
    {
        public string colorPalette = "";   // e.g., "warm_browns"
        public string textureStyle = "";   // e.g., "rough_stone"
        public string materialType = "";   // e.g., "metal", "wood", "stone"
        public List<string> tags = new List<string>(); // Additional style tags
    }

    /// <summary>
    /// Bonus/attribute type for area-of-effect bonuses.
    /// </summary>
    public enum BonusType
    {
        Fertility,          // Increases pregnancy chance
        Happiness,          // Increases happiness/morale
        Productivity,       // Increases work speed/efficiency
        Desirability,       // Increases desirability of nearby buildings
        HealthRegen,        // Regenerates health over time
        ManaGeneration,     // Generates mana/prayer power
        MovementSpeed,      // Increases movement speed
        ResourceYield,      // Increases resource gathering yield
        ConstructionSpeed,  // Increases construction speed
        TradeValue,         // Increases trade value
        Comfort,            // Increases comfort/restoration
        Defense,             // Increases defense/armor
        Attack,              // Increases attack damage
        Experience,         // Increases experience gain
        Moisture,           // Increases moisture/water availability
        Temperature,        // Modifies temperature
        Light,              // Provides light
        Custom              // Custom bonus type (specified by name)
    }

    /// <summary>
    /// Area-of-effect bonus/attribute.
    /// </summary>
    [Serializable]
    public class PrefabBonus
    {
        public BonusType bonusType = BonusType.Custom;
        public string customBonusName = ""; // Used when bonusType is Custom
        
        public float bonusValue = 0f;      // Magnitude of the bonus
        public bool isPercentage = false;  // True if bonusValue is a percentage (e.g., +10%)
        public float radius = 10f;         // Radius of effect in meters
        public bool useFalloff = false;     // If true, bonus decreases with distance
        public float falloffRate = 1f;     // How quickly bonus decreases (1.0 = linear)
        
        // Target filters
        public List<string> targetTags = new List<string>(); // e.g., "Villager", "Building", "ResourceNode"
        public bool affectsSelf = false;    // Does this bonus affect the prefab itself?
        public bool affectsAllies = true;  // Does this bonus affect allies/friendly entities?
        public bool affectsEnemies = false; // Does this bonus affect enemies?
        
        // Duration
        public bool isPermanent = true;    // If false, has a duration
        public float duration = 0f;        // Duration in seconds (0 = permanent)
        
        // Stacking
        public bool stacks = true;          // Can multiple instances stack?
        public float maxStacks = 0f;        // Maximum stack count (0 = unlimited)
        
        // Display
        public string displayName = "";      // Display name for UI
        public string description = "";     // Description of the bonus
    }

    /// <summary>
    /// Material quality tier affecting building stats.
    /// </summary>
    [Serializable]
    public class MaterialQuality
    {
        public string materialName;
        public float qualityMultiplier = 1f; // 0.5-2.0 range
        public float healthBonus = 0f;
        public float desirabilityBonus = 0f;
    }

    /// <summary>
    /// Tool quality affecting construction/build quality.
    /// </summary>
    [Serializable]
    public class ToolQuality
    {
        public string toolName;
        public float qualityMultiplier = 1f;
        public float constructionSpeedBonus = 0f;
        public float durabilityBonus = 0f;
    }

    /// <summary>
    /// Building footprint (size and shape).
    /// </summary>
    [Serializable]
    public class Footprint
    {
        public Vector2 size = Vector2.one; // Width x Depth
        public string shape = "rectangular"; // "rectangular", "circular", "polygonal"
        public List<Vector2> polygonPoints = new List<Vector2>(); // For polygonal shapes
    }

    /// <summary>
    /// Placement requirements/constraints.
    /// </summary>
    [Serializable]
    public class Placement
    {
        public List<string> allowedBiomes = new List<string>(); // Empty = all biomes
        public float maxSlope = 45f; // Maximum slope angle in degrees
        public float minAltitude = float.NegativeInfinity;
        public float maxAltitude = float.PositiveInfinity;
        public bool requiresWater = false;
        public float waterDistanceMin = 0f;
        public float waterDistanceMax = float.PositiveInfinity;
        public bool requiresRoad = false;
        public float roadDistanceMax = float.PositiveInfinity;
        public float adjacencyBonusRange = 0f; // Range for adjacency bonuses
    }

    /// <summary>
    /// Material cost requirement (rule-based).
    /// </summary>
    [Serializable]
    public class MaterialCost
    {
        public MaterialUsage requiredUsage = MaterialUsage.Building;
        public MaterialStats minStats = new MaterialStats(); // Minimum stats required
        public MaterialTraits requiredTraits = MaterialTraits.None;
        public MaterialTraits forbiddenTraits = MaterialTraits.None;
        public float quantity = 1f;
        public bool allowSubstitution = true;
    }

    /// <summary>
    /// Facility tags (what the building can do).
    /// </summary>
    [Flags]
    public enum FacilityTags
    {
        None = 0,
        RefitFacility = 1 << 0,
        Storage = 1 << 1,
        RitualSite = 1 << 2,
        Production = 1 << 3,
        Housing = 1 << 4,
        Defense = 1 << 5
    }

    /// <summary>
    /// Building template with derived stats from materials/tools and expanded Phase 3 fields.
    /// </summary>
    [Serializable]
    public class BuildingTemplate : PrefabTemplate
    {
        public enum BuildingType
        {
            Residence,      // Sleep, rest
            Workplace,      // Work activities
            Recreation,     // Relax, socialize
            Utility,        // Bonuses (well, fertility statue, etc.)
            Storage,        // Storehouse, warehouse
            Worship         // Temple, shrine
        }

        public BuildingType buildingType;
        
        // Phase 3 expansion
        public Footprint footprint = new Footprint();
        public Placement placement = new Placement();
        public List<MaterialCost> cost = new List<MaterialCost>();
        public FacilityTags facilityTags = FacilityTags.None;
        public int residencyCapacity = 0; // Max residents/workers
        public bool continuityRequired = false; // Requires continuous operation
        
        // Legacy material system (for backward compatibility)
        public List<MaterialQuality> materials = new List<MaterialQuality>();
        public ToolQuality tool;
        public float builderSkillLevel = 50f; // 0-100

        // Derived stats (calculated)
        public float baseHealth = 100f;
        public float baseDesirability = 50f;
        public float calculatedHealth;
        public float calculatedDesirability;

        // Building-specific properties
        public int maxResidents = 0; // For residence buildings
        public float comfortLevel = 0f;
        public float restorationRate = 0f;
        public float workCapacity = 0f; // For workplace buildings
        public float areaBonusRange = 0f; // For utility buildings
        public float bonusValue = 0f; // For utility buildings (fertility %, moisture %, etc.)
        public string bonusType = ""; // "fertility", "moisture", "desirability", etc.
        public float storageCapacity = 0f; // For storage buildings
        public float manaGenerationRate = 0f; // For worship buildings
        public float worshipperCapacity = 0f;
    }

    /// <summary>
    /// Discipline (skill) definition.
    /// </summary>
    [Serializable]
    public class Discipline
    {
        public string name = "";
        public float level = 0f; // 0-100
    }

    /// <summary>
    /// Limb reference for individual limb system (Rimworld-style).
    /// </summary>
    [Serializable]
    public class LimbReference
    {
        public string limbId = ""; // Reference to limb definition (e.g., "Head", "LeftArm", "RightLeg")
        public float health = 100f; // 0-100% health status
        public List<string> injuries = new List<string>(); // Permanent injuries (e.g., "LostEye", "CrippledArm")
    }

    /// <summary>
    /// Implant reference for prosthetics/enhancements.
    /// </summary>
    [Serializable]
    public class ImplantReference
    {
        public string implantId = ""; // Reference to implant definition
        public string attachedToLimb = ""; // Which limb this implant is attached to (empty = body)
        public Dictionary<string, float> statModifiers = new Dictionary<string, float>(); // Stat bonuses from implant
    }

    /// <summary>
    /// Individual entity template (villagers, animals, etc.) with expanded Phase 3 fields.
    /// </summary>
    [Serializable]
    public class IndividualTemplate : PrefabTemplate
    {
        public enum IndividualType
        {
            Villager,
            Animal,
            Creature
        }

        public IndividualType individualType;
        public int factionId = 0;
        
        // Phase 3 expansion
        public List<Discipline> disciplines = new List<Discipline>();
        public List<SlotKind> equipmentSlots = new List<SlotKind>();
        public PackageClass inventoryPackageClass = PackageClass.Stackable; // Determines carry capacity
        public MiracleAffinity miracleAffinity = new MiracleAffinity();
        public float spawnWeight = 1f; // Relative spawn probability
        
        // Base stats (legacy - kept for compatibility)
        public float baseHealth = 100f;
        public float baseSpeed = 5f;
        public Dictionary<string, float> baseAttributes = new Dictionary<string, float>();

        // ============================================================================
        // Core Attributes (Experience Modifiers)
        // ============================================================================
        
        /// <summary>
        /// Physical power, muscle, endurance. Modifies Strength experience gain.
        /// </summary>
        public float physique = 50f; // 0-100
        
        /// <summary>
        /// Skill, speed, agility, precision. Modifies Finesse experience gain.
        /// </summary>
        public float finesse = 50f; // 0-100
        
        /// <summary>
        /// Mental fortitude, courage, determination. Modifies Will experience gain.
        /// </summary>
        public float will = 50f; // 0-100
        
        /// <summary>
        /// Accumulates and generates general experience. Higher wisdom = faster overall progression.
        /// </summary>
        public float wisdom = 50f; // 0-100

        // ============================================================================
        // Derived Attributes
        // ============================================================================
        
        /// <summary>
        /// Physical power (derived from Physique + experience).
        /// </summary>
        public float strength = 50f; // 0-100
        
        /// <summary>
        /// Speed and dexterity (derived from Finesse + experience).
        /// </summary>
        public float agility = 50f; // 0-100
        
        /// <summary>
        /// Mental acuity (derived from Will + experience, affects magic).
        /// </summary>
        public float intelligence = 50f; // 0-100

        // ============================================================================
        // Social Stats
        // ============================================================================
        
        /// <summary>
        /// Public recognition, legendary status threshold at 500+.
        /// </summary>
        public float fame = 0f; // 0-1000
        
        /// <summary>
        /// Liquid wealth + asset value (currency).
        /// </summary>
        public float wealth = 0f; // Currency
        
        /// <summary>
        /// Standing in community.
        /// </summary>
        public float reputation = 0f; // -100 to +100
        
        /// <summary>
        /// Combat achievements, heroic deeds.
        /// </summary>
        public float glory = 0f; // 0-1000
        
        /// <summary>
        /// Overall legendary status (combines fame + glory).
        /// </summary>
        public float renown = 0f; // 0-1000

        // ============================================================================
        // Combat Stats (Base values, will be calculated from attributes at runtime)
        // ============================================================================
        
        /// <summary>
        /// To-hit chance (calculated: Finesse × 0.7 + Strength × 0.3).
        /// Can be overridden in template.
        /// </summary>
        public float baseAttack = 0f; // 0-100 (0 = auto-calculate from attributes)
        
        /// <summary>
        /// Dodge/block chance (calculated: Finesse × 0.6 + armor).
        /// Can be overridden in template.
        /// </summary>
        public float baseDefense = 0f; // 0-100 (0 = auto-calculate from attributes)
        
        /// <summary>
        /// Max HP (calculated: Strength × 0.6 + Will × 0.4 + 50).
        /// Can be overridden in template.
        /// </summary>
        public float baseHealthOverride = 0f; // 0 = use baseHealth or calculate from attributes
        
        /// <summary>
        /// Rounds before exhaustion (calculated: Strength / 10).
        /// Can be overridden in template.
        /// </summary>
        public float baseStamina = 10f; // Rounds (0 = auto-calculate from Strength)
        
        /// <summary>
        /// Max mana for magic users (calculated: Will × 0.5 + Intelligence × 0.5).
        /// Can be overridden in template.
        /// </summary>
        public float baseMana = 0f; // 0-100 (0 = auto-calculate from attributes, 0 = non-magic user)

        // ============================================================================
        // Need Stats (Runtime values, templates provide starting values)
        // ============================================================================
        
        /// <summary>
        /// Hunger level, 0 = starving.
        /// </summary>
        public float food = 100f; // 0-100
        
        /// <summary>
        /// Fatigue level, 0 = exhausted.
        /// </summary>
        public float rest = 100f; // 0-100
        
        /// <summary>
        /// Sleep need, 0 = sleep-deprived.
        /// </summary>
        public float sleep = 100f; // 0-100
        
        /// <summary>
        /// Overall health status (separate from combat HP).
        /// </summary>
        public float generalHealth = 100f; // 0-100

        // ============================================================================
        // Resistances (Damage Type Modifiers)
        // ============================================================================
        
        /// <summary>
        /// Damage type resistances (0-100% reduction).
        /// Keys: "Physical", "Fire", "Cold", "Poison", "Magic", "Lightning", "Holy", "Dark"
        /// Values: 0.0-1.0 (0.0 = no resistance, 1.0 = 100% immunity)
        /// </summary>
        public Dictionary<string, float> resistances = new Dictionary<string, float>();

        // ============================================================================
        // Healing & Spell Modifiers
        // ============================================================================
        
        /// <summary>
        /// Multiplies healing received (e.g., 1.2 = +20% healing).
        /// </summary>
        public float healBonus = 1.0f; // Multiplier (default 1.0 = no bonus)
        
        /// <summary>
        /// Modifies duration of own spells (e.g., 1.5 = +50% duration).
        /// </summary>
        public float spellDurationModifier = 1.0f; // Multiplier (default 1.0 = no modifier)
        
        /// <summary>
        /// Modifies intensity/damage of own spells (e.g., 1.3 = +30% damage).
        /// </summary>
        public float spellIntensityModifier = 1.0f; // Multiplier (default 1.0 = no modifier)

        // ============================================================================
        // Limb System (Rimworld-style)
        // ============================================================================
        
        /// <summary>
        /// List of limb references (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, etc.).
        /// </summary>
        public List<LimbReference> limbs = new List<LimbReference>();
        
        /// <summary>
        /// List of implant references (prosthetics, enhancements).
        /// </summary>
        public List<ImplantReference> implants = new List<ImplantReference>();

        // ============================================================================
        // Outlooks & Alignments
        // ============================================================================
        
        /// <summary>
        /// Reference to alignment definition (Moral/ideological position).
        /// </summary>
        public string alignmentId = ""; // Reference to alignment definition
        
        /// <summary>
        /// References to outlook definitions (Cultural/behavioral expressions).
        /// </summary>
        public List<string> outlookIds = new List<string>(); // References to outlook definitions
        
        /// <summary>
        /// Disposition toward external forces (Loyalty, Fear, Love, Trust, Respect).
        /// Keys: "Loyalty", "Fear", "Love", "Trust", "Respect"
        /// Values: -100 to +100
        /// </summary>
        public Dictionary<string, float> disposition = new Dictionary<string, float>();

        // ============================================================================
        // Personality Traits
        // ============================================================================
        
        /// <summary>
        /// Vengeful (-100) ↔ Forgiving (+100) personality axis.
        /// Range: -100 to +100 (sbyte)
        /// </summary>
        public sbyte vengefulScore = 0;
        
        /// <summary>
        /// Bold (+100) ↔ Craven (-100) personality axis.
        /// Range: -100 to +100 (sbyte)
        /// </summary>
        public sbyte boldScore = 0;
        
        /// <summary>
        /// Flag indicating this individual is undead.
        /// Used for various effects (healing, resurrection, etc.).
        /// </summary>
        public bool isUndead = false;
        
        /// <summary>
        /// Flag indicating this individual is summoned.
        /// Used for various effects (duration, dismissal, etc.).
        /// </summary>
        public bool isSummoned = false;

        // ============================================================================
        // Titles (Deeds of Rulership)
        // ============================================================================
        
        /// <summary>
        /// List of titles held by this individual.
        /// Titles are deeds of rulership passed on with inheritance or otherwise depending on outlooks.
        /// Examples: Hero of a village, Elite living in a mansion, Ruler over a village and its lands.
        /// Titles are acquired when founding a village, successfully defending it, having enough renown, etc.
        /// </summary>
        public List<Title> titles = new List<Title>();
    }
    
    /// <summary>
    /// Title (deed of rulership) held by an individual entity.
    /// Titles can be inherited or acquired through various means (founding villages, defending them, renown, etc.).
    /// Titles have levels from leader of an upstart band (low) to ruler of multiple empires (high).
    /// Only the highest level title is presented/displayed, but the entity is known by all its titles.
    /// Titles can be lost, usurped, disinherited, revoked, or become fallen (e.g., fallen empire).
    /// Former titles carry some prestige but are shadows of the proper title.
    /// </summary>
    [Serializable]
    public class Title
    {
        /// <summary>
        /// Title name/identifier (e.g., "Hero of Oakwood", "Elite of Stonehaven", "Ruler of Greenfield").
        /// </summary>
        public string titleName = "";
        
        /// <summary>
        /// Display name for the title (e.g., "Hero of Oakwood Village").
        /// </summary>
        public string displayName = "";
        
        /// <summary>
        /// Title type/category.
        /// </summary>
        public TitleType titleType = TitleType.Hero;
        
        /// <summary>
        /// Associated settlement/village name (if applicable).
        /// </summary>
        public string associatedSettlement = "";
        
        /// <summary>
        /// Associated building/structure name (if applicable, e.g., mansion name).
        /// </summary>
        public string associatedBuilding = "";
        
        /// <summary>
        /// How this title was acquired.
        /// </summary>
        public TitleAcquisitionMethod acquisitionMethod = TitleAcquisitionMethod.Founding;
        
        /// <summary>
        /// Can this title be inherited? (Depends on outlooks)
        /// </summary>
        public bool isInheritable = true;
        
        /// <summary>
        /// Minimum renown required to acquire this title (if applicable).
        /// </summary>
        public float minRenown = 0f;
        
        /// <summary>
        /// Title level/rank (higher = more prestigious).
        /// Levels range from 1 (leader of an upstart band) to 10+ (ruler of multiple empires).
        /// Only the highest level title is presented, but entity is known by all titles.
        /// </summary>
        public int titleLevel = 1;
        
        /// <summary>
        /// Current status of the title.
        /// Active titles are current and valid. Lost/Usurped/Disinherited/Revoked/Fallen titles are former titles.
        /// </summary>
        public TitleStatus status = TitleStatus.Active;
        
        /// <summary>
        /// Reason why the title was lost (if status is not Active).
        /// </summary>
        public string lossReason = "";
        
        /// <summary>
        /// Bonuses granted by this title (e.g., reputation, wealth, social standing).
        /// Former titles typically have reduced bonuses (shadow of the proper title).
        /// </summary>
        public TitleBonuses bonuses = new TitleBonuses();
        
        /// <summary>
        /// Bonuses for former titles (reduced prestige, typically 10-30% of original bonuses).
        /// If null, uses reduced bonuses based on status.
        /// </summary>
        public TitleBonuses formerBonuses = null;
        
        /// <summary>
        /// Description/flavor text for this title.
        /// </summary>
        public string description = "";
        
        /// <summary>
        /// Get the effective display name (includes "Former" prefix if title is lost).
        /// </summary>
        public string GetEffectiveDisplayName()
        {
            if (status == TitleStatus.Active)
                return displayName;
            
            string prefix = GetStatusPrefix();
            return string.IsNullOrEmpty(prefix) ? displayName : $"{prefix} {displayName}";
        }
        
        /// <summary>
        /// Get the prefix for lost titles (e.g., "Former", "Ex-").
        /// </summary>
        public string GetStatusPrefix()
        {
            switch (status)
            {
                case TitleStatus.Lost:
                    return "Former";
                case TitleStatus.Usurped:
                    return "Former";
                case TitleStatus.Disinherited:
                    return "Former";
                case TitleStatus.Revoked:
                    return "Former";
                case TitleStatus.Fallen:
                    return "Former";
                default:
                    return "";
            }
        }
        
        /// <summary>
        /// Get effective bonuses (reduced for former titles).
        /// </summary>
        public TitleBonuses GetEffectiveBonuses()
        {
            if (status == TitleStatus.Active)
                return bonuses;
            
            // Former titles get reduced bonuses (shadow of proper title)
            if (formerBonuses != null)
                return formerBonuses;
            
            // Default reduction: 20% of original bonuses for prestige, but keep some authority
            return new TitleBonuses
            {
                reputationBonus = bonuses.reputationBonus * 0.2f,
                fameBonus = bonuses.fameBonus * 0.2f,
                wealthBonus = bonuses.wealthBonus * 0.1f, // Wealth is mostly lost
                renownBonus = bonuses.renownBonus * 0.3f, // Renown persists more
                gloryBonus = bonuses.gloryBonus * 0.25f, // Glory persists
                socialStandingModifier = Mathf.Max(0.5f, bonuses.socialStandingModifier * 0.6f), // Reduced but not gone
                authorityLevel = Mathf.Max(0, bonuses.authorityLevel - 2) // Reduced authority
            };
        }
        
        /// <summary>
        /// Get a human-readable description of the title level.
        /// </summary>
        public string GetLevelDescription()
        {
            if (titleLevel <= 1)
                return "Leader of an upstart band";
            else if (titleLevel <= 2)
                return "Village leader";
            else if (titleLevel <= 3)
                return "Town leader";
            else if (titleLevel <= 4)
                return "City leader";
            else if (titleLevel <= 5)
                return "Regional leader";
            else if (titleLevel <= 6)
                return "Provincial leader";
            else if (titleLevel <= 7)
                return "Kingdom leader";
            else if (titleLevel <= 8)
                return "Empire leader";
            else if (titleLevel <= 9)
                return "Multi-kingdom ruler";
            else
                return "Ruler of multiple empires";
        }
        
        /// <summary>
        /// Check if this title is active (not lost/former).
        /// </summary>
        public bool IsActive()
        {
            return status == TitleStatus.Active;
        }
    }
    
    /// <summary>
    /// Status of a title (active or various lost states).
    /// </summary>
    public enum TitleStatus
    {
        /// <summary>
        /// Title is active and current.
        /// </summary>
        Active,
        
        /// <summary>
        /// Title was lost (e.g., band was broken, settlement fell).
        /// </summary>
        Lost,
        
        /// <summary>
        /// Title was usurped by another entity.
        /// </summary>
        Usurped,
        
        /// <summary>
        /// Title was disinherited (removed from inheritance line).
        /// </summary>
        Disinherited,
        
        /// <summary>
        /// Title was revoked (removed by authority).
        /// </summary>
        Revoked,
        
        /// <summary>
        /// Title became fallen (e.g., empire fell, settlement destroyed).
        /// </summary>
        Fallen
    }
    
    /// <summary>
    /// Helper class for title operations.
    /// </summary>
    public static class TitleHelper
    {
        /// <summary>
        /// Get the highest level ACTIVE title from a list of titles.
        /// This is the title that should be presented/displayed.
        /// Only active titles count for primary display.
        /// </summary>
        public static Title GetHighestLevelTitle(List<Title> titles, bool activeOnly = true)
        {
            if (titles == null || titles.Count == 0)
                return null;
            
            var filtered = activeOnly ? titles.Where(t => t.IsActive()).ToList() : titles;
            if (filtered.Count == 0)
                return null; // No active titles, return null (entity has no current title)
            
            return filtered.OrderByDescending(t => t.titleLevel).FirstOrDefault();
        }
        
        /// <summary>
        /// Get all titles sorted by level (highest first).
        /// </summary>
        public static List<Title> GetTitlesSortedByLevel(List<Title> titles, bool activeOnly = false)
        {
            if (titles == null || titles.Count == 0)
                return new List<Title>();
            
            var filtered = activeOnly ? titles.Where(t => t.IsActive()).ToList() : titles;
            return filtered.OrderByDescending(t => t.titleLevel).ToList();
        }
        
        /// <summary>
        /// Get all active titles.
        /// </summary>
        public static List<Title> GetActiveTitles(List<Title> titles)
        {
            if (titles == null || titles.Count == 0)
                return new List<Title>();
            
            return titles.Where(t => t.IsActive()).ToList();
        }
        
        /// <summary>
        /// Get all former/lost titles.
        /// </summary>
        public static List<Title> GetFormerTitles(List<Title> titles)
        {
            if (titles == null || titles.Count == 0)
                return new List<Title>();
            
            return titles.Where(t => !t.IsActive()).ToList();
        }
        
        /// <summary>
        /// Get the display name for an entity based on all their titles.
        /// Format: "Primary Title (also: Secondary Title, Tertiary Title)"
        /// Only active titles are shown in the primary display.
        /// </summary>
        public static string GetFullTitleDisplay(List<Title> titles)
        {
            if (titles == null || titles.Count == 0)
                return "";
            
            var activeTitles = GetActiveTitles(titles);
            if (activeTitles.Count == 0)
            {
                // No active titles, show highest former title
                var formerTitles = GetFormerTitles(titles);
                if (formerTitles.Count == 0)
                    return "";
                
                var highestFormer = formerTitles.OrderByDescending(t => t.titleLevel).First();
                return highestFormer.GetEffectiveDisplayName();
            }
            
            var sorted = GetTitlesSortedByLevel(activeTitles);
            var primary = sorted[0];
            
            if (sorted.Count == 1)
                return primary.GetEffectiveDisplayName();
            
            var secondary = sorted.Skip(1).Select(t => t.GetEffectiveDisplayName()).ToList();
            return $"{primary.GetEffectiveDisplayName()} (also: {string.Join(", ", secondary)})";
        }
        
        /// <summary>
        /// Get the primary title display name (highest level active title only).
        /// Returns "Former [Title]" if only former titles exist.
        /// </summary>
        public static string GetPrimaryTitleDisplay(List<Title> titles)
        {
            var highest = GetHighestLevelTitle(titles, activeOnly: true);
            if (highest != null)
                return highest.GetEffectiveDisplayName();
            
            // No active titles, check for former titles
            var formerHighest = GetHighestLevelTitle(titles, activeOnly: false);
            return formerHighest != null ? formerHighest.GetEffectiveDisplayName() : "";
        }
        
        /// <summary>
        /// Get a summary of all titles (active and former).
        /// </summary>
        public static string GetTitleSummary(List<Title> titles)
        {
            if (titles == null || titles.Count == 0)
                return "No titles";
            
            var active = GetActiveTitles(titles);
            var former = GetFormerTitles(titles);
            
            var parts = new List<string>();
            
            if (active.Count > 0)
            {
                parts.Add($"{active.Count} active title(s)");
            }
            
            if (former.Count > 0)
            {
                parts.Add($"{former.Count} former title(s)");
            }
            
            return string.Join(", ", parts);
        }
    }
    
    /// <summary>
    /// Title type/category.
    /// </summary>
    public enum TitleType
    {
        Hero,           // Hero of a village (defended it, performed heroic deeds)
        Elite,          // Elite living in a mansion (high renown, wealth)
        Ruler,          // Ruler over a village and its lands (founded it, inherited it, or claimed it)
        Noble,          // Noble title (inherited or granted)
        Champion,       // Champion (combat achievements)
        Founder,        // Founder of a settlement
        Defender,       // Defender of a settlement
        Merchant,       // Merchant/merchant guild title
        Artisan,        // Master artisan/craftsman title
        Scholar,        // Scholar/academic title
        Priest,         // Religious title
        Other           // Other custom title
    }
    
    /// <summary>
    /// How a title was acquired.
    /// </summary>
    public enum TitleAcquisitionMethod
    {
        Founding,           // Founded a village/settlement
        Defense,            // Successfully defended a village
        Renown,             // Achieved enough renown
        Inheritance,        // Inherited from parent/ancestor
        Grant,              // Granted by another ruler
        Purchase,           // Purchased (merchant titles)
        Achievement,        // Achieved through specific accomplishments
        Custom              // Custom/other method
    }
    
    /// <summary>
    /// Bonuses granted by a title.
    /// </summary>
    [Serializable]
    public class TitleBonuses
    {
        /// <summary>
        /// Reputation bonus (added to base reputation).
        /// </summary>
        public float reputationBonus = 0f; // -100 to +100
        
        /// <summary>
        /// Fame bonus (added to base fame).
        /// </summary>
        public float fameBonus = 0f; // 0-1000
        
        /// <summary>
        /// Wealth bonus (added to base wealth).
        /// </summary>
        public float wealthBonus = 0f; // Currency
        
        /// <summary>
        /// Renown bonus (added to base renown).
        /// </summary>
        public float renownBonus = 0f; // 0-1000
        
        /// <summary>
        /// Glory bonus (added to base glory).
        /// </summary>
        public float gloryBonus = 0f; // 0-1000
        
        /// <summary>
        /// Social standing modifier (multiplier for social interactions).
        /// </summary>
        public float socialStandingModifier = 1.0f; // Multiplier (1.0 = no change)
        
        /// <summary>
        /// Authority level (affects ability to command, make decisions, etc.).
        /// </summary>
        public int authorityLevel = 0; // 0 = no authority, higher = more authority
    }

    /// <summary>
    /// Equipment slot kind.
    /// </summary>
    public enum SlotKind
    {
        Hand,       // Main hand / off hand
        Body,       // Torso armor
        Head,       // Helmet/hat
        Feet,       // Boots
        Accessory   // Ring, amulet, etc.
    }

    /// <summary>
    /// Equipment stats (individual stat-oriented attributes).
    /// </summary>
    [Serializable]
    public class EquipmentStats
    {
        // Combat stats
        public float damage = 0f;           // Base damage (for weapons)
        public float armor = 0f;            // Armor value (for armor)
        public float blockChance = 0f;      // Block/parry chance (%)
        public float critChance = 0f;       // Critical hit chance (%)
        public float critDamage = 0f;       // Critical damage multiplier
        
        // Physical stats
        public float weight = 0f;           // Weight in kg (overrides mass if > 0)
        public float encumbrance = 0f;      // Movement speed penalty (%)
        
        // Derived from material quality
        public float calculatedDamage = 0f;
        public float calculatedArmor = 0f;
        public float calculatedDurability = 0f;
    }

    /// <summary>
    /// Equipment template (weapons, armor, tools) with expanded Phase 3 fields.
    /// Aligned with ItemDefinitionBlob from items.md spec.
    /// </summary>
    [Serializable]
    public class EquipmentTemplate : PrefabTemplate
    {
        public enum EquipmentType
        {
            Weapon,
            Armor,
            Tool,
            Accessory
        }

        // Catalog ID (maps to runtime ItemDefId)
        public ItemDefId itemDefId = new ItemDefId(0);

        public EquipmentType equipmentType;
        public SlotKind slotKind = SlotKind.Hand;
        public float mass = 1f; // kg (base mass, can be overridden by stats.weight)
        
        // Material requirements (rule-based)
        public MaterialUsage requiredUsage = MaterialUsage.None;
        public MaterialStats minStats = new MaterialStats();
        public MaterialTraits requiredTraits = MaterialTraits.None;
        public MaterialTraits forbiddenTraits = MaterialTraits.None;
        public bool allowSubstitution = true;
        
        // Equipment stats (individual stat-oriented)
        public EquipmentStats stats = new EquipmentStats();
        
        // Durability
        public float baseDurability = 100f;
        public float calculatedDurability;
        
        // Effects (legacy - use bonuses instead)
        public Dictionary<string, float> statBonuses = new Dictionary<string, float>();
        
        // Legacy compatibility
        public MaterialQuality material;
        
        // Production facilities
        /// <summary>
        /// List of facility IDs/types that can produce this equipment.
        /// Examples: "Forge", "Workshop", "Apothecary", "Smithy", etc.
        /// Empty list = can be produced anywhere or no production facility required.
        /// </summary>
        public List<string> productionFacilities = new List<string>();
    }

    /// <summary>
    /// Material template (raw/extracted/producible/luxury) with expanded traits system.
    /// Aligned with MaterialDefinitionBlob from items.md spec.
    /// </summary>
    [Serializable]
    public class MaterialTemplate : PrefabTemplate
    {
        // Catalog ID (maps to runtime MaterialId)
        public MaterialId materialId = new MaterialId(0);
        
        public MaterialCategory category;
        public MaterialUsage usage = MaterialUsage.None;
        public MaterialTraits traits = MaterialTraits.None;
        
        // Quality/purity/variant (base fields inherited from PrefabTemplate)
        // Additional material-specific fields:
        public float baseQuality = 50f; // 0-100 baseline quality
        public float purity = 100f; // For extracted materials (0-100), maps to BasePurity
        public string variantOf = ""; // If this is a variant, reference base material
        
        // Stat blocks
        public MaterialStats stats = new MaterialStats();
        public LogisticsBlock logistics = new LogisticsBlock();
        public EconomyBlock economy = new EconomyBlock();
        public MiracleAffinity miracleAffinity = new MiracleAffinity();
        public StyleTokens styleTokens = new StyleTokens();
        
        // Substitution support
        public int substitutionRank = 0; // Lower = preferred substitute
        public List<string> substituteFor = new List<string>(); // Material names this can substitute
        
        // Material attributes (can be added by skilled craftsmen when used in production)
        public List<MaterialAttribute> possibleAttributes = new List<MaterialAttribute>();
        
        // Used in production chains (what can be made from this material)
        public List<string> usedInProduction = new List<string>(); // Names of tools/products that use this material
        
        // Production facilities
        /// <summary>
        /// List of facility IDs/types that can produce/extract this material.
        /// Examples: "Forge", "Workshop", "Apothecary", "Mine", "Quarry", etc.
        /// Empty list = can be produced anywhere, gathered from environment, or no production facility required.
        /// </summary>
        public List<string> productionFacilities = new List<string>();
        
        // Legacy compatibility
        public Dictionary<string, float> properties = new Dictionary<string, float>();
    }

    /// <summary>
    /// Production chain input (material requirement for crafting).
    /// Aligned with ProductionInputBlob from items.md spec.
    /// </summary>
    [Serializable]
    public class ProductionInput
    {
        public string materialName = "";   // Name of required material (e.g., "Iron_Ingot") - editor-side
        public MaterialId materialId = new MaterialId(0); // Runtime MaterialId (set during baking)
        public float quantity = 1f;         // Quantity required
        public float minPurity = 0f;       // Minimum material purity (0-100)
        public float minQuality = 0f;      // Minimum material quality (0-100)
        public byte minTechTier = 0;       // Minimum tech tier (0-10) per items.md spec
        public bool isOptional = false;     // Can this input be omitted?
    }

    /// <summary>
    /// Quality derivation factors (how quality is calculated).
    /// Aligned with QualityWeights from items.md spec.
    /// </summary>
    [Serializable]
    public class QualityDerivation
    {
        public float materialPurityWeight = 0.4f;    // How much material purity affects quality (0-1)
        public float materialQualityWeight = 0.3f;   // How much material quality affects quality (0-1)
        public float craftsmanSkillWeight = 0.2f;   // How much craftsman skill affects quality (0-1)
        public float forgeQualityWeight = 0.1f;     // How much forge/facility quality affects quality (0-1)
        
        // Quality multipliers
        public float baseQualityMultiplier = 1f;    // Base multiplier
        public float minQuality = 0f;               // Minimum quality (0-100)
        public float maxQuality = 100f;              // Maximum quality (0-100)
        
        /// <summary>
        /// Convert to QualityWeights struct for StatCalculation.
        /// </summary>
        public QualityWeights ToQualityWeights()
        {
            return new QualityWeights
            {
                MaterialPurityWeight = materialPurityWeight,
                MaterialQualityWeight = materialQualityWeight,
                CraftsmanSkillWeight = craftsmanSkillWeight,
                FacilityQualityWeight = forgeQualityWeight
            };
        }
    }

    /// <summary>
    /// Material attribute (added by skilled craftsmen).
    /// </summary>
    [Serializable]
    public class MaterialAttribute
    {
        public string name = "";           // Attribute name (e.g., "IncreasedDurability")
        public float value = 0f;           // Attribute value
        public bool isPercentage = false;  // Is value a percentage?
        public float minCraftsmanSkill = 50f; // Minimum craftsman skill to add this attribute (0-100)
        public float chanceToAdd = 1f;     // Chance to add attribute (0-1, 1 = always if skill met)
    }

    /// <summary>
    /// Tool template with production chain support.
    /// Aligned with ProductionRecipeBlob from items.md spec.
    /// </summary>
    [Serializable]
    public class ToolTemplate : PrefabTemplate
    {
        // Catalog IDs
        public ItemDefId itemDefId = new ItemDefId(0); // Maps to runtime ItemDefId
        public RecipeId recipeId = new RecipeId(0);   // Maps to runtime RecipeId for production recipe
        
        // Production chain
        public List<ProductionInput> productionInputs = new List<ProductionInput>(); // Materials needed to make this tool
        public string producedFrom = "";   // Name of material/tool this is produced from (e.g., "Iron_Ingot")
        
        // Recipe metadata (per items.md spec)
        public float baseFacilityQuality = 50f; // Base facility quality for this recipe (0-100)
        
        // Quality derivation
        public QualityDerivation qualityDerivation = new QualityDerivation();
        public float baseQuality = 50f;
        public float calculatedQuality;
        
        // Material attributes (added by skilled craftsmen)
        public List<MaterialAttribute> possibleAttributes = new List<MaterialAttribute>(); // Attributes that can be added
        
        // Tool properties
        public float constructionSpeedBonus = 0f;
        public float durabilityBonus = 0f;
        public float workEfficiency = 1f;  // Multiplier for work speed/efficiency
        
        // Production facilities
        /// <summary>
        /// List of facility IDs/types that can produce this tool.
        /// Examples: "Forge", "Workshop", "CarpenterShop", "Smithy", etc.
        /// Empty list = can be produced anywhere or no production facility required.
        /// </summary>
        public List<string> productionFacilities = new List<string>();
        
        // Legacy compatibility
        public MaterialQuality material;
    }

    /// <summary>
    /// Reagent template (for crafting, miracles, etc.).
    /// </summary>
    [Serializable]
    public class ReagentTemplate : PrefabTemplate
    {
        public float potency = 50f; // 0-100
        public float rarity = 0f;
        public Dictionary<string, float> effects = new Dictionary<string, float>();
        
        // Production facilities
        /// <summary>
        /// List of facility IDs/types that can produce this reagent.
        /// Examples: "Apothecary", "AlchemyLab", "Workshop", "Temple", etc.
        /// Empty list = can be produced anywhere or no production facility required.
        /// </summary>
        public List<string> productionFacilities = new List<string>();
    }

    /// <summary>
    /// Target filter for miracles (what can be targeted).
    /// </summary>
    [Serializable]
    public class TargetFilter
    {
        public MaterialTraits requiredTraits = MaterialTraits.None;
        public MaterialTraits forbiddenTraits = MaterialTraits.None;
        public List<string> requiredTags = new List<string>(); // e.g., "Villager", "Building"
        public List<string> forbiddenTags = new List<string>();
    }

    /// <summary>
    /// Area shape for miracles.
    /// </summary>
    public enum AreaShape
    {
        Point,      // Single target
        Circle,     // Radius-based
        Rectangle,  // Width x Height
        Line,       // Line from caster
        Cone        // Cone from caster
    }

    /// <summary>
    /// Effect phase for miracle effects (when they execute).
    /// </summary>
    public enum MiracleEffectPhase
    {
        Channel,    // Active while player holds cast button (sustained mode)
        Impact,     // Triggers on projectile impact (throw mode)
        Lingering   // Persists after activation ends
    }

    /// <summary>
    /// Effect type for miracle effects (what they do).
    /// </summary>
    public enum MiracleEffectType
    {
        Damage,             // Deal damage to entities/buildings
        Healing,            // Restore health
        Status,             // Apply buffs/debuffs (burn, shock, fear, haste, slow)
        Environmental,      // Modify terrain (moisture, fire spread, time dilation)
        Knockback,          // Apply physics forces
        Cleanse,            // Remove status effects
        Synergy             // Modify other miracles (water + lightning = electrified puddles)
    }

    /// <summary>
    /// Status effect types that can be applied by miracles.
    /// </summary>
    public enum MiracleStatusType
    {
        None,
        Burn,               // Fire damage over time
        Shock,              // Stun, disarm (lightning)
        Fear,               // Panic chance increase
        Haste,              // Speed multiplier increase
        Slow,               // Speed multiplier decrease
        Freeze,             // Time stop
        Electrified,        // Water puddle hazard
        FortifiedHealth,    // Temporary health buff
        TemporalLashback    // Stun, mana debt increase
    }

    /// <summary>
    /// Target type filter for miracle effects.
    /// </summary>
    public enum MiracleTargetType
    {
        All,                // Everything
        Units,              // Villagers, creatures, enemies
        Buildings,          // Structures only
        Terrain,            // Ground, water, etc.
        UnitsAndBuildings,  // Both units and buildings
        EnemiesOnly,        // Hostile entities only
        AlliesOnly          // Friendly entities only
    }

    /// <summary>
    /// A single effect block for a miracle, defining gameplay logic.
    /// </summary>
    [Serializable]
    public class MiracleEffectBlock
    {
        public string effectName = "New Effect";
        public MiracleEffectType effectType = MiracleEffectType.Damage;
        public MiracleEffectPhase phase = MiracleEffectPhase.Impact;
        
        // Target filtering
        public MiracleTargetType targetType = MiracleTargetType.All;
        public bool friendlyFire = false;
        public bool requiresLineOfSight = false;
        public float range = 10f;
        
        // Area of effect
        public AreaShape areaShape = AreaShape.Circle;
        public Vector2 areaSize = Vector2.one; // For Circle: radius in x
        
        // Timing
        public float duration = 0f;           // Total duration (0 = instant)
        public float tickInterval = 1f;        // Seconds between ticks (0 = no ticks, instant)
        
        // Damage parameters
        public float damageAmount = 0f;
        public float armorPenetration = 0f;    // Percentage (0-100%)
        public float damagePerSecond = 0f;     // For DoT effects
        
        // Healing parameters
        public float healAmount = 0f;
        public float healPerTick = 0f;
        public float maxHeal = 0f;             // Cap on total healing (0 = no cap)
        public float cleanseStrength = 0f;     // Maximum status severity removed
        public bool overhealGivesBuff = false; // Convert overheal to buff
        
        // Status effect parameters
        public MiracleStatusType statusType = MiracleStatusType.None;
        public float statusDuration = 0f;
        public float statusSeverity = 1f;      // Intensity level
        public bool statusStacks = false;
        public int maxStacks = 1;
        
        // Environmental parameters
        public float moistureAmount = 0f;      // Terrain moisture added
        public float fireSuppression = 0f;      // Percentage reduction in fire DPS (0-100%)
        public float spreadChance = 0f;        // Probability of propagation per second (0-100%)
        public float speedMultiplier = 1f;     // Time dilation factor (0.25-2.0)
        
        // Knockback parameters
        public float knockbackForce = 0f;      // Knockback distance/strength
        public bool radialKnockback = true;    // Away from center vs directional
        
        // Synergy parameters
        public List<string> triggersOnMiracleTypes = new List<string>(); // Miracle types that activate this
        public string synergyEffect = "";      // What happens when triggered
        
        // Special parameters
        public float comaReduction = 0f;       // Days of coma reduced
        public float manaDebtReduction = 0f;    // Mana debt reduction amount
        public float lashbackChance = 0f;      // Risk chance (0-100%)
        public float lashbackManaDebtIncrease = 0f;
        
        // Chain/Propagation
        public bool canChain = false;
        public float chainRange = 0f;
        public float chainDamageDecay = 0f;    // Percentage reduction per chain (0-100%)
        public int maxChains = 0;
        
        // Scaling
        public bool scalesWithFaithDensity = false;
        public float faithDensityMultiplier = 1f;
        public bool scalesWithFocus = false;
        public float focusMultiplier = 1f;
    }

    /// <summary>
    /// Activation mode configuration for miracles.
    /// </summary>
    [Serializable]
    public class MiracleActivationConfig
    {
        public bool supportsSustain = true;
        public bool supportsThrow = true;
        
        // Base costs (one-time for throw, upfront for channel)
        public float basePrayerCost = 0f;
        public float baseManaCost = 0f;
        public float baseFocusCost = 0f;
        
        // Sustained costs (per second for channel mode)
        public float sustainPrayerCostPerSecond = 0f;
        public float sustainManaCostPerSecond = 0f;
        public float sustainFocusCostPerSecond = 0f;
        
        // Cooldown
        public float cooldown = 60f;
        
        // Faith discount
        public bool faithDensityReducesCost = false;
        public float maxFaithDiscount = 0f;     // Maximum percentage discount (0-100%)
        
        // Alignment modifiers (cost multipliers)
        public float evilAlignmentCostMultiplier = 1f;
        public float benevolentAlignmentCostMultiplier = 1f;
    }

    /// <summary>
    /// Miracle/spell template with expanded Phase 3 fields and effect blocks.
    /// </summary>
    [Serializable]
    public class MiracleTemplate : PrefabTemplate
    {
        public TargetFilter targetFilter = new TargetFilter();
        public AreaShape areaShape = AreaShape.Point;
        public Vector2 areaSize = Vector2.one; // For Circle: radius in x, unused in y
        
        // Legacy fields (kept for compatibility, prefer activationConfig)
        public float manaCost = 10f;
        public float cooldown = 60f;
        public float range = 10f;
        public float duration = 5f;
        
        // Activation configuration
        public MiracleActivationConfig activationConfig = new MiracleActivationConfig();
        
        // Effect blocks (ordered list of gameplay effects)
        public List<MiracleEffectBlock> effectBlocks = new List<MiracleEffectBlock>();
        
        // Synergy tags (which miracles this synergizes with)
        public List<string> synergyTags = new List<string>();
        
        public MiracleAffinity affinityMods = new MiracleAffinity(); // Affinity modifications
        
        // Legacy effects dictionary (kept for compatibility)
        public Dictionary<string, float> effects = new Dictionary<string, float>();
        public List<string> requiredReagents = new List<string>();
    }

    /// <summary>
    /// Resource node template with expanded Phase 3 fields.
    /// </summary>
    [Serializable]
    public class ResourceNodeTemplate : PrefabTemplate
    {
        public int resourceTypeIndex = 0; // Ties to MaterialTemplate ID
        public float capacity = 100f;
        public float regrowthRate = 0f; // Units per day
        public int regrowthSeed = 0; // For deterministic regrowth
        
        // Harvest tool requirements (rule-based)
        public MaterialUsage requiredToolUsage = MaterialUsage.Tool;
        public MaterialStats minToolStats = new MaterialStats();
        public MaterialTraits requiredToolTraits = MaterialTraits.None;
        
        public HazardClass hazard = HazardClass.None;
        public Footprint footprint = new Footprint();
    }

    /// <summary>
    /// Container/logistics template with expanded Phase 3 fields.
    /// </summary>
    [Serializable]
    public class ContainerTemplate : PrefabTemplate
    {
        public float capacityUnits = 100f;
        public List<PackageClass> acceptedPackageClasses = new List<PackageClass>();
        public float throughputRate = 10f; // Units per second
        
        // Decay/spoilage policy
        public bool appliesSpoilage = false;
        public float spoilageMultiplier = 1f; // Multiplies material spoilage rate
        
        public Footprint footprint = new Footprint();
    }
}

