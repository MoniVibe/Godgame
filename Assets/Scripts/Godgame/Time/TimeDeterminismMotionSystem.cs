using Godgame.Time;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Time
{
    /// <summary>
    /// Advances the determinism actor in record mode so rewind playback has deterministic state transitions.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(RecordSimulationSystemGroup))]
    public partial struct TimeDeterminismMotionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeDeterminismTag>();
            state.RequireForUpdate<TimeDeterminismConfig>();
            state.RequireForUpdate<TimeDeterminismState>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var config = SystemAPI.GetSingleton<TimeDeterminismConfig>();
            float dt = math.max(timeState.FixedDeltaTime, 0f);

            foreach (var (determinismState, transform) in SystemAPI
                         .Query<RefRW<TimeDeterminismState>, RefRW<LocalTransform>>())
            {
                var current = determinismState.ValueRO;
                current.Position += config.VelocityPerSecond * dt;
                current.Phase = math.fmod(current.Phase + config.PhaseRadiansPerSecond * dt, math.PI * 2f);
                current.LastAppliedTick = timeState.Tick;
                determinismState.ValueRW = current;

                var updatedTransform = transform.ValueRO;
                updatedTransform.Position = current.Position;
                updatedTransform.Rotation = quaternion.AxisAngle(math.up(), current.Phase);
                transform.ValueRW = updatedTransform;
            }
        }
    }
}
