using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Captures separation pressure for crowding-aware leisure rerolls.
    /// </summary>
    public struct VillagerCrowdingState : IComponentData
    {
        public float Pressure;
        public uint LastSampleTick;
    }
}
