using Godgame.Presentation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Performance
{
    /// <summary>
    /// System that automatically adjusts visual settings when performance budgets are exceeded.
    /// Monitors PresentationMetrics and PresentationConfig budgets, applies graceful degradation.
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_PresentationMetricsSystem))]
    [UpdateAfter(typeof(Godgame_PerformanceValidationSystem))]
    public partial struct Godgame_AutoPerformanceAdjustmentSystem : ISystem
    {
        private int _consecutiveBudgetExceededFrames;
        private int _consecutiveRecoveryFrames;
        private const int BudgetExceededThreshold = 3; // Apply degradation after 3 consecutive checks
        private const int RecoveryThreshold = 10; // Restore after 10 consecutive recovery checks

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationConfig>();
            state.RequireForUpdate<PresentationMetrics>();
            _consecutiveBudgetExceededFrames = 0;
            _consecutiveRecoveryFrames = 0;
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            // Check every 60 frames (1 second at 60fps)
            if (UnityEngine.Time.frameCount % 60 != 0)
            {
                return;
            }

            RefRW<PresentationConfig> configRef = default;
            bool hasConfig = false;

            foreach (var candidate in SystemAPI.Query<RefRW<PresentationConfig>>()
                         .WithNone<PresentationConfigRuntimeTag>())
            {
                configRef = candidate;
                hasConfig = true;
                break;
            }

            if (!hasConfig)
            {
                foreach (var candidate in SystemAPI.Query<RefRW<PresentationConfig>>())
                {
                    configRef = candidate;
                    hasConfig = true;
                    break;
                }
            }

            if (!hasConfig)
            {
                return;
            }

            ref var config = ref configRef.ValueRW;

            if (!SystemAPI.TryGetSingleton<PresentationMetrics>(out var metrics))
            {
                return;
            }

            float frameTimeMs = UnityEngine.Time.deltaTime * 1000f;
            bool budgetExceeded = frameTimeMs > config.MaxFrameTimeMs ||
                                 metrics.VillagersRendered > config.MaxLOD0Villagers * 2 ||
                                 metrics.ChunksRendered > config.MaxRenderedChunks;

            if (budgetExceeded)
            {
                _consecutiveBudgetExceededFrames++;
                _consecutiveRecoveryFrames = 0;

                if (_consecutiveBudgetExceededFrames >= BudgetExceededThreshold)
                {
                    ApplyDegradation(ref config);
                    Debug.Log("[AutoPerformanceAdjustment] Auto-adjusting visual density to maintain performance.");
                    _consecutiveBudgetExceededFrames = 0; // Reset counter
                }
            }
            else
            {
                _consecutiveBudgetExceededFrames = 0;
                _consecutiveRecoveryFrames++;

                // Optional: Gradually restore settings if performance recovers
                // For now, we keep degraded settings to avoid oscillation
                // if (_consecutiveRecoveryFrames >= RecoveryThreshold)
                // {
                //     RestoreSettings(ref config);
                //     _consecutiveRecoveryFrames = 0;
                // }
            }
#endif
        }

        private void ApplyDegradation(ref PresentationConfig config)
        {
            // Increase LOD thresholds (push entities to higher LOD sooner)
            config.LOD0Distance *= 0.8f;
            config.LOD1Distance *= 0.8f;
            config.LOD2Distance *= 0.8f;

            // Increase density sampling factor (render fewer entities)
            config.DensitySlider = math.max(0.1f, config.DensitySlider * 0.8f);

            // Reduce max rendered entities
            config.MaxLOD0Villagers = math.max(100, (int)(config.MaxLOD0Villagers * 0.8f));
            config.MaxRenderedChunks = math.max(1000, (int)(config.MaxRenderedChunks * 0.8f));
        }
    }
}
