using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Aggregated oracle counters between telemetry exports.
    /// </summary>
    public struct GodgameOracleAccumulator : IComponentData
    {
        public uint LastHaulTripIndex;
        public uint LastGatherYieldIndex;
        public float PonderFreezeSeconds;
        public float GatheredAmount;
        public float HauledAmount;
        public float SampleSeconds;
        public uint SampleTicks;
    }

    /// <summary>
    /// Storehouse loop duration samples (ticks).
    /// </summary>
    [InternalBufferCapacity(256)]
    public struct GodgameOracleLoopSample : IBufferElementData
    {
        public uint Value;
    }
}
