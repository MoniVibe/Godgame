using Godgame.Presentation;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Performance
{
    /// <summary>
    /// System that validates performance budgets and logs warnings when exceeded.
    /// Editor-only checks (not Burst-compiled).
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_PresentationMetricsSystem))]
    public partial struct Godgame_PerformanceValidationSystem : ISystem
    {
        private int _frameCounter;
        private const int ValidationInterval = 60; // Check every 60 frames

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            _frameCounter++;
            if (_frameCounter < ValidationInterval)
            {
                return;
            }
            _frameCounter = 0;

            var config = SystemAPI.GetSingleton<PresentationConfig>();

            // Check frame time
            float frameTimeMs = UnityEngine.Time.deltaTime * 1000f;
            if (frameTimeMs > config.MaxFrameTimeMs)
            {
                Debug.LogWarning($"[PerformanceValidation] Frame time ({frameTimeMs:F2}ms) exceeds budget ({config.MaxFrameTimeMs}ms)");
            }

            // Check draw calls (requires access to rendering stats)
            // Note: This is a placeholder - actual draw call count would come from Unity's rendering stats
            // For now, we'll check presentation metrics instead
            if (SystemAPI.TryGetSingleton<PresentationMetrics>(out var metrics))
            {
                // Check if too many entities are rendered
                if (metrics.VillagersRendered > config.MaxLOD0Villagers * 2)
                {
                    Debug.LogWarning($"[PerformanceValidation] Rendered villagers ({metrics.VillagersRendered}) exceeds recommended limit ({config.MaxLOD0Villagers * 2})");
                }

                if (metrics.ChunksRendered > config.MaxRenderedChunks)
                {
                    Debug.LogWarning($"[PerformanceValidation] Rendered chunks ({metrics.ChunksRendered}) exceeds budget ({config.MaxRenderedChunks})");
                }
            }

            // Check GC allocations (requires Profiler API)
            // Note: This is a placeholder - actual GC allocation check would use Unity.Profiling
            // For now, we'll skip this check as it requires managed code
#endif
        }
    }
}

