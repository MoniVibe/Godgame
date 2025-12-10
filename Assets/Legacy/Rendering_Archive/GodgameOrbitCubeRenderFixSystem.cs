#if LEGACY_RENDERING_ARCHIVE_DISABLED
using Unity.Entities;
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
            var em = state.EntityManager;
            var rmaQuery = SystemAPI.QueryBuilder().WithAll<RenderMeshArraySingleton>().Build();
            if (rmaQuery.IsEmpty)
            {
                return;
            }

            var rmaEntity = rmaQuery.GetSingletonEntity();
            var renderMeshArray = em.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity).Value;
            if (renderMeshArray == null)
            {
                return;
            }

            var desc = new RenderMeshDescription();
            int fixedCount = 0;

            // Find all orbit cubes that are missing render components
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<OrbitCubeTag>()
                .WithNone<MaterialMeshInfo>()
                .WithEntityAccess())
            {
                RenderMeshUtility.AddComponents(
                    entity,
                    em,
                    desc,
                    renderMeshArray,
                    MaterialMeshInfo.FromRenderMeshArrayIndices(
                        DemoMeshIndices.VillageVillagerMeshIndex, // mesh index (cube)
                        DemoMeshIndices.DemoMaterialIndex));      // material index (debug)
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
