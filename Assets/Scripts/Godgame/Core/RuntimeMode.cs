using System;
using Unity.Burst;
using UnityEngine;

/// <summary>
/// Utility for checking whether we're running in a headless/batch environment.
/// Lives in the global namespace so legacy scripts can reference RuntimeMode directly.
/// </summary>
public static class RuntimeMode
{
    private const string HeadlessEnvVar = "PUREDOTS_HEADLESS";
    private const string NoGraphicsEnvVar = "PUREDOTS_NOGRAPHICS";
    private const string ForceRenderEnvVar = "PUREDOTS_FORCE_RENDER";
    private const string LegacyRenderingEnvVar = "PUREDOTS_RENDERING";

    public struct Flags
    {
        public byte HeadlessRequested;
        public byte ForceRender;
        public byte RenderingEnabled;
        public byte BatchMode;
    }

    private struct RuntimeModeKey
    {
    }

    private static readonly SharedStatic<Flags> s_flags =
        SharedStatic<Flags>.GetOrCreate<RuntimeModeKey>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void InitializeRuntimeFlags()
    {
        RefreshFromEnvironment();
        LogState("Init");
    }

    public static bool IsBatchMode => s_flags.Data.BatchMode != 0;

    public static bool HeadlessRequested
    {
        get
        {
            return s_flags.Data.HeadlessRequested != 0;
        }
    }

    public static bool IsHeadless => HeadlessRequested;

    public static bool IsRenderingEnabled
    {
        get
        {
            return s_flags.Data.RenderingEnabled != 0;
        }
    }

    // Managed-only initializer (call from a NON-burst system)
    public static void RefreshFromEnvironment()
    {
        bool isBatchMode = Application.isBatchMode;
        bool headless = isBatchMode || EnvIsSet(HeadlessEnvVar);
        bool noGraphics = EnvIsSet(NoGraphicsEnvVar);
        bool forceRender = EnvIsSet(ForceRenderEnvVar) || EnvIsSet(LegacyRenderingEnvVar);
        bool rendering = (!isBatchMode && !noGraphics) || forceRender;

        s_flags.Data = new Flags
        {
            HeadlessRequested = (byte)(headless ? 1 : 0),
            ForceRender = (byte)(forceRender ? 1 : 0),
            RenderingEnabled = (byte)(rendering ? 1 : 0),
            BatchMode = (byte)(isBatchMode ? 1 : 0)
        };
    }

    private static bool EnvIsSet(string key)
    {
        var value = global::System.Environment.GetEnvironmentVariable(key);
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static void LogState(string reason = null)
    {
        int isBatchMode = IsBatchMode ? 1 : 0;
        int headlessRequested = HeadlessRequested ? 1 : 0;
        int renderingEnabled = IsRenderingEnabled ? 1 : 0;
        string suffix = string.IsNullOrWhiteSpace(reason) ? string.Empty : $" ({reason})";
        Debug.Log($"[RuntimeMode] IsBatchMode={isBatchMode} HeadlessRequested={headlessRequested} RenderingEnabled={renderingEnabled}{suffix}");
    }
}
