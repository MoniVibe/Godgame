#if UNITY_EDITOR || DEVELOPMENT_BUILD
using PureDOTS.Rendering;
using Unity.Entities;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace Godgame.Rendering
{
    /// <summary>
    /// Ensures MeshPresenter indices remain within the active catalog to avoid invalid batch commands.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Rendering.ApplyRenderVariantSystem))]
    public partial struct GodgameRenderContractValidationSystem : ISystem
    {
        private bool _validated;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RenderPresentationCatalog>();
            _validated = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_validated)
            {
                state.Enabled = false;
                return;
            }

            var catalog = SystemAPI.GetSingleton<RenderPresentationCatalog>();
            if (!catalog.Blob.IsCreated)
            {
                return;
            }

            var variantCount = catalog.Blob.Value.Variants.Length;
            if (variantCount == 0)
            {
                state.Enabled = false;
                return;
            }

            int corrected = 0;
            foreach (var (presenter, entity) in SystemAPI.Query<RefRW<MeshPresenter>>().WithEntityAccess())
            {
                if (presenter.ValueRO.DefIndex >= variantCount)
                {
                    presenter.ValueRW.DefIndex = 0;
                    if (SystemAPI.HasComponent<RenderVariantKey>(entity))
                    {
                        SystemAPI.SetComponent(entity, new RenderVariantKey(0));
                    }

                    corrected++;
                }
            }

            if (corrected > 0)
            {
                UnityDebug.LogWarning($"[GodgameRenderContractValidation] Reset {corrected} MeshPresenter entries to fallback variant (catalog variants={variantCount}).");
            }

            _validated = true;
            state.Enabled = false;
        }
    }
}
#endif
