using Godgame.Miracles;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Miracles;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Miracles
{
    /// <summary>
    /// Applies local time scales to entities based on TimeDistortion bubbles.
    /// Entities inside time distortion bubbles get LocalTimeScale component.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(MiracleEffectSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Miracles.MiracleActivationSystem))]
    public partial struct TimeDistortionApplySystem : ISystem
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

            // Collect all active time distortion bubbles
            var distortions = new NativeList<(float3 Center, float Radius, float TimeScale)>(16, Allocator.Temp);
            
            // Find effect entities with Temporal Veil that need TimeDistortion component
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (effect, transform, entity) in SystemAPI.Query<RefRO<MiracleEffectNew>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (effect.ValueRO.Id == MiracleId.TemporalVeil && !SystemAPI.HasComponent<TimeDistortion>(entity))
                {
                    // Add TimeDistortion component
                    ecb.AddComponent(entity, new TimeDistortion
                    {
                        Center = transform.ValueRO.Position,
                        Radius = effect.ValueRO.Radius,
                        TimeScale = 0.5f, // Default slow mode (can be adjusted based on intensity)
                        Mode = 0 // Slow mode
                    });
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            // Now collect active distortions
            foreach (var (distortion, effect, transform) in SystemAPI.Query<RefRO<TimeDistortion>, RefRO<MiracleEffectNew>, RefRO<LocalTransform>>())
            {
                if (effect.ValueRO.RemainingSeconds > 0f)
                {
                    distortions.Add((
                        distortion.ValueRO.Center,
                        distortion.ValueRO.Radius,
                        distortion.ValueRO.TimeScale
                    ));
                }
            }

            if (distortions.Length == 0)
            {
                // No active distortions - reset all LocalTimeScale to 1.0
                foreach (var (timeScale, entity) in SystemAPI.Query<RefRW<LocalTimeScale>>().WithEntityAccess())
                {
                    timeScale.ValueRW.Value = 1.0f;
                }
                distortions.Dispose();
                return;
            }

            // Apply time scales to entities with LocalTransform (villagers, projectiles, etc.)
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithEntityAccess())
            {
                float3 position = transform.ValueRO.Position;
                float finalTimeScale = 1.0f;

                // Check if entity is inside any distortion bubble
                // For now: use nearest/first bubble (later can define stacking rules)
                float closestDist = float.MaxValue;
                for (int i = 0; i < distortions.Length; i++)
                {
                    var dist = distortions[i];
                    float3 delta = position - dist.Center;
                    delta.y = 0f; // Only check horizontal distance
                    float distance = math.length(delta);
                    
                    if (distance <= dist.Radius && distance < closestDist)
                    {
                        closestDist = distance;
                        finalTimeScale = dist.TimeScale;
                    }
                }

                // Apply LocalTimeScale component
                if (SystemAPI.HasComponent<LocalTimeScale>(entity))
                {
                    var timeScale = SystemAPI.GetComponentRW<LocalTimeScale>(entity);
                    timeScale.ValueRW.Value = finalTimeScale;
                }
                else if (finalTimeScale != 1.0f)
                {
                    // Only add component if time scale is not normal (optimization)
                    state.EntityManager.AddComponentData(entity, new LocalTimeScale { Value = finalTimeScale });
                }
            }

            distortions.Dispose();
        }
    }
}

