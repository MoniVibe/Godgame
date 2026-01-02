using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Emits compact Godgame oracle metrics for nightly headless runs.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerActionTelemetrySystem))]
    [UpdateAfter(typeof(VillagerPonderSystem))]
    public partial struct GodgameOracleMetricsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<TelemetryStream>();
            state.RequireForUpdate<TelemetryExportConfig>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TelemetryExportConfig>(out var config) ||
                config.Enabled == 0 ||
                (config.Flags & TelemetryExportFlags.IncludeTelemetryMetrics) == 0)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var tick = timeState.Tick;
            var cadence = config.CadenceTicks > 0 ? config.CadenceTicks : 30u;
            var shouldExport = tick % cadence == 0u;

            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            var streamEntity = SystemAPI.GetSingletonEntity<TelemetryStream>();
            var entityManager = state.EntityManager;

            if (!entityManager.HasComponent<GodgameOracleAccumulator>(telemetryEntity))
            {
                entityManager.AddComponentData(telemetryEntity, new GodgameOracleAccumulator());
            }

            if (!entityManager.HasBuffer<GodgameOracleLoopSample>(telemetryEntity))
            {
                entityManager.AddBuffer<GodgameOracleLoopSample>(telemetryEntity);
            }

            var accumulator = entityManager.GetComponentData<GodgameOracleAccumulator>(telemetryEntity);
            var loopSamples = entityManager.GetBuffer<GodgameOracleLoopSample>(telemetryEntity);

            var deltaSeconds = timeState.IsPaused ? 0f : math.max(0f, timeState.DeltaSeconds);
            accumulator.SampleSeconds += deltaSeconds;
            accumulator.SampleTicks += 1;

            if (deltaSeconds > 0f)
            {
                foreach (var (goal, assignment, ponder) in SystemAPI
                             .Query<RefRO<VillagerGoalState>, RefRO<JobAssignment>, RefRO<VillagerPonderState>>())
                {
                    if (goal.ValueRO.CurrentGoal == VillagerGoal.Work &&
                        assignment.ValueRO.Ticket == Entity.Null &&
                        ponder.ValueRO.RemainingSeconds > 0f)
                    {
                        accumulator.PonderFreezeSeconds += deltaSeconds;
                    }
                }
            }

            if (entityManager.HasBuffer<GodgameGatherYieldRecord>(telemetryEntity))
            {
                var gatherBuffer = entityManager.GetBuffer<GodgameGatherYieldRecord>(telemetryEntity);
                if (accumulator.LastGatherYieldIndex > (uint)gatherBuffer.Length)
                {
                    accumulator.LastGatherYieldIndex = 0;
                }

                for (int i = (int)accumulator.LastGatherYieldIndex; i < gatherBuffer.Length; i++)
                {
                    accumulator.GatheredAmount += math.max(0f, gatherBuffer[i].Amount);
                }

                accumulator.LastGatherYieldIndex = (uint)gatherBuffer.Length;
            }

            if (entityManager.HasBuffer<GodgameHaulTripRecord>(telemetryEntity))
            {
                var haulBuffer = entityManager.GetBuffer<GodgameHaulTripRecord>(telemetryEntity);
                if (accumulator.LastHaulTripIndex > (uint)haulBuffer.Length)
                {
                    accumulator.LastHaulTripIndex = 0;
                }

                for (int i = (int)accumulator.LastHaulTripIndex; i < haulBuffer.Length; i++)
                {
                    var record = haulBuffer[i];
                    accumulator.HauledAmount += math.max(0f, record.CarriedAmount);

                    var duration = record.EndTick >= record.StartTick ? record.EndTick - record.StartTick : 0u;
                    if (duration > 0 && loopSamples.Length < MaxLoopSamples)
                    {
                        loopSamples.Add(new GodgameOracleLoopSample { Value = duration });
                    }
                }

                accumulator.LastHaulTripIndex = (uint)haulBuffer.Length;
            }

            if (shouldExport)
            {
                if (entityManager.HasBuffer<TelemetryMetric>(streamEntity))
                {
                    var metrics = entityManager.GetBuffer<TelemetryMetric>(streamEntity);
                    var seconds = math.max(0.0001f, accumulator.SampleSeconds);
                    var dropRate = accumulator.GatheredAmount > 0f ? (accumulator.GatheredAmount / seconds) * 60f : 0f;
                    var haulRate = accumulator.HauledAmount > 0f ? (accumulator.HauledAmount / seconds) * 60f : 0f;
                    var p50 = ResolvePercentile(loopSamples, 0.5f);
                    var p95 = ResolvePercentile(loopSamples, 0.95f);

                    metrics.AddMetric("god.storehouse_loop_period_ticks.p50", p50, TelemetryMetricUnit.Count);
                    metrics.AddMetric("god.storehouse_loop_period_ticks.p95", p95, TelemetryMetricUnit.Count);
                    metrics.AddMetric("god.ponder_freeze_time_s", accumulator.PonderFreezeSeconds, TelemetryMetricUnit.Custom);
                    metrics.AddMetric("god.pile_drop_rate", dropRate, TelemetryMetricUnit.Custom);
                    metrics.AddMetric("god.pile_haul_rate", haulRate, TelemetryMetricUnit.Custom);
                }

                accumulator.PonderFreezeSeconds = 0f;
                accumulator.GatheredAmount = 0f;
                accumulator.HauledAmount = 0f;
                accumulator.SampleSeconds = 0f;
                accumulator.SampleTicks = 0;
                loopSamples.Clear();
            }

            entityManager.SetComponentData(telemetryEntity, accumulator);
        }

        private static float ResolvePercentile(DynamicBuffer<GodgameOracleLoopSample> samples, float percentile)
        {
            if (!samples.IsCreated || samples.Length == 0)
            {
                return 0f;
            }

            var values = new NativeList<uint>(samples.Length, Allocator.Temp);
            for (int i = 0; i < samples.Length; i++)
            {
                values.Add(samples[i].Value);
            }

            values.Sort();
            var index = (int)math.floor(percentile * (values.Length - 1));
            index = math.clamp(index, 0, values.Length - 1);
            var value = values[index];
            values.Dispose();
            return value;
        }

        private const int MaxLoopSamples = 256;
    }
}
