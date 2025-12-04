using System.Diagnostics;
using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Lightweight logging facade so we can turn logs on/off per build.
    /// </summary>
    public static class GodgameDebug
    {
        // --- 1-arg overloads -------------------------------------------------

        [Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        // --- 2-arg overloads (context + message) -----------------------------

        [Conditional("UNITY_EDITOR")]
        public static void Log(object context, object message)
        {
            var prefix = context != null ? $"[{context}] " : "";
            UnityEngine.Debug.Log(prefix + message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object context, object message)
        {
            var prefix = context != null ? $"[{context}] " : "";
            UnityEngine.Debug.LogWarning(prefix + message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object context, object message)
        {
            var prefix = context != null ? $"[{context}] " : "";
            UnityEngine.Debug.LogError(prefix + message);
        }
    }
}

