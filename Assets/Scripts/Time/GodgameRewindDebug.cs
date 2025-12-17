using PureDOTS.Input;
using UnityEngine;
using Godgame.Time;

namespace Godgame.TimeDebug
{
    /// <summary>
    /// Debug MonoBehaviour for testing preview rewind functionality.
    /// Hold R to scrub backwards through time (ghosts preview rewind).
    /// Release R to freeze preview.
    /// Press Space to commit rewind.
    /// Press C or Escape to cancel rewind.
    /// </summary>
    public class GodgameRewindDebug : MonoBehaviour
    {
        [Header("Rewind Preview Settings")]
        [Tooltip("Rewind scrub speed multiplier (1-4x)")]
        [Range(1f, 4f)]
        public float scrubSpeed = 2.0f;
        
        [Tooltip("Key to hold for scrubbing rewind")]
        public KeyCode rewindKey = KeyCode.R;
        
        [Tooltip("Key to commit rewind from preview")]
        public KeyCode commitKey = KeyCode.Space;
        
        [Tooltip("Key to cancel rewind preview")]
        public KeyCode cancelKey = KeyCode.C;
        
        [Header("Debug")]
        [Tooltip("Log rewind events")]
        public bool logRewindEvents = true;

        private bool _isScrubbing = false;

        private void Update()
        {
            if (!_isScrubbing && Hotkeys.KeyDown(rewindKey))
            {
                BeginRewindPreview();
            }

            if (_isScrubbing && Hotkeys.KeyUp(rewindKey))
            {
                EndRewindScrub();
            }

            if (Hotkeys.KeyDown(commitKey))
            {
                CommitRewind();
            }

            if (Hotkeys.KeyDown(cancelKey) || Hotkeys.KeyDown(KeyCode.Escape))
            {
                CancelRewind();
            }
        }

        private void BeginRewindPreview()
        {
            _isScrubbing = true;
            
            if (logRewindEvents)
            {
                var currentTick = GodgameTimeAPI.GetCurrentTick();
                Debug.Log($"[RewindDebug] Starting preview rewind at tick {currentTick}, scrub speed {scrubSpeed:F2}x");
            }

            GodgameTimeAPI.BeginRewindPreview(scrubSpeed);
        }

        private void EndRewindScrub()
        {
            _isScrubbing = false;
            
            if (logRewindEvents)
            {
                Debug.Log("[RewindDebug] Ended scrub preview, preview frozen");
            }

            GodgameTimeAPI.EndRewindScrub();
        }

        private void CommitRewind()
        {
            if (logRewindEvents)
            {
                Debug.Log("[RewindDebug] Committing rewind from preview");
            }

            GodgameTimeAPI.CommitRewindFromPreview();
            _isScrubbing = false; // Reset state
        }

        private void CancelRewind()
        {
            if (logRewindEvents)
            {
                Debug.Log("[RewindDebug] Cancelling rewind preview");
            }

            GodgameTimeAPI.CancelRewindPreview();
            _isScrubbing = false; // Reset state
        }
    }
}
