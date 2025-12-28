using Unity.Entities;

namespace Godgame.Villagers
{
    public enum VillagerLeisureAction : byte
    {
        None = 0,
        Wander = 1,
        Tidy = 2,
        Observe = 3,
        Socialize = 4
    }

    /// <summary>
    /// Tracks leisure cadence state during work cooldowns.
    /// </summary>
    public struct VillagerLeisureState : IComponentData
    {
        public uint CooldownStartTick;
        public uint CadenceTicks;
        public uint EpisodeIndex;
        public byte RerollCount;
        public VillagerLeisureAction Action;
        public Entity ActionTarget;
    }
}
