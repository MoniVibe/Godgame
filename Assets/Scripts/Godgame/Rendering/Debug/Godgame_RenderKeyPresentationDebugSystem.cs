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
        private EntityQuery _renderSemanticKeyQuery;

        public void OnCreate(ref SystemState state)
        {
            _renderSemanticKeyQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderSemanticKey>());
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            int count = _renderSemanticKeyQuery.CalculateEntityCount();
            state.Enabled = false; // log once; remove if continuous logging is desired
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
            Log.Message($"[Godgame Render PRESENTATION] World '{worldName}' has {count} RenderSemanticKey entities.");
        }
#endif
    }
}
