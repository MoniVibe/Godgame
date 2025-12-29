using System;
using Godgame.Construction;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using SystemEnv = System.Environment;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless proof that a village-issued build request results in at least one completed jobsite.
    /// Disabled by default; enable via <see cref="EnabledEnv"/>.
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.LateSimulationSystemGroup), OrderLast = true)]
    public partial struct GodgameHeadlessVillageBuildProofSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_VILLAGE_BUILD_PROOF";
        private const string ExitOnResultEnv = "GODGAME_HEADLESS_VILLAGE_BUILD_PROOF_EXIT";
        private const uint DefaultTimeoutTicks = 1800; // ~30 seconds at 60hz

        private byte _done;
        private byte _seededRequest;
        private uint _startTick;
        private uint _timeoutTick;

        private static readonly FixedString32Bytes Expected = new FixedString32Bytes(">=1");
        private static readonly FixedString32Bytes Step = new FixedString32Bytes("village_build");

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            EnsureHeadlessBuildSliceConfig(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_done != 0)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            EnsureBuildRequest(ref state, timeState.Tick);

            if (_timeoutTick == 0u)
            {
                _startTick = timeState.Tick;
                _timeoutTick = _startTick + DefaultTimeoutTicks;
            }

            var completedCount = 0;
            foreach (var (flags, site) in SystemAPI.Query<RefRO<ConstructionSiteFlags>, RefRO<VillageConstructionSite>>())
            {
                if ((flags.ValueRO.Value & ConstructionSiteFlags.Completed) != 0)
                {
                    completedCount = 1;
                    break;
                }
            }

            if (completedCount > 0)
            {
                _done = 1;
                UnityDebug.Log($"[GodgameHeadlessVillageBuildProof] PASS tick={timeState.Tick} completedSites={completedCount} timeoutTicks={DefaultTimeoutTicks}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Construction, true, completedCount, Expected, DefaultTimeoutTicks, step: Step);
                ExitIfRequested(ref state, timeState.Tick, 0);
                return;
            }

            if (timeState.Tick >= _timeoutTick)
            {
                _done = 1;
                UnityDebug.LogError($"[GodgameHeadlessVillageBuildProof] FAIL tick={timeState.Tick} completedSites={completedCount} timeoutTicks={DefaultTimeoutTicks}");
                TelemetryLoopProofUtility.Emit(state.EntityManager, timeState.Tick, TelemetryLoopIds.Construction, false, completedCount, Expected, DefaultTimeoutTicks, step: Step);
                ExitIfRequested(ref state, timeState.Tick, 5);
            }
        }

        private static void EnsureHeadlessBuildSliceConfig(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<VillageBuildSliceConfig>());
            if (query.IsEmptyIgnoreFilter)
            {
                var entity = entityManager.CreateEntity(typeof(VillageBuildSliceConfig));
                entityManager.SetComponentData(entity, new VillageBuildSliceConfig { EnableInHeadless = 1 });
                return;
            }

            var existing = query.GetSingletonEntity();
            var config = entityManager.GetComponentData<VillageBuildSliceConfig>(existing);
            if (config.EnableInHeadless == 0)
            {
                config.EnableInHeadless = 1;
                entityManager.SetComponentData(existing, config);
            }
        }

        private static void ExitIfRequested(ref SystemState state, uint tick, int exitCode)
        {
            if (!string.Equals(SystemEnv.GetEnvironmentVariable(ExitOnResultEnv), "1", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            GodgameHeadlessExitSystem.Request(ref state, tick, exitCode);
        }

        private void EnsureBuildRequest(ref SystemState state, uint tick)
        {
            if (_seededRequest != 0)
            {
                return;
            }

            foreach (var (village, requests, entity) in SystemAPI.Query<RefRO<Village>, DynamicBuffer<VillageExpansionRequest>>()
                         .WithEntityAccess())
            {
                if (requests.Length > 0)
                {
                    _seededRequest = 1;
                    return;
                }

                var center = village.ValueRO.CenterPosition;
                if (state.EntityManager.HasComponent<LocalTransform>(entity))
                {
                    center = state.EntityManager.GetComponentData<LocalTransform>(entity).Position;
                }

                requests.Add(new VillageExpansionRequest
                {
                    BuildingType = (byte)VillageBuildingType.Storehouse,
                    Position = center + new float3(6f, 0f, 0f),
                    Priority = 80,
                    RequestTick = tick
                });

                _seededRequest = 1;
                return;
            }
        }
    }

    /// <summary>
    /// Headless-only assist to ensure village construction sites advance when no worker is assigned.
    /// Keeps build proof deterministic without relying on full presentation-driven flows.
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Villages.VillageConstructionDispatchSystem))]
    public partial struct GodgameHeadlessVillageBuildAssistSystem : ISystem
    {
        private const string EnabledEnv = "GODGAME_HEADLESS_VILLAGE_BUILD_PROOF";
        private const float DefaultAssistRate = 2f;

        private byte _enabled;
        private byte _tickInitialized;
        private uint _lastTick;

        private ComponentLookup<ConstructionSiteProgress> _progressLookup;
        private ComponentLookup<ConstructionSiteFlags> _flagsLookup;
        private ComponentLookup<VillageConstructionRuntime> _runtimeLookup;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
                return;
            }

            var enabled = SystemEnv.GetEnvironmentVariable(EnabledEnv);
            if (string.Equals(enabled, "0", StringComparison.OrdinalIgnoreCase))
            {
                state.Enabled = false;
                return;
            }

            _enabled = 1;
            _tickInitialized = 0;
            _lastTick = 0;

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<JobsitePlacementState>();
            state.RequireForUpdate<VillageConstructionRuntime>();

            _progressLookup = state.GetComponentLookup<ConstructionSiteProgress>(false);
            _flagsLookup = state.GetComponentLookup<ConstructionSiteFlags>(false);
            _runtimeLookup = state.GetComponentLookup<VillageConstructionRuntime>(false);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_enabled == 0)
            {
                return;
            }

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

            _progressLookup.Update(ref state);
            _flagsLookup.Update(ref state);
            _runtimeLookup.Update(ref state);

            TrySeedSite(ref state, timeState.Tick);
            AdvanceSites(ref state, deltaTime);
        }

        private void TrySeedSite(ref SystemState state, uint tick)
        {
            foreach (var runtime in SystemAPI.Query<RefRO<VillageConstructionRuntime>>())
            {
                if (runtime.ValueRO.ActiveSite != Entity.Null)
                {
                    return;
                }
            }

            if (SystemAPI.QueryBuilder().WithAll<ConstructionSiteFlags, VillageConstructionSite>().Build().IsEmptyIgnoreFilter == false)
            {
                return;
            }

            var placementConfig = SystemAPI.GetSingleton<JobsitePlacementConfig>();
            var placementEntity = SystemAPI.GetSingletonEntity<JobsitePlacementState>();
            var placementState = SystemAPI.GetComponentRW<JobsitePlacementState>(placementEntity);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (requests, village, entity) in SystemAPI.Query<DynamicBuffer<VillageExpansionRequest>, RefRO<Village>>()
                         .WithEntityAccess())
            {
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

                var siteId = placementState.ValueRO.NextSiteId;
                placementState.ValueRW.NextSiteId = siteId + 1;

                var siteEntity = ecb.CreateEntity();
                ecb.AddComponent(siteEntity, new JobsiteGhost { CompletionRequested = 0 });
                ecb.AddComponent(siteEntity, new ConstructionSiteId { Value = siteId });
                ecb.AddComponent(siteEntity, new ConstructionSiteProgress
                {
                    RequiredProgress = math.max(0.01f, placementConfig.DefaultRequiredProgress),
                    CurrentProgress = 0f
                });
                ecb.AddComponent(siteEntity, new ConstructionSiteFlags { Value = 0 });
                ecb.AddBuffer<ConstructionProgressCommand>(siteEntity);
                ecb.AddComponent(siteEntity, LocalTransform.FromPositionRotationScale(request.Position, quaternion.identity, 6f));
                ecb.AddComponent(siteEntity, new VillageConstructionSite
                {
                    Village = entity,
                    BuildingType = buildingType,
                    Priority = request.Priority,
                    IssuedTick = tick
                });

                var runtime = _runtimeLookup.HasComponent(entity)
                    ? _runtimeLookup[entity]
                    : new VillageConstructionRuntime();
                runtime.ActiveSite = siteEntity;
                runtime.ActiveWorker = Entity.Null;
                runtime.ActiveBuildingType = buildingType;
                runtime.LastIssuedTick = tick;
                if (_runtimeLookup.HasComponent(entity))
                {
                    ecb.SetComponent(entity, runtime);
                }
                else
                {
                    ecb.AddComponent(entity, runtime);
                }

                requests.RemoveAt(requestIndex);
                break;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void AdvanceSites(ref SystemState state, float deltaTime)
        {
            var placementConfig = SystemAPI.GetSingleton<JobsitePlacementConfig>();
            var assistRate = math.max(DefaultAssistRate, placementConfig.BuildRatePerSecond);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var usedEcb = false;

            foreach (var runtime in SystemAPI.Query<RefRO<VillageConstructionRuntime>>())
            {
                var site = runtime.ValueRO.ActiveSite;
                if (site == Entity.Null)
                {
                    continue;
                }

                if (!_progressLookup.HasComponent(site) || !_flagsLookup.HasComponent(site))
                {
                    continue;
                }

                var flags = _flagsLookup[site];
                if ((flags.Value & ConstructionSiteFlags.Completed) != 0)
                {
                    continue;
                }

                var progress = _progressLookup[site];
                progress.CurrentProgress = math.min(progress.CurrentProgress + assistRate * deltaTime, progress.RequiredProgress);
                _progressLookup[site] = progress;

                if (progress.CurrentProgress >= progress.RequiredProgress)
                {
                    flags.Value |= ConstructionSiteFlags.Completed;
                    _flagsLookup[site] = flags;

                    if (state.EntityManager.HasComponent<JobsiteGhost>(site))
                    {
                        var ghost = state.EntityManager.GetComponentData<JobsiteGhost>(site);
                        ghost.CompletionRequested = 1;
                        state.EntityManager.SetComponentData(site, ghost);
                    }

                    if (!state.EntityManager.HasComponent<JobsiteCompletionTag>(site))
                    {
                        ecb.AddComponent<JobsiteCompletionTag>(site);
                        usedEcb = true;
                    }
                }
            }

            if (usedEcb)
            {
                ecb.Playback(state.EntityManager);
            }
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
                    bestPriority = priority;
                    bestIndex = i;
                    bestTick = request.RequestTick;
                }
            }

            return bestIndex;
        }
    }
}
