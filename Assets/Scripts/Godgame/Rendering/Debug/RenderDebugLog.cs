using UnityEngine;
using Unity.Burst;

namespace Godgame.Rendering.Debug
{
    /// <summary>
    /// General log wrapper for Godgame rendering systems.
    /// </summary>
    public static class Log
    {
#if UNITY_EDITOR
        [BurstDiscard] public static void Message(string message) => UnityEngine.Debug.Log(message);
        [BurstDiscard] public static void Message(object message) => UnityEngine.Debug.Log(message);
        [BurstDiscard] public static void Info(string message) => UnityEngine.Debug.Log(message);
        [BurstDiscard] public static void Info(object message) => UnityEngine.Debug.Log(message);
#else
        public static void Message(string message) { }
        public static void Message(object message) { }
        public static void Info(string message) { }
        public static void Info(object message) { }
#endif
    }

    /// <summary>
    /// Error logging wrapper – hard failures.
    /// </summary>
    public static class LogError
    {
#if UNITY_EDITOR
        [BurstDiscard] public static void Message(string message) => UnityEngine.Debug.LogError(message);
        [BurstDiscard] public static void Message(object message) => UnityEngine.Debug.LogError(message);
        [BurstDiscard] public static void Error(string message) => UnityEngine.Debug.LogError(message);
        [BurstDiscard] public static void Error(object message) => UnityEngine.Debug.LogError(message);
#else
        public static void Message(string message) { }
        public static void Message(object message) { }
        public static void Error(string message) { }
        public static void Error(object message) { }
#endif
    }

    /// <summary>
    /// Warning logging wrapper – soft issues.
    /// </summary>
    public static class LogWarning
    {
#if UNITY_EDITOR
        [BurstDiscard] public static void Message(string message) => UnityEngine.Debug.LogWarning(message);
        [BurstDiscard] public static void Message(object message) => UnityEngine.Debug.LogWarning(message);
        [BurstDiscard] public static void Warn(string message) => UnityEngine.Debug.LogWarning(message);
        [BurstDiscard] public static void Warn(object message) => UnityEngine.Debug.LogWarning(message);
#else
        public static void Message(string message) { }
        public static void Message(object message) { }
        public static void Warn(string message) { }
        public static void Warn(object message) { }
#endif
    }
}

