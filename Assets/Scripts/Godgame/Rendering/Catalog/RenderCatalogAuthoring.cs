using Godgame.Rendering.Catalog;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering
{
    /// <summary>
    /// Authoring component that bakes a RenderCatalogDefinition into ECS singleton components.
    /// Creates RenderCatalogSingleton (with blob) and RenderMeshArray singleton.
    /// </summary>
    public class RenderCatalogAuthoring : MonoBehaviour
    {
        [Tooltip("Catalog definition asset containing mesh/material mappings")]
        public GodgameRenderCatalogDefinition CatalogDefinition;
    }

    public class RenderCatalogBaker : Baker<RenderCatalogAuthoring>
    {
        public override void Bake(RenderCatalogAuthoring authoring)
        {
            if (authoring.CatalogDefinition == null ||
                authoring.CatalogDefinition.Entries == null ||
                authoring.CatalogDefinition.Entries.Length == 0)
            {
                LogError.Message("[Godgame.Rendering] RenderCatalogAuthoring: No catalog definition assigned.");
                return;
            }

            var entity = GetEntity(TransformUsageFlags.None);

            // Build blob for RenderCatalogBlob
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<RenderCatalogBlob>();
            var array = builder.Allocate(ref root.Entries, authoring.CatalogDefinition.Entries.Length);

            for (int i = 0; i < authoring.CatalogDefinition.Entries.Length; i++)
            {
                var src = authoring.CatalogDefinition.Entries[i];

                if (src.Mesh == null || src.Material == null)
                {
                    LogWarning.Message($"[Godgame.Rendering] RenderCatalogAuthoring: Entry {i} (Key={src.Key}) missing mesh or material; skipping.");
                    continue;
                }

                array[i] = new RenderCatalogEntry
                {
                    ArchetypeId = src.Key,
                    MaterialMeshIndex = i, // Index into the RenderMeshArray we'll create below
                    BoundsExtents = src.BoundsExtents
                };
            }

            var blobRef = builder.CreateBlobAssetReference<RenderCatalogBlob>(Allocator.Persistent);

            // Collect unique meshes and materials for RenderMeshArray
            var meshes = new Mesh[authoring.CatalogDefinition.Entries.Length];
            var materials = new Material[authoring.CatalogDefinition.Entries.Length];
            for (int i = 0; i < authoring.CatalogDefinition.Entries.Length; i++)
            {
                var entry = authoring.CatalogDefinition.Entries[i];
                meshes[i] = entry.Mesh;
                materials[i] = entry.Material;
            }

            var renderMeshArray = new RenderMeshArray(materials, meshes);

            // Create a separate entity for the RenderMeshArray (shared component)
            var renderMeshArrayEntity = CreateAdditionalEntity(TransformUsageFlags.None);
            AddSharedComponentManaged(renderMeshArrayEntity, renderMeshArray);

            // Add catalog singleton to main entity
            AddComponent(entity, new RenderCatalogSingleton
            {
                Blob = blobRef,
                RenderMeshArrayEntity = renderMeshArrayEntity
            });

            // Note: RenderMeshArray is stored on renderMeshArrayEntity as a shared component.
            // ApplyRenderCatalogSystem retrieves it via RenderMeshArrayEntity reference.
        }
    }
}
