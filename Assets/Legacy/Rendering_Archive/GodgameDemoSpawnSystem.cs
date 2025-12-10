#if false
// DISABLED: Depends on DemoRenderUtil which modifies MaterialMeshInfo and can cause batch index -1 errors
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PureDOTS.Demo.Rendering; // DemoRenderUtil.MakeRenderable(...)

namespace Godgame.Demo
{
    // Run early so the cubes appear as soon as the world boots.
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Demo.Rendering.SharedRenderBootstrap))]
    public partial struct GodgameDemoSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // If you want to gate on your bootstrap singleton, add RequireForUpdate here.
            // state.RequireForUpdate<SharedRenderBootstrapTag>(); // if you exposed one
        }

        public void OnUpdate(ref SystemState state)
        {
            // Run once.
            state.Enabled = false;

            var em = state.EntityManager;

            const int size = 5;
            const float spacing = 2.5f;

            for (int z = 0; z < size; z++)
            for (int x = 0; x < size; x++)
            {
                var e = em.CreateEntity();
                var pos = new float3((x - (size-1)/2f) * spacing, 0, (z - (size-1)/2f) * spacing);
                em.AddComponentData(e, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 1f));

                // Nice gradient so you can tell cubes apart.
                var color = new float4(x / (size - 1f), z / (size - 1f), 0.85f, 1f);

                // Uses SharedRenderBootstrap's RenderMeshArray under the hood.
                // Mesh index 0 = cube mesh (magenta material in our setup)
                // Material index 0 = magenta material (matches mesh index 0)
                DemoRenderUtil.MakeRenderable(em, e, color, mat: 0, mesh: 0);
            }
        }
    }
}
#endif
