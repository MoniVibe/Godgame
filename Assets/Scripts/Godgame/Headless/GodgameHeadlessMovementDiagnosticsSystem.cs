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
using UnityEngine;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless-only movement diagnostics for villagers.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessMovementDiagnosticsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerMovementDiagnosticsState>();
            state.RequireForUpdate<MoveIntent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var deltaTime = math.max(0.0001f, SystemAPI.Time.DeltaTime);
            var config = EnsureConfig(ref state);
            var counters = EnsureCounters(ref state);
            var failure = EnsureFailure(ref state, out var hasFailure);
            Entity failureEntity = Entity.Null;
            if (hasFailure)
            {
                failureEntity = SystemAPI.GetSingletonEntity<VillagerMovementDiagnosticsFailure>();
            }

            foreach (var (transform, intent, diagnostics, trace, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<MoveIntent>, RefRW<VillagerMovementDiagnosticsState>, DynamicBuffer<MovementTickTrace>>()
                         .WithAll<VillagerTag>()
                         .WithEntityAccess())
            {
                var position = transform.ValueRO.Position;
                if (!math.all(math.isfinite(position)))
                {
                    counters.NaNCount++;
                    if (!hasFailure)
                    {
                        failure = new VillagerMovementDiagnosticsFailure
                        {
                            Reason = VillagerMovementFailureReason.NaN,
                            Offender = entity,
                            Tick = timeState.Tick,
                            Reported = 0
                        };
                        hasFailure = true;
                    }
                    continue;
                }

                var lastTick = diagnostics.ValueRO.LastTick;
                if (lastTick != 0)
                {
                    var delta = position - diagnostics.ValueRO.LastPosition;
                    delta.y = 0f;
                    var distance = math.length(delta);
                    var speed = distance / deltaTime;
                    if (speed > diagnostics.ValueRO.MaxSpeed)
                    {
                        diagnostics.ValueRW.MaxSpeed = speed;
                    }
                    if (speed > config.MaxSpeed)
                    {
                        counters.SpeedClampCount++;
                        if (!hasFailure && counters.SpeedClampCount > config.MaxAllowedSpeedViolations)
                        {
                            failure = new VillagerMovementDiagnosticsFailure
                            {
                                Reason = VillagerMovementFailureReason.Speed,
                                Offender = entity,
                                Tick = timeState.Tick,
                                Reported = 0
                            };
                            hasFailure = true;
                        }
                    }

                    if (distance > config.TeleportDistance)
                    {
                        counters.TeleportCount++;
                        if (distance > diagnostics.ValueRO.MaxTeleport)
                        {
                            diagnostics.ValueRW.MaxTeleport = distance;
                        }
                        if (!hasFailure && counters.TeleportCount > config.MaxAllowedTeleports)
                        {
                            failure = new VillagerMovementDiagnosticsFailure
                            {
                                Reason = VillagerMovementFailureReason.Teleport,
                                Offender = entity,
                                Tick = timeState.Tick,
                                Reported = 0
                            };
                            hasFailure = true;
                        }
                    }

                    var goalDistance = math.length(position - intent.ValueRO.TargetPosition);
                    var lastGoalDistance = diagnostics.ValueRO.LastGoalDistance;
                    if (intent.ValueRO.IntentType != MoveIntentType.None && lastGoalDistance > 0f)
                    {
                        if (goalDistance > lastGoalDistance - config.ProgressEpsilon)
                        {
                            diagnostics.ValueRW.StuckTicks++;
                        }
                        else
                        {
                            diagnostics.ValueRW.StuckTicks = 0;
                        }
                    }
                    else
                    {
                        diagnostics.ValueRW.StuckTicks = 0;
                    }

                    if (diagnostics.ValueRW.StuckTicks >= config.StuckTickThreshold)
                    {
                        counters.StuckCount++;
                        diagnostics.ValueRW.StuckTicks = 0;
                        if (!hasFailure && counters.StuckCount > config.MaxAllowedStuckEvents)
                        {
                            failure = new VillagerMovementDiagnosticsFailure
                            {
                                Reason = VillagerMovementFailureReason.Stuck,
                                Offender = entity,
                                Tick = timeState.Tick,
                                Reported = 0
                            };
                            hasFailure = true;
                        }
                    }

                    diagnostics.ValueRW.LastGoalDistance = goalDistance;
                }

                if (intent.ValueRO.IntentType != diagnostics.ValueRO.LastIntentType)
                {
                    if (diagnostics.ValueRO.LastIntentType != MoveIntentType.None)
                    {
                        diagnostics.ValueRW.StateFlipCount++;
                        if (!hasFailure && diagnostics.ValueRW.StateFlipCount > config.MaxAllowedStateFlips)
                        {
                            failure = new VillagerMovementDiagnosticsFailure
                            {
                                Reason = VillagerMovementFailureReason.StateFlip,
                                Offender = entity,
                                Tick = timeState.Tick,
                                Reported = 0
                            };
                            hasFailure = true;
                        }
                    }

                    diagnostics.ValueRW.LastIntentType = intent.ValueRO.IntentType;
                }

                if (config.TraceSampleStride > 0 && timeState.Tick % (uint)config.TraceSampleStride == 0)
                {
                    var delta = position - diagnostics.ValueRO.LastPosition;
                    delta.y = 0f;
                    var speed = lastTick == 0 ? 0f : math.length(delta) / deltaTime;
                    trace.Add(new MovementTickTrace
                    {
                        Tick = timeState.Tick,
                        Position = position,
                        Speed = speed,
                        IntentType = intent.ValueRO.IntentType
                    });
                    if (trace.Length > config.TraceEventLimit)
                    {
                        trace.RemoveAt(0);
                    }
                }

                diagnostics.ValueRW.LastPosition = position;
                if (lastTick == 0)
                {
                    diagnostics.ValueRW.LastGoalDistance = math.length(position - intent.ValueRO.TargetPosition);
                }
                diagnostics.ValueRW.LastTick = timeState.Tick;
            }

            SystemAPI.SetSingleton(counters);
            if (hasFailure)
            {
                if (failureEntity == Entity.Null || !state.EntityManager.Exists(failureEntity))
                {
                    failureEntity = state.EntityManager.CreateEntity(typeof(VillagerMovementDiagnosticsFailure));
                }

                state.EntityManager.SetComponentData(failureEntity, failure);
            }

            if (SystemAPI.TryGetSingletonEntity<TelemetryStream>(out var telemetryEntity) &&
                state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                var metrics = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
                metrics.AddMetric(new FixedString64Bytes("movement.speed_violations"), counters.SpeedClampCount, TelemetryMetricUnit.Count);
                metrics.AddMetric(new FixedString64Bytes("movement.teleport_violations"), counters.TeleportCount, TelemetryMetricUnit.Count);
                metrics.AddMetric(new FixedString64Bytes("movement.stuck_events"), counters.StuckCount, TelemetryMetricUnit.Count);
                metrics.AddMetric(new FixedString64Bytes("movement.nan_count"), counters.NaNCount, TelemetryMetricUnit.Count);
            }
        }

        private VillagerMovementDiagnosticsConfig EnsureConfig(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsConfig config))
            {
                return config;
            }

            var entity = state.EntityManager.CreateEntity(typeof(VillagerMovementDiagnosticsConfig));
            state.EntityManager.SetComponentData(entity, VillagerMovementDiagnosticsConfig.Default);
            return VillagerMovementDiagnosticsConfig.Default;
        }

        private VillagerMovementDiagnosticsCounters EnsureCounters(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsCounters counters))
            {
                return counters;
            }

            var entity = state.EntityManager.CreateEntity(typeof(VillagerMovementDiagnosticsCounters));
            counters = new VillagerMovementDiagnosticsCounters();
            state.EntityManager.SetComponentData(entity, counters);
            return counters;
        }

        private VillagerMovementDiagnosticsFailure EnsureFailure(ref SystemState state, out bool hasFailure)
        {
            if (SystemAPI.TryGetSingleton(out VillagerMovementDiagnosticsFailure failure))
            {
                hasFailure = true;
                return failure;
            }

            hasFailure = false;
            return default;
        }
    }
}
