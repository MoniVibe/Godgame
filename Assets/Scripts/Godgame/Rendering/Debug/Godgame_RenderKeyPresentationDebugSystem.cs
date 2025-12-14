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
        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            var q = SystemAPI.QueryBuilder()
                .WithAll<RenderKey>()
                .Build();

            int count = q.CalculateEntityCount();
            state.Enabled = false; // log once; remove if continuous logging is desired
            LogRenderKeyCount(state.WorldUnmanaged.Name, count);
#else
            state.Enabled = false;
#endif
        }

        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogRenderKeyCount(FixedString128Bytes worldName, int count)
        {
            Log.Message($"[Godgame RenderKey PRESENTATION] World '{worldName}' has {count} RenderKey entities.");
        }
#endif
    }
}
