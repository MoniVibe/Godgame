using Godgame.Construction;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Presentation;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Construction
{
    /// <summary>
    /// Emits presentation requests and telemetry when construction sites finish.
    /// </summary>
    [UpdateInGroup(typeof(ConstructionSystemGroup))]
    [UpdateAfter(typeof(JobsiteBuildSystem))]
    public partial struct JobsiteCompletionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobsiteMetrics>();
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<TelemetryStream>();
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

            var config = SystemAPI.GetSingleton<JobsitePlacementConfig>();
            var metricsEntity = SystemAPI.GetSingletonEntity<JobsiteMetrics>();
            var metrics = SystemAPI.GetComponent<JobsiteMetrics>(metricsEntity);

            DynamicBuffer<TelemetryMetric> telemetryBuffer = default;
            var telemetryEntity = SystemAPI.GetSingletonEntity<TelemetryStream>();
            if (state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                telemetryBuffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            }

            DynamicBuffer<PlayEffectRequest> effectBuffer = default;
            if (SystemAPI.TryGetSingletonEntity<PresentationCommandQueue>(out var effectEntity) &&
                state.EntityManager.HasBuffer<PlayEffectRequest>(effectEntity))
            {
                effectBuffer = state.EntityManager.GetBuffer<PlayEffectRequest>(effectEntity);
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            bool telemetryUpdated = false;

            foreach (var (transform, ghost, flags, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRW<JobsiteGhost>, RefRO<ConstructionSiteFlags>>()
                         .WithAll<JobsiteCompletionTag>()
                         .WithEntityAccess())
            {
                if ((flags.ValueRO.Value & ConstructionSiteFlags.Completed) == 0)
                {
                    ecb.RemoveComponent<JobsiteCompletionTag>(entity);
                    continue;
                }

                if (effectBuffer.IsCreated)
                {
                    effectBuffer.Add(new PlayEffectRequest
                    {
                        EffectId = config.CompletionEffectId,
                        Target = entity,
                        Position = transform.ValueRO.Position,
                        Rotation = transform.ValueRO.Rotation,
                        DurationSeconds = math.max(0f, config.CompletionEffectDuration),
                        StyleOverride = default
                    });
                }

                metrics.CompletedCount++;
                ghost.ValueRW.CompletionRequested = 1;

                if (telemetryBuffer.IsCreated)
                {
                    UpsertMetric(ref telemetryBuffer, config.TelemetryKey, metrics.CompletedCount);
                    telemetryUpdated = true;
                }

                ecb.RemoveComponent<JobsiteGhost>(entity);
                ecb.RemoveComponent<JobsiteCompletionTag>(entity);
            }

            if (telemetryUpdated)
            {
                var telemetry = SystemAPI.GetComponentRW<TelemetryStream>(telemetryEntity);
                telemetry.ValueRW.Version++;
                telemetry.ValueRW.LastTick = timeState.Tick;
            }

            state.EntityManager.SetComponentData(metricsEntity, metrics);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static void UpsertMetric(ref DynamicBuffer<TelemetryMetric> buffer, in FixedString64Bytes key, int value)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    buffer[i] = new TelemetryMetric
                    {
                        Key = key,
                        Value = value,
                        Unit = TelemetryMetricUnit.Count
                    };
                    return;
                }
            }

            buffer.AddMetric(key, value, TelemetryMetricUnit.Count);
        }
    }
}
