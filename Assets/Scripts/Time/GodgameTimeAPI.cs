using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using PureDOTS.Runtime.Time;
using PureDOTS.Runtime.Components;

namespace Godgame.Time
{
    /// <summary>
    /// Godgame-specific time control API.
    /// Extends the base TimeAPI with Godgame-specific helpers and miracles.
    /// </summary>
    public static class GodgameTimeAPI
    {
        /// <summary>
        /// Pauses the simulation.
        /// </summary>
        public static void Pause() => TimeAPI.Pause();

        /// <summary>
        /// Resumes the simulation.
        /// </summary>
        public static void Resume() => TimeAPI.Resume();

        /// <summary>
        /// Sets the simulation speed multiplier.
        /// </summary>
        /// <param name="speed">Speed multiplier (0.01-16.0)</param>
        public static void SetSpeed(float speed) => TimeAPI.SetSpeed(speed);

        /// <summary>
        /// Steps the simulation forward by one tick.
        /// </summary>
        public static void StepOneTick() => TimeAPI.StepOneTick();

        /// <summary>
        /// Gets the current simulation tick.
        /// </summary>
        public static uint GetCurrentTick() => TimeAPI.GetCurrentTick();

        /// <summary>
        /// Gets the current effective time scale.
        /// </summary>
        public static float GetCurrentScale() => TimeAPI.GetCurrentScale();

        /// <summary>
        /// Creates a stasis bubble at the specified position.
        /// This is a "time miracle" that freezes entities within the bubble.
        /// </summary>
        /// <param name="position">Center position of the bubble</param>
        /// <param name="radius">Radius of the bubble</param>
        /// <param name="durationTicks">Duration in ticks (0 = permanent until removed)</param>
        /// <returns>Entity ID of the created bubble, or Entity.Null if creation failed</returns>
        public static Entity CreateStasisBubble(float3 position, float radius, uint durationTicks)
        {
            return TimeAPI.CreateStasisBubble(position, radius, durationTicks);
        }

        /// <summary>
        /// Requests a rewind of the local player region.
        /// </summary>
        /// <param name="lastNSeconds">Number of seconds to rewind</param>
        public static void RewindLocalPlayerRegion(float lastNSeconds)
        {
            TimeAPI.RewindLocalPlayerRegion(lastNSeconds);
        }

        /// <summary>
        /// Begins preview rewind - freezes world and starts scrubbing ghosts backwards.
        /// </summary>
        /// <param name="scrubSpeed">Rewind speed multiplier (1-4x)</param>
        public static void BeginRewindPreview(float scrubSpeed)
        {
            TimeAPI.BeginRewindPreview(scrubSpeed);
        }

        /// <summary>
        /// Updates the preview rewind scrub speed while scrubbing.
        /// </summary>
        /// <param name="scrubSpeed">New rewind speed multiplier (1-4x)</param>
        public static void UpdateRewindPreviewSpeed(float scrubSpeed)
        {
            TimeAPI.UpdateRewindPreviewSpeed(scrubSpeed);
        }

        /// <summary>
        /// Ends scrub preview - freezes ghosts at current preview position.
        /// </summary>
        public static void EndRewindScrub()
        {
            TimeAPI.EndRewindScrub();
        }

        /// <summary>
        /// Commits rewind from preview - applies rewind to world state.
        /// </summary>
        public static void CommitRewindFromPreview()
        {
            TimeAPI.CommitRewindFromPreview();
        }

        /// <summary>
        /// Cancels rewind preview - aborts without changing world state.
        /// </summary>
        public static void CancelRewindPreview()
        {
            TimeAPI.CancelRewindPreview();
        }
    }
}


