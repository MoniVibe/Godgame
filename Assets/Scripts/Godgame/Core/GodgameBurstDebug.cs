#if UNITY_EDITOR
using Unity.Burst;

namespace Godgame.Core
{
    /// <summary>
    /// Burst-safe logging helpers for Godgame systems.
    /// </summary>
    static class GodgameBurstDebug
    {
        [BurstDiscard]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [BurstDiscard]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [BurstDiscard]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
#endif
