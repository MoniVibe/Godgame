using Godgame.Environment.Vegetation;
using Godgame.Rendering;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Adds presentation components to vegetation entities so stands are visible.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(GodgamePresentationRegistryIdentitySystem))]
    public partial struct Godgame_VegetationPresentationSystem : ISystem
    {
        private EntityQuery _missingVegetationPresentationQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingVegetationPresentationQuery = SystemAPI.QueryBuilder()
                .WithAll<PlantState, LocalTransform>()
                .WithNone<VegetationPresentationTag>()
                .Build();

            state.RequireForUpdate(_missingVegetationPresentationQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled || _missingVegetationPresentationQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (plant, entity) in SystemAPI
                         .Query<RefRO<PlantState>>()
                         .WithAll<LocalTransform>()
                         .WithNone<VegetationPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent<VegetationPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(entity, new VegetationVisualState
                {
                    GrowthStage = (byte)plant.ValueRO.Stage,
                    Health = plant.ValueRO.Health01,
                    IsClumped = (byte)(SystemAPI.HasComponent<PlantStand>(entity) ? 1 : 0),
                    BiomeTint = new float4(1f, 1f, 1f, 1f)
                });

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, entity, GodgameSemanticKeys.Vegetation, default);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
