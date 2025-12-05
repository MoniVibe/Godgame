using Godgame.Miracles;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Miracles;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// Tracks temporal lashback risk and applies penalties for excessive Temporal Veil usage.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(MiracleEffectSystemGroup))]
    [UpdateAfter(typeof(TimeDistortionApplySystem))]
    public partial struct TemporalLashbackSystem : ISystem
    {
        private TimeAwareController _controller;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            _controller = new TimeAwareController(
                TimeAwareExecutionPhase.Record | TimeAwareExecutionPhase.CatchUp,
                TimeAwareExecutionOptions.SkipWhenPaused);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();

            if (!_controller.TryBegin(timeState, rewindState, out var context))
            {
                return;
            }

            // Only process in Record mode
            if (rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            float deltaTime = SystemAPI.Time.DeltaTime;

            // Check for Temporal Veil activations and increase risk
            foreach (var (effect, distortion) in SystemAPI.Query<RefRO<MiracleEffectNew>, RefRO<TimeDistortion>>())
            {
                if (effect.ValueRO.Id == MiracleId.TemporalVeil)
                {
                    // Find player/god entity (simplified - in practice would track caster)
                    // For MVP, we'll add risk to a singleton or track per-player
                    // This is a placeholder - actual implementation would track which player cast it
                }
            }

            // Update lashback risk decay and active state
            foreach (var (lashback, entity) in SystemAPI.Query<RefRW<TemporalLashback>>().WithEntityAccess())
            {
                var lash = lashback.ValueRO;
                
                // Decay risk over time
                lash.Risk = math.max(0f, lash.Risk - deltaTime * 0.01f); // Decay 1% per second
                
                // Check if risk threshold passed
                if (lash.Risk >= 0.8f && lash.Active == 0)
                {
                    // Activate lashback penalty
                    lash.Active = 1;
                }
                else if (lash.Risk < 0.3f && lash.Active == 1)
                {
                    // Deactivate when risk drops
                    lash.Active = 0;
                }
                
                lashback.ValueRW = lash;
            }
        }
    }
}



