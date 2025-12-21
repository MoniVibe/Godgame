using Godgame.Construction;
using Godgame.Scenario;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Executes village-issued construction orders: travel to the jobsite, then advance progress until completion.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(VillagerSystemGroup), OrderLast = true)]
    public partial struct VillageConstructionWorkerSystem : ISystem
    {
        private uint _lastTick;
        private byte _tickInitialized;

        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<ConstructionSiteProgress> _progressLookup;
        private ComponentLookup<ConstructionSiteFlags> _flagsLookup;
        private ComponentLookup<JobsiteGhost> _ghostLookup;
        private ComponentLookup<VillageConstructionRuntime> _constructionLookup;
        private ComponentLookup<VillagerAvailability> _availabilityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<VillageConstructionWorker>();

            _tickInitialized = 0;
            _lastTick = 0;

            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _progressLookup = state.GetComponentLookup<ConstructionSiteProgress>();
            _flagsLookup = state.GetComponentLookup<ConstructionSiteFlags>();
            _ghostLookup = state.GetComponentLookup<JobsiteGhost>();
            _constructionLookup = state.GetComponentLookup<VillageConstructionRuntime>();
            _availabilityLookup = state.GetComponentLookup<VillagerAvailability>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var deltaTime = ResolveDeltaTime(timeState, out var deltaTicks);
            if (deltaTicks == 0u || deltaTime <= 0f)
            {
                return;
            }

            state.EntityManager.CompleteDependencyBeforeRW<LocalTransform>();
            state.EntityManager.CompleteDependencyBeforeRW<ConstructionSiteProgress>();
            state.EntityManager.CompleteDependencyBeforeRW<ConstructionSiteFlags>();
            state.EntityManager.CompleteDependencyBeforeRW<JobsiteGhost>();

            _transformLookup.Update(ref state);
            _progressLookup.Update(ref state);
            _flagsLookup.Update(ref state);
            _ghostLookup.Update(ref state);
            _constructionLookup.Update(ref state);
            _availabilityLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (worker, transform, ai, job, ticket, entity) in SystemAPI
                         .Query<RefRO<VillageConstructionWorker>, RefRW<LocalTransform>, RefRW<VillagerAIState>, RefRW<VillagerJob>, RefRW<VillagerJobTicket>>()
                         .WithEntityAccess())
            {
                var workerData = worker.ValueRO;
                var site = workerData.Site;

                if (site == Entity.Null || !_transformLookup.HasComponent(site))
                {
                    SetIdle(ref ai.ValueRW, ref job.ValueRW, ref ticket.ValueRW, timeState.Tick);
                    ClearVillageRuntime(ref ecb, workerData, entity);
                    ReleaseWorker(ref ecb, entity, workerData, timeState.Tick);
                    continue;
                }

                var sitePosition = _transformLookup[site].Position;
                var position = transform.ValueRO.Position;
                var toTarget = sitePosition - position;
                toTarget.y = 0f;
                var distSq = math.lengthsq(toTarget);

                if (distSq > workerData.ArrivalDistanceSq)
                {
                    MoveToward(ref position, toTarget, workerData.MoveSpeed, deltaTime);
                    transform.ValueRW.Position = position;
                    SetTravelling(ref ai.ValueRW, ref job.ValueRW, ref ticket.ValueRW, site, sitePosition, timeState.Tick);
                    continue;
                }

                SetBuilding(ref ai.ValueRW, ref job.ValueRW, ref ticket.ValueRW, site, sitePosition, timeState.Tick);

                if (_progressLookup.HasComponent(site) && _flagsLookup.HasComponent(site))
                {
                    var progress = _progressLookup[site];
                    var flags = _flagsLookup[site];
                    var completed = (flags.Value & ConstructionSiteFlags.Completed) != 0;

                    if (!completed)
                    {
                        var delta = workerData.BuildRatePerSecond * deltaTime;
                        if (delta > 0f)
                        {
                            progress.CurrentProgress = math.min(progress.CurrentProgress + delta, progress.RequiredProgress);
                            _progressLookup[site] = progress;
                        }

                        if (progress.CurrentProgress >= progress.RequiredProgress)
                        {
                            flags.Value |= ConstructionSiteFlags.Completed;
                            _flagsLookup[site] = flags;

                            if (_ghostLookup.HasComponent(site))
                            {
                                var ghost = _ghostLookup[site];
                                ghost.CompletionRequested = 1;
                                _ghostLookup[site] = ghost;
                            }

                            ecb.AddComponent<JobsiteCompletionTag>(site);
                            completed = true;
                        }
                    }

                    if (completed)
                    {
                        ClearVillageRuntime(ref ecb, workerData, entity);
                        SetCompleted(ref ai.ValueRW, ref job.ValueRW, ref ticket.ValueRW, site, sitePosition, timeState.Tick);
                        ReleaseWorker(ref ecb, entity, workerData, timeState.Tick);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private float ResolveDeltaTime(in TimeState timeState, out uint deltaTicks)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                deltaTicks = 0u;
                return 0f;
            }

            deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }

        private static void MoveToward(ref float3 position, float3 toTarget, float speed, float deltaTime)
        {
            var distance = math.length(toTarget);
            if (distance <= 1e-4f)
            {
                return;
            }

            var maxMove = math.max(0f, speed) * deltaTime;
            var move = math.min(distance, maxMove);
            var dir = toTarget / distance;
            position += dir * move;
        }

        private static void SetTravelling(ref VillagerAIState ai, ref VillagerJob job, ref VillagerJobTicket ticket, Entity site, float3 sitePosition, uint tick)
        {
            ai.CurrentState = VillagerAIState.State.Travelling;
            ai.CurrentGoal = VillagerAIState.Goal.Work;
            ai.TargetEntity = site;
            ai.TargetPosition = sitePosition;
            ai.StateTimer = 0f;
            ai.StateStartTick = tick;

            job.Type = VillagerJob.JobType.Builder;
            job.Phase = VillagerJob.JobPhase.Gathering;
            job.LastStateChangeTick = tick;

            ticket.ResourceEntity = site;
            ticket.Phase = (byte)VillagerJob.JobPhase.Gathering;
            ticket.LastProgressTick = tick;
        }

        private static void SetBuilding(ref VillagerAIState ai, ref VillagerJob job, ref VillagerJobTicket ticket, Entity site, float3 sitePosition, uint tick)
        {
            ai.CurrentState = VillagerAIState.State.Working;
            ai.CurrentGoal = VillagerAIState.Goal.Work;
            ai.TargetEntity = site;
            ai.TargetPosition = sitePosition;
            ai.StateTimer = 0f;
            ai.StateStartTick = tick;

            job.Type = VillagerJob.JobType.Builder;
            job.Phase = VillagerJob.JobPhase.Building;
            job.LastStateChangeTick = tick;

            ticket.ResourceEntity = site;
            ticket.Phase = (byte)VillagerJob.JobPhase.Building;
            ticket.LastProgressTick = tick;
        }

        private static void SetCompleted(ref VillagerAIState ai, ref VillagerJob job, ref VillagerJobTicket ticket, Entity site, float3 sitePosition, uint tick)
        {
            ai.CurrentState = VillagerAIState.State.Idle;
            ai.CurrentGoal = VillagerAIState.Goal.None;
            ai.TargetEntity = site;
            ai.TargetPosition = sitePosition;
            ai.StateTimer = 0f;
            ai.StateStartTick = tick;

            job.Phase = VillagerJob.JobPhase.Completed;
            job.LastStateChangeTick = tick;

            ticket.Phase = (byte)VillagerJob.JobPhase.Completed;
            ticket.LastProgressTick = tick;
        }

        private static void SetIdle(ref VillagerAIState ai, ref VillagerJob job, ref VillagerJobTicket ticket, uint tick)
        {
            ai.CurrentState = VillagerAIState.State.Idle;
            ai.CurrentGoal = VillagerAIState.Goal.None;
            ai.TargetEntity = Entity.Null;
            ai.TargetPosition = float3.zero;
            ai.StateTimer = 0f;
            ai.StateStartTick = tick;

            job.Phase = VillagerJob.JobPhase.Idle;
            job.LastStateChangeTick = tick;

            ticket.Phase = (byte)VillagerJob.JobPhase.Idle;
            ticket.LastProgressTick = tick;
        }

        private void ClearVillageRuntime(ref EntityCommandBuffer ecb, in VillageConstructionWorker worker, Entity villager)
        {
            if (worker.Village == Entity.Null)
            {
                return;
            }

            if (!_constructionLookup.HasComponent(worker.Village))
            {
                return;
            }

            var runtime = _constructionLookup[worker.Village];
            if (runtime.ActiveWorker != villager)
            {
                return;
            }

            runtime.ActiveSite = Entity.Null;
            runtime.ActiveWorker = Entity.Null;
            runtime.ActiveBuildingType = VillageBuildingType.None;
            ecb.SetComponent(worker.Village, runtime);
        }

        private void ReleaseWorker(ref EntityCommandBuffer ecb, Entity villager, in VillageConstructionWorker worker, uint tick)
        {
            ecb.RemoveComponent<VillageConstructionWorker>(villager);

            if (_availabilityLookup.HasComponent(villager))
            {
                var availability = _availabilityLookup[villager];
                availability.IsAvailable = 1;
                availability.IsReserved = 0;
                availability.LastChangeTick = tick;
                ecb.SetComponent(villager, availability);
            }

            if (worker.HadSettlementState != 0 && worker.ResumeSettlement != Entity.Null)
            {
                ecb.AddComponent(villager, new SettlementVillagerState
                {
                    Settlement = worker.ResumeSettlement,
                    CurrentResourceNode = Entity.Null,
                    CurrentDepot = Entity.Null,
                    Phase = SettlementVillagerPhase.Idle,
                    PhaseTimer = 0f,
                    RandomState = math.max(1u, worker.ResumeRandomState)
                });
            }
        }
    }
}
