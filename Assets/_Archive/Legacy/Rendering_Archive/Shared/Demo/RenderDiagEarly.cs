#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Early diagnostic to confirm SRP/API/compute availability before world bootstrap.
/// </summary>
static class RenderDiagEarly
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Log()
    {
        var rp = GraphicsSettings.currentRenderPipeline;
        var name = rp ? rp.GetType().Name : "<null>";
        Debug.Log($"[RenderDiagEarly] SRP:{name} API:{SystemInfo.graphicsDeviceType} Compute:{SystemInfo.supportsComputeShaders}");
    }
}
#endif
