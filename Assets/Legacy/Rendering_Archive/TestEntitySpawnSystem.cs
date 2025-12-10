#if false
// DISABLED: Depends on DemoRenderUtil which modifies MaterialMeshInfo and can cause batch index -1 errors
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using PureDOTS.Demo.Rendering;

namespace Godgame.Demo
{
    public struct TestEntityTag : IComponentData {}

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(SharedRenderBootstrap))]
    public partial class TestEntitySpawnSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<DemoRenderReady>();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.TryGetSingletonEntity<TestEntityTag>(out _)) return;

            var e = EntityManager.CreateEntity();
            EntityManager.AddComponent<TestEntityTag>(e);
            
            // Position (0,1,0), Scale 2
            EntityManager.AddComponentData(e, LocalTransform.FromPositionRotationScale(new float3(0, 1, 0), quaternion.identity, 2f));

            // Make renderable
            DemoRenderUtil.MakeRenderable(EntityManager, e, new float4(1, 0, 0, 1)); // Red color

            Enabled = false; // Run once
        }
    }
}
#endif
