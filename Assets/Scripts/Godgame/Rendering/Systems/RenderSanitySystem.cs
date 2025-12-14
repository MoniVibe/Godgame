using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;
using UnityEngine;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Emits loud errors whenever the render path is miswired so failures are obvious.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct RenderSanitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = true;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!Application.isPlaying)
                return;

            var q = SystemAPI.QueryBuilder()
                .WithAll<RenderKey>()
                .Build();

            int count = q.CalculateEntityCount();

            if (count == 0)
            {
#if UNITY_EDITOR
                LogOnceMissing(state.WorldUnmanaged.Name);
#endif
            }
            else
            {
#if UNITY_EDITOR
                LogOnceCount(state.WorldUnmanaged.Name, count);
#endif
            }

            // Run once to avoid repeated logging.
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state) { }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogOnceMissing(FixedString128Bytes worldName)
        {
            LogError.Message($"[RenderSanitySystem] No RenderKey entities exist in world '{worldName}'; nothing can render.");
        }

        [BurstDiscard]
        private static void LogOnceCount(FixedString128Bytes worldName, int count)
        {
            Log.Message($"[RenderSanitySystem] World '{worldName}' has {count} RenderKey entities.");
        }
#endif
    }
}
