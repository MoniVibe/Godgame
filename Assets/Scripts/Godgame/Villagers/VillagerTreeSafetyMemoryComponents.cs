using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tracks individual learning from tree felling incidents.
    /// </summary>
    public struct VillagerTreeSafetyMemory : IComponentData
    {
        public float CautionBias;
        public float RecentSeverity;
        public uint LastIncidentTick;
        public uint NextIncidentAllowedTick;
        public byte IncidentCount;
        public byte NearMissCount;
    }
}
