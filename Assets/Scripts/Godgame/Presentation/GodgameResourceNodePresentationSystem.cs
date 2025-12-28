using Godgame.Registry;
using Godgame.Rendering;
using Godgame.Resources;
using Godgame.Scenario;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Adds presentation components to resource nodes so villagers have visible destinations.
    /// </summary>
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
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
            if (!RuntimeMode.IsRenderingEnabled || _nodeQuery.IsEmptyIgnoreFilter)
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

                ushort semanticKey = ResolveSemanticKey(node.ValueRO.ResourceTypeIndex);
                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, semanticKey, default);

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

                ushort semanticKey = ResolveSemanticKey(config.ValueRO.ResourceTypeId);
                GodgamePresentationUtility.AssignRenderComponents(ref ecbSecondary, entity, semanticKey, default);

                var tint = GodgamePresentationColors.ForResourceId(config.ValueRO.ResourceTypeId);
                ecbSecondary.SetComponent(entity, new RenderTint { Value = tint });
            }

            ecbSecondary.Playback(state.EntityManager);
            ecbSecondary.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }

        private static ushort ResolveSemanticKey(ushort resourceTypeIndex)
        {
            return IsWoodType(resourceTypeIndex)
                ? GodgameSemanticKeys.Vegetation
                : GodgameSemanticKeys.ResourceNode;
        }

        private static ushort ResolveSemanticKey(in FixedString64Bytes resourceId)
        {
            return IsWoodId(resourceId)
                ? GodgameSemanticKeys.Vegetation
                : GodgameSemanticKeys.ResourceNode;
        }

        private static bool IsWoodType(ushort resourceTypeIndex)
        {
            return resourceTypeIndex >= 10 && resourceTypeIndex <= 14;
        }

        private static bool IsWoodId(in FixedString64Bytes resourceId)
        {
            return resourceId.Equals(WoodId) || resourceId.Equals(TimberId);
        }

        private static readonly FixedString64Bytes WoodId = CreateId('w', 'o', 'o', 'd');
        private static readonly FixedString64Bytes TimberId = CreateId('t', 'i', 'm', 'b', 'e', 'r');

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            return id;
        }

        private static FixedString64Bytes CreateId(char c0, char c1, char c2, char c3, char c4, char c5)
        {
            var id = new FixedString64Bytes();
            id.Append(c0);
            id.Append(c1);
            id.Append(c2);
            id.Append(c3);
            id.Append(c4);
            id.Append(c5);
            return id;
        }
    }
}
