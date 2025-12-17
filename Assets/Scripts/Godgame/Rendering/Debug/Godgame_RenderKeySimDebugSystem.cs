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
        private EntityQuery _renderSemanticKeyQuery;

        public void OnCreate(ref SystemState state)
        {
            _renderSemanticKeyQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderSemanticKey>());
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            int count = _renderSemanticKeyQuery.CalculateEntityCount();
            state.Enabled = false;
            LogRenderKeyCount(state.WorldUnmanaged.Name, count);
#else
            state.Enabled = false;
#endif
        }

        public void OnDestroy(ref SystemState state) { }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogRenderKeyCount(FixedString128Bytes worldName, int count)
        {
            Log.Message($"[Godgame Render SIM] World '{worldName}' has {count} RenderSemanticKey entities.");
        }
#endif
    }
}
