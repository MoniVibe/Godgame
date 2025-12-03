using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Skills;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Modules
{
    /// <summary>
    /// Applies passive wear to modules and queues refit/repair work when condition falls below thresholds.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CoreSingletonBootstrapSystem))]
    public partial struct ModuleDegradationSystem : ISystem
    {
        private EntityQuery _moduleQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<ModuleData>();
            _moduleQuery = SystemAPI.QueryBuilder().WithAll<ModuleData>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_moduleQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var config = SystemAPI.TryGetSingleton<ModuleMaintenanceConfig>(out var cfg)
                ? cfg
                : ModuleMaintenanceDefaults.Create();

            var deltaSeconds = math.max(timeState.FixedDeltaTime * math.max(timeState.CurrentSpeedMultiplier, 0f), 0f);

            var telemetryEntity = SystemAPI.TryGetSingletonEntity<TelemetryStream>(out var telemetryStreamEntity)
                ? telemetryStreamEntity
                : Entity.Null;
            DynamicBuffer<TelemetryMetric> telemetryBuffer = default;
            if (telemetryEntity != Entity.Null && state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                telemetryBuffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            }

            var total = 0;
            var damaged = 0;
            var offline = 0;
            var refitsQueued = 0;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (moduleRef, entity) in SystemAPI.Query<RefRW<ModuleData>>().WithEntityAccess())
            {
                ref var module = ref moduleRef.ValueRW;
                total++;

                module.MaxCondition = math.max(module.MaxCondition, 1f);
                module.Condition = math.clamp(module.Condition, 0f, module.MaxCondition);

                if (module.Status != ModuleStatus.Refit)
                {
                    var degradation = math.max(module.DegradationPerSecond * deltaSeconds, 0f);
                    if (degradation > 0f && module.Condition > 0f)
                    {
                        module.Condition = math.max(0f, module.Condition - degradation);
                    }
                }

                if (module.Condition <= module.MaxCondition * config.CriticalThreshold)
                {
                    module.Status = ModuleStatus.Offline;
                }
                else if (module.Status != ModuleStatus.Refit && module.Condition < module.MaxCondition * config.AutoRepairThreshold)
                {
                    module.Status = ModuleStatus.Damaged;
                }

                if (module.Status == ModuleStatus.Damaged || module.Status == ModuleStatus.Offline)
                {
                    damaged++;
                    if (module.Status == ModuleStatus.Offline)
                    {
                        offline++;
                    }
                }

                if (module.Condition < module.MaxCondition * config.AutoRepairThreshold &&
                    !state.EntityManager.HasComponent<ModuleRefitRequest>(entity))
                {
                    var workRequired = math.max((module.MaxCondition - module.Condition) * config.WorkRequiredPerCondition, 0.01f);
                    ecb.AddComponent(entity, new ModuleRefitRequest
                    {
                        WorkRemaining = workRequired,
                        TargetCondition = module.MaxCondition,
                        SkillId = SkillId.Processing,
                        AutoRequested = 1
                    });
                    module.Status = ModuleStatus.Refit;
                    refitsQueued++;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            if (telemetryBuffer.IsCreated)
            {
                UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.TotalModules, total);
                UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.DamagedModules, damaged);
                UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.OfflineModules, offline);

                if (refitsQueued > 0)
                {
                    UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.RefitsQueued, refitsQueued);
                }
            }
        }

        private static void UpsertTelemetry(ref DynamicBuffer<TelemetryMetric> buffer, in FixedString64Bytes key, float value)
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

            buffer.Add(new TelemetryMetric
            {
                Key = key,
                Value = value,
                Unit = TelemetryMetricUnit.Count
            });
        }
    }
}
