#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Godgame.Rendering;
using Godgame.Scenario;
using UnityEngine;

static class EnsureSrpEarly
{
    // Runs before any scene/world bootstraps
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Ensure()
    {
        if (!GodgameLegacyScenarioGate.IsEnabled)
        {
            return;
        }

        GodgameRenderPipelineBootstrap.TryEnsureRenderPipeline(legacyOnly: true);
    }
}
#endif
