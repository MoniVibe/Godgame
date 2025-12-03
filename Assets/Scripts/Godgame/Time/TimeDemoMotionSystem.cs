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
    /// Advances the demo actor in record mode so rewind playback has deterministic state transitions.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(RecordSimulationSystemGroup))]
    public partial struct TimeDemoMotionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeDemoConfig>();
            state.RequireForUpdate<TimeDemoState>();
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

            var config = SystemAPI.GetSingleton<TimeDemoConfig>();
            float dt = math.max(timeState.FixedDeltaTime, 0f);

            foreach (var (demoState, transform) in SystemAPI
                         .Query<RefRW<TimeDemoState>, RefRW<LocalTransform>>())
            {
                var current = demoState.ValueRO;
                current.Position += config.VelocityPerSecond * dt;
                current.Phase = math.fmod(current.Phase + config.PhaseRadiansPerSecond * dt, math.PI * 2f);
                current.LastAppliedTick = timeState.Tick;
                demoState.ValueRW = current;

                var updatedTransform = transform.ValueRO;
                updatedTransform.Position = current.Position;
                updatedTransform.Rotation = quaternion.AxisAngle(math.up(), current.Phase);
                transform.ValueRW = updatedTransform;
            }
        }
    }
}
