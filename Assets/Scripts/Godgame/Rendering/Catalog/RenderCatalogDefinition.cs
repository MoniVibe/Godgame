using System;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Rendering.Catalog
{
    /// <summary>
    /// ScriptableObject defining render catalog entries for Godgame entities.
    /// Each entry maps a RenderKey.ArchetypeId to a mesh/material pair.
    /// </summary>
    [CreateAssetMenu(fileName = "GodgameRenderCatalog", menuName = "Godgame/Rendering/RenderCatalog")]
    public class RenderCatalogDefinition : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("RenderKey.ArchetypeId value (e.g., 100 for villager, 110 for village center)")]
            public ushort Key;

            [Tooltip("Mesh to render")]
            public Mesh Mesh;

            [Tooltip("Material to use")]
            public Material Material;

            [Tooltip("Bounds center offset (usually zero)")]
            public float3 BoundsCenter;

            [Tooltip("Bounds extents (half-size of bounding box)")]
            public float3 BoundsExtents;
        }

        [Tooltip("Catalog entries mapping RenderKey.ArchetypeId to mesh/material pairs")]
        public Entry[] Entries;
    }
}













