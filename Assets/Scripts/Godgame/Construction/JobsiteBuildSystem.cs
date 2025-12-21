using Godgame.Construction;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Construction
{
    /// <summary>
    /// Advances ghost jobsite progress until completion and marks entities for telemetry/effect emission.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(ConstructionSystemGroup))]
    public partial struct JobsiteBuildSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<JobsitePlacementState>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
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

            var config = SystemAPI.GetSingleton<JobsitePlacementConfig>();
            var delta = config.BuildRatePerSecond * math.max(timeState.FixedDeltaTime, 0f);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (progress, flags, ghost, entity) in SystemAPI
                         .Query<RefRW<ConstructionSiteProgress>, RefRW<ConstructionSiteFlags>, RefRW<JobsiteGhost>>()
                         .WithNone<VillageConstructionSite>()
                         .WithEntityAccess())
            {
                // Skip if completion already finalized.
                if (ghost.ValueRO.CompletionRequested != 0 || (flags.ValueRO.Value & ConstructionSiteFlags.Completed) != 0)
                {
                    if (!state.EntityManager.HasComponent<JobsiteCompletionTag>(entity))
                    {
                        ecb.AddComponent<JobsiteCompletionTag>(entity);
                    }
                    continue;
                }

                if (delta > 0f)
                {
                    progress.ValueRW.CurrentProgress = math.min(
                        progress.ValueRO.CurrentProgress + delta,
                        progress.ValueRO.RequiredProgress);
                }

                if (progress.ValueRO.CurrentProgress >= progress.ValueRO.RequiredProgress)
                {
                    flags.ValueRW.Value |= ConstructionSiteFlags.Completed;
                    ghost.ValueRW.CompletionRequested = 1;
                    ecb.AddComponent<JobsiteCompletionTag>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
