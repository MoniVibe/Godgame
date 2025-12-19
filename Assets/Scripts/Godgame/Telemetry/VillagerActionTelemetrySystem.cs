using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Telemetry
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerActionTelemetryBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (job, entity) in SystemAPI.Query<RefRO<VillagerJobState>>()
                         .WithNone<VillagerActionTelemetryState>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new VillagerActionTelemetryState
                {
                    LastPhase = job.ValueRO.Phase,
                    PhaseStartTick = tick,
                    ActiveActionId = 0,
                    LastTarget = job.ValueRO.Target
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VillagerActionTelemetrySystem : ISystem
    {
        private ComponentLookup<Godgame.Villagers.VillagerId> _villagerIdLookup;
        private uint _nextActionId;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<VillagerActionTelemetryState>();
            state.RequireForUpdate<BehaviorTelemetryState>();
            _villagerIdLookup = state.GetComponentLookup<Godgame.Villagers.VillagerId>(true);
            _nextActionId = 1;
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            if (!entityManager.HasBuffer<GodgameActionLifecycleRecord>(telemetryEntity))
            {
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var lifecycleBuffer = entityManager.GetBuffer<GodgameActionLifecycleRecord>(telemetryEntity);
            var effectBuffer = entityManager.GetBuffer<GodgameActionEffectRecord>(telemetryEntity);
            var gatherAttemptBuffer = entityManager.GetBuffer<GodgameGatherAttemptRecord>(telemetryEntity);
            var gatherYieldBuffer = entityManager.GetBuffer<GodgameGatherYieldRecord>(telemetryEntity);
            var gatherFailureBuffer = entityManager.GetBuffer<GodgameGatherFailureRecord>(telemetryEntity);
            var haulTripBuffer = entityManager.GetBuffer<GodgameHaulTripRecord>(telemetryEntity);
            var capabilityUsageBuffer = entityManager.GetBuffer<GodgameCapabilityUsageSample>(telemetryEntity);
            var failureSampleBuffer = entityManager.GetBuffer<GodgameActionFailureSample>(telemetryEntity);

            var summaryAvailable = entityManager.HasComponent<GodgameTelemetrySummary>(telemetryEntity);
            var summary = summaryAvailable
                ? entityManager.GetComponentData<GodgameTelemetrySummary>(telemetryEntity)
                : default;

            _villagerIdLookup.Update(ref state);

            foreach (var (job, telemetry, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerJobState>, RefRW<VillagerActionTelemetryState>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                var cache = telemetry.ValueRO;
                var currentPhase = job.ValueRO.Phase;
                var agentId = BuildVillagerAgentId(entity);

                if (cache.LastPhase != currentPhase)
                {
                    if (cache.ActiveActionId != 0)
                    {
                        var endEvent = new GodgameActionLifecycleRecord
                        {
                            Tick = tick,
                            ActionId = cache.ActiveActionId,
                            AgentId = agentId,
                            Phase = cache.LastPhase,
                            Event = GodgameActionLifecycleEvent.End,
                            DurationTicks = math.max(1u, tick - cache.PhaseStartTick),
                            FailureReason = DetermineFailureReason(cache, job.ValueRO),
                            Target = cache.LastTarget
                        };

                        if (endEvent.FailureReason != GodgameActionFailureReason.None)
                        {
                            endEvent.Event = GodgameActionLifecycleEvent.Fail;
                            IncrementFailureSample(failureSampleBuffer, BuildActionLabel(cache.LastPhase), endEvent.FailureReason);
                            if (summaryAvailable && endEvent.FailureReason == GodgameActionFailureReason.ResourceMissing)
                            {
                                summary.StarvationCount++;
                                summary.SummaryDirty = 1;
                            }
                        }

                        lifecycleBuffer.Add(endEvent);

                        if (cache.LastPhase == JobPhase.Gather)
                        {
                            HandleGatherExit(cache, job.ValueRO, agentId, tick, gatherYieldBuffer, gatherFailureBuffer, effectBuffer, ref summary, summaryAvailable);
                        }
                        else if (cache.LastPhase == JobPhase.Deliver)
                        {
                            HandleDeliverExit(cache, agentId, transform.ValueRO.Position, job.ValueRO, tick, haulTripBuffer, effectBuffer, ref summary, summaryAvailable);
                        }
                    }

                    var newActionId = ++_nextActionId;
                    lifecycleBuffer.Add(new GodgameActionLifecycleRecord
                    {
                        Tick = tick,
                        ActionId = newActionId,
                        AgentId = agentId,
                        Phase = currentPhase,
                        Event = GodgameActionLifecycleEvent.Start,
                        DurationTicks = 0,
                        FailureReason = GodgameActionFailureReason.None,
                        Target = job.ValueRO.Target
                    });

                    IncrementCapabilityUsage(capabilityUsageBuffer, BuildActionLabel(currentPhase));

                    cache.LastPhase = currentPhase;
                    cache.PhaseStartTick = tick;
                    cache.ActiveActionId = newActionId;
                    cache.LastTarget = job.ValueRO.Target;

                    if (currentPhase == JobPhase.Gather)
                    {
                        cache.GatherStartTick = tick;
                        cache.GatherStartCarry = job.ValueRO.CarryCount;
                        cache.GatherStartPosition = transform.ValueRO.Position;
                        cache.LastResourceTypeIndex = job.ValueRO.ResourceTypeIndex;
                        gatherAttemptBuffer.Add(new GodgameGatherAttemptRecord
                        {
                            Tick = tick,
                            AgentId = agentId,
                            MethodId = BuildGatherMethodId(),
                            ResourceType = BuildResourceLabel(job.ValueRO.ResourceTypeIndex),
                            NodeId = GodgameTelemetryStringHelpers.BuildEntityLabel(job.ValueRO.Target)
                        });
                    }
                    else if (currentPhase == JobPhase.NavigateToStorehouse)
                    {
                        cache.HaulStartTick = tick;
                        cache.HaulStartPosition = transform.ValueRO.Position;
                        cache.HaulStartCarry = job.ValueRO.CarryCount;
                    }
                }

                cache.LastCarryCount = job.ValueRO.CarryCount;
                cache.LastResourceTypeIndex = job.ValueRO.ResourceTypeIndex;
                cache.LastTarget = job.ValueRO.Target;
                telemetry.ValueRW = cache;
            }

            if (summaryAvailable)
            {
                entityManager.SetComponentData(telemetryEntity, summary);
            }
        }

        private FixedString64Bytes BuildVillagerAgentId(Entity entity)
        {
            FixedString64Bytes agentId = default;
            if (_villagerIdLookup.HasComponent(entity))
            {
                var villagerId = _villagerIdLookup[entity].Value;
                agentId.Append("villager/");
                agentId.Append(villagerId);
            }
            else
            {
                agentId.Append("entity/");
                agentId.Append(entity.Index);
                agentId.Append('.');
                agentId.Append(entity.Version);
            }

            return agentId;
        }

        private static GodgameActionFailureReason DetermineFailureReason(in VillagerActionTelemetryState previous, in VillagerJobState current)
        {
            if (current.Phase != JobPhase.Idle)
            {
                return GodgameActionFailureReason.None;
            }

            if (previous.LastTarget == Entity.Null)
            {
                return GodgameActionFailureReason.InvalidTarget;
            }

            if (previous.LastPhase == JobPhase.Gather && current.CarryCount <= 0f)
            {
                return GodgameActionFailureReason.ResourceMissing;
            }

            if (previous.LastPhase == JobPhase.NavigateToStorehouse && current.DropoffCooldown > 0f)
            {
                return GodgameActionFailureReason.Cooldown;
            }

            return GodgameActionFailureReason.None;
        }

        private static FixedString32Bytes BuildGatherMethodId()
        {
            FixedString32Bytes method = default;
            method.Append("method/gather.default");
            return method;
        }

        private static FixedString32Bytes BuildResourceLabel(ushort resourceIndex)
        {
            FixedString32Bytes label = default;
            label.Append("resource/");
            label.Append(resourceIndex);
            return label;
        }

        private static FixedString64Bytes BuildActionLabel(JobPhase phase)
        {
            FixedString64Bytes label = default;
            label.Append("action/");
            label.Append(phase.ToString());
            return label;
        }

        private static void IncrementCapabilityUsage(DynamicBuffer<GodgameCapabilityUsageSample> buffer, in FixedString64Bytes capabilityId)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                var entry = buffer[i];
                if (entry.CapabilityId.Equals(capabilityId))
                {
                    entry.Count++;
                    buffer[i] = entry;
                    return;
                }
            }

            buffer.Add(new GodgameCapabilityUsageSample
            {
                CapabilityId = capabilityId,
                Count = 1
            });
        }

        private static void IncrementFailureSample(DynamicBuffer<GodgameActionFailureSample> buffer, in FixedString64Bytes actionId, GodgameActionFailureReason reason)
        {
            if (reason == GodgameActionFailureReason.None)
            {
                return;
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                var entry = buffer[i];
                if (entry.ActionId.Equals(actionId) && entry.Reason == reason)
                {
                    entry.Count++;
                    buffer[i] = entry;
                    return;
                }
            }

            buffer.Add(new GodgameActionFailureSample
            {
                ActionId = actionId,
                Reason = reason,
                Count = 1
            });
        }

        private static void HandleGatherExit(in VillagerActionTelemetryState cache,
            in VillagerJobState currentJob,
            in FixedString64Bytes agentId,
            uint tick,
            DynamicBuffer<GodgameGatherYieldRecord> yieldBuffer,
            DynamicBuffer<GodgameGatherFailureRecord> failureBuffer,
            DynamicBuffer<GodgameActionEffectRecord> effectBuffer,
            ref GodgameTelemetrySummary summary,
            bool summaryAvailable)
        {
            var gathered = math.max(0f, currentJob.CarryCount - cache.GatherStartCarry);
            var methodId = BuildGatherMethodId();
            var resourceLabel = BuildResourceLabel(cache.LastResourceTypeIndex);

            if (gathered > 0f)
            {
                yieldBuffer.Add(new GodgameGatherYieldRecord
                {
                    Tick = tick,
                    AgentId = agentId,
                    MethodId = methodId,
                    ResourceType = resourceLabel,
                    Amount = gathered,
                    TimeSpentTicks = math.max(1u, tick - cache.GatherStartTick)
                });

                effectBuffer.Add(new GodgameActionEffectRecord
                {
                    Tick = tick,
                    ActionId = cache.ActiveActionId,
                    AgentId = agentId,
                    ActionLabel = BuildActionLabel(JobPhase.Gather),
                    DeltaCargo = gathered,
                    DeltaResource = 0f,
                    DeltaHealth = 0f,
                    DeltaThreat = 0f
                });
            }
            else
            {
                failureBuffer.Add(new GodgameGatherFailureRecord
                {
                    Tick = tick,
                    AgentId = agentId,
                    MethodId = methodId,
                    ResourceType = resourceLabel,
                    Reason = DetermineGatherFailureReason(currentJob, cache)
                });
            }

            if (summaryAvailable)
            {
                summary.TaskCount++;
                summary.SummaryDirty = 1;
            }
        }

        private static void HandleDeliverExit(in VillagerActionTelemetryState cache,
            in FixedString64Bytes agentId,
            in float3 currentPosition,
            in VillagerJobState job,
            uint tick,
            DynamicBuffer<GodgameHaulTripRecord> haulTrips,
            DynamicBuffer<GodgameActionEffectRecord> effectBuffer,
            ref GodgameTelemetrySummary summary,
            bool summaryAvailable)
        {
            if (cache.HaulStartCarry <= 0f)
            {
                return;
            }

            haulTrips.Add(new GodgameHaulTripRecord
            {
                StartTick = cache.HaulStartTick,
                EndTick = tick,
                AgentId = agentId,
                CarriedAmount = cache.HaulStartCarry,
                Distance = math.distance(cache.HaulStartPosition, currentPosition),
                Congestion = job.DropoffCooldown
            });

            effectBuffer.Add(new GodgameActionEffectRecord
            {
                Tick = tick,
                ActionId = cache.ActiveActionId,
                AgentId = agentId,
                ActionLabel = BuildActionLabel(JobPhase.Deliver),
                DeltaCargo = -cache.HaulStartCarry,
                DeltaResource = cache.HaulStartCarry,
                DeltaHealth = 0f,
                DeltaThreat = 0f
            });

            if (summaryAvailable)
            {
                summary.ResourceTotal += cache.HaulStartCarry;
                summary.TaskCount++;
                summary.SummaryDirty = 1;
            }
        }

        private static GodgameGatherFailureReason DetermineGatherFailureReason(in VillagerJobState job, in VillagerActionTelemetryState cache)
        {
            if (cache.LastTarget == Entity.Null)
            {
                return GodgameGatherFailureReason.InvalidNode;
            }

            if (job.CarryCount <= cache.GatherStartCarry + 0.01f)
            {
                return GodgameGatherFailureReason.Blocked;
            }

            return GodgameGatherFailureReason.None;
        }
    }
}
