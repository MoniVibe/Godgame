using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Authoring-time data describing how to place the visual relative to the entity.
    /// </summary>
    public struct StaticVisualPrefab : IComponentData
    {
        public float3 LocalOffset;
        public quaternion LocalRotationOffset;
        public float ScaleMultiplier;
        public byte InheritParentScale;

        public bool ShouldInheritScale => InheritParentScale != 0;
    }

    /// <summary>
    /// UnityObject reference to the source prefab we should instantiate for this entity.
    /// Stored as a UnityObjectRef so baking can rebind across scene reloads.
    /// </summary>
    public struct StaticVisualPrefabReference : IComponentData
    {
        public UnityObjectRef<GameObject> Prefab;
    }
}
