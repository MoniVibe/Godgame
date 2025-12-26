using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Utility for checking whether we're running in a headless/batch environment.
/// Lives in the global namespace so legacy scripts can reference RuntimeMode directly.
/// </summary>
public static class RuntimeMode
{
    private const string RenderingEnvVar = "PUREDOTS_RENDERING";
    private static readonly SharedStatic<byte> s_renderingEnabled =
        SharedStatic<byte>.GetOrCreate<RenderingEnabledContext, RenderingEnabledKey>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void InitializeRenderingFlag()
    {
        var value = System.Environment.GetEnvironmentVariable(RenderingEnvVar);
        s_renderingEnabled.Data = (byte)(
            string.Equals(value, "1", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "true", System.StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0);
    }

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

    public static bool IsRenderingEnabled
    {
        get
        {
            if (!IsHeadless)
            {
                return true;
            }
            return s_renderingEnabled.Data != 0;
        }
    }

    private struct RenderingEnabledKey
    {
    }

    private struct RenderingEnabledContext
    {
    }
}
