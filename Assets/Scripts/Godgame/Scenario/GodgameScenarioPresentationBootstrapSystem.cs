using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Rendering;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Scenario
{
    /// <summary>
    /// Ensures baseline presentation components exist for scenario-spawned entities.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameScenarioPresentationBootstrapSystem : ISystem
    {
        private EntityQuery _presentationConfigQuery;
        private EntityQuery _missingVillagerPresentationQuery;
        private EntityQuery _missingVillagePresentationQuery;
        private EntityQuery _missingResourceChunkPresentationQuery;

        public void OnCreate(ref SystemState state)
        {
            _presentationConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<PresentationConfig>());
            _missingVillagerPresentationQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<VillagerBehavior>() },
                None = new[] { ComponentType.ReadOnly<VillagerPresentationTag>() }
            });
            _missingVillagePresentationQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Village>() },
                None = new[] { ComponentType.ReadOnly<VillageCenterPresentationTag>() }
            });
            _missingResourceChunkPresentationQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<ExtractedResource>() },
                None = new[] { ComponentType.ReadOnly<ResourceChunkPresentationTag>() }
            });
            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<GameWorldTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EnsurePresentationConfig(ref state);
            if (_missingVillagerPresentationQuery.IsEmptyIgnoreFilter &&
                _missingVillagePresentationQuery.IsEmptyIgnoreFilter &&
                _missingResourceChunkPresentationQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            AddPresentationToScenarioEntities(ref state);
        }

        private void EnsurePresentationConfig(ref SystemState state)
        {
            if (!_presentationConfigQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var configEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(configEntity, PresentationConfig.Default);
            state.EntityManager.SetName(configEntity, "PresentationConfig");
        }

        private void AddPresentationToScenarioEntities(ref SystemState state)
        {
            AddPresentationToVillagers(ref state);
            AddPresentationToVillages(ref state);
            AddPresentationToResourceChunks(ref state);
        }

        private void AddPresentationToVillagers(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<VillagerBehavior>>()
                         .WithNone<VillagerPresentationTag>()
                         .WithEntityAccess())
            {
                if (!state.EntityManager.HasComponent<LocalTransform>(entity))
                {
                    continue;
                }

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
                var resolvedKey = VillagerRenderKeyUtility.ResolveVillagerRenderKey(state.EntityManager, entity);
                GodgamePresentationUtility.ApplyScenarioRenderContract(ref ecb, entity, resolvedKey, default);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void AddPresentationToVillages(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (village, entity) in SystemAPI.Query<RefRO<Village>>()
                         .WithNone<VillageCenterPresentationTag>()
                         .WithEntityAccess())
            {
                if (!state.EntityManager.HasComponent<LocalTransform>(entity))
                {
                    continue;
                }

                ecb.AddComponent<VillageCenterPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(entity, new VillageCenterVisualState
                {
                    PhaseTint = GetPhaseColor(village.ValueRO.Phase),
                    InfluenceRadius = village.ValueRO.InfluenceRadius,
                    Intensity = 1f
                });
                GodgamePresentationUtility.ApplyScenarioRenderContract(ref ecb, entity, GodgameSemanticKeys.VillageCenter, default);

                var villageTint = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.VillageCenter);
                if (state.EntityManager.HasComponent<RenderTint>(entity))
                {
                    ecb.SetComponent(entity, new RenderTint { Value = villageTint });
                }
                else
                {
                    ecb.AddComponent(entity, new RenderTint { Value = villageTint });
                }

                if (!state.EntityManager.HasBuffer<VillageMember>(entity))
                {
                    ecb.AddBuffer<VillageMember>(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private void AddPresentationToResourceChunks(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (resource, entity) in SystemAPI.Query<RefRO<ExtractedResource>>()
                         .WithNone<ResourceChunkPresentationTag>()
                         .WithEntityAccess())
            {
                if (!state.EntityManager.HasComponent<LocalTransform>(entity))
                {
                    continue;
                }

                ecb.AddComponent<ResourceChunkPresentationTag>(entity);
                ecb.AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(entity, new ResourceChunkVisualState
                {
                    ResourceTypeTint = GodgamePresentationColors.ForResourceType(resource.ValueRO.Type),
                    QuantityScale = 0.5f + resource.ValueRO.Quantity * 0.01f,
                    IsCarried = 0
                });
                GodgamePresentationUtility.ApplyScenarioRenderContract(ref ecb, entity, GodgameSemanticKeys.ResourceChunk, default);

                var chunkTint = GodgamePresentationColors.ForResourceType(resource.ValueRO.Type);
                if (state.EntityManager.HasComponent<RenderTint>(entity))
                {
                    ecb.SetComponent(entity, new RenderTint { Value = chunkTint });
                }
                else
                {
                    ecb.AddComponent(entity, new RenderTint { Value = chunkTint });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static float4 GetPhaseColor(VillagePhase phase)
        {
            return phase switch
            {
                VillagePhase.Forming => new float4(0.5f, 0.5f, 0.5f, 1f),
                VillagePhase.Growing => new float4(0.2f, 0.8f, 0.2f, 1f),
                VillagePhase.Stable => new float4(0.2f, 0.4f, 0.8f, 1f),
                VillagePhase.Expanding => new float4(0.2f, 0.9f, 0.9f, 1f),
                VillagePhase.Crisis => new float4(0.9f, 0.2f, 0.2f, 1f),
                VillagePhase.Declining => new float4(0.6f, 0.3f, 0.1f, 1f),
                _ => new float4(1f, 1f, 1f, 1f)
            };
        }
    }
}
