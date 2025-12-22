using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Rendering;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime.Registry;
using PureDOTS.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Godgame.Scenario;

namespace Godgame.Scenario
{
    /// <summary>
    /// Bootstrap system that handles scenario mode selection.
    /// If Mode == Scenario01, delegates to GodgameScenario01BootstrapSystem.
    /// If Mode == Scenario_10k/100k/1M, adds presentation components to scenario entities.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(GodgameScenario01BootstrapSystem))]
    public partial struct GodgameScenarioBootstrapSystem : ISystem
    {
        private bool _initialized;
        private EntityQuery _scenarioConfigQuery;
        private EntityQuery _presentationConfigQuery;

        public void OnCreate(ref SystemState state)
        {
            _initialized = false;
            state.RequireForUpdate<ScenarioSceneTag>();
            _scenarioConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<GodgameScenarioConfigBlobReference>());
            _presentationConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<PresentationConfig>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_initialized)
            {
                state.Enabled = false;
                return;
            }

            _initialized = true;

            // Get scenario config to determine mode
            GodgameScenarioMode mode = GetScenarioMode(ref state);

            if (mode == GodgameScenarioMode.Scenario01)
            {
                // Scenario01 mode: Let GodgameScenario01BootstrapSystem handle it
                // This system just ensures presentation config exists
                EnsurePresentationConfig(ref state);
                state.Enabled = false;
                return;
            }

            // Scenario mode: Add presentation components to PureDOTS scenario entities
            EnsurePresentationConfig(ref state);
            AddPresentationToScenarioEntities(ref state, mode);

