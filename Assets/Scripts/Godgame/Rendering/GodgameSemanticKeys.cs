namespace Godgame.Rendering
{
    /// <summary>
    /// Centralized semantic key constants for Godgame entities.
    /// These values must match the Key values in RenderCatalogDefinition entries.
    /// </summary>
    public static class GodgameSemanticKeys
    {
        public const ushort VillagerMiner = 100;
        public const ushort VillagerFarmer = 101;
        public const ushort VillagerForester = 102;
        public const ushort VillagerBreeder = 103;
        public const ushort VillagerWorshipper = 104;
        public const ushort VillagerRefiner = 105;
        public const ushort VillagerPeacekeeper = 106;
        public const ushort VillagerCombatant = 107;

        /// <summary>
        /// Backwards compatible alias for generic villagers (defaults to Miner color).
        /// </summary>
        public const ushort Villager = VillagerMiner;

        public const ushort VillageCenter = 110;
        public const ushort ResourceChunk = 120;
        public const ushort Vegetation = 130;
        public const ushort ResourceNode = 140;
        public const ushort Storehouse = 150;
        public const ushort Housing = 151;
        public const ushort Worship = 152;
        public const ushort ConstructionGhost = 160;
        public const ushort Band = 170;
        public const ushort GhostTether = 280;
    }
}
