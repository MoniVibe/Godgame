using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Authoring component for village center presentation entities.
    /// Attach to a prefab with a mesh renderer to create a village center visual.
    /// </summary>
    public class VillageCenterPresentationAuthoring : MonoBehaviour
    {
        [Header("Visual Settings")]
        [Tooltip("Base color tint for the village center")]
        public Color BaseTint = Color.white;

        [Tooltip("Default influence radius for visualization")]
        public float DefaultInfluenceRadius = 50f;

        [Header("Phase Colors")]
        [Tooltip("Color for forming phase")]
        public Color FormingColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray

        [Tooltip("Color for growing phase")]
        public Color GrowingColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green

        [Tooltip("Color for stable phase")]
        public Color StableColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Blue

        [Tooltip("Color for expanding phase")]
        public Color ExpandingColor = new Color(0.2f, 0.9f, 0.9f, 1f); // Cyan

        [Tooltip("Color for crisis phase")]
        public Color CrisisColor = new Color(0.9f, 0.2f, 0.2f, 1f); // Red

        [Tooltip("Color for declining phase")]
        public Color DecliningColor = new Color(0.6f, 0.3f, 0.1f, 1f); // Brown

        class Baker : Baker<VillageCenterPresentationAuthoring>
        {
            public override void Bake(VillageCenterPresentationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                // Add presentation tag
                AddComponent<VillageCenterPresentationTag>(entity);

                // Add LOD state (village centers always render)
                AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });

                // Add visual state
                AddComponent(entity, new VillageCenterVisualState
                {
                    PhaseTint = new float4(
                        authoring.BaseTint.r,
                        authoring.BaseTint.g,
                        authoring.BaseTint.b,
                        authoring.BaseTint.a),
                    InfluenceRadius = authoring.DefaultInfluenceRadius,
                    Intensity = 1f
                });
            }
        }
    }
}

