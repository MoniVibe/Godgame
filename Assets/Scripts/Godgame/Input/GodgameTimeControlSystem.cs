using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Godgame.Core;
using UnityEngine;

namespace Godgame.Input
{
    /// <summary>
    /// System that reads input and writes TimeControlCommand to PureDOTS time system.
    /// Integrates with PureDOTS TimeControlCommand for deterministic rewind support.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameTimeControlSystem : ISystem
    {
        private float _currentSpeed;
        private bool _isPaused;
        private bool _logInitialized;
        private float _lastLoggedSpeed;
        private bool _lastLoggedPaused;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            _currentSpeed = 1f;
            _isPaused = false;
            _logInitialized = false;
            _lastLoggedSpeed = 0f;
            _lastLoggedPaused = false;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get input (from InputReader or direct input)
            // For now, this is a placeholder - actual input would come from GodgameInputReader
            // or a dedicated TimeControlInput component

            // Read TimeState for current state
            var timeState = SystemAPI.GetSingleton<TimeState>();
            _isPaused = timeState.IsPaused;
            _currentSpeed = timeState.CurrentSpeedMultiplier;

            // Get RewindState entity to write commands (safe guard)
            var rewindEntity = SystemAPI.GetSingletonEntity<RewindState>();

            if (!state.EntityManager.HasBuffer<TimeControlCommand>(rewindEntity))
            {
                return;
            }

            var commandBuffer = state.EntityManager.GetBuffer<TimeControlCommand>(rewindEntity);

            // Process input and write commands
            // This would read from InputActions or a TimeControlInput component
            // For now, placeholder logic

            // Example: If pause key pressed, write Pause command
            // Example: If speed key pressed, write SetSpeed command

            // Note: Actual implementation requires:
            // 1. InputActions for time controls (Space, Tab, Shift+Tab, Period)
            // 2. GodgameInputReader extension to read time control inputs
            // 3. Write commands to buffer based on input state
#if UNITY_EDITOR
            if (!Application.isBatchMode &&
                (!_logInitialized || _lastLoggedPaused != _isPaused || _lastLoggedSpeed != _currentSpeed))
            {
                _logInitialized = true;
                _lastLoggedPaused = _isPaused;
                _lastLoggedSpeed = _currentSpeed;
                LogState(_isPaused, _currentSpeed);
            }
#endif
        }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogState(bool paused, float speed)
        {
            GodgameBurstDebug.Log($"[GodgameTimeControl] paused={paused} speed={speed}");
        }
#endif
    }
}
