using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Authoring component for villager presentation entities.
    /// Attach to a prefab with a mesh renderer to create a villager visual.
    /// </summary>
    public class VillagerPresentationAuthoring : MonoBehaviour
    {
        [Header("Visual Settings")]
        [Tooltip("Base color tint for the villager")]
        public Color BaseTint = Color.white;

        [Tooltip("Scale multiplier for the mesh")]
        public float ScaleMultiplier = 1f;

        [Header("LOD Settings")]
        [Tooltip("Override LOD distances for this villager type")]
        public bool OverrideLODDistances;
        public float LOD0Distance = 50f;
        public float LOD1Distance = 200f;

        class Baker : Baker<VillagerPresentationAuthoring>
        {
            public override void Bake(VillagerPresentationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                // Add presentation tag
                AddComponent<VillagerPresentationTag>(entity);

                // Add LOD state (initialized to full detail)
                AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });

                // Add visual state
                AddComponent(entity, new VillagerVisualState
                {
                    AlignmentTint = new float4(
                        authoring.BaseTint.r,
                        authoring.BaseTint.g,
                        authoring.BaseTint.b,
                        authoring.BaseTint.a),
                    TaskIconIndex = 0,
                    AnimationState = 0,
                    EffectIntensity = 0f
                });
            }
        }
    }
}

