using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Configurable tuning for spawning chunked job tickets.
    /// </summary>
    public struct GodgameJobTicketTuning : IComponentData
    {
        public int MaxConcurrentNodeTickets;
        public int MaxConcurrentPileTickets;
        public int OpenTicketBuffer;
        public float ChunkDefaultUnits;
        public float ChunkMinUnits;
        public float ChunkMaxUnits;
        public int MaxBatchTickets;
        public int MaxAttachPerTick;
        public float MaxBatchWorkUnits;
        public float AttachRadius;
        public float ClaimTTLSeconds;
        public float HeavyCarryThresholdUnits;
        public int HeavyCarryRequiredWorkers;
        public int HeavyCarryMinWorkers;
        public float HeavyCarryAssemblyRadius;
        public float GroupCohesionBase;
        public float GroupCohesionOrderWeight;
        public float GroupCohesionMin;
        public float GroupCohesionMax;

        public static GodgameJobTicketTuning Default => new GodgameJobTicketTuning
        {
            MaxConcurrentNodeTickets = 2,
            MaxConcurrentPileTickets = 3,
            OpenTicketBuffer = 1,
            ChunkDefaultUnits = 25f,
            ChunkMinUnits = 5f,
            ChunkMaxUnits = 60f,
            MaxBatchTickets = 3,
            MaxAttachPerTick = 1,
            MaxBatchWorkUnits = 110f,
            AttachRadius = 18f,
            ClaimTTLSeconds = 6f,
            HeavyCarryThresholdUnits = 200f,
            HeavyCarryRequiredWorkers = 3,
            HeavyCarryMinWorkers = 2,
            HeavyCarryAssemblyRadius = 6f,
            GroupCohesionBase = 0.05f,
            GroupCohesionOrderWeight = 0.15f,
            GroupCohesionMin = -0.25f,
            GroupCohesionMax = 0.5f
        };
    }
}
