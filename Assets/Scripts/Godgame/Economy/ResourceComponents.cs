using Unity.Entities;
using Unity.Collections;

namespace Godgame.Economy
{
    /// <summary>
    /// Resource types for extracted materials (raw resources with purity only).
    /// </summary>
    public enum ResourceType : byte
    {
        None = 0,

        // Ores
        IronOre = 1,
        CopperOre = 2,
        TinOre = 3,
        SilverOre = 4,
        GoldOre = 5,
        MithrilOre = 6,
        AdamantiteOre = 7,

        // Wood
        Oak = 10,
        Pine = 11,
        Birch = 12,
        Ebony = 13,
        Ironwood = 14,

        // Stone
        Limestone = 20,
        Granite = 21,
        Marble = 22,
        Obsidian = 23,

        // Herbs
        Aloe = 30,
        Ginseng = 31,
        Nightshade = 32,
        Mandrake = 33,

        // Agricultural
        Wheat = 40,
        Barley = 41,
        Rye = 42,
        Cotton = 43,
        Flax = 44,
    }

    /// <summary>
    /// Material types for produced materials (refined resources with quality/rarity/tech tier).
    /// </summary>
    public enum MaterialType : byte
    {
        None = 0,

        // Metal Ingots
        IronIngot = 1,
        SteelIngot = 2,
        CopperIngot = 3,
        BronzeIngot = 4,
        SilverIngot = 5,
        GoldIngot = 6,
        MithrilIngot = 7,
        AdamantiteIngot = 8,

        // Lumber
        OakLumber = 10,
        PineLumber = 11,
        BirchLumber = 12,
        EbonyLumber = 13,
        IronwoodLumber = 14,

        // Cut Stone
        LimestoneBrick = 20,
        GraniteBrick = 21,
        MarbleBrick = 22,
        ObsidianBrick = 23,

        // Processed Herbs
        HealingPoultice = 30,
        GinsengExtract = 31,
        NightshadePoison = 32,
        MandrakeEssence = 33,

        // Processed Agricultural
        Flour = 40,
        Bread = 41,
        Cloth = 42,
        Linen = 43,
        Leather = 44,
    }

    /// <summary>
    /// Product types for end products (complex crafted goods).
    /// </summary>
    public enum ProductType : byte
    {
        None = 0,

        // Weapons
        Weapon = 1,

        // Armor
        Armor = 2,

        // Vehicles
        Wagon = 3,
        Carriage = 4,
        Stagecoach = 5,

        // Buildings
        Building = 6,

        // Potions
        Potion = 7,

        // Enchantments
        Enchantment = 8,
        Rune = 9,
        Jewel = 10,
    }

    /// <summary>
    /// Rarity tier for materials and products.
    /// Determines unique affixes and stat bonuses.
    /// </summary>
    public enum Rarity : byte
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
    }

    /// <summary>
    /// Extracted resource component (raw materials with purity only).
    /// Purity determines yield percentage when refining.
    /// Example: 100 iron ore with 69% purity → 69 iron ingots.
    /// </summary>
    public struct ExtractedResource : IComponentData
    {
        public ResourceType Type;               // Ore, Wood, Stone, Herbs, Grain
        public byte Purity;                     // 0-100% (yield percentage)
        public ushort Quantity;                 // Amount extracted (kg, units)
        public Entity ExtractorBusiness;        // Mine, Logging, Quarry, Herbalist, Farm
    }

    /// <summary>
    /// Produced material component (refined resources with quality/rarity/tech tier).
    /// Quality affects final product quality through propagation.
    /// Rarity determines unique affixes available.
    /// Tech tier determines base stats.
    /// </summary>
    public struct ProducedMaterial : IComponentData
    {
        public MaterialType Type;               // Ingots, Lumber, Cut Stone, Processed Herbs, Flour
        public byte Quality;                    // 1-100 (affects propagation to end products)
        public Rarity Rarity;                   // Common, Uncommon, Rare, Epic, Legendary
        public byte TechTier;                   // 0-10 (Bronze Age to Magitech)
        public ushort Quantity;                 // Amount produced
        public Entity ProducerBusiness;         // Blacksmith, Sawmill, Stonecutter, Herbalist, Mill
    }

    /// <summary>
    /// End product component (complex crafted goods).
    /// Final products assembled from multiple materials/components.
    /// </summary>
    public struct EndProduct : IComponentData
    {
        public ProductType Type;                // Weapon, Armor, Wagon, Building, Potion
        public FixedString64Bytes Name;         // "Longsword", "Plate Armor", "Healing Potion"
        public byte Quality;                    // 1-100 (inherited from materials + artisan skill)
        public Rarity Rarity;                   // Common, Uncommon, Rare, Epic, Legendary
        public byte TechTier;                   // 0-10
        public Entity CrafterBusiness;          // Weaponsmith, Armorer, Wainwright, Builder, Alchemist
    }

    /// <summary>
    /// Component assembly requirement for complex products.
    /// Example: Wagon requires wheels, axles, nuts/bolts, bed, hardware.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct ComponentRequirement : IBufferElementData
    {
        public MaterialType MaterialType;       // Type of material needed
        public byte QuantityRequired;           // Amount needed
        public byte MinQuality;                 // Minimum quality required (0 = no requirement)
        public Rarity MinRarity;                // Minimum rarity required
    }

    /// <summary>
    /// Material input for production (tracks what went into making this product).
    /// Used for quality propagation calculations.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct MaterialInput : IBufferElementData
    {
        public Entity MaterialEntity;           // Reference to material entity
        public MaterialType Type;               // Material type
        public byte Quality;                    // Quality of input material
        public byte QuantityUsed;               // Amount consumed in production
    }
}
