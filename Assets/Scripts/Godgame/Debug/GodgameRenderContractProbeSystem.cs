#if UNITY_EDITOR || DEVELOPMENT_BUILD
using PureDOTS.Runtime.Core;
using PureDOTS.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Godgame.DebugSystems
{
    /// <summary>
    /// Logs render contract counts once so we can tell whether semantics/variants/presenters are missing.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Rendering.PresentationContractValidationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct GodgameRenderContractProbeSystem : ISystem
    {
        private EntityQuery _semanticQuery;
        private EntityQuery _variantQuery;
        private EntityQuery _meshPresenterQuery;
        private EntityQuery _materialMeshQuery;
        private EntityQuery _missingPresenterQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            _semanticQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderSemanticKey>());
            _variantQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderVariantKey>());
            _meshPresenterQuery = state.GetEntityQuery(ComponentType.ReadOnly<MeshPresenter>());
            _materialMeshQuery = state.GetEntityQuery(ComponentType.ReadOnly<MaterialMeshInfo>());
            _missingPresenterQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<MaterialMeshInfo>(),
                    ComponentType.ReadOnly<RenderVariantKey>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<MeshPresenter>(),
                    ComponentType.ReadOnly<SpritePresenter>(),
                    ComponentType.ReadOnly<DebugPresenter>(),
                    ComponentType.ReadOnly<TracerPresenter>()
                },
                Options = EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab
            });
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            _missingPresenterQuery.ResetFilter();
            using var missingEntities = _missingPresenterQuery.ToEntityArray(Allocator.Temp);
            if (missingEntities.Length == 0)
            {
                return;
            }

            using var semanticEntities = _semanticQuery.ToEntityArray(Allocator.Temp);
            using var variantEntities = _variantQuery.ToEntityArray(Allocator.Temp);
            using var meshPresenterEntities = _meshPresenterQuery.ToEntityArray(Allocator.Temp);
            using var materialMeshEntities = _materialMeshQuery.ToEntityArray(Allocator.Temp);

            Debug.Log("[GodgameRenderProbe] RenderSemanticKey=" + semanticEntities.Length +
                      " RenderVariantKey=" + variantEntities.Length +
                      " MeshPresenter=" + meshPresenterEntities.Length +
                      " MaterialMeshInfo=" + materialMeshEntities.Length +
                      " MissingPresenter=" + missingEntities.Length);

            var entity = missingEntities[0];
            var entityManager = state.EntityManager;
            var name = entityManager.GetName(entity);
            var isPrefab = entityManager.HasComponent<Prefab>(entity);
            var isDisabled = entityManager.HasComponent<Disabled>(entity);
            var hasSemantic = entityManager.HasComponent<RenderSemanticKey>(entity);
            var hasVariant = entityManager.HasComponent<RenderVariantKey>(entity);
            var variantValue = hasVariant ? entityManager.GetComponentData<RenderVariantKey>(entity).Value : -1;
            var semanticValue = hasSemantic ? entityManager.GetComponentData<RenderSemanticKey>(entity).Value : (ushort)0;
            var hasMeshPresenter = entityManager.HasComponent<MeshPresenter>(entity);
            var hasSpritePresenter = entityManager.HasComponent<SpritePresenter>(entity);
            var hasDebugPresenter = entityManager.HasComponent<DebugPresenter>(entity);
            var hasTracerPresenter = entityManager.HasComponent<TracerPresenter>(entity);

            Debug.LogError("[GodgameRenderProbe] Missing presenter entity=" + entity +
                           " name='" + name + "'" +
                           " prefab=" + isPrefab +
                           " disabled=" + isDisabled +
                           " hasSemantic=" + hasSemantic +
                           " semantic=" + semanticValue +
                           " hasVariant=" + hasVariant +
                           " variant=" + variantValue +
                           " presenters(mesh=" + hasMeshPresenter +
                           " sprite=" + hasSpritePresenter +
                           " debug=" + hasDebugPresenter +
                           " tracer=" + hasTracerPresenter + ")");

            state.Enabled = false;
        }
    }
}
#endif
