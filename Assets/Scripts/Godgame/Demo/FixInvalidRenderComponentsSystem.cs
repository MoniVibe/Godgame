#if false
// DISABLED: Uses outdated Entities Graphics API (RenderMeshArraySingleton, MaterialMeshInfo.MaterialMeshIndex)
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using PureDOTS.Demo.Rendering;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// Fixup system that corrects invalid MaterialMeshIndex values and handles
    /// entities with RenderBounds but no MaterialMeshInfo.
    /// Runs in InitializationSystemGroup after RenderMeshArray is populated.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(GodgameDemoRenderSetupSystem))]
    public partial struct FixInvalidRenderComponentsSystem : ISystem
    {
        private bool _fixedOnce;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RenderMeshArraySingleton>();
        }

        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Get RenderMeshArray
            var rmaQuery = SystemAPI.QueryBuilder().WithAll<RenderMeshArraySingleton>().Build();
            if (rmaQuery.IsEmpty)
            {
                return;
            }

            var rmaEntity = rmaQuery.GetSingletonEntity();
            var rmaSingleton = em.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
            var rma = rmaSingleton.Value;

            int meshCount = rma.MeshReferences != null ? rma.MeshReferences.Length : 0;
            int materialCount = rma.MaterialReferences != null ? rma.MaterialReferences.Length : 0;
            
            if (meshCount == 0 || materialCount == 0)
            {
                Debug.LogWarning("[RenderFixup] RenderMeshArray is empty; cannot fix invalid indices.");
                ecb.Dispose();
                return;
            }
            
            int maxValidIndex = math.min(meshCount, materialCount) - 1;

            int fixedCount = 0;

            // Fix entities with invalid MaterialMeshIndex
            foreach (var (mmi, entity) in SystemAPI.Query<RefRW<MaterialMeshInfo>>()
                                                   .WithEntityAccess())
            {
                var currentIndex = mmi.ValueRO.MaterialMeshIndex;
                if (currentIndex < 0 || currentIndex > maxValidIndex)
                {
                    int fixedIndex = currentIndex < 0 ? 0 : maxValidIndex;
                    Debug.LogError(
                        $"[RenderFixup] Fixing invalid MaterialMeshIndex={currentIndex} on entity {entity}. " +
                        $"Forcing to {fixedIndex}. MeshCount={meshCount}, MaterialCount={materialCount}");

                    mmi.ValueRW.MaterialMeshIndex = fixedIndex;
                    fixedCount++;
                }
            }

            // Handle entities with RenderBounds but no MaterialMeshInfo
            // For now, we'll assign a default MaterialMeshInfo (index 0) if they have RenderBounds
            // This assumes they should render. If they shouldn't render, RenderBounds should be removed elsewhere.
            var rbNoMmiQuery = SystemAPI.QueryBuilder()
                .WithAll<RenderBounds>()
                .WithNone<MaterialMeshInfo>()
                .Build();

            using var rbNoMmiEntities = rbNoMmiQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in rbNoMmiEntities)
            {
                // Assign default MaterialMeshInfo (index 0) and ensure they have the RenderMeshArray shared component
                var defaultMmi = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);
                ecb.AddComponent(entity, defaultMmi);
                ecb.AddSharedComponentManaged(entity, rma);
                Debug.LogWarning(
                    $"[RenderFixup] Entity {entity} has RenderBounds but no MaterialMeshInfo. " +
                    $"Assigning default MaterialMeshInfo (index 0).");
                fixedCount++;
            }

            if (fixedCount > 0)
            {
                ecb.Playback(em);
                Debug.Log($"[RenderFixup] Fixed {fixedCount} entities with invalid render components.");
            }

            ecb.Dispose();

            // Disable after first run if no fixes were needed, or keep running if fixes were applied
            // (in case new entities are created with invalid indices)
            if (fixedCount == 0 && _fixedOnce)
            {
                state.Enabled = false;
            }

            _fixedOnce = true;
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
#endif

