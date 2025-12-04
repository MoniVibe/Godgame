using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// Singleton component for player-facing HUD data.
    /// Updated by Godgame_PlayerHUDSystem, read by PlayerHUD MonoBehaviour.
    /// </summary>
    public struct PlayerHUDData : IComponentData
    {
        /// <summary>Current scenario name/mode</summary>
        public FixedString64Bytes ScenarioName;
        /// <summary>Number of villages</summary>
        public int VillageCount;
        /// <summary>Number of villagers</summary>
        public int VillagerCount;
        /// <summary>Global "world health" indicator (0-1, aggregate of village states)</summary>
        public float WorldHealth;
    }

    /// <summary>
    /// Tag component identifying the player HUD singleton entity.
    /// </summary>
    public struct PlayerHUDSingleton : IComponentData { }
}

