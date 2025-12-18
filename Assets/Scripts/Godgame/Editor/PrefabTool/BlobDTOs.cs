using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Godgame.Scenario;

namespace Godgame.Editor.PrefabTool
{
    /// <summary>
    /// Data Transfer Objects for blob baking pipeline.
    /// These structures mirror the runtime blob definitions from items.md spec.
    /// Used to serialize template data for conversion to BlobAssetReference during baking.
    /// </summary>

    /// <summary>
    /// DTO for MaterialDefinitionBlob (per items.md spec).
    /// </summary>
    [System.Serializable]
    public class MaterialDefinitionDTO
    {
        public ushort Id; // MaterialId.Value
        public float BaseQuality;  // 0-100
        public float BasePurity;   // 0-100
        public Rarity BaseRarity;
        public byte TechTier;      // Min tech tier to extract/use
        
        // Physical stats
        public float Hardness;
        public float Toughness;
        public float Density;
        public float MeltingPoint;
        public float Conductivity;
        
        // Usage weights (simplified for now, can expand per items.md spec)
        public float BowFlexibilityScore;
        public float BowTensionScore;
        public float ShieldToughnessScore;
        public float ClubMassScore;
        
        // Production facilities
        public List<string> ProductionFacilities = new List<string>();
        
        // Editor metadata (not baked to blob)
        public string Name;
        public string DisplayName;
        public MaterialCategory Category;
        public MaterialUsage Usage;
        public MaterialTraits Traits;
    }

    /// <summary>
    /// DTO for ItemDefinitionBlob (per items.md spec).
    /// </summary>
    [System.Serializable]
    public class ItemDefinitionDTO
    {
        public ushort Id; // ItemDefId.Value
        
        // Classification
        public byte ItemType;      // Weapon / Armor / Tool / Reagent / Accessory / Other
        public byte SlotKind;      // Hand / Body / Head / Feet / Accessory / None
        
        // Base quality & rarity
        public float BaseQuality;      // 0-100
        public Rarity BaseRarity;      // Lower bound from template
        public byte TechTier;          // Tier to which this item belongs
        public byte RequiredTechTier;  // Min tier to craft/use
        
        // Base stats (unscaled)
        public float BaseDamage;
        public float BaseArmor;
        public float BaseDurability;
        public float BaseWeight;
        
        // Material/stat modifiers (weights)
        public float HardnessWeightForDamage;
        public float HardnessWeightForArmor;
        public float ToughnessWeightForDurability;
        public float DensityWeightForWeight;
        
        // Flags (usage, categories) could be bitmasks:
        public uint UsageFlags;        // Bits: Weapon, Armor, Tool, Container, etc.
        
        // Production facilities
        public List<string> ProductionFacilities = new List<string>();
        
        // Editor metadata (not baked to blob)
        public string Name;
        public string DisplayName;
    }

    /// <summary>
    /// DTO for ProductionRecipeBlob (per items.md spec).
    /// </summary>
    [System.Serializable]
    public class ProductionRecipeDTO
    {
        public ushort RecipeId; // RecipeId.Value
        public ushort OutputItemDefId; // ItemDefId.Value of produced item
        
        // Inputs
        public List<ProductionInputDTO> Inputs = new List<ProductionInputDTO>();
        
        // Quality derivation weights
        public QualityWeights QualityWeights;
        public float BaseFacilityQuality; // 0-100
        
        // Tech tier gate
        public byte RequiredTechTier;
        
        // Editor metadata (not baked to blob)
        public string Name;
        public string DisplayName;
    }

    /// <summary>
    /// DTO for ProductionInputBlob (per items.md spec).
    /// </summary>
    [System.Serializable]
    public class ProductionInputDTO
    {
        public ushort MaterialId; // MaterialId.Value
        public float Quantity;
        public float MinPurity;   // 0-100
        public float MinQuality;   // 0-100
        public byte MinTechTier;  // 0-10
        public bool IsOptional;
        
        // Editor metadata (not baked to blob)
        public string MaterialName; // For reference
    }

    /// <summary>
    /// DTO for MiracleEffectBlock (gameplay logic parameters).
    /// </summary>
    [System.Serializable]
    public class MiracleEffectBlockDTO
    {
        public string EffectName;
        public int EffectType; // MiracleEffectType enum value
        public int Phase; // MiracleEffectPhase enum value
        
