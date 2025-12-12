#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Authoring component for resource chunk presentation entities.
    /// Attach to a prefab with a mesh renderer to create a resource chunk visual.
    /// </summary>
    public class ResourceChunkPresentationAuthoring : MonoBehaviour
    {
        [Header("Visual Settings")]
        [Tooltip("Base color tint for the chunk (will be overridden by resource type)")]
        public Color BaseTint = Color.white;

        [Tooltip("Base scale for the chunk mesh")]
        public float BaseScale = 1f;

        [Header("Resource Type Colors")]
        [Tooltip("Color for wood resources")]
        public Color WoodColor = new Color(0.55f, 0.27f, 0.07f, 1f); // Brown

        [Tooltip("Color for ore resources")]
        public Color OreColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

        [Tooltip("Color for food resources")]
        public Color FoodColor = new Color(0.9f, 0.8f, 0.2f, 1f); // Yellow

        [Tooltip("Color for stone resources")]
        public Color StoneColor = new Color(0.4f, 0.4f, 0.4f, 1f); // Dark gray

        class Baker : Baker<ResourceChunkPresentationAuthoring>
        {
            public override void Bake(ResourceChunkPresentationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                // Add presentation tag
                AddComponent<ResourceChunkPresentationTag>(entity);

                // Add LOD state
                AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });

                // Add visual state with base tint
                AddComponent(entity, new ResourceChunkVisualState
                {
                    ResourceTypeTint = new float4(
                        authoring.BaseTint.r,
                        authoring.BaseTint.g,
                        authoring.BaseTint.b,
                        authoring.BaseTint.a),
                    QuantityScale = authoring.BaseScale,
                    IsCarried = 0
                });
            }
        }
    }
}
#endif
