using Godgame.Telemetry;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Headless
{
    /// <summary>
    /// Emits movement diagnostics snapshots and offender reports on failure for headless runs.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(GodgameHeadlessMovementDiagnosticsSystem))]
    public partial struct GodgameHeadlessMovementReportSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerMovementDiagnosticsFailure>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                return;
            }

            var failureEntity = SystemAPI.GetSingletonEntity<VillagerMovementDiagnosticsFailure>();
            var failure = state.EntityManager.GetComponentData<VillagerMovementDiagnosticsFailure>(failureEntity);
            if (failure.Reported != 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var config = SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsConfig diagConfig)
                ? diagConfig
                : VillagerMovementDiagnosticsConfig.Default;

            var counters = SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsCounters diagCounters)
                ? diagCounters
                : default;

            var streamEntity = TelemetryStreamUtility.EnsureEventStream(state.EntityManager);
            if (!state.EntityManager.HasBuffer<TelemetryEvent>(streamEntity))
            {
                state.EntityManager.AddBuffer<TelemetryEvent>(streamEntity);
            }

            var events = state.EntityManager.GetBuffer<TelemetryEvent>(streamEntity);

            EmitSummary(ref events, timeState.Tick, failure, counters);
            EmitSnapshot(ref state, ref events, timeState.Tick, failure.Offender);
            EmitTrace(ref state, ref events, timeState.Tick, failure.Offender, config.TraceEventLimit);

            var offenders = ResolveTopOffenders(ref state);
            EmitOffender(ref events, timeState.Tick, offenders.TopSpeed);
            EmitSnapshot(ref state, ref events, timeState.Tick, offenders.TopSpeed.Entity);
            EmitTrace(ref state, ref events, timeState.Tick, offenders.TopSpeed.Entity, config.TraceEventLimit);

            EmitOffender(ref events, timeState.Tick, offenders.TopTeleport);
            EmitSnapshot(ref state, ref events, timeState.Tick, offenders.TopTeleport.Entity);
            EmitTrace(ref state, ref events, timeState.Tick, offenders.TopTeleport.Entity, config.TraceEventLimit);

            EmitOffender(ref events, timeState.Tick, offenders.TopFlips);
            EmitSnapshot(ref state, ref events, timeState.Tick, offenders.TopFlips.Entity);
            EmitTrace(ref state, ref events, timeState.Tick, offenders.TopFlips.Entity, config.TraceEventLimit);

            failure.Reported = 1;
            state.EntityManager.SetComponentData(failureEntity, failure);
        }

        private static void EmitSummary(ref DynamicBuffer<TelemetryEvent> buffer, uint tick, VillagerMovementDiagnosticsFailure failure, VillagerMovementDiagnosticsCounters counters)
        {
            var writer = new GodgameTelemetryJsonWriter(128);
            writer.AddInt("reason", (byte)failure.Reason);
            writer.AddInt("offenderId", failure.Offender.Index);
            writer.AddInt("speed", counters.SpeedClampCount);
            writer.AddInt("teleport", counters.TeleportCount);
            writer.AddInt("stuck", counters.StuckCount);
            writer.AddInt("nan", counters.NaNCount);

            buffer.AddEvent(new FixedString64Bytes("movement.failure.summary"), tick, new FixedString64Bytes("headless"), writer.Build());
        }

        private static void EmitOffender(ref DynamicBuffer<TelemetryEvent> buffer, uint tick, MovementOffender offender)
        {
            if (offender.Entity == Entity.Null)
            {
                return;
            }

            var writer = new GodgameTelemetryJsonWriter(128);
            writer.AddInt("id", offender.Entity.Index);
            writer.AddInt("metric", offender.MetricId);
            writer.AddInt("value", offender.ValueMilli);
            buffer.AddEvent(new FixedString64Bytes("movement.failure.offender"), tick, new FixedString64Bytes("headless"), writer.Build());
        }

        private static void EmitSnapshot(ref SystemState state, ref DynamicBuffer<TelemetryEvent> buffer, uint tick, Entity entity)
        {
            if (entity == Entity.Null || !state.EntityManager.Exists(entity))
            {
                return;
            }

            var writer = new GodgameTelemetryJsonWriter(128);
            var pos = state.EntityManager.HasComponent<LocalTransform>(entity)
                ? state.EntityManager.GetComponentData<LocalTransform>(entity).Position
                : float3.zero;
            var intent = state.EntityManager.HasComponent<MoveIntent>(entity)
                ? state.EntityManager.GetComponentData<MoveIntent>(entity)
                : default;
            var plan = state.EntityManager.HasComponent<MovePlan>(entity)
                ? state.EntityManager.GetComponentData<MovePlan>(entity)
                : default;
            var goal = state.EntityManager.HasComponent<VillagerGoalState>(entity)
                ? state.EntityManager.GetComponentData<VillagerGoalState>(entity)
                : default;
            var job = state.EntityManager.HasComponent<VillagerJobState>(entity)
                ? state.EntityManager.GetComponentData<VillagerJobState>(entity)
                : default;
            var nav = state.EntityManager.HasComponent<Navigation>(entity)
                ? state.EntityManager.GetComponentData<Navigation>(entity)
                : default;
            var trace = state.EntityManager.HasComponent<DecisionTrace>(entity)
                ? state.EntityManager.GetComponentData<DecisionTrace>(entity)
                : default;

            writer.AddInt("id", entity.Index);
            writer.AddInt("px", ToMilli(pos.x));
            writer.AddInt("pz", ToMilli(pos.z));
            writer.AddInt("g", (byte)goal.CurrentGoal);
            writer.AddInt("jp", (byte)job.Phase);
            writer.AddInt("it", (byte)intent.IntentType);
            writer.AddInt("pm", (byte)plan.Mode);
            writer.AddInt("eta", ToMilli(plan.EtaSeconds));
            writer.AddInt("tx", ToMilli(nav.Destination.x));
            writer.AddInt("tz", ToMilli(nav.Destination.z));
            writer.AddInt("rc", trace.ReasonCode);
            writer.AddInt("sc", ToMilli(trace.Score));

            buffer.AddEvent(new FixedString64Bytes("movement.failure.snapshot"), tick, new FixedString64Bytes("headless"), writer.Build());
        }

        private static void EmitTrace(ref SystemState state, ref DynamicBuffer<TelemetryEvent> buffer, uint tick, Entity entity, int limit)
        {
            if (entity == Entity.Null || !state.EntityManager.Exists(entity))
            {
                return;
            }

            if (state.EntityManager.HasBuffer<MovementTickTrace>(entity))
            {
                var traces = state.EntityManager.GetBuffer<MovementTickTrace>(entity);
                var count = math.min(limit, traces.Length);
                for (int i = math.max(0, traces.Length - count); i < traces.Length; i++)
                {
                    var trace = traces[i];
                    var writer = new GodgameTelemetryJsonWriter(128);
                    writer.AddInt("id", entity.Index);
                    writer.AddUInt("t", trace.Tick);
                    writer.AddInt("px", ToMilli(trace.Position.x));
                    writer.AddInt("pz", ToMilli(trace.Position.z));
                    writer.AddInt("spd", ToMilli(trace.Speed));
                    writer.AddInt("it", (byte)trace.IntentType);
                    buffer.AddEvent(new FixedString64Bytes("movement.failure.trace"), tick, new FixedString64Bytes("headless"), writer.Build());
                }
                return;
            }

            if (!state.EntityManager.HasBuffer<DecisionTraceEvent>(entity))
            {
                return;
            }

            var events = state.EntityManager.GetBuffer<DecisionTraceEvent>(entity);
            var fallbackCount = math.min(limit, events.Length);
            for (int i = math.max(0, events.Length - fallbackCount); i < events.Length; i++)
            {
                var evt = events[i];
                var writer = new GodgameTelemetryJsonWriter(128);
                writer.AddInt("id", entity.Index);
                writer.AddUInt("t", evt.Tick);
                writer.AddInt("r", evt.ReasonCode);
                writer.AddInt("s", ToMilli(evt.Score));
                writer.AddInt("targetId", evt.TargetEntity.Index);
                buffer.AddEvent(new FixedString64Bytes("movement.failure.trace"), tick, new FixedString64Bytes("headless"), writer.Build());
            }
        }

        private OffenderSet ResolveTopOffenders(ref SystemState state)
        {
            var topSpeed = new MovementOffender();
            var topTeleport = new MovementOffender();
            var topFlips = new MovementOffender();

            foreach (var (diagnostics, entity) in SystemAPI
                         .Query<RefRO<VillagerMovementDiagnosticsState>>()
                         .WithAll<VillagerTag>()
                         .WithEntityAccess())
            {
                var diag = diagnostics.ValueRO;
                if (diag.MaxSpeed > topSpeed.Value)
                {
                    topSpeed = new MovementOffender(entity, MovementOffenderMetric.Speed, diag.MaxSpeed);
                }

                if (diag.MaxTeleport > topTeleport.Value)
                {
                    topTeleport = new MovementOffender(entity, MovementOffenderMetric.Teleport, diag.MaxTeleport);
                }

                if (diag.StateFlipCount > topFlips.Value)
                {
                    topFlips = new MovementOffender(entity, MovementOffenderMetric.StateFlip, diag.StateFlipCount);
                }
            }

            return new OffenderSet
            {
                TopSpeed = topSpeed,
                TopTeleport = topTeleport,
                TopFlips = topFlips
            };
        }

        private static int ToMilli(float value)
        {
            return (int)math.round(value * 1000f);
        }

        private struct MovementOffender
        {
            public Entity Entity;
            public int MetricId;
            public float Value;
            public int ValueMilli;

            public MovementOffender(Entity entity, MovementOffenderMetric metric, float value)
            {
                Entity = entity;
                MetricId = (int)metric;
                Value = value;
                ValueMilli = ToMilli(value);
            }
        }

        private enum MovementOffenderMetric
        {
            Speed = 1,
            Teleport = 2,
            StateFlip = 3
        }

        private struct OffenderSet
        {
            public MovementOffender TopSpeed;
            public MovementOffender TopTeleport;
            public MovementOffender TopFlips;
        }
    }
}
