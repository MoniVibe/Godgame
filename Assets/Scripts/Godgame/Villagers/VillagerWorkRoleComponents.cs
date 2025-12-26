using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    public enum VillagerWorkRoleKind : byte
    {
        None = 0,
        Forester = 1,
        Miner = 2,
        Farmer = 3,
        Builder = 4,
        Breeder = 5,
        Hauler = 6
    }

    /// <summary>
    /// Assigned villager work role used by the basic job loop.
    /// </summary>
    public struct VillagerWorkRole : IComponentData
    {
        public VillagerWorkRoleKind Value;
    }

    /// <summary>
    /// Optional override that locks a villager to a specific work role.
    /// </summary>
    public struct VillagerWorkRoleOverride : IComponentData
    {
        public VillagerWorkRoleKind Value;
    }

    /// <summary>
    /// Hauling preference for villagers who sometimes deliver their own goods.
    /// </summary>
    public struct VillagerHaulPreference : IComponentData
    {
        public float HaulChance;
        public float HaulCooldownSeconds;
        public uint NextHaulAllowedTick;
        public byte ForceHaul;
    }

    /// <summary>
    /// Marks a villager's hauling preference as manually overridden.
    /// </summary>
    public struct VillagerHaulPreferenceOverride : IComponentData
    {
    }

    /// <summary>
    /// Resource mapping and hauling tuning for villager work roles.
    /// </summary>
    public struct VillagerWorkTuning : IComponentData
    {
        public FixedString64Bytes ForesterInputId;
        public FixedString64Bytes ForesterOutputId;
        public FixedString64Bytes MinerOutputId;
        public FixedString64Bytes FarmerOutputId;

        public ushort ForesterInputIndex;
        public ushort ForesterOutputIndex;
        public ushort MinerOutputIndex;
        public ushort FarmerOutputIndex;

        public float HaulChance;
        public float HaulCooldownSeconds;
        public float PileDropMinUnits;
        public float PileDropMaxUnits;
        public float PilePickupMinUnits;
        public float PileSearchRadius;

        public uint LastResolvedTick;
    }
}
