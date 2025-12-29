using Godgame.Rendering;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Ensures a presenter exists for any entity with a render semantic key.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [UpdateBefore(typeof(RenderPresentationValidationSystem))]
#endif
    public partial struct GodgameRenderContractRepairSystem : ISystem
    {
        private EntityQuery _missingPresenterQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingPresenterQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<RenderSemanticKey>() },
                None = new[]
                {
                    ComponentType.ReadOnly<MeshPresenter>(),
                    ComponentType.ReadOnly<SpritePresenter>(),
                    ComponentType.ReadOnly<DebugPresenter>(),
                    ComponentType.ReadOnly<TracerPresenter>()
                }
            });

            state.RequireForUpdate<GameWorldTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless || _missingPresenterQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            using var entities = _missingPresenterQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var semantic = SystemAPI.GetComponent<RenderSemanticKey>(entity);
                var presentationState = new GodgamePresentationUtility.PrefabPresentationState
                {
                    HasRenderKey = SystemAPI.HasComponent<RenderKey>(entity),
                    HasRenderVariantKey = SystemAPI.HasComponent<RenderVariantKey>(entity),
                    HasSemanticKey = true,
                    HasRenderFlags = SystemAPI.HasComponent<RenderFlags>(entity),
                    HasMeshPresenter = false,
                    HasSpritePresenter = false,
                    HasDebugPresenter = false,
                    HasRenderTint = SystemAPI.HasComponent<RenderTint>(entity),
                    HasRenderTexSlice = SystemAPI.HasComponent<RenderTexSlice>(entity),
                    HasRenderUv = SystemAPI.HasComponent<RenderUvTransform>(entity),
                    HasRenderThemeOverride = SystemAPI.HasComponent<RenderThemeOverride>(entity)
                };

                GodgamePresentationUtility.AssignRenderComponents(
                    ref ecb,
                    entity,
                    semantic.Value,
                    presentationState);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
