using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Skills;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Godgame.Modules
{
    /// <summary>
    /// Resolves refit and repair work against modules, applying maintainer skill multipliers and emitting telemetry.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ModuleDegradationSystem))]
    public partial struct ModuleRefitSystem : ISystem
    {
        private ComponentLookup<SkillSet> _skillLookup;
        private ComponentLookup<ModuleMaintainerAssignment> _maintainerLookup;
        private ComponentLookup<ModuleHostReference> _hostLookup;
        private ComponentLookup<ModuleResourceWallet> _walletLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ModuleRefitRequest>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            _skillLookup = state.GetComponentLookup<SkillSet>(true);
            _maintainerLookup = state.GetComponentLookup<ModuleMaintainerAssignment>(true);
            _hostLookup = state.GetComponentLookup<ModuleHostReference>(true);
            _walletLookup = state.GetComponentLookup<ModuleResourceWallet>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _skillLookup.Update(ref state);
            _maintainerLookup.Update(ref state);
            _hostLookup.Update(ref state);
            _walletLookup.Update(ref state);

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

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var completedRefits = new NativeArray<int>(1, Allocator.TempJob);
            var completedRepairs = new NativeArray<int>(1, Allocator.TempJob);

            new RefitJob
            {
                Delta = deltaSeconds,
                Config = config,
                SkillLookup = _skillLookup,
                MaintainerLookup = _maintainerLookup,
                HostLookup = _hostLookup,
                WalletLookup = _walletLookup,
                Ecb = ecb,
                CurrentTick = timeState.Tick,
                CompletedRefits = completedRefits,
                CompletedRepairs = completedRepairs
            }.ScheduleParallel();

            state.Dependency.Complete();

            // Emit telemetry
            if (telemetryEntity != Entity.Null && state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                var telemetryBuffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
                var refitCount = completedRefits[0];
                var repairCount = completedRepairs[0];

                if (refitCount > 0)
                {
                    UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.RefitCompleted, refitCount);
                }

                if (repairCount > 0)
                {
                    UpsertTelemetry(ref telemetryBuffer, ModuleTelemetryKeys.RepairCompleted, repairCount);
                }
            }

            completedRefits.Dispose();
            completedRepairs.Dispose();
        }

        [BurstCompile]
        public partial struct RefitJob : IJobEntity
        {
            public float Delta;
            public ModuleMaintenanceConfig Config;
            [ReadOnly] public ComponentLookup<SkillSet> SkillLookup;
            [ReadOnly] public ComponentLookup<ModuleMaintainerAssignment> MaintainerLookup;
            [ReadOnly] public ComponentLookup<ModuleHostReference> HostLookup;
            public ComponentLookup<ModuleResourceWallet> WalletLookup;
            public EntityCommandBuffer.ParallelWriter Ecb;
            public uint CurrentTick;
            public NativeArray<int> CompletedRefits;
            public NativeArray<int> CompletedRepairs;

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref ModuleData module, ref ModuleRefitRequest refit)
            {
                var maintainerLevel = 0;
                var skillId = refit.SkillId == SkillId.None ? SkillId.Processing : refit.SkillId;
                var walletEntity = e;

                if (MaintainerLookup.HasComponent(e))
                {
                    var workerEntity = MaintainerLookup[e].WorkerEntity;
                    if (workerEntity != Entity.Null && SkillLookup.HasComponent(workerEntity))
                    {
                        maintainerLevel = SkillLookup[workerEntity].GetLevel(skillId);
                    }
                }
                else if (HostLookup.HasComponent(e))
                {
                    var hostRef = HostLookup[e];
                    if (hostRef.Host != Entity.Null && SkillLookup.HasComponent(hostRef.Host))
                    {
                        var hostSkills = SkillLookup[hostRef.Host];
                        maintainerLevel = hostSkills.GetLevel(skillId);
                        walletEntity = hostRef.Host;

                        // Auto-populate maintainer assignment
                        Ecb.AddComponent(ciq, e, new ModuleMaintainerAssignment
                        {
                            WorkerEntity = hostRef.Host
                        });
                    }
                }

                var workRate = math.max(Config.BaseWorkRate * (1f + maintainerLevel * Config.SkillRateBonus), 0f);
                var workDone = workRate * Delta;

                // Handle resource cost
                if (workDone > 0f && Config.ResourceCostPerWork > 0f && WalletLookup.HasComponent(walletEntity))
                {
                    var wallet = WalletLookup[walletEntity];
                    var cost = Config.ResourceCostPerWork * workDone;
                    if (wallet.Resources < cost)
                    {
                        return; // Insufficient resources
                    }

                    wallet.Resources -= cost;
                    WalletLookup[walletEntity] = wallet;
                }

                if (workDone > 0f && refit.WorkRemaining > 0f)
                {
                    refit.WorkRemaining = math.max(0f, refit.WorkRemaining - workDone);
                    var conditionGain = workDone / math.max(Config.WorkRequiredPerCondition, 0.0001f);
                    module.Condition = math.min(module.MaxCondition, module.Condition + conditionGain);
                    module.Status = ModuleStatus.Refit;
                }

                if (refit.WorkRemaining <= 0.0001f)
                {
                    module.Condition = math.min(module.MaxCondition, math.max(module.Condition, refit.TargetCondition));
                    module.Status = ModuleStatus.Operational;
                    module.LastServiceTick = CurrentTick;
                    Ecb.RemoveComponent<ModuleRefitRequest>(ciq, e);

                    if (refit.AutoRequested == 1)
                    {
                        Increment(ref CompletedRepairs);
                    }
                    else
                    {
                        Increment(ref CompletedRefits);
                    }
                }
            }
        }

        private static void Increment(ref NativeArray<int> counter)
        {
            var value = counter[0];
            counter[0] = value + 1;
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
