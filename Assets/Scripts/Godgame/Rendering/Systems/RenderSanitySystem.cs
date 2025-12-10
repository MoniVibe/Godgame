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
                LogError.Message($"[RenderSanitySystem] No RenderKey entities exist in world '{state.WorldUnmanaged.Name}'; nothing can render.");
            }
            else
            {
                Log.Message($"[RenderSanitySystem] World '{state.WorldUnmanaged.Name}' has {count} RenderKey entities.");
            }

            // Run once to avoid repeated logging.
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
