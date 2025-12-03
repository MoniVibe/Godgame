using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Runtime
{
    /// <summary>
    /// Component marking miracle target position and entity.
    /// Used by miracle systems to track where miracles are targeted.
    /// </summary>
    [BurstCompile]
    public struct MiracleTarget : IComponentData
    {
        public float3 TargetPosition;
        public Entity TargetEntity;
    }

    /// <summary>
    /// Per-player miracle selection & casting state. One singleton per hand/god.
    /// </summary>
    public struct MiracleCasterState : IComponentData
    {
        public Entity HandEntity;
        public byte SelectedSlot;        // 0-based index for miracle list
        public byte SustainedCastHeld;   // 1 = channeling
        public byte ThrowCastTriggered;  // 1 this frame
    }
}

