using Godgame.Registry;
using Godgame.Rendering;
using Godgame.Resources;
using Godgame.Scenario;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Adds presentation components to resource nodes so villagers have visible destinations.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_ResourceNodePresentationSystem : ISystem
    {
        private EntityQuery _nodeQuery;

        public void OnCreate(ref SystemState state)
        {
            _nodeQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameResourceNode, LocalTransform>()
                .WithNone<ResourceNodePresentationTag>()
                .Build();

            state.RequireForUpdate(_nodeQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless || _nodeQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (node, entity) in SystemAPI
                         .Query<RefRO<GodgameResourceNode>>()
                         .WithAll<LocalTransform>()
                         .WithNone<ResourceNodePresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<ResourceNodePresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    ShouldRender = 1,
                    DistanceToCamera = 0f
                });

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, GodgameSemanticKeys.ResourceNode, default);

                var tint = GodgamePresentationColors.ForResourceTypeIndex(node.ValueRO.ResourceTypeIndex);
                ecb.SetComponent(entity, new RenderTint { Value = tint });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            var ecbSecondary = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (config, entity) in SystemAPI
                         .Query<RefRO<ResourceSourceConfig>>()
                         .WithAll<LocalTransform>()
                         .WithNone<ResourceNodePresentationTag>()
                         .WithEntityAccess())
            {
                ecbSecondary.AddComponent<ResourceNodePresentationTag>(entity);
                ecbSecondary.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    ShouldRender = 1,
                    DistanceToCamera = 0f
                });

                GodgamePresentationUtility.AssignRenderComponents(ref ecbSecondary, entity, GodgameSemanticKeys.ResourceNode, default);

                var tint = GodgamePresentationColors.ForResourceId(config.ValueRO.ResourceTypeId);
                ecbSecondary.SetComponent(entity, new RenderTint { Value = tint });
            }

            ecbSecondary.Playback(state.EntityManager);
            ecbSecondary.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
