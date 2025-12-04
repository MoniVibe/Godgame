using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Presentation
{
    public struct PresentationBindingReference : IComponentData
    {
        public FixedString64Bytes BindingId;
    }

    /// <summary>
    /// Ensures a presentation binding reference exists and points at the Minimal bindings asset.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PresentationBindingBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var query = state.GetEntityQuery(ComponentType.ReadOnly<PresentationBindingReference>());
            if (query.IsEmptyIgnoreFilter)
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new PresentationBindingReference
                {
                    BindingId = new FixedString64Bytes("Minimal")
                });
            }

            // We don't RequireForUpdate because we want to run once to validate/load even if the singleton was just created.
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<PresentationBindingReference>(out var referenceEntity))
            {
                return;
            }

            var reference = SystemAPI.GetComponent<PresentationBindingReference>(referenceEntity);

            var binding = ResolveMinimalBinding();
            if (binding == null)
            {
                Debug.LogError("[PresentationBindingBootstrap] Missing Minimal binding set. Expected at Resources/Bindings/Minimal or Assets/Bindings/Minimal.asset.");
                SpawnMissingBindingSentinel();
                state.Enabled = false;
                return;
            }

            reference.BindingId = new FixedString64Bytes(binding.name);
            state.EntityManager.SetComponentData(referenceEntity, reference);
            state.Enabled = false;
        }

        private static PresentationBindingSet ResolveMinimalBinding()
        {
            var binding = Godgame.ResourcesHelper.Load<PresentationBindingSet>("Bindings/Minimal");
#if UNITY_EDITOR
            if (binding == null)
            {
                binding = UnityEditor.AssetDatabase.LoadAssetAtPath<PresentationBindingSet>("Assets/Resources/Bindings/Minimal.asset");
                if (binding == null)
                {
                    binding = UnityEditor.AssetDatabase.LoadAssetAtPath<PresentationBindingSet>("Assets/Bindings/Minimal.asset");
                }
            }
#endif
            return binding;
        }

        private static void SpawnMissingBindingSentinel()
        {
            var sentinel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sentinel.name = "MissingPresentationBinding";
            sentinel.transform.position = Vector3.zero;
            sentinel.transform.localScale = Vector3.one * 2f;
            var renderer = sentinel.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = Color.magenta;
            }
        }

    }
}
