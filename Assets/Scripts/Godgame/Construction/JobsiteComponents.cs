using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Construction
{
    /// <summary>
    /// Authoring-less defaults for jobsite placement and progression.
    /// </summary>
    public struct JobsitePlacementConfig : IComponentData
    {
        public float3 StartPosition;
        public float3 PositionStep;
        public float DefaultRequiredProgress;
        public float BuildRatePerSecond;
        public int CompletionEffectId;
        public float CompletionEffectDuration;
        public FixedString64Bytes TelemetryKey;
    }

    /// <summary>
    /// Tracks state needed to generate unique jobsite ids and walk placement positions.
    /// </summary>
    public struct JobsitePlacementState : IComponentData
    {
        public int NextSiteId;
        public float3 NextPosition;
    }

    /// <summary>
    /// Buffer for spawning new jobsites without relying on input devices (hotkey or tests enqueue here).
    /// </summary>
    public struct JobsitePlacementRequest : IBufferElementData
    {
        public float3 Position;
    }

    /// <summary>
    /// Edge-triggered input state for spawning jobsites via hotkey or external router systems.
    /// </summary>
    public struct JobsitePlacementHotkeyState : IComponentData
    {
        public byte PlaceRequested;
    }

    /// <summary>
    /// Marks a construction site as a ghost under player control.
    /// </summary>
    public struct JobsiteGhost : IComponentData
    {
        public byte CompletionRequested;
    }

    /// <summary>
    /// Ensures completion effects/telemetry only fire once per site.
    /// </summary>
    public struct JobsiteCompletionTag : IComponentData
    {
    }

    /// <summary>
    /// Aggregated counters for construction telemetry.
    /// </summary>
    public struct JobsiteMetrics : IComponentData
    {
        public int CompletedCount;
    }
}
