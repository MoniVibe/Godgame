using Godgame.Rendering.Catalog;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Godgame.Rendering
{
    /// <summary>
    /// Ensures a RenderCatalogSingleton + RenderMeshArray exist at runtime even if the
    /// subscene bake is unavailable (e.g., headless smoke tests that skip conversion).
    /// Creates the same data the RenderCatalogAuthoring baker would have produced.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RenderCatalogRuntimeBootstrap : MonoBehaviour
    {
        [Tooltip("Catalog definition asset containing mesh/material mappings.")]
        public GodgameRenderCatalogDefinition CatalogDefinition;

        private EntityManager _entityManager;
        private World _world;
        private Entity _catalogEntity = Entity.Null;
        private Entity _renderMeshArrayEntity = Entity.Null;
        private BlobAssetReference<RenderCatalogBlob> _catalogBlob;
        private bool _catalogCreated;

        private void Awake()
        {
            if (RuntimeMode.IsHeadless)
                return;
            if (CatalogDefinition == null || CatalogDefinition.Entries == null || CatalogDefinition.Entries.Length == 0)
            {
                UnityEngine.Debug.LogError("[RenderCatalogRuntimeBootstrap] CatalogDefinition is missing or empty; cannot seed runtime catalog.");
                return;
            }

            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null)
            {
                UnityEngine.Debug.LogWarning("[RenderCatalogRuntimeBootstrap] Default world not ready; catalog bootstrap skipped.");
                return;
            }

            _entityManager = _world.EntityManager;
            var existingCatalog = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<RenderCatalogSingleton>());
            if (existingCatalog.CalculateEntityCount() > 0)
            {
                existingCatalog.Dispose();
                return;
            }
            existingCatalog.Dispose();

            CreateRuntimeCatalog();
        }

        private void CreateRuntimeCatalog()
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<RenderCatalogBlob>();
            var blobEntries = builder.Allocate(ref root.Entries, CatalogDefinition.Entries.Length);

            for (int i = 0; i < CatalogDefinition.Entries.Length; i++)
            {
                var src = CatalogDefinition.Entries[i];
                if (src.Mesh == null || src.Material == null)
                {
                    UnityEngine.Debug.LogWarning($"[RenderCatalogRuntimeBootstrap] Entry {i} (Key={src.Key}) missing mesh/material; skipping.");
                    blobEntries[i] = default;
                    continue;
                }

                blobEntries[i] = new RenderCatalogEntry
                {
                    ArchetypeId = src.Key,
                    MaterialMeshIndex = i,
                    BoundsExtents = src.BoundsExtents
                };
            }

            _catalogBlob = builder.CreateBlobAssetReference<RenderCatalogBlob>(Allocator.Persistent);

            var meshes = new Mesh[CatalogDefinition.Entries.Length];
            var materials = new Material[CatalogDefinition.Entries.Length];
            for (int i = 0; i < CatalogDefinition.Entries.Length; i++)
            {
                var entry = CatalogDefinition.Entries[i];
                meshes[i] = entry.Mesh;
                materials[i] = entry.Material;
            }

            var renderMeshArray = new RenderMeshArray(materials, meshes);
            _renderMeshArrayEntity = _entityManager.CreateEntity();
            _entityManager.AddSharedComponentManaged(_renderMeshArrayEntity, renderMeshArray);

            _catalogEntity = _entityManager.CreateEntity(typeof(RenderCatalogSingleton));
            _entityManager.SetComponentData(_catalogEntity, new RenderCatalogSingleton
            {
                Blob = _catalogBlob,
                RenderMeshArrayEntity = _renderMeshArrayEntity
            });

            UnityEngine.Debug.Log($"[RenderCatalogRuntimeBootstrap] Created runtime catalog with {CatalogDefinition.Entries.Length} entries.");
            _catalogCreated = true;
        }

        private void OnDestroy()
        {
            if (_catalogCreated && _entityManager != default && _world != null && _world.IsCreated)
            {
                if (_catalogEntity != Entity.Null && _entityManager.Exists(_catalogEntity))
                {
                    _entityManager.DestroyEntity(_catalogEntity);
                }

                if (_renderMeshArrayEntity != Entity.Null && _entityManager.Exists(_renderMeshArrayEntity))
                {
                    _entityManager.DestroyEntity(_renderMeshArrayEntity);
                }
            }

            if (_catalogBlob.IsCreated)
            {
                _catalogBlob.Dispose();
            }
        }
    }
}
