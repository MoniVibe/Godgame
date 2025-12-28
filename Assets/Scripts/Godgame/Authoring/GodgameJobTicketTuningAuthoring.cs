using Godgame.Villagers;
using UnityEngine;
using Unity.Entities;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for job ticket tuning defaults.
    /// </summary>
    public sealed class GodgameJobTicketTuningAuthoring : MonoBehaviour
    {
        [Min(0)] public int maxConcurrentNodeTickets = 2;
        [Min(0)] public int maxConcurrentPileTickets = 3;
        [Min(0)] public int openTicketBuffer = 1;
        [Min(0f)] public float chunkDefaultUnits = 25f;
        [Min(0f)] public float chunkMinUnits = 5f;
        [Min(0f)] public float chunkMaxUnits = 60f;
        [Min(1)] public int maxBatchTickets = 3;
        [Min(0)] public int maxAttachPerTick = 1;
        [Min(0f)] public float maxBatchWorkUnits = 110f;
        [Min(0f)] public float attachRadius = 18f;
        [Min(0f)] public float claimTTLSeconds = 6f;
        [Min(0f)] public float minCommitSeconds = 2f;
        [Min(0f)] public float heavyCarryThresholdUnits = 200f;
        [Min(1)] public int heavyCarryRequiredWorkers = 3;
        [Min(1)] public int heavyCarryMinWorkers = 2;
        [Min(0f)] public float heavyCarryAssemblyRadius = 6f;
        public float groupCohesionBase = 0.05f;
        public float groupCohesionOrderWeight = 0.15f;
        public float groupCohesionMin = -0.25f;
        public float groupCohesionMax = 0.5f;

        private sealed class Baker : Baker<GodgameJobTicketTuningAuthoring>
        {
            public override void Bake(GodgameJobTicketTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GodgameJobTicketTuning
                {
                    MaxConcurrentNodeTickets = Mathf.Max(0, authoring.maxConcurrentNodeTickets),
                    MaxConcurrentPileTickets = Mathf.Max(0, authoring.maxConcurrentPileTickets),
                    OpenTicketBuffer = Mathf.Max(0, authoring.openTicketBuffer),
                    ChunkDefaultUnits = Mathf.Max(0f, authoring.chunkDefaultUnits),
                    ChunkMinUnits = Mathf.Max(0f, authoring.chunkMinUnits),
                    ChunkMaxUnits = Mathf.Max(0f, authoring.chunkMaxUnits),
                    MaxBatchTickets = Mathf.Max(1, authoring.maxBatchTickets),
                    MaxAttachPerTick = Mathf.Max(0, authoring.maxAttachPerTick),
                    MaxBatchWorkUnits = Mathf.Max(0f, authoring.maxBatchWorkUnits),
                    AttachRadius = Mathf.Max(0f, authoring.attachRadius),
                    ClaimTTLSeconds = Mathf.Max(0f, authoring.claimTTLSeconds),
                    MinCommitSeconds = Mathf.Max(0f, authoring.minCommitSeconds),
                    HeavyCarryThresholdUnits = Mathf.Max(0f, authoring.heavyCarryThresholdUnits),
                    HeavyCarryRequiredWorkers = Mathf.Max(1, authoring.heavyCarryRequiredWorkers),
                    HeavyCarryMinWorkers = Mathf.Max(1, authoring.heavyCarryMinWorkers),
                    HeavyCarryAssemblyRadius = Mathf.Max(0f, authoring.heavyCarryAssemblyRadius),
                    GroupCohesionBase = authoring.groupCohesionBase,
                    GroupCohesionOrderWeight = authoring.groupCohesionOrderWeight,
                    GroupCohesionMin = authoring.groupCohesionMin,
                    GroupCohesionMax = authoring.groupCohesionMax
                });
            }
        }
    }
}
