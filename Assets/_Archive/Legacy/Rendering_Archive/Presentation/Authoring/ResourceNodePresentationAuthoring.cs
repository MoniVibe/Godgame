using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Authoring component for resource node presentation entities.
    /// Attach to a prefab with a mesh renderer to create a resource node visual.
    /// </summary>
    public class ResourceNodePresentationAuthoring : MonoBehaviour
    {
        [Header("Visual Settings")]
        [Tooltip("Base color tint for the resource node")]
        public Color BaseTint = Color.white;

        [Tooltip("Scale multiplier for the mesh")]
        public float ScaleMultiplier = 1f;

        [Header("Resource Type")]
        [Tooltip("Type of resource this node provides")]
        public ResourceNodeType NodeType = ResourceNodeType.Generic;

        class Baker : Baker<ResourceNodePresentationAuthoring>
        {
            public override void Bake(ResourceNodePresentationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                // Add presentation tag
                AddComponent<ResourceNodePresentationTag>(entity);

                // Add LOD state
                AddComponent(entity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
            }
        }
    }

    /// <summary>
    /// Type of resource node for visual differentiation.
    /// </summary>
    public enum ResourceNodeType : byte
    {
        Generic = 0,
        Tree = 1,
        OreDeposit = 2,
        FoodSource = 3,
        StoneQuarry = 4,
        HerbPatch = 5
    }
}

