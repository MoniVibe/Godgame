using Godgame.Construction;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Construction
{
    /// <summary>
    /// Handles hotkey + buffered jobsite placement requests and seeds construction site entities.
    /// </summary>
    [UpdateInGroup(typeof(ConstructionSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(JobsitePlacementHotkeySystem))]
    public partial struct JobsitePlacementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobsitePlacementState>();
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<JobsitePlacementRequest>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var placementEntity = SystemAPI.GetSingletonEntity<JobsitePlacementState>();
            var config = SystemAPI.GetComponent<JobsitePlacementConfig>(placementEntity);
            var placementState = SystemAPI.GetComponent<JobsitePlacementState>(placementEntity);
            var requests = SystemAPI.GetBuffer<JobsitePlacementRequest>(placementEntity);

            if (requests.Length == 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                int siteId = placementState.NextSiteId++;

                var siteEntity = ecb.CreateEntity();
                ecb.AddComponent(siteEntity, new JobsiteGhost { CompletionRequested = 0 });
                ecb.AddComponent(siteEntity, new ConstructionSiteId { Value = siteId });
                ecb.AddComponent(siteEntity, new ConstructionSiteProgress
                {
                    RequiredProgress = math.max(0.01f, config.DefaultRequiredProgress),
                    CurrentProgress = 0f
                });
                ecb.AddComponent(siteEntity, new ConstructionSiteFlags { Value = 0 });
                ecb.AddComponent(siteEntity, LocalTransform.FromPositionRotationScale(request.Position, quaternion.identity, 1f));
                ecb.AddBuffer<ConstructionProgressCommand>(siteEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            requests.Clear();
            SystemAPI.SetComponent(placementEntity, placementState);
        }
    }
}
