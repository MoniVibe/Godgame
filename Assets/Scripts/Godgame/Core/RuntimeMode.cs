using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Utility for checking whether we're running in a headless/batch environment.
/// Lives in the global namespace so legacy scripts can reference RuntimeMode directly.
/// </summary>
public static class RuntimeMode
{
    public static bool IsHeadless
    {
        get
        {
#if UNITY_SERVER
            return true;
#else
            return Application.isBatchMode || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
#endif
        }
    }
}
