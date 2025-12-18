using Godgame.Telemetry;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Village-level AI decision-making system.
    /// Makes autonomous decisions about resource allocation, building expansion, and crisis response.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillageAIDecisionSystem : ISystem
    {
        private ComponentLookup<VillagerNeeds> _needsLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            _needsLookup = state.GetComponentLookup<VillagerNeeds>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BehaviorTelemetryState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            _needsLookup.Update(ref state);
            _transformLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            DynamicBuffer<GodgameDecisionTransitionRecord> decisionAuditBuffer = default;
            if (state.EntityManager.HasBuffer<GodgameDecisionTransitionRecord>(telemetryEntity))
            {
                decisionAuditBuffer = state.EntityManager.GetBuffer<GodgameDecisionTransitionRecord>(telemetryEntity);
            }

            DynamicBuffer<GodgameDecisionTraceRecord> decisionTraceBuffer = default;
            if (state.EntityManager.HasBuffer<GodgameDecisionTraceRecord>(telemetryEntity))
            {
                decisionTraceBuffer = state.EntityManager.GetBuffer<GodgameDecisionTraceRecord>(telemetryEntity);
            }

            DynamicBuffer<GodgameDecisionOscillationState> oscillationBuffer = default;
            if (state.EntityManager.HasBuffer<GodgameDecisionOscillationState>(telemetryEntity))
            {
                oscillationBuffer = state.EntityManager.GetBuffer<GodgameDecisionOscillationState>(telemetryEntity);
            }

            var summaryAvailable = state.EntityManager.HasComponent<GodgameTelemetrySummary>(telemetryEntity);
            var summaryData = summaryAvailable
                ? state.EntityManager.GetComponentData<GodgameTelemetrySummary>(telemetryEntity)
                : default;

            foreach (var (village, members, resources, decision, expansionRequests, entity) in SystemAPI.Query<
                RefRO<Village>,
                DynamicBuffer<VillageMember>,
                DynamicBuffer<VillageResource>,
                RefRW<VillageAIDecision>,
                DynamicBuffer<VillageExpansionRequest>>()
                .WithEntityAccess())
            {
                var villageValue = village.ValueRO;
                var decisionValue = decision.ValueRW;

                // Update decision if expired or none exists
                var previousDecision = decisionValue.DecisionType;
                var previousPriority = decisionValue.CurrentPriority;

                if (decisionValue.DecisionType == 0 || 
                    (timeState.Tick - decisionValue.DecisionTick) * timeState.FixedDeltaTime > decisionValue.DecisionDuration)
                {
                    // Calculate village needs
                    var totalResources = 0;
                    foreach (var resource in resources)
                    {
                        totalResources += resource.Quantity;
                    }

                    var averageEnergy = 0f;
                    var averageMorale = 0f;
                    var memberCount = 0;
                    foreach (var member in members)
                    {
                        if (_needsLookup.HasComponent(member.VillagerEntity))
                        {
                            var needs = _needsLookup[member.VillagerEntity];
                            averageEnergy += needs.Energy;
                            averageMorale += needs.Morale;
                            memberCount++;
                        }
                    }

                    if (memberCount > 0)
                    {
                        averageEnergy /= memberCount;
                        averageMorale /= memberCount;
                    }

                    // Decision logic based on village state
                    byte priority = 0;
                    byte decisionType = 0;
                    Entity targetEntity = Entity.Null;
                    float3 targetPosition = villageValue.CenterPosition;
                    var reason = GodgameDecisionReason.Unknown;

                    var gatherScore = math.max(0, 120 - totalResources);
                    var expandScore = villageValue.Phase == VillagePhase.Growing ? math.max(0, totalResources - 50) : 0f;
                    var moraleScore = math.max(0f, 600f - averageMorale);
                    var maintainScore = villageValue.Phase == VillagePhase.Stable ? 25f : 0f;

                    // Low resources -> prioritize gathering
                    if (totalResources < 100 && memberCount > 0)
                    {
                        priority = 80;
                        decisionType = 4; // Gather
                        reason = GodgameDecisionReason.ResourceShortage;
                    }
                    // Low morale -> prioritize expansion/improvement
                    else if (averageMorale < 500f && villageValue.Phase == VillagePhase.Growing)
                    {
                        priority = 60;
                        decisionType = 2; // Expand
                         reason = GodgameDecisionReason.LowMorale;
                        // Add expansion request
                        expansionRequests.Add(new VillageExpansionRequest
                        {
                            BuildingType = 1, // Housing
                            Position = villageValue.CenterPosition + new float3(5f, 0f, 0f),
                            Priority = 60,
                            RequestTick = timeState.Tick
                        });
                    }
                    // Growing phase -> prioritize expansion
                    else if (villageValue.Phase == VillagePhase.Growing && totalResources > 200)
                    {
                        priority = 50;
                        decisionType = 2; // Expand
                        reason = GodgameDecisionReason.GrowthPhaseExpansion;
                        expansionRequests.Add(new VillageExpansionRequest
                        {
                            BuildingType = 2, // Storehouse
                            Position = villageValue.CenterPosition + new float3(-5f, 0f, 0f),
                            Priority = 50,
                            RequestTick = timeState.Tick
                        });
                    }
                    // Stable phase -> maintain status quo
                    else if (villageValue.Phase == VillagePhase.Stable)
                    {
                        priority = 30;
                        decisionType = 0; // None (maintain)
                        reason = GodgameDecisionReason.StableMaintenance;
                    }

                    // Update decision
                    decisionValue.CurrentPriority = priority;
                    decisionValue.DecisionType = decisionType;
                    decisionValue.TargetEntity = targetEntity;
                    decisionValue.TargetPosition = targetPosition;
                    decisionValue.DecisionTick = timeState.Tick;
                    decisionValue.DecisionDuration = 10f; // 10 seconds
                    decision.ValueRW = decisionValue;

                    if (decisionAuditBuffer.IsCreated && (previousDecision != decisionType || previousPriority != priority))
                    {
                        var agentId = BuildVillageAgentId(village.ValueRO.VillageId);
                        var scores = BuildScoreboard(gatherScore, expandScore, moraleScore, maintainScore);
                        var record = new GodgameDecisionTransitionRecord
                        {
                            Tick = timeState.Tick,
                            AgentId = agentId,
                            OldState = previousDecision,
                            NewState = decisionType,
                            Reason = reason,
                            Target = targetEntity,
                            Priority = priority,
                            Scores = scores
                        };
                        decisionAuditBuffer.Add(record);

                        var decisionLabel = BuildDecisionLabel(decisionType);
                        if (decisionTraceBuffer.IsCreated)
                        {
                            decisionTraceBuffer.Add(new GodgameDecisionTraceRecord
                            {
                                Tick = timeState.Tick,
                                AgentId = agentId,
                                Domain = GodgameDecisionDomain.Village,
                                ChosenId = decisionLabel,
                                ReasonCode = (ushort)reason,
                                TopChoices = scores,
                                ContextHash = GodgameTelemetryStringHelpers.HashContext(decisionLabel, timeState.Tick)
                            });
                        }

                        if (oscillationBuffer.IsCreated)
                        {
                            if (UpdateOscillation(oscillationBuffer, agentId, decisionLabel, timeState.Tick) && summaryAvailable)
                            {
                                summaryData.OscillationCount++;
                                summaryData.SummaryDirty = 1;
                            }
                        }
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            if (summaryAvailable)
            {
                state.EntityManager.SetComponentData(telemetryEntity, summaryData);
            }
        }

        private static FixedList128Bytes<GodgameDecisionScoreEntry> BuildScoreboard(float gather, float expand, float morale, float maintain)
        {
            var scores = new FixedList128Bytes<GodgameDecisionScoreEntry>();
            InsertScore(ref scores, CreateScoreEntry("gather", gather));
            InsertScore(ref scores, CreateScoreEntry("expand", expand));
            InsertScore(ref scores, CreateScoreEntry("morale", morale));
            InsertScore(ref scores, CreateScoreEntry("maintain", maintain));
            return scores;
        }

        private static GodgameDecisionScoreEntry CreateScoreEntry(string label, float score)
        {
            var entry = new GodgameDecisionScoreEntry { Score = score };
            entry.Label.Append(label);
            return entry;
        }

        private static void InsertScore(ref FixedList128Bytes<GodgameDecisionScoreEntry> list, in GodgameDecisionScoreEntry entry)
        {
            if (entry.Score <= 0f)
            {
                return;
            }

            list.Add(entry);
        }

        private static FixedString64Bytes BuildVillageAgentId(in FixedString64Bytes sourceId)
        {
            FixedString64Bytes agentId = default;
            agentId.Append("village/");
            agentId.Append(sourceId);
            return agentId;
        }

        private static FixedString64Bytes BuildDecisionLabel(byte decisionType)
        {
            FixedString64Bytes label = default;
            label.Append("decision/");
            label.Append(decisionType);
            return label;
        }

        private const uint OscillationThresholdTicks = 600;

        private static bool UpdateOscillation(DynamicBuffer<GodgameDecisionOscillationState> buffer, in FixedString64Bytes agentId, in FixedString64Bytes decisionId, uint tick)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                var entry = buffer[i];
                if (entry.AgentId.Equals(agentId))
                {
                    var oscillated = !entry.LastDecisionId.Equals(decisionId) && tick - entry.LastDecisionTick <= OscillationThresholdTicks;
                    entry.LastDecisionId = decisionId;
                    entry.LastDecisionTick = tick;
                    buffer[i] = entry;
                    return oscillated;
                }
            }

            buffer.Add(new GodgameDecisionOscillationState
            {
                AgentId = agentId,
                LastDecisionId = decisionId,
                LastDecisionTick = tick
            });

            return false;
        }
    }
}
