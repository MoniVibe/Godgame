using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tunables for headless movement diagnostics.
    /// </summary>
    public struct VillagerMovementDiagnosticsConfig : IComponentData
    {
        public float MaxSpeed;
        public float TeleportDistance;
        public int StuckTickThreshold;
        public float ProgressEpsilon;
        public int MaxAllowedSpeedViolations;
        public int MaxAllowedTeleports;
        public int MaxAllowedStuckEvents;
        public int MaxAllowedStateFlips;
        public int TraceEventLimit;
        public int TraceSampleStride;

        public static VillagerMovementDiagnosticsConfig Default => new VillagerMovementDiagnosticsConfig
        {
            MaxSpeed = 12f,
            TeleportDistance = 8f,
            StuckTickThreshold = 120,
            ProgressEpsilon = 0.05f,
            MaxAllowedSpeedViolations = 5,
            MaxAllowedTeleports = 2,
            MaxAllowedStuckEvents = 4,
            MaxAllowedStateFlips = 24,
            TraceEventLimit = 12,
            TraceSampleStride = 4
        };
    }
}
