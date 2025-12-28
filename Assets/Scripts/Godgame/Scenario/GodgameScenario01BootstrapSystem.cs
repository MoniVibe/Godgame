using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Godgame.Presentation;
using Godgame.Villagers;
using Godgame.Villages;
using Godgame.Economy;
using Godgame.Scenario;
using PureDOTS.Rendering;

namespace Godgame.Scenario
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameScenario01BootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<GodgameScenarioConfigBlobReference>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!IsScenario01(ref state))
            {
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            bool anyWork = false;

            // Add presentation to villagers
            foreach (var (behavior, entity) in SystemAPI.Query<RefRO<VillagerBehavior>>().WithNone<VillagerPresentationTag>().WithEntityAccess())
            {
                if (!SystemAPI.HasComponent<LocalTransform>(entity)) continue;

                ecb.AddComponent<VillagerPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(entity, new VillagerVisualState
                {
                    AlignmentTint = new float4(0.7f, 0.7f, 0.7f, 1f),
                    TaskIconIndex = 0,
                    AnimationState = 0,
                    EffectIntensity = 0f
                });
                anyWork = true;
            }

            // Add presentation to villages
            foreach (var (village, entity) in SystemAPI.Query<RefRO<Village>>().WithNone<VillageCenterPresentationTag>().WithEntityAccess())
            {
                if (!SystemAPI.HasComponent<LocalTransform>(entity)) continue;

                ecb.AddComponent<VillageCenterPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(entity, new VillageCenterVisualState
                {
                    PhaseTint = new float4(0.2f, 0.4f, 0.8f, 1f),
                    InfluenceRadius = village.ValueRO.InfluenceRadius,
                    Intensity = 1f
                });
                
                if (!SystemAPI.HasBuffer<VillageMember>(entity))
                {
                    ecb.AddBuffer<VillageMember>(entity);
                }
                anyWork = true;
            }

            // Add presentation to resource chunks
            foreach (var (resource, entity) in SystemAPI.Query<RefRO<ExtractedResource>>().WithNone<ResourceChunkPresentationTag>().WithEntityAccess())
            {
                if (!SystemAPI.HasComponent<LocalTransform>(entity)) continue;

                ecb.AddComponent<ResourceChunkPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                var chunkTint = GodgamePresentationColors.ForResourceType(resource.ValueRO.Type);
                ecb.AddComponent(entity, new ResourceChunkVisualState
                {
                    ResourceTypeTint = chunkTint,
                    QuantityScale = 0.5f + resource.ValueRO.Quantity * 0.01f,
                    IsCarried = 0
                });
                if (SystemAPI.HasComponent<RenderTint>(entity))
                {
                    ecb.SetComponent(entity, new RenderTint { Value = chunkTint });
                }
                else
                {
                    ecb.AddComponent(entity, new RenderTint { Value = chunkTint });
                }
                anyWork = true;
            }

            if (anyWork)
            {
                ecb.Playback(state.EntityManager);
            }
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }

        private bool IsScenario01(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GodgameScenarioConfigBlobReference>(out var configRef))
            {
                return false;
            }

            if (!configRef.Config.IsCreated)
            {
                return false;
            }

            return configRef.Config.Value.Mode == GodgameScenarioMode.Scenario01;
        }
    }
}
