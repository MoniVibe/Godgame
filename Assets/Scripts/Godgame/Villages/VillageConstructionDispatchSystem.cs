using Godgame.Construction;
using Godgame.Presentation;
using Godgame.Rendering;
using Godgame.Scenario;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Agency;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Authority;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Turns village build requests into a single jobsite + a single assigned builder.
    /// Presentation-first: disabled in headless by default.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageAIDecisionSystem))]
    [UpdateBefore(typeof(GameplaySystemGroup))]
    public partial struct VillageConstructionDispatchSystem : ISystem
    {
        private const float WorkerMoveSpeed = 6.0f;
        private const float ArrivalDistance = 2.25f;
        private const float SiteScale = 6f;

        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerNeedState> _needLookup;
        private ComponentLookup<VillagerAvailability> _availabilityLookup;
        private ComponentLookup<VillagerAIState> _aiLookup;
        private ComponentLookup<VillagerJob> _jobLookup;
        private ComponentLookup<VillagerJobTicket> _ticketLookup;
        private ComponentLookup<SettlementVillagerState> _settlementLookup;
        private ComponentLookup<VillageConstructionWorker> _workerLookup;
        private ComponentLookup<AuthorityBody> _authorityBodyLookup;
        private BufferLookup<AuthorityDelegation> _authorityDelegationLookup;
        private ComponentLookup<AuthoritySeatOccupant> _authorityOccupantLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<JobsitePlacementState>();
            state.RequireForUpdate<VillageConstructionRuntime>();
            state.RequireForUpdate<AuthorityBody>();

            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _needLookup = state.GetComponentLookup<VillagerNeedState>(true);
            _availabilityLookup = state.GetComponentLookup<VillagerAvailability>(true);
            _aiLookup = state.GetComponentLookup<VillagerAIState>(true);
            _jobLookup = state.GetComponentLookup<VillagerJob>(true);
            _ticketLookup = state.GetComponentLookup<VillagerJobTicket>(true);
            _settlementLookup = state.GetComponentLookup<SettlementVillagerState>(true);
            _workerLookup = state.GetComponentLookup<VillageConstructionWorker>(true);
            _authorityBodyLookup = state.GetComponentLookup<AuthorityBody>(true);
            _authorityDelegationLookup = state.GetBufferLookup<AuthorityDelegation>(true);
            _authorityOccupantLookup = state.GetComponentLookup<AuthoritySeatOccupant>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                if (!SystemAPI.TryGetSingleton(out VillageBuildSliceConfig sliceConfig) || sliceConfig.EnableInHeadless == 0)
                {
                    return;
                }
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var config = SystemAPI.GetSingleton<JobsitePlacementConfig>();
            var placementEntity = SystemAPI.GetSingletonEntity<JobsitePlacementState>();
            var placementState = SystemAPI.GetComponentRW<JobsitePlacementState>(placementEntity);

            _transformLookup.Update(ref state);
            _needLookup.Update(ref state);
            _availabilityLookup.Update(ref state);
            _aiLookup.Update(ref state);
            _jobLookup.Update(ref state);
            _ticketLookup.Update(ref state);
            _settlementLookup.Update(ref state);
            _workerLookup.Update(ref state);
            _authorityBodyLookup.Update(ref state);
            _authorityDelegationLookup.Update(ref state);
            _authorityOccupantLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var arrivalDistanceSq = math.max(0.25f, ArrivalDistance * ArrivalDistance);

            foreach (var (construction, members, requests, entity) in SystemAPI
                         .Query<RefRO<VillageConstructionRuntime>, DynamicBuffer<VillageMember>, DynamicBuffer<VillageExpansionRequest>>()
                         .WithAll<Village>()
                         .WithEntityAccess())
            {
                if (construction.ValueRO.ActiveSite != Entity.Null || construction.ValueRO.ActiveWorker != Entity.Null)
                {
                    continue;
                }

                if (requests.Length == 0)
                {
                    continue;
                }

                var requestIndex = SelectHighestPriorityRequest(requests);
                var request = requests[requestIndex];
                var buildingType = (VillageBuildingType)request.BuildingType;
                if (buildingType == VillageBuildingType.None)
                {
                    requests.RemoveAt(requestIndex);
                    continue;
                }

                var worker = SelectWorker(request.Position, members);
                if (worker == Entity.Null)
                {
                    continue;
                }

                var siteId = placementState.ValueRO.NextSiteId;
                placementState.ValueRW.NextSiteId = siteId + 1;

                var siteEntity = ecb.CreateEntity();
                ecb.AddComponent(siteEntity, new JobsiteGhost { CompletionRequested = 0 });
                ecb.AddComponent(siteEntity, new ConstructionSiteId { Value = siteId });
                ecb.AddComponent(siteEntity, new ConstructionSiteProgress
                {
                    RequiredProgress = math.max(0.01f, config.DefaultRequiredProgress),
                    CurrentProgress = 0f
                });
                ecb.AddComponent(siteEntity, new ConstructionSiteFlags { Value = 0 });
                ecb.AddComponent(siteEntity, LocalTransform.FromPositionRotationScale(request.Position, quaternion.identity, SiteScale));
                ecb.AddBuffer<ConstructionProgressCommand>(siteEntity);
                ecb.AddComponent(siteEntity, new VillageConstructionSite
                {
                    Village = entity,
                    BuildingType = buildingType,
                    Priority = request.Priority,
                    IssuedTick = timeState.Tick
                });
                ecb.AddComponent(siteEntity, ResolveIssuedByAuthority(entity, AgencyDomain.Construction, timeState.Tick));

                if (!RuntimeMode.IsHeadless)
                {
                    AddConstructionPresentation(ref ecb, siteEntity, buildingType);
                }

                var workerState = new VillageConstructionWorker
                {
                    Village = entity,
                    Site = siteEntity,
                    ResumeSettlement = Entity.Null,
                    ResumeRandomState = 0u,
                    HadSettlementState = 0,
                    MoveSpeed = WorkerMoveSpeed,
                    BuildRatePerSecond = math.max(0.05f, config.BuildRatePerSecond),
                    ArrivalDistanceSq = arrivalDistanceSq
                };

                if (_settlementLookup.HasComponent(worker))
                {
                    var resume = _settlementLookup[worker];
                    workerState.ResumeSettlement = resume.Settlement;
                    workerState.ResumeRandomState = resume.RandomState;
                    workerState.HadSettlementState = 1;
                    ecb.RemoveComponent<SettlementVillagerState>(worker);
                }

                ecb.AddComponent(worker, workerState);

                var ticketId = (uint)math.max(1, siteId);

                AddOrSet(ref ecb, worker, CreateAiTravelling(siteEntity, request.Position, timeState.Tick), _aiLookup.HasComponent(worker));
                AddOrSet(ref ecb, worker, CreateBuilderJob(ticketId, timeState.Tick), _jobLookup.HasComponent(worker));
                AddOrSet(ref ecb, worker, CreateBuilderTicket(ticketId, request.Priority, siteEntity, timeState.Tick), _ticketLookup.HasComponent(worker));

                if (_availabilityLookup.HasComponent(worker))
                {
                    var availability = _availabilityLookup[worker];
                    availability.IsAvailable = 0;
                    availability.IsReserved = 1;
                    availability.LastChangeTick = timeState.Tick;
                    ecb.SetComponent(worker, availability);
                }

                var runtime = construction.ValueRO;
                runtime.ActiveSite = siteEntity;
                runtime.ActiveWorker = worker;
                runtime.ActiveBuildingType = buildingType;
                runtime.LastIssuedTick = timeState.Tick;
                ecb.SetComponent(entity, runtime);

                requests.RemoveAt(requestIndex);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private IssuedByAuthority ResolveIssuedByAuthority(Entity bodyEntity, AgencyDomain domain, uint tick)
        {
            var issued = new IssuedByAuthority
            {
                IssuingSeat = Entity.Null,
                IssuingOccupant = Entity.Null,
                ActingSeat = Entity.Null,
                ActingOccupant = Entity.Null,
                IssuedTick = tick
            };

            if (!_authorityBodyLookup.HasComponent(bodyEntity))
            {
                return issued;
            }

            var body = _authorityBodyLookup[bodyEntity];
            var principalSeat = body.ExecutiveSeat;
            if (principalSeat == Entity.Null)
            {
                return issued;
            }

            var actingSeat = principalSeat;
            var attribution = AuthorityAttributionMode.AsDelegateSeat;

            if (_authorityDelegationLookup.HasBuffer(principalSeat))
            {
                var delegations = _authorityDelegationLookup[principalSeat];
                for (int i = 0; i < delegations.Length; i++)
                {
                    var delegation = delegations[i];
                    if ((delegation.Domains & domain) == 0 || (delegation.GrantedRights & AuthoritySeatRights.Issue) == 0)
                    {
                        continue;
                    }

                    var candidateSeat = delegation.DelegateSeat;
                    if (candidateSeat == Entity.Null || !_authorityOccupantLookup.HasComponent(candidateSeat))
                    {
                        continue;
                    }

                    if (_authorityOccupantLookup[candidateSeat].OccupantEntity == Entity.Null)
                    {
                        continue;
                    }

                    actingSeat = candidateSeat;
                    attribution = delegation.Attribution;
                    break;
                }
            }

            var issuingSeat = actingSeat != principalSeat && attribution == AuthorityAttributionMode.AsPrincipalSeat
                ? principalSeat
                : actingSeat;

            issued.ActingSeat = actingSeat;
            issued.IssuingSeat = issuingSeat;
            issued.ActingOccupant = _authorityOccupantLookup.HasComponent(actingSeat)
                ? _authorityOccupantLookup[actingSeat].OccupantEntity
                : Entity.Null;
            issued.IssuingOccupant = _authorityOccupantLookup.HasComponent(issuingSeat)
                ? _authorityOccupantLookup[issuingSeat].OccupantEntity
                : Entity.Null;

            return issued;
        }

        private Entity SelectWorker(in float3 targetPosition, DynamicBuffer<VillageMember> members)
        {
            var bestEntity = Entity.Null;
            var bestDistSq = float.MaxValue;
            var bestIndex = int.MaxValue;

            if (members.Length == 0)
            {
                return Entity.Null;
            }

            for (int i = 0; i < members.Length; i++)
            {
                var candidate = members[i].VillagerEntity;
                if (!IsWorkerCandidate(candidate))
                {
                    continue;
                }

                if (!_transformLookup.HasComponent(candidate))
                {
                    continue;
                }

                var distSq = math.distancesq(_transformLookup[candidate].Position.xz, targetPosition.xz);
                if (distSq < bestDistSq || (math.abs(distSq - bestDistSq) < 1e-4f && candidate.Index < bestIndex))
                {
                    bestEntity = candidate;
                    bestDistSq = distSq;
                    bestIndex = candidate.Index;
                }
            }

            return bestEntity;
        }

        private bool IsWorkerCandidate(Entity candidate)
        {
            if (candidate == Entity.Null)
            {
                return false;
            }

            if (_workerLookup.HasComponent(candidate))
            {
                return false;
            }

            if (!_needLookup.HasComponent(candidate))
            {
                return false;
            }

            if (_availabilityLookup.HasComponent(candidate))
            {
                var availability = _availabilityLookup[candidate];
                if (availability.IsReserved != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static int SelectHighestPriorityRequest(DynamicBuffer<VillageExpansionRequest> requests)
        {
            var bestIndex = 0;
            var bestPriority = -1;
            var bestTick = uint.MaxValue;

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                var priority = request.Priority;
                if (priority > bestPriority || (priority == bestPriority && request.RequestTick < bestTick))
                {
                    bestIndex = i;
                    bestPriority = priority;
                    bestTick = request.RequestTick;
                }
            }

            return bestIndex;
        }

        private static VillagerAIState CreateAiTravelling(Entity target, in float3 targetPosition, uint tick)
        {
            return new VillagerAIState
            {
                CurrentState = VillagerAIState.State.Travelling,
                CurrentGoal = VillagerAIState.Goal.Work,
                TargetEntity = target,
                TargetPosition = targetPosition,
                StateTimer = 0f,
                StateStartTick = tick
            };
        }

        private static VillagerJob CreateBuilderJob(uint ticketId, uint tick)
        {
            return new VillagerJob
            {
                Type = VillagerJob.JobType.Builder,
                Phase = VillagerJob.JobPhase.Gathering,
                ActiveTicketId = ticketId,
                Productivity = 1f,
                LastStateChangeTick = tick
            };
        }

        private static VillagerJobTicket CreateBuilderTicket(uint ticketId, byte priority, Entity site, uint tick)
        {
            return new VillagerJobTicket
            {
                TicketId = ticketId,
                JobType = VillagerJob.JobType.Builder,
                ResourceTypeIndex = 0,
                ResourceEntity = site,
                StorehouseEntity = Entity.Null,
                Priority = priority,
                Phase = (byte)VillagerJob.JobPhase.Gathering,
                ReservedUnits = 0f,
                AssignedTick = tick,
                LastProgressTick = tick
            };
        }

        private static void AddOrSet<T>(ref EntityCommandBuffer ecb, Entity entity, in T value, bool hasComponent)
            where T : unmanaged, IComponentData
        {
            if (hasComponent)
            {
                ecb.SetComponent(entity, value);
            }
            else
            {
                ecb.AddComponent(entity, value);
            }
        }

        private static void AddConstructionPresentation(ref EntityCommandBuffer ecb, Entity siteEntity, VillageBuildingType buildingType)
        {
            var tint = ResolveConstructionTint(buildingType);
            GodgamePresentationUtility.ApplyScenarioRenderContract(ref ecb, siteEntity, GodgameSemanticKeys.ConstructionGhost, default);
            ecb.AddComponent(siteEntity, new PresentationLODState
            {
                CurrentLOD = PresentationLOD.LOD0_Full,
                DistanceToCamera = 0f,
                ShouldRender = 1
            });
            ecb.AddComponent(siteEntity, new RenderTint { Value = tint });
        }

        private static float4 ResolveConstructionTint(VillageBuildingType buildingType)
        {
            var semanticKey = buildingType switch
            {
                VillageBuildingType.Housing => GodgameSemanticKeys.Housing,
                VillageBuildingType.Storehouse => GodgameSemanticKeys.Storehouse,
                VillageBuildingType.Worship => GodgameSemanticKeys.Worship,
                _ => GodgameSemanticKeys.ConstructionGhost
            };

            var color = GodgamePresentationColors.ForBuilding(semanticKey);
            color.w = math.min(color.w, 0.75f);
            return color;
        }
    }
}