        // Target filtering
        public int TargetType; // MiracleTargetType enum value
        public bool FriendlyFire;
        public bool RequiresLineOfSight;
        public float Range;
        
        // Area of effect
        public int AreaShape; // AreaShape enum value
        public Vector2 AreaSize;
        
        // Timing
        public float Duration;
        public float TickInterval;
        
        // Damage parameters
        public float DamageAmount;
        public float ArmorPenetration;
        public float DamagePerSecond;
        
        // Healing parameters
        public float HealAmount;
        public float HealPerTick;
        public float MaxHeal;
        public float CleanseStrength;
        public bool OverhealGivesBuff;
        
        // Status effect parameters
        public int StatusType; // MiracleStatusType enum value
        public float StatusDuration;
        public float StatusSeverity;
        public bool StatusStacks;
        public int MaxStacks;
        
        // Environmental parameters
        public float MoistureAmount;
        public float FireSuppression;
        public float SpreadChance;
        public float SpeedMultiplier;
        
        // Knockback parameters
        public float KnockbackForce;
        public bool RadialKnockback;
        
        // Synergy parameters
        public List<string> TriggersOnMiracleTypes = new List<string>();
        public string SynergyEffect;
        
        // Special parameters
        public float ComaReduction;
        public float ManaDebtReduction;
        public float LashbackChance;
        public float LashbackManaDebtIncrease;
        
        // Chain/Propagation
        public bool CanChain;
        public float ChainRange;
        public float ChainDamageDecay;
        public int MaxChains;
        
        // Scaling
        public bool ScalesWithFaithDensity;
        public float FaithDensityMultiplier;
        public bool ScalesWithFocus;
        public float FocusMultiplier;
    }

    /// <summary>
    /// DTO for MiracleActivationConfig.
    /// </summary>
    [System.Serializable]
    public class MiracleActivationConfigDTO
    {
        public bool SupportsSustain;
        public bool SupportsThrow;
        
        // Base costs
        public float BasePrayerCost;
        public float BaseManaCost;
        public float BaseFocusCost;
        
        // Sustained costs
        public float SustainPrayerCostPerSecond;
        public float SustainManaCostPerSecond;
        public float SustainFocusCostPerSecond;
        
        // Cooldown
        public float Cooldown;
        
        // Faith discount
        public bool FaithDensityReducesCost;
        public float MaxFaithDiscount;
        
        // Alignment modifiers
        public float EvilAlignmentCostMultiplier;
        public float BenevolentAlignmentCostMultiplier;
    }

    /// <summary>
    /// DTO for MiracleDefinitionBlob (runtime miracle data).
    /// </summary>
    [System.Serializable]
    public class MiracleDefinitionDTO
    {
        public ushort Id; // Miracle ID (runtime will assign)
        
        // Activation configuration
        public MiracleActivationConfigDTO ActivationConfig = new MiracleActivationConfigDTO();
        
        // Effect blocks (ordered list)
        public List<MiracleEffectBlockDTO> EffectBlocks = new List<MiracleEffectBlockDTO>();
        
        // Synergy tags
        public List<string> SynergyTags = new List<string>();
        
        // Legacy fields (for compatibility)
        public float ManaCost;
        public float Cooldown;
        public float Range;
        public float Duration;
        
        // Area shape
        public int AreaShape; // AreaShape enum value
        public Vector2 AreaSize;
        
        // Editor metadata (not baked to blob)
        public string Name;
        public string DisplayName;
        public string Description;
    }

    /// <summary>
    /// DTO for IndividualDefinitionBlob (runtime individual/villager data).
    /// </summary>
    [System.Serializable]
    public class IndividualDefinitionDTO
    {
        public ushort Id; // Individual ID (runtime will assign)
        
        // Core Attributes (Experience Modifiers)
        public float Physique; // 0-100
        public float Finesse; // 0-100
        public float Will; // 0-100
        public float Wisdom; // 0-100
        
        // Derived Attributes
        public float Strength; // 0-100
        public float Agility; // 0-100
        public float Intelligence; // 0-100
        
        // Social Stats
        public float Fame; // 0-1000
        public float Wealth; // Currency
        public float Reputation; // -100 to +100
        public float Glory; // 0-1000
        public float Renown; // 0-1000
        
