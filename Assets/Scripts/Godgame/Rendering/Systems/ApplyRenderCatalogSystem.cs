using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using PureDOTS.Rendering;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using Godgame.Rendering;
using Godgame.Rendering.Catalog;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Attaches Entities Graphics components to any entity tagged with a RenderKey/RenderFlags pair.
    /// Runs in PresentationSystemGroup after catalog is baked.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct ApplyRenderCatalogSystem : ISystem
    {
        private EntityQuery _catalogQuery;
        private EntityQuery _renderKeyQuery;
        private EntityQuery _renderFlagsQuery;
        private bool _warnedMissingCatalog;
        private bool _warnedMissingKeys;
        private bool _warnedMissingFlags;
        private bool _warnedMissingBlob;
        private bool _warnedMissingRenderArrayEntity;
        private bool _warnedMultipleCatalogs;
        private NativeParallelHashSet<ushort> _warnedMissingArchetypes;

        public void OnCreate(ref SystemState state)
        {
            _catalogQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<RenderCatalogSingleton>());
            _renderKeyQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderKey>());
            _renderFlagsQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderFlags>());

            state.RequireForUpdate<RenderCatalogSingleton>();
            state.RequireForUpdate<GameWorldTag>();
            _warnedMissingArchetypes = new NativeParallelHashSet<ushort>(16, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_warnedMissingArchetypes.IsCreated)
            {
                _warnedMissingArchetypes.Dispose();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!Application.isPlaying)
                return;
            if (RuntimeMode.IsHeadless)
                return;

            if (_renderKeyQuery.IsEmptyIgnoreFilter)
            {
                if (!_warnedMissingKeys)
                {
                    UnityEngine.Debug.LogError("[ApplyRenderCatalogSystem] No RenderKey entities found. Rendering will never start; ensure spawners add RenderKey.");
                    _warnedMissingKeys = true;
                }
                return;
            }
            _warnedMissingKeys = false;

            if (_renderFlagsQuery.IsEmptyIgnoreFilter)
            {
                if (!_warnedMissingFlags)
                {
                    UnityEngine.Debug.LogError("[ApplyRenderCatalogSystem] RenderKey entities exist but no RenderFlags present. Add RenderFlags (Visible=1) to render.");
                    _warnedMissingFlags = true;
                }
                return;
            }
            _warnedMissingFlags = false;

            var catalogCount = _catalogQuery.CalculateEntityCount();

            if (catalogCount == 0)
            {
                if (!_warnedMissingCatalog)
                {
                    UnityEngine.Debug.LogError("[ApplyRenderCatalogSystem] Missing RenderCatalogSingleton + RenderMeshArray; cannot assign MaterialMeshInfo.");
                    _warnedMissingCatalog = true;
                }
                return;
            }
            _warnedMissingCatalog = false;

            if (catalogCount != 1)
            {
                if (!_warnedMultipleCatalogs)
                {
                    UnityEngine.Debug.LogError($"[ApplyRenderCatalogSystem] Found {catalogCount} RenderCatalogSingleton entities. Ensure only one RenderCatalogAuthoring exists in loaded SubScenes.");
                    _warnedMultipleCatalogs = true;
                }
                return;
            }
            _warnedMultipleCatalogs = false;

            var catalogSingleton = SystemAPI.GetSingleton<RenderCatalogSingleton>();
            if (!catalogSingleton.Blob.IsCreated)
            {
                if (!_warnedMissingBlob)
                {
                    UnityEngine.Debug.LogError("[ApplyRenderCatalogSystem] RenderCatalog blob is not created; aborting.");
                    _warnedMissingBlob = true;
                }
                return;
            }
            _warnedMissingBlob = false;

            if (!state.EntityManager.Exists(catalogSingleton.RenderMeshArrayEntity))
            {
                if (!_warnedMissingRenderArrayEntity)
                {
                    UnityEngine.Debug.LogError("[ApplyRenderCatalogSystem] RenderMeshArray entity referenced by catalog is missing.");
                    _warnedMissingRenderArrayEntity = true;
                }
                return;
            }
            _warnedMissingRenderArrayEntity = false;

            var renderMeshArray = state.EntityManager.GetSharedComponentManaged<RenderMeshArray>(catalogSingleton.RenderMeshArrayEntity);
            ref var entries = ref catalogSingleton.Blob.Value.Entries;
            if (entries.Length == 0)
                return;

            var meshCount = renderMeshArray.MeshReferences?.Length ?? 0;
            var materialCount = renderMeshArray.MaterialReferences?.Length ?? 0;
            if (meshCount == 0 || materialCount == 0)
            {
                UnityEngine.Debug.LogWarning("[ApplyRenderCatalogSystem] RenderMeshArray is empty; skipping MaterialMeshInfo assignment.");
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (renderKey, renderFlags, transform, entity) in SystemAPI
                         .Query<RefRO<RenderKey>, RefRO<RenderFlags>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                if (renderFlags.ValueRO.Visible == 0)
                    continue;

                var entryIndex = FindEntry(renderKey.ValueRO.ArchetypeId, ref entries);
                if (entryIndex < 0)
                {
                    // Warn once per missing archetype ID to avoid spam.
                    if (_warnedMissingArchetypes.Add(renderKey.ValueRO.ArchetypeId))
                    {
                        LogWarning.Message($"[ApplyRenderCatalogSystem] No catalog entry for ArchetypeId={renderKey.ValueRO.ArchetypeId}; entity {entity} will not be rendered.");
                    }
                    continue;
                }

                var entry = entries[entryIndex];
                if (entry.MaterialMeshIndex < 0 || entry.MaterialMeshIndex >= meshCount || entry.MaterialMeshIndex >= materialCount)
                {
                    if (_warnedMissingArchetypes.Add(renderKey.ValueRO.ArchetypeId))
                    {
                        LogWarning.Message($"[ApplyRenderCatalogSystem] Catalog entry for ArchetypeId={renderKey.ValueRO.ArchetypeId} has invalid MaterialMeshIndex={entry.MaterialMeshIndex}; entity {entity} will not be rendered.");
                    }
                    continue;
                }
                var mesh = renderMeshArray.MeshReferences[entry.MaterialMeshIndex];
                var material = renderMeshArray.MaterialReferences[entry.MaterialMeshIndex];
                if (mesh == null || material == null)
                {
                    if (_warnedMissingArchetypes.Add(renderKey.ValueRO.ArchetypeId))
                    {
                        LogWarning.Message($"[ApplyRenderCatalogSystem] Catalog entry for ArchetypeId={renderKey.ValueRO.ArchetypeId} references null Mesh/Material at index {entry.MaterialMeshIndex}; entity {entity} will not be rendered.");
                    }
                    continue;
                }
                var bounds = new AABB { Center = float3.zero, Extents = entry.BoundsExtents };
                var worldBounds = new AABB { Center = transform.ValueRO.Position, Extents = entry.BoundsExtents };

                ecb.AddSharedComponentManaged(entity, renderMeshArray);
                var meshIndex = (ushort)entry.MaterialMeshIndex;
                var materialIndex = meshIndex;

                var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(materialIndex, meshIndex);
                if (state.EntityManager.HasComponent<MaterialMeshInfo>(entity))
                    ecb.SetComponent(entity, materialMeshInfo);
                else
                    ecb.AddComponent(entity, materialMeshInfo);

                var renderBounds = new RenderBounds { Value = bounds };
                if (state.EntityManager.HasComponent<RenderBounds>(entity))
                    ecb.SetComponent(entity, renderBounds);
                else
                    ecb.AddComponent(entity, renderBounds);

                var worldRenderBounds = new WorldRenderBounds { Value = worldBounds };
                if (state.EntityManager.HasComponent<WorldRenderBounds>(entity))
                    ecb.SetComponent(entity, worldRenderBounds);
                else
                    ecb.AddComponent(entity, worldRenderBounds);

                var chunkBounds = new ChunkWorldRenderBounds { Value = worldBounds };
                if (state.EntityManager.HasComponent<ChunkWorldRenderBounds>(entity))
                    ecb.SetComponent(entity, chunkBounds);
                else
                    ecb.AddComponent(entity, chunkBounds);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static int FindEntry(ushort archetypeId, ref BlobArray<RenderCatalogEntry> entries)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].ArchetypeId == archetypeId)
                    return i;
            }

            return -1;
        }
    }
}
