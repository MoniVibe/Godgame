using Godgame.Rendering;
using Godgame.Resources;
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
    /// Adds presentation components to aggregate piles so they render as resource spheres.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
    public partial struct Godgame_AggregatePilePresentationEnsureSystem : ISystem
    {
        private EntityQuery _pileQuery;

        public void OnCreate(ref SystemState state)
        {
            _pileQuery = SystemAPI.QueryBuilder()
                .WithAll<AggregatePile, AggregatePileTag, LocalTransform>()
                .WithNone<AggregatePilePresentationTag>()
                .Build();

            state.RequireForUpdate(_pileQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled || _pileQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (pile, entity) in SystemAPI
                         .Query<RefRO<AggregatePile>>()
                         .WithAll<AggregatePileTag, LocalTransform>()
                         .WithNone<AggregatePilePresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<AggregatePilePresentationTag>(entity);
                ecb.AddComponent<ResourceChunkPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    ShouldRender = 1,
                    DistanceToCamera = 0f
                });

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, GodgameSemanticKeys.ResourceChunk, default);

                var tint = GodgamePresentationColors.ForResourceTypeIndex(pile.ValueRO.ResourceTypeIndex);
                ecb.SetComponent(entity, new RenderTint { Value = tint });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
