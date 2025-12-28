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
    /// Bridges new miracle system with existing rain cloud systems.
    /// When Rain miracle is activated, adds RainCloud component and converts to existing rain cloud format.
    /// </summary>
    [UpdateInGroup(typeof(MiracleEffectSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Miracles.MiracleActivationSystem))]
    public partial struct RainCloudMiracleSystem : ISystem
    {
        private TimeAwareController _controller;

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

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            // Find effect entities with Rain miracle that don't have RainCloud component yet
            foreach (var (effect, transform, entity) in SystemAPI.Query<RefRO<MiracleEffectNew>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (effect.ValueRO.Id == MiracleId.Rain && !SystemAPI.HasComponent<RainCloud>(entity))
                {
                    // Add RainCloud component (new miracle-specific component)
                    ecb.AddComponent(entity, new RainCloud
                    {
                        MoisturePool = 200f, // Default pool size
                        EmissionRate = 10f, // Default emission rate
                        Altitude = transform.ValueRO.Position.y,
                        GlideSpeed = 4f // Default glide speed
                    });

                    // Also add existing rain cloud components for compatibility
                    if (!SystemAPI.HasComponent<RainCloudTag>(entity))
                    {
                        ecb.AddComponent<RainCloudTag>(entity);
                    }

                    if (!SystemAPI.HasComponent<RainCloudConfig>(entity))
                    {
                        ecb.AddComponent(entity, new RainCloudConfig
                        {
                            BaseRadius = effect.ValueRO.Radius,
                            MinRadius = effect.ValueRO.Radius * 0.5f,
                            RadiusPerHeight = 0.1f,
                            MoisturePerSecond = 10f,
                            MoistureFalloff = 2f,
                            MoistureCapacity = 200f,
                            DefaultVelocity = new float3(0, 0, 0),
                            DriftNoiseStrength = 0.5f,
                            DriftNoiseFrequency = 0.1f,
                            FollowLerp = 0.1f
                        });
                    }

                    if (!SystemAPI.HasComponent<RainCloudState>(entity))
                    {
                        ecb.AddComponent(entity, new RainCloudState
                        {
                            MoistureRemaining = 200f,
                            ActiveRadius = effect.ValueRO.Radius,
                            Velocity = float3.zero,
                            AgeSeconds = 0f,
                            Flags = 0
                        });
                    }

                    // Set lifetime based on miracle effect
                    if (!SystemAPI.HasComponent<RainCloudLifetime>(entity))
                    {
                        ecb.AddComponent(entity, new RainCloudLifetime
                        {
                            SecondsRemaining = effect.ValueRO.RemainingSeconds
                        });
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

