using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Time
{
    /// <summary>
    /// Configuration for the demo rewind actor movement.
    /// </summary>
    public struct TimeDemoConfig : IComponentData
    {
        public float3 VelocityPerSecond;
        public float PhaseRadiansPerSecond;
    }

    /// <summary>
    /// Simple state used to prove rewind/resume determinism.
    /// </summary>
    public struct TimeDemoState : IComponentData
    {
        public float3 Position;
        public float Phase;
        public uint LastAppliedTick;
    }
}
