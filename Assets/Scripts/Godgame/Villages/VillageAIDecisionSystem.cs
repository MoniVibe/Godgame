using Godgame.Telemetry;
using PureDOTS.Runtime.Authority;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Knowledge;
using PureDOTS.Runtime.Social;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
        private ComponentLookup<AuthorityBody> _authorityBodyLookup;
        private BufferLookup<AuthoritySeatRef> _authoritySeatRefLookup;
        private ComponentLookup<AuthoritySeat> _authoritySeatLookup;
        private ComponentLookup<AuthoritySeatOccupant> _authoritySeatOccupantLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<BehaviorTelemetryState>();
            state.RequireForUpdate<VillageNeedAwareness>();
            state.RequireForUpdate<VillageConstructionRuntime>();
            state.RequireForUpdate<GroupKnowledgeCacheTag>();
            state.RequireForUpdate<MoralityEventQueueTag>();

            _authorityBodyLookup = state.GetComponentLookup<AuthorityBody>(true);
            _authoritySeatRefLookup = state.GetBufferLookup<AuthoritySeatRef>(true);
            _authoritySeatLookup = state.GetComponentLookup<AuthoritySeat>(true);
            _authoritySeatOccupantLookup = state.GetComponentLookup<AuthoritySeatOccupant>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _authorityBodyLookup.Update(ref state);
            _authoritySeatRefLookup.Update(ref state);
            _authoritySeatLookup.Update(ref state);
            _authoritySeatOccupantLookup.Update(ref state);

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

            var moralityQueueEntity = SystemAPI.GetSingletonEntity<MoralityEventQueueTag>();
            var moralityEvents = SystemAPI.GetBuffer<MoralityEvent>(moralityQueueEntity);

            foreach (var (village, awareness, construction, resources, groupCache, decision, expansionRequests, entity) in SystemAPI.Query<
                RefRO<Village>,
                RefRO<VillageNeedAwareness>,
                RefRO<VillageConstructionRuntime>,
                DynamicBuffer<VillageResource>,
                DynamicBuffer<GroupKnowledgeEntry>,
                RefRW<VillageAIDecision>,
                DynamicBuffer<VillageExpansionRequest>>()
                .WithEntityAccess())
            {
                var villageValue = village.ValueRO;
                var decisionValue = decision.ValueRW;

                // Update decision if expired or none exists
                var previousDecision = decisionValue.DecisionType;
                var previousPriority = decisionValue.CurrentPriority;

                var decisionExpired = decisionValue.DecisionType == 0 ||
                                      (timeState.Tick - decisionValue.DecisionTick) * timeState.FixedDeltaTime > decisionValue.DecisionDuration;

                var hasThreat = TrySelectActiveThreat(groupCache, timeState.Tick, out var threatEntry);
                var needsInterrupt = hasThreat && previousDecision != 3; // Defend interrupts anything else.

                if (decisionExpired || needsInterrupt)
                {
                    // Calculate coarse resource pressure (placeholder until economy is fully wired).
                    var totalResources = 0;
                    foreach (var resource in resources)
                    {
                        totalResources += resource.Quantity;
                    }

                    var needAwareness = awareness.ValueRO;
                    var hasActiveProject = construction.ValueRO.ActiveSite != Entity.Null;
                    var canIssueProject = !hasActiveProject && expansionRequests.Length == 0 && needAwareness.SampleCount > 0;

                    // Decision logic based on village state
                    byte priority = 0;
                    byte decisionType = 0;
                    byte requestedBuildingType = 0;
                    Entity targetEntity = Entity.Null;
                    float3 targetPosition = villageValue.CenterPosition;
                    var reason = GodgameDecisionReason.Unknown;

                    var gatherScore = math.max(0, 120 - totalResources);
                    var expandScore = villageValue.Phase == VillagePhase.Growing ? math.max(0, totalResources - 50) : 0f;
                    var moraleScore = math.max(0f, needAwareness.MaxNeed * 100f);
                    var maintainScore = villageValue.Phase == VillagePhase.Stable ? 25f : 0f;

                    const float buildThreshold = 0.65f;

                    // Group cache threat interrupt -> prioritize defense.
                    if (hasThreat)
                    {
                        priority = 100;
                        decisionType = 3; // Defend
                        targetEntity = threatEntry.SubjectEntity;
                        targetPosition = threatEntry.Position;
                        reason = GodgameDecisionReason.ThreatReported;
                    }
                    // Micro-driven needs pressure -> prioritize a single build project.
                    else if (canIssueProject && needAwareness.MaxNeed >= buildThreshold)
                    {
                        var buildingType = MapNeedToBuilding(needAwareness.DominantNeed);
                        if (buildingType != VillageBuildingType.None)
                        {
                            priority = (byte)math.clamp((int)math.round(needAwareness.MaxNeed * 100f), 55, 95);
                            decisionType = 1; // Build
                            reason = GodgameDecisionReason.MicroNeedPressure;

                            targetPosition = SelectBuildPosition(villageValue.CenterPosition, buildingType);
                            requestedBuildingType = (byte)buildingType;
                            expansionRequests.Add(new VillageExpansionRequest
                            {
                                BuildingType = (byte)buildingType,
                                Position = targetPosition,
                                Priority = priority,
                                RequestTick = timeState.Tick
                            });
                        }
                    }
                    // Low resources -> prioritize gathering
                    else if (totalResources < 100 && needAwareness.SampleCount > 0)
                    {
                        priority = 80;
                        decisionType = 4; // Gather
                        reason = GodgameDecisionReason.ResourceShortage;
                    }
                    // Growing phase -> prioritize expansion
                    else if (canIssueProject && villageValue.Phase == VillagePhase.Growing && totalResources > 200)
                    {
                        priority = 50;
                        decisionType = 2; // Expand
                        reason = GodgameDecisionReason.GrowthPhaseExpansion;
                        requestedBuildingType = (byte)VillageBuildingType.Storehouse;
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

                    var decisionChanged = previousDecision != decisionType || previousPriority != priority;
                    if (decisionChanged)
                    {
                        var token = ResolveMoralityToken(decisionType, requestedBuildingType);
                        if (token != MoralityActionToken.Unknown)
                        {
                            var issuedBy = ResolveIssuedByAuthority(entity, decisionType, timeState.Tick);
                            var actor = issuedBy.IssuingOccupant != Entity.Null
                                ? issuedBy.IssuingOccupant
                                : needAwareness.InfluenceEntity;

                            if (actor != Entity.Null)
                            {
                                moralityEvents.Add(new MoralityEvent
                                {
                                    Tick = timeState.Tick,
                                    Actor = actor,
                                    Scope = entity,
                                    Target = targetEntity,
                                    Token = token,
                                    Magnitude = (short)priority,
                                    Confidence255 = 255,
                                    Reserved0 = 0,
                                    IntentFlags = MoralityIntentFlags.None,
                                    IssuedByAuthority = issuedBy
                                });
                            }
                        }
                    }

                    if (decisionAuditBuffer.IsCreated && decisionChanged)
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

            if (summaryAvailable)
            {
                state.EntityManager.SetComponentData(telemetryEntity, summaryData);
            }
        }

        private IssuedByAuthority ResolveIssuedByAuthority(Entity villageEntity, byte decisionType, uint tick)
        {
            var issued = new IssuedByAuthority
            {
                IssuingSeat = Entity.Null,
                IssuingOccupant = Entity.Null,
                ActingSeat = Entity.Null,
                ActingOccupant = Entity.Null,
                IssuedTick = tick
            };

            if (!_authorityBodyLookup.HasComponent(villageEntity))
            {
                return issued;
            }

            var body = _authorityBodyLookup[villageEntity];
            var principalSeat = body.ExecutiveSeat;
            if (principalSeat == Entity.Null)
            {
                return issued;
            }

            var issuingSeat = principalSeat;
            var desiredRole = ResolveSeatRoleId(decisionType);

            if (desiredRole.Length > 0 &&
                _authoritySeatRefLookup.HasBuffer(villageEntity) &&
                AuthoritySeatHelpers.TryFindSeatByRole(
                    _authoritySeatRefLookup[villageEntity],
                    _authoritySeatLookup,
                    desiredRole,
                    out var delegateSeat))
            {
                if (_authoritySeatOccupantLookup.HasComponent(delegateSeat) &&
                    _authoritySeatOccupantLookup[delegateSeat].OccupantEntity != Entity.Null)
                {
                    issuingSeat = delegateSeat;
                }
            }

            var occupant = Entity.Null;
            if (_authoritySeatOccupantLookup.HasComponent(issuingSeat))
            {
                occupant = _authoritySeatOccupantLookup[issuingSeat].OccupantEntity;
            }

            issued.IssuingSeat = issuingSeat;
            issued.IssuingOccupant = occupant;
            issued.ActingSeat = issuingSeat;
            issued.ActingOccupant = occupant;
            return issued;
        }

        private static FixedString64Bytes ResolveSeatRoleId(byte decisionType)
        {
            return decisionType switch
            {
                3 => new FixedString64Bytes("village.marshal"),
                1 or 2 => new FixedString64Bytes("village.steward"),
                4 => new FixedString64Bytes("village.quartermaster"),
                _ => default
            };
        }

        private static MoralityActionToken ResolveMoralityToken(byte decisionType, byte requestedBuildingType)
        {
            if (decisionType == 3)
            {
                return MoralityActionToken.DefendHome;
            }

            if (decisionType == 1 || decisionType == 2)
            {
                if (requestedBuildingType == (byte)VillageBuildingType.Housing)
                {
                    return MoralityActionToken.BuildShelter;
                }

                if (requestedBuildingType == (byte)VillageBuildingType.Worship)
                {
                    return MoralityActionToken.Build;
                }

                if (requestedBuildingType == (byte)VillageBuildingType.Storehouse)
                {
                    return MoralityActionToken.Build;
                }

                return MoralityActionToken.Build;
            }

            return MoralityActionToken.Unknown;
        }

        private static bool TrySelectActiveThreat(DynamicBuffer<GroupKnowledgeEntry> cache, uint tick, out GroupKnowledgeEntry entry)
        {
            entry = default;
            var bestScore = 0f;
            var found = false;

            for (int i = 0; i < cache.Length; i++)
            {
                var candidate = cache[i];
                if (candidate.Kind != GroupKnowledgeKind.ThreatSeen || candidate.SubjectEntity == Entity.Null)
                {
                    continue;
                }

                if (candidate.ExpireTick != 0u && tick > candidate.ExpireTick)
                {
                    continue;
                }

                var score = math.saturate(candidate.Urgency) * math.saturate(candidate.Confidence);
                if (!found || score > bestScore || (score == bestScore && candidate.LastTick > entry.LastTick))
                {
                    entry = candidate;
                    bestScore = score;
                    found = true;
                }
            }

            return found && bestScore >= 0.15f;
        }

        private static VillageBuildingType MapNeedToBuilding(VillageNeedChannel need)
        {
            return need switch
            {
                VillageNeedChannel.Hunger => VillageBuildingType.Storehouse,
                VillageNeedChannel.Rest => VillageBuildingType.Housing,
                VillageNeedChannel.Faith => VillageBuildingType.Worship,
                VillageNeedChannel.Safety => VillageBuildingType.Housing,
                VillageNeedChannel.Social => VillageBuildingType.Worship,
                VillageNeedChannel.Work => VillageBuildingType.Storehouse,
                _ => VillageBuildingType.Housing
            };
        }

        private static float3 SelectBuildPosition(in float3 center, VillageBuildingType buildingType)
        {
            return buildingType switch
            {
                VillageBuildingType.Housing => center + new float3(6f, 0f, 0f),
                VillageBuildingType.Storehouse => center + new float3(-6f, 0f, 0f),
                VillageBuildingType.Worship => center + new float3(0f, 0f, 6f),
                _ => center + new float3(4f, 0f, 4f)
            };
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