            state.Enabled = false;
        }

        private GodgameScenarioMode GetScenarioMode(ref SystemState state)
        {
            if (!_scenarioConfigQuery.IsEmptyIgnoreFilter)
            {
                var blobRef = _scenarioConfigQuery.GetSingleton<GodgameScenarioConfigBlobReference>();
                if (blobRef.Config.IsCreated)
                {
                    return blobRef.Config.Value.Mode;
                }
            }

            return GodgameScenarioMode.Scenario01; // Default
        }

        private void EnsurePresentationConfig(ref SystemState state)
        {
            using var configs = _presentationConfigQuery.ToEntityArray(Allocator.Temp);
            if (configs.Length == 0)
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, PresentationConfig.Default);
                state.EntityManager.AddComponent<PresentationConfigRuntimeTag>(configEntity);
                state.EntityManager.SetName(configEntity, "PresentationConfig");
                return;
            }

            if (configs.Length == 1)
            {
                return;
            }

            var entityManager = state.EntityManager;
            Entity keep = Entity.Null;

            foreach (var entity in configs)
            {
                if (!entityManager.HasComponent<PresentationConfigRuntimeTag>(entity))
                {
                    keep = entity;
                    break;
                }
            }

            if (keep == Entity.Null)
            {
                keep = configs[0];
            }

            foreach (var entity in configs)
            {
                if (entity == keep)
                {
                    continue;
                }

                if (entityManager.HasComponent<PresentationConfigRuntimeTag>(entity))
                {
                    entityManager.DestroyEntity(entity);
                }
                else
                {
                    entityManager.RemoveComponent<PresentationConfig>(entity);
                }
            }
        }

        private void AddPresentationToScenarioEntities(ref SystemState state, GodgameScenarioMode mode)
        {
            // Adjust presentation config based on scenario scale
            AdjustPresentationConfigForScenario(ref state, mode);

            // Add presentation components to villagers from VillagerRegistry
            AddPresentationToVillagers(ref state);

            // Add presentation components to villages
            AddPresentationToVillages(ref state);

            // Add presentation components to resource chunks
            AddPresentationToResourceChunks(ref state);
        }

        private void AdjustPresentationConfigForScenario(ref SystemState state, GodgameScenarioMode mode)
        {
            bool updated = false;

            foreach (var configRef in SystemAPI.Query<RefRW<PresentationConfig>>()
                         .WithNone<PresentationConfigRuntimeTag>())
            {
                ApplyScenarioConfig(ref configRef.ValueRW, mode);
                updated = true;
                break;
            }

            if (!updated)
            {
                foreach (var configRef in SystemAPI.Query<RefRW<PresentationConfig>>())
                {
                    ApplyScenarioConfig(ref configRef.ValueRW, mode);
                    break;
                }
            }
        }

        private static void ApplyScenarioConfig(ref PresentationConfig configRef, GodgameScenarioMode mode)
        {
            // Adjust LOD distances and density based on scenario scale
            switch (mode)
            {
                case GodgameScenarioMode.Scenario_10k:
                    // 10k: Moderate LOD distances, some density sampling
                    configRef.LOD0Distance = 30f;
                    configRef.LOD1Distance = 150f;
                    configRef.LOD2Distance = 400f;
                    configRef.DensitySlider = 0.5f; // Render 50%
                    configRef.MaxLOD0Villagers = 500;
                    break;

                case GodgameScenarioMode.Scenario_100k:
                    // 100k: Aggressive LOD, heavy density sampling
                    configRef.LOD0Distance = 20f;
                    configRef.LOD1Distance = 100f;
                    configRef.LOD2Distance = 300f;
                    configRef.DensitySlider = 0.1f; // Render 10%
                    configRef.MaxLOD0Villagers = 200;
                    break;

                case GodgameScenarioMode.Scenario_1M:
                    // 1M: Maximum LOD, minimal density
                    configRef.LOD0Distance = 10f;
                    configRef.LOD1Distance = 50f;
                    configRef.LOD2Distance = 200f;
                    configRef.DensitySlider = 0.01f; // Render 1%
                    configRef.MaxLOD0Villagers = 100;
                    break;
            }
        }

        private void AddPresentationToVillagers(ref SystemState state)
        {
            // Query for entities that should be villagers but don't have presentation tags yet
            // This assumes PureDOTS scenario entities have some identifying component
            // For now, we'll add presentation to entities with VillagerBehavior but no VillagerPresentationTag

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (behavior, entity) in SystemAPI.Query<RefRO<VillagerBehavior>>().WithNone<VillagerPresentationTag>().WithEntityAccess())
            {
                if (!state.EntityManager.HasComponent<LocalTransform>(entity))
                {
                    continue; // Skip entities without transforms
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
                    AlignmentTint = new Unity.Mathematics.float4(0.7f, 0.7f, 0.7f, 1f),
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

            // Add presentation to village entities (entities with Village component but no presentation tag)
            foreach (var (village, entity) in SystemAPI.Query<RefRO<Village>>().WithNone<VillageCenterPresentationTag>().WithEntityAccess())
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

                // Ensure VillageMember buffer exists
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

            // Add presentation to resource chunks (entities with ExtractedResource but no presentation tag)
            foreach (var (resource, entity) in SystemAPI.Query<RefRO<ExtractedResource>>().WithNone<ResourceChunkPresentationTag>().WithEntityAccess())
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

        private static Unity.Mathematics.float4 GetPhaseColor(VillagePhase phase)
        {
            return phase switch
            {
                VillagePhase.Forming => new Unity.Mathematics.float4(0.5f, 0.5f, 0.5f, 1f),
                VillagePhase.Growing => new Unity.Mathematics.float4(0.2f, 0.8f, 0.2f, 1f),
                VillagePhase.Stable => new Unity.Mathematics.float4(0.2f, 0.4f, 0.8f, 1f),
                VillagePhase.Expanding => new Unity.Mathematics.float4(0.2f, 0.9f, 0.9f, 1f),
                VillagePhase.Crisis => new Unity.Mathematics.float4(0.9f, 0.2f, 0.2f, 1f),
                VillagePhase.Declining => new Unity.Mathematics.float4(0.6f, 0.3f, 0.1f, 1f),
                _ => new Unity.Mathematics.float4(1f, 1f, 1f, 1f)
            };
        }

    }
}
