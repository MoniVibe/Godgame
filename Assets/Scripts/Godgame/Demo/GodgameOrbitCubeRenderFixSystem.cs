#if false
// DISABLED: Modifies MaterialMeshInfo on existing entities, which can cause batch index -1 errors
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using PureDOTS.Demo.Orbit;
using PureDOTS.Demo.Rendering;

namespace Godgame.Demo
{
    /// <summary>
    /// Ensures orbit cubes spawned by PureDOTS systems have MaterialMeshInfo and RenderBounds
    /// so they render correctly with Entities Graphics.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Demo.Rendering.SharedRenderBootstrap))]
    public partial struct GodgameOrbitCubeRenderFixSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Only run if RenderMeshArray exists
            state.RequireForUpdate<RenderMeshArraySingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Run once, then disable
            state.Enabled = false;

            var em = state.EntityManager;
            int fixedCount = 0;

            // Find all orbit cubes that are missing render components
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<OrbitCubeTag>()
                .WithNone<MaterialMeshInfo>()
                .WithEntityAccess())
            {
                // Add MaterialMeshInfo pointing to cube mesh and material
                // Orbit cubes should use the same mesh as villagers (index 3) since that's the cube mesh in our RenderMeshArray
                // If PureDOTS uses a different index, this will need adjustment
                em.AddComponentData(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(
                    DemoMeshIndices.DemoMaterialIndex, // material
                    DemoMeshIndices.VillageVillagerMeshIndex // mesh
                ));

                // Add RenderBounds (cube-sized)
                em.AddComponentData(entity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero, // relative to LocalTransform
                        Extents = new float3(0.5f, 0.5f, 0.5f) // cube bounds
                    }
                });

                fixedCount++;
            }

            // Also check for orbit cubes that have MaterialMeshInfo but missing RenderBounds
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<OrbitCubeTag, MaterialMeshInfo>()
                .WithNone<RenderBounds>()
                .WithEntityAccess())
            {
                em.AddComponentData(entity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero,
                        Extents = new float3(0.5f, 0.5f, 0.5f)
                    }
                });
                fixedCount++;
            }

            if (fixedCount > 0)
            {
                Godgame.GodgameDebug.Log($"[GodgameOrbitCubeRenderFix] Added MaterialMeshInfo/RenderBounds to {fixedCount} orbit cubes");
            }
        }
    }
}
#endif

