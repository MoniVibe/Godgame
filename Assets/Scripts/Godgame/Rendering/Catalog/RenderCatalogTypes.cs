using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Godgame.Rendering
{
    /// <summary>
    /// Blob describing how a RenderKey maps to Entities Graphics assets.
    /// </summary>
    public struct RenderCatalogBlob
    {
        public BlobArray<RenderCatalogEntry> Entries;
    }

    public struct RenderCatalogEntry
    {
        public ushort ArchetypeId;
        public int MaterialMeshIndex; // index into the RenderMeshArray built for this catalog
        public float3 BoundsExtents;
    }

    public struct RenderCatalogSingleton : IComponentData
    {
        public BlobAssetReference<RenderCatalogBlob> Blob;
        public Entity RenderMeshArrayEntity;
    }
}
