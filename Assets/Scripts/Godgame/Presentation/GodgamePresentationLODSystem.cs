using PureDOTS.Rendering;
using PureDOTS.Runtime.Camera;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial struct Godgame_PresentationLayerBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationLODState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<VillagerPresentationTag>>()
                         .WithNone<PresentationLayer>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Colony });
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<VillageCenterPresentationTag>>()
                         .WithNone<PresentationLayer>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Colony });
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<ResourceChunkPresentationTag>>()
                         .WithNone<PresentationLayer>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Colony });
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<ResourceNodePresentationTag>>()
                         .WithNone<PresentationLayer>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Island });
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<VegetationPresentationTag>>()
                         .WithNone<PresentationLayer>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Continent });
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<PresentationLODState>>()
                         .WithNone<PresentationLayer>()
                         .WithNone<VillagerPresentationTag>()
                         .WithNone<VillageCenterPresentationTag>()
                         .WithNone<ResourceChunkPresentationTag>()
                         .WithNone<ResourceNodePresentationTag>()
                         .WithNone<VegetationPresentationTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PresentationLayer { Value = PresentationLayerId.Planet });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_PresentationLayerBootstrapSystem))]
    public partial struct Godgame_PresentationLODSystem : ISystem
    {
        private ComponentLookup<PresentationLayer> _layerLookup;
        private ComponentLookup<RenderKey> _renderKeyLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationLODState>();
            state.RequireForUpdate<CameraState>();
            _layerLookup = state.GetComponentLookup<PresentationLayer>(true);
            _renderKeyLookup = state.GetComponentLookup<RenderKey>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<CameraState>(out var cameraState))
            {
                return;
            }

            var baseConfig = PresentationConfig.Default;
            bool hasConfig = false;

            foreach (var config in SystemAPI.Query<RefRO<PresentationConfig>>()
                         .WithNone<PresentationConfigRuntimeTag>())
            {
                baseConfig = config.ValueRO;
                hasConfig = true;
                break;
            }

            if (!hasConfig)
            {
                foreach (var config in SystemAPI.Query<RefRO<PresentationConfig>>())
                {
                    baseConfig = config.ValueRO;
                    break;
                }
            }

            var layerConfig = PresentationLayerConfig.Default;
            foreach (var layerOverride in SystemAPI.Query<RefRO<PresentationLayerConfig>>())
            {
                layerConfig = layerOverride.ValueRO;
                break;
            }

            _layerLookup.Update(ref state);
            _renderKeyLookup.Update(ref state);

            foreach (var (lodState, transform, entity) in SystemAPI
                         .Query<RefRW<PresentationLODState>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                var layer = _layerLookup.HasComponent(entity)
                    ? _layerLookup[entity].Value
                    : PresentationLayerId.Planet;

                float multiplier = ResolveLayerMultiplier(layer, layerConfig);
                float lod0 = math.max(1f, baseConfig.LOD0Distance * multiplier);
                float lod1 = math.max(lod0, baseConfig.LOD1Distance * multiplier);
                float lod2 = math.max(lod1, baseConfig.LOD2Distance * multiplier);

                float distance = math.distance(transform.ValueRO.Position, cameraState.TargetPosition);
                var lod = distance < lod0
                    ? PresentationLOD.LOD0_Full
                    : distance < lod1
                        ? PresentationLOD.LOD1_Mid
                        : distance < lod2
                            ? PresentationLOD.LOD2_Far
                            : PresentationLOD.LOD3_Culled;

                lodState.ValueRW.CurrentLOD = lod;
                lodState.ValueRW.DistanceToCamera = distance;
                lodState.ValueRW.ShouldRender = lod == PresentationLOD.LOD3_Culled ? (byte)0 : (byte)1;

                if (_renderKeyLookup.HasComponent(entity))
                {
                    var key = _renderKeyLookup[entity];
                    var lodIndex = (byte)math.min((int)lod, 2);
                    if (key.LOD != lodIndex)
                    {
                        key.LOD = lodIndex;
                        _renderKeyLookup[entity] = key;
                    }
                }
            }
        }

        private static float ResolveLayerMultiplier(PresentationLayerId layer, in PresentationLayerConfig config)
        {
            return layer switch
            {
                PresentationLayerId.Colony => config.ColonyMultiplier,
                PresentationLayerId.Island => config.IslandMultiplier,
                PresentationLayerId.Continent => config.ContinentMultiplier,
                PresentationLayerId.Planet => config.PlanetMultiplier,
                PresentationLayerId.Orbital => config.OrbitalMultiplier,
                PresentationLayerId.Galactic => config.GalacticMultiplier,
                _ => config.SystemMultiplier
            };
        }
    }
}
