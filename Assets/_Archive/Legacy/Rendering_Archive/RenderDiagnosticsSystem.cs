#if LEGACY_RENDERING_ARCHIVE_DISABLED
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using PureDOTS.Demo.Rendering;

namespace Godgame.Debugging
{
    /// <summary>
    /// Diagnostic system that scans entities for invalid render indices
    /// and logs entities with RenderBounds but no MaterialMeshInfo.
    /// Runs once per world initialization to help diagnose rendering issues.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct RenderDiagnosticsSystem : ISystem
    {
        private bool _loggedOnce;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RenderMeshArraySingleton>();
        }

        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            if (_loggedOnce)
                return;

            _loggedOnce = true;

            var em = state.EntityManager;

            // Get RenderMeshArray
            var rmaQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderMeshArraySingleton>());
            if (rmaQuery.IsEmpty)
            {
                Debug.LogWarning("[RenderDiagnostics] RenderMeshArraySingleton not found; skipping diagnostics.");
                return;
            }

            var rmaEntity = rmaQuery.GetSingletonEntity();
            var rmaSingleton = em.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
            var renderMeshes = rmaSingleton.Value;

            int meshCount = renderMeshes.MeshReferences != null ? renderMeshes.MeshReferences.Length : 0;
            int materialCount = renderMeshes.MaterialReferences != null ? renderMeshes.MaterialReferences.Length : 0;

            Debug.Log($"[RenderDiagnostics] RenderMeshArray meshes={meshCount}, materials={materialCount}");

            if (meshCount == 0 || materialCount == 0)
            {
                Debug.LogWarning("[RenderDiagnostics] RenderMeshArray is empty; cannot validate indices.");
                return;
            }

            int invalidCount = 0;

            // 1. Entities with MaterialMeshInfo + RenderBounds
            foreach (var (mmi, entity) in SystemAPI.Query<RefRO<MaterialMeshInfo>>()
                                                   .WithEntityAccess())
            {
                // MaterialMeshInfo in Entities Graphics 1.4 stores mesh/material IDs; clamp if out of range
                var meshIndex = mmi.ValueRO.Mesh;
                var materialIndex = mmi.ValueRO.Material;

                if (meshIndex < 0 || materialIndex < 0)
                {
                    invalidCount++;
                    Debug.LogWarning($"[RenderDiagnostics] Entity {entity} has negative MaterialMeshInfo (Mesh={meshIndex}, Material={materialIndex}); skipping.");
                    continue;
                }

                if (meshIndex >= meshCount || materialIndex >= materialCount)
                {
                    invalidCount++;
                    Debug.LogWarning($"[RenderDiagnostics] Entity {entity} has out-of-range MaterialMeshInfo (Mesh={meshIndex}/{meshCount}, Material={materialIndex}/{materialCount}); skipping.");
                    continue;
                }
            }

            // 2. Entities with RenderBounds but no MaterialMeshInfo
            var rbQuery = SystemAPI.QueryBuilder()
                .WithAll<RenderBounds>()
                .WithNone<MaterialMeshInfo>()
                .Build();

            using var rbEntities = rbQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var e in rbEntities)
            {
                Debug.LogWarning($"[RenderDiagnostics] Entity {e} has RenderBounds but no MaterialMeshInfo.");
            }

            if (invalidCount > 0)
            {
                Debug.LogWarning($"[RenderDiagnostics] Found {invalidCount} entities with invalid RenderMeshArray indices.");
            }
            else
            {
                Debug.Log("[RenderDiagnostics] All RenderMeshArray indices are valid.");
            }

            Debug.Log($"[RenderDiagnostics] Scan complete. Found {rbEntities.Length} entities with RenderBounds but no MaterialMeshInfo.");
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
#endif

