using Godgame.Construction;
using Godgame.Rendering;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Adds presentation components to construction ghost entities so placement is visible.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(GodgamePresentationRegistryIdentitySystem))]
    public partial struct Godgame_ConstructionGhostPresentationSystem : ISystem
    {
        private EntityQuery _missingGhostQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingGhostQuery = SystemAPI.QueryBuilder()
                .WithAll<ConstructionGhost, LocalTransform>()
                .WithNone<ConstructionGhostPresentationTag>()
                .Build();

            state.RequireForUpdate(_missingGhostQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled || _missingGhostQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<ConstructionGhost>>()
                         .WithAll<LocalTransform>()
                         .WithNone<ConstructionGhostPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<ConstructionGhostPresentationTag>(entity);
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

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, GodgameSemanticKeys.ConstructionGhost, default);

                var tint = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.ConstructionGhost);
                ecb.AddComponent(entity, new RenderTint { Value = tint });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
