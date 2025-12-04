using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Backwards-compat shim for old Godgame.Time.* usages.
    /// For now this just forwards to UnityEngine.Time.
    /// Later we can wire this into the ECS time systems.
    /// 
    /// This compatibility wrapper restores the old static Time API that was removed during the time refactor.
    /// Long term, prefer GodgameTimeAPI (ECS-aware) instead of this.
    /// 
    /// Note: Renamed from 'Time' to 'GodgameTime' to avoid conflict with Godgame.Time namespace.
    /// </summary>
    public static class GodgameTime
    {
        /// <summary>
        /// Total time in seconds since the start of the application.
        /// Mirrors UnityEngine.Time.time.
        /// </summary>
        public static float time => UnityEngine.Time.time;

        /// <summary>
        /// Frame count since startup.
        /// </summary>
        public static int frameCount => UnityEngine.Time.frameCount;

        /// <summary>
        /// Scaled delta time (affected by UnityEngine.Time.timeScale).
        /// </summary>
        public static float deltaTime => UnityEngine.Time.deltaTime;

        /// <summary>
        /// Unscaled delta time (ignores UnityEngine.Time.timeScale).
        /// Useful for UI and camera controls.
        /// </summary>
        public static float unscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;

        /// <summary>
        /// Fixed delta time (physics step).
        /// </summary>
        public static float fixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

        /// <summary>
        /// Time scale multiplier (affects deltaTime).
        /// Old code uses Godgame.Time.timeScale.
        /// </summary>
        public static float timeScale
        {
            get => UnityEngine.Time.timeScale;
            set => UnityEngine.Time.timeScale = value;
        }
    }
}

