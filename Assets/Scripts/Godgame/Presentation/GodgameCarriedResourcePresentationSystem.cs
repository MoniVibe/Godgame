using Godgame.Rendering;
using Godgame.Registry;
using Godgame.Villagers;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using StructuralChangePresentationSystemGroup = PureDOTS.Systems.StructuralChangePresentationSystemGroup;
using UpdatePresentationSystemGroup = PureDOTS.Systems.UpdatePresentationSystemGroup;

namespace Godgame.Presentation
{
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
    public partial struct Godgame_CarriedResourcePresentationEnsureSystem : ISystem
    {
        private EntityQuery _missingCarryQuery;
        private ComponentLookup<PresentationLayer> _layerLookup;

        public void OnCreate(ref SystemState state)
        {
            _missingCarryQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerPresentationTag, VillagerJobState, LocalTransform>()
                .WithNone<CarriedResourceLink>()
                .Build();
            _layerLookup = state.GetComponentLookup<PresentationLayer>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled || _missingCarryQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            _layerLookup.Update(ref state);
            var config = EnsureConfig(ref state);

            var endEcb = state.World.GetOrCreateSystemManaged<EndPresentationECBSystem>();
            var ecb = endEcb.CreateCommandBuffer();

            foreach (var (_, _, entity) in SystemAPI
                         .Query<RefRO<VillagerJobState>, RefRO<LocalTransform>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<CarriedResourceLink>()
                         .WithEntityAccess())
            {
                var carriedEntity = ecb.CreateEntity();
                ecb.AddComponent(carriedEntity, new CarriedResourcePresentationTag());
                ecb.AddComponent(carriedEntity, new CarriedResourceParent { Value = entity });
                ecb.AddComponent(carriedEntity, new Parent { Value = entity });
                ecb.AddComponent(carriedEntity, new LocalTransform
                {
                    Position = config.LocalOffset,
                    Rotation = quaternion.identity,
                    Scale = math.max(0.001f, config.BaseScale)
                });
                ecb.AddComponent(carriedEntity, new LocalToWorld { Value = float4x4.identity });
                ecb.AddComponent(carriedEntity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    ShouldRender = 1,
                    DistanceToCamera = 0f
                });

                var layer = _layerLookup.HasComponent(entity)
                    ? _layerLookup[entity].Value
                    : PresentationLayerId.Colony;
                ecb.AddComponent(carriedEntity, new PresentationLayer { Value = layer });

                GodgamePresentationUtility.AssignRenderComponents(ref ecb, carriedEntity, GodgameSemanticKeys.ResourceChunk, default);
                ecb.SetComponentEnabled<MeshPresenter>(carriedEntity, false);

                var bounds = math.max(0.01f, config.BoundsExtents);
                ecb.AddComponent(carriedEntity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero,
                        Extents = new float3(bounds)
                    }
                });

                ecb.AddComponent(entity, new CarriedResourceLink { VisualEntity = carriedEntity });
            }
        }

        private GodgameCarriedResourcePresentationConfig EnsureConfig(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<GodgameCarriedResourcePresentationConfig>(out var config))
            {
                return config;
            }

            var entity = state.EntityManager.CreateEntity();
            var defaults = GodgameCarriedResourcePresentationConfig.Default;
            state.EntityManager.AddComponentData(entity, defaults);
            state.EntityManager.SetName(entity, "GodgameCarriedResourcePresentationConfig");
            return defaults;
        }
    }

    [UpdateInGroup(typeof(UpdatePresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    [UpdateBefore(typeof(Godgame_RenderTintSyncSystem))]
    public partial struct Godgame_CarriedResourcePresentationDriveSystem : ISystem
    {
        private ComponentLookup<VillagerJobState> _jobLookup;
        private ComponentLookup<VillagerCarrying> _carryingLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            _jobLookup = state.GetComponentLookup<VillagerJobState>(true);
            _carryingLookup = state.GetComponentLookup<VillagerCarrying>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            _jobLookup.Update(ref state);
            _carryingLookup.Update(ref state);
            _transformLookup.Update(ref state);
            var config = SystemAPI.TryGetSingleton<GodgameCarriedResourcePresentationConfig>(out var configValue)
                ? configValue
                : GodgameCarriedResourcePresentationConfig.Default;
            var deltaTime = (float)SystemAPI.Time.DeltaTime;
            var smoothing = math.max(0f, config.Smoothing);
            var lerpFactor = smoothing <= 0f ? 1f : 1f - math.exp(-smoothing * deltaTime);

            foreach (var (parentLink, carryTransform, carryTint, meshEnabled) in SystemAPI
                         .Query<RefRO<CarriedResourceParent>, RefRW<LocalTransform>, RefRW<RenderTint>, EnabledRefRW<MeshPresenter>>()
                         .WithAll<CarriedResourcePresentationTag>())
            {
                var parent = parentLink.ValueRO.Value;
                if (parent == Entity.Null ||
                    !_transformLookup.HasComponent(parent) ||
                    !_jobLookup.HasComponent(parent))
                {
                    meshEnabled.ValueRW = false;
                    continue;
                }

                float carryAmount;
                float carryMax;
                ushort resourceTypeIndex;
                ResolveCarryState(parent, out carryAmount, out carryMax, out resourceTypeIndex);

                meshEnabled.ValueRW = carryAmount > 0.01f;
                carryTint.ValueRW.Value = GodgamePresentationColors.ForResourceTypeIndex(resourceTypeIndex);

                float ratio = carryMax > 0f ? math.saturate(carryAmount / carryMax) : 0f;
                float targetScale = math.lerp(config.BaseScale, config.MaxScale, ratio);
                float currentScale = carryTransform.ValueRO.Scale;
                float resolvedScale = math.max(0.001f, math.lerp(currentScale, targetScale, lerpFactor));

                carryTransform.ValueRW.Position = config.LocalOffset;
                carryTransform.ValueRW.Rotation = quaternion.identity;
                carryTransform.ValueRW.Scale = resolvedScale;
            }
        }

        private void ResolveCarryState(Entity parent, out float carryAmount, out float carryMax, out ushort resourceTypeIndex)
        {
            var jobState = _jobLookup[parent];
            carryAmount = jobState.CarryCount;
            carryMax = jobState.CarryMax;
            resourceTypeIndex = jobState.OutputResourceTypeIndex != 0
                ? jobState.OutputResourceTypeIndex
                : jobState.ResourceTypeIndex;

            if (_carryingLookup.HasComponent(parent))
            {
                var carrying = _carryingLookup[parent];
                carryAmount = carrying.Amount;
                carryMax = carrying.MaxCarryCapacity;
                resourceTypeIndex = carrying.ResourceTypeIndex;
            }
        }
    }
}
