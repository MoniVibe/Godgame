using System;
using UnityEngine;

namespace Godgame.Rendering.Catalog
{
    [CreateAssetMenu(
        fileName = "GodgameRenderCatalog",
        menuName = "Godgame/Rendering/RenderCatalog")]
    public class GodgameRenderCatalogDefinition : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ushort Key;           // must match your GodgameRenderKeys
            public Mesh Mesh;
            public Material Material;

            public Vector3 BoundsCenter;
            public Vector3 BoundsExtents;
        }

        public Entry[] Entries;
    }
}
