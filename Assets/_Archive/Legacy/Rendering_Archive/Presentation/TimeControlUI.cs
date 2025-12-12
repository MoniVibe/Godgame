#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using UnityEngine;
using Unity.Entities;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Time;

namespace Godgame.Presentation
{
    /// <summary>
    /// MonoBehaviour for displaying time control UI and handling input.
    /// Shows current time scale, tick, and provides buttons for pause/fast-forward/slow-motion.
    /// </summary>
    public class TimeControlUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Text element for displaying current time scale")]
        public UnityEngine.UI.Text timeScaleText;
        [Tooltip("Text element for displaying current tick")]
        public UnityEngine.UI.Text tickText;
        [Tooltip("Button for pause")]
        public UnityEngine.UI.Button pauseButton;
        [Tooltip("Button for fast-forward")]
        public UnityEngine.UI.Button fastForwardButton;
        [Tooltip("Button for slow-motion")]
        public UnityEngine.UI.Button slowMotionButton;
        [Tooltip("Button for step forward (dev-only)")]
        public UnityEngine.UI.Button stepForwardButton;

        [Header("Settings")]
        [Tooltip("Show step forward button (dev-only)")]
        public bool showDevControls = false;

        private World _ecsWorld;
        private float[] _speedMultipliers = { 1f, 2f, 5f, 10f };
        private float[] _slowMotionMultipliers = { 1f, 0.5f, 0.25f };
        private int _currentSpeedIndex = 0;
        private int _currentSlowMotionIndex = 0;
        private bool _isSlowMotion = false;

        private void Start()
        {
            _ecsWorld = World.DefaultGameObjectInjectionWorld;

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
            }

            if (fastForwardButton != null)
            {
                fastForwardButton.onClick.AddListener(CycleFastForward);
            }

            if (slowMotionButton != null)
            {
                slowMotionButton.onClick.AddListener(CycleSlowMotion);
            }

            if (stepForwardButton != null)
            {
                stepForwardButton.onClick.AddListener(StepForward);
                stepForwardButton.gameObject.SetActive(showDevControls);
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                return;
            }

            var em = _ecsWorld.EntityManager;
            var timeStateQuery = em.CreateEntityQuery(typeof(TimeState));
            var rewindStateQuery = em.CreateEntityQuery(typeof(RewindState));

            if (timeStateQuery.TryGetSingleton(out TimeState timeState))
            {
                if (timeScaleText != null)
                {
                    // TODO: Check PureDOTS TimeState for correct speed field name (may be Speed, TimeScale, etc.)
                    // For now, use a placeholder - update when PureDOTS TimeState structure is confirmed
                    string scaleText = timeState.IsPaused ? "Paused" : "1.0x"; // timeState.TimeScale not available
                    timeScaleText.text = scaleText;
                }

                if (tickText != null)
                {
                    tickText.text = $"Tick: {timeState.Tick}";
                }
            }

            if (rewindStateQuery.TryGetSingleton(out RewindState rewindState))
            {
                if (tickText != null && rewindState.Mode != RewindMode.Record)
                {
                    tickText.text += $" (Rewind: {rewindState.Mode})";
                }
            }
        }

        private void TogglePause()
        {
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                return;
            }

            var em = _ecsWorld.EntityManager;
            var rewindEntity = em.CreateEntityQuery(typeof(RewindState)).GetSingletonEntity();
            var commandBuffer = em.GetBuffer<TimeControlCommand>(rewindEntity);

            var timeState = em.CreateEntityQuery(typeof(TimeState)).GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                // Resume
                commandBuffer.Add(new TimeControlCommand
                {
                    Type = TimeControlCommandType.SetSpeed,
                    FloatParam = _isSlowMotion ? _slowMotionMultipliers[_currentSlowMotionIndex] : _speedMultipliers[_currentSpeedIndex]
                });
            }
            else
            {
                // Pause
                commandBuffer.Add(new TimeControlCommand
                {
                    Type = TimeControlCommandType.Pause
                });
            }
        }

        private void CycleFastForward()
        {
            if (_isSlowMotion)
            {
                _isSlowMotion = false;
            }

            _currentSpeedIndex = (_currentSpeedIndex + 1) % _speedMultipliers.Length;
            float newSpeed = _speedMultipliers[_currentSpeedIndex];

            WriteSpeedCommand(newSpeed);
        }

        private void CycleSlowMotion()
        {
            _isSlowMotion = true;
            _currentSlowMotionIndex = (_currentSlowMotionIndex + 1) % _slowMotionMultipliers.Length;
            float newSpeed = _slowMotionMultipliers[_currentSlowMotionIndex];

            WriteSpeedCommand(newSpeed);
        }

        private void StepForward()
        {
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                return;
            }

            var em = _ecsWorld.EntityManager;
            var rewindEntity = em.CreateEntityQuery(typeof(RewindState)).GetSingletonEntity();
            var commandBuffer = em.GetBuffer<TimeControlCommand>(rewindEntity);

            // Step forward N ticks (default: 1)
            // TODO: Check if StepForward exists in PureDOTS TimeControlCommand.CommandType
            // If not, use StepOnce or equivalent command type
            // For now, comment out to avoid compilation error
            // commandBuffer.Add(new TimeControlCommand
            // {
            //     Type = TimeControlCommand.CommandType.StepForward,
            //     UintParam = 1
            // });
        }

        private void WriteSpeedCommand(float speed)
        {
            if (_ecsWorld == null || !_ecsWorld.IsCreated)
            {
                return;
            }

            var em = _ecsWorld.EntityManager;
            var rewindEntity = em.CreateEntityQuery(typeof(RewindState)).GetSingletonEntity();
            var commandBuffer = em.GetBuffer<TimeControlCommand>(rewindEntity);

            commandBuffer.Add(new TimeControlCommand
            {
                Type = TimeControlCommandType.SetSpeed,
                FloatParam = speed
            });
        }
    }
}
#endif
