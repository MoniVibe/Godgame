using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class VillagerMovementDiagnosticsAuthoring : MonoBehaviour
    {
        public float maxSpeed = 12f;
        public float teleportDistance = 8f;
        public int stuckTickThreshold = 120;
        public float progressEpsilon = 0.05f;
        public int maxAllowedSpeedViolations = 5;
        public int maxAllowedTeleports = 2;
        public int maxAllowedStuckEvents = 4;
        public int maxAllowedStateFlips = 24;
        public int traceEventLimit = 12;
        public int traceSampleStride = 4;

        private sealed class Baker : Baker<VillagerMovementDiagnosticsAuthoring>
        {
            public override void Bake(VillagerMovementDiagnosticsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerMovementDiagnosticsConfig
                {
                    MaxSpeed = Mathf.Max(0.1f, authoring.maxSpeed),
                    TeleportDistance = Mathf.Max(0.1f, authoring.teleportDistance),
                    StuckTickThreshold = Mathf.Max(1, authoring.stuckTickThreshold),
                    ProgressEpsilon = Mathf.Max(0.001f, authoring.progressEpsilon),
                    MaxAllowedSpeedViolations = Mathf.Max(0, authoring.maxAllowedSpeedViolations),
                    MaxAllowedTeleports = Mathf.Max(0, authoring.maxAllowedTeleports),
                    MaxAllowedStuckEvents = Mathf.Max(0, authoring.maxAllowedStuckEvents),
                    MaxAllowedStateFlips = Mathf.Max(0, authoring.maxAllowedStateFlips),
                    TraceEventLimit = Mathf.Max(4, authoring.traceEventLimit),
                    TraceSampleStride = Mathf.Max(1, authoring.traceSampleStride)
                });
            }
        }
    }
}
