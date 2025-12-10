using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Minimal test spawner to validate RenderKey → catalog → mesh pipeline.
    /// Spawns one RenderKey entity in Default world and disables itself.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct Godgame_TestRenderKeySpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var em = state.EntityManager;
            var e = em.CreateEntity();

            em.AddComponentData(e, LocalTransform.FromPositionRotationScale(
                new float3(0f, 0f, 0f),
                quaternion.identity,
                1f));

            em.AddComponentData(e, new RenderKey
            {
                ArchetypeId = GodgameRenderKeys.VillageCenter,
                LOD = 0
            });

            em.AddComponentData(e, new RenderFlags
            {
                Visible = 1,
                ShadowCaster = 1,
                HighlightMask = 0
            });

            Log.Message("[Godgame_TestRenderKeySpawnerSystem] Spawned test RenderKey entity in Default World.");
            state.Enabled = false;
        }

        [BurstCompile] public void OnUpdate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }
    }
}
