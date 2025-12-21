using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Scripting.APIUpdating;

namespace Godgame.Time
{
    /// <summary>
    /// Tag used to opt into time determinism systems.
    /// </summary>
    [MovedFrom(true, "Godgame.Time", null, "LegacyTimeDemoTag")]
    public struct TimeDeterminismTag : IComponentData
    {
    }

    /// <summary>
    /// Configuration for the determinism rewind actor movement.
    /// </summary>
    [MovedFrom(true, "Godgame.Time", null, "TimeDemoConfig")]
    public struct TimeDeterminismConfig : IComponentData
    {
        public float3 VelocityPerSecond;
        public float PhaseRadiansPerSecond;
    }

    /// <summary>
    /// Simple state used to prove rewind/resume determinism.
    /// </summary>
    [MovedFrom(true, "Godgame.Time", null, "TimeDemoState")]
    public struct TimeDeterminismState : IComponentData
    {
        public float3 Position;
        public float Phase;
        public uint LastAppliedTick;
    }
}
