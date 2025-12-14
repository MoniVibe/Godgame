using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Debug
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct Godgame_RenderKeySimDebugSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            var q = SystemAPI.QueryBuilder()
                .WithAll<RenderKey>()
                .Build();

            int count = q.CalculateEntityCount();
            state.Enabled = false;
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
            Log.Message($"[Godgame RenderKey SIM] World '{worldName}' has {count} RenderKey entities.");
        }
#endif
    }
}
