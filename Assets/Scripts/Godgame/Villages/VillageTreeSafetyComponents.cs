using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Aggregate learning state for tree felling safety within a village.
    /// </summary>
    public struct VillageTreeSafetyMemory : IComponentData
    {
        public float CautionBias;
        public float RecentSeverity;
        public uint LastIncidentTick;
        public uint IncidentCount;
    }
}
