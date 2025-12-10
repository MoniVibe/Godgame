using UnityEngine;

namespace Godgame.Rendering.Debug
{
    /// <summary>
    /// General log wrapper for Godgame rendering systems.
    /// </summary>
    public static class Log
    {
        public static void Message(string message) => UnityEngine.Debug.Log(message);
        public static void Message(object message) => UnityEngine.Debug.Log(message);
        public static void Info(string message) => UnityEngine.Debug.Log(message);
        public static void Info(object message) => UnityEngine.Debug.Log(message);
    }

    /// <summary>
    /// Error logging wrapper – hard failures.
    /// </summary>
    public static class LogError
    {
        public static void Message(string message) => UnityEngine.Debug.LogError(message);
        public static void Message(object message) => UnityEngine.Debug.LogError(message);
        public static void Error(string message) => UnityEngine.Debug.LogError(message);
        public static void Error(object message) => UnityEngine.Debug.LogError(message);
    }

    /// <summary>
    /// Warning logging wrapper – soft issues.
    /// </summary>
    public static class LogWarning
    {
        public static void Message(string message) => UnityEngine.Debug.LogWarning(message);
        public static void Message(object message) => UnityEngine.Debug.LogWarning(message);
        public static void Warn(string message) => UnityEngine.Debug.LogWarning(message);
        public static void Warn(object message) => UnityEngine.Debug.LogWarning(message);
    }
}


