using Godgame.Rendering;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Adds presentation components to band entities so groups are visible.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(GodgamePresentationRegistryIdentitySystem))]
    public partial struct Godgame_BandPresentationSystem : ISystem
    {
        private EntityQuery _missingBandQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingBandQuery = SystemAPI.QueryBuilder()
                .WithAll<BandId, LocalTransform>()
                .WithNone<BandPresentationTag>()
                .Build();

            state.RequireForUpdate(_missingBandQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled || _missingBandQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<BandId>>()
                         .WithAll<LocalTransform>()
                         .WithNone<BandPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<BandPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });

                if (!SystemAPI.HasComponent<PresentationLayer>(entity))
                {
                    ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Colony });
                }

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, GodgameSemanticKeys.Band, default);

                var tint = GodgamePresentationColors.ForBand();
                ecb.AddComponent(entity, new RenderTint { Value = tint });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