        // Combat Stats (Base values, calculated from attributes at runtime)
        public float BaseAttack; // 0-100 (0 = auto-calculate)
        public float BaseDefense; // 0-100 (0 = auto-calculate)
        public float BaseHealth; // HP (0 = auto-calculate)
        public float BaseStamina; // Rounds (0 = auto-calculate)
        public float BaseMana; // 0-100 (0 = auto-calculate, 0 = non-magic user)
        
        // Need Stats (Starting values)
        public float Food; // 0-100
        public float Rest; // 0-100
        public float Sleep; // 0-100
        public float GeneralHealth; // 0-100
        
        // Resistances (Dictionary serialized as lists for JSON compatibility)
        public List<string> ResistanceTypes = new List<string>(); // Keys
        public List<float> ResistanceValues = new List<float>(); // Values (0.0-1.0)
        
        // Healing & Spell Modifiers
        public float HealBonus; // Multiplier (default 1.0)
        public float SpellDurationModifier; // Multiplier (default 1.0)
        public float SpellIntensityModifier; // Multiplier (default 1.0)
        
        // Limb System (Simplified - full limb system to be implemented separately)
        public List<string> LimbIds = new List<string>(); // Limb references
        public List<float> LimbHealths = new List<float>(); // Per-limb health (0-100%)
        
        // Implants (Simplified)
        public List<string> ImplantIds = new List<string>(); // Implant references
        
        // Outlooks & Alignments
        public string AlignmentId; // Reference to alignment definition
        public List<string> OutlookIds = new List<string>(); // References to outlook definitions
        
        // Disposition (Dictionary serialized as lists)
        public List<string> DispositionTypes = new List<string>(); // Keys (Loyalty, Fear, etc.)
        public List<float> DispositionValues = new List<float>(); // Values (-100 to +100)
        
        // Titles (Deeds of Rulership)
        public List<TitleDTO> Titles = new List<TitleDTO>();
        
        // Editor metadata (not baked to blob)
        public string Name;
        public string DisplayName;
        public string Description;
        public int IndividualType; // IndividualTemplate.IndividualType enum value
        public int FactionId;
    }
    
    /// <summary>
    /// DTO for Title (deed of rulership).
    /// </summary>
    [System.Serializable]
    public class TitleDTO
    {
        public string TitleName = "";
        public string DisplayName = "";
        public TitleType TitleType = TitleType.Hero;
        public string AssociatedSettlement = "";
        public string AssociatedBuilding = "";
        public TitleAcquisitionMethod AcquisitionMethod = TitleAcquisitionMethod.Founding;
        public bool IsInheritable = true;
        public float MinRenown = 0f;
        public int TitleLevel = 1;
        public TitleStatus Status = TitleStatus.Active;
        public string LossReason = "";
        public TitleBonusesDTO Bonuses = new TitleBonusesDTO();
        public TitleBonusesDTO FormerBonuses = null; // Reduced bonuses for former titles
        public string Description = "";
    }
    
    /// <summary>
    /// DTO for Title bonuses.
    /// </summary>
    [System.Serializable]
    public class TitleBonusesDTO
    {
        public float ReputationBonus = 0f;
        public float FameBonus = 0f;
        public float WealthBonus = 0f;
        public float RenownBonus = 0f;
        public float GloryBonus = 0f;
        public float SocialStandingModifier = 1.0f;
        public int AuthorityLevel = 0;
    }

    /// <summary>
    /// Export catalog containing all DTOs ready for blob baking.
    /// </summary>
    [System.Serializable]
    public class PrefabCatalogExport
    {
        public List<MaterialDefinitionDTO> Materials = new List<MaterialDefinitionDTO>();
        public List<ItemDefinitionDTO> Items = new List<ItemDefinitionDTO>();
        public List<ProductionRecipeDTO> Recipes = new List<ProductionRecipeDTO>();
        public List<MiracleDefinitionDTO> Miracles = new List<MiracleDefinitionDTO>();
        public List<IndividualDefinitionDTO> Individuals = new List<IndividualDefinitionDTO>();
        
        // Export metadata
        public string ExportDate;
        public int MaterialCount => Materials.Count;
        public int ItemCount => Items.Count;
        public int RecipeCount => Recipes.Count;
        public int MiracleCount => Miracles.Count;
        public int IndividualCount => Individuals.Count;
    }
}
