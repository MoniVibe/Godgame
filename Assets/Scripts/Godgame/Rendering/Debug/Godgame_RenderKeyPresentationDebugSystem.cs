using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Debug
{
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    public partial struct Godgame_RenderKeyPresentationDebugSystem : ISystem
    {
#if UNITY_EDITOR
        public void OnUpdate(ref SystemState state)
        {
            var q = SystemAPI.QueryBuilder()
                .WithAll<RenderKey>()
                .Build();

            int count = q.CalculateEntityCount();

            state.Enabled = false; // log once; remove if continuous logging is desired
            var worldName = state.WorldUnmanaged.Name;
            Log.Message($"[Godgame RenderKey PRESENTATION] World '{worldName}' has {count} RenderKey entities.");
        }
#else
        public void OnUpdate(ref SystemState state) { }
#endif

        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }
    }
}
