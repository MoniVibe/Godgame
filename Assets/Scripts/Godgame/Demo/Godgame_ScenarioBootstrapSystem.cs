using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Rendering;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime.Registry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Demo
{
    /// <summary>
    /// Bootstrap system that handles scenario mode selection.
    /// If Mode == Demo01, delegates to Godgame_Demo01_BootstrapSystem.
    /// If Mode == Scenario_10k/100k/1M, adds presentation components to PureDOTS scenario entities.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(Godgame_Demo01_BootstrapSystem))]
    public partial struct Godgame_ScenarioBootstrapSystem : ISystem
    {
        private bool _initialized;
        private EntityQuery _demoConfigQuery;
        private EntityQuery _presentationConfigQuery;

        public void OnCreate(ref SystemState state)
        {
            _initialized = false;
            _demoConfigQuery = state.GetEntityQuery(ComponentType.ReadOnly<DemoConfigBlobReference>());
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

            // Get demo config to determine mode
            DemoScenarioMode mode = GetScenarioMode(ref state);

            if (mode == DemoScenarioMode.Demo01)
            {
                // Demo01 mode: Let Godgame_Demo01_BootstrapSystem handle it
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

        private DemoScenarioMode GetScenarioMode(ref SystemState state)
        {
            if (!_demoConfigQuery.IsEmptyIgnoreFilter)
            {
                var blobRef = _demoConfigQuery.GetSingleton<DemoConfigBlobReference>();
                if (blobRef.Config.IsCreated)
                {
                    return blobRef.Config.Value.Mode;
                }
            }

            return DemoScenarioMode.Demo01; // Default
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

        private void AddPresentationToScenarioEntities(ref SystemState state, DemoScenarioMode mode)
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

        private void AdjustPresentationConfigForScenario(ref SystemState state, DemoScenarioMode mode)
        {
            if (_presentationConfigQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            ref var configRef = ref _presentationConfigQuery.GetSingletonRW<PresentationConfig>().ValueRW;

            // Adjust LOD distances and density based on scenario scale
            switch (mode)
            {
                case DemoScenarioMode.Scenario_10k:
                    // 10k: Moderate LOD distances, some density sampling
                    configRef.LOD0Distance = 30f;
                    configRef.LOD1Distance = 150f;
                    configRef.LOD2Distance = 400f;
                    configRef.DensitySlider = 0.5f; // Render 50%
                    configRef.MaxLOD0Villagers = 500;
                    break;

                case DemoScenarioMode.Scenario_100k:
                    // 100k: Aggressive LOD, heavy density sampling
                    configRef.LOD0Distance = 20f;
                    configRef.LOD1Distance = 100f;
                    configRef.LOD2Distance = 300f;
                    configRef.DensitySlider = 0.1f; // Render 10%
                    configRef.MaxLOD0Villagers = 200;
                    break;

                case DemoScenarioMode.Scenario_1M:
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
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderKey
                {
                    ArchetypeId = GodgameRenderKeys.Villager,
                    LOD = 0
                });
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderFlags
                {
                    Visible = 1,
                    ShadowCaster = 1,
                    HighlightMask = 0
                });
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
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderKey
                {
                    ArchetypeId = GodgameRenderKeys.VillageCenter,
                    LOD = 0
                });
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderFlags
                {
                    Visible = 1,
                    ShadowCaster = 1,
                    HighlightMask = 0
                });

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
                    ResourceTypeTint = GetResourceTypeColor(resource.ValueRO.Type),
                    QuantityScale = 0.5f + resource.ValueRO.Quantity * 0.01f,
                    IsCarried = 0
                });
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderKey
                {
                    ArchetypeId = GodgameRenderKeys.ResourceChunk,
                    LOD = 0
                });
                ecb.AddComponent(entity, new PureDOTS.Rendering.RenderFlags
                {
                    Visible = 1,
                    ShadowCaster = 1,
                    HighlightMask = 0
                });
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

        private static Unity.Mathematics.float4 GetResourceTypeColor(ResourceType type)
        {
            byte typeValue = (byte)type;

            if (typeValue >= 1 && typeValue <= 7)
                return new Unity.Mathematics.float4(0.5f, 0.5f, 0.5f, 1f); // Ores: Gray
            if (typeValue >= 10 && typeValue <= 14)
                return new Unity.Mathematics.float4(0.55f, 0.27f, 0.07f, 1f); // Wood: Brown
            if (typeValue >= 20 && typeValue <= 23)
                return new Unity.Mathematics.float4(0.4f, 0.4f, 0.4f, 1f); // Stone: Dark gray
            if (typeValue >= 30 && typeValue <= 33)
                return new Unity.Mathematics.float4(0.2f, 0.6f, 0.2f, 1f); // Herbs: Green
            if (typeValue >= 40 && typeValue <= 44)
                return new Unity.Mathematics.float4(0.9f, 0.8f, 0.2f, 1f); // Agricultural: Yellow

            return new Unity.Mathematics.float4(1f, 1f, 1f, 1f);
        }
    }
}
