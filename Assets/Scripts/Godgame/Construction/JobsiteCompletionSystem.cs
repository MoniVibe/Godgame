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

            var query = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, JobsiteGhost, ConstructionSiteFlags, JobsiteCompletionTag>()
                .Build();

            var capacity = math.max(1, query.CalculateEntityCount());
            var completions = new NativeList<CompletionPayload>(capacity, Allocator.TempJob);
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var job = new CollectJobsiteCompletionsJob
            {
                CompletionEffectId = config.CompletionEffectId,
                CompletionEffectDuration = config.CompletionEffectDuration,
                Completions = completions.AsParallelWriter(),
                Ecb = ecb.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(query, state.Dependency);
            state.Dependency.Complete();

            if (completions.Length > 0)
            {
                if (effectBuffer.IsCreated)
                {
                    for (int i = 0; i < completions.Length; i++)
                    {
                        var payload = completions[i];
                        effectBuffer.Add(new PlayEffectRequest
                        {
                            EffectId = payload.EffectId,
                            Target = payload.Target,
                            Position = payload.Position,
                            Rotation = payload.Rotation,
                            DurationSeconds = payload.DurationSeconds,
                            StyleOverride = default
                        });
                    }
                }

                metrics.CompletedCount += completions.Length;

                if (telemetryBuffer.IsCreated)
                {
                    UpsertMetric(ref telemetryBuffer, config.TelemetryKey, metrics.CompletedCount);
                    var telemetry = SystemAPI.GetComponentRW<TelemetryStream>(telemetryEntity);
                    telemetry.ValueRW.Version++;
                    telemetry.ValueRW.LastTick = timeState.Tick;
                }
            }

            state.EntityManager.SetComponentData(metricsEntity, metrics);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            completions.Dispose();
        }

        private struct CompletionPayload
        {
            public Entity Target;
            public float3 Position;
            public quaternion Rotation;
            public int EffectId;
            public float DurationSeconds;
        }

        [BurstCompile]
        [WithAll(typeof(JobsiteCompletionTag))]
        private partial struct CollectJobsiteCompletionsJob : IJobEntity
        {
            public int CompletionEffectId;
            public float CompletionEffectDuration;
            public NativeList<CompletionPayload>.ParallelWriter Completions;
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute([EntityIndexInQuery] int sortKey,
                Entity entity,
                ref JobsiteGhost ghost,
                in ConstructionSiteFlags flags,
                in LocalTransform transform)
            {
                if ((flags.Value & ConstructionSiteFlags.Completed) == 0)
                {
                    Ecb.RemoveComponent<JobsiteCompletionTag>(sortKey, entity);
                    return;
                }

                ghost.CompletionRequested = 1;

                Completions.AddNoResize(new CompletionPayload
                {
                    EffectId = CompletionEffectId,
                    Target = entity,
                    Position = transform.Position,
                    Rotation = transform.Rotation,
                    DurationSeconds = math.max(0f, CompletionEffectDuration)
                });

                Ecb.RemoveComponent<JobsiteGhost>(sortKey, entity);
                Ecb.RemoveComponent<JobsiteCompletionTag>(sortKey, entity);
            }
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
