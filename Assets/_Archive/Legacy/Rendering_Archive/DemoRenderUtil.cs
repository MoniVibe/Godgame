#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
#if false
// DISABLED: Utility that modifies MaterialMeshInfo on entities, which can cause batch index -1 errors
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using PureDOTS.Demo.Rendering;

namespace Godgame.Demo
{
    /// <summary>
    /// Helper utilities for making entities renderable with Entities Graphics.
    /// Adds MaterialMeshInfo and RenderBounds components.
    /// </summary>
    public static class DemoRenderUtil
    {
        public static Entity SpawnDebugCube(EntityManager em, float3 position, float scale)
        {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, scale));
            MakeRenderable(em, entity, new float4(1f, 1f, 1f, 1f));
            return entity;
        }

        public static void MakeRenderable(EntityManager em, Entity entity, float4 color)
        {
            // Use default indices (0, 0) for mesh and material
            MakeRenderable(em, entity, color, mat: 0, mesh: 0);
        }

        /// <summary>
        /// Makes an entity renderable by adding MaterialMeshInfo and RenderBounds components.
        /// Uses the specified mesh and material indices from the RenderMeshArray.
        /// </summary>
        public static void MakeRenderable(
            EntityManager em,
            Entity entity,
            float4 color,
            int mat = 0,
            int mesh = 0)
        {
            // Step 1: Add component types first
            em.AddComponent(
                entity,
                new ComponentTypeSet(
                    typeof(MaterialMeshInfo),
                    typeof(RenderBounds)));
            
            // Step 2: Set component values
            var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(mat, mesh);
            em.SetComponentData(entity, materialMeshInfo);
            
            // Explicit RenderBounds (conservative cube bounds)
            em.SetComponentData(entity, new RenderBounds
            {
                Value = new AABB
                {
                    Center = float3.zero, // relative to LocalTransform
                    Extents = new float3(0.5f, 0.5f, 0.5f) // default cube bounds
                }
            });

#if GODGAME_DEBUG && UNITY_EDITOR
            Godgame.GodgameDebug.Log(
                $"[DemoRenderUtil] Made entity {entity.Index} renderable with mesh={mesh}, mat={mat}, color={color}");
#endif
        }

        // If the compiler complains about other DemoRenderUtil methods,
        // add more stub overloads here matching the signatures.
    }

    /// <summary>
    /// Marker used by some legacy demo systems to gate rendering.
    /// </summary>
    public struct DemoRenderReady : IComponentData {}
}
#endif
#endif
