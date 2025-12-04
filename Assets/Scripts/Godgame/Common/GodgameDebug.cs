using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Logging shim that forwards to UnityEngine.Debug.
    /// Use like UnityEngine.Debug, but namespaced under Godgame.
    /// </summary>
    public static class GodgameLogger
    {
        public static void Log(object message)
            => UnityEngine.Debug.Log(message);

        public static void LogWarning(object message)
            => UnityEngine.Debug.LogWarning(message);

        public static void LogError(object message)
            => UnityEngine.Debug.LogError(message);

        // Message + context (second arg is UnityEngine.Object)
        public static void Log(object message, Object context)
            => UnityEngine.Debug.Log(message, context);

        public static void LogWarning(object message, Object context)
            => UnityEngine.Debug.LogWarning(message, context);

        public static void LogError(object message, Object context)
            => UnityEngine.Debug.LogError(message, context);
    }
}

