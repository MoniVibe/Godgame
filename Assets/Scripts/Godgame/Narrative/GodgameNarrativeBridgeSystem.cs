using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Narrative;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Narrative
{
    /// <summary>
    /// Bridge system that reads PureDOTS narrative signals and maps them to Godgame-specific behaviors.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameNarrativeBridgeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<NarrativeSignalBufferElement>();
            state.RequireForUpdate<NarrativeRewardSignal>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!Application.isPlaying)
                return;

            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState))
                return;
            if (!SystemAPI.TryGetSingleton<RewindState>(out var rewindState))
                return;
            
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            // Read narrative signals
            if (!SystemAPI.TryGetSingletonEntity<NarrativeSignalBufferElement>(out var signalEntity))
            {
                return;
            }

            var signalBuffer = state.EntityManager.GetBuffer<NarrativeSignalBufferElement>(signalEntity);

            // Process signals
            for (int i = signalBuffer.Length - 1; i >= 0; i--)
            {
                var signal = signalBuffer[i];

                if (signal.SignalType == 0) // SituationStarted
                {
                    // Check if tags include Hostage
                    // For now, just log and mark location
                    UnityEngine.Debug.Log($"[GodgameNarrativeBridge] Situation started: {signal.Id.Value} at entity {signal.Target.Index}");

                    if (signal.Target != Entity.Null && state.EntityManager.Exists(signal.Target))
                    {
                        // Add debug marker tag (create simple tag component)
                        if (!state.EntityManager.HasComponent<DebugHostageMarker>(signal.Target))
                        {
                            state.EntityManager.AddComponent<DebugHostageMarker>(signal.Target);
                        }
                    }
                }
                else if (signal.SignalType == 1) // StepEntered
                {
                    UnityEngine.Debug.Log($"[GodgameNarrativeBridge] Situation step entered: {signal.Id.Value}, step {signal.PayloadA}");
                }
                else if (signal.SignalType == 2) // EventFired
                {
                    UnityEngine.Debug.Log($"[GodgameNarrativeBridge] Event fired: {signal.Id.Value}");
                }

                // Remove processed signal
                signalBuffer.RemoveAt(i);
            }

            // Read reward signals
            if (!SystemAPI.TryGetSingletonEntity<NarrativeRewardSignal>(out var rewardEntity))
            {
                return;
            }

            var rewardBuffer = state.EntityManager.GetBuffer<NarrativeRewardSignal>(rewardEntity);

            for (int i = rewardBuffer.Length - 1; i >= 0; i--)
            {
                var reward = rewardBuffer[i];
                UnityEngine.Debug.Log($"[GodgameNarrativeBridge] Reward: type={reward.RewardType}, amount={reward.Amount}, target={reward.Target.Index}");
                
                // Remove processed reward
                rewardBuffer.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Simple tag component for marking locations with hostage situations.
    /// </summary>
    public struct DebugHostageMarker : IComponentData
    {
    }
}

