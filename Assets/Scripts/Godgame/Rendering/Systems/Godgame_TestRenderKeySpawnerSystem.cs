using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;
using PureDOTS.Runtime.Core;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Minimal test spawner to validate RenderSemanticKey → variant pipeline.
    /// Spawns one renderable entity in Default world and disables itself.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct Godgame_TestRenderKeySpawnerSystem : ISystem
    {
        private bool _spawned;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _spawned = false;
            if (RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }
            // state.RequireForUpdate<GameWorldTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }
            if (_spawned)
            {
                state.Enabled = false;
                return;
            }

            // Only spawn inside the canonical Game World to avoid DefaultWorld side-effects.
            if (state.WorldUnmanaged.Name != "Game World")
                return;

            var em = state.EntityManager;
            var e = em.CreateEntity();

            em.AddComponentData(e, LocalTransform.FromPositionRotationScale(
                new float3(0f, 0f, 0f),
                quaternion.identity,
                1f));

            em.AddComponentData(e, new RenderSemanticKey { Value = GodgameSemanticKeys.VillageCenter });
            em.AddComponent<MeshPresenter>(e);

            em.AddComponentData(e, new RenderFlags
            {
                Visible = 1,
                ShadowCaster = 1,
                HighlightMask = 0
            });

#if UNITY_EDITOR
            LogSpawnMessage();
#endif

            _spawned = true;
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state) { }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogSpawnMessage()
        {
            Log.Message("[Godgame_TestRenderKeySpawnerSystem] Spawned test RenderSemanticKey entity in Game World.");
        }
#endif
    }
}
